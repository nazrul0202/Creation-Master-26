using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Theming;

/// <summary>
/// Dark professional renderer so ToolStrip/MenuStrip/StatusStrip items render with the
/// CM26 dark palette instead of the default light system colors. Without this the action
/// buttons, module buttons and menus would paint with light surfaces on the dark window.
/// </summary>
public sealed class DarkToolStripRenderer : ToolStripProfessionalRenderer
{
    public DarkToolStripRenderer() : base(new DarkColorTable()) { }

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var button = e.Item as ToolStripButton;
        var hover = e.Item.Selected;
        var pressed = e.Item.Pressed;
        var rect = new Rectangle(Point.Empty, e.Item.Size);

        if (pressed)
        {
            using var brush = new SolidBrush(Theme.Raised);
            e.Graphics.FillRectangle(brush, rect);
        }
        else if (hover && button is { DisplayStyle: ToolStripItemDisplayStyle.Image })
        {
            using var brush = new SolidBrush(Theme.Raised);
            e.Graphics.FillRoundedRectangle(brush, rect, 4);
        }
        else if (hover)
        {
            using var brush = new SolidBrush(Theme.Raised);
            e.Graphics.FillRectangle(brush, rect);
        }
        else if (button is { Checked: true })
        {
            // Active module button highlight (bright blue, matching the CM26 Scraper theme).
            using var brush = new SolidBrush(Color.FromArgb(20, 43, 77));
            e.Graphics.FillRoundedRectangle(brush, rect, 4);
            using var pen = new Pen(Theme.Accent);
            e.Graphics.DrawRectangle(pen, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
        }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Selected ? Theme.Text : e.Item.ForeColor;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var line = new Rectangle(e.Item.Width / 2 - 1, 4, 2, e.Item.Height - 8);
        using var brush = new SolidBrush(Theme.Border);
        e.Graphics.FillRectangle(brush, line);
    }
}

/// <summary>Color table that matches the CM26 dark palette.</summary>
internal sealed class DarkColorTable : ProfessionalColorTable
{
    public override Color ToolStripGradientBegin => Theme.Panel;
    public override Color ToolStripGradientMiddle => Theme.Panel;
    public override Color ToolStripGradientEnd => Theme.Panel;
    public override Color ToolStripBorder => Theme.Border;
    public override Color MenuBorder => Theme.Border;
    public override Color MenuItemBorder => Theme.Border;
    public override Color MenuItemSelected => Theme.Raised;
    public override Color MenuItemSelectedGradientBegin => Theme.Raised;
    public override Color MenuItemSelectedGradientEnd => Theme.Raised;
    public override Color MenuItemPressedGradientBegin => Theme.Raised;
    public override Color MenuItemPressedGradientMiddle => Theme.Raised;
    public override Color MenuItemPressedGradientEnd => Theme.Raised;
    public override Color MenuStripGradientBegin => Theme.Background;
    public override Color MenuStripGradientEnd => Theme.Background;
    public override Color StatusStripGradientBegin => Theme.Panel;
    public override Color StatusStripGradientEnd => Theme.Panel;
    public override Color ImageMarginGradientBegin => Theme.Panel;
    public override Color ImageMarginGradientMiddle => Theme.Panel;
    public override Color ImageMarginGradientEnd => Theme.Panel;
    public override Color SeparatorLight => Theme.Border;
    public override Color SeparatorDark => Theme.Border;
    public override Color CheckBackground => Theme.Accent;
    public override Color CheckSelectedBackground => Theme.Accent;
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, int radius)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        int d = radius * 2;
        using var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}
