using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Theming;

/// <summary>Central design system: palette, typography, spacing. Every control uses these.</summary>
public static class Theme
{
    private static bool _dark = true;

    /// <summary>Current visual theme mode (true = dark). Persisted with the settings.</summary>
    public static bool IsDark
    {
        get => _dark;
        set
        {
            if (_dark == value) return;
            _dark = value;
            ApplyPalette();
        }
    }

    private static void ApplyPalette()
    {
        if (_dark)
        {
            // CM26 fully-blue theme: every surface is a shade of blue,
            // matching the CM26 Scraper tool's blue appearance.
            _background = Color.FromArgb(20, 43, 77);     // Dark blue base
            _panel = Color.FromArgb(26, 54, 100);          // Medium dark blue
            _raised = Color.FromArgb(34, 74, 130);         // Medium blue
            _input = Color.FromArgb(15, 34, 64);           // Deep blue inputs
            _border = Color.FromArgb(59, 109, 181);        // Bright blue border
            _text = Color.FromArgb(255, 255, 255);         // Pure white text
            _muted = Color.FromArgb(127, 165, 214);        // Light blue-gray
            _accent = Color.FromArgb(4, 62, 195);          // Royal blue accent
            _accentHover = Color.FromArgb(12, 80, 214);    // Brighter blue hover
            _danger = Color.FromArgb(224, 96, 80);
            _success = Color.FromArgb(76, 195, 138);
            _warning = Color.FromArgb(230, 180, 58);
            _validationBackground = Color.FromArgb(26, 54, 100);
            _validationListBackground = Color.FromArgb(20, 43, 77);
            _validationText = Color.FromArgb(127, 165, 214);
        }
        else
        {
            // Light CM26 palette: soft off-white surfaces, blue borders/accents.
            _background = Color.FromArgb(246, 248, 251);
            _panel = Color.FromArgb(255, 255, 255);
            _raised = Color.FromArgb(233, 238, 249);
            _input = Color.FromArgb(255, 255, 255);
            _border = Color.FromArgb(20, 62, 120);
            _text = Color.FromArgb(24, 34, 50);
            _muted = Color.FromArgb(96, 116, 140);
            _accent = Color.FromArgb(4, 62, 195);
            _accentHover = Color.FromArgb(12, 58, 180);
            _danger = Color.FromArgb(200, 66, 50);
            _success = Color.FromArgb(40, 160, 90);
            _warning = Color.FromArgb(190, 145, 30);
            _validationBackground = Color.FromArgb(250, 235, 235);
            _validationListBackground = Color.FromArgb(255, 246, 246);
            _validationText = Color.FromArgb(150, 60, 55);
        }
    }

    private static Color _background = Color.FromArgb(20, 43, 77);
    private static Color _panel = Color.FromArgb(26, 54, 100);
    private static Color _raised = Color.FromArgb(34, 74, 130);
    private static Color _input = Color.FromArgb(15, 34, 64);
    private static Color _border = Color.FromArgb(59, 109, 181);
    private static Color _text = Color.FromArgb(255, 255, 255);
    private static Color _muted = Color.FromArgb(127, 165, 214);
    private static Color _accent = Color.FromArgb(4, 62, 195);
    private static Color _accentHover = Color.FromArgb(12, 80, 214);
    private static Color _danger = Color.FromArgb(224, 96, 80);
    private static Color _success = Color.FromArgb(76, 195, 138);
    private static Color _warning = Color.FromArgb(230, 180, 58);
    private static Color _validationBackground = Color.FromArgb(26, 54, 100);
    private static Color _validationListBackground = Color.FromArgb(20, 43, 77);
    private static Color _validationText = Color.FromArgb(127, 165, 214);

