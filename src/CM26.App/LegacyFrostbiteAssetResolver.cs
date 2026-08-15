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
                .Where(item => IsCompatibleMatch(normalized, item.Name, stem, query))
                .OrderByDescending(item => Score(item.Name, stem, query))
                .FirstOrDefault();
            if (match == null) continue;
            var exported = assets.ExportTexture(match.Name);
            if (IsFile(exported)) return exported;
        }
        return null;
    }

    internal static bool IsCompatibleMatch(string logicalPath, string assetName, string stem, string query)
    {
        var candidate = assetName.Replace('\\', '/').ToLowerInvariant();
        var file = Path.GetFileNameWithoutExtension(candidate);

        // Never accept a fuzzy result purely because a short numeric token
        // happens to occur in its name. That previously mapped country flags
        // to club banners and face/eye textures with the same numeric id.
        var exactStem = file.Equals(stem, StringComparison.OrdinalIgnoreCase) ||
                        candidate.EndsWith('/' + stem, StringComparison.OrdinalIgnoreCase);
        var exactQuery = file.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                         candidate.EndsWith('/' + query, StringComparison.OrdinalIgnoreCase);

        if (Regex.IsMatch(logicalPath,
                @"(?:countryflags/f_|miniflags/flag_|cardflags/|flags512x512/f_|countryshapes/c)\d+"))
        {
            var id = MatchId(logicalPath,
                @"(?:countryflags/f_|miniflags/flag_|cardflags/|flags512x512/f_|countryshapes/c)(\d+)");
            if (id < 0 || !HasBoundedId(candidate, id)) return false;
            var isShape = logicalPath.Contains("countryshapes", StringComparison.OrdinalIgnoreCase);
            // FC26 contains club banners whose RES names look exactly like a
            // numeric flag query (for example f_149). Country flags must come
            // from their canonical ChunkFileCollector aliases below; accepting
            // any fuzzy flag RES here can silently display another club/country.
            return isShape &&
                   (candidate.Contains("shape", StringComparison.OrdinalIgnoreCase) || exactStem);
        }

        // For every other family, require either an exact requested filename
        // or a family-specific term. Low-confidence arbitrary textures are
        // worse than an honest empty preview.
        if (exactStem || exactQuery) return true;
        if (logicalPath.Contains("/heads/", StringComparison.OrdinalIgnoreCase))
            return candidate.Contains("head", StringComparison.OrdinalIgnoreCase) ||
                   candidate.Contains("portrait", StringComparison.OrdinalIgnoreCase);
        if (logicalPath.Contains("/stadium", StringComparison.OrdinalIgnoreCase))
            return candidate.Contains("stadium", StringComparison.OrdinalIgnoreCase);
        if (logicalPath.Contains("/ball", StringComparison.OrdinalIgnoreCase))
            return candidate.Contains("ball", StringComparison.OrdinalIgnoreCase);
        if (logicalPath.Contains("/boot", StringComparison.OrdinalIgnoreCase) ||
            logicalPath.Contains("/shoe", StringComparison.OrdinalIgnoreCase))
            return candidate.Contains("boot", StringComparison.OrdinalIgnoreCase) ||
                   candidate.Contains("shoe", StringComparison.OrdinalIgnoreCase);
        if (logicalPath.Contains("/crest", StringComparison.OrdinalIgnoreCase))
            return candidate.Contains("crest", StringComparison.OrdinalIgnoreCase);
        return false;
    }

    private static bool HasBoundedId(string value, int id) =>
        Regex.IsMatch(value, $@"(?<!\d){id}(?!\d)", RegexOptions.IgnoreCase);

    private static IEnumerable<string> CollectorAliases(string logicalPath)
    {
        var normalized = logicalPath.Replace('\\', '/').TrimStart('/');
        yield return normalized;

        var countryId = MatchId(normalized,
            @"(?:flags512x512/f_|cardflags/|countryshapes/c)(\d+)");
        if (countryId >= 0)
        {
            if (normalized.Contains("flags512x512", StringComparison.OrdinalIgnoreCase))
            {
                yield return $"data/ui/imgAssets/flags512x512/light/f_{countryId}.dds";
                yield return $"data/ui/imgAssets/flags512x512/dark/f_{countryId}.dds";
            }
            yield return $"data/ui/artassets/countryflags/f_{countryId}.big";
            yield return $"data/ui/artassets/miniflags/flag_{countryId}.big";
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
