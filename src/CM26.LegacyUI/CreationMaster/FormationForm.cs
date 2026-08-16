using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class FormationForm : Form
{
	private Formation m_CurrentFormation;

	private bool m_LockUserChanges;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private bool m_PositioningFlag;

	private Label[] m_LabelPos = new Label[11];

	private Label[] m_LabelArrowAtt1 = new Label[11];

	private Label[] m_LabelArrowDef1 = new Label[11];

	private Label m_MovingLabel;

	private int m_MovingLabelIndex;

	private int m_BoundLeft;

	private int m_BoundRight = 250;

	private int m_BoundTop;

	private int m_BoundBottom = 350;

	private Point m_LabelLocation = new Point(0, 0);

	private ComboBox[,] m_ComboPlayerInstructions = new ComboBox[11, 5];

	private ComboBox[] m_ComboInterceptions = new ComboBox[11];

	private IContainer components;

	public PickUpControl pickUpControl;

	private GroupBox groupTactic;

	private Label labelAssignTeam;

	private TextBox textName;

	private Label labelName;

	private Label label9;

	private Label label10;

	private Label label11;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private Label label3;

	private Label label4;

	private Label label2;

	private Label label1;

	private ComboBox comboBox7;

	private ComboBox comboBox1;

	private ComboBox comboBox8;

	private ComboBox comboBox2;

	private ComboBox comboBox9;

	private ComboBox comboBox3;

	private ComboBox comboBox10;

	private ComboBox comboBox6;

	private ComboBox comboBox11;

	private ComboBox comboBox5;

	private ComboBox comboBox4;

	private TabControl tabFormation;

	private TabPage pagePosition;

	private ImageList imageListPlayers;

	private ImageList imageListArrows;

	private Panel panelFormation;

	private BindingSource teamBindingSource;

	private Button buttonPresetFormation;

	private ComboBox comboFormation;

	private Label label12;

	private BindingSource teamListBindingSource;

	private CheckBox checkIsSweeper;

	private ComboBox comboOffensiveRating;

	private Label label14;

	private ComboBox comboPI_10;

	private ComboBox comboPI_11;

	private ComboBox comboPI_103;

	private ComboBox comboPI_102;

	private ComboBox comboPI_101;

	private ComboBox comboPI_100;

	private ComboBox comboPI_93;

	private ComboBox comboPI_92;

	private ComboBox comboPI_91;

	private ComboBox comboPI_90;

	private ComboBox comboPI_83;

	private ComboBox comboPI_82;

	private ComboBox comboPI_81;

	private ComboBox comboPI_80;

	private ComboBox comboPI_73;

	private ComboBox comboPI_72;

	private ComboBox comboPI_71;

	private ComboBox comboPI_70;

	private ComboBox comboPI_63;

	private ComboBox comboPI_62;

	private ComboBox comboPI_61;

	private ComboBox comboPI_60;

	private ComboBox comboPI_53;

	private ComboBox comboPI_52;

	private ComboBox comboPI_51;

	private ComboBox comboPI_50;

	private ComboBox comboPI_43;

	private ComboBox comboPI_42;

	private ComboBox comboPI_41;

	private ComboBox comboPI_40;

	private ComboBox comboPI_33;

	private ComboBox comboPI_32;

	private ComboBox comboPI_31;

	private ComboBox comboPI_30;

	private ComboBox comboPI_23;

	private ComboBox comboPI_22;

	private ComboBox comboPI_21;

	private ComboBox comboPI_20;

	private ComboBox comboPI_13;

	private ComboBox comboPI_12;

	private GroupBox groupInstructions;

	private ComboBox comboPI_104;

	private ComboBox comboPI_14;

	private ComboBox comboPI_94;

	private ComboBox comboPI_24;

	private ComboBox comboPI_84;

	private ComboBox comboPI_34;

	private ComboBox comboPI_74;

	private ComboBox comboPI_44;

	private ComboBox comboPI_64;

	private ComboBox comboPI_54;

	private Label label13;

	private ComboBox comboFormationAudio;

	private ComboBox comboInterceptions_1;

	private ComboBox comboInterceptions_10;

	private ComboBox comboInterceptions_6;

	private ComboBox comboInterceptions_9;

	private ComboBox comboInterceptions_5;

	private ComboBox comboInterceptions_8;

	private ComboBox comboInterceptions_4;

	private ComboBox comboInterceptions_7;

	private ComboBox comboInterceptions_3;

	private ComboBox comboInterceptions_2;

	private Label label15;

	private TextBox textFullName;

	private Label labelFullName;

	private NumericUpDown numericFullName;

	public FormationForm()
	{
		InitializeComponent();
		pickUpControl.SelectObject = SelectFormation;
		pickUpControl.CreateObject = CreateFormation;
		pickUpControl.DeleteObject = DeleteFormation;
		pickUpControl.CloneObject = CloneFormation;
		pickUpControl.RefreshObject = RefreshFormation;
		pickUpControl.combo.Sorted = false;
		for (int i = 0; i <= 10; i++)
		{
			m_LabelPos[i] = new Label();
			m_LabelPos[i].AutoSize = false;
			m_LabelPos[i].Location = new Point(118, (i + 4) * 20);
			m_LabelPos[i].ImageList = imageListPlayers;
			m_LabelPos[i].ImageIndex = i;
			m_LabelPos[i].Width = 16;
			m_LabelPos[i].Height = 16;
			m_LabelPos[i].Cursor = Cursors.Hand;
			m_LabelPos[i].MouseUp += MouseUpService;
			m_LabelPos[i].MouseMove += MouseMoveService;
			m_LabelPos[i].MouseDown += MouseDownService;
			pagePosition.Controls.Add(m_LabelPos[i]);
		}
		for (int j = 0; j <= 10; j++)
		{
			m_LabelArrowAtt1[j] = new Label();
			m_LabelArrowAtt1[j].AutoSize = false;
			m_LabelArrowAtt1[j].Location = new Point(50 + j * 10, (j + 1) * 20);
			m_LabelArrowAtt1[j].ImageList = imageListArrows;
			m_LabelArrowAtt1[j].ImageIndex = 0;
			m_LabelArrowAtt1[j].Width = 48;
			m_LabelArrowAtt1[j].Height = 48;
			m_LabelArrowAtt1[j].ForeColor = Color.Black;
			m_LabelArrowAtt1[j].Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			m_LabelArrowAtt1[j].Text = (j + 1).ToString();
			m_LabelArrowAtt1[j].TextAlign = ContentAlignment.MiddleCenter;
			m_LabelArrowAtt1[j].Cursor = Cursors.Hand;
		}
		for (int k = 0; k <= 10; k++)
		{
			m_LabelArrowDef1[k] = new Label();
			m_LabelArrowDef1[k].AutoSize = false;
			m_LabelArrowDef1[k].Location = new Point(50 + k * 10, (k + 1) * 20);
			m_LabelArrowDef1[k].ImageList = imageListArrows;
			m_LabelArrowDef1[k].ImageIndex = 0;
			m_LabelArrowDef1[k].Width = 48;
			m_LabelArrowDef1[k].Height = 48;
			m_LabelArrowDef1[k].ForeColor = Color.Black;
			m_LabelArrowDef1[k].Text = (k + 1).ToString();
			m_LabelArrowDef1[k].Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			m_LabelArrowDef1[k].TextAlign = ContentAlignment.MiddleCenter;
			m_LabelArrowDef1[k].Cursor = Cursors.Hand;
		}
		m_ComboPlayerInstructions[0, 0] = null;
		m_ComboPlayerInstructions[0, 1] = null;
		m_ComboPlayerInstructions[0, 2] = null;
		m_ComboPlayerInstructions[0, 3] = null;
		m_ComboPlayerInstructions[1, 0] = comboPI_10;
		m_ComboPlayerInstructions[1, 1] = comboPI_11;
		m_ComboPlayerInstructions[1, 2] = comboPI_12;
		m_ComboPlayerInstructions[1, 3] = comboPI_13;
		m_ComboPlayerInstructions[1, 4] = comboPI_14;
		m_ComboPlayerInstructions[2, 0] = comboPI_20;
		m_ComboPlayerInstructions[2, 1] = comboPI_21;
		m_ComboPlayerInstructions[2, 2] = comboPI_22;
		m_ComboPlayerInstructions[2, 3] = comboPI_23;
		m_ComboPlayerInstructions[2, 4] = comboPI_24;
		m_ComboPlayerInstructions[3, 0] = comboPI_30;
		m_ComboPlayerInstructions[3, 1] = comboPI_31;
		m_ComboPlayerInstructions[3, 2] = comboPI_32;
		m_ComboPlayerInstructions[3, 3] = comboPI_33;
		m_ComboPlayerInstructions[3, 4] = comboPI_34;
		m_ComboPlayerInstructions[4, 0] = comboPI_40;
		m_ComboPlayerInstructions[4, 1] = comboPI_41;
		m_ComboPlayerInstructions[4, 2] = comboPI_42;
		m_ComboPlayerInstructions[4, 3] = comboPI_43;
		m_ComboPlayerInstructions[4, 4] = comboPI_44;
		m_ComboPlayerInstructions[5, 0] = comboPI_50;
		m_ComboPlayerInstructions[5, 1] = comboPI_51;
		m_ComboPlayerInstructions[5, 2] = comboPI_52;
		m_ComboPlayerInstructions[5, 3] = comboPI_53;
		m_ComboPlayerInstructions[5, 4] = comboPI_54;
		m_ComboPlayerInstructions[6, 0] = comboPI_60;
		m_ComboPlayerInstructions[6, 1] = comboPI_61;
		m_ComboPlayerInstructions[6, 2] = comboPI_62;
		m_ComboPlayerInstructions[6, 3] = comboPI_63;
		m_ComboPlayerInstructions[6, 4] = comboPI_64;
		m_ComboPlayerInstructions[7, 0] = comboPI_70;
		m_ComboPlayerInstructions[7, 1] = comboPI_71;
		m_ComboPlayerInstructions[7, 2] = comboPI_72;
		m_ComboPlayerInstructions[7, 3] = comboPI_73;
		m_ComboPlayerInstructions[7, 4] = comboPI_74;
		m_ComboPlayerInstructions[8, 0] = comboPI_80;
		m_ComboPlayerInstructions[8, 1] = comboPI_81;
		m_ComboPlayerInstructions[8, 2] = comboPI_82;
		m_ComboPlayerInstructions[8, 3] = comboPI_83;
		m_ComboPlayerInstructions[8, 4] = comboPI_84;
		m_ComboPlayerInstructions[9, 0] = comboPI_90;
		m_ComboPlayerInstructions[9, 1] = comboPI_91;
		m_ComboPlayerInstructions[9, 2] = comboPI_92;
		m_ComboPlayerInstructions[9, 3] = comboPI_93;
		m_ComboPlayerInstructions[9, 4] = comboPI_94;
		m_ComboPlayerInstructions[10, 0] = comboPI_100;
		m_ComboPlayerInstructions[10, 1] = comboPI_101;
		m_ComboPlayerInstructions[10, 2] = comboPI_102;
		m_ComboPlayerInstructions[10, 3] = comboPI_103;
		m_ComboPlayerInstructions[10, 4] = comboPI_104;
		m_ComboInterceptions[0] = null;
		m_ComboInterceptions[1] = comboInterceptions_1;
		m_ComboInterceptions[2] = comboInterceptions_2;
		m_ComboInterceptions[3] = comboInterceptions_3;
		m_ComboInterceptions[4] = comboInterceptions_4;
		m_ComboInterceptions[5] = comboInterceptions_5;
		m_ComboInterceptions[6] = comboInterceptions_6;
		m_ComboInterceptions[7] = comboInterceptions_7;
		m_ComboInterceptions[8] = comboInterceptions_8;
		m_ComboInterceptions[9] = comboInterceptions_9;
		m_ComboInterceptions[10] = comboInterceptions_10;
	}

	private void FormationForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	public void Clean()
	{
		base.Visible = false;
	}

	private Formation SelectFormation(object sender, object obj)
	{
		Refresh();
		LoadFormation((Formation)obj);
		return (Formation)obj;
	}

	private Formation CreateFormation(object sender, object obj)
	{
		DialogResult dialogResult = m_NewIdCreator.ShowDialog();
		if (m_NewIdCreator.NewObject == null)
		{
			if (dialogResult == DialogResult.OK)
			{
				FifaEnvironment.UserMessages.ShowMessage(5060, m_NewIdCreator.NewId);
			}
			return null;
		}
		Formation formation = (Formation)m_NewIdCreator.NewObject;
		if (m_NewIdCreator.NewName != null && formation != null)
		{
			formation.Name = m_NewIdCreator.NewName;
		}
		return formation;
	}

	private Formation CloneFormation(object sender, object obj)
	{
		Formation srcIdObject = (Formation)obj;
		int newId = FifaEnvironment.Formations.GetNewId();
		return (Formation)FifaEnvironment.Formations.CloneId(srcIdObject, newId);
	}

	private Formation DeleteFormation(object sender, object obj)
	{
		Formation formation = (Formation)obj;
		if (formation.Team != null)
		{
			formation.Team.formationid = 0;
			formation.Team.Formation = null;
			formation.Team = null;
		}
		FifaEnvironment.Formations.RemoveId(formation);
		m_CurrentFormation = null;
		return null;
	}

	public Formation RefreshFormation(object sender, object obj)
	{
		Preset();
		ReloadFormation(m_CurrentFormation);
		return m_CurrentFormation;
	}

	public void ReloadFormation(Formation formation)
	{
		m_CurrentFormation = null;
		LoadFormation(formation);
	}

	public void LoadFormation(Formation formation)
	{
		if (!m_IsLoaded)
		{
			return;
		}
		m_LockUserChanges = true;
		if (m_CurrentFormation == formation)
		{
			return;
		}
		m_CurrentFormation = formation;
		m_PositioningFlag = true;
		for (int i = 0; i < 11; i++)
		{
			m_LabelPos[i].Tag = m_CurrentFormation.PlayingRoles[i];
			m_LabelArrowAtt1[i].Tag = m_CurrentFormation.PlayingRoles[i];
			m_LabelArrowDef1[i].Tag = m_CurrentFormation.PlayingRoles[i];
			PutLabelsOnField(i);
		}
		comboBox1.SelectedItem = formation.PlayingRoles[0].Role;
		comboBox2.SelectedItem = formation.PlayingRoles[1].Role;
		comboBox3.SelectedItem = formation.PlayingRoles[2].Role;
		comboBox4.SelectedItem = formation.PlayingRoles[3].Role;
		comboBox5.SelectedItem = formation.PlayingRoles[4].Role;
		comboBox6.SelectedItem = formation.PlayingRoles[5].Role;
		comboBox7.SelectedItem = formation.PlayingRoles[6].Role;
		comboBox8.SelectedItem = formation.PlayingRoles[7].Role;
		comboBox9.SelectedItem = formation.PlayingRoles[8].Role;
		comboBox10.SelectedItem = formation.PlayingRoles[9].Role;
		comboBox11.SelectedItem = formation.PlayingRoles[10].Role;
		m_PositioningFlag = false;
		checkIsSweeper.Checked = formation.formations_issweeper != 0;
		textName.Text = formation.Name;
		if (m_CurrentFormation.teamid < 0)
		{
			textFullName.Text = m_CurrentFormation.formationfullname;
			textFullName.Visible = true;
			labelFullName.Visible = true;
			numericFullName.Visible = true;
			textFullName.Enabled = m_CurrentFormation.formationfullnameid != -1;
			SetNumericValue(numericFullName, m_CurrentFormation.formationfullnameid);
		}
		else
		{
			textFullName.Text = string.Empty;
			textFullName.Visible = false;
			labelFullName.Visible = false;
			numericFullName.Visible = false;
		}
		SetSelectedIndex(comboOffensiveRating, formation.offensiverating);
		SetSelectedIndex(comboFormationAudio, formation.formationaudioid);
		labelAssignTeam.Text = ((formation.Team != null) ? formation.Team.ToString() : "Generic");
		if (FifaEnvironment.Year != 14)
		{
			for (int j = 1; j < 11; j++)
			{
				ShowPlayerInstruction(j);
			}
		}
		m_LockUserChanges = false;
	}

	public void AuditFc26RecordsForSmoke()
	{
		if (FifaEnvironment.Formations.Count == 0) return;
		var samples = new[] { 0, FifaEnvironment.Formations.Count / 2, FifaEnvironment.Formations.Count - 1 };
		foreach (var index in samples)
			ReloadFormation((Formation)FifaEnvironment.Formations[index]);
	}

	private static void SetNumericValue(NumericUpDown control, decimal value)
	{
		if (value < control.Minimum) control.Minimum = value;
		if (value > control.Maximum) control.Maximum = value;
		control.Value = value;
	}

	private static void SetSelectedIndex(ComboBox control, int index)
	{
		control.SelectedIndex = index >= 0 && index < control.Items.Count ? index : -1;
	}

	private void ShowPlayerInstruction(int playerIndex)
	{
		int id = m_CurrentFormation.PlayingRoles[playerIndex].Role.Id;
		int num = PlayingRole.c_InstrucionNumber[id];
		for (int i = 0; i < 5; i++)
		{
			m_ComboPlayerInstructions[playerIndex, i].Visible = i < num;
			m_ComboPlayerInstructions[playerIndex, i].Items.Clear();
		}
		for (int j = 0; j < num; j++)
		{
			int num2 = PlayingRole.c_InstrucionSetSelection[id, j];
			m_ComboPlayerInstructions[playerIndex, j].Tag = num2;
			for (int k = 0; k < PlayingRole.c_InstrucionSet[num2].Length; k++)
			{
				int num3 = PlayingRole.c_InstrucionSet[num2][k];
				string item = PlayingRole.c_InstructionCaption[num3];
				m_ComboPlayerInstructions[playerIndex, j].Items.Add(item);
				if ((m_CurrentFormation.PlayingRoles[playerIndex].PlayerInstruction_1 & (1 << num3)) != 0)
				{
					m_ComboPlayerInstructions[playerIndex, j].SelectedIndex = k;
				}
			}
		}
		switch (m_CurrentFormation.PlayingRoles[playerIndex].PlayerInstruction_2)
		{
		case 1:
			m_ComboInterceptions[playerIndex].SelectedIndex = 0;
			break;
		case 2:
			m_ComboInterceptions[playerIndex].SelectedIndex = 1;
			break;
		case 4:
			m_ComboInterceptions[playerIndex].SelectedIndex = 2;
			break;
		case 3:
			break;
		}
	}

	private void ComboRoleSelectedIndexChanged(object sender, int i)
	{
		if (m_PositioningFlag)
		{
			return;
		}
		ComboBox comboBox = (ComboBox)sender;
		if (comboBox.SelectedIndex < 0)
		{
			return;
		}
		Role role = (Role)comboBox.SelectedItem;
		Role role2 = m_CurrentFormation.PlayingRoles[i].Role;
		m_CurrentFormation.PlayingRoles[i].Role = role;
		m_CurrentFormation.PlayingRoles[i].PlayerInstruction_1 = PlayingRole.GetDefaultInstruction(role.Id);
		Point center = role.GetCenter();
		m_CurrentFormation.PlayingRoles[i].OffsetX = center.X;
		m_CurrentFormation.PlayingRoles[i].OffsetY = center.Y;
		PutLabelsOnField(i);
		foreach (Team team in FifaEnvironment.Teams)
		{
			if (team.Formation == m_CurrentFormation)
			{
				team.Roster.ChangeRole(role2, role);
			}
		}
		ShowPlayerInstruction(i);
	}

	private void comboInstruction_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_PositioningFlag)
		{
			return;
		}
		ComboBox comboBox = (ComboBox)sender;
		if (comboBox.SelectedIndex < 0)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < 11; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				if (m_ComboPlayerInstructions[i, j] == comboBox)
				{
					num = i;
					num2 = j;
				}
			}
		}
		if (num <= 0 || num2 < 0)
		{
			return;
		}
		int id = m_CurrentFormation.PlayingRoles[num].Role.Id;
		int num3 = PlayingRole.c_InstrucionSetSelection[id, num2];
		int num4 = PlayingRole.c_InstrucionSet[num3][comboBox.SelectedIndex];
		for (int k = 0; k < PlayingRole.c_InstrucionSet[num3].Length; k++)
		{
			int num5 = PlayingRole.c_InstrucionSet[num3][k];
			if (num5 == num4)
			{
				m_CurrentFormation.PlayingRoles[num].PlayerInstruction_1 |= 1 << num5;
			}
			else
			{
				m_CurrentFormation.PlayingRoles[num].PlayerInstruction_1 &= ~(1 << num5);
			}
		}
	}

	private void comboInterceptions_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_PositioningFlag)
		{
			return;
		}
		ComboBox comboBox = (ComboBox)sender;
		if (comboBox.SelectedIndex < 0)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < 11; i++)
		{
			if (m_ComboInterceptions[i] == comboBox)
			{
				num = i;
			}
		}
		if (num > 0)
		{
			m_CurrentFormation.PlayingRoles[num].PlayerInstruction_2 = 1 << comboBox.SelectedIndex;
		}
	}

	private void textName_TextChanged(object sender, EventArgs e)
	{
		m_CurrentFormation.Name = textName.Text;
		pickUpControl.SwitchObject(m_CurrentFormation);
	}

	private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 1);
	}

	private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 2);
	}

	private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 3);
	}

	private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 4);
	}

	private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 5);
	}

	private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 6);
	}

	private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 7);
	}

	private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 8);
	}

	private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 9);
	}

	private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboRoleSelectedIndexChanged(sender, 10);
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.Formations;
		IdArrayList[] filterValues = new IdArrayList[4]
		{
			null,
			FifaEnvironment.Leagues,
			FifaEnvironment.Countries,
			FifaEnvironment.Teams
		};
		pickUpControl.FilterValues = filterValues;
		comboBox1.Items.Clear();
		comboBox1.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox2.Items.Clear();
		comboBox2.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox3.Items.Clear();
		comboBox3.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox4.Items.Clear();
		comboBox4.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox5.Items.Clear();
		comboBox5.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox6.Items.Clear();
		comboBox6.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox7.Items.Clear();
		comboBox7.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox8.Items.Clear();
		comboBox8.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox9.Items.Clear();
		comboBox9.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox10.Items.Clear();
		comboBox10.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboBox11.Items.Clear();
		comboBox11.Items.AddRange(FifaEnvironment.Roles.ToArray());
		comboFormation.Items.Clear();
		foreach (Formation formation in FifaEnvironment.Formations)
		{
			if (formation.IsGeneric())
			{
				comboFormation.Items.Add(formation);
			}
		}
		pickUpControl.ObjectList = FifaEnvironment.Formations;
		groupInstructions.Visible = FifaEnvironment.Year != 14;
	}

	private void PutLabelsOnField(int i)
	{
		int offsetX = m_CurrentFormation.PlayingRoles[i].OffsetX;
		int offsetY = m_CurrentFormation.PlayingRoles[i].OffsetY;
		offsetX = OffsetToFieldX(offsetX);
		offsetY = OffsetToFieldY(offsetY);
		m_LabelPos[i].Location = new Point(offsetX, offsetY);
		offsetX -= 16;
		offsetY -= 16;
	}

	private int FieldXToOffset(int x)
	{
		return (250 - (x + 8)) * 2 / 5;
	}

	private int FieldYToOffset(int y)
	{
		return (y + 8) * 2 / 7;
	}

	private int OffsetToFieldX(int x)
	{
		return 250 - (x * 2 + x / 2) - 8;
	}

	private int OffsetToFieldY(int y)
	{
		return y * 3 + y / 2 - 8;
	}

	private void MouseUpService(object sender, MouseEventArgs e)
	{
		int num = m_MovingLabel.Location.X;
		int num2 = m_MovingLabel.Location.Y;
		m_MovingLabel = null;
		if (m_MovingLabelIndex >= 0)
		{
			m_CurrentFormation.PlayingRoles[m_MovingLabelIndex].m_OffsetX = FieldXToOffset(num);
			m_CurrentFormation.PlayingRoles[m_MovingLabelIndex].m_OffsetY = FieldYToOffset(num2);
			PutLabelsOnField(m_MovingLabelIndex);
		}
	}

	private void MouseMoveService(object sender, MouseEventArgs e)
	{
		if (m_MovingLabel != null)
		{
			MovePicture(e, m_MovingLabel);
		}
	}

	private void MouseDownService(object sender, MouseEventArgs e)
	{
		m_MovingLabel = (Label)sender;
		m_MovingLabelIndex = -1;
		for (int i = 0; i < 11; i++)
		{
			if (m_MovingLabel == m_LabelPos[i])
			{
				m_MovingLabelIndex = i;
				m_BoundRight = OffsetToFieldX(m_CurrentFormation.PlayingRoles[i].Role.Xmin);
				m_BoundLeft = OffsetToFieldX(m_CurrentFormation.PlayingRoles[i].Role.Xmax);
				m_BoundTop = OffsetToFieldY(m_CurrentFormation.PlayingRoles[i].Role.Ymin);
				m_BoundBottom = OffsetToFieldY(m_CurrentFormation.PlayingRoles[i].Role.Ymax);
				break;
			}
		}
	}

	private void MovePicture(MouseEventArgs e, Label picture)
	{
		int num = e.X - 8;
		int num2 = e.Y - 8;
		m_LabelLocation.X = picture.Location.X + num;
		m_LabelLocation.Y = picture.Location.Y + num2;
		if (m_LabelLocation.X < m_BoundLeft)
		{
			m_LabelLocation.X = m_BoundLeft;
		}
		if (m_LabelLocation.X > m_BoundRight)
		{
			m_LabelLocation.X = m_BoundRight;
		}
		if (m_LabelLocation.Y < m_BoundTop)
		{
			m_LabelLocation.Y = m_BoundTop;
		}
		if (m_LabelLocation.Y > m_BoundBottom)
		{
			m_LabelLocation.Y = m_BoundBottom;
		}
		picture.Location = m_LabelLocation;
	}

	private void buttonPresetFormation_Click(object sender, EventArgs e)
	{
		Formation formation = (Formation)comboFormation.SelectedItem;
		if (formation != null)
		{
			m_CurrentFormation.ReInitialize(formation);
		}
		if (m_CurrentFormation.Team != null)
		{
			m_CurrentFormation.Team.AssignTitolarToRoles(m_CurrentFormation);
		}
		ReloadFormation(m_CurrentFormation);
	}

	private void checkIsSweeper_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			m_CurrentFormation.formations_issweeper = (checkIsSweeper.Checked ? 1 : 0);
		}
	}

	private void comboOffensiveRating_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboOffensiveRating.SelectedIndex >= 0)
		{
			m_CurrentFormation.offensiverating = comboOffensiveRating.SelectedIndex;
		}
	}

	private void labelAssignTeam_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentFormation.Team != null)
		{
			MainForm.CM.JumpTo(m_CurrentFormation.Team);
		}
	}

	private void comboFormationAudio_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboFormationAudio.SelectedIndex >= 0)
		{
			m_CurrentFormation.formationaudioid = comboFormationAudio.SelectedIndex;
		}
	}

	private void textFullName_TextChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges && m_CurrentFormation.teamid < 0 && m_CurrentFormation.formationfullnameid != -1 && m_CurrentFormation.formationfullname != textFullName.Text)
		{
			m_CurrentFormation.formationfullname = textFullName.Text;
			FifaEnvironment.Language.SetFormationString(m_CurrentFormation.formationfullnameid, m_CurrentFormation.formationfullname);
		}
	}

	private void numericFullName_ValueChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			m_CurrentFormation.formationfullnameid = (int)numericFullName.Value;
			m_CurrentFormation.formationfullname = FifaEnvironment.Language.GetFormationString(m_CurrentFormation.formationfullnameid);
			if (m_CurrentFormation.formationfullnameid == -1)
			{
				textFullName.Enabled = false;
				textFullName.Text = string.Empty;
			}
			else
			{
				textFullName.Enabled = true;
				textFullName.Text = m_CurrentFormation.formationfullname;
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.FormationForm));
		this.groupTactic = new System.Windows.Forms.GroupBox();
		this.textFullName = new System.Windows.Forms.TextBox();
		this.labelFullName = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.comboFormationAudio = new System.Windows.Forms.ComboBox();
		this.groupInstructions = new System.Windows.Forms.GroupBox();
		this.comboInterceptions_10 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_6 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_9 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_5 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_8 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_4 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_7 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_3 = new System.Windows.Forms.ComboBox();
		this.comboInterceptions_2 = new System.Windows.Forms.ComboBox();
		this.label15 = new System.Windows.Forms.Label();
		this.comboInterceptions_1 = new System.Windows.Forms.ComboBox();
		this.comboPI_104 = new System.Windows.Forms.ComboBox();
		this.comboPI_14 = new System.Windows.Forms.ComboBox();
		this.comboPI_94 = new System.Windows.Forms.ComboBox();
		this.comboPI_24 = new System.Windows.Forms.ComboBox();
		this.comboPI_84 = new System.Windows.Forms.ComboBox();
		this.comboPI_34 = new System.Windows.Forms.ComboBox();
		this.comboPI_74 = new System.Windows.Forms.ComboBox();
		this.comboPI_44 = new System.Windows.Forms.ComboBox();
		this.comboPI_64 = new System.Windows.Forms.ComboBox();
		this.comboPI_54 = new System.Windows.Forms.ComboBox();
		this.comboPI_10 = new System.Windows.Forms.ComboBox();
		this.comboPI_103 = new System.Windows.Forms.ComboBox();
		this.comboPI_11 = new System.Windows.Forms.ComboBox();
		this.comboPI_102 = new System.Windows.Forms.ComboBox();
		this.comboPI_12 = new System.Windows.Forms.ComboBox();
		this.comboPI_101 = new System.Windows.Forms.ComboBox();
		this.comboPI_13 = new System.Windows.Forms.ComboBox();
		this.comboPI_100 = new System.Windows.Forms.ComboBox();
		this.comboPI_20 = new System.Windows.Forms.ComboBox();
		this.comboPI_93 = new System.Windows.Forms.ComboBox();
		this.comboPI_21 = new System.Windows.Forms.ComboBox();
		this.comboPI_92 = new System.Windows.Forms.ComboBox();
		this.comboPI_22 = new System.Windows.Forms.ComboBox();
		this.comboPI_91 = new System.Windows.Forms.ComboBox();
		this.comboPI_23 = new System.Windows.Forms.ComboBox();
		this.comboPI_90 = new System.Windows.Forms.ComboBox();
		this.comboPI_30 = new System.Windows.Forms.ComboBox();
		this.comboPI_83 = new System.Windows.Forms.ComboBox();
		this.comboPI_31 = new System.Windows.Forms.ComboBox();
		this.comboPI_82 = new System.Windows.Forms.ComboBox();
		this.comboPI_32 = new System.Windows.Forms.ComboBox();
		this.comboPI_81 = new System.Windows.Forms.ComboBox();
		this.comboPI_33 = new System.Windows.Forms.ComboBox();
		this.comboPI_80 = new System.Windows.Forms.ComboBox();
		this.comboPI_40 = new System.Windows.Forms.ComboBox();
		this.comboPI_73 = new System.Windows.Forms.ComboBox();
		this.comboPI_41 = new System.Windows.Forms.ComboBox();
		this.comboPI_72 = new System.Windows.Forms.ComboBox();
		this.comboPI_42 = new System.Windows.Forms.ComboBox();
		this.comboPI_71 = new System.Windows.Forms.ComboBox();
		this.comboPI_43 = new System.Windows.Forms.ComboBox();
		this.comboPI_70 = new System.Windows.Forms.ComboBox();
		this.comboPI_50 = new System.Windows.Forms.ComboBox();
		this.comboPI_63 = new System.Windows.Forms.ComboBox();
		this.comboPI_51 = new System.Windows.Forms.ComboBox();
		this.comboPI_62 = new System.Windows.Forms.ComboBox();
		this.comboPI_52 = new System.Windows.Forms.ComboBox();
		this.comboPI_61 = new System.Windows.Forms.ComboBox();
		this.comboPI_53 = new System.Windows.Forms.ComboBox();
		this.comboPI_60 = new System.Windows.Forms.ComboBox();
		this.checkIsSweeper = new System.Windows.Forms.CheckBox();
		this.comboOffensiveRating = new System.Windows.Forms.ComboBox();
		this.label14 = new System.Windows.Forms.Label();
		this.buttonPresetFormation = new System.Windows.Forms.Button();
		this.comboFormation = new System.Windows.Forms.ComboBox();
		this.label12 = new System.Windows.Forms.Label();
		this.labelAssignTeam = new System.Windows.Forms.Label();
		this.textName = new System.Windows.Forms.TextBox();
		this.labelName = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.comboBox7 = new System.Windows.Forms.ComboBox();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.comboBox8 = new System.Windows.Forms.ComboBox();
		this.comboBox2 = new System.Windows.Forms.ComboBox();
		this.comboBox9 = new System.Windows.Forms.ComboBox();
		this.comboBox3 = new System.Windows.Forms.ComboBox();
		this.comboBox10 = new System.Windows.Forms.ComboBox();
		this.comboBox6 = new System.Windows.Forms.ComboBox();
		this.comboBox11 = new System.Windows.Forms.ComboBox();
		this.comboBox5 = new System.Windows.Forms.ComboBox();
		this.comboBox4 = new System.Windows.Forms.ComboBox();
		this.tabFormation = new System.Windows.Forms.TabControl();
		this.pagePosition = new System.Windows.Forms.TabPage();
		this.imageListPlayers = new System.Windows.Forms.ImageList(this.components);
		this.imageListArrows = new System.Windows.Forms.ImageList(this.components);
		this.panelFormation = new System.Windows.Forms.Panel();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.teamBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.teamListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.numericFullName = new System.Windows.Forms.NumericUpDown();
		this.groupTactic.SuspendLayout();
		this.groupInstructions.SuspendLayout();
		this.tabFormation.SuspendLayout();
		this.panelFormation.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.teamBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericFullName).BeginInit();
		base.SuspendLayout();
		this.groupTactic.AutoSize = true;
		this.groupTactic.Controls.Add(this.numericFullName);
		this.groupTactic.Controls.Add(this.textFullName);
		this.groupTactic.Controls.Add(this.labelFullName);
		this.groupTactic.Controls.Add(this.label13);
		this.groupTactic.Controls.Add(this.comboFormationAudio);
		this.groupTactic.Controls.Add(this.groupInstructions);
		this.groupTactic.Controls.Add(this.checkIsSweeper);
		this.groupTactic.Controls.Add(this.comboOffensiveRating);
		this.groupTactic.Controls.Add(this.label14);
		this.groupTactic.Controls.Add(this.buttonPresetFormation);
		this.groupTactic.Controls.Add(this.comboFormation);
		this.groupTactic.Controls.Add(this.label12);
		this.groupTactic.Controls.Add(this.labelAssignTeam);
		this.groupTactic.Controls.Add(this.textName);
		this.groupTactic.Controls.Add(this.labelName);
		this.groupTactic.Controls.Add(this.label9);
		this.groupTactic.Controls.Add(this.label10);
		this.groupTactic.Controls.Add(this.label11);
		this.groupTactic.Controls.Add(this.label5);
		this.groupTactic.Controls.Add(this.label6);
		this.groupTactic.Controls.Add(this.label7);
		this.groupTactic.Controls.Add(this.label8);
		this.groupTactic.Controls.Add(this.label3);
		this.groupTactic.Controls.Add(this.label4);
		this.groupTactic.Controls.Add(this.label2);
		this.groupTactic.Controls.Add(this.label1);
		this.groupTactic.Controls.Add(this.comboBox7);
		this.groupTactic.Controls.Add(this.comboBox1);
		this.groupTactic.Controls.Add(this.comboBox8);
		this.groupTactic.Controls.Add(this.comboBox2);
		this.groupTactic.Controls.Add(this.comboBox9);
		this.groupTactic.Controls.Add(this.comboBox3);
		this.groupTactic.Controls.Add(this.comboBox10);
		this.groupTactic.Controls.Add(this.comboBox6);
		this.groupTactic.Controls.Add(this.comboBox11);
		this.groupTactic.Controls.Add(this.comboBox5);
		this.groupTactic.Controls.Add(this.comboBox4);
		this.groupTactic.Location = new System.Drawing.Point(267, 6);
		this.groupTactic.Name = "groupTactic";
		this.groupTactic.Size = new System.Drawing.Size(1079, 490);
		this.groupTactic.TabIndex = 8;
		this.groupTactic.TabStop = false;
		this.groupTactic.Text = "Roles";
		this.textFullName.Location = new System.Drawing.Point(461, 35);
		this.textFullName.Name = "textFullName";
		this.textFullName.Size = new System.Drawing.Size(187, 20);
		this.textFullName.TabIndex = 80;
		this.textFullName.TextChanged += new System.EventHandler(textFullName_TextChanged);
		this.labelFullName.AutoSize = true;
		this.labelFullName.Location = new System.Drawing.Point(246, 38);
		this.labelFullName.Name = "labelFullName";
		this.labelFullName.Size = new System.Drawing.Size(128, 13);
		this.labelFullName.TabIndex = 79;
		this.labelFullName.Text = "Language Name Attribute";
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(15, 368);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(34, 13);
		this.label13.TabIndex = 78;
		this.label13.Text = "Audio";
		this.comboFormationAudio.FormattingEnabled = true;
		this.comboFormationAudio.Items.AddRange(new object[16]
		{
			"3-4-3 audio", "3-5-2 audio", "4-2-4 audio", "5-2-3 audio", "5-3-2 audio", "5-4-1 audio", "4-3-3 audio", "4-3-3 Falso Nueve audio", "4-3-2-1 audio", "4-3-1-2 audio",
			"4-4-2 audio", "4-1-2-1-2 audio", "4-1-3-2 audio", "4-4-1-1 audio", "4-2-3-1 audio", "4-5-1 audio"
		});
		this.comboFormationAudio.Location = new System.Drawing.Point(70, 365);
		this.comboFormationAudio.Name = "comboFormationAudio";
		this.comboFormationAudio.Size = new System.Drawing.Size(170, 21);
		this.comboFormationAudio.TabIndex = 77;
		this.comboFormationAudio.SelectedIndexChanged += new System.EventHandler(comboFormationAudio_SelectedIndexChanged);
		this.groupInstructions.Controls.Add(this.comboInterceptions_10);
		this.groupInstructions.Controls.Add(this.comboInterceptions_6);
		this.groupInstructions.Controls.Add(this.comboInterceptions_9);
		this.groupInstructions.Controls.Add(this.comboInterceptions_5);
		this.groupInstructions.Controls.Add(this.comboInterceptions_8);
		this.groupInstructions.Controls.Add(this.comboInterceptions_4);
		this.groupInstructions.Controls.Add(this.comboInterceptions_7);
		this.groupInstructions.Controls.Add(this.comboInterceptions_3);
		this.groupInstructions.Controls.Add(this.comboInterceptions_2);
		this.groupInstructions.Controls.Add(this.label15);
		this.groupInstructions.Controls.Add(this.comboInterceptions_1);
		this.groupInstructions.Controls.Add(this.comboPI_104);
		this.groupInstructions.Controls.Add(this.comboPI_14);
		this.groupInstructions.Controls.Add(this.comboPI_94);
		this.groupInstructions.Controls.Add(this.comboPI_24);
		this.groupInstructions.Controls.Add(this.comboPI_84);
		this.groupInstructions.Controls.Add(this.comboPI_34);
		this.groupInstructions.Controls.Add(this.comboPI_74);
		this.groupInstructions.Controls.Add(this.comboPI_44);
		this.groupInstructions.Controls.Add(this.comboPI_64);
		this.groupInstructions.Controls.Add(this.comboPI_54);
		this.groupInstructions.Controls.Add(this.comboPI_10);
		this.groupInstructions.Controls.Add(this.comboPI_103);
		this.groupInstructions.Controls.Add(this.comboPI_11);
		this.groupInstructions.Controls.Add(this.comboPI_102);
		this.groupInstructions.Controls.Add(this.comboPI_12);
		this.groupInstructions.Controls.Add(this.comboPI_101);
		this.groupInstructions.Controls.Add(this.comboPI_13);
		this.groupInstructions.Controls.Add(this.comboPI_100);
		this.groupInstructions.Controls.Add(this.comboPI_20);
		this.groupInstructions.Controls.Add(this.comboPI_93);
		this.groupInstructions.Controls.Add(this.comboPI_21);
		this.groupInstructions.Controls.Add(this.comboPI_92);
		this.groupInstructions.Controls.Add(this.comboPI_22);
		this.groupInstructions.Controls.Add(this.comboPI_91);
		this.groupInstructions.Controls.Add(this.comboPI_23);
		this.groupInstructions.Controls.Add(this.comboPI_90);
		this.groupInstructions.Controls.Add(this.comboPI_30);
		this.groupInstructions.Controls.Add(this.comboPI_83);
		this.groupInstructions.Controls.Add(this.comboPI_31);
		this.groupInstructions.Controls.Add(this.comboPI_82);
		this.groupInstructions.Controls.Add(this.comboPI_32);
		this.groupInstructions.Controls.Add(this.comboPI_81);
		this.groupInstructions.Controls.Add(this.comboPI_33);
		this.groupInstructions.Controls.Add(this.comboPI_80);
		this.groupInstructions.Controls.Add(this.comboPI_40);
		this.groupInstructions.Controls.Add(this.comboPI_73);
		this.groupInstructions.Controls.Add(this.comboPI_41);
		this.groupInstructions.Controls.Add(this.comboPI_72);
		this.groupInstructions.Controls.Add(this.comboPI_42);
		this.groupInstructions.Controls.Add(this.comboPI_71);
		this.groupInstructions.Controls.Add(this.comboPI_43);
		this.groupInstructions.Controls.Add(this.comboPI_70);
		this.groupInstructions.Controls.Add(this.comboPI_50);
		this.groupInstructions.Controls.Add(this.comboPI_63);
		this.groupInstructions.Controls.Add(this.comboPI_51);
		this.groupInstructions.Controls.Add(this.comboPI_62);
		this.groupInstructions.Controls.Add(this.comboPI_52);
		this.groupInstructions.Controls.Add(this.comboPI_61);
		this.groupInstructions.Controls.Add(this.comboPI_53);
		this.groupInstructions.Controls.Add(this.comboPI_60);
		this.groupInstructions.Location = new System.Drawing.Point(246, 56);
		this.groupInstructions.Name = "groupInstructions";
		this.groupInstructions.Size = new System.Drawing.Size(827, 287);
		this.groupInstructions.TabIndex = 76;
		this.groupInstructions.TabStop = false;
		this.groupInstructions.Text = "Instructions";
		this.comboInterceptions_10.FormattingEnabled = true;
		this.comboInterceptions_10.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_10.Location = new System.Drawing.Point(2, 255);
		this.comboInterceptions_10.Name = "comboInterceptions_10";
		this.comboInterceptions_10.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_10.TabIndex = 95;
		this.comboInterceptions_10.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_6.FormattingEnabled = true;
		this.comboInterceptions_6.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_6.Location = new System.Drawing.Point(2, 155);
		this.comboInterceptions_6.Name = "comboInterceptions_6";
		this.comboInterceptions_6.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_6.TabIndex = 91;
		this.comboInterceptions_6.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_9.FormattingEnabled = true;
		this.comboInterceptions_9.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_9.Location = new System.Drawing.Point(2, 230);
		this.comboInterceptions_9.Name = "comboInterceptions_9";
		this.comboInterceptions_9.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_9.TabIndex = 94;
		this.comboInterceptions_9.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_5.FormattingEnabled = true;
		this.comboInterceptions_5.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_5.Location = new System.Drawing.Point(2, 130);
		this.comboInterceptions_5.Name = "comboInterceptions_5";
		this.comboInterceptions_5.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_5.TabIndex = 90;
		this.comboInterceptions_5.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_8.FormattingEnabled = true;
		this.comboInterceptions_8.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_8.Location = new System.Drawing.Point(2, 205);
		this.comboInterceptions_8.Name = "comboInterceptions_8";
		this.comboInterceptions_8.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_8.TabIndex = 93;
		this.comboInterceptions_8.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_4.FormattingEnabled = true;
		this.comboInterceptions_4.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_4.Location = new System.Drawing.Point(2, 105);
		this.comboInterceptions_4.Name = "comboInterceptions_4";
		this.comboInterceptions_4.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_4.TabIndex = 89;
		this.comboInterceptions_4.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_7.FormattingEnabled = true;
		this.comboInterceptions_7.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_7.Location = new System.Drawing.Point(2, 180);
		this.comboInterceptions_7.Name = "comboInterceptions_7";
		this.comboInterceptions_7.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_7.TabIndex = 92;
		this.comboInterceptions_7.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_3.FormattingEnabled = true;
		this.comboInterceptions_3.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_3.Location = new System.Drawing.Point(2, 80);
		this.comboInterceptions_3.Name = "comboInterceptions_3";
		this.comboInterceptions_3.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_3.TabIndex = 88;
		this.comboInterceptions_3.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboInterceptions_2.FormattingEnabled = true;
		this.comboInterceptions_2.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_2.Location = new System.Drawing.Point(2, 55);
		this.comboInterceptions_2.Name = "comboInterceptions_2";
		this.comboInterceptions_2.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_2.TabIndex = 87;
		this.comboInterceptions_2.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(10, 15);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(68, 13);
		this.label15.TabIndex = 86;
		this.label15.Text = "Interceptions";
		this.comboInterceptions_1.FormattingEnabled = true;
		this.comboInterceptions_1.Items.AddRange(new object[3] { "Conservative", "Normal", "Aggressive" });
		this.comboInterceptions_1.Location = new System.Drawing.Point(2, 30);
		this.comboInterceptions_1.Name = "comboInterceptions_1";
		this.comboInterceptions_1.Size = new System.Drawing.Size(88, 21);
		this.comboInterceptions_1.TabIndex = 79;
		this.comboInterceptions_1.SelectedIndexChanged += new System.EventHandler(comboInterceptions_SelectedIndexChanged);
		this.comboPI_104.FormattingEnabled = true;
		this.comboPI_104.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_104.Location = new System.Drawing.Point(752, 255);
		this.comboPI_104.Name = "comboPI_104";
		this.comboPI_104.Size = new System.Drawing.Size(160, 21);
		this.comboPI_104.TabIndex = 85;
		this.comboPI_14.FormattingEnabled = true;
		this.comboPI_14.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_14.Location = new System.Drawing.Point(752, 30);
		this.comboPI_14.Name = "comboPI_14";
		this.comboPI_14.Size = new System.Drawing.Size(160, 21);
		this.comboPI_14.TabIndex = 76;
		this.comboPI_94.FormattingEnabled = true;
		this.comboPI_94.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_94.Location = new System.Drawing.Point(752, 230);
		this.comboPI_94.Name = "comboPI_94";
		this.comboPI_94.Size = new System.Drawing.Size(160, 21);
		this.comboPI_94.TabIndex = 84;
		this.comboPI_24.FormattingEnabled = true;
		this.comboPI_24.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_24.Location = new System.Drawing.Point(752, 55);
		this.comboPI_24.Name = "comboPI_24";
		this.comboPI_24.Size = new System.Drawing.Size(160, 21);
		this.comboPI_24.TabIndex = 77;
		this.comboPI_84.FormattingEnabled = true;
		this.comboPI_84.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_84.Location = new System.Drawing.Point(752, 205);
		this.comboPI_84.Name = "comboPI_84";
		this.comboPI_84.Size = new System.Drawing.Size(160, 21);
		this.comboPI_84.TabIndex = 83;
		this.comboPI_34.FormattingEnabled = true;
		this.comboPI_34.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_34.Location = new System.Drawing.Point(752, 80);
		this.comboPI_34.Name = "comboPI_34";
		this.comboPI_34.Size = new System.Drawing.Size(160, 21);
		this.comboPI_34.TabIndex = 78;
		this.comboPI_74.FormattingEnabled = true;
		this.comboPI_74.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_74.Location = new System.Drawing.Point(752, 180);
		this.comboPI_74.Name = "comboPI_74";
		this.comboPI_74.Size = new System.Drawing.Size(160, 21);
		this.comboPI_74.TabIndex = 82;
		this.comboPI_44.FormattingEnabled = true;
		this.comboPI_44.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_44.Location = new System.Drawing.Point(752, 105);
		this.comboPI_44.Name = "comboPI_44";
		this.comboPI_44.Size = new System.Drawing.Size(160, 21);
		this.comboPI_44.TabIndex = 79;
		this.comboPI_64.FormattingEnabled = true;
		this.comboPI_64.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_64.Location = new System.Drawing.Point(752, 155);
		this.comboPI_64.Name = "comboPI_64";
		this.comboPI_64.Size = new System.Drawing.Size(160, 21);
		this.comboPI_64.TabIndex = 81;
		this.comboPI_54.FormattingEnabled = true;
		this.comboPI_54.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_54.Location = new System.Drawing.Point(752, 130);
		this.comboPI_54.Name = "comboPI_54";
		this.comboPI_54.Size = new System.Drawing.Size(160, 21);
		this.comboPI_54.TabIndex = 80;
		this.comboPI_10.FormattingEnabled = true;
		this.comboPI_10.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_10.Location = new System.Drawing.Point(93, 30);
		this.comboPI_10.Name = "comboPI_10";
		this.comboPI_10.Size = new System.Drawing.Size(160, 21);
		this.comboPI_10.TabIndex = 36;
		this.comboPI_10.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_103.FormattingEnabled = true;
		this.comboPI_103.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_103.Location = new System.Drawing.Point(587, 255);
		this.comboPI_103.Name = "comboPI_103";
		this.comboPI_103.Size = new System.Drawing.Size(160, 21);
		this.comboPI_103.TabIndex = 75;
		this.comboPI_103.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_11.FormattingEnabled = true;
		this.comboPI_11.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_11.Location = new System.Drawing.Point(257, 30);
		this.comboPI_11.Name = "comboPI_11";
		this.comboPI_11.Size = new System.Drawing.Size(160, 21);
		this.comboPI_11.TabIndex = 37;
		this.comboPI_11.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_102.FormattingEnabled = true;
		this.comboPI_102.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_102.Location = new System.Drawing.Point(421, 255);
		this.comboPI_102.Name = "comboPI_102";
		this.comboPI_102.Size = new System.Drawing.Size(160, 21);
		this.comboPI_102.TabIndex = 74;
		this.comboPI_102.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_12.FormattingEnabled = true;
		this.comboPI_12.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_12.Location = new System.Drawing.Point(421, 30);
		this.comboPI_12.Name = "comboPI_12";
		this.comboPI_12.Size = new System.Drawing.Size(160, 21);
		this.comboPI_12.TabIndex = 38;
		this.comboPI_12.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_101.FormattingEnabled = true;
		this.comboPI_101.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_101.Location = new System.Drawing.Point(257, 255);
		this.comboPI_101.Name = "comboPI_101";
		this.comboPI_101.Size = new System.Drawing.Size(160, 21);
		this.comboPI_101.TabIndex = 73;
		this.comboPI_101.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_13.FormattingEnabled = true;
		this.comboPI_13.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_13.Location = new System.Drawing.Point(587, 30);
		this.comboPI_13.Name = "comboPI_13";
		this.comboPI_13.Size = new System.Drawing.Size(160, 21);
		this.comboPI_13.TabIndex = 39;
		this.comboPI_13.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_100.FormattingEnabled = true;
		this.comboPI_100.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_100.Location = new System.Drawing.Point(93, 255);
		this.comboPI_100.Name = "comboPI_100";
		this.comboPI_100.Size = new System.Drawing.Size(160, 21);
		this.comboPI_100.TabIndex = 72;
		this.comboPI_100.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_20.FormattingEnabled = true;
		this.comboPI_20.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_20.Location = new System.Drawing.Point(93, 55);
		this.comboPI_20.Name = "comboPI_20";
		this.comboPI_20.Size = new System.Drawing.Size(160, 21);
		this.comboPI_20.TabIndex = 40;
		this.comboPI_20.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_93.FormattingEnabled = true;
		this.comboPI_93.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_93.Location = new System.Drawing.Point(587, 230);
		this.comboPI_93.Name = "comboPI_93";
		this.comboPI_93.Size = new System.Drawing.Size(160, 21);
		this.comboPI_93.TabIndex = 71;
		this.comboPI_93.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_21.FormattingEnabled = true;
		this.comboPI_21.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_21.Location = new System.Drawing.Point(257, 55);
		this.comboPI_21.Name = "comboPI_21";
		this.comboPI_21.Size = new System.Drawing.Size(160, 21);
		this.comboPI_21.TabIndex = 41;
		this.comboPI_21.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_92.FormattingEnabled = true;
		this.comboPI_92.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_92.Location = new System.Drawing.Point(421, 230);
		this.comboPI_92.Name = "comboPI_92";
		this.comboPI_92.Size = new System.Drawing.Size(160, 21);
		this.comboPI_92.TabIndex = 70;
		this.comboPI_92.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_22.FormattingEnabled = true;
		this.comboPI_22.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_22.Location = new System.Drawing.Point(421, 55);
		this.comboPI_22.Name = "comboPI_22";
		this.comboPI_22.Size = new System.Drawing.Size(160, 21);
		this.comboPI_22.TabIndex = 42;
		this.comboPI_22.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_91.FormattingEnabled = true;
		this.comboPI_91.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_91.Location = new System.Drawing.Point(257, 230);
		this.comboPI_91.Name = "comboPI_91";
		this.comboPI_91.Size = new System.Drawing.Size(160, 21);
		this.comboPI_91.TabIndex = 69;
		this.comboPI_91.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_23.FormattingEnabled = true;
		this.comboPI_23.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_23.Location = new System.Drawing.Point(587, 55);
		this.comboPI_23.Name = "comboPI_23";
		this.comboPI_23.Size = new System.Drawing.Size(160, 21);
		this.comboPI_23.TabIndex = 43;
		this.comboPI_23.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_90.FormattingEnabled = true;
		this.comboPI_90.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_90.Location = new System.Drawing.Point(93, 230);
		this.comboPI_90.Name = "comboPI_90";
		this.comboPI_90.Size = new System.Drawing.Size(160, 21);
		this.comboPI_90.TabIndex = 68;
		this.comboPI_90.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_30.FormattingEnabled = true;
		this.comboPI_30.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_30.Location = new System.Drawing.Point(93, 80);
		this.comboPI_30.Name = "comboPI_30";
		this.comboPI_30.Size = new System.Drawing.Size(160, 21);
		this.comboPI_30.TabIndex = 44;
		this.comboPI_30.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_83.FormattingEnabled = true;
		this.comboPI_83.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_83.Location = new System.Drawing.Point(587, 205);
		this.comboPI_83.Name = "comboPI_83";
		this.comboPI_83.Size = new System.Drawing.Size(160, 21);
		this.comboPI_83.TabIndex = 67;
		this.comboPI_83.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_31.FormattingEnabled = true;
		this.comboPI_31.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_31.Location = new System.Drawing.Point(257, 80);
		this.comboPI_31.Name = "comboPI_31";
		this.comboPI_31.Size = new System.Drawing.Size(160, 21);
		this.comboPI_31.TabIndex = 45;
		this.comboPI_31.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_82.FormattingEnabled = true;
		this.comboPI_82.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_82.Location = new System.Drawing.Point(421, 205);
		this.comboPI_82.Name = "comboPI_82";
		this.comboPI_82.Size = new System.Drawing.Size(160, 21);
		this.comboPI_82.TabIndex = 66;
		this.comboPI_82.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_32.FormattingEnabled = true;
		this.comboPI_32.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_32.Location = new System.Drawing.Point(421, 80);
		this.comboPI_32.Name = "comboPI_32";
		this.comboPI_32.Size = new System.Drawing.Size(160, 21);
		this.comboPI_32.TabIndex = 46;
		this.comboPI_32.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_81.FormattingEnabled = true;
		this.comboPI_81.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_81.Location = new System.Drawing.Point(257, 205);
		this.comboPI_81.Name = "comboPI_81";
		this.comboPI_81.Size = new System.Drawing.Size(160, 21);
		this.comboPI_81.TabIndex = 65;
		this.comboPI_81.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_33.FormattingEnabled = true;
		this.comboPI_33.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_33.Location = new System.Drawing.Point(587, 80);
		this.comboPI_33.Name = "comboPI_33";
		this.comboPI_33.Size = new System.Drawing.Size(160, 21);
		this.comboPI_33.TabIndex = 47;
		this.comboPI_33.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_80.FormattingEnabled = true;
		this.comboPI_80.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_80.Location = new System.Drawing.Point(93, 205);
		this.comboPI_80.Name = "comboPI_80";
		this.comboPI_80.Size = new System.Drawing.Size(160, 21);
		this.comboPI_80.TabIndex = 64;
		this.comboPI_80.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_40.FormattingEnabled = true;
		this.comboPI_40.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_40.Location = new System.Drawing.Point(93, 105);
		this.comboPI_40.Name = "comboPI_40";
		this.comboPI_40.Size = new System.Drawing.Size(160, 21);
		this.comboPI_40.TabIndex = 48;
		this.comboPI_40.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_73.FormattingEnabled = true;
		this.comboPI_73.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_73.Location = new System.Drawing.Point(587, 180);
		this.comboPI_73.Name = "comboPI_73";
		this.comboPI_73.Size = new System.Drawing.Size(160, 21);
		this.comboPI_73.TabIndex = 63;
		this.comboPI_73.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_41.FormattingEnabled = true;
		this.comboPI_41.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_41.Location = new System.Drawing.Point(257, 105);
		this.comboPI_41.Name = "comboPI_41";
		this.comboPI_41.Size = new System.Drawing.Size(160, 21);
		this.comboPI_41.TabIndex = 49;
		this.comboPI_41.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_72.FormattingEnabled = true;
		this.comboPI_72.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_72.Location = new System.Drawing.Point(421, 180);
		this.comboPI_72.Name = "comboPI_72";
		this.comboPI_72.Size = new System.Drawing.Size(160, 21);
		this.comboPI_72.TabIndex = 62;
		this.comboPI_72.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_42.FormattingEnabled = true;
		this.comboPI_42.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_42.Location = new System.Drawing.Point(421, 105);
		this.comboPI_42.Name = "comboPI_42";
		this.comboPI_42.Size = new System.Drawing.Size(160, 21);
		this.comboPI_42.TabIndex = 50;
		this.comboPI_42.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_71.FormattingEnabled = true;
		this.comboPI_71.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_71.Location = new System.Drawing.Point(257, 180);
		this.comboPI_71.Name = "comboPI_71";
		this.comboPI_71.Size = new System.Drawing.Size(160, 21);
		this.comboPI_71.TabIndex = 61;
		this.comboPI_71.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_43.FormattingEnabled = true;
		this.comboPI_43.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_43.Location = new System.Drawing.Point(587, 105);
		this.comboPI_43.Name = "comboPI_43";
		this.comboPI_43.Size = new System.Drawing.Size(160, 21);
		this.comboPI_43.TabIndex = 51;
		this.comboPI_43.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_70.FormattingEnabled = true;
		this.comboPI_70.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_70.Location = new System.Drawing.Point(93, 180);
		this.comboPI_70.Name = "comboPI_70";
		this.comboPI_70.Size = new System.Drawing.Size(160, 21);
		this.comboPI_70.TabIndex = 60;
		this.comboPI_70.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_50.FormattingEnabled = true;
		this.comboPI_50.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_50.Location = new System.Drawing.Point(93, 130);
		this.comboPI_50.Name = "comboPI_50";
		this.comboPI_50.Size = new System.Drawing.Size(160, 21);
		this.comboPI_50.TabIndex = 52;
		this.comboPI_50.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_63.FormattingEnabled = true;
		this.comboPI_63.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_63.Location = new System.Drawing.Point(587, 155);
		this.comboPI_63.Name = "comboPI_63";
		this.comboPI_63.Size = new System.Drawing.Size(160, 21);
		this.comboPI_63.TabIndex = 59;
		this.comboPI_63.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_51.FormattingEnabled = true;
		this.comboPI_51.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_51.Location = new System.Drawing.Point(257, 130);
		this.comboPI_51.Name = "comboPI_51";
		this.comboPI_51.Size = new System.Drawing.Size(160, 21);
		this.comboPI_51.TabIndex = 53;
		this.comboPI_51.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_62.FormattingEnabled = true;
		this.comboPI_62.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_62.Location = new System.Drawing.Point(421, 155);
		this.comboPI_62.Name = "comboPI_62";
		this.comboPI_62.Size = new System.Drawing.Size(160, 21);
		this.comboPI_62.TabIndex = 58;
		this.comboPI_62.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_52.FormattingEnabled = true;
		this.comboPI_52.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_52.Location = new System.Drawing.Point(421, 130);
		this.comboPI_52.Name = "comboPI_52";
		this.comboPI_52.Size = new System.Drawing.Size(160, 21);
		this.comboPI_52.TabIndex = 54;
		this.comboPI_52.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_61.FormattingEnabled = true;
		this.comboPI_61.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_61.Location = new System.Drawing.Point(257, 155);
		this.comboPI_61.Name = "comboPI_61";
		this.comboPI_61.Size = new System.Drawing.Size(160, 21);
		this.comboPI_61.TabIndex = 57;
		this.comboPI_61.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_53.FormattingEnabled = true;
		this.comboPI_53.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_53.Location = new System.Drawing.Point(587, 130);
		this.comboPI_53.Name = "comboPI_53";
		this.comboPI_53.Size = new System.Drawing.Size(160, 21);
		this.comboPI_53.TabIndex = 55;
		this.comboPI_53.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.comboPI_60.FormattingEnabled = true;
		this.comboPI_60.Items.AddRange(new object[1] { "Stay On Edge Of Box For Cross" });
		this.comboPI_60.Location = new System.Drawing.Point(93, 155);
		this.comboPI_60.Name = "comboPI_60";
		this.comboPI_60.Size = new System.Drawing.Size(160, 21);
		this.comboPI_60.TabIndex = 56;
		this.comboPI_60.SelectedIndexChanged += new System.EventHandler(comboInstruction_SelectedIndexChanged);
		this.checkIsSweeper.AutoSize = true;
		this.checkIsSweeper.Location = new System.Drawing.Point(486, 415);
		this.checkIsSweeper.Name = "checkIsSweeper";
		this.checkIsSweeper.Size = new System.Drawing.Size(90, 17);
		this.checkIsSweeper.TabIndex = 30;
		this.checkIsSweeper.Text = "Has Sweeper";
		this.checkIsSweeper.UseVisualStyleBackColor = true;
		this.checkIsSweeper.Visible = false;
		this.checkIsSweeper.CheckedChanged += new System.EventHandler(checkIsSweeper_CheckedChanged);
		this.comboOffensiveRating.FormattingEnabled = true;
		this.comboOffensiveRating.Items.AddRange(new object[5] { "Very Defensive", "Defensive", "Neutral", "Offensive", "Very Offensive" });
		this.comboOffensiveRating.Location = new System.Drawing.Point(110, 392);
		this.comboOffensiveRating.Name = "comboOffensiveRating";
		this.comboOffensiveRating.Size = new System.Drawing.Size(130, 21);
		this.comboOffensiveRating.TabIndex = 35;
		this.comboOffensiveRating.SelectedIndexChanged += new System.EventHandler(comboOffensiveRating_SelectedIndexChanged);
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(13, 396);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(86, 13);
		this.label14.TabIndex = 34;
		this.label14.Text = "Offensive Rating";
		this.buttonPresetFormation.Location = new System.Drawing.Point(191, 439);
		this.buttonPresetFormation.Name = "buttonPresetFormation";
		this.buttonPresetFormation.Size = new System.Drawing.Size(47, 23);
		this.buttonPresetFormation.TabIndex = 29;
		this.buttonPresetFormation.Text = "Copy";
		this.buttonPresetFormation.UseVisualStyleBackColor = true;
		this.buttonPresetFormation.Click += new System.EventHandler(buttonPresetFormation_Click);
		this.comboFormation.FormattingEnabled = true;
		this.comboFormation.Location = new System.Drawing.Point(55, 441);
		this.comboFormation.Name = "comboFormation";
		this.comboFormation.Size = new System.Drawing.Size(130, 21);
		this.comboFormation.TabIndex = 28;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(12, 446);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(37, 13);
		this.label12.TabIndex = 27;
		this.label12.Text = "Preset";
		this.labelAssignTeam.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelAssignTeam.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelAssignTeam.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelAssignTeam.Location = new System.Drawing.Point(6, 13);
		this.labelAssignTeam.Name = "labelAssignTeam";
		this.labelAssignTeam.Size = new System.Drawing.Size(232, 19);
		this.labelAssignTeam.TabIndex = 26;
		this.labelAssignTeam.Text = "Formation";
		this.labelAssignTeam.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelAssignTeam.DoubleClick += new System.EventHandler(labelAssignTeam_DoubleClick);
		this.textName.Location = new System.Drawing.Point(71, 35);
		this.textName.Name = "textName";
		this.textName.Size = new System.Drawing.Size(169, 20);
		this.textName.TabIndex = 25;
		this.textName.TextChanged += new System.EventHandler(textName_TextChanged);
		this.labelName.AutoSize = true;
		this.labelName.Location = new System.Drawing.Point(12, 38);
		this.labelName.Name = "labelName";
		this.labelName.Size = new System.Drawing.Size(53, 13);
		this.labelName.TabIndex = 24;
		this.labelName.Text = "DB Name";
		this.label9.Location = new System.Drawing.Point(5, 311);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(20, 18);
		this.label9.TabIndex = 23;
		this.label9.Text = "11";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label10.Location = new System.Drawing.Point(5, 286);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(20, 18);
		this.label10.TabIndex = 22;
		this.label10.Text = "10";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label11.Location = new System.Drawing.Point(5, 261);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(20, 18);
		this.label11.TabIndex = 21;
		this.label11.Text = "9";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label5.Location = new System.Drawing.Point(5, 236);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(20, 18);
		this.label5.TabIndex = 20;
		this.label5.Text = "8";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label6.Location = new System.Drawing.Point(5, 211);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(20, 18);
		this.label6.TabIndex = 19;
		this.label6.Text = "7";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label7.Location = new System.Drawing.Point(5, 186);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(20, 18);
		this.label7.TabIndex = 18;
		this.label7.Text = "6";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label8.Location = new System.Drawing.Point(5, 161);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(20, 18);
		this.label8.TabIndex = 17;
		this.label8.Text = "5";
		this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label3.Location = new System.Drawing.Point(5, 136);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(20, 18);
		this.label3.TabIndex = 16;
		this.label3.Text = "4";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label4.Location = new System.Drawing.Point(5, 111);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(20, 18);
		this.label4.TabIndex = 15;
		this.label4.Text = "3";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label2.Location = new System.Drawing.Point(5, 86);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(20, 18);
		this.label2.TabIndex = 14;
		this.label2.Text = "2";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label1.Location = new System.Drawing.Point(5, 61);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(20, 18);
		this.label1.TabIndex = 13;
		this.label1.Text = "1";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.comboBox7.FormattingEnabled = true;
		this.comboBox7.Location = new System.Drawing.Point(31, 211);
		this.comboBox7.Name = "comboBox7";
		this.comboBox7.Size = new System.Drawing.Size(209, 21);
		this.comboBox7.TabIndex = 6;
		this.comboBox7.SelectedIndexChanged += new System.EventHandler(comboBox7_SelectedIndexChanged);
		this.comboBox1.BackColor = System.Drawing.Color.White;
		this.comboBox1.Enabled = false;
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Location = new System.Drawing.Point(31, 61);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(209, 21);
		this.comboBox1.TabIndex = 0;
		this.comboBox8.FormattingEnabled = true;
		this.comboBox8.Location = new System.Drawing.Point(31, 236);
		this.comboBox8.Name = "comboBox8";
		this.comboBox8.Size = new System.Drawing.Size(209, 21);
		this.comboBox8.TabIndex = 7;
		this.comboBox8.SelectedIndexChanged += new System.EventHandler(comboBox8_SelectedIndexChanged);
		this.comboBox2.FormattingEnabled = true;
		this.comboBox2.Location = new System.Drawing.Point(31, 86);
		this.comboBox2.Name = "comboBox2";
		this.comboBox2.Size = new System.Drawing.Size(209, 21);
		this.comboBox2.TabIndex = 1;
		this.comboBox2.SelectedIndexChanged += new System.EventHandler(comboBox2_SelectedIndexChanged);
		this.comboBox9.FormattingEnabled = true;
		this.comboBox9.Location = new System.Drawing.Point(31, 261);
		this.comboBox9.Name = "comboBox9";
		this.comboBox9.Size = new System.Drawing.Size(209, 21);
		this.comboBox9.TabIndex = 8;
		this.comboBox9.SelectedIndexChanged += new System.EventHandler(comboBox9_SelectedIndexChanged);
		this.comboBox3.FormattingEnabled = true;
		this.comboBox3.Location = new System.Drawing.Point(31, 111);
		this.comboBox3.Name = "comboBox3";
		this.comboBox3.Size = new System.Drawing.Size(209, 21);
		this.comboBox3.TabIndex = 2;
		this.comboBox3.SelectedIndexChanged += new System.EventHandler(comboBox3_SelectedIndexChanged);
		this.comboBox10.FormattingEnabled = true;
		this.comboBox10.Location = new System.Drawing.Point(31, 286);
		this.comboBox10.Name = "comboBox10";
		this.comboBox10.Size = new System.Drawing.Size(209, 21);
		this.comboBox10.TabIndex = 9;
		this.comboBox10.SelectedIndexChanged += new System.EventHandler(comboBox10_SelectedIndexChanged);
		this.comboBox6.FormattingEnabled = true;
		this.comboBox6.Location = new System.Drawing.Point(31, 186);
		this.comboBox6.Name = "comboBox6";
		this.comboBox6.Size = new System.Drawing.Size(209, 21);
		this.comboBox6.TabIndex = 5;
		this.comboBox6.SelectedIndexChanged += new System.EventHandler(comboBox6_SelectedIndexChanged);
		this.comboBox11.FormattingEnabled = true;
		this.comboBox11.Location = new System.Drawing.Point(31, 311);
		this.comboBox11.Name = "comboBox11";
		this.comboBox11.Size = new System.Drawing.Size(209, 21);
		this.comboBox11.TabIndex = 10;
		this.comboBox11.SelectedIndexChanged += new System.EventHandler(comboBox11_SelectedIndexChanged);
		this.comboBox5.FormattingEnabled = true;
		this.comboBox5.Location = new System.Drawing.Point(31, 161);
		this.comboBox5.Name = "comboBox5";
		this.comboBox5.Size = new System.Drawing.Size(209, 21);
		this.comboBox5.TabIndex = 4;
		this.comboBox5.SelectedIndexChanged += new System.EventHandler(comboBox5_SelectedIndexChanged);
		this.comboBox4.FormattingEnabled = true;
		this.comboBox4.Location = new System.Drawing.Point(31, 136);
		this.comboBox4.Name = "comboBox4";
		this.comboBox4.Size = new System.Drawing.Size(209, 21);
		this.comboBox4.TabIndex = 3;
		this.comboBox4.SelectedIndexChanged += new System.EventHandler(comboBox4_SelectedIndexChanged);
		this.tabFormation.Controls.Add(this.pagePosition);
		this.tabFormation.Location = new System.Drawing.Point(0, 0);
		this.tabFormation.Name = "tabFormation";
		this.tabFormation.SelectedIndex = 0;
		this.tabFormation.Size = new System.Drawing.Size(263, 376);
		this.tabFormation.TabIndex = 9;
		this.pagePosition.BackgroundImage = (System.Drawing.Image)resources.GetObject("pagePosition.BackgroundImage");
		this.pagePosition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.pagePosition.Location = new System.Drawing.Point(4, 22);
		this.pagePosition.Name = "pagePosition";
		this.pagePosition.Padding = new System.Windows.Forms.Padding(3);
		this.pagePosition.Size = new System.Drawing.Size(255, 350);
		this.pagePosition.TabIndex = 0;
		this.pagePosition.Text = "Position";
		this.pagePosition.UseVisualStyleBackColor = true;
		this.imageListPlayers.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListPlayers.ImageStream");
		this.imageListPlayers.TransparentColor = System.Drawing.Color.Fuchsia;
		this.imageListPlayers.Images.SetKeyName(0, "P1.png");
		this.imageListPlayers.Images.SetKeyName(1, "P2.png");
		this.imageListPlayers.Images.SetKeyName(2, "P3.png");
		this.imageListPlayers.Images.SetKeyName(3, "p4.png");
		this.imageListPlayers.Images.SetKeyName(4, "p5.png");
		this.imageListPlayers.Images.SetKeyName(5, "p6.png");
		this.imageListPlayers.Images.SetKeyName(6, "p7.png");
		this.imageListPlayers.Images.SetKeyName(7, "p8.png");
		this.imageListPlayers.Images.SetKeyName(8, "P9.png");
		this.imageListPlayers.Images.SetKeyName(9, "P10.png");
		this.imageListPlayers.Images.SetKeyName(10, "p11.png");
		this.imageListPlayers.Images.SetKeyName(11, "Pnull.png");
		this.imageListArrows.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListArrows.ImageStream");
		this.imageListArrows.TransparentColor = System.Drawing.Color.Fuchsia;
		this.imageListArrows.Images.SetKeyName(0, "Move0Yellow.PNG");
		this.imageListArrows.Images.SetKeyName(1, "Move1Yellow.PNG");
		this.imageListArrows.Images.SetKeyName(2, "Move2tYellow.PNG");
		this.imageListArrows.Images.SetKeyName(3, "Move3Yellow.PNG");
		this.imageListArrows.Images.SetKeyName(4, "Move4Yellow.PNG");
		this.imageListArrows.Images.SetKeyName(5, "Move5Yellow.PNG");
		this.imageListArrows.Images.SetKeyName(6, "Move6Yellow.PNG");
		this.imageListArrows.Images.SetKeyName(7, "Move7Yellow.PNG");
		this.imageListArrows.Images.SetKeyName(8, "Move8tYellow.PNG");
		this.imageListArrows.Images.SetKeyName(9, "Move0Red.PNG");
		this.imageListArrows.Images.SetKeyName(10, "Move1Red.PNG");
		this.imageListArrows.Images.SetKeyName(11, "Move2Red.PNG");
		this.imageListArrows.Images.SetKeyName(12, "Move3Red.PNG");
		this.imageListArrows.Images.SetKeyName(13, "Move4Red.PNG");
		this.imageListArrows.Images.SetKeyName(14, "Move5Red.PNG");
		this.imageListArrows.Images.SetKeyName(15, "Move6Red.PNG");
		this.imageListArrows.Images.SetKeyName(16, "Move7Red.PNG");
		this.imageListArrows.Images.SetKeyName(17, "Move8Red.PNG");
		this.imageListArrows.Images.SetKeyName(18, "Move0Blue.PNG");
		this.imageListArrows.Images.SetKeyName(19, "Move1Blue.PNG");
		this.imageListArrows.Images.SetKeyName(20, "Move2Blue.PNG");
		this.imageListArrows.Images.SetKeyName(21, "Move3Blue.PNG");
		this.imageListArrows.Images.SetKeyName(22, "Move4Blue.PNG");
		this.imageListArrows.Images.SetKeyName(23, "Move5Blue.PNG");
		this.imageListArrows.Images.SetKeyName(24, "Move6Blue.PNG");
		this.imageListArrows.Images.SetKeyName(25, "Move7Blue.PNG");
		this.imageListArrows.Images.SetKeyName(26, "Move8Blue.PNG");
		this.imageListArrows.Images.SetKeyName(27, "Move0White.PNG");
		this.imageListArrows.Images.SetKeyName(28, "Move1White.PNG");
		this.imageListArrows.Images.SetKeyName(29, "Move2White.PNG");
		this.imageListArrows.Images.SetKeyName(30, "Move3White.PNG");
		this.imageListArrows.Images.SetKeyName(31, "Move4White.PNG");
		this.imageListArrows.Images.SetKeyName(32, "Move5White.PNG");
		this.imageListArrows.Images.SetKeyName(33, "Move6White.PNG");
		this.imageListArrows.Images.SetKeyName(34, "Move7White.PNG");
		this.imageListArrows.Images.SetKeyName(35, "Move8White.PNG");
		this.panelFormation.AutoScroll = true;
		this.panelFormation.Controls.Add(this.groupTactic);
		this.panelFormation.Controls.Add(this.tabFormation);
		this.panelFormation.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelFormation.Location = new System.Drawing.Point(0, 25);
		this.panelFormation.Name = "panelFormation";
		this.panelFormation.Size = new System.Drawing.Size(1343, 750);
		this.panelFormation.TabIndex = 10;
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = true;
		this.pickUpControl.CreateButtonEnabled = true;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = new string[4] { "All", "by League", "by Country", "by Team" };
		this.pickUpControl.FilterEnabled = false;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = true;
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = true;
		this.pickUpControl.SearchEnabled = true;
		this.pickUpControl.Size = new System.Drawing.Size(1343, 25);
		this.pickUpControl.TabIndex = 1;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		this.teamBindingSource.DataSource = typeof(FifaLibrary.Team);
		this.teamListBindingSource.DataSource = typeof(FifaLibrary.TeamList);
		this.numericFullName.Location = new System.Drawing.Point(382, 35);
		this.numericFullName.Maximum = new decimal(new int[4] { 30, 0, 0, 0 });
		this.numericFullName.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericFullName.Name = "numericFullName";
		this.numericFullName.Size = new System.Drawing.Size(73, 20);
		this.numericFullName.TabIndex = 81;
		this.numericFullName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericFullName.ValueChanged += new System.EventHandler(numericFullName_ValueChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1343, 775);
		base.Controls.Add(this.panelFormation);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "FormationForm";
		this.Text = "FormationForm";
		base.Load += new System.EventHandler(FormationForm_Load);
		this.groupTactic.ResumeLayout(false);
		this.groupTactic.PerformLayout();
		this.groupInstructions.ResumeLayout(false);
		this.groupInstructions.PerformLayout();
		this.tabFormation.ResumeLayout(false);
		this.panelFormation.ResumeLayout(false);
		this.panelFormation.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.teamBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericFullName).EndInit();
		base.ResumeLayout(false);
	}
}
