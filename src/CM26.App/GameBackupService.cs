using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace CM26.App;

/// <summary>
/// Restores the FC26 Data/Patch tree from the user's CmModData snapshot.
/// The snapshot is treated as immutable source material; CM26 never writes to
/// it.  A restore is intentionally explicit because it replaces all modified
/// and extra files below the two game folders.
/// </summary>
public static class GameBackupService
{
    private static readonly string[] RestoredFolders = ["Data", "Patch"];
    private const string ManifestName = "cm26-backup-manifest.json";

    public sealed record BackupStatus(bool IsReady, string GameRoot, string BackupRoot, string Message);
    public sealed record BackupResult(bool Success, BackupStatus Status, int CopiedFiles, string Message);
    public sealed record RestoreProgress(
        string Phase, int Completed, int Total, string CurrentFile,
        long CompletedBytes = 0, long TotalBytes = 0);
    public sealed record RestoreResult(bool Success, string Message, int CopiedFiles, int DeletedFiles);
    public sealed record CompressionResult(bool Success, string Message);
    public sealed record BaselineStatus(bool IsMatch, string Message);
    public sealed record RefreshResult(bool Success, string Message, string ArchivedBackupPath);

    public static BackupStatus Inspect(string? gameRoot, bool verifyContent = false)
    {
        if (string.IsNullOrWhiteSpace(gameRoot) || !Directory.Exists(gameRoot))
            return new(false, string.Empty, string.Empty, "Game installation was not detected.");

        var root = Path.GetFullPath(gameRoot!).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var backup = Path.Combine(root, "CmModData");
        if (!Directory.Exists(backup))
            return new(false, root, backup, "CmModData backup folder was not found.");
        foreach (var name in RestoredFolders)
            if (!Directory.Exists(Path.Combine(backup, name)))
                return new(false, root, backup, $"CmModData is missing its {name} backup folder.");
        if (!File.Exists(Path.Combine(backup, "Patch", "layout.toc")) ||
            !File.Exists(Path.Combine(backup, "Patch", "initfs_Win32")))
            return new(false, root, backup, "CmModData does not contain a complete Patch snapshot.");
        var manifestPath = Path.Combine(backup, ManifestName);
        if (File.Exists(manifestPath))
        {
            try
            {
                var manifest = JsonSerializer.Deserialize<BackupManifest>(File.ReadAllText(manifestPath))
                    ?? throw new InvalidDataException("Backup manifest is empty.");
                foreach (var item in manifest.Files)
                {
                    var candidate = Path.Combine(backup, item.RelativePath);
                    EnsureChild(backup, candidate);
                    var info = new FileInfo(candidate);
                    if (!info.Exists || info.Length != item.Length)
                        return new(false, root, backup,
                            $"CmModData inventory validation failed for {item.RelativePath}.");
                    // Manifests written by older versions only store the size;
                    // newer manifests also carry a SHA-256 so the snapshot can be
                    // verified against silent corruption even when a file keeps
                    // its exact length.
                    // The full SHA-256 check is intentionally reserved for the
                    // explicit backup audit and Restore. Rehashing a 10 GB
                    // immutable tree on every Open Game made startup needlessly
                    // slow, while size/inventory checks still catch missing or
                    // truncated backup files.
                    if (verifyContent && item.Sha256 != null && item.Sha256.Length > 0 &&
                        !string.Equals(item.Sha256, ComputeSha256(candidate), StringComparison.OrdinalIgnoreCase))
                        return new(false, root, backup,
                            $"CmModData integrity check failed for {item.RelativePath}.");
                }
            }
            catch (Exception ex)
            {
                return new(false, root, backup, "CmModData manifest is invalid: " + ex.Message);
            }
        }
        return new(true, root, backup, "CmModData backup is ready.");
    }

