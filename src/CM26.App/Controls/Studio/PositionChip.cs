using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Compact pill showing a player position (CM, CDM, ST, GK, etc.).
/// </summary>
public sealed class PositionChip : Control
{
    private string _position = string.Empty;

    public PositionChip()
    {
        DoubleBuffered = true;
        AutoSize = false;
        Height = 22;
        Font = StudioFonts.Chip;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public string Position
    {
        get => _position;
        set
        {
            _position = value?.ToUpperInvariant() ?? string.Empty;
            var textWidth = TextRenderer.MeasureText(_position, Font).Width;
            Width = Math.Max(34, textWidth + 16);
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, StudioSpacing.ChipRadius);
        var color = StudioColors.PositionColor(_position);
        using var bg = new SolidBrush(Color.FromArgb(35, color));
        g.FillPath(bg, path);
        using var border = new Pen(Color.FromArgb(90, color), 1f);
        g.DrawPath(border, path);

        TextRenderer.DrawText(g, _position, Font, bounds, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        base.OnPaint(e);
    }

    private static GraphicsPath RoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
        if (d <= 0) d = 1;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
