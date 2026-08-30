using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>Classic CM26 Compdata workspace embedded in the Competition section.</summary>
internal sealed class Fc26CompdataPanel : UserControl
{
    private readonly ListBox _sheets = new ListBox();
    private readonly DataGridView _grid = Grid(false);
    private readonly TreeView _structure = new TreeView();
    private readonly DataGridView _calendar = Grid(false);
    private readonly Label _status = new Label();
    private readonly TabControl _views = new TabControl();
    private readonly Dictionary<string, DataTable> _tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
    private string _sourcePath = string.Empty;

    internal Fc26CompdataPanel()
    {
        Dock = DockStyle.Fill;
        MinimumSize = new Size(720, 480);
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tools.Items.Add(Item("Load FC26 Compdata", (_, _) => LoadInstalled()));
        tools.Items.Add(Item("Open Tournament Files", (_, _) => OpenFolder()));
        tools.Items.Add(Item("Open Workbook", (_, _) => OpenWorkbook()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Item("Create Tournament", (_, _) => TournamentWizard()));
        tools.Items.Add(Item("Add Advancement", (_, _) => AddAdvancement()));
        tools.Items.Add(Item("Assign Teams", (_, _) => AssignTeams()));
        tools.Items.Add(Item("Generate Schedule", (_, _) => GenerateSchedule()));
        tools.Items.Add(Item("Career Ready Check", (_, _) => ShowCareerReadyReport()));
        tools.Items.Add(Item("Make League In-Game Ready", (_, _) => ChooseLeagueForCareerSetup()));
        tools.Items.Add(Item("Stage Compdata to Save", (_, _) => StageCurrentCompdata()));
        tools.Items.Add(new ToolStripSeparator());
		tools.Items.Add(Item("Add New League", (_, _) => MainForm.CM?.CreateNewLeagueWorkflow()));
		tools.Items.Add(Item("Add New Team", (_, _) => MainForm.CM?.CreateNewTeamWorkflow()));
		tools.Items.Add(Item("Add New Nation", (_, _) => MainForm.CM?.CreateFriendlyEntity("nation")));
		tools.Items.Add(Item("Add New Player", (_, _) => MainForm.CM?.CreateFriendlyEntity("player")));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Item("Validate", (_, _) => ValidateTables()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Item("Save workbook copy", (_, _) => SaveWorkbook()));
        tools.Items.Add(Item("Export game TXT", (_, _) => ExportTxt()));
        _sheets.Dock = DockStyle.Fill;
        _sheets.SelectedIndexChanged += (_, _) => ShowSheet();
        _grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        _grid.DataError += (_, e) => e.ThrowException = false;
        _structure.Dock = DockStyle.Fill;
        _structure.HideSelection = false;
        _structure.AfterSelect += (_, e) =>
            _status.Text = e.Node == null ? "Select a competition object." : "Selected: " + e.Node.Text;
        _views.Dock = DockStyle.Fill;
        var structurePage = new TabPage("Competition Structure") { Controls = { _structure } };
        _views.TabPages.Add(structurePage);
        _views.TabPages.Add(new TabPage("Tournament Calendar") { Controls = { _calendar } });
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 210 };
        var sheetPanel = new Panel { Dock = DockStyle.Fill };
        var sheetLabel = new Label
        {
            Text = "Tournament Sections", Dock = DockStyle.Top, Height = 27,
            Padding = new Padding(6, 7, 0, 0), Font = new Font(Font, FontStyle.Bold)
        };
        sheetPanel.Controls.Add(_sheets); sheetPanel.Controls.Add(sheetLabel);
        split.Panel1.Controls.Add(sheetPanel); split.Panel2.Controls.Add(_views);
        _status.Dock = DockStyle.Bottom; _status.Height = 24; _status.Padding = new Padding(6, 4, 0, 0);
        _status.Text = "Open tournament files to edit the structure and calendar with guided controls.";
        Controls.Add(split); Controls.Add(_status); Controls.Add(tools);
    }

    private void OpenWorkbook()
    {
        using var dialog = new OpenFileDialog { Filter = "Compdata workbook|*.xlsx|All files|*.*" };
        if (dialog.ShowDialog(this) == DialogResult.OK) LoadSource(dialog.FileName);
    }

