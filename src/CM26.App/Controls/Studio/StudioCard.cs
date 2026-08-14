using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Base dark card: rounded surface, subtle border, optional top accent line.
/// </summary>
public class StudioCard : Panel
{
    private Color _accent = Color.Empty;

    public StudioCard()
    {
        DoubleBuffered = true;
        BackColor = StudioColors.Surface;
        ForeColor = StudioColors.PrimaryText;
        Padding = new Padding(StudioSpacing.Medium);
        Margin = new Padding(StudioSpacing.Small);
        BorderStyle = BorderStyle.None;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public Color AccentColor
    {
        get => _accent;
        set
        {
            _accent = value;
            Invalidate();
        }
    }

    public bool ShowAccentLine => _accent != Color.Empty;

    public Padding InnerPadding
    {
        get => Padding;
        set => Padding = value;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, StudioSpacing.CardRadius);
        using var bg = new SolidBrush(BackColor);
        g.FillPath(bg, path);

        using var border = new Pen(StudioColors.CardBorder, 1f);
        g.DrawPath(border, path);

        if (ShowAccentLine)
        {
            using var accent = new SolidBrush(_accent);
            g.FillRectangle(accent, new Rectangle(StudioSpacing.Medium, StudioSpacing.Medium, 3, 18));
        }

        base.OnPaint(e);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        Invalidate();
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
