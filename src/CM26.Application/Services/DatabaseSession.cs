using CM26.Application.Models;
using CM26.EngineBridge;

namespace CM26.Application.Services;

/// <summary>
/// Owns the engine session and exposes loaded schema + records to the UI layer.
/// All database access flows through here; UI never touches the bridge directly.
/// </summary>
public sealed class DatabaseSession : IDisposable
{
    private EngineSession? _session;
    private List<DbTable>? _tables;
    private readonly Dictionary<string, DbTable> _byName = new(StringComparer.OrdinalIgnoreCase);

    public bool IsLoaded => _session?.IsLoaded == true;
    public string? DatabasePath => _session?.DatabasePath;
    public string? LocalePath => _session?.LocalePath;
    public string? MetaPath => _session?.MetaPath;
    public string? LoadedFolder { get; private set; }

    public IReadOnlyList<DbTable> Tables => _tables ?? (IReadOnlyList<DbTable>)Array.Empty<DbTable>();

    public event EventHandler? DatabaseChanged;

    /// <summary>Validate a candidate folder without fully loading row data.</summary>
    public LoadSummary ValidateFolder(string folder)
    {
        using var probe = new EngineSession();
        return probe.ValidateFolder(folder);
    }

    /// <summary>Fully load main + locale databases from a validated folder. Runs synchronously; call off the UI thread.</summary>
    public void Load(string folder)
    {
        var summary = ValidateFolder(folder);
        if (summary.State != LoadStateKind.Success)
            throw new InvalidOperationException($"Unsupported database folder: {summary.Message}");

        var session = new EngineSession();
        try
        {
            session.Load(summary.MetaPath, summary.DatabasePath, summary.LocalePath);
        }
        catch
        {
            session.Dispose();
            throw;
        }

        var old = _session;
        _session = session;
        LoadedFolder = folder;
        BuildSchema();
        old?.Dispose();
        DatabaseChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BuildSchema()
    {
        _tables = new List<DbTable>();
        _byName.Clear();
        if (_session == null) return;
        AddTables(_session.Main, isLocale: false);
        AddTables(_session.Locale, isLocale: true);
    }

    private void AddTables(NativeDatabaseHandle handle, bool isLocale)
    {
        foreach (var t in handle.GetTables())
        {
            var table = new DbTable
            {
                Name = t.Name,
                ShortName = t.ShortName,
                RowCount = t.RowCount,
                IsLocale = isLocale,
                Columns = t.Columns.Select(c => new DbColumn
                {
                    Name = c.Name,
                    ShortName = c.ShortName,
                    Kind = (int)c.Kind,
                    Depth = c.Depth,
                    RangeLow = c.RangeLow,
                    RangeHigh = c.RangeHigh,
                    IsWritable = c.IsWritable,
                }).ToList(),
            };
            _tables!.Add(table);
            // Locale tables share names with main schema entries rarely; keep first registration.
            if (!_byName.ContainsKey(table.Name))
                _byName[table.Name] = table;
        }
    }

    public DbTable? GetTable(string name) => _byName.TryGetValue(name, out var t) ? t : null;

    /// <summary>Refreshes table row counts after a staged structural insert/delete.</summary>
    public void RefreshSchema()
    {
        if (_session == null) return;
        BuildSchema();
        DatabaseChanged?.Invoke(this, EventArgs.Empty);
    }

    public DbRecord? GetRecord(string tableName, int rowIndex)
    {
        if (_session == null) return null;
        var table = GetTable(tableName);
        if (table == null) return null;
        var handle = table.IsLocale ? _session.Locale : _session.Main;
        var row = handle.GetRow(tableName, rowIndex);
        return row == null ? null : new DbRecord { Index = row.Index, Values = row.Values.ToList() };
    }

    /// <summary>Current in-memory cell value (reflects staged edits).</summary>
    public string GetCell(string tableName, int rowIndex, string fieldName)
    {
        if (_session == null) return string.Empty;
        var table = GetTable(tableName);
        if (table == null) return string.Empty;
        return _session.GetCellText(table.IsLocale, tableName, rowIndex, fieldName) ?? string.Empty;
    }

    /// <summary>Raw bytes of a string cell (read-only), for codepage-specific decoding (e.g. CP1252).</summary>
    public byte[] GetCellBytes(string tableName, int rowIndex, string fieldName)
    {
        if (_session == null) return Array.Empty<byte>();
        var table = GetTable(tableName);
        if (table == null) return Array.Empty<byte>();
        return _session.GetCellBytes(table.IsLocale, tableName, rowIndex, fieldName) ?? Array.Empty<byte>();
    }

    /// <summary>Stage one validated edit through the engine (no bytes written).</summary>
    public EditOutcome StageEdit(string tableName, int rowIndex, string fieldName, string value)
    {
        if (_session == null)
            return Fail("Database not loaded");
        var table = GetTable(tableName);
        if (table == null)
            return Fail($"Table '{tableName}' not found");
        return _session.StageEdit(table.IsLocale, tableName, rowIndex, fieldName, value);
    }

    /// <summary>Stages a duplicate of an existing record. The native writer rebuilds layout and CRCs on save.</summary>
    public EditOutcome DuplicateRow(string tableName, int rowIndex)
    {
        if (_session == null) return Fail("Database not loaded");
        var table = GetTable(tableName);
        if (table == null) return Fail($"Table '{tableName}' not found");
        return _session.DuplicateRow(table.IsLocale, tableName, rowIndex);
    }

    /// <summary>Stages deletion of an existing record. Relationship cleanup remains the caller's responsibility.</summary>
    public EditOutcome DeleteRow(string tableName, int rowIndex)
    {
        if (_session == null) return Fail("Database not loaded");
        var table = GetTable(tableName);
        if (table == null) return Fail($"Table '{tableName}' not found");
        return _session.DeleteRow(table.IsLocale, tableName, rowIndex);
    }

    /// <summary>CM16-style parent deletion: remove dependent links and clear optional references first.</summary>
    public EditOutcome DeleteRowWithRelationships(string tableName, int rowIndex)
    {
        if (_session == null) return Fail("Database not loaded");
        var table = GetTable(tableName);
        if (table == null) return Fail($"Table '{tableName}' not found");
        return _session.DeleteRowWithRelationships(table.IsLocale, tableName, rowIndex);
    }

    /// <summary>Returns native primary-key and foreign-key violations for the main database.</summary>
    public IReadOnlyList<string> ValidateIntegrity() =>
        _session == null ? new[] { "Database not loaded" } : _session.ValidateIntegrity(locale: false).ToList();

    /// <summary>Write a validated copy of main and/or locale through the engine.</summary>
    public void SaveCopy(bool locale, string outputPath)
    {
        if (_session == null) throw new InvalidOperationException("Database not loaded");
        _session.SaveCopy(locale, outputPath);
    }

    /// <summary>Reload-verify a written file via the engine (read-only). Throws on failure.</summary>
    public void VerifyFile(string metaPath, string databasePath, bool encryptedLocale)
    {
        using var probe = new EngineSession();
        probe.VerifyFile(metaPath, databasePath, encryptedLocale);
    }

    private static EditOutcome Fail(string message)
    {
        var o = new EditOutcome { Success = false, Message = message };
        return o;
    }

    public void Dispose()
    {
        _session?.Dispose();
        _session = null;
    }
}
