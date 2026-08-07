using System.Runtime.InteropServices;

namespace CM26.App.Theming;

/// <summary>
/// Safe, best-effort native dark-mode theming (Windows 10 1809+ / 11).
///
/// WinForms has no managed API for dark scrollbars or a dark title bar, so the
/// immersive dark-mode attribute must be set on the underlying HWNDs. These calls
/// are entirely cosmetic and are guarded so that any native failure only disables
/// the enhancement (never a crash). This mirrors the project's policy of removing
/// rather than risking native theming that could raise a fatal 0xc0000005.
/// </summary>
internal static class NativeTheme
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaUseImmersiveDarkModeBefore2004 = 19;

    private static bool _dwmFailed;

    /// <summary>
    /// Applies immersive dark/light mode to an HWND to match <see cref="Theme.IsDark"/>.
    /// Returns false if the OS does not support immersive mode theming.
    /// </summary>
    public static bool TryApplyImmersiveMode(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return false;
        if (_dwmFailed) return false;

        var ok = TrySetDwmAttribute(hwnd, DwmwaUseImmersiveDarkMode, Theme.IsDark ? 1 : 0)
              || TrySetDwmAttribute(hwnd, DwmwaUseImmersiveDarkModeBefore2004, Theme.IsDark ? 1 : 0);
        if (!ok && System.Environment.OSVersion.Version.Build < 17763)
            _dwmFailed = true; // pre-1809 has no immersive dark mode at all
        return ok;
    }

    /// <summary>
    /// Attempts to put an HWND (and its child scrollbars) into immersive dark mode.
    /// Safe to call from any thread; returns false if dark mode is unsupported.
    /// </summary>
    public static bool TryApplyImmersiveDarkMode(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return false;
        if (_dwmFailed) return false;

        var ok = TrySetDwmAttribute(hwnd, DwmwaUseImmersiveDarkMode)
              || TrySetDwmAttribute(hwnd, DwmwaUseImmersiveDarkModeBefore2004);
        if (!ok && System.Environment.OSVersion.Version.Build < 17763)
            _dwmFailed = true; // pre-1809 has no immersive dark mode at all
        return ok;
    }

    private static bool TrySetDwmAttribute(IntPtr hwnd, int attribute)
        => TrySetDwmAttribute(hwnd, attribute, 1);

    private static bool TrySetDwmAttribute(IntPtr hwnd, int attribute, int desiredValue)
    {
        try
        {
            var value = desiredValue;
            var hr = DwmSetWindowAttribute(hwnd, attribute, ref value, Marshal.SizeOf<int>());
            return hr >= 0;
        }
        catch
        {
            _dwmFailed = true;
            return false;
        }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);
}
