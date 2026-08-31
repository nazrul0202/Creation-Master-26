using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>Converts technical exceptions into an actionable public dialog while
/// preserving the complete exception in a local diagnostic file.</summary>
internal static class Fc26FriendlyError
{
    internal static void Show(IWin32Window owner, string operation, Exception exception, string recovery = null)
    {
        exception = exception ?? new Exception("Unknown error.");
        var id = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "diagnostics");
        string log = null;
        try
        {
            Directory.CreateDirectory(folder);
            log = Path.Combine(folder, "error-" + id + ".log");
            File.WriteAllText(log,
                "Creation Master 26 diagnostic " + id + Environment.NewLine +
                "UTC: " + DateTimeOffset.UtcNow.ToString("O") + Environment.NewLine +
                "Operation: " + operation + Environment.NewLine +
                "Version: " + Application.ProductVersion + Environment.NewLine + Environment.NewLine + exception);
        }
        catch
        {
            // The error handler must still be usable when the original problem
            // is a full disk or an inaccessible profile folder.
            log = null;
        }

        var summary = FriendlySummary(exception);
        var next = string.IsNullOrWhiteSpace(recovery)
            ? "No unverified change will be treated as saved. Review the diagnostic, correct the item shown, then retry."
            : recovery;
        using (var form = new Form
        {
            Text = "Creation Master 26 — " + operation,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(660, 330),
            MinimumSize = new Size(560, 300),
            AutoScaleMode = AutoScaleMode.Dpi,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowIcon = true
        })
        {
            var text = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = SystemFonts.MessageBoxFont,
                Text = "What happened\r\n" + summary + "\r\n\r\nWhat to do next\r\n" + next +
                       "\r\n\r\nDiagnostic ID\r\n" + id +
                       (log == null ? "\r\nTechnical log could not be written; check free space and folder permissions." : string.Empty)
            };
            var copy = new Button { Text = "Copy Diagnostic ID", AutoSize = true };
            copy.Click += (_, _) => Clipboard.SetText(id);
            var open = new Button { Text = "Open Diagnostic Folder", AutoSize = true };
            open.Enabled = log != null;
            open.Click += (_, _) => Process.Start(new ProcessStartInfo("explorer.exe", "\"" + folder + "\"") { UseShellExecute = true });
            var details = new Button { Text = "Show Technical Details", AutoSize = true };
            details.Enabled = log != null;
            details.Click += (_, _) => Process.Start(new ProcessStartInfo("notepad.exe", "\"" + log + "\"") { UseShellExecute = true });
            var close = new Button { Text = "Close", AutoSize = true, DialogResult = DialogResult.OK };
            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8), FlowDirection = FlowDirection.RightToLeft };
            actions.Controls.Add(close); actions.Controls.Add(open); actions.Controls.Add(details); actions.Controls.Add(copy);
            form.Controls.Add(text); form.Controls.Add(actions); form.AcceptButton = close;
            form.ShowDialog(owner);
        }
    }

    internal static string FriendlySummary(Exception exception)
    {
        if (exception is OutOfMemoryException)
            return "The current preview or database operation exceeded the available memory. Database changes were not marked as saved.";
        if (exception is NullReferenceException || exception is IndexOutOfRangeException || exception is ArgumentOutOfRangeException)
            return "The selected record or relationship was incomplete, so the operation was stopped safely.";
        if (exception is UnauthorizedAccessException)
            return "Windows denied access to one of the required files. Close FC26 and check folder permissions.";
        if (exception is IOException)
            return "A required file could not be read, locked or replaced safely. Close FC26 and any program using the game files.";
        if (exception is InvalidDataException || exception is InvalidOperationException || exception is ArgumentException)
            return string.IsNullOrWhiteSpace(exception.Message) ? "The requested change did not pass validation." : exception.Message;
        return "The operation could not be completed safely. Full technical information was saved locally.";
    }
}
