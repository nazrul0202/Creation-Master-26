using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;
using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>
/// Visual FC26 adapters for the remaining original CM16 entity forms.  The
/// original .cs form files are linked into CM16Source as the geometry contract;
/// this class intentionally uses the old System.Windows.Forms canvas, compact
/// fonts, fixed group boxes and image areas rather than the generic CRUD grid.
/// </summary>
public abstract class ClassicEntitySection : SectionBase
{
    protected static readonly Font LegacyFont = Theme.Body;
    private readonly string _key, _title, _table;
    private readonly Func<IReadOnlyList<RecordListItem>> _records;
    private readonly IReadOnlyDictionary<string, string> _labels;
    private readonly Dictionary<string, FieldValue> _values = new(StringComparer.OrdinalIgnoreCase);
    protected readonly List<TextBox> _editors = [];
    protected readonly List<TextBox> _mirrors = [];
    private readonly FieldEditorGrid _staging = new();

    protected ClassicEntitySection(AppServices services, string key, string title, string table,
        Func<IReadOnlyList<RecordListItem>> records, IReadOnlyDictionary<string, string> labels) : base(services)
    {
        _key = key; _title = title; _table = table; _records = records; _labels = labels;
        Header.Visible = false;
        Tabs.Padding = new Point(3, 1);
    }

    public override string SectionKey => _key;
    public override string SectionTitle => _title;
    protected override string TableName => _table;
    protected override IReadOnlyList<RecordListItem> GetRecords() => _records();

    protected TabPage AddCanvasTab(string title)
    {
        var page = new TabPage(title) { BackColor = Theme.Background, Font = LegacyFont };
        page.Controls.Add(new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CardLayout.CardBackground });
        Tabs.TabPages.Add(page);
        return page;
    }

    protected Panel Canvas(TabPage page) => (Panel)page.Controls[0];

    protected Panel Group(string text, Point point, Size size)
    {
        var group = new Panel
        {
            Location = point,
            Size = size,
            BackColor = CardLayout.CardWhite,
        };
        CardLayout.ApplyRounded(group, 10);
        group.Controls.Add(new Panel
        {
            Location = Point.Empty,
            Size = new Size(size.Width, 4),
            BackColor = CardLayout.Fc26Green,
            Dock = DockStyle.Top,
        });
        group.Controls.Add(new Label
        {
            Text = text,
            // Field-based legacy panels commonly begin at Y=18/20.  The title
            // therefore occupies a compact header band which ends before row 1.
            Location = new Point(10, 6),
            Size = new Size(size.Width - 20, 13),
            Font = Theme.Muted9,
            ForeColor = CardLayout.Fc26Green,
            BackColor = CardLayout.CardWhite,
        });
        return group;
    }

    protected void AddField(Control parent, string field, string caption, Point point, int width = 150)
    {
        var captionWidth = Math.Clamp(point.X - 14, 40, 190);
        var captionX = point.X - captionWidth - 6;
        var label = new Label
        {
            Text = caption,
            Location = new Point(captionX, point.Y + 3),
            Size = new Size(captionWidth, 18),
            Font = LegacyFont,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true,
            BackColor = CardLayout.CardWhite,
            ForeColor = CardLayout.CardFieldLabel,
        };
        parent.Controls.Add(label);
        ToolTip.SetToolTip(label, caption);
        var box = new TextBox { Location = point, Size = new Size(width, 20), Tag = field, Font = LegacyFont, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(box);
        box.Leave += (_, _) => Commit(box);
        parent.Controls.Add(box);
        _editors.Add(box);
    }

    protected void AddReadonlyField(Control parent, string field, string caption, Point point, int width = 150)
    {
        var captionWidth = Math.Clamp(point.X - 14, 40, 190);
        var captionX = point.X - captionWidth - 6;
        parent.Controls.Add(new Label
        {
            Text = caption,
            Location = new Point(captionX, point.Y + 3),
            Size = new Size(captionWidth, 18),
            Font = LegacyFont,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true,
            BackColor = CardLayout.CardWhite,
            ForeColor = CardLayout.CardFieldLabel,
        });
        var box = new TextBox { Location = point, Size = new Size(width, 20), Tag = field, ReadOnly = true, Font = LegacyFont, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(box);
        box.BackColor = CardLayout.CardFieldBg;
        box.ForeColor = CardLayout.CardText;
        parent.Controls.Add(box);
        _mirrors.Add(box);
    }

    protected PictureBox ImageSurface(Control parent, Point point, Size size, string caption)
    {
        var holder = new Panel { Location = point, Size = new Size(size.Width, size.Height + 21), BackColor = CardLayout.CardWhite };
        var pic = new PictureBox { Size = size, BackColor = CardLayout.CardFieldBg, BorderStyle = BorderStyle.None, SizeMode = PictureBoxSizeMode.Zoom };
        holder.Controls.Add(pic);
        holder.Controls.Add(new Label { Text = caption, Location = new Point(0, size.Height + 2), Size = new Size(size.Width, 18), Font = LegacyFont, ForeColor = CardLayout.CardSubtle, TextAlign = ContentAlignment.MiddleCenter, BackColor = CardLayout.CardWhite });
        parent.Controls.Add(holder);
        return pic;
    }

    protected void AddReadonlyNote(Control parent, string text, Point point, Size size)
    {
        parent.Controls.Add(new Label { Text = text, Location = point, Size = size, Font = LegacyFont, ForeColor = CardLayout.CardSubtle, BackColor = CardLayout.CardWhite, TextAlign = ContentAlignment.MiddleCenter });
    }

    /// <summary>Loads a local image (including DDS) without locking its source file.</summary>
    protected void ShowAsset(PictureBox viewer, string? path)
    {
        viewer.Image?.Dispose();
        viewer.Image = null;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
        try
        {
            viewer.Image = Services.Textures.CreatePreview(path, viewer.Width, viewer.Height);
        }
        catch (System.AccessViolationException ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Texture preview unavailable: {ex.Message}"); /* empty means unavailable */ }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Texture preview unavailable: {ex.Message}"); /* empty means unavailable */ }
    }

    /// <summary>Prefer a loose preview, then resolve an equivalent read-only texture from FC26.</summary>
    protected void ShowFc26Asset(PictureBox viewer, string? localPath, params string[] queries)
    {
        FrostbitePreviewLoader.Load(viewer, Services, localPath, queries, (image, _) =>
        {
            if (IsDisposed) { image?.Dispose(); return; }
            viewer.Image?.Dispose();
            viewer.Image = image;
        });
    }

    /// <summary>Values of the current FC26 record, for fixed-form visual adapters.</summary>
    protected IReadOnlyDictionary<string, FieldValue> CurrentValues => _values;

    /// <summary>Raw string value of a field on the current record (empty when absent).</summary>
    protected string Value(string field) =>
        CurrentValues.TryGetValue(field, out var value) ? value.RawValue : string.Empty;

    protected override void ShowRecord(int recordIndex)
    {
        _values.Clear();
        var allValues = Services.RequireData().GetFields(TableName, recordIndex, _labels);
        foreach (var v in allValues) _values[v.FieldName] = v;
        foreach (var box in _editors)
        {
            var name = box.Tag as string ?? "";
            if (_values.TryGetValue(name, out var value))
            {
                box.Text = value.Value;
                box.ReadOnly = !value.IsWritable;
                box.BackColor = value.IsWritable ? Theme.Input : CardLayout.CardFieldBg;
                box.ForeColor = CardLayout.CardText;
                ToolTip.SetToolTip(box, value.IsWritable ? value.FieldName : value.FieldName + " (read-only)");
            }
            else
            {
                box.Text = "";
                box.ReadOnly = true;
                box.BackColor = CardLayout.CardFieldBg;
                box.ForeColor = CardLayout.CardSubtle;
                ToolTip.SetToolTip(box, name + " is not present in this database");
            }
        }
        foreach (var box in _mirrors)
        {
            var name = box.Tag as string ?? "";
            if (_values.TryGetValue(name, out var value))
            {
                box.Text = value.Value;
                box.ReadOnly = true;
                box.BackColor = CardLayout.CardFieldBg;
                box.ForeColor = CardLayout.CardText;
                ToolTip.SetToolTip(box, $"Read-only mirror of {value.FieldName} — edited in its section above.");
            }
            else
            {
                box.Text = "";
                box.ReadOnly = true;
                box.BackColor = CardLayout.CardFieldBg;
                box.ForeColor = CardLayout.CardSubtle;
                ToolTip.SetToolTip(box, name + " is not present in this database");
            }
        }
        OnRecordShown();
    }

    protected virtual void OnRecordShown() { }

    protected void Commit(TextBox box)
    {
        var name = box.Tag as string ?? "";
        if (CurrentRecordIndex >= 0 && !box.ReadOnly && _values.TryGetValue(name, out var old) && box.Text.Trim() != old.Value)
            StageField(TableName, CurrentRecordIndex, name, box.Text.Trim(), _staging);
    }
}

