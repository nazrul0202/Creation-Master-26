using System;

namespace CreationMaster;

/// <summary>FC26 kit slot rules used by the x86 classic shell.</summary>
internal static class Fc26KitSlots
{
    internal static string Label(int code)
    {
        switch (code)
        {
            case 0: return "Home";
            case 1: return "Away";
            case 2: return "Goalkeeper";
            case 3: return "Third";
            case 4: return "Fourth";
            case 5: return "Referee";
            case 30: return "GK Home";
            case 31: return "GK Away";
            case 32: return "GK Third";
            case 93: return "Training Home";
            case 94: return "Training Away";
            default: return "Kit " + code;
        }
    }

    internal static bool TryGetAssetVariant(int code, out string variant)
    {
        switch (code)
        {
            case 0: variant = "home"; return true;
            case 1: variant = "away"; return true;
            case 2: variant = "gk"; return true;
            case 3: variant = "third"; return true;
            default: variant = string.Empty; return false;
        }
    }

    internal static string ColourTexturePath(int teamId, int kitType, int kitId)
    {
        if (teamId <= 0 || kitId <= 0 || !TryGetAssetVariant(kitType, out var variant)) return null;
        return $"content/character/kit/{teamId}/{variant}_1_0/jersey_{kitId}_1_0_color.dds";
    }
}
