using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CM26.Application.Services;
using CM26.Studio.Services;
using CM26.Studio.Views;
using Microsoft.Win32;

namespace CM26.Studio;

public partial class MainWindow : Window
{
    private AppSession _session = new();
    private double _bottomHeight = 160;
    private double _rightWidth = 320;
    private bool _suppressAutomaticGameLoad;
    private string? _activeSectionKey;

    /// <summary>Lets the smoke harness inject its own session so it can observe the load.</summary>
    public AppSession SmokeSession
    {
        set
        {
            _session = value;
            // The smoke runner opens the same path explicitly after the visual
            // tree is ready, so it must not race the normal startup auto-open.
            _suppressAutomaticGameLoad = true;
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // CM16 startup state: no database opened, toolbar and panels disabled,
        // only the open/save-less File menu items and a few tools are available.
        ApplyDatabaseState(false);
        StatusBarText.Text = "Ready";
        if (!_suppressAutomaticGameLoad)
            _ = AutomaticallyOpenDetectedGameAsync();
    }

    /// <summary>
    /// Mirrors CM16's environment bootstrap, adapted for FC26: if a validated
    /// FC26 installation is already known (settings, EA registry, or a Steam
    /// library), open its Frostbite Data/Patch source without prompting for a
    /// folder. An undetected or unavailable install leaves the CM16 pre-open
    /// shell intact so File &gt; Open remains available.
    /// </summary>
    private async Task AutomaticallyOpenDetectedGameAsync()
    {
        if (string.IsNullOrWhiteSpace(FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder)))
            return;

        ProgressBar.Visibility = Visibility.Visible;
        StatusBarText.Text = "Automatically loading FC26 Frostbite files...";
        string message = string.Empty;
        var progress = new Progress<string>(phase => StatusBarText.Text = phase);
        var loaded = await Task.Run(() => _session.TryOpenGame(out message, progress));
        if (!IsLoaded) return;

        ProgressBar.Visibility = Visibility.Collapsed;
        StatusBarText.Text = loaded ? "FC26 opened automatically for direct editing." : message;
        if (!loaded) return;

        ApplyDatabaseState(true);
        OpenDefaultCm16Section();
    }

    /// <summary>
    /// Mirrors CM16 MainForm.EnableMenus(): enables/disables the menu items
    /// depending on whether a database is currently open.
    /// </summary>
    private void ApplyDatabaseState(bool open)
    {
        // FC26 can be re-indexed/reloaded at any time. Disabling these after
        // automatic startup loading made File > Open FC26 appear dead, even
        // though users reasonably expect it to reload both DB and assets.
        MenuOpenFifa16.IsEnabled = true;
        MenuOpenLang16.IsEnabled = true;
        MenuOpenAll.IsEnabled = true;
        MenuReopen.IsEnabled = true;
        MenuSave.IsEnabled = open;
        MenuClose.IsEnabled = open;
        MenuRegenerate.IsEnabled = true; // CM16 keeps Regenerate enabled in both states
        MenuExpandDatabase.IsEnabled = open;
        MenuAlignLanguageDB.IsEnabled = open;
        MenuMinimizeNamesTable.IsEnabled = open;
        MenuPreserveOriginalNames.IsEnabled = open;
        MenuInstallRevModPatch.IsEnabled = open;
        MenuRemoveFakePlayers.IsEnabled = open;
        MenuPlayerNameRules.IsEnabled = open;
        MenuFixProblems.IsEnabled = open;
        MenuRepairRoster.IsEnabled = open;
        MenuRepairTeamSheets.IsEnabled = open;
        MenuUniqueJerseys.IsEnabled = open;
        MenuEnableSpecificFaces.IsEnabled = open;
        MenuFixLoanDates.IsEnabled = open;
        MenuRemoveFreeAgentWithClub.IsEnabled = open;
        MenuAddFreeAgentWithoutClub.IsEnabled = open;
        MenuCreateDBEntryKits.IsEnabled = open;
        MenuCreateDummyKit.IsEnabled = open;
        MenuRandomizeLegends.IsEnabled = open;
        MenuSetFreeAgentDates.IsEnabled = open;
        MenuResetCommentary.IsEnabled = open;
        MenuAssociateCommentary.IsEnabled = open;
        MenuCreateCommentary.IsEnabled = open;
        MenuConvertMinheads.IsEnabled = open;
        MenuPatch.IsEnabled = open;
        MenuUpdateDB.IsEnabled = open;
        SectionBarHost.IsEnabled = open;
        Workspace.IsEnabled = open;
        // CM16 also keeps "Enable all messages" disabled until open
        var enableMessages = FindMenuItem("enable-messages");
        if (enableMessages != null) enableMessages.IsEnabled = open;
    }

