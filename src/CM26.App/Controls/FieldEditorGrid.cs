using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Controls;

/// <summary>
/// Scrollable, grouped editor of FieldValue rows. Emits FieldEdited with the new raw value.
/// Writable fields are editable; read-only fields are shown disabled with a tooltip.
/// </summary>
public sealed class FieldEditorGrid : UserControl
{
    private readonly TableLayoutPanel _table;
    private readonly Dictionary<string, Control> _editors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Label> _modifiedMarks = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<(string field, string value)>? FieldEdited;

    public FieldEditorGrid()
    {
        BackColor = Theme.Panel;
        AutoScroll = true;
        _table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            Padding = new Padding(Theme.Space),
        };
        _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18));
        Controls.Add(_table);
    }

    public void SetFields(IReadOnlyList<FieldValue> fields, ToolTip toolTip)
    {
        SuspendLayout();
        _table.SuspendLayout();
        foreach (Control control in _table.Controls) control.Dispose();
        _table.Controls.Clear();
        _table.RowStyles.Clear();
        _editors.Clear();
        _modifiedMarks.Clear();
        _table.RowCount = fields.Count;

        for (int i = 0; i < fields.Count; i++)
        {
            var f = fields[i];
            _table.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.ControlHeight + 8));

            var label = new Label
            {
                Text = f.Label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = f.IsWritable ? Theme.Text : Theme.Muted,
                Font = Theme.Body,
                AutoEllipsis = true,
                Margin = new Padding(2, 3, 6, 3),
            };
            var hint = f.IsWritable
                ? (f.RangeLow.HasValue ? $"{f.KindLabel} · range {f.RangeLow}..{f.RangeHigh}" : f.KindLabel)
                : "Read-only (engine does not support writing this field safely)";
            toolTip.SetToolTip(label, $"{f.FieldName}\n{hint}");

            Control editor = f.IsWritable
                ? MakeEditor(f, toolTip)
                : MakeReadOnly(f, toolTip);

            var mark = new Label
            {
                Text = "●",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Theme.Warning,
                Font = Theme.Muted9,
                Visible = f.Modified,
            };
            _modifiedMarks[f.FieldName] = mark;

            _table.Controls.Add(label, 0, i);
            _table.Controls.Add(editor, 1, i);
            _table.Controls.Add(mark, 2, i);
        }

        _table.ResumeLayout();
        ResumeLayout();
    }

    private Control MakeEditor(FieldValue f, ToolTip toolTip)
    {
        var box = new TextBox { Dock = DockStyle.Fill, Text = f.Value, Margin = new Padding(2, 3, 2, 3) };
        Theme.ApplyTextBox(box);
        toolTip.SetToolTip(box, f.RangeLow.HasValue ? $"Allowed {f.RangeLow}..{f.RangeHigh}" : f.KindLabel);
        box.Leave += (_, _) =>
        {
            if (box.Text != f.Value)
                FieldEdited?.Invoke(this, (f.FieldName, box.Text.Trim()));
        };
        box.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; this.Parent?.Focus(); }
        };
        _editors[f.FieldName] = box;
        return box;
    }

    private Control MakeReadOnly(FieldValue f, ToolTip toolTip)
    {
        var box = new TextBox { Dock = DockStyle.Fill, Text = f.Value, ReadOnly = true, Margin = new Padding(2, 3, 2, 3) };
        Theme.ApplyTextBox(box);
        box.BackColor = Theme.Raised;
        box.ForeColor = Theme.Muted;
        toolTip.SetToolTip(box, "Read-only field");
        return box;
    }

    public void UpdateValue(string field, string value, bool modified)
    {
        if (_editors.TryGetValue(field, out var ctl) && ctl is TextBox box && box.Text != value)
            box.Text = value;
        if (_modifiedMarks.TryGetValue(field, out var mark))
            mark.Visible = modified;
    }

    public void MarkModified(string field, bool modified)
    {
        if (_modifiedMarks.TryGetValue(field, out var mark))
            mark.Visible = modified;
    }
}
