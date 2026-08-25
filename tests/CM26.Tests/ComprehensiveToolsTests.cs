using CM26.App.Sections;

namespace CM26.Tests;

public sealed class ComprehensiveToolsTests
{
    [Fact]
    public void HubContainsEveryAcceptedModuleExactlyOnce()
    {
        Assert.Equal(Enumerable.Range(1, 38), ComprehensiveToolsSection.ModuleNumbers);
    }

    [Fact]
    public void EveryModuleHasAnOperationalRoute()
    {
        var registeredRoutes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "$open-game", "$health", "countries", "leagues", "teams", "players",
            "managers", "stadiums", "kits", "competitions", "formations", "transfers",
            "modmanager", "balls", "boots", "gloves", "adboards", "scoreboard",
            "browser", "diagnostics", "settings",
        };

        Assert.All(ComprehensiveToolsSection.ModuleRoutes,
            route => Assert.Contains(route, registeredRoutes));
    }
}