    private MenuItem? FindMenuItem(string tag)
    {
        foreach (var item in MenuTools.Items)
        {
            if (item is MenuItem mi && string.Equals(mi.Tag as string, tag, StringComparison.Ordinal))
                return mi;
        }
        return null;
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb || rb.Tag is not string key) return;
        if (ContentHost is null) return;
        ShowSection(key);
        RightPanelHost.Content = null;
    }

    private void ShowSection(string key)
    {
        _activeSectionKey = key;
        var vm = new ViewModel(_session);
        switch (key)
        {
            case "dashboard":
                RightPanelTitle.Text = "Preview";
                ContentHost.Content = new DashboardView(vm);
                break;
            case "players":
                RightPanelTitle.Text = "Player";
                ContentHost.Content = new PlayersView(vm);
                break;
            case "teams":
                RightPanelTitle.Text = "Team";
                ContentHost.Content = new TeamView(vm);
                break;
            case "countries":
                RightPanelTitle.Text = "Country";
                ContentHost.Content = new CountryView(vm);
                break;
            case "leagues":
                RightPanelTitle.Text = "League";
                ContentHost.Content = new LeagueView(vm);
                break;
            case "managers":
                RightPanelTitle.Text = "Manager";
                ContentHost.Content = new ManagerView(vm);
                break;
            case "stadiums":
                RightPanelTitle.Text = "Stadium";
                ContentHost.Content = new StadiumView(vm);
                break;
            case "referees":
                RightPanelTitle.Text = "Referee";
                ContentHost.Content = new RefereeView(vm);
                break;
            case "formations":
                RightPanelTitle.Text = "Formation";
                ContentHost.Content = new FormationView(vm);
                break;
            case "kits":
                RightPanelTitle.Text = "Kit";
                ContentHost.Content = new KitView(vm);
                break;
            case "tournament":
                RightPanelTitle.Text = "Tournament";
                ContentHost.Content = new TournamentView(vm);
                break;
            case "balls":
                RightPanelTitle.Text = "Ball";
                ContentHost.Content = new BallView(vm);
                break;
            case "shoes":
                RightPanelTitle.Text = "Shoes";
                ContentHost.Content = new ShoesView(vm);
                break;
            case "sponsor":
                RightPanelTitle.Text = "Sponsor";
                ContentHost.Content = new SponsorView(vm);
                break;
            case "gloves":
                RightPanelTitle.Text = "Gloves";
                ContentHost.Content = new GlovesView(vm);
                break;
            case "tv":
                RightPanelTitle.Text = "Tv";
                ContentHost.Content = new TvView();
                break;
            case "newspaper":
                RightPanelTitle.Text = "Newspaper";
                ContentHost.Content = new NewspaperView();
                break;
            case "audio":
                RightPanelTitle.Text = "Audio";
                ContentHost.Content = new AudioView();
                break;
            case "gamegraphics":
                RightPanelTitle.Text = "Game Graphics";
                ContentHost.Content = new GameGraphicsView();
                break;
            case "browser":
                RightPanelTitle.Text = "Browser";
                ContentHost.Content = new BrowserView();
                break;
            case "importgraphics":
                RightPanelTitle.Text = "Import Graphics";
                ContentHost.Content = new ImportGraphicsView();
                break;
            default:
                RightPanelTitle.Text = "Details";
                ContentHost.Content = new PlaceholderView(key);
                break;
        }
        UpdateStripLabelRight();
    }

    private void ShowDashboard() => ShowSection("dashboard");

    private void RefreshDashboardCounts()
    {
        if (ContentHost.Content is DashboardView dashboard) dashboard.RefreshCounts();
    }

    private async void MenuOpenGame_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardPendingChanges()) return;
        await OpenGameAsync(sender as MenuItem);
    }

    private async Task OpenGameAsync(MenuItem? menuItem = null)
    {
        if (menuItem != null) menuItem.IsEnabled = false;
        try
        {
            ProgressBar.Visibility = Visibility.Visible;
            StatusBarText.Text = "Loading FC26 database and Frostbite assets...";
            var progress = new Progress<string>(phase => StatusBarText.Text = phase);
            string message = string.Empty;
            var loaded = await Task.Run(() => _session.TryOpenGame(out message, progress));
            if (!IsLoaded) return;
            StatusBarText.Text = loaded ? "FC26 database and Frostbite assets loaded." : message;
            if (loaded)
            {
                ApplyDatabaseState(true);
                OpenDefaultCm16Section();
            }
            else
            {
                MessageBox.Show(this, message, "Open FC26", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        finally
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            if (menuItem != null) menuItem.IsEnabled = true;
        }
    }

    private bool ConfirmDiscardPendingChanges()
    {
        if (!_session.Pending.HasChanges && !_session.LegacyMods.HasChanges) return true;
        return MessageBox.Show(this,
            "There are staged changes that have not been saved. Open another source and discard them?",
            "Unsaved changes", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    private async void MenuOpenDatabaseFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardPendingChanges()) return;
        var dialog = new OpenFolderDialog
        {
            Title = "Select extracted FC26 database folder",
            InitialDirectory = Directory.Exists(SettingsService.LastFolder)
                ? SettingsService.LastFolder
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dialog.ShowDialog(this) != true) return;
        await OpenDatabaseFolderAsync(dialog.FolderName);
    }

    private async Task OpenDatabaseFolderAsync(string folder)
    {
        ProgressBar.Visibility = Visibility.Visible;
        StatusBarText.Text = "Validating extracted database and localization...";
        try
        {
            string message = string.Empty;
            var loaded = await Task.Run(() => _session.TryOpenDatabaseFolder(folder, out message));
            StatusBarText.Text = message;
            if (!loaded)
            {
                MessageBox.Show(this, message, "Open database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ApplyDatabaseState(true);
            OpenDefaultCm16Section();
        }
        finally
        {
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private async void MenuRecent_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        MenuReopen.Items.Clear();
        var recent = SettingsService.RecentFolders.Where(path => Directory.Exists(path)).ToArray();
        if (recent.Length == 0)
        {
            MenuReopen.Items.Add(new MenuItem { Header = "No recent sources", IsEnabled = false });
            return;
        }
        foreach (var path in recent)
        {
            var item = new MenuItem { Header = path, Tag = path };
            item.Click += async (_, _) =>
            {
                if (!ConfirmDiscardPendingChanges()) return;
                if (FrostbiteAssetSession.IsGameRoot(path))
                {
                    SettingsService.FC26GameFolder = path;
                    await OpenGameAsync(item);
                }
                else
                {
                    await OpenDatabaseFolderAsync(path);
                }
            };
            MenuReopen.Items.Add(item);
        }
        await Task.CompletedTask;
    }

    private void OpenDefaultCm16Section()
    {
        // CM16 opens directly on an editor section. A dashboard is not part of
        // its workflow and made the FC26 port look like a generic database app.
        CheckSection("countries");
        ShowSection("countries");
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    private async void MenuTool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem mi) return;
        if (mi.Tag is not string key) return;
        if (IsSectionKey(key))
        {
            CheckSection(key);
            ShowSection(key);
            return;
        }
        if (key == "save")
        {
            await SaveDirectToGameAsync();
            return;
        }
        if (key == "close")
        {
            if (!ConfirmDiscardPendingChanges()) return;
            _session.CloseDatabase();
            ContentHost.Content = null;
            RightPanelHost.Content = null;
            ApplyDatabaseState(false);
            StatusBarText.Text = "Database closed.";
            return;
        }
        if (key == "undo-operation" || key == "redo-operation")
        {
            var changed = key == "undo-operation"
                ? _session.Pending.UndoLastOperation()
                : _session.Pending.RedoLastOperation();
            StatusBarText.Text = changed
                ? (key == "undo-operation" ? "Complete operation undone." : "Complete operation restored.")
                : "No complete scalar operation is available.";
            if (changed && _activeSectionKey != null) ShowSection(_activeSectionKey);
            return;
        }

        var result = RunDbTool(key);
        if (result == null) return; // handled by UI state, nothing to report
        StatusBarText.Text = result.Message;
        MessageBox.Show(this, result.Message, mi.Header?.ToString() ?? key,
            MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        if (result.Success) RefreshDashboardCounts();
    }

    /// <summary>
    /// CM16-style Save: the staged draft is validated and committed through the
    /// direct FC26 Data/Patch transaction (same engine path as the WinForms
    /// shell), then the editor reloads from the freshly written archives so the
    /// game runs with the edits applied.
    /// </summary>
    private async Task SaveDirectToGameAsync()
    {
        if (!_session.Database.IsLoaded)
        {
            MessageBox.Show(this, "The database is not loaded. Open FC26 first.", "Save",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!_session.Pending.HasChanges && !_session.LegacyMods.HasChanges)
        {
            StatusBarText.Text = "No staged changes to save.";
            MessageBox.Show(this, "There are no staged changes to save.", "Save",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (new[] { "FC26", "FC26_Trial", "FC26_Showcase" }.Any(name => Process.GetProcessesByName(name).Length > 0))
        {
            MessageBox.Show(this, "Close FC26 before saving a direct edit.", "Save",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var issues = _session.Validation.ValidateAll(_session.Pending.Changes);
        if (issues.Any(issue => issue.IsError))
        {
            StatusBarText.Text = "Resolve validation errors before saving.";
            MessageBox.Show(this, "Resolve validation errors before saving a direct edit.", "Save",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!_session.IsDirectGameSource)
        {
            ProgressBar.Visibility = Visibility.Visible;
            StatusBarText.Text = "Backing up and saving extracted database...";
            try
            {
                var saved = await Task.Run(() => _session.Save.SaveToSourceFolder());
                if (saved.Success)
                {
                    _session.Pending.MarkSaved();
                    var sourcePath = _session.SourcePath;
                    string reloadMessage = string.Empty;
                    await Task.Run(() => _session.TryOpenDatabaseFolder(sourcePath, out reloadMessage));
                    if (_activeSectionKey != null) ShowSection(_activeSectionKey);
                    RefreshDashboardCounts();
                }
                StatusBarText.Text = saved.Message;
                MessageBox.Show(this, saved.Message, saved.Success ? "Save complete" : "Save failed",
                    MessageBoxButton.OK, saved.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
            return;
        }
        var gameRoot = !string.IsNullOrWhiteSpace(_session.FrostbiteAssets.GameRoot)
            ? _session.FrostbiteAssets.GameRoot
            : FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
        if (string.IsNullOrWhiteSpace(gameRoot))
        {
            MessageBox.Show(this, "The FC26 installation was not detected. Open FC26 first.", "Save",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var backup = GameBackupService.Inspect(gameRoot, verifyContent: true);
        if (!backup.IsReady)
        {
            MessageBox.Show(this, "Direct edit requires a verified CmModData backup.\n\n" + backup.Message,
                "Save", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var baseline = GameBackupService.InspectLiveBaseline(gameRoot);
        if (!baseline.IsMatch)
        {
            MessageBox.Show(this, baseline.Message, "Save blocked",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show(this,
            $"Apply {_session.Pending.Count + _session.LegacyMods.Count} staged change(s) directly to FC26 Data/Patch?\n\n" +
            "CM26 verifies the original CmModData snapshot, writes archive metadata atomically, and restores the previous TOCs if the transaction fails.\n\n" +
            "The live game files will change; the WinForms shell's File > Restore Original Data reverses the edit.",
            "Confirm direct offline edit", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        ProgressBar.Visibility = Visibility.Visible;
        StatusBarText.Text = "Preparing direct FC26 transaction...";
        try
        {
            if (_session.Pending.HasChanges)
            {
                var stagingFolder = Path.Combine(Path.GetTempPath(),
                    "CM26-studio-save-" + Guid.NewGuid().ToString("N"));
                var saved = await Task.Run(() => _session.Save.SaveToDirectory(stagingFolder));
                if (!saved.Success) throw new InvalidOperationException(saved.Message);
                _session.LegacyMods.StageDatabase(
                    stagingFolder,
                    includeLocale: _session.Pending.Changes.Any(change => change.IsLocale));
            }

            StatusBarText.Text = "Applying direct Data/Patch transaction...";
            var plan = _session.LegacyMods.WriteDirectPlan();
            var result = await Task.Run(() => _session.FrostbiteAssets.ApplyDirect(plan));
            if (!result.Success) throw new InvalidOperationException(result.Message);
            _session.LegacyMods.MarkApplied();
            _session.Pending.MarkSaved();

            var reloadMessage = string.Empty;
            var reloaded = await Task.Run(() => _session.ReloadFromGame(out reloadMessage));
            if (_activeSectionKey != null) ShowSection(_activeSectionKey);
            RefreshDashboardCounts();
            StatusBarText.Text = reloaded
                ? "Direct FC26 edit applied and reloaded."
                : "Direct FC26 edit applied; reload failed - reopen FC26 data before further editing.";
            MessageBox.Show(this,
                result.Message + (reloaded
                    ? "\n\nThe live Data/Patch archives now contain the edits and Creation Master has reloaded them."
                    : "\n\nThe live Data/Patch transaction completed, but the automatic reload failed. Reopen FC26 data before further editing.") +
                "\n\nLaunch the game as usual to play with the applied edits.",
                reloaded ? "Save complete" : "Save applied; reload required",
                MessageBoxButton.OK,
                reloaded ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            Cm26Log.Write("Studio direct save failed: " + ex);
            StatusBarText.Text = "Direct save failed; the live transaction was not completed.";
            MessageBox.Show(this, ex.Message, "Save failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>Runs one CM16-style Tools menu action against the loaded FC26 database.</summary>
    private ToolRunResult? RunDbTool(string key)
    {
        if (!_session.Database.IsLoaded)
            return new ToolRunResult(false, "The database is not loaded. Open FC26 first.");

        switch (key)
        {
            case "remove-fake":
                return DbToolsService.RemoveFakePlayers(_session.Database, _session.Pending);
            case "loan-dates":
                return DbToolsService.SetContractEndAfterLoanEnd(_session.Database, _session.Pending);
            case "freeagent-remove":
                return DbToolsService.RemoveFromFreeAgentIfHasClub(_session.Database, _session.Pending);
            case "freeagent-add":
                return DbToolsService.AddToFreeAgentIfWithoutClub(_session.Database, _session.Pending);
            case "name-rules":
                return DbToolsService.SimplifyPlayerNameUsingCountryRules(_session.Database, _session.Pending);
            case "commentary-reset":
                return DbToolsService.ResetCommentaryNames(_session.Database, _session.Pending);
            case "repair-roster":
                return DbToolsService.RepairRosterLinks(_session.Database, _session.Pending);
            case "repair-teamsheets":
                return DbToolsService.RepairTeamSheets(_session.Database, _session.Pending);
            case "unique-jerseys":
                return DbToolsService.AssignUniqueJerseyNumbers(_session.Database, _session.Pending);
            case "validate-integrity":
                return DbToolsService.ValidateDatabase(_session.Database);
            case "convert-minheads":
                return DbToolsService.ConvertMiniheadsToPng(FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder));
            case "enable-messages":
                return new ToolRunResult(true, "All messages are enabled.");
            case "regenerate":
            case "expand-db":
            case "align-langdb":
            case "minimize-names":
            case "preserve-names":
            case "revmod":
            case "specific-faces":
            case "dbentry-kits":
            case "dummy-kit":
            case "randomize-legends":
            case "freeagent-dates":
            case "commentary-associate":
            case "commentary-create":
            case "create-patch":
            case "load-patch":
            case "update-21":
            case "update-20":
            case "update-19":
            case "update-19-rosters":
            case "update-19-players":
            case "update-18":
            case "update-18-players":
            case "update-16":
            case "update-16-rosters":
            case "update-16-players":
                return DbToolsService.NotApplicable(key);
            default:
                return null;
        }
    }

    private static bool IsSectionKey(string key) => key is "countries" or "leagues" or "teams" or "kits"
        or "players" or "stadiums" or "tournament" or "referees" or "balls" or "shoes"
        or "managers" or "formations" or "sponsor" or "gloves" or "tv" or "newspaper"
        or "audio" or "gamegraphics" or "browser" or "importgraphics" or "dashboard";

    private void CheckSection(string key)
    {
        foreach (var child in SectionBar.Children)
        {
            if (child is RadioButton rb && string.Equals(rb.Tag as string, key, StringComparison.Ordinal))
            {
                rb.IsChecked = true;
                return;
            }
        }
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        new Views.AboutWindow { Owner = this }.ShowDialog();
    }

    // ============ CM16 toolStripBottom / toolStripRight show-hide ============

    private void ShowBottom_Click(object sender, RoutedEventArgs e)
    {
        BottomArea.Height = _bottomHeight;
        BtnShowBottom.Visibility = Visibility.Collapsed;
        BtnHideBottom.Visibility = Visibility.Visible;
    }

    private void HideBottom_Click(object sender, RoutedEventArgs e)
    {
        _bottomHeight = Math.Max(BottomArea.Height, 26);
        BottomArea.Height = 25;
        BtnShowBottom.Visibility = Visibility.Visible;
        BtnHideBottom.Visibility = Visibility.Collapsed;
    }

    private void ShowRight_Click(object sender, RoutedEventArgs e)
    {
        RightArea.Width = _rightWidth;
        BtnShowRight.Visibility = Visibility.Collapsed;
        BtnHideRight.Visibility = Visibility.Visible;
        UpdateStripLabelRight();
    }

    private void HideRight_Click(object sender, RoutedEventArgs e)
    {
        _rightWidth = Math.Max(RightArea.ActualWidth, 26);
        RightArea.Width = 24;
        BtnShowRight.Visibility = Visibility.Visible;
        BtnHideRight.Visibility = Visibility.Collapsed;
        UpdateStripLabelRight();
    }

    /// <summary>Mirrors CM16 ShowFormOnPanel: right strip label = hosted form text or "Empty".</summary>
    private void UpdateStripLabelRight()
    {
        if (RightPanelHost.Content is FrameworkElement fe && fe is not null)
            StripLabelRight.Text = RightPanelTitle.Text;
        else
            StripLabelRight.Text = "Empty";
    }
}
public sealed class ViewModel
{
    public AppSession Session { get; }
    public ViewModel(AppSession session) => Session = session;
}
