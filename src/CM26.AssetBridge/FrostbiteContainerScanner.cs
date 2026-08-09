using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CM26.AssetBridge;

/// <summary>
/// CM26-owned, read-only Frostbite container discovery. It does not depend on
/// FMT/FET/Frosty and never opens a game file with write access.
/// </summary>
public sealed class FrostbiteContainerScanner
{
    private const int IndexFormatVersion = 8;
    private static readonly HashSet<string> ContainerExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".cas", ".cat", ".sb" };

    public FrostbiteInventory Scan(string gameRoot)
    {
        var root = ValidateRoot(gameRoot);
        var dataRoot = Path.Combine(root, "Data");
        var patchRoot = Path.Combine(root, "Patch");
        var files = new List<FrostbiteFile>();

        foreach (var searchRoot in new[] { dataRoot, patchRoot })
            foreach (var path in Directory.EnumerateFiles(searchRoot, "*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(path);
                if (!ContainerExtensions.Contains(extension) &&
                    !extension.Equals(".toc", StringComparison.OrdinalIgnoreCase))
                    continue;

                var info = new FileInfo(path);
                files.Add(new FrostbiteFile(
                    Path.GetRelativePath(root, path).Replace('\\', '/'),
                    info.Length,
                    info.LastWriteTimeUtc.Ticks));
            }

        files.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Path, b.Path));
        var fingerprint = CalculateFingerprint(files);
        var layoutPath = Path.Combine(patchRoot, "layout.toc");
        var layoutMagic = ReadLayoutMagic(layoutPath);
        var layout = FrostbiteLayoutReader.Read(layoutPath);
        if (TryReadCache(root, fingerprint, out var cached)) return cached;
        var (tocIndexes, tocErrors, casBundles, directChunks) = BuildTocIndex(root, files);
        var assetIndex = BuildAssetIndex(root, layout.Catalogs, casBundles, directChunks, fingerprint);
        var inventory = new FrostbiteInventory(
            IndexFormatVersion,
            root, layoutMagic, layout.Base, layout.Head,
            layout.SuperBundleCount, layout.CatalogCount, fingerprint,
            files.Count(f => ContainerExtensions.Contains(Path.GetExtension(f.Path))),
            files.Count(f => Path.GetExtension(f.Path).Equals(".toc", StringComparison.OrdinalIgnoreCase)),
            tocIndexes.Count,
            tocIndexes.Sum(toc => toc.BundleCount),
            tocIndexes.Sum(toc => toc.ChunkCount),
            assetIndex.EbxCount,
            assetIndex.ResCount,
            assetIndex.ChunkCount,
            assetIndex.UniqueCount,
            assetIndex.Errors,
            assetIndex.Samples,
            tocErrors,
            tocIndexes,
            files);
        WriteCache(inventory);
        return inventory;
    }

    private static (
        IReadOnlyList<FrostbiteTocIndex> Indexes,
        IReadOnlyList<string> Errors,
        IReadOnlyList<FrostbiteCasBundle> CasBundles,
        IReadOnlyList<FrostbiteDirectChunk> DirectChunks)
        BuildTocIndex(string root, IReadOnlyList<FrostbiteFile> files)
    {
        // Patch TOCs are incremental in FC26. Both Data and Patch must be read;
        // the asset index later de-duplicates equal names with Patch precedence.
        var candidates = files
            .Where(file => Path.GetExtension(file.Path).Equals(".toc", StringComparison.OrdinalIgnoreCase))
            .Where(file => !Path.GetFileName(file.Path).Equals("layout.toc", StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file.Path.StartsWith("Patch/", StringComparison.OrdinalIgnoreCase))
            .ThenBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var indexes = new List<FrostbiteTocIndex>(candidates.Length);
        var errors = new List<string>();
        var casBundles = new List<FrostbiteCasBundle>();
        var directChunks = new List<FrostbiteDirectChunk>();
        foreach (var file in candidates)
        {
            try
            {
                var absolutePath = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
                var result = FrostbiteTocReader.Read(absolutePath, file.Path);
                indexes.Add(result.Index);
                casBundles.AddRange(result.CasBundles);
                directChunks.AddRange(result.DirectChunks);
            }
            catch (Exception ex) when (
                ex is IOException or UnauthorizedAccessException or InvalidDataException or OverflowException)
            {
                errors.Add($"{file.Path}: {ex.Message}");
            }
        }
        return (indexes, errors, casBundles, directChunks);
    }

    private static AssetIndexSummary BuildAssetIndex(
        string root,
        IReadOnlyDictionary<uint, string> catalogs,
        IReadOnlyList<FrostbiteCasBundle> bundles,
        IReadOnlyList<FrostbiteDirectChunk> directChunks,
        string fingerprint)
    {
        var ebxCount = 0;
        var resCount = 0;
        var chunkCount = 0;
        var errors = new List<string>();
        var samples = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var indexedAssets = new List<FrostbiteIndexedAsset>(1_200_000);

        foreach (var bundle in bundles)
        {
            try
            {
                if (!catalogs.TryGetValue(bundle.Metadata.Catalog, out var catalogName))
                    throw new InvalidDataException(
                        $"Unknown catalog 0x{bundle.Metadata.Catalog:X8}.");
                var source = bundle.Metadata.Patch ? "Patch" : "Data";
                var sourceRoot = Path.GetFullPath(Path.Combine(root, source, "Win32"))
                    .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var casPath = Path.GetFullPath(Path.Combine(
                    root, source, "Win32",
                    catalogName.Replace('/', Path.DirectorySeparatorChar),
                    $"cas_{bundle.Metadata.Cas:D2}.cas"));
                if (!casPath.StartsWith(sourceRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Catalog path escapes the FC26 installation.");
                if (!File.Exists(casPath))
                    throw new FileNotFoundException(
                        $"Referenced CAS file was not found: {casPath}", casPath);

                var index = FrostbiteBundleIndexReader.Read(
                    casPath, bundle.Metadata.Offset, bundle.Metadata.Size);
                if (index.Assets.Count != bundle.Assets.Count)
                    throw new InvalidDataException(
                        $"Manifest contains {index.Assets.Count} assets but TOC maps {bundle.Assets.Count} locations.");
                ebxCount = checked(ebxCount + index.EbxCount);
                resCount = checked(resCount + index.ResCount);
                chunkCount = checked(chunkCount + index.ChunkCount);
                if (samples.Count < 64)
                {
                    foreach (var name in index.EbxNames.Take(4))
                        samples.Add($"EBX:{name}");
                    foreach (var name in index.ResNames.Take(4))
                        samples.Add($"RES:{name}");
                }
                for (var i = 0; i < index.Assets.Count; i++)
                {
                    var asset = index.Assets[i];
                    var location = bundle.Assets[i];
                    indexedAssets.Add(new FrostbiteIndexedAsset(
                        asset.Kind, asset.Name, asset.Sha1, asset.OriginalSize,
                        asset.ResType, asset.ResMeta, asset.ResRid, asset.ChunkId,
                        asset.LogicalOffset, asset.LogicalSize,
                        bundle.SuperBundle,
                        location.Patch, location.Catalog, location.Cas,
                        location.Offset, location.Size));
                }
            }
            catch (Exception ex) when (
                ex is IOException or UnauthorizedAccessException or InvalidDataException or OverflowException)
            {
                if (errors.Count < 100) errors.Add($"{bundle.Name}: {ex.Message}");
            }
        }

        // Direct TOC chunks are intentionally indexed separately from bundle
        // manifests. FC26 uses them for the ChunkFileCollector backing the
        // legacy UI assets (crests, portraits and other menu artwork).
        foreach (var chunk in directChunks)
        {
            try
            {
                if (!catalogs.ContainsKey(chunk.Location.Catalog))
                    throw new InvalidDataException(
                        $"Unknown catalog 0x{chunk.Location.Catalog:X8}.");
                indexedAssets.Add(new FrostbiteIndexedAsset(
                    FrostbiteAssetKind.Chunk, chunk.Id.ToString("D"), string.Empty,
                    0, 0, string.Empty, 0, chunk.Id, 0, 0,
                    chunk.SuperBundle,
                    chunk.Location.Patch, chunk.Location.Catalog, chunk.Location.Cas,
                    chunk.Location.Offset, chunk.Location.Size));
                chunkCount = checked(chunkCount + 1);
            }
            catch (Exception ex) when (ex is InvalidDataException or OverflowException)
            {
                if (errors.Count < 100) errors.Add($"Direct chunk {chunk.Id}: {ex.Message}");
            }
        }

        var uniqueCount = FrostbiteAssetIndexStore.Write(fingerprint, indexedAssets);
        return new AssetIndexSummary(
            ebxCount, resCount, chunkCount, uniqueCount,
            errors, samples.Take(64).ToArray());
    }

    private sealed record AssetIndexSummary(
        int EbxCount,
        int ResCount,
        int ChunkCount,
        int UniqueCount,
        IReadOnlyList<string> Errors,
        IReadOnlyList<string> Samples);

    private static string ValidateRoot(string gameRoot)
    {
        if (string.IsNullOrWhiteSpace(gameRoot))
            throw new ArgumentException("FC26 game root is required.", nameof(gameRoot));
        var root = Path.GetFullPath(gameRoot.Trim());
        var required = new[]
        {
            Path.Combine(root, "Data"),
            Path.Combine(root, "Patch"),
            Path.Combine(root, "Patch", "layout.toc"),
            Path.Combine(root, "Patch", "initfs_Win32"),
        };
        if (!Directory.Exists(required[0]) || !Directory.Exists(required[1]) ||
            !File.Exists(required[2]) || !File.Exists(required[3]))
            throw new InvalidDataException("Folder is not a complete FC26 Frostbite installation.");
        return root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string CalculateFingerprint(IEnumerable<FrostbiteFile> files)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var file in files)
        {
            var line = Encoding.UTF8.GetBytes(
                $"{file.Path}\0{file.Length}\0{file.LastWriteUtcTicks}\n");
            hash.AppendData(line);
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static string ReadLayoutMagic(string layoutPath)
    {
        Span<byte> header = stackalloc byte[4];
        using var stream = new FileStream(layoutPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var read = stream.Read(header);
        return read == header.Length ? Convert.ToHexString(header) : string.Empty;
    }

    private static void WriteCache(FrostbiteInventory inventory)
    {
        var root = GetCacheRoot();
        Directory.CreateDirectory(root);
        var destination = Path.Combine(root, "fc26-container-index.json");
        var temporary = destination + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(inventory, BridgeJson.Options));
        File.Move(temporary, destination, overwrite: true);
    }

    private static bool TryReadCache(
        string gameRoot, string fingerprint, out FrostbiteInventory inventory)
    {
        inventory = null!;
        try
        {
            var path = Path.Combine(GetCacheRoot(), "fc26-container-index.json");
            if (!File.Exists(path)) return false;
            var cached = JsonSerializer.Deserialize<FrostbiteInventory>(
                File.ReadAllText(path), BridgeJson.Options);
            if (cached == null ||
                cached.IndexFormatVersion != IndexFormatVersion ||
                !cached.GameRoot.Equals(gameRoot, StringComparison.OrdinalIgnoreCase) ||
                !cached.Fingerprint.Equals(fingerprint, StringComparison.Ordinal) ||
                !FrostbiteAssetIndexStore.MatchesFingerprint(fingerprint))
                return false;
            inventory = cached.UniqueAssetCount > 0
                ? cached
                : cached with { UniqueAssetCount = FrostbiteAssetIndexStore.GetCount() };
            return true;
        }
        catch (Exception ex) when (
            ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    private static string GetCacheRoot() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "asset-cache");
}
