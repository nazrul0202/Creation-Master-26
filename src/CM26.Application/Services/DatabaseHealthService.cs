using System.Globalization;
using CM26.Application.Models;

namespace CM26.Application.Services;

public enum HealthSeverity { Info, Warning, Error }

public sealed record DatabaseHealthIssue(
    string Code, HealthSeverity Severity, string Table, int? Row, string Message, bool Repairable);

public sealed record DatabaseHealthReport(IReadOnlyList<DatabaseHealthIssue> Issues)
{
    public int Errors => Issues.Count(issue => issue.Severity == HealthSeverity.Error);
    public int Warnings => Issues.Count(issue => issue.Severity == HealthSeverity.Warning);
    public int Repairable => Issues.Count(issue => issue.Repairable);
    public bool IsHealthy => Errors == 0 && Warnings == 0;

    public string ToText(int limit = 500)
    {
        var header = IsHealthy
            ? "Database Health Centre: no integrity or roster problems found."
            : $"Database Health Centre: {Errors} error(s), {Warnings} warning(s), {Repairable} safely repairable issue(s).";
        if (Issues.Count == 0) return header;
        var lines = Issues.Take(Math.Max(1, limit)).Select(issue =>
            $"[{issue.Severity}] {issue.Code} · {issue.Table}" +
            (issue.Row.HasValue ? $"[{issue.Row.Value}]" : string.Empty) + " · " + issue.Message +
            (issue.Repairable ? " · repairable" : string.Empty));
        return header + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, lines) +
               (Issues.Count > limit ? $"{Environment.NewLine}… and {Issues.Count - limit} more." : string.Empty);
    }
}

/// <summary>Cross-table FC26 health checks. Analysis is read-only; repairs remain staged until Save.</summary>
public static class DatabaseHealthService
{
    public static DatabaseHealthReport Analyze(DatabaseSession session)
    {
        if (!session.IsLoaded)
            return new([new("database-not-loaded", HealthSeverity.Error, "database", null,
                "Open FC26 or an extracted database first.", false)]);

        var issues = new List<DatabaseHealthIssue>();
        foreach (var native in session.ValidateIntegrity())
            issues.Add(new("native-integrity", HealthSeverity.Error, "database", null, native, false));

        AnalyzeRoster(session, issues);
        AnalyzeTeamSheets(session, issues);
        AnalyzeLoans(session, issues);
        AnalyzeTeamAssetsAndLeague(session, issues);
        AnalyzeNationalTeams(session, issues);
        AnalyzeManagers(session, issues);
        return new(issues);
    }

    private static void AnalyzeRoster(DatabaseSession session, List<DatabaseHealthIssue> issues)
    {
        var links = session.GetTable("teamplayerlinks");
        var players = session.GetTable("players");
        var teams = session.GetTable("teams");
        if (links == null || players == null || teams == null) return;
        var playerIds = IdSet(session, players, "playerid");
        var teamIds = IdSet(session, teams, "teamid");
        var seenLinks = new HashSet<(int Team, int Player)>();
        var clubByPlayer = new Dictionary<int, int>();
        var freeAgentRows = new List<(int Row, int Player)>();
        var shirts = new HashSet<(int Team, int Shirt)>();
        var linkedPlayers = new HashSet<int>();

        for (var row = 0; row < links.RowCount; row++)
        {
            var player = Int(session, links.Name, row, "playerid");
            var team = Int(session, links.Name, row, "teamid");
            if (!playerIds.Contains(player))
                issues.Add(new("broken-player-link", HealthSeverity.Error, links.Name, row,
                    $"Player {player} does not exist.", true));
            else linkedPlayers.Add(player);
            if (team != -1 && !teamIds.Contains(team))
                issues.Add(new("broken-team-link", HealthSeverity.Error, links.Name, row,
                    $"Team {team} does not exist.", true));
            if (!seenLinks.Add((team, player)))
                issues.Add(new("duplicate-roster-link", HealthSeverity.Warning, links.Name, row,
                    $"Player {player} is linked to team {team} more than once.", true));
            if (team > 0 && player > 0)
            {
                clubByPlayer[player] = team;
                if (Has(links, "jerseynumber"))
                {
                    var shirt = Int(session, links.Name, row, "jerseynumber");
                    if (shirt is < 1 or > 99 || !shirts.Add((team, shirt)))
                        issues.Add(new("invalid-or-duplicate-shirt", HealthSeverity.Warning, links.Name, row,
                            $"Team {team} has an invalid or duplicate shirt number {shirt}.", true));
                }
            }
            if (team == -1 && player > 0) freeAgentRows.Add((row, player));
        }
        foreach (var freeAgent in freeAgentRows.Where(item => clubByPlayer.ContainsKey(item.Player)))
            issues.Add(new("free-agent-with-club", HealthSeverity.Warning, links.Name, freeAgent.Row,
                $"Player {freeAgent.Player} is both a free agent and linked to a club.", true));
        for (var row = 0; row < players.RowCount; row++)
        {
            var player = Int(session, players.Name, row, "playerid");
            if (player > 0 && !linkedPlayers.Contains(player))
                issues.Add(new("unlinked-player", HealthSeverity.Warning, players.Name, row,
                    $"Player {player} has no club or free-agent link.", true));
        }
    }

