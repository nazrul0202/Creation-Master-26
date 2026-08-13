using System.Buffers.Binary;
using System.Collections.Concurrent;

namespace CM26.AssetBridge;

/// <summary>
/// Resolves FC26 legacy UI files through CORE/ChunkFiles/ChunkFileCollector.
/// FET uses this same game-native route for assets such as
/// data/ui/imgAssets/crest/light/l{teamId}.dds.  No FET/FMT executable or
/// cache is required at runtime.
/// </summary>
internal static class FrostbiteLegacyAssetResolver
{
    private const string CollectorPrefix = "core/chunkfiles";
    private const int ManifestChunkIdOffset = 0x60;
    private const int HeaderSize = 80;
    private const int FileEntrySize = 28;
    private const int MaxEntries = 2_000_000;
    private static readonly Lazy<FrostbiteIndexedAsset[]> CollectorAssets = new(
        () => FrostbiteAssetIndexStore.Search(CollectorPrefix, FrostbiteAssetKind.Ebx, 500).ToArray(),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly ConcurrentDictionary<string, Lazy<LegacyEntry?>> Entries =
        new(StringComparer.OrdinalIgnoreCase);

    public static string Export(string gameRoot, IReadOnlyDictionary<uint, string> catalogs, string legacyPath)
    {
        if (string.IsNullOrWhiteSpace(legacyPath))
            throw new ArgumentException("Legacy asset path is required.", nameof(legacyPath));
        var normalizedPath = legacyPath.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
        var cacheKey = $"{gameRoot.GetHashCode()}|{normalizedPath}";
        if (Entries.Count > MaxEntries)
            Entries.Clear();
        var entry = Entries.GetOrAdd(cacheKey, _ => new Lazy<LegacyEntry?>(
            () => FindEntryForPath(gameRoot, catalogs, normalizedPath), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        if (entry == null)
            throw new FileNotFoundException($"Legacy FC26 asset was not found: {normalizedPath}");
        var sourceChunk = FrostbiteAssetIndexStore.FindExact(
            entry.Value.ChunkId.ToString("D"), FrostbiteAssetKind.Chunk)
            ?? throw new FileNotFoundException($"Legacy asset chunk was not indexed: {entry.Value.ChunkId}");
        var source = FrostbitePayloadReader.ReadDecoded(gameRoot, catalogs, sourceChunk);
        if (entry.Value.Offset > source.Length || entry.Value.Size > source.Length - entry.Value.Offset)
            throw new InvalidDataException("Legacy asset range lies outside its source chunk.");

        var destinationRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "legacy-assets");
        var destination = Path.Combine(destinationRoot,
            normalizedPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        var temporary = destination + ".tmp";
        File.WriteAllBytes(temporary, source.AsSpan(
            checked((int)entry.Value.Offset), checked((int)entry.Value.Size)).ToArray());
        File.Move(temporary, destination, overwrite: true);
        return destination;
    }

    public static LegacyAssetTarget ResolveTarget(
        string gameRoot, IReadOnlyDictionary<uint, string> catalogs, string legacyPath)
    {
        if (string.IsNullOrWhiteSpace(legacyPath))
            throw new ArgumentException("Legacy asset path is required.", nameof(legacyPath));
        var normalizedPath = legacyPath.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
        var targetHash = HashPath(normalizedPath);
        foreach (var collector in CollectorAssets.Value)
        {
            try
            {
                var ebx = FrostbitePayloadReader.ReadDecoded(gameRoot, catalogs, collector);
                if (ebx.Length < ManifestChunkIdOffset + 16 ||
                    !ebx.AsSpan(0, 4).SequenceEqual("RIFF"u8)) continue;
                var manifestChunkId = new Guid(ebx.AsSpan(ManifestChunkIdOffset, 16));
                var manifestChunk = FrostbiteAssetIndexStore.FindExact(
                    manifestChunkId.ToString("D"), FrostbiteAssetKind.Chunk);
                if (manifestChunk == null) continue;
                var entry = FindEntry(
                    FrostbitePayloadReader.ReadDecoded(gameRoot, catalogs, manifestChunk), targetHash);
                if (entry == null) continue;
                var originalChunk = FrostbiteAssetIndexStore.FindExact(
                    entry.Value.ChunkId.ToString("D"), FrostbiteAssetKind.Chunk)
                    ?? throw new InvalidDataException("Legacy payload chunk is not indexed.");
                return new LegacyAssetTarget(
                    normalizedPath,
                    targetHash,
                    entry.Value.ChunkId,
                    collector.Name,
                    collector.SuperBundle,
                    manifestChunkId,
                    collector.Patch,
                    originalChunk.Sha1,
                    originalChunk.OriginalSize,
                    originalChunk.Patch,
                    originalChunk.Catalog,
                    originalChunk.Cas,
                    originalChunk.Offset,
                    originalChunk.Size,
                    entry.Value.CompressedStartOffset,
                    entry.Value.Offset,
                    entry.Value.Size);
            }
            catch (Exception ex) when (
                ex is InvalidDataException or IOException or NotSupportedException)
            {
                // Continue until the collector that owns the path is found.
            }
        }
        throw new FileNotFoundException($"Legacy FC26 asset was not found: {normalizedPath}");
    }

    private static LegacyEntry? FindEntryForPath(
        string gameRoot, IReadOnlyDictionary<uint, string> catalogs, string normalizedPath)
    {
        var targetHash = HashPath(normalizedPath);
        LegacyEntry? entry = null;
        foreach (var collector in CollectorAssets.Value)
        {
            try
            {
                var ebx = FrostbitePayloadReader.ReadDecoded(gameRoot, catalogs, collector);
                if (ebx.Length < ManifestChunkIdOffset + 16 ||
                    !ebx.AsSpan(0, 4).SequenceEqual("RIFF"u8)) continue;
                var manifestChunkId = new Guid(ebx.AsSpan(ManifestChunkIdOffset, 16));
                var manifestChunk = FrostbiteAssetIndexStore.FindExact(
                    manifestChunkId.ToString("D"), FrostbiteAssetKind.Chunk);
                if (manifestChunk == null) continue;
                entry = FindEntry(FrostbitePayloadReader.ReadDecoded(gameRoot, catalogs, manifestChunk), targetHash);
                if (entry != null) break;
            }
            catch (Exception ex) when (ex is InvalidDataException or IOException or NotSupportedException)
            {
                // An EBX below core/chunkfiles can be a non-collector helper.
                // Continue safely until a collector that owns the requested file is found.
            }
        }
        return entry;
    }

    private static LegacyEntry? FindEntry(ReadOnlySpan<byte> manifest, ulong targetHash)
    {
        if (manifest.Length < HeaderSize) throw new InvalidDataException("ChunkFileCollector is truncated.");
        var roots = BinaryPrimitives.ReadUInt32LittleEndian(manifest);
        var fileCount = BinaryPrimitives.ReadUInt32LittleEndian(manifest.Slice(12, 4));
        var fileOffset = BinaryPrimitives.ReadInt64LittleEndian(manifest.Slice(16, 8));
        var cacheCount = BinaryPrimitives.ReadUInt32LittleEndian(manifest.Slice(24, 4));
        var guidsOffset = BinaryPrimitives.ReadInt64LittleEndian(manifest.Slice(48, 8));
        if (fileCount > MaxEntries || fileOffset < HeaderSize || guidsOffset < HeaderSize)
            throw new InvalidDataException("ChunkFileCollector has invalid offsets.");
        EnsureRange(manifest, fileOffset, checked((long)fileCount * FileEntrySize));
        var relocationBytes = checked(((long)roots + cacheCount + 6) * 4);
        var guidBytes = manifest.Length - relocationBytes - guidsOffset;
        if (guidBytes < 0 || guidBytes % 16 != 0)
            throw new InvalidDataException("ChunkFileCollector GUID pool is invalid.");
        var guidCount = checked((int)(guidBytes / 16));
        EnsureRange(manifest, guidsOffset, guidBytes);
        for (var i = 0; i < fileCount; i++)
        {
            var position = checked(fileOffset + (long)i * FileEntrySize);
            var row = manifest.Slice(checked((int)position), FileEntrySize);
            if (BinaryPrimitives.ReadUInt64LittleEndian(row) != targetHash) continue;
            var guidIndex = BinaryPrimitives.ReadInt32LittleEndian(row.Slice(24, 4));
            if (guidIndex < 0 || guidIndex >= guidCount)
                throw new InvalidDataException("ChunkFileCollector refers to an invalid GUID.");
            var guidPosition = checked((int)(guidsOffset + (long)guidIndex * 16));
            return new LegacyEntry(
                BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(8, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(16, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(20, 4)),
                new Guid(manifest.Slice(guidPosition, 16)));
        }
        return null;
    }

    private static ulong HashPath(string value)
    {
        ulong hash = 14695981039346656037UL;
        foreach (var character in value)
            hash = (hash * 1099511628211UL) ^ character;
        return hash;
    }

    private static void EnsureRange(ReadOnlySpan<byte> data, long offset, long length)
    {
        if (offset < 0 || length < 0 || offset > data.Length || length > data.Length - offset)
            throw new InvalidDataException("ChunkFileCollector range lies outside the manifest.");
    }

    private readonly record struct LegacyEntry(uint CompressedStartOffset, uint Offset, uint Size, Guid ChunkId);
}

internal readonly record struct LegacyAssetTarget(
    string Name,
    ulong NameHash,
    Guid OriginalChunkId,
    string CollectorEbxName,
    string CollectorSuperBundle,
    Guid CollectorManifestChunkId,
    bool CollectorInPatch,
    string OriginalSha1,
    uint OriginalSize,
    bool OriginalInPatch,
    uint OriginalCatalog,
    byte OriginalCas,
    uint OriginalOffset,
    uint OriginalCompressedSize,
    uint LegacyCompressedStartOffset,
    uint LegacyOffset,
    uint LegacySize);
