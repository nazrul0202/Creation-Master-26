using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
}
