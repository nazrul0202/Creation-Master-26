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
    private readonly List<TextBox> _editors = [];
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

        var page = new TabPage("General") { BackColor = SystemColors.Control, Font = LegacyFont };
        var canvas = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = SystemColors.Control };
        page.Controls.Add(canvas);
        Tabs.TabPages.Add(page);

        // LeagueForm.cs: the team selector (3,3,467,454).
        var teamBox = Group("Teams", new Point(3, 3), new Size(467, 454));
        var teamTools = new ToolStrip { Location = new Point(4, 17), Size = new Size(458, 25), GripStyle = ToolStripGripStyle.Hidden, Font = LegacyFont };
        teamTools.Items.Add(_teamPicker);
        teamTools.Items.Add(_addTeam);
        teamTools.Items.Add(_removeTeam);
        var showTeamLogo = new ToolStripButton("Show Team Logo") { CheckOnClick = true, Checked = true };
        showTeamLogo.CheckedChanged += (_, _) =>
        {
            _showTeamLogos = showTeamLogo.Checked;
            _teams.View = _showTeamLogos ? View.LargeIcon : View.List;
        };
        teamTools.Items.Add(showTeamLogo);
        var addNewLeague = new ToolStripButton("Add New League");
        addNewLeague.Click += (_, _) => CreateNewRecord();
        teamTools.Items.Add(addNewLeague);
        _addTeam.Click += (_, _) => AddSelectedTeam();
        _removeTeam.Click += (_, _) => RemoveSelectedTeam();
        var teamActions = new ToolStrip { Location = new Point(4, 43), Size = new Size(458, 25), GripStyle = ToolStripGripStyle.Hidden, Font = LegacyFont };
        teamActions.Items.Add(new ToolStripLabel("Find club to add"));
        teamActions.Items.Add(_teamSearch);
        var findTeam = new ToolStripButton("Find");
        findTeam.Click += (_, _) => FindTeams();
        teamActions.Items.Add(findTeam);
        _teamSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            FindTeams();
            e.SuppressKeyPress = true;
        };
        _teams.Location = new Point(4, 69);
        _teams.Size = new Size(458, 381);
        _teams.View = View.LargeIcon;
        _teams.LargeImageList = _teamImages;
        _teams.MultiSelect = false;
        _teams.GridLines = true;
        _teams.Font = LegacyFont;
        _teams.SelectedIndexChanged += (_, _) => _removeTeam.Enabled = _teams.SelectedItems.Count > 0;
        _teams.DoubleClick += (_, _) => OpenSelectedTeam();
        _teams.MouseUp += (_, e) =>
        {
            if (e.Button != MouseButtons.Right) return;
            var hit = _teams.HitTest(e.Location);
            if (hit.Item != null) _teams.SelectedIndices.Clear();
            if (hit.Item != null) hit.Item.Selected = true;
        };
        var teamMenu = new ContextMenuStrip();
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
        canvas.Controls.Add(teamBox);

        // LeagueForm.cs: 256-square, 200x64 and 512x128 image workspace.
        var logos = Group("Logos", new Point(476, 3), new Size(532, 454));
        logos.Controls.Add(Viewer(new Point(6, 18), new Size(256, 256), "256 x 256", out _mainLogo));
        logos.Controls.Add(Viewer(new Point(268, 18), new Size(256, 64), "200 x 64", out _bannerLogo));
        logos.Controls.Add(Viewer(new Point(6, 297), new Size(512, 128), "512 x 128", out _wideLogo));
        _logoCaption.Location = new Point(7, 278);
        _logoCaption.Size = new Size(255, 16);
        _logoCaption.Font = LegacyFont;
        _logoCaption.TextAlign = ContentAlignment.MiddleCenter;
        _logoCaption.ForeColor = SystemColors.ControlText;
        logos.Controls.Add(_logoCaption);
        canvas.Controls.Add(logos);

        // CM16 objective thresholds have no FC26 league-table counterpart, so
        // they are intentionally omitted instead of showing fake disabled data.
        var names = Group("Names and Other Information", new Point(3, 463), new Size(531, 202));
        AddField(names, "leaguename", "Database Name", new Point(91, 15), 181);
        AddField(names, "leaguename", "Name", new Point(91, 38), 181);
        AddField(names, "leaguename", "Long Name", new Point(91, 61), 181);
        AddField(names, "leagueid", "League Id", new Point(91, 89), 122);
        AddField(names, "level", "Level", new Point(91, 115), 122);
        AddCountryPicker(names, new Point(91, 141));
        AddField(names, "leaguetype", "Prestige", new Point(91, 168), 181);

        canvas.Controls.Add(names);

        // FC26 has these real league presentation/competition fields; CM16's
        // objective threshold boxes are not part of the FC26 leagues schema.
        var fc26 = Group("League Settings", new Point(3, 671), new Size(531, 116));
        AddLeagueFlag(fc26, "Women's competition", "iswomencompetition", new Point(12, 20));
        AddLeagueFlag(fc26, "International league", "isinternationalleague", new Point(12, 44));
        AddLeagueFlag(fc26, "Competition pole flags", "iscompetitionpoleflagenabled", new Point(12, 68));
        AddLeagueFlag(fc26, "Within transfer window", "iswithintransferwindow", new Point(250, 20));
        AddLeagueFlag(fc26, "Competition scarves", "iscompetitionscarfenabled", new Point(250, 44));
        AddLeagueFlag(fc26, "Crowd cards", "iscompetitioncrowdcardsenabled", new Point(250, 68));
        AddLeagueFlag(fc26, "Banner enabled", "isbannerenabled", new Point(250, 92));
        canvas.Controls.Add(fc26);
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
        _teams.Items.Clear();
        _leagueId = int.TryParse(record.Get(Col(table, "leagueid")), out var id) ? id : 0;
        PopulateTeamLinks();
        PopulateTeamPicker();
        if (_teams.Items.Count == 0)
            _teams.Items.Add("No teams linked in leagueteamlinks");
    }

    private static GroupBox Group(string text, Point location, Size size) => new()
    {
        Text = text, Location = location, Size = size, Font = LegacyFont,
        BackColor = SystemColors.Control, ForeColor = SystemColors.ControlText
    };

    private Panel Viewer(Point location, Size size, string caption, out PictureBox picture)
    {
        var holder = new Panel { Location = location, Size = new Size(size.Width, size.Height + 21), BackColor = SystemColors.Control };
        picture = new PictureBox { Location = Point.Empty, Size = size, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        var targetPicture = picture;
        holder.Controls.Add(picture);
        var view = new LinkLabel { Text = "view", Location = new Point(0, size.Height + 2), AutoSize = true, Font = LegacyFont };
        var import = new LinkLabel { Text = "import", Location = new Point(35, size.Height + 2), AutoSize = true, Font = LegacyFont };
        var remove = new LinkLabel { Text = "remove", Location = new Point(82, size.Height + 2), AutoSize = true, Font = LegacyFont };
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
            TextAlign = ContentAlignment.TopRight, ForeColor = SystemColors.GrayText
        });
        return holder;
    }

    private void ShowLeagueLogo(string path, string name, int leagueId)
    {
        // League marks are legacy UI DDS assets, independent from the 3D
        // leaguelogo material textures. Use the light collection on CM26's
        // white editor background, matching the asset browser display.
        var mainPath = $"data/ui/imgAssets/league/light/l{leagueId}.dds";
        var bannerPath = $"data/ui/imgAssets/leagueLogos_tiny/light/l{leagueId}.dds";
        var widePath = $"data/ui/imgAssets/league512x128/light/l{leagueId}.dds";
        LegacyAssetActions.SetTarget(_mainLogo, new LegacyAssetEditTarget(mainPath, 256, 256));
        LegacyAssetActions.SetTarget(_bannerLogo, new LegacyAssetEditTarget(bannerPath, 200, 64));
        LegacyAssetActions.SetTarget(_wideLogo, new LegacyAssetEditTarget(widePath, 512, 128));

        FrostbitePreviewLoader.LoadLegacyUiAsset(_mainLogo, Services,
            Services.LegacyMods.GetReplacement(mainPath) ?? path, mainPath, (image, source) =>
        {
            _mainLogo.Image?.Dispose();
            _mainLogo.Image = image;
            if (image != null) _logoCaption.Text = $"{name} · {source}";
        });
        FrostbitePreviewLoader.LoadLegacyUiAsset(_bannerLogo, Services,
            Services.LegacyMods.GetReplacement(bannerPath), bannerPath, (image, _) =>
        {
            _bannerLogo.Image?.Dispose();
            _bannerLogo.Image = image;
        });
        FrostbitePreviewLoader.LoadLegacyUiAsset(_wideLogo, Services,
            Services.LegacyMods.GetReplacement(widePath), widePath, (image, _) =>
        {
            _wideLogo.Image?.Dispose();
            _wideLogo.Image = image;
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
        parent.Controls.Add(new Label { Text = "Country", Location = new Point(10, location.Y + 3), Size = new Size(76, 18), Font = LegacyFont });
        _countryPicker.Location = location;
        _countryPicker.Size = new Size(181, 21);
        _countryPicker.Font = LegacyFont;
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
            _countryPicker.SelectedIndex = Enumerable.Range(0, _countryPicker.Items.Count).FirstOrDefault(i => _countryPicker.Items[i] is CountryItem item && item.NationId == selected);
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
        parent.Controls.Add(new Label { Text = label, Location = new Point(10, location.Y + 3), Size = new Size(Math.Max(72, location.X - 15), 18), Font = LegacyFont });
        var editor = new TextBox { Location = location, Size = new Size(width, 20), Font = LegacyFont, Tag = fieldName, BorderStyle = BorderStyle.FixedSingle };
        editor.Leave += (_, _) => Commit(editor);
        parent.Controls.Add(editor);
        _editors.Add(editor);
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
                editor.BackColor = SystemColors.Control;
                ToolTip.SetToolTip(editor, $"countryid = {field.RawValue} (resolved from nations)");
                return;
            }
            editor.Text = field.Value;
            editor.ReadOnly = !field.IsWritable;
            editor.BackColor = field.IsWritable ? Color.White : SystemColors.Control;
            ToolTip.SetToolTip(editor, field.IsWritable ? field.FieldName : field.FieldName + " (read-only)");
        }
        else
        {
            editor.Text = string.Empty; editor.ReadOnly = true; editor.BackColor = SystemColors.Control;
            ToolTip.SetToolTip(editor, key + " is not present in this database");
        }
    }

    private void Commit(TextBox editor)
    {
        if (CurrentRecordIndex < 0 || editor.ReadOnly || editor.Tag is not string key || !_fields.TryGetValue(key, out var field)) return;
        if (!string.Equals(editor.Text.Trim(), field.Value, StringComparison.Ordinal)) StageField(TableName, CurrentRecordIndex, key, editor.Text.Trim(), _stagingGrid);
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
        if (!EntityCreationDialog.TryShow(this, "Team for this league", [("Team name", "New Team")], out var values)) return;

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
        var ok = Stage("artificialkey", (maxKey + 1).ToString()) & Stage("leagueid", _leagueId.ToString()) & Stage("teamid", teamId.ToString());
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
                image = new Bitmap(source, _teamImages.ImageSize);
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
                // FC26's menu crest is a legacy UI file. Do this before any
                // RES fallback: logo_* and crest_* resources are often kit
                // material maps, hence the muted/wrong colours in previews.
                var legacyPath = Services.FrostbiteAssets.ExportLegacyAsset(
                    $"data/ui/imgAssets/crest/dark/l{teamId}.dds");
                if (string.IsNullOrWhiteSpace(legacyPath))
                    legacyPath = Services.FrostbiteAssets.ExportLegacyAsset(
                        $"data/ui/imgAssets/crest/light/l{teamId}.dds");
                if (!string.IsNullOrWhiteSpace(legacyPath))
                    return FrostbitePreviewLoader.CreatePreview(Services, legacyPath, 56, 56);
                var token = string.Concat(teamName.ToLowerInvariant().Where(char.IsLetterOrDigit));
                var match = Services.FrostbiteAssets.SearchAssets(token, "Res", 80)
                    .FirstOrDefault(x => x.ResType == 0x6BDE20BA &&
                        x.Name.Contains("/textures/logo/logo_", StringComparison.OrdinalIgnoreCase) &&
                        x.Name.EndsWith("_color", StringComparison.OrdinalIgnoreCase))
                    ?? Services.FrostbiteAssets.SearchAssets($"crest_{teamId}_", "Res", 40)
                    .FirstOrDefault(x => x.ResType == 0x6BDE20BA &&
                        x.Name.EndsWith("_color", StringComparison.OrdinalIgnoreCase));
                var path = match == null ? null : Services.FrostbiteAssets.ExportTexture(match.Name);
                return FrostbitePreviewLoader.CreatePreview(Services, path, 56, 56, linearColor: true);
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
        graphics.Clear(SystemColors.Window);
        using var pen = new Pen(SystemColors.ControlDark, 1);
        graphics.DrawRectangle(pen, 3, 3, 49, 49);
        using var font = new Font("Segoe UI", 14, FontStyle.Bold);
        TextRenderer.DrawText(graphics, "?", font, new Rectangle(3, 3, 49, 49), SystemColors.GrayText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        return image;
    }
}
