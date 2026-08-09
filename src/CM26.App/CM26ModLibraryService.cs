using System.Text.Json;

namespace CM26.App;

/// <summary>Local library and enabled-order state for standalone CM26 mods.</summary>
public static class CM26ModLibraryService
{
    private static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "mods");
    private static readonly string StatePath = Path.Combine(Root, "enabled.json");

    public sealed record LibraryItem(string PackagePath, CM26ModPackageService.PackageManifest Manifest, bool Enabled);

    public static IReadOnlyList<LibraryItem> List()
    {
        Directory.CreateDirectory(Root);
        var enabled = LoadEnabled();
        return Directory.EnumerateFiles(Root, "*" + CM26ModPackageService.Extension)
            .Select(path => TryRead(path, enabled.Contains(Path.GetFullPath(path), StringComparer.OrdinalIgnoreCase)))
            .Where(item => item != null)
            .Cast<LibraryItem>()
            .OrderBy(item => item.Manifest.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static LibraryItem Import(string sourcePath)
    {
        var manifest = CM26ModPackageService.ReadManifest(sourcePath);
        Directory.CreateDirectory(Root);
        var name = string.Concat(manifest.Name.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch)).Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "CM26-Mod";
        var target = Path.Combine(Root, name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + CM26ModPackageService.Extension);
        File.Copy(sourcePath, target, overwrite: false);
        return new(Path.GetFullPath(target), manifest, false);
    }

    public static void SetEnabled(string packagePath, bool enabled)
    {
        var full = Path.GetFullPath(packagePath);
        EnsureLibraryChild(full);
        var entries = LoadEnabled();
        entries.RemoveAll(item => string.Equals(item, full, StringComparison.OrdinalIgnoreCase));
        if (enabled) entries.Add(full);
        File.WriteAllText(StatePath, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static IReadOnlyList<string> EnabledPackages() => LoadEnabled()
        .Where(File.Exists).ToArray();

    private static LibraryItem? TryRead(string path, bool enabled)
    {
        try { return new LibraryItem(path, CM26ModPackageService.ReadManifest(path), enabled); }
        catch { return null; }
    }

    private static List<string> LoadEnabled()
    {
        try { return File.Exists(StatePath)
            ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(StatePath)) ?? []
            : []; }
        catch { return []; }
    }

    private static void EnsureLibraryChild(string candidate)
    {
        var prefix = Path.GetFullPath(Root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new IOException("CM26 mod is outside the local mod library.");
    }
}
