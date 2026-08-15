using System.Drawing;
using System.Reflection;
using CM26.App.Theming;

namespace CM26.Tests;

public sealed class SectionIconTests
{
    private static readonly Dictionary<string, string> ResourceByKey = ReadResourceMap();

    private static Dictionary<string, string> ReadResourceMap()
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var field = typeof(IconService).GetField("ResourceByKey", flags)
            ?? throw new InvalidOperationException("ResourceByKey field missing.");
        return (Dictionary<string, string>)(field.GetValue(null)
            ?? throw new InvalidOperationException("ResourceByKey is null."));
    }

    [Fact]
    public void EveryMappedIconResourceIsEmbeddedInAssembly()
    {
        var assembly = typeof(IconService).Assembly;
        var manifest = assembly.GetManifestResourceNames().ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, resource) in ResourceByKey)
        {
            if (string.IsNullOrEmpty(resource)) continue;
            Assert.True(
                manifest.Contains(resource),
                $"Section '{key}' maps to missing resource '{resource}'.");
        }
    }

    [Theory]
    [InlineData("players")]
    [InlineData("teams")]
    [InlineData("leagues")]
    [InlineData("countries")]
    [InlineData("managers")]
    [InlineData("stadiums")]
    [InlineData("kits")]
    [InlineData("competitions")]
    [InlineData("formations")]
    [InlineData("transfers")]
    [InlineData("balls")]
    [InlineData("boots")]
    [InlineData("gloves")]
    [InlineData("sponsors")]
    [InlineData("audio")]
    [InlineData("scoreboard")]
    [InlineData("referees")]
    [InlineData("browser")]
    public void Cm16SectionsResolveRealIcons(string key)
    {
        Assert.False(string.IsNullOrWhiteSpace(ResourceByKey[key]), $"Section '{key}' must use a CM16 icon.");
        var icon = IconService.Get(key, 18);
        Assert.Equal(18, icon.Width);
        Assert.Equal(18, icon.Height);
    }

    [Theory]
    [InlineData("teams", "Team.png")]
    [InlineData("competitions", "Tournament.png")]
    [InlineData("gloves", "Gloves.png")]
    [InlineData("sponsors", "Sponsor.png")]
    [InlineData("audio", "Audio.png")]
    [InlineData("scoreboard", "GameGraphics.png")]
    [InlineData("referees", "Referee.png")]
    [InlineData("browser", "Browser.png")]
    public void Cm16IconMappingsAreCorrect(string key, string expectedFile)
    {
        Assert.EndsWith(expectedFile, ResourceByKey[key], StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("dashboard")]
    [InlineData("adboards")]
    [InlineData("stadiumaudio")]
    [InlineData("diagnostics")]
    [InlineData("settings")]
    public void BadgeFallbackSectionsStillRender(string key)
    {
        var icon = IconService.Get(key, 18);
        Assert.Equal(18, icon.Width);
        Assert.Equal(18, icon.Height);
    }

    [Fact]
    public void AllSidebarSectionKeysHaveIconResolution()
    {
        var keys = new[]
        {
            "dashboard", "countries", "leagues", "teams", "players", "managers",
            "stadiums", "stadiumaudio", "kits", "competitions", "formations", "balls",
            "boots", "gloves", "sponsors", "adboards", "scoreboard", "audio",
            "transfers", "modmanager", "referees", "browser", "diagnostics", "settings",
        };
        foreach (var key in keys)
        {
            var icon = IconService.Get(key, 18);
            Assert.Equal(18, icon.Width);
            Assert.Equal(18, icon.Height);
        }
    }
}