public sealed class ManagersSection : ClassicEntitySection
{
    private readonly PictureBox _face;
    public ManagersSection(AppServices s) : base(s, "managers", "Managers", "manager", () => s.RequireData().GetManagers(), LabelMaps.Managers)
    {
        var general = AddCanvasTab("General"); var c = Canvas(general);
        var identity = Group("Identity", new Point(4, 3), new Size(750, 222));
        _face = ImageSurface(identity, new Point(12, 20), new Size(128, 128), "Manager face");
        LegacyAssetActions.Attach(Services, identity, _face, new Point(12, 178), () => OnRecordShown());
        AddField(identity, "managerid", "Manager Id", new Point(254, 20), 120);
        AddField(identity, "firstname", "First Name", new Point(254, 46), 130);
        AddField(identity, "surname", "Last Name", new Point(254, 72), 130);
        AddField(identity, "commonname", "Common Name", new Point(254, 98), 130);
        AddField(identity, "nationality", "Country", new Point(254, 124), 130);
        AddField(identity, "birthdate", "Birthdate", new Point(254, 150), 130);
        // Keep the team relationship below the portrait action buttons. The
        // previous y=190 placed its caption over the Import/Remove/Export row.
        AddField(identity, "teamid", "Playing for", new Point(560, 176), 130);
        c.Controls.Add(identity);

        var body = Group("Body and Look", new Point(4, 231), new Size(750, 150));
        AddField(body, "height", "Height", new Point(100, 20), 100);
        AddField(body, "weight", "Weight", new Point(300, 20), 100);
        AddField(body, "starrating", "Star Rating", new Point(100, 46), 100);
        AddField(body, "managerjointeamdate", "Joining Date", new Point(300, 46), 100);
        AddReadonlyNote(body, "Manager portrait preview requires a mapped visual asset.", new Point(12, 92), new Size(480, 45));
        c.Controls.Add(body);
        var recordLinks = Group("Record Links", new Point(760, 3), new Size(750, 128));
        AddReadonlyField(recordLinks, "managerid", "Manager Id", new Point(130, 22), 210);
        AddReadonlyField(recordLinks, "teamid", "Team Id", new Point(130, 48), 210);
        AddReadonlyNote(recordLinks, "Technical identifiers are kept with the General\nrecord for the CM16 workflow.", new Point(12, 78), new Size(360, 34));
        c.Controls.Add(recordLinks);

        // These are real FC26 manager columns that have no CM16 equivalent page.
        // Keep them as fixed legacy-style groups rather than exposing a raw schema grid.
        var face = AddCanvasTab("Face"); var fc = Canvas(face);
        var model = Group("Face Modelling", new Point(4, 4), new Size(750, 250));
        AddField(model, "headassetid", "Head Asset", new Point(160, 22), 150);
        AddField(model, "headclasscode", "Head Class", new Point(160, 48), 150);
        AddField(model, "headtypecode", "Head Type", new Point(160, 74), 150);
        AddField(model, "headvariation", "Head Variation", new Point(160, 100), 150);
        AddField(model, "faceposerpreset", "Face Preset", new Point(160, 126), 150);
        AddField(model, "facepsdlayer0", "Face Layer 0", new Point(160, 152), 150);
        AddField(model, "facepsdlayer1", "Face Layer 1", new Point(160, 178), 150);
        AddField(model, "hashighqualityhead", "High Quality Head", new Point(160, 204), 150);
        fc.Controls.Add(model);
        var hair = Group("Hair and Appearance", new Point(760, 4), new Size(750, 250));
        AddField(hair, "hairstylecode", "Hair Style", new Point(170, 22), 150);
        AddField(hair, "hairtypecode", "Hair Type", new Point(170, 48), 150);
        AddField(hair, "haircolorcode", "Hair Color", new Point(170, 74), 150);
        AddField(hair, "facialhairtypecode", "Facial Hair", new Point(170, 100), 150);
        AddField(hair, "facialhaircolorcode", "Facial Hair Color", new Point(170, 126), 150);
        AddField(hair, "eyecolorcode", "Eye Color", new Point(170, 152), 150);
        AddField(hair, "skintonecode", "Skin Tone", new Point(170, 178), 150);
        AddField(hair, "bodytypecode", "Body Type", new Point(170, 204), 150);
        fc.Controls.Add(hair);
        var skin = Group("Skin and Details", new Point(4, 260), new Size(750, 250));
        AddField(skin, "skintypecode", "Skin Type", new Point(170, 22), 150);
        AddField(skin, "skincomplexion", "Complexion", new Point(170, 48), 150);
        AddField(skin, "skinmakeup", "Skin Makeup", new Point(170, 74), 150);
        AddField(skin, "skinsurfacepack", "Surface Pack", new Point(170, 100), 150);
        AddField(skin, "eyebrowcode", "Eyebrows", new Point(170, 126), 150);
        AddField(skin, "sideburnscode", "Sideburns", new Point(170, 152), 150);
        AddField(skin, "lipcolor", "Lip Color", new Point(170, 178), 150);
        AddField(skin, "eyedetail", "Eye Detail", new Point(170, 204), 150);
        fc.Controls.Add(skin);
        var record = Group("Record Details", new Point(760, 260), new Size(750, 224));
        AddField(record, "accessorycolourcode3", "Accessory Colour 3", new Point(170, 22), 150);
        AddField(record, "accessorycolourcode4", "Accessory Colour 4", new Point(170, 48), 150);
        AddField(record, "personalityid", "Personality", new Point(170, 74), 150);
        AddField(record, "gender", "Gender", new Point(170, 100), 150);
        AddField(record, "ethnicity", "Ethnicity", new Point(170, 126), 150);
        AddField(record, "islicensed", "Licensed", new Point(170, 152), 150);
        AddField(record, "isrewardable", "Rewardable", new Point(170, 178), 150);
        fc.Controls.Add(record);

        var appearance = AddCanvasTab("Appearance"); var ac = Canvas(appearance);
        var outfit = Group("Outfit and Accessories", new Point(4, 4), new Size(1510, 250));
        AddField(outfit, "outfitid", "Outfit", new Point(180, 22), 160);
        AddField(outfit, "seasonaloutfitid", "Seasonal Outfit", new Point(180, 48), 160);
        AddField(outfit, "accessorycode1", "Accessory 1", new Point(180, 74), 160);
        AddField(outfit, "accessorycode2", "Accessory 2", new Point(180, 100), 160);
        AddField(outfit, "accessorycode3", "Accessory 3", new Point(180, 126), 160);
        AddField(outfit, "accessorycode4", "Accessory 4", new Point(180, 152), 160);
        AddField(outfit, "accessorycolourcode1", "Accessory Colour 1", new Point(180, 178), 160);
        AddField(outfit, "accessorycolourcode2", "Accessory Colour 2", new Point(180, 204), 160);
        ac.Controls.Add(outfit);
        var traits = Group("Trait Values", new Point(630, 4), new Size(360, 161));
        AddField(traits, "trait1vweak", "Weak Flags", new Point(145, 22), 150);
        AddField(traits, "trait1vstrong", "Strong Flags", new Point(145, 48), 150);
        AddField(traits, "trait1vequal", "Equal Flags", new Point(145, 74), 150);
        AddReadonlyNote(traits, "These advanced values are stored as database bitmasks.", new Point(12, 105), new Size(332, 30));
        ac.Controls.Add(traits);
    }

    protected override void OnRecordShown()
    {
        var id = CurrentValues.TryGetValue("managerid", out var value) ? Parse(value.RawValue) : 0;
        var headAssetId = CurrentValues.TryGetValue("headassetid", out var head) ? Parse(head.RawValue) : 0;
        var localPath = Services.Assets.GetManagerFace(headAssetId > 0 ? headAssetId : id);
        // Staff portraits are a dedicated FC26 legacy UI family.  A manager
        // record ID is not the portrait ID; headassetid is the correct link.
        var portraitId = headAssetId > 0 ? headAssetId : id;
        var legacyPath = $"data/ui/imgAssets/heads_staff/heads_staff_{portraitId}.dds";
        LegacyAssetActions.SetTarget(_face, new LegacyAssetEditTarget(legacyPath, 256, 256));
        FrostbitePreviewLoader.LoadLegacyUiAsset(_face, Services,
            LegacyAssetActions.Replacement(Services, legacyPath) ?? localPath,
            legacyPath, (image, _) =>
        {
            if (IsDisposed) { image?.Dispose(); return; }
            _face.Image?.Dispose();
            _face.Image = image;
        });
    }
}

