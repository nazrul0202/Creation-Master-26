using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

internal enum Fc26CheckState
{
    Pass,
    Warning,
    Error
}

internal sealed class Fc26SaveCheck
{
    internal Fc26SaveCheck(string name, Fc26CheckState state, string details, string section)
    { Name = name; State = state; Details = details; Section = section; }
    internal string Name { get; }
    internal Fc26CheckState State { get; }
    internal string Details { get; }
    internal string Section { get; }
    internal bool IsError => State == Fc26CheckState.Error;
    internal string StateText => State == Fc26CheckState.Pass ? "PASS" : State == Fc26CheckState.Warning ? "CHECK" : "FIX";
}

internal sealed class Fc26SavePreflightResult
{
    internal Fc26SavePreflightResult(IReadOnlyList<Fc26SaveCheck> checks) { Checks = checks; }
    internal IReadOnlyList<Fc26SaveCheck> Checks { get; }
    internal bool CanSave => Checks.All(value => !value.IsError);
}

/// <summary>Friendly save gate for the classic FC26 shell.  It checks the
/// relationships that the old editor used to leave half-created, before the
/// x64 transactional writer is invoked.</summary>
internal static class Fc26SavePreflight
{
    internal static Fc26SavePreflightResult Run(IEnumerable<int> pendingLeagueIds, IEnumerable<int> pendingTeamIds)
    {
        var checks = new List<Fc26SaveCheck>();
        if (!Fc26SnapshotLoader.IsLoaded)
        {
            checks.Add(new Fc26SaveCheck("FC26 database", Fc26CheckState.Error, "Open FC26 or an extracted FC26 database first.", "country"));
            return new Fc26SavePreflightResult(checks);
        }

        var leagueIds = new HashSet<int>((pendingLeagueIds ?? Array.Empty<int>()).Where(value => value > 0));
        var teamIds = new HashSet<int>((pendingTeamIds ?? Array.Empty<int>()).Where(value => value > 0));
        var leagues = (FifaEnvironment.Leagues?.Cast<League>() ?? Enumerable.Empty<League>()).Where(value => value != null).ToArray();
        var teams = (FifaEnvironment.Teams?.Cast<Team>() ?? Enumerable.Empty<Team>()).Where(value => value != null).ToArray();
        var players = (FifaEnvironment.Players?.Cast<Player>() ?? Enumerable.Empty<Player>()).Where(value => value != null).ToArray();
        var pendingLeagues = leagues.Where(value => leagueIds.Contains(value.Id)).ToArray();
        var pendingTeams = teams.Where(value => teamIds.Contains(value.Id) || pendingLeagues.Any(league => league.PlayingTeams.SearchId(value.Id) != null)).ToArray();

        var schemaCompatible = Fc26SnapshotLoader.IsSchemaCompatible(out var schemaReport);
        checks.Add(new Fc26SaveCheck("FC26 schema compatibility", schemaCompatible ? Fc26CheckState.Pass : Fc26CheckState.Error,
            schemaReport, "country"));

        var runningGame = Fc26RuntimeSafety.RunningGameProcesses();
        checks.Add(new Fc26SaveCheck("FC26 closed", runningGame.Length == 0 ? Fc26CheckState.Pass : Fc26CheckState.Error,
            runningGame.Length == 0 ? "No FC26 process is running." : "Close before Save: " + string.Join(", ", runningGame), "competition"));

        var recoveryFolders = Fc26RuntimeSafety.RecoveryRequiredFolders();
        checks.Add(new Fc26SaveCheck("Transaction recovery", recoveryFolders.Length == 0 ? Fc26CheckState.Pass : Fc26CheckState.Error,
            recoveryFolders.Length == 0 ? "No incomplete direct-save transaction needs recovery." :
            "A previous rollback was incomplete. Do not start FC26; open Recovery Folder from Public Readiness.", "competition"));

        var snapshotReadable = Fc26RuntimeSafety.SnapshotIsReadable(out var snapshotDetail);
        checks.Add(new Fc26SaveCheck("Loaded snapshot", snapshotReadable ? Fc26CheckState.Pass : Fc26CheckState.Error,
            snapshotDetail, "country"));

        var freeBytes = Fc26RuntimeSafety.AvailableWorkspaceBytes();
        var freeState = freeBytes < 0 ? Fc26CheckState.Warning : freeBytes < 1024L * 1024 * 1024 ? Fc26CheckState.Error : Fc26CheckState.Pass;
        checks.Add(new Fc26SaveCheck("Free disk space", freeState,
            freeBytes < 0 ? "Available workspace disk space could not be determined." :
            (freeBytes / (1024d * 1024 * 1024)).ToString("N1") + " GB available; at least 1 GB is required before Save.", "competition"));

        checks.Add(new Fc26SaveCheck("League country", pendingLeagues.All(value => value.Country != null)
            ? Fc26CheckState.Pass : Fc26CheckState.Error,
            pendingLeagues.Length == 0 ? "No new league is waiting for Compdata." :
            (pendingLeagues.All(value => value.Country != null) ? "Country is assigned to every new league." : "Choose a country for every new league."), "league"));

        var tooSmall = pendingLeagues.Where(value => value.PlayingTeams.Cast<Team>().Count(team => team != null && team.Id > 0) < 2).ToArray();
        checks.Add(new Fc26SaveCheck("Minimum teams", tooSmall.Length == 0 ? Fc26CheckState.Pass : Fc26CheckState.Error,
            tooSmall.Length == 0 ? (pendingLeagues.Length == 0 ? "No new league needs a team count check." : "Every new league has at least two teams.") :
            string.Join(", ", tooSmall.Select(value => value.ToString())) + " needs at least two teams.", "league"));

        checks.Add(CheckDuplicateIds("League IDs", leagues.Select(value => value.Id), "league"));
        checks.Add(CheckDuplicateIds("Team IDs", teams.Select(value => value.Id), "team"));
        checks.Add(CheckDuplicateIds("Player IDs", players.Select(value => value.Id), "player"));
        checks.Add(new Fc26SaveCheck("Player-name IDs", Fc26CheckState.Pass,
            Fc26SnapshotLoader.DescribePlayerNameAvailability(), "player"));

        var unlinked = pendingTeams.Where(team => team.League == null || team.League.PlayingTeams.SearchId(team.Id) == null).ToArray();
        var linkTable = Fc26SnapshotLoader.DetailTable("leagueteamlinks");
        var linkFailures = pendingTeams.Where(team => team.League != null &&
            (linkTable == null || !HasLink(linkTable, team.Id, team.League.Id))).ToArray();
        if (unlinked.Length > 0 || linkFailures.Length > 0)
            checks.Add(new Fc26SaveCheck("League-team links", Fc26CheckState.Error,
                "Fix links for: " + string.Join(", ", unlinked.Concat(linkFailures).Distinct().Select(value => value.TeamNameFull)), "team"));
        else
            checks.Add(new Fc26SaveCheck("League-team links", Fc26CheckState.Pass,
                pendingTeams.Length == 0 ? "No newly-created team relationship is pending." : "Every new team is linked to its league.", "team"));

        var missingRoster = pendingTeams.Where(team => team.Roster.Cast<TeamPlayer>().Count(value => value?.Player != null) < 11).ToArray();
        checks.Add(new Fc26SaveCheck("Roster", missingRoster.Length == 0 ? Fc26CheckState.Pass : Fc26CheckState.Error,
            missingRoster.Length == 0 ? "Every new team has a valid starting squad." :
            string.Join(", ", missingRoster.Select(value => value.TeamNameFull)) + " needs at least 11 players.", "player"));

        var missingAssets = pendingTeams.Where(team => team.Stadium == null || !HasKit(team)).ToArray();
        checks.Add(new Fc26SaveCheck("Kit / stadium references", missingAssets.Length == 0 ? Fc26CheckState.Pass : Fc26CheckState.Warning,
            missingAssets.Length == 0 ? "Every new team has a stadium and at least one kit reference." :
            string.Join(", ", missingAssets.Select(value => value.TeamNameFull)) + " has a missing kit or stadium; the game fallback will be used.", "kit"));

        checks.Add(new Fc26SaveCheck("Compdata", pendingLeagues.Length == 0 ? Fc26CheckState.Pass : Fc26CheckState.Warning,
            pendingLeagues.Length == 0 ? "No new league Compdata is waiting." : "Compdata, league-team assignment and schedule will be generated and validated during Save.", "competition"));
        checks.Add(new Fc26SaveCheck("Transactional backup", Fc26CheckState.Pass,
            "The FC26 save engine creates a timestamped backup before commit.", "competition"));
        return new Fc26SavePreflightResult(checks);
    }

