using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style LeagueForm: Find + league list on the left, Teams panel and
/// Names / Info groups on the right (matching LeagueForm groupBoxTeams and
/// groupBoxNames of Creation Master 16).
/// </summary>
public partial class LeagueView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();
    private IReadOnlyList<RecordListItem> _teams = Array.Empty<RecordListItem>();
    private int _leagueId;

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public LeagueView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetLeagues();
        _teams = _vm.Session.Sections.GetTeams();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchBox.Text;
        var items = _all.Where(x => x.Matches(q)).ToList();
        LeagueList.ItemsSource = items;
        CountText.Text = $"{items.Count} leagues" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void LeagueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LeagueList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void LoadEditor(RecordListItem item)
    {
        var fields = _vm.Session.Sections.GetFields("leagues", item.RecordIndex, LabelMaps.Leagues);
        NameFields.ItemsSource = fields.Where(f => IsName(f.FieldName));
        InfoFields.ItemsSource = fields.Where(f => !IsName(f.FieldName));

        _leagueId = fields.FirstOrDefault(f => f.FieldName == "leagueid") is { RawValue: var raw }
                    && int.TryParse(raw, out var id) ? id : 0;
        RefreshLeagueTeams();
    }

    private void RefreshLeagueTeams()
    {
        var names = _vm.Session.Sections.GetLeagueTeams(_leagueId);
        var byName = _teams.ToDictionary(t => t.Title, t => t, StringComparer.OrdinalIgnoreCase);
        var list = names.Select(n => byName.TryGetValue(n, out var t) ? t : new RecordListItem
        {
            RecordIndex = -1,
            Title = n,
        }).ToList();
        TeamList.ItemsSource = list;
        TeamSearchBox.Text = string.Empty;
    }

    private void TeamSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (TeamSearchBox.Text.Length == 0) { RefreshLeagueTeams(); return; }
        var q = TeamSearchBox.Text;
        var candidates = _teams.Where(x => x.Matches(q)).Take(40).ToList();
        TeamList.ItemsSource = candidates;
    }

    private void TeamList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (LeagueList.SelectedItem is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("leagues", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (LeagueList.SelectedItem is not RecordListItem item) return;
        LoadEditor(item);
    }

    private void NewId_Click(object sender, RoutedEventArgs e)
    {
        if (LeagueList.SelectedItem is not RecordListItem item) return;
        var f = _vm.Session.Sections.GetFields("leagues", item.RecordIndex, LabelMaps.Leagues);
        var idField = f.FirstOrDefault(x => x.FieldName == "leagueid");
        if (idField == null) return;
        if (long.TryParse(idField.RawValue, out var cur) && cur > 0)
        {
            idField.Value = (cur + 1).ToString();
            var outcome = _vm.Session.Pending.Stage("leagues", item.RecordIndex, "leagueid", idField.Value);
            if (outcome.Success) RefreshEditor();
        }
    }

    private void AddTeam_Click(object sender, RoutedEventArgs e)
    {
        var candidates = _teams.Where(x => x.Matches(TeamSearchBox.Text)).ToList();
        if (candidates.Count == 0) return;
        var team = candidates[0];
        var outcome = StageLeagueLink(team, _leagueId);
        if (outcome.Success) RefreshLeagueTeams();
    }

    private void ReplaceTeam_Click(object sender, RoutedEventArgs e)
    {
        if (TeamList.SelectedItem is not RecordListItem oldTeam) return;
        var candidates = _teams.Where(x => x.Matches(TeamSearchBox.Text)).ToList();
        if (candidates.Count == 0) return;
        var newTeam = candidates[0];
        var outcome = StageLeagueLink(newTeam, _leagueId);
        if (outcome.Success) RefreshLeagueTeams();
    }

    private void RemoveTeam_Click(object sender, RoutedEventArgs e)
    {
        if (TeamList.SelectedItem is not RecordListItem team) return;
        var links = _vm.Session.Database.GetTable("leagueteamlinks");
        if (links == null) return;
        for (var row = 0; row < links.RowCount; row++)
        {
            var l = _vm.Session.Database.GetCell("leagueteamlinks", row, "leagueid");
            var t = _vm.Session.Database.GetCell("leagueteamlinks", row, "teamid");
            if (int.TryParse(l, out var lid) && int.TryParse(t, out var tid) && lid == _leagueId
                && tid == ParseTeamId(team))
            {
                _vm.Session.Pending.Stage("leagueteamlinks", row, "leagueid", "-1");
                break;
            }
        }
        RefreshLeagueTeams();
    }

    private EditOutcome? StageLeagueLink(RecordListItem team, int leagueId)
    {
        if (team.RecordIndex < 0) return null;
        _vm.Session.Pending.MarkStructuralChange();
        var teamId = ParseTeamId(team);
        var links = _vm.Session.Database.GetTable("leagueteamlinks");
        if (links != null && links.RowCount > 0)
        {
            for (var row = 0; row < links.RowCount; row++)
            {
                var t = _vm.Session.Database.GetCell("leagueteamlinks", row, "teamid");
                var l = _vm.Session.Database.GetCell("leagueteamlinks", row, "leagueid");
                if (int.TryParse(t, out var tid) && int.TryParse(l, out var lid) && tid == teamId && lid == -1)
                    return _vm.Session.Pending.Stage("leagueteamlinks", row, "leagueid", leagueId.ToString());
            }
        }
        return _vm.Session.Pending.Stage("leagueteamlinks", 0, "teamid", teamId.ToString());
    }

    private static int ParseTeamId(RecordListItem team)
    {
        if (team.Detail.StartsWith("OVR", StringComparison.Ordinal)) return team.RecordIndex;
        return team.RecordIndex;
    }

    private static bool IsName(string n) => n is "leagueid" or "leaguename" or "leagueshortname" or "fullname" or "abbreviatedname";
}