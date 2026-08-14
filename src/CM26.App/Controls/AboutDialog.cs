using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls.Studio;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>
/// Branded About dialog: logo, version, feature summary, and links to the
/// project repository, releases page, and license.
/// </summary>
public static class AboutDialog
{
    private const string RepoUrl = "https://github.com/nazrul0202/Creation-Master-26";
    private const string ReleasesUrl = RepoUrl + "/releases";
    private const string LicenseUrl = RepoUrl + "/blob/main/LICENSE";

    public static void Show(IWin32Window? owner)
    {
        using var dialog = new Form
        {
            Text = "About Creation Master 26",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(540, 440),
            BackColor = Theme.Background,
            ForeColor = Theme.Text,
            Font = Theme.Body,
        };

        var canvas = new BufferedPanel { Dock = DockStyle.Fill, BackColor = StudioColors.AppBackground, Padding = new Padding(16) };
        canvas.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = StudioColors.Green });

        var card = new StudioCard { Dock = DockStyle.Fill, Padding = new Padding(24), AccentColor = StudioColors.Green };

        var logo = new PictureBox
        {
            Image = IconService.Get("dashboard", 84),
            Size = new Size(84, 84),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(24, 20),
        };
        var title = new Label
        {
            Text = "Creation Master 26",
            Font = Theme.AppTitle,
            ForeColor = StudioColors.PrimaryText,
            AutoSize = true,
            Location = new Point(124, 26),
        };
        var version = new Label
        {
            Text = $"Version {Program.ProductVersion}",
            Font = Theme.Label,
            ForeColor = StudioColors.MutedText,
            AutoSize = true,
            Location = new Point(126, 64),
        };
        var description = new Label
        {
            Text = "Database, competition data and legacy asset editor for EA SPORTS FC 26.\r\n" +
                   "Validated edits, safe saves, and an honest read-only player-name view.\r\n" +
                   "Unofficial, independent community tool by Rizco98.",
            Font = Theme.Body,
            ForeColor = StudioColors.PrimaryText,
            AutoSize = true,
            Location = new Point(24, 128),
        };
        var linksHeader = new Label
        {
            Text = "Project links",
            Font = Theme.Label,
            ForeColor = StudioColors.Green,
            AutoSize = true,
            Location = new Point(24, 214),
        };
        var github = Link("GitHub repository", RepoUrl);
        var releases = Link("Releases", ReleasesUrl);
        var license = Link("License (MIT)", LicenseUrl);
        github.Location = new Point(24, 242);
        releases.Location = new Point(184, 242);
        license.Location = new Point(300, 242);
        var footerNote = new Label
        {
            Text = "Not affiliated with Electronic Arts. Use File > Open Game to begin — see Help > Keyboard Shortcuts.",
            Font = Theme.Muted9,
            ForeColor = StudioColors.MutedText,
            AutoSize = true,
            Location = new Point(24, 296),
        };
        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            Location = new Point(412, 338),
            Size = new Size(96, 30),
        };
        Theme.ApplyButton(ok, primary: true);

        card.Controls.Add(logo);
        card.Controls.Add(title);
        card.Controls.Add(version);
        card.Controls.Add(description);
        card.Controls.Add(linksHeader);
        card.Controls.Add(github);
        card.Controls.Add(releases);
        card.Controls.Add(license);
        card.Controls.Add(footerNote);
        card.Controls.Add(ok);
        canvas.Controls.Add(card);
        dialog.Controls.Add(canvas);
        dialog.AcceptButton = ok;

        dialog.ShowDialog(owner);
    }

    private static LinkLabel Link(string text, string url)
    {
        var link = new LinkLabel
        {
            Text = text,
            AutoSize = true,
            LinkBehavior = LinkBehavior.HoverUnderline,
            Font = Theme.Body,
            ForeColor = StudioColors.PrimaryText,
            ActiveLinkColor = StudioColors.CyanAccent,
            LinkColor = StudioColors.CyanAccent,
            VisitedLinkColor = StudioColors.CyanAccent,
        };
        link.LinkClicked += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex) { Program.Log($"[CM26] Could not open browser: {ex.Message}"); /* cannot open browser */ }
        };
        return link;
    }
}
