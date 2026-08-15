using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>Modern tool dashboard: database status, metrics, quick actions and health.</summary>
public sealed class DashboardSection : SectionBase
{
    private readonly Panel _host;
    private readonly StudioToolbar _toolbar;

    public override string SectionKey => "dashboard";
    public override string SectionTitle => "Dashboard";
    protected override string TableName => "";
    protected override bool SinglePane => true;

    public DashboardSection(AppServices s) : base(s)
    {
        _toolbar = new StudioToolbar
        {
            Title = "Dashboard",
            CanCreate = false,
            ShowFilter = false,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchClicked += (_, _) => Services.RequestNavigation("players");

        _host = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(StudioSpacing.Medium),
            AutoScroll = true,
        };

        var page = new TabPage("Overview") { BackColor = StudioColors.AppBackground };
        page.Controls.Add(_host);
        page.Controls.Add(_toolbar);
        Tabs.TabPages.Add(page);
        Header.Visible = false;
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();
    protected override void ShowRecord(int recordIndex) { }

    public override void ActivateSection()
    {
        RenderDashboard();
    }

    private void RenderDashboard()
    {
        _host.SuspendLayout();
        _host.Controls.Clear();

        if (!Services.Session.IsLoaded)
        {
            RenderEmptyState();
            _host.ResumeLayout();
            return;
        }

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = StudioColors.AppBackground,
        };

        var hero = BuildHeroCard();
        layout.Controls.Add(hero, 0, 0);

        var metrics = BuildMetricsRow();
        layout.Controls.Add(metrics, 0, 1);

        var actionsAndHealth = BuildActionsAndHealthRow();
        layout.Controls.Add(actionsAndHealth, 0, 2);

        _host.Controls.Add(layout);
        _host.ResumeLayout();
    }

