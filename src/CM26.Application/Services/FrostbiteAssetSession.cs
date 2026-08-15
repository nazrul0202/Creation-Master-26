using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using CM26.AssetBridge;

namespace CM26.Application.Services;

/// <summary>
/// FC26 Frostbite discovery/index session. All asset operations run
/// in-process through <see cref="BridgeEngine"/> (single EXE like the CM16
/// FifaLibrary model); no helper process is spawned. Save can explicitly run
/// the validated direct Data/Patch transaction after CmModData is verified.
/// </summary>
public sealed class FrostbiteAssetSession
{
    private readonly ConcurrentDictionary<string, Lazy<IReadOnlyList<AssetMatch>>> _searchCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Lazy<string?>> _textureCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Lazy<string?>> _legacyCache = new(StringComparer.OrdinalIgnoreCase);
    public sealed record AssetMatch(
        string Type, string Name, string Sha1, uint OriginalSize,
        uint CompressedSize, uint ResType, ulong ResRid);
    public sealed record AudioBankInfo(
        string Name, string Endian, int Alignment, byte Version,
        uint BankKey, uint ProjectKey, IReadOnlyList<AudioDataSetInfo> DataSets,
        string ExtractedPath);
    public sealed record AudioDataSetInfo(
        uint Id, string Name, uint SampleGroupId, int RowCount,
        ushort FieldCount, ushort IndexCount);

    /// <summary>FC26 ResourceType ordinal for MeshSet RES assets.</summary>
    public const uint MeshSetResourceType = 1236358868;

    public string GameRoot { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }
    public int ContainerFileCount { get; private set; }
    public int TocFileCount { get; private set; }
    public string LayoutMagic { get; private set; } = string.Empty;
    public int BaseVersion { get; private set; }
    public int HeadVersion { get; private set; }
    public int SuperBundleCount { get; private set; }
    public int CatalogCount { get; private set; }
    public int ParsedTocCount { get; private set; }
    public int IndexedBundleCount { get; private set; }
    public int IndexedChunkCount { get; private set; }
    public int TocErrorCount { get; private set; }
    public int EbxAssetCount { get; private set; }
    public int ResAssetCount { get; private set; }
    public int BundleChunkAssetCount { get; private set; }
    public int UniqueAssetCount { get; private set; }
    public int AssetIndexErrorCount { get; private set; }
    public string Fingerprint { get; private set; } = string.Empty;
    public string Backend { get; private set; } = "in-process asset engine";
    public string Status { get; private set; } = "Game assets not loaded";

