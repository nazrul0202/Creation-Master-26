using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CreationMaster;

public class TournamentWizard : Form
{
	private int m_TournamentStructure;

	private int m_LeagueGames;

	private int m_PreliminaryGames;

	private int m_GroupGames;

	private int m_KnockOutGames;

	private int m_FinalGames;

	private int m_NTeams;

	private int m_NPreliminaryTeams;

	private int m_NKnockOutTeams;

	private int m_NTeamsInGroups;

	private int m_NGroups;

	private int m_NTeamsPerGroup;

	private IContainer components;

	private Button buttonCancel;

	private Button buttonOK;

	private GroupBox groupKO;

	private NumericUpDown numericFinalGames;

	private NumericUpDown numericKOGames;

	private DomainUpDown domainNTeamsKO;

	private Label labelFinalGames;

	private Label labelNTeamsKO;

	private Label labelKnockOutGames;

	private GroupBox groupGroups;

	private NumericUpDown numericGamesPerGroup;

	private NumericUpDown numericTeamsPerGroup;

	private DomainUpDown domainNGroups;

	private Label labelGamesPerGroup;

	private Label labelNumberofGroups;

	private Label labelTeamPerGroup;

	private GroupBox groupPreliminary;

	private NumericUpDown numericPrelimGames;

	private NumericUpDown numericPreliminaryTeams;

	private Label labelNumberofGames;

	private Label labelPrelimNTeams;

	private GroupBox groupStructure;

	private RadioButton radioEuro2008;

	private RadioButton radioEuro2004;

	private RadioButton radioWC2006;

	private RadioButton radioPGKO;

	private RadioButton radioPKO;

	private RadioButton radioGKO;

	private RadioButton radioKO;

	private RadioButton radioLeague;

	private NumericUpDown numericNTeams;

	private Label labelNumberofTeams;

	private GroupBox groupLeague;

	private NumericUpDown numericLeagueGames;

	private Label labelLeagueGames;

	private GroupBox groupQualification;

	private Label labelLeagueReadHelp;

	public int TournamentStructure => m_TournamentStructure;

	public int LeagueGames => m_LeagueGames;

	public int PreliminaryGames => m_PreliminaryGames;

	public int GroupGames => m_GroupGames;

	public int KnockOutGames => m_KnockOutGames;

	public int FinalGames => m_FinalGames;

	public int NTeams => m_NTeams;

	public int NPreliminaryTeams => m_NPreliminaryTeams;

	public int NKnockOutTeams => m_NKnockOutTeams;

	public int NTeamsInGroups => m_NTeamsInGroups;

	public int NGroups => m_NGroups;

	public int NTeamsPerGroup => m_NTeamsPerGroup;

	public TournamentWizard()
	{
		InitializeComponent();
		m_NTeams = 3;
		m_TournamentStructure = 0;
		m_LeagueGames = 1;
		m_NKnockOutTeams = 2;
		m_KnockOutGames = 1;
		m_FinalGames = 1;
		m_NGroups = 1;
		m_NTeamsPerGroup = 0;
		m_GroupGames = 1;
		m_NPreliminaryTeams = 0;
		m_PreliminaryGames = 2;
	}

	private void numericNTeams_ValueChanged(object sender, EventArgs e)
	{
		m_NTeams = (int)numericNTeams.Value;
		InitOptions();
		ToPanel();
	}

	private void ToPanel()
	{
		RadioToPanel();
		OptionsToPanel();
		OkButtonToPanel();
	}

	private void RadioToPanel()
	{
		if (numericNTeams.Value >= 3m && numericNTeams.Value <= 24m)
		{
			radioLeague.Enabled = true;
		}
		else
		{
			radioLeague.Enabled = false;
			radioLeague.Checked = false;
		}
		if (numericNTeams.Value == 2m || numericNTeams.Value == 4m || numericNTeams.Value == 8m || numericNTeams.Value == 16m || numericNTeams.Value == 32m || numericNTeams.Value == 64m)
		{
			radioKO.Enabled = true;
		}
		else
		{
			radioKO.Enabled = false;
			radioKO.Checked = false;
		}
		if (numericNTeams.Value >= 3m)
		{
			if (numericNTeams.Value <= 16m || (((int)numericNTeams.Value & 1) == 0 && numericNTeams.Value <= 32m) || (((int)numericNTeams.Value & 3) == 0 && numericNTeams.Value <= 64m))
			{
				radioGKO.Enabled = true;
			}
			else
			{
				radioGKO.Enabled = false;
				radioGKO.Checked = false;
			}
		}
		else
		{
			radioGKO.Enabled = false;
			radioGKO.Checked = false;
		}
		if (numericNTeams.Value >= 3m && numericNTeams.Value != 4m && numericNTeams.Value != 8m && numericNTeams.Value != 16m && numericNTeams.Value != 32m && numericNTeams.Value != 64m)
		{
			radioPKO.Enabled = true;
		}
		else
		{
			radioPKO.Enabled = false;
			radioPKO.Checked = false;
		}
		if (numericNTeams.Value > 8m && numericNTeams.Value != 16m && numericNTeams.Value != 32m && numericNTeams.Value != 64m)
		{
			radioPGKO.Enabled = true;
			return;
		}
		radioPGKO.Enabled = false;
		radioPGKO.Checked = false;
	}

	private void OkButtonToPanel()
	{
		if (radioLeague.Checked || radioKO.Checked || radioPKO.Checked || radioGKO.Checked || radioPGKO.Checked || radioWC2006.Checked || radioEuro2008.Checked || radioEuro2004.Checked)
		{
			buttonOK.Enabled = true;
		}
		else
		{
			buttonOK.Enabled = false;
		}
	}

