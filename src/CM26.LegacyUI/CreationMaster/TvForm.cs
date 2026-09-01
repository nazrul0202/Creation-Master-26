using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace CreationMaster;

public class TvForm : Form
{
	private IContainer components;
	private ListView assets;
	private PictureBox preview;
	private Label fileInfo;
	private Button openFile;
	private Button exportFile;
	private Button replaceFile;

	public TvForm()
	{
		InitializeComponent();
		BackgroundImage = null;
		var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 430 };
		assets = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false };
		assets.Columns.Add("Scoreboard / broadcast file", 330);
		assets.Columns.Add("Type", 75);
		preview = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(45,45,45) };
		fileInfo = new Label { Dock = DockStyle.Bottom, Height = 46, AutoEllipsis = true, Padding = new Padding(8) };
		openFile = new Button { AutoSize = true, Text = "Open exported file" };
		exportFile = new Button { AutoSize = true, Text = "Export selected" };
		replaceFile = new Button { AutoSize = true, Text = "Replace native asset" };
		var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(4), WrapContents = false };
		actions.Controls.AddRange(new Control[] { openFile, exportFile, replaceFile });
		split.Panel1.Controls.Add(assets);
		split.Panel2.Controls.Add(preview);
		split.Panel2.Controls.Add(fileInfo);
		split.Panel2.Controls.Add(actions);
		Controls.Add(split);
		assets.SelectedIndexChanged += async (_, _) => await LoadSelectedAsset();
		openFile.Click += (_, _) => { if (File.Exists(fileInfo.Tag as string)) Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + fileInfo.Tag + "\"") { UseShellExecute = true }); };
		exportFile.Click += (_, _) => ExportSelected();
		replaceFile.Click += (_, _) => ReplaceSelected();
		VisibleChanged += async (_, _) => { if (Visible && assets.Items.Count == 0) await LoadCatalog(); };
	}

	private async System.Threading.Tasks.Task LoadCatalog()
	{
		fileInfo.Text = "Scanning installed scoreboard and broadcast assets...";
		try
		{
			var rows = await System.Threading.Tasks.Task.Run(Fc26HostBridge.LoadScoreboards);
			foreach (var row in rows)
			{
				var item = new ListViewItem(row.ToString()) { Tag = row };
				item.SubItems.Add(row.Type); assets.Items.Add(item);
			}
			fileInfo.Text = rows.Length + " scoreboard / broadcast assets found.";
		}
		catch (System.Exception) { fileInfo.Text = "Scoreboard data could not be loaded safely."; }
	}

	private async System.Threading.Tasks.Task LoadSelectedAsset()
	{
		if (assets.SelectedItems.Count == 0 || assets.SelectedItems[0].Tag is not Fc26HostBridge.ScoreboardAsset row) return;
		fileInfo.Text = "Exporting " + row.Name + "...";
		string path = await System.Threading.Tasks.Task.Run(() => Fc26HostBridge.ExportAsset(row.Name));
		fileInfo.Tag = path; fileInfo.Text = string.IsNullOrWhiteSpace(path) ? row.Name : path;
		var old = preview.Image; preview.Image = null; old?.Dispose();
		if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
		{
			try { using var image = Image.FromFile(path); preview.Image = new Bitmap(image); } catch { }
		}
	}

	private void ExportSelected()
	{
		var source = fileInfo.Tag as string;
		if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) { MessageBox.Show(this, "Select and load a scoreboard or broadcast asset first."); return; }
		using var dialog = new SaveFileDialog { FileName = Path.GetFileName(source), Filter = "Native FC26 asset|*" + Path.GetExtension(source) + "|All files|*.*" };
		if (dialog.ShowDialog(this) != DialogResult.OK) return;
		File.Copy(source, dialog.FileName, true); fileInfo.Text = "Exported: " + dialog.FileName;
	}

	private void ReplaceSelected()
	{
		if (assets.SelectedItems.Count == 0 || assets.SelectedItems[0].Tag is not Fc26HostBridge.ScoreboardAsset row) return;
		var logicalPath = (row.Name ?? string.Empty).Replace('\\', '/').TrimStart('/');
		if (!logicalPath.StartsWith("data/", System.StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show(this, "This indexed Frostbite entry has no verified writable data/ path. Export remains available, but CM26 will not invent an unsafe replacement path.",
				"Broadcast asset safety", MessageBoxButtons.OK, MessageBoxIcon.Information); return;
		}
		using var dialog = new OpenFileDialog { Filter = "Format-compatible native asset|*" + Path.GetExtension(logicalPath) + "|All files|*.*", CheckFileExists = true };
		if (dialog.ShowDialog(this) != DialogResult.OK) return;
		if (!Path.GetExtension(dialog.FileName).Equals(Path.GetExtension(logicalPath), System.StringComparison.OrdinalIgnoreCase))
		{ MessageBox.Show(this, "Replacement extension must match " + Path.GetExtension(logicalPath) + "."); return; }
		if (MessageBox.Show(this, "Stage this native scoreboard/broadcast replacement?\r\n\r\n" + logicalPath,
			"Broadcast replacement preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
		try
		{
			Fc26HostBridge.StageFile(logicalPath, dialog.FileName);
			fileInfo.Text = "Staged replacement: " + logicalPath + ". Use File > Save to validate and commit.";
		}
		catch (System.Exception ex) { Fc26FriendlyError.Show(this, "Broadcast replacement", ex, "No scoreboard or broadcast asset was staged."); }
	}

	public void Clean()
	{
		base.Visible = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.TvForm));
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1165, 798);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "TvForm";
		this.Text = "TvForm";
		base.ResumeLayout(false);
	}
}
