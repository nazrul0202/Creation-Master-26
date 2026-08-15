using System.Net.Http;
using System.Reflection;

using CM26.Application.Services;

namespace CM26.App;

/// <summary>
/// Lightweight "check for updates" helper. It reads a small JSON manifest from a
/// configured URL (e.g. a GitHub release or your own endpoint) and reports whether
/// a newer public version exists. Failures degrade to "unable to check" instead of
/// crashing. Version parsing is tolerant of "1.0.23", "1.0.23.0", "v1.0.23".
/// </summary>
public static class UpdateChecker
{
    /// <summary>Manifest URL. Override at build/run time by editing this constant.</summary>
    public const string ManifestUrl =
        "https://raw.githubusercontent.com/nazrul0202/Creation-Master-26/main/version.json";

    private static readonly Version? LocalVersion =
        Assembly.GetExecutingAssembly().GetName().Version;

    /// <summary>Result of an update check.</summary>
    public sealed record CheckResult(bool IsNewerAvailable, string? LatestVersion, string? Message);

    /// <summary>True if a manifest was already fetched recently (throttle repeated checks).</summary>
    public static bool CheckedRecently => HasCheckedRecently();

    /// <summary>
    /// Check the remote manifest for a newer version. Returns null when the check
    /// could not be completed (offline / endpoint changed / bad data).
    /// </summary>
    public static async Task<CheckResult?> CheckAsync(CancellationToken cancellationToken = default)
    {
        if (LocalVersion == null) return new CheckResult(false, null, "Unknown local version.");
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CreationMaster26/1.0");
            var json = await client.GetStringAsync(ManifestUrl, cancellationToken);
            var latest = ParseLatestVersion(json);
            if (latest == null) return null;
            // Compare on MAJOR.MINOR.PATCH only. The assembly carries a 4-part
            // version (1.0.25.0) while the manifest publishes 3 parts (1.0.25);
            // an unspecified component sorts as -1, so comparing them directly
            // would make an equal release look older or newer than it is.
            var newer = Normalize(latest) > Normalize(LocalVersion);
            SettingsService.LastUpdateCheckTicks = DateTime.UtcNow.Ticks.ToString();
            return new CheckResult(newer, latest.ToString(3), newer
                ? Localization.T("Update.Available") : Localization.T("Update.Current"));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            return null;
        }
    }

    /// <summary>Reduces a version to MAJOR.MINOR.PATCH with no unspecified components.</summary>
    private static Version Normalize(Version version) =>
        new(version.Major, version.Minor, Math.Max(version.Build, 0));

    private static bool HasCheckedRecently()    {
        if (!long.TryParse(SettingsService.LastUpdateCheckTicks, out var ticks)) return false;
        var last = new DateTime(ticks, DateTimeKind.Utc);
        return (DateTime.UtcNow - last).TotalHours < 24;
    }

    /// <summary>Naively extract a version string from a JSON manifest of the form {"version":"1.0.24"}.</summary>
    private static Version? ParseLatestVersion(string json)
    {
        try
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                json, "\"version\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success) return null;
            var raw = match.Groups[1].Value.TrimStart('v', 'V');
            return Version.TryParse(raw, out var v) ? v : null;
        }
        catch { return null; }
    }
}
