using System.IO;
using CM26.Application;

namespace CM26.Studio.Services;

/// <summary>
/// Locates FC26 workspace database folders the same way the WinForms app does:
/// the settings file (LastFolder/RecentFolders) first, then the default
/// workspaces directory under LocalAppData\Creation Master 26.
/// </summary>
public static class WorkspaceLocator
{
    private static readonly string LocalAppData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26");

    private static readonly string DefaultWorkspaces = Path.Combine(LocalAppData, "workspaces");

    public static string? GameFolder
    {
        get
        {
            var v = SettingsValue("FC26GameFolder");
            if (!string.IsNullOrWhiteSpace(v) && Directory.Exists(v)) return v;
            return null;
        }
    }

    public static IReadOnlyList<string> RecentDatabaseFolders()
    {
        var list = new List<string>();
        foreach (var value in new[] { SettingsValue("LastFolder") }
            .Concat(SettingsValue("RecentFolders").Split(';', StringSplitOptions.RemoveEmptyEntries)))
        {
            if (!string.IsNullOrWhiteSpace(value) && !list.Contains(value, StringComparer.OrdinalIgnoreCase))
                list.Add(value);
        }
        list.AddRange(ScanWorkspaces());
        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IEnumerable<string> ScanWorkspaces()
    {
        if (!Directory.Exists(DefaultWorkspaces)) yield break;
        foreach (var dir in Directory.EnumerateDirectories(DefaultWorkspaces))
        {
            var db = Path.Combine(dir, "database");
            if (Directory.Exists(db) &&
                File.Exists(Path.Combine(db, "fifa_ng_db-meta.xml")) &&
                File.Exists(Path.Combine(db, "fifa_ng_db.db")))
                yield return db;
        }
    }

    private static string SettingsValue(string key)
    {
        try
        {
            var path = Path.Combine(LocalAppData, "settings.ini");
            if (!File.Exists(path)) return string.Empty;
            foreach (var line in File.ReadAllLines(path))
            {
                var idx = line.IndexOf('=');
                if (idx > 0 && string.Equals(line[..idx].Trim(), key, StringComparison.OrdinalIgnoreCase))
                    return line[(idx + 1)..].Trim();
            }
        }
        catch { /* best effort */ }
        return string.Empty;
    }
}
