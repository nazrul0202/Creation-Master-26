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
    private readonly ComboBox _filterField = new ComboBox();
    private readonly TextBox _filterExpression = new TextBox();
    private readonly CheckBox _changedOnly = new CheckBox();
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
        tools.Items.Add(Button("Add / Clone Row", (_, _) => CloneRow()));
        tools.Items.Add(Button("Delete Row", (_, _) => DeleteRow()));
        tools.Items.Add(Button("Copy", (_, _) => CopySelection()));
        tools.Items.Add(Button("Paste", (_, _) => PasteSelection()));
        tools.Items.Add(Button("Set Selected", (_, _) => BulkSet()));
        tools.Items.Add(Button("Replace", (_, _) => BulkReplace()));
        tools.Items.Add(Button("Compare", (_, _) => CompareTsv()));
        tools.Items.Add(Button("Validate XML Ranges", (_, _) => ValidateDescriptorRanges()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Button("Find References", (_, _) => ShowReferences(replace: false)));
        tools.Items.Add(Button("Replace References", (_, _) => ShowReferences(replace: true)));
        tools.Items.Add(Button("Remove References", (_, _) => RemoveReferences()));
        tools.Items.Add(Button("Swap IDs", (_, _) => SwapIds()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Button("Import TSV", (_, _) => ImportTsv()));
        tools.Items.Add(Button("Export TSV", (_, _) => ExportTsv()));
        tools.Items.Add(Button("Import All", (_, _) => ImportAllTables()));
        tools.Items.Add(Button("Export All", (_, _) => ExportAllTables()));
        tools.Items.Add(new ToolStripSeparator());
        tools.Items.Add(Button("Save Filter", (_, _) => SaveFilterPreset()));
        tools.Items.Add(Button("Load Filter", (_, _) => LoadFilterPreset()));
        tools.Items.Add(Button("Save Row Template", (_, _) => SaveRowTemplate()));
        tools.Items.Add(Button("Apply Row Template", (_, _) => ApplyRowTemplate()));

        _tableSearch.Dock = DockStyle.Top;
        _tableSearch.AccessibleName = "Find table";
        _tableSearch.TextChanged += (_, _) => PopulateTables();
        _rowSearch.Dock = DockStyle.Top;
        _rowSearch.AccessibleName = "Search all visible fields (accent-insensitive)";
        _rowSearch.TextChanged += (_, _) => LoadTable();
        _filterField.DropDownStyle = ComboBoxStyle.DropDownList;
        _filterField.Width = 170;
        _filterField.SelectedIndexChanged += (_, _) => LoadTable();
        _filterExpression.Width = 210;
        _filterExpression.AccessibleName = "Field filter expression";
        _filterExpression.TextChanged += (_, _) => LoadTable();
        _changedOnly.Text = "Changed records only";
        _changedOnly.AutoSize = true;
        _changedOnly.Margin = new Padding(10, 5, 2, 0);
        _changedOnly.CheckedChanged += (_, _) => LoadTable();

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

        var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 29, WrapContents = false };
        filterPanel.Controls.Add(new Label { Text = "Field filter:", AutoSize = true, Margin = new Padding(2, 6, 2, 0) });
        filterPanel.Controls.Add(_filterField);
        filterPanel.Controls.Add(new Label { Text = "Expression (=, !=, >, <, >=, <= or contains):", AutoSize = true, Margin = new Padding(8, 6, 2, 0) });
        filterPanel.Controls.Add(_filterExpression);
        filterPanel.Controls.Add(_changedOnly);
        var dataPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        dataPanel.Controls.Add(_grid);
        dataPanel.Controls.Add(filterPanel);
        dataPanel.Controls.Add(_rowSearch);

        _status.Dock = DockStyle.Bottom;
        _status.Height = 24;
        _status.BorderStyle = BorderStyle.Fixed3D;
        _status.TextAlign = ContentAlignment.MiddleLeft;

        Controls.Add(dataPanel);
        Controls.Add(tablePanel);
        Controls.Add(_status);
        Controls.Add(tools);
        FormClosing += (_, e) =>
        {
            if (Fc26SnapshotLoader.PendingDetailCount == 0 || e.CloseReason == CloseReason.ApplicationExitCall) return;
            if (MessageBox.Show(this, "There are " + Fc26SnapshotLoader.PendingDetailCount +
                " staged database change(s). Closing this workspace keeps them pending for File > Save. Close workspace?",
                "Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) e.Cancel = true;
        };
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
        if (_loading) return;
        if (_tables.SelectedItem == null) return;
        _active = Fc26SnapshotLoader.DetailTable(_tables.SelectedItem.ToString());
        if (_active == null) return;
        var previousField = _filterField.SelectedItem as string;
        _loading = true;
        _filterField.BeginUpdate();
        _filterField.Items.Clear();
        _filterField.Items.Add("All fields");
        _filterField.Items.AddRange(_active.Columns.Cast<object>().ToArray());
        _filterField.SelectedItem = previousField != null && _filterField.Items.Contains(previousField) ? previousField : "All fields";
        _filterField.EndUpdate();
        _loading = false;
        var query = NormalizeText(_rowSearch.Text);
        var data = new DataTable(_active.Name);
        data.Columns.Add("__CM26_ROW", typeof(int));
        foreach (var column in _active.Columns) data.Columns.Add(UniqueColumnName(data, column), typeof(string));
        for (var rowIndex = 0; rowIndex < _active.Rows.Count; rowIndex++)
        {
            if (Fc26SnapshotLoader.IsDetailDeleted(_active.Name, rowIndex)) continue;
            if (_changedOnly.Checked && !Fc26SnapshotLoader.IsDetailRowChanged(_active.Name, rowIndex)) continue;
            var source = _active.Rows[rowIndex];
            if (query.Length > 0 && !source.Any(value => NormalizeText(value).Contains(query))) continue;
            if (!MatchesFieldFilter(source)) continue;
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
        {
            _grid.Columns[column].HeaderText = _active.Columns[column - 1];
            var detail = _active.ColumnDetails[column - 1];
            _grid.Columns[column].ReadOnly = !detail.IsWritable;
            _grid.Columns[column].ToolTipText = detail.KindLabel + (detail.Kind == 3
                ? " · XML range " + detail.RangeLow + ".." + detail.RangeHigh : string.Empty);
        }
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
        try
        {
            ValidateCandidate(_active.ColumnDetails[e.ColumnIndex - 1], value);
            Fc26SnapshotLoader.StageDetailValue(_active.Name, rowIndex, field, value);
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Stage database value", ex, "The value was not staged. Review its supported range and retry."); LoadTable(); }
        UpdateStatus();
    }

    private void CopySelection()
    {
        var data = _grid.GetClipboardContent();
        if (data != null) Clipboard.SetDataObject(data);
    }

    private int CurrentSourceRow()
    {
        return _grid.CurrentRow == null ? -1 : Convert.ToInt32(_grid.CurrentRow.Cells[0].Value, CultureInfo.InvariantCulture);
    }

    private void CloneRow()
    {
        var row = CurrentSourceRow();
        if (_active == null || row < 0) return;
        try
        {
            var newIndex = Fc26SnapshotLoader.DuplicateDetailRow(_active.Name, row);
            LoadTable();
            foreach (DataGridViewRow gridRow in _grid.Rows)
                if (Convert.ToInt32(gridRow.Cells[0].Value, CultureInfo.InvariantCulture) == newIndex)
                { gridRow.Selected = true; _grid.CurrentCell = gridRow.Cells[Math.Min(1, gridRow.Cells.Count - 1)]; break; }
            _status.Text = "Cloned " + _active.Name + "[" + row + "] to new row " + newIndex + ". Change its identity fields before File > Save.";
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Clone database record", ex, "No clone was accepted. Save or revert other structural edits, then retry."); }
    }

    private void DeleteRow()
    {
        var row = CurrentSourceRow();
        if (_active == null || row < 0) return;
        var field = _active.Columns.FirstOrDefault(name => name.EndsWith("id", StringComparison.OrdinalIgnoreCase));
        var identity = field == null ? string.Empty : " (" + field + "=" + _active.Value(row, field) + ")";
        if (MessageBox.Show(this, "Delete " + _active.Name + "[" + row + "]" + identity + "?\r\n\r\nCM26 will remove dependent link rows and clear optional references through the native relationship engine.",
            "Dependency-aware delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            Fc26SnapshotLoader.DeleteDetailRow(_active.Name, row);
            LoadTable();
            _status.Text = "Deletion staged in isolation. Use File > Save to validate, back up and commit it.";
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Delete database record", ex, "No deletion was accepted. Resolve pending edits and dependency warnings first."); }
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

    private void BulkSet()
    {
        if (_active == null || _grid.SelectedCells.Count == 0) return;
        string value;
        if (!Prompt("Set selected cells", "New value for every selected writable cell:", string.Empty, out value)) return;
        var targets = _grid.SelectedCells.Cast<DataGridViewCell>()
            .Where(cell => cell.ColumnIndex > 0 && !_grid.Columns[cell.ColumnIndex].ReadOnly)
            .ToArray();
        if (targets.Length == 0) return;
        if (MessageBox.Show(this, "Stage " + targets.Length + " selected cell update(s)?",
            "Bulk edit preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var changed = 0;
        foreach (var cell in targets)
        {
            var column = cell.ColumnIndex - 1;
            ValidateCandidate(_active.ColumnDetails[column], value);
            var row = Convert.ToInt32(_grid.Rows[cell.RowIndex].Cells[0].Value, CultureInfo.InvariantCulture);
            Fc26SnapshotLoader.StageDetailValue(_active.Name, row, _active.Columns[column], value);
            cell.Value = value;
            changed++;
        }
        _status.Text = changed + " bulk value(s) staged; use File > Save to validate and commit.";
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

    private void RemoveReferences()
    {
        if (_grid.CurrentCell == null || _grid.CurrentCell.ColumnIndex <= 0) return;
        var field = _active.Columns[_grid.CurrentCell.ColumnIndex - 1];
        var value = Convert.ToString(_grid.CurrentCell.Value, CultureInfo.InvariantCulture) ?? string.Empty;
        var hits = FindReferences(field, value).Where(hit => !hit.Table.Equals(_active.Name, StringComparison.OrdinalIgnoreCase) || hit.Row != CurrentSourceRow()).ToArray();
        var removable = new List<ReferenceHit>(); var blocked = new List<ReferenceHit>();
        foreach (var hit in hits)
        {
            var table = Fc26SnapshotLoader.DetailTable(hit.Table); var column = table?.Column(hit.Field) ?? -1;
            var detail = column >= 0 ? table.ColumnDetails[column] : null;
            if (detail != null && detail.IsWritable && (detail.Kind != 3 || detail.RangeLow <= 0)) removable.Add(hit); else blocked.Add(hit);
        }
        if (removable.Count == 0)
        {
            MessageBox.Show(this, "No optional references can be cleared safely. " + blocked.Count +
                " required relationship(s) are protected; delete their link rows through the dependency-aware Delete workflow if appropriate.",
                "Remove references", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        if (MessageBox.Show(this, "Clear " + removable.Count + " optional reference(s) by setting them to 0?\r\n" + blocked.Count +
            " required reference(s) will remain protected.", "Remove references preview", MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var hit in removable) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, "0");
        LoadTable(); _status.Text = removable.Count + " optional reference(s) cleared; " + blocked.Count + " required link(s) protected.";
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
            try
            {
                var lines = File.ReadAllLines(dialog.FileName);
                if (lines.Length == 0 || !lines[0].Split('\t').SequenceEqual(_active.Columns))
                    throw new InvalidDataException("TSV header does not match the selected FC26 table.");
                if (lines.Length - 1 != _active.Rows.Count)
                    throw new InvalidDataException("TSV row count must match the selected table. Use Clone/Delete for structural changes.");
                var staged = 0;
                for (var row = 0; row < _active.Rows.Count; row++)
                {
                    var values = lines[row + 1].Split('\t');
                    if (values.Length != _active.Columns.Length) throw new InvalidDataException("TSV column count mismatch at row " + (row + 2) + ".");
                    for (var column = 0; column < values.Length; column++)
                    {
                        if (!_active.ColumnDetails[column].IsWritable || values[column] == _active.Rows[row][column]) continue;
                        ValidateCandidate(_active.ColumnDetails[column], values[column]);
                        Fc26SnapshotLoader.StageDetailValue(_active.Name, row, _active.Columns[column], values[column]);
                        staged++;
                    }
                }
                LoadTable();
                _status.Text = staged + " imported value(s) staged; source files are still untouched.";
            }
            catch (Exception ex) { Fc26FriendlyError.Show(this, "Import table", ex, "Invalid imported values were not staged. Review the table columns and ranges, then retry."); }
        }
    }

    private void ExportAllTables()
    {
        using (var dialog = new FolderBrowserDialog { Description = "Choose a folder for the FC26 TSV table export" })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            var exported = 0;
            foreach (var tableName in Fc26SnapshotLoader.DetailTableNames)
            {
                var table = Fc26SnapshotLoader.DetailTable(tableName);
                if (table == null) continue;
                var fileName = string.Concat(table.Name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character)) + ".tsv";
                var lines = new List<string> { string.Join("\t", table.Columns) };
                lines.AddRange(table.Rows.Select(row => string.Join("\t", row.Select(SafeTsv))));
                File.WriteAllLines(Path.Combine(dialog.SelectedPath, fileName), lines, new UTF8Encoding(false));
                exported++;
            }
            MessageBox.Show(this, exported + " table(s) exported to:\r\n" + dialog.SelectedPath,
                "Export all FC26 tables", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void ImportAllTables()
    {
        using (var dialog = new FolderBrowserDialog { Description = "Choose a folder containing CM26 TSV table exports" })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var changes = new List<ImportChange>();
                var files = Directory.GetFiles(dialog.SelectedPath, "*.tsv", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var table = Fc26SnapshotLoader.DetailTable(Path.GetFileNameWithoutExtension(file));
                    if (table == null) continue;
                    var lines = File.ReadAllLines(file);
                    if (lines.Length == 0 || !lines[0].Split('\t').SequenceEqual(table.Columns))
                        throw new InvalidDataException(Path.GetFileName(file) + " has a header that does not match the loaded table.");
                    if (lines.Length - 1 != table.Rows.Count)
                        throw new InvalidDataException(Path.GetFileName(file) + " has a different row count. Use Clone/Delete for structural changes.");
                    for (var row = 0; row < table.Rows.Count; row++)
                    {
                        var values = lines[row + 1].Split('\t');
                        if (values.Length != table.Columns.Length)
                            throw new InvalidDataException(Path.GetFileName(file) + " has a column mismatch at row " + (row + 2) + ".");
                        for (var column = 0; column < values.Length; column++)
                        {
                            if (!table.ColumnDetails[column].IsWritable || values[column] == table.Rows[row][column]) continue;
                            ValidateCandidate(table.ColumnDetails[column], values[column]);
                            changes.Add(new ImportChange(table.Name, row, table.Columns[column], table.Rows[row][column], values[column]));
                        }
                    }
                }
                if (changes.Count == 0)
                {
                    MessageBox.Show(this, "No writable differences were found in the TSV folder.", "Import all FC26 tables");
                    return;
                }
                if (MessageBox.Show(this, "Validated " + changes.Count + " writable difference(s) across " +
                    changes.Select(change => change.Table).Distinct(StringComparer.OrdinalIgnoreCase).Count() +
                    " table(s). Stage them as one import transaction?", "Import all preview",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                var applied = new List<ImportChange>();
                try
                {
                    foreach (var change in changes)
                    {
                        Fc26SnapshotLoader.StageDetailValue(change.Table, change.Row, change.Field, change.NewValue);
                        applied.Add(change);
                    }
                }
                catch
                {
                    foreach (var change in applied.AsEnumerable().Reverse())
                        Fc26SnapshotLoader.StageDetailValue(change.Table, change.Row, change.Field, change.OldValue);
                    throw;
                }
                LoadTable();
                _status.Text = changes.Count + " imported value(s) staged after full-folder validation.";
            }
            catch (Exception ex)
            {
                Fc26FriendlyError.Show(this, "Import all tables", ex,
                    "The folder import was rejected and any partially staged values were rolled back.");
            }
        }
    }

    private void SaveFilterPreset()
    {
        if (_active == null) return;
        using var dialog = new SaveFileDialog { Filter = "CM26 filter preset (*.cm26filter)|*.cm26filter", FileName = _active.Name + ".cm26filter" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllLines(dialog.FileName, new[] { _active.Name, _filterField.Text, _filterExpression.Text, _rowSearch.Text }, new UTF8Encoding(false));
    }

    private void LoadFilterPreset()
    {
        using var dialog = new OpenFileDialog { Filter = "CM26 filter preset (*.cm26filter)|*.cm26filter|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var lines = File.ReadAllLines(dialog.FileName); if (lines.Length < 4) throw new InvalidDataException("Invalid CM26 filter preset.");
        if (_tables.Items.Contains(lines[0])) _tables.SelectedItem = lines[0];
        if (_filterField.Items.Contains(lines[1])) _filterField.SelectedItem = lines[1];
        _filterExpression.Text = lines[2]; _rowSearch.Text = lines[3]; LoadTable();
    }

    private void SaveRowTemplate()
    {
        var row = CurrentSourceRow(); if (_active == null || row < 0) return;
        using var dialog = new SaveFileDialog { Filter = "CM26 row template (*.cm26template)|*.cm26template", FileName = _active.Name + ".cm26template" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var columns = Enumerable.Range(0, _active.Columns.Length).Where(index => _active.ColumnDetails[index].IsWritable && !_active.Columns[index].EndsWith("id", StringComparison.OrdinalIgnoreCase)).ToArray();
        File.WriteAllLines(dialog.FileName, new[] { _active.Name, string.Join("\t", columns.Select(index => _active.Columns[index])),
            string.Join("\t", columns.Select(index => SafeTsv(_active.Rows[row][index]))) }, new UTF8Encoding(false));
    }

    private void ApplyRowTemplate()
    {
        if (_active == null) return;
        using var dialog = new OpenFileDialog { Filter = "CM26 row template (*.cm26template)|*.cm26template|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var lines = File.ReadAllLines(dialog.FileName);
        if (lines.Length < 3 || !lines[0].Equals(_active.Name, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Template does not match " + _active.Name + ".");
        var fields = lines[1].Split('\t'); var values = lines[2].Split('\t');
        if (fields.Length != values.Length) throw new InvalidDataException("Template field/value count does not match.");
        var rows = _grid.SelectedCells.Cast<DataGridViewCell>().Select(cell => cell.RowIndex).Distinct().Select(index => Convert.ToInt32(_grid.Rows[index].Cells[0].Value, CultureInfo.InvariantCulture)).ToArray();
        if (rows.Length == 0) return;
        var edits = new List<ImportChange>();
        for (var index = 0; index < fields.Length; index++)
        {
            var column = _active.Column(fields[index]); if (column < 0 || !_active.ColumnDetails[column].IsWritable || fields[index].EndsWith("id", StringComparison.OrdinalIgnoreCase)) continue;
            ValidateCandidate(_active.ColumnDetails[column], values[index]);
            foreach (var row in rows) if (_active.Rows[row][column] != values[index]) edits.Add(new ImportChange(_active.Name, row, fields[index], _active.Rows[row][column], values[index]));
        }
        if (MessageBox.Show(this, "Apply " + edits.Count + " template field update(s) across " + rows.Length + " selected row(s)? Identity fields are protected.",
            "Row template preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var edit in edits) Fc26SnapshotLoader.StageDetailValue(edit.Table, edit.Row, edit.Field, edit.NewValue);
        LoadTable(); _status.Text = edits.Count + " row-template update(s) staged.";
    }

    private bool MatchesFieldFilter(string[] row)
    {
        var expression = _filterExpression.Text.Trim();
        if (expression.Length == 0 || _filterField.SelectedItem == null || _filterField.SelectedItem.ToString() == "All fields") return true;
        var column = _active.Column(_filterField.SelectedItem.ToString());
        if (column < 0 || column >= row.Length) return false;
        var value = row[column] ?? string.Empty;
        foreach (var op in new[] { ">=", "<=", "!=", "=", ">", "<" })
        {
            if (!expression.StartsWith(op, StringComparison.Ordinal)) continue;
            var operand = expression.Substring(op.Length).Trim();
            if (op == "=") return value.Equals(operand, StringComparison.OrdinalIgnoreCase);
            if (op == "!=") return !value.Equals(operand, StringComparison.OrdinalIgnoreCase);
            double left, right;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out left) ||
                !double.TryParse(operand, NumberStyles.Float, CultureInfo.InvariantCulture, out right)) return false;
            return op == ">=" ? left >= right : op == "<=" ? left <= right : op == ">" ? left > right : left < right;
        }
        return NormalizeText(value).Contains(NormalizeText(expression));
    }

    private static void ValidateCandidate(SnapshotDetailColumn column, string value)
    {
        if (!column.IsWritable) throw new InvalidOperationException(column.Name + " is read-only in the FC26 XML descriptor.");
        if (column.Kind != 3) return;
        long parsed;
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            throw new InvalidOperationException(column.Name + " requires an integer.");
        if (parsed < column.RangeLow || parsed > column.RangeHigh)
            throw new InvalidOperationException(column.Name + " must be within XML range " + column.RangeLow + ".." + column.RangeHigh + ".");
    }

    private void ValidateDescriptorRanges()
    {
        var issues = new List<string>();
        foreach (var tableName in Fc26SnapshotLoader.DetailTableNames)
        {
            var table = Fc26SnapshotLoader.DetailTable(tableName);
            if (table == null) continue;
            for (var column = 0; column < table.ColumnDetails.Length; column++)
            {
                var detail = table.ColumnDetails[column];
                if (detail.Kind != 3) continue;
                for (var row = 0; row < table.Rows.Count; row++)
                {
                    long value;
                    if (column >= table.Rows[row].Length || !long.TryParse(table.Rows[row][column], out value) ||
                        value < detail.RangeLow || value > detail.RangeHigh)
                        issues.Add(table.Name + "[" + row + "]." + detail.Name + " = " + (column < table.Rows[row].Length ? table.Rows[row][column] : "<missing>") +
                            " (expected " + detail.RangeLow + ".." + detail.RangeHigh + ")");
                }
            }
        }
        MessageBox.Show(this, issues.Count == 0 ? "All integer values pass the loaded FC26 XML descriptor ranges." :
            issues.Count + " XML range issue(s):\r\n\r\n" + string.Join("\r\n", issues.Take(300)),
            "FC26 XML descriptor validation", MessageBoxButtons.OK, issues.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void CompareTsv()
    {
        if (_active == null) return;
        using (var dialog = new OpenFileDialog { Filter = "TSV files (*.tsv)|*.tsv|All files (*.*)|*.*" })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var lines = File.ReadAllLines(dialog.FileName);
                if (lines.Length == 0 || !lines[0].Split('\t').SequenceEqual(_active.Columns))
                    throw new InvalidDataException("Comparison header does not match " + _active.Name + ".");
                var differences = new List<string>();
                var rows = Math.Max(_active.Rows.Count, lines.Length - 1);
                for (var row = 0; row < rows; row++)
                {
                    var other = row + 1 < lines.Length ? lines[row + 1].Split('\t') : Array.Empty<string>();
                    for (var column = 0; column < _active.Columns.Length; column++)
                    {
                        var current = row < _active.Rows.Count && column < _active.Rows[row].Length ? _active.Rows[row][column] : "<missing>";
                        var compared = column < other.Length ? other[column] : "<missing>";
                        if (current != compared) differences.Add("[" + row + "]." + _active.Columns[column] + ": '" + current + "' -> '" + compared + "'");
                    }
                }
                MessageBox.Show(this, differences.Count == 0 ? "No differences." : differences.Count + " different cell(s):\r\n\r\n" +
                    string.Join("\r\n", differences.Take(300)), "Compare " + _active.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { Fc26FriendlyError.Show(this, "Compare table", ex, "The active database was not changed. Reopen the comparison snapshot and retry."); }
        }
    }

    private void SwapIds()
    {
        if (_grid.CurrentCell == null || _grid.CurrentCell.ColumnIndex <= 0) return;
        var field = _active.Columns[_grid.CurrentCell.ColumnIndex - 1];
        if (!field.EndsWith("id", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "Select an ID field first.", "Swap IDs", MessageBoxButtons.OK, MessageBoxIcon.Information); return;
        }
        var first = Convert.ToString(_grid.CurrentCell.Value, CultureInfo.InvariantCulture) ?? string.Empty;
        string second;
        if (!Prompt("Swap linked IDs", "Other existing " + field + " value:", string.Empty, out second) || second == first) return;
        var firstHits = FindReferences(field, first);
        var secondHits = FindReferences(field, second);
        if (secondHits.Count == 0) { MessageBox.Show(this, "The other ID does not exist.", "Swap IDs", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (MessageBox.Show(this, "Swap " + firstHits.Count + " reference(s) for " + first + " with " + secondHits.Count + " reference(s) for " + second + "?",
            "Dependency-aware ID swap", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var hit in firstHits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, second);
        foreach (var hit in secondHits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, first);
        LoadTable();
        _status.Text = (firstHits.Count + secondHits.Count) + " linked ID values staged for atomic save.";
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

    private sealed class ImportChange
    {
        internal ImportChange(string table, int row, string field, string oldValue, string newValue)
        { Table = table; Row = row; Field = field; OldValue = oldValue; NewValue = newValue; }
        internal string Table { get; }
        internal int Row { get; }
        internal string Field { get; }
        internal string OldValue { get; }
        internal string NewValue { get; }
    }
}
