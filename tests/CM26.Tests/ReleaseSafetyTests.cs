using System.Text.Json;
using System.Text.RegularExpressions;
using CM26.AssetBridge;

namespace CM26.Tests;

public sealed class ReleaseSafetyTests
{
    [Fact]
    public void InterruptedDirectTransactionRestoresTocAndCas()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var game = Directory.CreateDirectory(Path.Combine(root, "game")).FullName;
            var transactions = Directory.CreateDirectory(Path.Combine(root, "transactions")).FullName;
            var transaction = Directory.CreateDirectory(Path.Combine(transactions, "case-1")).FullName;
            var cas = Path.Combine(game, "Data", "test.cas");
            var toc = Path.Combine(game, "Patch", "test.toc");
            Directory.CreateDirectory(Path.GetDirectoryName(cas)!);
            Directory.CreateDirectory(Path.GetDirectoryName(toc)!);
            File.WriteAllBytes(cas, new byte[] { 1, 2, 3, 4, 9, 9, 9 });
            File.WriteAllText(toc, "modified");
            var backup = Path.Combine(transaction, "rollback.toc");
            File.WriteAllText(backup, "original");
            WriteJournal(transaction, game, cas, 4, toc, backup);
            File.Copy(Path.Combine(transaction, "transaction.json"),
                Path.Combine(transaction, "RECOVERY_REQUIRED.json"));

            var result = DirectTransactionRecoveryService.RecoverPending(transactions);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Failures));
            Assert.Equal(1, result.Recovered);
            Assert.Equal("original", File.ReadAllText(toc));
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(cas));
            Assert.False(File.Exists(Path.Combine(transaction, "RECOVERY_REQUIRED.json")));
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(transaction, "transaction.json")));
            Assert.Equal("RolledBack", document.RootElement.GetProperty("State").GetString());
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void RecoveryRejectsJournalPathsOutsideRecordedRoots()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var game = Directory.CreateDirectory(Path.Combine(root, "game")).FullName;
            var transactions = Directory.CreateDirectory(Path.Combine(root, "transactions")).FullName;
            var transaction = Directory.CreateDirectory(Path.Combine(transactions, "case-escape")).FullName;
            var outside = Path.Combine(root, "outside.cas");
            File.WriteAllBytes(outside, new byte[] { 1, 2, 3, 4, 5 });
            WriteJournal(transaction, game, outside, 2, null, null);

            var result = DirectTransactionRecoveryService.RecoverPending(transactions);

            Assert.False(result.Success);
            Assert.Single(result.Failures);
            Assert.Contains("escapes", result.Failures[0], StringComparison.OrdinalIgnoreCase);
            Assert.Equal(5, new FileInfo(outside).Length);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void UserDialogsDoNotExposeRawExceptionMessages()
    {
        var repository = FindRepositoryRoot();
        var files = Directory.EnumerateFiles(Path.Combine(repository, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Contains("CM26.App", StringComparison.OrdinalIgnoreCase) ||
                           path.Contains("CM26.LegacyUI", StringComparison.OrdinalIgnoreCase));
        var violations = new List<string>();
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            if (Regex.IsMatch(text, @"MessageBox\.Show\s*\([^;]{0,500}\b(?:ex|exception)\.(?:Message|ToString)\b",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline))
                violations.Add(Path.GetRelativePath(repository, file));
        }
        Assert.True(violations.Count == 0,
            "Raw exception text is displayed by: " + string.Join(", ", violations));
    }

    private static void WriteJournal(string transaction, string game, string cas, long length,
        string? liveToc, string? backupToc)
    {
        var tocFiles = liveToc is null ? Array.Empty<object>() :
            new object[] { new { Live = liveToc, Backup = backupToc } };
        var journal = new
        {
            Product = "Creation Master 26",
            State = "RecoveryRequired",
            UpdatedUtc = DateTimeOffset.UtcNow,
            GameRoot = game,
            CasOriginalLengths = new Dictionary<string, long> { [cas] = length },
            TocFiles = tocFiles,
            Failure = "fault injection",
            RollbackErrors = new[] { "simulated" }
        };
        File.WriteAllText(Path.Combine(transaction, "transaction.json"),
            JsonSerializer.Serialize(journal, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string CreateTemporaryRoot()
    {
        var path = Path.Combine(Path.GetTempPath(), "cm26-recovery-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CM26.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("CM26 repository root was not found.");
    }
}
