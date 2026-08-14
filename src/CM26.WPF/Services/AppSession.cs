using CM26.Application;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.Studio.Services;

/// <summary>
/// Owns the shared database session and the service graph built on top of it,
/// reusing the same CM26.Application services the WinForms app uses.
/// </summary>
public sealed class AppSession : IDisposable
{
    public DatabaseSession Database { get; } = new();
    public PendingChangesService Pending { get; }
    public SectionDataService Sections { get; }

    public AppSession()
    {
        Pending = new PendingChangesService(Database);
        Sections = new SectionDataService(Database, new NameResolverService(Database), Pending);
    }

    /// <summary>Loads the most recent workspace database folder, if one exists.</summary>
    public bool TryLoadRecentWorkspace(out string message)
    {
        foreach (var folder in WorkspaceLocator.RecentDatabaseFolders())
        {
            var summary = Database.ValidateFolder(folder);
            if (summary.State == LoadStateKind.Success)
            {
                try
                {
                    Database.Load(folder);
                    message = "Database loaded: " + folder;
                    return true;
                }
                catch (Exception ex)
                {
                    message = "Database load failed: " + ex.Message;
                    return false;
                }
            }
        }
        message = "No FC26 workspace database found. Open a database folder first.";
        return false;
    }

    public void Dispose() => Database.Dispose();
}