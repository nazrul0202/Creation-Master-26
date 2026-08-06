using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CM26.App.Controls;
using CM26.App.Theming;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

namespace CM26.App.Sections;

/// <summary>
/// Shared layout for a CM16-style editor section: left browser, right editor
/// (header + tabbed grouped fields + validation + action bar). Subclasses supply
/// the record list, the editor tabs, and the save/stage behaviour for their table.
/// </summary>
public abstract class SectionBase : UserControl
{
    private static readonly Font ClassicFont = Theme.Body;
    protected readonly AppServices Services;
    protected readonly RecordListPanel Browser;
    protected readonly EditorHeader Header;
    protected readonly TabControl Tabs;
    protected readonly ValidationSummary Validation;
    protected readonly EmptyStatePanel EmptyState;
    protected readonly AssetPreviewPanel Preview;

    private readonly Button _revertBtn, _validateBtn;
    private readonly SplitContainer _split;
    private readonly BufferedPanel _previewHost;
    private readonly ComboBox _recordPicker;
    private readonly TextBox _recordSearch;
    private readonly Label _recordCount;
    private bool _syncPicker;
    protected readonly ToolTip ToolTip = new();
    protected readonly Toast Toast = new();

    /// <summary>When true, an asset-preview panel is docked to the right of the editor.</summary>
    protected virtual bool HasPreview => false;
    /// <summary>Enables the safe entity-creation command for supported public editor sections.</summary>
    protected virtual bool SupportsCreate => false;
    /// <summary>Compact search hint shown beside the record chooser.</summary>
    // This property is read while the base constructor is creating the command
    // strip.  Do not derive the text from SectionTitle here: derived sections
    // have not initialized their title fields yet, which previously caused a
    // NullReferenceException when opening several classic sections.
    protected virtual string RecordSearchPlaceholder => "Search records…";
    /// <summary>
    /// Operational tools such as Data Sync own their own workflow controls and
    /// must not display the empty record-picker command strip.
    /// </summary>
    protected virtual bool ShowRecordCommandStrip => true;

    public abstract string SectionKey { get; }
    public abstract string SectionTitle { get; }
    protected abstract string TableName { get; }

    /// <summary>When true, the left browser is hidden and the editor fills the section.</summary>
    protected virtual bool SinglePane => false;

    protected int CurrentRecordIndex { get; private set; } = -1;

