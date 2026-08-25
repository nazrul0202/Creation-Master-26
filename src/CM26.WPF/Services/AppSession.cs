using System.IO;
using CM26.Application;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Services;

/// <summary>
/// Owns the shared database session and the service graph built on top of it,
/// reusing the same CM26.Application services the WinForms app uses. Opening
/// the game follows the same direct Frostbite flow: detect the install, index
/// the game archives, extract the database, load it - no folder picking.
/// </summary>
public sealed class AppSession : IDisposable
{
    public DatabaseSession Database { get; } = new();
    public FrostbiteAssetSession FrostbiteAssets { get; } = new();
    public PendingChangesService Pending { get; }
    public NameResolverService Resolver { get; private set; }
    public SectionDataService Sections { get; private set; }
    public ValidationService Validation { get; }
    public SaveService Save { get; }
    public LegacyAssetModService LegacyMods { get; } = new();
    public string SourcePath { get; private set; } = string.Empty;
    public bool IsDirectGameSource { get; private set; } = true;

    public AppSession()
    {
        Pending = new PendingChangesService(Database);
        Resolver = new NameResolverService(Database);
        Sections = new SectionDataService(Database, Resolver, Pending);
        Validation = new ValidationService(Database);
        Save = new SaveService(Database);
    }

    /// <summary>
    /// Same method as the WinForms app: resolve the FC26 install from settings,
    /// index its archives, extract the database into a managed workspace and
    /// load it for direct editing.
    /// </summary>
    public bool TryOpenGame(out string message, IProgress<string>? progress = null)
    {
        var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
        if (string.IsNullOrWhiteSpace(gameRoot))
        {
            message = "The FC26 installation was not detected. Set the game folder in Settings, then try again.";
            return false;
        }
        try
        {
            progress?.Report("Indexing game archives.");
            FrostbiteAssets.Open(gameRoot);
            if (!FrostbiteAssets.IsAvailable)
            {
                message = FrostbiteAssets.Status;
                return false;
            }
            progress?.Report("Extracting database from game archives.");
            var workspace = Fc26WorkspaceService.Open(FrostbiteAssets);
            progress?.Report("Verifying the original Data/Patch safety backup.");
            var backup = GameBackupService.EnsureCreated(workspace.GameRoot);
            if (!backup.Success)
            {
                message = "CM26 cannot enable direct FC26 editing until the original Data/Patch backup is valid. " + backup.Message;
                return false;
            }
            SettingsService.FC26GameFolder = workspace.GameRoot;
            progress?.Report("Building editor indexes.");
            Database.Load(workspace.DatabaseFolder);
            Pending.ResetSession("Opened installed FC26 Data/Patch database.");
            RebuildSections();
            LegacyMods.Open(FrostbiteAssets.Fingerprint);
            IsDirectGameSource = true;
            SourcePath = workspace.GameRoot;
            SettingsService.PushRecentFolder(workspace.GameRoot);
            message = "FC26 database loaded for direct editing: " + workspace.GameRoot;
            return true;
        }
        catch (Exception ex)
        {
            message = "Open FC26 failed: " + ex.Message;
            return false;
        }
    }

    /// <summary>Opens a user-owned extracted DB/meta/localisation folder without touching FC26.</summary>
    public bool TryOpenDatabaseFolder(string folder, out string message)
    {
        try
        {
            var fullPath = Path.GetFullPath(folder);
            Database.Load(fullPath);
            Pending.ResetSession("Opened extracted database folder.");
            RebuildSections();
            IsDirectGameSource = false;
            SourcePath = fullPath;
            SettingsService.LastFolder = fullPath;
            SettingsService.PushRecentFolder(fullPath);
            message = "Extracted FC26 database and localisation loaded: " + fullPath;
            return true;
        }
        catch (Exception ex)
        {
            message = "Open extracted database failed: " + ex.Message;
            return false;
        }
    }

    public void CloseDatabase()
    {
        Database.Close();
        Pending.ResetSession("Database closed.");
        SourcePath = string.Empty;
        RebuildSections();
    }

    /// <summary>Reloads the live database from the game archives (after direct edits).</summary>
    public bool ReloadFromGame(out string message)
    {
        try
        {
            var workspace = Fc26WorkspaceService.Open(FrostbiteAssets);
            Database.Load(workspace.DatabaseFolder);
            RebuildSections();
            message = "Database reloaded from the FC26 archives.";
            return true;
        }
        catch (Exception ex)
        {
            message = "Reload failed: " + ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Name maps and section caches are built eagerly against the loaded
    /// database, so they must be rebuilt after every (re)load.
    /// </summary>
    private void RebuildSections()
    {
        Resolver = new NameResolverService(Database);
        Sections = new SectionDataService(Database, Resolver, Pending);
    }

    public void Dispose()
    {
        Database.Dispose();
    }
}
