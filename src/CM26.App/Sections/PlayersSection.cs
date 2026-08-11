using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;
using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>FC26 adapter retaining the CM16 PlayerForm page structure and compact layout.</summary>
public sealed class PlayersSection : SectionBase
{
    private static readonly Font LegacyFont = Theme.Body;
    private readonly List<TextBox> _editors = [];
    private readonly Dictionary<string, FieldValue> _fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _stagingGrid = new();
    private readonly PictureBox _miniface = new();
    private readonly PictureBox _overviewFace = new();
    private readonly PictureBox _shoePreview = new();
    private readonly PictureBox _facePreview = new();
    private readonly Label _facePreviewCaption = new();
    private readonly Label _playerName = new();
    private readonly Label _clubName = new();
    private readonly Dictionary<string, Label> _skillValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<TextBox>> _summaryValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TrackBar> _skillSliders = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Label> _overviewRatings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Panel> _overviewBars = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Label> _overviewFacts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<TextBox>> _overviewAttributeValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Label>> _overviewSupplementValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Panel>> _playerStatBars = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Panel>> _overviewAttributeBadges = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Panel> _overviewMetricTiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Panel> _overviewRatingTiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Button> _playerLayoutButtons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Button> _playerCategoryButtons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Label _overviewName = new();
    private readonly Label _overviewMeta = new();
    private readonly Label _overviewOverall = new();
    private readonly Label _overviewPotential = new();
    private readonly Label _overviewGrowth = new();
    private string _playerStatsLayout = "CM";
    private bool _playerUseBars = true;
    private readonly List<TextBox> _traitEditors = [];
    private readonly TextBox _callnameId = new();
    private readonly Label _callnameStatus = new();
    private Panel? _traitsPanel;
    private bool _syncSkillSliders;
    private bool _syncReferencePickers;
    private int _currentPlayerId;
    private int _currentHeadAssetId;
    private readonly Dictionary<string, ComboBox> _referencePickers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly string[] PlaystyleNames =
    [
        "Finesse Shot", "Power Shot", "Dead Ball", "Chip Shot", "Power Header", "Pinged Pass", "Long Ball Pass", "Tiki Taka",
        "Incisive Pass", "Whipped Pass", "First Touch", "Technical", "Rapid", "Quick Step", "Trickster", "Press Proven",
        "Flair", "Relentless", "Trivela", "Block", "Intercept", "Anticipate", "Slide Tackle", "Bruiser", "Jockey", "Aerial",
        "Acrobatic", "Far Reach", "Footwork", "Cross Claimer", "Rush Out", "Deflector", "1v1 Close Down", "Long Throw"
    ];
    private static readonly string[] RoleNames =
    [ "Goalkeeper", "Sweeper Keeper", "Defender", "Stopper", "Ball-Playing Defender", "Wide Back", "Fullback", "Wingback", "Falseback", "Attacking Wingback", "Inverted Wingback", "Holding", "Deep-Lying Playmaker", "Box-to-Box", "Playmaker", "Half Winger", "Winger", "Wide Playmaker", "Inside Forward", "Poacher", "Advanced Forward", "False 9", "Target Forward" ];

    public override string SectionKey => "players";
    public override string SectionTitle => "Players";
    protected override string TableName => "players";
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search players…";

    public PlayersSection(AppServices services) : base(services)
    {
        // Font/GDI+ can be hostile on some machines (remote sessions, exotic
        // display scaling). Build every tab defensively so a single field that
        // cannot accept a font never takes down the whole section — or the app.
        SafeCtorStep(Tabs, "Tabs font", () => Tabs.Font = LegacyFont);
        Tabs.Padding = new Point(4, 2);
        Tabs.SizeMode = TabSizeMode.Fixed;
        Tabs.ItemSize = new Size(0, 1);
        AddTabSafe("Player", AddOverviewTab);
        // Callname tab removed — not needed for basic player editing.
    }

    private void AddTabSafe(string name, Action build)
    {
        try { build(); }
        catch (Exception ex)
        {
            Program.Log($"PlayersSection tab '{name}' build failed (skipped): {ex.Message}");
        }
    }

