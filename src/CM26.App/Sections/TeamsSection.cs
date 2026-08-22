using System.Drawing;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
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
    private readonly List<SlotState> _lineupSlots = [];
    private readonly Dictionary<int, TeamRosterItem> _rosterByPlayerId = new();
    private readonly ComboBox _formationView = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private FormationBoard? _formationBoard;
    private Label? _formationStatus;
    private FormationChoice? _activeFormationChoice;
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
    private Label _teamFanbaseLabel = new();
    private Label _teamYouthLabel = new();
    private Label _teamFinanceLabel = new();
    private Label _teamHonoursLabel = new();
    private Label _teamStadiumLabel = new();
    private Label _teamRivalLabel = new();
    private readonly PictureBox _teamKitHome = new();
    private readonly PictureBox _teamKitAway = new();
    private readonly PictureBox _teamKitThird = new();
    private readonly PictureBox _teamKitGk = new();
    private readonly PictureBox _teamStadiumImg = new();
    private readonly PictureBox _teamManagerImg = new();
    private Label _teamManagerName = new();
    private Label _teamManagerNation = new();
    private int _activeTeamPreviewId;

    // Studio overview
    private StudioToolbar? _toolbar;
    private TeamHeroCard? _heroCard;
    private StudioCard? _stadiumCard;
    private StudioCard? _managerCard;
    private StudioCard? _rivalCard;
    private StudioCard? _metadataCard;
    private StudioCard? _tacticsCard;
    private StudioCard? _kitsCard;
    private StudioCard? _actionsCard;
    private readonly Label _teamFormationLabel = new();
    private readonly ComboBox _buildUpStylePicker = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _defensiveLinePicker = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _defensiveDepthEditor = new() { Minimum = 1, Maximum = 100 };
    private readonly Label _teamTraitsSummary = new();
    private bool _syncTacticsCard;

    // Studio roster
    private readonly List<RosterPlayerRow> _rosterRows = [];
    private int _selectedRosterPlayerId;
    private Panel? _rosterScrollPanel;
    private Panel? _startingXiSection;
    private Panel? _subsSection;
    private Panel? _reservesSection;

    protected override bool ShowRecordCommandStrip => false;

    public override string SectionKey => "teams";
    public override string SectionTitle => "Teams";
    protected override string TableName => "teams";
    // A standalone team starts unlinked. League editors can link it later, while
    // this command always creates a valid editable squad for the new record.
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search teams…";

    public TeamsSection(AppServices services) : base(services)
    {
        Header.Visible = false;
        EmptyState.Visible = false;
        Tabs.BringToFront();
        Tabs.Font = LegacyFont;
        Tabs.Padding = new Point(4, 2);
        Theme.ApplyTabs(Tabs);
        AddOverviewTab();
        AddRosterTab();
        AddSponsorsTab();
        AddAdboardsTab();
        AddFlagsTab();
        AddAudioTab();
    }

    public override void ActivateSection()
    {
        base.ActivateSection();
        UpdateToolbarCount();
        if (CurrentRecordIndex < 0 && Services.Session.IsLoaded)
        {
            var records = GetRecords();
            if (records.Count > 0) GoToRecord(records[0].RecordIndex);
        }
    }

    private void UpdateToolbarCount()
    {
        if (_toolbar == null) return;
        try
        {
            var count = Services.Session.IsLoaded ? GetRecords().Count : 0;
            _toolbar.RecordCountText = $"{count:N0} records";
        }
        catch { _toolbar.RecordCountText = string.Empty; }
    }

    private void HeroCard_BudgetChanged(object? sender, long budget)
    {
        if (CurrentRecordIndex < 0) return;
        var financial = TeamFinancialFieldResolver.Resolve(Services.Session.GetTable(TableName));
        if (financial is null) return;
        StageField(TableName, CurrentRecordIndex, financial.FieldName, budget.ToString(), _stagingGrid);
    }

    private void StepRecord(int delta)
    {
        var records = GetRecords();
        var found = -1;
        for (var i = 0; i < records.Count; i++)
        {
            if (records[i].RecordIndex == CurrentRecordIndex)
            {
                found = i;
                break;
            }
        }
        if (found < 0)
        {
            if (records.Count > 0) GoToRecord(records[0].RecordIndex);
            return;
        }
        var next = found + delta;
        if (next >= 0 && next < records.Count)
            GoToRecord(records[next].RecordIndex);
    }

    private void SearchTeams(string query)
    {
        var term = query.Trim();
        if (term.Length == 0) return;
        var result = GetRecords().FirstOrDefault(item => item.Matches(term));
        if (result == null)
        {
            MessageBox.Show(this, $"No team matches '{term}'.", "Search Team",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        GoToRecord(result.RecordIndex);
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
                ["transferbudget"] = "1000000",
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
                catch (Exception ex) { Program.Log($"[CM26] Team league link failed: {ex.Message}"); /* Non-critical: team created without league link */ }
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

    private static TabPage Page(string text)
    {
        var page = new TabPage(text) { BackColor = StudioColors.AppBackground, Font = LegacyFont };
        return page;
    }

    private static Panel Canvas(TabPage page)
    {
        var canvas = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            AutoScroll = true,
            Padding = new Padding(StudioSpacing.Medium),
        };
        page.Controls.Add(canvas);
        return canvas;
    }

    private static StudioCard StudioGroup(string title, Color accent)
    {
        var card = new StudioCard
        {
            AccentColor = accent,
            Margin = new Padding(0, 0, StudioSpacing.Medium, StudioSpacing.Medium),
        };
        var header = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 22,
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(header);
        return card;
    }

    private static PictureBox Viewer(Size size)
    {
        return new PictureBox
        {
            Size = size,
            BackColor = StudioColors.InputBackground,
            BorderStyle = BorderStyle.None,
            SizeMode = PictureBoxSizeMode.Zoom,
        };
    }

    private static Button StudioButton(string text, int width = 90)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(width, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = StudioColors.RaisedSurface,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.Button,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        btn.FlatAppearance.BorderColor = StudioColors.CardBorder;
        btn.FlatAppearance.MouseOverBackColor = StudioColors.CardBorder;
        return btn;
    }

    private static Label MakeLabel(string text, Font font, Color color, bool autoSize = true)
    {
        return new Label
        {
            Text = text,
            Font = font,
            ForeColor = color,
            BackColor = Color.Transparent,
            AutoSize = autoSize,
        };
    }

    private void AddOverviewTab()
    {
        var page = Page("Overview");
        var canvas = Canvas(page);

        _toolbar = new StudioToolbar
        {
            Title = "Teams",
            CanCreate = true,
            ShowFilter = true,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchBox.PlaceholderText = "Search teams…";
        _toolbar.NewClicked += (_, _) => CreateNewRecord();
        _toolbar.PreviousClicked += (_, _) => StepRecord(-1);
        _toolbar.NextClicked += (_, _) => StepRecord(+1);
        _toolbar.SearchClicked += (_, _) => SearchTeams(_toolbar.SearchText);
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            SearchTeams(_toolbar.SearchText);
        };
        _toolbar.FilterClicked += (_, _) => FocusSearchBox();
        canvas.Controls.Add(_toolbar);

        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(StudioSpacing.Medium),
            AutoScroll = true,
        };
        canvas.Controls.Add(scrollPanel);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = StudioColors.AppBackground,
        };

        _heroCard = new TeamHeroCard { Dock = DockStyle.Fill };
        _heroCard.BudgetChanged += HeroCard_BudgetChanged;
        layout.Controls.Add(_heroCard, 0, 0);

        var quickRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
            Margin = new Padding(0, StudioSpacing.Medium, 0, 0),
        };
        _stadiumCard = BuildStadiumCard();
        _managerCard = BuildManagerCard();
        _rivalCard = BuildRivalCard();
        _metadataCard = BuildMetadataCard();
        _tacticsCard = BuildTacticsCard();
        quickRow.Controls.Add(_stadiumCard);
        quickRow.Controls.Add(_managerCard);
        quickRow.Controls.Add(_rivalCard);
        quickRow.Controls.Add(_metadataCard);
        quickRow.Controls.Add(_tacticsCard);
        layout.Controls.Add(quickRow, 0, 1);

        _kitsCard = BuildKitsCard();
        layout.Controls.Add(_kitsCard, 0, 2);

        _actionsCard = BuildActionsCard();
        layout.Controls.Add(_actionsCard, 0, 3);

        scrollPanel.Controls.Add(layout);
        Tabs.TabPages.Add(page);
    }

    private StudioCard BuildStadiumCard()
    {
        var card = StudioGroup("Stadium", StudioColors.CyanAccent);
        card.Width = 260;
        card.Height = 260;

        _teamStadiumImg.Size = new Size(228, 140);
        _teamStadiumImg.Location = new Point(StudioSpacing.Medium, 30);
        card.Controls.Add(_teamStadiumImg);

        _teamStadiumLabel = MakeLabel("—", StudioFonts.DataValue, StudioColors.PrimaryText, false);
        _teamStadiumLabel.Location = new Point(StudioSpacing.Medium, 178);
        _teamStadiumLabel.Size = new Size(228, 20);
        card.Controls.Add(_teamStadiumLabel);

        return card;
    }

    private StudioCard BuildManagerCard()
    {
        var card = StudioGroup("Manager", StudioColors.Yellow);
        card.Width = 260;
        card.Height = 260;

        _teamManagerImg.Size = new Size(120, 120);
        _teamManagerImg.Location = new Point(StudioSpacing.Medium, 30);
        card.Controls.Add(_teamManagerImg);

        _teamManagerName = MakeLabel("—", StudioFonts.DataValue, StudioColors.PrimaryText, false);
        _teamManagerName.Location = new Point(StudioSpacing.Medium, 156);
        _teamManagerName.Size = new Size(228, 20);
        card.Controls.Add(_teamManagerName);

        _teamManagerNation = MakeLabel("", StudioFonts.DataLabel, StudioColors.MutedText, false);
        _teamManagerNation.Location = new Point(StudioSpacing.Medium, 180);
        _teamManagerNation.Size = new Size(228, 20);
        card.Controls.Add(_teamManagerNation);

        return card;
    }

    private StudioCard BuildRivalCard()
    {
        var card = StudioGroup("Rival", StudioColors.Red);
        card.Width = 260;
        card.Height = 260;

        _teamRivalLabel = MakeLabel("—", StudioFonts.DataValue, StudioColors.PrimaryText, false);
        _teamRivalLabel.Location = new Point(StudioSpacing.Medium, 30);
        _teamRivalLabel.Size = new Size(228, 20);
        card.Controls.Add(_teamRivalLabel);

        return card;
    }

    private StudioCard BuildMetadataCard()
    {
        var card = StudioGroup("Team Metadata", StudioColors.Purple);
        card.Width = 260;
        card.Height = 260;

        _teamFoundationLabel = MakeLabel("Founded: —", StudioFonts.DataValue, StudioColors.PrimaryText, false);
        _teamFoundationLabel.Location = new Point(StudioSpacing.Medium, 30);
        _teamFoundationLabel.Size = new Size(228, 20);
        card.Controls.Add(_teamFoundationLabel);

        _teamWorthLabel = MakeLabel("Worth: —", StudioFonts.DataValue, StudioColors.PrimaryText, false);
        _teamWorthLabel.Location = new Point(StudioSpacing.Medium, 56);
        _teamWorthLabel.Size = new Size(228, 20);
        card.Controls.Add(_teamWorthLabel);

        _teamFanbaseLabel = MakeLabel("Fanbase: —", StudioFonts.DataLabel, StudioColors.PrimaryText, false);
        _teamFanbaseLabel.Location = new Point(StudioSpacing.Medium, 84);
        _teamFanbaseLabel.Size = new Size(228, 20);
        card.Controls.Add(_teamFanbaseLabel);

        _teamYouthLabel = MakeLabel("Youth facilities: —", StudioFonts.DataLabel, StudioColors.PrimaryText, false);
        _teamYouthLabel.Location = new Point(StudioSpacing.Medium, 110);
        _teamYouthLabel.Size = new Size(228, 20);
        card.Controls.Add(_teamYouthLabel);

        _teamFinanceLabel = MakeLabel("Financial stability: —", StudioFonts.DataLabel, StudioColors.PrimaryText, false);
        _teamFinanceLabel.Location = new Point(StudioSpacing.Medium, 136);
        _teamFinanceLabel.Size = new Size(228, 20);
        card.Controls.Add(_teamFinanceLabel);

        _teamHonoursLabel = MakeLabel("Honours: —", StudioFonts.DataLabel, StudioColors.MutedText, false);
        _teamHonoursLabel.Location = new Point(StudioSpacing.Medium, 164);
        _teamHonoursLabel.Size = new Size(228, 46);
        card.Controls.Add(_teamHonoursLabel);

        var careerNote = MakeLabel("Budget & objectives: Career save only", StudioFonts.DataLabel, StudioColors.MutedText, false);
        careerNote.Location = new Point(StudioSpacing.Medium, 218);
        careerNote.Size = new Size(228, 20);
        card.Controls.Add(careerNote);

        return card;
    }

    private StudioCard BuildTacticsCard()
    {
        var card = StudioGroup("FC26 Tactics & Traits", StudioColors.Green);
        card.Width = 340;
        card.Height = 260;

        AddTacticsLabel(card, "Formation", 30);
        _teamFormationLabel.Location = new Point(122, 30);
        _teamFormationLabel.Size = new Size(200, 20);
        _teamFormationLabel.Font = StudioFonts.DataValue;
        _teamFormationLabel.ForeColor = StudioColors.PrimaryText;
        _teamFormationLabel.BackColor = Color.Transparent;
        card.Controls.Add(_teamFormationLabel);

        AddTacticsLabel(card, "Build-up style", 58);
        _buildUpStylePicker.Location = new Point(122, 55);
        _buildUpStylePicker.Size = new Size(200, 23);
        _buildUpStylePicker.Font = LegacyFont;
        _buildUpStylePicker.Items.AddRange(TeamTacticsMaps.BuildUpStyles.Cast<object>().ToArray());
        Theme.ApplyCombo(_buildUpStylePicker);
        _buildUpStylePicker.SelectedIndexChanged += (_, _) => StageBuildUpStyle();
        card.Controls.Add(_buildUpStylePicker);

        AddTacticsLabel(card, "Defensive line", 86);
        _defensiveLinePicker.Location = new Point(122, 83);
        _defensiveLinePicker.Size = new Size(200, 23);
        _defensiveLinePicker.Font = LegacyFont;
        _defensiveLinePicker.Items.AddRange(TeamTacticsMaps.DefensivePresets.Cast<object>().ToArray());
        Theme.ApplyCombo(_defensiveLinePicker);
        _defensiveLinePicker.SelectedIndexChanged += (_, _) => StageDefensivePreset();
        card.Controls.Add(_defensiveLinePicker);

        AddTacticsLabel(card, "Line height", 114);
        _defensiveDepthEditor.Location = new Point(122, 111);
        _defensiveDepthEditor.Size = new Size(80, 23);
        _defensiveDepthEditor.Font = LegacyFont;
        _defensiveDepthEditor.TextAlign = HorizontalAlignment.Center;
        _defensiveDepthEditor.Leave += (_, _) => StageDefensiveDepth((int)_defensiveDepthEditor.Value);
        card.Controls.Add(_defensiveDepthEditor);

        _teamTraitsSummary.Location = new Point(16, 142);
        _teamTraitsSummary.Size = new Size(306, 58);
        _teamTraitsSummary.Font = StudioFonts.Metadata;
        _teamTraitsSummary.ForeColor = StudioColors.MutedText;
        _teamTraitsSummary.BackColor = Color.Transparent;
        _teamTraitsSummary.AutoEllipsis = true;
        card.Controls.Add(_teamTraitsSummary);

        var editTraits = StudioButton("Edit known team traits…", 180);
        editTraits.Location = new Point(16, 210);
        editTraits.Click += (_, _) => OpenTeamTraitsEditor();
        card.Controls.Add(editTraits);

        ToolTip.SetToolTip(_defensiveDepthEditor,
            "FC26 line height: Deep ≤30, Balanced 31–60, High 61–89, Aggressive ≥90.");
        ToolTip.SetToolTip(_teamTraitsSummary,
            "FC26 stores three 23-bit masks. CM26 decodes the ten known legacy-compatible bits and preserves all higher bits.");
        return card;
    }

    private static void AddTacticsLabel(Control parent, string text, int y) => parent.Controls.Add(new Label
    {
        Text = text,
        Location = new Point(16, y),
        Size = new Size(100, 20),
        TextAlign = ContentAlignment.MiddleRight,
        Font = StudioFonts.DataLabel,
        ForeColor = StudioColors.MutedText,
        BackColor = Color.Transparent,
    });

    private void RefreshTacticsCard(int teamId)
    {
        if (_tacticsCard == null || CurrentRecordIndex < 0) return;
        var buildUp = Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, "buildupplay"));
        var depth = Math.Clamp(Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, "defensivedepth")), 1, 100);

        _syncTacticsCard = true;
        try
        {
            _buildUpStylePicker.SelectedItem = _buildUpStylePicker.Items
                .Cast<TeamTacticsMaps.Option>()
                .FirstOrDefault(option => option.Value == buildUp);

            var presetValues = TeamTacticsMaps.DefensivePresets.Select(option => option.Value).ToHashSet();
            for (var index = _defensiveLinePicker.Items.Count - 1; index >= 0; index--)
            {
                if (_defensiveLinePicker.Items[index] is TeamTacticsMaps.Option option && !presetValues.Contains(option.Value))
                    _defensiveLinePicker.Items.RemoveAt(index);
            }
            var line = _defensiveLinePicker.Items.Cast<TeamTacticsMaps.Option>()
                .FirstOrDefault(option => option.Value == depth);
            if (line == null)
            {
                line = new TeamTacticsMaps.Option(depth, TeamTacticsMaps.DefensiveApproach(depth));
                _defensiveLinePicker.Items.Add(line);
            }
            _defensiveLinePicker.SelectedItem = line;
            _defensiveDepthEditor.Value = depth;
            _teamFormationLabel.Text = ResolveTeamFormationName(teamId);
            RefreshTraitSummary();
        }
        finally { _syncTacticsCard = false; }
    }

    private string ResolveTeamFormationName(int teamId)
    {
        var formations = Services.Session.GetTable("formations");
        if (formations == null) return "Not available";
        var teamColumn = Col(formations, "teamid");
        var nameColumn = Col(formations, "formationname");
        if (teamColumn < 0 || nameColumn < 0) return "Not available";
        for (var row = 0; row < formations.RowCount; row++)
        {
            var record = Services.Session.GetRecord("formations", row);
            if (record != null && Parse(record.Get(teamColumn)) == teamId)
                return string.IsNullOrWhiteSpace(record.Get(nameColumn)) ? "Unnamed formation" : record.Get(nameColumn);
        }
        return "Not linked";
    }

    private void StageBuildUpStyle()
    {
        if (_syncTacticsCard || _buildUpStylePicker.SelectedItem is not TeamTacticsMaps.Option option) return;
        if (StageTeamTactic("buildupplay", option.Value)) RefreshTacticsCard(CurrentTeamId());
    }

    private void StageDefensivePreset()
    {
        if (_syncTacticsCard || _defensiveLinePicker.SelectedItem is not TeamTacticsMaps.Option option) return;
        StageDefensiveDepth(option.Value);
    }

    private void StageDefensiveDepth(int depth)
    {
        if (_syncTacticsCard) return;
        depth = Math.Clamp(depth, 1, 100);
        if (StageTeamTactic("defensivedepth", depth)) RefreshTacticsCard(CurrentTeamId());
    }

    private bool StageTeamTactic(string field, int value)
    {
        if (CurrentRecordIndex < 0) return false;
        var teamId = CurrentTeamId();
        if (teamId <= 0) return false;
        var raw = value.ToString(CultureInfo.InvariantCulture);
        if (!StageField(TableName, CurrentRecordIndex, field, raw, _stagingGrid)) return false;

        var mentalityRow = FindActiveDefaultMentalityRow(teamId);
        if (mentalityRow >= 0)
            StageWritableMirror("default_mentalities", mentalityRow, field, raw);

        if (field.Equals("defensivedepth", StringComparison.OrdinalIgnoreCase))
        {
            var defaultTeamRow = FindTeamRow("defaultteamdata", teamId);
            if (defaultTeamRow >= 0)
                StageWritableMirror("defaultteamdata", defaultTeamRow, field, raw);
        }
        return true;
    }

    private void StageWritableMirror(string tableName, int row, string field, string value)
    {
        var table = Services.Session.GetTable(tableName);
        if (table?.FindColumn(field)?.IsWritable == true)
            StageField(tableName, row, field, value, _stagingGrid);
    }

    private int FindActiveDefaultMentalityRow(int teamId)
    {
        var table = Services.Session.GetTable("default_mentalities");
        if (table == null) return -1;
        var teamColumn = Col(table, "teamid");
        var buildColumn = Col(table, "buildupplay");
        var depthColumn = Col(table, "defensivedepth");
        if (teamColumn < 0 || buildColumn < 0 || depthColumn < 0) return -1;

        var candidates = new List<TeamTacticsMaps.MentalityCandidate>();
        for (var row = 0; row < table.RowCount; row++)
        {
            var record = Services.Session.GetRecord("default_mentalities", row);
            if (record == null || Parse(record.Get(teamColumn)) != teamId) continue;
            candidates.Add(new(row, Parse(record.Get(buildColumn)), Parse(record.Get(depthColumn))));
        }
        return TeamTacticsMaps.FindActiveMentalityRow(candidates);
    }

    private int FindTeamRow(string tableName, int teamId)
    {
        var table = Services.Session.GetTable(tableName);
        var teamColumn = table == null ? -1 : Col(table, "teamid");
        if (table == null || teamColumn < 0) return -1;
        for (var row = 0; row < table.RowCount; row++)
        {
            var record = Services.Session.GetRecord(tableName, row);
            if (record != null && Parse(record.Get(teamColumn)) == teamId) return row;
        }
        return -1;
    }

    private int CurrentTeamId() => CurrentRecordIndex < 0
        ? 0
        : Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, "teamid"));

    private void RefreshTraitSummary()
    {
        var weak = TraitValue("trait1vweak");
        var equal = TraitValue("trait1vequal");
        var strong = TraitValue("trait1vstrong");
        var equalNames = TeamTacticsMaps.DecodeKnownTraits(equal);
        _teamTraitsSummary.Text =
            $"Known flags  W:{TeamTacticsMaps.DecodeKnownTraits(weak).Count}  E:{equalNames.Count}  S:{TeamTacticsMaps.DecodeKnownTraits(strong).Count}\n" +
            $"Equal: {(equalNames.Count == 0 ? "none" : string.Join(", ", equalNames))}";
        ToolTip.SetToolTip(_teamTraitsSummary,
            $"Weak 0x{weak:X6}: {TraitNames(weak)}\nEqual 0x{equal:X6}: {TraitNames(equal)}\nStrong 0x{strong:X6}: {TraitNames(strong)}");
    }

    private int TraitValue(string field) => CurrentRecordIndex < 0
        ? 0
        : Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, field));

    private static string TraitNames(int value)
    {
        var names = TeamTacticsMaps.DecodeKnownTraits(value);
        return names.Count == 0 ? "no known low-bit flags" : string.Join(", ", names);
    }

    private sealed record TraitContextChoice(string Label, string Field)
    {
        public override string ToString() => Label;
    }

    private void OpenTeamTraitsEditor()
    {
        if (CurrentRecordIndex < 0) return;
        var contexts = new[]
        {
            new TraitContextChoice("Versus weaker team", "trait1vweak"),
            new TraitContextChoice("Versus equal team", "trait1vequal"),
            new TraitContextChoice("Versus stronger team", "trait1vstrong"),
        };
        var original = contexts.ToDictionary(context => context.Field, context => TraitValue(context.Field));
        var edited = original.ToDictionary(pair => pair.Key, pair => pair.Value);

        using var dialog = new Form
        {
            Text = "FC26 Team Traits", StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
            ClientSize = new Size(520, 430), Font = LegacyFont, BackColor = Theme.Background,
        };
        dialog.Controls.Add(new Label
        {
            Text = "Opponent context", Location = new Point(16, 18), Size = new Size(120, 22),
            ForeColor = Theme.Text, BackColor = Color.Transparent,
        });
        var contextPicker = new ComboBox
        {
            Location = new Point(140, 15), Size = new Size(350, 23),
            DropDownStyle = ComboBoxStyle.DropDownList, Font = LegacyFont,
        };
        contextPicker.Items.AddRange(contexts.Cast<object>().ToArray());
        dialog.Controls.Add(contextPicker);
        var flags = new CheckedListBox
        {
            Location = new Point(16, 52), Size = new Size(474, 260),
            CheckOnClick = true, Font = LegacyFont,
        };
        flags.Items.AddRange(TeamTacticsMaps.KnownTraitNames.Cast<object>().ToArray());
        dialog.Controls.Add(flags);
        var note = new Label
        {
            Text = "Only the ten verified legacy-compatible bits are named here. " +
                   "All higher FC26 bits remain unchanged when these boxes are edited.",
            Location = new Point(16, 322), Size = new Size(474, 48),
            ForeColor = Theme.Muted, BackColor = Color.Transparent,
        };
        dialog.Controls.Add(note);
        var save = new Button { Text = "Stage Traits", DialogResult = DialogResult.OK, Location = new Point(300, 384), Size = new Size(100, 28) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(410, 384), Size = new Size(80, 28) };
        dialog.Controls.Add(save);
        dialog.Controls.Add(cancel);
        dialog.AcceptButton = save;
        dialog.CancelButton = cancel;

        var current = 0;
        var synchronizing = true;
        void StoreCurrent()
        {
            var value = edited[contexts[current].Field];
            for (var bit = 0; bit < TeamTacticsMaps.KnownTraitNames.Count; bit++)
                value = TeamTacticsMaps.SetKnownTrait(value, bit, flags.GetItemChecked(bit));
            edited[contexts[current].Field] = value;
        }
        void LoadCurrent()
        {
            var value = edited[contexts[current].Field];
            for (var bit = 0; bit < TeamTacticsMaps.KnownTraitNames.Count; bit++)
                flags.SetItemChecked(bit, (value & (1 << bit)) != 0);
        }
        contextPicker.SelectedIndexChanged += (_, _) =>
        {
            if (synchronizing || contextPicker.SelectedIndex < 0) return;
            StoreCurrent();
            current = contextPicker.SelectedIndex;
            LoadCurrent();
        };
        contextPicker.SelectedIndex = 0;
        LoadCurrent();
        synchronizing = false;
        Theme.ApplyControlTree(dialog);

        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        StoreCurrent();
        foreach (var context in contexts)
        {
            if (edited[context.Field] == original[context.Field]) continue;
            StageField(TableName, CurrentRecordIndex, context.Field,
                edited[context.Field].ToString(CultureInfo.InvariantCulture), _stagingGrid);
        }
        RefreshTraitSummary();
    }

    private StudioCard BuildKitsCard()
    {
        var card = StudioGroup("Kits", StudioColors.Green);
        card.Dock = DockStyle.Top;
        card.Height = 180;

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent,
            Margin = new Padding(0, StudioSpacing.Small, 0, 0),
        };

        flow.Controls.Add(BuildKitTile("Home", _teamKitHome, "home"));
        flow.Controls.Add(BuildKitTile("Away", _teamKitAway, "away"));
        flow.Controls.Add(BuildKitTile("Third", _teamKitThird, "third"));
        flow.Controls.Add(BuildKitTile("Goalkeeper", _teamKitGk, "gk"));

        card.Controls.Add(flow);
        return card;
    }

    private Control BuildKitTile(string label, PictureBox preview, string variant)
    {
        var tile = new StudioCard
        {
            Width = 180,
            Height = 140,
            Margin = new Padding(0, 0, StudioSpacing.Medium, StudioSpacing.Medium),
        };

        var title = MakeLabel(label, StudioFonts.CardTitle, StudioColors.PrimaryText);
        title.Location = new Point(StudioSpacing.Medium, StudioSpacing.Medium);
        tile.Controls.Add(title);

        preview.Size = new Size(160, 90);
        preview.Location = new Point(StudioSpacing.Medium, 30);
        tile.Controls.Add(preview);

        Action refresh = () => _ = LoadOneKitPreviewAsync(variant, preview);
        LegacyAssetActions.Attach(Services, tile, preview, new Point(StudioSpacing.Medium, 124), refresh);

        return tile;
    }

    private StudioCard BuildActionsCard()
    {
        var card = StudioGroup("Actions", StudioColors.CyanAccent);
        card.Dock = DockStyle.Top;
        card.Height = 90;

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent,
        };

        var import = StudioButton("Import Scraper Squad", 160);
        import.Click += (_, _) => ImportScraperSquad();
        flow.Controls.Add(import);

        var edit = StudioButton("Edit Team Details", 140);
        edit.Click += (_, _) => OpenTeamDetailsEditor();
        flow.Controls.Add(edit);

        card.Controls.Add(flow);
        return card;
    }

    private void OpenTeamDetailsEditor()
    {
        if (CurrentRecordIndex < 0 || _fields.Count == 0) return;
        var editor = new FieldEditorGrid { Dock = DockStyle.Fill };
        editor.SetFields(_fields.Values.OrderBy(v => v.Label).ToList(), ToolTip);
        editor.FieldEdited += (_, change) =>
        {
            if (StageField(TableName, CurrentRecordIndex, change.field, change.value, _stagingGrid))
                ShowRecord(CurrentRecordIndex);
        };
        using var dialog = new Form
        {
            Text = $"Edit {(_heroCard?.TeamName ?? "Team")}",
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(720, 760),
            MinimizeBox = false,
            ShowInTaskbar = false,
            BackColor = Theme.Background,
        };
        dialog.Controls.Add(editor);
        dialog.ShowDialog(this);
    }

    private void AddAudioTab()
    {
        var page = Page("Audio");
        var canvas = Canvas(page);
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
        };

        var presentation = StudioGroup("Selected Team Match Audio", StudioColors.CyanAccent);
        presentation.Width = 480;
        presentation.Height = 220;
        AddBoundFields(presentation, new[]
        {
            ("Sun Anthem Enabled", "hassuncanthem"),
            ("Crowd Region", "crowdregion"),
            ("Viking Clap", "hasvikingclap"),
            ("Team Personality", "personalityid"),
            ("Tifo Enabled", "hastifo")
        }, 15, 30, 175, 150, 26);
        presentation.Controls.Add(new Label
        {
            Text = "Match presentation settings for the selected team.",
            Location = new Point(15, 168), Size = new Size(430, 24),
            Font = LegacyFont, ForeColor = StudioColors.MutedText, BackColor = Color.Transparent
        });
        layout.Controls.Add(presentation);

        layout.Controls.Add(CreateAudioCatalog(
            "Custom Team Callname Catalog", "CustomizableTeamName",
            _teamCallnameSlots, 1));
        layout.Controls.Add(CreateAudioCatalog(
            "Custom Anthem Catalog", "CustomizableAnthemChant",
            _anthemSlots, 1001));
        layout.Controls.Add(CreateAudioCatalog(
            "Custom Chant / Goal Song Catalog", "CustomizableChantPackage",
            _goalSongSlots, 1501));

        canvas.Controls.Add(layout);
        Tabs.TabPages.Add(page);
    }

    private Panel CreateAudioCatalog(string title, string tableName, ListView list, int firstItemId)
    {
        var box = StudioGroup(title, StudioColors.Purple);
        box.Width = 480;
        box.Height = 340;
        list.Location = new Point(10, 34);
        list.Size = new Size(460, 260);
        list.View = View.Details;
        list.FullRowSelect = true;
        list.GridLines = false;
        list.Font = LegacyFont;
        list.Columns.Add("Item", 65);
        list.Columns.Add("Audio Item", 90);
        list.Columns.Add("HAL String", 270);
        list.Columns.Add("CM", 45);
        list.Columns.Add("Clubs", 55);
        box.Controls.Add(list);
        var add = StudioButton("Add", 72);
        add.Location = new Point(10, 302);
        add.Click += (_, _) => AddAudioCatalogEntry(tableName, list, firstItemId);
        box.Controls.Add(add);
        var edit = StudioButton("Edit", 72);
        edit.Location = new Point(88, 302);
        edit.Click += (_, _) => EditAudioCatalogEntry(tableName, list);
        box.Controls.Add(edit);
        var remove = StudioButton("Remove", 72);
        remove.Location = new Point(166, 302);
        remove.Click += (_, _) => RemoveAudioCatalogEntry(tableName, list);
        box.Controls.Add(remove);
        return box;
    }

    private void AddRosterTab()
    {
        var page = Page("Roster");
        var canvas = Canvas(page);
        canvas.AutoScroll = false;

        var workspace = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BackColor = StudioColors.Divider,
            SplitterWidth = 6,
        };
        workspace.Panel1.BackColor = StudioColors.AppBackground;
        workspace.Panel2.BackColor = StudioColors.AppBackground;
        workspace.SizeChanged += (_, _) =>
        {
            if (workspace.Width > 720)
            {
                workspace.Panel2MinSize = 310;
                workspace.SplitterDistance = Math.Clamp((int)(workspace.Width * 0.6), 390, workspace.Width - 320);
            }
        };
        canvas.Controls.Add(workspace);

        // Left: formation board
        var pitchPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(StudioSpacing.Medium),
            BackColor = StudioColors.AppBackground,
        };
        workspace.Panel1.Controls.Add(pitchPanel);

        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 34,
            BackColor = StudioColors.AppBackground,
        };
        pitchPanel.Controls.Add(topBar);

        var formationLabel = MakeLabel("Formation", StudioFonts.DataValue, StudioColors.PrimaryText);
        formationLabel.Location = new Point(StudioSpacing.Medium, 8);
        topBar.Controls.Add(formationLabel);

        _formationView.Size = new Size(180, 24);
        _formationView.Location = new Point(90, 4);
        _formationView.Font = LegacyFont;
        _formationView.DropDownHeight = 340;
        Theme.ApplyCombo(_formationView);
        _formationView.SelectedIndexChanged += (_, _) =>
        {
            if (!_syncFormationView && _formationView.SelectedItem is FormationChoice choice)
                SelectTeamFormation(choice);
        };
        topBar.Controls.Add(_formationView);

        _formationStatus = MakeLabel("", StudioFonts.DataLabel, StudioColors.MutedText, false);
        _formationStatus.Location = new Point(280, 8);
        _formationStatus.Size = new Size(300, 20);
        topBar.Controls.Add(_formationStatus);

        _formationBoard = new FormationBoard
        {
            Dock = DockStyle.Fill,
            AllowDrop = true,
        };
        _formationBoard.PlayerDropped += FormationBoard_PlayerDropped;
        _formationBoard.SlotClicked += FormationBoard_SlotClicked;
        pitchPanel.Controls.Add(_formationBoard);

        // Right: matchday roles + roster list
        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(StudioSpacing.Medium),
        };
        workspace.Panel2.Controls.Add(rightPanel);

        var rolesCard = BuildMatchdayRolesCard();
        rolesCard.Dock = DockStyle.Top;
        rolesCard.Height = 180;
        rightPanel.Controls.Add(rolesCard);

        var toolsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = StudioColors.AppBackground,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, StudioSpacing.Small, 0, 0),
        };
        var btnTransfer = StudioButton("Transfer", 80);
        btnTransfer.Click += (_, _) => OpenTransferDialog();
        toolsPanel.Controls.Add(btnTransfer);
        var btnLoan = StudioButton("Loan", 80);
        btnLoan.Click += (_, _) => ShowLoanDetails();
        toolsPanel.Controls.Add(btnLoan);
        var btnFind = StudioButton("Find", 80);
        btnFind.Click += (_, _) => FindSelectedPlayer();
        toolsPanel.Controls.Add(btnFind);
        rightPanel.Controls.Add(toolsPanel);

        _squadCountLabel = MakeLabel("Squad", StudioFonts.CardTitle, StudioColors.PrimaryText);
        _squadCountLabel.Dock = DockStyle.Top;
        _squadCountLabel.Height = 24;
        rightPanel.Controls.Add(_squadCountLabel);

        _rosterScrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            AutoScroll = true,
            Padding = new Padding(0, StudioSpacing.Small, 0, 0),
        };
        rightPanel.Controls.Add(_rosterScrollPanel);

        _startingXiSection = CreateRosterSection("STARTING XI");
        _subsSection = CreateRosterSection("SUBSTITUTES");
        _reservesSection = CreateRosterSection("RESERVES");
        _rosterScrollPanel.Controls.Add(_startingXiSection);
        _rosterScrollPanel.Controls.Add(_subsSection);
        _rosterScrollPanel.Controls.Add(_reservesSection);

        CreateLineupSlots();

        Tabs.TabPages.Add(page);
    }

    private StudioCard BuildMatchdayRolesCard()
    {
        var card = StudioGroup("Matchday Roles", StudioColors.CyanAccent);
        AddPlayerReferencePickers(card, new[] {
            ("Captain", "captainid"), ("Left Corner", "leftcornerkicktakerid"),
            ("Right Corner", "rightcornerkicktakerid"), ("Penalty", "penaltytakerid"),
            ("Free Kicks", "freekicktakerid")
        }, 8, 34, pickerX: 104, pickerWidth: 220);
        return card;
    }

    private static Panel CreateRosterSection(string title)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            BackColor = StudioColors.AppBackground,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, StudioSpacing.Small),
        };
        var header = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 24,
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.CyanAccent,
            BackColor = Color.Transparent,
        };
        panel.Controls.Add(header);
        return panel;
    }

    private void FormationBoard_SlotClicked(object? sender, FormationSlotEventArgs e)
    {
        if (_formationBoard == null) return;
        _formationBoard.SelectedSlot = e.Slot;
        if (e.Slot != null)
        {
            foreach (var row in _rosterRows.Where(r => r.PlayerId == e.Slot.PlayerId))
                SelectRosterRow(row);
        }
    }

    private void FormationBoard_PlayerDropped(object? sender, FormationDropEventArgs e)
    {
        if (e.PlayerId <= 0 || _lineupSlots.Count == 0) return;
        var slotIndex = e.TargetSlot?.SlotIndex ?? NearestSlotIndex(e.DropLocation);
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
        foreach (var slot in _lineupSlots.Where(s => s.PlayerId == e.PlayerId && s != _lineupSlots[slotIndex]).ToList())
        {
            if (StageLineupField(slot, -1))
                slot.PlayerId = 0;
        }
        if (StageLineupField(_lineupSlots[slotIndex], e.PlayerId))
            _lineupSlots[slotIndex].PlayerId = e.PlayerId;
        RenderLineup();
    }

    private int NearestSlotIndex(Point point)
    {
        if (_formationBoard == null) return -1;
        var best = -1;
        var distance = double.MaxValue;
        for (var i = 0; i < _lineupSlots.Count; i++)
        {
            var x = _lineupSlots[i].Visual.RelativeX * _formationBoard.Width;
            var y = _lineupSlots[i].Visual.RelativeY * _formationBoard.Height;
            var d = (point.X - x) * (point.X - x) + (point.Y - y) * (point.Y - y);
            if (d < distance) { distance = d; best = i; }
        }
        return best;
    }
    private Label? _squadCountLabel;

    private void FindSelectedPlayer()
    {
        if (_selectedRosterPlayerId <= 0) return;
        var recordIndex = FindTableRow("players", "playerid", _selectedRosterPlayerId);
        if (recordIndex >= 0) Services.RequestRecordNavigation("players", recordIndex);
    }

    private sealed class SlotState
    {
        public required string PlayerField { get; init; }
        public int PlayerId { get; set; }
        public int LoadedMinifacePlayerId { get; set; }
        public int AppliedMinifacePlayerId { get; set; }
        public string ExpectedPosition { get; set; } = string.Empty;
        public Point FormationPoint { get; set; }
        public FormationSlot Visual { get; } = new();
    }

    private sealed record FormationChoice(int RecordIndex, int FormationId, int LayoutId, string Name, bool IsGeneric)
    {
        public override string ToString() => Name;
    }

    private void CreateLineupSlots()
    {
        _lineupSlots.Clear();
        _formationBoard?.ClearSlots();
        foreach (var i in Enumerable.Range(0, 11))
        {
            var slot = new SlotState { PlayerField = $"playerid{i}" };
            slot.Visual.SlotIndex = i;
            _lineupSlots.Add(slot);
            _formationBoard?.AddSlot(slot.Visual);
        }
    }

    private bool StageLineupField(SlotState slot, int playerId)
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

    /// <summary>
    /// Some FC26 team sheets intentionally contain only a goalkeeper or are
    /// absent entirely. The roster still has the authoritative teamplayerlinks
    /// mapping, so fill only empty display slots from it. This is visual-only:
    /// no database field is staged until the user explicitly drags a player.
    /// Must be called after the formation layout has been applied so each slot's
    /// ExpectedPosition is known and position-aware matching can succeed.
    /// </summary>
    private void PopulateVisualLineupFallback(IReadOnlyList<TeamRosterItem> roster)
    {
        var used = _lineupSlots.Where(slot => slot.PlayerId > 0).Select(slot => slot.PlayerId).ToHashSet();
        var candidates = roster
            .Where(player => player.PlayerId > 0 && !used.Contains(player.PlayerId))
            .OrderBy(player => PositionOrder(player.Position))
            .ThenByDescending(player => Parse(player.Overall))
            .ToList();
        foreach (var slot in _lineupSlots.Where(slot => slot.PlayerId <= 0))
        {
            var match = candidates.FirstOrDefault(player =>
                string.Equals(player.Position, slot.ExpectedPosition, StringComparison.OrdinalIgnoreCase))
                ?? candidates.FirstOrDefault(player => IsCompatibleLineupPosition(player.Position, slot.ExpectedPosition))
                ?? candidates.FirstOrDefault();
            if (match == null) break;
            slot.PlayerId = match.PlayerId;
            candidates.Remove(match);
        }
    }

    private static bool IsCompatibleLineupPosition(string playerPosition, string slotPosition) =>
        (IsDefender(playerPosition) && IsDefender(slotPosition)) ||
        (playerPosition.ToUpperInvariant() is "CDM" or "CM" or "CAM" or "LM" or "RM") &&
        (slotPosition.ToUpperInvariant() is "CDM" or "CM" or "CAM" or "LM" or "RM") ||
        (playerPosition.ToUpperInvariant() is "ST" or "CF" or "LW" or "RW") &&
        (slotPosition.ToUpperInvariant() is "ST" or "CF" or "LW" or "RW");

    private void ApplyFormationLayout(FormationChoice choice, string? status = null)
    {
        var table = Services.Session.GetTable("formations");
        var record = table == null ? null : Services.Session.GetRecord("formations", choice.RecordIndex);
        if (table == null || record == null || _formationBoard == null) return;
        _activeFormationChoice = choice;
        for (var i = 0; i < _lineupSlots.Count; i++)
        {
            var x = ReadFormationOffset(table, record, $"offset{i}x", _formationBoard.Width);
            var y = ReadFormationOffset(table, record, $"offset{i}y", _formationBoard.Height);
            var positionColumn = Col(table, $"position{i}");
            var slot = _lineupSlots[i];
            slot.FormationPoint = new Point(x, _formationBoard.Height - y);
            slot.ExpectedPosition = positionColumn >= 0 ? NameResolverService.PositionLabel(Parse(record.Get(positionColumn))) : "Not stored";
        }
        ApplyGoalkeeperVisualClearance();
        ArrangeLineupInTacticalLanes();
        if (_formationStatus != null)
            _formationStatus.Text = status ?? choice.Name;
        _teamFormationLabel.Text = choice.Name;
        RenderLineup();
    }

    private void ArrangeLineupInTacticalLanes()
    {
        if (_formationBoard == null) return;
        foreach (var slot in _lineupSlots)
            UpdateSlotPosition(slot);
        _formationBoard.Invalidate();
    }

    private void UpdateSlotPosition(SlotState slot)
    {
        if (_formationBoard == null || _formationBoard.Width <= 20 || _formationBoard.Height <= 20) return;
        var size = Math.Min(64, Math.Max(48, _formationBoard.Width / 16));
        var half = size / 2 + 4;
        var rx = Math.Clamp(slot.FormationPoint.X / (float)_formationBoard.Width, half / (float)_formationBoard.Width, 1f - half / (float)_formationBoard.Width);
        var ry = Math.Clamp(slot.FormationPoint.Y / (float)_formationBoard.Height, half / (float)_formationBoard.Height, 1f - half / (float)_formationBoard.Height);
        slot.Visual.RelativeX = rx;
        slot.Visual.RelativeY = ry;
    }

    private void ApplyGoalkeeperVisualClearance()
    {
        if (_formationBoard == null) return;
        var defenders = _lineupSlots.Where(slot => IsDefender(slot.ExpectedPosition)).ToList();
        if (defenders.Count == 0) return;

        var lastDefenderY = defenders.Max(slot => slot.FormationPoint.Y);
        foreach (var goalkeeper in _lineupSlots.Where(slot => string.Equals(slot.ExpectedPosition, "GK", StringComparison.OrdinalIgnoreCase)))
        {
            var bottomCentre = _formationBoard.Height - 42;
            var safeY = Math.Min(bottomCentre, Math.Max(goalkeeper.FormationPoint.Y, lastDefenderY + 112));
            goalkeeper.FormationPoint = new Point(goalkeeper.FormationPoint.X, safeY);
        }
    }

    private static bool IsDefender(string position) => position.ToUpperInvariant() is "LB" or "LWB" or "LCB" or "CB" or "RCB" or "RB" or "RWB";

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
            var relativeColumn = Col(formations, "relativeformationid");
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
                if (owner == -1 && !string.IsNullOrWhiteSpace(name))
                {
                    genericChoices.Add(new FormationChoice(row, formationId, formationId,
                        Fc26FormationCatalog.DisplayName(formationId, name), IsGeneric: true));
                }
                else if (owner == teamId)
                {
                    // A team can have more than one linked style row in custom
                    // databases. Keep the first stable link instead of letting
                    // the last row silently replace it during enumeration.
                    if (_activeTeamFormationRow < 0) _activeTeamFormationRow = row;
                    var layoutId = relativeColumn >= 0 ? Parse(record.Get(relativeColumn)) : formationId;
                    teamChoices.Add(new FormationChoice(row, formationId, layoutId,
                        Fc26FormationCatalog.DisplayName(layoutId,
                            string.IsNullOrWhiteSpace(name) ? $"Team formation #{row + 1}" : name), IsGeneric: false));
                }
            }
        }
        var choices = genericChoices
            .GroupBy(c => c.FormationId)
            .Select(group => group.First())
            .OrderBy(c => Fc26FormationCatalog.SortOrder(c.FormationId))
            .ToList();
        if (choices.Count == 0) choices = teamChoices;
        var selected = _activeTeamFormationRow >= 0 ? teamChoices.FirstOrDefault() : null;
        if (selected != null)
        {
            var matchingGeneric = choices.FirstOrDefault(c => c.FormationId == selected.LayoutId);
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
            _activeFormationChoice = null;
            if (_formationStatus != null)
                _formationStatus.Text = "No formation is linked to this team.";
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
                slot.Visual.PlayerName = DisplayLineupName(player.Name);
                slot.Visual.Position = position;
                slot.Visual.Overall = int.TryParse(player.Overall, out var ovr) ? ovr : 0;
                QueueLineupMiniface(slot, slot.PlayerId);
            }
            else if (slot.PlayerId > 0)
            {
                var playerName = Services.Resolver?.PlayerNameByPlayerId(slot.PlayerId) ?? $"Player {slot.PlayerId}";
                slot.Visual.PlayerName = DisplayLineupName(playerName);
                slot.Visual.Position = DisplayLineupPosition(string.Empty, slot.ExpectedPosition);
                slot.Visual.Overall = 0;
                QueueLineupMiniface(slot, slot.PlayerId);
            }
            else
            {
                slot.Visual.PlayerName = "Empty slot";
                slot.Visual.Position = DisplayLineupPosition(string.Empty, slot.ExpectedPosition);
                slot.Visual.Overall = 0;
                ClearLineupMiniface(slot);
            }
        }
        if (_activeFormationChoice != null)
            ArrangeLineupInTacticalLanes();
        _formationBoard?.Invalidate();
    }

    internal void RefreshFormationLayoutForAudit()
    {
        if (_formationView.SelectedItem is FormationChoice choice)
            ApplyFormationLayout(choice);
    }

    internal string FormationLayoutSnapshot() =>
        $"active={_activeFormationChoice?.Name ?? "<null>"}; selected={_formationView.SelectedItem}; " +
        string.Join(" | ", _lineupSlots.Select((slot, index) =>
            $"{index}:{slot.FormationPoint.X},{slot.FormationPoint.Y}->{slot.Visual.RelativeX:F2},{slot.Visual.RelativeY:F2}"));

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

    private void QueueLineupMiniface(SlotState slot, int playerId)
    {
        if (playerId <= 0) return;
        if (slot.LoadedMinifacePlayerId == playerId && slot.AppliedMinifacePlayerId == playerId) return;

        if (slot.LoadedMinifacePlayerId != playerId)
        {
            var old = slot.Visual.Miniface;
            slot.Visual.Miniface = null;
            slot.AppliedMinifacePlayerId = 0;
            old?.Dispose();
        }
        slot.LoadedMinifacePlayerId = playerId;
        _ = Task.Run(async () => await LoadPlayerMinifaceAsync(playerId, 60))
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

    private static void ClearLineupMiniface(SlotState slot)
    {
        slot.LoadedMinifacePlayerId = 0;
        slot.AppliedMinifacePlayerId = 0;
        var old = slot.Visual.Miniface;
        slot.Visual.Miniface = null;
        old?.Dispose();
    }

    private static void SetLineupMiniface(SlotState slot, int playerId, Image image)
    {
        if (slot.LoadedMinifacePlayerId != playerId)
        {
            image.Dispose();
            return;
        }
        var old = slot.Visual.Miniface;
        slot.Visual.Miniface = CreateCircularMiniface(image, 52);
        slot.AppliedMinifacePlayerId = playerId;
        image.Dispose();
        old?.Dispose();
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
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
        };

        var sources = StudioGroup("Team Adboard Sources", StudioColors.CyanAccent);
        sources.Width = 520;
        sources.Height = 540;
        _adboardSources.Location = new Point(10, 34);
        _adboardSources.Size = new Size(500, 500);
        _adboardSources.View = View.Details;
        _adboardSources.FullRowSelect = true;
        _adboardSources.GridLines = false;
        _adboardSources.Font = LegacyFont;
        _adboardSources.BackColor = StudioColors.InputBackground;
        _adboardSources.ForeColor = StudioColors.PrimaryText;
        _adboardSources.Columns.Add("Sponsor", 200);
        _adboardSources.Columns.Add("Sponsor ID", 90);
        _adboardSources.Columns.Add("Dynamic Image", 115);
        _adboardSources.Columns.Add("Approved", 80);
        _adboardSources.SelectedIndexChanged += (_, _) => ShowSelectedAdboard();
        sources.Controls.Add(_adboardSources);
        layout.Controls.Add(sources);

        var preview = StudioGroup("Adboard / Dynamic Sponsor Preview", StudioColors.Purple);
        preview.Width = 560;
        preview.Height = 420;
        _adboardPreview.Location = new Point(10, 34);
        _adboardPreview.Size = new Size(540, 300);
        _adboardPreview.BackColor = StudioColors.InputBackground;
        _adboardPreview.BorderStyle = BorderStyle.None;
        _adboardPreview.SizeMode = PictureBoxSizeMode.Zoom;
        preview.Controls.Add(_adboardPreview);
        _adboardCaption.Location = new Point(10, 340);
        _adboardCaption.Size = new Size(540, 40);
        _adboardCaption.Font = LegacyFont;
        _adboardCaption.TextAlign = ContentAlignment.MiddleCenter;
        _adboardCaption.ForeColor = StudioColors.MutedText;
        _adboardCaption.BackColor = Color.Transparent;
        _adboardCaption.Text = "Select a linked sponsor or adboard source";
        preview.Controls.Add(_adboardCaption);
        LegacyAssetActions.Attach(Services, preview, _adboardPreview, new Point(10, 384), ShowSelectedAdboard);
        layout.Controls.Add(preview);

        canvas.Controls.Add(layout);
        Tabs.TabPages.Add(page);
    }

    private void AddSponsorsTab()
    {
        var page = Page("Sponsors");
        var canvas = Canvas(page);
        var links = StudioGroup("Team Sponsor Links", StudioColors.Green);
        links.Dock = DockStyle.Fill;

        _teamSponsors.Location = new Point(12, 34);
        _teamSponsors.Size = new Size(520, 420);
        _teamSponsors.View = View.Details;
        _teamSponsors.FullRowSelect = true;
        _teamSponsors.GridLines = false;
        _teamSponsors.Font = LegacyFont;
        _teamSponsors.BackColor = StudioColors.InputBackground;
        _teamSponsors.ForeColor = StudioColors.PrimaryText;
        _teamSponsors.Columns.Add("Sponsor", 200);
        _teamSponsors.Columns.Add("Approved", 90);
        _teamSponsors.Columns.Add("Dynamic Image", 130);
        _teamSponsors.Columns.Add("Link Key", 100);
        _teamSponsors.SelectedIndexChanged += (_, _) => ShowSelectedSponsor();
        links.Controls.Add(_teamSponsors);

        _sponsorPreview.Location = new Point(550, 34);
        _sponsorPreview.Size = new Size(400, 220);
        _sponsorPreview.BackColor = StudioColors.InputBackground;
        _sponsorPreview.BorderStyle = BorderStyle.None;
        _sponsorPreview.SizeMode = PictureBoxSizeMode.Zoom;
        links.Controls.Add(_sponsorPreview);

        _sponsorPreviewCaption.Location = new Point(550, 260);
        _sponsorPreviewCaption.Size = new Size(400, 40);
        _sponsorPreviewCaption.Font = LegacyFont;
        _sponsorPreviewCaption.TextAlign = ContentAlignment.MiddleCenter;
        _sponsorPreviewCaption.ForeColor = StudioColors.MutedText;
        _sponsorPreviewCaption.BackColor = Color.Transparent;
        _sponsorPreviewCaption.Text = "Select a sponsor link to preview its dynamic image";
        links.Controls.Add(_sponsorPreviewCaption);
        LegacyAssetActions.Attach(Services, links, _sponsorPreview, new Point(550, 310), ShowSelectedSponsor);

        links.Controls.Add(new Label
        {
            Text = "Select a sponsor relationship to preview its linked dynamic image.",
            Location = new Point(12, 470), Size = new Size(900, 28),
            Font = LegacyFont, ForeColor = StudioColors.MutedText, BackColor = Color.Transparent
        });
        canvas.Controls.Add(links);
        Tabs.TabPages.Add(page);
    }

    private void AddFlagsTab()
    {
        var page = Page("Flags");
        var canvas = Canvas(page);
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
        };

        var texture = StudioGroup("Team Flags", StudioColors.CyanAccent);
        texture.Width = 540;
        texture.Height = 360;
        _teamFlagPreview.Location = new Point(10, 34);
        _teamFlagPreview.Size = new Size(520, 256);
        _teamFlagPreview.BackColor = StudioColors.InputBackground;
        _teamFlagPreview.BorderStyle = BorderStyle.None;
        _teamFlagPreview.SizeMode = PictureBoxSizeMode.Zoom;
        texture.Controls.Add(_teamFlagPreview);
        LegacyAssetActions.Attach(Services, texture, _teamFlagPreview, new Point(10, 296), RefreshTeamFlags);
        _teamFlagCaption.Location = new Point(10, 324);
        _teamFlagCaption.Size = new Size(520, 28);
        _teamFlagCaption.Font = LegacyFont;
        _teamFlagCaption.TextAlign = ContentAlignment.MiddleCenter;
        _teamFlagCaption.ForeColor = StudioColors.MutedText;
        _teamFlagCaption.BackColor = Color.Transparent;
        texture.Controls.Add(_teamFlagCaption);
        layout.Controls.Add(texture);

        var flag = StudioGroup("Nation Flag", StudioColors.Purple);
        flag.Width = 540;
        flag.Height = 360;
        _nationFlagPreview.Location = new Point(10, 34);
        _nationFlagPreview.Size = new Size(520, 256);
        _nationFlagPreview.BackColor = StudioColors.InputBackground;
        _nationFlagPreview.BorderStyle = BorderStyle.None;
        _nationFlagPreview.SizeMode = PictureBoxSizeMode.Zoom;
        flag.Controls.Add(_nationFlagPreview);
        flag.Controls.Add(new Label { Text = "Flag n.", Location = new Point(12, 296), AutoSize = true, Font = LegacyFont, ForeColor = StudioColors.MutedText, BackColor = Color.Transparent });
        _nationFlagCaption.Location = new Point(70, 296);
        _nationFlagCaption.Size = new Size(460, 26);
        _nationFlagCaption.Font = LegacyFont;
        _nationFlagCaption.TextAlign = ContentAlignment.MiddleLeft;
        _nationFlagCaption.ForeColor = StudioColors.MutedText;
        _nationFlagCaption.BackColor = Color.Transparent;
        flag.Controls.Add(_nationFlagCaption);
        layout.Controls.Add(flag);

        canvas.Controls.Add(layout);
        Tabs.TabPages.Add(page);
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
        var old = preview.Image;
        preview.Image = null;
        old?.Dispose();
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                using var source = Image.FromFile(path);
                preview.Image = new Bitmap(source);
            }
        }
        catch (Exception ex) { Program.Log("Kit preview failed: " + ex.Message); }
    }

    protected override void ShowRecord(int recordIndex)
    {
        var table = Services.Session.GetTable(TableName);
        var record = table == null ? null : Services.Session.GetRecord(TableName, recordIndex);
        if (table == null || record == null)
        {
            if (_heroCard != null) _heroCard.TeamName = "Team unavailable";
            return;
        }
        var name = record.Get(Col(table, "teamname"));
        var id = record.Get(Col(table, "teamid"));
        int.TryParse(id, out var crestTeamId);

        _fields.Clear();
        foreach (var field in Services.RequireData().GetFields(TableName, recordIndex, LabelMaps.Teams))
            _fields[field.FieldName] = field;
        UpdateTeamColours();
        var countryColumn = Col(table, "countryid");
        try
        {
            ShowTeamFlags(crestTeamId, countryColumn >= 0 ? Parse(record.Get(countryColumn)) : 0);
        }
        catch (Exception ex) { Program.Log($"[CM26] Team flag preview failed: {ex.Message}"); }

        _activeTeamPreviewId = crestTeamId;

        if (_heroCard != null)
        {
            _heroCard.TeamName = name ?? string.Empty;
            _heroCard.LeagueNation = $"{ResolveLinkedValue("leagueid", crestTeamId)} · {ResolveLinkedValue("countryid", crestTeamId)}";
            _heroCard.Overall = int.TryParse(record.Get(Col(table, "overallrating")), out var ovr) ? ovr : 0;
            _heroCard.Attack = int.TryParse(record.Get(Col(table, "attackrating")), out var att) ? att : 0;
            _heroCard.Midfield = int.TryParse(record.Get(Col(table, "midfieldrating")), out var mid) ? mid : 0;
            _heroCard.Defence = int.TryParse(record.Get(Col(table, "defenserating")), out var def) ? def : 0;
            _heroCard.FoundedText = $"Founded: {record.Get(Col(table, "foundationyear")) ?? "—"}";
            var financial = TeamFinancialFieldResolver.Resolve(table);
            _heroCard.FinancialFieldLabel = financial?.DisplayName ?? "Financial value";
            _heroCard.FinancialEditorEnabled = financial is not null;
            _heroCard.WorthText = financial?.IsTransferBudget == true
                ? $"Club worth: {record.Get(Col(table, "clubworth")) ?? "—"}"
                : "Career budget: not stored in teams DB";
            var financialValue = 0L;
            if (financial is not null)
            {
                var financialColumn = Col(table, financial.FieldName);
                if (financialColumn >= 0 && long.TryParse(record.Get(financialColumn), out var parsed))
                    financialValue = parsed;
            }
            _heroCard.TransferBudget = financialValue;

            var oldCrest = _heroCard.Crest;
            _heroCard.Crest = null;
            oldCrest?.Dispose();
            LoadProfileCrest(crestTeamId);
            _heroCard.Crest = _teamCrestPreview.Image;
        }

        _teamStadiumLabel.Text = ResolveLinkedValue("stadiumid", crestTeamId);
        _teamManagerName.Text = ResolveLinkedValue("managerid", crestTeamId);
        _teamManagerNation.Text = "";
        _teamRivalLabel.Text = "—";
        _teamFoundationLabel.Text = $"Founded: {record.Get(Col(table, "foundationyear")) ?? "—"}";
        _teamWorthLabel.Text = $"Club worth: {Fc26ClubProfile.FormatClubWorth(record.Get(Col(table, "clubworth")))}";
        _teamFanbaseLabel.Text = $"Fanbase devotion: {Fc26ClubProfile.RatingLabel(record.Get(Col(table, "popularity")))}";
        _teamYouthLabel.Text = $"Youth facilities: {Fc26ClubProfile.RatingLabel(record.Get(Col(table, "youthdevelopment")))}";
        _teamFinanceLabel.Text = $"Financial stability: {Fc26ClubProfile.RatingLabel(record.Get(Col(table, "profitability")))}";
        _teamHonoursLabel.Text = $"Honours: League {record.Get(Col(table, "leaguetitles")) ?? "—"}  •  Cups {record.Get(Col(table, "domesticcups")) ?? "—"}\nChampions League {record.Get(Col(table, "uefa_cl_wins")) ?? "—"}";
        RefreshTacticsCard(crestTeamId);

        ReplacePreviewImage(_teamKitHome, null);
        ReplacePreviewImage(_teamKitAway, null);
        ReplacePreviewImage(_teamKitThird, null);
        ReplacePreviewImage(_teamKitGk, null);
        SetKitTargets(crestTeamId);
        _ = LoadTeamKitPreviewsAfterIdentityAsync(crestTeamId);

        LoadTeamStadiumPreview(crestTeamId, Parse(record.Get(Col(table, "stadiumid"))));
        LoadTeamManagerPreview(crestTeamId);
        SetAssetTarget(_teamStadiumImg, $"data/ui/imgAssets/stadium/stadium_{Parse(record.Get(Col(table, "stadiumid")))}_0.dds", 512, 256);
        SetManagerAssetTarget(crestTeamId);

        _rosterMinifaces.Images.Clear();
        _pendingRosterMinifaces.Clear();
        _teamPlayers.Items.Clear();
        ClearRosterRows();
        try
        {
            var roster = Services.RequireData().GetTeamRoster(int.TryParse(id, out var teamId) ? teamId : 0);
            LoadLineup(teamId, roster);
            SelectFormationLayout(teamId);
            if (_activeFormationChoice != null)
            {
                PopulateVisualLineupFallback(roster);
                RenderLineup();
            }
            _formationBoard?.Invalidate(true);
            PopulatePlayerReferencePickers(roster);

            var lineupIds = new HashSet<int>();
            var subIds = new HashSet<int>();
            foreach (var slot in _lineupSlots)
                if (slot.PlayerId > 0) lineupIds.Add(slot.PlayerId);
            foreach (ListViewItem sub in _matchdayBench.Items)
                if (sub.Tag is int subId && subId > 0) subIds.Add(subId);

            var startingXi = roster.Where(p => lineupIds.Contains(p.PlayerId)).ToList();
            var subs = roster.Where(p => subIds.Contains(p.PlayerId) && !lineupIds.Contains(p.PlayerId)).ToList();
            var reserves = roster.Where(p => !lineupIds.Contains(p.PlayerId) && !subIds.Contains(p.PlayerId)).ToList();

            var potentials = LoadPotentials(roster);
            BuildRosterSection(_startingXiSection, "STARTING XI", startingXi, potentials, "XI");
            BuildRosterSection(_subsSection, "SUBSTITUTES", subs, potentials, "SUB");
            BuildRosterSection(_reservesSection, "RESERVES", reserves, potentials, "RES");

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

    private Dictionary<int, int> LoadPotentials(IReadOnlyList<TeamRosterItem> roster)
    {
        var result = new Dictionary<int, int>();
        var players = Services.Session.GetTable("players");
        if (players == null) return result;
        var playerIdCol = Col(players, "playerid");
        var potentialCol = Col(players, "potential");
        if (playerIdCol < 0 || potentialCol < 0) return result;
        var ids = roster.Select(r => r.PlayerId).ToHashSet();
        for (var row = 0; row < players.RowCount; row++)
        {
            var record = Services.Session.GetRecord("players", row);
            if (record == null) continue;
            var pid = Parse(record.Get(playerIdCol));
            if (ids.Contains(pid) && int.TryParse(record.Get(potentialCol), out var pot))
                result[pid] = pot;
        }
        return result;
    }

    private void ClearRosterRows()
    {
        foreach (var row in _rosterRows)
        {
            row.Miniface?.Dispose();
            row.Dispose();
        }
        _rosterRows.Clear();
        _startingXiSection?.Controls.Clear();
        _subsSection?.Controls.Clear();
        _reservesSection?.Controls.Clear();
        _startingXiSection?.Controls.Add(CreateSectionHeader("STARTING XI"));
        _subsSection?.Controls.Add(CreateSectionHeader("SUBSTITUTES"));
        _reservesSection?.Controls.Add(CreateSectionHeader("RESERVES"));
    }

    private static Label CreateSectionHeader(string title)
    {
        return new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 24,
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.CyanAccent,
            BackColor = Color.Transparent,
        };
    }

    private void BuildRosterSection(Panel? section, string title, List<TeamRosterItem> players, Dictionary<int, int> potentials, string role)
    {
        if (section == null) return;
        section.Controls.Clear();
        section.Controls.Add(CreateSectionHeader($"{title} ({players.Count})"));
        foreach (var player in players.OrderBy(p => PositionOrder(p.Position)))
        {
            var ovr = int.TryParse(player.Overall, out var o) ? o : 0;
            var row = new RosterPlayerRow
            {
                PlayerId = player.PlayerId,
                PlayerName = player.Name,
                Position = player.Position,
                Overall = ovr,
                Potential = potentials.TryGetValue(player.PlayerId, out var pot) ? pot : ovr,
                RoleText = role,
                Dock = DockStyle.Top,
            };
            row.RowClicked += RosterRow_Clicked;
            row.DoubleClick += (_, _) => OpenSelectedRosterPlayer();
            _rosterRows.Add(row);
            section.Controls.Add(row);
            QueueRowMiniface(row, player.PlayerId);
        }
    }

    private void RosterRow_Clicked(object? sender, EventArgs e)
    {
        if (sender is not RosterPlayerRow row) return;
        SelectRosterRow(row);
    }

    private void SelectRosterRow(RosterPlayerRow row)
    {
        foreach (var r in _rosterRows) r.IsSelected = false;
        row.IsSelected = true;
        _selectedRosterPlayerId = row.PlayerId;
        if (_formationBoard != null)
        {
            var slot = _lineupSlots.FirstOrDefault(s => s.PlayerId == row.PlayerId);
            _formationBoard.SelectedSlot = slot?.Visual;
        }
    }

    private void QueueRowMiniface(RosterPlayerRow row, int playerId)
    {
        if (playerId <= 0) return;
        _ = Task.Run(async () => await LoadPlayerMinifaceAsync(playerId, 32)).ContinueWith(task =>
        {
            if (IsDisposed || task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
            var image = task.Result;
            var circular = CreateCircularMiniface(image, 32);
            image.Dispose();
            if (IsDisposed) { circular.Dispose(); return; }
            var old = row.Miniface;
            row.Miniface = circular;
            old?.Dispose();
            row.Invalidate();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void SetKitTargets(int teamId)
    {
        SetAssetTarget(_teamKitHome, $"data/ui/imgAssets/teamkits/team{teamId}_home.dds", 512, 512);
        SetAssetTarget(_teamKitAway, $"data/ui/imgAssets/teamkits/team{teamId}_away.dds", 512, 512);
        SetAssetTarget(_teamKitThird, $"data/ui/imgAssets/teamkits/team{teamId}_third.dds", 512, 512);
        SetAssetTarget(_teamKitGk, $"data/ui/imgAssets/teamkits/team{teamId}_gk.dds", 512, 512);
    }

    private static void SetAssetTarget(PictureBox picture, string legacyPath, int width, int height)
    {
        LegacyAssetActions.SetTarget(picture, new LegacyAssetEditTarget(legacyPath, width, height));
    }

    private void SetManagerAssetTarget(int teamId)
    {
        var managers = Services.Session.GetTable("manager");
        if (managers == null) return;
        var teamColumn = Col(managers, "teamid");
        var managerIdColumn = Col(managers, "managerid");
        var headColumn = Col(managers, "headassetid");
        for (var row = 0; row < managers.RowCount; row++)
        {
            var record = Services.Session.GetRecord("manager", row);
            if (record == null || Parse(record.Get(teamColumn)) != teamId) continue;
            var managerId = Parse(record.Get(managerIdColumn));
            var portraitId = headColumn >= 0 ? Parse(record.Get(headColumn)) : 0;
            if (portraitId <= 0) portraitId = managerId;
            SetAssetTarget(_teamManagerImg, $"data/ui/imgAssets/heads_staff/heads_staff_{portraitId}.dds", 256, 256);
            return;
        }
    }

    private async Task LoadOneKitPreviewAsync(string variant, PictureBox preview)
    {
        if (_activeTeamPreviewId <= 0) return;
        var targetPath = $"data/ui/imgAssets/teamkits/team{_activeTeamPreviewId}_{variant}.dds";
        var replacement = LegacyAssetActions.Replacement(Services, targetPath);
        if (!string.IsNullOrWhiteSpace(replacement))
        {
            FrostbitePreviewLoader.LoadLegacyUiAsset(preview, Services, replacement, targetPath, (img, _) =>
            {
                if (IsDisposed) { img?.Dispose(); return; }
                ReplacePreviewImage(preview, img);
            });
            return;
        }
        if (!Services.FrostbiteAssets.IsAvailable) return;
        try
        {
            var query = $"_{_activeTeamPreviewId}/{variant}_";
            var selected = (await Task.Run(() => Services.FrostbiteAssets.SearchAssets(query, "Res", 100)))
                .Where(match => match.ResType == 0x6BDE20BA && match.Name.EndsWith("_color", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(match => KitTextureScore(match.Name))
                .FirstOrDefault();
            if (selected == null) return;
            var path = await Task.Run(() => Services.FrostbiteAssets.ExportTexture(selected.Name));
            if (string.IsNullOrWhiteSpace(path)) return;
            var image = await Task.Run(() => Services.Textures.CreatePreview(path, 300, 180));
            if (image == null || IsDisposed) { image?.Dispose(); return; }
            if (InvokeRequired) BeginInvoke(() => ReplacePreviewImage(preview, image));
            else ReplacePreviewImage(preview, image);
        }
        catch (Exception ex)
        {
            Program.Log($"Team {_activeTeamPreviewId} {variant} kit preview failed: {ex.Message}");
        }
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
        if (_selectedRosterPlayerId <= 0) return;
        var players = Services.Session.GetTable("players");
        var playerIdColumn = players == null ? -1 : Col(players, "playerid");
        if (players == null || playerIdColumn < 0) return;
        var row = Enumerable.Range(0, players.RowCount).FirstOrDefault(index =>
        {
            var record = Services.Session.GetRecord("players", index);
            return record != null && Parse(record.Get(playerIdColumn)) == _selectedRosterPlayerId;
        }, -1);
        if (row >= 0) Services.RequestRecordNavigation("players", row);
    }

    private void OpenTransferDialog()
    {
        if (_selectedRosterPlayerId <= 0 || !_rosterByPlayerId.TryGetValue(_selectedRosterPlayerId, out var player))
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
            return link != null && Parse(link.Get(playerColumn)) == _selectedRosterPlayerId;
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
        if (_selectedRosterPlayerId <= 0 || !_rosterByPlayerId.TryGetValue(_selectedRosterPlayerId, out var player))
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
            return record != null && Parse(record.Get(playerColumn)) == _selectedRosterPlayerId;
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
            if (!StageField("playerloans", loanRow, "playerid", _selectedRosterPlayerId.ToString(), _stagingGrid)) return;
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
        "managerfirstname" => ResolveManagerField(teamId, "firstname"),
        "managersurname" => ResolveManagerField(teamId, "surname"),
        _ => string.Empty,
    };

    private string ResolveManagerField(int teamId, string field)
    {
        var managers = Services.Session.GetTable("manager");
        if (managers == null) return string.Empty;
        var managerTeamColumn = Col(managers, "teamid");
        var managerFieldColumn = Col(managers, field);
        if (managerTeamColumn < 0 || managerFieldColumn < 0)
            return string.Empty;
        for (var row = 0; row < managers.RowCount; row++)
        {
            var record = Services.Session.GetRecord("manager", row);
            if (record != null && Parse(record.Get(managerTeamColumn)) == teamId)
                return record.Get(managerFieldColumn) ?? string.Empty;
        }
        return string.Empty;
    }

    private void LoadTeamManagerPreview(int teamId)
    {
        var managers = Services.Session.GetTable("manager");
        if (managers == null) { ReplacePreviewImage(_teamManagerImg, null); return; }
        var teamColumn = Col(managers, "teamid");
        var managerIdColumn = Col(managers, "managerid");
        var headColumn = Col(managers, "headassetid");
        for (var row = 0; row < managers.RowCount; row++)
        {
            var record = Services.Session.GetRecord("manager", row);
            if (record == null || Parse(record.Get(teamColumn)) != teamId) continue;
            var managerId = Parse(record.Get(managerIdColumn));
            var portraitId = headColumn >= 0 ? Parse(record.Get(headColumn)) : 0;
            if (portraitId <= 0) portraitId = managerId;
            var local = Services.Assets.GetManagerFace(portraitId);
            FrostbitePreviewLoader.LoadLegacyUiAsset(_teamManagerImg, Services, local,
                $"data/ui/imgAssets/heads_staff/heads_staff_{portraitId}.dds", (image, _) =>
                {
                    if (IsDisposed) { image?.Dispose(); return; }
                    ReplacePreviewImage(_teamManagerImg, image);
                });
            return;
        }
        ReplacePreviewImage(_teamManagerImg, null);
    }

    private void LoadTeamStadiumPreview(int teamId, int fallbackStadiumId)
    {
        var stadiumId = fallbackStadiumId;
        var links = Services.Session.GetTable("teamstadiumlinks");
        if (links != null)
        {
            var teamColumn = Col(links, "teamid");
            var stadiumColumn = Col(links, "stadiumid");
            for (var row = 0; row < links.RowCount; row++)
            {
                var record = Services.Session.GetRecord("teamstadiumlinks", row);
                if (record != null && Parse(record.Get(teamColumn)) == teamId)
                { stadiumId = Parse(record.Get(stadiumColumn)); break; }
            }
        }
        var candidates = new[]
        {
            $"data/ui/imgAssets/stadium/stadium_{stadiumId}_0.dds",
            $"data/ui/external/ion_fut/imgAssets/stadiums/stadium_{stadiumId}.dds",
            $"data/ui/external/ion_fut/imgAssets/cards/stadium/stadium_{stadiumId}.dds",
            $"data/ui/imgAssets/clubInfo/stadium/st_{stadiumId}.dds",
        };
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(_teamStadiumImg, Services,
            Services.Assets.GetStadium(stadiumId), candidates, (image, _) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                ReplacePreviewImage(_teamStadiumImg, image);
            });
    }

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
                if (viewer.IsDisposed) { image?.Dispose(); return; }
                var old = viewer.Image;
                viewer.Image = image;
                old?.Dispose();
                _crestCaption.Text = image == null
                    ? $"{teamName}\r\nNo crest available"
                    : $"{teamName}\r\n{source}";
            }
            catch (System.AccessViolationException ex) { Program.Log("Team crest preview access violation: " + ex.Message); }
            catch (Exception ex) { Program.Log("Team crest preview failed: " + ex.Message); }
        }, resolvedPath => LegacyAssetActions.SetTarget(
            viewer, new LegacyAssetEditTarget(resolvedPath, 256, 256)));
    }

    /// <summary>
    /// Uses the canonical FC26 crest paths for the profile and kit fallback.
    /// Loose asset packs are optional; the installed Frostbite asset is the
    /// normal source when they are not present.
    /// </summary>
    private void LoadProfileCrest(int teamId)
    {
        var local = Services.Assets.GetTeamLogo(teamId);
        var candidates = new[]
        {
            $"data/ui/imgAssets/crest/dark/l{teamId}.dds",
            $"data/ui/imgAssets/crest/light/l{teamId}.dds",
            $"data/ui/imgAssets/crest/l{teamId}.dds"
        };
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(_teamCrestPreview, Services, local, candidates,
            (image, _) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                ReplacePreviewImage(_teamCrestPreview, image);
            });
    }

    /// <summary>Load the real installed FC26 jersey colour textures for the club profile.</summary>
    private async Task LoadTeamKitPreviewsAfterIdentityAsync(int teamId)
    {
        // Give the small crest/identity request first access to the bridge and
        // UI continuation. Four large jersey exports must never make the club
        // identity look broken while the page is opening.
        await Task.Delay(250);
        if (teamId == _activeTeamPreviewId && !IsDisposed)
            await LoadTeamKitPreviewsAsync(teamId);
    }

    private async Task LoadTeamKitPreviewsAsync(int teamId)
    {
        if (teamId <= 0 || teamId != _activeTeamPreviewId) return;
        var requests = new[]
        {
            (Variant: "home", Preview: _teamKitHome),
            (Variant: "away", Preview: _teamKitAway),
            (Variant: "third", Preview: _teamKitThird),
            (Variant: "gk", Preview: _teamKitGk),
        };
        await Task.WhenAll(requests.Select(r => LoadOneKitPreviewAsync(r.Variant, r.Preview)));
    }

    private static int KitTextureScore(string name)
    {
        var score = name.Contains("/jersey_", StringComparison.OrdinalIgnoreCase) ? 100 : 0;
        if (name.Contains("_coeff", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("_normal", StringComparison.OrdinalIgnoreCase)) score -= 80;
        return score - name.Length;
    }

    private static void ReplacePreviewImage(PictureBox preview, Image? image)
    {
        var old = preview.Image;
        preview.Image = image;
        old?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _minifaceLoadGate.Dispose();
            _rosterMinifaces.Dispose();
        }
        base.Dispose(disposing);
    }
}
