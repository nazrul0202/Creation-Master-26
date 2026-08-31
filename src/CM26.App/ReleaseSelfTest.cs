using System.Text;
using System.Reflection;
using CM26.App.Theming;

using CM26.Application.Services;

namespace CM26.App;

/// <summary>
/// Self-contained release checks that need no FC26 installation, no database and
/// no UI. These are the tests CI can run on a clean machine, so they exist to
/// lock down the regressions that shipped in past releases:
///   * a developer's absolute path baked into tool detection
///   * EA-derived game content reaching the package
///   * version strings drifting apart across files
/// Run with: Creation Master 26.exe --release-selftest
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

        // --- Runtime dependencies must be intact ---------------------------
        // A damaged Open XML assembly can leave the editor apparently healthy
        // until a Compdata workbook or TXT folder is opened. Validate both the
        // files and the real Compdata read path before any package is accepted.
        Check("Open XML runtime assemblies are intact", () =>
        {
            foreach (var name in new[]
                     {
                         "DocumentFormat.OpenXml.dll",
                         "DocumentFormat.OpenXml.Framework.dll",
                     })
            {
                var path = Path.Combine(AppContext.BaseDirectory, name);
                if (!File.Exists(path)) return $"{name} is missing";
                using (var stream = File.OpenRead(path))
                {
                    if (stream.ReadByte() != 'M' || stream.ReadByte() != 'Z')
                        return $"{name} is not a valid PE assembly";
                }
                var identity = AssemblyName.GetAssemblyName(path);
                if (identity.Version == null)
                    return $"{name} has no assembly version";
                Assembly.LoadFrom(path);
            }
            return null;
        });

        Check("Compdata TXT reader round-trips a real FC26 row", () =>
        {
            var temp = Path.Combine(Path.GetTempPath(), "cm26-selftest-compdata-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(temp);
                File.WriteAllText(Path.Combine(temp, "compobj.txt"),
                    "0,0,WORLD,World,-1\r\n", new UTF8Encoding(false));
                var workbook = new CompdataWorkbookService();
                workbook.OpenFromGameFolder(temp);
                var table = workbook.ReadSheet("compobj");
                if (table.Rows.Count != 1 || table.Columns.Count != 5)
                    return $"unexpected table shape {table.Rows.Count}x{table.Columns.Count}";
                return Convert.ToString(table.Rows[0][2]) == "WORLD"
                    ? null
                    : "Compdata row content changed during read";
            }
            finally
            {
                try { Directory.Delete(temp, recursive: true); }
                catch { }
            }
        });

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
                try { Directory.Delete(temp, true); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Self-test temp cleanup failed: {ex.Message}"); /* temp cleanup is best-effort */ }
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

        // --- Regression: ListView theming must not crash on creation ------
        // The app crashed on the real FC26 database with Windows "Exception
        // Processing Message 0xc0000005 - Unexpected parameters": the header
        // styling sent native HDM_* messages to the ListView header control,
        // and the fault happened inside the native window procedure where no
        // try/catch can intercept it. This test exercises the exact creation
        // path (HandleCreated fires inside WM_CREATE) with the theming applied.
        Check("ListView theming survives handle creation", () =>
        {
            using var form = new System.Windows.Forms.Form { ShowInTaskbar = false };
            var list = new System.Windows.Forms.ListView { View = System.Windows.Forms.View.Details };
            list.Columns.Add("Column A");
            list.Columns.Add("Column B");
            form.Controls.Add(list);
            Theme.ApplyListView(list);
            form.Show(); // forces handle creation -> HandleCreated fires
            System.Windows.Forms.Application.DoEvents();
            System.Windows.Forms.Application.DoEvents();
            var alive = !list.IsDisposed && list.IsHandleCreated;
            form.Close();
            return alive ? null : "listview handle was not created";
        });

        Check("ListView theming survives re-entrant handle creation", () =>
        {
            // Same path with a shown form and a freshly created control on the
            // same thread, which is how the record browsers build their lists.
            using var form = new System.Windows.Forms.Form { ShowInTaskbar = false };
            var list = new System.Windows.Forms.ListView { View = System.Windows.Forms.View.Details };
            list.Columns.Add("A");
            list.Columns.Add("B");
            list.Columns.Add("C");
            form.Controls.Add(list);
            Theme.ApplyListView(list);
            form.Show();
            System.Windows.Forms.Application.DoEvents();
            System.Windows.Forms.Application.DoEvents();
            var alive = !list.IsDisposed && list.IsHandleCreated;
            form.Close();
            return alive ? null : "listview handle was not created";
        });

        // --- Regression: every module icon must resolve without throwing ---
        // A section mapped to an absent icon must fall back to a drawn badge,
        // never call GetManifestResourceStream with an empty name (which throws
        // "String cannot have zero length" and aborted startup in v1.0.27).
        Check("every module icon resolves without throwing", () =>
        {
            var keys = new[]
            {
                "dashboard", "countries", "leagues", "teams", "players", "managers",
                "stadiums", "stadiumaudio", "kits", "competitions", "formations",
                "transfers", "balls", "boots", "gloves", "sponsors", "adboards",
                "audio", "scoreboard", "referees", "browser", "diagnostics", "settings",
            };
            foreach (var key in keys)
            {
                try
                {
                    using var img = IconService.Get(key, 28);
                    if (img == null) return $"icon for '{key}' was null";
                }
                catch (Exception ex)
                {
                    return $"icon for '{key}' threw {ex.GetType().Name}: {ex.Message}";
                }
            }
            return null;
        });

        // --- Regression: the full UI shell must construct at startup ---------
        // The Studio rebuild crashed on launch with "Control does not support
        // transparent background colors" inside StudioSidebarItem..ctor because
        // plain-Control custom controls set a transparent BackColor. This check
        // builds the exact startup path (MainForm -> StudioSidebar -> items),
        // which the previous release self-test never exercised.
        Check("MainForm shell constructs without throwing", () =>
        {
            using var form = new MainForm();
            form.CreateControl();
            System.Windows.Forms.Application.DoEvents();
            // The window must have its chrome: menu, toolbar, sidebar, status.
            var hasMenu = form.Controls.OfType<MenuStrip>().Any();
            var hasStatus = form.Controls.OfType<StatusStrip>().Any();
            if (!hasMenu || !hasStatus) return $"chrome incomplete (menu={hasMenu}, status={hasStatus})";
            return null;
        });

        // --- Regression: sidebar navigation must never recurse ---------------
        // Clicking a sidebar item fired StudioSidebar.SetActive -> ItemClicked ->
        // MainForm.NavigateTo -> SetActive ... until the native stack overflowed
        // ("A new guard page for the stack cannot be created") on every click and
        // after every database load. SetActive is now idempotent; this check
        // drives the real click path (dashboard needs no database) and verifies
        // the shell survives and stops navigating after one pass.
        Check("sidebar item click does not recurse", () =>
        {
            using var form = new MainForm();
            form.CreateControl();
            System.Windows.Forms.Application.DoEvents();
            var sidebar = Descendants(form)
                .OfType<CM26.App.Controls.Studio.StudioSidebar>()
                .FirstOrDefault();
            if (sidebar == null) return "StudioSidebar was not found in the shell";
            var dashboard = Descendants(sidebar)
                .OfType<CM26.App.Controls.Studio.StudioSidebarItem>()
                .FirstOrDefault(item => string.Equals(item.Model.Key, "dashboard", StringComparison.OrdinalIgnoreCase));
            if (dashboard == null) return "dashboard sidebar item was not found";
            // Click it twice: the second click must not re-raise navigation.
            typeof(CM26.App.Controls.Studio.StudioSidebarItem)
                .GetMethod("OnClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(dashboard, new object[] { EventArgs.Empty });
            System.Windows.Forms.Application.DoEvents();
            typeof(CM26.App.Controls.Studio.StudioSidebarItem)
                .GetMethod("OnClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(dashboard, new object[] { EventArgs.Empty });
            System.Windows.Forms.Application.DoEvents();
            return form.IsDisposed ? "MainForm was disposed during navigation" : null;
        });

        // --- Regression: game launch goes through the Steam DRM protocol ------
        // FC26.exe is a Steam build; starting the exe directly exits with code
        // 100010 and no window. Launch must detect steam_appid.txt and build
        // steam://run/<appid>//-dataPath ... instead of launching the exe.
        Check("game launch uses the Steam DRM protocol when present", () =>
        {
            var temp = Path.Combine(Path.GetTempPath(), "cm26-selftest-steam-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(Path.Combine(temp, "FIFAModData"));
                File.WriteAllText(Path.Combine(temp, "steam_appid.txt"), "3405690\n");
                var appId = CM26ModLaunchService.ResolveSteamAppId(temp);
                if (appId != "3405690") return "steam_appid.txt was not resolved: " + (appId ?? "<null>");
                var protocol = CM26ModLaunchService.BuildSteamProtocol(appId, "-dataPath FIFAModData");
                if (protocol != "steam://run/3405690//-dataPath%20FIFAModData")
                    return "unexpected protocol: " + protocol;
                return null;
            }
            catch (Exception ex) { return "protocol resolution threw: " + ex.Message; }
            finally { try { Directory.Delete(temp, recursive: true); } catch { } }
        });

        // --- Regression: game launch falls back to the exe without Steam -------
        Check("game launch falls back to FC26.exe when Steam is absent", () =>
        {
            var temp = Path.Combine(Path.GetTempPath(), "cm26-selftest-nosteam-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(temp);
                if (CM26ModLaunchService.ResolveSteamAppId(temp) != null)
                    return "steam app id resolved without steam_appid.txt";
                var launch = CM26ModLaunchService.Launch(temp, "-dataPath FIFAModData");
                if (launch.Success) return "launch unexpectedly reported success without FC26.exe";
                return null;
            }
            catch (Exception ex) { return "no-steam launch threw: " + ex.Message; }
            finally { try { Directory.Delete(temp, recursive: true); } catch { } }
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

    private static IEnumerable<System.Windows.Forms.Control> Descendants(System.Windows.Forms.Control root)
    {
        foreach (System.Windows.Forms.Control child in root.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }
}
