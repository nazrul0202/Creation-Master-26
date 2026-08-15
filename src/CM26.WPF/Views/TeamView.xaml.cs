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

        LogoFields.ItemsSource = fields.Where(f => IsLogo(f.FieldName));
        NameFields.ItemsSource = fields.Where(f => IsName(f.FieldName));
        StadiumFields.ItemsSource = fields.Where(f => IsStadium(f.FieldName));
        ManagerFields.ItemsSource = fields.Where(f => IsManager(f.FieldName));
        InfoFields.ItemsSource = fields.Where(f => IsInfo(f.FieldName));
        LastYearFields.ItemsSource = fields.Where(f => IsLastYear(f.FieldName));
        LocationFields.ItemsSource = fields.Where(f => IsLocation(f.FieldName));
        TraitsFields.ItemsSource = fields.Where(f => IsTraits(f.FieldName));
        KitFields.ItemsSource = fields.Where(f => IsKit(f.FieldName));

        DefenseFields.ItemsSource = fields.Where(f => IsDefense(f.FieldName));
        BuildUpFields.ItemsSource = fields.Where(f => IsBuildUp(f.FieldName));
        ChanceFields.ItemsSource = fields.Where(f => IsChance(f.FieldName));
        FormationFields.ItemsSource = fields.Where(f => IsFormation(f.FieldName));
        SetPieceFields.ItemsSource = fields.Where(f => IsSetPiece(f.FieldName));

        FlagFields.ItemsSource = fields.Where(f => IsFlag(f.FieldName));

        UniqueAdboardFields.ItemsSource = fields.Where(f => IsUniqueAdboard(f.FieldName));
        UniqueBallFields.ItemsSource = fields.Where(f => IsUniqueBall(f.FieldName));
        UniqueManagerFields.ItemsSource = fields.Where(f => IsUniqueManager(f.FieldName));
        UniqueScarfFields.ItemsSource = fields.Where(f => IsUniqueScarf(f.FieldName));
        UniqueNetFields.ItemsSource = fields.Where(f => IsUniqueNet(f.FieldName));

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

    private static bool IsLogo(string n) => n is "teamid" or "assetid" or "genericbanner" or "isbannerenabled"
        or "hastifo" or "haslargeflag" or "skinnyflags" or "iscompetitionpoleflagenabled"
        or "iscompetitionscarfenabled" or "iscompetitioncrowdcardsenabled" or "hasstandingcrowd"
        or "hassubstitutionboard" or "hasvikingclap";
    private static bool IsName(string n) => n is "teamname" or "teamid" or "jerseytype" or "scoreboardname"
        or "abbreviatedname" or "shortname";
    private static bool IsStadium(string n) => n.Contains("stadium", StringComparison.OrdinalIgnoreCase)
        || n.Contains("pitch", StringComparison.OrdinalIgnoreCase)
        || n.Contains("goalnet", StringComparison.OrdinalIgnoreCase)
        || n.Contains("mowpattern", StringComparison.OrdinalIgnoreCase)
        || n.Contains("playsurface", StringComparison.OrdinalIgnoreCase)
        || n.Contains("trainingstadium", StringComparison.OrdinalIgnoreCase)
        || n.Contains("stanchion", StringComparison.OrdinalIgnoreCase)
        || n.Contains("flamethrower", StringComparison.OrdinalIgnoreCase)
        || n.Contains("cornerflag", StringComparison.OrdinalIgnoreCase)
        || n.Contains("crowd", StringComparison.OrdinalIgnoreCase)
        || n.Contains("stadiumcapacity", StringComparison.OrdinalIgnoreCase);
    private static bool IsManager(string n) => n.Contains("manager", StringComparison.OrdinalIgnoreCase)
        || n.Contains("personality", StringComparison.OrdinalIgnoreCase);
    private static bool IsInfo(string n) => n is "overallrating" or "attackrating" or "midfieldrating" or "defenserating"
        or "matchdayoverallrating" or "matchdayattackrating" or "matchdaymidfieldrating" or "matchdaydefenserating"
        or "domesticprestige" or "internationalprestige" or "foundationyear" or "clubworth" or "popularity"
        or "youthdevelopment" or "form" or "gender" or "rivalteam" or "domesticcups"
        or "profitability" or "numtransfersin" or "ethnicity" or "cityid" or "leaguetitles"
        || n.Contains("objective", StringComparison.OrdinalIgnoreCase)
        || n.Contains("threshold", StringComparison.OrdinalIgnoreCase)
        || n.Contains("opponent", StringComparison.OrdinalIgnoreCase);
    private static bool IsLastYear(string n) => n is "prev_el_champ" or "uefa_cl_wins" or "uefa_el_wins"
        or "uefa_uecl_wins" or "uefa_consecutive_wins" or "prevleague" or "positionlastyear" or "ischampion";
    private static bool IsLocation(string n) => n is "latitude" or "longitude" or "utcoffset" or "cityid";
    private static bool IsTraits(string n) => n.Contains("trait", StringComparison.OrdinalIgnoreCase)
        || n.Contains("shortoutback", StringComparison.OrdinalIgnoreCase)
        || n.Contains("attackingathome", StringComparison.OrdinalIgnoreCase)
        || n.Contains("centerback", StringComparison.OrdinalIgnoreCase)
        || n.Contains("wingers", StringComparison.OrdinalIgnoreCase)
        || n.Contains("pressure", StringComparison.OrdinalIgnoreCase)
        || n.Contains("defendlead", StringComparison.OrdinalIgnoreCase)
        || n.Contains("lineup", StringComparison.OrdinalIgnoreCase)
        || n.Contains("rotation", StringComparison.OrdinalIgnoreCase)
        || n.Contains("loyal", StringComparison.OrdinalIgnoreCase)
        || n.Contains("impatient", StringComparison.OrdinalIgnoreCase);
    private static bool IsKit(string n) => n.Contains("kit", StringComparison.OrdinalIgnoreCase)
        || n.Contains("jersey", StringComparison.OrdinalIgnoreCase)
        || n.Contains("ballid", StringComparison.OrdinalIgnoreCase)
        || n.Contains("color", StringComparison.OrdinalIgnoreCase)
        || n.Contains("presasset", StringComparison.OrdinalIgnoreCase);
    private static bool IsDefense(string n) => n.Contains("defense", StringComparison.OrdinalIgnoreCase)
        || n.Contains("defensive", StringComparison.OrdinalIgnoreCase)
        || n.Contains("offside", StringComparison.OrdinalIgnoreCase)
        || n.Contains("aggression", StringComparison.OrdinalIgnoreCase)
        || n.Contains("mentality", StringComparison.OrdinalIgnoreCase)
        || n.Contains("teamwidth", StringComparison.OrdinalIgnoreCase);
    private static bool IsBuildUp(string n) => n.Contains("buildup", StringComparison.OrdinalIgnoreCase)
        || n.Contains("build", StringComparison.OrdinalIgnoreCase)
        || n.Contains("passing", StringComparison.OrdinalIgnoreCase)
        || n.Contains("width", StringComparison.OrdinalIgnoreCase)
        || n.Contains("speed", StringComparison.OrdinalIgnoreCase);
    private static bool IsChance(string n) => n.Contains("chance", StringComparison.OrdinalIgnoreCase)
        || n.Contains("crossing", StringComparison.OrdinalIgnoreCase)
        || n.Contains("shooting", StringComparison.OrdinalIgnoreCase)
        || n.Contains("positioning", StringComparison.OrdinalIgnoreCase)
        || n.Contains("cksupport", StringComparison.OrdinalIgnoreCase);
    private static bool IsFormation(string n) => n.Contains("formation", StringComparison.OrdinalIgnoreCase)
        || n.Contains("teamsheet", StringComparison.OrdinalIgnoreCase)
        || n.Contains("line", StringComparison.OrdinalIgnoreCase);
    private static bool IsSetPiece(string n) => n.Contains("taker", StringComparison.OrdinalIgnoreCase)
        || n.Contains("captain", StringComparison.OrdinalIgnoreCase)
        || n.Contains("thrower", StringComparison.OrdinalIgnoreCase)
        || n.Contains("penalty", StringComparison.OrdinalIgnoreCase);
    private static bool IsFlag(string n) => n.Contains("flag", StringComparison.OrdinalIgnoreCase)
        || n.Contains("banner", StringComparison.OrdinalIgnoreCase)
        || n.Contains("tifo", StringComparison.OrdinalIgnoreCase)
        || n.Contains("substitutionboard", StringComparison.OrdinalIgnoreCase);

    // ---------- CM16 Rev. Mod. Extensions tab ----------

    private static bool IsUniqueAdboard(string n) => n.Contains("adboard", StringComparison.OrdinalIgnoreCase);
    private static bool IsUniqueBall(string n) => n.Contains("ball", StringComparison.OrdinalIgnoreCase);
    private static bool IsUniqueManager(string n) => n.Contains("manager", StringComparison.OrdinalIgnoreCase);
    private static bool IsUniqueScarf(string n) => n.Contains("scarf", StringComparison.OrdinalIgnoreCase);
    private static bool IsUniqueNet(string n) => n.Contains("net", StringComparison.OrdinalIgnoreCase);
}