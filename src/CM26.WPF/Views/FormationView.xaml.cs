using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style FormationForm: Find + formation list on the left, edit tab on
/// the right with the exact group layout of Creation Master 16 FormationForm
/// (groupTactic "Roles", groupInstructions "Instructions" with position
/// offsets and role assignments).
/// </summary>
public partial class FormationView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public FormationView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("formations");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        FormationList.ItemsSource = items;
        CountText.Text = $"{items.Count} formations" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void FormationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FormationList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("formations", item.RecordIndex, LabelMaps.Formations);

        NameFields.ItemsSource = fields.Where(f => IsName(f.FieldName));
        RoleFields.ItemsSource = fields.Where(f => IsRole(f.FieldName));
        InstructionFields.ItemsSource = fields.Where(f => IsInstruction(f.FieldName));
        PositionFields.ItemsSource = fields.Where(f => IsPosition(f.FieldName));
        OffsetFields.ItemsSource = fields.Where(f => IsOffset(f.FieldName));
        StructureFields.ItemsSource = fields.Where(f => IsStructure(f.FieldName));

        EditTabs.SelectedIndex = 0;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (FormationList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("formations", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (FormationList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (FormationList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("formations", item.RecordIndex, LabelMaps.Formations);
        var idField = f.FirstOrDefault(x => x.FieldName == "formationid");
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("formations", item.RecordIndex, "formationid", idField.Value);
            if (outcome.Success) RefreshEditor();
        }
    }

    // ---------- CM16 FormationForm groupings ----------

    private static bool IsName(string n) => n is "formationid" or "formationname" or "formationfullnameid"
        or "formationaudioid" or "relativeformationid" or "teamid";
    private static bool IsRole(string n) => n.EndsWith("role", StringComparison.OrdinalIgnoreCase);
    private static bool IsInstruction(string n) => n.Contains("instruction", StringComparison.OrdinalIgnoreCase)
        || n.Contains("interception", StringComparison.OrdinalIgnoreCase);
    private static bool IsPosition(string n) => n is "position0" or "position1" or "position2" or "position3"
        or "position4" or "position5" or "position6" or "position7" or "position8" or "position9" or "position10";
    private static bool IsOffset(string n) => n.Contains("offset", StringComparison.OrdinalIgnoreCase);
    private static bool IsStructure(string n) => n is "defenders" or "midfielders" or "attackers" or "offensiverating"
        or "formationid" or "formationname" or "formationfullnameid" or "teamid";
}