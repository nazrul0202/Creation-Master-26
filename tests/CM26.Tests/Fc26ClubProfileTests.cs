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
}
