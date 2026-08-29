using System.Data;

namespace CM26.App;

internal sealed record CompdataLeagueBuildRequest(
    string Name, int DatabaseCompetitionId, int Stages, int GroupsPerStage);

internal sealed record CompdataBuildResult(int CompetitionObjectId, IReadOnlyList<int> StageIds, IReadOnlyList<int> GroupIds);
internal sealed record CountryCareerBuildRequest(
    string CountryName, int NationId, int ConfederationCode, string LeagueName,
    int DatabaseLeagueId, IReadOnlyList<int> TeamIds);

/// <summary>Creates a coherent, editable Compdata skeleton from the documented tables.</summary>
internal static class CompdataBuilder
{
    public static CompdataBuildResult CreateLeagueOrCup(
        IReadOnlyDictionary<string, DataTable> tables, CompdataLeagueBuildRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Competition name is required.");
        if (request.DatabaseCompetitionId < 0) throw new InvalidOperationException("Database Competition ID must be zero or greater.");
        if (request.Stages is < 1 or > 32 || request.GroupsPerStage is < 1 or > 64)
            throw new InvalidOperationException("Stages must be 1–32 and groups per stage must be 1–64.");
        Require(tables, "compobj", "compids", "standings", "schedule");

        var groupCount = checked(request.Stages * request.GroupsPerStage);
        CompdataSchema.EnsureCapacity(tables, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["compobj"] = 1 + request.Stages + groupCount,
            ["compids"] = 1,
            ["standings"] = groupCount,
            ["schedule"] = request.Stages,
        });

        var objectIds = ReserveObjectIds(tables["compobj"], 1 + request.Stages + groupCount);
        var competitionId = objectIds.Dequeue();
        Add(tables["compobj"], competitionId, 3, request.Name, request.Name, string.Empty);
        Add(tables["compids"], competitionId);

