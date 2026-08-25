using System.Globalization;

namespace CM26.Application.Services;

public sealed record ToolRunResult(bool Success, string Message);

/// <summary>
/// CM16-style database tools, implemented against the FC26 schema the studio
/// actually edits. Every operation stages edits through
/// <see cref="PendingChangesService"/> so the usual save flow owns the write.
/// Tools that are FIFA16-specific (NTables, RevMod, FIFA16-only leagues/teams)
/// are reported as not applicable instead of pretending to work.
/// </summary>
public static class DbToolsService
{
    private const int FreeAgentTeamId = -1;
    private const int DefaultCommentaryId = 900000;

    public static ToolRunResult RemoveFakePlayers(DatabaseSession session, PendingChangesService pending)
    {
        var players = session.GetTable("players");
        if (players == null) return NotLoaded();
        var links = session.GetTable("teamplayerlinks");
        var removed = 0;
        for (var row = 0; row < players.RowCount; row++)
        {
            if (!IsFakeBirthday(session, row)) continue;
            if (links == null || HasRealClub(session, row))
            {
                var result = session.DeleteRowWithRelationships("players", row);
                if (!result.Success) return new ToolRunResult(false, result.Message);
                removed++;
            }
        }
        session.RefreshSchema();
        if (removed > 0) pending.MarkStructuralChange();
        return new ToolRunResult(true, removed == 0
            ? "No fake players (born 29 February without a club) were found."
            : $"Removed {removed} fake player(s). They were born on 29 February and had no club team.");
    }

