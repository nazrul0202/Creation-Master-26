using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Sections;
using CM26.App.Theming;

using CM26.Application.Services;

namespace CM26.App;

public sealed class MainForm : Form
{
    private readonly AppServices _services = new();

    private readonly Panel _workspace;
    private readonly MenuStrip _menu;
    private readonly StudioToolbar _toolbar;
    private readonly StudioSidebar _sidebar;
    private readonly StatusStrip _status;
    private readonly ToolStripStatusLabel _statusText, _dbPath, _assetStatus, _pendingLabel;

    private readonly Dictionary<string, SectionBase> _sections = new();
    private readonly ToolTip _toolTip = new() { AutoPopDelay = 8000, InitialDelay = 400, ReshowDelay = 100 };
    private readonly WelcomePanel _welcome;
    private string? _activeKey;

    // Section registry (key, title, factory). Editors are created lazily.
    private readonly List<(string key, string title, Func<AppServices, SectionBase> factory)> _registry;

    public MainForm(string? initialDatabaseFolder = null)
    {
        Text = "Creation Master 26  |  FC26 Database Studio";
        MinimumSize = new Size(1180, 700);
        Size = new Size(1600, 940);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        KeyPreview = true;
        try { Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "Assets", "Logo", "Creation Master 26.ico")); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Icon load failed: {ex.Message}"); /* icon optional at runtime */ }

        // Best-effort immersive dark/light mode for the window chrome + scrollbars.
        HandleCreated += (_, _) =>
        {
            if (IsHandleCreated)
                NativeTheme.TryApplyImmersiveMode(Handle);
        };

        _registry = BuildRegistry();
        _services.NavigationRequested += NavigateTo;
        _services.RecordNavigationRequested += NavigateToRecord;
        _services.OpenGameRequested += async () =>
        {
            try { await OpenFc26Async(); }
            catch (Exception ex) { Debug.WriteLine($"[CM26] OpenFc26Async failed: {ex.Message}"); }
        };
        _services.SaveDraftRequested += async () =>
        {
            try { await SaveDirectAsync(); }
            catch (Exception ex) { Debug.WriteLine($"[CM26] SaveDirectAsync failed: {ex.Message}"); }
        };
        _services.ScraperSquadImportRequested += ImportScraperSquad;

