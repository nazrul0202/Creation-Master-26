using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CreationMaster;

internal sealed class Fc26WorkflowUtilitiesForm : Form
{
    private readonly TextBox _report = new TextBox();
    private readonly ListBox _history = new ListBox();

    internal Fc26WorkflowUtilitiesForm()
    {
        Text = "FC26 Workflow, History & Performance Tools";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(880, 620); MinimumSize = new Size(760, 520); Icon = Form.ActiveForm?.Icon;
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildOverview()); tabs.TabPages.Add(BuildHistory());
        Controls.Add(tabs); RefreshReport(); RefreshHistory();
    }

    private TabPage BuildOverview()
    {
        var page = new TabPage("Diagnostics & performance");
        _report.Dock = DockStyle.Fill; _report.Multiline = true; _report.ReadOnly = true; _report.ScrollBars = ScrollBars.Both; _report.Font = new Font("Consolas", 9);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(6) };
        actions.Controls.Add(Button("Refresh profile", (_, _) => RefreshReport()));
        actions.Controls.Add(Button("Export diagnostic report", (_, _) => ExportReport()));
        actions.Controls.Add(Button("Clear preview cache", (_, _) => ClearCache()));
        actions.Controls.Add(Button("Collect unused memory", (_, _) => { GC.Collect(); GC.WaitForPendingFinalizers(); RefreshReport(); }));
        page.Controls.Add(_report); page.Controls.Add(actions); return page;
    }

    private TabPage BuildHistory()
    {
        var page = new TabPage("Action history"); _history.Dock = DockStyle.Fill;
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(6) };
        actions.Controls.Add(Button("Refresh", (_, _) => RefreshHistory()));
        actions.Controls.Add(Button("Export history", (_, _) => ExportHistory()));
        page.Controls.Add(_history); page.Controls.Add(actions); return page;
    }

    private void RefreshReport()
    {
        var output = new StringBuilder();
        output.AppendLine("CM26 FC26 source profile"); output.AppendLine(new string('=', 72));
        output.AppendLine(Fc26SnapshotLoader.DescribeLoadedSource());
        output.AppendLine("Pending advanced/detail edits: " + Fc26SnapshotLoader.PendingDetailCount.ToString("N0"));
        output.AppendLine("Managed memory: " + (GC.GetTotalMemory(false) / 1048576d).ToString("N1") + " MB");
        output.AppendLine("Process working set: " + (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1048576d).ToString("N1") + " MB");
        output.AppendLine(); output.AppendLine(Fc26SnapshotLoader.DescribeIdAvailability());
        output.AppendLine(); output.AppendLine("Largest tables"); output.AppendLine(new string('-', 72));
        foreach (var name in Fc26SnapshotLoader.DetailTableNames.Select(name => new { Name = name, Table = Fc26SnapshotLoader.DetailTable(name) })
            .Where(item => item.Table != null).OrderByDescending(item => item.Table!.Rows.Count).Take(25))
            output.AppendLine(name.Name.PadRight(38) + name.Table!.Rows.Count.ToString("N0").PadLeft(12) + " rows");
        _report.Text = output.ToString();
    }

    private void RefreshHistory() { _history.Items.Clear(); _history.Items.AddRange(Fc26ActivityLog.Snapshot().AsEnumerable().Reverse().Cast<object>().ToArray()); }
    private void ExportReport() { using var dialog = new SaveFileDialog { Filter = "Text report (*.txt)|*.txt", FileName = "CM26_Diagnostic_Report.txt" }; if (dialog.ShowDialog(this) == DialogResult.OK) File.WriteAllText(dialog.FileName, _report.Text, new UTF8Encoding(true)); }
    private void ExportHistory() { using var dialog = new SaveFileDialog { Filter = "Tab-separated log (*.tsv)|*.tsv", FileName = "CM26_Action_History.tsv" }; if (dialog.ShowDialog(this) == DialogResult.OK) Fc26ActivityLog.Export(dialog.FileName); }
    private void ClearCache()
    {
        if (MessageBox.Show(this, "Clear locally cached FC26 image previews? Source game assets and staged edits are not removed.", "Preview cache", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { MessageBox.Show(this, Fc26HostBridge.ClearPreviewCache(), "Preview cache", MessageBoxButtons.OK, MessageBoxIcon.Information); RefreshReport(); }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Preview cache", ex, "The cache was not cleared. Close preview windows, check free space, then retry."); }
    }
    private static Button Button(string text, EventHandler action) { var button = new Button { Text = text, AutoSize = true }; button.Click += action; return button; }
}
