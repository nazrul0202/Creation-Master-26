using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>
/// CM16-style forms for FC26 tables that did not exist in the CM16 schema.
/// Each derived form declares its own FC26 fields and fixed group-box layout;
/// this is intentionally not a generic all-fields browser.
/// </summary>
internal abstract class Fc26ExtensionSection : SectionBase
{
    protected static readonly Font LegacyFont = Theme.Body;
    private static readonly IReadOnlyDictionary<string, string> FriendlyLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["adsponserid"] = "Sponsor Id",
            ["basecolour"] = "Base Colour",
            ["isea"] = "EA Sponsor",
            ["isfut"] = "Ultimate Team",
            ["nationid"] = "Nation Id",
            ["whistlesregionindex"] = "Whistle Region",
            ["crowdbedsregionindex"] = "Crowd Bed Region",
            ["chantregionindex"] = "Chant Region",
            ["playercallpatchbankindex"] = "Player Call Patch Bank",
            ["ssfplayercallindex"] = "Player Call Index",
            ["reactionsregionindex"] = "Crowd Reaction Region",
            ["hecklesregionindex"] = "Heckles Region",
            ["ambienceregionindex"] = "Ambience Region",
            ["palanguageindex"] = "PA Language",
            ["defaultcommlang"] = "Commentary Language",
            ["teamcanwhistleindex"] = "Team Whistle Set",
            ["artificialkey"] = "Link Id",
            ["teamid"] = "Team Id",
            ["leagueid"] = "League Id",
            ["modeid"] = "Presentation Mode Id",
            ["modetypestr"] = "Presentation Mode",
            ["isbrandpartnersenabled"] = "Brand Partners",
            ["isuniqueadboardsmodeenabled"] = "Unique Adboards",
            ["iscompetitionspecificboardsmodeenabled"] = "Competition Adboards",
            ["isgoallinetechforceenabled"] = "Force Goal-Line Technology",
            ["dynamicimageid"] = "Dynamic Image Id",
            ["isapproved"] = "Approved",
            ["storyid"] = "Story Id",
            ["target_fixture"] = "Target Fixture",
            ["entity_id"] = "Entity Id",
            ["entity_type"] = "Entity Type",
            ["audioid"] = "Audio Id",
            ["stadiumid"] = "Stadium Id",
            ["stadiumpalanguageindex"] = "Stadium PA Language",
            ["matchsizetypeindex"] = "Match Size Type"
        };
    private readonly string _key, _title, _table;
    private readonly string[] _fields;
    private readonly List<TextBox> _editors = [];
    private readonly Dictionary<string, FieldValue> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly FieldEditorGrid _staging = new();

    protected Fc26ExtensionSection(AppServices services, string key, string title, string table, string group, params string[] fields) : base(services)
    {
        _key = key; _title = title; _table = table; _fields = fields;
        Header.Visible = false;
        var page = new TabPage("General") { BackColor = Theme.Background, Font = LegacyFont };
        var canvas = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CardLayout.CardBackground };
        var boxHeight = Math.Max(120, 25 + ((fields.Length + 1) / 2 * 26));
        var box = new Panel { Location = new Point(4, 4), Size = new Size(630, boxHeight), BackColor = CardLayout.CardWhite };
        CardLayout.ApplyRounded(box, 10);
        box.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(630, 4), BackColor = CardLayout.Fc26Green });
        box.Controls.Add(new Label { Text = group, Location = new Point(10, 8), Size = new Size(610, 16), Font = Theme.BodyBold, ForeColor = CardLayout.Fc26Green, BackColor = CardLayout.CardWhite });
        for (var i = 0; i < fields.Length; i++)
        {
            var col = i % 2; var row = i / 2;
            var x = col == 0 ? 12 : 322; var y = 28 + (row * 26);
            var label = new Label { Text = Label(fields[i]), Location = new Point(x, y + 3), Size = new Size(165, 18), Font = LegacyFont, TextAlign = ContentAlignment.MiddleRight, AutoEllipsis = true, ForeColor = CardLayout.CardFieldLabel, BackColor = CardLayout.CardWhite };
            box.Controls.Add(label);
            ToolTip.SetToolTip(label, Label(fields[i]));
            var editor = new TextBox { Location = new Point(x + 171, y), Size = new Size(145, 20), Font = LegacyFont, Tag = fields[i] };
            Theme.ApplyTextBox(editor);
            editor.Leave += (_, _) => Commit(editor);
            _editors.Add(editor); box.Controls.Add(editor);
        }
        canvas.Controls.Add(box); page.Controls.Add(canvas); Tabs.TabPages.Add(page);
    }

    public override string SectionKey => _key;
    public override string SectionTitle => _title;
    protected override string TableName => _table;
    protected IReadOnlyDictionary<string, FieldValue> CurrentValues => _values;

    protected TabPage AddCanvasTab(string title)
    {
        var page = new TabPage(title) { BackColor = Theme.Background, Font = LegacyFont };
        page.Controls.Add(new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CardLayout.CardBackground });
        Tabs.TabPages.Add(page);
        return page;
    }

    protected static Panel Card(string title, Point location, Size size)
    {
        var card = new Panel { Location = location, Size = size, BackColor = CardLayout.CardWhite };
        CardLayout.ApplyRounded(card, 10);
        card.Controls.Add(new Panel { Location = Point.Empty, Size = new Size(size.Width, 4), BackColor = CardLayout.Fc26Green });
        card.Controls.Add(new Label
        {
            Text = title, Location = new Point(10, 8), Size = new Size(Math.Max(60, size.Width - 20), 16),
            Font = Theme.BodyBold, ForeColor = CardLayout.Fc26Green, BackColor = CardLayout.CardWhite
        });
        return card;
    }

    protected override IReadOnlyList<RecordListItem> GetRecords()
    {
        var table = Services.Session.GetTable(_table); if (table == null) return Array.Empty<RecordListItem>();
        var key = table.Columns.FirstOrDefault(c => c.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))?.Name ?? table.Columns.First().Name;
        var name = table.Columns.FirstOrDefault(c => c.Name.Equals("name", StringComparison.OrdinalIgnoreCase))?.Name;
        var list = new List<RecordListItem>(table.RowCount);
        for (var row = 0; row < table.RowCount; row++)
        {
            var record = Services.Session.GetRecord(_table, row); if (record == null) continue;
            var id = Services.Session.GetCell(_table, row, key);
            var title = name == null ? $"{_title} {id}" : Services.Session.GetCell(_table, row, name);
            list.Add(new RecordListItem { RecordIndex = row, Title = string.IsNullOrWhiteSpace(title) ? $"{_title} {id}" : title, Subtitle = $"ID {id}" });
        }
        return list;
    }

    protected override void ShowRecord(int recordIndex)
    {
        _values.Clear();
        var allValues = Services.RequireData().GetFields(_table, recordIndex);
        foreach (var value in allValues) _values[value.FieldName] = value;
        foreach (var editor in _editors)
        {
            var key = editor.Tag as string ?? string.Empty;
            if (_values.TryGetValue(key, out var value))
            {
                editor.Text = value.Value; editor.ReadOnly = !value.IsWritable;
                editor.BackColor = value.IsWritable ? Theme.Input : CardLayout.CardFieldBg;
                editor.ForeColor = CardLayout.CardText;
                ToolTip.SetToolTip(editor, value.IsWritable
                    ? Label(key)
                    : Label(key) + " (read-only)");
            }
            else { editor.Text = ""; editor.ReadOnly = true; editor.BackColor = CardLayout.CardFieldBg; editor.ForeColor = CardLayout.CardSubtle; }
        }
        OnRecordShown();
    }

    protected virtual void OnRecordShown() { }

    private void Commit(TextBox editor)
    {
        var field = editor.Tag as string ?? string.Empty;
        if (CurrentRecordIndex >= 0 && !editor.ReadOnly && _values.TryGetValue(field, out var value) && editor.Text.Trim() != value.Value)
            StageField(_table, CurrentRecordIndex, field, editor.Text.Trim(), _staging);
    }

    private static string Label(string field)
    {
        if (FriendlyLabels.TryGetValue(field, out var friendly))
            return friendly;
        var text = field.Replace('_', ' ').Trim();
        return string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }
}

