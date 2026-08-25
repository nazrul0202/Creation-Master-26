using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>
/// FC26 database workspace hosted inside the original CM16-style shell. Values
/// are staged in the same snapshot/change-plan used by File > Save, so this is
/// not a second writer and never touches the game before validation/backup.
/// </summary>
internal sealed class Fc26DatabaseWorkspaceForm : Form
{
    private readonly ListBox _tables = new ListBox();
    private readonly DataGridView _grid = new DataGridView();
    private readonly TextBox _tableSearch = new TextBox();
    private readonly TextBox _rowSearch = new TextBox();
    private readonly Label _status = new Label();
    private SnapshotDetailTable _active;
    private bool _loading;

    internal Fc26DatabaseWorkspaceForm()
    {
        Text = "FC26 Advanced Database Workspace";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1280, 780);
        MinimumSize = new Size(900, 560);
        Icon = Form.ActiveForm?.Icon;

        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tools.Items.Add(Button("Refresh", (_, _) => LoadTable()));
        tools.Items.Add(Button("Copy", (_, _) => CopySelection()));
        tools.Items.Add(Button("Paste", (_, _) => PasteSelection()));
        tools.Items.Add(Button("Replace", (_, _) => BulkReplace()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Button("Find References", (_, _) => ShowReferences(replace: false)));
        tools.Items.Add(Button("Replace References", (_, _) => ShowReferences(replace: true)));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Button("Import TSV", (_, _) => ImportTsv()));
        tools.Items.Add(Button("Export TSV", (_, _) => ExportTsv()));

        _tableSearch.Dock = DockStyle.Top;
        _tableSearch.AccessibleName = "Find table";
        _tableSearch.TextChanged += (_, _) => PopulateTables();
        _rowSearch.Dock = DockStyle.Top;
        _rowSearch.AccessibleName = "Search all visible fields (accent-insensitive)";
        _rowSearch.TextChanged += (_, _) => LoadTable();

        _tables.Dock = DockStyle.Fill;
        _tables.IntegralHeight = false;
        _tables.SelectedIndexChanged += (_, _) => LoadTable();

        var tablePanel = new Panel { Dock = DockStyle.Left, Width = 245, Padding = new Padding(6) };
        tablePanel.Controls.Add(_tables);
        tablePanel.Controls.Add(_tableSearch);

        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.MultiSelect = true;
        _grid.CellEndEdit += GridCellEndEdit;
        _grid.SelectionChanged += (_, _) => UpdateStatus();

        var dataPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        dataPanel.Controls.Add(_grid);
        dataPanel.Controls.Add(_rowSearch);

        _status.Dock = DockStyle.Bottom;
        _status.Height = 24;
        _status.BorderStyle = BorderStyle.Fixed3D;
        _status.TextAlign = ContentAlignment.MiddleLeft;

