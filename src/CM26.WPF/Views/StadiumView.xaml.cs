using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style StadiumForm: Find + stadium list on the left, edit tab on the
/// right matching the exact group layout of Creation Master 16 StadiumForm
/// (pageStadiumGeneral: Info / Mowing Pattern / Net / Adboards / Camera /
/// Time and Weather / Police / Tech Zone Home / Tech Zone Away).
/// </summary>
public partial class StadiumView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public StadiumView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("stadiums");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        StadiumList.ItemsSource = items;
        CountText.Text = $"{items.Count} stadiums" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void StadiumList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StadiumList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("stadiums", item.RecordIndex, LabelMaps.Stadiums);

        InfoFields.ItemsSource = fields.Where(f => IsInfo(f.FieldName));
        MowingFields.ItemsSource = fields.Where(f => IsMowing(f.FieldName));
        NetFields.ItemsSource = fields.Where(f => IsNet(f.FieldName));
        AdboardFields.ItemsSource = fields.Where(f => IsAdboards(f.FieldName));
        CameraFields.ItemsSource = fields.Where(f => IsCamera(f.FieldName));
        WeatherFields.ItemsSource = fields.Where(f => IsWeather(f.FieldName));
        PoliceFields.ItemsSource = fields.Where(f => IsPolice(f.FieldName));
        TechHomeFields.ItemsSource = fields.Where(f => IsTechHome(f.FieldName));
        TechAwayFields.ItemsSource = fields.Where(f => IsTechAway(f.FieldName));

        EditTabs.SelectedIndex = 0;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (StadiumList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("stadiums", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (StadiumList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (StadiumList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("stadiums", item.RecordIndex, LabelMaps.Stadiums);
        var idField = f.FirstOrDefault(x => x.FieldName == "stadiumid");
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("stadiums", item.RecordIndex, "stadiumid", idField.Value);
            if (outcome.Success) RefreshEditor();
        }
    }

    // ---------- CM16 General page groupings ----------

    private static bool IsInfo(string n) => n is "stadiumid" or "name" or "capacity" or "seatcolor"
        or "stadiumtype" or "islicensed" or "sectionfacedbydefault" or "hometeamid" or "cityid"
        or "countrycode" or "languageregion" or "defaultseason" or "dlc";
    private static bool IsMowing(string n) => n.Contains("mow", StringComparison.OrdinalIgnoreCase)
        || n.Contains("pitch", StringComparison.OrdinalIgnoreCase);
    private static bool IsNet(string n) => n.Contains("net", StringComparison.OrdinalIgnoreCase)
        || n.Contains("goalpost", StringComparison.OrdinalIgnoreCase)
        || n.Contains("goalnet", StringComparison.OrdinalIgnoreCase);
    private static bool IsAdboards(string n) => n.Contains("adboard", StringComparison.OrdinalIgnoreCase)
        || n.Contains("inflatables", StringComparison.OrdinalIgnoreCase)
        || n.Contains("tarp", StringComparison.OrdinalIgnoreCase);
    private static bool IsCamera(string n) => n.Contains("camera", StringComparison.OrdinalIgnoreCase)
        || n.Contains("drone", StringComparison.OrdinalIgnoreCase)
        || n.Contains("replay", StringComparison.OrdinalIgnoreCase)
        || n.Contains("rotation", StringComparison.OrdinalIgnoreCase);
    private static bool IsWeather(string n) => n.Contains("weather", StringComparison.OrdinalIgnoreCase)
        || n.Contains("timeofday", StringComparison.OrdinalIgnoreCase)
        || n.Contains("mexicanwave", StringComparison.OrdinalIgnoreCase);
    private static bool IsPolice(string n) => n.Contains("police", StringComparison.OrdinalIgnoreCase)
        || n.Contains("crowd", StringComparison.OrdinalIgnoreCase);
    private static bool IsTechHome(string n) => n.Contains("hometechzone", StringComparison.OrdinalIgnoreCase);
    private static bool IsTechAway(string n) => n.Contains("awaytechzone", StringComparison.OrdinalIgnoreCase);
}