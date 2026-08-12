using System.Drawing;
using System.Data;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>
/// FC26 data adapter presented with the original CM16 CountryForm geometry.
/// The CM16 source remains linked under CM16Source/CountryForm.cs as the
/// canonical reference for this port.
/// </summary>
public sealed class CountriesSection : SectionBase
{
    private static readonly Font LegacyFont = Theme.Body;
    private readonly List<TextBox> _editors = [];
    private readonly List<TextBox> _audioEditors = [];
    private readonly List<TextBox> _mirrors = [];
    private readonly Dictionary<string, FieldValue> _fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _stagingGrid = new();
    private readonly List<PictureBox> _flagViewers = [];
    private readonly List<Label> _flagCaptions = [];
    private readonly PictureBox _mapViewer;
    private readonly CheckBox _topTier = new();
    private readonly CheckBox _showAllDatabaseCountries = new();
    private readonly Button _openNationalTeam = new();
    private readonly Label _countryNameLabel = new();
    private readonly Label _countryMetaLabel = new();
    private readonly PictureBox _countryFlagPreview = new();
    private bool _syncTopTier;
    private bool _suppressListReload;

    public override string SectionKey => "countries";
    public override string SectionTitle => "Countries";
    protected override string TableName => "nations";
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search countries…";

    public CountriesSection(AppServices services) : base(services)
    {
        Header.Visible = false;
        Tabs.Padding = new Point(3, 1);

        var page = new TabPage("General") { BackColor = Theme.Background, Font = LegacyFont };
        var canvas = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CardLayout.CardBackground };
        canvas.AutoScrollMinSize = new Size(0, 900);
        page.Controls.Add(canvas);
        Tabs.TabPages.Add(page);

