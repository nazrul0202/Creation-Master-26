using CM26.Application.Models;

namespace CM26.Tests;

public sealed class RecordListItemTests
{
    private static readonly RecordListItem Sesko = new()
    {
        RecordIndex = 1,
        Title = "Benjamin Šeško",
        Subtitle = "Manchester United",
        SearchText = "260592 Slovenia",
    };

    [Theory]
    [InlineData("Sesko")]
    [InlineData("benjamin sesko")]
    [InlineData("Šeško")]
    [InlineData("260592")]
    public void MatchesPlayerNamesWithOrWithoutDiacritics(string query) =>
        Assert.True(Sesko.Matches(query));
}