    private static void AnalyzeTeamSheets(DatabaseSession session, List<DatabaseHealthIssue> issues)
    {
        var sheets = session.GetTable("default_teamsheets");
        var players = session.GetTable("players");
        if (sheets == null || players == null) return;
        var playerIds = IdSet(session, players, "playerid");
        var playerFields = sheets.Columns.Where(column =>
            column.Name.StartsWith("playerid", StringComparison.OrdinalIgnoreCase)).ToArray();
        for (var row = 0; row < sheets.RowCount; row++)
        {
            var seen = new HashSet<int>();
            var count = 0;
            foreach (var field in playerFields)
            {
                var player = Int(session, sheets.Name, row, field.Name);
                if (player <= 0) continue;
                count++;
                if (!playerIds.Contains(player))
                    issues.Add(new("missing-teamsheet-player", HealthSeverity.Error, sheets.Name, row,
                        $"{field.Name} references missing player {player}.", field.IsWritable));
                else if (!seen.Add(player))
                    issues.Add(new("duplicate-lineup-player", HealthSeverity.Warning, sheets.Name, row,
                        $"Player {player} appears more than once.", field.IsWritable));
            }
            if (playerFields.Length >= 11 && count < 11)
                issues.Add(new("empty-starting-xi", HealthSeverity.Warning, sheets.Name, row,
                    $"Only {count} populated lineup slots were found.", false));
        }
    }

    private static void AnalyzeLoans(DatabaseSession session, List<DatabaseHealthIssue> issues)
    {
        var loans = session.GetTable("playerloans");
        var players = session.GetTable("players");
        if (loans == null || players == null || !Has(loans, "playerid")) return;
        var playerIds = IdSet(session, players, "playerid");
        var teams = session.GetTable("teams");
        var teamIds = teams == null ? [] : IdSet(session, teams, "teamid");
        var seenPlayers = new HashSet<int>();
        for (var row = 0; row < loans.RowCount; row++)
        {
            var player = Int(session, loans.Name, row, "playerid");
            if (!playerIds.Contains(player))
                issues.Add(new("broken-loan-player", HealthSeverity.Error, loans.Name, row,
                    $"Loan references missing player {player}.", false));
            else if (!seenPlayers.Add(player))
                issues.Add(new("duplicate-player-loan", HealthSeverity.Error, loans.Name, row,
                    $"Player {player} has more than one active loan row.", false));
            if (Has(loans, "teamidloanedfrom"))
            {
                var sourceTeam = Int(session, loans.Name, row, "teamidloanedfrom");
                if (sourceTeam > 0 && teamIds.Count > 0 && !teamIds.Contains(sourceTeam))
                    issues.Add(new("broken-loan-source-team", HealthSeverity.Error, loans.Name, row,
                        $"Loan source team {sourceTeam} does not exist.", false));
            }
            if (Has(loans, "loandateend") && Int(session, loans.Name, row, "loandateend") <= 0)
                issues.Add(new("invalid-loan-end-date", HealthSeverity.Warning, loans.Name, row,
                    $"Player {player} has no valid loan end date.", false));
        }
    }

