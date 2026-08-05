using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>Editor header: record title, resolved relationship subtitle, and an icon.</summary>
public sealed class EditorHeader : UserControl
{
    private readonly PictureBox _icon;
    private readonly Label _title;
    private readonly Label _subtitle;
    private readonly Label _badge;

    public EditorHeader()
    {
        Height = 64;
        BackColor = Theme.Raised;
        Padding = new Padding(Theme.Space + 4, Theme.Space, Theme.Space, Theme.Space);

        _icon = new PictureBox { Size = new Size(44, 44), SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Left, BackColor = Color.Transparent };
        _badge = new Label
        {
            Dock = DockStyle.Right, AutoSize = false, Width = 90, TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Text, BackColor = Theme.Accent, Font = Theme.BodyBold, Visible = false,
        };
        var textPanel = new BufferedPanel { Dock = DockStyle.Fill, Padding = new Padding(Theme.Space, 0, 0, 0) };
        _title = new Label { Dock = DockStyle.Top, Height = 28, Font = Theme.RecordTitle, ForeColor = Theme.Text, AutoEllipsis = true };
        _subtitle = new Label { Dock = DockStyle.Fill, Font = Theme.Muted9, ForeColor = Theme.Muted, AutoEllipsis = true };
        textPanel.Controls.Add(_subtitle);
        textPanel.Controls.Add(_title);

        Controls.Add(textPanel);
        Controls.Add(_badge);
        Controls.Add(_icon);

        // bottom border line
        Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.Border);
            e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        };
    }

    public void SetRecord(string title, string subtitle, Image? icon, string? badge = null)
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        // IconService.Get returns a shared cached Image that must never be disposed
        // here. Clone so this control owns a private copy it can release safely.
        var owned = icon == null ? null : new Bitmap(icon);
        var old = _icon.Image;
        _icon.Image = owned;
        old?.Dispose();
        _badge.Visible = !string.IsNullOrEmpty(badge);
        _badge.Text = badge ?? string.Empty;
    }

    public void Clear(string emptyMessage)
    {
        _title.Text = emptyMessage;
        _subtitle.Text = string.Empty;
        var old = _icon.Image;
        _icon.Image = null;
        old?.Dispose();
        _badge.Visible = false;
    }
}
