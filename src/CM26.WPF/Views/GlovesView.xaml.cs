using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style GlovesForm: Find + gloves list on the left, info fields on the
/// right matching the layout of Creation Master 16 GlovesForm.
/// </summary>
public partial class GlovesView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public GlovesView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("goalkeepergloves");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        GlovesList.ItemsSource = items;
        CountText.Text = $"{items.Count} gloves" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void GlovesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GlovesList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("goalkeepergloves", item.RecordIndex, LabelMaps.Gloves);
        InfoFields.ItemsSource = fields;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (GlovesList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("goalkeepergloves", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (GlovesList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }
}