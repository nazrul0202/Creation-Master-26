namespace CM26.Application.Services;

/// <summary>FC26 playstyle catalogue: the 34 playstyles stored as a bitmask across
/// trait1/trait2 (and icontrait1/icontrait2 for the icon "+" variants). Single
/// source of truth shared by the legacy canvas and the WPF studio.</summary>
public static class PlaystyleCatalog
{
    public static readonly string[] Names =
    [
        "Finesse Shot", "Power Shot", "Dead Ball", "Chip Shot", "Power Header", "Pinged Pass", "Long Ball Pass", "Tiki Taka",
        "Incisive Pass", "Whipped Pass", "First Touch", "Technical", "Rapid", "Quick Step", "Trickster", "Press Proven",
        "Flair", "Relentless", "Trivela", "Block", "Intercept", "Anticipate", "Slide Tackle", "Bruiser", "Jockey", "Aerial",
        "Acrobatic", "Far Reach", "Footwork", "Cross Claimer", "Rush Out", "Deflector", "1v1 Close Down", "Long Throw"
    ];
}
