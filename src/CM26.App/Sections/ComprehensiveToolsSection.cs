using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>
/// Public entry point for the complete CM26 workflow. Every numbered area from
/// the accepted feature specification has one stable route into an operational
/// editor or tool; features are no longer hidden behind the classic shell.
/// </summary>
public sealed class ComprehensiveToolsSection : SectionBase
{
    private sealed record Module(int Number, string Group, string Title, string Route, string Summary);

    private static readonly Module[] Modules =
    [
        new(1, "A. Database & Project Core", "Project Launcher", "$open-game", "Installed FC26, extracted databases and recent sources"),
        new(2, "A. Database & Project Core", "Direct Frostbite Workflow", "$direct-save", "Stage, validate, backup, direct save and restore"),
        new(3, "A. Database & Project Core", "Advanced Database Workspace", "browser", "All tables, filters, bulk edit, compare and import/export"),
        new(4, "A. Database & Project Core", "Dependency-Aware Editing", "browser", "Impact preview, linked names and transactional reference repair"),
        new(5, "B. Player Management", "Complete Player Editor", "players", "Create, clone, delete, import, attributes, roles and appearance"),
        new(6, "B. Player Management", "Player ID & Names Manager", "players", "Available IDs, linked names, swaps and reference updates"),
        new(7, "B. Player Management", "Transfermarkt Import", "transfers", "URL/scraper import with editable preview and audit source"),
        new(8, "B. Player Management", "Appearance Assistant", "players", "Image analysis, confidence, alternatives and overrides"),
        new(9, "B. Player Management", "Miniface & Face Tools", "players", "Miniface, face, hair, cranium and missing-asset tools"),
        new(10, "C. Transfer, Loan & Roster", "Complete Transfer System", "teams", "Transfer, loan, contract, shirt, lineup and rollback workflow"),
        new(11, "C. Transfer, Loan & Roster", "Roster Manager", "teams", "Squads, loans, free agents, duplicates and roster repair"),
        new(12, "D. Formation & Team Sheets", "Formation Workspace", "formations", "Starting XI, bench, reserves, roles and set pieces"),
        new(13, "D. Formation & Team Sheets", "Formation Repair & Validation", "$health", "Teamsheet, mentality, lineup and role validation"),
        new(14, "E. Team, Club & League", "Complete Team Editor", "teams", "Identity, league, stadium, kits, manager and squads"),
        new(15, "E. Team, Club & League", "Club Details", "teams", "Profile, ratings, honours, location and career budget"),
        new(16, "E. Team, Club & League", "League Editor", "leagues", "Teams, levels, promotion, ball, presentation and validation"),
        new(17, "F. Countries & National Teams", "Country Editor", "countries", "Confederation, localisation, weather and national links"),
        new(18, "F. Countries & National Teams", "National-Team Manager", "countries", "National squads, targets, nationality and link repair"),
        new(19, "G. Tournament & Competition", "Competition Editor", "competitions", "Names, type, ball, trophy, presentation and rules"),
        new(20, "G. Tournament & Competition", "Tournament / Compdata Editor", "competitions", "Wizard, phases, sources, advancement and previews"),
        new(21, "G. Tournament & Competition", "Tournament Calendar", "competitions", "Dates, kick-off time, overlap and path preview"),
        new(22, "G. Tournament & Competition", "Compdata Validation", "competitions", "IDs, parents, phases, sources and schedule issues"),
        new(23, "H. Weather & Stadium", "Global Weather Editor", "countries", "Monthly profiles, presets, copy, fill and validation"),
        new(24, "H. Weather & Stadium", "Stadium Editor", "stadiums", "Assignment, pitch, nets, weather and previews"),
        new(25, "I. Kits & Visual Assets", "Complete Kit Folder Import", "kits", "Auto-detection, validation, preview and atomic import"),
        new(26, "I. Kits & Visual Assets", "Kit Export & Creation", "kits", "Create, clone, export, remap, fonts and minikits"),
        new(27, "I. Kits & Visual Assets", "Flags, Banners & Scarves", "adboards", "Generate, validate and import/export team presentation assets"),
        new(28, "I. Kits & Visual Assets", "Visual Asset Manager", "settings", "Packs, installed state, checksums and library location"),
        new(29, "I. Kits & Visual Assets", "Visual Asset Picker", "browser", "Searchable preview catalogue with used/missing state"),
        new(30, "J. Ball, Boots & Gloves", "Ball Editor", "balls", "Database, texture, assignment and 3D preview"),
        new(31, "J. Ball, Boots & Gloves", "Boots & GK Gloves", "boots", "Models, textures, player assignment and batch operations"),
        new(32, "K. TV, Scoreboards & Presentation", "Broadcast Editor", "scoreboard", "Broadcasts, overlays, scoreboards, packages and validation"),
        new(33, "L. Managers & Localization", "Manager Editor", "managers", "Identity, team assignment, appearance and templates"),
        new(34, "L. Managers & Localization", "Localization Editor", "browser", "Language records, missing keys and resolved entity names"),
        new(35, "M. Batch & Health Tools", "Batch Player Matrix", "players", "Grouped presets, calculated edits, preview and complete undo"),
        new(36, "M. Batch & Health Tools", "Database Health Centre", "$health", "Integrity report and confirmed transactional repairs"),
        new(37, "N. Career Save Module", "Career Save Editor", "teams", "Career budget and separate save-runtime data workflow"),
        new(38, "O. Utilities", "Modding Utilities", "diagnostics", "Hash, XML, dates, IDs, comparison, archives and diagnostics"),
    ];

