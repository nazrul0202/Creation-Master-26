using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>
/// Curated, relationship-aware editors for data introduced after the original
/// Creation Master UI. Only explicitly named football concepts are surfaced;
/// database column names and unrelated engine tables never appear on screen.
/// </summary>
internal sealed class CmStyleDetailsPanel : Panel
{
    private readonly DetailSection _section;
    private int _entityId;

    internal CmStyleDetailsPanel(DetailSection section)
    {
        _section = section;
        Dock = DockStyle.Fill;
        AutoScroll = true;
        BackColor = SystemColors.Control;
    }

    internal void Reload(int entityId)
    {
        _entityId = entityId;
        SuspendLayout();
        Controls.Clear();
        var top = 8;
        foreach (var definition in Definitions.For(_section))
        {
            var table = Fc26SnapshotLoader.DetailTable(definition.Table);
            if (table == null) continue;
            var rows = Enumerable.Range(0, table.Rows.Count)
                .Where(row => definition.MatchFields.Length == 0 ||
                    definition.MatchFields.Any(field => Parse(table.Value(row, field)) == entityId))
                .ToArray();
            if (rows.Length == 0 && !definition.ShowWhenEmpty) continue;
            var editor = new DetailCard(definition, table, rows);
            editor.Location = new Point(8, top);
            editor.Width = Math.Max(760, ClientSize.Width - 36);
            editor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(editor);
            top += editor.Height + 8;
        }
        if (Controls.Count == 0)
        {
            Controls.Add(new Label
            {
                Text = "No additional records are stored for this item.",
                AutoSize = true,
                Location = new Point(18, 20),
                ForeColor = SystemColors.GrayText
            });
        }
        ResumeLayout(true);
    }

    private static int Parse(string value) => int.TryParse(value, out var result) ? result : 0;

    private sealed class DetailCard : GroupBox
    {
        private readonly DetailTable _definition;
        private readonly SnapshotDetailTable _table;
        private readonly int[] _rows;
        private readonly ListBox _records = new();
        private readonly Dictionary<string, Control> _editors = new(StringComparer.OrdinalIgnoreCase);
        private bool _loading;

        internal DetailCard(DetailTable definition, SnapshotDetailTable table, int[] rows)
        {
            _definition = definition;
            _table = table;
            _rows = rows;
            Text = definition.Title;
            Font = SystemFonts.MessageBoxFont;
            Height = Math.Max(122, 42 + ((definition.Fields.Length + 1) / 2 * 29));

            _records.Location = new Point(10, 22);
            _records.Size = new Size(190, Height - 34);
            _records.IntegralHeight = false;
            _records.SelectedIndexChanged += (_, _) => LoadSelected();
            Controls.Add(_records);

            for (var i = 0; i < definition.Fields.Length; i++)
            {
                var field = definition.Fields[i];
                if (_table.Column(field.Name) < 0) continue;
                var column = i % 2;
                var row = i / 2;
                var x = 216 + column * 275;
                var y = 25 + row * 29;
                Controls.Add(new Label
                {
                    Text = field.Label,
                    Location = new Point(x, y + 3),
                    Size = new Size(112, 19),
                    TextAlign = ContentAlignment.MiddleRight,
                    AutoEllipsis = true
                });
                var editor = CreateEditor(field, new Point(x + 118, y));
                Controls.Add(editor);
                _editors[field.Name] = editor;
            }

            foreach (var row in rows)
                _records.Items.Add(new RowChoice(row, Describe(row)));
            if (_records.Items.Count > 0) _records.SelectedIndex = 0;
            else
            {
                _records.Items.Add("Not stored for this item");
                _records.Enabled = false;
                foreach (var editor in _editors.Values) editor.Enabled = false;
            }
        }

