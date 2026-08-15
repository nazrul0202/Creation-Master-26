using System.Windows;
using System.Windows.Controls;
using CM26.Studio.Services;
using CM26.Studio.Views;

namespace CM26.Studio;

public partial class MainWindow : Window
{
    private AppSession _session = new();
    private double _bottomHeight = 160;
    private double _rightWidth = 320;

    /// <summary>Lets the smoke harness inject its own session so it can observe the load.</summary>
    public AppSession SmokeSession
    {
        set => _session = value;
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
        DbStatusText.Text = "FC26 not open";
        PendingCountText.Text = _session.Pending.Count + " pending changes";
    }

    /// <summary>
    /// Mirrors CM16 MainForm.EnableMenus(): enables/disables the menu items
    /// depending on whether a database is currently open.
    /// </summary>
    private void ApplyDatabaseState(bool open)
    {
        MenuOpenFifa16.IsEnabled = !open;
        MenuOpenLang16.IsEnabled = !open;
        MenuOpenAll.IsEnabled = !open;
        MenuReopen.IsEnabled = !open;
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

    private void MenuOpenGame_Click(object sender, RoutedEventArgs e)
    {
        StatusBarText.Text = "Opening FC26...";
        var progress = new Progress<string>(phase => StatusBarText.Text = phase);
        string message = string.Empty;
        var loaded = _session.TryOpenGame(out message, progress);
        DbStatusText.Text = loaded ? "Database loaded - direct FC26 editing" : "No database loaded";
        StatusBarText.Text = loaded ? "FC26 opened for direct editing." : message;
        PendingCountText.Text = _session.Pending.Count + " pending changes";
        if (loaded)
        {
            ApplyDatabaseState(true);
            ShowDashboard();
            RefreshDashboardCounts();
        }
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    private void MenuTool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem mi) return;
        if (mi.Tag is not string key) return;
        if (IsSectionKey(key))
        {
            CheckSection(key);
            ShowSection(key);
        }
        else
        {
            RightPanelHost.Content = null;
            RightPanelTitle.Text = "Details";
            ContentHost.Content = new PlaceholderView(mi.Header?.ToString() ?? key);
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
        MessageBox.Show(
            "Creation Master 26 - Studio\n\nCM16-style database editor for EA Sports FC 26.\nDirect edit on the installed game; changes apply to the live game database.",
            "About", MessageBoxButton.OK, MessageBoxImage.Information);
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