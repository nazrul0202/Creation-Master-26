using System.Globalization;

namespace CM26.Application.Services;

/// <summary>
/// Converts the database's day-count date representation to and from a
/// readable ISO calendar date. The epoch is shared by player birth and join dates.
/// </summary>
public static class FifaDateConverter
{
    private static readonly DateTime Epoch = new(1582, 10, 14);

    public static bool TryToIso(string? rawValue, out string isoDate)
    {
        isoDate = string.Empty;
        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days) ||
            days < 0)
            return false;
        try
        {
            isoDate = Epoch.AddDays(days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public static bool TryFromIso(string? isoDate, out string rawValue)
    {
        rawValue = string.Empty;
        if (!DateTime.TryParseExact(isoDate?.Trim(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
            date < Epoch)
            return false;
        rawValue = ((int)(date.Date - Epoch).TotalDays).ToString(CultureInfo.InvariantCulture);
        return true;
    }
}
