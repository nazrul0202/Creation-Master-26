using System.Text.RegularExpressions;
using CM26.Application.Services;

namespace CM26.App;

/// <summary>
/// Resolves original CM16 logical filenames against FC26. FC26 still exposes
/// some UI files through ChunkFileCollector, while newer previews are indexed
/// as Frostbite texture RES assets. This adapter keeps the original forms and
/// filename conventions unchanged and translates only at the archive boundary.
/// </summary>
internal static class LegacyFrostbiteAssetResolver
{
    private const uint TextureResType = 0x6BDE20BA;

    internal static string? Resolve(FrostbiteAssetSession assets, string logicalPath)
    {
        foreach (var alias in CollectorAliases(logicalPath))
        {
            var exported = assets.ExportLegacyAsset(alias);
            if (IsFile(exported)) return exported;
        }

        var normalized = logicalPath.Replace('\\', '/').ToLowerInvariant();
        var stem = Path.GetFileNameWithoutExtension(normalized);
        foreach (var query in SearchQueries(normalized, stem))
        {
            var match = assets.SearchAssets(query, "Res", 500)
                .Where(item => item.ResType == TextureResType)
                .OrderByDescending(item => Score(item.Name, stem, query))
                .FirstOrDefault();
            if (match == null) continue;
            var exported = assets.ExportTexture(match.Name);
            if (IsFile(exported)) return exported;
        }
        return null;
    }

    private static IEnumerable<string> CollectorAliases(string logicalPath)
    {
        var normalized = logicalPath.Replace('\\', '/').TrimStart('/');
        yield return normalized;

        var countryId = MatchId(normalized,
            @"(?:flags512x512/f_|cardflags/|countryshapes/c)(\d+)");
        if (countryId >= 0)
        {
            yield return $"data/ui/imgAssets/countryflags/f_{countryId}.big";
            yield return $"data/ui/imgAssets/miniflags/flag_{countryId}.big";
        }

        var stadiumId = MatchId(normalized, @"(?:stadium_|stadiumsbig/st_)(\d+)");
        if (stadiumId >= 0)
        {
            yield return $"data/ui/imgAssets/stadium/stadium_{stadiumId}_0.dds";
            yield return $"data/ui/external/ion_fut/imgAssets/stadiums/stadium_{stadiumId}.dds";
            yield return $"data/ui/imgAssets/clubInfo/stadium/st_{stadiumId}.dds";
        }
    }

    private static IEnumerable<string> SearchQueries(string path, string stem)
    {
        var queries = new List<string>();
        Add(queries, stem);

        AddIdQueries(queries, path, @"(?:settingsimg/ball_|/ball(?:s)?/ball_)(\d+)",
            id => [$"ball_{id}", $"ball{id}"]);
        AddIdQueries(queries, path, @"(?:stadium_|stadiumsbig/st_)(\d+)",
            id => [$"stadium_{id}", $"st_{id}"]);
        AddIdQueries(queries, path, @"/heads/p(\d+)",
            id => [$"p{id}", $"player_{id}", $"head_{id}", $"portrait_{id}"]);
        AddIdQueries(queries, path, @"/crest(?:16x16|32x32|50x50)?/(?:light|dark)/l(\d+)",
            id => [$"crest_{id}", $"team_{id}_crest"]);
        AddIdQueries(queries, path,
            @"(?:countryflags/f_|miniflags/flag_|cardflags/|flags512x512/f_)(\d+)",
            id => [$"flag_{id}", $"f_{id}"]);

        var boot = Regex.Match(path, @"/boots/item_(\d+)_(\d+)_(\d+)_(\d+)",
            RegexOptions.IgnoreCase);
        if (boot.Success)
        {
            Add(queries, $"item_{boot.Groups[1].Value}_{boot.Groups[2].Value}");
            Add(queries, $"boot_{boot.Groups[1].Value}_{boot.Groups[2].Value}");
            Add(queries, $"shoe_{boot.Groups[1].Value}");
        }
        var oldShoe = Regex.Match(path, @"/shoe/shoe_(\d+)", RegexOptions.IgnoreCase);
        if (oldShoe.Success)
        {
            Add(queries, $"shoe_{oldShoe.Groups[1].Value}");
            Add(queries, $"boot_{oldShoe.Groups[1].Value}");
            Add(queries, $"item_{oldShoe.Groups[1].Value}");
        }
        return queries;
    }

    private static void AddIdQueries(List<string> queries, string path, string pattern,
        Func<string, IEnumerable<string>> factory)
    {
        var match = Regex.Match(path, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return;
        foreach (var query in factory(match.Groups[1].Value)) Add(queries, query);
    }

    private static void Add(List<string> queries, string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length >= 3 &&
            !queries.Contains(value, StringComparer.OrdinalIgnoreCase))
            queries.Add(value);
    }

    private static int Score(string name, string stem, string query)
    {
        var candidate = name.Replace('\\', '/').ToLowerInvariant();
        var file = Path.GetFileNameWithoutExtension(candidate);
        var score = 0;
        if (file.Equals(stem, StringComparison.OrdinalIgnoreCase)) score += 1000;
        if (file.Equals(query, StringComparison.OrdinalIgnoreCase)) score += 900;
        if (candidate.EndsWith('/' + stem, StringComparison.OrdinalIgnoreCase)) score += 600;
        if (candidate.Contains("/ui/", StringComparison.OrdinalIgnoreCase) ||
            candidate.Contains("imgasset", StringComparison.OrdinalIgnoreCase)) score += 160;
        if (candidate.EndsWith("_color", StringComparison.OrdinalIgnoreCase) ||
            candidate.EndsWith("_diffuse", StringComparison.OrdinalIgnoreCase)) score += 120;
        if (candidate.Contains("normal", StringComparison.OrdinalIgnoreCase) ||
            candidate.Contains("coeff", StringComparison.OrdinalIgnoreCase) ||
            candidate.Contains("mask", StringComparison.OrdinalIgnoreCase)) score -= 300;
        return score;
    }

    private static int MatchId(string value, string pattern)
    {
        var match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : -1;
    }

    private static bool IsFile(string? path) =>
        !string.IsNullOrWhiteSpace(path) && File.Exists(path) && new FileInfo(path).Length > 0;
}