        // ---- CM16-style application menu ----
        _menu = new MenuStrip { Dock = DockStyle.Top, BackColor = Theme.Background, ForeColor = Theme.Text, Font = Theme.Body, Renderer = new DarkToolStripRenderer() };
        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add("Open Game", null, (_, _) => SafeFire(OpenFc26Async));
        fileMenu.DropDownItems.Add("Save", null, (_, _) => SafeFire(SaveDirectAsync));
        fileMenu.DropDownItems.Add("Save Draft for FIFA Mod...", null, (_, _) => SafeFire(SaveAsync));
        fileMenu.DropDownItems.Add("Export FIFA Mod (.fifamod)...", null, (_, _) => SafeFire(ExportModAsync));
        fileMenu.DropDownItems.Add("Restore Original Data…", null, (_, _) => SafeFire(RestoreOriginalAsync));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Exit", null, (_, _) => Close());
        var toolsMenu = new ToolStripMenuItem("Tools");
        toolsMenu.DropDownItems.Add("Database Browser", null, (_, _) => NavigateTo("browser"));
        toolsMenu.DropDownItems.Add("Diagnostics", null, (_, _) => NavigateTo("diagnostics"));
        toolsMenu.DropDownItems.Add(new ToolStripSeparator());
        toolsMenu.DropDownItems.Add("Undo Last Complete Operation", null, (_, _) => UndoCompleteOperation());
        toolsMenu.DropDownItems.Add("Redo Last Complete Operation", null, (_, _) => RedoCompleteOperation());
        var patchMenu = new ToolStripMenuItem("Patch");
        patchMenu.DropDownItems.Add("Validate staged changes", null, (_, _) => ValidateAll());
        var helpMenu = new ToolStripMenuItem("Help");
        helpMenu.DropDownItems.Add("Settings", null, (_, _) => NavigateTo("settings"));
        helpMenu.DropDownItems.Add("Check for Updates…", null, async (_, _) => await CheckForUpdatesAsync());
        helpMenu.DropDownItems.Add("Keyboard Shortcuts…", null, (_, _) => ShowShortcuts());
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add("Discord Support", null, (_, _) =>
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://discord.gg/T75DFSuSU") { UseShellExecute = true }); }
            catch (Exception ex) { Program.Log($"[CM26] Could not open Discord link: {ex.Message}"); /* cannot open browser */ }
        });
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add("About", null, (_, _) => ShowAbout());
        _menu.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, patchMenu, helpMenu });

        // ---- Studio toolbar ----
        _toolbar = new StudioToolbar();
        _toolbar.OpenClicked += async (_, _) => await OpenFc26Async();
        _toolbar.SaveClicked += async (_, _) => await SaveDirectAsync();
        _toolbar.UndoClicked += (_, _) => Undo();
        _toolbar.RedoClicked += (_, _) => Redo();
        _toolbar.ValidateClicked += (_, _) => ValidateAll();
        _toolbar.SearchClicked += (_, _) => ApplyGlobalFilter();
        _toolbar.PreviousClicked += (_, _) => StepRecord(-1);
        _toolbar.NextClicked += (_, _) => StepRecord(+1);
        _toolbar.NewClicked += (_, _) =>
        {
            if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s))
                s.CreateRecord();
        };
        _toolbar.FilterClicked += (_, _) =>
        {
            if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s))
                s.FocusSearchBox();
        };
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyGlobalFilter();
            }
        };

        // ---- Studio sidebar ----
        _sidebar = new StudioSidebar { Width = Theme.SidebarWidth };
        _sidebar.AddGroup(string.Empty, new[]
        {
            new StudioSidebarItemModel("dashboard", "Dashboard", IconService.Get("dashboard", 18), "Ctrl+1"),
        });
        _sidebar.AddGroup("World", new[]
        {
            new StudioSidebarItemModel("countries", "Countries", IconService.Get("countries", 18), "Ctrl+2"),
            new StudioSidebarItemModel("leagues", "Leagues", IconService.Get("leagues", 18), "Ctrl+3"),
            new StudioSidebarItemModel("teams", "Teams", IconService.Get("teams", 18), "Ctrl+4"),
            new StudioSidebarItemModel("players", "Players", IconService.Get("players", 18), "Ctrl+5"),
            new StudioSidebarItemModel("managers", "Managers", IconService.Get("managers", 18), "Ctrl+6"),
        });
        _sidebar.AddGroup("Venue", new[]
        {
            new StudioSidebarItemModel("stadiums", "Stadiums", IconService.Get("stadiums", 18)),
            new StudioSidebarItemModel("stadiumaudio", "Stadium Audio", IconService.Get("stadiumaudio", 18)),
        });
        _sidebar.AddGroup("Team", new[]
        {
            new StudioSidebarItemModel("kits", "Kits", IconService.Get("kits", 18), "Ctrl+8"),
            new StudioSidebarItemModel("competitions", "Competitions", IconService.Get("competitions", 18), "Ctrl+9"),
            new StudioSidebarItemModel("formations", "Formations", IconService.Get("formations", 18)),
            new StudioSidebarItemModel("balls", "Balls", IconService.Get("balls", 18)),
            new StudioSidebarItemModel("boots", "Boots", IconService.Get("boots", 18)),
            new StudioSidebarItemModel("gloves", "Gloves", IconService.Get("gloves", 18)),
        });
        _sidebar.AddGroup("Media", new[]
        {
            new StudioSidebarItemModel("sponsors", "Sponsors", IconService.Get("sponsors", 18)),
            new StudioSidebarItemModel("adboards", "Adboards", IconService.Get("adboards", 18)),
            new StudioSidebarItemModel("scoreboard", "Broadcast", IconService.Get("scoreboard", 18)),
            new StudioSidebarItemModel("audio", "Audio", IconService.Get("audio", 18)),
        });
        _sidebar.AddGroup("Tools", new[]
        {
            new StudioSidebarItemModel("transfers", "Data Sync", IconService.Get("transfers", 18)),
            new StudioSidebarItemModel("modmanager", "Mod Manager", IconService.Get("modmanager", 18)),
            new StudioSidebarItemModel("referees", "Referees", IconService.Get("referees", 18)),
            new StudioSidebarItemModel("browser", "Database Browser", IconService.Get("browser", 18)),
            new StudioSidebarItemModel("diagnostics", "Diagnostics", IconService.Get("diagnostics", 18)),
        });
        _sidebar.AddGroup("System", new[]
        {
            new StudioSidebarItemModel("settings", "Settings", IconService.Get("settings", 18)),
        });
        _sidebar.ItemClicked += (_, e) => NavigateTo(e.Key);

        // ---- Status bar ----
        _status = new StatusStrip { BackColor = Theme.Panel, ForeColor = Theme.Muted, SizingGrip = true, Renderer = new DarkToolStripRenderer() };
        _statusText = new ToolStripStatusLabel("Ready — open game data") { ForeColor = Theme.Muted, Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        _pendingLabel = new ToolStripStatusLabel("") { ForeColor = Theme.Warning, Font = Theme.BodyBold };
        _assetStatus = new ToolStripStatusLabel("Assets: not loaded") { ForeColor = Theme.Muted };
        _dbPath = new ToolStripStatusLabel("") { ForeColor = Theme.Muted };
        _status.Items.Add(_statusText);
        _status.Items.Add(_pendingLabel);
        _status.Items.Add(_assetStatus);
        _status.Items.Add(_dbPath);

        // ---- Workspace ----
        _workspace = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(0) };
        _welcome = new WelcomePanel { Dock = DockStyle.Fill };
        _welcome.OpenRequested += async (_, _) => await OpenFc26Async();
        _welcome.FolderRequested += async (_, folder) => await LoadDatabaseFolderAsync(folder);
        ShowEmptyWorkspace();

        Controls.Add(_workspace);
        Controls.Add(_sidebar);
        Controls.Add(_toolbar);
        Controls.Add(_status);
        Controls.Add(_menu);

        _services.PendingChanged += (_, _) => RefreshPendingState();
        _services.DatabaseLoaded += (_, _) => OnDatabaseLoaded();
        _services.FrostbiteAssetsReady += (_, _) => OnFrostbiteAssetsReady();
        _services.ThemeChanged += (_, _) => ApplyThemeMode();

        if (!string.IsNullOrWhiteSpace(initialDatabaseFolder))
            Shown += async (_, _) => await LoadDatabaseFolderAsync(initialDatabaseFolder);

        RefreshPendingState();
    }

    private List<(string, string, Func<AppServices, SectionBase>)> BuildRegistry() => new()
    {
        ("dashboard", "Dashboard", s => new DashboardSection(s)),
        ("countries", "Countries", s => new CountriesSection(s)),
        ("leagues", "Leagues", s => new LeaguesSection(s)),
        ("teams", "Teams", s => new TeamsSection(s)),
        ("players", "Players", s => new PlayersSection(s)),
        ("managers", "Managers", s => new ManagersSection(s)),
        ("stadiums", "Stadiums", s => new StadiumsSection(s)),
        ("stadiumaudio", "Stadium Audio", s => new StadiumAudioSection(s)),
        ("kits", "Kits", s => new KitsSection(s)),
        ("competitions", "Competitions", s => new CompetitionsSection(s)),
        ("formations", "Formations", s => new FormationsSection(s)),
        ("transfers", "Data Sync", s => new TransfersSection(s)),
        ("modmanager", "CM26 Mod Manager", s => new ModManagerSection(s)),
        ("balls", "Balls", s => new BallsSection(s)),
        ("boots", "Boots", s => new BootsSection(s)),
        ("gloves", "Gloves", s => new GlovesSection(s)),
        ("sponsors", "Sponsors", s => new SponsorsSection(s)),
        ("adboards", "Adboards", s => new AdboardsSection(s)),
        ("audio", "Audio", s => new AudioNationSection(s)),
        ("scoreboard", "Broadcast Links", s => new TvSection(s)),
        ("referees", "Referees", s => new RefereesSection(s)),
        ("browser", "Database Browser", s => new DatabaseBrowserSection(s)),
        ("diagnostics", "Diagnostics", s => new DiagnosticsSection(s)),
        ("settings", "Settings", s => new SettingsSection(s)),
    };

    /// <summary>Applies the global filter box text to the active section's record search.</summary>
    private void ApplyGlobalFilter()
    {
        if (_activeKey == null || !_sections.TryGetValue(_activeKey, out var section))
        {
            SetStatus("Open a section first to filter its records.");
            return;
        }
        var query = _toolbar.SearchText.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            section.FocusSearchBox();
            return;
        }
        section.ApplyRecordFilter(query);
        SetStatus($"Filtering {section.SectionTitle} records for “{query}”.");
    }

    /// <summary>Moves the active section to the previous/next record.</summary>
    private void StepRecord(int delta)
    {
        if (_activeKey == null || !_sections.TryGetValue(_activeKey, out var section)) return;
        var index = section.CurrentRowIndex;
        if (index < 0) index = 0;
        section.GoToRecord(index + delta);
    }

    public void NavigateTo(string key)
    {
        if (!_services.Session.IsLoaded && key is not ("settings" or "dashboard"))
        {
            SetStatus("Open game data first (Ctrl+O).");
            return;
        }
        // Warn when leaving a section with staged changes that have not been saved,
        // unless the target is the same section (harmless re-selection).
        if (_activeKey != null && !_activeKey.Equals(key, StringComparison.OrdinalIgnoreCase) &&
            (_services.Pending.HasChanges || _services.LegacyMods.HasChanges))
        {
            var count = _services.Pending.Count + _services.LegacyMods.Count;
            var proceed = MessageBox.Show(this,
                $"You have {count} unsaved change(s) staged. " +
                "They remain staged (not lost) when you switch sections.\n\n" +
                $"Switch from {_activeKey} to {key} and continue editing?",
                "Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (proceed != DialogResult.Yes) return;
        }
        if (!_sections.TryGetValue(key, out var section))
        {
            var reg = _registry.Find(r => r.key == key);
            if (reg.factory == null) return;
            section = reg.factory(_services);
            section.Dock = DockStyle.Fill;
            Theme.ApplyControlTree(section);
            _sections[key] = section;
        }

        _workspace.SuspendLayout();
        _workspace.Controls.Clear();
        _workspace.Controls.Add(section);
        _workspace.ResumeLayout();

        _activeKey = key;
        _sidebar.SetActive(key);
        section.ActivateSection();
        SetStatus($"{section.SectionTitle} — {_services.Session.Tables.Count} tables loaded.");
    }

    private void NavigateToRecord(string key, int recordIndex)
    {
        NavigateTo(key);
        if (_sections.TryGetValue(key, out var section) && recordIndex >= 0)
            section.GoToRecord(recordIndex);
    }

    private void ImportScraperSquad(int teamId, string workbookPath)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ImportScraperSquad(teamId, workbookPath));
            return;
        }
        NavigateTo("teams");
        if (_sections.TryGetValue("teams", out var section) && section is TeamsSection teams)
            teams.ImportScraperSquadFromDataSync(teamId, workbookPath);
    }

    private void ShowEmptyWorkspace()
    {
        _workspace.Controls.Clear();
        _workspace.Controls.Add(_welcome);
    }

    private static async void SafeFire(Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) { Debug.WriteLine($"[CM26] Menu action failed: {ex.Message}"); }
    }

    private async Task OpenFc26Async()
    {
        var configuredRoot = SettingsService.FC26GameFolder;
        var gameRoot = Directory.Exists(configuredRoot)
            ? Path.GetFullPath(configuredRoot)
            : FrostbiteAssetSession.ResolveGameRoot(configuredRoot);
        var backup = GameBackupService.Inspect(gameRoot);
        if (backup.IsReady)
        {
            var baseline = GameBackupService.InspectLiveBaseline(gameRoot);
            if (!baseline.IsMatch)
            {
                await RunBaselineUpdateWizardAsync(backup, baseline.Message);
                return;
            }
        }
        SetBusy(true, "Preparing game data…");
        try
        {
            var phase = new Progress<string>(SetStatus);
            var backupProgress = new Progress<GameBackupService.RestoreProgress>(item =>
            {
                var percent = item.TotalBytes > 0
                    ? (int)Math.Clamp(item.CompletedBytes * 100 / item.TotalBytes, 0, 100)
                    : item.Total <= 0 ? 0 : item.Completed * 100 / item.Total;
                var progress = item.TotalBytes > 0
                    ? $"{FormatBytes(item.CompletedBytes)} / {FormatBytes(item.TotalBytes)}"
                    : $"{item.Completed}/{item.Total} files";
                SetStatus($"{item.Phase}: {percent}% · {progress} · {item.CurrentFile}");
            });
            var workspace = await Task.Run(() => _services.OpenFc26(backupProgress, phase));
            NavigateTo("dashboard");
            SetStatus($"Data/Patch ready - {_services.Session.Tables.Count} tables - {workspace.GameRoot}");
        }
        catch (Exception ex)
        {
            Program.Log("Open game failed: " + ex);
            MessageBox.Show(this, ex.Message, "Open Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Failed to open game data.");
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async Task RunBaselineUpdateWizardAsync(GameBackupService.BackupStatus backup, string detail)
    {
        var choice = MessageBox.Show(this,
            "FC26 needs a baseline update before CM26 can edit it.\n\n" + detail + "\n\n" +
            "1. Disable/restore FET mods.\n2. Launch FC26 without mods and reach the main menu.\n" +
            "3. Exit FC26, then click Yes to create a fresh CM26 baseline.\n\n" +
            "Yes = I have completed the vanilla launch\nNo = launch FC26 now\nCancel = do nothing",
            "CM26 Game Data Update", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        if (choice == DialogResult.Cancel) return;
if (choice == DialogResult.No)
        {
            var launch = CM26ModLaunchService.Launch(backup.GameRoot);
            SetStatus(launch.Message);
            if (!launch.Success)
                MessageBox.Show(this, launch.Message + "\n\nLaunch it from Steam/EA App without mods, then select Open Game again.",
                    "Launch FC26", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetBusy(true, "Refreshing FC26 baseline...");
        try
        {
            var progress = new Progress<GameBackupService.RestoreProgress>(item =>
            {
                var percent = item.TotalBytes > 0
                    ? (int)Math.Clamp(item.CompletedBytes * 100 / item.TotalBytes, 0, 100)
                    : item.Total <= 0 ? 0 : item.Completed * 100 / item.Total;
                SetStatus($"{item.Phase}: {percent}% - {item.CurrentFile}");
            });
            var refreshed = await Task.Run(() =>
                GameBackupService.RefreshAfterVanillaLaunch(backup.GameRoot, progress));
            if (!refreshed.Success)
            {
                MessageBox.Show(this, refreshed.Message, "CM26 Game Data Update",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Baseline refresh failed.");
                return;
            }
            MessageBox.Show(this, refreshed.Message + "\n\nCM26 will now load the updated game data.",
                "CM26 Game Data Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            SetBusy(false, null);
        }
        await OpenFc26Async();
    }

    private async Task LoadDatabaseFolderAsync(string folder)
    {
        SetBusy(true, "Loading database and installed assets (the first asset index may take about a minute)...");
        try
        {
            await Task.Run(() => _services.LoadDatabase(folder));
            SettingsService.LastFolder = folder;
            SettingsService.PushRecentFolder(folder);
            SetStatus($"Loaded {Path.GetFileName(folder)} - {_services.Session.Tables.Count} tables.");
            NavigateTo("dashboard");
        }
        catch (Exception ex)
        {
            Program.Log("Open game failed: " + ex);
            MessageBox.Show(this, ex.Message, "Open Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Failed to open game data.");
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async Task RestoreOriginalAsync()
    {
        var configuredRoot = SettingsService.FC26GameFolder;
        var gameRoot = Directory.Exists(configuredRoot)
            ? Path.GetFullPath(configuredRoot)
            : FrostbiteAssetSession.ResolveGameRoot(configuredRoot);
        var backup = GameBackupService.Inspect(gameRoot);
        if (!backup.IsReady)
        {
            MessageBox.Show(this, backup.Message,
                "Restore Original Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmation = MessageBox.Show(this,
            "This restores the original Data and Patch folders from:\n\n" +
            backup.BackupRoot + "\n\n" +
            "Every changed file will be replaced and files not present in CmModData will be removed. " +
            "Close the game first.\n\nContinue?",
            "Restore Original Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirmation != DialogResult.Yes) return;

        SetBusy(true, "Restoring original Data and Patch…");
        try
        {
            var progress = new Progress<GameBackupService.RestoreProgress>(update =>
            {
                SetStatus($"{update.Phase}: {update.Completed}/{update.Total} {update.CurrentFile}");
            });
            var result = await Task.Run(() => GameBackupService.Restore(backup, progress));
            SetStatus(result.Message);
            MessageBox.Show(this, result.Message + "\n\nRestart the game before playing.",
                result.Success ? "Restore complete" : "Restore failed",
                MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private void OnDatabaseLoaded()
    {
        // Database loading runs on a worker thread. All WinForms controls must only
        // be read or changed on the UI thread.
        if (IsDisposed || Disposing) return;
        if (InvokeRequired)
        {
            try { BeginInvoke((Action)OnDatabaseLoaded); }
            catch (InvalidOperationException ex) { System.Diagnostics.Debug.WriteLine($"[CM26] BeginInvoke while form closing: {ex.Message}"); /* form is closing */ }
            return;
        }
        var path = string.IsNullOrWhiteSpace(_services.ActiveGameRoot)
            ? (_services.Session.LoadedFolder ?? string.Empty)
            : _services.ActiveGameRoot;
        _dbPath.Text = ShortenPath(path);
        _dbPath.ToolTipText = path;
        _assetStatus.Text = _services.FrostbiteAssets.IsAvailable
            ? (_services.FrostbiteAssets.UniqueAssetCount > 0
                ? "Assets: indexed" : "Assets: detected")
            : "Assets: warming…";
        _assetStatus.ForeColor = _services.FrostbiteAssets.IsAvailable ? Theme.Success : Theme.Muted;
        _assetStatus.ToolTipText = _services.FrostbiteAssets.Status +
            (string.IsNullOrWhiteSpace(_services.FrostbiteAssets.GameRoot)
                ? string.Empty : Environment.NewLine + _services.FrostbiteAssets.GameRoot);
        // Refresh current section so it shows the new data.
        if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s))
            s.ActivateSection();
    }

    private void OnFrostbiteAssetsReady()
    {
        if (IsDisposed || Disposing) return;
        if (InvokeRequired)
        {
            try { BeginInvoke((Action)OnFrostbiteAssetsReady); } catch (InvalidOperationException ex) { System.Diagnostics.Debug.WriteLine($"[CM26] BeginInvoke while form closing: {ex.Message}"); }
            return;
        }
        _assetStatus.Text = _services.FrostbiteAssets.IsAvailable
            ? "Assets: indexed" : "Assets: unavailable";
        _assetStatus.ForeColor = _services.FrostbiteAssets.IsAvailable ? Theme.Success : Theme.Warning;
        _assetStatus.ToolTipText = _services.FrostbiteAssets.Status;
        if (_activeKey != null && _sections.TryGetValue(_activeKey, out var section)) section.ActivateSection();
    }

    /// <summary>
    /// Saves a CM26-owned draft for the FIFA Mod Manager workflow.  This is
    /// deliberately separate from the retired direct-write implementation
    /// below: clicking Save must never alter an installed FC26 Data/Patch tree.
    /// </summary>
    private async Task SaveAsync()
    {
        if (!_services.Pending.HasChanges && !_services.LegacyMods.HasChanges)
        {
            SetStatus("No staged changes to save.");
            return;
        }
        if (!_services.Pending.HasChanges)
        {
            MessageBox.Show(this,
                "Your asset changes are already staged in the CM26 export draft. Use File > Export FIFA Mod (.fifamod) to create the mod package.",
                "CM26 draft ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var issues = _services.Validation.ValidateAll(_services.Pending.Changes);
        if (issues.Any(issue => issue.IsError))
        {
            MessageBox.Show(this,
                $"There are {issues.Count(issue => issue.IsError)} validation error(s). Resolve them before saving the draft.",
                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetBusy(true, "Saving CM26 export draft...");
        try
        {
            var stagingFolder = Path.Combine(Path.GetTempPath(), "CM26-save-draft-" + Guid.NewGuid().ToString("N"));
            var result = await Task.Run(() => _services.Save.SaveToDirectory(stagingFolder));
            if (!result.Success)
            {
                Program.Log("Draft save failed: " + result.Message);
                MessageBox.Show(this, result.Message, "Draft save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Draft save failed - FC26 Data/Patch was not modified.");
                return;
            }

            _services.LegacyMods.StageDatabase(
                stagingFolder,
                includeLocale: _services.Pending.Changes.Any(change => change.IsLocale));
            _services.Pending.MarkSaved();
            SetStatus("CM26 draft saved. Export FIFA Mod to create a .fifamod package.");
            MessageBox.Show(this,
                "Draft saved safely. FC26 Data/Patch was not changed.\n\nNext: File > Export FIFA Mod (.fifamod).",
                "CM26 draft saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Program.Log("Draft save failed: " + ex);
            MessageBox.Show(this, ex.Message, "Draft save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Draft save failed - FC26 Data/Patch was not modified.");
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    /// <summary>
    /// Applies an explicitly confirmed offline edit to the installed Frostbite
    /// archives.  This remains separate from Save Draft so normal editing never
    /// mutates FC26 by accident.
    /// </summary>
    private async Task SaveDirectAsync()
    {
        if (!_services.Pending.HasChanges && !_services.LegacyMods.HasChanges)
        {
            SetStatus("No staged changes to apply directly.");
            return;
        }
        if (new[] { "FC26", "FC26_Trial", "FC26_Showcase" }.Any(name => Process.GetProcessesByName(name).Length > 0))
        {
            MessageBox.Show(this, "Close FC26 before applying a direct offline edit.", "Direct edit",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var issues = _services.Validation.ValidateAll(_services.Pending.Changes);
        if (issues.Any(issue => issue.IsError))
        {
            MessageBox.Show(this, "Resolve validation errors before applying a direct edit.", "Direct edit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var gameRoot = _services.ActiveGameRoot;
        var backup = GameBackupService.Inspect(gameRoot, verifyContent: true);
        if (!backup.IsReady)
        {
            MessageBox.Show(this, "Direct edit requires a verified CmModData backup.\n\n" + backup.Message,
                "Direct edit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var baseline = GameBackupService.InspectLiveBaseline(gameRoot);
        if (!baseline.IsMatch)
        {
            MessageBox.Show(this, baseline.Message, "Direct edit blocked",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this,
            $"Apply {_services.Pending.Count + _services.LegacyMods.Count} staged change(s) directly to FC26 Data/Patch?\n\n" +
            "Use this only for offline modding. CM26 verifies the original CmModData snapshot, writes archive metadata atomically, and restores the previous TOCs if the transaction fails.\n\n" +
            "The live game files will change; File > Restore Original Data reverses the edit.",
            "Confirm direct offline edit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true, "Preparing direct Frostbite transaction...");
        try
        {
            if (_services.Pending.HasChanges)
            {
                var stagingFolder = Path.Combine(Path.GetTempPath(), "CM26-direct-save-" + Guid.NewGuid().ToString("N"));
                var saved = await Task.Run(() => _services.Save.SaveToDirectory(stagingFolder));
                if (!saved.Success) throw new InvalidOperationException(saved.Message);
                _services.LegacyMods.StageDatabase(
                    stagingFolder,
                    includeLocale: _services.Pending.Changes.Any(change => change.IsLocale));
            }
            if (await ApplyDirectChangesAsync(reloadAfterApply: true, confirmApply: false))
            {
                _services.Pending.MarkSaved();
                SetStatus("Direct FC26 offline edit applied and reloaded.");
            }
        }
        catch (Exception ex)
        {
            Program.Log("Direct save failed: " + ex);
            MessageBox.Show(this, ex.Message, "Direct edit failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Direct edit failed; the live transaction was not completed.");
        }
        finally { SetBusy(false, null); }
    }

    // Direct writes to a live FC26 installation are intentionally retired.
#if false
    [Obsolete("Direct writing to FC26 is retained only for source compatibility and is not exposed by the UI.")]
    private async Task SaveDirectAsyncLegacy()
    {
        if (!_services.Pending.HasChanges && !_services.LegacyMods.HasChanges)
        {
            SetStatus("No changes to save.");
            return;
        }
        if (!_services.Pending.HasChanges)
        {
            await ApplyDirectChangesAsync();
            return;
        }
        var issues = _services.Validation.ValidateAll(_services.Pending.Changes);
        if (issues.Any(i => i.IsError))
        {
            MessageBox.Show(this,
                $"There are {issues.Count(i => i.IsError)} validation error(s). Resolve them before saving.",
                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var summary = string.Join(Environment.NewLine, _services.Pending.Changes.Take(30).Select(c => "  • " + c.Describe()));
        var structuralCount = _services.Pending.Count - _services.Pending.Changes.Count;
        if (structuralCount > 0)
            summary = "  • Structural record/link changes (insert or delete)" + (string.IsNullOrWhiteSpace(summary) ? string.Empty : Environment.NewLine + summary);
        if (_services.Pending.Changes.Count > 30)
            summary += $"{Environment.NewLine}  … and {_services.Pending.Changes.Count - 30} more.";

        var confirm = MessageBox.Show(this,
            $"Save {_services.Pending.Count} change(s) directly to this game installation?\n\n" +
            $"{_services.ActiveGameRoot}\\Data and Patch\n\n" +
            $"CmModData remains the original restore source. The live archives will be written atomically, " +
            $"then extracted and reload-verified.\n\n{summary}",
            "Confirm save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true, "Saving database…");
        try
        {
            var result = await Task.Run(() => _services.Save.SaveToSourceFolder());
            if (result.Success)
            {
                _services.LegacyMods.StageDatabase(
                    _services.Session.LoadedFolder
                    ?? throw new InvalidOperationException("The active database session is unavailable."));
                SetStatus(result.Message);
                if (await ApplyDirectChangesAsync(reloadAfterApply: false, confirmApply: false))
                {
                    _services.Pending.MarkSaved();
                    await ReloadFromLiveFc26Async();
                }
            }
            else
            {
                Program.Log("Save failed: " + result.Message);
                MessageBox.Show(this, result.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Save failed — originals were not modified.");
            }
        }
        finally
        {
            SetBusy(false, null);
        }
    }

#endif
    private async Task ExportProjectAsync()
    {
        if (!_services.Pending.HasChanges && !_services.LegacyMods.HasChanges)
        {
            MessageBox.Show(this, "There are no staged changes to save in a CM26 project.", "Export CM26 Project",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var issues = _services.Validation.ValidateAll(_services.Pending.Changes);
        if (issues.Any(issue => issue.IsError))
        {
            MessageBox.Show(this, "Resolve validation errors before exporting a project.", "Export CM26 Project",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Title = "Export CM26 Project",
            Filter = "CM26 FIFA Project (*.fifaproject)|*.fifaproject",
            DefaultExt = ".fifaproject",
            AddExtension = true,
            FileName = "CM26-Project-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".fifaproject",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        SetBusy(true, "Saving editable CM26 project...");
        try
        {
            if (_services.Pending.HasChanges)
            {
                var stagingFolder = Path.Combine(Path.GetTempPath(), "CM26-project-export-" + Guid.NewGuid().ToString("N"));
                var saved = await Task.Run(() => _services.Save.SaveToDirectory(stagingFolder));
                if (!saved.Success) throw new InvalidOperationException(saved.Message);
                _services.LegacyMods.StageDatabase(
                    stagingFolder,
                    includeLocale: _services.Pending.Changes.Any(change => change.IsLocale));
            }

            var project = await Task.Run(() => CM26ModPackageService.ExportProject(
                dialog.FileName,
                Path.GetFileNameWithoutExtension(dialog.FileName),
                _services.LegacyMods.GetModPayloads()
                    .Select(item => new CM26ModPackageService.Payload(item.LegacyPath, item.SourcePath))));
            _services.Pending.MarkSaved();
            SetStatus($"CM26 project exported: {project.Payloads.Length} payload(s).");
            MessageBox.Show(this,
                "Editable CM26 project exported. Import this .fifaproject in CM26 to continue editing.\n\n" +
                "To play it, also use File > Export FIFA Mod (.fifamod) and import that file in FIFA Mod Manager.",
                "CM26 project exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Program.Log("CM26 project export failed: " + ex);
            MessageBox.Show(this, ex.Message, "Export CM26 Project", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("CM26 project export failed.");
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async Task ImportProjectAsync()
    {
        if (!_services.Session.IsLoaded || string.IsNullOrWhiteSpace(_services.ActiveGameRoot))
        {
            MessageBox.Show(this, "Open FC26 first. CM26 needs the matching game version and original database metadata before it can load a project.",
                "Import CM26 Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if ((_services.Pending.HasChanges || _services.LegacyMods.HasChanges) &&
            MessageBox.Show(this,
                "Importing a project replaces the current unsaved CM26 draft. Export your current project first if you want to keep it. Continue?",
                "Import CM26 Project", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        using var dialog = new OpenFileDialog
        {
            Title = "Import CM26 Project",
            Filter = "CM26 FIFA Project (*.fifaproject)|*.fifaproject",
            Multiselect = false,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        SetBusy(true, "Loading CM26 project...");
        try
        {
            var loadedFolder = _services.Session.LoadedFolder
                ?? throw new InvalidOperationException("The current FC26 database session is unavailable.");
            var projectFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Creation Master 26", "projects", "import-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectFolder);
            foreach (var file in new[] { "fifa_ng_db-meta.xml", "fifa_ng_db.db", "eng_us.db" })
                File.Copy(Path.Combine(loadedFolder, file), Path.Combine(projectFolder, file), overwrite: true);

            var payloadFolder = Path.Combine(projectFolder, "payload");
            var manifest = CM26ModPackageService.ExtractToDirectory(dialog.FileName, payloadFolder);
            foreach (var payload in manifest.Payloads)
            {
                var source = Path.Combine(payloadFolder, payload.GamePath.Replace('/', Path.DirectorySeparatorChar));
                if (payload.GamePath.Equals("data/db/fifa_ng_db.db", StringComparison.OrdinalIgnoreCase))
                    File.Copy(source, Path.Combine(projectFolder, "fifa_ng_db.db"), overwrite: true);
                else if (payload.GamePath.Equals("data/loc/eng_us.db", StringComparison.OrdinalIgnoreCase))
                    File.Copy(source, Path.Combine(projectFolder, "eng_us.db"), overwrite: true);
            }

            await Task.Run(() => _services.LoadDatabase(projectFolder, _services.ActiveGameRoot));
            foreach (var payload in manifest.Payloads)
            {
                var source = Path.Combine(payloadFolder, payload.GamePath.Replace('/', Path.DirectorySeparatorChar));
                _services.LegacyMods.StageFile(payload.GamePath, source);
            }
            _services.Pending.MarkSaved();
            NavigateTo("dashboard");
            SetStatus($"CM26 project loaded: {manifest.Payloads.Length} payload(s).");
            MessageBox.Show(this,
                "CM26 project loaded safely. Continue editing, then export a new .fifaproject and/or .fifamod when ready.",
                "CM26 project imported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Program.Log("CM26 project import failed: " + ex);
            MessageBox.Show(this,
                "CM26 could not load this project. Only CM26-created .fifaproject files are supported.\n\n" + ex.Message,
                "Import CM26 Project", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("CM26 project import failed.");
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async Task ExportModAsync()
    {
        if (!_services.Pending.HasChanges && !_services.LegacyMods.HasChanges)
        {
            MessageBox.Show(this, "There are no staged changes to export.", "Export FIFA Mod",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var issues = _services.Validation.ValidateAll(_services.Pending.Changes);
        if (issues.Any(issue => issue.IsError))
        {
            MessageBox.Show(this, "Resolve validation errors before exporting a mod.", "Export FIFA Mod",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        // Exporting is read-only for the installed game, but require the game
        // to be closed so the generated package always reflects one stable
        // FC26 layout and cannot race a running launcher/game process.
        if (new[] { "FC26", "FC26_Trial", "FC26_Showcase" }.Any(name => Process.GetProcessesByName(name).Length > 0))
        {
            MessageBox.Show(this, "Close FC26 before exporting a FIFA Mod.", "Export FIFA Mod",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Title = "Export FIFA Mod",
            Filter = "FIFA Mod (*.fifamod)|*.fifamod",
            DefaultExt = ".fifamod",
            AddExtension = true,
            FileName = "CM26-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".fifamod",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        SetBusy(true, "Building FIFA Mod Manager mod...");
        try
        {
            if (_services.Pending.HasChanges)
            {
                var stagingFolder = Path.Combine(Path.GetTempPath(), "CM26-mod-export-" + Guid.NewGuid().ToString("N"));
                var saved = await Task.Run(() => _services.Save.SaveToDirectory(stagingFolder));
                if (!saved.Success) throw new InvalidOperationException(saved.Message);
                _services.LegacyMods.StageDatabase(
                    stagingFolder,
                    includeLocale: _services.Pending.Changes.Any(change => change.IsLocale));
            }
            var plan = _services.LegacyMods.WriteDirectPlan();
            var exported = await Task.Run(() => _services.FrostbiteAssets.ExportFetMod(plan, dialog.FileName));
            if (!exported.Success) throw new InvalidOperationException(exported.Message);
            _services.Pending.MarkSaved();
            _services.LegacyMods.MarkApplied();
            SetStatus(exported.Message);
            MessageBox.Show(this,
                exported.Message + "\n\nThe original FC26 Data/Patch files were not changed. Import this file in FIFA Mod Manager.",
                "FIFA Mod exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Program.Log("FIFA mod export failed: " + ex);
            MessageBox.Show(this, ex.Message, "Export FIFA Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("FIFA mod export failed.");
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async Task<bool> ApplyDirectChangesAsync(
        bool reloadAfterApply = true, bool confirmApply = true)
    {
        if (!_services.LegacyMods.HasChanges)
        {
            MessageBox.Show(this, "No database or asset replacements are staged.",
                "Direct edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        if (confirmApply)
        {
            var confirm = MessageBox.Show(this,
                "Apply the staged database and legacy changes directly to Data/Patch?\n\n" +
                "CmModData remains the original Restore source. CM26 validates all new chunks, " +
                "commits TOCs atomically, and rolls back automatically if the transaction fails.",
                "Confirm direct edit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return false;
        }

        SetBusy(true, "Applying direct Data/Patch transaction…");
        try
        {
            var plan = _services.LegacyMods.WriteDirectPlan();
            var result = await Task.Run(() =>
                _services.FrostbiteAssets.ApplyDirect(plan));
            if (!result.Success)
            {
                MessageBox.Show(this, result.Message, "Direct edit failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            _services.LegacyMods.MarkApplied();
            var reloaded = true;
            if (reloadAfterApply)
                reloaded = await ReloadFromLiveFc26Async();
            SetStatus(reloaded
                ? result.Message
                : result.Message + " Automatic editor reload failed; reopen FC26 data before further editing.");

            MessageBox.Show(this,
                result.Message + (reloaded
                    ? "\n\nThe live Data/Patch archives now contain the edits and Creation Master " +
                      "has reloaded the editor from those live archives."
                    : "\n\nThe live Data/Patch transaction completed, but automatic reload verification failed. " +
                      "The previous editor session is still open; close and reopen FC26 data before further editing. ") +
                "\n\nUse File > Restore Original Data to return to the untouched CmModData snapshot.",
                reloaded ? "Direct edit complete" : "Direct edit applied; reload required",
                MessageBoxButtons.OK, reloaded ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            return true;
        }
        finally { SetBusy(false, null); }
    }

    private async Task<bool> ReloadFromLiveFc26Async()
    {
        SetBusy(true, "Verifying and reloading live Data/Patch…");
        try
        {
            var workspace = await Task.Run(() => _services.ReloadFromLiveFc26());
            SetStatus(
                $"Saved, verified and reloaded directly from {workspace.GameRoot}\\Data and Patch.");
            if (_activeKey != null && _sections.TryGetValue(_activeKey, out var section))
                section.ActivateSection();
            return true;
        }
        catch (Exception ex)
        {
            Program.Log("Live reload verification failed after direct save: " + ex);
            SetStatus("Direct save completed, but reload verification failed. Reopen FC26 data before editing.");
            MessageBox.Show(this,
                "The direct Data/Patch transaction completed, but Creation Master could not reload the " +
                "fresh database payload. The previous editor session has been retained.\n\n" +
                "Close and reopen FC26 data before making more edits. If the game rejects the change, use " +
                "File > Restore Original Data.\n\n" + ex.Message,
                "Reload verification failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }

    private void Undo()
    {
        if (_services.Pending.Undo())
        {
            SetStatus("Undid last change.");
            if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s))
                s.ActivateSection();
        }
        else SetStatus("Nothing to undo.");
    }

    private void UndoCompleteOperation()
    {
        if (_services.Pending.UndoLastOperation())
        {
            SetStatus("Undid the last complete scalar operation.");
            if (_activeKey != null && _sections.TryGetValue(_activeKey, out var section)) section.ActivateSection();
        }
        else SetStatus("No complete scalar operation is available to undo.");
    }

    private void RedoCompleteOperation()
    {
        if (_services.Pending.RedoLastOperation())
        {
            SetStatus("Restored the last complete scalar operation.");
            if (_activeKey != null && _sections.TryGetValue(_activeKey, out var section)) section.ActivateSection();
        }
        else SetStatus("No complete scalar operation is available to redo.");
    }

    private void Redo()
    {
        if (_services.Pending.Redo())
        {
            SetStatus("Redid last change.");
            if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s))
                s.ActivateSection();
        }
        else SetStatus("Nothing to redo.");
    }

    private void ValidateAll()
    {
        var issues = _services.Validation.ValidateAll(_services.Pending.Changes);
        if (issues.Count == 0)
            MessageBox.Show(this, "All staged changes are valid.", "Validate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        else
            MessageBox.Show(this, string.Join(Environment.NewLine, issues.Select(i => $"• {i.Table}[{i.Row}].{i.Field}: {i.Message}")),
                "Validation issues", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void RefreshPendingState()
    {
        var count = _services.Pending.Count + _services.LegacyMods.Count;
        _pendingLabel.Text = count > 0 ? $"● {count} unsaved change(s)" : "";
        _toolbar.SaveButton.Enabled = count > 0;
        _toolbar.UndoButton.Enabled = _services.Pending.CanUndo;
        _toolbar.RedoButton.Enabled = _services.Pending.CanRedo;
    }

    private void ShowAbout()
    {
        AboutDialog.Show(this);
    }

    private void ShowShortcuts()
    {
        var text =
            "Keyboard shortcuts\n\n" +
            "Ctrl+O   Open game data\n" +
            "Ctrl+S   Save staged changes\n" +
            "Ctrl+Z   Undo last change\n" +
            "Ctrl+Y   Redo the last undone change\n" +
            "Ctrl+F   Focus the record search box\n" +
            "F5       Refresh the current section\n\n" +
            "Quick section navigation\n" +
            "Ctrl+1   Dashboard\n" +
            "Ctrl+2   Countries\n" +
            "Ctrl+3   Leagues\n" +
            "Ctrl+4   Teams\n" +
            "Ctrl+5   Players\n" +
            "Ctrl+6   Managers\n" +
            "Ctrl+7   Stadiums\n" +
            "Ctrl+8   Kits\n" +
            "Ctrl+9   Competitions\n";
        MessageBox.Show(this, text, "Keyboard Shortcuts",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task CheckForUpdatesAsync()
    {
        if (UpdateChecker.CheckedRecently)
        {
            SetStatus("Update check already performed recently.");
        }
        else
        {
            SetStatus("Checking for updates…");
            var result = await UpdateChecker.CheckAsync();
            if (result == null)
            {
                MessageBox.Show(this, Localization.T("Update.Failed"), "Check for Updates",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("Could not check for updates.");
            }
            else if (result.IsNewerAvailable)
            {
                var answer = MessageBox.Show(this,
                    $"{Localization.T("Update.Available")}\n\nLatest: v{result.LatestVersion}",
                    "Check for Updates", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (answer == DialogResult.Yes)
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(UpdateChecker.ManifestUrl) { UseShellExecute = true }); }
                    catch (Exception ex) { Program.Log($"[CM26] Could not open update link: {ex.Message}"); /* cannot open browser */ }
                SetStatus($"Update v{result.LatestVersion} available.");
            }
            else
            {
                MessageBox.Show(this, result.Message, "Check for Updates",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("You have the latest version.");
            }
        }
    }

    private void SetBusy(bool busy, string? message)
    {
        _toolbar.Progress.Visible = busy;
        _toolbar.OpenButton.Enabled = !busy;
        _toolbar.SaveButton.Enabled = !busy && (_services.Pending.HasChanges || _services.LegacyMods.HasChanges);
        if (message != null) SetStatus(message);
        UseWaitCursor = busy;
    }

    private void SetStatus(string text) => _statusText.Text = text;

    /// <summary>
    /// Re-applies the palette after the user toggles the theme. Cached sections are
    /// rebuilt (they capture colours at construction), and the active section is shown
    /// again so the new palette is immediately visible.
    /// </summary>
    private void ApplyThemeMode()
    {
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        NativeTheme.TryApplyImmersiveMode(Handle);
        _menu.BackColor = Theme.Background;
        _menu.ForeColor = Theme.Text;
        foreach (ToolStripItem item in _menu.Items)
        {
            item.BackColor = Theme.Background;
            item.ForeColor = Theme.Text;
        }
        _status.BackColor = Theme.Panel;
        _statusText.ForeColor = Theme.Muted;
        _dbPath.ForeColor = Theme.Muted;
        _assetStatus.ForeColor = Theme.Muted;
        _pendingLabel.ForeColor = Theme.Warning;
        _workspace.BackColor = Theme.Background;
        _welcome.BackColor = Theme.Background;
        _welcome.ApplyTheme();
        Theme.ApplyControlTree(_welcome);
        if (_workspace.Controls.Count == 1 && _workspace.Controls[0] == _welcome)
            _welcome.Invalidate();

        // Sections captured palette colours when they were created; drop them so the
        // next navigation rebuilds each with the new theme.
        foreach (var section in _sections.Values)
            section.Dispose();
        _sections.Clear();
        // Re-navigate to the active section (works without a database for
        // settings/dashboard) so the new palette is immediately visible.
        if (_activeKey != null)
            NavigateTo(_activeKey);
    }

    /// <summary>Keeps a long path readable in the status bar by showing only the tail segments.</summary>
    private static string ShortenPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (parts.Count <= 3) return path;
        return "…" + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar, parts.TakeLast(3));
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return unit == 0 ? $"{value} {units[unit]}" : $"{value:N1} {units[unit]}";
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Control | Keys.O: _ = OpenFc26Async(); return true;
            case Keys.Control | Keys.S: _ = SaveDirectAsync(); return true;
            case Keys.Control | Keys.Z: Undo(); return true;
            case Keys.Control | Keys.Y: Redo(); return true;
            case Keys.Control | Keys.F:
                if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s)) s.FocusSearchBox();
                return true;
            case Keys.F5:
                if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s2)) s2.LoadData();
                return true;
            case Keys.Control | Keys.D1: NavigateTo("dashboard"); return true;
            case Keys.Control | Keys.D2: NavigateTo("countries"); return true;
            case Keys.Control | Keys.D3: NavigateTo("leagues"); return true;
            case Keys.Control | Keys.D4: NavigateTo("teams"); return true;
            case Keys.Control | Keys.D5: NavigateTo("players"); return true;
            case Keys.Control | Keys.D6: NavigateTo("managers"); return true;
            case Keys.Control | Keys.D7: NavigateTo("stadiums"); return true;
            case Keys.Control | Keys.D8: NavigateTo("kits"); return true;
            case Keys.Control | Keys.D9: NavigateTo("competitions"); return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_services.Pending.HasChanges || _services.LegacyMods.HasChanges)
        {
            var count = _services.Pending.Count + _services.LegacyMods.Count;
            var r = MessageBox.Show(this,
                $"You have {count} unsaved change(s). Close without saving?",
                "Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) { e.Cancel = true; return; }
        }
        _services.Dispose();
        base.OnFormClosing(e);
    }
}