    private Control BuildHeroCard()
    {
        var card = new StudioCard
        {
            Dock = DockStyle.Top,
            Height = 120,
            Margin = new Padding(0, 0, 0, StudioSpacing.Medium),
            AccentColor = StudioColors.Green,
        };

        var icon = new PictureBox
        {
            Image = IconService.Get("dashboard", 48),
            Size = new Size(48, 48),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(StudioSpacing.Large, StudioSpacing.Large),
            BackColor = Color.Transparent,
        };

        var folder = Services.Session.LoadedFolder ?? string.Empty;
        var title = new Label
        {
            Text = $"Database ready — {Path.GetFileName(folder)}",
            Location = new Point(80, StudioSpacing.Large),
            AutoSize = true,
            Font = StudioFonts.SectionTitle,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };

        var path = new Label
        {
            Text = ShortenPath(folder),
            Location = new Point(80, 52),
            AutoSize = true,
            Font = StudioFonts.CardSubtitle,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        var pending = new Label
        {
            Text = $"{Services.Pending.Count + Services.LegacyMods.Count} pending change(s)",
            Location = new Point(80, 76),
            AutoSize = true,
            Font = StudioFonts.DataValue,
            ForeColor = Services.Pending.HasChanges || Services.LegacyMods.HasChanges ? StudioColors.Yellow : StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        var openBtn = new Button
        {
            Text = "Open Folder",
            Size = new Size(110, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = StudioColors.RaisedSurface,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.Button,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        openBtn.FlatAppearance.BorderColor = StudioColors.CardBorder;
        var root = !string.IsNullOrWhiteSpace(Services.ActiveGameRoot)
            ? Services.ActiveGameRoot
            : SettingsService.FC26GameFolder;
        openBtn.Click += (_, _) =>
        {
            try
            {
                if (Directory.Exists(root)) Process.Start(new ProcessStartInfo("explorer.exe", $"\"{root}\"") { UseShellExecute = true });
            }
            catch (Exception ex) { Program.Log($"[CM26] Could not open game folder: {ex.Message}"); }
        };

        var saveBtn = new Button
        {
            Text = "Save",
            Size = new Size(110, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = StudioColors.Green,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.Button,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
            Enabled = Services.Pending.Count > 0 || Services.LegacyMods.HasChanges,
        };
        saveBtn.FlatAppearance.BorderColor = StudioColors.Green;
        saveBtn.Click += (_, _) => Services.RequestSaveDraft();

        var buttonsHost = new Panel
        {
            Height = 40,
            Dock = DockStyle.Right,
            Width = 248,
            BackColor = Color.Transparent,
            Padding = new Padding(0, StudioSpacing.Large, StudioSpacing.Large, 0),
        };
        openBtn.Location = new Point(0, 0);
        saveBtn.Location = new Point(120, 0);
        buttonsHost.Controls.Add(openBtn);
        buttonsHost.Controls.Add(saveBtn);

        card.Controls.Add(buttonsHost);
        card.Controls.Add(pending);
        card.Controls.Add(path);
        card.Controls.Add(title);
        card.Controls.Add(icon);

        return card;
    }

    private Control BuildMetricsRow()
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = StudioColors.AppBackground,
            Padding = Padding.Empty,
            Margin = new Padding(0, 0, 0, StudioSpacing.Medium),
        };

        flow.Controls.Add(Metric("Tables", CountOf("players") == "0" ? Services.Session.Tables.Count.ToString("N0") : CountOf("players"), StudioColors.CyanAccent, "browser"));
        flow.Controls.Add(Metric("Players", CountOf("players"), StudioColors.Green, "players"));
        flow.Controls.Add(Metric("Teams", CountOf("teams"), StudioColors.CyanAccent, "teams"));
        flow.Controls.Add(Metric("Leagues", CountOf("leagues"), StudioColors.Purple, "leagues"));
        flow.Controls.Add(Metric("Countries", CountOf("nations"), StudioColors.Yellow, "countries"));
        flow.Controls.Add(Metric("Stadiums", CountOf("stadiums"), StudioColors.CyanAccent, "stadiums"));
        flow.Controls.Add(Metric("Kits", CountOf("teamkits"), StudioColors.Purple, "kits"));
        flow.Controls.Add(Metric("Formations", CountOf("formations"), StudioColors.Red, "formations"));

        return flow;
    }

    private Control Metric(string label, string value, Color accent, string? navigateKey)
    {
        var card = new MetricCard
        {
            Width = 148,
            Height = 84,
            Margin = new Padding(0, 0, StudioSpacing.Medium, StudioSpacing.Medium),
            AccentColor = accent,
            LabelText = label,
            ValueText = value,
            ValueColor = accent,
        };
        if (navigateKey != null)
        {
            card.Cursor = Cursors.Hand;
            card.Click += (_, _) => Services.RequestNavigation(navigateKey);
        }
        return card;
    }

    private Control BuildActionsAndHealthRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = StudioColors.AppBackground,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 50f),
                new ColumnStyle(SizeType.Percent, 50f),
            },
        };

