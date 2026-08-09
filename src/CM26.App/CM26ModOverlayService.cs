using System.Text.Json;

namespace CM26.App;

/// <summary>Builds an isolated Frostbite overlay. The installed Data/Patch tree is read-only input.</summary>
public static class CM26ModOverlayService
{
    public static string OverlayRoot(string gameRoot) => Path.Combine(gameRoot, "CM26ModData");

    public static (bool Success, string Message) Build(string gameRoot, IEnumerable<string> packages,
        FrostbiteAssetSession bridge, IProgress<string>? progress = null)
    {
        var root = Path.GetFullPath(gameRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Directory.Exists(Path.Combine(root, "Data")) || !Directory.Exists(Path.Combine(root, "Patch")))
            return (false, "FC26 Data/Patch folders were not found.");
        var selected = packages.Where(File.Exists).ToArray();
        if (selected.Length == 0) return (false, "Enable at least one CM26 mod before building the overlay.");
        var overlay = OverlayRoot(root);
        var temporary = overlay + ".building-" + Guid.NewGuid().ToString("N");
        try
        {
            progress?.Report("Copying original FC26 Data/Patch into the isolated CM26 overlay...");
            CopyTree(Path.Combine(root, "Data"), Path.Combine(temporary, "CmModData", "Data"));
            CopyTree(Path.Combine(root, "Patch"), Path.Combine(temporary, "CmModData", "Patch"));
            CopyTree(Path.Combine(temporary, "CmModData", "Data"), Path.Combine(temporary, "Data"));
            CopyTree(Path.Combine(temporary, "CmModData", "Patch"), Path.Combine(temporary, "Patch"));

            var payloadRoot = Path.Combine(temporary, "payload");
            progress?.Report("Extracting enabled CM26 mods...");
            foreach (var package in selected) CM26ModPackageService.ExtractToOverlay(package, payloadRoot);
            var replacements = Directory.EnumerateFiles(payloadRoot, "*", SearchOption.AllDirectories)
                .Select(path => new { legacyPath = Path.GetRelativePath(payloadRoot, path).Replace('\\', '/'), sourcePath = path })
                .ToArray();
            var plan = Path.Combine(temporary, "cm26-overlay-plan.json");
            File.WriteAllText(plan, JsonSerializer.Serialize(new { replacements }, new JsonSerializerOptions { WriteIndented = true }));
            progress?.Report("Writing CM26 payloads into the Frostbite overlay...");
            var applied = bridge.ApplyOverlay(temporary, plan);
            if (!applied.Success) return applied;

            if (Directory.Exists(overlay)) Directory.Delete(overlay, recursive: true);
            Directory.Move(temporary, overlay);
            return (true, "CM26ModData overlay built successfully. Original FC26 Data/Patch was not changed.");
        }
        catch (Exception ex) { return (false, "CM26 mod overlay build failed: " + ex.Message); }
        finally { if (Directory.Exists(temporary)) Directory.Delete(temporary, recursive: true); }
    }

    private static void CopyTree(string source, string destination)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: false);
        }
    }
}
