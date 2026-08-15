using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

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

    /// <summary>Wired to each FieldRow so field edits stage through the pending service.</summary>
    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public PlayersView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetPlayers();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        PlayerList.ItemsSource = items;
        CountText.Text = $"{items.Count} players" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void PlayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("players", item.RecordIndex, LabelMaps.Players);

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

        FaceFields.ItemsSource = fields.Where(f => IsFace(f.FieldName));

        var ovr = fields.FirstOrDefault(f => f.FieldName == "overallrating");
        OverallText.Text = ovr?.Value ?? "—";
        OverallSlider.Value = double.TryParse(ovr?.RawValue, out var v) ? v : 0;
        EditTabs.SelectedIndex = 0;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
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

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (PlayerList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("players", item.RecordIndex, LabelMaps.Players);
        var idField = f.FirstOrDefault(x => x.FieldName.Equals("playerid", StringComparison.OrdinalIgnoreCase));
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, "playerid", idField.Value);
            if (outcome.Success) RefreshEditor();
        }
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
        || IsAttacking(n) || IsPhysical(n) || IsFreeKick(n) || n is "overallrating" or "potential";

    private static bool IsFace(string n) => n.Contains("face", StringComparison.OrdinalIgnoreCase)
        || n.Contains("hair", StringComparison.OrdinalIgnoreCase)
        || n.Contains("head", StringComparison.OrdinalIgnoreCase)
        || n.Contains("skin", StringComparison.OrdinalIgnoreCase)
        || n.Contains("beard", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eye", StringComparison.OrdinalIgnoreCase)
        || n.Contains("brow", StringComparison.OrdinalIgnoreCase);
}