using System.Text;

namespace CM26.AssetBridge;

internal sealed record FrostbiteIndexedAsset(
    FrostbiteAssetKind Kind,
    string Name,
    string Sha1,
    uint OriginalSize,
    uint ResType,
    string ResMeta,
    ulong ResRid,
    Guid ChunkId,
    uint LogicalOffset,
    uint LogicalSize,
    bool Patch,
    uint Catalog,
    byte Cas,
    uint Offset,
    uint Size);

internal static class FrostbiteAssetIndexStore
{
    private static readonly byte[] Magic = "CM26AIDX"u8.ToArray();
    private const int Version = 1;

    public static string IndexPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "asset-cache", "fc26-assets.bin");

    public static int Write(string fingerprint, List<FrostbiteIndexedAsset> assets)
    {
        assets.Sort(CompareAssets);
        DeduplicateWithPatchPrecedence(assets);
        var destination = IndexPath;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        var temporary = destination + ".tmp";
        using (var stream = new FileStream(
                   temporary, FileMode.Create, FileAccess.ReadWrite, FileShare.None,
                   bufferSize: 1024 * 1024, FileOptions.SequentialScan))
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Magic);
            writer.Write(Version);
            writer.Write(fingerprint);
            writer.Write(assets.Count);
            var tablePosition = stream.Position;
            stream.Write(new byte[checked(assets.Count * sizeof(long))]);
            var offsets = new long[assets.Count];
            for (var i = 0; i < assets.Count; i++)
            {
                offsets[i] = stream.Position;
                WriteAsset(writer, assets[i]);
            }
            var end = stream.Position;
            stream.Position = tablePosition;
            foreach (var offset in offsets) writer.Write(offset);
            stream.Position = end;
            writer.Flush();
            stream.Flush(flushToDisk: true);
        }
        File.Move(temporary, destination, overwrite: true);
        return assets.Count;
    }

    public static bool MatchesFingerprint(string fingerprint)
    {
        try
        {
            using var reader = OpenReader(out _, out var storedFingerprint, out _);
            return storedFingerprint.Equals(fingerprint, StringComparison.Ordinal);
        }
        catch (Exception ex) when (
            ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return false;
        }
    }

    public static int GetCount()
    {
        using var reader = OpenReader(out var count, out _, out _);
        return count;
    }

    public static IReadOnlyList<FrostbiteIndexedAsset> Search(
        string query, FrostbiteAssetKind? kind, int maximum)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        maximum = Math.Clamp(maximum, 1, 500);
        using var reader = OpenReader(out var count, out _, out var tablePosition);
        var results = new List<FrostbiteIndexedAsset>(maximum);
        reader.BaseStream.Position = checked(tablePosition + (long)count * sizeof(long));
        for (var i = 0; i < count && results.Count < maximum; i++)
        {
            var asset = ReadAsset(reader);
            if (kind != null && asset.Kind != kind) continue;
            if (asset.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                results.Add(asset);
        }
        return results;
    }

    public static FrostbiteIndexedAsset? FindExact(string name, FrostbiteAssetKind? kind)
    {
        using var reader = OpenReader(out var count, out _, out var tablePosition);
        var low = 0;
        var high = count - 1;
        while (low <= high)
        {
            var middle = low + ((high - low) / 2);
            var asset = ReadAssetAt(reader, tablePosition, middle);
            var comparison = StringComparer.OrdinalIgnoreCase.Compare(asset.Name, name);
            if (comparison < 0) low = middle + 1;
            else if (comparison > 0) high = middle - 1;
            else
            {
                // Equal names can have more than one asset kind. Walk the small
                // equal-name range to honour the requested type.
                var first = middle;
                while (first > 0)
                {
                    var previous = ReadAssetAt(reader, tablePosition, first - 1);
                    if (!previous.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) break;
                    first--;
                }
                for (var i = first; i < count; i++)
                {
                    var candidate = ReadAssetAt(reader, tablePosition, i);
                    if (!candidate.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) break;
                    if (kind == null || candidate.Kind == kind) return candidate;
                }
                return null;
            }
        }
        return null;
    }

    private static BinaryReader OpenReader(
        out int count, out string fingerprint, out long tablePosition)
    {
        var stream = new FileStream(
            IndexPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 128 * 1024, FileOptions.RandomAccess);
        try
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
            if (!reader.ReadBytes(Magic.Length).AsSpan().SequenceEqual(Magic) ||
                reader.ReadInt32() != Version)
                throw new InvalidDataException("Unsupported CM26 asset index.");
            fingerprint = reader.ReadString();
            count = reader.ReadInt32();
            tablePosition = stream.Position;
            if (count < 0 || count > 5_000_000 ||
                checked(stream.Position + (long)count * sizeof(long)) > stream.Length)
                throw new InvalidDataException("Invalid CM26 asset index count.");
            return reader;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    private static FrostbiteIndexedAsset ReadAssetAt(
        BinaryReader reader, long tablePosition, int index)
    {
        var tableOffset = checked(tablePosition + (long)index * sizeof(long));
        reader.BaseStream.Position = tableOffset;
        var recordOffset = reader.ReadInt64();
        if (recordOffset < 0 || recordOffset >= reader.BaseStream.Length)
            throw new InvalidDataException("Invalid CM26 asset record offset.");
        reader.BaseStream.Position = recordOffset;
        return ReadAsset(reader);
    }

    private static int CompareAssets(FrostbiteIndexedAsset left, FrostbiteIndexedAsset right)
    {
        var byName = StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
        if (byName != 0) return byName;
        var byKind = left.Kind.CompareTo(right.Kind);
        if (byKind != 0) return byKind;
        return left.Patch.CompareTo(right.Patch);
    }

    private static void DeduplicateWithPatchPrecedence(List<FrostbiteIndexedAsset> assets)
    {
        var write = 0;
        for (var read = 0; read < assets.Count;)
        {
            var selected = assets[read];
            var next = read + 1;
            while (next < assets.Count &&
                   assets[next].Kind == selected.Kind &&
                   assets[next].Name.Equals(selected.Name, StringComparison.OrdinalIgnoreCase))
            {
                // A manifest-backed chunk carries its SHA1; a direct TOC
                // location does not. Prefer the manifest location whenever it
                // exists because it is the resource's explicit payload link.
                // Direct chunks remain essential for legacy CFC assets that
                // have no bundle-manifest counterpart.
                var candidate = assets[next];
                if (string.IsNullOrEmpty(selected.Sha1) && !string.IsNullOrEmpty(candidate.Sha1))
                    selected = candidate;
                else if (string.IsNullOrEmpty(selected.Sha1) == string.IsNullOrEmpty(candidate.Sha1))
                    // Sort order places Patch after Data, so the final entry is
                    // the current override when both sources have equal rank.
                    selected = candidate;
                next++;
            }
            assets[write++] = selected;
            read = next;
        }
        if (write < assets.Count) assets.RemoveRange(write, assets.Count - write);
    }

    private static void WriteAsset(BinaryWriter writer, FrostbiteIndexedAsset asset)
    {
        writer.Write((byte)asset.Kind);
        writer.Write(asset.Name);
        writer.Write(asset.Sha1);
        writer.Write(asset.OriginalSize);
        writer.Write(asset.ResType);
        writer.Write(asset.ResMeta);
        writer.Write(asset.ResRid);
        writer.Write(asset.ChunkId.ToByteArray());
        writer.Write(asset.LogicalOffset);
        writer.Write(asset.LogicalSize);
        writer.Write(asset.Patch);
        writer.Write(asset.Catalog);
        writer.Write(asset.Cas);
        writer.Write(asset.Offset);
        writer.Write(asset.Size);
    }

    private static FrostbiteIndexedAsset ReadAsset(BinaryReader reader)
    {
        var kind = (FrostbiteAssetKind)reader.ReadByte();
        var name = reader.ReadString();
        var sha1 = reader.ReadString();
        var originalSize = reader.ReadUInt32();
        var resType = reader.ReadUInt32();
        var resMeta = reader.ReadString();
        var resRid = reader.ReadUInt64();
        var chunkId = new Guid(reader.ReadBytes(16));
        var logicalOffset = reader.ReadUInt32();
        var logicalSize = reader.ReadUInt32();
        var patch = reader.ReadBoolean();
        var catalog = reader.ReadUInt32();
        var cas = reader.ReadByte();
        var offset = reader.ReadUInt32();
        var size = reader.ReadUInt32();
        return new FrostbiteIndexedAsset(
            kind, name, sha1, originalSize, resType, resMeta, resRid,
            chunkId, logicalOffset, logicalSize, patch, catalog, cas, offset, size);
    }
}