        var stages = new List<int>();
        var groups = new List<int>();
        for (var stageIndex = 1; stageIndex <= request.Stages; stageIndex++)
        {
            var stageId = objectIds.Dequeue();
            stages.Add(stageId);
            Add(tables["compobj"], stageId, 4, $"{request.Name} Stage {stageIndex}", string.Empty, competitionId);
            // A deliberately conservative schedule skeleton. Users set days, rounds and game counts in the grid.
            Add(tables["schedule"], stageId, 1, 1, 1, 1, 1200);
            for (var groupIndex = 1; groupIndex <= request.GroupsPerStage; groupIndex++)
            {
                var groupId = objectIds.Dequeue();
                groups.Add(groupId);
                Add(tables["compobj"], groupId, 5, $"{request.Name} S{stageIndex} Group {groupIndex}", string.Empty, stageId);
                Add(tables["standings"], groupId, 0);
            }
        }
        return new CompdataBuildResult(competitionId, stages, groups);
    }

    public static void AddAdvancement(IReadOnlyDictionary<string, DataTable> tables,
        int sourceGroupId, int sourceRank, int destinationGroupId, int destinationRank)
    {
        Require(tables, "compobj", "advancement");
        CompdataSchema.EnsureCapacity(tables, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["advancement"] = 1 });
        var objectTypes = tables["compobj"].Rows.Cast<DataRow>()
            .Where(row => TryInt(row, 0, out _) && TryInt(row, 1, out _))
            .ToDictionary(row => int.Parse(Convert.ToString(row[0])!), row => int.Parse(Convert.ToString(row[1])!));
        if (!objectTypes.TryGetValue(sourceGroupId, out var sourceType) || sourceType != 5 ||
            !objectTypes.TryGetValue(destinationGroupId, out var destinationType) || destinationType != 5)
            throw new InvalidOperationException("Promotion/relegation links must start and end at Compdata Group objects (type 5).");
        if (sourceRank < 0 || destinationRank < 0) throw new InvalidOperationException("Ranks cannot be negative.");
        Add(tables["advancement"], sourceGroupId, sourceRank, destinationGroupId, destinationRank);
    }

    public static CompdataBuildResult CreateCountryCareerLeague(
        IReadOnlyDictionary<string, DataTable> tables, CountryCareerBuildRequest request)
    {
        if (request.NationId <= 0 || request.DatabaseLeagueId <= 0)
            throw new InvalidOperationException("Country and League IDs must be positive.");
        if (request.TeamIds.Count < 2)
            throw new InvalidOperationException("Link at least two clubs to the league before building its Career setup.");
        Require(tables, "compobj", "compids", "settings", "initteams", "standings", "schedule");
        var confederation = request.ConfederationCode switch
        {
            2 => "UEFA", 3 => "CAF", 4 => "CNBL", 5 => "AFC", 6 => "OFC", 7 => "CCAF",
            _ => throw new InvalidOperationException("Select a country with a supported confederation."),
        };
        var parentId = tables["compobj"].Rows.Cast<DataRow>()
            .FirstOrDefault(row => TryInt(row, 1, out var type) && type == 1 &&
                string.Equals(Convert.ToString(row[2])?.Trim(), confederation, StringComparison.OrdinalIgnoreCase));
        if (parentId == null || !TryInt(parentId, 0, out var confederationObjectId))
            throw new InvalidOperationException($"The Compdata workbook has no {confederation} confederation object.");

        var teamIds = request.TeamIds.Where(id => id > 0).Distinct().ToArray();
        if (teamIds.Length < 2)
            throw new InvalidOperationException("Link at least two valid clubs to the league before building its Career setup.");
        var competitionCode = "C" + request.DatabaseLeagueId;
        if (tables["compobj"].Rows.Cast<DataRow>().Any(row =>
                TryInt(row, 1, out var type) && type == 3 &&
                string.Equals(Convert.ToString(row[2])?.Trim(), competitionCode, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"League {request.DatabaseLeagueId} already has a Compdata competition. Use Assign Teams or Generate Schedule to amend it.");

        var nationToken = $"NationName_{request.NationId}";
        var existingCountry = tables["compobj"].Rows.Cast<DataRow>().FirstOrDefault(row =>
            TryInt(row, 0, out _) && TryInt(row, 1, out var type) && type == 2 &&
            string.Equals(Convert.ToString(row[3])?.Trim(), nationToken, StringComparison.OrdinalIgnoreCase));
        var newCountry = existingCountry == null;
        CompdataSchema.EnsureCapacity(tables, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["compobj"] = newCountry ? 4 : 3, ["compids"] = 1,
            ["settings"] = newCountry ? 17 : 16,
            ["initteams"] = teamIds.Length, ["standings"] = teamIds.Length,
            ["schedule"] = RoundCount(teamIds.Length, 2),
        });
        var ids = ReserveObjectIds(tables["compobj"], newCountry ? 4 : 3);
        var countryObject = newCountry ? ids.Dequeue() : Convert.ToInt32(existingCountry![0]);
        var competitionObject = ids.Dequeue();
        var stageObject = ids.Dequeue();
        var groupObject = ids.Dequeue();
        if (newCountry)
        {
            Add(tables["compobj"], countryObject, 2, CountryCode(request.CountryName), nationToken, confederationObjectId);
            Add(tables["settings"], countryObject, "nation_id", request.NationId);
        }
        Add(tables["compobj"], competitionObject, 3, competitionCode, $"TrophyName_Abbr15_{request.DatabaseLeagueId}", countryObject);
        Add(tables["compids"], competitionObject);
        Add(tables["settings"], competitionObject, "asset_id", request.DatabaseLeagueId);
        Add(tables["settings"], competitionObject, "comp_type", "LEAGUE");
        Add(tables["settings"], competitionObject, "match_canusefancards", "on");
        Add(tables["settings"], competitionObject, "standings_sort", "POINTS");
        Add(tables["settings"], competitionObject, "standings_sort", "GOALDIFF");
        Add(tables["settings"], competitionObject, "standings_sort", "GOALSFOR");
        Add(tables["settings"], competitionObject, "standings_sort", "H2HPOINTS");
        Add(tables["compobj"], stageObject, 4, "S1", "FCE_League_Stage", competitionObject);
        Add(tables["settings"], stageObject, "match_stagetype", "LEAGUE");
        Add(tables["settings"], stageObject, "match_matchsituation", "LEAGUE");
        Add(tables["settings"], stageObject, "match_canusefancards", "on");
        Add(tables["settings"], stageObject, "standings_sort", "POINTS");
        Add(tables["settings"], stageObject, "standings_sort", "GOALDIFF");
        Add(tables["settings"], stageObject, "standings_sort", "GOALSFOR");
        Add(tables["settings"], stageObject, "standings_sort", "H2HPOINTS");
        Add(tables["settings"], stageObject, "schedule_year_real", 2024);
        Add(tables["compobj"], groupObject, 5, "G1", string.Empty, stageObject);
        Add(tables["settings"], groupObject, "num_games", 2);
        for (var rank = 0; rank < teamIds.Length; rank++) Add(tables["standings"], groupObject, rank);
        var rounds = RoundCount(teamIds.Length, 2);
        var games = Math.Max(1, teamIds.Length / 2);
        for (var round = 1; round <= rounds; round++)
            Add(tables["schedule"], stageObject, 201 + ((round - 1) * 7), round, games, games, 1500);
        var position = 0;
        foreach (var teamId in teamIds) Add(tables["initteams"], competitionObject, position++, teamId);
        return new CompdataBuildResult(competitionObject, [stageObject], [groupObject]);
    }

    private static int RoundCount(int teamCount, int legs) =>
        (teamCount % 2 == 0 ? teamCount - 1 : teamCount) * Math.Max(1, legs);

    private static string CountryCode(string name)
    {
        var letters = new string(name.Where(char.IsLetter).Select(char.ToUpperInvariant).ToArray());
        return string.IsNullOrWhiteSpace(letters) ? "COUN" : letters[..Math.Min(4, letters.Length)].PadRight(4, 'X');
    }

    private static Queue<int> ReserveObjectIds(DataTable table, int count)
    {
        var used = table.Rows.Cast<DataRow>().Select(row => TryInt(row, 0, out var id) ? id : -1).Where(id => id >= 0).ToHashSet();
        var next = used.Count == 0 ? 0 : used.Max() + 1;
        var result = new Queue<int>();
        while (result.Count < count)
        {
            if (!used.Contains(next)) result.Enqueue(next);
            next++;
        }
        return result;
    }

    private static void Require(IReadOnlyDictionary<string, DataTable> tables, params string[] sheets)
    {
        foreach (var sheet in sheets)
            if (!tables.ContainsKey(sheet)) throw new InvalidOperationException($"Required worksheet '{sheet}' is missing.");
    }

    private static void Add(DataTable table, params object[] values)
    {
        var row = table.NewRow();
        for (var index = 0; index < values.Length && index < table.Columns.Count; index++) row[index] = values[index].ToString() ?? string.Empty;
        table.Rows.Add(row);
    }

    private static bool TryInt(DataRow row, int column, out int value)
    {
        value = 0;
        return column < row.Table.Columns.Count && int.TryParse(Convert.ToString(row[column]), out value);
    }
}
