using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CM26.Studio;

public partial class App : System.Windows.Application
{
    public App()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            try
            {
                File.AppendAllText(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Creation Master 26", "studio-crash.log"),
                    $"[{DateTimeOffset.Now:O}] {e.Exception}\n\n");
            }
            catch { }
            MessageBox.Show("CM26 Studio crashed:\n\n" + e.Exception, "CM26 Studio",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
        Startup += (_, e) =>
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && args[1] == "--ui-smoke")
            {
                Dispatcher.BeginInvoke(new System.Action(async () =>
                {
                    try
                    {
                        await Smoke.RunAsync();
                        Shutdown(0);
                    }
                    catch (Exception ex)
                    {
                        try { File.AppendAllText(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Creation Master 26", "studio-crash.log"),
                            $"[{DateTimeOffset.Now:O}] SMOKE FAILED {ex}\n\n"); } catch { }
                        Shutdown(1);
                    }
                }));
            }
            else
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
        };
    }
}