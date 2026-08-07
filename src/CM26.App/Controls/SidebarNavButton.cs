using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>
/// FC-Editor-style vertical sidebar navigation button. Active state draws a
/// blue left accent bar on a tinted background; hover draws the raised surface.
/// </summary>
public sealed class SidebarNavButton : Button
{
    private bool _checked;
    private bool _hovered;

    public SidebarNavButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        Height = 34;
        Margin = new Padding(0, 1, 0, 1);
        Padding = new Padding(0);
        TextAlign = ContentAlignment.MiddleLeft;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            Invalidate();
        }
    }

    /// <summary>Re-paints using the current palette (called after a theme toggle).</summary>
    public void ApplyTheme() => Invalidate();

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        var rect = ClientRectangle;
        using (var bg = new SolidBrush(Checked ? SelectedBack : _hovered ? Theme.Raised : Theme.Panel))
            g.FillRectangle(bg, rect);
        if (Checked)
        {
            using var bar = new SolidBrush(Theme.Accent);
            g.FillRectangle(bar, 0, 0, 3, rect.Height);
        }

        var textX = 10;
        if (Image != null)
        {
            var size = 18;
            var imageRect = new Rectangle(12, (rect.Height - size) / 2, size, size);
            g.DrawImage(Image, imageRect);
            textX = imageRect.Right + 8;
        }
        var textRect = new Rectangle(textX, 0, Math.Max(0, rect.Width - textX - 6), rect.Height);
        var color = Checked ? Theme.Accent : Theme.Text;
        TextRenderer.DrawText(g, Text, Checked ? Theme.BodyBold : Theme.Body, textRect, color,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
    }

    /// <summary>Tinted background for the active item (blue wash over the panel colour).</summary>
    private static Color SelectedBack =>
        Color.FromArgb(Theme.IsDark ? 46 : 30, Theme.Accent);
}
