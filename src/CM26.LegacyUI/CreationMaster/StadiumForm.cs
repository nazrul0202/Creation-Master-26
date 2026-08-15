using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class StadiumForm : Form
{
	private Stadium m_CurrentStadium;

	private Stadium m_CopyStadium;

	private TabPage m_CurrentPage;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private string m_Undefined = "< Undefined >";

	private bool m_Locked;

	private IContainer components;

	public PickUpControl pickUpControl;

	private TabControl tabEsitStadium;

	private TabPage pageStadiumGeneral;

	private TabPage pageStadiumModel;

	private FlowLayoutPanel flowLayoutPanel1;

	private GroupBox groupBox1;

	private Button buttonGetId;

	private NumericUpDown numericStadiumId;

	private ComboBox comboHomeTeam;

	private TextBox textDatabaseStadiumName;

	private PictureBox pictureHomeTeam;

	private Label labelDatabaseStadiumName;

	private TextBox textLocalStadiumName;

	private Label labelLocalStadiumName;

	private Label labelStadiumId;

	private DomainUpDown domainStadiumType;

	private NumericUpDown numericYearBuilt;

	private NumericUpDown numericCapacity;

	private Label labelCapacity;

	private Label labelYearBuilt;

	private Label labelStadiumType;

	private NumericUpDown numericCrowdColor;

	private Label labelCrowdColor;

	private ComboBox comboCountry;

	private Label labelCountry;

	private CheckBox checkOrientation;

	private GroupBox groupMowingPattern;

	public NumericUpDown numericMowing;

	private Viewer2D viewer2DMowing;

	private GroupBox groupBox3;

	private Viewer2D viewer2DNet;

	public NumericUpDown numericNet;

	private CheckBox checkDeepNet;

	private GroupBox groupAdboards;

	private Label label1;

	private Label labelAdboardEndLine;

	private NumericUpDown numericSideLineDistance;

	private NumericUpDown numericEndLineDistance;

	private GroupBox groupTimeAndWeather;

	private CheckBox checkSunnyDay;

	private ComboBox comboDayWeather;

	private CheckBox checkNight;

	private Label label2;

	private FlowLayoutPanel flowLayoutPanel2;

	private MultiViewer2D multiViewer2DTextures;

	private MultiViewer2D multiViewer2DCoverMap;

	private GroupBox groupBox5;

	private GroupBox groupBox6;

	private GroupBox groupPolice;

	private Viewer2D viewer2DPolice;

	private ComboBox comboPolice;

	private Button buttonCopyCrowd;

	private BindingSource stadiumListBindingSource;

	private GroupBox groupLights;

	private Label label3;

	private ComboBox comboStadiumLights;

	private TabPage pageStadiumPreview;

	private GroupBox groupEnvironment;

	private GroupBox groupBox4;

	private Viewer2D viewer2DPreviewLarge;

	private Viewer2D viewer2DPreview;

	private GroupBox groupBox2;

	private RadioButton radioPreviewClearDay;

	private GroupBox groupBox7;

	private RadioButton radioModelClearDay;

	private GroupBox groupCamera;

	public NumericUpDown numericCameraZoom;

	public NumericUpDown numericCameraHeight;

	private Label label4;

	private Label label5;

	public NumericUpDown numericAdboardType;

	private Label label6;

	private RadioButton radioPreviewNight;

	private RadioButton radioModelNight;

	private GroupBox groupBox8;

	private GroupBox groupBox10;

	private NumericUpDown numericTechZoneAwayMaxZ;

	private NumericUpDown numericTechZoneAwayMinZ;

	private NumericUpDown numericTechZoneAwayMaxX;

	private NumericUpDown numericTechZoneAwayMinX;

	private Label label11;

	private Label label12;

	private Label label13;

	private Label label14;

	private GroupBox groupBox9;

	private NumericUpDown numericTechZoneHomeMaxZ;

	private NumericUpDown numericTechZoneHomeMinZ;

	private NumericUpDown numericTechZoneHomeMaxX;

	private NumericUpDown numericTechZoneHomeMinX;

	private Label label10;

	private Label label9;

	private Label label8;

	private Label label7;

	private CheckBox checkIsLicensed;

	private RadioButton radioPreviewSunset;

	private RadioButton radioPreviewOvercast;

	public StadiumForm()
	{
		base.Visible = false;
		InitializeComponent();
		pickUpControl.SelectObject = SelectStadium;
		pickUpControl.DeleteObject = DeleteStadium;
		pickUpControl.CloneObject = CloneStadium;
		pickUpControl.RefreshObject = RefreshStadium;
		viewer2DPreview.ImageImport = ImportImagePreview;
		viewer2DPreview.ImageDelete = DeletePreview;
		viewer2DPreview.ButtonStripVisible = true;
		viewer2DPreview.RemoveButton = true;
		viewer2DPreviewLarge.ImageImport = ImportImagePreviewLarge;
		viewer2DPreviewLarge.ImageDelete = DeletePreviewLarge;
		viewer2DPreviewLarge.ButtonStripVisible = true;
		viewer2DPreviewLarge.RemoveButton = true;
		viewer2DMowing.ImageImport = ImportImageMowing;
		viewer2DMowing.ImageDelete = DeleteMowing;
		viewer2DMowing.ButtonStripVisible = true;
		viewer2DMowing.RemoveButton = true;
		viewer2DNet.ImageImport = ImportImageNet;
		viewer2DNet.ImageDelete = DeleteNet;
		viewer2DNet.ButtonStripVisible = true;
		viewer2DNet.RemoveButton = true;
		viewer2DPolice.ButtonStripVisible = false;
		multiViewer2DTextures.Rx3ExportDelegate = ExportRx3StadiumTextures;
		multiViewer2DTextures.Rx3ImportDelegate = ImportRx3StadiumTextures;
		multiViewer2DTextures.Rx3SaveDelegate = SaveRx3StadiumTextures;
		multiViewer2DTextures.Rx3DeleteDelegate = DeleteRx3StadiumTextures;
		multiViewer2DTextures.ShowDeleteButton = true;
		if (FifaEnvironment.Year == 14)
		{
			viewer2DMowing.ImageSize = new Size(1024, 1024);
			viewer2DPolice.ImageSize = new Size(256, 256);
		}
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.Stadiums;
		numericStadiumId.Maximum = FifaEnvironment.Stadiums.MaxId;
		numericMowing.Maximum = FifaEnvironment.Year == 26 ? 9999 : FifaEnvironment.FifaDb.Table[TI.stadiums].TableDescriptor.MaxValues[FI.stadiums_stadiummowpattern_code];
		numericNet.Maximum = FifaEnvironment.Year == 26 ? 9999 : FifaEnvironment.FifaDb.Table[TI.stadiums].TableDescriptor.MaxValues[FI.stadiums_stadiumgoalnetstyle];
		IdArrayList[] filterValues = new IdArrayList[2]
		{
			null,
			FifaEnvironment.Countries
		};
		pickUpControl.FilterValues = filterValues;
		RefreshComboBoxes();
		stadiumListBindingSource.DataSource = FifaEnvironment.Stadiums;
		pickUpControl.ObjectList = FifaEnvironment.Stadiums;
		if (FifaEnvironment.Year == 2014)
		{
			viewer2DMowing.ImageSize = new Size(1024, 1024);
		}
	}

	public void RefreshComboBoxes()
	{
		if (comboCountry.Items.Count != FifaEnvironment.Countries.Count + 1)
		{
			comboCountry.Items.Clear();
			comboCountry.Items.Add("None");
			comboCountry.Items.AddRange(FifaEnvironment.Countries.ToArray());
		}
		if (comboHomeTeam.Items.Count != FifaEnvironment.Teams.Count + 1)
		{
			comboHomeTeam.Items.Clear();
			comboHomeTeam.Items.Add(m_Undefined);
			comboHomeTeam.Items.AddRange(FifaEnvironment.Teams.ToArray());
		}
	}

	private void StadiumForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private Stadium SelectStadium(object sender, object obj)
	{
		Stadium stadium = (Stadium)obj;
		Refresh();
		LoadStadium(stadium);
		return stadium;
	}

	private Stadium DeleteStadium(object sender, object obj)
	{
		Stadium stadium = (Stadium)obj;
		FifaEnvironment.Stadiums.DeleteStadium(stadium);
		m_CurrentStadium = null;
		return null;
	}

	private Stadium CloneStadium(object sender, object obj)
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
		Stadium srcIdObject = (Stadium)obj;
		return (Stadium)FifaEnvironment.Stadiums.CloneId(srcIdObject, m_NewIdCreator.NewObject);
	}

	public Stadium RefreshStadium(object sender, object obj)
	{
		Preset();
		ReloadStadium(m_CurrentStadium);
		return m_CurrentStadium;
	}

	public void ReloadStadium(Stadium stadium)
	{
		m_CurrentStadium = null;
		LoadStadium(stadium);
	}

	private bool ImportImagePreview(object sender, Bitmap bitmap)
	{
		int num = CurrentPreviewTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		return m_CurrentStadium.SetPreview(num, bitmap);
	}

	private bool DeletePreview(object sender)
	{
		int num = CurrentPreviewTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		return m_CurrentStadium.DeletePreview(num);
	}

	private bool ImportImagePreviewLarge(object sender, Bitmap bitmap)
	{
		int num = CurrentPreviewTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		return m_CurrentStadium.SetPreviewLarge(num, bitmap);
	}

	private bool DeletePreviewLarge(object sender)
	{
		int num = CurrentPreviewTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		return m_CurrentStadium.DeletePreviewLarge(num);
	}

	private bool ImportImageNet(object sender, Bitmap bitmap)
	{
		return m_CurrentStadium.SetNet(bitmap);
	}

	private bool DeleteNet(object sender)
	{
		return m_CurrentStadium.DeleteNet();
	}

	private bool ImportImageMowing(object sender, Bitmap bitmap)
	{
		return m_CurrentStadium.SetMowingPattern(bitmap);
	}

	private bool DeleteMowing(object sender)
	{
		return m_CurrentStadium.DeleteMowingPattern();
	}

	private bool ImportImagePolice(object sender, Bitmap bitmap)
	{
		return m_CurrentStadium.SetPolice(bitmap);
	}

	private bool DeletePolice(object sender)
	{
		return m_CurrentStadium.DeletePolice();
	}

	private bool ExportRx3StadiumTextures(object sender, string exportDir)
	{
		int num = CurrentModelTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		return FifaEnvironment.ExportFileFromZdata(m_CurrentStadium.TexturesFileName(num), exportDir);
	}

	private bool SaveRx3StadiumTextures(object sender, Bitmap[] bitmaps)
	{
		int num = CurrentModelTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		bool num2 = m_CurrentStadium.SetTextures(num, bitmaps);
		if (num2)
		{
			ReloadStadium(m_CurrentStadium);
		}
		return num2;
	}

	private bool ImportRx3StadiumTextures(object sender, string rx3FileName)
	{
		int num = CurrentModelTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		bool num2 = m_CurrentStadium.SetTextures(num, rx3FileName);
		if (num2)
		{
			ReloadStadium(m_CurrentStadium);
		}
		return num2;
	}

	private bool DeleteRx3StadiumTextures(object sender)
	{
		int num = CurrentModelTimeOfDay();
		if (num < 0)
		{
			return false;
		}
		bool num2 = m_CurrentStadium.DeleteContainer(num);
		if (num2)
		{
			ReloadStadium(m_CurrentStadium);
		}
		return num2;
	}

	public void LoadStadium(Stadium stadium)
	{
		if (m_IsLoaded && (m_CurrentStadium != stadium || m_CurrentPage != tabEsitStadium.SelectedTab))
		{
			m_CurrentStadium = stadium;
			m_CurrentPage = tabEsitStadium.SelectedTab;
			if (m_CurrentPage == pageStadiumGeneral)
			{
				LoadStadiumGeneral();
			}
			else if (m_CurrentPage == pageStadiumModel)
			{
				LoadStadiumModel();
			}
			else if (m_CurrentPage == pageStadiumPreview)
			{
				LoadStadiumPreview();
			}
		}
	}

	public void LoadStadiumModel()
	{
		m_Locked = true;
		AdjustPreviewModelRadio();
		int num = CurrentModelTimeOfDay();
		if (num < 0)
		{
			multiViewer2DTextures.Bitmaps = null;
			multiViewer2DCoverMap.Bitmaps = null;
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		multiViewer2DTextures.Bitmaps = m_CurrentStadium.GetTextures(num);
		EnableCopyButtons();
		Cursor.Current = Cursors.Default;
		m_Locked = false;
	}

	public void LoadStadiumPreview()
	{
		m_Locked = true;
		AdjustPreviewWeatherRadio();
		int num = CurrentPreviewTimeOfDay();
		if (num < 0)
		{
			viewer2DPreview.CurrentBitmap = null;
			viewer2DPreviewLarge.CurrentBitmap = null;
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		viewer2DPreview.CurrentBitmap = m_CurrentStadium.GetPreview(num);
		viewer2DPreviewLarge.CurrentBitmap = m_CurrentStadium.GetPreviewLarge(num);
		Cursor.Current = Cursors.Default;
		m_Locked = false;
	}

	private int CurrentModelTimeOfDay()
	{
		int result = -1;
		if (radioModelClearDay.Checked)
		{
			result = 1;
		}
		if (radioModelNight.Checked)
		{
			result = 3;
		}
		return result;
	}

	private int CurrentPreviewTimeOfDay()
	{
		int result = -1;
		if (radioPreviewClearDay.Checked)
		{
			result = 1;
		}
		if (radioPreviewNight.Checked)
		{
			result = 3;
		}
		if (radioPreviewOvercast.Checked)
		{
			result = 0;
		}
		if (radioPreviewSunset.Checked)
		{
			result = 4;
		}
		return result;
	}

	private void AdjustPreviewWeatherRadio()
	{
		radioPreviewClearDay.Enabled = true;
		radioPreviewNight.Enabled = true;
		CurrentPreviewTimeOfDay();
	}

	private void AdjustPreviewModelRadio()
	{
		radioPreviewClearDay.Enabled = true;
		radioPreviewNight.Enabled = true;
		CurrentModelTimeOfDay();
	}

	public void LoadStadiumGeneral()
	{
		m_Locked = true;
		textDatabaseStadiumName.Text = m_CurrentStadium.name;
		textLocalStadiumName.Text = m_CurrentStadium.LocalName;
		numericStadiumId.Value = m_CurrentStadium.Id;
		numericCapacity.Value = m_CurrentStadium.capacity;
		numericYearBuilt.Value = m_CurrentStadium.yearbuilt;
		numericCrowdColor.Value = m_CurrentStadium.seatcolor;
		checkOrientation.Checked = m_CurrentStadium.sectionfacedbydefault == 1;
		domainStadiumType.SelectedIndex = m_CurrentStadium.stadiumtype;
		numericEndLineDistance.Value = m_CurrentStadium.adboardendlinedistance;
		numericSideLineDistance.Value = m_CurrentStadium.adboardsidelinedistance;
		numericMowing.Value = m_CurrentStadium.MowingPatternId;
		numericNet.Value = m_CurrentStadium.NetColor;
		comboPolice.SelectedIndex = m_CurrentStadium.policetypecode;
		checkSunnyDay.Checked = m_CurrentStadium.HasSunnyDay();
		checkNight.Checked = m_CurrentStadium.HasNight();
		checkDeepNet.Checked = m_CurrentStadium.IsDeepNet;
		if (m_CurrentStadium.Country == null)
		{
			comboCountry.SelectedIndex = 0;
		}
		else
		{
			comboCountry.SelectedItem = m_CurrentStadium.Country;
		}
		comboDayWeather.SelectedIndex = m_CurrentStadium.GetWeather();
		if (m_CurrentStadium.hometeamid == 0 || m_CurrentStadium.HomeTeam == null)
		{
			comboHomeTeam.SelectedItem = m_Undefined;
			pictureHomeTeam.BackgroundImage = null;
		}
		else
		{
			comboHomeTeam.SelectedItem = m_CurrentStadium.HomeTeam;
			pictureHomeTeam.BackgroundImage = m_CurrentStadium.HomeTeam.GetCrest();
		}
		viewer2DPolice.CurrentBitmap = m_CurrentStadium.GetPolice();
		numericCameraHeight.Value = m_CurrentStadium.cameraheight;
		numericCameraZoom.Value = m_CurrentStadium.camerazoom;
		numericAdboardType.Value = m_CurrentStadium.adboardtype;
		numericTechZoneAwayMinX.Value = m_CurrentStadium.stadawaytechzoneminx;
		numericTechZoneAwayMaxX.Value = m_CurrentStadium.stadawaytechzonemaxx;
		numericTechZoneAwayMinZ.Value = m_CurrentStadium.stadawaytechzoneminz;
		numericTechZoneAwayMaxZ.Value = m_CurrentStadium.stadawaytechzonemaxz;
		numericTechZoneHomeMinX.Value = m_CurrentStadium.stadhometechzoneminx;
		numericTechZoneHomeMaxX.Value = m_CurrentStadium.stadhometechzonemaxx;
		numericTechZoneHomeMinZ.Value = m_CurrentStadium.stadhometechzoneminz;
		numericTechZoneHomeMaxZ.Value = m_CurrentStadium.stadhometechzonemaxz;
		checkIsLicensed.Checked = m_CurrentStadium.islicensed;
		m_Locked = false;
	}

	private void labelCountry_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentStadium.Country != null)
		{
			MainForm.CM.JumpTo(m_CurrentStadium.Country);
		}
	}

	private void textDatabaseStadiumName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.name = textDatabaseStadiumName.Text;
			pickUpControl.SwitchObject(m_CurrentStadium);
		}
	}

	private void textLocalStadiumName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.LocalName = textLocalStadiumName.Text;
		}
	}

	private void numericCapacity_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.capacity = (int)numericCapacity.Value;
		}
	}

	private void numericYearBuilt_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.yearbuilt = (int)numericYearBuilt.Value;
		}
	}

	private void domainStadiumType_SelectedItemChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadiumtype = domainStadiumType.SelectedIndex;
		}
	}

	private void numericStadiumId_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		int num = (int)numericStadiumId.Value;
		if (num != m_CurrentStadium.Id)
		{
			if (FifaEnvironment.Stadiums.SearchId(num) == null)
			{
				FifaEnvironment.Stadiums.ChangeId(m_CurrentStadium, num);
				return;
			}
			FifaEnvironment.UserMessages.ShowMessage(1015);
			numericStadiumId.Value = m_CurrentStadium.Id;
		}
	}

	private void buttonGetId_Click(object sender, EventArgs e)
	{
		int newId = FifaEnvironment.Stadiums.GetNewId();
		if (newId == -1)
		{
			FifaEnvironment.UserMessages.ShowMessage(5050);
		}
		else
		{
			numericStadiumId.Value = newId;
		}
	}

	private void comboHomeTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			if (comboHomeTeam.SelectedIndex == 0)
			{
				m_CurrentStadium.hometeamid = 0;
				m_CurrentStadium.HomeTeam = null;
				pictureHomeTeam.BackgroundImage = null;
			}
			else
			{
				Team team = (Team)comboHomeTeam.SelectedItem;
				m_CurrentStadium.hometeamid = team.Id;
				m_CurrentStadium.HomeTeam = team;
				pictureHomeTeam.BackgroundImage = team.GetCrest();
			}
		}
	}

	private void comboCountry_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboCountry.SelectedIndex >= 0)
		{
			if (comboCountry.SelectedIndex == 0)
			{
				m_CurrentStadium.Country = null;
			}
			else
			{
				m_CurrentStadium.Country = (Country)comboCountry.SelectedItem;
			}
		}
	}

	private void numericCrowdColor_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.seatcolor = (int)numericCrowdColor.Value;
		}
	}

	private void numericEndLineDistance_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.adboardendlinedistance = (int)numericEndLineDistance.Value;
		}
	}

	private void numericSideLineDistance_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.adboardsidelinedistance = (int)numericSideLineDistance.Value;
		}
	}

	private void numericMowing_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentStadium.MowingPatternId = (int)numericMowing.Value;
		viewer2DMowing.DisposeBitmap();
		viewer2DMowing.CurrentBitmap = m_CurrentStadium.GetMowingPattern();
	}

	private void numericNet_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentStadium.NetColor = (int)numericNet.Value;
		viewer2DNet.CurrentBitmap = m_CurrentStadium.GetNet();
	}

	private void radioModelOvercast_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumModel();
	}

	private void radioModelClearDay_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumModel();
	}

	private void radioModelNight_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumModel();
	}

	private void radioModelSunset_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumModel();
	}

	private void tabEsitStadium_SelectedIndexChanged(object sender, EventArgs e)
	{
		LoadStadium(m_CurrentStadium);
	}

	private void checkDeepNet_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.IsDeepNet = checkDeepNet.Checked;
		}
	}

	private void checkOrientation_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.sectionfacedbydefault = (checkOrientation.Checked ? 1 : 0);
		}
	}

	private void comboDayWeather_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboDayWeather.SelectedIndex >= 0)
		{
			m_CurrentStadium.SetWeather(comboDayWeather.SelectedIndex);
		}
	}

	private void checkSunnyDay_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.SetSunnyDay(checkSunnyDay.Checked);
		}
	}

	private void checkNight_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.SetNight(checkNight.Checked);
		}
	}

	private void comboPolice_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentStadium.policetypecode != comboPolice.SelectedIndex)
		{
			m_CurrentStadium.policetypecode = comboPolice.SelectedIndex;
			viewer2DPolice.CurrentBitmap = m_CurrentStadium.GetPolice();
		}
	}

	private void EnableCopyButtons()
	{
		m_CopyStadium = (Stadium)comboStadiumLights.SelectedItem;
		int timeofday = CurrentModelTimeOfDay();
		bool flag = m_CopyStadium != null;
		if (flag)
		{
			flag = FifaEnvironment.IsFilePresent(Stadium.GlaresLightFileNames(m_CopyStadium.Id, timeofday)[0]);
		}
		if (flag)
		{
			flag = flag && FifaEnvironment.IsFilePresent(Stadium.CrowdFileName(m_CopyStadium.Id, timeofday));
		}
		if (flag)
		{
			flag = flag && FifaEnvironment.IsFilePresent(Stadium.RadiosityFileName(m_CopyStadium.Id));
		}
		buttonCopyCrowd.Enabled = flag;
	}

	private void comboStadiumLights_SelectedIndexChanged(object sender, EventArgs e)
	{
		EnableCopyButtons();
	}

	private void buttonCopyCrowd_Click(object sender, EventArgs e)
	{
		int num = CurrentModelTimeOfDay();
		if (num >= 0 && m_CopyStadium != null)
		{
			m_CopyStadium.CloneCrowd(m_CurrentStadium.Id, num);
			m_CopyStadium.CloneGlares(m_CurrentStadium.Id, num);
			m_CopyStadium.CloneRadiosity(m_CurrentStadium.Id);
		}
	}

	private void buttonCopyLights_Click(object sender, EventArgs e)
	{
		int num = CurrentModelTimeOfDay();
		if (num >= 0 && m_CopyStadium != null)
		{
			m_CopyStadium.CloneGlares(m_CurrentStadium.Id, num);
		}
	}

	private void pictureHomeTeam_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentStadium.HomeTeam != null)
		{
			MainForm.CM.JumpTo(m_CurrentStadium.HomeTeam);
		}
	}

	private void radioPreviewOvercast_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumPreview();
	}

	private void radioPreviewClearDay_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumPreview();
	}

	private void radioPreviewlNight_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumPreview();
	}

	private void radioPreviewSunset_CheckedChanged(object sender, EventArgs e)
	{
		LoadStadiumPreview();
	}

	private void numericCameraHeight_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.cameraheight = (int)numericCameraHeight.Value;
		}
	}

	private void numericCameraZoom_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.camerazoom = (int)numericCameraZoom.Value;
		}
	}

	private void numericAdboardType_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.adboardtype = (int)numericAdboardType.Value;
		}
	}

	private void checkIsLicensed_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.islicensed = checkIsLicensed.Checked;
		}
	}

	private void numericTechZoneHomeMinX_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadhometechzoneminx = (int)numericTechZoneHomeMinX.Value;
		}
	}

	private void numericTechZoneHomeMaxX_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadhometechzonemaxx = (int)numericTechZoneHomeMaxX.Value;
		}
	}

	private void numericTechZoneHomeMinZ_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadhometechzoneminz = (int)numericTechZoneHomeMinZ.Value;
		}
	}

	private void numericTechZoneHomeMaxZ_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadhometechzonemaxz = (int)numericTechZoneHomeMaxZ.Value;
		}
	}

	private void numericTechZoneAwayMinX_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadawaytechzoneminx = (int)numericTechZoneAwayMinX.Value;
		}
	}

	private void numericTechZoneAwayMaxX_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadawaytechzonemaxx = (int)numericTechZoneAwayMaxX.Value;
		}
	}

	private void numericTechZoneAwayMinZ_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadawaytechzoneminz = (int)numericTechZoneAwayMinZ.Value;
		}
	}

	private void numericTechZoneAwayMaxZ_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStadium.stadawaytechzonemaxz = (int)numericTechZoneAwayMaxZ.Value;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.StadiumForm));
		this.tabEsitStadium = new System.Windows.Forms.TabControl();
		this.pageStadiumGeneral = new System.Windows.Forms.TabPage();
		this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.checkIsLicensed = new System.Windows.Forms.CheckBox();
		this.checkOrientation = new System.Windows.Forms.CheckBox();
		this.comboCountry = new System.Windows.Forms.ComboBox();
		this.labelCountry = new System.Windows.Forms.Label();
		this.numericCrowdColor = new System.Windows.Forms.NumericUpDown();
		this.labelCrowdColor = new System.Windows.Forms.Label();
		this.buttonGetId = new System.Windows.Forms.Button();
		this.numericStadiumId = new System.Windows.Forms.NumericUpDown();
		this.comboHomeTeam = new System.Windows.Forms.ComboBox();
		this.textDatabaseStadiumName = new System.Windows.Forms.TextBox();
		this.pictureHomeTeam = new System.Windows.Forms.PictureBox();
		this.labelDatabaseStadiumName = new System.Windows.Forms.Label();
		this.textLocalStadiumName = new System.Windows.Forms.TextBox();
		this.labelLocalStadiumName = new System.Windows.Forms.Label();
		this.labelStadiumId = new System.Windows.Forms.Label();
		this.domainStadiumType = new System.Windows.Forms.DomainUpDown();
		this.numericYearBuilt = new System.Windows.Forms.NumericUpDown();
		this.numericCapacity = new System.Windows.Forms.NumericUpDown();
		this.labelCapacity = new System.Windows.Forms.Label();
		this.labelYearBuilt = new System.Windows.Forms.Label();
		this.labelStadiumType = new System.Windows.Forms.Label();
		this.groupMowingPattern = new System.Windows.Forms.GroupBox();
		this.numericMowing = new System.Windows.Forms.NumericUpDown();
		this.viewer2DMowing = new FifaControls.Viewer2D();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.checkDeepNet = new System.Windows.Forms.CheckBox();
		this.viewer2DNet = new FifaControls.Viewer2D();
		this.numericNet = new System.Windows.Forms.NumericUpDown();
		this.groupBox6 = new System.Windows.Forms.GroupBox();
		this.groupCamera = new System.Windows.Forms.GroupBox();
		this.numericCameraZoom = new System.Windows.Forms.NumericUpDown();
		this.numericCameraHeight = new System.Windows.Forms.NumericUpDown();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.groupAdboards = new System.Windows.Forms.GroupBox();
		this.numericAdboardType = new System.Windows.Forms.NumericUpDown();
		this.label6 = new System.Windows.Forms.Label();
		this.numericSideLineDistance = new System.Windows.Forms.NumericUpDown();
		this.numericEndLineDistance = new System.Windows.Forms.NumericUpDown();
		this.label1 = new System.Windows.Forms.Label();
		this.labelAdboardEndLine = new System.Windows.Forms.Label();
		this.groupTimeAndWeather = new System.Windows.Forms.GroupBox();
		this.label2 = new System.Windows.Forms.Label();
		this.comboDayWeather = new System.Windows.Forms.ComboBox();
		this.checkNight = new System.Windows.Forms.CheckBox();
		this.checkSunnyDay = new System.Windows.Forms.CheckBox();
		this.groupPolice = new System.Windows.Forms.GroupBox();
		this.comboPolice = new System.Windows.Forms.ComboBox();
		this.viewer2DPolice = new FifaControls.Viewer2D();
		this.groupBox8 = new System.Windows.Forms.GroupBox();
		this.groupBox10 = new System.Windows.Forms.GroupBox();
		this.numericTechZoneAwayMaxZ = new System.Windows.Forms.NumericUpDown();
		this.numericTechZoneAwayMinZ = new System.Windows.Forms.NumericUpDown();
		this.numericTechZoneAwayMaxX = new System.Windows.Forms.NumericUpDown();
		this.numericTechZoneAwayMinX = new System.Windows.Forms.NumericUpDown();
		this.label11 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.groupBox9 = new System.Windows.Forms.GroupBox();
		this.numericTechZoneHomeMaxZ = new System.Windows.Forms.NumericUpDown();
		this.numericTechZoneHomeMinZ = new System.Windows.Forms.NumericUpDown();
		this.numericTechZoneHomeMaxX = new System.Windows.Forms.NumericUpDown();
		this.numericTechZoneHomeMinX = new System.Windows.Forms.NumericUpDown();
		this.label10 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.pageStadiumPreview = new System.Windows.Forms.TabPage();
		this.groupEnvironment = new System.Windows.Forms.GroupBox();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.viewer2DPreviewLarge = new FifaControls.Viewer2D();
		this.viewer2DPreview = new FifaControls.Viewer2D();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.radioPreviewClearDay = new System.Windows.Forms.RadioButton();
		this.radioPreviewNight = new System.Windows.Forms.RadioButton();
		this.pageStadiumModel = new System.Windows.Forms.TabPage();
		this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
		this.groupBox7 = new System.Windows.Forms.GroupBox();
		this.radioModelClearDay = new System.Windows.Forms.RadioButton();
		this.radioModelNight = new System.Windows.Forms.RadioButton();
		this.multiViewer2DTextures = new FifaControls.MultiViewer2D();
		this.groupLights = new System.Windows.Forms.GroupBox();
		this.comboStadiumLights = new System.Windows.Forms.ComboBox();
		this.stadiumListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.buttonCopyCrowd = new System.Windows.Forms.Button();
		this.label3 = new System.Windows.Forms.Label();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.multiViewer2DCoverMap = new FifaControls.MultiViewer2D();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.radioPreviewOvercast = new System.Windows.Forms.RadioButton();
		this.radioPreviewSunset = new System.Windows.Forms.RadioButton();
		this.tabEsitStadium.SuspendLayout();
		this.pageStadiumGeneral.SuspendLayout();
		this.flowLayoutPanel1.SuspendLayout();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCrowdColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericStadiumId).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHomeTeam).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericYearBuilt).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericCapacity).BeginInit();
		this.groupMowingPattern.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericMowing).BeginInit();
		this.groupBox3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNet).BeginInit();
		this.groupBox6.SuspendLayout();
		this.groupCamera.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCameraZoom).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericCameraHeight).BeginInit();
		this.groupAdboards.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericAdboardType).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericSideLineDistance).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericEndLineDistance).BeginInit();
		this.groupTimeAndWeather.SuspendLayout();
		this.groupPolice.SuspendLayout();
		this.groupBox8.SuspendLayout();
		this.groupBox10.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMaxZ).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMinZ).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMaxX).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMinX).BeginInit();
		this.groupBox9.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMaxZ).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMinZ).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMaxX).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMinX).BeginInit();
		this.pageStadiumPreview.SuspendLayout();
		this.groupEnvironment.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.pageStadiumModel.SuspendLayout();
		this.flowLayoutPanel2.SuspendLayout();
		this.groupBox7.SuspendLayout();
		this.groupLights.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.stadiumListBindingSource).BeginInit();
		this.groupBox5.SuspendLayout();
		base.SuspendLayout();
		this.tabEsitStadium.Controls.Add(this.pageStadiumGeneral);
		this.tabEsitStadium.Controls.Add(this.pageStadiumPreview);
		this.tabEsitStadium.Controls.Add(this.pageStadiumModel);
		this.tabEsitStadium.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabEsitStadium.Location = new System.Drawing.Point(0, 25);
		this.tabEsitStadium.Name = "tabEsitStadium";
		this.tabEsitStadium.SelectedIndex = 0;
		this.tabEsitStadium.Size = new System.Drawing.Size(1357, 807);
		this.tabEsitStadium.TabIndex = 2;
		this.tabEsitStadium.SelectedIndexChanged += new System.EventHandler(tabEsitStadium_SelectedIndexChanged);
		this.pageStadiumGeneral.Controls.Add(this.flowLayoutPanel1);
		this.pageStadiumGeneral.Location = new System.Drawing.Point(4, 22);
		this.pageStadiumGeneral.Name = "pageStadiumGeneral";
		this.pageStadiumGeneral.Padding = new System.Windows.Forms.Padding(3);
		this.pageStadiumGeneral.Size = new System.Drawing.Size(1349, 781);
		this.pageStadiumGeneral.TabIndex = 0;
		this.pageStadiumGeneral.Text = "General";
		this.pageStadiumGeneral.UseVisualStyleBackColor = true;
		this.flowLayoutPanel1.AutoScroll = true;
		this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
		this.flowLayoutPanel1.Controls.Add(this.groupBox1);
		this.flowLayoutPanel1.Controls.Add(this.groupMowingPattern);
		this.flowLayoutPanel1.Controls.Add(this.groupBox3);
		this.flowLayoutPanel1.Controls.Add(this.groupBox6);
		this.flowLayoutPanel1.Controls.Add(this.groupPolice);
		this.flowLayoutPanel1.Controls.Add(this.groupBox8);
		this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
		this.flowLayoutPanel1.Name = "flowLayoutPanel1";
		this.flowLayoutPanel1.Size = new System.Drawing.Size(1343, 775);
		this.flowLayoutPanel1.TabIndex = 0;
		this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
		this.groupBox1.Controls.Add(this.checkIsLicensed);
		this.groupBox1.Controls.Add(this.checkOrientation);
		this.groupBox1.Controls.Add(this.comboCountry);
		this.groupBox1.Controls.Add(this.labelCountry);
		this.groupBox1.Controls.Add(this.numericCrowdColor);
		this.groupBox1.Controls.Add(this.labelCrowdColor);
		this.groupBox1.Controls.Add(this.buttonGetId);
		this.groupBox1.Controls.Add(this.numericStadiumId);
		this.groupBox1.Controls.Add(this.comboHomeTeam);
		this.groupBox1.Controls.Add(this.textDatabaseStadiumName);
		this.groupBox1.Controls.Add(this.pictureHomeTeam);
		this.groupBox1.Controls.Add(this.labelDatabaseStadiumName);
		this.groupBox1.Controls.Add(this.textLocalStadiumName);
		this.groupBox1.Controls.Add(this.labelLocalStadiumName);
		this.groupBox1.Controls.Add(this.labelStadiumId);
		this.groupBox1.Controls.Add(this.domainStadiumType);
		this.groupBox1.Controls.Add(this.numericYearBuilt);
		this.groupBox1.Controls.Add(this.numericCapacity);
		this.groupBox1.Controls.Add(this.labelCapacity);
		this.groupBox1.Controls.Add(this.labelYearBuilt);
		this.groupBox1.Controls.Add(this.labelStadiumType);
		this.groupBox1.Location = new System.Drawing.Point(3, 3);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(265, 339);
		this.groupBox1.TabIndex = 0;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Info";
		this.checkIsLicensed.AutoSize = true;
		this.checkIsLicensed.Location = new System.Drawing.Point(6, 238);
		this.checkIsLicensed.Name = "checkIsLicensed";
		this.checkIsLicensed.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkIsLicensed.Size = new System.Drawing.Size(122, 17);
		this.checkIsLicensed.TabIndex = 126;
		this.checkIsLicensed.Text = "              Is Licensed";
		this.checkIsLicensed.UseVisualStyleBackColor = true;
		this.checkIsLicensed.CheckedChanged += new System.EventHandler(checkIsLicensed_CheckedChanged);
		this.checkOrientation.AutoSize = true;
		this.checkOrientation.Location = new System.Drawing.Point(6, 215);
		this.checkOrientation.Name = "checkOrientation";
		this.checkOrientation.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkOrientation.Size = new System.Drawing.Size(122, 17);
		this.checkOrientation.TabIndex = 8;
		this.checkOrientation.Text = "Opposite Orientation";
		this.checkOrientation.UseVisualStyleBackColor = true;
		this.checkOrientation.CheckedChanged += new System.EventHandler(checkOrientation_CheckedChanged);
		this.comboCountry.Location = new System.Drawing.Point(118, 177);
		this.comboCountry.Name = "comboCountry";
		this.comboCountry.Size = new System.Drawing.Size(137, 21);
		this.comboCountry.TabIndex = 7;
		this.comboCountry.SelectedIndexChanged += new System.EventHandler(comboCountry_SelectedIndexChanged);
		this.labelCountry.AutoSize = true;
		this.labelCountry.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelCountry.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelCountry.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCountry.Location = new System.Drawing.Point(6, 180);
		this.labelCountry.Name = "labelCountry";
		this.labelCountry.Size = new System.Drawing.Size(43, 13);
		this.labelCountry.TabIndex = 125;
		this.labelCountry.Text = "Country";
		this.labelCountry.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCountry.Click += new System.EventHandler(labelCountry_DoubleClick);
		this.numericCrowdColor.Location = new System.Drawing.Point(118, 152);
		this.numericCrowdColor.Maximum = new decimal(new int[4] { 8, 0, 0, 0 });
		this.numericCrowdColor.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericCrowdColor.Name = "numericCrowdColor";
		this.numericCrowdColor.Size = new System.Drawing.Size(136, 20);
		this.numericCrowdColor.TabIndex = 6;
		this.numericCrowdColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCrowdColor.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericCrowdColor.ValueChanged += new System.EventHandler(numericCrowdColor_ValueChanged);
		this.labelCrowdColor.AutoSize = true;
		this.labelCrowdColor.BackColor = System.Drawing.SystemColors.Control;
		this.labelCrowdColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCrowdColor.Location = new System.Drawing.Point(6, 154);
		this.labelCrowdColor.Name = "labelCrowdColor";
		this.labelCrowdColor.Size = new System.Drawing.Size(59, 13);
		this.labelCrowdColor.TabIndex = 122;
		this.labelCrowdColor.Text = "Seat Color ";
		this.labelCrowdColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonGetId.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonGetId.BackgroundImage");
		this.buttonGetId.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonGetId.Location = new System.Drawing.Point(225, 59);
		this.buttonGetId.Name = "buttonGetId";
		this.buttonGetId.Size = new System.Drawing.Size(24, 24);
		this.buttonGetId.TabIndex = 1;
		this.buttonGetId.TabStop = false;
		this.buttonGetId.UseVisualStyleBackColor = true;
		this.buttonGetId.Click += new System.EventHandler(buttonGetId_Click);
		this.numericStadiumId.Location = new System.Drawing.Point(118, 61);
		this.numericStadiumId.Maximum = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericStadiumId.Name = "numericStadiumId";
		this.numericStadiumId.Size = new System.Drawing.Size(100, 20);
		this.numericStadiumId.TabIndex = 2;
		this.numericStadiumId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericStadiumId.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericStadiumId.ValueChanged += new System.EventHandler(numericStadiumId_ValueChanged);
		this.comboHomeTeam.ItemHeight = 13;
		this.comboHomeTeam.Location = new System.Drawing.Point(155, 304);
		this.comboHomeTeam.MaxLength = 32767;
		this.comboHomeTeam.Name = "comboHomeTeam";
		this.comboHomeTeam.Size = new System.Drawing.Size(100, 21);
		this.comboHomeTeam.Sorted = true;
		this.comboHomeTeam.TabIndex = 9;
		this.comboHomeTeam.SelectedIndexChanged += new System.EventHandler(comboHomeTeam_SelectedIndexChanged);
		this.textDatabaseStadiumName.Location = new System.Drawing.Point(118, 16);
		this.textDatabaseStadiumName.Name = "textDatabaseStadiumName";
		this.textDatabaseStadiumName.Size = new System.Drawing.Size(136, 20);
		this.textDatabaseStadiumName.TabIndex = 0;
		this.textDatabaseStadiumName.TextChanged += new System.EventHandler(textDatabaseStadiumName_TextChanged);
		this.pictureHomeTeam.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureHomeTeam.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureHomeTeam.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureHomeTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureHomeTeam.Location = new System.Drawing.Point(155, 204);
		this.pictureHomeTeam.Name = "pictureHomeTeam";
		this.pictureHomeTeam.Size = new System.Drawing.Size(100, 100);
		this.pictureHomeTeam.TabIndex = 68;
		this.pictureHomeTeam.TabStop = false;
		this.pictureHomeTeam.DoubleClick += new System.EventHandler(pictureHomeTeam_DoubleClick);
		this.labelDatabaseStadiumName.AutoSize = true;
		this.labelDatabaseStadiumName.BackColor = System.Drawing.SystemColors.Control;
		this.labelDatabaseStadiumName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDatabaseStadiumName.Location = new System.Drawing.Point(6, 16);
		this.labelDatabaseStadiumName.Name = "labelDatabaseStadiumName";
		this.labelDatabaseStadiumName.Size = new System.Drawing.Size(84, 13);
		this.labelDatabaseStadiumName.TabIndex = 1;
		this.labelDatabaseStadiumName.Text = "Database Name";
		this.labelDatabaseStadiumName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textLocalStadiumName.Location = new System.Drawing.Point(118, 38);
		this.textLocalStadiumName.Name = "textLocalStadiumName";
		this.textLocalStadiumName.Size = new System.Drawing.Size(136, 20);
		this.textLocalStadiumName.TabIndex = 1;
		this.textLocalStadiumName.TextChanged += new System.EventHandler(textLocalStadiumName_TextChanged);
		this.labelLocalStadiumName.AutoSize = true;
		this.labelLocalStadiumName.BackColor = System.Drawing.SystemColors.Control;
		this.labelLocalStadiumName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLocalStadiumName.Location = new System.Drawing.Point(6, 37);
		this.labelLocalStadiumName.Name = "labelLocalStadiumName";
		this.labelLocalStadiumName.Size = new System.Drawing.Size(35, 13);
		this.labelLocalStadiumName.TabIndex = 2;
		this.labelLocalStadiumName.Text = "Name";
		this.labelLocalStadiumName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelStadiumId.AutoSize = true;
		this.labelStadiumId.BackColor = System.Drawing.SystemColors.Control;
		this.labelStadiumId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStadiumId.Location = new System.Drawing.Point(6, 63);
		this.labelStadiumId.Name = "labelStadiumId";
		this.labelStadiumId.Size = new System.Drawing.Size(57, 13);
		this.labelStadiumId.TabIndex = 121;
		this.labelStadiumId.Text = "Stadium Id";
		this.labelStadiumId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.domainStadiumType.Items.Add("Official");
		this.domainStadiumType.Items.Add("Training");
		this.domainStadiumType.Location = new System.Drawing.Point(118, 129);
		this.domainStadiumType.Name = "domainStadiumType";
		this.domainStadiumType.Size = new System.Drawing.Size(136, 20);
		this.domainStadiumType.TabIndex = 5;
		this.domainStadiumType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.domainStadiumType.Wrap = true;
		this.domainStadiumType.SelectedItemChanged += new System.EventHandler(domainStadiumType_SelectedItemChanged);
		this.numericYearBuilt.Location = new System.Drawing.Point(118, 106);
		this.numericYearBuilt.Maximum = new decimal(new int[4] { 2050, 0, 0, 0 });
		this.numericYearBuilt.Minimum = new decimal(new int[4] { 1800, 0, 0, 0 });
		this.numericYearBuilt.Name = "numericYearBuilt";
		this.numericYearBuilt.Size = new System.Drawing.Size(136, 20);
		this.numericYearBuilt.TabIndex = 4;
		this.numericYearBuilt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericYearBuilt.Value = new decimal(new int[4] { 1800, 0, 0, 0 });
		this.numericYearBuilt.ValueChanged += new System.EventHandler(numericYearBuilt_ValueChanged);
		this.numericCapacity.Increment = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.numericCapacity.Location = new System.Drawing.Point(118, 84);
		this.numericCapacity.Maximum = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericCapacity.Name = "numericCapacity";
		this.numericCapacity.Size = new System.Drawing.Size(136, 20);
		this.numericCapacity.TabIndex = 3;
		this.numericCapacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCapacity.ThousandsSeparator = true;
		this.numericCapacity.ValueChanged += new System.EventHandler(numericCapacity_ValueChanged);
		this.labelCapacity.AutoSize = true;
		this.labelCapacity.BackColor = System.Drawing.SystemColors.Control;
		this.labelCapacity.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCapacity.Location = new System.Drawing.Point(6, 84);
		this.labelCapacity.Name = "labelCapacity";
		this.labelCapacity.Size = new System.Drawing.Size(48, 13);
		this.labelCapacity.TabIndex = 70;
		this.labelCapacity.Text = "Capacity";
		this.labelCapacity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelYearBuilt.AutoSize = true;
		this.labelYearBuilt.BackColor = System.Drawing.SystemColors.Control;
		this.labelYearBuilt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelYearBuilt.Location = new System.Drawing.Point(6, 106);
		this.labelYearBuilt.Name = "labelYearBuilt";
		this.labelYearBuilt.Size = new System.Drawing.Size(52, 13);
		this.labelYearBuilt.TabIndex = 72;
		this.labelYearBuilt.Text = "Year Built";
		this.labelYearBuilt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelStadiumType.AutoSize = true;
		this.labelStadiumType.BackColor = System.Drawing.SystemColors.Control;
		this.labelStadiumType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStadiumType.Location = new System.Drawing.Point(6, 129);
		this.labelStadiumType.Name = "labelStadiumType";
		this.labelStadiumType.Size = new System.Drawing.Size(31, 13);
		this.labelStadiumType.TabIndex = 74;
		this.labelStadiumType.Text = "Type";
		this.labelStadiumType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupMowingPattern.BackColor = System.Drawing.SystemColors.Control;
		this.groupMowingPattern.Controls.Add(this.numericMowing);
		this.groupMowingPattern.Controls.Add(this.viewer2DMowing);
		this.groupMowingPattern.Location = new System.Drawing.Point(274, 3);
		this.groupMowingPattern.Name = "groupMowingPattern";
		this.groupMowingPattern.Size = new System.Drawing.Size(266, 339);
		this.groupMowingPattern.TabIndex = 1;
		this.groupMowingPattern.TabStop = false;
		this.groupMowingPattern.Text = "Mowing Pattern";
		this.numericMowing.Location = new System.Drawing.Point(6, 19);
		this.numericMowing.Maximum = new decimal(new int[4] { 13, 0, 0, 0 });
		this.numericMowing.Name = "numericMowing";
		this.numericMowing.Size = new System.Drawing.Size(64, 20);
		this.numericMowing.TabIndex = 0;
		this.numericMowing.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericMowing.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericMowing.ValueChanged += new System.EventHandler(numericMowing_ValueChanged);
		this.viewer2DMowing.AutoTransparency = false;
		this.viewer2DMowing.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DMowing.ButtonStripVisible = false;
		this.viewer2DMowing.CurrentBitmap = null;
		this.viewer2DMowing.ExtendedFormat = false;
		this.viewer2DMowing.FullSizeButton = false;
		this.viewer2DMowing.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DMowing.ImageSize = new System.Drawing.Size(1024, 2048);
		this.viewer2DMowing.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DMowing.Location = new System.Drawing.Point(6, 45);
		this.viewer2DMowing.Name = "viewer2DMowing";
		this.viewer2DMowing.RemoveButton = false;
		this.viewer2DMowing.ShowButton = false;
		this.viewer2DMowing.ShowButtonChecked = true;
		this.viewer2DMowing.Size = new System.Drawing.Size(256, 281);
		this.viewer2DMowing.TabIndex = 1;
		this.viewer2DMowing.TabStop = false;
		this.groupBox3.BackColor = System.Drawing.SystemColors.Control;
		this.groupBox3.Controls.Add(this.checkDeepNet);
		this.groupBox3.Controls.Add(this.viewer2DNet);
		this.groupBox3.Controls.Add(this.numericNet);
		this.groupBox3.Location = new System.Drawing.Point(546, 3);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(152, 339);
		this.groupBox3.TabIndex = 2;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Net";
		this.checkDeepNet.AutoSize = true;
		this.checkDeepNet.Location = new System.Drawing.Point(14, 214);
		this.checkDeepNet.Name = "checkDeepNet";
		this.checkDeepNet.Size = new System.Drawing.Size(72, 17);
		this.checkDeepNet.TabIndex = 1;
		this.checkDeepNet.Text = "Deep Net";
		this.checkDeepNet.UseVisualStyleBackColor = true;
		this.checkDeepNet.CheckedChanged += new System.EventHandler(checkDeepNet_CheckedChanged);
		this.viewer2DNet.AutoTransparency = true;
		this.viewer2DNet.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DNet.ButtonStripVisible = false;
		this.viewer2DNet.CurrentBitmap = null;
		this.viewer2DNet.ExtendedFormat = false;
		this.viewer2DNet.FullSizeButton = false;
		this.viewer2DNet.ImageLayout = System.Windows.Forms.ImageLayout.Tile;
		this.viewer2DNet.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DNet.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DNet.Location = new System.Drawing.Point(14, 44);
		this.viewer2DNet.Name = "viewer2DNet";
		this.viewer2DNet.RemoveButton = false;
		this.viewer2DNet.ShowButton = false;
		this.viewer2DNet.ShowButtonChecked = true;
		this.viewer2DNet.Size = new System.Drawing.Size(128, 153);
		this.viewer2DNet.TabIndex = 1;
		this.viewer2DNet.TabStop = false;
		this.numericNet.Location = new System.Drawing.Point(14, 19);
		this.numericNet.Maximum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericNet.Name = "numericNet";
		this.numericNet.Size = new System.Drawing.Size(64, 20);
		this.numericNet.TabIndex = 0;
		this.numericNet.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNet.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericNet.ValueChanged += new System.EventHandler(numericNet_ValueChanged);
		this.groupBox6.Controls.Add(this.groupCamera);
		this.groupBox6.Controls.Add(this.groupAdboards);
		this.groupBox6.Controls.Add(this.groupTimeAndWeather);
		this.groupBox6.Location = new System.Drawing.Point(704, 3);
		this.groupBox6.Name = "groupBox6";
		this.groupBox6.Size = new System.Drawing.Size(202, 339);
		this.groupBox6.TabIndex = 3;
		this.groupBox6.TabStop = false;
		this.groupCamera.Controls.Add(this.numericCameraZoom);
		this.groupCamera.Controls.Add(this.numericCameraHeight);
		this.groupCamera.Controls.Add(this.label4);
		this.groupCamera.Controls.Add(this.label5);
		this.groupCamera.Location = new System.Drawing.Point(6, 215);
		this.groupCamera.Name = "groupCamera";
		this.groupCamera.Size = new System.Drawing.Size(192, 73);
		this.groupCamera.TabIndex = 2;
		this.groupCamera.TabStop = false;
		this.groupCamera.Text = "Camera";
		this.numericCameraZoom.Location = new System.Drawing.Point(106, 45);
		this.numericCameraZoom.Maximum = new decimal(new int[4] { 15, 0, 0, 0 });
		this.numericCameraZoom.Name = "numericCameraZoom";
		this.numericCameraZoom.Size = new System.Drawing.Size(80, 20);
		this.numericCameraZoom.TabIndex = 119;
		this.numericCameraZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCameraZoom.Value = new decimal(new int[4] { 7, 0, 0, 0 });
		this.numericCameraZoom.ValueChanged += new System.EventHandler(numericCameraZoom_ValueChanged);
		this.numericCameraHeight.Location = new System.Drawing.Point(106, 19);
		this.numericCameraHeight.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.numericCameraHeight.Name = "numericCameraHeight";
		this.numericCameraHeight.Size = new System.Drawing.Size(80, 20);
		this.numericCameraHeight.TabIndex = 118;
		this.numericCameraHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCameraHeight.Value = new decimal(new int[4] { 15, 0, 0, 0 });
		this.numericCameraHeight.ValueChanged += new System.EventHandler(numericCameraHeight_ValueChanged);
		this.label4.AutoSize = true;
		this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label4.Location = new System.Drawing.Point(3, 47);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(34, 13);
		this.label4.TabIndex = 117;
		this.label4.Text = "Zoom";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label5.AutoSize = true;
		this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label5.Location = new System.Drawing.Point(3, 21);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(38, 13);
		this.label5.TabIndex = 116;
		this.label5.Text = "Height";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupAdboards.BackColor = System.Drawing.SystemColors.Control;
		this.groupAdboards.Controls.Add(this.numericAdboardType);
		this.groupAdboards.Controls.Add(this.label6);
		this.groupAdboards.Controls.Add(this.numericSideLineDistance);
		this.groupAdboards.Controls.Add(this.numericEndLineDistance);
		this.groupAdboards.Controls.Add(this.label1);
		this.groupAdboards.Controls.Add(this.labelAdboardEndLine);
		this.groupAdboards.Location = new System.Drawing.Point(6, 13);
		this.groupAdboards.Name = "groupAdboards";
		this.groupAdboards.Size = new System.Drawing.Size(192, 106);
		this.groupAdboards.TabIndex = 0;
		this.groupAdboards.TabStop = false;
		this.groupAdboards.Text = "Adboards";
		this.numericAdboardType.Location = new System.Drawing.Point(106, 26);
		this.numericAdboardType.Maximum = new decimal(new int[4] { 3, 0, 0, 0 });
		this.numericAdboardType.Name = "numericAdboardType";
		this.numericAdboardType.Size = new System.Drawing.Size(80, 20);
		this.numericAdboardType.TabIndex = 119;
		this.numericAdboardType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericAdboardType.ValueChanged += new System.EventHandler(numericAdboardType_ValueChanged);
		this.label6.AutoSize = true;
		this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label6.Location = new System.Drawing.Point(7, 28);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(31, 13);
		this.label6.TabIndex = 116;
		this.label6.Text = "Type";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericSideLineDistance.Location = new System.Drawing.Point(106, 79);
		this.numericSideLineDistance.Maximum = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.numericSideLineDistance.Name = "numericSideLineDistance";
		this.numericSideLineDistance.Size = new System.Drawing.Size(82, 20);
		this.numericSideLineDistance.TabIndex = 1;
		this.numericSideLineDistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericSideLineDistance.Value = new decimal(new int[4] { 500, 0, 0, 0 });
		this.numericSideLineDistance.ValueChanged += new System.EventHandler(numericSideLineDistance_ValueChanged);
		this.numericEndLineDistance.Location = new System.Drawing.Point(106, 53);
		this.numericEndLineDistance.Maximum = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.numericEndLineDistance.Name = "numericEndLineDistance";
		this.numericEndLineDistance.Size = new System.Drawing.Size(82, 20);
		this.numericEndLineDistance.TabIndex = 0;
		this.numericEndLineDistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericEndLineDistance.Value = new decimal(new int[4] { 500, 0, 0, 0 });
		this.numericEndLineDistance.ValueChanged += new System.EventHandler(numericEndLineDistance_ValueChanged);
		this.label1.AutoSize = true;
		this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1.Location = new System.Drawing.Point(7, 81);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(96, 13);
		this.label1.TabIndex = 115;
		this.label1.Text = "Side Line Distance";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelAdboardEndLine.AutoSize = true;
		this.labelAdboardEndLine.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelAdboardEndLine.Location = new System.Drawing.Point(7, 55);
		this.labelAdboardEndLine.Name = "labelAdboardEndLine";
		this.labelAdboardEndLine.Size = new System.Drawing.Size(94, 13);
		this.labelAdboardEndLine.TabIndex = 114;
		this.labelAdboardEndLine.Text = "End Line Distance";
		this.labelAdboardEndLine.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupTimeAndWeather.BackColor = System.Drawing.SystemColors.Control;
		this.groupTimeAndWeather.Controls.Add(this.label2);
		this.groupTimeAndWeather.Controls.Add(this.comboDayWeather);
		this.groupTimeAndWeather.Controls.Add(this.checkNight);
		this.groupTimeAndWeather.Controls.Add(this.checkSunnyDay);
		this.groupTimeAndWeather.Location = new System.Drawing.Point(6, 125);
		this.groupTimeAndWeather.Name = "groupTimeAndWeather";
		this.groupTimeAndWeather.Size = new System.Drawing.Size(192, 84);
		this.groupTimeAndWeather.TabIndex = 1;
		this.groupTimeAndWeather.TabStop = false;
		this.groupTimeAndWeather.Text = "Time and Weather";
		this.label2.AutoSize = true;
		this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label2.Location = new System.Drawing.Point(6, 55);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(48, 13);
		this.label2.TabIndex = 116;
		this.label2.Text = "Weather";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboDayWeather.FormattingEnabled = true;
		this.comboDayWeather.Items.AddRange(new object[3] { "Dry", "Can Rain", "Can Snow" });
		this.comboDayWeather.Location = new System.Drawing.Point(69, 52);
		this.comboDayWeather.Name = "comboDayWeather";
		this.comboDayWeather.Size = new System.Drawing.Size(117, 21);
		this.comboDayWeather.TabIndex = 4;
		this.comboDayWeather.SelectedIndexChanged += new System.EventHandler(comboDayWeather_SelectedIndexChanged);
		this.checkNight.AutoSize = true;
		this.checkNight.Location = new System.Drawing.Point(106, 25);
		this.checkNight.Name = "checkNight";
		this.checkNight.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.checkNight.Size = new System.Drawing.Size(51, 17);
		this.checkNight.TabIndex = 3;
		this.checkNight.Text = "Night";
		this.checkNight.UseVisualStyleBackColor = true;
		this.checkNight.CheckedChanged += new System.EventHandler(checkNight_CheckedChanged);
		this.checkSunnyDay.AutoSize = true;
		this.checkSunnyDay.Location = new System.Drawing.Point(6, 25);
		this.checkSunnyDay.Name = "checkSunnyDay";
		this.checkSunnyDay.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.checkSunnyDay.Size = new System.Drawing.Size(45, 17);
		this.checkSunnyDay.TabIndex = 0;
		this.checkSunnyDay.Text = "Day";
		this.checkSunnyDay.UseVisualStyleBackColor = true;
		this.checkSunnyDay.CheckedChanged += new System.EventHandler(checkSunnyDay_CheckedChanged);
		this.groupPolice.Controls.Add(this.comboPolice);
		this.groupPolice.Controls.Add(this.viewer2DPolice);
		this.groupPolice.Location = new System.Drawing.Point(912, 3);
		this.groupPolice.Name = "groupPolice";
		this.groupPolice.Size = new System.Drawing.Size(270, 339);
		this.groupPolice.TabIndex = 4;
		this.groupPolice.TabStop = false;
		this.groupPolice.Text = "Police";
		this.comboPolice.FormattingEnabled = true;
		this.comboPolice.Items.AddRange(new object[11]
		{
			"0 = None", "1 = English Police", "2 = French Police", "3 = Italian Police", "4 = German Police", "5 = Spanish Police", "6 = Mexican Police", "7 = Asiatic Traits Police", "8 = African Traits Police", "9 = CaucasicTraits Police",
			"10 = ArabicTraits Police"
		});
		this.comboPolice.Location = new System.Drawing.Point(32, 17);
		this.comboPolice.Name = "comboPolice";
		this.comboPolice.Size = new System.Drawing.Size(207, 21);
		this.comboPolice.TabIndex = 126;
		this.comboPolice.SelectedIndexChanged += new System.EventHandler(comboPolice_SelectedIndexChanged);
		this.viewer2DPolice.AutoTransparency = false;
		this.viewer2DPolice.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPolice.ButtonStripVisible = false;
		this.viewer2DPolice.CurrentBitmap = null;
		this.viewer2DPolice.ExtendedFormat = false;
		this.viewer2DPolice.FullSizeButton = false;
		this.viewer2DPolice.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DPolice.ImageSize = new System.Drawing.Size(1024, 1024);
		this.viewer2DPolice.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DPolice.Location = new System.Drawing.Point(7, 44);
		this.viewer2DPolice.Name = "viewer2DPolice";
		this.viewer2DPolice.RemoveButton = false;
		this.viewer2DPolice.ShowButton = false;
		this.viewer2DPolice.ShowButtonChecked = true;
		this.viewer2DPolice.Size = new System.Drawing.Size(256, 256);
		this.viewer2DPolice.TabIndex = 2;
		this.viewer2DPolice.TabStop = false;
		this.groupBox8.Controls.Add(this.groupBox10);
		this.groupBox8.Controls.Add(this.groupBox9);
		this.groupBox8.Location = new System.Drawing.Point(3, 348);
		this.groupBox8.Name = "groupBox8";
		this.groupBox8.Size = new System.Drawing.Size(475, 133);
		this.groupBox8.TabIndex = 5;
		this.groupBox8.TabStop = false;
		this.groupBox8.Text = "Technical Zone Coordinates";
		this.groupBox10.Controls.Add(this.numericTechZoneAwayMaxZ);
		this.groupBox10.Controls.Add(this.numericTechZoneAwayMinZ);
		this.groupBox10.Controls.Add(this.numericTechZoneAwayMaxX);
		this.groupBox10.Controls.Add(this.numericTechZoneAwayMinX);
		this.groupBox10.Controls.Add(this.label11);
		this.groupBox10.Controls.Add(this.label12);
		this.groupBox10.Controls.Add(this.label13);
		this.groupBox10.Controls.Add(this.label14);
		this.groupBox10.Location = new System.Drawing.Point(237, 19);
		this.groupBox10.Name = "groupBox10";
		this.groupBox10.Size = new System.Drawing.Size(228, 108);
		this.groupBox10.TabIndex = 1;
		this.groupBox10.TabStop = false;
		this.groupBox10.Text = "Away";
		this.numericTechZoneAwayMaxZ.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneAwayMaxZ.Location = new System.Drawing.Point(135, 72);
		this.numericTechZoneAwayMaxZ.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneAwayMaxZ.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneAwayMaxZ.Name = "numericTechZoneAwayMaxZ";
		this.numericTechZoneAwayMaxZ.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneAwayMaxZ.TabIndex = 15;
		this.numericTechZoneAwayMaxZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneAwayMaxZ.ValueChanged += new System.EventHandler(numericTechZoneAwayMaxZ_ValueChanged);
		this.numericTechZoneAwayMinZ.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneAwayMinZ.Location = new System.Drawing.Point(38, 72);
		this.numericTechZoneAwayMinZ.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneAwayMinZ.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneAwayMinZ.Name = "numericTechZoneAwayMinZ";
		this.numericTechZoneAwayMinZ.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneAwayMinZ.TabIndex = 14;
		this.numericTechZoneAwayMinZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneAwayMinZ.ValueChanged += new System.EventHandler(numericTechZoneAwayMinZ_ValueChanged);
		this.numericTechZoneAwayMaxX.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneAwayMaxX.Location = new System.Drawing.Point(135, 41);
		this.numericTechZoneAwayMaxX.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneAwayMaxX.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneAwayMaxX.Name = "numericTechZoneAwayMaxX";
		this.numericTechZoneAwayMaxX.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneAwayMaxX.TabIndex = 13;
		this.numericTechZoneAwayMaxX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneAwayMaxX.ValueChanged += new System.EventHandler(numericTechZoneAwayMaxX_ValueChanged);
		this.numericTechZoneAwayMinX.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneAwayMinX.Location = new System.Drawing.Point(38, 41);
		this.numericTechZoneAwayMinX.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneAwayMinX.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneAwayMinX.Name = "numericTechZoneAwayMinX";
		this.numericTechZoneAwayMinX.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneAwayMinX.TabIndex = 12;
		this.numericTechZoneAwayMinX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneAwayMinX.ValueChanged += new System.EventHandler(numericTechZoneAwayMinX_ValueChanged);
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(12, 74);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(14, 13);
		this.label11.TabIndex = 11;
		this.label11.Text = "Z";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(13, 45);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(14, 13);
		this.label12.TabIndex = 10;
		this.label12.Text = "X";
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(163, 17);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(26, 13);
		this.label13.TabIndex = 9;
		this.label13.Text = "max";
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(68, 17);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(23, 13);
		this.label14.TabIndex = 8;
		this.label14.Text = "min";
		this.groupBox9.Controls.Add(this.numericTechZoneHomeMaxZ);
		this.groupBox9.Controls.Add(this.numericTechZoneHomeMinZ);
		this.groupBox9.Controls.Add(this.numericTechZoneHomeMaxX);
		this.groupBox9.Controls.Add(this.numericTechZoneHomeMinX);
		this.groupBox9.Controls.Add(this.label10);
		this.groupBox9.Controls.Add(this.label9);
		this.groupBox9.Controls.Add(this.label8);
		this.groupBox9.Controls.Add(this.label7);
		this.groupBox9.Location = new System.Drawing.Point(9, 19);
		this.groupBox9.Name = "groupBox9";
		this.groupBox9.Size = new System.Drawing.Size(222, 108);
		this.groupBox9.TabIndex = 0;
		this.groupBox9.TabStop = false;
		this.groupBox9.Text = "Home";
		this.numericTechZoneHomeMaxZ.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneHomeMaxZ.Location = new System.Drawing.Point(128, 71);
		this.numericTechZoneHomeMaxZ.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneHomeMaxZ.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneHomeMaxZ.Name = "numericTechZoneHomeMaxZ";
		this.numericTechZoneHomeMaxZ.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneHomeMaxZ.TabIndex = 7;
		this.numericTechZoneHomeMaxZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneHomeMaxZ.ValueChanged += new System.EventHandler(numericTechZoneHomeMaxZ_ValueChanged);
		this.numericTechZoneHomeMinZ.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneHomeMinZ.Location = new System.Drawing.Point(31, 71);
		this.numericTechZoneHomeMinZ.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneHomeMinZ.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneHomeMinZ.Name = "numericTechZoneHomeMinZ";
		this.numericTechZoneHomeMinZ.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneHomeMinZ.TabIndex = 6;
		this.numericTechZoneHomeMinZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneHomeMinZ.ValueChanged += new System.EventHandler(numericTechZoneHomeMinZ_ValueChanged);
		this.numericTechZoneHomeMaxX.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneHomeMaxX.Location = new System.Drawing.Point(128, 40);
		this.numericTechZoneHomeMaxX.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneHomeMaxX.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneHomeMaxX.Name = "numericTechZoneHomeMaxX";
		this.numericTechZoneHomeMaxX.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneHomeMaxX.TabIndex = 5;
		this.numericTechZoneHomeMaxX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneHomeMaxX.ValueChanged += new System.EventHandler(numericTechZoneHomeMaxX_ValueChanged);
		this.numericTechZoneHomeMinX.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericTechZoneHomeMinX.Location = new System.Drawing.Point(31, 40);
		this.numericTechZoneHomeMinX.Maximum = new decimal(new int[4] { 4500, 0, 0, 0 });
		this.numericTechZoneHomeMinX.Minimum = new decimal(new int[4] { 4500, 0, 0, -2147483648 });
		this.numericTechZoneHomeMinX.Name = "numericTechZoneHomeMinX";
		this.numericTechZoneHomeMinX.Size = new System.Drawing.Size(82, 20);
		this.numericTechZoneHomeMinX.TabIndex = 4;
		this.numericTechZoneHomeMinX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTechZoneHomeMinX.ValueChanged += new System.EventHandler(numericTechZoneHomeMinX_ValueChanged);
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(5, 73);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(14, 13);
		this.label10.TabIndex = 3;
		this.label10.Text = "Z";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(6, 44);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(14, 13);
		this.label9.TabIndex = 2;
		this.label9.Text = "X";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(156, 16);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(26, 13);
		this.label8.TabIndex = 1;
		this.label8.Text = "max";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(61, 16);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(23, 13);
		this.label7.TabIndex = 0;
		this.label7.Text = "min";
		this.pageStadiumPreview.Controls.Add(this.groupEnvironment);
		this.pageStadiumPreview.Location = new System.Drawing.Point(4, 22);
		this.pageStadiumPreview.Name = "pageStadiumPreview";
		this.pageStadiumPreview.Size = new System.Drawing.Size(1349, 781);
		this.pageStadiumPreview.TabIndex = 2;
		this.pageStadiumPreview.Text = "Preview";
		this.pageStadiumPreview.UseVisualStyleBackColor = true;
		this.groupEnvironment.BackColor = System.Drawing.SystemColors.Control;
		this.groupEnvironment.Controls.Add(this.groupBox4);
		this.groupEnvironment.Controls.Add(this.groupBox2);
		this.groupEnvironment.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupEnvironment.Location = new System.Drawing.Point(0, 0);
		this.groupEnvironment.Name = "groupEnvironment";
		this.groupEnvironment.Size = new System.Drawing.Size(1349, 781);
		this.groupEnvironment.TabIndex = 104;
		this.groupEnvironment.TabStop = false;
		this.groupBox4.Controls.Add(this.viewer2DPreviewLarge);
		this.groupBox4.Controls.Add(this.viewer2DPreview);
		this.groupBox4.Location = new System.Drawing.Point(6, 69);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(1039, 595);
		this.groupBox4.TabIndex = 107;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Preview";
		this.viewer2DPreviewLarge.AutoTransparency = false;
		this.viewer2DPreviewLarge.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPreviewLarge.ButtonStripVisible = false;
		this.viewer2DPreviewLarge.CurrentBitmap = null;
		this.viewer2DPreviewLarge.ExtendedFormat = false;
		this.viewer2DPreviewLarge.FullSizeButton = false;
		this.viewer2DPreviewLarge.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DPreviewLarge.ImageSize = new System.Drawing.Size(1024, 512);
		this.viewer2DPreviewLarge.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DPreviewLarge.Location = new System.Drawing.Point(2, 282);
		this.viewer2DPreviewLarge.Name = "viewer2DPreviewLarge";
		this.viewer2DPreviewLarge.RemoveButton = false;
		this.viewer2DPreviewLarge.ShowButton = false;
		this.viewer2DPreviewLarge.ShowButtonChecked = true;
		this.viewer2DPreviewLarge.Size = new System.Drawing.Size(1024, 300);
		this.viewer2DPreviewLarge.TabIndex = 106;
		this.viewer2DPreview.AutoTransparency = false;
		this.viewer2DPreview.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPreview.ButtonStripVisible = false;
		this.viewer2DPreview.CurrentBitmap = null;
		this.viewer2DPreview.ExtendedFormat = false;
		this.viewer2DPreview.FullSizeButton = false;
		this.viewer2DPreview.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DPreview.ImageSize = new System.Drawing.Size(1024, 256);
		this.viewer2DPreview.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DPreview.Location = new System.Drawing.Point(2, 16);
		this.viewer2DPreview.Name = "viewer2DPreview";
		this.viewer2DPreview.RemoveButton = false;
		this.viewer2DPreview.ShowButton = false;
		this.viewer2DPreview.ShowButtonChecked = true;
		this.viewer2DPreview.Size = new System.Drawing.Size(605, 260);
		this.viewer2DPreview.TabIndex = 105;
		this.groupBox2.Controls.Add(this.radioPreviewSunset);
		this.groupBox2.Controls.Add(this.radioPreviewOvercast);
		this.groupBox2.Controls.Add(this.radioPreviewClearDay);
		this.groupBox2.Controls.Add(this.radioPreviewNight);
		this.groupBox2.Location = new System.Drawing.Point(8, 13);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(607, 50);
		this.groupBox2.TabIndex = 106;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Time";
		this.radioPreviewClearDay.AutoSize = true;
		this.radioPreviewClearDay.Checked = true;
		this.radioPreviewClearDay.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioPreviewClearDay.Location = new System.Drawing.Point(16, 19);
		this.radioPreviewClearDay.Name = "radioPreviewClearDay";
		this.radioPreviewClearDay.Size = new System.Drawing.Size(44, 17);
		this.radioPreviewClearDay.TabIndex = 100;
		this.radioPreviewClearDay.TabStop = true;
		this.radioPreviewClearDay.Text = "Day";
		this.radioPreviewClearDay.UseVisualStyleBackColor = true;
		this.radioPreviewClearDay.CheckedChanged += new System.EventHandler(radioPreviewClearDay_CheckedChanged);
		this.radioPreviewNight.AutoSize = true;
		this.radioPreviewNight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioPreviewNight.Location = new System.Drawing.Point(92, 19);
		this.radioPreviewNight.Name = "radioPreviewNight";
		this.radioPreviewNight.Size = new System.Drawing.Size(50, 17);
		this.radioPreviewNight.TabIndex = 101;
		this.radioPreviewNight.Text = "Night";
		this.radioPreviewNight.UseVisualStyleBackColor = true;
		this.radioPreviewNight.CheckedChanged += new System.EventHandler(radioPreviewlNight_CheckedChanged);
		this.pageStadiumModel.Controls.Add(this.flowLayoutPanel2);
		this.pageStadiumModel.Location = new System.Drawing.Point(4, 22);
		this.pageStadiumModel.Name = "pageStadiumModel";
		this.pageStadiumModel.Padding = new System.Windows.Forms.Padding(3);
		this.pageStadiumModel.Size = new System.Drawing.Size(1349, 781);
		this.pageStadiumModel.TabIndex = 1;
		this.pageStadiumModel.Text = "Model";
		this.pageStadiumModel.UseVisualStyleBackColor = true;
		this.flowLayoutPanel2.AutoScroll = true;
		this.flowLayoutPanel2.BackColor = System.Drawing.SystemColors.Control;
		this.flowLayoutPanel2.Controls.Add(this.groupBox7);
		this.flowLayoutPanel2.Controls.Add(this.multiViewer2DTextures);
		this.flowLayoutPanel2.Controls.Add(this.groupLights);
		this.flowLayoutPanel2.Controls.Add(this.groupBox5);
		this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
		this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
		this.flowLayoutPanel2.Name = "flowLayoutPanel2";
		this.flowLayoutPanel2.Size = new System.Drawing.Size(1343, 775);
		this.flowLayoutPanel2.TabIndex = 104;
		this.groupBox7.Controls.Add(this.radioModelClearDay);
		this.groupBox7.Controls.Add(this.radioModelNight);
		this.groupBox7.Location = new System.Drawing.Point(3, 3);
		this.groupBox7.Name = "groupBox7";
		this.groupBox7.Size = new System.Drawing.Size(512, 50);
		this.groupBox7.TabIndex = 110;
		this.groupBox7.TabStop = false;
		this.groupBox7.Text = "Time";
		this.radioModelClearDay.AutoSize = true;
		this.radioModelClearDay.Checked = true;
		this.radioModelClearDay.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioModelClearDay.Location = new System.Drawing.Point(16, 19);
		this.radioModelClearDay.Name = "radioModelClearDay";
		this.radioModelClearDay.Size = new System.Drawing.Size(44, 17);
		this.radioModelClearDay.TabIndex = 100;
		this.radioModelClearDay.TabStop = true;
		this.radioModelClearDay.Text = "Day";
		this.radioModelClearDay.UseVisualStyleBackColor = true;
		this.radioModelClearDay.CheckedChanged += new System.EventHandler(radioModelClearDay_CheckedChanged);
		this.radioModelNight.AutoSize = true;
		this.radioModelNight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioModelNight.Location = new System.Drawing.Point(92, 19);
		this.radioModelNight.Name = "radioModelNight";
		this.radioModelNight.Size = new System.Drawing.Size(50, 17);
		this.radioModelNight.TabIndex = 101;
		this.radioModelNight.Text = "Night";
		this.radioModelNight.UseVisualStyleBackColor = true;
		this.radioModelNight.CheckedChanged += new System.EventHandler(radioModelNight_CheckedChanged);
		this.multiViewer2DTextures.AutoTransparency = false;
		this.multiViewer2DTextures.BackColor = System.Drawing.SystemColors.Control;
		this.multiViewer2DTextures.Bitmaps = null;
		this.multiViewer2DTextures.CheckBitmapSize = false;
		this.multiViewer2DTextures.FixedSize = false;
		this.multiViewer2DTextures.FullSizeButton = false;
		this.multiViewer2DTextures.LabelText = "Image n.";
		this.multiViewer2DTextures.Location = new System.Drawing.Point(3, 59);
		this.multiViewer2DTextures.Name = "multiViewer2DTextures";
		this.multiViewer2DTextures.ShowDeleteButton = false;
		this.multiViewer2DTextures.Size = new System.Drawing.Size(512, 552);
		this.multiViewer2DTextures.TabIndex = 104;
		this.groupLights.Controls.Add(this.comboStadiumLights);
		this.groupLights.Controls.Add(this.buttonCopyCrowd);
		this.groupLights.Controls.Add(this.label3);
		this.groupLights.Location = new System.Drawing.Point(3, 617);
		this.groupLights.Name = "groupLights";
		this.groupLights.Size = new System.Drawing.Size(289, 121);
		this.groupLights.TabIndex = 111;
		this.groupLights.TabStop = false;
		this.groupLights.Text = "Crowd, Glares and Radiosity";
		this.comboStadiumLights.DataSource = this.stadiumListBindingSource;
		this.comboStadiumLights.FormattingEnabled = true;
		this.comboStadiumLights.Location = new System.Drawing.Point(17, 49);
		this.comboStadiumLights.Name = "comboStadiumLights";
		this.comboStadiumLights.Size = new System.Drawing.Size(247, 21);
		this.comboStadiumLights.TabIndex = 2;
		this.comboStadiumLights.SelectedIndexChanged += new System.EventHandler(comboStadiumLights_SelectedIndexChanged);
		this.stadiumListBindingSource.DataSource = typeof(FifaLibrary.StadiumList);
		this.buttonCopyCrowd.Enabled = false;
		this.buttonCopyCrowd.Image = (System.Drawing.Image)resources.GetObject("buttonCopyCrowd.Image");
		this.buttonCopyCrowd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonCopyCrowd.Location = new System.Drawing.Point(17, 76);
		this.buttonCopyCrowd.Name = "buttonCopyCrowd";
		this.buttonCopyCrowd.Size = new System.Drawing.Size(247, 25);
		this.buttonCopyCrowd.TabIndex = 102;
		this.buttonCopyCrowd.Text = "Copy Files";
		this.buttonCopyCrowd.UseVisualStyleBackColor = true;
		this.buttonCopyCrowd.Click += new System.EventHandler(buttonCopyCrowd_Click);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(62, 28);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(146, 13);
		this.label3.TabIndex = 1;
		this.label3.Text = "Select a stadium to copy from";
		this.groupBox5.Controls.Add(this.multiViewer2DCoverMap);
		this.groupBox5.Location = new System.Drawing.Point(521, 3);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(312, 604);
		this.groupBox5.TabIndex = 109;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "Shadow Map";
		this.groupBox5.Visible = false;
		this.multiViewer2DCoverMap.AutoTransparency = false;
		this.multiViewer2DCoverMap.Bitmaps = null;
		this.multiViewer2DCoverMap.CheckBitmapSize = false;
		this.multiViewer2DCoverMap.FixedSize = false;
		this.multiViewer2DCoverMap.FullSizeButton = false;
		this.multiViewer2DCoverMap.LabelText = "Image n.";
		this.multiViewer2DCoverMap.Location = new System.Drawing.Point(6, 19);
		this.multiViewer2DCoverMap.Name = "multiViewer2DCoverMap";
		this.multiViewer2DCoverMap.ShowDeleteButton = false;
		this.multiViewer2DCoverMap.Size = new System.Drawing.Size(301, 346);
		this.multiViewer2DCoverMap.TabIndex = 108;
		this.multiViewer2DCoverMap.Visible = false;
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = true;
		this.pickUpControl.CreateButtonEnabled = false;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = new string[2] { "All", "by Country" };
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
		this.pickUpControl.TabIndex = 0;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		this.radioPreviewOvercast.AutoSize = true;
		this.radioPreviewOvercast.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioPreviewOvercast.Location = new System.Drawing.Point(173, 19);
		this.radioPreviewOvercast.Name = "radioPreviewOvercast";
		this.radioPreviewOvercast.Size = new System.Drawing.Size(90, 17);
		this.radioPreviewOvercast.TabIndex = 102;
		this.radioPreviewOvercast.Text = "Overcast Day";
		this.radioPreviewOvercast.UseVisualStyleBackColor = true;
		this.radioPreviewOvercast.CheckedChanged += new System.EventHandler(radioPreviewOvercast_CheckedChanged);
		this.radioPreviewSunset.AutoSize = true;
		this.radioPreviewSunset.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioPreviewSunset.Location = new System.Drawing.Point(283, 19);
		this.radioPreviewSunset.Name = "radioPreviewSunset";
		this.radioPreviewSunset.Size = new System.Drawing.Size(58, 17);
		this.radioPreviewSunset.TabIndex = 103;
		this.radioPreviewSunset.Text = "Sunset";
		this.radioPreviewSunset.UseVisualStyleBackColor = true;
		this.radioPreviewSunset.CheckedChanged += new System.EventHandler(radioPreviewSunset_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1357, 832);
		base.Controls.Add(this.tabEsitStadium);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "StadiumForm";
		this.Text = "StadiumForm";
		base.Load += new System.EventHandler(StadiumForm_Load);
		this.tabEsitStadium.ResumeLayout(false);
		this.pageStadiumGeneral.ResumeLayout(false);
		this.flowLayoutPanel1.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCrowdColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericStadiumId).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHomeTeam).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericYearBuilt).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericCapacity).EndInit();
		this.groupMowingPattern.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericMowing).EndInit();
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNet).EndInit();
		this.groupBox6.ResumeLayout(false);
		this.groupCamera.ResumeLayout(false);
		this.groupCamera.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCameraZoom).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericCameraHeight).EndInit();
		this.groupAdboards.ResumeLayout(false);
		this.groupAdboards.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericAdboardType).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericSideLineDistance).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericEndLineDistance).EndInit();
		this.groupTimeAndWeather.ResumeLayout(false);
		this.groupTimeAndWeather.PerformLayout();
		this.groupPolice.ResumeLayout(false);
		this.groupBox8.ResumeLayout(false);
		this.groupBox10.ResumeLayout(false);
		this.groupBox10.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMaxZ).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMinZ).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMaxX).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneAwayMinX).EndInit();
		this.groupBox9.ResumeLayout(false);
		this.groupBox9.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMaxZ).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMinZ).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMaxX).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTechZoneHomeMinX).EndInit();
		this.pageStadiumPreview.ResumeLayout(false);
		this.groupEnvironment.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		this.pageStadiumModel.ResumeLayout(false);
		this.flowLayoutPanel2.ResumeLayout(false);
		this.groupBox7.ResumeLayout(false);
		this.groupBox7.PerformLayout();
		this.groupLights.ResumeLayout(false);
		this.groupLights.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.stadiumListBindingSource).EndInit();
		this.groupBox5.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
