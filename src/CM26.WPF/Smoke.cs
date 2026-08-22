using System.IO;
using System.Windows;
using System.Windows.Controls;
using CM26.Studio.Services;
using CM26.Studio.Views;

namespace CM26.Studio;

/// <summary>
/// Headless-ish UI smoke: opens the real MainWindow, opens the workspace
/// database through the same "Open FC26" flow the menu uses, forces
/// layout/render passes and exercises navigation. Any unhandled exception
/// fails the run.
/// </summary>
public static class Smoke
{
    /// <summary>
    /// Fast release smoke that requires no game installation. It constructs and
    /// renders every visible section so missing XAML resources fail immediately.
    /// </summary>
    public static Task RunShellAsync()
    {
        Report("constructing Studio shell (no-game mode)");
        using var session = new AppSession();
        var window = new MainWindow { SmokeSession = session };
        window.Show();
        try
        {
            window.UpdateLayout();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });

            var sections = FindAll(window).OfType<RadioButton>()
                .Where(r => r.GroupName == "Sec").ToArray();
            if (sections.Length == 0)
                throw new InvalidOperationException("Studio shell contains no section navigation controls.");

            foreach (var section in sections)
            {
                Report($"rendering section {section.Tag ?? section.Name}");
                section.IsEnabled = true;
                section.IsChecked = true;
                window.UpdateLayout();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });
            }

            Report($"SHELL SMOKE OK: rendered {sections.Length} sections without a game installation");
            return Task.CompletedTask;
        }
        finally
        {
            window.Close();
        }
    }

    public static async Task RunAsync()
    {
        var started = System.Diagnostics.Stopwatch.StartNew();
        Report("constructing Studio shell");
        var session = new AppSession();
        using (session)
        {
            var window = new MainWindow();
            window.SmokeSession = session;
            window.Show();

            // Let OnLoaded set the CM16 pre-load state, then invoke the actual
            // File > Open FC26 menu item. This locks in the regression where
            // automatic loading disabled Open and made the command unclickable.
            for (var i = 0; i < 40; i++)
            {
                await Task.Delay(50);
                window.UpdateLayout();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });
            }

            if (!window.MenuOpenFifa16.IsEnabled)
                throw new InvalidOperationException("File > Open FC26 is disabled before the smoke click.");
            Report("opening FC26 workspace");
            window.MenuOpenFifa16.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            // A cold Frostbite index can take several minutes on a mechanical
            // disk. Keep a hard five-minute limit and print progress so release
            // automation never appears to hang silently.
            for (var i = 0; i < 6000 &&
                 (!session.Database.IsLoaded || !window.MenuOpenFifa16.IsEnabled); i++)
            {
                await Task.Delay(50);
                if (i > 0 && i % 300 == 0)
                    Report($"still loading FC26 workspace ({started.Elapsed.TotalSeconds:N0}s)");
                window.UpdateLayout();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });
            }
            if (!session.Database.IsLoaded)
                throw new InvalidOperationException("Open FC26 click did not load the database and Frostbite assets.");
            if (!window.MenuOpenFifa16.IsEnabled)
                throw new InvalidOperationException("File > Open FC26 stayed disabled after loading.");
            Report($"workspace loaded ({started.Elapsed.TotalSeconds:N1}s); reloading database");
            window.UpdateLayout();

            // Simulate the "Reload Database" path on the UI thread.
            string message;
            if (!session.ReloadFromGame(out message))
                throw new InvalidOperationException("Reload failed in smoke run: " + message);
            window.UpdateLayout();
            await Task.Delay(200);
            window.UpdateLayout();

            // Navigate every section like a user would.
            Report("navigating every visible section");
            foreach (var rb in FindAll(window).OfType<RadioButton>().Where(r => r.GroupName == "Sec"))
            {
                rb.IsChecked = true;
                window.UpdateLayout();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });
                await Task.Delay(50);
            }

            // Select the first player so the playstyle checkbox grids render
            // (traits/virtual-pro bitmask path) — any exception here fails the run.
            var players = FindAll(window).OfType<Views.PlayersView>().FirstOrDefault();
            if (players != null)
            {
                var list = players.FindName("PlayerList") as System.Windows.Controls.ListView;
                if (list != null && list.Items.Count > 0)
                {
                    list.SelectedIndex = 0;
                    window.UpdateLayout();
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });
                    await Task.Delay(100);
                    window.UpdateLayout();
                }
            }

            Report($"SMOKE OK: database loaded, layout passed, navigation passed ({started.Elapsed.TotalSeconds:N1}s)");
        }
    }

    private static void Report(string message)
    {
        Console.WriteLine($"[ui-smoke {DateTime.Now:HH:mm:ss}] {message}");
        Console.Out.Flush();
    }

    private static IEnumerable<DependencyObject> FindAll(DependencyObject root)
    {
        var stack = new Stack<DependencyObject>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count; i++)
                stack.Push(System.Windows.Media.VisualTreeHelper.GetChild(node, i));
        }
    }
}
