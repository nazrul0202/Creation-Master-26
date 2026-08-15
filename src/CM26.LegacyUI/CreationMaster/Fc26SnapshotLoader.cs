using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FifaLibrary;

namespace CreationMaster;

internal static class Fc26SnapshotLoader
{
    internal static void Load(string path)
    {
        using var stream = File.OpenRead(path);
        var snapshot = JsonSerializer.Deserialize<Snapshot>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("FC26 snapshot is empty.");
        var tables = snapshot.Tables.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var countries = Build<CountryList, Country>(tables, "nations", "nationid");
        var leagues = Build<LeagueList, League>(tables, "leagues", "leagueid");
        var teams = Build<TeamList, Team>(tables, "teams", "teamid");
        var players = Build<PlayerList, Player>(tables, "players", "playerid");
        var stadiums = Build<StadiumList, Stadium>(tables, "stadiums", "stadiumid");
        var kits = Build<KitList, Kit>(tables, tables.ContainsKey("teamkits") ? "teamkits" : "kits", "kitid");
        var roles = BuildRoles(tables);
        FifaEnvironment.BeginFc26Bridge(roles);
        var formations = Build<FormationList, Formation>(tables, "formations", "formationid");
        ApplyFormationRoles(formations, tables, roles);
        var referees = Build<RefereeList, Referee>(tables, "referee", "refereeid");
        var balls = Build<BallList, Ball>(tables, "teamballs", "ballid");
        var shoes = Build<ShoesList, Shoes>(tables, "playerboots", "shoetype");
        var gloves = new GkGlovesList();
        var competitions = BuildCompetitions(tables);

        ApplyPlayerNames(players, tables);
        FifaEnvironment.InitializeFc26Bridge(snapshot.GameRoot, snapshot.DatabaseFolder,
            countries, leagues, teams, players, stadiums, kits, formations, roles,
            referees, balls, shoes, gloves, competitions);
        LinkCore(tables, countries, leagues, teams, players, stadiums, kits, formations);
        LinkReferees(tables, referees, countries, leagues);
    }

