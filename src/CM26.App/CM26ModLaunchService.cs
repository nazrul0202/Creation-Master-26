using System.Diagnostics;
using System.Text.Json;

namespace CM26.App;

/// <summary>Crash-recoverable Data/Patch swap, mirroring mod-manager overlay lifecycle.</summary>
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
        if (File.Exists(state)) return Recover(root);
        try
        {
            File.WriteAllText(state, JsonSerializer.Serialize(new { active = true, createdUtc = DateTimeOffset.UtcNow }));
            Directory.Move(Path.Combine(root, "Data"), Path.Combine(root, "CM26OriginalData"));
            Directory.Move(Path.Combine(root, "Patch"), Path.Combine(root, "CM26OriginalPatch"));
            Directory.Move(Path.Combine(overlay, "Data"), Path.Combine(root, "Data"));
            Directory.Move(Path.Combine(overlay, "Patch"), Path.Combine(root, "Patch"));
            return (true, "CM26 mod overlay is active. Launch FC26, then restore original data after it exits.");
        }
        catch (Exception ex) { return Recover(root) is var recovery && recovery.Success ? (false, "Activation failed and original data was restored: " + ex.Message) : (false, "Activation failed: " + ex.Message); }
    }

    public static (bool Success, string Message) Restore(string root, bool recovering = false)
    {
        if (IsGameRunning()) return (false, "Close FC26 before restoring original data.");
        var state = Path.Combine(root, Marker); var overlay = CM26ModOverlayService.OverlayRoot(root);
        if (!Directory.Exists(Path.Combine(root, "CM26OriginalData"))) return (true, "Original FC26 Data is already active.");
        try
        {
            if (Directory.Exists(Path.Combine(root, "Data"))) Directory.Move(Path.Combine(root, "Data"), Path.Combine(overlay, "Data"));
            if (Directory.Exists(Path.Combine(root, "Patch"))) Directory.Move(Path.Combine(root, "Patch"), Path.Combine(overlay, "Patch"));
            Directory.Move(Path.Combine(root, "CM26OriginalData"), Path.Combine(root, "Data"));
            Directory.Move(Path.Combine(root, "CM26OriginalPatch"), Path.Combine(root, "Patch"));
            if (File.Exists(state)) File.Delete(state);
            return (true, recovering ? "Recovered original FC26 Data/Patch after an interrupted mod launch." : "Original FC26 Data/Patch restored.");
        }
        catch (Exception ex) { return (false, "Unable to restore original FC26 data: " + ex.Message); }
    }
    private static bool IsGameRunning() => new[] { "FC26", "FC26_Trial", "FC26_Showcase" }.Any(n => Process.GetProcessesByName(n).Length > 0);
}
