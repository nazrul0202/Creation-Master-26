using System;
using System.Drawing;
using System.IO;
using ThreadingTask = System.Threading.Tasks.Task;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>Career data is intentionally kept separate from the direct Frostbite database session.</summary>
internal sealed class Fc26CareerSaveForm : Form
{
    private readonly Label _file = new Label();
	private readonly Label _type = new Label();
    private readonly Label _club = new Label();
    private readonly Label _status = new Label();
    private readonly NumericUpDown _current = MoneyBox();
    private readonly NumericUpDown _season = MoneyBox();
    private readonly Button _save = new Button { Text = "Save budget + backup", AutoSize = true, Enabled = false };
    private CareerBudgetEditor? _editor;

    internal Fc26CareerSaveForm()
    {
        Text = "FC26 Career Save Module";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(690, 340);
        MinimumSize = new Size(620, 330);
        AutoScaleMode = AutoScaleMode.Dpi;
        Icon = Form.ActiveForm?.Icon;

        var header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(5, 38, 82), Padding = new Padding(16) };
        header.Controls.Add(new Label { Text = "FC26 CAREER SAVE", ForeColor = Color.White, Font = new Font(Font.FontFamily, 15, FontStyle.Bold), AutoSize = true });
        header.Controls.Add(new Label { Text = "Separate Career container — CRC/recompression and timestamped backup on save", ForeColor = Color.WhiteSmoke, AutoSize = true, Location = new Point(18, 43) });

		var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 3, RowCount = 7 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
		table.Controls.Add(Label("Career file"), 0, 0); table.Controls.Add(_file, 1, 0); table.SetColumnSpan(_file, 2);
		table.Controls.Add(Label("Career type / tables"), 0, 1); table.Controls.Add(_type, 1, 1); table.SetColumnSpan(_type, 2);
		table.Controls.Add(Label("Active club team ID"), 0, 2); table.Controls.Add(_club, 1, 2);
		table.Controls.Add(Label("Current transfer budget"), 0, 3); table.Controls.Add(_current, 1, 3);
		table.Controls.Add(Label("Start-of-season budget"), 0, 4); table.Controls.Add(_season, 1, 4);
        var open = new Button { Text = "Open Career save...", AutoSize = true };
        var latest = new Button { Text = "Load latest Career", AutoSize = true };
        open.Click += async (_, _) => await OpenSelected(); latest.Click += async (_, _) => await OpenLatest(); _save.Click += async (_, _) => await Save();
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true }; actions.Controls.Add(open); actions.Controls.Add(latest); actions.Controls.Add(_save);
		table.Controls.Add(actions, 0, 5); table.SetColumnSpan(actions, 3);
		_status.AutoSize = true; _status.ForeColor = Color.DarkGreen; table.Controls.Add(_status, 0, 6); table.SetColumnSpan(_status, 3);
        _file.AutoEllipsis = true; _file.Dock = DockStyle.Fill; _club.AutoSize = true;
        _status.Text = "Open an FC26 Manager Career save. Player Career data remains read-only unless its exact table mapping is available.";
        Controls.Add(table); Controls.Add(header);
    }

    private async ThreadingTask OpenSelected()
    {
        using var dialog = new OpenFileDialog { Filter = "FC26 Career saves (Career*)|Career*|All files (*.*)|*.*" };
        var candidates = CareerBudgetEditor.FindCareerSaveCandidates();
        if (candidates.Count > 0) dialog.InitialDirectory = Path.GetDirectoryName(candidates[0]);
        if (dialog.ShowDialog(this) == DialogResult.OK) await LoadEditor(dialog.FileName);
    }

    private async ThreadingTask OpenLatest()
    {
        var candidates = CareerBudgetEditor.FindCareerSaveCandidates();
        if (candidates.Count == 0) { MessageBox.Show(this, "No FC26 Career save was found in the EA SPORTS FC 26 settings folder."); return; }
        await LoadEditor(candidates[0]);
    }

    private async ThreadingTask LoadEditor(string fileName)
    {
        var schema = FifaEnvironment.FifaXmlFileName;
        if (string.IsNullOrWhiteSpace(schema) || !File.Exists(schema))
        {
            MessageBox.Show(this, "Open FC26 or an extracted FC26 database first so CM26 can use its matching XML descriptor.", "Career schema", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            UseWaitCursor = true; _status.Text = "Reading Career container...";
            _editor = await ThreadingTask.Run(() => CareerBudgetEditor.Open(fileName, schema));
			_file.Text = _editor.FileName; _club.Text = _editor.ClubTeamId > 0 ? _editor.ClubTeamId.ToString() : "Not stored / not applicable";
			_type.Text = _editor.CareerType + " · " + _editor.TableCount + " table(s)";
			_current.Value = Math.Min(_current.Maximum, _editor.TransferBudget);
			_season.Value = Math.Min(_season.Maximum, _editor.StartOfSeasonTransferBudget);
			_current.Enabled = _editor.SupportsBudgetEditing; _season.Enabled = _editor.SupportsBudgetEditing;
			_save.Enabled = _editor.SupportsBudgetEditing;
			_status.Text = _editor.SupportsBudgetEditing
				? "Manager Career budget loaded. Changes are written only when Save is pressed."
				: "Player Career container loaded safely. No verified writable budget table exists, so this save remains read-only.";
            Fc26ActivityLog.Add("Career", "Loaded Career save: " + fileName);
        }
		catch (Exception ex) { _editor = null; _save.Enabled = false; _current.Enabled = false; _season.Enabled = false; Fc26FriendlyError.Show(this, "Open Career save", ex, "Select an unmodified FC26 Career save and retry."); }
        finally { UseWaitCursor = false; }
    }

    private async ThreadingTask Save()
    {
        if (_editor == null) return;
        if (MessageBox.Show(this, "Write both budget values to the selected Career save? CM26 will create a timestamped backup first.", "Career save preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            UseWaitCursor = true; _save.Enabled = false;
            var backup = await ThreadingTask.Run(() => _editor.Save((int)_current.Value, (int)_season.Value));
            _status.Text = "Career budget saved and reload-verified. Backup: " + backup;
            Fc26ActivityLog.Add("Career save", "Saved and reload-verified Career budget; backup: " + backup);
            MessageBox.Show(this, "Career budget saved and reload-verified.\r\n\r\nBackup: " + backup, "Career save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { Fc26FriendlyError.Show(this, "Career save", ex, "The timestamped backup is retained. The editor will not report success unless the written budget reload-verifies."); }
		finally { UseWaitCursor = false; _save.Enabled = _editor?.SupportsBudgetEditing == true; }
    }

	private static NumericUpDown MoneyBox() => new NumericUpDown { Maximum = 2000000000M, ThousandsSeparator = true, Width = 180, Enabled = false };
    private static Label Label(string text) => new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left };
}
