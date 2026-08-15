using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class NumericStars : UserControl
{
	public delegate void StarsEventHandler(object sender, int value);

	public StarsEventHandler ValueChanged;

	private IContainer components;

	private Label label;

	private ImageList imageList;

	public NumericUpDown numericUpDown;

	[Category("User")]
	[Description("Value")]
	public int Value
	{
		get
		{
			return (int)numericUpDown.Value;
		}
		set
		{
			if ((decimal)value < numericUpDown.Minimum)
			{
				numericUpDown.Value = numericUpDown.Minimum;
			}
			else if ((decimal)value > numericUpDown.Maximum)
			{
				numericUpDown.Value = numericUpDown.Maximum;
			}
			else
			{
				numericUpDown.Value = value;
			}
		}
	}

	[Category("User")]
	[Description("Maximum")]
	public int Maximum
	{
		get
		{
			return (int)numericUpDown.Maximum;
		}
		set
		{
			if (numericUpDown.Value > numericUpDown.Maximum)
			{
				numericUpDown.Value = numericUpDown.Maximum;
			}
			numericUpDown.Maximum = value;
		}
	}

	public NumericStars()
	{
		InitializeComponent();
	}

	private void numericUpDown_ValueChanged(object sender, EventArgs e)
	{
		int num = (int)numericUpDown.Value;
		int num2 = (int)numericUpDown.Maximum;
		int num3 = 0;
		num3 = ((num2 != 20) ? ((num >= 1) ? (num - 1) : 0) : ((num >= 2) ? ((num - 2) / 2) : 0));
		label.ImageIndex = num3;
		if (ValueChanged != null)
		{
			ValueChanged(this, num);
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.NumericStars));
		this.numericUpDown = new System.Windows.Forms.NumericUpDown();
		this.label = new System.Windows.Forms.Label();
		this.imageList = new System.Windows.Forms.ImageList(this.components);
		((System.ComponentModel.ISupportInitialize)this.numericUpDown).BeginInit();
		base.SuspendLayout();
		this.numericUpDown.Location = new System.Drawing.Point(0, 0);
		this.numericUpDown.Maximum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericUpDown.Name = "numericUpDown";
		this.numericUpDown.Size = new System.Drawing.Size(66, 20);
		this.numericUpDown.TabIndex = 0;
		this.numericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown.ValueChanged += new System.EventHandler(numericUpDown_ValueChanged);
		this.label.ImageIndex = 0;
		this.label.ImageList = this.imageList;
		this.label.Location = new System.Drawing.Point(70, 2);
		this.label.Name = "label";
		this.label.Size = new System.Drawing.Size(93, 17);
		this.label.TabIndex = 1;
		this.imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
		this.imageList.TransparentColor = System.Drawing.Color.Fuchsia;
		this.imageList.Images.SetKeyName(0, "Stars_0_5.PNG");
		this.imageList.Images.SetKeyName(1, "Stars_1.PNG");
		this.imageList.Images.SetKeyName(2, "Stars_1_5.PNG");
		this.imageList.Images.SetKeyName(3, "Stars_2.PNG");
		this.imageList.Images.SetKeyName(4, "Stars_2_5.PNG");
		this.imageList.Images.SetKeyName(5, "Stars_3.PNG");
		this.imageList.Images.SetKeyName(6, "Stars_3_5.PNG");
		this.imageList.Images.SetKeyName(7, "Stars_4.PNG");
		this.imageList.Images.SetKeyName(8, "Stars_4_5.PNG");
		this.imageList.Images.SetKeyName(9, "Stars_5.PNG");
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Transparent;
		base.Controls.Add(this.label);
		base.Controls.Add(this.numericUpDown);
		base.Name = "NumericStars";
		base.Size = new System.Drawing.Size(167, 20);
		((System.ComponentModel.ISupportInitialize)this.numericUpDown).EndInit();
		base.ResumeLayout(false);
	}
}
