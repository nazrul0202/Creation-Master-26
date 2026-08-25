namespace CM26.Application.Services;

public sealed record AssetDependencyHit(string AssetType, int AssetId, string TableName, string FieldName, int RowIndex);

/// <summary>Known FC26 database-to-visual-asset references used by CM26 preview/import workflows.</summary>
public static class AssetDependencyService
{
    private static readonly (string Type, string Table, string Field)[] Mappings =
    [
        ("Player face/miniface", "players", "playerid"),
        ("Team crest", "teams", "teamid"),
        ("League logo", "leagues", "leagueid"),
        ("Competition logo", "competition", "competitionid"),
        ("Stadium", "stadiums", "stadiumid"),
        ("Ball", "teamballs", "ballid"),
        ("Ball", "competitionballs", "ballid"),
        ("Boot", "playerboots", "shoetype"),
        ("Goalkeeper glove", "players", "gkglovetypecode"),
        ("Country flag", "nations", "nationid"),
        ("Manager face", "manager", "managerid"),
        ("Kit", "teamkits", "teamid"),
    ];

    public static IReadOnlyList<AssetDependencyHit> Find(DatabaseSession session, string assetType, int assetId)
    {
        var hits = new List<AssetDependencyHit>();
        foreach (var mapping in Mappings.Where(mapping =>
                     mapping.Type.Contains(assetType, StringComparison.OrdinalIgnoreCase) ||
                     assetType.Contains(mapping.Type, StringComparison.OrdinalIgnoreCase)))
        {
            var table = session.GetTable(mapping.Table);
            if (table?.FindColumn(mapping.Field) == null) continue;
            for (var row = 0; row < table.RowCount; row++)
                if (int.TryParse(session.GetCell(table.Name, row, mapping.Field), out var value) && value == assetId)
                    hits.Add(new(mapping.Type, assetId, table.Name, mapping.Field, row));
        }
        return hits;
    }

    public static IReadOnlyList<string> SupportedTypes => Mappings.Select(mapping => mapping.Type).Distinct().ToArray();
}
