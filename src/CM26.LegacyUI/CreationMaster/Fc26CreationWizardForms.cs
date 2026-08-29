using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>
/// The small, guided draft used by the FC26 Create menu.  It deliberately
/// collects only the information needed to make a league playable; all
/// database rows, links, squad placeholders and Compdata are created by the
/// host after the dialog closes.
/// </summary>
internal sealed class Fc26LeagueCreationDraft
{
    internal Country Country { get; set; }
    internal string LeagueName { get; set; } = string.Empty;
    internal int Level { get; set; } = 1;
    internal IReadOnlyList<string> TeamNames { get; set; } = Array.Empty<string>();
}

internal sealed class Fc26LeagueCreationDialog : Form
{
    private readonly ComboBox _country = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly TextBox _leagueName = new TextBox { Width = 300 };
    private readonly NumericUpDown _level = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 1, Width = 90 };
    private readonly TextBox _teamName = new TextBox { Width = 300 };
    private readonly ListBox _teams = new ListBox { Width = 390, Height = 130 };

    internal Fc26LeagueCreationDialog(IEnumerable<Country> countries)
    {
        Text = "Create New League";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        // Keep the guided flow compact, while leaving enough room for the
        // team entry row and its explicit "Create Another Team" action on
        // classic 100% DPI layouts.
        ClientSize = new Size(690, 430);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var countryItems = (countries ?? Array.Empty<Country>()).Where(value => value != null)
            .OrderBy(value => value.DatabaseName, StringComparer.OrdinalIgnoreCase).ToArray();
        _country.DataSource = countryItems;
        _country.DisplayMember = "DatabaseName";
        if (_country.Items.Count > 0) _country.SelectedIndex = 0;

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 2, RowCount = 6,
            AutoSize = false
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        fields.Controls.Add(new Label { Text = "Country", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        fields.Controls.Add(_country, 1, 0);
        fields.Controls.Add(new Label { Text = "League name", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        fields.Controls.Add(_leagueName, 1, 1);
        fields.Controls.Add(new Label { Text = "Competition level", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
        fields.Controls.Add(_level, 1, 2);

        var teamEntry = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        teamEntry.Controls.Add(new Label { Text = "Team", AutoSize = true, Padding = new Padding(0, 5, 8, 0) });
        teamEntry.Controls.Add(_teamName);
        var add = new Button { Text = "Create Another Team", AutoSize = true };
        add.Click += (_, _) => AddTeam();
        teamEntry.Controls.Add(add);
        fields.Controls.Add(new Label { Text = "Teams (minimum 2)", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
        fields.Controls.Add(teamEntry, 1, 3);

        var listPanel = new Panel { Dock = DockStyle.Fill };
        _teams.Dock = DockStyle.Fill;
        var remove = new Button { Text = "Remove Selected", Dock = DockStyle.Bottom, Height = 27 };
        remove.Click += (_, _) =>
        {
            if (_teams.SelectedIndex >= 0) _teams.Items.RemoveAt(_teams.SelectedIndex);
        };
        listPanel.Controls.Add(_teams); listPanel.Controls.Add(remove);
        fields.Controls.Add(listPanel, 1, 4);

        var finish = new Button { Text = "Finish & Save", AutoSize = true };
        finish.Click += (_, _) => Finish();
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.Add(cancel); buttons.Controls.Add(finish);
        fields.Controls.Add(buttons, 1, 5);
        Controls.Add(fields);
        AcceptButton = finish;
        CancelButton = cancel;
    }

    internal Fc26LeagueCreationDraft Draft { get; private set; }

    private void AddTeam()
    {
        var name = _teamName.Text.Trim();
        if (name.Length == 0) { MessageBox.Show(this, "Enter a team name first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        if (_teams.Items.Cast<object>().Any(value => string.Equals(Convert.ToString(value), name, StringComparison.OrdinalIgnoreCase)))
        { MessageBox.Show(this, "That team name is already in this league.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        _teams.Items.Add(name); _teamName.Clear(); _teamName.Focus();
    }

    private void Finish()
    {
        if (!(_country.SelectedItem is Country country)) { MessageBox.Show(this, "Choose the league country.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        // Treat text still in the entry box as the final team.  This avoids
        // the easy-to-miss extra click when a user types the second team and
        // immediately chooses Finish & Save.
        if (!string.IsNullOrWhiteSpace(_teamName.Text)) AddTeam();
        var name = _leagueName.Text.Trim();
        var names = _teams.Items.Cast<object>().Select(Convert.ToString).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray();
        if (name.Length == 0) { MessageBox.Show(this, "Enter a league name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (names.Length < 2) { MessageBox.Show(this, "Create at least two teams before finishing.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (names.GroupBy(value => value, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        { MessageBox.Show(this, "Team names must be unique within the new league.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Draft = new Fc26LeagueCreationDraft { Country = country, LeagueName = name, Level = Decimal.ToInt32(_level.Value), TeamNames = names };
        DialogResult = DialogResult.OK;
    }
}

internal sealed class Fc26StandaloneTeamDraft
{
    internal string TeamName { get; set; } = string.Empty;
    internal Country Country { get; set; }
    internal League League { get; set; }
    internal int FoundationYear { get; set; }
    internal int StadiumCapacity { get; set; }
    internal int ClubWorth { get; set; }
    internal int TransferBudget { get; set; }
}

/// <summary>Simple DBM/Deco-style team creation with the league relationship
/// visible before the record is created.</summary>
internal sealed class Fc26StandaloneTeamDialog : Form
{
    private readonly TextBox _name = new TextBox { Width = 290 };
    private readonly ComboBox _country = new ComboBox { Width = 290, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _league = new ComboBox { Width = 290, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _foundation = new NumericUpDown { Minimum = 0, Maximum = 2100, Value = 2026, Width = 100 };
    private readonly NumericUpDown _capacity = new NumericUpDown { Minimum = 0, Maximum = 200000, Value = 15000, Increment = 100, Width = 100 };
    private readonly NumericUpDown _worth = new NumericUpDown { Minimum = 0, Maximum = 2000000000, Value = 1000000, Increment = 10000, Width = 130 };
    private readonly NumericUpDown _budget = new NumericUpDown { Minimum = 0, Maximum = 2000000000, Value = 1000000, Increment = 10000, Width = 130 };

    internal Fc26StandaloneTeamDialog(IEnumerable<Country> countries, IEnumerable<League> leagues, League selectedLeague)
    {
        Text = "Create New Team"; FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(510, 335); MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        var countryItems = (countries ?? Array.Empty<Country>()).Where(value => value != null).OrderBy(value => value.DatabaseName, StringComparer.OrdinalIgnoreCase).ToArray();
        var leagueItems = (leagues ?? Array.Empty<League>()).Where(value => value != null).OrderBy(value => value.ToString(), StringComparer.OrdinalIgnoreCase).ToArray();
        _country.DataSource = countryItems; _country.DisplayMember = "DatabaseName";
        _league.DataSource = leagueItems;
        if (selectedLeague != null) _league.SelectedItem = selectedLeague;
        if (_league.SelectedItem is League current) _country.SelectedItem = current.Country;
        _league.SelectedIndexChanged += (_, _) => { if (_league.SelectedItem is League value && value.Country != null) _country.SelectedItem = value.Country; };

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 2, RowCount = 8 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Add(layout, "Team name", _name, 0); Add(layout, "Country", _country, 1); Add(layout, "League", _league, 2);
        Add(layout, "Foundation year", _foundation, 3); Add(layout, "Stadium capacity", _capacity, 4);
        Add(layout, "Club worth", _worth, 5); Add(layout, "Transfer budget", _budget, 6);
        var create = new Button { Text = "Create Team", AutoSize = true }; create.Click += (_, _) => Finish();
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft }; buttons.Controls.Add(cancel); buttons.Controls.Add(create);
        layout.Controls.Add(buttons, 1, 7); Controls.Add(layout); AcceptButton = create; CancelButton = cancel;
    }

    internal Fc26StandaloneTeamDraft Draft { get; private set; }
    private static void Add(TableLayoutPanel layout, string label, Control control, int row)
    { layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row); layout.Controls.Add(control, 1, row); }
    private void Finish()
    {
        if (_name.Text.Trim().Length == 0) { MessageBox.Show(this, "Enter a team name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!(_country.SelectedItem is Country country) || !(_league.SelectedItem is League league)) { MessageBox.Show(this, "Choose both country and league.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Draft = new Fc26StandaloneTeamDraft { TeamName = _name.Text.Trim(), Country = country, League = league,
            FoundationYear = Decimal.ToInt32(_foundation.Value), StadiumCapacity = Decimal.ToInt32(_capacity.Value),
            ClubWorth = Decimal.ToInt32(_worth.Value), TransferBudget = Decimal.ToInt32(_budget.Value) };
        DialogResult = DialogResult.OK;
    }
}
