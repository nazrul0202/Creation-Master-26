namespace CM26.Application.Services;

/// <summary>
/// DATABASE-NATIVE, READ-ONLY player-name source. Resolves names strictly from the loaded
/// database folder: players.*nameid → playernames.nameid → playernames.name → eng_us.DB locale.
///
/// No external TXT/CSV/XLSX export, no internet, no hard-coded or generated names, no fixed
/// development paths. The selected database folder is the ONLY authoritative source.
///
/// HONEST CONTRACT: in the current FC26 database the playernames.name column is an EA-ciphered
/// 0xC4 placeholder and the locale sourcetext is protected by EA's second-layer text cipher whose
/// runtime key is NOT present in the database files. This source therefore yields no readable name
/// today; it never fabricates one. The moment a decoded source is present (EA runtime key, or a
/// database whose playernames.name is literal), resolution flows through unchanged.
/// </summary>
public sealed class DatabasePlayerNameSource
{
    private readonly DatabaseSession _session;
    private readonly LocaleStringReader _locale;
    private readonly IReadOnlyDictionary<int, (string First, string Surname, string Common)>? _sessionNameOverrides;

    // nameid -> decoded readable name (only entries that decode to a real, non-placeholder string)
    private readonly Dictionary<int, string> _nameById = new();
    // playerid -> edited-name override (editedplayernames), when populated
    private readonly Dictionary<int, (string first, string last, string common)> _edited = new();

    public int DecodedNameCount { get; private set; }
    public int PlaceholderNameCount { get; private set; }
    public int LocaleStringCount => _locale.StringCount;

    /// <summary>True when at least one readable name was decoded from the loaded database.</summary>
    public bool IsAvailable => DecodedNameCount > 0;

    public DatabasePlayerNameSource(DatabaseSession session,
        IReadOnlyDictionary<int, (string First, string Surname, string Common)>? sessionNameOverrides = null)
    {
        _session = session;
        _sessionNameOverrides = sessionNameOverrides;
        _locale = new LocaleStringReader(session);
        Build();
        // After building from engine-decoded values (which are wrong for compressed strings),
        // overlay the correct Huffman-decoded names from the raw database file.
        LoadNativeHuffmanNames();
    }

    /// <summary>
    /// Load playernames using the verified C# Huffman decoder that reads raw bytes directly
    /// from fifa_ng_db.db, bypassing the engine's defective Huff::read(). This is the
    /// authoritative source for player names in the current database.
    /// </summary>
    private void LoadNativeHuffmanNames()
    {
        var dbPath = _session.DatabasePath;
        var metaPath = _session.MetaPath;
        if (string.IsNullOrEmpty(dbPath) || string.IsNullOrEmpty(metaPath)) return;
        try
        {
            var nativeNames = NativeHuffmanDecoder.BuildPlayerNameMap(dbPath, metaPath);
            foreach (var (id, name) in nativeNames)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _nameById[id] = name;
                }
            }
            DecodedNameCount = _nameById.Count;
        }
        catch
        {
            // best effort — if the native decoder fails, the honest fallback remains.
        }
    }

    /// <summary>The locale reader (indexed once per session) for diagnostics.</summary>
    public LocaleStringReader Locale => _locale;

    private void Build()
    {
        _nameById.Clear();
        _edited.Clear();
        DecodedNameCount = 0;
        PlaceholderNameCount = 0;

        LoadEditedNames();
        LoadSessionNameOverrides();
        // DBM-compatible precedence: dcplayernames (more specific) then playernames.
        LoadNameTable("dcplayernames", overwrite: true);
        LoadNameTable("playernames", overwrite: false);
    }

    private void LoadEditedNames()
    {
        var t = _session.GetTable("editedplayernames");
        if (t == null || t.RowCount == 0) return;
        int pid = Col(t, "playerid"), fn = Col(t, "firstname"), sn = Col(t, "surname"), cn = Col(t, "commonname");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("editedplayernames", r); if (rec == null) continue;
            if (int.TryParse(rec.Get(pid), out var id))
            {
                (string first, string last, string common) e = (rec.Get(fn), rec.Get(sn), rec.Get(cn));
                if (!string.IsNullOrWhiteSpace(e.first) || !string.IsNullOrWhiteSpace(e.last) || !string.IsNullOrWhiteSpace(e.common))
                    _edited[id] = e;
            }
        }
    }

    private void LoadSessionNameOverrides()
    {
        if (_sessionNameOverrides == null) return;
        foreach (var (playerId, name) in _sessionNameOverrides)
        {
            if (playerId <= 0) continue;
            if (string.IsNullOrWhiteSpace(name.First) && string.IsNullOrWhiteSpace(name.Surname) && string.IsNullOrWhiteSpace(name.Common)) continue;
            _edited[playerId] = (name.First, name.Surname, name.Common);
        }
    }

    private void LoadNameTable(string tableName, bool overwrite)
    {
        var t = _session.GetTable(tableName);
        if (t == null) return;
        int idCol = Col(t, "nameid"), nameCol = Col(t, "name");
        if (idCol < 0 || nameCol < 0) return;
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord(tableName, r); if (rec == null) continue;
            if (!int.TryParse(rec.Get(idCol), out var id)) continue;
            var decoded = NameTextDecoder.Decode(_session.GetCellBytes(tableName, r, "name"));
            if (decoded != null)
            {
                if (overwrite || !_nameById.ContainsKey(id))
                {
                    _nameById[id] = decoded;
                    DecodedNameCount++;
                }
            }
            else PlaceholderNameCount++;
        }
    }

    /// <summary>Resolve a single name part by nameid; null when not decodable from the database.</summary>
    public string? NameById(int nameId) =>
        nameId > 0 && _nameById.TryGetValue(nameId, out var n) ? n : null;

    /// <summary>
    /// Resolve all four name parts for a player. Parts that cannot be decoded are null.
    /// </summary>
    public PlayerNameParts? Resolve(int playerId, int firstNameId, int lastNameId, int commonNameId, int knownAsId = 0)
    {
        string? first = null, last = null, common = null, knownAs = null;
        if (_edited.TryGetValue(playerId, out var e))
        {
            first = NullIfEmpty(e.first);
            last = NullIfEmpty(e.last);
            common = NullIfEmpty(e.common);
        }
        first ??= NameById(firstNameId);
        last ??= NameById(lastNameId);
        common ??= NameById(commonNameId);
        knownAs = NameById(knownAsId) ?? common ?? Combine(first, last);

        return new PlayerNameParts(first, last, common, knownAs);
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
    private static string? Combine(string? a, string? b)
    {
        var c = $"{a} {b}".Trim();
        return c.Length == 0 ? null : c;
    }

    private static int Col(Models.DbTable t, string name)
    {
        for (int i = 0; i < t.Columns.Count; i++)
            if (t.Columns[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }
}
