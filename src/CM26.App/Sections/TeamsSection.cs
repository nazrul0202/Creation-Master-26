using System.Drawing;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;
using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>
/// FC26 data adapter using the original CM16 TeamForm canvas and tab structure.
/// The source layout reference is CM16Source\TeamForm.cs linked by CM26.App.csproj.
/// </summary>
public sealed class TeamsSection : SectionBase
{
    private static readonly Font LegacyFont = Theme.Body;
    private static readonly Color DarkCrestBackground = Color.FromArgb(20, 42, 63);
    private readonly List<TextBox> _editors = [];
    private readonly Dictionary<string, FieldValue> _fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _stagingGrid = new();
    private readonly Label _crestCaption = new();
    private readonly List<PictureBox> _crestViewers = [];
    private readonly List<Panel> _teamColorChips = [];
    private readonly ListView _teamPlayers = new();
    private readonly ImageList _rosterMinifaces = new() { ImageSize = new Size(32, 32), ColorDepth = ColorDepth.Depth32Bit };
    private readonly HashSet<int> _pendingRosterMinifaces = [];
    private readonly SemaphoreSlim _minifaceLoadGate = new(1, 1);
    private readonly ListView _availablePlayers = new();
    private readonly ListView _teamSponsors = new();
    private readonly PictureBox _sponsorPreview = new();
    private readonly Label _sponsorPreviewCaption = new();
    private readonly ListView _adboardSources = new();
    private readonly PictureBox _adboardPreview = new();
    private readonly Label _adboardCaption = new();
    private readonly PictureBox _teamFlagPreview = new();
    private readonly PictureBox _nationFlagPreview = new();
    private readonly Label _teamFlagCaption = new();
    private readonly Label _nationFlagCaption = new();
    private readonly ListView _matchdayBench = new();
    private readonly ListView _teamCallnameSlots = new();
    private readonly ListView _anthemSlots = new();
    private readonly ListView _goalSongSlots = new();
    private readonly Dictionary<string, ComboBox> _playerReferencePickers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<LineupSlot> _lineupSlots = [];
    private readonly Dictionary<int, TeamRosterItem> _rosterByPlayerId = new();
    private readonly ComboBox _formationView = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private Panel? _formationBoard;
    private Label? _formationStatus;
    private int _activeTeamFormationRow = -1;
    private int _activeTeamSheetRow = -1;
    private int _activeLineupTeamId;
    private bool _syncPlayerReferencePickers;
    private bool _syncFormationView;
    private readonly Label _teamNameLabel = new();
    private readonly Label _teamMetaLabel = new();
    private readonly PictureBox _teamCrestPreview = new();
    private readonly Label _teamOverallLabel = new();
    private readonly Label _teamAttLabel = new();
    private readonly Label _teamMidLabel = new();
    private readonly Label _teamDefLabel = new();
    private readonly Panel _teamOvrBar = new();
    private readonly Panel _teamAttBar = new();
    private readonly Panel _teamMidBar = new();
    private readonly Panel _teamDefBar = new();
    private Label _teamFoundationLabel = new();
    private Label _teamWorthLabel = new();
    private Label _teamStadiumLabel = new();
    private Label _teamRivalLabel = new();
    private Label _teamLeagueTitles = new();
    private Label _teamDomesticCups = new();
    private Label _teamContinentals = new();
    private Label _teamPrestigeDom = new();
    private Label _teamPrestigeInt = new();
    private Label _teamPopularity = new();
    private Label _teamYouthDev = new();
    private Label _teamProfitability = new();
    private readonly PictureBox _teamKitHome = new();
    private readonly PictureBox _teamKitAway = new();
    private readonly PictureBox _teamKitThird = new();
    private readonly PictureBox _teamKitGk = new();
    private readonly PictureBox _teamStadiumImg = new();
    private readonly PictureBox _teamManagerImg = new();
    private Label _teamManagerName = new();
    private readonly Label _teamManagerNation = new();

    public override string SectionKey => "teams";
    public override string SectionTitle => "Teams";
    protected override string TableName => "teams";
    // A standalone team starts unlinked. League editors can link it later, while
    // this command always creates a valid editable squad for the new record.
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search teams…";

