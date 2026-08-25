using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>
/// Classic-shell front end for the indexed FC26 Frostbite catalog and verified
/// ChunkFileCollector logical paths. Imports stay staged until File > Save.
/// </summary>
internal sealed class Fc26AssetManagerForm : Form
{
    private const string TextureResType = "6BDE20BA";
    private readonly ComboBox _family = new ComboBox();
    private readonly TextBox _id = new TextBox();
    private readonly ComboBox _logicalPath = new ComboBox();
    private readonly ComboBox _savedPaths = new ComboBox();
    private readonly Label _assetState = new Label();
    private readonly PictureBox _legacyPreview = PreviewBox();
    private readonly TextBox _query = new TextBox();
    private readonly ComboBox _type = new ComboBox();
    private readonly DataGridView _catalog = new DataGridView();
    private readonly PictureBox _catalogPreview = PreviewBox();
    private readonly Label _status = new Label();
    private string _legacySource = string.Empty;
    private string _catalogSource = string.Empty;
    private readonly HashSet<string> _favourites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _recent = new List<string>();
    private readonly string _libraryStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "asset-library.txt");

    private static readonly LegacyFamily[] Families =
    {
        new LegacyFamily("Player miniface", new[] { "data/ui/imgassets/heads/p{id}.dds" }),
        new LegacyFamily("Team crest", new[]
        {
            "data/ui/imgassets/crest16x16/light/l{id}.dds",
            "data/ui/imgassets/crest32x32/light/l{id}.dds",
            "data/ui/imgassets/crest50x50/light/l{id}.dds",
            "data/ui/imgassets/crest/light/l{id}.dds"
        }),
        new LegacyFamily("Country flag", new[]
        {
            "data/ui/imgassets/flags512x512/f_{id}.dds",
            "data/ui/artassets/countryflags/f_{id}.big",
            "data/ui/artassets/miniflags/flag_{id}.big"
        }),
        new LegacyFamily("Stadium image", new[]
        {
            "data/ui/imgassets/stadium/stadium_{id}_0.dds",
            "data/ui/imgassets/clubinfo/stadium/st_{id}.dds"
        }),
        new LegacyFamily("Ball image", new[] { "data/ui/imgassets/settingsimg/ball_{id}.dds" }),
        new LegacyFamily("Player face / head", new[]
        {
            "data/sceneassets/heads/head_{id}_0.rx3",
            "data/sceneassets/faces/face_{id}_0_0_0_0_0_0_0_0_textures.rx3"
        }),
        new LegacyFamily("Player hair", new[]
        {
            "data/sceneassets/hair/hair_{id}_0_0.rx3",
            "data/sceneassets/hairlod/hairlod_{id}_0_0.rx3",
            "data/sceneassets/hair/hair_{id}_0_textures.rx3"
        }),
        new LegacyFamily("Player eyes / skin / tattoo", new[]
        {
            "data/sceneassets/heads/eyes_{id}_0_textures.rx3",
            "data/sceneassets/body/playerskin_{id}_textures.rx3",
            "data/sceneassets/tattoo/tattoo_{id}_0.rx3"
        }),
        new LegacyFamily("Ball model / texture", new[]
        {
            "data/sceneassets/ball/ball_{id}.rx3",
            "data/sceneassets/ball/ball_{id}_textures.rx3"
        }),
        new LegacyFamily("Boot model (brand ID)", new[] { "data/sceneassets/shoe/shoe_{id}.rx3" }),
        new LegacyFamily("Boot texture (brand_design)", new[] { "data/sceneassets/shoe/shoe_{id}_textures.rx3" }),
        new LegacyFamily("Goalkeeper gloves", new[]
        {
            "data/sceneassets/gkglove/gkglove_{id}.rx3",
            "data/sceneassets/gkglove/gkglove_{id}_textures.rx3"
        }),
        new LegacyFamily("Stadium model (ID)", new[] { "data/sceneassets/stadium/stadium_{id}.rx3" }),
        new LegacyFamily("Stadium texture (ID_time)", new[] { "data/sceneassets/stadium/stadium_{id}_textures.rx3" }),
        new LegacyFamily("Kit (team_type_year)", new[] { "data/sceneassets/kit/kit_{id}.rx3" }),
        new LegacyFamily("Competition / presentation graphic", new[]
        {
            "data/ui/imgassets/compbadges/comp_{id}.dds",
            "data/ui/imgassets/scoreboards/scoreboard_{id}.dds",
            "data/ui/imgassets/adboards/adboard_{id}.dds"
        }),
        new LegacyFamily("Kit fonts / numbers", new[]
        {
            "data/sceneassets/kitnumbers/number_{id}.rx3",
            "data/sceneassets/kitfonts/font_{id}.rx3"
        }),
        new LegacyFamily("Custom verified path", new[] { "data/ui/imgassets/" })
    };

    internal Fc26AssetManagerForm(string initialFamily = null)
    {
        Text = "FC26 Visual Asset Manager";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1180, 760);
        MinimumSize = new Size(900, 600);
        Icon = Form.ActiveForm?.Icon;
        LoadLibraryState();

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateDirectPage());
        tabs.TabPages.Add(CreateCatalogPage());
        _status.Dock = DockStyle.Bottom;
        _status.Height = 24;
        _status.Padding = new Padding(6, 4, 0, 0);
        _status.Text = "Asset changes are staged. File > Save validates, backs up and commits them directly to FC26.";
        Controls.Add(tabs);
        Controls.Add(_status);
        if (!string.IsNullOrWhiteSpace(initialFamily))
        {
            for (var i = 0; i < _family.Items.Count; i++)
            {
                if (_family.Items[i].ToString().IndexOf(initialFamily, StringComparison.OrdinalIgnoreCase) < 0) continue;
                _family.SelectedIndex = i;
                break;
            }
        }
    }

    private TabPage CreateDirectPage()
    {
        var page = new TabPage("Direct Asset Editing");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = true };
        _family.DropDownStyle = ComboBoxStyle.DropDownList;
        _family.Width = 170;
        _family.Items.AddRange(Families.Cast<object>().ToArray());
        _family.SelectedIndexChanged += (_, _) => RebuildPaths();
        _id.Width = 120;
        _id.Text = "0";
        _id.TextChanged += (_, _) => RebuildPaths();
        _logicalPath.Width = 500; _logicalPath.DropDownStyle = ComboBoxStyle.DropDown;
        _savedPaths.Width = 330; _savedPaths.DropDownStyle = ComboBoxStyle.DropDownList;
        _savedPaths.SelectedIndexChanged += (_, _) => SelectSavedPath();
        _assetState.AutoSize = true; _assetState.Padding = new Padding(8, 6, 0, 0);
        top.Controls.AddRange(new Control[]
        {
            new Label { Text = "Asset family", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, _family,
            new Label { Text = "ID / compound key", AutoSize = true, Padding = new Padding(8, 6, 0, 0) }, _id,
            new Label { Text = "Verified logical path", AutoSize = true, Padding = new Padding(8, 6, 0, 0) }, _logicalPath,
            Button("Load / Preview", LoadLegacy), Button("Import image", ImportLegacy), Button("Import native file", ImportFile),
            Button("Export", ExportLegacy), Button("Remove staged", RemoveLegacy), Button("Check family", CheckFamily),
            Button("Favourite", ToggleFavourite), new Label { Text = "Favourites / recent", AutoSize = true, Padding = new Padding(8, 6, 0, 0) },
            _savedPaths, _assetState
        });
        var note = new Label
        {
            Dock = DockStyle.Top, Height = 48, Padding = new Padding(10, 4, 10, 4),
            Text = "Only game-native FC26 logical paths are written. Import creates a pending DDS replacement; nothing touches Data/Patch until the main File > Save command."
        };
        page.Controls.Add(_legacyPreview);
        page.Controls.Add(note);
        page.Controls.Add(top);
        _family.SelectedIndex = 0;
        RefreshSavedPaths();
        return page;
    }

    private TabPage CreateCatalogPage()
    {
        var page = new TabPage("Frostbite Catalog Browser");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
        _query.Width = 360;
        _type.DropDownStyle = ComboBoxStyle.DropDownList;
        _type.Items.AddRange(new object[] { "All", "Res", "Ebx", "Chunk" });
        _type.SelectedIndex = 1;
        top.Controls.AddRange(new Control[]
        {
            new Label { Text = "Name contains", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, _query,
            new Label { Text = "Type", AutoSize = true, Padding = new Padding(8, 6, 0, 0) }, _type,
            Button("Search", SearchCatalog), Button("Preview texture", PreviewCatalog),
            Button("Export selected", ExportCatalog)
        });
        _query.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { SearchCatalog(null, EventArgs.Empty); e.SuppressKeyPress = true; } };

        _catalog.Dock = DockStyle.Fill;
        _catalog.ReadOnly = true;
        _catalog.AllowUserToAddRows = false;
        _catalog.AllowUserToDeleteRows = false;
        _catalog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _catalog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _catalog.MultiSelect = false;
        _catalog.CellDoubleClick += (_, _) => PreviewCatalog(null, EventArgs.Empty);
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 760 };
        split.Panel1.Controls.Add(_catalog);
        split.Panel2.Controls.Add(_catalogPreview);
        page.Controls.Add(split);
        page.Controls.Add(top);
        return page;
    }

    private void RebuildPaths()
    {
        if (_family.SelectedItem is not LegacyFamily family) return;
        var id = (_id.Text ?? string.Empty).Trim();
        if (id.Length == 0 || id.Any(character => !char.IsDigit(character) && character != '_'))
        {
            _logicalPath.Items.Clear();
            return;
        }
        var current = _logicalPath.Text;
        _logicalPath.Items.Clear();
        foreach (var path in family.Paths.Select(path => path.Replace("{id}", id))) _logicalPath.Items.Add(path);
        if (family.Paths.Length == 1 && family.Name.StartsWith("Custom") && !string.IsNullOrWhiteSpace(current))
            _logicalPath.Text = current;
        else if (_logicalPath.Items.Count > 0) _logicalPath.SelectedIndex = 0;
    }

    private void LoadLegacy(object sender, EventArgs e)
    {
        Run("Loading FC26 asset...", () =>
        {
            var path = RequireLogicalPath();
            var exported = Fc26HostBridge.ExportAsset(path);
            if (string.IsNullOrWhiteSpace(exported) || !File.Exists(exported))
                throw new FileNotFoundException("The logical path was not found in this FC26 installation.", path);
            _legacySource = exported;
            if (!TryShowImage(_legacyPreview, exported)) _legacyPreview.Image = null;
            AddRecent(path);
            _assetState.Text = "Installed · " + UsageStatus();
            return "Loaded " + path;
        });
    }

    private void ImportFile(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "FC26 native assets|*.rx3;*.dds;*.big;*.bin|All files|*.*",
            Title = "Select a format-compatible FC26 native replacement"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run("Staging native FC26 asset...", () =>
        {
            var logicalPath = RequireLogicalPath();
            var targetExtension = Path.GetExtension(logicalPath);
            var sourceExtension = Path.GetExtension(dialog.FileName);
            if (!string.IsNullOrWhiteSpace(targetExtension) && !targetExtension.Equals(sourceExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Replacement extension must match the target asset (" + targetExtension + ").");
            Fc26HostBridge.StageFile(logicalPath, dialog.FileName);
            TryShowImage(_legacyPreview, dialog.FileName);
            AddRecent(logicalPath);
            _assetState.Text = "Staged replacement · " + UsageStatus();
            return "Native replacement staged for direct Save: " + logicalPath;
        });
    }

    private void ImportLegacy(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "Images|*.png;*.bmp;*.jpg;*.jpeg;*.dds|All files|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run("Staging asset replacement...", () =>
        {
            var path = RequireLogicalPath();
            int width = 0, height = 0;
            try { using var image = Image.FromFile(dialog.FileName); width = image.Width; height = image.Height; }
            catch when (Path.GetExtension(dialog.FileName).Equals(".dds", StringComparison.OrdinalIgnoreCase))
            {
                if (_legacyPreview.Image != null) { width = _legacyPreview.Image.Width; height = _legacyPreview.Image.Height; }
                else { width = 1; height = 1; }
            }
            Fc26HostBridge.StageImage(path, dialog.FileName, Math.Max(1, width), Math.Max(1, height));
            if (!Path.GetExtension(dialog.FileName).Equals(".dds", StringComparison.OrdinalIgnoreCase))
                ShowImage(_legacyPreview, dialog.FileName);
            AddRecent(path);
            _assetState.Text = "Staged replacement · " + UsageStatus();
            return "Replacement staged. Use File > Save to validate, back up and apply.";
        });
    }

    private void CheckFamily(object sender, EventArgs e)
    {
        Run("Checking selected FC26 asset family...", () =>
        {
            if (_family.SelectedItem is not LegacyFamily family) throw new InvalidOperationException("Select an asset family first.");
            var id = AssetKey();
            var paths = family.Paths.Select(path => path.Replace("{id}", id)).ToArray();
            var installed = paths.Where(path => !string.IsNullOrWhiteSpace(Fc26HostBridge.ExportAsset(path))).ToArray();
            var missing = paths.Except(installed, StringComparer.OrdinalIgnoreCase).ToArray();
            _assetState.Text = installed.Length + " installed / " + missing.Length + " missing · " + UsageStatus();
            return missing.Length == 0 ? "All known paths for this family are installed." :
                "Missing: " + string.Join(", ", missing.Select(Path.GetFileName));
        });
    }

    private string UsageStatus()
    {
        var idText = AssetKey().Split('_')[0];
        if (!int.TryParse(idText, out var id)) return "custom path";
        var family = (_family.SelectedItem as LegacyFamily)?.Name ?? string.Empty;
        if (family.StartsWith("Player", StringComparison.OrdinalIgnoreCase))
            return FifaLibrary.FifaEnvironment.Players.SearchId(id) == null ? "unlinked ID" : "used by player";
        if (family.StartsWith("Team", StringComparison.OrdinalIgnoreCase))
            return FifaLibrary.FifaEnvironment.Teams.SearchId(id) == null ? "unlinked ID" : "used by team";
        if (family.StartsWith("Country", StringComparison.OrdinalIgnoreCase))
            return FifaLibrary.FifaEnvironment.Countries.SearchId(id) == null ? "unlinked ID" : "used by country";
        if (family.StartsWith("Stadium", StringComparison.OrdinalIgnoreCase))
            return FifaLibrary.FifaEnvironment.Stadiums.SearchId(id) == null ? "unlinked ID" : "used by stadium";
        if (family.StartsWith("Ball", StringComparison.OrdinalIgnoreCase))
            return FifaLibrary.FifaEnvironment.Balls.SearchId(id) == null ? "unlinked ID" : "used by ball";
        if (family.StartsWith("Boot", StringComparison.OrdinalIgnoreCase))
            return FifaLibrary.FifaEnvironment.Shoes.SearchId(id) == null ? "unlinked ID" : "used by boot record";
        return "usage requires dependency scan";
    }

    private string AssetKey()
    {
        var value = (_id.Text ?? string.Empty).Trim();
        if (value.Length == 0 || value.Any(character => !char.IsDigit(character) && character != '_'))
            throw new InvalidOperationException("Use a numeric ID or an underscore-separated compound key, for example 11_0_0.");
        return value;
    }

    private void ToggleFavourite(object sender, EventArgs e)
    {
        try
        {
            var path = RequireLogicalPath();
            if (!_favourites.Add(path)) _favourites.Remove(path);
            SaveLibraryState(); RefreshSavedPaths();
            _status.Text = _favourites.Contains(path) ? "Added to favourites: " + path : "Removed from favourites: " + path;
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void AddRecent(string path)
    {
        _recent.RemoveAll(value => value.Equals(path, StringComparison.OrdinalIgnoreCase));
        _recent.Insert(0, path);
        if (_recent.Count > 20) _recent.RemoveRange(20, _recent.Count - 20);
        SaveLibraryState(); RefreshSavedPaths();
    }

    private void SelectSavedPath()
    {
        var item = _savedPaths.SelectedItem as SavedPath;
        if (item != null) _logicalPath.Text = item.Path;
    }

    private void RefreshSavedPaths()
    {
        _savedPaths.BeginUpdate(); _savedPaths.Items.Clear();
        foreach (var path in _favourites.OrderBy(value => value)) _savedPaths.Items.Add(new SavedPath("★", path));
        foreach (var path in _recent.Where(path => !_favourites.Contains(path))) _savedPaths.Items.Add(new SavedPath("Recent", path));
        _savedPaths.EndUpdate();
    }

    private void LoadLibraryState()
    {
        try
        {
            if (!File.Exists(_libraryStatePath)) return;
            foreach (var line in File.ReadAllLines(_libraryStatePath))
            {
                if (line.StartsWith("F|", StringComparison.Ordinal)) _favourites.Add(line.Substring(2));
                else if (line.StartsWith("R|", StringComparison.Ordinal) && _recent.Count < 20) _recent.Add(line.Substring(2));
            }
        }
        catch { }
    }

    private void SaveLibraryState()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_libraryStatePath));
            File.WriteAllLines(_libraryStatePath, _favourites.Select(path => "F|" + path).Concat(_recent.Select(path => "R|" + path)));
        }
        catch { }
    }

    private void ExportLegacy(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_legacySource) || !File.Exists(_legacySource)) LoadLegacy(sender, e);
        if (string.IsNullOrWhiteSpace(_legacySource) || !File.Exists(_legacySource)) return;
        SaveCopy(_legacySource);
    }

    private void RemoveLegacy(object sender, EventArgs e) => Run("Removing staged replacement...", () =>
        Fc26HostBridge.RemoveStagedAsset(RequireLogicalPath()) + ". The installed FC26 asset was not deleted.");

    private void SearchCatalog(object sender, EventArgs e)
    {
        if (_query.Text.Trim().Length < 2)
        {
            MessageBox.Show(this, "Enter at least two characters.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Run("Searching the indexed FC26 catalog...", () =>
        {
            var rows = Fc26HostBridge.SearchIndexedAssets(_query.Text.Trim(), _type.Text);
            _catalog.DataSource = rows.Select(item => new
            {
                item.Type, item.Name, item.ResType, item.OriginalSize, item.CompressedSize, item.Sha1
            }).ToArray();
            if (_catalog.Columns.Contains("Name")) _catalog.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            return rows.Length + " indexed asset(s) found.";
        });
    }

    private Fc26HostBridge.IndexedAsset SelectedAsset()
    {
        if (_catalog.CurrentRow?.DataBoundItem == null) throw new InvalidOperationException("Select an asset first.");
        var row = _catalog.CurrentRow;
        return new Fc26HostBridge.IndexedAsset(Convert.ToString(row.Cells["Type"].Value),
            Convert.ToString(row.Cells["Name"].Value), Convert.ToString(row.Cells["ResType"].Value),
            Convert.ToString(row.Cells["OriginalSize"].Value), Convert.ToString(row.Cells["CompressedSize"].Value),
            Convert.ToString(row.Cells["Sha1"].Value));
    }

    private void PreviewCatalog(object sender, EventArgs e) => Run("Exporting FC26 texture preview...", () =>
    {
        var asset = SelectedAsset();
        if (asset.Type != "Res" || !asset.ResType.Equals(TextureResType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The selected resource is not a supported FC26 texture.");
        _catalogSource = Fc26HostBridge.ExportIndexedTexture(asset.Name);
        ShowImage(_catalogPreview, _catalogSource);
        return "Texture preview loaded.";
    });

    private void ExportCatalog(object sender, EventArgs e)
    {
        Run("Exporting selected FC26 texture...", () =>
        {
            var asset = SelectedAsset();
            if (asset.Type != "Res" || !asset.ResType.Equals(TextureResType, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only supported indexed textures can be exported from this browser.");
            _catalogSource = Fc26HostBridge.ExportIndexedTexture(asset.Name);
            SaveCopy(_catalogSource);
            return "Texture export completed.";
        });
    }

    private string RequireLogicalPath()
    {
        var path = _logicalPath.Text.Trim().Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Enter a verified FC26 logical path beginning with data/.");
        return path;
    }

    private void SaveCopy(string source)
    {
        using var dialog = new SaveFileDialog
        {
            FileName = Path.GetFileName(source), Filter = "FC26 asset files|*.dds;*.rx3;*.big;*.bin;*.png|All files|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        File.Copy(source, dialog.FileName, true);
        _status.Text = "Exported: " + dialog.FileName;
    }

    private void Run(string busyText, Func<string> action)
    {
        try
        {
            UseWaitCursor = true; _status.Text = busyText; Application.DoEvents();
            _status.Text = action();
        }
        catch (Exception ex)
        {
            _status.Text = "Operation failed.";
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    private static PictureBox PreviewBox() => new PictureBox
    {
        Dock = DockStyle.Fill, BackColor = Color.FromArgb(38, 38, 38),
        SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle
    };

    private static Button Button(string text, EventHandler click)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += click;
        return button;
    }

    private static void ShowImage(PictureBox preview, string path)
    {
        using var loaded = Image.FromFile(path);
        var clone = new Bitmap(loaded);
        var old = preview.Image;
        preview.Image = clone;
        old?.Dispose();
    }

    private static bool TryShowImage(PictureBox preview, string path)
    {
        try { ShowImage(preview, path); return true; }
        catch { return false; }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _legacyPreview.Image?.Dispose(); _catalogPreview.Image?.Dispose(); }
        base.Dispose(disposing);
    }

    private sealed class LegacyFamily
    {
        internal LegacyFamily(string name, string[] paths) { Name = name; Paths = paths; }
        internal string Name { get; }
        internal string[] Paths { get; }
        public override string ToString() => Name;
    }
    private sealed class SavedPath
    {
        internal SavedPath(string kind, string path) { Kind = kind; Path = path; }
        internal string Kind { get; } internal string Path { get; }
        public override string ToString() => Kind + " · " + Path;
    }
}