    private static RoleList BuildRoles(Dictionary<string, TableSnapshot> tables)
    {
        var roles = new RoleList();
        for (var id = 0; id < 30; id++) roles.Add(new Role(id));
        roles.MinId = 0;
        roles.MaxId = 29;
        if (!tables.TryGetValue("fieldpositionboundingboxes", out var table)) return roles;

        var idColumn = Column(table, "positionid");
        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            var role = roles.SearchId(ParseIntAt(row, idColumn)) as Role;
            if (role == null) continue;
            var xs = new[] { ValueAt(table, row, "pointx0"), ValueAt(table, row, "pointx1"),
                ValueAt(table, row, "pointx2"), ValueAt(table, row, "pointx3") };
            var ys = new[] { ValueAt(table, row, "pointy0"), ValueAt(table, row, "pointy1"),
                ValueAt(table, row, "pointy2"), ValueAt(table, row, "pointy3") };
            role.Xmin = (int)(xs.Min() * 100f);
            role.Xmax = (int)(xs.Max() * 100f);
            role.Ymin = (int)(ys.Min() * 100f);
            role.Ymax = (int)(ys.Max() * 100f);
        }
        return roles;
    }

    private static void ApplyFormationRoles(FormationList formations,
        Dictionary<string, TableSnapshot> tables, RoleList roles)
    {
        if (!tables.TryGetValue("formations", out var table)) return;
        var idColumn = Column(table, "formationid");
        foreach (var row in table.Rows)
        {
            var formation = formations.SearchId(ParseIntAt(row, idColumn)) as Formation;
            if (formation == null) continue;
            var playingRoles = new PlayingRole[11];
            for (var index = 0; index < playingRoles.Length; index++)
            {
                var roleId = ParseIntAt(row, Column(table, "position" + index));
                var role = roles.SearchId(roleId) as Role ?? roles[0] as Role;
                if (role == null) continue;
                var playingRole = new PlayingRole(role)
                {
                    OffsetX = (int)(ValueAt(table, row, "offset" + index + "x") * 100f),
                    OffsetY = (int)(ValueAt(table, row, "offset" + index + "y") * 100f)
                };
                var instruction1 = Column(table, "playerinstruction" + index + "_1");
                var instruction2 = Column(table, "playerinstruction" + index + "_2");
                if (instruction1 >= 0) playingRole.PlayerInstruction_1 = ParseIntAt(row, instruction1);
                if (instruction2 >= 0) playingRole.PlayerInstruction_2 = ParseIntAt(row, instruction2);
                playingRoles[index] = playingRole;
            }
            formation.PlayingRoles = playingRoles;
        }
        formations.LinkRoles(roles);
    }

    private static CompobjList BuildCompetitions(Dictionary<string, TableSnapshot> tables)
    {
        var result = new CompobjList();
        var world = new World(0, "FC26", "FC26 Competitions");
        result.Add(world);
        if (!tables.TryGetValue("competition", out var table)) return result;
        var idColumn = Column(table, "competitionid");
        var ballColumn = Column(table, "ballid");
        foreach (var row in table.Rows)
        {
            var competitionId = ParseIntAt(row, idColumn);
            var trophy = new Trophy(competitionId + 1, "Competition " + competitionId,
                "Competition " + competitionId, world)
            {
                LongName = "Competition " + competitionId,
                ShortName = "Competition " + competitionId,
                ballid = ParseIntAt(row, ballColumn)
            };
            trophy.Settings.m_asset_id = competitionId;
            world.AddChild(trophy);
            result.Add(trophy);
        }
        result.SortId();
        return result;
    }

    private static void LinkReferees(Dictionary<string, TableSnapshot> tables, RefereeList referees,
        CountryList countries, LeagueList leagues)
    {
        if (tables.TryGetValue("leaguerefereelinks", out var links))
        {
            var refereeId = Column(links, "refereeid");
            var leagueId = Column(links, "leagueid");
            foreach (var row in links.Rows)
                (referees.SearchId(ParseIntAt(row, refereeId)) as Referee)?.SetLeague(ParseIntAt(row, leagueId));
        }
        referees.LinkCountry(countries);
        referees.LinkLeague(leagues);
    }

    private static TList Build<TList, TItem>(Dictionary<string, TableSnapshot> tables,
        string tableName, string idColumn)
        where TList : System.Collections.IList, new()
    {
        var list = new TList();
        if (!tables.TryGetValue(tableName, out var table)) return list;
        var idIndex = Column(table, idColumn);
        foreach (var row in table.Rows)
        {
            if (idIndex < 0 || idIndex >= row.Length || !int.TryParse(row[idIndex], out var id)) continue;
            var item = Activator.CreateInstance(typeof(TItem), id);
            if (item == null) continue;
            MapFields(item, table.Columns, row);
            list.Add(item);
        }
        if (list is IdArrayList ids) ids.SortId();
        return list;
    }

    private static void MapFields(object target, string[] columns, string[] values)
    {
        var fields = AllFields(target.GetType())
            .GroupBy(f => Normalize(f.Name)).ToDictionary(g => g.Key, g => g.First());
        for (var i = 0; i < columns.Length && i < values.Length; i++)
        {
            if (!fields.TryGetValue(Normalize(columns[i]), out var field)) continue;
            try { field.SetValue(target, ConvertValue(values[i], field.FieldType)); }
            catch { /* New FC26-only columns stay in the snapshot and are ignored by CM16 forms. */ }
        }
        if (target is Country country)
        {
            country.LanguageName = country.DatabaseName;
            country.LanguageShortName = country.DatabaseName;
        }
    }

    private static IEnumerable<FieldInfo> AllFields(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
            foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                yield return field;
    }

    private static object? ConvertValue(string value, Type type)
    {
        var inner = Nullable.GetUnderlyingType(type) ?? type;
        if (inner == typeof(string)) return value;
        if (inner == typeof(bool)) return value == "1" || bool.TryParse(value, out var b) && b;
        if (inner.IsEnum) return Enum.ToObject(inner, ParseInt(value));
        if (inner == typeof(int)) return ParseInt(value);
        if (inner == typeof(short)) return (short)ParseInt(value);
        if (inner == typeof(byte)) return (byte)ParseInt(value);
        if (inner == typeof(long)) return long.TryParse(value, out var l) ? l : 0L;
        if (inner == typeof(float)) return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f;
        if (inner == typeof(double)) return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0d;
        return null;
    }

    private static void ApplyPlayerNames(PlayerList players, Dictionary<string, TableSnapshot> tables)
    {
        if (!tables.TryGetValue("playernames", out var names)) return;
        var id = Column(names, "nameid"); var name = Column(names, "name");
        var lookup = new Dictionary<int, string>();
        foreach (var row in names.Rows)
            if (id >= 0 && name >= 0 && int.TryParse(row[id], out var key)) lookup[key] = row[name];

        foreach (Player player in players)
        {
            var fields = AllFields(typeof(Player)).ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
            SetResolved(fields, player, "m_firstnameid", "m_firstname", lookup);
            SetResolved(fields, player, "m_lastnameid", "m_lastname", lookup);
            SetResolved(fields, player, "m_commonnameid", "m_commonname", lookup);
        }
    }

    private static void SetResolved(Dictionary<string, FieldInfo> fields, Player player,
        string idField, string textField, Dictionary<int, string> names)
    {
        if (!fields.TryGetValue(idField, out var source) || !fields.TryGetValue(textField, out var target)) return;
        var id = (int)(source.GetValue(player) ?? 0);
        target.SetValue(player, names.TryGetValue(id, out var value) ? value : string.Empty);
    }

    private static void LinkCore(Dictionary<string, TableSnapshot> tables, CountryList countries,
        LeagueList leagues, TeamList teams, PlayerList players, StadiumList stadiums,
        KitList kits, FormationList formations)
    {
        leagues.LinkCountry(countries);
        teams.LinkCountry(countries);
        countries.LinkTeam(teams);
        players.LinkCountry(countries);
        teams.LinkKits(kits);
        kits.LinkTeam(teams);
        teams.LinkStadiums(stadiums);
        stadiums.LinkTeam(teams);
        stadiums.LinkCountry(countries);
        teams.LinkFormation(formations);
        formations.LinkTeam(teams);

        if (tables.TryGetValue("leagueteamlinks", out var leagueLinks))
        {
            var leagueId = Column(leagueLinks, "leagueid"); var teamId = Column(leagueLinks, "teamid");
            foreach (var row in leagueLinks.Rows)
            {
                var league = leagues.SearchLeague(ParseIntAt(row, leagueId));
                var team = teams.SearchId(ParseIntAt(row, teamId)) as Team;
                if (league != null && team != null) { league.LinkTeam(team); team.League = league; }
            }
        }
        if (tables.TryGetValue("teamplayerlinks", out var playerLinks))
        {
            var teamId = Column(playerLinks, "teamid"); var playerId = Column(playerLinks, "playerid");
            var jersey = Column(playerLinks, "jerseynumber");
            foreach (var row in playerLinks.Rows)
            {
                var team = teams.SearchId(ParseIntAt(row, teamId)) as Team;
                var player = players.SearchId(ParseIntAt(row, playerId)) as Player;
                if (team != null && player != null) team.AddTeamPlayer(player, ParseIntAt(row, jersey));
            }
        }
        players.LinkTeam(teams);
        teams.LinkPlayer(players);
        teams.LinkLeague(leagues);
        teams.LinkOpponent(teams);
    }

    private static int Column(TableSnapshot table, string name) =>
        Array.FindIndex(table.Columns, c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
    private static int ParseInt(string value) => int.TryParse(value, out var result) ? result : 0;
    private static int ParseIntAt(string[] row, int index) => index >= 0 && index < row.Length ? ParseInt(row[index]) : 0;
    private static float ValueAt(TableSnapshot table, string[] row, string column)
    {
        var index = Column(table, column);
        return index >= 0 && index < row.Length && float.TryParse(row[index], NumberStyles.Float,
            CultureInfo.InvariantCulture, out var value) ? value : 0f;
    }
    private static string Normalize(string value) => new string(value.TrimStart('m', 'M', '_')
        .Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private sealed class Snapshot
    {
        public string GameRoot { get; set; } = string.Empty;
        public string DatabaseFolder { get; set; } = string.Empty;
        public List<TableSnapshot> Tables { get; set; } = new();
    }
    private sealed class TableSnapshot
    {
        public string Name { get; set; } = string.Empty;
        public string[] Columns { get; set; } = Array.Empty<string>();
        public List<string[]> Rows { get; set; } = new();
    }
}