public sealed class StadiumsSection : ClassicEntitySection
{
    private readonly PictureBox _preview;
    private readonly PictureBox _generalPreview;
    private readonly PictureBox _tifoPreview;
    public StadiumsSection(AppServices s) : base(s, "stadiums", "Stadiums", "stadiums", () => s.RequireData().GetStadiums(), LabelMaps.Stadiums)
    {
        var general = AddCanvasTab("General"); var c = Canvas(general);
        var info = Group("Info", new Point(8, 3), new Size(400, 522));
        AddField(info, "name", "Database Name", new Point(120, 18), 136);
        AddField(info, "stadiumid", "Stadium Id", new Point(120, 44), 136);
        AddField(info, "capacity", "Capacity", new Point(120, 70), 136);
        AddField(info, "countrycode", "Country", new Point(120, 96), 136);
        AddField(info, "hometeamid", "Home Team", new Point(120, 122), 136);
        AddField(info, "cityid", "City", new Point(120, 148), 136);
        AddField(info, "stadiumtype", "Stadium Type", new Point(120, 174), 136);
        AddField(info, "islicensed", "Licensed", new Point(120, 200), 136);
        AddField(info, "dlc", "DLC", new Point(120, 226), 136);
        AddField(info, "isstadiumavailable", "Available", new Point(120, 252), 136);
        AddField(info, "ismirrored", "Mirrored", new Point(120, 278), 136);
        AddField(info, "iseditable", "Editable", new Point(120, 304), 136);
        AddField(info, "isdynamic", "Dynamic", new Point(120, 330), 136);
        AddField(info, "upgradetier", "Upgrade Tier", new Point(120, 356), 136);
        AddField(info, "upgradestyle", "Upgrade Style", new Point(120, 382), 136);
        AddField(info, "gmavailability", "GM Availability", new Point(120, 408), 136);
        AddField(info, "genericrank", "Generic Rank", new Point(120, 434), 136);
        AddField(info, "frontsettype", "Front Set Type", new Point(120, 460), 136);
        AddField(info, "seatcolor", "Seat Colour", new Point(120, 486), 136);
        c.Controls.Add(info);
        var pitchInfo = Group("Pitch", new Point(8, 530), new Size(400, 240));
        AddField(pitchInfo, "stadiumpitchlength", "Pitch Length", new Point(120, 20), 136);
        AddField(pitchInfo, "stadiumpitchwidth", "Pitch Width", new Point(120, 46), 136);
        AddField(pitchInfo, "playsurfacetype", "Surface", new Point(120, 72), 136);
        AddField(pitchInfo, "pitchcolor", "Pitch Color", new Point(120, 98), 136);
        AddField(pitchInfo, "pitchwear", "Pitch Wear", new Point(120, 124), 136);
        AddField(pitchInfo, "pitchlinecolor", "Line Color", new Point(120, 150), 136);
        AddField(pitchInfo, "stadiummowpattern_code", "Mowing Pattern", new Point(120, 176), 136);
        AddField(pitchInfo, "pitchtarps", "Pitch Tarps", new Point(120, 202), 136);
        c.Controls.Add(pitchInfo);
        // Keep the useful stadium image prominent on the General tab rather
        // than leaving it as a tiny thumbnail beside a mostly empty canvas.
        var pattern = Group("Stadium Preview", new Point(414, 3), new Size(1100, 438));
        _generalPreview = ImageSurface(pattern, new Point(10, 22), new Size(624, 390), "Stadium preview");
        c.Controls.Add(pattern);
        var preview = AddCanvasTab("Preview"); var pc = Canvas(preview);
        var p = Group("Preview", new Point(8, 4), new Size(1510, 680));
        _preview = ImageSurface(p, new Point(10, 22), new Size(1024, 512), "Stadium preview");
        pc.Controls.Add(p);
        var technical = AddCanvasTab("Model"); var mc = Canvas(technical);
        var model = Group("Stadium Model", new Point(8, 4), new Size(1510, 680));
ThreeDViewerLauncher.AttachPlaceholder(model, new Point(10, 22), new Size(1024, 512), "stadium",
            () => new[] { Value("stadiumid"), "stadium_" + Value("stadiumid") },
            () => Services.FrostbiteAssets.ExportMeshForQuery(new[] { "stadium_" + Value("stadiumid") }));
        mc.Controls.Add(model);

        var environment = AddCanvasTab("Environment"); var ec = Canvas(environment);
        var weather = Group("Weather and Match Setup", new Point(8, 4), new Size(410, 224));
        AddField(weather, "defaultweather", "Default Weather", new Point(180, 22), 150);
        AddField(weather, "defaultseason", "Default Season", new Point(180, 48), 150);
        AddField(weather, "defaulttime", "Default Time", new Point(180, 74), 150);
        AddField(weather, "tod1weather", "TOD 1 Weather", new Point(180, 100), 150);
        AddField(weather, "tod2weather", "TOD 2 Weather", new Point(180, 126), 150);
        AddField(weather, "tod3weather", "TOD 3 Weather", new Point(180, 152), 150);
        AddField(weather, "tod4weather", "TOD 4 Weather", new Point(180, 178), 150);
        ec.Controls.Add(weather);
        var presentation = Group("Presentation", new Point(424, 4), new Size(410, 224));
        AddField(presentation, "adboardtype", "Adboard Type", new Point(180, 22), 150);
        AddField(presentation, "hascenteradboard", "Centre Adboard", new Point(180, 48), 150);
        AddField(presentation, "goalnetads", "Goal Net Ads", new Point(180, 74), 150);
        AddField(presentation, "tifo", "Tifo", new Point(180, 100), 150);
        AddField(presentation, "inflatables", "Inflatables", new Point(180, 126), 150);
        AddField(presentation, "hasintrovideo", "Intro Video", new Point(180, 152), 150);
        AddField(presentation, "hasdroneshots", "Drone Shots", new Point(180, 178), 150);
        ec.Controls.Add(presentation);
        var tifo = Group("Tifo Preview", new Point(840, 4), new Size(520, 360));
        _tifoPreview = ImageSurface(tifo, new Point(10, 22), new Size(496, 312), "Club tifo");
        ec.Controls.Add(tifo);

        var broadcast = Group("Broadcast and Camera", new Point(8, 234), new Size(410, 302));
        AddField(broadcast, "cameraheight", "Camera Height", new Point(180, 22), 150);
        AddField(broadcast, "camerazoom", "Camera Zoom", new Point(180, 48), 150);
        AddField(broadcast, "dronetype", "Drone Type", new Point(180, 74), 150);
        AddField(broadcast, "replay360degree", "Replay 360°", new Point(180, 100), 150);
        AddField(broadcast, "rotation360degree", "Rotation 360°", new Point(180, 126), 150);
        AddField(broadcast, "hasmexicanwave", "Mexican Wave", new Point(180, 152), 150);
        AddField(broadcast, "policetypecode", "Police Type", new Point(180, 178), 150);
        AddField(broadcast, "sectionfacedbydefault", "Default Section", new Point(180, 204), 150);
        AddField(broadcast, "timeofday1", "Time of Day 1", new Point(180, 230), 150);
        AddField(broadcast, "timeofday2", "Time of Day 2", new Point(180, 256), 150);
        ec.Controls.Add(broadcast);
        var licensing = Group("Licensing", new Point(424, 234), new Size(410, 302));
        AddField(licensing, "hasunofficialname", "Unofficial Name", new Point(180, 22), 150);
        AddField(licensing, "supportsunlicensedbanners", "Unlicensed Banners", new Point(180, 48), 150);
        AddField(licensing, "supportsunlicensedtifos", "Unlicensed Tifos", new Point(180, 74), 150);
        AddField(licensing, "enhancedcrowdcardsupport", "Enhanced Crowd Cards", new Point(180, 100), 150);
        AddField(licensing, "issmallsided", "Small Sided", new Point(180, 126), 150);
        AddField(licensing, "adboardsidelinedistance", "Adboard Sideline Distance", new Point(180, 152), 150);
        AddField(licensing, "adboardendlinedistance", "Adboard Endline Distance", new Point(180, 178), 150);
        AddField(licensing, "presassetone", "Pres Asset One", new Point(180, 204), 150);
        AddField(licensing, "presassetthree", "Pres Asset Three", new Point(180, 230), 150);
        AddField(licensing, "presassetfour", "Pres Asset Four", new Point(180, 256), 150);
        ec.Controls.Add(licensing);
        var presentation2 = Group("Presentation Extras", new Point(840, 370), new Size(520, 320));
        AddField(presentation2, "colorregion", "Colour Region", new Point(180, 22), 150);
        AddField(presentation2, "languageregion", "Language Region", new Point(180, 48), 150);
        AddField(presentation2, "tunnellocation", "Tunnel Location", new Point(180, 74), 150);
        AddField(presentation2, "timeofday3", "Time of Day 3", new Point(180, 100), 150);
        AddField(presentation2, "timeofday4", "Time of Day 4", new Point(180, 126), 150);
        ec.Controls.Add(presentation2);

        var goals = AddCanvasTab("Goal Net"); var gc = Canvas(goals);
        var net = Group("Goal Net and Posts", new Point(8, 4), new Size(460, 250));
        AddField(net, "stadiumgoalnetstyle", "Net Style", new Point(190, 22), 160);
        AddField(net, "stadiumgoalnettype", "Net Type", new Point(190, 48), 160);
        AddField(net, "stadiumgoalnetpattern", "Net Pattern", new Point(190, 74), 160);
        AddField(net, "stadiumgoalnettension", "Net Tension", new Point(190, 100), 160);
        AddField(net, "stadiumgoalpoststyle", "Post Style", new Point(190, 126), 160);
        AddField(net, "goalnetstanchiontype", "Stanchion Type", new Point(190, 152), 160);
        AddField(net, "stadiumgloalnetdepth", "Goal Net Depth", new Point(190, 178), 160);
        AddField(net, "supportsgoalnetshape", "Supports Net Shape", new Point(190, 204), 160);
        gc.Controls.Add(net);
        var netColours = Group("Stanchion Colours", new Point(474, 4), new Size(460, 250));
        AddField(netColours, "goalnetstanchioncolor1r", "Stanchion 1 R", new Point(190, 22), 160);
        AddField(netColours, "goalnetstanchioncolor1g", "Stanchion 1 G", new Point(190, 48), 160);
        AddField(netColours, "goalnetstanchioncolor1b", "Stanchion 1 B", new Point(190, 74), 160);
        AddField(netColours, "goalnetstanchioncolor2r", "Stanchion 2 R", new Point(190, 100), 160);
        AddField(netColours, "goalnetstanchioncolor2g", "Stanchion 2 G", new Point(190, 126), 160);
        AddField(netColours, "goalnetstanchioncolor2b", "Stanchion 2 B", new Point(190, 152), 160);
        gc.Controls.Add(netColours);
        var techZone = Group("Technical Zone", new Point(8, 260), new Size(926, 94));
        AddField(techZone, "stadhometechzonemaxx", "Home Max X", new Point(140, 22), 120);
        AddField(techZone, "stadhometechzoneminx", "Home Min X", new Point(340, 22), 120);
        AddField(techZone, "stadhometechzonemaxz", "Home Max Z", new Point(540, 22), 120);
        AddField(techZone, "stadhometechzoneminz", "Home Min Z", new Point(740, 22), 120);
        AddField(techZone, "stadawaytechzonemaxx", "Away Max X", new Point(140, 48), 120);
        AddField(techZone, "stadawaytechzoneminx", "Away Min X", new Point(340, 48), 120);
        AddField(techZone, "stadawaytechzonemaxz", "Away Max Z", new Point(540, 48), 120);
        AddField(techZone, "stadawaytechzoneminz", "Away Min Z", new Point(740, 48), 120);
        gc.Controls.Add(techZone);
    }

