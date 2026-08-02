using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Diagnostics: load state, table/column inventory, engine + save verification info.</summary>
public sealed class DiagnosticsSection : SectionBase
{
    private readonly BufferedPanel _host;

    public override string SectionKey => "diagnostics";
    public override string SectionTitle => "Diagnostics";
    protected override string TableName => "";
    protected override bool SinglePane => true;

    public DiagnosticsSection(AppServices s) : base(s)
    {
        _host = new BufferedPanel { Dock = DockStyle.Fill, BackColor = SystemColors.Control, Padding = new Padding(8), AutoScroll = true };
        Tabs.TabPages.Add(MakeTab("Diagnostics", _host));
        Header.SetRecord("Diagnostics", "Engine and database health", IconService.Get("diagnostics", 44));
    }

    protected override IReadOnlyList<RecordListItem> GetRecords() => Array.Empty<RecordListItem>();
    protected override void ShowRecord(int recordIndex) { }

    public override void ActivateSection()
    {
        // Diagnostics has no record browser; avoid the base empty-record state
        // obscuring the diagnostics view.
        Render();
    }

    private void Render()
    {
        _host.SuspendLayout();
        _host.Controls.Clear();
        var sb = new System.Text.StringBuilder();
        if (!Services.Session.IsLoaded)
        {
            sb.AppendLine("No database loaded.");
        }
        else
        {
            sb.AppendLine($"Loaded folder : {Services.Session.LoadedFolder}");
            sb.AppendLine($"Main database : {Services.Session.DatabasePath}");
            sb.AppendLine($"Locale file   : {Services.Session.LocalePath}");
            sb.AppendLine($"Metadata      : {Services.Session.MetaPath}");
            sb.AppendLine($"Tables (total): {Services.Session.Tables.Count}");
            sb.AppendLine($"Pending edits : {Services.Pending.Count}");
            sb.AppendLine();
            sb.AppendLine("Largest tables:");
            foreach (var t in Services.Session.Tables.OrderByDescending(t => t.RowCount).Take(12))
                sb.AppendLine($"  {t.Name,-32} {t.RowCount,10:N0} rows  [{(t.IsLocale ? "locale" : "main")}]");
        }
        var box = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = sb.ToString(),
            Font = new Font("Consolas", 9F),
            BackColor = SystemColors.Window,
            ForeColor = SystemColors.WindowText,
            BorderStyle = BorderStyle.Fixed3D,
        };
        _host.Controls.Add(box);
        _host.ResumeLayout();
    }
}
