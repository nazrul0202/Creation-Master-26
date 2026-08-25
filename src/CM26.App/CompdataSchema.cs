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
        ["compids"] = ["Competition Object ID", "Database Competition ID"],
        ["standings"] = ["Group Object ID", "Rank"],
        ["schedule"] = ["Object ID", "Day", "Round", "Minimum Games", "Maximum Games", "Kick-off Time"],
        ["advancement"] = ["Source Group ID", "Source Rank", "Destination Group ID", "Destination Rank"],
        ["initteams"] = ["Competition Object ID", "Last-season Position", "Database Team ID"],
        ["objectives"] = ["Objective", "Competition Object ID", "Value"],
        ["settings"] = ["Competition Object ID", "Setting", "Value"],
        ["tasks"] = ["Trigger", "Task", "Source Object ID", "Rank", "Database ID", "Target Object ID"],
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
            if (!TryInt(objects.Rows[row], 4, out var parent)) continue;
            if (parent == -1)
            {
                if (type != 0) issues.Add(CompdataValidationIssue.Error("compobj", row + 1, "Only a World object can use parent ID -1."));
                continue;
            }
            if (!types.ContainsKey(parent))
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, $"Parent Object ID {parent} does not exist."));
            else if (parent == id)
                issues.Add(CompdataValidationIssue.Error("compobj", row + 1, "An object cannot be its own parent."));
        }

        ValidateReference(tables, "standings", 0, types, 5, "Group Object ID", issues);
        ValidateReference(tables, "schedule", 0, types, [4, 5], "Object ID", issues);
        ValidateReference(tables, "initteams", 0, types, 3, "Competition Object ID", issues);
        ValidateReference(tables, "advancement", 0, types, 5, "Source Group ID", issues);
        ValidateReference(tables, "advancement", 2, types, 5, "Destination Group ID", issues);
        ValidateCalendar(tables, issues);
        ValidateAdvancement(tables, issues);
        return issues;
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
            if (!keys.Add($"{objectId}|{day}|{round}")) issues.Add(CompdataValidationIssue.Error("schedule", row + 1, "Duplicate object/day/round creates a schedule conflict."));
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
            if (!TryInt(table.Rows[row], column, out var id)) continue;
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
}