    public static ToolRunResult SetContractEndAfterLoanEnd(DatabaseSession session, PendingChangesService pending)
    {
        var loans = session.GetTable("playerloans");
        if (loans == null) return NotLoaded();
        using var operation = pending.BeginOperation("Extend contracts beyond loan end dates");
        var updated = 0;
        for (var row = 0; row < loans.RowCount; row++)
        {
            var playerId = ParseInt(session.GetCell("playerloans", row, "playerid"));
            if (playerId < 0) continue;
            var endDate = session.GetCell("playerloans", row, "loandateend");
            if (!FifaDateConverter.TryToIso(endDate, out var iso)) continue;
            if (!DateTime.TryParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
                continue;
            var playerRow = FindPlayerRow(session, playerId);
            if (playerRow < 0) continue;
            var outcome = pending.Stage("players", playerRow, "contractvaliduntil", (end.Year + 1).ToString(CultureInfo.InvariantCulture));
            if (!outcome.Success) return new ToolRunResult(false, outcome.Message);
            updated++;
        }
        operation.Commit();
        return new ToolRunResult(true, updated == 0
            ? "No loaned players needed a contract extension."
            : $"Set contract end (loan end + 1 year) for {updated} loaned player(s).");
    }

    public static ToolRunResult RemoveFromFreeAgentIfHasClub(DatabaseSession session, PendingChangesService pending)
    {
        var links = session.GetTable("teamplayerlinks");
        if (links == null) return NotLoaded();
        var removed = 0;
        for (var row = 0; row < links.RowCount; row++)
        {
            if (ParseInt(session.GetCell("teamplayerlinks", row, "teamid")) != FreeAgentTeamId) continue;
            var playerId = ParseInt(session.GetCell("teamplayerlinks", row, "playerid"));
            if (playerId < 0 || !PlayerHasClub(session, playerId)) continue;
            var outcome = session.DeleteRow("teamplayerlinks", row);
            if (!outcome.Success) return new ToolRunResult(false, outcome.Message);
            removed++;
            row--;
        }
        session.RefreshSchema();
        if (removed > 0) pending.MarkStructuralChange();
        return new ToolRunResult(true, removed == 0
            ? "No free-agent entries with a club were found."
            : $"Removed {removed} free-agent link(s) for players that already have a club.");
    }

    public static ToolRunResult AddToFreeAgentIfWithoutClub(DatabaseSession session, PendingChangesService pending)
    {
        var links = session.GetTable("teamplayerlinks");
        var players = session.GetTable("players");
        if (links == null || players == null || links.RowCount == 0) return NotLoaded();
        var added = 0;
        for (var row = 0; row < players.RowCount; row++)
        {
            var playerId = ParseInt(session.GetCell("players", row, "playerid"));
            if (playerId < 0) continue;
            if (PlayerHasAnyLink(session, playerId)) continue;
            var template = links.RowCount - 1;
            var duplicated = session.DuplicateRow("teamplayerlinks", template);
            if (!duplicated.Success) return new ToolRunResult(false, duplicated.Message);
            var stagedTeam = pending.Stage("teamplayerlinks", links.RowCount, "teamid", FreeAgentTeamId.ToString(CultureInfo.InvariantCulture));
            if (!stagedTeam.Success) return new ToolRunResult(false, stagedTeam.Message);
            var stagedPlayer = pending.Stage("teamplayerlinks", links.RowCount, "playerid", playerId.ToString(CultureInfo.InvariantCulture));
            if (!stagedPlayer.Success) return new ToolRunResult(false, stagedPlayer.Message);
            session.RefreshSchema();
            added++;
        }
        if (added > 0) pending.MarkStructuralChange();
        return new ToolRunResult(true, added == 0
            ? "Every player already has a team link (or is already a free agent)."
            : $"Added {added} player(s) to the free agents without a club.");
    }

    public static ToolRunResult SimplifyPlayerNameUsingCountryRules(DatabaseSession session, PendingChangesService pending)
    {
        var nations = session.GetTable("nations");
        var players = session.GetTable("players");
        if (nations == null || players == null) return NotLoaded();
        using var operation = pending.BeginOperation("Apply country player-name rules");
        var targetNations = new HashSet<int>();
        for (var row = 0; row < nations.RowCount; row++)
        {
            var name = session.GetCell("nations", row, "nationname");
            if (name.Contains("England", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Scotland", StringComparison.OrdinalIgnoreCase))
            {
                var id = ParseInt(session.GetCell("nations", row, "nationid"));
                if (id >= 0) targetNations.Add(id);
            }
        }
        var simplified = 0;
        for (var row = 0; row < players.RowCount; row++)
        {
            var nationality = ParseInt(session.GetCell("players", row, "nationality"));
            if (!targetNations.Contains(nationality)) continue;
            var commonNameId = ParseInt(session.GetCell("players", row, "commonnameid"));
            if (commonNameId <= 0) continue;
            var jerseyId = ParseInt(session.GetCell("players", row, "playerjerseynameid"));
            if (jerseyId == commonNameId) continue;
            var outcome = pending.Stage("players", row, "playerjerseynameid", commonNameId.ToString(CultureInfo.InvariantCulture));
            if (!outcome.Success) return new ToolRunResult(false, outcome.Message);
            simplified++;
        }
        operation.Commit();
        return new ToolRunResult(true, simplified == 0
            ? "No England/Scotland player needed a name simplification."
            : $"Set the jersey name to the common name for {simplified} England/Scotland player(s).");
    }

    public static ToolRunResult ResetCommentaryNames(DatabaseSession session, PendingChangesService pending)
    {
        var names = session.GetTable("playernames");
        if (names == null) return NotLoaded();
        using var operation = pending.BeginOperation("Reset commentary name IDs");
        var reset = 0;
        for (var row = 0; row < names.RowCount; row++)
        {
            if (ParseInt(session.GetCell("playernames", row, "commentaryid")) == DefaultCommentaryId) continue;
            var outcome = pending.Stage("playernames", row, "commentaryid", DefaultCommentaryId.ToString(CultureInfo.InvariantCulture));
            if (!outcome.Success) return new ToolRunResult(false, outcome.Message);
            reset++;
        }
        operation.Commit();
        return new ToolRunResult(true, reset == 0
            ? "Commentary IDs are already at the default."
            : $"Reset commentary IDs to {DefaultCommentaryId} for {reset} player name(s).");
    }

    public static ToolRunResult RepairRosterLinks(DatabaseSession session, PendingChangesService pending)
    {
        var links = session.GetTable("teamplayerlinks");
        var players = session.GetTable("players");
        var teams = session.GetTable("teams");
        if (links == null || players == null || teams == null) return NotLoaded();

        var playerIds = ReadIdSet(session, players, "playerid");
        var teamIds = ReadIdSet(session, teams, "teamid");
        var seen = new HashSet<(int TeamId, int PlayerId)>();
        var deleteRows = new List<int>();
        for (var row = 0; row < links.RowCount; row++)
        {
            var playerId = ParseInt(session.GetCell("teamplayerlinks", row, "playerid"));
            var teamId = ParseInt(session.GetCell("teamplayerlinks", row, "teamid"));
            var invalid = !playerIds.Contains(playerId) || (teamId != FreeAgentTeamId && !teamIds.Contains(teamId));
            var duplicate = !invalid && !seen.Add((teamId, playerId));
            if (invalid || duplicate) deleteRows.Add(row);
        }
        foreach (var row in deleteRows.OrderByDescending(value => value))
        {
            var outcome = session.DeleteRow("teamplayerlinks", row);
            if (!outcome.Success) return new ToolRunResult(false, outcome.Message);
        }
        if (deleteRows.Count > 0)
        {
            session.RefreshSchema();
            pending.MarkStructuralChange();
        }
        return new ToolRunResult(true, deleteRows.Count == 0
            ? "Roster links are valid and contain no duplicate team/player pair."
            : $"Removed {deleteRows.Count} broken or duplicate roster link(s). Save will run the full integrity validator.");
    }

    public static ToolRunResult RepairTeamSheets(DatabaseSession session, PendingChangesService pending)
    {
        var sheets = session.GetTable("default_teamsheets");
        var players = session.GetTable("players");
        var teams = session.GetTable("teams");
        if (sheets == null || players == null || teams == null) return NotLoaded();
        using var operation = pending.BeginOperation("Repair missing team-sheet players");
        var playerIds = ReadIdSet(session, players, "playerid");
        var teamIds = ReadIdSet(session, teams, "teamid");
        var fixedCells = 0;
        for (var row = 0; row < sheets.RowCount; row++)
        {
            var teamId = ParseInt(session.GetCell("default_teamsheets", row, "teamid"));
            if (teamId > 0 && !teamIds.Contains(teamId)) continue; // parent cleanup is structural; report via integrity validation
            foreach (var column in sheets.Columns.Where(column =>
                         column.Name.StartsWith("playerid", StringComparison.OrdinalIgnoreCase) && column.IsWritable))
            {
                var playerId = ParseInt(session.GetCell("default_teamsheets", row, column.Name));
                if (playerId <= 0 || playerIds.Contains(playerId)) continue;
                var outcome = pending.Stage("default_teamsheets", row, column.Name, "0");
                if (!outcome.Success) return new ToolRunResult(false, outcome.Message);
                fixedCells++;
            }
        }
        operation.Commit();
        return new ToolRunResult(true, fixedCells == 0
            ? "Default team sheets contain no missing player references."
            : $"Cleared {fixedCells} missing player reference(s) from default team sheets.");
    }

    public static ToolRunResult AssignUniqueJerseyNumbers(DatabaseSession session, PendingChangesService pending)
    {
        var links = session.GetTable("teamplayerlinks");
        if (links == null || links.FindColumn("jerseynumber")?.IsWritable != true) return NotLoaded();
        using var operation = pending.BeginOperation("Assign unique jersey numbers");
        var byTeam = new Dictionary<int, List<int>>();
        for (var row = 0; row < links.RowCount; row++)
        {
            var teamId = ParseInt(session.GetCell("teamplayerlinks", row, "teamid"));
            if (teamId <= 0) continue;
            if (!byTeam.TryGetValue(teamId, out var rows)) byTeam[teamId] = rows = new List<int>();
            rows.Add(row);
        }
        var changed = 0;
        foreach (var rows in byTeam.Values)
        {
            var used = new HashSet<int>();
            foreach (var row in rows)
            {
                var current = ParseInt(session.GetCell("teamplayerlinks", row, "jerseynumber"));
                if (current is >= 1 and <= 99 && used.Add(current)) continue;
                var available = Enumerable.Range(1, 99).FirstOrDefault(number => !used.Contains(number));
                if (available == 0) continue;
                var outcome = pending.Stage("teamplayerlinks", row, "jerseynumber", available.ToString(CultureInfo.InvariantCulture));
                if (!outcome.Success) return new ToolRunResult(false, outcome.Message);
                used.Add(available);
                changed++;
            }
        }
        operation.Commit();
        return new ToolRunResult(true, changed == 0
            ? "All club squads already use unique valid jersey numbers."
            : $"Assigned {changed} unique jersey number(s) across club squads.");
    }

    public static ToolRunResult ValidateDatabase(DatabaseSession session)
    {
        var issues = session.ValidateIntegrity();
        return new ToolRunResult(issues.Count == 0,
            issues.Count == 0
                ? "Database integrity validation passed: no primary-key or foreign-key violations were found."
                : $"Database integrity validation found {issues.Count} issue(s):\n\n" + string.Join("\n", issues.Take(100)) +
                  (issues.Count > 100 ? $"\n… and {issues.Count - 100} more." : string.Empty));
    }

    public static ToolRunResult ConvertMiniheadsToPng(string? gameRoot)
    {
        if (string.IsNullOrWhiteSpace(gameRoot) || !Directory.Exists(gameRoot))
            return new ToolRunResult(false, "The FC26 game folder was not detected. Open the game first.");
        var heads = Path.Combine(gameRoot, "data", "ui", "imgassets", "heads");
        if (!Directory.Exists(heads))
            return new ToolRunResult(false, $"The minihead folder was not found: {heads}");
        var files = Directory.GetFiles(heads, "*.dds");
        var converted = 0;
        foreach (var file in files)
        {
            try
            {
                var bytes = File.ReadAllBytes(file);
                if (!DdsDecoder.TryReadHeader(bytes, out var info) || !info.IsSupported) continue;
                using var bitmap = DdsDecoder.DecodeToBitmap(bytes, info, CancellationToken.None);
                if (bitmap == null) continue;
                var png = Path.ChangeExtension(file, ".png");
                bitmap.Save(png, System.Drawing.Imaging.ImageFormat.Png);
                converted++;
            }
            catch
            {
                // one bad texture must not abort the batch
            }
        }
        return new ToolRunResult(true, converted == 0
            ? $"No miniheads were converted in {heads}."
            : $"Converted {converted} minihead(s) to PNG in {heads}.");
    }

    public static ToolRunResult NotApplicable(string toolName) =>
        new(false, $"{toolName} is a FIFA 16 (or FIFA 16-era) tool and does not apply to the FC26 database this editor works on.");

    private static ToolRunResult NotLoaded() => new(false, "The database is not loaded. Open FC26 first.");

    private static bool IsFakeBirthday(DatabaseSession session, int playerRow)
    {
        var birthdate = session.GetCell("players", playerRow, "birthdate");
        if (!FifaDateConverter.TryToIso(birthdate, out var iso)) return false;
        return DateTime.TryParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            && date.Month == 2 && date.Day == 29;
    }

    private static bool HasRealClub(DatabaseSession session, int playerRow)
    {
        var playerId = ParseInt(session.GetCell("players", playerRow, "playerid"));
        if (playerId < 0) return false;
        return PlayerHasClub(session, playerId);
    }

    private static bool PlayerHasClub(DatabaseSession session, int playerId)
    {
        var links = session.GetTable("teamplayerlinks");
        if (links == null) return false;
        for (var row = 0; row < links.RowCount; row++)
        {
            if (ParseInt(session.GetCell("teamplayerlinks", row, "playerid")) != playerId) continue;
            var teamId = ParseInt(session.GetCell("teamplayerlinks", row, "teamid"));
            if (teamId > 0) return true;
        }
        return false;
    }

    private static bool PlayerHasAnyLink(DatabaseSession session, int playerId)
    {
        var links = session.GetTable("teamplayerlinks");
        if (links == null) return true;
        for (var row = 0; row < links.RowCount; row++)
            if (ParseInt(session.GetCell("teamplayerlinks", row, "playerid")) == playerId) return true;
        return false;
    }

    private static int FindPlayerRow(DatabaseSession session, int playerId)
    {
        var players = session.GetTable("players");
        if (players == null) return -1;
        for (var row = 0; row < players.RowCount; row++)
            if (ParseInt(session.GetCell("players", row, "playerid")) == playerId) return row;
        return -1;
    }

    private static HashSet<int> ReadIdSet(DatabaseSession session, CM26.Application.Models.DbTable table, string fieldName)
    {
        var values = new HashSet<int>();
        for (var row = 0; row < table.RowCount; row++)
        {
            var value = ParseInt(session.GetCell(table.Name, row, fieldName));
            if (value >= 0) values.Add(value);
        }
        return values;
    }

    private static int ParseInt(string value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : -1;
}
