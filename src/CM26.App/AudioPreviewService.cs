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
        Check(mciSendString($"open \"{escaped}\" alias {Alias}", null, 0, IntPtr.Zero),
            "The selected audio format is not supported by Windows Media Control.");
        Check(mciSendString($"play {Alias}", null, 0, IntPtr.Zero),
            "Audio playback could not be started.");
    }

    public static void Stop()
    {
        mciSendString($"stop {Alias}", null, 0, IntPtr.Zero);
        mciSendString($"close {Alias}", null, 0, IntPtr.Zero);
    }

    private static void Check(int result, string message)
    {
        if (result == 0) return;
        Stop();
        throw new InvalidOperationException(message + $" (MCI {result})");
    }
}
