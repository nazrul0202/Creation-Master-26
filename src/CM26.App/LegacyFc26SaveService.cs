using System.Text.Json;
using CM26.Application.Services;

namespace CM26.App;

/// <summary>Applies edits collected from the original x86 CM16 forms through the native FC26 engine.</summary>
internal static class LegacyFc26SaveService
{
    internal static string Apply(string planPath, bool applyDirect = true, string? exportDestination = null)
    {
        if (!File.Exists(planPath)) throw new FileNotFoundException("FC26 change plan was not found.", planPath);
        var plan = JsonSerializer.Deserialize<ChangePlan>(File.ReadAllText(planPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("FC26 change plan is empty.");
        if (string.IsNullOrWhiteSpace(plan.GameRoot) && Directory.Exists(plan.DatabaseFolder))
        {
            if (!string.IsNullOrWhiteSpace(exportDestination))
                throw new InvalidOperationException("FIFA Mod export requires an installed FC26 source. Save extracted-database changes to their session instead.");
            return ApplyExtracted(plan, applyDirect);
        }
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
        var rowMap = ApplyStructuralChanges(session, plan.StructuralChanges, failures);
        foreach (var change in plan.Changes)
        {
            var tableName = ActualTable(change.TableName);
            if (!rowMap.TryTranslate(tableName, change.RowIndex, out var actualRow))
            {
                failures.Add($"{change.TableName}[{change.RowIndex}].{change.FieldName}: row was removed or could not be mapped after structural changes");
                continue;
            }
            var outcome = session.StageEdit(tableName, actualRow, change.FieldName, change.Value);
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
                !rowMap.TryTranslate(ActualTable(change.TableName), change.RowIndex, out var actualRow) ||
                !string.Equals(verification.GetCell(ActualTable(change.TableName), actualRow, change.FieldName),
                    change.Value, StringComparison.Ordinal)).Take(8).ToList();
            if (mismatches.Count > 0)
                throw new InvalidOperationException("FC26 reload verification failed: " +
                    string.Join("; ", mismatches.Select(change =>
                        $"{change.TableName}[{change.RowIndex}].{change.FieldName}")));
        }

        if (!applyDirect && string.IsNullOrWhiteSpace(exportDestination))
            return stagingFolder != null
                ? $"Staged and reload-verified {plan.Changes.Count} FC26 database change(s) at {stagingFolder}"
                : $"Validated {mods.Count} staged asset file(s); no database changes were requested.";

        var assetCount = mods.Count;
        if (stagingFolder != null) mods.StageDatabase(stagingFolder, includeLocale: HasLocaleChanges(plan));
        var directPlan = mods.WriteDirectPlan();
        if (!string.IsNullOrWhiteSpace(exportDestination))
        {
            var exported = assets.ExportFetMod(directPlan, exportDestination);
            if (!exported.Success) throw new InvalidOperationException(exported.Message);
            return $"Exported {plan.Changes.Count} database change(s) and {assetCount} asset file(s). {exported.Message}";
        }
        var applied = assets.ApplyDirect(directPlan);
        if (!applied.Success) throw new InvalidOperationException(applied.Message);
        mods.MarkApplied();
        return $"Saved and verified {plan.Changes.Count} database change(s) and {assetCount} asset file(s). {applied.Message}";
    }

    private static string ApplyExtracted(ChangePlan plan, bool applyDirect)
    {
        if (plan.Changes.Count == 0 && plan.StructuralChanges.Count == 0) return "No extracted-database changes to save.";
        using var session = new DatabaseSession();
        session.Load(plan.DatabaseFolder);
        var failures = new List<string>();
        var rowMap = ApplyStructuralChanges(session, plan.StructuralChanges, failures);
        failures.AddRange(StageChanges(session, rowMap, plan.Changes));
        if (failures.Count > 0)
            throw new InvalidOperationException("FC26 rejected the edit plan: " + string.Join("; ", failures.Take(8)));
        if (!applyDirect)
        {
            var stagingFolder = Path.Combine(Path.GetTempPath(), "CM26-extracted-verify-" + Guid.NewGuid().ToString("N"));
            var staged = new SaveService(session).SaveToDirectory(stagingFolder);
            if (!staged.Success) throw new InvalidOperationException(staged.Message);
            if (!string.IsNullOrWhiteSpace(session.MetaPath) && File.Exists(session.MetaPath))
                File.Copy(session.MetaPath, Path.Combine(stagingFolder, Path.GetFileName(session.MetaPath)), overwrite: true);
            using var verification = new DatabaseSession();
            verification.Load(stagingFolder);
            var mismatches = plan.Changes.Where(change =>
                !rowMap.TryTranslate(ActualTable(change.TableName), change.RowIndex, out var actualRow) ||
                !string.Equals(verification.GetCell(ActualTable(change.TableName), actualRow, change.FieldName),
                    change.Value, StringComparison.Ordinal)).Take(8).ToList();
            if (mismatches.Count > 0)
                throw new InvalidOperationException("FC26 extracted reload verification failed: " +
                    string.Join("; ", mismatches.Select(change => $"{change.TableName}[{change.RowIndex}].{change.FieldName}")));
            return $"Staged and reload-verified {plan.Changes.Count} field edit(s) and {plan.StructuralChanges.Count} structural edit(s) at {stagingFolder}";
        }
        var saved = new SaveService(session).SaveToSourceFolder();
        if (!saved.Success) throw new InvalidOperationException(saved.Message);
        return $"Saved and reload-verified {plan.Changes.Count} field edit(s) and {plan.StructuralChanges.Count} structural edit(s) to the extracted database. {saved.Message}";
    }

    private static RowIndexMap ApplyStructuralChanges(DatabaseSession session,
        IReadOnlyList<StructuralChange> structural, List<string> failures)
    {
        var rowMap = RowIndexMap.Create(session);
        foreach (var change in structural.Where(value => value.Kind.Equals("duplicate", StringComparison.OrdinalIgnoreCase)))
        {
            var tableName = ActualTable(change.TableName);
            if (!rowMap.TryTranslate(tableName, change.RowIndex, out var actualSource))
            {
                failures.Add($"Duplicate {change.TableName}[{change.RowIndex}]: source row could not be mapped after an earlier structural change.");
                continue;
            }
            var target = change.TargetRowIndex >= 0
                ? change.TargetRowIndex
                : rowMap.NextPlannedIndex(tableName);
            var outcome = session.DuplicateRow(tableName, actualSource);
            if (!outcome.Success)
            {
                failures.Add($"Duplicate {change.TableName}[{change.RowIndex}]: {outcome.Message}");
                continue;
            }
            rowMap.InsertAfter(tableName, target, actualSource + 1);
            session.RefreshSchema();
        }
        foreach (var change in structural.Where(value => value.Kind.Equals("append", StringComparison.OrdinalIgnoreCase)))
        {
            var tableName = ActualTable(change.TableName);
            var table = session.GetTable(tableName);
            if (table == null)
            {
                failures.Add($"Append {change.TableName}: table was not found.");
                continue;
            }
            var target = change.TargetRowIndex >= 0
                ? change.TargetRowIndex
                : rowMap.NextPlannedIndex(tableName);
            var actualRow = table.RowCount;
            var outcome = session.AppendRow(tableName);
            if (!outcome.Success)
            {
                failures.Add($"Append {change.TableName}: {outcome.Message}");
                continue;
            }
            rowMap.Append(tableName, target, actualRow);
            session.RefreshSchema();
        }
        // Deletions are deliberately isolated by the legacy UI. Process descending
        // indexes so a future multi-delete plan cannot invalidate a later index.
        foreach (var change in structural.Where(value => value.Kind.Equals("delete", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(value => value.TableName).ThenByDescending(value => value.RowIndex))
        {
            var tableName = ActualTable(change.TableName);
            if (!rowMap.TryTranslate(tableName, change.RowIndex, out var actualRow))
            {
                failures.Add($"Delete {change.TableName}[{change.RowIndex}]: source row could not be mapped after an earlier structural change.");
                continue;
            }
            var outcome = session.DeleteRowWithRelationships(tableName, actualRow);
            if (!outcome.Success)
            {
                failures.Add($"Delete {change.TableName}[{change.RowIndex}]: {outcome.Message}");
                continue;
            }
            rowMap.RemoveAt(tableName, change.RowIndex, actualRow);
            session.RefreshSchema();
        }
        return rowMap;
    }

    private static List<string> StageChanges(DatabaseSession session, RowIndexMap rowMap, IEnumerable<Change> changes)
    {
        var failures = new List<string>();
        foreach (var change in changes)
        {
            var tableName = ActualTable(change.TableName);
            if (!rowMap.TryTranslate(tableName, change.RowIndex, out var actualRow))
            {
                failures.Add($"{change.TableName}[{change.RowIndex}].{change.FieldName}: row was removed or could not be mapped after structural changes");
                continue;
            }
            var outcome = session.StageEdit(tableName, actualRow, change.FieldName, change.Value);
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

    /// <summary>
    /// Keeps the editor's append-oriented row numbers aligned with the native
    /// engine.  DatabaseEngine.DuplicateRow inserts immediately after the
    /// source row; the legacy snapshot deliberately appends the clone so the
    /// UI can keep stable row references while a wizard is open.  Every insert
    /// therefore shifts later native rows and must be translated before a field
    /// is staged (and again when reload verification reads it).
    /// </summary>
    private sealed class RowIndexMap
    {
        private readonly Dictionary<string, Dictionary<int, int>> _rows =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _nextPlanned =
            new(StringComparer.OrdinalIgnoreCase);

        internal static RowIndexMap Create(DatabaseSession session)
        {
            var result = new RowIndexMap();
            foreach (var table in session.Tables)
            {
                var indexes = new Dictionary<int, int>();
                for (var row = 0; row < table.RowCount; row++) indexes[row] = row;
                result._rows[table.Name] = indexes;
                result._nextPlanned[table.Name] = table.RowCount;
            }
            return result;
        }

        internal bool TryTranslate(string tableName, int plannedRow, out int actualRow)
        {
            actualRow = -1;
            return _rows.TryGetValue(tableName, out var indexes) &&
                   indexes.TryGetValue(plannedRow, out actualRow);
        }

        internal int NextPlannedIndex(string tableName)
        {
            if (!_nextPlanned.TryGetValue(tableName, out var next))
                next = 0;
            _nextPlanned[tableName] = next + 1;
            return next;
        }

        internal void InsertAfter(string tableName, int targetPlanned, int actualInserted)
        {
            if (!_rows.TryGetValue(tableName, out var indexes))
                _rows[tableName] = indexes = new Dictionary<int, int>();

            foreach (var key in indexes.Keys.ToArray())
                if (indexes[key] >= actualInserted) indexes[key]++;
            indexes[targetPlanned] = actualInserted;
            if (!_nextPlanned.TryGetValue(tableName, out var next) || targetPlanned >= next)
                _nextPlanned[tableName] = targetPlanned + 1;
        }

        internal void Append(string tableName, int targetPlanned, int actualInserted)
        {
            if (!_rows.TryGetValue(tableName, out var indexes))
                _rows[tableName] = indexes = new Dictionary<int, int>();
            indexes[targetPlanned] = actualInserted;
            if (!_nextPlanned.TryGetValue(tableName, out var next) || targetPlanned >= next)
                _nextPlanned[tableName] = targetPlanned + 1;
        }

        internal void RemoveAt(string tableName, int plannedRow, int actualRow)
        {
            if (!_rows.TryGetValue(tableName, out var indexes)) return;
            indexes.Remove(plannedRow);
            foreach (var key in indexes.Keys.ToArray())
                if (indexes[key] > actualRow) indexes[key]--;
        }
    }

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
        public int TargetRowIndex { get; set; } = -1;
    }

    private sealed class Change
    {
        public string TableName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
