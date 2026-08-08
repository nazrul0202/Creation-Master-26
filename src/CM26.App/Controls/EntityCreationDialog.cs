using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>
/// Defines the type of input control to display in the creation dialog.
/// </summary>
internal enum EntityFieldType
{
    /// <summary>Standard text input.</summary>
    Text,
    /// <summary>Dropdown selection from a list of items.</summary>
    Dropdown
}

/// <summary>
/// Describes a single field in the entity creation dialog.
/// </summary>
internal sealed record EntityField(
    string Label,
    string InitialValue,
    EntityFieldType FieldType = EntityFieldType.Text,
    IReadOnlyList<(string Display, string Value)>? Options = null);

internal static class EntityCreationDialog
{
    /// <summary>
    /// Shows the legacy text-only creation dialog (backward compatible).
    /// </summary>
    public static bool TryShow(
        IWin32Window owner,
        string entityName,
        IReadOnlyList<(string Label, string InitialValue)> fields,
        out string[] values)
    {
        var entityFields = fields.Select(f => new EntityField(f.Label, f.InitialValue)).ToList();
        return TryShow(owner, entityName, entityFields, out values);
    }

    /// <summary>
    /// Shows an enhanced creation dialog with support for text and dropdown fields.
    /// </summary>
    public static bool TryShow(
        IWin32Window owner,
        string entityName,
        IReadOnlyList<EntityField> fields,
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
            ClientSize = new Size(460, 100 + (fields.Count * 38)),
            Font = Theme.Body,
            BackColor = Theme.Background,
        };

        var controls = new List<Control>();
        for (var index = 0; index < fields.Count; index++)
        {
            var field = fields[index];
            var y = 18 + (index * 38);

            // Field label
            dialog.Controls.Add(new Label
            {
                Text = field.Label,
                Location = new Point(16, y + 4),
                Size = new Size(135, 22),
                ForeColor = Theme.Text,
                Font = Theme.Label,
                TextAlign = ContentAlignment.MiddleRight,
            });

            Control editor;
            switch (field.FieldType)
            {
                case EntityFieldType.Dropdown when field.Options is { Count: > 0 }:
                    var combo = new ComboBox
                    {
                        Location = new Point(158, y),
                        Size = new Size(275, 24),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = Theme.Body,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Theme.Input,
                        ForeColor = Theme.Text,
                    };
                    Theme.ApplyCombo(combo);
                    combo.Items.Clear();
                    var selectedIndex = 0;
                    for (var i = 0; i < field.Options.Count; i++)
                    {
                        var option = field.Options[i];
                        combo.Items.Add(option.Display);
                        if (string.Equals(option.Value, field.InitialValue, StringComparison.OrdinalIgnoreCase))
                            selectedIndex = i;
                    }
                    if (combo.Items.Count > 0) combo.SelectedIndex = selectedIndex;
                    editor = combo;
                    break;

                default:
                    var textBox = new TextBox
                    {
                        Text = field.InitialValue,
                        Location = new Point(158, y),
                        Size = new Size(275, 24),
                    };
                    Theme.ApplyTextBox(textBox);
                    editor = textBox;
                    break;
            }

            dialog.Controls.Add(editor);
            controls.Add(editor);
        }

        // Auto-ID note
        var note = new Label
        {
            Text = "A safe unused ID will be assigned automatically.",
            Location = new Point(16, 30 + (fields.Count * 38)),
            Size = new Size(280, 22),
            ForeColor = Theme.Muted,
            Font = Theme.Muted9,
        };
        dialog.Controls.Add(note);

        // Buttons
        var create = new Button
        {
            Text = "Create",
            DialogResult = DialogResult.OK,
            Location = new Point(262, 60 + (fields.Count * 38)),
            Size = new Size(85, 30),
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(352, 60 + (fields.Count * 38)),
            Size = new Size(85, 30),
        };
        Theme.ApplyButton(create, primary: true);
        Theme.ApplyButton(cancel);
        dialog.Controls.Add(create);
        dialog.Controls.Add(cancel);
        dialog.AcceptButton = create;
        dialog.CancelButton = cancel;

        // Focus first control on shown
        if (controls.Count > 0)
        {
            dialog.Shown += (_, _) =>
            {
                if (controls[0] is TextBox tb)
                {
                    tb.SelectAll();
                    tb.Focus();
                }
                else controls[0].Focus();
            };
        }

        if (dialog.ShowDialog(owner) != DialogResult.OK)
        {
            values = [];
            return false;
        }

        values = new string[controls.Count];
        for (var i = 0; i < controls.Count; i++)
        {
            values[i] = controls[i] switch
            {
                TextBox tb => tb.Text.Trim(),
                ComboBox cb when fields[i].Options is { Count: > 0 } options && cb.SelectedIndex >= 0
                    => options[cb.SelectedIndex].Value,
                ComboBox cb => cb.Text.Trim(),
                _ => string.Empty,
            };
        }

        if (values.Any(string.IsNullOrWhiteSpace))
        {
            MessageBox.Show(owner, "Complete every required field.", $"Create {entityName}",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Shows the creation dialog with dropdown fields populated from the database.
    /// Used by TeamsSection and PlayersSection for enhanced creation UX.
    /// </summary>
    public static bool TryShowEnhanced(
        IWin32Window owner,
        string entityName,
        IReadOnlyList<EntityField> fields,
        out string[] values)
    {
        return TryShow(owner, entityName, fields, out values);
    }
}
