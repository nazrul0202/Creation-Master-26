using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;
using CM26.Application.Services;

namespace CM26.App.Controls;

/// <summary>Shows validation issues; collapses when there are none.</summary>
public sealed class ValidationSummary : UserControl
{
    private readonly Label _header;
    private readonly ListBox _list;

    public ValidationSummary()
    {
        Height = 96;
        BackColor = Color.FromArgb(52, 36, 36);
        Visible = false;
        Padding = new Padding(Theme.Space, 4, Theme.Space, 4);

        _header = new Label { Dock = DockStyle.Top, Height = 20, ForeColor = Theme.Danger, Font = Theme.BodyBold, Text = "Validation issues" };
        _list = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 28, 28),
            ForeColor = Color.FromArgb(220, 190, 190),
            BorderStyle = BorderStyle.None,
            Font = Theme.Muted9,
        };
        Controls.Add(_list);
        Controls.Add(_header);
    }

    public void SetIssues(IReadOnlyList<ValidationIssue> issues)
    {
        _list.Items.Clear();
        if (issues.Count == 0) { Visible = false; return; }
        foreach (var i in issues)
            _list.Items.Add($"{(i.IsError ? "✕" : "⚠")} {i.Table}[{i.Row}].{i.Field}: {i.Message}");
        _header.Text = $"{issues.Count} validation issue(s) — resolve before saving";
        Visible = true;
    }

    public void ClearAll() { _list.Items.Clear(); Visible = false; }
}
