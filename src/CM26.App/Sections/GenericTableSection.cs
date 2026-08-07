using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>
/// Generic single-table section: browser + tabbed field editor built from the record's
/// schema, with resolved relationship names in the header and friendly field labels.
/// Used by most entity sections; specialised sections override behaviour.
/// </summary>
public class GenericTableSection : SectionBase
{
    private readonly Func<CM26.Application.Services.SectionDataService, IReadOnlyList<RecordListItem>> _listProvider;
    private readonly Dictionary<string, string> _labelMap;
    private readonly Func<string, string, string>? _valueFormatter;
    private readonly Func<int, (string title, string subtitle)> _headerProvider;
    private readonly Dictionary<string, string[]> _tabGroups; // tab -> field prefixes/names
    private readonly FieldEditorGrid[] _grids;
    private readonly Func<int, (string path, string caption)>? _previewProvider;
    private readonly bool _hasPreview;

    public override string SectionKey { get; }
    public override string SectionTitle { get; }
    protected override string TableName { get; }
    protected override bool HasPreview => _hasPreview;

    public GenericTableSection(
        AppServices services,
        string key,
        string title,
        string tableName,
        Func<CM26.Application.Services.SectionDataService, IReadOnlyList<RecordListItem>> listProvider,
        Dictionary<string, string> labelMap,
        Dictionary<string, string[]> tabGroups,
        Func<int, (string, string)> headerProvider,
        Func<string, string, string>? valueFormatter = null,
        Func<int, (string path, string caption)>? previewProvider = null)
        : base(services)
    {
        SectionKey = key;
        SectionTitle = title;
        TableName = tableName;
        _listProvider = listProvider;
        _labelMap = labelMap;
        _tabGroups = tabGroups;
        _headerProvider = headerProvider;
        _valueFormatter = valueFormatter;
        _previewProvider = previewProvider;
        _hasPreview = previewProvider != null;

        _grids = new FieldEditorGrid[_tabGroups.Count];
        int gi = 0;
        foreach (var kv in _tabGroups)
        {
            var grid = new FieldEditorGrid();
            grid.FieldEdited += (_, e) => OnFieldEdited(grid, e.field, e.value);
            // The original CM16 forms place controls inside labelled group boxes
            // on a white canvas.  Retain the schema-driven editor, but present it
            // through that legacy form vocabulary instead of a modern property page.
            var canvas = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background, Padding = new Padding(8) };
            var group = new ModernGroupBox
            {
                Text = kv.Key,
                // CM16 keeps the active editor compact at the top-left and
                // leaves the rest of the document surface available for asset
                // previews / future FC26-only controls.
                Location = new Point(8, 8),
                Size = new Size(620, 430),
                Padding = new Padding(6, 18, 6, 6),
            };
            grid.Dock = DockStyle.Fill;
            group.Controls.Add(grid);
            canvas.Controls.Add(group);
            Tabs.TabPages.Add(MakeTab(kv.Key, canvas));
            _grids[gi++] = grid;
        }
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => _listProvider(Services.RequireData());

    private void OnFieldEdited(object? sender, string field, string value)
    {
        if (CurrentRecordIndex < 0) return;
        // Mark the modified state on the grid that actually owns the field;
        // staging through the first grid would leave the marker off the editor
        // the user is looking at.
        var owner = sender as FieldEditorGrid ?? _grids[0];
        StageField(TableName, CurrentRecordIndex, field, value, owner);
    }

    protected override void ShowRecord(int recordIndex)
    {
        try
        {
            var (title, subtitle) = _headerProvider(recordIndex);
            Header.SetRecord(title, subtitle, IconService.Get(SectionKey, 44));

            // Asset preview (real file when present, honest state otherwise).
            if (_previewProvider != null)
            {
                var (path, caption) = _previewProvider(recordIndex);
                ShowPreview(path, string.IsNullOrEmpty(path) ? null : caption);
            }

            var fields = Services.RequireData().GetFields(TableName, recordIndex, _labelMap, _valueFormatter);

            int gi = 0;
            foreach (var kv in _tabGroups)
            {
                var wanted = new HashSet<string>(kv.Value, StringComparer.OrdinalIgnoreCase);
                var subset = wanted.Count == 0
                    ? fields
                    : fields.Where(f => wanted.Contains(f.FieldName)).ToList();
                _grids[gi].SetFields(subset, ToolTip);
                gi++;
            }
        }
        catch (Exception ex)
        {
            Header.Clear("Record unavailable");
            Header.SetRecord(SectionTitle, "This record could not be loaded.", IconService.Get(SectionKey, 44));
            MessageBox.Show(this, ex.Message, SectionTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
