using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>Preview-first multi-player editor using the classic Player objects.</summary>
internal sealed class Fc26BatchPlayerForm : Form
{
    private readonly ComboBox _team = new ComboBox();
    private readonly ComboBox _league = new ComboBox();
    private readonly ComboBox _ageGroup = new ComboBox();
    private readonly ComboBox _positionGroup = new ComboBox();
    private readonly TextBox _playerGroup = new TextBox();
    private readonly ComboBox _field = new ComboBox();
    private readonly ComboBox _operation = new ComboBox();
    private readonly NumericUpDown _value = new NumericUpDown();
    private readonly DataGridView _preview = new DataGridView();
    private readonly Label _status = new Label();
    private readonly CheckBox _onlyChanged = new CheckBox();
    private readonly ComboBox _playstyle = new ComboBox();
    private List<PreviewRow> _pending = new List<PreviewRow>();

    private static readonly FieldSpec[] Fields =
    {
        new FieldSpec("Overall", "overallrating", 1, 99), new FieldSpec("Potential", "potential", 1, 99),
        new FieldSpec("Acceleration", "acceleration", 1, 99), new FieldSpec("Sprint speed", "sprintspeed", 1, 99),
        new FieldSpec("Finishing", "finishing", 1, 99), new FieldSpec("Shot power", "shotpower", 1, 99),
        new FieldSpec("Long shots", "longshots", 1, 99), new FieldSpec("Short passing", "shortpassing", 1, 99),
        new FieldSpec("Long passing", "longpassing", 1, 99), new FieldSpec("Crossing", "crossing", 1, 99),
        new FieldSpec("Dribbling", "dribbling", 1, 99), new FieldSpec("Ball control", "ballcontrol", 1, 99),
        new FieldSpec("Reactions", "reactions", 1, 99), new FieldSpec("Stamina", "stamina", 1, 99),
        new FieldSpec("Strength", "strength", 1, 99), new FieldSpec("Aggression", "aggression", 1, 99),
        new FieldSpec("Defensive awareness", "marking", 1, 99), new FieldSpec("Composure", "composure", 1, 99),
        new FieldSpec("Standing tackle", "standingtackle", 1, 99),
        new FieldSpec("Sliding tackle", "slidingtackle", 1, 99), new FieldSpec("GK diving", "gkdiving", 1, 99),
        new FieldSpec("GK handling", "gkhandling", 1, 99), new FieldSpec("GK kicking", "gkkicking", 1, 99),
        new FieldSpec("GK positioning", "gkpositioning", 1, 99), new FieldSpec("GK reflexes", "gkreflexes", 1, 99),
        new FieldSpec("Skill moves", "skillmoves", 1, 5), new FieldSpec("Weak foot", "weakfootabilitytypecode", 1, 5),
        new FieldSpec("Height (cm)", "height", 150, 220), new FieldSpec("Weight (kg)", "weight", 45, 130),
        new FieldSpec("Contract valid until", "contractvaliduntil", 2025, 2050),
        new FieldSpec("International reputation", "internationalrep", 1, 5),
        new FieldSpec("Preferred foot", "preferredfoot", 1, 2),
        new FieldSpec("Preferred position", "preferredposition1", 0, 27),
        new FieldSpec("Attacking work rate", "attackingworkrate", 0, 2),
        new FieldSpec("Defensive work rate", "defensiveworkrate", 0, 2),
        new FieldSpec("Boots", "shoetypecode", 0, 9999),
        new FieldSpec("Goalkeeper gloves", "gkglovetypecode", 0, 9999),
        new FieldSpec("Running style 1", "runningcode1", 0, 255),
        new FieldSpec("Running style 2", "runningcode2", 0, 255),
        new FieldSpec("Jersey style", "jerseystylecode", 0, 255)
        ,new FieldSpec("Tactical role 1", "role1", 0, 152)
        ,new FieldSpec("Tactical role 2", "role2", 0, 152)
        ,new FieldSpec("Tactical role 3", "role3", 0, 152)
        ,new FieldSpec("Tactical role 4", "role4", 0, 152)
        ,new FieldSpec("Tactical role 5", "role5", 0, 152)
    };

