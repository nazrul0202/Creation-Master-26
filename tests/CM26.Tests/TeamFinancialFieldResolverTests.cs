using CM26.Application.Models;
using CM26.Application.Services;

namespace CM26.Tests;

public sealed class TeamFinancialFieldResolverTests
{
    [Fact]
    public void PrefersRealTransferBudgetWhenSchemaProvidesIt()
    {
        var selected = TeamFinancialFieldResolver.Resolve(Table("clubworth", "transferbudget"));

        Assert.NotNull(selected);
        Assert.Equal("transferbudget", selected.FieldName);
        Assert.Equal("Transfer Budget", selected.DisplayName);
        Assert.True(selected.IsTransferBudget);
    }

    [Fact]
    public void LabelsFc26ClubWorthHonestly()
    {
        var selected = TeamFinancialFieldResolver.Resolve(Table("clubworth"));

        Assert.NotNull(selected);
        Assert.Equal("clubworth", selected.FieldName);
        Assert.Equal("Club Worth", selected.DisplayName);
        Assert.False(selected.IsTransferBudget);
    }

    [Fact]
    public void ReturnsNullWhenNoFinancialFieldExists() =>
        Assert.Null(TeamFinancialFieldResolver.Resolve(Table("teamid")));

    private static DbTable Table(params string[] columns) => new()
    {
        Name = "teams",
        ShortName = "teams",
        RowCount = 1,
        IsLocale = false,
        Columns = columns.Select(name => new DbColumn
        {
            Name = name,
            ShortName = name,
            Kind = 3,
            Depth = 0,
            RangeLow = 0,
            RangeHigh = int.MaxValue,
            IsWritable = true,
        }).ToArray(),
    };
}