    private static void AnalyzeTeamAssetsAndLeague(DatabaseSession session, List<DatabaseHealthIssue> issues)
    {
        var teams = session.GetTable("teams");
        if (teams == null) return;
        var stadiums = session.GetTable("stadiums");
        var stadiumIds = stadiums == null ? [] : IdSet(session, stadiums, "stadiumid");
        var kits = session.GetTable("teamkits");
        var kitTeams = kits == null ? [] : IdSet(session, kits, "teamid");
        var leagueLinks = session.GetTable("leagueteamlinks");
        var leagueTeams = leagueLinks == null ? [] : IdSet(session, leagueLinks, "teamid");
        for (var row = 0; row < teams.RowCount; row++)
        {
            var team = Int(session, teams.Name, row, "teamid");
            if (Has(teams, "stadiumid"))
            {
                var stadium = Int(session, teams.Name, row, "stadiumid");
                if (stadium > 0 && stadiumIds.Count > 0 && !stadiumIds.Contains(stadium))
                    issues.Add(new("missing-stadium", HealthSeverity.Warning, teams.Name, row,
                        $"Team {team} references missing stadium {stadium}.", false));
            }
            if (kits != null && team > 0 && !kitTeams.Contains(team))
                issues.Add(new("missing-kit", HealthSeverity.Warning, teams.Name, row,
                    $"Team {team} has no teamkits record.", false));
            if (leagueLinks != null && team > 0 && !leagueTeams.Contains(team))
                issues.Add(new("team-without-league", HealthSeverity.Info, teams.Name, row,
                    $"Team {team} has no league link (national/free-agent teams may be valid).", false));
        }
    }

    private static void AnalyzeNationalTeams(DatabaseSession session, List<DatabaseHealthIssue> issues)
    {
        var links = session.GetTable("teamnationlinks");
        var teams = session.GetTable("teams");
        var nations = session.GetTable("nations");
        if (links == null || teams == null || nations == null) return;
        var teamIds = IdSet(session, teams, "teamid");
        var nationIds = IdSet(session, nations, "nationid");
        for (var row = 0; row < links.RowCount; row++)
        {
            var team = Int(session, links.Name, row, "teamid");
            var nationField = Has(links, "nationid") ? "nationid" : Has(links, "nationality") ? "nationality" : null;
            if (!teamIds.Contains(team))
                issues.Add(new("invalid-national-team", HealthSeverity.Error, links.Name, row,
                    $"National-team link references missing team {team}.", false));
            if (nationField != null)
            {
                var nation = Int(session, links.Name, row, nationField);
                if (!nationIds.Contains(nation))
                    issues.Add(new("invalid-team-nation", HealthSeverity.Error, links.Name, row,
                        $"National-team link references missing nation {nation}.", false));
            }
        }
    }

    private static void AnalyzeManagers(DatabaseSession session, List<DatabaseHealthIssue> issues)
    {
        var managers = session.GetTable("manager");
        var teams = session.GetTable("teams");
        if (managers == null || teams == null || !Has(managers, "managerid")) return;
        var managerIds = IdSet(session, managers, "managerid");
        var teamIds = IdSet(session, teams, "teamid");
        var managerTeams = new Dictionary<int, int>();
        if (Has(managers, "teamid"))
        {
            for (var row = 0; row < managers.RowCount; row++)
            {
                var manager = Int(session, managers.Name, row, "managerid");
                var team = Int(session, managers.Name, row, "teamid");
                if (team <= 0) continue;
                if (!teamIds.Contains(team))
                    issues.Add(new("broken-manager-team", HealthSeverity.Error, managers.Name, row,
                        $"Manager {manager} references missing team {team}.", false));
                if (managerTeams.TryGetValue(team, out var existing) && existing != manager)
                    issues.Add(new("duplicate-team-manager", HealthSeverity.Warning, managers.Name, row,
                        $"Team {team} is linked to managers {existing} and {manager}.", false));
                else managerTeams[team] = manager;
            }
        }
        if (!Has(teams, "managerid")) return;
        for (var row = 0; row < teams.RowCount; row++)
        {
            var team = Int(session, teams.Name, row, "teamid");
            var manager = Int(session, teams.Name, row, "managerid");
            if (manager > 0 && !managerIds.Contains(manager))
                issues.Add(new("missing-team-manager", HealthSeverity.Warning, teams.Name, row,
                    $"Team {team} references missing manager {manager}.", false));
            if (manager > 0 && managerTeams.TryGetValue(team, out var reverse) && reverse != manager)
                issues.Add(new("manager-link-mismatch", HealthSeverity.Warning, teams.Name, row,
                    $"Team {team} points to manager {manager}, but manager table points to {reverse}.", false));
        }
    }

    private static HashSet<int> IdSet(DatabaseSession session, DbTable table, string field)
    {
        var values = new HashSet<int>();
        if (!Has(table, field)) return values;
        for (var row = 0; row < table.RowCount; row++)
        {
            var value = Int(session, table.Name, row, field);
            if (value >= 0) values.Add(value);
        }
        return values;
    }

    private static bool Has(DbTable table, string field) => table.FindColumn(field) != null;
    private static int Int(DatabaseSession session, string table, int row, string field) =>
        int.TryParse(session.GetCell(table, row, field), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value : -1;
}
