using CM26.Application.Services;

namespace CM26.Tests;

public sealed class TeamTacticsMapsTests
{
    [Theory]
    [InlineData(1, "Short Passing")]
    [InlineData(2, "Balanced")]
    [InlineData(3, "Counter")]
    public void MapsFc26BuildUpValues(int value, string expected) =>
        Assert.Equal(expected, TeamTacticsMaps.BuildUpStyle(value));

    [Theory]
    [InlineData(30, "Deep")]
    [InlineData(31, "Balanced")]
    [InlineData(60, "Balanced")]
    [InlineData(61, "High")]
    [InlineData(89, "High")]
    [InlineData(90, "Aggressive")]
    public void ClassifiesDefensiveLineHeight(int value, string expected) =>
        Assert.Equal(expected, TeamTacticsMaps.DefensiveApproach(value));

    [Fact]
    public void TraitEditorPreservesUnknownFc26Bits()
    {
        const int original = 1 << 22;
        var edited = TeamTacticsMaps.SetKnownTrait(original, 4, true);

        Assert.Equal((1 << 22) | (1 << 4), edited);
        Assert.Equal(original, TeamTacticsMaps.SetKnownTrait(edited, 4, false));
    }

    [Fact]
    public void FindsRealDefaultMentalityAndSkipsSentinels()
    {
        var row = TeamTacticsMaps.FindActiveMentalityRow(
        [
            new(100, 0, 1),
            new(101, 0, 1),
            new(102, 3, 30),
            new(103, 0, 1),
            new(104, 0, 1),
        ]);

        Assert.Equal(102, row);
    }
}
