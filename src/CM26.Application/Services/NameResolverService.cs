using System.Globalization;

namespace CM26.Application.Services;

/// <summary>
/// Resolves foreign keys and coded values to human-readable names/labels using the
/// loaded database. Built once per load; UI consumes these instead of raw IDs.
/// </summary>
public sealed class NameResolverService
{
    private readonly DatabaseSession _session;
    private readonly IReadOnlyDictionary<int, (string First, string Surname, string Common)>? _playerNameOverrides;

    // id -> name lookups
    private readonly Dictionary<int, string> _nations = new();
    private readonly Dictionary<int, string> _leagues = new();
    private readonly Dictionary<int, int> _leagueNation = new();
    private readonly Dictionary<int, string> _stadiums = new();
    private readonly Dictionary<int, string> _playerNames = new();   // playernames.nameid -> name
    private readonly Dictionary<int, (string first, string last, string common)> _editedNames = new();
    private readonly Dictionary<int, int> _playerTeam = new();        // playerid -> teamid (teamplayerlinks)
    private readonly Dictionary<int, int> _playerJersey = new();      // playerid -> jerseynumber
    private readonly Dictionary<int, int> _teamLeague = new();        // teamid -> leagueid (leagueteamlinks)
    private readonly Dictionary<int, int> _teamNation = new();        // teamid -> nationid (teamnationlinks)
    private readonly Dictionary<int, int> _teamCountry = new();       // teamid -> teams.countryid fallback
    private readonly Dictionary<int, int> _teamStadium = new();       // teamid -> stadiumid (teamstadiumlinks)
    private readonly Dictionary<int, string> _teamNames = new();      // teamid -> teamname (teams.teamname literal)
    private readonly Dictionary<int, string> _managerNames = new();   // managerid -> display name
    private readonly Dictionary<int, int> _teamManager = new();       // teamid -> managerid fallback
    // playerid -> (firstnameid, lastnameid, commonnameid), to resolve any player reference (captain, set-pieces)
    private readonly Dictionary<int, (int first, int last, int common)> _playerNameIds = new();

    private PlayerNameService? _playerNameService;

    public NameResolverService(DatabaseSession session,
        IReadOnlyDictionary<int, (string First, string Surname, string Common)>? playerNameOverrides = null)
    {
        _session = session;
        _playerNameOverrides = playerNameOverrides;
        Rebuild();
    }

    /// <summary>The read-only, database-native player-name resolver (cached, single build per session).</summary>
    public PlayerNameService PlayerNames => _playerNameService ??= new PlayerNameService(
        new DatabasePlayerNameSource(_session, _playerNameOverrides));

    public void Rebuild()
    {
        _nations.Clear(); _leagues.Clear(); _leagueNation.Clear(); _stadiums.Clear(); _playerNames.Clear();
        _editedNames.Clear(); _playerTeam.Clear(); _playerJersey.Clear();
        _teamLeague.Clear(); _teamNation.Clear(); _teamCountry.Clear(); _teamStadium.Clear(); _teamNames.Clear();
        _managerNames.Clear(); _teamManager.Clear();
        _playerNameIds.Clear();
        _playerNameService = null;

        LoadNations();
        LoadLeagues();
        LoadStadiums();
        LoadPlayerNames();
        LoadEditedNames();
        LoadPlayerNameOverrides();
        LoadTeams();
        LoadManagers();
        LoadLinks();
        LoadPlayerNameIds();
    }

