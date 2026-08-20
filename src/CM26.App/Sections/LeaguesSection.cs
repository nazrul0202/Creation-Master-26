using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;
using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>
/// FC26 adapter using the Studio dark card layout. The linked CM16Source/LeagueForm.cs
/// remains the authoritative layout source.
/// </summary>
public sealed class LeaguesSection : SectionBase
{
    private static readonly Font LegacyFont = Theme.Body;
    private static readonly Color DarkCrestTile = Color.FromArgb(20, 42, 63);
    private readonly List<TextBox> _editors = [];
    private readonly List<TextBox> _nameMirrors = [];
    private readonly Dictionary<string, FieldValue> _fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _stagingGrid = new();
    private readonly HashSet<int> _pendingTeamCrests = [];
    private readonly SemaphoreSlim _teamCrestGate = new(2, 2);
    private readonly ComboBox _countryPicker = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Dictionary<string, CheckBox> _leagueFlags = new(StringComparer.OrdinalIgnoreCase);
    private readonly ComboBox _teamPicker = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _teamSearch = new();
    private readonly Button _addTeam = new();
    private readonly Button _removeTeam = new();
    private readonly Button _findTeam = new();
    private readonly Button _newLeagueTeam = new();
    private readonly Label _leagueNameLabel = new();
    private readonly Label _leagueMetaLabel = new();
    private readonly PictureBox _leagueLogoPreview = new();
    private readonly Label _leagueOverallLabel = new();
    private readonly Label _leagueAttVal = new();
    private readonly Label _leagueMidVal = new();
    private readonly Label _leagueDefVal = new();
    private readonly Panel _leagueAttBar = new();
    private readonly Panel _leagueMidBar = new();
    private readonly Panel _leagueDefBar = new();
    private int _leagueId;
    private bool _syncLeagueFlags;
    private bool _syncCountryPicker;

    private StudioToolbar? _toolbar;
    private StudioCard? _heroCard;
    private FlowLayoutPanel? _quickFlow;
    private StudioCard? _infoCard;
    private StudioCard? _settingsCard;
    private StudioCard? _clubsCard;
    private FlowLayoutPanel? _clubsFlow;
    private LeagueClubTile? _selectedClubTile;

    public override string SectionKey => "leagues";
    public override string SectionTitle => "Leagues";
    protected override string TableName => "leagues";
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search leagues…";
    protected override bool ShowRecordCommandStrip => false;

    public LeaguesSection(AppServices services) : base(services)
    {
        Header.Visible = false;
        EmptyState.Visible = false;
        Tabs.BringToFront();
        Tabs.Font = LegacyFont;
        Tabs.Padding = new Point(4, 2);
        Theme.ApplyTabs(Tabs);
        AddOverviewTab();
    }

    private void AddOverviewTab()
    {
        var page = new TabPage("General") { BackColor = StudioColors.AppBackground, Font = LegacyFont };

        _toolbar = new StudioToolbar
        {
            Title = "Leagues",
            CanCreate = true,
            ShowFilter = true,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchBox.PlaceholderText = RecordSearchPlaceholder;
        _toolbar.NewClicked += (_, _) => CreateNewRecord();
        _toolbar.PreviousClicked += (_, _) => StepRecord(-1);
        _toolbar.NextClicked += (_, _) => StepRecord(+1);
        _toolbar.SearchClicked += (_, _) => SearchLeagues(_toolbar.SearchText);
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            SearchLeagues(_toolbar.SearchText);
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

        _quickFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
            Margin = new Padding(0, StudioSpacing.Medium, 0, 0),
        };
        _quickFlow.Controls.Add(BuildMetric("Level", "—", StudioColors.CyanAccent));
        _quickFlow.Controls.Add(BuildMetric("Clubs", "—", StudioColors.Green));
        _quickFlow.Controls.Add(BuildMetric("Country", "—", StudioColors.Purple));
        _quickFlow.Controls.Add(BuildMetric("Prestige", "—", StudioColors.Yellow));
        layout.Controls.Add(_quickFlow, 0, 1);

        var infoSettingsRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
            Margin = new Padding(0, StudioSpacing.Medium, 0, 0),
        };
        _infoCard = BuildInfoCard();
        _settingsCard = BuildSettingsCard();
        infoSettingsRow.Controls.Add(_infoCard);
        infoSettingsRow.Controls.Add(_settingsCard);
        layout.Controls.Add(infoSettingsRow, 0, 2);

        _clubsCard = BuildClubsCard();
        layout.Controls.Add(_clubsCard, 0, 3);

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

        _leagueLogoPreview.Size = new Size(120, 120);
        _leagueLogoPreview.Location = new Point(StudioSpacing.Large, StudioSpacing.Large);
        _leagueLogoPreview.SizeMode = PictureBoxSizeMode.Zoom;
        _leagueLogoPreview.BackColor = Color.Transparent;
        _leagueLogoPreview.BorderStyle = BorderStyle.None;
        card.Controls.Add(_leagueLogoPreview);

        var import = new Button { Text = "Import", Location = new Point(StudioSpacing.Large, 138), Size = new Size(58, 21) };
        var remove = new Button { Text = "Remove", Location = new Point(StudioSpacing.Large + 62, 138), Size = new Size(58, 21) };
        var export = new Button { Text = "Export", Location = new Point(StudioSpacing.Large + 124, 138), Size = new Size(58, 21) };
        Theming.Theme.ApplyButton(import);
        Theming.Theme.ApplyButton(remove);
        Theming.Theme.ApplyButton(export);
        import.Click += (_, _) => ImportLeagueLogo();
        remove.Click += (_, _) => RemoveLeagueLogo();
        export.Click += (_, _) => ExportLeagueLogo();
        card.Controls.Add(import);
        card.Controls.Add(remove);
        card.Controls.Add(export);

