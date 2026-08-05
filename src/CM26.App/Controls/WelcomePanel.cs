using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>Start screen shown before a database is opened.</summary>
public sealed class WelcomePanel : UserControl
{
    public event EventHandler? OpenRequested;
    /// <summary>Raised when the user picks a folder from the recent list.</summary>
    public event EventHandler<string>? FolderRequested;

    public WelcomePanel()
    {
        BackColor = Theme.Background;
        var center = new BufferedPanel { Size = new Size(560, 320), BackColor = Color.Transparent };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Creation Master 26",
            Font = Theme.AppTitle,
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
        open.Font = Theme.ButtonLarge;
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

        var recentPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            AutoSize = false,
            Height = 140,
            BackColor = Color.Transparent,
            Padding = new Padding(40, 6, 40, 0),
        };
        recentPanel.Visible = false;

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(open, 0, 2);
        layout.Controls.Add(hint, 0, 3);
        layout.Controls.Add(recentPanel, 0, 4);
        center.Controls.Add(layout);
        Controls.Add(center);

        BuildRecentList(recentPanel);
        Resize += (_, _) => center.Location = new Point((Width - center.Width) / 2, (Height - center.Height) / 2 - 20);
    }

    private void BuildRecentList(FlowLayoutPanel recentPanel)
    {
        var recent = SettingsService.RecentFolders;
        if (recent.Count == 0) return;

        var heading = new Label
        {
            Text = "Recent",
            Font = Theme.Label,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 4),
        };
        recentPanel.Controls.Add(heading);
        recentPanel.Visible = true;

        foreach (var folder in recent)
        {
            var name = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar));
            if (string.IsNullOrWhiteSpace(name)) name = folder;
            var link = new LinkLabel
            {
                Text = name,
                AutoSize = true,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Font = Theme.Body,
                ForeColor = Theme.Text,
                ActiveLinkColor = Theme.Accent,
                LinkColor = Theme.Accent,
                VisitedLinkColor = Theme.Accent,
                Margin = new Padding(0, 0, 0, 2),
            };
            var path = folder;
            link.Click += (_, _) => FolderRequested?.Invoke(this, path);
            recentPanel.Controls.Add(link);
        }
    }
}
