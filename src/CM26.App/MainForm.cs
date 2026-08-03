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
    private readonly ToolStrip _toolbar;
    private readonly StatusStrip _status;
    private readonly ToolStripStatusLabel _statusText, _dbPath, _assetStatus, _pendingLabel;
    private readonly ToolStripButton _openBtn, _saveBtn, _undoBtn, _validateBtn;
    private readonly ToolStripProgressBar _progress;

    private readonly Dictionary<string, SectionBase> _sections = new();
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

        _registry = BuildRegistry();
        _services.NavigationRequested += NavigateTo;
        _services.RecordNavigationRequested += NavigateToRecord;
        _services.ScraperSquadImportRequested += ImportScraperSquad;

        // ---- CM16-style application menu ----
        _menu = new MenuStrip { Dock = DockStyle.Top, BackColor = Theme.Background, ForeColor = Theme.Text, Font = Theme.Body };
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
        helpMenu.DropDownItems.Add("About", null, (_, _) => MessageBox.Show(this,
            $"Creation Master 26\nVersion {Program.ProductVersion}\n\nDatabase, competition data and legacy asset editor.\nCommunity tool by Rizco98.",
            "About Creation Master 26", MessageBoxButtons.OK, MessageBoxIcon.Information));
        _menu.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, patchMenu, helpMenu });

        // ---- Toolbar ----
        _toolbar = new ToolStrip
        {
            Dock = DockStyle.Top,
            Height = Theme.ToolbarHeight,
            BackColor = Theme.Panel,
            GripStyle = ToolStripGripStyle.Hidden,
            Padding = new Padding(Theme.Space, 6, Theme.Space, 6),
            ImageScalingSize = new Size(36, 36),
            RenderMode = ToolStripRenderMode.Professional,
        };
        _openBtn = MakeToolButton("📂 Open Game", "Detect the game and load its database and assets automatically (Ctrl+O)");
        _saveBtn = MakeToolButton("💾 Save", "Save staged changes (Ctrl+S)", primary: true);
        _undoBtn = MakeToolButton("↶ Undo", "Undo last change (Ctrl+Z)");
        _validateBtn = MakeToolButton("✔ Validate", "Validate staged changes");
        _progress = new ToolStripProgressBar { Visible = false, Width = 180, Style = ProgressBarStyle.Marquee };
        var titleLabel = new ToolStripLabel("  Creation Master 26")
        { ForeColor = Theme.Text, Font = Theme.SectionTitle, Alignment = ToolStripItemAlignment.Right };
        _toolbar.Items.Add(_openBtn);
        _toolbar.Items.Add(new ToolStripSeparator());
        _toolbar.Items.Add(_saveBtn);
        _toolbar.Items.Add(_undoBtn);
        _toolbar.Items.Add(_validateBtn);
        _toolbar.Items.Add(new ToolStripSeparator());
        // CM16 navigates modules from its icon toolbar, not from a modern left sidebar.
        // Group modules into logical categories with separators, like CM16's tab strip.
        var categories = new[]
        {
            new[] { "dashboard" },
            new[] { "countries", "leagues", "teams", "players", "managers" },
            new[] { "stadiums", "stadiumaudio" },
            new[] { "kits", "competitions", "formations" },
            new[] { "transfers" },
            new[] { "balls", "boots", "gloves", "sponsors", "adboards" },
            new[] { "audio", "scoreboard" },
            new[] { "referees" },
        };
        foreach (var group in categories)
        {
            _toolbar.Items.Add(new ToolStripSeparator());
            foreach (var key in group)
                _toolbar.Items.Add(MakeModuleButton(key, _registry.First(r => r.key == key).title));
        }
        _toolbar.Items.Add(new ToolStripSeparator());
        _toolbar.Items.Add(_progress);
        _toolbar.Items.Add(titleLabel);

        // ---- Status bar ----
        _status = new StatusStrip { BackColor = Theme.Panel, ForeColor = Theme.Muted, SizingGrip = true };
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
        ShowEmptyWorkspace();

        Controls.Add(_workspace);
        Controls.Add(_toolbar);
        Controls.Add(_status);
        Controls.Add(_menu);

        // Events
        _openBtn.Click += async (_, _) => await OpenFc26Async();
        _saveBtn.Click += async (_, _) => await SaveAsync();
        _undoBtn.Click += (_, _) => Undo();
        _validateBtn.Click += (_, _) => ValidateAll();
        _services.PendingChanged += (_, _) => RefreshPendingState();
        _services.DatabaseLoaded += (_, _) => OnDatabaseLoaded();
        _services.FrostbiteAssetsReady += (_, _) => OnFrostbiteAssetsReady();

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
        ("scoreboard", "Scoreboard", s => new TvSection(s)),
        ("referees", "Referees", s => new RefereesSection(s)),
        ("browser", "Database Browser", s => new DatabaseBrowserSection(s)),
        ("diagnostics", "Diagnostics", s => new DiagnosticsSection(s)),
        ("settings", "Settings", s => new SettingsSection(s)),
    };

    private static ToolStripButton MakeToolButton(string text, string tooltip, bool primary = false)
    {
        return new ToolStripButton(text)
        {
            ToolTipText = tooltip,
            ForeColor = primary ? Color.White : Theme.Text,
            BackColor = primary ? Theme.Accent : Theme.Raised,
            Font = primary ? Theme.BodyBold : Theme.Body,
            Margin = new Padding(2, 2, 2, 2),
            Padding = new Padding(8, 2, 8, 2),
        };
    }

    private ToolStripButton MakeModuleButton(string key, string title)
    {
        var button = new ToolStripButton
        {
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            Image = IconService.Get(key, 36),
            ImageTransparentColor = Color.Transparent,
            ToolTipText = title,
            AutoSize = false,
            Width = 44,
            Height = 44,
            Margin = new Padding(1, 0, 1, 0),
        };
        button.Click += (_, _) => NavigateTo(key);
        return button;
    }

    public void NavigateTo(string key)
    {
        if (!_services.Session.IsLoaded && key is not ("settings" or "dashboard"))
        {
            SetStatus("Open game data first (Ctrl+O).");
            return;
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
        _dbPath.Text = string.IsNullOrWhiteSpace(_services.ActiveGameRoot)
            ? (_services.Session.LoadedFolder ?? string.Empty)
            : _services.ActiveGameRoot;
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
            if (reloadAfterApply)
                await ReloadFromLiveFc26Async();
            SetStatus(result.Message);
            MessageBox.Show(this,
                result.Message + "\n\nThe live Data/Patch archives now contain the edits and Creation Master " +
                "has reloaded the editor from those live archives. " +
                "Use File > Restore Original Data to return to the untouched CmModData snapshot.",
                "Direct edit complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        finally { SetBusy(false, null); }
    }

    private async Task ReloadFromLiveFc26Async()
    {
        SetBusy(true, "Verifying and reloading live Data/Patch…");
        var workspace = await Task.Run(() => _services.ReloadFromLiveFc26());
        SetStatus(
            $"Saved, verified and reloaded directly from {workspace.GameRoot}\\Data and Patch.");
        if (_activeKey != null && _sections.TryGetValue(_activeKey, out var section))
            section.ActivateSection();
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
            case Keys.Control | Keys.F:
                if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s)) s.FocusSearchBox();
                return true;
            case Keys.F5:
                if (_activeKey != null && _sections.TryGetValue(_activeKey, out var s2)) s2.LoadData();
                return true;
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
