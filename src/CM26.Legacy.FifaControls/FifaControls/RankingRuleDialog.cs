using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class RankingRuleDialog : Form
{
	private Rank m_Rank;

	private Trophy m_Trophy;

	private Stage m_Stage;

	private Group m_Group;

	private IContainer components;

	private Panel panel1;

	private Button buttonCancel;

	private Button buttonOk;

	private ComboBox comboTeam;

	private ComboBox comboGroup;

	private ComboBox comboStage;

	private ComboBox comboTrophy;

	public Rank Rank
	{
		get
		{
			return m_Rank;
		}
		set
		{
			m_Rank = value;
			LoadToPanel();
		}
	}

	public RankingRuleDialog()
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
		obj = m_Rank.Group;
		if (obj == null)
		{
			return;
		}
		m_Trophy = obj.ParentTrophy;
		if (m_Trophy == null)
		{
			return;
		}
		comboTrophy.SelectedItem = m_Trophy;
		comboStage.Items.Clear();
		foreach (Stage stage in m_Trophy.Stages)
		{
			comboStage.Items.Add(stage);
		}
		m_Stage = m_Rank.Group.ParentStage;
		comboStage.SelectedItem = m_Stage;
		comboGroup.Items.Clear();
		foreach (Group group in m_Stage.Groups)
		{
			comboGroup.Items.Add(group);
		}
		m_Group = obj;
		comboGroup.SelectedItem = m_Group;
		comboTeam.Items.Clear();
		for (int i = 1; i < m_Group.Ranks.Count; i++)
		{
			comboTeam.Items.Add(m_Group.Ranks[i]);
		}
		comboTeam.SelectedItem = m_Rank;
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
		comboGroup.SelectedIndex = 0;
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
		m_Rank = (Rank)comboTeam.SelectedItem;
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
		this.panel1 = new System.Windows.Forms.Panel();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOk = new System.Windows.Forms.Button();
		this.comboTeam = new System.Windows.Forms.ComboBox();
		this.comboGroup = new System.Windows.Forms.ComboBox();
		this.comboStage = new System.Windows.Forms.ComboBox();
		this.comboTrophy = new System.Windows.Forms.ComboBox();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.panel1.Controls.Add(this.buttonCancel);
		this.panel1.Controls.Add(this.buttonOk);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panel1.Location = new System.Drawing.Point(0, 130);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(380, 50);
		this.panel1.TabIndex = 18;
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
		this.comboTeam.FormattingEnabled = true;
		this.comboTeam.Location = new System.Drawing.Point(12, 93);
		this.comboTeam.Name = "comboTeam";
		this.comboTeam.Size = new System.Drawing.Size(351, 21);
		this.comboTeam.TabIndex = 24;
		this.comboTeam.SelectedIndexChanged += new System.EventHandler(comboTeam_SelectedIndexChanged);
		this.comboGroup.FormattingEnabled = true;
		this.comboGroup.Location = new System.Drawing.Point(12, 66);
		this.comboGroup.Name = "comboGroup";
		this.comboGroup.Size = new System.Drawing.Size(351, 21);
		this.comboGroup.TabIndex = 23;
		this.comboGroup.SelectedIndexChanged += new System.EventHandler(comboGroup_SelectedIndexChanged);
		this.comboStage.FormattingEnabled = true;
		this.comboStage.Location = new System.Drawing.Point(12, 39);
		this.comboStage.Name = "comboStage";
		this.comboStage.Size = new System.Drawing.Size(351, 21);
		this.comboStage.TabIndex = 22;
		this.comboStage.SelectedIndexChanged += new System.EventHandler(comboStage_SelectedIndexChanged);
		this.comboTrophy.FormattingEnabled = true;
		this.comboTrophy.Location = new System.Drawing.Point(12, 12);
		this.comboTrophy.Name = "comboTrophy";
		this.comboTrophy.Size = new System.Drawing.Size(351, 21);
		this.comboTrophy.TabIndex = 21;
		this.comboTrophy.SelectedIndexChanged += new System.EventHandler(comboTrophy_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(380, 180);
		base.Controls.Add(this.comboTeam);
		base.Controls.Add(this.comboGroup);
		base.Controls.Add(this.comboStage);
		base.Controls.Add(this.comboTrophy);
		base.Controls.Add(this.panel1);
		base.Name = "RankingRuleDialog";
		this.Text = "Ranking Rule";
		this.panel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
