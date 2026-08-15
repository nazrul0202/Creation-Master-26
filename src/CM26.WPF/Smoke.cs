using System.IO;
using System.Windows;
using System.Windows.Controls;
using CM26.Studio.Services;
using CM26.Studio.Views;

namespace CM26.Studio;

/// <summary>
/// Headless-ish UI smoke: opens the real MainWindow, opens the workspace
/// database through the same "Open - FIFA 16" flow the menu uses, forces
/// layout/render passes and exercises navigation. Any unhandled exception
/// fails the run.
/// </summary>
public static class Smoke
{
    public static async Task RunAsync()
    {
        var session = new AppSession();
        using (session)
        {
            var window = new MainWindow();
            window.SmokeSession = session;
            window.Show();

            // Let OnLoaded set the CM16 pre-load state, then open the database
            // through the same path as the File > Open - FIFA 16 menu item.
            for (var i = 0; i < 40; i++)
            {
                await Task.Delay(50);
                window.UpdateLayout();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });
            }

            string message;
            var loaded = session.TryOpenGame(out message, new Progress<string>(_ => { }));
            if (!loaded)
                throw new InvalidOperationException("FC26 database did not load in smoke run: " + message);
            window.UpdateLayout();

            // Simulate the "Reload Database" path on the UI thread.
            if (!session.ReloadFromGame(out message))
                throw new InvalidOperationException("Reload failed in smoke run: " + message);
            window.UpdateLayout();
            await Task.Delay(200);
            window.UpdateLayout();

            // Navigate every section like a user would.
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

            Console.WriteLine("SMOKE OK: database loaded, layout passed, navigation passed");
        }
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