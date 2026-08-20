using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;
using CM26.Studio.Controls;

namespace CM26.Studio.Views;

/// <summary>
/// CM16-style PlayerForm: Find + player list on the left, edit tabs on the
/// right matching the exact group layout of Creation Master 16 PlayerForm
/// (pageInfo: Identity Card / Body / Look / Playing for / Shoes / Playing Info,
/// pageSkills: Random Generation + skill groups, pageFace: Appearance).
/// </summary>
public partial class PlayersView : UserControl
{
    private readonly ViewModel _vm;
    private IReadOnlyList<RecordListItem> _all = Array.Empty<RecordListItem>();
    private IReadOnlyList<FieldValue> _currentFields = Array.Empty<FieldValue>();
    private RecordListItem? _current;
    private bool _loadingClub;

    /// <summary>Wired to each FieldRow so field edits stage through the pending service.</summary>
    public Func<string, string, EditOutcome?>? StageEditDelegate { get; }

    public PlayersView(ViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        StageEditDelegate = StageEdit;
        TraitsGrid.Toggle = TogglePlaystyle;
        VirtualProGrid.Toggle = TogglePlaystyle;
        PickUp.SelectObject += LoadEditorFromPickUp;
        PickUp.FilterByList = new[] { "All", "by Team", "by Country", "Free Agents" };
        PickUp.RefreshObject += LoadList;
        Loaded += (_, _) => LoadList();
    }

    private void LoadEditorFromPickUp(RecordListItem item) => LoadEditor(item);

    private void LoadList()
    {
        _all = _vm.Session.Sections.GetPlayers();
        PickUp.ObjectList = _all;
        if (_all.Count > 0 && PickUp.SelectedIndex < 0) PickUp.SelectedIndex = 0;
    }
    private void LoadEditor(RecordListItem item)
    {
        _current = item;
        _currentFields = _vm.Session.Sections.GetFields("players", item.RecordIndex, LabelMaps.Players);
        var fields = _currentFields;

        IdentityFields.ItemsSource = fields.Where(f => IsIdentity(f.FieldName));
        BodyFields.ItemsSource = fields.Where(f => IsBody(f.FieldName));
        LookFields.ItemsSource = fields.Where(f => IsLook(f.FieldName));
        TeamFields.ItemsSource = fields.Where(f => IsTeam(f.FieldName));
        ShoesFields.ItemsSource = fields.Where(f => IsShoes(f.FieldName));
        PositionFields.ItemsSource = fields.Where(f => IsPlayingInfo(f.FieldName));

        GkFields.ItemsSource = fields.Where(f => IsGk(f.FieldName));
        DefensiveFields.ItemsSource = fields.Where(f => IsDefensive(f.FieldName));
        MidfielderFields.ItemsSource = fields.Where(f => IsMidfielder(f.FieldName));
        MentalFields.ItemsSource = fields.Where(f => IsMental(f.FieldName));
        AttackingFields.ItemsSource = fields.Where(f => IsAttacking(f.FieldName));
        PhysicalFields.ItemsSource = fields.Where(f => IsPhysical(f.FieldName));
        FreeKickFields.ItemsSource = fields.Where(f => IsFreeKick(f.FieldName));

        TraitsGrid.Items = BuildPlaystyleFlags(fields, "trait1", "trait2");
        VirtualProGrid.Items = BuildPlaystyleFlags(fields, "icontrait1", "icontrait2");

        FaceTypeFields.ItemsSource = fields.Where(f => IsFaceType(f.FieldName) && !IsNamedAppearance(f.FieldName));
        HairFields.ItemsSource = fields.Where(f => IsHair(f.FieldName) && !IsNamedAppearance(f.FieldName));
        HeadFields.ItemsSource = fields.Where(f => IsHead(f.FieldName) && !IsNamedAppearance(f.FieldName));

        FillNamedCombos(fields);

        var ovr = fields.FirstOrDefault(f => f.FieldName == "overallrating");
        OverallText.Text = ovr?.Value ?? "—";
        OverallSlider.Value = double.TryParse(ovr?.RawValue, out var v) ? v : 0;
        FillClubPicker();
        LoadFacePreview();
        EditTabs.SelectedIndex = 0;
    }

