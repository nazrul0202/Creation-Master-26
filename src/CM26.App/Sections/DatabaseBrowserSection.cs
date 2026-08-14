using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

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
}
