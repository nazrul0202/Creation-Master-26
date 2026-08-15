using CM26.Application.Services;

namespace CM26.Tests;

public sealed class DbToolsTests
{
    [Fact]
    public void NotApplicableExplainsFifa16OnlyTools()
    {
        var result = DbToolsService.NotApplicable("Expand Database");
        Assert.False(result.Success);
        Assert.Contains("FIFA 16", result.Message);
    }

    [Fact]
    public void RemoveFakePlayersWithoutLoadedDbFailsCleanly()
    {
        using var session = new DatabaseSession();
        var pending = new PendingChangesService(session);
        var result = DbToolsService.RemoveFakePlayers(session, pending);
        Assert.False(result.Success);
        Assert.Contains("not loaded", result.Message);
    }

    [Fact]
    public void SetContractEndWithoutLoadedDbFailsCleanly()
    {
        using var session = new DatabaseSession();
        var pending = new PendingChangesService(session);
        var result = DbToolsService.SetContractEndAfterLoanEnd(session, pending);
        Assert.False(result.Success);
        Assert.Contains("not loaded", result.Message);
    }

    [Fact]
    public void ConvertMiniheadsWithoutGameRootFailsCleanly()
    {
        var result = DbToolsService.ConvertMiniheadsToPng(null);
        Assert.False(result.Success);
        Assert.Contains("game folder", result.Message);
    }

    [Fact]
    public void SimplifyNamesWithoutLoadedDbFailsCleanly()
    {
        using var session = new DatabaseSession();
        var pending = new PendingChangesService(session);
        var result = DbToolsService.SimplifyPlayerNameUsingCountryRules(session, pending);
        Assert.False(result.Success);
        Assert.Contains("not loaded", result.Message);
    }
}
