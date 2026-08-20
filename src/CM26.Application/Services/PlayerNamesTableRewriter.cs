using System.Text;

namespace CM26.Application.Services;

/// <summary>
/// Rebuilds the Huffman-compressed playernames blob inside fifa_ng_db.db.
///
/// The native engine only supports in-place compressed-string writes, where a new
/// name must fit the byte slot of the name it replaces. That makes almost every
/// realistic rename impossible (slots are packed tight). This rewriter applies the
/// CM16 method instead: it decodes every name, applies the staged edits, rebuilds
/// the Huffman tree and the whole compressed blob, re-points the record offsets,
/// shifts every later table and recomputes the affected CRCs, producing a fully
/// valid T3DB that the game and the editor both read back normally.
///
/// Every rewrite first re-verifies the complete original CRC chain; if the on-disk
/// layout does not match the expected format the rewrite is refused instead of
/// writing a corrupt file.
/// </summary>
public static class PlayerNamesTableRewriter
{
    private const int HeaderCrcOffset = 20;
    private const int DirectoryStart = 24;
    private const int TableHeaderSize = 36;

    /// <summary>Parsed playernames state used for staging and reading names.</summary>
    public sealed class TableData
    {
        public string TableShortName = string.Empty;
        public Dictionary<int, string> NameTexts = new();   // nameid -> decoded text
        public Dictionary<int, int> RowNameIds = new();     // row index -> nameid
        public string? Error;
    }

    /// <summary>Reads the playernames table (nameid -> text, row -> nameid) from a raw database file.</summary>
    public static TableData Read(string dbPath, string metaPath)
    {
        var data = new TableData();
        try
        {
            var shortName = FindTableShortName(metaPath, "playernames");
            if (shortName == null) { data.Error = "playernames is not described by the database metadata."; return data; }
            data.TableShortName = shortName;
            var bytes = File.ReadAllBytes(dbPath);
            var layout = ParseLayout(bytes, shortName);
            if (layout == null) { data.Error = "playernames was not found in the database."; return data; }
            ReadTableContent(bytes, layout, data);
            return data;
        }
        catch (Exception ex)
        {
            data.Error = "playernames could not be decoded: " + ex.Message;
            return data;
        }
    }

