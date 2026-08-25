using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>Classic CM26 Compdata workspace with raw, structure and calendar views.</summary>
internal sealed class Fc26CompdataForm : Form
{
    private readonly ListBox _sheets = new ListBox();
    private readonly DataGridView _grid = Grid(true);
    private readonly TreeView _structure = new TreeView();
    private readonly DataGridView _calendar = Grid(false);
    private readonly Label _status = new Label();
    private readonly TabControl _views = new TabControl();
    private readonly Dictionary<string, DataTable> _tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
    private string _sourcePath = string.Empty;

    internal Fc26CompdataForm()
    {
        Text = "FC26 Competition / Compdata Editor";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1240, 760);
        MinimumSize = new Size(900, 560);
        Icon = Form.ActiveForm?.Icon;
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tools.Items.Add(Item("Open workbook", (_, _) => OpenWorkbook()));
        tools.Items.Add(Item("Open TXT folder", (_, _) => OpenFolder()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Item("Tournament Wizard", (_, _) => TournamentWizard()));
        tools.Items.Add(Item("Add Advancement", (_, _) => AddAdvancement()));
        tools.Items.Add(Item("Add row", (_, _) => AddRow()));
        tools.Items.Add(Item("Delete row", (_, _) => DeleteRow()));
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
        _structure.AfterSelect += (_, e) => SelectObjectInRawView(e.Node?.Tag);
        _views.Dock = DockStyle.Fill;
        _views.TabPages.Add(new TabPage("Simple Structure") { Controls = { _structure } });
        _views.TabPages.Add(new TabPage("Tournament Calendar") { Controls = { _calendar } });
        _views.TabPages.Add(new TabPage("Advanced Raw Tables") { Controls = { _grid } });
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 210 };
        split.Panel1.Controls.Add(_sheets); split.Panel2.Controls.Add(_views);
        _status.Dock = DockStyle.Bottom; _status.Height = 24; _status.Padding = new Padding(6, 4, 0, 0);
        _status.Text = "Open an FC26 Compdata workbook or extracted Compdata TXT folder.";
        Controls.Add(split); Controls.Add(_status); Controls.Add(tools);
    }

    private void OpenWorkbook()
    {
        using var dialog = new OpenFileDialog { Filter = "Compdata workbook|*.xlsx|All files|*.*" };
        if (dialog.ShowDialog(this) == DialogResult.OK) LoadSource(dialog.FileName);
    }

    private void OpenFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select the extracted FC26 Compdata folder" };
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
        _sheets.DataSource = _tables.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
        if (_sheets.Items.Count > 0) _sheets.SelectedIndex = 0;
        RefreshSimpleViews();
    }

    private void ShowSheet()
    {
        var name = Convert.ToString(_sheets.SelectedItem);
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
                .GroupBy(row => Value(row, 0) + "|" + Value(row, 1) + "|" + Value(row, 2), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in schedule.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                var idText = Value(row, 0); int.TryParse(idText, out var id);
                var key = idText + "|" + Value(row, 1) + "|" + Value(row, 2);
                var state = duplicates.Contains(key) ? "Conflict: duplicate object/day/round" : CalendarStatus(row);
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

    private GroupChoice[] GroupChoices()
    {
        if (!_tables.TryGetValue("compobj", out var objects)) return Array.Empty<GroupChoice>();
        return objects.Rows.Cast<DataRow>().Where(row => Int(row, 0, out _) && Int(row, 1, out var type) && type == 5)
            .Select(row => new GroupChoice(Convert.ToInt32(Value(row, 0)), Value(row, 3), Value(row, 2)))
            .OrderBy(choice => choice.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private void SelectObjectInRawView(object tag)
    {
        if (!(tag is int id) || !_tables.ContainsKey("compobj")) return;
        _sheets.SelectedItem = _tables.Keys.First(name => name.Equals("compobj", StringComparison.OrdinalIgnoreCase)); _views.SelectedIndex = 2;
        foreach (DataGridViewRow row in _grid.Rows)
            if (Convert.ToString(row.Cells[0].Value) == id.ToString()) { row.Selected = true; _grid.CurrentCell = row.Cells[0]; break; }
    }

    private void AddRow()
    {
        if (_grid.DataSource is not DataTable table) return;
        table.Rows.Add(table.NewRow()); RefreshSimpleViews(); _status.Text = "Row added to " + table.TableName + ".";
    }

    private void DeleteRow()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView row)
        {
            var table = row.Row.Table.TableName; row.Row.Delete(); RefreshSimpleViews(); _status.Text = "Row deleted from " + table + ".";
        }
    }

    private void ValidateTables()
    {
        Run("Validating competition structure, references and calendar fields...", () =>
        {
            EnsureLoaded(); RefreshSimpleViews(); var snapshot = WriteSnapshot();
            try
            {
                var report = Fc26HostBridge.ValidateCompdata(snapshot);
                using var viewer = new Form { Text = "FC26 Compdata Validation", Size = new Size(850, 580), StartPosition = FormStartPosition.CenterParent, Icon = Icon };
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
        using var dialog = new FolderBrowserDialog { Description = "Select an output folder for validated FC26 Compdata TXT files" };
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
    private sealed class CompdataSnapshot { public string SourcePath { get; set; } = string.Empty; public List<CompdataSheet> Sheets { get; set; } = new List<CompdataSheet>(); }
    private sealed class CompdataSheet { public string Name { get; set; } = string.Empty; public List<string> Columns { get; set; } = new List<string>(); public List<List<string>> Rows { get; set; } = new List<List<string>>(); }
}
