using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>FC26 port of CM16 LeagueForm's fixed flow-panel editor.</summary>
public partial class LeagueView : UserControl
{
    private static readonly string[] NameFieldOrder =
    {
        "leaguename", "leagueshortname", "fullname", "leagueid", "level", "countryid", "prestige",
    };

    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();
    private IReadOnlyList<RecordListItem> _teams = Array.Empty<RecordListItem>();
    private RecordListItem? _current;
    private int _leagueId;
    private int _logoRequest;

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public LeagueView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        PickUp.SelectObject += LoadEditor;
        PickUp.FilterByList = new[] { "All", "Country" };
        PickUp.RefreshObject += LoadList;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetLeagues();
        _teams = _vm.Session.Sections.GetTeams();
        PickUp.ObjectList = _all;
        SwitchLeagueCombo.ItemsSource = _all;
        TeamSearchBox.ItemsSource = _teams;
        if (_all.Count > 0 && PickUp.SelectedIndex < 0) PickUp.SelectedIndex = 0;
    }

    private void LoadEditor(RecordListItem item)
    {
        _current = item;
        CurrentLeagueName.Text = item.Title;
        var fields = _vm.Session.Sections.GetFields("leagues", item.RecordIndex, LabelMaps.Leagues);
        var byName = fields.ToDictionary(x => x.FieldName, StringComparer.OrdinalIgnoreCase);
        NameFields.ItemsSource = NameFieldOrder.Where(byName.ContainsKey).Select(x => byName[x]).ToList();
        ObjectiveFields.ItemsSource = fields.Where(f => IsObjective(f.FieldName)).ToList();
        _leagueId = ParseField(fields, "leagueid");
        RefreshLeagueTeams();
        _ = LoadLogoAsync(_leagueId);
    }

    private static int ParseField(IEnumerable<FieldValue> fields, string name)
    {
        var value = fields.FirstOrDefault(f => f.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase))?.RawValue;
        return int.TryParse(value, out var result) ? result : 0;
    }

    private void RefreshLeagueTeams()
    {
        var names = _vm.Session.Sections.GetLeagueTeams(_leagueId);
        var byName = _teams.ToLookup(t => t.Title, StringComparer.OrdinalIgnoreCase);
        TeamList.ItemsSource = names.Select(n => byName[n].FirstOrDefault() ?? new RecordListItem
        {
            RecordIndex = -1, Title = n,
        }).ToList();
    }

    private void TeamSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    private void TeamList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private async Task LoadLogoAsync(int leagueId)
    {
        var request = ++_logoRequest;
        LogoLarge.Source = LogoTiny.Source = LogoSmall.Source = LogoBanner.Source = null;
        LogoCaption.Visibility = Visibility.Visible;
        if (leagueId <= 0 || !_vm.Session.FrostbiteAssets.IsAvailable) return;
        var exported = await Task.Run(() =>
        {
            foreach (var candidate in new[]
            {
                $"data/ui/imgAssets/leaguelogos_sm/light/l{leagueId}.dds",
                $"data/ui/imgAssets/leaguelogos_sm/dark/l{leagueId}.dds",
                $"data/ui/imgAssets/leaguelogos_tiny/light/l{leagueId}.dds",
            })
            {
                var path = _vm.Session.FrostbiteAssets.ExportLegacyAsset(candidate);
                if (!string.IsNullOrWhiteSpace(path)) return path;
            }
            return null;
        });
        if (request != _logoRequest || string.IsNullOrWhiteSpace(exported)) return;
        var bitmap = await Task.Run(() => CreateBitmapSource(exported));
        if (request != _logoRequest || bitmap == null) return;
        LogoLarge.Source = LogoTiny.Source = LogoSmall.Source = LogoBanner.Source = bitmap;
        LogoCaption.Visibility = Visibility.Collapsed;
    }

    private static BitmapSource? CreateBitmapSource(string path)
    {
        using var preview = new TexturePreviewService().CreatePreview(path, 512, 256);
        if (preview == null) return null;
        using var stream = new MemoryStream();
        preview.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;
        var bitmap = new BitmapImage();
        bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.StreamSource = stream; bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (_current == null) return null;
        var outcome = _vm.Session.Pending.Stage("leagues", _current.RecordIndex, fieldName, value);
        if (outcome.Success) LoadEditor(_current);
        return outcome;
    }

    private void AddTeam_Click(object sender, RoutedEventArgs e)
    {
        if (TeamSearchBox.SelectedItem is not RecordListItem team) return;
        var outcome = StageLeagueLink(team, _leagueId);
        if (outcome?.Success == true) RefreshLeagueTeams();
    }

    private void ReplaceTeam_Click(object sender, RoutedEventArgs e)
    {
        if (TeamList.SelectedItem is not RecordListItem oldTeam || TeamSearchBox.SelectedItem is not RecordListItem newTeam) return;
        RemoveLeagueLink(oldTeam);
        var outcome = StageLeagueLink(newTeam, _leagueId);
        if (outcome?.Success == true) RefreshLeagueTeams();
    }

    private void RemoveTeam_Click(object sender, RoutedEventArgs e)
    {
        if (TeamList.SelectedItem is RecordListItem team) RemoveLeagueLink(team);
        RefreshLeagueTeams();
    }

    private void RemoveLeagueLink(RecordListItem team)
    {
        var links = _vm.Session.Database.GetTable("leagueteamlinks");
        if (links == null) return;
        for (var row = 0; row < links.RowCount; row++)
        {
            var league = _vm.Session.Database.GetCell("leagueteamlinks", row, "leagueid");
            var teamId = _vm.Session.Database.GetCell("leagueteamlinks", row, "teamid");
            if (int.TryParse(league, out var lid) && int.TryParse(teamId, out var tid)
                && lid == _leagueId && tid == GetTeamId(team))
            {
                _vm.Session.Pending.Stage("leagueteamlinks", row, "leagueid", "-1");
                return;
            }
        }
    }

    private EditOutcome? StageLeagueLink(RecordListItem team, int leagueId)
    {
        if (team.RecordIndex < 0) return null;
        _vm.Session.Pending.MarkStructuralChange();
        var links = _vm.Session.Database.GetTable("leagueteamlinks");
        if (links == null || links.RowCount == 0) return null;
        for (var row = 0; row < links.RowCount; row++)
        {
            var t = _vm.Session.Database.GetCell("leagueteamlinks", row, "teamid");
            var l = _vm.Session.Database.GetCell("leagueteamlinks", row, "leagueid");
            if (int.TryParse(t, out var tid) && int.TryParse(l, out var lid) && tid == GetTeamId(team) && lid == -1)
                return _vm.Session.Pending.Stage("leagueteamlinks", row, "leagueid", leagueId.ToString());
        }
        return null;
    }

    private int GetTeamId(RecordListItem team)
    {
        var value = _vm.Session.Database.GetCell("teams", team.RecordIndex, "teamid");
        return int.TryParse(value, out var id) ? id : -1;
    }

    private static bool IsObjective(string name) => name.Contains("board", StringComparison.OrdinalIgnoreCase)
        || name.Contains("objective", StringComparison.OrdinalIgnoreCase)
        || name.Contains("promotion", StringComparison.OrdinalIgnoreCase)
        || name.Contains("relegation", StringComparison.OrdinalIgnoreCase)
        || name.Contains("classification", StringComparison.OrdinalIgnoreCase);
}
