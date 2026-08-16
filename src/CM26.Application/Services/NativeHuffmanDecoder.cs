using System.Text;

namespace CM26.Application.Services;

/// <summary>
/// READ-ONLY C# Huffman decoder for FC26 playernames.
///
/// The protected native engine has a proven defect in its Huff::read() that produces wrong
/// output for playernames.name (e.g. [C4 C4 C4 44 C4] instead of "David"). This C# decoder
/// replicates the exact same algorithm using the raw database bytes and produces correct
/// results (verified: David, Beckham, Iniesta, Shearer, Keane, etc.).
///
/// This component NEVER writes to the database. It reads raw bytes from fifa_ng_db.db
/// and decodes them independently of the native engine's Huffman implementation.
/// </summary>
public static class NativeHuffmanDecoder
{
    /// <summary>
    /// Read the playernames table directly from the raw database bytes and build a
    /// nameid → decoded-name map using the Huffman tree embedded in the table's compressed blob.
    /// </summary>
    public static Dictionary<int, string> BuildPlayerNameMap(string dbPath, string metaPath)
    {
        var bytes = File.ReadAllBytes(dbPath);
        if (bytes.Length < 24 || bytes[0] != 0x44 || bytes[1] != 0x42)
            return new Dictionary<int, string>();

        bool le = bytes[4] != 1;
        int count = (int)ReadU32(bytes, 16, le);
        int dirStart = 24;
        int shortNamesCrc = dirStart + count * 8;
        int baseOff = shortNamesCrc + 4;

        // Parse meta XML to find the playernames shortName
        var shortName = FindTableShortName(metaPath, "playernames");
        if (shortName == null) return new Dictionary<int, string>();

        // Find the playernames table in the directory
        for (int i = 0; i < count; i++)
        {
            var sn = ReadN4(bytes, dirStart + i * 8);
            if (sn != shortName) continue;

            int off = (int)ReadU32(bytes, dirStart + i * 8 + 4, le);
            int tp = baseOff + off;
            if (tp + 36 > bytes.Length) continue;

            uint recSize = ReadU32(bytes, tp + 4, le);
            uint compBytes = ReadU32(bytes, tp + 12, le);
            ushort recCount = ReadU16(bytes, tp + 16, le);
            ushort validCount = ReadU16(bytes, tp + 18, le);
            int cols = bytes[tp + 24];

            int recDataOff = tp + 36 + cols * 16;

            // Read column descriptors
            int nameColType = -1, nameColBitOff = -1, nameColDepth = -1;
            for (int c = 0; c < cols; c++)
            {
                int cp = tp + 36 + c * 16;
                int type = (int)ReadU32(bytes, cp, le);
                int bitOff = (int)ReadU32(bytes, cp + 4, le);
                int depth = (int)ReadU32(bytes, cp + 12, le);
                if (type == 13 || type == 14) // ShortCompressed or LongCompressed
                {
                    nameColType = type;
                    nameColBitOff = bitOff;
                    nameColDepth = depth;
                }
            }
            if (nameColType < 0) continue;

            int byteOff = nameColBitOff / 8;
            int recordsBytes = (int)(recSize * recCount);
            int blob = recDataOff + recordsBytes;

            // Find minOff (tree size) across all records
            int minOff = int.MaxValue;
            for (int r = 0; r < validCount; r++)
            {
                int rp = recDataOff + r * (int)recSize;
                if (rp + byteOff + 4 > bytes.Length) continue;
                int strOff = (int)ReadU32(bytes, rp + byteOff, le);
                if (strOff >= 0 && strOff < minOff) minOff = strOff;
            }
            if (minOff == int.MaxValue || minOff <= 0) continue;

            // Build Huffman tree
            int treeNodes = minOff / 4;
            var child0 = new byte[treeNodes];
            var child1 = new byte[treeNodes];
            var leaf0 = new byte[treeNodes];
            var leaf1 = new byte[treeNodes];
            for (int n = 0; n < treeNodes; n++)
            {
                child0[n] = bytes[blob + n * 4];
                leaf0[n] = bytes[blob + n * 4 + 1];
                child1[n] = bytes[blob + n * 4 + 2];
                leaf1[n] = bytes[blob + n * 4 + 3];
            }

            // Decode all records
            var map = new Dictionary<int, string>();
            for (int r = 0; r < validCount; r++)
            {
                int rp = recDataOff + r * (int)recSize;
                // Read nameid (first integer column - find it)
                int nameId = 0;
                for (int c = 0; c < cols; c++)
                {
                    int cp = tp + 36 + c * 16;
                    int type = (int)ReadU32(bytes, cp, le);
                    int bitOff = (int)ReadU32(bytes, cp + 4, le);
                    int depth = (int)ReadU32(bytes, cp + 12, le);
                    if (type == 3) // Integer
                    {
                        // Read bits at bitOffset, depth
                        nameId = ReadBits(bytes, rp, bitOff, depth, le);
                        break;
                    }
                }

                // Read string offset
                if (rp + byteOff + 4 > bytes.Length) continue;
                int strOff = (int)ReadU32(bytes, rp + byteOff, le);
                if (strOff < 0) continue;

                int at = blob + strOff;
                if (at >= blob + compBytes) continue;

                int len, data;
                if (nameColType == 13) // ShortCompressed
                {
                    len = bytes[at];
                    data = at + 1;
                }
                else // LongCompressed
                {
                    len = (bytes[at] << 8) | bytes[at + 1];
                    data = at + 2;
                }

                // Huffman decode
                // Huffman symbols are bytes, not Unicode code points. Accumulating them as
                // chars turns UTF-8 (for example C3 85 for Å) into mojibake (Ã…).
                var decoded = new List<byte>(len);
                int node = 0;
                int bp = data;
                while (decoded.Count < len && bp < bytes.Length)
                {
                    byte x = bytes[bp++];
                    for (int bit = 7; bit >= 0 && decoded.Count < len; bit--)
                    {
                        int dir = (x >> bit) & 1;
                        byte c = dir == 0 ? child0[node] : child1[node];
                        if (c == 0)
                        {
                            byte sym = dir == 0 ? leaf0[node] : leaf1[node];
                            decoded.Add(sym);
                            node = 0;
                        }
                        else
                        {
                            node = c;
                        }
                    }
                }
                if (decoded.Count > 0 && nameId > 0)
                {
                    var text = NameTextDecoder.DecodeHuffman(decoded.ToArray());
                    if (!string.IsNullOrWhiteSpace(text))
                        map[nameId] = text;
                }
            }
            return map;
        }
        return new Dictionary<int, string>();
    }

