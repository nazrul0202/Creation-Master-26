using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class ColorSelector : Form
{
	private Button buttonOK;

	private Button buttonCancel;

	private PictureBox pictureSelectedColor;

	private IContainer components;

	private Color m_SelectedColor;

	private Color[] m_Palette;

	private int m_SelectedIndex;

	private PictureBox pictureBoxHidden;

	private RadioButton[] m_RadioButtons;

	public Color SelectedColor => m_SelectedColor;

	public int SelectedIndex => m_SelectedIndex;

	public ColorSelector(Color[] palette, int selectedIndex)
	{
		InitializeComponent();
		m_Palette = palette;
		if (selectedIndex < 0 || selectedIndex >= palette.Length)
		{
			selectedIndex = 0;
		}
		m_SelectedIndex = selectedIndex;
		m_SelectedColor = m_Palette[selectedIndex];
		pictureSelectedColor.BackColor = m_SelectedColor;
		base.Height = 20 * ((palette.Length - 1) / 8) + 110;
		buttonOK.Location = new Point(16, base.Height - 72);
		buttonCancel.Location = new Point(104, base.Height - 72);
		int num = 32;
		int num2 = 8;
		m_RadioButtons = new RadioButton[palette.Length];
		for (int i = 0; i < palette.Length; i++)
		{
			m_RadioButtons[i] = new RadioButton();
			m_RadioButtons[i].Location = new Point(num, num2);
			num += 20;
			if ((i + 1) % 8 == 0)
			{
				num = 32;
				num2 += 20;
			}
			m_RadioButtons[i].BackColor = palette[i];
			if (palette[i] == Color.Transparent)
			{
				m_RadioButtons[i].BackgroundImage = pictureBoxHidden.BackgroundImage;
			}
			if (i == selectedIndex)
			{
				m_RadioButtons[i].Checked = true;
			}
			m_RadioButtons[i].Appearance = Appearance.Button;
			m_RadioButtons[i].Text = string.Empty;
			RadioButton obj = m_RadioButtons[i];
			int num3 = (m_RadioButtons[i].Height = 18);
			obj.Width = num3;
			m_RadioButtons[i].Visible = true;
			m_RadioButtons[i].CheckedChanged += radio_CheckedChanged;
			base.Controls.Add(m_RadioButtons[i]);
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
		System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.ColorSelector));
		this.buttonOK = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.pictureSelectedColor = new System.Windows.Forms.PictureBox();
		this.pictureBoxHidden = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.pictureSelectedColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBoxHidden).BeginInit();
		base.SuspendLayout();
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		componentResourceManager.ApplyResources(this.buttonOK, "buttonOK");
		this.buttonOK.Name = "buttonOK";
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.ForeColor = System.Drawing.SystemColors.ControlText;
		componentResourceManager.ApplyResources(this.buttonCancel, "buttonCancel");
		this.buttonCancel.Name = "buttonCancel";
		this.pictureSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		componentResourceManager.ApplyResources(this.pictureSelectedColor, "pictureSelectedColor");
		this.pictureSelectedColor.Name = "pictureSelectedColor";
		this.pictureSelectedColor.TabStop = false;
		componentResourceManager.ApplyResources(this.pictureBoxHidden, "pictureBoxHidden");
		this.pictureBoxHidden.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureBoxHidden.Name = "pictureBoxHidden";
		this.pictureBoxHidden.TabStop = false;
		base.AcceptButton = this.buttonOK;
		componentResourceManager.ApplyResources(this, "$this");
		base.CancelButton = this.buttonCancel;
		base.Controls.Add(this.pictureBoxHidden);
		base.Controls.Add(this.pictureSelectedColor);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOK);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "ColorSelector";
		((System.ComponentModel.ISupportInitialize)this.pictureSelectedColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBoxHidden).EndInit();
		base.ResumeLayout(false);
	}

	private void radio_CheckedChanged(object sender, EventArgs e)
	{
		RadioButton radioButton = (RadioButton)sender;
		if (!radioButton.Checked)
		{
			return;
		}
		pictureSelectedColor.BackColor = radioButton.BackColor;
		m_SelectedColor = radioButton.BackColor;
		if (m_SelectedColor == Color.Transparent)
		{
			pictureSelectedColor.BackgroundImage = pictureBoxHidden.BackgroundImage;
		}
		else
		{
			pictureSelectedColor.BackgroundImage = null;
		}
		for (int i = 0; i < m_Palette.Length; i++)
		{
			if (pictureSelectedColor.BackColor == m_RadioButtons[i].BackColor)
			{
				m_SelectedIndex = i;
				break;
			}
		}
	}
}
