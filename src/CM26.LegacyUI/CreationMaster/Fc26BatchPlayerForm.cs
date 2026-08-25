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
    private readonly ComboBox _field = new ComboBox();
    private readonly ComboBox _operation = new ComboBox();
    private readonly NumericUpDown _value = new NumericUpDown();
    private readonly DataGridView _preview = new DataGridView();
    private readonly Label _status = new Label();
    private readonly CheckBox _onlyChanged = new CheckBox();
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
        new FieldSpec("Marking", "marking", 1, 99), new FieldSpec("Standing tackle", "standingtackle", 1, 99),
        new FieldSpec("Sliding tackle", "slidingtackle", 1, 99), new FieldSpec("GK diving", "gkdiving", 1, 99),
        new FieldSpec("GK handling", "gkhandling", 1, 99), new FieldSpec("GK kicking", "gkkicking", 1, 99),
        new FieldSpec("GK positioning", "gkpositioning", 1, 99), new FieldSpec("GK reflexes", "gkreflexes", 1, 99),
        new FieldSpec("Skill moves", "skillmoves", 1, 5), new FieldSpec("Weak foot", "weakfootabilitytypecode", 1, 5),
        new FieldSpec("Height (cm)", "height", 150, 220), new FieldSpec("Weight (kg)", "weight", 45, 130),
        new FieldSpec("Contract valid until", "contractvaliduntil", 2025, 2050),
        new FieldSpec("International reputation", "internationalrep", 1, 5),
        new FieldSpec("Preferred foot", "preferredfoot", 1, 2)
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
        _field.Width = 170; _field.DropDownStyle = ComboBoxStyle.DropDownList; _field.Items.AddRange(Fields.Cast<object>().ToArray()); _field.SelectedIndex = 0;
        _operation.Width = 90; _operation.DropDownStyle = ComboBoxStyle.DropDownList; _operation.Items.AddRange(new object[] { "Set", "Add", "Subtract" }); _operation.SelectedIndex = 0;
        _value.Minimum = -9999; _value.Maximum = 9999; _value.Width = 85;
        _onlyChanged.Text = "Show changed only"; _onlyChanged.AutoSize = true; _onlyChanged.Padding = new Padding(8, 5, 0, 0);
        _onlyChanged.CheckedChanged += (_, _) => BindPreview();
        top.Controls.AddRange(new Control[]
        {
            Label("Players"), _team, Label("Field"), _field, Label("Operation"), _operation,
            Label("Value"), _value, Button("Preview", Preview), Button("Apply staged", Apply), _onlyChanged
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
            var selectedTeam = ((TeamChoice)_team.SelectedItem).Team;
            var players = FifaEnvironment.Players.Cast<Player>().Where(player => selectedTeam == null || player.GetClub() == selectedTeam).ToArray();
            _pending = players.Select(player =>
            {
                var before = Convert.ToInt32(property.GetValue(player, null));
                var raw = _operation.Text == "Add" ? before + Decimal.ToInt32(_value.Value)
                    : _operation.Text == "Subtract" ? before - Decimal.ToInt32(_value.Value) : Decimal.ToInt32(_value.Value);
                var after = Math.Max(spec.Minimum, Math.Min(spec.Maximum, raw));
                return new PreviewRow(player, property, before, after);
            }).ToList();
            BindPreview();
            _status.Text = _pending.Count.ToString("N0") + " player(s) previewed; " + _pending.Count(row => row.Before != row.After).ToString("N0") + " would change.";
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void BindPreview()
    {
        _preview.DataSource = _pending.Where(row => !_onlyChanged.Checked || row.Before != row.After).Select(row => new
        {
            PlayerId = row.Player.Id,
            Player = row.Player.ToString(),
            Club = row.Player.GetClub()?.TeamNameFull ?? "Free Agent",
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
        internal PreviewRow(Player player, PropertyInfo property, int before, int after) { Player = player; Property = property; Before = before; After = after; }
        internal Player Player { get; } internal PropertyInfo Property { get; } internal int Before { get; } internal int After { get; }
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
}
