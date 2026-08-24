using CM26.Application.Services;

namespace CM26.Tests;

public sealed class TableWorkspaceTests
{
    [Fact]
    public void DelimitedCodecRoundTripsQuotesTabsAndNewlines()
    {
        var values = new[] { "plain", "has\ttab", "has \"quote\"", "two\r\nlines", string.Empty };
        var text = TableWorkspaceService.FormatRow(values, '\t') + "\r\n";

        var parsed = TableWorkspaceService.Parse(text, '\t');

        Assert.Single(parsed);
        Assert.Equal(values, parsed[0]);
    }

    [Fact]
    public void DelimitedCodecParsesMultipleCsvRows()
    {
        var parsed = TableWorkspaceService.Parse("__rowindex,name\r\n0,Alpha\r\n1,\"Beta, FC\"", ',');

        Assert.Equal(3, parsed.Count);
        Assert.Equal(new[] { "1", "Beta, FC" }, parsed[2]);
    }

    [Fact]
    public void DecoruizFolderCanBeUsedAsTheExactAssetRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "cm26-deco-" + Guid.NewGuid().ToString("N"));
        var ballFolder = Path.Combine(root, "assets", "26", "ballid");
        Directory.CreateDirectory(ballFolder);
        var expected = Path.Combine(ballFolder, "42.png");
        File.WriteAllBytes(expected, new byte[] { 1, 2, 3 });
        try
        {
            var catalog = new AssetCatalogService(root);

            Assert.Equal(expected, catalog.GetBall(42));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
