using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>
/// One front door for the high-level Deco/RDBM/DBM-inspired workflows.  It is
/// deliberately only an orchestrator: every edit stays in the loaded CM26
/// transaction and the main Save command remains the only database/asset writer.
/// </summary>
internal sealed class Fc26PublicReadinessForm : Form
{
    private readonly MainForm _main;
    private readonly ComboBox _team = new ComboBox();
    private readonly TextBox _teamReport = ReportBox();
    private readonly TextBox _releaseReport = ReportBox();
    private readonly Label _pending = new Label();

    internal Fc26PublicReadinessForm(MainForm main)
    {
        _main = main ?? throw new ArgumentNullException(nameof(main));
        Text = "CM26 Public Readiness Centre — Direct FC26 Editing";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1040, 720);
        MinimumSize = new Size(860, 580);
        AutoScaleMode = AutoScaleMode.Dpi;
        Icon = main.Icon;

        var banner = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Color.FromArgb(12, 49, 92), Padding = new Padding(14, 8, 14, 6) };
        banner.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font(Font.FontFamily, 10.5f, FontStyle.Bold),
            Text = "DIRECT EDIT PIPELINE\r\nEvery action stages inside CM26. File > Save validates, creates a backup and commits directly to FC26. No FIFA Editing Tool import/export is used."
        });

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildReleasePage());
        tabs.TabPages.Add(BuildTeamPage());
        tabs.TabPages.Add(BuildPlayerPage());
        tabs.TabPages.Add(BuildCompetitionPage());
        tabs.TabPages.Add(BuildAssetPage());
        tabs.TabPages.Add(BuildAdvancedPage());

        _pending.Dock = DockStyle.Bottom;
        _pending.Height = 27;
        _pending.Padding = new Padding(8, 5, 0, 0);
        _pending.BorderStyle = BorderStyle.Fixed3D;
        Controls.Add(tabs);
        Controls.Add(_pending);
        Controls.Add(banner);

        PopulateTeams();
        RefreshReleaseReport();
    }

    private TabPage BuildReleasePage()
    {
        var page = Page("Release Gate");
        page.Controls.Add(_releaseReport);
        page.Controls.Add(ActionBar(
            Button("Refresh fast audit", (_, _) => RefreshReleaseReport()),
            Button("Full Database Health", (_, _) => _main.ShowFc26HealthCentre()),
            Button("ID availability", (_, _) => ShowText("Safe ID Availability", Fc26SnapshotLoader.DescribeIdAvailability())),
            Button("Schema compatibility", (_, _) => ShowText("FC26 Schema Compatibility", Fc26SnapshotLoader.DescribeCompatibility())),
            PrimaryButton("Save Direct to FC26", (_, _) => _main.CommitFc26DirectSave())));
        return page;
    }

    private TabPage BuildTeamPage()
    {
        var page = Page("Team Complete & Squad Doctor");
        _team.DropDownStyle = ComboBoxStyle.DropDownList;
        _team.Width = 330;
        _team.SelectedIndexChanged += (_, _) => RefreshTeamReport();
        var selector = ActionBar(Label("Team"), _team,
            Button("Audit team", (_, _) => RefreshTeamReport()),
            Button("Repair safe squad links", (_, _) => RepairSelectedTeam()),
            Button("Open Team Info", (_, _) => OpenSection("team")),
            Button("Open Squad Doctor", (_, _) => _main.ShowFc26RosterTools()),
            Button("Create New Team", (_, _) => { _main.CreateNewTeamWorkflow(); PopulateTeams(); }));
        page.Controls.Add(_teamReport);
        page.Controls.Add(selector);
        return page;
    }

    private TabPage BuildPlayerPage()
    {
        var page = Page("Players & Managers");
        page.Controls.Add(WorkflowPanel(
            "Player workflow",
            "Edit full player information, Tactical Roles and three-state PlayStyles; batch-edit attributes; import correctly scaled minifaces; repair duplicate/missing name records.",
            Button("Open Player Info", (_, _) => OpenSection("player")),
            Button("Batch Player Editor", (_, _) => _main.ShowFc26BatchPlayerEditor()),
            Button("Miniface & Face Tools", (_, _) => _main.ShowFc26FaceTools()),
            Button("Manager Editor", (_, _) => OpenSection("manager")),
            Button("Career Save", (_, _) => _main.ShowFc26CareerSaveModule()),
            Button("Player Names & Safe IDs", (_, _) => _main.ShowFc26ModdingUtilities()),
            "Manager data remains linked through the Team section. Team Complete flags a missing manager ID instead of inventing a raw manager relationship."));
        return page;
    }

    private TabPage BuildCompetitionPage()
    {
        var page = Page("League & Compdata Pro");
        page.Controls.Add(WorkflowPanel(
            "DBM-style competition workflow",
            "Create league and teams, assign tournament teams, generate the calendar, validate Compdata, then stage it into the same direct Save transaction.",
            Button("Create New League", (_, _) => _main.CreateNewLeagueWorkflow()),
            Button("Create New Team", (_, _) => _main.CreateNewTeamWorkflow()),
            Button("Open Compdata Pro", (_, _) => { _main.ShowFc26CompdataCentre(); Close(); }),
            Button("Open Competition Info", (_, _) => OpenSection("competition")),
            "A league is not claimed Career Ready until database mapping, stages, groups, at least two assigned teams and a valid schedule pass validation. Start a new Career after competition changes."));
        return page;
    }

    private TabPage BuildAssetPage()
    {
        var page = Page("Kit & Asset Centre");
        page.Controls.Add(WorkflowPanel(
            "Stable direct asset workflow",
            "Open team kits, import verified kit/crest/miniface assets, preview when decoding is supported and keep editing usable when a texture cannot be decoded.",
            Button("Open Kit Editor", (_, _) => OpenSection("kit")),
            Button("Kit Health Report", (_, _) => ShowText("FC26 Kit Health", BuildKitHealthReport())),
            Button("Direct Kit Assets", (_, _) => _main.ShowFc26AssetManager("Kit")),
            Button("All Visual Assets", (_, _) => _main.ShowFc26AssetManager()),
            Button("Miniface Batch Tools", (_, _) => _main.ShowFc26FaceTools()),
            "Native replacements are staged under verified FC26 logical paths. They are not exported to, or imported through, another editor."));
        return page;
    }

    private TabPage BuildAdvancedPage()
    {
        var page = Page("Bulk, Recovery & Safe IDs");
        page.Controls.Add(WorkflowPanel(
            "Power tools with guardrails",
            "Use preview-first batch edits, safe ID migration, readable activity history and the advanced workspace. External CSV/TSV files are optional input only; CM26 still validates and writes the result itself.",
            Button("Bulk Player Operations", (_, _) => _main.ShowFc26BatchPlayerEditor()),
            Button("Bulk Roster Operations", (_, _) => _main.ShowFc26RosterTools()),
            Button("Safe ID Migration", (_, _) => _main.ShowFc26ModdingUtilities()),
            Button("Advanced Database Workspace", (_, _) => _main.ShowFc26DatabaseWorkspace()),
            Button("Database Health", (_, _) => _main.ShowFc26HealthCentre()),
            Button("Revert All Unsaved DB Changes", (_, _) => { _main.RevertFc26UnsavedDatabaseChanges(); RefreshReleaseReport(); }),
            "Use advanced mode only when a friendly section cannot express the change. File > Save still blocks invalid references and creates the recovery backup."));
        return page;
    }

    private void PopulateTeams()
    {
        var current = SelectedTeam()?.Id ?? _main.CurrentFc26Team?.Id ?? -1;
        var teams = FifaEnvironment.Teams.Cast<Team>().Where(value => value != null)
            .OrderBy(value => value.ToString(), StringComparer.CurrentCultureIgnoreCase).ThenBy(value => value.Id).ToArray();
        _team.BeginUpdate();
        _team.DataSource = teams;
        _team.EndUpdate();
        var index = Array.FindIndex(teams, value => value.Id == current);
        if (index >= 0) _team.SelectedIndex = index;
        RefreshTeamReport();
    }

    private Team SelectedTeam() => _team.SelectedItem as Team;

    private void RefreshReleaseReport()
    {
        var teams = FifaEnvironment.Teams?.Cast<Team>().Where(value => value != null).ToArray() ?? Array.Empty<Team>();
        var players = FifaEnvironment.Players?.Cast<Player>().Where(value => value != null).ToArray() ?? Array.Empty<Player>();
        var leagues = FifaEnvironment.Leagues?.Cast<League>().Where(value => value != null).ToArray() ?? Array.Empty<League>();
        var missingLeague = teams.Count(value => value.IsClub() && value.League == null);
        var thinSquads = teams.Count(value => value.IsClub() && value.Roster.Cast<TeamPlayer>().Count(link => link?.Player != null) < 18);
        var missingKits = teams.Count(value => value.IsClub() && !HasCoreKits(value));
        var unlinkedPlayers = players.Count(value => value.GetClub() == null && !value.IsPlayingFor(FifaEnvironment.Teams.SearchId(111592) as Team));
        var sb = new StringBuilder();
        sb.AppendLine("CM26 PUBLIC RELEASE FAST AUDIT");
        sb.AppendLine(new string('=', 39));
        sb.AppendLine("Loaded source: " + Fc26SnapshotLoader.DescribeLoadedSource());
        sb.AppendLine();
        Pass(sb, "Direct writer", Fc26SnapshotLoader.IsLoaded, "CM26 snapshot/change-plan is loaded");
        Pass(sb, "League records", leagues.Length > 0, leagues.Length.ToString("N0") + " loaded");
        Pass(sb, "Club → league links", missingLeague == 0, missingLeague + " club(s) need a league");
        Pass(sb, "Core Home/Away/GK kit rows", missingKits == 0, missingKits + " club(s) need kit review");
        Pass(sb, "Public squad depth", thinSquads == 0, thinSquads + " club(s) have fewer than 18 players");
        Pass(sb, "Player club/free-agent links", unlinkedPlayers == 0, unlinkedPlayers + " player(s) need link review");
        sb.AppendLine();
        sb.AppendLine("This is a fast in-memory gate. Use Full Database Health and Compdata Career Ready Check before public release.");
        _releaseReport.Text = sb.ToString();
        RefreshPending();
    }

    private void RefreshTeamReport()
    {
        var team = SelectedTeam();
        if (team == null) { _teamReport.Text = "Select a team."; return; }
        var links = team.Roster.Cast<TeamPlayer>().Where(value => value?.Player != null).ToArray();
        var distinct = links.Select(value => value.Player.Id).Distinct().Count();
        var shirts = links.Select(value => value.jerseynumber).ToArray();
        var invalidShirts = shirts.Count(value => value < 1 || value > 99) + shirts.GroupBy(value => value).Sum(group => Math.Max(0, group.Count() - 1));
        var sb = new StringBuilder();
        sb.AppendLine(team + " [Team ID " + team.Id + "]");
        sb.AppendLine(new string('=', Math.Min(70, Math.Max(18, team.ToString().Length + 18))));
        Pass(sb, "Identity names", !string.IsNullOrWhiteSpace(team.TeamNameFull), "full/short names require review");
        Pass(sb, "Country", team.Country != null, team.Country?.ToString() ?? "missing");
        if (team.IsClub()) Pass(sb, "League relationship", team.League != null, team.League?.ToString() ?? "missing");
        Pass(sb, "Manager relationship", team.managerid > 0, team.managerid > 0 ? "manager ID " + team.managerid : "no manager assigned");
        if (team.IsClub()) Pass(sb, "Stadium relationship", team.Stadium != null, team.Stadium?.ToString() ?? "missing");
        Pass(sb, "Roster links", distinct == links.Length, links.Length + " rows / " + distinct + " unique players");
        Pass(sb, "Squad depth", !team.IsClub() || distinct >= 18, distinct + " players (18 recommended)");
        Pass(sb, "Starting XI", links.Count(value => value.position < 28) >= 11, links.Count(value => value.position < 28) + " populated XI slots");
        Pass(sb, "Formation", team.Formation != null, team.Formation?.ToString() ?? "missing");
        Pass(sb, "Shirt numbers", invalidShirts == 0, invalidShirts + " invalid/duplicate value(s)");
        if (team.IsClub()) Pass(sb, "Home/Away/GK kits", HasCoreKits(team), KitSummary(team));
        sb.AppendLine();
        sb.AppendLine("Safe squad repair removes duplicate/null player links, assigns unique shirts and refreshes bench/set-piece roles. Missing league, stadium, manager or kits require a user choice and are never guessed.");
        _teamReport.Text = sb.ToString();
        RefreshPending();
    }

    private void RepairSelectedTeam()
    {
        var team = SelectedTeam();
        if (team == null) return;
        if (MessageBox.Show(this,
            "Stage safe squad repairs for " + team + "?\r\n\r\nCM26 will remove null/duplicate roster links, assign unique shirt numbers and refresh available formation, bench and set-piece roles. Relationships and assets are never guessed.",
            "Team Complete — Preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var repaired = Fc26RosterToolsForm.RepairTeam(team);
        Fc26ActivityLog.Add("Team Complete", team.Id + ": " + repaired + " safe squad repair(s) staged");
        RefreshTeamReport();
        MessageBox.Show(this, repaired + " safe repair(s) staged. Review the report, then use Save Direct to FC26.",
            "Team Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OpenSection(string section)
    {
        _main.ShowFc26Section(section);
        Close();
    }

    private void RefreshPending()
    {
        _pending.Text = "Pending advanced changes: " + Fc26SnapshotLoader.PendingDetailCount.ToString("N0") +
            "  |  Normal File > Save is the only commit path.";
    }

    private static bool HasCoreKits(Team team)
    {
        var types = new HashSet<int>(team.m_KitList.Cast<Kit>().Where(value => value != null).Select(value => value.kittype));
        return types.Contains(0) && types.Contains(1) && types.Contains(2);
    }

    private static string KitSummary(Team team)
    {
        var types = new HashSet<int>(team.m_KitList.Cast<Kit>().Where(value => value != null).Select(value => value.kittype));
        return "Home " + (types.Contains(0) ? "OK" : "missing") + ", Away " + (types.Contains(1) ? "OK" : "missing") +
               ", GK " + (types.Contains(2) ? "OK" : "missing") + (types.Contains(3) ? ", Third OK" : string.Empty);
    }

    private static string BuildKitHealthReport()
    {
        var clubs = FifaEnvironment.Teams?.Cast<Team>().Where(team => team != null && team.IsClub())
            .OrderBy(team => team.TeamNameFull, StringComparer.CurrentCultureIgnoreCase).ToArray() ?? Array.Empty<Team>();
        var builder = new StringBuilder();
        builder.AppendLine("CM26 CORE KIT HEALTH");
        builder.AppendLine(new string('=', 28));
        var findings = 0;
        foreach (var team in clubs)
        {
            var kits = team.m_KitList.Cast<Kit>().Where(kit => kit != null).ToArray();
            var types = kits.Select(kit => kit.kittype).ToArray();
            var missing = new[] { 0, 1, 2 }.Where(type => !types.Contains(type)).ToArray();
            var duplicate = types.GroupBy(type => type).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
            var wrongOwner = kits.Count(kit => kit.teamid != team.Id);
            if (missing.Length == 0 && duplicate.Length == 0 && wrongOwner == 0) continue;
            findings++;
            builder.Append("[FIX] ").Append(team.TeamNameFull).Append(" [").Append(team.Id).Append("]: ");
            if (missing.Length > 0) builder.Append("missing ").Append(string.Join("/", missing.Select(type => type == 0 ? "Home" : type == 1 ? "Away" : "GK"))).Append("; ");
            if (duplicate.Length > 0) builder.Append("duplicate kit type ").Append(string.Join(",", duplicate)).Append("; ");
            if (wrongOwner > 0) builder.Append(wrongOwner).Append(" wrong team link(s); ");
            builder.AppendLine();
        }
        if (findings == 0) builder.AppendLine("[PASS] Every club has one Home, Away and GK kit row with valid team ownership.");
        builder.AppendLine();
        builder.AppendLine("This report checks database kit rows. Use Direct Kit Assets to inspect or stage the corresponding FC26 textures.");
        return builder.ToString();
    }

    private static void Pass(StringBuilder builder, string label, bool passed, string detail)
    {
        builder.Append('[').Append(passed ? "PASS" : "FIX ").Append("] ").Append(label).Append(": ").AppendLine(detail);
    }

    private static TabPage Page(string name) => new TabPage(name) { Padding = new Padding(8) };

    private static TextBox ReportBox() => new TextBox
    {
        Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both,
        Font = new Font("Consolas", 9.25f), BackColor = Color.White, WordWrap = false
    };

    private static FlowLayoutPanel ActionBar(params Control[] controls)
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(4), WrapContents = false, AutoScroll = true };
        panel.Controls.AddRange(controls);
        return panel;
    }

    private static Control WorkflowPanel(string title, string description, params object[] content)
    {
        var pageNote = content.LastOrDefault() as string;
        var buttons = content.OfType<Button>().Cast<Control>().ToArray();
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18) };
        var heading = new Label { Text = title, Dock = DockStyle.Top, Height = 34, Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 13f, FontStyle.Bold), ForeColor = Color.FromArgb(13, 76, 126) };
        var body = new Label { Text = description, Dock = DockStyle.Top, Height = 62, AutoEllipsis = true };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 92, WrapContents = true };
        actions.Controls.AddRange(buttons);
        var note = new Label { Text = pageNote ?? string.Empty, Dock = DockStyle.Top, Height = 72, ForeColor = Color.DarkGreen, Padding = new Padding(0, 12, 0, 0) };
        panel.Controls.Add(note); panel.Controls.Add(actions); panel.Controls.Add(body); panel.Controls.Add(heading);
        return panel;
    }

    private static Label Label(string text) => new Label { Text = text, AutoSize = true, Padding = new Padding(0, 7, 0, 0) };
    private static Button Button(string text, EventHandler action) { var button = new Button { Text = text, AutoSize = true, Height = 31 }; button.Click += action; return button; }
    private static Button PrimaryButton(string text, EventHandler action) { var button = Button(text, action); button.BackColor = Color.FromArgb(20, 108, 55); button.ForeColor = Color.White; return button; }

    private void ShowText(string title, string text)
    {
        using (var form = new Form { Text = title, Size = new Size(800, 580), StartPosition = FormStartPosition.CenterParent, Icon = Icon })
        {
            form.Controls.Add(new TextBox { Text = text, Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9f) });
            form.ShowDialog(this);
        }
    }
}
