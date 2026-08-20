using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;
using CM26.Studio.Controls;

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
    private bool _loadingPickers;

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public ManagerView(ViewModel vm)
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
        _all = _vm.Session.Sections.GetManagers();
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
        ManagerList.ItemsSource = items;
        CountText.Text = $"{items.Count} managers" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

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
        HeadFields.ItemsSource = fields.Where(f => IsHead(f.FieldName) && !IsNamedModel(f.FieldName));
        HairFields.ItemsSource = fields.Where(f => IsHair(f.FieldName) && !IsNamedModel(f.FieldName));
        TextureFields.ItemsSource = fields.Where(f => IsTexture(f.FieldName));
        FillModels(fields);
        FillManagerPickers(fields);
    }

    private sealed record PickerOption(string Display, string Value)
    {
        public override string ToString() => Display;
    }

    private void FillManagerPickers(IReadOnlyList<FieldValue> fields)
    {
        _loadingPickers = true;
        try
        {
            FillPicker(NationPicker, "nationality", fields, () =>
            {
                var items = new List<PickerOption> { new("Not set", "-1"), new("No nation", "0") };
                foreach (var nation in _vm.Session.Sections.GetCountries())
                {
                    var raw = _vm.Session.Database.GetCell("nations", nation.RecordIndex, "nationid");
                    if (int.TryParse(raw, out var id) && id > 0 && !string.IsNullOrWhiteSpace(nation.Title))
                        items.Add(new PickerOption(nation.Title, raw));
                }
                return items;
            });
            FillPicker(TeamPicker, "teamid", fields, () =>
            {
                var items = new List<PickerOption> { new("No team", "0") };
                foreach (var team in _vm.Session.Sections.GetTeams())
                {
                    var raw = _vm.Session.Database.GetCell("teams", team.RecordIndex, "teamid");
                    if (int.TryParse(raw, out var id) && id > 0 && !string.IsNullOrWhiteSpace(team.Title))
                        items.Add(new PickerOption(team.Title, raw));
                }
                return items;
            });
        }
        finally { _loadingPickers = false; }
    }

    private void FillPicker(ComboBox picker, string fieldName, IReadOnlyList<FieldValue> fields, Func<List<PickerOption>> build)
    {
        var field = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        if (field == null) { picker.Visibility = Visibility.Collapsed; return; }
        picker.Visibility = Visibility.Visible;
        picker.IsEnabled = field.IsWritable;
        if (picker.Items.Count == 0)
        {
            foreach (var option in build()
                         .DistinctBy(o => o.Value, StringComparer.OrdinalIgnoreCase)
                         .OrderBy(o => o.Display, StringComparer.OrdinalIgnoreCase))
                picker.Items.Add(option);
        }
        var selected = 0;
        for (var i = 0; i < picker.Items.Count; i++)
        {
            if (picker.Items[i] is PickerOption option && option.Value.Equals(field.RawValue, StringComparison.OrdinalIgnoreCase))
            {
                selected = i;
                break;
            }
        }
        picker.SelectedIndex = selected;
    }

    private void NationPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        StagePickerEdit(NationPicker, "nationality");

    private void TeamPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        StagePickerEdit(TeamPicker, "teamid");

    private void StagePickerEdit(ComboBox picker, string fieldName)
    {
        if (_loadingPickers || ManagerList.SelectedItem is not RecordListItem item) return;
        if (picker.SelectedItem is not PickerOption option) return;
        var current = _vm.Session.Database.GetCell("manager", item.RecordIndex, fieldName);
        if (option.Value.Equals(current, StringComparison.OrdinalIgnoreCase)) return;
        var outcome = _vm.Session.Pending.Stage("manager", item.RecordIndex, fieldName, option.Value);
        if (outcome.Success) RefreshEditor();
        else MessageBox.Show(outcome.Message, "Edit rejected", MessageBoxButton.OK, MessageBoxImage.Warning);
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

    private void FillModels(IReadOnlyList<FieldValue> fields)
    {
        FillModel(ComboHeadModel, "headtypecode", "Head Model",
            AppearanceCatalog.HeadModelSets.Select(set => (set.Name, set.Models)), fields);
        FillModel(ComboHairModel, "hairtypecode", "Hair Model",
            AppearanceCatalog.HairModelSets.Select(set => (set.Name, set.Models)), fields);
    }

    private void FillModel(GroupedModelPicker picker, string fieldName, string label,
        IEnumerable<(string Name, int[] Values)> groups, IReadOnlyList<FieldValue> fields)
    {
        var field = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        picker.Visibility = field == null ? Visibility.Collapsed : Visibility.Visible;
        if (field != null)
            picker.SetContent(label, field.FieldName, groups, field.RawValue, field.IsWritable, StageEdit);
    }


    // ---------- CM16 groupings ----------

    private static bool IsIdentity(string n) => n is "managerid" or "firstname" or "surname" or "commonname"
        or "birthdate" or "gender" or "islicensed" or "starrating"
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

    private static bool IsNamedModel(string n) => n.Equals("headtypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("hairtypecode", StringComparison.OrdinalIgnoreCase);
}
