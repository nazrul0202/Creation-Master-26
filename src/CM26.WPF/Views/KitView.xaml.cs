using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style KitForm: Find + kit list on the left, edit groups on the right
/// (Identity / Colors / Jersey / Shorts / Name / Numbers / Socks) matching the
/// group layout of Creation Master 16 KitForm.
/// </summary>
public partial class KitView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public KitView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        PickUp.SelectObject += LoadEditorFromPickUp;
        PickUp.FilterByList = new[] { "All", "by Name", "by Id" };
        PickUp.FilterChanged += ApplyFilter;
        Loaded += (_, _) => LoadList();
    }

    private void LoadEditorFromPickUp(RecordListItem item) => LoadEditor(item);

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetKits();
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
        KitList.ItemsSource = items;
        CountText.Text = $"{items.Count} kits" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void KitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KitList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("teamkits", item.RecordIndex, LabelMaps.Kits);

        PositionFields.ItemsSource = fields.Where(f => IsPosition(f.FieldName));
        JerseyFields.ItemsSource = fields.Where(f => !IsPosition(f.FieldName) && !IsName(f.FieldName));
        NameFields.ItemsSource = fields.Where(f => IsName(f.FieldName));
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (KitList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("teamkits", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (KitList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    // ---------- CM16 KitForm groupings (Positions / 3D Model / Jersey / Name) ----------

    private static bool IsPosition(string n) => n.Contains("placementcode", StringComparison.OrdinalIgnoreCase)
        || n.Contains("positions", StringComparison.OrdinalIgnoreCase)
        || n.Contains("namelayouttype", StringComparison.OrdinalIgnoreCase)
        || n.Contains("numberfonttype", StringComparison.OrdinalIgnoreCase)
        || n.Contains("backnamefontcase", StringComparison.OrdinalIgnoreCase)
        || n.Contains("hasbackname", StringComparison.OrdinalIgnoreCase);
    private static bool IsName(string n) => n.Contains("name", StringComparison.OrdinalIgnoreCase);
}
