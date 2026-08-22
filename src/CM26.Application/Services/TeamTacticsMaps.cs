namespace CM26.Application.Services;

/// <summary>
/// FC26 team-tactic values verified against the installed FC26 database.
/// Values 0/1 in the non-active mentality rows are sentinels and must not be
/// presented as an additional user-facing tactic.
/// </summary>
public static class TeamTacticsMaps
{
    public sealed record Option(int Value, string Name)
    {
        public override string ToString() => $"{Name} ({Value})";
    }

    public sealed record MentalityCandidate(int RowIndex, int BuildUpPlay, int DefensiveDepth);

    public static IReadOnlyList<Option> BuildUpStyles { get; } =
    [
        new(1, "Short Passing"),
        new(2, "Balanced"),
        new(3, "Counter"),
    ];

    public static IReadOnlyList<Option> DefensivePresets { get; } =
    [
        new(30, "Deep"),
        new(50, "Balanced"),
        new(65, "High"),
        new(90, "Aggressive"),
    ];

    /// <summary>
    /// These are the ten low-order trait bits decoded by the legacy database
    /// model. FC26 also uses higher bits; callers must preserve those bits.
    /// </summary>
    public static IReadOnlyList<string> KnownTraitNames { get; } =
    [
        "Impatient Board",
        "Loyal Board",
        "Squad Rotation",
        "Consistent Lineup",
        "Switch Wingers",
        "Center Backs Split",
        "Defend Lead",
        "Keep Up Pressure",
        "More Attacking At Home",
        "Short Out Back",
    ];

    public const int KnownTraitMask = (1 << 10) - 1;

    public static string BuildUpStyle(int value) =>
        BuildUpStyles.FirstOrDefault(option => option.Value == value)?.Name ?? $"Unknown ({value})";

    public static string DefensiveApproach(int depth) => depth switch
    {
        <= 30 => "Deep",
        <= 60 => "Balanced",
        < 90 => "High",
        _ => "Aggressive",
    };

    public static IReadOnlyList<string> DecodeKnownTraits(int value) =>
        KnownTraitNames.Where((_, bit) => (value & (1 << bit)) != 0).ToArray();

    public static int SetKnownTrait(int originalValue, int bit, bool enabled)
    {
        if (bit is < 0 or >= 10) throw new ArgumentOutOfRangeException(nameof(bit));
        var flag = 1 << bit;
        return enabled ? originalValue | flag : originalValue & ~flag;
    }

    /// <summary>
    /// Each FC26 team normally has five default mentality rows. Four contain
    /// sentinel values (build-up 0 and depth 1); the active/default row carries
    /// the same real tactic as the teams/defaultteamdata records.
    /// </summary>
    public static int FindActiveMentalityRow(IEnumerable<MentalityCandidate> candidates)
    {
        var rows = candidates.ToArray();
        var exact = rows.FirstOrDefault(candidate =>
            candidate.BuildUpPlay is >= 1 and <= 3 && candidate.DefensiveDepth is >= 2 and <= 100);
        if (exact != null) return exact.RowIndex;
        return rows.FirstOrDefault(candidate =>
            candidate.BuildUpPlay > 0 || candidate.DefensiveDepth > 1)?.RowIndex ?? -1;
    }
}
