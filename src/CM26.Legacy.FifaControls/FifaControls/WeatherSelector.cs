using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class WeatherSelector : UserControl
{
	public delegate void WeatherEventHandler(object sender, int value, int month);

	private int month = -1;

	public WeatherEventHandler ValueChanged;

	private IContainer components;

	private Label labelPicture;

	private ImageList imageListWeather;

	public Label labelText;

	[Category("User")]
	[Description("Header")]
	public string Header
	{
		get
		{
			return labelText.Text;
		}
		set
		{
			labelText.Text = value;
			if (labelText.Text == "Jan")
			{
				month = 0;
			}
			else if (labelText.Text == "Feb")
			{
				month = 1;
			}
			else if (labelText.Text == "Mar")
			{
				month = 2;
			}
			else if (labelText.Text == "Apr")
			{
				month = 3;
			}
			else if (labelText.Text == "May")
			{
				month = 4;
			}
			else if (labelText.Text == "Jun")
			{
				month = 5;
			}
			else if (labelText.Text == "Jul")
			{
				month = 6;
			}
			else if (labelText.Text == "Aug")
			{
				month = 7;
			}
			else if (labelText.Text == "Sep")
			{
				month = 8;
			}
			else if (labelText.Text == "Oct")
			{
				month = 9;
			}
			else if (labelText.Text == "Nov")
			{
				month = 10;
			}
			else if (labelText.Text == "Dec")
			{
				month = 11;
			}
		}
	}

	public int Value
	{
		get
		{
			return labelPicture.ImageIndex;
		}
		set
		{
			if (value < 0)
			{
				labelPicture.ImageIndex = 0;
			}
			else if (value > 4)
			{
				labelPicture.ImageIndex = 4;
			}
			else
			{
				labelPicture.ImageIndex = value;
			}
		}
	}

	public WeatherSelector()
	{
		InitializeComponent();
	}

	private void label1_Click(object sender, EventArgs e)
	{
		if (((MouseEventArgs)e).Button == MouseButtons.Left)
		{
			if (labelPicture.ImageIndex < 4)
			{
				labelPicture.ImageIndex++;
			}
			else
			{
				labelPicture.ImageIndex = 0;
			}
		}
		else if (labelPicture.ImageIndex > 0)
		{
			labelPicture.ImageIndex--;
		}
		else
		{
			labelPicture.ImageIndex = 4;
		}
		if (ValueChanged != null)
		{
			ValueChanged(this, Value, month);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.WeatherSelector));
		this.labelPicture = new System.Windows.Forms.Label();
		this.imageListWeather = new System.Windows.Forms.ImageList(this.components);
		this.labelText = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.labelPicture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelPicture.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.labelPicture.ImageIndex = 0;
		this.labelPicture.ImageList = this.imageListWeather;
		this.labelPicture.Location = new System.Drawing.Point(0, 15);
		this.labelPicture.Name = "labelPicture";
		this.labelPicture.Size = new System.Drawing.Size(40, 30);
		this.labelPicture.TabIndex = 2;
		this.labelPicture.Click += new System.EventHandler(label1_Click);
		this.imageListWeather.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListWeather.ImageStream");
		this.imageListWeather.TransparentColor = System.Drawing.Color.Transparent;
		this.imageListWeather.Images.SetKeyName(0, "Weather_0.PNG");
		this.imageListWeather.Images.SetKeyName(1, "Weather_1.PNG");
		this.imageListWeather.Images.SetKeyName(2, "Weather_2.PNG");
		this.imageListWeather.Images.SetKeyName(3, "Weather_3.PNG");
		this.imageListWeather.Images.SetKeyName(4, "Weather_4.PNG");
		this.labelText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelText.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelText.Location = new System.Drawing.Point(0, 0);
		this.labelText.Name = "labelText";
		this.labelText.Size = new System.Drawing.Size(40, 15);
		this.labelText.TabIndex = 3;
		this.labelText.Text = "TEX";
		this.labelText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Transparent;
		base.Controls.Add(this.labelPicture);
		base.Controls.Add(this.labelText);
		base.Name = "WeatherSelector";
		base.Size = new System.Drawing.Size(40, 45);
		base.ResumeLayout(false);
	}
}
