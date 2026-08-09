namespace CM26.Application.Services;

public sealed record SaveResult(bool Success, string Message, string? BackupMain, string? BackupLocale);

/// <summary>
/// Coordinates a safe save: backup sources, write validated copies THROUGH the engine,
/// then reload-verify. The app never writes database bytes itself.
/// </summary>
public sealed class SaveService
{
    private readonly DatabaseSession _session;

    public SaveService(DatabaseSession session) => _session = session;

    /// <summary>
    /// Save staged changes back to the loaded folder. Creates timestamped backups next to the
    /// originals, writes via the engine, then reload-verifies the written files.
    /// </summary>
    public SaveResult SaveToSourceFolder()
    {
        if (!_session.IsLoaded)
            return new SaveResult(false, "No database loaded", null, null);

        var folder = _session.LoadedFolder!;
        var mainPath = _session.DatabasePath!;
        var localePath = _session.LocalePath!;
        var metaPath = _session.MetaPath!;

        try
        {
            var integrityIssues = _session.ValidateIntegrity();
            if (integrityIssues.Count > 0)
                return new SaveResult(false, "Save blocked by database integrity checks: " + string.Join("; ", integrityIssues.Take(5)) + (integrityIssues.Count > 5 ? $" (+{integrityIssues.Count - 5} more)" : string.Empty), null, null);

            // 1. Detect read-only / locked files up front.
            EnsureWritable(mainPath);
            EnsureWritable(localePath);

            // 2. Timestamped backups (engine never overwrites sources; we do so only after backup).
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupMain = Path.Combine(folder, $"fifa_ng_db.backup_{stamp}.db");
            var backupLocale = Path.Combine(folder, $"eng_us.backup_{stamp}.db");
            File.Copy(mainPath, backupMain, overwrite: false);
            File.Copy(localePath, backupLocale, overwrite: false);

            // 3. Write to temp files via the engine, then replace originals atomically-ish.
            var tmpMain = Path.Combine(folder, "fifa_ng_db.cm26tmp.db");
            var tmpLocale = Path.Combine(folder, "eng_us.cm26tmp.db");
            try
            {
                _session.SaveCopy(locale: false, tmpMain);
                _session.SaveCopy(locale: true, tmpLocale);

                // 4. Reload-verify both temp files before replacing originals.
                _session.VerifyFile(metaPath, tmpMain, encryptedLocale: false);
                _session.VerifyFile(string.Empty, tmpLocale, encryptedLocale: true);

                File.Copy(tmpMain, mainPath, overwrite: true);
                File.Copy(tmpLocale, localePath, overwrite: true);
            }
            finally
            {
                TryDelete(tmpMain);
                TryDelete(tmpLocale);
            }

            return new SaveResult(true,
                $"Saved and verified. Backups created: {Path.GetFileName(backupMain)}, {Path.GetFileName(backupLocale)}",
                backupMain, backupLocale);
        }
        catch (Exception ex)
        {
            return new SaveResult(false, $"Save failed: {ex.Message}", null, null);
        }
    }

    /// <summary>
    /// Serialises the active edit session into an isolated folder for a mod
    /// package.  Unlike <see cref="SaveToSourceFolder"/>, this never replaces
    /// a file in the installed game.
    /// </summary>
    public SaveResult SaveToDirectory(string destinationFolder)
    {
        if (!_session.IsLoaded)
            return new SaveResult(false, "No database loaded", null, null);
        try
        {
            var integrityIssues = _session.ValidateIntegrity();
            if (integrityIssues.Count > 0)
                return new SaveResult(false, "Mod export blocked by database integrity checks: " + string.Join("; ", integrityIssues.Take(5)), null, null);
            Directory.CreateDirectory(destinationFolder);
            var main = Path.Combine(destinationFolder, "fifa_ng_db.db");
            var locale = Path.Combine(destinationFolder, "eng_us.db");
            _session.SaveCopy(locale: false, main);
            _session.SaveCopy(locale: true, locale);
            _session.VerifyFile(_session.MetaPath!, main, encryptedLocale: false);
            _session.VerifyFile(string.Empty, locale, encryptedLocale: true);
            return new SaveResult(true, "Edited database prepared for CM26 mod export.", null, null);
        }
        catch (Exception ex)
        {
            return new SaveResult(false, "Mod export save failed: " + ex.Message, null, null);
        }
    }

    private static void EnsureWritable(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("File missing", path);
        var attr = File.GetAttributes(path);
        if (attr.HasFlag(FileAttributes.ReadOnly))
            throw new IOException($"File is read-only: {Path.GetFileName(path)}");
        using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* best effort */ }
    }
}
