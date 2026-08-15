using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style CompetitionForm (Tournament): Find + competition list on the
/// left, edit tabs on the right matching the exact tab layout of Creation
/// Master 16 CompetitionForm (pageWorld "FIFA" / pageConfederation /
/// pageNation / pageTrophy / pageStage / pageGroup).
/// </summary>
public partial class TournamentView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public TournamentView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetItems("competition");
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        CompetitionList.ItemsSource = items;
        CountText.Text = $"{items.Count} competitions" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void CompetitionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CompetitionList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("competition", item.RecordIndex, LabelMaps.Tournaments);

        var idents = fields.Where(f => IsIdentity(f.FieldName)).ToList();
        var colors = fields.Where(f => IsColor(f.FieldName)).ToList();
        var badges = fields.Where(f => IsBadge(f.FieldName)).ToList();
        var balls = fields.Where(f => IsBall(f.FieldName)).ToList();
        var flags = fields.Where(f => IsFlag(f.FieldName)).ToList();
        var structure = fields.Where(f => IsStructure(f.FieldName)).ToList();

        IdentityFields.ItemsSource = idents;
        ColorFields.ItemsSource = colors;
        BadgeFields.ItemsSource = badges;
        BallFields.ItemsSource = balls;
        FlagFields.ItemsSource = flags;

        ConfIdentityFields.ItemsSource = idents;
        ConfColorFields.ItemsSource = colors;
        ConfBadgeFields.ItemsSource = badges;
        ConfFlagFields.ItemsSource = flags;

        NatIdentityFields.ItemsSource = idents;
        NatColorFields.ItemsSource = colors;
        NatBadgeFields.ItemsSource = badges;

        TrophyIdentityFields.ItemsSource = idents;
        TrophyStructureFields.ItemsSource = structure;
        TrophyColorFields.ItemsSource = colors;
        TrophyBadgeFields.ItemsSource = badges;
        TrophyBallFields.ItemsSource = balls;

        StageIdentityFields.ItemsSource = idents;
        StageStructureFields.ItemsSource = structure;

        GroupIdentityFields.ItemsSource = idents;
        GroupStructureFields.ItemsSource = structure;

        EditTabs.SelectedIndex = 0;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (CompetitionList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("competition", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (CompetitionList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (CompetitionList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("competition", item.RecordIndex, LabelMaps.Tournaments);
        var idField = f.FirstOrDefault(x => x.FieldName == "competitionid");
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("competition", item.RecordIndex, "competitionid", idField.Value);
            if (outcome.Success) RefreshEditor();
        }
    }

    // ---------- CM16 CompetitionForm groupings ----------

    private static bool IsIdentity(string n) => n is "competitionid" or "competitionimportance"
        or "isrealcompetition" or "iswomencompetition" or "country_lock" or "languageregion"
        or "subsonbench" or "goalscoringrecord" or "crowdregion" or "crowdskintonecode"
        or "competitionchampionid" or "finalstadium" or "finalballid" or "winterballid" or "ballid";
    private static bool IsColor(string n) => n.Contains("color", StringComparison.OrdinalIgnoreCase)
        || n.Contains("crowdregion", StringComparison.OrdinalIgnoreCase);
    private static bool IsBadge(string n) => n.Contains("badge", StringComparison.OrdinalIgnoreCase)
        || n.Contains("adboard", StringComparison.OrdinalIgnoreCase)
        || n.Contains("tarp", StringComparison.OrdinalIgnoreCase)
        || n.Contains("pitch", StringComparison.OrdinalIgnoreCase)
        || n.Contains("banner", StringComparison.OrdinalIgnoreCase)
        || n.Contains("wipe", StringComparison.OrdinalIgnoreCase)
        || n.Contains("board", StringComparison.OrdinalIgnoreCase);
    private static bool IsBall(string n) => n.Contains("ball", StringComparison.OrdinalIgnoreCase);
    private static bool IsFlag(string n) => n.Contains("flag", StringComparison.OrdinalIgnoreCase)
        || n.Contains("scarf", StringComparison.OrdinalIgnoreCase)
        || n.Contains("confetti", StringComparison.OrdinalIgnoreCase)
        || n.Contains("archway", StringComparison.OrdinalIgnoreCase)
        || n.Contains("viking", StringComparison.OrdinalIgnoreCase)
        || n.Contains("mascot", StringComparison.OrdinalIgnoreCase)
        || n.Contains("flame", StringComparison.OrdinalIgnoreCase)
        || n.Contains("stanchion", StringComparison.OrdinalIgnoreCase)
        || n.Contains("goalnet", StringComparison.OrdinalIgnoreCase)
        || n.Contains("celebr", StringComparison.OrdinalIgnoreCase)
        || n.Contains("handshake", StringComparison.OrdinalIgnoreCase)
        || n.Contains("injury", StringComparison.OrdinalIgnoreCase)
        || n.Contains("champion", StringComparison.OrdinalIgnoreCase)
        || n.Contains("trophy", StringComparison.OrdinalIgnoreCase);
    private static bool IsStructure(string n) => n.Contains("stage", StringComparison.OrdinalIgnoreCase)
        || n.Contains("enabled", StringComparison.OrdinalIgnoreCase)
        || n.Contains("intro", StringComparison.OrdinalIgnoreCase)
        || n.Contains("authentic", StringComparison.OrdinalIgnoreCase)
        || n.Contains("goaljingle", StringComparison.OrdinalIgnoreCase)
        || n.Contains("anthem", StringComparison.OrdinalIgnoreCase)
        || n.Contains("var", StringComparison.OrdinalIgnoreCase)
        || n.Contains("replay", StringComparison.OrdinalIgnoreCase)
        || n.Contains("corner", StringComparison.OrdinalIgnoreCase)
        || n.Contains("penalt", StringComparison.OrdinalIgnoreCase)
        || n.Contains("dressing", StringComparison.OrdinalIgnoreCase)
        || n.Contains("crowdcard", StringComparison.OrdinalIgnoreCase)
        || n.Contains("inflatables", StringComparison.OrdinalIgnoreCase);
}