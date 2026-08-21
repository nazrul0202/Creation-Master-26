using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FifaLibrary;

namespace CreationMaster;

internal static class Fc26SnapshotLoader
{
    private static Snapshot? s_snapshot;
    private static readonly Dictionary<object, List<RowOrigin>> s_rowOrigins =
        new Dictionary<object, List<RowOrigin>>(ReferenceComparer.Instance);
    /// <summary>nameid -> resolved display name recorded at snapshot load. Used to
    /// detect genuine user edits; decode artifacts must never be staged as edits.</summary>
    private static readonly Dictionary<int, string> s_loadedPlayerNames =
        new Dictionary<int, string>();

    internal static void Load(string path)
    {
        using var stream = File.OpenRead(path);
        var snapshot = JsonSerializer.Deserialize<Snapshot>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("FC26 snapshot is empty.");
        var tables = snapshot.Tables.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
        s_snapshot = snapshot;
        s_rowOrigins.Clear();
        s_loadedPlayerNames.Clear();

        var countries = Build<CountryList, Country>(tables, "nations", "nationid");
        var leagues = Build<LeagueList, League>(tables, "leagues", "leagueid");
        var teams = Build<TeamList, Team>(tables, "teams", "teamid");
        var players = Build<PlayerList, Player>(tables, "players", "playerid");
        var stadiums = Build<StadiumList, Stadium>(tables, "stadiums", "stadiumid");
        var kitTable = tables.ContainsKey("teamkits") ? "teamkits" : "kits";
        var kitIdColumn = kitTable == "teamkits" ? "teamkitid" : "kitid";
        var kits = Build<KitList, Kit>(tables, kitTable, kitIdColumn);
        var roles = BuildRoles(tables);
        FifaEnvironment.BeginFc26Bridge(roles);
        var formations = Build<FormationList, Formation>(tables, "formations", "formationid");
        ApplyFormationRoles(formations, tables, roles);
        var referees = Build<RefereeList, Referee>(tables, "referee", "refereeid");
        var balls = Build<BallList, Ball>(tables, "teamballs", "ballid");
        var shoes = Build<ShoesList, Shoes>(tables, "playerboots", "shoetype");
        var gloves = new GkGlovesList();
        var competitions = BuildCompetitions(tables);

        ApplyLeagueNames(leagues);
        ApplyTeamNames(teams);
        ApplyPlayerNames(players, tables);
        FifaEnvironment.InitializeFc26Bridge(snapshot.GameRoot, snapshot.DatabaseFolder,
            countries, leagues, teams, players, stadiums, kits, formations, roles,
            referees, balls, shoes, gloves, competitions);
        LinkCore(tables, countries, leagues, teams, players, stadiums, kits, formations);
        ApplyManagerNames(teams, tables);
        LinkReferees(tables, referees, countries, leagues);
    }

    private static void ApplyLeagueNames(LeagueList leagues)
    {
        // CM16 normally gets these display strings from its FIFA 16 language
        // database. FC26 exposes the current league name directly in the
        // Frostbite database, so use that authoritative value instead of the
        // constructor placeholders "Short League Name"/"Long League Name".
        foreach (League league in leagues)
        {
            var databaseName = league.leaguename?.Trim();
            if (string.IsNullOrWhiteSpace(databaseName)) continue;
            league.ShortName = databaseName;
            league.LongName = databaseName;
        }
    }

    private static void ApplyTeamNames(TeamList teams)
    {
        // Team(int) initializes the CM16 language fields with "Team <id>".
        // FC26 stores the current display name directly in teams.teamname, so
        // replace every constructor placeholder before the forms are opened.
        foreach (Team team in teams)
        {
            var name = team.DatabaseName?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            team.TeamNameFull = name;
            team.TeamNameAbbr15 = Abbreviate(name, 15);
            team.TeamNameAbbr10 = Abbreviate(name, 10);
            team.TeamNameAbbr7 = Abbreviate(name, 7);
            team.TeamNameAbbr3 = Abbreviate(name, 3).ToUpperInvariant();
        }
    }

    private static string Abbreviate(string value, int length) =>
        value.Length <= length ? value : value.Substring(0, length).TrimEnd();

