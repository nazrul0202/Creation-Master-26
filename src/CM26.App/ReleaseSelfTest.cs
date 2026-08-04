using System.Text;

namespace CM26.App;

/// <summary>
/// Self-contained release checks that need no FC26 installation, no database and
/// no UI. These are the tests CI can run on a clean machine, so they exist to
/// lock down the regressions that shipped in past releases:
///   * a developer's absolute path baked into tool detection
///   * EA-derived game content reaching the package
///   * version strings drifting apart across files
/// Run with: CM26_by_Rizco98.exe --release-selftest
/// </summary>
internal static class ReleaseSelfTest
{
    public static int Run()
    {
        var failures = new List<string>();
        var checks = 0;

        void Check(string name, Func<string?> test)
        {
            checks++;
            string? failure;
            try
            {
                failure = test();
            }
            catch (Exception ex)
            {
                failure = "threw " + ex.GetType().Name + ": " + ex.Message;
            }
            if (failure == null)
            {
                Console.WriteLine($"  PASS  {name}");
            }
            else
            {
                Console.WriteLine($"  FAIL  {name}: {failure}");
                failures.Add($"{name}: {failure}");
            }
        }

        Console.WriteLine("=== CM26 release self-test ===");

        // --- Tool detection must never depend on a developer machine ---------
        Check("scraper detection contains no hardcoded developer path", () =>
        {
            // With no configured folder and no installed copy, detection must
            // return null rather than a path from the author's PC.
            var previous = SettingsService.ScraperRoot;
            try
            {
                SettingsService.ScraperRoot = string.Empty;
                var found = ExternalToolLocator.FindScraperExecutable();
                if (found == null) return null;
                // A real local install is acceptable, but it must be a path that
                // actually exists and was discovered, not assumed.
                return File.Exists(found)
                    ? null
                    : $"returned a non-existent path '{found}'";
            }
            finally
            {
                SettingsService.ScraperRoot = previous;
            }
        });

        Check("drive-root probing skips network drives and never throws", () =>
        {
            var drives = ExternalToolLocator.EnumerateSearchableDrives();
            foreach (var drive in drives)
            {
                var info = new DriveInfo(drive);
                if (info.DriveType is not (DriveType.Fixed or DriveType.Removable))
                    return $"returned a {info.DriveType} drive '{drive}'";
            }
            return null;
        });

        Check("malformed user path is rejected instead of throwing", () =>
        {
            // A pasted path with invalid characters must not crash Data Sync.
            var result = ExternalToolLocator.SafeFullPath("C:\\", "bad\0name\\tool.exe");
            return result == null ? null : $"expected null, got '{result}'";
        });

        Check("configured scraper folder is honoured", () =>
        {
            var previous = SettingsService.ScraperRoot;
            var temp = Path.Combine(Path.GetTempPath(), "cm26-selftest-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(temp);
                var exe = Path.Combine(temp, "CM26 Scraper.exe");
                File.WriteAllText(exe, "stub");
                SettingsService.ScraperRoot = temp;
                var found = ExternalToolLocator.FindScraperExecutable();
                if (!string.Equals(found, exe, StringComparison.OrdinalIgnoreCase))
                    return $"expected '{exe}', got '{found ?? "null"}'";

                // Pointing directly at the exe must work too.
                SettingsService.ScraperRoot = exe;
                found = ExternalToolLocator.FindScraperExecutable();
                return string.Equals(found, exe, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : $"exe-path form failed: got '{found ?? "null"}'";
            }
            finally
            {
                SettingsService.ScraperRoot = previous;
                try { Directory.Delete(temp, true); } catch { /* temp cleanup is best-effort */ }
            }
        });

        // --- The shipped folder must not redistribute EA game content --------
        Check("installed package contains no EA-derived game content", () =>
        {
            var forbiddenExtensions = new[]
            {
                ".fcepatch", ".dbc", ".dbp", ".db", ".sqlite", ".big", ".ebx", ".res", ".chunk",
            };
            var forbiddenNames = new[]
            {
                "master.db", "fifa_ng_db-meta.xml", "playernames.txt",
            };
            var hits = new List<string>();
            foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories))
            {
                var name = Path.GetFileName(file).ToLowerInvariant();
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (forbiddenExtensions.Contains(ext) || forbiddenNames.Contains(name))
                    hits.Add(Path.GetRelativePath(AppContext.BaseDirectory, file));
            }
            return hits.Count == 0
                ? null
                : $"{hits.Count} file(s) violate the EULA no-game-content promise: " +
                  string.Join(", ", hits.Take(5));
        });

        // --- Version must be consistent everywhere --------------------------
        Check("assembly version matches version.json", () =>
        {
            var manifest = Path.Combine(AppContext.BaseDirectory, "version.json");
            if (!File.Exists(manifest)) return "version.json is not next to the executable";
            var text = File.ReadAllText(manifest, Encoding.UTF8);
            var marker = "\"version\"";
            var at = text.IndexOf(marker, StringComparison.Ordinal);
            if (at < 0) return "version.json has no \"version\" field";
            var open = text.IndexOf('"', text.IndexOf(':', at) + 1);
            var close = text.IndexOf('"', open + 1);
            var declared = text.Substring(open + 1, close - open - 1);
            return declared == Program.ProductVersion
                ? null
                : $"version.json='{declared}' but assembly='{Program.ProductVersion}'";
        });

        Check("EULA is present and states the no-game-content promise", () =>
        {
            var eula = Path.Combine(AppContext.BaseDirectory, "EULA.md");
            if (!File.Exists(eula)) return "EULA.md is not next to the executable";
            var text = File.ReadAllText(eula);
            return text.Contains("no EA game content", StringComparison.OrdinalIgnoreCase)
                ? null
                : "EULA.md no longer contains the no-game-content statement";
        });

        Console.WriteLine();
        if (failures.Count == 0)
        {
            Console.WriteLine($"RELEASE SELF-TEST OK ({checks} checks passed)");
            return 0;
        }
        Console.WriteLine($"RELEASE SELF-TEST FAILED ({failures.Count} of {checks} checks)");
        foreach (var failure in failures) Console.WriteLine("  * " + failure);
        return 1;
    }
}
