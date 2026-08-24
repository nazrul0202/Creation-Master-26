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
    private List<DbTable> _ordered = new();
    private DbTable? _activeTable;
    private bool _binding;
    private int _pageStart;
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
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(5, 4, 5, 2),
            BackColor = StudioColors.Surface,
        };
        AddAction("Copy", CopySelection);
        AddAction("Paste", PasteSelection);
        AddAction("Export table", ExportTable);
        AddAction("Export all", ExportAllTables);
        AddAction("Import table", ImportTable);
        AddAction("Dependencies", ShowDependencies);
        AddAction("Pending changes", ShowPendingChanges);
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

            int rows = Math.Min(table.RowCount - _pageStart, PageSize);
            for (int offset = 0; offset < rows; offset++)
            {
                int r = _pageStart + offset;
                var rec = Services.Session.GetRecord(table.Name, r);
                if (rec == null) continue;
                var row = _grid.Rows.Add(rec.Values.Cast<object>().ToArray());
                _grid.Rows[row].Tag = r;
            }
            _previousPage.Enabled = _pageStart > 0;
            _nextPage.Enabled = _pageStart + rows < table.RowCount;
            _duplicateRow.Enabled = _grid.Rows.Count > 0;
            _deleteRow.Enabled = _grid.Rows.Count > 0;
            _info.Text = table.RowCount == 0
                ? "0 rows. Editable cells are staged, validated, and saved with Ctrl+S."
                : $"Rows {_pageStart + 1:N0}-{_pageStart + rows:N0} of {table.RowCount:N0}. Editable cells are staged, validated, and saved with Ctrl+S.";
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
        if (next < 0 || next >= _activeTable.RowCount) return;
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
            MessageBox.Show(this, $"Nothing was pasted. {ex.Message}", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Export table", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Export all tables", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Import table", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
}
