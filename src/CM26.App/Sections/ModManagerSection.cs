using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Standalone CM26 mod library. It intentionally does not read or alter FET's library.</summary>
public sealed class ModManagerSection : SectionBase
{
    private readonly ListView _mods = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, CheckBoxes = true };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleLeft };
    private bool _loading;

    public override string SectionKey => "modmanager";
    public override string SectionTitle => "CM26 Mod Manager";
    protected override string TableName => "";
    protected override bool SinglePane => true;
    protected override bool ShowRecordCommandStrip => false;

    public ModManagerSection(AppServices services) : base(services)
    {
        _mods.Columns.Add("Enabled", 72);
        _mods.Columns.Add("Mod", 280);
        _mods.Columns.Add("Payloads", 80);
        _mods.Columns.Add("Created", 160);
        _mods.BackColor = Theme.Input;
        _mods.ForeColor = Theme.Text;
        _mods.ItemChecked += (_, e) =>
        {
            if (_loading || e.Item.Tag is not CM26ModLibraryService.LibraryItem item) return;
            CM26ModLibraryService.SetEnabled(item.PackagePath, e.Item.Checked);
            _status.Text = e.Item.Checked ? "Mod enabled for the next CM26ModData build." : "Mod disabled.";
        };
        var import = new Button { Text = "Import CM26 Mod...", Dock = DockStyle.Left, Width = 160 };
        var refresh = new Button { Text = "Refresh", Dock = DockStyle.Left, Width = 95 };
        Theme.ApplyButton(import, true); Theme.ApplyButton(refresh);
        import.Click += (_, _) => Import();
        refresh.Click += (_, _) => RefreshLibrary();
        var actions = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Theme.Background };
        actions.Controls.Add(refresh); actions.Controls.Add(import);
        var hint = new Label { Dock = DockStyle.Top, Height = 52, ForeColor = Theme.Muted,
            Text = "CM26 mods are separate from FET. Enable packages here; Build & Launch will create CM26ModData without writing the original game.",
            Padding = new Padding(0, 8, 0, 0) };
        var page = new TabPage("CM26 Mods") { BackColor = Theme.Background, Padding = new Padding(8) };
        page.Controls.Add(_mods); page.Controls.Add(_status); page.Controls.Add(hint); page.Controls.Add(actions);
        Tabs.TabPages.Add(page);
        RefreshLibrary();
    }

    public override void ActivateSection() { base.ActivateSection(); RefreshLibrary(); }
    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();
    protected override void ShowRecord(int recordIndex) { }

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
}
