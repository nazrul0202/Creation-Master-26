using CM26.Application.Services;

namespace CM26.App.Sections;

/// <summary>Lightweight service container passed to all sections (DI kept minimal on purpose).</summary>
public sealed class AppServices : IDisposable
{
    private readonly Dictionary<int, (string First, string Surname, string Common)> _playerNameOverrides = [];
    private readonly HashSet<int> _draftCountryIds = [];
    public DatabaseSession Session { get; } = new();
    public PendingChangesService Pending { get; }
    public ValidationService Validation { get; }
    public SaveService Save { get; }
    public NameResolverService? Resolver { get; private set; }
    public SectionDataService? Data { get; private set; }
    /// <summary>
    /// Names entered for freshly created players when the live database does not
    /// expose a safe editable-name template. They make the new record usable in
    /// this editing session without attempting an unsafe localisation write.
    /// </summary>
    public IReadOnlyDictionary<int, (string First, string Surname, string Common)> PlayerNameOverrides => _playerNameOverrides;

    /// <summary>Read-only texture preview (PNG + DDS decode). Shared across all sections.</summary>
    public TexturePreviewService Textures { get; } = new();

    /// <summary>Resolves DB asset IDs to real local files. Rebuilt when the asset root changes.</summary>
    public AssetCatalogService Assets { get; private set; }

    /// <summary>FC26 Frostbite discovery/index and validated direct archive transaction.</summary>
    public FrostbiteAssetSession FrostbiteAssets { get; } = new();
    public LegacyAssetModService LegacyMods { get; } = new();
    public string ActiveGameRoot { get; private set; } = string.Empty;

    public event EventHandler? PendingChanged;
    public event EventHandler? DatabaseLoaded;
    public event EventHandler? FrostbiteAssetsReady;
    public event Action<string>? NavigationRequested;
    public event Action<string, int>? RecordNavigationRequested;
    /// <summary>Imports a verified local CM26 Scraper workbook into a selected club.</summary>
    public event Action<int, string>? ScraperSquadImportRequested;

    public AppServices()
    {
        Pending = new PendingChangesService(Session);
        Pending.Changed += (_, _) => PendingChanged?.Invoke(this, EventArgs.Empty);
        LegacyMods.Changed += (_, _) => PendingChanged?.Invoke(this, EventArgs.Empty);
        Validation = new ValidationService(Session);
        Save = new SaveService(Session);
        Assets = new AssetCatalogService(SettingsService.AssetRoot);
    }

    /// <summary>Re-point the asset catalog at a new root folder (user changed it in Settings).</summary>
    public void RefreshAssetRoot()
    {
        Assets = new AssetCatalogService(SettingsService.AssetRoot);
        Textures.ClearCache();
    }

    public void RefreshFrostbiteAssets(string? gameRoot = null) =>
        FrostbiteAssets.Open(gameRoot ?? SettingsService.FC26GameFolder);

    /// <summary>Loads database and legacy assets directly from FC26 Data/Patch.</summary>
    public Fc26WorkspaceService.Workspace OpenFc26(
        IProgress<GameBackupService.RestoreProgress>? backupProgress = null,
        IProgress<string>? phaseProgress = null)
    {
        var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
        if (string.IsNullOrWhiteSpace(gameRoot))
            throw new InvalidOperationException(
                "The game installation was not detected. Set the game folder in Settings, then try again.");
        phaseProgress?.Report("Indexing game archives…");
        FrostbiteAssets.Open(gameRoot);
        if (!FrostbiteAssets.IsAvailable)
            throw new InvalidOperationException(FrostbiteAssets.Status);
        phaseProgress?.Report("Reading database and localisation files…");
        var workspace = Fc26WorkspaceService.Open(FrostbiteAssets);
        phaseProgress?.Report("Verifying the original Data/Patch safety backup…");
        var backup = GameBackupService.EnsureCreated(workspace.GameRoot, backupProgress);
        if (!backup.Success)
            throw new InvalidOperationException(
                "CM26 cannot enable direct FC26 editing until the original Data/Patch backup is valid. " +
                backup.Message);
        SettingsService.FC26GameFolder = workspace.GameRoot;
        SettingsService.LastFolder = workspace.GameRoot;
        ActiveGameRoot = workspace.GameRoot;
        phaseProgress?.Report("Building editor indexes…");
        LoadDatabase(workspace.DatabaseFolder, workspace.GameRoot);
        phaseProgress?.Report("Ready.");
        return workspace;
    }