        // ═══════════════════════════════════════════════════════════════
        //  COUNTRY PROFILE HEADER
        // ═══════════════════════════════════════════════════════════════
        var profile = new Panel { Location = new Point(12, 12), Size = new Size(1340, 180), BackColor = CardLayout.CardWhite };
        CardLayout.ApplyRounded(profile, 14);
        profile.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(6, 180), BackColor = CardLayout.Fc26Green });
        _countryFlagPreview.Location = new Point(24, 24);
        _countryFlagPreview.Size = new Size(120, 120);
        _countryFlagPreview.SizeMode = PictureBoxSizeMode.Zoom;
        _countryFlagPreview.BackColor = CardLayout.CardFieldBg;
        _countryFlagPreview.BorderStyle = BorderStyle.None;
        profile.Controls.Add(_countryFlagPreview);
        _countryNameLabel.Location = new Point(164, 30);
        _countryNameLabel.Size = new Size(500, 38);
        _countryNameLabel.Font = new Font("Segoe UI", 22, FontStyle.Bold);
        _countryNameLabel.ForeColor = CardLayout.CardText;
        profile.Controls.Add(_countryNameLabel);
        _countryMetaLabel.Location = new Point(166, 74);
        _countryMetaLabel.Size = new Size(600, 22);
        _countryMetaLabel.Font = Theme.BodyBold;
        _countryMetaLabel.ForeColor = CardLayout.CardMuted;
        profile.Controls.Add(_countryMetaLabel);
        var addCountry = CardLayoutButton("Add Country to Game", new Point(166, 108), new Size(180, 30));
        addCountry.Click += (_, _) => CreateNewRecord();
        profile.Controls.Add(addCountry);
        var createNationalTeam = CardLayoutButton("Create National Team", new Point(354, 108), new Size(180, 30));
        createNationalTeam.Click += (_, _) => CreateNationalTeam();
        profile.Controls.Add(createNationalTeam);
        _openNationalTeam.Text = "Open National Team";
        _openNationalTeam.Location = new Point(542, 108);
        _openNationalTeam.Size = new Size(180, 30);
        _openNationalTeam.Font = LegacyFont;
        _openNationalTeam.Enabled = false;
        Theme.ApplyButton(_openNationalTeam);
        _openNationalTeam.Click += (_, _) => OpenLinkedNationalTeam();
        profile.Controls.Add(_openNationalTeam);
        canvas.Controls.Add(profile);

        // ═══════════════════════════════════════════════════════════════
        //  COUNTRY DETAILS + MAP
        // ═══════════════════════════════════════════════════════════════
        var details = CardLayout.CreateGroup(canvas, "Country Details", CardLayout.Fc26Green, 12, 204, 560, 340);
        AddField(details, "nationname", "Database Name", new Point(130, 22), 105);
        AddField(details, "nationid", "Country Id", new Point(130, 48), 105);
        AddMirrorField(details, "nationname", "Name", new Point(130, 74), 105);
        AddField(details, "nationstartingfirstletter", "Starting Letter", new Point(130, 100), 105);
        AddField(details, "isocountrycode", "Abbreviation", new Point(130, 126), 105);
        AddField(details, "confederation", "Confederation", new Point(130, 152), 105);
        AddMirrorField(details, "isocountrycode", "ISO Country Code", new Point(130, 178), 105);
        AddField(details, "groupid", "Level", new Point(130, 204), 105);
        AddField(details, "streetdressing", "Street Dressing", new Point(130, 230), 105);
        _topTier.Text = "Top tier";
        _topTier.Location = new Point(11, 258); _topTier.Size = new Size(100, 22);
        _topTier.Font = LegacyFont; _topTier.BackColor = CardLayout.CardWhite; _topTier.ForeColor = CardLayout.CardText;
        _topTier.FlatStyle = FlatStyle.Flat; _topTier.Tag = "top_tier";
        _topTier.CheckedChanged += (_, _) => { if (_syncTopTier || CurrentRecordIndex < 0 || !_fields.TryGetValue("top_tier", out var value) || !value.IsWritable) return; StageField(TableName, CurrentRecordIndex, "top_tier", _topTier.Checked ? "1" : "0", _stagingGrid); };
        details.Controls.Add(_topTier);
        _showAllDatabaseCountries.Text = "Show countries awaiting setup";
        _showAllDatabaseCountries.Location = new Point(16, 288); _showAllDatabaseCountries.Size = new Size(210, 23);
        _showAllDatabaseCountries.Font = LegacyFont; _showAllDatabaseCountries.BackColor = CardLayout.CardWhite;
        _showAllDatabaseCountries.ForeColor = CardLayout.CardText; _showAllDatabaseCountries.FlatStyle = FlatStyle.Flat;
        _showAllDatabaseCountries.CheckedChanged += (_, _) => { if (_suppressListReload) return; LoadData(); };
        ToolTip.SetToolTip(_showAllDatabaseCountries, "Off: show only playable countries. On: also show database countries that still need a league, clubs and Compdata.");
        details.Controls.Add(_showAllDatabaseCountries);
        details.Controls.Add(new Label { Text = "Create a country ID, then add its national team, domestic league and clubs before a Career save.", Location = new Point(16, 316), Size = new Size(530, 20), Font = LegacyFont, ForeColor = CardLayout.CardSubtle, BackColor = CardLayout.CardWhite });

        // Flag viewers
        var flags = CardLayout.CreateGroup(canvas, "Flags", CardLayout.Fc26Blue, 588, 204, 764, 340);
        flags.Controls.Add(CreateViewer(new Point(10, 26), new Size(256, 256), "256 x 256", out var largeFlag, out var largeCaption));
        flags.Controls.Add(CreateViewer(new Point(276, 26), new Size(256, 256), "512 x 512", out var crestFlag, out var crestCaption));
        flags.Controls.Add(CreateViewer(new Point(542, 26), new Size(150, 150), "256 x 128", out var cardFlag, out var cardCaption));
        flags.Controls.Add(CreateViewer(new Point(10, 288), new Size(64, 64), "64 x 64", out var miniFlag, out var miniCaption));
        _flagViewers.AddRange([largeFlag, crestFlag, cardFlag, miniFlag]);
        _flagCaptions.AddRange([largeCaption, crestCaption, cardCaption, miniCaption]);
        LegacyAssetActions.Attach(Services, flags, largeFlag, new Point(10, 310), RefreshCurrentRecord);

        // Map
        var map = CardLayout.CreateGroup(canvas, "Map (Shape)", CardLayout.Fc26Blue, 12, 556, 1340, 340);
        map.Controls.Add(CreateViewer(new Point(8, 26), new Size(512, 256), "512 x 256", out _mapViewer, out _));
        LegacyAssetActions.Attach(Services, map, _mapViewer, new Point(8, 302), RefreshCurrentRecord);

        void ReflowCountry()
        {
            var width = Math.Max(680, canvas.ClientSize.Width - 28);
            profile.Width = width;
            var mapY = 556;
            if (width >= 1320)
            {
                details.Bounds = new Rectangle(12, 204, 560, 340);
                flags.Bounds = new Rectangle(588, 204, width - 576, 340);
            }
            else
            {
                details.Bounds = new Rectangle(12, 204, width, 340);
                flags.Bounds = new Rectangle(12, 556, width, 340);
                mapY = 908;
            }
            map.Bounds = new Rectangle(12, mapY, width, 340);
            canvas.AutoScrollMinSize = new Size(0, map.Bottom + 12);
        }
        canvas.ClientSizeChanged += (_, _) => ReflowCountry();
        ReflowCountry();

        AddNationalAudioTab();
    }

    protected override void CreateNewRecord()
    {
        if (!EntityCreationDialog.TryShow(this, "Country",
                [("Country name", "New Country"), ("ISO code", "NC")], out var values))
            return;
        // A newly created country is deliberately not Career-playable yet. Keep
        // it visible to its creator while they finish its league and Compdata.
        // The load that follows (below) re-applies the filtered list, so the
        // mid-creation CheckedChanged reload must be suppressed.
        _suppressListReload = true;
        _showAllDatabaseCountries.Checked = true;
        _suppressListReload = false;
        var iso = values[1].Trim().ToUpperInvariant();
        if (iso.Length != 2 || !iso.All(char.IsLetter))
        {
            MessageBox.Show(this, "ISO code must contain exactly two letters.", "Create Country",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var nations = Services.Session.GetTable(TableName);
        if (nations != null)
        {
            var idColumn = Col(nations, "nationid");
            var nameColumn = Col(nations, "nationname");
            var isoColumn = Col(nations, "isocountrycode");
            for (var row = 0; row < nations.RowCount; row++)
            {
                var record = Services.Session.GetRecord(TableName, row);
                if (record == null) continue;
                var existingName = nameColumn >= 0 ? record.Get(nameColumn) : string.Empty;
                var existingIso = isoColumn >= 0 ? record.Get(isoColumn) : string.Empty;
                if (!string.Equals(existingName, values[0], StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(existingIso, iso, StringComparison.OrdinalIgnoreCase)) continue;
                var existingId = idColumn >= 0 ? record.Get(idColumn) : "unknown";
                MessageBox.Show(this, $"{existingName} already exists with Country ID {existingId}. This only confirms the database record; it does not make the country Career-playable. Configure a league, clubs and Compdata before testing a new Career save.",
                    "Country Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
        try
        {
            var startingLetter = Math.Clamp(char.ToUpperInvariant(values[0][0]) - 'A' + 1, 1, 26);
            var id = CreateRecordFromTemplate(TableName, "nationid", new Dictionary<string, string>
            {
                ["nationname"] = values[0],
                ["isocountrycode"] = iso,
                ["nationstartingfirstletter"] = startingLetter.ToString(),
                ["top_tier"] = "0",
            });
            Services.RegisterDraftCountry(id);
            Services.Session.RefreshSchema();
            Services.RefreshDatabaseIndexes();
            LoadData();
            var created = GetRecords().FirstOrDefault(item =>
                Parse(Services.Session.GetCell(TableName, item.RecordIndex, "nationid")) == id);
            if (created != null) GoToRecord(created.RecordIndex);
            MessageBox.Show(this,
                $"Country created with ID {id}. It is now visible as a setup country. Add a domestic league and at least one club, then build its Compdata before starting a new Career save.",
                "Create Country", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create Country", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// National teams belong to a country workflow, not to the club editor.
    /// The selected country's ID is used directly so users never need to type
    /// an internal ID or accidentally link the side to the wrong nation.
    /// </summary>
    private void CreateNationalTeam()
    {
        if (CurrentRecordIndex < 0)
        {
            MessageBox.Show(this, "Select a country first.", "Create National Team",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var nationId = Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, "nationid"));
        var nationName = Services.Session.GetCell(TableName, CurrentRecordIndex, "nationname");
        if (nationId <= 0)
        {
            MessageBox.Show(this, "The selected country has no valid Country ID.", "Create National Team",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!EntityCreationDialog.TryShow(this, "National Team",
                [("Team name", $"{nationName} National Team")], out var values))
            return;

        var teams = Services.Session.GetTable("teams");
        var links = Services.Session.GetTable("teamnationlinks");
        if (teams == null || teams.RowCount == 0 || links == null || links.RowCount == 0)
        {
            MessageBox.Show(this, "A safe team and country-link template is required to create a national team.",
                "Create National Team", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            var teamId = CreateRecordFromTemplate("teams", "teamid", new Dictionary<string, string>
            {
                ["teamname"] = values[0], ["nationality"] = nationId.ToString(),
                ["assetid"] = "0", ["presassetone"] = "0", ["presassettwo"] = "0",
                ["captainid"] = "-1", ["penaltytakerid"] = "-1", ["freekicktakerid"] = "-1",
                ["leftcornerkicktakerid"] = "-1", ["rightcornerkicktakerid"] = "-1",
                ["stadiumid"] = "-1", ["managerid"] = "-1", ["kitids"] = "0",
                ["formationid"] = "-1", ["domesticprestige"] = "0",
                ["internationalprestige"] = "0", ["clubworth"] = "0",
                ["overallrating"] = "0", ["attackrating"] = "0",
                ["midfieldrating"] = "0", ["defenserating"] = "0", ["ballid"] = "0"
            }, templateRow: 0);
            var duplicate = Services.Session.DuplicateRow("teamnationlinks", 0);
            if (!duplicate.Success) throw new InvalidOperationException(duplicate.Message);
            // The native engine inserts the duplicated row right after the
            // template (index 1), not at the end of the table.
            var linkRow = 1;
            foreach (var (field, value) in new Dictionary<string, string>
            {
                ["teamid"] = teamId.ToString(), ["nationid"] = nationId.ToString(),
            })
            {
                if (links.FindColumn(field) == null) continue;
                var outcome = Services.Pending.Stage("teamnationlinks", linkRow, field, value);
                if (!outcome.Success) throw new InvalidOperationException(outcome.Message);
            }
            Services.Pending.MarkStructuralChange();
            var squad = FillTeamSquad(teamId);
            Services.Session.RefreshSchema();
            Services.RefreshDatabaseIndexes();
            UpdateOpenNationalTeam(nationId);
            var row = FindNationalTeamRow(nationId);
            if (row >= 0) Services.RequestRecordNavigation("teams", row);
            MessageBox.Show(this,
                $"{values[0]} was created for {nationName} with Team ID {teamId} and a squad of {squad} placeholder players.\n\n" +
                "The team has been opened in the Teams section — rename the Player 1..N rows and press Save when ready.",
                "Create National Team", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create National Team", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override IReadOnlyList<RecordListItem> GetRecords()
    {
        var countries = Services.RequireData().GetCountries();
        if (_showAllDatabaseCountries.Checked) return countries;

        var leagueTable = Services.Session.GetTable("leagues");
        var linkTable = Services.Session.GetTable("leagueteamlinks");
        var nationTable = Services.Session.GetTable(TableName);
        if (leagueTable == null || linkTable == null || nationTable == null) return countries;

        var linkedLeagueIds = new HashSet<int>();
        var linkLeagueColumn = Col(linkTable, "leagueid");
        for (var row = 0; row < linkTable.RowCount; row++)
        {
            var link = Services.Session.GetRecord("leagueteamlinks", row);
            if (link != null && int.TryParse(link.Get(linkLeagueColumn), out var leagueId) && leagueId > 0)
                linkedLeagueIds.Add(leagueId);
        }
        var playableNationIds = new HashSet<int>();
        var leagueIdColumn = Col(leagueTable, "leagueid");
        var leagueNationColumn = Col(leagueTable, "countryid");
        for (var row = 0; row < leagueTable.RowCount; row++)
        {
            var league = Services.Session.GetRecord("leagues", row);
            if (league == null || !int.TryParse(league.Get(leagueIdColumn), out var leagueId) || !linkedLeagueIds.Contains(leagueId)) continue;
            if (int.TryParse(league.Get(leagueNationColumn), out var nationId) && nationId > 0)
                playableNationIds.Add(nationId);
        }
        var nationIdColumn = Col(nationTable, "nationid");
        return countries.Where(item =>
        {
            var nation = Services.Session.GetRecord(TableName, item.RecordIndex);
            return nation != null && int.TryParse(nation.Get(nationIdColumn), out var nationId) &&
                   (playableNationIds.Contains(nationId) || Services.IsDraftCountry(nationId));
        }).ToList();
    }

    private List<int> LinkedTeamIds(int leagueId)
    {
        var links = Services.Session.GetTable("leagueteamlinks");
        if (links == null) return [];
        var leagueColumn = Col(links, "leagueid");
        var teamColumn = Col(links, "teamid");
        var result = new List<int>();
        for (var row = 0; row < links.RowCount; row++)
        {
            var link = Services.Session.GetRecord("leagueteamlinks", row);
            if (link != null && link.Get(leagueColumn) == leagueId.ToString() &&
                int.TryParse(link.Get(teamColumn), out var teamId) && teamId > 0) result.Add(teamId);
        }
        return result.Distinct().ToList();
    }

    protected override void ShowRecord(int recordIndex)
    {
        var table = Services.Session.GetTable(TableName)!;
        var record = Services.Session.GetRecord(TableName, recordIndex)!;
        _fields.Clear();
        foreach (var field in Services.RequireData().GetFields(TableName, recordIndex, LabelMaps.Nations)) _fields[field.FieldName] = field;

        foreach (var editor in _editors)
        {
            var fieldName = editor.Tag as string ?? string.Empty;
            if (_fields.TryGetValue(fieldName, out var field))
            {
                editor.Text = field.Value;
                editor.ReadOnly = !field.IsWritable;
                editor.BackColor = field.IsWritable ? Theme.Input : CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardText;
                ToolTip.SetToolTip(editor, field.IsWritable ? field.FieldName : $"{field.FieldName} (read-only)");
            }
            else
            {
                editor.Text = string.Empty;
                editor.ReadOnly = true;
                editor.BackColor = CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardSubtle;
                ToolTip.SetToolTip(editor, $"{fieldName} is not present in this database");
            }
        }
        RefreshMirrors();

        _syncTopTier = true;
        try
        {
            if (_fields.TryGetValue("top_tier", out var top))
            {
                _topTier.Checked = top.RawValue != "0";
                _topTier.Enabled = top.IsWritable;
                ToolTip.SetToolTip(_topTier, top.IsWritable ? top.FieldName : $"{top.FieldName} (read-only)");
            }
            else
            {
                _topTier.Checked = false;
                _topTier.Enabled = false;
                ToolTip.SetToolTip(_topTier, "top_tier is not present in this database");
            }
        }
        finally { _syncTopTier = false; }

        var nationId = Parse(record.Get(Col(table, "nationid")));
        var nationName = record.Get(Col(table, "nationname"));
        ShowFlag(Services.Assets.GetFlag(nationId), nationId, nationName);
        ShowNationalAudio(nationId);
        UpdateOpenNationalTeam(nationId);

        // ── Populate header card ──────────────────────────────────────────
        _countryNameLabel.Text = nationName ?? string.Empty;
        _countryMetaLabel.Text = $"Nation ID {record.Get(Col(table, "nationid"))}  ·  {record.Get(Col(table, "isocountrycode"))}  ·  {record.Get(Col(table, "confederation"))}";
        try
        {
            var flagPath = Services.Assets.GetFlag(nationId);
            if (!string.IsNullOrWhiteSpace(flagPath) && File.Exists(flagPath))
                _countryFlagPreview.Image = Image.FromFile(flagPath);
            else
                _countryFlagPreview.Image = null;
        }
        catch { _countryFlagPreview.Image = null; }
    }

    /// <summary>Finds the record index of the national team linked to a country.</summary>
    private int FindNationalTeamRow(int nationId)
    {
        var links = Services.Session.GetTable("teamnationlinks");
        if (links == null) return -1;
        var nationColumn = Col(links, "nationid");
        var teamColumn = Col(links, "teamid");
        if (nationColumn < 0 || teamColumn < 0) return -1;
        var linkedTeamId = -1;
        for (var row = 0; row < links.RowCount; row++)
        {
            var record = Services.Session.GetRecord("teamnationlinks", row);
            if (record == null) continue;
            if (Parse(record.Get(nationColumn)) == nationId)
            {
                linkedTeamId = Parse(record.Get(teamColumn));
                break;
            }
        }
        if (linkedTeamId <= 0) return -1;
        var teams = Services.Session.GetTable("teams");
        if (teams == null) return -1;
        var teamIdColumn = Col(teams, "teamid");
        if (teamIdColumn < 0) return -1;
        for (var row = 0; row < teams.RowCount; row++)
        {
            var record = Services.Session.GetRecord("teams", row);
            if (record != null && Parse(record.Get(teamIdColumn)) == linkedTeamId) return row;
        }
        return -1;
    }

    private void UpdateOpenNationalTeam(int nationId)
    {
        var row = FindNationalTeamRow(nationId);
        _openNationalTeam.Enabled = row >= 0;
        if (row >= 0)
        {
            var teams = Services.Session.GetTable("teams");
            var name = teams != null
                ? Services.Session.GetCell("teams", row, "teamname")
                : string.Empty;
            _openNationalTeam.Text = string.IsNullOrWhiteSpace(name)
                ? "Open National Team" : $"Open: {name}";
        }
        else
        {
            _openNationalTeam.Text = "Open National Team";
        }
    }

    private void OpenLinkedNationalTeam()
    {
        if (CurrentRecordIndex < 0) return;
        var nationId = Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, "nationid"));
        var row = FindNationalTeamRow(nationId);
        if (row < 0)
        {
            MessageBox.Show(this, "The selected country has no linked national team yet.",
                "Open National Team", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Services.RequestRecordNavigation("teams", row);
    }

    private void AddNationalAudioTab()
    {
        var page = new TabPage("National Team Audio")
        {
            BackColor = Theme.Background, Font = LegacyFont
        };
        var canvas = new Panel
        {
            Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background
        };
        page.Controls.Add(canvas);
        Tabs.TabPages.Add(page);
        var box = LegacyGroup("Nation and National Team Audio", new Point(3, 3), new Size(710, 237));
        var fields = new[]
        {
            ("PA Language", "palanguageindex"),
            ("Commentary Language", "defaultcommlang"),
            ("Player Call Bank", "playercallpatchbankindex"),
            ("SSF Player Call", "ssfplayercallindex"),
            ("Crowd Beds Region", "crowdbedsregionindex"),
            ("Chant Region", "chantregionindex"),
            ("Reactions Region", "reactionsregionindex"),
            ("Heckles Region", "hecklesregionindex"),
            ("Ambience Region", "ambienceregionindex"),
            ("Whistles Region", "whistlesregionindex"),
            ("Team Can Whistle", "teamcanwhistleindex")
        };
        for (var index = 0; index < fields.Length; index++)
        {
            var col = index % 2;
            var row = index / 2;
            var x = 16 + (col * 340);
            var y = 28 + (row * 26);
            var label = new Label
            {
                Text = fields[index].Item1, Location = new Point(x, y + 3),
                Size = new Size(165, 18), Font = LegacyFont, AutoEllipsis = true
            };
            ToolTip.SetToolTip(label, fields[index].Item1);
            var editor = new TextBox
            {
                Location = new Point(x + 170, y), Size = new Size(145, 20),
                Font = LegacyFont, Tag = fields[index].Item2
            };
            Theme.ApplyTextBox(editor);
            editor.Leave += (_, _) => CommitNationalAudio(editor);
            _audioEditors.Add(editor);
            box.Controls.Add(editor);
        }
        box.Controls.Add(new Label
        {
            Text = "Audio mappings for the selected country. They control regional commentary and crowd banks.",
            Location = new Point(16, 186), Size = new Size(660, 45),
            Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        canvas.Controls.Add(box);
    }

    private void ShowNationalAudio(int nationId)
    {
        var row = FindRow("audionation", "nationid", nationId);
        foreach (var editor in _audioEditors)
        {
            var field = editor.Tag as string ?? string.Empty;
            if (row < 0)
            {
                editor.Text = string.Empty;
                editor.ReadOnly = true;
                editor.BackColor = CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardSubtle;
                continue;
            }
            editor.Text = Services.Session.GetCell("audionation", row, field);
            var table = Services.Session.GetTable("audionation");
            var column = table?.Columns?.FirstOrDefault(x => x.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
            editor.ReadOnly = column?.IsWritable != true;
            editor.BackColor = editor.ReadOnly ? CardLayout.CardFieldBg : Theme.Input;
            editor.ForeColor = CardLayout.CardText;
        }
    }

    private void CommitNationalAudio(TextBox editor)
    {
        if (CurrentRecordIndex < 0 || editor.ReadOnly || editor.Tag is not string field) return;
        var nationId = Parse(Services.Session.GetCell(TableName, CurrentRecordIndex, "nationid"));
        var row = FindRow("audionation", "nationid", nationId);
        if (row < 0) return;
        StageField("audionation", row, field, editor.Text.Trim(), _stagingGrid);
    }

    private int FindRow(string tableName, string fieldName, int wanted)
    {
        var table = Services.Session.GetTable(tableName);
        if (table == null) return -1;
        for (var row = 0; row < table.RowCount; row++)
            if (Parse(Services.Session.GetCell(tableName, row, fieldName)) == wanted)
                return row;
        return -1;
    }

    private Panel LegacyGroup(string text, Point location, Size size)
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

    private void AddField(Control parent, string fieldName, string label, Point location, int width)
    {
        parent.Controls.Add(new Label
        {
            Text = label, Location = new Point(11, location.Y + 3), Size = new Size(Math.Max(70, location.X - 17), 18),
            Font = LegacyFont, TextAlign = ContentAlignment.MiddleRight, AutoEllipsis = true,
            ForeColor = CardLayout.CardFieldLabel, BackColor = CardLayout.CardWhite
        });
        var editor = new TextBox { Location = location, Size = new Size(width, 20), Font = LegacyFont, Tag = fieldName, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(editor);
        editor.Leave += (_, _) => Commit(editor);
        parent.Controls.Add(editor);
        _editors.Add(editor);
    }

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

    /// <summary>
    /// Read-only mirror of a field edited elsewhere in the same group, so "Name"
    /// never becomes a second writable editor for nationname.
    /// </summary>
    private void AddMirrorField(Control parent, string fieldName, string label, Point location, int width)
    {
        var caption = new Label
        {
            Text = label, Location = new Point(11, location.Y + 3), Size = new Size(Math.Max(70, location.X - 17), 18),
            Font = LegacyFont, TextAlign = ContentAlignment.MiddleRight, AutoEllipsis = true,
            ForeColor = CardLayout.CardFieldLabel, BackColor = CardLayout.CardWhite
        };
        parent.Controls.Add(caption);
        ToolTip.SetToolTip(caption, label);
        var editor = new TextBox { Location = location, Size = new Size(width, 20), Font = LegacyFont, Tag = fieldName, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(editor);
        editor.BackColor = CardLayout.CardFieldBg;
        editor.ForeColor = CardLayout.CardText;
        parent.Controls.Add(editor);
        _editors.Add(editor);
        _mirrors.Add(editor);
    }

    private void RefreshMirrors()
    {
        foreach (var mirror in _mirrors)
        {
            var fieldName = mirror.Tag as string ?? string.Empty;
            mirror.ReadOnly = true;
            mirror.BackColor = CardLayout.CardFieldBg;
            mirror.ForeColor = CardLayout.CardText;
            ToolTip.SetToolTip(mirror, $"Read-only mirror of {fieldName} — edit it in its named field above.");
            if (_fields.TryGetValue(fieldName, out var field))
                mirror.Text = field.Value;
        }
    }

    private static Panel CreateViewer(Point location, Size imageSize, string resolution, out PictureBox picture, out Label caption)
    {
        var holder = new Panel { Location = location, Size = new Size(imageSize.Width, imageSize.Height + 23), BackColor = Theme.Panel };
        picture = new PictureBox { Location = Point.Empty, Size = imageSize, BackColor = Theme.Input, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        caption = new Label { Text = "◉   ◧  ◨   " + resolution, Location = new Point(0, imageSize.Height + 2), Size = new Size(imageSize.Width, 18), Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel };
        holder.Controls.Add(picture);
        holder.Controls.Add(caption);
        return holder;
    }

    private void Commit(TextBox editor)
    {
        if (CurrentRecordIndex < 0 || editor.ReadOnly || editor.Tag is not string fieldName || !_fields.TryGetValue(fieldName, out var field)) return;
        if (!string.Equals(editor.Text.Trim(), field.Value, StringComparison.Ordinal))
        {
            if (StageField(TableName, CurrentRecordIndex, fieldName, editor.Text.Trim(), _stagingGrid))
                RefreshMirrors();
        }
    }

    private void ShowFlag(string? path, int nationId, string nationName)
    {
        var query = string.Join("_", nationName.ToLowerInvariant()
            .Split([' ', '-', '.', '\'', '’'], StringSplitOptions.RemoveEmptyEntries));
        var flagPath = $"data/ui/imgAssets/flags512x512/light/f_{nationId}.dds";
        LegacyAssetActions.SetTarget(_flagViewers[0], new LegacyAssetEditTarget(flagPath, 512, 512));
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _flagViewers[0], Services, LegacyAssetActions.Replacement(Services, flagPath) ?? path,
            flagPath, (image, source) =>
        {
            if (IsDisposed) { image?.Dispose(); return; }
            for (var index = 0; index < _flagViewers.Count; index++)
            {
                var viewer = _flagViewers[index];
                if (viewer.IsDisposed) continue;
                var next = index == 0 ? image : image == null ? null : new Bitmap(image);
                var old = viewer.Image;
                viewer.Image = next;
                old?.Dispose();
            }
            if (image != null)
            {
                foreach (var label in _flagCaptions)
                {
                    if (label.IsDisposed) continue;
                    label.Text = source ?? "Flag preview";
                }
            }
            else foreach (var label in _flagCaptions)
            {
                if (label.IsDisposed) continue;
                label.Text = $"No flag available ({nationId})";
            }
        });

        var mapPath = $"data/ui/imgAssets/countryShapes/c{nationId}.dds";
        LegacyAssetActions.SetTarget(_mapViewer, new LegacyAssetEditTarget(mapPath, 512, 256));
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _mapViewer, Services, LegacyAssetActions.Replacement(Services, mapPath),
            mapPath, (image, _) =>
        {
            if (IsDisposed) { image?.Dispose(); return; }
            _mapViewer.Image?.Dispose();
            _mapViewer.Image = image;
        });
    }

    private void RefreshCurrentRecord()
    {
        if (CurrentRecordIndex >= 0) ShowRecord(CurrentRecordIndex);
    }
}
