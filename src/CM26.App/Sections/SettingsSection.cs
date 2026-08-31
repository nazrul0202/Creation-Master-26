using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>Application settings (stored locally, not in the database).</summary>
public sealed class SettingsSection : SectionBase
{
    private TextBox _assetBox = null!;
    private TextBox _gameFolderBox = null!;
    private Label _frostbiteStatus = null!;
    private Label _logLabel = null!;
    private readonly BufferedPanel _canvas;
    private readonly StudioToolbar _toolbar;
    private bool _gameFolderLoading;

    public override string SectionKey => "settings";
    public override string SectionTitle => "Settings";
    protected override string TableName => "";
    protected override bool SinglePane => true;
    protected override bool ShowRecordCommandStrip => false;

    public SettingsSection(AppServices s) : base(s)
    {
        _canvas = new BufferedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(12),
            AutoScroll = true,
        };

        _toolbar = new StudioToolbar
        {
            Title = "Settings",
            CanCreate = false,
            ShowFilter = false,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchBox.PlaceholderText = "Find setting…";
        _toolbar.SearchTextChanged += (_, _) => HighlightSetting(_toolbar.SearchText);
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            HighlightSetting(_toolbar.SearchText);
        };

        var page = new TabPage("Settings") { BackColor = StudioColors.AppBackground };
        page.Controls.Add(_canvas);
        page.Controls.Add(_toolbar);
        Tabs.TabPages.Add(page);
        Header.SetRecord("Settings", "Application preferences", IconService.Get("settings", 44));

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = StudioColors.AppBackground,
            Padding = new Padding(0),
        };
        _canvas.Controls.Add(flow);
        _canvas.Resize += (_, _) => ReflowCards();

        flow.Controls.Add(BuildAppearanceCard());
        flow.Controls.Add(BuildGameDataCard());
        flow.Controls.Add(BuildAssetCard());
        flow.Controls.Add(BuildBackupCard());
        flow.Controls.Add(BuildAboutCard());
        ReflowCards();
    }

    private void ReflowCards()
    {
        // Keep every card at full canvas width (margins are simulated by the wrapper).
        foreach (Control control in _canvas.Controls)
        {
            if (control is not FlowLayoutPanel flow) continue;
            foreach (Control item in flow.Controls)
            {
                if (item is not Panel wrapper) continue;
                wrapper.Width = Math.Max(400, flow.ClientSize.Width - 24);
                foreach (Control inner in wrapper.Controls)
                {
                    if (inner is Label or TextBox or Button) continue;
                    inner.Width = Math.Max(0, wrapper.Width - 16);
                }
            }
        }
    }

    private void HighlightSetting(string query)
    {
        var term = query.Trim();
        if (string.IsNullOrWhiteSpace(term)) return;
        foreach (Control control in _canvas.Controls)
        {
            if (control is not FlowLayoutPanel flow) continue;
            foreach (Control wrapper in flow.Controls)
            {
                if (wrapper is not Panel panel || panel.Controls.Count == 0) continue;
                var card = panel.Controls[0];
                foreach (Control child in card.Controls)
                {
                    if (child is Label label && label.Text.Contains(term, StringComparison.OrdinalIgnoreCase))
                    {
                        _canvas.ScrollControlIntoView(wrapper);
                        return;
                    }
                }
            }
        }
    }

    private Panel BuildAppearanceCard()
    {
        var (wrapper, card) = StartCard(124);
        AddCardTitle(card, "Appearance");
        var darkMode = new CheckBox
        {
            Text = "Dark theme",
            Checked = Theme.IsDark,
            Location = new Point(16, 44),
            AutoSize = true,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };
        darkMode.CheckedChanged += (_, _) =>
        {
            SettingsService.DarkMode = darkMode.Checked;
            Theme.IsDark = darkMode.Checked;
            Services.NotifyThemeChanged();
        };
        card.Controls.Add(darkMode);
        card.Controls.Add(new Label
        {
            Text = "Light matches the FC26 franchise look; dark is easier on the eyes in low light.",
            Location = new Point(16, 74),
            Size = new Size(600, 32),
            Font = Theme.Body,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        });
        return wrapper;
    }

    private Panel BuildGameDataCard()
    {
        var (wrapper, card) = StartCard(182);
        AddCardTitle(card, "FC26 Game Data");
        var gameFolderLabel = new Label
        {
            Text = "Game folder (Frostbite Data / Patch)",
            Location = new Point(16, 42),
            Size = new Size(640, 18),
            Font = Theme.Label,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };
        _gameFolderBox = new TextBox
        {
            Location = new Point(16, 62),
            Size = new Size(640, 26),
            Text = SettingsService.FC26GameFolder,
            BackColor = Theme.Input,
            ForeColor = Theme.Text,
            Font = Theme.Body,
        };
        var gameBrowseBtn = new Button { Text = "Browse…", Location = new Point(666, 61), Width = 84, Height = 27 };
        Theme.ApplyButton(gameBrowseBtn);
        gameBrowseBtn.Click += async (_, _) =>
        {
            using var dlg = new FolderBrowserDialog { Description = "Select the EA SPORTS FC 26 installation folder", UseDescriptionForTitle = true };
            if (Directory.Exists(_gameFolderBox.Text)) dlg.SelectedPath = _gameFolderBox.Text;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _gameFolderBox.Text = dlg.SelectedPath;
                await ApplyGameFolderAsync();
            }
        };
        _gameFolderBox.Leave += async (_, _) => await ApplyGameFolderAsync();
        _frostbiteStatus = new Label
        {
            Text = Services.FrostbiteAssets.Status,
            Location = new Point(16, 96),
            Size = new Size(734, 22),
            ForeColor = StudioColors.PrimaryText,
            Font = Theme.Body,
            AutoEllipsis = true,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(gameFolderLabel);
        card.Controls.Add(_gameFolderBox);
        card.Controls.Add(gameBrowseBtn);
        card.Controls.Add(_frostbiteStatus);
        card.Controls.Add(new Label
        {
            Text = "Edits are written directly to Data/Patch. After a Steam/EA update, launch the game once before editing.",
            Location = new Point(16, 124),
            Size = new Size(734, 32),
            Font = Theme.Body,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        });
        return wrapper;
    }

    private Panel BuildAssetCard()
    {
        var (wrapper, card) = StartCard(238);
        AddCardTitle(card, "Preview Assets");
        var assetLabel = new Label
        {
            Text = "Asset pack folder (minifaces / balls / stadiums / boots / flags)",
            Location = new Point(16, 42),
            Size = new Size(640, 18),
            Font = Theme.Label,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };
        _assetBox = new TextBox
        {
            Location = new Point(16, 62),
            Size = new Size(640, 26),
            Text = SettingsService.AssetRoot,
            BackColor = Theme.Input,
            ForeColor = Theme.Text,
            Font = Theme.Body,
        };
        var browseBtn = new Button { Text = "Browse…", Location = new Point(666, 61), Width = 84, Height = 27 };
        Theme.ApplyButton(browseBtn);
        browseBtn.Click += (_, _) =>
        {
            using var dlg = new FolderBrowserDialog { Description = "Select the optional asset preview folder", UseDescriptionForTitle = true };
            if (!string.IsNullOrWhiteSpace(_assetBox.Text) && Directory.Exists(_assetBox.Text))
                dlg.SelectedPath = _assetBox.Text;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _assetBox.Text = dlg.SelectedPath;
                ApplyAssetRoot();
            }
        };
        _assetBox.Leave += (_, _) => ApplyAssetRoot();

        var scraperLabel = new Label
        {
            Text = "CM26 Scraper folder (Data Sync)",
            Location = new Point(16, 108),
            Size = new Size(640, 18),
            Font = Theme.Label,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };
        var scraperBox = new TextBox
        {
            Location = new Point(16, 128),
            Size = new Size(640, 26),
            Text = SettingsService.ScraperRoot,
            BackColor = Theme.Input,
            ForeColor = Theme.Text,
            Font = Theme.Body,
        };
        var scraperHint = new Label
        {
            Location = new Point(16, 164),
            Size = new Size(734, 34),
            Font = Theme.Body,
            ForeColor = StudioColors.MutedText,
            AutoEllipsis = true,
            BackColor = Color.Transparent,
        };
        void ApplyScraperRoot()
        {
            SettingsService.ScraperRoot = scraperBox.Text.Trim();
            scraperHint.Text = string.IsNullOrWhiteSpace(scraperBox.Text)
                ? "Optional. The packaged copy under Tools\\CM26 Scraper and copies next to CM26 or at a drive root are detected automatically; this setting overrides that search."
                : $"Scraper folder saved: {scraperBox.Text.Trim()}";
        }
        var scraperBrowse = new Button { Text = "Browse…", Location = new Point(666, 127), Width = 84, Height = 27 };
        Theme.ApplyButton(scraperBrowse);
        scraperBrowse.Click += (_, _) =>
        {
            using var dlg = new FolderBrowserDialog { Description = "Select the folder that contains CM26 Scraper.exe", UseDescriptionForTitle = true };
            if (!string.IsNullOrWhiteSpace(scraperBox.Text) && Directory.Exists(scraperBox.Text))
                dlg.SelectedPath = scraperBox.Text;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                scraperBox.Text = dlg.SelectedPath;
                ApplyScraperRoot();
            }
        };
        scraperBox.Leave += (_, _) => ApplyScraperRoot();

        card.Controls.Add(assetLabel);
        card.Controls.Add(_assetBox);
        card.Controls.Add(browseBtn);
        card.Controls.Add(scraperLabel);
        card.Controls.Add(scraperBox);
        card.Controls.Add(scraperBrowse);
        card.Controls.Add(scraperHint);
        return wrapper;
    }

    private Panel BuildBackupCard()
    {
        var (wrapper, card) = StartCard(200);
        AddCardTitle(card, "Original Data Backup");
        var refreshBackup = new Button
        {
            Text = "Refresh CmModData",
            Location = new Point(16, 44),
            Width = 160,
            Height = 30,
        };
        Theme.ApplyButton(refreshBackup);
        var compressBackup = new Button
        {
            Text = "Compress CmModData",
            Location = new Point(184, 44),
            Width = 160,
            Height = 30,
        };
        Theme.ApplyButton(compressBackup);
        var compressionStatus = new Label
        {
            Text = "Optional transparent NTFS compression",
            Location = new Point(16, 84),
            Size = new Size(734, 26),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };
        compressBackup.Click += async (_, _) =>
        {
            if (MessageBox.Show(this,
                    "Enable transparent NTFS compression for the verified CmModData backup?\n\n" +
                    "File contents remain unchanged. CAS archives are already compressed, so savings may be small.",
                    "Compress Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
                return;
            compressBackup.Enabled = false;
            compressionStatus.Text = "Compressing backup in the background…";
            try
            {
                var result = await Task.Run(() =>
                    GameBackupService.EnableNtfsCompression(_gameFolderBox.Text.Trim()));
                compressionStatus.Text = result.Message;
                compressionStatus.ForeColor = result.Success ? Theme.Success : Theme.Warning;
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    compressionStatus.Text = "Compression failed: " + ex.Message;
                    compressionStatus.ForeColor = Theme.Danger;
                }
            }
            finally
            {
                if (!IsDisposed) compressBackup.Enabled = true;
            }
        };
        refreshBackup.Click += async (_, _) =>
        {
            var gameRoot = _gameFolderBox.Text.Trim();
            var baseline = GameBackupService.InspectLiveBaseline(gameRoot);
            var prompt = baseline.IsMatch
                ? "CmModData already matches this FC26 installation. Refresh it anyway?\n\n" +
                  "The current snapshot will be archived, then a fresh snapshot will be created."
                : "Use this only after Steam/EA has finished updating or repairing FC26, FET mods are disabled, " +
                  "and FC26 has reached its main menu once without mods.\n\n" +
                  "CM26 will archive the existing CmModData folder and create a fresh snapshot. Continue?";
            if (MessageBox.Show(this, prompt, "Refresh CmModData", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            refreshBackup.Enabled = false;
            compressionStatus.Text = "Creating fresh CmModData snapshot…";
            try
            {
                var progress = new Progress<GameBackupService.RestoreProgress>(item =>
                {
                    var percent = item.TotalBytes > 0
                        ? (int)Math.Clamp(item.CompletedBytes * 100 / item.TotalBytes, 0, 100)
                        : item.Total <= 0 ? 0 : item.Completed * 100 / item.Total;
                    compressionStatus.Text = $"{item.Phase}: {percent}% – {item.CurrentFile}";
                });
                var result = await Task.Run(() =>
                    GameBackupService.RefreshAfterVanillaLaunch(gameRoot, progress));
                compressionStatus.Text = result.Message;
                compressionStatus.ForeColor = result.Success ? Theme.Success : Theme.Warning;
                MessageBox.Show(this, result.Message, result.Success ? "CmModData refreshed" : "Refresh failed",
                    MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                compressionStatus.Text = "Refresh failed: " + ex.Message;
                compressionStatus.ForeColor = Theme.Danger;
            }
            finally
            {
                if (!IsDisposed) refreshBackup.Enabled = true;
            }
        };
        card.Controls.Add(refreshBackup);
        card.Controls.Add(compressBackup);
        card.Controls.Add(compressionStatus);
        card.Controls.Add(new Label
        {
            Text = "After an FC26 update, launch vanilla once, then refresh CmModData. Compression is optional.",
            Location = new Point(16, 118),
            Size = new Size(734, 34),
            Font = Theme.Body,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        });
        return wrapper;
    }

    private Panel BuildAboutCard()
    {
        var (wrapper, card) = StartCard(196);
        AddCardTitle(card, "About");
        var about = new Label
        {
            Text = $"Creation Master 26 · Version {Program.ProductVersion}\n\n" +
                   "Save writes validated database and legacy changes directly to Data/Patch. " +
                   "File > Restore Original Data restores the immutable CmModData backup.\n\n" +
                   "Player names are resolved read-only from the loaded database folder. In the current database " +
                   "they are protected by the publisher's text cipher, so the app shows an honest \"Player {id}\" " +
                   "and never fabricates a name.",
            Location = new Point(16, 42),
            Size = new Size(734, 96),
            Font = Theme.Body,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };
        _logLabel = new Label
        {
            Text = $"Log file: {Program.LogPath}",
            Location = new Point(16, 148),
            Size = new Size(734, 22),
            Font = Theme.Mono,
            ForeColor = StudioColors.MutedText,
            AutoEllipsis = true,
            BackColor = Color.Transparent,
        };
        var aboutBtn = new Button { Text = "About…", Location = new Point(666, 144), Width = 84, Height = 27 };
        Theme.ApplyButton(aboutBtn);
        aboutBtn.Click += (_, _) => AboutDialog.Show(this);
        card.Controls.Add(about);
        card.Controls.Add(_logLabel);
        card.Controls.Add(aboutBtn);
        return wrapper;
    }

    /// <summary>Creates a dark Studio rounded card plus the transparent wrapper that owns its width.</summary>
    private (Panel Wrapper, StudioCard Card) StartCard(int height)
    {
        var wrapper = new Panel
        {
            Dock = DockStyle.None,
            Height = height + 12,
            BackColor = StudioColors.AppBackground,
            Margin = new Padding(0, 0, 0, 6),
        };
        var card = new StudioCard
        {
            Location = new Point(8, 6),
            Size = new Size(Math.Max(400, _canvas.ClientSize.Width - 40), height),
            BackColor = StudioColors.Surface,
            AccentColor = StudioColors.CyanAccent,
        };
        wrapper.Controls.Add(card);
        return (wrapper, card);
    }

    private static void AddCardTitle(Control card, string title)
    {
        card.Controls.Add(new Label
        {
            Text = title.ToUpperInvariant(),
            Location = new Point(16, 12),
            Size = new Size(700, 20),
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.CyanAccent,
            BackColor = Color.Transparent,
        });
    }

    private void ApplyAssetRoot()
    {
        var path = _assetBox.Text.Trim();
        SettingsService.AssetRoot = path;
        Services.RefreshAssetRoot();
    }

    public override void ActivateSection()
    {
        // Settings is a local workflow, not a database record list. Calling
        // SectionBase.LoadData would display its empty-record state over the
        // cards and made the entire page appear blank.
        Tabs.Visible = true;
        EmptyState.Visible = false;
        ReflowCards();
    }

    private async Task ApplyGameFolderAsync()
    {
        if (_gameFolderLoading) return;
        _gameFolderLoading = true;
        var path = _gameFolderBox.Text.Trim();
        SettingsService.FC26GameFolder = path;
        _frostbiteStatus.Text = "Indexing installed assets (first run may take about a minute)…";
        _frostbiteStatus.ForeColor = Theme.Warning;
        try
        {
            await Task.Run(() => Services.RefreshFrostbiteAssets());
            if (IsDisposed) return;
            _frostbiteStatus.Text = Services.FrostbiteAssets.Status;
            _frostbiteStatus.ForeColor = Services.FrostbiteAssets.IsAvailable
                ? Theme.Success : Theme.Warning;
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _frostbiteStatus.Text = ex.Message;
                _frostbiteStatus.ForeColor = Theme.Warning;
            }
        }
        finally
        {
            _gameFolderLoading = false;
        }
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();
    protected override void ShowRecord(int recordIndex) { }
}
