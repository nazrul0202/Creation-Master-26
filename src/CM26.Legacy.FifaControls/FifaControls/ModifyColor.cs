using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class ModifyColor : Form
{
	private Bitmap m_InputBitmap;

	private Bitmap m_OutputBitmap;

	private int m_X;

	private int m_Y;

	private Color m_TargetColor;

	private IContainer components;

	private PictureBox pictureBox;

	private Button buttonOk;

	private Button buttonCancel;

	private TrackBar trackBarRed;

	private Label label4;

	private TrackBar trackBarGreen;

	private TrackBar trackBarBlue;

	private Label label5;

	private Label label6;

	private Label label7;

	private Button buttonReset;

	private ContextMenuStrip contextMenu;

	private ToolStripMenuItem menuSampleRGB;

	private ToolStripMenuItem menuApplyRGB;

	public Bitmap InputBitmap
	{
		set
		{
			m_InputBitmap = value;
			m_OutputBitmap = (Bitmap)m_InputBitmap.Clone();
			ResetTrackBars();
			m_OutputBitmap = GraphicUtil.AddColorOffsetPreservingAlfa(m_InputBitmap, trackBarRed.Value, trackBarGreen.Value, trackBarBlue.Value, preserveAlfa: false);
			pictureBox.BackgroundImage = m_OutputBitmap;
			pictureBox.Refresh();
		}
	}

	public Bitmap OutputBitmap => m_OutputBitmap;

	public void ResetTrackBars()
	{
		trackBarRed.Value = 0;
		trackBarGreen.Value = 0;
		trackBarBlue.Value = 0;
	}

	public ModifyColor()
	{
		InitializeComponent();
		trackBarRed.Value = 0;
		trackBarGreen.Value = 0;
		trackBarBlue.Value = 0;
	}

	public ModifyColor(Bitmap inputBitmap)
	{
		InitializeComponent();
		trackBarRed.Value = 0;
		trackBarGreen.Value = 0;
		trackBarBlue.Value = 0;
		m_InputBitmap = inputBitmap;
		m_OutputBitmap = (Bitmap)m_InputBitmap.Clone();
		m_OutputBitmap = GraphicUtil.AddColorOffsetPreservingAlfa(m_InputBitmap, trackBarRed.Value, trackBarGreen.Value, trackBarBlue.Value, preserveAlfa: false);
		pictureBox.BackgroundImage = m_OutputBitmap;
		pictureBox.Refresh();
	}

	private void trackBar_MouseUp(object sender, MouseEventArgs e)
	{
		m_OutputBitmap = GraphicUtil.AddColorOffsetPreservingAlfa(m_InputBitmap, trackBarRed.Value, trackBarGreen.Value, trackBarBlue.Value, preserveAlfa: false);
		pictureBox.BackgroundImage = m_OutputBitmap;
		pictureBox.Refresh();
	}

	private void buttonReset_Click(object sender, EventArgs e)
	{
		trackBarRed.Value = 0;
		trackBarGreen.Value = 0;
		trackBarBlue.Value = 0;
		m_OutputBitmap = GraphicUtil.AddColorOffsetPreservingAlfa(m_InputBitmap, trackBarRed.Value, trackBarGreen.Value, trackBarBlue.Value, preserveAlfa: false);
		pictureBox.BackgroundImage = m_OutputBitmap;
		pictureBox.Refresh();
	}

	private void buttonOk_Click(object sender, EventArgs e)
	{
		m_OutputBitmap = (Bitmap)m_InputBitmap.Clone();
		m_OutputBitmap = GraphicUtil.AddColorOffsetPreservingAlfa(m_InputBitmap, trackBarRed.Value, trackBarGreen.Value, trackBarBlue.Value, preserveAlfa: true);
	}

	private void menuSampleRGB_Click(object sender, EventArgs e)
	{
		int num = ((m_X >= 4) ? (m_X * 2 - 8) : 0);
		num = ((m_X < 252) ? (m_X * 2 - 8) : 496);
		int num2 = ((m_Y >= 4) ? (m_Y * 2 - 8) : 0);
		num2 = ((m_Y < 252) ? (m_Y * 2 - 8) : 496);
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = num; i < num + 16; i++)
		{
			for (int j = num2; j < num2 + 16; j++)
			{
				Color pixel = m_OutputBitmap.GetPixel(i, j);
				num3 += pixel.R;
				num4 += pixel.G;
				num5 += pixel.B;
			}
		}
		num3 /= 256;
		num4 /= 256;
		num5 /= 256;
		m_TargetColor = Color.FromArgb(num3, num4, num5);
	}

	private void menuApplyRGB_Click(object sender, EventArgs e)
	{
		int num = ((m_X >= 4) ? (m_X * 2 - 8) : 0);
		num = ((m_X < 252) ? (m_X * 2 - 8) : 496);
		int num2 = ((m_Y >= 4) ? (m_Y * 2 - 8) : 0);
		num2 = ((m_Y < 252) ? (m_Y * 2 - 8) : 496);
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = num; i < num + 16; i++)
		{
			for (int j = num2; j < num2 + 16; j++)
			{
				Color pixel = m_InputBitmap.GetPixel(i, j);
				num3 += pixel.R;
				num4 += pixel.G;
				num5 += pixel.B;
			}
		}
		num3 /= 256;
		num4 /= 256;
		num5 /= 256;
		trackBarRed.Value = m_TargetColor.R - num3;
		trackBarGreen.Value = m_TargetColor.G - num4;
		trackBarBlue.Value = m_TargetColor.B - num5;
		m_OutputBitmap = GraphicUtil.AddColorOffsetPreservingAlfa(m_InputBitmap, trackBarRed.Value, trackBarGreen.Value, trackBarBlue.Value, preserveAlfa: false);
		pictureBox.BackgroundImage = m_OutputBitmap;
		pictureBox.Refresh();
	}

	private void pictureBox_MouseMove(object sender, MouseEventArgs e)
	{
		m_X = e.X;
		m_Y = e.Y;
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		m_OutputBitmap = m_InputBitmap;
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
		this.components = new System.ComponentModel.Container();
		this.pictureBox = new System.Windows.Forms.PictureBox();
		this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.menuSampleRGB = new System.Windows.Forms.ToolStripMenuItem();
		this.menuApplyRGB = new System.Windows.Forms.ToolStripMenuItem();
		this.buttonOk = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.trackBarRed = new System.Windows.Forms.TrackBar();
		this.label4 = new System.Windows.Forms.Label();
		this.trackBarGreen = new System.Windows.Forms.TrackBar();
		this.trackBarBlue = new System.Windows.Forms.TrackBar();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.buttonReset = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
		this.contextMenu.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackBarRed).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarGreen).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarBlue).BeginInit();
		base.SuspendLayout();
		this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBox.ContextMenuStrip = this.contextMenu;
		this.pictureBox.Location = new System.Drawing.Point(12, 12);
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.Size = new System.Drawing.Size(256, 256);
		this.pictureBox.TabIndex = 0;
		this.pictureBox.TabStop = false;
		this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(pictureBox_MouseMove);
		this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.menuSampleRGB, this.menuApplyRGB });
		this.contextMenu.Name = "contextMenu";
		this.contextMenu.Size = new System.Drawing.Size(139, 48);
		this.menuSampleRGB.Name = "menuSampleRGB";
		this.menuSampleRGB.Size = new System.Drawing.Size(138, 22);
		this.menuSampleRGB.Text = "Sample RGB";
		this.menuSampleRGB.Click += new System.EventHandler(menuSampleRGB_Click);
		this.menuApplyRGB.Name = "menuApplyRGB";
		this.menuApplyRGB.Size = new System.Drawing.Size(138, 22);
		this.menuApplyRGB.Text = "Apply RGB";
		this.menuApplyRGB.Click += new System.EventHandler(menuApplyRGB_Click);
		this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOk.Location = new System.Drawing.Point(12, 456);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 1;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonOk.Click += new System.EventHandler(buttonOk_Click);
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(197, 454);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 2;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.trackBarRed.LargeChange = 25;
		this.trackBarRed.Location = new System.Drawing.Point(55, 315);
		this.trackBarRed.Maximum = 250;
		this.trackBarRed.Minimum = -250;
		this.trackBarRed.Name = "trackBarRed";
		this.trackBarRed.Size = new System.Drawing.Size(217, 45);
		this.trackBarRed.TabIndex = 6;
		this.trackBarRed.TickFrequency = 25;
		this.trackBarRed.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBar_MouseUp);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(158, 414);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(13, 13);
		this.label4.TabIndex = 7;
		this.label4.Text = "0";
		this.trackBarGreen.LargeChange = 25;
		this.trackBarGreen.Location = new System.Drawing.Point(55, 360);
		this.trackBarGreen.Maximum = 250;
		this.trackBarGreen.Minimum = -250;
		this.trackBarGreen.Name = "trackBarGreen";
		this.trackBarGreen.Size = new System.Drawing.Size(217, 45);
		this.trackBarGreen.TabIndex = 8;
		this.trackBarGreen.TickFrequency = 25;
		this.trackBarGreen.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBar_MouseUp);
		this.trackBarBlue.LargeChange = 25;
		this.trackBarBlue.Location = new System.Drawing.Point(55, 405);
		this.trackBarBlue.Maximum = 250;
		this.trackBarBlue.Minimum = -250;
		this.trackBarBlue.Name = "trackBarBlue";
		this.trackBarBlue.Size = new System.Drawing.Size(217, 45);
		this.trackBarBlue.TabIndex = 9;
		this.trackBarBlue.TickFrequency = 25;
		this.trackBarBlue.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBar_MouseUp);
		this.label5.BackColor = System.Drawing.Color.FromArgb(0, 0, 192);
		this.label5.ForeColor = System.Drawing.Color.White;
		this.label5.Location = new System.Drawing.Point(9, 405);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(40, 14);
		this.label5.TabIndex = 12;
		this.label5.Text = "Blue";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label6.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.label6.ForeColor = System.Drawing.Color.White;
		this.label6.Location = new System.Drawing.Point(9, 360);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(40, 14);
		this.label6.TabIndex = 11;
		this.label6.Text = "Green";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label7.BackColor = System.Drawing.Color.Red;
		this.label7.ForeColor = System.Drawing.Color.White;
		this.label7.Location = new System.Drawing.Point(9, 315);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(40, 14);
		this.label7.TabIndex = 10;
		this.label7.Text = "Red";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.buttonReset.Location = new System.Drawing.Point(122, 274);
		this.buttonReset.Name = "buttonReset";
		this.buttonReset.Size = new System.Drawing.Size(75, 23);
		this.buttonReset.TabIndex = 13;
		this.buttonReset.Text = "Reset";
		this.buttonReset.UseVisualStyleBackColor = true;
		this.buttonReset.Click += new System.EventHandler(buttonReset_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(280, 489);
		base.Controls.Add(this.buttonReset);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.trackBarBlue);
		base.Controls.Add(this.trackBarGreen);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.trackBarRed);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOk);
		base.Controls.Add(this.pictureBox);
		base.Name = "ModifyColor";
		this.Text = "Modify Color";
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		this.contextMenu.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.trackBarRed).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarGreen).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarBlue).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
