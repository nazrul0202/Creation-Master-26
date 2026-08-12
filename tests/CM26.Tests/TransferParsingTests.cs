using CM26.App.Sections;

namespace CM26.Tests;

public sealed class TransferParsingTests
{
    [Theory]
    [InlineData("arsenal.html", "Arsenal FC", 2)]
    [InlineData("dedup.html", "Example United", 1)]
    [InlineData("empty.html", "Empty Club", 0)]
    public void ParseHtmlReadsFixtureWithoutDuplicateIds(string fixture, string team, int count)
    {
        var html = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", fixture));
        var parsed = TransfersSection.ParseForTest(html);
        Assert.Equal(team, parsed.TeamName);
        Assert.Equal(count, parsed.Players.Count);
        Assert.Equal(parsed.Players.Count, parsed.Players.Select(player => player.Id).Distinct().Count());
        Assert.DoesNotContain(parsed.Players, player => player.Name.Contains('<'));
    }

    [Theory]
    [InlineData("https://www.transfermarkt.com/club/startseite/verein/1", true)]
    [InlineData("https://www.transfermarkt.co.uk/club/kader/verein/1", true)]
    [InlineData("http://www.transfermarkt.com/club", false)]
    [InlineData("https://transfermarkt.evil.com/club", false)]
    [InlineData("https://example.com/club", false)]
    public void UrlValidationAllowsOnlyHttpsTransfermarkt(string value, bool expected) =>
        Assert.Equal(expected, TransfersSection.TryValidateUrl(value, out _));

    [Fact]
    public void SafeFileNameRemovesInvalidCharacters() =>
        Assert.DoesNotContain(':', TransfersSection.SafeFileName("Club: 26"));

    [Fact]
    public void CsvEscapesQuotes() =>
        Assert.Equal("\"A \"\"quoted\"\" club\"", TransfersSection.Csv("A \"quoted\" club"));

    [Theory]
    [InlineData(0u, "0 B")]
    [InlineData(1023u, "1,023 B")]
    [InlineData(1024u, "1.0 KB")]
    [InlineData(1048576u, "1.0 MB")]
    public void FormatSizeUsesExpectedBoundary(uint bytes, string expected) =>
        Assert.Equal(expected, AudioNationSection.FormatSize(bytes));
}
