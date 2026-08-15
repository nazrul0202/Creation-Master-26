using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;
using CM26.Studio.Controls;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style PlayerForm: Find + player list on the left, edit tabs on the
/// right matching the exact group layout of Creation Master 16 PlayerForm
/// (pageInfo: Identity Card / Body / Look / Playing for / Shoes / Playing Info,
/// pageSkills: Random Generation + skill groups, pageFace: Appearance).
/// </summary>
public partial class PlayersView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();
    private IReadOnlyList<FieldValue> _currentFields = Array.Empty<FieldValue>();

    /// <summary>Wired to each FieldRow so field edits stage through the pending service.</summary>
    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public PlayersView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        TraitsGrid.Toggle = TogglePlaystyle;
        VirtualProGrid.Toggle = TogglePlaystyle;
        PickUp.SelectObject += LoadEditorFromPickUp;
        PickUp.FilterByList = new[] { "All", "by Team", "by Country", "Free Agents" };
        PickUp.FilterChanged += ApplyFilter;
        Loaded += (_, _) => LoadList();
    }

    private void LoadEditorFromPickUp(RecordListItem item) => LoadEditor(item);

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetPlayers();
        PickUp.ObjectList = _all;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = PickUp.FilterValueText;
        var by = PickUp.FilterByComboText;
        IEnumerable<RecordListItem> source = _all;
        if (!string.IsNullOrWhiteSpace(by) && !string.IsNullOrWhiteSpace(q))
        {
            source = by switch
            {
                "by Team" => _all.Where(x => x.Subtitle.Contains(q, StringComparison.OrdinalIgnoreCase)),
                "by Country" => _all.Where(x => x.SearchText.Contains(q, StringComparison.OrdinalIgnoreCase)),
                _ => _all,
            };
        }
        else if (by == "Free Agents" && string.IsNullOrWhiteSpace(q))
        {
            source = _all.Where(x => string.IsNullOrWhiteSpace(x.Subtitle) || x.Subtitle == "Free Agent");
        }
        var items = source.ToList();
        PlayerList.ItemsSource = items;
        CountText.Text = $"{items.Count} players" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void PlayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }
    private void LoadEditor(RecordListItem item)
    {
        _currentFields = _vm.Session.Sections.GetFields("players", item.RecordIndex, LabelMaps.Players);
        var fields = _currentFields;

        IdentityFields.ItemsSource = fields.Where(f => IsIdentity(f.FieldName));
        BodyFields.ItemsSource = fields.Where(f => IsBody(f.FieldName));
        LookFields.ItemsSource = fields.Where(f => IsLook(f.FieldName));
        TeamFields.ItemsSource = fields.Where(f => IsTeam(f.FieldName));
        ShoesFields.ItemsSource = fields.Where(f => IsShoes(f.FieldName));
        PositionFields.ItemsSource = fields.Where(f => IsPlayingInfo(f.FieldName));

        GkFields.ItemsSource = fields.Where(f => IsGk(f.FieldName));
        DefensiveFields.ItemsSource = fields.Where(f => IsDefensive(f.FieldName));
        MidfielderFields.ItemsSource = fields.Where(f => IsMidfielder(f.FieldName));
        MentalFields.ItemsSource = fields.Where(f => IsMental(f.FieldName));
        AttackingFields.ItemsSource = fields.Where(f => IsAttacking(f.FieldName));
        PhysicalFields.ItemsSource = fields.Where(f => IsPhysical(f.FieldName));
        FreeKickFields.ItemsSource = fields.Where(f => IsFreeKick(f.FieldName));

        TraitsGrid.Items = BuildPlaystyleFlags(fields, "trait1", "trait2");
        VirtualProGrid.Items = BuildPlaystyleFlags(fields, "icontrait1", "icontrait2");

        FaceTypeFields.ItemsSource = fields.Where(f => IsFaceType(f.FieldName) && !IsNamedAppearance(f.FieldName));
        HairFields.ItemsSource = fields.Where(f => IsHair(f.FieldName) && !IsNamedAppearance(f.FieldName));
        HeadFields.ItemsSource = fields.Where(f => IsHead(f.FieldName) && !IsNamedAppearance(f.FieldName));

        FillNamedCombos(fields);

        var ovr = fields.FirstOrDefault(f => f.FieldName == "overallrating");
        OverallText.Text = ovr?.Value ?? "—";
        OverallSlider.Value = double.TryParse(ovr?.RawValue, out var v) ? v : 0;
        EditTabs.SelectedIndex = 0;
    }

    /// <summary>Builds the 34 playstyle flags for one bitmask pair (trait1/trait2 or icontrait1/icontrait2).</summary>
    private static IReadOnlyList<PlaystyleFlag> BuildPlaystyleFlags(IReadOnlyList<FieldValue> fields, string fieldA, string fieldB)
    {
        var flags = new List<PlaystyleFlag>(PlaystyleCatalog.Names.Length);
        for (int i = 0; i < PlaystyleCatalog.Names.Length; i++)
        {
            var (field, bit) = i < 32 ? (fieldA, i) : (fieldB, i - 32);
            var fv = fields.FirstOrDefault(f => f.FieldName.Equals(field, StringComparison.OrdinalIgnoreCase));
            var mask = fv != null && uint.TryParse(fv.RawValue, out var m) ? m : 0u;
            flags.Add(new PlaystyleFlag
            {
                Field = field,
                Bit = bit,
                Name = PlaystyleCatalog.Names[i],
                IsSet = (mask & (1u << bit)) != 0,
                IsWritable = fv?.IsWritable ?? false,
            });
        }
        return flags;
    }

    /// <summary>Playstyle checkbox toggled: re-read the current mask, flip the bit, stage the new mask.</summary>
    private EditOutcome? TogglePlaystyle(string fieldName, int bit, bool set)
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return null;
        var fv = _currentFields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        if (fv == null || !uint.TryParse(fv.RawValue, out var mask)) return null;
        var newMask = set ? mask | (1u << bit) : mask & ~(1u << bit);
        var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, fieldName, newMask.ToString());
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    /// <summary>Fills the CM16-style named combos (Face tab) from the current fields.
    /// Fields covered by a named combo are excluded from the generic field lists.</summary>
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
        CheckHighQualityHead.SetContent("High Quality Face", "hashighqualityhead", hq?.RawValue,
            hq?.IsWritable ?? false, StageEdit);
    }

    private void FillCombo(NamedComboField combo, string fieldName, string label, IReadOnlyList<string> names,
        IReadOnlyList<int>? values, IReadOnlyList<FieldValue> fields, int valueOffset = 0)
    {
        var fv = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        var valueList = values ?? Enumerable.Range(valueOffset, names.Count).ToArray();
        combo.SetContent(label, fieldName, names, valueList, fv?.RawValue,
            fv?.IsWritable ?? false, StageEdit);
    }

    private void FillModel(GroupedModelPicker picker, string fieldName, string label,
        IEnumerable<(string Name, int[] Values)> groups, IReadOnlyList<FieldValue> fields)
    {
        var field = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        picker.Visibility = field == null ? Visibility.Collapsed : Visibility.Visible;
        if (field != null)
            picker.SetContent(label, field.FieldName, groups, field.RawValue, field.IsWritable, StageEdit);
    }

    private void RefreshEditor()
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void OverallSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return;
        var value = ((int)OverallSlider.Value).ToString();
        var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, "overallrating", value);
        if (outcome.Success) OverallText.Text = value;
    }

    private void Randomize_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.Tag is not string target) return;
        if (PlayerList.SelectedItem is not RecordListItem item) return;
        var fields = _vm.Session.Sections.GetFields("players", item.RecordIndex, LabelMaps.Players);
        var rnd = new Random();
        foreach (var f in fields.Where(f => IsSkillField(f.FieldName) && int.TryParse(f.RawValue, out _)))
        {
            var skill = int.Parse(f.RawValue);
            var spread = rnd.Next(-10, 11);
            var newValue = Math.Clamp(skill + spread, 0, 99);
            _vm.Session.Pending.Stage("players", item.RecordIndex, f.FieldName, newValue.ToString());
        }
        var targetVal = Math.Clamp(int.Parse(target), 1, 99);
        _vm.Session.Pending.Stage("players", item.RecordIndex, "overallrating", targetVal.ToString());
        OverallSlider.Value = targetVal;
        OverallText.Text = targetVal.ToString();
        RefreshEditor();
    }

    // ---------- CM16 Info tab groupings ----------

    private static bool IsIdentity(string n) => n is "playerid" or "firstnameid" or "lastnameid" or "commonnameid"
        or "birthdate" or "nationality" or "gender";
    private static bool IsBody(string n) => n is "height" or "weight" or "preferredfoot" or "weakfootabilitytypecode" or "bodytype";
    private static bool IsLook(string n) => n.Contains("sleeve", StringComparison.OrdinalIgnoreCase)
        || n.Contains("jersey", StringComparison.OrdinalIgnoreCase)
        || n.Contains("accessor", StringComparison.OrdinalIgnoreCase)
        || n.Contains("winter", StringComparison.OrdinalIgnoreCase)
        || n.Contains("socks", StringComparison.OrdinalIgnoreCase)
        || n.Contains("fit", StringComparison.OrdinalIgnoreCase);
    private static bool IsTeam(string n) => n.Contains("team", StringComparison.OrdinalIgnoreCase)
        || n.Contains("loan", StringComparison.OrdinalIgnoreCase)
        || n.Contains("contract", StringComparison.OrdinalIgnoreCase)
        || n.Contains("jointeam", StringComparison.OrdinalIgnoreCase)
        || n.Contains("captain", StringComparison.OrdinalIgnoreCase);
    private static bool IsShoes(string n) => n.Contains("shoes", StringComparison.OrdinalIgnoreCase)
        || n.Contains("boots", StringComparison.OrdinalIgnoreCase)
        || n.Contains("sock", StringComparison.OrdinalIgnoreCase);
    private static bool IsPlayingInfo(string n) => n.Contains("position", StringComparison.OrdinalIgnoreCase)
        || n.Contains("skillmoves", StringComparison.OrdinalIgnoreCase)
        || n.Contains("workrate", StringComparison.OrdinalIgnoreCase)
        || n.Contains("reputation", StringComparison.OrdinalIgnoreCase)
        || n is "overallrating" or "potential" or "form";

    // ---------- CM16 Skills tab groupings ----------

    private static bool IsGk(string n) => n.StartsWith("gk", StringComparison.OrdinalIgnoreCase);
    private static bool IsDefensive(string n) => n is "aggression" or "marking" or "headingaccuracy" or "standingtackle"
        or "slidingtackle" or "interceptions" or "defensiveawareness";
    private static bool IsMidfielder(string n) => n is "ballcontrol" or "crossing" or "shortpassing" or "longpassing"
        or "vision" or "curve";
    private static bool IsMental(string n) => n is "reactions" or "composure" or "positioning" or "aggression" or "interceptions";
    private static bool IsAttacking(string n) => n is "dribbling" or "finishing" or "shotpower" or "longshots"
        or "volleys" or "penalties" or "positioning";
    private static bool IsPhysical(string n) => n is "acceleration" or "sprintspeed" or "agility" or "balance"
        or "jumping" or "stamina" or "strength";
    private static bool IsFreeKick(string n) => n.Contains("freekick", StringComparison.OrdinalIgnoreCase)
        || n.Contains("fk", StringComparison.OrdinalIgnoreCase)
        || n.Contains("penalty", StringComparison.OrdinalIgnoreCase)
        || n.Contains("corner", StringComparison.OrdinalIgnoreCase)
        || n.Contains("gkkick", StringComparison.OrdinalIgnoreCase);

    private static bool IsSkillField(string n) => IsGk(n) || IsDefensive(n) || IsMidfielder(n) || IsMental(n)
        || IsAttacking(n) || IsPhysical(n) || IsFreeKick(n)
        || n is "overallrating" or "potential";

    // ---------- CM16 Face tab groupings ----------

    private static bool IsFaceType(string n) => n.Contains("skin", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eyebrow", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eyecolor", StringComparison.OrdinalIgnoreCase)
        || n.Contains("facialhair", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eyedetail", StringComparison.OrdinalIgnoreCase);
    private static bool IsHair(string n) => n.Contains("hair", StringComparison.OrdinalIgnoreCase);
    private static bool IsHead(string n) => n.Contains("head", StringComparison.OrdinalIgnoreCase)
        || n.Contains("face", StringComparison.OrdinalIgnoreCase)
        || n.Contains("beard", StringComparison.OrdinalIgnoreCase)
        || n.Contains("tattoo", StringComparison.OrdinalIgnoreCase);

    /// <summary>Fields rendered as CM16-style named combos/checks on the Face tab
    /// (so they are excluded from the generic text-box field lists).</summary>
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
