using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Compact colored rating badge for OVR/POT/stat values.
/// </summary>
public sealed class RatingBadge : Control
{
    private int _rating;

    public RatingBadge()
    {
        DoubleBuffered = true;
        Size = new Size(34, 22);
        Font = StudioFonts.Badge;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public int Rating
    {
        get => _rating;
        set
        {
            _rating = Math.Clamp(value, 0, 99);
            Invalidate();
        }
    }

    public string RatingText
    {
        get => _rating.ToString();
        set
        {
            if (int.TryParse(value, out var r))
                Rating = r;
            else
                Text = value;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, StudioSpacing.BadgeRadius);
        using var bg = new SolidBrush(StudioColors.RatingSoftColor(_rating));
        g.FillPath(bg, path);

        using var border = new Pen(StudioColors.RatingColor(_rating), 1f);
        g.DrawPath(border, path);

        TextRenderer.DrawText(g, _rating.ToString(), Font, bounds, StudioColors.RatingColor(_rating),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        base.OnPaint(e);
    }

    private static GraphicsPath RoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
