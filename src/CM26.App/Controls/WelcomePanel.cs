using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>Start screen shown before a database is opened.</summary>
public sealed class WelcomePanel : UserControl
{
    public event EventHandler? OpenRequested;

    public WelcomePanel()
    {
        BackColor = Theme.Background;
        var center = new BufferedPanel { Size = new Size(560, 320), BackColor = Color.Transparent };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Creation Master 26",
            Font = new Font("Segoe UI Semibold", 26f),
            ForeColor = Theme.Text,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
        };
        var subtitle = new Label
        {
            Text = "FC26 database editor — real data, validated edits, safe saves",
            Font = Theme.SectionTitle,
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = true,
            Dock = DockStyle.Top,
        };
        var open = new Button { Text = "📂  Open FC26", AutoSize = true, Anchor = AnchorStyles.None, Padding = new Padding(18, 10, 18, 10) };
        Theme.ApplyButton(open, primary: true);
        open.Font = new Font("Segoe UI Semibold", 11f);
        open.Click += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
        var hint = new Label
        {
            Text = "Loads editable database and legacy assets directly from FC26 Data/Patch\nShortcut: Ctrl+O",
            Font = Theme.Muted9,
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = true,
            Dock = DockStyle.Top,
        };

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(open, 0, 2);
        layout.Controls.Add(hint, 0, 3);
        center.Controls.Add(layout);
        Controls.Add(center);

        Resize += (_, _) => center.Location = new Point((Width - center.Width) / 2, (Height - center.Height) / 2 - 20);
    }
}