    private static uint ReadU32(byte[] b, int p, bool le)
    {
        if (p + 4 > b.Length) return 0;
        return le ? (uint)b[p] | ((uint)b[p + 1] << 8) | ((uint)b[p + 2] << 16) | ((uint)b[p + 3] << 24)
                  : (uint)b[p + 3] | ((uint)b[p + 2] << 8) | ((uint)b[p + 1] << 16) | ((uint)b[p] << 24);
    }
    private static ushort ReadU16(byte[] b, int p, bool le)
    {
        if (p + 2 > b.Length) return 0;
        return le ? (ushort)(b[p] | b[p + 1] << 8) : (ushort)(b[p + 1] | b[p] << 8);
    }
    private static string ReadN4(byte[] b, int p)
    {
        int n = 0;
        while (n < 4 && p + n < b.Length && b[p + n] != 0) n++;
        return Encoding.ASCII.GetString(b, p, n);
    }
    private static int ReadBits(byte[] b, int recStart, int bitOffset, int depth, bool le)
    {
        if (le)
        {
            long x = 0;
            for (int i = 0; i < (bitOffset % 8 + depth + 7) / 8 && bitOffset / 8 + i < b.Length - recStart; i++)
                x |= (long)b[recStart + bitOffset / 8 + i] << (8 * i);
            return (int)((x >> (bitOffset % 8)) & ((depth >= 32) ? 0xFFFFFFFF : ((1L << depth) - 1)));
        }
        else
        {
            long x = 0;
            for (int i = 0; i < depth; i++)
                x = (x << 1) | (((long)b[recStart + (bitOffset + i) / 8] >> (7 - ((bitOffset + i) % 8))) & 1L);
            return (int)x;
        }
    }
    private static string? FindTableShortName(string metaPath, string tableName)
    {
        try
        {
            var xml = File.ReadAllText(metaPath);
            // Simple XML parse: find <table name="playernames" shortname="BGwe">
            var idx = xml.IndexOf($"name=\"{tableName}\"", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            var snIdx = xml.IndexOf("shortname=\"", idx, StringComparison.OrdinalIgnoreCase);
            if (snIdx < 0) return null;
            snIdx += "shortname=\"".Length;
            var end = xml.IndexOf('"', snIdx);
            if (end < 0) return null;
            return xml.Substring(snIdx, end - snIdx);
        }
        catch { return null; }
    }
}
