using System.Diagnostics;
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
            progress?.Report("Creating the lightweight FC26 symbolic-link overlay...");
            CreateSymbolicLinkOverlay(root, temporary);

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

            // Build-only material is not required by -dataPath. Removing it
            // keeps the installed overlay limited to links plus files changed
            // by FrostbiteDirectLegacyWriter's copy-on-write transaction.
            Directory.Delete(Path.Combine(temporary, "payload"), recursive: true);
            Directory.Delete(Path.Combine(temporary, "CmModData"), recursive: true);
            File.Delete(plan);

            if (Directory.Exists(overlay)) Directory.Delete(overlay, recursive: true);
            Directory.Move(temporary, overlay);
            return (true, "CM26ModData symbolic-link overlay built successfully. Original FC26 Data/Patch was not changed.");
        }
        catch (Exception ex) { return (false, "CM26 mod overlay build failed: " + ex.Message); }
        finally { if (Directory.Exists(temporary)) Directory.Delete(temporary, recursive: true); }
    }

    private static void CreateSymbolicLinkOverlay(string gameRoot, string overlayRoot)
    {
        var links = new List<(string Link, string Target)>();
        foreach (var sourceName in new[] { "Data", "Patch" })
        {
            var sourceRoot = Path.Combine(gameRoot, sourceName);
            var liveOverlayRoot = Path.Combine(overlayRoot, sourceName);
            foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                var link = Path.Combine(liveOverlayRoot, Path.GetRelativePath(sourceRoot, file));
                Directory.CreateDirectory(Path.GetDirectoryName(link)!);
                links.Add((link, file));
            }
        }

        var oodle = Path.Combine(gameRoot, "oo2core_9_win64.dll");
        if (!File.Exists(oodle))
            throw new FileNotFoundException("FC26 Oodle library was not found.", oodle);
        links.Add((Path.Combine(overlayRoot, Path.GetFileName(oodle)), oodle));

        // ApplyOverlay validates an immutable baseline before it writes. Only
        // its four sentinel files are required in this build-only backup tree.
        foreach (var relative in new[]
                 {
                     Path.Combine("Data", "layout.toc"),
                     Path.Combine("Data", "initfs_Win32"),
                     Path.Combine("Patch", "layout.toc"),
                     Path.Combine("Patch", "initfs_Win32"),
                 })
        {
            var target = Path.Combine(gameRoot, relative);
            if (!File.Exists(target))
                throw new FileNotFoundException("Required FC26 baseline file was not found.", target);
            var link = Path.Combine(overlayRoot, "CmModData", relative);
            Directory.CreateDirectory(Path.GetDirectoryName(link)!);
            links.Add((link, target));
        }

        CreateLinksWithMkLink(links);
    }

    private static void CreateLinksWithMkLink(IReadOnlyList<(string Link, string Target)> links)
    {
        var batch = Path.Combine(Path.GetTempPath(), "cm26-mklink-" + Guid.NewGuid().ToString("N") + ".cmd");
        try
        {
            using (var writer = new StreamWriter(batch, append: false, new System.Text.UTF8Encoding(false)))
            {
                writer.WriteLine("@echo off");
                foreach (var (link, target) in links)
                {
                    if (link.Contains('"') || target.Contains('"'))
                        throw new InvalidOperationException("FC26 paths containing quotation marks are not supported.");
                    writer.WriteLine($"mklink \"{link}\" \"{target}\" >nul || exit /b 1");
                }
            }

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/D /C \"\"{batch}\"\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            }) ?? throw new InvalidOperationException("Windows could not start mklink.");
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    "Windows could not create the CM26 symbolic links. Run Creation Master 26 as Administrator " +
                    "or enable Windows Developer Mode, then try again." +
                    (string.IsNullOrWhiteSpace(standardError) ? string.Empty : " " + standardError.Trim()));

            var missing = links.FirstOrDefault(item => !File.Exists(item.Link));
            if (!string.IsNullOrEmpty(missing.Link))
                throw new IOException("Windows reported success but a CM26 symbolic link was not created: " + missing.Link);
        }
        finally
        {
            try { File.Delete(batch); }
            catch (Exception ex) { Debug.WriteLine("CM26 mklink batch cleanup failed: " + ex.Message); }
        }
    }
}
