using System.Globalization;

namespace CM26.Application.Services;

/// <summary>
/// FC26 installs each league logo in up to three legacy UI families
/// (256x256, 512x128 banner and 200x64 tiny). A replaced logo is staged
/// into every family the installed game actually carries, so all menus
/// show one consistent image after the direct transaction commits.
/// </summary>
public static class LeagueLogoCatalog
{
    public static readonly (string PathFormat, int Width, int Height)[] Families =
    {
        ("data/ui/imgAssets/league/light/l{0}.dds", 256, 256),
        ("data/ui/imgAssets/league512x128/light/l{0}.dds", 512, 128),
        ("data/ui/imgAssets/leaguelogos_tiny/light/l{0}.dds", 200, 64),
    };

    public static string[] Paths(int leagueId) =>
        Families.Select(f => Format(f.PathFormat, leagueId)).ToArray();

    public static string PrimaryPath(int leagueId) => Format(Families[0].PathFormat, leagueId);

    /// <summary>Legacy paths that resolve in the installed game (or are already staged).</summary>
    public static IReadOnlyList<string> EditablePaths(
        FrostbiteAssetSession assets, LegacyAssetModService mods, int leagueId)
    {
        var result = new List<string>();
        foreach (var path in Paths(leagueId))
        {
            if (mods.GetReplacement(path) != null) { result.Add(path); continue; }
            try
            {
                var exported = assets.ExportLegacyAsset(path);
                if (!string.IsNullOrWhiteSpace(exported) && File.Exists(exported)) result.Add(path);
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (InvalidOperationException) { }
        }
        return result;
    }

    /// <summary>Stages one source image into every editable family. Returns the last staged file.</summary>
    public static string StageAll(
        LegacyAssetModService mods, IReadOnlyList<string> legacyPaths, int leagueId, string sourcePath)
    {
        var byPath = Families.ToDictionary(
            f => Format(f.PathFormat, leagueId), f => (f.Width, f.Height), StringComparer.OrdinalIgnoreCase);
        string last = string.Empty;
        foreach (var path in legacyPaths)
        {
            var (width, height) = byPath.TryGetValue(path, out var dims)
                ? dims
                : (256, 256);
            last = mods.StageImage(path, sourcePath, width, height);
        }
        return last;
    }

    public static bool RemoveAll(LegacyAssetModService mods, int leagueId)
    {
        var removed = false;
        foreach (var path in Paths(leagueId))
            removed |= mods.Remove(path);
        return removed;
    }

    /// <summary>The staged replacement for any family, or the first installed export.</summary>
    public static string? PreviewSource(
        FrostbiteAssetSession assets, LegacyAssetModService mods, int leagueId)
    {
        foreach (var path in Paths(leagueId))
        {
            var staged = mods.GetReplacement(path);
            if (!string.IsNullOrWhiteSpace(staged) && File.Exists(staged)) return staged;
        }
        foreach (var path in Paths(leagueId))
        {
            try
            {
                var exported = assets.ExportLegacyAsset(path);
                if (!string.IsNullOrWhiteSpace(exported) && File.Exists(exported)) return exported;
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (InvalidOperationException) { }
        }
        return null;
    }

    private static string Format(string format, int leagueId) =>
        string.Format(CultureInfo.InvariantCulture, format, leagueId);
}
