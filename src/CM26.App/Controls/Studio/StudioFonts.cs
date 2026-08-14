using System.Drawing;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Shared font cache for the Studio UI. Prevents hundreds of unmanaged Font
/// objects from being created across cards, badges and rows.
/// </summary>
public static class StudioFonts
{
    public const string Family = "Segoe UI";

    private static readonly Dictionary<(float, FontStyle), Font> Cache = new();

    public static Font Get(float size, FontStyle style = FontStyle.Regular)
    {
        var key = (size, style);
        lock (Cache)
        {
            if (!Cache.TryGetValue(key, out var font))
            {
                font = new Font(Family, size, style, GraphicsUnit.Point);
                Cache[key] = font;
            }
            return font;
        }
    }

    public static Font AppTitle => Get(20f, FontStyle.Bold);
    public static Font SectionTitle => Get(18f, FontStyle.Bold);
    public static Font CardTitle => Get(13f, FontStyle.Bold);
    public static Font CardSubtitle => Get(11f, FontStyle.Regular);
    public static Font MetricValue => Get(26f, FontStyle.Bold);
    public static Font MetricLabel => Get(9f, FontStyle.Regular);
    public static Font DataValue => Get(10f, FontStyle.Bold);
    public static Font DataLabel => Get(9f, FontStyle.Regular);
    public static Font Metadata => Get(8.5f, FontStyle.Regular);
    public static Font Button => Get(9.5f, FontStyle.Bold);
    public static Font Chip => Get(9f, FontStyle.Bold);
    public static Font RowPrimary => Get(10.5f, FontStyle.Regular);
    public static Font RowSecondary => Get(9f, FontStyle.Regular);
    public static Font Badge => Get(10f, FontStyle.Bold);
}