    private static Fc26SaveCheck CheckDuplicateIds(string name, IEnumerable<int> ids, string section)
    {
        var duplicates = (ids ?? Enumerable.Empty<int>()).GroupBy(value => value).Where(group => group.Key > 0 && group.Count() > 1).Select(group => group.Key).ToArray();
        return new Fc26SaveCheck(name, duplicates.Length == 0 ? Fc26CheckState.Pass : Fc26CheckState.Error,
            duplicates.Length == 0 ? "No duplicate IDs found." : "Duplicate ID(s): " + string.Join(", ", duplicates), section);
    }

    private static bool HasLink(SnapshotDetailTable table, int teamId, int leagueId)
    {
        var teamColumn = table.Column("teamid"); var leagueColumn = table.Column("leagueid");
        if (teamColumn < 0 || leagueColumn < 0) return false;
        return table.Rows.Any(row => Parse(row, teamColumn) == teamId && Parse(row, leagueColumn) == leagueId);
    }

    private static int Parse(string[] row, int column) => column >= 0 && column < row.Length && int.TryParse(row[column], out var value) ? value : int.MinValue;
    private static bool HasKit(Team team) => team?.m_KitList?.Cast<Kit>().Any(value => value != null && value.Id > 0 && value.teamid == team.Id) == true;
}

