using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Theming;

/// <summary>Central design system: palette, typography, spacing. Every control uses these constants.</summary>
public static class Theme
{
    // Dark CM26 desktop palette: near-black work surface, restrained green
    // borders and a brighter green only for active or primary actions.
    public static readonly Color Background = Color.FromArgb(7, 12, 9);
    public static readonly Color Panel = Color.FromArgb(10, 18, 13);
    public static readonly Color Raised = Color.FromArgb(18, 30, 22);
    public static readonly Color Input = Color.FromArgb(14, 24, 17);
    public static readonly Color Border = Color.FromArgb(25, 72, 43);
    public static readonly Color Text = Color.FromArgb(224, 239, 228);
    public static readonly Color Muted = Color.FromArgb(139, 169, 148);
    public static readonly Color Accent = Color.FromArgb(31, 190, 99);
    public static readonly Color AccentHover = Color.FromArgb(22, 146, 74);
    public static readonly Color Danger = Color.FromArgb(222, 83, 68);
    public static readonly Color Success = Color.FromArgb(50, 203, 111);
    public static readonly Color Warning = Color.FromArgb(230, 180, 58);

    // Typography
    public static readonly Font Body = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font BodyBold = new("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font Label = new("Segoe UI Semibold", 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font SectionTitle = new("Segoe UI Semibold", 12f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font RecordTitle = new("Segoe UI Semibold", 15f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font AppTitle = new("Segoe UI Semibold", 26f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font ButtonLarge = new("Segoe UI Semibold", 11f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font Muted9 = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font Mono = new("Consolas", 9f, FontStyle.Regular, GraphicsUnit.Point);

    // Metrics
    public const int Space = 8;
    public const int ControlHeight = 26;
    public const int ToolbarHeight = 54;
    public const int SidebarWidth = 220;
    public const int NavItemHeight = 34;

    public static void ApplyButton(Button b, bool primary = false)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.BorderColor = primary ? Accent : Border;
        b.BackColor = primary ? Accent : Panel;
        b.ForeColor = primary ? Background : Text;
        b.Font = primary ? BodyBold : Body;
        b.Height = ControlHeight;
        b.Cursor = Cursors.Hand;
        b.UseVisualStyleBackColor = false;
        if (primary)
        {
            b.FlatAppearance.MouseOverBackColor = AccentHover;
            b.FlatAppearance.MouseDownBackColor = AccentHover;
        }
        else
        {
            b.FlatAppearance.MouseOverBackColor = Raised;
            b.FlatAppearance.MouseDownBackColor = Raised;
        }
    }

    public static void ApplyTextBox(TextBox t)
    {
        t.BackColor = Input;
        t.ForeColor = Text;
        t.BorderStyle = BorderStyle.FixedSingle;
        t.Font = Body;
    }

    public static void ApplyCombo(ComboBox c)
    {
        c.BackColor = Input;
        c.ForeColor = Text;
        c.FlatStyle = FlatStyle.Flat;
        c.Font = Body;
    }

    /// <summary>
    /// Themes a Details-view ListView. The native column header of a WinForms
    /// ListView cannot be styled through managed APIs.
    ///
    /// NOTE: header colours were previously set with HDM_SETTEXTCOLOR /
    /// HDM_SETBKCOLOR. Those messages crashed the process with a fatal
    /// AccessViolationException (Windows "Exception Processing Message
    /// 0xc0000005 - Unexpected parameters") in every timing we tried: from the
    /// HandleCreated callback, from a deferred BeginInvoke, and from the normal
    /// message loop. The fault happens inside the native header control's own
    /// window procedure, which .NET cannot catch — so this styling is removed
    /// rather than risk a launch-time crash. Header theming is cosmetic only;
    /// the list body keeps the dark palette.
    /// </summary>
    public static void ApplyListView(ListView list)
    {
        list.BackColor = Input;
        list.ForeColor = Text;
        list.Font = Body;
        list.FullRowSelect = true;
    }