    private static readonly string[] Playstyles =
    {
        "Finesse Shot", "Power Shot", "Dead Ball", "Chip Shot", "Power Header", "Pinged Pass", "Long Ball Pass", "Tiki Taka",
        "Incisive Pass", "Whipped Pass", "First Touch", "Technical", "Rapid", "Quick Step", "Trickster", "Press Proven",
        "Flair", "Relentless", "Trivela", "Block", "Intercept", "Anticipate", "Slide Tackle", "Bruiser", "Jockey", "Aerial",
        "Acrobatic", "Far Reach", "Footwork", "Cross Claimer", "Rush Out", "Deflector", "1v1 Close Down", "Long Throw"
    };

    internal Fc26BatchPlayerForm()
    {
        Text = "FC26 Batch Player Editor";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1050, 700);
        MinimumSize = new Size(820, 520);
        Icon = Form.ActiveForm?.Icon;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = true };
        _team.Width = 230; _team.DropDownStyle = ComboBoxStyle.DropDownList;
        _team.Items.Add(new TeamChoice(null));
        foreach (Team team in FifaEnvironment.Teams.Cast<Team>().OrderBy(team => team.TeamNameFull)) _team.Items.Add(new TeamChoice(team));
        _team.SelectedIndex = 0;
        _league.Width = 190; _league.DropDownStyle = ComboBoxStyle.DropDownList;
        _league.Items.Add(new LeagueChoice(null));
        foreach (League league in FifaEnvironment.Leagues.Cast<League>().OrderBy(league => league.ToString())) _league.Items.Add(new LeagueChoice(league));
        _league.SelectedIndex = 0;
        _ageGroup.Width = 105; _ageGroup.DropDownStyle = ComboBoxStyle.DropDownList;
        _ageGroup.Items.AddRange(new object[] { "All ages", "Under 21", "21–29", "30 and over" }); _ageGroup.SelectedIndex = 0;
        _positionGroup.Width = 115; _positionGroup.DropDownStyle = ComboBoxStyle.DropDownList;
        _positionGroup.Items.AddRange(new object[] { "All positions", "Goalkeepers", "Defenders", "Midfielders", "Attackers" }); _positionGroup.SelectedIndex = 0;
        _playerGroup.Width = 145;
        _field.Width = 170; _field.DropDownStyle = ComboBoxStyle.DropDownList; _field.Items.AddRange(Fields.Cast<object>().ToArray()); _field.SelectedIndex = 0;
        _operation.Width = 90; _operation.DropDownStyle = ComboBoxStyle.DropDownList; _operation.Items.AddRange(new object[] { "Set", "Add", "Subtract" }); _operation.SelectedIndex = 0;
        _value.Minimum = -9999; _value.Maximum = 9999; _value.Width = 85;
        _onlyChanged.Text = "Show changed only"; _onlyChanged.AutoSize = true; _onlyChanged.Padding = new Padding(8, 5, 0, 0);
        _onlyChanged.CheckedChanged += (_, _) => BindPreview();
        _playstyle.Width = 145; _playstyle.DropDownStyle = ComboBoxStyle.DropDownList;
        _playstyle.Items.AddRange(Playstyles.Cast<object>().ToArray()); _playstyle.SelectedIndex = 0;
        top.Controls.AddRange(new Control[]
        {
            Label("Team"), _team, Label("League"), _league, Label("Group"), _playerGroup,
            Label("Age"), _ageGroup, Label("Position"), _positionGroup,
            Label("Field"), _field, Label("Operation"), _operation, Label("Value"), _value,
            Button("Preview field", Preview), Button("Young preset", (_, _) => PreviewPreset("young")),
            Button("Star preset", (_, _) => PreviewPreset("star")), Button("Position preset", (_, _) => PreviewPreset("position")),
            Button("Age curve", (_, _) => PreviewPreset("age")), Button("Physical +5", (_, _) => PreviewPreset("physical")),
            Button("Technical +5", (_, _) => PreviewPreset("technical")), Label("PlayStyle"), _playstyle,
            Button("Add", (_, _) => PreviewPlaystyle(true, false)), Button("Remove", (_, _) => PreviewPlaystyle(false, false)),
            Button("Add +", (_, _) => PreviewPlaystyle(true, true)), Button("Remove +", (_, _) => PreviewPlaystyle(false, true)),
            Button("Export Excel CSV", ExportPlayers), Button("Import/Create FC25/Excel CSV", ImportPlayers),
            Button("Apply staged", Apply), _onlyChanged
        });
        _preview.Dock = DockStyle.Fill; _preview.ReadOnly = true; _preview.AllowUserToAddRows = false;
        _preview.AllowUserToDeleteRows = false; _preview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _preview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _status.Dock = DockStyle.Bottom; _status.Height = 25; _status.Padding = new Padding(6, 4, 0, 0);
        _status.Text = "Preview first. Apply changes only the loaded CM26 session; File > Save performs validation, backup and direct commit.";
        Controls.Add(_preview); Controls.Add(_status); Controls.Add(top);
    }

    private void Preview(object sender, EventArgs e)
    {
        try
        {
            var spec = (FieldSpec)_field.SelectedItem;
            var property = typeof(Player).GetProperty(spec.Property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? throw new MissingMemberException("Player field is not mapped: " + spec.Property);
            var players = SelectedPlayers();
            _pending = players.Select(player =>
            {
                var before = Convert.ToInt32(property.GetValue(player, null));
                var raw = _operation.Text == "Add" ? before + Decimal.ToInt32(_value.Value)
                    : _operation.Text == "Subtract" ? before - Decimal.ToInt32(_value.Value) : Decimal.ToInt32(_value.Value);
                var after = Math.Max(spec.Minimum, Math.Min(spec.Maximum, raw));
                return new PreviewRow(player, spec.Name, property, before, after);
            }).ToList();
            BindPreview();
            _status.Text = _pending.Count.ToString("N0") + " player(s) previewed; " + _pending.Count(row => row.Before != row.After).ToString("N0") + " would change.";
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Batch Player preview", ex, "No batch values were applied. Review the selected players and filters, then retry."); }
    }

    private Player[] SelectedPlayers()
    {
        var selectedTeam = ((TeamChoice)_team.SelectedItem).Team;
        var selectedLeague = ((LeagueChoice)_league.SelectedItem).League;
        var query = _playerGroup.Text.Trim();
        return FifaEnvironment.Players.Cast<Player>()
            .Where(player => selectedTeam == null || player.GetClub() == selectedTeam)
            .Where(player => selectedLeague == null || player.GetClub()?.League == selectedLeague)
            .Where(player => string.IsNullOrWhiteSpace(query) || player.Id.ToString().Contains(query) ||
                player.ToString().IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            .Where(player => AgeMatches(player) && PositionMatches(player.preferredposition1)).ToArray();
    }

    private bool AgeMatches(Player player)
    {
        var age = DateTime.Today.Year - player.birthdate.Year;
        if (player.birthdate.Date > DateTime.Today.AddYears(-age)) age--;
        return _ageGroup.SelectedIndex switch { 1 => age < 21, 2 => age >= 21 && age <= 29, 3 => age >= 30, _ => true };
    }

    private bool PositionMatches(int role) => _positionGroup.SelectedIndex switch
    {
        1 => role == 0, 2 => role >= 1 && role <= 8, 3 => role >= 9 && role <= 19,
        4 => role >= 20 && role <= 27, _ => true
    };

    private void PreviewPreset(string preset)
    {
        try
        {
            _pending = new List<PreviewRow>();
            foreach (var player in SelectedPlayers())
            {
                if (preset == "young")
                {
                    Target(player, "potential", Math.Max(player.potential, Math.Min(99, player.overallrating + 8)), "Young potential");
                    foreach (var field in new[] { "acceleration", "sprintspeed", "reactions", "stamina", "ballcontrol" }) Delta(player, field, 3, "Young development");
                }
                else if (preset == "star")
                {
                    Target(player, "overallrating", Math.Max(player.overallrating, 88), "Star overall");
                    Target(player, "potential", Math.Max(player.potential, 90), "Star potential");
                    foreach (var field in new[] { "reactions", "ballcontrol", "shortpassing", "stamina", "strength" }) Target(player, field, 85, "Star core");
                }
                else if (preset == "physical")
                    foreach (var field in new[] { "acceleration", "sprintspeed", "stamina", "strength", "aggression" }) Delta(player, field, 5, "Physical +5");
                else if (preset == "technical")
                    foreach (var field in new[] { "ballcontrol", "dribbling", "shortpassing", "longpassing", "crossing", "finishing" }) Delta(player, field, 5, "Technical +5");
                else if (preset == "age")
                {
                    var age = DateTime.Today.Year - player.birthdate.Year;
                    if (player.birthdate.Date > DateTime.Today.AddYears(-age)) age--;
                    if (age <= 20)
                    {
                        Target(player, "potential", Math.Max(player.potential, Math.Min(99, player.overallrating + 10)), "Age curve");
                        foreach (var field in new[] { "acceleration", "sprintspeed", "stamina", "reactions" }) Delta(player, field, 3, "Age curve");
                    }
                    else if (age >= 31)
                    {
                        foreach (var field in new[] { "acceleration", "sprintspeed", "stamina" }) Delta(player, field, -2, "Age curve");
                        foreach (var field in new[] { "reactions", "shortpassing" }) Delta(player, field, 1, "Age experience");
                    }
                }
                else PositionPreset(player);
            }
            BindPreview();
            _status.Text = _pending.Select(row => row.Player.Id).Distinct().Count().ToString("N0") + " player(s), " +
                _pending.Count(row => row.Before != row.After).ToString("N0") + " field change(s) previewed.";
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Batch Player development preview", ex, "No development values were applied. Review the age curve and selection, then retry."); }
    }

    private void PreviewPlaystyle(bool add, bool plus)
    {
        try
        {
            var index = Math.Max(0, _playstyle.SelectedIndex);
            var propertyName = plus ? (index < 32 ? "icontrait1" : "icontrait2") : (index < 32 ? "trait1" : "trait2");
            var property = typeof(Player).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? throw new MissingMemberException("FC26 PlayStyle mask is unavailable: " + propertyName);
            var bit = unchecked((int)(1u << (index % 32)));
            _pending = SelectedPlayers().Select(player =>
            {
                var before = Convert.ToInt32(property.GetValue(player, null));
                var after = add ? before | bit : before & ~bit;
                return new PreviewRow(player, (plus ? "PlayStyle+ · " : "PlayStyle · ") + Playstyles[index], property, before, after);
            }).ToList();
            BindPreview();
            _status.Text = _pending.Count(row => row.Before != row.After).ToString("N0") + " PlayStyle mask change(s) previewed.";
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Batch PlayStyle preview", ex, "No PlayStyle values were applied. Review the three-state selections, then retry."); }
    }

    private void PositionPreset(Player player)
    {
        string[] fields = player.preferredposition1 == 0
            ? new[] { "gkdiving", "gkhandling", "gkkicking", "gkpositioning", "gkreflexes" }
            : player.preferredposition1 <= 8
                ? new[] { "marking", "standingtackle", "slidingtackle", "strength", "reactions" }
                : player.preferredposition1 <= 19
                    ? new[] { "shortpassing", "longpassing", "ballcontrol", "dribbling", "stamina" }
                    : new[] { "finishing", "shotpower", "acceleration", "sprintspeed", "ballcontrol" };
        foreach (var field in fields) Delta(player, field, 5, "Position preset");
    }

    private void Delta(Player player, string propertyName, int amount, string label)
    {
        var property = typeof(Player).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property == null || property.PropertyType != typeof(int)) return;
        var before = Convert.ToInt32(property.GetValue(player, null));
        _pending.Add(new PreviewRow(player, label + " · " + propertyName, property, before, Math.Max(1, Math.Min(99, before + amount))));
    }

    private void Target(Player player, string propertyName, int target, string label)
    {
        var property = typeof(Player).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property == null || property.PropertyType != typeof(int)) return;
        var before = Convert.ToInt32(property.GetValue(player, null));
        _pending.Add(new PreviewRow(player, label + " · " + propertyName, property, before, Math.Max(1, Math.Min(99, target))));
    }

    private void BindPreview()
    {
        _preview.DataSource = _pending.Where(row => !_onlyChanged.Checked || row.Before != row.After).Select(row => new
        {
            PlayerId = row.Player.Id,
            Player = row.Player.ToString(),
            Club = row.Player.GetClub()?.TeamNameFull ?? "Free Agent",
            Field = row.Field,
            Before = row.Before,
            After = row.After,
            Changed = row.Before != row.After
        }).ToArray();
    }

    private void Apply(object sender, EventArgs e)
    {
        if (_pending.Count == 0) { MessageBox.Show(this, "Create a preview first.", Text); return; }
        var changed = _pending.Where(row => row.Before != row.After).ToArray();
        if (changed.Length == 0) { _status.Text = "No values need changing."; return; }
        if (MessageBox.Show(this, "Stage " + changed.Length.ToString("N0") + " player field change(s)?", Text,
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var row in changed) row.Property.SetValue(row.Player, row.After, null);
        _status.Text = changed.Length.ToString("N0") + " change(s) staged. Review players, then use File > Save.";
        _pending.Clear(); BindPreview();
    }

    private void ExportPlayers(object sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Filter = "Excel-compatible CSV (*.csv)|*.csv", FileName = "CM26_players.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var lines = new List<string> { "playerid,firstname,lastname,nationality,teamid,position,overall,potential,birthdate,height,weight" };
        lines.AddRange(SelectedPlayers().Select(player => string.Join(",", new[]
        {
            player.Id.ToString(CultureInfo.InvariantCulture), Csv(player.firstname), Csv(player.lastname), player.nationality.ToString(CultureInfo.InvariantCulture),
            (player.GetClub()?.Id ?? -1).ToString(CultureInfo.InvariantCulture), player.preferredposition1.ToString(CultureInfo.InvariantCulture),
            player.overallrating.ToString(CultureInfo.InvariantCulture), player.potential.ToString(CultureInfo.InvariantCulture),
            player.birthdate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), player.height.ToString(CultureInfo.InvariantCulture), player.weight.ToString(CultureInfo.InvariantCulture)
        })));
        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
        _status.Text = (lines.Count - 1).ToString("N0") + " player(s) exported for Excel/FC25 mapping.";
    }

    private void ImportPlayers(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "Excel/FC25 CSV (*.csv)|*.csv|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var lines = File.ReadAllLines(dialog.FileName);
            if (lines.Length < 2) throw new InvalidDataException("The CSV contains no player rows.");
            var header = ParseCsv(lines[0]);
            var required = new[] { "firstname", "lastname", "nationality", "teamid", "position", "overall", "potential", "birthdate", "height", "weight" };
            if (required.Any(name => !header.Contains(name, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidDataException("CSV header must contain: " + string.Join(", ", required));
            var indexes = header.Select((name, index) => new { name, index }).ToDictionary(item => item.name, item => item.index, StringComparer.OrdinalIgnoreCase);
            var drafts = new List<PlayerDraft>();
            foreach (var line in lines.Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                var values = ParseCsv(line); string Value(string name) => indexes[name] < values.Length ? values[indexes[name]].Trim() : string.Empty;
                if (!int.TryParse(Value("nationality"), out var nationality) || !int.TryParse(Value("teamid"), out var teamId) ||
                    !int.TryParse(Value("position"), out var position) || !int.TryParse(Value("overall"), out var overall) ||
                    !int.TryParse(Value("potential"), out var potential) || !DateTime.TryParse(Value("birthdate"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthdate) ||
                    !int.TryParse(Value("height"), out var height) || !int.TryParse(Value("weight"), out var weight))
                    throw new InvalidDataException("Invalid numeric/date value at CSV line " + (drafts.Count + 2) + ".");
                var team = FifaEnvironment.Teams.SearchId(teamId) as Team;
                var country = FifaEnvironment.Countries.SearchId(nationality) as Country;
                if (team == null || country == null) throw new InvalidDataException("Unknown team/nationality ID at CSV line " + (drafts.Count + 2) + ".");
                drafts.Add(new PlayerDraft(Value("firstname"), Value("lastname"), team, country, Math.Max(0, Math.Min(27, position)),
                    Math.Max(1, Math.Min(99, overall)), Math.Max(1, Math.Min(99, potential)), birthdate,
                    Math.Max(150, Math.Min(220, height)), Math.Max(45, Math.Min(130, weight))));
            }
            var duplicates = drafts.GroupBy(draft => (draft.First + " " + draft.Last).Trim(), StringComparer.CurrentCultureIgnoreCase).Where(group => group.Count() > 1).ToArray();
            if (MessageBox.Show(this, "Validated " + drafts.Count + " player row(s). " + duplicates.Length +
                " duplicate-name group(s) detected. Create new FC26 player IDs and team links?",
                "FC25/Excel conversion preview", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            foreach (var draft in drafts)
            {
                var player = FifaEnvironment.Players.CreateNewId() as Player ?? throw new InvalidOperationException("No free FC26 player ID remains.");
                player.firstname = draft.First; player.lastname = draft.Last; player.commonname = string.Empty; player.playerjerseyname = draft.Last;
                player.Country = draft.Country; player.birthdate = draft.BirthDate; player.height = draft.Height; player.weight = draft.Weight;
                player.preferredposition1 = draft.Position; player.overallrating = draft.Overall; player.potential = Math.Max(draft.Overall, draft.Potential);
                player.joindate = DateTime.Today; player.contractvaliduntil = DateTime.Today.Year + 3;
                Fc26SnapshotLoader.StageNewEntity("player", player); Fc26SnapshotLoader.StageNewPlayerNames(player);
                var link = draft.Team.AddTeamPlayer(player); link.position = draft.Position; Fc26SnapshotLoader.StageNewTeamPlayerLink(link);
            }
            _status.Text = drafts.Count + " FC25/Excel player row(s) converted and staged with new FC26 IDs.";
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Import/Create players", ex, "No unvalidated batch player row was accepted."); }
    }

    private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    private static string[] ParseCsv(string line)
    {
        var values = new List<string>(); var value = new StringBuilder(); var quoted = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"' && quoted && index + 1 < line.Length && line[index + 1] == '"') { value.Append('"'); index++; }
            else if (character == '"') quoted = !quoted;
            else if (character == ',' && !quoted) { values.Add(value.ToString()); value.Clear(); }
            else value.Append(character);
        }
        values.Add(value.ToString()); return values.ToArray();
    }

    private static Label Label(string text) => new Label { Text = text, AutoSize = true, Padding = new Padding(5, 6, 0, 0) };
    private static Button Button(string text, EventHandler handler) { var button = new Button { Text = text, AutoSize = true }; button.Click += handler; return button; }

    private sealed class PreviewRow
    {
        internal PreviewRow(Player player, string field, PropertyInfo property, int before, int after) { Player = player; Field = field; Property = property; Before = before; After = after; }
        internal Player Player { get; } internal string Field { get; } internal PropertyInfo Property { get; } internal int Before { get; } internal int After { get; }
    }
    private sealed class FieldSpec
    {
        internal FieldSpec(string name, string property, int minimum, int maximum) { Name = name; Property = property; Minimum = minimum; Maximum = maximum; }
        internal string Name { get; } internal string Property { get; } internal int Minimum { get; } internal int Maximum { get; }
        public override string ToString() => Name;
    }
    private sealed class TeamChoice
    {
        internal TeamChoice(Team team) { Team = team; } internal Team Team { get; }
        public override string ToString() => Team == null ? "All loaded players" : Team.TeamNameFull;
    }
    private sealed class LeagueChoice
    {
        internal LeagueChoice(League league) { League = league; } internal League League { get; }
        public override string ToString() => League == null ? "All leagues" : League.ToString();
    }
    private sealed class PlayerDraft
    {
        internal PlayerDraft(string first, string last, Team team, Country country, int position, int overall, int potential, DateTime birthDate, int height, int weight)
        { First = first; Last = last; Team = team; Country = country; Position = position; Overall = overall; Potential = potential; BirthDate = birthDate; Height = height; Weight = weight; }
        internal string First { get; } internal string Last { get; } internal Team Team { get; } internal Country Country { get; }
        internal int Position { get; } internal int Overall { get; } internal int Potential { get; } internal DateTime BirthDate { get; }
        internal int Height { get; } internal int Weight { get; }
    }
}
