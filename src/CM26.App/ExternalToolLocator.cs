namespace CM26.App;

internal static class ExternalToolLocator
{
    /// <summary>
    /// Locates the CM26 Scraper executable. The scraper is a separate optional
    /// download and is deliberately not bundled in the CM26 package, so every
    /// candidate here is a user-supplied location. Search order:
    /// 1. the user-configured scraper folder (Settings &gt; CM26 Scraper folder),
    /// 2. a "CM26 Scraper" / "CM26 SCRAPER" folder next to CM26 or under Tools,
    /// 3. the same folder names at a drive root or under a drive-root
    ///    "FC26 FILE TOOL" folder.
    /// </summary>
    public static string? FindScraperExecutable()
    {
        var configured = SettingsService.ScraperRoot;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            // A user may point Settings at either the folder holding the exe or
            // the exe itself; accept both rather than silently finding nothing.
            var candidate = SafeFullPath(configured, "CM26 Scraper.exe");
            if (candidate != null && File.Exists(candidate)) return candidate;
            if (File.Exists(configured) &&
                string.Equals(Path.GetFileName(configured), "CM26 Scraper.exe", StringComparison.OrdinalIgnoreCase))
                return Path.GetFullPath(configured);
        }

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
        foreach (var drive in EnumerateSearchableDrives())
        {
            roots.Add(drive);
            roots.Add(Path.Combine(drive, "FC26 FILE TOOL"));
        }
        foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        foreach (var relative in relativeCandidates)
        {
            var candidate = SafeFullPath(root, relative);
            if (candidate != null && File.Exists(candidate)) return candidate;
        }
        return null;
    }

    /// <summary>
    /// Local fixed and removable drive roots only. Network drives are skipped because a
    /// disconnected mapping can block for the full SMB timeout and freeze the UI.
    /// Shared by every "scan the drive roots for an FC26 tool folder" probe.
    /// </summary>
    public static IReadOnlyList<string> EnumerateSearchableDrives()
    {
        DriveInfo[] drives;
        try
        {
            drives = DriveInfo.GetDrives();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }

        var results = new List<string>();
        foreach (var drive in drives)
        {
            try
            {
                if (drive.DriveType is not (DriveType.Fixed or DriveType.Removable)) continue;
                if (!drive.IsReady) continue;
                results.Add(drive.RootDirectory.FullName);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CM26] Drive probe IO error: {ex.Message}");
                // Drive disappeared or is unreadable between enumeration and probing.
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CM26] Drive probe access denied: {ex.Message}");
                // No permission to inspect this volume.
            }
        }
        return results;
    }

    /// <summary>
    /// Drive-root probe folders shared by the asset-pack and 3D export detectors,
    /// e.g. DriveRootFolders("FC26 FILE TOOL", "FC26 Assets") yields C:\FC26 Assets etc.
    /// </summary>
    public static IEnumerable<string> DriveRootFolders(params string[] folderNames)
    {
        foreach (var drive in EnumerateSearchableDrives())
        foreach (var name in folderNames)
        {
            var candidate = SafeFullPath(drive, name);
            if (candidate != null) yield return candidate;
        }
    }

    /// <summary>
    /// Combines and normalises a path, returning null instead of throwing when the
    /// user-supplied segment contains invalid characters or is otherwise malformed.
    /// </summary>
    public static string? SafeFullPath(string root, string relative)
    {
        try
        {
            return Path.GetFullPath(Path.Combine(root, relative));
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (PathTooLongException)
        {
            return null;
        }
    }
}
