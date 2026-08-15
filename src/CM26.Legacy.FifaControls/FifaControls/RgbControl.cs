using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FifaControls;

public class RgbControl : Form
{
	private Bitmap m_InputBitmap;

	private Bitmap m_OutputBitmap;

	private Control m_Caller;

	private int m_MouseStatus;

	private IContainer components;

	private TrackBar trackBarRed;

	private Label label1;

	private Label label2;

	private TrackBar trackBarGreen;

	private Label label3;

	private TrackBar trackBarBlue;

	private Button buttonOk;

	private Button buttonCancel;

	private Button buttonReset;

	public RgbControl(Bitmap inputBitmap, Control caller)
	{
		if (inputBitmap != null)
		{
			m_OutputBitmap = inputBitmap;
			m_InputBitmap = (Bitmap)inputBitmap.Clone();
			m_Caller = caller;
			InitializeComponent();
		}
	}

	private void Colorize()
	{
		if (m_InputBitmap == null || m_InputBitmap.PixelFormat != PixelFormat.Format32bppArgb)
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		int num = m_InputBitmap.Width * m_InputBitmap.Height;
		int[] array = new int[num];
		Rectangle rect = new Rectangle(0, 0, m_InputBitmap.Width, m_InputBitmap.Height);
		BitmapData bitmapData = m_InputBitmap.LockBits(rect, ImageLockMode.WriteOnly, m_InputBitmap.PixelFormat);
		Marshal.Copy(bitmapData.Scan0, array, 0, num);
		m_InputBitmap.UnlockBits(bitmapData);
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
			array[i] = Color.FromArgb(a, r, g, b).ToArgb();
		}
		rect = new Rectangle(0, 0, m_OutputBitmap.Width, m_OutputBitmap.Height);
		BitmapData bitmapData2 = m_OutputBitmap.LockBits(rect, ImageLockMode.WriteOnly, m_OutputBitmap.PixelFormat);
		IntPtr scan = bitmapData2.Scan0;
		Marshal.Copy(array, 0, scan, num);
		m_OutputBitmap.UnlockBits(bitmapData2);
		Cursor.Current = Cursors.Default;
		FindForm().Refresh();
		m_Caller.Refresh();
	}

	private void buttonOk_Click(object sender, EventArgs e)
	{
		m_InputBitmap = m_OutputBitmap;
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		m_OutputBitmap = m_InputBitmap;
	}

	private void trackBarRed_Scroll(object sender, EventArgs e)
	{
	}

	private void trackBarRed_MouseDown(object sender, MouseEventArgs e)
	{
		m_MouseStatus = -1;
	}

	private void trackBarRed_MouseUp(object sender, MouseEventArgs e)
	{
		m_MouseStatus = 0;
		Colorize();
	}

	private void buttonReset_Click(object sender, EventArgs e)
	{
		trackBarRed.Value = 0;
		trackBarGreen.Value = 0;
		trackBarBlue.Value = 0;
		Colorize();
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
		this.trackBarRed = new System.Windows.Forms.TrackBar();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.trackBarGreen = new System.Windows.Forms.TrackBar();
		this.label3 = new System.Windows.Forms.Label();
		this.trackBarBlue = new System.Windows.Forms.TrackBar();
		this.buttonOk = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonReset = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.trackBarRed).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarGreen).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarBlue).BeginInit();
		base.SuspendLayout();
		this.trackBarRed.BackColor = System.Drawing.SystemColors.Control;
		this.trackBarRed.LargeChange = 16;
		this.trackBarRed.Location = new System.Drawing.Point(36, 12);
		this.trackBarRed.Maximum = 64;
		this.trackBarRed.Minimum = -64;
		this.trackBarRed.Name = "trackBarRed";
		this.trackBarRed.Size = new System.Drawing.Size(268, 45);
		this.trackBarRed.TabIndex = 0;
		this.trackBarRed.TickFrequency = 8;
		this.trackBarRed.Scroll += new System.EventHandler(trackBarRed_Scroll);
		this.trackBarRed.MouseDown += new System.Windows.Forms.MouseEventHandler(trackBarRed_MouseDown);
		this.trackBarRed.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBarRed_MouseUp);
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.Red;
		this.label1.ForeColor = System.Drawing.Color.White;
		this.label1.Location = new System.Drawing.Point(3, 21);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(27, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Red";
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.label2.ForeColor = System.Drawing.Color.White;
		this.label2.Location = new System.Drawing.Point(3, 57);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(36, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "Green";
		this.trackBarGreen.BackColor = System.Drawing.SystemColors.Control;
		this.trackBarGreen.LargeChange = 16;
		this.trackBarGreen.Location = new System.Drawing.Point(36, 48);
		this.trackBarGreen.Maximum = 64;
		this.trackBarGreen.Minimum = -64;
		this.trackBarGreen.Name = "trackBarGreen";
		this.trackBarGreen.Size = new System.Drawing.Size(268, 45);
		this.trackBarGreen.TabIndex = 2;
		this.trackBarGreen.TickFrequency = 8;
		this.trackBarGreen.Scroll += new System.EventHandler(trackBarRed_Scroll);
		this.trackBarGreen.MouseDown += new System.Windows.Forms.MouseEventHandler(trackBarRed_MouseDown);
		this.trackBarGreen.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBarRed_MouseUp);
		this.label3.AutoSize = true;
		this.label3.BackColor = System.Drawing.Color.FromArgb(0, 0, 192);
		this.label3.ForeColor = System.Drawing.Color.White;
		this.label3.Location = new System.Drawing.Point(3, 95);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(28, 13);
		this.label3.TabIndex = 5;
		this.label3.Text = "Blue";
		this.trackBarBlue.BackColor = System.Drawing.SystemColors.Control;
		this.trackBarBlue.LargeChange = 16;
		this.trackBarBlue.Location = new System.Drawing.Point(36, 86);
		this.trackBarBlue.Maximum = 64;
		this.trackBarBlue.Minimum = -64;
		this.trackBarBlue.Name = "trackBarBlue";
		this.trackBarBlue.Size = new System.Drawing.Size(268, 45);
		this.trackBarBlue.TabIndex = 4;
		this.trackBarBlue.TickFrequency = 8;
		this.trackBarBlue.Scroll += new System.EventHandler(trackBarRed_Scroll);
		this.trackBarBlue.MouseDown += new System.Windows.Forms.MouseEventHandler(trackBarRed_MouseDown);
		this.trackBarBlue.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBarRed_MouseUp);
		this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOk.Location = new System.Drawing.Point(58, 159);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 6;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonOk.Click += new System.EventHandler(buttonOk_Click);
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(203, 159);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 7;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.buttonReset.Location = new System.Drawing.Point(131, 130);
		this.buttonReset.Name = "buttonReset";
		this.buttonReset.Size = new System.Drawing.Size(75, 23);
		this.buttonReset.TabIndex = 8;
		this.buttonReset.Text = "Reset";
		this.buttonReset.UseVisualStyleBackColor = true;
		this.buttonReset.Click += new System.EventHandler(buttonReset_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(310, 194);
		base.Controls.Add(this.buttonReset);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOk);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.trackBarBlue);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.trackBarGreen);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.trackBarRed);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "RgbControl";
		this.Text = "RgbControl";
		((System.ComponentModel.ISupportInitialize)this.trackBarRed).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarGreen).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBarBlue).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