    protected SectionBase(AppServices services)
    {
        Services = services;
        BackColor = Theme.Background;
        Dock = DockStyle.Fill;

        // CM16 used a compact record chooser in its green command strip, rather
        // than a persistent modern navigation pane.  Keep RecordListPanel only
        // as the data/selection adapter; it is deliberately collapsed below.
        Browser = new RecordListPanel { Dock = DockStyle.Fill };
        Header = new EditorHeader { Dock = DockStyle.Top, Visible = false };
        Tabs = new TabControl { Dock = DockStyle.Fill, Font = ClassicFont, Padding = new Point(3, 1), BackColor = Theme.Background };
        Validation = new ValidationSummary { Dock = DockStyle.Bottom };
        EmptyState = new EmptyStatePanel("Select a record to edit") { Dock = DockStyle.Fill, Visible = false };

        _revertBtn = new Button { Text = "Revert" };
        _validateBtn = new Button { Text = "Validate" };
        Theme.ApplyButton(_revertBtn);
        Theme.ApplyButton(_validateBtn);

        var actionBar = new BufferedPanel { Dock = DockStyle.Bottom, Height = Theme.ControlHeight + 14, Padding = new Padding(Theme.Space, 7, Theme.Space, 5), BackColor = Theme.Panel };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
        _revertBtn.Width = 84; _validateBtn.Width = 96;
        flow.Controls.Add(_validateBtn);
        flow.Controls.Add(_revertBtn);
        actionBar.Controls.Add(flow);

        var editorPanel = new BufferedPanel { Dock = DockStyle.Fill, BackColor = Theme.Background };
        editorPanel.Controls.Add(Tabs);
        editorPanel.Controls.Add(EmptyState);

        // Optional right-hand asset preview column (CM16-style) above the tabs.
        Preview = new AssetPreviewPanel(services.Textures) { Dock = DockStyle.Fill };
        _previewHost = new BufferedPanel
        {
            Dock = DockStyle.Right,
            Width = 200,
            Padding = new Padding(Theme.Space, 0, Theme.Space, 0),
            BackColor = Theme.Background,
            Visible = HasPreview,
        };
        _previewHost.Controls.Add(Preview);
        editorPanel.Controls.Add(_previewHost);

        var commandStrip = new BufferedPanel
        {
            Dock = DockStyle.Top,
            Height = Theme.ControlHeight + 5,
            BackColor = Theme.Panel,
            Padding = new Padding(5, 2, 5, 2),
            Visible = ShowRecordCommandStrip,
        };
        _recordPicker = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 390,
            Height = Theme.ControlHeight,
            Dock = DockStyle.Left,
            FlatStyle = FlatStyle.Flat,
        };
        Theme.ApplyCombo(_recordPicker);
        _recordSearch = new TextBox
        {
            Width = 175,
            Height = Theme.ControlHeight,
            Dock = DockStyle.Left,
            PlaceholderText = RecordSearchPlaceholder,
        };
        Theme.ApplyTextBox(_recordSearch);
        var find = new Button { Text = "Find", Width = 52, Dock = DockStyle.Left, TabStop = false };
        var refresh = new Button { Text = "Refresh", Width = 70, Dock = DockStyle.Left, TabStop = false };
        var create = new Button
        {
            Text = "New",
            Width = 76,
            Dock = DockStyle.Left,
            TabStop = false,
            Visible = SupportsCreate,
        };
        Theme.ApplyButton(find);
        Theme.ApplyButton(refresh);
        Theme.ApplyButton(create);
        _recordCount = new Label
        {
            Dock = DockStyle.Left,
            Width = 130,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.Muted,
            Font = ClassicFont,
            Padding = new Padding(8, 0, 0, 0),
        };
        commandStrip.Controls.Add(_recordCount);
        commandStrip.Controls.Add(create);
        commandStrip.Controls.Add(refresh);
        commandStrip.Controls.Add(find);
        commandStrip.Controls.Add(_recordSearch);
        commandStrip.Controls.Add(_recordPicker);

        // Editor header docks above the command strip (later Adds are processed
        // first, so Header added last wins the top edge). Sections that do not
        // want it (classic fixed-layout pages) hide it in their own ctors,
        // which run after this one, so the visibility decision is preserved.
        editorPanel.Controls.Add(Header);

        editorPanel.Controls.Add(commandStrip);
        editorPanel.Controls.Add(Validation);
        editorPanel.Controls.Add(actionBar);

