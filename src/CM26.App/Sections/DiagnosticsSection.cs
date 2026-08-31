using System.Drawing;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Controls.Studio;
using CM26.App.Theming;
using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Diagnostics: load state, table/column inventory, engine + save verification info.</summary>
public sealed class DiagnosticsSection : SectionBase
{
    private readonly BufferedPanel _host;
    private readonly StudioToolbar _toolbar;

    public override string SectionKey => "diagnostics";
    public override string SectionTitle => "Diagnostics";
    protected override string TableName => "";
    protected override bool SinglePane => true;
    protected override bool ShowRecordCommandStrip => false;
    protected override bool UsesRecordData => false;

    public DiagnosticsSection(AppServices s) : base(s)
    {
        _host = new BufferedPanel { Dock = DockStyle.Fill, BackColor = StudioColors.AppBackground, Padding = new Padding(StudioSpacing.Medium), AutoScroll = true };
        _toolbar = new StudioToolbar
        {
            Title = "Diagnostics",
            CanCreate = false,
            ShowFilter = false,
            Dock = DockStyle.Top,
        };
        _toolbar.SearchBox.PlaceholderText = "Find in diagnostics…";
        _toolbar.SearchTextChanged += (_, _) => HighlightDiagnostic(_toolbar.SearchText);
        _toolbar.SearchKeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            HighlightDiagnostic(_toolbar.SearchText);
        };

        var page = new TabPage("Diagnostics") { BackColor = StudioColors.AppBackground };
        page.Controls.Add(_host);
        page.Controls.Add(_toolbar);
        Tabs.TabPages.Add(page);
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

    private TextBox? _diagnosticBox;

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
        _diagnosticBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = sb.ToString(),
            Font = Theme.Mono,
            BackColor = StudioColors.InputBackground,
            ForeColor = StudioColors.PrimaryText,
            BorderStyle = BorderStyle.None,
        };
        var card = new StudioCard { Dock = DockStyle.Fill, BackColor = StudioColors.Surface };
        card.Controls.Add(_diagnosticBox);
        _host.Controls.Add(card);
        _host.ResumeLayout();
    }

    private void HighlightDiagnostic(string query)
    {
        if (_diagnosticBox == null || string.IsNullOrWhiteSpace(query)) return;
        var text = _diagnosticBox.Text;
        var index = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return;
        _diagnosticBox.Focus();
        _diagnosticBox.Select(index, query.Length);
    }
}
