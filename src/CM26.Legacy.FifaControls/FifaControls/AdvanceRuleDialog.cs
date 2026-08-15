using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class AdvanceRuleDialog : Form
{
	private Rank m_Rule;

	private Trophy m_Trophy;

	private Stage m_Stage;

	private Group m_Group;

	private IContainer components;

	private ComboBox comboTrophy;

	private Panel panel1;

	private Button buttonCancel;

	private Button buttonOk;

	private ComboBox comboStage;

	private ComboBox comboGroup;

	private ComboBox comboTeam;

	public Rank Rule
	{
		get
		{
			return m_Rule;
		}
		set
		{
			m_Rule = value;
			LoadToPanel();
		}
	}

	public AdvanceRuleDialog()
	{
		InitializeComponent();
	}

	public void Preset()
	{
		if (comboTrophy.Items.Count != FifaEnvironment.CompetitionObjects.Trophies.Count)
		{
			comboTrophy.Items.Clear();
			comboTrophy.Items.AddRange(FifaEnvironment.CompetitionObjects.Trophies.ToArray());
		}
	}

	private void LoadToPanel()
	{
		Preset();
		Group obj = null;
		if (m_Rule.MoveFrom != null)
		{
			obj = m_Rule.MoveFrom.Group;
			if (obj != null)
			{
				m_Trophy = obj.ParentTrophy;
			}
		}
		if (obj == null)
		{
			obj = m_Rule.Group;
		}
		if (m_Trophy == null)
		{
			m_Trophy = m_Rule.Group.ParentTrophy;
		}
		comboTrophy.SelectedItem = m_Trophy;
		comboStage.Items.Clear();
		foreach (Stage stage in m_Trophy.Stages)
		{
			comboStage.Items.Add(stage);
		}
		if (obj != null)
		{
			m_Stage = obj.ParentStage;
		}
		else
		{
			m_Stage = m_Rule.Group.ParentStage;
		}
		comboStage.SelectedItem = m_Stage;
		comboGroup.Items.Clear();
		foreach (Group group in m_Stage.Groups)
		{
			comboGroup.Items.Add(group);
		}
		m_Group = obj;
		comboGroup.SelectedItem = obj;
		comboTeam.Items.Clear();
		foreach (Rank rank in m_Group.Ranks)
		{
			comboTeam.Items.Add(rank);
		}
		comboTeam.SelectedItem = m_Rule.MoveFrom;
	}

	private void comboTrophy_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboTrophy.SelectedItem == m_Trophy)
		{
			return;
		}
		m_Trophy = (Trophy)comboTrophy.SelectedItem;
		comboStage.Items.Clear();
		foreach (Stage stage in m_Trophy.Stages)
		{
			comboStage.Items.Add(stage);
		}
		comboStage.SelectedIndex = 0;
	}

	private void comboStage_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboStage.SelectedItem == m_Stage)
		{
			return;
		}
		m_Stage = (Stage)comboStage.SelectedItem;
		comboGroup.Items.Clear();
		foreach (Group group in m_Stage.Groups)
		{
			comboGroup.Items.Add(group);
		}
		if (m_Stage.Groups.Count >= 1)
		{
			comboGroup.SelectedIndex = 0;
		}
	}

	private void comboGroup_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboGroup.SelectedItem == m_Group)
		{
			return;
		}
		m_Group = (Group)comboGroup.SelectedItem;
		comboTeam.Items.Clear();
		foreach (Rank rank in m_Group.Ranks)
		{
			comboTeam.Items.Add(rank);
		}
		comboTeam.SelectedIndex = 0;
	}

	private void comboTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_Rule.MoveFrom = (Rank)comboTeam.SelectedItem;
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
		this.comboTrophy = new System.Windows.Forms.ComboBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOk = new System.Windows.Forms.Button();
		this.comboStage = new System.Windows.Forms.ComboBox();
		this.comboGroup = new System.Windows.Forms.ComboBox();
		this.comboTeam = new System.Windows.Forms.ComboBox();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.comboTrophy.FormattingEnabled = true;
		this.comboTrophy.Location = new System.Drawing.Point(12, 12);
		this.comboTrophy.Name = "comboTrophy";
		this.comboTrophy.Size = new System.Drawing.Size(351, 21);
		this.comboTrophy.TabIndex = 15;
		this.comboTrophy.SelectedIndexChanged += new System.EventHandler(comboTrophy_SelectedIndexChanged);
		this.panel1.Controls.Add(this.buttonCancel);
		this.panel1.Controls.Add(this.buttonOk);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panel1.Location = new System.Drawing.Point(0, 134);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(375, 50);
		this.panel1.TabIndex = 17;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(236, 15);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 3;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOk.Location = new System.Drawing.Point(54, 15);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 2;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.comboStage.FormattingEnabled = true;
		this.comboStage.Location = new System.Drawing.Point(12, 39);
		this.comboStage.Name = "comboStage";
		this.comboStage.Size = new System.Drawing.Size(351, 21);
		this.comboStage.TabIndex = 18;
		this.comboStage.SelectedIndexChanged += new System.EventHandler(comboStage_SelectedIndexChanged);
		this.comboGroup.FormattingEnabled = true;
		this.comboGroup.Location = new System.Drawing.Point(12, 66);
		this.comboGroup.Name = "comboGroup";
		this.comboGroup.Size = new System.Drawing.Size(351, 21);
		this.comboGroup.TabIndex = 19;
		this.comboGroup.SelectedIndexChanged += new System.EventHandler(comboGroup_SelectedIndexChanged);
		this.comboTeam.FormattingEnabled = true;
		this.comboTeam.Location = new System.Drawing.Point(12, 93);
		this.comboTeam.Name = "comboTeam";
		this.comboTeam.Size = new System.Drawing.Size(351, 21);
		this.comboTeam.TabIndex = 20;
		this.comboTeam.SelectedIndexChanged += new System.EventHandler(comboTeam_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(375, 184);
		base.Controls.Add(this.comboTeam);
		base.Controls.Add(this.comboGroup);
		base.Controls.Add(this.comboStage);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.comboTrophy);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "AdvanceRuleDialog";
		this.Text = "Advance Rule";
		this.panel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
