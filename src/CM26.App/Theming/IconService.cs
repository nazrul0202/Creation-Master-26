using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace CM26.App.Theming;

/// <summary>Loads embedded section icons; provides a drawn fallback. Preserves transparency & aspect.</summary>
public static class IconService
{
    private static readonly ConcurrentDictionary<string, Image> _cache = new();
    private static readonly ConcurrentQueue<string> _cacheKeys = new();
    private const int MaxCacheEntries = 200;
    private static readonly Assembly _asm = Assembly.GetExecutingAssembly();

    // Map every logical section to an embedded icon.  The supplied Icon Section
    // assets cover the football categories; sections without a dedicated PNG use a
    // unique letter badge so every module has a visually distinct marker.
    private static readonly Dictionary<string, string> ResourceByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        // The full CM26 brand mark has an opaque black background.  At the
        // 18px sidebar size it rendered as a distracting black rectangle, so
        // navigation-only modules use the clean vector badge fallback.
        ["dashboard"] = "",
        ["players"] = "CM26.App.Assets.Icons.Player.png",
        ["teams"] = "CM26.App.Assets.Icons.Kit.png",
        ["leagues"] = "CM26.App.Assets.Icons.League.png",
        ["countries"] = "CM26.App.Assets.Icons.Country.png",
        ["managers"] = "CM26.App.Assets.Icons.Manager.png",
        ["stadiums"] = "CM26.App.Assets.Icons.Stadium.png",
        ["kits"] = "CM26.App.Assets.Icons.Kit.png",
        ["competitions"] = "CM26.App.Assets.Icons.League.png",
        ["formations"] = "CM26.App.Assets.Icons.Formation.png",
        ["transfers"] = "CM26.App.Assets.Icons.TransferMarket.png",
        ["balls"] = "CM26.App.Assets.Icons.Ball.png",
        ["boots"] = "CM26.App.Assets.Icons.Boots.png",
        // Sections without a dedicated PNG fall back to a distinct letter badge
        // (see DrawFallback) rather than reusing another section's icon.
        ["gloves"] = "",
        ["sponsors"] = "",
        ["adboards"] = "",
        ["audio"] = "",
        ["stadiumaudio"] = "",
        ["scoreboard"] = "",
        ["referees"] = "",
        ["browser"] = "",
        ["diagnostics"] = "",
        ["settings"] = "",
    };

    /// <summary>Distinct accent colour per section used for the letter badge fallback.</summary>
    private static readonly Dictionary<string, Color> BadgeColorByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gloves"] = Color.FromArgb(110, 122, 145),
        ["sponsors"] = Color.FromArgb(96, 118, 170),
        ["adboards"] = Color.FromArgb(88, 108, 120),
        ["audio"] = Color.FromArgb(0, 120, 212),
        ["stadiumaudio"] = Color.FromArgb(83, 116, 160),
        ["scoreboard"] = Color.FromArgb(72, 92, 130),
        ["referees"] = Color.FromArgb(138, 147, 162),
    };

    /// <summary>Get an icon scaled to <paramref name="size"/> keeping aspect ratio; fallback drawn if missing.</summary>
    public static Image Get(string key, int size)
    {
        var cacheKey = $"{key}:{size}";
        if (_cache.TryGetValue(cacheKey, out var cached)) return cached;

        Image? src = null;
        if (ResourceByKey.TryGetValue(key, out var file) && !string.IsNullOrEmpty(file))
        {
            using var stream = _asm.GetManifestResourceStream(file);
            if (stream != null) src = Image.FromStream(stream);
        }
        Image result;
        try
        {
            result = src != null ? ScaleToFit(src, size) : DrawFallback(key, size);
        }
        finally
        {
            src?.Dispose();
        }
        if (_cache.TryAdd(cacheKey, result))
        {
            _cacheKeys.Enqueue(cacheKey);
            while (_cache.Count > MaxCacheEntries && _cacheKeys.TryDequeue(out var oldest))
            {
                if (_cache.TryRemove(oldest, out var evicted))
                    evicted?.Dispose();
            }
            return result;
        }
        return _cache.TryGetValue(cacheKey, out var existing) ? existing : result;
    }

    private static Image ScaleToFit(Image src, int size)
    {
        var bmp = new Bitmap(size, size);
        bmp.SetResolution(96, 96);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        float scale = Math.Min((float)size / src.Width, (float)size / src.Height);
        int w = Math.Max(1, (int)(src.Width * scale));
        int h = Math.Max(1, (int)(src.Height * scale));
        g.DrawImage(src, (size - w) / 2, (size - h) / 2, w, h);
        return bmp;
    }

    private static Image DrawFallback(string key, int size)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var brush = new SolidBrush(BadgeColorByKey.TryGetValue(key, out var c) ? c : Theme.Accent);
        using var font = new Font("Segoe UI Semibold", Math.Max(6, size / 2f), FontStyle.Regular, GraphicsUnit.Pixel);
        var letter = string.IsNullOrEmpty(key) ? "?" : key.Substring(0, 1).ToUpperInvariant();
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.FillEllipse(brush, 1, 1, size - 2, size - 2);
        g.DrawString(letter, font, Brushes.White, new RectangleF(0, 0, size, size), sf);
        return bmp;
    }
}