        // NOTE: Panel1MinSize/Panel2MinSize and SplitterDistance are applied AFTER the control has a
        // real size (in ConfigureSplit), because setting them on a zero-width SplitContainer throws.
        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BackColor = Theme.Border,
            FixedPanel = FixedPanel.Panel1, // browser keeps its width when the window resizes (CM16 behaviour)
        };
        _split.Panel1.Controls.Add(Browser);
        _split.Panel1.BackColor = Theme.Background;
        _split.Panel2.Controls.Add(editorPanel);
        _split.Panel2.BackColor = Theme.Background;
        // All entity forms use the CM16 full-canvas layout.  The retained
        // RecordListPanel continues to provide filtering/selection internally.
        _split.Panel1Collapsed = true;

        Controls.Add(_split);

        // Apply split constraints + distance once the control has a genuinely usable width, and keep
        // them clamped on every subsequent resize. This is safe at 0 width (no-op until sized).
        _split.SizeChanged += (_, _) => ConfigureSplit();
        _split.HandleCreated += (_, _) => ConfigureSplit();

        Browser.SelectionChanged += (_, index) => OnRecordSelected(index);
        Browser.RefreshRequested += (_, _) => LoadData();
        _recordPicker.SelectedIndexChanged += (_, _) =>
        {
            if (!_syncPicker && _recordPicker.SelectedItem is RecordListItem item)
                OnRecordSelected(item.RecordIndex);
        };
        refresh.Click += (_, _) => LoadData();
        find.Click += (_, _) => FindRecord();
        _recordSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            FindRecord();
            e.SuppressKeyPress = true;
        };
        create.Click += (_, _) => CreateNewRecord();
        _revertBtn.Click += (_, _) => RevertCurrentRecord();
        _validateBtn.Click += (_, _) => ValidateCurrent();
    }

    /// <summary>
    /// "Revert" restores the selected record's staged state: every staged edit
    /// for the current row is discarded through the engine, then the record is
    /// re-read from the database so the fields show their original values.
    /// </summary>
    private void RevertCurrentRecord()
    {
        if (CurrentRecordIndex < 0) return;
        var row = CurrentRecordIndex;
        try
        {
            Services.Pending.DiscardForRow(TableName, row);
        }
        catch
        {
            // Workflow pages (e.g. Settings) have no real table; just re-read.
        }
        Services.NotifyPendingChanged();
        ShowRecord(row);
        Toast.ShowInfo(this, "Reverted staged edits for this record.");
    }

    private const int PreferredBrowserWidth = 430;
    private const int BrowserMin = 240;
    private const int EditorMin = 380;
    private bool _splitConfigured;

    /// <summary>Apply split min-sizes and distance once the control is wide enough; keep clamped on resize.</summary>
    private void ConfigureSplit()
    {
        if (_split.Panel1Collapsed) return;
        int w = _split.Width;
        // Need enough room for both panels + splitter; otherwise defer (minimised / not yet laid out).
        if (w < BrowserMin + EditorMin + _split.SplitterWidth) return;

        try
        {
            if (_split.Panel1MinSize != BrowserMin) _split.Panel1MinSize = BrowserMin;
            if (_split.Panel2MinSize != EditorMin) _split.Panel2MinSize = EditorMin;
            if (!_splitConfigured)
            {
                _split.SplitterDistance = Math.Min(PreferredBrowserWidth, w - EditorMin - _split.SplitterWidth);
                _splitConfigured = true;
            }
            ClampSplitter();
        }
        catch (ArgumentOutOfRangeException)
        {
            // Transient layout state; the next resize re-applies.
        }
    }

    /// <summary>Keep SplitterDistance within valid bounds.</summary>
    private void ClampSplitter()
    {
        if (_split.Panel1Collapsed || !_splitConfigured) return;
        int w = _split.Width - _split.SplitterWidth;
        int min = _split.Panel1MinSize;
        int max = w - _split.Panel2MinSize;
        if (max < min) return;
        try
        {
            if (_split.SplitterDistance < min) _split.SplitterDistance = min;
            else if (_split.SplitterDistance > max) _split.SplitterDistance = max;
        }
        catch (ArgumentOutOfRangeException) { /* ignore transient states */ }
    }

    /// <summary>Called once when the section becomes visible / DB loaded. Loads list data.</summary>
    public virtual void ActivateSection()
    {
        ConfigureSplit();
        LoadData();
    }

    public void LoadData()
    {
        // Workflow-only pages such as Data Sync do not have records to select.
        // Their controls must remain visible instead of being covered by the
        // generic record empty state.
        if (!ShowRecordCommandStrip)
        {
            EmptyState.Visible = false;
            Tabs.BringToFront();
            return;
        }
        if (!Services.Session.IsLoaded)
        {
            Browser.SetItems(Array.Empty<RecordListItem>());
            SetPickerItems(Array.Empty<RecordListItem>());
            Header.Clear("No database loaded");
            return;
        }
        try
        {
            var records = GetRecords();
            Browser.SetItems(records);
            SetPickerItems(records);
        }
        catch (Exception ex)
        {
            Header.Clear("Load error");
            MessageBox.Show(this, ex.Message, SectionTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected abstract IReadOnlyList<RecordListItem> GetRecords();

    private void OnRecordSelected(int recordIndex)
    {
        CurrentRecordIndex = recordIndex;
        SyncPicker(recordIndex);
        if (recordIndex < 0)
        {
            EmptyState.Visible = true;
            EmptyState.BringToFront();
            Header.Clear("Select a record to edit");
            ClearPreview();
            return;
        }
        EmptyState.Visible = false;
        // Generic header wiring for sections that do not manage the header
        // themselves; sections with their own header calls override it in
        // ShowRecord. Sections that hide the header keep it hidden.
        if (Header.Visible)
            Header.SetRecord(SectionTitle, PickerTitle(recordIndex), IconService.Get(SectionKey, 44));
        ShowRecord(recordIndex);
    }

    private string PickerTitle(int recordIndex)
    {
        foreach (var item in _recordPicker.Items)
            if (item is RecordListItem li && li.RecordIndex == recordIndex)
                return li.Title;
        return string.Empty;
    }

    protected abstract void ShowRecord(int recordIndex);

    protected virtual void CreateNewRecord()
    {
    }

    private void SetPickerItems(IReadOnlyList<RecordListItem> records)
    {
        _syncPicker = true;
        try
        {
            var previous = _recordPicker.SelectedItem as RecordListItem;
            _recordPicker.BeginUpdate();
            _recordPicker.Items.Clear();
            foreach (var record in records)
                _recordPicker.Items.Add(record);
            _recordCount.Text = $"{records.Count:N0} records";
            if (_recordPicker.Items.Count > 0)
            {
                // Keep the user's selection across a reload (refresh, duplicate,
                // delete) whenever the same record still exists; fall back to the
                // first record only when it no longer does.
                var keep = previous == null
                    ? -1
                    : _recordPicker.Items.OfType<RecordListItem>()
                        .Select((item, index) => (item, index))
                        .FirstOrDefault(t => t.item.RecordIndex == previous.RecordIndex)
                        .index;
                _recordPicker.SelectedIndex = keep >= 0 ? keep : 0;
            }
            else
            {
                CurrentRecordIndex = -1;
            }
        }
        finally
        {
            _recordPicker.EndUpdate();
            _syncPicker = false;
        }

        if (_recordPicker.SelectedItem is RecordListItem first)
            OnRecordSelected(first.RecordIndex);
        else
            OnRecordSelected(-1);
    }

    private void SyncPicker(int recordIndex)
    {
        if (_syncPicker) return;
        for (int i = 0; i < _recordPicker.Items.Count; i++)
        {
            if (_recordPicker.Items[i] is RecordListItem item && item.RecordIndex == recordIndex)
            {
                _syncPicker = true;
                _recordPicker.SelectedIndex = i;
                _syncPicker = false;
                break;
            }
        }
    }

    private void FindRecord()
    {
        var query = _recordSearch.Text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            _recordSearch.Focus();
            return;
        }

        var match = _recordPicker.Items.OfType<RecordListItem>().FirstOrDefault(item => item.Matches(query));
        if (match == null)
        {
            MessageBox.Show(this, $"No {SectionTitle.ToLowerInvariant()} record matches '{query}'.", "Find", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }
        OnRecordSelected(match.RecordIndex);
    }

    /// <summary>
    /// Resolve and show the asset preview for the current record. Pass the real local file path
    /// (from <see cref="AppServices.Assets"/>); an empty/null path shows an honest "no asset" state.
    /// </summary>
    protected void ShowPreview(string? filePath, string? caption = null, string unavailableReason = "No local asset")
    {
        if (!HasPreview) return;
        if (string.IsNullOrWhiteSpace(filePath))
            Preview.ShowUnavailable(unavailableReason);
        else
            Preview.ShowAsset(filePath, caption);
    }

    /// <summary>Clear the preview to an honest empty state (called when no record is selected).</summary>
    protected void ClearPreview() { if (HasPreview) Preview.ShowUnavailable("Select a record"); }

    protected void ValidateCurrent()
    {
        var issues = Services.Validation.ValidateAll(Services.Pending.Changes);
        Validation.SetIssues(issues);
        if (issues.Count == 0)
            MessageBox.Show(this, "No validation issues. Staged changes are ready to save.", SectionTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Stage one field edit; surface engine errors; reflect modified state.</summary>
    protected bool StageField(string tableName, int rowIndex, string fieldName, string value, FieldEditorGrid grid)
    {
        EditOutcome outcome;
        try
        {
            outcome = Services.Pending.Stage(tableName, rowIndex, fieldName, value);
        }
        catch (Exception ex)
        {
            Toast.ShowError(this, ex.Message);
            return false;
        }
        if (!outcome.Success)
        {
            Toast.ShowError(this, outcome.Message);
            return false;
        }
        grid.MarkModified(fieldName, Services.Pending.IsFieldModified(tableName, rowIndex, fieldName));
        Services.NotifyPendingChanged();
        return true;
    }

    /// <summary>Index of a column by name (shared by all sections).</summary>
    protected static int Col(CM26.Application.Models.DbTable t, string name)
    {
        for (int i = 0; i < t.Columns.Count; i++)
            if (t.Columns[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    protected static int Parse(string s) => int.TryParse(s, out var v) ? v : 0;

    protected int NextAvailableId(string tableName, string idField)
    {
        // The cached schema snapshot keeps a stale row count after staged
        // inserts (new rows are inserted at the top, shifting the original
        // rows down), which made this scan miss the table tail and hand out
        // ids that already exist. Refresh first so every live row is scanned.
        Services.Session.RefreshSchema();
        var table = Services.Session.GetTable(tableName)
            ?? throw new InvalidOperationException($"Table '{tableName}' is unavailable.");
        var column = table.FindColumn(idField)
            ?? throw new InvalidOperationException($"Field '{idField}' is unavailable.");
        var used = new HashSet<int>();
        var max = column.RangeLow;
        for (var row = 0; row < table.RowCount; row++)
        {
            if (!int.TryParse(Services.Session.GetCell(tableName, row, idField), out var id)) continue;
            used.Add(id);
            if (id > max) max = id;
        }

        var minimum = Math.Max(column.RangeLow, 1);
        if (max < column.RangeHigh && !used.Contains(max + 1))
            return Math.Max(minimum, max + 1);
        for (var id = minimum; id <= column.RangeHigh; id++)
            if (!used.Contains(id))
                return id;
        throw new InvalidOperationException($"No unused {idField} remains in the supported range.");
    }

    /// <summary>
    /// Picks the next unused team id that does not collide with a crest the
    /// installed game already maps to that id (historic/pro-club ids keep their
    /// game crests, so a brand-new custom team must never take one of those or
    /// it would silently display the wrong club's crest).
    /// </summary>
    protected int FindSafeTeamId()
    {
        var table = Services.Session.GetTable("teams")
            ?? throw new InvalidOperationException("Table 'teams' is unavailable.");
        var used = new HashSet<int>();
        var max = 0;
        for (var row = 0; row < table.RowCount; row++)
        {
            if (int.TryParse(Services.Session.GetCell("teams", row, "teamid"), out var id))
            {
                used.Add(id);
                if (id > max) max = id;
            }
        }
        var candidate = Math.Max(max + 1, 1);
        if (!Services.FrostbiteAssets.IsAvailable)
        {
            while (used.Contains(candidate)) candidate++;
            return candidate;
        }
        var groupMax = new Dictionary<int, int>();
        while (true)
        {
            var group = candidate / 10;
            if (!groupMax.TryGetValue(group, out var crestUpTo))
            {
                crestUpTo = -1;
                foreach (var match in Services.FrostbiteAssets.SearchAssets($"crest_{group}", "Res", 500))
                {
                    var m = Regex.Match(match.Name, @"crest_(\d+)_");
                    if (m.Success && int.TryParse(m.Groups[1].Value, out var id) && id / 10 == group)
                        crestUpTo = Math.Max(crestUpTo, id);
                }
                groupMax[group] = crestUpTo;
            }
            if (candidate <= crestUpTo)
            {
                candidate = crestUpTo + 1;
                continue;
            }
            if (!used.Contains(candidate)) return candidate;
            candidate++;
        }
    }

    protected int CreateRecordFromTemplate(
        string tableName,
        string idField,
        IReadOnlyDictionary<string, string> values,
        int? templateRow = null)
    {
        var table = Services.Session.GetTable(tableName)
            ?? throw new InvalidOperationException($"Table '{tableName}' is unavailable.");
        if (table.RowCount == 0)
            throw new InvalidOperationException($"Table '{tableName}' has no safe template record.");

        var sourceRow = Math.Clamp(templateRow ?? CurrentRecordIndex, 0, table.RowCount - 1);
        var duplicate = Services.Session.DuplicateRow(tableName, sourceRow);
        if (!duplicate.Success)
            throw new InvalidOperationException(duplicate.Message);
        // The native engine inserts the duplicated row directly after the
        // source row (index sourceRow + 1), never at the end of the table.
        var newRow = sourceRow + 1;
        // Refresh the row count so id pickers see the shifted table (the last
        // original row moved down one position after the insert).
        Services.Session.RefreshSchema();

        var id = tableName.Equals("teams", StringComparison.OrdinalIgnoreCase)
            ? FindSafeTeamId()
            : NextAvailableId(tableName, idField);
        var staged = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase)
        {
            [idField] = id.ToString(),
        };
        foreach (var (field, value) in staged)
        {
            if (table.FindColumn(field) == null) continue;
            // Track the initial values as ordinary pending edits as well as a
            // structural insert. This keeps a newly-created record's typed
            // name visible to the section models before Save and lets the
            // normal validation/reporting pipeline see every field.
            var outcome = Services.Pending.Stage(tableName, newRow, field, value);
            if (!outcome.Success)
                throw new InvalidOperationException(outcome.Message);
        }

        Services.Pending.MarkStructuralChange();
        Services.Session.RefreshSchema();
        Services.NotifyPendingChanged();
        // This helper is also used by cross-section commands (for example,
        // creating a player while importing a team squad).  Reloading the
        // current section with a row index from another table selected an
        // unrelated record, which is why newly-created teams appeared to turn
        // into a historic club in the League page.
        if (tableName.Equals(TableName, StringComparison.OrdinalIgnoreCase))
        {
            LoadData();
            SelectRecord(newRow);
        }
        return id;
    }

    protected void SelectRecord(int recordIndex) => OnRecordSelected(recordIndex);

    private static readonly string[] DefaultSquadPositions =
    {
        "GK", "RB", "CB", "CB", "CB", "LB", "CDM", "CM", "CM", "CAM",
        "ST", "GK", "LB", "CB", "CM", "RM", "LM", "ST", "ST", "CM",
        "RM", "LM", "RW",
    };

    private static readonly string[] PositionLabels =
    {
        "GK", "SW", "RWB", "RB", "RCB", "CB", "LCB", "LB", "LWB",
        "RDM", "CDM", "LDM", "RM", "RCM", "CM", "LCM", "LM",
        "RAM", "CAM", "LAM", "RF", "CF", "LF", "RW", "RS", "ST", "LS", "LW",
    };

    protected static bool TryPositionCode(string label, out int code)
    {
        code = Array.IndexOf(PositionLabels, label.ToUpperInvariant());
        return code >= 0;
    }

    /// <summary>
    /// Creates a full default squad (Player 1..23) for a brand-new team and links
    /// each player via teamplayerlinks, so the user can open the Team page and just
    /// rename the rows instead of creating them one by one.
    /// </summary>
    protected int FillTeamSquad(int teamId)
    {
        var links = Services.Session.GetTable("teamplayerlinks")
            ?? throw new InvalidOperationException("The team-player link table is unavailable.");
        if (links.RowCount == 0)
            throw new InvalidOperationException("The team-player link table has no safe template row.");

        var playerIds = new List<int>();
        for (var i = 1; i <= DefaultSquadPositions.Length; i++)
        {
            var positionCode = TryPositionCode(DefaultSquadPositions[i - 1], out var code) ? code : 0;
            var name = $"Player {i}";
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["teamid"] = teamId.ToString(),
                ["jerseynumber"] = i.ToString(),
                ["firstnameid"] = "0",
                ["lastnameid"] = "0",
                ["commonnameid"] = "0",
                ["playerjerseynameid"] = "0",
                ["headclasscode"] = "0",
                ["preferredposition1"] = positionCode.ToString(),
            };
            var playerId = CreateRecordFromTemplate("players", "playerid", values, templateRow: 0);
            playerIds.Add(playerId);
            Services.SetPlayerNameOverride(playerId, name, string.Empty);
            CreateSquadEditedPlayerName(playerId, name);
            CreateSquadPlayerLink(playerId, teamId, i, positionCode.ToString());
        }

        CreateSquadTeamsSheet(teamId, playerIds);
        Services.Pending.MarkStructuralChange();
        Services.Session.RefreshSchema();
        Services.RefreshDatabaseIndexes();
        Services.NotifyPendingChanged();
        return playerIds.Count;
    }

    private void CreateSquadPlayerLink(int playerId, int teamId, int jerseyNumber, string position)
    {
        var links = Services.Session.GetTable("teamplayerlinks");
        var duplicate = Services.Session.DuplicateRow("teamplayerlinks", 0);
        if (!duplicate.Success) throw new InvalidOperationException(duplicate.Message);
        var row = 1;
        var fields = new Dictionary<string, string>
        {
            ["playerid"] = playerId.ToString(),
            ["teamid"] = teamId.ToString(),
            ["jerseynumber"] = jerseyNumber.ToString(),
            ["position"] = position,
        };
        // teamplayerlinks keys on its artificialkey column; a duplicated template
        // row keeps the template's value, so a unique key must be staged or the
        // save's integrity check rejects every new link as a duplicate.
        if (links?.FindColumn("artificialkey") != null)
            fields["artificialkey"] = NextAvailableId("teamplayerlinks", "artificialkey").ToString();
        foreach (var (field, value) in fields)
        {
            if (links?.FindColumn(field) == null) continue;
            var result = Services.Pending.Stage("teamplayerlinks", row, field, value);
            if (!result.Success) throw new InvalidOperationException(result.Message);
        }
    }

    private void CreateSquadTeamsSheet(int teamId, IReadOnlyList<int> playerIds)
    {
        var sheets = Services.Session.GetTable("default_teamsheets");
        if (sheets == null || sheets.RowCount == 0) return;
        var duplicate = Services.Session.DuplicateRow("default_teamsheets", 0);
        if (!duplicate.Success) return;
        var row = 1;
        if (sheets.FindColumn("teamid") != null)
            Services.Pending.Stage("default_teamsheets", row, "teamid", teamId.ToString());
        for (var index = 0; index < DefaultSquadPositions.Length && index < playerIds.Count; index++)
        {
            var field = $"playerid{index}";
            if (sheets.FindColumn(field) == null) continue;
            Services.Pending.Stage("default_teamsheets", row, field, playerIds[index].ToString());
        }
    }

    /// <summary>Creates a safe, editable display-name row so the user can rename the placeholder player.</summary>
    protected void CreateSquadEditedPlayerName(int playerId, string name)
    {
        var names = Services.Session.GetTable("editedplayernames");
        if (names == null || names.RowCount == 0) return;
        var duplicate = Services.Session.DuplicateRow("editedplayernames", 0);
        if (!duplicate.Success) return;
        var row = 1;
        foreach (var (field, value) in new Dictionary<string, string>
        {
            ["playerid"] = playerId.ToString(),
            ["firstname"] = name,
            ["surname"] = string.Empty,
            ["commonname"] = string.Empty,
            ["playerjerseyname"] = name,
        })
        {
            if (names.FindColumn(field) == null) continue;
            var outcome = Services.Session.StageEdit("editedplayernames", row, field, value);
            if (!outcome.Success) return;
        }
        Services.Pending.MarkStructuralChange();
    }

    protected static TabPage MakeTab(string title, Control content)
    {
        var page = new TabPage(title) { BackColor = Theme.Background, Padding = new Padding(0) };
        content.Dock = DockStyle.Fill;
        page.Controls.Add(content);
        return page;
    }

    public void FocusSearchBox() => _recordSearch.Focus();
    public void GoToRecord(int recordIndex) => Browser.SelectRecord(recordIndex);
}
