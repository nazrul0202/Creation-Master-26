using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class ModelManager : UserControl
{
	public delegate bool ModelExportHandler(object sender, string rx3FileName);

	public delegate bool ModelImportHandler(object sender, string rx3FileName);

	public delegate bool ModelDeleteHandler(object sender, string rx3FileName);

	private IContainer components;

	private ToolStrip toolStrip1;

	public Viewer3D viewer;

	public ToolStripButton buttonShow;

	public ToolStripButton buttonImport;

	public ToolStripButton buttonExport;

	public ToolStripButton buttonRemove;

	public ModelManager()
	{
		InitializeComponent();
	}

	private void buttonShow_Click(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.ModelManager));
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.buttonShow = new System.Windows.Forms.ToolStripButton();
		this.buttonImport = new System.Windows.Forms.ToolStripButton();
		this.buttonExport = new System.Windows.Forms.ToolStripButton();
		this.buttonRemove = new System.Windows.Forms.ToolStripButton();
		this.viewer = new FifaControls.Viewer3D();
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.buttonShow, this.buttonImport, this.buttonExport, this.buttonRemove });
		this.toolStrip1.Location = new System.Drawing.Point(0, 285);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(324, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStrip1.Text = "toolStrip";
		this.buttonShow.Checked = true;
		this.buttonShow.CheckOnClick = true;
		this.buttonShow.CheckState = System.Windows.Forms.CheckState.Checked;
		this.buttonShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow.Image = (System.Drawing.Image)resources.GetObject("buttonShow.Image");
		this.buttonShow.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow.Name = "buttonShow";
		this.buttonShow.Size = new System.Drawing.Size(23, 22);
		this.buttonShow.Text = "Show / Hide";
		this.buttonShow.Click += new System.EventHandler(buttonShow_Click);
		this.buttonImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImport.Image = (System.Drawing.Image)resources.GetObject("buttonImport.Image");
		this.buttonImport.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImport.Name = "buttonImport";
		this.buttonImport.Size = new System.Drawing.Size(23, 22);
		this.buttonImport.Text = "Import 3D Model";
		this.buttonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport.Image = (System.Drawing.Image)resources.GetObject("buttonExport.Image");
		this.buttonExport.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport.Name = "buttonExport";
		this.buttonExport.Size = new System.Drawing.Size(23, 22);
		this.buttonExport.Text = "Export 3D Model";
		this.buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove.Image = (System.Drawing.Image)resources.GetObject("buttonRemove.Image");
		this.buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove.Name = "buttonRemove";
		this.buttonRemove.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove.Text = "Remove 3D Model";
		this.viewer.BackColor = System.Drawing.Color.Gray;
		this.viewer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.viewer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.viewer.LightDirectionZ = 0f;
		this.viewer.LightX = 0f;
		this.viewer.LightY = 100f;
		this.viewer.LightZ = 100f;
		this.viewer.Location = new System.Drawing.Point(0, 0);
		this.viewer.Name = "viewer";
		this.viewer.RotationX = 0f;
		this.viewer.RotationY = 0f;
		this.viewer.Size = new System.Drawing.Size(324, 285);
		this.viewer.TabIndex = 1;
		this.viewer.ViewX = 0f;
		this.viewer.ViewY = 100f;
		this.viewer.ViewZ = 100f;
		this.viewer.ZbufferRenderState = null;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.viewer);
		base.Controls.Add(this.toolStrip1);
		base.Name = "ModelManager";
		base.Size = new System.Drawing.Size(324, 310);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