    public static BackupResult EnsureCreated(
        string? gameRoot, IProgress<RestoreProgress>? progress = null)
    {
        var existing = Inspect(gameRoot);
        if (existing.IsReady)
        {
            EnsureManifest(existing.BackupRoot);
            var baseline = InspectLiveBaseline(existing.GameRoot);
            if (!baseline.IsMatch)
                return new(false, existing, 0, baseline.Message);
            return new(true, existing, 0, "Existing CmModData backup verified.");
        }
        if (!FrostbiteAssetSession.IsGameRoot(gameRoot))
            return new(false, existing, 0, existing.Message);
        if (IsGameRunning())
            return new(false, existing, 0, "Close the game before creating CmModData.");

        var root = Path.GetFullPath(gameRoot!).TrimEnd(
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var destination = Path.Combine(root, "CmModData");
        if (Directory.Exists(destination))
            return new(false, existing, 0,
                "CmModData exists but is incomplete. Move or repair that folder before creating a new backup.");
        var temporary = Path.Combine(root, ".CmModData.cm26creating-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(temporary);
            var copied = 0;
            var manifestFiles = new List<BackupFile>();
            foreach (var folder in RestoredFolders)
            {
                var source = Path.Combine(root, folder);
                var target = Path.Combine(temporary, folder);
                EnsureDirectChild(root, source);
                EnsureDirectChild(root, target);
                var files = EnumerateFiles(source).Select(path => new FileInfo(path)).ToArray();
                var totalBytes = files.Sum(file => file.Length);
                var completedBytes = 0L;
                for (var index = 0; index < files.Length; index++)
                {
                    var file = files[index];
                    var relative = Path.GetRelativePath(source, file.FullName);
                    var output = Path.Combine(target, relative);
                    EnsureChild(target, output);
                    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                    progress?.Report(new($"Backing up {folder}", index, files.Length, relative, completedBytes, totalBytes));
                    var hash = CopyAndHash(file.FullName, output, copiedBytes =>
                        progress?.Report(new($"Backing up {folder}", index, files.Length, relative,
                            completedBytes + copiedBytes, totalBytes)));
                    File.SetLastWriteTimeUtc(output, file.LastWriteTimeUtc);
                    File.SetAttributes(output, file.Attributes);
                    completedBytes += file.Length;
                    manifestFiles.Add(new BackupFile(
                        Path.Combine(folder, relative), file.Length, hash));
                    copied++;
                    progress?.Report(new($"Backing up {folder}", index + 1, files.Length, relative, completedBytes, totalBytes));
                }
            }
            WriteManifest(temporary, manifestFiles);
            Directory.Move(temporary, destination);
            var status = Inspect(root);
            return new(status.IsReady, status, copied,
                status.IsReady
                    ? $"Original Data and Patch backed up to CmModData ({copied} files)."
                    : status.Message);
        }
        catch (Exception ex)
        {
            return new(false, Inspect(root), 0, "CmModData backup failed: " + ex.Message);
        }
        finally
        {
            if (Directory.Exists(temporary)) Directory.Delete(temporary, recursive: true);
        }
    }

    /// <summary>
    /// Checks the small set of Frostbite root files that identify the exact FC26
    /// build.  This is deliberately fast enough for Open Game, unlike a full
    /// multi-gigabyte backup audit.
    /// </summary>
    public static BaselineStatus InspectLiveBaseline(string? gameRoot)
    {
        var backup = Inspect(gameRoot);
        if (!backup.IsReady) return new(false, backup.Message);
        var mismatches = new List<string>();
        foreach (var relative in new[]
                 {
                     Path.Combine("Data", "layout.toc"),
                     Path.Combine("Data", "initfs_Win32"),
                     Path.Combine("Patch", "layout.toc"),
                     Path.Combine("Patch", "initfs_Win32"),
                 })
        {
            var live = Path.Combine(backup.GameRoot, relative);
            var snapshot = Path.Combine(backup.BackupRoot, relative);
            if (!File.Exists(live) || !File.Exists(snapshot) ||
                new FileInfo(live).Length != new FileInfo(snapshot).Length ||
                !string.Equals(ComputeSha256(live), ComputeSha256(snapshot), StringComparison.OrdinalIgnoreCase))
                mismatches.Add(relative.Replace(Path.DirectorySeparatorChar, '/'));
        }
        if (mismatches.Count == 0)
            return new(true, "CmModData matches the installed FC26 baseline.");

        var fetData = Directory.Exists(Path.Combine(backup.GameRoot, "FIFAModData")) ||
                      Directory.Exists(Path.Combine(backup.GameRoot, "FIFAModData_backup"));
        return new(false,
            "FC26 has changed since CmModData was created (" + string.Join(", ", mismatches) + "). " +
            "This can happen after a title update" + (fetData ? " or while FET/FIFAModData is present" : string.Empty) + ". " +
            "Launch FC26 once without mods and confirm it reaches the main menu, then use Settings > Refresh CmModData. " +
            "CM26 will not open or save against a mixed baseline.");
    }

    /// <summary>
    /// Archives the previous immutable snapshot and creates a new one from the
    /// current vanilla game build. The caller must explicitly confirm that the
    /// game was launched without mods after the latest title update.
    /// </summary>
    public static RefreshResult RefreshAfterVanillaLaunch(
        string? gameRoot, IProgress<RestoreProgress>? progress = null)
    {
        if (!FrostbiteAssetSession.IsGameRoot(gameRoot))
            return new(false, "Game installation was not detected.", string.Empty);
        if (IsGameRunning())
            return new(false, "Close FC26 before refreshing CmModData.", string.Empty);

        var root = Path.GetFullPath(gameRoot!).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var current = Path.Combine(root, "CmModData");
        var archived = string.Empty;
        try
        {
            if (Directory.Exists(current))
            {
                var suffix = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                archived = Path.Combine(root, "CmModData_previous_" + suffix);
                var attempt = 1;
                while (Directory.Exists(archived))
                    archived = Path.Combine(root, "CmModData_previous_" + suffix + "_" + attempt++);
                Directory.Move(current, archived);
            }

            var created = EnsureCreated(root, progress);
            if (!created.Success)
                throw new InvalidOperationException(created.Message);
            return new(true,
                "A fresh CmModData snapshot was created for the current FC26 build. " +
                (string.IsNullOrWhiteSpace(archived) ? string.Empty : "Previous snapshot archived at " + archived + "."),
                archived);
        }
        catch (Exception ex)
        {
            if (!Directory.Exists(current) && !string.IsNullOrWhiteSpace(archived) && Directory.Exists(archived))
                Directory.Move(archived, current);
            return new(false, "CmModData refresh failed: " + ex.Message, archived);
        }
    }

    public static RestoreResult Restore(BackupStatus status, IProgress<RestoreProgress>? progress = null)
    {
        if (!status.IsReady) return new(false, status.Message, 0, 0);
        if (IsGameRunning())
            return new(false, "Close the game before restoring its Data and Patch folders.", 0, 0);

        var verified = Inspect(status.GameRoot, verifyContent: true);
        if (!verified.IsReady) return new(false, verified.Message, 0, 0);

        try
        {
            var copied = 0;
            var deleted = 0;
            foreach (var folder in RestoredFolders)
            {
                var source = Path.Combine(status.BackupRoot, folder);
                var target = Path.Combine(status.GameRoot, folder);
                EnsureDirectChild(verified.GameRoot, source);
                EnsureDirectChild(verified.GameRoot, target);
                Directory.CreateDirectory(target);

                var sourceFiles = EnumerateFiles(source).ToArray();
                var sourceNames = sourceFiles.Select(path => Path.GetRelativePath(source, path))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                for (var index = 0; index < sourceFiles.Length; index++)
                {
                    var file = sourceFiles[index];
                    var relative = Path.GetRelativePath(source, file);
                    var destination = Path.Combine(target, relative);
                    EnsureChild(target, destination);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                    progress?.Report(new($"Restoring {folder}", index + 1, sourceFiles.Length, relative));
                    CopyAtomically(file, destination);
                    copied++;
                }

                // Remove files introduced after the backup only after every
                // backup file has been copied successfully.
                foreach (var file in EnumerateFiles(target))
                {
                    var relative = Path.GetRelativePath(target, file);
                    if (sourceNames.Contains(relative)) continue;
                    EnsureChild(target, file);
                    File.Delete(file);
                    deleted++;
                }
                RemoveEmptyDirectories(target);
            }
            return new(true,
                $"Original Data and Patch restored from CmModData ({copied} files copied, {deleted} extra files removed).",
                copied, deleted);
        }
        catch (Exception ex)
        {
            return new(false, "Restore failed: " + ex.Message, 0, 0);
        }
    }

    /// <summary>
    /// Enables transparent NTFS compression for the immutable backup. File
    /// contents and hashes remain unchanged; Windows handles decompression.
    /// Frostbite CAS files are already compressed, so the space saving may be small.
    /// </summary>
    public static CompressionResult EnableNtfsCompression(string? gameRoot)
    {
        var status = Inspect(gameRoot);
        if (!status.IsReady) return new(false, status.Message);
        try
        {
            var compact = Path.Combine(Environment.SystemDirectory, "compact.exe");
            if (!File.Exists(compact))
                return new(false, "Windows compact.exe is unavailable.");
            var start = new ProcessStartInfo(compact)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            start.ArgumentList.Add("/C");
            start.ArgumentList.Add("/I");
            start.ArgumentList.Add("/Q");
            start.ArgumentList.Add("/S:" + status.BackupRoot);
            using var process = Process.Start(start)
                ?? throw new InvalidOperationException("Unable to start Windows compression.");
            var outputTask = Task.Run(() => process.StandardOutput.ReadToEnd());
            var errorTask = Task.Run(() => process.StandardError.ReadToEnd());
            var output = outputTask.GetAwaiter().GetResult().Trim();
            var error = errorTask.GetAwaiter().GetResult().Trim();
            process.WaitForExit();
            if (process.ExitCode != 0)
                return new(false, string.IsNullOrWhiteSpace(error) ? output : error);
            return new(true,
                "Transparent NTFS compression is enabled for CmModData. Backup file contents are unchanged.");
        }
        catch (Exception ex)
        {
            return new(false, "Backup compression failed: " + ex.Message);
        }
    }

    private static bool IsGameRunning() => new[] { "FC26", "FC26_Trial", "FC26_Showcase" }
        .Any(name => Process.GetProcessesByName(name).Length > 0);

    private static IEnumerable<string> EnumerateFiles(string root)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var full = Path.GetFullPath(file);
            if (!full.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                throw new IOException("A backup path points outside its expected folder.");
            yield return full;
        }
    }

