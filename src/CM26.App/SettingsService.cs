namespace CM26.App;

/// <summary>Persists lightweight user settings (last folder, etc.) to a local file.</summary>
public static class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "settings.ini");

    private static readonly Dictionary<string, string> _values = Load();

    public static string LastFolder
    {
        get => _values.TryGetValue("LastFolder", out var v) ? v : string.Empty;
        set { _values["LastFolder"] = value; Save(); }
    }

    /// <summary>Installed FC26 root containing Data/Patch Frostbite containers.</summary>
    public static string FC26GameFolder
    {
        get
        {
            if (_values.TryGetValue("FC26GameFolder", out var v) &&
                FrostbiteAssetSession.IsGameRoot(v))
                return v;
            return FrostbiteAssetSession.ResolveGameRoot() ?? string.Empty;
        }
        set { _values["FC26GameFolder"] = value; Save(); }
    }

    /// <summary>
    /// Folder containing the external CM26 Scraper tool (CM26 Scraper.exe).
    /// Optional: the scraper bundled under Tools\CM26 Scraper and drive-root
    /// copies are auto-detected when this is not set.
    /// </summary>
    public static string ScraperRoot
    {
        get => _values.TryGetValue("ScraperRoot", out var v) ? v : string.Empty;
        set { _values["ScraperRoot"] = value; Save(); }
    }

    /// <summary>Root folder containing FC26 visual-asset packs (minifaces, balls, etc.). User-configurable.</summary>
    public static string AssetRoot
    {
        get
        {
            if (_values.TryGetValue("AssetRoot", out var v) && !string.IsNullOrWhiteSpace(v))
                return v;
            return AutoDetectAssetRoot() ?? string.Empty;
        }
        set { _values["AssetRoot"] = value; Save(); }
    }

    /// <summary>True once the user has accepted the End User License Agreement.</summary>
    public static bool EulaAccepted
    {
        get
        {
            _values.TryGetValue("EulaAccepted", out var v);
            return v == "1";
        }
        set { _values["EulaAccepted"] = value ? "1" : "0"; Save(); }
    }

    /// <summary>Application UI language (BCP-47 tag, e.g. "en", "es", "pt"). Empty = default.</summary>
    public static string Language
    {
        get => _values.TryGetValue("Language", out var v) ? v : string.Empty;
        set { _values["Language"] = value; Save(); }
    }

    /// <summary>Last time "Check for updates" was performed, for throttling checks.</summary>
    public static string LastUpdateCheckTicks
    {
        get => _values.TryGetValue("LastUpdateCheckTicks", out var v) ? v : string.Empty;
        set { _values["LastUpdateCheckTicks"] = value; Save(); }
    }

    /// <summary>
    /// Best-effort detection of a local FC26 asset pack (the layout discovered in the asset
    /// inventory). Returns null when none is present; the user can always set AssetRoot manually.
    /// Only recognises a folder that actually contains a 'miniface' subfolder with real files, or
    /// the FC Editor 'assets' tree — never fabricates a path.
    /// </summary>
    private static string? AutoDetectAssetRoot()
    {
        // Only portable, non-developer locations are probed. The user can always set the
        // asset root explicitly in Settings. (No absolute development paths are baked in.)
        var candidates = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FC26 Assets"),
        };
        // Common portable pack names, checked only at a drive root (never a broad scan).
        candidates.AddRange(ExternalToolLocator.DriveRootFolders("FC26 FILE TOOL", "FC26 Assets"));
        foreach (var root in candidates)
        {
            try
            {
                var miniface = Path.Combine(root, "miniface");
                if (Directory.Exists(miniface) &&
                    (Directory.EnumerateFiles(miniface, "p*.png", SearchOption.AllDirectories).Any() ||
                     Directory.EnumerateFiles(miniface, "p*.dds", SearchOption.AllDirectories).Any()))
                    return root;
                var fceditor = Path.Combine(root, "FC Editor by decoruiz Alpha v21", "assets");
                if (Directory.Exists(fceditor))
                    return root;
            }
            catch { /* keep looking */ }
        }
        return null;
    }

    private static Dictionary<string, string> Load()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (File.Exists(SettingsPath))
                foreach (var line in File.ReadAllLines(SettingsPath))
                {
                    var idx = line.IndexOf('=');
                    if (idx > 0) dict[line[..idx].Trim()] = line[(idx + 1)..].Trim();
                }
        }
        catch { /* best effort */ }
        return dict;
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllLines(SettingsPath, _values.Select(kv => $"{kv.Key}={kv.Value}"));
        }
        catch { /* best effort */ }
    }
}
