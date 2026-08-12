using System.Runtime.InteropServices;
using System.Text;

namespace CM26.App;

internal static class AudioPreviewService
{
    private const string Alias = "cm26_audio_preview";

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    private static extern int mciSendString(
        string command, StringBuilder? returnValue, int returnLength, IntPtr callback);

    public static void Play(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file was not found.", filePath);
        Stop();
        var escaped = filePath.Replace("\"", "\"\"");
        var result = mciSendString($"open \"{escaped}\" type mpegvideo alias {Alias}", null, 0, IntPtr.Zero);
        if (result != 0)
            result = mciSendString($"open \"{escaped}\" alias {Alias}", null, 0, IntPtr.Zero);
        if (result != 0)
        {
            Stop();
            throw new InvalidOperationException($"The selected audio format is not supported by Windows Media Control. (MCI {result})");
        }
        result = mciSendString($"play {Alias}", null, 0, IntPtr.Zero);
        if (result != 0)
        {
            Stop();
            throw new InvalidOperationException($"Audio playback could not be started. (MCI {result})");
        }
    }

    public static void Stop()
    {
        try { mciSendString($"stop {Alias}", null, 0, IntPtr.Zero); }
        catch (Exception ex) { Program.Log("Audio preview stop failed: " + ex.Message); }
        try { mciSendString($"close {Alias}", null, 0, IntPtr.Zero); }
        catch (Exception ex) { Program.Log("Audio preview close failed: " + ex.Message); }
    }
}
