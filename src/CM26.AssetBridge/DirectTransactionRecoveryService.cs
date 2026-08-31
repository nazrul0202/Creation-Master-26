using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CM26.AssetBridge;

/// <summary>
/// Recovers interrupted direct-write transactions using the durable journal
/// produced by <see cref="FrostbiteDirectLegacyWriter"/>. Recovery is deliberately
/// fail-closed: every path must remain inside the recorded game or transaction
/// root, and FC26 must be closed before any file is changed.
/// </summary>
public static class DirectTransactionRecoveryService
{
    private static readonly string[] GameProcessNames = { "FC26", "FC26_Trial", "FC26_Showcase" };

    public static string DefaultTransactionRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "direct-transactions");

    public static DirectRecoveryResult RecoverPending(string? transactionRoot = null)
    {
        var running = GameProcessNames.Where(name => Process.GetProcessesByName(name).Length != 0).ToArray();
        if (running.Length != 0)
            throw new InvalidOperationException("Close FC26 before attempting direct-save recovery.");

        var root = Path.GetFullPath(transactionRoot ?? DefaultTransactionRoot);
        if (!Directory.Exists(root))
            return new DirectRecoveryResult(0, 0, Array.Empty<string>());

        var recovered = 0;
        var alreadySafe = 0;
        var failures = new List<string>();
        foreach (var folder in Directory.EnumerateDirectories(root).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var outcome = RecoverFolder(folder);
                if (outcome == RecoveryOutcome.Recovered) recovered++;
                else alreadySafe++;
            }
            catch (Exception ex)
            {
                failures.Add(Path.GetFileName(folder) + ": " + ex.Message);
            }
        }
        return new DirectRecoveryResult(recovered, alreadySafe, failures);
    }

    private static RecoveryOutcome RecoverFolder(string transactionFolder)
    {
        var transactionRoot = Path.GetFullPath(transactionFolder);
        var journalPath = Path.Combine(transactionRoot, "transaction.json");
        if (!File.Exists(journalPath))
            throw new InvalidDataException("transaction.json is missing; recovery evidence was preserved.");

        var node = JsonNode.Parse(File.ReadAllText(journalPath)) as JsonObject
            ?? throw new InvalidDataException("transaction.json is not a JSON object.");
        var state = node["State"]?.GetValue<string>() ?? string.Empty;
        if (state.Equals("Complete", StringComparison.OrdinalIgnoreCase) ||
            state.Equals("RolledBack", StringComparison.OrdinalIgnoreCase))
            return RecoveryOutcome.AlreadySafe;

        var gameRootValue = node["GameRoot"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(gameRootValue))
            throw new InvalidDataException("The journal has no game root.");
        var gameRoot = Path.GetFullPath(gameRootValue);
        if (!Directory.Exists(gameRoot))
            throw new DirectoryNotFoundException("The recorded FC26 workspace is no longer available.");

        var tocRestores = ReadTocRestores(node, gameRoot, transactionRoot);
        var casLengths = ReadCasLengths(node, gameRoot);

        // Validate the complete plan before changing a single byte.
        foreach (var item in tocRestores)
        {
            if (!File.Exists(item.Backup)) throw new FileNotFoundException("A TOC rollback copy is missing.", item.Backup);
            if (!File.Exists(item.Live)) throw new FileNotFoundException("The live TOC is missing.", item.Live);
        }
        foreach (var item in casLengths)
        {
            if (!File.Exists(item.Path)) throw new FileNotFoundException("A journaled CAS file is missing.", item.Path);
            if (item.Length < 0 || new FileInfo(item.Path).Length < item.Length)
                throw new InvalidDataException("A CAS file is shorter than its recorded original length.");
        }

        foreach (var item in tocRestores.AsEnumerable().Reverse())
        {
            var incoming = item.Live + ".cm26-recover-" + Guid.NewGuid().ToString("N");
            try
            {
                File.Copy(item.Backup, incoming, overwrite: false);
                File.Move(incoming, item.Live, overwrite: true);
            }
            finally
            {
                if (File.Exists(incoming)) File.Delete(incoming);
            }
        }
        foreach (var item in casLengths)
        {
            using var stream = new FileStream(item.Path, FileMode.Open, FileAccess.Write, FileShare.Read);
            stream.SetLength(item.Length);
            stream.Flush(flushToDisk: true);
        }

        node["State"] = "RolledBack";
        node["UpdatedUtc"] = DateTimeOffset.UtcNow;
        node["RecoveryCompletedUtc"] = DateTimeOffset.UtcNow;
        node["RollbackErrors"] = new JsonArray();
        WriteJournalAtomically(journalPath, node);
        var marker = Path.Combine(transactionRoot, "RECOVERY_REQUIRED.json");
        if (File.Exists(marker)) File.Delete(marker);
        return RecoveryOutcome.Recovered;
    }

    private static List<(string Live, string Backup)> ReadTocRestores(
        JsonObject journal, string gameRoot, string transactionRoot)
    {
        var result = new List<(string, string)>();
        if (journal["TocFiles"] is not JsonArray tocFiles) return result;
        foreach (var value in tocFiles.OfType<JsonObject>())
        {
            var liveValue = value["Live"]?.GetValue<string>();
            var backupValue = value["Backup"]?.GetValue<string>();
            // Prepared journals contain Staged, not Backup. No live TOC was
            // replaced at that state, so there is nothing to restore.
            if (string.IsNullOrWhiteSpace(backupValue)) continue;
            if (string.IsNullOrWhiteSpace(liveValue))
                throw new InvalidDataException("A TOC rollback entry has no live path.");
            var live = RequireContained(liveValue, gameRoot, "TOC live path");
            var backup = RequireContained(backupValue, transactionRoot, "TOC backup path");
            result.Add((live, backup));
        }
        return result;
    }

    private static List<(string Path, long Length)> ReadCasLengths(JsonObject journal, string gameRoot)
    {
        var result = new List<(string, long)>();
        if (journal["CasOriginalLengths"] is not JsonObject values) return result;
        foreach (var pair in values)
        {
            if (pair.Value is not JsonValue jsonValue || !jsonValue.TryGetValue<long>(out var length))
                throw new InvalidDataException("A CAS rollback length is invalid.");
            result.Add((RequireContained(pair.Key, gameRoot, "CAS path"), length));
        }
        return result;
    }

    private static string RequireContained(string candidate, string allowedRoot, string label)
    {
        var path = Path.GetFullPath(candidate);
        var root = Path.GetFullPath(allowedRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var prefix = root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(label + " escapes its allowed recovery root.");
        return path;
    }

    private static void WriteJournalAtomically(string destination, JsonObject journal)
    {
        var temporary = destination + ".recovery-new";
        File.WriteAllText(temporary, journal.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, destination, overwrite: true);
    }

    private enum RecoveryOutcome { Recovered, AlreadySafe }
}

public sealed record DirectRecoveryResult(int Recovered, int AlreadySafe, IReadOnlyList<string> Failures)
{
    public bool Success => Failures.Count == 0;

    public string ToDisplayText()
    {
        var lines = new List<string>
        {
            "Creation Master 26 direct-save recovery",
            "Recovered: " + Recovered,
            "Already safe: " + AlreadySafe
        };
        if (Failures.Count == 0) lines.Add("Result: all transaction folders are safe.");
        else
        {
            lines.Add("Result: " + Failures.Count + " transaction(s) still require review.");
            lines.AddRange(Failures.Select(value => "- " + value));
        }
        return string.Join(Environment.NewLine, lines);
    }
}