    private void LoadPlayerNameIds()
    {
        var t = _session.GetTable("players"); if (t == null) return;
        int id = Col(t, "playerid"), fn = Col(t, "firstnameid"), ln = Col(t, "lastnameid"), cn = Col(t, "commonnameid");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("players", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(id), out var pid))
                _playerNameIds[pid] = (ParseInt(rec.Get(fn)), ParseInt(rec.Get(ln)), ParseInt(rec.Get(cn)));
        }
    }

    private static int ParseInt(string s) => int.TryParse(s, out var v) ? v : 0;

    private void LoadNations()
    {
        var t = _session.GetTable("nations"); if (t == null) return;
        int id = Col(t, "nationid"), name = Col(t, "nationname");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("nations", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(id), out var nid)) _nations[nid] = rec.Get(name);
        }
    }

    private void LoadLeagues()
    {
        var t = _session.GetTable("leagues"); if (t == null) return;
        int id = Col(t, "leagueid"), name = Col(t, "leaguename"), country = Col(t, "countryid");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("leagues", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(id), out var lid))
            {
                _leagues[lid] = rec.Get(name);
                var nationId = ParseInt(rec.Get(country));
                if (nationId > 0) _leagueNation[lid] = nationId;
            }
        }
    }

    private void LoadStadiums()
    {
        var t = _session.GetTable("stadiums"); if (t == null) return;
        int id = Col(t, "stadiumid"), name = Col(t, "name");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("stadiums", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(id), out var sid)) _stadiums[sid] = rec.Get(name);
        }
    }

    private void LoadPlayerNames()
    {
        var t = _session.GetTable("playernames"); if (t == null) return;
        int id = Col(t, "nameid"), name = Col(t, "name");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("playernames", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(id), out var nid)) _playerNames[nid] = rec.Get(name);
        }
    }

    private void LoadEditedNames()
    {
        var t = _session.GetTable("editedplayernames"); if (t == null) return;
        int pid = Col(t, "playerid"), fn = Col(t, "firstname"), sn = Col(t, "surname"), cn = Col(t, "commonname");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("editedplayernames", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(pid), out var id)) _editedNames[id] = (rec.Get(fn), rec.Get(sn), rec.Get(cn));
        }
    }

    private void LoadPlayerNameOverrides()
    {
        if (_playerNameOverrides == null) return;
        foreach (var (playerId, name) in _playerNameOverrides)
        {
            if (playerId <= 0) continue;
            _editedNames[playerId] = (name.First, name.Surname, name.Common);
        }
    }

    private void LoadTeams()
    {
        var t = _session.GetTable("teams"); if (t == null) return;
        int id = Col(t, "teamid"), name = Col(t, "teamname"), country = Col(t, "countryid");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("teams", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(id), out var tid))
            {
                _teamNames[tid] = rec.Get(name);
                var nationId = ParseInt(rec.Get(country));
                if (nationId > 0) _teamCountry[tid] = nationId;
            }
        }
    }

    private void LoadManagers()
    {
        var t = _session.GetTable("manager"); if (t == null) return;
        int id = Col(t, "managerid"), first = Col(t, "firstname"), last = Col(t, "surname"), team = Col(t, "teamid");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("manager", r); if (rec == null) continue;
            if (!int.TryParse(rec.Get(id), out var managerId)) continue;
            var name = $"{rec.Get(first)} {rec.Get(last)}".Trim();
            if (!string.IsNullOrWhiteSpace(name)) _managerNames[managerId] = name;
            var teamId = ParseInt(rec.Get(team));
            if (teamId > 0) _teamManager[teamId] = managerId;
        }
    }

    private void LoadLinks()
    {
        var tpl = _session.GetTable("teamplayerlinks");
        if (tpl != null)
        {
            int pid = Col(tpl, "playerid"), tid = Col(tpl, "teamid"), jersey = Col(tpl, "jerseynumber");
            for (int r = 0; r < tpl.RowCount; r++)
            {
                var rec = _session.GetRecord("teamplayerlinks", r); if (rec == null) continue;
                if (int.TryParse(rec.Get(pid), out var p) && int.TryParse(rec.Get(tid), out var tm))
                {
                    _playerTeam[p] = tm;
                    if (int.TryParse(rec.Get(jersey), out var j)) _playerJersey[p] = j;
                }
            }
        }
        var ltl = _session.GetTable("leagueteamlinks");
        if (ltl != null)
        {
            int tid = Col(ltl, "teamid"), lid = Col(ltl, "leagueid");
            for (int r = 0; r < ltl.RowCount; r++)
            {
                var rec = _session.GetRecord("leagueteamlinks", r); if (rec == null) continue;
                if (int.TryParse(rec.Get(tid), out var tm) && int.TryParse(rec.Get(lid), out var lg))
                    _teamLeague[tm] = lg;
            }
        }
        var tnl = _session.GetTable("teamnationlinks");
        if (tnl != null)
        {
            int tid = Col(tnl, "teamid"), nid = Col(tnl, "nationid");
            for (int r = 0; r < tnl.RowCount; r++)
            {
                var rec = _session.GetRecord("teamnationlinks", r); if (rec == null) continue;
                if (int.TryParse(rec.Get(tid), out var tm) && int.TryParse(rec.Get(nid), out var nt))
                    _teamNation[tm] = nt;
            }
        }
        var tsl = _session.GetTable("teamstadiumlinks");
        if (tsl != null)
        {
            int tid = Col(tsl, "teamid"), sid = Col(tsl, "stadiumid");
            for (int r = 0; r < tsl.RowCount; r++)
            {
                var rec = _session.GetRecord("teamstadiumlinks", r); if (rec == null) continue;
                if (int.TryParse(rec.Get(tid), out var tm) && int.TryParse(rec.Get(sid), out var st))
                    _teamStadium[tm] = st;
            }
        }
    }

    private static int Col(Models.DbTable t, string name)
    {
        for (int i = 0; i < t.Columns.Count; i++)
            if (t.Columns[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    // ---- public resolvers (never return raw IDs when a name exists) ----

    public string NationName(int id) => _nations.TryGetValue(id, out var n) && !string.IsNullOrWhiteSpace(n) ? n : $"Nation {id}";
    public string LeagueName(int id) => _leagues.TryGetValue(id, out var n) && !string.IsNullOrWhiteSpace(n) ? n : $"League {id}";
    public string StadiumName(int id) => _stadiums.TryGetValue(id, out var n) && !string.IsNullOrWhiteSpace(n) ? n : (id <= 0 ? "—" : $"Stadium {id}");
    public string TeamName(int id) => _teamNames.TryGetValue(id, out var n) && !string.IsNullOrWhiteSpace(n) ? n : $"Team {id}";

    public string TeamLeagueName(int teamId) => _teamLeague.TryGetValue(teamId, out var lid) ? LeagueName(lid) : "—";
    public string TeamNationName(int teamId)
    {
        if (_teamNation.TryGetValue(teamId, out var linkedNation)) return NationName(linkedNation);
        if (_teamCountry.TryGetValue(teamId, out var directNation)) return NationName(directNation);
        if (_teamLeague.TryGetValue(teamId, out var leagueId) && _leagueNation.TryGetValue(leagueId, out var leagueNation))
            return NationName(leagueNation);
        return "—";
    }
    public string TeamStadiumName(int teamId) => _teamStadium.TryGetValue(teamId, out var sid) ? StadiumName(sid) : "—";
    public int? TeamStadiumId(int teamId) => _teamStadium.TryGetValue(teamId, out var sid) ? sid : null;
    public int? TeamLeagueId(int teamId) => _teamLeague.TryGetValue(teamId, out var lid) ? lid : null;
    public int? TeamNationId(int teamId) => _teamNation.TryGetValue(teamId, out var nid) ? nid : null;
    public string TeamManagerName(int teamId) =>
        _teamManager.TryGetValue(teamId, out var managerId) && _managerNames.TryGetValue(managerId, out var name)
            ? name : "—";

    /// <summary>
    /// Resolve a player's display name via the read-only PlayerNameService (known-as → common →
    /// first+last → honest "Player {id}" fallback). Never a raw key, never fabricated.
    /// </summary>
    public string PlayerDisplayName(int playerId, int firstNameId, int lastNameId, int commonNameId) =>
        PlayerNames.DisplayName(playerId, firstNameId, lastNameId, commonNameId);

    /// <summary>Full four-part resolution for the editor (first/last/common/known-as).</summary>
    public PlayerNameParts PlayerNameParts(int playerId, int firstNameId, int lastNameId, int commonNameId) =>
        PlayerNames.Resolve(playerId, firstNameId, lastNameId, commonNameId);

    public string NameById(int nameId) => _playerNames.TryGetValue(nameId, out var n) ? n : string.Empty;

    /// <summary>
    /// Resolve any player reference (captain, corner/penalty/free-kick taker) to a display name.
    /// Looks up the player's name IDs from the players table, then resolves them. Falls back to
    /// "Player {id}" — never shows a raw key as though it were a name.
    /// </summary>
    public string PlayerNameByPlayerId(int playerId)
    {
        if (playerId <= 0) return "—";
        if (_playerNameIds.TryGetValue(playerId, out var ids))
            return PlayerNames.DisplayName(playerId, ids.first, ids.last, ids.common);
        return $"Player {playerId}";
    }

    public int? PlayerTeamId(int playerId) => _playerTeam.TryGetValue(playerId, out var t) ? t : null;
    public string PlayerClubName(int playerId) => _playerTeam.TryGetValue(playerId, out var t) ? TeamName(t) : "Free Agent";
    public int? PlayerJersey(int playerId) => _playerJersey.TryGetValue(playerId, out var j) ? j : null;
    public string PlayerLeagueName(int playerId) => _playerTeam.TryGetValue(playerId, out var t) ? TeamLeagueName(t) : "—";

    // ---- label maps for coded values ----

    private static readonly Dictionary<int, string> Positions = new()
    {
        [0]="GK",[1]="SW",[2]="RWB",[3]="RB",[4]="RCB",[5]="CB",[6]="LCB",[7]="LB",[8]="LWB",
        [9]="RDM",[10]="CDM",[11]="LDM",[12]="RM",[13]="RCM",[14]="CM",[15]="LCM",[16]="LM",
        [17]="RAM",[18]="CAM",[19]="LAM",[20]="RF",[21]="CF",[22]="LF",[23]="RW",[24]="RS",
        [25]="ST",[26]="LS",[27]="LW"
    };
    public static string PositionLabel(int code) => Positions.TryGetValue(code, out var p) ? p : (code < 0 ? "—" : $"POS {code}");

    private static readonly Dictionary<int, string> WorkRates = new() { [0]="Low",[1]="Medium",[2]="High" };
    public static string WorkRateLabel(int code) => WorkRates.TryGetValue(code, out var w) ? w : $"WR {code}";

    public static string PreferredFootLabel(int code) => code switch { 1 => "Right", 2 => "Left", _ => $"Foot {code}" };

    public static string KitTypeLabel(int code) => Fc26KitSlot.Label(code);

    // FC26 confederation codes, derived from the actual nations table in this database:
    //   1 = special (Gibraltar, Greenland, International, Rest of World, created/free-agents)
    //   2 = UEFA (England, France, Germany, Spain, Italy, …)
    //   3 = CAF (Algeria, Egypt, Ghana, Morocco, Nigeria, South Africa, …)
    //   4 = CONMEBOL (Argentina, Brazil, Chile, Colombia, the CONMEBOL row itself, …)
    //   5 = AFC (Afghanistan, Australia, Japan, Qatar, Korea Republic, India, …)
    //   6 = OFC (Fiji, New Zealand, Papua New Guinea, Samoa, …)
    //   7 = CONCACAF (Canada, Mexico, United States, Costa Rica, Jamaica, …)
    public string ConfederationLabel(int code) => code switch
    {
        2 => "UEFA", 3 => "CAF", 4 => "CONMEBOL", 5 => "AFC", 6 => "OFC", 7 => "CONCACAF",
        1 => "—", _ => $"Conf {code}",
    };
}
