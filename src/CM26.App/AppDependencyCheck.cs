using System.Diagnostics;
using System.Windows.Forms;

namespace CM26.App;

/// <summary>
/// Startup dependency check for the Lite (framework-dependent) build.
///
/// The Full Portable build is self-contained and carries its own .NET 8 runtime, so this check
/// is compiled as a no-op there (<c>CM26_SELFCONTAINED</c>). The Lite build relies on a
/// system-installed <b>.NET 8 Desktop Runtime (x64)</b>; if it is missing we show a clear,
/// actionable message (with the official download link) instead of letting the apphost emit a
/// generic "You must install .NET" failure.
/// </summary>
internal static class AppDependencyCheck
{
    private const string DownloadUrl = "https://dotnet.microsoft.com/download/dotnet/8.0";

    /// <summary>Returns true when the app may start; false (after informing the user) when not.</summary>
    public static bool EnsureDesktopRuntime()
    {
#if CM26_SELFCONTAINED
        // Self-contained: the runtime is bundled; nothing to check.
        return true;
#else
        if (DesktopRuntimePresent()) return true;
        ShowMissingRuntimeMessage();
        return false;
#endif
    }

    private static bool DesktopRuntimePresent()
    {
        try
        {
            // Look for an installed Microsoft.WindowsDesktop.App 8.x runtime in the standard
            // system location for the running process architecture (x64 build -> Program Files).
            // The desktop runtime has no "Microsoft.WindowsDesktop.App.dll"; a reliable marker of a
            // *desktop* (WinForms/WPF) runtime is System.Windows.Forms.dll / PresentationFramework.dll.
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "dotnet", "shared", "Microsoft.WindowsDesktop.App");
            if (Directory.Exists(baseDir))
            {
                foreach (var dir in Directory.EnumerateDirectories(baseDir))
                {
                    var name = Path.GetFileName(dir);
                    if (!name.StartsWith("8.", StringComparison.Ordinal)) continue;
                    if (File.Exists(Path.Combine(dir, "System.Windows.Forms.dll")) ||
                        File.Exists(Path.Combine(dir, "PresentationFramework.dll")))
                        return true;
                }
            }
        }
        catch (Exception ex) { Program.Log($"[CM26] Runtime directory probe failed: {ex.Message}"); /* fall through to the dotnet probe */ }

        // Fallback: ask the .NET host itself (covers non-standard install locations).
        return ProbeViaDotnetListRuntimes();
    }

    private static bool ProbeViaDotnetListRuntimes()
    {
        try
        {
            var psi = new ProcessStartInfo("dotnet", "--list-runtimes")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            if (p == null) return false;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(5000);
            // A line like:  Microsoft.WindowsDesktop.App 8.0.28 [C:\Program Files\dotnet\shared\...]
            foreach (var line in output.Split('\n'))
                if (line.Contains("Microsoft.WindowsDesktop.App 8.")) return true;
        }
        catch (Exception ex) { Program.Log($"[CM26] dotnet runtime probe failed: {ex.Message}"); /* dotnet CLI not present */ }
        return false;
    }

    private static void ShowMissingRuntimeMessage()
    {
        var text =
            "Creation Master 26 (Lite) needs the Microsoft .NET 8 Desktop Runtime (x64),\r\n" +
            "which is not installed on this PC.\r\n\r\n" +
            "Install it once, then run CM26 again:\r\n" +
            "    Download: " + DownloadUrl + "\r\n" +
            "    Choose:   \".NET Desktop Runtime 8.x  x64\"\r\n\r\n" +
            "Alternatively, use the Full Portable package — it bundles the runtime\r\n" +
            "and needs no .NET installation.\r\n\r\n" +
            "Press OK to open the download page, or Cancel to exit.";
        try
        {
            var r = MessageBox.Show(text, "Creation Master 26 — .NET 8 Desktop Runtime required",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (r == DialogResult.OK)
                Process.Start(new ProcessStartInfo(DownloadUrl) { UseShellExecute = true });
        }
        catch (Exception ex) { Program.Log($"[CM26] Failed to show missing runtime message: {ex.Message}"); /* never crash the dependency handler */ }
    }
}
