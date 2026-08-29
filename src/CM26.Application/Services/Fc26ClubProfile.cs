using System.Globalization;

namespace CM26.Application.Services;

public static class Fc26ClubProfile
{
    private static readonly CultureInfo UsCurrency = CultureInfo.GetCultureInfo("en-US");

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

    /// <summary>
    /// Formats the static <c>teams.clubworth</c> value the same way Deco's
    /// Team Details window does.  FC26 stores this field in thousands and the
    /// Deco view applies the 1.08 database-to-dollar scale before formatting it.
    /// </summary>
    public static string FormatDecoClubWorth(string? raw)
    {
        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var thousands) || thousands < 0)
            return "—";
        return (thousands * 1000m * 1.08m).ToString("C2", UsCurrency);
    }

    /// <summary>
    /// Returns Deco's transfer-budget projection for a static squads record.
    /// This is deliberately an estimate: the real editable budget belongs to a
    /// Career save and is not stored on the base <c>teams</c> row.
    /// </summary>
    public static decimal EstimateDecoTransferBudget(int clubWorth, int profitability)
    {
        var tier = Math.Max(1, Math.Min(10, profitability)) - 1;
        decimal[] baseOne = [975, 880, 815, 785, 770, 750, 650, 600, 580, 560];
        decimal[] baseThousand = [960120, 866160, 801360, 772200, 757080, 737640, 639360, 590760, 570240, 550800];
        if (clubWorth <= 0) return 0;
        if (clubWorth <= 1000)
        {
            if (clubWorth == 1) return baseOne[tier];
            if (clubWorth == 1000) return baseThousand[tier];
            var position = DecimalLog(clubWorth) / DecimalLog(1000m);
            return baseOne[tier] + (baseThousand[tier] - baseOne[tier]) * position;
        }

        int[] points = [50000, 100000, 250000, 500000];
        decimal[][] values =
        [
            [17280432, 13860288, 12015296, 10080234, 9270261, 7920180, 7200144, 6030126, 5760126, 5310144],
            [22920432, 20520432, 17820324, 15120270, 13500243, 11880216, 11016195, 10260054, 9180108, 8100108],
            [48600808, 40500476, 35100540, 30240216, 25650270, 22140250, 19980432, 18090486, 16740417, 14580298],
            [75602160, 62101888, 54001616, 48601192, 44550676, 38880540, 35640432, 30224059, 25380808, 19980918],
        ];
        if (clubWorth > points[3])
            return values[3][tier] * (clubWorth / (decimal)points[3]) * 0.85m;

        var upperIndex = 0;
        while (upperIndex < points.Length && clubWorth > points[upperIndex]) upperIndex++;
        var upperPoint = points[upperIndex];
        var lowerPoint = upperIndex == 0 ? 1000 : points[upperIndex - 1];
        var lowerValue = upperIndex == 0
            ? values[0][tier] * (lowerPoint / (decimal)points[0])
            : values[upperIndex - 1][tier];
        var upperValue = values[upperIndex][tier];
        var logPosition = (DecimalLog(clubWorth) - DecimalLog(lowerPoint)) /
            (DecimalLog(upperPoint) - DecimalLog(lowerPoint));
        return lowerValue + (upperValue - lowerValue) * logPosition;
    }

    public static string FormatDecoTransferBudget(string? clubWorthRaw, string? profitabilityRaw)
    {
        if (!int.TryParse(clubWorthRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var clubWorth) ||
            !int.TryParse(profitabilityRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var profitability) ||
            clubWorth <= 0)
            return "—";
        return EstimateDecoTransferBudget(clubWorth, profitability).ToString("C2", UsCurrency);
    }

    private static decimal DecimalLog(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        var exponent = 0;
        while (value >= 2m) { value /= 2m; exponent++; }
        while (value < 1m) { value *= 2m; exponent--; }
        var z = (value - 1m) / (value + 1m);
        var zSquared = z * z;
        var term = z;
        var sum = 0m;
        for (var divisor = 1; divisor <= 61; divisor += 2)
        {
            sum += term / divisor;
            term *= zSquared;
        }
        return 2m * sum + exponent * 0.6931471805599453094172321215m;
    }
}