internal sealed class SponsorsSection : Fc26ExtensionSection
{
    private readonly PictureBox _preview = new();
    private readonly Label _caption = new();

    public SponsorsSection(AppServices s)
        : base(s, "sponsors", "Sponsors", "sponsors", "Sponsor",
            "adsponserid", "name", "basecolour", "length", "isea", "isfut")
    {
        var page = AddCanvasTab("Preview");
        var canvas = (Panel)page.Controls[0];
        var box = Card("Sponsor Artwork", new Point(4, 4), new Size(850, 520));
        _preview.Location = new Point(12, 28);
        _preview.Size = new Size(825, 410);
        _preview.BackColor = CardLayout.CardFieldBg;
        _preview.BorderStyle = BorderStyle.FixedSingle;
        _preview.SizeMode = PictureBoxSizeMode.Zoom;
        box.Controls.Add(_preview);
        _caption.Location = new Point(12, 446);
        _caption.Size = new Size(825, 40);
        _caption.Font = LegacyFont;
        _caption.TextAlign = ContentAlignment.MiddleCenter;
        box.Controls.Add(_caption);
        LegacyAssetActions.Attach(Services, box, _preview, new Point(12, 492), () => OnRecordShown());
        canvas.Controls.Add(box);
    }

    protected override void OnRecordShown()
    {
        var sponsorId = Value("adsponserid");
        var candidates = new[]
        {
            $"data/ui/imgAssets/cmSponsors/cmSponsors{sponsorId}.dds",
            $"data/ui/imgAssets/sponsors/sponsor_{sponsorId}.dds",
            $"data/ui/imgAssets/dynamicSponsors/dynamic_{sponsorId}.dds"
        };
        var staged = candidates.FirstOrDefault(path => Services.LegacyMods.GetReplacement(path) != null);
        LegacyAssetActions.SetTarget(_preview, new LegacyAssetEditTarget(staged ?? candidates[0], 1024, 256));
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(
            _preview,
            Services,
            staged == null ? null : Services.LegacyMods.GetReplacement(staged),
            candidates,
            (image, source) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                _preview.Image?.Dispose();
                _preview.Image = image;
                _caption.Text = image == null
                    ? $"Sponsor {sponsorId} loaded; installed artwork not found"
                    : $"Sponsor {sponsorId} · {source}";
            },
            path => LegacyAssetActions.SetTarget(
                _preview, new LegacyAssetEditTarget(path, 1024, 256)));
    }

    private int Value(string field) =>
        CurrentValues.TryGetValue(field, out var value) && int.TryParse(value.RawValue, out var parsed) ? parsed : 0;
}
internal sealed class AudioNationSection : Fc26ExtensionSection
{
    // Frostbite NewWaveAsset RES type used by FC26 audio resources (0xB2C465F6),
    // matching FET's ResourceType.NewWaveAsset catalog filter.
    private const uint NewWaveResourceType = 0xB2C465F6;
    private readonly TextBox _query = new();
    private readonly ListView _banks = new();
    private readonly ListView _dataSets = new();
    private readonly Label _bankDetails = new();
    private readonly Button _inspect = new();
    private readonly Button _export = new();
    private readonly Button _previewAudio = new();
    private readonly Button _stopAudio = new();
    private string? _selectedBank;
    private string? _extractedPath;

