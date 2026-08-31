using System.Diagnostics;
using System.Windows.Forms;
using WinApp = System.Windows.Forms.Application;

using CM26.Application.Services;
using CM26.AssetBridge;

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

        // x64 FC26 bridge used by the original x86 CM16 forms. The legacy UI
        // invokes this command for File > Open FC26, then consumes the snapshot.
        if (args.Length >= 2 && args[0] == "--legacy-open")
        {
            Environment.ExitCode = ExportLegacySnapshot(args[1]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-open-root")
        {
            Environment.ExitCode = ExportLegacySnapshotFromRoot(args[1], args[2]);
            return;
        }

        // Lazy asset endpoint used by the original x86 CM16 forms. Assets are
        // extracted by the native x64 Frostbite engine and cached on disk, so
        // the legacy UI can keep using its original Bitmap-based controls.
        if (args.Length >= 2 && args[0] == "--legacy-asset")
        {
            Environment.ExitCode = ExportLegacyAsset(args[1]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-assets-list")
        {
            Environment.ExitCode = ExportLegacyAssets(args[1]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-kit-texture")
        {
            Environment.ExitCode = ExportLegacyKitTexture(args[1], args[2]);
            return;
        }

        if (args.Length >= 5 && args[0] == "--legacy-stage-image")
        {
            Environment.ExitCode = StageLegacyImage(args[1], args[2], args[3], args[4]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-stage-file")
        {
            Environment.ExitCode = StageLegacyFile(args[1], args[2]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-move-asset")
        {
            Environment.ExitCode = MoveLegacyAsset(args[1], args[2]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-remove-asset")
        {
            Environment.ExitCode = RemoveLegacyAsset(args[1]);
            return;
        }

        if (args.Length >= 4 && args[0] == "--legacy-search-assets")
        {
            Environment.ExitCode = SearchLegacyAssets(args[1], args[2], args[3]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-export-texture")
        {
            Environment.ExitCode = ExportIndexedTexture(args[1], args[2]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-compdata-open")
        {
            Environment.ExitCode = OpenLegacyCompdata(args[1], args[2]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-compdata-open-installed")
        {
            Environment.ExitCode = OpenInstalledLegacyCompdata(args[1]);
            return;
        }

        if (args.Length >= 5 && args[0] == "--legacy-compdata-save")
        {
            Environment.ExitCode = SaveLegacyCompdata(args[1], args[2], args[3], args[4]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-compdata-validate")
        {
            Environment.ExitCode = ValidateLegacyCompdata(args[1], args[2]);
            return;
        }

        if (args.Length >= 6 && args[0] == "--legacy-compdata-build")
        {
            Environment.ExitCode = BuildLegacyCompdata(args[1], args[2], args[3], args[4], args[5]);
            return;
        }

        if (args.Length >= 6 && args[0] == "--legacy-compdata-advance")
        {
            Environment.ExitCode = AdvanceLegacyCompdata(args[1], args[2], args[3], args[4], args[5]);
            return;
        }

        if (args.Length >= 8 && args[0] == "--legacy-compdata-career")
        {
            Environment.ExitCode = BuildLegacyCareerCompdata(args[1], args[2], args[3], args[4],
                args[5], args[6], args[7]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-compdata-stage")
        {
            Environment.ExitCode = StageLegacyCompdata(args[1]);
            return;
        }

        if (args.Length >= 3 && args[0] == "--legacy-face-mesh")
        {
            Environment.ExitCode = ExportLegacyFaceMesh(args[1], args[2], args.Length >= 4 ? args[3] : null);
            return;
        }

        if (args.Length >= 4 && args[0] == "--legacy-equipment-preview")
        {
            Environment.ExitCode = ExportLegacyEquipmentPreview(args[1], args[2], args[3]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-scoreboards")
        {
            Environment.ExitCode = ExportLegacyScoreboardList(args[1]);
            return;
        }

        // Opens a user-selected extracted FC26 database/localization folder.
        // Unlike --legacy-open this never touches or requires installed archives.
        if (args.Length >= 3 && args[0] == "--legacy-open-folder")
        {
            Environment.ExitCode = ExportLegacyFolderSnapshot(args[1], args[2]);
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-health-report")
        {
            Environment.ExitCode = WriteLegacyHealthReport(args[1]);
            return;
        }

        if (args.Length >= 1 && args[0] == "--legacy-recover")
        {
            Environment.ExitCode = RecoverLegacyTransactions();
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-save")
        {
            try
            {
                Console.WriteLine(LegacyFc26SaveService.Apply(args[1]));
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
            return;
        }

        if (args.Length >= 2 && args[0] == "--legacy-save-verify")
        {
            try
            {
                Console.WriteLine(LegacyFc26SaveService.Apply(args[1], applyDirect: false));
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
            return;
        }

        // User/CI diagnostics need a console. The --legacy-* commands above are
        // invisible helper processes launched by the CM16 shell with redirected
        // output; allocating a console for them causes a black window to flash on
        // every section/asset request.
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

        if (args.Length >= 1 && args[0] == "--restore-original")
        {
            Environment.ExitCode = HeadlessSmoke.RestoreOriginal(args.Length >= 2 ? args[1] : null);
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

        // FC26 MeshSet name search, read-only: "--mesh-search <gameRoot> <query>"
        if (args.Length >= 3 && args[0] == "--mesh-search")
        {
            Environment.ExitCode = HeadlessSmoke.MeshSearch(args[1], args[2]);
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

        if (args.Length >= 1 && args[0] == "--codec-audit")
        {
            Environment.ExitCode = HeadlessSmoke.CodecAudit(args.Length >= 2 ? args[1] : null);
            return;
        }

        if (args.Length >= 1 && args[0] == "--asset-capability-audit")
        {
            Environment.ExitCode = HeadlessSmoke.AssetCapabilityAudit(args.Length >= 2 ? args[1] : null);
            return;
        }

        if (args.Length >= 1 && args[0] == "--texture-format-audit")
        {
            Environment.ExitCode = HeadlessSmoke.TextureFormatAudit(args.Length >= 2 ? args[1] : null);
            return;
        }

        // Render every editor tab to PNG for human visual QA. The optional
        // game root enables installed Frostbite previews during the capture.
        if (args.Length >= 3 && args[0] == "--visual-audit")
        {
            Environment.ExitCode = HeadlessSmoke.VisualAudit(
                args[1], args[2], args.Length >= 4 ? args[3] : null,
                args.Length >= 5 ? args[4] : null);
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
            Environment.ExitCode = HeadlessSmoke.TableProbe(
                args[1], args[2],
                args.Length >= 5 ? args[3] : null,
                args.Length >= 5 ? args[4] : null,
                args.Length >= 6 && int.TryParse(args[5], out var probeLimit) ? probeLimit : 20);
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

        // The public x64 host owns one editor shell for its full lifetime.
        // Hidden --legacy-* bridge invocations return above and never
        // participate in this UI-only mutex.
        using var singleInstance = new Mutex(true, @"Local\CreationMaster26.UI", out var isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show("Creation Master 26 is already running.", "Creation Master 26",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
        Log("=== Creation Master 26 starting ===");

        // Apply the saved UI language (defaults to the OS UI culture).
        Localization.SetCulture(SettingsService.Language);

        // Deco-inspired light mode is the default; users can retain dark mode in Settings.
        CM26.App.Theming.Theme.IsDark = SettingsService.DarkMode;

        // First-run End User License Agreement. If the user declines, do not continue.
        // Recover a CM26-owned folder swap before any editor/archive session is
        // opened. The marker is written before activation, so this also covers
        // a power loss or a force-closed modded launch.
        var configuredGameRoot = SettingsService.FC26GameFolder;
        if (Directory.Exists(configuredGameRoot))
        {
            var recovery = CM26ModLaunchService.Recover(configuredGameRoot);
            if (!recovery.Success) Log("CM26 mod recovery pending: " + recovery.Message);
            else if (!recovery.Message.StartsWith("Original FC26 Data is already active", StringComparison.Ordinal))
                Log("CM26 mod recovery: " + recovery.Message);
        }

        // Automated public-shell smoke has no interactive desktop for the EULA dialog;
        // normal launches retain the acknowledgement requirement unchanged.
        var isStudioSmoke = args.Length >= 1 && args[0] is "--ui-smoke" or "--ui-shell-smoke";
        if (!SettingsService.EulaAccepted && !isStudioSmoke)
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
            // The responsive x64 Studio is the public interface. It owns the fast,
            // lazy section workspace and direct Frostbite workflow. The original
            // CM16 shell remains available through --classic/--legacy for users who
            // need its compatibility-only surfaces.
            var legacyExe = Path.Combine(AppContext.BaseDirectory, "CM26.LegacyUI", "CM26.LegacyUI.exe");
            var useClassicShell = args.Length >= 1 &&
                (args[0].Equals("--classic", StringComparison.OrdinalIgnoreCase) ||
                 args[0].Equals("--legacy", StringComparison.OrdinalIgnoreCase));
            var useWpfCompatibility = args.Length >= 1 &&
                args[0].Equals("--cm16-studio", StringComparison.OrdinalIgnoreCase);
            if (useClassicShell && !useWpfCompatibility && !isStudioSmoke && File.Exists(legacyExe))
            {
                using var legacy = Process.Start(new ProcessStartInfo
                {
                    FileName = legacyExe,
                    Arguments = $"--cm26-host \"{Environment.ProcessPath}\"",
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(legacyExe)!
                });
                legacy?.WaitForExit();
            }
            else if (useWpfCompatibility)
            {
                var studio = new CM26.Studio.App();
                studio.InitializeForHost();
                Environment.ExitCode = studio.Run();
            }
            else
            {
                var initialDatabaseFolder = args.Length >= 2 &&
                    (args[0].Equals("--studio", StringComparison.OrdinalIgnoreCase) ||
                     args[0].Equals("--database", StringComparison.OrdinalIgnoreCase)) &&
                    Directory.Exists(args[1]) ? args[1] : null;
                using var mainForm = new MainForm(initialDatabaseFolder);
                if (isStudioSmoke)
                {
                    mainForm.Shown += (_, _) =>
                    {
                        Console.WriteLine("SHELL SMOKE OK: feature-complete x64 Studio");
                        mainForm.Close();
                    };
                }
                WinApp.Run(mainForm);
            }
        }
        catch (Exception ex)
        {
            HandleFatal("startup", ex);
        }
        Log("=== Creation Master 26 exited ===");
    }

    private static int ExportLegacySnapshot(string outputPath)
    {
        try
        {
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot))
                throw new InvalidOperationException("FC26 installation was not detected. Configure the game folder first.");
            return ExportLegacySnapshotFromRootCore(gameRoot, outputPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ExportLegacyAsset(string logicalPath)
    {
        try
        {
            var normalized = logicalPath.Replace('\\', '/').TrimStart('/');
            var cached = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Creation Master 26", "legacy-assets-v2",
                normalized.Replace('/', Path.DirectorySeparatorChar));
            var cachedAsset = FindCachedLegacyAsset(cached);
            if (cachedAsset is not null)
            {
                Console.WriteLine(cachedAsset);
                return 0;
            }

            // An explicitly configured extracted asset pack is a useful fast
            // path for previews that are represented as RES rather than legacy
            // ChunkFileCollector entries in FC26.
            var catalogPath = LegacyAssetCatalogFallback.Resolve(SettingsService.AssetRoot, normalized);
            if (!string.IsNullOrWhiteSpace(catalogPath))
            {
                Console.WriteLine(CacheLegacyAsset(catalogPath, cached));
                return 0;
            }

            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot))
                throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var output = LegacyFrostbiteAssetResolver.Resolve(assets, normalized);
            if (string.IsNullOrWhiteSpace(output) || !File.Exists(output))
                throw new FileNotFoundException("FC26 asset was not found.", normalized);
            Console.WriteLine(CacheLegacyAsset(output, cached));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ExportLegacyKitTexture(string teamText, string kitTypeText)
    {
        try
        {
            if (!int.TryParse(teamText, out var teamId) || teamId < 0)
                throw new ArgumentException("Invalid FC26 team id.");
            if (!int.TryParse(kitTypeText, out var kitType))
                throw new ArgumentException("Invalid FC26 kit type.");

            if (!Fc26KitSlot.TryGetAssetVariant(kitType, out var variant))
                throw new ArgumentOutOfRangeException(nameof(kitType), "Only Home, Away, Goalkeeper and Third kit textures use the core FC26 club-kit path.");
            var cached = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Creation Master 26", "legacy-kit-textures-v2", $"{teamId}_{kitType}.png");
            if (File.Exists(cached) && new FileInfo(cached).Length > 0 && IsReadableRaster(cached))
            {
                Console.WriteLine(cached);
                return 0;
            }
			try { if (File.Exists(cached)) File.Delete(cached); } catch { }

            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot))
                throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);

            var selected = assets.SearchAssets($"_{teamId}/{variant}_", "Res", 100)
                .Where(match => match.ResType == 0x6BDE20BA &&
                                match.Name.EndsWith("_color", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(match => LegacyKitTextureScore(match.Name))
                .FirstOrDefault();
            if (selected is null)
                throw new FileNotFoundException($"No FC26 colour texture found for team {teamId} ({variant}).");

            var source = assets.ExportTexture(selected.Name);
            if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
                throw new InvalidOperationException("FC26 kit texture extraction failed.");
            // The classic shell is a 32-bit process. A 2048x2048 ARGB bitmap can
            // exhaust its GDI heap after the database and image lists are loaded.
            // Generate a compact cache in the 64-bit host before WinForms opens it.
            using var preview = new TexturePreviewService().CreatePreview(source, 1024, 1024);
            if (preview is null) throw new InvalidOperationException("FC26 kit texture could not be decoded.");
            Directory.CreateDirectory(Path.GetDirectoryName(cached)!);
            preview.Save(cached, System.Drawing.Imaging.ImageFormat.Png);
            Console.WriteLine(cached);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int LegacyKitTextureScore(string name)
    {
        var score = 0;
        if (name.Contains("/jersey_", StringComparison.OrdinalIgnoreCase)) score += 100;
        if (name.Contains("jersey", StringComparison.OrdinalIgnoreCase)) score += 30;
        if (name.Contains("brand_", StringComparison.OrdinalIgnoreCase)) score -= 60;
        if (name.Contains("crest_", StringComparison.OrdinalIgnoreCase)) score -= 80;
        if (name.Contains("number_", StringComparison.OrdinalIgnoreCase)) score -= 80;
        return score;
    }

    private static int StageLegacyImage(string legacyPath, string sourcePath, string widthText, string heightText)
    {
        try
        {
            if (!int.TryParse(widthText, out var width) || !int.TryParse(heightText, out var height) ||
                width <= 0 || height <= 0) throw new ArgumentException("Invalid texture dimensions.");
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var mods = new LegacyAssetModService();
            mods.Open(assets.Fingerprint);
            Console.WriteLine(mods.StageImage(legacyPath, sourcePath, width, height));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ExportLegacyFaceMesh(string playerText, string headText, string? responsePath)
    {
        try
        {
            if (!int.TryParse(playerText, out var playerId) || playerId <= 0)
                throw new ArgumentException("Invalid FC26 player id.");
            if (!int.TryParse(headText, out var headAssetId) || headAssetId < 0)
                throw new ArgumentException("Invalid FC26 head asset id.");

            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot))
                throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);

            var queries = new[]
                {
                    headAssetId > 0 ? $"head_{headAssetId}_0_0_mesh" : string.Empty,
                    headAssetId > 0 ? $"head_{headAssetId}" : string.Empty,
                    $"head_{playerId}_0_0_mesh",
                    $"head_{playerId}"
                }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var exported = assets.ExportMeshForQuery(queries, 250);
            if (string.IsNullOrWhiteSpace(exported) || !File.Exists(exported))
                throw new FileNotFoundException($"No FC26 Frostbite head mesh was found for player {playerId}.");
            if (!string.IsNullOrWhiteSpace(responsePath))
            {
                var responseDirectory = Path.GetDirectoryName(responsePath);
                if (!string.IsNullOrWhiteSpace(responseDirectory)) Directory.CreateDirectory(responseDirectory);
                File.WriteAllText(responsePath, exported);
            }
            else
            {
                Console.WriteLine(exported);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ExportLegacyEquipmentPreview(string kind, string idText, string responsePath)
    {
        try
        {
            if (!int.TryParse(idText, out var id) || id < 0) throw new ArgumentException("Invalid equipment id.");
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            string[] stems = kind.Equals("ball", StringComparison.OrdinalIgnoreCase)
                ? new[] { $"ball_{id}_mesh", $"ball_{id}" }
                : new[] { $"shoe_{id}_mesh", $"boot_{id}_mesh", $"shoe_{id}", $"boot_{id}" };
            var mesh = assets.ExportMeshForQuery(stems, 300);
            if (string.IsNullOrWhiteSpace(mesh) || !File.Exists(mesh))
                throw new FileNotFoundException($"No indexed {kind} mesh was found for id {id}.");
            string texture = string.Empty;
            var selected = stems.SelectMany(stem => assets.SearchAssets(stem, "Res", 120))
                .Where(match => match.ResType == 0x6BDE20BA && match.Name.Contains("color", StringComparison.OrdinalIgnoreCase))
                .OrderBy(match => match.Name.Length).FirstOrDefault();
            if (selected != null)
            {
                var source = assets.ExportTexture(selected.Name);
                if (!string.IsNullOrWhiteSpace(source) && File.Exists(source))
                {
                    using var preview = new TexturePreviewService().CreatePreview(source, 2048, 2048);
                    if (preview != null)
                    {
                        texture = Path.Combine(Path.GetTempPath(), $"cm26-{kind}-{id}-color.png");
                        preview.Save(texture, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(responsePath)!);
            File.WriteAllLines(responsePath, new[] { mesh, texture });
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int ExportLegacyScoreboardList(string responsePath)
    {
        try
        {
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var rows = new[] { "scoreboard", "score_board", "broadcastoverlay", "matchoverlay" }
                .SelectMany(query => assets.SearchAssets(query, null, 750))
                .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase).Select(group => group.First())
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .Select(item => item.Type + "\t" + item.Name).ToArray();
            Directory.CreateDirectory(Path.GetDirectoryName(responsePath)!);
            File.WriteAllLines(responsePath, rows);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static bool IsReadableRaster(string path)
    {
        try
        {
            using var image = System.Drawing.Image.FromFile(path);
            return image.Width > 0 && image.Height > 0;
        }
        catch { return false; }
    }

    private static int StageLegacyFile(string legacyPath, string sourcePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(legacyPath) || !legacyPath.Replace('\\', '/').StartsWith("data/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("A verified FC26 logical path beginning with data/ is required.");
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var mods = new LegacyAssetModService();
            mods.Open(assets.Fingerprint);
            Console.WriteLine(mods.StageFile(legacyPath, sourcePath));
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int MoveLegacyAsset(string sourcePath, string targetPath)
    {
        try
        {
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var mods = new LegacyAssetModService();
            mods.Open(assets.Fingerprint);
            Console.WriteLine(mods.MoveReplacement(sourcePath, targetPath) ? "Moved" : "No staged source replacement");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int RemoveLegacyAsset(string legacyPath)
    {
        try
        {
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var mods = new LegacyAssetModService();
            mods.Open(assets.Fingerprint);
            Console.WriteLine(mods.Remove(legacyPath) ? "Removed" : "No staged replacement");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int SearchLegacyAssets(string query, string assetType, string responsePath)
    {
        try
        {
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var type = assetType.Equals("All", StringComparison.OrdinalIgnoreCase) ? null : assetType;
            var rows = assets.SearchAssets(query, type, 500)
                .Select(item => string.Join("\t", item.Type, item.Name, item.ResType.ToString("X8"),
                    item.OriginalSize, item.CompressedSize, item.Sha1))
                .ToArray();
            Directory.CreateDirectory(Path.GetDirectoryName(responsePath)!);
            File.WriteAllLines(responsePath, rows);
            Console.WriteLine(rows.Length);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int ExportIndexedTexture(string resourceName, string responsePath)
    {
        try
        {
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var exported = assets.ExportTexture(resourceName);
            if (string.IsNullOrWhiteSpace(exported) || !File.Exists(exported))
                throw new FileNotFoundException("The selected FC26 resource is not a supported texture.");
            Directory.CreateDirectory(Path.GetDirectoryName(responsePath)!);
            File.WriteAllText(responsePath, exported);
            Console.WriteLine(exported);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int OpenLegacyCompdata(string sourcePath, string responsePath)
    {
        try
        {
            var service = new CompdataWorkbookService();
            if (Directory.Exists(sourcePath)) service.OpenFromGameFolder(sourcePath);
            else service.Open(sourcePath);
            var snapshot = new LegacyCompdataSnapshot
            {
                SourcePath = Path.GetFullPath(sourcePath),
                Sheets = service.SheetNames.Select(name => ToLegacySheet(service.ReadSheet(name))).ToList()
            };
            Directory.CreateDirectory(Path.GetDirectoryName(responsePath)!);
            File.WriteAllText(responsePath, System.Text.Json.JsonSerializer.Serialize(snapshot,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = false }));
            Console.WriteLine(snapshot.Sheets.Count);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int OpenInstalledLegacyCompdata(string responsePath)
    {
        string? extracted = null;
        try
        {
            extracted = Fc26CompdataGameService.ExtractInstalled();
            return OpenLegacyCompdata(extracted, responsePath);
        }
        finally
        {
            try { if (!string.IsNullOrWhiteSpace(extracted) && Directory.Exists(extracted)) Directory.Delete(extracted, true); }
            catch (Exception ex) { Log("Installed Compdata cleanup failed: " + ex.Message); }
        }
    }

    private static int SaveLegacyCompdata(string sourcePath, string snapshotPath, string outputPath, string mode)
    {
        try
        {
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<LegacyCompdataSnapshot>(File.ReadAllText(snapshotPath))
                ?? throw new InvalidDataException("Compdata edit snapshot is empty.");
            var tables = snapshot.Sheets.ToDictionary(sheet => sheet.Name, FromLegacySheet, StringComparer.OrdinalIgnoreCase);
            var errors = CompdataSchema.Validate(tables).Where(issue => issue.IsError).ToArray();
            if (errors.Length > 0)
                throw new InvalidDataException("Compdata validation failed: " + string.Join(" | ", errors.Take(8)
                    .Select(issue => $"{issue.Sheet} row {issue.Row}: {issue.Message}")));
            if (mode.Equals("txt", StringComparison.OrdinalIgnoreCase))
            {
                CompdataWorkbookService.ExportTextFiles(outputPath, tables);
            }
            else
            {
                var service = new CompdataWorkbookService();
                service.Open(sourcePath);
                service.SaveCopy(outputPath, tables);
            }
            Console.WriteLine(outputPath);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int ValidateLegacyCompdata(string snapshotPath, string reportPath)
    {
        try
        {
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<LegacyCompdataSnapshot>(File.ReadAllText(snapshotPath))
                ?? throw new InvalidDataException("Compdata edit snapshot is empty.");
            var tables = snapshot.Sheets.ToDictionary(sheet => sheet.Name, FromLegacySheet, StringComparer.OrdinalIgnoreCase);
            var issues = CompdataSchema.Validate(tables);
            var report = issues.Count == 0 ? "Compdata validation passed. No issues found."
                : string.Join(Environment.NewLine, issues.Select(issue =>
                    $"[{(issue.IsError ? "Error" : "Warning")}] {issue.Sheet} row {issue.Row}: {issue.Message}"));
            var reportDirectory = Path.GetDirectoryName(Path.GetFullPath(reportPath));
            if (!string.IsNullOrWhiteSpace(reportDirectory)) Directory.CreateDirectory(reportDirectory);
            File.WriteAllText(reportPath, report);
            Console.WriteLine(issues.Count);
            return issues.Any(issue => issue.IsError) ? 2 : 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int BuildLegacyCompdata(string snapshotPath, string name, string databaseIdText,
        string stagesText, string groupsText)
    {
        try
        {
            if (!int.TryParse(databaseIdText, out var databaseId) || !int.TryParse(stagesText, out var stages) ||
                !int.TryParse(groupsText, out var groups)) throw new ArgumentException("Invalid tournament wizard values.");
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<LegacyCompdataSnapshot>(File.ReadAllText(snapshotPath))
                ?? throw new InvalidDataException("Compdata edit snapshot is empty.");
            var tables = snapshot.Sheets.ToDictionary(sheet => sheet.Name, FromLegacySheet, StringComparer.OrdinalIgnoreCase);
            var result = CompdataBuilder.CreateLeagueOrCup(tables,
                new CompdataLeagueBuildRequest(name, databaseId, stages, groups));
            snapshot.Sheets = tables.Values.Select(ToLegacySheet).ToList();
            File.WriteAllText(snapshotPath, System.Text.Json.JsonSerializer.Serialize(snapshot));
            Console.WriteLine($"Created competition object {result.CompetitionObjectId}, {result.StageIds.Count} stage(s), {result.GroupIds.Count} group(s)");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int AdvanceLegacyCompdata(string snapshotPath, string sourceGroupText, string sourceRankText,
        string destinationGroupText, string destinationRankText)
    {
        try
        {
            if (!int.TryParse(sourceGroupText, out var sourceGroup) || !int.TryParse(sourceRankText, out var sourceRank) ||
                !int.TryParse(destinationGroupText, out var destinationGroup) || !int.TryParse(destinationRankText, out var destinationRank))
                throw new ArgumentException("Invalid advancement values.");
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<LegacyCompdataSnapshot>(File.ReadAllText(snapshotPath))
                ?? throw new InvalidDataException("Compdata edit snapshot is empty.");
            var tables = snapshot.Sheets.ToDictionary(sheet => sheet.Name, FromLegacySheet, StringComparer.OrdinalIgnoreCase);
            CompdataBuilder.AddAdvancement(tables, sourceGroup, sourceRank, destinationGroup, destinationRank);
            snapshot.Sheets = tables.Values.Select(ToLegacySheet).ToList();
            File.WriteAllText(snapshotPath, System.Text.Json.JsonSerializer.Serialize(snapshot));
            Console.WriteLine("Advancement path created");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int BuildLegacyCareerCompdata(string snapshotPath, string countryName, string nationIdText,
        string confederationText, string leagueName, string leagueIdText, string teamIdsText)
    {
        try
        {
            if (!int.TryParse(nationIdText, out var nationId) ||
                !int.TryParse(confederationText, out var confederation) ||
                !int.TryParse(leagueIdText, out var leagueId))
                throw new ArgumentException("Invalid country or league identifiers.");
            var teamIds = teamIdsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => int.TryParse(value, out var id) ? id : -1).Where(id => id > 0).Distinct().ToArray();
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<LegacyCompdataSnapshot>(File.ReadAllText(snapshotPath))
                ?? throw new InvalidDataException("Compdata edit snapshot is empty.");
            var tables = snapshot.Sheets.ToDictionary(sheet => sheet.Name, FromLegacySheet, StringComparer.OrdinalIgnoreCase);
            var result = CompdataBuilder.CreateCountryCareerLeague(tables,
                new CountryCareerBuildRequest(countryName, nationId, confederation, leagueName, leagueId, teamIds));
            var errors = CompdataSchema.Validate(tables).Where(issue => issue.IsError).ToArray();
            if (errors.Length > 0)
                throw new InvalidDataException(string.Join(" | ", errors.Take(8).Select(issue => issue.Message)));
            snapshot.Sheets = tables.Values.Select(ToLegacySheet).ToList();
            File.WriteAllText(snapshotPath, System.Text.Json.JsonSerializer.Serialize(snapshot));
            Console.WriteLine($"Career league object {result.CompetitionObjectId} created with {teamIds.Length} teams and a complete double round-robin calendar.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static int StageLegacyCompdata(string snapshotPath)
    {
        try
        {
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<LegacyCompdataSnapshot>(File.ReadAllText(snapshotPath))
                ?? throw new InvalidDataException("Compdata edit snapshot is empty.");
            var tables = snapshot.Sheets.ToDictionary(sheet => sheet.Name, FromLegacySheet, StringComparer.OrdinalIgnoreCase);
            var count = Fc26CompdataGameService.StageForDirectSave(tables);
            Console.WriteLine(count + " Compdata TXT asset(s) staged for the normal CM26 Save transaction.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    private static LegacyCompdataSheet ToLegacySheet(System.Data.DataTable table) => new()
    {
        Name = table.TableName,
        Columns = table.Columns.Cast<System.Data.DataColumn>().Select(column => column.ColumnName).ToList(),
        Rows = table.Rows.Cast<System.Data.DataRow>().Select(row => table.Columns.Cast<System.Data.DataColumn>()
            .Select(column => Convert.ToString(row[column], System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty).ToList()).ToList()
    };

    private static System.Data.DataTable FromLegacySheet(LegacyCompdataSheet sheet)
    {
        var table = new System.Data.DataTable(sheet.Name);
        foreach (var column in sheet.Columns) table.Columns.Add(column, typeof(string));
        foreach (var source in sheet.Rows)
        {
            var row = table.NewRow();
            for (var index = 0; index < table.Columns.Count && index < source.Count; index++) row[index] = source[index] ?? string.Empty;
            table.Rows.Add(row);
        }
        return table;
    }

    private sealed class LegacyCompdataSnapshot
    {
        public string SourcePath { get; set; } = string.Empty;
        public List<LegacyCompdataSheet> Sheets { get; set; } = [];
    }

    private sealed class LegacyCompdataSheet
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = [];
        public List<List<string>> Rows { get; set; } = [];
    }

    private static int ExportLegacyFolderSnapshot(string folder, string outputPath)
    {
        try
        {
            using var database = new DatabaseSession();
            database.Load(Path.GetFullPath(folder));
            LegacySnapshotService.Write(database, outputPath, gameRoot: string.Empty);
            Console.WriteLine(outputPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ExportLegacySnapshotFromRoot(string gameRoot, string outputPath)
    {
        try
        {
            var resolved = FrostbiteAssetSession.ResolveGameRoot(gameRoot);
            if (string.IsNullOrWhiteSpace(resolved))
                throw new DirectoryNotFoundException("The saved FC26 game source is no longer available: " + gameRoot);
            return ExportLegacySnapshotFromRootCore(resolved, outputPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ExportLegacySnapshotFromRootCore(string gameRoot, string outputPath)
    {
        var assets = new FrostbiteAssetSession();
        assets.Open(gameRoot);
        if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
        var workspace = Fc26WorkspaceService.Open(assets);
        var backup = GameBackupService.EnsureCreated(workspace.GameRoot);
        if (!backup.Success) throw new InvalidOperationException(backup.Message);
        SettingsService.FC26GameFolder = workspace.GameRoot;

        using var database = new DatabaseSession();
        database.Load(workspace.DatabaseFolder);
        LegacySnapshotService.Write(database, outputPath, workspace.GameRoot);
        Console.WriteLine(outputPath);
        return 0;
    }

    private static int WriteLegacyHealthReport(string responsePath)
    {
        try
        {
            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot))
                throw new InvalidOperationException("FC26 installation was not detected.");
            using var database = new DatabaseSession();
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
            var workspace = Fc26WorkspaceService.Open(assets);
            database.Load(workspace.DatabaseFolder);
            var report = DatabaseHealthService.Analyze(database).ToText();
            Directory.CreateDirectory(Path.GetDirectoryName(responsePath)!);
            File.WriteAllText(responsePath, report);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int RecoverLegacyTransactions()
    {
        try
        {
            var result = DirectTransactionRecoveryService.RecoverPending();
            Console.WriteLine(result.ToDisplayText());
            return result.Success ? 0 : 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ExportLegacyAssets(string requestPath)
    {
        try
        {
            if (!File.Exists(requestPath))
                throw new FileNotFoundException("FC26 asset batch request was not found.", requestPath);

            var logicalPaths = File.ReadAllLines(requestPath)
                .Select(path => path.Trim())
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (logicalPaths.Length == 0) return 0;

            var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot))
                throw new InvalidOperationException("FC26 installation was not detected.");
            var assets = new FrostbiteAssetSession();
            assets.Open(gameRoot);
            if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);

            var count = 0;
            foreach (var logicalPath in logicalPaths)
            {
                var normalized = logicalPath.Replace('\\', '/').TrimStart('/');
                var cached = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Creation Master 26", "legacy-assets-v2",
                    normalized.Replace('/', Path.DirectorySeparatorChar));
                if (FindCachedLegacyAsset(cached) is not null)
                {
                    count++;
                    continue;
                }

                var source = LegacyAssetCatalogFallback.Resolve(SettingsService.AssetRoot, normalized);
                if (string.IsNullOrWhiteSpace(source))
                    source = LegacyFrostbiteAssetResolver.Resolve(assets, normalized);
                if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) continue;
                CacheLegacyAsset(source, cached);
                count++;
            }
            Console.WriteLine(count);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static string CacheLegacyAsset(string source, string destination)
    {
        if (Path.GetFullPath(source).Equals(Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            return source;
        // Never rename an extracted PNG/JPEG to DDS/BIG.  The legacy reader
        // selects its decoder from the extension, so doing that produced
        // corrupted previews and seemingly random assets in other sections.
        if (!string.Equals(Path.GetExtension(source), Path.GetExtension(destination), StringComparison.OrdinalIgnoreCase))
            destination = Path.ChangeExtension(destination, Path.GetExtension(source));
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, true);
        return destination;
    }

    private static string? FindCachedLegacyAsset(string requestedPath)
    {
        if (File.Exists(requestedPath) && new FileInfo(requestedPath).Length > 0) return requestedPath;
        var directory = Path.GetDirectoryName(requestedPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return null;
        var stem = Path.GetFileNameWithoutExtension(requestedPath);
        foreach (var extension in new[] { ".png", ".jpg", ".jpeg", ".bmp", ".dds" })
        {
            var candidate = Path.Combine(directory, stem + extension);
            if (File.Exists(candidate) && new FileInfo(candidate).Length > 0) return candidate;
        }
        return null;
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
        catch (Exception handlerEx) { System.Diagnostics.Debug.WriteLine($"[CM26] Fatal message box failed: {handlerEx.Message}"); /* never crash the handler */ }
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
