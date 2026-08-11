using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Overview: database info, entity counts, pending-change status, quick actions.</summary>
public sealed class DashboardSection : SectionBase
{
    private readonly BufferedPanel _host;
    private Panel? _hero;
    private FlowLayoutPanel? _flow;
    private Label? _pendingHeroLabel;
    private Button? _explorerHeroBtn;
    private Button? _saveHeroBtn;

    public override string SectionKey => "dashboard";
    public override string SectionTitle => "Dashboard";
    protected override string TableName => "";
    protected override bool SinglePane => true;

    public DashboardSection(AppServices s) : base(s)
    {
        _host = new BufferedPanel { Dock = DockStyle.Fill, BackColor = CardLayout.CardBackground, Padding = new Padding(12), AutoScroll = true };
        _host.Resize += (_, _) => PositionContent();
        var page = new TabPage("Overview") { BackColor = Theme.Background };
        page.Controls.Add(_host);
        Tabs.TabPages.Add(page);
        Header.SetRecord("Dashboard", "Database overview and activity", IconService.Get("dashboard", 44));
    }

    /// <summary>Re-lays the hero and stat flow after the canvas is resized.</summary>
    private void PositionContent()
    {
        if (_hero == null) return;
        _hero.Width = Math.Max(0, _host.ClientSize.Width - 24);
        if (_explorerHeroBtn != null)
            _explorerHeroBtn.Location = new Point(_hero.Width - 300, 20);
        if (_saveHeroBtn != null)
            _saveHeroBtn.Location = new Point(_hero.Width - 172, 20);
        if (_flow != null)
            _flow.Location = new Point(0, _hero.Bottom + 8);
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();
    protected override void ShowRecord(int recordIndex) { }

    public override void ActivateSection()
    {
        RenderDashboard();
    }

    private void RenderDashboard()
    {
        _host.SuspendLayout();
        _host.Controls.Clear();
        if (!Services.Session.IsLoaded)
        {
            RenderEmptyState();
            _host.ResumeLayout();
            return;
        }

        _hero = BuildHero();
        _hero.Size = new Size(_host.ClientSize.Width - 24, 118);
        _host.Controls.Add(_hero);

        _flow = new FlowLayoutPanel
        {
            Location = new Point(0, _hero.Bottom + 8),
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = CardLayout.CardBackground,
        };
        _flow.Controls.Add(Fc26StatCard("Tables", Services.Session.Tables.Count.ToString("N0"), null, CardLayout.Fc26Green));
        _flow.Controls.Add(Fc26StatCard("Players", CountOf("players"), "players", CardLayout.Fc26Blue));
        _flow.Controls.Add(Fc26StatCard("Teams", CountOf("teams"), "teams", CardLayout.Fc26Yellow));
        _flow.Controls.Add(Fc26StatCard("Leagues", CountOf("leagues"), "leagues", CardLayout.Fc26Purple));
        _flow.Controls.Add(Fc26StatCard("Nations", CountOf("nations"), "countries", CardLayout.Fc26Green));
        _flow.Controls.Add(Fc26StatCard("Stadiums", CountOf("stadiums"), "stadiums", CardLayout.Fc26Blue));
        _flow.Controls.Add(Fc26StatCard("Managers", CountOf("manager"), "managers", CardLayout.Fc26Yellow));
        _flow.Controls.Add(Fc26StatCard("Referees", CountOf("referee"), "referees", CardLayout.Fc26Orange));
        _flow.Controls.Add(Fc26StatCard("Kits", CountOf("teamkits"), "kits", CardLayout.Fc26Purple));
        _flow.Controls.Add(Fc26StatCard("Formations", CountOf("formations"), "formations", CardLayout.Fc26Red));
        _host.Controls.Add(_flow);

        _host.ResumeLayout();
    }

    private void RenderEmptyState()
    {
        var hero = BuildHero();
        hero.Size = new Size(Math.Max(520, _host.ClientSize.Width - 24), 150);
        _host.Controls.Add(hero);

        var hint = new Label
        {
            Text = "Creates an editable snapshot of the FC26 database and legacy assets in Data/Patch.\n" +
                   "Writes are staged and validated before they reach the game files.",
            AutoSize = true,
            Location = new Point(0, hero.Bottom + 10),
            Font = Theme.Body,
            ForeColor = CardLayout.CardSubtle,
        };
        _host.Controls.Add(hint);
    }

    private Panel BuildHero()
    {
        var hero = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(_host.ClientSize.Width - 24, 118),
            BackColor = CardLayout.CardWhite,
        };
        CardLayout.ApplyRounded(hero, 14);
        hero.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = CardLayout.Fc26Green });

        var logo = new PictureBox
        {
            Image = IconService.Get("dashboard", 56),
            Size = new Size(56, 56),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(24, 16),
        };
        hero.Controls.Add(logo);

        if (!Services.Session.IsLoaded)
        {
            hero.Controls.Add(new Label
            {
                Text = "No game data loaded",
                Location = new Point(96, 20),
                AutoSize = true,
                Font = Theme.RecordTitle,
                ForeColor = CardLayout.CardText,
            });
            hero.Controls.Add(new Label
            {
                Text = "Open an EA SPORTS FC 26 installation to start editing its database and legacy assets.",
                Location = new Point(96, 52),
                AutoSize = true,
                Font = Theme.Body,
                ForeColor = CardLayout.CardSubtle,
            });
            var openBtn = new Button { Text = "Open FC26…", Location = new Point(96, 84), Size = new Size(130, 30) };
            Theme.ApplyButton(openBtn, primary: true);
            openBtn.Click += (_, _) => Services.RequestOpenGame();
            hero.Controls.Add(openBtn);
            var shortcut = new Label
            {
                Text = "Shortcut: Ctrl+O",
                Location = new Point(236, 89),
                AutoSize = true,
                Font = Theme.Muted9,
                ForeColor = CardLayout.CardSubtle,
            };
            hero.Controls.Add(shortcut);
            return hero;
        }

        var folder = Services.Session.LoadedFolder ?? string.Empty;
        hero.Controls.Add(new Label
        {
            Text = ShortenPath(folder),
            Location = new Point(96, 18),
            Size = new Size(Math.Max(200, hero.Width - 380), 22),
            Font = Theme.RecordTitle,
            ForeColor = CardLayout.CardText,
            AutoEllipsis = true,
        });
        _pendingHeroLabel = new Label
        {
            Text = $"{Services.Pending.Count} pending change(s)",
            Location = new Point(96, 48),
            AutoSize = true,
            Font = Theme.BodyBold,
            ForeColor = Services.Pending.Count > 0 ? CardLayout.Fc26Yellow : CardLayout.CardMuted,
        };
        hero.Controls.Add(_pendingHeroLabel);
        hero.Controls.Add(new Label
        {
            Text = "Edits are staged here, validated, then written to Data/Patch on save.",
            Location = new Point(96, 72),
            AutoSize = true,
            Font = Theme.Muted9,
            ForeColor = CardLayout.CardSubtle,
        });

        _explorerHeroBtn = new Button { Text = "Open Folder", Location = new Point(hero.Width - 300, 20), Size = new Size(120, 30) };
        Theme.ApplyButton(_explorerHeroBtn);
        var root = !string.IsNullOrWhiteSpace(Services.ActiveGameRoot)
            ? Services.ActiveGameRoot
            : SettingsService.FC26GameFolder;
        _explorerHeroBtn.Click += (_, _) =>
        {
            try
            {
                if (Directory.Exists(root)) Process.Start(new ProcessStartInfo("explorer.exe", $"\"{root}\"") { UseShellExecute = true });
            }
            catch { /* cannot open explorer */ }
        };
        _saveHeroBtn = new Button { Text = "Save Draft", Location = new Point(hero.Width - 172, 20), Size = new Size(120, 30) };
        Theme.ApplyButton(_saveHeroBtn, primary: true);
        _saveHeroBtn.Enabled = Services.Pending.Count > 0;
        _saveHeroBtn.Click += (_, _) => Services.RequestSaveDraft();
        hero.Controls.Add(_explorerHeroBtn);
        hero.Controls.Add(_saveHeroBtn);
        return hero;
    }

    private string CountOf(string table) => (Services.Session.GetTable(table)?.RowCount ?? 0).ToString("N0");

    private Control Fc26StatCard(string label, string value, string? navigateKey, Color accent)
    {
        var card = new Panel { Size = new Size(155, 80), BackColor = CardLayout.CardWhite, Margin = new Padding(4) };
        CardLayout.ApplyRounded(card, 10);
        card.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(155, 4), BackColor = accent });
        var v = new Label { Text = value, Location = new Point(8, 10), Size = new Size(140, 32), Font = Theme.RecordTitle, ForeColor = CardLayout.CardText, TextAlign = ContentAlignment.MiddleCenter };
        var l = new Label { Text = label, Location = new Point(8, 44), Size = new Size(140, 28), Font = Theme.Label, ForeColor = CardLayout.CardSubtle, TextAlign = ContentAlignment.TopCenter };
        card.Controls.Add(l);
        card.Controls.Add(v);
        if (navigateKey != null)
        {
            card.Cursor = Cursors.Hand;
            var key = navigateKey;
            card.Click += (_, _) => Services.RequestNavigation(key);
            l.Click += (_, _) => Services.RequestNavigation(key);
            v.Click += (_, _) => Services.RequestNavigation(key);
            card.MouseEnter += (_, _) => card.BackColor = CardLayout.CardFieldBg;
            card.MouseLeave += (_, _) => card.BackColor = CardLayout.CardWhite;
        }
        return card;
    }

    /// <summary>Keeps a long path readable in the hero card by showing only the tail segments.</summary>
    private static string ShortenPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (parts.Count <= 3) return path;
        return "…" + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar, parts.TakeLast(3));
    }
}