    private static void CopyAtomically(string source, string destination)
    {
        var temporary = destination + ".cm26restore-" + Guid.NewGuid().ToString("N");
        try
        {
            if (File.Exists(destination))
            {
                var destinationAttributes = File.GetAttributes(destination);
                if (destinationAttributes.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(destination, destinationAttributes & ~FileAttributes.ReadOnly);
            }
            File.Copy(source, temporary, overwrite: false);
            File.Move(temporary, destination, overwrite: true);
            File.SetLastWriteTimeUtc(destination, File.GetLastWriteTimeUtc(source));
            File.SetAttributes(destination, File.GetAttributes(source));
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static void RemoveEmptyDirectories(string root)
    {
        foreach (var directory in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                     .OrderByDescending(path => path.Length))
        {
            if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory);
        }
    }

    private static void EnsureDirectChild(string root, string candidate) => EnsureChild(root, candidate);

    private static void EnsureManifest(string backupRoot)
    {
        var path = Path.Combine(backupRoot, ManifestName);
        if (!File.Exists(path))
        {
            WriteManifest(backupRoot);
            return;
        }
        // Upgrade older size-only inventories to the hashed format so existing
        // backups gain silent-corruption protection without being re-created.
        try
        {
            var manifest = JsonSerializer.Deserialize<BackupManifest>(File.ReadAllText(path));
            if (manifest != null && manifest.Version < 2)
                WriteManifest(backupRoot);
        }
        catch
        {
            // Leave a malformed manifest alone; Inspect reports it explicitly.
        }
    }

    private static void WriteManifest(string backupRoot)
    {
        var files = RestoredFolders
            .SelectMany(folder => EnumerateFiles(Path.Combine(backupRoot, folder)))
            .Select(path => new BackupFile(
                Path.GetRelativePath(backupRoot, path),
                new FileInfo(path).Length,
                ComputeSha256(path)))
            .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (files.Length == 0)
            throw new InvalidDataException("Cannot create an inventory for an empty CmModData backup.");
        WriteManifest(backupRoot, files);
    }

    private static void WriteManifest(string backupRoot, IEnumerable<BackupFile> files)
    {
        var inventory = files
            .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (inventory.Length == 0)
            throw new InvalidDataException("Cannot create an inventory for an empty CmModData backup.");
        var manifest = new BackupManifest(2, DateTimeOffset.UtcNow, inventory);
        File.WriteAllText(
            Path.Combine(backupRoot, ManifestName),
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void EnsureChild(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedCandidate = Path.GetFullPath(candidate);
        if (!normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new IOException("Restore target is outside the game installation.");
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string CopyAndHash(string source, string destination, Action<long>? copied)
    {
        using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 1024 * 1024, FileOptions.SequentialScan);
        using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            bufferSize: 1024 * 1024, FileOptions.SequentialScan);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[1024 * 1024];
        long total = 0;
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
            hash.AppendData(buffer, 0, read);
            total += read;
            copied?.Invoke(total);
        }
        output.Flush(flushToDisk: true);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private sealed record BackupManifest(int Version, DateTimeOffset CreatedUtc, BackupFile[] Files);
    private sealed record BackupFile(string RelativePath, long Length, string? Sha256);
}