	private void OptionsToPanel()
	{
		if (radioKO.Checked || radioGKO.Checked || radioPKO.Checked || radioPGKO.Checked)
		{
			GroupBox groupBox = groupKO;
			bool enabled = (groupKO.Visible = true);
			groupBox.Enabled = enabled;
			KOToPanel();
		}
		else
		{
			groupKO.Visible = false;
		}
		if (radioPKO.Checked || radioPGKO.Checked)
		{
			GroupBox groupBox2 = groupPreliminary;
			bool enabled = (groupPreliminary.Visible = true);
			groupBox2.Enabled = enabled;
			PreliminaryToPanel();
		}
		else
		{
			groupPreliminary.Visible = false;
		}
		if (radioGKO.Checked || radioPGKO.Checked)
		{
			GroupBox groupBox3 = groupGroups;
			bool enabled = (groupGroups.Visible = true);
			groupBox3.Enabled = enabled;
			GroupToPanel();
		}
		else
		{
			groupGroups.Visible = false;
		}
		if (radioLeague.Checked)
		{
			groupLeague.Visible = true;
			LeagueToPanel();
		}
		else
		{
			groupLeague.Visible = false;
		}
		if (radioWC2006.Checked || radioEuro2008.Checked || radioEuro2004.Checked)
		{
			groupQualification.Visible = true;
			groupGroups.Visible = false;
			groupKO.Visible = false;
			groupGroups.Enabled = false;
			groupKO.Enabled = false;
		}
		else
		{
			groupQualification.Visible = false;
		}
	}

	private void GroupToPanel()
	{
		domainNGroups.SelectedItem = m_NGroups.ToString();
		domainNGroups.Text = m_NGroups.ToString();
		numericTeamsPerGroup.Value = m_NTeamsPerGroup;
		numericGamesPerGroup.Value = m_GroupGames;
	}

	private void KOToPanel()
	{
		domainNTeamsKO.SelectedItem = m_NKnockOutTeams.ToString();
		domainNTeamsKO.Text = m_NKnockOutTeams.ToString();
		numericFinalGames.Value = m_FinalGames;
		numericKOGames.Value = m_KnockOutGames;
	}

	private void PreliminaryToPanel()
	{
		numericPrelimGames.Value = m_PreliminaryGames;
		numericPreliminaryTeams.Value = m_NPreliminaryTeams;
	}

	private void LeagueToPanel()
	{
		numericLeagueGames.Value = m_LeagueGames;
	}

	private void InitOptions()
	{
		InitGroupsOptions();
		InitKnockOutOptions();
	}

	private void InitGroupsOptions()
	{
		switch (m_TournamentStructure)
		{
		case 3:
			domainNGroups.Items.Clear();
			m_NTeamsInGroups = m_NTeams;
			if (m_NTeamsInGroups % 16 == 0 && m_NTeamsInGroups >= 48)
			{
				domainNGroups.Items.Add("16");
				m_NGroups = 16;
			}
			if (m_NTeamsInGroups % 8 == 0 && m_NTeamsInGroups >= 24)
			{
				domainNGroups.Items.Add("8");
				m_NGroups = 8;
			}
			if (m_NTeamsInGroups % 4 == 0 && m_NTeamsInGroups >= 12 && m_NTeamsInGroups <= 64)
			{
				domainNGroups.Items.Add("4");
				m_NGroups = 4;
			}
			if (m_NTeamsInGroups % 2 == 0 && m_NTeamsInGroups >= 6 && m_NTeamsInGroups <= 32)
			{
				domainNGroups.Items.Add("2");
				m_NGroups = 2;
			}
			if (m_NTeamsInGroups <= 16)
			{
				domainNGroups.Items.Add("1");
				m_NGroups = 1;
			}
			domainNGroups.SelectedItem = m_NGroups.ToString();
			m_NTeamsPerGroup = m_NTeamsInGroups / m_NGroups;
			break;
		case 4:
			m_NTeamsInGroups = (int)Math.Pow(2.0, Math.Floor(Math.Log((double)m_NTeams - 1.0, 2.0)));
			domainNGroups.Items.Clear();
			if (m_NTeamsInGroups % 8 == 0 && m_NTeamsInGroups >= 24)
			{
				domainNGroups.Items.Add("8");
				m_NGroups = 8;
			}
			if (m_NTeamsInGroups % 4 == 0 && m_NTeamsInGroups >= 12 && m_NTeamsInGroups <= 64)
			{
				domainNGroups.Items.Add("4");
				m_NGroups = 4;
			}
			if (m_NTeamsInGroups % 2 == 0 && m_NTeamsInGroups >= 6 && m_NTeamsInGroups <= 32)
			{
				domainNGroups.Items.Add("2");
				m_NGroups = 2;
			}
			if (m_NTeamsInGroups <= 16)
			{
				domainNGroups.Items.Add("1");
				m_NGroups = 1;
			}
			domainNGroups.SelectedItem = m_NGroups.ToString();
			m_NTeamsPerGroup = m_NTeamsInGroups / m_NGroups;
			m_NPreliminaryTeams = (m_NTeams - m_NTeamsInGroups) * 2;
			break;
		}
	}

