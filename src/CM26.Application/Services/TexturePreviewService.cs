using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CM26.Application.Services;

/// <summary>
/// Default <see cref="ITexturePreviewService"/>. Handles standard formats (PNG/JPEG/BMP/GIF/WEBP-
/// via-GDI+) through System.Drawing, and DDS through the self-contained <see cref="DdsDecoder"/>.
/// Read-only and thread-safe: the source file is opened with <see cref="FileShare.ReadWrite"/> and
/// fully read into memory, then closed before decoding, so it is never locked. A small bounded
/// thumbnail cache avoids re-decoding on repeat selection.
/// </summary>
public sealed class TexturePreviewService : ITexturePreviewService
{
    private static readonly HashSet<string> GdiExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff" };

    // Bounded thumbnail cache: key = path|w|h|lastWriteTicks. Prevents unbounded memory growth.
    private readonly Dictionary<string, Image> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _lru = new();
    private readonly object _gate = new();
    private const int MaxCacheEntries = 256;

    public bool CanOpen(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;
        var ext = Path.GetExtension(filePath);
        return ext.Equals(".dds", StringComparison.OrdinalIgnoreCase) || GdiExtensions.Contains(ext);
    }

    public TextureMetadata ReadMetadata(string filePath)
    {
        var meta = new TextureMetadata { FilePath = filePath };
        try
        {
            if (!File.Exists(filePath))
                return meta with { IsReadable = false, Error = "File not found" };

            var fi = new FileInfo(filePath);
            meta = meta with { FileSizeBytes = fi.Length };
            var ext = Path.GetExtension(filePath);

            if (ext.Equals(".dds", StringComparison.OrdinalIgnoreCase))
            {
                // DDS files with a DX10 pixel-format extension have a 148-byte
                // header.  Reading only the legacy 128 bytes made every FC26
                // texture exported with its exact DXGI format look invalid.
                var head = ReadBytes(filePath, 148);
                if (head == null || !DdsDecoder.TryReadHeader(head, out var info))
                    return meta with { IsReadable = false, Error = "Not a valid DDS file" };
                return meta with
                {
                    IsReadable = info.IsSupported,
                    Error = info.IsSupported ? null : $"Unsupported DDS format: {info.Format}",
                    Width = info.Width,
                    Height = info.Height,
                    MipLevels = info.MipLevels,
                    Format = "DDS/" + info.Format,
                    HasAlpha = info.HasAlpha,
                };
            }

            if (GdiExtensions.Contains(ext))
            {
                using var img = Image.FromFile(filePath);
                return meta with
                {
                    IsReadable = true,
                    Width = img.Width,
                    Height = img.Height,
                    MipLevels = 1,
                    Format = ext.TrimStart('.').ToUpperInvariant(),
                    HasAlpha = Image.IsAlphaPixelFormat(img.PixelFormat),
                };
            }

            return meta with { IsReadable = false, Error = $"Unsupported extension: {ext}" };
        }
        catch (Exception ex)
        {
            return meta with { IsReadable = false, Error = ex.Message };
        }
    }

    public Image? CreatePreview(string filePath, int maximumWidth, int maximumHeight, CancellationToken cancellationToken = default)
    {
        if (maximumWidth <= 0) maximumWidth = 256;
        if (maximumHeight <= 0) maximumHeight = 256;
        string cacheKey = CacheKey(filePath, maximumWidth, maximumHeight);

        lock (_gate)
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                Touch(cacheKey);
                return (Image)cached.Clone();
            }
        }

        Image? decoded = null;
        try
        {
            if (!File.Exists(filePath)) return null;
            var ext = Path.GetExtension(filePath);
            if (ext.Equals(".dds", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = ReadAllBytes(filePath);
                if (bytes == null || !DdsDecoder.TryReadHeader(bytes, out var info) || !info.IsSupported)
                    return null;
                cancellationToken.ThrowIfCancellationRequested();
                decoded = DdsDecoder.DecodeToBitmap(bytes, in info, cancellationToken);
            }
            else if (GdiExtensions.Contains(ext))
            {
                // Read fully into memory so the source file is not locked by the returned image.
                var bytes = ReadAllBytes(filePath);
                if (bytes == null) return null;
                using var ms = new MemoryStream(bytes, writable: false);
                using var tmp = Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: true);
                decoded = new Bitmap(tmp); // detach from the stream
            }
            else
            {
                return null;
            }
        }
        catch (OperationCanceledException) { decoded?.Dispose(); throw; }
        catch { decoded?.Dispose(); return null; }

        if (decoded == null) return null;
        cancellationToken.ThrowIfCancellationRequested();

        var scaled = ScaleToFit(decoded, maximumWidth, maximumHeight);
        decoded.Dispose();

        lock (_gate)
        {
            if (!_cache.ContainsKey(cacheKey))
            {
                _cache[cacheKey] = (Image)scaled.Clone();
                _lru.AddFirst(cacheKey);
                EvictIfNeeded();
            }
        }
        return scaled;
    }

    private static Image ScaleToFit(Image src, int maxW, int maxH)
    {
        double ratio = Math.Min((double)maxW / src.Width, (double)maxH / src.Height);
        // Never upscale beyond 2x to keep small icons crisp; always allow downscale.
        ratio = Math.Min(ratio, 2.0);
        int w = Math.Max(1, (int)Math.Round(src.Width * ratio));
        int h = Math.Max(1, (int)Math.Round(src.Height * ratio));
        var dest = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
        using (var g = Graphics.FromImage(dest))
        {
            g.CompositingMode = CompositingMode.SourceCopy; // preserve alpha exactly
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(src, new Rectangle(0, 0, w, h), new Rectangle(0, 0, src.Width, src.Height), GraphicsUnit.Pixel);
        }
        return dest;
    }

    private static string CacheKey(string path, int w, int h)
    {
        long ticks = 0;
        try { ticks = File.GetLastWriteTimeUtc(path).Ticks; }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Texture timestamp unavailable: " + ex.Message); }
        return $"{path}|{w}x{h}|{ticks}";
    }

    private void Touch(string key)
    {
        _lru.Remove(key);
        _lru.AddFirst(key);
    }

    private void EvictIfNeeded()
    {
        while (_cache.Count > MaxCacheEntries && _lru.Last != null)
        {
            var last = _lru.Last.Value;
            _lru.RemoveLast();
            if (_cache.Remove(last, out var img)) img.Dispose();
        }
        // Drop entries whose source file vanished (keeps cache honest).
    }

    private static byte[]? ReadBytes(string path, int count)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var buf = new byte[count];
            int read = fs.Read(buf, 0, count);
            return read == count ? buf : buf;
        }
        catch { return null; }
    }

    private static byte[]? ReadAllBytes(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream((int)Math.Min(fs.Length, int.MaxValue));
            fs.CopyTo(ms);
            return ms.ToArray();
        }
        catch { return null; }
    }

    /// <summary>Clear the thumbnail cache and dispose cached images.</summary>
    public void ClearCache()
    {
        lock (_gate)
        {
            foreach (var img in _cache.Values) img.Dispose();
            _cache.Clear();
            _lru.Clear();
        }
    }
}
