using System.Globalization;
using System.Text;

namespace CM26.Application.Models;

/// <summary>A row in a section browser list: resolved display text plus the underlying record index.</summary>
public sealed class RecordListItem
{
    private string? _normalizedSearchText;

    public required int RecordIndex { get; init; }
    public required string Title { get; init; }
    public string Subtitle { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;

    /// <summary>Optional extra text searched but not displayed (e.g. all name parts + ids + club).</summary>
    public string SearchText { get; init; } = string.Empty;

    public bool Matches(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;
        if (Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Detail.Contains(query, StringComparison.OrdinalIgnoreCase)
            || SearchText.Contains(query, StringComparison.OrdinalIgnoreCase))
            return true;

        // FC26 contains many names with diacritics (for example Benjamin Sesko's
        // display spelling is Benjamin Šeško). CM16 users expect an ASCII query
        // to find those records, so keep a folded search value per list item.
        _normalizedSearchText ??= FoldForSearch($"{Title}\n{Subtitle}\n{Detail}\n{SearchText}");
        return _normalizedSearchText.Contains(FoldForSearch(query), StringComparison.Ordinal);
    }

    internal static string FoldForSearch(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                result.Append(char.ToUpperInvariant(character));
        }
        return result.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Compact text used by the CM16-style record selector.</summary>
    public override string ToString()
        => string.IsNullOrWhiteSpace(Subtitle) ? Title : $"{Title}    —    {Subtitle}";
}

/// <summary>A player resolved through the FC26 <c>teamplayerlinks</c> table for the Team roster view.</summary>
public sealed class TeamRosterItem
{
    public required int PlayerId { get; init; }
    public int JerseyNumber { get; init; }

    /// <summary>Verified real player name, or the documented "Player {playerId}" fallback.</summary>
    public required string Name { get; init; }

    /// <summary>True when <see cref="Name"/> is a verified real name; false for the fallback.</summary>
    public bool Resolved { get; init; }

    public required string Position { get; init; }
    public required string Overall { get; init; }
    public int LeagueAppearances { get; init; }
    public int LeagueGoals { get; init; }
    public int YellowCards { get; init; }
    public int RedCards { get; init; }
    public required string Form { get; init; }
    public required string Injury { get; init; }
    public bool IsTopScorer { get; init; }
    public required string LoanFrom { get; init; }
    public required string LoanEndDate { get; init; }
    public required string ContractValidUntil { get; init; }
    public required string JoiningDate { get; init; }
}

/// <summary>One editable field shown in an editor, with resolved display and edit metadata.</summary>
public sealed class FieldValue
{
    public required string FieldName { get; init; }
    public required string Label { get; init; }
    public required string Value { get; set; }
    public required string RawValue { get; init; }
    public required bool IsWritable { get; init; }
    public required string KindLabel { get; init; }
    public string? Hint { get; init; }
    public int? RangeLow { get; init; }
    public int? RangeHigh { get; init; }
    public bool Modified { get; set; }
}
