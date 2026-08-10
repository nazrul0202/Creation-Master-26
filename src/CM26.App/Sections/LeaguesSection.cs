using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>
/// FC26 adapter using the original CM16 LeagueForm workspace geometry.  The
/// linked CM16Source/LeagueForm.cs remains the authoritative layout source.
/// </summary>
public sealed class LeaguesSection : SectionBase
{
    private static readonly Font LegacyFont = Theme.Body;
    // FC26's canonical `crest/dark` family intentionally contains white marks
    // (Liverpool and Nottingham Forest are examples). A transparent preview on
    // the normal white ListView background makes valid dark crests look missing.
    private static readonly Color DarkCrestTile = Color.FromArgb(20, 42, 63);
    private readonly List<TextBox> _editors = [];
    private readonly List<TextBox> _nameMirrors = [];
    private readonly Dictionary<string, FieldValue> _fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _stagingGrid = new();
    private readonly ListView _teams = new();
    private readonly ImageList _teamImages = new() { ImageSize = new Size(56, 56), ColorDepth = ColorDepth.Depth32Bit };
    private readonly HashSet<int> _pendingTeamCrests = [];
    private readonly SemaphoreSlim _teamCrestGate = new(2, 2);
    private readonly Label _logoCaption = new();
    private readonly PictureBox _mainLogo;
    private readonly PictureBox _bannerLogo;
    private readonly PictureBox _wideLogo;
    private readonly ToolStripComboBox _teamPicker = new() { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ToolStripTextBox _teamSearch = new() { Width = 180, ToolTipText = "Search teams to add to this league" };
    private readonly ToolStripButton _addTeam = new("Add");
    private readonly ToolStripButton _removeTeam = new("Remove");
    private readonly Dictionary<string, CheckBox> _leagueFlags = new(StringComparer.OrdinalIgnoreCase);
    private readonly ComboBox _countryPicker = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _leagueNameLabel = new();
    private readonly Label _leagueMetaLabel = new();
    private readonly PictureBox _leagueLogoPreview = new();
    private int _leagueId;
    private bool _syncLeagueFlags;
    private bool _syncCountryPicker;
    private bool _showTeamLogos = true;

    public override string SectionKey => "leagues";
    public override string SectionTitle => "Leagues";
    protected override string TableName => "leagues";
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search leagues…";

    public LeaguesSection(AppServices services) : base(services)
    {
        Header.Visible = false;
        Tabs.Padding = new Point(3, 1);

        var page = new TabPage("General") { BackColor = Theme.Background, Font = LegacyFont };
        var canvas = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CardLayout.CardBackground };
        page.Controls.Add(canvas);
        Tabs.TabPages.Add(page);

        // ── League header card ────────────────────────────────────────────
        var header = CardLayout.CreateHeader(1340, 142, Color.FromArgb(0, 120, 212));
        _leagueNameLabel.Location = new Point(150, 20);
        _leagueNameLabel.Size = new Size(460, 36);
        _leagueNameLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        _leagueNameLabel.ForeColor = CardLayout.CardText;
        header.Controls.Add(_leagueNameLabel);
        _leagueMetaLabel.Location = new Point(153, 63);
        _leagueMetaLabel.Size = new Size(500, 24);
        _leagueMetaLabel.Font = Theme.BodyBold;
        _leagueMetaLabel.ForeColor = CardLayout.CardMuted;
        header.Controls.Add(_leagueMetaLabel);
        _leagueLogoPreview.Location = new Point(16, 13);
        _leagueLogoPreview.Size = new Size(116, 116);
        _leagueLogoPreview.SizeMode = PictureBoxSizeMode.Zoom;
        _leagueLogoPreview.BackColor = Color.FromArgb(243, 245, 241);
        _leagueLogoPreview.BorderStyle = BorderStyle.None;
        header.Controls.Add(_leagueLogoPreview);
        CardLayout.ApplyRounded(header, 14);
        canvas.Controls.Add(header);

        // ── Teams card ────────────────────────────────────────────────────
        var teamBox = CardLayout.CreateGroup(canvas, "Teams in League", Color.FromArgb(0, 120, 212), 16, 172, 467, 454);
        var teamTools = new ToolStrip { Location = new Point(4, 26), Size = new Size(458, 25), GripStyle = ToolStripGripStyle.Hidden, Font = LegacyFont, BackColor = CardLayout.CardWhite, ForeColor = Theme.Text, Renderer = new DarkToolStripRenderer() };
        teamTools.Items.Add(_teamPicker);
        teamTools.Items.Add(_addTeam);
        teamTools.Items.Add(_removeTeam);
        var showTeamLogo = new ToolStripButton("Show Team Logo") { CheckOnClick = true, Checked = true, ForeColor = Theme.Text };
        showTeamLogo.CheckedChanged += (_, _) =>
        {
            _showTeamLogos = showTeamLogo.Checked;
            _teams.View = _showTeamLogos ? View.LargeIcon : View.List;
        };
        teamTools.Items.Add(showTeamLogo);
        _addTeam.Click += (_, _) => AddSelectedTeam();
        _removeTeam.Click += (_, _) => RemoveSelectedTeam();
        foreach (ToolStripItem item in teamTools.Items)
            if (item is not ToolStripComboBox && item is not ToolStripTextBox)
                item.ForeColor = Theme.Text;
        _teamPicker.ComboBox.BackColor = Theme.Input;
        _teamPicker.ComboBox.ForeColor = Theme.Text;
        var teamActions = new ToolStrip { Location = new Point(4, 52), Size = new Size(458, 25), GripStyle = ToolStripGripStyle.Hidden, Font = LegacyFont, BackColor = CardLayout.CardWhite, ForeColor = Theme.Text, Renderer = new DarkToolStripRenderer() };
        teamActions.Items.Add(new ToolStripLabel("Find club to add") { ForeColor = Theme.Muted });
        teamActions.Items.Add(_teamSearch);
        var findTeam = new ToolStripButton("Find") { ForeColor = Theme.Text };
        findTeam.Click += (_, _) => FindTeams();
        teamActions.Items.Add(findTeam);
        var addNewLeague = new ToolStripButton("Add New League") { ForeColor = Theme.Text };
        addNewLeague.Click += (_, _) => CreateNewRecord();
        teamActions.Items.Add(addNewLeague);
        foreach (ToolStripItem item in teamActions.Items)
            if (item is not ToolStripComboBox && item is not ToolStripTextBox)
                item.ForeColor = Theme.Text;
        _teamSearch.TextBox.BackColor = Theme.Input;
        _teamSearch.TextBox.ForeColor = Theme.Text;
        _teamSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            FindTeams();
            e.SuppressKeyPress = true;
        };
        _teams.Location = new Point(4, 78);
        _teams.Size = new Size(458, 368);
        _teams.View = View.LargeIcon;
        _teams.LargeImageList = _teamImages;
        _teams.MultiSelect = false;
        _teams.GridLines = true;
        _teams.Font = LegacyFont;
        _teams.BackColor = Theme.Input;
        _teams.ForeColor = Theme.Text;
        _teams.SelectedIndexChanged += (_, _) => _removeTeam.Enabled = _teams.SelectedItems.Count > 0;
        _teams.DoubleClick += (_, _) => OpenSelectedTeam();
        _teams.MouseUp += (_, e) =>
        {
            if (e.Button != MouseButtons.Right) return;
            var hit = _teams.HitTest(e.Location);
            if (hit.Item != null) _teams.SelectedIndices.Clear();
            if (hit.Item != null) hit.Item.Selected = true;
        };
        var teamMenu = new ContextMenuStrip { Renderer = new DarkToolStripRenderer(), BackColor = Theme.Panel, ForeColor = Theme.Text };
        teamMenu.Items.Add("Add New Team", null, (_, _) => CreateAndLinkTeam());
        teamMenu.Items.Add("Add Existing Team", null, (_, _) => _teamSearch.Focus());
        teamMenu.Items.Add(new ToolStripSeparator());
        teamMenu.Items.Add("Open Team", null, (_, _) => OpenSelectedTeam());
        teamMenu.Items.Add("Remove from League", null, (_, _) => RemoveSelectedTeam());
        teamMenu.Opening += (_, e) =>
        {
            var hasTeam = _teams.SelectedItems.Count > 0 && _teams.SelectedItems[0].Tag is LeagueTeamLink;
            teamMenu.Items[3].Enabled = hasTeam;
            teamMenu.Items[4].Enabled = hasTeam;
            teamMenu.Items[0].Enabled = _leagueId > 0 && CurrentRecordIndex >= 0;
        };
        _teams.ContextMenuStrip = teamMenu;
        teamBox.Controls.Add(teamTools);
        teamBox.Controls.Add(teamActions);
        teamBox.Controls.Add(_teams);

