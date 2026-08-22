using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CM26.Studio;

public partial class App : System.Windows.Application
{
    internal static Exception? UiAutomationException { get; private set; }

    public App()
    {
        var args = Environment.GetCommandLineArgs();
        var isUiAutomation = args.Length >= 2 && args[1] is "--ui-smoke" or "--ui-shell-smoke" or "--ui-audit";
        if (isUiAutomation)
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        UiAutomationException = null;
        DispatcherUnhandledException += (_, e) =>
        {
            if (isUiAutomation)
            {
                UiAutomationException ??= e.Exception;
                e.Handled = true;
                return;
            }
            try
            {
                File.AppendAllText(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Creation Master 26", "studio-crash.log"),
                    $"[{DateTimeOffset.Now:O}] {e.Exception}\n\n");
            }
            catch (Exception logEx) { System.Diagnostics.Debug.WriteLine($"[CM26] studio-crash.log write failed: {logEx.Message}"); }
            MessageBox.Show("CM26 Studio crashed:\n\n" + e.Exception, "CM26 Studio",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
        Startup += (_, e) =>
        {
            if (args.Length >= 2 && args[1] is "--ui-smoke" or "--ui-shell-smoke" or "--ui-audit")
            {
                Dispatcher.BeginInvoke(new System.Action(async () =>
                {
                    try
                    {
                        if (args[1] == "--ui-audit")
                        {
                            var output = args.Length >= 3
                                ? args[2]
                                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "Creation Master 26", "ui-audit");
                            await VisualAudit.RunAsync(output);
                        }
                        else if (args[1] == "--ui-shell-smoke")
                        {
                            await Smoke.RunShellAsync();
                        }
                        else
                        {
                            await Smoke.RunAsync();
                        }
                        if (UiAutomationException is not null)
                            throw new InvalidOperationException(
                                "Unhandled WPF exception occurred during UI automation.", UiAutomationException);
                        Shutdown(0);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("UI SMOKE FAILED: " + ex);
                        Console.Error.Flush();
                        try { File.AppendAllText(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Creation Master 26", "studio-crash.log"),
                            $"[{DateTimeOffset.Now:O}] SMOKE FAILED {ex}\n\n"); }
                        catch (Exception logEx) { System.Diagnostics.Debug.WriteLine($"[CM26] studio-crash.log write failed (smoke): {logEx.Message}"); }
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

    /// <summary>
    /// Loads App.xaml when Studio is hosted by CM26_by_Rizco98.exe. The normal
    /// WPF-generated entry point calls InitializeComponent itself; a referenced
    /// WPF assembly does not, so the host must call this before Run().
    /// </summary>
    public void InitializeForHost() => InitializeComponent();
}
