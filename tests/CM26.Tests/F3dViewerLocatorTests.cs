using CM26.Application.Services;

namespace CM26.Tests;

public sealed class F3dViewerLocatorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "cm26-f3d-test-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void PrefersPortableF3dBesideTheApplication()
    {
        var portable = Path.Combine(_root, "app", "Tools", "F3D", "f3d.exe");
        var configured = Path.Combine(_root, "configured", "f3d.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(portable)!);
        Directory.CreateDirectory(Path.GetDirectoryName(configured)!);
        File.WriteAllText(portable, string.Empty);
        File.WriteAllText(configured, string.Empty);

        var found = F3dViewerLocator.Find(Path.Combine(_root, "app"), null, null, null, null, null);

        Assert.Equal(portable, found, ignoreCase: true);
    }

    [Fact]
    public void SupportsExplicitConfiguredPath()
    {
        var configured = Path.Combine(_root, "custom", "f3d.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(configured)!);
        File.WriteAllText(configured, string.Empty);

        var found = F3dViewerLocator.Find(_root, configured, null, null, null, null);

        Assert.Equal(configured, found, ignoreCase: true);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
