using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// A single row in a team roster list showing miniface, name, position, OVR/POT and role.
/// </summary>
public sealed class RosterPlayerRow : Control
{
    private bool _hover;
    private bool _selected;

    public RosterPlayerRow()
    {
        DoubleBuffered = true;
        Height = 42;
        Dock = DockStyle.Top;
        BackColor = Color.Transparent;
        Font = StudioFonts.RowPrimary;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        AllowDrop = true;
    }

    public event EventHandler? RowClicked;

    public int PlayerId { get; set; }
    public Image? Miniface { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int Overall { get; set; }
    public int Potential { get; set; }
    public string RoleText { get; set; } = string.Empty;
    public string SectionText { get; set; } = string.Empty;

    public bool IsSelected
    {
        get => _selected;
        set
        {
            _selected = value;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            IsSelected = true;
            RowClicked?.Invoke(this, EventArgs.Empty);
            DoDragDrop(PlayerId, DragDropEffects.Move);
        }
        base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, StudioSpacing.Tiny);

        if (_selected)
        {
            using var bg = new SolidBrush(StudioColors.RaisedSurface);
            g.FillPath(bg, path);
            using var accent = new Pen(StudioColors.CyanAccent, 1f);
            g.DrawPath(accent, path);
        }
        else if (_hover)
        {
            using var bg = new SolidBrush(Color.FromArgb(40, StudioColors.CyanAccent));
            g.FillPath(bg, path);
        }

        var x = StudioSpacing.Small;
        if (Miniface != null)
        {
            g.DrawImage(Miniface, new Rectangle(x, 5, 32, 32));
            x += 40;
        }

        var nameBounds = new Rectangle(x, 0, Math.Max(80, Width - 220), Height);
        TextRenderer.DrawText(g, PlayerName, StudioFonts.RowPrimary, nameBounds, StudioColors.PrimaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var posX = Width - 140;
        if (!string.IsNullOrWhiteSpace(Position))
        {
            using var posBrush = new SolidBrush(Color.FromArgb(35, StudioColors.PositionColor(Position)));
            using var posPath = RoundedRect(new Rectangle(posX, 10, 36, 22), 10);
            g.FillPath(posBrush, posPath);
            TextRenderer.DrawText(g, Position, StudioFonts.Chip, new Rectangle(posX, 10, 36, 22), StudioColors.PositionColor(Position),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            posX += 44;
        }

        TextRenderer.DrawText(g, Overall.ToString(), StudioFonts.Badge, new Rectangle(posX, 0, 28, Height),
            StudioColors.RatingColor(Overall), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        posX += 32;

        TextRenderer.DrawText(g, Potential.ToString(), StudioFonts.DataLabel, new Rectangle(posX, 0, 28, Height),
            StudioColors.Yellow, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        posX += 36;

        if (!string.IsNullOrWhiteSpace(RoleText))
        {
            TextRenderer.DrawText(g, RoleText, StudioFonts.Metadata, new Rectangle(posX, 0, Math.Max(40, Width - posX - StudioSpacing.Small), Height),
                StudioColors.MutedText, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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
