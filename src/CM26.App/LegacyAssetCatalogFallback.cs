using System.Text.RegularExpressions;
using CM26.Application.Services;

namespace CM26.App;

/// <summary>Maps original CM16 logical filenames to an optional extracted FC26 asset pack.</summary>
internal static class LegacyAssetCatalogFallback
{
    internal static string Resolve(string assetRoot, string logicalPath)
    {
        var catalog = new AssetCatalogService(assetRoot);
        if (!catalog.IsConfigured) return string.Empty;
        var path = logicalPath.Replace('\\', '/').ToLowerInvariant();

        var id = MatchId(path, @"(?:countryflags/f_|miniflags/flag_|cardflags/|flags512x512/f_|countryshapes/c)(\d+)");
        if (id >= 0) return catalog.GetFlag(id);
        id = MatchId(path, @"/crest(?:16x16|32x32|50x50)?/(?:light|dark)/l(\d+)");
        if (id >= 0) return catalog.GetTeamLogo(id);
        id = MatchId(path, @"(?:settingsimg/ball_|/ball/ball_)(\d+)");
        if (id >= 0) return catalog.GetBall(id);
        id = MatchId(path, @"/shoe/shoe_(\d+)");
        if (id >= 0) return catalog.GetBoot(id);
        id = MatchId(path, @"(?:stadium_|stadiumsbig/st_)(\d+)");
        if (id >= 0) return catalog.GetStadium(id);
        id = MatchId(path, @"/heads/p(\d+)");
        if (id >= 0) return catalog.GetPlayerMiniface(id);
        return string.Empty;
    }

    private static int MatchId(string value, string pattern)
    {
        var match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : -1;
    }
}