        private Control CreateEditor(DetailField field, Point location)
        {
            if (field.Boolean)
            {
                var check = new CheckBox { Location = new Point(location.X, location.Y + 2), Size = new Size(145, 20) };
                check.CheckedChanged += (_, _) => Commit(field.Name, check.Checked ? "1" : "0");
                return check;
            }
            var references = ReferenceChoices(field.Name);
            if (references.Count > 0)
            {
                var combo = new ComboBox
                {
                    Location = location, Size = new Size(145, 21), DropDownStyle = ComboBoxStyle.DropDownList,
                    DisplayMember = nameof(ReferenceChoice.Name), ValueMember = nameof(ReferenceChoice.Id),
                    DataSource = references
                };
                combo.SelectedValueChanged += (_, _) =>
                {
                    if (combo.SelectedValue is int id) Commit(field.Name, id.ToString(CultureInfo.InvariantCulture));
                };
                return combo;
            }
            var text = new TextBox { Location = location, Size = new Size(145, 20), BorderStyle = BorderStyle.FixedSingle };
            text.Leave += (_, _) => Commit(field.Name, text.Text.Trim());
            return text;
        }

        private void LoadSelected()
        {
            if (_records.SelectedItem is not RowChoice selected) return;
            _loading = true;
            foreach (var field in _definition.Fields)
            {
                if (!_editors.TryGetValue(field.Name, out var editor)) continue;
                var value = _table.Value(selected.RowIndex, field.Name);
                switch (editor)
                {
                    case CheckBox check: check.Checked = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase); break;
                    case ComboBox combo when int.TryParse(value, out var id): combo.SelectedValue = id; break;
                    case TextBox text: text.Text = value; break;
                }
            }
            _loading = false;
        }

        private void Commit(string field, string value)
        {
            if (_loading || _records.SelectedItem is not RowChoice selected) return;
            Fc26SnapshotLoader.StageDetailValue(_table.Name, selected.RowIndex, field, value);
        }