        row.Controls.Add(BuildQuickActionsCard(), 0, 0);
        row.Controls.Add(BuildHealthCard(), 1, 0);
        return row;
    }

    private Control BuildQuickActionsCard()
    {
        var card = new StudioCard
        {
            Dock = DockStyle.Fill,
            Height = 220,
            Margin = new Padding(0, 0, StudioSpacing.Medium, 0),
        };

        var title = new Label
        {
            Text = "Quick actions",
            Dock = DockStyle.Top,
            Height = 24,
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent,
            Padding = new Padding(0, StudioSpacing.Medium, 0, 0),
        };

        flow.Controls.Add(ActionButton("Open game database", StudioColors.CyanAccent, () => Services.RequestOpenGame()));
        flow.Controls.Add(ActionButton("Browse players", StudioColors.Green, () => Services.RequestNavigation("players")));
        flow.Controls.Add(ActionButton("Browse teams", StudioColors.CyanAccent, () => Services.RequestNavigation("teams")));
        flow.Controls.Add(ActionButton("Data sync", StudioColors.Purple, () => Services.RequestNavigation("transfers")));
        flow.Controls.Add(ActionButton("Mod manager", StudioColors.Yellow, () => Services.RequestNavigation("modmanager")));
        flow.Controls.Add(ActionButton("Validate database", StudioColors.Green, ValidateDatabase));
        flow.Controls.Add(ActionButton("Settings", StudioColors.MutedText, () => Services.RequestNavigation("settings")));

        card.Controls.Add(flow);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildHealthCard()
    {
        var card = new StudioCard
        {
            Dock = DockStyle.Fill,
            Height = 220,
            Margin = new Padding(StudioSpacing.Medium, 0, 0, 0),
        };

        var title = new Label
        {
            Text = "Database health",
            Dock = DockStyle.Top,
            Height = 24,
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, StudioSpacing.Medium, 0, 0),
        };

        flow.Controls.Add(HealthRow("Database status", Services.Session.IsLoaded ? "Loaded" : "Not loaded", Services.Session.IsLoaded ? StudioColors.Green : StudioColors.Red));
        flow.Controls.Add(HealthRow("Asset index", Services.FrostbiteAssets.IsAvailable ? "Indexed" : "Not available", Services.FrostbiteAssets.IsAvailable ? StudioColors.Green : StudioColors.MutedText));
        flow.Controls.Add(HealthRow("Pending changes", (Services.Pending.Count + Services.LegacyMods.Count).ToString("N0"), Services.Pending.HasChanges || Services.LegacyMods.HasChanges ? StudioColors.Yellow : StudioColors.Green));
        flow.Controls.Add(HealthRow("Backup status", GameBackupService.Inspect(Services.ActiveGameRoot).IsReady ? "Ready" : "Unknown", StudioColors.MutedText));
        flow.Controls.Add(HealthRow("Last opened", ShortenPath(SettingsService.LastFolder), StudioColors.MutedText));

        card.Controls.Add(flow);
        card.Controls.Add(title);
        return card;
    }

    private Control HealthRow(string label, string value, Color valueColor)
    {
        var panel = new Panel
        {
            Height = 26,
            Width = 340,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, StudioSpacing.Tiny),
        };

        panel.Controls.Add(new Label
        {
            Text = label,
            ForeColor = StudioColors.MutedText,
            Font = StudioFonts.RowPrimary,
            AutoSize = true,
            Location = new Point(0, 4),
            BackColor = Color.Transparent,
        });

        panel.Controls.Add(new Label
        {
            Text = value,
            ForeColor = valueColor,
            Font = StudioFonts.DataValue,
            AutoSize = true,
            Location = new Point(140, 4),
            BackColor = Color.Transparent,
        });

        return panel;
    }

    private Button ActionButton(string text, Color accent, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Width = 150,
            Height = 34,
            Margin = new Padding(0, 0, StudioSpacing.Medium, StudioSpacing.Medium),
            FlatStyle = FlatStyle.Flat,
            BackColor = StudioColors.RaisedSurface,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.Button,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        button.FlatAppearance.BorderColor = StudioColors.CardBorder;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, accent);
        button.Click += (_, _) => action();
        return button;
    }

    private void ValidateDatabase()
    {
        var issues = Services.Validation.ValidateAll(Services.Pending.Changes);
        if (issues.Count == 0)
            MessageBox.Show(this, "All staged changes are valid.", "Validate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        else
            MessageBox.Show(this, string.Join(Environment.NewLine, issues.Select(i => $"• {i.Table}[{i.Row}].{i.Field}: {i.Message}")),
                "Validation issues", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void RenderEmptyState()
    {
        var empty = new EmptyStateCard
        {
            Dock = DockStyle.Fill,
            IconText = "🎮",
            TitleText = "No game data loaded",
            DescriptionText = "Open an EA SPORTS FC 26 installation to start editing its database, teams, players and assets.",
            ActionText = "Open FC26",
        };
        empty.ActionClicked += (_, _) => Services.RequestOpenGame();
        _host.Controls.Add(empty);
    }

    private string CountOf(string table) => (Services.Session.GetTable(table)?.RowCount ?? 0).ToString("N0");

    private static string ShortenPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (parts.Count <= 3) return path;
        return "…" + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar, parts.TakeLast(3));
    }
}
