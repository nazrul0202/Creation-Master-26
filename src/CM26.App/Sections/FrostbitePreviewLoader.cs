using System.Drawing;
using System.Windows.Forms;
using CM26.Application.Services;

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

    private sealed record PreviewRequest(Guid Id, CancellationTokenSource Cancellation);

    public static void Load(
        PictureBox viewer,
        AppServices services,
        string? localPath,
        IEnumerable<string> queries,
        Action<Image?, string?> apply,
        Func<FrostbiteAssetSession.AssetMatch, bool>? accept = null,
        bool linearColor = false)
    {
        var request = BeginRequest(viewer);
        var size = PreviewSize(viewer);
        var hasLocal = !string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath);

        if (!hasLocal && !services.FrostbiteAssets.IsAvailable)
        {
            apply(null, null);
            return;
        }

        _ = Task.Run(() =>
            {
                request.Cancellation.Token.ThrowIfCancellationRequested();
                var path = hasLocal ? localPath : FindTexture(services, queries, accept, request.Cancellation.Token);
                request.Cancellation.Token.ThrowIfCancellationRequested();
                var preview = CreatePreview(services, path, size.Width, size.Height, linearColor);
                return (Path: path, Preview: preview);
            }, request.Cancellation.Token)
            .ContinueWith(task =>
            {
                var result = task.Status == TaskStatus.RanToCompletion
                    ? task.Result : (Path: (string?)null, Preview: (Image?)null);
                Deliver(viewer, request, result.Preview, () =>
                {
                    apply(result.Preview, result.Path == null ? null : hasLocal ? "local asset" : "Installed game asset");
                });
            }, TaskScheduler.Default);
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
        var request = BeginRequest(viewer);
        var size = PreviewSize(viewer);
        var hasLocal = !string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath);
        if (!hasLocal && !services.FrostbiteAssets.IsAvailable)
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
            if (hasLocal)
                return (FilePath: localPath, LegacyPath: (string?)null,
                    Preview: CreatePreview(services, localPath, size.Width, size.Height));
            foreach (var legacyPath in candidates)
            {
                request.Cancellation.Token.ThrowIfCancellationRequested();
                try
                {
                    var path = services.FrostbiteAssets.ExportLegacyAsset(legacyPath);
                    if (!string.IsNullOrWhiteSpace(path))
                        return (FilePath: path, LegacyPath: legacyPath,
                            Preview: CreatePreview(services, path, size.Width, size.Height));
                }
                catch (FileNotFoundException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CM26] Preview asset not found: {ex.Message}");
                    // Try the next UI variant (for example dark before light).
                }
                catch (DirectoryNotFoundException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CM26] Preview asset directory missing: {ex.Message}");
                    // The collection is not present in this FC installation.
                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CM26] Preview asset bridge unavailable: {ex.Message}");
                    // Asset bridge unavailable for this candidate; preserve the rest.
                }
            }
            return (FilePath: (string?)null, LegacyPath: (string?)null, Preview: (Image?)null);
        }, request.Cancellation.Token)
            .ContinueWith(task =>
            {
                var result = task.Status == TaskStatus.RanToCompletion
                    ? task.Result : (FilePath: (string?)null, LegacyPath: (string?)null, Preview: (Image?)null);
                Deliver(viewer, request, result.Preview, () =>
                {
                    if (!string.IsNullOrWhiteSpace(result.LegacyPath))
                        resolvedLegacyPath?.Invoke(result.LegacyPath);
                    apply(result.Preview, result.FilePath == null ? null : hasLocal ? "local asset" : "Installed UI asset");
                });
            }, TaskScheduler.Default);
    }

    private static void Deliver(PictureBox viewer, PreviewRequest request, Image? preview, Action apply)
    {
        if (viewer.IsDisposed || !viewer.IsHandleCreated || !ReferenceEquals(viewer.Tag, request))
        {
            preview?.Dispose();
            return;
        }
        try
        {
            viewer.BeginInvoke((Action)(() =>
            {
                if (viewer.IsDisposed || !ReferenceEquals(viewer.Tag, request))
                {
                    preview?.Dispose();
                    return;
                }
                apply();
            }));
        }
        catch (InvalidOperationException)
        {
            preview?.Dispose();
        }
    }

    private static PreviewRequest BeginRequest(PictureBox viewer)
    {
        if (viewer.Tag is PreviewRequest previous)
        {
            previous.Cancellation.Cancel();
            previous.Cancellation.Dispose();
        }
        var request = new PreviewRequest(Guid.NewGuid(), new CancellationTokenSource());
        viewer.Tag = request;
        return request;
    }

    private static Size PreviewSize(PictureBox viewer)
    {
        float scale = viewer.DeviceDpi / 96f;
        return new Size(
            Math.Max(1, (int)(viewer.Width * scale)),
            Math.Max(1, (int)(viewer.Height * scale)));
    }

    private static string? FindTexture(
        AppServices services,
        IEnumerable<string> queries,
        Func<FrostbiteAssetSession.AssetMatch, bool>? accept,
        CancellationToken cancellationToken)
    {
        foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
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

}
