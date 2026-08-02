using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Sections;

/// <summary>
/// Resolves a display texture from the user's installed FC26 files when a loose
/// preview asset is absent.  Search/extraction runs off the UI thread and only
/// applies its result if the same record is still being viewed.
/// </summary>
internal static class FrostbitePreviewLoader
{
    // TextureAsset resource type used by the FC26 visual assets indexed by the bridge.
    private const uint TextureResType = 0x6BDE20BA;

    public static void Load(
        PictureBox viewer,
        AppServices services,
        string? localPath,
        IEnumerable<string> queries,
        Action<Image?, string?> apply,
        Func<FrostbiteAssetSession.AssetMatch, bool>? accept = null,
        bool linearColor = false)
    {
        var request = Guid.NewGuid();
        viewer.Tag = request;

        if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
        {
            apply(CreatePreview(services, localPath, viewer, linearColor), "local asset");
            return;
        }

        if (!services.FrostbiteAssets.IsAvailable)
        {
            apply(null, null);
            return;
        }

        _ = Task.Run(() => FindTexture(services, queries, accept))
            .ContinueWith(task =>
            {
                if (viewer.IsDisposed || viewer.Tag is not Guid current || current != request) return;
                var path = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
                apply(CreatePreview(services, path, viewer, linearColor), path == null ? null : "Installed game asset");
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// Loads a named FC26 legacy UI texture without falling back to a 3D
    /// material texture. This is used for minifaces, whose canonical files are
    /// data/ui/imgAssets/heads/p{playerId}.dds.
    /// </summary>
    public static void LoadLegacyUiAsset(
        PictureBox viewer, AppServices services, string? localPath,
        string legacyPath, Action<Image?, string?> apply)
        => LoadLegacyUiAssetCandidates(viewer, services, localPath, [legacyPath], apply);

    /// <summary>
    /// Loads the first available asset from a small ordered set of canonical
    /// legacy UI paths.  FC26 uses alternate UI families for some records
    /// (notably stadium cards), so this keeps the exact primary path while
    /// providing a game-native fallback without using an unrelated texture.
    /// </summary>
    public static void LoadLegacyUiAssetCandidates(
        PictureBox viewer, AppServices services, string? localPath,
        IEnumerable<string> legacyPaths, Action<Image?, string?> apply,
        Action<string>? resolvedLegacyPath = null)
    {
        var request = Guid.NewGuid();
        viewer.Tag = request;
        if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
        {
            apply(CreatePreview(services, localPath, viewer, linearColor: false), "local asset");
            return;
        }
        if (!services.FrostbiteAssets.IsAvailable)
        {
            apply(null, null);
            return;
        }
        var candidates = legacyPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _ = Task.Run(() =>
        {
            foreach (var legacyPath in candidates)
            {
                var path = services.FrostbiteAssets.ExportLegacyAsset(legacyPath);
                if (!string.IsNullOrWhiteSpace(path)) return (FilePath: path, LegacyPath: legacyPath);
            }
            return (FilePath: (string?)null, LegacyPath: (string?)null);
        })
            .ContinueWith(task =>
            {
                if (viewer.IsDisposed || viewer.Tag is not Guid current || current != request) return;
                var result = task.Status == TaskStatus.RanToCompletion
                    ? task.Result : (FilePath: (string?)null, LegacyPath: (string?)null);
                if (!string.IsNullOrWhiteSpace(result.LegacyPath))
                    resolvedLegacyPath?.Invoke(result.LegacyPath);
                apply(CreatePreview(services, result.FilePath, viewer, linearColor: false),
                    result.FilePath == null ? null : "Installed UI asset");
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static string? FindTexture(
        AppServices services,
        IEnumerable<string> queries,
        Func<FrostbiteAssetSession.AssetMatch, bool>? accept)
    {
        foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var match = services.FrostbiteAssets.SearchAssets(query, "Res", 150)
                .Where(x => x.ResType == TextureResType && x.Name.EndsWith("_color", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => Score(x.Name))
                .FirstOrDefault(x => accept?.Invoke(x) ?? true);
            if (match != null)
            {
                var path = services.FrostbiteAssets.ExportTexture(match.Name);
                if (!string.IsNullOrWhiteSpace(path)) return path;
            }
        }
        return null;
    }

    private static int Score(string name)
    {
        // A crest/logo/thumbnail is safer for an editor preview than a material map.
        if (name.Contains("/textures/logo/logo_", StringComparison.OrdinalIgnoreCase)) return 0;
        if (name.Contains("/crest_", StringComparison.OrdinalIgnoreCase)) return 1;
        if (name.Contains("thumbnail", StringComparison.OrdinalIgnoreCase)) return 2;
        if (name.Contains("preview", StringComparison.OrdinalIgnoreCase)) return 3;
        return 10;
    }

    internal static Image? CreatePreview(AppServices services, string? path, int width, int height, bool linearColor = false)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        try
        {
            var image = services.Textures.CreatePreview(path, width, height);
            if (image == null || !linearColor) return image;
            var corrected = new Bitmap(image.Width, image.Height);
            using var graphics = Graphics.FromImage(corrected);
            using var attributes = new System.Drawing.Imaging.ImageAttributes();
            // Frostbite diffuse/crest textures are stored in linear colour space.
            // Convert to sRGB for a normal WinForms viewer (otherwise City blue
            // and other bright colours appear markedly too dark/grey).
            attributes.SetGamma(1F / 2.2F);
            graphics.DrawImage(image, new Rectangle(0, 0, corrected.Width, corrected.Height),
                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            image.Dispose();
            return corrected;
        }
        catch { return null; }
    }

    private static Image? CreatePreview(AppServices services, string? path, PictureBox viewer, bool linearColor) =>
        CreatePreview(services, path, viewer.Width, viewer.Height, linearColor);
}
