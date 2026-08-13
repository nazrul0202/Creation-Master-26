using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>
/// FC-Editor-style vertical sidebar navigation button. Active state draws a
/// blue left accent bar on a tinted background; hover draws the raised surface.
/// </summary>
public sealed class SidebarNavButton : Panel
{
    private bool _checked;
    private bool _hovered;
    private Image? _image;

    public SidebarNavButton()
    {
        Height = 38;
        Margin = new Padding(0, 2, 0, 2);
        Padding = new Padding(0);
        Cursor = Cursors.Hand;
        TabStop = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
    }

    /// <summary>
    /// A Panel is used deliberately instead of the native Button control: a
    /// Button can paint its own text in addition to custom painting on some
    /// Windows themes, producing the duplicate sidebar captions seen in-app.
    /// </summary>
    public Image? Image
    {
        get => _image;
        set { _image = value; Invalidate(); }
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
        if (_image != null)
        {
            var size = 18;
            var imageRect = new Rectangle(12, (rect.Height - size) / 2, size, size);
            g.DrawImage(_image, imageRect);
            textX = imageRect.Right + 8;
        }
        var textRect = new Rectangle(textX, 0, Math.Max(0, rect.Width - textX - 6), rect.Height);
        var color = Checked ? Theme.Accent : Theme.Muted;
        TextRenderer.DrawText(g, Text, Checked ? Theme.BodyBold : Theme.Body, textRect, color,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
    }

    /// <summary>Tinted background for the active item (blue wash over the panel colour).</summary>
    private static Color SelectedBack =>
        Color.FromArgb(Theme.IsDark ? 46 : 30, Theme.Accent);
}
