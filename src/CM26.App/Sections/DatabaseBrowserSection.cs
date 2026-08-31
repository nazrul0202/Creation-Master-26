using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;
using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>
/// Advanced table browser. Supported scalar fields can be edited through the same
/// staged, validated save path as the dedicated editors.
/// </summary>
public sealed class DatabaseBrowserSection : SectionBase
{
    private readonly DataGridView _grid;
    private readonly Label _info;
    private readonly Button _previousPage;
    private readonly Button _nextPage;
    private readonly Button _duplicateRow;
    private readonly Button _deleteRow;
    private readonly StudioToolbar _toolbar;
    private readonly FlowLayoutPanel _actions;
    private readonly ComboBox _filterColumn;
    private readonly TextBox _recordSearch;
    private List<DbTable> _ordered = new();
    private DbTable? _activeTable;
    private bool _binding;
    private int _pageStart;
    private List<int>? _filteredRows;
    private const int PageSize = 500;

    public override string SectionKey => "browser";
    public override string SectionTitle => "Database Browser";
    protected override string TableName => ""; // dynamic per selection
    protected override bool ShowRecordCommandStrip => false;

    public DatabaseBrowserSection(AppServices s) : base(s)
    {
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = false,
            AllowUserToOrderColumns = true,
            Font = Theme.Body,
            BackgroundColor = StudioColors.AppBackground,
            BorderStyle = BorderStyle.None,
        };
        Theme.ApplyGrid(_grid);
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.CellBeginEdit += (_, e) =>
        {
            if (_binding || !CanEdit(e.ColumnIndex)) e.Cancel = true;
        };
        _grid.CellEndEdit += (_, e) => StageGridEdit(e.RowIndex, e.ColumnIndex);
        _info = new Label { Dock = DockStyle.Fill, ForeColor = StudioColors.MutedText, Font = Theme.Body, Padding = new Padding(6, 4, 0, 0), BackColor = Color.Transparent };
        _previousPage = new Button { Text = "Previous", Dock = DockStyle.Right, Width = 76, Enabled = false };
        _nextPage = new Button { Text = "Next", Dock = DockStyle.Right, Width = 60, Enabled = false };
        _previousPage.Click += (_, _) => ChangePage(-1);
        _nextPage.Click += (_, _) => ChangePage(1);
        _duplicateRow = new Button { Text = "Duplicate row", Dock = DockStyle.Right, Width = 100, Enabled = false };
        _deleteRow = new Button { Text = "Delete row", Dock = DockStyle.Right, Width = 86, Enabled = false };
        _duplicateRow.Click += (_, _) => DuplicateSelectedRow();
        _deleteRow.Click += (_, _) => DeleteSelectedRow();
        Theme.ApplyButton(_previousPage);
        Theme.ApplyButton(_nextPage);
        Theme.ApplyButton(_duplicateRow);
        Theme.ApplyButton(_deleteRow);
        var pager = new BufferedPanel { Dock = DockStyle.Top, Height = 28, BackColor = StudioColors.Surface };
        pager.Controls.Add(_info);
        pager.Controls.Add(_nextPage);
        pager.Controls.Add(_previousPage);
        pager.Controls.Add(_deleteRow);
        pager.Controls.Add(_duplicateRow);

        var card = new StudioCard { Dock = DockStyle.Fill, BackColor = StudioColors.Surface };
        card.Controls.Add(_grid);
        card.Controls.Add(pager);

