using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

public class ModifyHairColor : Form
{
	private Bitmap m_InputBitmap;

	private Bitmap m_PreviewBitmap;

	private Bitmap m_OutputBitmap;

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

	public Bitmap Bitmap => m_OutputBitmap;

	public ModifyHairColor(Bitmap inputBitmap)
	{
		InitializeComponent();
		trackBarRed.Value = 0;
		trackBarGreen.Value = 0;
		trackBarBlue.Value = 0;
		m_InputBitmap = inputBitmap;
		m_PreviewBitmap = GraphicUtil.SubSampleBitmap(inputBitmap, 2, 2);
		m_OutputBitmap = (Bitmap)m_PreviewBitmap.Clone();
		Colorize(m_PreviewBitmap, m_OutputBitmap, preserveAlfa: false);
		pictureBox.BackgroundImage = m_OutputBitmap;
		pictureBox.Refresh();
	}

	private void Colorize(Bitmap sourceBitmap, Bitmap destBitmap, bool preserveAlfa)
	{
		if (sourceBitmap == null || sourceBitmap.PixelFormat != PixelFormat.Format32bppArgb)
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		int num = sourceBitmap.Width * sourceBitmap.Height;
		int[] array = new int[num];
		Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
		BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.WriteOnly, m_InputBitmap.PixelFormat);
		Marshal.Copy(bitmapData.Scan0, array, 0, num);
		sourceBitmap.UnlockBits(bitmapData);
		for (int i = 0; i < num; i++)
		{
			Color color = Color.FromArgb(array[i]);
			int r = color.R;
			int g = color.G;
			int b = color.B;
			int a = color.A;
			r += trackBarRed.Value;
			g += trackBarGreen.Value;
			b += trackBarBlue.Value;
			if (r > 255)
			{
				r = 255;
			}
			if (g > 255)
			{
				g = 255;
			}
			if (b > 255)
			{
				b = 255;
			}
			if (r < 0)
			{
				r = 0;
			}
			if (g < 0)
			{
				g = 0;
			}
			if (b < 0)
			{
				b = 0;
			}
			if (preserveAlfa)
			{
				array[i] = Color.FromArgb(a, r, g, b).ToArgb();
			}
			else
			{
				array[i] = Color.FromArgb(255, r, g, b).ToArgb();
			}
		}
		rect = new Rectangle(0, 0, destBitmap.Width, m_OutputBitmap.Height);
		BitmapData bitmapData2 = destBitmap.LockBits(rect, ImageLockMode.WriteOnly, m_OutputBitmap.PixelFormat);
		IntPtr scan = bitmapData2.Scan0;
		Marshal.Copy(array, 0, scan, num);
		destBitmap.UnlockBits(bitmapData2);
		Cursor.Current = Cursors.Default;
	}

	private void trackBar_MouseUp(object sender, MouseEventArgs e)
	{
		Colorize(m_PreviewBitmap, m_OutputBitmap, preserveAlfa: false);
		pictureBox.BackgroundImage = m_OutputBitmap;
		pictureBox.Refresh();
	}

	private void buttonReset_Click(object sender, EventArgs e)
	{
		trackBarRed.Value = 0;
		trackBarGreen.Value = 0;
		trackBarBlue.Value = 0;
		Colorize(m_PreviewBitmap, m_OutputBitmap, preserveAlfa: false);
		pictureBox.BackgroundImage = m_OutputBitmap;
		pictureBox.Refresh();
	}

	private void buttonOk_Click(object sender, EventArgs e)
	{
		m_OutputBitmap = (Bitmap)m_InputBitmap.Clone();
		Colorize(m_InputBitmap, m_OutputBitmap, preserveAlfa: true);
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
		this.pictureBox = new System.Windows.Forms.PictureBox();
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
		((System.ComponentModel.ISupportInitialize)this.trackBarRed).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarGreen).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarBlue).BeginInit();
		base.SuspendLayout();
		this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBox.Location = new System.Drawing.Point(12, 12);
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.Size = new System.Drawing.Size(256, 256);
		this.pictureBox.TabIndex = 0;
		this.pictureBox.TabStop = false;
		this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOk.Location = new System.Drawing.Point(12, 433);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 1;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonOk.Click += new System.EventHandler(buttonOk_Click);
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(197, 433);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 2;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
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
		this.trackBarGreen.Location = new System.Drawing.Point(55, 347);
		this.trackBarGreen.Maximum = 250;
		this.trackBarGreen.Minimum = -250;
		this.trackBarGreen.Name = "trackBarGreen";
		this.trackBarGreen.Size = new System.Drawing.Size(217, 45);
		this.trackBarGreen.TabIndex = 8;
		this.trackBarGreen.TickFrequency = 25;
		this.trackBarGreen.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBar_MouseUp);
		this.trackBarBlue.LargeChange = 25;
		this.trackBarBlue.Location = new System.Drawing.Point(55, 382);
		this.trackBarBlue.Maximum = 250;
		this.trackBarBlue.Minimum = -250;
		this.trackBarBlue.Name = "trackBarBlue";
		this.trackBarBlue.Size = new System.Drawing.Size(217, 45);
		this.trackBarBlue.TabIndex = 9;
		this.trackBarBlue.TickFrequency = 25;
		this.trackBarBlue.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBar_MouseUp);
		this.label5.BackColor = System.Drawing.Color.FromArgb(0, 0, 192);
		this.label5.ForeColor = System.Drawing.Color.White;
		this.label5.Location = new System.Drawing.Point(9, 386);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(40, 14);
		this.label5.TabIndex = 12;
		this.label5.Text = "Blue";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label6.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.label6.ForeColor = System.Drawing.Color.White;
		this.label6.Location = new System.Drawing.Point(9, 350);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(40, 14);
		this.label6.TabIndex = 11;
		this.label6.Text = "Green";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label7.BackColor = System.Drawing.Color.Red;
		this.label7.ForeColor = System.Drawing.Color.White;
		this.label7.Location = new System.Drawing.Point(9, 318);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(40, 14);
		this.label7.TabIndex = 10;
		this.label7.Text = "Red";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.buttonReset.Location = new System.Drawing.Point(124, 286);
		this.buttonReset.Name = "buttonReset";
		this.buttonReset.Size = new System.Drawing.Size(75, 23);
		this.buttonReset.TabIndex = 13;
		this.buttonReset.Text = "Reset";
		this.buttonReset.UseVisualStyleBackColor = true;
		this.buttonReset.Click += new System.EventHandler(buttonReset_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(280, 479);
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
		base.Name = "ModifyHairColor";
		this.Text = "Modify Hair Color";
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarRed).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarGreen).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarBlue).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
