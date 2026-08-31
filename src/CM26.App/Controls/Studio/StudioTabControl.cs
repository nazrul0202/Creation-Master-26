using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>Clean, compact Deco-style tabs with an accent marker and no legacy focus rectangle.</summary>
public sealed class StudioTabControl : TabControl
{
    public StudioTabControl()
    {
        DrawMode = TabDrawMode.OwnerDrawFixed;
        SizeMode = TabSizeMode.Fixed;
        ItemSize = new Size(112, 34);
        Padding = new Point(14, 6);
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (e.Control is TabPage page)
        {
            page.UseVisualStyleBackColor = false;
            page.BackColor = StudioColors.AppBackground;
        }
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= TabPages.Count) return;
        var selected = SelectedIndex == e.Index;
        var bounds = GetTabRect(e.Index);
        var background = selected ? StudioColors.Surface : StudioColors.AppBackground;
        using var fill = new SolidBrush(background);
        e.Graphics.FillRectangle(fill, bounds);

        var textBounds = Rectangle.Inflate(bounds, -12, -2);
        TextRenderer.DrawText(e.Graphics, TabPages[e.Index].Text,
            selected ? StudioFonts.DataValue : Font,
            textBounds, selected ? StudioColors.PrimaryText : StudioColors.MutedText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        if (selected)
        {
            using var accent = new SolidBrush(StudioColors.CyanAccent);
            e.Graphics.FillRectangle(accent, bounds.Left + 8, bounds.Bottom - 3,
                Math.Max(1, bounds.Width - 16), 3);
        }
    }
}