        private string Describe(int row)
        {
            foreach (var field in _definition.SummaryFields)
            {
                var value = _table.Value(row, field);
                var reference = ReferenceChoices(field).FirstOrDefault(item => item.Id == Parse(value));
                if (reference != null) value = reference.Name;
                if (!string.IsNullOrWhiteSpace(value) && value != "0") return value;
            }
            return _definition.ItemName + " " + (Array.IndexOf(_rows, row) + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static List<ReferenceChoice> ReferenceChoices(string field)
        {
            System.Collections.IEnumerable? source = null;
            if (field.Contains("playerid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Players;
            else if (field.Contains("teamid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Teams;
            else if (field.Contains("leagueid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Leagues;
            else if (field.Contains("stadiumid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Stadiums;
            else if (field.Contains("formationid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Formations;
            else if (field.Contains("ballid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Balls;
            else if (field.Contains("competitionid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.CompetitionObjects;
            else if (field.Contains("refereeid", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Referees;
            else if (field.Contains("countryid", StringComparison.OrdinalIgnoreCase) || field.Contains("nationid", StringComparison.OrdinalIgnoreCase) || field.Equals("nationality", StringComparison.OrdinalIgnoreCase)) source = FifaEnvironment.Countries;
            else if (field.Equals("adsponserid", StringComparison.OrdinalIgnoreCase))
            {
                var sponsors = Fc26SnapshotLoader.DetailTable("sponsors");
                var choices = new List<ReferenceChoice> { new(0, "None") };
                if (sponsors != null)
                    for (var row = 0; row < sponsors.Rows.Count; row++)
                        if (int.TryParse(sponsors.Value(row, "adsponserid"), out var id))
                            choices.Add(new ReferenceChoice(id, sponsors.Value(row, "name")));
                return choices;
            }
            if (source == null) return new List<ReferenceChoice>();
            var result = new List<ReferenceChoice> { new(0, "None") };
            foreach (var item in source)
            {
                var property = item?.GetType().GetProperty("Id");
                var id = property?.GetValue(item) is int value ? value : int.MinValue;
                if (id != int.MinValue) result.Add(new ReferenceChoice(id, item?.ToString() ?? id.ToString(CultureInfo.InvariantCulture)));
            }
            return result;
        }
    }

    private sealed class RowChoice
    {
        internal RowChoice(int rowIndex, string name) { RowIndex = rowIndex; Name = name; }
        internal int RowIndex { get; }
        internal string Name { get; }
        public override string ToString() => Name;
    }
    private sealed class ReferenceChoice
    {
        internal ReferenceChoice(int id, string name) { Id = id; Name = name; }
        public int Id { get; }
        public string Name { get; }
    }
}

internal enum DetailSection { Country, League, Team, Player, Manager, Stadium, Kit, Competition, Formation, Ball, Boot, Gloves, Sponsor, Adboard, Broadcast, Referee }
internal sealed class DetailField
{
    internal DetailField(string name, string label, bool boolean = false) { Name = name; Label = label; Boolean = boolean; }
    internal string Name { get; }
    internal string Label { get; }
    internal bool Boolean { get; }
}
internal sealed class DetailTable
{
    internal DetailTable(string title, string table, string itemName, string[] matchFields,
        string[] summaryFields, DetailField[] fields, bool showWhenEmpty = false)
    {
        Title = title; Table = table; ItemName = itemName; MatchFields = matchFields;
        SummaryFields = summaryFields; Fields = fields; ShowWhenEmpty = showWhenEmpty;
    }
    internal string Title { get; }
    internal string Table { get; }
    internal string ItemName { get; }
    internal string[] MatchFields { get; }
    internal string[] SummaryFields { get; }
    internal DetailField[] Fields { get; }
    internal bool ShowWhenEmpty { get; }
}

internal static class Definitions
{
    private static DetailField F(string name, string label, bool boolean = false) => new(name, label, boolean);
    private static DetailTable T(string title, string table, string item, string match, string summary, params DetailField[] fields) =>
        new(title, table, item, match.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries),
            summary.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries), fields, true);

    internal static IReadOnlyList<DetailTable> For(DetailSection section) => section switch
    {
        DetailSection.Country => new[]
        {
            T("Association Coefficients", "nationcoefficients", "Season", "countryid", "points|year1", F("points", "Current Points"), F("year1", "Season 1"), F("year2", "Season 2"), F("year3", "Season 3"), F("year4", "Season 4"), F("year5", "Season 5"))
        },
        DetailSection.League => new[]
        {
            T("Broadcast Assignment", "broadcastleague", "Assignment", "leagueid", "teamid|nationid", F("teamid", "Club"), F("nationid", "Country")),
            T("Match Officials", "leaguerefereelinks", "Official", "leagueid", "refereeid", F("refereeid", "Referee"))
        },
        DetailSection.Team => new[]
        {
            T("Rival Clubs", "rivals", "Rival", "teamid1|teamid2", "teamid1|teamid2", F("teamid1", "Home Club"), F("teamid2", "Rival Club"), F("rivaltype", "Rivalry Level")),
            T("Club Sponsors", "teamsponsorlinks", "Sponsor", "teamid", "adsponserid", F("adsponserid", "Sponsor"), F("dynamicimageid", "Artwork"), F("isapproved", "Approved", true)),
            T("Stadium Displays", "tifo", "Display", "teamid", "name|type", F("name", "Display Name"), F("type", "Display Type"), F("assetid", "Artwork")),
            T("Playing Identity", "teamformationteamstylelinks", "Style", "teamid", "formationid|teamstyleid", F("formationid", "Formation"), F("teamstyleid", "Playing Style"))
        },
        DetailSection.Player => new[]
        {
            T("Loan Agreement", "playerloans", "Loan", "playerid", "teamidloanedfrom", F("teamidloanedfrom", "Parent Club"), F("loandateend", "Loan End"), F("isloantobuy", "Option to Buy", true)),
            T("Suspensions", "playersuspensions", "Suspension", "playerid", "games|enddate", F("teamid", "Club"), F("games", "Matches"), F("enddate", "End Date")),
            T("Transfer Record", "transfers", "Transfer", "playerid", "buyingteamid|sellingteamid", F("sellingteamid", "Previous Club"), F("buyingteamid", "New Club"), F("transferamount", "Fee")),
            T("Previous Club", "previousteam", "Club", "playerid", "previousteamid", F("previousteamid", "Previous Club")),
            T("Player Perks", "playerperks", "Perks", "playerperks_playerid", "perk_0", F("perk_0", "Perk 1"), F("perk_1", "Perk 2"), F("perk_2", "Perk 3")),
            T("Player Archetype", "playerarchetypelinks", "Archetype", "playerid", "archetypeid", F("archetypeid", "Archetype"), F("archetypelevel", "Level"), F("background", "Background"), F("sigplaystyle1", "Signature Style 1"), F("sigplaystyle2", "Signature Style 2"), F("sigplaystyle3", "Signature Style 3"))
        },
        DetailSection.Manager => new[]
        {
            T("Managers", "manager", "Manager", "", "firstname|surname|commonname", F("firstname", "First Name"), F("surname", "Last Name"), F("commonname", "Known As"), F("teamid", "Club"), F("nationality", "Country"), F("birthdate", "Birth Date"), F("height", "Height"), F("bodytypecode", "Body Type"), F("headassetid", "Head Model"))
        },
        DetailSection.Stadium => new[]
        {
            T("Public Address", "audiostadium", "Audio Profile", "stadiumid", "stadiumpalanguageindex", F("stadiumpalanguageindex", "PA Language"), F("matchsizetypeindex", "Match Size")),
            T("Competition Use", "competitionstadiumlinks", "Competition", "stadiumid", "competitionid", F("competitionid", "Competition"), F("iscupfinal", "Cup Final", true))
        },
        DetailSection.Kit => new[]
        {
            T("HUD Appearance", "teamkithudvalues", "HUD Style", "teamkitid", "pattern", F("pattern", "Pattern"), F("kittype", "Kit Type")),
            T("Colour Overrides", "kitcoloroverrides", "Colour Set", "teamkitid", "fontcolor1r", F("fontcolor1r", "Name Colour Red"), F("fontcolor1g", "Name Colour Green"), F("fontcolor1b", "Name Colour Blue"), F("teamcolor1r", "Primary Red"), F("teamcolor1g", "Primary Green"), F("teamcolor1b", "Primary Blue"))
        },
        DetailSection.Competition => new[]
        {
            T("Official Balls", "competitionballs", "Ball", "competitionid", "ballid", F("ballid", "Ball"), F("weather", "Weather"), F("stage", "Stage")),
            T("Referee Kit", "competitionrefereekits", "Kit", "competitionid", "refereekit", F("refereekit", "Kit Number")),
            T("Qualification Seeds", "competitionseeds", "Seed", "competitionid", "teamid", F("teamid", "Club"), F("group", "Group"), F("groupslot", "Slot"), F("isqualified", "Qualified", true)),
            T("Competition Stadiums", "competitionstadiumlinks", "Stadium", "competitionid", "stadiumid", F("stadiumid", "Stadium"), F("iscupfinal", "Cup Final", true)),
            T("Competition Sponsors", "competitionsponsorlinks", "Sponsor", "competitionid", "adsponserid", F("adsponserid", "Sponsor"), F("dynamicimageid", "Artwork"), F("isapproved", "Approved", true)),
            T("Scheduled Fixtures", "fixtures", "Fixture", "competitionid", "hometeamid|awayteamid", F("hometeamid", "Home Club"), F("awayteamid", "Away Club"), F("stadiumid", "Stadium"), F("fixturedate", "Date"), F("fixturetime", "Kick-off"))
        },
        DetailSection.Formation => new[]
        {
            T("Club Formation Links", "teamformationteamstylelinks", "Club", "formationid", "teamid", F("teamid", "Club"), F("teamstyleid", "Playing Style"))
        },
        DetailSection.Ball => new[]
        {
            T("Competition Assignments", "competitionballs", "Competition", "ballid", "competitionid", F("competitionid", "Competition"), F("weather", "Weather"), F("stage", "Stage")),
            T("Ball Replacement", "teamballremapping", "Replacement", "ballid", "ballremapid", F("ballremapid", "Replacement Ball"))
        },
        DetailSection.Boot => new[]
        {
            T("Boot Replacement", "playerbootremapping", "Replacement", "shoetypecode", "shoetyperemapcode", F("shoetyperemapcode", "Replacement Boot"))
        },
        DetailSection.Gloves => new[]
        {
            T("Goalkeeper Gloves", "goalkeepergloves", "Gloves", "", "gloveid", F("gloveid", "Glove Number"), F("isavailableinstore", "Available", true), F("islicensed", "Licensed", true), F("isembargoed", "Embargoed", true))
        },
        DetailSection.Sponsor => new[]
        {
            T("Sponsors", "sponsors", "Sponsor", "", "name|adsponserid", F("name", "Sponsor Name"), F("basecolour", "Base Colour"), F("length", "Artwork Width"), F("isea", "Official Partner", true), F("isfut", "Ultimate Team", true))
        },
        DetailSection.Adboard => new[]
        {
            T("Adboard Assignments", "modeadboardlinks", "Assignment", "", "modeid|adsponserid", F("modeid", "Presentation Mode"), F("adsponserid", "Sponsor"), F("dynamicimageid", "Artwork"), F("isapproved", "Approved", true)),
            T("Console Overrides", "adboardconsoleoverrides", "Override", "", "tablename|tablekey", F("tablename", "Target Type"), F("tablekey", "Target"), F("console", "Platform"), F("adsponserid", "Sponsor"), F("dynamicimageid", "Artwork"), F("replacementimageid", "Replacement Artwork"))
        },
        DetailSection.Broadcast => new[]
        {
            T("League Broadcasts", "broadcastleague", "Assignment", "", "leagueid|teamid|nationid", F("leagueid", "League"), F("teamid", "Club"), F("nationid", "Country")),
            T("Presentation Modes", "presentationmodesettings", "Mode", "", "modetypestr|modeid", F("modetypestr", "Presentation Style"), F("isbrandpartnersenabled", "Brand Partners", true), F("isuniqueadboardsmodeenabled", "Unique Adboards", true), F("iscompetitionspecificboardsmodeenabled", "Competition Adboards", true), F("isgoallinetechforceenabled", "Goal-line Technology", true))
        },
        DetailSection.Referee => Array.Empty<DetailTable>(),
        _ => Array.Empty<DetailTable>()
    };
}

internal static class CmStyleDetailsWindow
{
    internal static void Attach(Form owner, string caption, DetailSection section, Func<int> id)
    {
        var button = new Button
        {
            Text = caption,
            Size = new Size(122, 25),
            Location = new Point(Math.Max(4, owner.ClientSize.Width - 130), 4),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            UseVisualStyleBackColor = true
        };
        button.Click += (_, _) =>
        {
            var current = id();
            if (current < 0) return;
            Open(owner, caption, section, current);
        };
        owner.Controls.Add(button);
        button.BringToFront();
    }

    internal static void Open(IWin32Window owner, string caption, DetailSection section, int entityId = 0)
    {
        using var dialog = new Form
        {
            Text = caption,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(980, 720),
            MinimumSize = new Size(780, 520),
            ShowInTaskbar = false
        };
        var panel = new CmStyleDetailsPanel(section);
        dialog.Controls.Add(panel);
        dialog.Shown += (_, _) => panel.Reload(entityId);
        dialog.ShowDialog(owner);
    }
}
