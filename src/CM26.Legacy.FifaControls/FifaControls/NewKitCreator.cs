using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class NewKitCreator : Form
{
	private static int[] s_KitTypeMap = new int[13]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 30, 31,
		32, 93, 94
	};

	private Kit m_NewKit;

	private int m_NewId = -1;

	private Kit m_SourceKit;

	private Team m_Team;

	private int m_KitType;

	private int m_YearTournamentId;

	private KitList m_KitList;

	private IContainer components;

	private Button button1;

	private Button buttonOK;

	private Label labelTeam;

	private Label labelKitType;

	private ComboBox comboKitTypes;

	private ComboBox comboTeams;

	private Label label1;

	private NumericUpDown numericYearTournament;

	public Kit NewKit => m_NewKit;

	public int NewId => m_NewId;

	public Kit SourceKit
	{
		set
		{
			m_NewKit = null;
			m_NewId = -1;
			m_SourceKit = value;
			m_Team = m_SourceKit.Team;
			m_KitType = m_SourceKit.kittype;
			comboTeams.SelectedItem = Team;
			int selectedIndex = 0;
			for (int i = 0; i < s_KitTypeMap.Length; i++)
			{
				if (m_KitType == s_KitTypeMap[i])
				{
					selectedIndex = i;
					break;
				}
			}
			comboKitTypes.SelectedIndex = selectedIndex;
			numericYearTournament.Value = m_SourceKit.year;
		}
	}

	public Team Team => m_Team;

	public int KitType => m_KitType;

	public int YearTournamentId => m_YearTournamentId;

	public KitList KitList
	{
		set
		{
			m_KitList = value;
		}
	}

	public NewKitCreator()
	{
		InitializeComponent();
	}

	public void SetTeams(TeamList teamList)
	{
		comboTeams.Items.Clear();
		comboTeams.Items.AddRange(teamList.ToArray());
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		if (m_KitList == null)
		{
			m_NewKit = null;
			return;
		}
		int teamid = 0;
		if (m_Team != null)
		{
			teamid = m_Team.Id;
		}
		if (m_KitList.Exists(teamid, m_KitType, m_YearTournamentId))
		{
			m_NewKit = null;
			return;
		}
		m_NewKit = (Kit)m_KitList.CloneId(m_SourceKit);
		m_NewKit.ResetKitTextures();
		if (m_NewKit != null)
		{
			m_NewId = m_NewKit.Id;
			m_NewKit.Team = m_Team;
			m_NewKit.kittype = m_KitType;
			m_NewKit.year = m_YearTournamentId;
		}
	}

	private void comboTeams_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboTeams.SelectedItem != null)
		{
			m_Team = (Team)comboTeams.SelectedItem;
		}
	}

	private void comboKitTypes_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboKitTypes.SelectedIndex >= 0)
		{
			int selectedIndex = comboKitTypes.SelectedIndex;
			m_KitType = s_KitTypeMap[selectedIndex];
		}
	}

	private void numericYearTournament_ValueChanged(object sender, EventArgs e)
	{
		m_YearTournamentId = (int)numericYearTournament.Value;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.NewKitCreator));
		this.button1 = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.labelTeam = new System.Windows.Forms.Label();
		this.labelKitType = new System.Windows.Forms.Label();
		this.comboKitTypes = new System.Windows.Forms.ComboBox();
		this.comboTeams = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.numericYearTournament = new System.Windows.Forms.NumericUpDown();
		((System.ComponentModel.ISupportInitialize)this.numericYearTournament).BeginInit();
		base.SuspendLayout();
		this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button1.Location = new System.Drawing.Point(173, 144);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 8;
		this.button1.Text = "Cancel";
		this.button1.UseVisualStyleBackColor = true;
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOK.Location = new System.Drawing.Point(48, 144);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(75, 23);
		this.buttonOK.TabIndex = 7;
		this.buttonOK.Text = "OK";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		this.labelTeam.AutoSize = true;
		this.labelTeam.BackColor = System.Drawing.Color.Transparent;
		this.labelTeam.Location = new System.Drawing.Point(23, 31);
		this.labelTeam.Name = "labelTeam";
		this.labelTeam.Size = new System.Drawing.Size(34, 13);
		this.labelTeam.TabIndex = 9;
		this.labelTeam.Text = "Team";
		this.labelKitType.AutoSize = true;
		this.labelKitType.BackColor = System.Drawing.Color.Transparent;
		this.labelKitType.Location = new System.Drawing.Point(23, 63);
		this.labelKitType.Name = "labelKitType";
		this.labelKitType.Size = new System.Drawing.Size(42, 13);
		this.labelKitType.TabIndex = 10;
		this.labelKitType.Text = "Kit type";
		this.comboKitTypes.FormattingEnabled = true;
		this.comboKitTypes.Items.AddRange(new object[13]
		{
			"Home", "Away", "Goalkeeper", "3rd Kit", "4th Kit", "Referee", "6th Kit", "7th Kit", "Home Goalkeeper (RevMod)", "Away Goalkeeper (RevMod)",
			"3rd Goalkeeper (RevMod)", "Home Training (RevMod)", "Away Training (RevMod)"
		});
		this.comboKitTypes.Location = new System.Drawing.Point(91, 60);
		this.comboKitTypes.Name = "comboKitTypes";
		this.comboKitTypes.Size = new System.Drawing.Size(178, 21);
		this.comboKitTypes.TabIndex = 11;
		this.comboKitTypes.SelectedIndexChanged += new System.EventHandler(comboKitTypes_SelectedIndexChanged);
		this.comboTeams.FormattingEnabled = true;
		this.comboTeams.Location = new System.Drawing.Point(91, 28);
		this.comboTeams.Name = "comboTeams";
		this.comboTeams.Size = new System.Drawing.Size(178, 21);
		this.comboTeams.TabIndex = 12;
		this.comboTeams.SelectedIndexChanged += new System.EventHandler(comboTeams_SelectedIndexChanged);
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.Location = new System.Drawing.Point(23, 96);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(109, 13);
		this.label1.TabIndex = 13;
		this.label1.Text = "Year \\ Tournament Id";
		this.numericYearTournament.Location = new System.Drawing.Point(140, 94);
		this.numericYearTournament.Maximum = new decimal(new int[4] { 1000000, 0, 0, 0 });
		this.numericYearTournament.Name = "numericYearTournament";
		this.numericYearTournament.Size = new System.Drawing.Size(129, 20);
		this.numericYearTournament.TabIndex = 14;
		this.numericYearTournament.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericYearTournament.ValueChanged += new System.EventHandler(numericYearTournament_ValueChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.ClientSize = new System.Drawing.Size(300, 179);
		base.Controls.Add(this.numericYearTournament);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.comboTeams);
		base.Controls.Add(this.comboKitTypes);
		base.Controls.Add(this.labelKitType);
		base.Controls.Add(this.labelTeam);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.buttonOK);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Name = "NewKitCreator";
		this.Text = "New Kit Selector";
		((System.ComponentModel.ISupportInitialize)this.numericYearTournament).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