    public void Open(string? preferredRoot = null)
    {
        Reset();
        var root = ResolveGameRoot(preferredRoot);
        if (string.IsNullOrWhiteSpace(root))
        {
            Status = "Game folder was not detected";
            return;
        }

        root = Path.GetFullPath(root.Trim());
        var data = Path.Combine(root, "Data");
        var patch = Path.Combine(root, "Patch");
        var layout = Path.Combine(patch, "layout.toc");
        var initfs = Path.Combine(patch, "initfs_Win32");
        if (!Directory.Exists(data) || !Directory.Exists(patch) ||
            !File.Exists(layout) || !File.Exists(initfs))
        {
            GameRoot = root;
            Status = "Selected folder is not a complete Frostbite installation";
            return;
        }

        GameRoot = root;
        try
        {
            var inventory = BridgeEngine.OpenGame(root);
            ContainerFileCount = inventory.ContainerCount;
            TocFileCount = inventory.TocCount;
            LayoutMagic = inventory.LayoutMagic ?? string.Empty;
            BaseVersion = inventory.BaseVersion;
            HeadVersion = inventory.HeadVersion;
            SuperBundleCount = inventory.SuperBundleCount;
            CatalogCount = inventory.CatalogCount;
            ParsedTocCount = inventory.ParsedTocCount;
            IndexedBundleCount = inventory.IndexedBundleCount;
            IndexedChunkCount = inventory.IndexedChunkCount;
            TocErrorCount = inventory.TocErrors?.Count ?? 0;
            EbxAssetCount = inventory.EbxAssetCount;
            ResAssetCount = inventory.ResAssetCount;
            BundleChunkAssetCount = inventory.BundleChunkAssetCount;
            UniqueAssetCount = inventory.UniqueAssetCount;
            AssetIndexErrorCount = inventory.AssetIndexErrors?.Count ?? 0;
            Fingerprint = inventory.Fingerprint ?? string.Empty;
            IsAvailable = true;
            Status = inventory.SuperBundleCount > 0
                ? $"FC26 asset source ready: {inventory.SuperBundleCount} superbundles, " +
                  $"{inventory.IndexedBundleCount} bundles, {inventory.UniqueAssetCount} unique assets"
                : "FC26 asset source indexed";
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"In-process asset index failed: {ex.Message}");
            Status = "Asset index failed: " + ex.Message;
        }
    }

    public static string? ResolveGameRoot(string? preferredRoot = null)
    {
        if (IsGameRoot(preferredRoot)) return Path.GetFullPath(preferredRoot!.Trim());

        const string keyPath = @"SOFTWARE\EA Sports\EA SPORTS FC 26";
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var key = baseKey.OpenSubKey(keyPath);
                var value = key?.GetValue("Install Dir") as string;
                if (IsGameRoot(value)) return Path.GetFullPath(value!.Trim());
            }
            catch
            {
                // Detection is best-effort; the Settings picker remains available.
            }
        }

        // Steam installs are commonly not registered under EA's key. Probe the
        // default library and any additional libraries listed by Steam; this is
        // a bounded lookup, not a drive-wide scan.
        var steamRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
        foreach (var library in GetSteamLibraries(steamRoot))
        {
            foreach (var gameName in new[] { "FC 26", "EA SPORTS FC 26" })
            {
                var candidate = Path.Combine(library, "steamapps", "common", gameName);
                if (IsGameRoot(candidate)) return Path.GetFullPath(candidate);
            }
        }
        return null;
    }

    private static IEnumerable<string> GetSteamLibraries(string steamRoot)
    {
        yield return steamRoot;
        var libraryFile = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
        string content;
        try { content = File.ReadAllText(libraryFile); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] GetSteamLibraries failed: {ex.Message}"); yield break; }
        foreach (Match match in Regex.Matches(content, "\\\"path\\\"\\s+\\\"(?<path>(?:\\\\\\\"|[^\\\"])*)\\\"", RegexOptions.IgnoreCase))
        {
            var library = match.Groups["path"].Value.Replace("\\\\", "\\");
            if (!string.IsNullOrWhiteSpace(library)) yield return library;
        }
    }

    public static bool IsGameRoot(string? root)
    {
        if (string.IsNullOrWhiteSpace(root)) return false;
        try
        {
            return Directory.Exists(Path.Combine(root, "Data")) &&
                   Directory.Exists(Path.Combine(root, "Patch")) &&
                   File.Exists(Path.Combine(root, "Patch", "layout.toc")) &&
                   File.Exists(Path.Combine(root, "Patch", "initfs_Win32"));
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] IsGameRoot failed: {ex.Message}"); return false; }
    }

    public IReadOnlyList<AssetMatch> SearchAssets(
        string query, string? assetType = null, int maximum = 100)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return [];
        var key = $"{assetType}|{maximum}|{query}";
        return _searchCache.GetOrAdd(key, _ => new Lazy<IReadOnlyList<AssetMatch>>(() =>
        {
        try
        {
            return BridgeEngine.SearchAssets(GameRoot, query, assetType, Math.Clamp(maximum, 1, 500))
                .Select(a => new AssetMatch(a.Type, a.Name, a.Sha1, a.OriginalSize,
                    a.CompressedSize, a.ResType, a.ResRid)).ToArray();
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Asset search failed for '{query}': {ex.Message}");
            return [];
        }
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    public string? ExportTexture(string resourceName)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        return _textureCache.GetOrAdd(resourceName, name => new Lazy<string?>(() =>
        {
        try
        {
            var output = BridgeEngine.ExportTexture(GameRoot, name);
            return File.Exists(output) ? output : null;
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Texture export failed for '{name}': {ex.Message}");
            return null;
        }
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    /// <summary>Parses an FC26 MeshSet RES and exports it as self-contained ASCII FBX.</summary>
    public string? ExportMesh(string meshName, string? textureToken = null)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        try
        {
            var output = BridgeEngine.ExportMesh(GameRoot, meshName, textureToken);
            return File.Exists(output) ? output : null;
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Mesh export failed for '{meshName}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Resolves an FC26 MeshSet RES among the supplied query tokens (the first
    /// hit that is a MeshSet and looks like an actual geometry RES) and exports
    /// it to the shared exported-meshes folder for the 3D viewer.
    /// </summary>
    public string? ExportMeshForQuery(IReadOnlyList<string> queries, int maximum = 100)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot) || queries == null) return null;
        AssetMatch? selected = null;
        var textureToken = queries.FirstOrDefault(q => !string.IsNullOrWhiteSpace(q));
        foreach (var query in queries.Where(q => !string.IsNullOrWhiteSpace(q)))
        {
            foreach (var match in SearchAssets(query, "Res", maximum))
            {
                if (match.ResType != MeshSetResourceType) continue;
                if (match.Name.EndsWith("_mesh", StringComparison.OrdinalIgnoreCase) ||
                    match.Name.EndsWith("mesh", StringComparison.OrdinalIgnoreCase))
                {
                    selected = match;
                    textureToken = query;
                    break;
                }
            }
            if (selected != null) break;
        }
        if (selected == null) return null;
        return ExportMesh(selected.Name, textureToken);
    }

    /// <summary>Exports a named legacy UI file through FC26's own collector.</summary>
    public string? ExportLegacyAsset(string legacyPath)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot) || string.IsNullOrWhiteSpace(legacyPath)) return null;
        // The bridge preserves the canonical legacy path below this cache.
        // Reuse an already-extracted preview immediately; extracting for every
        // crest/miniface made valid assets appear blank for several seconds
        // whenever an editor record was selected.
        var cachedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "legacy-assets",
            legacyPath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(cachedPath) && new FileInfo(cachedPath).Length > 0)
            return cachedPath;
        return _legacyCache.GetOrAdd(legacyPath, path => new Lazy<string?>(() =>
        {
            try
            {
                var output = BridgeEngine.ExportLegacy(GameRoot, path);
                return File.Exists(output) ? output : null;
            }
            catch (Exception ex)
            {
                Cm26Log.Write($"Legacy asset export failed for '{path}': {ex.Message}");
                return null;
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    public string? ExtractAsset(string name, string assetType)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        try
        {
            var output = BridgeEngine.ExtractAsset(GameRoot, assetType, name);
            return File.Exists(output) ? output : null;
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Asset extraction failed for '{name}': {ex.Message}");
            return null;
        }
    }

    public AudioBankInfo? InspectNewWaveBank(string name)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        try
        {
            var (bank, path) = BridgeEngine.InspectAudio(GameRoot, name);
            return new AudioBankInfo(
                bank.Name, bank.Endian, bank.Alignment, bank.Version,
                bank.BankKey, bank.ProjectKey,
                bank.DataSets.Select(item => new AudioDataSetInfo(
                    item.Id, item.Name, item.SampleGroupId, item.RowCount,
                    item.FieldCount, item.IndexCount)).ToArray(),
                path);
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Audio bank inspection failed for '{name}': {ex.Message}");
            return null;
        }
    }

    public (bool Success, string Message) ApplyDirect(string planPath)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot))
            return (false, "Game assets are not ready.");
        try
        {
            var message = BridgeEngine.ApplyDirect(GameRoot, planPath);
            _searchCache.Clear();
            _textureCache.Clear();
            _legacyCache.Clear();
            Open(GameRoot);
            return (true, message);
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Direct apply failed: {ex.Message}");
            return (false, "The direct transaction failed: " + ex.Message);
        }
    }

    /// <summary>
    /// Applies the normal Frostbite transaction to a CM26-owned overlay root.
    /// Unlike <see cref="ApplyDirect"/>, the overlay intentionally has no
    /// FC26.exe and therefore must not be opened as a live game session.
    /// </summary>
    public (bool Success, string Message) ApplyOverlay(string overlayRoot, string planPath)
    {
        try
        {
            var message = BridgeEngine.ApplyDirect(overlayRoot, planPath);
            return (true, message);
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"Overlay apply failed: {ex.Message}");
            return (false, "The CM26 mod overlay build failed: " + ex.Message);
        }
    }

    public (bool Success, string Message) ExportFetMod(string planPath, string destination)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot))
            return (false, "Game assets are not ready.");
        try
        {
            var message = BridgeEngine.ExportFet(GameRoot, planPath, destination);
            return (true, message);
        }
        catch (Exception ex)
        {
            Cm26Log.Write($"FET mod export failed: {ex.Message}");
            return (false, "The FET mod export failed: " + ex.Message);
        }
    }

    private void Reset()
    {
        _searchCache.Clear();
        _textureCache.Clear();
        _legacyCache.Clear();
        GameRoot = string.Empty;
        IsAvailable = false;
        ContainerFileCount = 0;
        TocFileCount = 0;
        LayoutMagic = string.Empty;
        BaseVersion = 0;
        HeadVersion = 0;
        SuperBundleCount = 0;
        CatalogCount = 0;
        ParsedTocCount = 0;
        IndexedBundleCount = 0;
        IndexedChunkCount = 0;
        TocErrorCount = 0;
        EbxAssetCount = 0;
        ResAssetCount = 0;
        BundleChunkAssetCount = 0;
        UniqueAssetCount = 0;
        AssetIndexErrorCount = 0;
        Fingerprint = string.Empty;
        Backend = "in-process asset engine";
        Status = "Game assets not loaded";
    }
}