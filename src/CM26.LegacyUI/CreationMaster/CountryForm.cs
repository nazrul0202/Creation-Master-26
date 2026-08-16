using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class CountryForm : Form
{
	private Country m_CurrentCountry;

	private string m_NotPresent = "< Not Present >";

	private bool m_IsNationalTeamLocked;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private static Random m_Randomizer = new Random();

	private bool m_LockUserChanges;

	private int m_AssetLoadGeneration;

	private IContainer components;

	private FlowLayoutPanel flowLayoutPanel;

	private GroupBox groupBox;

	private NumericUpDown numericCountryId;

	private ComboBox comboContinent;

	private TextBox textLanguageName;

	private Label labelLanguageName;

	private TextBox textDatabaseCountryName;

	private Label labelDatabaseCountryName;

	private Label labelContinent;

	private Label labelCountrId;

	private Viewer2D viewer2DFlag;

	private Viewer2D viewer2DMiniFlag;

	private ToolTip toolTip;

	private NumericUpDown numericNationalTeam;

	private ComboBox comboNationalTeam;

	private PictureBox pictureNationalTeam;

	private Label labelNationalTeam;

	private Button buttonGetId;

	public PickUpControl pickUpControl;

	private BindingSource countryBindingSource;

	private BindingSource sponsorListBindingSource;

	private GroupBox groupAudio;

	private Label label10;

	private ComboBox comboChants;

	private Label label9;

	private Label label15;

	private Label label14;

	private Label label11;

	private ComboBox comboLanguage;

	private CheckBox checkCanWhistle;

	private CheckBox checkTauntKeeper;

	private ComboBox comboPlayerCall;

	private ComboBox comboCrowdType;

	private ComboBox comboPepper;

	private CheckBox checkTopTier;

	private Viewer2D viewer2DCardFlag;

	private Label label3;

	private Label label2;

	private Label label1;

	private GroupBox groupCountryShape;

	private Viewer2D viewer2DShape;

	private ComboBox comboRegionalTarget;

	private ComboBox comboWorkltarget;

	private Label labeRegionalTarget;

	private Label labelWorldTarget;

	private Label label4;

	private NumericUpDown numericLevel;

	private Label labelContry3Letters;

	private TextBox textLanguageAbbreviation;

	private TextBox textLanguageShortName;

	private Label labelNationShortName;

	private Viewer2D viewer2DFlag512;

	private TextBox textIsoCountryCode;

	private Label label5;

	private GroupBox groupBox1;

	public CountryForm()
	{
		base.Visible = false;
		InitializeComponent();
		pickUpControl.SelectObject = SelectCountry;
		pickUpControl.CreateObject = CreateCountry;
		pickUpControl.DeleteObject = DeleteCountry;
		pickUpControl.RefreshObject = RefreshCountry;
		viewer2DFlag.ImageImport = ImportImageFlag;
		viewer2DFlag.ImageDelete = DeleteFlag;
		viewer2DFlag.ButtonStripVisible = true;
		viewer2DFlag.RemoveButton = true;
		viewer2DFlag512.ImageImport = ImportImageFlag512;
		viewer2DFlag512.ImageDelete = DeleteFlag512;
		viewer2DFlag512.ButtonStripVisible = true;
		viewer2DFlag512.RemoveButton = true;
		viewer2DMiniFlag.ImageImport = ImportImageMiniFlag;
		viewer2DMiniFlag.ImageDelete = DeleteMiniFlag;
		viewer2DMiniFlag.ButtonStripVisible = true;
		viewer2DMiniFlag.RemoveButton = true;
		viewer2DCardFlag.ImageImport = ImportImageCardFlag;
		viewer2DCardFlag.ImageDelete = DeleteCardFlag;
		viewer2DCardFlag.ButtonStripVisible = true;
		viewer2DCardFlag.RemoveButton = true;
		viewer2DShape.ImageImport = ImportImageShape;
		viewer2DShape.ImageDelete = DeleteShape;
		viewer2DShape.ButtonStripVisible = true;
		viewer2DShape.RemoveButton = true;
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.Countries;
		comboNationalTeam.Items.Clear();
		comboNationalTeam.BeginUpdate();
		comboNationalTeam.Items.Add(m_NotPresent);
		comboNationalTeam.Items.AddRange(FifaEnvironment.Teams.ToArray());
		int num = FifaEnvironment.Year == 26 ? 999 : FifaEnvironment.FifaDb.Table[TI.players].TableDescriptor.MaxValues[FI.players_nationality];
		if (num < 255)
		{
			num = 255;
		}
		numericCountryId.Maximum = num;
		comboNationalTeam.EndUpdate();
		pickUpControl.ObjectList = FifaEnvironment.Countries;
	}

	public void ReloadCountry(Country country)
	{
		m_CurrentCountry = null;
		LoadCountry(country);
	}

	public void LoadCountry(Country country)
	{
		if (m_IsLoaded && m_CurrentCountry != country)
		{
			m_LockUserChanges = true;
			m_CurrentCountry = country;
			countryBindingSource.DataSource = m_CurrentCountry;
			viewer2DFlag.CurrentBitmap = null;
			viewer2DFlag512.CurrentBitmap = null;
			viewer2DMiniFlag.CurrentBitmap = null;
			viewer2DCardFlag.CurrentBitmap = null;
			viewer2DShape.CurrentBitmap = null;
			pictureNationalTeam.BackgroundImage = null;
			m_LockUserChanges = false;
			LoadCountryAssetsAsync(country, ++m_AssetLoadGeneration);
		}
	}

	private async void LoadCountryAssetsAsync(Country country, int generation)
	{
		try
		{
			var paths = new System.Collections.Generic.List<string>
			{
				country.FlagBigFileName(), country.Flag512DdsFileName(), country.MiniFlagBigFileName(),
				country.CardFlagBigFileName(), country.ShapeFileName()
			};
			if (country.NationalTeam != null) paths.Add(country.NationalTeam.CrestDdsFileName());
			await System.Threading.Tasks.Task.Run(() => Fc26HostBridge.PreloadAssets(paths));
			if (IsDisposed || Disposing || m_CurrentCountry != country || generation != m_AssetLoadGeneration) return;
			viewer2DFlag.CurrentBitmap = country.GetFlag();
			viewer2DFlag512.CurrentBitmap = country.GetFlag512();
			viewer2DMiniFlag.CurrentBitmap = country.GetMiniFlag();
			viewer2DCardFlag.CurrentBitmap = country.GetCardFlag();
			viewer2DShape.CurrentBitmap = country.GetShape();
			pictureNationalTeam.BackgroundImage = country.NationalTeam?.GetCrest();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}

	private Country SelectCountry(object sender, object obj)
	{
		Country country = (Country)obj;
		Refresh();
		LoadCountry(country);
		return country;
	}

	private Country CreateCountry(object sender, object obj)
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
		return (Country)m_NewIdCreator.NewObject;
	}

	private Country DeleteCountry(object sender, object obj)
	{
		Country country = (Country)obj;
		FifaEnvironment.Countries.DeleteCountry(country);
		FifaEnvironment.Language.RemoveCountryString(country.Id, Language.ECountryStringType.Full);
		return null;
	}

	private Country CloneCountry(object sender, object obj)
	{
		Country srcIdObject = (Country)obj;
		return (Country)FifaEnvironment.Countries.CloneId(srcIdObject);
	}

	public Country RefreshCountry(object sender, object obj)
	{
		Preset();
		ReloadCountry(m_CurrentCountry);
		return m_CurrentCountry;
	}

	private bool ImportImageFlag(object sender, Bitmap bitmap)
	{
		return m_CurrentCountry.SetFlag(bitmap);
	}

	private bool DeleteFlag(object sender)
	{
		return m_CurrentCountry.DeleteFlag();
	}

	private bool ImportImageFlag512(object sender, Bitmap bitmap)
	{
		return m_CurrentCountry.SetFlag512(bitmap);
	}

	private bool DeleteFlag512(object sender)
	{
		return m_CurrentCountry.DeleteFlag512();
	}

	private bool ImportImageMiniFlag(object sender, Bitmap bitmap)
	{
		return m_CurrentCountry.SetMiniFlag(bitmap);
	}

	private bool DeleteMiniFlag(object sender)
	{
		return m_CurrentCountry.DeleteMiniFlag();
	}

	private bool ImportImageCardFlag(object sender, Bitmap bitmap)
	{
		return m_CurrentCountry.SetCardFlag(bitmap);
	}

	private bool DeleteCardFlag(object sender)
	{
		return m_CurrentCountry.DeleteCardFlag();
	}

	private bool ImportImageShape(object sender, Bitmap bitmap)
	{
		return m_CurrentCountry.SetShape(bitmap);
	}

	private bool DeleteShape(object sender)
	{
		return m_CurrentCountry.DeleteShape();
	}

	private void textLanguageName_TextChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			m_CurrentCountry.LanguageName = textLanguageName.Text;
			pickUpControl.SwitchObject(m_CurrentCountry);
		}
	}

	private void numericCountryId_ValueChanged(object sender, EventArgs e)
	{
		int num = (int)numericCountryId.Value;
		if (num != m_CurrentCountry.Id)
		{
			if (FifaEnvironment.Countries.SearchId(num) == null)
			{
				FifaEnvironment.Countries.ChangeId(m_CurrentCountry, num);
				ReloadCountry(m_CurrentCountry);
			}
			else
			{
				FifaEnvironment.UserMessages.ShowMessage(1015);
				numericCountryId.Value = m_CurrentCountry.Id;
			}
		}
	}

	private void buttonGetId_Click(object sender, EventArgs e)
	{
		int newId = FifaEnvironment.Countries.GetNewId();
		if (newId == -1)
		{
			FifaEnvironment.UserMessages.ShowMessage(5050);
		}
		else
		{
			numericCountryId.Value = newId;
		}
	}

	private void numericNationalTeam_ValueChanged(object sender, EventArgs e)
	{
		if (!m_IsNationalTeamLocked)
		{
			m_IsNationalTeamLocked = true;
			int num = (int)numericNationalTeam.Value;
			Team team = (Team)FifaEnvironment.Teams.SearchId(num);
			if (team != null)
			{
				comboNationalTeam.SelectedItem = team;
			}
			else
			{
				comboNationalTeam.SelectedItem = m_NotPresent;
			}
			if (num == m_CurrentCountry.NationalTeamId)
			{
				m_IsNationalTeamLocked = false;
			}
			else if (num > 0 && FifaEnvironment.Countries.SearchNationalTeamId(num) != null)
			{
				numericNationalTeam.Value = m_CurrentCountry.NationalTeamId;
				FifaEnvironment.UserMessages.ShowMessage(1014);
				m_IsNationalTeamLocked = false;
			}
			else
			{
				m_CurrentCountry.SetNationalTeam(team, num);
				pictureNationalTeam.BackgroundImage = team?.GetCrest();
				m_IsNationalTeamLocked = false;
			}
		}
	}

	private void comboNationalTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_IsNationalTeamLocked)
		{
			m_IsNationalTeamLocked = true;
			int num;
			Team team;
			if (comboNationalTeam.SelectedItem.ToString() == m_NotPresent)
			{
				num = -1;
				team = null;
			}
			else
			{
				team = (Team)comboNationalTeam.SelectedItem;
				num = team.Id;
			}
			if (team == m_CurrentCountry.NationalTeam)
			{
				m_IsNationalTeamLocked = false;
			}
			else if (num > 0 && FifaEnvironment.Countries.SearchNationalTeamId(num) != null)
			{
				comboNationalTeam.SelectedItem = m_CurrentCountry.NationalTeam;
				FifaEnvironment.UserMessages.ShowMessage(1014);
				m_IsNationalTeamLocked = false;
			}
			else
			{
				numericNationalTeam.Value = num;
				m_CurrentCountry.SetNationalTeam(team, num);
				pictureNationalTeam.BackgroundImage = team?.GetCrest();
				m_IsNationalTeamLocked = false;
			}
		}
	}

	private void pictureNationalTeam_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentCountry.NationalTeam != null)
		{
			MainForm.CM.JumpTo(m_CurrentCountry.NationalTeam);
		}
	}

	private void CountryForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void numericTeamPrestige_ValueChanged(object sender, EventArgs e)
	{
	}

	private void buttonRandomize1_Click(object sender, EventArgs e)
	{
	}

	private void buttonRandomize2_Click(object sender, EventArgs e)
	{
	}

	private void buttonRandomiz23_Click(object sender, EventArgs e)
	{
	}

	private void buttonRandomize4_Click(object sender, EventArgs e)
	{
	}

	private void buttonSpain_Click(object sender, EventArgs e)
	{
	}

	private void buttonFrance_Click(object sender, EventArgs e)
	{
	}

	private void buttonItaly_Click(object sender, EventArgs e)
	{
	}

	private void buttonGermany_Click(object sender, EventArgs e)
	{
	}

	private void buttonScotland_Click(object sender, EventArgs e)
	{
	}

	private void buttonAustria_Click(object sender, EventArgs e)
	{
	}

	private void buttonBrazil_Click(object sender, EventArgs e)
	{
	}

	private void buttonCzech_Click(object sender, EventArgs e)
	{
	}

	private void buttonKorea_Click(object sender, EventArgs e)
	{
	}

	private void LoadAudio()
	{
		comboChants.SelectedIndex = m_CurrentCountry.ChantRegionIndex - 1;
		comboLanguage.SelectedIndex = m_CurrentCountry.PALanguageIndex;
		comboPlayerCall.SelectedIndex = m_CurrentCountry.PlayerCallPatchBankIndex;
		switch (m_CurrentCountry.CrowdBedsRegionIndex)
		{
		case 0:
			comboCrowdType.SelectedIndex = 0;
			break;
		case 8:
			comboCrowdType.SelectedIndex = 1;
			break;
		case 15:
			comboCrowdType.SelectedIndex = 2;
			break;
		}
		checkCanWhistle.Checked = m_CurrentCountry.TeamCanWhistleIndex == 1;
	}

	private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_CurrentCountry.PALanguageIndex = comboLanguage.SelectedIndex;
	}

	private void comboChants_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_CurrentCountry.ChantRegionIndex = comboChants.SelectedIndex + 1;
	}

	private void comboCrowdType_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (comboCrowdType.SelectedIndex)
		{
		case 0:
			m_CurrentCountry.CrowdBedsRegionIndex = 0;
			break;
		case 1:
			m_CurrentCountry.CrowdBedsRegionIndex = 8;
			break;
		case 2:
			m_CurrentCountry.CrowdBedsRegionIndex = 15;
			break;
		}
	}

	private void comboPlayerCall_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_CurrentCountry.PlayerCallPatchBankIndex = comboPlayerCall.SelectedIndex;
	}

	private void comboPepper_SelectedIndexChanged(object sender, EventArgs e)
	{
	}

	private void checkTauntKeeper_CheckedChanged(object sender, EventArgs e)
	{
	}

	private void checkCanWhistle_CheckedChanged(object sender, EventArgs e)
	{
		m_CurrentCountry.TeamCanWhistleIndex = (checkCanWhistle.Checked ? 1 : 0);
	}

	private void textLanguageShortName_TextChanged(object sender, EventArgs e)
	{
		if (textLanguageShortName.Text.Length > 15)
		{
			textLanguageShortName.Text = textLanguageShortName.Text.Substring(0, 15);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.CountryForm));
		this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.groupBox = new System.Windows.Forms.GroupBox();
		this.textIsoCountryCode = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.labelContry3Letters = new System.Windows.Forms.Label();
		this.textLanguageAbbreviation = new System.Windows.Forms.TextBox();
		this.textLanguageShortName = new System.Windows.Forms.TextBox();
		this.labelNationShortName = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.numericLevel = new System.Windows.Forms.NumericUpDown();
		this.comboRegionalTarget = new System.Windows.Forms.ComboBox();
		this.comboWorkltarget = new System.Windows.Forms.ComboBox();
		this.labeRegionalTarget = new System.Windows.Forms.Label();
		this.labelWorldTarget = new System.Windows.Forms.Label();
		this.checkTopTier = new System.Windows.Forms.CheckBox();
		this.buttonGetId = new System.Windows.Forms.Button();
		this.numericCountryId = new System.Windows.Forms.NumericUpDown();
		this.comboContinent = new System.Windows.Forms.ComboBox();
		this.textLanguageName = new System.Windows.Forms.TextBox();
		this.labelLanguageName = new System.Windows.Forms.Label();
		this.textDatabaseCountryName = new System.Windows.Forms.TextBox();
		this.labelDatabaseCountryName = new System.Windows.Forms.Label();
		this.labelContinent = new System.Windows.Forms.Label();
		this.labelCountrId = new System.Windows.Forms.Label();
		this.groupCountryShape = new System.Windows.Forms.GroupBox();
		this.groupAudio = new System.Windows.Forms.GroupBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.comboPepper = new System.Windows.Forms.ComboBox();
		this.comboPlayerCall = new System.Windows.Forms.ComboBox();
		this.comboCrowdType = new System.Windows.Forms.ComboBox();
		this.checkCanWhistle = new System.Windows.Forms.CheckBox();
		this.checkTauntKeeper = new System.Windows.Forms.CheckBox();
		this.comboLanguage = new System.Windows.Forms.ComboBox();
		this.label15 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.comboChants = new System.Windows.Forms.ComboBox();
		this.label9 = new System.Windows.Forms.Label();
		this.numericNationalTeam = new System.Windows.Forms.NumericUpDown();
		this.labelNationalTeam = new System.Windows.Forms.Label();
		this.pictureNationalTeam = new System.Windows.Forms.PictureBox();
		this.comboNationalTeam = new System.Windows.Forms.ComboBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.sponsorListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.countryBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.viewer2DFlag512 = new FifaControls.Viewer2D();
		this.viewer2DFlag = new FifaControls.Viewer2D();
		this.viewer2DCardFlag = new FifaControls.Viewer2D();
		this.viewer2DMiniFlag = new FifaControls.Viewer2D();
		this.viewer2DShape = new FifaControls.Viewer2D();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.flowLayoutPanel.SuspendLayout();
		this.groupBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericLevel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericCountryId).BeginInit();
		this.groupCountryShape.SuspendLayout();
		this.groupAudio.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNationalTeam).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureNationalTeam).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sponsorListBindingSource).BeginInit();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.countryBindingSource).BeginInit();
		base.SuspendLayout();
		this.flowLayoutPanel.AutoScroll = true;
		this.flowLayoutPanel.Controls.Add(this.groupBox);
		this.flowLayoutPanel.Controls.Add(this.groupCountryShape);
		this.flowLayoutPanel.Controls.Add(this.groupAudio);
		this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowLayoutPanel.Location = new System.Drawing.Point(0, 25);
		this.flowLayoutPanel.Name = "flowLayoutPanel";
		this.flowLayoutPanel.Size = new System.Drawing.Size(1357, 807);
		this.flowLayoutPanel.TabIndex = 0;
		this.groupBox.Controls.Add(this.groupBox1);
		this.groupBox.Controls.Add(this.textIsoCountryCode);
		this.groupBox.Controls.Add(this.label5);
		this.groupBox.Controls.Add(this.viewer2DFlag512);
		this.groupBox.Controls.Add(this.labelContry3Letters);
		this.groupBox.Controls.Add(this.textLanguageAbbreviation);
		this.groupBox.Controls.Add(this.textLanguageShortName);
		this.groupBox.Controls.Add(this.labelNationShortName);
		this.groupBox.Controls.Add(this.label4);
		this.groupBox.Controls.Add(this.numericLevel);
		this.groupBox.Controls.Add(this.checkTopTier);
		this.groupBox.Controls.Add(this.buttonGetId);
		this.groupBox.Controls.Add(this.viewer2DFlag);
		this.groupBox.Controls.Add(this.viewer2DCardFlag);
		this.groupBox.Controls.Add(this.viewer2DMiniFlag);
		this.groupBox.Controls.Add(this.numericCountryId);
		this.groupBox.Controls.Add(this.comboContinent);
		this.groupBox.Controls.Add(this.textLanguageName);
		this.groupBox.Controls.Add(this.labelLanguageName);
		this.groupBox.Controls.Add(this.textDatabaseCountryName);
		this.groupBox.Controls.Add(this.labelDatabaseCountryName);
		this.groupBox.Controls.Add(this.labelContinent);
		this.groupBox.Controls.Add(this.labelCountrId);
		this.groupBox.Location = new System.Drawing.Point(3, 1);
		this.groupBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
		this.groupBox.Name = "groupBox";
		this.groupBox.Size = new System.Drawing.Size(767, 489);
		this.groupBox.TabIndex = 0;
		this.groupBox.TabStop = false;
		this.textIsoCountryCode.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.countryBindingSource, "IsoCountryCode", true));
		this.textIsoCountryCode.Location = new System.Drawing.Point(117, 195);
		this.textIsoCountryCode.Name = "textIsoCountryCode";
		this.textIsoCountryCode.Size = new System.Drawing.Size(117, 20);
		this.textIsoCountryCode.TabIndex = 164;
		this.textIsoCountryCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.label5.AutoSize = true;
		this.label5.BackColor = System.Drawing.Color.Transparent;
		this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label5.Location = new System.Drawing.Point(11, 198);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(92, 13);
		this.label5.TabIndex = 163;
		this.label5.Text = "ISO Country Code";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelContry3Letters.AutoSize = true;
		this.labelContry3Letters.BackColor = System.Drawing.Color.Transparent;
		this.labelContry3Letters.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelContry3Letters.Location = new System.Drawing.Point(11, 137);
		this.labelContry3Letters.Name = "labelContry3Letters";
		this.labelContry3Letters.Size = new System.Drawing.Size(66, 13);
		this.labelContry3Letters.TabIndex = 161;
		this.labelContry3Letters.Text = "Abbreviation";
		this.labelContry3Letters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textLanguageAbbreviation.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.countryBindingSource, "LanguageAbbreviation", true));
		this.textLanguageAbbreviation.Location = new System.Drawing.Point(101, 134);
		this.textLanguageAbbreviation.Name = "textLanguageAbbreviation";
		this.textLanguageAbbreviation.Size = new System.Drawing.Size(133, 20);
		this.textLanguageAbbreviation.TabIndex = 160;
		this.textLanguageShortName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.countryBindingSource, "LanguageShortName", true));
		this.textLanguageShortName.Location = new System.Drawing.Point(101, 104);
		this.textLanguageShortName.Name = "textLanguageShortName";
		this.textLanguageShortName.Size = new System.Drawing.Size(133, 20);
		this.textLanguageShortName.TabIndex = 158;
		this.textLanguageShortName.TextChanged += new System.EventHandler(textLanguageShortName_TextChanged);
		this.labelNationShortName.AutoSize = true;
		this.labelNationShortName.BackColor = System.Drawing.Color.Transparent;
		this.labelNationShortName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelNationShortName.Location = new System.Drawing.Point(11, 107);
		this.labelNationShortName.Name = "labelNationShortName";
		this.labelNationShortName.Size = new System.Drawing.Size(63, 13);
		this.labelNationShortName.TabIndex = 159;
		this.labelNationShortName.Text = "Short Name";
		this.labelNationShortName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label4.AutoSize = true;
		this.label4.BackColor = System.Drawing.Color.Transparent;
		this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label4.Location = new System.Drawing.Point(11, 227);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(33, 13);
		this.label4.TabIndex = 157;
		this.label4.Text = "Level";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericLevel.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.countryBindingSource, "Level", true));
		this.numericLevel.Location = new System.Drawing.Point(117, 225);
		this.numericLevel.Maximum = new decimal(new int[4] { 7, 0, 0, 0 });
		this.numericLevel.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericLevel.Name = "numericLevel";
		this.numericLevel.Size = new System.Drawing.Size(115, 20);
		this.numericLevel.TabIndex = 156;
		this.numericLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericLevel.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.comboRegionalTarget.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.countryBindingSource, "ContinentalCupTarget", true));
		this.comboRegionalTarget.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.comboRegionalTarget.ItemHeight = 13;
		this.comboRegionalTarget.Items.AddRange(new object[7] { "N/A", "WIN", "FINAL", "SEMI", "QUARTER", "KNOCKOUT", "QUALIFY" });
		this.comboRegionalTarget.Location = new System.Drawing.Point(117, 44);
		this.comboRegionalTarget.Name = "comboRegionalTarget";
		this.comboRegionalTarget.Size = new System.Drawing.Size(102, 21);
		this.comboRegionalTarget.TabIndex = 155;
		this.comboWorkltarget.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.countryBindingSource, "WorldCupTarget", true));
		this.comboWorkltarget.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.comboWorkltarget.ItemHeight = 13;
		this.comboWorkltarget.Items.AddRange(new object[7] { "N/A", "WIN", "FINAL", "SEMI", "QUARTER", "KNOCKOUT", "QUALIFY" });
		this.comboWorkltarget.Location = new System.Drawing.Point(117, 18);
		this.comboWorkltarget.Name = "comboWorkltarget";
		this.comboWorkltarget.Size = new System.Drawing.Size(102, 21);
		this.comboWorkltarget.TabIndex = 154;
		this.labeRegionalTarget.AutoSize = true;
		this.labeRegionalTarget.BackColor = System.Drawing.SystemColors.Control;
		this.labeRegionalTarget.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labeRegionalTarget.Location = new System.Drawing.Point(6, 45);
		this.labeRegionalTarget.Name = "labeRegionalTarget";
		this.labeRegionalTarget.Size = new System.Drawing.Size(105, 13);
		this.labeRegionalTarget.TabIndex = 153;
		this.labeRegionalTarget.Text = "Regional Cup Target";
		this.labeRegionalTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelWorldTarget.AutoSize = true;
		this.labelWorldTarget.BackColor = System.Drawing.SystemColors.Control;
		this.labelWorldTarget.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelWorldTarget.Location = new System.Drawing.Point(6, 20);
		this.labelWorldTarget.Name = "labelWorldTarget";
		this.labelWorldTarget.Size = new System.Drawing.Size(91, 13);
		this.labelWorldTarget.TabIndex = 152;
		this.labelWorldTarget.Text = "World Cup Target";
		this.labelWorldTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.checkTopTier.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.countryBindingSource, "Top_tier", true));
		this.checkTopTier.Location = new System.Drawing.Point(11, 255);
		this.checkTopTier.Name = "checkTopTier";
		this.checkTopTier.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkTopTier.Size = new System.Drawing.Size(164, 18);
		this.checkTopTier.TabIndex = 151;
		this.checkTopTier.Text = "Top tier";
		this.checkTopTier.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkTopTier.UseVisualStyleBackColor = true;
		this.buttonGetId.Image = (System.Drawing.Image)resources.GetObject("buttonGetId.Image");
		this.buttonGetId.Location = new System.Drawing.Point(207, 37);
		this.buttonGetId.Name = "buttonGetId";
		this.buttonGetId.Size = new System.Drawing.Size(25, 23);
		this.buttonGetId.TabIndex = 150;
		this.toolTip.SetToolTip(this.buttonGetId, "Get a free id");
		this.buttonGetId.UseVisualStyleBackColor = true;
		this.buttonGetId.Click += new System.EventHandler(buttonGetId_Click);
		this.numericCountryId.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.countryBindingSource, "Id", true));
		this.numericCountryId.Location = new System.Drawing.Point(101, 44);
		this.numericCountryId.Maximum = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericCountryId.Name = "numericCountryId";
		this.numericCountryId.Size = new System.Drawing.Size(100, 20);
		this.numericCountryId.TabIndex = 143;
		this.numericCountryId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCountryId.ValueChanged += new System.EventHandler(numericCountryId_ValueChanged);
		this.comboContinent.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.countryBindingSource, "Confederation", true));
		this.comboContinent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.comboContinent.ItemHeight = 13;
		this.comboContinent.Items.AddRange(new object[7] { "None", "Europe", "Africa", "South America", "Asia", "Oceania", "North America" });
		this.comboContinent.Location = new System.Drawing.Point(101, 164);
		this.comboContinent.Name = "comboContinent";
		this.comboContinent.Size = new System.Drawing.Size(133, 21);
		this.comboContinent.TabIndex = 145;
		this.textLanguageName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.countryBindingSource, "LanguageName", true));
		this.textLanguageName.Location = new System.Drawing.Point(101, 74);
		this.textLanguageName.Name = "textLanguageName";
		this.textLanguageName.Size = new System.Drawing.Size(133, 20);
		this.textLanguageName.TabIndex = 144;
		this.textLanguageName.TextChanged += new System.EventHandler(textLanguageName_TextChanged);
		this.labelLanguageName.AutoSize = true;
		this.labelLanguageName.BackColor = System.Drawing.Color.Transparent;
		this.labelLanguageName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLanguageName.Location = new System.Drawing.Point(11, 77);
		this.labelLanguageName.Name = "labelLanguageName";
		this.labelLanguageName.Size = new System.Drawing.Size(35, 13);
		this.labelLanguageName.TabIndex = 147;
		this.labelLanguageName.Text = "Name";
		this.labelLanguageName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textDatabaseCountryName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.countryBindingSource, "DatabaseName", true));
		this.textDatabaseCountryName.Location = new System.Drawing.Point(101, 14);
		this.textDatabaseCountryName.Name = "textDatabaseCountryName";
		this.textDatabaseCountryName.Size = new System.Drawing.Size(133, 20);
		this.textDatabaseCountryName.TabIndex = 142;
		this.labelDatabaseCountryName.AutoSize = true;
		this.labelDatabaseCountryName.BackColor = System.Drawing.Color.Transparent;
		this.labelDatabaseCountryName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDatabaseCountryName.Location = new System.Drawing.Point(11, 18);
		this.labelDatabaseCountryName.Name = "labelDatabaseCountryName";
		this.labelDatabaseCountryName.Size = new System.Drawing.Size(84, 13);
		this.labelDatabaseCountryName.TabIndex = 146;
		this.labelDatabaseCountryName.Text = "Database Name";
		this.labelDatabaseCountryName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelContinent.AutoSize = true;
		this.labelContinent.BackColor = System.Drawing.Color.Transparent;
		this.labelContinent.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelContinent.Location = new System.Drawing.Point(11, 167);
		this.labelContinent.Name = "labelContinent";
		this.labelContinent.Size = new System.Drawing.Size(73, 13);
		this.labelContinent.TabIndex = 148;
		this.labelContinent.Text = "Confederation";
		this.labelContinent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCountrId.AutoSize = true;
		this.labelCountrId.BackColor = System.Drawing.Color.Transparent;
		this.labelCountrId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCountrId.Location = new System.Drawing.Point(11, 47);
		this.labelCountrId.Name = "labelCountrId";
		this.labelCountrId.Size = new System.Drawing.Size(55, 13);
		this.labelCountrId.TabIndex = 149;
		this.labelCountrId.Text = "Country Id";
		this.labelCountrId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupCountryShape.Controls.Add(this.viewer2DShape);
		this.groupCountryShape.Location = new System.Drawing.Point(776, 3);
		this.groupCountryShape.Name = "groupCountryShape";
		this.groupCountryShape.Size = new System.Drawing.Size(528, 308);
		this.groupCountryShape.TabIndex = 4;
		this.groupCountryShape.TabStop = false;
		this.groupCountryShape.Text = "Map (Shape)";
		this.groupAudio.Controls.Add(this.label3);
		this.groupAudio.Controls.Add(this.label2);
		this.groupAudio.Controls.Add(this.label1);
		this.groupAudio.Controls.Add(this.comboPepper);
		this.groupAudio.Controls.Add(this.comboPlayerCall);
		this.groupAudio.Controls.Add(this.comboCrowdType);
		this.groupAudio.Controls.Add(this.checkCanWhistle);
		this.groupAudio.Controls.Add(this.checkTauntKeeper);
		this.groupAudio.Controls.Add(this.comboLanguage);
		this.groupAudio.Controls.Add(this.label15);
		this.groupAudio.Controls.Add(this.label14);
		this.groupAudio.Controls.Add(this.label11);
		this.groupAudio.Controls.Add(this.label10);
		this.groupAudio.Controls.Add(this.labelNationalTeam);
		this.groupAudio.Controls.Add(this.comboChants);
		this.groupAudio.Controls.Add(this.label9);
		this.groupAudio.Controls.Add(this.numericNationalTeam);
		this.groupAudio.Controls.Add(this.comboNationalTeam);
		this.groupAudio.Location = new System.Drawing.Point(3, 496);
		this.groupAudio.Name = "groupAudio";
		this.groupAudio.Size = new System.Drawing.Size(624, 250);
		this.groupAudio.TabIndex = 3;
		this.groupAudio.TabStop = false;
		this.groupAudio.Text = "Audio";
		this.groupAudio.Visible = false;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(11, 219);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(55, 13);
		this.label3.TabIndex = 33;
		this.label3.Text = "Reactions";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(11, 192);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(46, 13);
		this.label2.TabIndex = 32;
		this.label2.Text = "Heckles";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(9, 165);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(54, 13);
		this.label1.TabIndex = 31;
		this.label1.Text = "Ambience";
		this.comboPepper.FormattingEnabled = true;
		this.comboPepper.Items.AddRange(new object[8] { "Undefined", "English", "French", "Italian", "German", "Spanish", "Scandinavian", "Brazilian" });
		this.comboPepper.Location = new System.Drawing.Point(88, 132);
		this.comboPepper.Name = "comboPepper";
		this.comboPepper.Size = new System.Drawing.Size(145, 21);
		this.comboPepper.TabIndex = 29;
		this.comboPepper.SelectedIndexChanged += new System.EventHandler(comboPepper_SelectedIndexChanged);
		this.comboPlayerCall.FormattingEnabled = true;
		this.comboPlayerCall.Items.AddRange(new object[19]
		{
			"English", "French", "Italian", "German", "Spanish", "Brazilian", "Japaneese", "Korean", "Dutch", "Danish",
			"Swedish", "Norwegian", "Portuguese", "Russian", "US English", "Iranian", "Indian", "Chineese", "Arabic"
		});
		this.comboPlayerCall.Location = new System.Drawing.Point(88, 105);
		this.comboPlayerCall.Name = "comboPlayerCall";
		this.comboPlayerCall.Size = new System.Drawing.Size(145, 21);
		this.comboPlayerCall.TabIndex = 28;
		this.comboPlayerCall.SelectedIndexChanged += new System.EventHandler(comboPlayerCall_SelectedIndexChanged);
		this.comboCrowdType.FormattingEnabled = true;
		this.comboCrowdType.Items.AddRange(new object[3] { " 0 = English", " 8 = Brazilian", "15 = Rest of World" });
		this.comboCrowdType.Location = new System.Drawing.Point(88, 78);
		this.comboCrowdType.Name = "comboCrowdType";
		this.comboCrowdType.Size = new System.Drawing.Size(145, 21);
		this.comboCrowdType.TabIndex = 27;
		this.comboCrowdType.SelectedIndexChanged += new System.EventHandler(comboCrowdType_SelectedIndexChanged);
		this.checkCanWhistle.AutoSize = true;
		this.checkCanWhistle.Location = new System.Drawing.Point(259, 48);
		this.checkCanWhistle.Name = "checkCanWhistle";
		this.checkCanWhistle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkCanWhistle.Size = new System.Drawing.Size(83, 17);
		this.checkCanWhistle.TabIndex = 26;
		this.checkCanWhistle.Text = "Can Whistle";
		this.checkCanWhistle.UseVisualStyleBackColor = true;
		this.checkCanWhistle.CheckedChanged += new System.EventHandler(checkCanWhistle_CheckedChanged);
		this.checkTauntKeeper.AutoSize = true;
		this.checkTauntKeeper.Location = new System.Drawing.Point(251, 25);
		this.checkTauntKeeper.Name = "checkTauntKeeper";
		this.checkTauntKeeper.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkTauntKeeper.Size = new System.Drawing.Size(91, 17);
		this.checkTauntKeeper.TabIndex = 25;
		this.checkTauntKeeper.Text = "Taunt Keeper";
		this.checkTauntKeeper.UseVisualStyleBackColor = true;
		this.checkTauntKeeper.CheckedChanged += new System.EventHandler(checkTauntKeeper_CheckedChanged);
		this.comboLanguage.FormattingEnabled = true;
		this.comboLanguage.Items.AddRange(new object[18]
		{
			"English ", "French ", "German ", "Italian ", "Spanish from Spain ", "Croatian", "Czech", "Dutch", "Greek", "Polish ",
			"Russian", "Swedish", "Turkish", "Spanish from Mexico ", "Spanish from Argentina ", "Brazilian Portuguese", "Korean", "Japanese"
		});
		this.comboLanguage.Location = new System.Drawing.Point(89, 24);
		this.comboLanguage.Name = "comboLanguage";
		this.comboLanguage.Size = new System.Drawing.Size(144, 21);
		this.comboLanguage.TabIndex = 24;
		this.comboLanguage.SelectedIndexChanged += new System.EventHandler(comboLanguage_SelectedIndexChanged);
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(9, 135);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(42, 13);
		this.label15.TabIndex = 23;
		this.label15.Text = "Whistle";
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(9, 108);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(56, 13);
		this.label14.TabIndex = 22;
		this.label14.Text = "Player Call";
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(9, 81);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(64, 13);
		this.label11.TabIndex = 19;
		this.label11.Text = "Crowd Type";
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(6, 27);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(55, 13);
		this.label10.TabIndex = 18;
		this.label10.Text = "Language";
		this.comboChants.FormattingEnabled = true;
		this.comboChants.Items.AddRange(new object[16]
		{
			"English Area", "French Area", "Italy", "German Area", "Spain", "Scandinavian Area", "Rest Of World", "Latin America", "Brazil", "Africa",
			"Asia", "Mexico", "Denmark", "Russian Area", "Portugal", "Turkey"
		});
		this.comboChants.Location = new System.Drawing.Point(89, 51);
		this.comboChants.Name = "comboChants";
		this.comboChants.Size = new System.Drawing.Size(144, 21);
		this.comboChants.TabIndex = 17;
		this.comboChants.SelectedIndexChanged += new System.EventHandler(comboChants_SelectedIndexChanged);
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(9, 54);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(40, 13);
		this.label9.TabIndex = 16;
		this.label9.Text = "Chants";
		this.numericNationalTeam.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.countryBindingSource, "NationalTeamId", true));
		this.numericNationalTeam.Location = new System.Drawing.Point(449, 27);
		this.numericNationalTeam.Maximum = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericNationalTeam.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericNationalTeam.Name = "numericNationalTeam";
		this.numericNationalTeam.Size = new System.Drawing.Size(133, 20);
		this.numericNationalTeam.TabIndex = 131;
		this.numericNationalTeam.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.toolTip.SetToolTip(this.numericNationalTeam, "Use this to assign a national team identifier though the national team does not exists");
		this.numericNationalTeam.ValueChanged += new System.EventHandler(numericNationalTeam_ValueChanged);
		this.labelNationalTeam.AutoSize = true;
		this.labelNationalTeam.BackColor = System.Drawing.SystemColors.Control;
		this.labelNationalTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelNationalTeam.Location = new System.Drawing.Point(266, 78);
		this.labelNationalTeam.Name = "labelNationalTeam";
		this.labelNationalTeam.Size = new System.Drawing.Size(76, 13);
		this.labelNationalTeam.TabIndex = 133;
		this.labelNationalTeam.Text = "National Team";
		this.labelNationalTeam.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pictureNationalTeam.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureNationalTeam.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureNationalTeam.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureNationalTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureNationalTeam.Location = new System.Drawing.Point(119, 71);
		this.pictureNationalTeam.Name = "pictureNationalTeam";
		this.pictureNationalTeam.Size = new System.Drawing.Size(100, 100);
		this.pictureNationalTeam.TabIndex = 134;
		this.pictureNationalTeam.TabStop = false;
		this.toolTip.SetToolTip(this.pictureNationalTeam, "Go to the team page for setting the National Team of a Country");
		this.pictureNationalTeam.DoubleClick += new System.EventHandler(pictureNationalTeam_DoubleClick);
		this.comboNationalTeam.ItemHeight = 13;
		this.comboNationalTeam.Location = new System.Drawing.Point(449, 50);
		this.comboNationalTeam.MaxLength = 32767;
		this.comboNationalTeam.Name = "comboNationalTeam";
		this.comboNationalTeam.Size = new System.Drawing.Size(133, 21);
		this.comboNationalTeam.Sorted = true;
		this.comboNationalTeam.TabIndex = 132;
		this.toolTip.SetToolTip(this.comboNationalTeam, "Use this to assign to the country an existing national team");
		this.comboNationalTeam.SelectedIndexChanged += new System.EventHandler(comboNationalTeam_SelectedIndexChanged);
		this.groupBox1.Controls.Add(this.pictureNationalTeam);
		this.groupBox1.Controls.Add(this.labelWorldTarget);
		this.groupBox1.Controls.Add(this.labeRegionalTarget);
		this.groupBox1.Controls.Add(this.comboWorkltarget);
		this.groupBox1.Controls.Add(this.comboRegionalTarget);
		this.groupBox1.Location = new System.Drawing.Point(9, 292);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(225, 184);
		this.groupBox1.TabIndex = 165;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "National Team";
		this.countryBindingSource.DataSource = typeof(FifaLibrary.Country);
		this.viewer2DFlag512.AutoTransparency = false;
		this.viewer2DFlag512.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DFlag512.ButtonStripVisible = true;
		this.viewer2DFlag512.CurrentBitmap = null;
		this.viewer2DFlag512.ExtendedFormat = false;
		this.viewer2DFlag512.FullSizeButton = false;
		this.viewer2DFlag512.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DFlag512.ImageSize = new System.Drawing.Size(512, 512);
		this.viewer2DFlag512.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DFlag512.Location = new System.Drawing.Point(502, 13);
		this.viewer2DFlag512.Name = "viewer2DFlag512";
		this.viewer2DFlag512.RemoveButton = false;
		this.viewer2DFlag512.ShowButton = true;
		this.viewer2DFlag512.ShowButtonChecked = true;
		this.viewer2DFlag512.Size = new System.Drawing.Size(256, 281);
		this.viewer2DFlag512.TabIndex = 162;
		this.toolTip.SetToolTip(this.viewer2DFlag512, "Country Flag 512 x 512");
		this.viewer2DFlag.AutoTransparency = true;
		this.viewer2DFlag.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DFlag.ButtonStripVisible = true;
		this.viewer2DFlag.CurrentBitmap = null;
		this.viewer2DFlag.ExtendedFormat = false;
		this.viewer2DFlag.FullSizeButton = false;
		this.viewer2DFlag.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DFlag.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DFlag.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.Auto256;
		this.viewer2DFlag.Location = new System.Drawing.Point(240, 13);
		this.viewer2DFlag.Name = "viewer2DFlag";
		this.viewer2DFlag.RemoveButton = false;
		this.viewer2DFlag.ShowButton = true;
		this.viewer2DFlag.ShowButtonChecked = true;
		this.viewer2DFlag.Size = new System.Drawing.Size(256, 281);
		this.viewer2DFlag.TabIndex = 1;
		this.toolTip.SetToolTip(this.viewer2DFlag, "Country Badge");
		this.viewer2DCardFlag.AutoTransparency = true;
		this.viewer2DCardFlag.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DCardFlag.ButtonStripVisible = true;
		this.viewer2DCardFlag.CurrentBitmap = null;
		this.viewer2DCardFlag.ExtendedFormat = false;
		this.viewer2DCardFlag.FullSizeButton = false;
		this.viewer2DCardFlag.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DCardFlag.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DCardFlag.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DCardFlag.Location = new System.Drawing.Point(240, 300);
		this.viewer2DCardFlag.Name = "viewer2DCardFlag";
		this.viewer2DCardFlag.RemoveButton = false;
		this.viewer2DCardFlag.ShowButton = true;
		this.viewer2DCardFlag.ShowButtonChecked = true;
		this.viewer2DCardFlag.Size = new System.Drawing.Size(150, 177);
		this.viewer2DCardFlag.TabIndex = 30;
		this.toolTip.SetToolTip(this.viewer2DCardFlag, "Country Flag");
		this.viewer2DMiniFlag.AutoTransparency = false;
		this.viewer2DMiniFlag.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DMiniFlag.ButtonStripVisible = true;
		this.viewer2DMiniFlag.CurrentBitmap = null;
		this.viewer2DMiniFlag.ExtendedFormat = false;
		this.viewer2DMiniFlag.FullSizeButton = false;
		this.viewer2DMiniFlag.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DMiniFlag.ImageSize = new System.Drawing.Size(64, 64);
		this.viewer2DMiniFlag.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DMiniFlag.Location = new System.Drawing.Point(502, 299);
		this.viewer2DMiniFlag.Name = "viewer2DMiniFlag";
		this.viewer2DMiniFlag.RemoveButton = false;
		this.viewer2DMiniFlag.ShowButton = true;
		this.viewer2DMiniFlag.ShowButtonChecked = true;
		this.viewer2DMiniFlag.Size = new System.Drawing.Size(64, 64);
		this.viewer2DMiniFlag.TabIndex = 2;
		this.viewer2DShape.AutoTransparency = true;
		this.viewer2DShape.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DShape.ButtonStripVisible = true;
		this.viewer2DShape.CurrentBitmap = null;
		this.viewer2DShape.ExtendedFormat = false;
		this.viewer2DShape.FullSizeButton = false;
		this.viewer2DShape.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DShape.ImageSize = new System.Drawing.Size(512, 256);
		this.viewer2DShape.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DShape.Location = new System.Drawing.Point(6, 16);
		this.viewer2DShape.Name = "viewer2DShape";
		this.viewer2DShape.RemoveButton = false;
		this.viewer2DShape.ShowButton = true;
		this.viewer2DShape.ShowButtonChecked = true;
		this.viewer2DShape.Size = new System.Drawing.Size(512, 281);
		this.viewer2DShape.TabIndex = 2;
		this.toolTip.SetToolTip(this.viewer2DShape, "Country Badge");
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = false;
		this.pickUpControl.CreateButtonEnabled = true;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = null;
		this.pickUpControl.FilterEnabled = false;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = true;
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = true;
		this.pickUpControl.SearchEnabled = true;
		this.pickUpControl.Size = new System.Drawing.Size(1357, 25);
		this.pickUpControl.TabIndex = 2;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1357, 832);
		base.Controls.Add(this.flowLayoutPanel);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "CountryForm";
		this.Text = "Country";
		base.Load += new System.EventHandler(CountryForm_Load);
		this.flowLayoutPanel.ResumeLayout(false);
		this.groupBox.ResumeLayout(false);
		this.groupBox.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericLevel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericCountryId).EndInit();
		this.groupCountryShape.ResumeLayout(false);
		this.groupAudio.ResumeLayout(false);
		this.groupAudio.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNationalTeam).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureNationalTeam).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sponsorListBindingSource).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.countryBindingSource).EndInit();
		base.ResumeLayout(false);
	}
}
