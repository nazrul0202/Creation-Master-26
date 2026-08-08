namespace CM26.Application.Services;

/// <summary>
/// Resolves FC26 database asset references to real files on local disk. This is a READ-ONLY
/// locator: it never assumes an asset exists merely because the DB has an ID — it checks the
/// filesystem and returns null when no real file is present. Callers then show an honest
/// "unavailable" state. The asset root is configurable (no hard-coded development paths).
/// </summary>
public sealed class AssetCatalogService
{
    private readonly string _assetRoot;

    // Relative sub-folder layouts (under the configurable asset root) discovered during the
    // asset inventory. Each candidate is probed; the first real file that exists wins.
    // {0} is replaced by the numeric asset id. Multiple patterns allow "p{id}" and "{id}" styles.
    private static readonly string[] MinifacePatterns =
    {
        Path.Combine("miniface", "**", "p{0}.png"),
        Path.Combine("miniface", "**", "p{0}.dds"),
    };
    private static readonly string[] BallPatterns =
    {
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "26", "ballid", "{0}.png"),
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "ballid", "{0}.png"),
    };
    private static readonly string[] StadiumPatterns =
    {
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "stadiumid", "{0}.png"),
    };
    private static readonly string[] BootPatterns =
    {
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "26", "shoetypecode", "{0}.png"),
    };
    private static readonly string[] GlovePatterns =
    {
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "26", "gkglovetypecode", "{0}.png"),
    };
    private static readonly string[] FlagPatterns =
    {
        Path.Combine("FC Editor by decoruiz Alpha v21", "FC Editor by decoruiz Alpha v21.exe_extracted", "art", "flags", "{0}.png"),
        Path.Combine("country_logos", "{0}.png"),
        Path.Combine("nation_logos", "{0}.png"),
        Path.Combine("flags", "{0}.png"),
    };
    private static readonly string[] TeamLogoPatterns =
    {
        // FC Editor by decoruiz keeps its menu crests in a separate dark
        // collection. Prefer these paths before the installed FC26 archive so
        // previews never accidentally pick the light/white variant.
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "26", "crest", "dark", "{0}.png"),
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "crest", "dark", "{0}.png"),
        Path.Combine("assets", "26", "crest", "dark", "{0}.png"),
        Path.Combine("assets", "crest", "dark", "{0}.png"),
        Path.Combine("26", "crest", "dark", "{0}.png"),
        Path.Combine("crest", "dark", "{0}.png"),
        Path.Combine("data", "ui", "imgAssets", "crest", "dark", "l{0}.dds"),
        Path.Combine("legacy", "data", "ui", "imgAssets", "crest", "dark", "l{0}.dds"),
        Path.Combine("imgAssets", "crest", "dark", "l{0}.dds"),
        Path.Combine("team_logos", "{0}.png"),
        Path.Combine("team_logos", "team_{0}.png"),
        Path.Combine("crests", "{0}.png"),
        Path.Combine("crests", "crest_{0}.png"),
        Path.Combine("badges", "{0}.png"),
    };
    private static readonly string[] LeagueLogoPatterns =
    {
        Path.Combine("league_logos", "{0}.png"),
        Path.Combine("leagues", "{0}.png"),
        Path.Combine("logos", "leagues", "{0}.png"),
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "26", "leagueid", "{0}.png"),
    };
    private static readonly string[] CompetitionLogoPatterns =
    {
        Path.Combine("competition_logos", "{0}.png"),
        Path.Combine("competitions", "{0}.png"),
        Path.Combine("logos", "competitions", "{0}.png"),
        Path.Combine("FC Editor by decoruiz Alpha v21", "assets", "26", "competitionid", "{0}.png"),
    };
    private static readonly string[] ManagerFacePatterns =
    {
        Path.Combine("managerfaces", "m{0}.png"),
        Path.Combine("managerfaces", "{0}.png"),
        Path.Combine("manager_faces", "m{0}.png"),
        Path.Combine("manager_faces", "{0}.png"),
        Path.Combine("miniface", "m{0}.png"),
        Path.Combine("miniface", "m{0}.dds"),
    };

    // A light-weight per-session cache of the miniface folder tree (id -> full path) so we do
    // not re-walk the directory for every player. Built lazily once.
    private Dictionary<int, string>? _minifaceIndex;
    private readonly object _minifaceGate = new();

    public AssetCatalogService(string assetRoot)
    {
        _assetRoot = assetRoot?.Trim() ?? string.Empty;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_assetRoot) && Directory.Exists(_assetRoot);

    private string Resolve(string[] patterns, int id)
    {
        if (!IsConfigured) return string.Empty;
        foreach (var pattern in patterns)
        {
            // '**' means "search subfolders" — only used by minifaces via the index.
            if (pattern.Contains("**")) continue;
            var path = Path.Combine(_assetRoot, string.Format(pattern, id));
            if (File.Exists(path)) return path;
        }
        return string.Empty;
    }

    /// <summary>Miniface for a player. PNG preferred (already decoded), DDS fallback.</summary>
    public string GetPlayerMiniface(int playerId)
    {
        if (!IsConfigured) return string.Empty;
        EnsureMinifaceIndex();
        return _minifaceIndex != null && _minifaceIndex.TryGetValue(playerId, out var p) ? p : string.Empty;
    }

    public string GetBall(int ballId) => Resolve(BallPatterns, ballId);
    public string GetStadium(int stadiumId) => Resolve(StadiumPatterns, stadiumId);
    public string GetBoot(int shoeType) => Resolve(BootPatterns, shoeType);
    public string GetGlove(int gloveCode) => Resolve(GlovePatterns, gloveCode);
    public string GetFlag(int nationId) => Resolve(FlagPatterns, nationId);
    public string GetTeamLogo(int teamId) => Resolve(TeamLogoPatterns, teamId);
    public string GetLeagueLogo(int leagueId) => Resolve(LeagueLogoPatterns, leagueId);
    public string GetCompetitionLogo(int competitionId) => Resolve(CompetitionLogoPatterns, competitionId);
    public string GetManagerFace(int managerId) => Resolve(ManagerFacePatterns, managerId);

    private void EnsureMinifaceIndex()
    {
        if (_minifaceIndex != null) return;
        lock (_minifaceGate)
        {
            if (_minifaceIndex != null) return;
            var index = new Dictionary<int, string>();
            var baseDir = Path.Combine(_assetRoot, "miniface");
            if (Directory.Exists(baseDir))
            {
                // PNG first (preferred), then DDS as fallback (only if no PNG for that id).
                foreach (var file in Directory.EnumerateFiles(baseDir, "p*.png", SearchOption.AllDirectories))
                    AddMiniface(index, file);
                foreach (var file in Directory.EnumerateFiles(baseDir, "p*.dds", SearchOption.AllDirectories))
                    AddMiniface(index, file);
            }
            _minifaceIndex = index;
        }
    }

    private static void AddMiniface(Dictionary<int, string> index, string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        if (name.Length > 1 && name[0] == 'p' && int.TryParse(name[1..], out var id))
        {
            // First one wins (PNG enumerated before DDS).
            if (!index.ContainsKey(id)) index[id] = file;
        }
    }

    /// <summary>Count of miniface files currently indexed (for diagnostics / honest reporting).</summary>
    public int MinifaceCount
    {
        get { EnsureMinifaceIndex(); return _minifaceIndex?.Count ?? 0; }
    }
}
