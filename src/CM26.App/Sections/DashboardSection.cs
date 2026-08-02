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
        // Hide the split browser; dashboard is a single pane.
        _host = new BufferedPanel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(8), AutoScroll = true };
        var page = new TabPage("Overview") { BackColor = Theme.Background };
        page.Controls.Add(_host);
        Tabs.TabPages.Add(page);
        Header.SetRecord("Dashboard", "Database overview and activity", IconService.Get("dashboard", 44));
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();

    protected override void ShowRecord(int recordIndex) { }

    public override void ActivateSection()
    {
        // Dashboard has no record browser. Calling the base implementation loads an
        // empty browser and shows its EmptyState over this section's overview.
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

        var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };

        flow.Controls.Add(StatCard("Tables", Services.Session.Tables.Count.ToString("N0")));
        flow.Controls.Add(StatCard("Players", CountOf("players")));
        flow.Controls.Add(StatCard("Teams", CountOf("teams")));
        flow.Controls.Add(StatCard("Leagues", CountOf("leagues")));
        flow.Controls.Add(StatCard("Nations", CountOf("nations")));
        flow.Controls.Add(StatCard("Stadiums", CountOf("stadiums")));
        flow.Controls.Add(StatCard("Managers", CountOf("manager")));
        flow.Controls.Add(StatCard("Referees", CountOf("referee")));
        flow.Controls.Add(StatCard("Kits", CountOf("teamkits")));
        flow.Controls.Add(StatCard("Formations", CountOf("formations")));

        _host.Controls.Add(flow);
        _host.Controls.Add(InfoBar($"Database: {Services.Session.LoadedFolder}"));
        _host.Controls.Add(InfoBar($"Pending changes: {Services.Pending.Count}"));
        // correct dock order (top stack)
        for (int i = 0; i < _host.Controls.Count; i++)
            _host.Controls[i].Dock = DockStyle.Top;
        _host.ResumeLayout();
    }

    private string CountOf(string table) => (Services.Session.GetTable(table)?.RowCount ?? 0).ToString("N0");

    private static Control InfoBar(string text) => new Label
    {
        Text = text,
        AutoSize = false,
        Height = 26,
        ForeColor = Theme.Text,
        Font = Theme.Body,
        Padding = new Padding(4, 4, 0, 0),
    };

    private static Control StatCard(string label, string value)
    {
        var card = new BufferedPanel { Size = new Size(150, 72), BackColor = Theme.Raised, Margin = new Padding(4), BorderStyle = BorderStyle.FixedSingle };
        var v = new Label { Text = value, Dock = DockStyle.Top, Height = 39, Font = new Font("Segoe UI Semibold", 15f), ForeColor = Theme.Text, TextAlign = ContentAlignment.MiddleCenter };
        var l = new Label { Text = label, Dock = DockStyle.Fill, Font = Theme.Label, ForeColor = Theme.Muted, TextAlign = ContentAlignment.TopCenter };
        card.Controls.Add(l);
        card.Controls.Add(v);
        return card;
    }
}
