using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CM26.Studio.Services;

namespace CM26.Studio;

/// <summary>
/// Renders the actual CM16 shell and every section after loading the installed
/// FC26 database. These images are the visual parity evidence used while
/// porting the original CreationMaster *Form.cs layouts.
/// </summary>
public static class VisualAudit
{
    public static async Task RunAsync(string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);
        using var session = new AppSession();
        var window = new MainWindow { SmokeSession = session };
        window.Show();
        await SettleAsync(window, 500);
        Capture(window, Path.Combine(outputFolder, "00-shell-closed.png"));

        if (!session.TryOpenGame(out var message, new Progress<string>(_ => { })))
            throw new InvalidOperationException("FC26 database did not load for visual audit: " + message);

        // Reproduce the state applied by File > Open and then render each
        // visible CM16 toolbar section through its real Checked handler.
        var controls = FindAll(window).ToArray();
        var sectionButtons = controls.OfType<RadioButton>()
            .Where(button => button.GroupName == "Sec" && button.Visibility == Visibility.Visible)
            .ToArray();
        var sequence = 1;
        foreach (var button in sectionButtons)
        {
            button.IsEnabled = true;
            button.IsChecked = false;
            button.IsChecked = true;
            await SettleAsync(window, 180);
            var recordList = FindAll(window).OfType<ListView>()
                .Where(list => list.Items.Count > 0)
                .OrderByDescending(list => list.Items.Count)
                .FirstOrDefault();
            if (recordList != null) recordList.SelectedIndex = 0;
            await SettleAsync(window, 120);
            var key = Sanitize(button.Tag?.ToString() ?? $"section-{sequence}");
            Capture(window, Path.Combine(outputFolder, $"{sequence:00}-{key}.png"));
            sequence++;
        }

        var report = new List<string>
        {
            "Creation Master 26 WPF visual audit",
            $"Generated: {DateTimeOffset.Now:O}",
            $"FC26: {session.FrostbiteAssets.GameRoot}",
            $"Sections: {sectionButtons.Length}",
        };
        var teams = session.Database.GetTable("teams");
        if (teams != null)
        {
            report.Add($"teams: rows={teams.RowCount}, columns={teams.Columns.Count}");
            report.Add("team fields: " + string.Join(" | ", teams.Columns.Select(column => column.Name)));
        }
        File.WriteAllLines(Path.Combine(outputFolder, "audit.txt"), report);
        window.Close();
    }

    private static async Task SettleAsync(Window window, int milliseconds)
    {
        var cycles = Math.Max(1, milliseconds / 50);
        for (var i = 0; i < cycles; i++)
        {
            await Task.Delay(50);
            window.UpdateLayout();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle, () => { });
        }
    }

    private static void Capture(Window window, string path)
    {
        window.UpdateLayout();
        var width = Math.Max(1, (int)Math.Ceiling(window.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(window.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static IEnumerable<DependencyObject> FindAll(DependencyObject root)
    {
        var stack = new Stack<DependencyObject>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count; i++) stack.Push(VisualTreeHelper.GetChild(node, i));
        }
    }

    private static string Sanitize(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '-');
        return value.ToLowerInvariant();
    }
}
