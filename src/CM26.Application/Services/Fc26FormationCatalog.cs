namespace CM26.Application.Services;

/// <summary>
/// Authoritative FC26 generic formation rows (formations.teamid = -1).
/// The database's short formationname is not unique, so consumers must use
/// formationid to preserve Narrow/Wide/Holding/Defend/Attack variants.
/// </summary>
public static class Fc26FormationCatalog
{
    public sealed record Entry(int FormationId, string DisplayName);

    public static readonly IReadOnlyList<Entry> Entries =
    [
        new(1, "4-1-3-2"),
        new(2, "4-1-4-1"),
        new(3, "4-2-3-1 Narrow"),
        new(4, "4-2-3-1 Wide"),
        new(5, "4-2-4"),
        new(6, "4-3-1-2"),
        new(7, "4-3-2-1"),
        new(8, "4-3-3 Flat"),
        new(9, "4-3-3 Holding"),
        new(10, "4-3-3 Defend"),
        new(11, "4-3-3 Attack"),
        new(13, "4-2-2-2"),
        new(14, "4-1-2-1-2 Wide"),
        new(15, "4-1-2-1-2 Narrow"),
        new(16, "4-4-2 Flat"),
        new(17, "4-4-2 Holding"),
        new(18, "4-4-1-1 Midfield"),
        new(20, "4-5-1 Flat"),
        new(21, "4-5-1 Attack"),
        new(22, "3-1-4-2"),
        new(23, "3-4-1-2"),
        new(24, "3-4-2-1"),
        new(25, "3-4-3 Flat"),
        new(27, "3-5-2"),
        new(29, "5-2-1-2"),
        new(30, "5-2-3"),
        new(31, "5-3-2 Holding"),
        new(33, "5-4-1 Flat"),
        new(36, "4-2-1-3"),
    ];

    private static readonly IReadOnlyDictionary<int, string> Names =
        Entries.ToDictionary(entry => entry.FormationId, entry => entry.DisplayName);

    public static string DisplayName(int formationId, string? fallback = null) =>
        Names.TryGetValue(formationId, out var name)
            ? name
            : string.IsNullOrWhiteSpace(fallback) ? $"Formation {formationId}" : fallback;

    public static int SortOrder(int formationId)
    {
        for (var index = 0; index < Entries.Count; index++)
            if (Entries[index].FormationId == formationId) return index;
        return int.MaxValue;
    }
}
