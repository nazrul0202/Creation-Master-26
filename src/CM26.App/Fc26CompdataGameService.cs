using System.Data;
using CM26.Application.Services;

namespace CM26.App;

/// <summary>Bridges the editable Compdata tables to FC26's packed legacy TXT assets.</summary>
internal static class Fc26CompdataGameService
{
    internal const string LogicalPrefix =
        "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/careermode_closedbeta/";

    internal static readonly string[] SheetNames =
    [
        "advancement", "activeteams", "compids", "compobj", "initteams",
        "objectives", "schedule", "settings", "standings", "tasks", "weather",
    ];

    internal static string ExtractInstalled()
    {
        var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder)
            ?? throw new InvalidOperationException("FC26 installation was not detected.");
        var assets = new FrostbiteAssetSession();
        assets.Open(gameRoot);
        if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);

        var output = Path.Combine(Path.GetTempPath(), "cm26-installed-compdata-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        try
        {
            foreach (var sheet in SheetNames)
            {
                var logicalPath = LogicalPrefix + sheet + ".txt";
                var exported = assets.ExportLegacyAsset(logicalPath)
                    ?? throw new FileNotFoundException("FC26 Compdata asset was not found: " + logicalPath);
                File.Copy(exported, Path.Combine(output, sheet + ".txt"), overwrite: true);
            }
            return output;
        }
        catch
        {
            try { Directory.Delete(output, recursive: true); } catch { }
            throw;
        }
    }

    internal static int StageForDirectSave(IReadOnlyDictionary<string, DataTable> tables)
    {
        var errors = CompdataSchema.Validate(tables).Where(issue => issue.IsError).ToArray();
        if (errors.Length > 0)
            throw new InvalidDataException("Compdata validation failed: " + string.Join(" | ", errors.Take(8)
                .Select(issue => $"{issue.Sheet} row {issue.Row}: {issue.Message}")));
        foreach (var required in new[] { "compobj", "compids", "initteams", "schedule", "settings", "standings" })
            if (!tables.ContainsKey(required)) throw new InvalidDataException($"Required Compdata section '{required}' is missing.");

        var gameRoot = FrostbiteAssetSession.ResolveGameRoot(SettingsService.FC26GameFolder)
            ?? throw new InvalidOperationException("FC26 installation was not detected.");
        var assets = new FrostbiteAssetSession();
        assets.Open(gameRoot);
        if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
        var mods = new LegacyAssetModService();
        mods.Open(assets.Fingerprint);
        var output = Path.Combine(Path.GetTempPath(), "cm26-compdata-stage-" + Guid.NewGuid().ToString("N"));
        try
        {
            CompdataWorkbookService.ExportTextFiles(output, tables);
            VerifyTextRoundTrip(output, tables);
            var targets = new List<(string LogicalPath, string SourcePath)>();
            foreach (var sheet in SheetNames.Where(tables.ContainsKey))
            {
                var logicalPath = LogicalPrefix + sheet + ".txt";
                _ = assets.ExportLegacyAsset(logicalPath)
                    ?? throw new FileNotFoundException("FC26 Compdata target was not found: " + logicalPath);
                targets.Add((logicalPath, Path.Combine(output, sheet + ".txt")));
            }
            // Do not persist a partial Compdata set: every exported file and every
            // FC26 target is proven before the replacement set enters Save state,
            // then all replacement metadata is committed in one workspace write.
            mods.StageFilesAtomically(targets.Select(target => (target.LogicalPath, target.SourcePath)));
            return targets.Count;
        }
        finally
        {
            try { if (Directory.Exists(output)) Directory.Delete(output, recursive: true); }
            catch (Exception ex) { Program.Log("Compdata staging cleanup failed: " + ex.Message); }
        }
    }

    private static void VerifyTextRoundTrip(string output,
        IReadOnlyDictionary<string, DataTable> expected)
    {
        var verify = new CompdataWorkbookService();
        verify.OpenFromGameFolder(output);
        foreach (var sheet in SheetNames.Where(expected.ContainsKey))
        {
            var source = expected[sheet];
            var saved = verify.ReadSheet(sheet);
            if (saved.Rows.Count != source.Rows.Count || saved.Columns.Count != source.Columns.Count)
                throw new InvalidDataException($"Compdata export verification failed for '{sheet}': row or column count changed.");
            for (var row = 0; row < source.Rows.Count; row++)
            for (var column = 0; column < source.Columns.Count; column++)
            {
                var before = NormalizeCell(source.Rows[row][column]);
                var after = NormalizeCell(saved.Rows[row][column]);
                if (!string.Equals(before, after, StringComparison.Ordinal))
                    throw new InvalidDataException(
                        $"Compdata export verification failed for '{sheet}' row {row + 1}, column {column + 1}.");
            }
        }
    }

    private static string NormalizeCell(object value)
    {
        var text = Convert.ToString(value)?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(text) ? string.Empty : text;
    }
}
