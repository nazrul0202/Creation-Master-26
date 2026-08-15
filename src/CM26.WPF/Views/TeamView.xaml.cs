using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style TeamForm: Find + team list on the left, edit tabs on the right
/// (Generic: Name / Stadium / Info / Kit Links / Defense / Build Up / Chance
/// Creation; Roster with call / let-free / delete actions).
/// </summary>
public partial class TeamView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();
    private IReadOnlyList<TeamRosterItem> _roster = Array.Empty<TeamRosterItem>();
    private int _teamId;

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public TeamView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetTeams();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        TeamList.ItemsSource = items;
        CountText.Text = $"{items.Count} teams" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void TeamList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TeamList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("teams", item.RecordIndex, LabelMaps.Teams);

        NameFields.ItemsSource = fields.Where(f => IsName(f.FieldName));
        StadiumFields.ItemsSource = fields.Where(f => IsStadium(f.FieldName));
        InfoFields.ItemsSource = fields.Where(f => IsInfo(f.FieldName));
        KitFields.ItemsSource = fields.Where(f => IsKit(f.FieldName));
        DefenseFields.ItemsSource = fields.Where(f => IsDefense(f.FieldName));
        BuildUpFields.ItemsSource = fields.Where(f => IsBuildUp(f.FieldName));
        ChanceFields.ItemsSource = fields.Where(f => IsChance(f.FieldName));

        _teamId = fields.FirstOrDefault(f => f.FieldName == "teamid") is { RawValue: var raw }
                  && int.TryParse(raw, out var id) ? id : 0;
        LoadRoster();
        EditTabs.SelectedIndex = 0;
    }

    private void LoadRoster()
    {
        _roster = _vm.Session.Sections.GetTeamRoster(_teamId);
        RosterList.ItemsSource = _roster;
        RosterSearchBox.Text = string.Empty;
    }

    private void RosterSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = RosterSearchBox.Text;
        RosterList.ItemsSource = string.IsNullOrWhiteSpace(q)
            ? _roster
            : _roster.Where(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void RosterList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (TeamList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("teams", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (TeamList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (TeamList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("teams", item.RecordIndex, LabelMaps.Teams);
        var idField = f.FirstOrDefault(x => x.FieldName == "teamid");
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("teams", item.RecordIndex, "teamid", idField.Value);
            if (outcome.Success) RefreshEditor();
        }
    }

    private void CallPlayer_Click(object sender, RoutedEventArgs e)
    {
        if (TeamList.SelectedItem is not RecordListItem team) return;
        var t = _vm.Session.Sections.GetFields("teams", team.RecordIndex, LabelMaps.Teams);
        var idField = t.FirstOrDefault(x => x.FieldName == "teamid");
        if (idField == null || !int.TryParse(idField.RawValue, out var teamId)) return;

        var players = _vm.Session.Sections.GetPlayers();
        var candidate = players.FirstOrDefault(p => p.RecordIndex >= 0);
        if (candidate == null) return;
        var fields = _vm.Session.Sections.GetFields("players", candidate.RecordIndex);
        var pid = fields.FirstOrDefault(f => f.FieldName == "playerid");
        if (pid == null || !int.TryParse(pid.RawValue, out var playerId)) return;
        _vm.Session.Pending.MarkStructuralChange();
        _vm.Session.Pending.Stage("teamplayerlinks", 0, "playerid", playerId.ToString());
        _vm.Session.Pending.Stage("teamplayerlinks", 0, "teamid", teamId.ToString());
        LoadRoster();
    }

    private void LetFree_Click(object sender, RoutedEventArgs e)
    {
        if (RosterList.SelectedItem is not TeamRosterItem player) return;
        if (TeamList.SelectedItem is not RecordListItem team) return;
        var t = _vm.Session.Sections.GetFields("teams", team.RecordIndex, LabelMaps.Teams);
        var idField = t.FirstOrDefault(x => x.FieldName == "teamid");
        if (idField == null || !int.TryParse(idField.RawValue, out var teamId)) return;

        var links = _vm.Session.Database.GetTable("teamplayerlinks");
        if (links == null) return;
        for (var row = 0; row < links.RowCount; row++)
        {
            var cell = _vm.Session.Database.GetCell("teamplayerlinks", row, "playerid");
            var tcell = _vm.Session.Database.GetCell("teamplayerlinks", row, "teamid");
            if (int.TryParse(cell, out var pid) && int.TryParse(tcell, out var tid)
                && pid == player.PlayerId && tid == teamId)
            {
                var outcome = _vm.Session.Pending.Stage("teamplayerlinks", row, "teamid", "-1");
                if (outcome.Success) break;
            }
        }
        LoadRoster();
    }

    private void DeletePlayer_Click(object sender, RoutedEventArgs e)
    {
        if (RosterList.SelectedItem is not TeamRosterItem player) return;
        var outcome = _vm.Session.Pending.Stage("players", player.PlayerId, "playerid", "-1");
        if (!outcome.Success) return;
        LoadRoster();
    }

    // ---------- CM16 Generic tab groupings ----------

    private static bool IsName(string n) => n is "teamid" or "teamname" or "abbreviatedname" or "shortname" or "scoreboardname";
    private static bool IsStadium(string n) => n.Contains("stadium", StringComparison.OrdinalIgnoreCase)
        || n.Contains("pitch", StringComparison.OrdinalIgnoreCase)
        || n.Contains("goalnet", StringComparison.OrdinalIgnoreCase)
        || n.Contains("mowpattern", StringComparison.OrdinalIgnoreCase)
        || n.Contains("playsurface", StringComparison.OrdinalIgnoreCase)
        || n.Contains("trainingstadium", StringComparison.OrdinalIgnoreCase);
    private static bool IsInfo(string n) => n is "overallrating" or "attackrating" or "midfieldrating" or "defenserating"
        or "matchdayoverallrating" or "matchdayattackrating" or "matchdaymidfieldrating" or "matchdaydefenserating"
        or "domesticprestige" or "internationalprestige" or "foundationyear" or "clubworth" or "popularity"
        or "youthdevelopment" or "leaguetitles" or "form" or "gender" or "cityid" or "latitude" or "longitude"
        or "utcoffset" or "rivalteam" or "domesticcups" or "profitability" or "numtransfersin" or "ethnicity"
        or "uefa_cl_wins" or "uefa_el_wins" or "uefa_uecl_wins" or "uefa_consecutive_wins" or "prev_el_champ";
    private static bool IsKit(string n) => n.Contains("kit", StringComparison.OrdinalIgnoreCase)
        || n.Contains("jersey", StringComparison.OrdinalIgnoreCase)
        || n.Contains("ballid", StringComparison.OrdinalIgnoreCase)
        || n.Contains("color", StringComparison.OrdinalIgnoreCase)
        || n.Contains("presasset", StringComparison.OrdinalIgnoreCase)
        || n.Contains("taker", StringComparison.OrdinalIgnoreCase)
        || n.Contains("captain", StringComparison.OrdinalIgnoreCase)
        || n.Contains("thrower", StringComparison.OrdinalIgnoreCase);
    private static bool IsDefense(string n) => n.Contains("defense", StringComparison.OrdinalIgnoreCase)
        || n.Contains("defensive", StringComparison.OrdinalIgnoreCase)
        || n.Contains("offside", StringComparison.OrdinalIgnoreCase)
        || n.Contains("aggression", StringComparison.OrdinalIgnoreCase);
    private static bool IsBuildUp(string n) => n.Contains("buildup", StringComparison.OrdinalIgnoreCase)
        || n.Contains("build", StringComparison.OrdinalIgnoreCase)
        || n.Contains("passing", StringComparison.OrdinalIgnoreCase)
        || n.Contains("width", StringComparison.OrdinalIgnoreCase);
    private static bool IsChance(string n) => n.Contains("chance", StringComparison.OrdinalIgnoreCase)
        || n.Contains("crossing", StringComparison.OrdinalIgnoreCase)
        || n.Contains("shooting", StringComparison.OrdinalIgnoreCase)
        || n.Contains("positioning", StringComparison.OrdinalIgnoreCase);
}