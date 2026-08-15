using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style RefereeForm: Find + referee list on the left, identity / leagues
/// on the left column and generic face (head / hair / texture) on the right,
/// matching the group layout of Creation Master 16 RefereeForm.
/// </summary>
public partial class RefereeView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public RefereeView(ViewModel vm)
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
        _all = _vm.Session.Sections.GetItems("referee");
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
        RefereeList.ItemsSource = items;
        CountText.Text = $"{items.Count} referees" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void RefereeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RefereeList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("referee", item.RecordIndex, LabelMaps.Referees);

        IdentityFields.ItemsSource = fields.Where(f => IsIdentity(f.FieldName));
        ShoesFields.ItemsSource = fields.Where(f => IsShoes(f.FieldName));
        LeagueFields.ItemsSource = fields.Where(f => IsLeague(f.FieldName));

        FaceTypeFields.ItemsSource = fields.Where(f => IsFaceType(f.FieldName));
        HairFields.ItemsSource = fields.Where(f => IsHair(f.FieldName));
        HeadFields.ItemsSource = fields.Where(f => IsHead(f.FieldName));
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (RefereeList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("referee", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (RefereeList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }


    // ---------- CM16 groupIdentity / groupGenericFace groupings ----------

    private static bool IsIdentity(string n) => n is "refereeid" or "firstname" or "surname"
        or "nationalitycode" or "birthdate" or "gender" or "isreal"
        or "height" or "weight" or "bodytypecode" or "shortstyle" or "socklengthcode"
        or "sockstylecode" or "jerseysleevelengthcode" or "smallsidedshoetypecode";
    private static bool IsShoes(string n) => n.Contains("shoe", StringComparison.OrdinalIgnoreCase);
    private static bool IsLeague(string n) => n.Contains("league", StringComparison.OrdinalIgnoreCase)
        || n.Contains("card", StringComparison.OrdinalIgnoreCase)
        || n.Contains("strict", StringComparison.OrdinalIgnoreCase)
        || n.Contains("foul", StringComparison.OrdinalIgnoreCase);

    // groupGenericFace: Face Type / Hair Model / Head Model
    private static bool IsFaceType(string n) => n.Contains("skin", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eye", StringComparison.OrdinalIgnoreCase)
        || n.Contains("brow", StringComparison.OrdinalIgnoreCase)
        || n.Contains("facialhair", StringComparison.OrdinalIgnoreCase)
        || n.Contains("sideburn", StringComparison.OrdinalIgnoreCase)
        || n.Contains("lip", StringComparison.OrdinalIgnoreCase)
        || n.Contains("makeup", StringComparison.OrdinalIgnoreCase)
        || n.Contains("complexion", StringComparison.OrdinalIgnoreCase);
    private static bool IsHair(string n) => n.Contains("hair", StringComparison.OrdinalIgnoreCase);
    private static bool IsHead(string n) => n is "headtypecode" or "headvariation" or "headclasscode"
        or "headassetid" or "faceposerpreset" or "facepsdlayer0" or "facepsdlayer1"
        || n.Contains("face", StringComparison.OrdinalIgnoreCase);
}