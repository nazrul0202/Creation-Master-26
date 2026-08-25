using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FifaLibrary;

namespace CreationMaster;

/// <summary>Low-level DBM/RDM-style utilities presented in the existing CM16 visual language.</summary>
internal sealed class Fc26ModdingUtilitiesForm : Form
{
    private readonly ComboBox _entity = new ComboBox();
    private readonly NumericUpDown _oldId = new NumericUpDown();
    private readonly NumericUpDown _newId = new NumericUpDown();
    private readonly TextBox _idReport = new TextBox();
    private readonly TextBox _hashInput = new TextBox();
    private readonly TextBox _hashOutput = new TextBox();
    private readonly DateTimePicker _date = new DateTimePicker();
    private readonly NumericUpDown _fifaDate = new NumericUpDown();
    private readonly TextBox _xmlReport = new TextBox();

    private static readonly Dictionary<string, string[]> Entities = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["Player"] = new[] { "players", "playerid" }, ["Team"] = new[] { "teams", "teamid" },
        ["League"] = new[] { "leagues", "leagueid" }, ["Competition"] = new[] { "competition", "competitionid" },
        ["Nation"] = new[] { "nations", "nationid" }, ["Stadium"] = new[] { "stadiums", "stadiumid" },
        ["Formation"] = new[] { "formations", "formationid" }, ["Manager"] = new[] { "manager", "managerid" },
    };

    internal Fc26ModdingUtilitiesForm()
    {
        Text = "FC26 Internal Modding Utilities";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(900, 640);
        MinimumSize = new Size(760, 520);
        Icon = Form.ActiveForm?.Icon;
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildIdPage());
        tabs.TabPages.Add(BuildHashPage());
        tabs.TabPages.Add(BuildXmlPage());
        tabs.TabPages.Add(BuildComparePage());
        Controls.Add(tabs);
    }

    private TabPage BuildIdPage()
    {
        var page = new TabPage("ID & References");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(8), WrapContents = true };
        _entity.DropDownStyle = ComboBoxStyle.DropDownList; _entity.Width = 120;
        _entity.Items.AddRange(Entities.Keys.Cast<object>().ToArray()); _entity.SelectedIndex = 0;
        ConfigureId(_oldId); ConfigureId(_newId);
        top.Controls.AddRange(new Control[] { Label("Entity"), _entity, Label("Current ID"), _oldId, Label("Target/other ID"), _newId,
            Button("Find available", FindAvailable), Button("Impact", ShowImpact), Button("Change ID", ChangeId), Button("Swap IDs", SwapIds),
            Button("Duplicate audit", AuditDuplicates) });
        _idReport.Dock = DockStyle.Fill; _idReport.Multiline = true; _idReport.ReadOnly = true;
        _idReport.ScrollBars = ScrollBars.Both; _idReport.Font = new Font("Consolas", 9f);
        page.Controls.Add(_idReport); page.Controls.Add(top);
        return page;
    }

    private TabPage BuildHashPage()
    {
        var page = new TabPage("Hash & FIFA Date");
        var hash = new GroupBox { Text = "FIFA hash calculation", Dock = DockStyle.Top, Height = 150, Padding = new Padding(10) };
        _hashInput.SetBounds(15, 32, 620, 24); _hashOutput.SetBounds(15, 68, 620, 56); _hashOutput.Multiline = true; _hashOutput.ReadOnly = true;
        var calculate = Button("Calculate", (_, _) =>
        {
            var value = _hashInput.Text;
            _hashOutput.Text = "FIFA 32-bit: " + FifaUtil.ComputeHash(value).ToString(CultureInfo.InvariantCulture) +
                " (0x" + unchecked((uint)FifaUtil.ComputeHash(value)).ToString("X8") + ")\r\nBH 64-bit: 0x" + FifaUtil.ComputeBhHash(value).ToString("X16");
        });
        calculate.SetBounds(650, 31, 110, 26); hash.Controls.AddRange(new Control[] { _hashInput, _hashOutput, calculate });
        var dates = new GroupBox { Text = "Internal FIFA date conversion", Dock = DockStyle.Top, Height = 125, Padding = new Padding(10) };
        _date.SetBounds(15, 36, 240, 24); _fifaDate.SetBounds(390, 36, 180, 24); _fifaDate.Maximum = int.MaxValue;
        var toInternal = Button("Date -> FIFA", (_, _) => _fifaDate.Value = Math.Max(0, FifaUtil.ConvertToFifaDate(_date.Value.Date)));
        toInternal.SetBounds(270, 35, 105, 26);
        var fromInternal = Button("FIFA -> Date", (_, _) =>
        {
            try { _date.Value = FifaUtil.ConvertFromFifaDate((int)_fifaDate.Value); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "FIFA date", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        });
        fromInternal.SetBounds(585, 35, 105, 26);
        dates.Controls.AddRange(new Control[] { _date, _fifaDate, toInternal, fromInternal });
        page.Controls.Add(dates); page.Controls.Add(hash);
        return page;
    }

    private TabPage BuildXmlPage()
    {
        var page = new TabPage("XML Descriptor");
        var open = Button("Open and validate XML descriptor...", OpenXml); open.Dock = DockStyle.Top; open.Height = 34;
        _xmlReport.Dock = DockStyle.Fill; _xmlReport.Multiline = true; _xmlReport.ReadOnly = true;
        _xmlReport.ScrollBars = ScrollBars.Both; _xmlReport.Font = new Font("Consolas", 9f);
        page.Controls.Add(_xmlReport); page.Controls.Add(open);
        return page;
    }

    private TabPage BuildComparePage()
    {
        var page = new TabPage("Compare Databases");
        var output = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9f) };
        var compare = Button("Compare loaded database with extracted folder...", (_, _) =>
        {
            using (var dialog = new FolderBrowserDialog { Description = "Select the other extracted FC26 database" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    Cursor = Cursors.WaitCursor;
                    var alternate = Fc26HostBridge.OpenExtractedFolder(dialog.SelectedPath);
                    output.Text = Fc26SnapshotLoader.CompareWithSnapshot(alternate);
                }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, "Compare databases", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { Cursor = Cursors.Default; }
            }
        });
        compare.Dock = DockStyle.Top; compare.Height = 34;
        page.Controls.Add(output); page.Controls.Add(compare);
        return page;
    }

    private static void ConfigureId(NumericUpDown value) { value.Minimum = 0; value.Maximum = int.MaxValue; value.Width = 105; }
    private static Label Label(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(6, 7, 2, 0) };
    private static Button Button(string text, EventHandler click) { var button = new Button { Text = text, AutoSize = true, Height = 27 }; button.Click += click; return button; }

    private string[] SelectedEntity() => Entities[_entity.SelectedItem.ToString()];

    private void FindAvailable(object sender, EventArgs e)
    {
        var entity = SelectedEntity(); var table = Fc26SnapshotLoader.DetailTable(entity[0]);
        if (table == null) return; var column = table.Column(entity[1]);
        var used = new HashSet<int>(table.Rows.Select(row => column < row.Length && int.TryParse(row[column], out var id) ? id : -1));
        var available = Enumerable.Range(1, Math.Min(2000000, Math.Max(1000, used.Count * 4))).FirstOrDefault(id => !used.Contains(id));
        _newId.Value = available; _idReport.Text = "First available " + entity[1] + ": " + available;
    }

    private List<IdHit> Hits(string field, string value)
    {
        var hits = new List<IdHit>();
        foreach (var tableName in Fc26SnapshotLoader.DetailTableNames)
        {
            var table = Fc26SnapshotLoader.DetailTable(tableName); if (table == null) continue;
            for (var column = 0; column < table.Columns.Length; column++)
                if (table.Columns[column].Equals(field, StringComparison.OrdinalIgnoreCase))
                    for (var row = 0; row < table.Rows.Count; row++)
                        if (column < table.Rows[row].Length && table.Rows[row][column] == value) hits.Add(new IdHit(table.Name, row, table.Columns[column]));
        }
        return hits;
    }

    private void ShowImpact(object sender, EventArgs e)
    {
        var entity = SelectedEntity(); var hits = Hits(entity[1], ((int)_oldId.Value).ToString(CultureInfo.InvariantCulture));
        _idReport.Text = hits.Count + " reference(s)\r\n\r\n" + string.Join("\r\n", hits.Take(2000).Select(hit => hit.ToString()));
    }

    private void ChangeId(object sender, EventArgs e)
    {
        var entity = SelectedEntity(); var oldValue = ((int)_oldId.Value).ToString(CultureInfo.InvariantCulture); var newValue = ((int)_newId.Value).ToString(CultureInfo.InvariantCulture);
        var oldHits = Hits(entity[1], oldValue); var newHits = Hits(entity[1], newValue);
        if (oldHits.Count == 0) { MessageBox.Show(this, "Current ID was not found."); return; }
        if (newHits.Count > 0) { MessageBox.Show(this, "Target ID already exists. Use Swap IDs."); return; }
        if (MessageBox.Show(this, "Stage " + oldHits.Count + " linked ID update(s)?", "Dependency-aware ID change", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var hit in oldHits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, newValue);
        _idReport.Text = oldHits.Count + " update(s) staged. File > Save validates and commits them.";
    }

    private void SwapIds(object sender, EventArgs e)
    {
        var entity = SelectedEntity(); var first = ((int)_oldId.Value).ToString(CultureInfo.InvariantCulture); var second = ((int)_newId.Value).ToString(CultureInfo.InvariantCulture);
        var firstHits = Hits(entity[1], first); var secondHits = Hits(entity[1], second);
        if (firstHits.Count == 0 || secondHits.Count == 0) { MessageBox.Show(this, "Both IDs must exist."); return; }
        if (MessageBox.Show(this, "Swap all linked references for these two IDs?", "Dependency-aware ID swap", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var hit in firstHits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, second);
        foreach (var hit in secondHits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, first);
        _idReport.Text = (firstHits.Count + secondHits.Count) + " linked values staged for swap.";
    }

    private void AuditDuplicates(object sender, EventArgs e)
    {
        var entity = SelectedEntity(); var table = Fc26SnapshotLoader.DetailTable(entity[0]); if (table == null) return; var column = table.Column(entity[1]);
        var duplicates = table.Rows.Select((row, index) => new { Row = index, Value = column < row.Length ? row[column] : string.Empty })
            .GroupBy(item => item.Value).Where(group => group.Key.Length > 0 && group.Count() > 1).ToArray();
        _idReport.Text = duplicates.Length == 0 ? "No duplicate " + entity[1] + " values." : duplicates.Length + " duplicate ID group(s):\r\n\r\n" +
            string.Join("\r\n", duplicates.Select(group => group.Key + " -> rows " + string.Join(", ", group.Select(item => item.Row))));
    }

    private void OpenXml(object sender, EventArgs e)
    {
        using (var dialog = new OpenFileDialog { Filter = "XML descriptors (*.xml)|*.xml|All files (*.*)|*.*" })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var document = new XmlDocument { XmlResolver = null }; document.Load(dialog.FileName);
                var elements = document.SelectNodes("//*").Cast<XmlNode>().OfType<XmlElement>().ToArray();
                var ranges = elements.Where(element => element.HasAttribute("rangeLow") || element.HasAttribute("rangeHigh") || element.HasAttribute("min") || element.HasAttribute("max")).ToArray();
                var invalid = new List<string>();
                foreach (var element in ranges)
                {
                    var lowText = element.HasAttribute("rangeLow") ? element.GetAttribute("rangeLow") : element.GetAttribute("min");
                    var highText = element.HasAttribute("rangeHigh") ? element.GetAttribute("rangeHigh") : element.GetAttribute("max");
                    long low, high; if (long.TryParse(lowText, out low) && long.TryParse(highText, out high) && low > high) invalid.Add(element.Name + ": " + low + " > " + high);
                }
                _xmlReport.Text = "File: " + dialog.FileName + "\r\nRoot: " + document.DocumentElement?.Name + "\r\nElements: " + elements.Length +
                    "\r\nRange descriptors: " + ranges.Length + "\r\nInvalid ranges: " + invalid.Count + "\r\n\r\n" + string.Join("\r\n", invalid);
            }
            catch (Exception ex) { _xmlReport.Text = "INVALID XML\r\n\r\n" + ex.Message; }
        }
    }

    private sealed class IdHit
    {
        internal IdHit(string table, int row, string field) { Table = table; Row = row; Field = field; }
        internal string Table { get; } internal int Row { get; } internal string Field { get; }
        public override string ToString() => Table + "[" + Row + "]." + Field;
    }
}