    /// <summary>
    /// Rewrites <paramref name="file"/> so each playernames row whose nameid appears in
    /// <paramref name="nameIdTexts"/> carries the new text. Returns the new file bytes.
    /// </summary>
    public static byte[] Rewrite(byte[] file, string tableShortName, IReadOnlyDictionary<int, string> nameIdTexts)
    {
        if (nameIdTexts.Count == 0) return file;
        var layout = ParseLayout(file, tableShortName)
            ?? throw new InvalidOperationException("The playernames table was not found in the saved database.");
        VerifyCrcChain(file, layout.Layout!);

        var le = file[4] != 1;
        var t = layout;

        // Column descriptors: name text column (compressed) + first integer column (nameid).
        int nameColType = -1, nameColByteOff = -1, nameIdBitOff = -1, nameIdDepth = -1;
        for (var c = 0; c < t.Columns; c++)
        {
            var cp = t.Offset + TableHeaderSize + c * 16;
            var type = (int)ReadU32(file, cp, le);
            var bitOff = (int)ReadU32(file, cp + 4, le);
            var depth = (int)ReadU32(file, cp + 12, le);
            if (type is 13 or 14 && nameColType < 0) { nameColType = type; nameColByteOff = bitOff / 8; }
            if (type == 3 && nameIdBitOff < 0) { nameIdBitOff = bitOff; nameIdDepth = depth; }
        }
        if (nameColType < 0 || nameIdBitOff < 0)
            throw new InvalidOperationException("The playernames table does not expose a compressed name column.");

        // Decode every existing payload with the current tree.
        var tree = ReadTree(file, t, le, out _);
        var recordsBytes = (int)(t.RecordSize * t.RecordCount);
        var payloads = new byte[t.RecordCount][];
        var rowNameIds = new int[t.ValidCount];
        var rowHasString = new bool[t.RecordCount];
        for (var r = 0; r < t.RecordCount; r++)
        {
            var rp = t.RecordDataOffset + r * (int)t.RecordSize;
            if (r < t.ValidCount) rowNameIds[r] = ReadBits(file, rp, nameIdBitOff, nameIdDepth, le);
            var off = (int)ReadU32(file, rp + nameColByteOff, le);
            if (off < 0) continue;
            var at = t.BlobStart + off;
            var len = nameColType == 13 ? file[at] : (file[at] << 8) | file[at + 1];
            var data = at + (nameColType == 13 ? 1 : 2);
            payloads[r] = DecodePayload(tree, file, data, len);
            rowHasString[r] = true;
        }

        // Apply the staged edits at the byte level (UTF-8 symbols, like the engine encoder).
        var applied = new HashSet<int>();
        for (var r = 0; r < t.ValidCount; r++)
        {
            if (!nameIdTexts.TryGetValue(rowNameIds[r], out var text)) continue;
            var symbols = Encoding.UTF8.GetBytes(text);
            var maxLength = nameColType == 13 ? 255 : 65535;
            if (symbols.Length > maxLength)
                throw new InvalidOperationException(
                    $"The new name for nameid {rowNameIds[r]} is too long for the playernames column ({symbols.Length} bytes; limit {maxLength}).");
            payloads[r] = symbols;
            rowHasString[r] = true;
            applied.Add(rowNameIds[r]);
        }
        var missing = nameIdTexts.Keys.Where(id => !applied.Contains(id)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException(
                "playernames does not contain nameid " + string.Join(", ", missing.Take(4)) +
                (missing.Length > 4 ? $" (+{missing.Length - 4} more)" : string.Empty) + ".");

        // Rebuild the Huffman tree over the final symbol frequencies.
        var (nodes, codes) = BuildTree(payloads);
        var newTreeBytes = new byte[nodes.Count * 4];
        for (var n = 0; n < nodes.Count; n++)
        {
            newTreeBytes[n * 4] = nodes[n].Child0Internal;
            newTreeBytes[n * 4 + 1] = nodes[n].Leaf0;
            newTreeBytes[n * 4 + 2] = nodes[n].Child1Internal;
            newTreeBytes[n * 4 + 3] = nodes[n].Leaf1;
        }

        // Encode once, then lay the strings out behind the tree, sharing one copy of identical payloads.
        var headerSize = nameColType == 13 ? 1 : 2;
        var encoded = new byte[t.RecordCount][];
        for (var r = 0; r < t.RecordCount; r++)
            if (rowHasString[r]) encoded[r] = EncodePayload(codes, payloads[r]);
        var position = newTreeBytes.Length;
        var offsetByPayload = new Dictionary<string, int>();
        var placementOrder = new List<int>(); // row index of the first occurrence, in placement order
        var rowOffset = new int[t.RecordCount];
        for (var r = 0; r < t.RecordCount; r++)
        {
            if (!rowHasString[r]) { rowOffset[r] = -1; continue; }
            var key = Convert.ToHexString(payloads[r]);
            if (offsetByPayload.TryGetValue(key, out var existing)) { rowOffset[r] = existing; continue; }
            rowOffset[r] = position;
            offsetByPayload[key] = position;
            placementOrder.Add(r);
            position += headerSize + encoded[r].Length;
        }
        var newCompressedBytes = position;

        // Assemble the new table: patched header + patched records + new blob + original padding/indexes.
        var newTable = new MemoryStream();
        var headerLength = TableHeaderSize + t.Columns * 16;
        var headerBytes = new byte[headerLength];
        Array.Copy(file, t.Offset, headerBytes, 0, headerLength);
        WriteU32(headerBytes, 12, (uint)newCompressedBytes, le);
        newTable.Write(headerBytes);

        var recordBytes = new byte[recordsBytes];
        Array.Copy(file, t.RecordDataOffset, recordBytes, 0, recordsBytes);
        for (var r = 0; r < t.RecordCount; r++)
        {
            if (!rowHasString[r]) continue; // preserve the empty-slot marker
            var rp = r * (int)t.RecordSize;
            WriteU32(recordBytes, rp + nameColByteOff, (uint)rowOffset[r], le);
        }
        newTable.Write(recordBytes);
        newTable.Write(newTreeBytes);
        foreach (var r in placementOrder)
        {
            var payload = payloads[r];
            var length = nameColType == 13 ? 1 : 2;
            var header = new byte[length];
            if (nameColType == 13) header[0] = (byte)payload.Length;
            else { header[0] = (byte)(payload.Length >> 8); header[1] = (byte)payload.Length; }
            newTable.Write(header);
            newTable.Write(encoded[r]);
        }
        // Pad the blob area to the same relative 8-byte alignment the reader expects.
        var blobAreaStartInStream = headerLength + recordsBytes;
        while ((newTable.Length - blobAreaStartInStream) % 8 != 0) newTable.WriteByte(0);
        var indexBytes = new byte[t.RecordsCrcOffset - t.IndexStart];
        Array.Copy(file, t.IndexStart, indexBytes, 0, indexBytes.Length);
        newTable.Write(indexBytes);
        newTable.Write(new byte[4]); // records CRC placeholder
        var newTableBytes = newTable.ToArray();

        // Rebuild the file: prefix (directory patched) + new table + verbatim suffix.
        var oldTableLength = t.RecordsCrcOffset + 4 - t.Offset;
        var delta = newTableBytes.Length - oldTableLength;
        using var output = new MemoryStream();
        var prefix = new byte[t.Offset];
        Array.Copy(file, 0, prefix, 0, t.Offset);
        var baseOff = t.Layout!.ShortNamesCrcOffset + 4;
        for (var i = 0; i < t.Layout.TableOffsets.Length; i++)
        {
            var abs = baseOff + t.Layout.TableOffsets[i];
            if (abs > t.Offset)
                WriteU32(prefix, DirectoryStart + i * 8 + 4,
                    (uint)(t.Layout.TableOffsets[i] + delta), le);
        }
        output.Write(prefix);
        output.Write(newTableBytes);
        var suffixStart = t.RecordsCrcOffset + 4;
        var suffix = new byte[file.Length - suffixStart];
        Array.Copy(file, suffixStart, suffix, 0, suffix.Length);
        output.Write(suffix);
        var result = output.ToArray();

        // Header: new file size, then the affected CRCs.
        WriteU32(result, 8, (uint)result.Length, le);
        WriteU32(result, HeaderCrcOffset, (uint)CrcDb11(result, 0, HeaderCrcOffset), le);
        WriteU32(result, t.Layout.ShortNamesCrcOffset,
            (uint)CrcDb11(result, DirectoryStart, t.Layout.ShortNamesCrcOffset), le);
        var chainStart = t.Layout.ChainStartOf(t.ChainIndex);
        WriteU32(result, t.Offset + 32,
            (uint)CrcDb11(result, chainStart, t.Offset + 32), le);
        var newRecordsCrc = t.Offset + newTableBytes.Length - 4;
        WriteU32(result, newRecordsCrc,
            (uint)CrcDb11(result, t.Offset + 36, newRecordsCrc), le);
        return result;
    }

    // --- layout ---------------------------------------------------------------

    private sealed class LayoutInfo
    {
        public int Count;
        public int ShortNamesCrcOffset;
        public int[] TableOffsets = Array.Empty<int>();
        public string[] TableShortNames = Array.Empty<string>();
        public int[] RecordsCrcOffsets = Array.Empty<int>(); // per table, CRC-chain order
        public int[] ChainStarts = Array.Empty<int>();       // coverage start per table
        public int ChainStartOf(int chainIndex) => chainIndex == 0 ? ShortNamesCrcOffset + 4 : RecordsCrcOffsets[chainIndex - 1] + 4;
    }

    private sealed class TableLayout
    {
        public LayoutInfo? Layout;
        public int ChainIndex;
        public int Offset;
        public uint RecordSize;
        public uint CompressedBytes;
        public int RecordCount;
        public int ValidCount;
        public int Columns;
        public int RecordDataOffset;
        public int BlobStart;
        public int IndexStart;
        public int RecordsCrcOffset;
    }

    private static TableLayout? ParseLayout(byte[] file, string tableShortName)
    {
        if (file.Length < 24 || file[0] != 0x44 || file[1] != 0x42)
            throw new InvalidOperationException("The file is not a T3DB database.");
        var le = file[4] != 1;
        var count = (int)ReadU32(file, 16, le);
        if (count <= 0 || count > 10000) throw new InvalidOperationException("Invalid table count.");
        var shortNamesCrcOffset = DirectoryStart + count * 8;
        if (shortNamesCrcOffset + 4 > file.Length) throw new InvalidOperationException("Invalid directory.");
        var baseOff = shortNamesCrcOffset + 4;
        // The physical file can carry trailing bytes beyond the logical size stored
        // in the header; every table boundary check uses the logical size like the
        // native reader does.
        var logicalEnd = Math.Min((int)ReadU32(file, 8, le), file.Length);

        var layout = new LayoutInfo
        {
            Count = count,
            ShortNamesCrcOffset = shortNamesCrcOffset,
            TableOffsets = new int[count],
            TableShortNames = new string[count],
        };
        for (var i = 0; i < count; i++)
        {
            layout.TableShortNames[i] = ReadN4(file, DirectoryStart + i * 8);
            layout.TableOffsets[i] = (int)ReadU32(file, DirectoryStart + i * 8 + 4, le);
        }

        TableLayout? target = null;
        var recordsCrc = new int[count];
        var chainStarts = new int[count];
        var chainPos = shortNamesCrcOffset + 4;
        for (var i = 0; i < count; i++)
        {
            var tp = baseOff + layout.TableOffsets[i];
            var limit = i + 1 < count ? baseOff + layout.TableOffsets[i + 1] : logicalEnd;
            if (tp + TableHeaderSize > limit) throw new InvalidOperationException("Invalid table header.");
            var recordSize = ReadU32(file, tp + 4, le);
            var compressedBytes = ReadU32(file, tp + 12, le);
            var recordCount = ReadU16(file, tp + 16, le);
            var validCount = ReadU16(file, tp + 18, le);
            var cols = file[tp + 24];
            var indexes = file[tp + 25];
            var recordDataOffset = tp + TableHeaderSize + cols * 16;
            var blobStart = recordDataOffset + (int)(recordSize * recordCount);
            // The index area is 8-byte aligned RELATIVE TO THE BLOB START (engine layout rule).
            var indexStart = blobStart + (((int)compressedBytes + 7) & ~7);
            if (indexStart > limit) throw new InvalidOperationException("Invalid compressed blob.");
            var ix = indexStart;
            for (var z = 0; z < indexes; z++)
            {
                if (ix + 8 > limit) throw new InvalidOperationException("Invalid table index.");
                var cc = file[ix + 4];
                ix += 8 + cc * 8;
            }
            if (ix + 4 > limit) throw new InvalidOperationException(
                $"Invalid record CRC position for table {layout.TableShortNames[i]}.");
            recordsCrc[i] = ix;
            chainStarts[i] = chainPos;
            chainPos = ix + 4;

            if (target == null && layout.TableShortNames[i].Equals(tableShortName, StringComparison.OrdinalIgnoreCase))
            {
                target = new TableLayout
                {
                    Layout = layout,
                    ChainIndex = i,
                    Offset = tp,
                    RecordSize = recordSize,
                    CompressedBytes = compressedBytes,
                    RecordCount = recordCount,
                    ValidCount = validCount,
                    Columns = cols,
                    RecordDataOffset = recordDataOffset,
                    BlobStart = blobStart,
                    IndexStart = indexStart,
                    RecordsCrcOffset = ix,
                };
            }
        }
        layout.RecordsCrcOffsets = recordsCrc;
        layout.ChainStarts = chainStarts;
        return target;
    }

    private static void VerifyCrcChain(byte[] file, LayoutInfo layout)
    {
        var le = file[4] != 1;
        if (ReadU32(file, HeaderCrcOffset, le) != (uint)CrcDb11(file, 0, HeaderCrcOffset))
            throw new InvalidOperationException("The database header CRC does not match; refusing to rewrite.");
        if (ReadU32(file, layout.ShortNamesCrcOffset, le) != (uint)CrcDb11(file, DirectoryStart, layout.ShortNamesCrcOffset))
            throw new InvalidOperationException("The database directory CRC does not match; refusing to rewrite.");
        for (var i = 0; i < layout.Count; i++)
        {
            var tp = layout.ShortNamesCrcOffset + 4 + layout.TableOffsets[i];
            var tableCrcAt = tp + 32;
            var chainStart = layout.ChainStartOf(i);
            if (ReadU32(file, tableCrcAt, le) != (uint)CrcDb11(file, chainStart, tableCrcAt))
                throw new InvalidOperationException(
                    $"Table {layout.TableShortNames[i]} header CRC does not match; refusing to rewrite.");
            if (ReadU32(file, layout.RecordsCrcOffsets[i], le) !=
                (uint)CrcDb11(file, tableCrcAt + 4, layout.RecordsCrcOffsets[i]))
                throw new InvalidOperationException(
                    $"Table {layout.TableShortNames[i]} records CRC does not match; refusing to rewrite.");
        }
    }

    private static void ReadTableContent(byte[] file, TableLayout t, TableData data)
    {
        var le = file[4] != 1;
        int nameColType = -1, nameColByteOff = -1, nameIdBitOff = -1, nameIdDepth = -1;
        for (var c = 0; c < t.Columns; c++)
        {
            var cp = t.Offset + TableHeaderSize + c * 16;
            var type = (int)ReadU32(file, cp, le);
            var bitOff = (int)ReadU32(file, cp + 4, le);
            var depth = (int)ReadU32(file, cp + 12, le);
            if (type is 13 or 14 && nameColType < 0) { nameColType = type; nameColByteOff = bitOff / 8; }
            if (type == 3 && nameIdBitOff < 0) { nameIdBitOff = bitOff; nameIdDepth = depth; }
        }
        if (nameColType < 0 || nameIdBitOff < 0)
        {
            data.Error = "playernames does not expose its name column in this database.";
            return;
        }
        var tree = ReadTree(file, t, le, out _);
        for (var r = 0; r < t.ValidCount; r++)
        {
            var rp = t.RecordDataOffset + r * (int)t.RecordSize;
            var nameId = ReadBits(file, rp, nameIdBitOff, nameIdDepth, le);
            data.RowNameIds[r] = nameId;
            var off = (int)ReadU32(file, rp + nameColByteOff, le);
            if (off < 0) continue;
            var at = t.BlobStart + off;
            var len = nameColType == 13 ? file[at] : (file[at] << 8) | file[at + 1];
            var payloadOffset = at + (nameColType == 13 ? 1 : 2);
            var payload = DecodePayload(tree, file, payloadOffset, len);
            var text = NameTextDecoder.DecodeHuffman(payload);
            if (!string.IsNullOrWhiteSpace(text))
                data.NameTexts[nameId] = text;
        }
    }

    // --- Huffman ---------------------------------------------------------------

    private sealed class Node
    {
        public byte Child0Internal; public byte Leaf0;
        public byte Child1Internal; public byte Leaf1;
    }

    private static (List<Node> Nodes, byte[][] Codes) BuildTree(byte[][] payloads)
    {
        var freq = new long[256];
        foreach (var payload in payloads)
        {
            if (payload == null) continue;
            foreach (var b in payload)
                freq[b]++;
        }

        var symbols = Enumerable.Range(0, 256).Where(s => freq[s] > 0).OrderBy(s => s).ToArray();
        var nodes = new List<Node>();
        if (symbols.Length == 0)
        {
            nodes.Add(new Node { Child0Internal = 0, Leaf0 = 0x20, Child1Internal = 0, Leaf1 = 0x20 });
            return (nodes, BuildCodes(nodes));
        }
        if (symbols.Length == 1)
        {
            nodes.Add(new Node { Child0Internal = 0, Leaf0 = (byte)symbols[0], Child1Internal = 0, Leaf1 = (byte)symbols[0] });
            return (nodes, BuildCodes(nodes));
        }

        // Standard Huffman merge. Pending items are either a symbol leaf or an
        // already-appended internal node; the last item left is the root.
        // During the build, child references use index+1 (0 marks a leaf slot);
        // RenumberTree converts them to the file's direct-index convention.
        var queue = new SortedSet<(long Freq, int Id, bool Leaf, int Ref)>();
        var id = 0;
        foreach (var s in symbols)
            queue.Add((freq[s], id++, true, s));
        while (queue.Count > 1)
        {
            var a = queue.Min; queue.Remove(a);
            var b = queue.Min; queue.Remove(b);
            var index = nodes.Count;
            nodes.Add(new Node
            {
                Child0Internal = a.Leaf ? (byte)0 : (byte)(a.Ref + 1),
                Leaf0 = a.Leaf ? (byte)a.Ref : (byte)0,
                Child1Internal = b.Leaf ? (byte)0 : (byte)(b.Ref + 1),
                Leaf1 = b.Leaf ? (byte)b.Ref : (byte)0,
            });
            queue.Add((a.Freq + b.Freq, id++, false, index));
        }
        var root = queue.Min.Ref;
        if (nodes.Count > 255)
            throw new InvalidOperationException("The player-name alphabet is too large for the Huffman tree format.");
        var renumbered = RenumberTree(nodes, root);
        return (renumbered, BuildCodes(renumbered));
    }

    private static List<Node> RenumberTree(List<Node> nodes, int root)
    {
        // BFS from the root assigning fresh sequential indices (root = 0), and
        // convert child references from the build convention (index+1) to the
        // file convention (direct index; the root can never be a child).
        var mapping = new int[nodes.Count];
        Array.Fill(mapping, -1);
        var order = new List<int>();
        var queue = new Queue<int>();
        queue.Enqueue(root);
        mapping[root] = 0;
        order.Add(root);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var node = nodes[current];
            foreach (var child in new[] { node.Child0Internal, node.Child1Internal })
            {
                if (child == 0) continue;
                var childIndex = child - 1;
                if (mapping[childIndex] >= 0) continue;
                mapping[childIndex] = order.Count;
                order.Add(childIndex);
                queue.Enqueue(childIndex);
            }
        }
        if (order.Count != nodes.Count)
            throw new InvalidOperationException("The rebuilt Huffman tree is inconsistent.");
        var result = new List<Node>(nodes.Count);
        foreach (var original in order)
        {
            var node = nodes[original];
            result.Add(new Node
            {
                Child0Internal = node.Child0Internal == 0 ? (byte)0 : (byte)mapping[node.Child0Internal - 1],
                Leaf0 = node.Leaf0,
                Child1Internal = node.Child1Internal == 0 ? (byte)0 : (byte)mapping[node.Child1Internal - 1],
                Leaf1 = node.Leaf1,
            });
        }
        return result;
    }

