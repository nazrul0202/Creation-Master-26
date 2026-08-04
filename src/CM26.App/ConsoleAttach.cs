using System.Runtime.InteropServices;

namespace CM26.App;

/// <summary>
/// Creation Master 26 is a WinExe, so its stdout is not connected to the terminal
/// that launched it. Every "--" diagnostic mode is a console workflow, so attach to
/// the parent console (or allocate one) before writing results. Without this, output
/// is only visible when redirected to a file, which makes CI logs and user-reported
/// diagnostics unnecessarily painful.
/// </summary>
internal static class ConsoleAttach
{
    private const int AttachParentProcess = -1;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    private static bool _attached;

    public static void EnsureConsole()
    {
        if (_attached) return;
        _attached = true;
        try
        {
            // Prefer the launching console so output appears inline; fall back to a
            // new window when started from Explorer.
            if (!AttachConsole(AttachParentProcess)) AllocConsole();

            // Rebind stdout/stderr: they were opened against no console at startup.
            var stdout = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(stdout);
            var stderr = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetError(stderr);
        }
        catch (IOException)
        {
            // No console is obtainable (e.g. output already redirected); the original
            // stdout still works for redirection, so continue silently.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
