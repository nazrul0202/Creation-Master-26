namespace CM26.Application.Models;

/// <summary>Lightweight managed mirror of a database table's schema (from the bridge).</summary>
public sealed class DbColumn
{
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required int Kind { get; init; } // bridge FieldKind as int
    public required int Depth { get; init; }
    public required int RangeLow { get; init; }
    public required int RangeHigh { get; init; }
    public required bool IsWritable { get; init; }

    public bool IsInteger => Kind == 3;
    public bool IsFloat => Kind == 4;
    public bool IsString => Kind == 0 || Kind == 13 || Kind == 14;
    public bool IsCompressed => Kind == 13 || Kind == 14;

    public string KindLabel => Kind switch
    {
        0 => "Text",
        3 => "Integer",
        4 => "Decimal",
        13 => "Text (short)",
        14 => "Text (long)",
        _ => "Unknown",
    };
}

public sealed class DbTable
{
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required int RowCount { get; init; }
    public required bool IsLocale { get; init; }
    public required IReadOnlyList<DbColumn> Columns { get; init; }

    public DbColumn? FindColumn(string name) =>
        Columns.FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.ShortName, name, StringComparison.OrdinalIgnoreCase));
}

/// <summary>A single record: raw cell strings aligned to <see cref="DbTable.Columns"/>.</summary>
public sealed class DbRecord
{
    public required int Index { get; init; }
    public required IReadOnlyList<string> Values { get; init; }

    public string Get(int columnIndex) =>
        columnIndex >= 0 && columnIndex < Values.Count ? Values[columnIndex] : string.Empty;
}

/// <summary>One staged, engine-validated change to a cell.</summary>
public sealed class PendingChange
{
    public required bool IsLocale { get; init; }
    public required string TableName { get; init; }
    public required int RowIndex { get; init; }
    public required string FieldName { get; init; }
    public required string OldValue { get; init; }
    public required string NewValue { get; init; }
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    public string Describe() => $"{TableName}[{RowIndex}].{FieldName}: '{OldValue}' → '{NewValue}'";
}
