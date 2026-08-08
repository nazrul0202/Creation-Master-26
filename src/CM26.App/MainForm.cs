using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Sections;
using CM26.App.Theming;

namespace CM26.App;

public sealed class MainForm : Form
{
    private readonly AppServices _services = new();

    private readonly Panel _workspace;
    private readonly MenuStrip _menu;
    private readonly Panel _filterBar;
    private readonly TextBox _filterSearch;
    private readonly Button _goBtn, _prevBtn, _nextBtn;
    private readonly Button _openBtn, _saveBtn, _undoBtn, _redoBtn, _validateBtn;
    private readonly ProgressBar _progress;
    private readonly Panel _sidebar;
    private readonly FlowLayoutPanel _sidebarList;
    private readonly StatusStrip _status;
    private readonly ToolStripStatusLabel _statusText, _dbPath, _assetStatus, _pendingLabel;

    private readonly Dictionary<string, SectionBase> _sections = new();
    private readonly Dictionary<string, SidebarNavButton> _moduleButtons = new(StringComparer.OrdinalIgnoreCase);
    private readonly ToolTip _toolTip = new() { AutoPopDelay = 8000, InitialDelay = 400, ReshowDelay = 100 };
    private readonly WelcomePanel _welcome;
    private string? _activeKey;

    // Section registry (key, title, factory). Editors are created lazily.
    private readonly List<(string key, string title, Func<AppServices, SectionBase> factory)> _registry;

