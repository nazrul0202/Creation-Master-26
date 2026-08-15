using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Views;

/// <summary>
/// FC26 implementation of CM16 CountryForm. The editor intentionally exposes
/// the named CM16 controls and real country graphics, not a generic table dump.
/// </summary>
public partial class CountryView : UserControl
{
    private static readonly string[] IdentityFieldOrder =
    {
        "nationname", "nationid", "confederation", "isocountrycode",
        "groupid", "nationstartingfirstletter", "top_tier", "streetdressing",
    };

    private readonly ViewModel _vm;
    private RecordListItem? _current;
    private int _assetRequest;

    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public CountryView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        PickUp.SelectObject += LoadEditor;
        PickUp.RefreshObject += LoadList;
        Loaded += (_, _) => LoadList();
    }

    private void LoadList()
    {
        var countries = _vm.Session.Sections.GetCountries();
        PickUp.ObjectList = countries;
        if (countries.Count > 0 && PickUp.SelectedIndex < 0)
            PickUp.SelectedIndex = 0;
    }

    private void LoadEditor(RecordListItem item)
    {
        _current = item;
        var fields = _vm.Session.Sections.GetFields("nations", item.RecordIndex, LabelMaps.Nations);
        IdentityFields.ItemsSource = OrderedFields(fields, IdentityFieldOrder);
        TeamFields.ItemsSource = fields.Where(f => IsNationalTeamField(f.FieldName)).ToList();
        AudioFields.ItemsSource = fields.Where(f => IsAudioField(f.FieldName)).ToList();

        var nationId = ParseField(fields, "nationid");
        _ = LoadFlagAsync(nationId);
    }

    private static IReadOnlyList<FieldValue> OrderedFields(IReadOnlyList<FieldValue> fields, IEnumerable<string> order)
    {
        var byName = fields.ToDictionary(x => x.FieldName, StringComparer.OrdinalIgnoreCase);
        return order.Where(byName.ContainsKey).Select(name => byName[name]).ToList();
    }

    private static int ParseField(IEnumerable<FieldValue> fields, string name)
    {
        var value = fields.FirstOrDefault(f => f.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
        return int.TryParse(value, out var result) ? result : -1;
    }

    private async Task LoadFlagAsync(int nationId)
    {
        var request = ++_assetRequest;
        FlagLarge.Source = Flag512.Source = FlagCard.Source = FlagMini.Source = null;
        FlagLargeCaption.Visibility = Flag512Caption.Visibility = Visibility.Visible;
        if (nationId < 0 || !_vm.Session.FrostbiteAssets.IsAvailable) return;

        var exported = await Task.Run(() =>
        {
            foreach (var candidate in new[]
            {
                $"data/ui/imgAssets/flags512x512/light/f_{nationId}.dds",
                $"data/ui/imgAssets/flags512x512/dark/f_{nationId}.dds",
                $"data/ui/imgAssets/flags/f_{nationId}.dds",
            })
            {
                var path = _vm.Session.FrostbiteAssets.ExportLegacyAsset(candidate);
                if (!string.IsNullOrWhiteSpace(path)) return path;
            }
            return null;
        });
        if (request != _assetRequest || string.IsNullOrWhiteSpace(exported)) return;

        var bitmap = await Task.Run(() => CreateBitmapSource(exported));
        if (request != _assetRequest || bitmap == null) return;
        FlagLarge.Source = Flag512.Source = FlagCard.Source = FlagMini.Source = bitmap;
        FlagLargeCaption.Visibility = Flag512Caption.Visibility = Visibility.Collapsed;
    }

    private static BitmapSource? CreateBitmapSource(string path)
    {
        using var preview = new TexturePreviewService().CreatePreview(path, 512, 512);
        if (preview == null) return null;
        using var stream = new MemoryStream();
        preview.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static bool IsAudioField(string name) =>
        name.Contains("audio", StringComparison.OrdinalIgnoreCase)
        || name.Contains("chant", StringComparison.OrdinalIgnoreCase)
        || name.Contains("whistle", StringComparison.OrdinalIgnoreCase)
        || name.Contains("heckle", StringComparison.OrdinalIgnoreCase)
        || name.Contains("reaction", StringComparison.OrdinalIgnoreCase)
        || name.Contains("taunt", StringComparison.OrdinalIgnoreCase)
        || name.Contains("ambience", StringComparison.OrdinalIgnoreCase)
        || name.Contains("crowdtype", StringComparison.OrdinalIgnoreCase);

    private static bool IsNationalTeamField(string name) =>
        name.Contains("target", StringComparison.OrdinalIgnoreCase)
        || name.Contains("worldcup", StringComparison.OrdinalIgnoreCase)
        || name.Contains("regional", StringComparison.OrdinalIgnoreCase)
        || name.Contains("nationalteam", StringComparison.OrdinalIgnoreCase);

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (_current == null) return null;
        var outcome = _vm.Session.Pending.Stage("nations", _current.RecordIndex, fieldName, value);
        if (outcome.Success) LoadEditor(_current);
        return outcome;
    }
}