    internal static IReadOnlyList<int> ModuleNumbers => Modules.Select(module => module.Number).ToArray();
    internal static IReadOnlyList<string> ModuleRoutes => Modules.Select(module => module.Route).ToArray();

    private readonly BufferedPanel _host;
    private readonly TextBox _search;

    public override string SectionKey => "toolhub";
    public override string SectionTitle => "Comprehensive Tools";
    protected override string TableName => string.Empty;
    protected override bool SinglePane => true;
    protected override bool ShowRecordCommandStrip => false;

    public ComprehensiveToolsSection(AppServices services) : base(services)
    {
        Header.Visible = false;
        Validation.Visible = false;
        _host = new BufferedPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(StudioSpacing.Medium),
        };
        _search = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 32,
            PlaceholderText = "Find any CM26 module or workflow…",
            Margin = new Padding(0, 0, 0, StudioSpacing.Small),
        };
        Theme.ApplyTextBox(_search);
        _search.TextChanged += (_, _) => Render();
        var page = new TabPage("All Tools") { BackColor = StudioColors.AppBackground };
        page.Controls.Add(_host);
        page.Controls.Add(_search);
        Tabs.TabPages.Add(page);
        Render();
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();
    protected override void ShowRecord(int recordIndex) { }
    public override void ActivateSection() => Render();

    private void Render()
    {
        var query = _search?.Text.Trim() ?? string.Empty;
        var visible = Modules.Where(module => string.IsNullOrWhiteSpace(query) ||
            module.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            module.Group.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            module.Summary.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();

        _host.SuspendLayout();
        _host.Controls.Clear();
        var y = StudioSpacing.Small;
        foreach (var group in visible.GroupBy(module => module.Group))
        {
            _host.Controls.Add(new Label
            {
                Text = group.Key,
                Location = new Point(StudioSpacing.Small, y),
                Size = new Size(1040, 25),
                Font = StudioFonts.SectionTitle,
                ForeColor = StudioColors.CyanAccent,
            });
            y += 31;
            foreach (var module in group)
            {
                var card = new StudioCard
                {
                    Location = new Point(StudioSpacing.Small, y),
                    Size = new Size(1080, 66),
                    AccentColor = module.Number <= 4 ? StudioColors.CyanAccent : StudioColors.Green,
                    Padding = new Padding(12),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                };
                card.Controls.Add(new Label
                {
                    Text = module.Number.ToString(),
                    Location = new Point(12, 15),
                    Size = new Size(38, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = StudioFonts.SectionTitle,
                    ForeColor = Color.White,
                    BackColor = card.AccentColor,
                });
                card.Controls.Add(new Label
                {
                    Text = module.Title,
                    Location = new Point(62, 9),
                    Size = new Size(360, 22),
                    Font = StudioFonts.CardTitle,
                    ForeColor = StudioColors.PrimaryText,
                });
                card.Controls.Add(new Label
                {
                    Text = module.Summary,
                    Location = new Point(62, 33),
                    Size = new Size(770, 20),
                    Font = Theme.Body,
                    ForeColor = StudioColors.MutedText,
                });
                var open = new Button { Text = "Open", Location = new Point(950, 18), Size = new Size(92, 30), Tag = module };
                Theme.ApplyButton(open);
                open.BackColor = StudioColors.Green;
                open.ForeColor = Color.Black;
                open.Click += OpenModule;
                card.Controls.Add(open);
                _host.Controls.Add(card);
                y += 74;
            }
            y += StudioSpacing.Medium;
        }
        _host.AutoScrollMinSize = new Size(0, y + StudioSpacing.Large);
        _host.ResumeLayout();
    }

    private void OpenModule(object? sender, EventArgs e)
    {
        if (sender is not Button { Tag: Module module }) return;
        if (module.Route == "$open-game") Services.RequestOpenGame();
        else if (module.Route == "$direct-save") Services.RequestSaveDraft();
        else Services.RequestNavigation(module.Route);
    }
}
