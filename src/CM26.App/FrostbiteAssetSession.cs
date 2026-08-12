using Microsoft.Win32;
using System.Diagnostics;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CM26.App;

/// <summary>
/// FC26 Frostbite discovery/index session. Read operations remain isolated in
/// the bridge; Save can explicitly run the validated direct Data/Patch
/// transaction after CmModData has been verified.
/// </summary>
public sealed class FrostbiteAssetSession
{
    private static readonly object BridgeGate = new();
    private static Process? _bridgeProcess;
    private static StreamWriter? _bridgeInput;
    private static StreamReader? _bridgeOutput;
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
    public string Backend { get; private set; } = "built-in fallback";
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
        if (TryOpenWithBridge(root)) return;

        ContainerFileCount = CountFiles(data, patch, "*.cas", "*.cat", "*.sb");
        TocFileCount = CountFiles(data, patch, "*.toc");
        IsAvailable = true;
        Status = $"Assets ready · {ContainerFileCount} containers · {TocFileCount} TOCs";
    }

    private bool TryOpenWithBridge(string root)
    {
        var startInfo = CreateBridgeStartInfo(redirectInput: false);
        if (startInfo == null) return false;
        try
        {
            using var process = new Process
            {
                StartInfo = startInfo
            };
            process.StartInfo.ArgumentList.Add("--scan");
            process.StartInfo.ArgumentList.Add(root);
            if (!process.Start()) return false;
            // Drain both redirected pipes asynchronously so a large first-run
            // asset index cannot deadlock before the timeout is evaluated.
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(120_000))
            {
                try { process.Kill(entireProcessTree: true); }
                catch (Exception ex) { Program.Log("Asset bridge timeout cleanup failed: " + ex.Message); }
                Status = "CM26 Asset Bridge timed out";
                return false;
            }
            var output = outputTask.GetAwaiter().GetResult();
            var error = errorTask.GetAwaiter().GetResult();
            if (process.ExitCode != 0)
            {
                Status = string.IsNullOrWhiteSpace(error) ? "CM26 Asset Bridge failed" : error.Trim();
                return false;
            }

            var response = JsonSerializer.Deserialize<BridgeScanResponse>(output,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (response?.Ok != true || response.Inventory == null) return false;
            ContainerFileCount = response.Inventory.ContainerCount;
            TocFileCount = response.Inventory.TocCount;
            LayoutMagic = response.Inventory.LayoutMagic ?? string.Empty;
            BaseVersion = response.Inventory.BaseVersion;
            HeadVersion = response.Inventory.HeadVersion;
            SuperBundleCount = response.Inventory.SuperBundleCount;
            CatalogCount = response.Inventory.CatalogCount;
            ParsedTocCount = response.Inventory.ParsedTocCount;
            IndexedBundleCount = response.Inventory.IndexedBundleCount;
            IndexedChunkCount = response.Inventory.IndexedChunkCount;
            TocErrorCount = response.Inventory.TocErrors?.Count ?? 0;
            EbxAssetCount = response.Inventory.EbxAssetCount;
            ResAssetCount = response.Inventory.ResAssetCount;
            BundleChunkAssetCount = response.Inventory.BundleChunkAssetCount;
            UniqueAssetCount = response.Inventory.UniqueAssetCount;
            AssetIndexErrorCount = response.Inventory.AssetIndexErrors?.Count ?? 0;
            Fingerprint = response.Inventory.Fingerprint ?? string.Empty;
            Backend = "CM26.AssetBridge";
            IsAvailable = true;
            Status = response.Message;
            return true;
        }
        catch
        {
            // The in-process discovery below remains a safe fallback.
            return false;
        }
    }

    private sealed record BridgeScanResponse(bool Ok, string Message, BridgeInventory? Inventory);
    private sealed record BridgeInventory(
        string? LayoutMagic, int BaseVersion, int HeadVersion,
        int SuperBundleCount, int CatalogCount, string? Fingerprint,
        int ContainerCount, int TocCount, int ParsedTocCount,
        int IndexedBundleCount, int IndexedChunkCount,
        int EbxAssetCount, int ResAssetCount, int BundleChunkAssetCount,
        int UniqueAssetCount,
        IReadOnlyList<string>? AssetIndexErrors,
        IReadOnlyList<string>? TocErrors);

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
        catch { yield break; }
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
        catch { return false; }
    }

    public IReadOnlyList<AssetMatch> SearchAssets(
        string query, string? assetType = null, int maximum = 100)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return [];
        var key = $"{assetType}|{maximum}|{query}";
        return _searchCache.GetOrAdd(key, _ => new Lazy<IReadOnlyList<AssetMatch>>(() =>
        {
        var arguments = new List<string> { "--search", GameRoot, query, assetType ?? string.Empty, Math.Clamp(maximum, 1, 500).ToString() };
        var response = RunBridge(arguments, timeoutMilliseconds: 60_000);
        return response?.Ok == true ? response.Assets ?? [] : [];
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    public string? ExportTexture(string resourceName)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        return _textureCache.GetOrAdd(resourceName, name => new Lazy<string?>(() =>
        {
        var response = RunBridge(
            ["--texture", GameRoot, name], timeoutMilliseconds: 60_000);
        return response?.Ok == true && File.Exists(response.OutputPath)
            ? response.OutputPath : null;
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    /// <summary>Parses an FC26 MeshSet RES and exports it as self-contained ASCII FBX.</summary>
    public string? ExportMesh(string meshName, string? textureToken = null)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        var args = string.IsNullOrWhiteSpace(textureToken)
            ? new[] { "--mesh", GameRoot, meshName }
            : new[] { "--mesh", GameRoot, meshName, textureToken };
        var response = RunBridge(args, timeoutMilliseconds: 120_000);
        return response?.Ok == true && File.Exists(response.OutputPath)
            ? response.OutputPath : null;
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
        return _legacyCache.GetOrAdd(legacyPath, path => new Lazy<string?>(() =>
        {
            var response = RunBridge(["--legacy", GameRoot, path], timeoutMilliseconds: 60_000);
            return response?.Ok == true && File.Exists(response.OutputPath) ? response.OutputPath : null;
        }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    public string? ExtractAsset(string name, string assetType)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        var response = RunBridge(
            ["--extract", GameRoot, assetType, name], timeoutMilliseconds: 60_000);
        return response?.Ok == true && File.Exists(response.OutputPath)
            ? response.OutputPath : null;
    }

    public AudioBankInfo? InspectNewWaveBank(string name)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot)) return null;
        var response = RunBridge(
            ["--audio-bank", GameRoot, name], timeoutMilliseconds: 60_000);
        var bank = response?.AudioBank;
        if (response?.Ok != true || bank == null || string.IsNullOrWhiteSpace(response.OutputPath))
            return null;
        return new AudioBankInfo(
            bank.Name, bank.Endian, bank.Alignment, bank.Version,
            bank.BankKey, bank.ProjectKey,
            bank.DataSets.Select(item => new AudioDataSetInfo(
                item.Id, item.Name, item.SampleGroupId, item.RowCount,
                item.FieldCount, item.IndexCount)).ToArray(),
            response.OutputPath!);
    }

    public (bool Success, string Message) ApplyDirect(string planPath)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot))
            return (false, "Game assets are not ready.");
        var response = RunBridge(
            ["--apply-direct", GameRoot, planPath],
            timeoutMilliseconds: 10 * 60_000);
        if (response?.Ok != true)
            return (false, response?.Message ?? "The asset bridge failed to apply the direct transaction.");
        _searchCache.Clear();
        _textureCache.Clear();
        _legacyCache.Clear();
        Open(GameRoot);
        return (true, response.Message);
    }

    /// <summary>
    /// Applies the normal Frostbite transaction to a CM26-owned overlay root.
    /// Unlike <see cref="ApplyDirect"/>, the overlay intentionally has no
    /// FC26.exe and therefore must not be opened as a live game session.
    /// </summary>
    public (bool Success, string Message) ApplyOverlay(string overlayRoot, string planPath)
    {
        var response = RunBridge(["--apply-direct", overlayRoot, planPath], timeoutMilliseconds: 10 * 60_000);
        return response?.Ok == true
            ? (true, response.Message)
            : (false, response?.Message ?? "The asset bridge failed to build the CM26 mod overlay.");
    }

    public (bool Success, string Message) ExportFetMod(string planPath, string destination)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(GameRoot))
            return (false, "Game assets are not ready.");
        var response = RunBridge(["--export-fet", GameRoot, planPath, destination], timeoutMilliseconds: 10 * 60_000);
        return response?.Ok == true
            ? (true, response.Message)
            : (false, response?.Message ?? "The asset bridge failed to export the FET mod.");
    }

    private static BridgeOperationResponse? RunBridge(
        IReadOnlyList<string> arguments, int timeoutMilliseconds)
    {
        var startInfo = CreateBridgeStartInfo(redirectInput: true);
        if (startInfo == null) return null;
        lock (BridgeGate)
        {
            try
            {
                if (_bridgeProcess == null || _bridgeProcess.HasExited || _bridgeInput == null || _bridgeOutput == null)
                {
                    _bridgeProcess?.Dispose();
                    _bridgeProcess = new Process
                    {
                        StartInfo = startInfo
                    };
                    if (!_bridgeProcess.Start()) return null;
                    _bridgeInput = _bridgeProcess.StandardInput;
                    _bridgeInput.AutoFlush = true;
                    _bridgeOutput = _bridgeProcess.StandardOutput;
                }

                object request = arguments[0] switch
                {
                    "--search" => new { command = "searchAssets", gameRoot = arguments[1], query = arguments[2], assetType = arguments[3], maxResults = int.Parse(arguments[4]) },
                    "--texture" => new { command = "exportTexture", gameRoot = arguments[1], query = arguments[2], assetType = "Res" },
                    "--mesh" => new { command = "exportMesh", gameRoot = arguments[1], query = arguments[2], assetType = "Res" },
                    "--legacy" => new { command = "exportLegacy", gameRoot = arguments[1], query = arguments[2] },
                    "--extract" => new { command = "extractAsset", gameRoot = arguments[1], assetType = arguments[2], query = arguments[3] },
                    "--audio-bank" => new { command = "inspectAudio", gameRoot = arguments[1], query = arguments[2], assetType = "Res" },
                    "--apply-direct" => new { command = "applyDirect", gameRoot = arguments[1], query = arguments[2] },
                    "--export-fet" => new { command = "exportFet", gameRoot = arguments[1], query = arguments[2], outputPath = arguments[3] },
                    _ => throw new ArgumentException("Unsupported bridge operation.")
                };
                _bridgeInput.WriteLine(JsonSerializer.Serialize(request));
                var outputTask = _bridgeOutput.ReadLineAsync();
                if (!outputTask.Wait(timeoutMilliseconds))
                    throw new TimeoutException("CM26 Asset Bridge timed out.");
                var output = outputTask.GetAwaiter().GetResult();
                return string.IsNullOrWhiteSpace(output) ? null : JsonSerializer.Deserialize<BridgeOperationResponse>(
                    output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                try { if (_bridgeProcess is { HasExited: false }) _bridgeProcess.Kill(entireProcessTree: true); }
                catch (Exception ex) { Program.Log("Asset bridge shutdown failed: " + ex.Message); }
                _bridgeProcess?.Dispose();
                _bridgeProcess = null; _bridgeInput = null; _bridgeOutput = null;
                return null;
            }
        }
    }

    private static ProcessStartInfo? CreateBridgeStartInfo(bool redirectInput)
    {
        var executable = Path.Combine(AppContext.BaseDirectory, "CM26.AssetBridge.exe");
        var assembly = Path.Combine(AppContext.BaseDirectory, "CM26.AssetBridge.dll");
        ProcessStartInfo info;
        if (File.Exists(executable))
        {
            info = new ProcessStartInfo { FileName = executable };
        }
        else if (File.Exists(assembly))
        {
            info = new ProcessStartInfo { FileName = "dotnet" };
            info.ArgumentList.Add(assembly);
        }
        else return null;

        info.UseShellExecute = false;
        info.RedirectStandardInput = redirectInput;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.CreateNoWindow = true;
        return info;
    }

    private sealed record BridgeOperationResponse(
        bool Ok, string Message, IReadOnlyList<AssetMatch>? Assets, string? OutputPath,
        AudioBankResponse? AudioBank);
    private sealed record AudioBankResponse(
        string Name, string Endian, int Alignment, byte Version,
        uint BankKey, uint ProjectKey, IReadOnlyList<AudioDataSetResponse> DataSets);
    private sealed record AudioDataSetResponse(
        uint Id, string Name, uint SampleGroupId, int RowCount,
        ushort FieldCount, ushort IndexCount);

    private static int CountFiles(string data, string patch, params string[] patterns)
    {
        var count = 0;
        foreach (var root in new[] { data, patch })
            foreach (var pattern in patterns)
            {
                try { count += Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories).Count(); }
                catch { /* A locked optional container must not block database loading. */ }
            }
        return count;
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
        Backend = "built-in fallback";
        Status = "Game assets not loaded";
    }
}