    public static void ApplyGrid(DataGridView g)
    {
        g.BackgroundColor = Background;
        g.BorderStyle = BorderStyle.None;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersDefaultCellStyle.BackColor = Raised;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        g.ColumnHeadersDefaultCellStyle.Font = Label;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Raised;
        g.ColumnHeadersHeight = 30;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        g.DefaultCellStyle.BackColor = Input;
        g.DefaultCellStyle.ForeColor = Text;
        g.DefaultCellStyle.SelectionBackColor = Accent;
        g.DefaultCellStyle.SelectionForeColor = Background;
        g.DefaultCellStyle.Font = Body;
        g.AlternatingRowsDefaultCellStyle.BackColor = Raised;
        g.AlternatingRowsDefaultCellStyle.ForeColor = Text;
        g.RowHeadersVisible = false;
        g.AllowUserToAddRows = false;
        g.AllowUserToResizeRows = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.MultiSelect = false;
        g.GridColor = Border;
        g.RowTemplate.Height = 26;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        // Auto-generated and explicitly-added columns do not inherit the grid default
        // styles for their header/cell rendering, so apply the palette at the column
        // level as well whenever columns already exist.
        foreach (DataGridViewColumn column in g.Columns)
        {
            column.HeaderCell.Style.BackColor = Raised;
            column.HeaderCell.Style.ForeColor = Text;
            column.HeaderCell.Style.Font = Label;
            column.HeaderCell.Style.SelectionBackColor = Raised;
            column.DefaultCellStyle.BackColor = Input;
            column.DefaultCellStyle.ForeColor = Text;
            column.DefaultCellStyle.SelectionBackColor = Accent;
            column.DefaultCellStyle.SelectionForeColor = Background;
            column.DefaultCellStyle.Font = Body;
        }
    }

    /// <summary>Applies the public dark theme to legacy fixed-layout forms.</summary>
    public static void ApplyControlTree(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case TextBox textBox:
                    ApplyTextBox(textBox);
                    if (textBox.ReadOnly) textBox.BackColor = Raised;
                    break;
                case ComboBox comboBox:
                    ApplyCombo(comboBox);
                    break;
                case Button button:
                    ApplyButton(button);
                    break;
                case CheckBox checkBox:
                    checkBox.ForeColor = Text;
                    checkBox.BackColor = Background;
                    checkBox.Font = Body;
                    checkBox.FlatStyle = FlatStyle.Flat;
                    break;
                case RadioButton radio:
                    radio.ForeColor = Text;
                    radio.BackColor = Background;
                    radio.Font = Body;
                    radio.FlatStyle = FlatStyle.Flat;
                    break;
                case NumericUpDown nud:
                    nud.BackColor = Input;
                    nud.ForeColor = Text;
                    nud.Font = Body;
                    nud.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case TrackBar trackBar:
                    trackBar.BackColor = Background;
                    break;
                case RichTextBox rtb:
                    rtb.BackColor = Input;
                    rtb.ForeColor = Text;
                    rtb.Font = Body;
                    rtb.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case DataGridView grid:
                    ApplyGrid(grid);
                    break;
                case ListView list:
                    ApplyListView(list);
                    break;
                case TreeView tree:
                    tree.BackColor = Input;
                    tree.ForeColor = Text;
                    tree.Font = Body;
                    tree.LineColor = Border;
                    break;
                case SplitContainer split:
                    split.BackColor = Background;
                    split.ForeColor = Text;
                    break;
                case SplitterPanel splitPanel:
                    splitPanel.BackColor = Background;
                    splitPanel.ForeColor = Text;
                    break;
                case TabControl tabs:
                    tabs.BackColor = Panel;
                    tabs.ForeColor = Text;
                    tabs.Font = Body;
                    break;
                case TabPage page:
                    page.BackColor = Background;
                    page.ForeColor = Text;
                    break;
                case GroupBox group:
                    group.BackColor = Panel;
                    group.ForeColor = Text;
                    group.Font = Body;
                    break;
                case Label label:
                    label.ForeColor = label.ForeColor == SystemColors.GrayText ? Muted : Text;
                    label.BackColor = Color.Transparent;
                    label.Font = label.Font.FontFamily.Name.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase) ? Body : label.Font;
                    break;
                case Panel panel when control is not PictureBox:
                    panel.BackColor = Background;
                    panel.ForeColor = Text;
                    break;
                case Form form:
                    form.BackColor = Background;
                    form.ForeColor = Text;
                    break;
            }
            if (control.HasChildren) ApplyControlTree(control);
        }
    }
}
