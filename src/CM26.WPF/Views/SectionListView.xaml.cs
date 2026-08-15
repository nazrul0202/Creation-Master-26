using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style generic section: Find box + record list on the left, field
/// editor on the right. Sections with a table map here (nations, leagues,
/// teams, manager, stadiums, referee, formations, teamkits).
/// </summary>
public partial class SectionListView : UserControl
{
    private readonly ViewModel _vm;
    private readonly string _table;
    private readonly string _sectionKey;
    private readonly string _idColumn;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    /// <summary>Wired to each FieldRow so field edits stage through the pending service.</summary>
    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    private static readonly Dictionary<string, string> TableByKey = new()
    {
        ["countries"] = "nations",
        ["leagues"] = "leagues",
        ["teams"] = "teams",
        ["managers"] = "manager",
        ["stadiums"] = "stadiums",
        ["referees"] = "referee",
        ["formations"] = "formations",
        ["kits"] = "teamkits",
    };

    private static readonly Dictionary<string, string> IdColumnByKey = new()
    {
        ["countries"] = "nationid",
        ["leagues"] = "leagueid",
        ["teams"] = "teamid",
        ["managers"] = "managerid",
        ["stadiums"] = "stadiumid",
        ["referees"] = "refereeid",
        ["formations"] = "formationid",
        ["kits"] = "teamkitid",
    };

    private static readonly Dictionary<string, Dictionary<string, string>> LabelMapByKey = new()
    {
        ["countries"] = LabelMaps.Nations,
        ["leagues"] = LabelMaps.Leagues,
        ["teams"] = LabelMaps.Teams,
        ["managers"] = LabelMaps.Managers,
        ["stadiums"] = LabelMaps.Stadiums,
        ["referees"] = LabelMaps.Referees,
        ["formations"] = LabelMaps.Formations,
        ["kits"] = LabelMaps.Kits,
    };

    private static Dictionary<string, string> LabelMapFor(string key)
        => LabelMapByKey.TryGetValue(key, out var m) ? m : new Dictionary<string, string>();

    private void LoadEditor(RecordListItem item)
        => InfoFields.ItemsSource = _vm.Session.Sections.GetFields(_table, item.RecordIndex, LabelMapFor(_sectionKey));

    public SectionListView(ViewModel vm, string sectionKey)
    {
        InitializeComponent();
        _vm = vm;
        _sectionKey = sectionKey;
        _table = TableByKey[sectionKey];
        _idColumn = IdColumnByKey[sectionKey];
        StageEditDelegate = StageEdit;
        PickUp.SelectObject += LoadEditorFromPickUp;
        PickUp.FilterByList = new[] { "All", "by Name", "by Id" };
        PickUp.FilterChanged += ApplyFilter;
        Loaded += (_, _) => LoadList();
    }

    private void LoadEditorFromPickUp(RecordListItem item) => LoadEditor(item);

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems(_table);
        PickUp.ObjectList = _all;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = PickUp.FilterValueText;
        var by = PickUp.FilterByComboText;
        IEnumerable<RecordListItem> source = _all;
        if (!string.IsNullOrWhiteSpace(q))
        {
            source = by switch
            {
                "by Name" => _all.Where(x => x.Title.Contains(q, StringComparison.OrdinalIgnoreCase)),
                "by Id" => _all.Where(x => x.Detail.Contains(q, StringComparison.OrdinalIgnoreCase)),
                _ => _all.Where(x => (x.Title + " " + x.Subtitle + " " + x.Detail).Contains(q, StringComparison.OrdinalIgnoreCase)),
            };
        }
        var items = source.ToList();
        RecordList.ItemsSource = items;
        CountText.Text = $"{items.Count} records" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void RecordList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RecordList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (RecordList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage(_table, item.RecordIndex, fieldName, value);
        if (outcome.Success) ReloadEditor();
        return outcome;
    }

    private void ReloadEditor()
    {
        if (RecordList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

}