    private static byte[][] BuildCodes(List<Node> nodes)
    {
        var codes = new byte[256][];
        if (nodes.Count == 0) return codes;
        var stack = new Stack<(int Node, byte[] Path)>();
        stack.Push((0, Array.Empty<byte>()));
        var visits = 0;
        while (stack.Count > 0)
        {
            if (++visits > nodes.Count * 4)
                throw new InvalidOperationException(
                    $"Huffman code walk exceeded the node budget (visited {visits}, nodes {nodes.Count}); the rebuilt tree contains a cycle.");
            var (nodeIndex, path) = stack.Pop();
            var node = nodes[nodeIndex];
            for (var bit = 0; bit < 2; bit++)
            {
                var child = bit == 0 ? node.Child0Internal : node.Child1Internal;
                var next = new byte[path.Length + 1];
                Array.Copy(path, next, path.Length);
                next[path.Length] = (byte)bit;
                if (child == 0)
                {
                    var symbol = bit == 0 ? node.Leaf0 : node.Leaf1;
                    codes[symbol] = next;
                }
                else
                {
                    stack.Push((child, next));
                }
            }
        }
        return codes;
    }

    private static byte[] EncodePayload(byte[][] codes, byte[] payload)
    {
        if (codes.All(c => c == null) || codes.Length == 0) return payload;
        var bits = new List<byte>(payload.Length);
        foreach (var symbol in payload)
        {
            var code = codes[symbol];
            if (code == null || code.Length == 0)
                throw new InvalidOperationException(
                    $"The name contains a character (0x{symbol:X2}) that cannot be stored in the player-name alphabet.");
            bits.AddRange(code);
        }
        var output = new byte[(bits.Count + 7) / 8];
        for (var i = 0; i < bits.Count; i++)
            if (bits[i] != 0)
                output[i / 8] |= (byte)(0x80 >> (i % 8));
        return output;
    }