        _leagueNameLabel.Location = new Point(152, StudioSpacing.Large);
        _leagueNameLabel.Size = new Size(500, 38);
        _leagueNameLabel.Font = StudioFonts.SectionTitle;
        _leagueNameLabel.ForeColor = StudioColors.PrimaryText;
        _leagueNameLabel.BackColor = Color.Transparent;
        _leagueNameLabel.Text = "League Name";
        card.Controls.Add(_leagueNameLabel);

        _leagueMetaLabel.Location = new Point(152, 54);
        _leagueMetaLabel.Size = new Size(500, 22);
        _leagueMetaLabel.Font = StudioFonts.CardSubtitle;
        _leagueMetaLabel.ForeColor = StudioColors.MutedText;
        _leagueMetaLabel.BackColor = Color.Transparent;
        card.Controls.Add(_leagueMetaLabel);

        _leagueOverallLabel.Text = "—";
        _leagueOverallLabel.Location = new Point(700, StudioSpacing.Large);
        _leagueOverallLabel.Size = new Size(80, 50);
        _leagueOverallLabel.Font = StudioFonts.MetricValue;
        _leagueOverallLabel.TextAlign = ContentAlignment.MiddleCenter;
        _leagueOverallLabel.ForeColor = StudioColors.Green;
        _leagueOverallLabel.BackColor = Color.Transparent;
        card.Controls.Add(_leagueOverallLabel);

        var ovrCaption = new Label
        {
            Text = "OVR",
            Location = new Point(700, 70),
            Size = new Size(80, 18),
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        card.Controls.Add(ovrCaption);

        AddRatingBar(card, "ATT", StudioColors.Yellow, _leagueAttBar, _leagueAttVal, 540, 92);
        AddRatingBar(card, "MID", StudioColors.CyanAccent, _leagueMidBar, _leagueMidVal, 540, 116);
        AddRatingBar(card, "DEF", StudioColors.Red, _leagueDefBar, _leagueDefVal, 540, 140);

        card.Resize += (_, _) =>
        {
            _leagueOverallLabel.Location = new Point(Math.Max(560, card.Width - 130), StudioSpacing.Large);
            ovrCaption.Location = new Point(_leagueOverallLabel.Left, 70);
        };

        return card;
    }

    private static void AddRatingBar(Control parent, string label, Color accent, Panel barFill, Label valueLabel, int x, int y)
    {
        var lbl = new Label
        {
            Text = label,
            Location = new Point(x, y),
            Size = new Size(36, 18),
            Font = StudioFonts.DataLabel,
            ForeColor = accent,
            BackColor = Color.Transparent,
        };
        parent.Controls.Add(lbl);
        var track = new Panel
        {
            Location = new Point(x + 40, y + 3),
            Size = new Size(120, 10),
            BackColor = StudioColors.CardBorder,
        };
        barFill.Location = Point.Empty;
        barFill.Size = new Size(1, 10);
        barFill.BackColor = accent;
        barFill.Tag = accent;
        track.Controls.Add(barFill);
        parent.Controls.Add(track);
        valueLabel.Location = new Point(x + 166, y - 2);
        valueLabel.Size = new Size(32, 18);
        valueLabel.Font = StudioFonts.DataValue;
        valueLabel.ForeColor = StudioColors.PrimaryText;
        valueLabel.BackColor = Color.Transparent;
        valueLabel.TextAlign = ContentAlignment.MiddleRight;
        parent.Controls.Add(valueLabel);
    }