    protected override void OnRecordShown()
    {
        var id = CurrentValues.TryGetValue("stadiumid", out var value) ? Parse(value.RawValue) : 0;
        var legacyPaths = new[]
        {
            $"data/ui/imgAssets/stadium/stadium_{id}_0.dds",
            $"data/ui/external/ion_fut/imgAssets/stadiums/stadium_{id}.dds",
            $"data/ui/external/ion_fut/imgAssets/cards/stadium/stadium_{id}.dds",
            $"data/ui/imgAssets/clubInfo/stadium/st_{id}.dds"
        };
        LoadStadiumPreview(_generalPreview, id, legacyPaths);
        LoadStadiumPreview(_preview, id, legacyPaths);

        var tifoId = CurrentValues.TryGetValue("tifo", out var tifoValue) ? Parse(tifoValue.RawValue) : 0;
        if (tifoId <= 0)
        {
            _tifoPreview.Image?.Dispose();
            _tifoPreview.Image = null;
            return;
        }

        FrostbitePreviewLoader.LoadLegacyUiAsset(
            _tifoPreview,
            Services,
            null,
            $"data/ui/imgAssets/clubtifo/tifo_{tifoId}.dds",
            (image, _) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                _tifoPreview.Image?.Dispose();
                _tifoPreview.Image = image;
            });
    }

    private void LoadStadiumPreview(PictureBox viewer, int stadiumId, IEnumerable<string> legacyPaths) =>
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(
            viewer,
            Services,
            Services.Assets.GetStadium(stadiumId),
            legacyPaths,
            (image, _) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                viewer.Image?.Dispose();
                viewer.Image = image;
            });
}

public sealed class RefereesSection : ClassicEntitySection
{
    public RefereesSection(AppServices s) : base(s, "referees", "Referees", "referee", () => s.RequireData().GetReferees(), LabelMaps.Referees)
    {
        var general = AddCanvasTab("General"); var c = Canvas(general);
        var identity = Group("Identity", new Point(3, 3), new Size(512, 272));
        AddField(identity, "refereeid", "Referee Id", new Point(98, 18), 130);
        AddField(identity, "firstname", "First Name", new Point(98, 44), 130);
        AddField(identity, "surname", "Last Name", new Point(98, 70), 130);
        AddField(identity, "birthdate", "Birthdate", new Point(98, 96), 130);
        AddField(identity, "nationalitycode", "Country", new Point(98, 122), 130);
        AddField(identity, "height", "Height", new Point(98, 148), 130);
        AddField(identity, "weight", "Weight", new Point(98, 174), 130);
        AddField(identity, "gender", "Gender", new Point(98, 200), 130);
        AddField(identity, "isreal", "Real Referee", new Point(98, 226), 130);
        ThreeDViewerLauncher.AttachPlaceholder(identity, new Point(270, 18), new Size(200, 200), "referee",
            () => new[] { Value("refereeid"), Value("firstname"), Value("surname"), Value("firstname") + "_" + Value("surname") },
            () => Services.FrostbiteAssets.ExportMeshForQuery(new[] { "referee_" + Value("refereeid") }));
        c.Controls.Add(identity);
        var officiating = AddCanvasTab("Officiating"); var oc = Canvas(officiating);
        var box = Group("Officiating", new Point(3, 3), new Size(512, 118));
        AddField(box, "leagueid", "League", new Point(154, 20), 190);
        AddField(box, "cardstrictness", "Cards Style", new Point(154, 46), 190);
        AddField(box, "foulstrictness", "Fouls Style", new Point(154, 72), 190);
        oc.Controls.Add(box);

        var face = AddCanvasTab("Face"); var fc = Canvas(face);
        var modelling = Group("Face Modelling", new Point(3, 3), new Size(620, 250));
        AddField(modelling, "headclasscode", "Head Class", new Point(180, 22), 150);
        AddField(modelling, "headtypecode", "Head Type", new Point(180, 48), 150);
        AddField(modelling, "headvariation", "Head Variation", new Point(180, 74), 150);
        AddField(modelling, "headassetid", "Head Asset", new Point(180, 100), 150);
        AddField(modelling, "faceposerpreset", "Face Preset", new Point(180, 126), 150);
        AddField(modelling, "facepsdlayer0", "Face Layer 0", new Point(180, 152), 150);
        AddField(modelling, "facepsdlayer1", "Face Layer 1", new Point(180, 178), 150);
        AddField(modelling, "skinsurfacepack", "Surface Pack", new Point(180, 204), 150);
        fc.Controls.Add(modelling);
        var hair = Group("Hair and Appearance", new Point(629, 3), new Size(620, 302));
        AddField(hair, "hairstylecode", "Hair Style", new Point(180, 22), 150);
        AddField(hair, "hairtypecode", "Hair Type", new Point(180, 48), 150);
        AddField(hair, "haircolorcode", "Hair Color", new Point(180, 74), 150);
        AddField(hair, "facialhairtypecode", "Facial Hair", new Point(180, 100), 150);
        AddField(hair, "facialhaircolorcode", "Facial Hair Color", new Point(180, 126), 150);
        AddField(hair, "eyecolorcode", "Eye Color", new Point(180, 152), 150);
        AddField(hair, "eyebrowcode", "Eyebrows", new Point(180, 178), 150);
        AddField(hair, "sideburnscode", "Sideburns", new Point(180, 204), 150);
        AddField(hair, "lipcolor", "Lip Color", new Point(180, 230), 150);
        AddField(hair, "eyedetail", "Eye Detail", new Point(180, 256), 150);
        fc.Controls.Add(hair);
        var skin = Group("Skin", new Point(3, 311), new Size(1246, 146));
        AddField(skin, "skintonecode", "Skin Tone", new Point(180, 22), 150);
        AddField(skin, "skintypecode", "Skin Type", new Point(180, 48), 150);
        AddField(skin, "skinmakeup", "Skin Makeup", new Point(180, 74), 150);
        AddField(skin, "skincomplexion", "Complexion", new Point(180, 100), 150);
        AddField(skin, "bodytypecode", "Body Type", new Point(430, 22), 150);
        AddField(skin, "jerseysleevelengthcode", "Sleeves Length", new Point(430, 48), 150);
        AddField(skin, "socklengthcode", "Socks Length", new Point(430, 74), 150);
        AddField(skin, "sockstylecode", "Socks Style", new Point(430, 100), 150);
        AddField(skin, "shoetypecode", "Shoe Type", new Point(680, 22), 150);
        AddField(skin, "shoedesigncode", "Shoe Design", new Point(680, 48), 150);
        AddField(skin, "shoecolorcode1", "Shoe Color 1", new Point(680, 74), 150);
        AddField(skin, "shoecolorcode2", "Shoe Color 2", new Point(680, 100), 150);
        AddField(skin, "shortstyle", "Shorts Style", new Point(930, 22), 150);
        AddField(skin, "smallsidedshoetypecode", "Small Sided Shoes", new Point(930, 48), 150);
        fc.Controls.Add(skin);
    }
}

