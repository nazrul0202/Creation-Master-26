using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class QuickEditPlayerPanel : UserControl
{
	private bool m_Locked;

	private TeamPlayer m_TeamPlayer;

	private Player m_CurrentPlayer;

	private IContainer components;

	private ComboBox comboJerseyNumber;

	private TextBox textFirstName;

	private TextBox textSurname;

	private TextBox textJerseyName;

	private TextBox textCommonName;

	private ComboBox comboCountry;

	private DateTimePicker dateBirthDate;

	private ComboBox comboRole;

	private NumericUpDown numericOverall;

	private NumericUpDown numericContract;

	private DateTimePicker dateJoin;

	private DateTimePicker dateLoanEnd;

	private ComboBox comboLoaningTeam;

	private ComboBox comboPrevTeam;

	private CheckBox checkLoan;

	public TeamPlayer TeamPlayer
	{
		get
		{
			return m_TeamPlayer;
		}
		set
		{
			m_TeamPlayer = value;
		}
	}

	public Player Player => m_TeamPlayer.Player;

	public void SetTeams(object[] teams)
	{
		comboLoaningTeam.Items.Clear();
		comboLoaningTeam.Items.AddRange(teams);
		comboPrevTeam.Items.Clear();
		comboPrevTeam.Items.Add(string.Empty);
		comboPrevTeam.Items.AddRange(teams);
	}

	public void SetCountries(object[] countries)
	{
		comboCountry.Items.Clear();
		comboCountry.Items.AddRange(countries);
	}

	private int ConvertRole(ERole preferredRole)
	{
		switch (preferredRole)
		{
		case ERole.Goalkeeper:
			return 0;
		case ERole.Right_Wing_Back:
			return 1;
		case ERole.Right_Back:
		case ERole.Right_Central_Back:
			return 2;
		case ERole.Sweeper:
		case ERole.Central_Back:
			return 3;
		case ERole.Left_Central_Back:
		case ERole.Left_Back:
			return 4;
		case ERole.Left_Wing_Back:
			return 5;
		case ERole.Right_Defensive_Midfielder:
		case ERole.Central_Defensive_Midfielder:
		case ERole.Left_Defensive_Midfielder:
			return 6;
		case ERole.Right_Midfielder:
			return 7;
		case ERole.Central_Midfielder:
			return 8;
		case ERole.Left_Midfielder:
			return 9;
		case ERole.Right_Advanced_Midfielder:
		case ERole.Central_Advanced_Midfielder:
		case ERole.Left_Advanced_Midfielder:
			return 10;
		case ERole.Right_Forward:
		case ERole.Central_Forward:
		case ERole.Left_Forward:
			return 11;
		case ERole.Right_Wing:
			return 12;
		case ERole.Right_Striker:
		case ERole.Central_Striker:
		case ERole.Left_Striker:
			return 13;
		case ERole.Left_Wing:
			return 14;
		default:
			return 0;
		}
	}

	public new void Load(TeamPlayer teamPlayer)
	{
		m_TeamPlayer = teamPlayer;
		m_CurrentPlayer = teamPlayer.Player;
		m_Locked = true;
		comboJerseyNumber.SelectedIndex = m_TeamPlayer.jerseynumber - 1;
		textFirstName.Text = m_CurrentPlayer.firstname;
		textSurname.Text = m_CurrentPlayer.lastname;
		textCommonName.Text = m_CurrentPlayer.commonname;
		textJerseyName.Text = m_CurrentPlayer.playerjerseyname;
		dateBirthDate.Value = m_CurrentPlayer.birthdate;
		comboCountry.SelectedItem = m_CurrentPlayer.Country;
		numericOverall.Value = m_CurrentPlayer.overallrating;
		numericContract.Value = m_CurrentPlayer.contractvaliduntil;
		checkLoan.Checked = m_CurrentPlayer.IsLoaned;
		if (m_CurrentPlayer.IsLoaned)
		{
			dateLoanEnd.Value = m_CurrentPlayer.loandateend;
			comboLoaningTeam.SelectedItem = m_CurrentPlayer.TeamLoanedFrom;
			dateLoanEnd.Visible = true;
			comboLoaningTeam.Visible = true;
		}
		else
		{
			dateLoanEnd.Visible = false;
			comboLoaningTeam.Visible = false;
		}
		dateJoin.Value = m_CurrentPlayer.joindate;
		comboPrevTeam.SelectedItem = m_CurrentPlayer.PreviousTeam;
		ERole preferredposition = (ERole)m_CurrentPlayer.preferredposition1;
		comboRole.SelectedIndex = ConvertRole(preferredposition);
		m_Locked = false;
	}

	public QuickEditPlayerPanel()
	{
		InitializeComponent();
	}

	private void comboJerseyNumber_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_TeamPlayer.jerseynumber = comboJerseyNumber.SelectedIndex + 1;
		}
	}

	private void textFirstName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.firstname = textFirstName.Text;
		}
	}

	private void textSurname_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.FastRename(textSurname.Text);
		}
	}

	private void textCommonName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.commonname = textCommonName.Text;
			m_CurrentPlayer.audioname = m_CurrentPlayer.commonname;
			m_CurrentPlayer.commentaryid = 900000;
		}
	}

	private void textJerseyName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.commonname = textCommonName.Text;
		}
	}

	private void dateBirthDate_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.playerjerseyname = textJerseyName.Text;
		}
	}

	private void comboCountry_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.Country = (Country)comboCountry.SelectedItem;
		}
	}

	private void comboRole_SelectedIndexChanged(object sender, EventArgs e)
	{
		_ = m_Locked;
	}

	private void numericOverall_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		int num = (int)numericOverall.Value;
		int averageRoleAttribute = m_CurrentPlayer.GetAverageRoleAttribute();
		if (averageRoleAttribute < num)
		{
			for (int i = averageRoleAttribute; i < num; i++)
			{
				m_CurrentPlayer.IncreaseAllAttributes();
			}
			return;
		}
		for (int num2 = averageRoleAttribute; num2 > num; num2--)
		{
			m_CurrentPlayer.DecreaseAllAttributes();
		}
	}

	private void numericContract_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.contractvaliduntil = (int)numericContract.Value;
		}
	}

	private void checkLoan_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			if (checkLoan.Checked)
			{
				dateLoanEnd.Visible = true;
				comboLoaningTeam.Visible = true;
				m_CurrentPlayer.IsLoaned = true;
			}
			else
			{
				dateLoanEnd.Visible = false;
				comboLoaningTeam.Visible = false;
				m_CurrentPlayer.IsLoaned = false;
			}
		}
	}

	private void dateLoanEnd_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.loandateend = dateLoanEnd.Value;
		}
	}

	private void comboLoaningTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.TeamLoanedFrom = (Team)comboLoaningTeam.SelectedItem;
		}
	}

	private void dateJoin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.joindate = dateJoin.Value;
		}
	}

	private void comboPrevTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			if (comboPrevTeam.SelectedIndex == 0)
			{
				m_CurrentPlayer.PreviousTeam = null;
			}
			else
			{
				m_CurrentPlayer.PreviousTeam = (Team)comboPrevTeam.SelectedItem;
			}
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
		this.comboJerseyNumber = new System.Windows.Forms.ComboBox();
		this.textFirstName = new System.Windows.Forms.TextBox();
		this.textSurname = new System.Windows.Forms.TextBox();
		this.textJerseyName = new System.Windows.Forms.TextBox();
		this.textCommonName = new System.Windows.Forms.TextBox();
		this.comboCountry = new System.Windows.Forms.ComboBox();
		this.dateBirthDate = new System.Windows.Forms.DateTimePicker();
		this.comboRole = new System.Windows.Forms.ComboBox();
		this.numericOverall = new System.Windows.Forms.NumericUpDown();
		this.numericContract = new System.Windows.Forms.NumericUpDown();
		this.dateJoin = new System.Windows.Forms.DateTimePicker();
		this.dateLoanEnd = new System.Windows.Forms.DateTimePicker();
		this.comboLoaningTeam = new System.Windows.Forms.ComboBox();
		this.comboPrevTeam = new System.Windows.Forms.ComboBox();
		this.checkLoan = new System.Windows.Forms.CheckBox();
		((System.ComponentModel.ISupportInitialize)this.numericOverall).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericContract).BeginInit();
		base.SuspendLayout();
		this.comboJerseyNumber.FormattingEnabled = true;
		this.comboJerseyNumber.Items.AddRange(new object[99]
		{
			"1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
			"11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
			"21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
			"31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
			"41", "42", "43", "44", "45", "46", "47", "48", "49", "50",
			"51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
			"61", "62", "63", "64", "65", "66", "67", "68", "69", "70",
			"71", "72", "73", "74", "75", "76", "77", "78", "79", "80",
			"81", "82", "83", "84", "85", "86", "87", "88", "89", "90",
			"91", "92", "93", "94", "95", "96", "97", "98", "99"
		});
		this.comboJerseyNumber.Location = new System.Drawing.Point(1, 0);
		this.comboJerseyNumber.Name = "comboJerseyNumber";
		this.comboJerseyNumber.Size = new System.Drawing.Size(44, 21);
		this.comboJerseyNumber.TabIndex = 166;
		this.comboJerseyNumber.Text = "88";
		this.comboJerseyNumber.SelectedIndexChanged += new System.EventHandler(comboJerseyNumber_SelectedIndexChanged);
		this.textFirstName.Location = new System.Drawing.Point(46, 0);
		this.textFirstName.Name = "textFirstName";
		this.textFirstName.Size = new System.Drawing.Size(110, 20);
		this.textFirstName.TabIndex = 167;
		this.textFirstName.TextChanged += new System.EventHandler(textFirstName_TextChanged);
		this.textSurname.Location = new System.Drawing.Point(157, 0);
		this.textSurname.Name = "textSurname";
		this.textSurname.Size = new System.Drawing.Size(110, 20);
		this.textSurname.TabIndex = 168;
		this.textSurname.TextChanged += new System.EventHandler(textSurname_TextChanged);
		this.textJerseyName.Location = new System.Drawing.Point(379, 0);
		this.textJerseyName.Name = "textJerseyName";
		this.textJerseyName.Size = new System.Drawing.Size(110, 20);
		this.textJerseyName.TabIndex = 170;
		this.textJerseyName.TextChanged += new System.EventHandler(textJerseyName_TextChanged);
		this.textCommonName.Location = new System.Drawing.Point(268, 0);
		this.textCommonName.Name = "textCommonName";
		this.textCommonName.Size = new System.Drawing.Size(110, 20);
		this.textCommonName.TabIndex = 169;
		this.textCommonName.TextChanged += new System.EventHandler(textCommonName_TextChanged);
		this.comboCountry.ItemHeight = 13;
		this.comboCountry.Location = new System.Drawing.Point(571, 0);
		this.comboCountry.MaxLength = 32767;
		this.comboCountry.Name = "comboCountry";
		this.comboCountry.Size = new System.Drawing.Size(105, 21);
		this.comboCountry.TabIndex = 172;
		this.comboCountry.SelectedIndexChanged += new System.EventHandler(comboCountry_SelectedIndexChanged);
		this.dateBirthDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateBirthDate.Location = new System.Drawing.Point(490, 0);
		this.dateBirthDate.MaxDate = new System.DateTime(2006, 12, 31, 0, 0, 0, 0);
		this.dateBirthDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateBirthDate.Name = "dateBirthDate";
		this.dateBirthDate.Size = new System.Drawing.Size(80, 20);
		this.dateBirthDate.TabIndex = 171;
		this.dateBirthDate.Value = new System.DateTime(2006, 12, 31, 0, 0, 0, 0);
		this.dateBirthDate.ValueChanged += new System.EventHandler(dateBirthDate_ValueChanged);
		this.comboRole.FormattingEnabled = true;
		this.comboRole.Items.AddRange(new object[15]
		{
			"GK", "RWB", "RB", "CB", "LB", "LWB", "CDM", "RM", "CM", "LM",
			"CAM", "CF", "RW", "CS", "LW"
		});
		this.comboRole.Location = new System.Drawing.Point(677, 0);
		this.comboRole.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
		this.comboRole.Name = "comboRole";
		this.comboRole.Size = new System.Drawing.Size(63, 21);
		this.comboRole.TabIndex = 173;
		this.comboRole.SelectedIndexChanged += new System.EventHandler(comboRole_SelectedIndexChanged);
		this.numericOverall.Location = new System.Drawing.Point(741, 0);
		this.numericOverall.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
		this.numericOverall.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericOverall.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericOverall.Name = "numericOverall";
		this.numericOverall.Size = new System.Drawing.Size(43, 20);
		this.numericOverall.TabIndex = 174;
		this.numericOverall.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericOverall.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericOverall.ValueChanged += new System.EventHandler(numericOverall_ValueChanged);
		this.numericContract.Location = new System.Drawing.Point(785, 0);
		this.numericContract.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
		this.numericContract.Maximum = new decimal(new int[4] { 2030, 0, 0, 0 });
		this.numericContract.Minimum = new decimal(new int[4] { 2015, 0, 0, 0 });
		this.numericContract.Name = "numericContract";
		this.numericContract.Size = new System.Drawing.Size(55, 20);
		this.numericContract.TabIndex = 175;
		this.numericContract.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericContract.Value = new decimal(new int[4] { 2015, 0, 0, 0 });
		this.numericContract.ValueChanged += new System.EventHandler(numericContract_ValueChanged);
		this.dateJoin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateJoin.Location = new System.Drawing.Point(1095, 0);
		this.dateJoin.MaxDate = new System.DateTime(2020, 12, 31, 0, 0, 0, 0);
		this.dateJoin.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateJoin.Name = "dateJoin";
		this.dateJoin.Size = new System.Drawing.Size(80, 20);
		this.dateJoin.TabIndex = 176;
		this.dateJoin.Value = new System.DateTime(2016, 7, 1, 0, 0, 0, 0);
		this.dateJoin.ValueChanged += new System.EventHandler(dateJoin_ValueChanged);
		this.dateLoanEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateLoanEnd.Location = new System.Drawing.Point(863, 0);
		this.dateLoanEnd.MaxDate = new System.DateTime(2030, 12, 31, 0, 0, 0, 0);
		this.dateLoanEnd.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateLoanEnd.Name = "dateLoanEnd";
		this.dateLoanEnd.Size = new System.Drawing.Size(80, 20);
		this.dateLoanEnd.TabIndex = 177;
		this.dateLoanEnd.Value = new System.DateTime(2017, 6, 30, 0, 0, 0, 0);
		this.dateLoanEnd.ValueChanged += new System.EventHandler(dateLoanEnd_ValueChanged);
		this.comboLoaningTeam.ItemHeight = 13;
		this.comboLoaningTeam.Location = new System.Drawing.Point(944, 0);
		this.comboLoaningTeam.MaxLength = 32767;
		this.comboLoaningTeam.Name = "comboLoaningTeam";
		this.comboLoaningTeam.Size = new System.Drawing.Size(150, 21);
		this.comboLoaningTeam.TabIndex = 178;
		this.comboLoaningTeam.SelectedIndexChanged += new System.EventHandler(comboLoaningTeam_SelectedIndexChanged);
		this.comboPrevTeam.ItemHeight = 13;
		this.comboPrevTeam.Location = new System.Drawing.Point(1176, 0);
		this.comboPrevTeam.MaxLength = 32767;
		this.comboPrevTeam.Name = "comboPrevTeam";
		this.comboPrevTeam.Size = new System.Drawing.Size(150, 21);
		this.comboPrevTeam.TabIndex = 180;
		this.comboPrevTeam.SelectedIndexChanged += new System.EventHandler(comboPrevTeam_SelectedIndexChanged);
		this.checkLoan.Location = new System.Drawing.Point(845, 1);
		this.checkLoan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
		this.checkLoan.Name = "checkLoan";
		this.checkLoan.Size = new System.Drawing.Size(13, 20);
		this.checkLoan.TabIndex = 181;
		this.checkLoan.UseVisualStyleBackColor = true;
		this.checkLoan.CheckedChanged += new System.EventHandler(checkLoan_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.checkLoan);
		base.Controls.Add(this.dateJoin);
		base.Controls.Add(this.dateLoanEnd);
		base.Controls.Add(this.comboLoaningTeam);
		base.Controls.Add(this.comboPrevTeam);
		base.Controls.Add(this.numericContract);
		base.Controls.Add(this.numericOverall);
		base.Controls.Add(this.comboRole);
		base.Controls.Add(this.comboCountry);
		base.Controls.Add(this.dateBirthDate);
		base.Controls.Add(this.textFirstName);
		base.Controls.Add(this.textSurname);
		base.Controls.Add(this.textJerseyName);
		base.Controls.Add(this.textCommonName);
		base.Controls.Add(this.comboJerseyNumber);
		base.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
		base.Name = "QuickEditPlayerPanel";
		base.Size = new System.Drawing.Size(1357, 22);
		((System.ComponentModel.ISupportInitialize)this.numericOverall).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericContract).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
