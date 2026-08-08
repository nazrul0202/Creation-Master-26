using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CM26.App;

/// <summary>
/// Opens the live FC26 Data/Patch archives and materialises the three database
/// payloads required by the editor. The materialised files are an internal
/// parser session only: every Open refreshes them from the installed game and
/// Save writes replacements back to the live Data/Patch archives.
/// CmModData remains the immutable original snapshot.
/// </summary>
public static class Fc26WorkspaceService
{
    private static readonly object Gate = new();
    private static readonly string[] RequiredFiles =
    ["fifa_ng_db-meta.xml", "fifa_ng_db.db", "eng_us.db"];

    public sealed record Workspace(string GameRoot, string DatabaseFolder, string SourceFolder, bool Created);

    public static Workspace Open(FrostbiteAssetSession? sharedAssets = null)
    {
        lock (Gate)
        {
            var gameRoot = sharedAssets?.IsAvailable == true
                ? sharedAssets.GameRoot
                : FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder);
            if (string.IsNullOrWhiteSpace(gameRoot))
                throw new InvalidOperationException(
                    "The game installation was not detected. Set the game folder in Settings, then use File > Open Game.");

            var sessionFolder = GetSessionFolder(gameRoot);
            RefreshFromLiveGame(gameRoot, sessionFolder, sharedAssets);
            return new Workspace(gameRoot, sessionFolder, gameRoot, Created: true);
        }
    }

    public static bool HasRequiredFiles(string? folder) =>
        !string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder) &&
        RequiredFiles.All(file => FindFile(folder, file) != null);

    public static bool IsManagedWorkspace(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return false;
        try
        {
            var root = Path.GetFullPath(GetSessionRoot()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var candidate = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static string GetSessionFolder(string gameRoot)
    {
        var sourceKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            Path.GetFullPath(gameRoot).ToUpperInvariant())))[..12].ToLowerInvariant();
        // Never refresh a database in place while the native parser may still own
        // the previous session. Each Open gets an immutable generation; AppServices
        // swaps to it only after the new payload has loaded successfully.
        return Path.Combine(
            GetSessionRoot(), "FC26-" + sourceKey,
            $"database-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}");
    }

    private static string GetSessionRoot() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "direct-sessions");

    private static void RefreshFromLiveGame(
        string gameRoot, string sessionFolder, FrostbiteAssetSession? sharedAssets)
    {
        var parent = Path.GetDirectoryName(sessionFolder)
            ?? throw new InvalidOperationException("Invalid CM26 direct-session path.");
        Directory.CreateDirectory(parent);

        var temporary = sessionFolder + ".refreshing-" + Guid.NewGuid().ToString("N");
        try
        {
            Directory.CreateDirectory(temporary);
            var extraction = Path.Combine(temporary, ".frostbite-extract");
            if (!ExtractLegacyDatabase(gameRoot, extraction, sharedAssets))
                throw new InvalidOperationException(
                    "Database and localisation assets could not be read from the installed Data/Patch archives.");
            CopyDatabaseFiles(extraction, temporary, overwrite: false);
            if (Directory.Exists(extraction)) Directory.Delete(extraction, recursive: true);

            var manifest = new WorkspaceManifest(Path.GetFullPath(gameRoot), Path.GetFullPath(gameRoot), DateTimeOffset.UtcNow);
            File.WriteAllText(Path.Combine(temporary, "cm26-workspace.json"), JsonSerializer.Serialize(manifest));

            // The destination is generation-specific, so publish the fully built
            // workspace with one directory rename instead of exposing partially
            // copied files to the native database loader.
            Directory.Move(temporary, sessionFolder);
            temporary = string.Empty;
        }
        finally
        {
            if (Directory.Exists(temporary)) Directory.Delete(temporary, recursive: true);
        }
    }

    /// <summary>
    /// Reads the legacy files from the game-native ChunkFileCollector. Open is
    /// read-only; direct Data/Patch changes only happen later through Save.
    /// </summary>
    private static bool ExtractLegacyDatabase(
        string gameRoot, string destination, FrostbiteAssetSession? sharedAssets)
    {
        try
        {
            Directory.CreateDirectory(destination);
            var assets = sharedAssets ?? new FrostbiteAssetSession();
            if (!assets.IsAvailable ||
                !Path.GetFullPath(assets.GameRoot).Equals(Path.GetFullPath(gameRoot), StringComparison.OrdinalIgnoreCase))
                assets.Open(gameRoot);
            if (!assets.IsAvailable) return false;

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["fifa_ng_db-meta.xml"] = "data/db/fifa_ng_db-meta.xml",
                ["fifa_ng_db.db"] = "data/db/fifa_ng_db.db",
                ["eng_us.db"] = "data/loc/eng_us.db",
            };
            foreach (var (destinationName, legacyPath) in files)
            {
                var extracted = assets.ExportLegacyAsset(legacyPath);
                if (string.IsNullOrWhiteSpace(extracted) || !File.Exists(extracted)) return false;
                File.Copy(extracted, Path.Combine(destination, destinationName), overwrite: false);
            }
            return true;
        }
        catch { return false; }
    }

    private static void CopyDatabaseFiles(string source, string destination, bool overwrite)
    {
        foreach (var required in RequiredFiles)
        {
            var sourceFile = FindFile(source, required)
                ?? throw new FileNotFoundException("Required legacy database file was not found.", required);
            File.Copy(sourceFile, Path.Combine(destination, required), overwrite);
        }
    }

    private static string? FindFile(string folder, string name)
    {
        try
        {
            return Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path => Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        catch { return null; }
    }

    private sealed record WorkspaceManifest(string SourceFolder, string GameRoot, DateTimeOffset CreatedUtc);
}
