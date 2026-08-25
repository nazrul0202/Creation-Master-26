using CM26.Application.Services;

namespace CM26.Tests;

public sealed class TableWorkspaceTests
{
    [Fact]
    public void PendingSessionResetClearsUndoRedoStructuralAndHistoryState()
    {
        using var session = new DatabaseSession();
        var pending = new PendingChangesService(session);
        pending.MarkStructuralChange();

        pending.ResetSession("Opened a new source.");

        Assert.False(pending.HasChanges);
        Assert.False(pending.CanUndo);
        Assert.False(pending.CanRedo);
        Assert.Single(pending.History);
        Assert.Equal("Opened a new source.", pending.History[0].Description);
    }

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

    [Theory]
    [InlineData("Arsenal", "senal", true)]
    [InlineData("Arsenal", "=arsenal", true)]
    [InlineData("Arsenal", "!=Chelsea", true)]
    [InlineData("87", ">= 85", true)]
    [InlineData("87", "< 80", false)]
    [InlineData("text", "> 2", false)]
    public void WorkspaceFilterSupportsTextExactAndNumericExpressions(string value, string expression, bool expected)
    {
        Assert.Equal(expected, TableWorkspaceService.MatchesFilter(value, expression));
    }

    [Fact]
    public void AssetDependencyCatalogCoversEveryPublicVisualEditorFamily()
    {
        var types = AssetDependencyService.SupportedTypes;

        Assert.Contains(types, value => value.Contains("face", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(types, value => value.Contains("crest", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(types, value => value.Contains("stadium", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(types, value => value.Contains("ball", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(types, value => value.Contains("boot", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(types, value => value.Contains("kit", StringComparison.OrdinalIgnoreCase));
    }
}
