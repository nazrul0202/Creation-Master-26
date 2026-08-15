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
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetCountries();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        CountryList.ItemsSource = items;
        CountText.Text = $"{items.Count} countries" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

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

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (CountryList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("nations", item.RecordIndex, LabelMaps.Nations);
        var idField = f.FirstOrDefault(x => x.FieldName == "nationid");
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("nations", item.RecordIndex, "nationid", idField.Value);
            if (outcome.Success) ReloadEditor();
        }
    }
}