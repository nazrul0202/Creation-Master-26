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
		openFile = new Button { Dock = DockStyle.Bottom, Height = 30, Text = "Open exported file" };
		split.Panel1.Controls.Add(assets);
		split.Panel2.Controls.Add(preview);
		split.Panel2.Controls.Add(fileInfo);
		split.Panel2.Controls.Add(openFile);
		Controls.Add(split);
		assets.SelectedIndexChanged += async (_, _) => await LoadSelectedAsset();
		openFile.Click += (_, _) => { if (File.Exists(fileInfo.Tag as string)) Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + fileInfo.Tag + "\"") { UseShellExecute = true }); };
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
