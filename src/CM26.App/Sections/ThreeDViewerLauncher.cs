using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Sections;

internal static class ThreeDViewerLauncher
{
    private const string ViewerName = "3D Face Viewer By Rizco98 FET Renderer.exe";

    public static void Attach(
        Control parent, Point location, string assetKind,
        Func<IReadOnlyList<string>>? tokens = null,
        Func<string?>? meshExporter = null)
    {
        var button = new Button
        {
            Text = "Open 3D Model Viewer…",
            Location = location,
            Size = new Size(175, 28)
        };
        Theme.ApplyButton(button);
        button.Click += async (_, _) => await OpenAsync(parent, assetKind, tokens?.Invoke(), meshExporter);
        parent.Controls.Add(button);
    }

    /// <summary>
    /// Replaces a never-filling "3D Model" image surface with an honest panel:
    /// an in-app 3D render is not performed, so the user gets a short note and
    /// the button that opens the exported FBX in the viewer instead. When
    /// <paramref name="meshExporter"/> is provided the button first exports the
    /// current record's mesh through the FC26 asset bridge and opens that FBX;
    /// otherwise it falls back to <paramref name="tokens"/> auto-detection and
    /// then the explicit file picker.
    /// </summary>
    public static void AttachPlaceholder(
        Control parent, Point location, Size surfaceSize, string assetKind,
        Func<IReadOnlyList<string>>? tokens = null,
        Func<string?>? meshExporter = null)
    {
        var note = new Label
        {
            Text = "No 3D preview is shown in-app.\nOpen an exported FBX model in the\nCM26 3D viewer via the button below.",
            Location = location,
            Size = surfaceSize,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Muted,
            BackColor = Theme.Raised,
            Font = Theme.Body,
            BorderStyle = BorderStyle.FixedSingle,
        };
        parent.Controls.Add(note);
        Attach(parent, new Point(location.X, location.Y + surfaceSize.Height + 7), assetKind, tokens, meshExporter);
    }

    /// <summary>
    /// Opens the viewer with the record's real FC26 mesh FBX. The mesh exporter
    /// runs first; when it yields no file (no indexed mesh, bridge offline) the
    /// user falls back to token auto-detection and finally a file picker.
    /// </summary>
    public static async Task OpenAsync(
        Control owner, string? assetKind, IReadOnlyList<string>? tokens = null,
        Func<string?>? meshExporter = null)
    {
        var executable = Path.Combine(
            AppContext.BaseDirectory, "Tools", "CM26.3DViewer", ViewerName);
        if (!File.Exists(executable))
        {
            MessageBox.Show(owner.FindForm(),
                "The packaged CM26 3D viewer is unavailable in Tools\\CM26.3DViewer.",
                "CM26 3D Viewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Preferred route: export the record's real FC26 mesh through the
        // bridge, then show exactly that FBX in the viewer.
        string? launchPath = null;
        if (meshExporter != null)
        {
            try
            {
                launchPath = await Task.Run(meshExporter).ConfigureAwait(true) ?? string.Empty;
            }
            catch
            {
                // Mesh resolution/export is best-effort; the fallbacks below
                // keep the viewer button usable even without the game assets.
            }
        }
        if (string.IsNullOrWhiteSpace(launchPath) || !File.Exists(launchPath))
            launchPath = AutoDetect(tokens);
        if (string.IsNullOrWhiteSpace(launchPath))
        {
            using var dialog = new OpenFileDialog
            {
                Title = $"Select an exported {assetKind ?? "3D"} FBX model",
                Filter = "3D model (*.fbx)|*.fbx|All files (*.*)|*.*",
                CheckFileExists = true
            };
            if (dialog.ShowDialog(owner.FindForm()) != DialogResult.OK) return;
            launchPath = dialog.FileName;
        }

        try
        {
            var start = new ProcessStartInfo(executable)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(executable)
            };
            start.ArgumentList.Add(launchPath);
            Process.Start(start);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner.FindForm(), ex.Message, "CM26 3D Viewer",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Scans the known FC26 FILE TOOL export locations plus the user asset pack
    /// for an FBX whose file name contains any of the supplied record tokens
    /// (ids/names of the currently selected record). Returns empty when nothing
    /// matches so the caller falls back to the explicit file picker.
    /// </summary>
    private static string AutoDetect(IReadOnlyList<string>? tokens)
    {
        if (tokens == null) return string.Empty;
        var haystack = tokens
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (haystack.Length == 0) return string.Empty;

        foreach (var root in KnownExportRoots())
        {
            foreach (var token in haystack)
            {
                try
                {
                    var hit = new DirectoryInfo(root)
                        .EnumerateFiles("*" + token + "*.fbx", SearchOption.AllDirectories)
                        .FirstOrDefault();
                    if (hit != null) return hit.FullName;
                }
                catch { /* an inaccessible optional export folder is skipped. */ }
            }
        }
        return string.Empty;
    }

    private static IReadOnlyList<string> KnownExportRoots()
    {
        var roots = new List<string>();
        if (!string.IsNullOrWhiteSpace(SettingsService.AssetRoot))
            roots.Add(SettingsService.AssetRoot);
        roots.Add(Path.Combine(AppContext.BaseDirectory, "ExportedAssets", "Models"));
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            roots.Add(Path.Combine(drive.RootDirectory.FullName, "FC26 FILE TOOL"));
        return roots.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}