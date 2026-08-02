using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls;

internal static class EntityCreationDialog
{
    public static bool TryShow(
        IWin32Window owner,
        string entityName,
        IReadOnlyList<(string Label, string InitialValue)> fields,
        out string[] values)
    {
        using var dialog = new Form
        {
            Text = $"Create New {entityName}",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(430, 92 + (fields.Count * 34)),
            Font = new Font("Segoe UI", 9F),
        };

        var editors = new List<TextBox>();
        for (var index = 0; index < fields.Count; index++)
        {
            var y = 18 + (index * 34);
            dialog.Controls.Add(new Label
            {
                Text = fields[index].Label,
                Location = new Point(16, y + 4),
                Size = new Size(125, 22),
            });
            var editor = new TextBox
            {
                Text = fields[index].InitialValue,
                Location = new Point(146, y),
                Size = new Size(265, 24),
            };
            dialog.Controls.Add(editor);
            editors.Add(editor);
        }

        var note = new Label
        {
            Text = "A safe unused ID will be assigned automatically.",
            Location = new Point(16, 26 + (fields.Count * 34)),
            Size = new Size(260, 22),
            ForeColor = SystemColors.GrayText,
        };
        dialog.Controls.Add(note);

        var create = new Button
        {
            Text = "Create",
            DialogResult = DialogResult.OK,
            Location = new Point(246, 54 + (fields.Count * 34)),
            Size = new Size(80, 28),
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(331, 54 + (fields.Count * 34)),
            Size = new Size(80, 28),
        };
        dialog.Controls.Add(create);
        dialog.Controls.Add(cancel);
        dialog.AcceptButton = create;
        dialog.CancelButton = cancel;

        if (editors.Count > 0)
        {
            dialog.Shown += (_, _) =>
            {
                editors[0].SelectAll();
                editors[0].Focus();
            };
        }

        if (dialog.ShowDialog(owner) != DialogResult.OK)
        {
            values = [];
            return false;
        }

        values = editors.Select(editor => editor.Text.Trim()).ToArray();
        if (values.Any(string.IsNullOrWhiteSpace))
        {
            MessageBox.Show(owner, "Complete every required field.", $"Create {entityName}",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }
}
