using System.Drawing;
using CM26.App.Sections;
using CM26.Application.Services;

namespace CM26.Tests;

public sealed class SectionLogicTests
{
    [Fact]
    public void NextAvailableIdFindsFirstGap() =>
        Assert.Equal(3, SectionBase.FindNextAvailableId([1, 2, 4, 8], 1, 10));

    [Fact]
    public void CreateRecordTemplateInsertionIsSourcePlusOne() =>
        Assert.Equal(8, SectionBase.InsertedRowAfter(7));

    [Fact]
    public void SafeTeamIdSkipsMappedCrestRange() =>
        Assert.Equal(130, SectionBase.FindSafeTeamId([118, 119], [120, 121, 129]));

    [Fact]
    public void PositionCodesMapAllFc26Positions()
    {
        Assert.Equal(28, SectionBase.PositionLabels.Length);
        for (var code = 0; code <= 27; code++)
        {
            var label = NameResolverService.PositionLabel(code);
            Assert.Equal(SectionBase.PositionLabels[code], label);
            Assert.True(SectionBase.TryPositionCode(label, out var parsed));
            Assert.Equal(code, parsed);
        }
    }

    [Fact]
    public void DefaultSquadContainsTwentyThreePositions() =>
        Assert.Equal(23, SectionBase.DefaultSquadPositions.Length);

    [Fact]
    public void FormationBoxAvoidsCollisionAndStaysOnPitch()
    {
        var occupied = new[] { new Rectangle(80, 80, 88, 48) };
        var result = FormationsSection.FindFreeFormationBox(80, 80, 88, 48, occupied, new Size(400, 300));
        Assert.DoesNotContain(occupied, box => box.IntersectsWith(result));
        Assert.InRange(result.Left, 0, 312);
        Assert.InRange(result.Top, 0, 252);
    }
}
