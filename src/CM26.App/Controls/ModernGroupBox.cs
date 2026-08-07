using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>
/// Group box that renders with the theme palette: hairline border with a gap
/// for the semibold header (classic GroupBox geometry, theme colours), so the
/// cards keep the same control coordinates as the original CM16 forms while
/// matching the neutral dark design system.
/// </summary>
public sealed class ModernGroupBox : GroupBox
{
    public ModernGroupBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Theme.Panel;
        ForeColor = Theme.Text;
        Font = Theme.Label;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Theme.Panel);

        var textSize = TextRenderer.MeasureText(g, Text, Font);
        var textX = 10;
        var textWidth = textSize.Width;
        var borderTop = 7;

        using var pen = new Pen(Theme.Border);
        // Bottom + sides full length; top border split around the title text.
        g.DrawLine(pen, 1, Height - 2, Width - 2, Height - 2);
        g.DrawLine(pen, 1, borderTop, 1, Height - 2);
        g.DrawLine(pen, Width - 2, borderTop, Width - 2, Height - 2);
        g.DrawLine(pen, 1, borderTop, textX - 2, borderTop);
        g.DrawLine(pen, textX + textWidth + 2, borderTop, Width - 2, borderTop);

        using var headerBrush = new SolidBrush(Theme.Text);
        TextRenderer.DrawText(g, Text, Font, new Point(textX, 2), Theme.Text);
    }
}