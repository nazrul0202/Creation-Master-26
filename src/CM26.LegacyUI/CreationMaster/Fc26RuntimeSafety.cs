using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CreationMaster;

/// <summary>
/// Runtime-only safety checks shared by Save Preflight and Public Readiness.
/// These checks never modify FC26 and deliberately fail closed when a previous
/// direct transaction requires manual recovery.
/// </summary>
internal static class Fc26RuntimeSafety
{
    private static readonly string[] GameProcessNames = { "FC26", "FC26_Trial", "FC26_Showcase" };

    internal static string TransactionRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "direct-transactions");

    internal static string[] RunningGameProcesses()
    {
        var running = new List<string>();
        foreach (var name in GameProcessNames)
        {
            try
            {
                if (Process.GetProcessesByName(name).Length > 0) running.Add(name);
            }
            catch { /* A failed process query is handled again by the x64 writer. */ }
        }
        return running.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    internal static string[] RecoveryRequiredFolders()
    {
        try
        {
            if (!Directory.Exists(TransactionRoot)) return Array.Empty<string>();
            return Directory.EnumerateDirectories(TransactionRoot)
                .Where(RequiresRecovery)
                .OrderBy(folder => folder, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch { return new[] { TransactionRoot }; }
    }

    private static bool RequiresRecovery(string folder)
    {
        if (File.Exists(Path.Combine(folder, "RECOVERY_REQUIRED.json"))) return true;
        var journal = Path.Combine(folder, "transaction.json");
        if (!File.Exists(journal)) return true;
        try
        {
            using (var document = JsonDocument.Parse(File.ReadAllText(journal)))
            {
                JsonElement state;
                if (!document.RootElement.TryGetProperty("State", out state)) return true;
                var value = state.GetString();
                return !string.Equals(value, "Complete", StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(value, "RolledBack", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch { return true; }
    }

    internal static long AvailableWorkspaceBytes()
    {
        try
        {
            var path = Fc26SnapshotLoader.CurrentSnapshotPath;
            if (string.IsNullOrWhiteSpace(path)) return -1;
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            return string.IsNullOrWhiteSpace(root) ? -1 : new DriveInfo(root).AvailableFreeSpace;
        }
        catch { return -1; }
    }

    internal static bool SnapshotIsReadable(out string detail)
    {
        var path = Fc26SnapshotLoader.CurrentSnapshotPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            detail = "The loaded FC26 snapshot is missing. Reopen FC26 before saving.";
            return false;
        }
        try
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                if (stream.Length == 0) throw new InvalidDataException("snapshot is empty");
            detail = "The loaded snapshot is present and readable.";
            return true;
        }
        catch (Exception ex)
        {
            detail = "The loaded snapshot cannot be read: " + ex.Message;
            return false;
        }
    }

    internal static bool BackupBaselineIsReady(out string detail)
    {
        var gameRoot = Fc26SnapshotLoader.CurrentGameRoot;
        if (string.IsNullOrWhiteSpace(gameRoot))
        {
            detail = "Extracted database mode does not write installed Data/Patch; its source folder remains the recovery boundary.";
            return true;
        }
        try
        {
            var backup = Path.Combine(Path.GetFullPath(gameRoot), "CmModData");
            var data = Path.Combine(backup, "Data");
            var patch = Path.Combine(backup, "Patch");
            if (!Directory.Exists(data) || !Directory.Exists(patch))
            {
                detail = "A complete CmModData\\Data and CmModData\\Patch baseline is required before direct Save.";
                return false;
            }
            var dataReady = Directory.EnumerateFiles(data, "*", SearchOption.AllDirectories).Take(1).Any();
            var patchReady = Directory.EnumerateFiles(patch, "*", SearchOption.AllDirectories).Take(1).Any();
            detail = dataReady && patchReady
                ? "CmModData contains both original Data and Patch payloads."
                : "CmModData exists but one original payload is empty; refresh the baseline before Save.";
            return dataReady && patchReady;
        }
        catch (Exception ex)
        {
            detail = "The original backup baseline could not be verified: " + ex.Message;
            return false;
        }
    }

    internal static void OpenRecoveryFolder()
    {
        Directory.CreateDirectory(TransactionRoot);
        Process.Start(new ProcessStartInfo("explorer.exe", "\"" + TransactionRoot + "\"") { UseShellExecute = true });
    }
}
