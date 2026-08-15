using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;
using CM26.Studio.Controls;

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

        FaceTypeFields.ItemsSource = fields.Where(f => IsFaceType(f.FieldName) && !IsNamedAppearance(f.FieldName));
        HairFields.ItemsSource = fields.Where(f => IsHair(f.FieldName) && !IsNamedAppearance(f.FieldName));
        HeadFields.ItemsSource = fields.Where(f => IsHead(f.FieldName) && !IsNamedAppearance(f.FieldName));
        FillNamedCombos(fields);
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

    /// <summary>Uses the same named CM16 appearance catalogues as PlayerForm.
    /// A referee database can omit some player-only fields, so absent rows are hidden
    /// instead of presenting a misleading disabled control.</summary>
    private void FillNamedCombos(IReadOnlyList<FieldValue> fields)
    {
        FillCombo(ComboSkinType, "skintypecode", "Skin Type", AppearanceCatalog.SkinTypes, null, fields);
        FillCombo(ComboEyesBrow, "eyebrowcode", "Eyes Brow", AppearanceCatalog.EyebrowTypes, null, fields);
        FillCombo(ComboFacialHair, "facialhairtypecode", "Facial Hair", AppearanceCatalog.FacialHairTypes, null, fields);
        FillCombo(ComboFacialHairColor, "facialhaircolorcode", "Color", AppearanceCatalog.FacialHairColors, null, fields);

        FillModel(ComboHairModel, "hairtypecode", "Hair Model",
            AppearanceCatalog.HairModelSets.Select(set => (set.Name, set.Models)), fields);
        FillCombo(ComboHairColor, "haircolorcode", "Hair Color", AppearanceCatalog.HairColors, null, fields);

        FillModel(ComboHeadModel, "headtypecode", "Head Model",
            AppearanceCatalog.HeadModelSets.Select(set => (set.Name, set.Models)), fields);
        FillCombo(ComboSideburns, "sideburnscode", "Sideburns", AppearanceCatalog.Sideburns, null, fields);
        FillCombo(ComboEyesColor, "eyecolorcode", "Eyes Color", AppearanceCatalog.EyeColors, null, fields, valueOffset: 1);
        FillCombo(ComboFacePoser, "faceposerpreset", "Face Poser", AppearanceCatalog.FacePosers, null, fields);

        var hq = fields.FirstOrDefault(f => f.FieldName.Equals("hashighqualityhead", StringComparison.OrdinalIgnoreCase));
        CheckHighQualityHead.Visibility = hq == null ? Visibility.Collapsed : Visibility.Visible;
        if (hq != null)
            CheckHighQualityHead.SetContent("High Quality Face", hq.FieldName, hq.RawValue, hq.IsWritable, StageEdit);
    }

    private void FillCombo(NamedComboField combo, string fieldName, string label, IReadOnlyList<string> names,
        IReadOnlyList<int>? values, IReadOnlyList<FieldValue> fields, int valueOffset = 0)
    {
        var field = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        combo.Visibility = field == null ? Visibility.Collapsed : Visibility.Visible;
        if (field == null) return;

        var valueList = values ?? Enumerable.Range(valueOffset, names.Count).ToArray();
        combo.SetContent(label, field.FieldName, names, valueList, field.RawValue, field.IsWritable, StageEdit);
    }

    private void FillModel(GroupedModelPicker picker, string fieldName, string label,
        IEnumerable<(string Name, int[] Values)> groups, IReadOnlyList<FieldValue> fields)
    {
        var field = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        picker.Visibility = field == null ? Visibility.Collapsed : Visibility.Visible;
        if (field != null)
            picker.SetContent(label, field.FieldName, groups, field.RawValue, field.IsWritable, StageEdit);
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

    private static bool IsNamedAppearance(string n) => n.Equals("skintypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("eyebrowcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("facialhairtypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("facialhaircolorcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("hairtypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("haircolorcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("headtypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("sideburnscode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("eyecolorcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("faceposerpreset", StringComparison.OrdinalIgnoreCase)
        || n.Equals("hashighqualityhead", StringComparison.OrdinalIgnoreCase);
}
