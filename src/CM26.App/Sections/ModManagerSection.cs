using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Standalone CM26 mod library. It intentionally does not read or alter FET's library.</summary>
public sealed class ModManagerSection : SectionBase
{
    private readonly ListView _mods = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, CheckBoxes = true };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleLeft };
    private readonly StudioToolbar _toolbar;
    private bool _loading;

    public override string SectionKey => "modmanager";
    public override string SectionTitle => "CM26 Mod Manager";
    protected override string TableName => "";
    protected override bool SinglePane => true;
    protected override bool ShowRecordCommandStrip => false;

    public ModManagerSection(AppServices services) : base(services)
    {
        _toolbar = new StudioToolbar
        {
            Title = "CM26 Mod Manager",
            CanCreate = false,
            ShowFilter = false,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchBox.PlaceholderText = "Search mods…";
        _toolbar.SearchTextChanged += (_, _) => HighlightMod(_toolbar.SearchText);
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            HighlightMod(_toolbar.SearchText);
        };

        _mods.Columns.Add("Enabled", 72);
        _mods.Columns.Add("Mod", 280);
        _mods.Columns.Add("Payloads", 80);
        _mods.Columns.Add("Created", 160);
        _mods.BackColor = StudioColors.InputBackground;
        _mods.ForeColor = StudioColors.PrimaryText;
        _mods.ItemChecked += (_, e) =>
        {
            if (_loading || e.Item.Tag is not CM26ModLibraryService.LibraryItem item) return;
            CM26ModLibraryService.SetEnabled(item.PackagePath, e.Item.Checked);
            _status.Text = e.Item.Checked ? "Mod enabled for the next CM26ModData build." : "Mod disabled.";
        };
        var import = new Button { Text = "Import CM26 Mod...", Dock = DockStyle.Left, Width = 160 };
        var build = new Button { Text = "Build CM26ModData", Dock = DockStyle.Left, Width = 165 };
        var launch = new Button { Text = "Launch CM26 Mods", Dock = DockStyle.Left, Width = 155 };
        var launchFet = new Button { Text = "Launch FET Mods", Dock = DockStyle.Left, Width = 140 };
        var restore = new Button { Text = "Restore Original", Dock = DockStyle.Left, Width = 130 };
        var refresh = new Button { Text = "Refresh", Dock = DockStyle.Left, Width = 95 };
        Theme.ApplyButton(import, true); Theme.ApplyButton(build, true); Theme.ApplyButton(launch, true); Theme.ApplyButton(launchFet, true); Theme.ApplyButton(restore); Theme.ApplyButton(refresh);
        import.Click += (_, _) => Import();
        build.Click += async (_, _) => await BuildOverlayAsync(build);
        launch.Click += async (_, _) => await LaunchAsync(
            root: FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder), button: launch);
        launchFet.Click += async (_, _) => await LaunchFetAsync(
            root: FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder), button: launchFet);
        restore.Click += (_, _) => Restore(root: FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder));
        refresh.Click += (_, _) => RefreshLibrary();
        var actions = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = StudioColors.RaisedSurface };
        actions.Controls.Add(refresh); actions.Controls.Add(restore); actions.Controls.Add(launchFet); actions.Controls.Add(launch); actions.Controls.Add(build); actions.Controls.Add(import);
        var hint = new Label { Dock = DockStyle.Top, Height = 52, ForeColor = StudioColors.MutedText, BackColor = Color.Transparent,
            Text = "Library: " + CM26ModLibraryService.Root + "  |  Lightweight symbolic-link overlay; only changed CAS/TOC files are copied. Original Data/Patch remains untouched.",
            Padding = new Padding(0, 8, 0, 0) };

        var listCard = new StudioCard { Dock = DockStyle.Fill, BackColor = StudioColors.Surface };
        listCard.Controls.Add(_mods);
        listCard.Controls.Add(_status);

        var page = new TabPage("CM26 Mods") { BackColor = StudioColors.AppBackground, Padding = new Padding(StudioSpacing.Medium) };
        page.Controls.Add(listCard);
        page.Controls.Add(hint);
        page.Controls.Add(actions);
        page.Controls.Add(_toolbar);
        Tabs.TabPages.Add(page);
        RefreshLibrary();
    }

    public override void ActivateSection() { base.ActivateSection(); RefreshLibrary(); }
    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();
    protected override void ShowRecord(int recordIndex) { }

    private void HighlightMod(string query)
    {
        var term = query.Trim();
        if (string.IsNullOrWhiteSpace(term)) return;
        foreach (ListViewItem item in _mods.Items)
        {
            if (item.Text.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.SubItems.Cast<ListViewItem.ListViewSubItem>().Any(sub => sub.Text.Contains(term, StringComparison.OrdinalIgnoreCase)))
            {
                item.Selected = true;
                _mods.EnsureVisible(item.Index);
                return;
            }
        }
    }

    private void Import()
    {
        using var dialog = new OpenFileDialog { Filter = "CM26 Mod (*.cm26mod)|*.cm26mod", Multiselect = false };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { CM26ModLibraryService.Import(dialog.FileName); RefreshLibrary(); _status.Text = "CM26 mod imported."; }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Import CM26 Mod", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void RefreshLibrary()
    {
        _loading = true;
        try
        {
            _mods.Items.Clear();
            foreach (var item in CM26ModLibraryService.List())
            {
                var row = new ListViewItem(item.Enabled ? "Yes" : "No") { Checked = item.Enabled, Tag = item };
                row.SubItems.Add(item.Manifest.Name);
                row.SubItems.Add(item.Manifest.Payloads.Length.ToString());
                row.SubItems.Add(item.Manifest.CreatedUtc.LocalDateTime.ToString("g"));
                _mods.Items.Add(row);
            }
            _status.Text = $"{_mods.Items.Count} CM26 mod(s); {_mods.CheckedItems.Count} enabled.";
        }
        finally { _loading = false; }
    }

    private async Task BuildOverlayAsync(Button button)
    {
        var root = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
        var packages = CM26ModLibraryService.EnabledPackages();
        if (string.IsNullOrWhiteSpace(root) || packages.Count == 0)
        {
            MessageBox.Show(this, "Set the FC26 game folder and enable at least one CM26 mod first.",
                "Build CM26ModData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show(this,
                "CM26 will build a lightweight CM26ModData symbolic-link overlay. Only files that " +
                "CM26 changes are copied, so it does not duplicate the full game. Windows may require " +
                "Administrator access or Developer Mode to create links. FET folders will not be used. Continue?",
                "Build CM26ModData", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        button.Enabled = false; _status.Text = "Building lightweight CM26ModData symbolic-link overlay...";
        try
        {
            var progress = new Progress<string>(value => _status.Text = value);
            var result = await Task.Run(() => CM26ModOverlayService.Build(root, packages, Services.FrostbiteAssets, progress));
            _status.Text = result.Message;
            MessageBox.Show(this, result.Message, "CM26 Mod Manager", MessageBoxButtons.OK,
                result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
        finally { button.Enabled = true; }
    }

    private async Task LaunchAsync(string? root, Button button)
    {
        if (string.IsNullOrWhiteSpace(root)) return;
        if (MessageBox.Show(this, "Launch FC26 with CM26ModData? The installed Data/Patch folders will not be changed.",
                "Launch with CM26 Mods", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var result = CM26ModLaunchService.Activate(root);
        _status.Text = result.Message;
        if (!result.Success) { MessageBox.Show(this, result.Message, "CM26 Mod Manager", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
        try
        {
            button.Enabled = false;
            var launch = CM26ModLaunchService.Launch(root, "-dataPath CM26ModData");
            _status.Text = launch.Message;
            if (!launch.Success)
                MessageBox.Show(this, launch.Message, "Launch FC26", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                await Task.Delay(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Launch FC26", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { button.Enabled = true; }
    }

    private async Task LaunchFetAsync(string? root, Button button)
    {
        if (string.IsNullOrWhiteSpace(root)) return;
        if (MessageBox.Show(this, "Launch FC26 with the FET mod data folder (FIFAModData)? " +
                "FET's installed mods will be active; the installed Data/Patch folders will not be changed.",
                "Launch FET Mods", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var overlay = Path.Combine(root, "FIFAModData");
        if (!Directory.Exists(Path.Combine(overlay, "Data")) || !Directory.Exists(Path.Combine(overlay, "Patch")))
        {
            MessageBox.Show(this, "FIFAModData was not found in the game folder. Apply FET mods with FIFA Editor Tool first.",
                "Launch FET Mods", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            button.Enabled = false;
            var launch = CM26ModLaunchService.Launch(root, "-dataPath FIFAModData");
            _status.Text = launch.Message;
            if (!launch.Success)
                MessageBox.Show(this, launch.Message, "Launch FC26", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                await Task.Delay(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Launch FC26", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { button.Enabled = true; }
    }

    private void Restore(string? root)
    {
        if (string.IsNullOrWhiteSpace(root)) return;
        var result = CM26ModLaunchService.Restore(root);
        _status.Text = result.Message;
        MessageBox.Show(this, result.Message, "CM26 Mod Manager", MessageBoxButtons.OK,
            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
    }
}
