using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Theming;

/// <summary>Central design system: palette, typography, spacing. Every control uses these constants.</summary>
public static class Theme
{
    // Palette
    // CM16-inspired desktop palette: a light work surface with the familiar
    // green command strip, while retaining clear FC26 validation colours.
    public static readonly Color Background = Color.FromArgb(245, 245, 245);
    public static readonly Color Panel = Color.FromArgb(250, 250, 250);
    public static readonly Color Raised = Color.FromArgb(238, 238, 238);
    public static readonly Color Border = Color.FromArgb(190, 190, 190);
    public static readonly Color Text = Color.FromArgb(25, 25, 25);
    public static readonly Color Muted = Color.FromArgb(92, 92, 92);
    public static readonly Color Accent = Color.FromArgb(0, 120, 215);
    public static readonly Color AccentHover = Color.FromArgb(0, 102, 184);
    public static readonly Color Danger = Color.FromArgb(196, 43, 28);
    public static readonly Color Success = Color.FromArgb(35, 130, 65);
    public static readonly Color Warning = Color.FromArgb(184, 116, 0);

    // Typography
    public static readonly Font Body = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font BodyBold = new("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font Label = new("Segoe UI Semibold", 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font SectionTitle = new("Segoe UI Semibold", 12f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font RecordTitle = new("Segoe UI Semibold", 15f, FontStyle.Regular, GraphicsUnit.Point);
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
        b.BackColor = primary ? Accent : Raised;
        b.ForeColor = primary ? Color.White : Text;
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
            b.FlatAppearance.MouseOverBackColor = Border;
            b.FlatAppearance.MouseDownBackColor = Border;
        }
    }

    public static void ApplyTextBox(TextBox t)
    {
        t.BackColor = Raised;
        t.ForeColor = Text;
        t.BorderStyle = BorderStyle.FixedSingle;
        t.Font = Body;
    }

    public static void ApplyCombo(ComboBox c)
    {
        c.BackColor = Raised;
        c.ForeColor = Text;
        c.FlatStyle = FlatStyle.Flat;
        c.Font = Body;
    }

    public static void ApplyGrid(DataGridView g)
    {
        g.BackgroundColor = Panel;
        g.BorderStyle = BorderStyle.None;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersDefaultCellStyle.BackColor = Raised;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        g.ColumnHeadersDefaultCellStyle.Font = Label;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Raised;
        g.ColumnHeadersHeight = 30;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        g.DefaultCellStyle.BackColor = Panel;
        g.DefaultCellStyle.ForeColor = Text;
        g.DefaultCellStyle.SelectionBackColor = Accent;
        g.DefaultCellStyle.SelectionForeColor = Color.White;
        g.DefaultCellStyle.Font = Body;
        g.AlternatingRowsDefaultCellStyle.BackColor = Raised;  // subtle zebra striping for scan-ability
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
    }
}