        Controls.Add(dataPanel);
        Controls.Add(tablePanel);
        Controls.Add(_status);
        Controls.Add(tools);
        PopulateTables();
    }

    private static ToolStripButton Button(string text, EventHandler click)
    {
        var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        button.Click += click;
        return button;
    }

    private void PopulateTables()
    {
        var selected = _tables.SelectedItem as string;
        var query = NormalizeText(_tableSearch.Text);
        _tables.BeginUpdate();
        _tables.Items.Clear();
        foreach (var name in Fc26SnapshotLoader.DetailTableNames)
            if (query.Length == 0 || NormalizeText(name).Contains(query)) _tables.Items.Add(name);
        _tables.EndUpdate();
        if (selected != null && _tables.Items.Contains(selected)) _tables.SelectedItem = selected;
        else if (_tables.Items.Count > 0) _tables.SelectedIndex = 0;
    }

    private void LoadTable()
    {
        if (_tables.SelectedItem == null) return;
        _active = Fc26SnapshotLoader.DetailTable(_tables.SelectedItem.ToString());
        if (_active == null) return;
        var query = NormalizeText(_rowSearch.Text);
        var data = new DataTable(_active.Name);
        data.Columns.Add("__CM26_ROW", typeof(int));
        foreach (var column in _active.Columns) data.Columns.Add(UniqueColumnName(data, column), typeof(string));
        for (var rowIndex = 0; rowIndex < _active.Rows.Count; rowIndex++)
        {
            var source = _active.Rows[rowIndex];
            if (query.Length > 0 && !source.Any(value => NormalizeText(value).Contains(query))) continue;
            var row = data.NewRow();
            row[0] = rowIndex;
            for (var column = 0; column < _active.Columns.Length; column++)
                row[column + 1] = column < source.Length ? source[column] ?? string.Empty : string.Empty;
            data.Rows.Add(row);
        }
        _loading = true;
        _grid.DataSource = data;
        _grid.Columns[0].Visible = false;
        for (var column = 1; column < _grid.Columns.Count; column++)
            _grid.Columns[column].HeaderText = _active.Columns[column - 1];
        _loading = false;
        UpdateStatus();
    }

    private static string UniqueColumnName(DataTable table, string requested)
    {
        var name = string.IsNullOrWhiteSpace(requested) ? "field" : requested;
        var suffix = 2;
        var candidate = name;
        while (table.Columns.Contains(candidate)) candidate = name + "_" + suffix++;
        return candidate;
    }

    private void GridCellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        if (_loading || _active == null || e.RowIndex < 0 || e.ColumnIndex <= 0) return;
        var rowIndex = Convert.ToInt32(_grid.Rows[e.RowIndex].Cells[0].Value, CultureInfo.InvariantCulture);
        var field = _active.Columns[e.ColumnIndex - 1];
        var value = Convert.ToString(_grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, CultureInfo.InvariantCulture) ?? string.Empty;
        try { Fc26SnapshotLoader.StageDetailValue(_active.Name, rowIndex, field, value); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Stage database value", MessageBoxButtons.OK, MessageBoxIcon.Error); LoadTable(); }
        UpdateStatus();
    }

    private void CopySelection()
    {
        var data = _grid.GetClipboardContent();
        if (data != null) Clipboard.SetDataObject(data);
    }

    private void PasteSelection()
    {
        if (_grid.CurrentCell == null || _grid.CurrentCell.ColumnIndex <= 0 || !Clipboard.ContainsText()) return;
        var rows = Clipboard.GetText().Replace("\r", string.Empty).Split('\n');
        var startRow = _grid.CurrentCell.RowIndex;
        var startColumn = _grid.CurrentCell.ColumnIndex;
        for (var rowOffset = 0; rowOffset < rows.Length && startRow + rowOffset < _grid.Rows.Count; rowOffset++)
        {
            if (rows[rowOffset].Length == 0 && rowOffset == rows.Length - 1) break;
            var values = rows[rowOffset].Split('\t');
            for (var columnOffset = 0; columnOffset < values.Length && startColumn + columnOffset < _grid.Columns.Count; columnOffset++)
            {
                var gridRow = _grid.Rows[startRow + rowOffset];
                var gridColumn = startColumn + columnOffset;
                if (gridColumn <= 0) continue;
                var sourceRow = Convert.ToInt32(gridRow.Cells[0].Value, CultureInfo.InvariantCulture);
                var field = _active.Columns[gridColumn - 1];
                Fc26SnapshotLoader.StageDetailValue(_active.Name, sourceRow, field, values[columnOffset]);
                gridRow.Cells[gridColumn].Value = values[columnOffset];
            }
        }
        UpdateStatus();
    }

    private void BulkReplace()
    {
        if (_grid.SelectedCells.Count == 0) return;
        string find;
        string replacement;
        if (!Prompt("Replace selected cells", "Exact value to find:", string.Empty, out find) ||
            !Prompt("Replace selected cells", "Replacement value:", string.Empty, out replacement)) return;
        var changed = 0;
        foreach (DataGridViewCell cell in _grid.SelectedCells)
        {
            if (cell.ColumnIndex <= 0 || !string.Equals(Convert.ToString(cell.Value), find, StringComparison.Ordinal)) continue;
            var rowIndex = Convert.ToInt32(_grid.Rows[cell.RowIndex].Cells[0].Value, CultureInfo.InvariantCulture);
            var field = _active.Columns[cell.ColumnIndex - 1];
            Fc26SnapshotLoader.StageDetailValue(_active.Name, rowIndex, field, replacement);
            cell.Value = replacement;
            changed++;
        }
        _status.Text = changed + " value(s) staged. File > Save validates, backs up and writes them.";
    }

    private void ShowReferences(bool replace)
    {
        if (_grid.CurrentCell == null || _grid.CurrentCell.ColumnIndex <= 0) return;
        var field = _active.Columns[_grid.CurrentCell.ColumnIndex - 1];
        var value = Convert.ToString(_grid.CurrentCell.Value, CultureInfo.InvariantCulture) ?? string.Empty;
        var hits = FindReferences(field, value);
        var preview = hits.Count == 0 ? "No matching references were found." :
            string.Join(Environment.NewLine, hits.Take(250).Select(hit => hit.Table + "[" + hit.Row + "]." + hit.Field));
        if (!replace)
        {
            MessageBox.Show(this, field + "=" + value + "\r\n\r\nAffected records: " + hits.Count + "\r\n\r\n" + preview,
                "Dependency impact", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        string replacement;
        if (!Prompt("Replace linked references", "New " + field + " value:", value, out replacement) || replacement == value) return;
        if (_active.Rows.Any(row => _active.Column(field) >= 0 && row.Length > _active.Column(field) && row[_active.Column(field)] == replacement))
        {
            MessageBox.Show(this, "That ID already exists in the selected parent table. Use an unused ID or the dedicated swap workflow.",
                "Replace linked references", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show(this, "Stage " + hits.Count + " linked update(s)?\r\n\r\n" + preview,
            "Dependency-aware ID change", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var hit in hits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, replacement);
        LoadTable();
        _status.Text = hits.Count + " dependency-aware update(s) staged as one save operation.";
    }

    private static List<ReferenceHit> FindReferences(string field, string value)
    {
        var hits = new List<ReferenceHit>();
        foreach (var tableName in Fc26SnapshotLoader.DetailTableNames)
        {
            var table = Fc26SnapshotLoader.DetailTable(tableName);
            if (table == null) continue;
            for (var column = 0; column < table.Columns.Length; column++)
            {
                if (!table.Columns[column].Equals(field, StringComparison.OrdinalIgnoreCase)) continue;
                for (var row = 0; row < table.Rows.Count; row++)
                    if (column < table.Rows[row].Length && string.Equals(table.Rows[row][column], value, StringComparison.Ordinal))
                        hits.Add(new ReferenceHit(table.Name, row, table.Columns[column]));
            }
        }
        return hits;
    }

    private void ExportTsv()
    {
        if (_active == null) return;
        using (var dialog = new SaveFileDialog { Filter = "TSV files (*.tsv)|*.tsv", FileName = _active.Name + ".tsv" })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            var lines = new List<string> { string.Join("\t", _active.Columns) };
            lines.AddRange(_active.Rows.Select(row => string.Join("\t", row.Select(SafeTsv))));
            File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(false));
        }
    }

    private void ImportTsv()
    {
        if (_active == null) return;
        using (var dialog = new OpenFileDialog { Filter = "TSV files (*.tsv)|*.tsv|All files (*.*)|*.*" })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            var lines = File.ReadAllLines(dialog.FileName);
            if (lines.Length == 0 || !lines[0].Split('\t').SequenceEqual(_active.Columns))
                throw new InvalidDataException("TSV header does not match the selected FC26 table.");
            if (lines.Length - 1 != _active.Rows.Count)
                throw new InvalidDataException("TSV row count must match the selected table; structural imports are blocked by this safe workspace.");
            var staged = 0;
            for (var row = 0; row < _active.Rows.Count; row++)
            {
                var values = lines[row + 1].Split('\t');
                if (values.Length != _active.Columns.Length) throw new InvalidDataException("TSV column count mismatch at row " + (row + 2) + ".");
                for (var column = 0; column < values.Length; column++)
                {
                    if (values[column] == _active.Rows[row][column]) continue;
                    Fc26SnapshotLoader.StageDetailValue(_active.Name, row, _active.Columns[column], values[column]);
                    staged++;
                }
            }
            LoadTable();
            _status.Text = staged + " imported value(s) staged; source files are still untouched.";
        }
    }

    private void UpdateStatus()
    {
        var selected = _grid.CurrentCell;
        var detail = string.Empty;
        if (_active != null && selected != null && selected.ColumnIndex > 0)
        {
            var field = _active.Columns[selected.ColumnIndex - 1];
            var value = Convert.ToString(selected.Value, CultureInfo.InvariantCulture) ?? string.Empty;
            detail = " | " + FriendlyValue(field, value);
        }
        _status.Text = (_active == null ? "No table" : _active.Name + ": " + _grid.Rows.Count + " visible / " + _active.Rows.Count + " rows") +
            " | " + Fc26SnapshotLoader.PendingDetailCount + " staged detail change(s)" + detail;
    }

    private static string FriendlyValue(string field, string value)
    {
        int id;
        if (!int.TryParse(value, out id)) return field + " = " + value;
        object item = null;
        if (field.Equals("playerid", StringComparison.OrdinalIgnoreCase)) item = FifaLibrary.FifaEnvironment.Players?.SearchId(id);
        else if (field.Equals("teamid", StringComparison.OrdinalIgnoreCase)) item = FifaLibrary.FifaEnvironment.Teams?.SearchId(id);
        else if (field.Equals("nationid", StringComparison.OrdinalIgnoreCase)) item = FifaLibrary.FifaEnvironment.Countries?.SearchId(id);
        else if (field.Equals("leagueid", StringComparison.OrdinalIgnoreCase)) item = FifaLibrary.FifaEnvironment.Leagues?.SearchId(id);
        return item == null ? field + " = " + value : field + " = " + value + " (" + item + ")";
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(character));
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string SafeTsv(string value) => (value ?? string.Empty).Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");

    private static bool Prompt(string title, string label, string initial, out string value)
    {
        using (var dialog = new Form { Text = title, FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(430, 115), MinimizeBox = false, MaximizeBox = false })
        {
            var caption = new Label { Text = label, Left = 12, Top = 12, Width = 400 };
            var input = new TextBox { Text = initial, Left = 12, Top = 36, Width = 400 };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 250, Top = 76, Width = 75 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 337, Top = 76, Width = 75 };
            dialog.Controls.AddRange(new Control[] { caption, input, ok, cancel });
            dialog.AcceptButton = ok;
            dialog.CancelButton = cancel;
            var accepted = dialog.ShowDialog() == DialogResult.OK;
            value = input.Text;
            return accepted;
        }
    }

    private sealed class ReferenceHit
    {
        internal ReferenceHit(string table, int row, string field) { Table = table; Row = row; Field = field; }
        internal string Table { get; }
        internal int Row { get; }
        internal string Field { get; }
    }
}
