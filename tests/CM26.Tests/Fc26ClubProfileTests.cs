using CM26.Application.Services;

namespace CM26.Tests;

public sealed class Fc26ClubProfileTests
{
    [Theory]
    [InlineData("9", "Very High")]
    [InlineData("8", "High")]
    [InlineData("3", "Low")]
    [InlineData("", "—")]
    public void RatingBandsMatchFc26ClubProfile(string? raw, string expected)
    {
        Assert.Equal(expected, Fc26ClubProfile.RatingLabel(raw));
    }

    [Fact]
    public void ClubWorthIsStoredInThousands() =>
        Assert.Equal("4.56B", Fc26ClubProfile.FormatClubWorth("4564360"));

    [Fact]
    public void DecoClubWorthUsesTheTeamDetailsScale() =>
        Assert.Equal("$175,068,000.00", Fc26ClubProfile.FormatDecoClubWorth("162100"));

    [Fact]
    public void DecoTransferBudgetMatchesLegacyDisplay()
    {
        var estimate = Fc26ClubProfile.EstimateDecoTransferBudget(162100, 6);

        Assert.Equal(17289023.40328413m, estimate, 2);
        Assert.Equal("$17,289,023.40", Fc26ClubProfile.FormatDecoTransferBudget("162100", "6"));
    }
}
