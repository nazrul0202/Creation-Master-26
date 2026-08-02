namespace CM26.App;

internal static class ExternalToolLocator
{
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
