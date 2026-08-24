using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class PlayerForm : Form
{
	public Player m_CurrentPlayer;

	private TabPage m_CurrentPage;

	private string m_PlayerCurrentFolder = FifaEnvironment.ExportFolder;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private HairSelector m_HairSelector = new HairSelector(HairSelector.ESelectionType.Hair);

	private HairSelector m_FaceSelector = new HairSelector(HairSelector.ESelectionType.Face);

	private bool m_AttributesSema = true;

	private bool m_OverallSema = true;

	private bool m_Fc26PlaystylesLoading;

	private readonly List<CheckBox> m_Fc26PlaystyleChecks = new List<CheckBox>();

	private readonly List<CheckBox> m_Fc26PlaystylePlusChecks = new List<CheckBox>();

	private static readonly string[] c_Fc26PlaystyleNames = new string[34]
	{
		"Finesse Shot", "Power Shot", "Dead Ball", "Chip Shot", "Power Header", "Pinged Pass",
		"Long Ball Pass", "Tiki Taka", "Incisive Pass", "Whipped Pass", "First Touch", "Technical",
		"Rapid", "Quick Step", "Trickster", "Press Proven", "Flair", "Relentless", "Trivela",
		"Block", "Intercept", "Anticipate", "Slide Tackle", "Bruiser", "Jockey", "Aerial",
		"Acrobatic", "Far Reach", "Footwork", "Cross Claimer", "Rush Out", "Deflector",
		"1v1 Close Down", "Long Throw"
	};

	private bool m_GenericAppearanceSema = true;

	private bool m_Locked;

	private int m_HairAlfaChannel = 1;

	private static Color[] c_AccPalette = new Color[14]
	{
		Color.White,
		Color.Black,
		Color.Blue,
		Color.Red,
		Color.Yellow,
		Color.Green,
		Color.Orange,
		Color.Purple,
		Color.Brown,
		Color.Pink,
		Color.Maroon,
		Color.LightBlue,
		Color.Navy,
		Color.Gray
	};

	private static Color[] c_GlovesPalette = new Color[5]
	{
		Color.White,
		Color.Black,
		Color.Yellow,
		Color.Red,
		Color.Navy
	};

	private Viewer3D viewer3D;

	private Panel m_Fc26Face3dPanel;

	private Label m_Fc26Face3dStatus;

	private Button m_Fc26Face3dButton;

	private CreationMaster.Controls.Mesh3DPreviewHost m_Fc26Mesh3DHost;

	private readonly Dictionary<string, string> m_Fc26FaceMeshCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private int m_Fc26FaceMeshRequest;

	private string m_Fc26FaceMeshKey;

	private string m_Fc26RenderedFaceMeshKey;

	private IContainer components;

	public PickUpControl pickUpControl;

	private TabControl tabEditPlayer;

	private TabPage pageInfo;

	private TabPage pageSkills;

	private TabPage pageFace;

	private ImageList imageListTabIcons;

	private FlowLayoutPanel flowPanelInfo;

	private GroupBox groupPlayerIdentity;

	private Button buttonGetId;

	private NumericUpDown numericPlayerId;

	private Viewer2D viewer2DPhoto;

	private Button buttonRandomizeIdentity;

	private DateTimePicker dateBirthDate;

	private Label labelBirthdate;

	private Label labelPlayerId;

	private TextBox textSurname;

	private TextBox textFirstName;

	private ComboBox comboCountry;

	private Label labelFirstName;

	private Label labelSurame;

	private Label labelCountry;

	private Label labelCommonName;

	private TextBox textCommonName;

	private TextBox textJerseyName;

	private Label labelJerseyName;

	private BindingSource countryListBindingSource;

	private BindingSource playerBindingSource;

	private GroupBox groupBoxBody;

	private NumericUpDown numericHeight;

	private NumericUpDown numericWeight;

	private Label labelWeight;

	private Label labelBody;

	private DomainUpDown domainPreferredFoot;

	private Label labelHeight;

	private Label labelPreferredFoot;

	private ComboBox comboBody;

	private GroupBox groupBoxLook;

	public NumericUpDown numericShoesDesign;

	private Viewer2D viewer2DShoes;

	private DomainUpDown domainJerseyStyle;

	public NumericUpDown numericShoesBrand;

	private DomainUpDown domainSleeves;

	private PictureBox pictureColorAcc2;

	private PictureBox pictureColorAcc3;

	private PictureBox pictureColorAcc4;

	private PictureBox pictureColorAcc1;

	private ComboBox domainAccessory4;

	private ComboBox domainAccessory3;

	private ComboBox domainAccessory2;

	private ComboBox domainAccessory1;

	private Label labelSleeves;

	private Label labelJerseyStyle;

	private Label labelAccesories;

	private Label labelShoes;

	private Label labelShoesColor;

	private Label labelShoesType;

	private GroupBox groupPlayFirTeam;

	private ListView listViewPlayingTeams;

	private ComboBox comboClubTeam;

	private Button buttonCallNationalTeam;

	private Button buttonRemoveNationalTeam;

	private ImageList imageListTeamLogos;

	private Label labelWinter;

	private ComboBox comboWinterAccessories;

	private ToolTip toolTip;

	private FlowLayoutPanel flowPanelSkills;

	private GroupBox groupGenerateAttributes;

	private Label labelRandomize;

	private NumericUpDown numericRandomize;

	private Button buttonRandomAboveAvg;

	private Button buttonRandomBelowAvg;

	private Button buttonRandomSuperstar;

	private Button buttonRandomVeryGood;

	private Button buttonRandomGood;

	private Button buttonRandomAverage;

	private Button buttonRandomPoor;

	private GroupBox groupGoalkeperSkills;

	private Label labelDiving;

	private Label labelPositioning;

	private Label labelReflexes;

	private Label labelHandling;

	private TrackBar trackDiving;

	private TrackBar trackPositioning;

	private TrackBar trackReflexes;

	private TrackBar trackHandling;

	private NumericUpDown numericGoalkeeperSkills;

	private GroupBox groupDefensiveSkills;

	private NumericUpDown numericDefensiveSkills;

	private Label labelAggression;

	private Label labelMarking;

	private Label labelHeading;

	private TrackBar trackHeading;

	private Label labelTackling;

	private TrackBar trackTackling;

	private TrackBar trackMarking;

	private TrackBar trackAggression;

	private GroupBox groupMidfielderSkills;

	private NumericUpDown numericMidfielderSkills;

	private Label labelBallControl;

	private Label labelCrossing;

	private Label labelLongPassing;

	private TrackBar trackLongPassing;

	private Label labelShortPassing;

	private TrackBar trackShortPassing;

	private TrackBar trackBallControl;

	private TrackBar trackCrossing;

	private GroupBox groupAttackingSkills;

	private NumericUpDown numericAttackingSkills;

	private Label labelDribbling;

	private Label labelLongShot;

	private Label labelFreeKick;

	private Label labelShotPower;

	private Label labelFinishing;

	private TrackBar trackFinishing;

	private TrackBar trackShotPower;

	private TrackBar trackLongShot;

	private TrackBar trackFreeKick;

	private TrackBar trackDribbling;

	private GroupBox groupGenericAttributes;

	private Label labelPlayerPositioning;

	private TrackBar trackPlayerPositioning;

	private Label labelPotential;

	private TrackBar trackPotential;

	private NumericUpDown numericPhysicalSkills;

	private Label labelReactions;

	private Label labelStrength;

	private Label labelStamina;

	private TrackBar trackStamina;

	private Label labelSprintSpeed;

	private TrackBar trackSprintSpeed;

	private Label labelAcceleration;

	private TrackBar trackAcceleration;

	private TrackBar trackStrength;

	private TrackBar trackReactions;

	private Label labelGkKick;

	private TrackBar trackGkKicking;

	private Label labelAgility;

	private TrackBar trackAgility;

	private Label labelBalance;

	private TrackBar trackBalance;

	private Label labelJumping;

	private TrackBar trackJumping;

	private Label labelPenalties;

	private TrackBar trackPenalties;

	private Label labelSliding;

	private TrackBar trackSliding;

	private Label labelVision;

	private TrackBar trackVision;

	private Label labelVolley;

	private TrackBar trackVolley;

	private Label labelOverallrating;

	private TrackBar trackOverallrating;

	private GroupBox groupMental;

	private Label labelFreeKickStart;

	private ComboBox comboFreeKickStart;

	private Label labelPenaltyKick;

	private ComboBox comboPenaltyKick;

	private Label labelPenaltyMove;

	private ComboBox comboPenaltyMove;

	private Label labelPenaltyStart;

	private ComboBox comboPenaltyStart;

	private GroupBox groupFreeKick;

	private SplitContainer splitContainer1;

	private SplitContainer splitContainer2;

	private SplitContainer splitContainer3;

	private GroupBox groupGenericFace;

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

	private ComboBox comboEyescolor;

	private Label label2;

	private ComboBox comboFacialHairColor;

	private Label labelFacialHairColor;

	private Label label1;

	private ComboBox comboSkintype;

	private Label labelSkintype;

	private ComboBox comboSideburns;

	private Label labelSideburns;

	private ComboBox domainFacialHair;

	private Label labelHeadType;

	private Label labelHairType;

	private Label labelFacialHair;

	private GroupBox groupBox1;

	private Label labelPreferredPositions;

	private ComboBox comboPreferredPosition4;

	private ComboBox comboPreferredPosition3;

	private ComboBox comboPreferredPosition2;

	private ComboBox comboPreferredPosition1;

	private DomainUpDown domainInternationalReputation;

	private Label labelInternationalReputation;

	private ToolStrip tool3D;

	private ToolStripButton buttonShow3DModel;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonImport3DHairModel;

	private ToolStripButton buttonExport3DHairModel;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton buttonRemove3DHeadModel;

	private Label labelEyeBow;

	private ComboBox comboEyeBow;

	private CheckBox checkShowTexures;

	private Viewer2D viewer2DEyeTexture;

	private Viewer2D viewer2DPlayerGui;

	private ComboBox comboWeakFoot;

	private Label labelWeakFoot;

	private CheckBox checkHasGenericFace;

	private GroupBox groupGenericFaceType;

	private ToolStripButton buttonImport3DHeadModel;

	private ToolStripButton buttonExport3DHeadModel;

	private NumericUpDown numericMentalSkills;

	private NumericUpDown numericFreeKickSkills;

	private Label labelCurve;

	private TrackBar trackCurve;

	private Label label3;

	private ComboBox comboGkKickStyle;

	private ImageList imageListStars;

	private Label labelSkillMoves;

	private Label labelSkillsStars;

	private NumericUpDown numericSkillMoves;

	private ToolStripButton toolPhoto;

	private ToolStripSeparator toolStripSeparator3;

	public NumericUpDown numericGkGloves;

	private Label labelGkGloves;

	private ToolStripButton buttonSwitchRenderingMode;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripButton buttonMoveHairAhead;

	private ToolStripButton buttonMoveHairUp;

	private ToolStripButton buttonMoveHairBack;

	private ToolStripButton buttonMoveHairDown;

	private ToolStripButton buttonSaveHair;

	private ToolStripButton buttonRemoveHairModel;

	private ToolStripButton buttonShowJesey;

	private MultiViewer2D multiViewerHair;

	private GroupBox groupTraits;

	private CheckBox checkLongThrower;

	private CheckBox checkGiantThrower;

	private CheckBox checkAvoidsWeakFoot;

	private CheckBox checkInjuryFree;

	private CheckBox checkPowerFreeKick;

	private CheckBox checkSelfish;

	private CheckBox checkPlaymaker;

	private CheckBox checkSpeedDribbler;

	private CheckBox checkLeadership;

	private CheckBox checkPuncher;

	private CheckBox checkDiver;

	private CheckBox checkDivesintotackles;

	private CheckBox checkLongshottaker;

	private CheckBox checkHighClubIdentification;

	private CheckBox checkPushesupforcorners;

	private CheckBox checkEarlycrosser;

	private CheckBox checkInjuryProne;

	private CheckBox checkBeatsOffsideTrap;

	private CheckBox checkLongPasser;

	private CheckBox checkFlair;

	private CheckBox checkFinesseShot;

	private CheckBox checkArguesWithOfficials;

	private CheckBox checkSwervePasser;

	private CheckBox checkCornerSpecialist;

	private CheckBox checkPowerHeader;

	private CheckBox checkGkLongThrower;

	private CheckBox checkTeamPlayer;

	private DateTimePicker dateJoiningDate;

	private Label label4;

	private Label label5;

	private ComboBox comboGkSaveStyle;

	private NumericUpDown numericUpDown1;

	private Label label6;

	private NumericUpDown numericUpDown2;

	private Label label7;

	private NumericUpDown numericUpDown3;

	private NumericUpDown numericUpDown4;

	private DomainUpDown domainSocksStyle;

	private Label label8;

	private ComboBox comboAttackWorkRate;

	private Label label9;

	private ComboBox comboDefensiveWorkrate;

	private Label label10;

	private CheckBox checkTrainingPants;

	private GroupBox groupShoes;

	private NumericUpDown numericSkinTone;

	private Viewer2D viewer2DSkinTexture;

	private Label labelSkinColorInfo;

	private Button buttonRgbHair;

	private GroupBox groupIsLoan;

	private CheckBox checkIsLoan;

	private DateTimePicker dateLoanEnd;

	private Label label11;

	private Label label12;

	private ComboBox comboTeamLoanedFrom;

	private BindingSource teamListBindingSource;

	private Label label1ShoesType;

	private PictureBox pictureColorShoes2;

	private PictureBox pictureColorShoes1;

	private CheckBox checkJerseyFit;

	private Label labelInterception;

	private TrackBar trackInterception;

	private ToolStripButton buttonMoveHairLeft;

	private ToolStripButton buttonMoveHairRight;

	private ToolStripButton buttonMakeHairCloser;

	private ToolStripButton buttonMakeHairWider;

	private GroupBox groupBox2;

	private CheckBox checkGKFlatKick;

	private CheckBox checkDrivenPass;

	private CheckBox checkDivingHeader;

	private CheckBox checkBycicleKick;

	private CheckBox checkChipperPenalty;

	private CheckBox checkStutterPenalty;

	private CheckBox checkFancyFlicks;

	private CheckBox checkFancyPasses;

	private CheckBox checkFancyFeet;

	private CheckBox checkGKOneonOne;

	private CheckBox checkAcrobaticClearance;

	private CheckBox checkSecondWind;

	private CheckBox checkCrowdFavourite;

	private CheckBox checkInflexible;

	private MultiViewer2D multiViewerFace;

	private Button buttonHairSelection;

	private CheckBox checkUsingRevMod;

	private RadioButton radioButtonGenderFemale;

	private RadioButton radioButtonGenderMale;

	private CheckBox checkHighQaualityFace;

	private ComboBox comboFemaleModels;

	private RadioButton radioButtonFemale;

	private RadioButton radioButtonFemaleHair;

	private ComboBox comboFemaleHair;

	private Label label13;

	private ComboBox comboFaceposer;

	private GroupBox groupCommonHeadControls;

	private GroupBox groupSpecifiHeadControls;

	private Viewer2D viewer2DTattoos;

	private Label label14;

	private NumericUpDown numericUpDown5;

	private Label label15;

	private CheckBox checkChipShot;

	private CheckBox checkTechDribbler;

	private CmStyleDetailsPanel m_CareerDetails;

	public PlayerForm()
	{
		base.Visible = false;
		InitializeComponent();
		var careerPage = new TabPage("Career Details") { BackColor = SystemColors.Control };
		m_CareerDetails = new CmStyleDetailsPanel(DetailSection.Player);
		careerPage.Controls.Add(m_CareerDetails);
		tabEditPlayer.TabPages.Add(careerPage);
		InitializeFc26PlaystyleControls();
		viewer3D = new Viewer3D();
		splitContainer2.Panel1.Controls.Add(viewer3D);
		viewer3D.AmbientColor = Color.Gray;
		viewer3D.BackColor = Color.Gray;
		viewer3D.BorderStyle = BorderStyle.Fixed3D;
		viewer3D.LightDirectionX = -0.6f;
		viewer3D.LightDirectionY = -0f;
		viewer3D.LightDirectionZ = -1f;
		viewer3D.LightX = 30f;
		viewer3D.LightY = 180f;
		viewer3D.LightZ = 100f;
		viewer3D.Location = new Point(0, 0);
		viewer3D.Name = "viewer3D";
		viewer3D.RotationX = 6.28f;
		viewer3D.RotationY = 0f;
		viewer3D.RotationYCoeff = 0.001f;
		viewer3D.Size = new Size(748, 441);
		viewer3D.ViewX = 0f;
		viewer3D.ViewY = 171f;
		viewer3D.ViewZ = 49f;
		viewer3D.ZbufferRenderState = null;
		InitializeFc26Face3dControls();
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
		comboFemaleModels.Items.Clear();
		for (int m = 0; m < GenericHead.c_FemaleModels.Length; m++)
		{
			comboFemaleModels.Items.Add(GenericHead.c_FemaleModels[m].ToString());
		}
		comboShaven.Items.Clear();
		for (int n = 0; n < GenericHead.c_ShavenModels.Length; n++)
		{
			comboShaven.Items.Add(GenericHead.c_ShavenModels[n].ToString());
		}
		comboVeryShort.Items.Clear();
		for (int num = 0; num < GenericHead.c_VeryShortModels.Length; num++)
		{
			comboVeryShort.Items.Add(GenericHead.c_VeryShortModels[num].ToString());
		}
		comboShort.Items.Clear();
		for (int num2 = 0; num2 < GenericHead.c_ShortModels.Length; num2++)
		{
			comboShort.Items.Add(GenericHead.c_ShortModels[num2].ToString());
		}
		comboModern.Items.Clear();
		for (int num3 = 0; num3 < GenericHead.c_ModernModels.Length; num3++)
		{
			comboModern.Items.Add(GenericHead.c_ModernModels[num3].ToString());
		}
		comboMedium.Items.Clear();
		for (int num4 = 0; num4 < GenericHead.c_MediumModels.Length; num4++)
		{
			comboMedium.Items.Add(GenericHead.c_MediumModels[num4].ToString());
		}
		comboLong.Items.Clear();
		for (int num5 = 0; num5 < GenericHead.c_LongModels.Length; num5++)
		{
			comboLong.Items.Add(GenericHead.c_LongModels[num5].ToString());
		}
		comboAfro.Items.Clear();
		for (int num6 = 0; num6 < GenericHead.c_AfroModels.Length; num6++)
		{
			comboAfro.Items.Add(GenericHead.c_AfroModels[num6].ToString());
		}
		comboHeadband.Items.Clear();
		for (int num7 = 0; num7 < GenericHead.c_HeadbendModels.Length; num7++)
		{
			comboHeadband.Items.Add(GenericHead.c_HeadbendModels[num7].ToString());
		}
		comboFemaleHair.Items.Clear();
		for (int num8 = 0; num8 < GenericHead.c_FemaleHairModels.Length; num8++)
		{
			comboFemaleHair.Items.Add(GenericHead.c_FemaleHairModels[num8].ToString());
		}
		pickUpControl.SelectObject = SelectPlayer;
		pickUpControl.CreateObject = CreatePlayer;
		pickUpControl.DeleteObject = DeletePlayer;
		pickUpControl.CloneObject = ClonePlayer;
		pickUpControl.RefreshObject = RefreshPlayer;
		pickUpControl.combo.Sorted = false;
		viewer2DPhoto.ButtonStripVisible = true;
		viewer2DPlayerGui.ButtonStripVisible = true;
		viewer2DShoes.ButtonStripVisible = false;
		viewer2DEyeTexture.ButtonStripVisible = true;
		viewer2DEyeTexture.ShowButton = true;
		viewer2DEyeTexture.ShowButtonChecked = true;
		viewer2DPlayerGui.ButtonStripVisible = true;
		viewer2DShoes.ButtonStripVisible = false;
		viewer2DEyeTexture.ImageImport = ImportImageEye;
		viewer2DEyeTexture.ImageDelete = DeleteImageEye;
		viewer2DEyeTexture.ButtonStripVisible = true;
		viewer2DEyeTexture.RemoveButton = true;
		viewer2DSkinTexture.ImageImport = ImportImageSkin;
		viewer2DSkinTexture.ImageDelete = DeleteImageSkin;
		viewer2DSkinTexture.ButtonStripVisible = true;
		viewer2DSkinTexture.RemoveButton = true;
		viewer2DSkinTexture.ShowButton = true;
		viewer2DSkinTexture.ShowButtonChecked = true;
		viewer2DSkinTexture.FullSizeButton = true;
		multiViewerHair.Rx3ExportDelegate = ExportRx3HairTextures;
		multiViewerHair.Rx3ImportDelegate = ImportRx3HairTextures;
		multiViewerHair.Rx3SaveDelegate = SaveRx3HairTextures;
		multiViewerHair.Rx3DeleteDelegate = DeleteRx3HairTextures;
		multiViewerHair.ShowDeleteButton = true;
		multiViewerHair.FullSizeButton = true;
		multiViewerFace.Rx3ExportDelegate = ExportRx3FaceTextures;
		multiViewerFace.Rx3ImportDelegate = ImportRx3FaceTextures;
		multiViewerFace.Rx3SaveDelegate = SaveRx3FaceTextures;
		multiViewerFace.Rx3DeleteDelegate = DeleteRx3FaceTextures;
		multiViewerFace.ShowDeleteButton = true;
		multiViewerFace.FullSizeButton = true;
		viewer2DPlayerGui.ImageImport = ImportImageMiniface;
		viewer2DPlayerGui.ImageDelete = DeleteMiniface;
		viewer2DPlayerGui.ButtonStripVisible = true;
		viewer2DPlayerGui.RemoveButton = true;
		viewer2DPhoto.ImageImport = ImportImageMiniface;
		viewer2DPhoto.ImageDelete = DeleteMiniface;
		viewer2DPhoto.ButtonStripVisible = true;
		viewer2DPhoto.RemoveButton = true;
		viewer2DPhoto.ShowButton = true;
		viewer2DPhoto.ShowButtonChecked = true;
		viewer2DPlayerGui.ImageImport = ImportImageTattoo;
		viewer2DPlayerGui.ImageDelete = DeleteTattoo;
		viewer2DTattoos.ButtonStripVisible = true;
		viewer2DTattoos.RemoveButton = true;
		viewer2DTattoos.FullSizeButton = true;
		viewer2DTattoos.ShowButton = true;
		viewer2DTattoos.ShowButtonChecked = true;
		tool3D.Visible = true;
	}

	private Player CreatePlayer(object sender, object obj)
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
		return (Player)m_NewIdCreator.NewObject;
	}

	private Player DeletePlayer(object sender, object obj)
	{
		Player player = (Player)obj;
		while (player.m_PlayingForTeams.Count > 0)
		{
			((Team)player.m_PlayingForTeams[0]).RemoveTeamPlayer(player);
		}
		FifaEnvironment.Players.DeletePlayer(player);
		return null;
	}

	private Player ClonePlayer(object sender, object obj)
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
		Player srcIdObject = (Player)obj;
		Player obj2 = (Player)FifaEnvironment.Players.CloneId(srcIdObject, m_NewIdCreator.NewObject);
		obj2.RandomizeAppearanceSameRace();
		return obj2;
	}

	public Player RefreshPlayer(object sender, object obj)
	{
		m_CurrentPlayer.CleanAllHead();
		Preset();
		ReloadPlayer(m_CurrentPlayer);
		return m_CurrentPlayer;
	}

	private bool ImportImageEye(object sender, Bitmap bitmap)
	{
		bool num = m_CurrentPlayer.SetEyesTextures(bitmap);
		if (num)
		{
			UpdateAndShowHead3D();
		}
		return num;
	}

	private bool DeleteImageEye(object sender)
	{
		return m_CurrentPlayer.DeleteEyesTexture();
	}

	private bool ImportImageSkin(object sender, Bitmap bitmap)
	{
		bool num = m_CurrentPlayer.SetSkinTextures(bitmap);
		if (num)
		{
			UpdateAndShowHead3D();
		}
		return num;
	}

	private bool DeleteImageSkin(object sender)
	{
		return m_CurrentPlayer.DeleteSkinTexture();
	}

	private bool ExportRx3HairTextures(object sender, string exportDir)
	{
		return FifaEnvironment.ExportFileFromZdata(m_CurrentPlayer.HairTexturesFileName(), exportDir);
	}

	private bool SaveRx3HairTextures(object sender, Bitmap[] bitmaps)
	{
		bool num = m_CurrentPlayer.SetHairTextures(bitmaps);
		if (num)
		{
			m_CurrentPlayer.CleanHairTextures();
			multiViewerHair.Bitmaps = m_CurrentPlayer.GetHairTextures();
			multiViewerHair.Enabled = true;
			UpdateAndShowHead3D();
		}
		return num;
	}

	private bool ImportRx3HairTextures(object sender, string rx3FileName)
	{
		bool num = m_CurrentPlayer.SetHairTextures(rx3FileName);
		if (num)
		{
			m_CurrentPlayer.CleanHairTextures();
			multiViewerHair.Bitmaps = m_CurrentPlayer.GetHairTextures();
			multiViewerHair.Enabled = true;
		}
		return num;
	}

	private bool DeleteRx3HairTextures(object sender)
	{
		return m_CurrentPlayer.DeleteHairTextures();
	}

	private bool ExportRx3FaceTextures(object sender, string exportDir)
	{
		return FifaEnvironment.ExportFileFromZdata(m_CurrentPlayer.FaceTextureFileName(), exportDir);
	}

	private bool SaveRx3FaceTextures(object sender, Bitmap[] bitmaps)
	{
		bool num = m_CurrentPlayer.SetFaceTextures(bitmaps);
		if (num)
		{
			m_CurrentPlayer.CleanFaceTextures();
			GetAndShowFaceTexture();
			UpdateAndShowHead3D();
		}
		return num;
	}

	private bool ImportRx3FaceTextures(object sender, string rx3FileName)
	{
		bool num = m_CurrentPlayer.SetFaceTextures(rx3FileName);
		if (num)
		{
			m_CurrentPlayer.CleanFaceTextures();
			GetAndShowFaceTexture();
			UpdateAndShowHead3D();
		}
		return num;
	}

	private bool DeleteRx3FaceTextures(object sender)
	{
		bool num = m_CurrentPlayer.DeleteFaceTexture();
		if (num)
		{
			GetAndShowFaceTexture();
			UpdateAndShowHead3D();
		}
		return num;
	}

	private bool ImportImageMiniface(object sender, Bitmap bitmap)
	{
		return m_CurrentPlayer.SetPhoto(bitmap);
	}

	private bool DeleteMiniface(object sender)
	{
		return m_CurrentPlayer.DeletePhoto();
	}

	private bool ImportImageTattoo(object sender, Bitmap bitmap)
	{
		return m_CurrentPlayer.SetTattoos(bitmap);
	}

	private bool DeleteTattoo(object sender)
	{
		return m_CurrentPlayer.DeleteTattoos();
	}

	public void Clean()
	{
		base.Visible = false;
	}

	private Player SelectPlayer(object sender, object obj)
	{
		Player player = (Player)obj;
		LoadPlayer(player);
		return player;
	}

	public void ReloadPlayer(Player player)
	{
		m_CurrentPlayer = null;
		LoadPlayer(player);
	}

	public void LoadPlayer(Player player)
	{
		if (m_IsLoaded && (m_CurrentPlayer != player || m_CurrentPage != tabEditPlayer.SelectedTab))
		{
			m_Locked = true;
			m_CurrentPlayer = player;
			buttonSaveHair.Enabled = false;
			playerBindingSource.DataSource = m_CurrentPlayer;
			m_CareerDetails.Reload(player.Id);
			m_CurrentPage = tabEditPlayer.SelectedTab;
			if (m_CurrentPage == pageInfo)
			{
				LoadPlayerInfo();
			}
			else if (m_CurrentPage == pageSkills)
			{
				LoadPlayerSkills();
			}
			else if (m_CurrentPage == pageFace)
			{
				LoadPlayerFace();
			}
			m_Locked = false;
		}
	}

	private void LoadPlayerInfo()
	{
		SetNumericValue(numericPlayerId, m_CurrentPlayer.Id);
		if (viewer2DPhoto.ShowButton)
		{
			viewer2DPhoto.CurrentBitmap = m_CurrentPlayer.GetPhoto();
		}
		else
		{
			viewer2DPhoto.CurrentBitmap = null;
		}
		InitListViewPlayingTeams(m_CurrentPlayer.m_PlayingForTeams);
		pictureColorAcc1.BackColor = SafePaletteColor(m_CurrentPlayer.accessorycolourcode1);
		pictureColorAcc2.BackColor = SafePaletteColor(m_CurrentPlayer.accessorycolourcode2);
		pictureColorAcc3.BackColor = SafePaletteColor(m_CurrentPlayer.accessorycolourcode3);
		pictureColorAcc4.BackColor = SafePaletteColor(m_CurrentPlayer.accessorycolourcode4);
		SetSelectedIndex(comboPreferredPosition1, m_CurrentPlayer.preferredposition1 + 1);
		SetSelectedIndex(comboPreferredPosition2, m_CurrentPlayer.preferredposition2 + 1);
		SetSelectedIndex(comboPreferredPosition3, m_CurrentPlayer.preferredposition3 + 1);
		SetSelectedIndex(comboPreferredPosition4, m_CurrentPlayer.preferredposition4 + 1);
		SetNumericValue(numericShoesBrand, m_CurrentPlayer.shoetypecode);
		SetNumericValue(numericShoesDesign, m_CurrentPlayer.shoedesigncode);
		pictureColorShoes1.BackColor = Shoes.GetGenericColor(m_CurrentPlayer.shoecolorcode1);
		pictureColorShoes2.BackColor = Shoes.GetGenericColor(m_CurrentPlayer.shoecolorcode2);
		if (m_CurrentPlayer.shoetypecode == 0)
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
		viewer2DShoes.CurrentBitmap = Shoes.GetShoesColorTexture(m_CurrentPlayer.shoetypecode, m_CurrentPlayer.shoedesigncode);
	}

	public void AuditFc26RecordsForSmoke()
	{
		if (FifaEnvironment.Players.Count == 0) return;
		var samples = new[] { 0, FifaEnvironment.Players.Count / 2, FifaEnvironment.Players.Count - 1 };
		var originalPage = tabEditPlayer.SelectedTab;
		foreach (var index in samples)
		{
			var player = (Player)FifaEnvironment.Players[index];
			tabEditPlayer.SelectedTab = pageInfo;
			ReloadPlayer(player);
			tabEditPlayer.SelectedTab = pageSkills;
			LoadPlayer(player);
			tabEditPlayer.SelectedTab = pageFace;
			LoadPlayer(player);
		}
		tabEditPlayer.SelectedTab = originalPage ?? pageInfo;
		LoadPlayer(m_CurrentPlayer);
	}

	private void InitializeFc26Face3dControls()
	{
		m_Fc26Face3dPanel = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(30, 30, 30),
			Visible = false
		};
		m_Fc26Face3dStatus = new Label
		{
			Dock = DockStyle.Top,
			Height = 30,
			TextAlign = ContentAlignment.MiddleCenter,
			ForeColor = Color.FromArgb(170, 170, 170),
			BackColor = Color.FromArgb(30, 30, 30),
			Text = "FC26 Frostbite 3D face — click below to load the real FBX head mesh."
		};
		m_Fc26Mesh3DHost = new CreationMaster.Controls.Mesh3DPreviewHost
		{
			Dock = DockStyle.Fill,
		};
		m_Fc26Face3dButton = new Button
		{
			Dock = DockStyle.Bottom,
			Height = 38,
			Text = "Load FC26 3D face mesh",
			BackColor = Color.FromArgb(60, 60, 60),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat,
		};
		m_Fc26Face3dButton.Click += Fc26Face3dButton_Click;
		m_Fc26Face3dPanel.Controls.Add(m_Fc26Mesh3DHost);
		m_Fc26Face3dPanel.Controls.Add(m_Fc26Face3dStatus);
		m_Fc26Face3dPanel.Controls.Add(m_Fc26Face3dButton);
		// Correct z-order: button at bottom, status at top, 3D host fills the rest.
		m_Fc26Mesh3DHost.BringToFront();
		splitContainer2.Panel1.Controls.Add(m_Fc26Face3dPanel);
		m_Fc26Face3dPanel.BringToFront();
	}

	private async void Fc26Face3dButton_Click(object sender, EventArgs e)
	{
		if (m_CurrentPlayer == null) return;
		var selectedPlayer = m_CurrentPlayer;
		var playerId = selectedPlayer.m_assetid > 0 ? selectedPlayer.m_assetid : selectedPlayer.Id;
		var headAssetId = selectedPlayer.headtypecode;
		var meshKey = GetFc26FaceMeshKey(selectedPlayer);
		m_Fc26FaceMeshKey = meshKey;
		var request = ++m_Fc26FaceMeshRequest;
		m_Fc26Face3dButton.Enabled = false;
		m_Fc26Face3dButton.Text = "Loading FC26 face mesh…";
		m_Fc26Face3dStatus.Text = $"Exporting real FC26 face mesh for Player ID {playerId}…";
		m_Fc26Mesh3DHost.ShowStatus("Exporting the selected FC26 Frostbite head mesh…");
		try
		{
			var mesh = await System.Threading.Tasks.Task.Run(() => Fc26HostBridge.ExportFaceMesh(playerId, headAssetId));
			if (IsDisposed || request != m_Fc26FaceMeshRequest ||
				m_CurrentPlayer == null || GetFc26FaceMeshKey(m_CurrentPlayer) != meshKey)
				return;

			m_Fc26FaceMeshCache[meshKey] = mesh;
			// Load the mesh into the in-app 3D panel.
			m_Fc26Mesh3DHost.LoadMesh(mesh);
			m_Fc26RenderedFaceMeshKey = meshKey;
			m_Fc26Face3dStatus.Text = $"FC26 face mesh loaded — Player ID {playerId}. Drag to rotate; mouse wheel to zoom.";
			m_Fc26Face3dButton.Text = "Reload FC26 3D face mesh";
		}
		catch (Exception ex)
		{
			if (!IsDisposed && request == m_Fc26FaceMeshRequest)
			{
				m_Fc26Mesh3DHost.ShowStatus("No viewable FC26 head mesh was exported for this player.");
				m_Fc26Face3dStatus.Text = "FC26 face mesh unavailable: " + ex.Message;
				m_Fc26Face3dButton.Text = "Retry FC26 3D face mesh";
			}
		}
		finally
		{
			if (!IsDisposed && request == m_Fc26FaceMeshRequest)
				m_Fc26Face3dButton.Enabled = true;
		}
	}

	private static string GetFc26FaceMeshKey(Player player)
	{
		if (player == null) return string.Empty;
		var playerId = player.m_assetid > 0 ? player.m_assetid : player.Id;
		return playerId + ":" + player.headtypecode;
	}

	private static Color SafePaletteColor(int index)
	{
		return index >= 0 && index < c_AccPalette.Length ? c_AccPalette[index] : Color.Transparent;
	}

	private void LoadPlayerSkills()
	{
		m_OverallSema = false;
		numericRandomize.Value = m_CurrentPlayer.GetAverageRoleAttribute();
		m_OverallSema = true;
		if (m_CurrentPlayer.skillmoves < 1)
		{
			m_CurrentPlayer.skillmoves = 1;
		}
		if (m_CurrentPlayer.skillmoves > 1)
		{
			m_CurrentPlayer.skillmoves = 5;
		}
		labelSkillsStars.ImageIndex = m_CurrentPlayer.skillmoves - 1;
		numericSkillMoves.Value = m_CurrentPlayer.skillmoves;
		playerBindingSource.ResetBindings(metadataChanged: false);
		RefreshFc26Playstyles();
	}

	private void InitializeFc26PlaystyleControls()
	{
		if (FifaEnvironment.Year != 26) return;

		groupTraits.SuspendLayout();
		groupTraits.Controls.Clear();
		groupTraits.Text = "FC 26 PlayStyles / PlayStyles+";
		groupTraits.Location = new Point(8, 387);
		groupTraits.Size = new Size(1235, 254);

		var grid = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 4,
			RowCount = 9,
			Padding = new Padding(4),
			Margin = Padding.Empty
		};
		for (int column = 0; column < 4; column++)
			grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
		for (int row = 0; row < 9; row++)
			grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 25f));

		for (int index = 0; index < c_Fc26PlaystyleNames.Length; index++)
		{
			var panel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				Margin = Padding.Empty
			};
			var playstyle = new CheckBox
			{
				AutoSize = false,
				Width = 220,
				Height = 22,
				Text = c_Fc26PlaystyleNames[index],
				Tag = index,
				Margin = new Padding(1)
			};
			var plus = new CheckBox
			{
				AutoSize = true,
				Text = "+",
				Tag = index,
				Margin = new Padding(1, 3, 1, 1)
			};
			playstyle.CheckedChanged += Fc26Playstyle_CheckedChanged;
			plus.CheckedChanged += Fc26PlaystylePlus_CheckedChanged;
			m_Fc26PlaystyleChecks.Add(playstyle);
			m_Fc26PlaystylePlusChecks.Add(plus);
			panel.Controls.Add(playstyle);
			panel.Controls.Add(plus);
			grid.Controls.Add(panel, index % 4, index / 4);
		}

		groupTraits.Controls.Add(grid);
		groupTraits.ResumeLayout(performLayout: true);
	}

	private void RefreshFc26Playstyles()
	{
		if (FifaEnvironment.Year != 26 || m_CurrentPlayer == null || m_Fc26PlaystyleChecks.Count == 0)
			return;
		m_Fc26PlaystylesLoading = true;
		try
		{
			for (int index = 0; index < c_Fc26PlaystyleNames.Length; index++)
			{
				m_Fc26PlaystyleChecks[index].Checked = GetFc26Playstyle(index, plus: false);
				m_Fc26PlaystylePlusChecks[index].Checked = GetFc26Playstyle(index, plus: true);
			}
		}
		finally
		{
			m_Fc26PlaystylesLoading = false;
		}
	}

	private bool GetFc26Playstyle(int index, bool plus)
	{
		int bit = index < 32 ? index : index - 32;
		int mask = index < 32
			? (plus ? m_CurrentPlayer.icontrait1 : m_CurrentPlayer.trait1)
			: (plus ? m_CurrentPlayer.icontrait2 : m_CurrentPlayer.trait2);
		return (mask & (1 << bit)) != 0;
	}

	private void SetFc26Playstyle(int index, bool plus, bool enabled)
	{
		int bit = index < 32 ? index : index - 32;
		int mask = index < 32
			? (plus ? m_CurrentPlayer.icontrait1 : m_CurrentPlayer.trait1)
			: (plus ? m_CurrentPlayer.icontrait2 : m_CurrentPlayer.trait2);
		mask = enabled ? mask | (1 << bit) : mask & ~(1 << bit);
		if (index < 32)
		{
			if (plus) m_CurrentPlayer.icontrait1 = mask;
			else m_CurrentPlayer.trait1 = mask;
		}
		else
		{
			if (plus) m_CurrentPlayer.icontrait2 = mask;
			else m_CurrentPlayer.trait2 = mask;
		}
	}

	private void Fc26Playstyle_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Fc26PlaystylesLoading || m_CurrentPlayer == null || sender is not CheckBox check) return;
		SetFc26Playstyle((int)check.Tag, plus: false, check.Checked);
	}

	private void Fc26PlaystylePlus_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Fc26PlaystylesLoading || m_CurrentPlayer == null || sender is not CheckBox check) return;
		int index = (int)check.Tag;
		SetFc26Playstyle(index, plus: true, check.Checked);
		if (check.Checked && !m_Fc26PlaystyleChecks[index].Checked)
			m_Fc26PlaystyleChecks[index].Checked = true;
	}

	public void Preset()
	{
		if (FifaEnvironment.Year == 26 && m_Fc26PlaystyleChecks.Count == 0)
			InitializeFc26PlaystyleControls();
		Kit.Prepare3DModels();
		m_NewIdCreator.IdList = FifaEnvironment.Players;
		IdArrayList[] filterValues = new IdArrayList[7]
		{
			null,
			FifaEnvironment.Teams,
			FifaEnvironment.Countries,
			FifaEnvironment.FreeAgents,
			new MultiClubList(),
			new SameNameList(),
			new SpecificHeadList()
		};
		pickUpControl.FilterValues = filterValues;
		numericShoesBrand.Maximum = FifaEnvironment.Year == 26 ? 9999 : FifaEnvironment.FifaDb.Table[TI.players].TableDescriptor.MaxValues[FI.players_shoetypecode];
		numericPlayerId.Maximum = FifaEnvironment.Year == 26 ? 9999999 : FifaEnvironment.FifaDb.Table[TI.teams].TableDescriptor.MaxValues[FI.teams_captainid];
		numericSkinTone.Maximum = FifaEnvironment.Year == 26 ? 255 : FifaEnvironment.FifaDb.Table[TI.players].TableDescriptor.MaxValues[FI.players_skintonecode];
		_ = FifaEnvironment.Year;
		_ = 14;
		countryListBindingSource.DataSource = FifaEnvironment.Countries;
		teamListBindingSource.DataSource = FifaEnvironment.Teams;
		pickUpControl.ObjectList = FifaEnvironment.Players;
	}

	private void InitListViewPlayingTeams(TeamList playingTeams)
	{
		listViewPlayingTeams.BeginUpdate();
		listViewPlayingTeams.Items.Clear();
		imageListTeamLogos.Images.Clear();
		for (int i = 0; i < playingTeams.Count; i++)
		{
			Team team = (Team)playingTeams[i];
			Bitmap bitmap = null;
			bitmap = team.GetCrest32();
			if (bitmap != null)
			{
				imageListTeamLogos.Images.Add(team.ToString(), bitmap);
			}
			ListViewItem listViewItem = new ListViewItem(team.ToString());
			listViewItem.Tag = team;
			listViewItem.ImageKey = team.ToString();
			listViewPlayingTeams.Items.Add(listViewItem);
		}
		if (listViewPlayingTeams.Items.Count > 0)
		{
			listViewPlayingTeams.Items[0].Selected = true;
		}
		listViewPlayingTeams.EndUpdate();
	}

	private void PlayerForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void buttonGetId_Click(object sender, EventArgs e)
	{
		int newId = FifaEnvironment.Players.GetNewId();
		if (newId == -1)
		{
			FifaEnvironment.UserMessages.ShowMessage(5050);
		}
		else
		{
			numericPlayerId.Value = newId;
		}
	}

	private void buttonRandomizeIdentity_Click(object sender, EventArgs e)
	{
		m_CurrentPlayer.RandomizeIdentity();
		LoadPlayer(m_CurrentPlayer);
	}

	private void labelCountry_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentPlayer.Country != null)
		{
			MainForm.CM.JumpTo(m_CurrentPlayer.Country);
		}
	}

	private void labelShoes_DoubleClick(object sender, EventArgs e)
	{
		Shoes shoes = (Shoes)FifaEnvironment.Shoes.SearchId(m_CurrentPlayer.shoetypecode);
		if (shoes != null)
		{
			MainForm.CM.JumpTo(shoes);
		}
	}

	private void numericPlayerId_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		int num = (int)numericPlayerId.Value;
		if (num != m_CurrentPlayer.Id)
		{
			if (FifaEnvironment.Players.SearchId(num) == null)
			{
				FifaEnvironment.Players.ChangeId(m_CurrentPlayer, num);
				m_CurrentPlayer.ChangeId();
				m_CurrentPlayer.m_assetid = num;
				m_CurrentPlayer.CleanFaceTextures();
				m_CurrentPlayer.CleanHairTextures();
				LoadPlayerFace();
				viewer2DPhoto.CurrentBitmap = m_CurrentPlayer.GetPhoto();
			}
			else
			{
				FifaEnvironment.UserMessages.ShowMessage(1015);
				numericPlayerId.Value = m_CurrentPlayer.Id;
			}
		}
	}

	private void numericShoesBrand_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericShoesBrand.Value;
			if (num == 0)
			{
				m_CurrentPlayer.shoetypecode = num;
				m_CurrentPlayer.shoecolorcode1 = 0;
				m_CurrentPlayer.shoecolorcode2 = 15;
				pictureColorShoes1.BackColor = Shoes.ShoesColorPalette[m_CurrentPlayer.shoecolorcode1];
				pictureColorShoes2.BackColor = Shoes.ShoesColorPalette[m_CurrentPlayer.shoecolorcode2];
				numericShoesDesign.Enabled = true;
				pictureColorShoes1.Enabled = true;
				pictureColorShoes2.Enabled = true;
			}
			else
			{
				m_CurrentPlayer.shoetypecode = num;
				numericShoesDesign.Enabled = false;
				pictureColorShoes1.Enabled = false;
				pictureColorShoes2.Enabled = false;
				pictureColorShoes1.BackColor = Color.Transparent;
				pictureColorShoes2.BackColor = Color.Transparent;
				m_CurrentPlayer.shoedesigncode = 0;
				m_CurrentPlayer.shoecolorcode1 = 30;
				m_CurrentPlayer.shoecolorcode2 = 31;
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
			m_CurrentPlayer.shoedesigncode = num;
			if (m_CurrentPlayer.shoetypecode == 0)
			{
				viewer2DShoes.CurrentBitmap = Shoes.GetShoesColorTexture(0, num);
			}
		}
	}

	private void pictureColorAcc1_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(c_AccPalette, m_CurrentPlayer.accessorycolourcode1);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentPlayer.accessorycolourcode1 = colorSelector.SelectedIndex;
			pictureColorAcc1.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void pictureColorAcc2_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(c_AccPalette, m_CurrentPlayer.accessorycolourcode2);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentPlayer.accessorycolourcode2 = colorSelector.SelectedIndex;
			pictureColorAcc2.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void pictureColorAcc3_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(c_AccPalette, m_CurrentPlayer.accessorycolourcode3);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentPlayer.accessorycolourcode3 = colorSelector.SelectedIndex;
			pictureColorAcc3.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void pictureColorAcc4_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(c_AccPalette, m_CurrentPlayer.accessorycolourcode4);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentPlayer.accessorycolourcode4 = colorSelector.SelectedIndex;
			pictureColorAcc4.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void trackReflexes_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.gkreflexes = trackReflexes.Value;
		labelReflexes.Text = labelReflexes.Text.Substring(0, labelReflexes.Text.IndexOf(' '));
		Label label = labelReflexes;
		label.Text = label.Text + " " + m_CurrentPlayer.gkreflexes;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericGoalkeeperSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(0);
			m_AttributesSema = true;
		}
	}

	private void trackHandling_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.gkhandling = trackHandling.Value;
		labelHandling.Text = labelHandling.Text.Substring(0, labelHandling.Text.IndexOf(' '));
		Label label = labelHandling;
		label.Text = label.Text + " " + m_CurrentPlayer.gkhandling;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericGoalkeeperSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(0);
			m_AttributesSema = true;
		}
	}

	private void trackDiving_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.gkdiving = trackDiving.Value;
		labelDiving.Text = labelDiving.Text.Substring(0, labelDiving.Text.IndexOf(' '));
		Label label = labelDiving;
		label.Text = label.Text + " " + m_CurrentPlayer.gkdiving;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericGoalkeeperSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(0);
			m_AttributesSema = true;
		}
	}

	private void trackPositioning_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.gkpositioning = trackPositioning.Value;
		labelPositioning.Text = labelPositioning.Text.Substring(0, labelPositioning.Text.IndexOf(' '));
		Label label = labelPositioning;
		label.Text = label.Text + " " + m_CurrentPlayer.gkpositioning;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericGoalkeeperSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(0);
			m_AttributesSema = true;
		}
	}

	private void trackGkKick_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.gkkicking = trackGkKicking.Value;
		labelGkKick.Text = labelGkKick.Text.Substring(0, labelGkKick.Text.IndexOf(' '));
		Label label = labelGkKick;
		label.Text = label.Text + " " + m_CurrentPlayer.gkkicking;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericGoalkeeperSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(0);
			m_AttributesSema = true;
		}
	}

	private void trackMarking_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.marking = trackMarking.Value;
		labelMarking.Text = labelMarking.Text.Substring(0, labelMarking.Text.IndexOf(' '));
		Label label = labelMarking;
		label.Text = label.Text + " " + m_CurrentPlayer.marking;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericDefensiveSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(1);
			m_AttributesSema = true;
		}
	}

	private void trackTackling_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.standingtackle = trackTackling.Value;
		labelTackling.Text = labelTackling.Text.Substring(0, labelTackling.Text.IndexOf(' '));
		Label label = labelTackling;
		label.Text = label.Text + " " + m_CurrentPlayer.standingtackle;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericDefensiveSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(1);
			m_AttributesSema = true;
		}
	}

	private void trackSliding_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.slidingtackle = trackSliding.Value;
		labelSliding.Text = labelSliding.Text.Substring(0, labelSliding.Text.IndexOf(' '));
		Label label = labelSliding;
		label.Text = label.Text + " " + m_CurrentPlayer.slidingtackle;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericDefensiveSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(1);
			m_AttributesSema = true;
		}
	}

	private void trackAggression_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.aggression = trackAggression.Value;
		labelAggression.Text = labelAggression.Text.Substring(0, labelAggression.Text.IndexOf(' '));
		Label label = labelAggression;
		label.Text = label.Text + " " + m_CurrentPlayer.aggression;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericDefensiveSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(1);
			m_AttributesSema = true;
		}
	}

	private void trackShortPassing_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.shortpassing = trackShortPassing.Value;
		labelShortPassing.Text = labelShortPassing.Text.Substring(0, labelShortPassing.Text.IndexOf(' '));
		Label label = labelShortPassing;
		label.Text = label.Text + " " + m_CurrentPlayer.shortpassing;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMidfielderSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(2);
			m_AttributesSema = true;
		}
	}

	private void trackLongPassing_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.longpassing = trackLongPassing.Value;
		labelLongPassing.Text = labelLongPassing.Text.Substring(0, labelLongPassing.Text.IndexOf(' '));
		Label label = labelLongPassing;
		label.Text = label.Text + " " + m_CurrentPlayer.longpassing;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMidfielderSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(2);
			m_AttributesSema = true;
		}
	}

	private void trackCrossing_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.crossing = trackCrossing.Value;
		labelCrossing.Text = labelCrossing.Text.Substring(0, labelCrossing.Text.IndexOf(' '));
		Label label = labelCrossing;
		label.Text = label.Text + " " + m_CurrentPlayer.crossing;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMidfielderSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(2);
			m_AttributesSema = true;
		}
	}

	private void trackBallControl_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.ballcontrol = trackBallControl.Value;
		labelBallControl.Text = labelBallControl.Text.Substring(0, labelBallControl.Text.IndexOf(' '));
		Label label = labelBallControl;
		label.Text = label.Text + " " + m_CurrentPlayer.ballcontrol;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMidfielderSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(2);
			m_AttributesSema = true;
		}
	}

	private void trackVision_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.vision = trackVision.Value;
		labelVision.Text = labelVision.Text.Substring(0, labelVision.Text.IndexOf(' '));
		Label label = labelVision;
		label.Text = label.Text + " " + m_CurrentPlayer.vision;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMidfielderSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(2);
			m_AttributesSema = true;
		}
	}

	private void trackCurve_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.curve = trackCurve.Value;
		labelCurve.Text = labelCurve.Text.Substring(0, labelCurve.Text.IndexOf(' '));
		Label label = labelCurve;
		label.Text = label.Text + " " + m_CurrentPlayer.curve;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMidfielderSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(2);
			m_AttributesSema = true;
		}
	}

	private void trackHeading_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.headingaccuracy = trackHeading.Value;
		labelHeading.Text = labelHeading.Text.Substring(0, labelHeading.Text.IndexOf(' '));
		Label label = labelHeading;
		label.Text = label.Text + " " + m_CurrentPlayer.headingaccuracy;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericAttackingSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(3);
			m_AttributesSema = true;
		}
	}

	private void trackFinishing_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.finishing = trackFinishing.Value;
		labelFinishing.Text = labelFinishing.Text.Substring(0, labelFinishing.Text.IndexOf(' '));
		Label label = labelFinishing;
		label.Text = label.Text + " " + m_CurrentPlayer.finishing;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericAttackingSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(3);
			m_AttributesSema = true;
		}
	}

	private void trackShotPower_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.shotpower = trackShotPower.Value;
		labelShotPower.Text = labelShotPower.Text.Substring(0, labelShotPower.Text.IndexOf(' '));
		Label label = labelShotPower;
		label.Text = label.Text + " " + m_CurrentPlayer.shotpower;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericAttackingSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(3);
			m_AttributesSema = true;
		}
	}

	private void trackLongShot_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.longshots = trackLongShot.Value;
		labelLongShot.Text = labelLongShot.Text.Substring(0, labelLongShot.Text.IndexOf(' '));
		Label label = labelLongShot;
		label.Text = label.Text + " " + m_CurrentPlayer.longshots;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericAttackingSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(3);
			m_AttributesSema = true;
		}
	}

	private void trackFreeKick_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.freekickaccuracy = trackFreeKick.Value;
		labelFreeKick.Text = labelFreeKick.Text.Substring(0, labelFreeKick.Text.IndexOf(' '));
		Label label = labelFreeKick;
		label.Text = label.Text + " " + m_CurrentPlayer.freekickaccuracy;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericFreeKickSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(6);
			m_AttributesSema = true;
		}
	}

	private void trackDribbling_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.dribbling = trackDribbling.Value;
		labelDribbling.Text = labelDribbling.Text.Substring(0, labelDribbling.Text.IndexOf(' '));
		Label label = labelDribbling;
		label.Text = label.Text + " " + m_CurrentPlayer.dribbling;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericAttackingSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(3);
			m_AttributesSema = true;
		}
	}

	private void trackPenalties_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.penalties = trackPenalties.Value;
		labelPenalties.Text = labelPenalties.Text.Substring(0, labelPenalties.Text.IndexOf(' '));
		Label label = labelPenalties;
		label.Text = label.Text + " " + m_CurrentPlayer.penalties;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericFreeKickSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(6);
			m_AttributesSema = true;
		}
	}

	private void trackVolley_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.volleys = trackVolley.Value;
		labelVolley.Text = labelVolley.Text.Substring(0, labelVolley.Text.IndexOf(' '));
		Label label = labelVolley;
		label.Text = label.Text + " " + m_CurrentPlayer.volleys;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericAttackingSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(3);
			m_AttributesSema = true;
		}
	}

	private void trackAcceleration_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.acceleration = trackAcceleration.Value;
		labelAcceleration.Text = labelAcceleration.Text.Substring(0, labelAcceleration.Text.IndexOf(' '));
		Label label = labelAcceleration;
		label.Text = label.Text + " " + m_CurrentPlayer.acceleration;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackSprintSpeed_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.sprintspeed = trackSprintSpeed.Value;
		labelSprintSpeed.Text = labelSprintSpeed.Text.Substring(0, labelSprintSpeed.Text.IndexOf(' '));
		Label label = labelSprintSpeed;
		label.Text = label.Text + " " + m_CurrentPlayer.sprintspeed;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackStamina_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.stamina = trackStamina.Value;
		labelStamina.Text = labelStamina.Text.Substring(0, labelStamina.Text.IndexOf(' '));
		Label label = labelStamina;
		label.Text = label.Text + " " + m_CurrentPlayer.stamina;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackStrength_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.strength = trackStrength.Value;
		labelStrength.Text = labelStrength.Text.Substring(0, labelStrength.Text.IndexOf(' '));
		Label label = labelStrength;
		label.Text = label.Text + " " + m_CurrentPlayer.strength;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackAgility_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.agility = trackAgility.Value;
		labelAgility.Text = labelAgility.Text.Substring(0, labelAgility.Text.IndexOf(' '));
		Label label = labelAgility;
		label.Text = label.Text + " " + m_CurrentPlayer.agility;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackJumping_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.jumping = trackJumping.Value;
		labelJumping.Text = labelJumping.Text.Substring(0, labelJumping.Text.IndexOf(' '));
		Label label = labelJumping;
		label.Text = label.Text + " " + m_CurrentPlayer.jumping;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackReactions_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.reactions = trackReactions.Value;
		labelReactions.Text = labelReactions.Text.Substring(0, labelReactions.Text.IndexOf(' '));
		Label label = labelReactions;
		label.Text = label.Text + " " + m_CurrentPlayer.reactions;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackPotential_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.potential = trackPotential.Value;
		labelPotential.Text = labelPotential.Text.Substring(0, labelPotential.Text.IndexOf(' '));
		Label label = labelPotential;
		label.Text = label.Text + " " + m_CurrentPlayer.potential;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMentalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(5);
			m_AttributesSema = true;
		}
	}

	private void trackPlayerPositioning_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.positioning = trackPlayerPositioning.Value;
		labelPlayerPositioning.Text = labelPlayerPositioning.Text.Substring(0, labelPlayerPositioning.Text.IndexOf(' '));
		Label label = labelPlayerPositioning;
		label.Text = label.Text + " " + m_CurrentPlayer.positioning;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericMentalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(5);
			m_AttributesSema = true;
		}
	}

	private void trackBalance_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.balance = trackBalance.Value;
		labelBalance.Text = labelBalance.Text.Substring(0, labelBalance.Text.IndexOf(' '));
		Label label = labelBalance;
		label.Text = label.Text + " " + m_CurrentPlayer.balance;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericPhysicalSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(4);
			m_AttributesSema = true;
		}
	}

	private void trackOverallrating_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.overallrating = trackOverallrating.Value;
		labelOverallrating.Text = labelOverallrating.Text.Substring(0, labelOverallrating.Text.IndexOf(' '));
		Label label = labelOverallrating;
		label.Text = label.Text + " " + m_CurrentPlayer.overallrating;
	}

	private void RandomizeAttributes(int level)
	{
		DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(13);
		if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
		{
			m_CurrentPlayer.RandomizeAttributes(level);
			LoadPlayerSkills();
		}
	}

	private void buttonRandomPoor_Click(object sender, EventArgs e)
	{
		RandomizeAttributes(0);
	}

	private void buttonRandomBelowAvg_Click(object sender, EventArgs e)
	{
		RandomizeAttributes(1);
	}

	private void buttonRandomAverage_Click(object sender, EventArgs e)
	{
		RandomizeAttributes(2);
	}

	private void buttonRandomAboveAvg_Click(object sender, EventArgs e)
	{
		RandomizeAttributes(3);
	}

	private void buttonRandomGood_Click(object sender, EventArgs e)
	{
		RandomizeAttributes(4);
	}

	private void buttonRandomVeryGood_Click(object sender, EventArgs e)
	{
		RandomizeAttributes(5);
	}

	private void buttonRandomSuperstar_Click(object sender, EventArgs e)
	{
		RandomizeAttributes(6);
	}

	private void numericOverall_ValueChanged(object sender, EventArgs e)
	{
		if (m_OverallSema)
		{
			m_OverallSema = false;
			int num = (int)numericRandomize.Value;
			int averageRoleAttribute = m_CurrentPlayer.GetAverageRoleAttribute();
			int num2 = num - averageRoleAttribute;
			if (num2 == 0)
			{
				return;
			}
			if (numericGoalkeeperSkills.Value + (decimal)num2 < 100m && numericGoalkeeperSkills.Value + (decimal)num2 > 0m)
			{
				numericGoalkeeperSkills.Value += (decimal)num2;
			}
			if (numericDefensiveSkills.Value + (decimal)num2 < 100m && numericGoalkeeperSkills.Value + (decimal)num2 > 0m)
			{
				numericDefensiveSkills.Value += (decimal)num2;
			}
			if (numericMidfielderSkills.Value + (decimal)num2 < 100m && numericGoalkeeperSkills.Value + (decimal)num2 > 0m)
			{
				numericMidfielderSkills.Value += (decimal)num2;
			}
			if (numericAttackingSkills.Value + (decimal)num2 < 100m && numericGoalkeeperSkills.Value + (decimal)num2 > 0m)
			{
				numericAttackingSkills.Value += (decimal)num2;
			}
			if (numericPhysicalSkills.Value + (decimal)num2 < 100m && numericGoalkeeperSkills.Value + (decimal)num2 > 0m)
			{
				numericPhysicalSkills.Value += (decimal)num2;
			}
			if (numericMentalSkills.Value + (decimal)num2 < 100m && numericGoalkeeperSkills.Value + (decimal)num2 > 0m)
			{
				numericMentalSkills.Value += (decimal)num2;
			}
			if (numericFreeKickSkills.Value + (decimal)num2 < 100m && numericGoalkeeperSkills.Value + (decimal)num2 > 0m)
			{
				numericFreeKickSkills.Value += (decimal)num2;
			}
		}
		trackOverallrating.Value = (int)numericRandomize.Value;
		m_OverallSema = true;
	}

	private void numericGoalkeeperSkills_ValueChanged(object sender, EventArgs e)
	{
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			int num = (int)numericGoalkeeperSkills.Value;
			int num2 = m_CurrentPlayer.ComputeMeanAttributes(0);
			int num3 = num - num2;
			int num4 = trackPositioning.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackPositioning.Value = num4;
			num4 = trackDiving.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackDiving.Value = num4;
			num4 = trackHandling.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackHandling.Value = num4;
			num4 = trackReflexes.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackReflexes.Value = num4;
			num4 = trackGkKicking.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackGkKicking.Value = num4;
			m_AttributesSema = true;
		}
	}

	private void numericDefensiveSkills_ValueChanged(object sender, EventArgs e)
	{
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			int num = (int)numericDefensiveSkills.Value;
			int num2 = m_CurrentPlayer.ComputeMeanAttributes(1);
			int num3 = num - num2;
			int num4 = trackAggression.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackAggression.Value = num4;
			num4 = trackTackling.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackTackling.Value = num4;
			num4 = trackSliding.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackSliding.Value = num4;
			num4 = trackMarking.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackMarking.Value = num4;
			num4 = trackInterception.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackInterception.Value = num4;
			m_AttributesSema = true;
		}
	}

	private void numericMidfielderSkills_ValueChanged(object sender, EventArgs e)
	{
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			int num = (int)numericMidfielderSkills.Value;
			int num2 = m_CurrentPlayer.ComputeMeanAttributes(2);
			int num3 = num - num2;
			int num4 = trackShortPassing.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackShortPassing.Value = num4;
			num4 = trackLongPassing.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackLongPassing.Value = num4;
			num4 = trackCrossing.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackCrossing.Value = num4;
			num4 = trackBallControl.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackBallControl.Value = num4;
			num4 = trackVision.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackVision.Value = num4;
			num4 = trackCurve.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackCurve.Value = num4;
			m_AttributesSema = true;
		}
	}

	private void numericAttackingSkills_ValueChanged(object sender, EventArgs e)
	{
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			int num = (int)numericAttackingSkills.Value;
			int num2 = m_CurrentPlayer.ComputeMeanAttributes(3);
			int num3 = num - num2;
			int num4 = trackFinishing.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackFinishing.Value = num4;
			num4 = trackShotPower.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackShotPower.Value = num4;
			num4 = trackLongShot.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackLongShot.Value = num4;
			num4 = trackDribbling.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackDribbling.Value = num4;
			num4 = trackVolley.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackVolley.Value = num4;
			num4 = trackHeading.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackHeading.Value = num4;
			num4 = trackFreeKick.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackFreeKick.Value = num4;
			num4 = trackPenalties.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackPenalties.Value = num4;
			m_AttributesSema = true;
		}
	}

	private void numericFreeKickSkills_ValueChanged(object sender, EventArgs e)
	{
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			int num = (int)numericFreeKickSkills.Value;
			int num2 = m_CurrentPlayer.ComputeMeanAttributes(6);
			int num3 = num - num2;
			int num4 = trackFreeKick.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackFreeKick.Value = num4;
			num4 = trackPenalties.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackPenalties.Value = num4;
			m_AttributesSema = true;
		}
	}

	private void numericGenericSkills_ValueChanged(object sender, EventArgs e)
	{
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			int num = (int)numericPhysicalSkills.Value;
			int num2 = m_CurrentPlayer.ComputeMeanAttributes(4);
			int num3 = num - num2;
			int num4 = trackAcceleration.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackAcceleration.Value = num4;
			num4 = trackSprintSpeed.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackSprintSpeed.Value = num4;
			num4 = trackStamina.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackStamina.Value = num4;
			num4 = trackStrength.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackStrength.Value = num4;
			num4 = trackAgility.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackAgility.Value = num4;
			num4 = trackJumping.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackJumping.Value = num4;
			num4 = trackReactions.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackReactions.Value = num4;
			num4 = trackBalance.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackBalance.Value = num4;
			m_AttributesSema = true;
		}
	}

	private void numericMentalSkills_ValueChanged(object sender, EventArgs e)
	{
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			int num = (int)numericMentalSkills.Value;
			int num2 = m_CurrentPlayer.ComputeMeanAttributes(5);
			int num3 = num - num2;
			int num4 = trackPotential.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackPotential.Value = num4;
			num4 = trackPlayerPositioning.Value + num3;
			num4 = ((num4 < 1) ? 1 : ((num4 > 99) ? 99 : num4));
			trackPlayerPositioning.Value = num4;
			m_AttributesSema = true;
		}
	}

	private void tabEditPlayer_SelectedIndexChanged(object sender, EventArgs e)
	{
		LoadPlayer(m_CurrentPlayer);
	}

	private void LoadPlayerFace()
	{
		m_GenericAppearanceSema = false;
		checkHasGenericFace.Checked = m_CurrentPlayer.headclasscode != 0;
		groupHairModel.Enabled = checkHasGenericFace.Checked;
		groupHeadModel.Enabled = checkHasGenericFace.Checked;
		groupGenericFaceType.Enabled = checkHasGenericFace.Checked;
		groupSpecifiHeadControls.Enabled = !checkHasGenericFace.Checked;
		buttonRgbHair.Visible = !checkHasGenericFace.Checked && checkShowTexures.Checked;
		SetNumericValue(numericSkinTone, m_CurrentPlayer.skintonecode);
		SetSkinLabel(m_CurrentPlayer.skintonecode);
		GenericHead.EHeadModelSet eHeadModelSet = GenericHead.GetModelSet(m_CurrentPlayer.headtypecode);
		int modelSetIndex = GenericHead.GetModelSetIndex(eHeadModelSet, m_CurrentPlayer.headtypecode);
		switch (eHeadModelSet)
		{
		case GenericHead.EHeadModelSet.Caucasic:
			SetSelectedIndex(comboCaucasicModels, modelSetIndex);
			radioButtonCaucasic.Checked = true;
			break;
		case GenericHead.EHeadModelSet.Latin:
			SetSelectedIndex(comboLatinModels, modelSetIndex);
			radioButtonLatin.Checked = true;
			break;
		case GenericHead.EHeadModelSet.African:
			SetSelectedIndex(comboAfricanModels, modelSetIndex);
			radioButtonAfrican.Checked = true;
			break;
		case GenericHead.EHeadModelSet.Asiatic:
			SetSelectedIndex(comboAsiaticModels, modelSetIndex);
			radioButtonAsiatic.Checked = true;
			break;
		case GenericHead.EHeadModelSet.Female:
			SetSelectedIndex(comboFemaleModels, modelSetIndex);
			radioButtonFemale.Checked = true;
			break;
		}
		GenericHead.EHairModelSet hairModelSet = GenericHead.GetHairModelSet(m_CurrentPlayer.hairtypecode);
		int hairModelSetIndex = GenericHead.GetHairModelSetIndex(hairModelSet, m_CurrentPlayer.hairtypecode);
		switch (hairModelSet)
		{
		case GenericHead.EHairModelSet.Shaven:
			SetSelectedIndex(comboShaven, hairModelSetIndex);
			radioShaven.Checked = true;
			break;
		case GenericHead.EHairModelSet.VeryShort:
			SetSelectedIndex(comboVeryShort, hairModelSetIndex);
			radioVeryShort.Checked = true;
			break;
		case GenericHead.EHairModelSet.Short:
			SetSelectedIndex(comboShort, hairModelSetIndex);
			radioShort.Checked = true;
			break;
		case GenericHead.EHairModelSet.Modern:
			SetSelectedIndex(comboModern, hairModelSetIndex);
			radioModern.Checked = true;
			break;
		case GenericHead.EHairModelSet.Medium:
			SetSelectedIndex(comboMedium, hairModelSetIndex);
			radioMedium.Checked = true;
			break;
		case GenericHead.EHairModelSet.Long:
			SetSelectedIndex(comboLong, hairModelSetIndex);
			radioLong.Checked = true;
			break;
		case GenericHead.EHairModelSet.Afro:
			SetSelectedIndex(comboAfro, hairModelSetIndex);
			radioAfro.Checked = true;
			break;
		case GenericHead.EHairModelSet.Headbend:
			SetSelectedIndex(comboHeadband, hairModelSetIndex);
			radioHeadband.Checked = true;
			break;
		case GenericHead.EHairModelSet.FemaleHair:
			SetSelectedIndex(comboFemaleHair, hairModelSetIndex);
			radioButtonFemaleHair.Checked = true;
			break;
		}
		SetSelectedIndex(domainFacialHair, m_CurrentPlayer.facialhairtypecode);
		SetSelectedIndex(domainHairColor, m_CurrentPlayer.haircolorcode);
		SetSelectedIndex(comboSideburns, m_CurrentPlayer.sideburnscode);
		SetSelectedIndex(comboSkintype, m_CurrentPlayer.skintypecode);
		SetSelectedIndex(comboEyescolor, m_CurrentPlayer.eyecolorcode - 1);
		SetSelectedIndex(comboEyeBow, m_CurrentPlayer.eyebrowcode);
		SetSelectedIndex(comboFaceposer, m_CurrentPlayer.faceposercode);
		SetSelectedIndex(comboFacialHairColor, m_CurrentPlayer.facialhaircolorcode);
		m_GenericAppearanceSema = true;
		viewer2DPlayerGui.CurrentBitmap = m_CurrentPlayer.GetPhoto();
		if (FifaEnvironment.Year == 26)
		{
			m_Fc26Face3dPanel.Visible = true;
			m_Fc26Face3dPanel.BringToFront();
			UpdateAndShowHead3D();
			return;
		}
		m_Fc26Face3dPanel.Visible = false;
		GetAndShowTextures();
		UpdateAndShowHead3D();
	}

	private static void SetNumericValue(NumericUpDown control, decimal value)
	{
		if (value < control.Minimum)
		{
			control.Minimum = value;
		}
		if (value > control.Maximum)
		{
			control.Maximum = value;
		}
		control.Value = value;
	}

	private static void SetSelectedIndex(ComboBox control, int index)
	{
		control.SelectedIndex = index >= 0 && index < control.Items.Count ? index : -1;
	}

	private static void SetSelectedIndex(DomainUpDown control, int index)
	{
		control.SelectedIndex = index >= 0 && index < control.Items.Count ? index : -1;
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
			if (m_CurrentPlayer.headtypecode != GenericHead.c_AsiaticModels[comboAsiaticModels.SelectedIndex])
			{
				m_CurrentPlayer.headtypecode = GenericHead.c_AsiaticModels[comboAsiaticModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
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
			if (m_CurrentPlayer.headtypecode != GenericHead.c_CaucasicModels[comboCaucasicModels.SelectedIndex])
			{
				m_CurrentPlayer.headtypecode = GenericHead.c_CaucasicModels[comboCaucasicModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
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
			if (m_CurrentPlayer.headtypecode != GenericHead.c_AfricanModels[comboAfricanModels.SelectedIndex])
			{
				m_CurrentPlayer.headtypecode = GenericHead.c_AfricanModels[comboAfricanModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
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
			if (m_CurrentPlayer.headtypecode != GenericHead.c_LatinModels[comboLatinModels.SelectedIndex])
			{
				m_CurrentPlayer.headtypecode = GenericHead.c_LatinModels[comboLatinModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
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

	private void radioButtonFemale_CheckedChanged(object sender, EventArgs e)
	{
		if (comboFemaleModels.SelectedIndex < 0)
		{
			comboFemaleModels.SelectedIndex = 0;
		}
		comboFemaleModels.Visible = radioButtonFemale.Checked;
		if (radioButtonFemale.Checked)
		{
			radioButtonFemale.BackColor = Color.LightSkyBlue;
			if (m_CurrentPlayer.headtypecode != GenericHead.c_FemaleModels[comboFemaleModels.SelectedIndex])
			{
				m_CurrentPlayer.headtypecode = GenericHead.c_FemaleModels[comboFemaleModels.SelectedIndex];
				if (m_GenericAppearanceSema && buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
				{
					UpdateAndShowHead3D();
				}
			}
		}
		else
		{
			radioButtonFemale.BackColor = Color.Transparent;
		}
	}

	private void comboAsiaticModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboAsiaticModels.SelectedIndex >= 0)
		{
			m_CurrentPlayer.headtypecode = GenericHead.c_AsiaticModels[comboAsiaticModels.SelectedIndex];
			if (buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboAfricanModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboAfricanModels.SelectedIndex >= 0)
		{
			m_CurrentPlayer.headtypecode = GenericHead.c_AfricanModels[comboAfricanModels.SelectedIndex];
			if (buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboCaucasicModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboCaucasicModels.SelectedIndex >= 0)
		{
			m_CurrentPlayer.headtypecode = GenericHead.c_CaucasicModels[comboCaucasicModels.SelectedIndex];
			if (buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboLatinModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboLatinModels.SelectedIndex >= 0)
		{
			m_CurrentPlayer.headtypecode = GenericHead.c_LatinModels[comboLatinModels.SelectedIndex];
			if (buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboFemaleModels_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboFemaleModels.SelectedIndex >= 0)
		{
			m_CurrentPlayer.headtypecode = GenericHead.c_FemaleModels[comboFemaleModels.SelectedIndex];
			if (buttonShow3DModel.Checked && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanHeadModel();
				UpdateAndShowHead3D();
			}
		}
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

	private void radioButtonFemaleHair_CheckedChanged(object sender, EventArgs e)
	{
		radioHair_CheckedChanged(sender, GenericHead.c_FemaleHairModels);
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
			if (m_CurrentPlayer.hairtypecode != hairMap[comboBox.SelectedIndex])
			{
				m_CurrentPlayer.hairtypecode = hairMap[comboBox.SelectedIndex];
				if (m_GenericAppearanceSema && (buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
				{
					m_CurrentPlayer.CleanHairTextures();
					GetAndShowHairTexture();
					UpdateAndShowHead3D();
				}
			}
		}
		else
		{
			radioButton.BackColor = Color.Transparent;
		}
	}

	private void comboHeadband_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_HeadbendModels);
	}

	private void comboFemaleHair_SelectedIndexChanged(object sender, EventArgs e)
	{
		comboHair_SelectedIndexChanged(sender, GenericHead.c_FemaleHairModels);
	}

	private void comboHair_SelectedIndexChanged(object sender, int[] hairMap)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (m_GenericAppearanceSema && comboBox.SelectedIndex >= 0)
		{
			m_CurrentPlayer.hairtypecode = hairMap[comboBox.SelectedIndex];
			if (m_GenericAppearanceSema && (buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanHair();
				GetAndShowHairTexture();
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

	private void EnableTool3DButtons()
	{
		if (m_CurrentPlayer != null)
		{
			buttonImport3DHairModel.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonRemoveHairModel.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMoveHairDown.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMoveHairAhead.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMoveHairBack.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMoveHairUp.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMoveHairLeft.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMoveHairRight.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMakeHairCloser.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonMakeHairWider.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
			buttonRemove3DHeadModel.Enabled = m_CurrentPlayer.HasSpecificHeadModel;
		}
	}

	private void UpdateAndShowHead3D()
	{
		if (FifaEnvironment.Year == 26)
		{
			m_Fc26Face3dPanel.Visible = true;
			m_Fc26Face3dPanel.BringToFront();
			if (m_CurrentPlayer == null)
			{
				m_Fc26Mesh3DHost.ShowStatus("No FC26 player selected.");
				return;
			}

			var key = GetFc26FaceMeshKey(m_CurrentPlayer);
			if (!string.Equals(m_Fc26FaceMeshKey, key, StringComparison.Ordinal))
			{
				m_Fc26FaceMeshKey = key;
				++m_Fc26FaceMeshRequest;
				m_Fc26RenderedFaceMeshKey = null;
			}

			var playerId = m_CurrentPlayer.m_assetid > 0 ? m_CurrentPlayer.m_assetid : m_CurrentPlayer.Id;
			if (m_Fc26FaceMeshCache.TryGetValue(key, out var cachedMesh) && System.IO.File.Exists(cachedMesh))
			{
				if (!string.Equals(m_Fc26RenderedFaceMeshKey, key, StringComparison.Ordinal))
				{
					m_Fc26Mesh3DHost.LoadMesh(cachedMesh);
					m_Fc26RenderedFaceMeshKey = key;
				}
				m_Fc26Face3dStatus.Text = $"FC26 face mesh loaded — Player ID {playerId}. Drag to rotate; mouse wheel to zoom.";
				m_Fc26Face3dButton.Text = "Reload FC26 3D face mesh";
			}
			else if (m_Fc26Face3dButton.Enabled)
			{
				m_Fc26Mesh3DHost.ShowStatus("Click below to export and load this player's real FC26 FBX head mesh.");
				m_Fc26Face3dStatus.Text = $"FC26 Frostbite 3D face — Player ID {playerId}.";
				m_Fc26Face3dButton.Text = "Load FC26 3D face mesh";
			}
			return;
		}
		EnableTool3DButtons();
		if (!buttonShow3DModel.Checked)
		{
			viewer3D.ShowEmpty();
			return;
		}
		Bitmap faceTexture = m_CurrentPlayer.GetFaceTexture();
		Bitmap eyesTexture = m_CurrentPlayer.GetEyesTexture();
		Rx3File headModel = m_CurrentPlayer.GetHeadModel();
		if (headModel == null) { viewer3D.ShowEmpty(); return; }
		Player.s_Model3DHead = null;
		Player.s_Model3DEyes = null;
		Player.s_Model3DHairPart4 = null;
		Player.s_Model3DHairPart5 = null;
		if (headModel.Rx3VertexArrays[0].nVertex > headModel.Rx3VertexArrays[1].nVertex)
		{
			Player.s_Model3DHead = new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], faceTexture);
			Player.s_Model3DEyes = new Model3D(headModel.Rx3IndexArrays[1], headModel.Rx3VertexArrays[1], eyesTexture);
		}
		else
		{
			Player.s_Model3DHead = new Model3D(headModel.Rx3IndexArrays[1], headModel.Rx3VertexArrays[1], faceTexture);
			Player.s_Model3DEyes = new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], eyesTexture);
		}
		Rx3File hairModel = m_CurrentPlayer.GetHairModel();
		if (hairModel != null)
		{
			Bitmap hairColorTexture = m_CurrentPlayer.GetHairColorTexture();
			Bitmap genericHairColorTexture = m_CurrentPlayer.GetGenericHairColorTexture();
			Bitmap hairAlfaTexture = m_CurrentPlayer.GetHairAlfaTexture();
			Bitmap bitmap = null;
			Bitmap bitmap2 = null;
			if (hairAlfaTexture != null)
			{
				_ = hairColorTexture.Width / hairAlfaTexture.Width;
				_ = hairColorTexture.Height / hairAlfaTexture.Height;
				if (genericHairColorTexture != null)
				{
					bitmap = (Bitmap)GraphicUtil.CanvasSizeBitmapCentered(hairColorTexture, hairAlfaTexture.Width, hairAlfaTexture.Height).Clone();
					GraphicUtil.GetAlfaFromChannel(bitmap, hairAlfaTexture, 4 - m_HairAlfaChannel);
				}
				if (hairColorTexture != null)
				{
					bitmap2 = (Bitmap)GraphicUtil.CanvasSizeBitmapCentered(hairColorTexture, hairAlfaTexture.Width, hairAlfaTexture.Height).Clone();
					GraphicUtil.GetAlfaFromChannel(bitmap2, hairAlfaTexture, m_HairAlfaChannel);
				}
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
		if (buttonShowJesey.Checked && m_CurrentPlayer.m_PlayingForTeams.Count > 0)
		{
			Team team = (Team)m_CurrentPlayer.m_PlayingForTeams[0];
			int num2 = ((m_CurrentPlayer.preferredposition1 == 0) ? 2 : 0);
			if (num2 == 2 && team.m_KitList.Count < 3)
			{
				kit = FifaEnvironment.Kits.GetKit(5000 + (team.Id & 0xF), 2);
			}
			else
			{
				for (int i = 0; i < team.m_KitList.Count; i++)
				{
					kit = (Kit)team.m_KitList[i];
					if (kit.kittype == num2 && kit.year == 0)
					{
						break;
					}
				}
			}
			if (kit != null)
			{
				Bitmap[] kitTextures = kit.GetKitTextures();
				if (kitTextures != null)
				{
					Bitmap textureBitmap = GraphicUtil.EmbossBitmap(kitTextures[1], Kit.s_JerseyWrinkle);
					Kit.s_JerseyModel3D[kit.jerseyCollar].TextureBitmap = textureBitmap;
				}
			}
		}
		if (kit != null)
		{
			num++;
		}
		viewer3D.Clean(num);
		int num3 = 0;
		if (kit != null)
		{
			viewer3D.SetMesh(num3++, Kit.s_JerseyModel3D[kit.jerseyCollar]);
		}
		viewer3D.SetMesh(num3++, Player.s_Model3DHead);
		viewer3D.SetMesh(num3++, Player.s_Model3DEyes);
		if (Player.s_Model3DHairPart4 != null)
		{
			viewer3D.SetMesh(num3++, Player.s_Model3DHairPart4, zBufferState: false);
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			viewer3D.SetMesh(num3++, Player.s_Model3DHairPart5, zBufferState: false);
		}
		viewer3D.Render();
	}

	private void GetAndShowTextures()
	{
		GetAndShowFaceTexture();
		GetAndShowHairTexture();
		GetAndShowSkinTexture();
		GetAndShowEyeTexture();
		GetAndShowTattoosTexture();
	}

	private void GetAndShowSkinTexture()
	{
		if (checkShowTexures.Checked && viewer2DSkinTexture.ShowButtonChecked)
		{
			viewer2DSkinTexture.CurrentBitmap = m_CurrentPlayer.GetSkinTexture();
			viewer2DSkinTexture.Enabled = true;
		}
		else
		{
			viewer2DSkinTexture.CurrentBitmap = null;
			viewer2DSkinTexture.Enabled = false;
		}
	}

	private void GetAndShowTattoosTexture()
	{
		if (checkShowTexures.Checked && m_CurrentPlayer.HasSpecificHeadModel && viewer2DTattoos.ShowButtonChecked)
		{
			viewer2DTattoos.CurrentBitmap = m_CurrentPlayer.GetTattoos();
			viewer2DTattoos.Enabled = true;
		}
		else
		{
			viewer2DTattoos.CurrentBitmap = null;
			viewer2DTattoos.Enabled = false;
		}
	}

	private void GetAndShowEyeTexture()
	{
		if (checkShowTexures.Checked && viewer2DEyeTexture.ShowButtonChecked)
		{
			viewer2DEyeTexture.CurrentBitmap = m_CurrentPlayer.GetEyesTexture();
			viewer2DEyeTexture.Enabled = true;
		}
		else
		{
			viewer2DEyeTexture.CurrentBitmap = null;
			viewer2DEyeTexture.Enabled = false;
		}
	}

	private void GetAndShowFaceTexture()
	{
		if (checkShowTexures.Checked && m_CurrentPlayer.HasSpecificHeadModel)
		{
			Bitmap[] faceTextures = m_CurrentPlayer.GetFaceTextures();
			if (faceTextures != null)
			{
				Bitmap[] array = new Bitmap[faceTextures.Length];
				for (int i = 0; i < array.Length; i++)
				{
					if (faceTextures != null)
					{
						if (faceTextures.Length > i && faceTextures[i] != null)
						{
							array[i] = (Bitmap)m_CurrentPlayer.GetFaceTextures()[i].Clone();
						}
						else
						{
							array[i] = null;
						}
					}
					else
					{
						array[i] = null;
					}
				}
				multiViewerFace.Bitmaps = array;
				multiViewerFace.Enabled = true;
			}
			else
			{
				multiViewerFace.Bitmaps = null;
				multiViewerFace.Enabled = true;
			}
		}
		else
		{
			multiViewerFace.Bitmaps = null;
			multiViewerFace.Enabled = false;
		}
	}

	private void GetAndShowHairTexture()
	{
		if (checkShowTexures.Checked)
		{
			Bitmap[] array = new Bitmap[2];
			for (int i = 0; i < array.Length; i++)
			{
				if (m_CurrentPlayer.GetHairTextures() != null)
				{
					array[i] = (Bitmap)m_CurrentPlayer.GetHairTextures()[i].Clone();
				}
				else
				{
					array[i] = null;
				}
			}
			multiViewerHair.Bitmaps = array;
			multiViewerHair.Enabled = true;
			buttonRgbHair.Visible = !checkHasGenericFace.Checked;
		}
		else
		{
			multiViewerHair.Bitmaps = null;
			multiViewerHair.Enabled = false;
			buttonRgbHair.Visible = false;
		}
	}

	private void domainHairColor_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema)
		{
			m_CurrentPlayer.haircolorcode = domainHairColor.SelectedIndex;
			if ((buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanHairTextures();
				GetAndShowHairTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboSkintype_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboSkintype.SelectedIndex >= 0)
		{
			m_CurrentPlayer.skintypecode = comboSkintype.SelectedIndex;
			if ((buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanFaceTextures();
				GetAndShowFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboEyescolor_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboEyescolor.SelectedIndex >= 0)
		{
			m_CurrentPlayer.eyecolorcode = comboEyescolor.SelectedIndex + 1;
			m_CurrentPlayer.CleanEyesTexture();
			GetAndShowEyeTexture();
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
			m_CurrentPlayer.facialhairtypecode = domainFacialHair.SelectedIndex;
			if ((buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanFaceTextures();
				GetAndShowFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboFacialHairColor_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboFacialHairColor.SelectedIndex >= 0)
		{
			m_CurrentPlayer.facialhaircolorcode = comboFacialHairColor.SelectedIndex;
			if ((buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanFaceTextures();
				GetAndShowFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboSideburns_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboSideburns.SelectedIndex >= 0)
		{
			m_CurrentPlayer.sideburnscode = comboSideburns.SelectedIndex;
			if ((buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanFaceTextures();
				GetAndShowFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void comboEyeBow_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_GenericAppearanceSema && comboEyeBow.SelectedIndex >= 0)
		{
			m_CurrentPlayer.eyebrowcode = comboEyeBow.SelectedIndex;
			if ((buttonShow3DModel.Checked || checkShowTexures.Checked) && !m_CurrentPlayer.HasSpecificHeadModel)
			{
				m_CurrentPlayer.CleanFaceTextures();
				GetAndShowFaceTexture();
				UpdateAndShowHead3D();
			}
		}
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		UpdateAndShowHead3D();
	}

	private void checkShowTexures_CheckedChanged(object sender, EventArgs e)
	{
		GetAndShowTextures();
	}

	private void buttonRandomizeAppearance_Click(object sender, EventArgs e)
	{
		m_FaceSelector.SelectedKey = m_CurrentPlayer.headtypecode;
		m_FaceSelector.SetPlayerPicture(m_CurrentPlayer.GetPhoto());
		if (m_FaceSelector.ShowDialog() == DialogResult.OK && m_CurrentPlayer.headtypecode != m_FaceSelector.SelectedKey)
		{
			m_CurrentPlayer.headtypecode = m_FaceSelector.SelectedKey;
			LoadPlayerFace();
		}
	}

	private void checkHasGenericFace_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			bool flag = checkHasGenericFace.Checked;
			m_CurrentPlayer.headclasscode = (flag ? 1 : 0);
			groupHairModel.Enabled = flag;
			groupHeadModel.Enabled = flag;
			groupGenericFaceType.Enabled = flag;
			groupSpecifiHeadControls.Enabled = !flag;
			buttonRgbHair.Visible = !flag && checkShowTexures.Checked;
			LoadPlayerFace();
		}
	}

	private void checkUsingrevMod_CheckedChanged(object sender, EventArgs e)
	{
		_ = checkHasGenericFace.Checked;
		_ = checkUsingRevMod.Checked;
	}

	private void buttonImport3DHairModels_Click(object sender, EventArgs e)
	{
		string text = FifaEnvironment.BrowseAndCheckModel(ref m_PlayerCurrentFolder, "Open 3D-Near Hair Model file", "3D-Near hair model files (*.rx3)|hair_*.rx3");
		if (text != null)
		{
			m_CurrentPlayer.SetHairModel(text);
			text = FifaEnvironment.BrowseAndCheckModel(ref m_PlayerCurrentFolder, "Open 3D-Far Hair Model file", "3D-Far hair model files (*.rx3)|hairlod_*.rx3");
			if (text != null)
			{
				m_CurrentPlayer.SetHairLodModel(text);
				m_CurrentPlayer.CleanHairModel();
				LoadPlayerFace();
			}
		}
	}

	private void buttonExport3DHairModels_Click(object sender, EventArgs e)
	{
		string text = m_CurrentPlayer.HairModelFileName();
		if (text != null)
		{
			FifaEnvironment.AskAndExportFromZdata(text, ref m_PlayerCurrentFolder);
		}
		text = m_CurrentPlayer.HairLodModelFileName();
		if (text != null)
		{
			FifaEnvironment.AskAndExportFromZdata(text, ref m_PlayerCurrentFolder);
		}
	}

	private void buttonRemove3DModel_Click(object sender, EventArgs e)
	{
		if (m_CurrentPlayer.HasSpecificHeadModel)
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(10);
			if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
			{
				m_CurrentPlayer.DeleteHeadModel();
				LoadPlayerFace();
			}
		}
	}

	private void buttonImport3DHeadModel_Click(object sender, EventArgs e)
	{
		string text = FifaEnvironment.BrowseAndCheckModel(ref m_PlayerCurrentFolder, "Open 3D Head Model file", "3D head model files (*.rx3)|head_*.rx3");
		if (text != null)
		{
			m_CurrentPlayer.CleanHead();
			m_CurrentPlayer.SetHeadModel(text);
			LoadPlayerFace();
		}
	}

	private void buttonExport3DHeadModel_Click(object sender, EventArgs e)
	{
		string text = m_CurrentPlayer.HeadModelFileName();
		if (text != null)
		{
			FifaEnvironment.AskAndExportFromZdata(text, ref m_PlayerCurrentFolder);
		}
	}

	private void comboPreferredPosition1_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboPreferredPosition1.SelectedIndex >= 0)
		{
			m_CurrentPlayer.preferredposition1 = comboPreferredPosition1.SelectedIndex - 1;
		}
	}

	private void comboPreferredPosition2_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboPreferredPosition2.SelectedIndex >= 0)
		{
			m_CurrentPlayer.preferredposition2 = comboPreferredPosition2.SelectedIndex - 1;
		}
	}

	private void comboPreferredPosition3_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboPreferredPosition3.SelectedIndex >= 0)
		{
			m_CurrentPlayer.preferredposition3 = comboPreferredPosition3.SelectedIndex - 1;
		}
	}

	private void comboPreferredPosition4_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboPreferredPosition4.SelectedIndex >= 0)
		{
			m_CurrentPlayer.preferredposition4 = comboPreferredPosition4.SelectedIndex - 1;
		}
	}

	private void numericSkillMoves_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			labelSkillsStars.ImageIndex = (int)numericSkillMoves.Value - 1;
			m_CurrentPlayer.skillmoves = (int)numericSkillMoves.Value;
		}
	}

	private void toolPhoto_Click(object sender, EventArgs e)
	{
		Bitmap bitmap = viewer3D.Photo();
		int num = bitmap.Height * 85 / 100;
		int num2 = bitmap.Width;
		int num3 = ((num2 < num) ? num2 : num);
		int num4 = (num2 - num3) / 2;
		Rectangle srcRect = new Rectangle(num4, 0, num3, num);
		Rectangle destRect = new Rectangle(0, 0, 128, 128);
		Bitmap srcBitmap = GraphicUtil.MakeAutoTransparent(bitmap);
		Bitmap bitmap2 = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
		GraphicUtil.RemapRectangle(srcBitmap, srcRect, bitmap2, destRect);
		m_CurrentPlayer.SetPhoto(bitmap2);
		viewer2DPlayerGui.CurrentBitmap = bitmap2;
	}

	private void labelGkGloves_DoubleClick(object sender, EventArgs e)
	{
		GkGloves gkGloves = (GkGloves)FifaEnvironment.GkGloves.SearchId(m_CurrentPlayer.gkglovetypecode);
		if (gkGloves != null)
		{
			MainForm.CM.JumpTo(gkGloves);
		}
	}

	private void buttonSwitchRenderingMode_Click(object sender, EventArgs e)
	{
		m_HairAlfaChannel = 4 - m_HairAlfaChannel;
		UpdateAndShowHead3D();
	}

	private void buttonAhead_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MoveForward();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MoveForward();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonBack_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MoveBack();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MoveBack();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonUp_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MoveUp();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MoveUp();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonDown_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MoveDown();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MoveDown();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonSaveHair_Click(object sender, EventArgs e)
	{
		PositionNormalTextured[] newVertex = null;
		PositionNormalTextured[] newVertex2 = null;
		if (Player.s_Model3DHairPart4 != null)
		{
			newVertex = Player.s_Model3DHairPart4.Vertex;
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			newVertex2 = Player.s_Model3DHairPart5.Vertex;
		}
		m_CurrentPlayer.UpdateHairVertex(newVertex, newVertex2);
		buttonSaveHair.Enabled = false;
	}

	private void buttonRemoveHairModel_Click(object sender, EventArgs e)
	{
		if (m_CurrentPlayer.HasSpecificHeadModel)
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(10);
			if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
			{
				m_CurrentPlayer.DeleteHairModel();
				m_CurrentPlayer.DeleteHairLodModel();
				LoadPlayerFace();
			}
		}
	}

	private void textFirstName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			textFirstName.Text.Trim();
			m_CurrentPlayer.firstname = textFirstName.Text;
			pickUpControl.SwitchObject(m_CurrentPlayer);
		}
	}

	private void textSurname_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.lastname = textSurname.Text;
			if (m_CurrentPlayer.commonname == string.Empty)
			{
				m_CurrentPlayer.audioname = m_CurrentPlayer.lastname;
				m_CurrentPlayer.commentaryid = 900000;
			}
			pickUpControl.SwitchObject(m_CurrentPlayer);
		}
	}

	private void textCommonName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.commonname = textCommonName.Text;
			m_CurrentPlayer.audioname = m_CurrentPlayer.commonname;
			m_CurrentPlayer.commentaryid = 900000;
			pickUpControl.SwitchObject(m_CurrentPlayer);
		}
	}

	private void buttonShowJesey_Click(object sender, EventArgs e)
	{
		ShowHead3D();
	}

	private void listViewPlayingTeams_DoubleClick(object sender, EventArgs e)
	{
		if (listViewPlayingTeams.SelectedItems.Count > 0)
		{
			Team team = (Team)listViewPlayingTeams.SelectedItems[0].Tag;
			if (team != null)
			{
				MainForm.CM.JumpTo(team);
			}
		}
	}

	private void SetSkinLabel(int skintone)
	{
		switch (skintone)
		{
		case 1:
			labelSkinColorInfo.Text = "Light Pink";
			break;
		case 2:
			labelSkinColorInfo.Text = "Pink";
			break;
		case 3:
			labelSkinColorInfo.Text = "Dark Pink";
			break;
		case 4:
			labelSkinColorInfo.Text = "Light Yellow";
			break;
		case 5:
			labelSkinColorInfo.Text = "Medium Yellow";
			break;
		case 6:
			labelSkinColorInfo.Text = "Dark Yellow";
			break;
		case 7:
			labelSkinColorInfo.Text = "Very Light Brown";
			break;
		case 8:
			labelSkinColorInfo.Text = "Light Brown";
			break;
		case 9:
			labelSkinColorInfo.Text = "Medium Brown";
			break;
		case 10:
			labelSkinColorInfo.Text = "Dark Brown";
			break;
		default:
			labelSkinColorInfo.Text = "Special";
			break;
		}
	}

	private void numericSkinTone_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		int num = (int)numericSkinTone.Value;
		m_CurrentPlayer.skintonecode = num;
		SetSkinLabel(num);
		GetAndShowSkinTexture();
		if (!m_CurrentPlayer.HasSpecificHeadModel)
		{
			m_CurrentPlayer.CleanFaceTextures();
			GetAndShowFaceTexture();
			if (buttonShow3DModel.Checked)
			{
				UpdateAndShowHead3D();
			}
		}
	}

	private void buttonRgbHair_Click(object sender, EventArgs e)
	{
		if (multiViewerHair.Bitmaps == null)
		{
			return;
		}
		Bitmap bitmap = multiViewerHair.Bitmaps[1];
		if (bitmap != null)
		{
			ModifyHairColor modifyHairColor = new ModifyHairColor(bitmap);
			if (modifyHairColor.ShowDialog() == DialogResult.OK)
			{
				multiViewerHair.Bitmaps[1] = modifyHairColor.Bitmap;
				multiViewerHair.buttonSave.Enabled = true;
			}
			modifyHairColor.Dispose();
		}
	}

	private void checkIsLoan_CheckedChanged(object sender, EventArgs e)
	{
		groupIsLoan.Visible = checkIsLoan.Checked;
		if (m_Locked)
		{
			return;
		}
		if (checkIsLoan.Checked)
		{
			m_CurrentPlayer.IsLoaned = true;
			if (m_CurrentPlayer.TeamLoanedFrom == null)
			{
				if (m_CurrentPlayer.PreviousTeam != null)
				{
					m_CurrentPlayer.TeamLoanedFrom = m_CurrentPlayer.PreviousTeam;
				}
				else
				{
					m_CurrentPlayer.TeamLoanedFrom = (Team)comboTeamLoanedFrom.SelectedItem;
				}
				comboTeamLoanedFrom.SelectedItem = m_CurrentPlayer.TeamLoanedFrom;
			}
			_ = m_CurrentPlayer.loandateend;
		}
		else
		{
			m_CurrentPlayer.IsLoaned = false;
		}
	}

	private void comboTeamLoanedFrom_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboTeamLoanedFrom.SelectedItem == null)
		{
			comboTeamLoanedFrom.Text = string.Empty;
		}
	}

	private void pictureColorShoes1_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(Shoes.ShoesColorPalette, m_CurrentPlayer.shoecolorcode1);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentPlayer.shoecolorcode1 = colorSelector.SelectedIndex;
			pictureColorShoes1.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void pictureColorShoes2_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(Shoes.ShoesColorPalette, m_CurrentPlayer.shoecolorcode2);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentPlayer.shoecolorcode2 = colorSelector.SelectedIndex;
			pictureColorShoes2.BackColor = colorSelector.SelectedColor;
		}
		colorSelector.Dispose();
	}

	private void trackInterception_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentPlayer.interceptions = trackInterception.Value;
		labelInterception.Text = labelInterception.Text.Substring(0, labelInterception.Text.IndexOf(' '));
		Label label = labelInterception;
		label.Text = label.Text + " " + m_CurrentPlayer.interceptions;
		if (m_AttributesSema)
		{
			m_AttributesSema = false;
			numericDefensiveSkills.Value = m_CurrentPlayer.ComputeMeanAttributes(1);
			m_AttributesSema = true;
		}
	}

	private void buttonMoveHairLeft_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MoveLeft();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MoveLeft();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonMoveHairRight_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MoveRight();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MoveRight();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonMakeHairCloser_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MakeCloser();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MakeCloser();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonMakeHairWider_Click(object sender, EventArgs e)
	{
		if (Player.s_Model3DHairPart4 != null)
		{
			Player.s_Model3DHairPart4.MakeWider();
		}
		if (Player.s_Model3DHairPart5 != null)
		{
			Player.s_Model3DHairPart5.MakeWider();
		}
		buttonSaveHair.Enabled = true;
		ShowHead3D();
	}

	private void buttonHairSelection_Click(object sender, EventArgs e)
	{
		m_HairSelector.SelectedKey = m_CurrentPlayer.hairtypecode;
		if (m_HairSelector.ShowDialog() == DialogResult.OK && m_CurrentPlayer.hairtypecode != m_HairSelector.SelectedKey)
		{
			m_CurrentPlayer.hairtypecode = m_HairSelector.SelectedKey;
			m_CurrentPlayer.CleanHair();
			LoadPlayerFace();
		}
	}

	private void comboFaceposer_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboFaceposer.SelectedIndex >= 0)
		{
			m_CurrentPlayer.faceposercode = comboFaceposer.SelectedIndex;
		}
	}

	private void textJerseyName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentPlayer.playerjerseyname = textJerseyName.Text;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.PlayerForm));
		this.tabEditPlayer = new System.Windows.Forms.TabControl();
		this.pageInfo = new System.Windows.Forms.TabPage();
		this.flowPanelInfo = new System.Windows.Forms.FlowLayoutPanel();
		this.groupPlayerIdentity = new System.Windows.Forms.GroupBox();
		this.radioButtonGenderFemale = new System.Windows.Forms.RadioButton();
		this.playerBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.radioButtonGenderMale = new System.Windows.Forms.RadioButton();
		this.labelCommonName = new System.Windows.Forms.Label();
		this.textCommonName = new System.Windows.Forms.TextBox();
		this.textJerseyName = new System.Windows.Forms.TextBox();
		this.labelJerseyName = new System.Windows.Forms.Label();
		this.buttonGetId = new System.Windows.Forms.Button();
		this.viewer2DPhoto = new FifaControls.Viewer2D();
		this.numericPlayerId = new System.Windows.Forms.NumericUpDown();
		this.buttonRandomizeIdentity = new System.Windows.Forms.Button();
		this.dateBirthDate = new System.Windows.Forms.DateTimePicker();
		this.labelBirthdate = new System.Windows.Forms.Label();
		this.labelPlayerId = new System.Windows.Forms.Label();
		this.textSurname = new System.Windows.Forms.TextBox();
		this.textFirstName = new System.Windows.Forms.TextBox();
		this.comboCountry = new System.Windows.Forms.ComboBox();
		this.countryListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.labelFirstName = new System.Windows.Forms.Label();
		this.labelSurame = new System.Windows.Forms.Label();
		this.labelCountry = new System.Windows.Forms.Label();
		this.groupBoxBody = new System.Windows.Forms.GroupBox();
		this.comboWeakFoot = new System.Windows.Forms.ComboBox();
		this.labelWeakFoot = new System.Windows.Forms.Label();
		this.comboBody = new System.Windows.Forms.ComboBox();
		this.numericHeight = new System.Windows.Forms.NumericUpDown();
		this.numericWeight = new System.Windows.Forms.NumericUpDown();
		this.labelWeight = new System.Windows.Forms.Label();
		this.labelBody = new System.Windows.Forms.Label();
		this.domainPreferredFoot = new System.Windows.Forms.DomainUpDown();
		this.labelHeight = new System.Windows.Forms.Label();
		this.labelPreferredFoot = new System.Windows.Forms.Label();
		this.groupBoxLook = new System.Windows.Forms.GroupBox();
		this.checkJerseyFit = new System.Windows.Forms.CheckBox();
		this.checkTrainingPants = new System.Windows.Forms.CheckBox();
		this.domainSocksStyle = new System.Windows.Forms.DomainUpDown();
		this.label8 = new System.Windows.Forms.Label();
		this.numericGkGloves = new System.Windows.Forms.NumericUpDown();
		this.labelGkGloves = new System.Windows.Forms.Label();
		this.labelWinter = new System.Windows.Forms.Label();
		this.comboWinterAccessories = new System.Windows.Forms.ComboBox();
		this.domainJerseyStyle = new System.Windows.Forms.DomainUpDown();
		this.domainSleeves = new System.Windows.Forms.DomainUpDown();
		this.pictureColorAcc2 = new System.Windows.Forms.PictureBox();
		this.pictureColorAcc3 = new System.Windows.Forms.PictureBox();
		this.pictureColorAcc4 = new System.Windows.Forms.PictureBox();
		this.pictureColorAcc1 = new System.Windows.Forms.PictureBox();
		this.domainAccessory4 = new System.Windows.Forms.ComboBox();
		this.domainAccessory3 = new System.Windows.Forms.ComboBox();
		this.domainAccessory2 = new System.Windows.Forms.ComboBox();
		this.domainAccessory1 = new System.Windows.Forms.ComboBox();
		this.labelSleeves = new System.Windows.Forms.Label();
		this.labelJerseyStyle = new System.Windows.Forms.Label();
		this.labelAccesories = new System.Windows.Forms.Label();
		this.groupPlayFirTeam = new System.Windows.Forms.GroupBox();
		this.label15 = new System.Windows.Forms.Label();
		this.groupIsLoan = new System.Windows.Forms.GroupBox();
		this.comboTeamLoanedFrom = new System.Windows.Forms.ComboBox();
		this.teamListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.label12 = new System.Windows.Forms.Label();
		this.dateLoanEnd = new System.Windows.Forms.DateTimePicker();
		this.label11 = new System.Windows.Forms.Label();
		this.checkIsLoan = new System.Windows.Forms.CheckBox();
		this.dateJoiningDate = new System.Windows.Forms.DateTimePicker();
		this.label4 = new System.Windows.Forms.Label();
		this.listViewPlayingTeams = new System.Windows.Forms.ListView();
		this.imageListTeamLogos = new System.Windows.Forms.ImageList(this.components);
		this.comboClubTeam = new System.Windows.Forms.ComboBox();
		this.buttonCallNationalTeam = new System.Windows.Forms.Button();
		this.buttonRemoveNationalTeam = new System.Windows.Forms.Button();
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
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.labelPreferredPositions = new System.Windows.Forms.Label();
		this.comboPreferredPosition4 = new System.Windows.Forms.ComboBox();
		this.comboPreferredPosition3 = new System.Windows.Forms.ComboBox();
		this.comboPreferredPosition2 = new System.Windows.Forms.ComboBox();
		this.comboPreferredPosition1 = new System.Windows.Forms.ComboBox();
		this.domainInternationalReputation = new System.Windows.Forms.DomainUpDown();
		this.labelInternationalReputation = new System.Windows.Forms.Label();
		this.pageSkills = new System.Windows.Forms.TabPage();
		this.flowPanelSkills = new System.Windows.Forms.FlowLayoutPanel();
		this.groupGenerateAttributes = new System.Windows.Forms.GroupBox();
		this.labelOverallrating = new System.Windows.Forms.Label();
		this.trackOverallrating = new System.Windows.Forms.TrackBar();
		this.labelRandomize = new System.Windows.Forms.Label();
		this.numericRandomize = new System.Windows.Forms.NumericUpDown();
		this.buttonRandomAboveAvg = new System.Windows.Forms.Button();
		this.buttonRandomBelowAvg = new System.Windows.Forms.Button();
		this.buttonRandomSuperstar = new System.Windows.Forms.Button();
		this.buttonRandomVeryGood = new System.Windows.Forms.Button();
		this.buttonRandomGood = new System.Windows.Forms.Button();
		this.buttonRandomAverage = new System.Windows.Forms.Button();
		this.buttonRandomPoor = new System.Windows.Forms.Button();
		this.groupGoalkeperSkills = new System.Windows.Forms.GroupBox();
		this.label5 = new System.Windows.Forms.Label();
		this.comboGkSaveStyle = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.labelGkKick = new System.Windows.Forms.Label();
		this.comboGkKickStyle = new System.Windows.Forms.ComboBox();
		this.trackGkKicking = new System.Windows.Forms.TrackBar();
		this.labelDiving = new System.Windows.Forms.Label();
		this.labelPositioning = new System.Windows.Forms.Label();
		this.labelReflexes = new System.Windows.Forms.Label();
		this.labelHandling = new System.Windows.Forms.Label();
		this.trackDiving = new System.Windows.Forms.TrackBar();
		this.trackPositioning = new System.Windows.Forms.TrackBar();
		this.trackReflexes = new System.Windows.Forms.TrackBar();
		this.trackHandling = new System.Windows.Forms.TrackBar();
		this.numericGoalkeeperSkills = new System.Windows.Forms.NumericUpDown();
		this.groupDefensiveSkills = new System.Windows.Forms.GroupBox();
		this.labelInterception = new System.Windows.Forms.Label();
		this.trackInterception = new System.Windows.Forms.TrackBar();
		this.labelSliding = new System.Windows.Forms.Label();
		this.trackSliding = new System.Windows.Forms.TrackBar();
		this.numericDefensiveSkills = new System.Windows.Forms.NumericUpDown();
		this.labelAggression = new System.Windows.Forms.Label();
		this.labelMarking = new System.Windows.Forms.Label();
		this.labelTackling = new System.Windows.Forms.Label();
		this.trackTackling = new System.Windows.Forms.TrackBar();
		this.trackMarking = new System.Windows.Forms.TrackBar();
		this.trackAggression = new System.Windows.Forms.TrackBar();
		this.groupMidfielderSkills = new System.Windows.Forms.GroupBox();
		this.labelCurve = new System.Windows.Forms.Label();
		this.trackCurve = new System.Windows.Forms.TrackBar();
		this.labelVision = new System.Windows.Forms.Label();
		this.trackVision = new System.Windows.Forms.TrackBar();
		this.numericMidfielderSkills = new System.Windows.Forms.NumericUpDown();
		this.labelBallControl = new System.Windows.Forms.Label();
		this.labelCrossing = new System.Windows.Forms.Label();
		this.labelLongPassing = new System.Windows.Forms.Label();
		this.trackLongPassing = new System.Windows.Forms.TrackBar();
		this.labelShortPassing = new System.Windows.Forms.Label();
		this.trackShortPassing = new System.Windows.Forms.TrackBar();
		this.trackBallControl = new System.Windows.Forms.TrackBar();
		this.trackCrossing = new System.Windows.Forms.TrackBar();
		this.groupMental = new System.Windows.Forms.GroupBox();
		this.label14 = new System.Windows.Forms.Label();
		this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
		this.comboDefensiveWorkrate = new System.Windows.Forms.ComboBox();
		this.label10 = new System.Windows.Forms.Label();
		this.comboAttackWorkRate = new System.Windows.Forms.ComboBox();
		this.label9 = new System.Windows.Forms.Label();
		this.numericMentalSkills = new System.Windows.Forms.NumericUpDown();
		this.labelPlayerPositioning = new System.Windows.Forms.Label();
		this.labelPotential = new System.Windows.Forms.Label();
		this.trackPlayerPositioning = new System.Windows.Forms.TrackBar();
		this.trackPotential = new System.Windows.Forms.TrackBar();
		this.groupAttackingSkills = new System.Windows.Forms.GroupBox();
		this.labelFinishing = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
		this.labelHeading = new System.Windows.Forms.Label();
		this.trackHeading = new System.Windows.Forms.TrackBar();
		this.labelVolley = new System.Windows.Forms.Label();
		this.trackVolley = new System.Windows.Forms.TrackBar();
		this.numericAttackingSkills = new System.Windows.Forms.NumericUpDown();
		this.labelDribbling = new System.Windows.Forms.Label();
		this.labelLongShot = new System.Windows.Forms.Label();
		this.labelShotPower = new System.Windows.Forms.Label();
		this.trackFinishing = new System.Windows.Forms.TrackBar();
		this.trackShotPower = new System.Windows.Forms.TrackBar();
		this.trackLongShot = new System.Windows.Forms.TrackBar();
		this.trackDribbling = new System.Windows.Forms.TrackBar();
		this.groupGenericAttributes = new System.Windows.Forms.GroupBox();
		this.label7 = new System.Windows.Forms.Label();
		this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
		this.labelJumping = new System.Windows.Forms.Label();
		this.labelBalance = new System.Windows.Forms.Label();
		this.trackBalance = new System.Windows.Forms.TrackBar();
		this.labelAgility = new System.Windows.Forms.Label();
		this.trackAgility = new System.Windows.Forms.TrackBar();
		this.numericPhysicalSkills = new System.Windows.Forms.NumericUpDown();
		this.labelReactions = new System.Windows.Forms.Label();
		this.labelStrength = new System.Windows.Forms.Label();
		this.labelStamina = new System.Windows.Forms.Label();
		this.trackStamina = new System.Windows.Forms.TrackBar();
		this.labelSprintSpeed = new System.Windows.Forms.Label();
		this.trackSprintSpeed = new System.Windows.Forms.TrackBar();
		this.labelAcceleration = new System.Windows.Forms.Label();
		this.trackAcceleration = new System.Windows.Forms.TrackBar();
		this.trackStrength = new System.Windows.Forms.TrackBar();
		this.trackReactions = new System.Windows.Forms.TrackBar();
		this.trackJumping = new System.Windows.Forms.TrackBar();
		this.groupFreeKick = new System.Windows.Forms.GroupBox();
		this.labelSkillsStars = new System.Windows.Forms.Label();
		this.imageListStars = new System.Windows.Forms.ImageList(this.components);
		this.numericSkillMoves = new System.Windows.Forms.NumericUpDown();
		this.labelSkillMoves = new System.Windows.Forms.Label();
		this.numericFreeKickSkills = new System.Windows.Forms.NumericUpDown();
		this.labelPenalties = new System.Windows.Forms.Label();
		this.labelFreeKick = new System.Windows.Forms.Label();
		this.trackFreeKick = new System.Windows.Forms.TrackBar();
		this.trackPenalties = new System.Windows.Forms.TrackBar();
		this.labelPenaltyKick = new System.Windows.Forms.Label();
		this.comboPenaltyKick = new System.Windows.Forms.ComboBox();
		this.labelPenaltyMove = new System.Windows.Forms.Label();
		this.comboPenaltyMove = new System.Windows.Forms.ComboBox();
		this.labelFreeKickStart = new System.Windows.Forms.Label();
		this.labelPenaltyStart = new System.Windows.Forms.Label();
		this.comboFreeKickStart = new System.Windows.Forms.ComboBox();
		this.comboPenaltyStart = new System.Windows.Forms.ComboBox();
		this.groupTraits = new System.Windows.Forms.GroupBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.checkTechDribbler = new System.Windows.Forms.CheckBox();
		this.checkChipShot = new System.Windows.Forms.CheckBox();
		this.checkGKFlatKick = new System.Windows.Forms.CheckBox();
		this.checkDrivenPass = new System.Windows.Forms.CheckBox();
		this.checkDivingHeader = new System.Windows.Forms.CheckBox();
		this.checkBycicleKick = new System.Windows.Forms.CheckBox();
		this.checkChipperPenalty = new System.Windows.Forms.CheckBox();
		this.checkStutterPenalty = new System.Windows.Forms.CheckBox();
		this.checkFancyFlicks = new System.Windows.Forms.CheckBox();
		this.checkFancyPasses = new System.Windows.Forms.CheckBox();
		this.checkFancyFeet = new System.Windows.Forms.CheckBox();
		this.checkGKOneonOne = new System.Windows.Forms.CheckBox();
		this.checkAcrobaticClearance = new System.Windows.Forms.CheckBox();
		this.checkSecondWind = new System.Windows.Forms.CheckBox();
		this.checkCrowdFavourite = new System.Windows.Forms.CheckBox();
		this.checkInflexible = new System.Windows.Forms.CheckBox();
		this.checkTeamPlayer = new System.Windows.Forms.CheckBox();
		this.checkSwervePasser = new System.Windows.Forms.CheckBox();
		this.checkCornerSpecialist = new System.Windows.Forms.CheckBox();
		this.checkPowerHeader = new System.Windows.Forms.CheckBox();
		this.checkGkLongThrower = new System.Windows.Forms.CheckBox();
		this.checkLongPasser = new System.Windows.Forms.CheckBox();
		this.checkFlair = new System.Windows.Forms.CheckBox();
		this.checkFinesseShot = new System.Windows.Forms.CheckBox();
		this.checkArguesWithOfficials = new System.Windows.Forms.CheckBox();
		this.checkBeatsOffsideTrap = new System.Windows.Forms.CheckBox();
		this.checkAvoidsWeakFoot = new System.Windows.Forms.CheckBox();
		this.checkInjuryFree = new System.Windows.Forms.CheckBox();
		this.checkPowerFreeKick = new System.Windows.Forms.CheckBox();
		this.checkSelfish = new System.Windows.Forms.CheckBox();
		this.checkPlaymaker = new System.Windows.Forms.CheckBox();
		this.checkSpeedDribbler = new System.Windows.Forms.CheckBox();
		this.checkLeadership = new System.Windows.Forms.CheckBox();
		this.checkPuncher = new System.Windows.Forms.CheckBox();
		this.checkDiver = new System.Windows.Forms.CheckBox();
		this.checkDivesintotackles = new System.Windows.Forms.CheckBox();
		this.checkLongshottaker = new System.Windows.Forms.CheckBox();
		this.checkHighClubIdentification = new System.Windows.Forms.CheckBox();
		this.checkPushesupforcorners = new System.Windows.Forms.CheckBox();
		this.checkEarlycrosser = new System.Windows.Forms.CheckBox();
		this.checkInjuryProne = new System.Windows.Forms.CheckBox();
		this.checkGiantThrower = new System.Windows.Forms.CheckBox();
		this.checkLongThrower = new System.Windows.Forms.CheckBox();
		this.pageFace = new System.Windows.Forms.TabPage();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.splitContainer2 = new System.Windows.Forms.SplitContainer();
		this.tool3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.buttonSwitchRenderingMode = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImport3DHeadModel = new System.Windows.Forms.ToolStripButton();
		this.buttonExport3DHeadModel = new System.Windows.Forms.ToolStripButton();
		this.buttonRemove3DHeadModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImport3DHairModel = new System.Windows.Forms.ToolStripButton();
		this.buttonExport3DHairModel = new System.Windows.Forms.ToolStripButton();
		this.buttonRemoveHairModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonMoveHairAhead = new System.Windows.Forms.ToolStripButton();
		this.buttonMoveHairBack = new System.Windows.Forms.ToolStripButton();
		this.buttonMoveHairUp = new System.Windows.Forms.ToolStripButton();
		this.buttonMoveHairDown = new System.Windows.Forms.ToolStripButton();
		this.buttonMoveHairLeft = new System.Windows.Forms.ToolStripButton();
		this.buttonMoveHairRight = new System.Windows.Forms.ToolStripButton();
		this.buttonMakeHairCloser = new System.Windows.Forms.ToolStripButton();
		this.buttonMakeHairWider = new System.Windows.Forms.ToolStripButton();
		this.buttonSaveHair = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.toolPhoto = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonShowJesey = new System.Windows.Forms.ToolStripButton();
		this.groupGenericFace = new System.Windows.Forms.GroupBox();
		this.checkUsingRevMod = new System.Windows.Forms.CheckBox();
		this.viewer2DPlayerGui = new FifaControls.Viewer2D();
		this.groupGenericFaceType = new System.Windows.Forms.GroupBox();
		this.labelFacialHair = new System.Windows.Forms.Label();
		this.labelEyeBow = new System.Windows.Forms.Label();
		this.domainFacialHair = new System.Windows.Forms.ComboBox();
		this.comboEyeBow = new System.Windows.Forms.ComboBox();
		this.labelSkintype = new System.Windows.Forms.Label();
		this.comboSkintype = new System.Windows.Forms.ComboBox();
		this.comboFacialHairColor = new System.Windows.Forms.ComboBox();
		this.labelFacialHairColor = new System.Windows.Forms.Label();
		this.checkHasGenericFace = new System.Windows.Forms.CheckBox();
		this.groupHairModel = new System.Windows.Forms.GroupBox();
		this.comboFemaleHair = new System.Windows.Forms.ComboBox();
		this.radioButtonFemaleHair = new System.Windows.Forms.RadioButton();
		this.buttonHairSelection = new System.Windows.Forms.Button();
		this.comboHeadband = new System.Windows.Forms.ComboBox();
		this.comboAfro = new System.Windows.Forms.ComboBox();
		this.comboLong = new System.Windows.Forms.ComboBox();
		this.comboMedium = new System.Windows.Forms.ComboBox();
		this.comboModern = new System.Windows.Forms.ComboBox();
		this.labelHairColor = new System.Windows.Forms.Label();
		this.domainHairColor = new System.Windows.Forms.ComboBox();
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
		this.groupHeadModel = new System.Windows.Forms.GroupBox();
		this.radioButtonFemale = new System.Windows.Forms.RadioButton();
		this.comboFemaleModels = new System.Windows.Forms.ComboBox();
		this.comboLatinModels = new System.Windows.Forms.ComboBox();
		this.radioButtonLatin = new System.Windows.Forms.RadioButton();
		this.comboAsiaticModels = new System.Windows.Forms.ComboBox();
		this.radioButtonAsiatic = new System.Windows.Forms.RadioButton();
		this.comboAfricanModels = new System.Windows.Forms.ComboBox();
		this.radioButtonAfrican = new System.Windows.Forms.RadioButton();
		this.radioButtonCaucasic = new System.Windows.Forms.RadioButton();
		this.comboCaucasicModels = new System.Windows.Forms.ComboBox();
		this.buttonRandomizeAppearance = new System.Windows.Forms.Button();
		this.labelSideburns = new System.Windows.Forms.Label();
		this.comboSideburns = new System.Windows.Forms.ComboBox();
		this.labelHeadType = new System.Windows.Forms.Label();
		this.labelHairType = new System.Windows.Forms.Label();
		this.splitContainer3 = new System.Windows.Forms.SplitContainer();
		this.groupSpecifiHeadControls = new System.Windows.Forms.GroupBox();
		this.viewer2DTattoos = new FifaControls.Viewer2D();
		this.checkHighQaualityFace = new System.Windows.Forms.CheckBox();
		this.multiViewerFace = new FifaControls.MultiViewer2D();
		this.groupCommonHeadControls = new System.Windows.Forms.GroupBox();
		this.comboFaceposer = new System.Windows.Forms.ComboBox();
		this.label13 = new System.Windows.Forms.Label();
		this.buttonRgbHair = new System.Windows.Forms.Button();
		this.multiViewerHair = new FifaControls.MultiViewer2D();
		this.viewer2DEyeTexture = new FifaControls.Viewer2D();
		this.viewer2DSkinTexture = new FifaControls.Viewer2D();
		this.label1 = new System.Windows.Forms.Label();
		this.labelSkinColorInfo = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.comboEyescolor = new System.Windows.Forms.ComboBox();
		this.numericSkinTone = new System.Windows.Forms.NumericUpDown();
		this.checkShowTexures = new System.Windows.Forms.CheckBox();
		this.imageListTabIcons = new System.Windows.Forms.ImageList(this.components);
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.pickUpControl = new FifaControls.PickUpControl();
		this.tabEditPlayer.SuspendLayout();
		this.pageInfo.SuspendLayout();
		this.flowPanelInfo.SuspendLayout();
		this.groupPlayerIdentity.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.playerBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPlayerId).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).BeginInit();
		this.groupBoxBody.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericHeight).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericWeight).BeginInit();
		this.groupBoxLook.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericGkGloves).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc1).BeginInit();
		this.groupPlayFirTeam.SuspendLayout();
		this.groupIsLoan.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).BeginInit();
		this.groupShoes.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesBrand).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesDesign).BeginInit();
		this.groupBox1.SuspendLayout();
		this.pageSkills.SuspendLayout();
		this.flowPanelSkills.SuspendLayout();
		this.groupGenerateAttributes.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackOverallrating).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericRandomize).BeginInit();
		this.groupGoalkeperSkills.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGkKicking).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackDiving).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackPositioning).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackReflexes).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackHandling).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericGoalkeeperSkills).BeginInit();
		this.groupDefensiveSkills.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackInterception).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackSliding).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericDefensiveSkills).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackTackling).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackMarking).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackAggression).BeginInit();
		this.groupMidfielderSkills.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackCurve).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackVision).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericMidfielderSkills).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackLongPassing).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackShortPassing).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBallControl).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackCrossing).BeginInit();
		this.groupMental.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericMentalSkills).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackPlayerPositioning).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackPotential).BeginInit();
		this.groupAttackingSkills.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackHeading).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackVolley).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericAttackingSkills).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackFinishing).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackShotPower).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackLongShot).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackDribbling).BeginInit();
		this.groupGenericAttributes.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBalance).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackAgility).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPhysicalSkills).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackStamina).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackSprintSpeed).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackAcceleration).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackStrength).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackReactions).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackJumping).BeginInit();
		this.groupFreeKick.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericSkillMoves).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericFreeKickSkills).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackFreeKick).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackPenalties).BeginInit();
		this.groupTraits.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.pageFace.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).BeginInit();
		this.splitContainer2.Panel1.SuspendLayout();
		this.splitContainer2.Panel2.SuspendLayout();
		this.splitContainer2.SuspendLayout();
		this.tool3D.SuspendLayout();
		this.groupGenericFace.SuspendLayout();
		this.groupGenericFaceType.SuspendLayout();
		this.groupHairModel.SuspendLayout();
		this.groupHeadModel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer3).BeginInit();
		this.splitContainer3.Panel1.SuspendLayout();
		this.splitContainer3.SuspendLayout();
		this.groupSpecifiHeadControls.SuspendLayout();
		this.groupCommonHeadControls.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericSkinTone).BeginInit();
		base.SuspendLayout();
		this.tabEditPlayer.Controls.Add(this.pageInfo);
		this.tabEditPlayer.Controls.Add(this.pageSkills);
		this.tabEditPlayer.Controls.Add(this.pageFace);
		this.tabEditPlayer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabEditPlayer.ImageList = this.imageListTabIcons;
		this.tabEditPlayer.Location = new System.Drawing.Point(0, 25);
		this.tabEditPlayer.Name = "tabEditPlayer";
		this.tabEditPlayer.SelectedIndex = 0;
		this.tabEditPlayer.Size = new System.Drawing.Size(1357, 807);
		this.tabEditPlayer.TabIndex = 1;
		this.tabEditPlayer.SelectedIndexChanged += new System.EventHandler(tabEditPlayer_SelectedIndexChanged);
		this.pageInfo.Controls.Add(this.flowPanelInfo);
		this.pageInfo.ImageIndex = 0;
		this.pageInfo.Location = new System.Drawing.Point(4, 23);
		this.pageInfo.Name = "pageInfo";
		this.pageInfo.Padding = new System.Windows.Forms.Padding(3);
		this.pageInfo.Size = new System.Drawing.Size(1349, 780);
		this.pageInfo.TabIndex = 0;
		this.pageInfo.Text = "Info";
		this.pageInfo.UseVisualStyleBackColor = true;
		this.flowPanelInfo.AutoScroll = true;
		this.flowPanelInfo.Controls.Add(this.groupPlayerIdentity);
		this.flowPanelInfo.Controls.Add(this.groupBoxBody);
		this.flowPanelInfo.Controls.Add(this.groupBoxLook);
		this.flowPanelInfo.Controls.Add(this.groupPlayFirTeam);
		this.flowPanelInfo.Controls.Add(this.groupShoes);
		this.flowPanelInfo.Controls.Add(this.groupBox1);
		this.flowPanelInfo.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowPanelInfo.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
		this.flowPanelInfo.Location = new System.Drawing.Point(3, 3);
		this.flowPanelInfo.Name = "flowPanelInfo";
		this.flowPanelInfo.Size = new System.Drawing.Size(1343, 774);
		this.flowPanelInfo.TabIndex = 0;
		this.groupPlayerIdentity.Controls.Add(this.radioButtonGenderFemale);
		this.groupPlayerIdentity.Controls.Add(this.radioButtonGenderMale);
		this.groupPlayerIdentity.Controls.Add(this.labelCommonName);
		this.groupPlayerIdentity.Controls.Add(this.textCommonName);
		this.groupPlayerIdentity.Controls.Add(this.textJerseyName);
		this.groupPlayerIdentity.Controls.Add(this.labelJerseyName);
		this.groupPlayerIdentity.Controls.Add(this.buttonGetId);
		this.groupPlayerIdentity.Controls.Add(this.viewer2DPhoto);
		this.groupPlayerIdentity.Controls.Add(this.numericPlayerId);
		this.groupPlayerIdentity.Controls.Add(this.buttonRandomizeIdentity);
		this.groupPlayerIdentity.Controls.Add(this.dateBirthDate);
		this.groupPlayerIdentity.Controls.Add(this.labelBirthdate);
		this.groupPlayerIdentity.Controls.Add(this.labelPlayerId);
		this.groupPlayerIdentity.Controls.Add(this.textSurname);
		this.groupPlayerIdentity.Controls.Add(this.textFirstName);
		this.groupPlayerIdentity.Controls.Add(this.comboCountry);
		this.groupPlayerIdentity.Controls.Add(this.labelFirstName);
		this.groupPlayerIdentity.Controls.Add(this.labelSurame);
		this.groupPlayerIdentity.Controls.Add(this.labelCountry);
		this.groupPlayerIdentity.Location = new System.Drawing.Point(3, 3);
		this.groupPlayerIdentity.Name = "groupPlayerIdentity";
		this.groupPlayerIdentity.Size = new System.Drawing.Size(391, 239);
		this.groupPlayerIdentity.TabIndex = 85;
		this.groupPlayerIdentity.TabStop = false;
		this.groupPlayerIdentity.Text = "Identity Card";
		this.radioButtonGenderFemale.AutoSize = true;
		this.radioButtonGenderFemale.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Female", true));
		this.radioButtonGenderFemale.Location = new System.Drawing.Point(314, 206);
		this.radioButtonGenderFemale.Name = "radioButtonGenderFemale";
		this.radioButtonGenderFemale.Size = new System.Drawing.Size(59, 17);
		this.radioButtonGenderFemale.TabIndex = 163;
		this.radioButtonGenderFemale.TabStop = true;
		this.radioButtonGenderFemale.Text = "Female";
		this.radioButtonGenderFemale.UseVisualStyleBackColor = true;
		this.playerBindingSource.DataSource = typeof(FifaLibrary.Player);
		this.radioButtonGenderMale.AutoSize = true;
		this.radioButtonGenderMale.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Male", true));
		this.radioButtonGenderMale.Location = new System.Drawing.Point(260, 206);
		this.radioButtonGenderMale.Name = "radioButtonGenderMale";
		this.radioButtonGenderMale.Size = new System.Drawing.Size(48, 17);
		this.radioButtonGenderMale.TabIndex = 162;
		this.radioButtonGenderMale.TabStop = true;
		this.radioButtonGenderMale.Text = "Male";
		this.radioButtonGenderMale.UseVisualStyleBackColor = true;
		this.labelCommonName.AutoSize = true;
		this.labelCommonName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCommonName.Location = new System.Drawing.Point(156, 99);
		this.labelCommonName.Name = "labelCommonName";
		this.labelCommonName.Size = new System.Drawing.Size(79, 13);
		this.labelCommonName.TabIndex = 161;
		this.labelCommonName.Text = "Common Name";
		this.labelCommonName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textCommonName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.playerBindingSource, "commonname", true));
		this.textCommonName.Location = new System.Drawing.Point(247, 96);
		this.textCommonName.Name = "textCommonName";
		this.textCommonName.Size = new System.Drawing.Size(131, 20);
		this.textCommonName.TabIndex = 2;
		this.textCommonName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textCommonName.TextChanged += new System.EventHandler(textCommonName_TextChanged);
		this.textJerseyName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.playerBindingSource, "playerjerseyname", true));
		this.textJerseyName.Location = new System.Drawing.Point(247, 122);
		this.textJerseyName.Name = "textJerseyName";
		this.textJerseyName.Size = new System.Drawing.Size(131, 20);
		this.textJerseyName.TabIndex = 3;
		this.textJerseyName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textJerseyName.TextChanged += new System.EventHandler(textJerseyName_TextChanged);
		this.labelJerseyName.AutoSize = true;
		this.labelJerseyName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelJerseyName.Location = new System.Drawing.Point(156, 125);
		this.labelJerseyName.Name = "labelJerseyName";
		this.labelJerseyName.Size = new System.Drawing.Size(37, 13);
		this.labelJerseyName.TabIndex = 159;
		this.labelJerseyName.Text = "Jersey";
		this.labelJerseyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonGetId.Image = (System.Drawing.Image)resources.GetObject("buttonGetId.Image");
		this.buttonGetId.Location = new System.Drawing.Point(354, 19);
		this.buttonGetId.Name = "buttonGetId";
		this.buttonGetId.Size = new System.Drawing.Size(24, 20);
		this.buttonGetId.TabIndex = 156;
		this.buttonGetId.TabStop = false;
		this.buttonGetId.UseVisualStyleBackColor = true;
		this.buttonGetId.Click += new System.EventHandler(buttonGetId_Click);
		this.viewer2DPhoto.AutoTransparency = true;
		this.viewer2DPhoto.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPhoto.ButtonStripVisible = false;
		this.viewer2DPhoto.CurrentBitmap = null;
		this.viewer2DPhoto.ExtendedFormat = false;
		this.viewer2DPhoto.FullSizeButton = false;
		this.viewer2DPhoto.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DPhoto.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DPhoto.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.MiniFace;
		this.viewer2DPhoto.Location = new System.Drawing.Point(6, 16);
		this.viewer2DPhoto.Name = "viewer2DPhoto";
		this.viewer2DPhoto.RemoveButton = false;
		this.viewer2DPhoto.ShowButton = false;
		this.viewer2DPhoto.ShowButtonChecked = true;
		this.viewer2DPhoto.Size = new System.Drawing.Size(128, 153);
		this.viewer2DPhoto.TabIndex = 125;
		this.viewer2DPhoto.TabStop = false;
		this.numericPlayerId.Location = new System.Drawing.Point(248, 19);
		this.numericPlayerId.Maximum = new decimal(new int[4] { 524287, 0, 0, 0 });
		this.numericPlayerId.Name = "numericPlayerId";
		this.numericPlayerId.Size = new System.Drawing.Size(91, 20);
		this.numericPlayerId.TabIndex = 154;
		this.numericPlayerId.TabStop = false;
		this.numericPlayerId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPlayerId.Value = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericPlayerId.ValueChanged += new System.EventHandler(numericPlayerId_ValueChanged);
		this.buttonRandomizeIdentity.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomizeIdentity.Location = new System.Drawing.Point(6, 177);
		this.buttonRandomizeIdentity.Name = "buttonRandomizeIdentity";
		this.buttonRandomizeIdentity.Size = new System.Drawing.Size(128, 23);
		this.buttonRandomizeIdentity.TabIndex = 124;
		this.buttonRandomizeIdentity.TabStop = false;
		this.buttonRandomizeIdentity.Text = "Randomize";
		this.buttonRandomizeIdentity.UseVisualStyleBackColor = true;
		this.buttonRandomizeIdentity.Click += new System.EventHandler(buttonRandomizeIdentity_Click);
		this.dateBirthDate.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "birthdate", true));
		this.dateBirthDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateBirthDate.Location = new System.Drawing.Point(247, 153);
		this.dateBirthDate.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
		this.dateBirthDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateBirthDate.Name = "dateBirthDate";
		this.dateBirthDate.Size = new System.Drawing.Size(131, 20);
		this.dateBirthDate.TabIndex = 4;
		this.dateBirthDate.Value = new System.DateTime(2006, 12, 31, 0, 0, 0, 0);
		this.labelBirthdate.AutoSize = true;
		this.labelBirthdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBirthdate.Location = new System.Drawing.Point(156, 156);
		this.labelBirthdate.Name = "labelBirthdate";
		this.labelBirthdate.Size = new System.Drawing.Size(49, 13);
		this.labelBirthdate.TabIndex = 4;
		this.labelBirthdate.Text = "Birthdate";
		this.labelBirthdate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelPlayerId.AutoSize = true;
		this.labelPlayerId.BackColor = System.Drawing.Color.Transparent;
		this.labelPlayerId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPlayerId.Location = new System.Drawing.Point(156, 23);
		this.labelPlayerId.Name = "labelPlayerId";
		this.labelPlayerId.Size = new System.Drawing.Size(48, 13);
		this.labelPlayerId.TabIndex = 122;
		this.labelPlayerId.Text = "Player Id";
		this.labelPlayerId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textSurname.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.playerBindingSource, "lastname", true));
		this.textSurname.Location = new System.Drawing.Point(247, 70);
		this.textSurname.Name = "textSurname";
		this.textSurname.Size = new System.Drawing.Size(131, 20);
		this.textSurname.TabIndex = 1;
		this.textSurname.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textSurname.TextChanged += new System.EventHandler(textSurname_TextChanged);
		this.textFirstName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.playerBindingSource, "firstname", true));
		this.textFirstName.Location = new System.Drawing.Point(248, 44);
		this.textFirstName.Name = "textFirstName";
		this.textFirstName.Size = new System.Drawing.Size(131, 20);
		this.textFirstName.TabIndex = 0;
		this.textFirstName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textFirstName.TextChanged += new System.EventHandler(textFirstName_TextChanged);
		this.comboCountry.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.playerBindingSource, "Country", true));
		this.comboCountry.DataSource = this.countryListBindingSource;
		this.comboCountry.ItemHeight = 13;
		this.comboCountry.Location = new System.Drawing.Point(247, 179);
		this.comboCountry.MaxLength = 32767;
		this.comboCountry.Name = "comboCountry";
		this.comboCountry.Size = new System.Drawing.Size(131, 21);
		this.comboCountry.TabIndex = 5;
		this.countryListBindingSource.DataSource = typeof(FifaLibrary.CountryList);
		this.labelFirstName.AutoSize = true;
		this.labelFirstName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFirstName.Location = new System.Drawing.Point(156, 47);
		this.labelFirstName.Name = "labelFirstName";
		this.labelFirstName.Size = new System.Drawing.Size(57, 13);
		this.labelFirstName.TabIndex = 1;
		this.labelFirstName.Text = "First Name";
		this.labelFirstName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelSurame.AutoSize = true;
		this.labelSurame.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSurame.Location = new System.Drawing.Point(156, 73);
		this.labelSurame.Name = "labelSurame";
		this.labelSurame.Size = new System.Drawing.Size(58, 13);
		this.labelSurame.TabIndex = 2;
		this.labelSurame.Text = "Last Name";
		this.labelSurame.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCountry.AutoSize = true;
		this.labelCountry.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelCountry.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelCountry.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCountry.Location = new System.Drawing.Point(156, 182);
		this.labelCountry.Name = "labelCountry";
		this.labelCountry.Size = new System.Drawing.Size(43, 13);
		this.labelCountry.TabIndex = 5;
		this.labelCountry.Text = "Country";
		this.labelCountry.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCountry.DoubleClick += new System.EventHandler(labelCountry_DoubleClick);
		this.groupBoxBody.Controls.Add(this.comboWeakFoot);
		this.groupBoxBody.Controls.Add(this.labelWeakFoot);
		this.groupBoxBody.Controls.Add(this.comboBody);
		this.groupBoxBody.Controls.Add(this.numericHeight);
		this.groupBoxBody.Controls.Add(this.numericWeight);
		this.groupBoxBody.Controls.Add(this.labelWeight);
		this.groupBoxBody.Controls.Add(this.labelBody);
		this.groupBoxBody.Controls.Add(this.domainPreferredFoot);
		this.groupBoxBody.Controls.Add(this.labelHeight);
		this.groupBoxBody.Controls.Add(this.labelPreferredFoot);
		this.groupBoxBody.Location = new System.Drawing.Point(3, 248);
		this.groupBoxBody.Name = "groupBoxBody";
		this.groupBoxBody.Size = new System.Drawing.Size(391, 113);
		this.groupBoxBody.TabIndex = 86;
		this.groupBoxBody.TabStop = false;
		this.groupBoxBody.Text = "Body";
		this.comboWeakFoot.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "weakfootabilitytypecode", true));
		this.comboWeakFoot.FormattingEnabled = true;
		this.comboWeakFoot.Items.AddRange(new object[5] { "Very Poor", "Poor", "Medium", "Good", "Ambidexter" });
		this.comboWeakFoot.Location = new System.Drawing.Point(247, 76);
		this.comboWeakFoot.Name = "comboWeakFoot";
		this.comboWeakFoot.Size = new System.Drawing.Size(103, 21);
		this.comboWeakFoot.TabIndex = 3;
		this.labelWeakFoot.AutoSize = true;
		this.labelWeakFoot.BackColor = System.Drawing.Color.Transparent;
		this.labelWeakFoot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelWeakFoot.Location = new System.Drawing.Point(184, 79);
		this.labelWeakFoot.Name = "labelWeakFoot";
		this.labelWeakFoot.Size = new System.Drawing.Size(57, 13);
		this.labelWeakFoot.TabIndex = 54;
		this.labelWeakFoot.Text = "Weak foot";
		this.labelWeakFoot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboBody.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "bodytypecode", true));
		this.comboBody.FormattingEnabled = true;
		this.comboBody.Items.AddRange(new object[18]
		{
			"Average Height and Lean", "Average Height ", "Average Height and Muscular", "Tall and Lean", "Tall", "Tall and Muscular", "Short and Lean", "Short ", "Short and Muscular", "10 Messi",
			"Very Tall and Lean", "12 Akinfenwa", "13 Courtois", "14 Neymar", "15 Shaqiri", "16 Ronaldo", "17 Unused", "18 Leroux"
		});
		this.comboBody.Location = new System.Drawing.Point(71, 46);
		this.comboBody.Name = "comboBody";
		this.comboBody.Size = new System.Drawing.Size(279, 21);
		this.comboBody.TabIndex = 4;
		this.numericHeight.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "height", true));
		this.numericHeight.Location = new System.Drawing.Point(71, 20);
		this.numericHeight.Maximum = new decimal(new int[4] { 215, 0, 0, 0 });
		this.numericHeight.Minimum = new decimal(new int[4] { 150, 0, 0, 0 });
		this.numericHeight.Name = "numericHeight";
		this.numericHeight.Size = new System.Drawing.Size(103, 20);
		this.numericHeight.TabIndex = 0;
		this.numericHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericHeight.Value = new decimal(new int[4] { 150, 0, 0, 0 });
		this.numericWeight.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "weight", true));
		this.numericWeight.Location = new System.Drawing.Point(247, 20);
		this.numericWeight.Maximum = new decimal(new int[4] { 115, 0, 0, 0 });
		this.numericWeight.Minimum = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericWeight.Name = "numericWeight";
		this.numericWeight.Size = new System.Drawing.Size(103, 20);
		this.numericWeight.TabIndex = 2;
		this.numericWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericWeight.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.labelWeight.AutoSize = true;
		this.labelWeight.BackColor = System.Drawing.Color.Transparent;
		this.labelWeight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelWeight.Location = new System.Drawing.Point(184, 23);
		this.labelWeight.Name = "labelWeight";
		this.labelWeight.Size = new System.Drawing.Size(41, 13);
		this.labelWeight.TabIndex = 12;
		this.labelWeight.Text = "Weight";
		this.labelWeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelBody.AutoSize = true;
		this.labelBody.BackColor = System.Drawing.Color.Transparent;
		this.labelBody.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBody.Location = new System.Drawing.Point(6, 48);
		this.labelBody.Name = "labelBody";
		this.labelBody.Size = new System.Drawing.Size(31, 13);
		this.labelBody.TabIndex = 26;
		this.labelBody.Text = "Body";
		this.labelBody.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.domainPreferredFoot.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "preferredfoot", true));
		this.domainPreferredFoot.Items.Add("Right");
		this.domainPreferredFoot.Items.Add("Left");
		this.domainPreferredFoot.Location = new System.Drawing.Point(71, 77);
		this.domainPreferredFoot.Name = "domainPreferredFoot";
		this.domainPreferredFoot.Size = new System.Drawing.Size(103, 20);
		this.domainPreferredFoot.TabIndex = 1;
		this.domainPreferredFoot.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.domainPreferredFoot.Wrap = true;
		this.labelHeight.AutoSize = true;
		this.labelHeight.BackColor = System.Drawing.Color.Transparent;
		this.labelHeight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHeight.Location = new System.Drawing.Point(6, 23);
		this.labelHeight.Name = "labelHeight";
		this.labelHeight.Size = new System.Drawing.Size(38, 13);
		this.labelHeight.TabIndex = 11;
		this.labelHeight.Text = "Height";
		this.labelHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelPreferredFoot.AutoSize = true;
		this.labelPreferredFoot.BackColor = System.Drawing.Color.Transparent;
		this.labelPreferredFoot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPreferredFoot.Location = new System.Drawing.Point(6, 79);
		this.labelPreferredFoot.Name = "labelPreferredFoot";
		this.labelPreferredFoot.Size = new System.Drawing.Size(49, 13);
		this.labelPreferredFoot.TabIndex = 49;
		this.labelPreferredFoot.Text = "Best foot";
		this.labelPreferredFoot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupBoxLook.Controls.Add(this.checkJerseyFit);
		this.groupBoxLook.Controls.Add(this.checkTrainingPants);
		this.groupBoxLook.Controls.Add(this.domainSocksStyle);
		this.groupBoxLook.Controls.Add(this.label8);
		this.groupBoxLook.Controls.Add(this.numericGkGloves);
		this.groupBoxLook.Controls.Add(this.labelGkGloves);
		this.groupBoxLook.Controls.Add(this.labelWinter);
		this.groupBoxLook.Controls.Add(this.comboWinterAccessories);
		this.groupBoxLook.Controls.Add(this.domainJerseyStyle);
		this.groupBoxLook.Controls.Add(this.domainSleeves);
		this.groupBoxLook.Controls.Add(this.pictureColorAcc2);
		this.groupBoxLook.Controls.Add(this.pictureColorAcc3);
		this.groupBoxLook.Controls.Add(this.pictureColorAcc4);
		this.groupBoxLook.Controls.Add(this.pictureColorAcc1);
		this.groupBoxLook.Controls.Add(this.domainAccessory4);
		this.groupBoxLook.Controls.Add(this.domainAccessory3);
		this.groupBoxLook.Controls.Add(this.domainAccessory2);
		this.groupBoxLook.Controls.Add(this.domainAccessory1);
		this.groupBoxLook.Controls.Add(this.labelSleeves);
		this.groupBoxLook.Controls.Add(this.labelJerseyStyle);
		this.groupBoxLook.Controls.Add(this.labelAccesories);
		this.groupBoxLook.Location = new System.Drawing.Point(3, 367);
		this.groupBoxLook.Name = "groupBoxLook";
		this.groupBoxLook.Size = new System.Drawing.Size(391, 280);
		this.groupBoxLook.TabIndex = 87;
		this.groupBoxLook.TabStop = false;
		this.groupBoxLook.Text = "Look";
		this.checkJerseyFit.AutoSize = true;
		this.checkJerseyFit.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "jerseyfit", true));
		this.checkJerseyFit.Location = new System.Drawing.Point(280, 18);
		this.checkJerseyFit.Name = "checkJerseyFit";
		this.checkJerseyFit.Size = new System.Drawing.Size(70, 17);
		this.checkJerseyFit.TabIndex = 99;
		this.checkJerseyFit.Text = "Jersey Fit";
		this.checkJerseyFit.UseVisualStyleBackColor = true;
		this.checkTrainingPants.AutoSize = true;
		this.checkTrainingPants.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "TrainingPants", true));
		this.checkTrainingPants.Location = new System.Drawing.Point(238, 131);
		this.checkTrainingPants.Name = "checkTrainingPants";
		this.checkTrainingPants.Size = new System.Drawing.Size(112, 17);
		this.checkTrainingPants.TabIndex = 98;
		this.checkTrainingPants.Text = "GK Training Pants";
		this.checkTrainingPants.UseVisualStyleBackColor = true;
		this.domainSocksStyle.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "socklengthcode", true));
		this.domainSocksStyle.Items.Add("Normal Socks");
		this.domainSocksStyle.Items.Add("Low Socks");
		this.domainSocksStyle.Items.Add("High Socks");
		this.domainSocksStyle.Location = new System.Drawing.Point(108, 70);
		this.domainSocksStyle.Name = "domainSocksStyle";
		this.domainSocksStyle.Size = new System.Drawing.Size(242, 20);
		this.domainSocksStyle.TabIndex = 68;
		this.domainSocksStyle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.domainSocksStyle.Wrap = true;
		this.label8.AutoSize = true;
		this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label8.Location = new System.Drawing.Point(6, 72);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(63, 13);
		this.label8.TabIndex = 69;
		this.label8.Text = "Socks Style";
		this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericGkGloves.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "gkglovetypecode", true));
		this.numericGkGloves.Location = new System.Drawing.Point(108, 130);
		this.numericGkGloves.Maximum = new decimal(new int[4] { 127, 0, 0, 0 });
		this.numericGkGloves.Name = "numericGkGloves";
		this.numericGkGloves.Size = new System.Drawing.Size(106, 20);
		this.numericGkGloves.TabIndex = 8;
		this.numericGkGloves.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericGkGloves.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.labelGkGloves.AutoSize = true;
		this.labelGkGloves.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelGkGloves.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelGkGloves.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelGkGloves.Location = new System.Drawing.Point(10, 132);
		this.labelGkGloves.Name = "labelGkGloves";
		this.labelGkGloves.Size = new System.Drawing.Size(58, 13);
		this.labelGkGloves.TabIndex = 67;
		this.labelGkGloves.Text = "GK Gloves";
		this.labelGkGloves.DoubleClick += new System.EventHandler(labelGkGloves_DoubleClick);
		this.labelWinter.AutoSize = true;
		this.labelWinter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelWinter.Location = new System.Drawing.Point(6, 101);
		this.labelWinter.Name = "labelWinter";
		this.labelWinter.Size = new System.Drawing.Size(98, 13);
		this.labelWinter.TabIndex = 64;
		this.labelWinter.Text = "Winter Accessories";
		this.labelWinter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboWinterAccessories.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "hasseasonaljersey", true));
		this.comboWinterAccessories.FormattingEnabled = true;
		this.comboWinterAccessories.Items.AddRange(new object[5] { "None", "Long Sleeves no underarmor stuff", "Long Sleeves with underarmor neck", "Short sleeves with underarmor arms no neck", "Short sleeves with underarmor arms and neck" });
		this.comboWinterAccessories.Location = new System.Drawing.Point(108, 98);
		this.comboWinterAccessories.Name = "comboWinterAccessories";
		this.comboWinterAccessories.Size = new System.Drawing.Size(242, 21);
		this.comboWinterAccessories.TabIndex = 2;
		this.domainJerseyStyle.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "jerseystylecode", true));
		this.domainJerseyStyle.Items.Add("Normal");
		this.domainJerseyStyle.Items.Add("Untucked");
		this.domainJerseyStyle.Location = new System.Drawing.Point(108, 17);
		this.domainJerseyStyle.Name = "domainJerseyStyle";
		this.domainJerseyStyle.Size = new System.Drawing.Size(164, 20);
		this.domainJerseyStyle.TabIndex = 1;
		this.domainJerseyStyle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.domainJerseyStyle.Wrap = true;
		this.domainSleeves.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "jerseysleevelengthcode", true));
		this.domainSleeves.Items.Add("Short Sleeves ");
		this.domainSleeves.Items.Add("Long Sleeves ");
		this.domainSleeves.Items.Add("Long Sleeves with underarmor neck");
		this.domainSleeves.Items.Add("Short sleeves with underarmor arms no neck");
		this.domainSleeves.Items.Add("Short sleeves with underarmor arms and neck");
		this.domainSleeves.Location = new System.Drawing.Point(108, 43);
		this.domainSleeves.Name = "domainSleeves";
		this.domainSleeves.Size = new System.Drawing.Size(242, 20);
		this.domainSleeves.TabIndex = 0;
		this.domainSleeves.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.domainSleeves.Wrap = true;
		this.pictureColorAcc2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureColorAcc2.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureColorAcc2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorAcc2.Location = new System.Drawing.Point(223, 201);
		this.pictureColorAcc2.Name = "pictureColorAcc2";
		this.pictureColorAcc2.Size = new System.Drawing.Size(20, 20);
		this.pictureColorAcc2.TabIndex = 55;
		this.pictureColorAcc2.TabStop = false;
		this.pictureColorAcc2.Click += new System.EventHandler(pictureColorAcc2_Click);
		this.pictureColorAcc3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureColorAcc3.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureColorAcc3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorAcc3.Location = new System.Drawing.Point(223, 227);
		this.pictureColorAcc3.Name = "pictureColorAcc3";
		this.pictureColorAcc3.Size = new System.Drawing.Size(20, 20);
		this.pictureColorAcc3.TabIndex = 56;
		this.pictureColorAcc3.TabStop = false;
		this.pictureColorAcc3.Click += new System.EventHandler(pictureColorAcc3_Click);
		this.pictureColorAcc4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureColorAcc4.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureColorAcc4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorAcc4.Location = new System.Drawing.Point(223, 253);
		this.pictureColorAcc4.Name = "pictureColorAcc4";
		this.pictureColorAcc4.Size = new System.Drawing.Size(20, 20);
		this.pictureColorAcc4.TabIndex = 57;
		this.pictureColorAcc4.TabStop = false;
		this.pictureColorAcc4.Click += new System.EventHandler(pictureColorAcc4_Click);
		this.pictureColorAcc1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureColorAcc1.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureColorAcc1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorAcc1.Location = new System.Drawing.Point(223, 174);
		this.pictureColorAcc1.Name = "pictureColorAcc1";
		this.pictureColorAcc1.Size = new System.Drawing.Size(20, 20);
		this.pictureColorAcc1.TabIndex = 45;
		this.pictureColorAcc1.TabStop = false;
		this.pictureColorAcc1.Click += new System.EventHandler(pictureColorAcc1_Click);
		this.domainAccessory4.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "accessorycode4", true));
		this.domainAccessory4.Items.AddRange(new object[28]
		{
			"None", "1 Unused", "2 Hearphone", "3 Unused", "4 Left watch", "5 Right watch", "6 Left hand tape", "7 Right hand tape", "8 Left wristle tape", "9 Right wristle tape",
			"10 Left knee tape", "11 Right knee tape", "12 Left knee tutor", "13 Right knee tutor", "14 Left ankle tape", "15 Right ankle tape", "16 Gloves", "17 Unused", "18 Unused", "19 Unused",
			"20 Unused", "21 Unused", "22 Left finger tape", "23 Right finger tape", "24 Left wide wristle tape", "25 Right wide wristle tape", "26 Left bracelet", "27 Right bracelet"
		});
		this.domainAccessory4.Location = new System.Drawing.Point(12, 252);
		this.domainAccessory4.Name = "domainAccessory4";
		this.domainAccessory4.Size = new System.Drawing.Size(197, 21);
		this.domainAccessory4.TabIndex = 7;
		this.domainAccessory3.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "accessorycode3", true));
		this.domainAccessory3.Items.AddRange(new object[28]
		{
			"None", "1 Unused", "2 Hearphone", "3 Unused", "4 Left watch", "5 Right watch", "6 Left hand tape", "7 Right hand tape", "8 Left wristle tape", "9 Right wristle tape",
			"10 Left knee tape", "11 Right knee tape", "12 Left knee tutor", "13 Right knee tutor", "14 Left ankle tape", "15 Right ankle tape", "16 Gloves", "17 Unused", "18 Unused", "19 Unused",
			"20 Unused", "21 Unused", "22 Left finger tape", "23 Right finger tape", "24 Left wide wristle tape", "25 Right wide wristle tape", "26 Left bracelet", "27 Right bracelet"
		});
		this.domainAccessory3.Location = new System.Drawing.Point(12, 226);
		this.domainAccessory3.Name = "domainAccessory3";
		this.domainAccessory3.Size = new System.Drawing.Size(197, 21);
		this.domainAccessory3.TabIndex = 6;
		this.domainAccessory2.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "accessorycode2", true));
		this.domainAccessory2.Items.AddRange(new object[28]
		{
			"None", "1 Unused", "2 Hearphone", "3 Unused", "4 Left watch", "5 Right watch", "6 Left hand tape", "7 Right hand tape", "8 Left wristle tape", "9 Right wristle tape",
			"10 Left knee tape", "11 Right knee tape", "12 Left knee tutor", "13 Right knee tutor", "14 Left ankle tape", "15 Right ankle tape", "16 Gloves", "17 Unused", "18 Unused", "19 Unused",
			"20 Unused", "21 Unused", "22 Left finger tape", "23 Right finger tape", "24 Left wide wristle tape", "25 Right wide wristle tape", "26 Left bracelet", "27 Right bracelet"
		});
		this.domainAccessory2.Location = new System.Drawing.Point(12, 200);
		this.domainAccessory2.Name = "domainAccessory2";
		this.domainAccessory2.Size = new System.Drawing.Size(197, 21);
		this.domainAccessory2.TabIndex = 5;
		this.domainAccessory1.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "accessorycode1", true));
		this.domainAccessory1.Items.AddRange(new object[28]
		{
			"None", "1 Unused", "2 Hearphone", "3 Unused", "4 Left watch", "5 Right watch", "6 Left hand tape", "7 Right hand tape", "8 Left wristle tape", "9 Right wristle tape",
			"10 Left knee tape", "11 Right knee tape", "12 Left knee tutor", "13 Right knee tutor", "14 Left ankle tape", "15 Right ankle tape", "16 Gloves", "17 Unused", "18 Unused", "19 Unused",
			"20 Unused", "21 Unused", "22 Left finger tape", "23 Right finger tape", "24 Left wide wristle tape", "25 Right wide wristle tape", "26 Left bracelet", "27 Right bracelet"
		});
		this.domainAccessory1.Location = new System.Drawing.Point(12, 173);
		this.domainAccessory1.Name = "domainAccessory1";
		this.domainAccessory1.Size = new System.Drawing.Size(197, 21);
		this.domainAccessory1.TabIndex = 4;
		this.labelSleeves.AutoSize = true;
		this.labelSleeves.BackColor = System.Drawing.Color.Transparent;
		this.labelSleeves.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSleeves.Location = new System.Drawing.Point(5, 44);
		this.labelSleeves.Name = "labelSleeves";
		this.labelSleeves.Size = new System.Drawing.Size(81, 13);
		this.labelSleeves.TabIndex = 35;
		this.labelSleeves.Text = "Sleeves Length";
		this.labelSleeves.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelJerseyStyle.AutoSize = true;
		this.labelJerseyStyle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelJerseyStyle.Location = new System.Drawing.Point(5, 19);
		this.labelJerseyStyle.Name = "labelJerseyStyle";
		this.labelJerseyStyle.Size = new System.Drawing.Size(63, 13);
		this.labelJerseyStyle.TabIndex = 37;
		this.labelJerseyStyle.Text = "Jersey Style";
		this.labelJerseyStyle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelAccesories.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelAccesories.Location = new System.Drawing.Point(42, 154);
		this.labelAccesories.Name = "labelAccesories";
		this.labelAccesories.Size = new System.Drawing.Size(136, 20);
		this.labelAccesories.TabIndex = 39;
		this.labelAccesories.Text = "Accesories";
		this.labelAccesories.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.groupPlayFirTeam.Controls.Add(this.label15);
		this.groupPlayFirTeam.Controls.Add(this.groupIsLoan);
		this.groupPlayFirTeam.Controls.Add(this.checkIsLoan);
		this.groupPlayFirTeam.Controls.Add(this.dateJoiningDate);
		this.groupPlayFirTeam.Controls.Add(this.label4);
		this.groupPlayFirTeam.Controls.Add(this.listViewPlayingTeams);
		this.groupPlayFirTeam.Controls.Add(this.comboClubTeam);
		this.groupPlayFirTeam.Controls.Add(this.buttonCallNationalTeam);
		this.groupPlayFirTeam.Controls.Add(this.buttonRemoveNationalTeam);
		this.groupPlayFirTeam.Location = new System.Drawing.Point(400, 3);
		this.groupPlayFirTeam.Name = "groupPlayFirTeam";
		this.groupPlayFirTeam.Size = new System.Drawing.Size(243, 239);
		this.groupPlayFirTeam.TabIndex = 88;
		this.groupPlayFirTeam.TabStop = false;
		this.groupPlayFirTeam.Text = "Playing for";
		this.label15.AutoSize = true;
		this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label15.Location = new System.Drawing.Point(12, 210);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(78, 13);
		this.label15.TabIndex = 140;
		this.label15.Text = "Previous Team";
		this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label15.Visible = false;
		this.groupIsLoan.Controls.Add(this.comboTeamLoanedFrom);
		this.groupIsLoan.Controls.Add(this.label12);
		this.groupIsLoan.Controls.Add(this.dateLoanEnd);
		this.groupIsLoan.Controls.Add(this.label11);
		this.groupIsLoan.Location = new System.Drawing.Point(6, 139);
		this.groupIsLoan.Name = "groupIsLoan";
		this.groupIsLoan.Size = new System.Drawing.Size(231, 63);
		this.groupIsLoan.TabIndex = 139;
		this.groupIsLoan.TabStop = false;
		this.groupIsLoan.Visible = false;
		this.comboTeamLoanedFrom.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.playerBindingSource, "TeamLoanedFrom", true));
		this.comboTeamLoanedFrom.DataSource = this.teamListBindingSource;
		this.comboTeamLoanedFrom.ItemHeight = 13;
		this.comboTeamLoanedFrom.Location = new System.Drawing.Point(89, 37);
		this.comboTeamLoanedFrom.MaxLength = 32767;
		this.comboTeamLoanedFrom.Name = "comboTeamLoanedFrom";
		this.comboTeamLoanedFrom.Size = new System.Drawing.Size(131, 21);
		this.comboTeamLoanedFrom.TabIndex = 141;
		this.comboTeamLoanedFrom.SelectedIndexChanged += new System.EventHandler(comboTeamLoanedFrom_SelectedIndexChanged);
		this.teamListBindingSource.DataSource = typeof(FifaLibrary.TeamList);
		this.label12.AutoSize = true;
		this.label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label12.Location = new System.Drawing.Point(6, 40);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(69, 13);
		this.label12.TabIndex = 140;
		this.label12.Text = "Loaned From";
		this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dateLoanEnd.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "loandateend", true));
		this.dateLoanEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateLoanEnd.Location = new System.Drawing.Point(89, 11);
		this.dateLoanEnd.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
		this.dateLoanEnd.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateLoanEnd.Name = "dateLoanEnd";
		this.dateLoanEnd.Size = new System.Drawing.Size(131, 20);
		this.dateLoanEnd.TabIndex = 139;
		this.dateLoanEnd.Value = new System.DateTime(2026, 6, 30, 0, 0, 0, 0);
		this.label11.AutoSize = true;
		this.label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label11.Location = new System.Drawing.Point(6, 15);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(79, 13);
		this.label11.TabIndex = 138;
		this.label11.Text = "Loan End Date";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.checkIsLoan.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "IsLoaned", true));
		this.checkIsLoan.Location = new System.Drawing.Point(6, 124);
		this.checkIsLoan.Name = "checkIsLoan";
		this.checkIsLoan.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkIsLoan.Size = new System.Drawing.Size(104, 17);
		this.checkIsLoan.TabIndex = 138;
		this.checkIsLoan.Text = "Is Loaned ";
		this.checkIsLoan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkIsLoan.UseVisualStyleBackColor = true;
		this.checkIsLoan.CheckedChanged += new System.EventHandler(checkIsLoan_CheckedChanged);
		this.dateJoiningDate.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "joindate", true));
		this.dateJoiningDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateJoiningDate.Location = new System.Drawing.Point(95, 99);
		this.dateJoiningDate.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
		this.dateJoiningDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateJoiningDate.Name = "dateJoiningDate";
		this.dateJoiningDate.Size = new System.Drawing.Size(131, 20);
		this.dateJoiningDate.TabIndex = 137;
		this.dateJoiningDate.Value = new System.DateTime(2025, 7, 1, 0, 0, 0, 0);
		this.label4.AutoSize = true;
		this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label4.Location = new System.Drawing.Point(12, 103);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(66, 13);
		this.label4.TabIndex = 136;
		this.label4.Text = "Joining Date";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.listViewPlayingTeams.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewPlayingTeams.FullRowSelect = true;
		this.listViewPlayingTeams.GridLines = true;
		this.listViewPlayingTeams.HideSelection = false;
		this.listViewPlayingTeams.LargeImageList = this.imageListTeamLogos;
		this.listViewPlayingTeams.Location = new System.Drawing.Point(6, 19);
		this.listViewPlayingTeams.MultiSelect = false;
		this.listViewPlayingTeams.Name = "listViewPlayingTeams";
		this.listViewPlayingTeams.Size = new System.Drawing.Size(231, 71);
		this.listViewPlayingTeams.TabIndex = 135;
		this.listViewPlayingTeams.TabStop = false;
		this.listViewPlayingTeams.UseCompatibleStateImageBehavior = false;
		this.listViewPlayingTeams.DoubleClick += new System.EventHandler(listViewPlayingTeams_DoubleClick);
		this.imageListTeamLogos.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
		this.imageListTeamLogos.ImageSize = new System.Drawing.Size(32, 32);
		this.imageListTeamLogos.TransparentColor = System.Drawing.Color.Transparent;
		this.comboClubTeam.ItemHeight = 13;
		this.comboClubTeam.Location = new System.Drawing.Point(10, 334);
		this.comboClubTeam.MaxLength = 32767;
		this.comboClubTeam.Name = "comboClubTeam";
		this.comboClubTeam.Size = new System.Drawing.Size(100, 21);
		this.comboClubTeam.Sorted = true;
		this.comboClubTeam.TabIndex = 0;
		this.comboClubTeam.Visible = false;
		this.buttonCallNationalTeam.Enabled = false;
		this.buttonCallNationalTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonCallNationalTeam.Location = new System.Drawing.Point(130, 334);
		this.buttonCallNationalTeam.Name = "buttonCallNationalTeam";
		this.buttonCallNationalTeam.Size = new System.Drawing.Size(50, 20);
		this.buttonCallNationalTeam.TabIndex = 1;
		this.buttonCallNationalTeam.Text = "Call";
		this.buttonCallNationalTeam.Visible = false;
		this.buttonRemoveNationalTeam.Enabled = false;
		this.buttonRemoveNationalTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRemoveNationalTeam.Location = new System.Drawing.Point(180, 334);
		this.buttonRemoveNationalTeam.Name = "buttonRemoveNationalTeam";
		this.buttonRemoveNationalTeam.Size = new System.Drawing.Size(50, 20);
		this.buttonRemoveNationalTeam.TabIndex = 2;
		this.buttonRemoveNationalTeam.Text = "Remove";
		this.buttonRemoveNationalTeam.Visible = false;
		this.groupShoes.Controls.Add(this.label1ShoesType);
		this.groupShoes.Controls.Add(this.pictureColorShoes2);
		this.groupShoes.Controls.Add(this.pictureColorShoes1);
		this.groupShoes.Controls.Add(this.numericShoesBrand);
		this.groupShoes.Controls.Add(this.labelShoesType);
		this.groupShoes.Controls.Add(this.labelShoesColor);
		this.groupShoes.Controls.Add(this.numericShoesDesign);
		this.groupShoes.Controls.Add(this.viewer2DShoes);
		this.groupShoes.Controls.Add(this.labelShoes);
		this.groupShoes.Location = new System.Drawing.Point(400, 248);
		this.groupShoes.Name = "groupShoes";
		this.groupShoes.Size = new System.Drawing.Size(243, 178);
		this.groupShoes.TabIndex = 90;
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
		this.pictureColorShoes2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorShoes2.Location = new System.Drawing.Point(72, 131);
		this.pictureColorShoes2.Name = "pictureColorShoes2";
		this.pictureColorShoes2.Size = new System.Drawing.Size(20, 20);
		this.pictureColorShoes2.TabIndex = 63;
		this.pictureColorShoes2.TabStop = false;
		this.pictureColorShoes2.Click += new System.EventHandler(pictureColorShoes2_Click);
		this.pictureColorShoes1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureColorShoes1.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureColorShoes1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureColorShoes1.Location = new System.Drawing.Point(12, 131);
		this.pictureColorShoes1.Name = "pictureColorShoes1";
		this.pictureColorShoes1.Size = new System.Drawing.Size(20, 20);
		this.pictureColorShoes1.TabIndex = 62;
		this.pictureColorShoes1.TabStop = false;
		this.pictureColorShoes1.Click += new System.EventHandler(pictureColorShoes1_Click);
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
		this.groupBox1.Controls.Add(this.labelPreferredPositions);
		this.groupBox1.Controls.Add(this.comboPreferredPosition4);
		this.groupBox1.Controls.Add(this.comboPreferredPosition3);
		this.groupBox1.Controls.Add(this.comboPreferredPosition2);
		this.groupBox1.Controls.Add(this.comboPreferredPosition1);
		this.groupBox1.Controls.Add(this.domainInternationalReputation);
		this.groupBox1.Controls.Add(this.labelInternationalReputation);
		this.groupBox1.Location = new System.Drawing.Point(400, 432);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(243, 215);
		this.groupBox1.TabIndex = 89;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Playing Info";
		this.labelPreferredPositions.AutoSize = true;
		this.labelPreferredPositions.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPreferredPositions.Location = new System.Drawing.Point(66, 12);
		this.labelPreferredPositions.Name = "labelPreferredPositions";
		this.labelPreferredPositions.Size = new System.Drawing.Size(95, 13);
		this.labelPreferredPositions.TabIndex = 157;
		this.labelPreferredPositions.Text = "Preferred Positions";
		this.labelPreferredPositions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboPreferredPosition4.FormattingEnabled = true;
		this.comboPreferredPosition4.Items.AddRange(new object[29]
		{
			"None", "Goalkeeper", "Sweeper", "Right Wing Back", "Right Back", "Right Central Back", "Central Back", "Left Central Back", "Left Back", "Left Wing Back",
			"Right Defensive Midfielder", "Central Defensive Midfielder", "Left Defensive Midfielder", "Right Midfielder", "Right Central Midfielder", "Central Midfielder", "Left Central Midfielder", "Left Midfielder", "Right Advanced Midfielder", "Central Advanced Midfielder",
			"Left Advanced Midfielder", "Right Forward", "Central Forward", "Left Forward", "Right Wing", "Right Striker", "Central Striker", "Left Striker", "Left Wing"
		});
		this.comboPreferredPosition4.Location = new System.Drawing.Point(18, 121);
		this.comboPreferredPosition4.Name = "comboPreferredPosition4";
		this.comboPreferredPosition4.Size = new System.Drawing.Size(208, 21);
		this.comboPreferredPosition4.TabIndex = 3;
		this.comboPreferredPosition4.SelectedIndexChanged += new System.EventHandler(comboPreferredPosition4_SelectedIndexChanged);
		this.comboPreferredPosition3.FormattingEnabled = true;
		this.comboPreferredPosition3.Items.AddRange(new object[29]
		{
			"None", "Goalkeeper", "Sweeper", "Right Wing Back", "Right Back", "Right Central Back", "Central Back", "Left Central Back", "Left Back", "Left Wing Back",
			"Right Defensive Midfielder", "Central Defensive Midfielder", "Left Defensive Midfielder", "Right Midfielder", "Right Central Midfielder", "Central Midfielder", "Left Central Midfielder", "Left Midfielder", "Right Advanced Midfielder", "Central Advanced Midfielder",
			"Left Advanced Midfielder", "Right Forward", "Central Forward", "Left Forward", "Right Wing", "Right Striker", "Central Striker", "Left Striker", "Left Wing"
		});
		this.comboPreferredPosition3.Location = new System.Drawing.Point(17, 94);
		this.comboPreferredPosition3.Name = "comboPreferredPosition3";
		this.comboPreferredPosition3.Size = new System.Drawing.Size(208, 21);
		this.comboPreferredPosition3.TabIndex = 2;
		this.comboPreferredPosition3.SelectedIndexChanged += new System.EventHandler(comboPreferredPosition3_SelectedIndexChanged);
		this.comboPreferredPosition2.FormattingEnabled = true;
		this.comboPreferredPosition2.Items.AddRange(new object[29]
		{
			"None", "Goalkeeper", "Sweeper", "Right Wing Back", "Right Back", "Right Central Back", "Central Back", "Left Central Back", "Left Back", "Left Wing Back",
			"Right Defensive Midfielder", "Central Defensive Midfielder", "Left Defensive Midfielder", "Right Midfielder", "Right Central Midfielder", "Central Midfielder", "Left Central Midfielder", "Left Midfielder", "Right Advanced Midfielder", "Central Advanced Midfielder",
			"Left Advanced Midfielder", "Right Forward", "Central Forward", "Left Forward", "Right Wing", "Right Striker", "Central Striker", "Left Striker", "Left Wing"
		});
		this.comboPreferredPosition2.Location = new System.Drawing.Point(17, 67);
		this.comboPreferredPosition2.Name = "comboPreferredPosition2";
		this.comboPreferredPosition2.Size = new System.Drawing.Size(208, 21);
		this.comboPreferredPosition2.TabIndex = 1;
		this.comboPreferredPosition2.SelectedIndexChanged += new System.EventHandler(comboPreferredPosition2_SelectedIndexChanged);
		this.comboPreferredPosition1.FormattingEnabled = true;
		this.comboPreferredPosition1.Items.AddRange(new object[29]
		{
			"None", "Goalkeeper", "Sweeper", "Right Wing Back", "Right Back", "Right Central Back", "Central Back", "Left Central Back", "Left Back", "Left Wing Back",
			"Right Defensive Midfielder", "Central Defensive Midfielder", "Left Defensive Midfielder", "Right Midfielder", "Right Central Midfielder", "Central Midfielder", "Left Central Midfielder", "Left Midfielder", "Right Advanced Midfielder", "Central Advanced Midfielder",
			"Left Advanced Midfielder", "Right Forward", "Central Forward", "Left Forward", "Right Wing", "Right Striker", "Central Striker", "Left Striker", "Left Wing"
		});
		this.comboPreferredPosition1.Location = new System.Drawing.Point(17, 40);
		this.comboPreferredPosition1.Name = "comboPreferredPosition1";
		this.comboPreferredPosition1.Size = new System.Drawing.Size(208, 21);
		this.comboPreferredPosition1.TabIndex = 0;
		this.comboPreferredPosition1.SelectedIndexChanged += new System.EventHandler(comboPreferredPosition1_SelectedIndexChanged);
		this.domainInternationalReputation.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "internationalrep", true));
		this.domainInternationalReputation.Items.Add("Poor");
		this.domainInternationalReputation.Items.Add("Medium");
		this.domainInternationalReputation.Items.Add("Good");
		this.domainInternationalReputation.Items.Add("Very Good");
		this.domainInternationalReputation.Items.Add("Superstar");
		this.domainInternationalReputation.Location = new System.Drawing.Point(107, 164);
		this.domainInternationalReputation.Name = "domainInternationalReputation";
		this.domainInternationalReputation.Size = new System.Drawing.Size(119, 20);
		this.domainInternationalReputation.TabIndex = 4;
		this.domainInternationalReputation.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.domainInternationalReputation.Wrap = true;
		this.labelInternationalReputation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelInternationalReputation.Location = new System.Drawing.Point(14, 152);
		this.labelInternationalReputation.Name = "labelInternationalReputation";
		this.labelInternationalReputation.Size = new System.Drawing.Size(87, 41);
		this.labelInternationalReputation.TabIndex = 141;
		this.labelInternationalReputation.Text = "International Reputation";
		this.labelInternationalReputation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pageSkills.Controls.Add(this.flowPanelSkills);
		this.pageSkills.ImageIndex = 1;
		this.pageSkills.Location = new System.Drawing.Point(4, 23);
		this.pageSkills.Name = "pageSkills";
		this.pageSkills.Padding = new System.Windows.Forms.Padding(3);
		this.pageSkills.Size = new System.Drawing.Size(1349, 780);
		this.pageSkills.TabIndex = 1;
		this.pageSkills.Text = "Skills";
		this.pageSkills.UseVisualStyleBackColor = true;
		this.flowPanelSkills.AutoScroll = true;
		this.flowPanelSkills.Controls.Add(this.groupGenerateAttributes);
		this.flowPanelSkills.Controls.Add(this.groupGoalkeperSkills);
		this.flowPanelSkills.Controls.Add(this.groupDefensiveSkills);
		this.flowPanelSkills.Controls.Add(this.groupMidfielderSkills);
		this.flowPanelSkills.Controls.Add(this.groupMental);
		this.flowPanelSkills.Controls.Add(this.groupAttackingSkills);
		this.flowPanelSkills.Controls.Add(this.groupGenericAttributes);
		this.flowPanelSkills.Controls.Add(this.groupFreeKick);
		this.flowPanelSkills.Controls.Add(this.groupTraits);
		this.flowPanelSkills.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowPanelSkills.Location = new System.Drawing.Point(3, 3);
		this.flowPanelSkills.Name = "flowPanelSkills";
		this.flowPanelSkills.Size = new System.Drawing.Size(1343, 774);
		this.flowPanelSkills.TabIndex = 0;
		this.groupGenerateAttributes.BackColor = System.Drawing.SystemColors.Control;
		this.groupGenerateAttributes.Controls.Add(this.labelOverallrating);
		this.groupGenerateAttributes.Controls.Add(this.trackOverallrating);
		this.groupGenerateAttributes.Controls.Add(this.labelRandomize);
		this.groupGenerateAttributes.Controls.Add(this.numericRandomize);
		this.groupGenerateAttributes.Controls.Add(this.buttonRandomAboveAvg);
		this.groupGenerateAttributes.Controls.Add(this.buttonRandomBelowAvg);
		this.groupGenerateAttributes.Controls.Add(this.buttonRandomSuperstar);
		this.groupGenerateAttributes.Controls.Add(this.buttonRandomVeryGood);
		this.groupGenerateAttributes.Controls.Add(this.buttonRandomGood);
		this.groupGenerateAttributes.Controls.Add(this.buttonRandomAverage);
		this.groupGenerateAttributes.Controls.Add(this.buttonRandomPoor);
		this.groupGenerateAttributes.Location = new System.Drawing.Point(3, 3);
		this.groupGenerateAttributes.Name = "groupGenerateAttributes";
		this.groupGenerateAttributes.Size = new System.Drawing.Size(128, 378);
		this.groupGenerateAttributes.TabIndex = 9;
		this.groupGenerateAttributes.TabStop = false;
		this.groupGenerateAttributes.Text = "Random Generation";
		this.labelOverallrating.BackColor = System.Drawing.SystemColors.Control;
		this.labelOverallrating.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelOverallrating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelOverallrating.ForeColor = System.Drawing.Color.Yellow;
		this.labelOverallrating.Image = (System.Drawing.Image)resources.GetObject("labelOverallrating.Image");
		this.labelOverallrating.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelOverallrating.Location = new System.Drawing.Point(10, 280);
		this.labelOverallrating.Name = "labelOverallrating";
		this.labelOverallrating.Size = new System.Drawing.Size(112, 16);
		this.labelOverallrating.TabIndex = 126;
		this.labelOverallrating.Text = "Overall ";
		this.labelOverallrating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackOverallrating.BackColor = System.Drawing.SystemColors.Control;
		this.trackOverallrating.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackOverallrating.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "overallrating", true));
		this.trackOverallrating.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackOverallrating.LargeChange = 10;
		this.trackOverallrating.Location = new System.Drawing.Point(2, 288);
		this.trackOverallrating.Maximum = 99;
		this.trackOverallrating.Minimum = 1;
		this.trackOverallrating.Name = "trackOverallrating";
		this.trackOverallrating.Size = new System.Drawing.Size(128, 45);
		this.trackOverallrating.TabIndex = 125;
		this.trackOverallrating.TickFrequency = 10;
		this.trackOverallrating.Value = 1;
		this.trackOverallrating.ValueChanged += new System.EventHandler(trackOverallrating_ValueChanged);
		this.labelRandomize.Location = new System.Drawing.Point(2, 16);
		this.labelRandomize.Name = "labelRandomize";
		this.labelRandomize.Size = new System.Drawing.Size(56, 31);
		this.labelRandomize.TabIndex = 8;
		this.labelRandomize.Text = "Computed Overall";
		this.numericRandomize.Location = new System.Drawing.Point(59, 24);
		this.numericRandomize.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericRandomize.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericRandomize.Name = "numericRandomize";
		this.numericRandomize.Size = new System.Drawing.Size(53, 20);
		this.numericRandomize.TabIndex = 0;
		this.numericRandomize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericRandomize.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericRandomize.ValueChanged += new System.EventHandler(numericOverall_ValueChanged);
		this.buttonRandomAboveAvg.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomAboveAvg.Location = new System.Drawing.Point(11, 134);
		this.buttonRandomAboveAvg.Name = "buttonRandomAboveAvg";
		this.buttonRandomAboveAvg.Size = new System.Drawing.Size(101, 27);
		this.buttonRandomAboveAvg.TabIndex = 4;
		this.buttonRandomAboveAvg.Text = "Above Avg.";
		this.buttonRandomAboveAvg.Click += new System.EventHandler(buttonRandomAboveAvg_Click);
		this.buttonRandomBelowAvg.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomBelowAvg.Location = new System.Drawing.Point(11, 78);
		this.buttonRandomBelowAvg.Name = "buttonRandomBelowAvg";
		this.buttonRandomBelowAvg.Size = new System.Drawing.Size(101, 27);
		this.buttonRandomBelowAvg.TabIndex = 2;
		this.buttonRandomBelowAvg.Text = "Below Avg.";
		this.buttonRandomBelowAvg.Click += new System.EventHandler(buttonRandomBelowAvg_Click);
		this.buttonRandomSuperstar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonRandomSuperstar.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomSuperstar.Location = new System.Drawing.Point(11, 219);
		this.buttonRandomSuperstar.Name = "buttonRandomSuperstar";
		this.buttonRandomSuperstar.Size = new System.Drawing.Size(101, 27);
		this.buttonRandomSuperstar.TabIndex = 7;
		this.buttonRandomSuperstar.Text = "Superstar";
		this.buttonRandomSuperstar.Click += new System.EventHandler(buttonRandomSuperstar_Click);
		this.buttonRandomVeryGood.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomVeryGood.Location = new System.Drawing.Point(11, 190);
		this.buttonRandomVeryGood.Name = "buttonRandomVeryGood";
		this.buttonRandomVeryGood.Size = new System.Drawing.Size(101, 27);
		this.buttonRandomVeryGood.TabIndex = 6;
		this.buttonRandomVeryGood.Text = "Very Good";
		this.buttonRandomVeryGood.Click += new System.EventHandler(buttonRandomVeryGood_Click);
		this.buttonRandomGood.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomGood.Location = new System.Drawing.Point(11, 162);
		this.buttonRandomGood.Name = "buttonRandomGood";
		this.buttonRandomGood.Size = new System.Drawing.Size(101, 27);
		this.buttonRandomGood.TabIndex = 5;
		this.buttonRandomGood.Text = "Good";
		this.buttonRandomGood.Click += new System.EventHandler(buttonRandomGood_Click);
		this.buttonRandomAverage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomAverage.Location = new System.Drawing.Point(11, 106);
		this.buttonRandomAverage.Name = "buttonRandomAverage";
		this.buttonRandomAverage.Size = new System.Drawing.Size(101, 27);
		this.buttonRandomAverage.TabIndex = 3;
		this.buttonRandomAverage.Text = "Average";
		this.buttonRandomAverage.Click += new System.EventHandler(buttonRandomAverage_Click);
		this.buttonRandomPoor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRandomPoor.Location = new System.Drawing.Point(11, 50);
		this.buttonRandomPoor.Name = "buttonRandomPoor";
		this.buttonRandomPoor.Size = new System.Drawing.Size(101, 27);
		this.buttonRandomPoor.TabIndex = 1;
		this.buttonRandomPoor.Text = "Poor";
		this.buttonRandomPoor.Click += new System.EventHandler(buttonRandomPoor_Click);
		this.groupGoalkeperSkills.BackColor = System.Drawing.SystemColors.Control;
		this.groupGoalkeperSkills.Controls.Add(this.label5);
		this.groupGoalkeperSkills.Controls.Add(this.comboGkSaveStyle);
		this.groupGoalkeperSkills.Controls.Add(this.label3);
		this.groupGoalkeperSkills.Controls.Add(this.labelGkKick);
		this.groupGoalkeperSkills.Controls.Add(this.comboGkKickStyle);
		this.groupGoalkeperSkills.Controls.Add(this.trackGkKicking);
		this.groupGoalkeperSkills.Controls.Add(this.labelDiving);
		this.groupGoalkeperSkills.Controls.Add(this.labelPositioning);
		this.groupGoalkeperSkills.Controls.Add(this.labelReflexes);
		this.groupGoalkeperSkills.Controls.Add(this.labelHandling);
		this.groupGoalkeperSkills.Controls.Add(this.trackDiving);
		this.groupGoalkeperSkills.Controls.Add(this.trackPositioning);
		this.groupGoalkeperSkills.Controls.Add(this.trackReflexes);
		this.groupGoalkeperSkills.Controls.Add(this.trackHandling);
		this.groupGoalkeperSkills.Controls.Add(this.numericGoalkeeperSkills);
		this.groupGoalkeperSkills.Location = new System.Drawing.Point(137, 3);
		this.groupGoalkeperSkills.Name = "groupGoalkeperSkills";
		this.groupGoalkeperSkills.Size = new System.Drawing.Size(140, 378);
		this.groupGoalkeperSkills.TabIndex = 14;
		this.groupGoalkeperSkills.TabStop = false;
		this.groupGoalkeperSkills.Text = "Goalkeeper Skills";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(43, 301);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(58, 13);
		this.label5.TabIndex = 96;
		this.label5.Text = "Save Style";
		this.comboGkSaveStyle.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "gksavetype", true));
		this.comboGkSaveStyle.FormattingEnabled = true;
		this.comboGkSaveStyle.Items.AddRange(new object[2] { "Traditional", "Acrobatic" });
		this.comboGkSaveStyle.Location = new System.Drawing.Point(7, 317);
		this.comboGkSaveStyle.Name = "comboGkSaveStyle";
		this.comboGkSaveStyle.Size = new System.Drawing.Size(124, 21);
		this.comboGkSaveStyle.TabIndex = 95;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(6, 280);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(54, 13);
		this.label3.TabIndex = 81;
		this.label3.Text = "Kick Style";
		this.labelGkKick.BackColor = System.Drawing.SystemColors.Control;
		this.labelGkKick.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelGkKick.ForeColor = System.Drawing.Color.Yellow;
		this.labelGkKick.Image = (System.Drawing.Image)resources.GetObject("labelGkKick.Image");
		this.labelGkKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelGkKick.Location = new System.Drawing.Point(14, 232);
		this.labelGkKick.Name = "labelGkKick";
		this.labelGkKick.Size = new System.Drawing.Size(112, 16);
		this.labelGkKick.TabIndex = 94;
		this.labelGkKick.Text = "Kicking ";
		this.labelGkKick.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.comboGkKickStyle.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "gkkickstyle", true));
		this.comboGkKickStyle.FormattingEnabled = true;
		this.comboGkKickStyle.Items.AddRange(new object[4] { "0", "1", "2", "3" });
		this.comboGkKickStyle.Location = new System.Drawing.Point(66, 277);
		this.comboGkKickStyle.Name = "comboGkKickStyle";
		this.comboGkKickStyle.Size = new System.Drawing.Size(65, 21);
		this.comboGkKickStyle.TabIndex = 7;
		this.trackGkKicking.BackColor = System.Drawing.SystemColors.Control;
		this.trackGkKicking.Cursor = System.Windows.Forms.Cursors.Default;
		this.trackGkKicking.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "gkkicking", true));
		this.trackGkKicking.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackGkKicking.LargeChange = 10;
		this.trackGkKicking.Location = new System.Drawing.Point(6, 240);
		this.trackGkKicking.Maximum = 99;
		this.trackGkKicking.Minimum = 1;
		this.trackGkKicking.Name = "trackGkKicking";
		this.trackGkKicking.Size = new System.Drawing.Size(128, 45);
		this.trackGkKicking.TabIndex = 6;
		this.trackGkKicking.TickFrequency = 10;
		this.trackGkKicking.Value = 1;
		this.trackGkKicking.ValueChanged += new System.EventHandler(trackGkKick_ValueChanged);
		this.labelDiving.BackColor = System.Drawing.SystemColors.Control;
		this.labelDiving.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelDiving.ForeColor = System.Drawing.Color.Yellow;
		this.labelDiving.Image = (System.Drawing.Image)resources.GetObject("labelDiving.Image");
		this.labelDiving.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDiving.Location = new System.Drawing.Point(14, 136);
		this.labelDiving.Name = "labelDiving";
		this.labelDiving.Size = new System.Drawing.Size(112, 16);
		this.labelDiving.TabIndex = 88;
		this.labelDiving.Text = "Diving ";
		this.labelDiving.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPositioning.BackColor = System.Drawing.SystemColors.Control;
		this.labelPositioning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPositioning.ForeColor = System.Drawing.Color.Yellow;
		this.labelPositioning.Image = (System.Drawing.Image)resources.GetObject("labelPositioning.Image");
		this.labelPositioning.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPositioning.Location = new System.Drawing.Point(14, 184);
		this.labelPositioning.Name = "labelPositioning";
		this.labelPositioning.Size = new System.Drawing.Size(112, 16);
		this.labelPositioning.TabIndex = 90;
		this.labelPositioning.Text = "Positioning ";
		this.labelPositioning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelReflexes.BackColor = System.Drawing.SystemColors.Control;
		this.labelReflexes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelReflexes.ForeColor = System.Drawing.Color.Yellow;
		this.labelReflexes.Image = (System.Drawing.Image)resources.GetObject("labelReflexes.Image");
		this.labelReflexes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelReflexes.Location = new System.Drawing.Point(14, 40);
		this.labelReflexes.Name = "labelReflexes";
		this.labelReflexes.Size = new System.Drawing.Size(112, 16);
		this.labelReflexes.TabIndex = 84;
		this.labelReflexes.Text = "Reflexes ";
		this.labelReflexes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelHandling.BackColor = System.Drawing.SystemColors.Control;
		this.labelHandling.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelHandling.ForeColor = System.Drawing.Color.Yellow;
		this.labelHandling.Image = (System.Drawing.Image)resources.GetObject("labelHandling.Image");
		this.labelHandling.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHandling.Location = new System.Drawing.Point(14, 88);
		this.labelHandling.Name = "labelHandling";
		this.labelHandling.Size = new System.Drawing.Size(112, 16);
		this.labelHandling.TabIndex = 86;
		this.labelHandling.Text = "Handling ";
		this.labelHandling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackDiving.BackColor = System.Drawing.SystemColors.Control;
		this.trackDiving.Cursor = System.Windows.Forms.Cursors.Default;
		this.trackDiving.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "gkdiving", true));
		this.trackDiving.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackDiving.LargeChange = 10;
		this.trackDiving.Location = new System.Drawing.Point(5, 144);
		this.trackDiving.Maximum = 99;
		this.trackDiving.Minimum = 1;
		this.trackDiving.Name = "trackDiving";
		this.trackDiving.Size = new System.Drawing.Size(128, 45);
		this.trackDiving.TabIndex = 3;
		this.trackDiving.TickFrequency = 10;
		this.trackDiving.Value = 1;
		this.trackDiving.ValueChanged += new System.EventHandler(trackDiving_ValueChanged);
		this.trackPositioning.BackColor = System.Drawing.SystemColors.Control;
		this.trackPositioning.Cursor = System.Windows.Forms.Cursors.Default;
		this.trackPositioning.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "gkpositioning", true));
		this.trackPositioning.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackPositioning.LargeChange = 10;
		this.trackPositioning.Location = new System.Drawing.Point(6, 195);
		this.trackPositioning.Maximum = 99;
		this.trackPositioning.Minimum = 1;
		this.trackPositioning.Name = "trackPositioning";
		this.trackPositioning.Size = new System.Drawing.Size(128, 45);
		this.trackPositioning.TabIndex = 4;
		this.trackPositioning.TickFrequency = 10;
		this.trackPositioning.Value = 1;
		this.trackPositioning.ValueChanged += new System.EventHandler(trackPositioning_ValueChanged);
		this.trackReflexes.BackColor = System.Drawing.SystemColors.Control;
		this.trackReflexes.Cursor = System.Windows.Forms.Cursors.Default;
		this.trackReflexes.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "gkreflexes", true));
		this.trackReflexes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackReflexes.LargeChange = 10;
		this.trackReflexes.Location = new System.Drawing.Point(6, 48);
		this.trackReflexes.Maximum = 99;
		this.trackReflexes.Minimum = 1;
		this.trackReflexes.Name = "trackReflexes";
		this.trackReflexes.Size = new System.Drawing.Size(128, 45);
		this.trackReflexes.TabIndex = 1;
		this.trackReflexes.TickFrequency = 10;
		this.trackReflexes.Value = 1;
		this.trackReflexes.ValueChanged += new System.EventHandler(trackReflexes_ValueChanged);
		this.trackHandling.BackColor = System.Drawing.SystemColors.Control;
		this.trackHandling.Cursor = System.Windows.Forms.Cursors.Default;
		this.trackHandling.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "gkhandling", true));
		this.trackHandling.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackHandling.LargeChange = 10;
		this.trackHandling.Location = new System.Drawing.Point(5, 96);
		this.trackHandling.Maximum = 99;
		this.trackHandling.Minimum = 1;
		this.trackHandling.Name = "trackHandling";
		this.trackHandling.Size = new System.Drawing.Size(128, 45);
		this.trackHandling.TabIndex = 2;
		this.trackHandling.TickFrequency = 10;
		this.trackHandling.Value = 1;
		this.trackHandling.ValueChanged += new System.EventHandler(trackHandling_ValueChanged);
		this.numericGoalkeeperSkills.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericGoalkeeperSkills.BackColor = System.Drawing.Color.Teal;
		this.numericGoalkeeperSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold);
		this.numericGoalkeeperSkills.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.numericGoalkeeperSkills.Location = new System.Drawing.Point(49, 15);
		this.numericGoalkeeperSkills.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericGoalkeeperSkills.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericGoalkeeperSkills.Name = "numericGoalkeeperSkills";
		this.numericGoalkeeperSkills.Size = new System.Drawing.Size(44, 22);
		this.numericGoalkeeperSkills.TabIndex = 0;
		this.numericGoalkeeperSkills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericGoalkeeperSkills.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericGoalkeeperSkills.ValueChanged += new System.EventHandler(numericGoalkeeperSkills_ValueChanged);
		this.groupDefensiveSkills.BackColor = System.Drawing.SystemColors.Control;
		this.groupDefensiveSkills.Controls.Add(this.labelInterception);
		this.groupDefensiveSkills.Controls.Add(this.trackInterception);
		this.groupDefensiveSkills.Controls.Add(this.labelSliding);
		this.groupDefensiveSkills.Controls.Add(this.trackSliding);
		this.groupDefensiveSkills.Controls.Add(this.numericDefensiveSkills);
		this.groupDefensiveSkills.Controls.Add(this.labelAggression);
		this.groupDefensiveSkills.Controls.Add(this.labelMarking);
		this.groupDefensiveSkills.Controls.Add(this.labelTackling);
		this.groupDefensiveSkills.Controls.Add(this.trackTackling);
		this.groupDefensiveSkills.Controls.Add(this.trackMarking);
		this.groupDefensiveSkills.Controls.Add(this.trackAggression);
		this.groupDefensiveSkills.Location = new System.Drawing.Point(283, 3);
		this.groupDefensiveSkills.Name = "groupDefensiveSkills";
		this.groupDefensiveSkills.Size = new System.Drawing.Size(140, 378);
		this.groupDefensiveSkills.TabIndex = 15;
		this.groupDefensiveSkills.TabStop = false;
		this.groupDefensiveSkills.Text = "Defensive Skills";
		this.labelInterception.BackColor = System.Drawing.SystemColors.Control;
		this.labelInterception.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelInterception.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelInterception.ForeColor = System.Drawing.Color.Yellow;
		this.labelInterception.Image = (System.Drawing.Image)resources.GetObject("labelInterception.Image");
		this.labelInterception.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelInterception.Location = new System.Drawing.Point(16, 230);
		this.labelInterception.Name = "labelInterception";
		this.labelInterception.Size = new System.Drawing.Size(112, 16);
		this.labelInterception.TabIndex = 102;
		this.labelInterception.Text = "Interception ";
		this.labelInterception.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackInterception.BackColor = System.Drawing.SystemColors.Control;
		this.trackInterception.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackInterception.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "interceptions", true));
		this.trackInterception.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackInterception.LargeChange = 10;
		this.trackInterception.Location = new System.Drawing.Point(6, 238);
		this.trackInterception.Maximum = 99;
		this.trackInterception.Minimum = 1;
		this.trackInterception.Name = "trackInterception";
		this.trackInterception.Size = new System.Drawing.Size(128, 45);
		this.trackInterception.TabIndex = 101;
		this.trackInterception.TickFrequency = 10;
		this.trackInterception.Value = 1;
		this.trackInterception.ValueChanged += new System.EventHandler(trackInterception_ValueChanged);
		this.labelSliding.BackColor = System.Drawing.SystemColors.Control;
		this.labelSliding.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelSliding.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelSliding.ForeColor = System.Drawing.Color.Yellow;
		this.labelSliding.Image = (System.Drawing.Image)resources.GetObject("labelSliding.Image");
		this.labelSliding.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSliding.Location = new System.Drawing.Point(16, 184);
		this.labelSliding.Name = "labelSliding";
		this.labelSliding.Size = new System.Drawing.Size(112, 16);
		this.labelSliding.TabIndex = 100;
		this.labelSliding.Text = "Sliding ";
		this.labelSliding.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackSliding.BackColor = System.Drawing.SystemColors.Control;
		this.trackSliding.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackSliding.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "slidingtackle", true));
		this.trackSliding.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackSliding.LargeChange = 10;
		this.trackSliding.Location = new System.Drawing.Point(6, 192);
		this.trackSliding.Maximum = 99;
		this.trackSliding.Minimum = 1;
		this.trackSliding.Name = "trackSliding";
		this.trackSliding.Size = new System.Drawing.Size(128, 45);
		this.trackSliding.TabIndex = 4;
		this.trackSliding.TickFrequency = 10;
		this.trackSliding.Value = 1;
		this.trackSliding.ValueChanged += new System.EventHandler(trackSliding_ValueChanged);
		this.numericDefensiveSkills.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericDefensiveSkills.BackColor = System.Drawing.Color.Teal;
		this.numericDefensiveSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold);
		this.numericDefensiveSkills.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.numericDefensiveSkills.Location = new System.Drawing.Point(48, 16);
		this.numericDefensiveSkills.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericDefensiveSkills.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericDefensiveSkills.Name = "numericDefensiveSkills";
		this.numericDefensiveSkills.Size = new System.Drawing.Size(44, 22);
		this.numericDefensiveSkills.TabIndex = 0;
		this.numericDefensiveSkills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericDefensiveSkills.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericDefensiveSkills.ValueChanged += new System.EventHandler(numericDefensiveSkills_ValueChanged);
		this.labelAggression.BackColor = System.Drawing.SystemColors.Control;
		this.labelAggression.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelAggression.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelAggression.ForeColor = System.Drawing.Color.Yellow;
		this.labelAggression.Image = (System.Drawing.Image)resources.GetObject("labelAggression.Image");
		this.labelAggression.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelAggression.Location = new System.Drawing.Point(14, 136);
		this.labelAggression.Name = "labelAggression";
		this.labelAggression.Size = new System.Drawing.Size(112, 16);
		this.labelAggression.TabIndex = 67;
		this.labelAggression.Text = "Aggression ";
		this.labelAggression.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelMarking.BackColor = System.Drawing.SystemColors.Control;
		this.labelMarking.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelMarking.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelMarking.ForeColor = System.Drawing.Color.Yellow;
		this.labelMarking.Image = (System.Drawing.Image)resources.GetObject("labelMarking.Image");
		this.labelMarking.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelMarking.Location = new System.Drawing.Point(14, 40);
		this.labelMarking.Name = "labelMarking";
		this.labelMarking.Size = new System.Drawing.Size(112, 16);
		this.labelMarking.TabIndex = 75;
		this.labelMarking.Text = "Marking ";
		this.labelMarking.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelTackling.BackColor = System.Drawing.SystemColors.Control;
		this.labelTackling.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelTackling.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelTackling.ForeColor = System.Drawing.Color.Yellow;
		this.labelTackling.Image = (System.Drawing.Image)resources.GetObject("labelTackling.Image");
		this.labelTackling.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelTackling.Location = new System.Drawing.Point(14, 88);
		this.labelTackling.Name = "labelTackling";
		this.labelTackling.Size = new System.Drawing.Size(112, 16);
		this.labelTackling.TabIndex = 77;
		this.labelTackling.Text = "Tackling ";
		this.labelTackling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackTackling.BackColor = System.Drawing.SystemColors.Control;
		this.trackTackling.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackTackling.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "standingtackle", true));
		this.trackTackling.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackTackling.LargeChange = 10;
		this.trackTackling.Location = new System.Drawing.Point(6, 96);
		this.trackTackling.Maximum = 99;
		this.trackTackling.Minimum = 1;
		this.trackTackling.Name = "trackTackling";
		this.trackTackling.Size = new System.Drawing.Size(128, 45);
		this.trackTackling.TabIndex = 2;
		this.trackTackling.TickFrequency = 10;
		this.trackTackling.Value = 1;
		this.trackTackling.ValueChanged += new System.EventHandler(trackTackling_ValueChanged);
		this.trackMarking.BackColor = System.Drawing.SystemColors.Control;
		this.trackMarking.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackMarking.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "marking", true));
		this.trackMarking.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackMarking.LargeChange = 10;
		this.trackMarking.Location = new System.Drawing.Point(6, 48);
		this.trackMarking.Maximum = 99;
		this.trackMarking.Minimum = 1;
		this.trackMarking.Name = "trackMarking";
		this.trackMarking.Size = new System.Drawing.Size(128, 45);
		this.trackMarking.TabIndex = 1;
		this.trackMarking.TickFrequency = 10;
		this.trackMarking.Value = 1;
		this.trackMarking.ValueChanged += new System.EventHandler(trackMarking_ValueChanged);
		this.trackAggression.BackColor = System.Drawing.SystemColors.Control;
		this.trackAggression.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackAggression.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "aggression", true));
		this.trackAggression.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackAggression.LargeChange = 10;
		this.trackAggression.Location = new System.Drawing.Point(6, 144);
		this.trackAggression.Maximum = 99;
		this.trackAggression.Minimum = 1;
		this.trackAggression.Name = "trackAggression";
		this.trackAggression.Size = new System.Drawing.Size(128, 45);
		this.trackAggression.TabIndex = 3;
		this.trackAggression.TickFrequency = 10;
		this.trackAggression.Value = 1;
		this.trackAggression.ValueChanged += new System.EventHandler(trackAggression_ValueChanged);
		this.groupMidfielderSkills.BackColor = System.Drawing.SystemColors.Control;
		this.groupMidfielderSkills.Controls.Add(this.labelCurve);
		this.groupMidfielderSkills.Controls.Add(this.trackCurve);
		this.groupMidfielderSkills.Controls.Add(this.labelVision);
		this.groupMidfielderSkills.Controls.Add(this.trackVision);
		this.groupMidfielderSkills.Controls.Add(this.numericMidfielderSkills);
		this.groupMidfielderSkills.Controls.Add(this.labelBallControl);
		this.groupMidfielderSkills.Controls.Add(this.labelCrossing);
		this.groupMidfielderSkills.Controls.Add(this.labelLongPassing);
		this.groupMidfielderSkills.Controls.Add(this.trackLongPassing);
		this.groupMidfielderSkills.Controls.Add(this.labelShortPassing);
		this.groupMidfielderSkills.Controls.Add(this.trackShortPassing);
		this.groupMidfielderSkills.Controls.Add(this.trackBallControl);
		this.groupMidfielderSkills.Controls.Add(this.trackCrossing);
		this.groupMidfielderSkills.Location = new System.Drawing.Point(429, 3);
		this.groupMidfielderSkills.Name = "groupMidfielderSkills";
		this.groupMidfielderSkills.Size = new System.Drawing.Size(140, 378);
		this.groupMidfielderSkills.TabIndex = 16;
		this.groupMidfielderSkills.TabStop = false;
		this.groupMidfielderSkills.Text = "Midfielder Skills";
		this.labelCurve.BackColor = System.Drawing.SystemColors.Control;
		this.labelCurve.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelCurve.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelCurve.ForeColor = System.Drawing.Color.Yellow;
		this.labelCurve.Image = (System.Drawing.Image)resources.GetObject("labelCurve.Image");
		this.labelCurve.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCurve.Location = new System.Drawing.Point(11, 280);
		this.labelCurve.Name = "labelCurve";
		this.labelCurve.Size = new System.Drawing.Size(112, 16);
		this.labelCurve.TabIndex = 106;
		this.labelCurve.Text = "Curve ";
		this.labelCurve.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackCurve.BackColor = System.Drawing.SystemColors.Control;
		this.trackCurve.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackCurve.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "curve", true));
		this.trackCurve.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackCurve.LargeChange = 10;
		this.trackCurve.Location = new System.Drawing.Point(1, 288);
		this.trackCurve.Maximum = 99;
		this.trackCurve.Minimum = 1;
		this.trackCurve.Name = "trackCurve";
		this.trackCurve.Size = new System.Drawing.Size(128, 45);
		this.trackCurve.TabIndex = 6;
		this.trackCurve.TickFrequency = 10;
		this.trackCurve.Value = 1;
		this.trackCurve.ValueChanged += new System.EventHandler(trackCurve_ValueChanged);
		this.labelVision.BackColor = System.Drawing.SystemColors.Control;
		this.labelVision.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelVision.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelVision.ForeColor = System.Drawing.Color.Yellow;
		this.labelVision.Image = (System.Drawing.Image)resources.GetObject("labelVision.Image");
		this.labelVision.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelVision.Location = new System.Drawing.Point(11, 232);
		this.labelVision.Name = "labelVision";
		this.labelVision.Size = new System.Drawing.Size(112, 16);
		this.labelVision.TabIndex = 104;
		this.labelVision.Text = "Vision ";
		this.labelVision.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackVision.BackColor = System.Drawing.SystemColors.Control;
		this.trackVision.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackVision.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "vision", true));
		this.trackVision.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackVision.LargeChange = 10;
		this.trackVision.Location = new System.Drawing.Point(1, 240);
		this.trackVision.Maximum = 99;
		this.trackVision.Minimum = 1;
		this.trackVision.Name = "trackVision";
		this.trackVision.Size = new System.Drawing.Size(128, 45);
		this.trackVision.TabIndex = 5;
		this.trackVision.TickFrequency = 10;
		this.trackVision.Value = 1;
		this.trackVision.ValueChanged += new System.EventHandler(trackVision_ValueChanged);
		this.numericMidfielderSkills.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericMidfielderSkills.BackColor = System.Drawing.Color.Teal;
		this.numericMidfielderSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold);
		this.numericMidfielderSkills.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.numericMidfielderSkills.Location = new System.Drawing.Point(41, 15);
		this.numericMidfielderSkills.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericMidfielderSkills.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericMidfielderSkills.Name = "numericMidfielderSkills";
		this.numericMidfielderSkills.Size = new System.Drawing.Size(44, 22);
		this.numericMidfielderSkills.TabIndex = 0;
		this.numericMidfielderSkills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericMidfielderSkills.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericMidfielderSkills.ValueChanged += new System.EventHandler(numericMidfielderSkills_ValueChanged);
		this.labelBallControl.BackColor = System.Drawing.SystemColors.Control;
		this.labelBallControl.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelBallControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelBallControl.ForeColor = System.Drawing.Color.Yellow;
		this.labelBallControl.Image = (System.Drawing.Image)resources.GetObject("labelBallControl.Image");
		this.labelBallControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBallControl.Location = new System.Drawing.Point(11, 184);
		this.labelBallControl.Name = "labelBallControl";
		this.labelBallControl.Size = new System.Drawing.Size(112, 16);
		this.labelBallControl.TabIndex = 79;
		this.labelBallControl.Text = "Ball-Control ";
		this.labelBallControl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelCrossing.BackColor = System.Drawing.SystemColors.Control;
		this.labelCrossing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelCrossing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelCrossing.ForeColor = System.Drawing.Color.Yellow;
		this.labelCrossing.Image = (System.Drawing.Image)resources.GetObject("labelCrossing.Image");
		this.labelCrossing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCrossing.Location = new System.Drawing.Point(9, 136);
		this.labelCrossing.Name = "labelCrossing";
		this.labelCrossing.Size = new System.Drawing.Size(112, 16);
		this.labelCrossing.TabIndex = 84;
		this.labelCrossing.Text = "Crossing ";
		this.labelCrossing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelLongPassing.BackColor = System.Drawing.SystemColors.Control;
		this.labelLongPassing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelLongPassing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelLongPassing.ForeColor = System.Drawing.Color.Yellow;
		this.labelLongPassing.Image = (System.Drawing.Image)resources.GetObject("labelLongPassing.Image");
		this.labelLongPassing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLongPassing.Location = new System.Drawing.Point(9, 88);
		this.labelLongPassing.Name = "labelLongPassing";
		this.labelLongPassing.Size = new System.Drawing.Size(112, 16);
		this.labelLongPassing.TabIndex = 102;
		this.labelLongPassing.Text = "Long-Passing ";
		this.labelLongPassing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackLongPassing.BackColor = System.Drawing.SystemColors.Control;
		this.trackLongPassing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackLongPassing.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "longpassing", true));
		this.trackLongPassing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackLongPassing.LargeChange = 10;
		this.trackLongPassing.Location = new System.Drawing.Point(1, 96);
		this.trackLongPassing.Maximum = 99;
		this.trackLongPassing.Minimum = 1;
		this.trackLongPassing.Name = "trackLongPassing";
		this.trackLongPassing.Size = new System.Drawing.Size(128, 45);
		this.trackLongPassing.TabIndex = 2;
		this.trackLongPassing.TickFrequency = 10;
		this.trackLongPassing.Value = 1;
		this.trackLongPassing.ValueChanged += new System.EventHandler(trackLongPassing_ValueChanged);
		this.labelShortPassing.BackColor = System.Drawing.SystemColors.Control;
		this.labelShortPassing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelShortPassing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelShortPassing.ForeColor = System.Drawing.Color.Yellow;
		this.labelShortPassing.Image = (System.Drawing.Image)resources.GetObject("labelShortPassing.Image");
		this.labelShortPassing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelShortPassing.Location = new System.Drawing.Point(9, 40);
		this.labelShortPassing.Name = "labelShortPassing";
		this.labelShortPassing.Size = new System.Drawing.Size(112, 16);
		this.labelShortPassing.TabIndex = 100;
		this.labelShortPassing.Text = "Short-Passing ";
		this.labelShortPassing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackShortPassing.BackColor = System.Drawing.SystemColors.Control;
		this.trackShortPassing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackShortPassing.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "shortpassing", true));
		this.trackShortPassing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackShortPassing.LargeChange = 10;
		this.trackShortPassing.Location = new System.Drawing.Point(1, 48);
		this.trackShortPassing.Maximum = 99;
		this.trackShortPassing.Minimum = 1;
		this.trackShortPassing.Name = "trackShortPassing";
		this.trackShortPassing.Size = new System.Drawing.Size(128, 45);
		this.trackShortPassing.TabIndex = 1;
		this.trackShortPassing.TickFrequency = 10;
		this.trackShortPassing.Value = 1;
		this.trackShortPassing.ValueChanged += new System.EventHandler(trackShortPassing_ValueChanged);
		this.trackBallControl.BackColor = System.Drawing.SystemColors.Control;
		this.trackBallControl.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackBallControl.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "ballcontrol", true));
		this.trackBallControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackBallControl.LargeChange = 10;
		this.trackBallControl.Location = new System.Drawing.Point(1, 192);
		this.trackBallControl.Maximum = 99;
		this.trackBallControl.Minimum = 1;
		this.trackBallControl.Name = "trackBallControl";
		this.trackBallControl.Size = new System.Drawing.Size(128, 45);
		this.trackBallControl.TabIndex = 4;
		this.trackBallControl.TickFrequency = 10;
		this.trackBallControl.Value = 1;
		this.trackBallControl.ValueChanged += new System.EventHandler(trackBallControl_ValueChanged);
		this.trackCrossing.BackColor = System.Drawing.SystemColors.Control;
		this.trackCrossing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackCrossing.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "crossing", true));
		this.trackCrossing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackCrossing.LargeChange = 10;
		this.trackCrossing.Location = new System.Drawing.Point(1, 144);
		this.trackCrossing.Maximum = 99;
		this.trackCrossing.Minimum = 1;
		this.trackCrossing.Name = "trackCrossing";
		this.trackCrossing.Size = new System.Drawing.Size(128, 45);
		this.trackCrossing.TabIndex = 3;
		this.trackCrossing.TickFrequency = 10;
		this.trackCrossing.Value = 1;
		this.trackCrossing.ValueChanged += new System.EventHandler(trackCrossing_ValueChanged);
		this.groupMental.BackColor = System.Drawing.SystemColors.Control;
		this.groupMental.Controls.Add(this.label14);
		this.groupMental.Controls.Add(this.numericUpDown5);
		this.groupMental.Controls.Add(this.comboDefensiveWorkrate);
		this.groupMental.Controls.Add(this.label10);
		this.groupMental.Controls.Add(this.comboAttackWorkRate);
		this.groupMental.Controls.Add(this.label9);
		this.groupMental.Controls.Add(this.numericMentalSkills);
		this.groupMental.Controls.Add(this.labelPlayerPositioning);
		this.groupMental.Controls.Add(this.labelPotential);
		this.groupMental.Controls.Add(this.trackPlayerPositioning);
		this.groupMental.Controls.Add(this.trackPotential);
		this.groupMental.Location = new System.Drawing.Point(575, 3);
		this.groupMental.Name = "groupMental";
		this.groupMental.Size = new System.Drawing.Size(140, 378);
		this.groupMental.TabIndex = 26;
		this.groupMental.TabStop = false;
		this.groupMental.Text = "Mental Skills";
		this.label14.AutoSize = true;
		this.label14.BackColor = System.Drawing.Color.Transparent;
		this.label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label14.Location = new System.Drawing.Point(13, 234);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(45, 13);
		this.label14.TabIndex = 138;
		this.label14.Text = "Emotion";
		this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericUpDown5.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "emotion", true));
		this.numericUpDown5.Location = new System.Drawing.Point(70, 230);
		this.numericUpDown5.Maximum = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown5.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDown5.Name = "numericUpDown5";
		this.numericUpDown5.Size = new System.Drawing.Size(58, 20);
		this.numericUpDown5.TabIndex = 137;
		this.numericUpDown5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown5.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.comboDefensiveWorkrate.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "defensiveworkrate", true));
		this.comboDefensiveWorkrate.FormattingEnabled = true;
		this.comboDefensiveWorkrate.Items.AddRange(new object[3] { "Medium", "Low", "High" });
		this.comboDefensiveWorkrate.Location = new System.Drawing.Point(8, 190);
		this.comboDefensiveWorkrate.Name = "comboDefensiveWorkrate";
		this.comboDefensiveWorkrate.Size = new System.Drawing.Size(120, 21);
		this.comboDefensiveWorkrate.TabIndex = 135;
		this.label10.AutoSize = true;
		this.label10.BackColor = System.Drawing.Color.Transparent;
		this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label10.Location = new System.Drawing.Point(13, 174);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(102, 13);
		this.label10.TabIndex = 136;
		this.label10.Text = "Defensive Workrate";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboAttackWorkRate.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "attackingworkrate", true));
		this.comboAttackWorkRate.FormattingEnabled = true;
		this.comboAttackWorkRate.Items.AddRange(new object[3] { "Medium", "Low", "High" });
		this.comboAttackWorkRate.Location = new System.Drawing.Point(8, 145);
		this.comboAttackWorkRate.Name = "comboAttackWorkRate";
		this.comboAttackWorkRate.Size = new System.Drawing.Size(120, 21);
		this.comboAttackWorkRate.TabIndex = 133;
		this.label9.AutoSize = true;
		this.label9.BackColor = System.Drawing.Color.Transparent;
		this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label9.Location = new System.Drawing.Point(15, 129);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(99, 13);
		this.label9.TabIndex = 134;
		this.label9.Text = "Attacking Workrate";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericMentalSkills.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericMentalSkills.BackColor = System.Drawing.Color.Teal;
		this.numericMentalSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold);
		this.numericMentalSkills.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.numericMentalSkills.Location = new System.Drawing.Point(44, 13);
		this.numericMentalSkills.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericMentalSkills.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericMentalSkills.Name = "numericMentalSkills";
		this.numericMentalSkills.Size = new System.Drawing.Size(44, 22);
		this.numericMentalSkills.TabIndex = 0;
		this.numericMentalSkills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericMentalSkills.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericMentalSkills.ValueChanged += new System.EventHandler(numericMentalSkills_ValueChanged);
		this.labelPlayerPositioning.BackColor = System.Drawing.SystemColors.Control;
		this.labelPlayerPositioning.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelPlayerPositioning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPlayerPositioning.ForeColor = System.Drawing.Color.Yellow;
		this.labelPlayerPositioning.Image = (System.Drawing.Image)resources.GetObject("labelPlayerPositioning.Image");
		this.labelPlayerPositioning.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPlayerPositioning.Location = new System.Drawing.Point(16, 86);
		this.labelPlayerPositioning.Name = "labelPlayerPositioning";
		this.labelPlayerPositioning.Size = new System.Drawing.Size(112, 16);
		this.labelPlayerPositioning.TabIndex = 120;
		this.labelPlayerPositioning.Text = "Positioning ";
		this.labelPlayerPositioning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPotential.BackColor = System.Drawing.SystemColors.Control;
		this.labelPotential.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelPotential.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPotential.ForeColor = System.Drawing.Color.Yellow;
		this.labelPotential.Image = (System.Drawing.Image)resources.GetObject("labelPotential.Image");
		this.labelPotential.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPotential.Location = new System.Drawing.Point(16, 38);
		this.labelPotential.Name = "labelPotential";
		this.labelPotential.Size = new System.Drawing.Size(112, 16);
		this.labelPotential.TabIndex = 118;
		this.labelPotential.Text = "Potential ";
		this.labelPotential.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackPlayerPositioning.BackColor = System.Drawing.SystemColors.Control;
		this.trackPlayerPositioning.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackPlayerPositioning.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "positioning", true));
		this.trackPlayerPositioning.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackPlayerPositioning.LargeChange = 10;
		this.trackPlayerPositioning.Location = new System.Drawing.Point(8, 94);
		this.trackPlayerPositioning.Maximum = 99;
		this.trackPlayerPositioning.Minimum = 1;
		this.trackPlayerPositioning.Name = "trackPlayerPositioning";
		this.trackPlayerPositioning.Size = new System.Drawing.Size(128, 45);
		this.trackPlayerPositioning.TabIndex = 3;
		this.trackPlayerPositioning.TickFrequency = 10;
		this.trackPlayerPositioning.Value = 1;
		this.trackPlayerPositioning.ValueChanged += new System.EventHandler(trackPlayerPositioning_ValueChanged);
		this.trackPotential.BackColor = System.Drawing.SystemColors.Control;
		this.trackPotential.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackPotential.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "potential", true));
		this.trackPotential.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackPotential.LargeChange = 10;
		this.trackPotential.Location = new System.Drawing.Point(8, 46);
		this.trackPotential.Maximum = 99;
		this.trackPotential.Minimum = 1;
		this.trackPotential.Name = "trackPotential";
		this.trackPotential.Size = new System.Drawing.Size(128, 45);
		this.trackPotential.TabIndex = 1;
		this.trackPotential.TickFrequency = 10;
		this.trackPotential.Value = 1;
		this.trackPotential.ValueChanged += new System.EventHandler(trackPotential_ValueChanged);
		this.groupAttackingSkills.BackColor = System.Drawing.SystemColors.Control;
		this.groupAttackingSkills.Controls.Add(this.labelFinishing);
		this.groupAttackingSkills.Controls.Add(this.label6);
		this.groupAttackingSkills.Controls.Add(this.numericUpDown2);
		this.groupAttackingSkills.Controls.Add(this.numericUpDown1);
		this.groupAttackingSkills.Controls.Add(this.labelHeading);
		this.groupAttackingSkills.Controls.Add(this.trackHeading);
		this.groupAttackingSkills.Controls.Add(this.labelVolley);
		this.groupAttackingSkills.Controls.Add(this.trackVolley);
		this.groupAttackingSkills.Controls.Add(this.numericAttackingSkills);
		this.groupAttackingSkills.Controls.Add(this.labelDribbling);
		this.groupAttackingSkills.Controls.Add(this.labelLongShot);
		this.groupAttackingSkills.Controls.Add(this.labelShotPower);
		this.groupAttackingSkills.Controls.Add(this.trackFinishing);
		this.groupAttackingSkills.Controls.Add(this.trackShotPower);
		this.groupAttackingSkills.Controls.Add(this.trackLongShot);
		this.groupAttackingSkills.Controls.Add(this.trackDribbling);
		this.groupAttackingSkills.Location = new System.Drawing.Point(721, 3);
		this.groupAttackingSkills.Name = "groupAttackingSkills";
		this.groupAttackingSkills.Size = new System.Drawing.Size(140, 378);
		this.groupAttackingSkills.TabIndex = 17;
		this.groupAttackingSkills.TabStop = false;
		this.groupAttackingSkills.Text = "Attacking Skills";
		this.labelFinishing.BackColor = System.Drawing.SystemColors.Control;
		this.labelFinishing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelFinishing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelFinishing.ForeColor = System.Drawing.Color.Yellow;
		this.labelFinishing.Image = (System.Drawing.Image)resources.GetObject("labelFinishing.Image");
		this.labelFinishing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFinishing.Location = new System.Drawing.Point(14, 280);
		this.labelFinishing.Name = "labelFinishing";
		this.labelFinishing.Size = new System.Drawing.Size(112, 16);
		this.labelFinishing.TabIndex = 106;
		this.labelFinishing.Text = "Finishing ";
		this.labelFinishing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(31, 327);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(79, 13);
		this.label6.TabIndex = 121;
		this.label6.Text = "Finishing Styles";
		this.numericUpDown2.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "finishingcode2", true));
		this.numericUpDown2.Location = new System.Drawing.Point(74, 348);
		this.numericUpDown2.Maximum = new decimal(new int[4] { 127, 0, 0, 0 });
		this.numericUpDown2.Name = "numericUpDown2";
		this.numericUpDown2.Size = new System.Drawing.Size(58, 20);
		this.numericUpDown2.TabIndex = 120;
		this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown1.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "finishingcode1", true));
		this.numericUpDown1.Location = new System.Drawing.Point(10, 348);
		this.numericUpDown1.Maximum = new decimal(new int[4] { 127, 0, 0, 0 });
		this.numericUpDown1.Name = "numericUpDown1";
		this.numericUpDown1.Size = new System.Drawing.Size(58, 20);
		this.numericUpDown1.TabIndex = 119;
		this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.labelHeading.BackColor = System.Drawing.SystemColors.Control;
		this.labelHeading.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelHeading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelHeading.ForeColor = System.Drawing.Color.Yellow;
		this.labelHeading.Image = (System.Drawing.Image)resources.GetObject("labelHeading.Image");
		this.labelHeading.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHeading.Location = new System.Drawing.Point(14, 230);
		this.labelHeading.Name = "labelHeading";
		this.labelHeading.Size = new System.Drawing.Size(112, 16);
		this.labelHeading.TabIndex = 98;
		this.labelHeading.Text = "Heading ";
		this.labelHeading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackHeading.BackColor = System.Drawing.SystemColors.Control;
		this.trackHeading.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackHeading.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "headingaccuracy", true));
		this.trackHeading.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackHeading.LargeChange = 10;
		this.trackHeading.Location = new System.Drawing.Point(6, 238);
		this.trackHeading.Maximum = 99;
		this.trackHeading.Minimum = 1;
		this.trackHeading.Name = "trackHeading";
		this.trackHeading.Size = new System.Drawing.Size(128, 45);
		this.trackHeading.TabIndex = 7;
		this.trackHeading.TickFrequency = 10;
		this.trackHeading.Value = 1;
		this.trackHeading.ValueChanged += new System.EventHandler(trackHeading_ValueChanged);
		this.labelVolley.BackColor = System.Drawing.SystemColors.Control;
		this.labelVolley.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelVolley.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelVolley.ForeColor = System.Drawing.Color.Yellow;
		this.labelVolley.Image = (System.Drawing.Image)resources.GetObject("labelVolley.Image");
		this.labelVolley.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelVolley.Location = new System.Drawing.Point(14, 182);
		this.labelVolley.Name = "labelVolley";
		this.labelVolley.Size = new System.Drawing.Size(112, 16);
		this.labelVolley.TabIndex = 118;
		this.labelVolley.Text = "Volley ";
		this.labelVolley.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackVolley.BackColor = System.Drawing.SystemColors.Control;
		this.trackVolley.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackVolley.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "volleys", true));
		this.trackVolley.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackVolley.LargeChange = 10;
		this.trackVolley.Location = new System.Drawing.Point(6, 190);
		this.trackVolley.Maximum = 99;
		this.trackVolley.Minimum = 1;
		this.trackVolley.Name = "trackVolley";
		this.trackVolley.Size = new System.Drawing.Size(128, 45);
		this.trackVolley.TabIndex = 6;
		this.trackVolley.TickFrequency = 10;
		this.trackVolley.Value = 1;
		this.trackVolley.ValueChanged += new System.EventHandler(trackVolley_ValueChanged);
		this.numericAttackingSkills.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericAttackingSkills.BackColor = System.Drawing.Color.Teal;
		this.numericAttackingSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold);
		this.numericAttackingSkills.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.numericAttackingSkills.Location = new System.Drawing.Point(43, 15);
		this.numericAttackingSkills.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericAttackingSkills.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericAttackingSkills.Name = "numericAttackingSkills";
		this.numericAttackingSkills.Size = new System.Drawing.Size(44, 22);
		this.numericAttackingSkills.TabIndex = 0;
		this.numericAttackingSkills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericAttackingSkills.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericAttackingSkills.ValueChanged += new System.EventHandler(numericAttackingSkills_ValueChanged);
		this.labelDribbling.BackColor = System.Drawing.SystemColors.Control;
		this.labelDribbling.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelDribbling.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelDribbling.ForeColor = System.Drawing.Color.Yellow;
		this.labelDribbling.Image = (System.Drawing.Image)resources.GetObject("labelDribbling.Image");
		this.labelDribbling.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDribbling.Location = new System.Drawing.Point(14, 136);
		this.labelDribbling.Name = "labelDribbling";
		this.labelDribbling.Size = new System.Drawing.Size(112, 16);
		this.labelDribbling.TabIndex = 82;
		this.labelDribbling.Text = "Dribbling ";
		this.labelDribbling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelLongShot.BackColor = System.Drawing.SystemColors.Control;
		this.labelLongShot.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelLongShot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelLongShot.ForeColor = System.Drawing.Color.Yellow;
		this.labelLongShot.Image = (System.Drawing.Image)resources.GetObject("labelLongShot.Image");
		this.labelLongShot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLongShot.Location = new System.Drawing.Point(14, 88);
		this.labelLongShot.Name = "labelLongShot";
		this.labelLongShot.Size = new System.Drawing.Size(112, 16);
		this.labelLongShot.TabIndex = 104;
		this.labelLongShot.Text = "Long-Shot ";
		this.labelLongShot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShotPower.BackColor = System.Drawing.SystemColors.Control;
		this.labelShotPower.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelShotPower.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelShotPower.ForeColor = System.Drawing.Color.Yellow;
		this.labelShotPower.Image = (System.Drawing.Image)resources.GetObject("labelShotPower.Image");
		this.labelShotPower.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelShotPower.Location = new System.Drawing.Point(14, 40);
		this.labelShotPower.Name = "labelShotPower";
		this.labelShotPower.Size = new System.Drawing.Size(112, 16);
		this.labelShotPower.TabIndex = 108;
		this.labelShotPower.Text = "Shot-Power ";
		this.labelShotPower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackFinishing.BackColor = System.Drawing.SystemColors.Control;
		this.trackFinishing.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackFinishing.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "finishing", true));
		this.trackFinishing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackFinishing.LargeChange = 10;
		this.trackFinishing.Location = new System.Drawing.Point(6, 288);
		this.trackFinishing.Maximum = 99;
		this.trackFinishing.Minimum = 1;
		this.trackFinishing.Name = "trackFinishing";
		this.trackFinishing.Size = new System.Drawing.Size(128, 45);
		this.trackFinishing.TabIndex = 1;
		this.trackFinishing.TickFrequency = 10;
		this.trackFinishing.Value = 1;
		this.trackFinishing.ValueChanged += new System.EventHandler(trackFinishing_ValueChanged);
		this.trackShotPower.BackColor = System.Drawing.SystemColors.Control;
		this.trackShotPower.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackShotPower.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "shotpower", true));
		this.trackShotPower.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackShotPower.LargeChange = 10;
		this.trackShotPower.Location = new System.Drawing.Point(6, 48);
		this.trackShotPower.Maximum = 99;
		this.trackShotPower.Minimum = 1;
		this.trackShotPower.Name = "trackShotPower";
		this.trackShotPower.Size = new System.Drawing.Size(128, 45);
		this.trackShotPower.TabIndex = 2;
		this.trackShotPower.TickFrequency = 10;
		this.trackShotPower.Value = 1;
		this.trackShotPower.ValueChanged += new System.EventHandler(trackShotPower_ValueChanged);
		this.trackLongShot.BackColor = System.Drawing.SystemColors.Control;
		this.trackLongShot.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackLongShot.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "longshots", true));
		this.trackLongShot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackLongShot.LargeChange = 10;
		this.trackLongShot.Location = new System.Drawing.Point(6, 96);
		this.trackLongShot.Maximum = 99;
		this.trackLongShot.Minimum = 1;
		this.trackLongShot.Name = "trackLongShot";
		this.trackLongShot.Size = new System.Drawing.Size(128, 45);
		this.trackLongShot.TabIndex = 3;
		this.trackLongShot.TickFrequency = 10;
		this.trackLongShot.Value = 1;
		this.trackLongShot.ValueChanged += new System.EventHandler(trackLongShot_ValueChanged);
		this.trackDribbling.BackColor = System.Drawing.SystemColors.Control;
		this.trackDribbling.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackDribbling.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "dribbling", true));
		this.trackDribbling.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackDribbling.LargeChange = 10;
		this.trackDribbling.Location = new System.Drawing.Point(6, 144);
		this.trackDribbling.Maximum = 99;
		this.trackDribbling.Minimum = 1;
		this.trackDribbling.Name = "trackDribbling";
		this.trackDribbling.Size = new System.Drawing.Size(128, 45);
		this.trackDribbling.TabIndex = 4;
		this.trackDribbling.TickFrequency = 10;
		this.trackDribbling.Value = 1;
		this.trackDribbling.ValueChanged += new System.EventHandler(trackDribbling_ValueChanged);
		this.groupGenericAttributes.BackColor = System.Drawing.SystemColors.Control;
		this.groupGenericAttributes.Controls.Add(this.label7);
		this.groupGenericAttributes.Controls.Add(this.numericUpDown3);
		this.groupGenericAttributes.Controls.Add(this.numericUpDown4);
		this.groupGenericAttributes.Controls.Add(this.labelJumping);
		this.groupGenericAttributes.Controls.Add(this.labelBalance);
		this.groupGenericAttributes.Controls.Add(this.trackBalance);
		this.groupGenericAttributes.Controls.Add(this.labelAgility);
		this.groupGenericAttributes.Controls.Add(this.trackAgility);
		this.groupGenericAttributes.Controls.Add(this.numericPhysicalSkills);
		this.groupGenericAttributes.Controls.Add(this.labelReactions);
		this.groupGenericAttributes.Controls.Add(this.labelStrength);
		this.groupGenericAttributes.Controls.Add(this.labelStamina);
		this.groupGenericAttributes.Controls.Add(this.trackStamina);
		this.groupGenericAttributes.Controls.Add(this.labelSprintSpeed);
		this.groupGenericAttributes.Controls.Add(this.trackSprintSpeed);
		this.groupGenericAttributes.Controls.Add(this.labelAcceleration);
		this.groupGenericAttributes.Controls.Add(this.trackAcceleration);
		this.groupGenericAttributes.Controls.Add(this.trackStrength);
		this.groupGenericAttributes.Controls.Add(this.trackReactions);
		this.groupGenericAttributes.Controls.Add(this.trackJumping);
		this.groupGenericAttributes.Location = new System.Drawing.Point(867, 3);
		this.groupGenericAttributes.Name = "groupGenericAttributes";
		this.groupGenericAttributes.Size = new System.Drawing.Size(268, 378);
		this.groupGenericAttributes.TabIndex = 18;
		this.groupGenericAttributes.TabStop = false;
		this.groupGenericAttributes.Text = "Physical Skills";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(30, 327);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(78, 13);
		this.label7.TabIndex = 133;
		this.label7.Text = "Running Styles";
		this.numericUpDown3.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "runningcode2", true));
		this.numericUpDown3.Location = new System.Drawing.Point(73, 348);
		this.numericUpDown3.Maximum = new decimal(new int[4] { 127, 0, 0, 0 });
		this.numericUpDown3.Name = "numericUpDown3";
		this.numericUpDown3.Size = new System.Drawing.Size(58, 20);
		this.numericUpDown3.TabIndex = 132;
		this.numericUpDown3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown4.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "runningcode1", true));
		this.numericUpDown4.Location = new System.Drawing.Point(9, 348);
		this.numericUpDown4.Maximum = new decimal(new int[4] { 127, 0, 0, 0 });
		this.numericUpDown4.Name = "numericUpDown4";
		this.numericUpDown4.Size = new System.Drawing.Size(58, 20);
		this.numericUpDown4.TabIndex = 131;
		this.numericUpDown4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.labelJumping.BackColor = System.Drawing.SystemColors.Control;
		this.labelJumping.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelJumping.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelJumping.ForeColor = System.Drawing.Color.Yellow;
		this.labelJumping.Image = (System.Drawing.Image)resources.GetObject("labelJumping.Image");
		this.labelJumping.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelJumping.Location = new System.Drawing.Point(12, 280);
		this.labelJumping.Name = "labelJumping";
		this.labelJumping.Size = new System.Drawing.Size(112, 16);
		this.labelJumping.TabIndex = 130;
		this.labelJumping.Text = "Jumping ";
		this.labelJumping.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelBalance.BackColor = System.Drawing.SystemColors.Control;
		this.labelBalance.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelBalance.ForeColor = System.Drawing.Color.Yellow;
		this.labelBalance.Image = (System.Drawing.Image)resources.GetObject("labelBalance.Image");
		this.labelBalance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBalance.Location = new System.Drawing.Point(148, 86);
		this.labelBalance.Name = "labelBalance";
		this.labelBalance.Size = new System.Drawing.Size(112, 16);
		this.labelBalance.TabIndex = 128;
		this.labelBalance.Text = "Balance ";
		this.labelBalance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackBalance.BackColor = System.Drawing.SystemColors.Control;
		this.trackBalance.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackBalance.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "balance", true));
		this.trackBalance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackBalance.LargeChange = 10;
		this.trackBalance.Location = new System.Drawing.Point(140, 94);
		this.trackBalance.Maximum = 99;
		this.trackBalance.Minimum = 1;
		this.trackBalance.Name = "trackBalance";
		this.trackBalance.Size = new System.Drawing.Size(128, 45);
		this.trackBalance.TabIndex = 8;
		this.trackBalance.TickFrequency = 10;
		this.trackBalance.Value = 1;
		this.trackBalance.ValueChanged += new System.EventHandler(trackBalance_ValueChanged);
		this.labelAgility.BackColor = System.Drawing.SystemColors.Control;
		this.labelAgility.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelAgility.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelAgility.ForeColor = System.Drawing.Color.Yellow;
		this.labelAgility.Image = (System.Drawing.Image)resources.GetObject("labelAgility.Image");
		this.labelAgility.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelAgility.Location = new System.Drawing.Point(12, 232);
		this.labelAgility.Name = "labelAgility";
		this.labelAgility.Size = new System.Drawing.Size(112, 16);
		this.labelAgility.TabIndex = 126;
		this.labelAgility.Text = "Agility ";
		this.labelAgility.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackAgility.BackColor = System.Drawing.SystemColors.Control;
		this.trackAgility.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackAgility.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "agility", true));
		this.trackAgility.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackAgility.LargeChange = 10;
		this.trackAgility.Location = new System.Drawing.Point(4, 240);
		this.trackAgility.Maximum = 99;
		this.trackAgility.Minimum = 1;
		this.trackAgility.Name = "trackAgility";
		this.trackAgility.Size = new System.Drawing.Size(128, 45);
		this.trackAgility.TabIndex = 5;
		this.trackAgility.TickFrequency = 10;
		this.trackAgility.Value = 1;
		this.trackAgility.ValueChanged += new System.EventHandler(trackAgility_ValueChanged);
		this.numericPhysicalSkills.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericPhysicalSkills.BackColor = System.Drawing.Color.Teal;
		this.numericPhysicalSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold);
		this.numericPhysicalSkills.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.numericPhysicalSkills.Location = new System.Drawing.Point(114, 15);
		this.numericPhysicalSkills.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericPhysicalSkills.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPhysicalSkills.Name = "numericPhysicalSkills";
		this.numericPhysicalSkills.Size = new System.Drawing.Size(44, 22);
		this.numericPhysicalSkills.TabIndex = 0;
		this.numericPhysicalSkills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPhysicalSkills.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericPhysicalSkills.ValueChanged += new System.EventHandler(numericGenericSkills_ValueChanged);
		this.labelReactions.BackColor = System.Drawing.SystemColors.Control;
		this.labelReactions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelReactions.ForeColor = System.Drawing.Color.Yellow;
		this.labelReactions.Image = (System.Drawing.Image)resources.GetObject("labelReactions.Image");
		this.labelReactions.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelReactions.Location = new System.Drawing.Point(148, 40);
		this.labelReactions.Name = "labelReactions";
		this.labelReactions.Size = new System.Drawing.Size(112, 16);
		this.labelReactions.TabIndex = 82;
		this.labelReactions.Text = "Reactions ";
		this.labelReactions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelStrength.BackColor = System.Drawing.SystemColors.Control;
		this.labelStrength.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelStrength.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelStrength.ForeColor = System.Drawing.Color.Yellow;
		this.labelStrength.Image = (System.Drawing.Image)resources.GetObject("labelStrength.Image");
		this.labelStrength.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStrength.Location = new System.Drawing.Point(12, 184);
		this.labelStrength.Name = "labelStrength";
		this.labelStrength.Size = new System.Drawing.Size(112, 16);
		this.labelStrength.TabIndex = 73;
		this.labelStrength.Text = "Strength ";
		this.labelStrength.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelStamina.BackColor = System.Drawing.SystemColors.Control;
		this.labelStamina.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelStamina.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelStamina.ForeColor = System.Drawing.Color.Yellow;
		this.labelStamina.Image = (System.Drawing.Image)resources.GetObject("labelStamina.Image");
		this.labelStamina.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStamina.Location = new System.Drawing.Point(12, 134);
		this.labelStamina.Name = "labelStamina";
		this.labelStamina.Size = new System.Drawing.Size(112, 16);
		this.labelStamina.TabIndex = 71;
		this.labelStamina.Text = "Stamina ";
		this.labelStamina.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackStamina.BackColor = System.Drawing.SystemColors.Control;
		this.trackStamina.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackStamina.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "stamina", true));
		this.trackStamina.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackStamina.LargeChange = 10;
		this.trackStamina.Location = new System.Drawing.Point(4, 142);
		this.trackStamina.Maximum = 99;
		this.trackStamina.Minimum = 1;
		this.trackStamina.Name = "trackStamina";
		this.trackStamina.Size = new System.Drawing.Size(128, 45);
		this.trackStamina.TabIndex = 3;
		this.trackStamina.TickFrequency = 10;
		this.trackStamina.Value = 1;
		this.trackStamina.ValueChanged += new System.EventHandler(trackStamina_ValueChanged);
		this.labelSprintSpeed.BackColor = System.Drawing.SystemColors.Control;
		this.labelSprintSpeed.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelSprintSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelSprintSpeed.ForeColor = System.Drawing.Color.Yellow;
		this.labelSprintSpeed.Image = (System.Drawing.Image)resources.GetObject("labelSprintSpeed.Image");
		this.labelSprintSpeed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSprintSpeed.Location = new System.Drawing.Point(12, 88);
		this.labelSprintSpeed.Name = "labelSprintSpeed";
		this.labelSprintSpeed.Size = new System.Drawing.Size(112, 16);
		this.labelSprintSpeed.TabIndex = 69;
		this.labelSprintSpeed.Text = "Sprint-Speed ";
		this.labelSprintSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackSprintSpeed.BackColor = System.Drawing.SystemColors.Control;
		this.trackSprintSpeed.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackSprintSpeed.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "sprintspeed", true));
		this.trackSprintSpeed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackSprintSpeed.LargeChange = 10;
		this.trackSprintSpeed.Location = new System.Drawing.Point(4, 96);
		this.trackSprintSpeed.Maximum = 99;
		this.trackSprintSpeed.Minimum = 1;
		this.trackSprintSpeed.Name = "trackSprintSpeed";
		this.trackSprintSpeed.Size = new System.Drawing.Size(128, 45);
		this.trackSprintSpeed.TabIndex = 2;
		this.trackSprintSpeed.TickFrequency = 10;
		this.trackSprintSpeed.Value = 1;
		this.trackSprintSpeed.ValueChanged += new System.EventHandler(trackSprintSpeed_ValueChanged);
		this.labelAcceleration.BackColor = System.Drawing.SystemColors.Control;
		this.labelAcceleration.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelAcceleration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelAcceleration.ForeColor = System.Drawing.Color.Yellow;
		this.labelAcceleration.Image = (System.Drawing.Image)resources.GetObject("labelAcceleration.Image");
		this.labelAcceleration.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelAcceleration.Location = new System.Drawing.Point(12, 40);
		this.labelAcceleration.Name = "labelAcceleration";
		this.labelAcceleration.Size = new System.Drawing.Size(112, 16);
		this.labelAcceleration.TabIndex = 65;
		this.labelAcceleration.Text = "Acceleration ";
		this.labelAcceleration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackAcceleration.BackColor = System.Drawing.SystemColors.Control;
		this.trackAcceleration.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackAcceleration.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "acceleration", true));
		this.trackAcceleration.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackAcceleration.LargeChange = 10;
		this.trackAcceleration.Location = new System.Drawing.Point(4, 48);
		this.trackAcceleration.Maximum = 99;
		this.trackAcceleration.Minimum = 1;
		this.trackAcceleration.Name = "trackAcceleration";
		this.trackAcceleration.Size = new System.Drawing.Size(128, 45);
		this.trackAcceleration.TabIndex = 1;
		this.trackAcceleration.TickFrequency = 10;
		this.trackAcceleration.Value = 1;
		this.trackAcceleration.ValueChanged += new System.EventHandler(trackAcceleration_ValueChanged);
		this.trackStrength.BackColor = System.Drawing.SystemColors.Control;
		this.trackStrength.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackStrength.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "strength", true));
		this.trackStrength.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackStrength.LargeChange = 10;
		this.trackStrength.Location = new System.Drawing.Point(4, 192);
		this.trackStrength.Maximum = 99;
		this.trackStrength.Minimum = 1;
		this.trackStrength.Name = "trackStrength";
		this.trackStrength.Size = new System.Drawing.Size(128, 45);
		this.trackStrength.TabIndex = 4;
		this.trackStrength.TickFrequency = 10;
		this.trackStrength.Value = 1;
		this.trackStrength.ValueChanged += new System.EventHandler(trackStrength_ValueChanged);
		this.trackReactions.BackColor = System.Drawing.SystemColors.Control;
		this.trackReactions.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "reactions", true));
		this.trackReactions.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackReactions.LargeChange = 10;
		this.trackReactions.Location = new System.Drawing.Point(139, 48);
		this.trackReactions.Maximum = 99;
		this.trackReactions.Minimum = 1;
		this.trackReactions.Name = "trackReactions";
		this.trackReactions.Size = new System.Drawing.Size(128, 45);
		this.trackReactions.TabIndex = 7;
		this.trackReactions.TickFrequency = 10;
		this.trackReactions.Value = 1;
		this.trackReactions.ValueChanged += new System.EventHandler(trackReactions_ValueChanged);
		this.trackJumping.BackColor = System.Drawing.SystemColors.Control;
		this.trackJumping.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackJumping.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "jumping", true));
		this.trackJumping.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackJumping.LargeChange = 10;
		this.trackJumping.Location = new System.Drawing.Point(4, 288);
		this.trackJumping.Maximum = 99;
		this.trackJumping.Minimum = 1;
		this.trackJumping.Name = "trackJumping";
		this.trackJumping.Size = new System.Drawing.Size(128, 45);
		this.trackJumping.TabIndex = 6;
		this.trackJumping.TickFrequency = 10;
		this.trackJumping.Value = 1;
		this.trackJumping.ValueChanged += new System.EventHandler(trackJumping_ValueChanged);
		this.groupFreeKick.BackColor = System.Drawing.SystemColors.Control;
		this.groupFreeKick.Controls.Add(this.labelSkillsStars);
		this.groupFreeKick.Controls.Add(this.numericSkillMoves);
		this.groupFreeKick.Controls.Add(this.labelSkillMoves);
		this.groupFreeKick.Controls.Add(this.numericFreeKickSkills);
		this.groupFreeKick.Controls.Add(this.labelPenalties);
		this.groupFreeKick.Controls.Add(this.labelFreeKick);
		this.groupFreeKick.Controls.Add(this.trackFreeKick);
		this.groupFreeKick.Controls.Add(this.trackPenalties);
		this.groupFreeKick.Controls.Add(this.labelPenaltyKick);
		this.groupFreeKick.Controls.Add(this.comboPenaltyKick);
		this.groupFreeKick.Controls.Add(this.labelPenaltyMove);
		this.groupFreeKick.Controls.Add(this.comboPenaltyMove);
		this.groupFreeKick.Controls.Add(this.labelFreeKickStart);
		this.groupFreeKick.Controls.Add(this.labelPenaltyStart);
		this.groupFreeKick.Controls.Add(this.comboFreeKickStart);
		this.groupFreeKick.Controls.Add(this.comboPenaltyStart);
		this.groupFreeKick.Location = new System.Drawing.Point(3, 387);
		this.groupFreeKick.Name = "groupFreeKick";
		this.groupFreeKick.Size = new System.Drawing.Size(250, 309);
		this.groupFreeKick.TabIndex = 28;
		this.groupFreeKick.TabStop = false;
		this.groupFreeKick.Text = "Free Kick Skills";
		this.labelSkillsStars.ImageList = this.imageListStars;
		this.labelSkillsStars.Location = new System.Drawing.Point(118, 148);
		this.labelSkillsStars.Name = "labelSkillsStars";
		this.labelSkillsStars.Size = new System.Drawing.Size(117, 23);
		this.labelSkillsStars.TabIndex = 156;
		this.imageListStars.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListStars.ImageStream");
		this.imageListStars.TransparentColor = System.Drawing.Color.Fuchsia;
		this.imageListStars.Images.SetKeyName(0, "Stars_1.PNG");
		this.imageListStars.Images.SetKeyName(1, "Stars_2.PNG");
		this.imageListStars.Images.SetKeyName(2, "Stars_3.PNG");
		this.imageListStars.Images.SetKeyName(3, "Stars_4.PNG");
		this.imageListStars.Images.SetKeyName(4, "Stars_5.PNG");
		this.numericSkillMoves.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "skillmoves", true));
		this.numericSkillMoves.Location = new System.Drawing.Point(69, 151);
		this.numericSkillMoves.Maximum = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericSkillMoves.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericSkillMoves.Name = "numericSkillMoves";
		this.numericSkillMoves.Size = new System.Drawing.Size(43, 20);
		this.numericSkillMoves.TabIndex = 3;
		this.numericSkillMoves.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericSkillMoves.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericSkillMoves.ValueChanged += new System.EventHandler(numericSkillMoves_ValueChanged);
		this.labelSkillMoves.AutoSize = true;
		this.labelSkillMoves.Location = new System.Drawing.Point(8, 153);
		this.labelSkillMoves.Name = "labelSkillMoves";
		this.labelSkillMoves.Size = new System.Drawing.Size(61, 13);
		this.labelSkillMoves.TabIndex = 154;
		this.labelSkillMoves.Text = "Skill Moves";
		this.numericFreeKickSkills.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericFreeKickSkills.BackColor = System.Drawing.Color.Teal;
		this.numericFreeKickSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold);
		this.numericFreeKickSkills.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.numericFreeKickSkills.Location = new System.Drawing.Point(50, 15);
		this.numericFreeKickSkills.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericFreeKickSkills.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericFreeKickSkills.Name = "numericFreeKickSkills";
		this.numericFreeKickSkills.Size = new System.Drawing.Size(44, 22);
		this.numericFreeKickSkills.TabIndex = 0;
		this.numericFreeKickSkills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericFreeKickSkills.Value = new decimal(new int[4] { 99, 0, 0, 0 });
		this.numericFreeKickSkills.ValueChanged += new System.EventHandler(numericFreeKickSkills_ValueChanged);
		this.labelPenalties.BackColor = System.Drawing.SystemColors.Control;
		this.labelPenalties.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelPenalties.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPenalties.ForeColor = System.Drawing.Color.Yellow;
		this.labelPenalties.Image = (System.Drawing.Image)resources.GetObject("labelPenalties.Image");
		this.labelPenalties.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPenalties.Location = new System.Drawing.Point(16, 88);
		this.labelPenalties.Name = "labelPenalties";
		this.labelPenalties.Size = new System.Drawing.Size(112, 16);
		this.labelPenalties.TabIndex = 116;
		this.labelPenalties.Text = "Penalties ";
		this.labelPenalties.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelFreeKick.BackColor = System.Drawing.SystemColors.Control;
		this.labelFreeKick.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelFreeKick.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelFreeKick.ForeColor = System.Drawing.Color.Yellow;
		this.labelFreeKick.Image = (System.Drawing.Image)resources.GetObject("labelFreeKick.Image");
		this.labelFreeKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFreeKick.Location = new System.Drawing.Point(16, 40);
		this.labelFreeKick.Name = "labelFreeKick";
		this.labelFreeKick.Size = new System.Drawing.Size(112, 16);
		this.labelFreeKick.TabIndex = 112;
		this.labelFreeKick.Text = "Free-Kick ";
		this.labelFreeKick.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.trackFreeKick.BackColor = System.Drawing.SystemColors.Control;
		this.trackFreeKick.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackFreeKick.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "freekickaccuracy", true));
		this.trackFreeKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackFreeKick.LargeChange = 10;
		this.trackFreeKick.Location = new System.Drawing.Point(8, 48);
		this.trackFreeKick.Maximum = 99;
		this.trackFreeKick.Minimum = 1;
		this.trackFreeKick.Name = "trackFreeKick";
		this.trackFreeKick.Size = new System.Drawing.Size(128, 45);
		this.trackFreeKick.TabIndex = 1;
		this.trackFreeKick.TickFrequency = 10;
		this.trackFreeKick.Value = 1;
		this.trackFreeKick.ValueChanged += new System.EventHandler(trackFreeKick_ValueChanged);
		this.trackPenalties.BackColor = System.Drawing.SystemColors.Control;
		this.trackPenalties.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.trackPenalties.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.playerBindingSource, "penalties", true));
		this.trackPenalties.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.trackPenalties.LargeChange = 10;
		this.trackPenalties.Location = new System.Drawing.Point(8, 96);
		this.trackPenalties.Maximum = 99;
		this.trackPenalties.Minimum = 1;
		this.trackPenalties.Name = "trackPenalties";
		this.trackPenalties.Size = new System.Drawing.Size(128, 45);
		this.trackPenalties.TabIndex = 2;
		this.trackPenalties.TickFrequency = 10;
		this.trackPenalties.Value = 1;
		this.trackPenalties.ValueChanged += new System.EventHandler(trackPenalties_ValueChanged);
		this.labelPenaltyKick.AutoSize = true;
		this.labelPenaltyKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPenaltyKick.Location = new System.Drawing.Point(6, 259);
		this.labelPenaltyKick.Name = "labelPenaltyKick";
		this.labelPenaltyKick.Size = new System.Drawing.Size(66, 13);
		this.labelPenaltyKick.TabIndex = 153;
		this.labelPenaltyKick.Text = "Penalty Kick";
		this.labelPenaltyKick.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboPenaltyKick.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "animpenaltieskickstylecode", true));
		this.comboPenaltyKick.FormattingEnabled = true;
		this.comboPenaltyKick.Items.AddRange(new object[3] { "Normal", "Finesse Shot", "Powerful Shot" });
		this.comboPenaltyKick.Location = new System.Drawing.Point(89, 253);
		this.comboPenaltyKick.Name = "comboPenaltyKick";
		this.comboPenaltyKick.Size = new System.Drawing.Size(139, 21);
		this.comboPenaltyKick.TabIndex = 7;
		this.labelPenaltyMove.AutoSize = true;
		this.labelPenaltyMove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPenaltyMove.Location = new System.Drawing.Point(6, 235);
		this.labelPenaltyMove.Name = "labelPenaltyMove";
		this.labelPenaltyMove.Size = new System.Drawing.Size(72, 13);
		this.labelPenaltyMove.TabIndex = 151;
		this.labelPenaltyMove.Text = "Penalty Move";
		this.labelPenaltyMove.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboPenaltyMove.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "animpenaltiesmotionstylecode", true));
		this.comboPenaltyMove.FormattingEnabled = true;
		this.comboPenaltyMove.Items.AddRange(new object[7] { "Continuous Motion", "Start/Stop Motion", "Henry's style", "Unknown style", "Lampard's style", "Podolski's style", "Ronaldinho's style" });
		this.comboPenaltyMove.Location = new System.Drawing.Point(89, 229);
		this.comboPenaltyMove.Name = "comboPenaltyMove";
		this.comboPenaltyMove.Size = new System.Drawing.Size(139, 21);
		this.comboPenaltyMove.TabIndex = 6;
		this.labelFreeKickStart.AutoSize = true;
		this.labelFreeKickStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFreeKickStart.Location = new System.Drawing.Point(6, 189);
		this.labelFreeKickStart.Name = "labelFreeKickStart";
		this.labelFreeKickStart.Size = new System.Drawing.Size(77, 13);
		this.labelFreeKickStart.TabIndex = 147;
		this.labelFreeKickStart.Text = "Free Kick Start";
		this.labelFreeKickStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelPenaltyStart.AutoSize = true;
		this.labelPenaltyStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPenaltyStart.Location = new System.Drawing.Point(6, 212);
		this.labelPenaltyStart.Name = "labelPenaltyStart";
		this.labelPenaltyStart.Size = new System.Drawing.Size(67, 13);
		this.labelPenaltyStart.TabIndex = 149;
		this.labelPenaltyStart.Text = "Penalty Start";
		this.labelPenaltyStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboFreeKickStart.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "animfreekickstartposcode", true));
		this.comboFreeKickStart.FormattingEnabled = true;
		this.comboFreeKickStart.Items.AddRange(new object[10] { "Normal", "Long run-up", "90 degrees from ball", "Henry's style", "Beckham's style", "Lampard's style", "Adriano's style", "Cristiano Ronaldo's style", "Juninho's style", "Ronaldinho's style" });
		this.comboFreeKickStart.Location = new System.Drawing.Point(89, 183);
		this.comboFreeKickStart.Name = "comboFreeKickStart";
		this.comboFreeKickStart.Size = new System.Drawing.Size(139, 21);
		this.comboFreeKickStart.TabIndex = 4;
		this.comboPenaltyStart.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.playerBindingSource, "animpenaltiesstartposcode", true));
		this.comboPenaltyStart.FormattingEnabled = true;
		this.comboPenaltyStart.Items.AddRange(new object[9] { "Edge of the penalty box", "Close to the ball", "Outside the penalty box", "Henry's style", "Unknown style", "Lampard's style", "Podolski's style", "Ronaldinho's style", "Cristiano Ronaldo's style" });
		this.comboPenaltyStart.Location = new System.Drawing.Point(89, 206);
		this.comboPenaltyStart.Name = "comboPenaltyStart";
		this.comboPenaltyStart.Size = new System.Drawing.Size(139, 21);
		this.comboPenaltyStart.TabIndex = 5;
		this.groupTraits.Controls.Add(this.groupBox2);
		this.groupTraits.Controls.Add(this.checkGKOneonOne);
		this.groupTraits.Controls.Add(this.checkAcrobaticClearance);
		this.groupTraits.Controls.Add(this.checkSecondWind);
		this.groupTraits.Controls.Add(this.checkCrowdFavourite);
		this.groupTraits.Controls.Add(this.checkInflexible);
		this.groupTraits.Controls.Add(this.checkTeamPlayer);
		this.groupTraits.Controls.Add(this.checkSwervePasser);
		this.groupTraits.Controls.Add(this.checkCornerSpecialist);
		this.groupTraits.Controls.Add(this.checkPowerHeader);
		this.groupTraits.Controls.Add(this.checkGkLongThrower);
		this.groupTraits.Controls.Add(this.checkLongPasser);
		this.groupTraits.Controls.Add(this.checkFlair);
		this.groupTraits.Controls.Add(this.checkFinesseShot);
		this.groupTraits.Controls.Add(this.checkArguesWithOfficials);
		this.groupTraits.Controls.Add(this.checkBeatsOffsideTrap);
		this.groupTraits.Controls.Add(this.checkAvoidsWeakFoot);
		this.groupTraits.Controls.Add(this.checkInjuryFree);
		this.groupTraits.Controls.Add(this.checkPowerFreeKick);
		this.groupTraits.Controls.Add(this.checkSelfish);
		this.groupTraits.Controls.Add(this.checkPlaymaker);
		this.groupTraits.Controls.Add(this.checkSpeedDribbler);
		this.groupTraits.Controls.Add(this.checkLeadership);
		this.groupTraits.Controls.Add(this.checkPuncher);
		this.groupTraits.Controls.Add(this.checkDiver);
		this.groupTraits.Controls.Add(this.checkDivesintotackles);
		this.groupTraits.Controls.Add(this.checkLongshottaker);
		this.groupTraits.Controls.Add(this.checkHighClubIdentification);
		this.groupTraits.Controls.Add(this.checkPushesupforcorners);
		this.groupTraits.Controls.Add(this.checkEarlycrosser);
		this.groupTraits.Controls.Add(this.checkInjuryProne);
		this.groupTraits.Controls.Add(this.checkGiantThrower);
		this.groupTraits.Controls.Add(this.checkLongThrower);
		this.groupTraits.Location = new System.Drawing.Point(259, 387);
		this.groupTraits.Name = "groupTraits";
		this.groupTraits.Size = new System.Drawing.Size(619, 309);
		this.groupTraits.TabIndex = 30;
		this.groupTraits.TabStop = false;
		this.groupTraits.Text = "Traits";
		this.groupBox2.Controls.Add(this.checkTechDribbler);
		this.groupBox2.Controls.Add(this.checkChipShot);
		this.groupBox2.Controls.Add(this.checkGKFlatKick);
		this.groupBox2.Controls.Add(this.checkDrivenPass);
		this.groupBox2.Controls.Add(this.checkDivingHeader);
		this.groupBox2.Controls.Add(this.checkBycicleKick);
		this.groupBox2.Controls.Add(this.checkChipperPenalty);
		this.groupBox2.Controls.Add(this.checkStutterPenalty);
		this.groupBox2.Controls.Add(this.checkFancyFlicks);
		this.groupBox2.Controls.Add(this.checkFancyPasses);
		this.groupBox2.Controls.Add(this.checkFancyFeet);
		this.groupBox2.Location = new System.Drawing.Point(472, 15);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(130, 288);
		this.groupBox2.TabIndex = 57;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Virtual Pro";
		this.checkTechDribbler.AutoSize = true;
		this.checkTechDribbler.BackColor = System.Drawing.Color.Transparent;
		this.checkTechDribbler.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "TechDribbler", true));
		this.checkTechDribbler.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTechDribbler.Location = new System.Drawing.Point(13, 242);
		this.checkTechDribbler.Name = "checkTechDribbler";
		this.checkTechDribbler.Size = new System.Drawing.Size(112, 17);
		this.checkTechDribbler.TabIndex = 53;
		this.checkTechDribbler.Text = "Technical Dribbler";
		this.checkTechDribbler.UseVisualStyleBackColor = false;
		this.checkChipShot.AutoSize = true;
		this.checkChipShot.BackColor = System.Drawing.Color.Transparent;
		this.checkChipShot.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "ChipShot", true));
		this.checkChipShot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkChipShot.Location = new System.Drawing.Point(13, 219);
		this.checkChipShot.Name = "checkChipShot";
		this.checkChipShot.Size = new System.Drawing.Size(72, 17);
		this.checkChipShot.TabIndex = 52;
		this.checkChipShot.Text = "Chip Shot";
		this.checkChipShot.UseVisualStyleBackColor = false;
		this.checkGKFlatKick.AutoSize = true;
		this.checkGKFlatKick.BackColor = System.Drawing.Color.Transparent;
		this.checkGKFlatKick.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "GkFlatKick", true));
		this.checkGKFlatKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkGKFlatKick.Location = new System.Drawing.Point(13, 198);
		this.checkGKFlatKick.Name = "checkGKFlatKick";
		this.checkGKFlatKick.Size = new System.Drawing.Size(85, 17);
		this.checkGKFlatKick.TabIndex = 51;
		this.checkGKFlatKick.Text = "GK Flat Kick";
		this.checkGKFlatKick.UseVisualStyleBackColor = false;
		this.checkDrivenPass.AutoSize = true;
		this.checkDrivenPass.BackColor = System.Drawing.Color.Transparent;
		this.checkDrivenPass.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "DrivenPass", true));
		this.checkDrivenPass.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkDrivenPass.Location = new System.Drawing.Point(13, 176);
		this.checkDrivenPass.Name = "checkDrivenPass";
		this.checkDrivenPass.Size = new System.Drawing.Size(83, 17);
		this.checkDrivenPass.TabIndex = 50;
		this.checkDrivenPass.Text = "Driven Pass";
		this.checkDrivenPass.UseVisualStyleBackColor = false;
		this.checkDivingHeader.AutoSize = true;
		this.checkDivingHeader.BackColor = System.Drawing.Color.Transparent;
		this.checkDivingHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "DivingHeader", true));
		this.checkDivingHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkDivingHeader.Location = new System.Drawing.Point(13, 154);
		this.checkDivingHeader.Name = "checkDivingHeader";
		this.checkDivingHeader.Size = new System.Drawing.Size(94, 17);
		this.checkDivingHeader.TabIndex = 49;
		this.checkDivingHeader.Text = "Diving Header";
		this.checkDivingHeader.UseVisualStyleBackColor = false;
		this.checkBycicleKick.AutoSize = true;
		this.checkBycicleKick.BackColor = System.Drawing.Color.Transparent;
		this.checkBycicleKick.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "BycicleKick", true));
		this.checkBycicleKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkBycicleKick.Location = new System.Drawing.Point(13, 132);
		this.checkBycicleKick.Name = "checkBycicleKick";
		this.checkBycicleKick.Size = new System.Drawing.Size(84, 17);
		this.checkBycicleKick.TabIndex = 48;
		this.checkBycicleKick.Text = "Bicycle Kick";
		this.checkBycicleKick.UseVisualStyleBackColor = false;
		this.checkChipperPenalty.AutoSize = true;
		this.checkChipperPenalty.BackColor = System.Drawing.Color.Transparent;
		this.checkChipperPenalty.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "ChipperPenalty", true));
		this.checkChipperPenalty.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkChipperPenalty.Location = new System.Drawing.Point(13, 110);
		this.checkChipperPenalty.Name = "checkChipperPenalty";
		this.checkChipperPenalty.Size = new System.Drawing.Size(100, 17);
		this.checkChipperPenalty.TabIndex = 47;
		this.checkChipperPenalty.Text = "Chipper Penalty";
		this.checkChipperPenalty.UseVisualStyleBackColor = false;
		this.checkStutterPenalty.AutoSize = true;
		this.checkStutterPenalty.BackColor = System.Drawing.Color.Transparent;
		this.checkStutterPenalty.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "StutterPenalty", true));
		this.checkStutterPenalty.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStutterPenalty.Location = new System.Drawing.Point(13, 88);
		this.checkStutterPenalty.Name = "checkStutterPenalty";
		this.checkStutterPenalty.Size = new System.Drawing.Size(95, 17);
		this.checkStutterPenalty.TabIndex = 46;
		this.checkStutterPenalty.Text = "Stutter Penalty";
		this.checkStutterPenalty.UseVisualStyleBackColor = false;
		this.checkFancyFlicks.AutoSize = true;
		this.checkFancyFlicks.BackColor = System.Drawing.Color.Transparent;
		this.checkFancyFlicks.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "FancyFlicks", true));
		this.checkFancyFlicks.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkFancyFlicks.Location = new System.Drawing.Point(13, 66);
		this.checkFancyFlicks.Name = "checkFancyFlicks";
		this.checkFancyFlicks.Size = new System.Drawing.Size(85, 17);
		this.checkFancyFlicks.TabIndex = 45;
		this.checkFancyFlicks.Text = "Fancy Flicks";
		this.checkFancyFlicks.UseVisualStyleBackColor = false;
		this.checkFancyPasses.AutoSize = true;
		this.checkFancyPasses.BackColor = System.Drawing.Color.Transparent;
		this.checkFancyPasses.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "FancyPasses", true));
		this.checkFancyPasses.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkFancyPasses.Location = new System.Drawing.Point(13, 44);
		this.checkFancyPasses.Name = "checkFancyPasses";
		this.checkFancyPasses.Size = new System.Drawing.Size(92, 17);
		this.checkFancyPasses.TabIndex = 44;
		this.checkFancyPasses.Text = "Fancy Passes";
		this.checkFancyPasses.UseVisualStyleBackColor = false;
		this.checkFancyFeet.AutoSize = true;
		this.checkFancyFeet.BackColor = System.Drawing.Color.Transparent;
		this.checkFancyFeet.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "FancyFeet", true));
		this.checkFancyFeet.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkFancyFeet.Location = new System.Drawing.Point(13, 22);
		this.checkFancyFeet.Name = "checkFancyFeet";
		this.checkFancyFeet.Size = new System.Drawing.Size(79, 17);
		this.checkFancyFeet.TabIndex = 43;
		this.checkFancyFeet.Text = "Fancy Feet";
		this.checkFancyFeet.UseVisualStyleBackColor = false;
		this.checkGKOneonOne.AutoSize = true;
		this.checkGKOneonOne.BackColor = System.Drawing.Color.Transparent;
		this.checkGKOneonOne.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "GkOneOnOne", true));
		this.checkGKOneonOne.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkGKOneonOne.Location = new System.Drawing.Point(24, 213);
		this.checkGKOneonOne.Name = "checkGKOneonOne";
		this.checkGKOneonOne.Size = new System.Drawing.Size(102, 17);
		this.checkGKOneonOne.TabIndex = 56;
		this.checkGKOneonOne.Text = "GK One on One";
		this.checkGKOneonOne.UseVisualStyleBackColor = false;
		this.checkAcrobaticClearance.AutoSize = true;
		this.checkAcrobaticClearance.BackColor = System.Drawing.Color.Transparent;
		this.checkAcrobaticClearance.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "AcrobaticClearance", true));
		this.checkAcrobaticClearance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkAcrobaticClearance.Location = new System.Drawing.Point(24, 235);
		this.checkAcrobaticClearance.Name = "checkAcrobaticClearance";
		this.checkAcrobaticClearance.Size = new System.Drawing.Size(122, 17);
		this.checkAcrobaticClearance.TabIndex = 55;
		this.checkAcrobaticClearance.Text = "Acrobatic Clearance";
		this.checkAcrobaticClearance.UseVisualStyleBackColor = false;
		this.checkSecondWind.AutoSize = true;
		this.checkSecondWind.BackColor = System.Drawing.Color.Transparent;
		this.checkSecondWind.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "SecondWind", true));
		this.checkSecondWind.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkSecondWind.Location = new System.Drawing.Point(24, 81);
		this.checkSecondWind.Name = "checkSecondWind";
		this.checkSecondWind.Size = new System.Drawing.Size(91, 17);
		this.checkSecondWind.TabIndex = 54;
		this.checkSecondWind.Text = "Second Wind";
		this.checkSecondWind.UseVisualStyleBackColor = false;
		this.checkCrowdFavourite.AutoSize = true;
		this.checkCrowdFavourite.BackColor = System.Drawing.Color.Transparent;
		this.checkCrowdFavourite.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "CrowdFavorite", true));
		this.checkCrowdFavourite.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCrowdFavourite.Location = new System.Drawing.Point(334, 213);
		this.checkCrowdFavourite.Name = "checkCrowdFavourite";
		this.checkCrowdFavourite.Size = new System.Drawing.Size(103, 17);
		this.checkCrowdFavourite.TabIndex = 53;
		this.checkCrowdFavourite.Text = "Crowd Favourite";
		this.checkCrowdFavourite.UseVisualStyleBackColor = false;
		this.checkInflexible.AutoSize = true;
		this.checkInflexible.BackColor = System.Drawing.Color.Transparent;
		this.checkInflexible.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Inflexible", true));
		this.checkInflexible.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkInflexible.Location = new System.Drawing.Point(334, 191);
		this.checkInflexible.Name = "checkInflexible";
		this.checkInflexible.Size = new System.Drawing.Size(67, 17);
		this.checkInflexible.TabIndex = 52;
		this.checkInflexible.Text = "Inflexible";
		this.checkInflexible.UseVisualStyleBackColor = false;
		this.checkTeamPlayer.AutoSize = true;
		this.checkTeamPlayer.BackColor = System.Drawing.Color.Transparent;
		this.checkTeamPlayer.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "TeamPlayer", true));
		this.checkTeamPlayer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamPlayer.Location = new System.Drawing.Point(334, 169);
		this.checkTeamPlayer.Name = "checkTeamPlayer";
		this.checkTeamPlayer.Size = new System.Drawing.Size(85, 17);
		this.checkTeamPlayer.TabIndex = 51;
		this.checkTeamPlayer.Text = "Team Player";
		this.checkTeamPlayer.UseVisualStyleBackColor = false;
		this.checkSwervePasser.AutoSize = true;
		this.checkSwervePasser.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "SwervePasser", true));
		this.checkSwervePasser.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkSwervePasser.Location = new System.Drawing.Point(170, 125);
		this.checkSwervePasser.Name = "checkSwervePasser";
		this.checkSwervePasser.Size = new System.Drawing.Size(97, 17);
		this.checkSwervePasser.TabIndex = 50;
		this.checkSwervePasser.Text = "Swerve Passer";
		this.checkSwervePasser.UseVisualStyleBackColor = false;
		this.checkCornerSpecialist.AutoSize = true;
		this.checkCornerSpecialist.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "OutsideFootShot", true));
		this.checkCornerSpecialist.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCornerSpecialist.Location = new System.Drawing.Point(170, 258);
		this.checkCornerSpecialist.Name = "checkCornerSpecialist";
		this.checkCornerSpecialist.Size = new System.Drawing.Size(111, 17);
		this.checkCornerSpecialist.TabIndex = 49;
		this.checkCornerSpecialist.Text = "Outside Foot Shot";
		this.checkCornerSpecialist.UseVisualStyleBackColor = false;
		this.checkPowerHeader.AutoSize = true;
		this.checkPowerHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "PowerHeader", true));
		this.checkPowerHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPowerHeader.Location = new System.Drawing.Point(170, 192);
		this.checkPowerHeader.Name = "checkPowerHeader";
		this.checkPowerHeader.Size = new System.Drawing.Size(94, 17);
		this.checkPowerHeader.TabIndex = 48;
		this.checkPowerHeader.Text = "Power Header";
		this.checkPowerHeader.UseVisualStyleBackColor = false;
		this.checkGkLongThrower.AutoSize = true;
		this.checkGkLongThrower.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "GkLongThrower", true));
		this.checkGkLongThrower.Location = new System.Drawing.Point(24, 191);
		this.checkGkLongThrower.Name = "checkGkLongThrower";
		this.checkGkLongThrower.Size = new System.Drawing.Size(110, 17);
		this.checkGkLongThrower.TabIndex = 47;
		this.checkGkLongThrower.Text = "GK Long Thrower";
		this.checkGkLongThrower.UseVisualStyleBackColor = true;
		this.checkLongPasser.AutoSize = true;
		this.checkLongPasser.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "LongPasser", true));
		this.checkLongPasser.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLongPasser.Location = new System.Drawing.Point(170, 103);
		this.checkLongPasser.Name = "checkLongPasser";
		this.checkLongPasser.Size = new System.Drawing.Size(85, 17);
		this.checkLongPasser.TabIndex = 46;
		this.checkLongPasser.Text = "Long Passer";
		this.checkLongPasser.UseVisualStyleBackColor = false;
		this.checkFlair.AutoSize = true;
		this.checkFlair.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Flair", true));
		this.checkFlair.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkFlair.Location = new System.Drawing.Point(170, 147);
		this.checkFlair.Name = "checkFlair";
		this.checkFlair.Size = new System.Drawing.Size(45, 17);
		this.checkFlair.TabIndex = 45;
		this.checkFlair.Text = "Flair";
		this.checkFlair.UseVisualStyleBackColor = false;
		this.checkFinesseShot.AutoSize = true;
		this.checkFinesseShot.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "FinesseShot", true));
		this.checkFinesseShot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkFinesseShot.Location = new System.Drawing.Point(170, 236);
		this.checkFinesseShot.Name = "checkFinesseShot";
		this.checkFinesseShot.Size = new System.Drawing.Size(87, 17);
		this.checkFinesseShot.TabIndex = 44;
		this.checkFinesseShot.Text = "Finesse Shot";
		this.checkFinesseShot.UseVisualStyleBackColor = false;
		this.checkArguesWithOfficials.AutoSize = true;
		this.checkArguesWithOfficials.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "ArguesWithOfficials", true));
		this.checkArguesWithOfficials.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkArguesWithOfficials.Location = new System.Drawing.Point(334, 125);
		this.checkArguesWithOfficials.Name = "checkArguesWithOfficials";
		this.checkArguesWithOfficials.Size = new System.Drawing.Size(121, 17);
		this.checkArguesWithOfficials.TabIndex = 43;
		this.checkArguesWithOfficials.Text = "Argues with Officials";
		this.checkArguesWithOfficials.UseVisualStyleBackColor = false;
		this.checkBeatsOffsideTrap.AutoSize = true;
		this.checkBeatsOffsideTrap.BackColor = System.Drawing.Color.Transparent;
		this.checkBeatsOffsideTrap.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "BeatDefensiveLine", true));
		this.checkBeatsOffsideTrap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkBeatsOffsideTrap.Location = new System.Drawing.Point(334, 59);
		this.checkBeatsOffsideTrap.Name = "checkBeatsOffsideTrap";
		this.checkBeatsOffsideTrap.Size = new System.Drawing.Size(114, 17);
		this.checkBeatsOffsideTrap.TabIndex = 42;
		this.checkBeatsOffsideTrap.Text = "Beats Offside Trap";
		this.checkBeatsOffsideTrap.UseVisualStyleBackColor = false;
		this.checkAvoidsWeakFoot.AutoSize = true;
		this.checkAvoidsWeakFoot.BackColor = System.Drawing.Color.Transparent;
		this.checkAvoidsWeakFoot.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "AvoidsWeakFoot", true));
		this.checkAvoidsWeakFoot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkAvoidsWeakFoot.Location = new System.Drawing.Point(170, 37);
		this.checkAvoidsWeakFoot.Name = "checkAvoidsWeakFoot";
		this.checkAvoidsWeakFoot.Size = new System.Drawing.Size(144, 17);
		this.checkAvoidsWeakFoot.TabIndex = 41;
		this.checkAvoidsWeakFoot.Text = "Avoids Using Weak Foot";
		this.checkAvoidsWeakFoot.UseVisualStyleBackColor = false;
		this.checkInjuryFree.AutoSize = true;
		this.checkInjuryFree.BackColor = System.Drawing.Color.Transparent;
		this.checkInjuryFree.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "InjuryFree", true));
		this.checkInjuryFree.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkInjuryFree.Location = new System.Drawing.Point(24, 59);
		this.checkInjuryFree.Name = "checkInjuryFree";
		this.checkInjuryFree.Size = new System.Drawing.Size(75, 17);
		this.checkInjuryFree.TabIndex = 40;
		this.checkInjuryFree.Text = "Injury Free";
		this.checkInjuryFree.UseVisualStyleBackColor = false;
		this.checkPowerFreeKick.AutoSize = true;
		this.checkPowerFreeKick.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "PowerfulFreeKicks", true));
		this.checkPowerFreeKick.Location = new System.Drawing.Point(334, 37);
		this.checkPowerFreeKick.Name = "checkPowerFreeKick";
		this.checkPowerFreeKick.Size = new System.Drawing.Size(104, 17);
		this.checkPowerFreeKick.TabIndex = 39;
		this.checkPowerFreeKick.Text = "Power Free Kick";
		this.checkPowerFreeKick.UseVisualStyleBackColor = true;
		this.checkSelfish.AutoSize = true;
		this.checkSelfish.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Selfish", true));
		this.checkSelfish.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkSelfish.Location = new System.Drawing.Point(334, 81);
		this.checkSelfish.Name = "checkSelfish";
		this.checkSelfish.Size = new System.Drawing.Size(57, 17);
		this.checkSelfish.TabIndex = 37;
		this.checkSelfish.Text = "Selfish";
		this.checkSelfish.UseVisualStyleBackColor = false;
		this.checkPlaymaker.AutoSize = true;
		this.checkPlaymaker.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Playmaker", true));
		this.checkPlaymaker.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlaymaker.Location = new System.Drawing.Point(170, 59);
		this.checkPlaymaker.Name = "checkPlaymaker";
		this.checkPlaymaker.Size = new System.Drawing.Size(75, 17);
		this.checkPlaymaker.TabIndex = 33;
		this.checkPlaymaker.Text = "Playmaker";
		this.checkPlaymaker.UseVisualStyleBackColor = false;
		this.checkSpeedDribbler.AutoSize = true;
		this.checkSpeedDribbler.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "SpeedDribbler", true));
		this.checkSpeedDribbler.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkSpeedDribbler.Location = new System.Drawing.Point(170, 169);
		this.checkSpeedDribbler.Name = "checkSpeedDribbler";
		this.checkSpeedDribbler.Size = new System.Drawing.Size(96, 17);
		this.checkSpeedDribbler.TabIndex = 38;
		this.checkSpeedDribbler.Text = "Speed Dribbler";
		this.checkSpeedDribbler.UseVisualStyleBackColor = false;
		this.checkLeadership.AutoSize = true;
		this.checkLeadership.BackColor = System.Drawing.Color.Transparent;
		this.checkLeadership.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Leadership", true));
		this.checkLeadership.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeadership.Location = new System.Drawing.Point(334, 235);
		this.checkLeadership.Name = "checkLeadership";
		this.checkLeadership.Size = new System.Drawing.Size(78, 17);
		this.checkLeadership.TabIndex = 36;
		this.checkLeadership.Text = "Leadership";
		this.checkLeadership.UseVisualStyleBackColor = false;
		this.checkPuncher.AutoSize = true;
		this.checkPuncher.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Puncher", true));
		this.checkPuncher.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPuncher.Location = new System.Drawing.Point(24, 169);
		this.checkPuncher.Name = "checkPuncher";
		this.checkPuncher.Size = new System.Drawing.Size(84, 17);
		this.checkPuncher.TabIndex = 34;
		this.checkPuncher.Text = "GK Puncher";
		this.checkPuncher.UseVisualStyleBackColor = false;
		this.checkDiver.AutoSize = true;
		this.checkDiver.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Diver", true));
		this.checkDiver.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkDiver.Location = new System.Drawing.Point(334, 103);
		this.checkDiver.Name = "checkDiver";
		this.checkDiver.Size = new System.Drawing.Size(51, 17);
		this.checkDiver.TabIndex = 27;
		this.checkDiver.Text = "Diver";
		this.checkDiver.UseVisualStyleBackColor = false;
		this.checkDivesintotackles.AutoSize = true;
		this.checkDivesintotackles.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Divesintotackles", true));
		this.checkDivesintotackles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkDivesintotackles.Location = new System.Drawing.Point(24, 257);
		this.checkDivesintotackles.Name = "checkDivesintotackles";
		this.checkDivesintotackles.Size = new System.Drawing.Size(114, 17);
		this.checkDivesintotackles.TabIndex = 28;
		this.checkDivesintotackles.Text = "Dives into Tackles";
		this.checkDivesintotackles.UseVisualStyleBackColor = false;
		this.checkLongshottaker.AutoSize = true;
		this.checkLongshottaker.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "LongShotTaker", true));
		this.checkLongshottaker.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLongshottaker.Location = new System.Drawing.Point(170, 214);
		this.checkLongshottaker.Name = "checkLongshottaker";
		this.checkLongshottaker.Size = new System.Drawing.Size(106, 17);
		this.checkLongshottaker.TabIndex = 30;
		this.checkLongshottaker.Text = "Long Shot Taker";
		this.checkLongshottaker.UseVisualStyleBackColor = false;
		this.checkHighClubIdentification.AutoSize = true;
		this.checkHighClubIdentification.BackColor = System.Drawing.Color.Transparent;
		this.checkHighClubIdentification.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "HighClubIdentification", true));
		this.checkHighClubIdentification.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkHighClubIdentification.Location = new System.Drawing.Point(334, 147);
		this.checkHighClubIdentification.Name = "checkHighClubIdentification";
		this.checkHighClubIdentification.Size = new System.Drawing.Size(107, 17);
		this.checkHighClubIdentification.TabIndex = 31;
		this.checkHighClubIdentification.Text = "High Club Identif.";
		this.checkHighClubIdentification.UseVisualStyleBackColor = false;
		this.checkPushesupforcorners.AutoSize = true;
		this.checkPushesupforcorners.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Pushesupforcorners", true));
		this.checkPushesupforcorners.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPushesupforcorners.Location = new System.Drawing.Point(24, 147);
		this.checkPushesupforcorners.Name = "checkPushesupforcorners";
		this.checkPushesupforcorners.Size = new System.Drawing.Size(112, 17);
		this.checkPushesupforcorners.TabIndex = 35;
		this.checkPushesupforcorners.Text = "GK Up for Corners";
		this.checkPushesupforcorners.UseVisualStyleBackColor = false;
		this.checkEarlycrosser.AutoSize = true;
		this.checkEarlycrosser.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Earlycrosser", true));
		this.checkEarlycrosser.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkEarlycrosser.Location = new System.Drawing.Point(170, 81);
		this.checkEarlycrosser.Name = "checkEarlycrosser";
		this.checkEarlycrosser.Size = new System.Drawing.Size(87, 17);
		this.checkEarlycrosser.TabIndex = 29;
		this.checkEarlycrosser.Text = "Early Crosser";
		this.checkEarlycrosser.UseVisualStyleBackColor = false;
		this.checkInjuryProne.AutoSize = true;
		this.checkInjuryProne.BackColor = System.Drawing.Color.Transparent;
		this.checkInjuryProne.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "InjuryProne", true));
		this.checkInjuryProne.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkInjuryProne.Location = new System.Drawing.Point(24, 37);
		this.checkInjuryProne.Name = "checkInjuryProne";
		this.checkInjuryProne.Size = new System.Drawing.Size(82, 17);
		this.checkInjuryProne.TabIndex = 32;
		this.checkInjuryProne.Text = "Injury Prone";
		this.checkInjuryProne.UseVisualStyleBackColor = false;
		this.checkGiantThrower.AutoSize = true;
		this.checkGiantThrower.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "GiantThrow", true));
		this.checkGiantThrower.Location = new System.Drawing.Point(24, 125);
		this.checkGiantThrower.Name = "checkGiantThrower";
		this.checkGiantThrower.Size = new System.Drawing.Size(93, 17);
		this.checkGiantThrower.TabIndex = 1;
		this.checkGiantThrower.Text = "Giant Thrower";
		this.checkGiantThrower.UseVisualStyleBackColor = true;
		this.checkLongThrower.AutoSize = true;
		this.checkLongThrower.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "Longthrows", true));
		this.checkLongThrower.Location = new System.Drawing.Point(24, 103);
		this.checkLongThrower.Name = "checkLongThrower";
		this.checkLongThrower.Size = new System.Drawing.Size(92, 17);
		this.checkLongThrower.TabIndex = 0;
		this.checkLongThrower.Text = "Long Thrower";
		this.checkLongThrower.UseVisualStyleBackColor = true;
		this.pageFace.BackColor = System.Drawing.Color.Transparent;
		this.pageFace.Controls.Add(this.splitContainer1);
		this.pageFace.ImageIndex = 2;
		this.pageFace.Location = new System.Drawing.Point(4, 23);
		this.pageFace.Name = "pageFace";
		this.pageFace.Size = new System.Drawing.Size(1349, 780);
		this.pageFace.TabIndex = 2;
		this.pageFace.Text = "Face";
		this.pageFace.UseVisualStyleBackColor = true;
		this.splitContainer1.BackColor = System.Drawing.Color.Transparent;
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 0);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
		this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
		this.splitContainer1.Size = new System.Drawing.Size(1349, 780);
		this.splitContainer1.SplitterDistance = 748;
		this.splitContainer1.TabIndex = 1;
		this.splitContainer2.BackColor = System.Drawing.Color.Transparent;
		this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer2.Location = new System.Drawing.Point(0, 0);
		this.splitContainer2.Name = "splitContainer2";
		this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer2.Panel1.Controls.Add(this.tool3D);
		this.splitContainer2.Panel2.AutoScroll = true;
		this.splitContainer2.Panel2.Controls.Add(this.groupGenericFace);
		this.splitContainer2.Size = new System.Drawing.Size(748, 780);
		this.splitContainer2.SplitterDistance = 466;
		this.splitContainer2.TabIndex = 0;
		this.tool3D.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.tool3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.tool3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[24]
		{
			this.buttonShow3DModel, this.buttonSwitchRenderingMode, this.toolStripSeparator1, this.buttonImport3DHeadModel, this.buttonExport3DHeadModel, this.buttonRemove3DHeadModel, this.toolStripSeparator4, this.buttonImport3DHairModel, this.buttonExport3DHairModel, this.buttonRemoveHairModel,
			this.toolStripSeparator5, this.buttonMoveHairAhead, this.buttonMoveHairBack, this.buttonMoveHairUp, this.buttonMoveHairDown, this.buttonMoveHairLeft, this.buttonMoveHairRight, this.buttonMakeHairCloser, this.buttonMakeHairWider, this.buttonSaveHair,
			this.toolStripSeparator2, this.toolPhoto, this.toolStripSeparator3, this.buttonShowJesey
		});
		this.tool3D.Location = new System.Drawing.Point(0, 441);
		this.tool3D.Name = "tool3D";
		this.tool3D.Size = new System.Drawing.Size(748, 25);
		this.tool3D.TabIndex = 4;
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
		this.buttonImport3DHeadModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImport3DHeadModel.Image = (System.Drawing.Image)resources.GetObject("buttonImport3DHeadModel.Image");
		this.buttonImport3DHeadModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImport3DHeadModel.Name = "buttonImport3DHeadModel";
		this.buttonImport3DHeadModel.Size = new System.Drawing.Size(23, 22);
		this.buttonImport3DHeadModel.Text = "Import 3D Head Model";
		this.buttonImport3DHeadModel.Click += new System.EventHandler(buttonImport3DHeadModel_Click);
		this.buttonExport3DHeadModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport3DHeadModel.Image = (System.Drawing.Image)resources.GetObject("buttonExport3DHeadModel.Image");
		this.buttonExport3DHeadModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport3DHeadModel.Name = "buttonExport3DHeadModel";
		this.buttonExport3DHeadModel.Size = new System.Drawing.Size(23, 22);
		this.buttonExport3DHeadModel.Text = "Export 3D Head Model";
		this.buttonExport3DHeadModel.Click += new System.EventHandler(buttonExport3DHeadModel_Click);
		this.buttonRemove3DHeadModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove3DHeadModel.Image = (System.Drawing.Image)resources.GetObject("buttonRemove3DHeadModel.Image");
		this.buttonRemove3DHeadModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove3DHeadModel.Name = "buttonRemove3DHeadModel";
		this.buttonRemove3DHeadModel.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove3DHeadModel.Text = "Remove 3D Head Model";
		this.buttonRemove3DHeadModel.Click += new System.EventHandler(buttonRemove3DModel_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.buttonImport3DHairModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImport3DHairModel.Image = (System.Drawing.Image)resources.GetObject("buttonImport3DHairModel.Image");
		this.buttonImport3DHairModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImport3DHairModel.Name = "buttonImport3DHairModel";
		this.buttonImport3DHairModel.Size = new System.Drawing.Size(23, 22);
		this.buttonImport3DHairModel.Text = "Import 3D Hair Model";
		this.buttonImport3DHairModel.Click += new System.EventHandler(buttonImport3DHairModels_Click);
		this.buttonExport3DHairModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport3DHairModel.Image = (System.Drawing.Image)resources.GetObject("buttonExport3DHairModel.Image");
		this.buttonExport3DHairModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport3DHairModel.Name = "buttonExport3DHairModel";
		this.buttonExport3DHairModel.Size = new System.Drawing.Size(23, 22);
		this.buttonExport3DHairModel.Text = "Export 3D Hair Model";
		this.buttonExport3DHairModel.Click += new System.EventHandler(buttonExport3DHairModels_Click);
		this.buttonRemoveHairModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemoveHairModel.Image = (System.Drawing.Image)resources.GetObject("buttonRemoveHairModel.Image");
		this.buttonRemoveHairModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemoveHairModel.Name = "buttonRemoveHairModel";
		this.buttonRemoveHairModel.Size = new System.Drawing.Size(23, 22);
		this.buttonRemoveHairModel.Text = "Remove Hair Model";
		this.buttonRemoveHairModel.Click += new System.EventHandler(buttonRemoveHairModel_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
		this.buttonMoveHairAhead.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMoveHairAhead.Image = (System.Drawing.Image)resources.GetObject("buttonMoveHairAhead.Image");
		this.buttonMoveHairAhead.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMoveHairAhead.Name = "buttonMoveHairAhead";
		this.buttonMoveHairAhead.Size = new System.Drawing.Size(23, 22);
		this.buttonMoveHairAhead.Text = "Move Hair Ahead";
		this.buttonMoveHairAhead.Click += new System.EventHandler(buttonAhead_Click);
		this.buttonMoveHairBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMoveHairBack.Image = (System.Drawing.Image)resources.GetObject("buttonMoveHairBack.Image");
		this.buttonMoveHairBack.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.buttonMoveHairBack.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMoveHairBack.Name = "buttonMoveHairBack";
		this.buttonMoveHairBack.Size = new System.Drawing.Size(23, 22);
		this.buttonMoveHairBack.Text = "Move Hair Back";
		this.buttonMoveHairBack.Click += new System.EventHandler(buttonBack_Click);
		this.buttonMoveHairUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMoveHairUp.Image = (System.Drawing.Image)resources.GetObject("buttonMoveHairUp.Image");
		this.buttonMoveHairUp.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMoveHairUp.Name = "buttonMoveHairUp";
		this.buttonMoveHairUp.Size = new System.Drawing.Size(23, 22);
		this.buttonMoveHairUp.Text = "Move Hair Up";
		this.buttonMoveHairUp.Click += new System.EventHandler(buttonUp_Click);
		this.buttonMoveHairDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMoveHairDown.Image = (System.Drawing.Image)resources.GetObject("buttonMoveHairDown.Image");
		this.buttonMoveHairDown.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.buttonMoveHairDown.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMoveHairDown.Name = "buttonMoveHairDown";
		this.buttonMoveHairDown.Size = new System.Drawing.Size(23, 22);
		this.buttonMoveHairDown.Text = "Move Hair Down";
		this.buttonMoveHairDown.Click += new System.EventHandler(buttonDown_Click);
		this.buttonMoveHairLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMoveHairLeft.Image = (System.Drawing.Image)resources.GetObject("buttonMoveHairLeft.Image");
		this.buttonMoveHairLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMoveHairLeft.Name = "buttonMoveHairLeft";
		this.buttonMoveHairLeft.Size = new System.Drawing.Size(23, 22);
		this.buttonMoveHairLeft.Text = "Move Hair Left";
		this.buttonMoveHairLeft.Click += new System.EventHandler(buttonMoveHairLeft_Click);
		this.buttonMoveHairRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMoveHairRight.Image = (System.Drawing.Image)resources.GetObject("buttonMoveHairRight.Image");
		this.buttonMoveHairRight.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMoveHairRight.Name = "buttonMoveHairRight";
		this.buttonMoveHairRight.Size = new System.Drawing.Size(23, 22);
		this.buttonMoveHairRight.Text = "Move Hair Right";
		this.buttonMoveHairRight.Click += new System.EventHandler(buttonMoveHairRight_Click);
		this.buttonMakeHairCloser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMakeHairCloser.Image = (System.Drawing.Image)resources.GetObject("buttonMakeHairCloser.Image");
		this.buttonMakeHairCloser.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMakeHairCloser.Name = "buttonMakeHairCloser";
		this.buttonMakeHairCloser.Size = new System.Drawing.Size(23, 22);
		this.buttonMakeHairCloser.Text = "Make Hair Closer";
		this.buttonMakeHairCloser.Click += new System.EventHandler(buttonMakeHairCloser_Click);
		this.buttonMakeHairWider.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonMakeHairWider.Image = (System.Drawing.Image)resources.GetObject("buttonMakeHairWider.Image");
		this.buttonMakeHairWider.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonMakeHairWider.Name = "buttonMakeHairWider";
		this.buttonMakeHairWider.Size = new System.Drawing.Size(23, 22);
		this.buttonMakeHairWider.Text = "Make Hair Wider";
		this.buttonMakeHairWider.Click += new System.EventHandler(buttonMakeHairWider_Click);
		this.buttonSaveHair.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSaveHair.Enabled = false;
		this.buttonSaveHair.Image = (System.Drawing.Image)resources.GetObject("buttonSaveHair.Image");
		this.buttonSaveHair.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSaveHair.Name = "buttonSaveHair";
		this.buttonSaveHair.Size = new System.Drawing.Size(23, 22);
		this.buttonSaveHair.Text = "Save Modified Hair";
		this.buttonSaveHair.Click += new System.EventHandler(buttonSaveHair_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.toolPhoto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolPhoto.Image = (System.Drawing.Image)resources.GetObject("toolPhoto.Image");
		this.toolPhoto.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolPhoto.Name = "toolPhoto";
		this.toolPhoto.Size = new System.Drawing.Size(23, 22);
		this.toolPhoto.Text = "Take a picture";
		this.toolPhoto.Click += new System.EventHandler(toolPhoto_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
		this.buttonShowJesey.Checked = true;
		this.buttonShowJesey.CheckOnClick = true;
		this.buttonShowJesey.CheckState = System.Windows.Forms.CheckState.Checked;
		this.buttonShowJesey.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShowJesey.Image = (System.Drawing.Image)resources.GetObject("buttonShowJesey.Image");
		this.buttonShowJesey.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShowJesey.Name = "buttonShowJesey";
		this.buttonShowJesey.Size = new System.Drawing.Size(23, 22);
		this.buttonShowJesey.Text = "Show team jersey";
		this.buttonShowJesey.Click += new System.EventHandler(buttonShowJesey_Click);
		this.groupGenericFace.Controls.Add(this.checkUsingRevMod);
		this.groupGenericFace.Controls.Add(this.viewer2DPlayerGui);
		this.groupGenericFace.Controls.Add(this.groupGenericFaceType);
		this.groupGenericFace.Controls.Add(this.checkHasGenericFace);
		this.groupGenericFace.Controls.Add(this.groupHairModel);
		this.groupGenericFace.Controls.Add(this.groupHeadModel);
		this.groupGenericFace.Controls.Add(this.labelSideburns);
		this.groupGenericFace.Controls.Add(this.comboSideburns);
		this.groupGenericFace.Controls.Add(this.labelHeadType);
		this.groupGenericFace.Controls.Add(this.labelHairType);
		this.groupGenericFace.Location = new System.Drawing.Point(8, 3);
		this.groupGenericFace.Name = "groupGenericFace";
		this.groupGenericFace.Size = new System.Drawing.Size(734, 296);
		this.groupGenericFace.TabIndex = 86;
		this.groupGenericFace.TabStop = false;
		this.groupGenericFace.Text = "Face Modelling";
		this.checkUsingRevMod.AutoSize = true;
		this.checkUsingRevMod.Location = new System.Drawing.Point(610, 19);
		this.checkUsingRevMod.Name = "checkUsingRevMod";
		this.checkUsingRevMod.Size = new System.Drawing.Size(106, 17);
		this.checkUsingRevMod.TabIndex = 122;
		this.checkUsingRevMod.Text = "Using Rev. Mod.";
		this.toolTip.SetToolTip(this.checkUsingRevMod, "Check this box if you are using RevMod and want to enable specific face even if Has Generic Face is checked");
		this.checkUsingRevMod.UseVisualStyleBackColor = true;
		this.checkUsingRevMod.Visible = false;
		this.checkUsingRevMod.CheckedChanged += new System.EventHandler(checkUsingrevMod_CheckedChanged);
		this.viewer2DPlayerGui.AutoTransparency = true;
		this.viewer2DPlayerGui.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPlayerGui.ButtonStripVisible = false;
		this.viewer2DPlayerGui.CurrentBitmap = null;
		this.viewer2DPlayerGui.ExtendedFormat = false;
		this.viewer2DPlayerGui.FullSizeButton = false;
		this.viewer2DPlayerGui.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DPlayerGui.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DPlayerGui.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.MiniFace;
		this.viewer2DPlayerGui.Location = new System.Drawing.Point(599, 43);
		this.viewer2DPlayerGui.Name = "viewer2DPlayerGui";
		this.viewer2DPlayerGui.RemoveButton = false;
		this.viewer2DPlayerGui.ShowButton = false;
		this.viewer2DPlayerGui.ShowButtonChecked = true;
		this.viewer2DPlayerGui.Size = new System.Drawing.Size(128, 153);
		this.viewer2DPlayerGui.TabIndex = 126;
		this.groupGenericFaceType.Controls.Add(this.labelFacialHair);
		this.groupGenericFaceType.Controls.Add(this.labelEyeBow);
		this.groupGenericFaceType.Controls.Add(this.domainFacialHair);
		this.groupGenericFaceType.Controls.Add(this.comboEyeBow);
		this.groupGenericFaceType.Controls.Add(this.labelSkintype);
		this.groupGenericFaceType.Controls.Add(this.comboSkintype);
		this.groupGenericFaceType.Controls.Add(this.comboFacialHairColor);
		this.groupGenericFaceType.Controls.Add(this.labelFacialHairColor);
		this.groupGenericFaceType.Location = new System.Drawing.Point(376, 42);
		this.groupGenericFaceType.Name = "groupGenericFaceType";
		this.groupGenericFaceType.Size = new System.Drawing.Size(217, 220);
		this.groupGenericFaceType.TabIndex = 35;
		this.groupGenericFaceType.TabStop = false;
		this.groupGenericFaceType.Text = "Face Type";
		this.labelFacialHair.AutoSize = true;
		this.labelFacialHair.BackColor = System.Drawing.Color.Transparent;
		this.labelFacialHair.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFacialHair.Location = new System.Drawing.Point(6, 123);
		this.labelFacialHair.Name = "labelFacialHair";
		this.labelFacialHair.Size = new System.Drawing.Size(57, 13);
		this.labelFacialHair.TabIndex = 15;
		this.labelFacialHair.Text = "Facial Hair";
		this.labelFacialHair.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelEyeBow.AutoSize = true;
		this.labelEyeBow.BackColor = System.Drawing.Color.Transparent;
		this.labelEyeBow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelEyeBow.Location = new System.Drawing.Point(6, 90);
		this.labelEyeBow.Name = "labelEyeBow";
		this.labelEyeBow.Size = new System.Drawing.Size(57, 13);
		this.labelEyeBow.TabIndex = 33;
		this.labelEyeBow.Text = "Eyes Brow";
		this.labelEyeBow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.domainFacialHair.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.domainFacialHair.FormattingEnabled = true;
		this.domainFacialHair.Items.AddRange(new object[32]
		{
			"None", "Chin Stubble", "Chin Strap", "Goatee", "Casual Beard", "Partial Goatee", "Stubble", "Tuft", "Full Beard", "Light Goatee",
			"Mustache", "Light Chin Curtain", "Full Goatee", "Chin Curtain", "Beard", "Patchy Beard", "Light Goatee 2", "Light Goatee 3", "Patchy Beard 2", "Beard 2",
			"Chin Stubble 2", "Chin Stubble 3", "Full Goatee 2", "Goatee 2", "Casual Beard 2", "Partial Goatee 2", "Stubble 3", "Chin Curtain 2", "Full Berad 2", "Light Goatee 4",
			"Mustache 2", "Light Chin Curtain 2"
		});
		this.domainFacialHair.Location = new System.Drawing.Point(70, 120);
		this.domainFacialHair.Name = "domainFacialHair";
		this.domainFacialHair.Size = new System.Drawing.Size(140, 21);
		this.domainFacialHair.TabIndex = 4;
		this.domainFacialHair.SelectedIndexChanged += new System.EventHandler(domainFacialHair_SelectedItemChanged);
		this.comboEyeBow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboEyeBow.FormattingEnabled = true;
		this.comboEyeBow.Items.AddRange(new object[7] { "Normal", "Big", "Thin", "Type Female 3", "Type Female 4", "Type Female 5", "Type Female 6" });
		this.comboEyeBow.Location = new System.Drawing.Point(70, 87);
		this.comboEyeBow.Name = "comboEyeBow";
		this.comboEyeBow.Size = new System.Drawing.Size(140, 21);
		this.comboEyeBow.TabIndex = 3;
		this.comboEyeBow.SelectedIndexChanged += new System.EventHandler(comboEyeBow_SelectedIndexChanged);
		this.labelSkintype.AutoSize = true;
		this.labelSkintype.BackColor = System.Drawing.Color.Transparent;
		this.labelSkintype.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSkintype.Location = new System.Drawing.Point(6, 54);
		this.labelSkintype.Name = "labelSkintype";
		this.labelSkintype.Size = new System.Drawing.Size(55, 13);
		this.labelSkintype.TabIndex = 21;
		this.labelSkintype.Text = "Skin Type";
		this.labelSkintype.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboSkintype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboSkintype.FormattingEnabled = true;
		this.comboSkintype.Items.AddRange(new object[8] { "Clean", "Freckled", "Rough", "Type Female 3", "Type Female 4", "Type Female 5", "Type Female 6", "Type Female 7" });
		this.comboSkintype.Location = new System.Drawing.Point(70, 51);
		this.comboSkintype.Name = "comboSkintype";
		this.comboSkintype.Size = new System.Drawing.Size(140, 21);
		this.comboSkintype.TabIndex = 1;
		this.comboSkintype.SelectedIndexChanged += new System.EventHandler(comboSkintype_SelectedIndexChanged);
		this.comboFacialHairColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboFacialHairColor.FormattingEnabled = true;
		this.comboFacialHairColor.Items.AddRange(new object[6] { "Black", "Light Blonde", "Dark Brown", "Light Brown", "Red", "Dark Blonde" });
		this.comboFacialHairColor.Location = new System.Drawing.Point(70, 154);
		this.comboFacialHairColor.Name = "comboFacialHairColor";
		this.comboFacialHairColor.Size = new System.Drawing.Size(140, 21);
		this.comboFacialHairColor.TabIndex = 5;
		this.comboFacialHairColor.SelectedIndexChanged += new System.EventHandler(comboFacialHairColor_SelectedIndexChanged);
		this.labelFacialHairColor.AutoSize = true;
		this.labelFacialHairColor.BackColor = System.Drawing.Color.Transparent;
		this.labelFacialHairColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFacialHairColor.Location = new System.Drawing.Point(6, 157);
		this.labelFacialHairColor.Name = "labelFacialHairColor";
		this.labelFacialHairColor.Size = new System.Drawing.Size(31, 13);
		this.labelFacialHairColor.TabIndex = 17;
		this.labelFacialHairColor.Text = "Color";
		this.labelFacialHairColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.checkHasGenericFace.AutoSize = true;
		this.checkHasGenericFace.Location = new System.Drawing.Point(66, 19);
		this.checkHasGenericFace.Name = "checkHasGenericFace";
		this.checkHasGenericFace.Size = new System.Drawing.Size(112, 17);
		this.checkHasGenericFace.TabIndex = 0;
		this.checkHasGenericFace.Text = "Has Generic Face";
		this.toolTip.SetToolTip(this.checkHasGenericFace, "Check this box if you want the player has a generic face. Uncheck for ahaving a specific face. This info is saved in the database.");
		this.checkHasGenericFace.UseVisualStyleBackColor = true;
		this.checkHasGenericFace.CheckedChanged += new System.EventHandler(checkHasGenericFace_CheckedChanged);
		this.groupHairModel.Controls.Add(this.comboFemaleHair);
		this.groupHairModel.Controls.Add(this.radioButtonFemaleHair);
		this.groupHairModel.Controls.Add(this.buttonHairSelection);
		this.groupHairModel.Controls.Add(this.comboHeadband);
		this.groupHairModel.Controls.Add(this.comboAfro);
		this.groupHairModel.Controls.Add(this.comboLong);
		this.groupHairModel.Controls.Add(this.comboMedium);
		this.groupHairModel.Controls.Add(this.comboModern);
		this.groupHairModel.Controls.Add(this.labelHairColor);
		this.groupHairModel.Controls.Add(this.domainHairColor);
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
		this.groupHairModel.Location = new System.Drawing.Point(6, 127);
		this.groupHairModel.Name = "groupHairModel";
		this.groupHairModel.Size = new System.Drawing.Size(364, 135);
		this.groupHairModel.TabIndex = 29;
		this.groupHairModel.TabStop = false;
		this.groupHairModel.Text = "Hair Model and Color";
		this.comboFemaleHair.FormattingEnabled = true;
		this.comboFemaleHair.Location = new System.Drawing.Point(6, 78);
		this.comboFemaleHair.Name = "comboFemaleHair";
		this.comboFemaleHair.Size = new System.Drawing.Size(260, 21);
		this.comboFemaleHair.TabIndex = 32;
		this.comboFemaleHair.Visible = false;
		this.comboFemaleHair.SelectedIndexChanged += new System.EventHandler(comboFemaleHair_SelectedIndexChanged);
		this.radioButtonFemaleHair.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButtonFemaleHair.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButtonFemaleHair.Location = new System.Drawing.Point(290, 17);
		this.radioButtonFemaleHair.Name = "radioButtonFemaleHair";
		this.radioButtonFemaleHair.Size = new System.Drawing.Size(65, 23);
		this.radioButtonFemaleHair.TabIndex = 31;
		this.radioButtonFemaleHair.TabStop = true;
		this.radioButtonFemaleHair.Tag = this.comboFemaleHair;
		this.radioButtonFemaleHair.Text = "Female";
		this.radioButtonFemaleHair.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioButtonFemaleHair.UseVisualStyleBackColor = true;
		this.radioButtonFemaleHair.CheckedChanged += new System.EventHandler(radioButtonFemaleHair_CheckedChanged);
		this.buttonHairSelection.Location = new System.Drawing.Point(272, 83);
		this.buttonHairSelection.Name = "buttonHairSelection";
		this.buttonHairSelection.Size = new System.Drawing.Size(86, 46);
		this.buttonHairSelection.TabIndex = 30;
		this.buttonHairSelection.Text = "Fast Hair Selection";
		this.buttonHairSelection.UseVisualStyleBackColor = true;
		this.buttonHairSelection.Click += new System.EventHandler(buttonHairSelection_Click);
		this.comboHeadband.FormattingEnabled = true;
		this.comboHeadband.Location = new System.Drawing.Point(6, 78);
		this.comboHeadband.Name = "comboHeadband";
		this.comboHeadband.Size = new System.Drawing.Size(260, 21);
		this.comboHeadband.TabIndex = 0;
		this.comboHeadband.Visible = false;
		this.comboHeadband.SelectedIndexChanged += new System.EventHandler(comboHeadband_SelectedIndexChanged);
		this.comboAfro.FormattingEnabled = true;
		this.comboAfro.Items.AddRange(new object[8] { "71", "4", "42", "27", "5", "6", "96", "3" });
		this.comboAfro.Location = new System.Drawing.Point(6, 78);
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
		this.comboLong.Location = new System.Drawing.Point(6, 78);
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
		this.comboMedium.Location = new System.Drawing.Point(6, 78);
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
		this.comboModern.Location = new System.Drawing.Point(6, 78);
		this.comboModern.Name = "comboModern";
		this.comboModern.Size = new System.Drawing.Size(260, 21);
		this.comboModern.TabIndex = 26;
		this.comboModern.Visible = false;
		this.comboModern.SelectedIndexChanged += new System.EventHandler(comboModern_SelectedIndexChanged);
		this.labelHairColor.AutoSize = true;
		this.labelHairColor.BackColor = System.Drawing.Color.Transparent;
		this.labelHairColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHairColor.Location = new System.Drawing.Point(6, 112);
		this.labelHairColor.Name = "labelHairColor";
		this.labelHairColor.Size = new System.Drawing.Size(53, 13);
		this.labelHairColor.TabIndex = 13;
		this.labelHairColor.Text = "Hair Color";
		this.labelHairColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.domainHairColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.domainHairColor.FormattingEnabled = true;
		this.domainHairColor.Items.AddRange(new object[13]
		{
			"Blonde", "Black", "Ash Blonde", "Dark Brown", "Platinum Blonde", "Light Brown", "Brown", "Red", "White", "Gray",
			"Green", "Violet", "Intense Red"
		});
		this.domainHairColor.Location = new System.Drawing.Point(71, 108);
		this.domainHairColor.Name = "domainHairColor";
		this.domainHairColor.Size = new System.Drawing.Size(195, 21);
		this.domainHairColor.TabIndex = 1;
		this.domainHairColor.SelectedIndexChanged += new System.EventHandler(domainHairColor_SelectedIndexChanged);
		this.comboShort.FormattingEnabled = true;
		this.comboShort.Items.AddRange(new object[23]
		{
			"2", "21", "22", "30", "38", "54", "57", "70", "75", "78",
			"82", "97", "101", "102", "104", "105", "106", "107", "108", "109",
			"110", "111", "112"
		});
		this.comboShort.Location = new System.Drawing.Point(6, 78);
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
		this.comboVeryShort.Location = new System.Drawing.Point(6, 78);
		this.comboVeryShort.Name = "comboVeryShort";
		this.comboVeryShort.Size = new System.Drawing.Size(260, 21);
		this.comboVeryShort.TabIndex = 24;
		this.comboVeryShort.Visible = false;
		this.comboVeryShort.SelectedIndexChanged += new System.EventHandler(comboVeryShort_SelectedIndexChanged);
		this.comboShaven.FormattingEnabled = true;
		this.comboShaven.Items.AddRange(new object[6] { "0", "25", "1", "43", "41", "46" });
		this.comboShaven.Location = new System.Drawing.Point(6, 78);
		this.comboShaven.Name = "comboShaven";
		this.comboShaven.Size = new System.Drawing.Size(260, 21);
		this.comboShaven.TabIndex = 23;
		this.comboShaven.Visible = false;
		this.comboShaven.SelectedIndexChanged += new System.EventHandler(comboShaven_SelectedIndexChanged);
		this.radioHeadband.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioHeadband.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioHeadband.Location = new System.Drawing.Point(148, 46);
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
		this.radioAfro.Location = new System.Drawing.Point(219, 46);
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
		this.radioLong.Location = new System.Drawing.Point(77, 46);
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
		this.radioMedium.Location = new System.Drawing.Point(6, 46);
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
		this.radioModern.Location = new System.Drawing.Point(219, 17);
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
		this.radioShort.Location = new System.Drawing.Point(148, 17);
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
		this.radioVeryShort.Location = new System.Drawing.Point(77, 17);
		this.radioVeryShort.Name = "radioVeryShort";
		this.radioVeryShort.Size = new System.Drawing.Size(65, 23);
		this.radioVeryShort.TabIndex = 15;
		this.radioVeryShort.TabStop = true;
		this.radioVeryShort.Tag = this.comboVeryShort;
		this.radioVeryShort.Text = "Very Short";
		this.radioVeryShort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioVeryShort.UseVisualStyleBackColor = true;
		this.radioVeryShort.CheckedChanged += new System.EventHandler(radioVeryShort_CheckedChanged);
		this.groupHeadModel.Controls.Add(this.radioButtonFemale);
		this.groupHeadModel.Controls.Add(this.comboFemaleModels);
		this.groupHeadModel.Controls.Add(this.comboLatinModels);
		this.groupHeadModel.Controls.Add(this.radioButtonLatin);
		this.groupHeadModel.Controls.Add(this.comboAsiaticModels);
		this.groupHeadModel.Controls.Add(this.radioButtonAsiatic);
		this.groupHeadModel.Controls.Add(this.comboAfricanModels);
		this.groupHeadModel.Controls.Add(this.radioButtonAfrican);
		this.groupHeadModel.Controls.Add(this.radioButtonCaucasic);
		this.groupHeadModel.Controls.Add(this.comboCaucasicModels);
		this.groupHeadModel.Controls.Add(this.buttonRandomizeAppearance);
		this.groupHeadModel.Location = new System.Drawing.Point(6, 42);
		this.groupHeadModel.Name = "groupHeadModel";
		this.groupHeadModel.Size = new System.Drawing.Size(364, 79);
		this.groupHeadModel.TabIndex = 28;
		this.groupHeadModel.TabStop = false;
		this.groupHeadModel.Text = "Head Model";
		this.radioButtonFemale.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButtonFemale.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButtonFemale.Location = new System.Drawing.Point(290, 19);
		this.radioButtonFemale.Name = "radioButtonFemale";
		this.radioButtonFemale.Size = new System.Drawing.Size(65, 23);
		this.radioButtonFemale.TabIndex = 29;
		this.radioButtonFemale.TabStop = true;
		this.radioButtonFemale.Tag = "comboFemaleModels [System.Windows.Forms.ComboBox], Items.Count: 42";
		this.radioButtonFemale.Text = "Female";
		this.radioButtonFemale.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radioButtonFemale.UseVisualStyleBackColor = true;
		this.radioButtonFemale.CheckedChanged += new System.EventHandler(radioButtonFemale_CheckedChanged);
		this.comboFemaleModels.FormattingEnabled = true;
		this.comboFemaleModels.Items.AddRange(new object[33]
		{
			"5500", "5501", "5502", "6000", "6001", "6002", "6500", "6501", "6502", "7000",
			"7001", "7002", "7500", "7501", "7502", "8000", "8001", "8002", "8500", "8501",
			"8502", "9000", "9001", "9002", "9500", "9501", "9502", "10000", "10001", "10002",
			"10500", "10501", "10502"
		});
		this.comboFemaleModels.Location = new System.Drawing.Point(6, 48);
		this.comboFemaleModels.Name = "comboFemaleModels";
		this.comboFemaleModels.Size = new System.Drawing.Size(260, 21);
		this.comboFemaleModels.TabIndex = 28;
		this.comboFemaleModels.Visible = false;
		this.comboFemaleModels.SelectedIndexChanged += new System.EventHandler(comboFemaleModels_SelectedIndexChanged);
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
		this.comboLatinModels.TabIndex = 0;
		this.comboLatinModels.Visible = false;
		this.comboLatinModels.SelectedIndexChanged += new System.EventHandler(comboLatinModels_SelectedIndexChanged);
		this.radioButtonLatin.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButtonLatin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButtonLatin.Location = new System.Drawing.Point(219, 19);
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
		this.radioButtonAsiatic.Location = new System.Drawing.Point(77, 19);
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
		this.radioButtonCaucasic.Location = new System.Drawing.Point(148, 19);
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
		this.buttonRandomizeAppearance.Location = new System.Drawing.Point(272, 46);
		this.buttonRandomizeAppearance.Name = "buttonRandomizeAppearance";
		this.buttonRandomizeAppearance.Size = new System.Drawing.Size(86, 23);
		this.buttonRandomizeAppearance.TabIndex = 27;
		this.buttonRandomizeAppearance.Text = "Fast Face";
		this.buttonRandomizeAppearance.UseVisualStyleBackColor = true;
		this.buttonRandomizeAppearance.Click += new System.EventHandler(buttonRandomizeAppearance_Click);
		this.labelSideburns.AutoSize = true;
		this.labelSideburns.BackColor = System.Drawing.Color.Transparent;
		this.labelSideburns.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSideburns.Location = new System.Drawing.Point(603, 208);
		this.labelSideburns.Name = "labelSideburns";
		this.labelSideburns.Size = new System.Drawing.Size(54, 13);
		this.labelSideburns.TabIndex = 23;
		this.labelSideburns.Text = "Sideburns";
		this.labelSideburns.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelSideburns.Visible = false;
		this.comboSideburns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboSideburns.FormattingEnabled = true;
		this.comboSideburns.Items.AddRange(new object[2] { "No", "Yes" });
		this.comboSideburns.Location = new System.Drawing.Point(674, 205);
		this.comboSideburns.Name = "comboSideburns";
		this.comboSideburns.Size = new System.Drawing.Size(140, 21);
		this.comboSideburns.TabIndex = 6;
		this.comboSideburns.Visible = false;
		this.comboSideburns.SelectedIndexChanged += new System.EventHandler(comboSideburns_SelectedIndexChanged);
		this.labelHeadType.BackColor = System.Drawing.SystemColors.Control;
		this.labelHeadType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHeadType.Location = new System.Drawing.Point(185, 164);
		this.labelHeadType.Name = "labelHeadType";
		this.labelHeadType.Size = new System.Drawing.Size(127, 20);
		this.labelHeadType.TabIndex = 11;
		this.labelHeadType.Text = "Head Model";
		this.labelHeadType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelHairType.BackColor = System.Drawing.SystemColors.Control;
		this.labelHairType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHairType.Location = new System.Drawing.Point(16, 204);
		this.labelHairType.Name = "labelHairType";
		this.labelHairType.Size = new System.Drawing.Size(119, 20);
		this.labelHairType.TabIndex = 9;
		this.labelHairType.Text = "Hair Model";
		this.labelHairType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.splitContainer3.BackColor = System.Drawing.Color.Transparent;
		this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer3.Location = new System.Drawing.Point(0, 0);
		this.splitContainer3.Name = "splitContainer3";
		this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer3.Panel1.AutoScroll = true;
		this.splitContainer3.Panel1.Controls.Add(this.groupSpecifiHeadControls);
		this.splitContainer3.Panel1.Controls.Add(this.groupCommonHeadControls);
		this.splitContainer3.Panel1.Controls.Add(this.checkShowTexures);
		this.splitContainer3.Panel2.AutoScroll = true;
		this.splitContainer3.Size = new System.Drawing.Size(597, 780);
		this.splitContainer3.SplitterDistance = 745;
		this.splitContainer3.TabIndex = 0;
		this.groupSpecifiHeadControls.Controls.Add(this.viewer2DTattoos);
		this.groupSpecifiHeadControls.Controls.Add(this.checkHighQaualityFace);
		this.groupSpecifiHeadControls.Controls.Add(this.multiViewerFace);
		this.groupSpecifiHeadControls.Location = new System.Drawing.Point(3, 359);
		this.groupSpecifiHeadControls.Name = "groupSpecifiHeadControls";
		this.groupSpecifiHeadControls.Size = new System.Drawing.Size(544, 349);
		this.groupSpecifiHeadControls.TabIndex = 127;
		this.groupSpecifiHeadControls.TabStop = false;
		this.viewer2DTattoos.AutoTransparency = false;
		this.viewer2DTattoos.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTattoos.ButtonStripVisible = false;
		this.viewer2DTattoos.CurrentBitmap = null;
		this.viewer2DTattoos.ExtendedFormat = false;
		this.viewer2DTattoos.FullSizeButton = false;
		this.viewer2DTattoos.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DTattoos.ImageSize = new System.Drawing.Size(1024, 1024);
		this.viewer2DTattoos.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTattoos.Location = new System.Drawing.Point(268, 51);
		this.viewer2DTattoos.Name = "viewer2DTattoos";
		this.viewer2DTattoos.RemoveButton = false;
		this.viewer2DTattoos.ShowButton = false;
		this.viewer2DTattoos.ShowButtonChecked = true;
		this.viewer2DTattoos.Size = new System.Drawing.Size(256, 279);
		this.viewer2DTattoos.TabIndex = 124;
		this.checkHighQaualityFace.AutoSize = true;
		this.checkHighQaualityFace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.playerBindingSource, "hashighqualityhead", true));
		this.checkHighQaualityFace.Location = new System.Drawing.Point(6, 7);
		this.checkHighQaualityFace.Name = "checkHighQaualityFace";
		this.checkHighQaualityFace.Size = new System.Drawing.Size(110, 17);
		this.checkHighQaualityFace.TabIndex = 123;
		this.checkHighQaualityFace.Text = "High Quality Face";
		this.toolTip.SetToolTip(this.checkHighQaualityFace, "Check this box if the player face is high quality.");
		this.checkHighQaualityFace.UseVisualStyleBackColor = true;
		this.multiViewerFace.AutoTransparency = false;
		this.multiViewerFace.Bitmaps = null;
		this.multiViewerFace.CheckBitmapSize = false;
		this.multiViewerFace.FixedSize = false;
		this.multiViewerFace.FullSizeButton = false;
		this.multiViewerFace.LabelText = "Image n.";
		this.multiViewerFace.Location = new System.Drawing.Point(6, 30);
		this.multiViewerFace.Name = "multiViewerFace";
		this.multiViewerFace.ShowButton = false;
		this.multiViewerFace.ShowDeleteButton = false;
		this.multiViewerFace.Size = new System.Drawing.Size(256, 301);
		this.multiViewerFace.TabIndex = 101;
		this.groupCommonHeadControls.Controls.Add(this.comboFaceposer);
		this.groupCommonHeadControls.Controls.Add(this.label13);
		this.groupCommonHeadControls.Controls.Add(this.buttonRgbHair);
		this.groupCommonHeadControls.Controls.Add(this.multiViewerHair);
		this.groupCommonHeadControls.Controls.Add(this.viewer2DEyeTexture);
		this.groupCommonHeadControls.Controls.Add(this.viewer2DSkinTexture);
		this.groupCommonHeadControls.Controls.Add(this.label1);
		this.groupCommonHeadControls.Controls.Add(this.labelSkinColorInfo);
		this.groupCommonHeadControls.Controls.Add(this.label2);
		this.groupCommonHeadControls.Controls.Add(this.comboEyescolor);
		this.groupCommonHeadControls.Controls.Add(this.numericSkinTone);
		this.groupCommonHeadControls.Location = new System.Drawing.Point(3, 30);
		this.groupCommonHeadControls.Name = "groupCommonHeadControls";
		this.groupCommonHeadControls.Size = new System.Drawing.Size(544, 323);
		this.groupCommonHeadControls.TabIndex = 128;
		this.groupCommonHeadControls.TabStop = false;
		this.comboFaceposer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboFaceposer.FormattingEnabled = true;
		this.comboFaceposer.Items.AddRange(new object[4] { "Default", "Variant 1", "Variant 2", "Variant 3" });
		this.comboFaceposer.Location = new System.Drawing.Point(385, 245);
		this.comboFaceposer.Name = "comboFaceposer";
		this.comboFaceposer.Size = new System.Drawing.Size(128, 21);
		this.comboFaceposer.TabIndex = 125;
		this.comboFaceposer.SelectedIndexChanged += new System.EventHandler(comboFaceposer_SelectedIndexChanged);
		this.label13.AutoSize = true;
		this.label13.BackColor = System.Drawing.Color.Transparent;
		this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label13.Location = new System.Drawing.Point(289, 250);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(90, 13);
		this.label13.TabIndex = 124;
		this.label13.Text = "Face Expressions";
		this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonRgbHair.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonRgbHair.BackgroundImage");
		this.buttonRgbHair.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonRgbHair.Location = new System.Drawing.Point(236, 290);
		this.buttonRgbHair.Name = "buttonRgbHair";
		this.buttonRgbHair.Size = new System.Drawing.Size(25, 23);
		this.buttonRgbHair.TabIndex = 100;
		this.toolTip.SetToolTip(this.buttonRgbHair, "Modify the RGB components");
		this.buttonRgbHair.UseVisualStyleBackColor = true;
		this.buttonRgbHair.Click += new System.EventHandler(buttonRgbHair_Click);
		this.multiViewerHair.AutoTransparency = false;
		this.multiViewerHair.Bitmaps = null;
		this.multiViewerHair.CheckBitmapSize = false;
		this.multiViewerHair.FixedSize = false;
		this.multiViewerHair.FullSizeButton = false;
		this.multiViewerHair.LabelText = "Image n.";
		this.multiViewerHair.Location = new System.Drawing.Point(6, 13);
		this.multiViewerHair.Name = "multiViewerHair";
		this.multiViewerHair.ShowButton = false;
		this.multiViewerHair.ShowDeleteButton = false;
		this.multiViewerHair.Size = new System.Drawing.Size(256, 301);
		this.multiViewerHair.TabIndex = 5;
		this.viewer2DEyeTexture.AutoTransparency = false;
		this.viewer2DEyeTexture.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DEyeTexture.ButtonStripVisible = false;
		this.viewer2DEyeTexture.CurrentBitmap = null;
		this.viewer2DEyeTexture.ExtendedFormat = false;
		this.viewer2DEyeTexture.FullSizeButton = false;
		this.viewer2DEyeTexture.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DEyeTexture.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DEyeTexture.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DEyeTexture.Location = new System.Drawing.Point(268, 33);
		this.viewer2DEyeTexture.Name = "viewer2DEyeTexture";
		this.viewer2DEyeTexture.RemoveButton = false;
		this.viewer2DEyeTexture.ShowButton = false;
		this.viewer2DEyeTexture.ShowButtonChecked = true;
		this.viewer2DEyeTexture.Size = new System.Drawing.Size(128, 153);
		this.viewer2DEyeTexture.TabIndex = 4;
		this.viewer2DSkinTexture.AutoTransparency = false;
		this.viewer2DSkinTexture.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DSkinTexture.ButtonStripVisible = false;
		this.viewer2DSkinTexture.CurrentBitmap = null;
		this.viewer2DSkinTexture.ExtendedFormat = false;
		this.viewer2DSkinTexture.FullSizeButton = false;
		this.viewer2DSkinTexture.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DSkinTexture.ImageSize = new System.Drawing.Size(1024, 1024);
		this.viewer2DSkinTexture.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DSkinTexture.Location = new System.Drawing.Point(402, 33);
		this.viewer2DSkinTexture.Name = "viewer2DSkinTexture";
		this.viewer2DSkinTexture.RemoveButton = false;
		this.viewer2DSkinTexture.ShowButton = false;
		this.viewer2DSkinTexture.ShowButtonChecked = true;
		this.viewer2DSkinTexture.Size = new System.Drawing.Size(128, 153);
		this.viewer2DSkinTexture.TabIndex = 7;
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1.Location = new System.Drawing.Point(408, 195);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(55, 13);
		this.label1.TabIndex = 19;
		this.label1.Text = "Skin Color";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelSkinColorInfo.AutoSize = true;
		this.labelSkinColorInfo.BackColor = System.Drawing.Color.Transparent;
		this.labelSkinColorInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSkinColorInfo.Location = new System.Drawing.Point(429, 221);
		this.labelSkinColorInfo.Name = "labelSkinColorInfo";
		this.labelSkinColorInfo.Size = new System.Drawing.Size(84, 13);
		this.labelSkinColorInfo.TabIndex = 121;
		this.labelSkinColorInfo.Text = "Skin Description";
		this.labelSkinColorInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label2.Location = new System.Drawing.Point(308, 221);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(57, 13);
		this.label2.TabIndex = 25;
		this.label2.Text = "Eyes Color";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboEyescolor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboEyescolor.FormattingEnabled = true;
		this.comboEyescolor.Items.AddRange(new object[10] { "Dark Blue", "Light Blue", "Dark Brown", "Light Brown", "Brown and Green", "Dark Green", "Light Green", "Gray", "Black", "Dark Gray" });
		this.comboEyescolor.Location = new System.Drawing.Point(268, 192);
		this.comboEyescolor.Name = "comboEyescolor";
		this.comboEyescolor.Size = new System.Drawing.Size(128, 21);
		this.comboEyescolor.TabIndex = 2;
		this.comboEyescolor.SelectedIndexChanged += new System.EventHandler(comboEyescolor_SelectedIndexChanged);
		this.numericSkinTone.Location = new System.Drawing.Point(469, 193);
		this.numericSkinTone.Maximum = new decimal(new int[4] { 256, 0, 0, 0 });
		this.numericSkinTone.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericSkinTone.Name = "numericSkinTone";
		this.numericSkinTone.Size = new System.Drawing.Size(61, 20);
		this.numericSkinTone.TabIndex = 120;
		this.numericSkinTone.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericSkinTone.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericSkinTone.ValueChanged += new System.EventHandler(numericSkinTone_ValueChanged);
		this.checkShowTexures.AutoSize = true;
		this.checkShowTexures.Location = new System.Drawing.Point(3, 7);
		this.checkShowTexures.Name = "checkShowTexures";
		this.checkShowTexures.Size = new System.Drawing.Size(97, 17);
		this.checkShowTexures.TabIndex = 0;
		this.checkShowTexures.Text = "Show Textures";
		this.checkShowTexures.UseVisualStyleBackColor = true;
		this.checkShowTexures.CheckedChanged += new System.EventHandler(checkShowTexures_CheckedChanged);
		this.imageListTabIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListTabIcons.ImageStream");
		this.imageListTabIcons.TransparentColor = System.Drawing.Color.Fuchsia;
		this.imageListTabIcons.Images.SetKeyName(0, "Info_16.PNG");
		this.imageListTabIcons.Images.SetKeyName(1, "Star_16.PNG");
		this.imageListTabIcons.Images.SetKeyName(2, "Face_16.PNG");
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = true;
		this.pickUpControl.CreateButtonEnabled = true;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = new string[7] { "All", "by Team", "by Country", "Free Agents", "Multi Club", "Same Name", "Specific Head" };
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
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1357, 832);
		base.Controls.Add(this.tabEditPlayer);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "PlayerForm";
		this.Text = "PlayerForm";
		base.Load += new System.EventHandler(PlayerForm_Load);
		this.tabEditPlayer.ResumeLayout(false);
		this.pageInfo.ResumeLayout(false);
		this.flowPanelInfo.ResumeLayout(false);
		this.groupPlayerIdentity.ResumeLayout(false);
		this.groupPlayerIdentity.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.playerBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPlayerId).EndInit();
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).EndInit();
		this.groupBoxBody.ResumeLayout(false);
		this.groupBoxBody.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericHeight).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericWeight).EndInit();
		this.groupBoxLook.ResumeLayout(false);
		this.groupBoxLook.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericGkGloves).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorAcc1).EndInit();
		this.groupPlayFirTeam.ResumeLayout(false);
		this.groupPlayFirTeam.PerformLayout();
		this.groupIsLoan.ResumeLayout(false);
		this.groupIsLoan.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).EndInit();
		this.groupShoes.ResumeLayout(false);
		this.groupShoes.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureColorShoes1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesBrand).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericShoesDesign).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.pageSkills.ResumeLayout(false);
		this.flowPanelSkills.ResumeLayout(false);
		this.groupGenerateAttributes.ResumeLayout(false);
		this.groupGenerateAttributes.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackOverallrating).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericRandomize).EndInit();
		this.groupGoalkeperSkills.ResumeLayout(false);
		this.groupGoalkeperSkills.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGkKicking).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackDiving).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackPositioning).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackReflexes).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackHandling).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericGoalkeeperSkills).EndInit();
		this.groupDefensiveSkills.ResumeLayout(false);
		this.groupDefensiveSkills.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackInterception).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackSliding).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericDefensiveSkills).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackTackling).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackMarking).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackAggression).EndInit();
		this.groupMidfielderSkills.ResumeLayout(false);
		this.groupMidfielderSkills.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackCurve).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackVision).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericMidfielderSkills).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackLongPassing).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackShortPassing).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBallControl).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackCrossing).EndInit();
		this.groupMental.ResumeLayout(false);
		this.groupMental.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericMentalSkills).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackPlayerPositioning).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackPotential).EndInit();
		this.groupAttackingSkills.ResumeLayout(false);
		this.groupAttackingSkills.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackHeading).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackVolley).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericAttackingSkills).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackFinishing).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackShotPower).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackLongShot).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackDribbling).EndInit();
		this.groupGenericAttributes.ResumeLayout(false);
		this.groupGenericAttributes.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBalance).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackAgility).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPhysicalSkills).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackStamina).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackSprintSpeed).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackAcceleration).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackStrength).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackReactions).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackJumping).EndInit();
		this.groupFreeKick.ResumeLayout(false);
		this.groupFreeKick.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericSkillMoves).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericFreeKickSkills).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackFreeKick).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackPenalties).EndInit();
		this.groupTraits.ResumeLayout(false);
		this.groupTraits.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		this.pageFace.ResumeLayout(false);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.splitContainer2.Panel1.ResumeLayout(false);
		this.splitContainer2.Panel1.PerformLayout();
		this.splitContainer2.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).EndInit();
		this.splitContainer2.ResumeLayout(false);
		this.tool3D.ResumeLayout(false);
		this.tool3D.PerformLayout();
		this.groupGenericFace.ResumeLayout(false);
		this.groupGenericFace.PerformLayout();
		this.groupGenericFaceType.ResumeLayout(false);
		this.groupGenericFaceType.PerformLayout();
		this.groupHairModel.ResumeLayout(false);
		this.groupHairModel.PerformLayout();
		this.groupHeadModel.ResumeLayout(false);
		this.splitContainer3.Panel1.ResumeLayout(false);
		this.splitContainer3.Panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer3).EndInit();
		this.splitContainer3.ResumeLayout(false);
		this.groupSpecifiHeadControls.ResumeLayout(false);
		this.groupSpecifiHeadControls.PerformLayout();
		this.groupCommonHeadControls.ResumeLayout(false);
		this.groupCommonHeadControls.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericSkinTone).EndInit();
		base.ResumeLayout(false);
	}
}
