using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FifaLibrary;

namespace CreationMaster;

internal static class Fc26SnapshotLoader
{
    private static Snapshot? s_snapshot;
    private static string s_snapshotPath = string.Empty;
    private static readonly HashSet<string> s_coreTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "nations", "leagues", "leagueteamlinks", "teams", "teamplayerlinks",
        "teamnationlinks", "teamstadiumlinks", "players", "playernames",
        "stadiums", "teamkits", "kits", "formations", "default_mentalities",
        "defaultteamdata", "default_teamsheets", "manager", "referee",
        "leaguerefereelinks", "teamballs", "playerboots",
        "fieldpositionboundingboxes", "competition"
    };
    private static readonly Dictionary<object, List<RowOrigin>> s_rowOrigins =
        new Dictionary<object, List<RowOrigin>>(ReferenceComparer.Instance);
    /// <summary>nameid -> resolved display name recorded at snapshot load. Used to
    /// detect genuine user edits; decode artifacts must never be staged as edits.</summary>
    private static readonly Dictionary<int, string> s_loadedPlayerNames =
        new Dictionary<int, string>();
    // Player-name allocation is on the create-player hot path.  Keep one
    // decoded lookup for the lifetime of the loaded snapshot instead of
    // rescanning the ~40k-row playernames table for every field of every new
    // roster player.
    private static bool s_playerNameIndexReady;
    private static readonly Dictionary<int, string> s_playerNameTextById =
        new Dictionary<int, string>();
    private static readonly Dictionary<string, HashSet<int>> s_playerNameIdsByText =
        new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
    private static readonly HashSet<int> s_playerNameUsedIds = new HashSet<int>();
    private static readonly Dictionary<int, (int Id, string Text)> s_playerNameRowState =
        new Dictionary<int, (int Id, string Text)>();
    /// <summary>Original name IDs/text for each loaded Player object. FC26 has
    /// many intentionally empty common-name IDs, so the writer must know which
    /// values were changed by the user before allocating a new dictionary row.
    /// </summary>
    private static readonly Dictionary<object, PlayerNameState> s_loadedPlayerNameStates =
        new Dictionary<object, PlayerNameState>(ReferenceComparer.Instance);
    /// <summary>
    /// Formation coordinates in the FC26 database are stored as precise floats,
    /// while the legacy editor exposes integer percentages.  Keep the exact
    /// editor-side state captured at load time so opening and saving a project
    /// does not quantize every untouched formation in the database.
    /// </summary>
    private static readonly Dictionary<object, FormationRoleState[]> s_loadedFormationRoles =
        new Dictionary<object, FormationRoleState[]>(ReferenceComparer.Instance);
    private static readonly Dictionary<object, TeamSheetState> s_loadedTeamSheets =
        new Dictionary<object, TeamSheetState>(ReferenceComparer.Instance);
    private static readonly HashSet<string> s_teamSheetAssignmentFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "captainid", "freekicktakerid", "leftcornerkicktakerid", "leftfreekicktakerid",
        "longkicktakerid", "penaltytakerid", "rightcornerkicktakerid", "rightfreekicktakerid"
    };
    private static readonly Dictionary<string, Change> s_detailChanges =
        new Dictionary<string, Change>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> s_detailOriginalValues =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static readonly List<StructuralChange> s_structuralChanges = new List<StructuralChange>();
    private static readonly Dictionary<string, string[]> s_requiredSchema =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["nations"] = new[] { "nationid" },
            ["leagues"] = new[] { "leagueid", "countryid" },
            ["teams"] = new[] { "teamid" },
            ["leagueteamlinks"] = new[] { "leagueid", "teamid" },
            ["players"] = new[] { "playerid" },
            ["teamplayerlinks"] = new[] { "teamid", "playerid" },
            ["playernames"] = new[] { "nameid", "name" },
            ["stadiums"] = new[] { "stadiumid" },
            ["teamkits"] = new[] { "teamid", "teamkittypetechid" }
        };
    // Schema fingerprints are based only on ordered table/column names, never
    // database content. Add a fingerprint only after that FC26 title update
    // passes extraction, direct-save verification and the release matrix.
    private static readonly HashSet<string> s_verifiedSchemaFingerprints =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a828de1dbf6238ab"
        };

    internal static void Load(string path)
    {
        var snapshot = ReadSnapshot(path);
        s_snapshotPath = Path.GetFullPath(path);
        // The legacy shell is a 32-bit process.  Decode core payloads serially
        // so peak working-set stays predictable; the UI now runs this method on
        // a worker thread, so serial decoding no longer makes the window freeze.
        foreach (var table in snapshot.Tables.Where(table =>
            table.IsCore || s_coreTables.Contains(table.Name)))
            EnsureRows(table, s_snapshotPath);
        var tables = snapshot.Tables.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
        s_snapshot = snapshot;
        s_rowOrigins.Clear();
        s_loadedPlayerNames.Clear();
        s_playerNameIndexReady = false;
        s_playerNameTextById.Clear();
        s_playerNameIdsByText.Clear();
        s_playerNameUsedIds.Clear();
        s_playerNameRowState.Clear();
        s_loadedPlayerNameStates.Clear();
        s_loadedFormationRoles.Clear();
        s_loadedTeamSheets.Clear();
        s_detailChanges.Clear();
        s_detailOriginalValues.Clear();
        s_structuralChanges.Clear();

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
        ApplyFormationNames(formations);
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
        Fc26ActivityLog.Add("Open", "Loaded " + snapshot.Tables.Count + " FC26 table(s) from " +
            (string.IsNullOrWhiteSpace(snapshot.GameRoot) ? snapshot.DatabaseFolder : snapshot.GameRoot));
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

    private static void ApplyFormationNames(FormationList formations)
    {
        // FC26's formationname is deliberately shared by several variants.
        // Preserve the 29 database-native choices by resolving their generic
        // relative ID instead of collapsing them to the short name.
        foreach (Formation formation in formations)
        {
            var layoutId = formation.teamid == -1 ? formation.Id : formation.relativeformationid;
            formation.formationfullname = Fc26FormationNames.Get(layoutId, formation.formationname);
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

        // FC26's playernames table is a shared dictionary.  The legacy object
        // model exposes the decoded string next to the nameid, so editing a
        // player used to leave the old shared id in place.  The writer then
        // attempted to change one row to two different names (for example the
        // 30683 warning reported by users).  Detach every genuinely edited
        // name before mapping player rows so the new nameid is written together
        // with the player and the original database row remains untouched.
        NormalizeEditedPlayerNameIds();

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
                    if (mapping.Table.Equals("teamplayerlinks", StringComparison.OrdinalIgnoreCase) &&
                        columnName == "position" && s_loadedTeamSheets.Count > 0) continue;
                    if (mapping.Table.Equals("teams", StringComparison.OrdinalIgnoreCase) &&
                        s_teamSheetAssignmentFields.Contains(table.Columns[columnIndex]) && s_loadedTeamSheets.Count > 0) continue;
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
        AppendDefaultTeamSheetChanges(snapshot, changes);
        AppendFc26TacticMirrorChanges(snapshot, changes);
        AppendFormationRoleChanges(snapshot, changes);
        AppendPlayerNameChanges(snapshot, changes);
        AppendDetailChanges(changes);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new ChangePlan
        {
            Version = 1,
            GameRoot = snapshot.GameRoot,
            DatabaseFolder = snapshot.DatabaseFolder,
            StructuralChanges = s_structuralChanges,
            Changes = changes
        }));
        return changes.Count + s_structuralChanges.Count;
    }

    internal static void StageNewEntity(string section, object item)
    {
        var mapping = (section ?? string.Empty).ToLowerInvariant() switch
        {
            "nation" => (Table: "nations", IdColumn: "nationid"),
            "league" => (Table: "leagues", IdColumn: "leagueid"),
            "team" => (Table: "teams", IdColumn: "teamid"),
            "player" => (Table: "players", IdColumn: "playerid"),
            _ => throw new InvalidOperationException("Unsupported direct creation type: " + section)
        };
        var snapshot = s_snapshot ?? throw new InvalidOperationException("No FC26 snapshot is loaded.");
        var table = snapshot.Tables.FirstOrDefault(candidate =>
            candidate.Name.Equals(mapping.Table, StringComparison.OrdinalIgnoreCase));
        if (table != null) EnsureRows(table, s_snapshotPath);
        if (table == null || table.Rows.Count == 0)
            throw new InvalidOperationException("The FC26 " + mapping.Table + " table has no safe template row.");
        var id = ObjectId(item);
        if (id == int.MinValue) throw new InvalidOperationException("The new record has no usable database ID.");
        if (Origins(item).Any(origin => origin.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))) return;

        var newRow = DuplicateDetailRow(table.Name, 0);
        StageDetailValue(table.Name, newRow, mapping.IdColumn, id.ToString(CultureInfo.InvariantCulture));
        SetOrigin(item, table.Name, newRow);
        Fc26ActivityLog.Add("Create", section + " " + id + " staged from a validated template row");
    }

    internal static void AssignTeamToLeague(Team team, League league)
    {
        if (team == null || league == null) throw new InvalidOperationException("Select a valid team and league.");
        var snapshot = s_snapshot ?? throw new InvalidOperationException("No FC26 snapshot is loaded.");
        var table = snapshot.Tables.FirstOrDefault(candidate =>
            candidate.Name.Equals("leagueteamlinks", StringComparison.OrdinalIgnoreCase));
        if (table != null) EnsureRows(table, s_snapshotPath);
        if (table == null || table.Rows.Count == 0)
            throw new InvalidOperationException("The FC26 league-team link table has no safe template row.");

        // Keep the friendly object graph consistent even when WinForms data binding
        // has already assigned Team.League before SelectedIndexChanged is raised.
        foreach (League candidate in FifaEnvironment.Leagues)
            if (candidate != league) candidate.PlayingTeams.RemoveId(team);
        league.AddTeam(team);

        var origin = Origins(team).FirstOrDefault(value =>
            value.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase));
        var rowIndex = origin == null ? -1 : origin.RowIndex;
        if (rowIndex < 0)
        {
            rowIndex = DuplicateDetailRow(table.Name, 0);
            SetOrigin(team, table.Name, rowIndex);
            var keyColumn = Column(table, "artificialkey");
            if (keyColumn >= 0)
            {
                var nextKey = table.Rows.Select(row => ParseIntAt(row, keyColumn)).DefaultIfEmpty(0).Max() + 1;
                var keyDescriptor = table.ColumnDetails.FirstOrDefault(column =>
                    column.Name.Equals("artificialkey", StringComparison.OrdinalIgnoreCase));
                if (keyDescriptor != null && nextKey > keyDescriptor.RangeHigh)
                    throw new InvalidOperationException("No free league-team link key remains.");
                StageDetailValue(table.Name, rowIndex, "artificialkey", nextKey.ToString(CultureInfo.InvariantCulture));
            }
        }

        void StageIfPresent(string field, int value)
        {
            if (Column(table, field) >= 0)
                StageDetailValue(table.Name, rowIndex, field, value.ToString(CultureInfo.InvariantCulture));
        }
        StageIfPresent("teamid", team.Id);
        StageIfPresent("leagueid", league.Id);
        StageIfPresent("prevleagueid", league.Id);
        StageIfPresent("currenttableposition", team.currenttableposition);
        StageIfPresent("previousyeartableposition", team.previousyeartableposition);
        Fc26ActivityLog.Add("League link", "team " + team.Id + " → league " + league.Id + " staged");
    }

    /// <summary>
    /// Adds a new roster link for a team created by the guided FC26 wizard.
    /// The old CM16 object model can hold the TeamPlayer in memory without
    /// creating a database row; FC26 needs an explicit duplicated link row.
    /// Keeping this operation in the snapshot writer makes the classic shell
    /// and the direct-save bridge use the same artificial-key rules.
    /// </summary>
    internal static void StageNewTeamPlayerLink(TeamPlayer teamPlayer)
    {
        if (teamPlayer?.Team == null || teamPlayer.Player == null)
            throw new InvalidOperationException("The new roster link is missing a team or player.");
        var table = DetailTable("teamplayerlinks");
        if (table == null || table.Rows.Count == 0)
            throw new InvalidOperationException("The FC26 team-player link table has no safe template row.");
        var teamColumn = table.Column("teamid");
        var playerColumn = table.Column("playerid");
        if (teamColumn >= 0 && playerColumn >= 0)
        {
            for (var row = 0; row < table.Rows.Count; row++)
            {
                if (IsDetailDeleted(table.Name, row)) continue;
                if (ParseIntAt(table.Rows[row], teamColumn) == teamPlayer.Team.Id &&
                    ParseIntAt(table.Rows[row], playerColumn) == teamPlayer.Player.Id)
                    return;
            }
        }

        var rowIndex = DuplicateDetailRow(table.Name, 0);
        StageDetailValue(table.Name, rowIndex, "teamid", teamPlayer.Team.Id.ToString(CultureInfo.InvariantCulture));
        StageDetailValue(table.Name, rowIndex, "playerid", teamPlayer.Player.Id.ToString(CultureInfo.InvariantCulture));
        StageDetailValue(table.Name, rowIndex, "jerseynumber", teamPlayer.jerseynumber.ToString(CultureInfo.InvariantCulture));
        StageDetailValue(table.Name, rowIndex, "position", teamPlayer.position.ToString(CultureInfo.InvariantCulture));
        if (table.Column("artificialkey") >= 0)
            StageDetailValue(table.Name, rowIndex, "artificialkey", NextDetailKey(table, "artificialkey").ToString(CultureInfo.InvariantCulture));
        // Keep the object-to-row map aware of the newly-created link.  This is
        // important if the user changes a shirt number or position before the
        // first Save: WriteChanges must target this staged row, not an older
        // occurrence of the same player ID.
        SetOrigin(teamPlayer, table.Name, rowIndex);
        Fc26ActivityLog.Add("Roster link", "team " + teamPlayer.Team.Id + " ← player " + teamPlayer.Player.Id + " staged");
    }

    /// <summary>Creates the active FC26 teamsheet row for a newly-created team.</summary>
    internal static void StageNewTeamSheet(Team team)
    {
        if (team == null) throw new InvalidOperationException("The new team is unavailable.");
        var table = DetailTable("default_teamsheets");
        if (table == null || table.Rows.Count == 0) return;
        var teamColumn = table.Column("teamid");
        if (teamColumn < 0) return;
        for (var row = 0; row < table.Rows.Count; row++)
        {
            if (!IsDetailDeleted(table.Name, row) && ParseIntAt(table.Rows[row], teamColumn) == team.Id)
                return;
        }
        var rowIndex = DuplicateDetailRow(table.Name, 0);
        StageDetailValue(table.Name, rowIndex, "teamid", team.Id.ToString(CultureInfo.InvariantCulture));
        var players = team.Roster.Cast<TeamPlayer>().Where(value => value?.Player != null)
            .GroupBy(value => value.Player.Id).Select(group => group.First()).Take(52).ToArray();
        for (var slot = 0; slot < players.Length; slot++)
        {
            var field = "playerid" + slot.ToString(CultureInfo.InvariantCulture);
            if (table.Column(field) >= 0)
                StageDetailValue(table.Name, rowIndex, field, players[slot].Player.Id.ToString(CultureInfo.InvariantCulture));
            var position = "position" + slot.ToString(CultureInfo.InvariantCulture);
            if (slot < 11 && table.Column(position) >= 0)
                StageDetailValue(table.Name, rowIndex, position, players[slot].position.ToString(CultureInfo.InvariantCulture));
        }
        Fc26ActivityLog.Add("Teamsheet", "team " + team.Id + " default teamsheet staged");
    }

    /// <summary>Creates a safe stadium relationship when the wizard has a template stadium.</summary>
    internal static void StageNewTeamStadiumLink(Team team)
    {
        if (team == null || team.Stadium == null) return;
        var table = DetailTable("teamstadiumlinks");
        if (table == null || table.Rows.Count == 0) return;
        var teamColumn = table.Column("teamid");
        var stadiumColumn = table.Column("stadiumid");
        if (teamColumn < 0 || stadiumColumn < 0) return;
        for (var row = 0; row < table.Rows.Count; row++)
        {
            if (!IsDetailDeleted(table.Name, row))
            {
                if (ParseIntAt(table.Rows[row], teamColumn) == team.Id) return;
            }
        }
        var rowIndex = DuplicateDetailRow(table.Name, 0);
        StageDetailValue(table.Name, rowIndex, "teamid", team.Id.ToString(CultureInfo.InvariantCulture));
        StageDetailValue(table.Name, rowIndex, "stadiumid", team.Stadium.Id.ToString(CultureInfo.InvariantCulture));
        if (table.Column("artificialkey") >= 0)
            StageDetailValue(table.Name, rowIndex, "artificialkey", NextDetailKey(table, "artificialkey").ToString(CultureInfo.InvariantCulture));
        SetOrigin(team, table.Name, rowIndex);
        Fc26ActivityLog.Add("Stadium link", "team " + team.Id + " → stadium " + team.Stadium.Id + " staged");
    }

    /// <summary>Stages a duplicated teamkits row.  FC26 stores the team
    /// relationship on the kit record itself (there is no separate kit link
    /// table), so the duplicated row must carry both teamtechid/teamid and the
    /// kit type before the save engine can validate it.</summary>
    internal static void StageNewKit(Kit kit)
    {
        if (kit == null || kit.Team == null) throw new InvalidOperationException("The new kit is missing its team reference.");
        var table = DetailTable("teamkits") ?? DetailTable("kits");
        if (table == null || table.Rows.Count == 0) throw new InvalidOperationException("The FC26 kit table has no safe template row.");
        var idColumn = table.Column(table.Name.Equals("teamkits", StringComparison.OrdinalIgnoreCase) ? "teamkitid" : "kitid");
        if (idColumn < 0) throw new InvalidOperationException("The FC26 kit table has no kit ID column.");
        if (Origins(kit).Any(value => value.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))) return;
        var rowIndex = DuplicateDetailRow(table.Name, 0);
        StageDetailValue(table.Name, rowIndex, table.Columns[idColumn], kit.Id.ToString(CultureInfo.InvariantCulture));
        if (table.Column("teamtechid") >= 0) StageDetailValue(table.Name, rowIndex, "teamtechid", kit.Team.Id.ToString(CultureInfo.InvariantCulture));
        if (table.Column("teamid") >= 0) StageDetailValue(table.Name, rowIndex, "teamid", kit.Team.Id.ToString(CultureInfo.InvariantCulture));
        if (table.Column("teamkittypetechid") >= 0) StageDetailValue(table.Name, rowIndex, "teamkittypetechid", kit.kittype.ToString(CultureInfo.InvariantCulture));
        if (table.Column("kittype") >= 0) StageDetailValue(table.Name, rowIndex, "kittype", kit.kittype.ToString(CultureInfo.InvariantCulture));
        SetOrigin(kit, table.Name, rowIndex);
        Fc26ActivityLog.Add("Kit", "team " + kit.Team.Id + " " + kit.kittype + " staged");
    }

    /// <summary>
    /// Adds editable player-name rows for a player created by the wizard.  FC26
    /// keeps names in playernames rather than in the players table, so merely
    /// setting Player.firstname/lastname would otherwise display a blank name in
    /// a fresh Career.
    /// </summary>
    internal static void StageNewPlayerNames(Player player)
    {
        if (player == null) throw new InvalidOperationException("The new player is unavailable.");
        var table = DetailTable("playernames");
        if (table == null || table.Rows.Count == 0) return;
        player.firstnameid = StagePlayerName(table, player.firstnameid, player.firstname);
        player.lastnameid = StagePlayerName(table, player.lastnameid, player.lastname);
        player.commonnameid = StagePlayerName(table, player.commonnameid, player.commonname);
        player.playerjerseynameid = StagePlayerName(table, player.playerjerseynameid, player.playerjerseyname);
    }

    private static int StagePlayerName(SnapshotDetailTable table, int currentId, string value)
    {
        value = (value ?? string.Empty).Trim();
        if (value.Length == 0) return 0;
        var idColumn = table.Column("nameid");
        var nameColumn = table.Column("name");
        if (idColumn < 0 || nameColumn < 0) return currentId;

        EnsurePlayerNameIndex(table);
        if (currentId > 0)
        {
            // A nameid is not an ownership token: many players can point at
            // one row.  Reuse it only when its current decoded value matches.
            if (s_playerNameTextById.TryGetValue(currentId, out var current) &&
                string.Equals(current, value, StringComparison.Ordinal))
                return currentId;
        }
        if (s_playerNameIdsByText.TryGetValue(value, out var matches) && matches.Count > 0)
            return matches.Min();

        var rowIndex = DuplicateDetailRow(table.Name, 0);
        var id = NextDetailKey(table, "nameid");
        StageDetailValue(table.Name, rowIndex, "nameid", id.ToString(CultureInfo.InvariantCulture));
        StageDetailValue(table.Name, rowIndex, "name", value);
        return id;
    }

    private static string DetailPlayerName(SnapshotDetailTable table, int rowIndex,
        int idColumn, int nameColumn)
    {
        var id = ParseIntAt(table.Rows[rowIndex], idColumn);
        // A staged value is already decoded/literal and must win over the
        // load-time cache.  This also makes a second click on Save idempotent.
        if (IsDetailChanged(table.Name, rowIndex, "name"))
            return table.Rows[rowIndex][nameColumn]?.Trim() ?? string.Empty;
        if (s_loadedPlayerNames.TryGetValue(id, out var loaded))
            return (loaded ?? string.Empty).Trim();
        return table.Rows[rowIndex][nameColumn]?.Trim() ?? string.Empty;
    }

    private static void EnsurePlayerNameIndex(SnapshotDetailTable table)
    {
        if (s_playerNameIndexReady || !table.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase))
            return;
        var idColumn = table.Column("nameid");
        var nameColumn = table.Column("name");
        if (idColumn < 0 || nameColumn < 0) return;

        for (var row = 0; row < table.Rows.Count; row++)
            RegisterPlayerNameRow(table, row);
        s_playerNameIndexReady = true;
    }

    private static void RegisterPlayerNameRow(SnapshotDetailTable table, int rowIndex)
    {
        if (!table.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase) ||
            rowIndex < 0 || rowIndex >= table.Rows.Count) return;
        var idColumn = table.Column("nameid");
        var nameColumn = table.Column("name");
        if (idColumn < 0 || nameColumn < 0) return;

        var id = ParseIntAt(table.Rows[rowIndex], idColumn);
        var text = DetailPlayerName(table, rowIndex, idColumn, nameColumn);
        if (s_playerNameRowState.TryGetValue(rowIndex, out var previous))
            RemovePlayerNameIndex(previous.Id, previous.Text);
        s_playerNameRowState[rowIndex] = (id, text);
        AddPlayerNameIndex(id, text);
    }

    private static void AddPlayerNameIndex(int id, string? text)
    {
        if (id <= 0) return;
        s_playerNameUsedIds.Add(id);
        text = (text ?? string.Empty).Trim();
        if (text.Length == 0) return;
        s_playerNameTextById[id] = text;
        if (!s_playerNameIdsByText.TryGetValue(text, out var ids))
            s_playerNameIdsByText[text] = ids = new HashSet<int>();
        ids.Add(id);
    }

    private static void RemovePlayerNameIndex(int id, string? text)
    {
        if (id <= 0) return;
        text = (text ?? string.Empty).Trim();
        if (text.Length == 0) return;
        if (s_playerNameIdsByText.TryGetValue(text, out var ids))
        {
            ids.Remove(id);
            if (ids.Count == 0) s_playerNameIdsByText.Remove(text);
        }
        if (s_playerNameTextById.TryGetValue(id, out var current) &&
            string.Equals(current, text, StringComparison.Ordinal))
            s_playerNameTextById.Remove(id);
    }

    private sealed class PlayerNameState
    {
        internal int FirstNameId;
        internal string FirstName = string.Empty;
        internal int LastNameId;
        internal string LastName = string.Empty;
        internal int CommonNameId;
        internal string CommonName = string.Empty;
        internal int JerseyNameId;
        internal string JerseyName = string.Empty;
    }

    /// <summary>
    /// Separates edited legacy Player objects from FC26's shared playernames
    /// rows.  This runs immediately before the change plan is built, so the
    /// updated nameid values are included in the players table changes.
    /// </summary>
    private static void NormalizeEditedPlayerNameIds()
    {
        if (s_snapshot == null || FifaEnvironment.Players == null) return;
        var table = DetailTable("playernames");
        if (table == null || table.Rows.Count == 0) return;

        // Do not rescan the ~40k-row playernames table for every one of the
        // ~20k players on an ordinary Save.  The lookup is only used to prove
        // that an unchanged reference can stay in place; the allocator is
        // reached only for the small number of genuinely edited/new values.
        var idColumn = table.Column("nameid");
        var nameColumn = table.Column("name");
        if (idColumn < 0 || nameColumn < 0) return;
        EnsurePlayerNameIndex(table);
        var namesById = s_playerNameTextById;

        foreach (Player player in FifaEnvironment.Players)
        {
            // Do not turn the normal Save into a 16k-row common-name import:
            // FC26 deliberately stores zero/empty commonname IDs for most
            // players. Only fields that differ from the load-time state (or
            // a genuinely new Player object) need an allocator pass.
            if (!s_loadedPlayerNameStates.TryGetValue(player, out var original))
            {
                player.firstnameid = NormalizePlayerNameId(table, namesById, player.firstnameid, player.firstname);
                player.lastnameid = NormalizePlayerNameId(table, namesById, player.lastnameid, player.lastname);
                player.commonnameid = NormalizePlayerNameId(table, namesById, player.commonnameid, player.commonname);
                player.playerjerseynameid = NormalizePlayerNameId(table, namesById,
                    player.playerjerseynameid, player.playerjerseyname);
                continue;
            }

            if (player.firstnameid != original.FirstNameId ||
                !string.Equals(player.firstname, original.FirstName, StringComparison.Ordinal))
                player.firstnameid = NormalizePlayerNameId(table, namesById, player.firstnameid, player.firstname);
            if (player.lastnameid != original.LastNameId ||
                !string.Equals(player.lastname, original.LastName, StringComparison.Ordinal))
                player.lastnameid = NormalizePlayerNameId(table, namesById, player.lastnameid, player.lastname);
            if (player.commonnameid != original.CommonNameId ||
                !string.Equals(player.commonname, original.CommonName, StringComparison.Ordinal))
                player.commonnameid = NormalizePlayerNameId(table, namesById, player.commonnameid, player.commonname);
            if (player.playerjerseynameid != original.JerseyNameId ||
                !string.Equals(player.playerjerseyname, original.JerseyName, StringComparison.Ordinal))
                player.playerjerseynameid = NormalizePlayerNameId(table, namesById,
                    player.playerjerseynameid, player.playerjerseyname);
        }
    }

    private static int NormalizePlayerNameId(SnapshotDetailTable table,
        IReadOnlyDictionary<int, string> namesById, int currentId, string? value)
    {
        var desired = (value ?? string.Empty).Trim();
        if (desired.Length == 0)
        {
            // The current FC26 database contains compressed rows that the
            // legacy shell may not be able to decode.  Their text appears
            // empty in the form even though the numeric reference is valid;
            // preserve that reference unless the loaded value was genuinely
            // readable and the user cleared it.
            if (currentId > 0 &&
                (!s_loadedPlayerNames.TryGetValue(currentId, out var loaded) ||
                 string.IsNullOrWhiteSpace(loaded)))
                return currentId;
            return 0;
        }

        if (currentId > 0 && namesById.TryGetValue(currentId, out var current) &&
            string.Equals(current, desired, StringComparison.Ordinal))
            return currentId;

        // StagePlayerName performs the decoded-value comparison and allocates
        // from the table's validated descriptor range when the current id is a
        // different/shared name.  Keeping this call for new/staged IDs makes
        // the operation safe for both newly-created and existing players.
        return StagePlayerName(table, currentId, desired);
    }

    private static int NextDetailKey(SnapshotDetailTable table, string fieldName)
    {
        var column = table.Column(fieldName);
        if (column < 0) return 1;
        var descriptor = table.ColumnDetails.FirstOrDefault(value =>
            value.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        var used = table.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase) &&
                   fieldName.Equals("nameid", StringComparison.OrdinalIgnoreCase)
            ? s_playerNameUsedIds
            : new HashSet<int>(table.Rows.Select(row => ParseIntAt(row, column)).Where(value => value > 0));
        var highest = used.Count == 0 ? 0L : used.Max();
        var low = descriptor == null ? 1L : Math.Max(1L, descriptor.RangeLow);
        var high = descriptor == null
            ? Math.Max(low, Math.Min(int.MaxValue, highest + 10000L))
            : Math.Min(int.MaxValue, descriptor.RangeHigh);
        if (high < low) throw new InvalidOperationException("No free " + fieldName + " remains in the FC26 table.");

        // Prefer an append id (which keeps the database diff compact), then
        // wrap once to recover a genuine hole if an imported table is full at
        // its high end.  Every candidate is checked against all loaded and
        // staged rows, so duplicate IDs cannot be generated by repeated saves.
        var preferred = Math.Max(low, highest + 1L);
        var candidate = FindFreeDetailKey(used, preferred, high);
        if (candidate < 0 && preferred > low)
            candidate = FindFreeDetailKey(used, low, preferred - 1L);
        if (candidate < 0)
            throw new InvalidOperationException("No free " + fieldName + " remains in the FC26 table.");
        return candidate;
    }

    private static int FindFreeDetailKey(HashSet<int> used, long low, long high)
    {
        for (var value = low; value <= high; value++)
        {
            if (value <= int.MaxValue && !used.Contains((int)value)) return (int)value;
            if (value == long.MaxValue) break;
        }
        return -1;
    }

    internal static SnapshotDetailTable? DetailTable(string tableName)
    {
        var table = s_snapshot?.Tables.FirstOrDefault(value =>
            value.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (table != null) EnsureRows(table, s_snapshotPath);
        return table == null ? null : new SnapshotDetailTable(table.Name, table.Columns, table.ColumnDetails, table.Rows);
    }

    internal static IReadOnlyList<string> DetailTableNames => s_snapshot?.Tables
        .Select(table => table.Name)
        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
        .ToArray() ?? Array.Empty<string>();

    internal static bool IsLoaded => s_snapshot != null;

    internal static string CurrentGameRoot => s_snapshot?.GameRoot ?? string.Empty;

    internal static string CurrentDatabaseFolder => s_snapshot?.DatabaseFolder ?? string.Empty;

    internal static int CurrentTableCount => s_snapshot?.Tables.Count ?? 0;

    internal static long CurrentRowCount => s_snapshot?.Tables.Sum(table =>
        (long)(table.RowCount > 0 ? table.RowCount : table.Rows.Count)) ?? 0L;

    internal static string DescribeLoadedSource()
    {
        if (s_snapshot == null) return "No FC26 data is loaded.";
        var source = string.IsNullOrWhiteSpace(s_snapshot.GameRoot)
            ? s_snapshot.DatabaseFolder
            : s_snapshot.GameRoot;
        return CurrentTableCount.ToString("N0", CultureInfo.CurrentCulture) + " table(s), " +
            CurrentRowCount.ToString("N0", CultureInfo.CurrentCulture) + " row(s)\r\n" + source;
    }

    /// <summary>
    /// Human-readable ID inventory for the writable FC26 tables.  The report
    /// deliberately uses compact ranges instead of dumping thousands of
    /// individual values, so it can be shown in the legacy diagnostics window
    /// and copied into a support ticket.  The playernames line is also the
    /// source of truth used by the automatic edited-name allocator.
    /// </summary>
    internal static string DescribeIdAvailability()
    {
        if (s_snapshot == null) return "ID availability: no FC26 data is loaded.";
        var output = new StringBuilder();
        output.AppendLine("FC26 ID availability (loaded database)");
        output.AppendLine(new string('-', 72));
        var tables = new (string Table, string IdColumn)[]
        {
            ("playernames", "nameid"), ("dcplayernames", "nameid"),
            ("players", "playerid"), ("teams", "teamid"),
            ("leagues", "leagueid"), ("nations", "nationid"),
            ("manager", "managerid"), ("stadiums", "stadiumid"),
            ("teamkits", "teamkitid"), ("referee", "refereeid")
        };
        foreach (var item in tables)
        {
            var table = s_snapshot.Tables.FirstOrDefault(value =>
                value.Name.Equals(item.Table, StringComparison.OrdinalIgnoreCase));
            if (table == null) continue;
            EnsureRows(table, s_snapshotPath);
            var idColumn = Column(table, item.IdColumn);
            if (idColumn < 0) continue;
            var descriptor = table.ColumnDetails.FirstOrDefault(value =>
                value.Name.Equals(item.IdColumn, StringComparison.OrdinalIgnoreCase));
            var used = new HashSet<int>(table.Rows.Select(row => ParseIntAt(row, idColumn)).Where(value => value > 0));
            var maximum = used.Count == 0 ? 0L : used.Max();
            var low = descriptor == null ? 1L : Math.Max(1L, descriptor.RangeLow);
            var high = descriptor == null
                ? Math.Max(low, Math.Min(int.MaxValue, maximum + 10000L))
                : Math.Min(int.MaxValue, descriptor.RangeHigh);
            if (high < low) high = Math.Max(low, maximum);
            var occupied = used.LongCount(value => value >= low && value <= high);
            var capacity = high - low + 1L;
            var free = Math.Max(0L, capacity - occupied);
            var ranges = FormatFreeIdRanges(used, low, high, 10);
            output.AppendLine(item.Table + "." + item.IdColumn + ": " +
                "used " + occupied.ToString("N0", CultureInfo.CurrentCulture) + "/" +
                capacity.ToString("N0", CultureInfo.CurrentCulture) + ", free " +
                free.ToString("N0", CultureInfo.CurrentCulture) +
                (free == 0 ? " (none)" : " (" + ranges + ")"));
        }
        return output.ToString().TrimEnd();
    }

    internal static string DescribePlayerNameAvailability()
    {
        if (s_snapshot == null) return "Player-name IDs: no FC26 data is loaded.";
        var all = DescribeIdAvailability();
        var lines = all.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(value => value.StartsWith("playernames.", StringComparison.OrdinalIgnoreCase) ||
                            value.StartsWith("dcplayernames.", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return lines.Length == 0 ? "Player-name IDs: playernames tables are unavailable." :
            "Player-name IDs are allocated automatically; original shared rows are never overwritten.\r\n" +
            string.Join("\r\n", lines);
    }

    /// <summary>Fails before a guided creator mutates the in-memory graph when
    /// the loaded schema lacks a template or enough identity values. This keeps
    /// predictable capacity failures out of the middle of a multi-row wizard.</summary>
    internal static void ValidateCreationCapacity(int leagueCount, int teamCount, int playerCount)
    {
        if (s_snapshot == null) throw new InvalidOperationException("Open FC26 before creating records.");
        var requirements = new[]
        {
            new { Table = "leagues", Id = "leagueid", Needed = Math.Max(0, leagueCount) },
            new { Table = "teams", Id = "teamid", Needed = Math.Max(0, teamCount) },
            new { Table = "players", Id = "playerid", Needed = Math.Max(0, playerCount) }
        };
        foreach (var requirement in requirements.Where(value => value.Needed > 0))
        {
            var table = s_snapshot.Tables.FirstOrDefault(value => value.Name.Equals(requirement.Table, StringComparison.OrdinalIgnoreCase));
            if (table != null) EnsureRows(table, s_snapshotPath);
            if (table == null || table.Rows.Count == 0)
                throw new InvalidOperationException("The FC26 " + requirement.Table + " table has no safe creation template.");
            var column = Column(table, requirement.Id);
            if (column < 0) throw new InvalidOperationException("The FC26 " + requirement.Table + " identity field is unavailable.");
            var descriptor = table.ColumnDetails.FirstOrDefault(value => value.Name.Equals(requirement.Id, StringComparison.OrdinalIgnoreCase));
            if (descriptor == null) continue;
            var used = new HashSet<int>(table.Rows.Select(row => ParseIntAt(row, column)));
            long free = 0;
            for (var id = Math.Max(1L, descriptor.RangeLow); id <= Math.Min(int.MaxValue, descriptor.RangeHigh) && free < requirement.Needed; id++)
                if (!used.Contains((int)id)) free++;
            if (free < requirement.Needed)
                throw new InvalidOperationException("Not enough unused " + requirement.Id + " values remain. Needed " + requirement.Needed + ", available " + free + ".");
        }
        foreach (var tableName in new[] { "leagueteamlinks", "teamplayerlinks", "playernames" })
        {
            if ((tableName == "leagueteamlinks" && teamCount == 0) ||
                (tableName == "teamplayerlinks" && playerCount == 0) ||
                (tableName == "playernames" && playerCount == 0)) continue;
            var table = s_snapshot.Tables.FirstOrDefault(value => value.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (table != null) EnsureRows(table, s_snapshotPath);
            if (table == null || table.Rows.Count == 0)
                throw new InvalidOperationException("The FC26 " + tableName + " table has no safe creation template.");
        }
    }

    private static string FormatFreeIdRanges(HashSet<int> used, long low, long high, int maxRanges)
    {
        var ranges = new List<string>();
        var cursor = low;
        foreach (var id in used.Where(value => value >= low && value <= high).OrderBy(value => value))
        {
            if (id > cursor)
            {
                ranges.Add(FormatIdRange(cursor, id - 1L));
                if (ranges.Count >= maxRanges) return string.Join(", ", ranges) + ", ...";
            }
            cursor = Math.Max(cursor, (long)id + 1L);
            if (cursor > high) break;
        }
        if (cursor <= high) ranges.Add(FormatIdRange(cursor, high));
        return ranges.Count == 0 ? "none" : string.Join(", ", ranges);
    }

    private static string FormatIdRange(long first, long last) => first == last
        ? first.ToString(CultureInfo.InvariantCulture)
        : first.ToString(CultureInfo.InvariantCulture) + "-" + last.ToString(CultureInfo.InvariantCulture);

    internal static string CompareWithSnapshot(string path)
    {
        var current = s_snapshot ?? throw new InvalidOperationException("No FC26 database is loaded.");
        var other = ReadSnapshot(path);
        var otherPath = Path.GetFullPath(path);
        var output = new StringBuilder();
        var changedTables = 0; long changedCells = 0;
        var otherTables = other.Tables.ToDictionary(table => table.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var table in current.Tables.OrderBy(value => value.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (!otherTables.TryGetValue(table.Name, out var compared))
            {
                output.AppendLine(table.Name + ": missing from comparison database"); changedTables++; continue;
            }
            var differences = 0L;
            if (!table.Columns.SequenceEqual(compared.Columns, StringComparer.OrdinalIgnoreCase))
            {
                output.AppendLine(table.Name + ": schema/field order differs"); changedTables++; continue;
            }
            var currentWasLoaded = table.RowsLoaded;
            EnsureRows(table, s_snapshotPath);
            EnsureRows(compared, otherPath);
            var rows = Math.Max(table.Rows.Count, compared.Rows.Count);
            for (var row = 0; row < rows; row++)
                for (var column = 0; column < table.Columns.Length; column++)
                {
                    var left = row < table.Rows.Count && column < table.Rows[row].Length ? table.Rows[row][column] : "<missing>";
                    var right = row < compared.Rows.Count && column < compared.Rows[row].Length ? compared.Rows[row][column] : "<missing>";
                    if (!string.Equals(left, right, StringComparison.Ordinal)) differences++;
                }
            if (differences != 0)
            {
                changedTables++; changedCells += differences;
                output.AppendLine(table.Name + ": " + differences.ToString("N0", CultureInfo.InvariantCulture) + " different cell(s), " +
                    table.Rows.Count.ToString("N0", CultureInfo.InvariantCulture) + " vs " + compared.Rows.Count.ToString("N0", CultureInfo.InvariantCulture) + " rows");
            }
            if (!currentWasLoaded) UnloadRows(table);
            UnloadRows(compared);
        }
        foreach (var table in other.Tables.Where(value => current.Tables.All(existing => !existing.Name.Equals(value.Name, StringComparison.OrdinalIgnoreCase))))
        { output.AppendLine(table.Name + ": only in comparison database"); changedTables++; }
        return changedTables == 0 ? "Databases match across all exported main and locale tables and cells." :
            changedTables.ToString("N0", CultureInfo.InvariantCulture) + " changed table(s), " + changedCells.ToString("N0", CultureInfo.InvariantCulture) +
            " changed cell(s).\r\n\r\n" + output;
    }

    internal static int PendingDetailCount => s_detailChanges.Count + s_structuralChanges.Count;

    internal static string CurrentSnapshotPath => s_snapshotPath;

    internal static bool IsSchemaCompatible(out string report)
    {
        if (s_snapshot == null)
        {
            report = "No FC26 snapshot is loaded.";
            return false;
        }

        var missing = new List<string>();
        var tables = s_snapshot.Tables.ToDictionary(table => table.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var requirement in s_requiredSchema)
        {
            if (!tables.TryGetValue(requirement.Key, out var table))
            {
                missing.Add(requirement.Key + " (table)");
                continue;
            }
            foreach (var field in requirement.Value)
                if (!table.Columns.Contains(field, StringComparer.OrdinalIgnoreCase))
                    missing.Add(requirement.Key + "." + field);
        }

        var fingerprint = SchemaFingerprint();
        var verified = s_verifiedSchemaFingerprints.Contains(fingerprint);
        report = (missing.Count == 0
            ? "Required direct-edit tables and fields are present."
            : "Missing required schema item(s): " + string.Join(", ", missing)) +
            " Fingerprint: " + fingerprint + (verified ? " (verified FC26 schema)." :
            " (unknown Title Update schema; direct Save is read-only until verified).");
        return missing.Count == 0 && verified;
    }

    internal static string DescribeCompatibility()
    {
        var compatible = IsSchemaCompatible(out var result);
        var builder = new StringBuilder();
        builder.AppendLine("CM26 FC26 SCHEMA COMPATIBILITY");
        builder.AppendLine(new string('=', 34));
        builder.AppendLine("State: " + (compatible ? "COMPATIBLE" : "BLOCKED"));
        builder.AppendLine(result);
        builder.AppendLine();
        builder.AppendLine("This fingerprint identifies the loaded table/field layout. CM26 blocks direct Save when required FC26 relationships are missing.");
        return builder.ToString();
    }

    internal static string SchemaFingerprint()
    {
        if (s_snapshot == null) return "none";
        var canonical = string.Join("\n", s_snapshot.Tables
            .OrderBy(table => table.Name, StringComparer.OrdinalIgnoreCase)
            .Select(table => table.Name.ToLowerInvariant() + "|" + string.Join(",", table.Columns.Select(column => column.ToLowerInvariant()))));
        using (var sha = SHA256.Create())
        {
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
            return string.Concat(hash.Take(8).Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }

    internal static bool IsDetailDeleted(string tableName, int rowIndex) => s_structuralChanges.Any(change =>
        change.Kind == "delete" && change.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) && change.RowIndex == rowIndex);

    internal static int DuplicateDetailRow(string tableName, int rowIndex)
    {
        if (s_structuralChanges.Any(change => change.Kind == "delete"))
            throw new InvalidOperationException("Save the pending deletion before cloning another record.");
        var table = s_snapshot?.Tables.FirstOrDefault(candidate => candidate.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (table != null) EnsureRows(table, s_snapshotPath);
        if (table == null || rowIndex < 0 || rowIndex >= table.Rows.Count) throw new InvalidOperationException("Selected record is unavailable.");
        var newIndex = table.Rows.Count;
        table.Rows.Add((string[])table.Rows[rowIndex].Clone());
        s_structuralChanges.Add(new StructuralChange
        {
            Kind = "duplicate",
            TableName = table.Name,
            RowIndex = rowIndex,
            TargetRowIndex = newIndex
        });
        if (table.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase) && s_playerNameIndexReady)
            RegisterPlayerNameRow(new SnapshotDetailTable(table.Name, table.Columns, table.ColumnDetails, table.Rows), newIndex);
        Fc26ActivityLog.Add("Clone row", table.Name + "[" + rowIndex + "] → staged row " + newIndex);
        return newIndex;
    }

    internal static void DeleteDetailRow(string tableName, int rowIndex)
    {
        if (s_detailChanges.Count > 0 || s_structuralChanges.Any(change => change.Kind != "delete"))
            throw new InvalidOperationException("Save or close the current staged field/clone edits first. Dependency-cleaned deletions are isolated so row indexes cannot shift under other edits.");
        var table = s_snapshot?.Tables.FirstOrDefault(candidate => candidate.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (table != null) EnsureRows(table, s_snapshotPath);
        if (table == null || rowIndex < 0 || rowIndex >= table.Rows.Count) throw new InvalidOperationException("Selected record is unavailable.");
        if (IsDetailDeleted(tableName, rowIndex)) return;
        s_structuralChanges.Add(new StructuralChange { Kind = "delete", TableName = table.Name, RowIndex = rowIndex });
        Fc26ActivityLog.Add("Delete row", table.Name + "[" + rowIndex + "] staged for dependency-aware deletion");
    }

    internal static bool IsDetailChanged(string tableName, int rowIndex, string fieldName) =>
        s_detailChanges.ContainsKey(tableName + "\u001f" + rowIndex.ToString(CultureInfo.InvariantCulture) + "\u001f" + fieldName);

    internal static void StageDetailValue(string tableName, int rowIndex, string fieldName, string value)
    {
        var table = s_snapshot?.Tables.FirstOrDefault(candidate =>
            candidate.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (table != null) EnsureRows(table, s_snapshotPath);
        if (table == null || rowIndex < 0 || rowIndex >= table.Rows.Count)
            throw new InvalidOperationException("The selected detail record is unavailable.");
        if (IsDetailDeleted(tableName, rowIndex)) throw new InvalidOperationException("The selected record is staged for deletion.");
        var column = Column(table, fieldName);
        if (column < 0 || column >= table.Rows[rowIndex].Length)
            throw new InvalidOperationException("The selected detail field is unavailable.");
        var isPlayerName = table.Name.Equals("playernames", StringComparison.OrdinalIgnoreCase);
        if (isPlayerName && s_playerNameIndexReady)
            RegisterPlayerNameRow(new SnapshotDetailTable(table.Name, table.Columns, table.ColumnDetails, table.Rows), rowIndex);
        var key = table.Name + "\u001f" + rowIndex.ToString(CultureInfo.InvariantCulture) + "\u001f" + fieldName;
        if (!s_detailOriginalValues.TryGetValue(key, out var original))
        {
            original = table.Rows[rowIndex][column] ?? string.Empty;
            s_detailOriginalValues[key] = original;
        }
        if (string.Equals(original, value ?? string.Empty, StringComparison.Ordinal))
        {
            s_detailChanges.Remove(key);
            table.Rows[rowIndex][column] = original;
            return;
        }
        s_detailChanges[key] = new Change
        {
            TableName = table.Name,
            RowIndex = rowIndex,
            FieldName = table.Columns[column],
            Value = value ?? string.Empty
        };
        // Keep the in-memory view in sync so closing and reopening a details
        // page shows the staged value instead of the old snapshot text.
        table.Rows[rowIndex][column] = value ?? string.Empty;
        if (isPlayerName && s_playerNameIndexReady)
            RegisterPlayerNameRow(new SnapshotDetailTable(table.Name, table.Columns, table.ColumnDetails, table.Rows), rowIndex);
        Fc26ActivityLog.Add("Edit", table.Name + "[" + rowIndex + "]." + fieldName + " staged");
    }

    private static void AppendDetailChanges(List<Change> changes)
    {
        foreach (var staged in s_detailChanges.Values)
        {
            changes.RemoveAll(existing =>
                existing.RowIndex == staged.RowIndex &&
                existing.TableName.Equals(staged.TableName, StringComparison.OrdinalIgnoreCase) &&
                existing.FieldName.Equals(staged.FieldName, StringComparison.OrdinalIgnoreCase));
            changes.Add(staged);
        }
    }

    private static void AppendFc26TacticMirrorChanges(Snapshot snapshot, List<Change> changes)
    {
        if (FifaEnvironment.Teams == null) return;
        var teamsTable = snapshot.Tables.FirstOrDefault(value =>
            value.Name.Equals("teams", StringComparison.OrdinalIgnoreCase));
        if (teamsTable == null) return;

        var teamsId = Column(teamsTable, "teamid");
        var teamsBuild = Column(teamsTable, "buildupplay");
        var teamsDepth = Column(teamsTable, "defensivedepth");
        var originalRows = teamsTable.Rows
            .Select((row, index) => new { Row = row, Index = index })
            .Where(value => teamsId >= 0)
            .GroupBy(value => ParseIntAt(value.Row, teamsId))
            .ToDictionary(group => group.Key, group => group.First().Row);

        var mentalities = snapshot.Tables.FirstOrDefault(value =>
            value.Name.Equals("default_mentalities", StringComparison.OrdinalIgnoreCase));
        var defaultTeamData = snapshot.Tables.FirstOrDefault(value =>
            value.Name.Equals("defaultteamdata", StringComparison.OrdinalIgnoreCase));

        foreach (Team team in FifaEnvironment.Teams)
        {
            if (!originalRows.TryGetValue(team.Id, out var original)) continue;
            var buildChanged = teamsBuild >= 0 && ParseIntAt(original, teamsBuild) != team.buildupplay;
            var depthChanged = teamsDepth >= 0 && ParseIntAt(original, teamsDepth) != team.defensivedepth;
            if (!buildChanged && !depthChanged) continue;

            if (mentalities != null)
            {
                var teamId = Column(mentalities, "teamid");
                var build = Column(mentalities, "buildupplay");
                var depth = Column(mentalities, "defensivedepth");
                var activeRow = -1;
                for (var row = 0; row < mentalities.Rows.Count; row++)
                {
                    var values = mentalities.Rows[row];
                    if (ParseIntAt(values, teamId) != team.Id) continue;
                    var buildValue = ParseIntAt(values, build);
                    var depthValue = ParseIntAt(values, depth);
                    if (buildValue >= 1 && buildValue <= 3 && depthValue >= 2 && depthValue <= 100)
                    {
                        activeRow = row;
                        break;
                    }
                    if (activeRow < 0 && (buildValue > 0 || depthValue > 1)) activeRow = row;
                }
                if (activeRow >= 0)
                {
                    if (buildChanged) AddMirrorChange(mentalities, activeRow, build, team.buildupplay, changes);
                    if (depthChanged) AddMirrorChange(mentalities, activeRow, depth, team.defensivedepth, changes);
                }
            }

            if (depthChanged && defaultTeamData != null)
            {
                var teamId = Column(defaultTeamData, "teamid");
                var depth = Column(defaultTeamData, "defensivedepth");
                for (var row = 0; row < defaultTeamData.Rows.Count; row++)
                {
                    if (ParseIntAt(defaultTeamData.Rows[row], teamId) != team.Id) continue;
                    AddMirrorChange(defaultTeamData, row, depth, team.defensivedepth, changes);
                    break;
                }
            }
        }
    }

    private static void AppendDefaultTeamSheetChanges(Snapshot snapshot, List<Change> changes)
    {
        if (FifaEnvironment.Teams == null) return;
        var table = snapshot.Tables.FirstOrDefault(value =>
            value.Name.Equals("default_teamsheets", StringComparison.OrdinalIgnoreCase));
        if (table == null) return;

        var teamIdColumn = Column(table, "teamid");
        var tacticIdColumn = Column(table, "tacticid");
        if (teamIdColumn < 0) return;

        var activeRows = table.Rows
            .Select((row, index) => new
            {
                TeamId = ParseIntAt(row, teamIdColumn),
                TacticId = ParseIntAt(row, tacticIdColumn),
                Index = index
            })
            .GroupBy(value => value.TeamId)
            .ToDictionary(group => group.Key,
                group => group.OrderBy(value => value.TacticId == 0 ? 0 : 1)
                    .ThenBy(value => value.Index).First().Index);

        var mentalities = snapshot.Tables.FirstOrDefault(value =>
            value.Name.Equals("default_mentalities", StringComparison.OrdinalIgnoreCase));
        var mentalityTeamId = mentalities == null ? -1 : Column(mentalities, "teamid");

        foreach (Team team in FifaEnvironment.Teams)
        {
            if (!activeRows.TryGetValue(team.Id, out var rowIndex)) continue;
            var original = table.Rows[rowIndex];
            var roster = team.Roster.Cast<TeamPlayer>()
                .Where(value => value?.Player != null)
                .GroupBy(value => value.Player.Id)
                .Select(group => group.First())
                .ToList();

            s_loadedTeamSheets.TryGetValue(team, out var loadedSheet);
            var rosterChanged = loadedSheet == null || !roster.Select(value => value.Player.Id)
                .SequenceEqual(loadedSheet.PlayerIds);

            for (var slot = 0; rosterChanged && slot < 52; slot++)
            {
                var playerId = slot < roster.Count ? roster[slot].Player.Id : -1;
                var playerColumn = Column(table, "playerid" + slot);
                var previousPlayerId = ParseIntAt(original, playerColumn);
                AddFormationRoleChange(table, rowIndex, "playerid" + slot,
                    playerId, typeof(int), changes);

                // FC26 keeps the active XI in both tables. Mirror only a genuine
                // replacement of the same slot, preserving alternate mentalities.
                if (slot >= 11 || mentalities == null || mentalityTeamId < 0 ||
                    previousPlayerId <= 0 || playerId <= 0 || previousPlayerId == playerId) continue;
                var mentalityPlayer = Column(mentalities, "playerid" + slot);
                if (mentalityPlayer < 0) continue;
                for (var mentalityRow = 0; mentalityRow < mentalities.Rows.Count; mentalityRow++)
                {
                    var values = mentalities.Rows[mentalityRow];
                    if (ParseIntAt(values, mentalityTeamId) != team.Id ||
                        ParseIntAt(values, mentalityPlayer) != previousPlayerId) continue;
                    AddFormationRoleChange(mentalities, mentalityRow, "playerid" + slot,
                        playerId, typeof(int), changes);
                }
            }

            // The formations table stores precise floating-point coordinates while
            // the legacy UI exposes integer percentages.  Do not mirror an
            // untouched layout into the teamsheet because that would quantise it
            // merely by opening and saving a project.
            if (team.Formation?.PlayingRoles != null && FormationLayoutChanged(team.Formation))
            {
                AddFormationRoleChange(table, rowIndex, "formationaudioid",
                    team.Formation.formationaudioid, typeof(int), changes);
                for (var slot = 0; slot < Math.Min(11, team.Formation.PlayingRoles.Length); slot++)
                {
                    var role = team.Formation.PlayingRoles[slot];
                    if (role == null) continue;
                    AddFormationRoleChange(table, rowIndex, "position" + slot,
                        role.Role?.Id ?? role.Id, typeof(int), changes);
                    AddFormationRoleChange(table, rowIndex, "offset" + slot + "x",
                        role.OffsetX / 100f, typeof(float), changes);
                    AddFormationRoleChange(table, rowIndex, "offset" + slot + "y",
                        role.OffsetY / 100f, typeof(float), changes);
                    AddFormationRoleChange(table, rowIndex, "playerinstruction" + slot + "_1",
                        role.PlayerInstruction_1, typeof(int), changes);
                    AddFormationRoleChange(table, rowIndex, "playerinstruction" + slot + "_2",
                        role.PlayerInstruction_2, typeof(int), changes);
                }
            }

            var integerFields = new (string Name, int Value)[]
            {
                ("busbuildupspeed", team.busbuildupspeed),
                ("busdribbling", team.busdribbling),
                ("buspassing", team.buspassing),
                ("buspositioning", team.buspositioning),
                ("cccrossing", team.cccrossing),
                ("ccpassing", team.ccpassing),
                ("ccpositioning", team.ccpositioning),
                ("ccshooting", team.ccshooting),
                ("defaggression", team.defaggression),
                ("defdefenderline", team.defdefenderline),
                ("defmentality", team.defmentality),
                ("defteamwidth", team.defteamwidth),
                ("captainid", team.captainid),
                ("freekicktakerid", team.freekicktakerid),
                ("leftcornerkicktakerid", team.leftcornerkicktakerid),
                ("leftfreekicktakerid", team.leftfreekicktakerid),
                ("longkicktakerid", team.longkicktakerid),
                ("penaltytakerid", team.penaltytakerid),
                ("rightcornerkicktakerid", team.rightcornerkicktakerid),
                ("rightfreekicktakerid", team.rightfreekicktakerid)
            };
            foreach (var field in integerFields)
            {
                if (loadedSheet != null && loadedSheet.IntegerValues.TryGetValue(field.Name, out var loaded) &&
                    loaded == field.Value) continue;
                AddFormationRoleChange(table, rowIndex, field.Name, field.Value, typeof(int), changes);
            }
        }
    }

    private static void AddMirrorChange(TableSnapshot table, int rowIndex, int columnIndex,
        int value, List<Change> changes)
    {
        if (columnIndex < 0 || rowIndex < 0 || rowIndex >= table.Rows.Count) return;
        var text = value.ToString(CultureInfo.InvariantCulture);
        if (string.Equals(table.Rows[rowIndex][columnIndex], text, StringComparison.Ordinal)) return;
        changes.Add(new Change
        {
            TableName = table.Name,
            RowIndex = rowIndex,
            FieldName = table.Columns[columnIndex],
            Value = text
        });
    }

    private static void AppendFormationRoleChanges(Snapshot snapshot, List<Change> changes)
    {
        if (FifaEnvironment.Formations == null) return;
        var table = snapshot.Tables.FirstOrDefault(value =>
            value.Name.Equals("formations", StringComparison.OrdinalIgnoreCase));
        if (table == null) return;

        var idColumn = Column(table, "formationid");
        if (idColumn < 0) return;

        var rowsById = table.Rows
            .Select((row, index) => new { Id = ParseIntAt(row, idColumn), Index = index })
            .GroupBy(value => value.Id)
            .ToDictionary(group => group.Key, group => group.First().Index);

        foreach (Formation formation in FifaEnvironment.Formations)
        {
            var origin = Origins(formation).FirstOrDefault(value =>
                value.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase));
            var rowIndex = origin?.RowIndex ??
                (rowsById.TryGetValue(formation.Id, out var mappedRow) ? mappedRow : -1);
            if (rowIndex < 0 || rowIndex >= table.Rows.Count || formation.PlayingRoles == null) continue;

            for (var index = 0; index < Math.Min(11, formation.PlayingRoles.Length); index++)
            {
                var role = formation.PlayingRoles[index];
                if (role == null) continue;
                var current = FormationRoleState.From(role);
                FormationRoleState? loaded = null;
                if (s_loadedFormationRoles.TryGetValue(formation, out var loadedRoles) &&
                    index < loadedRoles.Length)
                    loaded = loadedRoles[index];

                if (!loaded.HasValue || current.Position != loaded.Value.Position)
                    AddFormationRoleChange(table, rowIndex, "position" + index,
                        current.Position, typeof(int), changes);
                if (!loaded.HasValue || current.OffsetX != loaded.Value.OffsetX)
                    AddFormationRoleChange(table, rowIndex, "offset" + index + "x",
                        current.OffsetX / 100f, typeof(float), changes);
                if (!loaded.HasValue || current.OffsetY != loaded.Value.OffsetY)
                    AddFormationRoleChange(table, rowIndex, "offset" + index + "y",
                        current.OffsetY / 100f, typeof(float), changes);
                if (!loaded.HasValue || current.Instruction1 != loaded.Value.Instruction1)
                    AddFormationRoleChange(table, rowIndex, "playerinstruction" + index + "_1",
                        current.Instruction1, typeof(int), changes);
                if (!loaded.HasValue || current.Instruction2 != loaded.Value.Instruction2)
                    AddFormationRoleChange(table, rowIndex, "playerinstruction" + index + "_2",
                        current.Instruction2, typeof(int), changes);
            }
        }
    }

    private static bool FormationLayoutChanged(Formation formation)
    {
        if (formation.PlayingRoles == null ||
            !s_loadedFormationRoles.TryGetValue(formation, out var loadedRoles))
            return true;

        var count = Math.Min(11, formation.PlayingRoles.Length);
        if (count != loadedRoles.Length) return true;
        for (var index = 0; index < count; index++)
        {
            var current = FormationRoleState.From(formation.PlayingRoles[index]);
            var loaded = loadedRoles[index];
            if (current.Position != loaded.Position ||
                current.OffsetX != loaded.OffsetX ||
                current.OffsetY != loaded.OffsetY ||
                current.Instruction1 != loaded.Instruction1 ||
                current.Instruction2 != loaded.Instruction2)
                return true;
        }
        return false;
    }

    private static void AddFormationRoleChange(TableSnapshot table, int rowIndex,
        string fieldName, object value, Type type, List<Change> changes)
    {
        var columnIndex = Column(table, fieldName);
        if (columnIndex < 0 || rowIndex < 0 || rowIndex >= table.Rows.Count) return;
        var current = ToDatabaseText(value, type);
        if (DatabaseEquals(table.Rows[rowIndex][columnIndex], current, type)) return;

        var existing = changes.FirstOrDefault(change =>
            change.RowIndex == rowIndex &&
            change.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase) &&
            change.FieldName.Equals(table.Columns[columnIndex], StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Value = current;
            return;
        }
        changes.Add(new Change
        {
            TableName = table.Name,
            RowIndex = rowIndex,
            FieldName = table.Columns[columnIndex],
            Value = current
        });
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
                    $"FC26 player-name ID {nameId} is shared by different names ('{existing}' and '{desired}'). " +
                    "The edit could not be separated automatically; use a fresh player-name ID from the " +
                    "ID Availability report before saving.");
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
        foreach (Formation formation in formations)
        {
            if (formation.PlayingRoles == null) continue;
            s_loadedFormationRoles[formation] = formation.PlayingRoles
                .Take(11)
                .Select(FormationRoleState.From)
                .ToArray();
        }
    }

    private readonly struct FormationRoleState
    {
        internal int Position { get; }
        internal int OffsetX { get; }
        internal int OffsetY { get; }
        internal int Instruction1 { get; }
        internal int Instruction2 { get; }

        private FormationRoleState(int position, int offsetX, int offsetY,
            int instruction1, int instruction2)
        {
            Position = position;
            OffsetX = offsetX;
            OffsetY = offsetY;
            Instruction1 = instruction1;
            Instruction2 = instruction2;
        }

        internal static FormationRoleState From(PlayingRole? role)
        {
            return role == null
                ? new FormationRoleState(-1, 0, 0, 0, 0)
                : new FormationRoleState(role.Role?.Id ?? role.Id,
                    role.OffsetX, role.OffsetY,
                    role.PlayerInstruction_1, role.PlayerInstruction_2);
        }
    }

    private static CompobjList BuildCompetitions(Dictionary<string, TableSnapshot> tables)
    {
        var result = new CompobjList();
        var world = new World(0, "FC26", "FC26 Competitions");
        result.Add(world);
        if (!tables.TryGetValue("competition", out var table)) return result;
        var leagueNames = new Dictionary<int, string>();
        if (tables.TryGetValue("leagues", out var leagues))
        {
            var leagueId = Column(leagues, "leagueid");
            var leagueName = Column(leagues, "leaguename");
            foreach (var row in leagues.Rows)
            {
                var id = ParseIntAt(row, leagueId);
                if (leagueName >= 0 && leagueName < row.Length && !string.IsNullOrWhiteSpace(row[leagueName]))
                    leagueNames[id] = row[leagueName].Trim();
            }
        }
        var idColumn = Column(table, "competitionid");
        var ballColumn = Column(table, "ballid");
        var womenColumn = Column(table, "iswomencompetition");
        var countryColumn = Column(table, "country_lock");
        foreach (var row in table.Rows)
        {
            var competitionId = ParseIntAt(row, idColumn);
            string displayName;
            if (!leagueNames.TryGetValue(competitionId, out displayName!))
            {
                bool women = ParseIntAt(row, womenColumn) != 0;
                bool international = ParseIntAt(row, countryColumn) < 0;
                displayName = international
                    ? (women ? "Women's International Tournament" : "International Tournament")
                    : (women ? "Women's Cup Competition" : "Cup Competition");
            }
            var trophy = new Trophy(competitionId + 1, displayName, displayName, world)
            {
                LongName = displayName,
                ShortName = displayName,
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
        if (list is IdArrayList ids)
        {
            ids.SortId();
            var usedMaximum = table.Rows.Select(row => ParseIntAt(row, idIndex)).DefaultIfEmpty(-1).Max();
            var descriptor = table.ColumnDetails.FirstOrDefault(column =>
                column.Name.Equals(idColumn, StringComparison.OrdinalIgnoreCase));
            var rangeHigh = descriptor == null ? Math.Max(usedMaximum + 10000L, int.MaxValue / 4L) : descriptor.RangeHigh;
            ids.MinId = usedMaximum < int.MaxValue ? Math.Max(0, usedMaximum + 1) : int.MaxValue;
            ids.MaxId = (int)Math.Max(ids.MinId, Math.Min(int.MaxValue, rangeHigh));
        }
        return list;
    }

    private static void MapFields(object target, string[] columns, string[] values)
    {
        var fields = AllFields(target.GetType())
            .GroupBy(f => Normalize(f.Name)).ToDictionary(g => g.Key, g => g.First());
        for (var i = 0; i < columns.Length && i < values.Length; i++)
        {
            var columnName = Normalize(columns[i]);
            // TeamPlayer already receives its linked Team and Player objects in
            // LinkCore. The numeric foreign keys are resolved only for change-plan
            // serialization; assigning an Int32 to those reference fields converts
            // to null and leaves a roster full of empty placeholder entries.
            if (target is TeamPlayer && (columnName == "teamid" || columnName == "playerid"))
                continue;
            if (!TryResolveField(target.GetType(), fields, columnName, out var field)) continue;
            try
            {
                var value = ConvertValue(values[i], field.FieldType);
                if (target is Player && columnName == "preferredfoot" && value is int preferredFoot)
                    value = preferredFoot <= 1 ? 0 : 1;
                // FC26 stores confederations as 1..7, while the legacy Country
                // object and its combo box use the zero-based 0..6 enum.  The
                // normal database constructor performs this conversion too;
                // snapshot loading must preserve the same contract.
                if (target is Country && columnName == "confederation" && value is int confederation)
                    value = Math.Max(0, confederation - 1);
                field.SetValue(target, value);
            }
            catch { /* New FC26-only columns stay in the snapshot and are ignored by CM16 forms. */ }
        }
        if (target is Country country)
        {
            country.LanguageName = country.DatabaseName;
            country.LanguageShortName = country.DatabaseName;
        }
        if (target is Team team) team.SyncFc26SnapshotFields();
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
        if (targetType == typeof(TeamPlayer))
        {
            // teamplayerlinks stores ids while the legacy object keeps linked
            // Team/Player objects. Resolve these explicitly so moving an existing
            // player to another club is emitted as a teamid change on save.
            if (columnName == "teamid" && fields.TryGetValue("team", out field!)) return true;
            if (columnName == "playerid" && fields.TryGetValue("player", out field!)) return true;
        }
        field = null!;
        return false;
    }

    private static string ToDatabaseTextForColumn(object item, FieldInfo field, string columnName)
    {
        if (item is TeamPlayer teamPlayer)
        {
            if (columnName == "teamid")
                return (teamPlayer.Team?.Id ?? 0).ToString(CultureInfo.InvariantCulture);
            if (columnName == "playerid")
                return (teamPlayer.Player?.Id ?? 0).ToString(CultureInfo.InvariantCulture);
        }
        var value = field.GetValue(item);
        if (item is Player && columnName == "preferredfoot" && value is int preferredFoot)
            value = (preferredFoot <= 0 ? 0 : 1) + 1;
        if (item is Country && columnName == "confederation" && value is int confederation)
            value = confederation + 1;
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

            s_loadedPlayerNameStates[player] = new PlayerNameState
            {
                FirstNameId = player.firstnameid,
                FirstName = player.firstname ?? string.Empty,
                LastNameId = player.lastnameid,
                LastName = player.lastname ?? string.Empty,
                CommonNameId = player.commonnameid,
                CommonName = player.commonname ?? string.Empty,
                JerseyNameId = player.playerjerseynameid,
                JerseyName = player.playerjerseyname ?? string.Empty
            };
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
        ApplyDefaultTeamSheets(tables, teams);
        players.LinkTeam(teams);
        teams.LinkPlayer(players);
        teams.LinkLeague(leagues);
        teams.LinkOpponent(teams);
    }

    private static void ApplyDefaultTeamSheets(Dictionary<string, TableSnapshot> tables, TeamList teams)
    {
        if (!tables.TryGetValue("default_teamsheets", out var sheets)) return;
        var teamIdColumn = Column(sheets, "teamid");
        var tacticIdColumn = Column(sheets, "tacticid");
        if (teamIdColumn < 0) return;

        foreach (var group in sheets.Rows
            .Select((row, index) => new { Row = row, Index = index })
            .GroupBy(value => ParseIntAt(value.Row, teamIdColumn)))
        {
            var team = teams.SearchId(group.Key) as Team;
            if (team == null) continue;
            if (team.Roster.Count == 0)
            {
                // Empty placeholder clubs still own a teamsheet row. Capture
                // their loaded state so a no-op save never writes synthetic
                // captain and set-piece values into that row.
                s_loadedTeamSheets[team] = TeamSheetState.From(team);
                continue;
            }
            var selected = group.OrderBy(value => ParseIntAt(value.Row, tacticIdColumn) == 0 ? 0 : 1)
                .ThenBy(value => value.Index).First();

            // Tactics, captain and set-piece assignments live on the active
            // FC26 teamsheet rather than teams. Populate those legacy fields.
            MapFields(team, sheets.Columns, selected.Row);

            var original = team.Roster.Cast<TeamPlayer>()
                .Where(value => value?.Player != null).ToList();
            var byPlayerId = original.GroupBy(value => value.Player.Id)
                .ToDictionary(grouped => grouped.Key, grouped => grouped.First());
            var ordered = new List<TeamPlayer>(original.Count);
            var seen = new HashSet<int>();

            for (var slot = 0; slot < 52; slot++)
            {
                var playerId = ParseIntAt(selected.Row, Column(sheets, "playerid" + slot));
                if (playerId <= 0 || !seen.Add(playerId) || !byPlayerId.TryGetValue(playerId, out var player))
                    continue;
                if (slot < 11)
                {
                    var positionColumn = Column(sheets, "position" + slot);
                    var position = ParseIntAt(selected.Row, positionColumn);
                    // Current FC26 default_teamsheets rows have playerid0..51,
                    // but no position0..10 columns.  Treating a missing column
                    // as integer zero stacks every starter in the same UI role,
                    // leaving only one player visible on the pitch.
                    if (positionColumn < 0 || position < 0 || position >= 28)
                        position = team.Formation?.PlayingRoles != null && slot < team.Formation.PlayingRoles.Length
                            ? team.Formation.PlayingRoles[slot].Role?.Id ?? team.Formation.PlayingRoles[slot].Id
                            : player.Player.preferredposition1;
                    player.position = position;
                }
                else
                {
                    player.position = slot < 18 ? 28 : 29;
                }
                ordered.Add(player);
            }

            foreach (var player in original)
            {
                if (!seen.Add(player.Player.Id)) continue;
                player.position = 29;
                ordered.Add(player);
            }
            team.Roster.Clear();
            team.Roster.AddRange(ordered.ToArray());
            s_loadedTeamSheets[team] = TeamSheetState.From(team);
        }
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

    private static Snapshot ReadSnapshot(string path)
    {
        using var stream = File.OpenRead(path);
        var snapshot = JsonSerializer.Deserialize<Snapshot>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("FC26 snapshot is empty.");
        foreach (var table in snapshot.Tables)
        {
            table.Rows ??= new List<string[]>();
            table.RowsLoaded = string.IsNullOrWhiteSpace(table.DataFile);
            if (table.RowCount <= 0) table.RowCount = table.Rows.Count;
        }
        return snapshot;
    }

    private static void EnsureRows(TableSnapshot table, string snapshotPath)
    {
        if (table.RowsLoaded) return;
        if (string.IsNullOrWhiteSpace(table.DataFile))
        {
            table.RowsLoaded = true;
            return;
        }

        var snapshotDirectory = Path.GetDirectoryName(Path.GetFullPath(snapshotPath))
            ?? throw new InvalidDataException("FC26 snapshot folder is unavailable.");
        var dataPath = Path.GetFullPath(Path.Combine(snapshotDirectory,
            table.DataFile.Replace('/', Path.DirectorySeparatorChar)));
        var allowedPrefix = snapshotDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        if (!dataPath.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("FC26 snapshot contains an unsafe table data path.");
        if (!File.Exists(dataPath))
            throw new FileNotFoundException("FC26 snapshot table data is missing: " + table.Name, dataPath);

        using var file = File.OpenRead(dataPath);
        using var input = dataPath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            ? (Stream)new GZipStream(file, CompressionMode.Decompress, leaveOpen: false)
            : file;
        table.Rows = JsonSerializer.Deserialize<List<string[]>>(input,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string[]>();
        table.RowCount = table.Rows.Count;
        table.RowsLoaded = true;
    }

    private static void UnloadRows(TableSnapshot table)
    {
        if (string.IsNullOrWhiteSpace(table.DataFile)) return;
        table.Rows = new List<string[]>();
        table.RowsLoaded = false;
    }

    private sealed class Snapshot
    {
        public int Version { get; set; }
        public string GameRoot { get; set; } = string.Empty;
        public string DatabaseFolder { get; set; } = string.Empty;
        public List<TableSnapshot> Tables { get; set; } = new();
    }
    private sealed class TableSnapshot
    {
        public string Name { get; set; } = string.Empty;
        public bool IsCore { get; set; }
        public string[] Columns { get; set; } = Array.Empty<string>();
        public List<ColumnSnapshot> ColumnDetails { get; set; } = new();
        public List<string[]> Rows { get; set; } = new();
        public int RowCount { get; set; }
        public string DataFile { get; set; } = string.Empty;
        internal bool RowsLoaded { get; set; }
    }

    internal sealed class ColumnSnapshot
    {
        public string Name { get; set; } = string.Empty;
        public bool IsWritable { get; set; } = true;
        public int Kind { get; set; }
        public int Depth { get; set; }
        public long RangeLow { get; set; }
        public long RangeHigh { get; set; }
    }

    private sealed class ChangePlan
    {
        public int Version { get; set; }
        public string GameRoot { get; set; } = string.Empty;
        public string DatabaseFolder { get; set; } = string.Empty;
        public List<StructuralChange> StructuralChanges { get; set; } = new();
        public List<Change> Changes { get; set; } = new();
    }

    internal sealed class StructuralChange
    {
        public string Kind { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        // Snapshot rows are appended for a predictable editor view, while the
        // native engine inserts a duplicate immediately after its source row.
        // Persist the editor-side target so the save service can translate it
        // to the native row index before staging fields.
        public int TargetRowIndex { get; set; } = -1;
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

    private sealed class TeamSheetState
    {
        internal int[] PlayerIds { get; private set; } = Array.Empty<int>();
        internal Dictionary<string, int> IntegerValues { get; private set; } = new Dictionary<string, int>();

        internal static TeamSheetState From(Team team) => new TeamSheetState
        {
            PlayerIds = team.Roster.Cast<TeamPlayer>().Where(value => value?.Player != null)
                .GroupBy(value => value.Player.Id).Select(group => group.First().Player.Id).ToArray(),
            IntegerValues = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["busbuildupspeed"] = team.busbuildupspeed, ["busdribbling"] = team.busdribbling,
                ["buspassing"] = team.buspassing, ["buspositioning"] = team.buspositioning,
                ["cccrossing"] = team.cccrossing, ["ccpassing"] = team.ccpassing,
                ["ccpositioning"] = team.ccpositioning, ["ccshooting"] = team.ccshooting,
                ["defaggression"] = team.defaggression, ["defdefenderline"] = team.defdefenderline,
                ["defmentality"] = team.defmentality, ["defteamwidth"] = team.defteamwidth,
                ["captainid"] = team.captainid, ["freekicktakerid"] = team.freekicktakerid,
                ["leftcornerkicktakerid"] = team.leftcornerkicktakerid,
                ["leftfreekicktakerid"] = team.leftfreekicktakerid,
                ["longkicktakerid"] = team.longkicktakerid, ["penaltytakerid"] = team.penaltytakerid,
                ["rightcornerkicktakerid"] = team.rightcornerkicktakerid,
                ["rightfreekicktakerid"] = team.rightfreekicktakerid
            }
        };
    }
}

internal sealed class SnapshotDetailTable
{
    internal SnapshotDetailTable(string name, string[] columns, List<Fc26SnapshotLoader.ColumnSnapshot> details, List<string[]> rows)
    {
        Name = name;
        Columns = columns;
        ColumnDetails = columns.Select((column, index) => index < details.Count
            ? new SnapshotDetailColumn(details[index].Name, details[index].IsWritable, details[index].Kind,
                details[index].Depth, details[index].RangeLow, details[index].RangeHigh)
            : new SnapshotDetailColumn(column, true, 0, 0, 0, 0)).ToArray();
        Rows = rows;
    }

    internal string Name { get; }
    internal string[] Columns { get; }
    internal SnapshotDetailColumn[] ColumnDetails { get; }
    internal IReadOnlyList<string[]> Rows { get; }

    internal int Column(string fieldName) =>
        Array.FindIndex(Columns, value => value.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

    internal string Value(int rowIndex, string fieldName)
    {
        var column = Column(fieldName);
        return rowIndex >= 0 && rowIndex < Rows.Count && column >= 0 && column < Rows[rowIndex].Length
            ? Rows[rowIndex][column]
            : string.Empty;
    }
}

internal sealed class SnapshotDetailColumn
{
    internal SnapshotDetailColumn(string name, bool writable, int kind, int depth, long low, long high)
    {
        Name = name; IsWritable = writable; Kind = kind; Depth = depth; RangeLow = low; RangeHigh = high;
    }
    internal string Name { get; }
    internal bool IsWritable { get; }
    internal int Kind { get; }
    internal int Depth { get; }
    internal long RangeLow { get; }
    internal long RangeHigh { get; }
    internal string KindLabel => Kind == 3 ? "Integer" : Kind == 4 ? "Decimal" : Kind == 13 || Kind == 14 ? "Compressed text" : "Text";
}
