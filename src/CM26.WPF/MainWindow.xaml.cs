using System.Windows;
using System.Windows.Controls;
using CM26.Studio.Services;
using CM26.Studio.Views;

namespace CM26.Studio;

public partial class MainWindow : Window
{
    private readonly AppSession _session = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ShowDashboard();
        var loaded = await Task.Run(() => _session.TryLoadRecentWorkspace(out var message));
        DbStatusText.Text = loaded ? "Database loaded" : "No database loaded";
        StatusBarText.Text = loaded ? "Workspace database loaded." : "Open a database folder from Settings to begin.";
        PendingCountText.Text = _session.Pending.Count + " pending changes";
        if (loaded) RefreshDashboardCounts();
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb || rb.Tag is not string key) return;
        if (ContentHost is null) return;
        var vm = new ViewModel(_session);
        switch (key)
        {
            case "dashboard": ContentHost.Content = new DashboardView(vm); break;
            default:
                ContentHost.Content = new PlaceholderView(key);
                break;
        }
    }

    private void ShowDashboard()
    {
        var vm = new ViewModel(_session);
        ContentHost.Content = new DashboardView(vm);
    }

    private void RefreshDashboardCounts()
    {
        if (ContentHost.Content is DashboardView dashboard) dashboard.RefreshCounts();
    }
}

public sealed class ViewModel
{
    public AppSession Session { get; }
    public ViewModel(AppSession session) => Session = session;
}