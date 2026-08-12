using CM26.App;
using System.Diagnostics;
using System.Windows.Forms;

namespace CM26.ModManager;

internal sealed class ManagerForm : Form
{
    private readonly ListView _mods = new() { Dock = DockStyle.Fill, View = View.Details, CheckBoxes = true, FullRowSelect = true };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleLeft };
    private readonly TextBox _gameFolder = new() { Dock = DockStyle.Fill };
    private bool _loading;
    public ManagerForm()
    {
        Text = "CM26 Mod Manager"; Width = 1000; Height = 610; StartPosition = FormStartPosition.CenterScreen;
        _mods.Columns.Add("Enabled", 80); _mods.Columns.Add("Mod", 360); _mods.Columns.Add("Payloads", 90); _mods.Columns.Add("Created", 180);
        _mods.ItemChecked += (_, e) => { if (!_loading && e.Item.Tag is CM26ModLibraryService.LibraryItem item) CM26ModLibraryService.SetEnabled(item.PackagePath, e.Item.Checked); };
        var import = new Button { Text = "Import Mod", Dock = DockStyle.Left, Width = 120 };
        var build = new Button { Text = "Build ModData", Dock = DockStyle.Left, Width = 130 };
        var launch = new Button { Text = "Launch with Mods", Dock = DockStyle.Left, Width = 145 };
        var restore = new Button { Text = "Restore Original", Dock = DockStyle.Left, Width = 130 };
        var refresh = new Button { Text = "Refresh", Dock = DockStyle.Left, Width = 100 };
        var browse = new Button { Text = "Browse...", Dock = DockStyle.Right, Width = 100 };
        import.Click += (_, _) => Import(); refresh.Click += (_, _) => Reload();
        build.Click += async (_, _) => await BuildAsync(build);
        launch.Click += async (_, _) => await LaunchAsync(launch);
        restore.Click += (_, _) => Restore();
        browse.Click += (_, _) => BrowseGameFolder();
        _gameFolder.Text = SettingsService.FC26GameFolder;
        var gameBar = new Panel { Dock = DockStyle.Top, Height = 34 };
        gameBar.Controls.Add(_gameFolder); gameBar.Controls.Add(browse);
        var gameLabel = new Label { Text = "FC26 folder:", Dock = DockStyle.Left, Width = 86, TextAlign = ContentAlignment.MiddleLeft };
        gameBar.Controls.Add(gameLabel);
        var bar = new Panel { Dock = DockStyle.Top, Height = 40 }; bar.Controls.Add(refresh); bar.Controls.Add(restore); bar.Controls.Add(launch); bar.Controls.Add(build); bar.Controls.Add(import);
        Controls.Add(_mods); Controls.Add(_status); Controls.Add(gameBar); Controls.Add(bar); Reload();
    }
    private void Import()
    {
        using var dialog = new OpenFileDialog { Filter = "CM26 Mod (*.cm26mod)|*.cm26mod" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { CM26ModLibraryService.Import(dialog.FileName); CM26ModLibraryService.WriteLog("Imported " + dialog.FileName); Reload(); } catch (Exception ex) { MessageBox.Show(ex.Message, Text); }
    }
    private void Reload()
    {
        _loading = true; _mods.Items.Clear();
        foreach (var item in CM26ModLibraryService.List())
        {
            var row = new ListViewItem(item.Enabled ? "Yes" : "No") { Checked = item.Enabled, Tag = item };
            row.SubItems.Add(item.Manifest.Name); row.SubItems.Add(item.Manifest.Payloads.Length.ToString()); row.SubItems.Add(item.Manifest.CreatedUtc.LocalDateTime.ToString("g")); _mods.Items.Add(row);
        }
        _loading = false; _status.Text = $"{_mods.Items.Count} mod(s), {_mods.CheckedItems.Count} enabled  |  Library: {CM26ModLibraryService.ModsRoot}";
    }

    private void BrowseGameFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select the FC26 folder containing Data and Patch" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _gameFolder.Text = dialog.SelectedPath;
        SettingsService.FC26GameFolder = dialog.SelectedPath;
    }

    private string? GameRoot()
    {
        var root = FrostbiteAssetSession.ResolveGameRoot(_gameFolder.Text);
        if (string.IsNullOrWhiteSpace(root))
        {
            MessageBox.Show(this, "Select a valid FC26 installation folder first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }
        SettingsService.FC26GameFolder = root;
        return root;
    }

    private async Task BuildAsync(Button button)
    {
        var root = GameRoot(); if (root == null) return;
        var packages = CM26ModLibraryService.EnabledPackages();
        if (packages.Count == 0) { MessageBox.Show(this, "Enable at least one CM26 mod first.", Text); return; }
        if (MessageBox.Show(this, "Build a lightweight CM26ModData symbolic-link overlay? Only modified CAS/TOC files are copied. Windows may require Administrator access or Developer Mode. Original FC26 files remain untouched.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        button.Enabled = false;
        try
        {
            var progress = new Progress<string>(value => _status.Text = value);
            var result = await Task.Run(() => CM26ModOverlayService.Build(root, packages, new FrostbiteAssetSession(), progress));
            CM26ModLibraryService.WriteLog("Build: " + result.Message);
            _status.Text = result.Message;
            MessageBox.Show(this, result.Message, Text, MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
        finally { button.Enabled = true; }
    }

    private async Task LaunchAsync(Button button)
    {
        var root = GameRoot(); if (root == null) return;
        if (MessageBox.Show(this, "Launch FC26 with CM26ModData? The installed Data/Patch folders will not be changed.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var activated = CM26ModLaunchService.Activate(root);
        CM26ModLibraryService.WriteLog("Activate: " + activated.Message);
        if (!activated.Success) { MessageBox.Show(this, activated.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
        try
        {
            using var game = Process.Start(new ProcessStartInfo(Path.Combine(root, "FC26.exe"))
            {
                UseShellExecute = true,
                WorkingDirectory = root,
                Arguments = "-dataPath CM26ModData"
            }) ?? throw new InvalidOperationException("FC26 did not start.");
            button.Enabled = false; _status.Text = "FC26 launched with -dataPath CM26ModData. Original Data/Patch remains untouched.";
            await game.WaitForExitAsync(); Restore(showMessage: false);
        }
        catch (Exception ex) { CM26ModLibraryService.WriteLog("Launch error: " + ex); MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { button.Enabled = true; }
    }

    private void Restore(bool showMessage = true)
    {
        var root = GameRoot(); if (root == null) return;
        var result = CM26ModLaunchService.Restore(root);
        CM26ModLibraryService.WriteLog("Restore: " + result.Message);
        _status.Text = result.Message;
        if (showMessage || !result.Success) MessageBox.Show(this, result.Message, Text, MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
    }
}
