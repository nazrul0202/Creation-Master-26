using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style ShoesForm: Find + shoes list on the left, shoes / availability
/// groups on the right matching the layout of Creation Master 16 ShoesForm.
/// </summary>
public partial class ShoesView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public ShoesView(ViewModel vm)
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
        _all = _vm.Session.Sections.GetItems("playerboots");
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
        ShoesList.ItemsSource = items;
        CountText.Text = $"{items.Count} shoes" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void ShoesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ShoesList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("playerboots", item.RecordIndex, LabelMaps.Shoes);

        ShoesFields.ItemsSource = fields.Where(f => IsShoes(f.FieldName));
        AvailabilityFields.ItemsSource = fields.Where(f => !IsShoes(f.FieldName));
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (ShoesList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("playerboots", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (ShoesList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private static bool IsShoes(string n) => n is "shoetype" or "shoedesign" or "shoecolor1" or "shoecolor2"
        or "manufacturerid" or "isadidas" or "ishighboot" or "issmallsided" or "islegacy";
}