        // ── Logos card ────────────────────────────────────────────────────
        var logos = CardLayout.CreateGroup(canvas, "Logos", Color.FromArgb(116, 185, 34), 499, 172, 532, 457);
        logos.Controls.Add(Viewer(new Point(6, 26), new Size(256, 256), "256 x 256", out _mainLogo));
        logos.Controls.Add(Viewer(new Point(268, 26), new Size(200, 64), "200 x 64", out _bannerLogo));
        logos.Controls.Add(Viewer(new Point(6, 305), new Size(512, 128), "512 x 128", out _wideLogo));
        _logoCaption.Location = new Point(268, 110);
        _logoCaption.Size = new Size(200, 16);
        _logoCaption.AutoEllipsis = true;
        _logoCaption.Font = LegacyFont;
        _logoCaption.TextAlign = ContentAlignment.MiddleCenter;
        _logoCaption.ForeColor = CardLayout.CardSubtle;
        _logoCaption.BackColor = CardLayout.CardWhite;
        logos.Controls.Add(_logoCaption);

        // ── Names card ────────────────────────────────────────────────────
        var names = CardLayout.CreateGroup(canvas, "Names and Information", Color.FromArgb(94, 108, 57), 16, 640, 531, 197);
        AddField(names, "leaguename", "Database Name", new Point(120, 23), 152);
        AddMirrorField(names, "leaguename", "Name", new Point(120, 49), 152);
        AddMirrorField(names, "leaguename", "Long Name", new Point(120, 75), 152);
        AddField(names, "leagueid", "League Id", new Point(120, 101), 122);
        AddField(names, "level", "Level", new Point(120, 127), 122);
        AddCountryPicker(names, new Point(120, 153));
        AddField(names, "leaguetype", "Prestige", new Point(120, 179), 152);

