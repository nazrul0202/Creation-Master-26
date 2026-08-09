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
}
