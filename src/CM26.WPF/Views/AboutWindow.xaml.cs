using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style About dialog: product name, release, a Go Back button and
/// links to the project pages. Mirrors the FifaControls.AboutForm layout.
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        LabelRelease.Text = "Release v" + ReadVersion();
        LabelBody.Text = "Creation Master 26 is a CM16-style database editor for EA Sports FC 26. " +
                         "It edits the live FC26 database directly (squad, team and career data) with " +
                         "automatic backups, in the same spirit as the original Creation Master.";
    }

    private static string ReadVersion()
    {
        try
        {
            var root = AppContext.BaseDirectory;
            for (var dir = new DirectoryInfo(root); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "version.json");
                if (!File.Exists(candidate)) continue;
                var json = File.ReadAllText(candidate);
                var marker = "\"version\": \"";
                var start = json.IndexOf(marker, System.StringComparison.Ordinal);
                if (start < 0) break;
                start += marker.Length;
                var end = json.IndexOf('"', start);
                if (end > start) return json.Substring(start, end - start);
            }
        }
        catch
        {
            // best effort; fall back to a bare release label
        }
        return "1.0.119";
    }

    private void GoBack_Click(object sender, RoutedEventArgs e) => Close();

    private void GitHub_Click(object sender, MouseButtonEventArgs e) =>
        OpenUrl("https://github.com/nazrul0202/Creation-Master-26");

    private void Releases_Click(object sender, MouseButtonEventArgs e) =>
        OpenUrl("https://github.com/nazrul0202/Creation-Master-26/releases");

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // best effort
        }
    }
}
