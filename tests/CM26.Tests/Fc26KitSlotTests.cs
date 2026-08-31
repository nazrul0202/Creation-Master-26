using CM26.Application.Services;
using Xunit;

namespace CM26.Tests;

public sealed class Fc26KitSlotTests
{
    [Theory]
    [InlineData(0, "Home", "home")]
    [InlineData(1, "Away", "away")]
    [InlineData(2, "Goalkeeper", "gk")]
    [InlineData(3, "Third", "third")]
    public void CoreSlotsUseDatabaseAndFrostbiteMapping(int code, string label, string variant)
    {
        Assert.Equal(label, Fc26KitSlot.Label(code));
        Assert.True(Fc26KitSlot.TryGetAssetVariant(code, out var actual));
        Assert.Equal(variant, actual);
    }

    [Fact]
    public void ColourPathUsesSelectedKitIdentity()
    {
        Assert.Equal(
            "content/character/kit/11/gk_1_0/jersey_1366_1_0_color.dds",
            Fc26KitSlot.BuildColourTexturePath(11, 2, 1366));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(30)]
    public void NonCoreSlotsDoNotGuessAnAssetFolder(int code)
    {
        Assert.False(Fc26KitSlot.TryGetAssetVariant(code, out _));
    }
}
