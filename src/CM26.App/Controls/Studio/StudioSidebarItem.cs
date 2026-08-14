using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// A single selectable navigation row inside <see cref="StudioSidebar"/>.
/// </summary>
public sealed class StudioSidebarItem : Control
{
    private bool _isActive;
    private bool _hover;

    public StudioSidebarItem(StudioSidebarItemModel model)
    {
        Model = model;
        Height = 36;
        BackColor = Color.Transparent;
        ForeColor = StudioColors.PrimaryText;
        Font = StudioFonts.RowPrimary;
        Cursor = Cursors.Hand;
        TabStop = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public StudioSidebarItemModel Model { get; }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
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

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, StudioSpacing.Tiny);

        if (_isActive)
        {
            using var bg = new SolidBrush(StudioColors.RaisedSurface);
            g.FillPath(bg, path);
            using var accent = new SolidBrush(StudioColors.CyanAccent);
            g.FillRectangle(accent, new Rectangle(0, 10, 3, Height - 20));
        }
        else if (_hover)
        {
            using var bg = new SolidBrush(Color.FromArgb(40, StudioColors.CyanAccent));
            g.FillPath(bg, path);
        }

        var icon = Model.Icon;
        var x = StudioSpacing.Medium + (_isActive ? 2 : 0);
        if (icon != null)
        {
            var size = 18;
            g.DrawImage(icon, new Rectangle(x, (Height - size) / 2, size, size));
            x += size + StudioSpacing.Small;
        }

        var textBounds = new Rectangle(x, 0, bounds.Width - x - StudioSpacing.Medium, Height);
        TextRenderer.DrawText(g, Model.Title, Font, textBounds, StudioColors.PrimaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        if (!string.IsNullOrWhiteSpace(Model.Shortcut))
        {
            var shortcutWidth = TextRenderer.MeasureText(g, Model.Shortcut, StudioFonts.Metadata, Size.Empty,
                TextFormatFlags.NoPadding).Width;
            var shortcutBounds = new Rectangle(bounds.Width - shortcutWidth - StudioSpacing.Medium, 0, shortcutWidth, Height);
            TextRenderer.DrawText(g, Model.Shortcut, StudioFonts.Metadata, shortcutBounds, StudioColors.MutedText,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        base.OnPaint(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
        {
            OnClick(EventArgs.Empty);
            e.Handled = true;
        }
        base.OnKeyDown(e);
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
