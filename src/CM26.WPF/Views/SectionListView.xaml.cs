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
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems(_table);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        RecordList.ItemsSource = items;
        CountText.Text = $"{items.Count} records" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

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

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (RecordList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields(_table, item.RecordIndex);
        var idField = f.FirstOrDefault(x => x.FieldName.Equals(_idColumn, StringComparison.OrdinalIgnoreCase));
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage(_table, item.RecordIndex, _idColumn, idField.Value);
            if (outcome.Success) ReloadEditor();
        }
    }
}