    private sealed class Tree
    {
        public byte[] Child0 = Array.Empty<byte>();
        public byte[] Leaf0 = Array.Empty<byte>();
        public byte[] Child1 = Array.Empty<byte>();
        public byte[] Leaf1 = Array.Empty<byte>();
    }

    private static Tree ReadTree(byte[] file, TableLayout t, bool le, out byte[] treeBytes)
    {
        var minOff = int.MaxValue;
        for (var r = 0; r < t.ValidCount; r++)
        {
            var rp = t.RecordDataOffset + r * (int)t.RecordSize;
            for (var c = 0; c < t.Columns; c++)
            {
                var cp = t.Offset + TableHeaderSize + c * 16;
                var type = (int)ReadU32(file, cp, le);
                if (type is not (13 or 14)) continue;
                var off = (int)ReadU32(file, rp + (int)ReadU32(file, cp + 4, le) / 8, le);
                if (off >= 0 && off < minOff) minOff = off;
            }
        }
        if (minOff == int.MaxValue) minOff = 0;
        if (minOff % 4 != 0) throw new InvalidOperationException("Invalid Huffman tree size.");
        var nodes = minOff / 4;
        treeBytes = new byte[nodes * 4];
        Array.Copy(file, t.BlobStart, treeBytes, 0, treeBytes.Length);
        var tree = new Tree
        {
            Child0 = new byte[nodes], Leaf0 = new byte[nodes],
            Child1 = new byte[nodes], Leaf1 = new byte[nodes],
        };
        for (var n = 0; n < nodes; n++)
        {
            tree.Child0[n] = treeBytes[n * 4];
            tree.Leaf0[n] = treeBytes[n * 4 + 1];
            tree.Child1[n] = treeBytes[n * 4 + 2];
            tree.Leaf1[n] = treeBytes[n * 4 + 3];
        }
        return tree;
    }