    public TeamsSection(AppServices services) : base(services)
    {
        Tabs.Font = LegacyFont;
        Tabs.Padding = new Point(4, 2);
        AddGenericTab();
        AddRosterTab();
        // Sponsors, Adboards, Flags, Audio tabs removed — not needed for basic team editing.
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Services.RequireData().GetTeams();

    protected override void CreateNewRecord()
    {
        // Build dropdown options from the database
        var countries = Services.RequireData().GetCountries();
        var leagues = Services.RequireData().GetLeagues();

        var countryOptions = countries.Select(c => (c.Title, c.RecordIndex.ToString())).ToList();
        var leagueOptions = leagues.Select(l => (l.Title, l.RecordIndex.ToString())).ToList();

        // Insert a "None" option at the top
        // Use a negative sentinel so the first real database row (index 0) is
        // not confused with the explicit "none" option.
        countryOptions.Insert(0, ("(No country)", "-1"));
        leagueOptions.Insert(0, ("(No league)", "-1"));

        var fields = new List<EntityField>
        {
            new("Team name", "New Team"),
            new("Country", "-1", EntityFieldType.Dropdown, countryOptions),
            new("League", "-1", EntityFieldType.Dropdown, leagueOptions),
        };

        if (!EntityCreationDialog.TryShow(this, "Team", fields, out var values))
            return;
        try
        {
            if (GetRecords().Any(item => string.Equals(item.Title.Trim(), values[0], StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A team with that name already exists. Choose a distinct public team name.");

            // Dropdown values are database row indexes (not indexes into the
            // alphabetically sorted option list).  The previous implementation
            // treated them as list positions, which silently linked a team to a
            // different country/league or left the FK at zero whenever the
            // selected row was beyond the visible list order.
            var countryId = ResolveReferenceId("countries", values[1], "countryid");
            var leagueId = ResolveReferenceId("leagues", values[2], "leagueid");

            var id = CreateRecordFromTemplate(TableName, "teamid", new Dictionary<string, string>
            {
                ["teamname"] = values[0],
                ["countryid"] = countryId,
                ["leagueid"] = leagueId,
                ["assetid"] = "0",
                ["presassetone"] = "0",
                ["presassettwo"] = "0",
                ["captainid"] = "-1",
                ["penaltytakerid"] = "-1",
                ["freekicktakerid"] = "-1",
                ["leftcornerkicktakerid"] = "-1",
                ["rightcornerkicktakerid"] = "-1",
                ["overallrating"] = "0",
                ["attackrating"] = "0",
                ["midfieldrating"] = "0",
                ["defenserating"] = "0",
                ["domesticprestige"] = "0",
                ["internationalprestige"] = "0",
                ["clubworth"] = "0",
            });

            // If a league was selected, create the league-team link
            var linkedToLeague = false;
            if (leagueId != "0")
            {
                try
                {
                    var links = Services.Session.GetTable("leagueteamlinks");
                    if (links != null && links.RowCount > 0)
                    {
                        var keyCol = Col(links, "artificialkey");
                        var leagueCol = Col(links, "leagueid");
                        var teamCol = Col(links, "teamid");
                        if (keyCol >= 0 && leagueCol >= 0 && teamCol >= 0)
                        {
                            var maxKey = 0;
                            for (var row = 0; row < links.RowCount; row++)
                            {
                                var rec = Services.Session.GetRecord("leagueteamlinks", row);
                                if (rec != null && int.TryParse(rec.Get(keyCol), out var key))
                                    maxKey = Math.Max(maxKey, key);
                            }
                            var duplicate = Services.Session.DuplicateRow("leagueteamlinks", 0);
                            if (duplicate.Success)
                            {
                                var newRow = 1;
                                Services.Pending.Stage("leagueteamlinks", newRow, "artificialkey", (maxKey + 1).ToString());
                                Services.Pending.Stage("leagueteamlinks", newRow, "leagueid", leagueId);
                                Services.Pending.Stage("leagueteamlinks", newRow, "teamid", id.ToString());
                                Services.Pending.MarkStructuralChange();
                                linkedToLeague = true;
                            }
                        }
                    }
                }
                catch { /* Non-critical: team created without league link */ }
            }

            var squad = FillTeamSquad(id);
            Services.Session.RefreshSchema();
            Services.RefreshDatabaseIndexes();
            LoadData();
            var created = GetRecords().FirstOrDefault(item =>
                Parse(Services.Session.GetCell(TableName, item.RecordIndex, "teamid")) == id);
            if (created != null) GoToRecord(created.RecordIndex);

            var leagueNote = linkedToLeague ? " and linked to the selected league" : "";
            MessageBox.Show(this,
                $"{values[0]} was created with ID {id}{leagueNote} and {squad} editable placeholder players.\n\nReview kits and artwork before Save.",
                "Create Team", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create Team", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string ResolveReferenceId(string tableName, string rowValue, string idField)
    {
        if (!int.TryParse(rowValue, out var row) || row < 0) return "0";
        var table = Services.Session.GetTable(tableName);
        if (table == null || row >= table.RowCount || table.FindColumn(idField) == null) return "0";
        return Services.Session.GetCell(tableName, row, idField) ?? "0";
    }

    private void FindTeam(string query)
    {
        var term = query.Trim();
        if (term.Length == 0) return;
        var result = GetRecords().FirstOrDefault(item =>
            item.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            item.Subtitle.Contains(term, StringComparison.OrdinalIgnoreCase));
        if (result == null)
        {
            MessageBox.Show(this, $"No team matches '{term}'.", "Search Team",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        GoToRecord(result.RecordIndex);
    }

    private void ImportScraperSquad()
    {
        if (CurrentRecordIndex < 0)
        {
            MessageBox.Show(this, "Select or create the destination team first.", "Import Scraper Squad",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new OpenFileDialog
        {
            Title = "Import CM26 Scraper Squad",
            Filter = "Scraper workbook (*.xlsx)|*.xlsx",
            CheckFileExists = true,
        };
        var known = ExternalToolLocator.FindFile(Path.Combine("CM26 SCRAPER", "Scraped teams", "squad_Inter.xlsx"));
        if (!string.IsNullOrWhiteSpace(known)) dialog.InitialDirectory = Path.GetDirectoryName(known);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { ImportScraperSquadWorkbook(Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, "teamid")), dialog.FileName); }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import Scraper Squad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ImportScraperSquadFromDataSync(int teamId, string workbookPath)
    {
        try { ImportScraperSquadWorkbook(teamId, workbookPath); }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import Scraper Squad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportScraperSquadWorkbook(int teamId, string workbookPath)
    {
        if (teamId <= 0) throw new InvalidOperationException("Choose a valid destination team before importing a scraper squad.");
        if (!File.Exists(workbookPath)) throw new FileNotFoundException("The selected scraper output no longer exists.", workbookPath);
        var workbook = new CompdataWorkbookService();
        workbook.Open(workbookPath);
        var squad = workbook.SheetNames.Select(workbook.ReadSheet)
            .FirstOrDefault(table => table.Columns.Contains("firstname") && table.Columns.Contains("lastname"))
            ?? throw new InvalidDataException("The workbook has no CM26 Scraper player sheet.");
        var players = squad.Rows.Cast<DataRow>().Where(row =>
            !string.IsNullOrWhiteSpace(Cell(row, "firstname")) || !string.IsNullOrWhiteSpace(Cell(row, "lastname"))).ToArray();
        if (players.Length == 0) throw new InvalidDataException("The scraper workbook has no player rows.");
        if (MessageBox.Show(this,
                $"Import {players.Length} new player(s) and link them to Team ID {teamId}?\n\nCM26 creates Player IDs and team-player links. Existing records are not overwritten.",
                "Import Scraper Squad", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var created = 0;
        var editableNames = 0;
        foreach (var row in players)
        {
            var firstName = Cell(row, "firstname");
            var surname = Cell(row, "lastname");
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(surname)) continue;
            firstName = string.IsNullOrWhiteSpace(firstName) ? "Unknown" : firstName;
            surname = string.IsNullOrWhiteSpace(surname) ? firstName : surname;
            var values = PlayerValuesFromScraper(row, teamId);
            var playerId = CreateRecordFromTemplate("players", "playerid", values, templateRow: 0);
            Services.SetPlayerNameOverride(playerId, firstName, surname);
            if (TryCreateEditedPlayerName(playerId, firstName, surname)) editableNames++;
            CreateTeamPlayerLink(playerId, teamId, values);
            created++;
        }
        Services.Session.RefreshSchema();
        Services.RefreshDatabaseIndexes();
        LoadData();
        var team = GetRecords().FirstOrDefault(item => Parse(Services.Session.GetCell(TableName, item.RecordIndex, "teamid")) == teamId);
        if (team != null) GoToRecord(team.RecordIndex);
        var nameNote = editableNames == created
            ? " Names were stored in editable FC26 name records."
            : " This database has no safe editable-name template; CM26 keeps imported names searchable for this session, while in-game names need a compatible name source.";
        MessageBox.Show(this, $"Imported and linked {created} new player(s) to Team ID {teamId}.{nameNote} Review and Save when ready.",
            "Import Scraper Squad", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CreateTeamPlayerLink(int playerId, int teamId, IReadOnlyDictionary<string, string> playerValues)
    {
        var links = Services.Session.GetTable("teamplayerlinks") ?? throw new InvalidOperationException("The team-player link table is unavailable.");
        if (links.RowCount == 0) throw new InvalidOperationException("The team-player link table has no safe template record.");
        var duplicate = Services.Session.DuplicateRow("teamplayerlinks", 0);
        if (!duplicate.Success) throw new InvalidOperationException(duplicate.Message);
        var row = 1;
        var fields = new Dictionary<string, string> { ["playerid"] = playerId.ToString(), ["teamid"] = teamId.ToString() };
        if (playerValues.TryGetValue("jerseynumber", out var jersey)) fields["jerseynumber"] = jersey;
        if (playerValues.TryGetValue("preferredposition1", out var position)) fields["position"] = position;
        // teamplayerlinks keys on its artificialkey column; a duplicated template
        // row keeps the template's value, so a unique key must be staged or the
        // save's integrity check rejects every new link as a duplicate.
        if (links.FindColumn("artificialkey") != null)
            fields["artificialkey"] = NextAvailableId("teamplayerlinks", "artificialkey").ToString();
        foreach (var (field, value) in fields)
        {
            if (links.FindColumn(field) == null) continue;
            var result = Services.Pending.Stage("teamplayerlinks", row, field, value);
            if (!result.Success) throw new InvalidOperationException(result.Message);
        }
        Services.Pending.MarkStructuralChange();
    }

    private Dictionary<string, string> PlayerValuesFromScraper(DataRow row, int teamId)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["teamid"] = teamId.ToString(), ["firstnameid"] = "0", ["lastnameid"] = "0",
            ["commonnameid"] = "0", ["playerjerseynameid"] = "0", ["headclasscode"] = "0",
        };
        foreach (DataColumn column in row.Table.Columns)
        {
            var name = column.ColumnName;
            if (name.Equals("playerid", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("teamid", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("firstname", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("lastname", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("commonname", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("jerseyname", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("miniface", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("tmprofile", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("nationality", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("position", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("preferredfoot", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("birthdate", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("playerjointeamdate", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("contractvaliduntil", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("gender", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("jerseynumber", StringComparison.OrdinalIgnoreCase)) continue;
            var value = Convert.ToString(row[column])?.Trim();
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)) values[name] = value!;
        }
        var nation = Cell(row, "nationality");
        if (!string.IsNullOrWhiteSpace(nation) && TryResolveNationId(nation, out var nationId)) values["nationality"] = nationId.ToString();
        var position = Cell(row, "position");
        if (!string.IsNullOrWhiteSpace(position) && TryPositionCode(position, out var positionCode)) values["preferredposition1"] = positionCode.ToString();
        var foot = Cell(row, "preferredfoot");
        if (string.Equals(foot, "Right", StringComparison.OrdinalIgnoreCase)) values["preferredfoot"] = "1";
        else if (string.Equals(foot, "Left", StringComparison.OrdinalIgnoreCase)) values["preferredfoot"] = "2";
        if (TryScraperDate(row, "birthdate", out var birthdate)) values["birthdate"] = birthdate;
        if (TryScraperDate(row, "playerjointeamdate", out var joined)) values["playerjointeamdate"] = joined;
        var contract = Cell(row, "contractvaliduntil");
        if (DateTime.TryParseExact(contract, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var contractDate))
            values["contractvaliduntil"] = contractDate.Year.ToString(CultureInfo.InvariantCulture);
        var jersey = Cell(row, "jerseynumber");
        if (int.TryParse(jersey, NumberStyles.Integer, CultureInfo.InvariantCulture, out var jerseyNumber)) values["jerseynumber"] = jerseyNumber.ToString();
        return values;
    }

    private bool TryResolveNationId(string name, out int nationId)
    {
        nationId = 0;
        var nations = Services.Session.GetTable("nations");
        if (nations == null) return false;
        var idColumn = Col(nations, "nationid"); var nameColumn = Col(nations, "nationname");
        if (idColumn < 0 || nameColumn < 0) return false;
        for (var row = 0; row < nations.RowCount; row++)
        {
            var record = Services.Session.GetRecord("nations", row);
            if (record == null || !string.Equals(record.Get(nameColumn), name, StringComparison.OrdinalIgnoreCase)) continue;
            return int.TryParse(record.Get(idColumn), out nationId);
        }
        return false;
    }

    private static bool TryScraperDate(DataRow row, string column, out string rawDate)
    {
        rawDate = string.Empty;
        var value = Cell(row, column);
        if (!DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) return false;
        return FifaDateConverter.TryFromIso(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), out rawDate);
    }

    private static string Cell(DataRow row, string column) => row.Table.Columns.Contains(column)
        ? Convert.ToString(row[column])?.Trim() ?? string.Empty : string.Empty;

    private bool TryCreateEditedPlayerName(int playerId, string firstName, string surname)
    {
        var names = Services.Session.GetTable("editedplayernames");
        if (names == null || names.RowCount == 0) return false;
        var duplicate = Services.Session.DuplicateRow("editedplayernames", 0);
        if (!duplicate.Success) return false;
        var row = 1;
        foreach (var (field, value) in new Dictionary<string, string>
        {
            ["playerid"] = playerId.ToString(), ["firstname"] = firstName, ["surname"] = surname,
            ["commonname"] = string.Empty, ["playerjerseyname"] = surname,
        })
        {
            var outcome = Services.Session.StageEdit("editedplayernames", row, field, value);
            if (!outcome.Success) return false;
        }
        Services.Pending.MarkStructuralChange();
        return true;
    }

    private TabPage Page(string text)
    {
        var page = new TabPage(text) { BackColor = Theme.Background, Font = LegacyFont };
        page.Controls.Add(new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background });
        Tabs.TabPages.Add(page);
        return page;
    }

    private static Panel Canvas(TabPage page) => (Panel)page.Controls[0];

    private static Panel Group(string text, Point location, Size size)
    {
        var box = new Panel { Location = location, Size = size, BackColor = CardLayout.CardWhite };
        CardLayout.ApplyRounded(box, 10);
        box.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(size.Width, 4), BackColor = CardLayout.Fc26Green });
        box.Controls.Add(new Label
        {
            Text = text, Location = new Point(10, 8), Size = new Size(size.Width - 20, 16),
            Font = Theme.BodyBold, ForeColor = CardLayout.Fc26Green, BackColor = CardLayout.CardWhite
        });
        return box;
    }

    private static PictureBox Viewer(Point location, Size size) => new()
    {
        Location = location, Size = size, BackColor = Theme.Input,
        BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom
    };

    private PictureBox CrestViewer(Point location, Size size)
    {
        var viewer = Viewer(location, size);
        _crestViewers.Add(viewer);
        return viewer;
    }

    private static Button LegacyButton(string text, Point location, Size size) => new()
    {
        Text = text, Location = location, Size = size, Font = LegacyFont, UseVisualStyleBackColor = true
    };

    private static Button CardLayoutButton(string text, Point location, Size size)
    {
        var btn = new Button
        {
            Text = text, Location = location, Size = size,
            FlatStyle = FlatStyle.Flat, Font = Theme.BodyBold,
            BackColor = CardLayout.CardWhite, ForeColor = CardLayout.CardText,
            Cursor = Cursors.Hand,
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 182);
        btn.FlatAppearance.MouseOverBackColor = CardLayout.CardFieldBg;
        return btn;
    }

    private void AddGenericTab()
    {
        var page = Page("Overview");
        var canvas = Canvas(page);
        canvas.BackColor = CardLayout.CardBackground;
        canvas.AutoScrollMinSize = new Size(1370, 1100);

        // ═══════════════════════════════════════════════════════════════
        //  CLUB PROFILE HEADER (FC Tools Hub style)
        // ═══════════════════════════════════════════════════════════════
        var profile = new Panel { Location = new Point(12, 12), Size = new Size(1340, 220), BackColor = CardLayout.CardWhite };
        CardLayout.ApplyRounded(profile, 14);
        // Left green accent
        profile.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(6, 220), BackColor = CardLayout.Fc26Green });
        // Badge
        _teamCrestPreview.Location = new Point(24, 24);
        _teamCrestPreview.Size = new Size(140, 140);
        _teamCrestPreview.SizeMode = PictureBoxSizeMode.Zoom;
        _teamCrestPreview.BackColor = CardLayout.CardFieldBg;
        _teamCrestPreview.BorderStyle = BorderStyle.None;
        profile.Controls.Add(_teamCrestPreview);
        // Name
        _teamNameLabel.Location = new Point(184, 30);
        _teamNameLabel.Size = new Size(400, 38);
        _teamNameLabel.Font = new Font("Segoe UI", 22, FontStyle.Bold);
        _teamNameLabel.ForeColor = CardLayout.CardText;
        profile.Controls.Add(_teamNameLabel);
        // Meta line (league · nation)
        _teamMetaLabel.Location = new Point(186, 74);
        _teamMetaLabel.Size = new Size(500, 22);
        _teamMetaLabel.Font = Theme.BodyBold;
        _teamMetaLabel.ForeColor = CardLayout.CardMuted;
        profile.Controls.Add(_teamMetaLabel);
        // Overall rating
        var ovrTile = CardLayout.CreateTile(profile, "OVR", 184, 108, CardLayout.Fc26Green, 110, 90);
        _teamOverallLabel.Text = "--";
        ovrTile.Tile.Controls.Remove(ovrTile.Value);
        _teamOverallLabel.Location = new Point(5, 8);
        _teamOverallLabel.Size = new Size(100, 52);
        _teamOverallLabel.Font = new Font("Segoe UI", 26, FontStyle.Bold);
        _teamOverallLabel.TextAlign = ContentAlignment.MiddleCenter;
        _teamOverallLabel.ForeColor = Color.White;
        ovrTile.Tile.Controls.Add(_teamOverallLabel);

        // ATT / MID / DEF rating bars
        AddRatingBar(profile, "ATT", CardLayout.Fc26Yellow, _teamAttBar, _teamAttLabel, 310, 110);
        AddRatingBar(profile, "MID", CardLayout.Fc26Blue, _teamMidBar, _teamMidLabel, 310, 138);
        AddRatingBar(profile, "DEF", CardLayout.Fc26Red, _teamDefBar, _teamDefLabel, 310, 166);

        canvas.Controls.Add(profile);

        // ═══════════════════════════════════════════════════════════════
        //  QUICK INFO CARDS (Foundation, Worth, Stadium, Rival, Manager)
        // ═══════════════════════════════════════════════════════════════
        var quickInfo = new Panel { Location = new Point(12, 244), Size = new Size(1340, 72), BackColor = CardLayout.CardBackground };
        _teamFoundationLabel = CardLayout.CreateFact(quickInfo, "Founded", 0, 0, 248);
        _teamWorthLabel = CardLayout.CreateFact(quickInfo, "Worth", 264, 0, 248);
        _teamStadiumLabel = CardLayout.CreateFact(quickInfo, "Stadium", 528, 0, 248);
        _teamRivalLabel = CardLayout.CreateFact(quickInfo, "Rival", 792, 0, 248);
        _teamManagerName = CardLayout.CreateFact(quickInfo, "Manager", 1056, 0, 248);
        canvas.Controls.Add(quickInfo);

        // ═══════════════════════════════════════════════════════════════
        //  KITS SECTION
        // ═══════════════════════════════════════════════════════════════
        var kits = CardLayout.CreateGroup(canvas, "Kits", CardLayout.Fc26Green, 12, 326, 1340, 140);
        AddKitPreview(kits, _teamKitHome, "Home", 20, 30);
        AddKitPreview(kits, _teamKitAway, "Away", 223, 30);
        AddKitPreview(kits, _teamKitThird, "Third", 426, 30);
        AddKitPreview(kits, _teamKitGk, "Goalkeeper", 629, 30);

        // ═══════════════════════════════════════════════════════════════
        //  CLUB INFORMATION (Identity · Financial · Trophies · Reputation)
        // ═══════════════════════════════════════════════════════════════
        var infoGroup = CardLayout.CreateGroup(canvas, "Club Information", CardLayout.Fc26Green, 12, 478, 660, 380);
        // Identity
        AddFieldCard(infoGroup, "Identity", 14, 30,
            ("Team Id", "teamid"), ("Country", "countryid"), ("League", "leagueid"),
            ("Gender", "gender"), ("Ball Number", "ballid"));
        // Financial
        AddFieldCard(infoGroup, "Financial", 14, 180,
            ("Budget", "clubworth"), ("Domestic", "domesticprestige"),
            ("International", "internationalprestige"), ("Training Stadium", "trainingstadium"));
        // Trophies
        AddFieldCard(infoGroup, "Trophies", 340, 30,
            ("League Titles", "leaguetitles"), ("Domestic Cups", "domesticcups"),
            ("UCL Wins", "uefa_cl_wins"), ("UEL Wins", "uefa_el_wins"));
        // Reputation
        AddFieldCard(infoGroup, "Reputation", 340, 180,
            ("Domestic Prestige", "domesticprestige"), ("Intl Prestige", "internationalprestige"),
            ("Popularity", "popularity"), ("Youth Dev", "youthdevelopment"),
            ("Profitability", "profitability"));

        // ═══════════════════════════════════════════════════════════════
        //  STADIUM + MANAGER CARDS
        // ═══════════════════════════════════════════════════════════════
        var stadium = CardLayout.CreateGroup(canvas, "Stadium", CardLayout.Fc26Blue, 688, 478, 320, 380);
        _teamStadiumImg.Location = new Point(14, 30);
        _teamStadiumImg.Size = new Size(290, 160);
        _teamStadiumImg.SizeMode = PictureBoxSizeMode.Zoom;
        _teamStadiumImg.BackColor = CardLayout.CardFieldBg;
        _teamStadiumImg.BorderStyle = BorderStyle.None;
        stadium.Controls.Add(_teamStadiumImg);
        AddBoundFields(stadium, new[] { ("Stadium Name", "stadiumid"), ("Capacity", "teamstadiumcapacity"), ("Corner Flag", "cornerflagpolecolor") }, 14, 200, 140, 155, 26);

        var manager = CardLayout.CreateGroup(canvas, "Manager", CardLayout.CardMuted, 1024, 478, 328, 380);
        _teamManagerImg.Location = new Point(14, 30);
        _teamManagerImg.Size = new Size(120, 120);
        _teamManagerImg.SizeMode = PictureBoxSizeMode.Zoom;
        _teamManagerImg.BackColor = CardLayout.CardFieldBg;
        _teamManagerImg.BorderStyle = BorderStyle.None;
        manager.Controls.Add(_teamManagerImg);
        _teamManagerNation.Location = new Point(14, 160);
        _teamManagerNation.Size = new Size(300, 20);
        _teamManagerNation.Font = Theme.Body;
        _teamManagerNation.ForeColor = CardLayout.CardMuted;
        _teamManagerNation.BackColor = CardLayout.CardWhite;
        manager.Controls.Add(_teamManagerNation);
        AddBoundFields(manager, new[] { ("First Name", "managerid"), ("Surname", "managerid"), ("Latitude", "latitude"), ("Longitude", "longitude"), ("UTC Offset", "utcoffset") }, 14, 186, 140, 155, 26);

        // ═══════════════════════════════════════════════════════════════
        //  PERFORMANCE CARDS
        // ═══════════════════════════════════════════════════════════════
        var perf = CardLayout.CreateGroup(canvas, "Performance", CardLayout.Fc26Yellow, 12, 870, 440, 160);
        AddBoundFields(perf, new[] { ("Last Year Position", "form"), ("Last Year Champion", "prev_el_champ"), ("Current Form", "form") }, 14, 30, 170, 175, 26);

        var ratings = CardLayout.CreateGroup(canvas, "Matchday Ratings", CardLayout.Fc26Blue, 468, 870, 440, 160);
        AddBoundFields(ratings, new[] { ("Overall", "matchdayoverallrating"), ("Attack", "matchdayattackrating"), ("Midfield", "matchdaymidfieldrating"), ("Defence", "matchdaydefenserating") }, 14, 30, 116, 170, 26);

        var tendency = CardLayout.CreateGroup(canvas, "Team Tendency", CardLayout.Fc26Orange, 924, 870, 428, 160);
        AddBoundFields(tendency, new[] { ("Build Up Play", "buildupplay"), ("Defensive Depth", "defensivedepth"), ("Opponent Weak", "opponentweakthreshold"), ("Opponent Strong", "opponentstrongthreshold") }, 14, 30, 190, 170, 26);

        // Search and import (compact)
        var findActions = new Panel { Location = new Point(12, 1042), Size = new Size(1340, 32), BackColor = CardLayout.CardBackground };
        var search = new TextBox { Location = new Point(0, 2), Size = new Size(140, 24), Font = LegacyFont, PlaceholderText = "Find team…" };
        Theme.ApplyTextBox(search);
        var find = CardLayoutButton("Find", new Point(146, 2), new Size(58, 26));
        find.Click += (_, _) => FindTeam(search.Text);
        search.KeyDown += (_, eventArgs) => { if (eventArgs.KeyCode == Keys.Enter) { FindTeam(search.Text); eventArgs.SuppressKeyPress = true; } };
        var importSquad = CardLayoutButton("Import Scraper Squad", new Point(212, 2), new Size(180, 26));
        importSquad.Click += (_, _) => ImportScraperSquad();
        findActions.Controls.Add(search);
        findActions.Controls.Add(find);
        findActions.Controls.Add(importSquad);
        canvas.Controls.Add(findActions);
    }

    private void AddRatingBar(Control parent, string label, Color accent, Panel barFill, Label valueLabel, int x, int y)
    {
        var lbl = new Label { Text = label, Location = new Point(x, y), Size = new Size(36, 22), Font = Theme.BodyBold, ForeColor = accent, BackColor = CardLayout.CardWhite };
        parent.Controls.Add(lbl);
        var track = new Panel { Location = new Point(x + 40, y + 4), Size = new Size(160, 14), BackColor = CardLayout.CardFieldBg };
        CardLayout.ApplyRounded(track, 7);
        barFill.Location = Point.Empty;
        barFill.Size = new Size(1, 14);
        barFill.BackColor = accent;
        barFill.Tag = accent;
        track.Controls.Add(barFill);
        parent.Controls.Add(track);
        valueLabel.Location = new Point(x + 208, y);
        valueLabel.Size = new Size(40, 22);
        valueLabel.Font = Theme.BodyBold;
        valueLabel.ForeColor = CardLayout.CardText;
        valueLabel.BackColor = CardLayout.CardWhite;
        parent.Controls.Add(valueLabel);
    }

    private void AddKitPreview(Control parent, PictureBox preview, string label, int x, int y)
    {
        var holder = new Panel { Location = new Point(x, y), Size = new Size(180, 96), BackColor = CardLayout.CardWhite };
        preview.Location = new Point(10, 4);
        preview.Size = new Size(160, 72);
        preview.SizeMode = PictureBoxSizeMode.Zoom;
        preview.BackColor = CardLayout.CardFieldBg;
        preview.BorderStyle = BorderStyle.None;
        holder.Controls.Add(preview);
        holder.Controls.Add(new Label { Text = label, Location = new Point(0, 78), Size = new Size(180, 16), Font = Theme.Muted9, TextAlign = ContentAlignment.MiddleCenter, ForeColor = CardLayout.CardSubtle, BackColor = CardLayout.CardWhite });
        parent.Controls.Add(holder);
    }

    private void AddFieldCard(Control parent, string title, int x, int y, params (string Label, string Field)[] fields)
    {
        var block = new Panel { Location = new Point(x, y), Size = new Size(300, 140), BackColor = CardLayout.CardFieldBg };
        CardLayout.ApplyRounded(block, 8);
        block.Controls.Add(new Label { Text = title.ToUpperInvariant(), Location = new Point(10, 6), Size = new Size(280, 16), Font = new Font(Theme.Body, FontStyle.Bold), ForeColor = CardLayout.CardSubtle });
        var row = 0;
        foreach (var (label, field) in fields)
        {
            var ry = 28 + row * 26;
            block.Controls.Add(new Label { Text = label, Location = new Point(10, ry + 3), Size = new Size(140, 18), Font = Theme.Body, ForeColor = CardLayout.CardFieldLabel, BackColor = CardLayout.CardFieldBg, AutoEllipsis = true });
            var editor = new TextBox { Location = new Point(155, ry), Size = new Size(135, 22), Font = LegacyFont, Tag = field, BorderStyle = BorderStyle.FixedSingle };
            Theme.ApplyTextBox(editor);
            editor.Leave += (_, _) => StageEditor(editor);
            _editors.Add(editor);
            block.Controls.Add(editor);
            row++;
        }
        parent.Controls.Add(block);
    }

    private void AddAudioTab()
    {
        var page = Page("Audio");
        var canvas = Canvas(page);
        var presentation = Group("Selected Team Match Audio", new Point(3, 3), new Size(475, 193));
        AddBoundFields(presentation, new[]
        {
            ("Sun Anthem Enabled", "hassuncanthem"),
            ("Crowd Region", "crowdregion"),
            ("Viking Clap", "hasvikingclap"),
            ("Team Personality", "personalityid"),
            ("Tifo Enabled", "hastifo")
        }, 15, 25, 175, 150, 26);
        presentation.Controls.Add(new Label
        {
            Text = "Match presentation settings for the selected team.",
            Location = new Point(15, 163), Size = new Size(430, 24),
            Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        canvas.Controls.Add(presentation);

        canvas.Controls.Add(CreateAudioCatalog(
            "Custom Team Callname Catalog", "CustomizableTeamName",
            _teamCallnameSlots, new Point(3, 204), 1));
        canvas.Controls.Add(CreateAudioCatalog(
            "Custom Anthem Catalog", "CustomizableAnthemChant",
            _anthemSlots, new Point(490, 3), 1001));
        canvas.Controls.Add(CreateAudioCatalog(
            "Custom Chant / Goal Song Catalog", "CustomizableChantPackage",
            _goalSongSlots, new Point(490, 322), 1501));
    }

    private Panel CreateAudioCatalog(
        string title, string tableName, ListView list, Point location, int firstItemId)
    {
        var width = location.X < 100 ? 475 : 610;
        var box = Group(title, location, new Size(width, 311));
        list.Location = new Point(10, 30);
        list.Size = new Size(width - 20, 235);
        list.View = View.Details;
        list.FullRowSelect = true;
        list.GridLines = true;
        list.Font = LegacyFont;
        list.Columns.Add("Item", 65);
        list.Columns.Add("Audio Item", 90);
        list.Columns.Add("HAL String", 270);
        list.Columns.Add("CM", 45);
        list.Columns.Add("Clubs", 55);
        box.Controls.Add(list);
        var add = LegacyButton("Add", new Point(10, 279), new Size(80, 26));
        add.Click += (_, _) => AddAudioCatalogEntry(tableName, list, firstItemId);
        box.Controls.Add(add);
        var edit = LegacyButton("Edit", new Point(96, 279), new Size(80, 26));
        edit.Click += (_, _) => EditAudioCatalogEntry(tableName, list);
        box.Controls.Add(edit);
        var remove = LegacyButton("Remove", new Point(182, 279), new Size(80, 26));
        remove.Click += (_, _) => RemoveAudioCatalogEntry(tableName, list);
        box.Controls.Add(remove);
        return box;
    }

    private void AddRosterTab()
    {
        var page = Page("Roster");
        var canvas = Canvas(page);
        // This is deliberately a docked workspace rather than a set of controls
        // positioned on a large virtual canvas.  The latter looked acceptable at
        // 1920px but put the squad rail outside the visible area at common laptop
        // widths, and it caused child controls to overlap while scrolling.
        canvas.AutoScroll = false;
        canvas.AutoScrollMinSize = Size.Empty;

        var workspace = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            FixedPanel = FixedPanel.Panel2,
            SplitterWidth = 6,
            BackColor = CardLayout.CardBackground,
            Panel2MinSize = 310,
        };
        workspace.Panel1.Padding = new Padding(3);
        workspace.Panel2.Padding = new Padding(3);
        workspace.Panel1.BackColor = CardLayout.CardBackground;
        workspace.Panel2.BackColor = CardLayout.CardBackground;
        workspace.SizeChanged += (_, _) =>
        {
            // Keep a useful pitch even when Windows restores a narrow window.
            if (workspace.Width > 720)
                workspace.SplitterDistance = Math.Clamp(workspace.Width - 420, 390, workspace.Width - workspace.Panel2MinSize - workspace.SplitterWidth);
        };
        canvas.Controls.Add(workspace);

        // The matchday roles belong directly above the squad rail, making the
        // common "pick player, assign role, drag to XI" workflow predictable.
        var matchday = CreateMatchdayPanel(Point.Empty, new Size(0, 168));
        matchday.Dock = DockStyle.Top;
        matchday.Height = 168;
        workspace.Panel2.Controls.Add(matchday);

        // === VISUAL SQUAD LIST ===
        // Mirrors the provided formation-board layout: the pitch is the primary
        // canvas on the left and the searchable squad sits as a visual rail on
        // the right, with a miniface beside each player.
        var squadGroup = Group("Squad", Point.Empty, new Size(400, 600));
        squadGroup.Dock = DockStyle.Fill;
        workspace.Panel2.Controls.Add(squadGroup);

        // Player count label
        var squadCount = new Label
        {
            Text = "Squad (0 players)",
            Location = new Point(10, 30),
            Size = new Size(200, 18),
            Font = Theme.BodyBold,
            ForeColor = Theme.Text,
            BackColor = Theme.Panel,
        };
        squadGroup.Controls.Add(squadCount);
        _squadCountLabel = squadCount;

        // Tools section
        var toolsGroup = Group("Tools", new Point(10, 52), new Size(300, 62));
        toolsGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        var btnTransfer = LegacyButton("Transfer", new Point(8, 30), new Size(90, 26));
        btnTransfer.Click += (_, _) => OpenTransferDialog();
        toolsGroup.Controls.Add(btnTransfer);
        var btnLoan = LegacyButton("Loan", new Point(104, 30), new Size(90, 26));
        btnLoan.Click += (_, _) => ShowLoanDetails();
        toolsGroup.Controls.Add(btnLoan);
        var btnFind = LegacyButton("Find", new Point(200, 30), new Size(70, 26));
        btnFind.Click += (_, _) => FindSelectedPlayer();
        toolsGroup.Controls.Add(btnFind);
        squadGroup.Controls.Add(toolsGroup);

        // Compact squad ListView
        _teamPlayers.Location = new Point(10, 120);
        _teamPlayers.Size = new Size(10, 10);
        _teamPlayers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _teamPlayers.View = View.Details;
        _teamPlayers.FullRowSelect = true;
        _teamPlayers.GridLines = false;
        _teamPlayers.HideSelection = false;
        _teamPlayers.BackColor = Theme.Input;
        _teamPlayers.ForeColor = Theme.Text;
        _teamPlayers.Font = new Font("Segoe UI", 8.5f);
        _teamPlayers.SmallImageList = _rosterMinifaces;
        _teamPlayers.Columns.Add("#", 28);
        _teamPlayers.Columns.Add("Player", 260);
        _teamPlayers.Columns.Add("POS", 54);
        _teamPlayers.Columns.Add("Ovr", 42);
        _teamPlayers.Columns.Add("Role", 46);
        _teamPlayers.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _teamPlayers.MultiSelect = false;
        _teamPlayers.ItemDrag += (_, e) =>
        {
            if (e.Item is ListViewItem item && item.Tag is int playerId && playerId > 0)
                _teamPlayers.DoDragDrop(playerId, DragDropEffects.Copy);
        };
        _teamPlayers.DoubleClick += (_, _) => OpenSelectedRosterPlayer();
        squadGroup.Controls.Add(_teamPlayers);
        squadGroup.SizeChanged += (_, _) =>
        {
            var width = Math.Max(240, squadGroup.ClientSize.Width - 20);
            toolsGroup.Width = width;
            _teamPlayers.Width = width;
            _teamPlayers.Height = Math.Max(140, squadGroup.ClientSize.Height - _teamPlayers.Top - 10);
        };

        // === FORMATION / PITCH ===
        // The roster is a one-screen workspace. A compact pitch plus the
        // dedicated caption strip keeps the exact FC26 coordinates readable.
        var pitch = Group("Formation Board", Point.Empty, new Size(700, 600));
        pitch.Dock = DockStyle.Fill;
        workspace.Panel1.Controls.Add(pitch);
        var board = new Panel
        {
            Location = new Point(8, 28),
            Size = new Size(600, 400),
            BackColor = Color.FromArgb(106, 190, 87),
            BorderStyle = BorderStyle.FixedSingle,
            AllowDrop = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        board.Paint += (_, e) =>
        {
            try
            {
                if (e.Graphics != null && board.ClientSize.Width > 0 && board.ClientSize.Height > 0)
                    DrawPitch(e.Graphics, board.ClientRectangle);
            }
            catch { /* A pitch redraw fault must never take down the message loop. */ }
        };
        board.DragEnter += (_, e) => e.Effect = e.Data?.GetDataPresent(typeof(int)) == true ? DragDropEffects.Copy : DragDropEffects.None;
        board.DragDrop += (_, e) => AssignDroppedPlayer(e, board);
        _formationBoard = board;
        CreateLineupSlots(board);
        pitch.Controls.Add(board);

        // Formation selector (bottom of pitch group)
        var formationLabel = new Label { Text = "Formation", Size = new Size(65, 20), Font = LegacyFont, BackColor = CardLayout.CardWhite };
        pitch.Controls.Add(formationLabel);
        _formationView.Location = new Point(80, 0);
        _formationView.Size = new Size(180, 21);
        _formationView.Font = LegacyFont;
        _formationView.DropDownHeight = 340;
        _formationView.SelectedIndexChanged += (_, _) =>
        {
            if (!_syncFormationView && _formationView.SelectedItem is FormationChoice choice)
                SelectTeamFormation(choice);
        };
        pitch.Controls.Add(_formationView);
        _formationStatus = new Label { Location = new Point(270, 0), Size = new Size(850, 20), Font = LegacyFont, ForeColor = Theme.Muted, BackColor = CardLayout.CardWhite, Visible = true, AutoEllipsis = true };
        pitch.Controls.Add(_formationStatus);
        ToolTip.SetToolTip(_formationView, "Choose a formation template for this team.");
        pitch.SizeChanged += (_, _) =>
        {
            var bottom = Math.Max(80, pitch.ClientSize.Height - 48);
            board.Bounds = new Rectangle(8, 28, Math.Max(220, pitch.ClientSize.Width - 16), Math.Max(140, bottom - 28));
            formationLabel.Location = new Point(10, bottom + 8);
            _formationView.Location = new Point(80, bottom + 6);
            _formationStatus.Location = new Point(270, bottom + 8);
            _formationStatus.Width = Math.Max(80, pitch.ClientSize.Width - 280);
        };

    }

    private Panel CreateMatchdayPanel(Point location, Size size)
    {
        var setPieces = Group("Matchday", location, size);
        AddPlayerReferencePickers(setPieces, new[] {
            ("Captain", "captainid"), ("Left Corner", "leftcornerkicktakerid"),
            ("Right Corner", "rightcornerkicktakerid"), ("Penalty", "penaltytakerid"),
            ("Free Kicks", "freekicktakerid")
        }, 8, 30, pickerX: 104, pickerWidth: 380);
        return setPieces;
    }

    private Label? _squadCountLabel;

    private void FindSelectedPlayer()
    {
        if (_teamPlayers.SelectedItems.Count > 0 && _teamPlayers.SelectedItems[0].Tag is int playerId && playerId > 0)
        {
            var recordIndex = FindTableRow("players", "playerid", playerId);
            if (recordIndex >= 0) Services.RequestRecordNavigation("players", recordIndex);
        }
    }

    private sealed class LineupSlot
    {
        public required Label Label { get; init; }
        public required string PlayerField { get; init; }
        public int PlayerId { get; set; }
        public int LoadedMinifacePlayerId { get; set; }
        public int AppliedMinifacePlayerId { get; set; }
        public string ExpectedPosition { get; set; } = string.Empty;
        public Point FormationPoint { get; set; }
    }

    private sealed record FormationChoice(int RecordIndex, int FormationId, string Name, bool IsGeneric)
    {
        public override string ToString() => Name;
    }

    /// <summary>
    /// Paints the miniface and its two-line label in separate regions.  The
    /// label has an outline/shadow for contrast but deliberately no caption
    /// tile, preserving the clean on-pitch style requested for the roster.
    /// </summary>
    private sealed class LineupMarker : Label
    {
        public LineupMarker()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Let WinForms render the parent pitch into this transparent child;
            // never draw the parent manually from here.
            base.OnPaintBackground(e);

            const int faceSize = 70;
            if (Image != null)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                var width = Math.Min(faceSize, Image.Width);
                var height = Math.Min(faceSize, Image.Height);
                e.Graphics.DrawImage(Image, new Rectangle((Width - width) / 2, 0, width, height));
            }

            var textBounds = new Rectangle(1, 70, Math.Max(1, Width - 2), Math.Max(1, Height - 70));
            const TextFormatFlags format = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top |
                                            TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
            // A compact black shadow makes white names readable on every pitch
            // stripe without reintroducing a filled rectangle behind the text.
            var shadowBounds = new Rectangle(textBounds.X + 1, textBounds.Y + 1, textBounds.Width, textBounds.Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, shadowBounds, Color.FromArgb(235, 0, 0, 0), format);
            TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, ForeColor, format);
        }
    }

    private void CreateLineupSlots(Panel board)
    {
        foreach (var _ in Enumerable.Range(0, 11))
        {
            // Keep the original clean pitch treatment: a circular miniface and
            // white text directly on the grass. A custom caption tile looked
            // heavier than the rest of CM26 and could leave paint artefacts.
            var label = new LineupMarker
            {
                Size = new Size(122, 104), BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None, TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White, AllowDrop = true, Tag = _lineupSlots.Count,
                ImageAlign = ContentAlignment.TopCenter,
                Padding = Padding.Empty
            };
            label.DragEnter += (_, e) => e.Effect = e.Data?.GetDataPresent(typeof(int)) == true ? DragDropEffects.Copy : DragDropEffects.None;
            label.DragDrop += (_, e) => AssignDroppedPlayer(e, label);
            board.Controls.Add(label);
            _lineupSlots.Add(new LineupSlot { Label = label, PlayerField = $"playerid{_lineupSlots.Count}" });
        }
        foreach (var slot in _lineupSlots) slot.Label.Visible = false;
    }

    private static void DrawPitch(Graphics graphics, Rectangle bounds)
    {
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var playable = Rectangle.Inflate(bounds, -10, -10);

        // Classic green pitch with subtle mowing bands.
        using (var turf = new System.Drawing.Drawing2D.LinearGradientBrush(playable,
                   Color.FromArgb(0, 110, 50), Color.FromArgb(0, 130, 60),
                   System.Drawing.Drawing2D.LinearGradientMode.Vertical))
        {
            graphics.FillRectangle(turf, playable);
        }
        using var band = new SolidBrush(Color.FromArgb(20, 255, 255, 255));
        var bandHeight = Math.Max(1, playable.Height / 10);
        for (var row = 0; row < 10; row += 2)
            graphics.FillRectangle(band, playable.Left, playable.Top + (row * bandHeight),
                playable.Width, bandHeight);

        // White lines.
        using var line = new Pen(Color.FromArgb(210, Color.White), 1.5f);
        graphics.DrawRectangle(line, playable);
        var centerY = playable.Top + (playable.Height / 2);
        graphics.DrawLine(line, playable.Left, centerY, playable.Right, centerY);

        var circleRadius = Math.Max(30, Math.Min(playable.Width, playable.Height) / 9);
        var cx = playable.Left + (playable.Width / 2);
        graphics.DrawEllipse(line, cx - circleRadius, centerY - circleRadius, circleRadius * 2, circleRadius * 2);
        graphics.FillEllipse(Brushes.White, cx - 3, centerY - 3, 6, 6);

        var penaltyWidth = Math.Max(140, playable.Width / 3);
        var penaltyHeight = Math.Max(50, playable.Height / 7);
        var penaltyLeft = playable.Left + ((playable.Width - penaltyWidth) / 2);
        graphics.DrawRectangle(line, penaltyLeft, playable.Top, penaltyWidth, penaltyHeight);
        graphics.DrawRectangle(line, penaltyLeft, playable.Bottom - penaltyHeight, penaltyWidth, penaltyHeight);

        var sixWidth = Math.Max(70, playable.Width / 7);
        var sixHeight = Math.Max(22, playable.Height / 18);
        var sixLeft = playable.Left + ((playable.Width - sixWidth) / 2);
        graphics.DrawRectangle(line, sixLeft, playable.Top, sixWidth, sixHeight);
        graphics.DrawRectangle(line, sixLeft, playable.Bottom - sixHeight, sixWidth, sixHeight);

        // Penalty spots.
        using var dot = new SolidBrush(Color.White);
        graphics.FillEllipse(dot, cx - 3, playable.Top + penaltyHeight - 3, 6, 6);
        graphics.FillEllipse(dot, cx - 3, playable.Bottom - penaltyHeight - 3, 6, 6);
    }

    private void AssignDroppedPlayer(DragEventArgs e, Control target)
    {
        if (e.Data?.GetData(typeof(int)) is not int playerId || playerId <= 0 || _lineupSlots.Count == 0) return;
        var slotIndex = target.Tag is int tagged ? tagged : NearestLineupSlot(_formationBoard?.PointToClient(new Point(e.X, e.Y)) ?? Point.Empty);
        if (slotIndex < 0 || slotIndex >= _lineupSlots.Count) return;
        if (_activeTeamSheetRow < 0)
        {
            MessageBox.Show(this, "This team does not have an editable lineup record.", "Starting Lineup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var sheet = Services.Session.GetTable("default_teamsheets");
        if (sheet?.FindColumn(_lineupSlots[slotIndex].PlayerField)?.IsWritable != true)
        {
            MessageBox.Show(this, "This starting-lineup slot is read-only.", "Starting Lineup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        foreach (var slot in _lineupSlots.Where(s => s.PlayerId == playerId && s != _lineupSlots[slotIndex]).ToList())
        {
            if (StageLineupField(slot, -1))
                slot.PlayerId = 0;
        }
        if (StageLineupField(_lineupSlots[slotIndex], playerId))
            _lineupSlots[slotIndex].PlayerId = playerId;
        RenderLineup();
    }

    /// <summary>
    /// FC26 retains the default XI in <c>default_teamsheets</c>, then copies it
    /// into one or more <c>default_mentalities</c> tactic records.  Updating only
    /// the former is the source of many tools showing a correct XI while the game
    /// keeps an old one.  Mirror only rows whose current player matches the source
    /// slot, so tactic-specific variations are never overwritten blindly.
    /// </summary>
    private bool StageLineupField(LineupSlot slot, int playerId)
    {
        var previousPlayerId = slot.PlayerId;
        if (!StageField("default_teamsheets", _activeTeamSheetRow, slot.PlayerField, playerId.ToString(), _stagingGrid))
            return false;

        var mirrored = MirrorDefaultMentalitiesSlot(slot.PlayerField, previousPlayerId, playerId);
        if (_formationStatus != null)
        {
            _formationStatus.Text = mirrored > 0
                ? $"Starting XI updated across {mirrored} tactic profile{(mirrored == 1 ? string.Empty : "s")}."
                : "Starting XI updated.";
        }
        return true;
    }

    private int MirrorDefaultMentalitiesSlot(string playerField, int previousPlayerId, int playerId)
    {
        // A blank default-sheet slot has no authoritative player to mirror.  Do
        // not populate every tactic's same numeric slot: those tactics may hold
        // a deliberately different player or formation.
        if (_activeLineupTeamId <= 0 || previousPlayerId <= 0) return 0;

        var mentalities = Services.Session.GetTable("default_mentalities");
        var teamColumn = mentalities == null ? -1 : Col(mentalities, "teamid");
        var playerColumn = mentalities == null ? -1 : Col(mentalities, playerField);
        if (mentalities == null || teamColumn < 0 || playerColumn < 0 || !mentalities.Columns[playerColumn].IsWritable)
            return 0;

        var mirrored = 0;
        for (var row = 0; row < mentalities.RowCount; row++)
        {
            var record = Services.Session.GetRecord("default_mentalities", row);
            if (record == null || Parse(record.Get(teamColumn)) != _activeLineupTeamId || Parse(record.Get(playerColumn)) != previousPlayerId)
                continue;
            if (StageField("default_mentalities", row, playerField, playerId.ToString(), _stagingGrid))
                mirrored++;
        }
        return mirrored;
    }

    private int NearestLineupSlot(Point point)
    {
        var best = -1; var distance = int.MaxValue;
        for (var i = 0; i < _lineupSlots.Count; i++)
        {
            var centre = new Point(_lineupSlots[i].Label.Left + (_lineupSlots[i].Label.Width / 2), _lineupSlots[i].Label.Top + (_lineupSlots[i].Label.Height / 2));
            var d = ((centre.X - point.X) * (centre.X - point.X)) + ((centre.Y - point.Y) * (centre.Y - point.Y));
            if (d < distance) { distance = d; best = i; }
        }
        return best;
    }

    private void LoadLineup(int teamId, IReadOnlyList<TeamRosterItem> roster)
    {
        _activeLineupTeamId = teamId;
        _rosterByPlayerId.Clear();
        foreach (var player in roster) _rosterByPlayerId[player.PlayerId] = player;
        foreach (var slot in _lineupSlots) slot.PlayerId = 0;
        _matchdayBench.Items.Clear();
        _activeTeamSheetRow = -1;
        var sheets = Services.Session.GetTable("default_teamsheets");
        if (sheets != null)
        {
            var teamColumn = Col(sheets, "teamid");
            for (var row = 0; row < sheets.RowCount; row++)
            {
                var record = Services.Session.GetRecord("default_teamsheets", row);
                if (record == null || Parse(record.Get(teamColumn)) != teamId) continue;
                _activeTeamSheetRow = row;
                foreach (var slot in _lineupSlots)
                    slot.PlayerId = Parse(record.Get(Col(sheets, slot.PlayerField)));
                for (var benchIndex = 11; benchIndex <= 17; benchIndex++)
                {
                    var playerId = Parse(record.Get(Col(sheets, $"playerid{benchIndex}")));
                    if (playerId <= 0) continue;
                    if (_rosterByPlayerId.TryGetValue(playerId, out var benchPlayer))
                    {
                        var item = new ListViewItem(benchPlayer.JerseyNumber.ToString());
                        item.SubItems.Add(benchPlayer.Name);
                        item.SubItems.Add(DisplayLineupPosition(benchPlayer.Position, string.Empty));
                        item.Tag = playerId;
                        _matchdayBench.Items.Add(item);
                    }
                    else
                    {
                        var item = new ListViewItem("—");
                        item.SubItems.Add($"Player {playerId}");
                        item.SubItems.Add("—");
                        item.Tag = playerId;
                        _matchdayBench.Items.Add(item);
                    }
                }
                break;
            }
        }
        RenderLineup();
    }

    private void ApplyFormationLayout(FormationChoice choice, string? status = null)
    {
        var table = Services.Session.GetTable("formations");
        var record = table == null ? null : Services.Session.GetRecord("formations", choice.RecordIndex);
        if (table == null || record == null || _formationBoard == null) return;
        for (var i = 0; i < _lineupSlots.Count; i++)
        {
            var x = ReadFormationOffset(table, record, $"offset{i}x", _formationBoard.Width);
            var y = ReadFormationOffset(table, record, $"offset{i}y", _formationBoard.Height);
            var positionColumn = Col(table, $"position{i}");
            var slot = _lineupSlots[i];
            // FC26's Y axis starts at the defending goal.  Reverse it once for
            // the broadcast-style board: attack at the top, goalkeeper below.
            slot.FormationPoint = new Point(x, _formationBoard.Height - y);
            slot.ExpectedPosition = positionColumn >= 0 ? NameResolverService.PositionLabel(Parse(record.Get(positionColumn))) : "Not stored";
            slot.Label.Visible = true;
        }
        ApplyGoalkeeperVisualClearance();
        ArrangeLineupInTacticalLanes();
        _formationStatus!.Text = status ?? choice.Name;
        RenderLineup();
    }

    /// <summary>
    /// Draw the exact tactical points stored in the FC26 formations table.
    /// A defensive line intentionally has small Y differences (full-backs and
    /// centre-backs), so grouping or flattening those points creates a different
    /// formation. The only conversion is the display's reversed Y axis.
    /// </summary>
    private void ArrangeLineupInTacticalLanes()
    {
        if (_formationBoard == null) return;
        foreach (var slot in _lineupSlots.Where(slot => slot.Label.Visible))
            PlaceLineupSlot(slot);
    }

    /// <summary>
    /// The stored offsets remain the source of truth. This only protects the
    /// rendered miniface/text bounds at the goal line: otherwise a full-size
    /// goalkeeper marker is clamped upward onto the defenders.
    /// </summary>
    private void ApplyGoalkeeperVisualClearance()
    {
        if (_formationBoard == null) return;
        var defenders = _lineupSlots.Where(slot => slot.Label.Visible && IsDefender(slot.ExpectedPosition)).ToList();
        if (defenders.Count == 0) return;

        var lastDefenderY = defenders.Max(slot => slot.FormationPoint.Y);
        foreach (var goalkeeper in _lineupSlots.Where(slot => slot.Label.Visible && string.Equals(slot.ExpectedPosition, "GK", StringComparison.OrdinalIgnoreCase)))
        {
            // 112 px keeps the face and its two text lines visibly clear of the
            // defending line. The bottom bound leaves the full label on pitch.
            var bottomCentre = _formationBoard.Height - (goalkeeper.Label.Height / 2) - 2;
            var safeY = Math.Min(bottomCentre, Math.Max(goalkeeper.FormationPoint.Y, lastDefenderY + 112));
            goalkeeper.FormationPoint = new Point(goalkeeper.FormationPoint.X, safeY);
        }
    }

    private static bool IsDefender(string position) => position.ToUpperInvariant() is "LB" or "LWB" or "LCB" or "CB" or "RCB" or "RB" or "RWB";

    private void PlaceLineupSlot(LineupSlot slot)
    {
        if (_formationBoard == null) return;
        var horizontalMargin = 6;
        var verticalMargin = string.Equals(slot.ExpectedPosition, "GK", StringComparison.OrdinalIgnoreCase) ? 2 : 6;
        var maxLeft = Math.Max(horizontalMargin, _formationBoard.Width - slot.Label.Width - horizontalMargin);
        var maxTop = Math.Max(verticalMargin, _formationBoard.Height - slot.Label.Height - verticalMargin);
        slot.Label.Location = new Point(
            Math.Clamp(slot.FormationPoint.X - (slot.Label.Width / 2), horizontalMargin, maxLeft),
            Math.Clamp(slot.FormationPoint.Y - (slot.Label.Height / 2), verticalMargin, maxTop));
    }

    private static int ReadFormationOffset(CM26.Application.Models.DbTable table, CM26.Application.Models.DbRecord record, string field, int extent)
    {
        var column = Col(table, field);
        if (column < 0 || extent <= 20) return Math.Max(10, extent / 2);
        var raw = record.Get(column);
        var value = double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 0d;
        var normalized = value is >= 0d and <= 1d ? value : value / 100d;
        var max = Math.Max(10, extent - 10);
        return Math.Clamp((int)Math.Round(Math.Clamp(normalized, 0d, 1d) * (extent - 20)) + 10, Math.Min(10, max), max);
    }

    private void SelectFormationLayout(int teamId)
    {
        var genericChoices = new List<FormationChoice>();
        var teamChoices = new List<FormationChoice>();
        _activeTeamFormationRow = -1;
        var formations = Services.Session.GetTable("formations");
        if (formations != null)
        {
            var teamColumn = Col(formations, "teamid");
            var nameColumn = Col(formations, "formationname");
            var idColumn = Col(formations, "formationid");
            if (teamColumn < 0 || nameColumn < 0 || idColumn < 0)
            {
                _formationView.Items.Clear();
                _formationView.Enabled = false;
                _formationStatus!.Text = "Formation columns are unavailable in this database.";
                return;
            }
            for (var row = 0; row < formations.RowCount; row++)
            {
                var record = Services.Session.GetRecord("formations", row);
                if (record == null || !int.TryParse(record.Get(teamColumn), out var owner)) continue;
                var name = record.Get(nameColumn);
                var formationId = Parse(record.Get(idColumn));
                if (owner <= 0 && !string.IsNullOrWhiteSpace(name))
                {
                    genericChoices.Add(new FormationChoice(row, formationId, name, IsGeneric: true));
                }
                else if (owner == teamId)
                {
                    // A team can have more than one linked style row in custom
                    // databases. Keep the first stable link instead of letting
                    // the last row silently replace it during enumeration.
                    if (_activeTeamFormationRow < 0) _activeTeamFormationRow = row;
                    teamChoices.Add(new FormationChoice(row, formationId, string.IsNullOrWhiteSpace(name) ? $"Team formation #{row + 1}" : name, IsGeneric: false));
                }
            }
        }
        var choices = genericChoices
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (choices.Count == 0) choices = teamChoices;
        var selected = _activeTeamFormationRow >= 0 ? teamChoices.FirstOrDefault() : null;
        if (selected != null)
        {
            var matchingGeneric = choices.FirstOrDefault(c => c.Name.Equals(selected.Name, StringComparison.OrdinalIgnoreCase));
            if (matchingGeneric != null) selected = matchingGeneric;
            else if (!choices.Contains(selected)) choices.Insert(0, selected);
        }
        _syncFormationView = true;
        try
        {
            _formationView.Items.Clear();
            _formationView.Items.AddRange(choices.Cast<object>().ToArray());
            _formationView.Enabled = choices.Count > 0;
            _formationView.SelectedItem = selected ?? choices.FirstOrDefault();
        }
        finally { _syncFormationView = false; }
        if (_formationView.SelectedItem is FormationChoice initial) ApplyFormationLayout(initial);
        else
        {
            foreach (var slot in _lineupSlots) slot.Label.Visible = false;
            _formationStatus!.Text = "No formation is linked to this team.";
        }
    }

    private void SelectTeamFormation(FormationChoice choice)
    {
        ApplyFormationLayout(choice);
        if (_activeTeamFormationRow < 0 || !choice.IsGeneric) return;
        var table = Services.Session.GetTable("formations");
        var relationship = table?.FindColumn("relativeformationid");
        if (table == null || relationship?.IsWritable != true)
        {
            _formationStatus!.Text = $"Previewing {choice.Name}; this team link is read-only.";
            return;
        }
        if (StageField("formations", _activeTeamFormationRow, relationship.Name, choice.FormationId.ToString(), _stagingGrid))
            _formationStatus!.Text = choice.Name;
    }

    private static int PositionOrder(string position) => position.ToUpperInvariant() switch
    {
        "GK" => 0, "LB" or "LWB" or "CB" or "RB" or "RWB" => 1,
        "CDM" or "CM" or "LM" or "RM" or "CAM" => 2, _ => 3,
    };

    private void RenderLineup()
    {
        foreach (var slot in _lineupSlots)
        {
            if (_rosterByPlayerId.TryGetValue(slot.PlayerId, out var player))
            {
                var position = DisplayLineupPosition(player.Position, slot.ExpectedPosition);
                slot.Label.Text = $"{DisplayLineupName(player.Name)}\n{position}  •  OVR {player.Overall}";
                ToolTip.SetToolTip(slot.Label, $"{player.Name} · {player.Position} · {player.Overall}\nDrag a roster player here to replace.");
                QueueLineupMiniface(slot, slot.PlayerId);
            }
            else if (slot.PlayerId > 0)
            {
                var playerName = Services.Resolver?.PlayerNameByPlayerId(slot.PlayerId) ?? $"Player {slot.PlayerId}";
                slot.Label.Text = $"{DisplayLineupName(playerName)}\n{DisplayLineupPosition(string.Empty, slot.ExpectedPosition)}  •  OVR —";
                ToolTip.SetToolTip(slot.Label, $"{playerName} is in the saved lineup but is not currently linked to the club roster.");
                QueueLineupMiniface(slot, slot.PlayerId);
            }
            else
            {
                slot.Label.Text = $"Empty slot\n{DisplayLineupPosition(string.Empty, slot.ExpectedPosition)}";
                ToolTip.SetToolTip(slot.Label, "Drag a player from Team Players onto this slot.");
                ClearLineupMiniface(slot);
            }
        }
    }

    private static string DisplayLineupPosition(string position, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(position) && !position.Equals("Not stored", StringComparison.OrdinalIgnoreCase) && position != "-")
            return position;
        return string.IsNullOrWhiteSpace(fallback) || fallback.Equals("Not stored", StringComparison.OrdinalIgnoreCase) ? "Position —" : fallback;
    }

    private static string DisplayLineupName(string name)
    {
        if (name.Length <= 18) return name;
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}. {parts[^1]}" : name[..18];
    }

    private int FindTableRow(string tableName, string fieldName, int value)
    {
        var table = Services.Session.GetTable(tableName);
        if (table == null) return -1;
        for (var row = 0; row < table.RowCount; row++)
            if (Parse(Services.Session.GetCell(tableName, row, fieldName)) == value)
                return row;
        return -1;
    }

    private void QueueLineupMiniface(LineupSlot slot, int playerId)
    {
        if (playerId <= 0) return;
        if (slot.LoadedMinifacePlayerId == playerId && slot.AppliedMinifacePlayerId == playerId) return;

        // Never leave the previous player's face visible while an asset loads
        // (or if that asset is unavailable). This is essential when changing
        // teams because each visual slot is reused for a different player.
        if (slot.LoadedMinifacePlayerId != playerId)
        {
            var old = slot.Label.Image;
            slot.Label.Image = null;
            slot.AppliedMinifacePlayerId = 0;
            old?.Dispose();
            slot.Label.Invalidate();
        }
        slot.LoadedMinifacePlayerId = playerId;
        _ = Task.Run(async () => await LoadPlayerMinifaceAsync(playerId, 76))
        .ContinueWith(task =>
        {
            var image = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
            if (image is null) return;
            if (IsDisposed || slot.LoadedMinifacePlayerId != playerId)
            {
                image.Dispose();
                return;
            }
            SetLineupMiniface(slot, playerId, image);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static void ClearLineupMiniface(LineupSlot slot)
    {
        slot.LoadedMinifacePlayerId = 0;
        slot.AppliedMinifacePlayerId = 0;
        var old = slot.Label.Image;
        slot.Label.Image = null;
        old?.Dispose();
    }

    private static void SetLineupMiniface(LineupSlot slot, int playerId, Image image)
    {
        if (slot.LoadedMinifacePlayerId != playerId)
        {
            image.Dispose();
            return;
        }
        var old = slot.Label.Image;
        slot.Label.Image = CreateCircularMiniface(image, 76);
        slot.AppliedMinifacePlayerId = playerId;
        image.Dispose();
        old?.Dispose();
        slot.Label.Invalidate();
    }

    private async Task<Image?> LoadPlayerMinifaceAsync(int playerId, int size)
    {
        await _minifaceLoadGate.WaitAsync().ConfigureAwait(false);
        try
        {
            var path = Services.Assets.GetPlayerMiniface(playerId);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                path = Services.FrostbiteAssets.IsAvailable
                    ? Services.FrostbiteAssets.ExportLegacyAsset($"data/ui/imgAssets/heads/p{playerId}.dds")
                    : null;
            return string.IsNullOrWhiteSpace(path) ? null : Services.Textures.CreatePreview(path, size, size);
        }
        finally { _minifaceLoadGate.Release(); }
    }

    private static Image CreateCircularMiniface(Image source, int diameter)
    {
        var image = new Bitmap(diameter, diameter);
        using var graphics = Graphics.FromImage(image);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddEllipse(1, 1, diameter - 2, diameter - 2);
        graphics.SetClip(path);
        graphics.DrawImage(source, new Rectangle(0, 0, diameter, diameter));
        graphics.ResetClip();
        using var border = new Pen(Color.FromArgb(235, Color.White), 2);
        graphics.DrawEllipse(border, 1, 1, diameter - 3, diameter - 3);
        return image;
    }

    private void AddAdboardsTab()
    {
        var page = Page("Adboards");
        var canvas = Canvas(page);
        var sources = Group("Team Adboard Sources", new Point(3, 3), new Size(600, 621));
        _adboardSources.Location = new Point(10, 30);
        _adboardSources.Size = new Size(580, 590);
        _adboardSources.View = View.Details;
        _adboardSources.FullRowSelect = true;
        _adboardSources.GridLines = true;
        _adboardSources.Font = LegacyFont;
        _adboardSources.BackColor = Theme.Input;
        _adboardSources.ForeColor = Theme.Text;
        _adboardSources.Columns.Add("Sponsor", 235);
        _adboardSources.Columns.Add("Sponsor ID", 90);
        _adboardSources.Columns.Add("Dynamic Image", 115);
        _adboardSources.Columns.Add("Approved", 80);
        _adboardSources.SelectedIndexChanged += (_, _) => ShowSelectedAdboard();
        sources.Controls.Add(_adboardSources);
        canvas.Controls.Add(sources);

        var preview = Group("Adboard / Dynamic Sponsor Preview", new Point(610, 3), new Size(620, 420));
        _adboardPreview.Location = new Point(10, 30);
        _adboardPreview.Size = new Size(600, 320);
        _adboardPreview.BackColor = Theme.Input;
        _adboardPreview.BorderStyle = BorderStyle.FixedSingle;
        _adboardPreview.SizeMode = PictureBoxSizeMode.Zoom;
        preview.Controls.Add(_adboardPreview);
        _adboardCaption.Location = new Point(10, 350);
        _adboardCaption.Size = new Size(600, 48);
        _adboardCaption.Font = LegacyFont;
        _adboardCaption.TextAlign = ContentAlignment.MiddleCenter;
        _adboardCaption.Text = "Select a linked sponsor or adboard source";
        preview.Controls.Add(_adboardCaption);
        LegacyAssetActions.Attach(Services, preview, _adboardPreview, new Point(10, 388), ShowSelectedAdboard);
        canvas.Controls.Add(preview);

        var note = Group("Asset Mapping", new Point(610, 430), new Size(620, 126));
        note.Controls.Add(new Label
        {
            Text = "Adboard content is driven by teamsponsorlinks and its dynamicimageid. " +
                   "This view shows the exact database links and resolves the corresponding installed artwork when available.",
            Location = new Point(14, 30), Size = new Size(590, 95), Font = LegacyFont,
            ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        canvas.Controls.Add(note);
    }

    private void AddSponsorsTab()
    {
        var page = Page("Sponsors");
        var canvas = Canvas(page);
        var links = Group("Team Sponsor Links", new Point(3, 3), new Size(1120, 629));
        _teamSponsors.Location = new Point(12, 30);
        _teamSponsors.Size = new Size(660, 565);
        _teamSponsors.View = View.Details;
        _teamSponsors.FullRowSelect = true;
        _teamSponsors.GridLines = true;
        _teamSponsors.Font = LegacyFont;
        _teamSponsors.BackColor = Theme.Input;
        _teamSponsors.ForeColor = Theme.Text;
        _teamSponsors.Columns.Add("Sponsor", 255);
        _teamSponsors.Columns.Add("Approved", 90);
        _teamSponsors.Columns.Add("Dynamic Image", 130);
        _teamSponsors.Columns.Add("Link Key", 100);
        _teamSponsors.SelectedIndexChanged += (_, _) => ShowSelectedSponsor();
        links.Controls.Add(_teamSponsors);
        _sponsorPreview.Location = new Point(690, 35);
        _sponsorPreview.Size = new Size(400, 250);
        _sponsorPreview.BackColor = Theme.Input;
        _sponsorPreview.BorderStyle = BorderStyle.FixedSingle;
        _sponsorPreview.SizeMode = PictureBoxSizeMode.Zoom;
        links.Controls.Add(_sponsorPreview);
        _sponsorPreviewCaption.Location = new Point(690, 292);
        _sponsorPreviewCaption.Size = new Size(400, 36);
        _sponsorPreviewCaption.Font = LegacyFont;
        _sponsorPreviewCaption.TextAlign = ContentAlignment.MiddleCenter;
        _sponsorPreviewCaption.Text = "Select a sponsor link to preview its dynamic image";
        links.Controls.Add(_sponsorPreviewCaption);
        LegacyAssetActions.Attach(Services, links, _sponsorPreview, new Point(690, 335), ShowSelectedSponsor);
        links.Controls.Add(new Label
        {
            Text = "Select a sponsor relationship to preview its linked dynamic image.",
            Location = new Point(12, 595), Size = new Size(1075, 28), Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        canvas.Controls.Add(links);
    }

    private void AddFlagsTab()
    {
        var page = Page("Flags");
        var canvas = Canvas(page);
        var texture = Group("Team Flags", new Point(3, 3), new Size(525, 346));
        _teamFlagPreview.Location = new Point(10, 30);
        _teamFlagPreview.Size = new Size(512, 256);
        _teamFlagPreview.BackColor = Theme.Input;
        _teamFlagPreview.BorderStyle = BorderStyle.FixedSingle;
        _teamFlagPreview.SizeMode = PictureBoxSizeMode.Zoom;
        texture.Controls.Add(_teamFlagPreview);
        LegacyAssetActions.Attach(Services, texture, _teamFlagPreview, new Point(10, 286), RefreshTeamFlags);
        _teamFlagCaption.Location = new Point(10, 312);
        _teamFlagCaption.Size = new Size(505, 28);
        _teamFlagCaption.Font = LegacyFont;
        _teamFlagCaption.TextAlign = ContentAlignment.MiddleCenter;
        texture.Controls.Add(_teamFlagCaption);
        canvas.Controls.Add(texture);
        var flag = Group("Flags", new Point(534, 3), new Size(525, 346));
        _nationFlagPreview.Location = new Point(10, 30);
        _nationFlagPreview.Size = new Size(512, 256);
        _nationFlagPreview.BackColor = Theme.Input;
        _nationFlagPreview.BorderStyle = BorderStyle.FixedSingle;
        _nationFlagPreview.SizeMode = PictureBoxSizeMode.Zoom;
        flag.Controls.Add(_nationFlagPreview);
        flag.Controls.Add(new Label { Text = "Flag n.", Location = new Point(12, 294), AutoSize = true, Font = LegacyFont });
        _nationFlagCaption.Location = new Point(65, 286);
        _nationFlagCaption.Size = new Size(450, 26);
        _nationFlagCaption.Font = LegacyFont;
        _nationFlagCaption.TextAlign = ContentAlignment.MiddleLeft;
        flag.Controls.Add(_nationFlagCaption);
        canvas.Controls.Add(flag);
    }

    private void AddBoundFields(Control parent, IEnumerable<(string label, string field)> definitions, int labelX, int top, int editorX, int editorWidth, int rowHeight)
    {
        var row = 0;
        // Fixed-width right-aligned captions (ellipsized with a tooltip when
        // narrow) so long names never slide under their editors.
        var labelWidth = Math.Max(70, editorX - labelX - 6);
        foreach (var (label, field) in definitions)
        {
            var y = top + (row++ * rowHeight);
            parent.Controls.Add(new Label
            {
                Text = label, Location = new Point(labelX, y + 3), Size = new Size(labelWidth, 18),
                AutoSize = false, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleRight, Font = LegacyFont,
                ForeColor = Theme.Muted, BackColor = Theme.Panel
            });
            var editor = new TextBox { Location = new Point(editorX, y), Size = new Size(editorWidth, 20), Font = LegacyFont, Tag = field };
            Theme.ApplyTextBox(editor);
            editor.Leave += (_, _) => StageEditor(editor);
            _editors.Add(editor);
            parent.Controls.Add(editor);
        }
    }

    // FC26 player-id foreign keys must be editable as relationships, not merely resolved text.
    private void AddPlayerReferencePickers(Control parent, IEnumerable<(string label, string field)> definitions, int labelX, int top, int pickerX = 90, int pickerWidth = 372)
    {
        var row = 0;
        var labelWidth = Math.Max(70, pickerX - labelX - 6);
        foreach (var (label, field) in definitions)
        {
            var y = top + (row++ * 26);
            parent.Controls.Add(new Label
            {
                Text = label, Location = new Point(labelX, y + 3), Size = new Size(labelWidth, 18),
                AutoSize = false, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleRight, Font = LegacyFont,
                ForeColor = Theme.Muted, BackColor = Theme.Panel
            });
            var picker = new ComboBox { Location = new Point(pickerX, y), Size = new Size(pickerWidth, 21), Font = LegacyFont, DropDownStyle = ComboBoxStyle.DropDownList, Tag = field };
            Theme.ApplyCombo(picker);
            picker.SelectedIndexChanged += (_, _) => CommitPlayerReference(picker);
            _playerReferencePickers[field] = picker;
            parent.Controls.Add(picker);
        }
    }

    private sealed record PlayerReferenceItem(int PlayerId, string Name)
    {
        public override string ToString() => PlayerId > 0 ? $"{Name}  [{PlayerId}]" : "None";
    }

    private void PopulatePlayerReferencePickers(IReadOnlyList<TeamRosterItem> roster)
    {
        _syncPlayerReferencePickers = true;
        try
        {
            foreach (var (field, picker) in _playerReferencePickers)
            {
                var current = _fields.TryGetValue(field, out var value) && int.TryParse(value.RawValue, out var id) ? id : 0;
                picker.BeginUpdate();
                picker.Items.Clear();
                picker.Items.Add(new PlayerReferenceItem(0, "None"));
                foreach (var player in roster.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase))
                    picker.Items.Add(new PlayerReferenceItem(player.PlayerId, player.Name));
                picker.SelectedIndex = Enumerable.Range(0, picker.Items.Count).FirstOrDefault(i => picker.Items[i] is PlayerReferenceItem item && item.PlayerId == current);
                picker.Enabled = _fields.TryGetValue(field, out var writable) && writable.IsWritable;
                picker.EndUpdate();
            }
        }
        finally { _syncPlayerReferencePickers = false; }
    }

    private void CommitPlayerReference(ComboBox picker)
    {
        if (_syncPlayerReferencePickers || CurrentRecordIndex < 0 || picker.Tag is not string field || picker.SelectedItem is not PlayerReferenceItem item || !_fields.TryGetValue(field, out var value) || !value.IsWritable)
            return;
        StageField(TableName, CurrentRecordIndex, value.FieldName, item.PlayerId.ToString(), _stagingGrid);
    }

    private void StageEditor(TextBox editor)
    {
        if (CurrentRecordIndex < 0) return;
        var field = editor.Tag as string ?? string.Empty;
        if (!_fields.TryGetValue(field, out var value) || !value.IsWritable) return;
        StageField(TableName, CurrentRecordIndex, value.FieldName, editor.Text, _stagingGrid);
    }

    private static void SetRatingBar(Panel bar, string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var num) || num <= 0)
        {
            bar.Width = Math.Max(8, (bar.Parent?.Width - 2 ?? 160) / 10);
            bar.BackColor = Color.FromArgb(60, bar.BackColor);
            return;
        }
        if (bar.Tag is Color accent) bar.BackColor = accent;
        bar.Width = Math.Max(8, Math.Min(bar.Parent?.Width - 2 ?? 160, (int)((double)num / max * 160)));
    }

    private static void LoadKitPreview(PictureBox preview, string? path)
    {
        preview.Image = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                preview.Image = Image.FromFile(path);
        }
        catch { }
    }

    protected override void ShowRecord(int recordIndex)
    {
        var table = Services.Session.GetTable(TableName)!;
        var record = Services.Session.GetRecord(TableName, recordIndex)!;
        var name = record.Get(Col(table, "teamname"));
        var id = record.Get(Col(table, "teamid"));
        int.TryParse(id, out var crestTeamId);
        try
        {
            ShowCrest(Services.Assets.GetTeamLogo(crestTeamId), name, crestTeamId);
        }
        catch { /* Crest preview failure must not prevent the record from loading. */ }
        _fields.Clear();
        foreach (var field in Services.RequireData().GetFields(TableName, recordIndex, LabelMaps.Teams))
            _fields[field.FieldName] = field;
        UpdateTeamColours();
        var countryColumn = Col(table, "countryid");
        try
        {
            ShowTeamFlags(crestTeamId, countryColumn >= 0 ? Parse(record.Get(countryColumn)) : 0);
        }
        catch { /* Flag preview failure must not prevent the record from loading. */ }

        foreach (var editor in _editors)
        {
            var key = editor.Tag as string ?? string.Empty;
            // Player-reference fields (captain / corner / penalty / free-kick takers) resolve to a
            // player display name — a verified real name, or "Player {id}" — never a bare key.
            if (IsPlayerReferenceField(key) && _fields.TryGetValue(key, out var pref) &&
                int.TryParse(pref.RawValue, out var refPlayerId) && refPlayerId > 0)
            {
                editor.Text = Services.Resolver?.PlayerNameByPlayerId(refPlayerId) ?? $"Player {refPlayerId}";
                editor.ReadOnly = true;
                editor.BackColor = CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardSubtle;
                ToolTip.SetToolTip(editor, $"{key} = {pref.RawValue} (player id)");
            }
            else if (IsLinkedDisplayField(key))
            {
                // FC26 stores some team relationships in dedicated link tables.  Present the
                // resolved name in the CM16-style form instead of an empty/-1 raw FK.
                editor.Text = ResolveLinkedValue(key, int.TryParse(id, out var linkedTeamId) ? linkedTeamId : 0);
                editor.ReadOnly = true;
                editor.BackColor = CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardSubtle;
                ToolTip.SetToolTip(editor, $"Resolved {key}; select the linked player or roster control to change it.");
            }
            else if (_fields.TryGetValue(key, out var value))
            {
                editor.Text = value.Value;
                editor.ReadOnly = !value.IsWritable;
                editor.BackColor = value.IsWritable ? Theme.Input : CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardText;
            }
            else
            {
                editor.Text = ResolveLinkedValue(key, int.TryParse(id, out var linkedTeamId) ? linkedTeamId : 0);
                editor.ReadOnly = true;
                editor.BackColor = CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardSubtle;
            }
        }

        // ── Populate FC Tools Hub club profile ──────────────────────────
        _teamNameLabel.Text = name ?? string.Empty;
        _teamMetaLabel.Text = $"{ResolveLinkedValue("leagueid", crestTeamId)} · {ResolveLinkedValue("countryid", crestTeamId)}";
        try
        {
            var crestPath = Services.Assets.GetTeamLogo(crestTeamId);
            if (!string.IsNullOrWhiteSpace(crestPath) && File.Exists(crestPath))
                _teamCrestPreview.Image = Image.FromFile(crestPath);
            else
                _teamCrestPreview.Image = null;
        }
        catch { _teamCrestPreview.Image = null; }

        // Overall rating
        var ovr = record.Get(Col(table, "overallrating"));
        _teamOverallLabel.Text = ovr;
        SetRatingBar(_teamOvrBar, ovr, 99);
        SetRatingBar(_teamAttBar, record.Get(Col(table, "attackrating")), 99);
        SetRatingBar(_teamMidBar, record.Get(Col(table, "midfieldrating")), 99);
        SetRatingBar(_teamDefBar, record.Get(Col(table, "defenserating")), 99);

        // Quick info
        _teamFoundationLabel.Text = record.Get(Col(table, "foundationyear")) ?? "—";
        _teamWorthLabel.Text = record.Get(Col(table, "clubworth")) ?? "—";
        _teamStadiumLabel.Text = ResolveLinkedValue("stadiumid", crestTeamId);
        _teamRivalLabel.Text = "—";
        _teamManagerName.Text = ResolveLinkedValue("managerid", crestTeamId);
        _teamManagerNation.Text = "";

        // Kit previews — use crest as fallback since no kit asset resolver
        try
        {
            var crestPath = Services.Assets.GetTeamLogo(crestTeamId);
            LoadKitPreview(_teamKitHome, crestPath);
            LoadKitPreview(_teamKitAway, crestPath);
            LoadKitPreview(_teamKitThird, crestPath);
            LoadKitPreview(_teamKitGk, crestPath);
        }
        catch { }

        // Stadium image
        try
        {
            var stadiumPath = Services.Assets.GetStadium(crestTeamId);
            if (!string.IsNullOrWhiteSpace(stadiumPath) && System.IO.File.Exists(stadiumPath))
                LoadKitPreview(_teamStadiumImg, stadiumPath);
            else
                _teamStadiumImg.Image = null;
        }
        catch { _teamStadiumImg.Image = null; }

        // Manager image
        try
        {
            var mgrId = record.Get(Col(table, "managerid"));
            if (int.TryParse(mgrId, out var mid) && mid > 0)
            {
                var mgrPath = Services.Assets.GetManagerFace(mid);
                LoadKitPreview(_teamManagerImg, mgrPath);
            }
            else _teamManagerImg.Image = null;
        }
        catch { _teamManagerImg.Image = null; }

        _rosterMinifaces.Images.Clear();
        _pendingRosterMinifaces.Clear();
        _teamPlayers.Items.Clear();
        try
        {
            var roster = Services.RequireData().GetTeamRoster(int.TryParse(id, out var teamId) ? teamId : 0);
            LoadLineup(teamId, roster);
            SelectFormationLayout(teamId);
            // Force the formation board to repaint with all lineup slots.
            foreach (var slot in _lineupSlots)
            {
                if (slot.PlayerId > 0)
                {
                    slot.Label.Visible = true;
                    slot.Label.Invalidate();
                }
            }
            _formationBoard?.Invalidate(true);
            PopulatePlayerReferencePickers(roster);

            // Categorize players: Starting XI, Substitutes, Reserves
            var lineupIds = new HashSet<int>();
            var subIds = new HashSet<int>();
            foreach (var slot in _lineupSlots)
                if (slot.PlayerId > 0) lineupIds.Add(slot.PlayerId);
            foreach (ListViewItem sub in _matchdayBench.Items)
                if (sub.Tag is int subId && subId > 0) subIds.Add(subId);

            var startingXi = roster.Where(p => lineupIds.Contains(p.PlayerId)).ToList();
            var subs = roster.Where(p => subIds.Contains(p.PlayerId) && !lineupIds.Contains(p.PlayerId)).ToList();
            var reserves = roster.Where(p => !lineupIds.Contains(p.PlayerId) && !subIds.Contains(p.PlayerId)).ToList();

            _teamPlayers.BeginUpdate();
            try
            {
                // Section: Starting XI
                AddSectionHeader("STARTING XI", startingXi.Count);
                foreach (var player in startingXi.OrderBy(p => PositionOrder(p.Position)))
                    AddPlayerRow(player, "XI");

                // Section: Substitutes
                if (subs.Count > 0)
                {
                    AddSectionHeader("SUBSTITUTES", subs.Count);
                    foreach (var player in subs.OrderBy(p => PositionOrder(p.Position)))
                        AddPlayerRow(player, "SUB");
                }

                // Section: Reserves
                if (reserves.Count > 0)
                {
                    AddSectionHeader("RESERVES", reserves.Count);
                    foreach (var player in reserves.OrderBy(p => PositionOrder(p.Position)))
                        AddPlayerRow(player, "RES");
                }

                if (roster.Count == 0)
                    _teamPlayers.Items.Add(new ListViewItem(new[] { "", "No players linked to this team", "", "", "" }));
            }
            finally { _teamPlayers.EndUpdate(); }

            // Update squad count
            if (_squadCountLabel != null)
                _squadCountLabel.Text = $"Squad ({roster.Count} players)";

            LoadSponsors(teamId);
            LoadAudioCatalogs();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TeamsSection] Roster load error: {ex.Message}");
        }
    }

    private void AddSectionHeader(string title, int count)
    {
        var item = new ListViewItem(new[] { "", $"── {title} ({count}) ──", "", "", "" })
        {
            BackColor = Color.FromArgb(30, 60, 80),
            ForeColor = Theme.Accent,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            Tag = -1 // Section header marker
        };
        _teamPlayers.Items.Add(item);
    }

    private void AddPlayerRow(TeamRosterItem player, string role)
    {
        var item = new ListViewItem(new[]
        {
            player.JerseyNumber > 0 ? player.JerseyNumber.ToString() : "",
            player.Name,
            player.Position,
            player.Overall,
            role
        }) { Tag = player.PlayerId, ImageKey = player.PlayerId.ToString() };
        _teamPlayers.Items.Add(item);
        QueueRosterMiniface(player.PlayerId);
    }

    private void QueueRosterMiniface(int playerId)
    {
        if (playerId <= 0) return;
        var key = playerId.ToString();
        if (!_rosterMinifaces.Images.ContainsKey(key))
            _rosterMinifaces.Images.Add(key, MissingRosterMiniface());
        if (!_pendingRosterMinifaces.Add(playerId)) return;

        _ = Task.Run(async () => await LoadPlayerMinifaceAsync(playerId, 32)).ContinueWith(task =>
        {
            _pendingRosterMinifaces.Remove(playerId);
            if (IsDisposed || task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
            var image = task.Result;
            // Replacing an image in place keeps every ListView item's image
            // index stable. RemoveByKey/Add shifts later indices and was the
            // reason faces could appear next to the wrong roster name.
            using (image)
            {
                var circular = CreateCircularMiniface(image, 32);
                var index = _rosterMinifaces.Images.IndexOfKey(key);
                if (index >= 0)
                {
                    var old = _rosterMinifaces.Images[index];
                    _rosterMinifaces.Images[index] = circular;
                    old?.Dispose();
                }
                else
                {
                    _rosterMinifaces.Images.Add(key, circular);
                }
            }
            _teamPlayers.Invalidate();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static Image MissingRosterMiniface()
    {
        var image = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.FromArgb(25, 47, 68));
        using var face = new SolidBrush(Color.FromArgb(150, 190, 205));
        graphics.FillEllipse(face, 11, 6, 10, 10);
        graphics.FillEllipse(face, 7, 16, 18, 18);
        return image;
    }

    private void LoadAudioCatalogs()
    {
        LoadAudioCatalog("CustomizableTeamName", _teamCallnameSlots);
        LoadAudioCatalog("CustomizableAnthemChant", _anthemSlots);
        LoadAudioCatalog("CustomizableChantPackage", _goalSongSlots);
    }

    private void LoadAudioCatalog(string tableName, ListView list)
    {
        list.BeginUpdate();
        try
        {
            list.Items.Clear();
            var table = Services.Session.GetTable(tableName);
            if (table == null) return;
            for (var row = 0; row < table.RowCount; row++)
            {
                list.Items.Add(new ListViewItem(new[]
                {
                    Services.Session.GetCell(tableName, row, "itemId"),
                    Services.Session.GetCell(tableName, row, "audioItemId"),
                    Services.Session.GetCell(tableName, row, "halstring"),
                    Services.Session.GetCell(tableName, row, "isInCM"),
                    Services.Session.GetCell(tableName, row, "isInProClubs")
                }) { Tag = row });
            }
        }
        finally { list.EndUpdate(); }
    }

    private void AddAudioCatalogEntry(string tableName, ListView list, int firstItemId)
    {
        var table = Services.Session.GetTable(tableName);
        if (table == null || table.RowCount == 0) return;
        var maxId = firstItemId - 1;
        for (var row = 0; row < table.RowCount; row++)
            maxId = Math.Max(maxId, Parse(Services.Session.GetCell(tableName, row, "itemId")));
        if (!ShowAudioEntryDialog(tableName, maxId + 1, 0, $"CM26_AUDIO_{maxId + 1}",
                out var itemId, out var audioItemId, out var halString, out var inCm, out var inClubs)) return;
        var duplicate = Services.Session.DuplicateRow(tableName, 0);
        if (!duplicate.Success)
        {
            MessageBox.Show(this, duplicate.Message, "Team Audio",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Services.Pending.MarkStructuralChange();
        var newRow = 1;
        StageAudioEntry(tableName, newRow, itemId, audioItemId, halString, inCm, inClubs);
        Services.Session.RefreshSchema();
        LoadAudioCatalog(tableName, list);
    }

    private void EditAudioCatalogEntry(string tableName, ListView list)
    {
        if (list.SelectedItems.Count == 0 || list.SelectedItems[0].Tag is not int row) return;
        if (!ShowAudioEntryDialog(
                tableName,
                Parse(Services.Session.GetCell(tableName, row, "itemId")),
                Parse(Services.Session.GetCell(tableName, row, "audioItemId")),
                Services.Session.GetCell(tableName, row, "halstring"),
                out var itemId, out var audioItemId, out var halString, out var inCm, out var inClubs,
                Parse(Services.Session.GetCell(tableName, row, "isInCM")) != 0,
                Parse(Services.Session.GetCell(tableName, row, "isInProClubs")) != 0)) return;
        StageAudioEntry(tableName, row, itemId, audioItemId, halString, inCm, inClubs);
        LoadAudioCatalog(tableName, list);
    }

    private void RemoveAudioCatalogEntry(string tableName, ListView list)
    {
        if (list.SelectedItems.Count == 0 || list.SelectedItems[0].Tag is not int row) return;
        if (MessageBox.Show(this, "Remove the selected custom audio catalog entry?",
                "Team Audio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var result = Services.Session.DeleteRow(tableName, row);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, "Team Audio",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Services.Pending.MarkStructuralChange();
        Services.Session.RefreshSchema();
        LoadAudioCatalog(tableName, list);
    }

    private void StageAudioEntry(
        string tableName, int row, int itemId, int audioItemId,
        string halString, bool inCm, bool inClubs)
    {
        Services.Pending.Stage(tableName, row, "itemId", itemId.ToString());
        Services.Pending.Stage(tableName, row, "audioItemId", audioItemId.ToString());
        Services.Pending.Stage(tableName, row, "halstring", halString);
        Services.Pending.Stage(tableName, row, "isInCM", inCm ? "1" : "0");
        Services.Pending.Stage(tableName, row, "isInProClubs", inClubs ? "1" : "0");
    }

    private bool ShowAudioEntryDialog(
        string tableName, int initialItemId, int initialAudioItemId, string initialHalString,
        out int itemId, out int audioItemId, out string halString, out bool inCm, out bool inClubs,
        bool initialInCm = true, bool initialInClubs = true)
    {
        using var dialog = new Form
        {
            Text = tableName, ClientSize = new Size(445, 238),
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false,
            MinimizeBox = false, StartPosition = FormStartPosition.CenterParent, Font = LegacyFont
        };
        var item = new NumericUpDown { Location = new Point(145, 24), Size = new Size(170, 22), Maximum = 999999, Value = Math.Clamp(initialItemId, 0, 999999) };
        var audio = new NumericUpDown { Location = new Point(145, 59), Size = new Size(170, 22), Maximum = 9999999, Value = Math.Clamp(initialAudioItemId, 0, 9999999) };
        var hal = new TextBox { Location = new Point(145, 94), Size = new Size(280, 22), Text = initialHalString };
        var cm = new CheckBox { Location = new Point(145, 130), Size = new Size(115, 24), Text = "Career Mode", Checked = initialInCm };
        var clubs = new CheckBox { Location = new Point(270, 130), Size = new Size(115, 24), Text = "Pro Clubs", Checked = initialInClubs };
        dialog.Controls.AddRange([
            new Label { Text = "Item Id", Location = new Point(18, 27), Size = new Size(115, 20) },
            new Label { Text = "Audio Item Id", Location = new Point(18, 62), Size = new Size(115, 20) },
            new Label { Text = "HAL String", Location = new Point(18, 97), Size = new Size(115, 20) },
            item, audio, hal, cm, clubs
        ]);
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(238, 184), Size = new Size(88, 28) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(337, 184), Size = new Size(88, 28) };
        dialog.Controls.AddRange([ok, cancel]);
        Theme.ApplyControlTree(dialog);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;
        var accepted = dialog.ShowDialog(this) == DialogResult.OK;
        itemId = (int)item.Value;
        audioItemId = (int)audio.Value;
        halString = hal.Text.Trim();
        inCm = cm.Checked;
        inClubs = clubs.Checked;
        return accepted && halString.Length > 0;
    }

    private void ShowTeamFlags(int teamId, int nationId)
    {
        if (teamId <= 0)
        {
            var old = _teamFlagPreview.Image;
            _teamFlagPreview.Image = null;
            old?.Dispose();
            _teamFlagCaption.Text = "No team flag is linked";
        }
        else
        {
            var candidates = new[]
            {
                $"data/ui/imgAssets/teamFlags/t{teamId}.dds",
                $"data/ui/imgAssets/teamflags/t{teamId}.dds",
                $"data/ui/imgAssets/flags/team/t{teamId}.dds"
            };
            var staged = candidates.FirstOrDefault(path => Services.LegacyMods.GetReplacement(path) != null);
            var target = staged ?? candidates[0];
            LegacyAssetActions.SetTarget(_teamFlagPreview, new LegacyAssetEditTarget(target, 512, 256));
            FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(
                _teamFlagPreview,
                Services,
                staged == null ? null : Services.LegacyMods.GetReplacement(staged),
                candidates,
                (image, source) =>
                {
                    if (IsDisposed) { image?.Dispose(); return; }
                    var old = _teamFlagPreview.Image;
                    _teamFlagPreview.Image = image;
                    old?.Dispose();
                    _teamFlagCaption.Text = image == null
                        ? $"No dedicated team flag ({teamId})"
                        : $"Team flag · {source}";
                },
                path => LegacyAssetActions.SetTarget(
                    _teamFlagPreview, new LegacyAssetEditTarget(path, 512, 256)));
        }

        var nationPath = $"data/ui/imgAssets/flags512x512/light/f_{nationId}.dds";
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _nationFlagPreview,
            Services,
            null,
            nationPath,
            (image, source) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                var old = _nationFlagPreview.Image;
                _nationFlagPreview.Image = image;
                old?.Dispose();
                _nationFlagCaption.Text = image == null
                    ? $"No nation flag ({nationId})"
                    : $"Nation flag · {source}";
            });
    }

    private void LoadSponsors(int teamId)
    {
        _teamSponsors.Items.Clear();
        _adboardSources.Items.Clear();
        var oldAdboard = _adboardPreview.Image;
        _adboardPreview.Image = null;
        oldAdboard?.Dispose();
        _adboardCaption.Text = "Select a linked sponsor or adboard source";
        var links = Services.Session.GetTable("teamsponsorlinks");
        var sponsors = Services.Session.GetTable("sponsors");
        if (links == null || sponsors == null || teamId <= 0)
        {
            _adboardSources.Items.Add(new ListViewItem(new[] { "No team sponsor links are available", "", "", "" }));
            return;
        }
        var sponsorNames = new Dictionary<int, string>();
        var sponsorIdColumn = Col(sponsors, "adsponserid");
        var sponsorNameColumn = Col(sponsors, "name");
        if (sponsorIdColumn >= 0 && sponsorNameColumn >= 0)
        {
            for (var row = 0; row < sponsors.RowCount; row++)
            {
                var sponsor = Services.Session.GetRecord("sponsors", row);
                if (sponsor != null) sponsorNames[Parse(sponsor.Get(sponsorIdColumn))] = sponsor.Get(sponsorNameColumn);
            }
        }
        var teamColumn = Col(links, "teamid");
        var linkSponsorColumn = Col(links, "adsponserid");
        var approvedColumn = Col(links, "isapproved");
        var imageColumn = Col(links, "dynamicimageid");
        var keyColumn = Col(links, "artificialkey");
        for (var row = 0; row < links.RowCount; row++)
        {
            var link = Services.Session.GetRecord("teamsponsorlinks", row);
            if (link == null || teamColumn < 0 || Parse(link.Get(teamColumn)) != teamId) continue;
            var sponsorId = linkSponsorColumn >= 0 ? Parse(link.Get(linkSponsorColumn)) : 0;
            var dynamicImageId = imageColumn >= 0 ? Parse(link.Get(imageColumn)) : 0;
            var approved = approvedColumn >= 0 ? link.Get(approvedColumn) : string.Empty;
            var name = sponsorNames.TryGetValue(sponsorId, out var resolved) ? resolved : $"Sponsor {sponsorId}";
            var asset = new TeamSponsorAsset(sponsorId, dynamicImageId, name);
            _teamSponsors.Items.Add(new ListViewItem(new[] { name, approved, dynamicImageId.ToString(), keyColumn >= 0 ? link.Get(keyColumn) : string.Empty }) { Tag = asset });
            _adboardSources.Items.Add(new ListViewItem(new[] { name, sponsorId.ToString(), dynamicImageId.ToString(), approved }) { Tag = asset });
        }
        if (_teamSponsors.Items.Count == 0)
        {
            _teamSponsors.Items.Add(new ListViewItem(new[] { "No sponsor is linked to this team", "", "", "" }));
            _adboardSources.Items.Add(new ListViewItem(new[] { "No adboard or sponsor source is linked", "", "", "" }));
        }
        else
        {
            _teamSponsors.Items[0].Selected = true;
            _adboardSources.Items[0].Selected = true;
        }
    }

    private void ShowSelectedSponsor()
    {
        if (_teamSponsors.SelectedItems.Count == 0 || _teamSponsors.SelectedItems[0].Tag is not TeamSponsorAsset asset) return;
        var legacyPath = $"data/ui/imgAssets/cmSponsors/cmSponsors{asset.SponsorId}.dds";
        LegacyAssetActions.SetTarget(_sponsorPreview, new LegacyAssetEditTarget(legacyPath, 512, 256));
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _sponsorPreview,
            Services,
            LegacyAssetActions.Replacement(Services, legacyPath),
            legacyPath,
            (image, source) =>
        {
            if (IsDisposed) { image?.Dispose(); return; }
            var old = _sponsorPreview.Image;
            _sponsorPreview.Image = image;
            old?.Dispose();
            _sponsorPreviewCaption.Text = image == null
                ? $"{asset.Name}: sponsor artwork is unavailable"
                : $"{asset.Name} · {source}";
        });
    }

    private void ShowSelectedAdboard()
    {
        if (_adboardSources.SelectedItems.Count == 0 || _adboardSources.SelectedItems[0].Tag is not TeamSponsorAsset asset) return;
        var candidates = new[]
        {
            $"data/ui/imgAssets/cmSponsors/cmSponsors{asset.SponsorId}.dds",
            $"data/ui/imgAssets/cmSponsors/cmSponsors{asset.DynamicImageId}.dds",
            $"data/ui/imgAssets/dynamicSponsors/dynamic_{asset.DynamicImageId}.dds"
        };
        var staged = candidates.FirstOrDefault(path => Services.LegacyMods.GetReplacement(path) != null);
        var target = staged ?? candidates[0];
        LegacyAssetActions.SetTarget(_adboardPreview, new LegacyAssetEditTarget(target, 1024, 256));
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(
            _adboardPreview,
            Services,
            staged == null ? null : Services.LegacyMods.GetReplacement(staged),
            candidates,
            (image, source) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                var old = _adboardPreview.Image;
                _adboardPreview.Image = image;
                old?.Dispose();
                _adboardCaption.Text = image == null
                    ? $"{asset.Name}: link found, but matching artwork is unavailable"
                    : $"{asset.Name} · sponsor {asset.SponsorId} · dynamic image {asset.DynamicImageId} · {source}";
            },
            path => LegacyAssetActions.SetTarget(
                _adboardPreview, new LegacyAssetEditTarget(path, 1024, 256)));
    }

    private void RefreshTeamFlags()
    {
        if (CurrentRecordIndex < 0) return;
        var table = Services.Session.GetTable(TableName);
        var record = Services.Session.GetRecord(TableName, CurrentRecordIndex);
        if (table == null || record == null) return;
        ShowTeamFlags(Parse(record.Get(Col(table, "teamid"))), Parse(record.Get(Col(table, "countryid"))));
    }

    private sealed record TeamSponsorAsset(int SponsorId, int DynamicImageId, string Name);

    private void UpdateTeamColours()
    {
        for (var index = 0; index < _teamColorChips.Count; index++)
        {
            var prefix = $"teamcolor{index + 1}";
            var red = TeamColourComponent(prefix + "r");
            var green = TeamColourComponent(prefix + "g");
            var blue = TeamColourComponent(prefix + "b");
            _teamColorChips[index].BackColor = Color.FromArgb(red, green, blue);
            ToolTip.SetToolTip(_teamColorChips[index], $"{prefix}: {red}, {green}, {blue}");
        }
    }

    private int TeamColourComponent(string field) =>
        _fields.TryGetValue(field, out var value) && int.TryParse(value.RawValue, out var component) ? Math.Clamp(component, 0, 255) : 0;

    /// <summary>CM16 behaviour: double-clicking a roster player opens Player editor.</summary>
    private void OpenSelectedRosterPlayer()
    {
        if (_teamPlayers.SelectedItems.Count == 0 || _teamPlayers.SelectedItems[0].Tag is not int playerId || playerId <= 0)
            return;
        var players = Services.Session.GetTable("players");
        var playerIdColumn = players == null ? -1 : Col(players, "playerid");
        if (players == null || playerIdColumn < 0) return;
        var row = Enumerable.Range(0, players.RowCount).FirstOrDefault(index =>
        {
            var record = Services.Session.GetRecord("players", index);
            return record != null && Parse(record.Get(playerIdColumn)) == playerId;
        }, -1);
        if (row >= 0) Services.RequestRecordNavigation("players", row);
    }

    private void OpenTransferDialog()
    {
        if (_teamPlayers.SelectedItems.Count == 0 || _teamPlayers.SelectedItems[0].Tag is not int playerId ||
            playerId <= 0 || !_rosterByPlayerId.TryGetValue(playerId, out var player))
        {
            MessageBox.Show(this, "Select a Team Player first.", "Transfer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var links = Services.Session.GetTable("teamplayerlinks");
        var teams = Services.Session.GetTable("teams");
        if (links == null || teams == null)
        {
            MessageBox.Show(this, "Roster relationship data is unavailable.", "Transfer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var playerColumn = Col(links, "playerid");
        var teamColumn = Col(links, "teamid");
        var jerseyColumn = Col(links, "jerseynumber");
        var positionColumn = Col(links, "position");
        var linkRow = Enumerable.Range(0, links.RowCount).FirstOrDefault(row =>
        {
            var link = Services.Session.GetRecord("teamplayerlinks", row);
            return link != null && Parse(link.Get(playerColumn)) == playerId;
        }, -1);
        if (linkRow < 0 || teamColumn < 0 || jerseyColumn < 0 || positionColumn < 0 ||
            !links.Columns[teamColumn].IsWritable || !links.Columns[jerseyColumn].IsWritable || !links.Columns[positionColumn].IsWritable)
        {
            MessageBox.Show(this, "This player relationship cannot be safely edited.", "Transfer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var choices = new List<TeamChoice>();
        var teamIdColumn = Col(teams, "teamid");
        var teamNameColumn = Col(teams, "teamname");
        for (var row = 0; row < teams.RowCount; row++)
        {
            var team = Services.Session.GetRecord("teams", row);
            if (team == null) continue;
            var id = Parse(team.Get(teamIdColumn));
            if (id > 0) choices.Add(new TeamChoice(id, team.Get(teamNameColumn)));
        }
        if (choices.Count == 0) return;

        var current = Services.Session.GetRecord("teamplayerlinks", linkRow)!;
        using var dialog = new Form
        {
            Text = $"Transfer {player.Name}", StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
            ClientSize = new Size(420, 205), Font = LegacyFont
        };
        dialog.Controls.Add(new Label { Text = $"{player.Name}  ({player.Position}, OVR {player.Overall})", Location = new Point(15, 16), Size = new Size(390, 22), Font = Theme.BodyBold });
        dialog.Controls.Add(new Label { Text = "Destination team", Location = new Point(15, 54), AutoSize = true });
        var destination = new ComboBox { Location = new Point(130, 50), Size = new Size(270, 22), DropDownStyle = ComboBoxStyle.DropDownList };
        destination.Items.AddRange(choices.OrderBy(choice => choice.Name, StringComparer.CurrentCultureIgnoreCase).Cast<object>().ToArray());
        var currentTeamId = Parse(current.Get(teamColumn));
        destination.SelectedItem = destination.Items.Cast<TeamChoice>().FirstOrDefault(choice => choice.Id == currentTeamId);
        dialog.Controls.Add(destination);
        dialog.Controls.Add(new Label { Text = "Shirt number", Location = new Point(15, 90), AutoSize = true });
        var shirt = new NumericUpDown { Location = new Point(130, 86), Size = new Size(90, 22), Minimum = 0, Maximum = 99, Value = Math.Clamp(Parse(current.Get(jerseyColumn)), 0, 99) };
        dialog.Controls.Add(shirt);
        dialog.Controls.Add(new Label { Text = "Position", Location = new Point(240, 90), AutoSize = true });
        var position = new ComboBox { Location = new Point(290, 86), Size = new Size(110, 22), DropDownStyle = ComboBoxStyle.DropDownList };
        for (var value = 0; value <= 27; value++) position.Items.Add(new PositionChoice(value));
        position.SelectedItem = position.Items.Cast<PositionChoice>().FirstOrDefault(choice => choice.Id == Parse(current.Get(positionColumn)));
        dialog.Controls.Add(position);
        var stage = new Button { Text = "Stage Transfer", DialogResult = DialogResult.OK, Location = new Point(210, 150), Size = new Size(100, 28) };
        dialog.Controls.Add(stage);
        dialog.Controls.Add(new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(320, 150), Size = new Size(80, 28) });
        Theme.ApplyControlTree(dialog);
        dialog.AcceptButton = stage;
        if (dialog.ShowDialog(this) != DialogResult.OK || destination.SelectedItem is not TeamChoice target || position.SelectedItem is not PositionChoice role) return;

        var ok = StageField("teamplayerlinks", linkRow, "teamid", target.Id.ToString(), _stagingGrid);
        ok &= StageField("teamplayerlinks", linkRow, "jerseynumber", ((int)shirt.Value).ToString(), _stagingGrid);
        ok &= StageField("teamplayerlinks", linkRow, "position", role.Id.ToString(), _stagingGrid);
        if (!ok) return;
        MessageBox.Show(this, $"{player.Name} will move to {target.Name}. Save to apply the transfer.", "Transfer", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private sealed record TeamChoice(int Id, string Name)
    {
        public override string ToString() => $"{Name} [{Id}]";
    }

    private sealed record PositionChoice(int Id)
    {
        public override string ToString() => NameResolverService.PositionLabel(Id);
    }

    private void ShowLoanDetails()
    {
        if (_teamPlayers.SelectedItems.Count == 0 || _teamPlayers.SelectedItems[0].Tag is not int playerId ||
            !_rosterByPlayerId.TryGetValue(playerId, out var player))
        {
            MessageBox.Show(this, "Select a Team Player first.", "Loan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var loans = Services.Session.GetTable("playerloans");
        var teams = Services.Session.GetTable("teams");
        if (loans == null || teams == null || loans.RowCount == 0)
        {
            MessageBox.Show(this, "Loan or team data is unavailable.", "Loan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var playerColumn = Col(loans, "playerid");
        var sourceColumn = Col(loans, "teamidloanedfrom");
        var endColumn = Col(loans, "loandateend");
        var buyColumn = Col(loans, "isloantobuy");
        if (playerColumn < 0 || sourceColumn < 0 || endColumn < 0 || buyColumn < 0)
        {
            MessageBox.Show(this, "The loan data schema is incomplete.", "Loan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var loanRow = Enumerable.Range(0, loans.RowCount).FirstOrDefault(row =>
        {
            var record = Services.Session.GetRecord("playerloans", row);
            return record != null && Parse(record.Get(playerColumn)) == playerId;
        }, -1);
        var current = loanRow >= 0 ? Services.Session.GetRecord("playerloans", loanRow) : null;

        var choices = new List<TeamChoice>();
        var teamIdColumn = Col(teams, "teamid");
        var teamNameColumn = Col(teams, "teamname");
        for (var row = 0; row < teams.RowCount; row++)
        {
            var team = Services.Session.GetRecord("teams", row);
            if (team == null) continue;
            var id = Parse(team.Get(teamIdColumn));
            if (id > 0) choices.Add(new TeamChoice(id, team.Get(teamNameColumn)));
        }

        using var dialog = new Form
        {
            Text = $"Player Loan · {player.Name}", StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
            ClientSize = new Size(455, 240), Font = LegacyFont
        };
        dialog.Controls.Add(new Label
        {
            Text = $"{player.Name}  ({player.Position}, OVR {player.Overall})",
            Location = new Point(15, 16), Size = new Size(420, 22), Font = Theme.BodyBold
        });
        dialog.Controls.Add(new Label { Text = "Loaned from", Location = new Point(15, 57), AutoSize = true });
        var source = new ComboBox
        {
            Location = new Point(125, 53), Size = new Size(310, 22),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        source.Items.AddRange(choices.OrderBy(choice => choice.Name, StringComparer.CurrentCultureIgnoreCase).Cast<object>().ToArray());
        var currentSource = current == null ? 0 : Parse(current.Get(sourceColumn));
        source.SelectedItem = source.Items.Cast<TeamChoice>().FirstOrDefault(choice => choice.Id == currentSource);
        if (source.SelectedIndex < 0 && source.Items.Count > 0) source.SelectedIndex = 0;
        dialog.Controls.Add(source);
        dialog.Controls.Add(new Label { Text = "Loan end date", Location = new Point(15, 94), AutoSize = true });
        var endDate = new NumericUpDown
        {
            Location = new Point(125, 90), Size = new Size(130, 22), Minimum = 0, Maximum = 1048575,
            Value = Math.Clamp(current == null ? 0 : Parse(current.Get(endColumn)), 0, 1048575)
        };
        dialog.Controls.Add(endDate);
        var loanToBuy = new CheckBox
        {
            Text = "Loan to buy", Location = new Point(280, 91), Size = new Size(130, 22),
            Checked = current != null && Parse(current.Get(buyColumn)) != 0
        };
        dialog.Controls.Add(loanToBuy);
        dialog.Controls.Add(new Label
        {
            Text = loanRow >= 0 ? "Existing loan record" : "No current loan record — a new relationship will be created",
            Location = new Point(15, 130), Size = new Size(420, 22), ForeColor = Theme.Muted
        });
        var stage = new Button { Text = loanRow >= 0 ? "Stage Changes" : "Create Loan", DialogResult = DialogResult.OK, Location = new Point(185, 185), Size = new Size(105, 28) };
        var remove = new Button { Text = "Remove Loan", DialogResult = DialogResult.Yes, Location = new Point(70, 185), Size = new Size(105, 28), Enabled = loanRow >= 0 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(300, 185), Size = new Size(95, 28) };
        dialog.Controls.Add(remove);
        dialog.Controls.Add(stage);
        dialog.Controls.Add(cancel);
        Theme.ApplyControlTree(dialog);
        dialog.AcceptButton = stage;
        dialog.CancelButton = cancel;

        var result = dialog.ShowDialog(this);
        if (result == DialogResult.Yes && loanRow >= 0)
        {
            if (MessageBox.Show(this, $"Remove the loan for {player.Name}?", "Remove Loan",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var removed = Services.Session.DeleteRow("playerloans", loanRow);
            if (!removed.Success)
            {
                MessageBox.Show(this, removed.Message, "Remove Loan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Services.Pending.MarkStructuralChange();
            Services.Session.RefreshSchema();
            MessageBox.Show(this, "Loan link removed. Save to persist the change.", "Loan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (result != DialogResult.OK || source.SelectedItem is not TeamChoice sourceTeam) return;

        if (loanRow < 0)
        {
            var duplicate = Services.Session.DuplicateRow("playerloans", 0);
            if (!duplicate.Success)
            {
                MessageBox.Show(this, duplicate.Message, "Create Loan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Services.Pending.MarkStructuralChange();
            loanRow = 1;
            if (!StageField("playerloans", loanRow, "playerid", playerId.ToString(), _stagingGrid)) return;
        }

        var ok = StageField("playerloans", loanRow, "teamidloanedfrom", sourceTeam.Id.ToString(), _stagingGrid);
        ok &= StageField("playerloans", loanRow, "loandateend", ((int)endDate.Value).ToString(), _stagingGrid);
        ok &= StageField("playerloans", loanRow, "isloantobuy", loanToBuy.Checked ? "1" : "0", _stagingGrid);
        if (ok)
            MessageBox.Show(this, $"Loan prepared for {player.Name}. Save to apply it.", "Loan", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SetMiniface(PictureBox viewer, string? path, int playerId)
    {
        if (playerId <= 0)
        {
            var old = viewer.Image;
            viewer.Image = null;
            old?.Dispose();
            return;
        }
        FrostbitePreviewLoader.LoadLegacyUiAsset(viewer, Services, path,
            $"data/ui/imgAssets/heads/p{playerId}.dds", (image, _) =>
        {
            var old = viewer.Image;
            viewer.Image = image;
            old?.Dispose();
        });
    }

    private static bool IsPlayerReferenceField(string field) =>
        field.Equals("captainid", StringComparison.OrdinalIgnoreCase) ||
        field.Equals("penaltytakerid", StringComparison.OrdinalIgnoreCase) ||
        field.Equals("freekicktakerid", StringComparison.OrdinalIgnoreCase) ||
        field.Equals("leftcornerkicktakerid", StringComparison.OrdinalIgnoreCase) ||
        field.Equals("rightcornerkicktakerid", StringComparison.OrdinalIgnoreCase);

    private static bool IsLinkedDisplayField(string field) =>
        field.Equals("countryid", StringComparison.OrdinalIgnoreCase) ||
        field.Equals("leagueid", StringComparison.OrdinalIgnoreCase) ||
        field.Equals("stadiumid", StringComparison.OrdinalIgnoreCase) ||
        field.Equals("managerid", StringComparison.OrdinalIgnoreCase);

    private string ResolveLinkedValue(string field, int teamId) => field switch
    {
        "countryid" => Services.Resolver?.TeamNationName(teamId) ?? string.Empty,
        "leagueid" => Services.Resolver?.TeamLeagueName(teamId) ?? string.Empty,
        "stadiumid" => Services.Resolver?.TeamStadiumName(teamId) ?? string.Empty,
        "managerid" => Services.Resolver?.TeamManagerName(teamId) ?? string.Empty,
        _ => string.Empty,
    };

    private void ShowCrest(string path, string teamName, int teamId)
    {
        var viewer = _crestViewers[0];
        var candidates = teamId > 0
            ? new[]
            {
                $"data/ui/imgAssets/crest/dark/l{teamId}.dds",
                $"data/ui/imgAssets/crest/light/l{teamId}.dds"
            }
            : Array.Empty<string>();

        // Do not retain the previous club's target while the actual FC26 asset
        // variant is being resolved. A missing dark asset must be allowed to
        // fall back to light, not become a save target for the wrong club.
        LegacyAssetActions.ClearTarget(viewer);
        viewer.BackColor = DarkCrestBackground;
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(viewer, Services, null, candidates,
            (image, source) =>
        {
            try
            {
                if (!viewer.IsDisposed)
                {
                    var old = viewer.Image;
                    viewer.Image = image;
                    old?.Dispose();
                }
                _crestCaption.Text = image == null
                    ? $"{teamName}\r\nNo crest available"
                    : $"{teamName}\r\n{source}";
        }
        catch (System.AccessViolationException) { }
        catch { }
    }, resolvedPath => LegacyAssetActions.SetTarget(
        viewer, new LegacyAssetEditTarget(resolvedPath, 256, 256)));
    }
}
