using System.Xml.Linq;

namespace CM26.Tests;

public sealed class ReleaseBrandingTests
{
    [Fact]
    public void PublicExecutableUsesCreationMaster26NameEverywhere()
    {
        var root = FindRepositoryRoot();
        var project = XDocument.Load(Path.Combine(root, "src", "CM26.App", "CM26.App.csproj"));
        Assert.Equal("Creation Master 26",
            project.Descendants("AssemblyName").Single().Value);

        var packageScript = File.ReadAllText(Path.Combine(root, "Release", "assemble_packages.ps1"));
        Assert.Contains("Creation Master 26.exe", packageScript, StringComparison.Ordinal);
        Assert.Contains("Creation Master 26.dll", packageScript, StringComparison.Ordinal);
        Assert.DoesNotContain("CM26_by_Rizco98.exe", packageScript, StringComparison.Ordinal);

        var manifest = File.ReadAllText(Path.Combine(root, "src", "CM26.App", "app.manifest"));
        Assert.Contains("Creation Master 26.app", manifest, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseVersionSourcesStaySynchronized()
    {
        var root = FindRepositoryRoot();
        var props = XDocument.Load(Path.Combine(root, "Directory.Build.props"));
        var projectVersion = props.Descendants("CM26Version").Single().Value;
        var versionJson = System.Text.Json.JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "version.json")));
        Assert.Equal(projectVersion, versionJson.RootElement.GetProperty("version").GetString());
    }

    private static string FindRepositoryRoot()
    {
        var cursor = new DirectoryInfo(AppContext.BaseDirectory);
        while (cursor != null)
        {
            if (File.Exists(Path.Combine(cursor.FullName, "version.json")) &&
                Directory.Exists(Path.Combine(cursor.FullName, "src")))
                return cursor.FullName;
            cursor = cursor.Parent;
        }
        throw new DirectoryNotFoundException("Creation Master 26 repository root was not found.");
    }
}
