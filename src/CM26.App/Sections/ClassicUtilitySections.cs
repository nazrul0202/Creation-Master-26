using System.Drawing;
using System.Data;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>
/// CM16-style visual forms for assets and competitions.  These are deliberately
/// fixed WinForms canvases, matching the original editor family, while field
/// commits still go through the FC26 staging adapter.
/// </summary>
public sealed class CompetitionsSection : ClassicEntitySection
{
    private readonly TreeView _competitionTree = new();
    private readonly PictureBox _logo;
    private readonly CompdataWorkbookService _compdata = new();
    private readonly Dictionary<string, DataTable> _compdataTables = new(StringComparer.OrdinalIgnoreCase);
    private readonly ComboBox _compdataSheets = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _compdataGrid = new();
    private readonly Label _compdataStatus = new();
    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["competitionid"] = "Competition Id", ["country_lock"] = "Country",
        ["ballid"] = "Ball", ["competitionimportance"] = "Importance",
        ["has_var"] = "Has VAR", ["iswomencompetition"] = "Women's Competition",
        ["isrealcompetition"] = "Licensed", ["crowdregion"] = "Crowd Region"
    };

    protected override bool UseStudioToolbar => true;

    public CompetitionsSection(AppServices s) : base(s, "competitions", "Competitions", "competition", () => Records(s), Fields)
    {
        var fifa = AddCanvasTab("FIFA");
        AttachStudioToolbar(fifa, "Competitions");
        var c = Canvas(fifa);
        var tree = Group("Competitions", new Point(3, 3), new Size(575, 820));
        _competitionTree.Location = new Point(8, 22);
        _competitionTree.Size = new Size(555, 785);
        _competitionTree.Font = LegacyFont;
        _competitionTree.ShowRootLines = true;
        _competitionTree.ShowLines = true;
        _competitionTree.BackColor = StudioColors.InputBackground;
        _competitionTree.ForeColor = StudioColors.PrimaryText;
        _competitionTree.LineColor = StudioColors.CardBorder;
        _competitionTree.AfterSelect += (_, e) =>
        {
            if (e.Node?.Tag is int recordIndex) GoToRecord(recordIndex);
        };
        PopulateCompetitionTree(s);
        tree.Controls.Add(_competitionTree);
        c.Controls.Add(tree);
        var info = Group("Competition", new Point(584, 3), new Size(790, 260));
        AddField(info, "competitionid", "Id.", new Point(100, 20), 110);
        AddField(info, "country_lock", "Nation", new Point(100, 46), 110);
        AddField(info, "ballid", "Ball", new Point(100, 72), 110);
        AddField(info, "competitionimportance", "Importance", new Point(100, 98), 110);
        AddField(info, "isrealcompetition", "Licensed", new Point(100, 124), 110);
        _logo = ImageSurface(info, new Point(260, 18), new Size(180, 180), "Competition logo");
        LegacyAssetActions.Attach(Services, info, _logo, new Point(260, 224), () => OnRecordShown());
        c.Controls.Add(info);
        AddCompdataTab();
    }

    private void AddCompdataTab()
    {
        var page = AddCanvasTab("Compdata");
        var canvas = Canvas(page);
        var workspace = Group("Competition Data Workbook", new Point(3, 3), new Size(1340, 800));
        var open = new Button { Text = "Open Workbook…", Location = new Point(12, 24), Size = new Size(120, 28) };
        var openGame = new Button { Text = "Open from Game Folder…", Location = new Point(138, 24), Size = new Size(170, 28) };
        var add = new Button { Text = "New Object + Auto ID", Location = new Point(600, 24), Size = new Size(142, 28), Enabled = false };
        var build = new Button { Text = "Build League / Cup", Location = new Point(748, 24), Size = new Size(126, 28), Enabled = false };
        var advancement = new Button { Text = "Promotion / Relegation", Location = new Point(880, 24), Size = new Size(146, 28), Enabled = false };
        var validate = new Button { Text = "Validate", Location = new Point(1032, 24), Size = new Size(82, 28), Enabled = false };
        var export = new Button { Text = "Export TXT", Location = new Point(1120, 24), Size = new Size(88, 28), Enabled = false };
        var save = new Button { Text = "Save Copy…", Location = new Point(1214, 24), Size = new Size(100, 28), Enabled = false };
        Theme.ApplyButton(open);
        Theme.ApplyButton(openGame);
        Theme.ApplyButton(add);
        Theme.ApplyButton(build);
        Theme.ApplyButton(advancement);
        Theme.ApplyButton(validate);
        Theme.ApplyButton(export);
        Theme.ApplyButton(save);
        workspace.Controls.Add(open);
        workspace.Controls.Add(openGame);
        _compdataSheets.Location = new Point(318, 27);
        _compdataSheets.Size = new Size(275, 24);
        Theme.ApplyCombo(_compdataSheets);
        workspace.Controls.Add(_compdataSheets);
        workspace.Controls.Add(add);
        workspace.Controls.Add(build);
        workspace.Controls.Add(advancement);
        workspace.Controls.Add(validate);
        workspace.Controls.Add(export);
        workspace.Controls.Add(save);

        _compdataStatus.Location = new Point(12, 778);
        _compdataStatus.Size = new Size(1306, 18);
        _compdataStatus.Text = "Open a Compdata workbook. Validate before saving.";
        _compdataStatus.AutoEllipsis = true;
        _compdataStatus.ForeColor = StudioColors.MutedText;
        _compdataStatus.BackColor = Color.Transparent;
        workspace.Controls.Add(_compdataStatus);

        _compdataGrid.Location = new Point(12, 62);
        _compdataGrid.Size = new Size(1306, 715);
        _compdataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _compdataGrid.AllowUserToAddRows = false;
        _compdataGrid.AllowUserToDeleteRows = true;
        _compdataGrid.RowHeadersVisible = false;
        _compdataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _compdataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _compdataGrid.BackgroundColor = StudioColors.AppBackground;
        _compdataGrid.BorderStyle = BorderStyle.None;
        _compdataGrid.EnableHeadersVisualStyles = false;
        _compdataGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
        _compdataGrid.ColumnHeadersDefaultCellStyle.BackColor = StudioColors.RaisedSurface;
        _compdataGrid.ColumnHeadersDefaultCellStyle.ForeColor = StudioColors.PrimaryText;
        _compdataGrid.ColumnHeadersDefaultCellStyle.Font = Theme.Label;
        _compdataGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = StudioColors.RaisedSurface;
        _compdataGrid.ColumnHeadersHeight = 30;
        _compdataGrid.DefaultCellStyle.BackColor = StudioColors.Surface;
        _compdataGrid.DefaultCellStyle.ForeColor = StudioColors.PrimaryText;
        _compdataGrid.DefaultCellStyle.SelectionBackColor = StudioColors.CyanAccent;
        _compdataGrid.DefaultCellStyle.SelectionForeColor = StudioColors.PrimaryText;
        _compdataGrid.DefaultCellStyle.Font = Theme.Body;
        _compdataGrid.GridColor = StudioColors.CardBorder;
        workspace.Controls.Add(_compdataGrid);
        canvas.Controls.Add(workspace);

        open.Click += (_, _) => OpenCompdataWorkbook();
        openGame.Click += (_, _) => OpenCompdataFromGameFolder();
        _compdataSheets.SelectedIndexChanged += (_, _) => ShowCompdataSheet();
        add.Click += (_, _) => AddCompdataRow();
        build.Click += (_, _) => BuildCompdataLeague();
        advancement.Click += (_, _) => AddCompdataAdvancement();
        validate.Click += (_, _) => ValidateCompdata(showSuccess: true);
        export.Click += (_, _) => ExportCompdataText();
        save.Click += (_, _) => SaveCompdataCopy();
        _compdataSheets.SelectedIndexChanged += (_, _) =>
        {
            add.Enabled = CompdataSchema.CanCreateStandaloneRow(_compdataSheets.SelectedItem as string);
            add.Text = add.Enabled ? "New Object + Auto ID" : "Rows need linked data";
            save.Enabled = _compdataTables.Count > 0;
            validate.Enabled = _compdata.SheetNames.Count > 0;
            export.Enabled = _compdata.SheetNames.Count > 0;
            build.Enabled = _compdata.SheetNames.Count > 0;
            advancement.Enabled = _compdata.SheetNames.Count > 0;
        };
    }

    private void OpenCompdataWorkbook()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open Compdata Workbook",
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            CheckFileExists = true,
        };
        var known = ExternalToolLocator.FindFile(
            Path.Combine("Compdata Tool By Rizco98", "FC26", "26.xlsx"),
            Path.Combine("Compdata", "26.xlsx"));
        if (!string.IsNullOrWhiteSpace(known) && File.Exists(known))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(known);
            dialog.FileName = Path.GetFileName(known);
        }
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            _compdata.Open(dialog.FileName);
            _compdataTables.Clear();
            _compdataSheets.Items.Clear();
            _compdataSheets.Items.AddRange(_compdata.SheetNames.Cast<object>().ToArray());
            if (_compdataSheets.Items.Count > 0) _compdataSheets.SelectedIndex = 0;
            _compdataStatus.ForeColor = Theme.Text;
            _compdataStatus.Text = $"{Path.GetFileName(dialog.FileName)} · {_compdata.SheetNames.Count} worksheets";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Open Compdata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenCompdataFromGameFolder()
    {
        var gameFolder = SettingsService.FC26GameFolder;
        if (string.IsNullOrWhiteSpace(gameFolder) || !Directory.Exists(gameFolder))
        {
            MessageBox.Show(this, "Please set the FC26 Game Folder in Settings first.", "Game Folder Required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var directPath = FindCompdataFolder(gameFolder);
        if (directPath != null)
        {
            OpenCompdataFolder(directPath, "Game folder compdata");
            return;
        }
        var casRoot = ExportCompdataFromCas(gameFolder);
        if (casRoot != null)
        {
            OpenCompdataFolder(casRoot, "Game CAS compdata");
            return;
        }
        MessageBox.Show(this,
            $"Compdata was not found in the game folder.\n" +
            $"Checked folder:\n{directPath ?? Path.Combine(gameFolder, "dlc", "FootballCompEng", "data", "compdata")}\n" +
            "and it is not present in the FC26 CAS containers either.",
            "Compdata Folder Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>Locates a physically installed compdata folder (pre-FC26 layout).</summary>
    private static string? FindCompdataFolder(string gameFolder)
    {
        foreach (var candidate in new[]
        {
            Path.Combine(gameFolder, "dlc", "FootballCompEng", "data", "compdata"),
            Path.Combine(gameFolder, "dlc", "dlc_FootballCompEng", "dlc", "FootballCompEng", "data", "compdata"),
            Path.Combine(gameFolder, "dlc", "dlc_footballcompeng", "dlc", "footballcompeng", "data", "compdata"),
        })
        {
            if (Directory.Exists(candidate)) return candidate;
        }
        return null;
    }

    /// <summary>
    /// FC26 ships compdata inside CORE/ChunkFiles (dlc/dlc_footballcompeng/...)
    /// rather than as loose files. Exports the known compdata TXT files into a
    /// local cache folder and returns its root, or null when nothing exported.
    /// </summary>
    private string? ExportCompdataFromCas(string gameFolder)
    {
        if (!Services.FrostbiteAssets.IsAvailable) Services.FrostbiteAssets.Open(gameFolder);
        if (!Services.FrostbiteAssets.IsAvailable) return null;

        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "compdata-extract");
        var sheetDirectory = Path.Combine(root, "careermode_closedbeta");
        const string prefix =
            "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/careermode_closedbeta/";
        var names = new[]
        {
            "advancement", "activeteams", "compids", "compobj", "initteams",
            "objectives", "schedule", "settings", "standings", "tasks", "weather",
        };
        var exported = 0;
        foreach (var name in names)
        {
            var cached = Path.Combine(sheetDirectory, name + ".txt");
            if (File.Exists(cached)) { exported++; continue; }
            try
            {
                var output = Services.FrostbiteAssets.ExportLegacyAsset(prefix + name + ".txt");
                if (string.IsNullOrWhiteSpace(output) || !File.Exists(output)) continue;
                Directory.CreateDirectory(sheetDirectory);
                File.Copy(output, cached, overwrite: true);
                exported++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CM26] Compdata CAS export skipped: {ex.Message}");
                // A single missing compdata file must not abort the import.
            }
        }
        return exported > 0 ? root : null;
    }

    private void OpenCompdataFolder(string compdataPath, string label)
    {
        try
        {
            _compdata.OpenFromGameFolder(compdataPath);
            _compdataTables.Clear();
            _compdataSheets.Items.Clear();
            _compdataSheets.Items.AddRange(_compdata.SheetNames.Cast<object>().ToArray());
            if (_compdataSheets.Items.Count > 0) _compdataSheets.SelectedIndex = 0;
            _compdataStatus.ForeColor = Theme.Text;
            _compdataStatus.Text = $"{label} · {_compdata.SheetNames.Count} worksheets";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Open Compdata from Game Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowCompdataSheet()
    {
        if (_compdataSheets.SelectedItem is not string sheetName) return;
        try
        {
            if (!_compdataTables.TryGetValue(sheetName, out var table))
            {
                table = _compdata.ReadSheet(sheetName);
                _compdataTables[sheetName] = table;
            }
            _compdataGrid.DataSource = table;
            _compdataGrid.EnableHeadersVisualStyles = false;
            foreach (DataGridViewColumn column in _compdataGrid.Columns)
            {
                column.HeaderCell.Style.BackColor = StudioColors.RaisedSurface;
                column.HeaderCell.Style.ForeColor = StudioColors.PrimaryText;
                column.HeaderCell.Style.Font = Theme.Label;
                column.HeaderCell.Style.SelectionBackColor = StudioColors.RaisedSurface;
                column.DefaultCellStyle.BackColor = StudioColors.Surface;
                column.DefaultCellStyle.ForeColor = StudioColors.PrimaryText;
                column.DefaultCellStyle.SelectionBackColor = StudioColors.CyanAccent;
                column.DefaultCellStyle.SelectionForeColor = StudioColors.PrimaryText;
                column.DefaultCellStyle.Font = Theme.Body;
            }
            var limit = CompdataSchema.GetRowLimit(sheetName);
            _compdataStatus.ForeColor = Theme.Text;
            _compdataStatus.Text = CompdataSchema.CanCreateStandaloneRow(sheetName)
                ? $"{sheetName} · {table.Rows.Count:N0}/{limit?.ToString("N0") ?? "—"} objects · IDs are allocated automatically."
                : $"{sheetName} · {table.Rows.Count:N0}/{limit?.ToString("N0") ?? "—"} rows · linked data must reference an existing object.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Compdata Worksheet", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddCompdataRow()
    {
        if (_compdataSheets.SelectedItem is not string sheetName ||
            !CompdataSchema.CanCreateStandaloneRow(sheetName))
        {
            MessageBox.Show(this,
                "Only compobj can create an independent row. Other Compdata worksheets reference an existing object and must be completed together.",
                "Linked Compdata Row", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_compdataGrid.DataSource is not DataTable table || table.Columns.Count == 0) return;
        EnsureAllCompdataTables();
        CompdataSchema.EnsureCapacity(_compdataTables, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { [sheetName] = 1 });
        var row = table.NewRow();
        var used = table.Rows.Cast<DataRow>()
            .Select(item => int.TryParse(Convert.ToString(item[0]), out var id) ? id : -1)
            .Where(id => id >= 0)
            .ToHashSet();
        var nextId = used.Count == 0 ? 1 : used.Max() + 1;
        while (used.Contains(nextId)) nextId++;
        row[0] = nextId.ToString();
        table.Rows.Add(row);
        _compdataGrid.CurrentCell = _compdataGrid.Rows[^1].Cells[0];
        _compdataStatus.Text = $"Created object {nextId}. Set its type and parent, then add matching linked rows where needed.";
    }

    private void BuildCompdataLeague()
    {
        if (!CompdataCreationDialog.TryShowLeague(this, out var request)) return;
        try
        {
            EnsureAllCompdataTables();
            var result = CompdataBuilder.CreateLeagueOrCup(_compdataTables, request);
            _compdataSheets.SelectedItem = "compobj";
            ShowCompdataSheet();
            _compdataStatus.Text = $"Created {request.Name}: competition object {result.CompetitionObjectId}, {result.StageIds.Count} stage(s), {result.GroupIds.Count} group(s).";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Build League / Cup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddCompdataAdvancement()
    {
        if (!CompdataCreationDialog.TryShowAdvancement(this, out var link)) return;
        try
        {
            EnsureAllCompdataTables();
            CompdataBuilder.AddAdvancement(_compdataTables, link.Source, link.SourceRank, link.Destination, link.DestinationRank);
            _compdataSheets.SelectedItem = "advancement";
            ShowCompdataSheet();
            _compdataStatus.Text = $"Added advancement: group {link.Source} rank {link.SourceRank} → group {link.Destination} rank {link.DestinationRank}.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Promotion / Relegation", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EnsureAllCompdataTables()
    {
        foreach (var sheetName in _compdata.SheetNames)
            if (!_compdataTables.ContainsKey(sheetName)) _compdataTables[sheetName] = _compdata.ReadSheet(sheetName);
    }

    private bool ValidateCompdata(bool showSuccess)
    {
        _compdataGrid.EndEdit();
        try
        {
            EnsureAllCompdataTables();
            var issues = CompdataSchema.Validate(_compdataTables);
            if (issues.Count == 0)
            {
                _compdataStatus.Text = "Compdata validation passed · object and linked-row references are consistent.";
                _compdataStatus.ForeColor = Theme.Success;
                if (showSuccess)
                    MessageBox.Show(this, _compdataStatus.Text, "Compdata Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            var preview = string.Join(Environment.NewLine, issues.Take(12)
                .Select(issue => $"{issue.Sheet}, row {issue.Row}: {issue.Message}"));
            _compdataStatus.Text = $"Compdata validation found {issues.Count} issue(s).";
            _compdataStatus.ForeColor = Theme.Warning;
            MessageBox.Show(this, preview + (issues.Count > 12 ? Environment.NewLine + "…" : string.Empty),
                "Compdata Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        catch (Exception ex)
        {
            _compdataStatus.Text = "Compdata validation failed: " + ex.Message;
            _compdataStatus.ForeColor = Theme.Danger;
            MessageBox.Show(this, ex.Message, "Compdata Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void SaveCompdataCopy()
    {
        if (_compdataTables.Count == 0) return;
        if (!ValidateCompdata(showSuccess: false)) return;
        using var dialog = new SaveFileDialog
        {
            Title = "Save Compdata Copy",
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            FileName = Path.GetFileNameWithoutExtension(_compdata.FilePath) + "-edited.xlsx",
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            _compdata.SaveCopy(dialog.FileName, _compdataTables);
            _compdataStatus.Text = $"Saved verified workbook copy: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save Compdata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportCompdataText()
    {
        if (_compdata.SheetNames.Count == 0 || !ValidateCompdata(showSuccess: false)) return;
        using var dialog = new FolderBrowserDialog { Description = "Choose an empty folder for Compdata TXT files" };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            EnsureAllCompdataTables();
            CompdataWorkbookService.ExportTextFiles(dialog.SelectedPath, _compdataTables);
            _compdataStatus.Text = $"Exported {_compdataTables.Count} UTF-8 Compdata TXT files to: {dialog.SelectedPath}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Export Compdata TXT", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnRecordShown()
    {
        var id = CurrentValues.TryGetValue("competitionid", out var value) ? Parse(value.RawValue) : 0;
        var legacyPath = $"data/ui/imgAssets/competition/{id}.dds";
        LegacyAssetActions.SetTarget(_logo, new LegacyAssetEditTarget(legacyPath, 256, 256));
        // Competition emblems live in FC26's legacy UI store. A generic
        // texture search can collide with a club/team id and show the wrong badge.
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _logo,
            Services,
            LegacyAssetActions.Replacement(Services, legacyPath) ?? Services.Assets.GetCompetitionLogo(id),
            legacyPath,
            (image, _) =>
            {
                _logo.Image?.Dispose();
                _logo.Image = image;
            });
    }

    /// <summary>
    /// Refreshes the tree on every activation: row indices captured when the
    /// section was built become stale after staged inserts shift the table.
    /// </summary>
    public override void ActivateSection()
    {
        base.ActivateSection();
        PopulateCompetitionTree(Services);
    }

    private void PopulateCompetitionTree(AppServices services)
    {
        _competitionTree.BeginUpdate();
        try
        {
            _competitionTree.Nodes.Clear();
            var table = services.Session.GetTable("competition");
            if (table == null) return;
            var id = Col(table, "competitionid");
            var nation = Col(table, "country_lock");
            var groups = new SortedDictionary<string, List<TreeNode>>(StringComparer.OrdinalIgnoreCase);
            for (var row = 0; row < table.RowCount; row++)
            {
                var record = services.Session.GetRecord("competition", row);
                if (record == null) continue;
                var nationName = services.Resolver?.NationName(Parse(record.Get(nation))) ?? "Unassigned";
                if (string.IsNullOrWhiteSpace(nationName)) nationName = "Unassigned";
                if (!groups.TryGetValue(nationName, out var nodes)) groups[nationName] = nodes = [];
                nodes.Add(new TreeNode($"Competition {record.Get(id)}") { Tag = row });
            }
            foreach (var (nationName, nodes) in groups)
            {
                var parent = new TreeNode($"{nationName} ({nodes.Count})");
                parent.Nodes.AddRange(nodes.ToArray());
                _competitionTree.Nodes.Add(parent);
            }
        }
        finally { _competitionTree.EndUpdate(); }
    }

    private static IReadOnlyList<RecordListItem> Records(AppServices s)
    {
        var table = s.Session.GetTable("competition");
        if (table == null) return Array.Empty<RecordListItem>();
        var id = Col(table, "competitionid"); var nation = Col(table, "country_lock"); var ball = Col(table, "ballid");
        var items = new List<RecordListItem>();
        for (var r = 0; r < table.RowCount; r++)
        {
            var record = s.Session.GetRecord("competition", r);
            if (record == null) continue;
            items.Add(new RecordListItem { RecordIndex = r, Title = "Competition " + record.Get(id), Subtitle = s.Resolver!.NationName(Parse(record.Get(nation))), Detail = "Ball " + record.Get(ball) });
        }
        return items.OrderBy(x => x.Title).ToList();
    }
}

public sealed class BallsSection : ClassicEntitySection
{
    private readonly PictureBox _texture;

    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ballid"] = "Id", ["balltype"] = "Type", ["islicensed"] = "Licensed",
        ["isavailableinstore"] = "Visible in Game Menu", ["isembargoed"] = "Embargoed",
        ["isrewardable"] = "Rewardable"
    };

    protected override bool UseStudioToolbar => true;

    public BallsSection(AppServices s) : base(s, "balls", "Balls", "teamballs", () => Records(s), Fields)
    {
        var general = AddCanvasTab("General");
        AttachStudioToolbar(general, "Balls");
        var c = Canvas(general);
        var texture = Group("Texture", new Point(3, 3), new Size(720, 580));
        _texture = ImageSurface(texture, new Point(4, 20), new Size(700, 500), "1024 x 1024");
        LegacyAssetActions.Attach(Services, texture, _texture, new Point(8, 545), () => OnRecordShown());
        c.Controls.Add(texture);
        var model = Group("3D Model", new Point(728, 3), new Size(1165, 550));
        ThreeDViewerLauncher.AttachPlaceholder(model, new Point(5, 20), new Size(1145, 495), "ball",
            () => new[] { Value("ballid").ToString(), "ball_" + Value("ballid") },
            () => Services.FrostbiteAssets.ExportMeshForQuery(new[] { $"ball_{Value("ballid")}" }));
        c.Controls.Add(model);
        var values = Group("Info", new Point(3, 589), new Size(720, 80));
        AddField(values, "ballid", "Id", new Point(110, 20), 170);
        AddField(values, "balltype", "Ball Type", new Point(110, 46), 170);
        AddField(values, "isavailableinstore", "Game Menu", new Point(390, 20), 170);
        AddField(values, "islicensed", "Licensed", new Point(390, 46), 170);
        c.Controls.Add(values);
    }

    protected override void OnRecordShown()
    {
        var ballId = CurrentValues.TryGetValue("ballid", out var value) ? Parse(value.RawValue) : 0;
        var legacyPath = $"data/ui/imgAssets/balls/ball_{ballId}.dds";
        LegacyAssetActions.SetTarget(_texture, new LegacyAssetEditTarget(legacyPath, 1024, 1024));
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _texture,
            Services,
            LegacyAssetActions.Replacement(Services, legacyPath) ?? Services.Assets.GetBall(ballId),
            legacyPath,
            (image, _) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                _texture.Image?.Dispose();
                _texture.Image = image;
            });
    }

    private static IReadOnlyList<RecordListItem> Records(AppServices s)
    {
        var table = s.Session.GetTable("teamballs");
        if (table == null) return Array.Empty<RecordListItem>();
        var id = Col(table, "ballid"); var type = Col(table, "balltype");
        var items = new List<RecordListItem>();
        for (var r = 0; r < table.RowCount; r++)
        {
            var record = s.Session.GetRecord("teamballs", r);
            if (record == null) continue;
            items.Add(new RecordListItem { RecordIndex = r, Title = "Ball " + record.Get(id), Subtitle = "Type " + record.Get(type) });
        }
        return items.OrderBy(x => x.Title).ToList();
    }
}

public sealed class BootsSection : ClassicEntitySection
{
    private readonly PictureBox _texture;

    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["manufacturerid"] = "Brand", ["shoetype"] = "Shoes Type",
        ["shoedesign"] = "Design", ["shoecolor1"] = "Color 1",
        ["shoecolor2"] = "Color 2", ["islicensed"] = "Licensed", ["gender"] = "Gender"
    };

    protected override bool UseStudioToolbar => true;

    public BootsSection(AppServices s) : base(s, "boots", "Boots", "playerboots", () => Records(s), Fields)
    {
        var general = AddCanvasTab("General");
        AttachStudioToolbar(general, "Boots");
        var c = Canvas(general);
        var texture = Group("Texture", new Point(3, 3), new Size(516, 580));
        _texture = ImageSurface(texture, new Point(4, 20), new Size(500, 500), "512 x 512");
        LegacyAssetActions.Attach(Services, texture, _texture, new Point(8, 545), () => OnRecordShown());
        c.Controls.Add(texture);
        var model = Group("3D Model", new Point(524, 3), new Size(1360, 790));
        ThreeDViewerLauncher.AttachPlaceholder(model, new Point(5, 20), new Size(1340, 730), "boot",
            () => new[]
            {
                Value("shoetype").ToString(), "item_" + Value("shoetype") + "_" + Value("shoedesign"),
                Value("shoetype").ToString() + "_" + Value("shoedesign") + "_" + Value("shoecolor1")
            },
            () => Services.FrostbiteAssets.ExportMeshForQuery(new[] { $"boot_{Value("shoetype")}_{Value("shoedesign")}", $"item_{Value("shoetype")}" }));
        c.Controls.Add(model);
        var details = Group("Shoes", new Point(3, 589), new Size(516, 100));
        AddField(details, "manufacturerid", "Brand", new Point(110, 20), 120);
        AddField(details, "shoetype", "Type", new Point(110, 46), 120);
        AddField(details, "shoedesign", "Design", new Point(110, 72), 120);
        AddField(details, "islicensed", "Licensed", new Point(300, 20), 120);
        c.Controls.Add(details);
        var colours = AddCanvasTab("Colors"); var cc = Canvas(colours);
        var col = Group("Colors", new Point(3, 3), new Size(510, 80));
        AddField(col, "shoecolor1", "Color 1", new Point(105, 20), 130);
        AddField(col, "shoecolor2", "Color 2", new Point(105, 46), 130);
        cc.Controls.Add(col);
    }

    protected override void OnRecordShown()
    {
        var shoeType = Number("shoetype");
        var shoeDesign = Number("shoedesign");
        var color1 = Number("shoecolor1");
        var color2 = Number("shoecolor2");
        // The canonical FC26 UI file is item_{type}_{design}_{color1}_{color2}.
        // Brand is deliberately not part of this filename (the database's
        // manufacturerid describes metadata rather than the preview resource).
        var legacyPath = $"data/ui/imgAssets/boots/item_{shoeType}_{shoeDesign}_{color1}_{color2}.dds";
        LegacyAssetActions.SetTarget(_texture, new LegacyAssetEditTarget(legacyPath, 512, 512));
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _texture,
            Services,
            LegacyAssetActions.Replacement(Services, legacyPath) ?? Services.Assets.GetBoot(shoeType),
            legacyPath,
            (image, _) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                _texture.Image?.Dispose();
                _texture.Image = image;
            });
    }

    private int Number(string field) =>
        CurrentValues.TryGetValue(field, out var value) ? Parse(value.RawValue) : 0;

    private static IReadOnlyList<RecordListItem> Records(AppServices s)
    {
        var table = s.Session.GetTable("playerboots");
        if (table == null) return Array.Empty<RecordListItem>();
        var brand = Col(table, "manufacturerid"); var type = Col(table, "shoetype");
        var items = new List<RecordListItem>();
        for (var r = 0; r < table.RowCount; r++)
        {
            var record = s.Session.GetRecord("playerboots", r);
            if (record == null) continue;
            items.Add(new RecordListItem { RecordIndex = r, Title = "Boots " + (r + 1), Subtitle = "Manufacturer " + record.Get(brand), Detail = "Type " + record.Get(type) });
        }
        return items;
    }
}

/// <summary>Goalkeeper gloves with a game-native texture preview and the complete FC26 record tab.</summary>
internal sealed class GlovesSection : ClassicEntitySection
{
    private readonly PictureBox _texture;
    private readonly Label _caption = new();

    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gloveid"] = "Glove Id", ["isavailableinstore"] = "Available in Store",
        ["islicensed"] = "Licensed", ["isembargoed"] = "Embargoed"
    };

    protected override bool UseStudioToolbar => true;

    public GlovesSection(AppServices s) : base(s, "gloves", "Gloves", "goalkeepergloves", () => Records(s), Fields)
    {
        var general = AddCanvasTab("General");
        AttachStudioToolbar(general, "Gloves");
        var canvas = Canvas(general);
        var texture = Group("Glove Texture", new Point(3, 3), new Size(720, 632));
        _texture = ImageSurface(texture, new Point(4, 20), new Size(700, 520), "Installed texture");
        _caption.Location = new Point(8, 566);
        _caption.Size = new Size(695, 26);
        _caption.TextAlign = ContentAlignment.MiddleCenter;
        _caption.Font = LegacyFont;
        texture.Controls.Add(_caption);
        LegacyAssetActions.Attach(Services, texture, _texture, new Point(8, 598), () => OnRecordShown());
        canvas.Controls.Add(texture);

        var info = Group("Goalkeeper Gloves", new Point(728, 3), new Size(480, 126));
        AddField(info, "gloveid", "Id", new Point(125, 20), 175);
        AddField(info, "isavailableinstore", "Store", new Point(125, 46), 175);
        AddField(info, "islicensed", "Licensed", new Point(125, 72), 175);
        AddField(info, "isembargoed", "Embargoed", new Point(125, 98), 175);
        canvas.Controls.Add(info);
    }

    protected override void OnRecordShown()
    {
        var gloveId = CurrentValues.TryGetValue("gloveid", out var value) ? Parse(value.RawValue) : 0;
        var candidates = new[]
        {
            $"data/ui/imgAssets/gloves/gloves_{gloveId}.dds",
            $"data/ui/imgAssets/gloves/glove_{gloveId}.dds",
            $"data/ui/imgAssets/gkgloves/gloves_{gloveId}.dds"
        };
        var stagedPath = candidates.FirstOrDefault(x => Services.LegacyMods.GetReplacement(x) != null);
        var target = stagedPath ?? candidates[0];
        LegacyAssetActions.SetTarget(_texture, new LegacyAssetEditTarget(target, 512, 512));
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(
            _texture,
            Services,
            stagedPath == null ? Services.Assets.GetGlove(gloveId) : Services.LegacyMods.GetReplacement(stagedPath),
            candidates,
            (image, source) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                _texture.Image?.Dispose();
                _texture.Image = image;
                _caption.Text = image == null
                    ? $"No glove texture found for glove {gloveId}"
                    : $"Glove texture · {source}";
            },
            path => LegacyAssetActions.SetTarget(
                _texture, new LegacyAssetEditTarget(path, 512, 512)));
    }

    private static IReadOnlyList<RecordListItem> Records(AppServices s)
    {
        var table = s.Session.GetTable("goalkeepergloves");
        if (table == null) return Array.Empty<RecordListItem>();
        var id = Col(table, "gloveid");
        var licensed = Col(table, "islicensed");
        var items = new List<RecordListItem>();
        for (var row = 0; row < table.RowCount; row++)
        {
            var record = s.Session.GetRecord("goalkeepergloves", row);
            if (record == null) continue;
            items.Add(new RecordListItem
            {
                RecordIndex = row,
                Title = $"Gloves {record.Get(id)}",
                Subtitle = licensed >= 0 ? $"Licensed {record.Get(licensed)}" : "Goalkeeper gloves"
            });
        }
        return items;
    }
}
