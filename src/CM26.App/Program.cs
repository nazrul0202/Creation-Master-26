using System.Diagnostics;
using System.Windows.Forms;
using WinApp = System.Windows.Forms.Application;

namespace CM26.App;

internal static class Program
{
    public static string ProductVersion => typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "unknown";

    public static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "cm26.log");

    [STAThread]
    private static void Main(string[] args)
    {
        // Self-contained Full Portable carries its own .NET runtime; the Lite framework-dependent
        // build does not. On the Lite build, verify the .NET 8 Desktop Runtime is present and, if
        // not, show a clear install requirement instead of a generic launch failure.
        if (!AppDependencyCheck.EnsureDesktopRuntime())
            return;

        // Every "--" argument is a headless console diagnostic. Attach to the parent
        // console first so results are visible in the terminal and in CI logs.
        if (args.Length >= 1 && args[0].StartsWith("--", StringComparison.Ordinal))
            ConsoleAttach.EnsureConsole();

        // Self-contained release checks: no game install, no database, no UI.
        // This is the gate CI runs on a clean machine.
        if (args.Length >= 1 && args[0] == "--release-selftest")
        {
            Environment.ExitCode = ReleaseSelfTest.Run();
            return;
        }

        // Headless smoke: "--smoke <dbFolder>" loads via the real services and exits. No UI.
        if (args.Length >= 2 && args[0] == "--smoke")
        {
            Environment.ExitCode = HeadlessSmoke.Run(args[1]);
            return;
        }

        // Verifies direct extraction from the installed FC26 Data/Patch archives.
        if (args.Length >= 1 && args[0] == "--workspace-test")
        {
            Environment.ExitCode = HeadlessSmoke.WorkspaceTest(args.Length >= 2 ? args[1] : null);
            return;
        }

        if (args.Length >= 1 && args[0] == "--backup-audit")
        {
            Environment.ExitCode = HeadlessSmoke.BackupAudit(args.Length >= 2 ? args[1] : null);
            return;
        }

        if (args.Length >= 2 && args[0] == "--live-save-roundtrip")
        {
            Environment.ExitCode = HeadlessSmoke.LiveSaveRoundTrip(args[1]);
            return;
        }

        // Read-only FC26 name diagnostic. Unlike --smoke, this never stages or saves an edit.
        if (args.Length >= 2 && args[0] == "--name-probe")
        {
            Environment.ExitCode = HeadlessSmoke.NameProbe(args[1]);
            return;
        }

        // Player-name binding tests (read-only; real names from local source, honest fallback).
        if (args.Length >= 2 && args[0] == "--name-tests")
        {
            Environment.ExitCode = HeadlessSmoke.NameTests(args[1]);
            return;
        }

        // Texture preview self-test: "--texture-test <imageFile>"
        if (args.Length >= 2 && args[0] == "--texture-test")
        {
            Environment.ExitCode = HeadlessSmoke.TextureTest(args[1]);
            return;
        }

        // DDS decode accuracy check vs a reference PNG: "--dds-verify <ddsFile> <referencePng>"
        if (args.Length >= 3 && args[0] == "--dds-verify")
        {
            Environment.ExitCode = HeadlessSmoke.DdsVerify(args[1], args[2]);
            return;
        }

        // Asset catalog resolution test: "--asset-test <dbFolder> <assetRoot>"
        if (args.Length >= 3 && args[0] == "--asset-test")
        {
            Environment.ExitCode = HeadlessSmoke.AssetTest(args[1], args[2]);
            return;
        }

        // Read-only installed-game Frostbite container discovery.
        if (args.Length >= 1 && args[0] == "--frostbite-test")
        {
            Environment.ExitCode = HeadlessSmoke.FrostbiteAssetTest(args.Length >= 2 ? args[1] : null);
            return;
        }

        if (args.Length >= 1 && args[0] == "--frostbite-kit-test")
        {
            Environment.ExitCode = HeadlessSmoke.FrostbiteKitPreviewTest(
                args.Length >= 2 ? args[1] : null);
            return;
        }

        // FC26 MeshSet parse + ASCII FBX export, read-only.
        if (args.Length >= 1 && args[0] == "--mesh-export-test")
        {
            Environment.ExitCode = HeadlessSmoke.MeshExportTest(
                args.Length >= 2 ? args[1] : null,
                args.Length >= 3 ? args[2] : null);
            return;
        }

        // FC26 MeshSet query→export resolution, read-only (the UI 3D-viewer path).
        if (args.Length >= 1 && args[0] == "--mesh-query-test")
        {
            Environment.ExitCode = HeadlessSmoke.MeshQueryExportTest(
                args.Length >= 2 ? args[1] : null,
                args.Length >= 3 ? args[2] : null);
            return;
        }

        // Player-list performance test: "--perf <dbFolder>"
        if (args.Length >= 2 && args[0] == "--perf")
        {
            Environment.ExitCode = HeadlessSmoke.PerfTest(args[1]);
            return;
        }

        // All-section navigation test: "--nav-test <dbFolder> <assetRoot-or-empty>"
        if (args.Length >= 2 && args[0] == "--nav-test")
        {
            Environment.ExitCode = HeadlessSmoke.NavTest(args[1], args.Length >= 3 ? args[2] : "");
            return;
        }

        // Layout robustness test: "--layout-test <dbFolder> <assetRoot>"
        if (args.Length >= 2 && args[0] == "--layout-test")
        {
            Environment.ExitCode = HeadlessSmoke.LayoutTest(args[1], args.Length >= 3 ? args[2] : "");
            return;
        }

        // Per-label truncation audit: "--label-audit <dbFolder>"
        if (args.Length >= 2 && args[0] == "--label-audit")
        {
            Environment.ExitCode = HeadlessSmoke.LabelAudit(args[1]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--formation-test")
        {
            Environment.ExitCode = HeadlessSmoke.FormationTest(args[1]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--formation-dump")
        {
            Environment.ExitCode = HeadlessSmoke.FormationDump(args[1], args[2]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--roster-test")
        {
            Environment.ExitCode = HeadlessSmoke.RosterTest(args[1]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--audio-mapping-test")
        {
            Environment.ExitCode = HeadlessSmoke.AudioMappingTest(args[1]);
            return;
        }

        if (args.Length >= 1 && args[0] == "--newwave-test")
        {
            Environment.ExitCode = HeadlessSmoke.NewWaveAudioTest(
                args.Length >= 2 ? args[1] : null);
            return;
        }

        // Read-only schema/value diagnostic for an FC26 table.
        if (args.Length >= 3 && args[0] == "--table-probe")
        {
            Environment.ExitCode = HeadlessSmoke.TableProbe(args[1], args[2]);
            return;
        }

        // Reproduces the Add-New-Team pipeline on a DB copy to verify where the
        // duplicated record lands (template-inheritance regression probe).
        if (args.Length >= 2 && args[0] == "--create-team-probe")
        {
            Environment.ExitCode = HeadlessSmoke.CreateTeamProbe(args[1]);
            return;
        }

        // Runs the full create-team + 23-player squad pipeline on a DB copy,
        // then saves through the native engine and reloads the written files
        // (position-code regression probe for "Integer value required").
        if (args.Length >= 2 && args[0] == "--squad-probe")
        {
            Environment.ExitCode = HeadlessSmoke.SquadProbe(args[1]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--malaysia-super-league-audit")
        {
            Environment.ExitCode = HeadlessSmoke.MalaysiaSuperLeagueAudit(args[1]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--malaysia-super-league-probe")
        {
            Environment.ExitCode = HeadlessSmoke.MalaysiaSuperLeagueProbe(args[1]);
            return;
        }

        // Diagnoses the Bayern-crest-on-new-team screenshot against the installed game.
        if (args.Length >= 1 && args[0] == "--crest-probe")
        {
            Environment.ExitCode = HeadlessSmoke.CrestProbe(args.Length >= 2 ? args[1] : null);
            return;
        }

        // Verifies compdata TXT extraction from the CAS-packed FC26 game files.
        if (args.Length >= 1 && args[0] == "--compdata-cas-probe")
        {
            Environment.ExitCode = HeadlessSmoke.CompdataCasProbe(args.Length >= 2 ? args[1] : null);
            return;
        }

        // Scans the CORE/ChunkFiles collector manifests for compdata path hashes.
        if (args.Length >= 1 && args[0] == "--compdata-manifest-probe")
        {
            Environment.ExitCode = HeadlessSmoke.CompdataManifestProbe(args.Length >= 2 ? args[1] : null);
            return;
        }

        if (args.Length >= 2 && args[0] == "--transfermarkt-parser-test")
        {
            Environment.ExitCode = HeadlessSmoke.TransfermarktParserTest(args[1]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--compdata-test")
        {
            Environment.ExitCode = HeadlessSmoke.CompdataTest(args[1]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--compdata-builder-test")
        {
            Environment.ExitCode = HeadlessSmoke.CompdataBuilderTest(args[1]);
            return;
        }

        WinApp.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ApplicationConfiguration.Initialize();
        WinApp.SetDefaultFont(new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular));

        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
        Log("=== Creation Master 26 starting ===");

        // Apply the saved UI language (defaults to the OS UI culture).
        Localization.SetCulture(SettingsService.Language);

        // Apply the saved visual theme (dark default) before any window is created.
        CM26.App.Theming.Theme.IsDark = SettingsService.DarkMode;

        // First-run End User License Agreement. If the user declines, do not continue.
        if (!SettingsService.EulaAccepted)
        {
            var accepted = CM26.App.Controls.EulaDialog.Show(null);
            if (!accepted)
            {
                Log("User declined the EULA; exiting.");
                return;
            }
            SettingsService.EulaAccepted = true;
        }

        WinApp.ThreadException += (_, e) => HandleFatal("UI thread", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            HandleFatal("background", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log("Unobserved task exception: " + e.Exception);
            e.SetObserved();
        };

        try
        {
            // A normal folder argument starts the exact portable build with the
            // selected database already loaded, avoiding stale-shortcut confusion.
            var initialDatabaseFolder = args.Length == 1 && Directory.Exists(args[0]) ? args[0] : null;
            WinApp.Run(new MainForm(initialDatabaseFolder));
        }
        catch (Exception ex)
        {
            HandleFatal("startup", ex);
        }
        Log("=== Creation Master 26 exited ===");
    }

    private static void HandleFatal(string context, Exception? ex)
    {
        Log($"FATAL ({context}): {ex}");
        try
        {
            MessageBox.Show(
                $"An unexpected error occurred ({context}).\n\n{ex?.Message}\n\nDetails were written to:\n{LogPath}",
                "Creation Master 26", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch { /* never crash the handler */ }
    }

    public static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch { Debug.WriteLine(message); }
    }
}
