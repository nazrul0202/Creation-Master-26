using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;

namespace CM26.App.Theming;

/// <summary>Central design system: palette, typography, spacing. Every control uses these.</summary>
public static class Theme
{
    private static bool _dark = true;

    static Theme() => ApplyPalette();

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
            // Dark variant kept as an alternative; default is the FC Editor light theme.
            _background = Color.FromArgb(14, 16, 20);
            _panel = Color.FromArgb(22, 25, 31);
            _raised = Color.FromArgb(31, 36, 44);
            _input = Color.FromArgb(17, 20, 25);
            _border = Color.FromArgb(48, 55, 65);
            _text = Color.FromArgb(224, 224, 224);          // Primary text (#E0E0E0)
            _muted = Color.FromArgb(150, 152, 158);         // Labels / secondary text
            _accent = Color.FromArgb(56, 189, 248);
            _accentHover = Color.FromArgb(14, 165, 233);
            _link = Color.FromArgb(0, 123, 255);            // Link blue (#007BFF)
            _danger = Color.FromArgb(201, 42, 42);          // Material red (#C92A2A)
            _success = Color.FromArgb(76, 175, 80);         // Material green (#4CAF50)
            _warning = Color.FromArgb(230, 119, 0);         // Orange (#E67700)
            _validationBackground = Color.FromArgb(46, 33, 34);
            _validationListBackground = Color.FromArgb(56, 40, 42);
            _validationText = Color.FromArgb(255, 150, 150);
        }
        else
        {
            // FC Editor (decoruiz) light theme: #F0F0F0 canvas, white cards,
            // Microsoft blue accent, flat Tk-style controls.
            _background = Color.FromArgb(240, 240, 240);    // App canvas (#F0F0F0)
            _panel = Color.FromArgb(255, 255, 255);         // Cards / group boxes
            _raised = Color.FromArgb(227, 227, 227);        // Hover, headers, elevated (#E3E3E3)
            _input = Color.FromArgb(255, 255, 255);         // Input wells / grid body
            _border = Color.FromArgb(189, 189, 189);        // Hairline separators
            _text = Color.FromArgb(26, 26, 26);             // Primary text
            _muted = Color.FromArgb(105, 105, 105);         // Labels / secondary text
            _accent = Color.FromArgb(0, 120, 212);          // Microsoft blue (#0078D4)
            _accentHover = Color.FromArgb(0, 96, 171);      // Deeper hover (#0060AB)
            _link = Color.FromArgb(0, 120, 212);
            _danger = Color.FromArgb(201, 42, 42);          // Material red (#C92A2A)
            _success = Color.FromArgb(76, 175, 80);         // Material green (#4CAF50)
            _warning = Color.FromArgb(230, 119, 0);         // Orange (#E67700)
            _validationBackground = Color.FromArgb(255, 244, 244);
            _validationListBackground = Color.FromArgb(255, 250, 250);
            _validationText = Color.FromArgb(175, 32, 32);
        }
        CardLayout.ApplyTheme();
    }

    private static Color _background = Color.FromArgb(36, 37, 41);
    private static Color _panel = Color.FromArgb(43, 45, 51);
    private static Color _raised = Color.FromArgb(51, 53, 60);
    private static Color _input = Color.FromArgb(28, 29, 33);
    private static Color _border = Color.FromArgb(61, 63, 70);
    private static Color _text = Color.FromArgb(224, 224, 224);
    private static Color _muted = Color.FromArgb(150, 152, 158);
    private static Color _accent = Color.FromArgb(0, 120, 212);
    private static Color _accentHover = Color.FromArgb(0, 112, 186);
    private static Color _link = Color.FromArgb(0, 123, 255);
    private static Color _danger = Color.FromArgb(201, 42, 42);
    private static Color _success = Color.FromArgb(76, 175, 80);
    private static Color _warning = Color.FromArgb(230, 119, 0);
    private static Color _validationBackground = Color.FromArgb(46, 33, 34);
    private static Color _validationListBackground = Color.FromArgb(56, 40, 42);
    private static Color _validationText = Color.FromArgb(255, 150, 150);

    public static Color Background => _background;
    public static Color Panel => _panel;
    public static Color Raised => _raised;
    public static Color Input => _input;
    public static Color Border => _border;
    public static Color Text => _text;
    public static Color Muted => _muted;
    public static Color Accent => _accent;
    public static Color AccentHover => _accentHover;
    public static Color Link => _link;
    public static Color Danger => _danger;
    public static Color Success => _success;
    public static Color Warning => _warning;
    public static Color ValidationBackground => _validationBackground;
    public static Color ValidationListBackground => _validationListBackground;
    public static Color ValidationText => _validationText;

    // FC26 brand accent for primary actions (green #74B922 family).
    public static Color Brand => Color.FromArgb(116, 185, 34);
    public static Color BrandHover => Color.FromArgb(103, 168, 28);
    public static Color BrandDown => Color.FromArgb(88, 146, 22);
    public static Color BrandSoft => Color.FromArgb(235, 246, 220);

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
    public const int SidebarWidth = 248;
    public const int NavItemHeight = 38;

    public static void ApplyButton(Button b, bool primary = false)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.BorderColor = primary ? Brand : Border;
        b.BackColor = primary ? Brand : Panel;
        b.ForeColor = primary ? Color.White : Text;
        b.Font = primary ? BodyBold : Body;
        // Keep explicit custom heights; only normalize the WinForms default 23px.
        if (b.Height == 23) b.Height = ControlHeight;
        b.Cursor = Cursors.Hand;
        b.UseVisualStyleBackColor = false;
        // Visible focus border for keyboard navigation; cleared when focus leaves.
        // Named handlers (instead of lambdas) so re-theming never stacks duplicates.
        b.GotFocus -= ButtonGotFocus;
        b.LostFocus -= ButtonLostFocus;
        b.GotFocus += ButtonGotFocus;
        b.LostFocus += ButtonLostFocus;
        if (primary)
        {
            b.FlatAppearance.MouseOverBackColor = BrandHover;
            b.FlatAppearance.MouseDownBackColor = BrandDown;
        }
        else
        {
            b.FlatAppearance.MouseOverBackColor = Raised;
            b.FlatAppearance.MouseDownBackColor = Raised;
        }
    }

    private static void ButtonGotFocus(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;
        b.FlatAppearance.BorderColor = Accent;
    }

    private static void ButtonLostFocus(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;
        b.FlatAppearance.BorderColor = b.BackColor == Brand ? Brand : Border;
    }

    public static void ApplyTextBox(TextBox t)
    {
        t.BackColor = Input;
        t.ForeColor = Text;
        t.BorderStyle = BorderStyle.FixedSingle;
        t.Font = Body;
        // Accent border while focused so the active field is obvious for keyboard users.
        EventHandler gotFocus = (_, _) => t.BorderStyle = BorderStyle.FixedSingle;
        PaintEventHandler paint = (_, e) =>
        {
            if (!t.Focused) return;
            using var pen = new Pen(Accent);
            e.Graphics.DrawRectangle(pen, 0, 0, t.Width - 1, t.Height - 1);
        };
        t.GotFocus -= gotFocus;
        t.Paint -= paint;
        t.GotFocus += gotFocus;
        t.Paint += paint;
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
        c.DrawItem -= ComboDrawItem;
        c.DrawItem += ComboDrawItem;
    }

    private static void ComboDrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        if (sender is not ComboBox combo) return;
        using var bg = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ? Accent : Input);
        e.Graphics.FillRectangle(bg, e.Bounds);
        var text = combo.GetItemText(combo.Items[e.Index]);
        var color = Text;
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
        g.DefaultCellStyle.SelectionForeColor = Text;
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
        g.CellBorderStyle = DataGridViewCellBorderStyle.Single;
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
            column.DefaultCellStyle.SelectionForeColor = Text;
            column.DefaultCellStyle.Font = Body;
        }
    }

    /// <summary>
    /// Enables owner-drawn, dark tab headers so the native tab strip matches the
    /// CM26 palette instead of the default light system tab header.
    /// </summary>
    public static void ApplyTabs(TabControl tabs)
    {
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.ItemSize = new Size(104, 30);
        tabs.Padding = new Point(8, 4);
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

        using var bg = new SolidBrush(selected ? Panel : Background);
        e.Graphics.FillRectangle(bg, tabRect);

        using var pen = new Pen(selected ? Accent : Border);
        // Emphasise the top/active edge of the selected tab.
        e.Graphics.DrawRectangle(pen, tabRect.Left, tabRect.Top, tabRect.Width - 1, tabRect.Height - 1);

        var text = tabs.TabPages[e.Index].Text;
        var color = selected ? Text : Muted;
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
                    // Preserve explicit styling (e.g. green validation boxes); only
                    // map WinForms system defaults to the current palette.
                    if (checkBox.ForeColor == SystemColors.ControlText || checkBox.ForeColor == SystemColors.WindowText)
                        checkBox.ForeColor = Text;
                    if (checkBox.BackColor == SystemColors.Control)
                        checkBox.BackColor = Background;
                    checkBox.Font = Body;
                    checkBox.FlatStyle = FlatStyle.Flat;
                    break;
                case RadioButton radio:
                    if (radio.ForeColor == SystemColors.ControlText || radio.ForeColor == SystemColors.WindowText)
                        radio.ForeColor = Text;
                    if (radio.BackColor == SystemColors.Control)
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
                    link.LinkColor = Link;
                    link.ActiveLinkColor = AccentHover;
                    link.VisitedLinkColor = Link;
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
                    // Preserve explicitly-set colors (muted captions, warnings);
                    // only remap the WinForms system defaults.
                    if (label.ForeColor == SystemColors.ControlText || label.ForeColor == SystemColors.WindowText || label.ForeColor == SystemColors.GrayText)
                        label.ForeColor = label.ForeColor == SystemColors.GrayText ? Muted : Text;
                    if (label.BackColor == SystemColors.Control)
                        label.BackColor = Color.Transparent;
                    if (IsDark && label.BackColor == Color.White)
                        label.BackColor = Panel;
                    label.Font = label.Font.FontFamily.Name.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase) ? Body : label.Font;
                    break;
                case Panel panel when control is not PictureBox:
                    if (panel.BackColor == SystemColors.Control || (IsDark && panel.BackColor == Color.White))
                        panel.BackColor = Background;
                    if (panel.ForeColor == SystemColors.ControlText || panel.ForeColor == SystemColors.WindowText)
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
