namespace CM26.Application.Services;

/// <summary>
/// Read-only, DATABASE-NATIVE player-name resolution for FC26.
///
/// Names are resolved strictly from the loaded database folder (players.*nameid → playernames →
/// eng_us.DB locale) via <see cref="DatabasePlayerNameSource"/>. There is NO external TXT/CSV/XLSX
/// dependency, no internet lookup, no hard-coded or generated names, and no fixed development path.
///
/// Honest behaviour:
///  - resolves all four name parts (first/last/common/known-as) from the database when decodable;
///  - never fabricates a name and never shows a raw numeric key as a name;
///  - uses a single indexed cache built once per session (no per-player locale scans);
///  - when the database's names are EA-ciphered (the current FC26 database), resolution yields no
///    readable name and the documented "Player {playerId}" fallback is used.
/// </summary>
public sealed class PlayerNameService
{
    private readonly DatabasePlayerNameSource _source;

    public PlayerNameService(DatabasePlayerNameSource source)
    {
        _source = source;
    }

    /// <summary>The underlying database-native source (for diagnostics / provenance).</summary>
    public DatabasePlayerNameSource Source => _source;

    /// <summary>Number of nameids that decoded to a readable name from the loaded database.</summary>
    public int DecodableNameCount => _source.DecodedNameCount;
    /// <summary>Number of playernames entries that were placeholder/undecodable.</summary>
    public int PlaceholderNameCount => _source.PlaceholderNameCount;
    /// <summary>True when at least one readable name was decoded from the database.</summary>
    public bool NamesDecodable => _source.IsAvailable;

    /// <summary>Resolve a single name part by nameid; null when not decodable.</summary>
    public string? NameById(int nameId) => _source.NameById(nameId);

    /// <summary>All four name parts for a player. Parts that cannot be decoded are null.</summary>
    public PlayerNameParts Resolve(int playerId, int firstNameId, int lastNameId, int commonNameId, int knownAsId = 0) =>
        _source.Resolve(playerId, firstNameId, lastNameId, commonNameId, knownAsId)
        ?? new PlayerNameParts(null, null, null, null);

    /// <summary>Display name with honest fallback (never a raw key, never fabricated).</summary>
    public string DisplayName(int playerId, int firstNameId, int lastNameId, int commonNameId)
    {
        var parts = Resolve(playerId, firstNameId, lastNameId, commonNameId);
        return parts.KnownAs ?? $"Player {playerId}";
    }
}

public sealed record PlayerNameParts(string? FirstName, string? LastName, string? CommonName, string? KnownAs)
{
    /// <summary>True when at least one part was decodable (so we don't show a bare ID).</summary>
    public bool HasAnyName => FirstName != null || LastName != null || CommonName != null;
}
