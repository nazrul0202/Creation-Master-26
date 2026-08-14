using System.Diagnostics;
using System.Text.Json;

namespace CM26.App;

/// <summary>
/// Validates a CM26 data-path overlay.  FET launches FC26 with a separate
/// data path; moving the installed Data/Patch folders is both unnecessary and
/// unsafe because the EA/Steam launcher can inspect those folders while it
/// starts the game.
/// </summary>
public static class CM26ModLaunchService
{
    private const string Marker = "cm26-mod-launch-state.json";
    public static (bool Success, string Message) Recover(string root) => Restore(root, recovering: true);

    public static (bool Success, string Message) Activate(string root)
    {
        if (IsGameRunning()) return (false, "Close FC26 before activating CM26 mods.");
        var overlay = CM26ModOverlayService.OverlayRoot(root);
        if (!Directory.Exists(Path.Combine(overlay, "Data")) || !Directory.Exists(Path.Combine(overlay, "Patch")))
            return (false, "Build CM26ModData before launching mods.");
        var state = Path.Combine(root, Marker);
        try
        {
            File.WriteAllText(state, JsonSerializer.Serialize(new
            {
                active = true,
                dataPath = Path.GetFileName(overlay),
                createdUtc = DateTimeOffset.UtcNow
            }));
            return (true, "CM26ModData is ready. FC26 will be launched with -dataPath CM26ModData; original Data/Patch remains untouched.");
        }
        catch (Exception ex) { return (false, "Unable to prepare CM26 mod launch: " + ex.Message); }
    }

    public static (bool Success, string Message) Restore(string root, bool recovering = false)
    {
        var state = Path.Combine(root, Marker);
        try
        {
            if (File.Exists(state)) File.Delete(state);
            return (true, recovering ? "Cleared an interrupted CM26 mod launch marker; original Data/Patch was never changed." : "CM26 mod launch marker cleared. Original Data/Patch was never changed.");
        }
        catch (Exception ex) { return (false, "Unable to clear CM26 mod launch state: " + ex.Message); }
    }
    private static bool IsGameRunning() => new[] { "FC26", "FC26_Trial", "FC26_Showcase" }.Any(n => Process.GetProcessesByName(n).Length > 0);

    /// <summary>
    /// Launches FC26 the way its storefront expects.  FC26.exe is a Steam DRM
    /// build: starting the executable directly (as older CM26 versions did)
    /// makes it exit immediately with code 100010 and no window.  Launching
    /// through the steam://run/&lt;appid&gt; protocol passes the DRM check and
    /// forwards command-line arguments such as -dataPath CM26ModData.
    /// </summary>
    public static (bool Success, string Message) Launch(string root, string? arguments = null)
    {
        root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var appId = TryReadSteamAppId(root);
        if (appId is not null)
        {
            var protocol = BuildSteamProtocol(appId, arguments);
            try
            {
                using (Process.Start(new ProcessStartInfo(protocol) { UseShellExecute = true })) { }
                return (true, $"FC26 was launched through Steam (appid {appId})" +
                    (string.IsNullOrWhiteSpace(arguments) ? string.Empty : $" with arguments: {arguments.Trim()}") + ".");
            }
            catch (Exception ex)
            {
                return (false, "CM26 could not launch FC26 through Steam: " + ex.Message +
                    "\n\nLaunch it from Steam manually, then try again.");
            }
        }

        var exe = Path.Combine(root, "FC26.exe");
        if (!File.Exists(exe))
            return (false, "FC26.exe was not found in the game installation.");
        try
        {
            using (Process.Start(new ProcessStartInfo(exe)
            {
                UseShellExecute = true,
                WorkingDirectory = root,
                Arguments = arguments ?? string.Empty,
            })) { }
            return (true, "FC26 was launched directly from the installation folder.");
        }
        catch (Exception ex)
        {
            return (false, "CM26 could not launch FC26: " + ex.Message);
        }
    }

    /// <summary>steam://run/&lt;appid&gt;//&lt;args&gt; with arguments URL-encoded (%20 for spaces).</summary>
    internal static string BuildSteamProtocol(string appId, string? arguments)
    {
        return "steam://run/" + appId + "//" + (string.IsNullOrWhiteSpace(arguments)
            ? string.Empty
            : Uri.EscapeDataString(arguments));
    }

    internal static string? ResolveSteamAppId(string root)
    {
        var full = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return TryReadSteamAppId(full);
    }

    /// <summary>
    /// Launches FC26 with a single -dataPath overlay argument (the standard
    /// FIFA Mod Manager / FET convention, e.g. -dataPath FIFAModData) through
    /// Steam, falling back to the storefront-less direct launch when no Steam
    /// app id is present.
    /// </summary>
    public static (bool Success, string Message) LaunchWithDataPath(string root, string dataPath)
    {
        var name = Path.GetFileName(dataPath.Trim().TrimEnd('/', '\\'));
        if (string.IsNullOrWhiteSpace(name))
            return (false, "A mod data folder name is required (for example FIFAModData or CM26ModData).");
        return Launch(root, "-dataPath " + name);
    }

    private static string? TryReadSteamAppId(string root)
    {
        var appIdFile = Path.Combine(root, "steam_appid.txt");
        if (!File.Exists(appIdFile)) return null;
        var text = File.ReadAllText(appIdFile).Trim();
        return text.Length is > 0 and <= 16 && text.All(char.IsAsciiDigit) ? text : null;
    }
}
