using System.Text;

namespace CM26.Application.Services;

/// <summary>
/// Decode a playernames.name value to a readable name, or null when the payload is a
/// placeholder/undecodable. Accepts UTF-8 or CP1252 real text; rejects the 0xC4 placeholder fill
/// and EA-ciphered payloads (which never form valid name text). Never fabricates a name.
/// </summary>
public static class NameTextDecoder
{
    private static readonly Encoding Cp1252 = CreateCp1252();

    private static Encoding CreateCp1252()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1252);
    }

    /// <summary>Returns the decoded name, or null when the bytes are not a readable name.</summary>
    public static string? Decode(byte[] bytes)
    {
        if (bytes.Length == 0) return null;
        // placeholder fill
        if (bytes.All(b => b == 0x20)) return null;      // spaces
        // FC26's unavailable-name marker can be mixed with a few residual bytes
        // (for example C4 C4 C4 44 C4). It is never a real CP1252 player name.
        if (bytes.Any(b => b == 0xC4)) return null;
        // A real name is mostly letters/space/punct. Try UTF-8 then CP1252.
        foreach (var enc in new[] { new UTF8Encoding(false, false), Cp1252 })
        {
            string text;
            try { text = enc.GetString(bytes).Trim('\0', ' '); }
            catch { continue; }
            if (text.Length == 0) continue;
            if (LooksLikeName(text)) return RepairUtf8Mojibake(text);
        }
        return null;
    }

    /// <summary>
    /// Some FC26 name payloads contain UTF-8 text that was previously converted through
    /// Windows-1252 (for example "AarÃ³n" instead of "Aarón").  Repair only the well-known
    /// marker characters and only when the recovered text is still a plausible name.
    /// This is deliberately conservative: valid Unicode names such as Åke remain untouched.
    /// </summary>
    private static string RepairUtf8Mojibake(string value)
    {
        var current = value;
        for (var pass = 0; pass < 2 && HasMojibakeMarker(current); pass++)
        {
            try
            {
                var bytes = Cp1252.GetBytes(current);
                var repaired = new UTF8Encoding(false, true).GetString(bytes);
                if (repaired == current || !LooksLikeName(repaired)) break;
                current = repaired;
            }
            catch (DecoderFallbackException) { break; }
        }
        return current;
    }

    private static bool HasMojibakeMarker(string value) =>
        value.IndexOf('\u00c3') >= 0 || value.IndexOf('\u00c2') >= 0 || value.IndexOf('\u00e2') >= 0;

    private static bool LooksLikeName(string s)
    {
        int letters = 0, vowels = 0;
        foreach (var c in s)
        {
            if (char.IsLetter(c))
            {
                letters++;
                if ("aeiouAEIOUàáâäèéêëìíîïòóôöùúûü".IndexOf(c) >= 0) vowels++;
            }
            else if (c == ' ' || c == '-' || c == '\'' || c == '.') { /* allowed */ }
            else return false; // digits/symbols => not a name
        }
        // must be mostly letters and contain at least one vowel to be a plausible name
        return letters >= s.Length * 0.6 && vowels >= 1;
    }
}
