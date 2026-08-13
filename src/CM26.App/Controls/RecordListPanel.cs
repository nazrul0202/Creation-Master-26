using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Controls;

/// <summary>
/// Left browser: search bar + paged/sorted record list + prev/next + refresh + count.
/// Sections feed it RecordListItem pages; it raises SelectionChanged with the record index.
/// </summary>
public sealed class RecordListPanel : UserControl
{
    private const int PageSize = 300;

    private readonly SearchBar _search;
    private readonly DataGridView _grid;
    private readonly Button _prev, _next, _refresh;
    private readonly Label _page;

    private List<RecordListItem> _all = new();
    private List<RecordListItem> _filtered = new();
    private int _pageIndex;
    private bool _suspend;

    public event EventHandler<int>? SelectionChanged;   // record index
    public event EventHandler? RefreshRequested;

    public RecordListPanel()
    {
        BackColor = Theme.Panel;

        _search = new SearchBar { Dock = DockStyle.Top };
        _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, VirtualMode = false };
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Title", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sub", HeaderText = "", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Det", HeaderText = "", Width = 92 });
        Theme.ApplyGrid(_grid);

        var footer = new BufferedPanel { Dock = DockStyle.Bottom, Height = Theme.ControlHeight + 12, Padding = new Padding(Theme.Space, 6, Theme.Space, 4) };
        _prev = MakeBtn("◀ Prev"); _next = MakeBtn("Next ▶"); _refresh = MakeBtn("⟳");
        _page = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Theme.Muted, Font = Theme.Muted9 };
        _prev.Dock = DockStyle.Left; _prev.Width = 74;
        _next.Dock = DockStyle.Right; _next.Width = 74;
        _refresh.Dock = DockStyle.Right; _refresh.Width = 36;
        footer.Controls.Add(_page);
        footer.Controls.Add(_refresh);
        footer.Controls.Add(_next);
        footer.Controls.Add(_prev);

        Controls.Add(_grid);
        Controls.Add(_search);
        Controls.Add(footer);

        _search.SearchChanged += (_, _) => ApplyFilter();
        _prev.Click += (_, _) => { if (_pageIndex > 0) { _pageIndex--; BindPage(); } };
        _next.Click += (_, _) => { if ((_pageIndex + 1) * PageSize < _filtered.Count) { _pageIndex++; BindPage(); } };
        _refresh.Click += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _grid.SelectionChanged += (_, _) => EmitSelection();
        _grid.CellDoubleClick += (_, _) => EmitSelection();
    }

    private static Button MakeBtn(string text) { var b = new Button { Text = text }; Theme.ApplyButton(b); return b; }

    public void FocusSearch() => _search.FocusBox();

    public void SetItems(IEnumerable<RecordListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _all = items.ToList();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        _filtered = _all.Where(i => i.Matches(_search.Query)).ToList();
        _pageIndex = 0;
        _search.SetCount(_filtered.Count, _all.Count);
        BindPage();
    }

    private void BindPage()
    {
        _suspend = true;
        _grid.SuspendLayout();
        _grid.Rows.Clear();
        var pageItems = _filtered.Skip(_pageIndex * PageSize).Take(PageSize).ToList();
        foreach (var it in pageItems)
        {
            var row = new DataGridViewRow();
            row.CreateCells(_grid, it.Title, it.Subtitle, it.Detail);
            row.Tag = it;
            _grid.Rows.Add(row);
        }
        int totalPages = Math.Max(1, (int)Math.Ceiling(_filtered.Count / (double)PageSize));
        _page.Text = $"Page {_pageIndex + 1} / {totalPages}";
        _prev.Enabled = _pageIndex > 0;
        _next.Enabled = (_pageIndex + 1) * PageSize < _filtered.Count;
        _grid.ResumeLayout();
        _suspend = false;
        if (_grid.Rows.Count > 0) _grid.Rows[0].Selected = true;
        else SelectionChanged?.Invoke(this, -1);
    }

    private void EmitSelection()
    {
        if (_suspend) return;
        if (_grid.SelectedRows.Count == 0) { SelectionChanged?.Invoke(this, -1); return; }
        if (_grid.SelectedRows[0].Tag is RecordListItem item)
            SelectionChanged?.Invoke(this, item.RecordIndex);
    }

    public void SelectRecord(int recordIndex)
    {
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.Tag is RecordListItem it && it.RecordIndex == recordIndex)
            {
                row.Selected = true;
                try { _grid.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index - 4); }
                catch (InvalidOperationException ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Record scroll failed: {ex.Message}"); }
                break;
            }
        }
    }
}