    public AudioNationSection(AppServices s)
        : base(s, "audio", "Audio", "audionation", "Nation Audio",
            "nationid", "whistlesregionindex", "crowdbedsregionindex",
            "chantregionindex", "playercallpatchbankindex", "ssfplayercallindex",
            "reactionsregionindex", "hecklesregionindex", "ambienceregionindex",
            "palanguageindex", "defaultcommlang", "teamcanwhistleindex")
    {
        var page = AddCanvasTab("NewWave Banks");
        var canvas = (Panel)page.Controls[0];
        var box = Card("NewWave Audio Banks", new Point(4, 4), new Size(1120, 610));
        box.Controls.Add(new Label
        {
            Text = "Search", Location = new Point(12, 31), Size = new Size(54, 20),
            TextAlign = ContentAlignment.MiddleRight, Font = LegacyFont,
            ForeColor = CardLayout.CardFieldLabel, BackColor = CardLayout.CardWhite
        });
        _query.Location = new Point(72, 30);
        _query.Size = new Size(430, 20);
        _query.Text = "sound/chants/newwaves";
        box.Controls.Add(_query);
        var search = new Button
        {
            Text = "Search Banks", Location = new Point(510, 24), Size = new Size(105, 25)
        };
        Theme.ApplyButton(search);
        search.Click += async (_, _) => await SearchBanksAsync(search);
        box.Controls.Add(search);

        _banks.Location = new Point(12, 58);
        _banks.Size = new Size(590, 545);
        _banks.View = View.Details;
        _banks.FullRowSelect = true;
        _banks.HideSelection = false;
        _banks.BackColor = Theme.Input;
        _banks.ForeColor = Theme.Text;
        _banks.Font = Theme.Body;
        _banks.Columns.Add("NewWave resource", 445);
        _banks.Columns.Add("Size", 90, HorizontalAlignment.Right);
        _banks.SelectedIndexChanged += (_, _) => SelectBank();
        box.Controls.Add(_banks);

        _bankDetails.Location = new Point(618, 59);
        _bankDetails.Size = new Size(482, 65);
        _bankDetails.BorderStyle = BorderStyle.FixedSingle;
        _bankDetails.Padding = new Padding(6);
        _bankDetails.Text = "Select a NewWave bank to inspect metadata. CM26 does not decode or play bank samples.";
        box.Controls.Add(_bankDetails);

        _inspect.Text = "Inspect Bank";
        _inspect.Location = new Point(618, 132);
        _inspect.Size = new Size(105, 26);
        _inspect.Enabled = false;
        Theme.ApplyButton(_inspect);
        _inspect.Click += async (_, _) => await InspectBankAsync();
        box.Controls.Add(_inspect);
        _export.Text = "Export Raw Bank";
        _export.Location = new Point(731, 132);
        _export.Size = new Size(120, 26);
        _export.Enabled = false;
        Theme.ApplyButton(_export);
        _export.Click += (_, _) => ExportBank();
        box.Controls.Add(_export);
        _previewAudio.Text = "Play Local File…";
        _previewAudio.Location = new Point(859, 132);
        _previewAudio.Size = new Size(116, 26);
        Theme.ApplyButton(_previewAudio);
        _previewAudio.Click += (_, _) => PreviewLocalAudio();
        box.Controls.Add(_previewAudio);
        _stopAudio.Text = "Stop";
        _stopAudio.Location = new Point(983, 132);
        _stopAudio.Size = new Size(70, 26);
        Theme.ApplyButton(_stopAudio);
        _stopAudio.Click += (_, _) => AudioPreviewService.Stop();
        box.Controls.Add(_stopAudio);

        _dataSets.Location = new Point(618, 168);
        _dataSets.Size = new Size(482, 435);
        _dataSets.View = View.Details;
        _dataSets.FullRowSelect = true;
        _dataSets.BackColor = Theme.Input;
        _dataSets.ForeColor = Theme.Text;
        _dataSets.Font = Theme.Body;
        _dataSets.Columns.Add("Dataset", 130);
        _dataSets.Columns.Add("Rows", 65, HorizontalAlignment.Right);
        _dataSets.Columns.Add("Fields", 65, HorizontalAlignment.Right);
        _dataSets.Columns.Add("Indexes", 65, HorizontalAlignment.Right);
        _dataSets.Columns.Add("Sample group", 125);
        box.Controls.Add(_dataSets);
        canvas.Controls.Add(box);
        Disposed += (_, _) => AudioPreviewService.Stop();
    }