        // ── League settings card ──────────────────────────────────────────
        var fc26 = CardLayout.CreateGroup(canvas, "League Settings", Color.FromArgb(205, 142, 16), 559, 640, 532, 197);
        AddLeagueFlag(fc26, "Women's competition", "iswomencompetition", new Point(12, 28));
        AddLeagueFlag(fc26, "International league", "isinternationalleague", new Point(12, 54));
        AddLeagueFlag(fc26, "Competition pole flags", "iscompetitionpoleflagenabled", new Point(12, 80));
        AddLeagueFlag(fc26, "Within transfer window", "iswithintransferwindow", new Point(230, 28));
        AddLeagueFlag(fc26, "Competition scarves", "iscompetitionscarfenabled", new Point(230, 54));
        AddLeagueFlag(fc26, "Crowd cards", "iscompetitioncrowdcardsenabled", new Point(230, 80));
        AddLeagueFlag(fc26, "Banner enabled", "isbannerenabled", new Point(230, 106));
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
            MessageBox.Show(this, "Enter an existing Country ID. Create the country first with Add Country to Game if needed.",
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

    /// <summary>CM16 behaviour: double-click a league club to open its Team record.</summary>
    private void OpenSelectedTeam()
    {
        if (_teams.SelectedItems.Count == 0) return;
        if (_teams.SelectedItems[0].Tag is not LeagueTeamLink link || link.TeamId <= 0)
            return;
        var teamId = link.TeamId;
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
                    ToolTip.SetToolTip(check, value.IsWritable ? field : field + " (read-only)");
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
        ShowLeagueLogo(logo, name, leagueId);
        _logoCaption.Text = string.IsNullOrWhiteSpace(logo)
            ? (string.IsNullOrWhiteSpace(name) ? "No local league logo" : $"{name} · no local logo")
            : name;

        // ── Populate header card ──────────────────────────────────────────
        _leagueNameLabel.Text = name ?? string.Empty;
        _leagueMetaLabel.Text = $"League ID {record.Get(Col(table, "leagueid"))}  ·  Level {record.Get(Col(table, "level"))}";
        try
        {
            if (!string.IsNullOrWhiteSpace(logo) && File.Exists(logo))
                _leagueLogoPreview.Image = Image.FromFile(logo);
            else
                _leagueLogoPreview.Image = null;
        }
        catch { _leagueLogoPreview.Image = null; }

        _teams.Items.Clear();
        _leagueId = int.TryParse(record.Get(Col(table, "leagueid")), out var id) ? id : 0;
        PopulateTeamLinks();
        PopulateTeamPicker();
        if (_teams.Items.Count == 0)
            _teams.Items.Add("No teams linked in leagueteamlinks");
    }

    private static GroupBox Group(string text, Point location, Size size) =>
        new ModernGroupBox { Text = text, Location = location, Size = size };

    private Panel Viewer(Point location, Size size, string caption, out PictureBox picture)
    {
        var holder = new Panel { Location = location, Size = new Size(size.Width, size.Height + 21), BackColor = Theme.Panel };
        picture = new PictureBox { Location = Point.Empty, Size = size, BackColor = Theme.Input, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        var targetPicture = picture;
        holder.Controls.Add(picture);
        var view = new LinkLabel { Text = "view", Location = new Point(0, size.Height + 2), AutoSize = true, Font = LegacyFont, BackColor = Theme.Panel, ForeColor = Theme.Link };
        var import = new LinkLabel { Text = "import", Location = new Point(35, size.Height + 2), AutoSize = true, Font = LegacyFont, BackColor = Theme.Panel, ForeColor = Theme.Link };
        var remove = new LinkLabel { Text = "remove", Location = new Point(82, size.Height + 2), AutoSize = true, Font = LegacyFont, BackColor = Theme.Panel, ForeColor = Theme.Danger };
        view.LinkClicked += (_, _) => ViewAsset(targetPicture);
        import.LinkClicked += (_, _) => ImportAsset(targetPicture);
        remove.LinkClicked += (_, _) => RemoveAsset(targetPicture);
        holder.Controls.Add(view);
        holder.Controls.Add(import);
        holder.Controls.Add(remove);
        holder.Controls.Add(new Label
        {
            Text = caption, Location = new Point(135, size.Height + 2),
            Size = new Size(Math.Max(30, size.Width - 135), 18), Font = LegacyFont,
            TextAlign = ContentAlignment.TopRight, ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        return holder;
    }

    private void ShowLeagueLogo(string path, string name, int leagueId)
    {
        // Load the primary logo first. The wide preview can then reliably use it
        // as a fallback when a league does not ship a dedicated 512x128 texture.
        LoadLeagueLogo(_mainLogo, name, leagueId, "league", 256, 256, updateCaption: true,
            completed: () =>
            {
                LoadLeagueLogo(_bannerLogo, name, leagueId, "leagueLogos_tiny", 200, 64);
                LoadLeagueLogo(_wideLogo, name, leagueId, "league512x128", 512, 128, fallbackToMain: true);
            });
    }

    private void LoadLeagueLogo(
        PictureBox viewer,
        string name,
        int leagueId,
        string collection,
        int width,
        int height,
        bool updateCaption = false,
        bool fallbackToMain = false,
        Action? completed = null)
    {
        var darkPath = $"data/ui/imgAssets/{collection}/dark/l{leagueId}.dds";
        var lightPath = $"data/ui/imgAssets/{collection}/light/l{leagueId}.dds";
        var candidates = new[] { darkPath, lightPath };
        var stagedPath = candidates.FirstOrDefault(candidate =>
            !string.IsNullOrWhiteSpace(Services.LegacyMods.GetReplacement(candidate)));

        // Older versions always targeted light. Clear that stale target until
        // the installed game tells us which variant actually exists.
        LegacyAssetActions.ClearTarget(viewer);
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(viewer, Services, null, candidates,
            (image, source) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                if (image == null && fallbackToMain && _mainLogo.Image != null)
                    image = new Bitmap(_mainLogo.Image);

                var old = viewer.Image;
                viewer.Image = image;
                old?.Dispose();
                completed?.Invoke();
                if (updateCaption && image != null) _logoCaption.Text = $"{name} · {source}";
            },
            resolvedPath =>
            {
                if (!string.IsNullOrWhiteSpace(stagedPath) &&
                    !string.Equals(stagedPath, resolvedPath, StringComparison.OrdinalIgnoreCase))
                    Services.LegacyMods.MoveReplacement(stagedPath, resolvedPath);
                LegacyAssetActions.SetTarget(viewer, new LegacyAssetEditTarget(resolvedPath, width, height));
            });
    }

    private void ImportAsset(PictureBox picture)
    {
        if (LegacyAssetActions.GetTarget(picture) is not { } target) return;
        using var dialog = new OpenFileDialog
        {
            Title = $"Import {target.LegacyPath}",
            Filter = "Texture files (*.dds;*.png;*.jpg;*.jpeg;*.bmp)|*.dds;*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var replacement = Services.LegacyMods.StageImage(
                target.LegacyPath, dialog.FileName, target.Width, target.Height);
            using var source = Services.Textures.CreatePreview(
                replacement, target.Width, target.Height);
            picture.Image?.Dispose();
            picture.Image = source == null ? null : new Bitmap(source);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import league logo",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RemoveAsset(PictureBox picture)
    {
        if (LegacyAssetActions.GetTarget(picture) is not { } target) return;
        if (!Services.LegacyMods.Remove(target.LegacyPath))
        {
            MessageBox.Show(this, "This logo has no staged replacement. The installed asset is retained.",
                "Remove league logo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        ShowRecord(CurrentRecordIndex);
    }

    private void ViewAsset(PictureBox picture)
    {
        if (LegacyAssetActions.GetTarget(picture) is not { } target) return;
        var path = Services.LegacyMods.GetReplacement(target.LegacyPath)
            ?? Services.FrostbiteAssets.ExportLegacyAsset(target.LegacyPath);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "View league logo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private sealed record CountryItem(int NationId, string Name)
    {
        public override string ToString() => Name;
    }

    private void AddCountryPicker(Control parent, Point location)
    {
        parent.Controls.Add(new Label
        {
            Text = "Country", Location = new Point(10, location.Y + 3), Size = new Size(Math.Max(70, location.X - 16), 18),
            AutoSize = false, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleRight, Font = LegacyFont
        });
        _countryPicker.Location = location;
        _countryPicker.Size = new Size(181, 21);
        _countryPicker.Font = LegacyFont;
        Theme.ApplyCombo(_countryPicker);
        _countryPicker.SelectedIndexChanged += (_, _) =>
        {
            if (_syncCountryPicker || CurrentRecordIndex < 0 || _countryPicker.SelectedItem is not CountryItem item || !_fields.TryGetValue("countryid", out var value) || !value.IsWritable) return;
            StageField(TableName, CurrentRecordIndex, "countryid", item.NationId.ToString(), _stagingGrid);
        };
        parent.Controls.Add(_countryPicker);
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
                    var record = Services.Session.GetRecord("nations", row); if (record == null || !int.TryParse(record.Get(id), out var nationId)) continue;
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

    private void AddLeagueFlag(Control parent, string label, string field, Point location)
    {
        var check = new CheckBox { Text = label, Location = location, AutoSize = true, Font = LegacyFont, Tag = field };
        check.CheckedChanged += (_, _) =>
        {
            if (_syncLeagueFlags || CurrentRecordIndex < 0 || !_fields.TryGetValue(field, out var value) || !value.IsWritable) return;
            StageField(TableName, CurrentRecordIndex, field, check.Checked ? "1" : "0", _stagingGrid);
        };
        _leagueFlags[field] = check;
        parent.Controls.Add(check);
    }

    private void AddField(Control parent, string fieldName, string label, Point location, int width)
    {
        parent.Controls.Add(new Label
        {
            Text = label, Location = new Point(10, location.Y + 3), Size = new Size(Math.Max(70, location.X - 16), 18),
            AutoSize = false, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleRight, Font = LegacyFont,
            ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        var editor = new TextBox { Location = location, Size = new Size(width, 20), Font = LegacyFont, Tag = fieldName, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(editor);
        editor.Leave += (_, _) => Commit(editor);
        parent.Controls.Add(editor);
        _editors.Add(editor);
    }

    /// <summary>
    /// Read-only mirror of a field edited elsewhere in the same group ("Name" and
    /// "Long Name" both display the single leaguename value, avoiding duplicate
    /// editors for the same database field).
    /// </summary>
    private void AddMirrorField(Control parent, string fieldName, string label, Point location, int width)
    {
        parent.Controls.Add(new Label
        {
            Text = label, Location = new Point(10, location.Y + 3), Size = new Size(Math.Max(70, location.X - 16), 18),
            AutoSize = false, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleRight, Font = LegacyFont,
            ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        var editor = new TextBox { Location = location, Size = new Size(width, 20), Font = LegacyFont, Tag = fieldName, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(editor);
        editor.BackColor = Theme.Raised;
        editor.ForeColor = Theme.Text;
        parent.Controls.Add(editor);
        _editors.Add(editor);
        _nameMirrors.Add(editor);
    }

    private void RefreshNameMirrors()
    {
        foreach (var mirror in _nameMirrors)
        {
            mirror.ReadOnly = true;
            mirror.BackColor = Theme.Raised;
            mirror.ForeColor = Theme.Text;
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
            // countryid is an FK, not a user-facing country name.  Show the resolved FC26
            // nation while retaining the raw ID in the tooltip instead of presenting "21".
            if (key.Equals("countryid", StringComparison.OrdinalIgnoreCase) && int.TryParse(field.RawValue, out var nationId))
            {
                editor.Text = Services.Resolver?.NationName(nationId) ?? field.Value;
                editor.ReadOnly = true;
                editor.BackColor = Theme.Raised;
                editor.ForeColor = Theme.Muted;
                ToolTip.SetToolTip(editor, $"countryid = {field.RawValue} (resolved from nations)");
                return;
            }
            editor.Text = field.Value;
            editor.ReadOnly = !field.IsWritable;
            editor.BackColor = field.IsWritable ? Theme.Input : Theme.Raised;
            editor.ForeColor = Theme.Text;
            ToolTip.SetToolTip(editor, field.IsWritable ? field.FieldName : field.FieldName + " (read-only)");
        }
        else
        {
            editor.Text = string.Empty; editor.ReadOnly = true; editor.BackColor = Theme.Raised; editor.ForeColor = Theme.Muted;
            ToolTip.SetToolTip(editor, key + " is not present in this database");
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
        for (var row = 0; row < links.RowCount; row++)
        {
            var rec = Services.Session.GetRecord("leagueteamlinks", row); if (rec == null || !int.TryParse(rec.Get(leagueCol), out var league) || league != _leagueId) continue;
            if (!int.TryParse(rec.Get(teamCol), out var teamId)) continue;
            var name = Services.Resolver?.TeamName(teamId) ?? $"Team {teamId}";
            var item = new ListViewItem(name)
            {
                Tag = new LeagueTeamLink(row, teamId),
                ImageKey = teamId.ToString(),
                ToolTipText = $"{name} [{teamId}]"
            };
            EnsureTeamImage(teamId, name);
            _teams.Items.Add(item);
        }
    }

    private void PopulateTeamPicker(string? filter = null)
    {
        _teamPicker.Items.Clear(); var teams = Services.Session.GetTable("teams"); if (teams == null) return;
        var idCol = Col(teams, "teamid"); var nameCol = Col(teams, "teamname");
        var linked = _teams.Items.Cast<ListViewItem>().Select(x => x.Tag).OfType<LeagueTeamLink>().Select(x => x.TeamId).ToHashSet();
        for (var row = 0; row < teams.RowCount; row++)
        {
            var rec = Services.Session.GetRecord("teams", row); if (rec == null || !int.TryParse(rec.Get(idCol), out var id)) continue;
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
            // The native engine inserts the duplicated row right after the
            // template (index 1), not at the end of the table.
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
            // Open the record the user just created instead of leaving the
            // League browser focused on the duplicated template club.
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
        if (_teams.SelectedItems.Count == 0 || _teams.SelectedItems[0].Tag is not LeagueTeamLink link) return;
        if (MessageBox.Show(this, "Remove this team from the league?", "League", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var removed = Services.Session.DeleteRow("leagueteamlinks", link.LinkRow);
        if (!removed.Success) { MessageBox.Show(this, removed.Message, "League", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Services.Pending.MarkStructuralChange(); Services.Session.RefreshSchema(); ShowRecord(CurrentRecordIndex);
    }

    private sealed record LeagueTeamLink(int LinkRow, int TeamId);

    private int EnsureTeamImage(int teamId, string teamName)
    {
        var key = teamId.ToString();
        if (_teamImages.Images.ContainsKey(key)) return _teamImages.Images.IndexOfKey(key);

        Image image;
        var path = Services.Assets.GetTeamLogo(teamId);
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            try
            {
                using var source = Image.FromFile(path);
                image = CreateDarkCrestTile(source);
            }
            catch { image = MissingCrest(); }
        }
        else image = MissingCrest();

        _teamImages.Images.Add(key, image);
        QueueFc26TeamCrest(teamId, key, teamName);
        return _teamImages.Images.IndexOfKey(key);
    }

    private void QueueFc26TeamCrest(int teamId, string key, string teamName)
    {
        if (teamId <= 0 || !Services.FrostbiteAssets.IsAvailable || !_pendingTeamCrests.Add(teamId)) return;
        _ = Task.Run(async () =>
        {
            await _teamCrestGate.WaitAsync();
            try
            {
                // FC26's menu crest is a legacy UI file. Never fall back to a
                // RES texture here: those matches are often kit materials and
                // silently substitute the wrong crest.
                var legacyPath = Services.FrostbiteAssets.ExportLegacyAsset(
                    $"data/ui/imgAssets/crest/dark/l{teamId}.dds");
                if (!string.IsNullOrWhiteSpace(legacyPath))
                {
                    using var crest = FrostbitePreviewLoader.CreatePreview(Services, legacyPath, 56, 56);
                    return crest == null ? null : CreateDarkCrestTile(crest);
                }
                return null;
            }
            finally { _teamCrestGate.Release(); }
        }).ContinueWith(task =>
        {
            if (IsDisposed || task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
            var image = task.Result;
            var old = _teamImages.Images[key];
            _teamImages.Images.RemoveByKey(key);
            _teamImages.Images.Add(key, image);
            old?.Dispose();
            _teams.Invalidate();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static Image MissingCrest()
    {
        var image = new Bitmap(56, 56);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(DarkCrestTile);
        using var pen = new Pen(Color.FromArgb(145, Color.White), 1);
        graphics.DrawRectangle(pen, 3, 3, 49, 49);
        TextRenderer.DrawText(graphics, "?", Theme.Body, new Rectangle(3, 3, 49, 49), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        return image;
    }

    private static Image CreateDarkCrestTile(Image crest)
    {
        var image = new Bitmap(56, 56);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(DarkCrestTile);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        var scale = Math.Min(50d / crest.Width, 50d / crest.Height);
        var width = Math.Max(1, (int)Math.Round(crest.Width * scale));
        var height = Math.Max(1, (int)Math.Round(crest.Height * scale));
        graphics.DrawImage(crest, new Rectangle((56 - width) / 2, (56 - height) / 2, width, height));
        return image;
    }
}
