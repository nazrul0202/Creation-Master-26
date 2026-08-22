using CM26.Application.Services;

namespace CM26.Tests;

public sealed class Fc26FormationCatalogTests
{
    [Fact]
    public void CatalogContainsAll29UniqueFc26Layouts()
    {
        Assert.Equal(29, Fc26FormationCatalog.Entries.Count);
        Assert.Equal(29, Fc26FormationCatalog.Entries.Select(entry => entry.FormationId).Distinct().Count());
        Assert.Equal(29, Fc26FormationCatalog.Entries.Select(entry => entry.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory]
    [InlineData(3, "4-2-3-1 Narrow")]
    [InlineData(4, "4-2-3-1 Wide")]
    [InlineData(9, "4-3-3 Holding")]
    [InlineData(10, "4-3-3 Defend")]
    [InlineData(11, "4-3-3 Attack")]
    [InlineData(36, "4-2-1-3")]
    public void VariantNamesAreResolvedByFormationId(int id, string expected) =>
        Assert.Equal(expected, Fc26FormationCatalog.DisplayName(id));
}
