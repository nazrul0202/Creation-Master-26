using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class RefereeForm : Form
{
	private Referee m_CurrentReferee;

	private string m_RefereeCurrentFolder = FifaEnvironment.ExportFolder;

	private string m_NotPresent = "< None >";

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private bool m_GenericAppearanceSema = true;

	private bool m_Locked;

	private int m_HairAlfaChannel = 1;

	private Viewer3D viewer3DReferee;

	private IContainer components;

	public PickUpControl pickUpControl;

	private SplitContainer splitContainer1;

	private FlowLayoutPanel flowLayoutPanel1;

	private SplitContainer splitContainer2;

	private GroupBox groupIdentity;

	private Button buttonGetId;

	private NumericUpDown numericRefereeId;

	private Button buttonRandomizeIdentity;

	private DateTimePicker dateBirthDate;

	private Label labelBirthdate;

	private Label labelRefereeId;

	private TextBox textSurname;

	private TextBox textFirstName;

	private ComboBox comboCountry;

	private Label labelFirstName;

	private Label labelSurame;

	private Label labelCountry;

	private BindingSource refereeBindingSource;

	private BindingSource countryListBindingSource;

	private ComboBox comboBody;

	private NumericUpDown numericHeight;

	private NumericUpDown numericWeight;

	private Label labelWeight;

	private Label labelBody;

	private Label labelHeight;

	private DomainUpDown domainSleeves;

	private Label labelSleeves;

	private ComboBox comboLeague0;

	private ComboBox comboStyle;

	private Label labelStyle;

	private ToolStrip tool3D;

	private ToolStripButton buttonShow3DModel;

	private ToolStripSeparator toolStripSeparator1;

	private GroupBox groupGenericFace;

	private GroupBox groupTextureInfo;

	private ComboBox comboSkinColor;

	private Label labelFacialHair;

	private Label labelEyeBow;

	private ComboBox domainFacialHair;

	private ComboBox comboEyeBow;

	private Label labelSideburns;

	private ComboBox comboSideburns;

	private Label labelSkintype;

	private ComboBox comboEyescolor;

	private ComboBox comboSkintype;

	private Label label2;

	private Label label1;

	private ComboBox comboFacialHairColor;

	private Label labelFacialHairColor;

	private GroupBox groupHairModel;

	private ComboBox comboHeadband;

	private ComboBox comboAfro;

	private ComboBox comboLong;

	private ComboBox comboMedium;

	private ComboBox comboModern;

	private ComboBox comboShort;

	private ComboBox comboVeryShort;

	private ComboBox comboShaven;

	private RadioButton radioHeadband;

	private RadioButton radioShaven;

	private RadioButton radioAfro;

	private RadioButton radioLong;

	private RadioButton radioMedium;

	private RadioButton radioModern;

	private RadioButton radioShort;

	private RadioButton radioVeryShort;

	private ComboBox domainHairColor;

	private Label labelHairColor;

	private GroupBox groupHeadModel;

	private ComboBox comboLatinModels;

	private RadioButton radioButtonLatin;

	private ComboBox comboAsiaticModels;

	private RadioButton radioButtonAsiatic;

	private ComboBox comboAfricanModels;

	private RadioButton radioButtonAfrican;

	private RadioButton radioButtonCaucasic;

	private ComboBox comboCaucasicModels;

	private Button buttonRandomizeAppearance;

	private Label labelHeadType;

	private Label labelHairType;

	private ToolStripButton buttonSwitchRenderingMode;

	private ComboBox comboBox1;

	private Label label3;

	private ComboBox comboLeague1;

	private ComboBox comboLeague2;

	private ComboBox comboLeague3;

	private GroupBox groupLeagues;

	private ComboBox comboLeague4;

	private ComboBox comboLeague7;

	private ComboBox comboLeague5;

	private ComboBox comboLeague6;

	private Viewer2D viewer2DPlayerGui;

	private ToolStripButton toolPhoto;

	private GroupBox groupShoes;

	private Label label1ShoesType;

	private PictureBox pictureColorShoes2;

	private PictureBox pictureColorShoes1;

	public NumericUpDown numericShoesBrand;

	private Label labelShoesType;

	private Label labelShoesColor;

	public NumericUpDown numericShoesDesign;

	private Viewer2D viewer2DShoes;

	private Label labelShoes;

	private RadioButton radioButtonGenderFemale;

	private RadioButton radioButtonGenderMale;

	private Button buttonRandomizeAllReferees;

	public RefereeForm()
	{
		InitializeComponent();
		viewer3DReferee = new Viewer3D();
		viewer3DReferee.AmbientColor = Color.Gray;
		viewer3DReferee.BackColor = Color.Gray;
		viewer3DReferee.BorderStyle = BorderStyle.Fixed3D;
		viewer3DReferee.Dock = DockStyle.Fill;
		viewer3DReferee.LightDirectionX = -0.5f;
		viewer3DReferee.LightDirectionY = -0.25f;
		viewer3DReferee.LightDirectionZ = -1f;
		viewer3DReferee.LightX = 30f;
		viewer3DReferee.LightY = 180f;
		viewer3DReferee.LightZ = 100f;
		viewer3DReferee.Location = new Point(0, 0);
		viewer3DReferee.Name = "viewer3DReferee";
		viewer3DReferee.RotationX = 6.28f;
		viewer3DReferee.RotationY = 0f;
		viewer3DReferee.RotationYCoeff = 0.001f;
		viewer3DReferee.Size = new Size(826, 458);
		viewer3DReferee.TabIndex = 5;
		viewer3DReferee.ViewX = 0f;
		viewer3DReferee.ViewY = 171f;
		viewer3DReferee.ViewZ = 49f;
		viewer3DReferee.ZbufferRenderState = null;
		splitContainer2.Panel1.Controls.Add(viewer3DReferee);
		comboLatinModels.Items.Clear();
		for (int i = 0; i < GenericHead.c_LatinModels.Length; i++)
		{
			comboLatinModels.Items.Add(GenericHead.c_LatinModels[i].ToString());
		}
		comboCaucasicModels.Items.Clear();
		for (int j = 0; j < GenericHead.c_CaucasicModels.Length; j++)
		{
			comboCaucasicModels.Items.Add(GenericHead.c_CaucasicModels[j].ToString());
		}
		comboAfricanModels.Items.Clear();
		for (int k = 0; k < GenericHead.c_AfricanModels.Length; k++)
		{
			comboAfricanModels.Items.Add(GenericHead.c_AfricanModels[k].ToString());
		}
		comboAsiaticModels.Items.Clear();
		for (int l = 0; l < GenericHead.c_AsiaticModels.Length; l++)
		{
			comboAsiaticModels.Items.Add(GenericHead.c_AsiaticModels[l].ToString());
		}
		comboShaven.Items.Clear();
		for (int m = 0; m < GenericHead.c_ShavenModels.Length; m++)
		{
			comboShaven.Items.Add(GenericHead.c_ShavenModels[m].ToString());
		}
		comboVeryShort.Items.Clear();
		for (int n = 0; n < GenericHead.c_VeryShortModels.Length; n++)
		{
			comboVeryShort.Items.Add(GenericHead.c_VeryShortModels[n].ToString());
		}
		comboShort.Items.Clear();
		for (int num = 0; num < GenericHead.c_ShortModels.Length; num++)
		{
			comboShort.Items.Add(GenericHead.c_ShortModels[num].ToString());
		}
		comboModern.Items.Clear();
		for (int num2 = 0; num2 < GenericHead.c_ModernModels.Length; num2++)
		{
			comboModern.Items.Add(GenericHead.c_ModernModels[num2].ToString());
		}
		comboMedium.Items.Clear();
		for (int num3 = 0; num3 < GenericHead.c_MediumModels.Length; num3++)
		{
			comboMedium.Items.Add(GenericHead.c_MediumModels[num3].ToString());
		}
		comboLong.Items.Clear();
		for (int num4 = 0; num4 < GenericHead.c_LongModels.Length; num4++)
		{
			comboLong.Items.Add(GenericHead.c_LongModels[num4].ToString());
		}
		comboAfro.Items.Clear();
		for (int num5 = 0; num5 < GenericHead.c_AfroModels.Length; num5++)
		{
			comboAfro.Items.Add(GenericHead.c_AfroModels[num5].ToString());
		}
		comboHeadband.Items.Clear();
		for (int num6 = 0; num6 < GenericHead.c_HeadbendModels.Length; num6++)
		{
			comboHeadband.Items.Add(GenericHead.c_HeadbendModels[num6].ToString());
		}
		pickUpControl.SelectObject = SelectReferee;
		pickUpControl.CreateObject = CreateReferee;
		pickUpControl.DeleteObject = DeleteReferee;
		pickUpControl.CloneObject = CloneReferee;
		pickUpControl.RefreshObject = RefreshReferee;
		pickUpControl.combo.Sorted = false;
		viewer2DShoes.ButtonStripVisible = false;
		viewer2DPlayerGui.ButtonStripVisible = true;
		viewer2DPlayerGui.ShowButton = true;
		viewer2DPlayerGui.ShowButtonChecked = false;
		viewer2DPlayerGui.ImageImport = ImportImageMiniface;
		viewer2DPlayerGui.ImageDelete = DeleteMiniface;
		viewer2DPlayerGui.ButtonStripVisible = true;
		viewer2DPlayerGui.RemoveButton = true;
	}

	private Referee SelectReferee(object sender, object obj)
	{
		Referee referee = (Referee)obj;
		LoadReferee(referee);
		return referee;
	}

	private Referee CreateReferee(object sender, object obj)
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
		return (Referee)m_NewIdCreator.NewObject;
	}

	private Referee DeleteReferee(object sender, object obj)
	{
		Referee referee = (Referee)obj;
		FifaEnvironment.Referees.DeleteReferee(referee);
		return null;
	}

	private Referee CloneReferee(object sender, object obj)
	{
		Referee srcIdObject = (Referee)obj;
		return (Referee)FifaEnvironment.Referees.CloneId(srcIdObject);
	}

	public Referee RefreshReferee(object sender, object obj)
	{
		Preset();
		ReloadReferee(m_CurrentReferee);
		return m_CurrentReferee;
	}

	public void ReloadReferee(Referee referee)
	{
		m_CurrentReferee = null;
		LoadReferee(referee);
	}

	public void LoadReferee(Referee referee)
	{
		if (m_IsLoaded && m_CurrentReferee != referee)
		{
			m_Locked = true;
			m_CurrentReferee = referee;
			refereeBindingSource.DataSource = m_CurrentReferee;
			Refresh();
			LoadRefereeInfo();
			LoadRefereeFace();
			m_Locked = false;
		}
	}

	private void LoadRefereeInfo()
	{
		m_Locked = true;
		numericRefereeId.Value = m_CurrentReferee.Id;
		if (m_CurrentReferee.Leagues[0] == null)
		{
			comboLeague0.SelectedIndex = 0;
		}
		else
		{
			comboLeague0.SelectedItem = m_CurrentReferee.Leagues[0];
		}
		if (m_CurrentReferee.Leagues[1] == null)
		{
			comboLeague1.SelectedIndex = 0;
		}
		else
		{
			comboLeague1.SelectedItem = m_CurrentReferee.Leagues[1];
		}
		if (m_CurrentReferee.Leagues[2] == null)
		{
			comboLeague2.SelectedIndex = 0;
		}
		else
		{
			comboLeague2.SelectedItem = m_CurrentReferee.Leagues[2];
		}
		if (m_CurrentReferee.Leagues[3] == null)
		{
			comboLeague3.SelectedIndex = 0;
		}
		else
		{
			comboLeague3.SelectedItem = m_CurrentReferee.Leagues[3];
		}
		if (m_CurrentReferee.Leagues[4] == null)
		{
			comboLeague4.SelectedIndex = 0;
		}
		else
		{
			comboLeague4.SelectedItem = m_CurrentReferee.Leagues[4];
		}
		if (m_CurrentReferee.Leagues[5] == null)
		{
			comboLeague5.SelectedIndex = 0;
		}
		else
		{
			comboLeague5.SelectedItem = m_CurrentReferee.Leagues[5];
		}
		if (m_CurrentReferee.Leagues[6] == null)
		{
			comboLeague6.SelectedIndex = 0;
		}
		else
		{
			comboLeague6.SelectedItem = m_CurrentReferee.Leagues[6];
		}
		if (m_CurrentReferee.Leagues[7] == null)
		{
			comboLeague7.SelectedIndex = 0;
		}
		else
		{
			comboLeague7.SelectedItem = m_CurrentReferee.Leagues[7];
		}
		numericShoesBrand.Value = m_CurrentReferee.shoetypecode;
		numericShoesDesign.Value = m_CurrentReferee.shoedesigncode;
		pictureColorShoes1.BackColor = Shoes.GetGenericColor(m_CurrentReferee.shoecolorcode1);
		pictureColorShoes2.BackColor = Shoes.GetGenericColor(m_CurrentReferee.shoecolorcode2);
		if (m_CurrentReferee.shoetypecode == 0)
		{
			numericShoesDesign.Enabled = true;
			pictureColorShoes1.Enabled = true;
			pictureColorShoes2.Enabled = true;
		}
		else
		{
			numericShoesDesign.Enabled = false;
			pictureColorShoes1.Enabled = false;
			pictureColorShoes2.Enabled = false;
			numericShoesDesign.Value = 0m;
		}
		viewer2DShoes.CurrentBitmap = Shoes.GetShoesColorTexture(m_CurrentReferee.shoetypecode, m_CurrentReferee.shoedesigncode);
		m_Locked = false;
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		Kit.Prepare3DModels();
		m_NewIdCreator.IdList = FifaEnvironment.Referees;
		IdArrayList[] filterValues = new IdArrayList[3]
		{
			null,
			FifaEnvironment.Countries,
			FifaEnvironment.Leagues
		};
		pickUpControl.FilterValues = filterValues;
		// The FC26 friendly snapshot does not carry the legacy descriptor array.
		// Use the real editor ranges instead of dereferencing missing metadata.
		if (FifaEnvironment.Year == 26)
		{
			numericShoesBrand.Maximum = 9999;
			numericRefereeId.Maximum = 9999999;
		}
		else
		{
			numericShoesBrand.Maximum = FifaEnvironment.FifaDb.Table[TI.referee].TableDescriptor.MaxValues[FI.referee_shoetypecode];
			numericRefereeId.Maximum = FifaEnvironment.FifaDb.Table[TI.referee].TableDescriptor.MaxValues[FI.referee_refereeid];
		}
		comboLeague0.Items.Clear();
		comboLeague0.BeginUpdate();
		comboLeague0.Items.Add(m_NotPresent);
		comboLeague0.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague0.EndUpdate();
		comboLeague1.Items.Clear();
		comboLeague1.BeginUpdate();
		comboLeague1.Items.Add(m_NotPresent);
		comboLeague1.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague1.EndUpdate();
		comboLeague2.Items.Clear();
		comboLeague2.BeginUpdate();
		comboLeague2.Items.Add(m_NotPresent);
		comboLeague2.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague2.EndUpdate();
		comboLeague3.Items.Clear();
		comboLeague3.BeginUpdate();
		comboLeague3.Items.Add(m_NotPresent);
		comboLeague3.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague3.EndUpdate();
		comboLeague4.Items.Clear();
		comboLeague4.BeginUpdate();
		comboLeague4.Items.Add(m_NotPresent);
		comboLeague4.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague4.EndUpdate();
		comboLeague5.Items.Clear();
		comboLeague5.BeginUpdate();
		comboLeague5.Items.Add(m_NotPresent);
		comboLeague5.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague5.EndUpdate();
		comboLeague6.Items.Clear();
		comboLeague6.BeginUpdate();
		comboLeague6.Items.Add(m_NotPresent);
		comboLeague6.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague6.EndUpdate();
		comboLeague7.Items.Clear();
		comboLeague7.BeginUpdate();
		comboLeague7.Items.Add(m_NotPresent);
		comboLeague7.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboLeague7.EndUpdate();
		countryListBindingSource.DataSource = FifaEnvironment.Countries;
		viewer2DPlayerGui.Visible = FifaEnvironment.Year == 14;
		toolPhoto.Visible = FifaEnvironment.Year == 14;
		pickUpControl.ObjectList = FifaEnvironment.Referees;
	}

	private void numericRefereeId_ValueChanged(object sender, EventArgs e)
	{
		if (m_CurrentReferee == null)
		{
			return;
		}
		int num = (int)numericRefereeId.Value;
		if (num != m_CurrentReferee.Id)
		{
			if (FifaEnvironment.Referees.SearchId(num) == null)
			{
				FifaEnvironment.Referees.ChangeId(m_CurrentReferee, num);
				m_CurrentReferee.Id = num;
				m_CurrentReferee.CleanFaceTexture();
				m_CurrentReferee.CleanHairTextures();
				LoadRefereeFace();
			}
			else
			{
				FifaEnvironment.UserMessages.ShowMessage(1015);
				numericRefereeId.Value = m_CurrentReferee.Id;
			}
		}
	}

	private void LoadRefereeFace()
	{
		m_GenericAppearanceSema = false;
		GenericHead.EHeadModelSet eHeadModelSet = GenericHead.GetModelSet(m_CurrentReferee.headtypecode);
		if (eHeadModelSet == GenericHead.EHeadModelSet.Unknown)
		{
			eHeadModelSet = GenericHead.EHeadModelSet.Caucasic;
			m_CurrentReferee.headtypecode = GenericHead.GetModelId(eHeadModelSet, 0);
		}
		int modelSetIndex = GenericHead.GetModelSetIndex(eHeadModelSet, m_CurrentReferee.headtypecode);
		switch (eHeadModelSet)
		{
		case GenericHead.EHeadModelSet.Caucasic:
			comboCaucasicModels.SelectedIndex = modelSetIndex;
			radioButtonCaucasic.Checked = true;
			break;
		case GenericHead.EHeadModelSet.Latin:
			comboLatinModels.SelectedIndex = modelSetIndex;
			radioButtonLatin.Checked = true;
			break;
		case GenericHead.EHeadModelSet.African:
			comboAfricanModels.SelectedIndex = modelSetIndex;
			radioButtonAfrican.Checked = true;
			break;
		case GenericHead.EHeadModelSet.Asiatic:
			comboAsiaticModels.SelectedIndex = modelSetIndex;
			radioButtonAsiatic.Checked = true;
			break;
		}
		GenericHead.EHairModelSet hairModelSet = GenericHead.GetHairModelSet(m_CurrentReferee.hairtypecode);
		int hairModelSetIndex = GenericHead.GetHairModelSetIndex(hairModelSet, m_CurrentReferee.hairtypecode);
		switch (hairModelSet)
		{
		case GenericHead.EHairModelSet.Shaven:
			comboShaven.SelectedIndex = hairModelSetIndex;
			radioShaven.Checked = true;
			break;
		case GenericHead.EHairModelSet.VeryShort:
			comboVeryShort.SelectedIndex = hairModelSetIndex;
			radioVeryShort.Checked = true;
			break;
		case GenericHead.EHairModelSet.Short:
			comboShort.SelectedIndex = hairModelSetIndex;
			radioShort.Checked = true;
			break;
		case GenericHead.EHairModelSet.Modern:
			comboModern.SelectedIndex = hairModelSetIndex;
			radioModern.Checked = true;
			break;
		case GenericHead.EHairModelSet.Medium:
			comboMedium.SelectedIndex = hairModelSetIndex;
			radioMedium.Checked = true;
			break;
		case GenericHead.EHairModelSet.Long:
			comboLong.SelectedIndex = hairModelSetIndex;
			radioLong.Checked = true;
			break;
		case GenericHead.EHairModelSet.Afro:
			comboAfro.SelectedIndex = hairModelSetIndex;
			radioAfro.Checked = true;
			break;
		case GenericHead.EHairModelSet.Headbend:
			comboHeadband.SelectedIndex = hairModelSetIndex;
			radioHeadband.Checked = true;
			break;
		}
		domainFacialHair.SelectedIndex = m_CurrentReferee.facialhairtypecode;
		domainHairColor.SelectedIndex = m_CurrentReferee.haircolorcode;
		comboSideburns.SelectedIndex = m_CurrentReferee.sideburnscode;
		comboSkintype.SelectedIndex = m_CurrentReferee.skintypecode;
		comboSkinColor.SelectedIndex = m_CurrentReferee.skintonecode - 1;
		comboEyescolor.SelectedIndex = m_CurrentReferee.eyecolorcode - 1;
		comboEyeBow.SelectedIndex = m_CurrentReferee.eyebrowcode;
		comboFacialHairColor.SelectedIndex = m_CurrentReferee.facialhaircolorcode;
		m_GenericAppearanceSema = true;
		if (FifaEnvironment.Year == 14)
		{
			viewer2DPlayerGui.CurrentBitmap = m_CurrentReferee.GetPhoto();
		}
		UpdateAndShowHead3D();
	}

	private void toolPhoto_Click(object sender, EventArgs e)
	{
		Bitmap bitmap = viewer3DReferee.Photo();
		int num = bitmap.Height;
		int num2 = bitmap.Width;
		int num3 = ((num2 < num * 17 / 16) ? num2 : (num * 5 / 4));
		int num4 = (num2 - num3) / 2;
		Rectangle srcRect = new Rectangle(num4, 0, num3, num);
		Rectangle destRect = new Rectangle(0, 10, 256, 190);
		Bitmap srcBitmap = GraphicUtil.MakeAutoTransparent(bitmap);
		Bitmap bitmap2 = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
		GraphicUtil.RemapRectangle(srcBitmap, srcRect, bitmap2, destRect);
		m_CurrentReferee.SetPhoto(bitmap2);
		viewer2DPlayerGui.CurrentBitmap = bitmap2;
	}

	private bool ImportImageMiniface(object sender, Bitmap bitmap)
	{
		return m_CurrentReferee.SetPhoto(bitmap);
	}

	private bool DeleteMiniface(object sender)
	{
		return m_CurrentReferee.DeletePhoto();
	}

	private void buttonGetId_Click(object sender, EventArgs e)
	{
		int newId = FifaEnvironment.Referees.GetNewId();
		if (newId == -1)
		{
			FifaEnvironment.UserMessages.ShowMessage(5050);
		}
		else
		{
			numericRefereeId.Value = newId;
		}
	}

	private void labelCountry_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentReferee.Country != null)
		{
			MainForm.CM.JumpTo(m_CurrentReferee.Country);
		}
	}

	private void RefereeForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void radioButtonAsiatic_CheckedChanged(object sender, EventArgs e)
	{
		if (comboAsiaticModels.SelectedIndex < 0)
		{
			comboAsiaticModels.SelectedIndex = 0;
		}
		comboAsiaticModels.Visible = radioButtonAsiatic.Checked;
		if (radioButtonAsiatic.Checked)
		{
			radioButtonAsiatic.BackColor = Color.LightSkyBlue;
			if (m_CurrentReferee.headtypecode != GenericHead.c_AsiaticModels[comboAsiaticModels.SelectedIndex])
			{
				m_CurrentReferee.headtypecode = GenericHead.c_AsiaticModels[comboAsiaticModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked)
				{
					UpdateAndShowHead3D();
				}
			}
		}
		else
		{
			radioButtonAsiatic.BackColor = Color.Transparent;
		}
	}

	private void radioButtonCaucasic_CheckedChanged(object sender, EventArgs e)
	{
		if (comboCaucasicModels.SelectedIndex < 0)
		{
			comboCaucasicModels.SelectedIndex = 0;
		}
		comboCaucasicModels.Visible = radioButtonCaucasic.Checked;
		if (radioButtonCaucasic.Checked)
		{
			radioButtonCaucasic.BackColor = Color.LightSkyBlue;
			if (m_CurrentReferee.headtypecode != GenericHead.c_CaucasicModels[comboCaucasicModels.SelectedIndex])
			{
				m_CurrentReferee.headtypecode = GenericHead.c_CaucasicModels[comboCaucasicModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked)
				{
					UpdateAndShowHead3D();
				}
			}
		}
		else
		{
			radioButtonCaucasic.BackColor = Color.Transparent;
		}
	}

	private void radioButtonAfrican_CheckedChanged(object sender, EventArgs e)
	{
		if (comboAfricanModels.SelectedIndex < 0)
		{
			comboAfricanModels.SelectedIndex = 0;
		}
		comboAfricanModels.Visible = radioButtonAfrican.Checked;
		if (radioButtonAfrican.Checked)
		{
			radioButtonAfrican.BackColor = Color.LightSkyBlue;
			if (m_CurrentReferee.headtypecode != GenericHead.c_AfricanModels[comboAfricanModels.SelectedIndex])
			{
				m_CurrentReferee.headtypecode = GenericHead.c_AfricanModels[comboAfricanModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked)
				{
					UpdateAndShowHead3D();
				}
			}
		}
		else
		{
			radioButtonAfrican.BackColor = Color.Transparent;
		}
	}

	private void radioButtonLatin_CheckedChanged(object sender, EventArgs e)
	{
		if (comboLatinModels.SelectedIndex < 0)
		{
			comboLatinModels.SelectedIndex = 0;
		}
		comboLatinModels.Visible = radioButtonLatin.Checked;
		if (radioButtonLatin.Checked)
		{
			radioButtonLatin.BackColor = Color.LightSkyBlue;
			if (m_CurrentReferee.headtypecode != GenericHead.c_LatinModels[comboLatinModels.SelectedIndex])
			{
				m_CurrentReferee.headtypecode = GenericHead.c_LatinModels[comboLatinModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked)
				{
					UpdateAndShowHead3D();
				}
			}
		}
		else
		{
			radioButtonLatin.BackColor = Color.Transparent;
		}
	}

	private void UpdateAndShowHead3D()
	{
		if (!buttonShow3DModel.Checked)
		{
			viewer3DReferee.ShowEmpty();
			return;
		}
		Bitmap faceTexture = m_CurrentReferee.GetFaceTexture();
		Bitmap eyesTexture = m_CurrentReferee.GetEyesTexture();
		Rx3File headModel = m_CurrentReferee.GetHeadModel();
		if (headModel == null)
		{
			viewer3DReferee.ShowEmpty();
			return;
		}
		Player.s_Model3DHead = new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], faceTexture);
		Player.s_Model3DEyes = new Model3D(headModel.Rx3IndexArrays[1], headModel.Rx3VertexArrays[1], eyesTexture);
		Player.s_Model3DHairPart4 = null;
		Player.s_Model3DHairPart5 = null;
		if (headModel.Rx3VertexArrays[0].nVertex > headModel.Rx3VertexArrays[1].nVertex)
		{
			Player.s_Model3DHead = new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], faceTexture);
			Player.s_Model3DEyes = new Model3D(headModel.Rx3IndexArrays[1], headModel.Rx3VertexArrays[1], eyesTexture);
		}
		else
		{
			Player.s_Model3DHead = new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], eyesTexture);
			Player.s_Model3DEyes = new Model3D(headModel.Rx3IndexArrays[1], headModel.Rx3VertexArrays[1], faceTexture);
		}
		Rx3File hairModel = m_CurrentReferee.GetHairModel();
		if (hairModel != null)
		{
			Bitmap hairColorTexture = m_CurrentReferee.GetHairColorTexture();
			Bitmap hairAlfaTexture = m_CurrentReferee.GetHairAlfaTexture();
			Bitmap bitmap = null;
			Bitmap bitmap2 = null;
			if (hairColorTexture != null && hairAlfaTexture != null)
			{
				hairColorTexture = GraphicUtil.ResizeBitmap(hairColorTexture, hairAlfaTexture.Width, hairAlfaTexture.Height, InterpolationMode.Bilinear);
				bitmap = (Bitmap)GraphicUtil.CanvasSizeBitmapCentered(hairColorTexture, hairAlfaTexture.Width, hairAlfaTexture.Height).Clone();
				GraphicUtil.GetAlfaFromChannel(bitmap, hairAlfaTexture, 4 - m_HairAlfaChannel);
				bitmap2 = (Bitmap)GraphicUtil.CanvasSizeBitmapCentered(hairColorTexture, hairAlfaTexture.Width, hairAlfaTexture.Height).Clone();
				GraphicUtil.GetAlfaFromChannel(bitmap2, hairAlfaTexture, m_HairAlfaChannel);
			}
			Rx3IndexArray.TriangleListType = Rx3IndexArray.ETriangleListType.InvertEven;
			if (hairModel.HairAlfaFlag == 53 || hairModel.HairAlfaFlag == 54 || hairModel.HairAlfaFlag == 58)
			{
				Player.s_Model3DHairPart4 = new Model3D(hairModel.Rx3IndexArrays[0], hairModel.Rx3VertexArrays[0], bitmap2);
				if (hairModel.Rx3IndexArrays.Length > 1)
				{
					Player.s_Model3DHairPart5 = new Model3D(hairModel.Rx3IndexArrays[1], hairModel.Rx3VertexArrays[1], bitmap);
				}
			}
			else if (hairModel.HairAlfaFlag == 50)
			{
				Player.s_Model3DHairPart4 = new Model3D(hairModel.Rx3IndexArrays[0], hairModel.Rx3VertexArrays[0], bitmap);
				if (hairModel.Rx3IndexArrays.Length > 1)
				{
					Player.s_Model3DHairPart5 = new Model3D(hairModel.Rx3IndexArrays[1], hairModel.Rx3VertexArrays[1], bitmap2);
				}
			}
			else
			{
				FifaEnvironment.UserMessages.ShowMessage(14999, "Debug Trap: Unexpected Hair Format");
				Player.s_Model3DHairPart4 = new Model3D(hairModel.Rx3IndexArrays[0], hairModel.Rx3VertexArrays[0], bitmap2);
				if (hairModel.Rx3IndexArrays.Length > 1)
				{
					Player.s_Model3DHairPart5 = new Model3D(hairModel.Rx3IndexArrays[1], hairModel.Rx3VertexArrays[1], bitmap);
				}
			}
		}
		ShowHead3D();
	}

	private void ShowHead3D()
	{
		int num = 2;
		if (Player.s_Model3DHairPart4 != null)
		{
			num = 3;
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			num = 4;
		}
		Kit kit = null;
		kit = FifaEnvironment.Kits.GetKit(6004, 5);
		if (kit != null)
		{
			Bitmap textureBitmap = GraphicUtil.EmbossBitmap(kit.GetKitTextures()[1], Kit.s_JerseyWrinkle);
			Kit.s_JerseyModel3D[kit.jerseyCollar].TextureBitmap = textureBitmap;
		}
		if (kit != null)
		{
			num++;
		}
		viewer3DReferee.Clean(num);
		int num2 = 0;
		if (kit != null)
		{
			viewer3DReferee.SetMesh(num2++, Kit.s_JerseyModel3D[kit.jerseyCollar]);
		}
		viewer3DReferee.SetMesh(num2++, Player.s_Model3DHead);
		viewer3DReferee.SetMesh(num2++, Player.s_Model3DEyes);
		if (Player.s_Model3DHairPart4 != null)
		{
			viewer3DReferee.SetMesh(num2++, Player.s_Model3DHairPart4, zBufferState: false);
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			viewer3DReferee.SetMesh(num2++, Player.s_Model3DHairPart5, zBufferState: false);
		}
		viewer3DReferee.Render();
	}

	private void radioButtonAfro_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_AfroModels);
	}

	private void radioButtonLong_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_LongModels);
	}

	private void radioButtonMedium_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_MediumModels);
	}

	private void radioShaven_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_ShavenModels);
	}

	private void radioModern_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_ModernModels);
	}

	private void radioVeryShort_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_VeryShortModels);
	}

	private void radioShort_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_ShortModels);
	}

	private void radioHeadband_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_HeadbendModels);
	}

	private void radioHair_CheckedChanged(object sender, int[] hairMap)
	{
		RadioButton radioButton = (RadioButton)sender;
		ComboBox comboBox = (ComboBox)radioButton.Tag;
		if (comboBox.SelectedIndex < 0)
		{
			comboBox.SelectedIndex = 0;
		}
		comboBox.Visible = radioButton.Checked;
		if (radioButton.Checked)
		{
			radioButton.BackColor = Color.LightSkyBlue;
			if (m_CurrentReferee.hairtypecode != hairMap[comboBox.SelectedIndex])
			{
				m_CurrentReferee.hairtypecode = hairMap[comboBox.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked)
				{
					m_CurrentReferee.CleanHairTextures();
					UpdateAndShowHead3D();
				}
			}
		}
		else
		{
			radioButton.BackColor = Color.Transparent;
		}
	}

	private void comboAsiaticModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboAsiaticModels.SelectedIndex >= 0)
		{
			m_CurrentReferee.headtypecode = GenericHead.c_AsiaticModels[comboAsiaticModels.SelectedIndex];
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboAfricanModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboAfricanModels.SelectedIndex >= 0)
		{
			m_CurrentReferee.headtypecode = GenericHead.c_AfricanModels[comboAfricanModels.SelectedIndex];
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboCaucasicModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboCaucasicModels.SelectedIndex >= 0)
		{
			m_CurrentReferee.headtypecode = GenericHead.c_CaucasicModels[comboCaucasicModels.SelectedIndex];
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboLatinModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboLatinModels.SelectedIndex >= 0)
		{
			m_CurrentReferee.headtypecode = GenericHead.c_LatinModels[comboLatinModels.SelectedIndex];
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboHeadband_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_HeadbendModels);
	}

	private void comboHair_SelectedIndexChanged(object sender, int[] hairMap)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (m_GenericAppearanceSema && comboBox.SelectedIndex >= 0)
		{
			m_CurrentReferee.hairtypecode = hairMap[comboBox.SelectedIndex];
			if (m_GenericAppearanceSema && buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanHair();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboAfro_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_AfroModels);
	}

	private void comboLong_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_LongModels);
	}

	private void comboMedium_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_MediumModels);
	}

	private void comboModern_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_ModernModels);
	}

	private void comboShaven_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_ShavenModels);
	}

	private void comboShort_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_ShortModels);
	}

	private void comboVeryShort_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_VeryShortModels);
	}

	private void domainHairColor_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema)
		{
			m_CurrentReferee.haircolorcode = domainHairColor.SelectedIndex;
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanHairTextures();
				UpdateAndShowHead3D();
			}
		}
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		UpdateAndShowHead3D();
	}

	private void comboSkintype_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboSkintype.SelectedIndex >= 0)
		{
			m_CurrentReferee.skintypecode = comboSkintype.SelectedIndex;
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboSkinColor_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboSkinColor.SelectedIndex >= 0)
		{
			m_CurrentReferee.skintonecode = comboSkinColor.SelectedIndex + 1;
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboEyescolor_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboEyescolor.SelectedIndex >= 0)
		{
			m_CurrentReferee.eyecolorcode = comboEyescolor.SelectedIndex + 1;
			m_CurrentReferee.CleanEyesTexture();
			if (buttonShow3DModel.Checked)
			{
				UpdateAndShowHead3D();
			}
		}
	}

	private void domainFacialHair_SelectedItemChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema)
		{
			m_CurrentReferee.facialhairtypecode = domainFacialHair.SelectedIndex;
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboFacialHairColor_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboFacialHairColor.SelectedIndex >= 0)
		{
			m_CurrentReferee.facialhaircolorcode = comboFacialHairColor.SelectedIndex;
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboSideburns_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboSideburns.SelectedIndex >= 0)
		{
			m_CurrentReferee.sideburnscode = comboSideburns.SelectedIndex;
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboEyeBow_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboEyeBow.SelectedIndex >= 0)
		{
			m_CurrentReferee.eyebrowcode = comboEyeBow.SelectedIndex;
			if (buttonShow3DModel.Checked)
			{
				m_CurrentReferee.CleanFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void buttonRandomizeIdentity_Click(object sender, EventArgs e)
	{
	}

	private void buttonRandomizeAppearance_Click(object sender, EventArgs e)
	{
		if (radioButtonAfrican.Checked)
		{
			m_CurrentReferee.RandomizeAfricanAppearance();
		}
		else if (radioButtonAsiatic.Checked)
		{
			m_CurrentReferee.RandomizeAsiaticAppearance();
		}
		else if (radioButtonCaucasic.Checked)
		{
			m_CurrentReferee.RandomizeCaucasianAppearance();
		}
		else if (radioButtonLatin.Checked)
		{
			m_CurrentReferee.RandomizeLatinAppearance();
		}
		m_CurrentReferee.CleanAllHead();
		LoadRefereeFace();
		m_GenericAppearanceSema = true;
	}

	private void buttonSwitchRenderingMode_Click(object sender, EventArgs e)
	{
		m_HairAlfaChannel = 4 - m_HairAlfaChannel;
		UpdateAndShowHead3D();
	}

	private void textFirstName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentReferee.firstname = textFirstName.Text;
			pickUpControl.SwitchObject(m_CurrentReferee);
		}
	}

	private void textSurname_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentReferee.surname = textSurname.Text;
			pickUpControl.SwitchObject(m_CurrentReferee);
		}
	}

	private void comboLeague0_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague0.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[0] = null;
			m_CurrentReferee.leagueids[0] = 0;
		}
		else
		{
			League league = (League)comboLeague0.SelectedItem;
			m_CurrentReferee.Leagues[0] = league;
			m_CurrentReferee.leagueids[0] = league.Id;
		}
	}

	private void comboLeague1_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague1.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[1] = null;
			m_CurrentReferee.leagueids[1] = 0;
		}
		else
		{
			League league = (League)comboLeague1.SelectedItem;
			m_CurrentReferee.Leagues[1] = league;
			m_CurrentReferee.leagueids[1] = league.Id;
		}
	}

	private void comboLeague2_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague2.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[2] = null;
			m_CurrentReferee.leagueids[2] = 0;
		}
		else
		{
			League league = (League)comboLeague2.SelectedItem;
			m_CurrentReferee.Leagues[2] = league;
			m_CurrentReferee.leagueids[2] = league.Id;
		}
	}

	private void comboLeague3_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague3.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[3] = null;
			m_CurrentReferee.leagueids[3] = 0;
		}
		else
		{
			League league = (League)comboLeague3.SelectedItem;
			m_CurrentReferee.Leagues[3] = league;
			m_CurrentReferee.leagueids[3] = league.Id;
		}
	}

	private void comboLeague4_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague4.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[4] = null;
			m_CurrentReferee.leagueids[4] = 0;
		}
		else
		{
			League league = (League)comboLeague4.SelectedItem;
			m_CurrentReferee.Leagues[4] = league;
			m_CurrentReferee.leagueids[4] = league.Id;
		}
	}

	private void comboLeague5_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague5.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[5] = null;
			m_CurrentReferee.leagueids[5] = 0;
		}
		else
		{
			League league = (League)comboLeague5.SelectedItem;
			m_CurrentReferee.Leagues[5] = league;
			m_CurrentReferee.leagueids[5] = league.Id;
		}
	}

	private void comboLeague6_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague6.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[6] = null;
			m_CurrentReferee.leagueids[6] = 0;
		}
		else
		{
			League league = (League)comboLeague6.SelectedItem;
			m_CurrentReferee.Leagues[6] = league;
			m_CurrentReferee.leagueids[6] = league.Id;
		}
	}

	private void comboLeague7_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeague7.SelectedIndex == 0)
		{
			m_CurrentReferee.Leagues[7] = null;
			m_CurrentReferee.leagueids[7] = 0;
		}
		else
		{
			League league = (League)comboLeague7.SelectedItem;
			m_CurrentReferee.Leagues[7] = league;
			m_CurrentReferee.leagueids[7] = league.Id;
		}
	}

	private void labelShoes_DoubleClick(object sender, EventArgs e)
	{
		Shoes shoes = (Shoes)FifaEnvironment.Shoes.SearchId(m_CurrentReferee.shoetypecode);
		if (shoes != null)
		{
			MainForm.CM.JumpTo(shoes);
		}
	}

	private void numericShoesBrand_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericShoesBrand.Value;
			if (num == 0)
			{
				m_CurrentReferee.shoetypecode = num;
				m_CurrentReferee.shoecolorcode1 = 0;
				m_CurrentReferee.shoecolorcode2 = 15;
				pictureColorShoes1.BackColor = Shoes.ShoesColorPalette[m_CurrentReferee.shoecolorcode1];
				pictureColorShoes2.BackColor = Shoes.ShoesColorPalette[m_CurrentReferee.shoecolorcode2];
				numericShoesDesign.Enabled = true;
				pictureColorShoes1.Enabled = true;
				pictureColorShoes2.Enabled = true;
			}
			else
			{
				m_CurrentReferee.shoetypecode = num;
				numericShoesDesign.Enabled = false;
				pictureColorShoes1.Enabled = false;
				pictureColorShoes2.Enabled = false;
				pictureColorShoes1.BackColor = Color.Transparent;
				pictureColorShoes2.BackColor = Color.Transparent;
				m_CurrentReferee.shoedesigncode = 0;
				m_CurrentReferee.shoecolorcode1 = 30;
				m_CurrentReferee.shoecolorcode2 = 31;
				numericShoesDesign.Value = 0m;
			}
			viewer2DShoes.CurrentBitmap = Shoes.GetShoesColorTexture(num, 0);
		}
	}

	private void numericShoesDesign_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericShoesDesign.Value;
			m_CurrentReferee.shoedesigncode = num;
			if (m_CurrentReferee.shoetypecode == 0)
			{
				viewer2DShoes.CurrentBitmap = Shoes.GetShoesColorTexture(0, num);
			}
		}
	}

	private void pictureColorShoes1_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(Shoes.ShoesColorPalette, m_CurrentReferee.shoecolorcode1);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentReferee.shoecolorcode1 = colorSelector.SelectedIndex;
			pictureColorShoes1.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void pictureColorShoes2_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(Shoes.ShoesColorPalette, m_CurrentReferee.shoecolorcode2);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentReferee.shoecolorcode2 = colorSelector.SelectedIndex;
			pictureColorShoes2.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void buttonRandomizeAllReferees_Click(object sender, EventArgs e)
	{
		foreach (Referee referee in FifaEnvironment.Referees)
		{
			switch (referee.Country.Confederation)
			{
			default:
				referee.RandomizeCaucasianAppearance();
				break;
			case 2:
				referee.RandomizeAfricanAppearance();
				break;
			case 3:
				referee.RandomizeLatinAppearance();
				break;
			case 4:
				if (referee.Country.Id == 195)
				{
					referee.RandomizeCaucasianAppearance();
				}
				else
				{
					referee.RandomizeAsiaticAppearance();
				}
				break;
			case 6:
				if (referee.Country.Id == 83)
				{
					referee.RandomizeLatinAppearance();
				}
				else
				{
					referee.RandomizeCaucasianAppearance();
				}
				break;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.RefereeForm));
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
		this.groupIdentity = new System.Windows.Forms.GroupBox();
		this.radioButtonGenderFemale = new System.Windows.Forms.RadioButton();
		this.refereeBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.radioButtonGenderMale = new System.Windows.Forms.RadioButton();
		this.groupShoes = new System.Windows.Forms.GroupBox();
		this.label1ShoesType = new System.Windows.Forms.Label();
		this.pictureColorShoes2 = new System.Windows.Forms.PictureBox();
		this.pictureColorShoes1 = new System.Windows.Forms.PictureBox();
		this.numericShoesBrand = new System.Windows.Forms.NumericUpDown();
		this.labelShoesType = new System.Windows.Forms.Label();
		this.labelShoesColor = new System.Windows.Forms.Label();
		this.numericShoesDesign = new System.Windows.Forms.NumericUpDown();
		this.viewer2DShoes = new FifaControls.Viewer2D();
		this.labelShoes = new System.Windows.Forms.Label();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.comboStyle = new System.Windows.Forms.ComboBox();
		this.labelStyle = new System.Windows.Forms.Label();
		this.domainSleeves = new System.Windows.Forms.DomainUpDown();
		this.labelSleeves = new System.Windows.Forms.Label();
		this.comboBody = new System.Windows.Forms.ComboBox();
		this.numericHeight = new System.Windows.Forms.NumericUpDown();
		this.numericWeight = new System.Windows.Forms.NumericUpDown();
		this.labelWeight = new System.Windows.Forms.Label();
		this.labelBody = new System.Windows.Forms.Label();
		this.labelHeight = new System.Windows.Forms.Label();
		this.buttonGetId = new System.Windows.Forms.Button();
		this.numericRefereeId = new System.Windows.Forms.NumericUpDown();
		this.buttonRandomizeIdentity = new System.Windows.Forms.Button();
		this.dateBirthDate = new System.Windows.Forms.DateTimePicker();
		this.labelBirthdate = new System.Windows.Forms.Label();
		this.labelRefereeId = new System.Windows.Forms.Label();
		this.textSurname = new System.Windows.Forms.TextBox();
		this.textFirstName = new System.Windows.Forms.TextBox();
		this.comboCountry = new System.Windows.Forms.ComboBox();
		this.countryListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.labelFirstName = new System.Windows.Forms.Label();
		this.labelSurame = new System.Windows.Forms.Label();
		this.labelCountry = new System.Windows.Forms.Label();
		this.groupLeagues = new System.Windows.Forms.GroupBox();
		this.comboLeague4 = new System.Windows.Forms.ComboBox();
		this.comboLeague7 = new System.Windows.Forms.ComboBox();
		this.comboLeague5 = new System.Windows.Forms.ComboBox();
		this.comboLeague6 = new System.Windows.Forms.ComboBox();
		this.comboLeague0 = new System.Windows.Forms.ComboBox();
		this.comboLeague3 = new System.Windows.Forms.ComboBox();
		this.comboLeague1 = new System.Windows.Forms.ComboBox();
		this.comboLeague2 = new System.Windows.Forms.ComboBox();
		this.viewer2DPlayerGui = new FifaControls.Viewer2D();
		this.splitContainer2 = new System.Windows.Forms.SplitContainer();
		this.tool3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.buttonSwitchRenderingMode = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.toolPhoto = new System.Windows.Forms.ToolStripButton();
		this.groupGenericFace = new System.Windows.Forms.GroupBox();
		this.groupTextureInfo = new System.Windows.Forms.GroupBox();
		this.comboSkinColor = new System.Windows.Forms.ComboBox();
		this.labelFacialHair = new System.Windows.Forms.Label();
		this.labelEyeBow = new System.Windows.Forms.Label();
		this.domainFacialHair = new System.Windows.Forms.ComboBox();
		this.comboEyeBow = new System.Windows.Forms.ComboBox();
		this.labelSideburns = new System.Windows.Forms.Label();
		this.comboSideburns = new System.Windows.Forms.ComboBox();
		this.labelSkintype = new System.Windows.Forms.Label();
		this.comboEyescolor = new System.Windows.Forms.ComboBox();
		this.comboSkintype = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.comboFacialHairColor = new System.Windows.Forms.ComboBox();
		this.labelFacialHairColor = new System.Windows.Forms.Label();
		this.groupHairModel = new System.Windows.Forms.GroupBox();
		this.comboHeadband = new System.Windows.Forms.ComboBox();
		this.comboAfro = new System.Windows.Forms.ComboBox();
		this.comboLong = new System.Windows.Forms.ComboBox();
		this.comboMedium = new System.Windows.Forms.ComboBox();
		this.comboModern = new System.Windows.Forms.ComboBox();
		this.comboShort = new System.Windows.Forms.ComboBox();
		this.comboVeryShort = new System.Windows.Forms.ComboBox();
		this.comboShaven = new System.Windows.Forms.ComboBox();
		this.radioHeadband = new System.Windows.Forms.RadioButton();
		this.radioShaven = new System.Windows.Forms.RadioButton();
		this.radioAfro = new System.Windows.Forms.RadioButton();
		this.radioLong = new System.Windows.Forms.RadioButton();
		this.radioMedium = new System.Windows.Forms.RadioButton();
		this.radioModern = new System.Windows.Forms.RadioButton();
		this.radioShort = new System.Windows.Forms.RadioButton();
		this.radioVeryShort = new System.Windows.Forms.RadioButton();
		this.domainHairColor = new System.Windows.Forms.ComboBox();
		this.labelHairColor = new System.Windows.Forms.Label();
		this.groupHeadModel = new System.Windows.Forms.GroupBox();
		this.comboLatinModels = new System.Windows.Forms.ComboBox();
		this.radioButtonLatin = new System.Windows.Forms.RadioButton();
		this.comboAsiaticModels = new System.Windows.Forms.ComboBox();
		this.radioButtonAsiatic = new System.Windows.Forms.RadioButton();
		this.comboAfricanModels = new System.Windows.Forms.ComboBox();
		this.radioButtonAfrican = new System.Windows.Forms.RadioButton();
		this.radioButtonCaucasic = new System.Windows.Forms.RadioButton();
		this.comboCaucasicModels = new System.Windows.Forms.ComboBox();
		this.buttonRandomizeAppearance = new System.Windows.Forms.Button();
		this.labelHeadType = new System.Windows.Forms.Label();
		this.labelHairType = new System.Windows.Forms.Label();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.buttonRandomizeAllReferees = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		this.flowLayoutPanel1.SuspendLayout();
		this.groupIdentity.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.refereeBindingSource).BeginInit();
		this.groupShoes.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesBrand).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesDesign).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericHeight).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericWeight).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericRefereeId).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).BeginInit();
		this.groupLeagues.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).BeginInit();
		this.splitContainer2.Panel1.SuspendLayout();
		this.splitContainer2.Panel2.SuspendLayout();
		this.splitContainer2.SuspendLayout();
		this.tool3D.SuspendLayout();
		this.groupGenericFace.SuspendLayout();
		this.groupTextureInfo.SuspendLayout();
		this.groupHairModel.SuspendLayout();
		this.groupHeadModel.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.AutoScroll = true;
		this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
		this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
		this.splitContainer1.Size = new System.Drawing.Size(1357, 807);
		this.splitContainer1.SplitterDistance = 527;
		this.splitContainer1.TabIndex = 2;
		this.flowLayoutPanel1.Controls.Add(this.groupIdentity);
		this.flowLayoutPanel1.Controls.Add(this.groupLeagues);
		this.flowLayoutPanel1.Controls.Add(this.viewer2DPlayerGui);
		this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
		this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		this.flowLayoutPanel1.Name = "flowLayoutPanel1";
		this.flowLayoutPanel1.Size = new System.Drawing.Size(527, 807);
		this.flowLayoutPanel1.TabIndex = 0;
		this.groupIdentity.Controls.Add(this.radioButtonGenderFemale);
		this.groupIdentity.Controls.Add(this.radioButtonGenderMale);
		this.groupIdentity.Controls.Add(this.groupShoes);
		this.groupIdentity.Controls.Add(this.comboBox1);
		this.groupIdentity.Controls.Add(this.label3);
		this.groupIdentity.Controls.Add(this.comboStyle);
		this.groupIdentity.Controls.Add(this.labelStyle);
		this.groupIdentity.Controls.Add(this.domainSleeves);
		this.groupIdentity.Controls.Add(this.labelSleeves);
		this.groupIdentity.Controls.Add(this.comboBody);
		this.groupIdentity.Controls.Add(this.numericHeight);
		this.groupIdentity.Controls.Add(this.numericWeight);
		this.groupIdentity.Controls.Add(this.labelWeight);
		this.groupIdentity.Controls.Add(this.labelBody);
		this.groupIdentity.Controls.Add(this.labelHeight);
		this.groupIdentity.Controls.Add(this.buttonGetId);
		this.groupIdentity.Controls.Add(this.numericRefereeId);
		this.groupIdentity.Controls.Add(this.buttonRandomizeIdentity);
		this.groupIdentity.Controls.Add(this.dateBirthDate);
		this.groupIdentity.Controls.Add(this.labelBirthdate);
		this.groupIdentity.Controls.Add(this.labelRefereeId);
		this.groupIdentity.Controls.Add(this.textSurname);
		this.groupIdentity.Controls.Add(this.textFirstName);
		this.groupIdentity.Controls.Add(this.comboCountry);
		this.groupIdentity.Controls.Add(this.labelFirstName);
		this.groupIdentity.Controls.Add(this.labelSurame);
		this.groupIdentity.Controls.Add(this.labelCountry);
		this.groupIdentity.Location = new System.Drawing.Point(3, 3);
		this.groupIdentity.Name = "groupIdentity";
		this.groupIdentity.Size = new System.Drawing.Size(512, 282);
		this.groupIdentity.TabIndex = 0;
		this.groupIdentity.TabStop = false;
		this.groupIdentity.Text = "Identity";
		this.radioButtonGenderFemale.AutoSize = true;
		this.radioButtonGenderFemale.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.refereeBindingSource, "Female", true));
		this.radioButtonGenderFemale.Location = new System.Drawing.Point(163, 147);
		this.radioButtonGenderFemale.Name = "radioButtonGenderFemale";
		this.radioButtonGenderFemale.Size = new System.Drawing.Size(59, 17);
		this.radioButtonGenderFemale.TabIndex = 191;
		this.radioButtonGenderFemale.TabStop = true;
		this.radioButtonGenderFemale.Text = "Female";
		this.radioButtonGenderFemale.UseVisualStyleBackColor = true;
		this.refereeBindingSource.DataSource = typeof(FifaLibrary.Referee);
		this.radioButtonGenderMale.AutoSize = true;
		this.radioButtonGenderMale.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.refereeBindingSource, "Male", true));
		this.radioButtonGenderMale.Location = new System.Drawing.Point(109, 147);
		this.radioButtonGenderMale.Name = "radioButtonGenderMale";
		this.radioButtonGenderMale.Size = new System.Drawing.Size(48, 17);
		this.radioButtonGenderMale.TabIndex = 190;
		this.radioButtonGenderMale.TabStop = true;
		this.radioButtonGenderMale.Text = "Male";
		this.radioButtonGenderMale.UseVisualStyleBackColor = true;
		this.groupShoes.Controls.Add(this.label1ShoesType);
		this.groupShoes.Controls.Add(this.pictureColorShoes2);
		this.groupShoes.Controls.Add(this.pictureColorShoes1);
		this.groupShoes.Controls.Add(this.numericShoesBrand);
		this.groupShoes.Controls.Add(this.labelShoesType);
		this.groupShoes.Controls.Add(this.labelShoesColor);
		this.groupShoes.Controls.Add(this.numericShoesDesign);
		this.groupShoes.Controls.Add(this.viewer2DShoes);
		this.groupShoes.Controls.Add(this.labelShoes);
		this.groupShoes.Location = new System.Drawing.Point(263, 94);
		this.groupShoes.Name = "groupShoes";
		this.groupShoes.Size = new System.Drawing.Size(243, 178);
		this.groupShoes.TabIndex = 189;
		this.groupShoes.TabStop = false;
		this.groupShoes.Text = "Shoes";
		this.label1ShoesType.AutoSize = true;
		this.label1ShoesType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1ShoesType.Location = new System.Drawing.Point(29, 66);
		this.label1ShoesType.Name = "label1ShoesType";
		this.label1ShoesType.Size = new System.Drawing.Size(40, 13);
		this.label1ShoesType.TabIndex = 64;
		this.label1ShoesType.Text = "Design";
		this.label1ShoesType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pictureColorShoes2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureColorShoes2.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureColorShoes2.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.refereeBindingSource, "shoecolorcode2", true));
		this.pictureColorShoes2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorShoes2.Location = new System.Drawing.Point(72, 131);
		this.pictureColorShoes2.Name = "pictureColorShoes2";
		this.pictureColorShoes2.Size = new System.Drawing.Size(20, 20);
		this.pictureColorShoes2.TabIndex = 63;
		this.pictureColorShoes2.TabStop = false;
		this.pictureColorShoes2.Click += new System.EventHandler(pictureColorShoes2_Click);
		this.pictureColorShoes1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureColorShoes1.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureColorShoes1.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.refereeBindingSource, "shoecolorcode1", true));
		this.pictureColorShoes1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorShoes1.Location = new System.Drawing.Point(12, 131);
		this.pictureColorShoes1.Name = "pictureColorShoes1";
		this.pictureColorShoes1.Size = new System.Drawing.Size(20, 20);
		this.pictureColorShoes1.TabIndex = 62;
		this.pictureColorShoes1.TabStop = false;
		this.pictureColorShoes1.Click += new System.EventHandler(pictureColorShoes1_Click);
		this.numericShoesBrand.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.refereeBindingSource, "shoetypecode", true));
		this.numericShoesBrand.Location = new System.Drawing.Point(12, 36);
		this.numericShoesBrand.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
		this.numericShoesBrand.Name = "numericShoesBrand";
		this.numericShoesBrand.Size = new System.Drawing.Size(80, 20);
		this.numericShoesBrand.TabIndex = 9;
		this.numericShoesBrand.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericShoesBrand.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericShoesBrand.ValueChanged += new System.EventHandler(numericShoesBrand_ValueChanged);
		this.labelShoesType.AutoSize = true;
		this.labelShoesType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelShoesType.Location = new System.Drawing.Point(31, 18);
		this.labelShoesType.Name = "labelShoesType";
		this.labelShoesType.Size = new System.Drawing.Size(35, 13);
		this.labelShoesType.TabIndex = 60;
		this.labelShoesType.Text = "Brand";
		this.labelShoesType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelShoesColor.AutoSize = true;
		this.labelShoesColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelShoesColor.Location = new System.Drawing.Point(33, 113);
		this.labelShoesColor.Name = "labelShoesColor";
		this.labelShoesColor.Size = new System.Drawing.Size(36, 13);
		this.labelShoesColor.TabIndex = 61;
		this.labelShoesColor.Text = "Colors";
		this.labelShoesColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericShoesDesign.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.refereeBindingSource, "shoedesigncode", true));
		this.numericShoesDesign.Location = new System.Drawing.Point(12, 82);
		this.numericShoesDesign.Maximum = new decimal(new int[4] { 3, 0, 0, 0 });
		this.numericShoesDesign.Name = "numericShoesDesign";
		this.numericShoesDesign.Size = new System.Drawing.Size(80, 20);
		this.numericShoesDesign.TabIndex = 10;
		this.numericShoesDesign.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericShoesDesign.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericShoesDesign.ValueChanged += new System.EventHandler(numericShoesDesign_ValueChanged);
		this.viewer2DShoes.AutoTransparency = false;
		this.viewer2DShoes.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DShoes.ButtonStripVisible = false;
		this.viewer2DShoes.CurrentBitmap = null;
		this.viewer2DShoes.ExtendedFormat = false;
		this.viewer2DShoes.FullSizeButton = false;
		this.viewer2DShoes.ImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.viewer2DShoes.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DShoes.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.Double;
		this.viewer2DShoes.Location = new System.Drawing.Point(107, 37);
		this.viewer2DShoes.Name = "viewer2DShoes";
		this.viewer2DShoes.RemoveButton = false;
		this.viewer2DShoes.ShowButton = false;
		this.viewer2DShoes.ShowButtonChecked = true;
		this.viewer2DShoes.Size = new System.Drawing.Size(128, 128);
		this.viewer2DShoes.TabIndex = 59;
		this.labelShoes.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelShoes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelShoes.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelShoes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelShoes.Location = new System.Drawing.Point(106, 14);
		this.labelShoes.Name = "labelShoes";
		this.labelShoes.Size = new System.Drawing.Size(131, 20);
		this.labelShoes.TabIndex = 47;
		this.labelShoes.Text = "Shoes";
		this.labelShoes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShoes.DoubleClick += new System.EventHandler(labelShoes_DoubleClick);
		this.comboBox1.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.refereeBindingSource, "cardstrictness", true));
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Items.AddRange(new object[3] { "Tolerant", "Balanced", "Easy Card" });
		this.comboBox1.Location = new System.Drawing.Point(357, 42);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(132, 21);
		this.comboBox1.TabIndex = 188;
		this.label3.AutoSize = true;
		this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label3.Location = new System.Drawing.Point(266, 46);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(60, 13);
		this.label3.TabIndex = 187;
		this.label3.Text = "Cards Style";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboStyle.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.refereeBindingSource, "foulstrictness", true));
		this.comboStyle.FormattingEnabled = true;
		this.comboStyle.Items.AddRange(new object[3] { "Let Play", "Balanced", "Easy Whistle" });
		this.comboStyle.Location = new System.Drawing.Point(357, 17);
		this.comboStyle.Name = "comboStyle";
		this.comboStyle.Size = new System.Drawing.Size(132, 21);
		this.comboStyle.TabIndex = 186;
		this.labelStyle.AutoSize = true;
		this.labelStyle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStyle.Location = new System.Drawing.Point(266, 19);
		this.labelStyle.Name = "labelStyle";
		this.labelStyle.Size = new System.Drawing.Size(58, 13);
		this.labelStyle.TabIndex = 185;
		this.labelStyle.Text = "Fouls Style";
		this.labelStyle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.domainSleeves.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.refereeBindingSource, "jerseysleevelengthcode", true));
		this.domainSleeves.Items.Add("Short");
		this.domainSleeves.Items.Add("Long");
		this.domainSleeves.Location = new System.Drawing.Point(357, 68);
		this.domainSleeves.Name = "domainSleeves";
		this.domainSleeves.Size = new System.Drawing.Size(132, 20);
		this.domainSleeves.TabIndex = 175;
		this.domainSleeves.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.domainSleeves.Wrap = true;
		this.labelSleeves.AutoSize = true;
		this.labelSleeves.BackColor = System.Drawing.Color.Transparent;
		this.labelSleeves.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSleeves.Location = new System.Drawing.Point(266, 70);
		this.labelSleeves.Name = "labelSleeves";
		this.labelSleeves.Size = new System.Drawing.Size(81, 13);
		this.labelSleeves.TabIndex = 176;
		this.labelSleeves.Text = "Sleeves Length";
		this.labelSleeves.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboBody.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.refereeBindingSource, "bodytypecode", true));
		this.comboBody.FormattingEnabled = true;
		this.comboBody.Items.AddRange(new object[3] { "Small", "Normal", "Big" });
		this.comboBody.Location = new System.Drawing.Point(96, 225);
		this.comboBody.Name = "comboBody";
		this.comboBody.Size = new System.Drawing.Size(132, 21);
		this.comboBody.TabIndex = 174;
		this.numericHeight.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.refereeBindingSource, "height", true));
		this.numericHeight.Location = new System.Drawing.Point(96, 173);
		this.numericHeight.Maximum = new decimal(new int[4] { 215, 0, 0, 0 });
		this.numericHeight.Minimum = new decimal(new int[4] { 150, 0, 0, 0 });
		this.numericHeight.Name = "numericHeight";
		this.numericHeight.Size = new System.Drawing.Size(132, 20);
		this.numericHeight.TabIndex = 169;
		this.numericHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericHeight.Value = new decimal(new int[4] { 150, 0, 0, 0 });
		this.numericWeight.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.refereeBindingSource, "weight", true));
		this.numericWeight.Location = new System.Drawing.Point(96, 199);
		this.numericWeight.Maximum = new decimal(new int[4] { 115, 0, 0, 0 });
		this.numericWeight.Minimum = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericWeight.Name = "numericWeight";
		this.numericWeight.Size = new System.Drawing.Size(132, 20);
		this.numericWeight.TabIndex = 170;
		this.numericWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericWeight.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.labelWeight.AutoSize = true;
		this.labelWeight.BackColor = System.Drawing.Color.Transparent;
		this.labelWeight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelWeight.Location = new System.Drawing.Point(6, 201);
		this.labelWeight.Name = "labelWeight";
		this.labelWeight.Size = new System.Drawing.Size(41, 13);
		this.labelWeight.TabIndex = 172;
		this.labelWeight.Text = "Weight";
		this.labelWeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelBody.AutoSize = true;
		this.labelBody.BackColor = System.Drawing.Color.Transparent;
		this.labelBody.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBody.Location = new System.Drawing.Point(5, 228);
		this.labelBody.Name = "labelBody";
		this.labelBody.Size = new System.Drawing.Size(31, 13);
		this.labelBody.TabIndex = 173;
		this.labelBody.Text = "Body";
		this.labelBody.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelHeight.AutoSize = true;
		this.labelHeight.BackColor = System.Drawing.Color.Transparent;
		this.labelHeight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHeight.Location = new System.Drawing.Point(5, 175);
		this.labelHeight.Name = "labelHeight";
		this.labelHeight.Size = new System.Drawing.Size(38, 13);
		this.labelHeight.TabIndex = 171;
		this.labelHeight.Text = "Height";
		this.labelHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonGetId.Location = new System.Drawing.Point(204, 17);
		this.buttonGetId.Name = "buttonGetId";
		this.buttonGetId.Size = new System.Drawing.Size(24, 20);
		this.buttonGetId.TabIndex = 168;
		this.buttonGetId.Text = "...";
		this.buttonGetId.UseVisualStyleBackColor = true;
		this.buttonGetId.Click += new System.EventHandler(buttonGetId_Click);
		this.numericRefereeId.Location = new System.Drawing.Point(98, 17);
		this.numericRefereeId.Maximum = new decimal(new int[4] { 600000, 0, 0, 0 });
		this.numericRefereeId.Name = "numericRefereeId";
		this.numericRefereeId.Size = new System.Drawing.Size(91, 20);
		this.numericRefereeId.TabIndex = 167;
		this.numericRefereeId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericRefereeId.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericRefereeId.ValueChanged += new System.EventHandler(numericRefereeId_ValueChanged);
		this.buttonRandomizeIdentity.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomizeIdentity.Location = new System.Drawing.Point(10, 252);
		this.buttonRandomizeIdentity.Name = "buttonRandomizeIdentity";
		this.buttonRandomizeIdentity.Size = new System.Drawing.Size(218, 23);
		this.buttonRandomizeIdentity.TabIndex = 166;
		this.buttonRandomizeIdentity.Text = "Randomize";
		this.buttonRandomizeIdentity.UseVisualStyleBackColor = true;
		this.buttonRandomizeIdentity.Visible = false;
		this.buttonRandomizeIdentity.Click += new System.EventHandler(buttonRandomizeIdentity_Click);
		this.dateBirthDate.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.refereeBindingSource, "birthdate", true));
		this.dateBirthDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateBirthDate.Location = new System.Drawing.Point(97, 94);
		this.dateBirthDate.MaxDate = new System.DateTime(2006, 12, 31, 0, 0, 0, 0);
		this.dateBirthDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateBirthDate.Name = "dateBirthDate";
		this.dateBirthDate.Size = new System.Drawing.Size(131, 20);
		this.dateBirthDate.TabIndex = 161;
		this.dateBirthDate.Value = new System.DateTime(2006, 12, 31, 0, 0, 0, 0);
		this.labelBirthdate.AutoSize = true;
		this.labelBirthdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBirthdate.Location = new System.Drawing.Point(7, 98);
		this.labelBirthdate.Name = "labelBirthdate";
		this.labelBirthdate.Size = new System.Drawing.Size(49, 13);
		this.labelBirthdate.TabIndex = 163;
		this.labelBirthdate.Text = "Birthdate";
		this.labelBirthdate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelRefereeId.AutoSize = true;
		this.labelRefereeId.BackColor = System.Drawing.Color.Transparent;
		this.labelRefereeId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRefereeId.Location = new System.Drawing.Point(7, 21);
		this.labelRefereeId.Name = "labelRefereeId";
		this.labelRefereeId.Size = new System.Drawing.Size(57, 13);
		this.labelRefereeId.TabIndex = 165;
		this.labelRefereeId.Text = "Referee Id";
		this.labelRefereeId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textSurname.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.refereeBindingSource, "surname", true));
		this.textSurname.Location = new System.Drawing.Point(97, 68);
		this.textSurname.Name = "textSurname";
		this.textSurname.Size = new System.Drawing.Size(131, 20);
		this.textSurname.TabIndex = 159;
		this.textSurname.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textSurname.TextChanged += new System.EventHandler(textSurname_TextChanged);
		this.textFirstName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.refereeBindingSource, "firstname", true));
		this.textFirstName.Location = new System.Drawing.Point(98, 42);
		this.textFirstName.Name = "textFirstName";
		this.textFirstName.Size = new System.Drawing.Size(131, 20);
		this.textFirstName.TabIndex = 157;
		this.textFirstName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textFirstName.TextChanged += new System.EventHandler(textFirstName_TextChanged);
		this.comboCountry.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.refereeBindingSource, "Country", true));
		this.comboCountry.DataSource = this.countryListBindingSource;
		this.comboCountry.ItemHeight = 13;
		this.comboCountry.Location = new System.Drawing.Point(97, 120);
		this.comboCountry.MaxLength = 32767;
		this.comboCountry.Name = "comboCountry";
		this.comboCountry.Size = new System.Drawing.Size(131, 21);
		this.comboCountry.TabIndex = 162;
		this.countryListBindingSource.DataSource = typeof(FifaLibrary.CountryList);
		this.labelFirstName.AutoSize = true;
		this.labelFirstName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFirstName.Location = new System.Drawing.Point(7, 45);
		this.labelFirstName.Name = "labelFirstName";
		this.labelFirstName.Size = new System.Drawing.Size(57, 13);
		this.labelFirstName.TabIndex = 158;
		this.labelFirstName.Text = "First Name";
		this.labelFirstName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelSurame.AutoSize = true;
		this.labelSurame.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSurame.Location = new System.Drawing.Point(6, 71);
		this.labelSurame.Name = "labelSurame";
		this.labelSurame.Size = new System.Drawing.Size(58, 13);
		this.labelSurame.TabIndex = 160;
		this.labelSurame.Text = "Last Name";
		this.labelSurame.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCountry.AutoSize = true;
		this.labelCountry.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelCountry.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelCountry.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCountry.Location = new System.Drawing.Point(7, 119);
		this.labelCountry.Name = "labelCountry";
		this.labelCountry.Size = new System.Drawing.Size(43, 13);
		this.labelCountry.TabIndex = 164;
		this.labelCountry.Text = "Country";
		this.labelCountry.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCountry.DoubleClick += new System.EventHandler(labelCountry_DoubleClick);
		this.groupLeagues.Controls.Add(this.comboLeague4);
		this.groupLeagues.Controls.Add(this.comboLeague7);
		this.groupLeagues.Controls.Add(this.comboLeague5);
		this.groupLeagues.Controls.Add(this.comboLeague6);
		this.groupLeagues.Controls.Add(this.comboLeague0);
		this.groupLeagues.Controls.Add(this.comboLeague3);
		this.groupLeagues.Controls.Add(this.comboLeague1);
		this.groupLeagues.Controls.Add(this.comboLeague2);
		this.groupLeagues.Location = new System.Drawing.Point(3, 291);
		this.groupLeagues.Name = "groupLeagues";
		this.groupLeagues.Size = new System.Drawing.Size(512, 134);
		this.groupLeagues.TabIndex = 192;
		this.groupLeagues.TabStop = false;
		this.groupLeagues.Text = "Leagues";
		this.comboLeague4.FormattingEnabled = true;
		this.comboLeague4.Location = new System.Drawing.Point(264, 19);
		this.comboLeague4.Name = "comboLeague4";
		this.comboLeague4.Size = new System.Drawing.Size(204, 21);
		this.comboLeague4.TabIndex = 192;
		this.comboLeague4.SelectedIndexChanged += new System.EventHandler(comboLeague4_SelectedIndexChanged);
		this.comboLeague7.FormattingEnabled = true;
		this.comboLeague7.Location = new System.Drawing.Point(264, 99);
		this.comboLeague7.Name = "comboLeague7";
		this.comboLeague7.Size = new System.Drawing.Size(204, 21);
		this.comboLeague7.TabIndex = 195;
		this.comboLeague7.SelectedIndexChanged += new System.EventHandler(comboLeague7_SelectedIndexChanged);
		this.comboLeague5.FormattingEnabled = true;
		this.comboLeague5.Location = new System.Drawing.Point(264, 46);
		this.comboLeague5.Name = "comboLeague5";
		this.comboLeague5.Size = new System.Drawing.Size(204, 21);
		this.comboLeague5.TabIndex = 193;
		this.comboLeague5.SelectedIndexChanged += new System.EventHandler(comboLeague5_SelectedIndexChanged);
		this.comboLeague6.FormattingEnabled = true;
		this.comboLeague6.Location = new System.Drawing.Point(264, 73);
		this.comboLeague6.Name = "comboLeague6";
		this.comboLeague6.Size = new System.Drawing.Size(204, 21);
		this.comboLeague6.TabIndex = 194;
		this.comboLeague6.SelectedIndexChanged += new System.EventHandler(comboLeague6_SelectedIndexChanged);
		this.comboLeague0.FormattingEnabled = true;
		this.comboLeague0.Location = new System.Drawing.Point(9, 19);
		this.comboLeague0.Name = "comboLeague0";
		this.comboLeague0.Size = new System.Drawing.Size(204, 21);
		this.comboLeague0.TabIndex = 183;
		this.comboLeague0.SelectedIndexChanged += new System.EventHandler(comboLeague0_SelectedIndexChanged);
		this.comboLeague3.FormattingEnabled = true;
		this.comboLeague3.Location = new System.Drawing.Point(9, 99);
		this.comboLeague3.Name = "comboLeague3";
		this.comboLeague3.Size = new System.Drawing.Size(204, 21);
		this.comboLeague3.TabIndex = 191;
		this.comboLeague3.SelectedIndexChanged += new System.EventHandler(comboLeague3_SelectedIndexChanged);
		this.comboLeague1.FormattingEnabled = true;
		this.comboLeague1.Location = new System.Drawing.Point(9, 46);
		this.comboLeague1.Name = "comboLeague1";
		this.comboLeague1.Size = new System.Drawing.Size(204, 21);
		this.comboLeague1.TabIndex = 189;
		this.comboLeague1.SelectedIndexChanged += new System.EventHandler(comboLeague1_SelectedIndexChanged);
		this.comboLeague2.FormattingEnabled = true;
		this.comboLeague2.Location = new System.Drawing.Point(9, 73);
		this.comboLeague2.Name = "comboLeague2";
		this.comboLeague2.Size = new System.Drawing.Size(204, 21);
		this.comboLeague2.TabIndex = 190;
		this.comboLeague2.SelectedIndexChanged += new System.EventHandler(comboLeague2_SelectedIndexChanged);
		this.viewer2DPlayerGui.AutoTransparency = true;
		this.viewer2DPlayerGui.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPlayerGui.ButtonStripVisible = true;
		this.viewer2DPlayerGui.CurrentBitmap = null;
		this.viewer2DPlayerGui.ExtendedFormat = false;
		this.viewer2DPlayerGui.FullSizeButton = false;
		this.viewer2DPlayerGui.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DPlayerGui.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DPlayerGui.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DPlayerGui.Location = new System.Drawing.Point(3, 431);
		this.viewer2DPlayerGui.Name = "viewer2DPlayerGui";
		this.viewer2DPlayerGui.RemoveButton = false;
		this.viewer2DPlayerGui.ShowButton = false;
		this.viewer2DPlayerGui.ShowButtonChecked = true;
		this.viewer2DPlayerGui.Size = new System.Drawing.Size(256, 225);
		this.viewer2DPlayerGui.TabIndex = 193;
		this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer2.Location = new System.Drawing.Point(0, 0);
		this.splitContainer2.Name = "splitContainer2";
		this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer2.Panel1.Controls.Add(this.tool3D);
		this.splitContainer2.Panel2.AutoScroll = true;
		this.splitContainer2.Panel2.Controls.Add(this.buttonRandomizeAllReferees);
		this.splitContainer2.Panel2.Controls.Add(this.groupGenericFace);
		this.splitContainer2.Size = new System.Drawing.Size(826, 807);
		this.splitContainer2.SplitterDistance = 483;
		this.splitContainer2.TabIndex = 0;
		this.tool3D.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.tool3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.tool3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.buttonShow3DModel, this.buttonSwitchRenderingMode, this.toolStripSeparator1, this.toolPhoto });
		this.tool3D.Location = new System.Drawing.Point(0, 458);
		this.tool3D.Name = "tool3D";
		this.tool3D.Size = new System.Drawing.Size(826, 25);
		this.tool3D.TabIndex = 6;
		this.buttonShow3DModel.CheckOnClick = true;
		this.buttonShow3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonShow3DModel.Image");
		this.buttonShow3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow3DModel.Name = "buttonShow3DModel";
		this.buttonShow3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonShow3DModel.Text = "Show / Hide";
		this.buttonShow3DModel.Click += new System.EventHandler(buttonShow3DModel_Click);
		this.buttonSwitchRenderingMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSwitchRenderingMode.Image = (System.Drawing.Image)resources.GetObject("buttonSwitchRenderingMode.Image");
		this.buttonSwitchRenderingMode.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSwitchRenderingMode.Name = "buttonSwitchRenderingMode";
		this.buttonSwitchRenderingMode.Size = new System.Drawing.Size(23, 22);
		this.buttonSwitchRenderingMode.Text = "Switch Rendering Mode";
		this.buttonSwitchRenderingMode.Click += new System.EventHandler(buttonSwitchRenderingMode_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.toolPhoto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolPhoto.Image = (System.Drawing.Image)resources.GetObject("toolPhoto.Image");
		this.toolPhoto.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolPhoto.Name = "toolPhoto";
		this.toolPhoto.Size = new System.Drawing.Size(23, 22);
		this.toolPhoto.Text = "Take a picture";
		this.toolPhoto.Click += new System.EventHandler(toolPhoto_Click);
		this.groupGenericFace.Controls.Add(this.groupTextureInfo);
		this.groupGenericFace.Controls.Add(this.groupHairModel);
		this.groupGenericFace.Controls.Add(this.groupHeadModel);
		this.groupGenericFace.Controls.Add(this.labelHeadType);
		this.groupGenericFace.Controls.Add(this.labelHairType);
		this.groupGenericFace.Location = new System.Drawing.Point(3, 3);
		this.groupGenericFace.Name = "groupGenericFace";
		this.groupGenericFace.Size = new System.Drawing.Size(590, 246);
		this.groupGenericFace.TabIndex = 87;
		this.groupGenericFace.TabStop = false;
		this.groupGenericFace.Text = "Face Modelling";
		this.groupTextureInfo.Controls.Add(this.comboSkinColor);
		this.groupTextureInfo.Controls.Add(this.labelFacialHair);
		this.groupTextureInfo.Controls.Add(this.labelEyeBow);
		this.groupTextureInfo.Controls.Add(this.domainFacialHair);
		this.groupTextureInfo.Controls.Add(this.comboEyeBow);
		this.groupTextureInfo.Controls.Add(this.labelSideburns);
		this.groupTextureInfo.Controls.Add(this.comboSideburns);
		this.groupTextureInfo.Controls.Add(this.labelSkintype);
		this.groupTextureInfo.Controls.Add(this.comboEyescolor);
		this.groupTextureInfo.Controls.Add(this.comboSkintype);
		this.groupTextureInfo.Controls.Add(this.label2);
		this.groupTextureInfo.Controls.Add(this.label1);
		this.groupTextureInfo.Controls.Add(this.comboFacialHairColor);
		this.groupTextureInfo.Controls.Add(this.labelFacialHairColor);
		this.groupTextureInfo.Location = new System.Drawing.Point(381, 19);
		this.groupTextureInfo.Name = "groupTextureInfo";
		this.groupTextureInfo.Size = new System.Drawing.Size(200, 217);
		this.groupTextureInfo.TabIndex = 35;
		this.groupTextureInfo.TabStop = false;
		this.groupTextureInfo.Text = "Face Type";
		this.comboSkinColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboSkinColor.FormattingEnabled = true;
		this.comboSkinColor.Items.AddRange(new object[10] { "1 = unused", "Pink", "3 = unused", "Llight Yellow", "Medium Yellow", "Dark Yellow", "7 = unused", "Light Brown", "Medium Brown", "Dark brown" });
		this.comboSkinColor.Location = new System.Drawing.Point(77, 22);
		this.comboSkinColor.Name = "comboSkinColor";
		this.comboSkinColor.Size = new System.Drawing.Size(111, 21);
		this.comboSkinColor.TabIndex = 20;
		this.comboSkinColor.SelectedIndexChanged += new System.EventHandler(comboSkinColor_SelectedIndexChanged);
		this.labelFacialHair.AutoSize = true;
		this.labelFacialHair.BackColor = System.Drawing.SystemColors.Control;
		this.labelFacialHair.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFacialHair.Location = new System.Drawing.Point(6, 136);
		this.labelFacialHair.Name = "labelFacialHair";
		this.labelFacialHair.Size = new System.Drawing.Size(57, 13);
		this.labelFacialHair.TabIndex = 15;
		this.labelFacialHair.Text = "Facial Hair";
		this.labelFacialHair.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelEyeBow.AutoSize = true;
		this.labelEyeBow.BackColor = System.Drawing.SystemColors.Control;
		this.labelEyeBow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelEyeBow.Location = new System.Drawing.Point(6, 108);
		this.labelEyeBow.Name = "labelEyeBow";
		this.labelEyeBow.Size = new System.Drawing.Size(57, 13);
		this.labelEyeBow.TabIndex = 33;
		this.labelEyeBow.Text = "Eyes Brow";
		this.labelEyeBow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.domainFacialHair.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.domainFacialHair.FormattingEnabled = true;
		this.domainFacialHair.Items.AddRange(new object[16]
		{
			"None", "Chin Stubble Light", "Chin Strap", "Goatee", "Casual Beard", "Partial Goatee", "Stubble", "Tuft", "Full Beard", "Light Goatee",
			"Mustache", "Light Chin Curtain", "Full Goatee", "Chin Curtain", "Beard", "Patchy Beard"
		});
		this.domainFacialHair.Location = new System.Drawing.Point(77, 133);
		this.domainFacialHair.Name = "domainFacialHair";
		this.domainFacialHair.Size = new System.Drawing.Size(111, 21);
		this.domainFacialHair.TabIndex = 16;
		this.domainFacialHair.SelectedIndexChanged += new System.EventHandler(domainFacialHair_SelectedItemChanged);
		this.comboEyeBow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboEyeBow.FormattingEnabled = true;
		this.comboEyeBow.Items.AddRange(new object[7] { "Normal", "Big", "Thin", "Type Female 3", "Type Female 4", "Type Female 5", "Type Female 6" });
		this.comboEyeBow.Location = new System.Drawing.Point(77, 105);
		this.comboEyeBow.Name = "comboEyeBow";
		this.comboEyeBow.Size = new System.Drawing.Size(111, 21);
		this.comboEyeBow.TabIndex = 32;
		this.comboEyeBow.SelectedIndexChanged += new System.EventHandler(comboEyeBow_SelectedIndexChanged);
		this.labelSideburns.AutoSize = true;
		this.labelSideburns.BackColor = System.Drawing.SystemColors.Control;
		this.labelSideburns.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSideburns.Location = new System.Drawing.Point(6, 192);
		this.labelSideburns.Name = "labelSideburns";
		this.labelSideburns.Size = new System.Drawing.Size(54, 13);
		this.labelSideburns.TabIndex = 23;
		this.labelSideburns.Text = "Sideburns";
		this.labelSideburns.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelSideburns.Visible = false;
		this.comboSideburns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboSideburns.FormattingEnabled = true;
		this.comboSideburns.Items.AddRange(new object[2] { "No", "Yes" });
		this.comboSideburns.Location = new System.Drawing.Point(77, 189);
		this.comboSideburns.Name = "comboSideburns";
		this.comboSideburns.Size = new System.Drawing.Size(111, 21);
		this.comboSideburns.TabIndex = 24;
		this.comboSideburns.Visible = false;
		this.comboSideburns.SelectedIndexChanged += new System.EventHandler(comboSideburns_SelectedIndexChanged);
		this.labelSkintype.AutoSize = true;
		this.labelSkintype.BackColor = System.Drawing.SystemColors.Control;
		this.labelSkintype.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSkintype.Location = new System.Drawing.Point(6, 52);
		this.labelSkintype.Name = "labelSkintype";
		this.labelSkintype.Size = new System.Drawing.Size(55, 13);
		this.labelSkintype.TabIndex = 21;
		this.labelSkintype.Text = "Skin Type";
		this.labelSkintype.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboEyescolor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboEyescolor.FormattingEnabled = true;
		this.comboEyescolor.Items.AddRange(new object[10] { "Dark Blue", "Light Blue", "Dark Brown", "Light Brown", "Brown and Green", "Dark Green", "Light Green", "Gray", "Black", "Dark Gray" });
		this.comboEyescolor.Location = new System.Drawing.Point(77, 77);
		this.comboEyescolor.Name = "comboEyescolor";
		this.comboEyescolor.Size = new System.Drawing.Size(111, 21);
		this.comboEyescolor.TabIndex = 26;
		this.comboEyescolor.SelectedIndexChanged += new System.EventHandler(comboEyescolor_SelectedIndexChanged);
		this.comboSkintype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboSkintype.FormattingEnabled = true;
		this.comboSkintype.Items.AddRange(new object[8] { "Clean", "Freckled", "Rough", "Type Female 3", "Type Female 4", "Type Female 5", "Type Female 6", "Type Female 7" });
		this.comboSkintype.Location = new System.Drawing.Point(77, 49);
		this.comboSkintype.Name = "comboSkintype";
		this.comboSkintype.Size = new System.Drawing.Size(111, 21);
		this.comboSkintype.TabIndex = 22;
		this.comboSkintype.SelectedIndexChanged += new System.EventHandler(comboSkintype_SelectedIndexChanged);
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.SystemColors.Control;
		this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label2.Location = new System.Drawing.Point(6, 80);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(57, 13);
		this.label2.TabIndex = 25;
		this.label2.Text = "Eyes Color";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.SystemColors.Control;
		this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1.Location = new System.Drawing.Point(6, 27);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(55, 13);
		this.label1.TabIndex = 19;
		this.label1.Text = "Skin Color";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboFacialHairColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboFacialHairColor.FormattingEnabled = true;
		this.comboFacialHairColor.Items.AddRange(new object[5] { "Black", "Blonde", "Dark brown", "Light brown", "Red" });
		this.comboFacialHairColor.Location = new System.Drawing.Point(77, 161);
		this.comboFacialHairColor.Name = "comboFacialHairColor";
		this.comboFacialHairColor.Size = new System.Drawing.Size(111, 21);
		this.comboFacialHairColor.TabIndex = 18;
		this.comboFacialHairColor.SelectedIndexChanged += new System.EventHandler(comboFacialHairColor_SelectedIndexChanged);
		this.labelFacialHairColor.AutoSize = true;
		this.labelFacialHairColor.BackColor = System.Drawing.SystemColors.Control;
		this.labelFacialHairColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFacialHairColor.Location = new System.Drawing.Point(6, 164);
		this.labelFacialHairColor.Name = "labelFacialHairColor";
		this.labelFacialHairColor.Size = new System.Drawing.Size(31, 13);
		this.labelFacialHairColor.TabIndex = 17;
		this.labelFacialHairColor.Text = "Color";
		this.labelFacialHairColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupHairModel.Controls.Add(this.comboHeadband);
		this.groupHairModel.Controls.Add(this.comboAfro);
		this.groupHairModel.Controls.Add(this.comboLong);
		this.groupHairModel.Controls.Add(this.comboMedium);
		this.groupHairModel.Controls.Add(this.comboModern);
		this.groupHairModel.Controls.Add(this.comboShort);
		this.groupHairModel.Controls.Add(this.comboVeryShort);
		this.groupHairModel.Controls.Add(this.comboShaven);
		this.groupHairModel.Controls.Add(this.radioHeadband);
		this.groupHairModel.Controls.Add(this.radioShaven);
		this.groupHairModel.Controls.Add(this.radioAfro);
		this.groupHairModel.Controls.Add(this.radioLong);
		this.groupHairModel.Controls.Add(this.radioMedium);
		this.groupHairModel.Controls.Add(this.radioModern);
		this.groupHairModel.Controls.Add(this.radioShort);
		this.groupHairModel.Controls.Add(this.radioVeryShort);
		this.groupHairModel.Controls.Add(this.domainHairColor);
		this.groupHairModel.Controls.Add(this.labelHairColor);
		this.groupHairModel.Location = new System.Drawing.Point(6, 104);
		this.groupHairModel.Name = "groupHairModel";
		this.groupHairModel.Size = new System.Drawing.Size(364, 132);
		this.groupHairModel.TabIndex = 29;
		this.groupHairModel.TabStop = false;
		this.groupHairModel.Text = "Hair Model";
		this.comboHeadband.FormattingEnabled = true;
		this.comboHeadband.Items.AddRange(new object[6] { "55", "56", "76", "81", "49", "91" });
		this.comboHeadband.Location = new System.Drawing.Point(6, 76);
		this.comboHeadband.Name = "comboHeadband";
		this.comboHeadband.Size = new System.Drawing.Size(260, 21);
		this.comboHeadband.TabIndex = 30;
		this.comboHeadband.Visible = false;
		this.comboHeadband.SelectedIndexChanged += new System.EventHandler(comboHeadband_SelectedIndexChanged);
		this.comboAfro.FormattingEnabled = true;
		this.comboAfro.Items.AddRange(new object[8] { "71", "4", "42", "27", "5", "6", "96", "3" });
		this.comboAfro.Location = new System.Drawing.Point(6, 76);
		this.comboAfro.Name = "comboAfro";
		this.comboAfro.Size = new System.Drawing.Size(260, 21);
		this.comboAfro.TabIndex = 29;
		this.comboAfro.Visible = false;
		this.comboAfro.SelectedIndexChanged += new System.EventHandler(comboAfro_SelectedIndexChanged);
		this.comboLong.FormattingEnabled = true;
		this.comboLong.Items.AddRange(new object[16]
		{
			"8", "9", "15", "44", "84", "34", "10", "33", "12", "80",
			"11", "51", "79", "53", "7", "48"
		});
		this.comboLong.Location = new System.Drawing.Point(6, 76);
		this.comboLong.Name = "comboLong";
		this.comboLong.Size = new System.Drawing.Size(260, 21);
		this.comboLong.TabIndex = 28;
		this.comboLong.Visible = false;
		this.comboLong.SelectedIndexChanged += new System.EventHandler(comboLong_SelectedIndexChanged);
		this.comboMedium.FormattingEnabled = true;
		this.comboMedium.Items.AddRange(new object[27]
		{
			"36", "74", "13", "35", "59", "69", "73", "85", "93", "32",
			"66", "67", "68", "14", "20", "23", "58", "62", "83", "95",
			"22", "52", "87", "98", "99", "100", "103"
		});
		this.comboMedium.Location = new System.Drawing.Point(6, 76);
		this.comboMedium.Name = "comboMedium";
		this.comboMedium.Size = new System.Drawing.Size(260, 21);
		this.comboMedium.TabIndex = 27;
		this.comboMedium.Visible = false;
		this.comboMedium.SelectedIndexChanged += new System.EventHandler(comboMedium_SelectedIndexChanged);
		this.comboModern.FormattingEnabled = true;
		this.comboModern.Items.AddRange(new object[13]
		{
			"17", "18", "19", "24", "39", "60", "61", "63", "64", "86",
			"88", "89", "94"
		});
		this.comboModern.Location = new System.Drawing.Point(6, 76);
		this.comboModern.Name = "comboModern";
		this.comboModern.Size = new System.Drawing.Size(260, 21);
		this.comboModern.TabIndex = 26;
		this.comboModern.Visible = false;
		this.comboModern.SelectedIndexChanged += new System.EventHandler(comboModern_SelectedIndexChanged);
		this.comboShort.FormattingEnabled = true;
		this.comboShort.Items.AddRange(new object[23]
		{
			"2", "21", "22", "30", "38", "54", "57", "70", "75", "78",
			"82", "97", "101", "102", "104", "105", "106", "107", "108", "109",
			"110", "111", "112"
		});
		this.comboShort.Location = new System.Drawing.Point(6, 76);
		this.comboShort.Name = "comboShort";
		this.comboShort.Size = new System.Drawing.Size(260, 21);
		this.comboShort.TabIndex = 25;
		this.comboShort.Visible = false;
		this.comboShort.SelectedIndexChanged += new System.EventHandler(comboShort_SelectedIndexChanged);
		this.comboVeryShort.FormattingEnabled = true;
		this.comboVeryShort.Items.AddRange(new object[14]
		{
			"26", "29", "47", "72", "92", "16", "28", "31", "37", "40",
			"45", "65", "77", "90"
		});
		this.comboVeryShort.Location = new System.Drawing.Point(6, 76);
		this.comboVeryShort.Name = "comboVeryShort";
		this.comboVeryShort.Size = new System.Drawing.Size(260, 21);
		this.comboVeryShort.TabIndex = 24;
		this.comboVeryShort.Visible = false;
		this.comboVeryShort.SelectedIndexChanged += new System.EventHandler(comboVeryShort_SelectedIndexChanged);
		this.comboShaven.FormattingEnabled = true;
		this.comboShaven.Items.AddRange(new object[6] { "0", "25", "1", "43", "41", "46" });
		this.comboShaven.Location = new System.Drawing.Point(6, 76);
		this.comboShaven.Name = "comboShaven";
		this.comboShaven.Size = new System.Drawing.Size(260, 21);
		this.comboShaven.TabIndex = 23;
		this.comboShaven.Visible = false;
		this.comboShaven.SelectedIndexChanged += new System.EventHandler(comboShaven_SelectedIndexChanged);
		this.radioHeadband.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioHeadband.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioHeadband.Location = new System.Drawing.Point(136, 40);
		this.radioHeadband.Name = "radioHeadband";
		this.radioHeadband.Size = new System.Drawing.Size(65, 23);
		this.radioHeadband.TabIndex = 22;
		this.radioHeadband.TabStop = true;
		this.radioHeadband.Tag = this.comboHeadband;
		this.radioHeadband.Text = "Headband";
		this.radioHeadband.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioHeadband.UseVisualStyleBackColor = true;
		this.radioHeadband.CheckedChanged += new System.EventHandler(radioHeadband_CheckedChanged);
		this.radioShaven.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioShaven.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioShaven.Location = new System.Drawing.Point(6, 17);
		this.radioShaven.Name = "radioShaven";
		this.radioShaven.Size = new System.Drawing.Size(65, 23);
		this.radioShaven.TabIndex = 21;
		this.radioShaven.TabStop = true;
		this.radioShaven.Tag = this.comboShaven;
		this.radioShaven.Text = "Shaven";
		this.radioShaven.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioShaven.UseVisualStyleBackColor = true;
		this.radioShaven.CheckedChanged += new System.EventHandler(radioShaven_CheckedChanged);
		this.radioAfro.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioAfro.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioAfro.Location = new System.Drawing.Point(201, 40);
		this.radioAfro.Name = "radioAfro";
		this.radioAfro.Size = new System.Drawing.Size(65, 23);
		this.radioAfro.TabIndex = 20;
		this.radioAfro.TabStop = true;
		this.radioAfro.Tag = this.comboAfro;
		this.radioAfro.Text = "Afro";
		this.radioAfro.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioAfro.UseVisualStyleBackColor = true;
		this.radioAfro.CheckedChanged += new System.EventHandler(radioButtonAfro_CheckedChanged);
		this.radioLong.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioLong.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioLong.Location = new System.Drawing.Point(71, 40);
		this.radioLong.Name = "radioLong";
		this.radioLong.Size = new System.Drawing.Size(65, 23);
		this.radioLong.TabIndex = 19;
		this.radioLong.TabStop = true;
		this.radioLong.Tag = this.comboLong;
		this.radioLong.Text = "Long";
		this.radioLong.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioLong.UseVisualStyleBackColor = true;
		this.radioLong.CheckedChanged += new System.EventHandler(radioButtonLong_CheckedChanged);
		this.radioMedium.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioMedium.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioMedium.Location = new System.Drawing.Point(6, 40);
		this.radioMedium.Name = "radioMedium";
		this.radioMedium.Size = new System.Drawing.Size(65, 23);
		this.radioMedium.TabIndex = 18;
		this.radioMedium.TabStop = true;
		this.radioMedium.Tag = this.comboMedium;
		this.radioMedium.Text = "Medium";
		this.radioMedium.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioMedium.UseVisualStyleBackColor = true;
		this.radioMedium.CheckedChanged += new System.EventHandler(radioButtonMedium_CheckedChanged);
		this.radioModern.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioModern.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioModern.Location = new System.Drawing.Point(201, 17);
		this.radioModern.Name = "radioModern";
		this.radioModern.Size = new System.Drawing.Size(65, 23);
		this.radioModern.TabIndex = 17;
		this.radioModern.TabStop = true;
		this.radioModern.Tag = this.comboModern;
		this.radioModern.Text = "Modern";
		this.radioModern.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioModern.UseVisualStyleBackColor = true;
		this.radioModern.CheckedChanged += new System.EventHandler(radioModern_CheckedChanged);
		this.radioShort.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioShort.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioShort.Location = new System.Drawing.Point(136, 17);
		this.radioShort.Name = "radioShort";
		this.radioShort.Size = new System.Drawing.Size(65, 23);
		this.radioShort.TabIndex = 16;
		this.radioShort.TabStop = true;
		this.radioShort.Tag = this.comboShort;
		this.radioShort.Text = "Short";
		this.radioShort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioShort.UseVisualStyleBackColor = true;
		this.radioShort.CheckedChanged += new System.EventHandler(radioShort_CheckedChanged);
		this.radioVeryShort.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioVeryShort.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioVeryShort.Location = new System.Drawing.Point(71, 17);
		this.radioVeryShort.Name = "radioVeryShort";
		this.radioVeryShort.Size = new System.Drawing.Size(65, 23);
		this.radioVeryShort.TabIndex = 15;
		this.radioVeryShort.TabStop = true;
		this.radioVeryShort.Tag = this.comboVeryShort;
		this.radioVeryShort.Text = "Very Short";
		this.radioVeryShort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioVeryShort.UseVisualStyleBackColor = true;
		this.radioVeryShort.CheckedChanged += new System.EventHandler(radioVeryShort_CheckedChanged);
		this.domainHairColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.domainHairColor.FormattingEnabled = true;
		this.domainHairColor.Items.AddRange(new object[12]
		{
			"Blonde", "Black", "Dark Blonde", "Dark Brown", "Light Blonde", "Light Brown", "Brown", "Red", "White", "Gray",
			"Green", "Violet"
		});
		this.domainHairColor.Location = new System.Drawing.Point(155, 102);
		this.domainHairColor.Name = "domainHairColor";
		this.domainHairColor.Size = new System.Drawing.Size(111, 21);
		this.domainHairColor.TabIndex = 14;
		this.domainHairColor.SelectedIndexChanged += new System.EventHandler(domainHairColor_SelectedIndexChanged);
		this.labelHairColor.BackColor = System.Drawing.SystemColors.Control;
		this.labelHairColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHairColor.Location = new System.Drawing.Point(6, 101);
		this.labelHairColor.Name = "labelHairColor";
		this.labelHairColor.Size = new System.Drawing.Size(103, 20);
		this.labelHairColor.TabIndex = 13;
		this.labelHairColor.Text = "Hair Color";
		this.labelHairColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupHeadModel.Controls.Add(this.comboLatinModels);
		this.groupHeadModel.Controls.Add(this.radioButtonLatin);
		this.groupHeadModel.Controls.Add(this.comboAsiaticModels);
		this.groupHeadModel.Controls.Add(this.radioButtonAsiatic);
		this.groupHeadModel.Controls.Add(this.comboAfricanModels);
		this.groupHeadModel.Controls.Add(this.radioButtonAfrican);
		this.groupHeadModel.Controls.Add(this.radioButtonCaucasic);
		this.groupHeadModel.Controls.Add(this.comboCaucasicModels);
		this.groupHeadModel.Controls.Add(this.buttonRandomizeAppearance);
		this.groupHeadModel.Location = new System.Drawing.Point(6, 19);
		this.groupHeadModel.Name = "groupHeadModel";
		this.groupHeadModel.Size = new System.Drawing.Size(364, 79);
		this.groupHeadModel.TabIndex = 28;
		this.groupHeadModel.TabStop = false;
		this.groupHeadModel.Text = "Head Model";
		this.comboLatinModels.FormattingEnabled = true;
		this.comboLatinModels.Items.AddRange(new object[42]
		{
			"1500", "1501", "1502", "1503", "1504", "1505", "1506", "1507", "1508", "1509",
			"1510", "1511", "1512", "1513", "1514", "1515", "1516", "1517", "1518", "1519",
			"1520", "1521", "1522", "1523", "1524", "1525", "1526", "1527", "1528", "2500",
			"2501", "2502", "2503", "2504", "2505", "2506", "2507", "2508", "2509", "2510",
			"2511", "2512"
		});
		this.comboLatinModels.Location = new System.Drawing.Point(6, 48);
		this.comboLatinModels.Name = "comboLatinModels";
		this.comboLatinModels.Size = new System.Drawing.Size(260, 21);
		this.comboLatinModels.TabIndex = 3;
		this.comboLatinModels.Visible = false;
		this.comboLatinModels.SelectedIndexChanged += new System.EventHandler(comboLatinModels_SelectedIndexChanged);
		this.radioButtonLatin.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButtonLatin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButtonLatin.Location = new System.Drawing.Point(201, 19);
		this.radioButtonLatin.Name = "radioButtonLatin";
		this.radioButtonLatin.Size = new System.Drawing.Size(65, 23);
		this.radioButtonLatin.TabIndex = 8;
		this.radioButtonLatin.TabStop = true;
		this.radioButtonLatin.Tag = this.comboLatinModels;
		this.radioButtonLatin.Text = "Latin";
		this.radioButtonLatin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioButtonLatin.UseVisualStyleBackColor = true;
		this.radioButtonLatin.CheckedChanged += new System.EventHandler(radioButtonLatin_CheckedChanged);
		this.comboAsiaticModels.FormattingEnabled = true;
		this.comboAsiaticModels.Items.AddRange(new object[33]
		{
			"500", "501", "502", "503", "504", "505", "506", "507", "508", "509",
			"510", "511", "512", "513", "514", "515", "516", "517", "518", "519",
			"520", "521", "522", "523", "524", "525", "526", "527", "528", "529",
			"530", "531", "532"
		});
		this.comboAsiaticModels.Location = new System.Drawing.Point(6, 48);
		this.comboAsiaticModels.Name = "comboAsiaticModels";
		this.comboAsiaticModels.Size = new System.Drawing.Size(254, 21);
		this.comboAsiaticModels.TabIndex = 0;
		this.comboAsiaticModels.Visible = false;
		this.comboAsiaticModels.SelectedIndexChanged += new System.EventHandler(comboAsiaticModels_SelectedIndexChanged);
		this.radioButtonAsiatic.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButtonAsiatic.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButtonAsiatic.Location = new System.Drawing.Point(71, 19);
		this.radioButtonAsiatic.Name = "radioButtonAsiatic";
		this.radioButtonAsiatic.Size = new System.Drawing.Size(65, 23);
		this.radioButtonAsiatic.TabIndex = 6;
		this.radioButtonAsiatic.TabStop = true;
		this.radioButtonAsiatic.Tag = this.comboAsiaticModels;
		this.radioButtonAsiatic.Text = "Asiatic";
		this.radioButtonAsiatic.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioButtonAsiatic.UseVisualStyleBackColor = true;
		this.radioButtonAsiatic.CheckedChanged += new System.EventHandler(radioButtonAsiatic_CheckedChanged);
		this.comboAfricanModels.FormattingEnabled = true;
		this.comboAfricanModels.Items.AddRange(new object[36]
		{
			"1000", "1001", "1002", "1003", "1004", "1005", "1006", "1007", "1008", "1009",
			"1010", "1011", "1012", "1013", "1014", "1015", "1016", "1017", "1018", "1019",
			"1020", "1021", "3000", "3001", "3002", "3003", "3004", "3005", "4500", "4501",
			"4502", "4525", "5000", "5001", "5002", "5003"
		});
		this.comboAfricanModels.Location = new System.Drawing.Point(6, 48);
		this.comboAfricanModels.Name = "comboAfricanModels";
		this.comboAfricanModels.Size = new System.Drawing.Size(254, 21);
		this.comboAfricanModels.TabIndex = 1;
		this.comboAfricanModels.Visible = false;
		this.comboAfricanModels.SelectedIndexChanged += new System.EventHandler(comboAfricanModels_SelectedIndexChanged);
		this.radioButtonAfrican.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButtonAfrican.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButtonAfrican.Location = new System.Drawing.Point(6, 19);
		this.radioButtonAfrican.Name = "radioButtonAfrican";
		this.radioButtonAfrican.Size = new System.Drawing.Size(65, 23);
		this.radioButtonAfrican.TabIndex = 5;
		this.radioButtonAfrican.TabStop = true;
		this.radioButtonAfrican.Tag = this.comboAfricanModels;
		this.radioButtonAfrican.Text = "African";
		this.radioButtonAfrican.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioButtonAfrican.UseVisualStyleBackColor = true;
		this.radioButtonAfrican.CheckedChanged += new System.EventHandler(radioButtonAfrican_CheckedChanged);
		this.radioButtonCaucasic.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButtonCaucasic.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButtonCaucasic.Location = new System.Drawing.Point(136, 19);
		this.radioButtonCaucasic.Name = "radioButtonCaucasic";
		this.radioButtonCaucasic.Size = new System.Drawing.Size(65, 23);
		this.radioButtonCaucasic.TabIndex = 7;
		this.radioButtonCaucasic.TabStop = true;
		this.radioButtonCaucasic.Tag = this.comboCaucasicModels;
		this.radioButtonCaucasic.Text = "Caucasian";
		this.radioButtonCaucasic.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioButtonCaucasic.UseVisualStyleBackColor = true;
		this.radioButtonCaucasic.CheckedChanged += new System.EventHandler(radioButtonCaucasic_CheckedChanged);
		this.comboCaucasicModels.FormattingEnabled = true;
		this.comboCaucasicModels.Items.AddRange(new object[57]
		{
			"1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
			"11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
			"21", "22", "23", "24", "25", "2000", "2001", "2002", "2003", "2004",
			"2005", "2006", "2007", "2008", "2009", "2010", "2011", "2012", "2013", "2014",
			"2015", "2016", "2017", "2018", "2019", "2020", "2021", "3500", "3501", "3502",
			"3503", "3504", "3505", "4000", "4001", "4002", "4003"
		});
		this.comboCaucasicModels.Location = new System.Drawing.Point(6, 48);
		this.comboCaucasicModels.Name = "comboCaucasicModels";
		this.comboCaucasicModels.Size = new System.Drawing.Size(254, 21);
		this.comboCaucasicModels.TabIndex = 2;
		this.comboCaucasicModels.Visible = false;
		this.comboCaucasicModels.SelectedIndexChanged += new System.EventHandler(comboCaucasicModels_SelectedIndexChanged);
		this.buttonRandomizeAppearance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomizeAppearance.Location = new System.Drawing.Point(272, 18);
		this.buttonRandomizeAppearance.Name = "buttonRandomizeAppearance";
		this.buttonRandomizeAppearance.Size = new System.Drawing.Size(86, 23);
		this.buttonRandomizeAppearance.TabIndex = 27;
		this.buttonRandomizeAppearance.Text = "Randomize";
		this.buttonRandomizeAppearance.UseVisualStyleBackColor = true;
		this.buttonRandomizeAppearance.Click += new System.EventHandler(buttonRandomizeAppearance_Click);
		this.labelHeadType.BackColor = System.Drawing.SystemColors.Control;
		this.labelHeadType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHeadType.Location = new System.Drawing.Point(185, 112);
		this.labelHeadType.Name = "labelHeadType";
		this.labelHeadType.Size = new System.Drawing.Size(127, 20);
		this.labelHeadType.TabIndex = 11;
		this.labelHeadType.Text = "Head Model";
		this.labelHeadType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelHairType.BackColor = System.Drawing.SystemColors.Control;
		this.labelHairType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHairType.Location = new System.Drawing.Point(16, 184);
		this.labelHairType.Name = "labelHairType";
		this.labelHairType.Size = new System.Drawing.Size(119, 20);
		this.labelHairType.TabIndex = 9;
		this.labelHairType.Text = "Hair Model";
		this.labelHairType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = true;
		this.pickUpControl.CreateButtonEnabled = true;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = new string[3] { "All", "by Country", "by League" };
		this.pickUpControl.FilterEnabled = true;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = true;
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = true;
		this.pickUpControl.SearchEnabled = true;
		this.pickUpControl.Size = new System.Drawing.Size(1357, 25);
		this.pickUpControl.TabIndex = 1;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		this.buttonRandomizeAllReferees.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomizeAllReferees.Location = new System.Drawing.Point(9, 255);
		this.buttonRandomizeAllReferees.Name = "buttonRandomizeAllReferees";
		this.buttonRandomizeAllReferees.Size = new System.Drawing.Size(136, 23);
		this.buttonRandomizeAllReferees.TabIndex = 88;
		this.buttonRandomizeAllReferees.Text = "Randomize All";
		this.buttonRandomizeAllReferees.UseVisualStyleBackColor = true;
		this.buttonRandomizeAllReferees.Click += new System.EventHandler(buttonRandomizeAllReferees_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1357, 832);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "RefereeForm";
		this.Text = "RefereeForm";
		base.Load += new System.EventHandler(RefereeForm_Load);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.flowLayoutPanel1.ResumeLayout(false);
		this.groupIdentity.ResumeLayout(false);
		this.groupIdentity.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.refereeBindingSource).EndInit();
		this.groupShoes.ResumeLayout(false);
		this.groupShoes.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesBrand).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesDesign).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericHeight).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericWeight).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericRefereeId).EndInit();
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).EndInit();
		this.groupLeagues.ResumeLayout(false);
		this.splitContainer2.Panel1.ResumeLayout(false);
		this.splitContainer2.Panel1.PerformLayout();
		this.splitContainer2.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).EndInit();
		this.splitContainer2.ResumeLayout(false);
		this.tool3D.ResumeLayout(false);
		this.tool3D.PerformLayout();
		this.groupGenericFace.ResumeLayout(false);
		this.groupTextureInfo.ResumeLayout(false);
		this.groupTextureInfo.PerformLayout();
		this.groupHairModel.ResumeLayout(false);
		this.groupHeadModel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