    private static void SafeCtorStep(Control target, string name, Action build)
    {
        try { build(); }
        catch (Exception ex)
        {
            Program.Log($"PlayersSection '{name}' failed: {ex.Message}");
        }
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Services.RequireData().GetPlayers();

    protected override void CreateNewRecord()
    {
        // Build position options
        var positionOptions = new List<(string Display, string Value)>
        {
            ("Striker (ST)", "25"),
            ("Centre Forward (CF)", "21"),
            ("Right Winger (RW)", "23"),
            ("Left Winger (LW)", "27"),
            ("Right Midfielder (RM)", "12"),
            ("Central Midfielder (CM)", "14"),
            ("Left Midfielder (LM)", "16"),
            ("Attacking Midfielder (CAM)", "18"),
            ("Defensive Midfielder (CDM)", "10"),
            ("Right Back (RB)", "3"),
            ("Centre Back (CB)", "5"),
            ("Left Back (LB)", "7"),
            ("Right Wing Back (RWB)", "2"),
            ("Left Wing Back (LWB)", "8"),
            ("Goalkeeper (GK)", "0"),
        };

        // `players.nationality` stores nations.nationid, not the nations table
        // row index. Reuse the editor's canonical mapping so a selected country
        // and the Player Summary always resolve to the same nation.
        var nationalityOptions = NationOptions().Select(option => (option.Display, option.Value)).ToList();

        var fields = new List<EntityField>
        {
            new("First name", "New"),
            new("Surname", "Player"),
            new("Position", "25", EntityFieldType.Dropdown, positionOptions),
            new("Nationality", "-1", EntityFieldType.Dropdown, nationalityOptions),
            new("Preferred foot", "1", EntityFieldType.Dropdown, new List<(string, string)>
            {
                ("Right", "1"),
                ("Left", "2"),
            }),
        };

        if (!EntityCreationDialog.TryShow(this, "Player", fields, out var values))
            return;
        try
        {
            var firstName = values[0];
            var surname = values[1];
            var requestedName = $"{firstName} {surname}".Trim();
            if (GetRecords().Any(item => string.Equals(item.Title.Trim(), requestedName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A player with that name already exists. Add a distinguishing name before creating the record.");

            // Dropdown values already contain the FC26 position code.  Treating
            // the code as an option-list index made e.g. GK (0) resolve to the
            // first item while higher positions mapped to the wrong role.
            var positionCode = int.TryParse(values[2], out var parsedPosition) &&
                parsedPosition is >= 0 and <= 27
                    ? parsedPosition.ToString()
                    : "25";

            // Parse nationality
            var nationality = "0";
            if (int.TryParse(values[3], out var nationRow) && nationRow >= 0)
            {
                var nations = Services.Session.GetTable("nations");
                if (nations != null && nationRow < nations.RowCount && nations.FindColumn("nationid") != null)
                    nationality = Services.Session.GetCell("nations", nationRow, "nationid") ?? "0";
            }

            // Parse preferred foot
            var preferredFoot = values[4] == "2" ? "2" : "1";

            var id = CreateRecordFromTemplate(TableName, "playerid", new Dictionary<string, string>
            {
                ["firstnameid"] = "0",
                ["lastnameid"] = "0",
                ["commonnameid"] = "0",
                ["playerjerseynameid"] = "0",
                ["headclasscode"] = "0",
                ["contractvaliduntil"] = DateTime.Today.Year.ToString(),
                ["overallrating"] = "50",
                ["potential"] = "60",
                ["preferredposition1"] = positionCode,
                ["preferredposition2"] = "-1",
                ["preferredposition3"] = "-1",
                ["preferredposition4"] = "-1",
                ["preferredfoot"] = preferredFoot,
                ["nationality"] = nationality,
                ["height"] = "180",
                ["weight"] = "75",
                ["jerseynumber"] = "0",
                ["isretiring"] = "0",
            });
            var nameSaved = TryCreateEditedPlayerName(id, firstName, surname);
            Services.SetPlayerNameOverride(id, firstName, surname);
            Services.Session.RefreshSchema();
            Services.RefreshDatabaseIndexes();
            LoadData();
            var created = GetRecords().FirstOrDefault(item =>
                Parse(Services.Session.GetCell(TableName, item.RecordIndex, "playerid")) == id);
            if (created != null) GoToRecord(created.RecordIndex);
            var nameNote = nameSaved
                ? " The player starts as a free agent."
                : " The player starts as a free agent. CM26 will show and search this name during this session; this FC26 database has no safe editable-name template, so the in-game name still requires a compatible name source.";
            MessageBox.Show(this, $"Player created with ID {id}.{nameNote}",
                "Create Player", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create Player", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryCreateEditedPlayerName(int playerId, string firstName, string surname)
    {
        var names = Services.Session.GetTable("editedplayernames");
        if (names == null || names.RowCount == 0) return false;
        var duplicate = Services.Session.DuplicateRow("editedplayernames", 0);
        if (!duplicate.Success) return false;
        var row = 1;
        foreach (var (field, value) in new Dictionary<string, string>
        {
            ["playerid"] = playerId.ToString(),
            ["firstname"] = firstName,
            ["surname"] = surname,
            ["commonname"] = string.Empty,
            ["playerjerseyname"] = surname,
        })
        {
            var outcome = Services.Session.StageEdit("editedplayernames", row, field, value);
            if (!outcome.Success)
                return false;
        }
        Services.Pending.MarkStructuralChange();
        Services.NotifyPendingChanged();
        return true;
    }

    private TabPage Page(string name)
    {
        var page = new TabPage(name) { BackColor = Theme.Background, Font = LegacyFont };
        page.Controls.Add(new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background });
        Tabs.TabPages.Add(page);
        return page;
    }

    private static Panel Canvas(TabPage p) => (Panel)p.Controls[0];
    private static Panel Box(string name, Point point, Size size)
    {
        var box = new Panel { Location = point, Size = size, BackColor = CardLayout.CardWhite };
        CardLayout.ApplyRounded(box, 10);
        box.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(size.Width, 4), BackColor = CardLayout.Fc26Green });
        box.Controls.Add(new Label
        {
            Text = name, Location = new Point(10, 8), Size = new Size(size.Width - 20, 16),
            Font = Theme.BodyBold, ForeColor = CardLayout.Fc26Green, BackColor = CardLayout.CardWhite
        });
        return box;
    }
    private static PictureBox Viewer(Point point, Size size) => new() { Location = point, Size = size, BackColor = Theme.Input, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

    // A read-only player card keeps the editable legacy form intact while
    // giving the section the quick, stat-first view expected from career tools.
    private void AddOverviewTab()
    {
        var page = Page("Player");
        var canvas = Canvas(page);
        canvas.AutoScrollMinSize = new Size(1370, 940);
canvas.BackColor = CardLayout.CardBackground;
        var card = new Panel { Location = new Point(12, 12), Size = new Size(1340, 910), BackColor = CardLayout.CardBackground };
        canvas.Controls.Add(card);
        var header = new Panel { Location = new Point(16, 16), Size = new Size(1308, 142), BackColor = CardLayout.CardWhite };
        ApplyRoundedCorners(header, 14);
        card.Controls.Add(header);
        header.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(6, 142), BackColor = CardLayout.Fc26Green });
        _overviewFace.Location = new Point(16, 13);
        _overviewFace.Size = new Size(116, 116);
        _overviewFace.SizeMode = PictureBoxSizeMode.Zoom;
        _overviewFace.BackColor = CardLayout.CardFieldBg;
        _overviewFace.BorderStyle = BorderStyle.None;
        header.Controls.Add(_overviewFace);
        _overviewName.Location = new Point(150, 20);
        _overviewName.Size = new Size(460, 36);
        _overviewName.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        _overviewName.ForeColor = CardLayout.CardText;
        header.Controls.Add(_overviewName);
        _overviewMeta.Location = new Point(153, 63);
        _overviewMeta.Size = new Size(500, 24);
        _overviewMeta.Font = Theme.BodyBold;
        _overviewMeta.ForeColor = CardLayout.CardMuted;
        header.Controls.Add(_overviewMeta);
        var editDetails = new Button { Text = "Edit player details…", Location = new Point(690, 20), Size = new Size(176, 32), FlatStyle = FlatStyle.Flat, Font = Theme.BodyBold };
        editDetails.FlatAppearance.BorderColor = CardLayout.Fc26Green;
        editDetails.Click += (_, _) => OpenSinglePlayerEditor();
        header.Controls.Add(editDetails);
AddHeaderMetric(header, "PAC", 150, CardLayout.Fc26Green, "acceleration", "sprintspeed");
        AddHeaderMetric(header, "SHO", 268, CardLayout.Fc26Yellow, "finishing", "shotpower", "longshots", "penalties", "volleys");
        AddHeaderMetric(header, "PAS", 386, CardLayout.Fc26Blue, "shortpassing", "longpassing", "vision", "crossing", "curve");
        AddHeaderMetric(header, "DRI", 504, CardLayout.Fc26Purple, "agility", "balance", "reactions", "ballcontrol", "dribbling", "composure");
        AddHeaderMetric(header, "DEF", 622, CardLayout.Fc26Blue, "interceptions", "headingaccuracy", "defensiveawareness", "standingtackle", "slidingtackle");
        AddHeaderMetric(header, "PHY", 740, CardLayout.Fc26Orange, "jumping", "stamina", "strength", "aggression");
AddOverviewTile(header, _overviewOverall, "OVR", 1050, CardLayout.Fc26Green);
        AddOverviewTile(header, _overviewGrowth, "GRO", 940, CardLayout.Fc26Yellow);
        AddOverviewTile(header, _overviewPotential, "POT", 1160, CardLayout.Fc26Green);
var facts = new Panel { Location = new Point(16, 172), Size = new Size(1308, 92), BackColor = CardLayout.CardWhite };
        ApplyRoundedCorners(facts, 12);
        card.Controls.Add(facts);
        facts.Controls.Add(new Label { Text = "PLAYER INFO", Location = new Point(12, 4), Size = new Size(180, 16), Font = Theme.Muted9, ForeColor = CardLayout.CardSubtle });
        AddOverviewFact(facts, "Position", "preferredposition1", 12);
        AddOverviewFact(facts, "Club", "club", 224);
        AddOverviewFact(facts, "Nation", "nationality", 436);
        AddOverviewFact(facts, "Height", "height", 648, " cm");
        AddOverviewFact(facts, "Weight", "weight", 860, " kg");
        AddOverviewFact(facts, "Preferred foot", "preferredfoot", 1072);

        var headings = new Label { Text = "PLAYER ATTRIBUTES", Location = new Point(18, 282), Size = new Size(420, 22), Font = Theme.BodyBold, ForeColor = CardLayout.CardText };
        card.Controls.Add(headings);
        AddOverviewAttributeGroup(card, "ATTACKING", CardLayout.Fc26Yellow, 18, 316,
            ("Crossing", "crossing"), ("Finishing", "finishing"), ("Heading accuracy", "headingaccuracy"), ("Short passing", "shortpassing"), ("Volleys", "volleys"), ("Penalties", "penalties"));
        AddOverviewAttributeGroup(card, "SKILL", CardLayout.Fc26Purple, 455, 316,
            ("Dribbling", "dribbling"), ("Curve", "curve"), ("Free-kick accuracy", "freekickaccuracy"), ("Long passing", "longpassing"), ("Ball control", "ballcontrol"), ("Composure", "composure"));
        AddOverviewAttributeGroup(card, "MOVEMENT", CardLayout.Fc26Green, 892, 316,
            ("Acceleration", "acceleration"), ("Sprint speed", "sprintspeed"), ("Agility", "agility"), ("Reactions", "reactions"), ("Balance", "balance"), ("Positioning", "positioning"));
        AddOverviewAttributeGroup(card, "POWER", CardLayout.Fc26Orange, 18, 480,
            ("Shot power", "shotpower"), ("Jumping", "jumping"), ("Stamina", "stamina"), ("Strength", "strength"), ("Long shots", "longshots"));
        AddOverviewAttributeGroup(card, "MENTALITY", CardLayout.Fc26Blue, 455, 480,
            ("Vision", "vision"), ("Aggression", "aggression"), ("Interceptions", "interceptions"), ("Att. position", "positioning"), ("Reactions", "reactions"));
        AddOverviewAttributeGroup(card, "DEFENDING", CardLayout.Fc26Blue, 892, 480,
            ("Def. awareness", "defensiveawareness"), ("Stand tackle", "standingtackle"), ("Slide tackle", "slidingtackle"), ("Heading accuracy", "headingaccuracy"), ("Strength", "strength"));
        AddOverviewAttributeGroup(card, "GOALKEEPING", CardLayout.Fc26Red, 18, 644,
            ("GK diving", "gkdiving"), ("GK handling", "gkhandling"), ("GK kicking", "gkkicking"), ("GK positioning", "gkpositioning"), ("GK reflexes", "gkreflexes"));
AddOverviewSupplement(card, "CONTRACT & VALUE", 455, 644,
            ("Contract until", "contractvaliduntil"), ("Value", "value"), ("Wage", "wage"), ("Reputation", "internationalrep"));
        AddOverviewSupplement(card, "PLAYING ROLES", 892, 644,
            ("Primary role", "role1"), ("Secondary role", "role2"), ("Third role", "role3"), ("Fourth role", "role4"));
        _traitsPanel = Box("PLAYSTYLES", new Point(455, 804), new Size(855, 86));
        card.Controls.Add(_traitsPanel);
        var note = new Label { Text = "Read-only career overview · Edit all database values in the Info and Skills tabs.", Location = new Point(28, 550), Size = new Size(800, 24), ForeColor = CardLayout.CardSubtle, Font = Theme.Body };
        note.Visible = false;
        note.Location = new Point(18, 858);
        card.Controls.Add(note);
    }

    private void AddOverviewTile(Control parent, Label value, string title, int x, Color accent)
    {
        var tile = new Panel { Location = new Point(x, 22), Size = new Size(90, 112), BackColor = accent };
        ApplyRoundedCorners(tile, 14);
        value.Location = new Point(5, 10); value.Size = new Size(80, 54); value.Font = new Font("Segoe UI", 24, FontStyle.Bold); value.TextAlign = ContentAlignment.MiddleCenter; value.ForeColor = Color.White;
        tile.Controls.Add(value);
        tile.Controls.Add(new Label { Text = title, Location = new Point(4, 73), Size = new Size(82, 20), TextAlign = ContentAlignment.MiddleCenter, Font = Theme.BodyBold, ForeColor = Color.White });
        parent.Controls.Add(tile);
        _overviewRatingTiles[title] = tile;
    }

    private static Color Lighten(Color color, int amount)
    {
        amount = Math.Clamp(amount, 0, 255);
        return Color.FromArgb(
            color.R + (255 - color.R) * amount / 255,
            color.G + (255 - color.G) * amount / 255,
            color.B + (255 - color.B) * amount / 255);
    }

    /// <summary>FUT-style rating grade used for the fcradar-style colored badges (0-99 scale).</summary>
    private static Color RatingColor(int rating) => rating switch
    {
        >= 90 => Color.FromArgb(24, 133, 74),
        >= 80 => Color.FromArgb(92, 173, 61),
        >= 70 => Color.FromArgb(224, 138, 39),
        >= 60 => Color.FromArgb(226, 170, 40),
        >= 50 => Color.FromArgb(213, 99, 53),
        > 0 => Color.FromArgb(196, 63, 63),
        _ => Color.FromArgb(196, 199, 191)
    };

    /// <summary>Clips a control to a rounded rectangle. Cheap and one-shot since every
    /// overview tile/badge here has a fixed size set at creation time.</summary>
    private static void ApplyRoundedCorners(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0) return;
        var d = Math.Min(radius * 2, Math.Min(control.Width, control.Height));
        var rect = new Rectangle(0, 0, control.Width, control.Height);
        using var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        control.Region?.Dispose();
        control.Region = new Region(path);
    }

    private void AddHeaderMetric(Control parent, string code, int x, Color accent, params string[] fields)
    {
        var metric = new Panel { Location = new Point(x, 96), Size = new Size(106, 32), BackColor = Lighten(accent, 235) };
        ApplyRoundedCorners(metric, 8);
        var value = new Label { Location = new Point(11, 4), Size = new Size(40, 23), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Tag = fields };
        metric.Controls.Add(value);
        metric.Controls.Add(new Label { Text = code, Location = new Point(47, 5), Size = new Size(52, 20), Font = Theme.BodyBold, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleRight });
        _overviewRatings[code] = value;
        _overviewMetricTiles[code] = metric;
        parent.Controls.Add(metric);
    }

private void AddOverviewFact(Control parent, string title, string field, int x, string suffix = "")
    {
        var block = new Panel { Location = new Point(x, 22), Size = new Size(196, 58), BackColor = CardLayout.CardFieldBg };
        ApplyRoundedCorners(block, 8);
        block.Controls.Add(new Label { Text = title.ToUpperInvariant(), Location = new Point(10, 6), Size = new Size(176, 16), Font = new Font(Theme.Body, FontStyle.Bold), ForeColor = CardLayout.CardSubtle });
        var value = new Label { Location = new Point(10, 25), Size = new Size(176, 26), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = CardLayout.CardText, AutoEllipsis = true, Tag = suffix };
        block.Controls.Add(value);
        parent.Controls.Add(block);
        _overviewFacts[field] = value;
    }

    private void AddLayoutButton(Control parent, string mode, int x)
    {
        var button = new Button { Text = mode, Location = new Point(x, 278), Size = new Size(66, 26), FlatStyle = FlatStyle.Flat, Font = Theme.Muted9 };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 182);
        button.Click += (_, _) => { _playerStatsLayout = mode; RefreshOverview(); };
        _playerLayoutButtons[mode] = button;
        parent.Controls.Add(button);
    }

    private void AddCategoryButton(Control parent, string text, int x, bool bars)
    {
        var button = new Button { Text = text, Location = new Point(x, 278), Size = new Size(78, 26), FlatStyle = FlatStyle.Flat, Font = Theme.Muted9 };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 182);
        button.Click += (_, _) => { _playerUseBars = bars; RefreshOverview(); };
        _playerCategoryButtons[text] = button;
        parent.Controls.Add(button);
    }

