using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

using CM26.Application.Services;

namespace CM26.App.Controls;

/// <summary>
/// Modal first-run End User License Agreement dialog. The user must accept to
/// continue. Acceptance is persisted in <see cref="SettingsService"/>.
/// </summary>
public static class EulaDialog
{
    public static bool Show(IWin32Window? owner)
    {
        var eula = Path.Combine(AppContext.BaseDirectory, "EULA.md");
        var text = File.Exists(eula) ? File.ReadAllText(eula) : "End User License Agreement — see LICENSE file.";
        // A readable plain-text view of the agreement (the .md file is fine to show as-is).
        using var dialog = new Form
        {
            Text = Localization.T("Eula.Title"),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(640, 520),
            BackColor = Theme.Background,
            ForeColor = Theme.Text,
            Font = Theme.Body,
        };
        var textBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Theme.Input,
            ForeColor = Theme.Text,
            Font = Theme.Body,
            Text = text,
            Margin = new Padding(0),
        };
        var accept = new Button { Text = Localization.T("Eula.Accept"), Dock = DockStyle.Right, Width = 110, DialogResult = DialogResult.OK };
        var decline = new Button { Text = Localization.T("Eula.Decline"), Dock = DockStyle.Right, Width = 100, DialogResult = DialogResult.Cancel };
        Theme.ApplyButton(accept, primary: true);
        Theme.ApplyButton(decline);
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Theme.Panel, Padding = new Padding(8) };
        footer.Controls.Add(accept);
        footer.Controls.Add(decline);
        dialog.Controls.Add(textBox);
        dialog.Controls.Add(footer);
        dialog.AcceptButton = accept;
        dialog.CancelButton = decline;
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }
}