    private async Task SearchBanksAsync(Button search)
    {
        search.Enabled = false;
        _banks.Items.Clear();
        _dataSets.Items.Clear();
        _bankDetails.Text = "Searching the installed Data/Patch archives...";
        try
        {
            var query = _query.Text.Trim();
            var matches = await Task.Run(() =>
                Services.FrostbiteAssets.SearchAssets(query, "Res", 500)
                    .Where(item => item.ResType == NewWaveResourceType)
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray());
            foreach (var match in matches)
            {
                var item = new ListViewItem(match.Name) { Tag = match.Name };
                item.SubItems.Add(FormatSize(match.OriginalSize));
                _banks.Items.Add(item);
            }
            _bankDetails.Text = matches.Length == 0
                ? "No NewWave banks matched this search."
                : $"{matches.Length} NewWave bank(s) loaded from the installed game.";
        }
        catch (Exception ex)
        {
            _bankDetails.Text = "Audio bank search failed: " + ex.Message;
        }
        finally { search.Enabled = true; }
    }

    private void SelectBank()
    {
        _selectedBank = _banks.SelectedItems.Count == 0
            ? null : _banks.SelectedItems[0].Tag as string;
        _extractedPath = null;
        _dataSets.Items.Clear();
        _inspect.Enabled = _selectedBank != null;
        _export.Enabled = false;
        if (_selectedBank != null)
            _bankDetails.Text = _selectedBank;
    }