    private void OpenFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select the extracted tournament data folder" };
        if (dialog.ShowDialog(this) == DialogResult.OK) LoadSource(dialog.SelectedPath);
    }

    private void LoadSource(string source)
    {
        Run("Loading Compdata...", () =>
        {
            var path = Fc26HostBridge.OpenCompdata(source);
            try { LoadSnapshot(path); } finally { try { File.Delete(path); } catch { } }
            _sourcePath = source;
            return _tables.Count + " Compdata sheet(s) loaded with structure and calendar views.";
        });
    }

    private void LoadSnapshot(string path)
    {
        var snapshot = JsonSerializer.Deserialize<CompdataSnapshot>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Compdata snapshot is empty.");
        _tables.Clear();
        foreach (var sheet in snapshot.Sheets)
        {
            var table = new DataTable(sheet.Name);
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var sourceColumn in sheet.Columns)
            {
                var column = string.IsNullOrWhiteSpace(sourceColumn) ? "Column" : sourceColumn;
                var unique = column;
                for (var suffix = 2; !used.Add(unique); suffix++) unique = column + " (" + suffix + ")";
                table.Columns.Add(unique, typeof(string));
            }
            foreach (var values in sheet.Rows)
            {
                var row = table.NewRow();
                for (var index = 0; index < table.Columns.Count && index < values.Count; index++) row[index] = values[index] ?? string.Empty;
                table.Rows.Add(row);
            }
            _tables[table.TableName] = table;
        }
        _sheets.DataSource = _tables.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Select(name => new CompdataSheetChoice(name, FriendlySectionName(name))).ToArray();
        if (_sheets.Items.Count > 0) _sheets.SelectedIndex = 0;
        RefreshSimpleViews();
    }

    private void ShowSheet()
    {
        var name = (_sheets.SelectedItem as CompdataSheetChoice)?.Name;
        _grid.DataSource = !string.IsNullOrWhiteSpace(name) && _tables.TryGetValue(name, out var table) ? table : null;
        if (_grid.Columns.Count > 0) _grid.Columns[_grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
    }

    private void RefreshSimpleViews() { BuildStructureTree(); BuildCalendar(); }

    private void BuildStructureTree()
    {
        _structure.BeginUpdate(); _structure.Nodes.Clear();
        if (!_tables.TryGetValue("compobj", out var objects)) { _structure.EndUpdate(); return; }
        var nodes = new Dictionary<int, TreeNode>(); var parents = new Dictionary<int, int>();
        foreach (DataRow row in objects.Rows)
        {
            if (!Int(row, 0, out var id)) continue;
            Int(row, 1, out var type); Int(row, 4, out var parent);
            var name = string.IsNullOrWhiteSpace(Value(row, 3)) ? Value(row, 2) : Value(row, 3);
            nodes[id] = new TreeNode(id + " · " + TypeName(type) + " · " + name) { Tag = id }; parents[id] = parent;
        }
        foreach (var pair in nodes.OrderBy(pair => pair.Key))
        {
            if (parents.TryGetValue(pair.Key, out var parent) && parent != pair.Key && nodes.TryGetValue(parent, out var parentNode))
                parentNode.Nodes.Add(pair.Value);
            else _structure.Nodes.Add(pair.Value);
        }
        _structure.ExpandAll(); _structure.EndUpdate();
    }

    private void BuildCalendar()
    {
        var view = new DataTable("Calendar");
        foreach (var name in new[] { "Object ID", "Competition / Phase", "Day", "Round", "Minimum Games", "Maximum Games", "Kick-off", "Status" }) view.Columns.Add(name);
        if (_tables.TryGetValue("schedule", out var schedule))
        {
            var names = ObjectNames();
            var duplicates = schedule.Rows.Cast<DataRow>().Where(row => row.RowState != DataRowState.Deleted)
                .GroupBy(row => string.Join("|", Enumerable.Range(0, Math.Min(6, schedule.Columns.Count))
                    .Select(column => Value(row, column))), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in schedule.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                var idText = Value(row, 0); int.TryParse(idText, out var id);
                var key = string.Join("|", Enumerable.Range(0, Math.Min(6, schedule.Columns.Count))
                    .Select(column => Value(row, column)));
                var state = duplicates.Contains(key) ? "Conflict: exact duplicate schedule row" : CalendarStatus(row);
                view.Rows.Add(idText, names.TryGetValue(id, out var objectName) ? objectName : "Object " + idText,
                    Value(row, 1), Value(row, 2), Value(row, 3), Value(row, 4), FormatKickoff(Value(row, 5)), state);
            }
        }
        _calendar.DataSource = view;
        if (_calendar.Columns.Count > 1) _calendar.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
    }

    private void TournamentWizard()
    {
        EnsureLoaded(); using var dialog = new TournamentWizardDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run("Creating validated tournament structure...", () =>
        {
            var snapshot = WriteSnapshot();
            try
            {
                var result = Fc26HostBridge.BuildCompdata(snapshot, dialog.CompetitionName, dialog.DatabaseId, dialog.Stages, dialog.Groups);
                LoadSnapshot(snapshot); _views.SelectedIndex = 0; return result;
            }
            finally { try { File.Delete(snapshot); } catch { } }
        });
    }

    private void AddAdvancement()
    {
        EnsureLoaded(); var groups = GroupChoices();
        if (groups.Length < 2) { MessageBox.Show(this, "At least two Compdata Group objects are required.", Text); return; }
        using var dialog = new AdvancementDialog(groups);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run("Adding group-to-group advancement path...", () =>
        {
            var snapshot = WriteSnapshot();
            try
            {
                var result = Fc26HostBridge.AddCompdataAdvancement(snapshot, dialog.Source.Id, dialog.SourceRank, dialog.Destination.Id, dialog.DestinationRank);
                LoadSnapshot(snapshot); return result;
            }
            finally { try { File.Delete(snapshot); } catch { } }
        });
    }

    private void LoadInstalled()
    {
        Run("Loading installed FC26 Compdata...", () =>
        {
            LoadInstalledCore();
            return _tables.Count + " installed FC26 Compdata section(s) loaded.";
        });
    }

    private void LoadInstalledCore()
    {
        var path = Fc26HostBridge.OpenInstalledCompdata();
        try { LoadSnapshot(path); }
        finally { try { File.Delete(path); } catch { } }
        _sourcePath = string.Empty;
    }

    private CompetitionChoice[] CompetitionChoices()
    {
        if (!_tables.TryGetValue("compobj", out var objects)) return Array.Empty<CompetitionChoice>();
        return objects.Rows.Cast<DataRow>()
            .Where(row => Int(row, 0, out _) && Int(row, 1, out var type) && type == 3)
            .Select(row => new CompetitionChoice(Convert.ToInt32(Value(row, 0)),
                string.IsNullOrWhiteSpace(Value(row, 3)) ? Value(row, 2) : Value(row, 3)))
            .OrderBy(choice => choice.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private void AssignTeams()
    {
        EnsureLoaded();
        if (!_tables.TryGetValue("initteams", out var initTeams))
        { MessageBox.Show(this, "The opened Compdata has no initteams section.", Text); return; }
        var competitions = CompetitionChoices();
        if (competitions.Length == 0) { MessageBox.Show(this, "Create a tournament before assigning teams.", Text); return; }
        var teams = FifaEnvironment.Teams?.Cast<Team>().Where(team => !team.NationalTeam)
            .OrderBy(team => team.TeamNameFull, StringComparer.OrdinalIgnoreCase)
            .Select(team => new TeamChoice(team.Id, string.IsNullOrWhiteSpace(team.TeamNameFull) ? team.DatabaseName : team.TeamNameFull))
            .ToArray() ?? Array.Empty<TeamChoice>();
        using var dialog = new TeamAssignmentDialog(competitions, teams, competitionId =>
            initTeams.Rows.Cast<DataRow>().Where(row => Value(row, 0) == competitionId.ToString())
                .Select(row => Int(row, 2, out var id) ? id : -1).Where(id => id >= 0).ToHashSet());
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run("Assigning teams to tournament...", () =>
        {
            var selected = dialog.TeamIds.Distinct().ToArray();
            if (selected.Length < 2) throw new InvalidOperationException("Select at least two teams.");
            foreach (var row in initTeams.Rows.Cast<DataRow>()
                         .Where(row => Value(row, 0) == dialog.Competition.Id.ToString()).ToArray()) row.Delete();
            initTeams.AcceptChanges();
            var position = 0;
            foreach (var teamId in selected)
            {
                var row = initTeams.NewRow();
                row[0] = dialog.Competition.Id.ToString();
                if (initTeams.Columns.Count > 1) row[1] = (position++).ToString();
                if (initTeams.Columns.Count > 2) row[2] = teamId.ToString();
                initTeams.Rows.Add(row);
            }
            RefreshSimpleViews();
            return selected.Length + " team(s) assigned to " + dialog.Competition.Name + ".";
        });
    }

    private void GenerateSchedule()
    {
        EnsureLoaded();
        if (!_tables.TryGetValue("compobj", out var objects) || !_tables.TryGetValue("initteams", out var initTeams) ||
            !_tables.TryGetValue("schedule", out var schedule))
        { MessageBox.Show(this, "Compdata requires compobj, initteams and schedule sections.", Text); return; }
        var competitions = CompetitionChoices();
        if (competitions.Length == 0) { MessageBox.Show(this, "Create a tournament first.", Text); return; }
        using var dialog = new ScheduleGeneratorDialog(competitions);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run("Generating round-robin calendar...", () =>
        {
            var competitionId = dialog.Competition.Id;
            var teamCount = initTeams.Rows.Cast<DataRow>().Count(row => Value(row, 0) == competitionId.ToString());
            if (teamCount < 2) throw new InvalidOperationException("Assign at least two teams before generating a schedule.");
            var stages = objects.Rows.Cast<DataRow>().Where(row =>
                    Int(row, 0, out _) && Int(row, 1, out var type) && type == 4 &&
                    Int(row, 4, out var parent) && parent == competitionId)
                .Select(row => Convert.ToInt32(Value(row, 0))).ToArray();
            if (stages.Length == 0) throw new InvalidOperationException("The tournament has no stage object.");
            foreach (var row in schedule.Rows.Cast<DataRow>()
                         .Where(row => Int(row, 0, out var objectId) && stages.Contains(objectId)).ToArray()) row.Delete();
            schedule.AcceptChanges();
            var singleLegRounds = teamCount % 2 == 0 ? teamCount - 1 : teamCount;
            var rounds = singleLegRounds * dialog.Legs;
            var games = teamCount / 2;
            foreach (var stage in stages)
                for (var round = 1; round <= rounds; round++)
                {
                    var row = schedule.NewRow();
                    var values = new[] { stage, dialog.StartDay + ((round - 1) * dialog.DayInterval), round, games, games, dialog.Kickoff };
                    for (var column = 0; column < values.Length && column < schedule.Columns.Count; column++) row[column] = values[column].ToString();
                    schedule.Rows.Add(row);
                }
            RefreshSimpleViews(); _views.SelectedIndex = 1;
            return rounds + " round(s) generated for " + teamCount + " teams.";
        });
    }

    private void ShowCareerReadyReport()
    {
        EnsureLoaded();
        var report = BuildCareerReadyReport();
        using var viewer = new Form { Text = "Career Ready Check", Size = new Size(820, 560), StartPosition = FormStartPosition.CenterParent };
        viewer.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
            ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9f), Text = report });
        viewer.ShowDialog(this);
    }

    private void ChooseLeagueForCareerSetup()
    {
        var leagues = FifaEnvironment.Leagues?.Cast<League>()
            .OrderBy(league => league.ToString(), StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<League>();
        if (leagues.Length == 0) { MessageBox.Show(this, "Open an FC26 database first.", Text); return; }
        using var dialog = new Form { Text = "Make League In-Game Ready", FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(470, 135), MaximizeBox = false, MinimizeBox = false };
        var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420, DataSource = leagues };
        var ok = new Button { Text = "Build and Stage", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(14) };
        layout.Controls.Add(new Label { Text = "League", AutoSize = true }); layout.Controls.Add(combo);
        var buttons = new FlowLayoutPanel { AutoSize = true }; buttons.Controls.AddRange(new Control[] { ok, cancel }); layout.Controls.Add(buttons);
        dialog.Controls.Add(layout); dialog.AcceptButton = ok; dialog.CancelButton = cancel;
        if (dialog.ShowDialog(this) == DialogResult.OK && combo.SelectedItem is League league) MakeLeagueInGameReady(league);
    }

    internal void MakeLeagueInGameReady(League league)
    {
        if (league == null) return;
        Run("Building and staging an in-game Career league...", () =>
        {
            var result = StageLeagueForSave(league);
            MessageBox.Show(this, result + Environment.NewLine + Environment.NewLine +
                "Use the normal CM26 Save command to commit the database and Compdata together. Start a new Career after saving.",
                "League In-Game Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return result;
        });
    }

	internal string StageLeagueForSave(League league)
	{
		if (league == null) throw new InvalidOperationException("Select a league first.");
		if (league.Country == null) throw new InvalidOperationException("Assign a country to this league first.");
		var teams = league.PlayingTeams.Cast<Team>().Where(team => team != null && team.Id > 0)
			.Select(team => team.Id).Distinct().ToArray();
		if (teams.Length < 2) throw new InvalidOperationException("Add at least two teams to this league first.");
		if (_tables.Count == 0) LoadInstalledCore();
		var snapshot = WriteSnapshot();
		try
		{
			var result = Fc26HostBridge.BuildCareerCompdata(snapshot,
				league.Country.DatabaseName, league.Country.Id, league.Country.Confederation + 1,
				string.IsNullOrWhiteSpace(league.LongName) ? league.leaguename : league.LongName,
				league.Id, teams);
			LoadSnapshot(snapshot);
			var stagedSnapshot = WriteSnapshot();
			try
			{
				var staged = Fc26HostBridge.StageCompdataForSave(stagedSnapshot);
				RefreshSimpleViews(); _views.SelectedIndex = 0;
				return result + " " + staged;
			}
			finally { try { File.Delete(stagedSnapshot); } catch { } }
		}
		finally { try { File.Delete(snapshot); } catch { } }
	}

    private void StageCurrentCompdata()
    {
        Run("Staging validated Compdata for Save...", () =>
        {
            EnsureLoaded(); var snapshot = WriteSnapshot();
            try { return Fc26HostBridge.StageCompdataForSave(snapshot) + " Use normal CM26 Save to commit it."; }
            finally { try { File.Delete(snapshot); } catch { } }
        });
    }

    private string BuildCareerReadyReport()
    {
        if (!_tables.TryGetValue("compobj", out var objects)) return "NOT READY: compobj is missing.";
        _tables.TryGetValue("compids", out var compIds); _tables.TryGetValue("initteams", out var initTeams);
        _tables.TryGetValue("schedule", out var schedule); _tables.TryGetValue("settings", out var settings);
        var lines = new List<string> { "CM26 COMPDATA CAREER READINESS", new string('=', 36), string.Empty };
        var readyCount = 0;
        foreach (var competition in CompetitionChoices())
        {
            var competitionRow = objects.Rows.Cast<DataRow>().First(row =>
                Int(row, 0, out var objectId) && objectId == competition.Id);
            var shortCode = Value(competitionRow, 2).Trim();
            var databaseId = shortCode.Length > 1 && shortCode[0] == 'C' &&
                int.TryParse(shortCode.Substring(1), out var parsedDatabaseId) ? parsedDatabaseId : -1;
            var stages = objects.Rows.Cast<DataRow>().Where(row => Int(row, 0, out _) && Int(row, 1, out var type) && type == 4 && Int(row, 4, out var parent) && parent == competition.Id)
                .Select(row => Convert.ToInt32(Value(row, 0))).ToArray();
            var groups = objects.Rows.Cast<DataRow>().Count(row => Int(row, 1, out var type) && type == 5 && Int(row, 4, out var parent) && stages.Contains(parent));
            var mapped = compIds != null && compIds.Rows.Cast<DataRow>().Any(row => Value(row, 0) == competition.Id.ToString());
            var teams = initTeams?.Rows.Cast<DataRow>().Count(row => Value(row, 0) == competition.Id.ToString()) ?? 0;
            var calendarRows = schedule?.Rows.Cast<DataRow>().Count(row => Int(row, 0, out var id) && stages.Contains(id)) ?? 0;
            var settingRows = settings?.Rows.Cast<DataRow>().Count(row => Value(row, 0) == competition.Id.ToString() || (Int(row, 0, out var id) && stages.Contains(id))) ?? 0;
            var assetMapped = databaseId > 0 && settings != null && settings.Rows.Cast<DataRow>().Any(row =>
                Value(row, 0) == competition.Id.ToString() &&
                string.Equals(Value(row, 1).Trim(), "asset_id", StringComparison.OrdinalIgnoreCase) &&
                Value(row, 2).Trim() == databaseId.ToString());
            var hasCompetitionType = settings != null && settings.Rows.Cast<DataRow>().Any(row =>
                Value(row, 0) == competition.Id.ToString() &&
                string.Equals(Value(row, 1).Trim(), "comp_type", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(Value(row, 2)));
            var ready = mapped && assetMapped && hasCompetitionType && stages.Length > 0 && groups > 0 && teams >= 2 && calendarRows > 0;
            if (ready) readyCount++;
            lines.Add((ready ? "[READY] " : "[NEEDS SETUP] ") + competition.Id + " · " + competition.Name);
            lines.Add("  Database link: " + (mapped ? "OK" : "MISSING"));
            lines.Add("  Database competition ID: " + (databaseId > 0 ? databaseId.ToString() : "INVALID"));
            lines.Add("  Asset ID / competition type: " +
                (assetMapped ? "OK" : "MISSING OR MISMATCHED") + " / " +
                (hasCompetitionType ? "OK" : "MISSING"));
            lines.Add("  Stages / groups: " + stages.Length + " / " + groups);
            lines.Add("  Assigned teams: " + teams + (teams >= 2 ? " (OK)" : " (minimum 2)"));
            lines.Add("  Calendar rows: " + calendarRows + (calendarRows > 0 ? " (OK)" : " (MISSING)"));
            lines.Add("  Friendly settings rows: " + settingRows);
            lines.Add(string.Empty);
        }
        lines.Insert(2, readyCount + " of " + CompetitionChoices().Length + " tournament(s) pass the structural Career Ready gate.");
        return string.Join(Environment.NewLine, lines);
    }

    private GroupChoice[] GroupChoices()
    {
        if (!_tables.TryGetValue("compobj", out var objects)) return Array.Empty<GroupChoice>();
        return objects.Rows.Cast<DataRow>().Where(row => Int(row, 0, out _) && Int(row, 1, out var type) && type == 5)
            .Select(row => new GroupChoice(Convert.ToInt32(Value(row, 0)), Value(row, 3), Value(row, 2)))
            .OrderBy(choice => choice.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private void ValidateTables()
    {
        Run("Validating competition structure, references and calendar fields...", () =>
        {
            EnsureLoaded(); RefreshSimpleViews(); var snapshot = WriteSnapshot();
            try
            {
                var report = Fc26HostBridge.ValidateCompdata(snapshot);
                using var viewer = new Form { Text = "Tournament Validation", Size = new Size(850, 580), StartPosition = FormStartPosition.CenterParent, Icon = Form.ActiveForm?.Icon };
                viewer.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Text = report });
                viewer.ShowDialog(this);
                return report.StartsWith("Compdata validation passed", StringComparison.OrdinalIgnoreCase) ? "Compdata validation passed." : "Compdata validation completed with findings.";
            }
            finally { try { File.Delete(snapshot); } catch { } }
        });
    }

    private void SaveWorkbook()
    {
        if (_tables.Count == 0) { MessageBox.Show(this, "Open Compdata first.", Text); return; }
        if (Directory.Exists(_sourcePath)) { MessageBox.Show(this, "A workbook copy requires an XLSX source. Use Export game TXT for a folder source.", Text); return; }
        using var dialog = new SaveFileDialog { Filter = "Compdata workbook|*.xlsx", FileName = Path.GetFileNameWithoutExtension(_sourcePath) + "_CM26.xlsx" };
        if (dialog.ShowDialog(this) == DialogResult.OK) Save(dialog.FileName, false);
    }

    private void ExportTxt()
    {
        if (_tables.Count == 0) { MessageBox.Show(this, "Open Compdata first.", Text); return; }
        using var dialog = new FolderBrowserDialog { Description = "Select an output folder for validated tournament files" };
        if (dialog.ShowDialog(this) == DialogResult.OK) Save(dialog.SelectedPath, true);
    }

    private void Save(string destination, bool textFiles)
    {
        Run("Validating and writing Compdata...", () =>
        {
            var snapshot = WriteSnapshot();
            try { Fc26HostBridge.SaveCompdata(_sourcePath, snapshot, destination, textFiles); }
            finally { try { File.Delete(snapshot); } catch { } }
            return "Validated Compdata written to " + destination;
        });
    }

    private string WriteSnapshot()
    {
        _grid.EndEdit();
        var snapshot = new CompdataSnapshot
        {
            SourcePath = _sourcePath,
            Sheets = _tables.Values.Select(table => new CompdataSheet
            {
                Name = table.TableName, Columns = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList(),
                Rows = table.Rows.Cast<DataRow>().Where(row => row.RowState != DataRowState.Deleted)
                    .Select(row => table.Columns.Cast<DataColumn>().Select(column => Convert.ToString(row[column]) ?? string.Empty).ToList()).ToList()
            }).ToList()
        };
        var path = Path.Combine(Path.GetTempPath(), "cm26-compdata-edits-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot)); return path;
    }

    private Dictionary<int, string> ObjectNames() => !_tables.TryGetValue("compobj", out var table) ? new Dictionary<int, string>()
        : table.Rows.Cast<DataRow>().Where(row => Int(row, 0, out _)).ToDictionary(row => Convert.ToInt32(Value(row, 0)), row => string.IsNullOrWhiteSpace(Value(row, 3)) ? Value(row, 2) : Value(row, 3));
    private static string CalendarStatus(DataRow row)
    {
        if (!int.TryParse(Value(row, 1), out var day) || day < 0) return "Invalid day";
        if (!int.TryParse(Value(row, 3), out var min) || !int.TryParse(Value(row, 4), out var max) || min < 0 || max < min) return "Invalid game range";
        if (!int.TryParse(Value(row, 5), out var time) || time < 0 || time > 2359 || time % 100 > 59) return "Invalid kick-off";
        return "OK";
    }
    private static string FormatKickoff(string value) => int.TryParse(value, out var time) ? (time / 100).ToString("00") + ":" + (time % 100).ToString("00") : value;
    private static string FriendlySectionName(string value) => value.ToLowerInvariant() switch
    {
        "compobj" => "Competition Objects",
        "schedule" => "Tournament Calendar",
        "advancement" => "Advancement Paths",
        "advancements" => "Advancement Paths",
        "settings" => "Competition Settings",
        "teams" => "Competition Teams",
        "standings" => "Standings Rules",
        "rules" => "Competition Rules",
        _ => "Competition Data"
    };
    private static string TypeName(int value) => value switch { 0 => "World", 1 => "Confederation", 2 => "Country", 3 => "Competition", 4 => "Stage", 5 => "Group", 6 => "Special Group", _ => "Unknown" };
    private static string Value(DataRow row, int index) => index < row.Table.Columns.Count ? Convert.ToString(row[index]) ?? string.Empty : string.Empty;
    private static bool Int(DataRow row, int index, out int value) => int.TryParse(Value(row, index), out value);
    private void EnsureLoaded() { if (_tables.Count == 0) throw new InvalidOperationException("Open Compdata first."); }
    private void Run(string busy, Func<string> action)
    {
        try { UseWaitCursor = true; _status.Text = busy; Application.DoEvents(); _status.Text = action(); }
        catch (Exception ex) { _status.Text = "Operation failed."; MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; }
    }
    private static ToolStripItem Item(string text, EventHandler click) { var item = new ToolStripButton(text); item.Click += click; return item; }
    private static DataGridView Grid(bool editable) => new DataGridView { Dock = DockStyle.Fill, ReadOnly = !editable, AllowUserToAddRows = editable, AllowUserToDeleteRows = editable, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells };
    private static NumericUpDown Number(decimal min, decimal max, decimal value) => new NumericUpDown { Minimum = min, Maximum = max, Value = value, Width = 100 };

    private sealed class TournamentWizardDialog : Form
    {
        private readonly TextBox _name = new TextBox { Width = 240 };
        private readonly NumericUpDown _databaseId = Number(0, 999999, 0), _stages = Number(1, 32, 1), _groups = Number(1, 64, 1);
        internal TournamentWizardDialog()
        {
            Text = "Tournament Wizard"; FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(430, 210); MaximizeBox = false; MinimizeBox = false;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
            layout.Controls.Add(new Label { Text = "Competition name", AutoSize = true }, 0, 0); layout.Controls.Add(_name, 1, 0);
            layout.Controls.Add(new Label { Text = "Database competition ID", AutoSize = true }, 0, 1); layout.Controls.Add(_databaseId, 1, 1);
            layout.Controls.Add(new Label { Text = "Stages", AutoSize = true }, 0, 2); layout.Controls.Add(_stages, 1, 2);
            layout.Controls.Add(new Label { Text = "Groups per stage", AutoSize = true }, 0, 3); layout.Controls.Add(_groups, 1, 3);
            var ok = new Button { Text = "Create", DialogResult = DialogResult.OK, AutoSize = true }; var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            var buttons = new FlowLayoutPanel { AutoSize = true }; buttons.Controls.AddRange(new Control[] { ok, cancel }); layout.Controls.Add(buttons, 1, 4);
            Controls.Add(layout); AcceptButton = ok; CancelButton = cancel;
        }
        internal string CompetitionName => _name.Text.Trim(); internal int DatabaseId => Decimal.ToInt32(_databaseId.Value); internal int Stages => Decimal.ToInt32(_stages.Value); internal int Groups => Decimal.ToInt32(_groups.Value);
    }

    private sealed class AdvancementDialog : Form
    {
        private readonly ComboBox _source = new ComboBox { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList }, _destination = new ComboBox { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _sourceRank = Number(0, 128, 0), _destinationRank = Number(0, 128, 0);
        internal AdvancementDialog(GroupChoice[] groups)
        {
            Text = "Group Advancement"; FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(500, 220); MaximizeBox = false; MinimizeBox = false;
            _source.Items.AddRange(groups); _destination.Items.AddRange(groups); _source.SelectedIndex = 0; _destination.SelectedIndex = 1;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
            layout.Controls.Add(new Label { Text = "Source group", AutoSize = true }, 0, 0); layout.Controls.Add(_source, 1, 0);
            layout.Controls.Add(new Label { Text = "Source rank", AutoSize = true }, 0, 1); layout.Controls.Add(_sourceRank, 1, 1);
            layout.Controls.Add(new Label { Text = "Destination group", AutoSize = true }, 0, 2); layout.Controls.Add(_destination, 1, 2);
            layout.Controls.Add(new Label { Text = "Destination rank", AutoSize = true }, 0, 3); layout.Controls.Add(_destinationRank, 1, 3);
            var ok = new Button { Text = "Add", DialogResult = DialogResult.OK, AutoSize = true }; var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            var buttons = new FlowLayoutPanel { AutoSize = true }; buttons.Controls.AddRange(new Control[] { ok, cancel }); layout.Controls.Add(buttons, 1, 4);
            Controls.Add(layout); AcceptButton = ok; CancelButton = cancel;
        }
        internal GroupChoice Source => (GroupChoice)_source.SelectedItem; internal GroupChoice Destination => (GroupChoice)_destination.SelectedItem;
        internal int SourceRank => Decimal.ToInt32(_sourceRank.Value); internal int DestinationRank => Decimal.ToInt32(_destinationRank.Value);
    }
    private sealed class GroupChoice
    {
        internal GroupChoice(int id, string description, string shortName) { Id = id; Name = string.IsNullOrWhiteSpace(description) ? shortName : description; }
        internal int Id { get; } internal string Name { get; } public override string ToString() => Id + " · " + Name;
    }
    private sealed class CompetitionChoice
    {
        internal CompetitionChoice(int id, string name) { Id = id; Name = string.IsNullOrWhiteSpace(name) ? "Competition " + id : name; }
        internal int Id { get; } internal string Name { get; } public override string ToString() => Id + " · " + Name;
    }
    private sealed class TeamChoice
    {
        internal TeamChoice(int id, string name) { Id = id; Name = string.IsNullOrWhiteSpace(name) ? "Team " + id : name; }
        internal int Id { get; } internal string Name { get; } public override string ToString() => Id + " · " + Name;
    }
    private sealed class TeamAssignmentDialog : Form
    {
        private readonly ComboBox _competition = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckedListBox _teams = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true };
        private readonly Func<int, HashSet<int>> _existing;
        internal TeamAssignmentDialog(CompetitionChoice[] competitions, TeamChoice[] teams, Func<int, HashSet<int>> existing)
        {
            _existing = existing; Text = "Assign Tournament Teams"; StartPosition = FormStartPosition.CenterParent; Size = new Size(560, 620);
            _competition.Items.AddRange(competitions); _teams.Items.AddRange(teams);
            _competition.SelectedIndexChanged += (_, _) => RefreshChecks(); _competition.SelectedIndex = 0;
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft };
            buttons.Controls.Add(new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true });
            buttons.Controls.Add(new Button { Text = "Apply", DialogResult = DialogResult.OK, AutoSize = true });
            Controls.Add(_teams); Controls.Add(new Label { Text = "Select a competition, then tick its participating clubs.", Dock = DockStyle.Top, Height = 28 });
            Controls.Add(_competition); Controls.Add(buttons); AcceptButton = buttons.Controls[1] as Button; CancelButton = buttons.Controls[0] as Button;
        }
        private void RefreshChecks()
        {
            if (_competition.SelectedItem is not CompetitionChoice competition) return;
            var selected = _existing(competition.Id);
            for (var index = 0; index < _teams.Items.Count; index++)
                _teams.SetItemChecked(index, _teams.Items[index] is TeamChoice team && selected.Contains(team.Id));
        }
        internal CompetitionChoice Competition => (CompetitionChoice)_competition.SelectedItem;
        internal int[] TeamIds => _teams.CheckedItems.Cast<TeamChoice>().Select(team => team.Id).ToArray();
    }
    private sealed class ScheduleGeneratorDialog : Form
    {
        private readonly ComboBox _competition = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _legs = Number(1, 2, 2), _startDay = Number(0, 2000, 1),
            _interval = Number(1, 60, 7), _kickoff = Number(0, 2359, 1500);
        internal ScheduleGeneratorDialog(CompetitionChoice[] competitions)
        {
            Text = "Generate Round-robin Schedule"; FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(520, 255);
            _competition.Items.AddRange(competitions); _competition.SelectedIndex = 0;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 6 };
            layout.Controls.Add(new Label { Text = "Competition", AutoSize = true }, 0, 0); layout.Controls.Add(_competition, 1, 0);
            layout.Controls.Add(new Label { Text = "Legs", AutoSize = true }, 0, 1); layout.Controls.Add(_legs, 1, 1);
            layout.Controls.Add(new Label { Text = "Start day", AutoSize = true }, 0, 2); layout.Controls.Add(_startDay, 1, 2);
            layout.Controls.Add(new Label { Text = "Days between rounds", AutoSize = true }, 0, 3); layout.Controls.Add(_interval, 1, 3);
            layout.Controls.Add(new Label { Text = "Kick-off (HHMM)", AutoSize = true }, 0, 4); layout.Controls.Add(_kickoff, 1, 4);
            var ok = new Button { Text = "Generate", DialogResult = DialogResult.OK, AutoSize = true }; var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            var buttons = new FlowLayoutPanel { AutoSize = true }; buttons.Controls.Add(ok); buttons.Controls.Add(cancel); layout.Controls.Add(buttons, 1, 5);
            Controls.Add(layout); AcceptButton = ok; CancelButton = cancel;
        }
        internal CompetitionChoice Competition => (CompetitionChoice)_competition.SelectedItem;
        internal int Legs => Decimal.ToInt32(_legs.Value); internal int StartDay => Decimal.ToInt32(_startDay.Value);
        internal int DayInterval => Decimal.ToInt32(_interval.Value); internal int Kickoff => Decimal.ToInt32(_kickoff.Value);
    }
    private sealed class CompdataSheetChoice
    {
        internal CompdataSheetChoice(string name, string label) { Name = name; Label = label; }
        internal string Name { get; } internal string Label { get; }
        public override string ToString() => Label;
    }
    private sealed class CompdataSnapshot { public string SourcePath { get; set; } = string.Empty; public List<CompdataSheet> Sheets { get; set; } = new List<CompdataSheet>(); }
    private sealed class CompdataSheet { public string Name { get; set; } = string.Empty; public List<string> Columns { get; set; } = new List<string>(); public List<List<string>> Rows { get; set; } = new List<List<string>>(); }
}
