namespace CM26.Application.Services;

/// <summary>
/// Shared application log used by the logic layer. Writes to the same
/// LocalAppData log file the UI shell uses, so all components stay in one
/// timeline. UI shells may also subscribe to OnWrite for on-screen diagnostics.
/// </summary>
public static class Cm26Log
{
    public static string LogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Creation Master 26", "cm26.log");

    /// <summary>Optional listener (e.g. the UI shell's diagnostics panel).</summary>
    public static Action<string>? OnWrite { get; set; }

    public static void Write(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, line + Environment.NewLine);
            OnWrite?.Invoke(line);
        }
        catch { /* logging must never break the caller */ }
    }
}