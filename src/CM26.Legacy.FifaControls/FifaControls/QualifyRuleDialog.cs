using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class QualifyRuleDialog : Form
{
	private Task m_QualifyRule;

	private EQualifyingRule m_Rule;

	private Trophy m_Trophy1;

	private Trophy m_Trophy2;

	private League m_League;

	private Team m_Team;

	private int m_Number;

	private uint m_CountryLimitation;

	private IContainer components;

	private Button buttonOk;

	private Button buttonCancel;

	private RadioButton radioRule1;

	private RadioButton radioRule2;

	private RadioButton radioRule3;

	private RadioButton radioRule4;

	private RadioButton radioRule5;

	private RadioButton radioRule6;

	private RadioButton radioRule7;

	private ComboBox comboTrophy1;

	private ComboBox comboTrophy2;

	private ComboBox comboLeague;

	private NumericUpDown numericN;

	private ComboBox comboTeam;

	private NumericUpDown numericCountryLimitation;

	private RadioButton radioRule8;

	public Task QualifyRule
	{
		get
		{
			return m_QualifyRule;
		}
		set
		{
			m_QualifyRule = value;
			LoadToPanel();
		}
	}

	private void LoadToPanel()
	{
		Preset();
		m_Rule = m_QualifyRule.Rule;
		switch (m_Rule)
		{
		case EQualifyingRule.FillFromCompTable:
			m_Trophy1 = m_QualifyRule.Trophy1;
			comboTrophy1.SelectedItem = m_Trophy1;
			m_Number = m_QualifyRule.Parameter2;
			numericN.Value = m_Number;
			radioRule1.Checked = true;
			break;
		case EQualifyingRule.FillFromCompTableBackup:
			m_Trophy1 = m_QualifyRule.Trophy1;
			m_Trophy2 = m_QualifyRule.Trophy2;
			comboTrophy1.SelectedItem = m_Trophy1;
			comboTrophy2.SelectedItem = m_Trophy2;
			radioRule2.Checked = true;
			break;
		case EQualifyingRule.FillFromCompTableBackupLeague:
			m_Trophy1 = m_QualifyRule.Trophy1;
			m_League = m_QualifyRule.League;
			comboTrophy1.SelectedItem = m_Trophy1;
			comboLeague.SelectedItem = m_League;
			radioRule3.Checked = true;
			break;
		case EQualifyingRule.FillFromLeague:
			m_League = m_QualifyRule.League;
			comboLeague.SelectedItem = m_League;
			radioRule4.Checked = true;
			break;
		case EQualifyingRule.FillFromLeagueInOrder:
			m_League = m_QualifyRule.League;
			comboLeague.SelectedItem = m_League;
			radioRule8.Checked = true;
			break;
		case EQualifyingRule.FillFromLeagueMaxFromCountry:
			m_League = m_QualifyRule.League;
			comboLeague.SelectedItem = m_League;
			m_Number = m_QualifyRule.Parameter2;
			m_CountryLimitation = (uint)m_QualifyRule.Parameter3;
			numericN.Value = m_Number;
			numericCountryLimitation.Value = m_CountryLimitation;
			radioRule5.Checked = true;
			break;
		case EQualifyingRule.FillFromSpecialTeams:
			m_Number = m_QualifyRule.Parameter1;
			numericN.Value = m_Number;
			radioRule8.Checked = true;
			break;
		case EQualifyingRule.FillWithTeam:
			m_Team = m_QualifyRule.Team;
			comboTeam.SelectedItem = m_Team;
			m_Number = m_QualifyRule.Parameter1;
			numericN.Value = m_Number;
			radioRule7.Checked = true;
			break;
		}
		EnableRule();
	}

	public void Preset()
	{
		if (comboTrophy1.Items.Count != FifaEnvironment.CompetitionObjects.Trophies.Count)
		{
			comboTrophy1.Items.Clear();
			comboTrophy1.Items.AddRange(FifaEnvironment.CompetitionObjects.Trophies.ToArray());
		}
		if (comboTrophy2.Items.Count != FifaEnvironment.CompetitionObjects.Trophies.Count)
		{
			comboTrophy2.Items.Clear();
			comboTrophy2.Items.AddRange(FifaEnvironment.CompetitionObjects.Trophies.ToArray());
		}
		if (comboLeague.Items.Count != FifaEnvironment.Leagues.Count)
		{
			comboLeague.Items.Clear();
			comboLeague.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		}
		if (comboTeam.Items.Count != FifaEnvironment.Teams.Count)
		{
			comboTeam.Items.Clear();
			comboTeam.Items.AddRange(FifaEnvironment.Teams.ToArray());
		}
	}

	public QualifyRuleDialog()
	{
		InitializeComponent();
	}

	private void radioRule1_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillFromCompTable;
		EnableRule();
	}

	private void radioRule2_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillFromCompTableBackup;
		EnableRule();
	}

	private void radioRule3_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillFromCompTableBackupLeague;
		EnableRule();
	}

	private void radioRule4_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillFromLeague;
		EnableRule();
	}

	private void radioRule5_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillFromLeagueMaxFromCountry;
		EnableRule();
	}

	private void radioRule6_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillFromSpecialTeams;
		EnableRule();
	}

	private void radioRule7_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillWithTeam;
		EnableRule();
	}

	private void radioRule8_CheckedChanged(object sender, EventArgs e)
	{
		m_Rule = EQualifyingRule.FillFromLeagueInOrder;
		EnableRule();
	}

	private void EnableRule()
	{
		switch (m_Rule)
		{
		case EQualifyingRule.FillFromCompTable:
			comboTrophy1.SelectedItem = m_Trophy1;
			numericN.Value = m_Number;
			comboTrophy1.Visible = true;
			comboTrophy2.Visible = false;
			comboLeague.Visible = false;
			comboTeam.Visible = false;
			numericN.Visible = true;
			numericCountryLimitation.Visible = false;
			m_Trophy1 = (Trophy)comboTrophy1.SelectedItem;
			break;
		case EQualifyingRule.FillFromCompTableBackup:
			comboTrophy1.SelectedItem = m_Trophy1;
			comboTrophy2.SelectedItem = m_Trophy2;
			comboTrophy1.Visible = true;
			comboTrophy2.Visible = true;
			comboLeague.Visible = false;
			comboTeam.Visible = false;
			numericN.Visible = false;
			numericCountryLimitation.Visible = false;
			break;
		case EQualifyingRule.FillFromCompTableBackupLeague:
			comboTrophy1.SelectedItem = m_Trophy1;
			comboLeague.SelectedItem = m_League;
			comboTrophy1.Visible = true;
			comboTrophy2.Visible = false;
			comboLeague.Visible = true;
			comboTeam.Visible = false;
			numericN.Visible = false;
			numericCountryLimitation.Visible = false;
			break;
		case EQualifyingRule.FillFromLeague:
			comboLeague.SelectedItem = m_League;
			comboTrophy1.Visible = false;
			comboTrophy2.Visible = false;
			comboLeague.Visible = true;
			comboTeam.Visible = false;
			numericN.Visible = false;
			numericCountryLimitation.Visible = false;
			break;
		case EQualifyingRule.FillFromLeagueInOrder:
			comboLeague.SelectedItem = m_League;
			comboTrophy1.Visible = false;
			comboTrophy2.Visible = false;
			comboLeague.Visible = true;
			comboTeam.Visible = false;
			numericN.Visible = false;
			numericCountryLimitation.Visible = false;
			break;
		case EQualifyingRule.FillFromLeagueMaxFromCountry:
			comboLeague.SelectedItem = m_League;
			numericCountryLimitation.Value = m_CountryLimitation;
			numericN.Value = m_Number;
			comboTrophy1.Visible = false;
			comboTrophy2.Visible = false;
			comboLeague.Visible = true;
			comboTeam.Visible = false;
			numericN.Visible = true;
			numericCountryLimitation.Visible = true;
			break;
		case EQualifyingRule.FillFromSpecialTeams:
			numericN.Value = m_Number;
			comboTrophy1.Visible = false;
			comboTrophy2.Visible = false;
			comboLeague.Visible = false;
			comboTeam.Visible = false;
			numericN.Visible = true;
			numericCountryLimitation.Visible = false;
			break;
		case EQualifyingRule.FillWithTeam:
			comboTeam.SelectedItem = m_Team;
			numericN.Value = m_Number;
			comboTrophy1.Visible = false;
			comboTrophy2.Visible = false;
			comboLeague.Visible = false;
			comboTeam.Visible = true;
			numericN.Visible = true;
			numericCountryLimitation.Visible = false;
			break;
		}
	}

	private void comboTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboTeam.SelectedItem != null)
		{
			m_Team = (Team)comboTeam.SelectedItem;
		}
	}

	private void numericN_ValueChanged(object sender, EventArgs e)
	{
		m_Number = (int)numericN.Value;
	}

	private void comboTrophy1_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboTrophy1.SelectedItem != null)
		{
			m_Trophy1 = (Trophy)comboTrophy1.SelectedItem;
		}
	}

	private void comboLeague_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague.SelectedItem != null)
		{
			m_League = (League)comboLeague.SelectedItem;
		}
	}

	private void comboTrophy2_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboTrophy2.SelectedItem != null)
		{
			m_Trophy2 = (Trophy)comboTrophy2.SelectedItem;
		}
	}

	private void buttonOk_Click(object sender, EventArgs e)
	{
		m_QualifyRule.Rule = m_Rule;
		switch (m_Rule)
		{
		case EQualifyingRule.FillFromCompTable:
			if (m_Trophy1 != null)
			{
				m_QualifyRule.Parameter1 = m_Trophy1.Id;
				m_QualifyRule.Trophy1 = m_Trophy1;
				m_QualifyRule.Parameter2 = m_Number;
				m_QualifyRule.Parameter3 = 0;
			}
			break;
		case EQualifyingRule.FillFromCompTableBackup:
			if (m_Trophy2 != null && m_Trophy1 != null)
			{
				m_QualifyRule.Parameter1 = m_Trophy1.Id;
				m_QualifyRule.Trophy1 = m_Trophy1;
				m_QualifyRule.Parameter2 = m_Trophy2.Id;
				m_QualifyRule.Trophy2 = m_Trophy2;
				m_QualifyRule.Parameter3 = 1;
			}
			break;
		case EQualifyingRule.FillFromCompTableBackupLeague:
			if (m_League != null && m_Trophy1 != null)
			{
				m_QualifyRule.Parameter1 = m_Trophy1.Id;
				m_QualifyRule.Trophy1 = m_Trophy1;
				m_QualifyRule.Parameter2 = m_League.Id;
				m_QualifyRule.League = m_League;
				m_QualifyRule.Parameter3 = 1;
			}
			break;
		case EQualifyingRule.FillFromLeague:
			if (m_League != null)
			{
				m_QualifyRule.Parameter1 = m_League.Id;
				m_QualifyRule.League = m_League;
				m_QualifyRule.Parameter2 = 0;
				m_QualifyRule.Parameter3 = 0;
			}
			break;
		case EQualifyingRule.FillFromLeagueMaxFromCountry:
			if (m_League != null)
			{
				m_QualifyRule.Parameter1 = m_League.Id;
				m_QualifyRule.League = m_League;
				m_QualifyRule.Parameter2 = m_Number;
				m_QualifyRule.Parameter3 = (int)m_CountryLimitation;
			}
			break;
		case EQualifyingRule.FillFromSpecialTeams:
			m_QualifyRule.Parameter1 = m_Number;
			m_QualifyRule.Parameter2 = 0;
			m_QualifyRule.Parameter3 = 0;
			break;
		case EQualifyingRule.FillWithTeam:
			if (m_Team != null)
			{
				m_QualifyRule.Parameter1 = m_Number;
				m_QualifyRule.Parameter2 = m_Team.Id;
				m_QualifyRule.Team = m_Team;
				m_QualifyRule.Parameter3 = 0;
			}
			break;
		case EQualifyingRule.FillFromLeagueInOrder:
			break;
		}
	}

	private void numericCountryLimitation_ValueChanged(object sender, EventArgs e)
	{
		m_CountryLimitation = (uint)numericCountryLimitation.Value;
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
		this.buttonOk = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.radioRule1 = new System.Windows.Forms.RadioButton();
		this.radioRule2 = new System.Windows.Forms.RadioButton();
		this.radioRule3 = new System.Windows.Forms.RadioButton();
		this.radioRule4 = new System.Windows.Forms.RadioButton();
		this.radioRule5 = new System.Windows.Forms.RadioButton();
		this.radioRule6 = new System.Windows.Forms.RadioButton();
		this.radioRule7 = new System.Windows.Forms.RadioButton();
		this.comboTrophy1 = new System.Windows.Forms.ComboBox();
		this.comboTrophy2 = new System.Windows.Forms.ComboBox();
		this.comboLeague = new System.Windows.Forms.ComboBox();
		this.numericN = new System.Windows.Forms.NumericUpDown();
		this.comboTeam = new System.Windows.Forms.ComboBox();
		this.numericCountryLimitation = new System.Windows.Forms.NumericUpDown();
		this.radioRule8 = new System.Windows.Forms.RadioButton();
		((System.ComponentModel.ISupportInitialize)this.numericN).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericCountryLimitation).BeginInit();
		base.SuspendLayout();
		this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOk.Location = new System.Drawing.Point(89, 258);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 0;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonOk.Click += new System.EventHandler(buttonOk_Click);
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(327, 258);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 1;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.radioRule1.AutoSize = true;
		this.radioRule1.Location = new System.Drawing.Point(12, 12);
		this.radioRule1.Name = "radioRule1";
		this.radioRule1.Size = new System.Drawing.Size(193, 17);
		this.radioRule1.TabIndex = 2;
		this.radioRule1.TabStop = true;
		this.radioRule1.Text = "Get the Best N Team(s) of a Trophy";
		this.radioRule1.UseVisualStyleBackColor = true;
		this.radioRule1.CheckedChanged += new System.EventHandler(radioRule1_CheckedChanged);
		this.radioRule2.AutoSize = true;
		this.radioRule2.Location = new System.Drawing.Point(12, 35);
		this.radioRule2.Name = "radioRule2";
		this.radioRule2.Size = new System.Drawing.Size(303, 17);
		this.radioRule2.TabIndex = 5;
		this.radioRule2.TabStop = true;
		this.radioRule2.Text = "Get the Winner of a Trophy or a Team from another Trophy";
		this.radioRule2.UseVisualStyleBackColor = true;
		this.radioRule2.CheckedChanged += new System.EventHandler(radioRule2_CheckedChanged);
		this.radioRule3.AutoSize = true;
		this.radioRule3.Location = new System.Drawing.Point(12, 58);
		this.radioRule3.Name = "radioRule3";
		this.radioRule3.Size = new System.Drawing.Size(276, 17);
		this.radioRule3.TabIndex = 7;
		this.radioRule3.TabStop = true;
		this.radioRule3.Text = "Get the Winner of a Trophy or a Team from a League";
		this.radioRule3.UseVisualStyleBackColor = true;
		this.radioRule3.CheckedChanged += new System.EventHandler(radioRule3_CheckedChanged);
		this.radioRule4.AutoSize = true;
		this.radioRule4.Location = new System.Drawing.Point(12, 81);
		this.radioRule4.Name = "radioRule4";
		this.radioRule4.Size = new System.Drawing.Size(166, 17);
		this.radioRule4.TabIndex = 9;
		this.radioRule4.TabStop = true;
		this.radioRule4.Text = "Get the Teams from a League";
		this.radioRule4.UseVisualStyleBackColor = true;
		this.radioRule4.CheckedChanged += new System.EventHandler(radioRule4_CheckedChanged);
		this.radioRule5.AutoSize = true;
		this.radioRule5.Location = new System.Drawing.Point(12, 130);
		this.radioRule5.Name = "radioRule5";
		this.radioRule5.Size = new System.Drawing.Size(270, 17);
		this.radioRule5.TabIndex = 9;
		this.radioRule5.TabStop = true;
		this.radioRule5.Text = "Get Team(s) from a League with Country limitation to";
		this.radioRule5.UseVisualStyleBackColor = true;
		this.radioRule5.CheckedChanged += new System.EventHandler(radioRule5_CheckedChanged);
		this.radioRule6.AutoSize = true;
		this.radioRule6.Location = new System.Drawing.Point(12, 153);
		this.radioRule6.Name = "radioRule6";
		this.radioRule6.Size = new System.Drawing.Size(226, 17);
		this.radioRule6.TabIndex = 11;
		this.radioRule6.TabStop = true;
		this.radioRule6.Text = "Get N Teams from \"Special Teams Group\"";
		this.radioRule6.UseVisualStyleBackColor = true;
		this.radioRule6.CheckedChanged += new System.EventHandler(radioRule6_CheckedChanged);
		this.radioRule7.AutoSize = true;
		this.radioRule7.Location = new System.Drawing.Point(12, 176);
		this.radioRule7.Name = "radioRule7";
		this.radioRule7.Size = new System.Drawing.Size(209, 17);
		this.radioRule7.TabIndex = 13;
		this.radioRule7.TabStop = true;
		this.radioRule7.Text = "Get a specific Team in a given Position";
		this.radioRule7.UseVisualStyleBackColor = true;
		this.radioRule7.CheckedChanged += new System.EventHandler(radioRule7_CheckedChanged);
		this.comboTrophy1.FormattingEnabled = true;
		this.comboTrophy1.Location = new System.Drawing.Point(12, 215);
		this.comboTrophy1.Name = "comboTrophy1";
		this.comboTrophy1.Size = new System.Drawing.Size(205, 21);
		this.comboTrophy1.TabIndex = 14;
		this.comboTrophy1.SelectedIndexChanged += new System.EventHandler(comboTrophy1_SelectedIndexChanged);
		this.comboTrophy2.FormattingEnabled = true;
		this.comboTrophy2.Location = new System.Drawing.Point(294, 214);
		this.comboTrophy2.Name = "comboTrophy2";
		this.comboTrophy2.Size = new System.Drawing.Size(205, 21);
		this.comboTrophy2.TabIndex = 15;
		this.comboTrophy2.SelectedIndexChanged += new System.EventHandler(comboTrophy2_SelectedIndexChanged);
		this.comboLeague.FormattingEnabled = true;
		this.comboLeague.Location = new System.Drawing.Point(294, 214);
		this.comboLeague.Name = "comboLeague";
		this.comboLeague.Size = new System.Drawing.Size(205, 21);
		this.comboLeague.TabIndex = 16;
		this.comboLeague.SelectedIndexChanged += new System.EventHandler(comboLeague_SelectedIndexChanged);
		this.numericN.Location = new System.Drawing.Point(222, 215);
		this.numericN.Name = "numericN";
		this.numericN.Size = new System.Drawing.Size(66, 20);
		this.numericN.TabIndex = 17;
		this.numericN.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericN.ValueChanged += new System.EventHandler(numericN_ValueChanged);
		this.comboTeam.FormattingEnabled = true;
		this.comboTeam.Location = new System.Drawing.Point(12, 215);
		this.comboTeam.Name = "comboTeam";
		this.comboTeam.Size = new System.Drawing.Size(204, 21);
		this.comboTeam.TabIndex = 18;
		this.comboTeam.SelectedIndexChanged += new System.EventHandler(comboTeam_SelectedIndexChanged);
		this.numericCountryLimitation.Location = new System.Drawing.Point(288, 129);
		this.numericCountryLimitation.Maximum = new decimal(new int[4] { 12, 0, 0, 0 });
		this.numericCountryLimitation.Name = "numericCountryLimitation";
		this.numericCountryLimitation.Size = new System.Drawing.Size(85, 20);
		this.numericCountryLimitation.TabIndex = 19;
		this.numericCountryLimitation.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCountryLimitation.Value = new decimal(new int[4] { 4, 0, 0, 0 });
		this.numericCountryLimitation.ValueChanged += new System.EventHandler(numericCountryLimitation_ValueChanged);
		this.radioRule8.AutoSize = true;
		this.radioRule8.Location = new System.Drawing.Point(12, 104);
		this.radioRule8.Name = "radioRule8";
		this.radioRule8.Size = new System.Drawing.Size(204, 17);
		this.radioRule8.TabIndex = 20;
		this.radioRule8.TabStop = true;
		this.radioRule8.Text = "Get the Teams from a League in order";
		this.radioRule8.UseVisualStyleBackColor = true;
		this.radioRule8.CheckedChanged += new System.EventHandler(radioRule8_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(512, 301);
		base.Controls.Add(this.radioRule8);
		base.Controls.Add(this.numericCountryLimitation);
		base.Controls.Add(this.comboTeam);
		base.Controls.Add(this.numericN);
		base.Controls.Add(this.comboLeague);
		base.Controls.Add(this.comboTrophy2);
		base.Controls.Add(this.comboTrophy1);
		base.Controls.Add(this.radioRule7);
		base.Controls.Add(this.radioRule6);
		base.Controls.Add(this.radioRule5);
		base.Controls.Add(this.radioRule4);
		base.Controls.Add(this.radioRule3);
		base.Controls.Add(this.radioRule2);
		base.Controls.Add(this.radioRule1);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOk);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "QualifyRuleDialog";
		this.Text = "Qualification Rule";
		((System.ComponentModel.ISupportInitialize)this.numericN).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericCountryLimitation).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
