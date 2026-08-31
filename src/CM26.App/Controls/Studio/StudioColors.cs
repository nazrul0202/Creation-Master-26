using System.Drawing;
using CM26.App.Theming;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Deco-inspired football database palette used by the Studio control set.
/// It follows the live application theme so light mode never contains dark islands.
/// </summary>
public static class StudioColors
{
    // The Studio control set follows the same live palette as the rest of CM26.
    // This makes Deco-inspired light mode cohesive instead of leaving dark islands
    // in custom sidebar/cards when the application theme changes.
    public static Color AppBackground => Theme.Background;
    public static Color Surface => Theme.Panel;
    public static Color RaisedSurface => Theme.Raised;
    public static Color InputBackground => Theme.Input;
    public static Color CardBorder => Theme.Border;
    public static Color Divider => Theme.Border;

    public static Color PrimaryText => Theme.Text;
    public static Color MutedText => Theme.Muted;
    public static Color DisabledText => Theme.IsDark
        ? Color.FromArgb(0x5A, 0x66, 0x74)
        : Color.FromArgb(150, 150, 150);

    public static Color CyanAccent => Theme.Accent;
    public static Color CyanAccentHover => Theme.AccentHover;

    public static Color Green => Theme.Success;
    public static Color GreenHover => Theme.IsDark
        ? Color.FromArgb(0x74, 0xB9, 0x22)
        : Color.FromArgb(92, 154, 24);
    public static Color GreenSoft => Theme.IsDark
        ? Color.FromArgb(0x22, 0x3D, 0x11)
        : Color.FromArgb(232, 245, 216);

    public static Color Yellow => Theme.Warning;
    public static Color YellowSoft => Theme.IsDark
        ? Color.FromArgb(0x3D, 0x2E, 0x0A)
        : Color.FromArgb(255, 244, 214);

    public static readonly Color Orange = Color.FromArgb(0xF9, 0x73, 0x16);
    public static Color OrangeSoft => Theme.IsDark
        ? Color.FromArgb(0x3D, 0x1E, 0x0A)
        : Color.FromArgb(255, 240, 229);

    public static Color Red => Theme.Danger;
    public static Color RedSoft => Theme.IsDark
        ? Color.FromArgb(0x3D, 0x12, 0x12)
        : Color.FromArgb(253, 232, 232);

    public static readonly Color Purple = Color.FromArgb(0xA8, 0x55, 0xF7);
    public static Color PurpleSoft => Theme.IsDark
        ? Color.FromArgb(0x2D, 0x1A, 0x47)
        : Color.FromArgb(240, 231, 250);

    public static readonly Color PitchGreen = Color.FromArgb(0x10, 0x3D, 0x28);
    public static readonly Color PitchLine = Color.FromArgb(0xFF, 0xFF, 0xFF);

    /// <summary>Returns a rating color for the 0-99 EA scale.</summary>
    public static Color RatingColor(int rating)
    {
        return rating switch
        {
            >= 90 => Green,
            >= 80 => Color.FromArgb(0xA3, 0xE6, 0x35),
            >= 75 => Yellow,
            >= 70 => Color.FromArgb(0xFB, 0x92, 0x3C),
            >= 60 => Orange,
            _ => Red,
        };
    }

    public static Color RatingSoftColor(int rating)
    {
        return rating switch
        {
            >= 90 => GreenSoft,
            >= 80 => Theme.IsDark ? Color.FromArgb(0x2A, 0x3D, 0x0F) : Color.FromArgb(239, 248, 220),
            >= 75 => YellowSoft,
            >= 70 => Theme.IsDark ? Color.FromArgb(0x3D, 0x28, 0x0A) : Color.FromArgb(255, 241, 218),
            >= 60 => OrangeSoft,
            _ => RedSoft,
        };
    }

    /// <summary>Returns a softer accent background for chips and badges.</summary>
    public static Color PositionColor(string position)
    {
        var p = position?.ToUpperInvariant() ?? string.Empty;
        if (p.Contains("GK")) return CyanAccent;
        if (p.Contains("CB") || p.Contains("LB") || p.Contains("RB") || p.Contains("WB") || p.Contains("LWB") || p.Contains("RWB")) return CyanAccent;
        if (p.Contains("CM") || p.Contains("CDM") || p.Contains("CAM") || p.Contains("LM") || p.Contains("RM")) return Green;
        if (p.Contains("ST") || p.Contains("CF") || p.Contains("LW") || p.Contains("RW")) return Red;
        return Purple;
    }
}
