using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Compact chip for a player playstyle (standard or plus variant).
/// </summary>
public sealed class PlaystyleChip : Control
{
    private string _text = string.Empty;
    private bool _isPlus;

    public PlaystyleChip()
    {
        DoubleBuffered = true;
        AutoSize = false;
        Height = 22;
        Font = StudioFonts.Chip;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public string PlaystyleText
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _isPlus = _text.EndsWith("+", StringComparison.Ordinal);
            var textWidth = TextRenderer.MeasureText(DisplayText, Font).Width;
            Width = Math.Max(34, textWidth + 18);
            Invalidate();
        }
    }

    private string DisplayText => _isPlus ? _text.TrimEnd('+') : _text;

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, StudioSpacing.ChipRadius);

        var color = _isPlus ? StudioColors.Purple : StudioColors.CyanAccent;
        using var bg = new SolidBrush(Color.FromArgb(35, color));
        g.FillPath(bg, path);
        using var border = new Pen(Color.FromArgb(90, color), 1f);
        g.DrawPath(border, path);

        if (_isPlus)
        {
            var plusSize = 12;
            using var plusBrush = new SolidBrush(StudioColors.Purple);
            g.FillEllipse(plusBrush, new Rectangle(4, 5, plusSize, plusSize));
            TextRenderer.DrawText(g, "+", StudioFonts.Badge, new Rectangle(4, 5, plusSize, plusSize), StudioColors.PrimaryText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            var textBounds = new Rectangle(20, 0, Width - 24, Height);
            TextRenderer.DrawText(g, DisplayText, Font, textBounds, StudioColors.PrimaryText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
        else
        {
            TextRenderer.DrawText(g, DisplayText, Font, bounds, StudioColors.PrimaryText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

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