    private static byte[] DecodePayload(Tree tree, byte[] file, int data, int length)
    {
        var decoded = new byte[length];
        var node = 0;
        var produced = 0;
        var p = data;
        while (produced < length)
        {
            if (p >= file.Length) throw new InvalidOperationException("Compressed string is truncated.");
            var x = file[p++];
            for (var bit = 7; bit >= 0 && produced < length; bit--)
            {
                var dir = (x >> bit) & 1;
                var child = dir == 0 ? tree.Child0[node] : tree.Child1[node];
                if (child == 0)
                {
                    decoded[produced++] = dir == 0 ? tree.Leaf0[node] : tree.Leaf1[node];
                    node = 0;
                }
                else
                {
                    node = child;
                }
            }
        }
        return decoded;
    }

    // --- primitives --------------------------------------------------------------

    private static uint ReadU32(byte[] b, int p, bool le) =>
        le ? (uint)b[p] | ((uint)b[p + 1] << 8) | ((uint)b[p + 2] << 16) | ((uint)b[p + 3] << 24)
           : (uint)b[p + 3] | ((uint)b[p + 2] << 8) | ((uint)b[p + 1] << 16) | (uint)b[p] << 24;

    private static void WriteU32(byte[] b, int p, uint v, bool le)
    {
        if (le)
        {
            b[p] = (byte)v; b[p + 1] = (byte)(v >> 8); b[p + 2] = (byte)(v >> 16); b[p + 3] = (byte)(v >> 24);
        }
        else
        {
            b[p + 3] = (byte)v; b[p + 2] = (byte)(v >> 8); b[p + 1] = (byte)(v >> 16); b[p] = (byte)(v >> 24);
        }
    }