    public static Color Background => _background;
    public static Color Panel => _panel;
    public static Color Raised => _raised;
    public static Color Input => _input;
    public static Color Border => _border;
    public static Color Text => _text;
    public static Color Muted => _muted;
    public static Color Accent => _accent;
    public static Color AccentHover => _accentHover;
    public static Color Danger => _danger;
    public static Color Success => _success;
    public static Color Warning => _warning;
    public static Color ValidationBackground => _validationBackground;
    public static Color ValidationListBackground => _validationListBackground;
    public static Color ValidationText => _validationText;

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
        // Visible focus border for keyboard navigation; cleared when focus leaves.
        b.GotFocus += (_, _) => b.FlatAppearance.BorderColor = Accent;
        b.LostFocus += (_, _) => b.FlatAppearance.BorderColor = primary ? Accent : Border;
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
        // Accent border while focused so the active field is obvious for keyboard users.
        t.GotFocus += (_, _) => t.BorderStyle = BorderStyle.FixedSingle;
        t.Paint += (_, e) =>
        {
            if (!t.Focused) return;
            using var pen = new Pen(Accent);
            e.Graphics.DrawRectangle(pen, 0, 0, t.Width - 1, t.Height - 1);
        };
    }

    public static void ApplyCombo(ComboBox c)
    {
        c.BackColor = Input;
        c.ForeColor = Text;
        c.FlatStyle = FlatStyle.Flat;
        c.Font = Body;
        // Owner-draw so the dropdown list uses the dark palette instead of the
        // default light system colors (the native dropdown is otherwise unstyled).
        c.DrawMode = DrawMode.OwnerDrawFixed;
        c.DropDownStyle = c.DropDownStyle == ComboBoxStyle.Simple
            ? c.DropDownStyle
            : ComboBoxStyle.DropDownList;
        c.DrawItem += ComboDrawItem;
    }

    private static void ComboDrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        if (sender is not ComboBox combo) return;
        using var bg = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ? Accent : Input);
        e.Graphics.FillRectangle(bg, e.Bounds);
        var text = combo.GetItemText(combo.Items[e.Index]);
        var color = e.State.HasFlag(DrawItemState.Selected) ? Background : Text;
        var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
        TextRenderer.DrawText(e.Graphics, text, combo.Font, e.Bounds, color, flags);
        e.DrawFocusRectangle();
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

    /// <summary>
    /// Enables owner-drawn, dark tab headers so the native tab strip matches the
    /// CM26 palette instead of the default light system tab header.
    /// </summary>
    public static void ApplyTabs(TabControl tabs)
    {
        if (tabs.DrawMode != TabDrawMode.OwnerDrawFixed)
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem -= TabDrawItem;
            tabs.DrawItem += TabDrawItem;
        }
        // Keep the tab page content area dark (already themed by the tree walk).
    }

    private static void TabDrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not TabControl tabs || e.Index < 0) return;
        var selected = e.Index == tabs.SelectedIndex;
        var tabRect = tabs.GetTabRect(e.Index);

        using var bg = new SolidBrush(selected ? Background : Raised);
        e.Graphics.FillRectangle(bg, tabRect);

        using var pen = new Pen(selected ? Accent : Border);
        // Emphasise the top/active edge of the selected tab.
        if (selected)
            e.Graphics.DrawLine(pen, tabRect.Left, tabRect.Top, tabRect.Right, tabRect.Top);

        var text = tabs.TabPages[e.Index].Text;
        var color = selected ? Accent : Muted;
        TextRenderer.DrawText(e.Graphics, text, tabs.Font, tabRect, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
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
                case DateTimePicker dtp:
                    dtp.BackColor = Input;
                    dtp.ForeColor = Text;
                    dtp.CalendarMonthBackground = Input;
                    dtp.CalendarForeColor = Text;
                    dtp.CalendarTitleBackColor = Raised;
                    dtp.CalendarTitleForeColor = Text;
                    dtp.CalendarTrailingForeColor = Muted;
                    break;
                case MaskedTextBox mtb:
                    mtb.BackColor = Input;
                    mtb.ForeColor = Text;
                    mtb.BorderStyle = BorderStyle.FixedSingle;
                    mtb.Font = Body;
                    break;
                case LinkLabel link:
                    link.LinkColor = Accent;
                    link.ActiveLinkColor = AccentHover;
                    link.VisitedLinkColor = Accent;
                    link.BackColor = Color.Transparent;
                    break;
                case MonthCalendar cal:
                    cal.BackColor = Input;
                    cal.ForeColor = Text;
                    cal.TitleBackColor = Raised;
                    cal.TitleForeColor = Text;
                    cal.TrailingForeColor = Muted;
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
                    ApplyTabs(tabs);
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