    private static void ApplyManagerNames(TeamList teams, Dictionary<string, TableSnapshot> tables)
    {
        if (!tables.TryGetValue("manager", out var managers)) return;
        var teamId = Column(managers, "teamid");
        var firstName = Column(managers, "firstname");
        var surname = Column(managers, "surname");
        foreach (var row in managers.Rows)
        {
            var team = teams.SearchId(ParseIntAt(row, teamId)) as Team;
            if (team == null) continue;
            if (firstName >= 0 && firstName < row.Length) team.ManagerFirstName = row[firstName];
            if (surname >= 0 && surname < row.Length) team.ManagerSurname = row[surname];
        }
    }

    internal static int WriteChanges(string path)
    {
        var snapshot = s_snapshot ?? throw new InvalidOperationException("No FC26 snapshot is loaded.");
        var mappings = new (string Table, string IdColumn, System.Collections.IList? Items)[]
        {
            ("nations", "nationid", FifaEnvironment.Countries),
            ("leagues", "leagueid", FifaEnvironment.Leagues),
            ("teams", "teamid", FifaEnvironment.Teams),
            ("players", "playerid", FifaEnvironment.Players),
            ("stadiums", "stadiumid", FifaEnvironment.Stadiums),
            (snapshot.Tables.Any(t => t.Name.Equals("teamkits", StringComparison.OrdinalIgnoreCase))
                ? "teamkits" : "kits",
                snapshot.Tables.Any(t => t.Name.Equals("teamkits", StringComparison.OrdinalIgnoreCase))
                    ? "teamkitid" : "kitid", FifaEnvironment.Kits),
            ("formations", "formationid", FifaEnvironment.Formations),
            ("referee", "refereeid", FifaEnvironment.Referees),
            ("teamballs", "ballid", FifaEnvironment.Balls),
            ("playerboots", "shoetype", FifaEnvironment.Shoes),
            ("fieldpositionboundingboxes", "positionid", FifaEnvironment.Roles),
            ("leagueteamlinks", "teamid", FifaEnvironment.Teams),
            ("teamstadiumlinks", "teamid", FifaEnvironment.Teams),
            ("teamplayerlinks", "playerid", TeamPlayers()),
        };
        var changes = new List<Change>();
        foreach (var mapping in mappings)
        {
            var table = snapshot.Tables.FirstOrDefault(t =>
                t.Name.Equals(mapping.Table, StringComparison.OrdinalIgnoreCase));
            if (table == null || mapping.Items == null) continue;
            var idIndex = Column(table, mapping.IdColumn);
            if (idIndex < 0) continue;
            var rowObjects = mapping.Items.Cast<object>()
                .SelectMany(item => Origins(item)
                    .Where(origin => origin.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))
                    .Select(origin => (origin.RowIndex, Item: item)))
                .ToDictionary(value => value.RowIndex, value => value.Item);
            var objects = mapping.Items.Cast<object>()
                .Select(item => (Item: item, Id: ObjectId(item)))
                .Where(item => item.Id != int.MinValue)
                .GroupBy(item => item.Id)
                .ToDictionary(group => group.Key, group => group.Select(value => value.Item).ToList());
            var occurrences = new Dictionary<int, int>();
            for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];
                var id = ParseIntAt(row, idIndex);
                object item;
                if (!rowObjects.TryGetValue(rowIndex, out item))
                {
                    if (!objects.TryGetValue(id, out var matches)) continue;
                    occurrences.TryGetValue(id, out var occurrence);
                    occurrences[id] = occurrence + 1;
                    if (occurrence >= matches.Count) continue;
                    item = matches[occurrence];
                }
                var fields = AllFields(item.GetType())
                    .GroupBy(field => Normalize(field.Name))
                    .ToDictionary(group => group.Key, group => group.First());
                for (var columnIndex = 0; columnIndex < table.Columns.Length && columnIndex < row.Length; columnIndex++)
                {
                    if (columnIndex == idIndex) continue;
                    var columnName = Normalize(table.Columns[columnIndex]);
                    if (!TryResolveField(item.GetType(), fields, columnName, out var field)) continue;
                    var current = ToDatabaseTextForColumn(item, field, columnName);
                    if (DatabaseEquals(row[columnIndex], current, field.FieldType)) continue;
                    changes.Add(new Change
                    {
                        TableName = table.Name,
                        RowIndex = rowIndex,
                        FieldName = table.Columns[columnIndex],
                        Value = current
                    });
                }
            }
        }
        AppendPlayerNameChanges(snapshot, changes);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new ChangePlan
        {
            Version = 1,
            GameRoot = snapshot.GameRoot,
            DatabaseFolder = snapshot.DatabaseFolder,
            Changes = changes
        }));
        return changes.Count;
    }

    private static void AppendPlayerNameChanges(Snapshot snapshot, List<Change> changes)
    {
        var table = snapshot.Tables.FirstOrDefault(value =>
            value.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase));
        if (table == null || FifaEnvironment.Players == null) return;

        var idColumn = Column(table, "nameid");
        var nameColumn = Column(table, "name");
        if (idColumn < 0 || nameColumn < 0) return;

        var rowsById = new Dictionary<int, int>();
        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            rowsById[ParseIntAt(table.Rows[rowIndex], idColumn)] = rowIndex;

        var desiredByRow = new Dictionary<int, string>();
        foreach (Player player in FifaEnvironment.Players)
        {
            AddPlayerName(player.firstnameid, player.firstname);
            AddPlayerName(player.lastnameid, player.lastname);
            AddPlayerName(player.commonnameid, player.commonname);
            AddPlayerName(player.playerjerseynameid, player.playerjerseyname);
        }

        foreach (var desired in desiredByRow.OrderBy(value => value.Key))
        {
            var original = table.Rows[desired.Key][nameColumn];
            if (string.Equals(original, desired.Value, StringComparison.Ordinal)) continue;
            changes.Add(new Change
            {
                TableName = table.Name,
                RowIndex = desired.Key,
                FieldName = table.Columns[nameColumn],
                Value = desired.Value
            });
        }

        void AddPlayerName(int nameId, string? value)
        {
            if (nameId <= 0 || !rowsById.TryGetValue(nameId, out var rowIndex)) return;
            var desired = value ?? string.Empty;
            // Only stage names the user actually changed. Comparing against the
            // load-time resolution (not the raw snapshot text) avoids staging
            // decode artifacts (U+FFFD, control chars, LookBook rows) as fake
            // playername edits, which then fail reload verification.
            if (s_loadedPlayerNames.TryGetValue(nameId, out var loaded) &&
                string.Equals(loaded, desired, StringComparison.Ordinal))
                return;
            if (desiredByRow.TryGetValue(rowIndex, out var existing) &&
                !string.Equals(existing, desired, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Shared FC26 player name id {nameId} was edited to conflicting values.");
            desiredByRow[rowIndex] = desired;
        }
    }

    private static int ObjectId(object item)
    {
        if (item is TeamPlayer teamPlayer) return teamPlayer.Player?.Id ?? int.MinValue;
        var property = item.GetType().GetProperty("Id",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property?.GetValue(item) is int id) return id;
        var field = AllFields(item.GetType()).FirstOrDefault(value => Normalize(value.Name) == "id");
        return field?.GetValue(item) is int fieldId ? fieldId : int.MinValue;
    }

    private static System.Collections.IList TeamPlayers()
    {
        var result = new System.Collections.ArrayList();
        if (FifaEnvironment.Teams == null) return result;
        foreach (Team team in FifaEnvironment.Teams)
            foreach (TeamPlayer player in team.Roster)
                result.Add(player);
        return result;
    }

    private static IEnumerable<RowOrigin> Origins(object item) =>
        s_rowOrigins.TryGetValue(item, out var origins) ? origins : Enumerable.Empty<RowOrigin>();

    private static void SetOrigin(object item, string tableName, int rowIndex)
    {
        if (!s_rowOrigins.TryGetValue(item, out var origins))
            s_rowOrigins[item] = origins = new List<RowOrigin>();
        origins.Add(new RowOrigin(tableName, rowIndex));
    }

    private static string ToDatabaseText(object? value, Type type)
    {
        if (value == null) return string.Empty;
        var inner = Nullable.GetUnderlyingType(type) ?? type;
        if (inner == typeof(bool)) return (bool)value ? "1" : "0";
        if (inner == typeof(DateTime))
            return FifaUtil.ConvertFromDate((DateTime)value).ToString(CultureInfo.InvariantCulture);
        if (inner.IsEnum) return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        if (value is IFormattable formattable) return formattable.ToString(null, CultureInfo.InvariantCulture);
        return value.ToString() ?? string.Empty;
    }

    private static bool DatabaseEquals(string original, string current, Type type)
    {
        var inner = Nullable.GetUnderlyingType(type) ?? type;
        if (inner == typeof(DateTime))
            return int.TryParse(original, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gregorian) &&
                   int.TryParse(current, NumberStyles.Integer, CultureInfo.InvariantCulture, out var currentGregorian) &&
                   gregorian == currentGregorian;
        if (inner == typeof(bool))
            return (original == "1" || original.Equals("true", StringComparison.OrdinalIgnoreCase)) == (current == "1");
        if (inner == typeof(float) || inner == typeof(double))
            return double.TryParse(original, NumberStyles.Float, CultureInfo.InvariantCulture, out var left) &&
                   double.TryParse(current, NumberStyles.Float, CultureInfo.InvariantCulture, out var right) &&
                   Math.Abs(left - right) < 0.00001;
        return string.Equals(original, current, StringComparison.Ordinal);
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
        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            if (idIndex < 0 || idIndex >= row.Length || !int.TryParse(row[idIndex], out var id)) continue;
            var item = Activator.CreateInstance(typeof(TItem), id);
            if (item == null) continue;
            MapFields(item, table.Columns, row);
            SetOrigin(item, table.Name, rowIndex);
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
            var columnName = Normalize(columns[i]);
            if (!TryResolveField(target.GetType(), fields, columnName, out var field)) continue;
            try
            {
                var value = ConvertValue(values[i], field.FieldType);
                if (target is Player && columnName == "preferredfoot" && value is int preferredFoot)
                    value = preferredFoot <= 1 ? 0 : 1;
                field.SetValue(target, value);
            }
            catch { /* New FC26-only columns stay in the snapshot and are ignored by CM16 forms. */ }
        }
        if (target is Country country)
        {
            country.LanguageName = country.DatabaseName;
            country.LanguageShortName = country.DatabaseName;
        }
    }

    private static bool TryResolveField(Type targetType, Dictionary<string, FieldInfo> fields,
        string columnName, out FieldInfo field)
    {
        if (fields.TryGetValue(columnName, out field!)) return true;
        if (targetType == typeof(Kit))
        {
            // Frostbite renamed the CM16 teamkits helper columns. Keep the
            // legacy Kit object model intact and translate only at the bridge.
            if (columnName == "teamtechid" && fields.TryGetValue("teamid", out field!)) return true;
            if (columnName == "teamkittypetechid" && fields.TryGetValue("kittype", out field!)) return true;
        }
        if (targetType == typeof(Team))
        {
            // FC26 removed teams.transferbudget; clubworth is the nearest per-team value.
            if (columnName == "clubworth" && fields.TryGetValue("transferbudget", out field!)) return true;
        }
        field = null!;
        return false;
    }

    private static string ToDatabaseTextForColumn(object item, FieldInfo field, string columnName)
    {
        var value = field.GetValue(item);
        if (item is Player && columnName == "preferredfoot" && value is int preferredFoot)
            value = (preferredFoot <= 0 ? 0 : 1) + 1;
        return ToDatabaseText(value, field.FieldType);
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
        if (inner == typeof(DateTime)) return FifaUtil.ConvertToDate(ParseInt(value));
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
            if (id >= 0 && name >= 0 && int.TryParse(row[id], out var key))
                lookup[key] = SanitizeDisplayName(row[name]);

        foreach (Player player in players)
        {
            var fields = AllFields(typeof(Player)).ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
            SetResolved(fields, player, "m_firstnameid", "m_firstname", lookup);
            SetResolved(fields, player, "m_lastnameid", "m_lastname", lookup);
            SetResolved(fields, player, "m_commonnameid", "m_commonname", lookup);
            SetResolved(fields, player, "m_playerjerseynameid", "m_playerjerseyname", lookup);

            if (string.IsNullOrWhiteSpace(player.firstname) && string.IsNullOrWhiteSpace(player.lastname) &&
                string.IsNullOrWhiteSpace(player.commonname))
            {
                player.commonname = "Player " + player.Id.ToString(CultureInfo.InvariantCulture);
                if (fields.TryGetValue("m_commonnameid", out var commonId) && commonId.GetValue(player) is int nameId)
                    s_loadedPlayerNames[nameId] = player.commonname;
            }
        }
    }

    private static string SanitizeDisplayName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var cleaned = new string(value.Where(ch => ch != '\uFFFD' && ch != '\uFFFE' && ch != '\uFFFF' &&
            (!char.IsControl(ch) || ch == '\t')).ToArray()).Trim();
        return cleaned.Contains("LookBook", StringComparison.OrdinalIgnoreCase) ? string.Empty : cleaned;
    }

    private static void SetResolved(Dictionary<string, FieldInfo> fields, Player player,
        string idField, string textField, Dictionary<int, string> names)
    {
        if (!fields.TryGetValue(idField, out var source) || !fields.TryGetValue(textField, out var target)) return;
        var id = (int)(source.GetValue(player) ?? 0);
        var value = names.TryGetValue(id, out var resolved) ? resolved : string.Empty;
        target.SetValue(player, value);
        if (id > 0) s_loadedPlayerNames[id] = value;
    }

    private static void LinkCore(Dictionary<string, TableSnapshot> tables, CountryList countries,
        LeagueList leagues, TeamList teams, PlayerList players, StadiumList stadiums,
        KitList kits, FormationList formations)
    {
        if (tables.TryGetValue("leagueteamlinks", out var leagueRows))
        {
            var teamId = Column(leagueRows, "teamid");
            for (var rowIndex = 0; rowIndex < leagueRows.Rows.Count; rowIndex++)
            {
                var row = leagueRows.Rows[rowIndex];
                var team = teams.SearchId(ParseIntAt(row, teamId)) as Team;
                if (team == null) continue;
                MapFields(team, leagueRows.Columns, row);
                SetOrigin(team, leagueRows.Name, rowIndex);
            }
        }
        if (tables.TryGetValue("teamstadiumlinks", out var stadiumRows))
        {
            var teamId = Column(stadiumRows, "teamid");
            for (var rowIndex = 0; rowIndex < stadiumRows.Rows.Count; rowIndex++)
            {
                var row = stadiumRows.Rows[rowIndex];
                var team = teams.SearchId(ParseIntAt(row, teamId)) as Team;
                if (team == null) continue;
                MapFields(team, stadiumRows.Columns, row);
                SetOrigin(team, stadiumRows.Name, rowIndex);
            }
        }

        leagues.LinkCountry(countries);
        teams.LinkCountry(countries);
        countries.LinkTeam(teams);
        players.LinkCountry(countries);
        kits.LinkTeam(teams);
        // FC26 teamkits are linked by teamtechid and do not populate the
        // legacy CM16 m_teamkitidList helper. Build each team's kit collection
        // directly so the Kit section and team/kit pickers are never empty.
        foreach (Team team in teams) team.m_KitList.Clear();
        foreach (Kit kit in kits)
        {
            if (kit.Team != null) kit.Team.m_KitList.Add(kit);
        }
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
                if (league != null && team != null)
                {
                    league.LinkTeam(team);
                    team.League = league;
                    // FC26 teams do not carry the legacy CM16 country helper
                    // columns. Derive club country from the linked league.
                    if (team.Country == null && league.Country != null) team.Country = league.Country;
                }
            }
        }
        if (tables.TryGetValue("teamnationlinks", out var nationLinks))
        {
            var nationId = Column(nationLinks, "nationid");
            var teamId = Column(nationLinks, "teamid");
            foreach (var row in nationLinks.Rows)
            {
                var country = countries.SearchId(ParseIntAt(row, nationId)) as Country;
                var team = teams.SearchId(ParseIntAt(row, teamId)) as Team;
                if (country == null || team == null) continue;
                team.Country = country;
                team.NationalTeam = true;
            }
        }
        if (tables.TryGetValue("teamplayerlinks", out var playerLinks))
        {
            var teamId = Column(playerLinks, "teamid"); var playerId = Column(playerLinks, "playerid");
            var jersey = Column(playerLinks, "jerseynumber");
            for (var rowIndex = 0; rowIndex < playerLinks.Rows.Count; rowIndex++)
            {
                var row = playerLinks.Rows[rowIndex];
                var team = teams.SearchId(ParseIntAt(row, teamId)) as Team;
                var player = players.SearchId(ParseIntAt(row, playerId)) as Player;
                if (team != null && player != null)
                {
                    var teamPlayer = team.AddTeamPlayer(player, ParseIntAt(row, jersey));
                    MapFields(teamPlayer, playerLinks.Columns, row);
                    SetOrigin(teamPlayer, playerLinks.Name, rowIndex);
                }
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

    private sealed class ChangePlan
    {
        public int Version { get; set; }
        public string GameRoot { get; set; } = string.Empty;
        public string DatabaseFolder { get; set; } = string.Empty;
        public List<Change> Changes { get; set; } = new();
    }

    private sealed class Change
    {
        public string TableName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private sealed class RowOrigin
    {
        internal RowOrigin(string tableName, int rowIndex)
        {
            TableName = tableName;
            RowIndex = rowIndex;
        }

        internal string TableName { get; }
        internal int RowIndex { get; }
    }

    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        internal static readonly ReferenceComparer Instance = new ReferenceComparer();
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