public sealed class FormationsSection : ClassicEntitySection
{
    private readonly Panel _pitch;
    private readonly Label _pitchStatus;
    private readonly Panel _pitchGroup;
    private readonly Dictionary<string, ComboBox> _formationPickers = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _pickerStaging = new();
    private bool _syncFormationPickers;

    public FormationsSection(AppServices s) : base(s, "formations", "Formations", "formations", () => s.RequireData().GetFormations(), LabelMaps.Formations)
    {
        var general = AddCanvasTab("Position"); var c = Canvas(general);
        // Keep the tactical board and its editors in a real split workspace.
        // Fixed X/Y placement made the role map disappear or overlap the pitch
        // when the window was restored on a smaller display.
        c.AutoScroll = false;
        var workspace = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            FixedPanel = FixedPanel.None,
            SplitterWidth = 6,
            BackColor = CardLayout.CardBackground,
        };
        workspace.Panel1.Padding = new Padding(3);
        workspace.Panel2.Padding = new Padding(3);
        workspace.Panel1.BackColor = CardLayout.CardBackground;
        workspace.Panel2.BackColor = CardLayout.CardBackground;
        workspace.SizeChanged += (_, _) =>
        {
            // Setting either MinSize first makes WinForms immediately validate
            // the old splitter distance. At construction/restoration that
            // distance can still be zero, producing the fatal
            // "SplitterDistance must be between..." exception seen in live UI.
            // Establish a safe distance first, then apply both constraints.
            var available = workspace.Width - workspace.SplitterWidth;
            if (available < 956) return;
            var target = Math.Clamp((int)(workspace.Width * 0.43), 360, workspace.Width - 590 - workspace.SplitterWidth);
            try
            {
                workspace.SplitterDistance = target;
                workspace.Panel1MinSize = 360;
                workspace.Panel2MinSize = 590;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CM26] Stadium workspace splitter transient: {ex.Message}");
                // A transient zero-size layout pass can race resize. The next
                // SizeChanged event will apply the same safe configuration.
            }
        };
        c.Controls.Add(workspace);

        _pitchGroup = Group("Formation Preview", Point.Empty, new Size(520, 490));
        _pitchGroup.Dock = DockStyle.Fill;
        _pitch = new RatableBoard { DrawBasePitch = false, Location = new Point(8, 20), Size = new Size(558, 430), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, BackColor = Color.FromArgb(43, 132, 82), BorderStyle = BorderStyle.FixedSingle };
        _pitch.Paint += (_, e) =>
        {
            // GDI+ drawing runs inside the WinForms message pump. A native fault
            // here (which .NET's ThreadException handler cannot intercept) would
            // surface as the Windows "Exception Processing Message 0xc0000005"
            // dialog, so paint defensively and never let it escape the WndProc.
            try { if (e.Graphics != null) DrawFormationPitch(null, e); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Pitch redraw failed: {ex.Message}"); /* A pitch redraw fault must never take down the message loop. */ }
        };
        _pitchGroup.Controls.Add(_pitch);
        _pitchStatus = new Label { Location = new Point(12, 455), Size = new Size(550, 28), Font = LegacyFont, ForeColor = Theme.Muted, BackColor = Theme.Panel, AutoEllipsis = true };
        _pitchStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _pitchGroup.Controls.Add(_pitchStatus);
        _pitchGroup.SizeChanged += (_, _) => ResizePitchPreview();
        workspace.Panel1.Controls.Add(_pitchGroup);

        var info = Group("Formation", Point.Empty, new Size(600, 302));
        info.Dock = DockStyle.Top;
        AddField(info, "formationid", "Formation Id", new Point(150, 22), 160);
        AddField(info, "formationname", "Database Name", new Point(150, 48), 160);
        AddField(info, "formationfullnameid", "Full Name Id", new Point(150, 74), 160);
        AddField(info, "formationaudioid", "Audio Id", new Point(150, 100), 160);
        AddField(info, "teamid", "Team Id", new Point(150, 126), 160);
        AddField(info, "relativeformationid", "Base Formation", new Point(150, 152), 160);
        AddField(info, "attackers", "Attackers", new Point(150, 178), 160);
        AddField(info, "midfielders", "Midfielders", new Point(150, 204), 160);
        AddField(info, "defenders", "Defenders", new Point(150, 230), 160);
        AddField(info, "offensiverating", "Offensive Rating", new Point(150, 256), 160);
        workspace.Panel2.Controls.Add(info);

        var roles = Group("Position Map", Point.Empty, new Size(720, 278));
        roles.Dock = DockStyle.Fill;
        roles.AutoScroll = true;
        for (var i = 0; i < 11; i++)
        {
            var column = i / 6;
            var row = i % 6;
            var x = 92 + (column * 340);
            var y = 22 + (row * 42);
            // Hand-placed captions: AddField's auto-caption would collide with
            // the adjacent slot editor at this narrow column pitch.
            roles.Controls.Add(new Label
            {
                Text = $"Slot {i + 1}",
                Location = new Point(column == 0 ? 10 : 350, y + 3),
                Size = new Size(74, 18),
                Font = LegacyFont,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Theme.Panel,
                ForeColor = Theme.Text,
            });
            AddFormationDropdown(roles, $"position{i}", new Point(x, y), 78, PositionOptions());
            roles.Controls.Add(new Label
            {
                Text = "Role",
                Location = new Point(x + 88, y + 3),
                Size = new Size(58, 18),
                Font = LegacyFont,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Theme.Panel,
                ForeColor = Theme.Text,
            });
            AddFormationDropdown(roles, $"pos{i}role", new Point(x + 154, y), 78, RoleOptions());
            var clearSlot = new Button
            {
                Text = "×",
                Location = new Point(x + 240, y - 1),
                Size = new Size(28, 23),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.BodyBold,
                AccessibleName = $"Clear formation slot {i + 1}",
            };
            Theme.ApplyButton(clearSlot);
            var slot = i;
            clearSlot.Click += (_, _) => ClearFormationSlot(slot);
            roles.Controls.Add(clearSlot);
        }
        workspace.Panel2.Controls.Add(roles);

        var layout = AddCanvasTab("Layout"); var lc = Canvas(layout);
        lc.AutoScroll = true;
        lc.AutoScrollMinSize = new Size(920, 300);
        var offsets = Group("Position Coordinates", new Point(3, 3), new Size(900, 278));
        for (var i = 0; i < 11; i++)
        {
            var column = i / 6;
            var row = i % 6;
            var x = 22 + (column * 430);
            var y = 22 + (row * 42);
            offsets.Controls.Add(new Label
            {
                Text = $"Slot {i + 1}", Location = new Point(x, y + 3), Size = new Size(56, 18),
                Font = LegacyFont, ForeColor = CardLayout.CardFieldLabel,
                BackColor = CardLayout.CardWhite, TextAlign = ContentAlignment.MiddleRight,
            });
            offsets.Controls.Add(new Label
            {
                Text = "X", Location = new Point(x + 66, y + 3), Size = new Size(18, 18),
                Font = LegacyFont, ForeColor = CardLayout.CardFieldLabel,
                BackColor = CardLayout.CardWhite, TextAlign = ContentAlignment.MiddleRight,
            });
            AddSlotEditor(offsets, $"offset{i}x", new Point(x + 90, y), 92);
            offsets.Controls.Add(new Label
            {
                Text = "Y", Location = new Point(x + 194, y + 3), Size = new Size(18, 18),
                Font = LegacyFont, ForeColor = CardLayout.CardFieldLabel,
                BackColor = CardLayout.CardWhite, TextAlign = ContentAlignment.MiddleRight,
            });
            AddSlotEditor(offsets, $"offset{i}y", new Point(x + 218, y), 92);
        }
        AddReadonlyNote(offsets, "Coordinates control the marker positions shown in the Formation Preview.", new Point(16, 256), new Size(760, 22));
        lc.Controls.Add(offsets);
    }

    protected override void OnRecordShown()
    {
        var name = Value("formationname");
        var validSlots = Enumerable.Range(0, 11).Count(i =>
            TryCoordinate(Value($"offset{i}x"), out _) && TryCoordinate(Value($"offset{i}y"), out _));
        _pitchStatus.Text = string.IsNullOrWhiteSpace(name)
            ? "Select a formation."
            : $"{name} · {validSlots}/11 positions mapped";
        RefreshFormationPickers();
        ResizePitchPreview();
        _pitch.Invalidate();
    }

    /// <summary>Editor with the same commit wiring as AddField, minus the caption.</summary>
    private void AddSlotEditor(Control parent, string field, Point point, int width)
    {
        var box = new TextBox { Location = point, Size = new Size(width, 20), Tag = field, Font = LegacyFont, BorderStyle = BorderStyle.FixedSingle };
        Theme.ApplyTextBox(box);
        box.Leave += (_, _) => Commit(box);
        parent.Controls.Add(box);
        _editors.Add(box);
    }

    private sealed record FormationOption(string Display, string Value)
    {
        public override string ToString() => Display;
    }

    private static IReadOnlyList<FormationOption> PositionOptions() =>
        new[] { new FormationOption("Not set", "-1") }
            .Concat(Enumerable.Range(0, 28).Select(code =>
                new FormationOption(NameResolverService.PositionLabel(code), code.ToString())))
            .ToArray();

    private IReadOnlyList<FormationOption> RoleOptions()
    {
        var values = new HashSet<int> { 0 };
        try
        {
            var table = Services.Session.GetTable("formations");
            foreach (var name in Enumerable.Range(0, 11).Select(i => $"pos{i}role"))
            {
                if (table?.FindColumn(name) == null) continue;
                for (var row = 0; row < table.RowCount; row++)
                    if (int.TryParse(Services.Session.GetCell("formations", row, name), out var parsedRole)) values.Add(parsedRole);
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Formation role enumeration failed: {ex.Message}"); /* Keep the safe zero option when a partial schema is loaded. */ }
        return new[] { new FormationOption("Not set", "0") }
            .Concat(values.Where(role => role > 0).OrderBy(role => role)
                .Select(role => new FormationOption($"Role {role}", role.ToString())))
            .ToArray();
    }

    private void AddFormationDropdown(
        Control parent, string field, Point point, int width,
        IReadOnlyList<FormationOption> options)
    {
        var picker = new ComboBox
        {
            Location = point,
            Size = new Size(width, 21),
            Tag = field,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = LegacyFont,
            IntegralHeight = false,
            DropDownHeight = 300,
        };
        Theme.ApplyCombo(picker);
        picker.Items.AddRange(options.Cast<object>().ToArray());
        picker.SelectedIndexChanged += (_, _) => CommitFormationPicker(picker);
        parent.Controls.Add(picker);
        _formationPickers[field] = picker;
    }

    private void CommitFormationPicker(ComboBox picker)
    {
        if (_syncFormationPickers || CurrentRowIndex < 0 || picker.Tag is not string field ||
            picker.SelectedItem is not FormationOption option ||
            !CurrentValues.TryGetValue(field, out var value) || !value.IsWritable)
            return;
        StageField("formations", CurrentRowIndex, value.FieldName, option.Value, _pickerStaging);
    }

    private void ClearFormationSlot(int slot)
    {
        if (CurrentRowIndex < 0 || slot is < 0 or > 10) return;
        foreach (var (field, value) in new[] { ($"position{slot}", "-1"), ($"pos{slot}role", "0") })
        {
            if (CurrentValues.TryGetValue(field, out var target) && target.IsWritable)
                StageField("formations", CurrentRowIndex, target.FieldName, value, _pickerStaging);
        }
        RefreshFormationPickers();
        _pitch.Invalidate();
    }

    private void RefreshFormationPickers()
    {
        _syncFormationPickers = true;
        try
        {
            foreach (var (field, picker) in _formationPickers)
            {
                var raw = CurrentValues.TryGetValue(field, out var value) ? value.RawValue : string.Empty;
                picker.SelectedItem = picker.Items.Cast<FormationOption>()
                    .FirstOrDefault(item => item.Value.Equals(raw, StringComparison.OrdinalIgnoreCase))
                    ?? picker.Items.Cast<FormationOption>().FirstOrDefault();
                picker.Enabled = CurrentValues.TryGetValue(field, out value) && value.IsWritable;
                picker.BackColor = picker.Enabled ? Theme.Input : Theme.Raised;
                picker.ForeColor = picker.Enabled ? Theme.Text : Theme.Muted;
            }
        }
        finally { _syncFormationPickers = false; }
    }

    private void ResizePitchPreview()
    {
        if (_pitchGroup.Width <= 0 || _pitchGroup.Height <= 0) return;
        var width = Math.Max(260, _pitchGroup.ClientSize.Width - 16);
        var statusHeight = 28;
        var height = Math.Max(190, _pitchGroup.ClientSize.Height - 20 - statusHeight - 8);
        _pitch.Bounds = new Rectangle(8, 20, width, height);
        _pitchStatus.Bounds = new Rectangle(12, _pitch.Bottom + 4, Math.Max(100, _pitchGroup.ClientSize.Width - 24), statusHeight);
        _pitch.Invalidate();
    }

    private void DrawFormationPitch(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var field = RatableBoard.DrawPitch(g, _pitch.ClientRectangle, Color.FromArgb(43, 132, 82));
        if (_pitch.ClientSize.Width < 100 || _pitch.ClientSize.Height < 100) return;
        var occupied = new List<Rectangle>();
        for (var i = 0; i < 11; i++)
        {
            if (!TryCoordinate(Value($"offset{i}x"), out var x) || !TryCoordinate(Value($"offset{i}y"), out var y)) continue;
            var rawPosition = Value($"position{i}");
            var position = int.TryParse(rawPosition, out var positionCode)
                ? NameResolverService.PositionLabel(positionCode)
                : "—";
            const int boxWidth = 88, boxHeight = 48;
            // Stored offsets are normalized centres.  Anchor them to the
            // actual field rectangle (rather than the whole control) so the
            // cards stay inside the painted pitch after a resize.
            var centreX = field.Left + (int)Math.Round(x * field.Width);
            var centreY = field.Top + (int)Math.Round(y * field.Height);
            var minLeft = Math.Min(field.Left + 3, Math.Max(0, field.Right - boxWidth));
            var maxLeft = Math.Max(minLeft, field.Right - boxWidth - 3);
            var minTop = Math.Min(field.Top + 3, Math.Max(0, field.Bottom - boxHeight));
            var maxTop = Math.Max(minTop, field.Bottom - boxHeight - 3);
            var left = Math.Clamp(centreX - (boxWidth / 2), minLeft, maxLeft);
            var top = Math.Clamp(centreY - (boxHeight / 2), minTop, maxTop);
            var box = FindFreeFormationBox(left, top, boxWidth, boxHeight, occupied, _pitch.ClientSize);
            occupied.Add(box);
            var marker = new Rectangle(box.Left + 27, box.Top, 34, 34);
            using var shadow = new SolidBrush(Color.FromArgb(70, Color.Black));
            g.FillEllipse(shadow, marker.Left + 2, marker.Top + 3, marker.Width, marker.Height);
            using var fill = new SolidBrush(Color.FromArgb(18, 43, 71));
            g.FillEllipse(fill, marker);
            using var markerPen = new Pen(Color.White, 2);
            g.DrawEllipse(markerPen, marker);
            TextRenderer.DrawText(g, (i + 1).ToString(), Theme.BodyBold, marker,
                Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            var caption = new Rectangle(box.Left, box.Top + 32, box.Width, 16);
            TextRenderer.DrawText(g, position, Theme.BodyBold, caption,
                Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    internal static Rectangle FindFreeFormationBox(
        int left, int top, int width, int height, IReadOnlyList<Rectangle> occupied, Size pitchSize)
    {
        var minX = Math.Min(12, Math.Max(0, pitchSize.Width - width));
        var maxX = Math.Max(minX, pitchSize.Width - width - 12);
        var minY = Math.Min(12, Math.Max(0, pitchSize.Height - height));
        var maxY = Math.Max(minY, pitchSize.Height - height - 12);
        var candidate = new Rectangle(left, top, width, height);
        if (!occupied.Any(box => box.IntersectsWith(candidate))) return candidate;

        // FC26 can store identical or nearly identical offsets for a GK and a
        // defensive slot. Keep the record values untouched, but separate their
        // visual labels so the formation remains readable.
        foreach (var distance in new[] { 12, 24, 36, 48, 60, 72, 84 })
        {
            foreach (var (dx, dy) in new[] { (-distance, 0), (distance, 0), (0, distance), (0, -distance), (-distance, distance), (distance, distance) })
            {
                var x = Math.Clamp(left + dx, minX, maxX);
                var y = Math.Clamp(top + dy, minY, maxY);
                candidate = new Rectangle(x, y, width, height);
                if (!occupied.Any(box => box.IntersectsWith(candidate))) return candidate;
            }
        }
        return candidate;
    }

    private static bool TryCoordinate(string raw, out double coordinate)
    {
        coordinate = 0;
        if (!double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value)) return false;
        coordinate = value is >= 0 and <= 1 ? value : value / 100d;
        return coordinate is >= 0 and <= 1;
    }
}

public sealed class KitsSection : ClassicEntitySection
{
    private readonly PictureBox _texturePreview;
    private readonly Label _assetStatus;
    private readonly Button _loadTexture;
    private CancellationTokenSource? _previewCancellation;

    public KitsSection(AppServices s) : base(s, "kits", "Kits", "teamkits", () => s.RequireData().GetKits(), LabelMaps.Kits)
    {
        var general = AddCanvasTab("General"); var c = Canvas(general);
        var texture = Group("Texture", new Point(3, 3), new Size(750, 560));
        _texturePreview = ImageSurface(texture, new Point(5, 20), new Size(734, 480), "Kit texture");
        _loadTexture = new Button
        {
            Text = "Load Texture",
            Location = new Point(8, 527),
            Size = new Size(128, 25),
            Font = LegacyFont,
        };
        _loadTexture.Click += async (_, _) => await LoadFrostbitePreviewAsync(_previewCancellation?.Token ?? default);
        texture.Controls.Add(_loadTexture);
        var importKit = new Button { Text = "Import", Location = new Point(142, 527), Size = new Size(72, 25), Font = LegacyFont };
        importKit.Click += (_, _) => ImportKitTexture();
        texture.Controls.Add(importKit);
        var exportKit = new Button { Text = "Export", Location = new Point(220, 527), Size = new Size(72, 25), Font = LegacyFont };
        exportKit.Click += (_, _) => ExportKitTexture();
        texture.Controls.Add(exportKit);
        var hotspot = new Button { Text = "Hotspot", Location = new Point(298, 527), Size = new Size(80, 25), Font = LegacyFont };
        hotspot.Click += (_, _) => OpenKitHotspot();
        texture.Controls.Add(hotspot);
        _assetStatus = new Label
        {
            Text = "Select a kit",
            Location = new Point(386, 531),
            Size = new Size(316, 18),
            Font = LegacyFont,
            ForeColor = Theme.Muted,
            BackColor = Theme.Panel,
        };
        texture.Controls.Add(_assetStatus);
        c.Controls.Add(texture);
        var model = Group("3D Model", new Point(759, 3), new Size(750, 560));
        ThreeDViewerLauncher.AttachPlaceholder(model, new Point(5, 20), new Size(730, 480), "kit",
            () => new[] { Value("teamkitid"), "kit_" + Value("teamkitid"), Value("jerseytemplateindex") },
            () => Services.FrostbiteAssets.ExportMeshForQuery(new[] { $"kit_{Value("teamkittypetechid")}", $"kit_{Value("teamtechid")}_{Value("teamkittypetechid")}" }));
        c.Controls.Add(model);

        // Uniform rows below the previews: every group in a row shares the row's
        // top and bottom edge, fields pitch at 26 px, captions are right-aligned
        // 6 px from their editor. The canvas scrolls on smaller displays.
        var colours = Group("Colours and Percentages", new Point(3, 571), new Size(470, 176));
        AddField(colours, "teamcolorprimr", "Primary R", new Point(110, 20), 70);
        AddField(colours, "teamcolorprimg", "Primary G", new Point(110, 46), 70);
        AddField(colours, "teamcolorprimb", "Primary B", new Point(110, 72), 70);
        AddField(colours, "teamcolorprimpercent", "Primary %", new Point(110, 98), 70);
        AddField(colours, "teamcolorsecr", "Secondary R", new Point(260, 20), 70);
        AddField(colours, "teamcolorsecg", "Secondary G", new Point(260, 46), 70);
        AddField(colours, "teamcolorsecb", "Secondary B", new Point(260, 72), 70);
        AddField(colours, "teamcolorsecpercent", "Secondary %", new Point(260, 98), 70);
        AddField(colours, "teamcolortertr", "Tertiary R", new Point(400, 20), 70);
        AddField(colours, "teamcolortertg", "Tertiary G", new Point(400, 46), 70);
        AddField(colours, "teamcolortertb", "Tertiary B", new Point(400, 72), 70);
        AddField(colours, "teamcolortertpercent", "Tertiary %", new Point(400, 98), 70);
        c.Controls.Add(colours);

        var tpl = Group("Templates", new Point(479, 571), new Size(325, 176));
        AddField(tpl, "jerseytemplateindex", "Jersey", new Point(160, 20), 140);
        AddField(tpl, "shortstemplateindex", "Shorts", new Point(160, 46), 140);
        AddField(tpl, "sockstemplateindex", "Socks", new Point(160, 72), 140);
        AddField(tpl, "numberfonttype", "Number Font", new Point(160, 98), 140);
        c.Controls.Add(tpl);

        var badge = Group("Badges", new Point(810, 571), new Size(375, 176));
        AddField(badge, "chestbadge", "Chest Badge", new Point(210, 20), 145);
        AddField(badge, "jerseyleftsleevebadge", "Left Sleeve Badge", new Point(210, 46), 145);
        AddField(badge, "jerseyrightsleevebadge", "Right Sleeve Badge", new Point(210, 72), 145);
        AddField(badge, "captainarmband", "Captain Armband", new Point(210, 98), 145);
        AddField(badge, "armbandtype", "Armband Type", new Point(210, 124), 145);
        c.Controls.Add(badge);

        var nameColours = Group("Name Colours", new Point(1191, 571), new Size(400, 176));
        AddField(nameColours, "jerseynamecolorr", "Name R", new Point(200, 20), 160);
        AddField(nameColours, "jerseynamecolorg", "Name G", new Point(200, 46), 160);
        AddField(nameColours, "jerseynamecolorb", "Name B", new Point(200, 72), 160);
        AddField(nameColours, "jerseynameoutlinecolorr", "Outline R", new Point(200, 98), 160);
        AddField(nameColours, "jerseynameoutlinecolorg", "Outline G", new Point(200, 124), 160);
        AddField(nameColours, "jerseynameoutlinecolorb", "Outline B", new Point(200, 150), 160);
        c.Controls.Add(nameColours);

        var numbers = Group("Name and Numbers", new Point(3, 755), new Size(525, 230));
        AddField(numbers, "jerseyfrontnumberplacementcode", "Front Number", new Point(250, 20), 180);
        AddField(numbers, "jerseybacknameplacementcode", "Back Name", new Point(250, 46), 180);
        AddField(numbers, "jerseybacknamefontcase", "Back Name Case", new Point(250, 72), 180);
        AddField(numbers, "jerseynamefonttype", "Name Font", new Point(250, 98), 180);
        AddField(numbers, "jerseynamelayouttype", "Name Layout", new Point(250, 124), 180);
        AddField(numbers, "jerseynameoutlinewidth", "Name Outline Width", new Point(250, 150), 180);
        AddField(numbers, "shortsnumberplacementcode", "Shorts Number", new Point(250, 176), 180);
        AddField(numbers, "shortsnumberfonttype", "Shorts Font", new Point(250, 202), 180);
        c.Controls.Add(numbers);
        var shape = Group("Appearance", new Point(534, 755), new Size(525, 230));
        AddField(shape, "jerseyshapestyle", "Jersey Shape", new Point(250, 20), 180);
        AddField(shape, "jerseycollargeometrytype", "Collar Geometry", new Point(250, 46), 180);
        AddField(shape, "jerseyfit", "Jersey Fit", new Point(250, 72), 180);
        AddField(shape, "shortstyle", "Shorts Style", new Point(250, 98), 180);
        AddField(shape, "jerseyrestriction", "Jersey Restriction", new Point(250, 124), 180);
        AddField(shape, "hasadvertisingkit", "Advertising Kit", new Point(250, 150), 180);
        c.Controls.Add(shape);
        var rendering = Group("Rendering", new Point(1065, 755), new Size(525, 230));
        AddField(rendering, "renderingmaterialtype", "Material", new Point(250, 20), 180);
        AddField(rendering, "jerseyrenderingdetailmaptype", "Jersey Detail Map", new Point(250, 46), 180);
        AddField(rendering, "shortsrenderingdetailmaptype", "Shorts Detail Map", new Point(250, 72), 180);
        AddField(rendering, "isinheritbasedetailmap", "Inherit Base Detail", new Point(250, 98), 180);
        AddField(rendering, "isgeneric", "Generic Kit", new Point(250, 124), 180);
        AddField(rendering, "islocked", "Locked", new Point(250, 150), 180);
        AddField(rendering, "isembargoed", "Embargoed", new Point(250, 176), 180);
        c.Controls.Add(rendering);

        var numberColours = Group("Jersey Number Colours", new Point(3, 993), new Size(525, 254));
        AddField(numberColours, "jerseynumbercolorprimr", "Primary R", new Point(180, 20), 100);
        AddField(numberColours, "jerseynumbercolorprimg", "Primary G", new Point(180, 46), 100);
        AddField(numberColours, "jerseynumbercolorprimb", "Primary B", new Point(180, 72), 100);
        AddField(numberColours, "jerseynumbercolorsecr", "Secondary R", new Point(180, 98), 100);
        AddField(numberColours, "jerseynumbercolorsecg", "Secondary G", new Point(180, 124), 100);
        AddField(numberColours, "jerseynumbercolorsecb", "Secondary B", new Point(180, 150), 100);
        AddField(numberColours, "jerseynumbercolorterr", "Tertiary R", new Point(180, 176), 100);
        AddField(numberColours, "jerseynumbercolorterg", "Tertiary G", new Point(180, 202), 100);
        AddField(numberColours, "jerseynumbercolorterb", "Tertiary B", new Point(180, 228), 100);
        c.Controls.Add(numberColours);

        var shortsColours = Group("Shorts Number Colours", new Point(534, 993), new Size(525, 254));
        AddField(shortsColours, "shortsnumbercolorprimr", "Primary R", new Point(180, 20), 100);
        AddField(shortsColours, "shortsnumbercolorprimg", "Primary G", new Point(180, 46), 100);
        AddField(shortsColours, "shortsnumbercolorprimb", "Primary B", new Point(180, 72), 100);
        AddField(shortsColours, "shortsnumbercolorsecr", "Secondary R", new Point(180, 98), 100);
        AddField(shortsColours, "shortsnumbercolorsecg", "Secondary G", new Point(180, 124), 100);
        AddField(shortsColours, "shortsnumbercolorsecb", "Secondary B", new Point(180, 150), 100);
        AddField(shortsColours, "shortsnumbercolorterr", "Tertiary R", new Point(180, 176), 100);
        AddField(shortsColours, "shortsnumbercolorterg", "Tertiary G", new Point(180, 202), 100);
        AddField(shortsColours, "shortsnumbercolorterb", "Tertiary B", new Point(180, 228), 100);
        c.Controls.Add(shortsColours);

        // Kit Record group removed — technical identifiers are not needed for editing.
    }

    protected override void OnRecordShown()
    {
        _previewCancellation?.Cancel();
        _previewCancellation?.Dispose();
        _previewCancellation = new CancellationTokenSource();
        _ = LoadFrostbitePreviewAsync(_previewCancellation.Token);
    }

    private async Task LoadFrostbitePreviewAsync(CancellationToken cancellationToken = default)
    {
        if (!TryRawInt("teamtechid", out var teamId) ||
            !TryRawInt("teamkittypetechid", out var kitType))
        {
            SetAssetStatus("Kit identifiers are unavailable.", isError: true);
            return;
        }
        if (!Services.FrostbiteAssets.IsAvailable)
        {
            SetAssetStatus("The asset library is unavailable.", isError: true);
            return;
        }

        var variant = kitType switch
        {
            0 => "home",
            1 => "away",
            2 => "third",
            3 => "gk",
            4 => "gk_away",
            5 => "gk_third",
            _ => string.Empty,
        };
        if (variant.Length == 0)
        {
            SetAssetStatus($"Unsupported kit type {kitType}.", isError: true);
            return;
        }

        _loadTexture.Enabled = false;
        SetAssetStatus("Searching installed assets…");
        try
        {
            var query = $"_{teamId}/{variant}_";
            var matches = await Task.Run(
                () => Services.FrostbiteAssets.SearchAssets(query, "Res", 100),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var selected = matches
                .Where(match => match.ResType == 0x6BDE20BA &&
                                match.Name.EndsWith("_color", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(match => TextureScore(match.Name))
                .FirstOrDefault();
            if (selected == null)
            {
                SetAssetStatus($"No colour texture found for team {teamId} ({variant}).", isError: true);
                return;
            }

            SetAssetStatus("Extracting RES and texture chunk…");
            var path = await Task.Run(
                () => Services.FrostbiteAssets.ExportTexture(selected.Name),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(path))
            {
                SetAssetStatus("Texture extraction failed.", isError: true);
                return;
            }

            var preview = await Task.Run(
                () => Services.Textures.CreatePreview(path, 700, 480),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var old = _texturePreview.Image;
            _texturePreview.Image = preview;
            old?.Dispose();
            SetAssetStatus(selected.Name);
        }
        catch (OperationCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CM26] Kit preview superseded: {ex.Message}");
            // Selecting another kit supersedes this preview request.
        }
        catch (Exception ex)
        {
            SetAssetStatus(ex.Message, isError: true);
        }
        finally
        {
            if (!IsDisposed) _loadTexture.Enabled = true;
        }
    }

    private bool TryRawInt(string field, out int value)
    {
        value = 0;
        return CurrentValues.TryGetValue(field, out var current) &&
               int.TryParse(current.RawValue, out value);
    }

    private static int TextureScore(string name)
    {
        var score = 0;
        if (name.Contains("/jersey_", StringComparison.OrdinalIgnoreCase)) score += 100;
        if (name.Contains("jersey", StringComparison.OrdinalIgnoreCase)) score += 30;
        if (name.Contains("brand_", StringComparison.OrdinalIgnoreCase)) score -= 60;
        if (name.Contains("crest_", StringComparison.OrdinalIgnoreCase)) score -= 80;
        if (name.Contains("number_", StringComparison.OrdinalIgnoreCase)) score -= 80;
        return score;
    }

    private void SetAssetStatus(string text, bool isError = false)
    {
        if (IsDisposed) return;
        _assetStatus.Text = text;
        _assetStatus.ForeColor = isError ? Theme.Warning : Theme.Muted;
        ToolTip.SetToolTip(_assetStatus, text);
    }

    private string? BuildKitTextureLegacyPath()
    {
        if (!TryRawInt("teamtechid", out var teamId) ||
            !TryRawInt("teamkittypetechid", out var kitType) ||
            !TryRawInt("teamkitid", out var kitId))
            return null;
        var variant = kitType switch
        {
            0 => "home", 1 => "away", 2 => "third",
            3 => "gk", 4 => "gk_away", 5 => "gk_third",
            _ => string.Empty,
        };
        if (variant.Length == 0) return null;
        return $"content/character/kit/{teamId}/{variant}_1_0/jersey_{kitId}_1_0_color.dds";
    }

    private void ImportKitTexture()
    {
        var legacyPath = BuildKitTextureLegacyPath();
        if (string.IsNullOrWhiteSpace(legacyPath))
        {
            MessageBox.Show(FindForm(), "Kit identifiers are unavailable. Select a kit record first.",
                "Import Kit Texture", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var dialog = new OpenFileDialog
        {
            Title = "Import Kit Texture",
            Filter = "Texture files (*.dds;*.png;*.jpg;*.jpeg;*.bmp)|*.dds;*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            var path = Services.LegacyMods.StageImage(legacyPath, dialog.FileName, 2048, 2048);
            using var preview = Services.Textures.CreatePreview(path, _texturePreview.Width, _texturePreview.Height);
            var old = _texturePreview.Image;
            _texturePreview.Image = preview == null ? null : new Bitmap(preview);
            old?.Dispose();
            SetAssetStatus("Kit texture imported. Save to apply.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Import Kit Texture",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportKitTexture()
    {
        var legacyPath = BuildKitTextureLegacyPath();
        if (string.IsNullOrWhiteSpace(legacyPath))
        {
            MessageBox.Show(FindForm(), "Kit identifiers are unavailable. Select a kit record first.",
                "Export Kit Texture", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var staged = Services.LegacyMods.GetReplacement(legacyPath);
        var path = staged ?? Services.FrostbiteAssets.ExportLegacyAsset(legacyPath);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show(FindForm(), "No installed or staged kit texture is available to export.",
                "Export Kit Texture", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Title = "Export Kit Texture",
            FileName = Path.GetFileName(path),
            Filter = "DDS texture (*.dds)|*.dds|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try { File.Copy(path, dialog.FileName, overwrite: true); SetAssetStatus("Kit texture exported."); }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Export Kit Texture",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void OpenKitHotspot()
    {
        if (!TryRawInt("teamtechid", out var teamId) || !TryRawInt("teamkittypetechid", out var kitType))
        {
            MessageBox.Show(FindForm(), "Kit identifiers are unavailable. Select a kit record first.",
                "Kit Hotspot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _ = ThreeDViewerLauncher.OpenAsync(
            this, "kit",
            new[] { Value("teamkitid"), "kit_" + Value("teamkitid"), Value("jerseytemplateindex") },
            () => Services.FrostbiteAssets.ExportMeshForQuery(new[] { $"kit_{kitType}", $"{teamId}_{kitType}" }));
    }
}