    private void OpenSinglePlayerEditor()
    {
        if (_currentPlayerId <= 0 || _fields.Count == 0) return;
        var editor = new FieldEditorGrid { Dock = DockStyle.Fill };
        editor.SetFields(_fields.Values.OrderBy(value => value.Label).ToList(), ToolTip);
        editor.FieldEdited += (_, change) =>
        {
            if (StageField(TableName, CurrentRecordIndex, change.field, change.value, editor))
                ShowRecord(CurrentRecordIndex);
        };
        using var dialog = new Form
        {
            Text = $"Edit { _playerName.Text }",
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(720, 760),
            MinimizeBox = false,
            ShowInTaskbar = false,
            BackColor = Theme.Background
        };
        dialog.Controls.Add(editor);
        dialog.ShowDialog(this);
    }

    private void AddOverviewAttributeGroup(Control parent, string title, Color accent, int x, int y, params (string Label, string Field)[] attributes)
    {
var group = new Panel { Location = new Point(x, y), Size = new Size(418, 160), BackColor = CardLayout.CardWhite };
        ApplyRoundedCorners(group, 12);
        group.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(418, 4), BackColor = accent });
        group.Controls.Add(new Label { Text = title, Location = new Point(14, 12), Size = new Size(250, 20), Font = Theme.BodyBold, ForeColor = accent });
        var row = 0;
        foreach (var (label, field) in attributes)
        {
            var yOffset = 38 + row * 20;
            group.Controls.Add(new Label { Text = label, Location = new Point(14, yOffset), Size = new Size(305, 18), Font = Theme.Body, ForeColor = CardLayout.CardFieldLabel });
            var value = new TextBox
            {
                Location = new Point(336, yOffset - 1), Size = new Size(64, 20),
                BorderStyle = BorderStyle.None, TextAlign = HorizontalAlignment.Right,
                Font = Theme.BodyBold, ForeColor = CardLayout.CardText,
                BackColor = CardLayout.CardWhite, Tag = field
            };
            value.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                CommitOverviewAttribute(value);
                e.SuppressKeyPress = true;
            };
            value.Leave += (_, _) => CommitOverviewAttribute(value);
            group.Controls.Add(value);
            var track = new Panel { Location = new Point(218, yOffset + 5), Size = new Size(106, 8), BackColor = Color.FromArgb(223, 225, 219), Visible = false };
            var fill = new Panel { Location = Point.Empty, Size = new Size(1, 8), BackColor = accent };
            track.Controls.Add(fill);
            group.Controls.Add(track);
            if (!_overviewAttributeValues.TryGetValue(field, out var values))
                _overviewAttributeValues[field] = values = [];
            values.Add(value);
            if (!_playerStatBars.TryGetValue(field, out var bars))
                _playerStatBars[field] = bars = [];
            bars.Add(track);
            row++;
        }
        parent.Controls.Add(group);
    }

    private void CommitOverviewAttribute(TextBox editor)
    {
        if (_syncSkillSliders || CurrentRecordIndex < 0 || editor.Tag is not string field ||
            !_fields.TryGetValue(field, out var value) || !value.IsWritable)
            return;
        if (!int.TryParse(editor.Text.Trim(), out var rating) || rating is < 0 or > 99)
        {
            editor.Text = GetOverviewNumber(field).ToString();
            return;
        }
        if (StageField(TableName, CurrentRecordIndex, field, rating.ToString(), _stagingGrid))
            ShowRecord(CurrentRecordIndex);
    }

    private void AddOverviewSupplement(Control parent, string title, int x, int y, params (string Label, string Field)[] entries)
    {
        var group = new Panel { Location = new Point(x, y), Size = new Size(418, 150), BackColor = Color.White };
        ApplyRoundedCorners(group, 12);
        group.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(418, 4), BackColor = Color.FromArgb(116, 185, 34) });
        group.Controls.Add(new Label { Text = title, Location = new Point(14, 12), Size = new Size(360, 20), Font = Theme.BodyBold, ForeColor = Color.FromArgb(65, 105, 39) });
        for (var index = 0; index < entries.Length; index++)
        {
            var (label, field) = entries[index];
            var yOffset = 40 + index * 25;
            group.Controls.Add(new Label { Text = label, Location = new Point(14, yOffset), Size = new Size(185, 20), Font = Theme.Body, ForeColor = Color.FromArgb(55, 55, 51) });
            var value = new Label { Location = new Point(204, yOffset), Size = new Size(198, 20), TextAlign = ContentAlignment.MiddleRight, Font = Theme.BodyBold, ForeColor = Color.FromArgb(37, 37, 34), AutoEllipsis = true };
            group.Controls.Add(value);
            if (!_overviewSupplementValues.TryGetValue(field, out var values))
                _overviewSupplementValues[field] = values = [];
            values.Add(value);
        }
        parent.Controls.Add(group);
    }

    private void AddOverviewMetric(Control parent, string code, string[] fields, int x, int y, Color accent)
    {
        var box = new Panel { Location = new Point(x, y), Size = new Size(418, 118), BackColor = Color.FromArgb(18, 27, 29) };
        var value = new Label { Location = new Point(14, 12), Size = new Size(64, 36), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = accent, TextAlign = ContentAlignment.MiddleCenter };
        box.Controls.Add(value);
        box.Controls.Add(new Label { Text = code, Location = new Point(84, 20), Size = new Size(72, 20), Font = Theme.BodyBold, ForeColor = Color.White });
        var label = new Label { Location = new Point(14, 54), Size = new Size(390, 20), ForeColor = Color.FromArgb(190, 205, 207), Font = Theme.Body };
        box.Controls.Add(label);
        var track = new Panel { Location = new Point(14, 88), Size = new Size(390, 10), BackColor = Color.FromArgb(54, 72, 74) };
        var fill = new Panel { Location = Point.Empty, Size = new Size(1, 10), BackColor = accent };
        track.Controls.Add(fill); box.Controls.Add(track); parent.Controls.Add(box);
        _overviewRatings[code] = value;
        _overviewBars[code] = fill;
        value.Tag = fields;
        label.Tag = fields;
    }

    private void AddInfoTab()
    {
        var page = Page("Info");
        var canvas = Canvas(page);

        var identity = Box("Identity Card", new Point(3, 3), new Size(390, 240));
        _miniface.Location = new Point(12, 20);
        _miniface.Size = new Size(100, 100);
        _miniface.BackColor = Theme.Input;
        _miniface.BorderStyle = BorderStyle.FixedSingle;
        _miniface.SizeMode = PictureBoxSizeMode.Zoom;
        identity.Controls.Add(_miniface);
        LegacyAssetActions.Attach(Services, identity, _miniface, new Point(12, 124), () => ShowRecord(CurrentRecordIndex));
        _playerName.Location = new Point(11, 216);
        _playerName.Size = new Size(365, 20);
        _playerName.Font = LegacyFont;
        _playerName.AutoEllipsis = true;
        identity.Controls.Add(_playerName);
        AddFields(identity, new[]
        {
            ("Player Id", "playerid"), ("First Name", "firstnameid"), ("Surname", "lastnameid"), ("Common Name", "commonnameid"),
            ("Jersey", "jerseynumber"), ("Birthdate", "birthdate")
        }, 155, 20, 255, 120, 26);
        AddReferenceDropdown(identity, "Country", "nationality", new Point(255, 176), 120, NationOptions());
        canvas.Controls.Add(identity);

        var playingFor = Box("Playing for", new Point(399, 3), new Size(245, 174));
        playingFor.Controls.Add(Viewer(new Point(12, 20), new Size(218, 70)));
        _clubName.Location = new Point(12, 94);
        _clubName.Size = new Size(218, 18);
        _clubName.Font = LegacyFont;
        _clubName.TextAlign = ContentAlignment.MiddleCenter;
        playingFor.Controls.Add(_clubName);
        AddFields(playingFor, new[] { ("Joining Date", "playerjointeamdate"), ("Is Retiring", "isretiring") }, 12, 122, 102, 125, 26);
        canvas.Controls.Add(playingFor);

        var body = Box("Body", new Point(3, 249), new Size(390, 154));
        AddFields(body, new[] { ("Height", "height"), ("Weight", "weight"), ("Body", "bodytypecode"), ("Weak foot", "weakfootabilitytypecode") }, 12, 18, 245, 120, 26);
        AddReferenceDropdown(body, "Best foot", "preferredfoot", new Point(245, 96), 120, FootOptions());
        canvas.Controls.Add(body);

        var look = Box("Look", new Point(3, 409), new Size(390, 252));
        AddFields(look, new[]
        {
            ("Jersey Style", "jerseystylecode"), ("Sleeves Length", "jerseysleevelengthcode"), ("Socks Style", "sockstylecode"),
            ("Socks Length", "socklengthcode"), ("GK Gloves", "gkglovetypecode"), ("Shoes Type", "shoetypecode"),
            ("Undershort Style", "undershortstyle"), ("Short Style", "shortstyle"), ("Jersey Fit", "jerseyfit")
        }, 12, 18, 145, 240, 26);
        canvas.Controls.Add(look);

        var shoes = Box("Boots", new Point(399, 249), new Size(245, 154));
        _shoePreview.Location = new Point(112, 30);
        _shoePreview.Size = new Size(118, 118);
        _shoePreview.BackColor = Theme.Input;
        _shoePreview.BorderStyle = BorderStyle.FixedSingle;
        _shoePreview.SizeMode = PictureBoxSizeMode.Zoom;
        shoes.Controls.Add(_shoePreview);
        AddFields(shoes, new[]
        {
            ("Type", "shoetypecode"), ("Design", "shoedesigncode"),
            ("Color 1", "shoecolorcode1"), ("Color 2", "shoecolorcode2")
        }, 12, 22, 65, 40, 26);
        canvas.Controls.Add(shoes);

        var play = Box("Playing Info", new Point(399, 409), new Size(245, 155));
        AddReferenceDropdown(play, "Preferred Position 1", "preferredposition1", new Point(148, 25), 92, PositionOptions());
        AddReferenceDropdown(play, "Preferred Position 2", "preferredposition2", new Point(148, 51), 92, PositionOptions());
        AddReferenceDropdown(play, "Preferred Position 3", "preferredposition3", new Point(148, 77), 92, PositionOptions());
        AddReferenceDropdown(play, "Preferred Position 4", "preferredposition4", new Point(148, 103), 92, PositionOptions());
        AddFields(play, new[] { ("International Reputation", "internationalrep") }, 12, 129, 148, 92, 26);
        canvas.Controls.Add(play);

        // A compact FC26 player overview, while keeping the original CM16 group-box
        // visual language instead of replacing this page with a modern card UI.
        var summary = Box("Player Summary", new Point(650, 3), new Size(880, 129));
        AddSummaryValue(summary, "Overall", "overallrating", new Point(12, 25));
        AddSummaryValue(summary, "Potential", "potential", new Point(12, 51));
        AddSummaryValue(summary, "Position", "preferredposition1", new Point(12, 77));
        AddSummaryValue(summary, "Nation", "nationality", new Point(12, 103));
        AddSummaryValue(summary, "Height", "height", new Point(280, 25));
        AddSummaryValue(summary, "Weight", "weight", new Point(280, 51));
        AddSummaryValue(summary, "Preferred Foot", "preferredfoot", new Point(280, 77));
        AddSummaryValue(summary, "International Rep.", "internationalrep", new Point(280, 103));
        canvas.Controls.Add(summary);

        var attributes = Box("Key Attributes", new Point(650, 138), new Size(880, 181));
        AddSummaryValue(attributes, "Acceleration", "acceleration", new Point(12, 25));
        AddSummaryValue(attributes, "Sprint Speed", "sprintspeed", new Point(12, 51));
        AddSummaryValue(attributes, "Dribbling", "dribbling", new Point(12, 77));
        AddSummaryValue(attributes, "Short Passing", "shortpassing", new Point(12, 103));
        AddSummaryValue(attributes, "Finishing", "finishing", new Point(12, 129));
        AddSummaryValue(attributes, "Ball Control", "ballcontrol", new Point(12, 155));
        AddSummaryValue(attributes, "Reactions", "reactions", new Point(280, 25));
        AddSummaryValue(attributes, "Vision", "vision", new Point(280, 51));
        AddSummaryValue(attributes, "Standing Tackle", "standingtackle", new Point(280, 77));
        AddSummaryValue(attributes, "Strength", "strength", new Point(280, 103));
        AddSummaryValue(attributes, "Stamina", "stamina", new Point(280, 129));
        AddSummaryValue(attributes, "Skill Moves", "skillmoves", new Point(280, 155));
        canvas.Controls.Add(attributes);

        // Keep the FC26 record keys and contract values on CM16's primary Info page.
        var technical = Box("Record and Contract", new Point(650, 325), new Size(880, 106));
        AddFields(technical, new[]
        {
            ("Contract Valid Until", "contractvaliduntil")
        }, 15, 25, 205, 285, 26);
        technical.Controls.Add(new Label
        {
            Text = "Player Id, name ids and joining date are edited in the " +
                   "Identity Card and Playing for sections above.",
            Location = new Point(15, 60), Size = new Size(520, 40),
            Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        canvas.Controls.Add(technical);
    }

    private void AddSkillsTab()
    {
        var page = Page("Skills");
        var canvas = Canvas(page);
        var columns = new[]
        {
            ("Random Generation", new[] { "overallrating", "potential" }),
            ("Goalkeeper Skills", new[] { "gkreflexes", "gkhandling", "gkdiving", "gkpositioning", "gkkicking" }),
            ("Defensive Skills", new[] { "defensiveawareness", "standingtackle", "aggression", "slidingtackle", "interceptions" }),
            ("Midfielder Skills", new[] { "shortpassing", "longpassing", "crossing", "ballcontrol", "vision", "curve" }),
            ("Mental Skills", new[] { "reactions", "composure", "positioning", "balance" }),
            ("Attacking Skills", new[] { "shotpower", "longshots", "dribbling", "volleys", "headingaccuracy", "finishing" }),
            ("Physical Skills", new[] { "acceleration", "sprintspeed", "stamina", "strength", "agility", "jumping" }),
            ("Free Kick Skills", new[] { "freekickaccuracy", "penalties", "skillmoves" })
        };
        for (var c = 0; c < columns.Length; c++)
        {
            var group = Box(columns[c].Item1, new Point(3 + (c * 160), 3), new Size(153, 318));
            var y = 25;
            foreach (var field in columns[c].Item2)
            {
                var label = new Label { Location = new Point(6, y), Size = new Size(141, 18), TextAlign = ContentAlignment.MiddleCenter, BackColor = Theme.Accent, ForeColor = Theme.Text, Font = Theme.BodyBold };
                _skillValues[field] = label;
                group.Controls.Add(label);
                var slider = new TrackBar { Location = new Point(5, y + 19), Size = new Size(142, 28), Minimum = 1, Maximum = 99, TickStyle = TickStyle.None, Tag = field, BackColor = Theme.Panel };
                // TrackBar fires ValueChanged continuously while dragging; stage
                // once per gesture (mouse release / keyboard) instead of per tick.
                slider.ValueChanged += (_, _) => UpdateSkillLabel(slider);
                slider.MouseUp += (_, _) => StageSkill(slider);
                slider.KeyUp += (_, _) => StageSkill(slider);
                _skillSliders[field] = slider;
                group.Controls.Add(slider);
                y += 48;
            }
            canvas.Controls.Add(group);
        }

        _traitsPanel = Box("Player Traits", new Point(3, 327), new Size(1280, 300));
        _traitsPanel.Controls.Add(new Label
        {
            Text = "Trait bitmasks are shown only when the loaded database provides them.",
            Location = new Point(14, 24), Size = new Size(580, 22), Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        canvas.Controls.Add(_traitsPanel);
    }

    private void AddFaceTab()
    {
        var page = Page("Face");
        var canvas = Canvas(page);
        var preview = Box("Face Preview", new Point(3, 3), new Size(1200, 451));
        _facePreview.Location = new Point(8, 20);
        _facePreview.Size = new Size(1183, 390);
        _facePreview.BackColor = CardLayout.CardFieldBg;
        _facePreview.BorderStyle = BorderStyle.FixedSingle;
        _facePreview.SizeMode = PictureBoxSizeMode.Zoom;
        preview.Controls.Add(_facePreview);
        var open3d = new Button
        {
            Text = "Open 3D Face Viewer…", Location = new Point(12, 417),
            Size = new Size(165, 28), Font = LegacyFont
        };
        Theme.ApplyButton(open3d);
        open3d.Click += async (_, _) => await Open3DFaceViewerAsync();
        preview.Controls.Add(open3d);
        // The visible player face is a real legacy UI texture.  Keep its
        // import/export controls beside the 3D viewer rather than hiding asset
        // work in a separate technical screen.
        LegacyAssetActions.Attach(Services, preview, _facePreview, new Point(184, 417), RefreshFacePreview,
            "Import Face", "Remove Face");
        _facePreviewCaption.Location = new Point(410, 417);
        _facePreviewCaption.Size = new Size(320, 28);
        _facePreviewCaption.Font = LegacyFont;
        _facePreviewCaption.TextAlign = ContentAlignment.MiddleCenter;
        _facePreviewCaption.Text = "Face preview";
        preview.Controls.Add(_facePreviewCaption);
        canvas.Controls.Add(preview);
        var modelling = Box("Face Modelling", new Point(3, 462), new Size(1200, 204));
        AddFields(modelling, new[] { ("Head Model", "headclasscode"), ("Head Type", "headtypecode"), ("Head Variation", "headvariation"), ("Head Asset Id", "headassetid"), ("Hair Model", "hairstylecode"), ("Hair Type", "hairtypecode"), ("High Quality Head", "hashighqualityhead") }, 16, 22, 150, 260, 26);
        canvas.Controls.Add(modelling);
        var appearance = Box("Appearance", new Point(3, 674), new Size(1200, 256));
        AddFields(appearance, new[]
        {
            ("Hair Color", "haircolorcode"), ("Facial Hair", "facialhairtypecode"), ("Facial Hair Color", "facialhaircolorcode"),
            ("Skin Tone", "skintonecode"), ("Eyes Color", "eyecolorcode"), ("Eye Detail", "eyedetail"),
            ("Eyebrow Code", "eyebrowcode"), ("Sideburns Code", "sideburnscode"), ("Lip Color", "lipcolor")
        }, 16, 22, 150, 260, 26);
        canvas.Controls.Add(appearance);
        var skin = Box("Skin Details", new Point(3, 938), new Size(1200, 230));
        AddFields(skin, new[]
        {
            ("Skin Type", "skintypecode"), ("Skin Makeup", "skinmakeup"), ("Skin Surface Pack", "skinsurfacepack"),
            ("Skin Complexion", "skincomplexion"), ("Muscularity", "muscularitycode"), ("Face Pose Preset", "faceposerpreset"),
            ("Face PSD Layer 0", "facepsdlayer0"), ("Face PSD Layer 1", "facepsdlayer1")
        }, 16, 22, 150, 260, 26);
        canvas.Controls.Add(skin);
    }

    private void AddDetailsTab()
    {
        var page = Page("Details");
        var canvas = Canvas(page);

        var tattoos = Box("Tattoos", new Point(3, 3), new Size(750, 207));
        AddFields(tattoos, new[]
        {
            ("Head Tattoo", "tattoohead"), ("Front Tattoo", "tattoofront"), ("Back Tattoo", "tattoback"),
            ("Left Arm", "tattooleftarm"), ("Right Arm", "tattoorightarm"),
            ("Left Leg", "tattooleftleg"), ("Right Leg", "tattoorightleg")
        }, 12, 25, 145, 260, 26);
        canvas.Controls.Add(tattoos);

        var accessories = Box("Accessories", new Point(759, 3), new Size(750, 233));
        AddFields(accessories, new[]
        {
            ("Accessory 1", "accessorycode1"), ("Accessory 2", "accessorycode2"),
            ("Accessory 3", "accessorycode3"), ("Accessory 4", "accessorycode4"),
            ("Colour 1", "accessorycolourcode1"), ("Colour 2", "accessorycolourcode2"),
            ("Colour 3", "accessorycolourcode3"), ("Colour 4", "accessorycolourcode4")
        }, 12, 25, 145, 260, 26);
        canvas.Controls.Add(accessories);

        var positions = Box("Preferred Positions", new Point(3, 218), new Size(750, 100));
        AddFields(positions, new[]
        {
            ("Preferred Position 5", "preferredposition5"), ("Preferred Position 6", "preferredposition6"),
            ("Preferred Position 7", "preferredposition7")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(positions);

        var roles = Box("Playing Roles", new Point(759, 242), new Size(750, 204));
        AddFields(roles, new[]
        {
            ("Role 1", "role1"), ("Role 2", "role2"), ("Role 3", "role3"), ("Role 4", "role4"), ("Role 5", "role5"),
            ("Finishing Code 1", "finishingcode1"), ("Finishing Code 2", "finishingcode2")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(roles);

        var goalkeeper = Box("Goalkeeper Style", new Point(3, 326), new Size(750, 74));
        AddFields(goalkeeper, new[]
        {
            ("Save Type", "gksavetype"), ("Kick Style", "gkkickstyle")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(goalkeeper);

        var profile = Box("Player Profile", new Point(3, 406), new Size(750, 259));
        AddFields(profile, new[]
        {
            ("Gender", "gender"), ("Personality", "personality"), ("Emotion", "emotion"),
            ("Run Style", "runstylecode"), ("Running Code 1", "runningcode1"), ("Running Code 2", "runningcode2"),
            ("Free Kick Start Pose", "animfreekickstartposcode"), ("Penalty Start Pose", "animpenaltiesstartposcode"),
            ("Skill Moves Likelihood", "skillmoveslikelihood")
        }, 12, 25, 145, 260, 26);
        canvas.Controls.Add(profile);

        var development = Box("Development", new Point(759, 452), new Size(750, 178));
        AddFields(development, new[]
        {
            ("Pace Division", "pacdiv"), ("Dribble Reference", "driref"), ("Defence Reference", "defspe"),
            ("Passing Reference", "paskic"), ("Physical Reference", "phypos"), ("Modifier", "modifier")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(development);

        var customisation = Box("Customisation", new Point(759, 636), new Size(750, 178));
        AddFields(customisation, new[]
        {
            ("User Can Edit Name", "usercaneditname"), ("Is Customized", "iscustomized"),
            ("Avatar POM Id", "avatarpomid"), ("Shohan", "shohan"),
            ("Jersey Name Id", "playerjerseynameid"), ("Small Sided Shoes", "smallsidedshoetypecode")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(customisation);

        var iconTraits = Box("Icon Traits", new Point(3, 671), new Size(750, 74));
        AddFields(iconTraits, new[]
        {
            ("Icon Trait 1", "icontrait1"), ("Icon Trait 2", "icontrait2")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(iconTraits);
    }

    private void AddCallnameTab()
    {
        var page = Page("Callname");
        var canvas = Canvas(page);
        var assignment = Box("Commentary Callname", new Point(3, 3), new Size(640, 215));
        assignment.Controls.Add(new Label
        {
            Text = "Commentary Id", Location = new Point(16, 33),
            Size = new Size(105, 20), Font = LegacyFont
        });
        _callnameId.Location = new Point(126, 30);
        _callnameId.Size = new Size(150, 20);
        _callnameId.Font = LegacyFont;
        assignment.Controls.Add(_callnameId);
        var assign = new Button
        {
            Text = "Assign Callname", Location = new Point(291, 27),
            Size = new Size(125, 27), Font = LegacyFont
        };
        Theme.ApplyButton(assign);
        assign.Click += (_, _) => AssignPlayerCallname();
        assignment.Controls.Add(assign);
        var remove = new Button
        {
            Text = "Remove Mapping", Location = new Point(423, 27),
            Size = new Size(125, 27), Font = LegacyFont
        };
        Theme.ApplyButton(remove);
        remove.Click += (_, _) => RemovePlayerCallname();
        assignment.Controls.Add(remove);
        _callnameStatus.Location = new Point(16, 68);
        _callnameStatus.Size = new Size(610, 55);
        _callnameStatus.Font = LegacyFont;
        assignment.Controls.Add(_callnameStatus);
        assignment.Controls.Add(new Label
        {
            Text = "The Commentary Id must already exist in the installed commentary\n" +
                   "bank; CM26 will not create a fake database-only audio entry.",
            Location = new Point(16, 128), Size = new Size(610, 42),
            Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel
        });
        var generator = new Button
        {
            Text = "Open Callname TTS Tool…", Location = new Point(16, 181),
            Size = new Size(180, 28), Font = LegacyFont
        };
        Theme.ApplyButton(generator);
        generator.Click += (_, _) => OpenCallnameGenerator();
        assignment.Controls.Add(generator);
        canvas.Controls.Add(assignment);
    }

    private async Task Open3DFaceViewerAsync()
    {
        var executable = Path.Combine(
            AppContext.BaseDirectory, "Tools", "CM26.3DViewer",
            "3D Face Viewer By Rizco98 FET Renderer.exe");
        if (!File.Exists(executable))
        {
            MessageBox.Show(this,
                "The CM26 3D viewer component is not installed beside this build.",
                "3D Face Viewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            _facePreviewCaption.Text = "Searching for the selected player's head mesh…";
            var exported = await Task.Run(() => Services.FrostbiteAssets.ExportMeshForQuery(
                new[] { $"head_{_currentHeadAssetId}", $"head_{_currentPlayerId}" }));
            if (IsDisposed) return;
            if (!string.IsNullOrWhiteSpace(exported))
            {
                _facePreviewCaption.Text = "3D head mesh exported · opening viewer…";
                Launch3DViewer(executable, exported);
                return;
            }
            _facePreviewCaption.Text = "Searching for the selected player's extracted 3D face…";
            var detected = await Task.Run(FindExtractedFaceFolder);
            if (IsDisposed) return;
            if (!string.IsNullOrWhiteSpace(detected))
            {
                _facePreviewCaption.Text = $"3D face assets found · {detected}";
                Launch3DViewer(executable, detected);
                return;
            }
            _facePreviewCaption.Text = "No head mesh or extracted face found; select an export folder.";
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            _facePreviewCaption.Text = "3D face export failed.";
            MessageBox.Show(this, ex.Message, "3D Face Viewer",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select an exported face folder containing head, hair, mouthbag FBX and textures",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Launch3DViewer(executable, dialog.SelectedPath);
    }

    private void Launch3DViewer(string executable, string assetPath)
    {
        try
        {
            var start = new ProcessStartInfo(executable)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(executable)
            };
            start.ArgumentList.Add(assetPath);
            Process.Start(start);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "3D Face Viewer",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshFacePreview()
    {
        if (_currentPlayerId <= 0) return;
        var image = Services.Assets.GetPlayerMiniface(_currentPlayerId);
        FrostbitePreviewLoader.LoadLegacyUiAsset(_facePreview, Services, image,
            $"data/ui/imgAssets/heads/p{_currentPlayerId}.dds", (preview, source) =>
        {
            if (IsDisposed) { preview?.Dispose(); return; }
            _facePreview.Image?.Dispose();
            _facePreview.Image = preview;
            _facePreviewCaption.Text = $"Face preview · {source}";
        });
    }

    private string? FindExtractedFaceFolder()
    {
        var roots = new List<string>();
        if (!string.IsNullOrWhiteSpace(SettingsService.AssetRoot))
            roots.Add(SettingsService.AssetRoot);
        roots.Add(Path.Combine(AppContext.BaseDirectory, "ExportedAssets", "Faces"));
        roots.AddRange(ExternalToolLocator.DriveRootFolders("FC26 FILE TOOL"));

        foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(root)) continue;
            foreach (var id in new[] { _currentHeadAssetId, _currentPlayerId }.Where(id => id > 0).Distinct())
            {
                try
                {
                    var match = Directory.EnumerateFiles(root, $"head_{id}_*_mesh.fbx",
                            SearchOption.AllDirectories)
                        .FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(match))
                        return Path.GetDirectoryName(match);
                }
                catch { /* An inaccessible optional export folder is skipped. */ }
            }
        }
        return null;
    }

    private sealed record ReferenceOption(string Display, string Value)
    {
        public override string ToString() => Display;
    }

    private static IReadOnlyList<ReferenceOption> PositionOptions() =>
        new[] { new ReferenceOption("Not set", "-1") }
            .Concat(Enumerable.Range(0, 28).Select(code =>
                new ReferenceOption(NameResolverService.PositionLabel(code), code.ToString())))
            .ToArray();

    private static IReadOnlyList<ReferenceOption> FootOptions() =>
        [new("Not set", "0"), new("Right", "1"), new("Left", "2")];

    private IReadOnlyList<ReferenceOption> NationOptions()
    {
        var options = new List<ReferenceOption> { new("Not set", "-1"), new("No nation", "0") };
        try
        {
            var nations = Services.RequireData().GetCountries();
            options.AddRange(nations
                .Where(n => !string.IsNullOrWhiteSpace(n.Title))
                .Select(n => new ReferenceOption(n.Title,
                    Services.Session.GetCell("nations", n.RecordIndex, "nationid")))
                .Where(option => int.TryParse(option.Value, out var nationId) && nationId > 0)
                .DistinctBy(option => option.Value));
        }
        catch { /* Section can be constructed before a database is attached. */ }
        return options;
    }

    private void AddReferenceDropdown(
        Control parent, string labelText, string field, Point point, int width,
        IReadOnlyList<ReferenceOption> options)
    {
        var labelWidth = Math.Max(70, point.X - 12 - 6);
        parent.Controls.Add(new Label
        {
            Text = labelText,
            Location = new Point(12, point.Y + 3),
            Size = new Size(labelWidth, 18),
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleRight,
            Font = LegacyFont,
            ForeColor = Theme.Muted,
            BackColor = Theme.Panel,
        });
        var picker = new ComboBox
        {
            Location = point,
            Size = new Size(width, 21),
            Tag = field,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = LegacyFont,
            IntegralHeight = false,
            DropDownHeight = 320,
        };
        Theme.ApplyCombo(picker);
        picker.Items.AddRange(options.Cast<object>().ToArray());
        picker.SelectedIndexChanged += (_, _) => StageReferencePicker(picker);
        ToolTip.SetToolTip(picker, $"Choose the {labelText.ToLowerInvariant()} used by FC26.");
        parent.Controls.Add(picker);
        _referencePickers[field] = picker;
    }

    private void StageReferencePicker(ComboBox picker)
    {
        if (_syncReferencePickers || CurrentRecordIndex < 0 || picker.Tag is not string field ||
            picker.SelectedItem is not ReferenceOption option ||
            !_fields.TryGetValue(field, out var value) || !value.IsWritable)
            return;
        if (StageField(TableName, CurrentRecordIndex, value.FieldName, option.Value, _stagingGrid))
        {
            value.Value = option.Value;
            RefreshSummaryMirrors(field);
        }
    }

    private void RefreshReferencePickers()
    {
        _syncReferencePickers = true;
        try
        {
            foreach (var (field, picker) in _referencePickers)
            {
                var raw = _fields.TryGetValue(field, out var value) ? value.RawValue : string.Empty;
                var selected = picker.Items.Cast<ReferenceOption>()
                    .FirstOrDefault(option => option.Value.Equals(raw, StringComparison.OrdinalIgnoreCase));
                picker.SelectedItem = selected ?? picker.Items.Cast<ReferenceOption>().FirstOrDefault();
                picker.Enabled = _fields.TryGetValue(field, out value) && value.IsWritable;
                picker.BackColor = picker.Enabled ? Theme.Input : Theme.Raised;
                picker.ForeColor = picker.Enabled ? Theme.Text : Theme.Muted;
            }
        }
        finally { _syncReferencePickers = false; }
    }

    private void AddFields(Control parent, IEnumerable<(string label, string field)> fields, int labelX, int top, int editorX, int width, int rowHeight)
    {
        var row = 0;
        // Fixed-width captions (ellipsized with a tooltip when narrow) so long
        // names such as "Preferred Position 1" never overlap their editors.
        var labelWidth = Math.Max(70, editorX - labelX - 6);
        foreach (var (label, field) in fields)
        {
            var y = top + (row++ * rowHeight);
            var caption = new Label
            {
                Text = label,
                Location = new Point(labelX, y + 3),
                Size = new Size(labelWidth, 18),
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Font = LegacyFont,
                Tag = label,
                ForeColor = Theme.Muted,
                BackColor = Theme.Panel,
            };
            parent.Controls.Add(caption);
            ToolTip.SetToolTip(caption, label);
            var edit = new TextBox { Location = new Point(editorX, y), Size = new Size(width, 20), Font = LegacyFont, Tag = field };
            Theme.ApplyTextBox(edit);
            edit.Leave += (_, _) => StageEdit(edit);
            _editors.Add(edit);
            parent.Controls.Add(edit);
        }
    }

    private void AddSummaryValue(Control parent, string label, string field, Point location)
    {
        try
        {
            var safeFont = LegacyFont ?? Theme.Body ?? new Font("Segoe UI", 9f);
            parent.Controls.Add(new Label { Text = label, Location = location, Size = new Size(120, 20), Font = safeFont });
            var value = new TextBox
            {
                Location = new Point(location.X + 125, location.Y), Size = new Size(115, 20),
                BorderStyle = BorderStyle.FixedSingle, BackColor = Theme.Input, ForeColor = Theme.Text,
                TextAlign = HorizontalAlignment.Center, Font = Theme.BodyBold ?? safeFont, Tag = field
            };
            value.Leave += (_, _) => StageSummary(value);
            if (!_summaryValues.TryGetValue(field, out var values)) _summaryValues[field] = values = [];
            values.Add(value);
            parent.Controls.Add(value);
        }
        catch (ArgumentException) { /* Skip field if font or layout is invalid on this system. */ }
    }

    private void StageEdit(TextBox editor)
    {
        if (CurrentRecordIndex < 0) return;
        var key = editor.Tag as string ?? string.Empty;
        if (_fields.TryGetValue(key, out var value) && value.IsWritable)
        {
            var candidate = editor.Text.Trim();
            if (key is "birthdate" or "playerjointeamdate")
            {
                if (!FifaDateConverter.TryFromIso(candidate, out candidate))
                {
                    MessageBox.Show(this, "Enter the date as YYYY-MM-DD.", "Invalid date",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    editor.Focus();
                    return;
                }
            }
            if (StageField(TableName, CurrentRecordIndex, value.FieldName, candidate, _stagingGrid))
                RefreshSummaryMirrors(key);
        }
    }

    private void StageSummary(TextBox editor)
    {
        if (CurrentRecordIndex < 0 || editor.ReadOnly || editor.Tag is not string field || !_fields.TryGetValue(field, out var value)) return;
        if (StageField(TableName, CurrentRecordIndex, value.FieldName, editor.Text.Trim(), _stagingGrid))
            RefreshSummaryMirrors(field);
    }

    private void UpdateSkillLabel(TrackBar slider)
    {
        if (slider.Tag is not string field || !_skillValues.TryGetValue(field, out var label)) return;
        label.Text = $"{FieldLabel(field)} {slider.Value}";
    }

    private void StageSkill(TrackBar slider)
    {
        if (_syncSkillSliders || CurrentRecordIndex < 0 || slider.Tag is not string field || !_fields.TryGetValue(field, out var value) || !value.IsWritable) return;
        if (StageField(TableName, CurrentRecordIndex, value.FieldName, slider.Value.ToString(), _stagingGrid))
            RefreshSummaryMirrors(field);
    }

    /// <summary>
    /// Keeps the "Player Summary"/"Key Attributes" mirrors in sync with the editor
    /// that staged the change (a summary box can be edited on the Info tab while
    /// the Skills tab or Identity Card shows the same underlying field).
    /// </summary>
    private void RefreshSummaryMirrors(string field)
    {
        if (!_summaryValues.TryGetValue(field, out var editors) || !_fields.TryGetValue(field, out var value)) return;
        var writable = value.IsWritable && field is not "preferredposition1" and not "nationality" and not "preferredfoot";
        var display = TryGetMappedDisplay(field, _currentPlayerId, value.RawValue, out var mapped) ? mapped : value.Value;
        foreach (var editor in editors)
        {
            editor.Text = display;
            editor.ReadOnly = !writable;
            editor.BackColor = writable ? Theme.Input : CardLayout.CardFieldBg;
            editor.ForeColor = CardLayout.CardText;
            ToolTip.SetToolTip(editor, writable ? field : field + " is a resolved value; edit its relationship in the appropriate picker.");
        }
    }

    protected override void ShowRecord(int recordIndex)
    {
        var table = Services.Session.GetTable(TableName)!;
        var record = Services.Session.GetRecord(TableName, recordIndex)!;
        var playerId = Parse(record.Get(Col(table, "playerid")));
        _currentPlayerId = playerId;
        var parts = Services.Resolver!.PlayerNameParts(playerId, Parse(record.Get(Col(table, "firstnameid"))), Parse(record.Get(Col(table, "lastnameid"))), Parse(record.Get(Col(table, "commonnameid"))));
        _playerName.Text = parts.KnownAs ?? $"Player {playerId}";
        ToolTip.SetToolTip(_playerName, _playerName.Text);
        _clubName.Text = Services.Resolver.PlayerClubName(playerId);
        var image = Services.Assets.GetPlayerMiniface(playerId);
        FrostbitePreviewLoader.LoadLegacyUiAsset(_miniface, Services, image,
            $"data/ui/imgAssets/heads/p{playerId}.dds", (preview, _) =>
        {
            if (IsDisposed) { preview?.Dispose(); return; }
            _miniface.Image?.Dispose();
            _miniface.Image = preview;
            _overviewFace.Image?.Dispose();
            _overviewFace.Image = preview == null ? null : new Bitmap(preview);
        });
        LegacyAssetActions.SetTarget(_miniface,
            new LegacyAssetEditTarget($"data/ui/imgAssets/heads/p{playerId}.dds", 128, 128));
        LegacyAssetActions.SetTarget(_facePreview,
            new LegacyAssetEditTarget($"data/ui/imgAssets/heads/p{playerId}.dds", 256, 256));
        FrostbitePreviewLoader.LoadLegacyUiAsset(_facePreview, Services, image,
            $"data/ui/imgAssets/heads/p{playerId}.dds", (preview, source) =>
        {
            if (IsDisposed) { preview?.Dispose(); return; }
            _facePreview.Image?.Dispose();
            _facePreview.Image = preview;
            _facePreviewCaption.Text = $"Face preview · {source}";
        });

        _fields.Clear();
        foreach (var field in Services.RequireData().GetFields(TableName, recordIndex, LabelMaps.Players))
            _fields[field.FieldName] = field;
        RefreshShoePreview();
        _currentHeadAssetId = _fields.TryGetValue("headclasscode", out var head)
            ? Parse(head.RawValue)
            : 0;
        RenderDatabaseTraits(table);
        foreach (var edit in _editors)
        {
            var key = edit.Tag as string ?? string.Empty;
            if (_fields.TryGetValue(key, out var value))
            {
                if (TryGetMappedDisplay(key, playerId, value.RawValue, out var display))
                {
                    edit.Text = display;
                    var editableDate = key is "birthdate" or "playerjointeamdate";
                    edit.ReadOnly = !editableDate || !value.IsWritable;
                    edit.BackColor = edit.ReadOnly ? CardLayout.CardFieldBg : Theme.Input;
                    edit.ForeColor = CardLayout.CardText;
                    ToolTip.SetToolTip(edit, editableDate
                        ? "Use YYYY-MM-DD. The value is converted to the database date format when staged."
                        : NameFieldTooltip(key, value.RawValue));
                }
                else
                {
                    edit.Text = value.Value;
                    edit.ReadOnly = !value.IsWritable;
                    edit.BackColor = value.IsWritable ? Theme.Input : CardLayout.CardFieldBg;
                    edit.ForeColor = CardLayout.CardText;
                    ToolTip.SetToolTip(edit, string.Empty);
                }
            }
            else
            {
                edit.Text = TryGetMappedDisplay(key, playerId, string.Empty, out var display) ? display : string.Empty;
                edit.ReadOnly = true;
                edit.BackColor = CardLayout.CardFieldBg;
                edit.ForeColor = CardLayout.CardSubtle;
                ToolTip.SetToolTip(edit, NameFieldTooltip(key, string.Empty));
            }
        }
        RefreshReferencePickers();
        _syncSkillSliders = true;
        try
        {
            foreach (var (field, label) in _skillValues)
            {
                if (_fields.TryGetValue(field, out var value) && int.TryParse(value.RawValue, out var rating))
                {
                    var clamped = Math.Clamp(rating, 1, 99);
                    label.Text = $"{FieldLabel(field)} {clamped}";
                    _skillSliders[field].Value = clamped;
                    _skillSliders[field].Enabled = value.IsWritable;
                }
                else
                {
                    label.Text = FieldLabel(field);
                    _skillSliders[field].Enabled = false;
                }
            }
        }
        finally { _syncSkillSliders = false; }
        foreach (var (field, labels) in _summaryValues)
        {
            var display = string.Empty;
            var writable = false;
            if (_fields.TryGetValue(field, out var value))
            {
                display = TryGetMappedDisplay(field, playerId, value.RawValue, out var mapped) ? mapped : value.Value;
                writable = value.IsWritable && field is not "preferredposition1" and not "nationality" and not "preferredfoot";
            }
            foreach (var editor in labels)
            {
                editor.Text = display;
                editor.ReadOnly = !writable;
                editor.BackColor = writable ? Theme.Input : CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardText;
                ToolTip.SetToolTip(editor, writable ? field : field + " is a resolved value; edit its relationship in the appropriate picker.");
            }
        }
        RefreshOverview();
        RefreshPlayerCallname();
    }

    private void RefreshOverview()
    {
        _overviewName.Text = _playerName.Text;
        var position = GetOverviewText("preferredposition1");
        var nation = GetOverviewText("nationality");
        _overviewMeta.Text = string.Join("  ·  ", new[] { position, _clubName.Text, nation }.Where(text => !string.IsNullOrWhiteSpace(text)));
        _overviewOverall.Text = GetOverviewNumber("overallrating").ToString();
        _overviewPotential.Text = GetOverviewNumber("potential").ToString();
        _overviewGrowth.Text = $"+{Math.Max(0, GetOverviewNumber("potential") - GetOverviewNumber("overallrating"))}";
        foreach (var (mode, button) in _playerLayoutButtons)
        {
            var active = string.Equals(mode, _playerStatsLayout, StringComparison.OrdinalIgnoreCase);
            button.BackColor = active ? Color.FromArgb(137, 202, 35) : Color.White;
            button.ForeColor = Color.FromArgb(42, 42, 39);
        }
        foreach (var (name, button) in _playerCategoryButtons)
        {
            var active = (name == "Bars") == _playerUseBars;
            button.BackColor = active ? Color.FromArgb(246, 183, 36) : Color.White;
            button.ForeColor = Color.FromArgb(42, 42, 39);
        }
        foreach (var (field, label) in _overviewFacts)
        {
            var value = string.Equals(field, "club", StringComparison.OrdinalIgnoreCase)
                ? _clubName.Text
                : GetOverviewText(field);
            label.Text = string.IsNullOrWhiteSpace(value) ? "—" : value + (label.Tag as string ?? string.Empty);
        }
        foreach (var (field, labels) in _overviewAttributeValues)
        {
            var value = GetOverviewNumber(field);
            foreach (var label in labels)
            {
                var writable = _fields.TryGetValue(field, out var data) && data.IsWritable;
                label.Text = value.ToString();
                label.ReadOnly = !writable;
                label.BackColor = writable ? Color.FromArgb(243, 250, 237) : Color.White;
                label.ForeColor = writable ? Color.FromArgb(42, 111, 29) : Color.FromArgb(100, 100, 96);
                ToolTip.SetToolTip(label, writable ? "Enter a value from 0 to 99, then press Enter." : "This FC26 field is not writable.");
            }
        }
        foreach (var (field, labels) in _overviewSupplementValues)
        {
            var text = GetOverviewText(field);
            foreach (var label in labels)
                label.Text = string.IsNullOrWhiteSpace(text) ? "-" : text;
        }
        foreach (var (field, bars) in _playerStatBars)
        {
            var score = GetOverviewNumber(field);
            foreach (var track in bars)
            {
                track.Visible = _playerUseBars;
                if (track.Controls.Count > 0)
                    track.Controls[0].Width = Math.Max(1, (int)Math.Round(track.Width * Math.Clamp(score, 0, 99) / 99d));
            }
        }
        foreach (var (code, value) in _overviewRatings)
        {
            var fields = value.Tag as string[] ?? [];
            var available = fields.Select(GetOverviewNumber).Where(number => number > 0).ToArray();
            var rating = available.Length == 0 ? 0 : (int)Math.Round(available.Average());
            value.Text = rating == 0 ? "—" : rating.ToString();
            if (_overviewBars.TryGetValue(code, out var fill))
                fill.Width = Math.Max(1, (int)Math.Round(390 * Math.Clamp(rating, 0, 99) / 99d));
        }
    }

    private int GetOverviewNumber(string field) => _fields.TryGetValue(field, out var value) && int.TryParse(value.RawValue, out var parsed) ? parsed : 0;

    private string GetOverviewText(string field)
    {
        if (!_fields.TryGetValue(field, out var value)) return string.Empty;
        if (field.StartsWith("role", StringComparison.OrdinalIgnoreCase) && int.TryParse(value.RawValue, out var role))
            return role is > 0 and <= 23 ? RoleNames[role - 1] : string.Empty;
        return TryGetMappedDisplay(field, _currentPlayerId, value.RawValue, out var mapped) ? mapped : value.Value;
    }

    private void RefreshPlayerCallname()
    {
        var row = FindRow("playernamemap", "playerid", _currentPlayerId);
        if (row < 0)
        {
            _callnameId.Text = string.Empty;
            _callnameStatus.Text = $"Player {_currentPlayerId} has no commentary callname mapping.";
            return;
        }
        var commentaryId = Services.Session.GetCell("playernamemap", row, "commentaryid");
        _callnameId.Text = commentaryId;
        var commentaryRow = FindRow("commentarynames", "commentaryid", Parse(commentaryId));
        var commentaryString = commentaryRow < 0
            ? "commentary entry not found"
            : Services.Session.GetCell("commentarynames", commentaryRow, "commentarystring");
        _callnameStatus.Text =
            $"Player {_currentPlayerId} → Commentary {commentaryId} · {commentaryString}";
    }

    private void AssignPlayerCallname()
    {
        if (_currentPlayerId <= 0 || !int.TryParse(_callnameId.Text.Trim(), out var commentaryId))
        {
            MessageBox.Show(this, "Enter a valid numeric Commentary Id.", "Player Callname",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (FindRow("commentarynames", "commentaryid", commentaryId) < 0)
        {
            MessageBox.Show(this,
                $"Commentary Id {commentaryId} does not exist in the installed commentary catalog.",
                "Player Callname", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var map = Services.Session.GetTable("playernamemap");
        if (map == null || map.RowCount == 0)
        {
            MessageBox.Show(this, "The player callname mapping table is unavailable.", "Player Callname",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var row = FindRow("playernamemap", "playerid", _currentPlayerId);
        if (row < 0)
        {
            var duplicate = Services.Session.DuplicateRow("playernamemap", 0);
            if (!duplicate.Success)
            {
                MessageBox.Show(this, duplicate.Message, "Player Callname",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Services.Pending.MarkStructuralChange();
            row = 1;
            if (!Services.Pending.Stage("playernamemap", row, "playerid", _currentPlayerId.ToString()).Success)
                return;
        }
        var result = Services.Pending.Stage(
            "playernamemap", row, "commentaryid", commentaryId.ToString());
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, "Player Callname",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Services.Session.RefreshSchema();
        RefreshPlayerCallname();
        MessageBox.Show(this, "Player callname mapping staged. Use Save to write the pending change.",
            "Player Callname", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RemovePlayerCallname()
    {
        var row = FindRow("playernamemap", "playerid", _currentPlayerId);
        if (row < 0) return;
        if (MessageBox.Show(this, "Remove this player's commentary callname mapping?",
                "Player Callname", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var result = Services.Session.DeleteRow("playernamemap", row);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, "Player Callname",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Services.Pending.MarkStructuralChange();
        Services.Session.RefreshSchema();
        RefreshPlayerCallname();
    }

    private int FindRow(string tableName, string fieldName, int wanted)
    {
        var table = Services.Session.GetTable(tableName);
        if (table == null) return -1;
        for (var row = 0; row < table.RowCount; row++)
            if (Parse(Services.Session.GetCell(tableName, row, fieldName)) == wanted)
                return row;
        return -1;
    }

    private void OpenCallnameGenerator()
    {
        var executable = ExternalToolLocator.FindFile(
            Path.Combine("CALLNAME TTS TOOL BY RIZCO98", "Publish", "Latest", "CallName_TTS_Tool_By_Rizco98.exe"),
            Path.Combine("CALLNAME TTS TOOL BY RIZCO98", "dist", "CallName_TTS_Tool_By_Rizco98.exe"),
            Path.Combine("Callname TTS", "CallName_TTS_Tool_By_Rizco98.exe"));
        if (executable == null)
        {
            MessageBox.Show(this, "Callname TTS Tool was not found.", "Player Callname",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Process.Start(new ProcessStartInfo(executable)
        {
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(executable)
        });
    }

    private void RenderDatabaseTraits(DbTable table)
    {
        if (_traitsPanel == null) return;
        foreach (var editor in _traitEditors) _editors.Remove(editor);
        _traitEditors.Clear();
        _traitsPanel.Controls.Clear();
        var chips = new FlowLayoutPanel
        {
            Location = new Point(12, 23), Size = new Size(690, 53), AutoScroll = true,
            WrapContents = true, BackColor = Color.White, Padding = new Padding(2), Margin = Padding.Empty
        };
        var names = DecodePlaystyles();
        if (names.Count == 0)
            chips.Controls.Add(new Label { Text = "No active Playstyles", AutoSize = true, Padding = new Padding(7, 5, 7, 5), Font = Theme.Body, ForeColor = Theme.Muted });
        else
            foreach (var (name, plus) in names)
                chips.Controls.Add(CreatePlaystyleChip(name, plus));
        _traitsPanel.Controls.Add(chips);

        var edit = new Button { Text = "Edit playstyles…", Location = new Point(714, 32), Size = new Size(126, 28), FlatStyle = FlatStyle.Flat, Font = Theme.Muted9 };
        edit.FlatAppearance.BorderColor = Color.FromArgb(116, 185, 34);
        edit.Click += (_, _) => OpenSinglePlayerEditor();
        _traitsPanel.Controls.Add(edit);
    }

    private static Control CreatePlaystyleChip(string name, bool plus) => new Label
    {
        Text = plus ? name + " +" : name, AutoSize = true, Margin = new Padding(3, 2, 3, 2), Padding = new Padding(8, 4, 8, 4),
        Font = Theme.BodyBold, ForeColor = plus ? Color.FromArgb(48, 112, 27) : Color.FromArgb(67, 67, 62),
        BackColor = plus ? Color.FromArgb(225, 243, 213) : Color.FromArgb(239, 241, 236), BorderStyle = BorderStyle.FixedSingle
    };

    private List<(string Name, bool Plus)> DecodePlaystyles()
    {
        var result = new List<(string Name, bool Plus)>();
        AddPlaystyleMask("trait1", 0, false, result);
        AddPlaystyleMask("trait2", 32, false, result);
        AddPlaystyleMask("icontrait1", 0, true, result);
        AddPlaystyleMask("icontrait2", 32, true, result);
        return result.Distinct().ToList();
    }

    private void AddPlaystyleMask(string field, int offset, bool plus, ICollection<(string Name, bool Plus)> output)
    {
        if (!_fields.TryGetValue(field, out var value) || !uint.TryParse(value.RawValue, out var mask)) return;
        for (var bit = 0; bit < 32; bit++)
        {
            var index = offset + bit;
            if ((mask & (1u << bit)) == 0 || index >= PlaystyleNames.Length) continue;
            output.Add((PlaystyleNames[index], plus));
        }
    }

    /// <summary>Render FC26 foreign keys/codes as their database-backed meaning.  These fields
    /// are intentionally read-only in this legacy canvas so a display string can never be saved
    /// back over the underlying numeric ID.</summary>
    private bool TryGetMappedDisplay(string field, int playerId, string rawValue, out string display)
    {
        var resolver = Services.Resolver;
        display = string.Empty;
        if (resolver == null) return false;
        if (field.Equals("jerseynumber", StringComparison.OrdinalIgnoreCase))
        {
            var shirt = resolver.PlayerJersey(playerId);
            display = shirt is > 0 ? shirt.Value.ToString() : "Not linked";
            return true;
        }
        if (field.Equals("nationality", StringComparison.OrdinalIgnoreCase) && int.TryParse(rawValue, out var nation))
        {
            display = resolver.NationName(nation);
            return true;
        }
        if (field.StartsWith("preferredposition", StringComparison.OrdinalIgnoreCase) && int.TryParse(rawValue, out var position))
        {
            display = NameResolverService.PositionLabel(position);
            return true;
        }
        if (field.Equals("preferredfoot", StringComparison.OrdinalIgnoreCase) && int.TryParse(rawValue, out var foot))
        {
            display = NameResolverService.PreferredFootLabel(foot);
            return true;
        }
        if (field is "birthdate" or "playerjointeamdate")
        {
            display = FifaDateConverter.TryToIso(rawValue, out var iso) ? iso : "Not set";
            return true;
        }
        if (field is "firstnameid" or "lastnameid" or "commonnameid")
        {
            // Resolve the localization ID to a real name. Show the honest "Unavailable" fallback
            // when the readable name source is absent — never display the raw numeric ID as a name.
            if (!int.TryParse(rawValue, out var nameId) || nameId <= 0)
            {
                display = "Not set";
                return true;
            }
            var resolved = resolver.PlayerNames.NameById(nameId);
            display = resolved ?? "Unavailable";
            return true;
        }
        return false;
    }

    private void RefreshShoePreview()
    {
        var shoeType = FieldInt("shoetypecode");
        var design = FieldInt("shoedesigncode");
        var color1 = FieldInt("shoecolorcode1");
        var color2 = FieldInt("shoecolorcode2");
        var legacyPath = $"data/ui/imgAssets/boots/item_{shoeType}_{design}_{color1}_{color2}.dds";
        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _shoePreview,
            Services,
            Services.Assets.GetBoot(shoeType),
            legacyPath,
            (image, _) =>
            {
                _shoePreview.Image?.Dispose();
                _shoePreview.Image = image;
            });
    }

    private int FieldInt(string field) =>
        _fields.TryGetValue(field, out var value) && int.TryParse(value.RawValue, out var parsed)
            ? parsed
            : 0;

    /// <summary>Explain, for name-reference fields, where the shown value comes from and whether the
    /// readable name source is available. Returns a non-intrusive note for other mapped fields.</summary>
    private string NameFieldTooltip(string field, string rawValue)
    {
        var resolver = Services.Resolver;
        if (field is "firstnameid" or "lastnameid" or "commonnameid")
        {
            var idText = string.IsNullOrWhiteSpace(rawValue) ? "0" : rawValue;
            if (resolver?.PlayerNames.NamesDecodable == true)
                return $"{field} = {idText} · resolved from the loaded database";
            return $"{field} = {idText} · readable name data is unavailable";
        }
        return $"{field} = {rawValue}";
    }

    private static string FieldLabel(string field) => field switch
    {
        "overallrating" => "Overall Rating", "gkreflexes" => "GK Reflexes", "gkhandling" => "GK Handling", "gkdiving" => "GK Diving", "gkpositioning" => "GK Positioning", "gkkicking" => "GK Kicking",
        "defensiveawareness" => "Defensive Awareness", "standingtackle" => "Standing Tackle", "slidingtackle" => "Sliding Tackle", "shortpassing" => "Short Passing", "longpassing" => "Long Passing",
        "ballcontrol" => "Ball Control", "shotpower" => "Shot Power", "longshots" => "Long Shots", "headingaccuracy" => "Heading Accuracy", "freekickaccuracy" => "Free Kick Accuracy",
        "sprintspeed" => "Sprint Speed", "skillmoves" => "Skill Moves", _ => string.Concat(field.Select((c, i) => i == 0 ? char.ToUpperInvariant(c).ToString() : c.ToString()))
    };
}