    private static void SetRatingBar(Panel bar, string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var num) || num <= 0)
        {
            bar.Width = Math.Max(8, (bar.Parent?.Width - 2 ?? 120) / 10);
            bar.BackColor = Color.FromArgb(60, bar.BackColor);
            return;
        }
        if (bar.Tag is Color accent) bar.BackColor = accent;
        bar.Width = Math.Max(8, Math.Min(bar.Parent?.Width - 2 ?? 120, (int)((double)num / max * 120)));
    }

    private StudioCard BuildInfoCard()
    {
        var card = StudioGroup("League Information", StudioColors.Green);
        card.Width = 420;
        card.Height = 210;

        var table = new TableLayoutPanel
        {
            Location = new Point(StudioSpacing.Medium, 34),
            Size = new Size(388, 160),
            ColumnCount = 2,
            BackColor = Color.Transparent,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));

        AddField(table, "leaguename", "Database Name");
        AddMirrorField(table, "leaguename", "Name");
        AddField(table, "leagueid", "League ID");
        AddField(table, "level", "Level");
        AddCountryPicker(table);
        AddField(table, "leaguetype", "Prestige");

        card.Controls.Add(table);
        return card;
    }

    private StudioCard BuildSettingsCard()
    {
        var card = StudioGroup("League Settings", StudioColors.Yellow);
        card.Width = 420;
        card.Height = 210;

        var flags = new[]
        {
            ("Women's competition", "iswomencompetition"),
            ("International league", "isinternationalleague"),
            ("Competition pole flags", "iscompetitionpoleflagenabled"),
            ("Within transfer window", "iswithintransferwindow"),
            ("Competition scarves", "iscompetitionscarfenabled"),
            ("Crowd cards", "iscompetitioncrowdcardsenabled"),
            ("Banner enabled", "isbannerenabled"),
        };

        var table = new TableLayoutPanel
        {
            Location = new Point(StudioSpacing.Medium, 34),
            Size = new Size(388, 160),
            ColumnCount = 2,
            BackColor = Color.Transparent,
        };
        for (var i = 0; i < flags.Length; i++)
        {
            var col = i % 2;
            var row = i / 2;
            AddLeagueFlag(table, flags[i].Item1, flags[i].Item2, col, row);
        }
        card.Controls.Add(table);
        return card;
    }

    private StudioCard BuildClubsCard()
    {
        var card = StudioGroup("Clubs", StudioColors.Green);
        card.Height = 360;

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 38,
            BackColor = Color.Transparent,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, StudioSpacing.Small),
        };

        _teamPicker.Width = 180;
        _teamPicker.Font = LegacyFont;
        Theme.ApplyCombo(_teamPicker);
        toolbar.Controls.Add(_teamPicker);

        _addTeam.Text = "Add";
        _addTeam.Size = new Size(72, 28);
        _addTeam.FlatStyle = FlatStyle.Flat;
        _addTeam.BackColor = StudioColors.Green;
        _addTeam.ForeColor = StudioColors.AppBackground;
        _addTeam.Font = StudioFonts.Button;
        _addTeam.Cursor = Cursors.Hand;
        _addTeam.UseVisualStyleBackColor = false;
        _addTeam.Click += (_, _) => AddSelectedTeam();
        toolbar.Controls.Add(_addTeam);

        _removeTeam.Text = "Remove";
        _removeTeam.Size = new Size(72, 28);
        _removeTeam.FlatStyle = FlatStyle.Flat;
        _removeTeam.BackColor = StudioColors.Red;
        _removeTeam.ForeColor = StudioColors.PrimaryText;
        _removeTeam.Font = StudioFonts.Button;
        _removeTeam.Cursor = Cursors.Hand;
        _removeTeam.UseVisualStyleBackColor = false;
        _removeTeam.Enabled = false;
        _removeTeam.Click += (_, _) => RemoveSelectedTeam();
        toolbar.Controls.Add(_removeTeam);

        _teamSearch.Width = 160;
        _teamSearch.Height = 28;
        _teamSearch.PlaceholderText = "Find club to add…";
        _teamSearch.Font = LegacyFont;
        Theme.ApplyTextBox(_teamSearch);
        _teamSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            FindTeams();
            e.SuppressKeyPress = true;
        };
        toolbar.Controls.Add(_teamSearch);

        _findTeam.Text = "Find";
        _findTeam.Size = new Size(64, 28);
        _findTeam.FlatStyle = FlatStyle.Flat;
        _findTeam.BackColor = StudioColors.RaisedSurface;
        _findTeam.ForeColor = StudioColors.PrimaryText;
        _findTeam.Font = StudioFonts.Button;
        _findTeam.Cursor = Cursors.Hand;
        _findTeam.UseVisualStyleBackColor = false;
        _findTeam.Click += (_, _) => FindTeams();
        toolbar.Controls.Add(_findTeam);

        _newLeagueTeam.Text = "New Club";
        _newLeagueTeam.Size = new Size(90, 28);
        _newLeagueTeam.FlatStyle = FlatStyle.Flat;
        _newLeagueTeam.BackColor = StudioColors.CyanAccent;
        _newLeagueTeam.ForeColor = StudioColors.AppBackground;
        _newLeagueTeam.Font = StudioFonts.Button;
        _newLeagueTeam.Cursor = Cursors.Hand;
        _newLeagueTeam.UseVisualStyleBackColor = false;
        _newLeagueTeam.Click += (_, _) => CreateAndLinkTeam();
        toolbar.Controls.Add(_newLeagueTeam);

        _clubsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent,
        };

        card.Controls.Add(_clubsFlow);
        card.Controls.Add(toolbar);
        return card;
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

    protected override void CreateNewRecord()
    {
        var defaultCountryId = _fields.TryGetValue("countryid", out var currentCountry)
            ? currentCountry.RawValue : "0";
        if (!EntityCreationDialog.TryShow(this, "League",
                [("League name", "New League"), ("Country ID", defaultCountryId)], out var values))
            return;
        if (!int.TryParse(values[1], out var countryId) || !NationExists(countryId))
        {
            MessageBox.Show(this, "Enter an existing Country ID. Create the country first with Add Country if needed.",
                "Create League", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            var id = CreateRecordFromTemplate(TableName, "leagueid", new Dictionary<string, string>
            {
                ["leaguename"] = values[0],
                ["countryid"] = countryId.ToString(),
            });
            Services.Session.RefreshSchema();
            Services.RefreshDatabaseIndexes();
            LoadData();
            var created = GetRecords().FirstOrDefault(item =>
                Parse(Services.Session.GetCell(TableName, item.RecordIndex, "leagueid")) == id);
            if (created != null) GoToRecord(created.RecordIndex);
            MessageBox.Show(this, $"League created with ID {id} and assigned to Country ID {countryId}.",
                "Create League", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create League", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool NationExists(int nationId)
    {
        var nations = Services.Session.GetTable("nations");
        if (nations == null) return false;
        var idColumn = Col(nations, "nationid");
        return idColumn >= 0 && Enumerable.Range(0, nations.RowCount)
            .Select(row => Services.Session.GetRecord("nations", row)?.Get(idColumn))
            .Any(value => int.TryParse(value, out var id) && id == nationId);
    }

    private void OpenSelectedTeam()
    {
        if (_selectedClubTile == null) return;
        var teamId = _selectedClubTile.TeamId;
        if (teamId <= 0) return;
        var teams = Services.Session.GetTable("teams");
        var teamIdColumn = teams == null ? -1 : Col(teams, "teamid");
        if (teams == null || teamIdColumn < 0) return;
        var row = Enumerable.Range(0, teams.RowCount).FirstOrDefault(index =>
        {
            var record = Services.Session.GetRecord("teams", index);
            return record != null && Parse(record.Get(teamIdColumn)) == teamId;
        }, -1);
        if (row >= 0) Services.RequestRecordNavigation("teams", row);
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Services.RequireData().GetLeagues();

    protected override void ShowRecord(int recordIndex)
    {
        _fields.Clear();
        foreach (var field in Services.RequireData().GetFields(TableName, recordIndex, LabelMaps.Leagues)) _fields[field.FieldName] = field;
        foreach (var editor in _editors) SetEditor(editor);
        RefreshNameMirrors();
        PopulateCountryPicker();
        _syncLeagueFlags = true;
        try
        {
            foreach (var (field, check) in _leagueFlags)
            {
                if (_fields.TryGetValue(field, out var value))
                {
                    check.Checked = value.RawValue != "0";
                    check.Enabled = value.IsWritable;
                    ToolTip.SetToolTip(check, value.IsWritable ? field : $"{field} (read-only)");
                }
                else { check.Checked = false; check.Enabled = false; }
            }
        }
        finally { _syncLeagueFlags = false; }

        var table = Services.Session.GetTable(TableName)!;
        var record = Services.Session.GetRecord(TableName, recordIndex)!;
        var name = record.Get(Col(table, "leaguename"));
        var leagueId = Parse(record.Get(Col(table, "leagueid")));
        var logo = Services.Assets.GetLeagueLogo(leagueId);

        _leagueNameLabel.Text = name ?? string.Empty;
        _leagueMetaLabel.Text = $"{ResolveCountryName()} · Level {record.Get(Col(table, "level"))}";
        LoadLeagueLogo(leagueId, logo);

        var ovr = record.Get(Col(table, "overallrating")) ?? "0";
        _leagueOverallLabel.Text = ovr;
        _leagueOverallLabel.ForeColor = StudioColors.RatingColor(int.TryParse(ovr, out var ovrNum) ? ovrNum : 0);
        SetRatingBar(_leagueAttBar, record.Get(Col(table, "attackrating")), 99);
        _leagueAttVal.Text = record.Get(Col(table, "attackrating")) ?? "—";
        SetRatingBar(_leagueMidBar, record.Get(Col(table, "midfieldrating")), 99);
        _leagueMidVal.Text = record.Get(Col(table, "midfieldrating")) ?? "—";
        SetRatingBar(_leagueDefBar, record.Get(Col(table, "defenserating")), 99);
        _leagueDefVal.Text = record.Get(Col(table, "defenserating")) ?? "—";

        if (_quickFlow != null)
        {
            var metrics = _quickFlow.Controls.OfType<MetricCard>().ToList();
            SetMetric(metrics, "Level", record.Get(Col(table, "level")));
            SetMetric(metrics, "Country", ResolveCountryName());
            SetMetric(metrics, "Prestige", record.Get(Col(table, "leaguetype")));
        }

        _leagueId = leagueId;
        ClearClubTiles();
        PopulateTeamLinks();
        PopulateTeamPicker();
        var clubCount = _clubsFlow?.Controls.Count ?? 0;
        if (_quickFlow != null) SetMetric(_quickFlow.Controls.OfType<MetricCard>().ToList(), "Clubs", clubCount > 0 ? clubCount.ToString() : "—");
        UpdateToolbarCount();
    }

    private void LoadLeagueLogo(int leagueId, string localPath)
    {
        // A staged replacement always wins over the installed asset.
        var staged = LeagueLogoCatalog.PreviewSource(Services.FrostbiteAssets, Services.LegacyMods, leagueId)
            ?? localPath;
        var candidates = new[]
        {
            $"data/ui/imgAssets/league/light/l{leagueId}.dds",
            $"data/ui/imgAssets/league512x128/light/l{leagueId}.dds",
            $"data/ui/imgAssets/leaguelogos_tiny/light/l{leagueId}.dds",
            $"data/ui/imgAssets/league/dark/l{leagueId}.dds",
            $"data/ui/imgAssets/league/l{leagueId}.dds"
        };
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(_leagueLogoPreview, Services, staged, candidates,
            (image, _) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                var old = _leagueLogoPreview.Image;
                _leagueLogoPreview.Image = image;
                old?.Dispose();
            });
    }

    private void ImportLeagueLogo()
    {
        if (_leagueId <= 0) return;
        var editable = LeagueLogoCatalog.EditablePaths(
            Services.FrostbiteAssets, Services.LegacyMods, _leagueId);
        if (editable.Count == 0)
        {
            MessageBox.Show(FindForm(),
                "This league has no installed logo in FC26, so there is nothing to replace.",
                "Import League Logo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new OpenFileDialog
        {
            Title = "Import League Logo",
            Filter = "Image files (*.dds;*.png;*.jpg;*.jpeg;*.bmp)|*.dds;*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            LeagueLogoCatalog.StageAll(Services.LegacyMods, editable, _leagueId, dialog.FileName);
            LoadLeagueLogo(_leagueId, null);
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Import League Logo",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RemoveLeagueLogo()
    {
        if (_leagueId <= 0) return;
        try
        {
            if (LeagueLogoCatalog.RemoveAll(Services.LegacyMods, _leagueId))
                LoadLeagueLogo(_leagueId, null);
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Remove League Logo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ExportLeagueLogo()
    {
        if (_leagueId <= 0) return;
        var source = LeagueLogoCatalog.PreviewSource(Services.FrostbiteAssets, Services.LegacyMods, _leagueId);
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
        {
            MessageBox.Show(FindForm(), "No installed or staged league logo is available to export.",
                "Export League Logo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Title = "Export League Logo",
            FileName = $"l{_leagueId}.dds",
            Filter = "DDS texture (*.dds)|*.dds|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try { File.Copy(source, dialog.FileName, overwrite: true); }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Export League Logo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private string ResolveCountryName()
    {
        if (_fields.TryGetValue("countryid", out var f) && int.TryParse(f.RawValue, out var cid))
        {
            var nations = Services.Session.GetTable("nations");
            if (nations != null)
            {
                var nc = Col(nations, "nationid");
                for (int i = 0; i < nations.RowCount; i++)
                {
                    var r = Services.Session.GetRecord("nations", i);
                    if (r != null && int.TryParse(r.Get(nc), out var nid) && nid == cid)
                        return r.Get(Col(nations, "nationname")) ?? "Unknown";
                }
            }
        }
        return "Unknown";
    }

    private sealed record CountryItem(int NationId, string Name)
    {
        public override string ToString() => Name;
    }

    private void AddCountryPicker(TableLayoutPanel table)
    {
        var row = table.RowCount;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new Label
        {
            Text = "Country",
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 0, StudioSpacing.Small),
        };
        _countryPicker.Font = LegacyFont;
        Theme.ApplyCombo(_countryPicker);
        _countryPicker.Dock = DockStyle.Fill;
        _countryPicker.Margin = new Padding(0, 0, 0, StudioSpacing.Small);
        _countryPicker.SelectedIndexChanged += (_, _) =>
        {
            if (_syncCountryPicker || CurrentRecordIndex < 0 || _countryPicker.SelectedItem is not CountryItem item || !_fields.TryGetValue("countryid", out var value) || !value.IsWritable) return;
            StageField(TableName, CurrentRecordIndex, "countryid", item.NationId.ToString(), _stagingGrid);
        };
        table.Controls.Add(label, 0, row);
        table.Controls.Add(_countryPicker, 1, row);
    }

    private void PopulateCountryPicker()
    {
        _syncCountryPicker = true;
        try
        {
            _countryPicker.BeginUpdate();
            _countryPicker.Items.Clear();
            var nations = Services.Session.GetTable("nations");
            if (nations != null)
            {
                var id = Col(nations, "nationid");
                for (var row = 0; row < nations.RowCount; row++)
                {
                    var record = Services.Session.GetRecord("nations", row);
                    if (record == null || !int.TryParse(record.Get(id), out var nationId)) continue;
                    _countryPicker.Items.Add(new CountryItem(nationId, Services.Resolver?.NationName(nationId) ?? record.Get(Col(nations, "nationname"))));
                }
            }
            var selected = _fields.TryGetValue("countryid", out var value) && int.TryParse(value.RawValue, out var countryId) ? countryId : -1;
            var index = -1;
            for (var i = 0; i < _countryPicker.Items.Count; i++)
            {
                if (_countryPicker.Items[i] is CountryItem item && item.NationId == selected)
                {
                    index = i;
                    break;
                }
            }
            _countryPicker.SelectedIndex = index;
            _countryPicker.Enabled = _fields.TryGetValue("countryid", out var writable) && writable.IsWritable;
            _countryPicker.EndUpdate();
        }
        finally { _syncCountryPicker = false; }
    }

    private void AddLeagueFlag(TableLayoutPanel table, string label, string field, int col, int row)
    {
        var check = new CheckBox
        {
            Text = label,
            AutoSize = true,
            Font = LegacyFont,
            Tag = field,
            BackColor = Color.Transparent,
            ForeColor = StudioColors.PrimaryText,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 0, StudioSpacing.Small),
        };
        check.CheckedChanged += (_, _) =>
        {
            if (_syncLeagueFlags || CurrentRecordIndex < 0 || !_fields.TryGetValue(field, out var value) || !value.IsWritable) return;
            StageField(TableName, CurrentRecordIndex, field, check.Checked ? "1" : "0", _stagingGrid);
        };
        _leagueFlags[field] = check;
        table.Controls.Add(check, col, row);
    }

    private void AddField(TableLayoutPanel table, string fieldName, string label)
    {
        var row = table.RowCount;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
        table.Controls.Add(caption, 0, row);
        table.Controls.Add(editor, 1, row);
    }

    private void AddMirrorField(TableLayoutPanel table, string fieldName, string label)
    {
        var row = table.RowCount;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
        _nameMirrors.Add(editor);
        table.Controls.Add(caption, 0, row);
        table.Controls.Add(editor, 1, row);
    }

    private void RefreshNameMirrors()
    {
        foreach (var mirror in _nameMirrors)
        {
            mirror.ReadOnly = true;
            mirror.BackColor = StudioColors.RaisedSurface;
            mirror.ForeColor = StudioColors.PrimaryText;
            ToolTip.SetToolTip(mirror, "Read-only mirror of leaguename — edit it in the Database Name field.");
            if (_fields.TryGetValue(mirror.Tag as string ?? string.Empty, out var field))
                mirror.Text = field.Value;
        }
    }

    private void SetEditor(TextBox editor)
    {
        var key = editor.Tag as string ?? string.Empty;
        if (_fields.TryGetValue(key, out var field))
        {
            if (key.Equals("countryid", StringComparison.OrdinalIgnoreCase) && int.TryParse(field.RawValue, out var nationId))
            {
                editor.Text = Services.Resolver?.NationName(nationId) ?? field.Value;
                editor.ReadOnly = true;
                editor.BackColor = StudioColors.RaisedSurface;
                editor.ForeColor = StudioColors.MutedText;
                ToolTip.SetToolTip(editor, $"countryid = {field.RawValue} (resolved from nations)");
                return;
            }
            editor.Text = field.Value;
            editor.ReadOnly = !field.IsWritable;
            editor.BackColor = field.IsWritable ? StudioColors.InputBackground : StudioColors.RaisedSurface;
            editor.ForeColor = StudioColors.PrimaryText;
            ToolTip.SetToolTip(editor, field.IsWritable ? field.FieldName : $"{field.FieldName} (read-only)");
        }
        else
        {
            editor.Text = string.Empty; editor.ReadOnly = true; editor.BackColor = StudioColors.RaisedSurface; editor.ForeColor = StudioColors.MutedText;
            ToolTip.SetToolTip(editor, $"{key} is not present in this database");
        }
    }

    private void Commit(TextBox editor)
    {
        if (CurrentRecordIndex < 0 || editor.ReadOnly || editor.Tag is not string key || !_fields.TryGetValue(key, out var field)) return;
        if (!string.Equals(editor.Text.Trim(), field.Value, StringComparison.Ordinal))
        {
            if (StageField(TableName, CurrentRecordIndex, key, editor.Text.Trim(), _stagingGrid))
                RefreshNameMirrors();
        }
    }

    private void PopulateTeamLinks()
    {
        var links = Services.Session.GetTable("leagueteamlinks"); if (links == null) return;
        var leagueCol = Col(links, "leagueid"); var teamCol = Col(links, "teamid");
        var teams = Services.Session.GetTable("teams");
        var teamIdCol = teams == null ? -1 : Col(teams, "teamid");
        var teamNameCol = teams == null ? -1 : Col(teams, "teamname");
        var teamCountryCol = teams == null ? -1 : Col(teams, "countryid");
        var teamOvrCol = teams == null ? -1 : Col(teams, "overallrating");

        for (var row = 0; row < links.RowCount; row++)
        {
            var rec = Services.Session.GetRecord("leagueteamlinks", row);
            if (rec == null || !int.TryParse(rec.Get(leagueCol), out var league) || league != _leagueId) continue;
            if (!int.TryParse(rec.Get(teamCol), out var teamId)) continue;

            string name = Services.Resolver?.TeamName(teamId) ?? $"Team {teamId}";
            string country = "Unknown";
            string ovr = "0";
            if (teams != null && teamIdCol >= 0)
            {
                for (var t = 0; t < teams.RowCount; t++)
                {
                    var tr = Services.Session.GetRecord("teams", t);
                    if (tr == null || Parse(tr.Get(teamIdCol)) != teamId) continue;
                    if (teamNameCol >= 0) name = tr.Get(teamNameCol) ?? name;
                    if (teamOvrCol >= 0) ovr = tr.Get(teamOvrCol) ?? ovr;
                    if (teamCountryCol >= 0 && int.TryParse(tr.Get(teamCountryCol), out var cid))
                        country = Services.Resolver?.NationName(cid) ?? cid.ToString();
                    break;
                }
            }

            var tile = new LeagueClubTile
            {
                TeamId = teamId,
                LinkRow = row,
                ClubName = name,
                Overall = int.TryParse(ovr, out var o) ? o : 0,
                CountryName = country,
            };
            tile.SelectRequested += ClubTile_SelectRequested;
            tile.OpenRequested += (_, _) => OpenSelectedTeam();
            _clubsFlow?.Controls.Add(tile);

            var path = Services.Assets.GetTeamLogo(teamId);
            Image initial;
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                using var source = Image.FromFile(path);
                initial = CreateDarkCrestTile(source);
            }
            else initial = MissingCrest();
            tile.CrestImage = initial;
            QueueFc26TeamCrest(teamId, img =>
            {
                if (!tile.IsDisposed) tile.CrestImage = img;
            });
        }
    }

    private void ClubTile_SelectRequested(object? sender, EventArgs e)
    {
        if (sender is not LeagueClubTile tile) return;
        SelectClubTile(tile);
    }

    private void SelectClubTile(LeagueClubTile tile)
    {
        if (_selectedClubTile != null) _selectedClubTile.Selected = false;
        _selectedClubTile = tile;
        tile.Selected = true;
        _removeTeam.Enabled = true;
    }

    private void ClearClubTiles()
    {
        if (_clubsFlow == null) return;
        foreach (var tile in _clubsFlow.Controls.OfType<LeagueClubTile>().ToList())
        {
            tile.CrestImage = null;
            tile.SelectRequested -= ClubTile_SelectRequested;
            tile.Dispose();
        }
        _clubsFlow.Controls.Clear();
        _selectedClubTile = null;
        _removeTeam.Enabled = false;
    }

    private void PopulateTeamPicker(string? filter = null)
    {
        _teamPicker.Items.Clear();
        var teams = Services.Session.GetTable("teams");
        if (teams == null) return;
        var idCol = Col(teams, "teamid");
        var nameCol = Col(teams, "teamname");
        var linked = _clubsFlow?.Controls.OfType<LeagueClubTile>().Select(t => t.TeamId).ToHashSet() ?? [];
        for (var row = 0; row < teams.RowCount; row++)
        {
            var rec = Services.Session.GetRecord("teams", row);
            if (rec == null || !int.TryParse(rec.Get(idCol), out var id)) continue;
            var name = rec.Get(nameCol);
            var text = $"{name} [{id}]";
            if (!linked.Contains(id) && (string.IsNullOrWhiteSpace(filter) || text.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                _teamPicker.Items.Add(text);
        }
    }

    private void FindTeams()
    {
        PopulateTeamPicker(_teamSearch.Text.Trim());
        if (_teamPicker.Items.Count > 0) _teamPicker.SelectedIndex = 0;
        else MessageBox.Show(this, "No unlinked team matches that search.", "Search Team",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CreateAndLinkTeam()
    {
        if (_leagueId <= 0 || CurrentRecordIndex < 0)
        {
            MessageBox.Show(this, "Select or create a league first.", "Add New Team",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var fields = new List<EntityField>
        {
            new("Team name", "New Team"),
        };

        if (!EntityCreationDialog.TryShow(this, "Team for this league", fields, out var values)) return;

        var teams = Services.Session.GetTable("teams");
        if (teams == null || teams.RowCount == 0)
        {
            MessageBox.Show(this, "The team table has no safe template record.", "Add New Team",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            var duplicate = Services.Session.DuplicateRow("teams", 0);
            if (!duplicate.Success) throw new InvalidOperationException(duplicate.Message);
            var newRow = 1;
            Services.Session.RefreshSchema();
            var teamId = FindSafeTeamId();
            var countryId = _fields.TryGetValue("countryid", out var country) ? country.RawValue : "0";
            var valuesToStage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["teamid"] = teamId.ToString(), ["teamname"] = values[0], ["countryid"] = countryId, ["leagueid"] = _leagueId.ToString(),
                ["assetid"] = "0", ["presassetone"] = "0", ["presassettwo"] = "0",
                ["captainid"] = "-1", ["penaltytakerid"] = "-1", ["freekicktakerid"] = "-1",
                ["leftcornerkicktakerid"] = "-1", ["rightcornerkicktakerid"] = "-1",
            };
            foreach (var (field, value) in valuesToStage)
            {
                if (teams.FindColumn(field) == null) continue;
                var outcome = Services.Pending.Stage("teams", newRow, field, value);
                if (!outcome.Success) throw new InvalidOperationException(outcome.Message);
            }
            foreach (var (field, value) in new Dictionary<string, string>
            {
                ["stadiumid"] = "-1", ["managerid"] = "-1", ["kitids"] = "0",
                ["formationid"] = "-1", ["domesticprestige"] = "0",
                ["internationalprestige"] = "0", ["clubworth"] = "0",
                ["overallrating"] = "0", ["attackrating"] = "0",
                ["midfieldrating"] = "0", ["defenserating"] = "0", ["ballid"] = "0"
            })
            {
                if (teams.FindColumn(field) == null) continue;
                var outcome = Services.Pending.Stage("teams", newRow, field, value);
                if (!outcome.Success) throw new InvalidOperationException(outcome.Message);
            }
            Services.Pending.MarkStructuralChange();
            if (!TryLinkTeamToCurrentLeague(teamId, out var message)) throw new InvalidOperationException(message);
            var squad = FillTeamSquad(teamId);
            Services.Session.RefreshSchema();
            Services.RefreshDatabaseIndexes();
            ShowRecord(CurrentRecordIndex);
            MessageBox.Show(this, $"{values[0]} was created with ID {teamId}, added to this league and given a squad of {squad} placeholder players. The new team is opened for editing — rename the Player 1..N rows and press Save when ready.", "Add New Team",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Services.RequestRecordNavigation("teams", newRow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Add New Team", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddSelectedTeam()
    {
        if (_leagueId <= 0 || _teamPicker.SelectedItem is not string selected) return;
        var start = selected.LastIndexOf('['); if (start < 0 || !int.TryParse(selected[(start + 1)..].TrimEnd(']'), out var teamId)) return;
        if (TryLinkTeamToCurrentLeague(teamId, out var message)) { Services.Session.RefreshSchema(); Services.RefreshDatabaseIndexes(); ShowRecord(CurrentRecordIndex); }
        else MessageBox.Show(this, message, "League", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private bool TryLinkTeamToCurrentLeague(int teamId, out string message)
    {
        message = string.Empty;
        var links = Services.Session.GetTable("leagueteamlinks");
        if (links == null || links.RowCount == 0)
        {
            message = "The league-team link table has no safe template record.";
            return false;
        }
        var keyCol = Col(links, "artificialkey"); var maxKey = 0;
        var leagueCol = Col(links, "leagueid"); var teamCol = Col(links, "teamid");
        if (keyCol < 0 || leagueCol < 0 || teamCol < 0)
        {
            message = "The league-team link table is missing required columns.";
            return false;
        }
        for (var row = 0; row < links.RowCount; row++)
        {
            var rec = Services.Session.GetRecord("leagueteamlinks", row);
            if (rec == null) continue;
            if (int.TryParse(rec.Get(keyCol), out var key)) maxKey = Math.Max(maxKey, key);
            if (rec.Get(leagueCol) == _leagueId.ToString() && rec.Get(teamCol) == teamId.ToString())
            {
                message = "This team is already linked to the current league.";
                return false;
            }
        }
        if (maxKey >= 4000) { message = "No available league-team link key remains."; return false; }
        var duplicate = Services.Session.DuplicateRow("leagueteamlinks", 0);
        if (!duplicate.Success) { message = duplicate.Message; return false; }
        Services.Pending.MarkStructuralChange();
        var newRow = 1;
        var ok = Stage("artificialkey", (maxKey + 1).ToString()) && Stage("leagueid", _leagueId.ToString()) && Stage("teamid", teamId.ToString());
        if (!ok) { message = "Could not stage the team-to-league link."; return false; }
        return true;
        bool Stage(string field, string value) => Services.Pending.Stage("leagueteamlinks", newRow, field, value).Success;
    }

    private void RemoveSelectedTeam()
    {
        if (_selectedClubTile == null) return;
        if (MessageBox.Show(this, "Remove this team from the league?", "League", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var removed = Services.Session.DeleteRow("leagueteamlinks", _selectedClubTile.LinkRow);
        if (!removed.Success) { MessageBox.Show(this, removed.Message, "League", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Services.Pending.MarkStructuralChange(); Services.Session.RefreshSchema(); ShowRecord(CurrentRecordIndex);
    }

    private void QueueFc26TeamCrest(int teamId, Action<Image?> apply)
    {
        if (teamId <= 0 || !Services.FrostbiteAssets.IsAvailable || !_pendingTeamCrests.Add(teamId)) return;
        _ = Task.Run(async () =>
        {
            await _teamCrestGate.WaitAsync();
            try
            {
                var legacyPath = Services.FrostbiteAssets.ExportLegacyAsset(
                    $"data/ui/imgAssets/crest/dark/l{teamId}.dds");
                if (!string.IsNullOrWhiteSpace(legacyPath))
                {
                    using var crest = FrostbitePreviewLoader.CreatePreview(Services, legacyPath, 72, 72);
                    return crest == null ? null : CreateDarkCrestTile(crest);
                }
                return null;
            }
            finally { _teamCrestGate.Release(); }
        }).ContinueWith(task =>
        {
            if (IsDisposed) return;
            _pendingTeamCrests.Remove(teamId);
            if (task.Status != TaskStatus.RanToCompletion) return;
            apply(task.Result);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static Image MissingCrest()
    {
        var image = new Bitmap(72, 72);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(DarkCrestTile);
        using var pen = new Pen(Color.FromArgb(145, Color.White), 1);
        graphics.DrawRectangle(pen, 4, 4, 63, 63);
        TextRenderer.DrawText(graphics, "?", Theme.Body, new Rectangle(4, 4, 63, 63), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        return image;
    }

    private static Image CreateDarkCrestTile(Image crest)
    {
        var image = new Bitmap(72, 72);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(DarkCrestTile);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        var scale = Math.Min(64d / crest.Width, 64d / crest.Height);
        var width = Math.Max(1, (int)Math.Round(crest.Width * scale));
        var height = Math.Max(1, (int)Math.Round(crest.Height * scale));
        graphics.DrawImage(crest, new Rectangle((72 - width) / 2, (72 - height) / 2, width, height));
        return image;
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

    private void SearchLeagues(string query)
    {
        var term = query.Trim();
        if (term.Length == 0) return;
        var result = GetRecords().FirstOrDefault(item => item.Matches(term));
        if (result == null)
        {
            MessageBox.Show(this, $"No league matches '{term}'.", "Search League",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        GoToRecord(result.RecordIndex);
    }

    private static StudioCard StudioGroup(string title, Color accent)
    {
        var card = new StudioCard
        {
            AccentColor = accent,
            Margin = new Padding(0, StudioSpacing.Medium, StudioSpacing.Medium, 0),
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _teamCrestGate.Dispose();
        }
        base.Dispose(disposing);
    }

    private sealed class LeagueClubTile : StudioCard
    {
        private readonly PictureBox _crest;
        private readonly Label _name;
        private readonly RatingBadge _ovr;
        private readonly Label _country;
        private bool _selected;

        public LeagueClubTile()
        {
            Width = 180;
            Height = 140;
            Margin = new Padding(0, 0, StudioSpacing.Small, StudioSpacing.Small);
            AccentColor = Color.Empty;
            Cursor = Cursors.Hand;

            _crest = new PictureBox
            {
                Size = new Size(72, 72),
                Location = new Point(StudioSpacing.Medium, StudioSpacing.Medium),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
            };
            _ovr = new RatingBadge
            {
                Location = new Point(128, StudioSpacing.Medium),
            };
            _name = new Label
            {
                Location = new Point(StudioSpacing.Medium, 90),
                Size = new Size(156, 20),
                Font = StudioFonts.DataValue,
                ForeColor = StudioColors.PrimaryText,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
            };
            _country = new Label
            {
                Location = new Point(StudioSpacing.Medium, 114),
                Size = new Size(156, 18),
                Font = StudioFonts.DataLabel,
                ForeColor = StudioColors.MutedText,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
            };

            Controls.Add(_ovr);
            Controls.Add(_country);
            Controls.Add(_name);
            Controls.Add(_crest);

            Click += (_, _) => OnSelectRequested();
            _crest.Click += (_, _) => OnSelectRequested();
            _name.Click += (_, _) => OnSelectRequested();
            _ovr.Click += (_, _) => OnSelectRequested();
            _country.Click += (_, _) => OnSelectRequested();
            DoubleClick += (_, _) => OnOpenRequested();
        }

        public int TeamId { get; init; }
        public int LinkRow { get; init; }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                AccentColor = value ? StudioColors.CyanAccent : Color.Empty;
                Invalidate();
            }
        }

        public Image? CrestImage
        {
            get => _crest.Image;
            set
            {
                var old = _crest.Image;
                _crest.Image = value;
                old?.Dispose();
            }
        }

        public string ClubName { set => _name.Text = value; }
        public int Overall { set => _ovr.Rating = value; }
        public string CountryName { set => _country.Text = value; }

        public event EventHandler? SelectRequested;
        public event EventHandler? OpenRequested;

        private void OnSelectRequested() => SelectRequested?.Invoke(this, EventArgs.Empty);
        private void OnOpenRequested() => OpenRequested?.Invoke(this, EventArgs.Empty);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CrestImage = null;
            }
            base.Dispose(disposing);
        }
    }
}
