namespace CM26.App;

/// <summary>Public error boundary for the x64 Studio/support host. Technical
/// details go to the local log; dialogs show only an actionable summary and a
/// correlation ID.</summary>
internal static class FriendlyErrorDialog
{
    internal static void Show(IWin32Window owner, string operation, Exception exception, string? recovery = null)
    {
        exception ??= new Exception("Unknown error.");
        var id = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        Program.Log($"Diagnostic {id} · {operation}{Environment.NewLine}{exception}");
        var summary = exception switch
        {
            OutOfMemoryException => "The operation exceeded the available memory and was stopped safely.",
            UnauthorizedAccessException => "Windows denied access to a required file or folder.",
            IOException => "A required file was missing, locked, or could not be replaced safely.",
            InvalidDataException or InvalidOperationException or ArgumentException when !string.IsNullOrWhiteSpace(exception.Message)
                => exception.Message,
            _ => "The operation could not be completed safely. Technical details were written to the local log."
        };
        var next = string.IsNullOrWhiteSpace(recovery)
            ? "No unverified change was accepted. Correct the reported item and retry."
            : recovery;
        MessageBox.Show(owner,
            "What happened\r\n" + summary + "\r\n\r\nWhat to do next\r\n" + next +
            "\r\n\r\nDiagnostic ID\r\n" + id + "\r\n\r\nLog\r\n" + Program.LogPath,
            "Creation Master 26 — " + operation, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    internal static string Status(Exception exception) => exception switch
    {
        OutOfMemoryException => "Operation stopped: insufficient available memory.",
        IOException => "Operation stopped: required file unavailable.",
        _ => "Operation stopped safely; see the diagnostic log."
    };
}
