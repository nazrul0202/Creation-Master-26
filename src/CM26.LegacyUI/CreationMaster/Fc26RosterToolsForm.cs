using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>FC26 roster, national-team and youth workflows in the classic CM shell.</summary>
internal sealed class Fc26RosterToolsForm : Form
{
    private readonly ComboBox _team = new ComboBox();
    private readonly ComboBox _targetTeam = new ComboBox();
    private readonly TextBox _search = new TextBox();
    private readonly DataGridView _roster = new DataGridView();
    private readonly ListBox _available = new ListBox();
    private readonly Label _summary = new Label();
    private readonly DateTimePicker _joinDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 105 };
    private readonly DateTimePicker _loanEnd = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 105 };
    private readonly NumericUpDown _contractYear = new NumericUpDown { Minimum = 2026, Maximum = 2100, Value = 2030, Width = 68 };
    private readonly CheckBox _loanToBuy = new CheckBox { Text = "Loan-to-buy", AutoSize = true };
    private Team[] _teams = Array.Empty<Team>();

    internal Fc26RosterToolsForm()
    {
        Text = "FC26 Roster, National Team & Youth Tools";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1120, 720);
        MinimumSize = new Size(900, 580);
        Icon = Form.ActiveForm?.Icon;

        var selectors = new TableLayoutPanel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(8), ColumnCount = 6, RowCount = 2 };
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
        selectors.Controls.Add(Label("Current team"), 0, 0); selectors.Controls.Add(_team, 1, 0);
        selectors.Controls.Add(Label("Target team"), 2, 0); selectors.Controls.Add(_targetTeam, 3, 0);
        selectors.Controls.Add(Label("Search"), 4, 0); selectors.Controls.Add(_search, 5, 0);
        _summary.AutoSize = true; _summary.ForeColor = Color.DarkGreen; _summary.Anchor = AnchorStyles.Left;
        selectors.Controls.Add(_summary, 0, 1); selectors.SetColumnSpan(_summary, 6);
        _team.DropDownStyle = ComboBoxStyle.DropDownList; _targetTeam.DropDownStyle = ComboBoxStyle.DropDownList;
        _team.SelectedIndexChanged += (_, _) => RefreshAll();
        _search.TextChanged += (_, _) => RefreshAvailable();

        ConfigureRosterGrid();
        _available.DisplayMember = "Display"; _available.SelectionMode = SelectionMode.MultiExtended; _available.Dock = DockStyle.Fill;
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 720 };
        var rosterBox = new GroupBox { Text = "Real roster — XI / substitutes / reserves", Dock = DockStyle.Fill, Padding = new Padding(8) };
        rosterBox.Controls.Add(_roster);
        var availableBox = new GroupBox { Text = "Available players", Dock = DockStyle.Fill, Padding = new Padding(8) };
        availableBox.Controls.Add(_available);
        split.Panel1.Controls.Add(rosterBox); split.Panel2.Controls.Add(availableBox);

        _joinDate.Value = DateTime.Today; _loanEnd.Value = DateTime.Today.AddYears(1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 122, Padding = new Padding(8), WrapContents = true };
        actions.Controls.AddRange(new Control[]
        {
            Button("Add / transfer selected", (_, _) => AddSelected()),
            Button("Transfer roster → target", (_, _) => TransferSelected()),
            Button("Transfer ALL → target", (_, _) => TransferAll()),
            Label("Join"), _joinDate, Label("Contract"), _contractYear,
            Button("Start loan → target", (_, _) => StartLoan()), _loanEnd, _loanToBuy,
            Button("Terminate selected loan", (_, _) => TerminateLoan()),
            Button("Remove from team", (_, _) => RemoveSelected()),
            Button("Release to Free Agents", (_, _) => ReleaseSelected()),
            Button("National call-up", (_, _) => NationalCallUp()),
            Button("Remove national call-up", (_, _) => RemoveNationalCallUp()),
            Button("Validate national squad", (_, _) => ValidateNationalSquad()),
            Button("Sync nationality links", (_, _) => SyncNationalityLinks()),
            Button("Replace injured call-ups", (_, _) => ReplaceInjuredCallUps()),
            Button("Merge / sync U21 → target", (_, _) => SyncYouth()),
            Button("Export U21 CSV", (_, _) => ExportYouth()),
            Button("Import / merge U21 CSV", (_, _) => ImportYouth()),
            Button("Auto Best XI", (_, _) => AutoBestXi()),
            Button("Repair roster", (_, _) => RepairRoster()),
            Button("Export roster CSV", (_, _) => ExportRoster()),
            Button("Import roster CSV", (_, _) => ImportRoster())
        });

        Controls.Add(split); Controls.Add(actions); Controls.Add(selectors);
        LoadTeams();
    }

    private void ConfigureRosterGrid()
    {
        _roster.Dock = DockStyle.Fill; _roster.AllowUserToAddRows = false; _roster.AllowUserToDeleteRows = false;
        _roster.ReadOnly = true; _roster.MultiSelect = true; _roster.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _roster.AutoGenerateColumns = false; _roster.RowHeadersVisible = false;
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Slot", DataPropertyName = "Slot", Width = 88 });
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "No.", DataPropertyName = "Number", Width = 48 });
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Player", DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 80 });
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Age", DataPropertyName = "Age", Width = 48 });
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OVR", DataPropertyName = "Overall", Width = 52 });
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Position", DataPropertyName = "Position", Width = 66 });
        _roster.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Loan", DataPropertyName = "Loan", Width = 125 });
    }

    private void LoadTeams()
    {
        _teams = FifaEnvironment.Teams.Cast<Team>().Where(team => team != null)
            .OrderBy(team => team.ToString(), StringComparer.CurrentCultureIgnoreCase).ThenBy(team => team.Id).ToArray();
        _team.DataSource = _teams.ToArray();
        _targetTeam.DataSource = _teams.ToArray();
        if (_targetTeam.Items.Count > 1) _targetTeam.SelectedIndex = 1;
        RefreshAll();
    }

    private Team CurrentTeam() => _team.SelectedItem as Team;
    private Team TargetTeam() => _targetTeam.SelectedItem as Team;

    private void RefreshAll()
    {
        var team = CurrentTeam();
        if (team == null) return;
        var rows = team.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null)
            .OrderBy(link => SlotOrder(link.position)).ThenBy(link => link.position).ThenByDescending(link => link.Player.overallrating)
            .Select(link => new RosterRow(link)).ToArray();
        _roster.DataSource = rows;
        var xi = rows.Count(row => row.Link.position < 28);
        var subs = rows.Count(row => row.Link.position == 28);
        var reserves = rows.Count(row => row.Link.position >= 29);
        var youth = rows.Count(row => row.Age <= 21);
        var loansIn = rows.Count(row => row.Link.Player.IsLoaned && row.Link.Player.TeamLoanedFrom != team);
        var loansOut = FifaEnvironment.Players.Cast<Player>().Count(player => player.IsLoaned && player.TeamLoanedFrom == team);
        _summary.Text = team + " [" + team.Id + "] — " + rows.Length + " players | XI " + xi + " | Subs " + subs + " | Reserves " + reserves + " | U21 " + youth + " | Loans in " + loansIn + " / out " + loansOut;
        RefreshAvailable();
    }

    private void RefreshAvailable()
    {
        var team = CurrentTeam(); if (team == null) return;
        var query = (_search.Text ?? string.Empty).Trim();
        var linked = new HashSet<int>(team.Roster.Cast<TeamPlayer>().Where(row => row?.Player != null).Select(row => row.Player.Id));
        var choices = FifaEnvironment.Players.Cast<Player>().Where(player => !linked.Contains(player.Id))
            .Where(player => query.Length == 0 || player.Id.ToString(CultureInfo.InvariantCulture).Contains(query) ||
                player.ToString().IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            .OrderBy(player => player.ToString(), StringComparer.CurrentCultureIgnoreCase).ThenBy(player => player.Id)
            .Take(5000).Select(player => new PlayerChoice(player)).ToArray();
        _available.DataSource = choices;
    }

    private Player[] SelectedAvailable() => _available.SelectedItems.Cast<PlayerChoice>().Select(choice => choice.Player).Distinct().ToArray();
    private TeamPlayer[] SelectedRoster() => _roster.SelectedRows.Cast<DataGridViewRow>()
        .Select(row => row.DataBoundItem as RosterRow).Where(row => row != null).Select(row => row.Link).Distinct().ToArray();

    private void AddSelected()
    {
        var team = CurrentTeam(); var players = SelectedAvailable();
        if (team == null || players.Length == 0) return;
        if (MessageBox.Show(this, "Add/transfer " + players.Length + " player(s) to " + team + "? Existing conflicting club or national-team links will be repaired.",
            "Roster transfer preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var player in players) { player.RemoveCurrentConflictingTeam(team); if (!player.IsPlayingFor(team)) team.AddTeamPlayer(player); }
        RepairTeam(team); RefreshAll();
    }

    private void TransferSelected()
    {
        var source = CurrentTeam(); var target = TargetTeam(); var links = SelectedRoster();
        if (source == null || target == null || source == target || links.Length == 0) return;
        TransferPlayers(source, target, links.Select(link => link.Player).Distinct().ToArray(), "Transfer selected players");
    }

    private void TransferAll()
    {
        var source = CurrentTeam(); var target = TargetTeam();
        if (source == null || target == null || source == target) return;
        var players = source.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null).Select(link => link.Player).Distinct().ToArray();
        if (players.Length == 0) return;
        TransferPlayers(source, target, players, "Transfer ALL players");
    }

    private void TransferPlayers(Team source, Team target, Player[] players, string operation)
    {
        if (source.IsNationalTeam() != target.IsNationalTeam())
        {
            MessageBox.Show(this, "Club transfers and national-team call-ups are separate operations. Select two clubs or two national teams.", "Transfer validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        var loanRows = players.Select(player => new { Player = player, Row = FindLoanRow(player.Id) }).Where(item => item.Row >= 0).ToArray();
        if (loanRows.Length > 0 && Fc26SnapshotLoader.PendingDetailCount > 0)
        {
            MessageBox.Show(this, "Save or reopen the current staged advanced edits before breaking existing loan records.", "Transfer validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        var message = operation + ": " + players.Length + " player(s)\r\n" + source + " → " + target +
            "\r\nJoin date: " + _joinDate.Value.ToShortDateString() + "\r\nContract through: " + _contractYear.Value +
            (loanRows.Length > 0 ? "\r\nExisting loans to terminate: " + loanRows.Length : string.Empty) +
            "\r\n\r\nNational-team links are preserved. Formation, shirts, captain and set pieces will be repaired.";
        if (MessageBox.Show(this, message, "Transfer preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var item in loanRows) Fc26SnapshotLoader.DeleteDetailRow("playerloans", item.Row);
        foreach (var player in players)
        {
            player.IsLoaned = false; player.TeamLoanedFrom = null; player.loandateend = DateTime.MinValue;
            player.joindate = _joinDate.Value.Date; player.contractvaliduntil = (int)_contractYear.Value;
            player.PreviousTeam = source;
            player.RemoveCurrentConflictingTeam(target);
            if (!player.IsPlayingFor(target)) target.AddTeamPlayer(player);
        }
        RepairTeam(source); RepairTeam(target); Fc26ActivityLog.Add("Transfer", operation + ": " + players.Length + " player(s), " + source.Id + " → " + target.Id); RefreshAll();
    }

    private void StartLoan()
    {
        var source = CurrentTeam(); var target = TargetTeam(); var players = SelectedRoster().Select(link => link.Player).Distinct().ToArray();
        if (source == null || target == null || source == target || players.Length == 0) return;
        if (!source.IsClub() || !target.IsClub()) { MessageBox.Show(this, "Loans can only be created between club teams."); return; }
        if (_loanEnd.Value.Date <= _joinDate.Value.Date) { MessageBox.Show(this, "Loan end date must be after the joining date."); return; }
        if (players.Any(player => FindLoanRow(player.Id) >= 0)) { MessageBox.Show(this, "Terminate an existing loan before starting a new one for the same player."); return; }
        var loanTable = Fc26SnapshotLoader.DetailTable("playerloans");
        if (loanTable == null || loanTable.Rows.Count == 0) { MessageBox.Show(this, "The FC26 playerloans table is unavailable."); return; }
        if (MessageBox.Show(this, "Loan " + players.Length + " player(s) from " + source + " to " + target + " until " + _loanEnd.Value.ToShortDateString() +
            (_loanToBuy.Checked ? " with an option to buy?" : "?") + "\r\n\r\nRoster, shirt, formation and set-piece links will be repaired.", "Loan preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var player in players)
        {
            var row = Fc26SnapshotLoader.DuplicateDetailRow("playerloans", 0);
            Fc26SnapshotLoader.StageDetailValue("playerloans", row, "playerid", player.Id.ToString(CultureInfo.InvariantCulture));
            Fc26SnapshotLoader.StageDetailValue("playerloans", row, "teamidloanedfrom", source.Id.ToString(CultureInfo.InvariantCulture));
            Fc26SnapshotLoader.StageDetailValue("playerloans", row, "loandateend", FifaUtil.ConvertFromDate(_loanEnd.Value.Date).ToString(CultureInfo.InvariantCulture));
            if (loanTable.Column("isloantobuy") >= 0)
                Fc26SnapshotLoader.StageDetailValue("playerloans", row, "isloantobuy", _loanToBuy.Checked ? "1" : "0");
            player.IsLoaned = true; player.TeamLoanedFrom = source; player.loandateend = _loanEnd.Value.Date;
            player.joindate = _joinDate.Value.Date; player.contractvaliduntil = Math.Max((int)_contractYear.Value, _loanEnd.Value.Year + 1);
            player.RemoveCurrentConflictingTeam(target); if (!player.IsPlayingFor(target)) target.AddTeamPlayer(player);
        }
        RepairTeam(source); RepairTeam(target); Fc26ActivityLog.Add("Loan", players.Length + " player(s), " + source.Id + " → " + target.Id); RefreshAll();
    }

    private void TerminateLoan()
    {
        var current = CurrentTeam(); var players = SelectedRoster().Select(link => link.Player).Where(player => player.IsLoaned).Distinct().ToArray();
        if (current == null || players.Length == 0) { MessageBox.Show(this, "Select one or more loaned roster players."); return; }
        var rows = players.Select(player => new { Player = player, Row = FindLoanRow(player.Id) }).Where(item => item.Row >= 0).ToArray();
        if (rows.Length == 0) { MessageBox.Show(this, "No matching playerloans records were found."); return; }
        if (Fc26SnapshotLoader.PendingDetailCount > 0) { MessageBox.Show(this, "Save or reopen the current staged advanced edits before deleting loan records."); return; }
        if (MessageBox.Show(this, "Terminate " + rows.Length + " loan(s) and return the players to their parent clubs?", "Terminate loan preview", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var item in rows)
        {
            var parent = item.Player.TeamLoanedFrom;
            Fc26SnapshotLoader.DeleteDetailRow("playerloans", item.Row);
            if (parent != null) { item.Player.RemoveCurrentConflictingTeam(parent); if (!item.Player.IsPlayingFor(parent)) parent.AddTeamPlayer(item.Player); RepairTeam(parent); }
            item.Player.IsLoaned = false; item.Player.TeamLoanedFrom = null; item.Player.loandateend = DateTime.MinValue;
        }
        RepairTeam(current); Fc26ActivityLog.Add("Loan", "Terminated " + rows.Length + " loan(s)"); RefreshAll();
    }

    private static int FindLoanRow(int playerId)
    {
        var table = Fc26SnapshotLoader.DetailTable("playerloans"); if (table == null) return -1;
        var column = table.Column("playerid"); if (column < 0) return -1;
        for (var row = 0; row < table.Rows.Count; row++)
            if (!Fc26SnapshotLoader.IsDetailDeleted("playerloans", row) && column < table.Rows[row].Length && Parse(table.Rows[row][column]) == playerId) return row;
        return -1;
    }

    private void RemoveSelected()
    {
        var team = CurrentTeam(); var links = SelectedRoster(); if (team == null || links.Length == 0) return;
        if (MessageBox.Show(this, "Remove " + links.Length + " selected player link(s) from " + team + "?", "Roster preview",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var link in links) team.RemoveTeamPlayer(link);
        RepairTeam(team); RefreshAll();
    }

    private void ReleaseSelected()
    {
        var links = SelectedRoster(); if (links.Length == 0) return;
        var freeAgents = FifaEnvironment.Teams.SearchId(111592) as Team;
        if (freeAgents == null) { MessageBox.Show(this, "FC26 Free Agents team (111592) is not loaded."); return; }
        if (MessageBox.Show(this, "Release " + links.Length + " player(s) from all clubs and add them to Free Agents?", "Release preview",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var player in links.Select(link => link.Player).Distinct())
        {
            foreach (var club in _teams.Where(team => team.IsClub() && player.IsPlayingFor(team)).ToArray()) club.RemoveTeamPlayer(player);
            if (!player.IsPlayingFor(freeAgents)) freeAgents.AddTeamPlayer(player);
        }
        RefreshAll();
    }

    private void NationalCallUp()
    {
        var national = TargetTeam();
        var selectedRosterPlayers = SelectedRoster().Select(link => link.Player).Distinct().ToArray();
        var players = selectedRosterPlayers.Length > 0 ? selectedRosterPlayers : SelectedAvailable();
        if (national == null || !national.IsNationalTeam()) { MessageBox.Show(this, "Select a national team as Target team."); return; }
        if (players.Length == 0) return;
        var invalid = players.Where(player => national.Country == null || player.Country == null || player.Country.Id != national.Country.Id).ToArray();
        if (invalid.Length > 0)
        {
            MessageBox.Show(this, invalid.Length + " selected player(s) do not match " + national.Country + " nationality. No changes were made.",
                "Nationality validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        foreach (var player in players) { player.RemoveCurrentConflictingTeam(national); if (!player.IsPlayingFor(national)) national.AddTeamPlayer(player); }
        RepairTeam(national); MessageBox.Show(this, players.Length + " national-team call-up(s) staged. Use File > Save to commit.");
    }

    private void RemoveNationalCallUp()
    {
        var national = CurrentTeam();
        var links = SelectedRoster();
        if (national == null || !national.IsNationalTeam())
        { MessageBox.Show(this, "Select a national team as Current team."); return; }
        if (links.Length == 0) return;
        if (MessageBox.Show(this, "Remove " + links.Length + " selected player(s) from " + national +
            "? Club links are preserved.", "National-team removal preview",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var link in links) national.RemoveTeamPlayer(link);
        RepairTeam(national);
        Fc26ActivityLog.Add("National squad", "Removed " + links.Length + " call-up(s) from " + national.Id);
        RefreshAll();
    }

    private void ValidateNationalSquad()
    {
        var national = CurrentTeam();
        if (national == null || !national.IsNationalTeam())
        { MessageBox.Show(this, "Select a national team as Current team."); return; }
        var links = national.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null).ToArray();
        var duplicateIds = links.GroupBy(link => link.Player.Id).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
        var nationalityMismatch = links.Where(link => national.Country == null || link.Player.Country == null ||
            link.Player.Country.Id != national.Country.Id).Select(link => link.Player.Id).ToArray();
        var invalidSlots = links.Where(link => link.position < 0 || link.position > 29).Select(link => link.Player.Id).ToArray();
        var goalkeepers = links.Count(link => link.Player.preferredposition1 == 0);
        var issues = new List<string>();
        if (links.Length < 18 || links.Length > 26) issues.Add("Squad size is " + links.Length + " (recommended 18–26).");
        if (goalkeepers < 2) issues.Add("Only " + goalkeepers + " goalkeeper(s) found; at least two are recommended.");
        if (duplicateIds.Length > 0) issues.Add("Duplicate player IDs: " + string.Join(", ", duplicateIds));
        if (nationalityMismatch.Length > 0) issues.Add("Nationality mismatch IDs: " + string.Join(", ", nationalityMismatch));
        if (invalidSlots.Length > 0) issues.Add("Invalid formation slot IDs: " + string.Join(", ", invalidSlots));
        MessageBox.Show(this, issues.Count == 0 ? "National squad passes size, goalkeeper, nationality, duplicate and slot checks." :
            string.Join("\r\n", issues), "National squad validation", MessageBoxButtons.OK,
            issues.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void SyncNationalityLinks()
    {
        var nationalTeams = _teams.Where(team => team.IsNationalTeam() && team.Country != null).ToArray();
        var moves = (from team in nationalTeams
                     from link in team.Roster.Cast<TeamPlayer>()
                     where link?.Player?.Country != null && link.Player.Country.Id != team.Country.Id
                     let target = nationalTeams.FirstOrDefault(candidate => candidate.Country.Id == link.Player.Country.Id)
                     select new { Source = team, Link = link, Target = target }).ToArray();
        if (moves.Length == 0)
        { MessageBox.Show(this, "All loaded national-team links match player nationality."); return; }
        var resolvable = moves.Count(move => move.Target != null);
        if (MessageBox.Show(this, "Found " + moves.Length + " nationality mismatch(es).\r\n" + resolvable +
            " can be moved to the matching loaded national team; the remainder will be removed from the incorrect squad. Continue?",
            "Sync nationality links preview", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var move in moves)
        {
            move.Source.RemoveTeamPlayer(move.Link);
            if (move.Target != null && !move.Link.Player.IsPlayingFor(move.Target)) move.Target.AddTeamPlayer(move.Link.Player);
        }
        foreach (var team in moves.Select(move => move.Source).Concat(moves.Where(move => move.Target != null).Select(move => move.Target)).Distinct())
            RepairTeam(team);
        Fc26ActivityLog.Add("National squad", "Synchronized " + moves.Length + " nationality link(s)");
        RefreshAll();
    }

    private void ReplaceInjuredCallUps()
    {
        var national = CurrentTeam();
        if (national == null || !national.IsNationalTeam() || national.Country == null)
        { MessageBox.Show(this, "Select a national team as Current team."); return; }
        var injured = national.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null && link.injury > 0).ToArray();
        if (injured.Length == 0) { MessageBox.Show(this, "No injured call-ups were found in this national squad."); return; }
        var currentIds = new HashSet<int>(national.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null).Select(link => link.Player.Id));
        var replacements = FifaEnvironment.Players.Cast<Player>()
            .Where(player => player?.Country != null && player.Country.Id == national.Country.Id && !currentIds.Contains(player.Id))
            .OrderByDescending(player => player.overallrating).ThenByDescending(player => player.potential).Take(injured.Length).ToArray();
        if (replacements.Length < injured.Length)
        { MessageBox.Show(this, "Only " + replacements.Length + " eligible replacement(s) were found for " + injured.Length + " injured player(s)."); return; }
        var preview = string.Join("\r\n", injured.Select((link, index) => link.Player + " → " + replacements[index]));
        if (MessageBox.Show(this, "Replace " + injured.Length + " injured call-up(s)?\r\n\r\n" + preview,
            "Injury replacement preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        for (var index = 0; index < injured.Length; index++)
        {
            national.RemoveTeamPlayer(injured[index]);
            if (!replacements[index].IsPlayingFor(national)) national.AddTeamPlayer(replacements[index]);
        }
        RepairTeam(national);
        Fc26ActivityLog.Add("National squad", "Replaced " + injured.Length + " injured call-up(s) for " + national.Id);
        RefreshAll();
    }

    private void SyncYouth()
    {
        var source = CurrentTeam(); var target = TargetTeam(); if (source == null || target == null || source == target) return;
        var youth = source.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null && Age(link.Player) <= 21).Select(link => link.Player).Distinct().ToArray();
        if (youth.Length == 0) { MessageBox.Show(this, "The current team has no U21 player links."); return; }
        if (MessageBox.Show(this, "Move " + youth.Length + " U21 player(s) from " + source + " to " + target + " and repair both rosters?",
            "Youth squad sync preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var player in youth) { player.RemoveCurrentConflictingTeam(target); if (!player.IsPlayingFor(target)) target.AddTeamPlayer(player); }
        RepairTeam(source); RepairTeam(target); RefreshAll();
    }

    private void ExportYouth()
    {
        var team = CurrentTeam(); if (team == null) return;
        using var dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "youth_u21_" + team.Id + ".csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var youth = team.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null && Age(link.Player) <= 21).ToArray();
        var lines = new List<string> { "playerid,name,age,overall,potential,jerseynumber,position" };
        lines.AddRange(youth.Select(link => link.Player.Id + ",\"" + (link.Player.ToString() ?? string.Empty).Replace("\"", "\"\"") + "\"," +
            Age(link.Player) + "," + link.Player.overallrating + "," + link.Player.potential + "," + link.jerseynumber + "," + link.position));
        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
    }

    private void ImportYouth()
    {
        var team = CurrentTeam(); if (team == null) return;
        using var dialog = new OpenFileDialog { Filter = "Youth CSV (*.csv)|*.csv|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var resolved = File.ReadAllLines(dialog.FileName).Skip(1).Select(line => line.Split(','))
            .Where(parts => parts.Length >= 7 && int.TryParse(parts[0], out _))
            .Select(parts => new { Player = FifaEnvironment.Players.SearchId(Parse(parts[0])) as Player,
                Number = Parse(parts[parts.Length - 2]), Position = Parse(parts[parts.Length - 1]) })
            .Where(item => item.Player != null && Age(item.Player) <= 21).GroupBy(item => item.Player.Id).Select(group => group.First()).ToArray();
        var existing = new HashSet<int>(team.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null).Select(link => link.Player.Id));
        var additions = resolved.Where(item => !existing.Contains(item.Player.Id)).ToArray();
        if (additions.Length == 0) { MessageBox.Show(this, "No new valid U21 player IDs were found."); return; }
        if (MessageBox.Show(this, "Merge " + additions.Length + " U21 player(s) into " + team + "? Existing senior and youth links are retained.",
            "Youth import preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var item in additions)
        {
            item.Player.RemoveCurrentConflictingTeam(team);
            var link = team.AddTeamPlayer(item.Player, item.Number); link.position = item.Position;
        }
        RepairTeam(team); Fc26ActivityLog.Add("Youth squad", "Merged " + additions.Length + " U21 player(s) into " + team.Id); RefreshAll();
    }

    private void AutoBestXi()
    {
        var team = CurrentTeam(); if (team == null) return;
        if (team.Formation == null) { MessageBox.Show(this, "Assign a formation to this team first."); return; }
        team.AssignRoles(team.Formation); team.AssignVacantRolesToSubstitute(); team.AssignBench(); team.AssignVacantSpecialPlayers();
        RefreshAll();
    }

    private void RepairRoster()
    {
        var team = CurrentTeam(); if (team == null) return;
        var repaired = RepairTeam(team);
        MessageBox.Show(this, repaired + " duplicate, shirt-number, formation or set-piece repair(s) staged. Use File > Save to validate and commit.",
            "Roster repair", MessageBoxButtons.OK, MessageBoxIcon.Information); RefreshAll();
    }

    /// <summary>
    /// Applies the same conservative squad repair used by the Roster Tools UI.
    /// The method only changes the loaded CM26 object graph; the normal Save
    /// pipeline still performs validation, backup and the direct FC26 commit.
    /// </summary>
    internal static int RepairTeam(Team team)
    {
        var repaired = 0; var ids = new HashSet<int>();
        foreach (var link in team.Roster.Cast<TeamPlayer>().ToArray())
        {
            if (link == null || link.Player == null) { team.Roster.Remove(link); repaired++; continue; }
            if (!ids.Add(link.Player.Id)) { team.RemoveTeamPlayer(link); repaired++; }
        }
        var numbers = new HashSet<int>();
        foreach (var link in team.Roster.Cast<TeamPlayer>())
        {
            if (link.jerseynumber >= 1 && link.jerseynumber <= 99 && numbers.Add(link.jerseynumber)) continue;
            for (var number = 1; number <= 99; number++) if (numbers.Add(number)) { link.jerseynumber = number; repaired++; break; }
        }
        if (team.Formation != null) { team.AssignVacantRolesToSubstitute(); team.AssignBench(); team.AssignVacantSpecialPlayers(); }
        return repaired;
    }

    private void ExportRoster()
    {
        var team = CurrentTeam(); if (team == null) return;
        using var dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "roster_" + team.Id + ".csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var lines = new List<string> { "playerid,name,jerseynumber,position" };
        lines.AddRange(team.Roster.Cast<TeamPlayer>().Where(link => link?.Player != null).Select(link =>
            link.Player.Id + ",\"" + (link.Player.ToString() ?? string.Empty).Replace("\"", "\"\"") + "\"," + link.jerseynumber + "," + link.position));
        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
    }

    private void ImportRoster()
    {
        var team = CurrentTeam(); if (team == null) return;
        using var dialog = new OpenFileDialog { Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var rows = File.ReadAllLines(dialog.FileName).Skip(1).Select(line => line.Split(','))
            .Where(parts => parts.Length >= 4 && int.TryParse(parts[0], out _)).ToArray();
        var resolved = rows.Select(parts => new { Player = FifaEnvironment.Players.SearchId(int.Parse(parts[0], CultureInfo.InvariantCulture)) as Player,
            Number = Parse(parts[parts.Length - 2]), Position = Parse(parts[parts.Length - 1]) }).Where(row => row.Player != null).ToArray();
        if (resolved.Length == 0) { MessageBox.Show(this, "No valid FC26 player IDs were found in this roster file."); return; }
        if (MessageBox.Show(this, "Replace the " + team + " roster with " + resolved.Length + " resolved player(s)? Affected links will be repaired.",
            "Roster import preview", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var link in team.Roster.Cast<TeamPlayer>().ToArray()) team.RemoveTeamPlayer(link);
        foreach (var row in resolved) { row.Player.RemoveCurrentConflictingTeam(team); var link = team.AddTeamPlayer(row.Player, row.Number); link.position = row.Position; }
        RepairTeam(team); RefreshAll();
    }

    private static int Parse(string value) => int.TryParse(value.Trim().Trim('"'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;
    private static int SlotOrder(int position) => position < 28 ? 0 : position == 28 ? 1 : 2;
    private static int Age(Player player) { var today = DateTime.Today; var age = today.Year - player.birthdate.Year; if (player.birthdate.Date > today.AddYears(-age)) age--; return Math.Max(0, age); }
    private static Label Label(string text) => new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left };
    private static Button Button(string text, EventHandler action) { var button = new Button { Text = text, AutoSize = true, Height = 29 }; button.Click += action; return button; }

    private sealed class PlayerChoice
    {
        internal PlayerChoice(Player player) { Player = player; Display = player + " [" + player.Id + "] — " + (player.GetClub()?.ToString() ?? "No club"); }
        public Player Player { get; } public string Display { get; }
    }

    private sealed class RosterRow
    {
        internal RosterRow(TeamPlayer link) { Link = link; Age = Fc26RosterToolsForm.Age(link.Player); }
        public TeamPlayer Link { get; }
        public string Slot => Link.position < 28 ? "Starting XI" : Link.position == 28 ? "Substitute" : "Reserve";
        public int Number => Link.jerseynumber; public string Name => Link.Player.ToString(); public int Id => Link.Player.Id;
        public int Age { get; } public int Overall => Link.Player.overallrating; public int Position => Link.position;
        public string Loan => Link.Player.IsLoaned ? "From " + (Link.Player.TeamLoanedFrom?.ToString() ?? "?") : string.Empty;
    }
}
