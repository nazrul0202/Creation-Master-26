namespace CM26.App;

internal static class ExternalToolLocator
{
    /// <summary>
    /// Locates the CM26 Scraper executable. Search order:
    /// 1. the user-configured scraper folder (Settings &gt; CM26 Scraper folder),
    /// 2. the bundled copy under Tools\CM26 Scraper in the package,
    /// 3. a "CM26 Scraper" / "CM26 SCRAPER" folder next to CM26, under Tools,
    ///    at a drive root or under a drive-root "FC26 FILE TOOL" folder.
    /// </summary>
    public static string? FindScraperExecutable()
    {
        var configured = SettingsService.ScraperRoot;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var candidate = Path.GetFullPath(Path.Combine(configured, "CM26 Scraper.exe"));
            if (File.Exists(candidate)) return candidate;
        }

        var bundled = Path.Combine(AppContext.BaseDirectory, "Tools", "CM26 Scraper", "CM26 Scraper.exe");
        if (File.Exists(bundled)) return bundled;

        // This is the established public installation location. Check it before
        // enumerating every removable/network drive, which can delay Data Sync.
        var fc26Tool = @"D:\FC26 FILE TOOL\CM26 SCRAPER\CM26 Scraper.exe";
        if (File.Exists(fc26Tool)) return fc26Tool;

        return FindFile(
            Path.Combine("CM26 Scraper", "CM26 Scraper.exe"),
            Path.Combine("CM26 SCRAPER", "CM26 Scraper.exe"));
    }

    public static string? FindFile(params string[] relativeCandidates)
    {
        var roots = new List<string>
        {
            AppContext.BaseDirectory,
            Path.Combine(AppContext.BaseDirectory, "Tools"),
        };
        foreach (var drive in DriveInfo.GetDrives())
        {
            try
            {
                if (!drive.IsReady) continue;
                roots.Add(drive.RootDirectory.FullName);
                roots.Add(Path.Combine(drive.RootDirectory.FullName, "FC26 FILE TOOL"));
            }
            catch { }
        }
        foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        foreach (var relative in relativeCandidates)
        {
            var candidate = Path.GetFullPath(Path.Combine(root, relative));
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }
}
