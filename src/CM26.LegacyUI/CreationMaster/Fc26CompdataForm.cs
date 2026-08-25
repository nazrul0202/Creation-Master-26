using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>Editable Compdata workbook/TXT workspace inside the classic shell.</summary>
internal sealed class Fc26CompdataForm : Form
{
    private readonly ListBox _sheets = new ListBox();
    private readonly DataGridView _grid = new DataGridView();
    private readonly Label _status = new Label();
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
        tools.Items.Add(Item("Add row", (_, _) => AddRow()));
        tools.Items.Add(Item("Delete row", (_, _) => DeleteRow()));
        tools.Items.Add(Item("Validate", (_, _) => ValidateTables()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Item("Save workbook copy", (_, _) => SaveWorkbook()));
        tools.Items.Add(Item("Export game TXT", (_, _) => ExportTxt()));

        _sheets.Dock = DockStyle.Fill;
        _sheets.SelectedIndexChanged += (_, _) => ShowSheet();
        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = true;
        _grid.AllowUserToDeleteRows = true;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        _grid.DataError += (_, e) => { e.ThrowException = false; };
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 210 };
        split.Panel1.Controls.Add(_sheets);
        split.Panel2.Controls.Add(_grid);
        _status.Dock = DockStyle.Bottom;
        _status.Height = 24;
        _status.Padding = new Padding(6, 4, 0, 0);
        _status.Text = "Open an FC26 Compdata workbook or extracted Compdata TXT folder.";
        Controls.Add(split);
        Controls.Add(_status);
        Controls.Add(tools);
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
            try
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
                _sourcePath = source;
                _sheets.DataSource = _tables.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
                if (_sheets.Items.Count > 0) _sheets.SelectedIndex = 0;
                return _tables.Count + " Compdata sheet(s) loaded.";
            }
            finally { try { File.Delete(path); } catch { } }
        });
    }

    private void ShowSheet()
    {
        var name = Convert.ToString(_sheets.SelectedItem);
        _grid.DataSource = !string.IsNullOrWhiteSpace(name) && _tables.TryGetValue(name, out var table) ? table : null;
        if (_grid.Columns.Count > 0) _grid.Columns[_grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
    }

    private void AddRow()
    {
        if (_grid.DataSource is not DataTable table) return;
        table.Rows.Add(table.NewRow());
        _status.Text = "Row added to " + table.TableName + ".";
    }

    private void DeleteRow()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView row)
        {
            var table = row.Row.Table.TableName;
            row.Row.Delete();
            _status.Text = "Row deleted from " + table + ".";
        }
    }

    private void ValidateTables()
    {
        Run("Validating competition structure, references and calendar fields...", () =>
        {
            EnsureLoaded();
            var snapshot = WriteSnapshot();
            try
            {
                var report = Fc26HostBridge.ValidateCompdata(snapshot);
                using var viewer = new Form { Text = "FC26 Compdata Validation", Size = new Size(850, 580), StartPosition = FormStartPosition.CenterParent, Icon = Icon };
                viewer.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Text = report });
                viewer.ShowDialog(this);
                return report.StartsWith("Compdata validation passed", StringComparison.OrdinalIgnoreCase)
                    ? "Compdata validation passed." : "Compdata validation completed with findings.";
            }
            finally { try { File.Delete(snapshot); } catch { } }
        });
    }

    private void SaveWorkbook()
    {
        if (_tables.Count == 0) { MessageBox.Show(this, "Open Compdata first.", Text); return; }
        if (Directory.Exists(_sourcePath))
        {
            MessageBox.Show(this, "A workbook copy requires an XLSX source. Use Export game TXT for a folder source.", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog { Filter = "Compdata workbook|*.xlsx", FileName = Path.GetFileNameWithoutExtension(_sourcePath) + "_CM26.xlsx" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Save(dialog.FileName, textFiles: false);
    }

    private void ExportTxt()
    {
        if (_tables.Count == 0) { MessageBox.Show(this, "Open Compdata first.", Text); return; }
        using var dialog = new FolderBrowserDialog { Description = "Select an empty/output folder for validated FC26 Compdata TXT files" };
        if (dialog.ShowDialog(this) == DialogResult.OK) Save(dialog.SelectedPath, textFiles: true);
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
                Name = table.TableName,
                Columns = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList(),
                Rows = table.Rows.Cast<DataRow>().Where(row => row.RowState != DataRowState.Deleted)
                    .Select(row => table.Columns.Cast<DataColumn>().Select(column => Convert.ToString(row[column]) ?? string.Empty).ToList()).ToList()
            }).ToList()
        };
        var path = Path.Combine(Path.GetTempPath(), "cm26-compdata-edits-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot));
        return path;
    }

    private void EnsureLoaded()
    {
        if (_tables.Count == 0) throw new InvalidOperationException("Open Compdata first.");
    }

    private void Run(string busy, Func<string> action)
    {
        try { UseWaitCursor = true; _status.Text = busy; Application.DoEvents(); _status.Text = action(); }
        catch (Exception ex) { _status.Text = "Operation failed."; MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; }
    }

    private static ToolStripItem Item(string text, EventHandler click)
    {
        var item = new ToolStripButton(text); item.Click += click; return item;
    }

    private sealed class CompdataSnapshot
    {
        public string SourcePath { get; set; } = string.Empty;
        public List<CompdataSheet> Sheets { get; set; } = new List<CompdataSheet>();
    }

    private sealed class CompdataSheet
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }
}
