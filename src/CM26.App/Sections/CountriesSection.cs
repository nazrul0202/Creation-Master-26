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
    private readonly Dictionary<string, FieldValue> _fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _stagingGrid = new();
    private readonly List<PictureBox> _flagViewers = [];
    private readonly List<Label> _flagCaptions = [];
    private readonly PictureBox _mapViewer;
    private readonly CheckBox _topTier = new();
    private readonly CheckBox _showAllDatabaseCountries = new();
    private readonly Button _openNationalTeam = new();
    private bool _syncTopTier;

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
        var canvas = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background };
        page.Controls.Add(canvas);
        Tabs.TabPages.Add(page);

        // Exact CountryForm.cs grouping: identity at left; 256/512/card/mini
        // image viewers in the centre; country map at the right.
        var country = LegacyGroup("Country", new Point(3, 1), new Size(767, 548));
        AddField(country, "nationname", "Database Name", new Point(101, 14), 133);
        AddField(country, "nationid", "Country Id", new Point(101, 44), 100);
        AddField(country, "nationname", "Name", new Point(101, 74), 133);
        AddField(country, "nationstartingfirstletter", "Starting Letter", new Point(101, 104), 133);
        AddField(country, "isocountrycode", "Abbreviation", new Point(101, 134), 133);
        AddField(country, "confederation", "Confederation", new Point(101, 164), 133);
        AddField(country, "isocountrycode", "ISO Country Code", new Point(117, 195), 117);
        AddField(country, "groupid", "Level", new Point(117, 225), 117);
        AddField(country, "streetdressing", "Street Dressing", new Point(117, 255), 117);

        _topTier.Text = "Top tier";
        _topTier.Location = new Point(11, 282);
        _topTier.Size = new Size(100, 22);
        _topTier.Font = LegacyFont;
        _topTier.BackColor = Theme.Panel;
        _topTier.ForeColor = Theme.Text;
        _topTier.FlatStyle = FlatStyle.Flat;
        _topTier.Tag = "top_tier";
        _topTier.CheckedChanged += (_, _) =>
        {
            if (_syncTopTier || CurrentRecordIndex < 0 ||
                !_fields.TryGetValue("top_tier", out var value) || !value.IsWritable) return;
            StageField(TableName, CurrentRecordIndex, "top_tier", _topTier.Checked ? "1" : "0", _stagingGrid);
        };
        country.Controls.Add(_topTier);

        var addCountry = new Button
        {
            Text = "Add Country to Game",
            Location = new Point(16, 316),
            Size = new Size(194, 29),
            Font = LegacyFont,
        };
        Theme.ApplyButton(addCountry);
        addCountry.Click += (_, _) => CreateNewRecord();
        country.Controls.Add(addCountry);
        var createNationalTeam = new Button
        {
            Text = "Create National Team",
            Location = new Point(16, 350),
            Size = new Size(194, 29),
            Font = LegacyFont,
        };
        Theme.ApplyButton(createNationalTeam);
        createNationalTeam.Click += (_, _) => CreateNationalTeam();
        country.Controls.Add(createNationalTeam);
        _openNationalTeam.Text = "Open National Team";
        _openNationalTeam.Location = new Point(16, 383);
        _openNationalTeam.Size = new Size(194, 29);
        _openNationalTeam.Font = LegacyFont;
        _openNationalTeam.Enabled = false;
        Theme.ApplyButton(_openNationalTeam);
        _openNationalTeam.Click += (_, _) => OpenLinkedNationalTeam();
        country.Controls.Add(_openNationalTeam);
        country.Controls.Add(new Label
        {
            Text = "Create a country ID, then add its national team, domestic league,\nclubs and Compdata before starting a new Career save.",
            Location = new Point(16, 416),
            Size = new Size(215, 32),
            Font = LegacyFont,
            ForeColor = Theme.Muted,
            BackColor = Theme.Panel,
        });
        _showAllDatabaseCountries.Text = "Show countries awaiting setup";
        _showAllDatabaseCountries.Location = new Point(16, 454);
        _showAllDatabaseCountries.Size = new Size(210, 23);
        _showAllDatabaseCountries.Font = LegacyFont;
        _showAllDatabaseCountries.BackColor = Theme.Panel;
        _showAllDatabaseCountries.ForeColor = Theme.Text;
        _showAllDatabaseCountries.FlatStyle = FlatStyle.Flat;
        _showAllDatabaseCountries.CheckedChanged += (_, _) => LoadData();
        ToolTip.SetToolTip(_showAllDatabaseCountries,
            "Off: show only playable countries. On: also show database countries that still need a league, clubs and Compdata.");
        country.Controls.Add(_showAllDatabaseCountries);
        country.Controls.Add(CreateViewer(new Point(240, 13), new Size(256, 256), "256 x 256", out var largeFlag, out var largeCaption));
        country.Controls.Add(CreateViewer(new Point(502, 13), new Size(256, 256), "512 x 512", out var crestFlag, out var crestCaption));
        country.Controls.Add(CreateViewer(new Point(240, 318), new Size(150, 150), "256 x 128", out var cardFlag, out var cardCaption));
        country.Controls.Add(CreateViewer(new Point(502, 318), new Size(64, 64), "64 x 64", out var miniFlag, out var miniCaption));
        _flagViewers.AddRange([largeFlag, crestFlag, cardFlag, miniFlag]);
        _flagCaptions.AddRange([largeCaption, crestCaption, cardCaption, miniCaption]);
        // Keep the flag commands clear of the image captions and the card flag.
        // The former location at y=275 overlapped both controls at normal DPI.
        LegacyAssetActions.Attach(Services, country, largeFlag, new Point(16, 502), RefreshCurrentRecord);

        canvas.Controls.Add(country);

        var map = LegacyGroup("Map (Shape)", new Point(776, 3), new Size(528, 342));
        map.Controls.Add(CreateViewer(new Point(8, 15), new Size(512, 256), "512 x 256", out _mapViewer, out _));
        LegacyAssetActions.Attach(Services, map, _mapViewer, new Point(8, 294), RefreshCurrentRecord);
        canvas.Controls.Add(map);

        AddNationalAudioTab();
    }

    protected override void CreateNewRecord()
    {
        if (!EntityCreationDialog.TryShow(this, "Country",
                [("Country name", "New Country"), ("ISO code", "NC")], out var values))
            return;
        // A newly created country is deliberately not Career-playable yet. Keep
        // it visible to its creator while they finish its league and Compdata.
        _showAllDatabaseCountries.Checked = true;
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
                editor.BackColor = field.IsWritable ? Theme.Input : Theme.Raised;
                editor.ForeColor = Theme.Text;
                ToolTip.SetToolTip(editor, field.IsWritable ? field.FieldName : $"{field.FieldName} (read-only)");
            }
            else
            {
                editor.Text = string.Empty;
                editor.ReadOnly = true;
                editor.BackColor = Theme.Raised;
                editor.ForeColor = Theme.Muted;
                ToolTip.SetToolTip(editor, $"{fieldName} is not present in this database");
            }
        }

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
        var box = LegacyGroup("Nation and National Team Audio", new Point(3, 3), new Size(710, 390));
        var fields = new[]
        {
            ("PA Language", "palanguageindex"),
            ("Default Commentary Language", "defaultcommlang"),
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
            var y = 28 + (row * 43);
            box.Controls.Add(new Label
            {
                Text = fields[index].Item1, Location = new Point(x, y + 3),
                Size = new Size(165, 20), Font = LegacyFont
            });
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
            Location = new Point(16, 303), Size = new Size(660, 45),
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
                editor.BackColor = Theme.Raised;
                editor.ForeColor = Theme.Muted;
                continue;
            }
            editor.Text = Services.Session.GetCell("audionation", row, field);
            var table = Services.Session.GetTable("audionation");
            var column = table?.Columns?.FirstOrDefault(x => x.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
            editor.ReadOnly = column?.IsWritable != true;
            editor.BackColor = editor.ReadOnly ? Theme.Raised : Theme.Input;
            editor.ForeColor = Theme.Text;
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

    private GroupBox LegacyGroup(string text, Point location, Size size) => new()
    {
        Text = text, Location = location, Size = size, Font = LegacyFont,
        BackColor = Theme.Panel, ForeColor = Theme.Text
    };

    private void AddField(Control parent, string fieldName, string label, Point location, int width)
    {
        parent.Controls.Add(new Label { Text = label, Location = new Point(11, location.Y + 3), Size = new Size(location.X - 16, 18), Font = LegacyFont, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Theme.Text, BackColor = Theme.Panel });
        var editor = new TextBox { Location = location, Size = new Size(width, 20), Font = LegacyFont, Tag = fieldName, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(editor);
        editor.Leave += (_, _) => Commit(editor);
        parent.Controls.Add(editor);
        _editors.Add(editor);
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
        if (!string.Equals(editor.Text.Trim(), field.Value, StringComparison.Ordinal)) StageField(TableName, CurrentRecordIndex, fieldName, editor.Text.Trim(), _stagingGrid);
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
