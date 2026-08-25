using CM26.Application.Models;
using CM26.EngineBridge;

namespace CM26.Application.Services;

/// <summary>
/// Coordinates staged edits: validation via the engine, modified-field tracking, undo/redo.
/// Nothing here writes bytes; it only stages in-memory edits through the session.
/// </summary>
public sealed class PendingChangesService
{
    private readonly DatabaseSession _session;
    private readonly List<PendingChange> _changes = new();
    private readonly Stack<PendingChange> _redo = new();
    private readonly List<WorkspaceHistoryEntry> _history = new();
    private bool _hasStructuralChanges;

    public PendingChangesService(DatabaseSession session) => _session = session;

    public IReadOnlyList<PendingChange> Changes => _changes;
    public int Count => _changes.Count + (_hasStructuralChanges ? 1 : 0);
    public bool HasChanges => _changes.Count > 0 || _hasStructuralChanges;
    public bool CanUndo => _changes.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public IReadOnlyList<WorkspaceHistoryEntry> History => _history;

    public event EventHandler? Changed;

    /// <summary>Marks an insert/delete already staged by the native engine. Structural undo is not available.</summary>
    public void MarkStructuralChange()
    {
        _hasStructuralChanges = true;
        _changes.Clear();
        _redo.Clear();
        AddHistory("Structure", "A record was inserted or deleted; structural undo is unavailable.");
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Stage a validated edit. Returns the engine outcome; on success tracks the change.</summary>
    public EditOutcome Stage(string tableName, int rowIndex, string fieldName, string newValue)
    {
        var oldValue = _session.GetCell(tableName, rowIndex, fieldName);
        if (oldValue == newValue)
            return Ok(); // no-op

        var outcome = _session.StageEdit(tableName, rowIndex, fieldName, newValue);
        if (!outcome.Success) return outcome;

        var table = _session.GetTable(tableName);
        _changes.Add(new PendingChange
        {
            IsLocale = table?.IsLocale ?? false,
            TableName = tableName,
            RowIndex = rowIndex,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
        });
        AddHistory("Edit", $"{tableName}[{rowIndex}].{fieldName}: '{oldValue}' → '{newValue}'");
        _redo.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
        return outcome;
    }

    /// <summary>Undo the most recent staged change by restoring its previous value through the engine.</summary>
    public bool Undo()
    {
        if (_changes.Count == 0) return false;
        var change = _changes[^1];
        var outcome = _session.StageEdit(change.TableName, change.RowIndex, change.FieldName, change.OldValue);
        if (!outcome.Success) return false;
        _changes.RemoveAt(_changes.Count - 1);
        _redo.Push(change);
        AddHistory("Undo", change.Describe());
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool Redo()
    {
        if (_redo.Count == 0) return false;
        var change = _redo.Pop();
        var outcome = _session.StageEdit(change.TableName, change.RowIndex, change.FieldName, change.NewValue);
        if (!outcome.Success) return false;
        _changes.Add(change);
        AddHistory("Redo", change.Describe());
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool IsFieldModified(string tableName, int rowIndex, string fieldName) =>
        _changes.Any(c => c.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                          && c.RowIndex == rowIndex
                          && c.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Discards every staged edit for one row, restoring its staged values through
    /// the engine so the section can re-read the original state ("Revert" action).
    /// </summary>
    public void DiscardForRow(string tableName, int rowIndex)
    {
        var affected = _changes
            .Where(c => c.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) && c.RowIndex == rowIndex)
            .ToList();
        if (affected.Count == 0) return;
        foreach (var change in affected)
            _session.StageEdit(change.TableName, change.RowIndex, change.FieldName, change.OldValue);
        _changes.RemoveAll(c => c.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) && c.RowIndex == rowIndex);
        _redo.Clear();
        AddHistory("Revert row", $"Restored {affected.Count} field(s) in {tableName}[{rowIndex}].");
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Clears tracked state after a successful save (staged values remain in memory).</summary>
    public void MarkSaved()
    {
        var savedCount = Count;
        _changes.Clear();
        _redo.Clear();
        _hasStructuralChanges = false;
        AddHistory("Save", $"Committed {savedCount} staged change group(s) and reload-verified the database.");
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void AddHistory(string action, string description)
    {
        _history.Add(new WorkspaceHistoryEntry(DateTime.Now, action, description));
        if (_history.Count > 2000) _history.RemoveRange(0, _history.Count - 2000);
    }

    private static EditOutcome Ok() => new EditOutcome { Success = true, Message = "No change" };
}

public sealed record WorkspaceHistoryEntry(DateTime Timestamp, string Action, string Description);
