using System.Windows;
using System.Windows.Controls;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style KitForm: Find + kit list on the left, edit groups on the right
/// (Identity / Colors / Jersey / Shorts / Name / Numbers / Socks) matching the
/// group layout of Creation Master 16 KitForm.
/// </summary>
public partial class KitView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();
    private RecordListItem? _current;

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public KitView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        PickUp.SelectObject += LoadEditorFromPickUp;
        PickUp.FilterByList = new[] { "All", "by Name", "by Id" };
        PickUp.RefreshObject += LoadList;
        Loaded += (_, _) => LoadList();
    }

    private void LoadEditorFromPickUp(RecordListItem item) => LoadEditor(item);

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetKits();
        PickUp.ObjectList = _all;
        if (_all.Count > 0 && PickUp.SelectedIndex < 0) PickUp.SelectedIndex = 0;
    }

    private void LoadEditor(RecordListItem item)
    {
        _current = item;
        var fields = _vm.Session.Sections.GetFields("teamkits", item.RecordIndex, LabelMaps.Kits);

        PositionFields.ItemsSource = fields.Where(f => IsPosition(f.FieldName));
        JerseyFields.ItemsSource = fields.Where(f => !IsPosition(f.FieldName) && !IsName(f.FieldName));
        NameFields.ItemsSource = fields.Where(f => IsName(f.FieldName));

        Load3DPreview(item);
    }

    /// <summary>Exports the kit's real FC26 Frostbite mesh and renders it in the 3D panel.</summary>
    private void Load3DPreview(RecordListItem item)
    {
        // teamkits table: teamid, kitname, kitid (kit type index), and linked
        // teamtechid (team asset ID used to resolve mesh + texture).
        var kitId = _vm.Session.Database.GetCell("teamkits", item.RecordIndex, "kitid");
        var teamTechId = _vm.Session.Database.GetCell("teamkits", item.RecordIndex, "teamtechid");
        if (string.IsNullOrWhiteSpace(kitId) || string.IsNullOrWhiteSpace(teamTechId))
        {
            Kit3DPreview.ShowStatus("Select a kit to preview its 3D model.");
            return;
        }

        Kit3DPreview.ShowStatus("Exporting kit mesh from FC26...");

        // Query tokens to locate the kit's MeshSet in the FC26 archives.
        // The primary hit is kit_{teamtechid}_{kitid}; the team asset is also a
        // fallback when the individual kit mesh is absent.
        var queries = new[]
        {
            $"kit_{teamTechId}_{kitId}",
            $"kit_{teamTechId}",
            $"team_{teamTechId}"
        };
        Task.Run(() =>
        {
            var assets = _vm.Session.FrostbiteAssets;
            var fbxPath = assets.ExportMeshForQuery(queries);
            if (string.IsNullOrWhiteSpace(fbxPath)) return null;

            // The sidecar texture is resolved by BridgeEngine (kit_*_color)
            // and embedded in the FBX material, so no extra work is needed here.
            return fbxPath;
        }).ContinueWith(task =>
        {
            if (_current != item) return;
            if (task.Status != TaskStatus.RanToCompletion || task.Result == null)
            {
                Kit3DPreview.ShowStatus("Kit mesh not found in FC26 archives.");
                return;
            }
            Kit3DPreview.LoadMesh(task.Result);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (_current is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("teamkits", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private void RefreshEditor()
    {
        if (_current is not RecordListItem item) return;
        LoadEditor(item);
    }

    // ---------- CM16 KitForm groupings (Positions / 3D Model / Jersey / Name) ----------

    private static bool IsPosition(string n) => n.Contains("placementcode", StringComparison.OrdinalIgnoreCase)
        || n.Contains("positions", StringComparison.OrdinalIgnoreCase)
        || n.Contains("namelayouttype", StringComparison.OrdinalIgnoreCase)
        || n.Contains("numberfonttype", StringComparison.OrdinalIgnoreCase)
        || n.Contains("backnamefontcase", StringComparison.OrdinalIgnoreCase)
        || n.Contains("hasbackname", StringComparison.OrdinalIgnoreCase);
    private static bool IsName(string n) => n.Contains("name", StringComparison.OrdinalIgnoreCase);
}
