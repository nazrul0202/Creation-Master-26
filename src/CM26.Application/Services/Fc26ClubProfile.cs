using System.Globalization;

namespace CM26.Application.Services;

public static class Fc26ClubProfile
{
    public static string RatingLabel(string? raw)
    {
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)) return "—";
        return value switch
        {
            >= 9 => "Very High",
            >= 6 => "High",
            >= 4 => "Medium",
            >= 2 => "Low",
            _ => "Very Low",
        };
    }

    public static string FormatClubWorth(string? raw)
    {
        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var thousands)) return "—";
        var amount = thousands * 1000m;
        if (amount >= 1_000_000_000m) return $"{amount / 1_000_000_000m:0.##}B";
        if (amount >= 1_000_000m) return $"{amount / 1_000_000m:0.##}M";
        return amount.ToString("N0", CultureInfo.InvariantCulture);
    }
}
