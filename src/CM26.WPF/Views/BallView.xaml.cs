using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style BallForm: Find + ball list on the left, Id / Name / checkboxes
/// on the right matching the layout of Creation Master 16 BallForm.
/// </summary>
public partial class BallView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public BallView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("teamballs");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        BallList.ItemsSource = items;
        CountText.Text = $"{items.Count} balls" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void BallList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BallList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("teamballs", item.RecordIndex, LabelMaps.Balls);

        InfoFields.ItemsSource = fields.Where(f => IsInfo(f.FieldName));
        AvailabilityFields.ItemsSource = fields.Where(f => !IsInfo(f.FieldName));
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (BallList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("teamballs", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (BallList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private static bool IsInfo(string n) => n is "ballid" or "balltype" or "name" or "assetid";
}