	private void InitKnockOutOptions()
	{
		switch (m_TournamentStructure)
		{
		case 1:
		case 2:
			m_NKnockOutTeams = (int)Math.Pow(2.0, Math.Floor(Math.Log((double)numericNTeams.Value, 2.0)));
			domainNTeamsKO.Items.Clear();
			domainNTeamsKO.Items.Add(m_NKnockOutTeams.ToString());
			domainNTeamsKO.Enabled = false;
			domainNTeamsKO.SelectedItem = m_NKnockOutTeams.ToString();
			m_NPreliminaryTeams = (m_NTeams - m_NKnockOutTeams) * 2;
			break;
		case 3:
		{
			int num2 = (int)Math.Pow(2.0, Math.Floor(Math.Log((double)m_NTeams - 1.0, 2.0)));
			domainNTeamsKO.Items.Clear();
			if (num2 >= 32 && 32 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("32");
			}
			if (num2 >= 16 && 16 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("16");
			}
			if (num2 >= 8 && 8 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("8");
			}
			if (num2 >= 4 && 4 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("4");
			}
			if (num2 >= 2 && 2 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("2");
			}
			domainNTeamsKO.Enabled = true;
			m_NKnockOutTeams = m_NGroups * 2;
			domainNTeamsKO.SelectedItem = m_NKnockOutTeams.ToString();
			break;
		}
		case 4:
		{
			int num = (int)Math.Pow(2.0, Math.Floor(Math.Log(m_NTeamsInGroups - 1, 2.0)));
			domainNTeamsKO.Items.Clear();
			if (num >= 32 && 32 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("32");
			}
			if (num >= 16 && 16 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("16");
			}
			if (num >= 8 && 8 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("8");
			}
			if (num >= 4 && 4 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("4");
			}
			if (num >= 2 && 2 >= m_NGroups)
			{
				domainNTeamsKO.Items.Add("2");
			}
			domainNTeamsKO.Enabled = true;
			m_NKnockOutTeams = m_NGroups * 2;
			domainNTeamsKO.SelectedItem = m_NKnockOutTeams.ToString();
			break;
		}
		case 0:
			break;
		}
	}

	private void InitKOToPanel()
	{
		if (m_TournamentStructure == 1 || m_TournamentStructure == 2)
		{
			m_NKnockOutTeams = (int)Math.Pow(2.0, Math.Floor(Math.Log((double)numericNTeams.Value, 2.0)));
			domainNTeamsKO.Enabled = true;
			domainNTeamsKO.Items.Clear();
			domainNTeamsKO.Items.Add(m_NKnockOutTeams.ToString());
			domainNTeamsKO.SelectedIndex = 0;
			m_NPreliminaryTeams = (m_NTeams - m_NKnockOutTeams) * 2;
			KOToPanel();
			if (m_TournamentStructure == 2)
			{
				PreliminaryToPanel();
			}
		}
		else if (m_TournamentStructure == 3)
		{
			m_NKnockOutTeams = (int)Math.Pow(2.0, Math.Floor(Math.Log((double)m_NTeams - 1.0, 2.0)));
			domainNTeamsKO.Items.Clear();
			if (m_NKnockOutTeams >= 2 && 2 > m_NGroups)
			{
				domainNTeamsKO.Items.Add("2");
			}
			if (m_NKnockOutTeams >= 4 && 4 > m_NGroups)
			{
				domainNTeamsKO.Items.Add("4");
			}
			if (m_NKnockOutTeams >= 8 && 8 > m_NGroups)
			{
				domainNTeamsKO.Items.Add("8");
			}
			if (m_NKnockOutTeams >= 16 && 16 > m_NGroups)
			{
				domainNTeamsKO.Items.Add("16");
			}
			if (m_NKnockOutTeams >= 32 && 32 > m_NGroups)
			{
				domainNTeamsKO.Items.Add("32");
			}
			domainNTeamsKO.SelectedIndex = 0;
			domainNTeamsKO.Enabled = true;
			KOToPanel();
		}
		else
		{
			_ = m_TournamentStructure;
			_ = 4;
		}
	}

	private void InitGroupsToPanel()
	{
		if (m_TournamentStructure == 3)
		{
			domainNGroups.Items.Clear();
			if (m_NTeams <= 16)
			{
				domainNGroups.Items.Add("1");
			}
			if (m_NTeams % 2 == 0 && m_NTeams >= 6)
			{
				domainNGroups.Items.Add("2");
			}
			if (m_NTeams % 4 == 0 && m_NTeams >= 12)
			{
				domainNGroups.Items.Add("4");
			}
			if (m_NTeams % 8 == 0 && m_NTeams >= 24)
			{
				domainNGroups.Items.Add("8");
			}
			domainNGroups.SelectedIndex = 0;
			numericTeamsPerGroup.Enabled = false;
			GroupToPanel();
		}
		else
		{
			_ = m_TournamentStructure;
			_ = 4;
		}
	}

	private void radioLeague_CheckedChanged(object sender, EventArgs e)
	{
		if (radioLeague.Checked)
		{
			m_TournamentStructure = 0;
			ToPanel();
		}
	}

	private void radioKO_CheckedChanged(object sender, EventArgs e)
	{
		if (radioKO.Checked)
		{
			m_TournamentStructure = 1;
			InitKnockOutOptions();
			ToPanel();
		}
	}

	private void radioPKO_CheckedChanged(object sender, EventArgs e)
	{
		if (radioPKO.Checked)
		{
			m_TournamentStructure = 2;
			InitKnockOutOptions();
			ToPanel();
		}
	}

	private void radioGKO_CheckedChanged(object sender, EventArgs e)
	{
		if (radioGKO.Checked)
		{
			m_TournamentStructure = 3;
			InitGroupsOptions();
			InitKnockOutOptions();
			ToPanel();
		}
	}