    private async Task InspectBankAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedBank)) return;
        _inspect.Enabled = false;
        _bankDetails.Text = "Parsing NewWave bank structure...";
        try
        {
            var bankName = _selectedBank;
            var bank = await Task.Run(() =>
                Services.FrostbiteAssets.InspectNewWaveBank(bankName));
            if (bank == null)
                throw new InvalidDataException("The selected RES is not a supported NewWave bank.");
            if (bankName != _selectedBank) return;  // user switched banks mid-inspection
            _extractedPath = bank.ExtractedPath;
            _dataSets.Items.Clear();
            foreach (var dataSet in bank.DataSets)
            {
                var item = new ListViewItem(dataSet.Name);
                item.SubItems.Add(dataSet.RowCount.ToString("N0"));
                item.SubItems.Add(dataSet.FieldCount.ToString("N0"));
                item.SubItems.Add(dataSet.IndexCount.ToString("N0"));
                item.SubItems.Add(dataSet.SampleGroupId.ToString());
                _dataSets.Items.Add(item);
            }
            _bankDetails.Text =
                $"{bank.Name}\r\n{bank.Endian} · alignment {bank.Alignment} · " +
                $"bank {bank.BankKey} · project {bank.ProjectKey}\r\n" +
                "Metadata only; raw .res export is not playable in CM26.";
            _export.Enabled = File.Exists(_extractedPath);
        }
        catch (Exception ex)
        {
            _bankDetails.Text = "Audio bank inspection failed: " + ex.Message;
        }
        finally { _inspect.Enabled = _selectedBank != null; }
    }

    private void ExportBank()
    {
        if (string.IsNullOrWhiteSpace(_extractedPath) || !File.Exists(_extractedPath)) return;
        using var dialog = new SaveFileDialog
        {
            Title = "Export NewWave Bank",
            Filter = "NewWave bank (*.res)|*.res|All files (*.*)|*.*",
            FileName = Path.GetFileNameWithoutExtension(_selectedBank) + ".res"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            File.Copy(_extractedPath, dialog.FileName, overwrite: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Export NewWave Bank",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        _bankDetails.Text = $"Exported to {dialog.FileName}";
    }

    private void PreviewLocalAudio()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Play Local Audio File",
            Filter = "Audio files (*.wav;*.mp3;*.wma;*.m4a)|*.wav;*.mp3;*.wma;*.m4a|All files (*.*)|*.*",
            CheckFileExists = true,
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            AudioPreviewService.Play(dialog.FileName);
            _bankDetails.Text = $"Playing preview\r\n{dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Audio Preview", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    internal static string FormatSize(uint bytes) =>
        bytes >= 1024 * 1024 ? (bytes / 1024d / 1024d).ToString("N1", System.Globalization.CultureInfo.InvariantCulture) + " MB" :
        bytes >= 1024 ? (bytes / 1024d).ToString("N1", System.Globalization.CultureInfo.InvariantCulture) + " KB" :
        bytes.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " B";
}
// FC26's broadcastleague table maps broadcast presentation to team/league/nation
// IDs. It does not contain an overlay asset path, so label it accurately instead
// of implying that this is a complete scoreboard texture editor.
internal sealed class TvSection(AppServices s) : Fc26ExtensionSection(s, "scoreboard", "Broadcast Links", "broadcastleague", "Broadcast League Links", "artificialkey", "teamid", "leagueid", "nationid");
// These are FC26-only data sets, surfaced as explicit CM16-style forms rather than
// hidden behind a generic database grid.  The field order follows the game table.
internal sealed class AdboardsSection : Fc26ExtensionSection
{
    private readonly PictureBox _preview = new();
    private readonly Label _caption = new();

    public AdboardsSection(AppServices s)
        : base(s, "adboards", "Adboards", "modeadboardlinks", "Mode Adboards",
            "artificialkey", "modeid", "dynamicimageid", "adsponserid", "isapproved")
    {
        var page = AddCanvasTab("Preview");
        var canvas = (Panel)page.Controls[0];
        var box = Card("Dynamic Adboard Artwork", new Point(4, 4), new Size(850, 520));
        _preview.Location = new Point(12, 28);
        _preview.Size = new Size(825, 410);
        _preview.BackColor = CardLayout.CardFieldBg;
        _preview.BorderStyle = BorderStyle.FixedSingle;
        _preview.SizeMode = PictureBoxSizeMode.Zoom;
        box.Controls.Add(_preview);
        _caption.Location = new Point(12, 446);
        _caption.Size = new Size(825, 40);
        _caption.Font = LegacyFont;
        _caption.TextAlign = ContentAlignment.MiddleCenter;
        box.Controls.Add(_caption);
        LegacyAssetActions.Attach(Services, box, _preview, new Point(12, 492), () => OnRecordShown());
        canvas.Controls.Add(box);
    }

    protected override void OnRecordShown()
    {
        var sponsorId = Value("adsponserid");
        var dynamicImageId = Value("dynamicimageid");
        var candidates = new[]
        {
            $"data/ui/imgAssets/cmSponsors/cmSponsors{sponsorId}.dds",
            $"data/ui/imgAssets/cmSponsors/cmSponsors{dynamicImageId}.dds",
            $"data/ui/imgAssets/dynamicSponsors/dynamic_{dynamicImageId}.dds"
        };
        var staged = candidates.FirstOrDefault(path => Services.LegacyMods.GetReplacement(path) != null);
        LegacyAssetActions.SetTarget(_preview, new LegacyAssetEditTarget(staged ?? candidates[0], 1024, 256));
        FrostbitePreviewLoader.LoadLegacyUiAssetCandidates(
            _preview,
            Services,
            staged == null ? null : Services.LegacyMods.GetReplacement(staged),
            candidates,
            (image, source) =>
            {
                if (IsDisposed) { image?.Dispose(); return; }
                _preview.Image?.Dispose();
                _preview.Image = image;
                _caption.Text = image == null
                    ? $"DB record loaded: sponsor {sponsorId}, dynamic image {dynamicImageId}; installed artwork not found"
                    : $"Sponsor {sponsorId} · dynamic image {dynamicImageId} · {source}";
            },
            path => LegacyAssetActions.SetTarget(
                _preview, new LegacyAssetEditTarget(path, 1024, 256)));
    }

    private int Value(string field) =>
        CurrentValues.TryGetValue(field, out var value) && int.TryParse(value.RawValue, out var parsed) ? parsed : 0;
}
internal sealed class StadiumAudioSection(AppServices s) : Fc26ExtensionSection(s, "stadiumaudio", "Stadium Audio", "audiostadium", "Stadium Audio", "stadiumid", "stadiumpalanguageindex", "matchsizetypeindex");
