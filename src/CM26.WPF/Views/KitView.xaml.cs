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
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("teamkits");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        KitList.ItemsSource = items;
        CountText.Text = $"{items.Count} kits" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void KitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KitList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("teamkits", item.RecordIndex, LabelMaps.Kits);

        IdentityFields.ItemsSource = fields.Where(f => IsIdentity(f.FieldName));
        ColorFields.ItemsSource = fields.Where(f => IsColor(f.FieldName));
        JerseyFields.ItemsSource = fields.Where(f => IsJersey(f.FieldName));
        ShortsFields.ItemsSource = fields.Where(f => IsShorts(f.FieldName));
        NameFields.ItemsSource = fields.Where(f => IsName(f.FieldName));
        NumberFields.ItemsSource = fields.Where(f => IsNumber(f.FieldName));
        SockFields.ItemsSource = fields.Where(f => IsSocks(f.FieldName));
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

    // ---------- CM16 KitForm groupings ----------

    private static bool IsIdentity(string n) => n is "teamkitid" or "teamtechid" or "teamkittypetechid"
        or "isgeneric" or "islocked" or "isembargoed" or "year" or "dlc" or "powid"
        or "teamid" or "captainarmband" or "armbandtype" or "jerseyrestriction";
    private static bool IsColor(string n) => n.Contains("color", StringComparison.OrdinalIgnoreCase)
        || n.Contains("teamcolor", StringComparison.OrdinalIgnoreCase)
        || n.Contains("percent", StringComparison.OrdinalIgnoreCase);
    private static bool IsJersey(string n) => n.Contains("jersey", StringComparison.OrdinalIgnoreCase)
        || n.Contains("chestbadge", StringComparison.OrdinalIgnoreCase)
        || n.Contains("sleevebadge", StringComparison.OrdinalIgnoreCase)
        || n.Contains("rendering", StringComparison.OrdinalIgnoreCase)
        || n.Contains("template", StringComparison.OrdinalIgnoreCase)
        || n.Contains("shapestyle", StringComparison.OrdinalIgnoreCase)
        || n.Contains("fit", StringComparison.OrdinalIgnoreCase)
        || n.Contains("advertising", StringComparison.OrdinalIgnoreCase);
    private static bool IsShorts(string n) => n.Contains("short", StringComparison.OrdinalIgnoreCase)
        && !n.Contains("number", StringComparison.OrdinalIgnoreCase);
    private static bool IsName(string n) => n.Contains("name", StringComparison.OrdinalIgnoreCase)
        && !n.Contains("number", StringComparison.OrdinalIgnoreCase);
    private static bool IsNumber(string n) => n.Contains("number", StringComparison.OrdinalIgnoreCase);
    private static bool IsSocks(string n) => n.Contains("sock", StringComparison.OrdinalIgnoreCase);
}