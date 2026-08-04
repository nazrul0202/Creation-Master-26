using System.Globalization;
using System.Resources;

namespace CM26.App;

/// <summary>
/// UI localization. Strings are looked up by a BCP-47 culture tag ("en", "es",
/// "pt-BR", …) from embedded .resx resources named
/// <c>CM26.App.Resources.Strings.{culture}.resx</c>. Fallback order: exact culture
/// → two-letter parent → English. A missing key returns the English value, then
/// the key itself. Add new languages by dropping a Strings.{culture}.resx file
/// into <c>src/CM26.App/Resources/</c>.
/// </summary>
public static class Localization
{
    private static CultureInfo _culture = SafeUiCulture();
    private static readonly object _gate = new();
    private static readonly ResourceManager EnManager =
        new("CM26.App.Resources.Strings", typeof(Localization).Assembly);

    public static CultureInfo Culture
    {
        get => _culture;
        set { lock (_gate) { _culture = value ?? new CultureInfo("en"); } }
    }

    /// <summary>Set the active culture from a BCP-47 tag; falls back to English on error.</summary>
    public static void SetCulture(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) { Culture = SafeUiCulture(); return; }
        try { Culture = CultureInfo.GetCultureInfo(tag); }
        catch (CultureNotFoundException) { Culture = new CultureInfo("en"); }
    }

    /// <summary>Translate a key into the active culture, falling back to English then the key.</summary>
    public static string T(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;
        var culture = Culture;
        if (!culture.Name.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            var localized = Lookup(culture, key);
            if (localized != null) return localized;
        }
        return English(key) ?? key;
    }

    private static string? Lookup(CultureInfo culture, string key)
    {
        var exact = TryGet(culture, key);
        if (exact != null) return exact;
        if (culture.Name != culture.TwoLetterISOLanguageName &&
            !culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            var parent = TryGet(new CultureInfo(culture.TwoLetterISOLanguageName), key);
            if (parent != null) return parent;
        }
        return null;
    }

    private static string? TryGet(CultureInfo culture, string key)
    {
        try
        {
            // Uses a satellite assembly if one exists, otherwise the base resource.
            return EnManager.GetString(key, culture);
        }
        catch { return null; }
    }

    private static string? English(string key)
    {
        try { return EnManager.GetString(key, CultureInfo.InvariantCulture); }
        catch { return null; }
    }

    private static CultureInfo SafeUiCulture()
    {
        try { return CultureInfo.CurrentUICulture; }
        catch { return new CultureInfo("en"); }
    }
}