	private void radioPGKO_CheckedChanged(object sender, EventArgs e)
	{
		if (radioPGKO.Checked)
		{
			m_TournamentStructure = 4;
			InitGroupsOptions();
			InitKnockOutOptions();
			ToPanel();
		}
	}

	private void radioWC2006_CheckedChanged(object sender, EventArgs e)
	{
		if (radioWC2006.Checked)
		{
			m_TournamentStructure = 5;
			ToPanel();
		}
	}

	private void radioEuro2004_CheckedChanged(object sender, EventArgs e)
	{
		if (radioEuro2004.Checked)
		{
			m_TournamentStructure = 7;
			ToPanel();
		}
	}

	private void radioEuro2008_CheckedChanged(object sender, EventArgs e)
	{
		if (radioEuro2008.Checked)
		{
			m_TournamentStructure = 6;
			ToPanel();
		}
	}

	private void domainNTeamsKO_SelectedItemChanged(object sender, EventArgs e)
	{
		if (domainNTeamsKO.SelectedItem != null)
		{
			string obj = domainNTeamsKO.SelectedItem.ToString();
			if (obj == "2")
			{
				m_NKnockOutTeams = 2;
			}
			if (obj == "4")
			{
				m_NKnockOutTeams = 4;
			}
			if (obj == "8")
			{
				m_NKnockOutTeams = 8;
			}
			if (obj == "16")
			{
				m_NKnockOutTeams = 16;
			}
			if (obj == "32")
			{
				m_NKnockOutTeams = 32;
			}
			if (obj == "64")
			{
				m_NKnockOutTeams = 64;
			}
			if (m_TournamentStructure == 2)
			{
				m_NPreliminaryTeams = (m_NTeams - m_NKnockOutTeams) * 2;
			}
		}
	}

	private void domainNGroups_SelectedItemChanged(object sender, EventArgs e)
	{
		if (domainNGroups.SelectedItem != null)
		{
			string obj = domainNGroups.SelectedItem.ToString();
			if (obj == "1")
			{
				m_NGroups = 1;
			}
			if (obj == "2")
			{
				m_NGroups = 2;
			}
			if (obj == "4")
			{
				m_NGroups = 4;
			}
			if (obj == "8")
			{
				m_NGroups = 8;
			}
			if (obj == "16")
			{
				m_NGroups = 16;
			}
			InitKnockOutOptions();
			if (m_TournamentStructure == 3)
			{
				m_NTeamsPerGroup = m_NTeamsInGroups / m_NGroups;
				GroupToPanel();
				KOToPanel();
			}
			if (m_TournamentStructure == 4)
			{
				m_NTeamsPerGroup = m_NTeamsInGroups / m_NGroups;
				m_NPreliminaryTeams = (m_NTeams - m_NTeamsInGroups) * 2;
				GroupToPanel();
				KOToPanel();
				PreliminaryToPanel();
			}
		}
	}

	private void numericPrelimGames_ValueChanged(object sender, EventArgs e)
	{
		m_PreliminaryGames = (int)numericPrelimGames.Value;
	}

	private void numericGamesPerGroup_ValueChanged(object sender, EventArgs e)
	{
		m_GroupGames = (int)numericGamesPerGroup.Value;
	}

	private void numericKOGames_ValueChanged(object sender, EventArgs e)
	{
		m_KnockOutGames = (int)numericKOGames.Value;
	}

	private void numericFinalGames_ValueChanged(object sender, EventArgs e)
	{
		m_FinalGames = (int)numericFinalGames.Value;
	}

