using System.Data;

namespace CM26.App;

/// <summary>
/// The official Compdata workbook stores raw rows without a header row.  Keep the
/// schema in the editor instead of treating the first competition record as one.
/// </summary>
internal static class CompdataSchema
{
    private static readonly Dictionary<string, int> RowLimits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["advancement"] = 4500, ["compids"] = 115, ["compobj"] = 2500,
        ["initteams"] = 500, ["objectives"] = 670, ["schedule"] = 8500,
        ["settings"] = 5500, ["standings"] = 6000, ["tasks"] = 1020,
        ["weather"] = 420,
    };
    private static readonly Dictionary<string, string[]> Columns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["compobj"] = ["Object ID", "Object Type", "Short Name", "Description", "Parent Object ID"],
        // FC26 stores one Compdata object ID per row. The linked database ID is
        // encoded by the competition object's C{id} short name.
        ["compids"] = ["Competition Object ID"],
        ["standings"] = ["Group Object ID", "Rank"],
        ["schedule"] = ["Object ID", "Day", "Round", "Minimum Games", "Maximum Games", "Kick-off Time"],
        ["advancement"] = ["Source Group ID", "Source Rank", "Destination Group ID", "Destination Rank"],
        ["initteams"] = ["Competition Object ID", "Last-season Position", "Database Team ID"],
        ["objectives"] = ["Competition Object ID", "Objective", "Value"],
        ["settings"] = ["Competition Object ID", "Setting", "Value"],
        ["tasks"] = ["Competition Object ID", "Trigger", "Task", "Source Object ID", "Rank", "Database ID", "Target Object ID"],
        ["weather"] = ["Weather ID", "Temperature", "Rain", "Snow", "Wind", "Cloud", "Start Time", "End Time"],
    };

    public static bool HasFixedColumns(string sheetName) => Columns.ContainsKey(sheetName);

    public static string[] GetColumns(string sheetName, int width)
    {
        if (!Columns.TryGetValue(sheetName, out var known))
            return Enumerable.Range(1, Math.Max(width, 1)).Select(index => $"Column {index}").ToArray();
        return Enumerable.Range(0, Math.Max(width, known.Length))
            .Select(index => index < known.Length ? known[index] : $"Value {index + 1}")
            .ToArray();
    }

    public static bool CanCreateStandaloneRow(string? sheetName) =>
        string.Equals(sheetName, "compobj", StringComparison.OrdinalIgnoreCase);

    public static int? GetRowLimit(string sheetName) =>
        RowLimits.TryGetValue(sheetName, out var limit) ? limit : null;

    public static void EnsureCapacity(IReadOnlyDictionary<string, DataTable> tables,
        IReadOnlyDictionary<string, int> additions)
    {
        foreach (var (sheet, amount) in additions)
        {
            if (!RowLimits.TryGetValue(sheet, out var limit) || amount <= 0) continue;
            var current = tables.TryGetValue(sheet, out var table) ? table.Rows.Count : 0;
            if (current + amount > limit)
                throw new InvalidOperationException(
                    $"{sheet} has room for {Math.Max(0, limit - current):N0} more row(s), but this action needs {amount:N0}. Limit: {limit:N0}.");
        }
    }

    public static IReadOnlyList<CompdataValidationIssue> Validate(IReadOnlyDictionary<string, DataTable> tables)
    {
        var issues = new List<CompdataValidationIssue>();
        foreach (var (sheet, table) in tables)
            if (RowLimits.TryGetValue(sheet, out var limit) && table.Rows.Count > limit)
                issues.Add(CompdataValidationIssue.Error(sheet, 0,
                    $"Section has {table.Rows.Count:N0} rows, exceeding the FC26 limit of {limit:N0}."));
        if (!tables.TryGetValue("compobj", out var objects))
        {
            issues.Add(CompdataValidationIssue.Error("compobj", 0, "Missing compobj worksheet."));
            return issues;
        }

        var types = new Dictionary<int, int>();
        for (var row = 0; row < objects.Rows.Count; row++)
        {
            if (!TryInt(objects.Rows[row], 0, out var id))
            {
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, "Object ID is required."));
                continue;
            }
            if (!TryInt(objects.Rows[row], 1, out var type) || type is < 0 or > 6)
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, "Object Type must be 0 (World) through 6 (special group)."));
            else if (!types.TryAdd(id, type))
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, $"Object ID {id} is duplicated."));
        }

        for (var row = 0; row < objects.Rows.Count; row++)
        {
            if (!TryInt(objects.Rows[row], 0, out var id) || !TryInt(objects.Rows[row], 1, out var type)) continue;
            if (!TryInt(objects.Rows[row], 4, out var parent))
            {
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, "Parent Object ID is required and must be an integer."));
                continue;
            }
            if (parent == -1)
            {
                if (type != 0) issues.Add(CompdataValidationIssue.Error("compobj", row + 1, "Only a World object can use parent ID -1."));
                continue;
            }
            if (!types.ContainsKey(parent))
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, $"Parent Object ID {parent} does not exist."));
            else if (parent == id)
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, "An object cannot be its own parent."));
            else if (!IsValidParentType(type, types[parent]))
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1,
                    $"Object type {type} cannot use a type {types[parent]} parent."));

            if (type == 3)
            {
                var shortName = Convert.ToString(objects.Rows[row][2])?.Trim() ?? string.Empty;
                if (shortName.Length < 2 || shortName[0] != 'C' ||
                    !int.TryParse(shortName[1..], out var databaseId) || databaseId <= 0)
                    issues.Add(CompdataValidationIssue.Error("compobj", row + 1,
                        "Competition Short Name must be C followed by its positive database competition ID."));
            }
        }

        ValidateReference(tables, "standings", 0, types, 5, "Group Object ID", issues);
        ValidateReference(tables, "schedule", 0, types, [4, 5], "Object ID", issues);
        // FC26 registers both normal competition objects (3) and special
        // competition groups (6) in compids.
        ValidateReference(tables, "compids", 0, types, [3, 6], "Registered Competition Object ID", issues);
        ValidateReference(tables, "initteams", 0, types, 3, "Competition Object ID", issues);
        ValidateReference(tables, "settings", 0, types, [0, 1, 2, 3, 4, 5, 6], "Competition Object ID", issues);
        ValidateReference(tables, "objectives", 0, types, [0, 1, 2, 3, 4, 5, 6], "Competition Object ID", issues);
        ValidateReference(tables, "tasks", 0, types, [0, 1, 2, 3, 4, 5, 6], "Competition Object ID", issues);
        ValidateReference(tables, "advancement", 0, types, 5, "Source Group ID", issues);
        ValidateReference(tables, "advancement", 2, types, 5, "Destination Group ID", issues);
        ValidateUniqueRows(tables, "compids", [0], "Competition object is registered more than once.", issues);
        ValidateCompetitionMappings(tables, objects, issues);
        ValidateCalendar(tables, issues);
        foreach (var conflict in FindTeamCalendarConflicts(tables).Take(200))
            issues.Add(CompdataValidationIssue.Warning("schedule", conflict.ScheduleRow,
                $"Team {conflict.TeamId} is assigned to competitions {conflict.FirstCompetitionId} and {conflict.SecondCompetitionId}, both scheduled on day {conflict.Day}. Review calendar spacing."));
        ValidateAdvancement(tables, issues);
        return issues;
    }

    internal static IReadOnlyList<CompdataCalendarConflict> FindTeamCalendarConflicts(
        IReadOnlyDictionary<string, DataTable> tables)
    {
        if (!tables.TryGetValue("compobj", out var objects) ||
            !tables.TryGetValue("initteams", out var initTeams) ||
            !tables.TryGetValue("schedule", out var schedule))
            return Array.Empty<CompdataCalendarConflict>();

        var parentByObject = new Dictionary<int, int>();
        var typeByObject = new Dictionary<int, int>();
        foreach (DataRow row in objects.Rows)
            if (TryInt(row, 0, out var id) && TryInt(row, 1, out var type) && TryInt(row, 4, out var parent))
            { parentByObject[id] = parent; typeByObject[id] = type; }

        int CompetitionOf(int objectId)
        {
            var visited = new HashSet<int>();
            while (visited.Add(objectId) && typeByObject.TryGetValue(objectId, out var type))
            {
                if (type == 3) return objectId;
                if (!parentByObject.TryGetValue(objectId, out objectId)) break;
            }
            return -1;
        }

        var teamsByCompetition = initTeams.Rows.Cast<DataRow>()
            .Where(row => TryInt(row, 0, out _) && TryInt(row, 2, out _))
            .GroupBy(row => { TryInt(row, 0, out var id); return id; })
            .ToDictionary(group => group.Key,
                group => group.Select(row => { TryInt(row, 2, out var id); return id; }).Distinct().ToHashSet());
        var scheduled = schedule.Rows.Cast<DataRow>().Select((row, index) => new
            {
                Row = index + 1,
                Object = TryInt(row, 0, out var id) ? id : -1,
                Day = TryInt(row, 1, out var day) ? day : -1
            })
            .Select(value => new { value.Row, Competition = CompetitionOf(value.Object), value.Day })
            .Where(value => value.Competition >= 0 && value.Day >= 0)
            .GroupBy(value => new { value.Competition, value.Day })
            .Select(group => new { group.Key.Competition, group.Key.Day, Row = group.Min(value => value.Row) })
            .ToArray();

        var conflicts = new List<CompdataCalendarConflict>();
        foreach (var day in scheduled.GroupBy(value => value.Day))
        {
            var entries = day.ToArray();
            for (var left = 0; left < entries.Length; left++)
                for (var right = left + 1; right < entries.Length; right++)
                {
                    if (!teamsByCompetition.TryGetValue(entries[left].Competition, out var first) ||
                        !teamsByCompetition.TryGetValue(entries[right].Competition, out var second)) continue;
                    foreach (var teamId in first.Intersect(second))
                        conflicts.Add(new CompdataCalendarConflict(entries[right].Row, day.Key, teamId,
                            entries[left].Competition, entries[right].Competition));
                }
        }
        return conflicts;
    }

    private static void ValidateCompetitionMappings(IReadOnlyDictionary<string, DataTable> tables,
        DataTable objects, ICollection<CompdataValidationIssue> issues)
    {
        if (!tables.TryGetValue("compids", out var compIds) ||
            !tables.TryGetValue("settings", out var settings)) return;
        var registered = compIds.Rows.Cast<DataRow>()
            .Select(row => TryInt(row, 0, out var id) ? id : -1).Where(id => id >= 0).ToHashSet();
        for (var row = 0; row < objects.Rows.Count; row++)
        {
            var data = objects.Rows[row];
            if (!TryInt(data, 0, out var objectId) || !TryInt(data, 1, out var type) || type != 3) continue;
            var shortName = Convert.ToString(data[2])?.Trim() ?? string.Empty;
            if (shortName.Length < 2 || shortName[0] != 'C' ||
                !int.TryParse(shortName[1..], out var databaseId) || databaseId <= 0) continue;
            if (!registered.Contains(objectId))
                issues.Add(CompdataValidationIssue.Error("compids", 0,
                    $"Competition object {objectId} ({shortName}) is not registered."));
            var linkedSettings = settings.Rows.Cast<DataRow>()
                .Where(setting => TryInt(setting, 0, out var id) && id == objectId).ToArray();
            var assetMapped = linkedSettings.Any(setting =>
                string.Equals(Convert.ToString(setting[1])?.Trim(), "asset_id", StringComparison.OrdinalIgnoreCase) &&
                TryInt(setting, 2, out var assetId) && assetId == databaseId);
            if (!assetMapped)
                issues.Add(CompdataValidationIssue.Error("settings", 0,
                    $"Competition object {objectId} must map asset_id {databaseId} from {shortName}."));
            var hasType = linkedSettings.Any(setting =>
                string.Equals(Convert.ToString(setting[1])?.Trim(), "comp_type", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(Convert.ToString(setting[2])));
            if (!hasType)
                issues.Add(CompdataValidationIssue.Error("settings", 0,
                    $"Competition object {objectId} ({shortName}) has no comp_type setting."));
        }
    }

    private static bool IsValidParentType(int type, int parentType) => type switch
    {
        1 => parentType == 0,
        2 => parentType == 1,
        3 => parentType is 0 or 1 or 2,
        4 => parentType == 3,
        5 => parentType is 4 or 5 or 6,
        6 => parentType == 1,
        _ => false,
    };

    private static void ValidateUniqueRows(IReadOnlyDictionary<string, DataTable> tables, string sheet,
        IReadOnlyList<int> columns, string message, ICollection<CompdataValidationIssue> issues)
    {
        if (!tables.TryGetValue(sheet, out var table)) return;
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var row = 0; row < table.Rows.Count; row++)
        {
            var values = new List<string>(columns.Count);
            var complete = true;
            foreach (var column in columns)
            {
                if (column >= table.Columns.Count || string.IsNullOrWhiteSpace(Convert.ToString(table.Rows[row][column])))
                { complete = false; break; }
                values.Add(Convert.ToString(table.Rows[row][column])!.Trim());
            }
            if (complete && !keys.Add(string.Join("|", values)))
                issues.Add(CompdataValidationIssue.Error(sheet, row + 1, message));
        }
    }

    private static void ValidateCalendar(IReadOnlyDictionary<string, DataTable> tables,
        ICollection<CompdataValidationIssue> issues)
    {
        if (!tables.TryGetValue("schedule", out var table)) return;
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var row = 0; row < table.Rows.Count; row++)
        {
            var data = table.Rows[row];
            if (!TryInt(data, 0, out var objectId) || !TryInt(data, 1, out var day) ||
                !TryInt(data, 2, out var round) || !TryInt(data, 3, out var minimum) ||
                !TryInt(data, 4, out var maximum) || !TryInt(data, 5, out var kickoff))
            {
                issues.Add(CompdataValidationIssue.Error("schedule", row + 1, "Object, day, round, game range and kick-off must be integers."));
                continue;
            }
            if (day < 0 || round < 0) issues.Add(CompdataValidationIssue.Error("schedule", row + 1, "Day and round cannot be negative."));
            if (minimum < 0 || maximum < minimum) issues.Add(CompdataValidationIssue.Error("schedule", row + 1, "Maximum games must be greater than or equal to minimum games."));
            if (kickoff < 0 || kickoff > 2359 || kickoff % 100 > 59) issues.Add(CompdataValidationIssue.Error("schedule", row + 1, "Kick-off must use a valid FC HHMM value (0000–2359)."));
            // FC26 legitimately uses multiple kick-off/game-range rows for the same
            // object, day and round. Only a fully identical schedule row is a duplicate.
            if (!keys.Add($"{objectId}|{day}|{round}|{minimum}|{maximum}|{kickoff}"))
                issues.Add(CompdataValidationIssue.Error("schedule", row + 1, "Exact duplicate schedule row."));
        }
    }

    private static void ValidateAdvancement(IReadOnlyDictionary<string, DataTable> tables,
        ICollection<CompdataValidationIssue> issues)
    {
        if (!tables.TryGetValue("advancement", out var table)) return;
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var row = 0; row < table.Rows.Count; row++)
        {
            var data = table.Rows[row];
            if (!TryInt(data, 0, out var source) || !TryInt(data, 1, out var sourceRank) ||
                !TryInt(data, 2, out var destination) || !TryInt(data, 3, out var destinationRank)) continue;
            if (source == destination && sourceRank == destinationRank)
                issues.Add(CompdataValidationIssue.Error("advancement", row + 1, "An advancement path cannot point back to the same group and rank."));
            if (!keys.Add($"{source}|{sourceRank}|{destination}|{destinationRank}"))
                issues.Add(CompdataValidationIssue.Error("advancement", row + 1, "Duplicate advancement path."));
        }
    }

    private static void ValidateReference(IReadOnlyDictionary<string, DataTable> tables, string sheet, int column,
        IReadOnlyDictionary<int, int> types, int expectedType, string label, ICollection<CompdataValidationIssue> issues) =>
        ValidateReference(tables, sheet, column, types, [expectedType], label, issues);

    private static void ValidateReference(IReadOnlyDictionary<string, DataTable> tables, string sheet, int column,
        IReadOnlyDictionary<int, int> types, IReadOnlyCollection<int> expectedTypes, string label, ICollection<CompdataValidationIssue> issues)
    {
        if (!tables.TryGetValue(sheet, out var table)) return;
        for (var row = 0; row < table.Rows.Count; row++)
        {
            if (!TryInt(table.Rows[row], column, out var id))
            {
                issues.Add(CompdataValidationIssue.Error(sheet, row + 1, $"{label} is required and must be an integer."));
                continue;
            }
            if (!types.TryGetValue(id, out var actual))
                issues.Add(CompdataValidationIssue.Error(sheet, row + 1, $"{label} {id} does not exist in compobj."));
            else if (!expectedTypes.Contains(actual))
                issues.Add(CompdataValidationIssue.Error(sheet, row + 1, $"{label} {id} has object type {actual}, which is not valid here."));
        }
    }

    private static bool TryInt(DataRow row, int column, out int value)
    {
        value = 0;
        return column < row.Table.Columns.Count && int.TryParse(Convert.ToString(row[column]), out value);
    }
}

internal sealed record CompdataValidationIssue(string Sheet, int Row, string Message, bool IsError)
{
    public static CompdataValidationIssue Error(string sheet, int row, string message) => new(sheet, row, message, true);
    public static CompdataValidationIssue Warning(string sheet, int row, string message) => new(sheet, row, message, false);
}

internal sealed record CompdataCalendarConflict(int ScheduleRow, int Day, int TeamId,
    int FirstCompetitionId, int SecondCompetitionId);
