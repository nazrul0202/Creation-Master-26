using System.Text.Json;

namespace CM26.App;

/// <summary>Local library and enabled-order state for standalone CM26 mods.</summary>
public static class CM26ModLibraryService
{
    /// <summary>User-visible standalone library, kept away from the FC26 install and FET.</summary>
    public static readonly string Root = @"D:\CM26 Mod Manager";
    public static readonly string ModsRoot = Path.Combine(Root, "Mods");
    public static readonly string LogsRoot = Path.Combine(Root, "Logs");
    private static readonly string StatePath = Path.Combine(Root, "CM26 Mod Manager.json");
    private static readonly string LegacyStatePath = Path.Combine(Root, "enabled.json");

    public sealed record LibraryItem(string PackagePath, CM26ModPackageService.PackageManifest Manifest, bool Enabled);

    public static IReadOnlyList<LibraryItem> List()
    {
        EnsureRoots();
        var enabled = LoadEnabled();
        return Directory.EnumerateFiles(ModsRoot, "*" + CM26ModPackageService.Extension)
            .Select(path => TryRead(path, enabled.Contains(Path.GetFullPath(path), StringComparer.OrdinalIgnoreCase)))
            .Where(item => item != null)
            .Cast<LibraryItem>()
            .OrderBy(item => item.Manifest.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static LibraryItem Import(string sourcePath)
    {
        var manifest = CM26ModPackageService.ReadManifest(sourcePath);
        EnsureRoots();
        var name = string.Concat(manifest.Name.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch)).Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "CM26-Mod";
        var target = Path.Combine(ModsRoot, name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + CM26ModPackageService.Extension);
        File.Copy(sourcePath, target, overwrite: false);
        return new(Path.GetFullPath(target), manifest, false);
    }

    public static void SetEnabled(string packagePath, bool enabled)
    {
        EnsureRoots();
        var full = Path.GetFullPath(packagePath);
        EnsureLibraryChild(full);
        var entries = LoadEnabled();
        entries.RemoveAll(item => string.Equals(item, full, StringComparison.OrdinalIgnoreCase));
        if (enabled) entries.Add(full);
        File.WriteAllText(StatePath, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static IReadOnlyList<string> EnabledPackages() => LoadEnabled()
        .Where(File.Exists).ToArray();

    public static string WriteLog(string message)
    {
        EnsureRoots();
        var path = Path.Combine(LogsRoot, "CM26-Mod-Manager-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
        File.AppendAllText(path, $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}");
        return path;
    }

    private static LibraryItem? TryRead(string path, bool enabled)
    {
        try { return new LibraryItem(path, CM26ModPackageService.ReadManifest(path), enabled); }
        catch { return null; }
    }

    private static List<string> LoadEnabled()
    {
        try { return File.Exists(StatePath)
            ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(StatePath)) ?? []
            : File.Exists(LegacyStatePath)
                ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(LegacyStatePath)) ?? []
                : []; }
        catch { return []; }
    }

    private static void EnsureLibraryChild(string candidate)
    {
        var prefix = Path.GetFullPath(ModsRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new IOException("CM26 mod is outside the local mod library.");
    }

    private static void EnsureRoots()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(ModsRoot);
        Directory.CreateDirectory(LogsRoot);
    }
}
