namespace CM26.Application.Services;

/// <summary>
/// READ-ONLY indexed reader for the decrypted FC26 locale (eng_us.DB → LanguageStrings1/2),
/// loaded through the protected engine via <see cref="DatabaseSession"/>.
///
/// The index (hashid → decoded sourcetext, and stringid → hashid) is built ONCE per database
/// session; lookups are O(1). The full locale file is never re-scanned per player.
///
/// HONEST CONTRACT: this reader returns exactly what the engine decodes. In the current FC26
/// database the locale sourcetext is protected by EA's second-layer text cipher (a ~44-symbol
/// alphabet), so the decoded payloads are NOT readable names. This class never fabricates text —
/// callers must treat undecodable payloads as "unavailable".
/// </summary>
public sealed class LocaleStringReader
{
    private readonly Dictionary<int, string> _textByHash = new();
    private readonly Dictionary<string, int> _hashByStringId = new(StringComparer.Ordinal);

    public int StringCount { get; }
    public bool IsLoaded { get; }

    public LocaleStringReader(DatabaseSession session)
    {
        foreach (var tableName in new[] { "LanguageStrings1", "LanguageStrings2" })
        {
            var t = session.GetTable(tableName);
            if (t == null) continue;
            int sid = Col(t, "stringid"), txt = Col(t, "sourcetext"), hash = Col(t, "hashid");
            if (txt < 0 || hash < 0) continue;
            for (int r = 0; r < t.RowCount; r++)
            {
                var rec = session.GetRecord(tableName, r);
                if (rec == null) continue;
                if (!int.TryParse(rec.Get(hash), out var h)) continue;
                var text = rec.Get(txt);
                if (!string.IsNullOrEmpty(text))
                {
                    _textByHash.TryAdd(h, text);
                    StringCount++;
                }
                if (sid >= 0)
                {
                    var key = rec.Get(sid);
                    if (!string.IsNullOrEmpty(key)) _hashByStringId.TryAdd(key, h);
                }
            }
        }
        IsLoaded = _textByHash.Count > 0;
    }

    /// <summary>Decoded locale text for a hashid, or null when absent.</summary>
    public string? TextByHash(int hashId) =>
        _textByHash.TryGetValue(hashId, out var s) ? s : null;

    /// <summary>Decoded locale text for a stringid, or null when absent.</summary>
    public string? TextByStringId(string stringId) =>
        _hashByStringId.TryGetValue(stringId, out var h) ? TextByHash(h) : null;

    private static int Col(Models.DbTable t, string name)
    {
        for (int i = 0; i < t.Columns.Count; i++)
            if (t.Columns[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }
}
