using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>CM16-style CountryForm: Find + nation list on the left, field editor on the right.</summary>
public partial class CountryView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public CountryView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        PickUp.SelectObject += LoadEditorFromPickUp;
        PickUp.FilterByList = new[] { "All", "by Name", "by Confederation" };
        PickUp.FilterChanged += ApplyFilter;
        Loaded += (_, _) => LoadList();
    }

    private void LoadEditorFromPickUp(RecordListItem item) => LoadEditor(item);

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetCountries();
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
                "by Confederation" => _all.Where(x => x.Subtitle.Contains(q, StringComparison.OrdinalIgnoreCase)),
                _ => _all.Where(x => (x.Title + " " + x.Subtitle + " " + x.Detail).Contains(q, StringComparison.OrdinalIgnoreCase)),
            };
        }
        var items = source.ToList();
        CountryList.ItemsSource = items;
        CountText.Text = $"{items.Count} countries" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void CountryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CountryList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("nations", item.RecordIndex, LabelMaps.Nations);
        InfoFields.ItemsSource = fields;
        AudioFields.ItemsSource = fields.Where(f => IsAudio(f.FieldName));
        TeamFields.ItemsSource = fields.Where(f => IsTeam(f.FieldName));
    }

    private static bool IsAudio(string n) => n.Contains("audio", StringComparison.OrdinalIgnoreCase)
        || n.Contains("chant", StringComparison.OrdinalIgnoreCase)
        || n.Contains("whistle", StringComparison.OrdinalIgnoreCase)
        || n.Contains("heckle", StringComparison.OrdinalIgnoreCase)
        || n.Contains("reaction", StringComparison.OrdinalIgnoreCase)
        || n.Contains("taunt", StringComparison.OrdinalIgnoreCase)
        || n.Contains("ambience", StringComparison.OrdinalIgnoreCase)
        || n.Contains("call", StringComparison.OrdinalIgnoreCase)
        || n.Contains("crowdtype", StringComparison.OrdinalIgnoreCase);
    private static bool IsTeam(string n) => n.Contains("target", StringComparison.OrdinalIgnoreCase)
        || n.Contains("worldcup", StringComparison.OrdinalIgnoreCase)
        || n.Contains("regional", StringComparison.OrdinalIgnoreCase);

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (CountryList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("nations", item.RecordIndex, fieldName, value);
        if (outcome.Success) ReloadEditor();
        return outcome;
    }

    private void ReloadEditor()
    {
        if (CountryList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

}