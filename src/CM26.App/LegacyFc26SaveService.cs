using System.Text.Json;
using CM26.Application.Services;

namespace CM26.App;

/// <summary>Applies edits collected from the original x86 CM16 forms through the native FC26 engine.</summary>
internal static class LegacyFc26SaveService
{
    internal static string Apply(string planPath, bool applyDirect = true)
    {
        if (!File.Exists(planPath)) throw new FileNotFoundException("FC26 change plan was not found.", planPath);
        var plan = JsonSerializer.Deserialize<ChangePlan>(File.ReadAllText(planPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("FC26 change plan is empty.");
        var gameRoot = FrostbiteAssetSession.ResolveGameRoot(
            string.IsNullOrWhiteSpace(plan.GameRoot) ? SettingsService.FC26GameFolder : plan.GameRoot)
            ?? throw new InvalidOperationException("FC26 installation was not detected.");
        var assets = new FrostbiteAssetSession();
        assets.Open(gameRoot);
        if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
        var mods = new LegacyAssetModService();
        mods.Open(assets.Fingerprint);
        if (plan.Changes.Count == 0 && !mods.HasChanges)
            return "No database or asset changes to save.";
        if (applyDirect)
        {
            var backup = GameBackupService.EnsureCreated(gameRoot);
            if (!backup.Success) throw new InvalidOperationException(backup.Message);
        }

        string? stagingFolder = null;
        var workspace = Fc26WorkspaceService.Open(assets);
        using var session = new DatabaseSession();
        session.Load(workspace.DatabaseFolder);
        var failures = new List<string>();
        foreach (var change in plan.Changes)
        {
            var outcome = session.StageEdit(change.TableName, change.RowIndex, change.FieldName, change.Value);
            if (!outcome.Success)
                failures.Add($"{change.TableName}[{change.RowIndex}].{change.FieldName}: {outcome.Message}");
        }
        if (failures.Count > 0)
            throw new InvalidOperationException("FC26 rejected the edit plan: " +
                string.Join("; ", failures.Take(8)) +
                (failures.Count > 8 ? $" (+{failures.Count - 8} more)" : string.Empty));

        if (plan.Changes.Count > 0)
        {
            stagingFolder = Path.Combine(Path.GetTempPath(), "CM26-legacy-save-" + Guid.NewGuid().ToString("N"));
            var save = new SaveService(session).SaveToDirectory(stagingFolder);
            if (!save.Success) throw new InvalidOperationException(save.Message);
            if (!string.IsNullOrWhiteSpace(session.MetaPath) && File.Exists(session.MetaPath))
                File.Copy(session.MetaPath, Path.Combine(stagingFolder, Path.GetFileName(session.MetaPath)), overwrite: true);
        }

        if (stagingFolder != null) using (var verification = new DatabaseSession())
        {
            verification.Load(stagingFolder);
            var mismatches = plan.Changes.Where(change =>
                !string.Equals(verification.GetCell(change.TableName, change.RowIndex, change.FieldName),
                    change.Value, StringComparison.Ordinal)).Take(8).ToList();
            if (mismatches.Count > 0)
                throw new InvalidOperationException("FC26 reload verification failed: " +
                    string.Join("; ", mismatches.Select(change =>
                        $"{change.TableName}[{change.RowIndex}].{change.FieldName}")));
        }

        if (!applyDirect)
            return stagingFolder != null
                ? $"Staged and reload-verified {plan.Changes.Count} FC26 database change(s) at {stagingFolder}"
                : $"Validated {mods.Count} staged asset file(s); no database changes were requested.";

        var assetCount = mods.Count;
        if (stagingFolder != null) mods.StageDatabase(stagingFolder, includeLocale: false);
        var directPlan = mods.WriteDirectPlan();
        var applied = assets.ApplyDirect(directPlan);
        if (!applied.Success) throw new InvalidOperationException(applied.Message);
        mods.MarkApplied();
        return $"Saved and verified {plan.Changes.Count} database change(s) and {assetCount} asset file(s). {applied.Message}";
    }

    private sealed class ChangePlan
    {
        public List<Change> Changes { get; set; } = new();
        public string GameRoot { get; set; } = string.Empty;
    }

    private sealed class Change
    {
        public string TableName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
