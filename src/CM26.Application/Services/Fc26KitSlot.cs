namespace CM26.Application.Services;

/// <summary>
/// Canonical FC26 team-kit slot mapping shared by the database list, Frostbite
/// preview and direct asset importer.  The legacy database uses 2 for the
/// goalkeeper kit and 3 for the third outfield kit.
/// </summary>
public static class Fc26KitSlot
{
    public static string Label(int code) => code switch
    {
        0 => "Home",
        1 => "Away",
        2 => "Goalkeeper",
        3 => "Third",
        4 => "Fourth",
        5 => "Referee",
        30 => "GK Home",
        31 => "GK Away",
        32 => "GK Third",
        93 => "Training Home",
        94 => "Training Away",
        _ => $"Kit {code}",
    };

    /// <summary>Returns the installed Frostbite folder token for core club kits.</summary>
    public static bool TryGetAssetVariant(int code, out string variant)
    {
        variant = code switch
        {
            0 => "home",
            1 => "away",
            2 => "gk",
            3 => "third",
            _ => string.Empty,
        };
        return variant.Length > 0;
    }

    public static string? BuildColourTexturePath(int teamId, int kitType, int kitId)
    {
        if (teamId <= 0 || kitId <= 0 || !TryGetAssetVariant(kitType, out var variant)) return null;
        return $"content/character/kit/{teamId}/{variant}_1_0/jersey_{kitId}_1_0_color.dds";
    }
}