        _actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 43,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(5, 4, 5, 2),
            BackColor = StudioColors.Surface,
        };
        AddAction("Copy", CopySelection);
        AddAction("Paste", PasteSelection);
        AddAction("Replace", BulkReplace);
        AddAction("Export table", ExportTable);
        AddAction("Export all", ExportAllTables);
        AddAction("Import table", ImportTable);
        AddAction("Compare file", CompareFile);
        AddAction("Save template", SaveRowTemplate);
        AddAction("Apply template", ApplyRowTemplate);
        AddAction("Dependencies", ShowDependencies);
        AddAction("Replace refs", ReplaceReferences);
        AddAction("Pending changes", ShowPendingChanges);
        AddAction("History", ShowHistory);
        _filterColumn = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(8, 1, 2, 0) };
        _recordSearch = new TextBox { Width = 170, PlaceholderText = "Filter rows…", Margin = new Padding(2, 1, 2, 0) };
        _recordSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            ApplyRecordFilter();
        };
        _actions.Controls.Add(_filterColumn);
        _actions.Controls.Add(_recordSearch);
        AddAction("Filter", ApplyRecordFilter);
        AddAction("Clear", ClearRecordFilter);
        AddAction("Save filter", SaveFilterPreset);
        AddAction("Saved filters", LoadFilterPreset);
        AddAction("Asset usage", ShowAssetUsage);
        card.Controls.Add(_actions);

        _toolbar = new StudioToolbar
        {
            Title = "Database Browser",
            CanCreate = false,
            ShowFilter = false,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchBox.PlaceholderText = "Search tables…";
        _toolbar.PreviousClicked += (_, _) => StepTable(-1);
        _toolbar.NextClicked += (_, _) => StepTable(+1);
        _toolbar.SearchClicked += (_, _) => FindTable(_toolbar.SearchText);
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            FindTable(_toolbar.SearchText);
        };

        var page = new TabPage("Records") { BackColor = StudioColors.AppBackground };
        page.Controls.Add(card);
        page.Controls.Add(_toolbar);
        Tabs.TabPages.Add(page);
        Header.SetRecord("Database Browser", "Inspect every table and edit fields supported by the validated writer", IconService.Get("browser", 44));
    }

    private void AddAction(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 27, Margin = new Padding(2, 0, 2, 0) };
        button.Click += (_, _) => action();
        Theme.ApplyButton(button);
        _actions.Controls.Add(button);
    }

    private void StepTable(int delta)
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
        if (found < 0) return;
        var next = found + delta;
        if (next >= 0 && next < records.Count)
            GoToRecord(records[next].RecordIndex);
    }

    private void FindTable(string query)
    {
        var term = query.Trim();
        if (string.IsNullOrWhiteSpace(term)) return;
        var match = GetRecords().FirstOrDefault(item => item.Matches(term));
        if (match == null)
        {
            MessageBox.Show(this, $"No table matches '{term}'.", "Find Table", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        GoToRecord(match.RecordIndex);
    }

    protected override IReadOnlyList<RecordListItem> GetRecords()
    {
        _ordered = Services.Session.Tables.OrderBy(t => t.Name).ToList();
        var list = new List<RecordListItem>(_ordered.Count);
        for (int i = 0; i < _ordered.Count; i++)
            list.Add(new RecordListItem
            {
                RecordIndex = i, // position in _ordered
                Title = _ordered[i].Name,
                Subtitle = _ordered[i].IsLocale ? "Locale" : "Main",
                Detail = $"{_ordered[i].RowCount:N0} rows",
            });
        return list;
    }

    protected override void ShowRecord(int recordIndex)
    {
        if (recordIndex < 0 || recordIndex >= _ordered.Count) return;
        var table = _ordered[recordIndex];
        _pageStart = 0;
        _filteredRows = null;
        _recordSearch.Clear();
        Header.SetRecord(table.Name,
            $"{table.RowCount:N0} rows · {(table.IsLocale ? "locale" : "main")} · {table.Columns.Count} columns",
            IconService.Get("browser", 44));
        BindGrid(table);
    }

    private void BindGrid(DbTable table)
    {
        _activeTable = table;
        _binding = true;
        _grid.SuspendLayout();
        try
        {
            _grid.Columns.Clear();
            _grid.Rows.Clear();
            foreach (var c in table.Columns)
            {
                var column = _grid.Columns[_grid.Columns.Add(c.Name, c.Name)];
                column.ReadOnly = !c.IsWritable;
                column.ToolTipText = c.IsWritable
                    ? "Editable: staged and validated before Save."
                    : "Read-only: unsupported by the validated database writer.";
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
            _grid.BackgroundColor = StudioColors.AppBackground;
            _grid.EnableHeadersVisualStyles = false;

            var previousFilter = Convert.ToString(_filterColumn.SelectedItem);
            _filterColumn.Items.Clear();
            _filterColumn.Items.Add("All fields");
            foreach (var column in table.Columns) _filterColumn.Items.Add(column.Name);
            _filterColumn.SelectedItem = previousFilter != null && _filterColumn.Items.Contains(previousFilter)
                ? previousFilter
                : "All fields";

            var recordCount = _filteredRows?.Count ?? table.RowCount;
            int rows = Math.Min(Math.Max(0, recordCount - _pageStart), PageSize);
            for (int offset = 0; offset < rows; offset++)
            {
                int r = _filteredRows?[_pageStart + offset] ?? _pageStart + offset;
                var rec = Services.Session.GetRecord(table.Name, r);
                if (rec == null) continue;
                var row = _grid.Rows.Add(rec.Values.Cast<object>().ToArray());
                _grid.Rows[row].Tag = r;
            }
            _previousPage.Enabled = _pageStart > 0;
            _nextPage.Enabled = _pageStart + rows < recordCount;
            _duplicateRow.Enabled = _grid.Rows.Count > 0;
            _deleteRow.Enabled = _grid.Rows.Count > 0;
            _info.Text = recordCount == 0
                ? "0 rows. Editable cells are staged, validated, and saved with Ctrl+S."
                : $"Rows {_pageStart + 1:N0}-{_pageStart + rows:N0} of {recordCount:N0}" +
                  (_filteredRows != null ? $" filtered ({table.RowCount:N0} total)" : string.Empty) +
                  ". Editable cells are staged, validated, and saved with Ctrl+S.";
        }
        finally
        {
            _grid.ResumeLayout();
            _binding = false;
        }
    }

    private void ChangePage(int direction)
    {
        if (_activeTable == null) return;
        var next = _pageStart + direction * PageSize;
        var count = _filteredRows?.Count ?? _activeTable.RowCount;
        if (next < 0 || next >= count) return;
        _pageStart = next;
        BindGrid(_activeTable);
    }

    private bool CanEdit(int columnIndex) =>
        _activeTable != null && columnIndex >= 0 && columnIndex < _activeTable.Columns.Count &&
        _activeTable.Columns[columnIndex].IsWritable;

    private void StageGridEdit(int gridRow, int columnIndex)
    {
        if (_binding || _activeTable == null || !CanEdit(columnIndex) || gridRow < 0 || gridRow >= _grid.Rows.Count)
            return;
        if (_grid.Rows[gridRow].Tag is not int recordIndex) return;

        var field = _activeTable.Columns[columnIndex].Name;
        var newValue = Convert.ToString(_grid.Rows[gridRow].Cells[columnIndex].Value) ?? string.Empty;
        var outcome = Services.Pending.Stage(_activeTable.Name, recordIndex, field, newValue);
        if (!outcome.Success)
        {
            _binding = true;
            _grid.CancelEdit();
            _grid.Rows[gridRow].Cells[columnIndex].Value = Services.Session.GetCell(_activeTable.Name, recordIndex, field);
            _binding = false;
            MessageBox.Show(this, outcome.Message, "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Services.NotifyPendingChanged();
        _info.Text = $"Staged {_activeTable.Name}[{recordIndex}].{field}. Use Validate or Ctrl+S to save {Services.Pending.Count} change(s).";
    }

    private int? SelectedRecordIndex() => _grid.CurrentRow?.Tag as int?;

    private void DuplicateSelectedRow()
    {
        if (_activeTable == null || SelectedRecordIndex() is not int row) return;
        var result = Services.Session.DuplicateRow(_activeTable.Name, row);
        if (!result.Success) { MessageBox.Show(this, result.Message, "Duplicate record", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Services.Pending.MarkStructuralChange();
        Services.Session.RefreshSchema();
        ReloadActiveTable();
        // The base reload keeps the current table selected; this message must be
        // set after LoadData because the reload re-renders the pager info text.
        _info.Text = "Record duplicated in memory. Change key fields before Ctrl+S; relationship cleanup is manual.";
    }

    private void DeleteSelectedRow()
    {
        if (_activeTable == null || SelectedRecordIndex() is not int row) return;
        if (MessageBox.Show(this, "Delete the selected record and apply CM16-style relationship cleanup? Dependent link rows are removed and optional references are cleared.", "Delete record", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var result = Services.Session.DeleteRowWithRelationships(_activeTable.Name, row);
        if (!result.Success) { MessageBox.Show(this, result.Message, "Delete record", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Services.Pending.MarkStructuralChange();
        Services.Session.RefreshSchema();
        ReloadActiveTable();
        // Keep the deletion notice visible after the reload re-renders the pager.
        _info.Text = result.Message + ". Ctrl+S creates backups, saves and reload-verifies.";
    }

    private void ReloadActiveTable()
    {
        var tableName = _activeTable?.Name;
        LoadData();
        if (string.IsNullOrWhiteSpace(tableName)) return;
        var index = _ordered.FindIndex(table => table.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) GoToRecord(index);
    }

    private void CopySelection()
    {
        if (_grid.GetCellCount(DataGridViewElementStates.Selected) == 0) return;
        var data = _grid.GetClipboardContent();
        if (data != null) Clipboard.SetDataObject(data);
        _info.Text = $"Copied {_grid.GetCellCount(DataGridViewElementStates.Selected):N0} cell(s).";
    }

    private void PasteSelection()
    {
        if (_activeTable == null || _grid.CurrentCell == null || !Clipboard.ContainsText()) return;
        var values = TableWorkspaceService.Parse(Clipboard.GetText(), '\t');
        if (values.Count == 0) return;
        var startRow = _grid.CurrentCell.RowIndex;
        var startColumn = _grid.CurrentCell.ColumnIndex;
        var staged = 0;
        try
        {
            for (var rowOffset = 0; rowOffset < values.Count; rowOffset++)
            {
                var gridRow = startRow + rowOffset;
                if (gridRow >= _grid.Rows.Count || _grid.Rows[gridRow].Tag is not int recordIndex) break;
                for (var columnOffset = 0; columnOffset < values[rowOffset].Count; columnOffset++)
                {
                    var columnIndex = startColumn + columnOffset;
                    if (columnIndex >= _activeTable.Columns.Count) break;
                    if (!CanEdit(columnIndex))
                        throw new InvalidOperationException($"{_activeTable.Columns[columnIndex].Name} is read-only.");
                    var value = values[rowOffset][columnOffset];
                    var field = _activeTable.Columns[columnIndex].Name;
                    if (Services.Session.GetCell(_activeTable.Name, recordIndex, field) == value) continue;
                    var outcome = Services.Pending.Stage(_activeTable.Name, recordIndex, field, value);
                    if (!outcome.Success) throw new InvalidOperationException(outcome.Message);
                    staged++;
                }
            }
        }
        catch (Exception ex)
        {
            while (staged-- > 0) Services.Pending.Undo();
            FriendlyErrorDialog.Show(this, "Paste database values", ex, "Nothing was staged. Review the clipboard shape and writable columns, then retry.");
            return;
        }
        Services.NotifyPendingChanged();
        BindGrid(_activeTable);
        _info.Text = $"Pasted and staged {staged:N0} cell change(s). Validate or Ctrl+S to save.";
    }

    private void ExportTable()
    {
        if (_activeTable == null) return;
        using var dialog = new SaveFileDialog
        {
            Title = $"Export {_activeTable.Name}",
            FileName = _activeTable.Name + ".tsv",
            Filter = "Tab-separated table (*.tsv)|*.tsv|CSV table (*.csv)|*.csv",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            TableWorkspaceService.ExportTable(Services.Session, _activeTable.Name, dialog.FileName);
            _info.Text = $"Exported {_activeTable.RowCount:N0} {_activeTable.Name} row(s) to {dialog.FileName}.";
        }
        catch (Exception ex) { FriendlyErrorDialog.Show(this, "Export table", ex); }
    }

    private void ExportAllTables()
    {
        using var dialog = new FolderBrowserDialog { Description = "Choose a folder for all CM26 table exports", UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            foreach (var table in Services.Session.Tables)
                TableWorkspaceService.ExportTable(Services.Session, table.Name,
                    Path.Combine(dialog.SelectedPath, SafeFileName(table.Name) + ".tsv"));
            _info.Text = $"Exported {Services.Session.Tables.Count:N0} table(s) to {dialog.SelectedPath}.";
        }
        catch (Exception ex) { FriendlyErrorDialog.Show(this, "Export all tables", ex); }
    }

    private void ImportTable()
    {
        if (_activeTable == null) return;
        using var dialog = new OpenFileDialog
        {
            Title = $"Import edits into {_activeTable.Name}",
            Filter = "CM26 table exports (*.tsv;*.csv)|*.tsv;*.csv|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var plan = TableWorkspaceService.BuildImportPlan(Services.Session, _activeTable.Name, dialog.FileName);
            if (plan.Count == 0)
            {
                MessageBox.Show(this, "The file matches the current table; no changes are required.", "Import table", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this,
                    $"Preview: {plan.Count:N0} cell change(s) across {plan.Select(e => e.RowIndex).Distinct().Count():N0} row(s).\n\nStage these changes?",
                    "Import table preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var staged = 0;
            try
            {
                foreach (var edit in plan)
                {
                    var outcome = Services.Pending.Stage(edit.TableName, edit.RowIndex, edit.FieldName, edit.NewValue);
                    if (!outcome.Success) throw new InvalidOperationException(outcome.Message);
                    staged++;
                }
            }
            catch
            {
                while (staged-- > 0) Services.Pending.Undo();
                throw;
            }
            Services.NotifyPendingChanged();
            BindGrid(_activeTable);
            _info.Text = $"Imported and staged {plan.Count:N0} validated cell change(s). Ctrl+S to commit.";
        }
        catch (Exception ex) { FriendlyErrorDialog.Show(this, "Import table", ex, "Invalid rows were not staged. Review the columns and ranges, then retry."); }
    }

    private void ShowPendingChanges()
    {
        var changes = Services.Pending.Changes;
        var text = changes.Count == 0
            ? "No scalar changes are currently staged."
            : string.Join(Environment.NewLine, changes.Take(500).Select(change => change.Describe())) +
              (changes.Count > 500 ? $"{Environment.NewLine}… and {changes.Count - 500:N0} more." : string.Empty);
        ShowTextDialog("Pending changes", text);
    }

    private void ShowDependencies()
    {
        if (_activeTable == null || SelectedRecordIndex() is not int rowIndex) return;
        var key = FindIdentityColumn(_activeTable);
        if (key == null)
        {
            ShowTextDialog("Dependencies", "No reliable identity field was found for this table.");
            return;
        }
        var value = Services.Session.GetCell(_activeTable.Name, rowIndex, key.Name);
        var lines = new List<string> { $"{_activeTable.Name}[{rowIndex}].{key.Name} = {value}", string.Empty };
        Cursor = Cursors.WaitCursor;
        try
        {
            foreach (var table in Services.Session.Tables.OrderBy(table => table.Name))
            {
                var column = table.FindColumn(key.Name);
                if (column == null || table.Name.Equals(_activeTable.Name, StringComparison.OrdinalIgnoreCase)) continue;
                var count = 0;
                for (var row = 0; row < table.RowCount; row++)
                    if (Services.Session.GetCell(table.Name, row, column.Name) == value) count++;
                if (count > 0) lines.Add($"{table.Name}.{column.Name}: {count:N0} linked row(s)");
            }
        }
        finally { Cursor = Cursors.Default; }
        if (lines.Count == 2) lines.Add("No exact references were found in tables sharing this identity field.");
        ShowTextDialog("Dependency impact", string.Join(Environment.NewLine, lines));
    }

    private static DbColumn? FindIdentityColumn(DbTable table)
    {
        var singular = table.Name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? table.Name[..^1] : table.Name;
        return table.FindColumn(singular + "id")
            ?? table.FindColumn("artificialkey")
            ?? table.Columns.FirstOrDefault(column => column.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase));
    }

    private void ShowTextDialog(string title, string text)
    {
        using var dialog = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(760, 520),
            MinimizeBox = false,
            MaximizeBox = true,
            BackColor = StudioColors.AppBackground,
        };
        var box = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Text = text,
            Font = new Font(FontFamily.GenericMonospace, 9f),
            BackColor = StudioColors.Surface,
            ForeColor = StudioColors.PrimaryText,
        };
        dialog.Controls.Add(box);
        dialog.ShowDialog(this);
    }

    private static string SafeFileName(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
        return value;
    }

    private void ApplyRecordFilter()
    {
        if (_activeTable == null) return;
        var expression = _recordSearch.Text.Trim();
        if (expression.Length == 0) { ClearRecordFilter(); return; }
        var selectedField = Convert.ToString(_filterColumn.SelectedItem) ?? "All fields";
        var selectedColumn = selectedField == "All fields" ? -1 : _activeTable.Columns
            .Select((column, index) => (column, index))
            .FirstOrDefault(item => item.column.Name.Equals(selectedField, StringComparison.OrdinalIgnoreCase)).index;
        var matches = new List<int>();
        Cursor = Cursors.WaitCursor;
        try
        {
            for (var rowIndex = 0; rowIndex < _activeTable.RowCount; rowIndex++)
            {
                var record = Services.Session.GetRecord(_activeTable.Name, rowIndex);
                if (record == null) continue;
                var found = selectedColumn >= 0
                    ? TableWorkspaceService.MatchesFilter(record.Get(selectedColumn), expression)
                    : record.Values.Any(value => TableWorkspaceService.MatchesFilter(value, expression));
                if (found) matches.Add(rowIndex);
            }
        }
        finally { Cursor = Cursors.Default; }
        _filteredRows = matches;
        _pageStart = 0;
        BindGrid(_activeTable);
    }

    private void ClearRecordFilter()
    {
        if (_activeTable == null) return;
        _filteredRows = null;
        _recordSearch.Clear();
        _pageStart = 0;
        BindGrid(_activeTable);
    }

    private void BulkReplace()
    {
        if (_activeTable == null) return;
        var selected = _grid.SelectedCells.Cast<DataGridViewCell>()
            .Where(cell => cell.RowIndex >= 0 && cell.ColumnIndex >= 0 && CanEdit(cell.ColumnIndex))
            .ToArray();
        if (selected.Length == 0)
        {
            MessageBox.Show(this, "Select one or more writable cells first.", "Bulk replace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!PromptForText("Bulk replace", "Find exact value:", string.Empty, out var find) ||
            !PromptForText("Bulk replace", "Replacement value:", string.Empty, out var replacement)) return;
        var plan = new List<TableImportEdit>();
        foreach (var cell in selected)
        {
            if (_grid.Rows[cell.RowIndex].Tag is not int rowIndex) continue;
            var column = _activeTable.Columns[cell.ColumnIndex];
            var oldValue = Services.Session.GetCell(_activeTable.Name, rowIndex, column.Name);
            if (oldValue == find && oldValue != replacement)
                plan.Add(new(_activeTable.Name, rowIndex, column.Name, oldValue, replacement));
        }
        if (plan.Count == 0)
        {
            MessageBox.Show(this, "No selected writable cell contains that exact value.", "Bulk replace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ConfirmAndStage(plan, "Bulk replace")) return;
        BindGrid(_activeTable);
    }

    private void CompareFile()
    {
        if (_activeTable == null) return;
        using var dialog = new OpenFileDialog
        {
            Title = $"Compare {_activeTable.Name} with an exported table",
            Filter = "CM26 table exports (*.tsv;*.csv)|*.tsv;*.csv|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var plan = TableWorkspaceService.BuildImportPlan(Services.Session, _activeTable.Name, dialog.FileName);
            var lines = new List<string>
            {
                $"{plan.Count:N0} different writable cell(s) across {plan.Select(edit => edit.RowIndex).Distinct().Count():N0} row(s).",
                string.Empty,
            };
            lines.AddRange(plan.Take(500).Select(edit =>
                $"{edit.TableName}[{edit.RowIndex}].{edit.FieldName}: '{edit.OldValue}' → '{edit.NewValue}'"));
            if (plan.Count > 500) lines.Add($"… and {plan.Count - 500:N0} more.");
            ShowTextDialog("Table comparison", string.Join(Environment.NewLine, lines));
        }
        catch (Exception ex) { FriendlyErrorDialog.Show(this, "Compare table", ex); }
    }

    private void SaveRowTemplate()
    {
        if (_activeTable == null || SelectedRecordIndex() is not int rowIndex) return;
        using var dialog = new SaveFileDialog
        {
            Title = $"Save {_activeTable.Name} row template",
            FileName = _activeTable.Name + "-template.json",
            Filter = "CM26 row template (*.json)|*.json",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            TableWorkspaceService.ExportRowTemplate(Services.Session, _activeTable.Name, rowIndex, dialog.FileName);
            _info.Text = $"Saved a reusable {_activeTable.Name} field template to {dialog.FileName}.";
        }
        catch (Exception ex) { FriendlyErrorDialog.Show(this, "Save template", ex); }
    }

    private void ApplyRowTemplate()
    {
        if (_activeTable == null || SelectedRecordIndex() is not int rowIndex) return;
        using var dialog = new OpenFileDialog
        {
            Title = $"Apply a template to {_activeTable.Name}[{rowIndex}]",
            Filter = "CM26 row template (*.json)|*.json|All files (*.*)|*.*",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var plan = TableWorkspaceService.BuildTemplatePlan(Services.Session, _activeTable.Name, rowIndex, dialog.FileName);
            if (!ConfirmAndStage(plan, "Apply row template")) return;
            BindGrid(_activeTable);
        }
        catch (Exception ex) { FriendlyErrorDialog.Show(this, "Apply template", ex, "No invalid template value was accepted."); }
    }

    private void ReplaceReferences()
    {
        if (_activeTable == null || SelectedRecordIndex() is not int rowIndex) return;
        var key = FindIdentityColumn(_activeTable);
        if (key == null)
        {
            MessageBox.Show(this, "No reliable identity field was found for this table.", "Replace references", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var oldValue = Services.Session.GetCell(_activeTable.Name, rowIndex, key.Name);
        if (!PromptForText("Replace references", $"Replace every linked {key.Name}={oldValue} with:", oldValue, out var newValue) || newValue == oldValue) return;
        var targetExists = false;
        for (var row = 0; row < _activeTable.RowCount; row++)
            if (Services.Session.GetCell(_activeTable.Name, row, key.Name) == newValue) { targetExists = true; break; }
        if (!targetExists)
        {
            MessageBox.Show(this, $"Target {key.Name}={newValue} does not exist in {_activeTable.Name}.", "Replace references", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var plan = new List<TableImportEdit>();
        Cursor = Cursors.WaitCursor;
        try
        {
            foreach (var table in Services.Session.Tables)
            {
                if (table.Name.Equals(_activeTable.Name, StringComparison.OrdinalIgnoreCase)) continue;
                var column = table.FindColumn(key.Name);
                if (column == null || !column.IsWritable) continue;
                for (var row = 0; row < table.RowCount; row++)
                    if (Services.Session.GetCell(table.Name, row, column.Name) == oldValue)
                        plan.Add(new(table.Name, row, column.Name, oldValue, newValue));
            }
        }
        finally { Cursor = Cursors.Default; }
        ConfirmAndStage(plan, "Replace linked references");
    }

    private void ShowHistory()
    {
        var history = Services.Pending.History;
        var text = history.Count == 0
            ? "No workspace actions have been recorded in this session."
            : string.Join(Environment.NewLine, history.Reverse().Take(1000)
                .Select(entry => $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}  {entry.Action,-12} {entry.Description}"));
        ShowTextDialog("Workspace history", text);
    }

    private bool ConfirmAndStage(IReadOnlyList<TableImportEdit> plan, string title)
    {
        if (plan.Count == 0)
        {
            MessageBox.Show(this, "No changes are required.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        if (MessageBox.Show(this,
                $"Preview: {plan.Count:N0} validated cell change(s) across {plan.Select(edit => (edit.TableName, edit.RowIndex)).Distinct().Count():N0} row(s).\n\nStage all changes?",
                title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return false;
        var staged = 0;
        try
        {
            foreach (var edit in plan)
            {
                var outcome = Services.Pending.Stage(edit.TableName, edit.RowIndex, edit.FieldName, edit.NewValue);
                if (!outcome.Success) throw new InvalidOperationException(outcome.Message);
                staged++;
            }
        }
        catch (Exception ex)
        {
            for (var i = 0; i < staged; i++) Services.Pending.Undo();
            FriendlyErrorDialog.Show(this, title, ex, "Nothing was staged. Review the selected records and retry.");
            return false;
        }
        Services.NotifyPendingChanged();
        _info.Text = $"{title}: staged {staged:N0} change(s). Validate or Ctrl+S to commit.";
        return true;
    }

    private bool PromptForText(string title, string label, string initialValue, out string value)
    {
        using var dialog = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(440, 126),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            BackColor = StudioColors.AppBackground,
        };
        var caption = new Label { Text = label, Left = 12, Top = 12, Width = 416, ForeColor = StudioColors.PrimaryText };
        var input = new TextBox { Text = initialValue, Left = 12, Top = 38, Width = 416 };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 268, Top = 82, Width = 76 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 352, Top = 82, Width = 76 };
        Theme.ApplyButton(ok); Theme.ApplyButton(cancel);
        dialog.Controls.AddRange(new Control[] { caption, input, ok, cancel });
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;
        var accepted = dialog.ShowDialog(this) == DialogResult.OK;
        value = input.Text;
        return accepted;
    }

    private void SaveFilterPreset()
    {
        if (_activeTable == null || string.IsNullOrWhiteSpace(_recordSearch.Text)) return;
        if (!PromptForText("Save filter", "Preset name:", $"{_activeTable.Name} filter", out var name) || string.IsNullOrWhiteSpace(name)) return;
        WorkspacePresetService.Save(new WorkspaceFilterPreset(
            name.Trim(), _activeTable.Name, Convert.ToString(_filterColumn.SelectedItem) ?? "All fields", _recordSearch.Text.Trim()));
        _info.Text = $"Saved filter preset '{name.Trim()}'.";
    }

    private void LoadFilterPreset()
    {
        var presets = WorkspacePresetService.Load();
        if (presets.Count == 0)
        {
            MessageBox.Show(this, "No saved filters are available.", "Saved filters", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new Form
        {
            Text = "Saved filters", StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(580, 360),
            BackColor = StudioColors.AppBackground, MinimizeBox = false, MaximizeBox = false,
        };
        var list = new ListBox { Dock = DockStyle.Fill, DisplayMember = nameof(WorkspaceFilterPreset.Name) };
        list.Items.AddRange(presets.Cast<object>().ToArray());
        var apply = new Button { Text = "Apply", DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 90 };
        var delete = new Button { Text = "Delete", Dock = DockStyle.Right, Width = 90 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Dock = DockStyle.Right, Width = 90 };
        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 38 };
        buttons.Controls.Add(cancel); buttons.Controls.Add(delete); buttons.Controls.Add(apply);
        delete.Click += (_, _) =>
        {
            if (list.SelectedItem is not WorkspaceFilterPreset selected) return;
            WorkspacePresetService.Delete(selected.Name);
            list.Items.Remove(selected);
        };
        dialog.Controls.Add(list); dialog.Controls.Add(buttons);
        Theme.ApplyControlTree(dialog);
        dialog.AcceptButton = apply; dialog.CancelButton = cancel;
        if (dialog.ShowDialog(this) != DialogResult.OK || list.SelectedItem is not WorkspaceFilterPreset preset) return;
        var tableIndex = _ordered.FindIndex(table => table.Name.Equals(preset.TableName, StringComparison.OrdinalIgnoreCase));
        if (tableIndex < 0) return;
        GoToRecord(tableIndex);
        _filterColumn.SelectedItem = _filterColumn.Items.Contains(preset.FieldName) ? preset.FieldName : "All fields";
        _recordSearch.Text = preset.Expression;
        ApplyRecordFilter();
    }

    private void ShowAssetUsage()
    {
        if (!PromptForText("Asset dependency", "Asset type (face, crest, league, competition, stadium, ball, boot, glove, flag, manager or kit):", "crest", out var type) ||
            !PromptForText("Asset dependency", "Numeric asset ID:", string.Empty, out var idText) ||
            !int.TryParse(idText, out var assetId)) return;
        var hits = AssetDependencyService.Find(Services.Session, type, assetId);
        var text = hits.Count == 0
            ? $"No known {type} database reference uses ID {assetId}."
            : string.Join(Environment.NewLine, hits.Select(hit =>
                $"{hit.AssetType} {hit.AssetId}: {hit.TableName}[{hit.RowIndex}].{hit.FieldName}"));
        ShowTextDialog("Asset dependency report", text);
    }
}