    /// <summary>Builds the 34 playstyle flags for one bitmask pair (trait1/trait2 or icontrait1/icontrait2).</summary>
    private static IReadOnlyList<PlaystyleFlag> BuildPlaystyleFlags(IReadOnlyList<FieldValue> fields, string fieldA, string fieldB)
    {
        var flags = new List<PlaystyleFlag>(PlaystyleCatalog.Names.Length);
        for (int i = 0; i < PlaystyleCatalog.Names.Length; i++)
        {
            var (field, bit) = i < 32 ? (fieldA, i) : (fieldB, i - 32);
            var fv = fields.FirstOrDefault(f => f.FieldName.Equals(field, StringComparison.OrdinalIgnoreCase));
            var mask = fv != null && uint.TryParse(fv.RawValue, out var m) ? m : 0u;
            flags.Add(new PlaystyleFlag
            {
                Field = field,
                Bit = bit,
                Name = PlaystyleCatalog.Names[i],
                IsSet = (mask & (1u << bit)) != 0,
                IsWritable = fv?.IsWritable ?? false,
            });
        }
        return flags;
    }

    /// <summary>Playstyle checkbox toggled: re-read the current mask, flip the bit, stage the new mask.</summary>
    private EditOutcome? TogglePlaystyle(string fieldName, int bit, bool set)
    {
        if (_current is not RecordListItem item) return null;
        var fv = _currentFields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        if (fv == null || !uint.TryParse(fv.RawValue, out var mask)) return null;
        var newMask = set ? mask | (1u << bit) : mask & ~(1u << bit);
        var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, fieldName, newMask.ToString());
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    private EditOutcome? StageEdit(string fieldName, string value)
    {
        if (_current is not RecordListItem item) return null;
        var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, fieldName, value);
        if (outcome.Success) RefreshEditor();
        return outcome;
    }

    /// <summary>Fills the CM16-style named combos (Face tab) from the current fields.
    /// Fields covered by a named combo are excluded from the generic field lists.</summary>
    private void FillNamedCombos(IReadOnlyList<FieldValue> fields)
    {
        var genericFace = fields.FirstOrDefault(f => f.FieldName.Equals("headclasscode", StringComparison.OrdinalIgnoreCase));
        CheckHasGenericFace.Visibility = genericFace == null ? Visibility.Collapsed : Visibility.Visible;
        if (genericFace != null)
        {
            CheckHasGenericFace.SetContent("Has Generic Face", genericFace.FieldName, genericFace.RawValue,
                genericFace.IsWritable, StageEdit);
            GenericFaceControls.IsEnabled = genericFace.RawValue != "0";
        }
        else
        {
            GenericFaceControls.IsEnabled = true;
        }

        FillCombo(ComboSkinType, "skintypecode", "Skin Type", AppearanceCatalog.SkinTypes, null, fields);
        FillCombo(ComboSkinTone, "skintonecode", "Skin Tone", AppearanceCatalog.SkinTones, null, fields, valueOffset: 1);
        FillCombo(ComboEyesBrow, "eyebrowcode", "Eyes Brow", AppearanceCatalog.EyebrowTypes, null, fields);
        FillCombo(ComboFacialHair, "facialhairtypecode", "Facial Hair", AppearanceCatalog.FacialHairTypes, null, fields);
        FillCombo(ComboFacialHairColor, "facialhaircolorcode", "Color", AppearanceCatalog.FacialHairColors, null, fields);

        FillModel(ComboHairModel, "hairtypecode", "Hair Model",
            AppearanceCatalog.HairModelSets.Select(set => (set.Name, set.Models)), fields);
        FillCombo(ComboHairColor, "haircolorcode", "Hair Color", AppearanceCatalog.HairColors, null, fields);

        FillModel(ComboHeadModel, "headtypecode", "Head Model",
            AppearanceCatalog.HeadModelSets.Select(set => (set.Name, set.Models)), fields);
        FillCombo(ComboSideburns, "sideburnscode", "Sideburns", AppearanceCatalog.Sideburns, null, fields);
        FillCombo(ComboEyesColor, "eyecolorcode", "Eyes Color", AppearanceCatalog.EyeColors, null, fields, valueOffset: 1);
        FillCombo(ComboFacePoser, "faceposerpreset", "Face Poser", AppearanceCatalog.FacePosers, null, fields);

        var hq = fields.FirstOrDefault(f => f.FieldName.Equals("hashighqualityhead", StringComparison.OrdinalIgnoreCase));
        CheckHighQualityHead.SetContent("High Quality Face", "hashighqualityhead", hq?.RawValue,
            hq?.IsWritable ?? false, StageEdit);
    }

    private void FillCombo(NamedComboField combo, string fieldName, string label, IReadOnlyList<string> names,
        IReadOnlyList<int>? values, IReadOnlyList<FieldValue> fields, int valueOffset = 0)
    {
        var fv = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        var valueList = values ?? Enumerable.Range(valueOffset, names.Count).ToArray();
        combo.SetContent(label, fieldName, names, valueList, fv?.RawValue,
            fv?.IsWritable ?? false, StageEdit);
    }

    private void FillModel(GroupedModelPicker picker, string fieldName, string label,
        IEnumerable<(string Name, int[] Values)> groups, IReadOnlyList<FieldValue> fields)
    {
        var field = fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        picker.Visibility = field == null ? Visibility.Collapsed : Visibility.Visible;
        if (field != null)
            picker.SetContent(label, field.FieldName, groups, field.RawValue, field.IsWritable, StageEdit);
    }

    private void RefreshEditor()
    {
        if (_current is not RecordListItem item) return;
        LoadEditor(item);
    }

    private sealed record ClubOption(string Name, int TeamId)
    {
        public override string ToString() => Name;
    }

    /// <summary>Team options for the transfer picker: Free Agent + every teams row.</summary>
    private IReadOnlyList<ClubOption> BuildClubOptions()
    {
        var options = new List<ClubOption> { new("Free Agent", -1) };
        try
        {
            var teams = _vm.Session.Sections.GetTeams();
            var table = _vm.Session.Database.GetTable("teams");
            if (table != null)
            {
                foreach (var team in teams)
                {
                    if (string.IsNullOrWhiteSpace(team.Title)) continue;
                    var raw = _vm.Session.Database.GetCell("teams", team.RecordIndex, "teamid");
                    if (int.TryParse(raw, out var teamId) && teamId > 0)
                        options.Add(new ClubOption(team.Title, teamId));
                }
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CM26] Club options load failed: {ex.Message}"); }
        return options;
    }

    private void FillClubPicker()
    {
        _loadingClub = true;
        try
        {
            if (ClubPicker.Items.Count == 0)
            {
                foreach (var option in BuildClubOptions())
                    ClubPicker.Items.Add(option);
            }
            var playerId = CurrentPlayerId();
            var currentTeam = _vm.Session.Resolver?.PlayerTeamId(playerId);
            var selected = 0;
            for (var i = 0; i < ClubPicker.Items.Count; i++)
            {
                if (ClubPicker.Items[i] is ClubOption option && currentTeam.HasValue && option.TeamId == currentTeam.Value)
                {
                    selected = i;
                    break;
                }
            }
            ClubPicker.SelectedIndex = selected;
        }
        finally { _loadingClub = false; }
    }

    private void ClubPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingClub || _current is not RecordListItem item) return;
        if (ClubPicker.SelectedItem is not ClubOption option) return;
        var playerId = CurrentPlayerId();
        if (playerId <= 0) return;

        var links = _vm.Session.Database.GetTable("teamplayerlinks");
        if (links == null || links.RowCount == 0) return;
        try
        {
            var linkRow = -1;
            for (var row = 0; row < links.RowCount; row++)
            {
                if (int.TryParse(_vm.Session.Database.GetCell("teamplayerlinks", row, "playerid"), out var pid) && pid == playerId)
                {
                    linkRow = row;
                    break;
                }
            }
            if (linkRow < 0)
            {
                var duplicated = _vm.Session.Database.DuplicateRow("teamplayerlinks", links.RowCount - 1);
                if (!duplicated.Success)
                {
                    MessageBox.Show(duplicated.Message, "Transfer Player", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                linkRow = links.RowCount - 1;
                _vm.Session.Pending.Stage("teamplayerlinks", linkRow, "playerid", playerId.ToString());
                _vm.Session.Pending.MarkStructuralChange();
            }
            var outcome = _vm.Session.Pending.Stage("teamplayerlinks", linkRow, "teamid", option.TeamId.ToString());
            if (outcome.Success)
            {
                _vm.Session.Resolver?.Rebuild();
                RefreshEditor();
            }
            else
            {
                MessageBox.Show(outcome.Message, "Transfer Player", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Transfer Player", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private int CurrentPlayerId()
    {
        if (_current is not RecordListItem item) return -1;
        var raw = _vm.Session.Database.GetCell("players", item.RecordIndex, "playerid");
        return int.TryParse(raw, out var id) ? id : -1;
    }

    // ---------- Face texture preview + 3D viewer (FC26 assets) ----------

    private int _faceRequest;
    private string FaceLegacyPath => $"data/ui/imgAssets/heads/p{CurrentPlayerId()}.dds";

    private async void LoadFacePreview()
    {
        var request = ++_faceRequest;
        var playerId = CurrentPlayerId();
        FacePreview.Source = null;
        FaceCaption.Text = "Player face texture";
        if (playerId <= 0) return;
        var exported = await Task.Run(() =>
        {
            try
            {
                var staged = _vm.Session.LegacyMods.GetReplacement(FaceLegacyPath);
                if (!string.IsNullOrWhiteSpace(staged) && File.Exists(staged)) return staged;
                return _vm.Session.FrostbiteAssets.ExportLegacyAsset(FaceLegacyPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CM26] Face export failed: {ex.Message}");
                return null;
            }
        });
        if (request != _faceRequest || string.IsNullOrWhiteSpace(exported)) return;
        var bitmap = await Task.Run(() => CreateFaceBitmap(exported));
        if (request != _faceRequest || bitmap == null) return;
        FacePreview.Source = bitmap;
        FaceCaption.Text = "Installed face texture";
    }

    private static BitmapSource? CreateFaceBitmap(string path)
    {
        try
        {
            using var preview = new TexturePreviewService().CreatePreview(path, 256, 256);
            if (preview == null) return null;
            using var stream = new MemoryStream();
            preview.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            var bitmap = new BitmapImage();
            bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.StreamSource = stream; bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CM26] Face preview decode failed: {ex.Message}");
            return null;
        }
    }

    private void ImportFace_Click(object sender, RoutedEventArgs e)
    {
        var playerId = CurrentPlayerId();
        if (playerId <= 0) return;
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import Player Face",
            Filter = "Image files (*.dds;*.png;*.jpg;*.jpeg;*.bmp)|*.dds;*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        try
        {
            _vm.Session.LegacyMods.StageImage(FaceLegacyPath, dialog.FileName, 256, 256);
            LoadFacePreview();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Import Face",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RemoveFace_Click(object sender, RoutedEventArgs e)
    {
        var playerId = CurrentPlayerId();
        if (playerId <= 0) return;
        try
        {
            _vm.Session.LegacyMods.Remove(FaceLegacyPath);
            LoadFacePreview();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Remove Face",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExportFace_Click(object sender, RoutedEventArgs e)
    {
        var playerId = CurrentPlayerId();
        if (playerId <= 0) return;
        string? source;
        try
        {
            source = _vm.Session.LegacyMods.GetReplacement(FaceLegacyPath)
                ?? _vm.Session.FrostbiteAssets.ExportLegacyAsset(FaceLegacyPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Export Face",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
        {
            MessageBox.Show(Window.GetWindow(this), "No installed or staged face texture is available to export.",
                "Export Face", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Export Player Face",
            FileName = $"p{playerId}.dds",
            Filter = "DDS texture (*.dds)|*.dds|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        try { File.Copy(source, dialog.FileName, overwrite: true); }
        catch (Exception ex)
        {
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Export Face",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Open3DFace_Click(object sender, RoutedEventArgs e)
    {
        var playerId = CurrentPlayerId();
        if (playerId <= 0) return;
        FaceCaption.Text = "Searching for the player's head mesh…";
        Face3DPreview.ShowStatus("Exporting head mesh from FC26…");
        var headAssetId = CurrentHeadAssetId();
        var exported = await Task.Run(() =>
        {
            var queries = new[]
            {
                headAssetId > 0 ? $"head_{headAssetId}_0_0_mesh" : string.Empty,
                headAssetId > 0 ? $"head_{headAssetId}" : string.Empty,
                playerId > 0 ? $"head_{playerId}_0_0_mesh" : string.Empty,
                playerId > 0 ? $"head_{playerId}" : string.Empty,
            };
            return _vm.Session.FrostbiteAssets.ExportMeshForQuery(queries);
        });
        if (string.IsNullOrWhiteSpace(exported))
        {
            FaceCaption.Text = "No head mesh found for this player.";
            Face3DPreview.ShowStatus("No head mesh found for this player.");
            return;
        }
        FaceCaption.Text = "3D head mesh exported · rendering in-app…";
        Face3DPreview.LoadMesh(exported);
    }

    private int CurrentHeadAssetId()
    {
        if (_current is not RecordListItem item) return 0;
        var raw = _vm.Session.Database.GetCell("players", item.RecordIndex, "headassetid");
        return int.TryParse(raw, out var id) ? id : 0;
    }

    private void OverallSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_current is not RecordListItem item) return;
        var value = ((int)OverallSlider.Value).ToString();
        var outcome = _vm.Session.Pending.Stage("players", item.RecordIndex, "overallrating", value);
        if (outcome.Success) OverallText.Text = value;
    }

    private void Randomize_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.Tag is not string target) return;
        if (_current is not RecordListItem item) return;
        var fields = _vm.Session.Sections.GetFields("players", item.RecordIndex, LabelMaps.Players);
        var rnd = new Random();
        foreach (var f in fields.Where(f => IsSkillField(f.FieldName) && int.TryParse(f.RawValue, out _)))
        {
            var skill = int.Parse(f.RawValue);
            var spread = rnd.Next(-10, 11);
            var newValue = Math.Clamp(skill + spread, 0, 99);
            _vm.Session.Pending.Stage("players", item.RecordIndex, f.FieldName, newValue.ToString());
        }
        var targetVal = Math.Clamp(int.Parse(target), 1, 99);
        _vm.Session.Pending.Stage("players", item.RecordIndex, "overallrating", targetVal.ToString());
        OverallSlider.Value = targetVal;
        OverallText.Text = targetVal.ToString();
        RefreshEditor();
    }

    // ---------- CM16 Info tab groupings ----------

    private static bool IsIdentity(string n) => n is "playerid" or "firstnameid" or "lastnameid" or "commonnameid"
        or "birthdate" or "nationality" or "gender";
    private static bool IsBody(string n) => n is "height" or "weight" or "preferredfoot" or "weakfootabilitytypecode" or "bodytype";
    private static bool IsLook(string n) => n.Contains("sleeve", StringComparison.OrdinalIgnoreCase)
        || n.Contains("jersey", StringComparison.OrdinalIgnoreCase)
        || n.Contains("accessor", StringComparison.OrdinalIgnoreCase)
        || n.Contains("winter", StringComparison.OrdinalIgnoreCase)
        || n.Contains("socks", StringComparison.OrdinalIgnoreCase)
        || n.Contains("fit", StringComparison.OrdinalIgnoreCase);
    private static bool IsTeam(string n) => n.Contains("team", StringComparison.OrdinalIgnoreCase)
        || n.Contains("loan", StringComparison.OrdinalIgnoreCase)
        || n.Contains("contract", StringComparison.OrdinalIgnoreCase)
        || n.Contains("jointeam", StringComparison.OrdinalIgnoreCase)
        || n.Contains("captain", StringComparison.OrdinalIgnoreCase);
    private static bool IsShoes(string n) => n.Contains("shoes", StringComparison.OrdinalIgnoreCase)
        || n.Contains("boots", StringComparison.OrdinalIgnoreCase)
        || n.Contains("sock", StringComparison.OrdinalIgnoreCase);
    private static bool IsPlayingInfo(string n) => n.Contains("position", StringComparison.OrdinalIgnoreCase)
        || n.Contains("skillmoves", StringComparison.OrdinalIgnoreCase)
        || n.Contains("workrate", StringComparison.OrdinalIgnoreCase)
        || n.Contains("reputation", StringComparison.OrdinalIgnoreCase)
        || n is "overallrating" or "potential" or "form";

    // ---------- CM16 Skills tab groupings ----------

    private static bool IsGk(string n) => n.StartsWith("gk", StringComparison.OrdinalIgnoreCase);
    private static bool IsDefensive(string n) => n is "aggression" or "marking" or "headingaccuracy" or "standingtackle"
        or "slidingtackle" or "interceptions" or "defensiveawareness";
    private static bool IsMidfielder(string n) => n is "ballcontrol" or "crossing" or "shortpassing" or "longpassing"
        or "vision" or "curve";
    private static bool IsMental(string n) => n is "reactions" or "composure" or "positioning" or "aggression" or "interceptions";
    private static bool IsAttacking(string n) => n is "dribbling" or "finishing" or "shotpower" or "longshots"
        or "volleys" or "penalties" or "positioning";
    private static bool IsPhysical(string n) => n is "acceleration" or "sprintspeed" or "agility" or "balance"
        or "jumping" or "stamina" or "strength";
    private static bool IsFreeKick(string n) => n.Contains("freekick", StringComparison.OrdinalIgnoreCase)
        || n.Contains("fk", StringComparison.OrdinalIgnoreCase)
        || n.Contains("penalty", StringComparison.OrdinalIgnoreCase)
        || n.Contains("corner", StringComparison.OrdinalIgnoreCase)
        || n.Contains("gkkick", StringComparison.OrdinalIgnoreCase);

    private static bool IsSkillField(string n) => IsGk(n) || IsDefensive(n) || IsMidfielder(n) || IsMental(n)
        || IsAttacking(n) || IsPhysical(n) || IsFreeKick(n)
        || n is "overallrating" or "potential";

    // ---------- CM16 Face tab groupings ----------

    private static bool IsFaceType(string n) => n.Contains("skin", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eyebrow", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eyecolor", StringComparison.OrdinalIgnoreCase)
        || n.Contains("facialhair", StringComparison.OrdinalIgnoreCase)
        || n.Contains("eyedetail", StringComparison.OrdinalIgnoreCase);
    private static bool IsHair(string n) => n.Contains("hair", StringComparison.OrdinalIgnoreCase);
    private static bool IsHead(string n) => n.Contains("head", StringComparison.OrdinalIgnoreCase)
        || n.Contains("face", StringComparison.OrdinalIgnoreCase)
        || n.Contains("beard", StringComparison.OrdinalIgnoreCase)
        || n.Contains("tattoo", StringComparison.OrdinalIgnoreCase);

    /// <summary>Fields rendered as CM16-style named combos/checks on the Face tab
    /// (so they are excluded from the generic text-box field lists).</summary>
    private static bool IsNamedAppearance(string n) => n.Equals("skintypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("skintonecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("eyebrowcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("facialhairtypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("facialhaircolorcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("hairtypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("haircolorcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("headtypecode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("sideburnscode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("eyecolorcode", StringComparison.OrdinalIgnoreCase)
        || n.Equals("faceposerpreset", StringComparison.OrdinalIgnoreCase)
        || n.Equals("hashighqualityhead", StringComparison.OrdinalIgnoreCase)
        || n.Equals("headclasscode", StringComparison.OrdinalIgnoreCase);
}
