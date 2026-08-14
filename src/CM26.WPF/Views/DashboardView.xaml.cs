using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Studio.Services;
using Microsoft.Win32;

namespace CM26.Studio.Views;

public partial class DashboardView : UserControl
{
    private readonly ViewModel _vm;

    public DashboardView(ViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        RefreshCounts();
    }

    public void RefreshCounts()
    {
        var session = _vm.Session;
        if (!session.Database.IsLoaded)
        {
            MetricPlayers.Text = MetricTeams.Text = MetricLeagues.Text = MetricCountries.Text = "—";
            DbPathText.Text = "No database loaded";
            DbTablesText.Text = string.Empty;
            return;
        }

        try
        {
            MetricPlayers.Text = session.Sections.GetPlayers().Count.ToString("N0");
            MetricTeams.Text = session.Sections.GetTeams().Count.ToString("N0");
            MetricLeagues.Text = session.Sections.GetLeagues().Count.ToString("N0");
            MetricCountries.Text = session.Sections.GetCountries().Count.ToString("N0");
            DbPathText.Text = session.Database.DatabasePath ?? string.Empty;
            DbTablesText.Text = session.Database.Tables.Count + " tables, " + session.Pending.Count + " pending changes";
            HeroSubtitle.Text = "Workspace database loaded from " + session.Database.LoadedFolder;
        }
        catch (Exception ex)
        {
            DbPathText.Text = "Count failed: " + ex.Message;
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open FC26 database folder",
            Filter = "FC26 Database (fifa_ng_db-meta.xml)|fifa_ng_db-meta.xml|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog() != true) return;
        var folder = System.IO.Path.GetDirectoryName(dialog.FileName);
        if (folder is null) return;

        var summary = _vm.Session.Database.ValidateFolder(folder);
        if (summary.State != CM26.EngineBridge.LoadStateKind.Success)
        {
            MessageBox.Show("Unsupported database folder: " + summary.Message, "Open Database",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            _vm.Session.Database.Load(folder);
            RefreshCounts();
            DbStatusRefresh?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Database load failed: " + ex.Message, "Open Database",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        var folder = _vm.Session.Database.LoadedFolder;
        if (folder is null) return;
        try
        {
            _vm.Session.Database.Load(folder);
            RefreshCounts();
            DbStatusRefresh?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Reload failed: " + ex.Message, "Reload Database",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Raised so the shell can refresh its own status text after a load.</summary>
    public event Action? DbStatusRefresh;
}
