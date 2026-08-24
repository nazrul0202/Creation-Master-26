using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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
    private int _crestRequest;

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public TeamView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        PickUp.SelectObject += LoadEditorFromPickUp;
        PickUp.FilterByList = new[] { "All", "by League" };
        PickUp.FilterChanged += ApplyFilter;
        Loaded += (_, _) => LoadList();
    }

    private void LoadEditorFromPickUp(RecordListItem item) => LoadEditor(item);

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetTeams();
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
                "by League" => _all.Where(x => x.Subtitle.Contains(q, StringComparison.OrdinalIgnoreCase)),
                _ => _all,
            };
        }
        var items = source.ToList();
        TeamList.ItemsSource = items;
        CountText.Text = $"{items.Count} teams" + (string.IsNullOrWhiteSpace(q) ? "" : $" matching '{q}'");
    }

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
        Fc26ExtensionFields.ItemsSource = fields.Where(f => !IsCm16Field(f.FieldName));

        _teamId = fields.FirstOrDefault(f => f.FieldName == "teamid") is { RawValue: var raw }
                  && int.TryParse(raw, out var id) ? id : 0;
        _ = LoadCrestAsync(_teamId);
        LoadRoster();
        EditTabs.SelectedIndex = 0;
    }

    private async Task LoadCrestAsync(int teamId)
    {
        var request = ++_crestRequest;
        CrestLarge.Source = Crest50.Source = Crest32.Source = Crest16.Source = null;
        CrestLargeCaption.Visibility = Crest50Caption.Visibility = Crest32Caption.Visibility = Crest16Caption.Visibility = Visibility.Visible;
        if (teamId <= 0 || !_vm.Session.FrostbiteAssets.IsAvailable) return;

        var path = await Task.Run(() =>
        {
            foreach (var candidate in new[]
            {
                $"data/ui/imgAssets/crest/dark/l{teamId}.dds",
                $"data/ui/imgAssets/crest/light/l{teamId}.dds",
                $"data/ui/imgAssets/crest/l{teamId}.dds",
            })
            {
                var exported = _vm.Session.FrostbiteAssets.ExportLegacyAsset(candidate);
                if (!string.IsNullOrWhiteSpace(exported)) return exported;
            }
            return null;
        });
        if (request != _crestRequest || string.IsNullOrWhiteSpace(path)) return;

        var bitmap = await Task.Run(() => CreateBitmapSource(path));
        if (request != _crestRequest || bitmap == null) return;
        CrestLarge.Source = Crest50.Source = Crest32.Source = Crest16.Source = bitmap;
        CrestLargeCaption.Visibility = Crest50Caption.Visibility = Crest32Caption.Visibility = Crest16Caption.Visibility = Visibility.Collapsed;
    }

    private static BitmapSource? CreateBitmapSource(string path)
    {
        using var preview = new TexturePreviewService().CreatePreview(path, 256, 256);
        if (preview == null) return null;
        using var stream = new MemoryStream();
        preview.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
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

    private void TransferPlayer_Click(object sender, RoutedEventArgs e)
    {
        if (RosterList.SelectedItem is not TeamRosterItem player)
        {
            MessageBox.Show(Window.GetWindow(this), "Select a team player first.", "Transfer Player",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var links = _vm.Session.Database.GetTable("teamplayerlinks");
        var teams = _vm.Session.Database.GetTable("teams");
        if (links == null || teams == null)
        {
            MessageBox.Show(Window.GetWindow(this), "Roster relationship data is unavailable.", "Transfer Player",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var linkTeamColumn = ColumnIndex(links, "teamid");
        var linkPlayerColumn = ColumnIndex(links, "playerid");
        var teamIdColumn = ColumnIndex(teams, "teamid");
        var teamNameColumn = ColumnIndex(teams, "teamname");
        if (_teamId <= 0 || linkTeamColumn < 0 || linkPlayerColumn < 0 || teamIdColumn < 0 || teamNameColumn < 0 ||
            !links.Columns[linkTeamColumn].IsWritable)
        {
            MessageBox.Show(Window.GetWindow(this), "This player relationship cannot be edited safely.", "Transfer Player",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sourceLinkRow = -1;
        var alreadyLinkedTeamIds = new HashSet<int>();
        for (var row = 0; row < links.RowCount; row++)
        {
            if (!int.TryParse(_vm.Session.Database.GetCell("teamplayerlinks", row, "playerid"), out var linkedPlayerId) ||
                linkedPlayerId != player.PlayerId) continue;
            if (!int.TryParse(_vm.Session.Database.GetCell("teamplayerlinks", row, "teamid"), out var linkedTeamId)) continue;
            alreadyLinkedTeamIds.Add(linkedTeamId);
            if (linkedTeamId == _teamId) sourceLinkRow = row;
        }
        if (sourceLinkRow < 0)
        {
            MessageBox.Show(Window.GetWindow(this), "The selected player's link to this team was not found.", "Transfer Player",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var choices = new List<TeamTransferChoice>();
        for (var row = 0; row < teams.RowCount; row++)
        {
            var record = _vm.Session.Database.GetRecord("teams", row);
            if (record == null || !int.TryParse(record.Get(teamIdColumn), out var targetTeamId) || targetTeamId <= 0 ||
                targetTeamId == _teamId || alreadyLinkedTeamIds.Contains(targetTeamId)) continue;
            choices.Add(new TeamTransferChoice(targetTeamId, record.Get(teamNameColumn)));
        }
        choices.Sort((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name));
        if (choices.Count == 0)
        {
            MessageBox.Show(Window.GetWindow(this), "No valid destination team is available.", "Transfer Player",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var destination = new ComboBox { ItemsSource = choices, SelectedIndex = 0, MinWidth = 300, Margin = new Thickness(0, 6, 0, 12) };
        var dialog = new Window
        {
            Title = $"Transfer {player.Name}", Owner = Window.GetWindow(this), WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false,
            Content = new StackPanel { Margin = new Thickness(16), MinWidth = 340 }
        };
        var content = (StackPanel)dialog.Content;
        content.Children.Add(new TextBlock { Text = $"Move {player.Name} to:", FontWeight = FontWeights.SemiBold });
        content.Children.Add(destination);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var transfer = new Button { Content = "Transfer", IsDefault = true, MinWidth = 86, Margin = new Thickness(0, 0, 6, 0) };
        transfer.Click += (_, _) => dialog.DialogResult = true;
        var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 76 };
        buttons.Children.Add(transfer);
        buttons.Children.Add(cancel);
        content.Children.Add(buttons);

        if (dialog.ShowDialog() != true || destination.SelectedItem is not TeamTransferChoice target) return;
        var outcome = _vm.Session.Pending.Stage("teamplayerlinks", sourceLinkRow, "teamid", target.TeamId.ToString());
        if (!outcome.Success)
        {
            MessageBox.Show(Window.GetWindow(this), outcome.Message, "Transfer Player", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        LoadRoster();
        MessageBox.Show(Window.GetWindow(this), $"{player.Name} will move to {target.Name}. Save to apply the transfer.",
            "Transfer Player", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static int ColumnIndex(DbTable table, string name)
    {
        for (var index = 0; index < table.Columns.Count; index++)
            if (string.Equals(table.Columns[index].Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(table.Columns[index].ShortName, name, StringComparison.OrdinalIgnoreCase)) return index;
        return -1;
    }

    private sealed record TeamTransferChoice(int TeamId, string Name)
    {
        public override string ToString() => $"{Name} [{TeamId}]";
    }

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

    // Logos, translated team names, manager names and kit links are assets or
    // linked tables in both CM16 and FC26. They must not be filled with
    // unrelated columns merely because a column happens to contain "flag",
    // "crowd" or "manager".
    private static bool IsLogo(string n) => false;
    private static bool IsName(string n) => n is "teamname";
    private static bool IsStadium(string n) => n is "trainingstadium";
    private static bool IsManager(string n) => false;
    private static bool IsInfo(string n) => n is "teamid" or "overallrating" or "attackrating" or "midfieldrating" or "defenserating"
        or "matchdayoverallrating" or "matchdayattackrating" or "matchdaymidfieldrating" or "matchdaydefenserating"
        or "domesticprestige" or "internationalprestige" or "foundationyear" or "clubworth" or "profitability" or "popularity"
        or "youthdevelopment" or "form" or "gender" or "rivalteam" or "ballid";
    private static bool IsLastYear(string n) => n is "prevleague" or "positionlastyear" or "ischampion";
    private static bool IsLocation(string n) => n is "latitude" or "longitude" or "utcoffset" or "cityid";
    private static bool IsTraits(string n) => n is "trait1vweak" or "trait1vequal" or "trait1vstrong";
    private static bool IsKit(string n) => false;
    private static bool IsDefense(string n) => n is "defensivedepth";
    private static bool IsBuildUp(string n) => n is "buildupplay";
    private static bool IsChance(string n) => false;
    private static bool IsFormation(string n) => n is "favoriteteamsheetid";
    private static bool IsSetPiece(string n) => n is "captainid" or "penaltytakerid" or "freekicktakerid"
        or "leftfreekicktakerid" or "rightfreekicktakerid" or "longkicktakerid"
        or "leftcornerkicktakerid" or "rightcornerkicktakerid" or "throwerleft" or "throwerright"
        || n.StartsWith("cksupport", StringComparison.OrdinalIgnoreCase);
    private static bool IsFlag(string n) => n is "genericbanner" or "isbannerenabled" or "hastifo"
        or "haslargeflag" or "skinnyflags" or "iscompetitionpoleflagenabled"
        or "iscompetitionscarfenabled" or "iscompetitioncrowdcardsenabled"
        or "hassubstitutionboard" or "hasvikingclap" or "hasstandingcrowd";

    // ---------- CM16 Rev. Mod. Extensions tab ----------

    private static bool IsUniqueAdboard(string n) => n.Contains("adboard", StringComparison.OrdinalIgnoreCase);
    private static bool IsUniqueBall(string n) => false;
    private static bool IsUniqueManager(string n) => n is "personalityid";
    private static bool IsUniqueScarf(string n) => false;
    private static bool IsUniqueNet(string n) => n.Contains("goalnet", StringComparison.OrdinalIgnoreCase)
        || n.Contains("stanchion", StringComparison.OrdinalIgnoreCase);

    private static bool IsCm16Field(string n) => IsLogo(n) || IsName(n) || IsStadium(n) || IsManager(n)
        || IsInfo(n) || IsLastYear(n) || IsLocation(n) || IsTraits(n) || IsKit(n)
        || IsDefense(n) || IsBuildUp(n) || IsChance(n) || IsFormation(n) || IsSetPiece(n)
        || IsFlag(n) || IsUniqueAdboard(n) || IsUniqueBall(n) || IsUniqueManager(n)
        || IsUniqueScarf(n) || IsUniqueNet(n);
}
