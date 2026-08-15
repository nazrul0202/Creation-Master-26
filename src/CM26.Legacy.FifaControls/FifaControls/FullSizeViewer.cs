using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class FullSizeViewer : Form
{
	private IContainer components;

	private ToolStrip toolStrip;

	private ToolStripButton toolStripButton1;

	private PictureBox pictureBox;

	public FullSizeViewer()
	{
		InitializeComponent();
	}

	public void SetImage(Image image)
	{
		base.Width = image.Width + 10;
		base.Height = image.Height + 50;
		pictureBox.BackgroundImage = image;
	}

	private void toolStripButton1_Click(object sender, EventArgs e)
	{
		Close();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.FullSizeViewer));
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
		this.pictureBox = new System.Windows.Forms.PictureBox();
		this.toolStrip.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
		base.SuspendLayout();
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.toolStripButton1 });
		this.toolStrip.Location = new System.Drawing.Point(0, 0);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.Size = new System.Drawing.Size(292, 25);
		this.toolStrip.TabIndex = 0;
		this.toolStrip.Text = "toolStrip";
		this.toolStripButton1.Image = (System.Drawing.Image)resources.GetObject("toolStripButton1.Image");
		this.toolStripButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton1.Name = "toolStripButton1";
		this.toolStripButton1.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
		this.toolStripButton1.Size = new System.Drawing.Size(69, 22);
		this.toolStripButton1.Text = "Close";
		this.toolStripButton1.Click += new System.EventHandler(toolStripButton1_Click);
		this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pictureBox.Location = new System.Drawing.Point(0, 25);
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.Size = new System.Drawing.Size(292, 241);
		this.pictureBox.TabIndex = 1;
		this.pictureBox.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoScroll = true;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(292, 266);
		base.Controls.Add(this.pictureBox);
		base.Controls.Add(this.toolStrip);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Name = "FullSizeViewer";
		this.Text = "Full Size Viewer";
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
