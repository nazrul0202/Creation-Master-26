using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style Sponsor section: Find + sponsor list on the left, sponsor fields
/// on the right. The Sponsor button exists in CM16 (hidden) and edits
/// adboard sponsors.
/// </summary>
public partial class SponsorView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public SponsorView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("sponsors");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        SponsorList.ItemsSource = items;
        CountText.Text = $"{items.Count} sponsors" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void SponsorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SponsorList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("sponsors", item.RecordIndex, LabelMaps.Sponsors);
        SponsorFields.ItemsSource = fields;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (SponsorList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("sponsors", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (SponsorList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }
}