internal sealed class Fc26SavePreflightDialog : Form
{
    private readonly ListView _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
    private readonly Button _fix = new Button { Text = "Fix Selected", AutoSize = true };
    private readonly Button _continue = new Button { Text = "Continue Save", AutoSize = true };
    private readonly MainForm _owner;

    internal Fc26SavePreflightDialog(MainForm owner, Fc26SavePreflightResult result)
    {
        _owner = owner; Text = result.CanSave ? "Save Preflight" : "Save Preflight — Fix Required";
        StartPosition = FormStartPosition.CenterParent; Size = new Size(780, 440); MinimizeBox = false; MaximizeBox = false;
        _list.Columns.Add("Check", 190); _list.Columns.Add("State", 80); _list.Columns.Add("Details", 470);
        foreach (var check in result.Checks)
        {
            var item = new ListViewItem(check.Name); item.SubItems.Add(check.StateText); item.SubItems.Add(check.Details); item.Tag = check;
            item.ForeColor = check.State == Fc26CheckState.Error ? Color.DarkRed : check.State == Fc26CheckState.Warning ? Color.DarkGoldenrod : Color.DarkGreen;
            _list.Items.Add(item);
        }
        _fix.Click += (_, _) =>
        {
            if (_list.SelectedItems.Count == 0 || !(_list.SelectedItems[0].Tag is Fc26SaveCheck check)) return;
            _owner.ShowFc26Section(check.Section); DialogResult = DialogResult.Cancel;
        };
        _continue.Enabled = result.CanSave; _continue.DialogResult = result.CanSave ? DialogResult.OK : DialogResult.None;
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 43, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        buttons.Controls.Add(cancel); buttons.Controls.Add(_continue); buttons.Controls.Add(_fix);
        Controls.Add(_list); Controls.Add(buttons);
    }
}

internal sealed class Fc26SaveProofDialog : Form
{
    internal Fc26SaveProofDialog(string title, string report)
    {
        Text = title; StartPosition = FormStartPosition.CenterParent; Size = new Size(760, 480);
        var text = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9f), Text = report };
        var close = new Button { Text = "Close", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 32 };
        Controls.Add(text); Controls.Add(close); AcceptButton = close;
    }
}