    public MainForm(string? initialDatabaseFolder = null)
    {
        Text = "Creation Master 26";
        MinimumSize = new Size(1180, 700);
        Size = new Size(1600, 940);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        KeyPreview = true;
        try { Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "Assets", "Logo", "Creation Master 26.ico")); }
        catch { /* icon optional at runtime */ }

        // Best-effort immersive dark/light mode for the window chrome + scrollbars.
        HandleCreated += (_, _) =>
        {
            if (IsHandleCreated)
                NativeTheme.TryApplyImmersiveMode(Handle);
        };

        _registry = BuildRegistry();
        _services.NavigationRequested += NavigateTo;
        _services.RecordNavigationRequested += NavigateToRecord;
        _services.ScraperSquadImportRequested += ImportScraperSquad;

        // ---- CM16-style application menu ----
        _menu = new MenuStrip { Dock = DockStyle.Top, BackColor = Theme.Background, ForeColor = Theme.Text, Font = Theme.Body, Renderer = new DarkToolStripRenderer() };
        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add("Open Game", null, async (_, _) => await OpenFc26Async());
        fileMenu.DropDownItems.Add("Save", null, async (_, _) => await SaveAsync());
        fileMenu.DropDownItems.Add("Restore Original Data…", null, async (_, _) => await RestoreOriginalAsync());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Exit", null, (_, _) => Close());
        var toolsMenu = new ToolStripMenuItem("Tools");
        toolsMenu.DropDownItems.Add("Database Browser", null, (_, _) => NavigateTo("browser"));
        toolsMenu.DropDownItems.Add("Diagnostics", null, (_, _) => NavigateTo("diagnostics"));
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
            catch { /* cannot open browser */ }
        });
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add("About", null, (_, _) => ShowAbout());
        _menu.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, patchMenu, helpMenu });

        // ---- Filter bar (FC Editor style): actions left, record filter right ----
        _filterBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Theme.Panel,
            Padding = new Padding(Theme.Space, 8, Theme.Space, 8),
        };
        _openBtn = MakeActionButton("Open Game", "Detect the game and load its database and assets automatically (Ctrl+O)");
        _saveBtn = MakeActionButton("Save", "Save staged changes (Ctrl+S)", primary: true);
        _undoBtn = MakeActionButton("Undo", "Undo last change (Ctrl+Z)");
        _redoBtn = MakeActionButton("Redo", "Redo the last undone change (Ctrl+Y)");
        _validateBtn = MakeActionButton("Validate", "Validate staged changes");
        _progress = new ProgressBar { Visible = false, Width = 170, Height = 18, Style = ProgressBarStyle.Marquee };
        _filterSearch = new TextBox { PlaceholderText = "Filter records…", Width = 230, Height = 28 };
        Theme.ApplyTextBox(_filterSearch);
        _filterSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyGlobalFilter();
            }
        };
        _goBtn = MakeActionButton("Go", "Apply the filter to the current section");
        _prevBtn = MakeActionButton("◀ Prev", "Previous record");
        _nextBtn = MakeActionButton("Next ▶", "Next record");
        _goBtn.Click += (_, _) => ApplyGlobalFilter();
        _prevBtn.Click += (_, _) => StepRecord(-1);
        _nextBtn.Click += (_, _) => StepRecord(+1);

        var filtersCaption = new Label
        {
            Text = "Filters",
            AutoSize = true,
            Font = Theme.Label,
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        _filterBar.Controls.Add(_nextBtn);
        _filterBar.Controls.Add(_prevBtn);
        _filterBar.Controls.Add(_goBtn);
        _filterBar.Controls.Add(_filterSearch);
        _filterBar.Controls.Add(filtersCaption);
        _filterBar.Controls.Add(_progress);
        _filterBar.Controls.Add(_validateBtn);
        _filterBar.Controls.Add(_redoBtn);
        _filterBar.Controls.Add(_undoBtn);
        _filterBar.Controls.Add(_saveBtn);
        _filterBar.Controls.Add(_openBtn);
        PositionFilterBar();
        _filterBar.Resize += (_, _) => PositionFilterBar();

        // ---- Left sidebar: Main Functions (FC Editor style) ----
        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = Theme.SidebarWidth,
            BackColor = Theme.Background,
            Padding = new Padding(0),
        };
        var sidebarHeader = new Label
        {
            Text = "Main Functions",
            Dock = DockStyle.Top,
            Height = 34,
            Font = Theme.Label,
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            BackColor = Theme.Background,
        };
        _sidebarList = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Theme.Background,
            Padding = new Padding(8, 2, 8, 8),
        };
        var categories = new (string Label, string[] Keys)[]
        {
            ("", new[] { "dashboard" }),
            ("World", new[] { "countries", "leagues", "teams", "players", "managers" }),
            ("Venue", new[] { "stadiums" }),
            ("Team", new[] { "kits", "competitions", "formations" }),
            ("System", new[] { "settings" }),
        };
        foreach (var (label, keys) in categories)
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                _sidebarList.Controls.Add(new Label
                {
                    Text = label,
                    AutoSize = true,
                    Font = Theme.Label,
                    ForeColor = Theme.Muted,
                    Margin = new Padding(10, 12, 0, 2),
                });
            }
            foreach (var key in keys)
                _sidebarList.Controls.Add(MakeSidebarButton(key, _registry.First(r => r.key == key).title));
        }
        _sidebarList.Resize += (_, _) => ResizeSidebarButtons();
        _sidebar.Controls.Add(_sidebarList);
        _sidebar.Controls.Add(sidebarHeader);

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
        Controls.Add(_filterBar);
        Controls.Add(_status);
        Controls.Add(_menu);

        // Events
        _openBtn.Click += async (_, _) => await OpenFc26Async();
        _saveBtn.Click += async (_, _) => await SaveAsync();
        _undoBtn.Click += (_, _) => Undo();
        _redoBtn.Click += (_, _) => Redo();
        _validateBtn.Click += (_, _) => ValidateAll();
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

    private Button MakeActionButton(string text, string tooltip, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Width = TextRenderer.MeasureText(text, Theme.Body).Width + 30,
            Height = 28,
            Margin = new Padding(2, 0, 2, 0),
            Font = primary ? Theme.BodyBold : Theme.Body,
        };
        _toolTip.SetToolTip(button, tooltip);
        Theme.ApplyButton(button, primary);
        return button;
    }

    private static readonly Dictionary<string, string> ShortcutByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dashboard"] = "Ctrl+1",
        ["countries"] = "Ctrl+2",
        ["leagues"] = "Ctrl+3",
        ["teams"] = "Ctrl+4",
        ["players"] = "Ctrl+5",
        ["managers"] = "Ctrl+6",
        ["stadiums"] = "Ctrl+7",
        ["kits"] = "Ctrl+8",
        ["competitions"] = "Ctrl+9",
    };

    private SidebarNavButton MakeSidebarButton(string key, string title)
    {
        var shortcut = ShortcutByKey.TryGetValue(key, out var sc) ? $" ({sc})" : string.Empty;
        var button = new SidebarNavButton
        {
            Image = IconService.Get(key, 20),
            Text = title,
            Width = _sidebarList.ClientSize.Width - 16,
            Font = Theme.Body,
        };
        _toolTip.SetToolTip(button, title + shortcut);
        button.Click += (_, _) => NavigateTo(key);
        _moduleButtons[key] = button;
        return button;
    }

    /// <summary>Stretches sidebar buttons to the flow panel width.</summary>
    private void ResizeSidebarButtons()
    {
        var width = Math.Max(120, _sidebarList.ClientSize.Width - 16);
        foreach (SidebarNavButton button in _sidebarList.Controls.OfType<SidebarNavButton>())
            button.Width = width;
    }

    /// <summary>Lays out the filter bar: actions left, Filters caption + search + Go + prev/next right.</summary>
    private void PositionFilterBar()
    {
        var y = (_filterBar.Height - 28) / 2;
        var x = Theme.Space;
        _openBtn.Location = new Point(x, y);
        x += _openBtn.Width + 4;
        _saveBtn.Location = new Point(x, y);
        x += _saveBtn.Width + 4;
        _undoBtn.Location = new Point(x, y);
        x += _undoBtn.Width + 4;
        _redoBtn.Location = new Point(x, y);
        x += _redoBtn.Width + 4;
        _validateBtn.Location = new Point(x, y);
        x += _validateBtn.Width + 4;
        _progress.Location = new Point(x, y + 5);
        x += _progress.Width + 4;

        var caption = _filterBar.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Filters");
        if (caption != null)
        {
            var captionRight = _filterBar.ClientSize.Width - Theme.Space;
            _nextBtn.Location = new Point(captionRight - _nextBtn.Width, y);
            captionRight -= _nextBtn.Width + 6;
            _prevBtn.Location = new Point(captionRight - _prevBtn.Width, y);
            captionRight -= _prevBtn.Width + 6;
            _goBtn.Location = new Point(captionRight - _goBtn.Width, y);
            captionRight -= _goBtn.Width + 8;
            _filterSearch.Location = new Point(captionRight - _filterSearch.Width, y);
            captionRight -= _filterSearch.Width + 8;
            caption.Location = new Point(captionRight - caption.Width, y + 5);
        }
    }

    /// <summary>Applies the global filter box text to the active section's record search.</summary>
    private void ApplyGlobalFilter()
    {
        if (_activeKey == null || !_sections.TryGetValue(_activeKey, out var section))
        {
            SetStatus("Open a section first to filter its records.");
            return;
        }
        var query = _filterSearch.Text.Trim();
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
        foreach (var kvp in _moduleButtons)
            kvp.Value.Checked = kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase);
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

    private async Task OpenFc26Async()
    {
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
            catch (InvalidOperationException) { /* form is closing */ }
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
            try { BeginInvoke((Action)OnFrostbiteAssetsReady); } catch (InvalidOperationException) { }
            return;
        }
        _assetStatus.Text = _services.FrostbiteAssets.IsAvailable
            ? "Assets: indexed" : "Assets: unavailable";
        _assetStatus.ForeColor = _services.FrostbiteAssets.IsAvailable ? Theme.Success : Theme.Warning;
        _assetStatus.ToolTipText = _services.FrostbiteAssets.Status;
        if (_activeKey != null && _sections.TryGetValue(_activeKey, out var section)) section.ActivateSection();
    }

    private async Task SaveAsync()
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
                      "has reloaded the editor from those live archives. "
                    : "\n\nThe live Data/Patch transaction completed, but automatic reload verification failed. " +
                      "The previous editor session is still open; close and reopen FC26 data before further editing. ") +
                "Use File > Restore Original Data to return to the untouched CmModData snapshot.",
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
        _saveBtn.Enabled = count > 0;
        _undoBtn.Enabled = _services.Pending.CanUndo;
        _redoBtn.Enabled = _services.Pending.CanRedo;
    }

    private void ShowAbout()
    {
        var text =
            $"Creation Master 26\nVersion {Program.ProductVersion}\n\n" +
            "Database, competition data and legacy asset editor for EA SPORTS FC 26.\n" +
            "Unofficial, independent community tool by Rizco98.\n\n" +
            "Use File > Open Game to begin. See the LICENSE file for terms.";
        var result = MessageBox.Show(this, text, "About Creation Master 26",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        _ = result;
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
                    catch { /* cannot open browser */ }
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
        _progress.Visible = busy;
        _openBtn.Enabled = !busy;
        _saveBtn.Enabled = !busy && (_services.Pending.HasChanges || _services.LegacyMods.HasChanges);
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
        ReThemeChrome();
        _status.BackColor = Theme.Panel;
        _statusText.ForeColor = Theme.Muted;
        _dbPath.ForeColor = Theme.Muted;
        _assetStatus.ForeColor = Theme.Muted;
        _pendingLabel.ForeColor = Theme.Warning;
        _workspace.BackColor = Theme.Background;
        _welcome.BackColor = Theme.Background;
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

    /// <summary>Re-colours the filter bar and sidebar after a theme toggle.</summary>
    private void ReThemeChrome()
    {
        _filterBar.BackColor = Theme.Panel;
        _sidebar.BackColor = Theme.Background;
        _sidebarList.BackColor = Theme.Background;
        foreach (Control control in _sidebar.Controls)
        {
            if (control is Label label)
            {
                label.BackColor = Theme.Background;
                label.ForeColor = Theme.Muted;
            }
        }
        foreach (Control control in _sidebarList.Controls)
        {
            switch (control)
            {
                case Label label:
                    label.ForeColor = Theme.Muted;
                    break;
                case SidebarNavButton nav:
                    nav.ApplyTheme();
                    break;
            }
        }
        Theme.ApplyTextBox(_filterSearch);
        foreach (Control control in _filterBar.Controls)
        {
            if (control is Label label)
            {
                label.ForeColor = Theme.Muted;
                label.BackColor = Theme.Panel;
            }
        }
        foreach (var button in new[] { _openBtn, _saveBtn, _undoBtn, _redoBtn, _validateBtn, _goBtn, _prevBtn, _nextBtn })
            Theme.ApplyButton(button, primary: ReferenceEquals(button, _saveBtn));
        _filterBar.Invalidate(true);
        _sidebar.Invalidate(true);
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
            case Keys.Control | Keys.S: _ = SaveAsync(); return true;
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