	private void numericLeagueGames_ValueChanged(object sender, EventArgs e)
	{
		m_LeagueGames = (int)numericLeagueGames.Value;
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
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.groupKO = new System.Windows.Forms.GroupBox();
		this.numericFinalGames = new System.Windows.Forms.NumericUpDown();
		this.numericKOGames = new System.Windows.Forms.NumericUpDown();
		this.domainNTeamsKO = new System.Windows.Forms.DomainUpDown();
		this.labelFinalGames = new System.Windows.Forms.Label();
		this.labelNTeamsKO = new System.Windows.Forms.Label();
		this.labelKnockOutGames = new System.Windows.Forms.Label();
		this.groupGroups = new System.Windows.Forms.GroupBox();
		this.numericGamesPerGroup = new System.Windows.Forms.NumericUpDown();
		this.numericTeamsPerGroup = new System.Windows.Forms.NumericUpDown();
		this.domainNGroups = new System.Windows.Forms.DomainUpDown();
		this.labelGamesPerGroup = new System.Windows.Forms.Label();
		this.labelNumberofGroups = new System.Windows.Forms.Label();
		this.labelTeamPerGroup = new System.Windows.Forms.Label();
		this.groupPreliminary = new System.Windows.Forms.GroupBox();
		this.numericPrelimGames = new System.Windows.Forms.NumericUpDown();
		this.numericPreliminaryTeams = new System.Windows.Forms.NumericUpDown();
		this.labelNumberofGames = new System.Windows.Forms.Label();
		this.labelPrelimNTeams = new System.Windows.Forms.Label();
		this.groupStructure = new System.Windows.Forms.GroupBox();
		this.radioEuro2008 = new System.Windows.Forms.RadioButton();
		this.radioEuro2004 = new System.Windows.Forms.RadioButton();
		this.radioWC2006 = new System.Windows.Forms.RadioButton();
		this.radioPGKO = new System.Windows.Forms.RadioButton();
		this.radioPKO = new System.Windows.Forms.RadioButton();
		this.radioGKO = new System.Windows.Forms.RadioButton();
		this.radioKO = new System.Windows.Forms.RadioButton();
		this.radioLeague = new System.Windows.Forms.RadioButton();
		this.numericNTeams = new System.Windows.Forms.NumericUpDown();
		this.labelNumberofTeams = new System.Windows.Forms.Label();
		this.groupLeague = new System.Windows.Forms.GroupBox();
		this.numericLeagueGames = new System.Windows.Forms.NumericUpDown();
		this.labelLeagueGames = new System.Windows.Forms.Label();
		this.groupQualification = new System.Windows.Forms.GroupBox();
		this.labelLeagueReadHelp = new System.Windows.Forms.Label();
		this.groupKO.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericFinalGames).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericKOGames).BeginInit();
		this.groupGroups.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericGamesPerGroup).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTeamsPerGroup).BeginInit();
		this.groupPreliminary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericPrelimGames).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPreliminaryTeams).BeginInit();
		this.groupStructure.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNTeams).BeginInit();
		this.groupLeague.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericLeagueGames).BeginInit();
		this.groupQualification.SuspendLayout();
		base.SuspendLayout();
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonCancel.Location = new System.Drawing.Point(389, 334);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(94, 44);
		this.buttonCancel.TabIndex = 150;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonOK.Location = new System.Drawing.Point(179, 334);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(94, 44);
		this.buttonOK.TabIndex = 148;
		this.buttonOK.Text = "Create Tournament";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.groupKO.Controls.Add(this.numericFinalGames);
		this.groupKO.Controls.Add(this.numericKOGames);
		this.groupKO.Controls.Add(this.domainNTeamsKO);
		this.groupKO.Controls.Add(this.labelFinalGames);
		this.groupKO.Controls.Add(this.labelNTeamsKO);
		this.groupKO.Controls.Add(this.labelKnockOutGames);
		this.groupKO.Location = new System.Drawing.Point(439, 184);
		this.groupKO.Name = "groupKO";
		this.groupKO.Size = new System.Drawing.Size(200, 144);
		this.groupKO.TabIndex = 147;
		this.groupKO.TabStop = false;
		this.groupKO.Text = "Knock Out Stage Options";
		this.groupKO.Visible = false;
		this.numericFinalGames.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.numericFinalGames.Location = new System.Drawing.Point(125, 91);
		this.numericFinalGames.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericFinalGames.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericFinalGames.Name = "numericFinalGames";
		this.numericFinalGames.ReadOnly = true;
		this.numericFinalGames.Size = new System.Drawing.Size(69, 20);
		this.numericFinalGames.TabIndex = 133;
		this.numericFinalGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericFinalGames.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericFinalGames.ValueChanged += new System.EventHandler(numericFinalGames_ValueChanged);
		this.numericKOGames.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.numericKOGames.Location = new System.Drawing.Point(125, 61);
		this.numericKOGames.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericKOGames.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericKOGames.Name = "numericKOGames";
		this.numericKOGames.ReadOnly = true;
		this.numericKOGames.Size = new System.Drawing.Size(69, 20);
		this.numericKOGames.TabIndex = 132;
		this.numericKOGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericKOGames.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericKOGames.ValueChanged += new System.EventHandler(numericKOGames_ValueChanged);
		this.domainNTeamsKO.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.domainNTeamsKO.Items.Add("2");
		this.domainNTeamsKO.Items.Add("4");
		this.domainNTeamsKO.Items.Add("8");
		this.domainNTeamsKO.Items.Add("16");
		this.domainNTeamsKO.Items.Add("32");
		this.domainNTeamsKO.Items.Add("64");
		this.domainNTeamsKO.Location = new System.Drawing.Point(125, 29);
		this.domainNTeamsKO.Name = "domainNTeamsKO";
		this.domainNTeamsKO.ReadOnly = true;
		this.domainNTeamsKO.Size = new System.Drawing.Size(69, 20);
		this.domainNTeamsKO.TabIndex = 131;
		this.domainNTeamsKO.Wrap = true;
		this.domainNTeamsKO.SelectedItemChanged += new System.EventHandler(domainNTeamsKO_SelectedItemChanged);
		this.labelFinalGames.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelFinalGames.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFinalGames.Location = new System.Drawing.Point(6, 89);
		this.labelFinalGames.Name = "labelFinalGames";
		this.labelFinalGames.Size = new System.Drawing.Size(188, 20);
		this.labelFinalGames.TabIndex = 130;
		this.labelFinalGames.Text = "Final Games";
		this.labelFinalGames.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelNTeamsKO.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelNTeamsKO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelNTeamsKO.Location = new System.Drawing.Point(6, 29);
		this.labelNTeamsKO.Name = "labelNTeamsKO";
		this.labelNTeamsKO.Size = new System.Drawing.Size(188, 20);
		this.labelNTeamsKO.TabIndex = 129;
		this.labelNTeamsKO.Text = "Number of Teams";
		this.labelNTeamsKO.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelKnockOutGames.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelKnockOutGames.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelKnockOutGames.Location = new System.Drawing.Point(6, 59);
		this.labelKnockOutGames.Name = "labelKnockOutGames";
		this.labelKnockOutGames.Size = new System.Drawing.Size(188, 20);
		this.labelKnockOutGames.TabIndex = 128;
		this.labelKnockOutGames.Text = "Knock Out Games";
		this.labelKnockOutGames.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupGroups.Controls.Add(this.numericGamesPerGroup);
		this.groupGroups.Controls.Add(this.numericTeamsPerGroup);
		this.groupGroups.Controls.Add(this.domainNGroups);
		this.groupGroups.Controls.Add(this.labelGamesPerGroup);
		this.groupGroups.Controls.Add(this.labelNumberofGroups);
		this.groupGroups.Controls.Add(this.labelTeamPerGroup);
		this.groupGroups.Location = new System.Drawing.Point(230, 184);
		this.groupGroups.Name = "groupGroups";
		this.groupGroups.Size = new System.Drawing.Size(200, 144);
		this.groupGroups.TabIndex = 146;
		this.groupGroups.TabStop = false;
		this.groupGroups.Text = "Groups Stage Options";
		this.groupGroups.Visible = false;
		this.numericGamesPerGroup.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.numericGamesPerGroup.Location = new System.Drawing.Point(125, 86);
		this.numericGamesPerGroup.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericGamesPerGroup.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericGamesPerGroup.Name = "numericGamesPerGroup";
		this.numericGamesPerGroup.ReadOnly = true;
		this.numericGamesPerGroup.Size = new System.Drawing.Size(69, 20);
		this.numericGamesPerGroup.TabIndex = 130;
		this.numericGamesPerGroup.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericGamesPerGroup.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericGamesPerGroup.ValueChanged += new System.EventHandler(numericGamesPerGroup_ValueChanged);
		this.numericTeamsPerGroup.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.numericTeamsPerGroup.Enabled = false;
		this.numericTeamsPerGroup.Location = new System.Drawing.Point(125, 56);
		this.numericTeamsPerGroup.Maximum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.numericTeamsPerGroup.Minimum = new decimal(new int[4] { 3, 0, 0, 0 });
		this.numericTeamsPerGroup.Name = "numericTeamsPerGroup";
		this.numericTeamsPerGroup.ReadOnly = true;
		this.numericTeamsPerGroup.Size = new System.Drawing.Size(69, 20);
		this.numericTeamsPerGroup.TabIndex = 129;
		this.numericTeamsPerGroup.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTeamsPerGroup.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.domainNGroups.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.domainNGroups.Items.Add("1");
		this.domainNGroups.Items.Add("2");
		this.domainNGroups.Items.Add("4");
		this.domainNGroups.Items.Add("8");
		this.domainNGroups.Location = new System.Drawing.Point(125, 26);
		this.domainNGroups.Name = "domainNGroups";
		this.domainNGroups.ReadOnly = true;
		this.domainNGroups.Size = new System.Drawing.Size(69, 20);
		this.domainNGroups.TabIndex = 128;
		this.domainNGroups.Wrap = true;
		this.domainNGroups.SelectedItemChanged += new System.EventHandler(domainNGroups_SelectedItemChanged);
		this.labelGamesPerGroup.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelGamesPerGroup.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelGamesPerGroup.Location = new System.Drawing.Point(6, 86);
		this.labelGamesPerGroup.Name = "labelGamesPerGroup";
		this.labelGamesPerGroup.Size = new System.Drawing.Size(188, 20);
		this.labelGamesPerGroup.TabIndex = 127;
		this.labelGamesPerGroup.Text = "Number of Games";
		this.labelGamesPerGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelNumberofGroups.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelNumberofGroups.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelNumberofGroups.Location = new System.Drawing.Point(6, 26);
		this.labelNumberofGroups.Name = "labelNumberofGroups";
		this.labelNumberofGroups.Size = new System.Drawing.Size(188, 20);
		this.labelNumberofGroups.TabIndex = 126;
		this.labelNumberofGroups.Text = "Number of Groups";
		this.labelNumberofGroups.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelTeamPerGroup.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelTeamPerGroup.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelTeamPerGroup.Location = new System.Drawing.Point(6, 56);
		this.labelTeamPerGroup.Name = "labelTeamPerGroup";
		this.labelTeamPerGroup.Size = new System.Drawing.Size(188, 20);
		this.labelTeamPerGroup.TabIndex = 125;
		this.labelTeamPerGroup.Text = "Teams per Group";
		this.labelTeamPerGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupPreliminary.Controls.Add(this.numericPrelimGames);
		this.groupPreliminary.Controls.Add(this.numericPreliminaryTeams);
		this.groupPreliminary.Controls.Add(this.labelNumberofGames);
		this.groupPreliminary.Controls.Add(this.labelPrelimNTeams);
		this.groupPreliminary.Location = new System.Drawing.Point(15, 184);
		this.groupPreliminary.Name = "groupPreliminary";
		this.groupPreliminary.Size = new System.Drawing.Size(200, 144);
		this.groupPreliminary.TabIndex = 144;
		this.groupPreliminary.TabStop = false;
		this.groupPreliminary.Text = "Preliminary Stage Options";
		this.groupPreliminary.Visible = false;
		this.numericPrelimGames.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.numericPrelimGames.Location = new System.Drawing.Point(125, 56);
		this.numericPrelimGames.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericPrelimGames.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPrelimGames.Name = "numericPrelimGames";
		this.numericPrelimGames.ReadOnly = true;
		this.numericPrelimGames.Size = new System.Drawing.Size(69, 20);
		this.numericPrelimGames.TabIndex = 127;
		this.numericPrelimGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPrelimGames.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPrelimGames.ValueChanged += new System.EventHandler(numericPrelimGames_ValueChanged);
		this.numericPreliminaryTeams.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.numericPreliminaryTeams.Enabled = false;
		this.numericPreliminaryTeams.Location = new System.Drawing.Point(125, 26);
		this.numericPreliminaryTeams.Maximum = new decimal(new int[4] { 64, 0, 0, 0 });
		this.numericPreliminaryTeams.Minimum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericPreliminaryTeams.Name = "numericPreliminaryTeams";
		this.numericPreliminaryTeams.ReadOnly = true;
		this.numericPreliminaryTeams.Size = new System.Drawing.Size(69, 20);
		this.numericPreliminaryTeams.TabIndex = 126;
		this.numericPreliminaryTeams.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPreliminaryTeams.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.labelNumberofGames.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelNumberofGames.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelNumberofGames.Location = new System.Drawing.Point(6, 56);
		this.labelNumberofGames.Name = "labelNumberofGames";
		this.labelNumberofGames.Size = new System.Drawing.Size(188, 20);
		this.labelNumberofGames.TabIndex = 125;
		this.labelNumberofGames.Text = "Number of Games";
		this.labelNumberofGames.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelPrelimNTeams.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelPrelimNTeams.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPrelimNTeams.Location = new System.Drawing.Point(6, 26);
		this.labelPrelimNTeams.Name = "labelPrelimNTeams";
		this.labelPrelimNTeams.Size = new System.Drawing.Size(188, 20);
		this.labelPrelimNTeams.TabIndex = 124;
		this.labelPrelimNTeams.Text = "Number of Teams";
		this.labelPrelimNTeams.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupStructure.Controls.Add(this.radioEuro2008);
		this.groupStructure.Controls.Add(this.radioEuro2004);
		this.groupStructure.Controls.Add(this.radioWC2006);
		this.groupStructure.Controls.Add(this.radioPGKO);
		this.groupStructure.Controls.Add(this.radioPKO);
		this.groupStructure.Controls.Add(this.radioGKO);
		this.groupStructure.Controls.Add(this.radioKO);
		this.groupStructure.Controls.Add(this.radioLeague);
		this.groupStructure.Location = new System.Drawing.Point(15, 35);
		this.groupStructure.Name = "groupStructure";
		this.groupStructure.Size = new System.Drawing.Size(624, 117);
		this.groupStructure.TabIndex = 143;
		this.groupStructure.TabStop = false;
		this.groupStructure.Text = "Tournament Structure";
		this.radioEuro2008.AutoSize = true;
		this.radioEuro2008.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioEuro2008.Location = new System.Drawing.Point(439, 79);
		this.radioEuro2008.Name = "radioEuro2008";
		this.radioEuro2008.Size = new System.Drawing.Size(109, 17);
		this.radioEuro2008.TabIndex = 7;
		this.radioEuro2008.TabStop = true;
		this.radioEuro2008.Text = "Euro 2008 Format";
		this.radioEuro2008.UseVisualStyleBackColor = true;
		this.radioEuro2008.CheckedChanged += new System.EventHandler(radioEuro2008_CheckedChanged);
		this.radioEuro2004.AutoSize = true;
		this.radioEuro2004.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioEuro2004.Location = new System.Drawing.Point(439, 56);
		this.radioEuro2004.Name = "radioEuro2004";
		this.radioEuro2004.Size = new System.Drawing.Size(109, 17);
		this.radioEuro2004.TabIndex = 6;
		this.radioEuro2004.TabStop = true;
		this.radioEuro2004.Text = "Euro 2004 Format";
		this.radioEuro2004.UseVisualStyleBackColor = true;
		this.radioEuro2004.CheckedChanged += new System.EventHandler(radioEuro2004_CheckedChanged);
		this.radioWC2006.AutoSize = true;
		this.radioWC2006.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioWC2006.Location = new System.Drawing.Point(439, 33);
		this.radioWC2006.Name = "radioWC2006";
		this.radioWC2006.Size = new System.Drawing.Size(78, 17);
		this.radioWC2006.TabIndex = 5;
		this.radioWC2006.TabStop = true;
		this.radioWC2006.Text = "WC Format";
		this.radioWC2006.UseVisualStyleBackColor = true;
		this.radioWC2006.CheckedChanged += new System.EventHandler(radioWC2006_CheckedChanged);
		this.radioPGKO.AutoSize = true;
		this.radioPGKO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioPGKO.Location = new System.Drawing.Point(195, 79);
		this.radioPGKO.Name = "radioPGKO";
		this.radioPGKO.Size = new System.Drawing.Size(210, 17);
		this.radioPGKO.TabIndex = 4;
		this.radioPGKO.TabStop = true;
		this.radioPGKO.Text = "Preliminary + Group Stage + Knock Out";
		this.radioPGKO.UseVisualStyleBackColor = true;
		this.radioPGKO.CheckedChanged += new System.EventHandler(radioPGKO_CheckedChanged);
		this.radioPKO.AutoSize = true;
		this.radioPKO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioPKO.Location = new System.Drawing.Point(195, 56);
		this.radioPKO.Name = "radioPKO";
		this.radioPKO.Size = new System.Drawing.Size(138, 17);
		this.radioPKO.TabIndex = 3;
		this.radioPKO.TabStop = true;
		this.radioPKO.Text = "Preliminary + Knock Out";
		this.radioPKO.UseVisualStyleBackColor = true;
		this.radioPKO.CheckedChanged += new System.EventHandler(radioPKO_CheckedChanged);
		this.radioGKO.AutoSize = true;
		this.radioGKO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioGKO.Location = new System.Drawing.Point(28, 79);
		this.radioGKO.Name = "radioGKO";
		this.radioGKO.Size = new System.Drawing.Size(151, 17);
		this.radioGKO.TabIndex = 2;
		this.radioGKO.TabStop = true;
		this.radioGKO.Text = "Groups Stage + Knock out";
		this.radioGKO.UseVisualStyleBackColor = true;
		this.radioGKO.CheckedChanged += new System.EventHandler(radioGKO_CheckedChanged);
		this.radioKO.AutoSize = true;
		this.radioKO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioKO.Location = new System.Drawing.Point(28, 56);
		this.radioKO.Name = "radioKO";
		this.radioKO.Size = new System.Drawing.Size(76, 17);
		this.radioKO.TabIndex = 1;
		this.radioKO.TabStop = true;
		this.radioKO.Text = "Knock Out";
		this.radioKO.UseVisualStyleBackColor = true;
		this.radioKO.CheckedChanged += new System.EventHandler(radioKO_CheckedChanged);
		this.radioLeague.AutoSize = true;
		this.radioLeague.Checked = true;
		this.radioLeague.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioLeague.Location = new System.Drawing.Point(28, 33);
		this.radioLeague.Name = "radioLeague";
		this.radioLeague.Size = new System.Drawing.Size(61, 17);
		this.radioLeague.TabIndex = 0;
		this.radioLeague.TabStop = true;
		this.radioLeague.Text = "League";
		this.radioLeague.UseVisualStyleBackColor = true;
		this.radioLeague.CheckedChanged += new System.EventHandler(radioLeague_CheckedChanged);
		this.numericNTeams.Location = new System.Drawing.Point(140, 9);
		this.numericNTeams.Maximum = new decimal(new int[4] { 64, 0, 0, 0 });
		this.numericNTeams.Minimum = new decimal(new int[4] { 3, 0, 0, 0 });
		this.numericNTeams.Name = "numericNTeams";
		this.numericNTeams.Size = new System.Drawing.Size(60, 20);
		this.numericNTeams.TabIndex = 141;
		this.numericNTeams.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNTeams.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.numericNTeams.ValueChanged += new System.EventHandler(numericNTeams_ValueChanged);
		this.labelNumberofTeams.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelNumberofTeams.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelNumberofTeams.Location = new System.Drawing.Point(12, 9);
		this.labelNumberofTeams.Name = "labelNumberofTeams";
		this.labelNumberofTeams.Size = new System.Drawing.Size(188, 20);
		this.labelNumberofTeams.TabIndex = 142;
		this.labelNumberofTeams.Text = "Number of Teams";
		this.labelNumberofTeams.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupLeague.Controls.Add(this.numericLeagueGames);
		this.groupLeague.Controls.Add(this.labelLeagueGames);
		this.groupLeague.Location = new System.Drawing.Point(15, 184);
		this.groupLeague.Name = "groupLeague";
		this.groupLeague.Size = new System.Drawing.Size(200, 144);
		this.groupLeague.TabIndex = 149;
		this.groupLeague.TabStop = false;
		this.groupLeague.Text = "League Options";
		this.groupLeague.Visible = false;
		this.numericLeagueGames.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.numericLeagueGames.Location = new System.Drawing.Point(125, 26);
		this.numericLeagueGames.Maximum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericLeagueGames.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericLeagueGames.Name = "numericLeagueGames";
		this.numericLeagueGames.ReadOnly = true;
		this.numericLeagueGames.Size = new System.Drawing.Size(69, 20);
		this.numericLeagueGames.TabIndex = 129;
		this.numericLeagueGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericLeagueGames.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.labelLeagueGames.BackColor = System.Drawing.SystemColors.ControlLight;
		this.labelLeagueGames.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeagueGames.Location = new System.Drawing.Point(6, 26);
		this.labelLeagueGames.Name = "labelLeagueGames";
		this.labelLeagueGames.Size = new System.Drawing.Size(188, 20);
		this.labelLeagueGames.TabIndex = 128;
		this.labelLeagueGames.Text = "Number of Games";
		this.labelLeagueGames.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupQualification.Controls.Add(this.labelLeagueReadHelp);
		this.groupQualification.Location = new System.Drawing.Point(15, 184);
		this.groupQualification.Name = "groupQualification";
		this.groupQualification.Size = new System.Drawing.Size(200, 144);
		this.groupQualification.TabIndex = 145;
		this.groupQualification.TabStop = false;
		this.groupQualification.Text = "Special Format";
		this.groupQualification.Visible = false;
		this.labelLeagueReadHelp.BackColor = System.Drawing.SystemColors.Control;
		this.labelLeagueReadHelp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeagueReadHelp.Location = new System.Drawing.Point(6, 62);
		this.labelLeagueReadHelp.Name = "labelLeagueReadHelp";
		this.labelLeagueReadHelp.Size = new System.Drawing.Size(188, 20);
		this.labelLeagueReadHelp.TabIndex = 127;
		this.labelLeagueReadHelp.Text = "See the Help";
		this.labelLeagueReadHelp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(649, 384);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOK);
		base.Controls.Add(this.groupKO);
		base.Controls.Add(this.groupGroups);
		base.Controls.Add(this.groupPreliminary);
		base.Controls.Add(this.groupStructure);
		base.Controls.Add(this.numericNTeams);
		base.Controls.Add(this.labelNumberofTeams);
		base.Controls.Add(this.groupLeague);
		base.Controls.Add(this.groupQualification);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "TournamentWizard";
		this.Text = "TournamentWizard";
		this.groupKO.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericFinalGames).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericKOGames).EndInit();
		this.groupGroups.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericGamesPerGroup).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTeamsPerGroup).EndInit();
		this.groupPreliminary.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericPrelimGames).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPreliminaryTeams).EndInit();
		this.groupStructure.ResumeLayout(false);
		this.groupStructure.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNTeams).EndInit();
		this.groupLeague.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericLeagueGames).EndInit();
		this.groupQualification.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
