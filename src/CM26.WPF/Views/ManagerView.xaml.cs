using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style ManagerForm: Find + manager list on the left, identity / body /
/// dress / generic face groups on the right, matching the group layout of
/// Creation Master 16 ManagerForm.
/// </summary>
public partial class ManagerView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public ManagerView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("manager");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        ManagerList.ItemsSource = items;
        CountText.Text = $"{items.Count} managers" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void ManagerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ManagerList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("manager", item.RecordIndex, LabelMaps.Managers);

        IdentityFields.ItemsSource = fields.Where(f => IsIdentity(f.FieldName));
        BodyFields.ItemsSource = fields.Where(f => IsBody(f.FieldName));
        DressFields.ItemsSource = fields.Where(f => IsDress(f.FieldName));
        HeadFields.ItemsSource = fields.Where(f => IsHead(f.FieldName));
        HairFields.ItemsSource = fields.Where(f => IsHair(f.FieldName));
        TextureFields.ItemsSource = fields.Where(f => IsTexture(f.FieldName));
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (ManagerList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("manager", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (ManagerList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (ManagerList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("manager", item.RecordIndex, LabelMaps.Managers);
        var idField = f.FirstOrDefault(x => x.FieldName == "managerid");
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("manager", item.RecordIndex, "managerid", idField.Value);
            if (outcome.Success) RefreshEditor();
        }
    }

    // ---------- CM16 groupings ----------

    private static bool IsIdentity(string n) => n is "managerid" or "firstname" or "surname" or "commonname"
        or "birthdate" or "nationality" or "gender" or "islicensed" or "starrating" or "teamid"
        or "managerjointeamdate" or "isrewardable" or "ethnicity";
    private static bool IsBody(string n) => n is "height" or "weight" or "bodytypecode";
    private static bool IsDress(string n) => n.Contains("outfit", StringComparison.OrdinalIgnoreCase)
        || n.Contains("accessor", StringComparison.OrdinalIgnoreCase);
    private static bool IsHead(string n) => n is "headtypecode" or "headvariation" or "headclasscode"
        or "headassetid" or "faceposerpreset" or "hashighqualityhead";
    private static bool IsHair(string n) => n.Contains("hair", StringComparison.OrdinalIgnoreCase);
    private static bool IsTexture(string n) => n.Contains("skin", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eye", StringComparison.OrdinalIgnoreCase)
        || n.Contains("brow", StringComparison.OrdinalIgnoreCase)
        || n.Contains("face", StringComparison.OrdinalIgnoreCase)
        || n.Contains("lip", StringComparison.OrdinalIgnoreCase)
        || n.Contains("makeup", StringComparison.OrdinalIgnoreCase)
        || n.Contains("complexion", StringComparison.OrdinalIgnoreCase)
        || n.Contains("trait", StringComparison.OrdinalIgnoreCase)
        || n.Contains("personality", StringComparison.OrdinalIgnoreCase);
}