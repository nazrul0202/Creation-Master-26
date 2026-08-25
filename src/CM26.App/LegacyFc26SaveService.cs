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
        if (string.IsNullOrWhiteSpace(plan.GameRoot) && Directory.Exists(plan.DatabaseFolder))
            return ApplyExtracted(plan);
        var gameRoot = FrostbiteAssetSession.ResolveGameRoot(
            string.IsNullOrWhiteSpace(plan.GameRoot) ? SettingsService.FC26GameFolder : plan.GameRoot)
            ?? throw new InvalidOperationException("FC26 installation was not detected.");
        var assets = new FrostbiteAssetSession();
        assets.Open(gameRoot);
        if (!assets.IsAvailable) throw new InvalidOperationException(assets.Status);
        var mods = new LegacyAssetModService();
        mods.Open(assets.Fingerprint);
        if (plan.Changes.Count == 0 && plan.StructuralChanges.Count == 0 && !mods.HasChanges)
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
        ApplyStructuralChanges(session, plan.StructuralChanges, failures);
        foreach (var change in plan.Changes)
        {
            var outcome = session.StageEdit(ActualTable(change.TableName), change.RowIndex, change.FieldName, change.Value);
            if (!outcome.Success)
                failures.Add($"{change.TableName}[{change.RowIndex}].{change.FieldName}: {outcome.Message}");
        }
        if (failures.Count > 0)
            throw new InvalidOperationException("FC26 rejected the edit plan: " +
                string.Join("; ", failures.Take(8)) +
                (failures.Count > 8 ? $" (+{failures.Count - 8} more)" : string.Empty));

        if (plan.Changes.Count > 0 || plan.StructuralChanges.Count > 0)
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
                !string.Equals(verification.GetCell(ActualTable(change.TableName), change.RowIndex, change.FieldName),
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
        if (stagingFolder != null) mods.StageDatabase(stagingFolder, includeLocale: HasLocaleChanges(plan));
        var directPlan = mods.WriteDirectPlan();
        var applied = assets.ApplyDirect(directPlan);
        if (!applied.Success) throw new InvalidOperationException(applied.Message);
        mods.MarkApplied();
        return $"Saved and verified {plan.Changes.Count} database change(s) and {assetCount} asset file(s). {applied.Message}";
    }

    private static string ApplyExtracted(ChangePlan plan)
    {
        if (plan.Changes.Count == 0 && plan.StructuralChanges.Count == 0) return "No extracted-database changes to save.";
        using var session = new DatabaseSession();
        session.Load(plan.DatabaseFolder);
        var failures = new List<string>();
        ApplyStructuralChanges(session, plan.StructuralChanges, failures);
        failures.AddRange(StageChanges(session, plan.Changes));
        if (failures.Count > 0)
            throw new InvalidOperationException("FC26 rejected the edit plan: " + string.Join("; ", failures.Take(8)));
        var saved = new SaveService(session).SaveToSourceFolder();
        if (!saved.Success) throw new InvalidOperationException(saved.Message);
        return $"Saved and reload-verified {plan.Changes.Count} field edit(s) and {plan.StructuralChanges.Count} structural edit(s) to the extracted database. {saved.Message}";
    }

    private static void ApplyStructuralChanges(DatabaseSession session, IReadOnlyList<StructuralChange> structural, List<string> failures)
    {
        foreach (var change in structural.Where(value => value.Kind.Equals("duplicate", StringComparison.OrdinalIgnoreCase)))
        {
            var outcome = session.DuplicateRow(ActualTable(change.TableName), change.RowIndex);
            if (!outcome.Success) failures.Add($"Duplicate {change.TableName}[{change.RowIndex}]: {outcome.Message}");
            else session.RefreshSchema();
        }
        // Deletions are deliberately isolated by the legacy UI. Process descending
        // indexes so a future multi-delete plan cannot invalidate a later index.
        foreach (var change in structural.Where(value => value.Kind.Equals("delete", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(value => value.TableName).ThenByDescending(value => value.RowIndex))
        {
            var outcome = session.DeleteRowWithRelationships(ActualTable(change.TableName), change.RowIndex);
            if (!outcome.Success) failures.Add($"Delete {change.TableName}[{change.RowIndex}]: {outcome.Message}");
            else session.RefreshSchema();
        }
    }

    private static List<string> StageChanges(DatabaseSession session, IEnumerable<Change> changes)
    {
        var failures = new List<string>();
        foreach (var change in changes)
        {
            var outcome = session.StageEdit(ActualTable(change.TableName), change.RowIndex, change.FieldName, change.Value);
            if (!outcome.Success)
                failures.Add($"{change.TableName}[{change.RowIndex}].{change.FieldName}: {outcome.Message}");
        }
        return failures;
    }

    private static string ActualTable(string tableName) =>
        tableName.StartsWith("locale::", StringComparison.OrdinalIgnoreCase)
            ? tableName.Substring("locale::".Length) : tableName;

    private static bool HasLocaleChanges(ChangePlan plan) =>
        plan.Changes.Any(change => change.TableName.StartsWith("locale::", StringComparison.OrdinalIgnoreCase)) ||
        plan.StructuralChanges.Any(change => change.TableName.StartsWith("locale::", StringComparison.OrdinalIgnoreCase));

    private sealed class ChangePlan
    {
        public List<Change> Changes { get; set; } = new();
        public string GameRoot { get; set; } = string.Empty;
        public string DatabaseFolder { get; set; } = string.Empty;
        public List<StructuralChange> StructuralChanges { get; set; } = new();
    }

    private sealed class StructuralChange
    {
        public string Kind { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
    }

    private sealed class Change
    {
        public string TableName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
