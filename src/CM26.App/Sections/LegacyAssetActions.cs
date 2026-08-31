using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Sections;

internal sealed record LegacyAssetEditTarget(string LegacyPath, int Width, int Height);

internal static class LegacyAssetActions
{
    private sealed class TargetHolder
    {
        public required LegacyAssetEditTarget Target { get; init; }
    }

    // FrostbitePreviewLoader owns PictureBox.Tag for its asynchronous request
    // token. Keep editable asset metadata outside Tag so loading a preview
    // cannot silently disable Import/Remove/View.
    private static readonly ConditionalWeakTable<PictureBox, TargetHolder> Targets = new();

    public static void SetTarget(PictureBox picture, LegacyAssetEditTarget target)
    {
        Targets.Remove(picture);
        Targets.Add(picture, new TargetHolder { Target = target });
    }

    public static void ClearTarget(PictureBox picture) => Targets.Remove(picture);

    public static LegacyAssetEditTarget? GetTarget(PictureBox picture) =>
        Targets.TryGetValue(picture, out var holder) ? holder.Target : null;

    public static void Attach(
        AppServices services, Control parent, PictureBox picture, Point location,
        Action refresh, string importText = "Import", string removeText = "Remove")
    {
        var import = new Button { Text = importText, Location = location, Size = new Size(72, 25) };
        var remove = new Button { Text = removeText, Location = new Point(location.X + 78, location.Y), Size = new Size(72, 25) };
        var export = new Button { Text = "Export", Location = new Point(location.X + 156, location.Y), Size = new Size(60, 25) };
        Theme.ApplyButton(import);
        Theme.ApplyButton(remove);
        Theme.ApplyButton(export);
        import.Click += (_, _) => Import(services, picture);
        remove.Click += (_, _) =>
        {
            try
            {
                if (GetTarget(picture) is { } target && services.LegacyMods.Remove(target.LegacyPath))
                    refresh();
            }
            catch (Exception ex)
            {
                FriendlyErrorDialog.Show(picture.FindForm()!, "Remove asset", ex, "The staged asset state was left unchanged.");
            }
        };
        export.Click += (_, _) => Export(services, picture);
        parent.Controls.Add(import);
        parent.Controls.Add(remove);
        parent.Controls.Add(export);
    }

    public static string? Replacement(AppServices services, string legacyPath) =>
        services.LegacyMods.GetReplacement(legacyPath);

    private static void Import(AppServices services, PictureBox picture)
    {
        if (GetTarget(picture) is not { } target) return;
        using var dialog = new OpenFileDialog
        {
            Title = $"Import {target.LegacyPath}",
            Filter = "Texture files (*.dds;*.png;*.jpg;*.jpeg;*.bmp)|*.dds;*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(picture.FindForm()) != DialogResult.OK) return;
        try
        {
            var path = services.LegacyMods.StageImage(
                target.LegacyPath, dialog.FileName, target.Width, target.Height);
            using var image = services.Textures.CreatePreview(path, picture.Width, picture.Height);
            picture.Image?.Dispose();
            picture.Image = image == null ? null : new Bitmap(image);
        }
        catch (Exception ex)
        {
            FriendlyErrorDialog.Show(picture.FindForm()!, "Import asset", ex, "No asset was staged. Select a supported source and retry.");
        }
    }

    private static void Export(AppServices services, PictureBox picture)
    {
        if (GetTarget(picture) is not { } target) return;
        var path = services.LegacyMods.GetReplacement(target.LegacyPath)
            ?? services.FrostbiteAssets.ExportLegacyAsset(target.LegacyPath);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show(picture.FindForm(), "No installed or staged asset is available to export.", "Export asset",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Title = "Export asset",
            FileName = Path.GetFileName(path),
            Filter = "DDS texture (*.dds)|*.dds|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(picture.FindForm()) != DialogResult.OK) return;
        try { File.Copy(path, dialog.FileName, overwrite: true); }
        catch (Exception ex)
        {
            FriendlyErrorDialog.Show(picture.FindForm()!, "Export asset", ex, "No output was reported complete. Re-index the asset and retry.");
        }
    }
}
