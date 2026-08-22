namespace CM26.Application.Services;

/// <summary>Locates an optional F3D desktop viewer without installing software.</summary>
public static class F3dViewerLocator
{
    public static string? FindInstalled(string appBaseDirectory)
    {
        var configured = Environment.GetEnvironmentVariable("CM26_F3D_PATH");
        return Find(
            appBaseDirectory,
            configured,
            Environment.GetEnvironmentVariable("PATH"),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
    }

    public static string? Find(
        string appBaseDirectory,
        string? configuredPath,
        string? pathVariable,
        string? localAppData,
        string? programFiles,
        string? programFilesX86)
    {
        var candidates = new List<string?>
        {
            configuredPath,
            Path.Combine(appBaseDirectory, "Tools", "F3D", "f3d.exe"),
            Path.Combine(appBaseDirectory, "f3d.exe"),
            Combine(localAppData, "Programs", "F3D", "bin", "f3d.exe"),
            Combine(programFiles, "F3D", "bin", "f3d.exe"),
            Combine(programFilesX86, "F3D", "bin", "f3d.exe"),
        };

        if (!string.IsNullOrWhiteSpace(pathVariable))
        {
            candidates.AddRange(pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(folder => Combine(folder.Trim().Trim('"'), "f3d.exe")));
        }

        return candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(candidate => Path.GetFullPath(candidate!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(File.Exists);
    }

    private static string? Combine(string? root, params string[] parts) =>
        string.IsNullOrWhiteSpace(root) ? null : Path.Combine([root, .. parts]);
}
