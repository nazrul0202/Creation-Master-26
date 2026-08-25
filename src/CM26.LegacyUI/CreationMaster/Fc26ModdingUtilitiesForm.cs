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
    private readonly ComboBox _namePlayer = new ComboBox();
    private readonly TextBox _firstName = new TextBox();
    private readonly TextBox _lastName = new TextBox();
    private readonly TextBox _commonName = new TextBox();
    private readonly TextBox _jerseyName = new TextBox();
    private readonly TextBox _nameReport = new TextBox();

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
        tabs.TabPages.Add(BuildNamesPage());
        tabs.TabPages.Add(BuildHashPage());
        tabs.TabPages.Add(BuildXmlPage());
        tabs.TabPages.Add(BuildComparePage());
        Controls.Add(tabs);
    }

    private TabPage BuildNamesPage()
    {
        var page = new TabPage("Player Names");
        var editor = new TableLayoutPanel { Dock = DockStyle.Top, Height = 196, Padding = new Padding(10), ColumnCount = 4, RowCount = 6 };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        _namePlayer.DropDownStyle = ComboBoxStyle.DropDownList;
        _namePlayer.DisplayMember = "Display";
        _namePlayer.ValueMember = "Player";
        _namePlayer.SelectedIndexChanged += (_, _) => LoadSelectedPlayerNames();
        editor.Controls.Add(new Label { Text = "Player", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        editor.Controls.Add(_namePlayer, 1, 0); editor.SetColumnSpan(_namePlayer, 3);
        AddNameField(editor, "First name", _firstName, 0, 1);
        AddNameField(editor, "Surname", _lastName, 2, 1);
        AddNameField(editor, "Common name", _commonName, 0, 2);
        AddNameField(editor, "Jersey name", _jerseyName, 2, 2);
        var apply = Button("Apply names", (_, _) => ApplyNames());
        var repair = Button("Create / repair name records", (_, _) => RepairSelectedNameRecords());
        var audit = Button("Audit all players", (_, _) => AuditPlayerNames());
        var refresh = Button("Refresh", (_, _) => PopulateNamePlayers());
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        actions.Controls.AddRange(new Control[] { apply, repair, audit, refresh });
        editor.Controls.Add(actions, 0, 4); editor.SetColumnSpan(actions, 4);
        editor.Controls.Add(new Label
        {
            Text = "Names are staged in CM26. File > Save resolves/creates playernames records, validates and commits them.",
            AutoSize = true, ForeColor = Color.DarkGreen, Anchor = AnchorStyles.Left
        }, 0, 5); editor.SetColumnSpan(editor.GetControlFromPosition(0, 5), 4);
        _nameReport.Dock = DockStyle.Fill; _nameReport.Multiline = true; _nameReport.ReadOnly = true;
        _nameReport.ScrollBars = ScrollBars.Both; _nameReport.Font = new Font("Consolas", 9f);
        page.Controls.Add(_nameReport); page.Controls.Add(editor);
        page.Enter += (_, _) => PopulateNamePlayers();
        return page;
    }

    private static void AddNameField(TableLayoutPanel editor, string label, Control field, int column, int row)
    {
        field.Dock = DockStyle.Fill;
        editor.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, column, row);
        editor.Controls.Add(field, column + 1, row);
    }

    private void PopulateNamePlayers()
    {
        var selected = SelectedNamePlayer()?.Id ?? -1;
        var rows = FifaEnvironment.Players.Cast<Player>()
            .OrderBy(player => player.ToString(), StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(player => player.Id)
            .Select(player => new PlayerChoice(player)).ToArray();
        _namePlayer.BeginUpdate();
        _namePlayer.DataSource = rows;
        _namePlayer.EndUpdate();
        if (selected >= 0)
        {
            var index = Array.FindIndex(rows, row => row.Player.Id == selected);
            if (index >= 0) _namePlayer.SelectedIndex = index;
        }
        LoadSelectedPlayerNames();
    }

    private Player SelectedNamePlayer()
    {
        var choice = _namePlayer.SelectedItem as PlayerChoice;
        return choice?.Player;
    }

    private void LoadSelectedPlayerNames()
    {
        var player = SelectedNamePlayer();
        if (player == null) return;
        _firstName.Text = player.firstname ?? string.Empty;
        _lastName.Text = player.lastname ?? string.Empty;
        _commonName.Text = player.commonname ?? string.Empty;
        _jerseyName.Text = player.playerjerseyname ?? string.Empty;
        _nameReport.Text = NameRecordReport(player);
    }

    private void ApplyNames()
    {
        var player = SelectedNamePlayer();
        if (player == null) return;
        player.firstname = PlayerNames.Normalize(_firstName.Text.Trim());
        player.lastname = PlayerNames.Normalize(_lastName.Text.Trim());
        player.commonname = PlayerNames.Normalize(_commonName.Text.Trim());
        player.playerjerseyname = PlayerNames.Normalize(_jerseyName.Text.Trim());
        RepairNameRecords(player);
        _nameReport.Text = "Applied to player " + player.Id + ".\r\n\r\n" + NameRecordReport(player);
    }

    private void RepairSelectedNameRecords()
    {
        var player = SelectedNamePlayer();
        if (player == null) return;
        var repaired = RepairNameRecords(player);
        _nameReport.Text = repaired + " name record(s) created/relinked for player " + player.Id + ".\r\n\r\n" + NameRecordReport(player);
    }

    private static int RepairNameRecords(Player player)
    {
        var repaired = 0;
        repaired += EnsureNameRecord(player.firstname, player.firstnameid, id => player.firstnameid = id);
        repaired += EnsureNameRecord(player.lastname, player.lastnameid, id => player.lastnameid = id);
        repaired += EnsureNameRecord(player.commonname, player.commonnameid, id => player.commonnameid = id);
        repaired += EnsureNameRecord(player.playerjerseyname, player.playerjerseynameid, id => player.playerjerseynameid = id);
        return repaired;
    }

    private static int EnsureNameRecord(string text, int currentId, Action<int> assign)
    {
        text = PlayerNames.Normalize(text ?? string.Empty);
        if (text.Length == 0) { if (currentId != 0) assign(0); return currentId == 0 ? 0 : 1; }
        string existing;
        if (currentId > 0 && FifaEnvironment.PlayerNamesList.TryGetValue(currentId, out existing, isUsed: true) && existing == text) return 0;
        var resolved = FifaEnvironment.PlayerNamesList.GetKey(text);
        assign(resolved);
        return 1;
    }

    private void AuditPlayerNames()
    {
        var issues = new List<string>();
        foreach (Player player in FifaEnvironment.Players)
        {
            AuditName(player, "firstnameid", player.firstnameid, player.firstname, issues);
            AuditName(player, "lastnameid", player.lastnameid, player.lastname, issues);
            AuditName(player, "commonnameid", player.commonnameid, player.commonname, issues);
            AuditName(player, "playerjerseynameid", player.playerjerseynameid, player.playerjerseyname, issues);
        }
        _nameReport.Text = issues.Count == 0 ? "All player name links are valid." :
            issues.Count + " invalid or missing player-name link(s):\r\n\r\n" + string.Join("\r\n", issues.Take(5000));
    }

    private static void AuditName(Player player, string field, int id, string text, List<string> issues)
    {
        if (string.IsNullOrWhiteSpace(text)) { if (id != 0) issues.Add(player.Id + " " + field + "=" + id + " points to an empty value"); return; }
        string linked;
        if (id <= 0 || !FifaEnvironment.PlayerNamesList.TryGetValue(id, out linked, isUsed: false))
            issues.Add(player.Id + " " + field + "=" + id + " missing (value: " + text + ")");
        else if (!string.Equals(linked, text, StringComparison.Ordinal))
            issues.Add(player.Id + " " + field + "=" + id + " resolves to '" + linked + "' instead of '" + text + "'");
    }

    private static string NameRecordReport(Player player)
    {
        return "Player ID: " + player.Id + "\r\n" +
            "firstnameid: " + player.firstnameid + " -> " + player.firstname + "\r\n" +
            "lastnameid: " + player.lastnameid + " -> " + player.lastname + "\r\n" +
            "commonnameid: " + player.commonnameid + " -> " + player.commonname + "\r\n" +
            "playerjerseynameid: " + player.playerjerseynameid + " -> " + player.playerjerseyname;
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
        var assets = _entity.Text == "Player" ? MovePlayerAssets((int)_oldId.Value, (int)_newId.Value) : 0;
        _idReport.Text = oldHits.Count + " update(s) and " + assets + " staged linked asset(s) prepared. File > Save validates and commits them.";
    }

    private void SwapIds(object sender, EventArgs e)
    {
        var entity = SelectedEntity(); var first = ((int)_oldId.Value).ToString(CultureInfo.InvariantCulture); var second = ((int)_newId.Value).ToString(CultureInfo.InvariantCulture);
        var firstHits = Hits(entity[1], first); var secondHits = Hits(entity[1], second);
        if (firstHits.Count == 0 || secondHits.Count == 0) { MessageBox.Show(this, "Both IDs must exist."); return; }
        if (MessageBox.Show(this, "Swap all linked references for these two IDs?", "Dependency-aware ID swap", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        foreach (var hit in firstHits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, second);
        foreach (var hit in secondHits) Fc26SnapshotLoader.StageDetailValue(hit.Table, hit.Row, hit.Field, first);
        var assets = _entity.Text == "Player" ? SwapPlayerAssets((int)_oldId.Value, (int)_newId.Value) : 0;
        _idReport.Text = (firstHits.Count + secondHits.Count) + " linked values and " + assets + " staged linked asset(s) prepared for swap.";
    }

    private static int MovePlayerAssets(int sourceId, int targetId)
    {
        var moved = 0;
        foreach (var pair in PlayerAssetPairs(sourceId, targetId))
            if (Fc26HostBridge.MoveStagedAsset(pair.Item1, pair.Item2).StartsWith("Moved", StringComparison.OrdinalIgnoreCase)) moved++;
        return moved;
    }

    private static int SwapPlayerAssets(int firstId, int secondId)
    {
        var moved = 0; var temporaryId = 900000000 + firstId % 99999999;
        var firstToTemp = PlayerAssetPairs(firstId, temporaryId).ToArray();
        var secondToFirst = PlayerAssetPairs(secondId, firstId).ToArray();
        var tempToSecond = PlayerAssetPairs(temporaryId, secondId).ToArray();
        for (var i = 0; i < firstToTemp.Length; i++)
        {
            var firstMoved = Fc26HostBridge.MoveStagedAsset(firstToTemp[i].Item1, firstToTemp[i].Item2).StartsWith("Moved", StringComparison.OrdinalIgnoreCase);
            var secondMoved = Fc26HostBridge.MoveStagedAsset(secondToFirst[i].Item1, secondToFirst[i].Item2).StartsWith("Moved", StringComparison.OrdinalIgnoreCase);
            var finalMoved = Fc26HostBridge.MoveStagedAsset(tempToSecond[i].Item1, tempToSecond[i].Item2).StartsWith("Moved", StringComparison.OrdinalIgnoreCase);
            if (firstMoved) moved++; if (secondMoved) moved++; if (finalMoved) moved++;
        }
        return moved;
    }

    private static IEnumerable<Tuple<string, string>> PlayerAssetPairs(int sourceId, int targetId)
    {
        yield return Tuple.Create(Player.SpecificPhotoDdsFileName(sourceId), Player.SpecificPhotoDdsFileName(targetId));
        yield return Tuple.Create(Player.SpecificFaceTextureFileName(sourceId), Player.SpecificFaceTextureFileName(targetId));
        yield return Tuple.Create(Player.SpecificHeadModelFileName(sourceId), Player.SpecificHeadModelFileName(targetId));
        yield return Tuple.Create(Player.SpecificHairTexturesFileName(sourceId), Player.SpecificHairTexturesFileName(targetId));
        yield return Tuple.Create(Player.SpecificHairModelFileName(sourceId), Player.SpecificHairModelFileName(targetId));
        yield return Tuple.Create(Player.SpecificHairLodModelFileName(sourceId), Player.SpecificHairLodModelFileName(targetId));
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

    private sealed class PlayerChoice
    {
        internal PlayerChoice(Player player) { Player = player; Display = player + "  [" + player.Id + "]"; }
        public Player Player { get; }
        public string Display { get; }
        public override string ToString() => Display;
    }
}
