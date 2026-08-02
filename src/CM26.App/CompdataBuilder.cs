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
        Add(tables["compids"], competitionId, request.DatabaseCompetitionId);

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

        CompdataSchema.EnsureCapacity(tables, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["compobj"] = 4, ["compids"] = 1, ["settings"] = 4,
            ["initteams"] = request.TeamIds.Distinct().Count(), ["standings"] = 1, ["schedule"] = 1,
        });
        var ids = ReserveObjectIds(tables["compobj"], 4);
        var countryObject = ids.Dequeue();
        var competitionObject = ids.Dequeue();
        var stageObject = ids.Dequeue();
        var groupObject = ids.Dequeue();
        Add(tables["compobj"], countryObject, 2, CountryCode(request.CountryName), $"NationName_{request.NationId}", confederationObjectId);
        Add(tables["settings"], countryObject, "nation_id", request.NationId);
        Add(tables["compobj"], competitionObject, 3, $"C{request.DatabaseLeagueId}", request.LeagueName, countryObject);
        Add(tables["compids"], competitionObject, request.DatabaseLeagueId);
        Add(tables["settings"], competitionObject, "comp_type", "LEAGUE");
        Add(tables["compobj"], stageObject, 4, "S1", "League Stage", competitionObject);
        Add(tables["settings"], stageObject, "match_stagetype", "LEAGUE");
        Add(tables["schedule"], stageObject, 1, 1, 1, 1, 1200);
        Add(tables["compobj"], groupObject, 5, "G1", "League Table", stageObject);
        Add(tables["settings"], groupObject, "match_matchsituation", "LEAGUE");
        Add(tables["standings"], groupObject, 0);
        var position = 0;
        foreach (var teamId in request.TeamIds.Distinct()) Add(tables["initteams"], competitionObject, position++, teamId);
        return new CompdataBuildResult(competitionObject, [stageObject], [groupObject]);
    }

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
