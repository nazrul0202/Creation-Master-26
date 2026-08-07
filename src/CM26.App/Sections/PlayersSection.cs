using System.Drawing;
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
    private readonly PictureBox _shoePreview = new();
    private readonly PictureBox _facePreview = new();
    private readonly Label _facePreviewCaption = new();
    private readonly Label _playerName = new();
    private readonly Label _clubName = new();
    private readonly Dictionary<string, Label> _skillValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<TextBox>> _summaryValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TrackBar> _skillSliders = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<TextBox> _traitEditors = [];
    private readonly TextBox _callnameId = new();
    private readonly Label _callnameStatus = new();
    private GroupBox? _traitsPanel;
    private bool _syncSkillSliders;
    private int _currentPlayerId;
    private int _currentHeadAssetId;

    public override string SectionKey => "players";
    public override string SectionTitle => "Players";
    protected override string TableName => "players";
    protected override bool SupportsCreate => true;
    protected override string RecordSearchPlaceholder => "Search players…";

    public PlayersSection(AppServices services) : base(services)
    {
        Tabs.Font = LegacyFont;
        Tabs.Padding = new Point(4, 2);
        AddInfoTab();
        AddSkillsTab();
        AddFaceTab();
        AddDetailsTab();
        AddCallnameTab();
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Services.RequireData().GetPlayers();

    protected override void CreateNewRecord()
    {
        if (!EntityCreationDialog.TryShow(this, "Player",
                [("First name", "New"), ("Surname", "Player")], out var values))
            return;
        try
        {
            var requestedName = $"{values[0]} {values[1]}".Trim();
            if (GetRecords().Any(item => string.Equals(item.Title.Trim(), requestedName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A player with that name already exists. Add a distinguishing name before creating the record.");
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
                ["preferredposition1"] = "25",
                ["preferredposition2"] = "-1",
                ["preferredposition3"] = "-1",
                ["preferredposition4"] = "-1",
                ["preferredfoot"] = "1",
                ["height"] = "180",
                ["weight"] = "75",
                ["jerseynumber"] = "0",
                ["isretiring"] = "0",
            });
            var nameSaved = TryCreateEditedPlayerName(id, values[0], values[1]);
            Services.SetPlayerNameOverride(id, values[0], values[1]);
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
    private static GroupBox Box(string name, Point point, Size size) => new ModernGroupBox { Text = name, Location = point, Size = size };
    private static PictureBox Viewer(Point point, Size size) => new() { Location = point, Size = size, BackColor = Theme.Input, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

    private void AddInfoTab()
    {
        var page = Page("Info");
        var canvas = Canvas(page);

        var identity = Box("Identity Card", new Point(3, 3), new Size(390, 220));
        _miniface.Location = new Point(12, 20);
        _miniface.Size = new Size(100, 100);
        _miniface.BackColor = Theme.Input;
        _miniface.BorderStyle = BorderStyle.FixedSingle;
        _miniface.SizeMode = PictureBoxSizeMode.Zoom;
        identity.Controls.Add(_miniface);
        LegacyAssetActions.Attach(Services, identity, _miniface, new Point(12, 124), () => ShowRecord(CurrentRecordIndex));
        _playerName.Location = new Point(11, 196);
        _playerName.Size = new Size(365, 20);
        _playerName.Font = LegacyFont;
        _playerName.AutoEllipsis = true;
        identity.Controls.Add(_playerName);
        AddFields(identity, new[]
        {
            ("Player Id", "playerid"), ("First Name", "firstnameid"), ("Surname", "lastnameid"), ("Common Name", "commonnameid"),
            ("Jersey", "jerseynumber"), ("Birthdate", "birthdate"), ("Country", "nationality")
        }, 155, 20, 255, 120, 26);
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

        var body = Box("Body", new Point(3, 229), new Size(390, 154));
        AddFields(body, new[] { ("Height", "height"), ("Weight", "weight"), ("Body", "bodytypecode"), ("Best foot", "preferredfoot"), ("Weak foot", "weakfootabilitytypecode") }, 12, 18, 245, 120, 26);
        canvas.Controls.Add(body);

        var look = Box("Look", new Point(3, 391), new Size(390, 252));
        AddFields(look, new[]
        {
            ("Jersey Style", "jerseystylecode"), ("Sleeves Length", "jerseysleevelengthcode"), ("Socks Style", "sockstylecode"),
            ("Socks Length", "socklengthcode"), ("GK Gloves", "gkglovetypecode"), ("Shoes Type", "shoetypecode"),
            ("Undershort Style", "undershortstyle"), ("Short Style", "shortstyle"), ("Jersey Fit", "jerseyfit")
        }, 12, 18, 145, 240, 26);
        canvas.Controls.Add(look);

        var shoes = Box("Boots", new Point(399, 229), new Size(245, 154));
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

        var play = Box("Playing Info", new Point(399, 391), new Size(245, 155));
        AddFields(play, new[]
        {
            ("Preferred Position 1", "preferredposition1"), ("Preferred Position 2", "preferredposition2"),
            ("Preferred Position 3", "preferredposition3"), ("Preferred Position 4", "preferredposition4"), ("International Reputation", "internationalrep")
        }, 12, 25, 148, 92, 26);
        canvas.Controls.Add(play);

        // A compact FC26 player overview, while keeping the original CM16 group-box
        // visual language instead of replacing this page with a modern card UI.
        var summary = Box("Player Summary", new Point(650, 3), new Size(555, 129));
        AddSummaryValue(summary, "Overall", "overallrating", new Point(12, 25));
        AddSummaryValue(summary, "Potential", "potential", new Point(12, 51));
        AddSummaryValue(summary, "Position", "preferredposition1", new Point(12, 77));
        AddSummaryValue(summary, "Nation", "nationality", new Point(12, 103));
        AddSummaryValue(summary, "Height", "height", new Point(280, 25));
        AddSummaryValue(summary, "Weight", "weight", new Point(280, 51));
        AddSummaryValue(summary, "Preferred Foot", "preferredfoot", new Point(280, 77));
        AddSummaryValue(summary, "International Rep.", "internationalrep", new Point(280, 103));
        canvas.Controls.Add(summary);

        var attributes = Box("Key Attributes", new Point(650, 138), new Size(555, 181));
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
        var technical = Box("Record and Contract", new Point(650, 325), new Size(555, 106));
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

        _traitsPanel = Box("Player Traits", new Point(3, 327), new Size(620, 300));
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
        var preview = Box("Face Preview", new Point(3, 3), new Size(745, 451));
        _facePreview.Location = new Point(8, 20);
        _facePreview.Size = new Size(728, 390);
        _facePreview.BackColor = Theme.Raised;
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
        var modelling = Box("Face Modelling", new Point(3, 462), new Size(745, 204));
        AddFields(modelling, new[] { ("Head Model", "headclasscode"), ("Head Type", "headtypecode"), ("Head Variation", "headvariation"), ("Head Asset Id", "headassetid"), ("Hair Model", "hairstylecode"), ("Hair Type", "hairtypecode"), ("High Quality Head", "hashighqualityhead") }, 16, 22, 150, 260, 26);
        canvas.Controls.Add(modelling);
        var appearance = Box("Appearance", new Point(3, 674), new Size(745, 256));
        AddFields(appearance, new[]
        {
            ("Hair Color", "haircolorcode"), ("Facial Hair", "facialhairtypecode"), ("Facial Hair Color", "facialhaircolorcode"),
            ("Skin Tone", "skintonecode"), ("Eyes Color", "eyecolorcode"), ("Eye Detail", "eyedetail"),
            ("Eyebrow Code", "eyebrowcode"), ("Sideburns Code", "sideburnscode"), ("Lip Color", "lipcolor")
        }, 16, 22, 150, 260, 26);
        canvas.Controls.Add(appearance);
        var skin = Box("Skin Details", new Point(3, 938), new Size(745, 230));
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

        var tattoos = Box("Tattoos", new Point(3, 3), new Size(440, 207));
        AddFields(tattoos, new[]
        {
            ("Head Tattoo", "tattoohead"), ("Front Tattoo", "tattoofront"), ("Back Tattoo", "tattoback"),
            ("Left Arm", "tattooleftarm"), ("Right Arm", "tattoorightarm"),
            ("Left Leg", "tattooleftleg"), ("Right Leg", "tattoorightleg")
        }, 12, 25, 145, 260, 26);
        canvas.Controls.Add(tattoos);

        var accessories = Box("Accessories", new Point(449, 3), new Size(440, 233));
        AddFields(accessories, new[]
        {
            ("Accessory 1", "accessorycode1"), ("Accessory 2", "accessorycode2"),
            ("Accessory 3", "accessorycode3"), ("Accessory 4", "accessorycode4"),
            ("Colour 1", "accessorycolourcode1"), ("Colour 2", "accessorycolourcode2"),
            ("Colour 3", "accessorycolourcode3"), ("Colour 4", "accessorycolourcode4")
        }, 12, 25, 145, 260, 26);
        canvas.Controls.Add(accessories);

        var positions = Box("Preferred Positions", new Point(3, 218), new Size(440, 100));
        AddFields(positions, new[]
        {
            ("Preferred Position 5", "preferredposition5"), ("Preferred Position 6", "preferredposition6"),
            ("Preferred Position 7", "preferredposition7")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(positions);

        var roles = Box("Playing Roles", new Point(449, 242), new Size(440, 204));
        AddFields(roles, new[]
        {
            ("Role 1", "role1"), ("Role 2", "role2"), ("Role 3", "role3"), ("Role 4", "role4"), ("Role 5", "role5"),
            ("Finishing Code 1", "finishingcode1"), ("Finishing Code 2", "finishingcode2")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(roles);

        var goalkeeper = Box("Goalkeeper Style", new Point(3, 326), new Size(440, 74));
        AddFields(goalkeeper, new[]
        {
            ("Save Type", "gksavetype"), ("Kick Style", "gkkickstyle")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(goalkeeper);

        var profile = Box("Player Profile", new Point(3, 406), new Size(440, 259));
        AddFields(profile, new[]
        {
            ("Gender", "gender"), ("Personality", "personality"), ("Emotion", "emotion"),
            ("Run Style", "runstylecode"), ("Running Code 1", "runningcode1"), ("Running Code 2", "runningcode2"),
            ("Free Kick Start Pose", "animfreekickstartposcode"), ("Penalty Start Pose", "animpenaltiesstartposcode"),
            ("Skill Moves Likelihood", "skillmoveslikelihood")
        }, 12, 25, 145, 260, 26);
        canvas.Controls.Add(profile);

        var development = Box("Development", new Point(449, 452), new Size(440, 178));
        AddFields(development, new[]
        {
            ("Pace Division", "pacdiv"), ("Dribble Reference", "driref"), ("Defence Reference", "defspe"),
            ("Passing Reference", "paskic"), ("Physical Reference", "phypos"), ("Modifier", "modifier")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(development);

        var customisation = Box("Customisation", new Point(449, 636), new Size(440, 178));
        AddFields(customisation, new[]
        {
            ("User Can Edit Name", "usercaneditname"), ("Is Customized", "iscustomized"),
            ("Avatar POM Id", "avatarpomid"), ("Shohan", "shohan"),
            ("Jersey Name Id", "playerjerseynameid"), ("Small Sided Shoes", "smallsidedshoetypecode")
        }, 12, 22, 145, 260, 26);
        canvas.Controls.Add(customisation);

        var iconTraits = Box("Icon Traits", new Point(3, 671), new Size(440, 74));
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
        parent.Controls.Add(new Label { Text = label, Location = location, Size = new Size(120, 20), Font = LegacyFont });
        var value = new TextBox
        {
            Location = new Point(location.X + 125, location.Y), Size = new Size(115, 20),
            BorderStyle = BorderStyle.FixedSingle, BackColor = Theme.Input, ForeColor = Theme.Text,
            TextAlign = HorizontalAlignment.Center, Font = Theme.BodyBold, Tag = field
        };
        value.Leave += (_, _) => StageSummary(value);
        if (!_summaryValues.TryGetValue(field, out var values)) _summaryValues[field] = values = [];
        values.Add(value);
        parent.Controls.Add(value);
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
            editor.BackColor = writable ? Theme.Input : Theme.Raised;
            editor.ForeColor = Theme.Text;
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
                    edit.BackColor = edit.ReadOnly ? Theme.Raised : Theme.Input;
                    edit.ForeColor = Theme.Text;
                    ToolTip.SetToolTip(edit, editableDate
                        ? "Use YYYY-MM-DD. The value is converted to the database date format when staged."
                        : NameFieldTooltip(key, value.RawValue));
                }
                else
                {
                    edit.Text = value.Value;
                    edit.ReadOnly = !value.IsWritable;
                    edit.BackColor = value.IsWritable ? Theme.Input : Theme.Raised;
                    edit.ForeColor = Theme.Text;
                    ToolTip.SetToolTip(edit, string.Empty);
                }
            }
            else
            {
                edit.Text = TryGetMappedDisplay(key, playerId, string.Empty, out var display) ? display : string.Empty;
                edit.ReadOnly = true;
                edit.BackColor = Theme.Raised;
                edit.ForeColor = Theme.Muted;
                ToolTip.SetToolTip(edit, NameFieldTooltip(key, string.Empty));
            }
        }
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
                editor.BackColor = writable ? Theme.Input : Theme.Raised;
                editor.ForeColor = Theme.Text;
                ToolTip.SetToolTip(editor, writable ? field : field + " is a resolved value; edit its relationship in the appropriate picker.");
            }
        }
        RefreshPlayerCallname();
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
        var traitFields = table.Columns
            .Where(c => c.Name.Contains("trait", StringComparison.OrdinalIgnoreCase) ||
                        c.Name.Contains("playstyle", StringComparison.OrdinalIgnoreCase) ||
                        c.Name.Contains("speciality", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Name)
            .ToList();
        var neededHeight = traitFields.Count == 0
            ? 60
            : 24 + ((traitFields.Count + 1) / 2) * 30 + 10;
        if (_traitsPanel.Height != neededHeight) _traitsPanel.Height = neededHeight;
        if (traitFields.Count == 0)
        {
            _traitsPanel.Controls.Add(new Label
            {
                Text = "This database has no separate player trait or playstyle fields.",
                Location = new Point(14, 24), Size = new Size(580, 36), Font = LegacyFont, ForeColor = Theme.Muted
            });
            return;
        }

        for (var index = 0; index < traitFields.Count; index++)
        {
            var field = traitFields[index];
            var x = 14 + ((index % 2) * 294);
            var y = 24 + ((index / 2) * 30);
            _traitsPanel.Controls.Add(new Label { Text = FieldLabel(field), Location = new Point(x, y + 4), Size = new Size(160, 18), Font = LegacyFont, ForeColor = Theme.Text });
            var editor = new TextBox { Location = new Point(x + 165, y), Size = new Size(105, 20), Font = LegacyFont, Tag = field };
            Theme.ApplyTextBox(editor);
            editor.Leave += (_, _) => StageEdit(editor);
            _editors.Add(editor);
            _traitEditors.Add(editor);
            if (_fields.TryGetValue(field, out var value))
            {
                editor.Text = value.Value;
                editor.ReadOnly = !value.IsWritable;
                editor.BackColor = value.IsWritable ? Theme.Input : Theme.Raised;
                editor.ForeColor = Theme.Text;
            }
            else
            {
                editor.ReadOnly = true;
                editor.BackColor = Theme.Raised;
                editor.ForeColor = Theme.Text;
            }
            _traitsPanel.Controls.Add(editor);
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
