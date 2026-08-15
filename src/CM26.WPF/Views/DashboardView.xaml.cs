using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Studio.Services;

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
            DbPathText.Text = "FC26 is not open yet";
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
            HeroSubtitle.Text = "Direct FC26 editing - extracted from the game archives";
        }
        catch (Exception ex)
        {
            DbPathText.Text = "Count failed: " + ex.Message;
        }
    }

    private void OpenGame_Click(object sender, RoutedEventArgs e)
    {
        var progress = new Progress<string>(phase => HeroSubtitle.Text = phase);
        try
        {
            string message = string.Empty;
            var ok = Task.Run(() => _vm.Session.TryOpenGame(out message, progress)).Result;
            HeroSubtitle.Text = ok ? "FC26 opened for direct editing." : message;
            if (!ok)
                MessageBox.Show(message, "Open FC26", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                RefreshCounts();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Open FC26 failed: " + ex.Message, "Open FC26",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        var progress = new Progress<string>(phase => HeroSubtitle.Text = phase);
        try
        {
            string message = string.Empty;
            var ok = Task.Run(() => _vm.Session.ReloadFromGame(out message)).Result;
            HeroSubtitle.Text = message;
            if (ok) RefreshCounts();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Reload failed: " + ex.Message, "Reload Database",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