    /// <summary>
    /// Re-extracts the three live database payloads from the FC26 Data/Patch
    /// archives after a direct commit, then rebuilds every editor service from
    /// those freshly written archives.  The extraction folder is parser-only;
    /// the authoritative source and save target remain the installed game.
    /// </summary>
    public Fc26WorkspaceService.Workspace ReloadFromLiveFc26()
    {
        var workspace = Fc26WorkspaceService.Open();
        ActiveGameRoot = workspace.GameRoot;
        SettingsService.LastFolder = workspace.GameRoot;
        LoadDatabase(workspace.DatabaseFolder, workspace.GameRoot);
        return workspace;
    }

    /// <summary>Load a folder and (re)build resolver + data services. Runs off the UI thread.</summary>
    public void LoadDatabase(string folder, string? gameRoot = null)
    {
        if (!string.IsNullOrWhiteSpace(gameRoot))
            ActiveGameRoot = Path.GetFullPath(gameRoot);
        Session.Load(folder);
        Resolver = new NameResolverService(Session, _playerNameOverrides);
        Data = new SectionDataService(Session, Resolver, Pending);
        DatabaseLoaded?.Invoke(this, EventArgs.Empty);
        // The first Frostbite scan can take a minute.  Keep the editor usable
        // and warm the persistent bridge/index in the background instead.
        var assetsAlreadyReady = FrostbiteAssets.IsAvailable &&
            !string.IsNullOrWhiteSpace(FrostbiteAssets.GameRoot) &&
            !string.IsNullOrWhiteSpace(gameRoot) &&
            Path.GetFullPath(FrostbiteAssets.GameRoot).Equals(
                Path.GetFullPath(gameRoot), StringComparison.OrdinalIgnoreCase);
        if (assetsAlreadyReady)
        {
            LegacyMods.Open(FrostbiteAssets.Fingerprint);
            FrostbiteAssetsReady?.Invoke(this, EventArgs.Empty);
            return;
        }
        _ = Task.Run(() =>
        {
            RefreshFrostbiteAssets(gameRoot);
            if (FrostbiteAssets.IsAvailable)
                LegacyMods.Open(FrostbiteAssets.Fingerprint);
            FrostbiteAssetsReady?.Invoke(this, EventArgs.Empty);
        });
    }

    public SectionDataService RequireData()
    {
        if (Data == null) throw new InvalidOperationException("No database loaded");
        return Data;
    }

    public void NotifyPendingChanged() => PendingChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>Lets a CM16-style child control open the matching operational module.</summary>
    public void RequestNavigation(string sectionKey) => NavigationRequested?.Invoke(sectionKey);

    /// <summary>Open a specific database row in another CM16-style editor module.</summary>
    public void RequestRecordNavigation(string sectionKey, int recordIndex) =>
        RecordNavigationRequested?.Invoke(sectionKey, recordIndex);

    /// <summary>
    /// Refreshes name, team-link and section indexes after a structural edit.
    /// The active database remains open; no Data/Patch files are written here.
    /// </summary>
    public void RefreshDatabaseIndexes()
    {
        if (!Session.IsLoaded) return;
        Resolver = new NameResolverService(Session, _playerNameOverrides);
        Data = new SectionDataService(Session, Resolver, Pending);
        DatabaseLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void SetPlayerNameOverride(int playerId, string firstName, string surname, string commonName = "")
    {
        if (playerId <= 0) return;
        _playerNameOverrides[playerId] = (firstName.Trim(), surname.Trim(), commonName.Trim());
    }

    public void RegisterDraftCountry(int nationId)
    {
        if (nationId > 0) _draftCountryIds.Add(nationId);
    }

    public bool IsDraftCountry(int nationId) => _draftCountryIds.Contains(nationId);

    /// <summary>Route a local scraper output to the team editor for a safe import.</summary>
    public void RequestScraperSquadImport(int teamId, string workbookPath) =>
        ScraperSquadImportRequested?.Invoke(teamId, workbookPath);

    public void Dispose() => Session.Dispose();
}
