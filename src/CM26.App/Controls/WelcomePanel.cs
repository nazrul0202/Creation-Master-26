using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls.Studio;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>Start screen shown before a database is opened.</summary>
public sealed class WelcomePanel : UserControl
{
    private readonly Panel _card;
    private readonly Label _title;
    private readonly Label _version;
    private readonly Label _subtitle;
    private readonly PictureBox _logo;

    public event EventHandler? OpenRequested;
    public event EventHandler<string>? FolderRequested;

    public WelcomePanel()
    {
        BackColor = Theme.Background;
        var center = new BufferedPanel { Size = new Size(600, 460), BackColor = Color.Transparent };

        _card = new StudioCard
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 12),
            AccentColor = StudioColors.Green,
        };
        // Brand left accent bar.
        _card.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = StudioColors.Green });

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));   // logo
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // title
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // version
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // subtitle
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));   // spacer
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // open button
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   // hint

        _logo = new PictureBox
        {
            Image = IconService.Get("dashboard", 84),
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
        };
        _title = new Label
        {
            Text = "Creation Master 26",
            Font = Theme.AppTitle,
            ForeColor = StudioColors.PrimaryText,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = true,
            Dock = DockStyle.Top,
        };
        _version = new Label
        {
            Text = "v" + Program.ProductVersion,
            Font = Theme.Body,
            ForeColor = StudioColors.MutedText,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = true,
            Dock = DockStyle.Top,
        };
        _subtitle = new Label
        {
            Text = "FC26 database editor — real data, validated edits, safe saves",
            Font = Theme.SectionTitle,
            ForeColor = StudioColors.MutedText,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = true,
            Dock = DockStyle.Top,
        };
        var open = new Button
        {
            Text = "Open FC26",
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Padding = new Padding(22, 10, 22, 10),
        };
        Theme.ApplyButton(open, primary: true);
        open.Font = Theme.ButtonLarge;
        open.Click += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
        var hint = new Label
        {
            Text = "Loads editable database and legacy assets directly from FC26 Data/Patch\r\nShortcut: Ctrl+O",
            Font = Theme.Muted9,
            ForeColor = StudioColors.MutedText,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = true,
            Dock = DockStyle.Top,
        };

        layout.Controls.Add(_logo, 0, 0);
        layout.Controls.Add(_title, 0, 1);
        layout.Controls.Add(_version, 0, 2);
        layout.Controls.Add(_subtitle, 0, 3);
        layout.Controls.Add(open, 0, 5);
        layout.Controls.Add(hint, 0, 6);

        var recentPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Height = 130,
            BackColor = Color.Transparent,
            Padding = new Padding(40, 4, 40, 0),
            Visible = false,
        };
        BuildRecentList(recentPanel);

        _card.Controls.Add(recentPanel);
        _card.Controls.Add(layout);
        center.Controls.Add(_card);
        Controls.Add(center);

        Resize += (_, _) => center.Location = new Point((Width - center.Width) / 2, (Height - center.Height) / 2 - 20);
    }

    /// <summary>Re-applies the card palette after a theme toggle.</summary>
    public void ApplyTheme()
    {
        _title.ForeColor = StudioColors.PrimaryText;
        _version.ForeColor = StudioColors.MutedText;
        _subtitle.ForeColor = StudioColors.MutedText;
    }

    private void BuildRecentList(FlowLayoutPanel recentPanel)
    {
        var recent = SettingsService.RecentFolders;
        if (recent.Count == 0) return;

        var heading = new Label
        {
            Text = "Recent",
            Font = Theme.Label,
            ForeColor = StudioColors.MutedText,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 4),
        };
        recentPanel.Controls.Add(heading);
        recentPanel.Visible = true;

        foreach (var folder in recent)
        {
            if (string.IsNullOrWhiteSpace(folder)) continue;
            var name = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar));
            if (string.IsNullOrWhiteSpace(name)) name = folder;
            var link = new LinkLabel
            {
                Text = name,
                AutoSize = true,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Font = Theme.Body,
                ForeColor = Theme.Text,
                ActiveLinkColor = Theme.Link,
                LinkColor = Theme.Link,
                VisitedLinkColor = Theme.Link,
                Margin = new Padding(0, 0, 0, 2),
            };
            var path = folder;
            link.Click += (_, _) => FolderRequested?.Invoke(this, path);
            recentPanel.Controls.Add(link);
        }
    }
}
