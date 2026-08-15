using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class ViewCompetitionSetting : UserControl
{
	private CompetitionSettings m_CompetitionSettings;

	private string m_Description;

	private int m_Index;

	private bool m_IsSpecific;

	private IContainer components;

	private CheckBox check;

	public Label label;

	public TextBox textBox;

	[Category("User")]
	[Description("Settings")]
	public CompetitionSettings Settings
	{
		get
		{
			return m_CompetitionSettings;
		}
		set
		{
			m_CompetitionSettings = value;
			if (m_CompetitionSettings != null)
			{
				textBox.Text = m_CompetitionSettings.GetProperty(m_Description, m_Index, out m_IsSpecific);
				check.Checked = m_IsSpecific;
			}
			else
			{
				textBox.Text = string.Empty;
				check.Checked = false;
			}
		}
	}

	[Category("User")]
	[Description("Index")]
	public int Index
	{
		get
		{
			return m_Index;
		}
		set
		{
			m_Index = value;
		}
	}

	[Category("User")]
	[Description("Description")]
	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			m_Description = value;
			label.Text = m_Description;
		}
	}

	public ViewCompetitionSetting()
	{
		InitializeComponent();
	}

	private void check_CheckedChanged(object sender, EventArgs e)
	{
		if (check.Checked)
		{
			label.BackColor = Color.LightGreen;
			label.ForeColor = Color.Black;
			textBox.Enabled = true;
		}
		else
		{
			m_CompetitionSettings.UnsetProperty(m_Description);
			textBox.Text = m_CompetitionSettings.GetProperty(m_Description, m_Index, out m_IsSpecific);
			textBox.Enabled = false;
			label.BackColor = Color.Gray;
			label.BackColor = Color.DarkGray;
		}
	}

	private void textBox_TextChanged(object sender, EventArgs e)
	{
		_ = check.Checked;
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
		this.check = new System.Windows.Forms.CheckBox();
		this.label = new System.Windows.Forms.Label();
		this.textBox = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.check.AutoSize = true;
		this.check.Dock = System.Windows.Forms.DockStyle.Left;
		this.check.Location = new System.Drawing.Point(0, 0);
		this.check.Name = "check";
		this.check.Size = new System.Drawing.Size(15, 20);
		this.check.TabIndex = 0;
		this.check.UseVisualStyleBackColor = true;
		this.check.CheckedChanged += new System.EventHandler(check_CheckedChanged);
		this.label.BackColor = System.Drawing.Color.Transparent;
		this.label.Dock = System.Windows.Forms.DockStyle.Fill;
		this.label.Location = new System.Drawing.Point(15, 0);
		this.label.Name = "label";
		this.label.Size = new System.Drawing.Size(368, 20);
		this.label.TabIndex = 1;
		this.label.Text = "Description";
		this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textBox.Dock = System.Windows.Forms.DockStyle.Right;
		this.textBox.Location = new System.Drawing.Point(266, 0);
		this.textBox.Name = "textBox";
		this.textBox.Size = new System.Drawing.Size(117, 20);
		this.textBox.TabIndex = 2;
		this.textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.textBox.TextChanged += new System.EventHandler(textBox_TextChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.textBox);
		base.Controls.Add(this.label);
		base.Controls.Add(this.check);
		base.Name = "CompetitionSetting";
		base.Size = new System.Drawing.Size(383, 20);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
