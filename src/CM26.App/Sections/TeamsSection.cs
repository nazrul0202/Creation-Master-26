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
    private readonly List<TextBox> _editors = [];
    private readonly Dictionary<string, FieldValue> _fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _stagingGrid = new();
    private readonly Label _crestCaption = new();
    private readonly List<PictureBox> _crestViewers = [];
    private readonly List<Panel> _teamColorChips = [];
    private readonly ListView _teamPlayers = new();
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
    private PictureBox? _selectedPlayerFace;
    private Label? _selectedPlayerName;
    private Label? _selectedPlayerDetails;
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

    public override string SectionKey => "teams";
    public override string SectionTitle => "Teams";
    protected override string TableName => "teams";
    // Club creation is intentionally owned by Leagues, where it can be linked
    // to a competition in the same operation. This page is for editing teams.
    protected override bool SupportsCreate => false;
    protected override string RecordSearchPlaceholder => "Search teams…";

    public TeamsSection(AppServices services) : base(services)
    {
        Tabs.Font = LegacyFont;
        Tabs.Padding = new Point(4, 2);
        AddGenericTab();
        AddRosterTab();
        AddSponsorsTab();
        AddAdboardsTab();
        AddFlagsTab();
        AddAudioTab();
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Services.RequireData().GetTeams();

    protected override void CreateNewRecord()
    {
        if (!EntityCreationDialog.TryShow(this, "Team", [("Team name", "New Team")], out var values))
            return;
        try
        {
            var id = CreateRecordFromTemplate(TableName, "teamid", new Dictionary<string, string>
            {
                ["teamname"] = values[0],
                ["assetid"] = "0",
                ["presassetone"] = "0",
                ["presassettwo"] = "0",
                ["captainid"] = "-1",
                ["penaltytakerid"] = "-1",
                ["freekicktakerid"] = "-1",
                ["leftcornerkicktakerid"] = "-1",
                ["rightcornerkicktakerid"] = "-1",
            });
            MessageBox.Show(this, $"Team created with ID {id}. Assign its league, players, kits and artwork before Save.",
                "Create Team", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create Team", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
        var page = new TabPage(text) { BackColor = SystemColors.Control, Font = LegacyFont };
        page.Controls.Add(new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = SystemColors.Control });
        Tabs.TabPages.Add(page);
        return page;
    }

    private static Panel Canvas(TabPage page) => (Panel)page.Controls[0];

    private static GroupBox Group(string text, Point location, Size size) => new()
    {
        Text = text, Location = location, Size = size, Font = LegacyFont,
        BackColor = SystemColors.Control, ForeColor = SystemColors.ControlText
    };

    private static PictureBox Viewer(Point location, Size size) => new()
    {
        Location = location, Size = size, BackColor = Color.White,
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

    private void AddGenericTab()
    {
        var page = Page("Generic");
        var canvas = Canvas(page);

        var logos = Group("Logos", new Point(3, 3), new Size(270, 445));
        logos.Controls.Add(CrestViewer(new Point(6, 19), new Size(256, 256)));
        LegacyAssetActions.Attach(Services, logos, _crestViewers[0], new Point(6, 279), () => ShowRecord(CurrentRecordIndex));
        logos.Controls.Add(CrestViewer(new Point(7, 306), new Size(64, 62)));
        logos.Controls.Add(CrestViewer(new Point(102, 306), new Size(64, 62)));
        logos.Controls.Add(CrestViewer(new Point(194, 306), new Size(64, 62)));
        LegacyAssetActions.Attach(Services, logos, _crestViewers[1], new Point(7, 372), () => ShowRecord(CurrentRecordIndex));
        _crestCaption.Location = new Point(8, 270);
        _crestCaption.Size = new Size(252, 32);
        _crestCaption.TextAlign = ContentAlignment.MiddleCenter;
        _crestCaption.ForeColor = SystemColors.GrayText;
        _crestCaption.Font = LegacyFont;
        logos.Controls.Add(_crestCaption);
        canvas.Controls.Add(logos);

        // Preserve the original CM16 TeamForm geometry. FC26's canonical name
        // is mirrored into the legacy display-name slots by the adapter.
        var name = Group("Name", new Point(3, 454), new Size(270, 160));
        AddBoundFields(name, new[]
        {
            ("Database Name", "teamname"), ("Full Name", "teamname"), ("Name (15 chars)", "teamname"),
            ("Name (10 chars)", "teamname"), ("Name (7 chars)", "teamname"), ("Score Board", "teamname")
        }, 10, 18, 88, 158, 23);
        canvas.Controls.Add(name);

        var stadium = Group("Stadium", new Point(3, 620), new Size(270, 67));
        AddBoundFields(stadium, new[] { ("Stadium Model", "stadiumid"), ("Stadium Name", "stadiumid") }, 10, 18, 98, 158, 24);
        canvas.Controls.Add(stadium);

        var manager = Group("Manager", new Point(3, 693), new Size(270, 67));
        AddBoundFields(manager, new[] { ("First Name", "managerid"), ("Surname", "managerid") }, 10, 18, 98, 158, 24);
        canvas.Controls.Add(manager);

        var info = Group("Info", new Point(279, 3), new Size(270, 496));
        for (var i = 0; i < 3; i++)
        {
            var chip = new Panel { Location = new Point(98 + (i * 37), 13), Size = new Size(23, 23), BackColor = SystemColors.Control, BorderStyle = BorderStyle.FixedSingle };
            _teamColorChips.Add(chip);
            info.Controls.Add(chip);
        }
        AddBoundFields(info, new[]
        {
            ("Team Id", "teamid"), ("Country", "countryid"), ("League", "leagueid"),
            ("Domestic", "domesticprestige"), ("International", "internationalprestige"), ("Budget", "clubworth"),
            ("Overall Rating", "overallrating"), ("Attack Rating", "attackrating"), ("Midfield Rating", "midfieldrating"),
            ("Defence Rating", "defenserating"), ("Ball Number", "ballid")
        }, 10, 40, 98, 158, 26);
        var search = new TextBox { Location = new Point(105, 340), Size = new Size(84, 21), Font = LegacyFont };
        var find = LegacyButton("Find", new Point(195, 338), new Size(58, 24));
        find.Click += (_, _) => FindTeam(search.Text);
        search.KeyDown += (_, eventArgs) => { if (eventArgs.KeyCode == Keys.Enter) { FindTeam(search.Text); eventArgs.SuppressKeyPress = true; } };
        info.Controls.Add(new Label { Text = "Find team record", Location = new Point(12, 342), Size = new Size(88, 20), Font = LegacyFont, TextAlign = ContentAlignment.MiddleLeft });
        info.Controls.Add(search);
        info.Controls.Add(find);
        var importSquad = LegacyButton("Import Scraper Squad", new Point(12, 409), new Size(241, 27));
        importSquad.Click += (_, _) => ImportScraperSquad();
        info.Controls.Add(importSquad);
        canvas.Controls.Add(info);

        var lastYear = Group("Last Year Performance", new Point(279, 505), new Size(270, 101));
        AddBoundFields(lastYear, new[] { ("League", "leagueid"), ("Position", "form"), ("Is Champion", "prev_el_champ") }, 10, 18, 98, 158, 24);
        canvas.Controls.Add(lastYear);

        var location = Group("Location", new Point(279, 612), new Size(270, 99));
        AddBoundFields(location, new[] { ("Latitude", "latitude"), ("Longitude", "longitude"), ("UTC Time", "utctime") }, 10, 18, 98, 86, 24);
        canvas.Controls.Add(location);

        var traits = Group("Opponent Behaviour", new Point(555, 3), new Size(276, 126));
        AddBoundFields(traits, new[] { ("Vs. weaker teams", "trait1vweak"), ("Vs. stronger teams", "trait1vstrong"), ("Vs. equal teams", "trait1vequal") }, 10, 20, 118, 145, 27);
        ToolTip.SetToolTip(traits,
            "Internal behaviour bitmasks used to vary team tendencies against weaker, stronger, or evenly matched opponents.");
        canvas.Controls.Add(traits);

        var kitLinks = Group("Kit Links", new Point(555, 211), new Size(276, 66));
        foreach (var link in new[] { "Home Kit", "Away Kit", "Keeper Kit", "3rd Kit" })
            kitLinks.Controls.Add(new LinkLabel { Text = link, AutoSize = true, Location = new Point(12 + (kitLinks.Controls.Count * 68), 27), Font = LegacyFont });
        canvas.Controls.Add(kitLinks);

        // CM16 keeps the day-to-day club record on one General/Generic surface.
        // These are FC26 fields; only their placement changes here.
        var club = Group("Club Details", new Point(555, 286), new Size(360, 210));
        AddBoundFields(club, new[]
        {
            ("Founded", "foundationyear"), ("City", "cityid"), ("Gender", "gender"),
            ("Stadium Capacity", "teamstadiumcapacity"), ("Training Stadium", "trainingstadium"),
            ("Youth Development", "youthdevelopment"), ("Popularity", "popularity"), ("Profitability", "profitability")
        }, 10, 20, 160, 180, 22);
        canvas.Controls.Add(club);

        var history = Group("Club History", new Point(555, 502), new Size(360, 115));
        AddBoundFields(history, new[]
        {
            ("League Titles", "leaguetitles"), ("Domestic Cups", "domesticcups"),
            ("UEFA Champions League", "uefa_cl_wins"), ("UEFA Europa League", "uefa_el_wins")
        }, 10, 20, 145, 180, 21);
        canvas.Controls.Add(history);

        var ratings = Group("Matchday Ratings", new Point(921, 3), new Size(290, 155));
        AddBoundFields(ratings, new[]
        {
            ("Overall", "matchdayoverallrating"), ("Attack", "matchdayattackrating"),
            ("Midfield", "matchdaymidfieldrating"), ("Defence", "matchdaydefenserating"),
            ("Current Form", "form")
        }, 15, 22, 116, 128, 24);
        canvas.Controls.Add(ratings);

        var tactics = Group("Team Tendency", new Point(921, 164), new Size(290, 105));
        AddBoundFields(tactics, new[]
        {
            ("Build Up Play", "buildupplay"), ("Defensive Depth", "defensivedepth"),
            ("Opponent Weak Threshold", "opponentweakthreshold"), ("Opponent Strong Threshold", "opponentstrongthreshold")
        }, 14, 22, 154, 116, 20);
        canvas.Controls.Add(tactics);

        var presentation = Group("Matchday Presentation", new Point(921, 275), new Size(390, 267));
        AddBoundFields(presentation, new[]
        {
            ("Standing Crowd", "hasstandingcrowd"), ("Tifo", "hastifo"), ("Large Flag", "haslargeflag"),
            ("Skinny Flags", "skinnyflags"), ("Sun Anthem", "hassuncanthem"), ("Viking Clap", "hasvikingclap"),
            ("Substitution Board", "hassubstitutionboard"), ("Competition Banner", "isbannerenabled"),
            ("Competition Crowd Cards", "iscompetitioncrowdcardsenabled"), ("Competition Pole Flags", "iscompetitionpoleflagenabled"),
            ("Competition Scarves", "iscompetitionscarfenabled")
        }, 15, 22, 190, 150, 22);
        canvas.Controls.Add(presentation);

        var pitchDetails = Group("Team Pitch and Goal Net", new Point(1217, 3), new Size(310, 220));
        AddBoundFields(pitchDetails, new[]
        {
            ("Pitch Surface", "playsurfacetype"), ("Pitch Colour", "pitchcolor"), ("Pitch Wear", "pitchwear"),
            ("Pitch Line Colour", "pitchlinecolor"), ("Mowing Pattern", "stadiummowpattern_code"),
            ("Goal Net Style", "stadiumgoalnetstyle"), ("Goal Net Pattern", "stadiumgoalnetpattern")
        }, 15, 22, 132, 148, 24);
        canvas.Controls.Add(pitchDetails);
    }

    private void AddAudioTab()
    {
        var page = Page("Audio");
        var canvas = Canvas(page);
        var presentation = Group("Selected Team Match Audio", new Point(3, 3), new Size(475, 245));
        AddBoundFields(presentation, new[]
        {
            ("Sun Anthem Enabled", "hassuncanthem"),
            ("Crowd Region", "crowdregion"),
            ("Viking Clap", "hasvikingclap"),
            ("Team Personality", "personalityid"),
            ("Tifo Enabled", "hastifo")
        }, 15, 25, 175, 135, 34);
        presentation.Controls.Add(new Label
        {
            Text = "Match presentation settings for the selected team.",
            Location = new Point(15, 202), Size = new Size(430, 24),
            Font = LegacyFont, ForeColor = SystemColors.GrayText
        });
        canvas.Controls.Add(presentation);

        canvas.Controls.Add(CreateAudioCatalog(
            "Custom Team Callname Catalog", "CustomizableTeamName",
            _teamCallnameSlots, new Point(3, 255), 1));
        canvas.Controls.Add(CreateAudioCatalog(
            "Custom Anthem Catalog", "CustomizableAnthemChant",
            _anthemSlots, new Point(490, 3), 1001));
        canvas.Controls.Add(CreateAudioCatalog(
            "Custom Chant / Goal Song Catalog", "CustomizableChantPackage",
            _goalSongSlots, new Point(490, 345), 1501));
    }

    private GroupBox CreateAudioCatalog(
        string title, string tableName, ListView list, Point location, int firstItemId)
    {
        var width = location.X < 100 ? 475 : 610;
        var box = Group(title, location, new Size(width, 330));
        list.Location = new Point(10, 22);
        list.Size = new Size(width - 20, 245);
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

        var players = Group("Team Players", new Point(3, 3), new Size(383, 798));
        _selectedPlayerFace = Viewer(new Point(10, 20), new Size(128, 128));
        players.Controls.Add(_selectedPlayerFace);
        _selectedPlayerName = new Label { Location = new Point(10, 151), Size = new Size(295, 18), Font = LegacyFont, Text = "Select a player" };
        _selectedPlayerDetails = new Label { Location = new Point(148, 22), Size = new Size(160, 150), Font = LegacyFont, ForeColor = SystemColors.ControlText, Text = "Select a roster player\nto view details." };
        players.Controls.Add(_selectedPlayerName);
        players.Controls.Add(_selectedPlayerDetails);
        var transfer = LegacyButton("Transfer", new Point(315, 22), new Size(60, 24));
        transfer.Click += (_, _) => OpenTransferDialog();
        players.Controls.Add(transfer);
        var loan = LegacyButton("Loan", new Point(315, 53), new Size(60, 24));
        loan.Click += (_, _) => ShowLoanDetails();
        players.Controls.Add(loan);
        _teamPlayers.Location = new Point(7, 187);
        _teamPlayers.Size = new Size(370, 600);
        _teamPlayers.View = View.Details;
        _teamPlayers.FullRowSelect = true;
        _teamPlayers.GridLines = true;
        _teamPlayers.Columns.Add("Number", 50);
        _teamPlayers.Columns.Add("Display Name", 175);
        _teamPlayers.Columns.Add("Position", 60);
        _teamPlayers.Columns.Add("Overall", 60);
        _teamPlayers.ItemDrag += (_, e) =>
        {
            if (e.Item is ListViewItem item && item.Tag is int playerId && playerId > 0)
                _teamPlayers.DoDragDrop(playerId, DragDropEffects.Copy);
        };
        _teamPlayers.SelectedIndexChanged += (_, _) => ShowSelectedRosterPlayer();
        _teamPlayers.DoubleClick += (_, _) => OpenSelectedRosterPlayer();
        players.Controls.Add(_teamPlayers);
        canvas.Controls.Add(players);

        var available = Group("Available Players", new Point(390, 3), new Size(335, 798));
        var addTransfer = LegacyButton("Transfer <<", new Point(8, 22), new Size(65, 38));
        addTransfer.Click += (_, _) => OpenTransferDialog();
        available.Controls.Add(addTransfer);
        var addLoan = LegacyButton("Loan <<", new Point(8, 65), new Size(65, 38));
        addLoan.Click += (_, _) => ShowLoanDetails();
        available.Controls.Add(addLoan);
        _availablePlayers.Location = new Point(6, 185);
        _availablePlayers.Size = new Size(322, 602);
        _availablePlayers.View = View.Details;
        _availablePlayers.FullRowSelect = true;
        _availablePlayers.GridLines = true;
        _availablePlayers.Columns.Add("Display Name", 165);
        _availablePlayers.Columns.Add("Position", 60);
        _availablePlayers.Columns.Add("Overall", 60);
        available.Controls.Add(_availablePlayers);
        canvas.Controls.Add(available);

        var pitch = Group("Starting Lineup", new Point(731, 3), new Size(990, 795));
        var board = new Panel { Location = new Point(8, 20), Size = new Size(650, 500), BackColor = Color.FromArgb(106, 190, 87), BorderStyle = BorderStyle.FixedSingle, AllowDrop = true };
        board.Paint += (_, e) => DrawPitch(e.Graphics, board.ClientRectangle);
        board.DragEnter += (_, e) => e.Effect = e.Data?.GetDataPresent(typeof(int)) == true ? DragDropEffects.Copy : DragDropEffects.None;
        board.DragDrop += (_, e) => AssignDroppedPlayer(e, board);
        _formationBoard = board;
        CreateLineupSlots(board);
        pitch.Controls.Add(board);
        var bench = Group("Reserve Squad", new Point(666, 20), new Size(300, 500));
        bench.Controls.Add(new Label
        {
            Text = "Matchday substitutes · double-click to open player",
            Location = new Point(12, 20), Size = new Size(276, 18), Font = LegacyFont,
            ForeColor = SystemColors.GrayText
        });
        _matchdayBench.Location = new Point(12, 42);
        _matchdayBench.Size = new Size(276, 443);
        _matchdayBench.Font = LegacyFont;
        _matchdayBench.View = View.Details;
        _matchdayBench.FullRowSelect = true;
        _matchdayBench.GridLines = true;
        _matchdayBench.HideSelection = false;
        _matchdayBench.Columns.Add("No.", 42);
        _matchdayBench.Columns.Add("Player", 166);
        _matchdayBench.Columns.Add("Pos.", 55);
        _matchdayBench.DoubleClick += (_, _) =>
        {
            if (_matchdayBench.SelectedItems.Count > 0 && _matchdayBench.SelectedItems[0].Tag is int playerId && playerId > 0)
            {
                var recordIndex = FindTableRow("players", "playerid", playerId);
                if (recordIndex >= 0) Services.RequestRecordNavigation("players", recordIndex);
            }
        };
        bench.Controls.Add(_matchdayBench);
        pitch.Controls.Add(bench);
        pitch.Controls.Add(new Label { Text = "Formation", Location = new Point(15, 535), Size = new Size(67, 20), Font = LegacyFont });
        _formationView.Location = new Point(88, 532);
        _formationView.Size = new Size(260, 21);
        _formationView.Font = LegacyFont;
        _formationView.DropDownHeight = 340;
        _formationView.SelectedIndexChanged += (_, _) =>
        {
            if (!_syncFormationView && _formationView.SelectedItem is FormationChoice choice)
                SelectTeamFormation(choice);
        };
        pitch.Controls.Add(_formationView);
        _formationStatus = new Label { Location = new Point(355, 535), Size = new Size(610, 20), Font = LegacyFont, ForeColor = SystemColors.GrayText, Visible = false };
        pitch.Controls.Add(_formationStatus);
        ToolTip.SetToolTip(_formationView, "Choose a formation template for this team.");
        AddPlayerReferencePickers(pitch, new[] { ("Captain", "captainid"), ("Left Corner", "leftcornerkicktakerid"), ("Right Corner", "rightcornerkicktakerid"), ("Penalty", "penaltytakerid"), ("Free Kicks", "freekicktakerid") }, 15, 565);
        canvas.Controls.Add(pitch);
    }

    private sealed class LineupSlot
    {
        public required Label Label { get; init; }
        public required string PlayerField { get; init; }
        public int PlayerId { get; set; }
        public int LoadedMinifacePlayerId { get; set; }
        public string ExpectedPosition { get; set; } = string.Empty;
    }

    private sealed record FormationChoice(int RecordIndex, int FormationId, string Name, bool IsGeneric)
    {
        public override string ToString() => Name;
    }

    private void CreateLineupSlots(Panel board)
    {
        foreach (var _ in Enumerable.Range(0, 11))
        {
            var label = new Label
            {
                Size = new Size(128, 48), BackColor = Color.FromArgb(17, 38, 56),
                BorderStyle = BorderStyle.FixedSingle, TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 7.1F, FontStyle.Bold),
                ForeColor = Color.White, AllowDrop = true, Tag = _lineupSlots.Count,
                ImageAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 1, 4, 1)
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
        using var stripe = new SolidBrush(Color.FromArgb(18, 255, 255, 255));
        var playable = Rectangle.Inflate(bounds, -8, -8);
        var stripeHeight = Math.Max(1, playable.Height / 10);
        for (var row = 0; row < 10; row += 2)
            graphics.FillRectangle(stripe, playable.Left, playable.Top + (row * stripeHeight),
                playable.Width, stripeHeight);

        using var line = new Pen(Color.FromArgb(235, Color.White), 2f);
        graphics.DrawRectangle(line, playable);
        graphics.DrawLine(line, playable.Left, playable.Top + (playable.Height / 2),
            playable.Right, playable.Top + (playable.Height / 2));
        graphics.DrawEllipse(line,
            playable.Left + (playable.Width / 2) - 45,
            playable.Top + (playable.Height / 2) - 45, 90, 90);
        graphics.FillEllipse(Brushes.White,
            playable.Left + (playable.Width / 2) - 3,
            playable.Top + (playable.Height / 2) - 3, 6, 6);

        var penaltyWidth = Math.Max(150, playable.Width / 3);
        var penaltyHeight = Math.Max(54, playable.Height / 7);
        var penaltyLeft = playable.Left + ((playable.Width - penaltyWidth) / 2);
        graphics.DrawRectangle(line, penaltyLeft, playable.Top, penaltyWidth, penaltyHeight);
        graphics.DrawRectangle(line, penaltyLeft, playable.Bottom - penaltyHeight, penaltyWidth, penaltyHeight);

        var sixWidth = Math.Max(74, playable.Width / 7);
        var sixHeight = Math.Max(24, playable.Height / 18);
        var sixLeft = playable.Left + ((playable.Width - sixWidth) / 2);
        graphics.DrawRectangle(line, sixLeft, playable.Top, sixWidth, sixHeight);
        graphics.DrawRectangle(line, sixLeft, playable.Bottom - sixHeight, sixWidth, sixHeight);
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
            // Stored positions may sit on the pitch edge. Keep the whole player
            // card visible now that full names can wrap across multiple lines.
            slot.Label.Location = new Point(
                Math.Clamp(x - (slot.Label.Width / 2), 8, _formationBoard.Width - slot.Label.Width - 8),
                Math.Clamp(y - (slot.Label.Height / 2), 8, _formationBoard.Height - slot.Label.Height - 8));
            slot.ExpectedPosition = positionColumn >= 0 ? NameResolverService.PositionLabel(Parse(record.Get(positionColumn))) : "Not stored";
            slot.Label.Visible = true;
        }
        ResolveLineupCardCollisions();
        _formationStatus!.Text = status ?? choice.Name;
        RenderLineup();
    }

    /// <summary>
    /// FC26 formation offsets describe points, while CM26 draws a full
    /// miniface/name card around each point. Adjacent stored points can
    /// therefore overlap even when the game formation itself is valid.
    /// Keep the nearest non-overlapping card position without changing any
    /// database formation coordinate.
    /// </summary>
    private void ResolveLineupCardCollisions()
    {
        if (_formationBoard == null) return;
        var placed = new List<Rectangle>();
        foreach (var slot in _lineupSlots
                     .Where(item => item.Label.Visible)
                     .OrderBy(item => item.Label.Top)
                     .ThenBy(item => item.Label.Left))
        {
            var desired = slot.Label.Location;
            var candidates =
                from radius in Enumerable.Range(0, 22).Select(value => value * 6)
                from dy in Enumerable.Range(-radius / 6, (radius * 2 / 6) + 1).Select(value => value * 6)
                let dxMagnitude = radius - Math.Abs(dy)
                from dx in dxMagnitude == 0 ? new[] { 0 } : new[] { -dxMagnitude, dxMagnitude }
                let x = Math.Clamp(desired.X + dx, 8, _formationBoard.Width - slot.Label.Width - 8)
                let y = Math.Clamp(desired.Y + dy, 8, _formationBoard.Height - slot.Label.Height - 8)
                let bounds = new Rectangle(x, y, slot.Label.Width, slot.Label.Height)
                orderby (x - desired.X) * (x - desired.X) + (y - desired.Y) * (y - desired.Y)
                select bounds;

            var selected = candidates.FirstOrDefault(candidate =>
            {
                var padded = candidate;
                padded.Inflate(5, 5);
                return placed.All(existing => !padded.IntersectsWith(existing));
            });
            if (selected.Width == 0)
                selected = new Rectangle(desired, slot.Label.Size);
            slot.Label.Location = selected.Location;
            selected.Inflate(5, 5);
            placed.Add(selected);
        }
    }

    private static int ReadFormationOffset(CM26.Application.Models.DbTable table, CM26.Application.Models.DbRecord record, string field, int extent)
    {
        var raw = record.Get(Col(table, field));
        var value = double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 0d;
        // FC26 stores these offsets as fractional board coordinates (for example
        // 0.05, 0.50 and 0.95). This is a display-only conversion.
        var normalized = value is >= 0d and <= 1d ? value : value / 100d;
        return Math.Clamp((int)Math.Round(Math.Clamp(normalized, 0d, 1d) * (extent - 16)) + 8, 8, extent - 8);
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
            for (var row = 0; row < formations.RowCount; row++)
            {
                var record = Services.Session.GetRecord("formations", row);
                if (record == null || !int.TryParse(record.Get(teamColumn), out var owner)) continue;
                var name = record.Get(nameColumn);
                var formationId = Parse(record.Get(idColumn));
                if (owner < 0 && !string.IsNullOrWhiteSpace(name))
                {
                    genericChoices.Add(new FormationChoice(row, formationId, name, IsGeneric: true));
                }
                else if (owner == teamId)
                {
                    _activeTeamFormationRow = row;
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
        if (name.Length <= 16) return name;
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}. {parts[^1]}" : name[..16];
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
        if (playerId <= 0 || slot.LoadedMinifacePlayerId == playerId) return;
        slot.LoadedMinifacePlayerId = playerId;
        var local = Services.Assets.GetPlayerMiniface(playerId);
        if (!string.IsNullOrWhiteSpace(local) && File.Exists(local))
        {
            var image = Services.Textures.CreatePreview(local, 28, 28);
            if (image is not null) SetLineupMiniface(slot, image);
            return;
        }

        if (!Services.FrostbiteAssets.IsAvailable) return;
        _ = Task.Run(() =>
        {
            var asset = Services.FrostbiteAssets.ExportLegacyAsset($"data/ui/imgAssets/heads/p{playerId}.dds");
            return string.IsNullOrWhiteSpace(asset) ? null : Services.Textures.CreatePreview(asset, 28, 28);
        }).ContinueWith(task =>
        {
            var image = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
            if (IsDisposed || image is null || slot.LoadedMinifacePlayerId != playerId)
                return;
            SetLineupMiniface(slot, image!);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static void ClearLineupMiniface(LineupSlot slot)
    {
        slot.LoadedMinifacePlayerId = 0;
        slot.Label.Image?.Dispose();
        slot.Label.Image = null;
    }

    private static void SetLineupMiniface(LineupSlot slot, Image image)
    {
        slot.Label.Image?.Dispose();
        slot.Label.Image = image;
        slot.Label.Invalidate();
    }

    private void AddAdboardsTab()
    {
        var page = Page("Adboards");
        var canvas = Canvas(page);
        var sources = Group("Team Adboard Sources", new Point(3, 3), new Size(600, 650));
        _adboardSources.Location = new Point(10, 23);
        _adboardSources.Size = new Size(580, 590);
        _adboardSources.View = View.Details;
        _adboardSources.FullRowSelect = true;
        _adboardSources.GridLines = true;
        _adboardSources.Font = LegacyFont;
        _adboardSources.Columns.Add("Sponsor", 235);
        _adboardSources.Columns.Add("Sponsor ID", 90);
        _adboardSources.Columns.Add("Dynamic Image", 115);
        _adboardSources.Columns.Add("Approved", 80);
        _adboardSources.SelectedIndexChanged += (_, _) => ShowSelectedAdboard();
        sources.Controls.Add(_adboardSources);
        canvas.Controls.Add(sources);

        var preview = Group("Adboard / Dynamic Sponsor Preview", new Point(610, 3), new Size(620, 420));
        _adboardPreview.Location = new Point(10, 23);
        _adboardPreview.Size = new Size(600, 320);
        _adboardPreview.BackColor = Color.White;
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

        var note = Group("Asset Mapping", new Point(610, 430), new Size(620, 155));
        note.Controls.Add(new Label
        {
            Text = "Adboard content is driven by teamsponsorlinks and its dynamicimageid. " +
                   "This view shows the exact database links and resolves the corresponding installed artwork when available.",
            Location = new Point(14, 25), Size = new Size(590, 95), Font = LegacyFont,
            ForeColor = SystemColors.ControlText
        });
        canvas.Controls.Add(note);
    }

    private void AddSponsorsTab()
    {
        var page = Page("Sponsors");
        var canvas = Canvas(page);
        var links = Group("Team Sponsor Links", new Point(3, 3), new Size(1120, 650));
        _teamSponsors.Location = new Point(12, 23);
        _teamSponsors.Size = new Size(660, 565);
        _teamSponsors.View = View.Details;
        _teamSponsors.FullRowSelect = true;
        _teamSponsors.GridLines = true;
        _teamSponsors.Font = LegacyFont;
        _teamSponsors.Columns.Add("Sponsor", 255);
        _teamSponsors.Columns.Add("Approved", 90);
        _teamSponsors.Columns.Add("Dynamic Image", 130);
        _teamSponsors.Columns.Add("Link Key", 100);
        _teamSponsors.SelectedIndexChanged += (_, _) => ShowSelectedSponsor();
        links.Controls.Add(_teamSponsors);
        _sponsorPreview.Location = new Point(690, 35);
        _sponsorPreview.Size = new Size(400, 250);
        _sponsorPreview.BackColor = Color.White;
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
            Location = new Point(12, 595), Size = new Size(1075, 28), Font = LegacyFont, ForeColor = SystemColors.GrayText
        });
        canvas.Controls.Add(links);
    }

    private void AddFlagsTab()
    {
        var page = Page("Flags");
        var canvas = Canvas(page);
        var texture = Group("Team Flags", new Point(3, 3), new Size(525, 420));
        _teamFlagPreview.Location = new Point(10, 24);
        _teamFlagPreview.Size = new Size(512, 256);
        _teamFlagPreview.BackColor = Color.White;
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
        var flag = Group("Flags", new Point(534, 3), new Size(525, 420));
        _nationFlagPreview.Location = new Point(10, 24);
        _nationFlagPreview.Size = new Size(512, 256);
        _nationFlagPreview.BackColor = Color.White;
        _nationFlagPreview.BorderStyle = BorderStyle.FixedSingle;
        _nationFlagPreview.SizeMode = PictureBoxSizeMode.Zoom;
        flag.Controls.Add(_nationFlagPreview);
        flag.Controls.Add(new Label { Text = "Flag n.", Location = new Point(12, 294), AutoSize = true, Font = LegacyFont });
        _nationFlagCaption.Location = new Point(65, 286);
        _nationFlagCaption.Size = new Size(450, 26);
        _nationFlagCaption.Font = LegacyFont;
        _nationFlagCaption.TextAlign = ContentAlignment.MiddleLeft;
        flag.Controls.Add(_nationFlagCaption);
        AddBoundFields(flag, new[]
        {
            ("Teamcolor1r", "teamcolor1r"), ("Teamcolor1g", "teamcolor1g"), ("Teamcolor1b", "teamcolor1b"),
            ("Teamcolor2r", "teamcolor2r"), ("Teamcolor2g", "teamcolor2g"), ("Teamcolor2b", "teamcolor2b")
        }, 12, 320, 130, 105, 24);
        canvas.Controls.Add(flag);
    }

    private void AddBoundFields(Control parent, IEnumerable<(string label, string field)> definitions, int labelX, int top, int editorX, int editorWidth, int rowHeight)
    {
        var row = 0;
        foreach (var (label, field) in definitions)
        {
            var y = top + (row++ * rowHeight);
            parent.Controls.Add(new Label { Text = label, Location = new Point(labelX, y + 4), AutoSize = true, Font = LegacyFont });
            var editor = new TextBox { Location = new Point(editorX, y), Size = new Size(editorWidth, 20), Font = LegacyFont, Tag = field };
            editor.Leave += (_, _) => StageEditor(editor);
            _editors.Add(editor);
            parent.Controls.Add(editor);
        }
    }

    // FC26 player-id foreign keys must be editable as relationships, not merely resolved text.
    private void AddPlayerReferencePickers(Control parent, IEnumerable<(string label, string field)> definitions, int labelX, int top)
    {
        var row = 0;
        foreach (var (label, field) in definitions)
        {
            var y = top + (row++ * 25);
            parent.Controls.Add(new Label { Text = label, Location = new Point(labelX, y + 4), AutoSize = true, Font = LegacyFont });
            var picker = new ComboBox { Location = new Point(90, y), Size = new Size(372, 21), Font = LegacyFont, DropDownStyle = ComboBoxStyle.DropDownList, Tag = field };
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

    protected override void ShowRecord(int recordIndex)
    {
        var table = Services.Session.GetTable(TableName)!;
        var record = Services.Session.GetRecord(TableName, recordIndex)!;
        var name = record.Get(Col(table, "teamname"));
        var id = record.Get(Col(table, "teamid"));
        ShowCrest(Services.Assets.GetTeamLogo(int.TryParse(id, out var crestTeamId) ? crestTeamId : 0), name, crestTeamId);
        _fields.Clear();
        foreach (var field in Services.RequireData().GetFields(TableName, recordIndex, LabelMaps.Teams))
            _fields[field.FieldName] = field;
        UpdateTeamColours();
        var countryColumn = Col(table, "countryid");
        ShowTeamFlags(crestTeamId, countryColumn >= 0 ? Parse(record.Get(countryColumn)) : 0);

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
                editor.BackColor = SystemColors.Control;
                ToolTip.SetToolTip(editor, $"{key} = {pref.RawValue} (player id)");
            }
            else if (IsLinkedDisplayField(key))
            {
                // FC26 stores some team relationships in dedicated link tables.  Present the
                // resolved name in the CM16-style form instead of an empty/-1 raw FK.
                editor.Text = ResolveLinkedValue(key, int.TryParse(id, out var linkedTeamId) ? linkedTeamId : 0);
                editor.ReadOnly = true;
                editor.BackColor = SystemColors.Control;
                ToolTip.SetToolTip(editor, $"Resolved {key}; select the linked player or roster control to change it.");
            }
            else if (_fields.TryGetValue(key, out var value))
            {
                editor.Text = value.Value;
                editor.ReadOnly = !value.IsWritable;
                editor.BackColor = value.IsWritable ? Color.White : SystemColors.Control;
            }
            else
            {
                editor.Text = ResolveLinkedValue(key, int.TryParse(id, out var linkedTeamId) ? linkedTeamId : 0);
                editor.ReadOnly = true;
                editor.BackColor = SystemColors.Control;
            }
        }

        _teamPlayers.Items.Clear();
        _availablePlayers.Items.Clear();
        var roster = Services.RequireData().GetTeamRoster(int.TryParse(id, out var teamId) ? teamId : 0);
        foreach (var player in roster)
        {
            // Display Name is a verified real name, or the documented "Player {id}" fallback.
            // It is NEVER split into a first/surname pair and NEVER a bare numeric key.
            _teamPlayers.Items.Add(new ListViewItem(new[]
            {
                player.JerseyNumber > 0 ? player.JerseyNumber.ToString() : "",
                player.Name,
                player.Position,
                player.Overall,
            }) { Tag = player.PlayerId });
        }
        if (roster.Count == 0)
            _teamPlayers.Items.Add(new ListViewItem(new[] { "", "No players linked to this team", "", "" }));
        SelectFormationLayout(teamId);
        LoadLineup(teamId, roster);
        PopulatePlayerReferencePickers(roster);
        _availablePlayers.Items.Add(new ListViewItem(new[] { "Use Transfers to move or release players through teamplayerlinks", "", "" }));
        LoadSponsors(teamId);
        LoadAudioCatalogs();
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
            _teamFlagPreview.Image?.Dispose();
            _teamFlagPreview.Image = null;
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
                    _teamFlagPreview.Image?.Dispose();
                    _teamFlagPreview.Image = image;
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
                _nationFlagPreview.Image?.Dispose();
                _nationFlagPreview.Image = image;
                _nationFlagCaption.Text = image == null
                    ? $"No nation flag ({nationId})"
                    : $"Nation flag · {source}";
            });
    }

    private void LoadSponsors(int teamId)
    {
        _teamSponsors.Items.Clear();
        _adboardSources.Items.Clear();
        _adboardPreview.Image?.Dispose();
        _adboardPreview.Image = null;
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
        for (var row = 0; row < sponsors.RowCount; row++)
        {
            var sponsor = Services.Session.GetRecord("sponsors", row);
            if (sponsor != null) sponsorNames[Parse(sponsor.Get(sponsorIdColumn))] = sponsor.Get(sponsorNameColumn);
        }
        var teamColumn = Col(links, "teamid");
        var linkSponsorColumn = Col(links, "adsponserid");
        var approvedColumn = Col(links, "isapproved");
        var imageColumn = Col(links, "dynamicimageid");
        var keyColumn = Col(links, "artificialkey");
        for (var row = 0; row < links.RowCount; row++)
        {
            var link = Services.Session.GetRecord("teamsponsorlinks", row);
            if (link == null || Parse(link.Get(teamColumn)) != teamId) continue;
            var sponsorId = Parse(link.Get(linkSponsorColumn));
            var dynamicImageId = Parse(link.Get(imageColumn));
            var approved = link.Get(approvedColumn);
            var name = sponsorNames.TryGetValue(sponsorId, out var resolved) ? resolved : $"Sponsor {sponsorId}";
            var asset = new TeamSponsorAsset(sponsorId, dynamicImageId, name);
            _teamSponsors.Items.Add(new ListViewItem(new[] { name, approved, dynamicImageId.ToString(), link.Get(keyColumn) }) { Tag = asset });
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
            _sponsorPreview.Image?.Dispose();
            _sponsorPreview.Image = image;
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
                _adboardPreview.Image?.Dispose();
                _adboardPreview.Image = image;
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

    private void ShowSelectedRosterPlayer()
    {
        if (_selectedPlayerName == null || _selectedPlayerDetails == null || _selectedPlayerFace == null) return;
        if (_teamPlayers.SelectedItems.Count == 0 || _teamPlayers.SelectedItems[0].Tag is not int playerId || !_rosterByPlayerId.TryGetValue(playerId, out var player))
        {
            _selectedPlayerName.Text = "Select a player";
            _selectedPlayerDetails.Text = "Select a roster player\nto view details.";
            return;
        }
        _selectedPlayerName.Text = player.Name;
        var injury = string.IsNullOrWhiteSpace(player.Injury) || player.Injury == "0" ? "None" : player.Injury;
        var form = string.IsNullOrWhiteSpace(player.Form) ? "Not stored" : player.Form;
        var loan = string.IsNullOrWhiteSpace(player.LoanFrom) ? "No" : $"From {player.LoanFrom} (ends {player.LoanEndDate})";
        var contract = string.IsNullOrWhiteSpace(player.ContractValidUntil) ? "Not stored" : player.ContractValidUntil;
        var joined = string.IsNullOrWhiteSpace(player.JoiningDate) ? "Not stored" : player.JoiningDate;
        _selectedPlayerDetails.Text = $"Player ID: {player.PlayerId}\nShirt number: {player.JerseyNumber}\nPosition: {player.Position}\nOverall: {player.Overall}\nContract until: {contract}\nJoined: {joined}\nLeague: {player.LeagueAppearances} apps, {player.LeagueGoals} goals\nCards: {player.YellowCards} yellow, {player.RedCards} red\nForm: {form}\nInjury: {injury}\nLoan: {loan}";
        var path = Services.Assets.GetPlayerMiniface(playerId);
        SetMiniface(_selectedPlayerFace, path, playerId);
    }

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
        dialog.Controls.Add(new Label { Text = $"{player.Name}  ({player.Position}, OVR {player.Overall})", Location = new Point(15, 16), Size = new Size(390, 22), Font = new Font(LegacyFont, FontStyle.Bold) });
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
            Location = new Point(15, 16), Size = new Size(420, 22), Font = new Font(LegacyFont, FontStyle.Bold)
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
            Location = new Point(15, 130), Size = new Size(420, 22), ForeColor = SystemColors.GrayText
        });
        var stage = new Button { Text = loanRow >= 0 ? "Stage Changes" : "Create Loan", DialogResult = DialogResult.OK, Location = new Point(185, 185), Size = new Size(105, 28) };
        var remove = new Button { Text = "Remove Loan", DialogResult = DialogResult.Yes, Location = new Point(70, 185), Size = new Size(105, 28), Enabled = loanRow >= 0 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(300, 185), Size = new Size(95, 28) };
        dialog.Controls.Add(remove);
        dialog.Controls.Add(stage);
        dialog.Controls.Add(cancel);
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
            viewer.Image?.Dispose();
            viewer.Image = null;
            return;
        }
        FrostbitePreviewLoader.LoadLegacyUiAsset(viewer, Services, path,
            $"data/ui/imgAssets/heads/p{playerId}.dds", (image, _) =>
        {
            viewer.Image?.Dispose();
            viewer.Image = image;
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
        var legacyPath = teamId > 0
            ? Services.FrostbiteAssets.ExportLegacyAsset(
                $"data/ui/imgAssets/crest/dark/l{teamId}.dds")
            : null;
        if (string.IsNullOrWhiteSpace(legacyPath) && teamId > 0)
            legacyPath = Services.FrostbiteAssets.ExportLegacyAsset(
                $"data/ui/imgAssets/crest/light/l{teamId}.dds");
        if (!string.IsNullOrWhiteSpace(legacyPath)) path = legacyPath;
        if (teamId > 0)
        {
            var crestPath = !string.IsNullOrWhiteSpace(legacyPath) && legacyPath.Contains("crest/dark")
                ? $"data/ui/imgAssets/crest/dark/l{teamId}.dds"
                : $"data/ui/imgAssets/crest/light/l{teamId}.dds";
            var target = new LegacyAssetEditTarget(crestPath, 256, 256);
            LegacyAssetActions.SetTarget(_crestViewers[0], target);
            if (_crestViewers.Count > 1)
                LegacyAssetActions.SetTarget(_crestViewers[1], new LegacyAssetEditTarget(crestPath, 64, 62));
        }
        FrostbitePreviewLoader.Load(_crestViewers[0], Services, path,
            [string.Concat(teamName.ToLowerInvariant().Where(char.IsLetterOrDigit)), $"crest_{teamId}_"], (image, source) =>
        {
            for (var index = 0; index < _crestViewers.Count; index++)
            {
                var viewer = _crestViewers[index];
                viewer.Image?.Dispose();
                viewer.Image = index == 0 ? image : image == null ? null : new Bitmap(image);
            }
            _crestCaption.Text = image == null
                ? $"{teamName}\r\nNo crest available"
                : $"{teamName}\r\n{source}";
        }, asset => asset.Name.Contains("/textures/logo/logo_", StringComparison.OrdinalIgnoreCase) ||
                    asset.Name.Contains("/crest_", StringComparison.OrdinalIgnoreCase), linearColor: true);
    }
}
