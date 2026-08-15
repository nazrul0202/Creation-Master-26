using CM26.App;

namespace CM26.Tests;

public sealed class LegacyFrostbiteAssetResolverTests
{
    [Theory]
    [InlineData("data/ui/imgAssets/countryshapes/c149.dds", "ui/country/shape_149", "c149", "c149")]
    public void CountryShape_AcceptsCompatibleFamily(
        string logicalPath, string candidate, string stem, string query)
    {
        Assert.True(LegacyFrostbiteAssetResolver.IsCompatibleMatch(
            logicalPath, candidate, stem, query));
    }

    [Theory]
    [InlineData("ui/clubs/teamflag_149")]
    [InlineData("ui/club/lincoln_city_banner_149")]
    [InlineData("characters/face/eye_texture_149")]
    [InlineData("ui/country/flag_1149")]
    [InlineData("ui/country/flag_1490")]
    public void CountryAssets_RejectUnrelatedOrUnboundedMatches(string candidate)
    {
        Assert.False(LegacyFrostbiteAssetResolver.IsCompatibleMatch(
            "data/ui/imgAssets/countryflags/f_149.big", candidate, "f_149", "flag_149"));
    }

    [Theory]
    [InlineData("ui/country/flag_149")]
    [InlineData("ui/nations/nation_149_flag")]
    [InlineData("ui/flags/f_149")]
    public void CountryFlags_RejectEvenPlausibleFuzzyResNames(string candidate)
    {
        Assert.False(LegacyFrostbiteAssetResolver.IsCompatibleMatch(
            "data/ui/imgAssets/flags512x512/f_149.dds", candidate, "f_149", "flag_149"));
    }

    [Theory]
    [InlineData("data/ui/imgAssets/heads/p270735.dds", "characters/player_head_270735", true)]
    [InlineData("data/ui/imgAssets/heads/p270735.dds", "ui/team_banner_270735", false)]
    [InlineData("data/ui/imgAssets/stadium/stadium_149_0.dds", "world/stadium_149_diffuse", true)]
    [InlineData("data/ui/imgAssets/stadium/stadium_149_0.dds", "characters/eye_149", false)]
    [InlineData("data/ui/imgAssets/settingsimg/ball_33.dds", "props/ball_33_color", true)]
    [InlineData("data/ui/imgAssets/settingsimg/ball_33.dds", "ui/crest_33", false)]
    [InlineData("data/ui/imgAssets/boots/item_160_0_0_0.dds", "characters/boot_160_0", true)]
    [InlineData("data/ui/imgAssets/boots/item_160_0_0_0.dds", "characters/hair_160", false)]
    public void OtherAssets_RequireExactOrFamilyCompatibleMatch(
        string logicalPath, string candidate, bool expected)
    {
        var stem = Path.GetFileNameWithoutExtension(logicalPath);
        Assert.Equal(expected, LegacyFrostbiteAssetResolver.IsCompatibleMatch(
            logicalPath, candidate, stem, stem));
    }
}
