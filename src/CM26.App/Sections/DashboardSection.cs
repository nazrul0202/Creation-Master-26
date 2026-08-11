using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Overview: database info, entity counts, pending-change status.</summary>
public sealed class DashboardSection : SectionBase
{
    private readonly BufferedPanel _host;

    public override string SectionKey => "dashboard";
    public override string SectionTitle => "Dashboard";
    protected override string TableName => "";
    protected override bool SinglePane => true;

    public DashboardSection(AppServices s) : base(s)
    {
        _host = new BufferedPanel { Dock = DockStyle.Fill, BackColor = CardLayout.CardBackground, Padding = new Padding(12), AutoScroll = true };
        var page = new TabPage("Overview") { BackColor = Theme.Background };
        page.Controls.Add(_host);
        Tabs.TabPages.Add(page);
        Header.SetRecord("Dashboard", "Database overview and activity", IconService.Get("dashboard", 44));
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
            _host.Controls.Add(new Label
            {
                Text = "No game data loaded. Use File > Open Game (Ctrl+O).",
                Dock = DockStyle.Top, Height = 40, ForeColor = Theme.Muted, Font = Theme.Body,
            });
            _host.ResumeLayout();
            return;
        }

        var header = new Panel { Location = new Point(0, 0), Size = new Size(_host.Width - 24, 60), BackColor = CardLayout.CardBackground };
        header.Controls.Add(new Label
        {
            Text = $"Database: {Services.Session.LoadedFolder}",
            AutoSize = true, Location = new Point(0, 4), Font = Theme.Body, ForeColor = CardLayout.CardSubtle,
        });
        header.Controls.Add(new Label
        {
            Text = $"{Services.Pending.Count} pending changes",
            AutoSize = true, Location = new Point(0, 24), Font = Theme.BodyBold,
            ForeColor = Services.Pending.Count > 0 ? CardLayout.Fc26Yellow : CardLayout.CardMuted,
        });
        _host.Controls.Add(header);

        var flow = new FlowLayoutPanel { Location = new Point(0, 68), AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = CardLayout.CardBackground };

        flow.Controls.Add(Fc26StatCard("Tables", Services.Session.Tables.Count.ToString("N0"), null, CardLayout.Fc26Green));
        flow.Controls.Add(Fc26StatCard("Players", CountOf("players"), "players", CardLayout.Fc26Blue));
        flow.Controls.Add(Fc26StatCard("Teams", CountOf("teams"), "teams", CardLayout.Fc26Yellow));
        flow.Controls.Add(Fc26StatCard("Leagues", CountOf("leagues"), "leagues", CardLayout.Fc26Purple));
        flow.Controls.Add(Fc26StatCard("Nations", CountOf("nations"), "countries", CardLayout.Fc26Green));
        flow.Controls.Add(Fc26StatCard("Stadiums", CountOf("stadiums"), "stadiums", CardLayout.Fc26Blue));
        flow.Controls.Add(Fc26StatCard("Managers", CountOf("manager"), "managers", CardLayout.Fc26Yellow));
        flow.Controls.Add(Fc26StatCard("Referees", CountOf("referee"), "referees", CardLayout.Fc26Orange));
        flow.Controls.Add(Fc26StatCard("Kits", CountOf("teamkits"), "kits", CardLayout.Fc26Purple));
        flow.Controls.Add(Fc26StatCard("Formations", CountOf("formations"), "formations", CardLayout.Fc26Red));

        _host.Controls.Add(flow);
        _host.ResumeLayout();
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
}