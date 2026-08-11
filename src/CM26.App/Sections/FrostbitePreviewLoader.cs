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
                var path = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
                var preview = CreatePreview(services, path, viewer, linearColor);
                if (viewer.IsDisposed || viewer.Tag is not Guid current || current != request)
                {
                    preview?.Dispose();
                    return;
                }
                apply(preview, path == null ? null : "Installed game asset");
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
                try
                {
                    var path = services.FrostbiteAssets.ExportLegacyAsset(legacyPath);
                    if (!string.IsNullOrWhiteSpace(path)) return (FilePath: path, LegacyPath: legacyPath);
                }
                catch (FileNotFoundException)
                {
                    // Try the next UI variant (for example dark before light).
                }
                catch (DirectoryNotFoundException)
                {
                    // The collection is not present in this FC installation.
                }
                catch (InvalidOperationException)
                {
                    // Asset bridge unavailable for this candidate; preserve the rest.
                }
            }
            return (FilePath: (string?)null, LegacyPath: (string?)null);
        })
            .ContinueWith(task =>
            {
                var result = task.Status == TaskStatus.RanToCompletion
                    ? task.Result : (FilePath: (string?)null, LegacyPath: (string?)null);
                var preview = CreatePreview(services, result.FilePath, viewer, linearColor: false);
                if (viewer.IsDisposed || viewer.Tag is not Guid current || current != request)
                {
                    preview?.Dispose();
                    return;
                }
                if (!string.IsNullOrWhiteSpace(result.LegacyPath))
                    resolvedLegacyPath?.Invoke(result.LegacyPath);
                apply(preview, result.FilePath == null ? null : "Installed UI asset");
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
            if (image.Width <= 0 || image.Height <= 0) { image.Dispose(); return null; }
            var corrected = new Bitmap(image.Width, image.Height);
            using var graphics = Graphics.FromImage(corrected);
            using var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetGamma(1F / 2.2F);
            graphics.DrawImage(image, new Rectangle(0, 0, corrected.Width, corrected.Height),
                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            image.Dispose();
            return corrected;
        }
        catch (System.AccessViolationException) { return null; }
        catch { return null; }
    }

    private static Image? CreatePreview(AppServices services, string? path, PictureBox viewer, bool linearColor)
    {
        // The app is PerMonitorV2 DPI-aware: viewer.Width/Height are LOGICAL pixels.
        // Pre-scale to physical pixels so images stay sharp on 125%/150%/200% displays.
        float scale = viewer.DeviceDpi / 96f;
        int w = Math.Max(1, (int)(viewer.Width * scale));
        int h = Math.Max(1, (int)(viewer.Height * scale));
        return CreatePreview(services, path, w, h, linearColor);
    }
}
