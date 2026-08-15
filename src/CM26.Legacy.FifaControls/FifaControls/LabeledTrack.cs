using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class LabeledTrack : UserControl
{
	public delegate void ValueChangedHandler(object sender, int value);

	private string m_LabelText = string.Empty;

	public ValueChangedHandler ValueChanged;

	private IContainer components;

	private Label label;

	private TrackBar track;

	[Category("User")]
	[Description("Label")]
	public string LabelText
	{
		get
		{
			return m_LabelText;
		}
		set
		{
			m_LabelText = value;
			if (value.Contains(" "))
			{
				value.Replace(' ', '-');
			}
			label.Text = m_LabelText + " " + track.Value;
		}
	}

	[Category("User")]
	[Description("Value")]
	public int Value
	{
		get
		{
			return track.Value;
		}
		set
		{
			track.Value = value;
		}
	}

	public LabeledTrack()
	{
		InitializeComponent();
	}

	private void track_ValueChanged(object sender, EventArgs e)
	{
		label.Text = m_LabelText + " " + track.Value;
		if (ValueChanged != null)
		{
			ValueChanged(sender, track.Value);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.LabeledTrack));
		this.label = new System.Windows.Forms.Label();
		this.track = new System.Windows.Forms.TrackBar();
		((System.ComponentModel.ISupportInitialize)this.track).BeginInit();
		base.SuspendLayout();
		this.label.BackColor = System.Drawing.SystemColors.Control;
		this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.label.ForeColor = System.Drawing.Color.Yellow;
		this.label.Image = (System.Drawing.Image)resources.GetObject("label.Image");
		this.label.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label.Location = new System.Drawing.Point(1, -1);
		this.label.Name = "label";
		this.label.Size = new System.Drawing.Size(100, 16);
		this.label.TabIndex = 90;
		this.label.Text = "Name";
		this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.track.BackColor = System.Drawing.SystemColors.Control;
		this.track.Cursor = System.Windows.Forms.Cursors.Default;
		this.track.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.track.LargeChange = 10;
		this.track.Location = new System.Drawing.Point(-7, 6);
		this.track.Maximum = 99;
		this.track.Minimum = 1;
		this.track.Name = "track";
		this.track.Size = new System.Drawing.Size(116, 45);
		this.track.TabIndex = 89;
		this.track.TickFrequency = 10;
		this.track.Value = 99;
		this.track.ValueChanged += new System.EventHandler(track_ValueChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.label);
		base.Controls.Add(this.track);
		base.Name = "LabeledTrack";
		base.Size = new System.Drawing.Size(104, 45);
		((System.ComponentModel.ISupportInitialize)this.track).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
