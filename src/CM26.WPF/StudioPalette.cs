using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CM26.Studio;

/// <summary>Studio palette mirrors of the WinForms StudioColors, as WPF brushes.</summary>
public static class StudioPalette
{
    public static readonly Color AppBackground = Color.FromRgb(0x0D, 0x10, 0x14);
    public static readonly Color Surface = Color.FromRgb(0x15, 0x1A, 0x21);
    public static readonly Color RaisedSurface = Color.FromRgb(0x1D, 0x25, 0x30);
    public static readonly Color InputBackground = Color.FromRgb(0x11, 0x16, 0x1C);
    public static readonly Color CardBorder = Color.FromRgb(0x2D, 0x39, 0x47);
    public static readonly Color Divider = Color.FromRgb(0x23, 0x2D, 0x38);

    public static readonly Color PrimaryText = Color.FromRgb(0xF3, 0xF6, 0xF8);
    public static readonly Color MutedText = Color.FromRgb(0x8E, 0x9A, 0xA8);
    public static readonly Color DisabledText = Color.FromRgb(0x5A, 0x66, 0x74);

    public static readonly Color CyanAccent = Color.FromRgb(0x38, 0xBD, 0xF8);
    public static readonly Color CyanAccentHover = Color.FromRgb(0x0E, 0xA5, 0xE8);

    public static readonly Color Green = Color.FromRgb(0x84, 0xCC, 0x16);
    public static readonly Color GreenSoft = Color.FromRgb(0x22, 0x3D, 0x11);
    public static readonly Color Yellow = Color.FromRgb(0xF5, 0x9E, 0x0B);
    public static readonly Color YellowSoft = Color.FromRgb(0x3D, 0x2E, 0x0A);
    public static readonly Color Orange = Color.FromRgb(0xF9, 0x73, 0x16);
    public static readonly Color OrangeSoft = Color.FromRgb(0x3D, 0x1E, 0x0A);
    public static readonly Color Red = Color.FromRgb(0xEF, 0x44, 0x44);
    public static readonly Color RedSoft = Color.FromRgb(0x3D, 0x12, 0x12);
    public static readonly Color Purple = Color.FromRgb(0xA8, 0x55, 0xF7);
    public static readonly Color PurpleSoft = Color.FromRgb(0x2D, 0x1A, 0x47);

    /// <summary>Returns a rating brush for the 0-99 EA scale.</summary>
    public static SolidColorBrush RatingBrush(int rating) => new(RatingColor(rating));
    public static Color RatingColor(int rating)
    {
        return rating switch
        {
            >= 90 => Green,
            >= 80 => Color.FromRgb(0xA3, 0xE6, 0x35),
            >= 75 => Yellow,
            >= 70 => Color.FromRgb(0xFB, 0x92, 0x3C),
            >= 60 => Orange,
            _ => Red,
        };
    }
    public static Color RatingSoftColor(int rating)
    {
        return rating switch
        {
            >= 90 => GreenSoft,
            >= 80 => Color.FromRgb(0x2A, 0x3D, 0x0F),
            >= 75 => YellowSoft,
            >= 70 => Color.FromRgb(0x3D, 0x28, 0x0A),
            >= 60 => OrangeSoft,
            _ => RedSoft,
        };
    }

    /// <summary>Position chip color.</summary>
    public static Color PositionColor(string position)
    {
        var p = position?.ToUpperInvariant() ?? string.Empty;
        if (p.Contains("GK") || p.Contains("CB") || p.Contains("LB") || p.Contains("RB") || p.Contains("WB")) return CyanAccent;
        if (p.Contains("CM") || p.Contains("CDM") || p.Contains("CAM") || p.Contains("LM") || p.Contains("RM")) return Green;
        if (p.Contains("ST") || p.Contains("CF") || p.Contains("LW") || p.Contains("RW")) return Red;
        return Purple;
    }
}

public sealed class RatingToBrushConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var rating = value is int i ? i : (value is string s && int.TryParse(s, out var r) ? r : 0);
        return new SolidColorBrush(StudioPalette.RatingColor(rating));
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