    private static ushort ReadU16(byte[] b, int p, bool le) =>
        le ? (ushort)(b[p] | b[p + 1] << 8) : (ushort)(b[p + 1] | b[p] << 8);

    private static string ReadN4(byte[] b, int p)
    {
        var n = 0;
        while (n < 4 && p + n < b.Length && b[p + n] != 0) n++;
        return Encoding.ASCII.GetString(b, p, n);
    }

    private static int ReadBits(byte[] b, int recStart, int bitOffset, int depth, bool le)
    {
        if (le)
        {
            long x = 0;
            for (var i = 0; i < (bitOffset % 8 + depth + 7) / 8 && bitOffset / 8 + i < b.Length - recStart; i++)
                x |= (long)b[recStart + bitOffset / 8 + i] << (8 * i);
            return (int)((x >> (bitOffset % 8)) & (depth >= 32 ? 0xFFFFFFFF : ((1L << depth) - 1)));
        }
        long y = 0;
        for (var i = 0; i < depth; i++)
            y = (y << 1) | (((long)b[recStart + (bitOffset + i) / 8] >> (7 - ((bitOffset + i) % 8))) & 1L);
        return (int)y;
    }

    /// <summary>CRC-32 with polynomial 0x04C11DB7, init -1, MSB-first, no final XOR (EA "db11").</summary>
    private static int CrcDb11(byte[] b, int from, int to)
    {
        var crc = -1;
        for (var i = from; i < to; i++)
        {
            crc ^= b[i] << 24;
            for (var bit = 0; bit < 8; bit++)
            {
                if (crc >= 0) crc *= 2;
                else { crc *= 2; crc ^= 0x04C11DB7; }
            }
        }
        return crc;
    }

    internal static string? FindTableShortName(string metaPath, string tableName)
    {
        try
        {
            var xml = File.ReadAllText(metaPath);
            var idx = xml.IndexOf($"name=\"{tableName}\"", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            var snIdx = xml.IndexOf("shortname=\"", idx, StringComparison.OrdinalIgnoreCase);
            if (snIdx < 0) return null;
            snIdx += "shortname=\"".Length;
            var end = xml.IndexOf('"', snIdx);
            return end < 0 ? null : xml.Substring(snIdx, end - snIdx);
        }
        catch { return null; }
    }
}
