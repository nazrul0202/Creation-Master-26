using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Application settings (stored locally, not in the database).</summary>
public sealed class SettingsSection : SectionBase
{
    private readonly TextBox _assetBox;
    private readonly TextBox _gameFolderBox;
    private readonly Label _frostbiteStatus;
    private readonly Label _logLabel;
    private bool _gameFolderLoading;

    public override string SectionKey => "settings";
    public override string SectionTitle => "Settings";
    protected override string TableName => "";
    protected override bool SinglePane => true;

    public SettingsSection(AppServices s) : base(s)
    {
        var panel = new BufferedPanel { Dock = DockStyle.Fill, Padding = new Padding(8), BackColor = Theme.Background, AutoScroll = true };

        var gameFolderLabel = new Label { Text = "Game folder (Frostbite Data / Patch)", Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Text, Font = Theme.Body, Padding = new Padding(0, 10, 0, 0) };
        var gameFolderRow = new BufferedPanel { Dock = DockStyle.Top, Height = 27, BackColor = Theme.Background };
        _gameFolderBox = new TextBox { Dock = DockStyle.Fill, Text = SettingsService.FC26GameFolder, BackColor = Theme.Input, ForeColor = Theme.Text, Font = Theme.Body };
        var gameBrowseBtn = new Button { Text = "Browse…", Dock = DockStyle.Right, Width = 84 };
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
        gameFolderRow.Controls.Add(_gameFolderBox);
        gameFolderRow.Controls.Add(gameBrowseBtn);

        _frostbiteStatus = new Label
        {
            Text = s.FrostbiteAssets.Status,
            Dock = DockStyle.Top, Height = 30, ForeColor = Theme.Text,
            Font = Theme.Body, AutoEllipsis = true,
        };

        var assetLabel = new Label { Text = "Asset pack folder (minifaces / balls / stadiums / boots / flags)", Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Text, Font = Theme.Body, Padding = new Padding(0, 10, 0, 0) };
        var assetRow = new BufferedPanel { Dock = DockStyle.Top, Height = 27, BackColor = Theme.Background };
        _assetBox = new TextBox { Dock = DockStyle.Fill, Text = SettingsService.AssetRoot, BackColor = Theme.Input, ForeColor = Theme.Text, Font = Theme.Body };
        var browseBtn = new Button { Text = "Browse…", Dock = DockStyle.Right, Width = 84 };
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
        assetRow.Controls.Add(_assetBox);
        assetRow.Controls.Add(browseBtn);

        var assetHint = new Label
        {
            Text = "Optional fallback preview pack. Installed assets are used whenever available.",
            Dock = DockStyle.Top, Height = 32, ForeColor = Theme.Muted, Font = Theme.Body, AutoEllipsis = true,
        };

        var scraperHint = new Label
        {
            Text = "Optional. The packaged copy under Tools\\CM26 Scraper and copies next to CM26 or at a drive root are detected automatically; this setting overrides that search.",
            Dock = DockStyle.Top, Height = 32, ForeColor = Theme.Muted, Font = Theme.Body, AutoEllipsis = true,
        };

        var scraperLabel = new Label { Text = "CM26 Scraper folder (Data Sync)", Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Text, Font = Theme.Body, Padding = new Padding(0, 10, 0, 0) };
        var scraperRow = new BufferedPanel { Dock = DockStyle.Top, Height = 27, BackColor = Theme.Background };
        var scraperBox = new TextBox { Dock = DockStyle.Fill, Text = SettingsService.ScraperRoot, BackColor = Theme.Input, ForeColor = Theme.Text, Font = Theme.Body };
        var scraperBrowse = new Button { Text = "Browse…", Dock = DockStyle.Right, Width = 84 };
        Theme.ApplyButton(scraperBrowse);
        void ApplyScraperRoot()
        {
            SettingsService.ScraperRoot = scraperBox.Text.Trim();
            scraperHint.Text = string.IsNullOrWhiteSpace(scraperBox.Text)
                ? "Optional. The packaged copy under Tools\\CM26 Scraper and copies next to CM26 or at a drive root are detected automatically; this setting overrides that search."
                : $"Scraper folder saved: {scraperBox.Text.Trim()}";
        }
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
        scraperRow.Controls.Add(scraperBox);
        scraperRow.Controls.Add(scraperBrowse);

        var backupRow = new BufferedPanel { Dock = DockStyle.Top, Height = 32, BackColor = Theme.Background };
        var compressBackup = new Button
        {
            Text = "Compress CmModData",
            Dock = DockStyle.Left,
            Width = 160,
        };
        Theme.ApplyButton(compressBackup);
        var compressionStatus = new Label
        {
            Text = "Optional transparent NTFS compression",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            AutoEllipsis = true,
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
            finally
            {
                compressBackup.Enabled = true;
            }
        };
        backupRow.Controls.Add(compressionStatus);
        backupRow.Controls.Add(compressBackup);
        var backupHint = new Label
        {
            Text = "Backup storage: compression is optional and does not replace backup validation.",
            Dock = DockStyle.Top,
            Height = 25,
            ForeColor = Theme.Muted,
        };

        var nameNote = new Label
        {
            Text = "Player names are resolved read-only from the loaded database folder (players → playernames → eng_us.DB). " +
                   "In the current database these names are protected by the publisher's text cipher (key not present), so the app " +
                   "shows an honest 'Player {id}' and never fabricates a name.",
            Dock = DockStyle.Top, Height = 46, ForeColor = Theme.Text, Font = Theme.Body, AutoEllipsis = true, Padding = new Padding(0, 8, 0, 0),
        };

        _logLabel = new Label
        {
            Text = $"Log file: {Program.LogPath}",
            Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Muted, Font = Theme.Body, Padding = new Padding(0, 8, 0, 0),
        };

        var about = new Label
        {
            Text = $"Creation Master 26 · Version {Program.ProductVersion}\nCM26_by_Rizco98.exe\n\n" +
                   "Save writes validated database and legacy changes directly to Data/Patch. " +
                   "File > Restore Original Data restores the immutable CmModData backup.",
            Dock = DockStyle.Top, Height = 110, ForeColor = Theme.Text, Font = Theme.Body, Padding = new Padding(0, 12, 0, 0),
        };

        panel.Controls.Add(about);
        panel.Controls.Add(_logLabel);
        panel.Controls.Add(nameNote);
        panel.Controls.Add(assetHint);
        panel.Controls.Add(assetRow);
        panel.Controls.Add(assetLabel);
        panel.Controls.Add(backupHint);
        panel.Controls.Add(scraperHint);
        panel.Controls.Add(scraperRow);
        panel.Controls.Add(scraperLabel);
        panel.Controls.Add(backupRow);
        panel.Controls.Add(_frostbiteStatus);
        panel.Controls.Add(gameFolderRow);
        panel.Controls.Add(gameFolderLabel);
        for (int i = 0; i < panel.Controls.Count; i++) panel.Controls[i].Dock = DockStyle.Top;

        Tabs.TabPages.Add(MakeTab("Settings", panel));
        Header.SetRecord("Settings", "Application preferences", IconService.Get("settings", 44));
    }

    private void ApplyAssetRoot()
    {
        var path = _assetBox.Text.Trim();
        SettingsService.AssetRoot = path;
        Services.RefreshAssetRoot();
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
