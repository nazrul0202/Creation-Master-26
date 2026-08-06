using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>
/// A lightweight transient toast that slides in at the top of a parent control and
/// auto-dismisses after a short duration. Used to surface field-level feedback
/// without interrupting the workflow with a modal MessageBox.
/// </summary>
public sealed class Toast : BufferedPanel
{
    private readonly Label _label;
    private readonly System.Windows.Forms.Timer _timer;
    private Control? _parent;

    public Toast()
    {
        Height = 34;
        BackColor = Theme.Raised;
        Padding = new Padding(12, 4, 12, 4);
        Visible = false;

        _label = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Font = Theme.Body,
        };
        Controls.Add(_label);

        _timer = new System.Windows.Forms.Timer { Interval = 2600 };
        _timer.Tick += (_, _) => HideToast();

        Paint += (_, e) =>
        {
            using var pen = new Pen(_label.ForeColor);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        };
    }

    /// <summary>Show an error-styled toast anchored to the top of <paramref name="owner"/>.</summary>
    public void ShowError(Control owner, string message)
    {
        Show(owner, message, Theme.Danger);
    }

    /// <summary>Show an informational toast anchored to the top of <paramref name="owner"/>.</summary>
    public void ShowInfo(Control owner, string message)
    {
        Show(owner, message, Theme.Accent);
    }

    private void Show(Control owner, string message, Color border)
    {
        if (owner == null) return;
        _parent = owner;
        _label.Text = message;
        _label.ForeColor = border;
        Width = Math.Max(160, Math.Min(owner.Width - 40, 560));
        Location = new Point((owner.Width - Width) / 2, 6);
        owner.Controls.Add(this);
        BringToFront();
        Visible = true;
        _timer.Stop();
        _timer.Start();
    }

    private void HideToast()
    {
        _timer.Stop();
        Visible = false;
        if (_parent != null && _parent.Controls.Contains(this))
            _parent.Controls.Remove(this);
        _parent = null;
    }
}
