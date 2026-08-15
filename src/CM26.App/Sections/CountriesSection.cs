using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>
/// FC26 data adapter presented with the Studio dark card layout.
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

    private StudioToolbar? _toolbar;
    private StudioCard? _heroCard;
    private FlowLayoutPanel? _detailsFlow;
    private StudioCard? _flagsCard;
    private StudioCard? _actionsCard;
    private StudioCard? _mapCard;
    private StudioCard? _fieldsCard;
    private TableLayoutPanel? _fieldsTable;

    public override string SectionKey => "countries";
    public override string SectionTitle => "Countries";
    protected override string TableName => "nations";
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search countries…";
    protected override bool ShowRecordCommandStrip => false;

    public CountriesSection(AppServices services) : base(services)
    {
        Header.Visible = false;
        EmptyState.Visible = false;
        Tabs.BringToFront();
        Tabs.Font = LegacyFont;
        Tabs.Padding = new Point(4, 2);
        Theme.ApplyTabs(Tabs);

        _mapViewer = new PictureBox
        {
            Size = new Size(512, 256),
            BackColor = StudioColors.InputBackground,
            BorderStyle = BorderStyle.None,
            SizeMode = PictureBoxSizeMode.Zoom,
        };

        AddOverviewTab();
        AddNationalAudioTab();
    }

    private void AddOverviewTab()
    {
        var page = new TabPage("General") { BackColor = StudioColors.AppBackground, Font = LegacyFont };

        _toolbar = new StudioToolbar
        {
            Title = "Countries",
            CanCreate = true,
            ShowFilter = true,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchBox.PlaceholderText = RecordSearchPlaceholder;
        _toolbar.NewClicked += (_, _) => CreateNewRecord();
        _toolbar.PreviousClicked += (_, _) => StepRecord(-1);
        _toolbar.NextClicked += (_, _) => StepRecord(+1);
        _toolbar.SearchClicked += (_, _) => SearchCountries(_toolbar.SearchText);
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            SearchCountries(_toolbar.SearchText);
        };
        _toolbar.FilterClicked += (_, _) => FocusSearchBox();
        page.Controls.Add(_toolbar);

        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(StudioSpacing.Medium),
            AutoScroll = true,
        };
        page.Controls.Add(scrollPanel);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = StudioColors.AppBackground,
        };

        _heroCard = BuildHeroCard();
        layout.Controls.Add(_heroCard, 0, 0);

        _detailsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
            Margin = new Padding(0, StudioSpacing.Medium, 0, 0),
        };
        _detailsFlow.Controls.Add(BuildMetric("Nation ID", "—", StudioColors.CyanAccent));
        _detailsFlow.Controls.Add(BuildMetric("ISO Code", "—", StudioColors.Purple));
        _detailsFlow.Controls.Add(BuildMetric("Confederation", "—", StudioColors.Green));
        _detailsFlow.Controls.Add(BuildMetric("Level", "—", StudioColors.Yellow));
        layout.Controls.Add(_detailsFlow, 0, 1);

        _flagsCard = BuildFlagsCard();
        layout.Controls.Add(_flagsCard, 0, 2);

        _actionsCard = BuildActionsCard();
        layout.Controls.Add(_actionsCard, 0, 3);

        _mapCard = BuildMapCard();
        layout.Controls.Add(_mapCard, 0, 4);

        _fieldsCard = BuildFieldsCard();
        layout.Controls.Add(_fieldsCard, 0, 5);

        scrollPanel.Controls.Add(layout);
        Tabs.TabPages.Add(page);
    }

    private StudioCard BuildHeroCard()
    {
        var card = new StudioCard
        {
            Dock = DockStyle.Top,
            Height = 160,
            AccentColor = StudioColors.CyanAccent,
        };

        _countryFlagPreview.Size = new Size(120, 120);
        _countryFlagPreview.Location = new Point(StudioSpacing.Large, StudioSpacing.Large);
        _countryFlagPreview.SizeMode = PictureBoxSizeMode.Zoom;
        _countryFlagPreview.BackColor = Color.Transparent;
        _countryFlagPreview.BorderStyle = BorderStyle.None;
        card.Controls.Add(_countryFlagPreview);

        _countryNameLabel.Location = new Point(152, StudioSpacing.Large);
        _countryNameLabel.Size = new Size(500, 38);
        _countryNameLabel.Font = StudioFonts.SectionTitle;
        _countryNameLabel.ForeColor = StudioColors.PrimaryText;
        _countryNameLabel.BackColor = Color.Transparent;
        _countryNameLabel.Text = "Country Name";
        card.Controls.Add(_countryNameLabel);

        _countryMetaLabel.Location = new Point(152, 54);
        _countryMetaLabel.Size = new Size(600, 22);
        _countryMetaLabel.Font = StudioFonts.CardSubtitle;
        _countryMetaLabel.ForeColor = StudioColors.MutedText;
        _countryMetaLabel.BackColor = Color.Transparent;
        card.Controls.Add(_countryMetaLabel);

        var addCountry = StudioButton("Add Country", 120);
        addCountry.Location = new Point(152, 90);
        addCountry.Click += (_, _) => CreateNewRecord();
        card.Controls.Add(addCountry);

        var createNationalTeam = StudioButton("Create National Team", 150);
        createNationalTeam.Location = new Point(280, 90);
        createNationalTeam.Click += (_, _) => CreateNationalTeam();
        card.Controls.Add(createNationalTeam);

        _openNationalTeam.Text = "Open National Team";
        _openNationalTeam.Location = new Point(438, 90);
        _openNationalTeam.Size = new Size(150, 28);
        _openNationalTeam.Font = StudioFonts.Button;
        _openNationalTeam.Enabled = false;
        _openNationalTeam.FlatStyle = FlatStyle.Flat;
        _openNationalTeam.BackColor = StudioColors.RaisedSurface;
        _openNationalTeam.ForeColor = StudioColors.PrimaryText;
        _openNationalTeam.Cursor = Cursors.Hand;
        _openNationalTeam.UseVisualStyleBackColor = false;
        _openNationalTeam.Click += (_, _) => OpenLinkedNationalTeam();
        card.Controls.Add(_openNationalTeam);

        return card;
    }

    private StudioCard BuildFlagsCard()
    {
        var card = StudioGroup("Flag Assets", StudioColors.Green);
        card.AutoSize = true;

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent,
        };

        var (largeFlag, largeCaption) = CreateFlagViewer(256, 256, "256 x 256");
        var (crestFlag, crestCaption) = CreateFlagViewer(256, 256, "512 x 512");
        var (cardFlag, cardCaption) = CreateFlagViewer(150, 150, "256 x 128");
        var (miniFlag, miniCaption) = CreateFlagViewer(64, 64, "64 x 64");
        _flagViewers.AddRange([largeFlag, crestFlag, cardFlag, miniFlag]);
        _flagCaptions.AddRange([largeCaption, crestCaption, cardCaption, miniCaption]);

        flow.Controls.Add(largeFlag.Parent);
        flow.Controls.Add(crestFlag.Parent);
        flow.Controls.Add(cardFlag.Parent);
        flow.Controls.Add(miniFlag.Parent);

        LegacyAssetActions.Attach(Services, card, largeFlag, new Point(StudioSpacing.Medium, 310), RefreshCurrentRecord);

        card.Controls.Add(flow);
        return card;
    }

    private static (PictureBox Picture, Label Caption) CreateFlagViewer(int width, int height, string resolution)
    {
        var holder = new Panel
        {
            Size = new Size(width + 16, height + 28),
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, StudioSpacing.Medium, StudioSpacing.Small),
        };
        var picture = new PictureBox
        {
            Location = new Point(8, 4),
            Size = new Size(width, height),
            BackColor = StudioColors.InputBackground,
            BorderStyle = BorderStyle.None,
            SizeMode = PictureBoxSizeMode.Zoom,
        };
        var caption = new Label
        {
            Text = resolution,
            Location = new Point(8, height + 6),
            Size = new Size(width, 18),
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoEllipsis = true,
        };
        holder.Controls.Add(picture);
        holder.Controls.Add(caption);
        return (picture, caption);
    }

    private StudioCard BuildActionsCard()
    {
        var card = StudioGroup("National Team", StudioColors.CyanAccent);
        card.Height = 110;

        var info = new Label
        {
            Text = "Create or open the national team linked to this country.",
            Location = new Point(StudioSpacing.Medium, 34),
            Size = new Size(520, 20),
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(info);
        return card;
    }

    private StudioCard BuildMapCard()
    {
        var card = StudioGroup("Map (Shape)", StudioColors.Purple);
        card.Height = 360;

        _mapViewer.Location = new Point(StudioSpacing.Medium, 34);
        card.Controls.Add(_mapViewer);

        LegacyAssetActions.Attach(Services, card, _mapViewer, new Point(StudioSpacing.Medium, 302), RefreshCurrentRecord);
        return card;
    }

    private StudioCard BuildFieldsCard()
    {
        var card = StudioGroup("Database Fields", StudioColors.Yellow);
        card.AutoSize = true;

        _fieldsTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
        };
        _fieldsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
        _fieldsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));

        AddField("nationname", "Database Name");
        AddField("nationid", "Country ID");
        AddMirrorField("nationname", "Name");
        AddField("nationstartingfirstletter", "Starting Letter");
        AddField("isocountrycode", "Abbreviation");
        AddField("confederation", "Confederation");
        AddMirrorField("isocountrycode", "ISO Country Code");
        AddField("groupid", "Level");
        AddField("streetdressing", "Street Dressing");

        _topTier.Text = "Top tier";
        _topTier.AutoSize = true;
        _topTier.Font = LegacyFont;
        _topTier.BackColor = Color.Transparent;
        _topTier.ForeColor = StudioColors.PrimaryText;
        _topTier.FlatStyle = FlatStyle.Flat;
        _topTier.Tag = "top_tier";
        _topTier.CheckedChanged += (_, _) =>
        {
            if (_syncTopTier || CurrentRecordIndex < 0 || !_fields.TryGetValue("top_tier", out var value) || !value.IsWritable) return;
            StageField(TableName, CurrentRecordIndex, "top_tier", _topTier.Checked ? "1" : "0", _stagingGrid);
        };
        AddFullWidthControl(_topTier);

        _showAllDatabaseCountries.Text = "Show countries awaiting setup";
        _showAllDatabaseCountries.AutoSize = true;
        _showAllDatabaseCountries.Font = LegacyFont;
        _showAllDatabaseCountries.BackColor = Color.Transparent;
        _showAllDatabaseCountries.ForeColor = StudioColors.PrimaryText;
        _showAllDatabaseCountries.FlatStyle = FlatStyle.Flat;
        _showAllDatabaseCountries.CheckedChanged += (_, _) => { if (_suppressListReload) return; LoadData(); };
        ToolTip.SetToolTip(_showAllDatabaseCountries, "Off: show only playable countries. On: also show database countries that still need a league, clubs and Compdata.");
        AddFullWidthControl(_showAllDatabaseCountries);

        var hint = new Label
        {
            Text = "Create a country ID, then add its national team, domestic league and clubs before a Career save.",
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
            AutoSize = true,
            Margin = new Padding(0, StudioSpacing.Small, 0, 0),
        };
        AddFullWidthControl(hint);

        card.Controls.Add(_fieldsTable);
        return card;
    }

    private void AddField(string fieldName, string label)
    {
        if (_fieldsTable == null) return;
        var row = _fieldsTable.RowCount;
        _fieldsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var caption = new Label
        {
            Text = label,
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 0, StudioSpacing.Small),
        };
        var editor = new TextBox
        {
            Font = LegacyFont,
            Tag = fieldName,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, StudioSpacing.Small),
        };
        Theme.ApplyTextBox(editor);
        editor.Leave += (_, _) => Commit(editor);
        _editors.Add(editor);

        _fieldsTable.Controls.Add(caption, 0, row);
        _fieldsTable.Controls.Add(editor, 1, row);
    }

    private void AddMirrorField(string fieldName, string label)
    {
        if (_fieldsTable == null) return;
        var row = _fieldsTable.RowCount;
        _fieldsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var caption = new Label
        {
            Text = label,
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 0, StudioSpacing.Small),
        };
        var editor = new TextBox
        {
            Font = LegacyFont,
            Tag = fieldName,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, StudioSpacing.Small),
        };
        Theme.ApplyTextBox(editor);
        editor.BackColor = StudioColors.RaisedSurface;
        editor.ForeColor = StudioColors.PrimaryText;
        _editors.Add(editor);
        _mirrors.Add(editor);

        _fieldsTable.Controls.Add(caption, 0, row);
        _fieldsTable.Controls.Add(editor, 1, row);
    }

    private void AddFullWidthControl(Control control)
    {
        if (_fieldsTable == null) return;
        var row = _fieldsTable.RowCount;
        _fieldsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fieldsTable.SetColumnSpan(control, 2);
        _fieldsTable.Controls.Add(control, 0, row);
    }

    private void AddNationalAudioTab()
    {
        var page = new TabPage("National Team Audio") { BackColor = StudioColors.AppBackground, Font = LegacyFont };
        var canvas = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(StudioSpacing.Medium),
        };
        page.Controls.Add(canvas);
        Tabs.TabPages.Add(page);

        var card = StudioGroup("Nation and National Team Audio", StudioColors.Green);
        card.Width = 720;
        card.Height = 280;

        var table = new TableLayoutPanel
        {
            Location = new Point(StudioSpacing.Medium, 34),
            Size = new Size(688, 200),
            ColumnCount = 4,
            RowCount = 6,
            BackColor = Color.Transparent,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27f));

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
            ("Team Can Whistle", "teamcanwhistleindex"),
        };

        for (var index = 0; index < fields.Length; index++)
        {
            var col = (index % 2) * 2;
            var row = index / 2;
            var label = new Label
            {
                Text = fields[index].Item1,
                Font = StudioFonts.DataLabel,
                ForeColor = StudioColors.MutedText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            };
            var editor = new TextBox
            {
                Font = LegacyFont,
                Tag = fields[index].Item2,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
            };
            Theme.ApplyTextBox(editor);
            editor.Leave += (_, _) => CommitNationalAudio(editor);
            _audioEditors.Add(editor);
            ToolTip.SetToolTip(label, fields[index].Item1);
            table.Controls.Add(label, col, row);
            table.Controls.Add(editor, col + 1, row);
        }

        var description = new Label
        {
            Text = "Audio mappings for the selected country. They control regional commentary and crowd banks.",
            Location = new Point(StudioSpacing.Medium, 240),
            Size = new Size(660, 30),
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        card.Controls.Add(description);
        card.Controls.Add(table);
        canvas.Controls.Add(card);
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
        if (!Services.Session.IsLoaded)
        {
            _toolbar.RecordCountText = "0 records";
            return;
        }
        try
        {
            _toolbar.RecordCountText = $"{GetRecords().Count:N0} records";
        }
        catch
        {
            _toolbar.RecordCountText = string.Empty;
        }
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
        if (next >= 0 && next < records.Count) GoToRecord(records[next].RecordIndex);
    }

    private void SearchCountries(string query)
    {
        var term = query.Trim();
        if (term.Length == 0) return;
        var result = GetRecords().FirstOrDefault(item => item.Matches(term));
        if (result == null)
        {
            MessageBox.Show(this, $"No country matches '{term}'.", "Search Country",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        GoToRecord(result.RecordIndex);
    }

    protected override void CreateNewRecord()
    {
        if (!EntityCreationDialog.TryShow(this, "Country",
                [("Country name", "New Country"), ("ISO code", "NC")], out var values))
            return;
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
        var table = Services.Session.GetTable(TableName);
        var record = table == null ? null : Services.Session.GetRecord(TableName, recordIndex);
        if (table == null || record == null)
        {
            _countryNameLabel.Text = "Country unavailable";
            return;
        }
        _fields.Clear();
        foreach (var field in Services.RequireData().GetFields(TableName, recordIndex, LabelMaps.Nations)) _fields[field.FieldName] = field;

        foreach (var editor in _editors)
        {
            var fieldName = editor.Tag as string ?? string.Empty;
            if (_fields.TryGetValue(fieldName, out var field))
            {
                editor.Text = field.Value;
                editor.ReadOnly = !field.IsWritable;
                editor.BackColor = field.IsWritable ? StudioColors.InputBackground : StudioColors.RaisedSurface;
                editor.ForeColor = StudioColors.PrimaryText;
                ToolTip.SetToolTip(editor, field.IsWritable ? field.FieldName : $"{field.FieldName} (read-only)");
            }
            else
            {
                editor.Text = string.Empty;
                editor.ReadOnly = true;
                editor.BackColor = StudioColors.RaisedSurface;
                editor.ForeColor = StudioColors.MutedText;
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

        _countryNameLabel.Text = nationName ?? string.Empty;
        _countryMetaLabel.Text = $"Nation ID {record.Get(Col(table, "nationid"))}  ·  {record.Get(Col(table, "isocountrycode"))}  ·  {record.Get(Col(table, "confederation"))}";

        if (_detailsFlow != null)
        {
            var metrics = _detailsFlow.Controls.OfType<MetricCard>().ToList();
            SetMetric(metrics, "Nation ID", record.Get(Col(table, "nationid")));
            SetMetric(metrics, "ISO Code", record.Get(Col(table, "isocountrycode")));
            SetMetric(metrics, "Confederation", record.Get(Col(table, "confederation")));
            SetMetric(metrics, "Level", record.Get(Col(table, "groupid")));
        }

        UpdateToolbarCount();
    }

    private static MetricCard BuildMetric(string label, string value, Color accent)
    {
        return new MetricCard
        {
            Width = 140,
            Height = 76,
            Margin = new Padding(0, 0, StudioSpacing.Medium, StudioSpacing.Small),
            AccentColor = accent,
            LabelText = label,
            ValueText = value,
        };
    }

    private static void SetMetric(List<MetricCard> metrics, string label, string? value)
    {
        var metric = metrics.FirstOrDefault(m => m.LabelText == label);
        if (metric != null) metric.ValueText = string.IsNullOrWhiteSpace(value) ? "—" : value;
    }

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
                editor.BackColor = StudioColors.RaisedSurface;
                editor.ForeColor = StudioColors.MutedText;
                continue;
            }
            editor.Text = Services.Session.GetCell("audionation", row, field);
            var table = Services.Session.GetTable("audionation");
            var column = table?.Columns?.FirstOrDefault(x => x.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
            editor.ReadOnly = column?.IsWritable != true;
            editor.BackColor = editor.ReadOnly ? StudioColors.RaisedSurface : StudioColors.InputBackground;
            editor.ForeColor = StudioColors.PrimaryText;
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

    private static StudioCard StudioGroup(string title, Color accent)
    {
        var card = new StudioCard
        {
            AccentColor = accent,
            Margin = new Padding(0, StudioSpacing.Medium, 0, 0),
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

    private static Button StudioButton(string text, int width)
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
        btn.FlatAppearance.MouseDownBackColor = StudioColors.CardBorder;
        return btn;
    }

    private void RefreshMirrors()
    {
        foreach (var mirror in _mirrors)
        {
            var fieldName = mirror.Tag as string ?? string.Empty;
            mirror.ReadOnly = true;
            mirror.BackColor = StudioColors.RaisedSurface;
            mirror.ForeColor = StudioColors.PrimaryText;
            ToolTip.SetToolTip(mirror, $"Read-only mirror of {fieldName} — edit it in its named field above.");
            if (_fields.TryGetValue(fieldName, out var field))
                mirror.Text = field.Value;
        }
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
                if (!_countryFlagPreview.IsDisposed)
                {
                    var previousHeader = _countryFlagPreview.Image;
                    _countryFlagPreview.Image = new Bitmap(image);
                    previousHeader?.Dispose();
                }
                foreach (var label in _flagCaptions)
                {
                    if (label.IsDisposed) continue;
                    label.Text = source ?? "Flag preview";
                }
            }
            else
            {
                if (!_countryFlagPreview.IsDisposed)
                {
                    _countryFlagPreview.Image?.Dispose();
                    _countryFlagPreview.Image = null;
                }
                foreach (var label in _flagCaptions)
                {
                    if (label.IsDisposed) continue;
                    label.Text = label.Width < 100 ? "Unavailable" : $"No flag available ({nationId})";
                    ToolTip.SetToolTip(label, $"No flag asset is available for country {nationId}.");
                }
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
