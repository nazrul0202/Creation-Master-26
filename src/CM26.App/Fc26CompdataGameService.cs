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
        foreach (var sheet in SheetNames)
        {
            var logicalPath = LogicalPrefix + sheet + ".txt";
            var exported = assets.ExportLegacyAsset(logicalPath)
                ?? throw new FileNotFoundException("FC26 Compdata asset was not found: " + logicalPath);
            File.Copy(exported, Path.Combine(output, sheet + ".txt"), overwrite: true);
        }
        return output;
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
            var staged = 0;
            foreach (var sheet in SheetNames.Where(tables.ContainsKey))
            {
                var logicalPath = LogicalPrefix + sheet + ".txt";
                _ = assets.ExportLegacyAsset(logicalPath)
                    ?? throw new FileNotFoundException("FC26 Compdata target was not found: " + logicalPath);
                mods.StageFile(logicalPath, Path.Combine(output, sheet + ".txt"));
                staged++;
            }
            return staged;
        }
        finally
        {
            try { if (Directory.Exists(output)) Directory.Delete(output, recursive: true); }
            catch (Exception ex) { Program.Log("Compdata staging cleanup failed: " + ex.Message); }
        }
    }
}
