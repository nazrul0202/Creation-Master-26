using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class TeamForm : Form
{
	public Team m_CurrentTeam;

	private TabPage m_CurrentPage;

	private Formation m_CurrentFormation;

	private Formation m_BackupSpecificFormation;

	private bool m_IsLoaded;

	private bool m_Locked;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private TeamPlayer m_CurrentTeamPlayer;

	private Player m_CurrentAvailablePlayer;

	private Team m_CurrentAvailableTeam;

	private string m_TeamCurrentFolder = FifaEnvironment.ExportFolder;

	private bool m_AvailablePlayerLocked;

	private bool m_ChangeNumberFlag;

	private Label m_DraggedLabel;

	private Point m_LabelLocation = new Point(0, 0);

	private int m_BoundLeft;

	private int m_BoundRight = 250;

	private int m_BoundTop;

	private int m_BoundBottom = 350;

	private bool m_LockUserChanges;

	private bool m_Fc26TeamUiConfigured;

	private CareerBudgetEditor m_Fc26CareerBudgetEditor;

	private GroupBox groupFc26CareerBudget;

	private Label labelFc26CareerBudgetStatus;

	private NumericUpDown numericFc26CareerTransferBudget;

	private NumericUpDown numericFc26CareerStartBudget;

	private Button buttonFc26OpenCareer;

	private Button buttonFc26SaveCareerBudget;

	private bool m_Fc26CareerBudgetBusy;

	private ComboBox comboTraitContext;

	private CheckBox[] m_Fc26TraitChecks;

	private Label[] m_Fc26RosterLabels;

	private readonly Dictionary<int, Image> m_Fc26MiniFaceCache = new Dictionary<int, Image>();

	private const int Fc26MiniFaceCacheLimit = 256;

	private Image m_Fc26PitchBackground;

	private bool m_Fc26RosterLayoutBusy;

	private const int Fc26PitchHeight = 456;

	private const int Fc26SubstitutesTop = 464;

	private const int Fc26ReservesTop = 539;

	private int m_Fc26MiniFaceLoadingTeamId = -1;

	private int m_Fc26MiniFaceLoadGeneration;

	private QuickEditPlayerPanel[] m_QuickPanels = new QuickEditPlayerPanel[40];

	private DataTable m_WebPlayerTable = new DataTable("WebPlayer");

	private DataTable m_WebTeamTable = new DataTable("WebTeam");

	private Viewer3D viewer3DTeamManager;

	private Viewer3D viewer3DTeamBall;

	public string m_NewJerseyNumber = string.Empty;

	public int m_NewJerseyNum = -1;

	private IContainer components;

	public PickUpControl pickUpControl;

	private TabControl tableEditTeam;

	private TabPage pageTeamGeneric;

	private FlowLayoutPanel flowPanelTeamGeneric;

	private TabPage pageTeamRoster;

	private GroupBox groupBoxName;

	private TextBox textScoreBoardName;

	private TextBox textDatabaseTeamName;

	private TextBox textFullTeamName;

	private TextBox textStandardTeamName;

	private TextBox textShortTeamName;

	private Label labelDatabaseTeamName;

	private Label labelFullTeamName;

	private Label labelStandardTeamName;

	private Label labelShortTeamName;

	private Label labelScoreBoardName;

	private BindingSource teamBindingSource;

	private ToolTip toolTip;

	private GroupBox groupBox1;

	private ComboBox comboStadiums;

	private BindingSource stadiumListBindingSource;

	private Label labelStadium;

	private TextBox textStadiumName;

	private Label labelStadiumName;

	private GroupBox groupBox3;

	private ComboBox comboTeamCountry;

	private Label labelTeamCountry;

	private BindingSource countryListBindingSource;

	private Label labelOpponent;

	private BindingSource teamListBindingSource;

	private NumericStars numericStarsInternationalPrestige;

	private NumericStars numericStarsDomesticPrestige;

	private Label labelDomesticPrestige;

	private NumericUpDown numericInitialBudget;

	private Label labelInternationalPrestige;

	private Label labelInitialBudget;

	private PictureBox pictureTeamPrimColor;

	private PictureBox pictureTeamSecColor;

	private PictureBox pictureTeamTerColor;

	private ColorDialog colorDialog;

	private TabPage pageTeamAdboard;

	private TabPage pageTeamFlags;

	private BindingSource ballListBindingSource;

	public NumericUpDown numericAdboards;

	private Viewer2D viewer2DAdboards_0;

	private Label labelAdboard;

	private Viewer2D viewer2DBanners;

	private PictureBox pictureBall;

	private GroupBox groupAvailablePlayers;

	private ListView listViewPlayersAvailable;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private ColumnHeader columnHeader3;

	private ColumnHeader columnHeader4;

	private Panel panelAvailablePlayersTop;

	private Button buttonTransferFrom;

	private PickUpControl pickUpAvailablePlayers;

	private Button buttonCall;

	private Label labelAvailablePlayerStars;

	private Label labelRosterNameFrom;

	private PictureBox pictureAvailablePlayer;

	private GroupBox groupTeamPlayers;

	private ListView listViewTeamPlayers;

	private ColumnHeader columnRosterNum;

	private ColumnHeader columnRosterSurname;

	private ColumnHeader columnRosterFirstName;

	private ColumnHeader columnRosterYearContract;

	private ColumnHeader columnPreferredRole;

	private ColumnHeader columnAverageAttributes;

	private Panel panelTeamPlayersTop;

	private NumericUpDown numericRosterYear;

	private Label labelTeamPlayerStars;

	private Label labelRosterName;

	private ComboBox comboRosterNumber;

	private Button buttonTransferPlayer;

	private Button buttonRosterLetFree;

	private Label labelRosterNumber;

	private ImageList imageListStars;

	private Label label1;

	private NumericUpDown numericBall;

	private Button buttonGetId;

	private NumericUpDown numericTeamId;

	private Label labelTeamId;

	private DateTimePicker dateJoiningDate;

	private Label labelJoiningDate;

	private BindingSource formationListBindingSource;

	private ImageList imageListPlayers;

	private Viewer2D viewer2DCrest32;

	private Viewer2D viewer2DCrestLarge;

	private Viewer2D viewer2DCrest16;

	private GroupBox groupBox2;

	private Label label15;

	private Button buttonReplicateLogo;

	private GroupBox groupManager;

	private TextBox textBox3;

	private Label label17;

	private TextBox textBox2;

	private Label label16;

	private ImageList imageListArrows;

	private GroupBox groupLastYear;

	private CheckBox checkIsChampion;

	private Label label19;

	private Label label18;

	private NumericUpDown numericPositionLastYear;

	private ComboBox comboPrevLeague;

	private BindingSource leagueListBindingSource;

	private Label labelLeague;

	private ComboBox comboTeamLeague;

	private BindingSource prevLeagueListBindingSource;

	private ComboBox comboRivalTeam;

	private ComboBox comboObjective;

	private Label labelObjective;

	private Label labelProbObjective;

	private Label labelMaxObjective;

	private ComboBox comboProbObjective;

	private ComboBox comboMaxOnjective;

	private GroupBox groupTeamTraits;

	private CheckBox checkShortOutBack;

	private CheckBox checkMoreAttackingAtHome;

	private CheckBox checkCenterBacksSplit;

	private CheckBox checkSwitchWingers;

	private CheckBox checkKeepUpPressure;

	private CheckBox checkDefendLead;

	private CheckBox checkConsistentLineup;

	private CheckBox checkSquadRotation;

	private CheckBox checkLoyalBoard;

	private CheckBox checkImpatientBoard;

	private Viewer2D viewer2DCrest50;

	private Button buttonMinusContract;

	private Button buttonPlusContract;

	private GroupBox groupTeamPlayerTuning;

	private Button buttonTeamPlayerMinus;

	private Button buttonTeamPlayerPlus;

	private GroupBox groupFlag;

	private Label labelFlag1;

	private ImageList imageListFlags;

	private Label labelFlag4;

	private Label labelFlag3;

	private Label labelFlag2;

	private CheckBox checkFlag4;

	private CheckBox checkFlag3;

	private CheckBox checkFlag2;

	private CheckBox checkFlag1;

	private PictureBox pictureBox4;

	private Label label22;

	private PictureBox pictureFlagBlue;

	private PictureBox pictureFlagRed;

	private PictureBox pictureFlagGreen;

	private Button buttonCreateFlags;

	private CheckBox checkHasSpecificAdboard;

	private GroupBox groupLocation;

	private Label label25;

	private Label label24;

	private Label label23;

	private MultiViewer2D multiViewer2DFlags15;

	private Panel panel1;

	private Label labelPos33U;

	private Label labelPos33T;

	private Label labelPos33S;

	private Label labelPos33R;

	private Label labelPos33Q;

	private Label labelPos33O;

	private Label labelPos33P;

	private Label labelPos33N;

	private Label labelPos33M;

	private Label labelPos33L;

	private Label labelPos33K;

	private Label labelPos33J;

	private Label labelPos33H;

	private Label labelPos33I;

	private Label labelPos33G;

	private Label labelPos33F;

	private Label labelPos33E;

	private Label labelPos33D;

	private Label labelPos33C;

	private Label labelPos33A;

	private Label labelPos33B;

	private Label labelPos32G;

	private Label labelPos32F;

	private Label labelPos32E;

	private Label labelPos32D;

	private Label labelPos32C;

	private Label labelPos32A;

	private Label labelPos32B;

	private Label labelPos26;

	private Label labelPos27;

	private Label labelPos21;

	private Label labelPos22;

	private Label labelPos23;

	private Label labelPos24;

	private Label labelPos25;

	private Label labelPos14;

	private Label labelPos15;

	private Label labelPos16;

	private Label labelPos17;

	private Label labelPos18;

	private Label labelPos20;

	private Label labelPos19;

	private Label labelPos9;

	private Label labelPos10;

	private Label labelPos11;

	private Label labelPos12;

	private Label labelPos13;

	private Label labelPos2;

	private Label labelPos3;

	private Label labelPos4;

	private Label labelPos5;

	private Label labelPos6;

	private Label labelPos8;

	private Label labelPos7;

	private Label labelPos0;

	private Label labelPos1;

	private Label label20;

	private NumericUpDown numericUpDown2;

	private ComboBox comboDEFLine;

	private NumericUpDown numericDefteamwidth;

	private NumericUpDown numericDefaggression;

	private NumericUpDown numericDefmentality;

	private Label labelDefdefendeline;

	private Label labelDefteamwidth;

	private Label labelDefaggression;

	private Label labelDefmentality;

	private ComboBox comboCCPositioning;

	private NumericUpDown numericCcshooting;

	private NumericUpDown numericCccrossing;

	private NumericUpDown numericCcpassing;

	private Label labelCcpositioning;

	private Label labelCcshooting;

	private Label labelCccrossing;

	private Label labelCcpassing;

	private ComboBox comboBUSPositioning;

	private NumericUpDown numericBuspassing;

	private NumericUpDown numericBusbuildupspeed;

	private Label labelBuspositioning;

	private Label labelBuspassing;

	private Label labelBusbuildupspeed;

	private GroupBox groupBox6;

	private GroupBox groupBox5;

	private GroupBox groupBox4;

	private Label labelRightFreeKickText;

	private Label labelRightFreeKick;

	private Label labelLeftFreeKickText;

	private Label labelLeftFreeKick;

	private GroupBox groupFormation;

	private ComboBox comboGenericFormations;

	private RadioButton radioUseSpecificFormation;

	private RadioButton radioUseGenericFormation;

	private Label labelLongKick;

	private Label labelLomgKickText;

	private Label labelRightCornerText;

	private Label labelCaptainTetx;

	private Label labelLeftCornertext;

	private Label labelRightCorner;

	private Label labelCaptain;

	private Label labelLeftCorner;

	private Label labelFreeKickText;

	private Label labelPenaltyText;

	private Label labelPenalty;

	private Label labelFreeKick;

	private Label labelTeamFormationName;

	private Label label2;

	private Viewer2D viewer2DPhoto;

	private Label label3;

	private TextBox textTeamName7;

	private NumericUpDown numericUtcOffset;

	private NumericUpDown numericLongitude;

	private NumericUpDown numericLatitude;

	private TabPage pageTeamrevMod;

	private FlowLayoutPanel flowLayoutPanel1;

	private GroupBox groupTeamAdboardsRevMod;

	private Viewer2D viewer2DTeamAdboard;

	private GroupBox groupTeamGoalNetRevMod;

	private Viewer2D viewer2DTeamNet;

	private GroupBox groupTeamScarfRevMod;

	private MultiViewer2D multiViewer2DTeamScarf;

	private GroupBox groupTeamBallRevMod;

	private MultiViewer2D multiViewer2DTeamBallTextures;

	private ToolStrip toolTeamBall3D;

	private ToolStripButton buttonShow3DBall;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonImport3DModelTeamBall;

	private ToolStripButton buttonExport3DModelTeamBall;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton buttonRemove3DModelTeamBall;

	private GroupBox groupTeamManager;

	private ToolStrip toolTeamManager3D;

	private ToolStripButton buttonShow3DManager;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripButton buttonImportModel3DTeamManager;

	private ToolStripButton buttonExportModel3DTeamManager;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripButton buttonDeleteModel3DTeamManager;

	private Viewer2D viewer2DTeamManager;

	private CheckBox checkIsNationalTeam;

	private Label label4;

	private DateTimePicker dateTransferPreset;

	private Button buttonLoanFrom;

	private Button buttonLoanTo;

	private GroupBox groupBox7;

	private Label labelHomeKit;

	private Label labelThirdKit;

	private Label labelKeeprKit;

	private Label labelAwayKit;

	private ComboBox comboTeamLoanedFrom;

	private Label labelLoanedFrom;

	private DateTimePicker dateLoanEnd;

	private Label labelLoanEnd;

	private CheckBox checkIsLoan;

	private Label label5;

	private Label labelPreviousTeam;

	private ComboBox comboTeamPrevious;

	private Button buttonDeletePlayer;

	private Button buttonCreateNewFormation;

	private Button buttonTransferAll;

	public TeamForm()
	{
		base.Visible = false;
		InitializeComponent();
		viewer3DTeamManager = new Viewer3D();
		groupTeamManager.Controls.Add(viewer3DTeamManager);
		viewer3DTeamManager.Width = 256;
		viewer3DTeamManager.Height = 256;
		viewer3DTeamManager.Location = new Point(267, 19);
		viewer3DTeamManager.AmbientColor = Color.Black;
		viewer3DTeamManager.BackColor = Color.Gray;
		viewer3DTeamManager.BorderStyle = BorderStyle.Fixed3D;
		viewer3DTeamManager.LightDirectionX = 0.5f;
		viewer3DTeamManager.LightDirectionY = -0.25f;
		viewer3DTeamManager.LightDirectionZ = -1f;
		viewer3DTeamManager.LightX = -30f;
		viewer3DTeamManager.LightY = 10f;
		viewer3DTeamManager.LightZ = 30f;
		viewer3DTeamManager.Location = new Point(267, 19);
		viewer3DTeamManager.Margin = new Padding(4);
		viewer3DTeamManager.Name = "viewer3DTeamManager";
		viewer3DTeamManager.RotationX = 0f;
		viewer3DTeamManager.RotationY = 0f;
		viewer3DTeamManager.RotationYCoeff = 0.01f;
		viewer3DTeamManager.Size = new Size(256, 256);
		viewer3DTeamManager.TabIndex = 5;
		viewer3DTeamManager.ViewX = 0f;
		viewer3DTeamManager.ViewY = 150f;
		viewer3DTeamManager.ViewZ = 100f;
		viewer3DTeamManager.ZbufferRenderState = null;
		viewer3DTeamBall = new Viewer3D();
		groupTeamBallRevMod.Controls.Add(viewer3DTeamBall);
		viewer3DTeamBall.Width = 256;
		viewer3DTeamBall.Height = 256;
		viewer3DTeamBall.Location = new Point(267, 19);
		viewer3DTeamBall.AmbientColor = Color.Black;
		viewer3DTeamBall.BackColor = Color.Gray;
		viewer3DTeamBall.BorderStyle = BorderStyle.Fixed3D;
		viewer3DTeamBall.LightDirectionX = 0.5f;
		viewer3DTeamBall.LightDirectionY = -0.25f;
		viewer3DTeamBall.LightDirectionZ = -1f;
		viewer3DTeamBall.LightX = -30f;
		viewer3DTeamBall.LightY = 10f;
		viewer3DTeamBall.LightZ = 30f;
		viewer3DTeamBall.Location = new Point(265, 44);
		viewer3DTeamBall.Margin = new Padding(4);
		viewer3DTeamBall.Name = "viewer3DTeamBall";
		viewer3DTeamBall.RotationX = 0f;
		viewer3DTeamBall.RotationY = 0f;
		viewer3DTeamBall.RotationYCoeff = 0.01f;
		viewer3DTeamBall.Size = new Size(256, 256);
		viewer3DTeamBall.TabIndex = 3;
		viewer3DTeamBall.ViewX = 0f;
		viewer3DTeamBall.ViewY = 0f;
		viewer3DTeamBall.ViewZ = 30f;
		viewer3DTeamBall.ZbufferRenderState = null;
		pickUpControl.SelectObject = SelectTeam;
		pickUpControl.CreateObject = CreateTeam;
		pickUpControl.DeleteObject = DeleteTeam;
		pickUpControl.RefreshObject = RefreshTeam;
		viewer2DCrestLarge.ImageImport = ImportImageCrest;
		viewer2DCrestLarge.ImageDelete = DeleteCrest;
		viewer2DCrestLarge.ButtonStripVisible = true;
		viewer2DCrestLarge.RemoveButton = true;
		viewer2DCrest50.ImageImport = ImportImageCrest50;
		viewer2DCrest50.ImageDelete = DeleteCrest50;
		viewer2DCrest50.ButtonStripVisible = true;
		viewer2DCrest50.RemoveButton = true;
		viewer2DCrest32.ImageImport = ImportImageCrest32;
		viewer2DCrest32.ImageDelete = DeleteCrest32;
		viewer2DCrest32.ButtonStripVisible = true;
		viewer2DCrest32.RemoveButton = true;
		viewer2DCrest16.ImageImport = ImportImageCrest16;
		viewer2DCrest16.ImageDelete = DeleteCrest16;
		viewer2DCrest16.ButtonStripVisible = true;
		viewer2DCrest16.RemoveButton = true;
		viewer2DAdboards_0.ImageImport = ImportImageAdboard_0;
		viewer2DAdboards_0.ImageDelete = DeleteAdboard_0;
		viewer2DAdboards_0.ButtonStripVisible = true;
		viewer2DAdboards_0.FullSizeButton = true;
		viewer2DAdboards_0.RemoveButton = true;
		viewer2DAdboards_0.ShowButton = true;
		viewer2DAdboards_0.ShowButtonChecked = true;
		viewer2DBanners.ImageImport = ImportImageBanners;
		viewer2DBanners.ImageDelete = DeleteImageBanners;
		viewer2DBanners.ButtonStripVisible = true;
		viewer2DBanners.RemoveButton = true;
		multiViewer2DFlags15.Rx3ImportDelegate = ImportRx3Flags;
		multiViewer2DFlags15.Rx3ExportDelegate = ExportRx3Flags;
		multiViewer2DFlags15.Rx3SaveDelegate = SaveRx3Flags;
		multiViewer2DFlags15.Rx3DeleteDelegate = DeleteRx3Flags;
		viewer2DPhoto.ButtonStripVisible = true;
		viewer2DPhoto.ImageImport = ImportImageMiniface;
		viewer2DPhoto.ImageDelete = DeleteMiniface;
		viewer2DPhoto.ButtonStripVisible = true;
		viewer2DPhoto.RemoveButton = true;
		viewer2DPhoto.ShowButton = true;
		viewer2DPhoto.ShowButtonChecked = true;
		viewer2DTeamAdboard.ButtonStripVisible = true;
		viewer2DTeamAdboard.ImageImport = ImportRevModAdboard;
		viewer2DTeamAdboard.ImageDelete = DeleteRevModAdboard;
		viewer2DTeamNet.ButtonStripVisible = true;
		viewer2DTeamNet.ImageImport = ImportRevModNet;
		viewer2DTeamNet.ImageDelete = DeleteRevModNet;
		viewer2DTeamManager.ButtonStripVisible = true;
		viewer2DTeamManager.ImageImport = ImportImageManager;
		viewer2DTeamManager.ImageDelete = DeleteManager;
		multiViewer2DTeamScarf.Rx3ImportDelegate = ImportRx3Scarf;
		multiViewer2DTeamScarf.Rx3ExportDelegate = ExportRx3Scarf;
		multiViewer2DTeamScarf.Rx3SaveDelegate = SaveRx3Scarf;
		multiViewer2DTeamScarf.Rx3DeleteDelegate = DeleteRx3Scarf;
		multiViewer2DTeamBallTextures.Rx3ImportDelegate = ImportRx3BallTextures;
		multiViewer2DTeamBallTextures.Rx3ExportDelegate = ExportRx3BallTextures;
		multiViewer2DTeamBallTextures.Rx3SaveDelegate = SaveRx3BallTextures;
		multiViewer2DTeamBallTextures.Rx3DeleteDelegate = DeleteRx3BallTextures;
		pickUpAvailablePlayers.FilterChanged = AvailablePlayersFilterChanged;
		m_WebPlayerTable.Columns.Add("name");
		m_WebPlayerTable.Columns.Add("surname");
		m_WebPlayerTable.Columns.Add("country");
		m_WebPlayerTable.Columns.Add("birthdate");
		m_WebPlayerTable.Columns.Add("role");
		m_WebPlayerTable.Columns.Add("height");
		m_WebPlayerTable.Columns.Add("weight");
		m_WebPlayerTable.Columns.Add("foot");
		m_WebPlayerTable.Columns.Add("team");
		m_WebPlayerTable.Columns.Add("number");
		m_WebPlayerTable.Columns.Add("since");
		m_WebPlayerTable.Columns.Add("contract");
		m_WebPlayerTable.Columns.Add("previousteam");
		m_WebPlayerTable.Columns.Add("loanedfrom");
		m_WebPlayerTable.Columns.Add("loanenddate");
	}

	private void tableEditTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (tableEditTeam.SelectedIndex >= 0)
		{
			UpdateCurrentPage();
		}
	}

	public void UpdateCurrentPage()
	{
		m_CurrentPage = tableEditTeam.SelectedTab;
		if (m_CurrentPage == pageTeamAdboard)
		{
			LoadAdboardPage();
		}
		else if (m_CurrentPage == pageTeamFlags)
		{
			LoadFlagsPage();
		}
		else if (m_CurrentPage == pageTeamGeneric)
		{
			LoadGenericPage();
		}
		else if (m_CurrentPage == pageTeamRoster)
		{
			LoadRosterPage();
		}
		else if (m_CurrentPage == pageTeamrevMod)
		{
			LoadRevModPage();
		}
	}

	private bool ImportImageCrest(object sender, Bitmap bitmap)
	{
		m_CurrentTeam.SetCrestDark(bitmap);
		return m_CurrentTeam.SetCrest(bitmap);
	}

	private bool DeleteCrest(object sender)
	{
		return m_CurrentTeam.DeleteCrest();
	}

	private bool ImportImageCrest50(object sender, Bitmap bitmap)
	{
		m_CurrentTeam.SetCrest50Dark(bitmap);
		return m_CurrentTeam.SetCrest50(bitmap);
	}

	private bool DeleteCrest50(object sender)
	{
		m_CurrentTeam.DeleteCrest50Dark();
		return m_CurrentTeam.DeleteCrest50();
	}

	private bool ImportImageCrest32(object sender, Bitmap bitmap)
	{
		m_CurrentTeam.SetCrest32Dark(bitmap);
		return m_CurrentTeam.SetCrest32(bitmap);
	}

	private bool DeleteCrest32(object sender)
	{
		m_CurrentTeam.DeleteCrest32Dark();
		return m_CurrentTeam.DeleteCrest32();
	}

	private bool ImportImageCrest16(object sender, Bitmap bitmap)
	{
		m_CurrentTeam.SetCrest16Dark(bitmap);
		return m_CurrentTeam.SetCrest16(bitmap);
	}

	private bool DeleteCrest16(object sender)
	{
		m_CurrentTeam.DeleteCrest16Dark();
		return m_CurrentTeam.DeleteCrest16();
	}

	public void Clean()
	{
		base.Visible = false;
	}

	private bool ImportImageAdboard_0(object sender, Bitmap bitmap)
	{
		return m_CurrentTeam.SetAdboard(bitmap);
	}

	private bool DeleteAdboard_0(object sender)
	{
		return Adboard.DeleteAdboard(m_CurrentTeam.adboardid);
	}

	private bool ImportRevModAdboard(object sender, Bitmap bitmap)
	{
		return m_CurrentTeam.SetRevModAdboard(bitmap);
	}

	private bool DeleteRevModAdboard(object sender)
	{
		return Adboard.DeleteRevModTeamAdboard(m_CurrentTeam.Id);
	}

	private bool ImportRevModNet(object sender, Bitmap bitmap)
	{
		return m_CurrentTeam.SetRevModNet(bitmap);
	}

	private bool DeleteRevModNet(object sender)
	{
		return Net.DeleteRevModNet(m_CurrentTeam.Id);
	}

	private bool ImportImageBanners(object sender, Bitmap bitmap)
	{
		return m_CurrentTeam.SetBanner(bitmap);
	}

	private bool DeleteImageBanners(object sender)
	{
		return m_CurrentTeam.DeleteBanner();
	}

	private bool ImportRx3Flags(object sender, string rx3FileName)
	{
		bool num = m_CurrentTeam.SetFlags(rx3FileName);
		if (num)
		{
			LoadFlagsPage();
		}
		return num;
	}

	private bool ExportRx3Flags(object sender, string exportDir)
	{
		return m_CurrentTeam.ExportFlags(exportDir);
	}

	private bool SaveRx3Flags(object sender, Bitmap[] bitmaps)
	{
		return m_CurrentTeam.SetFlags(bitmaps);
	}

	private bool DeleteRx3Flags(object sender)
	{
		return m_CurrentTeam.DeleteFlag();
	}

	private bool ImportRx3Scarf(object sender, string rx3FileName)
	{
		return m_CurrentTeam.SetScarfs(rx3FileName);
	}

	private bool ExportRx3Scarf(object sender, string exportDir)
	{
		return m_CurrentTeam.ExportScarfs(exportDir);
	}

	private bool SaveRx3Scarf(object sender, Bitmap[] bitmaps)
	{
		return m_CurrentTeam.SetScarfs(bitmaps);
	}

	private bool DeleteRx3Scarf(object sender)
	{
		return m_CurrentTeam.DeleteScarf();
	}

	private bool ImportRx3BallTextures(object sender, string rx3FileName)
	{
		return m_CurrentTeam.SetRevModBallTextures(rx3FileName);
	}

	private bool ExportRx3BallTextures(object sender, string exportDir)
	{
		return m_CurrentTeam.ExportRevModBallTextures(exportDir);
	}

	private bool SaveRx3BallTextures(object sender, Bitmap[] bitmaps)
	{
		return m_CurrentTeam.SetRevModBallTextures(bitmaps);
	}

	private bool DeleteRx3BallTextures(object sender)
	{
		return m_CurrentTeam.DeleteRevModBallTextures();
	}

	private bool ImportImageMiniface(object sender, Bitmap bitmap)
	{
		return m_CurrentTeamPlayer.Player.SetPhoto(bitmap);
	}

	private bool DeleteMiniface(object sender)
	{
		return m_CurrentTeamPlayer.Player.DeletePhoto();
	}

	private Team SelectTeam(object sender, object obj)
	{
		Team team = (Team)obj;
		Refresh();
		LoadTeam(team);
		return team;
	}

	private Team CreateTeam(object sender, object obj)
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
		Team team = (Team)m_NewIdCreator.NewObject;
		if (m_NewIdCreator.NewName != null && team != null)
		{
			team.TeamNameFull = m_NewIdCreator.NewName;
			team.DatabaseName = m_NewIdCreator.NewName;
			if (team.TeamNameFull.Length > 15)
			{
				team.TeamNameAbbr15 = team.TeamNameFull.Substring(0, 15);
			}
			else
			{
				team.TeamNameAbbr15 = team.TeamNameFull;
			}
			if (team.TeamNameFull.Length > 10)
			{
				team.TeamNameAbbr10 = team.TeamNameFull.Substring(0, 10);
			}
			else
			{
				team.TeamNameAbbr10 = team.TeamNameFull;
			}
			if (team.TeamNameFull.Length > 3)
			{
				team.TeamNameAbbr3 = team.TeamNameFull.Substring(0, 3).ToUpper();
			}
			else
			{
				team.TeamNameAbbr3 = team.TeamNameFull;
			}
		}
		Formation formation = null;
		formation = FifaEnvironment.Formations.CreateNewFormation();
		if (formation != null)
		{
			formation.formationfullname = formation.Name;
			team.Formation = formation;
			team.formationid = formation.Id;
			formation.Team = team;
		}
		if (m_CurrentTeam != null)
		{
			team.Country = m_CurrentTeam.Country;
			team.adboardid = m_CurrentTeam.adboardid;
			team.balltype = m_CurrentTeam.balltype;
			team.Stadium = m_CurrentTeam.Stadium;
			team.RivalTeam = m_CurrentTeam.RivalTeam;
			team.latitude = m_CurrentTeam.latitude;
			team.longitude = m_CurrentTeam.longitude;
			team.utcoffset = m_CurrentTeam.utcoffset;
			team.highestpossible = m_CurrentTeam.highestpossible;
			team.highestprobable = m_CurrentTeam.highestprobable;
			team.objective = m_CurrentTeam.objective;
			team.previousyeartableposition = m_CurrentTeam.previousyeartableposition;
			team.transferbudget = m_CurrentTeam.transferbudget;
		}
		DialogResult dialogResult2 = FifaEnvironment.UserMessages.ShowMessage(15);
		if (dialogResult2 != DialogResult.No && dialogResult2 != DialogResult.Cancel)
		{
			_ = new Player[32];
			TeamPlayer[] array = new TeamPlayer[32];
			int num = ((m_CurrentTeam.Roster.Count > 32) ? 32 : m_CurrentTeam.Roster.Count);
			for (int i = 0; i < num; i++)
			{
				TeamPlayer teamPlayer = (TeamPlayer)m_CurrentTeam.Roster[i];
				Player player = teamPlayer.Player;
				int newId = FifaEnvironment.Players.GetNewId();
				Player player2 = (Player)FifaEnvironment.Players.CloneId(player, newId);
				player2.headclasscode = 1;
				player2.firstname = "";
				player2.lastname = "Player_" + player2.Id;
				player2.commonname = "";
				player2.playerjerseyname = "";
				player2.commentaryid = 900000;
				player2.RandomizeAppearanceSameRace();
				array[i] = new TeamPlayer(player2);
				array[i].position = teamPlayer.position;
				array[i].jerseynumber = teamPlayer.jerseynumber;
				team.AddTeamPlayer(array[i]);
			}
			team.AssignRoles();
			team.AssignBench();
			team.AssignCaptain();
			team.AssignFreeKick();
			team.AssignPenalty();
			team.AssignLeftCorner();
			team.AssignRightCorner();
		}
		return team;
	}

	private Team DeleteTeam(object sender, object obj)
	{
		Team team = (Team)obj;
		DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(30);
		switch (dialogResult)
		{
		case DialogResult.Cancel:
			return team;
		default:
			if (dialogResult == DialogResult.Cancel)
			{
				break;
			}
			foreach (TeamPlayer item in team.Roster)
			{
				if (item.Player.m_PlayingForTeams.Count <= 1)
				{
					FifaEnvironment.Players.RemoveId(item.Player.Id);
				}
				else
				{
					item.Player.NotPlayFor(team);
				}
			}
			break;
		case DialogResult.No:
			break;
		}
		foreach (Kit kit in team.m_KitList)
		{
			FifaEnvironment.Kits.RemoveId(kit);
		}
		if (team.League != null)
		{
			team.League.RemoveTeam(team);
		}
		FifaEnvironment.Teams.DeleteTeam(team);
		m_CurrentTeam = null;
		return null;
	}

	public Team RefreshTeam(object sender, object obj)
	{
		Preset();
		ReloadTeam(m_CurrentTeam);
		return m_CurrentTeam;
	}

	public void ReloadTeam(Team team)
	{
		m_CurrentTeam = null;
		LoadTeam(team);
	}

	public void LoadTeam(Team team)
	{
		if (!m_IsLoaded || team == null) return;
		m_Locked = true;
		try
		{
			if (m_CurrentTeam == team && m_CurrentPage == tableEditTeam.SelectedTab) return;
			m_CurrentTeam = team;
			teamBindingSource.DataSource = m_CurrentTeam;
			UpdateCurrentPage();
		}
		finally
		{
			// Returning early for an already-selected team used to leave the form
			// permanently locked, which made later section/record changes appear blank.
			m_Locked = false;
		}
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

	private void EnsureFc26TeamUi()
	{
		if (m_Fc26TeamUiConfigured || FifaEnvironment.Year != 26) return;
		m_Fc26TeamUiConfigured = true;

		// FC26 keeps three trait masks. Reuse the familiar CM16 check boxes, add
		// an explicit opponent context, and remove bindings to the obsolete trait1.
		m_Fc26TraitChecks = new[]
		{
			checkImpatientBoard, checkLoyalBoard, checkSquadRotation, checkConsistentLineup,
			checkSwitchWingers, checkCenterBacksSplit, checkDefendLead, checkKeepUpPressure,
			checkMoreAttackingAtHome, checkShortOutBack
		};
		foreach (CheckBox check in m_Fc26TraitChecks)
		{
			check.DataBindings.Clear();
			check.Top += 28;
			check.CheckedChanged += Fc26TraitCheck_CheckedChanged;
		}
		groupTeamTraits.Height += 28;
		var contextLabel = new Label
		{
			Text = "Opponent", Location = new Point(14, 20), Size = new Size(60, 21),
			TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent
		};
		comboTraitContext = new ComboBox
		{
			Location = new Point(78, 19), Size = new Size(176, 21),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		comboTraitContext.Items.AddRange(new object[] { "Weaker team", "Equal team", "Stronger team" });
		comboTraitContext.SelectedIndexChanged += delegate { LoadFc26TraitChecks(); };
		groupTeamTraits.Controls.Add(contextLabel);
		groupTeamTraits.Controls.Add(comboTraitContext);
		comboTraitContext.SelectedIndex = 1;

		// Replace the removed CM16 custom-tactic fields with the real FC26 values.
		comboBUSPositioning.DataBindings.Clear();
		comboBUSPositioning.Items.Clear();
		comboBUSPositioning.Items.AddRange(new object[] { "Short Passing", "Balanced", "Counter" });
		labelBuspositioning.Text = "Build Up Style";
		labelBuspositioning.Location = new Point(6, 25);
		comboBUSPositioning.Location = new Point(100, 21);
		comboBUSPositioning.Size = new Size(125, 21);
		numericBusbuildupspeed.Visible = false;
		numericBuspassing.Visible = false;
		numericUpDown2.Visible = false;
		labelBusbuildupspeed.Visible = false;
		labelBuspassing.Visible = false;
		label20.Visible = false;

		comboDEFLine.DataBindings.Clear();
		comboDEFLine.Items.Clear();
		comboDEFLine.Items.AddRange(new object[] { "Deep", "Balanced", "High", "Aggressive" });
		labelDefdefendeline.Text = "Approach";
		comboDEFLine.Location = new Point(100, 19);
		comboDEFLine.Size = new Size(124, 21);
		numericDefmentality.DataBindings.Clear();
		numericDefmentality.Minimum = 1;
		numericDefmentality.Maximum = 100;
		numericDefmentality.Location = new Point(160, 47);
		numericDefmentality.ValueChanged += Fc26DefensiveDepth_ValueChanged;
		labelDefmentality.Text = "Line Height (1-100)";
		numericDefaggression.Visible = false;
		numericDefteamwidth.Visible = false;
		labelDefaggression.Visible = false;
		labelDefteamwidth.Visible = false;
		groupBox6.Visible = false;

		// These values are generated inside a Career save. The squads database
		// contains zero placeholders, which the CM16 enum previously rendered as
		// the misleading "Win League Title" objective.
		if (comboObjective.Items.Count > 0) comboObjective.Items[0] = "Career generated (not in squads DB)";
		if (comboMaxOnjective.Items.Count > 0) comboMaxOnjective.Items[0] = "Career generated (not in squads DB)";
		if (comboProbObjective.Items.Count > 0) comboProbObjective.Items[0] = "Career generated (not in squads DB)";
		labelInitialBudget.Text = "Club Worth";
		ConfigureFc26CareerBudgetUi();

		ConfigureFc26RosterFormationUi();
	}

	private void ConfigureFc26CareerBudgetUi()
	{
		groupFc26CareerBudget = new GroupBox
		{
			Name = "groupFc26CareerBudget",
			Text = "FC26 Career Transfer Budget",
			Size = new Size(540, 183),
			TabStop = false
		};

		labelFc26CareerBudgetStatus = new Label
		{
			Location = new Point(14, 20),
			Size = new Size(506, 34),
			ForeColor = Color.DarkSlateBlue,
			BackColor = Color.Transparent
		};
		var currentBudgetLabel = new Label
		{
			Text = "Current Transfer Budget",
			Location = new Point(14, 60),
			Size = new Size(165, 21),
			TextAlign = ContentAlignment.MiddleLeft
		};
		numericFc26CareerTransferBudget = CreateFc26BudgetControl(new Point(184, 58));
		var startBudgetLabel = new Label
		{
			Text = "Start-of-season Budget",
			Location = new Point(14, 91),
			Size = new Size(165, 21),
			TextAlign = ContentAlignment.MiddleLeft
		};
		numericFc26CareerStartBudget = CreateFc26BudgetControl(new Point(184, 89));
		buttonFc26OpenCareer = new Button
		{
			Text = "Open Career Save...",
			Location = new Point(354, 57),
			Size = new Size(166, 27)
		};
		buttonFc26SaveCareerBudget = new Button
		{
			Text = "Save Budget + Backup",
			Location = new Point(354, 88),
			Size = new Size(166, 27)
		};
		var hint = new Label
		{
			Text = "Budget is stored in the Career save, separate from this team's Club Worth. A timestamped .bak file is created before every save.",
			Location = new Point(14, 124),
			Size = new Size(506, 43),
			ForeColor = Color.DimGray,
			BackColor = Color.Transparent
		};

		buttonFc26OpenCareer.Click += OpenFc26CareerBudget_Click;
		buttonFc26SaveCareerBudget.Click += SaveFc26CareerBudget_Click;
		groupFc26CareerBudget.Controls.Add(labelFc26CareerBudgetStatus);
		groupFc26CareerBudget.Controls.Add(currentBudgetLabel);
		groupFc26CareerBudget.Controls.Add(numericFc26CareerTransferBudget);
		groupFc26CareerBudget.Controls.Add(startBudgetLabel);
		groupFc26CareerBudget.Controls.Add(numericFc26CareerStartBudget);
		groupFc26CareerBudget.Controls.Add(buttonFc26OpenCareer);
		groupFc26CareerBudget.Controls.Add(buttonFc26SaveCareerBudget);
		groupFc26CareerBudget.Controls.Add(hint);
		flowPanelTeamGeneric.Controls.Add(groupFc26CareerBudget);
		RefreshFc26CareerBudgetUi();
	}

	private static NumericUpDown CreateFc26BudgetControl(Point location)
	{
		return new NumericUpDown
		{
			Location = location,
			Size = new Size(151, 20),
			Minimum = 0m,
			Maximum = 2147483520m,
			ThousandsSeparator = true,
			TextAlign = HorizontalAlignment.Right
		};
	}

	private async void OpenFc26CareerBudget_Click(object sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog
		{
			Title = "Open an EA SPORTS FC 26 Career save",
			Filter = "FC26 Career saves (Career*;*.sav)|Career*;*.sav|All files (*.*)|*.*",
			CheckFileExists = true,
			Multiselect = false
		};
		string settingsFolder = System.IO.Path.Combine(
			System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
			"EA SPORTS FC 26", "settings");
		if (System.IO.Directory.Exists(settingsFolder)) dialog.InitialDirectory = settingsFolder;
		if (dialog.ShowDialog(this) != DialogResult.OK) return;

		SetFc26CareerBudgetBusy(true, "Loading Career save...");
		try
		{
			string fileName = dialog.FileName;
			string schemaFile = FifaEnvironment.FifaXmlFileName;
			m_Fc26CareerBudgetEditor = await System.Threading.Tasks.Task.Run(
				() => CareerBudgetEditor.Open(fileName, schemaFile));
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "The Career budget could not be loaded.\r\n\r\n" + ex.Message,
				"FC26 Career Budget", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		finally
		{
			SetFc26CareerBudgetBusy(false, null);
			RefreshFc26CareerBudgetUi();
		}
	}

	private async void SaveFc26CareerBudget_Click(object sender, EventArgs e)
	{
		if (m_Fc26CareerBudgetEditor == null) return;
		int transferBudget = Decimal.ToInt32(numericFc26CareerTransferBudget.Value);
		int startBudget = Decimal.ToInt32(numericFc26CareerStartBudget.Value);
		SetFc26CareerBudgetBusy(true, "Saving Career budget and backup...");
		try
		{
			string backupFile = await System.Threading.Tasks.Task.Run(
				() => m_Fc26CareerBudgetEditor.Save(transferBudget, startBudget));
			MessageBox.Show(this,
				"Transfer budget saved to the Career file.\r\n\r\nBackup: " + backupFile,
				"FC26 Career Budget", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "The Career budget could not be saved.\r\n\r\n" + ex.Message,
				"FC26 Career Budget", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		finally
		{
			SetFc26CareerBudgetBusy(false, null);
			RefreshFc26CareerBudgetUi();
		}
	}

	private void SetFc26CareerBudgetBusy(bool busy, string status)
	{
		m_Fc26CareerBudgetBusy = busy;
		UseWaitCursor = busy;
		if (buttonFc26OpenCareer != null) buttonFc26OpenCareer.Enabled = !busy;
		if (buttonFc26SaveCareerBudget != null) buttonFc26SaveCareerBudget.Enabled = !busy && m_Fc26CareerBudgetEditor != null;
		if (numericFc26CareerTransferBudget != null) numericFc26CareerTransferBudget.Enabled = !busy && m_Fc26CareerBudgetEditor != null;
		if (numericFc26CareerStartBudget != null) numericFc26CareerStartBudget.Enabled = !busy && m_Fc26CareerBudgetEditor != null;
		if (busy && labelFc26CareerBudgetStatus != null) labelFc26CareerBudgetStatus.Text = status;
	}

	private void RefreshFc26CareerBudgetUi()
	{
		if (groupFc26CareerBudget == null) return;
		bool loaded = m_Fc26CareerBudgetEditor != null;
		buttonFc26OpenCareer.Enabled = !m_Fc26CareerBudgetBusy;
		buttonFc26SaveCareerBudget.Enabled = loaded && !m_Fc26CareerBudgetBusy;
		numericFc26CareerTransferBudget.Enabled = loaded && !m_Fc26CareerBudgetBusy;
		numericFc26CareerStartBudget.Enabled = loaded && !m_Fc26CareerBudgetBusy;
		if (!loaded)
		{
			labelFc26CareerBudgetStatus.Text = "No Career save loaded. Open a save to edit its real ingame budget.";
			return;
		}

		Team careerTeam = FifaEnvironment.Teams.SearchId(m_Fc26CareerBudgetEditor.ClubTeamId) as Team;
		string teamName = careerTeam?.DatabaseName ?? "Unknown team";
		string saveName = string.IsNullOrWhiteSpace(m_Fc26CareerBudgetEditor.InGameName)
			? System.IO.Path.GetFileName(m_Fc26CareerBudgetEditor.FileName)
			: m_Fc26CareerBudgetEditor.InGameName;
		labelFc26CareerBudgetStatus.Text = "Loaded: " + saveName + "  |  Active club: " + teamName
			+ " (Team ID " + m_Fc26CareerBudgetEditor.ClubTeamId + ")";
		if (m_CurrentTeam != null && m_CurrentTeam.Id != m_Fc26CareerBudgetEditor.ClubTeamId)
		{
			labelFc26CareerBudgetStatus.Text += "\r\nSelected squads team differs from the Career club.";
		}
		SetNumericValue(numericFc26CareerTransferBudget, m_Fc26CareerBudgetEditor.TransferBudget);
		SetNumericValue(numericFc26CareerStartBudget, m_Fc26CareerBudgetEditor.StartOfSeasonTransferBudget);
	}

	private void ConfigureFc26RosterFormationUi()
	{
		m_Fc26RosterLabels = new[]
		{
			labelPos0, labelPos1, labelPos2, labelPos3, labelPos4, labelPos5, labelPos6,
			labelPos7, labelPos8, labelPos9, labelPos10, labelPos11, labelPos12, labelPos13,
			labelPos14, labelPos15, labelPos16, labelPos17, labelPos18, labelPos19, labelPos20,
			labelPos21, labelPos22, labelPos23, labelPos24, labelPos25, labelPos26, labelPos27,
			labelPos32A, labelPos32B, labelPos32C, labelPos32D, labelPos32E, labelPos32F, labelPos32G,
			labelPos33A, labelPos33B, labelPos33C, labelPos33D, labelPos33E, labelPos33F, labelPos33G,
			labelPos33H, labelPos33I, labelPos33J, labelPos33K, labelPos33L, labelPos33M, labelPos33N,
			labelPos33O, labelPos33P, labelPos33Q, labelPos33R, labelPos33S, labelPos33T, labelPos33U
		};

		foreach (Label label in m_Fc26RosterLabels)
		{
			label.BackColor = Color.Transparent;
			label.ForeColor = Color.White;
			label.TextAlign = ContentAlignment.MiddleCenter;
			label.Paint += Fc26RosterLabel_Paint;
		}

		panel1.BackColor = Color.FromArgb(16, 22, 28);
		panel1.BackgroundImageLayout = ImageLayout.Stretch;
		pageTeamRoster.Resize += Fc26RosterPage_Resize;

		// FC26 has 29 database-native layouts. Make them directly editable on the
		// roster page; the blue formation name remains the advanced position editor.
		comboGenericFormations.Visible = true;
		comboGenericFormations.DropDownStyle = ComboBoxStyle.DropDownList;
		comboGenericFormations.Location = new Point(10, 50);
		comboGenericFormations.Size = new Size(212, 21);
		buttonCreateNewFormation.Location = new Point(194, 18);
		var chooseFormation = new Label
		{
			Text = "Choose FC26 layout", Location = new Point(10, 33), Size = new Size(150, 15),
			ForeColor = Color.DimGray, BackColor = Color.Transparent
		};
		var editFormation = new Label
		{
			Text = "Double-click the name for advanced position editing.",
			Location = new Point(10, 76), Size = new Size(212, 31),
			ForeColor = Color.DimGray, BackColor = Color.Transparent
		};
		groupFormation.Controls.Add(chooseFormation);
		groupFormation.Controls.Add(editFormation);
		LayoutFc26RosterFormationUi();
	}

	private void Fc26RosterPage_Resize(object sender, EventArgs e)
	{
		LayoutFc26RosterFormationUi();
	}

	private void LayoutFc26RosterFormationUi()
	{
		if (FifaEnvironment.Year != 26 || m_Fc26RosterLabels == null || m_Fc26RosterLayoutBusy)
		{
			return;
		}

		m_Fc26RosterLayoutBusy = true;
		try
		{
			const int left = 732;
			const int setPieceWidth = 85;
			int availableWidth = pageTeamRoster.ClientSize.Width - left - setPieceWidth - 24;
			int panelWidth = Math.Max(477, Math.Min(980, availableWidth));
			panel1.Bounds = new Rectangle(left, 3, panelWidth, 718);
			pageTeamRoster.AutoScrollMinSize = new Size(left + panelWidth + setPieceWidth + 24, 875);

			int setPieceLeft = panel1.Right + 8;
			foreach (Label label in new[]
			{
				labelCaptainTetx, labelCaptain, labelLeftCornertext, labelLeftCorner,
				labelRightCornerText, labelRightCorner, labelPenaltyText, labelPenalty,
				labelLomgKickText, labelLongKick, labelLeftFreeKickText, labelLeftFreeKick,
				labelRightFreeKickText, labelRightFreeKick, labelFreeKickText, labelFreeKick
			})
			{
				label.Left = setPieceLeft;
				label.Width = setPieceWidth;
			}

			int tacticsTop = panel1.Bottom + 8;
			groupFormation.Location = new Point(left, tacticsTop);
			groupBox4.Location = new Point(groupFormation.Right + 6, tacticsTop);
			groupBox5.Location = new Point(groupBox4.Right + 6, tacticsTop);

			LayoutFc26StartingCards();
			LayoutFc26CardGrid(m_Fc26RosterLabels.Skip(28).Take(7).ToArray(), Fc26SubstitutesTop + 17, 7, 52);
			LayoutFc26CardGrid(m_Fc26RosterLabels.Skip(35).Take(21).ToArray(), Fc26ReservesTop + 17, 7, 52);
			RefreshFc26PitchBackground();
		}
		finally
		{
			m_Fc26RosterLayoutBusy = false;
		}
	}

	private void LayoutFc26StartingCards()
	{
		if (m_CurrentFormation == null)
		{
			return;
		}

		int cardWidth = Math.Max(104, Math.Min(132, panel1.ClientSize.Width / 7));
		const int cardHeight = 56;
		const int marginX = 10;
		const int marginY = 10;
		int usableHeight = Math.Max(1, Fc26PitchHeight - cardHeight - marginY * 2);
		var placements = new List<Fc26FormationCardPlacement>();
		var usedRoleIds = new HashSet<int>();

		foreach (PlayingRole playingRole in m_CurrentFormation.PlayingRoles)
		{
			if (playingRole?.Role == null)
			{
				continue;
			}

			int roleId = (int)playingRole.Role.RoleId;
			if (roleId < 0 || roleId >= 28 || !usedRoleIds.Add(roleId))
			{
				continue;
			}

			Label label = m_Fc26RosterLabels[roleId];
			float normalizedX = Math.Max(0f, Math.Min(1f, playingRole.OffsetX / 100f));
			float normalizedY = Math.Max(0f, Math.Min(1f, playingRole.OffsetY / 100f));

			// FC26 stores the goalkeeper only a few percentage points below its
			// centre-backs. That is accurate for the data, but a 56 px card then
			// overlaps the defensive line. Keep the database coordinates intact
			// while reserving a readable visual lane for the goalkeeper and back line.
			float screenY = 1f - normalizedY;
			if (roleId == (int)ERole.Goalkeeper)
			{
				screenY = 1f;
			}
			else if (roleId >= (int)ERole.Sweeper && roleId <= (int)ERole.Left_Wing_Back)
			{
				screenY = Math.Min(screenY, 0.72f);
			}

			int y = marginY + (int)Math.Round(screenY * usableHeight);
			float cardCentreY = Math.Max(0f, Math.Min(1f,
				(y + cardHeight * 0.5f) / Math.Max(1f, Fc26PitchHeight)));
			float pitchLeft = 0.14f + (0.005f - 0.14f) * cardCentreY;
			float pitchRight = 0.86f + (0.995f - 0.86f) * cardCentreY;
			int left = marginX + (int)Math.Round(panel1.ClientSize.Width * pitchLeft);
			int right = panel1.ClientSize.Width - marginX -
				(int)Math.Round(panel1.ClientSize.Width * (1f - pitchRight));
			int usableRowWidth = Math.Max(1, right - left - cardWidth);
			int x = left + (int)Math.Round(normalizedX * usableRowWidth);
			placements.Add(new Fc26FormationCardPlacement(label, x, y, left, right - cardWidth));
		}

		// Database-native FC26 coordinates are centre points, not card rectangles.
		// Two forwards or a five-player defensive line can therefore overlap even
		// when their points are valid. Resolve only the visual X positions, keeping
		// the stored formation coordinates and vertical lanes unchanged.
		foreach (List<Fc26FormationCardPlacement> row in BuildFc26FormationRows(placements, cardHeight - 8))
		{
			ResolveFc26FormationRow(row, cardWidth, 7);
		}

		foreach (Fc26FormationCardPlacement placement in placements)
		{
			placement.Label.Bounds = new Rectangle(placement.X, placement.Y, cardWidth, cardHeight);
			placement.Label.BringToFront();
		}
	}

	private static List<List<Fc26FormationCardPlacement>> BuildFc26FormationRows(
		IEnumerable<Fc26FormationCardPlacement> placements, int verticalTolerance)
	{
		var rows = new List<List<Fc26FormationCardPlacement>>();
		foreach (Fc26FormationCardPlacement placement in placements.OrderBy(item => item.Y))
		{
			List<Fc26FormationCardPlacement> row = rows.FirstOrDefault(candidate =>
				candidate.Any(item => Math.Abs(item.Y - placement.Y) <= verticalTolerance));
			if (row == null)
			{
				row = new List<Fc26FormationCardPlacement>();
				rows.Add(row);
			}
			row.Add(placement);
		}
		return rows;
	}

	private static void ResolveFc26FormationRow(
		List<Fc26FormationCardPlacement> row, int cardWidth, int gap)
	{
		if (row.Count < 2)
		{
			return;
		}

		row.Sort((left, right) => left.X.CompareTo(right.X));
		int minimumX = row.Max(item => item.MinimumX);
		int maximumX = row.Min(item => item.MaximumX);
		int step = cardWidth + gap;
		if (maximumX - minimumX < step * (row.Count - 1))
		{
			minimumX = row.Min(item => item.MinimumX);
			maximumX = row.Max(item => item.MaximumX);
		}

		for (int i = 0; i < row.Count; i++)
		{
			row[i].X = Math.Max(minimumX, Math.Min(maximumX, row[i].X));
			if (i > 0)
			{
				row[i].X = Math.Max(row[i].X, row[i - 1].X + step);
			}
		}

		int overflow = row[row.Count - 1].X - maximumX;
		if (overflow > 0)
		{
			foreach (Fc26FormationCardPlacement item in row)
			{
				item.X -= overflow;
			}
		}

		for (int i = row.Count - 2; i >= 0; i--)
		{
			row[i].X = Math.Min(row[i].X, row[i + 1].X - step);
		}

		int underflow = minimumX - row[0].X;
		if (underflow > 0)
		{
			foreach (Fc26FormationCardPlacement item in row)
			{
				item.X += underflow;
			}
		}
	}

	private sealed class Fc26FormationCardPlacement
	{
		internal Fc26FormationCardPlacement(Label label, int x, int y, int minimumX, int maximumX)
		{
			Label = label;
			X = x;
			Y = y;
			MinimumX = minimumX;
			MaximumX = maximumX;
		}

		internal Label Label { get; }
		internal int X { get; set; }
		internal int Y { get; }
		internal int MinimumX { get; }
		internal int MaximumX { get; }
	}

	private void LayoutFc26CardGrid(Label[] labels, int top, int columns, int cardHeight)
	{
		const int margin = 8;
		const int gap = 5;
		int cardWidth = Math.Max(55, (panel1.ClientSize.Width - margin * 2 - gap * (columns - 1)) / columns);
		for (int i = 0; i < labels.Length; i++)
		{
			int row = i / columns;
			int column = i % columns;
			labels[i].Bounds = new Rectangle(
				margin + column * (cardWidth + gap),
				top + row * (cardHeight + gap),
				cardWidth,
				cardHeight);
			labels[i].BringToFront();
		}
	}

	private void RefreshFc26PitchBackground()
	{
		if (m_Fc26PitchBackground != null && m_Fc26PitchBackground.Size == panel1.ClientSize)
		{
			if (!ReferenceEquals(panel1.BackgroundImage, m_Fc26PitchBackground))
				panel1.BackgroundImage = m_Fc26PitchBackground;
			return;
		}

		panel1.BackgroundImage = null;
		m_Fc26PitchBackground?.Dispose();
		m_Fc26PitchBackground = CreateFc26PitchBackground(panel1.ClientSize);
		panel1.BackgroundImage = m_Fc26PitchBackground;
	}

	private static Image CreateFc26PitchBackground(Size size)
	{
		var bitmap = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
		using (Graphics graphics = Graphics.FromImage(bitmap))
		using (var background = new LinearGradientBrush(new Rectangle(Point.Empty, bitmap.Size),
			Color.FromArgb(7, 8, 9), Color.FromArgb(27, 27, 28), LinearGradientMode.Vertical))
		using (var pitchPen = new Pen(Color.FromArgb(116, 151, 154, 156), 1.2f))
		using (var sectionBrush = new SolidBrush(Color.FromArgb(190, 22, 27, 35)))
		using (var headingBrush = new SolidBrush(Color.FromArgb(235, 240, 243, 245)))
		using (var headingFont = new Font("Segoe UI", 8f, FontStyle.Bold))
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.FillRectangle(background, 0, 0, bitmap.Width, bitmap.Height);
			int pitchTop = 1;
			int pitchBottom = Fc26PitchHeight - 2;
			var topLeft = new Point((int)(bitmap.Width * 0.14f), pitchTop);
			var topRight = new Point((int)(bitmap.Width * 0.86f), pitchTop);
			var bottomRight = new Point(bitmap.Width - 2, pitchBottom);
			var bottomLeft = new Point(1, pitchBottom);
			graphics.DrawPolygon(pitchPen, new[] { topLeft, topRight, bottomRight, bottomLeft });

			// Mirror the supplied FC26 base without redistributing its source texture.
			int halfY = (int)Math.Round(Fc26PitchHeight * 0.418f);
			float halfProgress = (halfY - pitchTop) / (float)Math.Max(1, pitchBottom - pitchTop);
			int halfLeft = (int)Math.Round(topLeft.X + (bottomLeft.X - topLeft.X) * halfProgress);
			int halfRight = (int)Math.Round(topRight.X + (bottomRight.X - topRight.X) * halfProgress);
			graphics.DrawLine(pitchPen, halfLeft, halfY, halfRight, halfY);
			int centreWidth = (int)Math.Round(bitmap.Width * 0.282f);
			int centreHeight = (int)Math.Round(Fc26PitchHeight * 0.176f);
			graphics.DrawEllipse(pitchPen, bitmap.Width / 2 - centreWidth / 2,
				halfY - centreHeight / 2, centreWidth, centreHeight);
			graphics.DrawEllipse(pitchPen, bitmap.Width / 2 - 2, halfY - 2, 4, 4);

			graphics.DrawPolygon(pitchPen, new[]
			{
				new Point((int)(bitmap.Width * 0.27f), pitchTop), new Point((int)(bitmap.Width * 0.73f), pitchTop),
				new Point((int)(bitmap.Width * 0.80f), (int)(Fc26PitchHeight * 0.135f)),
				new Point((int)(bitmap.Width * 0.20f), (int)(Fc26PitchHeight * 0.135f))
			});
			graphics.DrawPolygon(pitchPen, new[]
			{
				new Point((int)(bitmap.Width * 0.39f), pitchTop), new Point((int)(bitmap.Width * 0.61f), pitchTop),
				new Point((int)(bitmap.Width * 0.65f), (int)(Fc26PitchHeight * 0.064f)),
				new Point((int)(bitmap.Width * 0.35f), (int)(Fc26PitchHeight * 0.064f))
			});
			graphics.DrawPolygon(pitchPen, new[]
			{
				new Point((int)(bitmap.Width * 0.194f), pitchBottom), new Point((int)(bitmap.Width * 0.806f), pitchBottom),
				new Point((int)(bitmap.Width * 0.776f), (int)(Fc26PitchHeight * 0.77f)),
				new Point((int)(bitmap.Width * 0.212f), (int)(Fc26PitchHeight * 0.77f))
			});
			graphics.DrawPolygon(pitchPen, new[]
			{
				new Point((int)(bitmap.Width * 0.35f), pitchBottom), new Point((int)(bitmap.Width * 0.65f), pitchBottom),
				new Point((int)(bitmap.Width * 0.632f), (int)(Fc26PitchHeight * 0.89f)),
				new Point((int)(bitmap.Width * 0.357f), (int)(Fc26PitchHeight * 0.89f))
			});

			graphics.FillRectangle(sectionBrush, 0, Fc26SubstitutesTop, bitmap.Width,
				Fc26ReservesTop - Fc26SubstitutesTop - 3);
			graphics.FillRectangle(sectionBrush, 0, Fc26ReservesTop, bitmap.Width,
				bitmap.Height - Fc26ReservesTop);
			graphics.DrawString("SUBSTITUTES", headingFont, headingBrush, 8, Fc26SubstitutesTop + 1);
			graphics.DrawString("RESERVES", headingFont, headingBrush, 8, Fc26ReservesTop + 1);
		}
		return bitmap;
	}

	private void Fc26RosterLabel_Paint(object sender, PaintEventArgs e)
	{
		var label = sender as Label;
		if (label == null || FifaEnvironment.Year != 26) return;
		var teamPlayer = label.Tag as TeamPlayer;
		var player = teamPlayer == null ? null : teamPlayer.Player;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

		var bounds = new Rectangle(0, 0, Math.Max(1, label.Width - 1), Math.Max(1, label.Height - 1));
		using (GraphicsPath card = CreateRoundedRectangle(bounds, 5))
		using (var fill = new SolidBrush(teamPlayer == m_CurrentTeamPlayer
			? Color.FromArgb(255, 45, 55, 68) : Color.FromArgb(255, 20, 24, 30)))
		using (var border = new Pen(teamPlayer == m_CurrentTeamPlayer
			? Color.FromArgb(0, 226, 230) : Color.FromArgb(125, 112, 124, 132)))
		{
			e.Graphics.FillPath(fill, card);
			e.Graphics.DrawPath(border, card);
		}

		if (player == null)
		{
			using (var placeholder = new SolidBrush(Color.FromArgb(105, 205, 211, 216)))
			using (var font = new Font("Segoe UI", 6.5f, FontStyle.Regular))
			using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
			{
				string role = "Open slot";
				if (teamPlayer != null)
				{
					role = teamPlayer.position == 28
						? "Substitute"
						: teamPlayer.position == 29
							? "Reserve"
							: ((ERole)teamPlayer.position).ToString().Replace('_', ' ');
				}
				e.Graphics.DrawString(role, font, placeholder, bounds, format);
			}
			return;
		}

		int faceSize = label.Width >= 85 ? 35 : 27;
		var faceBounds = new Rectangle(3, Math.Max(2, (label.Height - faceSize) / 2), faceSize, faceSize);
		if (m_Fc26MiniFaceCache.TryGetValue(player.Id, out Image face))
		{
			GraphicsState state = e.Graphics.Save();
			using (var clip = new GraphicsPath())
			{
				clip.AddEllipse(faceBounds);
				e.Graphics.SetClip(clip);
				e.Graphics.DrawImage(face, faceBounds);
			}
			e.Graphics.Restore(state);
		}
		else
		{
			using (var fallback = new SolidBrush(Color.FromArgb(72, 88, 100)))
				e.Graphics.FillEllipse(fallback, faceBounds);
		}

		int textLeft = faceBounds.Right + 3;
		int textWidth = Math.Max(12, label.Width - textLeft - 3);
		Color ratingColor = player.overallrating >= 80 ? Color.FromArgb(22, 220, 112)
			: player.overallrating >= 70 ? Color.FromArgb(248, 204, 65) : Color.White;
		using (var ratingBrush = new SolidBrush(ratingColor))
		using (var nameBrush = new SolidBrush(Color.White))
		using (var ratingFont = new Font("Segoe UI", label.Width >= 85 ? 9.5f : 8f, FontStyle.Bold))
		using (var nameFont = new Font("Segoe UI", label.Width >= 85 ? 8.2f : 7f, FontStyle.Bold))
		using (var format = new StringFormat { Alignment = StringAlignment.Far,
			Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
		{
			e.Graphics.DrawString(player.overallrating.ToString(), ratingFont, ratingBrush,
				new RectangleF(textLeft, 2, textWidth, 16), format);
			e.Graphics.DrawString(player.Name, nameFont, nameBrush,
				new RectangleF(textLeft, 20, textWidth, label.Height - 22), format);
		}
		using (var fitness = new Pen(Color.FromArgb(18, 232, 118), 2.3f))
			e.Graphics.DrawLine(fitness, textLeft, label.Height - 3, label.Width - 4, label.Height - 3);
	}

	private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
	{
		var path = new GraphicsPath();
		int diameter = Math.Max(2, radius * 2);
		path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
		path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
		path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
		path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
		path.CloseFigure();
		return path;
	}

	private async void LoadFc26RosterMiniFacesAsync(Team team)
	{
		if (FifaEnvironment.Year != 26 || team == null || team.Roster == null) return;
		var rosterEntries = team.Roster.Cast<TeamPlayer>()
			.Where(value => value.Player != null).ToArray();
		var players = rosterEntries.Select(value => value.Player).Distinct().ToArray();
		var missingPlayers = players.Where(player => !m_Fc26MiniFaceCache.ContainsKey(player.Id)).ToArray();
		InvalidateFc26RosterLabels();
		if (missingPlayers.Length == 0 || m_Fc26MiniFaceLoadingTeamId == team.Id)
		{
			return;
		}

		int generation = ++m_Fc26MiniFaceLoadGeneration;
		m_Fc26MiniFaceLoadingTeamId = team.Id;
		Dictionary<int, Image> loadedImages = null;
		try
		{
			var missingIds = new HashSet<int>(missingPlayers.Select(player => player.Id));
			Player[] startingPlayers = rosterEntries
				.Where(value => value.position >= 0 && value.position < 28 && missingIds.Contains(value.Player.Id))
				.Select(value => value.Player).Distinct().ToArray();
			var startingIds = new HashSet<int>(startingPlayers.Select(player => player.Id));
			Player[] remainingPlayers = missingPlayers.Where(player => !startingIds.Contains(player.Id)).ToArray();

			// Decode the visible XI first so the pitch becomes useful immediately. The
			// substitutes and reserves continue afterwards, still away from the UI thread.
			foreach (Player[] batch in new[] { startingPlayers, remainingPlayers })
			{
				if (batch.Length == 0) continue;
				loadedImages = await System.Threading.Tasks.Task.Run(() => DecodeFc26MiniFaces(batch));

				if (IsDisposed || Disposing || generation != m_Fc26MiniFaceLoadGeneration || m_CurrentTeam != team)
				{
					DisposeFc26Images(loadedImages.Values);
					loadedImages = null;
					return;
				}

				foreach (KeyValuePair<int, Image> pair in loadedImages)
				{
					if (m_Fc26MiniFaceCache.TryGetValue(pair.Key, out Image previous)) previous.Dispose();
					m_Fc26MiniFaceCache[pair.Key] = pair.Value;
				}
				loadedImages = null;
				InvalidateFc26RosterLabels();
			}
			TrimFc26MiniFaceCache(players.Select(player => player.Id));
		}
		catch (Exception ex)
		{
			if (loadedImages != null) DisposeFc26Images(loadedImages.Values);
			System.Diagnostics.Debug.WriteLine(ex);
		}
		finally
		{
			if (generation == m_Fc26MiniFaceLoadGeneration) m_Fc26MiniFaceLoadingTeamId = -1;
		}
	}

	private static Dictionary<int, Image> DecodeFc26MiniFaces(IEnumerable<Player> players)
	{
		Player[] batch = players.ToArray();
		Fc26HostBridge.PreloadAssets(batch.Select(value => value.SpecificPhotoDdsFileName()));
		var decoded = new Dictionary<int, Image>();
		foreach (Player player in batch)
		{
			try
			{
				using (Bitmap photo = player.GetPhoto())
				{
					if (photo != null) decoded[player.Id] = new Bitmap(photo);
				}
			}
			catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
		}
		return decoded;
	}

	private void TrimFc26MiniFaceCache(IEnumerable<int> currentPlayerIds)
	{
		if (m_Fc26MiniFaceCache.Count <= Fc26MiniFaceCacheLimit) return;
		var current = new HashSet<int>(currentPlayerIds);
		foreach (int playerId in m_Fc26MiniFaceCache.Keys.Where(id => !current.Contains(id)).ToArray())
		{
			m_Fc26MiniFaceCache[playerId].Dispose();
			m_Fc26MiniFaceCache.Remove(playerId);
			if (m_Fc26MiniFaceCache.Count <= Fc26MiniFaceCacheLimit) break;
		}
	}

	private static void DisposeFc26Images(IEnumerable<Image> images)
	{
		foreach (Image image in images) image?.Dispose();
	}

	private void InvalidateFc26RosterLabels()
	{
		if (m_Fc26RosterLabels == null) return;
		foreach (Label label in m_Fc26RosterLabels)
		{
			// FC26 cards are fully owner-drawn from Label.Tag. Clearing the inherited
			// CM16 text prevents the same player name appearing underneath the card.
			label.Text = string.Empty;
			label.Invalidate();
		}
	}

	private void LoadFc26TraitChecks()
	{
		if (FifaEnvironment.Year != 26 || m_CurrentTeam == null || comboTraitContext == null ||
			m_Fc26TraitChecks == null || comboTraitContext.SelectedIndex < 0) return;
		bool oldLock = m_LockUserChanges;
		m_LockUserChanges = true;
		try
		{
			int mask = m_CurrentTeam.GetFc26TraitMask(comboTraitContext.SelectedIndex);
			for (int bit = 0; bit < m_Fc26TraitChecks.Length; bit++)
				m_Fc26TraitChecks[bit].Checked = (mask & (1 << bit)) != 0;
		}
		finally { m_LockUserChanges = oldLock; }
	}

	private void Fc26TraitCheck_CheckedChanged(object sender, EventArgs e)
	{
		if (m_LockUserChanges || m_CurrentTeam == null || comboTraitContext == null ||
			m_Fc26TraitChecks == null || comboTraitContext.SelectedIndex < 0) return;
		int knownMask = 0;
		for (int bit = 0; bit < m_Fc26TraitChecks.Length; bit++)
			if (m_Fc26TraitChecks[bit].Checked) knownMask |= 1 << bit;
		m_CurrentTeam.SetFc26KnownTraitMask(comboTraitContext.SelectedIndex, knownMask);
	}

	private static int Fc26DefensivePresetIndex(int depth)
	{
		if (depth <= 30) return 0;
		if (depth <= 60) return 1;
		if (depth < 90) return 2;
		return 3;
	}

	private void LoadFc26Tactics()
	{
		if (FifaEnvironment.Year != 26 || m_CurrentTeam == null) return;
		bool oldLock = m_LockUserChanges;
		m_LockUserChanges = true;
		try
		{
			SetSelectedIndex(comboBUSPositioning, m_CurrentTeam.buildupplay - 1);
			SetNumericValue(numericDefmentality, m_CurrentTeam.defensivedepth);
			SetSelectedIndex(comboDEFLine, Fc26DefensivePresetIndex(m_CurrentTeam.defensivedepth));
		}
		finally { m_LockUserChanges = oldLock; }
	}

	private void Fc26DefensiveDepth_ValueChanged(object sender, EventArgs e)
	{
		if (m_LockUserChanges || FifaEnvironment.Year != 26 || m_CurrentTeam == null) return;
		m_CurrentTeam.defensivedepth = (int)numericDefmentality.Value;
		bool oldLock = m_LockUserChanges;
		m_LockUserChanges = true;
		SetSelectedIndex(comboDEFLine, Fc26DefensivePresetIndex(m_CurrentTeam.defensivedepth));
		m_LockUserChanges = oldLock;
	}

	public void LoadGenericPage()
	{
		EnsureFc26TeamUi();
		SetNumericValue(numericTeamId, m_CurrentTeam.Id);
		comboRivalTeam.SelectedItem = m_CurrentTeam.RivalTeam;
		checkIsNationalTeam.Checked = m_CurrentTeam.NationalTeam;
		SetSelectedIndex(comboObjective, m_CurrentTeam.objective);
		SetSelectedIndex(comboMaxOnjective, m_CurrentTeam.highestpossible);
		SetSelectedIndex(comboProbObjective, m_CurrentTeam.highestprobable);
		if (FifaEnvironment.Year == 26)
		{
			bool hasStoredObjective = m_CurrentTeam.objective != 0 || m_CurrentTeam.highestpossible != 0 || m_CurrentTeam.highestprobable != 0;
			comboObjective.Enabled = hasStoredObjective;
			comboMaxOnjective.Enabled = hasStoredObjective;
			comboProbObjective.Enabled = hasStoredObjective;
		}
		teamBindingSource.ResetBindings(metadataChanged: false);
		LoadFc26TraitChecks();
		RefreshFc26CareerBudgetUi();
		viewer2DCrestLarge.CurrentBitmap = null;
		viewer2DCrest50.CurrentBitmap = null;
		viewer2DCrest32.CurrentBitmap = null;
		viewer2DCrest16.CurrentBitmap = null;
		LoadTeamCrestsAsync(m_CurrentTeam);
		if (m_CurrentTeam.Stadium == null)
		{
			comboStadiums.Text = string.Empty;
		}
		if (m_CurrentTeam.Country == null)
		{
			comboTeamCountry.Text = string.Empty;
		}
		if (m_CurrentTeam.League == null)
		{
			comboTeamLeague.Text = string.Empty;
		}
	}

	private async void LoadTeamCrestsAsync(Team team)
	{
		try
		{
			await System.Threading.Tasks.Task.Run(() => Fc26HostBridge.PreloadAssets(
				new[] { team.CrestDdsFileName(), team.Crest50DdsFileName(),
					team.Crest32DdsFileName(), team.Crest16DdsFileName() }));
			if (IsDisposed || Disposing || m_CurrentTeam != team || tableEditTeam.SelectedTab != pageTeamGeneric) return;
			viewer2DCrestLarge.CurrentBitmap = team.GetCrest();
			viewer2DCrest50.CurrentBitmap = team.GetCrest50();
			viewer2DCrest32.CurrentBitmap = team.GetCrest32();
			viewer2DCrest16.CurrentBitmap = team.GetCrest16();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}

	public void LoadAdboardPage()
	{
		if (m_CurrentTeam == null)
		{
			viewer2DAdboards_0.CurrentBitmap = null;
			return;
		}
		m_LockUserChanges = true;
		viewer2DAdboards_0.CurrentBitmap = m_CurrentTeam.GetAdboard();
		checkHasSpecificAdboard.Checked = m_CurrentTeam.HasSpecifiAdboard;
		numericAdboards.Enabled = !m_CurrentTeam.HasSpecifiAdboard;
		SetNumericValue(numericAdboards, m_CurrentTeam.adboardid);
		m_LockUserChanges = false;
	}

	public void LoadRevModPage()
	{
		viewer2DTeamAdboard.CurrentBitmap = m_CurrentTeam.GetRevModAdboard();
		viewer2DTeamManager.CurrentBitmap = m_CurrentTeam.GetRevModManagerTexture();
		Show3DManager();
		multiViewer2DTeamBallTextures.Bitmaps = m_CurrentTeam.GetRevModBallTextures();
		viewer2DTeamNet.CurrentBitmap = m_CurrentTeam.GetRevModNet();
		multiViewer2DTeamScarf.Bitmaps = m_CurrentTeam.GetScarfs();
	}

	public void Show3DManager()
	{
		if (!buttonShow3DManager.Checked)
		{
			viewer3DTeamManager.ShowEmpty();
			return;
		}
		Bitmap currentBitmap = viewer2DTeamManager.CurrentBitmap;
		if (currentBitmap == null)
		{
			viewer3DTeamManager.ShowEmpty();
			return;
		}
		Rx3File revModManagerModel = Manager.GetRevModManagerModel(m_CurrentTeam.Id);
		if (currentBitmap == null || revModManagerModel == null)
		{
			viewer3DTeamManager.Clean(1);
			return;
		}
		Rx3IndexArray.TriangleListType = Rx3IndexArray.ETriangleListType.InvertEven;
		Model3D model3D = new Model3D(revModManagerModel.Rx3IndexArrays[0], revModManagerModel.Rx3VertexArrays[0], currentBitmap);
		viewer3DTeamManager.Clean(1);
		viewer3DTeamManager.SetMesh(0, model3D);
		viewer3DTeamManager.Render();
	}

	public void LoadFlagsPage()
	{
		viewer2DBanners.CurrentBitmap = m_CurrentTeam.GetBanner();
		multiViewer2DFlags15.Bitmaps = m_CurrentTeam.GetFlags();
		pictureFlagRed.BackColor = m_CurrentTeam.TeamColor1;
		pictureFlagGreen.BackColor = m_CurrentTeam.TeamColor2;
		pictureFlagBlue.BackColor = m_CurrentTeam.TeamColor3;
	}

	public void LoadRosterPage()
	{
		EnsureFc26TeamUi();
		InitListViewTeamPlayers(m_CurrentTeam.Roster);
		m_CurrentFormation = m_CurrentTeam.Formation;
		if (m_CurrentFormation == null && FifaEnvironment.Year == 26 && FifaEnvironment.Formations != null)
		{
			// FC26's teamformationteamstylelinks table is empty. The authoritative
			// relationship is formations.teamid, so recover it directly when the
			// legacy link pass did not attach the formation object.
			m_CurrentFormation = FifaEnvironment.Formations.SearchByTeamId(m_CurrentTeam.Id);
			if (m_CurrentFormation != null)
				m_CurrentTeam.LinkFormation(m_CurrentFormation);
		}
		m_BackupSpecificFormation = null;
		if (m_CurrentFormation == null)
		{
			labelTeamFormationName.Text = "Formation not available";
			buttonCreateNewFormation.Enabled = true;
		}
		else
		{
			labelTeamFormationName.Text = m_CurrentFormation.ToString();
			buttonCreateNewFormation.Enabled = false;
		}
		InitVisualFormation(m_CurrentTeam.Roster);
		SelectFc26FormationPreset();
		LoadFc26RosterMiniFacesAsync(m_CurrentTeam);
		LoadFc26Tactics();
	}

	private void SelectFc26FormationPreset()
	{
		if (FifaEnvironment.Year != 26 || m_CurrentFormation == null || comboGenericFormations.Items.Count == 0) return;
		bool oldLock = m_LockUserChanges;
		m_LockUserChanges = true;
		try
		{
			int layoutId = m_CurrentFormation.relativeformationid;
			int selected = -1;
			for (int index = 0; index < comboGenericFormations.Items.Count; index++)
			{
				var preset = comboGenericFormations.Items[index] as Formation;
				if (preset != null && (preset.Id == layoutId ||
					(layoutId <= 0 && string.Equals(preset.formationfullname,
						m_CurrentFormation.formationfullname, StringComparison.OrdinalIgnoreCase))))
				{
					selected = index;
					break;
				}
			}
			comboGenericFormations.SelectedIndex = selected;
		}
		finally { m_LockUserChanges = oldLock; }
	}

	public void AuditFc26RecordsForSmoke()
	{
		if (!m_IsLoaded || FifaEnvironment.Teams == null || FifaEnvironment.Teams.Count == 0) return;
		Team originalTeam = m_CurrentTeam;
		TabPage originalPage = tableEditTeam.SelectedTab;
		var samples = new[] { 0, FifaEnvironment.Teams.Count / 2, FifaEnvironment.Teams.Count - 1 };
		try
		{
			foreach (int index in samples)
			{
				Team team = (Team)FifaEnvironment.Teams[index];
				tableEditTeam.SelectedTab = pageTeamGeneric;
				ReloadTeam(team);
				tableEditTeam.SelectedTab = pageTeamRoster;
				ReloadTeam(team);
				Application.DoEvents();
			}
			if (FifaEnvironment.Year == 26)
			{
				var formationNames = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (object item in comboGenericFormations.Items) formationNames.Add(item.ToString());
				if (comboGenericFormations.Items.Count != 29 || formationNames.Count != 29)
					throw new InvalidOperationException("FC26's 29 generic formation variants were not preserved.");
				Team heidenheim = FifaEnvironment.Teams.SearchId(111235) as Team;
				if (heidenheim != null)
				{
					tableEditTeam.SelectedTab = pageTeamRoster;
					ReloadTeam(heidenheim);
					Application.DoEvents();
					if (heidenheim.Formation == null || labelTeamFormationName.Text.IndexOf("5-4-1", StringComparison.Ordinal) < 0)
						throw new InvalidOperationException("FC26 team formation was not linked from formations.teamid.");
					var linkedRoleIds = new System.Collections.Generic.HashSet<int>();
					foreach (PlayingRole playingRole in heidenheim.Formation.PlayingRoles)
						if (playingRole?.Role != null) linkedRoleIds.Add((int)playingRole.Role.RoleId);
					int startingPlayers = 0;
					foreach (TeamPlayer teamPlayer in heidenheim.Roster)
						if (teamPlayer.position >= 0 && teamPlayer.position < 28) startingPlayers++;
					if (linkedRoleIds.Count != 11 || startingPlayers != 11)
						throw new InvalidOperationException("FC26 Starting XI roles collapsed or were moved into reserves.");
					if (m_Fc26RosterLabels.Any(label => !string.IsNullOrEmpty(label.Text)))
						throw new InvalidOperationException("FC26 owner-drawn formation cards still contain duplicate legacy text.");
					if (comboBUSPositioning.SelectedIndex != heidenheim.buildupplay - 1 ||
						(int)numericDefmentality.Value != heidenheim.defensivedepth)
						throw new InvalidOperationException("FC26 build-up style or defensive line height was not rendered.");
					tableEditTeam.SelectedTab = pageTeamGeneric;
					ReloadTeam(heidenheim);
					comboTraitContext.SelectedIndex = 1;
					LoadFc26TraitChecks();
					int expectedKnown = heidenheim.GetFc26TraitMask(1) & 1023;
					int renderedKnown = 0;
					for (int bit = 0; bit < m_Fc26TraitChecks.Length; bit++)
						if (m_Fc26TraitChecks[bit].Checked) renderedKnown |= 1 << bit;
					if (renderedKnown != expectedKnown)
						throw new InvalidOperationException("FC26 opponent-context team traits were not rendered.");
					if (heidenheim.objective == 0 && comboObjective.Enabled)
						throw new InvalidOperationException("FC26 Career-generated objective placeholder is incorrectly editable.");
				}
			}
		}
		finally
		{
			if (originalPage != null) tableEditTeam.SelectedTab = originalPage;
			if (originalTeam != null) ReloadTeam(originalTeam);
		}
	}

	private void buttonCreateNewFormation_Click(object sender, EventArgs e)
	{
		m_CurrentFormation = FifaEnvironment.Formations.CreateNewFormation();
		m_CurrentFormation.LinkTeam(m_CurrentTeam);
		m_CurrentTeam.LinkFormation(m_CurrentFormation);
		if (m_CurrentTeam.Roster.Count >= 11)
		{
			m_CurrentTeam.AssignTitolarToRoles(m_CurrentFormation);
		}
		else
		{
			m_CurrentTeam.AssignRoles();
		}
		m_CurrentTeam.AssignCaptain();
		m_CurrentTeam.AssignFreeKick();
		m_CurrentTeam.AssignPenalty();
		m_CurrentTeam.AssignLeftCorner();
		m_CurrentTeam.AssignRightCorner();
		labelTeamFormationName.Text = m_CurrentFormation.ToString();
		buttonCreateNewFormation.Enabled = false;
		InitVisualFormation(m_CurrentTeam.Roster);
		if (m_CurrentTeam.Formation != null)
		{
			MainForm.CM.JumpTo(m_CurrentTeam.Formation);
		}
	}

	public void LoadRosterGridPage()
	{
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.Teams;
		IdArrayList[] filterValues = new IdArrayList[5]
		{
			null,
			FifaEnvironment.Leagues,
			FifaEnvironment.Countries,
			new NoLeagueClubList(),
			new MissedKitClubList()
		};
		pickUpControl.FilterValues = filterValues;
		numericBall.Maximum = FifaEnvironment.Year == 26 ? 99999 : FifaEnvironment.FifaDb.Table[TI.teams].TableDescriptor.MaxValues[FI.teams_balltype];
		numericAdboards.Maximum = FifaEnvironment.Year == 26 ? 99999 : FifaEnvironment.FifaDb.Table[TI.teams].TableDescriptor.MaxValues[FI.teams_adboardid];
		numericTeamId.Maximum = FifaEnvironment.Year == 26 ? 999999 : FifaEnvironment.FifaDb.Table[TI.teams].TableDescriptor.MaxValues[FI.teams_teamid];
		teamListBindingSource.DataSource = FifaEnvironment.Teams;
		comboRivalTeam.DataSource = teamListBindingSource;
		teamListBindingSource.ResetBindings(metadataChanged: false);
		stadiumListBindingSource.DataSource = FifaEnvironment.Stadiums;
		comboStadiums.DataSource = stadiumListBindingSource;
		stadiumListBindingSource.ResetBindings(metadataChanged: false);
		countryListBindingSource.DataSource = FifaEnvironment.Countries;
		comboTeamCountry.DataSource = countryListBindingSource;
		countryListBindingSource.ResetBindings(metadataChanged: false);
		leagueListBindingSource.DataSource = FifaEnvironment.Leagues;
		prevLeagueListBindingSource.DataSource = FifaEnvironment.Leagues;
		comboTeamLeague.DataSource = leagueListBindingSource;
		comboPrevLeague.DataSource = prevLeagueListBindingSource;
		leagueListBindingSource.ResetBindings(metadataChanged: false);
		prevLeagueListBindingSource.ResetBindings(metadataChanged: false);
		IdArrayList[] filterValues2 = new IdArrayList[5]
		{
			null,
			FifaEnvironment.Teams,
			FifaEnvironment.Countries,
			FifaEnvironment.Roles,
			FifaEnvironment.FreeAgents
		};
		pickUpAvailablePlayers.FilterValues = filterValues2;
		pickUpAvailablePlayers.comboFilterValue.Width = 300;
		comboGenericFormations.Items.Clear();
		foreach (Formation formation in FifaEnvironment.Formations)
		{
			if (formation.IsGeneric() && formation.formations_issweeper == 0)
			{
				comboGenericFormations.Items.Add(formation);
			}
		}
		pickUpControl.ObjectList = FifaEnvironment.Teams;
		labelRightFreeKickText.Visible = FifaEnvironment.Year > 14;
		labelLeftFreeKickText.Visible = FifaEnvironment.Year > 14;
		labelLeftFreeKick.Visible = FifaEnvironment.Year > 14;
		labelRightFreeKick.Visible = FifaEnvironment.Year > 14;
		checkHasSpecificAdboard.Enabled = FifaEnvironment.Year > 14;
	}

	public void RefreshComboBoxes()
	{
		if (comboRivalTeam.Items.Count != FifaEnvironment.Teams.Count)
		{
			comboRivalTeam.Items.Clear();
			comboRivalTeam.Items.AddRange(FifaEnvironment.Teams.ToArray());
		}
	}

	private void TeamForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void numericTeamId_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		int num = (int)numericTeamId.Value;
		if (num == m_CurrentTeam.Id)
		{
			return;
		}
		if (FifaEnvironment.Teams.SearchId(num) == null)
		{
			FifaEnvironment.Teams.ChangeId(m_CurrentTeam, num);
			m_CurrentTeam.assetid = num;
			m_CurrentTeam.m_KitList = new KitList();
			m_CurrentTeam.LinkKits(FifaEnvironment.Kits);
			foreach (Kit kit in m_CurrentTeam.m_KitList)
			{
				kit.Team = m_CurrentTeam;
			}
			if (m_CurrentFormation != null)
			{
				m_CurrentFormation.Team = m_CurrentTeam;
			}
			LoadGenericPage();
			LoadFlagsPage();
		}
		else
		{
			FifaEnvironment.UserMessages.ShowMessage(1015);
			numericTeamId.Value = m_CurrentTeam.Id;
		}
	}

	private void buttonGetId_Click(object sender, EventArgs e)
	{
		int newId = FifaEnvironment.Teams.GetNewId();
		if (newId == -1)
		{
			FifaEnvironment.UserMessages.ShowMessage(5050);
		}
		else
		{
			numericTeamId.Value = newId;
		}
	}

	private void pictureTeamPrimColor_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureTeamPrimColor.BackColor;
		colorDialog.ShowDialog();
		pictureTeamPrimColor.BackColor = colorDialog.Color;
		m_CurrentTeam.TeamColor1 = colorDialog.Color;
	}

	private void pictureTeamSecColor_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureTeamSecColor.BackColor;
		colorDialog.ShowDialog();
		pictureTeamSecColor.BackColor = colorDialog.Color;
		m_CurrentTeam.TeamColor2 = colorDialog.Color;
	}

	private void pictureTeamTerColor_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureTeamTerColor.BackColor;
		colorDialog.ShowDialog();
		pictureTeamTerColor.BackColor = colorDialog.Color;
		m_CurrentTeam.TeamColor3 = colorDialog.Color;
	}

	private void numericAdboards_ValueChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			if (m_CurrentTeam == null)
			{
				viewer2DAdboards_0.CurrentBitmap = null;
				return;
			}
			m_CurrentTeam.adboardid = (int)numericAdboards.Value;
			viewer2DAdboards_0.CurrentBitmap = m_CurrentTeam.GetAdboard();
		}
	}

	private void InitListViewTeamPlayers(Roster roster)
	{
		InitListViewTeamPlayers(roster, null);
	}

	private void InitListViewTeamPlayers(Roster roster, TeamPlayer selectedTeamPlayer)
	{
		listViewTeamPlayers.BeginUpdate();
		listViewTeamPlayers.Items.Clear();
		for (int i = 0; i < roster.Count; i++)
		{
			TeamPlayer teamPlayer = (TeamPlayer)roster[i];
			string s = teamPlayer.jerseynumber.ToString();
			s = FifaUtil.PadBlanks(s, 2);
			ListViewItem listViewItem = new ListViewItem(teamPlayer.Player.Name);
			listViewItem.Tag = teamPlayer;
			listViewItem.SubItems.Add(teamPlayer.Player.firstname);
			listViewItem.SubItems.Add(teamPlayer.Player.contractvaliduntil.ToString());
			listViewItem.SubItems.Add(teamPlayer.Player.GetRoleAcronym());
			listViewItem.SubItems.Add(teamPlayer.Player.overallrating.ToString());
			listViewItem.SubItems.Add(s);
			listViewTeamPlayers.Items.Add(listViewItem);
		}
		if (selectedTeamPlayer == null)
		{
			if (listViewTeamPlayers.Items.Count > 0)
			{
				listViewTeamPlayers.Items[0].Selected = true;
			}
			else
			{
				m_CurrentTeamPlayer = null;
				CleanRosterTeamPlayer();
			}
		}
		else
		{
			for (int j = 0; j < listViewTeamPlayers.Items.Count; j++)
			{
				if (listViewTeamPlayers.Items[j].Tag == selectedTeamPlayer)
				{
					listViewTeamPlayers.Items[j].Selected = true;
				}
				else
				{
					listViewTeamPlayers.Items[j].Selected = false;
				}
			}
		}
		listViewTeamPlayers.EndUpdate();
	}

	private void InitListViewPlayersAvailable(Team team, Country country, bool showFreeAgents)
	{
		if (m_AvailablePlayerLocked)
		{
			return;
		}
		m_AvailablePlayerLocked = true;
		bool flag = true;
		IComparer listViewItemSorter = listViewPlayersAvailable.ListViewItemSorter;
		listViewPlayersAvailable.ListViewItemSorter = null;
		listViewPlayersAvailable.BeginUpdate();
		listViewPlayersAvailable.Items.Clear();
		for (int i = 0; i < FifaEnvironment.Players.Count; i++)
		{
			Player player = (Player)FifaEnvironment.Players[i];
			if ((!flag || player.Id < 400000 || player.Id >= 500000) && (!showFreeAgents || player.m_PlayingForTeams.Count <= 0) && (team == null || player.IsPlayingFor(team)) && (country == null || player.Country == country))
			{
				ListViewItem listViewItem = new ListViewItem(player.Name);
				listViewItem.Tag = player;
				listViewItem.SubItems.Add(player.firstname);
				string roleAcronym = player.GetRoleAcronym();
				listViewItem.SubItems.Add(roleAcronym);
				int averageRoleAttribute = player.GetAverageRoleAttribute();
				listViewItem.SubItems.Add(averageRoleAttribute.ToString());
				listViewPlayersAvailable.Items.Add(listViewItem);
			}
		}
		if (listViewPlayersAvailable.Items.Count > 0)
		{
			listViewPlayersAvailable.Items[0].Selected = true;
		}
		listViewPlayersAvailable.EndUpdate();
		EnableRosterButtons();
		listViewPlayersAvailable.ListViewItemSorter = listViewItemSorter;
		m_AvailablePlayerLocked = false;
	}

	private void InitListViewPlayersAvailable(IdObject filterObject, bool excludeYoung)
	{
		if (m_AvailablePlayerLocked)
		{
			return;
		}
		m_AvailablePlayerLocked = true;
		IComparer listViewItemSorter = listViewPlayersAvailable.ListViewItemSorter;
		listViewPlayersAvailable.ListViewItemSorter = null;
		listViewPlayersAvailable.BeginUpdate();
		listViewPlayersAvailable.Items.Clear();
		PlayerList playerList = (PlayerList)FifaEnvironment.Players.Filter(filterObject);
		for (int i = 0; i < playerList.Count; i++)
		{
			Player player = (Player)playerList[i];
			_ = player.Id;
			_ = 209610;
			if (!(player.Id > 400000 && excludeYoung))
			{
				ListViewItem listViewItem = new ListViewItem(player.Name);
				listViewItem.Tag = player;
				listViewItem.SubItems.Add(player.firstname);
				string roleAcronym = player.GetRoleAcronym();
				listViewItem.SubItems.Add(roleAcronym);
				int averageRoleAttribute = player.GetAverageRoleAttribute();
				listViewItem.SubItems.Add(averageRoleAttribute.ToString());
				listViewPlayersAvailable.Items.Add(listViewItem);
			}
		}
		if (listViewPlayersAvailable.Items.Count > 0)
		{
			listViewPlayersAvailable.Items[0].Selected = true;
		}
		listViewPlayersAvailable.EndUpdate();
		EnableRosterButtons();
		listViewPlayersAvailable.ListViewItemSorter = listViewItemSorter;
		m_AvailablePlayerLocked = false;
	}

	private void EnableRosterButtons()
	{
		if (m_CurrentTeamPlayer != null)
		{
			buttonRosterLetFree.Enabled = true;
			if (m_CurrentAvailableTeam != null && m_CurrentAvailableTeam != m_CurrentTeam && m_CurrentAvailableTeam.Id != 0)
			{
				buttonTransferPlayer.Enabled = true;
				buttonLoanTo.Enabled = true;
			}
			else
			{
				buttonTransferPlayer.Enabled = false;
				buttonLoanTo.Enabled = false;
			}
		}
		else
		{
			buttonTransferPlayer.Enabled = false;
			buttonLoanTo.Enabled = false;
			buttonRosterLetFree.Enabled = false;
		}
		if (m_CurrentAvailablePlayer == null)
		{
			buttonTransferFrom.Enabled = false;
			buttonLoanFrom.Enabled = false;
			buttonCall.Enabled = false;
		}
		else if (m_CurrentAvailablePlayer.IsPlayingFor(m_CurrentTeam))
		{
			buttonTransferFrom.Enabled = false;
			buttonLoanFrom.Enabled = false;
			buttonCall.Enabled = false;
		}
		else
		{
			buttonTransferFrom.Enabled = true;
			buttonLoanFrom.Enabled = true;
			buttonCall.Enabled = true;
		}
	}

	private void CleanRosterTeamPlayer()
	{
		labelRosterName.Text = string.Empty;
		comboRosterNumber.Items.Clear();
		comboRosterNumber.Text = string.Empty;
		numericRosterYear.Value = 2014m;
		viewer2DPhoto.CurrentBitmap = null;
		labelTeamPlayerStars.ImageIndex = 0;
	}

	private void buttonTransferFrom_Click(object sender, EventArgs e)
	{
		Team team = null;
		if (m_CurrentAvailableTeam != null)
		{
			team = m_CurrentAvailableTeam;
		}
		else
		{
			for (int i = 0; i < m_CurrentAvailablePlayer.m_PlayingForTeams.Count; i++)
			{
				Team team2 = (Team)m_CurrentAvailablePlayer.m_PlayingForTeams[i];
				if (!team2.NationalTeam)
				{
					team = team2;
					break;
				}
			}
		}
		team?.RemoveTeamPlayer(m_CurrentAvailablePlayer);
		TeamPlayer selectedTeamPlayer = m_CurrentTeam.AddTeamPlayer(m_CurrentAvailablePlayer);
		m_CurrentAvailablePlayer.joindate = dateTransferPreset.Value;
		m_CurrentAvailablePlayer.IsLoaned = false;
		m_CurrentAvailablePlayer.TeamLoanedFrom = team;
		if (m_CurrentAvailablePlayer.contractvaliduntil < m_CurrentAvailablePlayer.joindate.Year + 1)
		{
			m_CurrentAvailablePlayer.contractvaliduntil = m_CurrentAvailablePlayer.joindate.Year + 1;
		}
		m_CurrentAvailablePlayer.PreviousTeam = team;
		InitListViewTeamPlayers(m_CurrentTeam.Roster, selectedTeamPlayer);
		InitVisualFormation(m_CurrentTeam.Roster);
		if (m_CurrentAvailableTeam != null)
		{
			InitListViewPlayersAvailable(m_CurrentAvailableTeam, excludeYoung: false);
		}
		EnableRosterButtons();
	}

	private void buttonLoanFrom_Click(object sender, EventArgs e)
	{
		Team team = null;
		if (m_CurrentAvailableTeam != null)
		{
			team = m_CurrentAvailableTeam;
		}
		else
		{
			for (int i = 0; i < m_CurrentAvailablePlayer.m_PlayingForTeams.Count; i++)
			{
				Team team2 = (Team)m_CurrentAvailablePlayer.m_PlayingForTeams[i];
				if (!team2.NationalTeam)
				{
					team = team2;
					break;
				}
			}
		}
		team?.RemoveTeamPlayer(m_CurrentAvailablePlayer);
		TeamPlayer selectedTeamPlayer = m_CurrentTeam.AddTeamPlayer(m_CurrentAvailablePlayer);
		m_CurrentAvailablePlayer.joindate = dateTransferPreset.Value;
		m_CurrentAvailablePlayer.loandateend = m_CurrentAvailablePlayer.joindate.AddDays(364.0);
		m_CurrentAvailablePlayer.TeamLoanedFrom = team;
		m_CurrentAvailablePlayer.IsLoaned = true;
		InitListViewTeamPlayers(m_CurrentTeam.Roster, selectedTeamPlayer);
		InitVisualFormation(m_CurrentTeam.Roster);
		if (m_CurrentAvailableTeam != null)
		{
			InitListViewPlayersAvailable(m_CurrentAvailableTeam, excludeYoung: false);
		}
		EnableRosterButtons();
	}

	private void buttonCall_Click(object sender, EventArgs e)
	{
		TeamPlayer selectedTeamPlayer = m_CurrentTeam.AddTeamPlayer(m_CurrentAvailablePlayer);
		InitListViewTeamPlayers(m_CurrentTeam.Roster, selectedTeamPlayer);
		InitVisualFormation(m_CurrentTeam.Roster);
		EnableRosterButtons();
	}

	private void buttonRosterLetFree_Click(object sender, EventArgs e)
	{
		m_CurrentTeam.RemoveTeamPlayer(m_CurrentTeamPlayer);
		InitListViewTeamPlayers(m_CurrentTeam.Roster);
		InitVisualFormation(m_CurrentTeam.Roster);
		EnableRosterButtons();
	}

	private void listViewTeamPlayers_DoubleClick(object sender, EventArgs e)
	{
		if (listViewTeamPlayers.SelectedItems.Count <= 0)
		{
			return;
		}
		TeamPlayer teamPlayer = (TeamPlayer)listViewTeamPlayers.SelectedItems[0].Tag;
		if (teamPlayer != null)
		{
			Player player = teamPlayer.Player;
			if (player != null)
			{
				MainForm.CM.JumpTo(player);
			}
		}
	}

	private void listViewTeamPlayers_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listViewTeamPlayers.SelectedItems.Count <= 0)
		{
			return;
		}
		TeamPlayer teamPlayer = (TeamPlayer)listViewTeamPlayers.SelectedItems[0].Tag;
		if (m_CurrentTeamPlayer == teamPlayer)
		{
			return;
		}
		m_ChangeNumberFlag = false;
		m_CurrentTeamPlayer = teamPlayer;
		if (m_CurrentTeamPlayer != null)
		{
			comboRosterNumber.Items.Clear();
			comboRosterNumber.Items.Add(m_CurrentTeamPlayer.jerseynumber.ToString());
			string[] freeNumbers = m_CurrentTeam.Roster.GetFreeNumbers();
			ComboBox.ObjectCollection items = comboRosterNumber.Items;
			object[] items2 = freeNumbers;
			items.AddRange(items2);
			comboRosterNumber.SelectedIndex = 0;
			m_Locked = true;
			labelRosterName.Text = m_CurrentTeamPlayer.Player.Name;
			numericRosterYear.Value = m_CurrentTeamPlayer.Player.contractvaliduntil;
			if (m_CurrentTeamPlayer.Player.joindate.Year == 1)
			{
				m_CurrentTeamPlayer.Player.joindate = new DateTime(2017, 7, 1);
			}
			dateJoiningDate.Value = m_CurrentTeamPlayer.Player.joindate;
			checkIsLoan.Checked = m_CurrentTeamPlayer.Player.IsLoaned;
			if (checkIsLoan.Checked)
			{
				if (m_CurrentTeamPlayer.Player.loandateend < dateLoanEnd.MinDate || m_CurrentTeamPlayer.Player.loandateend > dateLoanEnd.MaxDate)
				{
					m_CurrentTeamPlayer.Player.loandateend = m_CurrentTeamPlayer.Player.joindate.AddDays(364.0);
				}
				dateLoanEnd.Value = m_CurrentTeamPlayer.Player.loandateend;
				comboTeamLoanedFrom.SelectedItem = m_CurrentTeamPlayer.Player.TeamLoanedFrom;
			}
			else if (m_CurrentTeamPlayer.Player.PreviousTeam == null)
			{
				comboTeamPrevious.SelectedItem = m_CurrentTeamPlayer.Team;
			}
			else
			{
				comboTeamPrevious.SelectedItem = m_CurrentTeamPlayer.Player.PreviousTeam;
			}
			viewer2DPhoto.CurrentBitmap = m_CurrentTeamPlayer.Player.GetPhoto();
			int averageRoleAttribute = m_CurrentTeamPlayer.Player.GetAverageRoleAttribute();
			averageRoleAttribute = (averageRoleAttribute - 45) / 5;
			if (averageRoleAttribute < 0)
			{
				averageRoleAttribute = 0;
			}
			if (averageRoleAttribute > 9)
			{
				averageRoleAttribute = 9;
			}
			labelTeamPlayerStars.ImageIndex = averageRoleAttribute;
			EnableRosterButtons();
			m_Locked = false;
		}
		else
		{
			CleanRosterTeamPlayer();
			EnableRosterButtons();
		}
		m_ChangeNumberFlag = true;
		InvalidateFc26RosterLabels();
	}

	private void buttonTransferPlayer_Click(object sender, EventArgs e)
	{
		m_CurrentTeam.RemoveTeamPlayer(m_CurrentTeamPlayer);
		m_CurrentAvailableTeam.AddTeamPlayer(m_CurrentTeamPlayer);
		m_CurrentTeamPlayer.Player.joindate = dateTransferPreset.Value;
		m_CurrentTeamPlayer.Player.IsLoaned = false;
		m_CurrentTeamPlayer.Player.TeamLoanedFrom = m_CurrentTeam;
		if (m_CurrentTeamPlayer.Player.contractvaliduntil < m_CurrentTeamPlayer.Player.joindate.Year + 1)
		{
			m_CurrentTeamPlayer.Player.contractvaliduntil = m_CurrentTeamPlayer.Player.joindate.Year + 1;
		}
		m_CurrentTeamPlayer.Player.PreviousTeam = m_CurrentTeam;
		InitListViewTeamPlayers(m_CurrentTeam.Roster);
		InitListViewPlayersAvailable(m_CurrentAvailableTeam, null, showFreeAgents: false);
		EnableRosterButtons();
		InitVisualFormation(m_CurrentTeam.Roster);
	}

	private Team AvailablePlayersFilterChanged(object sender, object obj)
	{
		if (m_AvailablePlayerLocked)
		{
			return null;
		}
		m_CurrentAvailableTeam = null;
		if (obj != null && obj.GetType().Name == "Team")
		{
			Team currentAvailableTeam = (Team)obj;
			m_CurrentAvailableTeam = currentAvailableTeam;
		}
		InitListViewPlayersAvailable((IdObject)obj, excludeYoung: false);
		return null;
	}

	private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		ListView obj = (ListView)sender;
		SortOrder sortOrder = SortOrder.None;
		obj.ListViewItemSorter = new ListViewItemComparer(sortOrder: obj.Sorting = ((obj.Sorting != SortOrder.Ascending) ? SortOrder.Ascending : SortOrder.Descending), column: e.Column);
	}

	private void listViewPlayersAvailable_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listViewPlayersAvailable.SelectedItems.Count >= 1)
		{
			m_CurrentAvailablePlayer = (Player)listViewPlayersAvailable.SelectedItems[0].Tag;
			labelRosterNameFrom.Text = m_CurrentAvailablePlayer.Name;
			pictureAvailablePlayer.Image = m_CurrentAvailablePlayer.GetPhoto();
			int averageRoleAttribute = m_CurrentAvailablePlayer.GetAverageRoleAttribute();
			averageRoleAttribute = (averageRoleAttribute - 45) / 5;
			if (averageRoleAttribute < 0)
			{
				averageRoleAttribute = 0;
			}
			if (averageRoleAttribute > 9)
			{
				averageRoleAttribute = 9;
			}
			labelAvailablePlayerStars.ImageIndex = averageRoleAttribute;
			EnableRosterButtons();
		}
	}

	private void listViewPlayersAvailable_DoubleClick(object sender, EventArgs e)
	{
		if (listViewPlayersAvailable.SelectedItems.Count > 0)
		{
			Player player = (Player)listViewPlayersAvailable.SelectedItems[0].Tag;
			if (player != null)
			{
				MainForm.CM.JumpTo(player);
			}
		}
	}

	private void numericBall_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentTeam != null)
		{
			m_CurrentTeam.balltype = (int)numericBall.Value;
			pictureBall.BackgroundImage = Ball.GetBallPicture(m_CurrentTeam.balltype);
		}
	}

	private void labelTeamCountry_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentTeam.Country != null)
		{
			MainForm.CM.JumpTo(m_CurrentTeam.Country);
		}
	}

	private void labelTeamLeague_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentTeam.League != null)
		{
			MainForm.CM.JumpTo(m_CurrentTeam.League);
		}
	}

	private void labelTeamStadium_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentTeam.Stadium != null)
		{
			MainForm.CM.JumpTo(m_CurrentTeam.Stadium);
		}
	}

	private void labelOpponent_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentTeam.RivalTeam != null)
		{
			MainForm.CM.JumpTo(m_CurrentTeam.RivalTeam);
		}
	}

	private void labelBall_DoubleClick(object sender, EventArgs e)
	{
		Ball ball = (Ball)FifaEnvironment.Balls.SearchId(m_CurrentTeam.balltype);
		if (ball != null)
		{
			MainForm.CM.JumpTo(ball);
		}
	}

	private void comboRosterNumber_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_ChangeNumberFlag)
		{
			m_ChangeNumberFlag = false;
			string text = (string)comboRosterNumber.SelectedItem;
			int jerseynumber = Convert.ToInt32(text);
			m_CurrentTeamPlayer.jerseynumber = jerseynumber;
			listViewTeamPlayers.SelectedItems[0].SubItems[5].Text = FifaUtil.PadBlanks(text.ToString(), 2);
			m_ChangeNumberFlag = true;
		}
	}

	private void numericRosterYear_ValueChanged(object sender, EventArgs e)
	{
		if (m_CurrentTeamPlayer != null)
		{
			m_CurrentTeamPlayer.Player.contractvaliduntil = (int)numericRosterYear.Value;
			listViewTeamPlayers.SelectedItems[0].SubItems[2].Text = m_CurrentTeamPlayer.Player.contractvaliduntil.ToString();
		}
	}

	private void comboTeamCountry_SelectedIndexChanged(object sender, EventArgs e)
	{
	}

	private void dateJoiningDate_ValueChanged(object sender, EventArgs e)
	{
		if (m_CurrentTeamPlayer != null)
		{
			m_CurrentTeamPlayer.Player.joindate = dateJoiningDate.Value;
		}
	}

	private void InitVisualFormation(Roster roster)
	{
		Formation currentFormation = m_CurrentFormation;
		if (currentFormation != null)
		{
			labelPos0.Visible = currentFormation.IsRoleUsed(ERole.Goalkeeper);
			labelPos1.Visible = currentFormation.IsRoleUsed(ERole.Sweeper);
			labelPos2.Visible = currentFormation.IsRoleUsed(ERole.Right_Wing_Back);
			labelPos3.Visible = currentFormation.IsRoleUsed(ERole.Right_Back);
			labelPos4.Visible = currentFormation.IsRoleUsed(ERole.Right_Central_Back);
			labelPos5.Visible = currentFormation.IsRoleUsed(ERole.Central_Back);
			labelPos6.Visible = currentFormation.IsRoleUsed(ERole.Left_Central_Back);
			labelPos7.Visible = currentFormation.IsRoleUsed(ERole.Left_Back);
			labelPos8.Visible = currentFormation.IsRoleUsed(ERole.Left_Wing_Back);
			labelPos9.Visible = currentFormation.IsRoleUsed(ERole.Right_Defensive_Midfielder);
			labelPos10.Visible = currentFormation.IsRoleUsed(ERole.Central_Defensive_Midfielder);
			labelPos11.Visible = currentFormation.IsRoleUsed(ERole.Left_Defensive_Midfielder);
			labelPos12.Visible = currentFormation.IsRoleUsed(ERole.Right_Midfielder);
			labelPos13.Visible = currentFormation.IsRoleUsed(ERole.Right_Central_Midfielder);
			labelPos14.Visible = currentFormation.IsRoleUsed(ERole.Central_Midfielder);
			labelPos15.Visible = currentFormation.IsRoleUsed(ERole.Left_Central_Midfielder);
			labelPos16.Visible = currentFormation.IsRoleUsed(ERole.Left_Midfielder);
			labelPos17.Visible = currentFormation.IsRoleUsed(ERole.Right_Advanced_Midfielder);
			labelPos18.Visible = currentFormation.IsRoleUsed(ERole.Central_Advanced_Midfielder);
			labelPos19.Visible = currentFormation.IsRoleUsed(ERole.Left_Advanced_Midfielder);
			labelPos20.Visible = currentFormation.IsRoleUsed(ERole.Right_Forward);
			labelPos21.Visible = currentFormation.IsRoleUsed(ERole.Central_Forward);
			labelPos22.Visible = currentFormation.IsRoleUsed(ERole.Left_Forward);
			labelPos23.Visible = currentFormation.IsRoleUsed(ERole.Right_Wing);
			labelPos24.Visible = currentFormation.IsRoleUsed(ERole.Right_Striker);
			labelPos25.Visible = currentFormation.IsRoleUsed(ERole.Central_Striker);
			labelPos26.Visible = currentFormation.IsRoleUsed(ERole.Left_Striker);
			labelPos27.Visible = currentFormation.IsRoleUsed(ERole.Left_Wing);
			labelPos32A.Visible = true;
			labelPos32B.Visible = true;
			labelPos32C.Visible = true;
			labelPos32D.Visible = true;
			labelPos32E.Visible = true;
			labelPos32F.Visible = true;
			labelPos32G.Visible = true;
			labelPos33A.Visible = true;
			labelPos33B.Visible = true;
			labelPos33C.Visible = true;
			labelPos33D.Visible = true;
			labelPos33E.Visible = true;
			labelPos33F.Visible = true;
			labelPos33G.Visible = true;
			labelPos33H.Visible = true;
			labelPos33I.Visible = true;
			labelPos33J.Visible = true;
			labelPos33K.Visible = true;
			labelPos33L.Visible = true;
			labelPos33M.Visible = true;
			labelPos33N.Visible = true;
			labelPos33O.Visible = true;
			labelPos33P.Visible = true;
			labelPos33Q.Visible = true;
			labelPos33R.Visible = true;
			labelPos33S.Visible = true;
			labelPos33T.Visible = true;
			labelPos33U.Visible = true;
		}
		else
		{
			labelPos0.Visible = false;
			labelPos1.Visible = false;
			labelPos2.Visible = false;
			labelPos3.Visible = false;
			labelPos4.Visible = false;
			labelPos5.Visible = false;
			labelPos6.Visible = false;
			labelPos7.Visible = false;
			labelPos8.Visible = false;
			labelPos9.Visible = false;
			labelPos10.Visible = false;
			labelPos11.Visible = false;
			labelPos12.Visible = false;
			labelPos13.Visible = false;
			labelPos14.Visible = false;
			labelPos15.Visible = false;
			labelPos16.Visible = false;
			labelPos17.Visible = false;
			labelPos18.Visible = false;
			labelPos19.Visible = false;
			labelPos20.Visible = false;
			labelPos21.Visible = false;
			labelPos22.Visible = false;
			labelPos23.Visible = false;
			labelPos24.Visible = false;
			labelPos25.Visible = false;
			labelPos26.Visible = false;
			labelPos27.Visible = false;
			labelPos32A.Visible = false;
			labelPos32B.Visible = false;
			labelPos32C.Visible = false;
			labelPos32D.Visible = false;
			labelPos32E.Visible = false;
			labelPos32F.Visible = false;
			labelPos32G.Visible = false;
			labelPos33A.Visible = false;
			labelPos33B.Visible = false;
			labelPos33C.Visible = false;
			labelPos33D.Visible = false;
			labelPos33E.Visible = false;
			labelPos33F.Visible = false;
			labelPos33G.Visible = false;
			labelPos33H.Visible = false;
			labelPos33I.Visible = false;
			labelPos33J.Visible = false;
			labelPos33K.Visible = false;
			labelPos33L.Visible = false;
			labelPos33M.Visible = false;
			labelPos33N.Visible = false;
			labelPos33O.Visible = false;
			labelPos33P.Visible = false;
			labelPos33Q.Visible = false;
			labelPos33R.Visible = false;
			labelPos33S.Visible = false;
			labelPos33T.Visible = false;
			labelPos33U.Visible = false;
		}
		Label label = labelPos0;
		Label label2 = labelPos1;
		Label label3 = labelPos2;
		Label label4 = labelPos3;
		Label label5 = labelPos4;
		Label label6 = labelPos5;
		Label label7 = labelPos6;
		Label label8 = labelPos7;
		Label label9 = labelPos8;
		Label label10 = labelPos9;
		Label label11 = labelPos10;
		Label label12 = labelPos11;
		Label label13 = labelPos12;
		Label label14 = labelPos13;
		Label label15 = labelPos14;
		Label label16 = labelPos15;
		Label label17 = labelPos16;
		Label label18 = labelPos17;
		Label label19 = labelPos18;
		Label label20 = labelPos19;
		Label label21 = labelPos20;
		Label label22 = labelPos21;
		Label label23 = labelPos22;
		Label label24 = labelPos23;
		Label label25 = labelPos24;
		Label label26 = labelPos25;
		Label label27 = labelPos26;
		Label label28 = labelPos27;
		Label label29 = labelPos32A;
		Label label30 = labelPos32B;
		Label label31 = labelPos32C;
		Label label32 = labelPos32D;
		Label label33 = labelPos32E;
		Label label34 = labelPos32F;
		Label label35 = labelPos32G;
		Label label36 = labelPos33A;
		Label label37 = labelPos33B;
		Label label38 = labelPos33C;
		Label label39 = labelPos33D;
		Label label40 = labelPos33E;
		Label label41 = labelPos33F;
		Label label42 = labelPos33G;
		Label label43 = labelPos33H;
		Label label44 = labelPos33I;
		Label label45 = labelPos33J;
		Label label46 = labelPos33K;
		Label label47 = labelPos33L;
		Label label48 = labelPos33M;
		Label label49 = labelPos33N;
		Label label50 = labelPos33O;
		Label label51 = labelPos33P;
		Label label52 = labelPos33Q;
		Label label53 = labelPos33R;
		Label label54 = labelPos33S;
		Label label55 = labelPos33T;
		string text = (labelPos33U.Text = "______");
		string text3 = (label55.Text = text);
		string text5 = (label54.Text = text3);
		string text7 = (label53.Text = text5);
		string text9 = (label52.Text = text7);
		string text11 = (label51.Text = text9);
		string text13 = (label50.Text = text11);
		string text15 = (label49.Text = text13);
		string text17 = (label48.Text = text15);
		string text19 = (label47.Text = text17);
		string text21 = (label46.Text = text19);
		string text23 = (label45.Text = text21);
		string text25 = (label44.Text = text23);
		string text27 = (label43.Text = text25);
		string text29 = (label42.Text = text27);
		string text31 = (label41.Text = text29);
		string text33 = (label40.Text = text31);
		string text35 = (label39.Text = text33);
		string text37 = (label38.Text = text35);
		string text39 = (label37.Text = text37);
		string text41 = (label36.Text = text39);
		string text43 = (label35.Text = text41);
		string text45 = (label34.Text = text43);
		string text47 = (label33.Text = text45);
		string text49 = (label32.Text = text47);
		string text51 = (label31.Text = text49);
		string text53 = (label30.Text = text51);
		string text55 = (label29.Text = text53);
		string text57 = (label28.Text = text55);
		string text59 = (label27.Text = text57);
		string text61 = (label26.Text = text59);
		string text63 = (label25.Text = text61);
		string text65 = (label24.Text = text63);
		string text67 = (label23.Text = text65);
		string text69 = (label22.Text = text67);
		string text71 = (label21.Text = text69);
		string text73 = (label20.Text = text71);
		string text75 = (label19.Text = text73);
		string text77 = (label18.Text = text75);
		string text79 = (label17.Text = text77);
		string text81 = (label16.Text = text79);
		string text83 = (label15.Text = text81);
		string text85 = (label14.Text = text83);
		string text87 = (label13.Text = text85);
		string text89 = (label12.Text = text87);
		string text91 = (label11.Text = text89);
		string text93 = (label10.Text = text91);
		string text95 = (label9.Text = text93);
		string text97 = (label8.Text = text95);
		string text99 = (label7.Text = text97);
		string text101 = (label6.Text = text99);
		string text103 = (label5.Text = text101);
		string text105 = (label4.Text = text103);
		string text107 = (label3.Text = text105);
		string text109 = (label2.Text = text107);
		label.Text = text109;
		labelPos0.Tag = new TeamPlayer(ERole.Goalkeeper);
		labelPos1.Tag = new TeamPlayer(ERole.Sweeper);
		labelPos2.Tag = new TeamPlayer(ERole.Right_Wing_Back);
		labelPos3.Tag = new TeamPlayer(ERole.Right_Back);
		labelPos4.Tag = new TeamPlayer(ERole.Right_Central_Back);
		labelPos5.Tag = new TeamPlayer(ERole.Central_Back);
		labelPos6.Tag = new TeamPlayer(ERole.Left_Central_Back);
		labelPos7.Tag = new TeamPlayer(ERole.Left_Back);
		labelPos8.Tag = new TeamPlayer(ERole.Left_Wing_Back);
		labelPos9.Tag = new TeamPlayer(ERole.Right_Defensive_Midfielder);
		labelPos10.Tag = new TeamPlayer(ERole.Central_Defensive_Midfielder);
		labelPos11.Tag = new TeamPlayer(ERole.Left_Defensive_Midfielder);
		labelPos12.Tag = new TeamPlayer(ERole.Right_Midfielder);
		labelPos13.Tag = new TeamPlayer(ERole.Right_Central_Midfielder);
		labelPos14.Tag = new TeamPlayer(ERole.Central_Midfielder);
		labelPos15.Tag = new TeamPlayer(ERole.Left_Central_Midfielder);
		labelPos16.Tag = new TeamPlayer(ERole.Left_Midfielder);
		labelPos17.Tag = new TeamPlayer(ERole.Right_Advanced_Midfielder);
		labelPos18.Tag = new TeamPlayer(ERole.Central_Advanced_Midfielder);
		labelPos19.Tag = new TeamPlayer(ERole.Left_Advanced_Midfielder);
		labelPos20.Tag = new TeamPlayer(ERole.Right_Forward);
		labelPos21.Tag = new TeamPlayer(ERole.Central_Forward);
		labelPos22.Tag = new TeamPlayer(ERole.Left_Forward);
		labelPos23.Tag = new TeamPlayer(ERole.Right_Wing);
		labelPos24.Tag = new TeamPlayer(ERole.Right_Striker);
		labelPos25.Tag = new TeamPlayer(ERole.Central_Striker);
		labelPos26.Tag = new TeamPlayer(ERole.Left_Striker);
		labelPos27.Tag = new TeamPlayer(ERole.Left_Wing);
		labelPos32A.Tag = new TeamPlayer(ERole.Substitute);
		labelPos32B.Tag = new TeamPlayer(ERole.Substitute);
		labelPos32C.Tag = new TeamPlayer(ERole.Substitute);
		labelPos32D.Tag = new TeamPlayer(ERole.Substitute);
		labelPos32E.Tag = new TeamPlayer(ERole.Substitute);
		labelPos32F.Tag = new TeamPlayer(ERole.Substitute);
		labelPos32G.Tag = new TeamPlayer(ERole.Substitute);
		labelPos33A.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33B.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33C.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33D.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33E.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33F.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33G.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33H.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33I.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33J.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33K.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33L.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33M.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33N.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33O.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33P.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33Q.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33R.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33S.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33T.Tag = new TeamPlayer(ERole.Tribune);
		labelPos33U.Tag = new TeamPlayer(ERole.Tribune);
		Label[] substituteLabels =
		{
			labelPos32A, labelPos32B, labelPos32C, labelPos32D,
			labelPos32E, labelPos32F, labelPos32G
		};
		Label[] reserveLabels =
		{
			labelPos33A, labelPos33B, labelPos33C, labelPos33D, labelPos33E,
			labelPos33F, labelPos33G, labelPos33H, labelPos33I, labelPos33J,
			labelPos33K, labelPos33L, labelPos33M, labelPos33N, labelPos33O,
			labelPos33P, labelPos33Q, labelPos33R, labelPos33S, labelPos33T,
			labelPos33U
		};
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < roster.Count; i++)
		{
			TeamPlayer teamPlayer = (TeamPlayer)roster[i];
			switch (teamPlayer.position)
			{
			case 0:
				labelPos0.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos0.Visible = true;
				labelPos0.Tag = teamPlayer;
				break;
			case 1:
				labelPos1.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos1.Visible = true;
				labelPos1.Tag = teamPlayer;
				break;
			case 2:
				labelPos2.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos2.Visible = true;
				labelPos2.Tag = teamPlayer;
				break;
			case 3:
				labelPos3.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos3.Visible = true;
				labelPos3.Tag = teamPlayer;
				break;
			case 4:
				labelPos4.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos4.Visible = true;
				labelPos4.Tag = teamPlayer;
				break;
			case 5:
				labelPos5.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos5.Visible = true;
				labelPos5.Tag = teamPlayer;
				break;
			case 6:
				labelPos6.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos6.Visible = true;
				labelPos6.Tag = teamPlayer;
				break;
			case 7:
				labelPos7.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos7.Visible = true;
				labelPos7.Tag = teamPlayer;
				break;
			case 8:
				labelPos8.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos8.Visible = true;
				labelPos8.Tag = teamPlayer;
				break;
			case 9:
				labelPos9.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos9.Visible = true;
				labelPos9.Tag = teamPlayer;
				break;
			case 10:
				labelPos10.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos10.Visible = true;
				labelPos10.Tag = teamPlayer;
				break;
			case 11:
				labelPos11.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos11.Visible = true;
				labelPos11.Tag = teamPlayer;
				break;
			case 12:
				labelPos12.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos12.Visible = true;
				labelPos12.Tag = teamPlayer;
				break;
			case 13:
				labelPos13.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos13.Visible = true;
				labelPos13.Tag = teamPlayer;
				break;
			case 14:
				labelPos14.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos14.Visible = true;
				labelPos14.Tag = teamPlayer;
				break;
			case 15:
				labelPos15.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos15.Visible = true;
				labelPos15.Tag = teamPlayer;
				break;
			case 16:
				labelPos16.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos16.Visible = true;
				labelPos16.Tag = teamPlayer;
				break;
			case 17:
				labelPos17.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos17.Visible = true;
				labelPos17.Tag = teamPlayer;
				break;
			case 18:
				labelPos18.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos18.Visible = true;
				labelPos18.Tag = teamPlayer;
				break;
			case 19:
				labelPos19.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos19.Visible = true;
				labelPos19.Tag = teamPlayer;
				break;
			case 20:
				labelPos20.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos20.Visible = true;
				labelPos20.Tag = teamPlayer;
				break;
			case 21:
				labelPos21.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos21.Visible = true;
				labelPos21.Tag = teamPlayer;
				break;
			case 22:
				labelPos22.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos22.Visible = true;
				labelPos22.Tag = teamPlayer;
				break;
			case 23:
				labelPos23.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos23.Visible = true;
				labelPos23.Tag = teamPlayer;
				break;
			case 24:
				labelPos24.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos24.Visible = true;
				labelPos24.Tag = teamPlayer;
				break;
			case 25:
				labelPos25.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos25.Visible = true;
				labelPos25.Tag = teamPlayer;
				break;
			case 26:
				labelPos26.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos26.Visible = true;
				labelPos26.Tag = teamPlayer;
				break;
			case 27:
				labelPos27.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
				labelPos27.Visible = true;
				labelPos27.Tag = teamPlayer;
				break;
			case 28:
				if (num < substituteLabels.Length)
				{
					SetRosterPlayerLabel(substituteLabels[num++], teamPlayer);
				}
				else if (num2 < reserveLabels.Length)
				{
					// FC26 squads can contain more substitutes than the legacy
					// seven-slot UI. Display the overflow as reserves without
					// mutating the player's database position just by viewing it.
					SetRosterPlayerLabel(reserveLabels[num2++], teamPlayer);
				}
				break;
			case 29:
				if (num2 < reserveLabels.Length)
				{
					SetRosterPlayerLabel(reserveLabels[num2++], teamPlayer);
				}
				break;
			}
		}
		InitSpecialPlayers(m_CurrentTeam);
		LayoutFc26RosterFormationUi();
		InvalidateFc26RosterLabels();
	}

	private static void SetRosterPlayerLabel(Label label, TeamPlayer teamPlayer)
	{
		label.Text = teamPlayer.jerseynumber + "\n" + teamPlayer.Player.Name;
		label.Visible = true;
		label.Tag = teamPlayer;
	}

	private void InitSpecialPlayers(Team team)
	{
		if (team == null)
		{
			return;
		}
		if (team.PlayerCaptain != null)
		{
			labelCaptain.Text = team.PlayerCaptain.Name;
		}
		else
		{
			labelCaptain.Text = "______";
		}
		if (team.PlayerPenalty != null)
		{
			labelPenalty.Text = team.PlayerPenalty.Name;
		}
		else
		{
			labelPenalty.Text = "______";
		}
		if (team.PlayerFreeKick != null)
		{
			labelFreeKick.Text = team.PlayerFreeKick.Name;
		}
		else
		{
			labelFreeKick.Text = "______";
		}
		if (FifaEnvironment.Year > 14)
		{
			if (team.PlayerRightFreeKick != null)
			{
				labelRightFreeKick.Text = team.PlayerRightFreeKick.Name;
			}
			else
			{
				labelRightFreeKick.Text = "______";
			}
			if (team.PlayerLeftFreeKick != null)
			{
				labelLeftFreeKick.Text = team.PlayerLeftFreeKick.Name;
			}
			else
			{
				labelLeftFreeKick.Text = "______";
			}
		}
		if (team.PlayerLongKick != null)
		{
			labelLongKick.Text = team.PlayerLongKick.Name;
		}
		else
		{
			labelLongKick.Text = "______";
		}
		if (team.PlayerLeftCorner != null)
		{
			labelLeftCorner.Text = team.PlayerLeftCorner.Name;
		}
		else
		{
			labelLeftCorner.Text = "______";
		}
		if (team.PlayerRightCorner != null)
		{
			labelRightCorner.Text = team.PlayerRightCorner.Name;
		}
		else
		{
			labelRightCorner.Text = "______";
		}
	}

	private void labelSpecial_DragEnter(object sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.Copy;
	}

	private void labelSpecial_DragDrop(object sender, DragEventArgs e)
	{
		Label label = (Label)sender;
		TeamPlayer teamPlayer = (TeamPlayer)m_DraggedLabel.Tag;
		label.Text = teamPlayer.Player.Name;
		if (label == labelCaptain)
		{
			m_CurrentTeam.PlayerCaptain = teamPlayer.Player;
			m_CurrentTeam.captainid = m_CurrentTeam.PlayerCaptain.Id;
		}
		else if (label == labelPenalty)
		{
			m_CurrentTeam.PlayerPenalty = teamPlayer.Player;
			m_CurrentTeam.penaltytakerid = m_CurrentTeam.PlayerPenalty.Id;
		}
		else if (label == labelFreeKick)
		{
			m_CurrentTeam.PlayerFreeKick = teamPlayer.Player;
			m_CurrentTeam.freekicktakerid = m_CurrentTeam.PlayerFreeKick.Id;
		}
		else if (label == labelLeftFreeKick)
		{
			m_CurrentTeam.PlayerLeftFreeKick = teamPlayer.Player;
			m_CurrentTeam.leftfreekicktakerid = m_CurrentTeam.PlayerLeftFreeKick.Id;
		}
		else if (label == labelRightFreeKick)
		{
			m_CurrentTeam.PlayerRightFreeKick = teamPlayer.Player;
			m_CurrentTeam.rightfreekicktakerid = m_CurrentTeam.PlayerRightFreeKick.Id;
		}
		else if (label == labelLongKick)
		{
			m_CurrentTeam.PlayerLongKick = teamPlayer.Player;
			m_CurrentTeam.longkicktakerid = m_CurrentTeam.PlayerLongKick.Id;
		}
		else if (label == labelLeftCorner)
		{
			m_CurrentTeam.PlayerLeftCorner = teamPlayer.Player;
			m_CurrentTeam.leftcornerkicktakerid = m_CurrentTeam.PlayerLeftCorner.Id;
		}
		else if (label == labelRightCorner)
		{
			m_CurrentTeam.PlayerRightCorner = teamPlayer.Player;
			m_CurrentTeam.rightcornerkicktakerid = m_CurrentTeam.PlayerRightCorner.Id;
		}
	}

	private void labelPos_MouseDown(object sender, MouseEventArgs e)
	{
		Label label = (Label)sender;
		if (label.Text == "______")
		{
			return;
		}
		m_DraggedLabel = label;
		if (listViewTeamPlayers.SelectedItems.Count > 0)
		{
			listViewTeamPlayers.SelectedItems[0].Selected = false;
		}
		TeamPlayer teamPlayer = (TeamPlayer)m_DraggedLabel.Tag;
		for (int i = 0; i < listViewTeamPlayers.Items.Count; i++)
		{
			ListViewItem listViewItem = listViewTeamPlayers.Items[i];
			if (listViewItem.Tag == teamPlayer)
			{
				listViewItem.Selected = true;
				break;
			}
		}
		m_DraggedLabel.DoDragDrop(m_DraggedLabel.Text, DragDropEffects.Copy | DragDropEffects.Move);
	}

	private void labelPos_DragDrop(object sender, DragEventArgs e)
	{
		Label label = (Label)sender;
		TeamPlayer obj = (TeamPlayer)m_DraggedLabel.Tag;
		string text = m_DraggedLabel.Text;
		m_DraggedLabel.Text = label.Text;
		label.Text = text;
		TeamPlayer teamPlayer = (TeamPlayer)label.Tag;
		int position = obj.position;
		obj.position = teamPlayer.position;
		teamPlayer.position = position;
		TeamPlayer tag = obj;
		m_DraggedLabel.Tag = teamPlayer;
		label.Tag = tag;
		m_DraggedLabel.Invalidate();
		label.Invalidate();
	}

	private void labelPos_DragEnter(object sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.Move;
	}

	private void listViewRoster_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		ListView obj = (ListView)sender;
		SortOrder sortOrder = SortOrder.None;
		obj.ListViewItemSorter = new ListViewItemComparer(sortOrder: obj.Sorting = ((obj.Sorting != SortOrder.Ascending) ? SortOrder.Ascending : SortOrder.Descending), column: e.Column);
	}

	private void radioUseSpecificFormation_CheckedChanged(object sender, EventArgs e)
	{
		if (m_LockUserChanges || !radioUseSpecificFormation.Checked)
		{
			return;
		}
		int newId = FifaEnvironment.Formations.GetNewId();
		if (newId < 0)
		{
			FifaEnvironment.UserMessages.ShowMessage(5043);
			radioUseGenericFormation.Checked = true;
			return;
		}
		Formation formation = null;
		formation = ((m_BackupSpecificFormation == null) ? m_CurrentFormation : m_BackupSpecificFormation);
		if (formation != null)
		{
			Formation formation2 = (Formation)formation.Clone(newId);
			FifaEnvironment.Formations.InsertId(formation2);
			m_CurrentTeam.Formation = formation2;
			m_CurrentFormation = formation2;
			m_CurrentTeam.formationid = formation2.Id;
			m_CurrentFormation.Team = m_CurrentTeam;
			if (m_BackupSpecificFormation != null)
			{
				m_CurrentTeam.AssignTitolarToRoles(formation2);
			}
			InitVisualFormation(m_CurrentTeam.Roster);
			labelTeamFormationName.Text = m_CurrentFormation.ToString();
		}
	}

	private void radioUseGenericFormation_CheckedChanged(object sender, EventArgs e)
	{
		if (m_LockUserChanges)
		{
			return;
		}
		if (radioUseGenericFormation.Checked)
		{
			if (m_CurrentFormation != null && !m_CurrentFormation.IsGeneric())
			{
				m_BackupSpecificFormation = m_CurrentFormation;
				FifaEnvironment.Formations.RemoveId(m_CurrentFormation);
			}
			if (comboGenericFormations.SelectedIndex < 0)
			{
				comboGenericFormations.SelectedIndex = 0;
			}
			Formation formation = (Formation)comboGenericFormations.SelectedItem;
			m_CurrentTeam.Formation = formation;
			m_CurrentFormation = formation;
			m_CurrentTeam.formationid = formation.Id;
			m_CurrentTeam.AssignTitolarToRoles(formation);
			InitVisualFormation(m_CurrentTeam.Roster);
			labelTeamFormationName.Text = m_CurrentFormation.ToString();
		}
		comboGenericFormations.Visible = radioUseGenericFormation.Checked;
	}

	private void comboGenericFormations_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges && comboGenericFormations.SelectedIndex >= 0)
		{
			Formation formation = (Formation)comboGenericFormations.SelectedItem;
			if (FifaEnvironment.Year == 26 && formation != null && m_CurrentFormation != null &&
				!m_CurrentFormation.IsGeneric())
			{
				// Keep the team's existing formations row/ID, but copy the complete
				// database-native layout so the change remains a team-specific edit.
				m_CurrentFormation.ReInitialize(formation);
				m_CurrentFormation.relativeformationid = formation.Id;
				m_CurrentFormation.Team = m_CurrentTeam;
				m_CurrentTeam.LinkFormation(m_CurrentFormation);
				m_CurrentTeam.formationid = m_CurrentFormation.Id;
				m_CurrentTeam.AssignTitolarToRoles(m_CurrentFormation);
				InitVisualFormation(m_CurrentTeam.Roster);
				labelTeamFormationName.Text = m_CurrentFormation.ToString();
				return;
			}
			if (formation != null && formation != m_CurrentTeam.Formation)
			{
				m_CurrentTeam.Formation = formation;
				m_CurrentFormation = formation;
				m_CurrentTeam.formationid = formation.Id;
				m_CurrentTeam.AssignTitolarToRoles(formation);
				InitVisualFormation(m_CurrentTeam.Roster);
				labelTeamFormationName.Text = m_CurrentFormation.ToString();
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

	private EPlayingDirection ClickToAttackRole(EventArgs e)
	{
		int num = ((MouseEventArgs)e).X;
		int num2 = ((MouseEventArgs)e).Y;
		if (num < 16)
		{
			if (num2 < 16)
			{
				return EPlayingDirection.Right;
			}
			if (num2 < 32)
			{
				return EPlayingDirection.Right;
			}
			return EPlayingDirection.DiagonalRight;
		}
		if (num < 32)
		{
			if (num2 < 16)
			{
				return EPlayingDirection.Standing;
			}
			if (num2 < 32)
			{
				return EPlayingDirection.Standing;
			}
			return EPlayingDirection.Stright;
		}
		if (num2 < 16)
		{
			return EPlayingDirection.Left;
		}
		if (num2 < 32)
		{
			return EPlayingDirection.Left;
		}
		return EPlayingDirection.DiagonalLeft;
	}

	private EPlayingDirection ClickToDefenseRole(EventArgs e)
	{
		int num = ((MouseEventArgs)e).X;
		int num2 = ((MouseEventArgs)e).Y;
		if (num < 16)
		{
			if (num2 < 16)
			{
				return EPlayingDirection.DiagonalRight;
			}
			if (num2 < 32)
			{
				return EPlayingDirection.Right;
			}
			return EPlayingDirection.Right;
		}
		if (num < 32)
		{
			if (num2 < 16)
			{
				return EPlayingDirection.Stright;
			}
			if (num2 < 32)
			{
				return EPlayingDirection.Standing;
			}
			return EPlayingDirection.Standing;
		}
		if (num2 < 16)
		{
			return EPlayingDirection.DiagonalLeft;
		}
		if (num2 < 32)
		{
			return EPlayingDirection.Left;
		}
		return EPlayingDirection.Left;
	}

	private void buttonReplicateLogo_Click(object sender, EventArgs e)
	{
		Bitmap currentBitmap = viewer2DCrestLarge.CurrentBitmap;
		m_CurrentTeam.SetAllCrests(currentBitmap);
		viewer2DCrest16.CurrentBitmap = m_CurrentTeam.GetCrest16();
		viewer2DCrest32.CurrentBitmap = m_CurrentTeam.GetCrest32();
		viewer2DCrest50.CurrentBitmap = m_CurrentTeam.GetCrest50();
	}

	private void textStadiumName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked && textStadiumName.Text.Length > 30)
		{
			textStadiumName.Text = textStadiumName.Text.Substring(0, 30);
		}
	}

	private void comboTeamLeague_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboTeamLeague.SelectedItem == null)
		{
			comboTeamLeague.Text = string.Empty;
		}
	}

	private void comboPrevLeague_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboPrevLeague.SelectedItem == null)
		{
			comboPrevLeague.Text = string.Empty;
		}
	}

	private void textShortTeamName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked && textShortTeamName.Text.Length > 10)
		{
			textShortTeamName.Text = textShortTeamName.Text.Substring(0, 10);
		}
	}

	private void textTeamName7_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked && textTeamName7.Text.Length > 7)
		{
			textTeamName7.Text = textTeamName7.Text.Substring(0, 7);
		}
	}

	private void textStandardTeamName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked && textStandardTeamName.Text.Length > 15)
		{
			textStandardTeamName.Text = textStandardTeamName.Text.Substring(0, 15);
		}
	}

	private void comboRivalTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentTeam != null)
		{
			Team team = (Team)comboRivalTeam.SelectedItem;
			if (team != m_CurrentTeam.RivalTeam)
			{
				m_CurrentTeam.RivalTeam = team;
			}
		}
	}

	private void comboObjective_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentTeam != null && comboObjective.SelectedIndex >= 0 && comboObjective.SelectedIndex != m_CurrentTeam.objective)
		{
			m_CurrentTeam.objective = comboObjective.SelectedIndex;
		}
	}

	private void comboMaxOnjective_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentTeam != null && comboMaxOnjective.SelectedIndex >= 0 && comboMaxOnjective.SelectedIndex != m_CurrentTeam.highestpossible)
		{
			m_CurrentTeam.highestpossible = comboMaxOnjective.SelectedIndex;
		}
	}

	private void comboProbObjective_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentTeam != null && comboProbObjective.SelectedIndex >= 0 && comboProbObjective.SelectedIndex != m_CurrentTeam.highestprobable)
		{
			m_CurrentTeam.highestprobable = comboProbObjective.SelectedIndex;
		}
	}

	private void buttonTeamPlayerPlus_Click(object sender, EventArgs e)
	{
		int num = listViewTeamPlayers.SelectedIndices[0];
		foreach (TeamPlayer item in m_CurrentTeam.Roster)
		{
			item.Player.ChangeSkills(1);
		}
		LoadRosterPage();
		if (num >= 0)
		{
			listViewTeamPlayers.Items[num].Selected = true;
		}
	}

	private void buttonTeamPlayerMinus_Click(object sender, EventArgs e)
	{
		int num = listViewTeamPlayers.SelectedIndices[0];
		foreach (TeamPlayer item in m_CurrentTeam.Roster)
		{
			item.Player.ChangeSkills(-1);
		}
		LoadRosterPage();
		if (num >= 0)
		{
			listViewTeamPlayers.Items[num].Selected = true;
		}
	}

	private void buttonPlusContract_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam == null || m_CurrentTeam.Roster == null)
		{
			return;
		}
		int num = listViewTeamPlayers.SelectedIndices[0];
		foreach (TeamPlayer item in m_CurrentTeam.Roster)
		{
			item.Player.contractvaliduntil++;
		}
		LoadRosterPage();
		if (num >= 0)
		{
			listViewTeamPlayers.Items[num].Selected = true;
		}
	}

	private void buttonMinusContract_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam == null || m_CurrentTeam.Roster == null)
		{
			return;
		}
		int num = listViewTeamPlayers.SelectedIndices[0];
		foreach (TeamPlayer item in m_CurrentTeam.Roster)
		{
			item.Player.contractvaliduntil--;
		}
		LoadRosterPage();
		if (num >= 0)
		{
			listViewTeamPlayers.Items[num].Selected = true;
		}
	}

	private void labelFlag1_Click(object sender, EventArgs e)
	{
		Label label = (Label)sender;
		MouseEventArgs e2 = (MouseEventArgs)e;
		if (e2.Button == MouseButtons.Left)
		{
			if (label.ImageIndex == label.ImageList.Images.Count - 1)
			{
				label.ImageIndex = 0;
			}
			else
			{
				label.ImageIndex++;
			}
		}
		else if (e2.Button == MouseButtons.Right)
		{
			if (label.ImageIndex == 0)
			{
				label.ImageIndex = label.ImageList.Images.Count - 1;
			}
			else
			{
				label.ImageIndex--;
			}
		}
	}

	private void pictureFlagRed_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureFlagRed.BackColor;
		colorDialog.ShowDialog();
		pictureFlagRed.BackColor = colorDialog.Color;
		m_CurrentTeam.TeamColor1 = colorDialog.Color;
	}

	private void pictureFlagGreen_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureFlagGreen.BackColor;
		colorDialog.ShowDialog();
		pictureFlagGreen.BackColor = colorDialog.Color;
		m_CurrentTeam.TeamColor2 = colorDialog.Color;
	}

	private void pictureFlagBlue_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureFlagBlue.BackColor;
		colorDialog.ShowDialog();
		pictureFlagBlue.BackColor = colorDialog.Color;
		m_CurrentTeam.TeamColor3 = colorDialog.Color;
	}

	private void buttonCreateFlags_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam != null)
		{
			Bitmap[] array = new Bitmap[4];
			Bitmap crest = m_CurrentTeam.GetCrest();
			new Rectangle(0, 0, 256, 256);
			Rectangle destRectangle = new Rectangle(160, 32, 192, 192);
			int style = labelFlag1.ImageIndex + 1;
			string filename = FifaEnvironment.LaunchDir + "\\Templates\\" + Team.GenericFlagFileName(style);
			array[0] = new Bitmap(filename);
			GraphicUtil.ColorizeRGB(array[0], pictureFlagRed.BackColor, pictureFlagGreen.BackColor, pictureFlagBlue.BackColor, preserveArmBand: false);
			if (checkFlag1.Checked)
			{
				array[0] = GraphicUtil.Overlap(array[0], crest, destRectangle);
			}
			style = labelFlag2.ImageIndex + 1;
			filename = FifaEnvironment.LaunchDir + "\\Templates\\" + Team.GenericFlagFileName(style);
			array[1] = new Bitmap(filename);
			GraphicUtil.ColorizeRGB(array[1], pictureFlagRed.BackColor, pictureFlagGreen.BackColor, pictureFlagBlue.BackColor, preserveArmBand: false);
			if (checkFlag2.Checked)
			{
				array[1] = GraphicUtil.Overlap(array[1], crest, destRectangle);
			}
			destRectangle = new Rectangle(32, 32, 192, 192);
			style = labelFlag3.ImageIndex + 1;
			filename = FifaEnvironment.LaunchDir + "\\Templates\\" + Team.GenericFlagFileName(style);
			array[2] = new Bitmap(filename);
			GraphicUtil.ColorizeRGB(array[2], pictureFlagRed.BackColor, pictureFlagGreen.BackColor, pictureFlagBlue.BackColor, preserveArmBand: false);
			if (checkFlag3.Checked)
			{
				array[2] = GraphicUtil.Overlap(array[2], crest, destRectangle);
			}
			style = labelFlag4.ImageIndex + 1;
			filename = FifaEnvironment.LaunchDir + "\\Templates\\" + Team.GenericFlagFileName(style);
			array[3] = new Bitmap(filename);
			GraphicUtil.ColorizeRGB(array[3], pictureFlagRed.BackColor, pictureFlagGreen.BackColor, pictureFlagBlue.BackColor, preserveArmBand: false);
			if (checkFlag4.Checked)
			{
				array[3] = GraphicUtil.Overlap(array[3], crest, destRectangle);
			}
			m_CurrentTeam.SetFlags(array);
			multiViewer2DFlags15.Bitmaps = m_CurrentTeam.GetFlags();
		}
	}

	private void comboDEFLine_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_LockUserChanges || FifaEnvironment.Year != 26 || m_CurrentTeam == null ||
			comboDEFLine.SelectedIndex < 0) return;
		int[] presets = { 30, 50, 65, 90 };
		m_CurrentTeam.defensivedepth = presets[comboDEFLine.SelectedIndex];
		bool oldLock = m_LockUserChanges;
		m_LockUserChanges = true;
		SetNumericValue(numericDefmentality, m_CurrentTeam.defensivedepth);
		m_LockUserChanges = oldLock;
	}

	private void comboBUSPositioning_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_LockUserChanges || FifaEnvironment.Year != 26 || m_CurrentTeam == null ||
			comboBUSPositioning.SelectedIndex < 0) return;
		m_CurrentTeam.buildupplay = comboBUSPositioning.SelectedIndex + 1;
	}

	private void comboCCPositioning_SelectedIndexChanged(object sender, EventArgs e)
	{
		_ = m_LockUserChanges;
	}

	private void checkHasSpecificAdboard_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			if (checkHasSpecificAdboard.Checked)
			{
				m_CurrentTeam.CreateSpecificAdboard();
			}
			else
			{
				m_CurrentTeam.DeleteSpecificAdboard();
			}
		}
	}

	private void labelTeamFormationName_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentTeam.Formation != null)
		{
			MainForm.CM.JumpTo(m_CurrentTeam.Formation);
		}
	}

	private void labelStandardTeamName_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentTeam.TeamNameFull.Length > 15)
		{
			m_CurrentTeam.TeamNameAbbr15 = m_CurrentTeam.TeamNameFull.Substring(0, 15);
		}
		else
		{
			m_CurrentTeam.TeamNameAbbr15 = m_CurrentTeam.TeamNameFull;
		}
		textStandardTeamName.Text = m_CurrentTeam.TeamNameAbbr15;
	}

	private void textShortTeamName_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam.TeamNameFull.Length > 10)
		{
			m_CurrentTeam.TeamNameAbbr10 = m_CurrentTeam.TeamNameFull.Substring(0, 10);
		}
		else
		{
			m_CurrentTeam.TeamNameAbbr10 = m_CurrentTeam.TeamNameFull;
		}
		textShortTeamName.Text = m_CurrentTeam.TeamNameAbbr10;
	}

	private void labelTeamName7_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentTeam.TeamNameFull.Length > 7)
		{
			m_CurrentTeam.TeamNameAbbr7 = m_CurrentTeam.TeamNameFull.Substring(0, 7);
		}
		else
		{
			m_CurrentTeam.TeamNameAbbr7 = m_CurrentTeam.TeamNameFull;
		}
		textTeamName7.Text = m_CurrentTeam.TeamNameAbbr7;
	}

	private void label3_Click(object sender, EventArgs e)
	{
	}

	private void numericLatitude_ValueChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			m_CurrentTeam.latitude = (int)numericLatitude.Value;
		}
	}

	private void numericLongitude_ValueChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			m_CurrentTeam.longitude = (int)numericLongitude.Value;
		}
	}

	private void numericUtcOffset_ValueChanged(object sender, EventArgs e)
	{
		if (!m_LockUserChanges)
		{
			m_CurrentTeam.utcoffset = (int)numericUtcOffset.Value;
		}
	}

	private void buttonShow3DManager_Click(object sender, EventArgs e)
	{
		Show3DManager();
	}

	private void buttonImportModel3DTeamManager_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam != null)
		{
			string text = FifaEnvironment.BrowseAndCheckModel(ref m_TeamCurrentFolder, "Open 3D Manger Model file", "3D manager model files (*.rx3)|manager_*.rx3");
			if (text != null)
			{
				Manager.SetRevModManagerModel(m_CurrentTeam.Id, text);
				Show3DManager();
			}
		}
	}

	private void buttonExportModel3DTeamManager_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam != null)
		{
			string text = Manager.RevModManagerModelFileName(m_CurrentTeam.Id);
			if (text != null)
			{
				FifaEnvironment.AskAndExportFromZdata(text, ref m_TeamCurrentFolder);
			}
		}
	}

	private void buttonDeleteModel3DTeamManager_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam != null)
		{
			Manager.DeleteRevModManagerModel(m_CurrentTeam.Id);
			Show3DManager();
		}
	}

	private bool ImportImageManager(object sender, Bitmap bitmap)
	{
		if (m_CurrentTeam == null)
		{
			return false;
		}
		return Manager.SetRevModManagerTexture(m_CurrentTeam.Id, bitmap);
	}

	private bool DeleteManager(object sender)
	{
		if (m_CurrentTeam == null)
		{
			return false;
		}
		return Manager.DeleteRevModManagerTexture(m_CurrentTeam.Id);
	}

	private void buttonShow3DBall_Click(object sender, EventArgs e)
	{
		Show3DRevModBall();
	}

	public void Show3DRevModBall()
	{
		if (!buttonShow3DBall.Checked)
		{
			viewer3DTeamBall.ShowEmpty();
		}
		else if (m_CurrentTeam != null)
		{
			Bitmap[] revModTeamBallTextures = Ball.GetRevModTeamBallTextures(m_CurrentTeam.Id);
			Bitmap bitmap = null;
			if (revModTeamBallTextures != null)
			{
				bitmap = GraphicUtil.EmbossBitmap(revModTeamBallTextures[0], revModTeamBallTextures[1]);
			}
			Rx3File revModTeamBallModel = Ball.GetRevModTeamBallModel(m_CurrentTeam.Id);
			if (bitmap == null || revModTeamBallModel == null)
			{
				viewer3DTeamBall.Clean(1);
				viewer3DTeamBall.Render();
				return;
			}
			Rx3IndexArray.TriangleListType = Rx3IndexArray.ETriangleListType.InvertEven;
			Model3D model3D = new Model3D(revModTeamBallModel.Rx3IndexArrays[0], revModTeamBallModel.Rx3VertexArrays[0], bitmap);
			viewer3DTeamBall.Clean(1);
			viewer3DTeamBall.SetMesh(0, model3D);
			viewer3DTeamBall.Render();
		}
	}

	private void buttonImport3DModelTeamBall_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam != null)
		{
			string text = FifaEnvironment.BrowseAndCheckModel(ref m_TeamCurrentFolder, "Open 3D Ball Model file", "3D ball model files (*.rx3)|ball_*.rx3");
			if (text != null)
			{
				Ball.SetRevModTeamBallModel(m_CurrentTeam.Id, text);
				LoadRevModBall();
			}
		}
	}

	private void buttonExport3DModelTeamBall_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam != null)
		{
			string text = Ball.RevModTeamBallModelFileName(m_CurrentTeam.Id);
			if (text != null)
			{
				FifaEnvironment.AskAndExportFromZdata(text, ref m_TeamCurrentFolder);
			}
		}
	}

	private void buttonRemove3DModelTeamBall_Click(object sender, EventArgs e)
	{
		if (m_CurrentTeam != null)
		{
			Ball.DeleteRevModTeamBallModel(m_CurrentTeam.Id);
			LoadRevModBall();
		}
	}

	private void LoadRevModBall()
	{
		if (m_IsLoaded && m_CurrentTeam != null)
		{
			multiViewer2DTeamBallTextures.Bitmaps = Ball.GetRevModTeamBallTextures(m_CurrentTeam.Id);
			Show3DRevModBall();
		}
	}

	private void checkIsNationalTeam_CheckedChanged(object sender, EventArgs e)
	{
		if (m_CurrentTeam == null || m_Locked)
		{
			return;
		}
		if (checkIsNationalTeam.Checked)
		{
			m_CurrentTeam.NationalTeam = true;
			if (m_CurrentTeam.Country != null && !m_CurrentTeam.IsFemale())
			{
				m_CurrentTeam.Country.SetNationalTeam(m_CurrentTeam, m_CurrentTeam.Id);
			}
		}
		else
		{
			m_CurrentTeam.NationalTeam = false;
			if (m_CurrentTeam.Country != null && !m_CurrentTeam.IsFemale())
			{
				m_CurrentTeam.Country.SetNationalTeam(null, 0);
			}
		}
	}

	private void buttonLoanTo_Click(object sender, EventArgs e)
	{
		m_CurrentTeam.RemoveTeamPlayer(m_CurrentTeamPlayer);
		m_CurrentAvailableTeam.AddTeamPlayer(m_CurrentTeamPlayer);
		m_CurrentTeamPlayer.Player.joindate = dateTransferPreset.Value;
		m_CurrentTeamPlayer.Player.TeamLoanedFrom = m_CurrentTeam;
		m_CurrentTeamPlayer.Player.loandateend = m_CurrentTeamPlayer.Player.joindate.AddDays(364.0);
		m_CurrentTeamPlayer.Player.IsLoaned = true;
		InitListViewTeamPlayers(m_CurrentTeam.Roster);
		InitListViewPlayersAvailable(m_CurrentAvailableTeam, null, showFreeAgents: false);
		EnableRosterButtons();
		InitVisualFormation(m_CurrentTeam.Roster);
	}

	private void labelHomeKit_DoubleClick(object sender, EventArgs e)
	{
		Kit kit = FifaEnvironment.Kits.GetKit(m_CurrentTeam.Id, 0);
		if (kit != null)
		{
			MainForm.CM.JumpTo(kit);
		}
		else
		{
			FifaEnvironment.UserMessages.ShowMessage(3001);
		}
	}

	private void labelAwayKit_DoubleClick(object sender, EventArgs e)
	{
		Kit kit = FifaEnvironment.Kits.GetKit(m_CurrentTeam.Id, 1);
		if (kit != null)
		{
			MainForm.CM.JumpTo(kit);
		}
		else
		{
			FifaEnvironment.UserMessages.ShowMessage(3001);
		}
	}

	private void labelKeeprKit_DoubleClick(object sender, EventArgs e)
	{
		Kit kit = FifaEnvironment.Kits.GetKit(m_CurrentTeam.Id, 2);
		if (kit != null)
		{
			MainForm.CM.JumpTo(kit);
		}
		else
		{
			FifaEnvironment.UserMessages.ShowMessage(3001);
		}
	}

	private void labelThirdKit_DoubleClick(object sender, EventArgs e)
	{
		Kit kit = FifaEnvironment.Kits.GetKit(m_CurrentTeam.Id, 3);
		if (kit != null)
		{
			MainForm.CM.JumpTo(kit);
		}
		else
		{
			FifaEnvironment.UserMessages.ShowMessage(3001);
		}
	}

	private void checkIsLoan_CheckedChanged(object sender, EventArgs e)
	{
		labelLoanEnd.Visible = checkIsLoan.Checked;
		labelLoanedFrom.Visible = checkIsLoan.Checked;
		dateLoanEnd.Visible = checkIsLoan.Checked;
		comboTeamLoanedFrom.Visible = checkIsLoan.Checked;
		if (m_Locked)
		{
			return;
		}
		Player player = m_CurrentTeamPlayer.Player;
		if (checkIsLoan.Checked)
		{
			player.IsLoaned = true;
			if (player.TeamLoanedFrom == null)
			{
				if (player.PreviousTeam != null)
				{
					player.TeamLoanedFrom = player.PreviousTeam;
				}
				else
				{
					player.TeamLoanedFrom = (Team)comboTeamLoanedFrom.SelectedItem;
				}
				comboTeamLoanedFrom.SelectedItem = player.TeamLoanedFrom;
			}
			_ = player.loandateend;
		}
		else
		{
			player.IsLoaned = false;
		}
	}

	private void comboTeamLoanedFrom_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_Locked || m_CurrentTeamPlayer == null)
		{
			return;
		}
		Player player = m_CurrentTeamPlayer.Player;
		if (player != null)
		{
			if (comboTeamLoanedFrom.SelectedItem == null)
			{
				comboTeamLoanedFrom.Text = string.Empty;
			}
			else
			{
				player.TeamLoanedFrom = (Team)comboTeamLoanedFrom.SelectedItem;
			}
		}
	}

	private void dateLoanEnd_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			Player player = m_CurrentTeamPlayer.Player;
			if (player != null)
			{
				player.loandateend = dateLoanEnd.Value;
			}
		}
	}

	private void comboTeamPrevious_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_Locked || m_CurrentTeamPlayer == null)
		{
			return;
		}
		Player player = m_CurrentTeamPlayer.Player;
		if (player == null)
		{
			return;
		}
		Team team = (Team)comboTeamPrevious.SelectedItem;
		if (team != null)
		{
			if (team != m_CurrentTeamPlayer.Team)
			{
				player.PreviousTeam = team;
			}
			else
			{
				player.PreviousTeam = null;
			}
		}
	}

	private void listViewTeamPlayers_KeyPress(object sender, KeyPressEventArgs e)
	{
		char keyChar = e.KeyChar;
		switch (keyChar)
		{
		case '\r':
		{
			m_NewJerseyNum = -1;
			if (!(m_NewJerseyNumber != string.Empty))
			{
				return;
			}
			if (m_NewJerseyNumber.Length > 2)
			{
				m_NewJerseyNumber = m_NewJerseyNumber.Substring(m_NewJerseyNumber.Length - 2);
			}
			m_NewJerseyNum = Convert.ToInt32(m_NewJerseyNumber);
			int jerseynumber = m_CurrentTeamPlayer.jerseynumber;
			TeamPlayer teamPlayer = m_CurrentTeam.Roster.IsNumberUsed(m_NewJerseyNum);
			for (int i = 0; i < listViewTeamPlayers.Items.Count; i++)
			{
				if (m_CurrentTeamPlayer == (TeamPlayer)listViewTeamPlayers.Items[i].Tag)
				{
					m_CurrentTeamPlayer.jerseynumber = m_NewJerseyNum;
					listViewTeamPlayers.Items[i].SubItems[5].Text = FifaUtil.PadBlanks(m_NewJerseyNumber, 2);
				}
				else if (teamPlayer == (TeamPlayer)listViewTeamPlayers.Items[i].Tag)
				{
					teamPlayer.jerseynumber = jerseynumber;
					listViewTeamPlayers.Items[i].SubItems[5].Text = FifaUtil.PadBlanks(teamPlayer.jerseynumber.ToString(), 2);
				}
			}
			m_NewJerseyNumber = string.Empty;
			return;
		}
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			m_NewJerseyNumber += keyChar;
			return;
		}
		switch (keyChar)
		{
		case '+':
			m_CurrentTeamPlayer.Player.ChangeSkills(1);
			listViewTeamPlayers.SelectedItems[0].SubItems[4].Text = m_CurrentTeamPlayer.Player.GetAverageRoleAttribute().ToString();
			break;
		case '-':
			m_CurrentTeamPlayer.Player.ChangeSkills(-1);
			listViewTeamPlayers.SelectedItems[0].SubItems[4].Text = m_CurrentTeamPlayer.Player.GetAverageRoleAttribute().ToString();
			break;
		case '<':
			numericRosterYear.Value = m_CurrentTeamPlayer.Player.contractvaliduntil - 1;
			break;
		case '>':
			numericRosterYear.Value = m_CurrentTeamPlayer.Player.contractvaliduntil + 1;
			break;
		case '.':
			m_CurrentTeamPlayer.State += 1;
			if (m_CurrentTeamPlayer.State == 2)
			{
				m_CurrentTeamPlayer.State = -1;
			}
			switch (m_CurrentTeamPlayer.State)
			{
			case 0:
				listViewTeamPlayers.SelectedItems[0].ForeColor = Color.Black;
				break;
			case 1:
				listViewTeamPlayers.SelectedItems[0].ForeColor = Color.Green;
				break;
			case -1:
				listViewTeamPlayers.SelectedItems[0].ForeColor = Color.Red;
				break;
			}
			break;
		}
	}

	private void buttonDeletePlayer_Click(object sender, EventArgs e)
	{
		if (listViewPlayersAvailable.SelectedItems.Count <= 0)
		{
			return;
		}
		Player player = (Player)listViewPlayersAvailable.SelectedItems[0].Tag;
		if (player != null)
		{
			FifaEnvironment.Players.DeletePlayer(player);
			listViewPlayersAvailable.Items.Remove(listViewPlayersAvailable.SelectedItems[0]);
			if (listViewPlayersAvailable.Items.Count > 0)
			{
				listViewPlayersAvailable.Items[0].Selected = true;
			}
		}
	}

	private void listViewTeamPlayers_AfterLabelEdit(object sender, LabelEditEventArgs e)
	{
		string label = e.Label;
		m_CurrentTeamPlayer.Player.FastRename(label);
		e.CancelEdit = true;
		listViewTeamPlayers.SelectedItems[0].SubItems[0].Text = m_CurrentTeamPlayer.Player.Name;
		listViewTeamPlayers.SelectedItems[0].SubItems[1].Text = m_CurrentTeamPlayer.Player.firstname;
	}

	private void labelPos16_Click(object sender, EventArgs e)
	{
	}

	private void labelPos15_Click(object sender, EventArgs e)
	{
	}

	private void labelPos14_Click(object sender, EventArgs e)
	{
	}

	private void labelPos12_Click(object sender, EventArgs e)
	{
	}

	private void buttonTransferAll_Click(object sender, EventArgs e)
	{
		while (m_CurrentTeam.Roster.Count > 0)
		{
			m_CurrentTeamPlayer = (TeamPlayer)m_CurrentTeam.Roster[0];
			m_CurrentTeam.RemoveTeamPlayer(m_CurrentTeamPlayer);
			m_CurrentAvailableTeam.AddTeamPlayer(m_CurrentTeamPlayer);
			m_CurrentTeamPlayer.Player.joindate = dateTransferPreset.Value;
			m_CurrentTeamPlayer.Player.IsLoaned = false;
			m_CurrentTeamPlayer.Player.TeamLoanedFrom = m_CurrentTeam;
			if (m_CurrentTeamPlayer.Player.contractvaliduntil < m_CurrentTeamPlayer.Player.joindate.Year + 1)
			{
				m_CurrentTeamPlayer.Player.contractvaliduntil = m_CurrentTeamPlayer.Player.joindate.Year + 1;
			}
			m_CurrentTeamPlayer.Player.PreviousTeam = m_CurrentTeam;
		}
		InitListViewTeamPlayers(m_CurrentTeam.Roster);
		InitListViewPlayersAvailable(m_CurrentAvailableTeam, null, showFreeAgents: false);
		EnableRosterButtons();
		InitVisualFormation(m_CurrentTeam.Roster);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (Image image in m_Fc26MiniFaceCache.Values) image.Dispose();
			m_Fc26MiniFaceCache.Clear();
			if (m_Fc26PitchBackground != null) m_Fc26PitchBackground.Dispose();
			if (components != null) components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.TeamForm));
		this.tableEditTeam = new System.Windows.Forms.TabControl();
		this.pageTeamGeneric = new System.Windows.Forms.TabPage();
		this.flowPanelTeamGeneric = new System.Windows.Forms.FlowLayoutPanel();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.viewer2DCrest50 = new FifaControls.Viewer2D();
		this.buttonReplicateLogo = new System.Windows.Forms.Button();
		this.viewer2DCrestLarge = new FifaControls.Viewer2D();
		this.viewer2DCrest16 = new FifaControls.Viewer2D();
		this.viewer2DCrest32 = new FifaControls.Viewer2D();
		this.groupBoxName = new System.Windows.Forms.GroupBox();
		this.label3 = new System.Windows.Forms.Label();
		this.textTeamName7 = new System.Windows.Forms.TextBox();
		this.teamBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.textScoreBoardName = new System.Windows.Forms.TextBox();
		this.textDatabaseTeamName = new System.Windows.Forms.TextBox();
		this.textFullTeamName = new System.Windows.Forms.TextBox();
		this.textStandardTeamName = new System.Windows.Forms.TextBox();
		this.textShortTeamName = new System.Windows.Forms.TextBox();
		this.labelDatabaseTeamName = new System.Windows.Forms.Label();
		this.labelFullTeamName = new System.Windows.Forms.Label();
		this.labelStandardTeamName = new System.Windows.Forms.Label();
		this.labelShortTeamName = new System.Windows.Forms.Label();
		this.labelScoreBoardName = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.textStadiumName = new System.Windows.Forms.TextBox();
		this.labelStadiumName = new System.Windows.Forms.Label();
		this.comboStadiums = new System.Windows.Forms.ComboBox();
		this.stadiumListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.labelStadium = new System.Windows.Forms.Label();
		this.groupManager = new System.Windows.Forms.GroupBox();
		this.textBox3 = new System.Windows.Forms.TextBox();
		this.label17 = new System.Windows.Forms.Label();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.label16 = new System.Windows.Forms.Label();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.checkIsNationalTeam = new System.Windows.Forms.CheckBox();
		this.labelProbObjective = new System.Windows.Forms.Label();
		this.labelMaxObjective = new System.Windows.Forms.Label();
		this.comboProbObjective = new System.Windows.Forms.ComboBox();
		this.comboMaxOnjective = new System.Windows.Forms.ComboBox();
		this.comboObjective = new System.Windows.Forms.ComboBox();
		this.labelObjective = new System.Windows.Forms.Label();
		this.comboTeamLeague = new System.Windows.Forms.ComboBox();
		this.leagueListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.labelLeague = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.buttonGetId = new System.Windows.Forms.Button();
		this.pictureTeamTerColor = new System.Windows.Forms.PictureBox();
		this.label1 = new System.Windows.Forms.Label();
		this.comboRivalTeam = new System.Windows.Forms.ComboBox();
		this.pictureTeamPrimColor = new System.Windows.Forms.PictureBox();
		this.pictureTeamSecColor = new System.Windows.Forms.PictureBox();
		this.numericTeamId = new System.Windows.Forms.NumericUpDown();
		this.numericBall = new System.Windows.Forms.NumericUpDown();
		this.labelTeamId = new System.Windows.Forms.Label();
		this.pictureBall = new System.Windows.Forms.PictureBox();
		this.comboTeamCountry = new System.Windows.Forms.ComboBox();
		this.countryListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.numericStarsInternationalPrestige = new FifaControls.NumericStars();
		this.labelTeamCountry = new System.Windows.Forms.Label();
		this.labelOpponent = new System.Windows.Forms.Label();
		this.labelDomesticPrestige = new System.Windows.Forms.Label();
		this.numericStarsDomesticPrestige = new FifaControls.NumericStars();
		this.labelInitialBudget = new System.Windows.Forms.Label();
		this.labelInternationalPrestige = new System.Windows.Forms.Label();
		this.numericInitialBudget = new System.Windows.Forms.NumericUpDown();
		this.groupLastYear = new System.Windows.Forms.GroupBox();
		this.comboPrevLeague = new System.Windows.Forms.ComboBox();
		this.numericPositionLastYear = new System.Windows.Forms.NumericUpDown();
		this.checkIsChampion = new System.Windows.Forms.CheckBox();
		this.label19 = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.groupLocation = new System.Windows.Forms.GroupBox();
		this.numericUtcOffset = new System.Windows.Forms.NumericUpDown();
		this.numericLongitude = new System.Windows.Forms.NumericUpDown();
		this.numericLatitude = new System.Windows.Forms.NumericUpDown();
		this.label25 = new System.Windows.Forms.Label();
		this.label24 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.groupTeamTraits = new System.Windows.Forms.GroupBox();
		this.checkShortOutBack = new System.Windows.Forms.CheckBox();
		this.checkMoreAttackingAtHome = new System.Windows.Forms.CheckBox();
		this.checkCenterBacksSplit = new System.Windows.Forms.CheckBox();
		this.checkSwitchWingers = new System.Windows.Forms.CheckBox();
		this.checkKeepUpPressure = new System.Windows.Forms.CheckBox();
		this.checkDefendLead = new System.Windows.Forms.CheckBox();
		this.checkConsistentLineup = new System.Windows.Forms.CheckBox();
		this.checkSquadRotation = new System.Windows.Forms.CheckBox();
		this.checkLoyalBoard = new System.Windows.Forms.CheckBox();
		this.checkImpatientBoard = new System.Windows.Forms.CheckBox();
		this.groupBox7 = new System.Windows.Forms.GroupBox();
		this.labelThirdKit = new System.Windows.Forms.Label();
		this.labelKeeprKit = new System.Windows.Forms.Label();
		this.labelAwayKit = new System.Windows.Forms.Label();
		this.labelHomeKit = new System.Windows.Forms.Label();
		this.pageTeamRoster = new System.Windows.Forms.TabPage();
		this.groupBox6 = new System.Windows.Forms.GroupBox();
		this.labelCcpositioning = new System.Windows.Forms.Label();
		this.labelCcpassing = new System.Windows.Forms.Label();
		this.labelCccrossing = new System.Windows.Forms.Label();
		this.labelCcshooting = new System.Windows.Forms.Label();
		this.numericCcpassing = new System.Windows.Forms.NumericUpDown();
		this.numericCccrossing = new System.Windows.Forms.NumericUpDown();
		this.numericCcshooting = new System.Windows.Forms.NumericUpDown();
		this.comboCCPositioning = new System.Windows.Forms.ComboBox();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.labelBuspositioning = new System.Windows.Forms.Label();
		this.labelBusbuildupspeed = new System.Windows.Forms.Label();
		this.labelBuspassing = new System.Windows.Forms.Label();
		this.numericBusbuildupspeed = new System.Windows.Forms.NumericUpDown();
		this.numericBuspassing = new System.Windows.Forms.NumericUpDown();
		this.comboBUSPositioning = new System.Windows.Forms.ComboBox();
		this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
		this.label20 = new System.Windows.Forms.Label();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.labelDefdefendeline = new System.Windows.Forms.Label();
		this.labelDefmentality = new System.Windows.Forms.Label();
		this.labelDefaggression = new System.Windows.Forms.Label();
		this.labelDefteamwidth = new System.Windows.Forms.Label();
		this.numericDefmentality = new System.Windows.Forms.NumericUpDown();
		this.numericDefaggression = new System.Windows.Forms.NumericUpDown();
		this.numericDefteamwidth = new System.Windows.Forms.NumericUpDown();
		this.comboDEFLine = new System.Windows.Forms.ComboBox();
		this.labelRightFreeKickText = new System.Windows.Forms.Label();
		this.labelRightFreeKick = new System.Windows.Forms.Label();
		this.labelLeftFreeKickText = new System.Windows.Forms.Label();
		this.labelLeftFreeKick = new System.Windows.Forms.Label();
		this.groupFormation = new System.Windows.Forms.GroupBox();
		this.buttonCreateNewFormation = new System.Windows.Forms.Button();
		this.labelTeamFormationName = new System.Windows.Forms.Label();
		this.comboGenericFormations = new System.Windows.Forms.ComboBox();
		this.radioUseSpecificFormation = new System.Windows.Forms.RadioButton();
		this.radioUseGenericFormation = new System.Windows.Forms.RadioButton();
		this.labelLongKick = new System.Windows.Forms.Label();
		this.labelLomgKickText = new System.Windows.Forms.Label();
		this.labelRightCornerText = new System.Windows.Forms.Label();
		this.labelCaptainTetx = new System.Windows.Forms.Label();
		this.labelLeftCornertext = new System.Windows.Forms.Label();
		this.labelRightCorner = new System.Windows.Forms.Label();
		this.labelCaptain = new System.Windows.Forms.Label();
		this.labelLeftCorner = new System.Windows.Forms.Label();
		this.labelFreeKickText = new System.Windows.Forms.Label();
		this.labelPenaltyText = new System.Windows.Forms.Label();
		this.labelPenalty = new System.Windows.Forms.Label();
		this.labelFreeKick = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.labelPos33U = new System.Windows.Forms.Label();
		this.labelPos33T = new System.Windows.Forms.Label();
		this.labelPos33S = new System.Windows.Forms.Label();
		this.labelPos33R = new System.Windows.Forms.Label();
		this.labelPos33Q = new System.Windows.Forms.Label();
		this.labelPos33O = new System.Windows.Forms.Label();
		this.labelPos33P = new System.Windows.Forms.Label();
		this.labelPos33N = new System.Windows.Forms.Label();
		this.labelPos33M = new System.Windows.Forms.Label();
		this.labelPos33L = new System.Windows.Forms.Label();
		this.labelPos33K = new System.Windows.Forms.Label();
		this.labelPos33J = new System.Windows.Forms.Label();
		this.labelPos33H = new System.Windows.Forms.Label();
		this.labelPos33I = new System.Windows.Forms.Label();
		this.labelPos33G = new System.Windows.Forms.Label();
		this.labelPos33F = new System.Windows.Forms.Label();
		this.labelPos33E = new System.Windows.Forms.Label();
		this.labelPos33D = new System.Windows.Forms.Label();
		this.labelPos33C = new System.Windows.Forms.Label();
		this.labelPos33A = new System.Windows.Forms.Label();
		this.labelPos33B = new System.Windows.Forms.Label();
		this.labelPos32G = new System.Windows.Forms.Label();
		this.labelPos32F = new System.Windows.Forms.Label();
		this.labelPos32E = new System.Windows.Forms.Label();
		this.labelPos32D = new System.Windows.Forms.Label();
		this.labelPos32C = new System.Windows.Forms.Label();
		this.labelPos32A = new System.Windows.Forms.Label();
		this.labelPos32B = new System.Windows.Forms.Label();
		this.labelPos26 = new System.Windows.Forms.Label();
		this.labelPos27 = new System.Windows.Forms.Label();
		this.labelPos21 = new System.Windows.Forms.Label();
		this.labelPos22 = new System.Windows.Forms.Label();
		this.labelPos23 = new System.Windows.Forms.Label();
		this.labelPos24 = new System.Windows.Forms.Label();
		this.labelPos25 = new System.Windows.Forms.Label();
		this.labelPos14 = new System.Windows.Forms.Label();
		this.labelPos15 = new System.Windows.Forms.Label();
		this.labelPos16 = new System.Windows.Forms.Label();
		this.labelPos17 = new System.Windows.Forms.Label();
		this.labelPos18 = new System.Windows.Forms.Label();
		this.labelPos20 = new System.Windows.Forms.Label();
		this.labelPos19 = new System.Windows.Forms.Label();
		this.labelPos9 = new System.Windows.Forms.Label();
		this.labelPos10 = new System.Windows.Forms.Label();
		this.labelPos11 = new System.Windows.Forms.Label();
		this.labelPos12 = new System.Windows.Forms.Label();
		this.labelPos13 = new System.Windows.Forms.Label();
		this.labelPos2 = new System.Windows.Forms.Label();
		this.labelPos3 = new System.Windows.Forms.Label();
		this.labelPos4 = new System.Windows.Forms.Label();
		this.labelPos5 = new System.Windows.Forms.Label();
		this.labelPos6 = new System.Windows.Forms.Label();
		this.labelPos8 = new System.Windows.Forms.Label();
		this.labelPos7 = new System.Windows.Forms.Label();
		this.labelPos0 = new System.Windows.Forms.Label();
		this.labelPos1 = new System.Windows.Forms.Label();
		this.groupAvailablePlayers = new System.Windows.Forms.GroupBox();
		this.listViewPlayersAvailable = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
		this.panelAvailablePlayersTop = new System.Windows.Forms.Panel();
		this.buttonDeletePlayer = new System.Windows.Forms.Button();
		this.buttonLoanFrom = new System.Windows.Forms.Button();
		this.label4 = new System.Windows.Forms.Label();
		this.dateTransferPreset = new System.Windows.Forms.DateTimePicker();
		this.buttonTransferFrom = new System.Windows.Forms.Button();
		this.pickUpAvailablePlayers = new FifaControls.PickUpControl();
		this.buttonCall = new System.Windows.Forms.Button();
		this.labelAvailablePlayerStars = new System.Windows.Forms.Label();
		this.imageListStars = new System.Windows.Forms.ImageList(this.components);
		this.pictureAvailablePlayer = new System.Windows.Forms.PictureBox();
		this.groupTeamPlayers = new System.Windows.Forms.GroupBox();
		this.listViewTeamPlayers = new System.Windows.Forms.ListView();
		this.columnRosterSurname = new System.Windows.Forms.ColumnHeader();
		this.columnRosterFirstName = new System.Windows.Forms.ColumnHeader();
		this.columnRosterYearContract = new System.Windows.Forms.ColumnHeader();
		this.columnPreferredRole = new System.Windows.Forms.ColumnHeader();
		this.columnAverageAttributes = new System.Windows.Forms.ColumnHeader();
		this.columnRosterNum = new System.Windows.Forms.ColumnHeader();
		this.panelTeamPlayersTop = new System.Windows.Forms.Panel();
		this.buttonTransferAll = new System.Windows.Forms.Button();
		this.label5 = new System.Windows.Forms.Label();
		this.buttonPlusContract = new System.Windows.Forms.Button();
		this.buttonMinusContract = new System.Windows.Forms.Button();
		this.labelLoanedFrom = new System.Windows.Forms.Label();
		this.comboTeamLoanedFrom = new System.Windows.Forms.ComboBox();
		this.teamListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.label2 = new System.Windows.Forms.Label();
		this.dateLoanEnd = new System.Windows.Forms.DateTimePicker();
		this.labelLoanEnd = new System.Windows.Forms.Label();
		this.buttonRosterLetFree = new System.Windows.Forms.Button();
		this.buttonTransferPlayer = new System.Windows.Forms.Button();
		this.checkIsLoan = new System.Windows.Forms.CheckBox();
		this.numericRosterYear = new System.Windows.Forms.NumericUpDown();
		this.buttonLoanTo = new System.Windows.Forms.Button();
		this.dateJoiningDate = new System.Windows.Forms.DateTimePicker();
		this.viewer2DPhoto = new FifaControls.Viewer2D();
		this.labelJoiningDate = new System.Windows.Forms.Label();
		this.groupTeamPlayerTuning = new System.Windows.Forms.GroupBox();
		this.buttonTeamPlayerMinus = new System.Windows.Forms.Button();
		this.buttonTeamPlayerPlus = new System.Windows.Forms.Button();
		this.labelTeamPlayerStars = new System.Windows.Forms.Label();
		this.labelRosterName = new System.Windows.Forms.Label();
		this.comboRosterNumber = new System.Windows.Forms.ComboBox();
		this.labelRosterNumber = new System.Windows.Forms.Label();
		this.labelRosterNameFrom = new System.Windows.Forms.Label();
		this.labelPreviousTeam = new System.Windows.Forms.Label();
		this.comboTeamPrevious = new System.Windows.Forms.ComboBox();
		this.pageTeamAdboard = new System.Windows.Forms.TabPage();
		this.numericAdboards = new System.Windows.Forms.NumericUpDown();
		this.checkHasSpecificAdboard = new System.Windows.Forms.CheckBox();
		this.labelAdboard = new System.Windows.Forms.Label();
		this.viewer2DAdboards_0 = new FifaControls.Viewer2D();
		this.pageTeamFlags = new System.Windows.Forms.TabPage();
		this.groupFlag = new System.Windows.Forms.GroupBox();
		this.multiViewer2DFlags15 = new FifaControls.MultiViewer2D();
		this.buttonCreateFlags = new System.Windows.Forms.Button();
		this.pictureBox4 = new System.Windows.Forms.PictureBox();
		this.label22 = new System.Windows.Forms.Label();
		this.pictureFlagBlue = new System.Windows.Forms.PictureBox();
		this.pictureFlagRed = new System.Windows.Forms.PictureBox();
		this.pictureFlagGreen = new System.Windows.Forms.PictureBox();
		this.checkFlag4 = new System.Windows.Forms.CheckBox();
		this.checkFlag3 = new System.Windows.Forms.CheckBox();
		this.checkFlag2 = new System.Windows.Forms.CheckBox();
		this.checkFlag1 = new System.Windows.Forms.CheckBox();
		this.labelFlag4 = new System.Windows.Forms.Label();
		this.imageListFlags = new System.Windows.Forms.ImageList(this.components);
		this.labelFlag3 = new System.Windows.Forms.Label();
		this.labelFlag2 = new System.Windows.Forms.Label();
		this.labelFlag1 = new System.Windows.Forms.Label();
		this.viewer2DBanners = new FifaControls.Viewer2D();
		this.pageTeamrevMod = new System.Windows.Forms.TabPage();
		this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
		this.groupTeamAdboardsRevMod = new System.Windows.Forms.GroupBox();
		this.viewer2DTeamAdboard = new FifaControls.Viewer2D();
		this.groupTeamBallRevMod = new System.Windows.Forms.GroupBox();
		this.toolTeamBall3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DBall = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImport3DModelTeamBall = new System.Windows.Forms.ToolStripButton();
		this.buttonExport3DModelTeamBall = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonRemove3DModelTeamBall = new System.Windows.Forms.ToolStripButton();
		this.multiViewer2DTeamBallTextures = new FifaControls.MultiViewer2D();
		this.groupTeamManager = new System.Windows.Forms.GroupBox();
		this.toolTeamManager3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DManager = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImportModel3DTeamManager = new System.Windows.Forms.ToolStripButton();
		this.buttonExportModel3DTeamManager = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonDeleteModel3DTeamManager = new System.Windows.Forms.ToolStripButton();
		this.viewer2DTeamManager = new FifaControls.Viewer2D();
		this.groupTeamScarfRevMod = new System.Windows.Forms.GroupBox();
		this.multiViewer2DTeamScarf = new FifaControls.MultiViewer2D();
		this.groupTeamGoalNetRevMod = new System.Windows.Forms.GroupBox();
		this.viewer2DTeamNet = new FifaControls.Viewer2D();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.colorDialog = new System.Windows.Forms.ColorDialog();
		this.imageListPlayers = new System.Windows.Forms.ImageList(this.components);
		this.imageListArrows = new System.Windows.Forms.ImageList(this.components);
		this.pickUpControl = new FifaControls.PickUpControl();
		this.formationListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.ballListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.prevLeagueListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.tableEditTeam.SuspendLayout();
		this.pageTeamGeneric.SuspendLayout();
		this.flowPanelTeamGeneric.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.groupBoxName.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.teamBindingSource).BeginInit();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.stadiumListBindingSource).BeginInit();
		this.groupManager.SuspendLayout();
		this.groupBox3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.leagueListBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamTerColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamPrimColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamSecColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTeamId).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBall).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBall).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericInitialBudget).BeginInit();
		this.groupLastYear.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericPositionLastYear).BeginInit();
		this.groupLocation.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUtcOffset).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericLongitude).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericLatitude).BeginInit();
		this.groupTeamTraits.SuspendLayout();
		this.groupBox7.SuspendLayout();
		this.pageTeamRoster.SuspendLayout();
		this.groupBox6.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCcpassing).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericCccrossing).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericCcshooting).BeginInit();
		this.groupBox5.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericBusbuildupspeed).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBuspassing).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
		this.groupBox4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericDefmentality).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericDefaggression).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericDefteamwidth).BeginInit();
		this.groupFormation.SuspendLayout();
		this.panel1.SuspendLayout();
		this.groupAvailablePlayers.SuspendLayout();
		this.panelAvailablePlayersTop.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureAvailablePlayer).BeginInit();
		this.groupTeamPlayers.SuspendLayout();
		this.panelTeamPlayersTop.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericRosterYear).BeginInit();
		this.groupTeamPlayerTuning.SuspendLayout();
		this.pageTeamAdboard.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericAdboards).BeginInit();
		this.pageTeamFlags.SuspendLayout();
		this.groupFlag.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFlagBlue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFlagRed).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFlagGreen).BeginInit();
		this.pageTeamrevMod.SuspendLayout();
		this.flowLayoutPanel1.SuspendLayout();
		this.groupTeamAdboardsRevMod.SuspendLayout();
		this.groupTeamBallRevMod.SuspendLayout();
		this.toolTeamBall3D.SuspendLayout();
		this.groupTeamManager.SuspendLayout();
		this.toolTeamManager3D.SuspendLayout();
		this.groupTeamScarfRevMod.SuspendLayout();
		this.groupTeamGoalNetRevMod.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.formationListBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ballListBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.prevLeagueListBindingSource).BeginInit();
		base.SuspendLayout();
		this.tableEditTeam.Controls.Add(this.pageTeamGeneric);
		this.tableEditTeam.Controls.Add(this.pageTeamRoster);
		this.tableEditTeam.Controls.Add(this.pageTeamAdboard);
		this.tableEditTeam.Controls.Add(this.pageTeamFlags);
		this.tableEditTeam.Controls.Add(this.pageTeamrevMod);
		this.tableEditTeam.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableEditTeam.Location = new System.Drawing.Point(0, 25);
		this.tableEditTeam.Name = "tableEditTeam";
		this.tableEditTeam.SelectedIndex = 0;
		this.tableEditTeam.Size = new System.Drawing.Size(1311, 807);
		this.tableEditTeam.TabIndex = 5;
		this.tableEditTeam.SelectedIndexChanged += new System.EventHandler(tableEditTeam_SelectedIndexChanged);
		this.pageTeamGeneric.AutoScroll = true;
		this.pageTeamGeneric.Controls.Add(this.flowPanelTeamGeneric);
		this.pageTeamGeneric.Location = new System.Drawing.Point(4, 22);
		this.pageTeamGeneric.Name = "pageTeamGeneric";
		this.pageTeamGeneric.Padding = new System.Windows.Forms.Padding(3);
		this.pageTeamGeneric.Size = new System.Drawing.Size(1303, 781);
		this.pageTeamGeneric.TabIndex = 0;
		this.pageTeamGeneric.Text = "Generic";
		this.pageTeamGeneric.UseVisualStyleBackColor = true;
		this.flowPanelTeamGeneric.AutoScroll = true;
		this.flowPanelTeamGeneric.Controls.Add(this.groupBox2);
		this.flowPanelTeamGeneric.Controls.Add(this.groupBoxName);
		this.flowPanelTeamGeneric.Controls.Add(this.groupBox1);
		this.flowPanelTeamGeneric.Controls.Add(this.groupManager);
		this.flowPanelTeamGeneric.Controls.Add(this.groupBox3);
		this.flowPanelTeamGeneric.Controls.Add(this.groupLastYear);
		this.flowPanelTeamGeneric.Controls.Add(this.groupLocation);
		this.flowPanelTeamGeneric.Controls.Add(this.groupTeamTraits);
		this.flowPanelTeamGeneric.Controls.Add(this.groupBox7);
		this.flowPanelTeamGeneric.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowPanelTeamGeneric.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
		this.flowPanelTeamGeneric.Location = new System.Drawing.Point(3, 3);
		this.flowPanelTeamGeneric.Name = "flowPanelTeamGeneric";
		this.flowPanelTeamGeneric.Size = new System.Drawing.Size(1297, 775);
		this.flowPanelTeamGeneric.TabIndex = 0;
		this.groupBox2.Controls.Add(this.viewer2DCrest50);
		this.groupBox2.Controls.Add(this.buttonReplicateLogo);
		this.groupBox2.Controls.Add(this.viewer2DCrestLarge);
		this.groupBox2.Controls.Add(this.viewer2DCrest16);
		this.groupBox2.Controls.Add(this.viewer2DCrest32);
		this.groupBox2.Location = new System.Drawing.Point(3, 3);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(270, 445);
		this.groupBox2.TabIndex = 12;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Logos";
		this.viewer2DCrest50.AutoTransparency = true;
		this.viewer2DCrest50.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DCrest50.ButtonStripVisible = false;
		this.viewer2DCrest50.CurrentBitmap = null;
		this.viewer2DCrest50.ExtendedFormat = false;
		this.viewer2DCrest50.FullSizeButton = false;
		this.viewer2DCrest50.ImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.viewer2DCrest50.ImageSize = new System.Drawing.Size(64, 64);
		this.viewer2DCrest50.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DCrest50.Location = new System.Drawing.Point(7, 306);
		this.viewer2DCrest50.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DCrest50.Name = "viewer2DCrest50";
		this.viewer2DCrest50.RemoveButton = false;
		this.viewer2DCrest50.ShowButton = false;
		this.viewer2DCrest50.ShowButtonChecked = true;
		this.viewer2DCrest50.Size = new System.Drawing.Size(64, 89);
		this.viewer2DCrest50.TabIndex = 151;
		this.viewer2DCrest50.TabStop = false;
		this.toolTip.SetToolTip(this.viewer2DCrest50, "Crest 50x50");
		this.buttonReplicateLogo.Location = new System.Drawing.Point(78, 403);
		this.buttonReplicateLogo.Name = "buttonReplicateLogo";
		this.buttonReplicateLogo.Size = new System.Drawing.Size(117, 25);
		this.buttonReplicateLogo.TabIndex = 150;
		this.buttonReplicateLogo.Text = "Replicate";
		this.buttonReplicateLogo.UseVisualStyleBackColor = true;
		this.buttonReplicateLogo.Click += new System.EventHandler(buttonReplicateLogo_Click);
		this.viewer2DCrestLarge.AutoTransparency = true;
		this.viewer2DCrestLarge.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DCrestLarge.ButtonStripVisible = false;
		this.viewer2DCrestLarge.CurrentBitmap = null;
		this.viewer2DCrestLarge.ExtendedFormat = false;
		this.viewer2DCrestLarge.FullSizeButton = false;
		this.viewer2DCrestLarge.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DCrestLarge.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DCrestLarge.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.Auto256;
		this.viewer2DCrestLarge.Location = new System.Drawing.Point(6, 19);
		this.viewer2DCrestLarge.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DCrestLarge.Name = "viewer2DCrestLarge";
		this.viewer2DCrestLarge.RemoveButton = false;
		this.viewer2DCrestLarge.ShowButton = false;
		this.viewer2DCrestLarge.ShowButtonChecked = true;
		this.viewer2DCrestLarge.Size = new System.Drawing.Size(256, 281);
		this.viewer2DCrestLarge.TabIndex = 149;
		this.viewer2DCrestLarge.TabStop = false;
		this.toolTip.SetToolTip(this.viewer2DCrestLarge, "Country Map");
		this.viewer2DCrest16.AutoTransparency = true;
		this.viewer2DCrest16.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DCrest16.ButtonStripVisible = false;
		this.viewer2DCrest16.CurrentBitmap = null;
		this.viewer2DCrest16.ExtendedFormat = false;
		this.viewer2DCrest16.FullSizeButton = false;
		this.viewer2DCrest16.ImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.viewer2DCrest16.ImageSize = new System.Drawing.Size(16, 16);
		this.viewer2DCrest16.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DCrest16.Location = new System.Drawing.Point(194, 306);
		this.viewer2DCrest16.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DCrest16.Name = "viewer2DCrest16";
		this.viewer2DCrest16.RemoveButton = false;
		this.viewer2DCrest16.ShowButton = false;
		this.viewer2DCrest16.ShowButtonChecked = true;
		this.viewer2DCrest16.Size = new System.Drawing.Size(64, 89);
		this.viewer2DCrest16.TabIndex = 148;
		this.viewer2DCrest16.TabStop = false;
		this.toolTip.SetToolTip(this.viewer2DCrest16, "Crest 16x16");
		this.viewer2DCrest32.AutoTransparency = true;
		this.viewer2DCrest32.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DCrest32.ButtonStripVisible = false;
		this.viewer2DCrest32.CurrentBitmap = null;
		this.viewer2DCrest32.ExtendedFormat = false;
		this.viewer2DCrest32.FullSizeButton = false;
		this.viewer2DCrest32.ImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.viewer2DCrest32.ImageSize = new System.Drawing.Size(32, 32);
		this.viewer2DCrest32.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DCrest32.Location = new System.Drawing.Point(102, 306);
		this.viewer2DCrest32.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DCrest32.Name = "viewer2DCrest32";
		this.viewer2DCrest32.RemoveButton = false;
		this.viewer2DCrest32.ShowButton = false;
		this.viewer2DCrest32.ShowButtonChecked = true;
		this.viewer2DCrest32.Size = new System.Drawing.Size(64, 89);
		this.viewer2DCrest32.TabIndex = 147;
		this.viewer2DCrest32.TabStop = false;
		this.toolTip.SetToolTip(this.viewer2DCrest32, "Crest 32x32");
		this.groupBoxName.Controls.Add(this.label3);
		this.groupBoxName.Controls.Add(this.textTeamName7);
		this.groupBoxName.Controls.Add(this.textScoreBoardName);
		this.groupBoxName.Controls.Add(this.textDatabaseTeamName);
		this.groupBoxName.Controls.Add(this.textFullTeamName);
		this.groupBoxName.Controls.Add(this.textStandardTeamName);
		this.groupBoxName.Controls.Add(this.textShortTeamName);
		this.groupBoxName.Controls.Add(this.labelDatabaseTeamName);
		this.groupBoxName.Controls.Add(this.labelFullTeamName);
		this.groupBoxName.Controls.Add(this.labelStandardTeamName);
		this.groupBoxName.Controls.Add(this.labelShortTeamName);
		this.groupBoxName.Controls.Add(this.labelScoreBoardName);
		this.groupBoxName.Location = new System.Drawing.Point(3, 454);
		this.groupBoxName.Name = "groupBoxName";
		this.groupBoxName.Size = new System.Drawing.Size(270, 160);
		this.groupBoxName.TabIndex = 0;
		this.groupBoxName.TabStop = false;
		this.groupBoxName.Text = "Name";
		this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
		this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label3.Location = new System.Drawing.Point(4, 107);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(88, 20);
		this.label3.TabIndex = 56;
		this.label3.Text = "Name (7 chars)";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.label3, "Double click to fill automatically");
		this.label3.Click += new System.EventHandler(label3_Click);
		this.label3.DoubleClick += new System.EventHandler(labelTeamName7_DoubleClick);
		this.textTeamName7.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "TeamNameAbbr7", true));
		this.textTeamName7.Location = new System.Drawing.Point(98, 107);
		this.textTeamName7.Name = "textTeamName7";
		this.textTeamName7.Size = new System.Drawing.Size(160, 20);
		this.textTeamName7.TabIndex = 4;
		this.textTeamName7.TextChanged += new System.EventHandler(textTeamName7_TextChanged);
		this.teamBindingSource.DataSource = typeof(FifaLibrary.Team);
		this.textScoreBoardName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "TeamNameAbbr3", true));
		this.textScoreBoardName.Location = new System.Drawing.Point(98, 130);
		this.textScoreBoardName.Name = "textScoreBoardName";
		this.textScoreBoardName.Size = new System.Drawing.Size(160, 20);
		this.textScoreBoardName.TabIndex = 5;
		this.textScoreBoardName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.textDatabaseTeamName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "DatabaseName", true));
		this.textDatabaseTeamName.Location = new System.Drawing.Point(98, 15);
		this.textDatabaseTeamName.Name = "textDatabaseTeamName";
		this.textDatabaseTeamName.Size = new System.Drawing.Size(160, 20);
		this.textDatabaseTeamName.TabIndex = 0;
		this.textFullTeamName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "TeamNameFull", true));
		this.textFullTeamName.Location = new System.Drawing.Point(98, 38);
		this.textFullTeamName.Name = "textFullTeamName";
		this.textFullTeamName.Size = new System.Drawing.Size(160, 20);
		this.textFullTeamName.TabIndex = 1;
		this.textStandardTeamName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "TeamNameAbbr15", true));
		this.textStandardTeamName.Location = new System.Drawing.Point(98, 61);
		this.textStandardTeamName.Name = "textStandardTeamName";
		this.textStandardTeamName.Size = new System.Drawing.Size(160, 20);
		this.textStandardTeamName.TabIndex = 2;
		this.textStandardTeamName.TextChanged += new System.EventHandler(textStandardTeamName_TextChanged);
		this.textShortTeamName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "TeamNameAbbr10", true));
		this.textShortTeamName.Location = new System.Drawing.Point(98, 84);
		this.textShortTeamName.Name = "textShortTeamName";
		this.textShortTeamName.Size = new System.Drawing.Size(160, 20);
		this.textShortTeamName.TabIndex = 3;
		this.textShortTeamName.TextChanged += new System.EventHandler(textShortTeamName_TextChanged);
		this.labelDatabaseTeamName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDatabaseTeamName.Location = new System.Drawing.Point(4, 15);
		this.labelDatabaseTeamName.Name = "labelDatabaseTeamName";
		this.labelDatabaseTeamName.Size = new System.Drawing.Size(89, 20);
		this.labelDatabaseTeamName.TabIndex = 4;
		this.labelDatabaseTeamName.Text = "Database Name";
		this.labelDatabaseTeamName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelFullTeamName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFullTeamName.Location = new System.Drawing.Point(4, 38);
		this.labelFullTeamName.Name = "labelFullTeamName";
		this.labelFullTeamName.Size = new System.Drawing.Size(87, 20);
		this.labelFullTeamName.TabIndex = 52;
		this.labelFullTeamName.Text = "Full Name";
		this.labelFullTeamName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelStandardTeamName.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelStandardTeamName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStandardTeamName.Location = new System.Drawing.Point(4, 61);
		this.labelStandardTeamName.Name = "labelStandardTeamName";
		this.labelStandardTeamName.Size = new System.Drawing.Size(93, 20);
		this.labelStandardTeamName.TabIndex = 5;
		this.labelStandardTeamName.Text = "Name (15 chars)";
		this.labelStandardTeamName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.labelStandardTeamName, "Double click to fill automatically");
		this.labelStandardTeamName.DoubleClick += new System.EventHandler(labelStandardTeamName_DoubleClick);
		this.labelShortTeamName.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelShortTeamName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelShortTeamName.Location = new System.Drawing.Point(4, 84);
		this.labelShortTeamName.Name = "labelShortTeamName";
		this.labelShortTeamName.Size = new System.Drawing.Size(93, 20);
		this.labelShortTeamName.TabIndex = 6;
		this.labelShortTeamName.Text = "Name (10 chars)";
		this.labelShortTeamName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.labelShortTeamName, "Double click to fill automatically");
		this.labelShortTeamName.DoubleClick += new System.EventHandler(textShortTeamName_Click);
		this.labelScoreBoardName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelScoreBoardName.Location = new System.Drawing.Point(4, 130);
		this.labelScoreBoardName.Name = "labelScoreBoardName";
		this.labelScoreBoardName.Size = new System.Drawing.Size(88, 20);
		this.labelScoreBoardName.TabIndex = 54;
		this.labelScoreBoardName.Text = "Score Board";
		this.labelScoreBoardName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupBox1.Controls.Add(this.textStadiumName);
		this.groupBox1.Controls.Add(this.labelStadiumName);
		this.groupBox1.Controls.Add(this.comboStadiums);
		this.groupBox1.Controls.Add(this.labelStadium);
		this.groupBox1.Location = new System.Drawing.Point(3, 620);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(270, 67);
		this.groupBox1.TabIndex = 1;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Stadium";
		this.textStadiumName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "stadiumcustomname", true));
		this.textStadiumName.Location = new System.Drawing.Point(98, 41);
		this.textStadiumName.Name = "textStadiumName";
		this.textStadiumName.Size = new System.Drawing.Size(160, 20);
		this.textStadiumName.TabIndex = 1;
		this.textStadiumName.TextChanged += new System.EventHandler(textStadiumName_TextChanged);
		this.labelStadiumName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStadiumName.Location = new System.Drawing.Point(0, 41);
		this.labelStadiumName.Name = "labelStadiumName";
		this.labelStadiumName.Size = new System.Drawing.Size(90, 20);
		this.labelStadiumName.TabIndex = 73;
		this.labelStadiumName.Text = "Stadium Name";
		this.labelStadiumName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboStadiums.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.teamBindingSource, "Stadium", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
		this.comboStadiums.DataSource = this.stadiumListBindingSource;
		this.comboStadiums.Location = new System.Drawing.Point(98, 15);
		this.comboStadiums.Name = "comboStadiums";
		this.comboStadiums.Size = new System.Drawing.Size(160, 21);
		this.comboStadiums.TabIndex = 0;
		this.stadiumListBindingSource.DataSource = typeof(FifaLibrary.StadiumList);
		this.labelStadium.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelStadium.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelStadium.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelStadium.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelStadium.Location = new System.Drawing.Point(0, 15);
		this.labelStadium.Name = "labelStadium";
		this.labelStadium.Size = new System.Drawing.Size(101, 20);
		this.labelStadium.TabIndex = 71;
		this.labelStadium.Text = "Stadium Model";
		this.labelStadium.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelStadium.DoubleClick += new System.EventHandler(labelTeamStadium_DoubleClick);
		this.groupManager.Controls.Add(this.textBox3);
		this.groupManager.Controls.Add(this.label17);
		this.groupManager.Controls.Add(this.textBox2);
		this.groupManager.Controls.Add(this.label16);
		this.groupManager.Location = new System.Drawing.Point(3, 693);
		this.groupManager.Name = "groupManager";
		this.groupManager.Size = new System.Drawing.Size(270, 72);
		this.groupManager.TabIndex = 2;
		this.groupManager.TabStop = false;
		this.groupManager.Text = "Manager";
		this.textBox3.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "ManagerSurname", true));
		this.textBox3.Location = new System.Drawing.Point(98, 40);
		this.textBox3.Name = "textBox3";
		this.textBox3.Size = new System.Drawing.Size(160, 20);
		this.textBox3.TabIndex = 1;
		this.label17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label17.Location = new System.Drawing.Point(6, 40);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(90, 20);
		this.label17.TabIndex = 77;
		this.label17.Text = "Surname";
		this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.teamBindingSource, "ManagerFirstName", true));
		this.textBox2.Location = new System.Drawing.Point(98, 16);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(160, 20);
		this.textBox2.TabIndex = 0;
		this.label16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label16.Location = new System.Drawing.Point(6, 16);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(90, 20);
		this.label16.TabIndex = 75;
		this.label16.Text = "First Name";
		this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupBox3.Controls.Add(this.checkIsNationalTeam);
		this.groupBox3.Controls.Add(this.labelProbObjective);
		this.groupBox3.Controls.Add(this.labelMaxObjective);
		this.groupBox3.Controls.Add(this.comboProbObjective);
		this.groupBox3.Controls.Add(this.comboMaxOnjective);
		this.groupBox3.Controls.Add(this.comboObjective);
		this.groupBox3.Controls.Add(this.labelObjective);
		this.groupBox3.Controls.Add(this.comboTeamLeague);
		this.groupBox3.Controls.Add(this.labelLeague);
		this.groupBox3.Controls.Add(this.label15);
		this.groupBox3.Controls.Add(this.buttonGetId);
		this.groupBox3.Controls.Add(this.pictureTeamTerColor);
		this.groupBox3.Controls.Add(this.label1);
		this.groupBox3.Controls.Add(this.comboRivalTeam);
		this.groupBox3.Controls.Add(this.pictureTeamPrimColor);
		this.groupBox3.Controls.Add(this.pictureTeamSecColor);
		this.groupBox3.Controls.Add(this.numericTeamId);
		this.groupBox3.Controls.Add(this.numericBall);
		this.groupBox3.Controls.Add(this.labelTeamId);
		this.groupBox3.Controls.Add(this.pictureBall);
		this.groupBox3.Controls.Add(this.comboTeamCountry);
		this.groupBox3.Controls.Add(this.numericStarsInternationalPrestige);
		this.groupBox3.Controls.Add(this.labelTeamCountry);
		this.groupBox3.Controls.Add(this.labelOpponent);
		this.groupBox3.Controls.Add(this.labelDomesticPrestige);
		this.groupBox3.Controls.Add(this.numericStarsDomesticPrestige);
		this.groupBox3.Controls.Add(this.labelInitialBudget);
		this.groupBox3.Controls.Add(this.labelInternationalPrestige);
		this.groupBox3.Controls.Add(this.numericInitialBudget);
		this.groupBox3.Location = new System.Drawing.Point(279, 3);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(270, 496);
		this.groupBox3.TabIndex = 3;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Info";
		this.checkIsNationalTeam.BackColor = System.Drawing.Color.Transparent;
		this.checkIsNationalTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkIsNationalTeam.Location = new System.Drawing.Point(6, 127);
		this.checkIsNationalTeam.Name = "checkIsNationalTeam";
		this.checkIsNationalTeam.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkIsNationalTeam.Size = new System.Drawing.Size(179, 17);
		this.checkIsNationalTeam.TabIndex = 155;
		this.checkIsNationalTeam.Text = "Is National Team";
		this.checkIsNationalTeam.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkIsNationalTeam.UseVisualStyleBackColor = true;
		this.checkIsNationalTeam.CheckedChanged += new System.EventHandler(checkIsNationalTeam_CheckedChanged);
		this.labelProbObjective.AutoSize = true;
		this.labelProbObjective.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelProbObjective.Location = new System.Drawing.Point(6, 285);
		this.labelProbObjective.Name = "labelProbObjective";
		this.labelProbObjective.Size = new System.Drawing.Size(49, 13);
		this.labelProbObjective.TabIndex = 154;
		this.labelProbObjective.Text = "Probable";
		this.labelProbObjective.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelMaxObjective.AutoSize = true;
		this.labelMaxObjective.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelMaxObjective.Location = new System.Drawing.Point(6, 258);
		this.labelMaxObjective.Name = "labelMaxObjective";
		this.labelMaxObjective.Size = new System.Drawing.Size(43, 13);
		this.labelMaxObjective.TabIndex = 153;
		this.labelMaxObjective.Text = "Highest";
		this.labelMaxObjective.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboProbObjective.FormattingEnabled = true;
		this.comboProbObjective.Items.AddRange(new object[17]
		{
			"Win the League Title", "Qualify for Champions' Cup", "Qualify for Euro League", "Finish Mid Table", "Avoid Relegation", "Avoid Finish in Bottom Part", "Gain Automatic Promotion", "Fight For Promotion", "Achieve a High Finish", "Fight for the League Title",
			"Qualify For Europe", "Run for the Playoffs", "Reach the Wildcard Stage.", "Reach the Quarter Final", "Reach the Playoff Semi Final", "Reach the Playoff Final", "Become the Playoff Champion"
		});
		this.comboProbObjective.Location = new System.Drawing.Point(92, 282);
		this.comboProbObjective.Name = "comboProbObjective";
		this.comboProbObjective.Size = new System.Drawing.Size(167, 21);
		this.comboProbObjective.TabIndex = 8;
		this.comboProbObjective.SelectedIndexChanged += new System.EventHandler(comboProbObjective_SelectedIndexChanged);
		this.comboMaxOnjective.FormattingEnabled = true;
		this.comboMaxOnjective.Items.AddRange(new object[17]
		{
			"Win the League Title", "Qualify for Champions' Cup", "Qualify for Euro League", "Finish Mid Table", "Avoid Relegation", "Avoid Finish in Bottom Part", "Gain Automatic Promotion", "Fight For Promotion", "Achieve a High Finish", "Fight for the League Title",
			"Qualify For Europe", "Run for the Playoffs", "Reach the Wildcard Stage.", "Reach the Quarter Final", "Reach the Playoff Semi Final", "Reach the Playoff Final", "Become the Playoff Champion"
		});
		this.comboMaxOnjective.Location = new System.Drawing.Point(92, 255);
		this.comboMaxOnjective.Name = "comboMaxOnjective";
		this.comboMaxOnjective.Size = new System.Drawing.Size(167, 21);
		this.comboMaxOnjective.TabIndex = 7;
		this.comboMaxOnjective.SelectedIndexChanged += new System.EventHandler(comboMaxOnjective_SelectedIndexChanged);
		this.comboObjective.FormattingEnabled = true;
		this.comboObjective.Items.AddRange(new object[17]
		{
			"Win the League Title", "Qualify for Champions' Cup", "Qualify for Euro League", "Finish Mid Table", "Avoid Relegation", "Avoid Finish in Bottom Part", "Gain Automatic Promotion", "Fight For Promotion", "Achieve a High Finish", "Fight for the League Title",
			"Qualify For Europe", "Run for the Playoffs", "Reach the Wildcard Stage.", "Reach the Quarter Final", "Reach the Playoff Semi Final", "Reach the Playoff Final", "Become the Playoff Champion"
		});
		this.comboObjective.Location = new System.Drawing.Point(92, 228);
		this.comboObjective.Name = "comboObjective";
		this.comboObjective.Size = new System.Drawing.Size(167, 21);
		this.comboObjective.TabIndex = 6;
		this.comboObjective.SelectedIndexChanged += new System.EventHandler(comboObjective_SelectedIndexChanged);
		this.labelObjective.AutoSize = true;
		this.labelObjective.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelObjective.Location = new System.Drawing.Point(6, 231);
		this.labelObjective.Name = "labelObjective";
		this.labelObjective.Size = new System.Drawing.Size(52, 13);
		this.labelObjective.TabIndex = 149;
		this.labelObjective.Text = "Objective";
		this.labelObjective.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboTeamLeague.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.teamBindingSource, "League", true));
		this.comboTeamLeague.DataSource = this.leagueListBindingSource;
		this.comboTeamLeague.Enabled = false;
		this.comboTeamLeague.Location = new System.Drawing.Point(92, 100);
		this.comboTeamLeague.Name = "comboTeamLeague";
		this.comboTeamLeague.Size = new System.Drawing.Size(167, 21);
		this.comboTeamLeague.TabIndex = 2;
		this.comboTeamLeague.SelectedIndexChanged += new System.EventHandler(comboTeamLeague_SelectedIndexChanged);
		this.leagueListBindingSource.DataSource = typeof(FifaLibrary.LeagueList);
		this.labelLeague.AutoSize = true;
		this.labelLeague.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelLeague.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelLeague.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelLeague.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeague.Location = new System.Drawing.Point(6, 97);
		this.labelLeague.Name = "labelLeague";
		this.labelLeague.Size = new System.Drawing.Size(43, 13);
		this.labelLeague.TabIndex = 148;
		this.labelLeague.Text = "League";
		this.labelLeague.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.labelLeague, "For changing the league go to the league page.");
		this.labelLeague.DoubleClick += new System.EventHandler(labelTeamLeague_DoubleClick);
		this.label15.AutoSize = true;
		this.label15.BackColor = System.Drawing.Color.Transparent;
		this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label15.Location = new System.Drawing.Point(6, 24);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(36, 13);
		this.label15.TabIndex = 147;
		this.label15.Text = "Colors";
		this.label15.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.buttonGetId.Image = (System.Drawing.Image)resources.GetObject("buttonGetId.Image");
		this.buttonGetId.Location = new System.Drawing.Point(184, 45);
		this.buttonGetId.Name = "buttonGetId";
		this.buttonGetId.Size = new System.Drawing.Size(28, 24);
		this.buttonGetId.TabIndex = 6;
		this.buttonGetId.TabStop = false;
		this.buttonGetId.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.buttonGetId.UseVisualStyleBackColor = true;
		this.buttonGetId.Click += new System.EventHandler(buttonGetId_Click);
		this.pictureTeamTerColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureTeamTerColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureTeamTerColor.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.teamBindingSource, "TeamColor3", true));
		this.pictureTeamTerColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureTeamTerColor.Location = new System.Drawing.Point(165, 18);
		this.pictureTeamTerColor.Name = "pictureTeamTerColor";
		this.pictureTeamTerColor.Size = new System.Drawing.Size(24, 24);
		this.pictureTeamTerColor.TabIndex = 146;
		this.pictureTeamTerColor.TabStop = false;
		this.pictureTeamTerColor.Click += new System.EventHandler(pictureTeamTerColor_Click);
		this.label1.AutoSize = true;
		this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline);
		this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1.Location = new System.Drawing.Point(6, 338);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(64, 13);
		this.label1.TabIndex = 96;
		this.label1.Text = "Ball Number";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.DoubleClick += new System.EventHandler(labelBall_DoubleClick);
		this.comboRivalTeam.FormattingEnabled = true;
		this.comboRivalTeam.Location = new System.Drawing.Point(92, 309);
		this.comboRivalTeam.Name = "comboRivalTeam";
		this.comboRivalTeam.Size = new System.Drawing.Size(167, 21);
		this.comboRivalTeam.TabIndex = 9;
		this.comboRivalTeam.SelectedIndexChanged += new System.EventHandler(comboRivalTeam_SelectedIndexChanged);
		this.pictureTeamPrimColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureTeamPrimColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureTeamPrimColor.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.teamBindingSource, "TeamColor1", true));
		this.pictureTeamPrimColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureTeamPrimColor.Location = new System.Drawing.Point(91, 18);
		this.pictureTeamPrimColor.Name = "pictureTeamPrimColor";
		this.pictureTeamPrimColor.Size = new System.Drawing.Size(24, 24);
		this.pictureTeamPrimColor.TabIndex = 144;
		this.pictureTeamPrimColor.TabStop = false;
		this.pictureTeamPrimColor.Click += new System.EventHandler(pictureTeamPrimColor_Click);
		this.pictureTeamSecColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureTeamSecColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureTeamSecColor.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.teamBindingSource, "TeamColor2", true));
		this.pictureTeamSecColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureTeamSecColor.Location = new System.Drawing.Point(128, 18);
		this.pictureTeamSecColor.Name = "pictureTeamSecColor";
		this.pictureTeamSecColor.Size = new System.Drawing.Size(24, 24);
		this.pictureTeamSecColor.TabIndex = 145;
		this.pictureTeamSecColor.TabStop = false;
		this.pictureTeamSecColor.Click += new System.EventHandler(pictureTeamSecColor_Click);
		this.numericTeamId.Location = new System.Drawing.Point(91, 47);
		this.numericTeamId.Maximum = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericTeamId.Name = "numericTeamId";
		this.numericTeamId.Size = new System.Drawing.Size(87, 20);
		this.numericTeamId.TabIndex = 0;
		this.numericTeamId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTeamId.Value = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericTeamId.ValueChanged += new System.EventHandler(numericTeamId_ValueChanged);
		this.numericBall.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "balltype", true));
		this.numericBall.Location = new System.Drawing.Point(168, 336);
		this.numericBall.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBall.Name = "numericBall";
		this.numericBall.Size = new System.Drawing.Size(91, 20);
		this.numericBall.TabIndex = 10;
		this.numericBall.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBall.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBall.ValueChanged += new System.EventHandler(numericBall_ValueChanged);
		this.labelTeamId.AutoSize = true;
		this.labelTeamId.BackColor = System.Drawing.Color.Transparent;
		this.labelTeamId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelTeamId.Location = new System.Drawing.Point(6, 49);
		this.labelTeamId.Name = "labelTeamId";
		this.labelTeamId.Size = new System.Drawing.Size(46, 13);
		this.labelTeamId.TabIndex = 5;
		this.labelTeamId.Text = "Team Id";
		this.labelTeamId.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.pictureBall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBall.Location = new System.Drawing.Point(10, 376);
		this.pictureBall.Name = "pictureBall";
		this.pictureBall.Size = new System.Drawing.Size(249, 110);
		this.pictureBall.TabIndex = 5;
		this.pictureBall.TabStop = false;
		this.comboTeamCountry.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.teamBindingSource, "Country", true));
		this.comboTeamCountry.DataSource = this.countryListBindingSource;
		this.comboTeamCountry.Location = new System.Drawing.Point(92, 73);
		this.comboTeamCountry.Name = "comboTeamCountry";
		this.comboTeamCountry.Size = new System.Drawing.Size(167, 21);
		this.comboTeamCountry.TabIndex = 1;
		this.comboTeamCountry.SelectedIndexChanged += new System.EventHandler(comboTeamCountry_SelectedIndexChanged);
		this.countryListBindingSource.DataSource = typeof(FifaLibrary.CountryList);
		this.numericStarsInternationalPrestige.BackColor = System.Drawing.Color.Transparent;
		this.numericStarsInternationalPrestige.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "internationalprestige", true));
		this.numericStarsInternationalPrestige.Location = new System.Drawing.Point(92, 176);
		this.numericStarsInternationalPrestige.Margin = new System.Windows.Forms.Padding(4);
		this.numericStarsInternationalPrestige.Maximum = 20;
		this.numericStarsInternationalPrestige.Name = "numericStarsInternationalPrestige";
		this.numericStarsInternationalPrestige.Size = new System.Drawing.Size(167, 20);
		this.numericStarsInternationalPrestige.TabIndex = 4;
		this.numericStarsInternationalPrestige.Value = 0;
		this.labelTeamCountry.AutoSize = true;
		this.labelTeamCountry.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelTeamCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTeamCountry.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelTeamCountry.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelTeamCountry.Location = new System.Drawing.Point(6, 73);
		this.labelTeamCountry.Name = "labelTeamCountry";
		this.labelTeamCountry.Size = new System.Drawing.Size(43, 13);
		this.labelTeamCountry.TabIndex = 122;
		this.labelTeamCountry.Text = "Country";
		this.labelTeamCountry.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelTeamCountry.DoubleClick += new System.EventHandler(labelTeamCountry_DoubleClick);
		this.labelOpponent.AutoSize = true;
		this.labelOpponent.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelOpponent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelOpponent.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelOpponent.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelOpponent.Location = new System.Drawing.Point(6, 312);
		this.labelOpponent.Name = "labelOpponent";
		this.labelOpponent.Size = new System.Drawing.Size(84, 13);
		this.labelOpponent.TabIndex = 124;
		this.labelOpponent.Text = "Opponent Team";
		this.labelOpponent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelOpponent.DoubleClick += new System.EventHandler(labelOpponent_DoubleClick);
		this.labelDomesticPrestige.AutoSize = true;
		this.labelDomesticPrestige.BackColor = System.Drawing.Color.Transparent;
		this.labelDomesticPrestige.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDomesticPrestige.Location = new System.Drawing.Point(6, 152);
		this.labelDomesticPrestige.Name = "labelDomesticPrestige";
		this.labelDomesticPrestige.Size = new System.Drawing.Size(51, 13);
		this.labelDomesticPrestige.TabIndex = 103;
		this.labelDomesticPrestige.Text = "Domestic";
		this.labelDomesticPrestige.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericStarsDomesticPrestige.BackColor = System.Drawing.Color.Transparent;
		this.numericStarsDomesticPrestige.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "domesticprestige", true));
		this.numericStarsDomesticPrestige.Location = new System.Drawing.Point(92, 150);
		this.numericStarsDomesticPrestige.Margin = new System.Windows.Forms.Padding(4);
		this.numericStarsDomesticPrestige.Maximum = 20;
		this.numericStarsDomesticPrestige.Name = "numericStarsDomesticPrestige";
		this.numericStarsDomesticPrestige.Size = new System.Drawing.Size(167, 20);
		this.numericStarsDomesticPrestige.TabIndex = 3;
		this.numericStarsDomesticPrestige.Value = 0;
		this.labelInitialBudget.AutoSize = true;
		this.labelInitialBudget.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelInitialBudget.Location = new System.Drawing.Point(6, 204);
		this.labelInitialBudget.Name = "labelInitialBudget";
		this.labelInitialBudget.Size = new System.Drawing.Size(41, 13);
		this.labelInitialBudget.TabIndex = 95;
		// FC26 stores club valuation here; career transfer budgets live outside
		// the teams table and must not be represented by this control.
		this.labelInitialBudget.Text = "Club Worth";
		this.labelInitialBudget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelInternationalPrestige.AutoSize = true;
		this.labelInternationalPrestige.BackColor = System.Drawing.Color.Transparent;
		this.labelInternationalPrestige.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelInternationalPrestige.Location = new System.Drawing.Point(6, 179);
		this.labelInternationalPrestige.Name = "labelInternationalPrestige";
		this.labelInternationalPrestige.Size = new System.Drawing.Size(65, 13);
		this.labelInternationalPrestige.TabIndex = 101;
		this.labelInternationalPrestige.Text = "International";
		this.labelInternationalPrestige.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericInitialBudget.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "transferbudget", true));
		this.numericInitialBudget.Increment = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.numericInitialBudget.Location = new System.Drawing.Point(92, 202);
		this.numericInitialBudget.Maximum = new decimal(new int[4] { 900000000, 0, 0, 0 });
		this.numericInitialBudget.Name = "numericInitialBudget";
		this.numericInitialBudget.Size = new System.Drawing.Size(167, 20);
		this.numericInitialBudget.TabIndex = 5;
		this.numericInitialBudget.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericInitialBudget.ThousandsSeparator = true;
		this.numericInitialBudget.Value = new decimal(new int[4] { 10000000, 0, 0, 0 });
		this.groupLastYear.Controls.Add(this.comboPrevLeague);
		this.groupLastYear.Controls.Add(this.numericPositionLastYear);
		this.groupLastYear.Controls.Add(this.checkIsChampion);
		this.groupLastYear.Controls.Add(this.label19);
		this.groupLastYear.Controls.Add(this.label18);
		this.groupLastYear.Location = new System.Drawing.Point(279, 505);
		this.groupLastYear.Name = "groupLastYear";
		this.groupLastYear.Size = new System.Drawing.Size(270, 101);
		this.groupLastYear.TabIndex = 4;
		this.groupLastYear.TabStop = false;
		this.groupLastYear.Text = "Last Year Performance";
		this.comboPrevLeague.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.teamBindingSource, "PrevLeague", true));
		this.comboPrevLeague.DataSource = this.leagueListBindingSource;
		this.comboPrevLeague.Location = new System.Drawing.Point(97, 18);
		this.comboPrevLeague.Name = "comboPrevLeague";
		this.comboPrevLeague.Size = new System.Drawing.Size(167, 21);
		this.comboPrevLeague.TabIndex = 0;
		this.comboPrevLeague.SelectedIndexChanged += new System.EventHandler(comboPrevLeague_SelectedIndexChanged);
		this.numericPositionLastYear.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "previousyeartableposition", true));
		this.numericPositionLastYear.Location = new System.Drawing.Point(97, 42);
		this.numericPositionLastYear.Maximum = new decimal(new int[4] { 256, 0, 0, 0 });
		this.numericPositionLastYear.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPositionLastYear.Name = "numericPositionLastYear";
		this.numericPositionLastYear.Size = new System.Drawing.Size(63, 20);
		this.numericPositionLastYear.TabIndex = 1;
		this.numericPositionLastYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPositionLastYear.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.checkIsChampion.AutoSize = true;
		this.checkIsChampion.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "IsChampion", true));
		this.checkIsChampion.Location = new System.Drawing.Point(6, 68);
		this.checkIsChampion.Name = "checkIsChampion";
		this.checkIsChampion.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkIsChampion.Size = new System.Drawing.Size(117, 17);
		this.checkIsChampion.TabIndex = 2;
		this.checkIsChampion.Text = "Is Champion           ";
		this.checkIsChampion.UseVisualStyleBackColor = true;
		this.label19.AutoSize = true;
		this.label19.BackColor = System.Drawing.Color.Transparent;
		this.label19.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label19.Location = new System.Drawing.Point(6, 44);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(44, 13);
		this.label19.TabIndex = 149;
		this.label19.Text = "Position";
		this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label18.AutoSize = true;
		this.label18.BackColor = System.Drawing.Color.Transparent;
		this.label18.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label18.Location = new System.Drawing.Point(7, 21);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(43, 13);
		this.label18.TabIndex = 148;
		this.label18.Text = "League";
		this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupLocation.Controls.Add(this.numericUtcOffset);
		this.groupLocation.Controls.Add(this.numericLongitude);
		this.groupLocation.Controls.Add(this.numericLatitude);
		this.groupLocation.Controls.Add(this.label25);
		this.groupLocation.Controls.Add(this.label24);
		this.groupLocation.Controls.Add(this.label23);
		this.groupLocation.Location = new System.Drawing.Point(279, 612);
		this.groupLocation.Name = "groupLocation";
		this.groupLocation.Size = new System.Drawing.Size(270, 102);
		this.groupLocation.TabIndex = 162;
		this.groupLocation.TabStop = false;
		this.groupLocation.Text = "Location";
		this.numericUtcOffset.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "utcoffset", true));
		this.numericUtcOffset.Location = new System.Drawing.Point(91, 73);
		this.numericUtcOffset.Maximum = new decimal(new int[4] { 13, 0, 0, 0 });
		this.numericUtcOffset.Minimum = new decimal(new int[4] { 12, 0, 0, -2147483648 });
		this.numericUtcOffset.Name = "numericUtcOffset";
		this.numericUtcOffset.Size = new System.Drawing.Size(87, 20);
		this.numericUtcOffset.TabIndex = 154;
		this.numericUtcOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUtcOffset.ValueChanged += new System.EventHandler(numericUtcOffset_ValueChanged);
		this.numericLongitude.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "longitude", true));
		this.numericLongitude.Location = new System.Drawing.Point(91, 45);
		this.numericLongitude.Maximum = new decimal(new int[4] { 180, 0, 0, 0 });
		this.numericLongitude.Minimum = new decimal(new int[4] { 180, 0, 0, -2147483648 });
		this.numericLongitude.Name = "numericLongitude";
		this.numericLongitude.Size = new System.Drawing.Size(87, 20);
		this.numericLongitude.TabIndex = 153;
		this.numericLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericLongitude.ValueChanged += new System.EventHandler(numericLongitude_ValueChanged);
		this.numericLatitude.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "latitude", true));
		this.numericLatitude.Location = new System.Drawing.Point(91, 19);
		this.numericLatitude.Maximum = new decimal(new int[4] { 90, 0, 0, 0 });
		this.numericLatitude.Minimum = new decimal(new int[4] { 90, 0, 0, -2147483648 });
		this.numericLatitude.Name = "numericLatitude";
		this.numericLatitude.Size = new System.Drawing.Size(87, 20);
		this.numericLatitude.TabIndex = 152;
		this.numericLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericLatitude.ValueChanged += new System.EventHandler(numericLatitude_ValueChanged);
		this.label25.AutoSize = true;
		this.label25.BackColor = System.Drawing.Color.Transparent;
		this.label25.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label25.Location = new System.Drawing.Point(6, 73);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(55, 13);
		this.label25.TabIndex = 151;
		this.label25.Text = "UTC Time";
		this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label24.AutoSize = true;
		this.label24.BackColor = System.Drawing.Color.Transparent;
		this.label24.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label24.Location = new System.Drawing.Point(6, 48);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(54, 13);
		this.label24.TabIndex = 150;
		this.label24.Text = "Longitude";
		this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label23.AutoSize = true;
		this.label23.BackColor = System.Drawing.Color.Transparent;
		this.label23.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label23.Location = new System.Drawing.Point(6, 24);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(45, 13);
		this.label23.TabIndex = 149;
		this.label23.Text = "Latitude";
		this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupTeamTraits.Controls.Add(this.checkShortOutBack);
		this.groupTeamTraits.Controls.Add(this.checkMoreAttackingAtHome);
		this.groupTeamTraits.Controls.Add(this.checkCenterBacksSplit);
		this.groupTeamTraits.Controls.Add(this.checkSwitchWingers);
		this.groupTeamTraits.Controls.Add(this.checkKeepUpPressure);
		this.groupTeamTraits.Controls.Add(this.checkDefendLead);
		this.groupTeamTraits.Controls.Add(this.checkConsistentLineup);
		this.groupTeamTraits.Controls.Add(this.checkSquadRotation);
		this.groupTeamTraits.Controls.Add(this.checkLoyalBoard);
		this.groupTeamTraits.Controls.Add(this.checkImpatientBoard);
		this.groupTeamTraits.Location = new System.Drawing.Point(555, 3);
		this.groupTeamTraits.Name = "groupTeamTraits";
		this.groupTeamTraits.Size = new System.Drawing.Size(270, 209);
		this.groupTeamTraits.TabIndex = 161;
		this.groupTeamTraits.TabStop = false;
		this.groupTeamTraits.Text = "Team Traits";
		this.checkShortOutBack.AutoSize = true;
		this.checkShortOutBack.BackColor = System.Drawing.Color.Transparent;
		this.checkShortOutBack.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "ShortOutBack", true));
		this.checkShortOutBack.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkShortOutBack.Location = new System.Drawing.Point(83, 170);
		this.checkShortOutBack.Name = "checkShortOutBack";
		this.checkShortOutBack.Size = new System.Drawing.Size(99, 17);
		this.checkShortOutBack.TabIndex = 9;
		this.checkShortOutBack.Text = "Short Out Back";
		this.checkShortOutBack.UseVisualStyleBackColor = false;
		this.checkMoreAttackingAtHome.AutoSize = true;
		this.checkMoreAttackingAtHome.BackColor = System.Drawing.Color.Transparent;
		this.checkMoreAttackingAtHome.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "MoreAttackingAtHome", true));
		this.checkMoreAttackingAtHome.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkMoreAttackingAtHome.Location = new System.Drawing.Point(83, 148);
		this.checkMoreAttackingAtHome.Name = "checkMoreAttackingAtHome";
		this.checkMoreAttackingAtHome.Size = new System.Drawing.Size(142, 17);
		this.checkMoreAttackingAtHome.TabIndex = 8;
		this.checkMoreAttackingAtHome.Text = "More Attacking At Home";
		this.checkMoreAttackingAtHome.UseVisualStyleBackColor = false;
		this.checkCenterBacksSplit.AutoSize = true;
		this.checkCenterBacksSplit.BackColor = System.Drawing.Color.Transparent;
		this.checkCenterBacksSplit.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "CenterBacksSplit", true));
		this.checkCenterBacksSplit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCenterBacksSplit.Location = new System.Drawing.Point(83, 126);
		this.checkCenterBacksSplit.Name = "checkCenterBacksSplit";
		this.checkCenterBacksSplit.Size = new System.Drawing.Size(113, 17);
		this.checkCenterBacksSplit.TabIndex = 7;
		this.checkCenterBacksSplit.Text = "Center Backs Split";
		this.checkCenterBacksSplit.UseVisualStyleBackColor = false;
		this.checkSwitchWingers.AutoSize = true;
		this.checkSwitchWingers.BackColor = System.Drawing.Color.Transparent;
		this.checkSwitchWingers.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "SwitchWingers", true));
		this.checkSwitchWingers.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkSwitchWingers.Location = new System.Drawing.Point(83, 104);
		this.checkSwitchWingers.Name = "checkSwitchWingers";
		this.checkSwitchWingers.Size = new System.Drawing.Size(100, 17);
		this.checkSwitchWingers.TabIndex = 6;
		this.checkSwitchWingers.Text = "Switch Wingers";
		this.checkSwitchWingers.UseVisualStyleBackColor = false;
		this.checkKeepUpPressure.AutoSize = true;
		this.checkKeepUpPressure.BackColor = System.Drawing.Color.Transparent;
		this.checkKeepUpPressure.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "KeepUpPressure", true));
		this.checkKeepUpPressure.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKeepUpPressure.Location = new System.Drawing.Point(142, 65);
		this.checkKeepUpPressure.Name = "checkKeepUpPressure";
		this.checkKeepUpPressure.Size = new System.Drawing.Size(112, 17);
		this.checkKeepUpPressure.TabIndex = 5;
		this.checkKeepUpPressure.Text = "Keep Up Pressure";
		this.checkKeepUpPressure.UseVisualStyleBackColor = false;
		this.checkDefendLead.AutoSize = true;
		this.checkDefendLead.BackColor = System.Drawing.Color.Transparent;
		this.checkDefendLead.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "DefendLead", true));
		this.checkDefendLead.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkDefendLead.Location = new System.Drawing.Point(19, 65);
		this.checkDefendLead.Name = "checkDefendLead";
		this.checkDefendLead.Size = new System.Drawing.Size(88, 17);
		this.checkDefendLead.TabIndex = 2;
		this.checkDefendLead.Text = "Defend Lead";
		this.checkDefendLead.UseVisualStyleBackColor = false;
		this.checkConsistentLineup.AutoSize = true;
		this.checkConsistentLineup.BackColor = System.Drawing.Color.Transparent;
		this.checkConsistentLineup.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "ConsistentLineup", true));
		this.checkConsistentLineup.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkConsistentLineup.Location = new System.Drawing.Point(142, 42);
		this.checkConsistentLineup.Name = "checkConsistentLineup";
		this.checkConsistentLineup.Size = new System.Drawing.Size(110, 17);
		this.checkConsistentLineup.TabIndex = 4;
		this.checkConsistentLineup.Text = "Consistent Lineup";
		this.checkConsistentLineup.UseVisualStyleBackColor = false;
		this.checkSquadRotation.AutoSize = true;
		this.checkSquadRotation.BackColor = System.Drawing.Color.Transparent;
		this.checkSquadRotation.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "SquadRotation", true));
		this.checkSquadRotation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkSquadRotation.Location = new System.Drawing.Point(19, 42);
		this.checkSquadRotation.Name = "checkSquadRotation";
		this.checkSquadRotation.Size = new System.Drawing.Size(100, 17);
		this.checkSquadRotation.TabIndex = 1;
		this.checkSquadRotation.Text = "Squad Rotation";
		this.checkSquadRotation.UseVisualStyleBackColor = false;
		this.checkLoyalBoard.AutoSize = true;
		this.checkLoyalBoard.BackColor = System.Drawing.Color.Transparent;
		this.checkLoyalBoard.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "LoyalBoard", true));
		this.checkLoyalBoard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLoyalBoard.Location = new System.Drawing.Point(142, 19);
		this.checkLoyalBoard.Name = "checkLoyalBoard";
		this.checkLoyalBoard.Size = new System.Drawing.Size(82, 17);
		this.checkLoyalBoard.TabIndex = 3;
		this.checkLoyalBoard.Text = "Loyal Board";
		this.checkLoyalBoard.UseVisualStyleBackColor = false;
		this.checkImpatientBoard.AutoSize = true;
		this.checkImpatientBoard.BackColor = System.Drawing.Color.Transparent;
		this.checkImpatientBoard.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.teamBindingSource, "ImpatientBoard", true));
		this.checkImpatientBoard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkImpatientBoard.Location = new System.Drawing.Point(19, 19);
		this.checkImpatientBoard.Name = "checkImpatientBoard";
		this.checkImpatientBoard.Size = new System.Drawing.Size(100, 17);
		this.checkImpatientBoard.TabIndex = 0;
		this.checkImpatientBoard.Text = "Impatient Board";
		this.checkImpatientBoard.UseVisualStyleBackColor = false;
		this.groupBox7.Controls.Add(this.labelThirdKit);
		this.groupBox7.Controls.Add(this.labelKeeprKit);
		this.groupBox7.Controls.Add(this.labelAwayKit);
		this.groupBox7.Controls.Add(this.labelHomeKit);
		this.groupBox7.Location = new System.Drawing.Point(555, 218);
		this.groupBox7.Name = "groupBox7";
		this.groupBox7.Size = new System.Drawing.Size(270, 61);
		this.groupBox7.TabIndex = 163;
		this.groupBox7.TabStop = false;
		this.groupBox7.Text = "Kit Links";
		this.labelThirdKit.AutoSize = true;
		this.labelThirdKit.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelThirdKit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline);
		this.labelThirdKit.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelThirdKit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelThirdKit.Location = new System.Drawing.Point(217, 30);
		this.labelThirdKit.Name = "labelThirdKit";
		this.labelThirdKit.Size = new System.Drawing.Size(37, 13);
		this.labelThirdKit.TabIndex = 100;
		this.labelThirdKit.Text = "3rd Kit";
		this.labelThirdKit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.labelThirdKit, "Double click to jump to the 3rd Kit, if any");
		this.labelThirdKit.DoubleClick += new System.EventHandler(labelThirdKit_DoubleClick);
		this.labelKeeprKit.AutoSize = true;
		this.labelKeeprKit.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelKeeprKit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline);
		this.labelKeeprKit.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelKeeprKit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelKeeprKit.Location = new System.Drawing.Point(142, 30);
		this.labelKeeprKit.Name = "labelKeeprKit";
		this.labelKeeprKit.Size = new System.Drawing.Size(56, 13);
		this.labelKeeprKit.TabIndex = 99;
		this.labelKeeprKit.Text = "Keeper Kit";
		this.labelKeeprKit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.labelKeeprKit, "Double click to jump to the Keepr Kit, if any");
		this.labelKeeprKit.DoubleClick += new System.EventHandler(labelKeeprKit_DoubleClick);
		this.labelAwayKit.AutoSize = true;
		this.labelAwayKit.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelAwayKit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline);
		this.labelAwayKit.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelAwayKit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelAwayKit.Location = new System.Drawing.Point(75, 30);
		this.labelAwayKit.Name = "labelAwayKit";
		this.labelAwayKit.Size = new System.Drawing.Size(48, 13);
		this.labelAwayKit.TabIndex = 98;
		this.labelAwayKit.Text = "Away Kit";
		this.labelAwayKit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.labelAwayKit, "Double click to jump to the Away Kit");
		this.labelAwayKit.DoubleClick += new System.EventHandler(labelAwayKit_DoubleClick);
		this.labelHomeKit.AutoSize = true;
		this.labelHomeKit.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelHomeKit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline);
		this.labelHomeKit.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelHomeKit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelHomeKit.Location = new System.Drawing.Point(6, 30);
		this.labelHomeKit.Name = "labelHomeKit";
		this.labelHomeKit.Size = new System.Drawing.Size(50, 13);
		this.labelHomeKit.TabIndex = 97;
		this.labelHomeKit.Text = "Home Kit";
		this.labelHomeKit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip.SetToolTip(this.labelHomeKit, "Double click to jump to the Home Kit");
		this.labelHomeKit.DoubleClick += new System.EventHandler(labelHomeKit_DoubleClick);
		this.pageTeamRoster.AutoScroll = true;
		this.pageTeamRoster.Controls.Add(this.groupBox6);
		this.pageTeamRoster.Controls.Add(this.groupBox5);
		this.pageTeamRoster.Controls.Add(this.groupBox4);
		this.pageTeamRoster.Controls.Add(this.labelRightFreeKickText);
		this.pageTeamRoster.Controls.Add(this.labelRightFreeKick);
		this.pageTeamRoster.Controls.Add(this.labelLeftFreeKickText);
		this.pageTeamRoster.Controls.Add(this.labelLeftFreeKick);
		this.pageTeamRoster.Controls.Add(this.groupFormation);
		this.pageTeamRoster.Controls.Add(this.labelLongKick);
		this.pageTeamRoster.Controls.Add(this.labelLomgKickText);
		this.pageTeamRoster.Controls.Add(this.labelRightCornerText);
		this.pageTeamRoster.Controls.Add(this.labelCaptainTetx);
		this.pageTeamRoster.Controls.Add(this.labelLeftCornertext);
		this.pageTeamRoster.Controls.Add(this.labelRightCorner);
		this.pageTeamRoster.Controls.Add(this.labelCaptain);
		this.pageTeamRoster.Controls.Add(this.labelLeftCorner);
		this.pageTeamRoster.Controls.Add(this.labelFreeKickText);
		this.pageTeamRoster.Controls.Add(this.labelPenaltyText);
		this.pageTeamRoster.Controls.Add(this.labelPenalty);
		this.pageTeamRoster.Controls.Add(this.labelFreeKick);
		this.pageTeamRoster.Controls.Add(this.panel1);
		this.pageTeamRoster.Controls.Add(this.groupAvailablePlayers);
		this.pageTeamRoster.Controls.Add(this.groupTeamPlayers);
		this.pageTeamRoster.Location = new System.Drawing.Point(4, 22);
		this.pageTeamRoster.Name = "pageTeamRoster";
		this.pageTeamRoster.Padding = new System.Windows.Forms.Padding(3);
		this.pageTeamRoster.Size = new System.Drawing.Size(1303, 781);
		this.pageTeamRoster.TabIndex = 1;
		this.pageTeamRoster.Text = "Roster";
		this.pageTeamRoster.UseVisualStyleBackColor = true;
		this.groupBox6.Controls.Add(this.labelCcpositioning);
		this.groupBox6.Controls.Add(this.labelCcpassing);
		this.groupBox6.Controls.Add(this.labelCccrossing);
		this.groupBox6.Controls.Add(this.labelCcshooting);
		this.groupBox6.Controls.Add(this.numericCcpassing);
		this.groupBox6.Controls.Add(this.numericCccrossing);
		this.groupBox6.Controls.Add(this.numericCcshooting);
		this.groupBox6.Controls.Add(this.comboCCPositioning);
		this.groupBox6.Location = new System.Drawing.Point(970, 630);
		this.groupBox6.Name = "groupBox6";
		this.groupBox6.Size = new System.Drawing.Size(230, 128);
		this.groupBox6.TabIndex = 272;
		this.groupBox6.TabStop = false;
		this.groupBox6.Text = "Chance Creation";
		this.labelCcpositioning.AutoSize = true;
		this.labelCcpositioning.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCcpositioning.Location = new System.Drawing.Point(6, 22);
		this.labelCcpositioning.Name = "labelCcpositioning";
		this.labelCcpositioning.Size = new System.Drawing.Size(98, 13);
		this.labelCcpositioning.TabIndex = 240;
		this.labelCcpositioning.Text = "Chance Positioning";
		this.labelCcpositioning.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCcpassing.AutoSize = true;
		this.labelCcpassing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCcpassing.Location = new System.Drawing.Point(6, 49);
		this.labelCcpassing.Name = "labelCcpassing";
		this.labelCcpassing.Size = new System.Drawing.Size(110, 13);
		this.labelCcpassing.TabIndex = 237;
		this.labelCcpassing.Text = "Passing (Safe - Risky)";
		this.labelCcpassing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCccrossing.AutoSize = true;
		this.labelCccrossing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCccrossing.Location = new System.Drawing.Point(6, 73);
		this.labelCccrossing.Name = "labelCccrossing";
		this.labelCccrossing.Size = new System.Drawing.Size(107, 13);
		this.labelCccrossing.TabIndex = 238;
		this.labelCccrossing.Text = "Crossing (Little - Lots)";
		this.labelCccrossing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCcshooting.AutoSize = true;
		this.labelCcshooting.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCcshooting.Location = new System.Drawing.Point(6, 97);
		this.labelCcshooting.Name = "labelCcshooting";
		this.labelCcshooting.Size = new System.Drawing.Size(109, 13);
		this.labelCcshooting.TabIndex = 239;
		this.labelCcshooting.Text = "Shooting (Little - Lots)";
		this.labelCcshooting.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericCcpassing.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "ccpassing", true));
		this.numericCcpassing.Location = new System.Drawing.Point(160, 45);
		this.numericCcpassing.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericCcpassing.Name = "numericCcpassing";
		this.numericCcpassing.Size = new System.Drawing.Size(64, 20);
		this.numericCcpassing.TabIndex = 234;
		this.numericCcpassing.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCcpassing.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericCccrossing.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "cccrossing", true));
		this.numericCccrossing.Location = new System.Drawing.Point(160, 69);
		this.numericCccrossing.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericCccrossing.Name = "numericCccrossing";
		this.numericCccrossing.Size = new System.Drawing.Size(64, 20);
		this.numericCccrossing.TabIndex = 235;
		this.numericCccrossing.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCccrossing.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericCcshooting.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "ccshooting", true));
		this.numericCcshooting.Location = new System.Drawing.Point(160, 93);
		this.numericCcshooting.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericCcshooting.Name = "numericCcshooting";
		this.numericCcshooting.Size = new System.Drawing.Size(64, 20);
		this.numericCcshooting.TabIndex = 236;
		this.numericCcshooting.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCcshooting.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.comboCCPositioning.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.teamBindingSource, "ccpositioning", true));
		this.comboCCPositioning.FormattingEnabled = true;
		this.comboCCPositioning.Items.AddRange(new object[2] { "Organized", "Free Form" });
		this.comboCCPositioning.Location = new System.Drawing.Point(110, 18);
		this.comboCCPositioning.Name = "comboCCPositioning";
		this.comboCCPositioning.Size = new System.Drawing.Size(114, 21);
		this.comboCCPositioning.TabIndex = 233;
		this.comboCCPositioning.SelectedIndexChanged += new System.EventHandler(comboCCPositioning_SelectedIndexChanged);
		this.groupBox5.Controls.Add(this.labelBuspositioning);
		this.groupBox5.Controls.Add(this.labelBusbuildupspeed);
		this.groupBox5.Controls.Add(this.labelBuspassing);
		this.groupBox5.Controls.Add(this.numericBusbuildupspeed);
		this.groupBox5.Controls.Add(this.numericBuspassing);
		this.groupBox5.Controls.Add(this.comboBUSPositioning);
		this.groupBox5.Controls.Add(this.numericUpDown2);
		this.groupBox5.Controls.Add(this.label20);
		this.groupBox5.Location = new System.Drawing.Point(734, 627);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(230, 128);
		this.groupBox5.TabIndex = 271;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "Build Up";
		this.labelBuspositioning.AutoSize = true;
		this.labelBuspositioning.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBuspositioning.Location = new System.Drawing.Point(6, 25);
		this.labelBuspositioning.Name = "labelBuspositioning";
		this.labelBuspositioning.Size = new System.Drawing.Size(101, 13);
		this.labelBuspositioning.TabIndex = 231;
		this.labelBuspositioning.Text = "Build Up Positioning";
		this.labelBuspositioning.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelBusbuildupspeed.AutoSize = true;
		this.labelBusbuildupspeed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBusbuildupspeed.Location = new System.Drawing.Point(6, 52);
		this.labelBusbuildupspeed.Name = "labelBusbuildupspeed";
		this.labelBusbuildupspeed.Size = new System.Drawing.Size(109, 13);
		this.labelBusbuildupspeed.TabIndex = 229;
		this.labelBusbuildupspeed.Text = "Speed (Patient - Fast)";
		this.labelBusbuildupspeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelBuspassing.AutoSize = true;
		this.labelBuspassing.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelBuspassing.Location = new System.Drawing.Point(6, 76);
		this.labelBuspassing.Name = "labelBuspassing";
		this.labelBuspassing.Size = new System.Drawing.Size(105, 13);
		this.labelBuspassing.TabIndex = 230;
		this.labelBuspassing.Text = "Passing (Short-Long)";
		this.labelBuspassing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericBusbuildupspeed.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "busbuildupspeed", true));
		this.numericBusbuildupspeed.Location = new System.Drawing.Point(161, 48);
		this.numericBusbuildupspeed.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBusbuildupspeed.Name = "numericBusbuildupspeed";
		this.numericBusbuildupspeed.Size = new System.Drawing.Size(64, 20);
		this.numericBusbuildupspeed.TabIndex = 226;
		this.numericBusbuildupspeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBusbuildupspeed.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericBuspassing.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "buspassing", true));
		this.numericBuspassing.Location = new System.Drawing.Point(161, 72);
		this.numericBuspassing.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBuspassing.Name = "numericBuspassing";
		this.numericBuspassing.Size = new System.Drawing.Size(64, 20);
		this.numericBuspassing.TabIndex = 227;
		this.numericBuspassing.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBuspassing.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.comboBUSPositioning.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.teamBindingSource, "buspositioning", true));
		this.comboBUSPositioning.FormattingEnabled = true;
		this.comboBUSPositioning.Items.AddRange(new object[2] { "Organized", "Free Form" });
		this.comboBUSPositioning.Location = new System.Drawing.Point(116, 21);
		this.comboBUSPositioning.Name = "comboBUSPositioning";
		this.comboBUSPositioning.Size = new System.Drawing.Size(109, 21);
		this.comboBUSPositioning.TabIndex = 228;
		this.comboBUSPositioning.SelectedIndexChanged += new System.EventHandler(comboBUSPositioning_SelectedIndexChanged);
		this.numericUpDown2.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "busdribbling", true));
		this.numericUpDown2.Location = new System.Drawing.Point(161, 96);
		this.numericUpDown2.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDown2.Name = "numericUpDown2";
		this.numericUpDown2.Size = new System.Drawing.Size(64, 20);
		this.numericUpDown2.TabIndex = 250;
		this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown2.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.label20.AutoSize = true;
		this.label20.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label20.Location = new System.Drawing.Point(6, 100);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(108, 13);
		this.label20.TabIndex = 251;
		this.label20.Text = "Dribbling (Little - Lots)";
		this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupBox4.Controls.Add(this.labelDefdefendeline);
		this.groupBox4.Controls.Add(this.labelDefmentality);
		this.groupBox4.Controls.Add(this.labelDefaggression);
		this.groupBox4.Controls.Add(this.labelDefteamwidth);
		this.groupBox4.Controls.Add(this.numericDefmentality);
		this.groupBox4.Controls.Add(this.numericDefaggression);
		this.groupBox4.Controls.Add(this.numericDefteamwidth);
		this.groupBox4.Controls.Add(this.comboDEFLine);
		this.groupBox4.Location = new System.Drawing.Point(970, 493);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(230, 128);
		this.groupBox4.TabIndex = 270;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Defense";
		this.labelDefdefendeline.AutoSize = true;
		this.labelDefdefendeline.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDefdefendeline.Location = new System.Drawing.Point(6, 22);
		this.labelDefdefendeline.Name = "labelDefdefendeline";
		this.labelDefdefendeline.Size = new System.Drawing.Size(71, 13);
		this.labelDefdefendeline.TabIndex = 248;
		this.labelDefdefendeline.Text = "Defende Line";
		this.labelDefdefendeline.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelDefmentality.AutoSize = true;
		this.labelDefmentality.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDefmentality.Location = new System.Drawing.Point(6, 49);
		this.labelDefmentality.Name = "labelDefmentality";
		this.labelDefmentality.Size = new System.Drawing.Size(104, 13);
		this.labelDefmentality.TabIndex = 245;
		this.labelDefmentality.Text = "Position (Deep-High)";
		this.labelDefmentality.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelDefaggression.AutoSize = true;
		this.labelDefaggression.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDefaggression.Location = new System.Drawing.Point(6, 73);
		this.labelDefaggression.Name = "labelDefaggression";
		this.labelDefaggression.Size = new System.Drawing.Size(113, 13);
		this.labelDefaggression.TabIndex = 246;
		this.labelDefaggression.Text = "Aggression (Low-High)";
		this.labelDefaggression.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelDefteamwidth.AutoSize = true;
		this.labelDefteamwidth.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDefteamwidth.Location = new System.Drawing.Point(6, 97);
		this.labelDefteamwidth.Name = "labelDefteamwidth";
		this.labelDefteamwidth.Size = new System.Drawing.Size(106, 13);
		this.labelDefteamwidth.TabIndex = 247;
		this.labelDefteamwidth.Text = "Width (Narrow-Wide)";
		this.labelDefteamwidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericDefmentality.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "defmentality", true));
		this.numericDefmentality.Location = new System.Drawing.Point(160, 47);
		this.numericDefmentality.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericDefmentality.Name = "numericDefmentality";
		this.numericDefmentality.Size = new System.Drawing.Size(64, 20);
		this.numericDefmentality.TabIndex = 242;
		this.numericDefmentality.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericDefmentality.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericDefaggression.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "defaggression", true));
		this.numericDefaggression.Location = new System.Drawing.Point(160, 71);
		this.numericDefaggression.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericDefaggression.Name = "numericDefaggression";
		this.numericDefaggression.Size = new System.Drawing.Size(64, 20);
		this.numericDefaggression.TabIndex = 243;
		this.numericDefaggression.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericDefaggression.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.numericDefteamwidth.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.teamBindingSource, "defteamwidth", true));
		this.numericDefteamwidth.Location = new System.Drawing.Point(160, 95);
		this.numericDefteamwidth.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericDefteamwidth.Name = "numericDefteamwidth";
		this.numericDefteamwidth.Size = new System.Drawing.Size(64, 20);
		this.numericDefteamwidth.TabIndex = 244;
		this.numericDefteamwidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericDefteamwidth.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.comboDEFLine.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.teamBindingSource, "defdefenderline", true));
		this.comboDEFLine.FormattingEnabled = true;
		this.comboDEFLine.Items.AddRange(new object[2] { "Cover", "Offside Trap" });
		this.comboDEFLine.Location = new System.Drawing.Point(102, 19);
		this.comboDEFLine.Name = "comboDEFLine";
		this.comboDEFLine.Size = new System.Drawing.Size(122, 21);
		this.comboDEFLine.TabIndex = 241;
		this.comboDEFLine.SelectedIndexChanged += new System.EventHandler(comboDEFLine_SelectedIndexChanged);
		this.labelRightFreeKickText.BackColor = System.Drawing.Color.Transparent;
		this.labelRightFreeKickText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRightFreeKickText.Location = new System.Drawing.Point(1213, 343);
		this.labelRightFreeKickText.Name = "labelRightFreeKickText";
		this.labelRightFreeKickText.Size = new System.Drawing.Size(85, 16);
		this.labelRightFreeKickText.TabIndex = 269;
		this.labelRightFreeKickText.Text = "Right Free Kick";
		this.labelRightFreeKickText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelRightFreeKick.AllowDrop = true;
		this.labelRightFreeKick.BackColor = System.Drawing.Color.Transparent;
		this.labelRightFreeKick.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelRightFreeKick.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelRightFreeKick.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelRightFreeKick.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelRightFreeKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRightFreeKick.Location = new System.Drawing.Point(1213, 359);
		this.labelRightFreeKick.Name = "labelRightFreeKick";
		this.labelRightFreeKick.Size = new System.Drawing.Size(85, 38);
		this.labelRightFreeKick.TabIndex = 268;
		this.labelRightFreeKick.Text = "Name";
		this.labelRightFreeKick.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelRightFreeKick.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelRightFreeKick.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.labelLeftFreeKickText.BackColor = System.Drawing.Color.Transparent;
		this.labelLeftFreeKickText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeftFreeKickText.Location = new System.Drawing.Point(1213, 287);
		this.labelLeftFreeKickText.Name = "labelLeftFreeKickText";
		this.labelLeftFreeKickText.Size = new System.Drawing.Size(85, 16);
		this.labelLeftFreeKickText.TabIndex = 267;
		this.labelLeftFreeKickText.Text = "Left Free Kicks";
		this.labelLeftFreeKickText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelLeftFreeKick.AllowDrop = true;
		this.labelLeftFreeKick.BackColor = System.Drawing.Color.Transparent;
		this.labelLeftFreeKick.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelLeftFreeKick.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelLeftFreeKick.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelLeftFreeKick.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelLeftFreeKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeftFreeKick.Location = new System.Drawing.Point(1213, 303);
		this.labelLeftFreeKick.Name = "labelLeftFreeKick";
		this.labelLeftFreeKick.Size = new System.Drawing.Size(85, 38);
		this.labelLeftFreeKick.TabIndex = 266;
		this.labelLeftFreeKick.Text = "Name";
		this.labelLeftFreeKick.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelLeftFreeKick.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelLeftFreeKick.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.groupFormation.Controls.Add(this.buttonCreateNewFormation);
		this.groupFormation.Controls.Add(this.labelTeamFormationName);
		this.groupFormation.Controls.Add(this.comboGenericFormations);
		this.groupFormation.Controls.Add(this.radioUseSpecificFormation);
		this.groupFormation.Controls.Add(this.radioUseGenericFormation);
		this.groupFormation.Location = new System.Drawing.Point(732, 493);
		this.groupFormation.Name = "groupFormation";
		this.groupFormation.Size = new System.Drawing.Size(232, 128);
		this.groupFormation.TabIndex = 265;
		this.groupFormation.TabStop = false;
		this.groupFormation.Text = "Formation";
		this.buttonCreateNewFormation.Enabled = false;
		this.buttonCreateNewFormation.Image = (System.Drawing.Image)resources.GetObject("buttonCreateNewFormation.Image");
		this.buttonCreateNewFormation.Location = new System.Drawing.Point(100, 37);
		this.buttonCreateNewFormation.Name = "buttonCreateNewFormation";
		this.buttonCreateNewFormation.Size = new System.Drawing.Size(28, 24);
		this.buttonCreateNewFormation.TabIndex = 131;
		this.buttonCreateNewFormation.TabStop = false;
		this.buttonCreateNewFormation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.buttonCreateNewFormation.UseVisualStyleBackColor = true;
		this.buttonCreateNewFormation.Click += new System.EventHandler(buttonCreateNewFormation_Click);
		this.labelTeamFormationName.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelTeamFormationName.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelTeamFormationName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTeamFormationName.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelTeamFormationName.Location = new System.Drawing.Point(3, 16);
		this.labelTeamFormationName.Name = "labelTeamFormationName";
		this.labelTeamFormationName.Size = new System.Drawing.Size(226, 13);
		this.labelTeamFormationName.TabIndex = 130;
		this.labelTeamFormationName.Text = "Formation Name";
		this.labelTeamFormationName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelTeamFormationName.DoubleClick += new System.EventHandler(labelTeamFormationName_DoubleClick);
		this.comboGenericFormations.FormattingEnabled = true;
		this.comboGenericFormations.Location = new System.Drawing.Point(11, 89);
		this.comboGenericFormations.Name = "comboGenericFormations";
		this.comboGenericFormations.Size = new System.Drawing.Size(211, 21);
		this.comboGenericFormations.TabIndex = 129;
		this.comboGenericFormations.Visible = false;
		this.comboGenericFormations.SelectedIndexChanged += new System.EventHandler(comboGenericFormations_SelectedIndexChanged);
		this.radioUseSpecificFormation.AutoSize = true;
		this.radioUseSpecificFormation.Checked = true;
		this.radioUseSpecificFormation.Location = new System.Drawing.Point(6, 37);
		this.radioUseSpecificFormation.Name = "radioUseSpecificFormation";
		this.radioUseSpecificFormation.Size = new System.Drawing.Size(112, 17);
		this.radioUseSpecificFormation.TabIndex = 128;
		this.radioUseSpecificFormation.TabStop = true;
		this.radioUseSpecificFormation.Text = "Specific Formation";
		this.radioUseSpecificFormation.UseVisualStyleBackColor = true;
		this.radioUseSpecificFormation.Visible = false;
		this.radioUseSpecificFormation.CheckedChanged += new System.EventHandler(radioUseSpecificFormation_CheckedChanged);
		this.radioUseGenericFormation.AutoSize = true;
		this.radioUseGenericFormation.Enabled = false;
		this.radioUseGenericFormation.Location = new System.Drawing.Point(6, 55);
		this.radioUseGenericFormation.Name = "radioUseGenericFormation";
		this.radioUseGenericFormation.Size = new System.Drawing.Size(108, 17);
		this.radioUseGenericFormation.TabIndex = 127;
		this.radioUseGenericFormation.Text = "Generic formation";
		this.toolTip.SetToolTip(this.radioUseGenericFormation, "A Team cannot have a generic formation.");
		this.radioUseGenericFormation.UseVisualStyleBackColor = true;
		this.radioUseGenericFormation.Visible = false;
		this.radioUseGenericFormation.CheckedChanged += new System.EventHandler(radioUseGenericFormation_CheckedChanged);
		this.labelLongKick.AllowDrop = true;
		this.labelLongKick.BackColor = System.Drawing.Color.Transparent;
		this.labelLongKick.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelLongKick.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelLongKick.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelLongKick.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelLongKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLongKick.Location = new System.Drawing.Point(1213, 247);
		this.labelLongKick.Name = "labelLongKick";
		this.labelLongKick.Size = new System.Drawing.Size(85, 38);
		this.labelLongKick.TabIndex = 264;
		this.labelLongKick.Text = "Name";
		this.labelLongKick.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelLongKick.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelLongKick.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.labelLomgKickText.BackColor = System.Drawing.Color.Transparent;
		this.labelLomgKickText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLomgKickText.Location = new System.Drawing.Point(1213, 231);
		this.labelLomgKickText.Name = "labelLomgKickText";
		this.labelLomgKickText.Size = new System.Drawing.Size(85, 16);
		this.labelLomgKickText.TabIndex = 263;
		this.labelLomgKickText.Text = "Long Kicks";
		this.labelLomgKickText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelRightCornerText.BackColor = System.Drawing.Color.Transparent;
		this.labelRightCornerText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRightCornerText.Location = new System.Drawing.Point(1213, 118);
		this.labelRightCornerText.Name = "labelRightCornerText";
		this.labelRightCornerText.Size = new System.Drawing.Size(85, 16);
		this.labelRightCornerText.TabIndex = 262;
		this.labelRightCornerText.Text = "Right Corner";
		this.labelRightCornerText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelCaptainTetx.BackColor = System.Drawing.Color.Transparent;
		this.labelCaptainTetx.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCaptainTetx.Location = new System.Drawing.Point(1213, 2);
		this.labelCaptainTetx.Name = "labelCaptainTetx";
		this.labelCaptainTetx.Size = new System.Drawing.Size(85, 16);
		this.labelCaptainTetx.TabIndex = 253;
		this.labelCaptainTetx.Text = "Captain";
		this.labelCaptainTetx.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelLeftCornertext.BackColor = System.Drawing.Color.Transparent;
		this.labelLeftCornertext.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeftCornertext.Location = new System.Drawing.Point(1213, 60);
		this.labelLeftCornertext.Name = "labelLeftCornertext";
		this.labelLeftCornertext.Size = new System.Drawing.Size(85, 16);
		this.labelLeftCornertext.TabIndex = 261;
		this.labelLeftCornertext.Text = "Left Corner";
		this.labelLeftCornertext.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelRightCorner.AllowDrop = true;
		this.labelRightCorner.BackColor = System.Drawing.Color.Transparent;
		this.labelRightCorner.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelRightCorner.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelRightCorner.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelRightCorner.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelRightCorner.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRightCorner.Location = new System.Drawing.Point(1213, 134);
		this.labelRightCorner.Name = "labelRightCorner";
		this.labelRightCorner.Size = new System.Drawing.Size(85, 38);
		this.labelRightCorner.TabIndex = 258;
		this.labelRightCorner.Text = "Name";
		this.labelRightCorner.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelRightCorner.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelRightCorner.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.labelCaptain.AllowDrop = true;
		this.labelCaptain.BackColor = System.Drawing.Color.Transparent;
		this.labelCaptain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelCaptain.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelCaptain.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelCaptain.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelCaptain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCaptain.Location = new System.Drawing.Point(1213, 18);
		this.labelCaptain.Name = "labelCaptain";
		this.labelCaptain.Size = new System.Drawing.Size(85, 38);
		this.labelCaptain.TabIndex = 254;
		this.labelCaptain.Text = "Name";
		this.labelCaptain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelCaptain.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelCaptain.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.labelLeftCorner.AllowDrop = true;
		this.labelLeftCorner.BackColor = System.Drawing.Color.Transparent;
		this.labelLeftCorner.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelLeftCorner.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelLeftCorner.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelLeftCorner.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelLeftCorner.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeftCorner.Location = new System.Drawing.Point(1213, 76);
		this.labelLeftCorner.Name = "labelLeftCorner";
		this.labelLeftCorner.Size = new System.Drawing.Size(85, 38);
		this.labelLeftCorner.TabIndex = 257;
		this.labelLeftCorner.Text = "Name";
		this.labelLeftCorner.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelLeftCorner.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelLeftCorner.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.labelFreeKickText.BackColor = System.Drawing.Color.Transparent;
		this.labelFreeKickText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFreeKickText.Location = new System.Drawing.Point(1213, 401);
		this.labelFreeKickText.Name = "labelFreeKickText";
		this.labelFreeKickText.Size = new System.Drawing.Size(85, 16);
		this.labelFreeKickText.TabIndex = 260;
		this.labelFreeKickText.Text = "Free Kicks";
		this.labelFreeKickText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelFreeKickText.Visible = false;
		this.labelPenaltyText.BackColor = System.Drawing.Color.Transparent;
		this.labelPenaltyText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPenaltyText.Location = new System.Drawing.Point(1213, 175);
		this.labelPenaltyText.Name = "labelPenaltyText";
		this.labelPenaltyText.Size = new System.Drawing.Size(85, 16);
		this.labelPenaltyText.TabIndex = 259;
		this.labelPenaltyText.Text = "Penalty";
		this.labelPenaltyText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPenalty.AllowDrop = true;
		this.labelPenalty.BackColor = System.Drawing.Color.Transparent;
		this.labelPenalty.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelPenalty.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelPenalty.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPenalty.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelPenalty.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPenalty.Location = new System.Drawing.Point(1213, 191);
		this.labelPenalty.Name = "labelPenalty";
		this.labelPenalty.Size = new System.Drawing.Size(85, 38);
		this.labelPenalty.TabIndex = 255;
		this.labelPenalty.Text = "Name";
		this.labelPenalty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPenalty.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelPenalty.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.labelFreeKick.AllowDrop = true;
		this.labelFreeKick.BackColor = System.Drawing.Color.Transparent;
		this.labelFreeKick.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelFreeKick.Cursor = System.Windows.Forms.Cursors.Arrow;
		this.labelFreeKick.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelFreeKick.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelFreeKick.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFreeKick.Location = new System.Drawing.Point(1214, 417);
		this.labelFreeKick.Name = "labelFreeKick";
		this.labelFreeKick.Size = new System.Drawing.Size(85, 38);
		this.labelFreeKick.TabIndex = 256;
		this.labelFreeKick.Text = "Name";
		this.labelFreeKick.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelFreeKick.Visible = false;
		this.labelFreeKick.DragDrop += new System.Windows.Forms.DragEventHandler(labelSpecial_DragDrop);
		this.labelFreeKick.DragEnter += new System.Windows.Forms.DragEventHandler(labelSpecial_DragEnter);
		this.panel1.BackColor = System.Drawing.SystemColors.Control;
		this.panel1.BackgroundImage = (System.Drawing.Image)resources.GetObject("panel1.BackgroundImage");
		this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel1.Controls.Add(this.labelPos33U);
		this.panel1.Controls.Add(this.labelPos33T);
		this.panel1.Controls.Add(this.labelPos33S);
		this.panel1.Controls.Add(this.labelPos33R);
		this.panel1.Controls.Add(this.labelPos33Q);
		this.panel1.Controls.Add(this.labelPos33O);
		this.panel1.Controls.Add(this.labelPos33P);
		this.panel1.Controls.Add(this.labelPos33N);
		this.panel1.Controls.Add(this.labelPos33M);
		this.panel1.Controls.Add(this.labelPos33L);
		this.panel1.Controls.Add(this.labelPos33K);
		this.panel1.Controls.Add(this.labelPos33J);
		this.panel1.Controls.Add(this.labelPos33H);
		this.panel1.Controls.Add(this.labelPos33I);
		this.panel1.Controls.Add(this.labelPos33G);
		this.panel1.Controls.Add(this.labelPos33F);
		this.panel1.Controls.Add(this.labelPos33E);
		this.panel1.Controls.Add(this.labelPos33D);
		this.panel1.Controls.Add(this.labelPos33C);
		this.panel1.Controls.Add(this.labelPos33A);
		this.panel1.Controls.Add(this.labelPos33B);
		this.panel1.Controls.Add(this.labelPos32G);
		this.panel1.Controls.Add(this.labelPos32F);
		this.panel1.Controls.Add(this.labelPos32E);
		this.panel1.Controls.Add(this.labelPos32D);
		this.panel1.Controls.Add(this.labelPos32C);
		this.panel1.Controls.Add(this.labelPos32A);
		this.panel1.Controls.Add(this.labelPos32B);
		this.panel1.Controls.Add(this.labelPos26);
		this.panel1.Controls.Add(this.labelPos27);
		this.panel1.Controls.Add(this.labelPos21);
		this.panel1.Controls.Add(this.labelPos22);
		this.panel1.Controls.Add(this.labelPos23);
		this.panel1.Controls.Add(this.labelPos24);
		this.panel1.Controls.Add(this.labelPos25);
		this.panel1.Controls.Add(this.labelPos14);
		this.panel1.Controls.Add(this.labelPos15);
		this.panel1.Controls.Add(this.labelPos16);
		this.panel1.Controls.Add(this.labelPos17);
		this.panel1.Controls.Add(this.labelPos18);
		this.panel1.Controls.Add(this.labelPos20);
		this.panel1.Controls.Add(this.labelPos19);
		this.panel1.Controls.Add(this.labelPos9);
		this.panel1.Controls.Add(this.labelPos10);
		this.panel1.Controls.Add(this.labelPos11);
		this.panel1.Controls.Add(this.labelPos12);
		this.panel1.Controls.Add(this.labelPos13);
		this.panel1.Controls.Add(this.labelPos2);
		this.panel1.Controls.Add(this.labelPos3);
		this.panel1.Controls.Add(this.labelPos4);
		this.panel1.Controls.Add(this.labelPos5);
		this.panel1.Controls.Add(this.labelPos6);
		this.panel1.Controls.Add(this.labelPos8);
		this.panel1.Controls.Add(this.labelPos7);
		this.panel1.Controls.Add(this.labelPos0);
		this.panel1.Controls.Add(this.labelPos1);
		this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.panel1.Location = new System.Drawing.Point(732, 3);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(477, 484);
		this.panel1.TabIndex = 150;
		this.labelPos33U.AllowDrop = true;
		this.labelPos33U.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33U.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33U.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33U.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33U.ForeColor = System.Drawing.Color.Black;
		this.labelPos33U.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33U.Location = new System.Drawing.Point(407, 440);
		this.labelPos33U.Name = "labelPos33U";
		this.labelPos33U.Size = new System.Drawing.Size(68, 40);
		this.labelPos33U.TabIndex = 59;
		this.labelPos33U.Text = "Tribune";
		this.labelPos33U.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33U.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33U.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33U.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33T.AllowDrop = true;
		this.labelPos33T.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33T.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33T.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33T.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33T.ForeColor = System.Drawing.Color.Black;
		this.labelPos33T.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33T.Location = new System.Drawing.Point(339, 440);
		this.labelPos33T.Name = "labelPos33T";
		this.labelPos33T.Size = new System.Drawing.Size(68, 40);
		this.labelPos33T.TabIndex = 58;
		this.labelPos33T.Text = "Tribune";
		this.labelPos33T.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33T.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33T.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33T.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33S.AllowDrop = true;
		this.labelPos33S.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33S.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33S.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33S.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33S.ForeColor = System.Drawing.Color.Black;
		this.labelPos33S.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33S.Location = new System.Drawing.Point(271, 440);
		this.labelPos33S.Name = "labelPos33S";
		this.labelPos33S.Size = new System.Drawing.Size(68, 40);
		this.labelPos33S.TabIndex = 57;
		this.labelPos33S.Text = "Tribune";
		this.labelPos33S.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33S.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33S.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33S.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33R.AllowDrop = true;
		this.labelPos33R.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33R.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33R.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33R.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33R.ForeColor = System.Drawing.Color.Black;
		this.labelPos33R.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33R.Location = new System.Drawing.Point(203, 440);
		this.labelPos33R.Name = "labelPos33R";
		this.labelPos33R.Size = new System.Drawing.Size(68, 40);
		this.labelPos33R.TabIndex = 56;
		this.labelPos33R.Text = "Tribune";
		this.labelPos33R.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33R.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33R.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33R.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33Q.AllowDrop = true;
		this.labelPos33Q.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33Q.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33Q.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33Q.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33Q.ForeColor = System.Drawing.Color.Black;
		this.labelPos33Q.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33Q.Location = new System.Drawing.Point(135, 440);
		this.labelPos33Q.Name = "labelPos33Q";
		this.labelPos33Q.Size = new System.Drawing.Size(68, 40);
		this.labelPos33Q.TabIndex = 55;
		this.labelPos33Q.Text = "Tribune";
		this.labelPos33Q.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33Q.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33Q.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33Q.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33O.AllowDrop = true;
		this.labelPos33O.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33O.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33O.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33O.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33O.ForeColor = System.Drawing.Color.Black;
		this.labelPos33O.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33O.Location = new System.Drawing.Point(-1, 440);
		this.labelPos33O.Name = "labelPos33O";
		this.labelPos33O.Size = new System.Drawing.Size(68, 40);
		this.labelPos33O.TabIndex = 54;
		this.labelPos33O.Text = "Tribune";
		this.labelPos33O.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33O.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33O.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33O.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33P.AllowDrop = true;
		this.labelPos33P.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33P.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33P.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33P.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33P.ForeColor = System.Drawing.Color.Black;
		this.labelPos33P.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33P.Location = new System.Drawing.Point(67, 440);
		this.labelPos33P.Name = "labelPos33P";
		this.labelPos33P.Size = new System.Drawing.Size(68, 40);
		this.labelPos33P.TabIndex = 53;
		this.labelPos33P.Text = "Tribune";
		this.labelPos33P.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33P.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33P.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33P.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33N.AllowDrop = true;
		this.labelPos33N.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33N.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33N.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33N.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33N.ForeColor = System.Drawing.Color.Black;
		this.labelPos33N.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33N.Location = new System.Drawing.Point(407, 397);
		this.labelPos33N.Name = "labelPos33N";
		this.labelPos33N.Size = new System.Drawing.Size(68, 40);
		this.labelPos33N.TabIndex = 52;
		this.labelPos33N.Text = "Tribune";
		this.labelPos33N.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33N.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33N.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33N.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33M.AllowDrop = true;
		this.labelPos33M.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33M.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33M.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33M.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33M.ForeColor = System.Drawing.Color.Black;
		this.labelPos33M.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33M.Location = new System.Drawing.Point(339, 397);
		this.labelPos33M.Name = "labelPos33M";
		this.labelPos33M.Size = new System.Drawing.Size(68, 40);
		this.labelPos33M.TabIndex = 51;
		this.labelPos33M.Text = "Tribune";
		this.labelPos33M.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33M.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33M.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33M.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33L.AllowDrop = true;
		this.labelPos33L.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33L.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33L.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33L.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33L.ForeColor = System.Drawing.Color.Black;
		this.labelPos33L.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33L.Location = new System.Drawing.Point(271, 397);
		this.labelPos33L.Name = "labelPos33L";
		this.labelPos33L.Size = new System.Drawing.Size(68, 40);
		this.labelPos33L.TabIndex = 50;
		this.labelPos33L.Text = "Tribune";
		this.labelPos33L.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33L.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33L.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33L.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33K.AllowDrop = true;
		this.labelPos33K.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33K.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33K.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33K.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33K.ForeColor = System.Drawing.Color.Black;
		this.labelPos33K.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33K.Location = new System.Drawing.Point(203, 397);
		this.labelPos33K.Name = "labelPos33K";
		this.labelPos33K.Size = new System.Drawing.Size(68, 40);
		this.labelPos33K.TabIndex = 49;
		this.labelPos33K.Text = "Tribune";
		this.labelPos33K.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33K.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33K.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33K.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33J.AllowDrop = true;
		this.labelPos33J.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33J.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33J.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33J.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33J.ForeColor = System.Drawing.Color.Black;
		this.labelPos33J.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33J.Location = new System.Drawing.Point(135, 397);
		this.labelPos33J.Name = "labelPos33J";
		this.labelPos33J.Size = new System.Drawing.Size(68, 40);
		this.labelPos33J.TabIndex = 48;
		this.labelPos33J.Text = "Tribune";
		this.labelPos33J.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33J.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33J.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33J.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33H.AllowDrop = true;
		this.labelPos33H.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33H.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33H.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33H.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33H.ForeColor = System.Drawing.Color.Black;
		this.labelPos33H.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33H.Location = new System.Drawing.Point(-1, 397);
		this.labelPos33H.Name = "labelPos33H";
		this.labelPos33H.Size = new System.Drawing.Size(68, 40);
		this.labelPos33H.TabIndex = 47;
		this.labelPos33H.Text = "Tribune";
		this.labelPos33H.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33H.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33H.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33H.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33I.AllowDrop = true;
		this.labelPos33I.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33I.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33I.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33I.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33I.ForeColor = System.Drawing.Color.Black;
		this.labelPos33I.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33I.Location = new System.Drawing.Point(67, 397);
		this.labelPos33I.Name = "labelPos33I";
		this.labelPos33I.Size = new System.Drawing.Size(68, 40);
		this.labelPos33I.TabIndex = 46;
		this.labelPos33I.Text = "Tribune";
		this.labelPos33I.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33I.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33I.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33I.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33G.AllowDrop = true;
		this.labelPos33G.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33G.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33G.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33G.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33G.ForeColor = System.Drawing.Color.Black;
		this.labelPos33G.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33G.Location = new System.Drawing.Point(407, 354);
		this.labelPos33G.Name = "labelPos33G";
		this.labelPos33G.Size = new System.Drawing.Size(68, 40);
		this.labelPos33G.TabIndex = 45;
		this.labelPos33G.Text = "Tribune";
		this.labelPos33G.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33G.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33G.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33G.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33F.AllowDrop = true;
		this.labelPos33F.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33F.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33F.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33F.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33F.ForeColor = System.Drawing.Color.Black;
		this.labelPos33F.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33F.Location = new System.Drawing.Point(339, 354);
		this.labelPos33F.Name = "labelPos33F";
		this.labelPos33F.Size = new System.Drawing.Size(68, 40);
		this.labelPos33F.TabIndex = 44;
		this.labelPos33F.Text = "Tribune";
		this.labelPos33F.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33F.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33F.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33F.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33E.AllowDrop = true;
		this.labelPos33E.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33E.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33E.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33E.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33E.ForeColor = System.Drawing.Color.Black;
		this.labelPos33E.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33E.Location = new System.Drawing.Point(271, 354);
		this.labelPos33E.Name = "labelPos33E";
		this.labelPos33E.Size = new System.Drawing.Size(68, 40);
		this.labelPos33E.TabIndex = 43;
		this.labelPos33E.Text = "Tribune";
		this.labelPos33E.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33E.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33E.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33E.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33D.AllowDrop = true;
		this.labelPos33D.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33D.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33D.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33D.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33D.ForeColor = System.Drawing.Color.Black;
		this.labelPos33D.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33D.Location = new System.Drawing.Point(203, 354);
		this.labelPos33D.Name = "labelPos33D";
		this.labelPos33D.Size = new System.Drawing.Size(68, 40);
		this.labelPos33D.TabIndex = 42;
		this.labelPos33D.Text = "Tribune";
		this.labelPos33D.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33D.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33D.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33D.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33C.AllowDrop = true;
		this.labelPos33C.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33C.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33C.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33C.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33C.ForeColor = System.Drawing.Color.Black;
		this.labelPos33C.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33C.Location = new System.Drawing.Point(135, 354);
		this.labelPos33C.Name = "labelPos33C";
		this.labelPos33C.Size = new System.Drawing.Size(68, 40);
		this.labelPos33C.TabIndex = 41;
		this.labelPos33C.Text = "Tribune";
		this.labelPos33C.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33C.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33C.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33C.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33A.AllowDrop = true;
		this.labelPos33A.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33A.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33A.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33A.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33A.ForeColor = System.Drawing.Color.Black;
		this.labelPos33A.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33A.Location = new System.Drawing.Point(-1, 354);
		this.labelPos33A.Name = "labelPos33A";
		this.labelPos33A.Size = new System.Drawing.Size(68, 40);
		this.labelPos33A.TabIndex = 40;
		this.labelPos33A.Text = "Tribune";
		this.labelPos33A.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33A.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33A.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33A.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos33B.AllowDrop = true;
		this.labelPos33B.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos33B.BackColor = System.Drawing.Color.Transparent;
		this.labelPos33B.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos33B.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos33B.ForeColor = System.Drawing.Color.Black;
		this.labelPos33B.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos33B.Location = new System.Drawing.Point(67, 354);
		this.labelPos33B.Name = "labelPos33B";
		this.labelPos33B.Size = new System.Drawing.Size(68, 40);
		this.labelPos33B.TabIndex = 39;
		this.labelPos33B.Text = "Tribune";
		this.labelPos33B.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos33B.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos33B.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos33B.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos32G.AllowDrop = true;
		this.labelPos32G.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos32G.BackColor = System.Drawing.Color.Transparent;
		this.labelPos32G.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos32G.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos32G.ForeColor = System.Drawing.Color.Black;
		this.labelPos32G.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos32G.Location = new System.Drawing.Point(407, 311);
		this.labelPos32G.Name = "labelPos32G";
		this.labelPos32G.Size = new System.Drawing.Size(68, 40);
		this.labelPos32G.TabIndex = 38;
		this.labelPos32G.Text = "Bench";
		this.labelPos32G.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos32G.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos32G.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos32G.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos32F.AllowDrop = true;
		this.labelPos32F.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos32F.BackColor = System.Drawing.Color.Transparent;
		this.labelPos32F.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos32F.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos32F.ForeColor = System.Drawing.Color.Black;
		this.labelPos32F.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos32F.Location = new System.Drawing.Point(339, 311);
		this.labelPos32F.Name = "labelPos32F";
		this.labelPos32F.Size = new System.Drawing.Size(68, 40);
		this.labelPos32F.TabIndex = 37;
		this.labelPos32F.Text = "Bench";
		this.labelPos32F.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos32F.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos32F.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos32F.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos32E.AllowDrop = true;
		this.labelPos32E.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos32E.BackColor = System.Drawing.Color.Transparent;
		this.labelPos32E.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos32E.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos32E.ForeColor = System.Drawing.Color.Black;
		this.labelPos32E.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos32E.Location = new System.Drawing.Point(271, 311);
		this.labelPos32E.Name = "labelPos32E";
		this.labelPos32E.Size = new System.Drawing.Size(68, 40);
		this.labelPos32E.TabIndex = 36;
		this.labelPos32E.Text = "Bench";
		this.labelPos32E.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos32E.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos32E.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos32E.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos32D.AllowDrop = true;
		this.labelPos32D.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos32D.BackColor = System.Drawing.Color.Transparent;
		this.labelPos32D.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos32D.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos32D.ForeColor = System.Drawing.Color.Black;
		this.labelPos32D.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos32D.Location = new System.Drawing.Point(203, 311);
		this.labelPos32D.Name = "labelPos32D";
		this.labelPos32D.Size = new System.Drawing.Size(68, 40);
		this.labelPos32D.TabIndex = 35;
		this.labelPos32D.Text = "Bench";
		this.labelPos32D.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos32D.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos32D.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos32D.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos32C.AllowDrop = true;
		this.labelPos32C.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos32C.BackColor = System.Drawing.Color.Transparent;
		this.labelPos32C.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos32C.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos32C.ForeColor = System.Drawing.Color.Black;
		this.labelPos32C.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos32C.Location = new System.Drawing.Point(135, 311);
		this.labelPos32C.Name = "labelPos32C";
		this.labelPos32C.Size = new System.Drawing.Size(68, 40);
		this.labelPos32C.TabIndex = 34;
		this.labelPos32C.Text = "Bench";
		this.labelPos32C.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos32C.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos32C.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos32C.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos32A.AllowDrop = true;
		this.labelPos32A.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos32A.BackColor = System.Drawing.Color.Transparent;
		this.labelPos32A.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos32A.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos32A.ForeColor = System.Drawing.Color.Black;
		this.labelPos32A.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos32A.Location = new System.Drawing.Point(-1, 311);
		this.labelPos32A.Name = "labelPos32A";
		this.labelPos32A.Size = new System.Drawing.Size(68, 40);
		this.labelPos32A.TabIndex = 33;
		this.labelPos32A.Text = "Bench";
		this.labelPos32A.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos32A.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos32A.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos32A.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos32B.AllowDrop = true;
		this.labelPos32B.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos32B.BackColor = System.Drawing.Color.Transparent;
		this.labelPos32B.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos32B.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos32B.ForeColor = System.Drawing.Color.Black;
		this.labelPos32B.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos32B.Location = new System.Drawing.Point(67, 311);
		this.labelPos32B.Name = "labelPos32B";
		this.labelPos32B.Size = new System.Drawing.Size(68, 40);
		this.labelPos32B.TabIndex = 32;
		this.labelPos32B.Text = "Bench";
		this.labelPos32B.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos32B.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos32B.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos32B.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos26.AllowDrop = true;
		this.labelPos26.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos26.BackColor = System.Drawing.Color.Transparent;
		this.labelPos26.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos26.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos26.ForeColor = System.Drawing.Color.Black;
		this.labelPos26.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos26.Location = new System.Drawing.Point(96, 2);
		this.labelPos26.Name = "labelPos26";
		this.labelPos26.Size = new System.Drawing.Size(95, 40);
		this.labelPos26.TabIndex = 28;
		this.labelPos26.Text = "LS";
		this.labelPos26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos26.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos26.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos26.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos27.AllowDrop = true;
		this.labelPos27.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos27.BackColor = System.Drawing.Color.Transparent;
		this.labelPos27.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos27.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos27.ForeColor = System.Drawing.Color.Black;
		this.labelPos27.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos27.Location = new System.Drawing.Point(21, 25);
		this.labelPos27.Name = "labelPos27";
		this.labelPos27.Size = new System.Drawing.Size(95, 40);
		this.labelPos27.TabIndex = 27;
		this.labelPos27.Text = "LW";
		this.labelPos27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos27.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos27.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos27.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos21.AllowDrop = true;
		this.labelPos21.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos21.BackColor = System.Drawing.Color.Transparent;
		this.labelPos21.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos21.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos21.ForeColor = System.Drawing.Color.Black;
		this.labelPos21.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos21.Location = new System.Drawing.Point(189, 31);
		this.labelPos21.Name = "labelPos21";
		this.labelPos21.Size = new System.Drawing.Size(95, 40);
		this.labelPos21.TabIndex = 25;
		this.labelPos21.Text = "CF";
		this.labelPos21.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos21.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos21.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos21.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos22.AllowDrop = true;
		this.labelPos22.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos22.BackColor = System.Drawing.Color.Transparent;
		this.labelPos22.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos22.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos22.ForeColor = System.Drawing.Color.Black;
		this.labelPos22.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos22.Location = new System.Drawing.Point(96, 31);
		this.labelPos22.Name = "labelPos22";
		this.labelPos22.Size = new System.Drawing.Size(95, 40);
		this.labelPos22.TabIndex = 24;
		this.labelPos22.Text = "LF";
		this.labelPos22.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos22.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos22.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos22.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos23.AllowDrop = true;
		this.labelPos23.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos23.BackColor = System.Drawing.Color.Transparent;
		this.labelPos23.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos23.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos23.ForeColor = System.Drawing.Color.Black;
		this.labelPos23.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos23.Location = new System.Drawing.Point(361, 25);
		this.labelPos23.Name = "labelPos23";
		this.labelPos23.Size = new System.Drawing.Size(95, 40);
		this.labelPos23.TabIndex = 23;
		this.labelPos23.Text = "RW";
		this.labelPos23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos23.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos23.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos23.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos24.AllowDrop = true;
		this.labelPos24.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos24.BackColor = System.Drawing.Color.Transparent;
		this.labelPos24.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos24.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos24.ForeColor = System.Drawing.Color.Black;
		this.labelPos24.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos24.Location = new System.Drawing.Point(286, 2);
		this.labelPos24.Name = "labelPos24";
		this.labelPos24.Size = new System.Drawing.Size(95, 40);
		this.labelPos24.TabIndex = 22;
		this.labelPos24.Text = "RS";
		this.labelPos24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos24.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos24.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos24.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos25.AllowDrop = true;
		this.labelPos25.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos25.BackColor = System.Drawing.Color.Transparent;
		this.labelPos25.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos25.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos25.ForeColor = System.Drawing.Color.Black;
		this.labelPos25.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos25.Location = new System.Drawing.Point(191, 2);
		this.labelPos25.Name = "labelPos25";
		this.labelPos25.Size = new System.Drawing.Size(95, 40);
		this.labelPos25.TabIndex = 21;
		this.labelPos25.Text = "ST";
		this.labelPos25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos25.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos25.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos25.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos14.AllowDrop = true;
		this.labelPos14.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos14.BackColor = System.Drawing.Color.Transparent;
		this.labelPos14.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos14.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos14.ForeColor = System.Drawing.Color.Black;
		this.labelPos14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos14.Location = new System.Drawing.Point(191, 102);
		this.labelPos14.Name = "labelPos14";
		this.labelPos14.Size = new System.Drawing.Size(95, 40);
		this.labelPos14.TabIndex = 20;
		this.labelPos14.Text = "CM";
		this.labelPos14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos14.Click += new System.EventHandler(labelPos14_Click);
		this.labelPos14.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos14.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos14.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos15.AllowDrop = true;
		this.labelPos15.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos15.BackColor = System.Drawing.Color.Transparent;
		this.labelPos15.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos15.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos15.ForeColor = System.Drawing.Color.Black;
		this.labelPos15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos15.Location = new System.Drawing.Point(101, 102);
		this.labelPos15.Name = "labelPos15";
		this.labelPos15.Size = new System.Drawing.Size(95, 40);
		this.labelPos15.TabIndex = 19;
		this.labelPos15.Text = "LCM";
		this.labelPos15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos15.Click += new System.EventHandler(labelPos15_Click);
		this.labelPos15.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos15.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos15.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos16.AllowDrop = true;
		this.labelPos16.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos16.BackColor = System.Drawing.Color.Transparent;
		this.labelPos16.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos16.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos16.ForeColor = System.Drawing.Color.Black;
		this.labelPos16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos16.Location = new System.Drawing.Point(11, 102);
		this.labelPos16.Name = "labelPos16";
		this.labelPos16.Size = new System.Drawing.Size(95, 40);
		this.labelPos16.TabIndex = 18;
		this.labelPos16.Text = "LM";
		this.labelPos16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos16.Click += new System.EventHandler(labelPos16_Click);
		this.labelPos16.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos16.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos16.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos17.AllowDrop = true;
		this.labelPos17.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos17.BackColor = System.Drawing.Color.Transparent;
		this.labelPos17.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos17.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos17.ForeColor = System.Drawing.Color.Black;
		this.labelPos17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos17.Location = new System.Drawing.Point(326, 62);
		this.labelPos17.Name = "labelPos17";
		this.labelPos17.Size = new System.Drawing.Size(95, 40);
		this.labelPos17.TabIndex = 17;
		this.labelPos17.Text = "RAM";
		this.labelPos17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos17.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos17.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos17.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos18.AllowDrop = true;
		this.labelPos18.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos18.BackColor = System.Drawing.Color.Transparent;
		this.labelPos18.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos18.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos18.ForeColor = System.Drawing.Color.Black;
		this.labelPos18.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos18.Location = new System.Drawing.Point(191, 62);
		this.labelPos18.Name = "labelPos18";
		this.labelPos18.Size = new System.Drawing.Size(95, 40);
		this.labelPos18.TabIndex = 16;
		this.labelPos18.Text = "CAM";
		this.labelPos18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos18.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos18.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos18.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos20.AllowDrop = true;
		this.labelPos20.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos20.BackColor = System.Drawing.Color.Transparent;
		this.labelPos20.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos20.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos20.ForeColor = System.Drawing.Color.Black;
		this.labelPos20.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos20.Location = new System.Drawing.Point(286, 31);
		this.labelPos20.Name = "labelPos20";
		this.labelPos20.Size = new System.Drawing.Size(95, 40);
		this.labelPos20.TabIndex = 15;
		this.labelPos20.Text = "RF";
		this.labelPos20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos20.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos20.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos20.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos19.AllowDrop = true;
		this.labelPos19.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos19.BackColor = System.Drawing.Color.Transparent;
		this.labelPos19.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos19.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos19.ForeColor = System.Drawing.Color.Black;
		this.labelPos19.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos19.Location = new System.Drawing.Point(56, 62);
		this.labelPos19.Name = "labelPos19";
		this.labelPos19.Size = new System.Drawing.Size(95, 40);
		this.labelPos19.TabIndex = 14;
		this.labelPos19.Text = "LAM";
		this.labelPos19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos19.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos19.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos19.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos9.AllowDrop = true;
		this.labelPos9.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos9.BackColor = System.Drawing.Color.Transparent;
		this.labelPos9.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos9.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos9.ForeColor = System.Drawing.Color.Black;
		this.labelPos9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos9.Location = new System.Drawing.Point(286, 149);
		this.labelPos9.Name = "labelPos9";
		this.labelPos9.Size = new System.Drawing.Size(95, 40);
		this.labelPos9.TabIndex = 13;
		this.labelPos9.Text = "RDM";
		this.labelPos9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos9.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos9.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos9.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos10.AllowDrop = true;
		this.labelPos10.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos10.BackColor = System.Drawing.Color.Transparent;
		this.labelPos10.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos10.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos10.ForeColor = System.Drawing.Color.Black;
		this.labelPos10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos10.Location = new System.Drawing.Point(191, 149);
		this.labelPos10.Name = "labelPos10";
		this.labelPos10.Size = new System.Drawing.Size(95, 40);
		this.labelPos10.TabIndex = 12;
		this.labelPos10.Text = "CDM";
		this.labelPos10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos10.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos10.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos10.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos11.AllowDrop = true;
		this.labelPos11.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos11.BackColor = System.Drawing.Color.Transparent;
		this.labelPos11.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos11.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos11.ForeColor = System.Drawing.Color.Black;
		this.labelPos11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos11.Location = new System.Drawing.Point(96, 149);
		this.labelPos11.Name = "labelPos11";
		this.labelPos11.Size = new System.Drawing.Size(95, 40);
		this.labelPos11.TabIndex = 11;
		this.labelPos11.Text = "LDM";
		this.labelPos11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos11.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos11.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos11.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos12.AllowDrop = true;
		this.labelPos12.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos12.BackColor = System.Drawing.Color.Transparent;
		this.labelPos12.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos12.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos12.ForeColor = System.Drawing.Color.Black;
		this.labelPos12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos12.Location = new System.Drawing.Point(371, 102);
		this.labelPos12.Name = "labelPos12";
		this.labelPos12.Size = new System.Drawing.Size(95, 40);
		this.labelPos12.TabIndex = 10;
		this.labelPos12.Text = "RM";
		this.labelPos12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos12.Click += new System.EventHandler(labelPos12_Click);
		this.labelPos12.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos12.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos12.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos13.AllowDrop = true;
		this.labelPos13.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos13.BackColor = System.Drawing.Color.Transparent;
		this.labelPos13.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos13.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos13.ForeColor = System.Drawing.Color.Black;
		this.labelPos13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos13.Location = new System.Drawing.Point(281, 102);
		this.labelPos13.Name = "labelPos13";
		this.labelPos13.Size = new System.Drawing.Size(95, 40);
		this.labelPos13.TabIndex = 9;
		this.labelPos13.Text = "RCM";
		this.labelPos13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos13.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos13.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos13.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos2.AllowDrop = true;
		this.labelPos2.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos2.BackColor = System.Drawing.Color.Transparent;
		this.labelPos2.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos2.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos2.ForeColor = System.Drawing.Color.Black;
		this.labelPos2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos2.Location = new System.Drawing.Point(381, 181);
		this.labelPos2.Name = "labelPos2";
		this.labelPos2.Size = new System.Drawing.Size(95, 40);
		this.labelPos2.TabIndex = 8;
		this.labelPos2.Text = "RWB";
		this.labelPos2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos2.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos2.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos2.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos3.AllowDrop = true;
		this.labelPos3.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos3.BackColor = System.Drawing.Color.Transparent;
		this.labelPos3.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos3.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos3.ForeColor = System.Drawing.Color.Black;
		this.labelPos3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos3.Location = new System.Drawing.Point(386, 215);
		this.labelPos3.Name = "labelPos3";
		this.labelPos3.Size = new System.Drawing.Size(95, 40);
		this.labelPos3.TabIndex = 7;
		this.labelPos3.Text = "RB";
		this.labelPos3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos3.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos3.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos3.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos4.AllowDrop = true;
		this.labelPos4.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos4.BackColor = System.Drawing.Color.Transparent;
		this.labelPos4.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos4.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos4.ForeColor = System.Drawing.Color.Black;
		this.labelPos4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos4.Location = new System.Drawing.Point(286, 225);
		this.labelPos4.Name = "labelPos4";
		this.labelPos4.Size = new System.Drawing.Size(95, 40);
		this.labelPos4.TabIndex = 6;
		this.labelPos4.Text = "RCB";
		this.labelPos4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos4.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos4.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos4.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos5.AllowDrop = true;
		this.labelPos5.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos5.BackColor = System.Drawing.Color.Transparent;
		this.labelPos5.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos5.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos5.ForeColor = System.Drawing.Color.Black;
		this.labelPos5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos5.Location = new System.Drawing.Point(191, 225);
		this.labelPos5.Name = "labelPos5";
		this.labelPos5.Size = new System.Drawing.Size(95, 40);
		this.labelPos5.TabIndex = 5;
		this.labelPos5.Text = "CB";
		this.labelPos5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos5.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos5.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos5.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos6.AllowDrop = true;
		this.labelPos6.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos6.BackColor = System.Drawing.Color.Transparent;
		this.labelPos6.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos6.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos6.ForeColor = System.Drawing.Color.Black;
		this.labelPos6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos6.Location = new System.Drawing.Point(96, 225);
		this.labelPos6.Name = "labelPos6";
		this.labelPos6.Size = new System.Drawing.Size(95, 40);
		this.labelPos6.TabIndex = 4;
		this.labelPos6.Text = "LCB";
		this.labelPos6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos6.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos6.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos6.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos8.AllowDrop = true;
		this.labelPos8.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos8.BackColor = System.Drawing.Color.Transparent;
		this.labelPos8.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos8.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos8.ForeColor = System.Drawing.Color.Black;
		this.labelPos8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos8.Location = new System.Drawing.Point(1, 180);
		this.labelPos8.Name = "labelPos8";
		this.labelPos8.Size = new System.Drawing.Size(95, 40);
		this.labelPos8.TabIndex = 3;
		this.labelPos8.Text = "LWB";
		this.labelPos8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos8.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos8.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos8.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos7.AllowDrop = true;
		this.labelPos7.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos7.BackColor = System.Drawing.Color.Transparent;
		this.labelPos7.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos7.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos7.ForeColor = System.Drawing.Color.Black;
		this.labelPos7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos7.Location = new System.Drawing.Point(1, 214);
		this.labelPos7.Name = "labelPos7";
		this.labelPos7.Size = new System.Drawing.Size(95, 40);
		this.labelPos7.TabIndex = 2;
		this.labelPos7.Text = "LB";
		this.labelPos7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos7.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos7.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos7.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos0.AllowDrop = true;
		this.labelPos0.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelPos0.BackColor = System.Drawing.Color.Transparent;
		this.labelPos0.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos0.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos0.ForeColor = System.Drawing.Color.Black;
		this.labelPos0.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos0.Location = new System.Drawing.Point(191, 260);
		this.labelPos0.Name = "labelPos0";
		this.labelPos0.Size = new System.Drawing.Size(95, 40);
		this.labelPos0.TabIndex = 0;
		this.labelPos0.Text = "GK";
		this.labelPos0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos0.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos0.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos0.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.labelPos1.AllowDrop = true;
		this.labelPos1.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.labelPos1.BackColor = System.Drawing.Color.Transparent;
		this.labelPos1.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelPos1.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelPos1.ForeColor = System.Drawing.Color.Black;
		this.labelPos1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPos1.Location = new System.Drawing.Point(191, 227);
		this.labelPos1.Name = "labelPos1";
		this.labelPos1.Size = new System.Drawing.Size(95, 40);
		this.labelPos1.TabIndex = 1;
		this.labelPos1.Text = "SW";
		this.labelPos1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPos1.DragDrop += new System.Windows.Forms.DragEventHandler(labelPos_DragDrop);
		this.labelPos1.DragEnter += new System.Windows.Forms.DragEventHandler(labelPos_DragEnter);
		this.labelPos1.MouseDown += new System.Windows.Forms.MouseEventHandler(labelPos_MouseDown);
		this.groupAvailablePlayers.BackColor = System.Drawing.SystemColors.Control;
		this.groupAvailablePlayers.Controls.Add(this.listViewPlayersAvailable);
		this.groupAvailablePlayers.Controls.Add(this.panelAvailablePlayersTop);
		this.groupAvailablePlayers.Dock = System.Windows.Forms.DockStyle.Left;
		this.groupAvailablePlayers.Location = new System.Drawing.Point(388, 3);
		this.groupAvailablePlayers.Name = "groupAvailablePlayers";
		this.groupAvailablePlayers.Size = new System.Drawing.Size(341, 775);
		this.groupAvailablePlayers.TabIndex = 3;
		this.groupAvailablePlayers.TabStop = false;
		this.groupAvailablePlayers.Text = "Available Players";
		this.listViewPlayersAvailable.AllowColumnReorder = true;
		this.listViewPlayersAvailable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.columnHeader1, this.columnHeader2, this.columnHeader3, this.columnHeader4 });
		this.listViewPlayersAvailable.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewPlayersAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewPlayersAvailable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.listViewPlayersAvailable.FullRowSelect = true;
		this.listViewPlayersAvailable.GridLines = true;
		this.listViewPlayersAvailable.HideSelection = false;
		this.listViewPlayersAvailable.Location = new System.Drawing.Point(3, 231);
		this.listViewPlayersAvailable.MultiSelect = false;
		this.listViewPlayersAvailable.Name = "listViewPlayersAvailable";
		this.listViewPlayersAvailable.Size = new System.Drawing.Size(335, 541);
		this.listViewPlayersAvailable.TabIndex = 4;
		this.listViewPlayersAvailable.UseCompatibleStateImageBehavior = false;
		this.listViewPlayersAvailable.View = System.Windows.Forms.View.Details;
		this.listViewPlayersAvailable.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(listView_ColumnClick);
		this.listViewPlayersAvailable.SelectedIndexChanged += new System.EventHandler(listViewPlayersAvailable_SelectedIndexChanged);
		this.listViewPlayersAvailable.DoubleClick += new System.EventHandler(listViewPlayersAvailable_DoubleClick);
		this.columnHeader1.Text = "Surname";
		this.columnHeader1.Width = 108;
		this.columnHeader2.Text = "First Name";
		this.columnHeader2.Width = 108;
		this.columnHeader3.Text = "Role";
		this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnHeader3.Width = 42;
		this.columnHeader4.Text = "Avg.";
		this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnHeader4.Width = 42;
		this.panelAvailablePlayersTop.BackColor = System.Drawing.Color.Transparent;
		this.panelAvailablePlayersTop.Controls.Add(this.buttonDeletePlayer);
		this.panelAvailablePlayersTop.Controls.Add(this.buttonLoanFrom);
		this.panelAvailablePlayersTop.Controls.Add(this.label4);
		this.panelAvailablePlayersTop.Controls.Add(this.dateTransferPreset);
		this.panelAvailablePlayersTop.Controls.Add(this.buttonTransferFrom);
		this.panelAvailablePlayersTop.Controls.Add(this.pickUpAvailablePlayers);
		this.panelAvailablePlayersTop.Controls.Add(this.buttonCall);
		this.panelAvailablePlayersTop.Controls.Add(this.labelAvailablePlayerStars);
		this.panelAvailablePlayersTop.Controls.Add(this.pictureAvailablePlayer);
		this.panelAvailablePlayersTop.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAvailablePlayersTop.Location = new System.Drawing.Point(3, 16);
		this.panelAvailablePlayersTop.Name = "panelAvailablePlayersTop";
		this.panelAvailablePlayersTop.Size = new System.Drawing.Size(335, 215);
		this.panelAvailablePlayersTop.TabIndex = 149;
		this.buttonDeletePlayer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonDeletePlayer.Location = new System.Drawing.Point(79, 85);
		this.buttonDeletePlayer.Name = "buttonDeletePlayer";
		this.buttonDeletePlayer.Size = new System.Drawing.Size(68, 38);
		this.buttonDeletePlayer.TabIndex = 152;
		this.buttonDeletePlayer.Text = "Delete";
		this.buttonDeletePlayer.UseVisualStyleBackColor = true;
		this.buttonDeletePlayer.Click += new System.EventHandler(buttonDeletePlayer_Click);
		this.buttonLoanFrom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonLoanFrom.Location = new System.Drawing.Point(5, 47);
		this.buttonLoanFrom.Name = "buttonLoanFrom";
		this.buttonLoanFrom.Size = new System.Drawing.Size(68, 37);
		this.buttonLoanFrom.TabIndex = 151;
		this.buttonLoanFrom.Text = "   Loan    <<";
		this.buttonLoanFrom.UseVisualStyleBackColor = true;
		this.buttonLoanFrom.Click += new System.EventHandler(buttonLoanFrom_Click);
		this.label4.AutoSize = true;
		this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label4.Location = new System.Drawing.Point(15, 127);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(72, 13);
		this.label4.TabIndex = 150;
		this.label4.Text = "Transfer Date";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dateTransferPreset.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateTransferPreset.Location = new System.Drawing.Point(5, 144);
		this.dateTransferPreset.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
		this.dateTransferPreset.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateTransferPreset.Name = "dateTransferPreset";
		this.dateTransferPreset.Size = new System.Drawing.Size(92, 20);
		this.dateTransferPreset.TabIndex = 149;
		this.dateTransferPreset.Value = new System.DateTime(2020, 7, 1, 0, 0, 0, 0);
		this.buttonTransferFrom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonTransferFrom.Location = new System.Drawing.Point(5, 9);
		this.buttonTransferFrom.Name = "buttonTransferFrom";
		this.buttonTransferFrom.Size = new System.Drawing.Size(68, 37);
		this.buttonTransferFrom.TabIndex = 0;
		this.buttonTransferFrom.Text = "Transfer <<";
		this.buttonTransferFrom.UseVisualStyleBackColor = true;
		this.buttonTransferFrom.Click += new System.EventHandler(buttonTransferFrom_Click);
		this.pickUpAvailablePlayers.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpAvailablePlayers.CloneButtonEnabled = false;
		this.pickUpAvailablePlayers.CreateButtonEnabled = false;
		this.pickUpAvailablePlayers.CurrentIndex = 0;
		this.pickUpAvailablePlayers.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.pickUpAvailablePlayers.FilterByList = new string[5] { "All", "By Team", "By Country", "By Role", "Free Agent" };
		this.pickUpAvailablePlayers.FilterEnabled = true;
		this.pickUpAvailablePlayers.FilterValues = null;
		this.pickUpAvailablePlayers.Location = new System.Drawing.Point(0, 190);
		this.pickUpAvailablePlayers.MainSelectionEnabled = false;
		this.pickUpAvailablePlayers.Margin = new System.Windows.Forms.Padding(4);
		this.pickUpAvailablePlayers.Name = "pickUpAvailablePlayers";
		this.pickUpAvailablePlayers.ObjectList = null;
		this.pickUpAvailablePlayers.RefreshButtonEnabled = false;
		this.pickUpAvailablePlayers.RemoveButtonEnabled = false;
		this.pickUpAvailablePlayers.SearchEnabled = false;
		this.pickUpAvailablePlayers.Size = new System.Drawing.Size(335, 25);
		this.pickUpAvailablePlayers.TabIndex = 148;
		this.pickUpAvailablePlayers.WizardButtonEnabled = false;
		this.pickUpAvailablePlayers.YoungPlayersEnabled = false;
		this.buttonCall.Enabled = false;
		this.buttonCall.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonCall.Location = new System.Drawing.Point(5, 85);
		this.buttonCall.Name = "buttonCall";
		this.buttonCall.Size = new System.Drawing.Size(68, 38);
		this.buttonCall.TabIndex = 1;
		this.buttonCall.Text = "     Call       <<";
		this.buttonCall.UseVisualStyleBackColor = true;
		this.buttonCall.Click += new System.EventHandler(buttonCall_Click);
		this.labelAvailablePlayerStars.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelAvailablePlayerStars.ImageIndex = 9;
		this.labelAvailablePlayerStars.ImageList = this.imageListStars;
		this.labelAvailablePlayerStars.Location = new System.Drawing.Point(231, 142);
		this.labelAvailablePlayerStars.Name = "labelAvailablePlayerStars";
		this.labelAvailablePlayerStars.Size = new System.Drawing.Size(101, 20);
		this.labelAvailablePlayerStars.TabIndex = 147;
		this.labelAvailablePlayerStars.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.imageListStars.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListStars.ImageStream");
		this.imageListStars.TransparentColor = System.Drawing.Color.Fuchsia;
		this.imageListStars.Images.SetKeyName(0, "Stars_0_5.PNG");
		this.imageListStars.Images.SetKeyName(1, "Stars_1.PNG");
		this.imageListStars.Images.SetKeyName(2, "Stars_1_5.PNG");
		this.imageListStars.Images.SetKeyName(3, "Stars_2.PNG");
		this.imageListStars.Images.SetKeyName(4, "Stars_2_5.PNG");
		this.imageListStars.Images.SetKeyName(5, "Stars_3.PNG");
		this.imageListStars.Images.SetKeyName(6, "Stars_3_5.PNG");
		this.imageListStars.Images.SetKeyName(7, "Stars_4.PNG");
		this.imageListStars.Images.SetKeyName(8, "Stars_4_5.PNG");
		this.imageListStars.Images.SetKeyName(9, "Stars_5.PNG");
		this.pictureAvailablePlayer.Location = new System.Drawing.Point(164, 7);
		this.pictureAvailablePlayer.Name = "pictureAvailablePlayer";
		this.pictureAvailablePlayer.Size = new System.Drawing.Size(128, 128);
		this.pictureAvailablePlayer.TabIndex = 146;
		this.pictureAvailablePlayer.TabStop = false;
		this.groupTeamPlayers.BackColor = System.Drawing.SystemColors.Control;
		this.groupTeamPlayers.Controls.Add(this.listViewTeamPlayers);
		this.groupTeamPlayers.Controls.Add(this.panelTeamPlayersTop);
		this.groupTeamPlayers.Dock = System.Windows.Forms.DockStyle.Left;
		this.groupTeamPlayers.Location = new System.Drawing.Point(3, 3);
		this.groupTeamPlayers.Name = "groupTeamPlayers";
		this.groupTeamPlayers.Size = new System.Drawing.Size(385, 775);
		this.groupTeamPlayers.TabIndex = 0;
		this.groupTeamPlayers.TabStop = false;
		this.groupTeamPlayers.Text = "Team Players";
		this.listViewTeamPlayers.AllowDrop = true;
		this.listViewTeamPlayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[6] { this.columnRosterSurname, this.columnRosterFirstName, this.columnRosterYearContract, this.columnPreferredRole, this.columnAverageAttributes, this.columnRosterNum });
		this.listViewTeamPlayers.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewTeamPlayers.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewTeamPlayers.FullRowSelect = true;
		this.listViewTeamPlayers.GridLines = true;
		this.listViewTeamPlayers.HideSelection = false;
		this.listViewTeamPlayers.Location = new System.Drawing.Point(3, 231);
		this.listViewTeamPlayers.MultiSelect = false;
		this.listViewTeamPlayers.Name = "listViewTeamPlayers";
		this.listViewTeamPlayers.Size = new System.Drawing.Size(379, 541);
		this.listViewTeamPlayers.TabIndex = 8;
		this.listViewTeamPlayers.UseCompatibleStateImageBehavior = false;
		this.listViewTeamPlayers.View = System.Windows.Forms.View.Details;
		this.listViewTeamPlayers.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(listViewTeamPlayers_AfterLabelEdit);
		this.listViewTeamPlayers.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(listView_ColumnClick);
		this.listViewTeamPlayers.SelectedIndexChanged += new System.EventHandler(listViewTeamPlayers_SelectedIndexChanged);
		this.listViewTeamPlayers.DoubleClick += new System.EventHandler(listViewTeamPlayers_DoubleClick);
		this.listViewTeamPlayers.KeyPress += new System.Windows.Forms.KeyPressEventHandler(listViewTeamPlayers_KeyPress);
		this.columnRosterSurname.DisplayIndex = 1;
		this.columnRosterSurname.Text = "Surname";
		this.columnRosterSurname.Width = 90;
		this.columnRosterFirstName.DisplayIndex = 2;
		this.columnRosterFirstName.Text = "First Name";
		this.columnRosterFirstName.Width = 89;
		this.columnRosterYearContract.DisplayIndex = 3;
		this.columnRosterYearContract.Text = "Y.C.";
		this.columnRosterYearContract.Width = 42;
		this.columnPreferredRole.DisplayIndex = 4;
		this.columnPreferredRole.Text = "Role";
		this.columnPreferredRole.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnPreferredRole.Width = 44;
		this.columnAverageAttributes.DisplayIndex = 5;
		this.columnAverageAttributes.Text = "Overall";
		this.columnAverageAttributes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnAverageAttributes.Width = 52;
		this.columnRosterNum.DisplayIndex = 0;
		this.columnRosterNum.Text = "N.";
		this.columnRosterNum.Width = 25;
		this.panelTeamPlayersTop.Controls.Add(this.buttonTransferAll);
		this.panelTeamPlayersTop.Controls.Add(this.label5);
		this.panelTeamPlayersTop.Controls.Add(this.buttonPlusContract);
		this.panelTeamPlayersTop.Controls.Add(this.buttonMinusContract);
		this.panelTeamPlayersTop.Controls.Add(this.labelLoanedFrom);
		this.panelTeamPlayersTop.Controls.Add(this.comboTeamLoanedFrom);
		this.panelTeamPlayersTop.Controls.Add(this.label2);
		this.panelTeamPlayersTop.Controls.Add(this.dateLoanEnd);
		this.panelTeamPlayersTop.Controls.Add(this.labelLoanEnd);
		this.panelTeamPlayersTop.Controls.Add(this.buttonRosterLetFree);
		this.panelTeamPlayersTop.Controls.Add(this.buttonTransferPlayer);
		this.panelTeamPlayersTop.Controls.Add(this.checkIsLoan);
		this.panelTeamPlayersTop.Controls.Add(this.numericRosterYear);
		this.panelTeamPlayersTop.Controls.Add(this.buttonLoanTo);
		this.panelTeamPlayersTop.Controls.Add(this.dateJoiningDate);
		this.panelTeamPlayersTop.Controls.Add(this.viewer2DPhoto);
		this.panelTeamPlayersTop.Controls.Add(this.labelJoiningDate);
		this.panelTeamPlayersTop.Controls.Add(this.groupTeamPlayerTuning);
		this.panelTeamPlayersTop.Controls.Add(this.labelRosterName);
		this.panelTeamPlayersTop.Controls.Add(this.comboRosterNumber);
		this.panelTeamPlayersTop.Controls.Add(this.labelRosterNumber);
		this.panelTeamPlayersTop.Controls.Add(this.labelRosterNameFrom);
		this.panelTeamPlayersTop.Controls.Add(this.labelPreviousTeam);
		this.panelTeamPlayersTop.Controls.Add(this.comboTeamPrevious);
		this.panelTeamPlayersTop.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTeamPlayersTop.Location = new System.Drawing.Point(3, 16);
		this.panelTeamPlayersTop.Name = "panelTeamPlayersTop";
		this.panelTeamPlayersTop.Size = new System.Drawing.Size(379, 215);
		this.panelTeamPlayersTop.TabIndex = 132;
		this.toolTip.SetToolTip(this.panelTeamPlayersTop, "Add 1 year of contract to all player");
		this.buttonTransferAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonTransferAll.Location = new System.Drawing.Point(305, 41);
		this.buttonTransferAll.Name = "buttonTransferAll";
		this.buttonTransferAll.Size = new System.Drawing.Size(68, 26);
		this.buttonTransferAll.TabIndex = 168;
		this.buttonTransferAll.Text = "Transfer All >>";
		this.buttonTransferAll.UseVisualStyleBackColor = true;
		this.buttonTransferAll.Click += new System.EventHandler(buttonTransferAll_Click);
		this.label5.AutoSize = true;
		this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label5.Location = new System.Drawing.Point(3, 191);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(52, 13);
		this.label5.TabIndex = 165;
		this.label5.Text = "Contracts";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonPlusContract.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonPlusContract.BackgroundImage");
		this.buttonPlusContract.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonPlusContract.Location = new System.Drawing.Point(64, 181);
		this.buttonPlusContract.Name = "buttonPlusContract";
		this.buttonPlusContract.Size = new System.Drawing.Size(32, 32);
		this.buttonPlusContract.TabIndex = 134;
		this.toolTip.SetToolTip(this.buttonPlusContract, "Increase 1 year of contract to all players");
		this.buttonPlusContract.UseVisualStyleBackColor = false;
		this.buttonPlusContract.Click += new System.EventHandler(buttonPlusContract_Click);
		this.buttonMinusContract.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonMinusContract.BackgroundImage");
		this.buttonMinusContract.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonMinusContract.Location = new System.Drawing.Point(101, 181);
		this.buttonMinusContract.Name = "buttonMinusContract";
		this.buttonMinusContract.Size = new System.Drawing.Size(32, 32);
		this.buttonMinusContract.TabIndex = 135;
		this.toolTip.SetToolTip(this.buttonMinusContract, "Decrease 1 year of contract to all players");
		this.buttonMinusContract.UseVisualStyleBackColor = false;
		this.buttonMinusContract.Click += new System.EventHandler(buttonMinusContract_Click);
		this.labelLoanedFrom.AutoSize = true;
		this.labelLoanedFrom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLoanedFrom.Location = new System.Drawing.Point(137, 129);
		this.labelLoanedFrom.Name = "labelLoanedFrom";
		this.labelLoanedFrom.Size = new System.Drawing.Size(69, 13);
		this.labelLoanedFrom.TabIndex = 140;
		this.labelLoanedFrom.Text = "Loaned From";
		this.labelLoanedFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboTeamLoanedFrom.DataSource = this.teamListBindingSource;
		this.comboTeamLoanedFrom.ItemHeight = 13;
		this.comboTeamLoanedFrom.Location = new System.Drawing.Point(210, 125);
		this.comboTeamLoanedFrom.MaxLength = 32767;
		this.comboTeamLoanedFrom.Name = "comboTeamLoanedFrom";
		this.comboTeamLoanedFrom.Size = new System.Drawing.Size(163, 21);
		this.comboTeamLoanedFrom.TabIndex = 141;
		this.comboTeamLoanedFrom.SelectedIndexChanged += new System.EventHandler(comboTeamLoanedFrom_SelectedIndexChanged);
		this.teamListBindingSource.DataSource = typeof(FifaLibrary.TeamList);
		this.label2.AutoSize = true;
		this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label2.Location = new System.Drawing.Point(137, 57);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(69, 13);
		this.label2.TabIndex = 136;
		this.label2.Text = "Contract End";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.dateLoanEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateLoanEnd.Location = new System.Drawing.Point(210, 100);
		this.dateLoanEnd.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
		this.dateLoanEnd.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateLoanEnd.Name = "dateLoanEnd";
		this.dateLoanEnd.Size = new System.Drawing.Size(92, 20);
		this.dateLoanEnd.TabIndex = 139;
		this.dateLoanEnd.Value = new System.DateTime(2024, 6, 30, 0, 0, 0, 0);
		this.dateLoanEnd.ValueChanged += new System.EventHandler(dateLoanEnd_ValueChanged);
		this.labelLoanEnd.AutoSize = true;
		this.labelLoanEnd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLoanEnd.Location = new System.Drawing.Point(137, 104);
		this.labelLoanEnd.Name = "labelLoanEnd";
		this.labelLoanEnd.Size = new System.Drawing.Size(53, 13);
		this.labelLoanEnd.TabIndex = 138;
		this.labelLoanEnd.Text = "Loan End";
		this.labelLoanEnd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonRosterLetFree.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonRosterLetFree.Location = new System.Drawing.Point(305, 99);
		this.buttonRosterLetFree.Name = "buttonRosterLetFree";
		this.buttonRosterLetFree.Size = new System.Drawing.Size(68, 24);
		this.buttonRosterLetFree.TabIndex = 1;
		this.buttonRosterLetFree.Text = "Let Free >>";
		this.buttonRosterLetFree.UseVisualStyleBackColor = true;
		this.buttonRosterLetFree.Click += new System.EventHandler(buttonRosterLetFree_Click);
		this.buttonTransferPlayer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonTransferPlayer.Location = new System.Drawing.Point(305, 6);
		this.buttonTransferPlayer.Name = "buttonTransferPlayer";
		this.buttonTransferPlayer.Size = new System.Drawing.Size(68, 31);
		this.buttonTransferPlayer.TabIndex = 0;
		this.buttonTransferPlayer.Text = "Transfer >>";
		this.buttonTransferPlayer.UseVisualStyleBackColor = true;
		this.buttonTransferPlayer.Click += new System.EventHandler(buttonTransferPlayer_Click);
		this.checkIsLoan.Checked = true;
		this.checkIsLoan.CheckState = System.Windows.Forms.CheckState.Indeterminate;
		this.checkIsLoan.Location = new System.Drawing.Point(176, 75);
		this.checkIsLoan.Name = "checkIsLoan";
		this.checkIsLoan.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkIsLoan.Size = new System.Drawing.Size(85, 24);
		this.checkIsLoan.TabIndex = 164;
		this.checkIsLoan.Text = "Is Loaned ";
		this.checkIsLoan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkIsLoan.UseVisualStyleBackColor = true;
		this.checkIsLoan.CheckedChanged += new System.EventHandler(checkIsLoan_CheckedChanged);
		this.numericRosterYear.Location = new System.Drawing.Point(210, 54);
		this.numericRosterYear.Maximum = new decimal(new int[4] { 3000, 0, 0, 0 });
		this.numericRosterYear.Minimum = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.numericRosterYear.Name = "numericRosterYear";
		this.numericRosterYear.Size = new System.Drawing.Size(92, 20);
		this.numericRosterYear.TabIndex = 3;
		this.numericRosterYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericRosterYear.Value = new decimal(new int[4] { 2005, 0, 0, 0 });
		this.numericRosterYear.ValueChanged += new System.EventHandler(numericRosterYear_ValueChanged);
		this.buttonLoanTo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonLoanTo.Location = new System.Drawing.Point(304, 69);
		this.buttonLoanTo.Name = "buttonLoanTo";
		this.buttonLoanTo.Size = new System.Drawing.Size(68, 27);
		this.buttonLoanTo.TabIndex = 163;
		this.buttonLoanTo.Text = "   Loan    >>";
		this.buttonLoanTo.UseVisualStyleBackColor = true;
		this.buttonLoanTo.Click += new System.EventHandler(buttonLoanTo_Click);
		this.dateJoiningDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.dateJoiningDate.Location = new System.Drawing.Point(210, 30);
		this.dateJoiningDate.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
		this.dateJoiningDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
		this.dateJoiningDate.Name = "dateJoiningDate";
		this.dateJoiningDate.Size = new System.Drawing.Size(92, 20);
		this.dateJoiningDate.TabIndex = 132;
		this.dateJoiningDate.Value = new System.DateTime(2023, 7, 1, 0, 0, 0, 0);
		this.dateJoiningDate.ValueChanged += new System.EventHandler(dateJoiningDate_ValueChanged);
		this.viewer2DPhoto.AutoTransparency = true;
		this.viewer2DPhoto.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPhoto.ButtonStripVisible = false;
		this.viewer2DPhoto.CurrentBitmap = null;
		this.viewer2DPhoto.ExtendedFormat = false;
		this.viewer2DPhoto.FullSizeButton = false;
		this.viewer2DPhoto.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DPhoto.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DPhoto.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.MiniFace;
		this.viewer2DPhoto.Location = new System.Drawing.Point(5, 4);
		this.viewer2DPhoto.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DPhoto.Name = "viewer2DPhoto";
		this.viewer2DPhoto.RemoveButton = false;
		this.viewer2DPhoto.ShowButton = false;
		this.viewer2DPhoto.ShowButtonChecked = true;
		this.viewer2DPhoto.Size = new System.Drawing.Size(128, 153);
		this.viewer2DPhoto.TabIndex = 162;
		this.viewer2DPhoto.TabStop = false;
		this.labelJoiningDate.AutoSize = true;
		this.labelJoiningDate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelJoiningDate.Location = new System.Drawing.Point(137, 33);
		this.labelJoiningDate.Name = "labelJoiningDate";
		this.labelJoiningDate.Size = new System.Drawing.Size(26, 13);
		this.labelJoiningDate.TabIndex = 133;
		this.labelJoiningDate.Text = "Join";
		this.labelJoiningDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupTeamPlayerTuning.Controls.Add(this.buttonTeamPlayerMinus);
		this.groupTeamPlayerTuning.Controls.Add(this.buttonTeamPlayerPlus);
		this.groupTeamPlayerTuning.Controls.Add(this.labelTeamPlayerStars);
		this.groupTeamPlayerTuning.Location = new System.Drawing.Point(139, 153);
		this.groupTeamPlayerTuning.Name = "groupTeamPlayerTuning";
		this.groupTeamPlayerTuning.Size = new System.Drawing.Size(232, 60);
		this.groupTeamPlayerTuning.TabIndex = 161;
		this.groupTeamPlayerTuning.TabStop = false;
		this.groupTeamPlayerTuning.Text = "Rating";
		this.toolTip.SetToolTip(this.groupTeamPlayerTuning, "Increase all players overall");
		this.groupTeamPlayerTuning.Visible = false;
		this.buttonTeamPlayerMinus.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonTeamPlayerMinus.BackgroundImage");
		this.buttonTeamPlayerMinus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonTeamPlayerMinus.Location = new System.Drawing.Point(156, 9);
		this.buttonTeamPlayerMinus.Name = "buttonTeamPlayerMinus";
		this.buttonTeamPlayerMinus.Size = new System.Drawing.Size(48, 48);
		this.buttonTeamPlayerMinus.TabIndex = 1;
		this.toolTip.SetToolTip(this.buttonTeamPlayerMinus, "Decrease all players overall");
		this.buttonTeamPlayerMinus.UseVisualStyleBackColor = false;
		this.buttonTeamPlayerMinus.Visible = false;
		this.buttonTeamPlayerMinus.Click += new System.EventHandler(buttonTeamPlayerMinus_Click);
		this.buttonTeamPlayerPlus.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonTeamPlayerPlus.BackgroundImage");
		this.buttonTeamPlayerPlus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonTeamPlayerPlus.Location = new System.Drawing.Point(103, 9);
		this.buttonTeamPlayerPlus.Name = "buttonTeamPlayerPlus";
		this.buttonTeamPlayerPlus.Size = new System.Drawing.Size(48, 48);
		this.buttonTeamPlayerPlus.TabIndex = 0;
		this.toolTip.SetToolTip(this.buttonTeamPlayerPlus, "Increase all players overall");
		this.buttonTeamPlayerPlus.UseVisualStyleBackColor = false;
		this.buttonTeamPlayerPlus.Visible = false;
		this.buttonTeamPlayerPlus.Click += new System.EventHandler(buttonTeamPlayerPlus_Click);
		this.labelTeamPlayerStars.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelTeamPlayerStars.ImageIndex = 9;
		this.labelTeamPlayerStars.ImageList = this.imageListStars;
		this.labelTeamPlayerStars.Location = new System.Drawing.Point(3, 23);
		this.labelTeamPlayerStars.Name = "labelTeamPlayerStars";
		this.labelTeamPlayerStars.Size = new System.Drawing.Size(101, 20);
		this.labelTeamPlayerStars.TabIndex = 5;
		this.labelTeamPlayerStars.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelTeamPlayerStars.Visible = false;
		this.labelRosterName.BackColor = System.Drawing.SystemColors.Control;
		this.labelRosterName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelRosterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelRosterName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRosterName.Location = new System.Drawing.Point(4, 159);
		this.labelRosterName.Name = "labelRosterName";
		this.labelRosterName.Size = new System.Drawing.Size(129, 20);
		this.labelRosterName.TabIndex = 4;
		this.labelRosterName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.comboRosterNumber.FormattingEnabled = true;
		this.comboRosterNumber.Location = new System.Drawing.Point(210, 6);
		this.comboRosterNumber.Name = "comboRosterNumber";
		this.comboRosterNumber.Size = new System.Drawing.Size(92, 21);
		this.comboRosterNumber.TabIndex = 2;
		this.comboRosterNumber.SelectedIndexChanged += new System.EventHandler(comboRosterNumber_SelectedIndexChanged);
		this.labelRosterNumber.AutoSize = true;
		this.labelRosterNumber.BackColor = System.Drawing.SystemColors.Control;
		this.labelRosterNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRosterNumber.Location = new System.Drawing.Point(137, 10);
		this.labelRosterNumber.Name = "labelRosterNumber";
		this.labelRosterNumber.Size = new System.Drawing.Size(42, 13);
		this.labelRosterNumber.TabIndex = 6;
		this.labelRosterNumber.Text = "Shirt N.";
		this.labelRosterNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelRosterNameFrom.BackColor = System.Drawing.SystemColors.Control;
		this.labelRosterNameFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelRosterNameFrom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelRosterNameFrom.Location = new System.Drawing.Point(137, 52);
		this.labelRosterNameFrom.Name = "labelRosterNameFrom";
		this.labelRosterNameFrom.Size = new System.Drawing.Size(76, 20);
		this.labelRosterNameFrom.TabIndex = 144;
		this.labelRosterNameFrom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelPreviousTeam.AutoSize = true;
		this.labelPreviousTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPreviousTeam.Location = new System.Drawing.Point(137, 129);
		this.labelPreviousTeam.Name = "labelPreviousTeam";
		this.labelPreviousTeam.Size = new System.Drawing.Size(48, 13);
		this.labelPreviousTeam.TabIndex = 167;
		this.labelPreviousTeam.Text = "Previous";
		this.labelPreviousTeam.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboTeamPrevious.DataSource = this.teamListBindingSource;
		this.comboTeamPrevious.ItemHeight = 13;
		this.comboTeamPrevious.Location = new System.Drawing.Point(210, 125);
		this.comboTeamPrevious.MaxLength = 32767;
		this.comboTeamPrevious.Name = "comboTeamPrevious";
		this.comboTeamPrevious.Size = new System.Drawing.Size(163, 21);
		this.comboTeamPrevious.TabIndex = 166;
		this.comboTeamPrevious.SelectedIndexChanged += new System.EventHandler(comboTeamPrevious_SelectedIndexChanged);
		this.pageTeamAdboard.AutoScroll = true;
		this.pageTeamAdboard.Controls.Add(this.numericAdboards);
		this.pageTeamAdboard.Controls.Add(this.checkHasSpecificAdboard);
		this.pageTeamAdboard.Controls.Add(this.labelAdboard);
		this.pageTeamAdboard.Controls.Add(this.viewer2DAdboards_0);
		this.pageTeamAdboard.Location = new System.Drawing.Point(4, 22);
		this.pageTeamAdboard.Name = "pageTeamAdboard";
		this.pageTeamAdboard.Size = new System.Drawing.Size(1303, 781);
		this.pageTeamAdboard.TabIndex = 2;
		this.pageTeamAdboard.Text = "Adboards";
		this.pageTeamAdboard.UseVisualStyleBackColor = true;
		this.numericAdboards.Location = new System.Drawing.Point(115, 32);
		this.numericAdboards.Maximum = new decimal(new int[4] { 245, 0, 0, 0 });
		this.numericAdboards.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericAdboards.Name = "numericAdboards";
		this.numericAdboards.Size = new System.Drawing.Size(112, 20);
		this.numericAdboards.TabIndex = 0;
		this.numericAdboards.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericAdboards.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericAdboards.ValueChanged += new System.EventHandler(numericAdboards_ValueChanged);
		this.checkHasSpecificAdboard.AutoSize = true;
		this.checkHasSpecificAdboard.Location = new System.Drawing.Point(25, 9);
		this.checkHasSpecificAdboard.Name = "checkHasSpecificAdboard";
		this.checkHasSpecificAdboard.Size = new System.Drawing.Size(129, 17);
		this.checkHasSpecificAdboard.TabIndex = 5;
		this.checkHasSpecificAdboard.Text = "Has Specific Adboard";
		this.toolTip.SetToolTip(this.checkHasSpecificAdboard, "Create an Adboard specific for this team");
		this.checkHasSpecificAdboard.UseVisualStyleBackColor = true;
		this.checkHasSpecificAdboard.CheckedChanged += new System.EventHandler(checkHasSpecificAdboard_CheckedChanged);
		this.labelAdboard.AutoSize = true;
		this.labelAdboard.Location = new System.Drawing.Point(22, 34);
		this.labelAdboard.Name = "labelAdboard";
		this.labelAdboard.Size = new System.Drawing.Size(87, 13);
		this.labelAdboard.TabIndex = 4;
		this.labelAdboard.Text = "Adboard Number";
		this.viewer2DAdboards_0.AutoTransparency = false;
		this.viewer2DAdboards_0.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DAdboards_0.ButtonStripVisible = false;
		this.viewer2DAdboards_0.CurrentBitmap = null;
		this.viewer2DAdboards_0.ExtendedFormat = false;
		this.viewer2DAdboards_0.FullSizeButton = false;
		this.viewer2DAdboards_0.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DAdboards_0.ImageSize = new System.Drawing.Size(512, 1024);
		this.viewer2DAdboards_0.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DAdboards_0.Location = new System.Drawing.Point(8, 58);
		this.viewer2DAdboards_0.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DAdboards_0.Name = "viewer2DAdboards_0";
		this.viewer2DAdboards_0.RemoveButton = false;
		this.viewer2DAdboards_0.ShowButton = false;
		this.viewer2DAdboards_0.ShowButtonChecked = true;
		this.viewer2DAdboards_0.Size = new System.Drawing.Size(256, 537);
		this.viewer2DAdboards_0.TabIndex = 3;
		this.pageTeamFlags.AutoScroll = true;
		this.pageTeamFlags.Controls.Add(this.groupFlag);
		this.pageTeamFlags.Controls.Add(this.viewer2DBanners);
		this.pageTeamFlags.Location = new System.Drawing.Point(4, 22);
		this.pageTeamFlags.Name = "pageTeamFlags";
		this.pageTeamFlags.Size = new System.Drawing.Size(1303, 781);
		this.pageTeamFlags.TabIndex = 3;
		this.pageTeamFlags.Text = "Flags";
		this.pageTeamFlags.UseVisualStyleBackColor = true;
		this.groupFlag.Controls.Add(this.multiViewer2DFlags15);
		this.groupFlag.Controls.Add(this.buttonCreateFlags);
		this.groupFlag.Controls.Add(this.pictureBox4);
		this.groupFlag.Controls.Add(this.label22);
		this.groupFlag.Controls.Add(this.pictureFlagBlue);
		this.groupFlag.Controls.Add(this.pictureFlagRed);
		this.groupFlag.Controls.Add(this.pictureFlagGreen);
		this.groupFlag.Controls.Add(this.checkFlag4);
		this.groupFlag.Controls.Add(this.checkFlag3);
		this.groupFlag.Controls.Add(this.checkFlag2);
		this.groupFlag.Controls.Add(this.checkFlag1);
		this.groupFlag.Controls.Add(this.labelFlag4);
		this.groupFlag.Controls.Add(this.labelFlag3);
		this.groupFlag.Controls.Add(this.labelFlag2);
		this.groupFlag.Controls.Add(this.labelFlag1);
		this.groupFlag.Location = new System.Drawing.Point(526, 3);
		this.groupFlag.Name = "groupFlag";
		this.groupFlag.Size = new System.Drawing.Size(532, 405);
		this.groupFlag.TabIndex = 2;
		this.groupFlag.TabStop = false;
		this.groupFlag.Text = "Flags";
		this.multiViewer2DFlags15.AutoTransparency = false;
		this.multiViewer2DFlags15.Bitmaps = null;
		this.multiViewer2DFlags15.CheckBitmapSize = true;
		this.multiViewer2DFlags15.FixedSize = false;
		this.multiViewer2DFlags15.FullSizeButton = false;
		this.multiViewer2DFlags15.LabelText = "Flag n.";
		this.multiViewer2DFlags15.Location = new System.Drawing.Point(6, 19);
		this.multiViewer2DFlags15.Name = "multiViewer2DFlags15";
		this.multiViewer2DFlags15.ShowButton = false;
		this.multiViewer2DFlags15.ShowDeleteButton = false;
		this.multiViewer2DFlags15.Size = new System.Drawing.Size(514, 302);
		this.multiViewer2DFlags15.TabIndex = 154;
		this.buttonCreateFlags.Location = new System.Drawing.Point(403, 335);
		this.buttonCreateFlags.Name = "buttonCreateFlags";
		this.buttonCreateFlags.Size = new System.Drawing.Size(104, 55);
		this.buttonCreateFlags.TabIndex = 153;
		this.buttonCreateFlags.Text = "Create Flags";
		this.buttonCreateFlags.UseVisualStyleBackColor = true;
		this.buttonCreateFlags.Click += new System.EventHandler(buttonCreateFlags_Click);
		this.pictureBox4.BackgroundImage = (System.Drawing.Image)resources.GetObject("pictureBox4.BackgroundImage");
		this.pictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.pictureBox4.Location = new System.Drawing.Point(286, 347);
		this.pictureBox4.Name = "pictureBox4";
		this.pictureBox4.Size = new System.Drawing.Size(98, 13);
		this.pictureBox4.TabIndex = 152;
		this.pictureBox4.TabStop = false;
		this.label22.AutoSize = true;
		this.label22.BackColor = System.Drawing.Color.Transparent;
		this.label22.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label22.Location = new System.Drawing.Point(320, 331);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(36, 13);
		this.label22.TabIndex = 151;
		this.label22.Text = "Colors";
		this.label22.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.pictureFlagBlue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureFlagBlue.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureFlagBlue.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.teamBindingSource, "TeamColor3", true));
		this.pictureFlagBlue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureFlagBlue.Location = new System.Drawing.Point(360, 366);
		this.pictureFlagBlue.Name = "pictureFlagBlue";
		this.pictureFlagBlue.Size = new System.Drawing.Size(24, 24);
		this.pictureFlagBlue.TabIndex = 150;
		this.pictureFlagBlue.TabStop = false;
		this.pictureFlagBlue.Click += new System.EventHandler(pictureFlagBlue_Click);
		this.pictureFlagRed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureFlagRed.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureFlagRed.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.teamBindingSource, "TeamColor1", true));
		this.pictureFlagRed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureFlagRed.Location = new System.Drawing.Point(286, 366);
		this.pictureFlagRed.Name = "pictureFlagRed";
		this.pictureFlagRed.Size = new System.Drawing.Size(24, 24);
		this.pictureFlagRed.TabIndex = 148;
		this.pictureFlagRed.TabStop = false;
		this.pictureFlagRed.Click += new System.EventHandler(pictureFlagRed_Click);
		this.pictureFlagGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureFlagGreen.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureFlagGreen.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.teamBindingSource, "TeamColor2", true));
		this.pictureFlagGreen.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureFlagGreen.Location = new System.Drawing.Point(323, 366);
		this.pictureFlagGreen.Name = "pictureFlagGreen";
		this.pictureFlagGreen.Size = new System.Drawing.Size(24, 24);
		this.pictureFlagGreen.TabIndex = 149;
		this.pictureFlagGreen.TabStop = false;
		this.pictureFlagGreen.Click += new System.EventHandler(pictureFlagGreen_Click);
		this.checkFlag4.AutoSize = true;
		this.checkFlag4.Checked = true;
		this.checkFlag4.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkFlag4.Location = new System.Drawing.Point(223, 335);
		this.checkFlag4.Name = "checkFlag4";
		this.checkFlag4.Size = new System.Drawing.Size(15, 14);
		this.checkFlag4.TabIndex = 7;
		this.toolTip.SetToolTip(this.checkFlag4, "Check to add logo to the flag");
		this.checkFlag4.UseVisualStyleBackColor = true;
		this.checkFlag3.AutoSize = true;
		this.checkFlag3.Checked = true;
		this.checkFlag3.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkFlag3.Location = new System.Drawing.Point(159, 335);
		this.checkFlag3.Name = "checkFlag3";
		this.checkFlag3.Size = new System.Drawing.Size(15, 14);
		this.checkFlag3.TabIndex = 6;
		this.toolTip.SetToolTip(this.checkFlag3, "Check to add logo to the flag");
		this.checkFlag3.UseVisualStyleBackColor = true;
		this.checkFlag2.AutoSize = true;
		this.checkFlag2.Checked = true;
		this.checkFlag2.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkFlag2.Location = new System.Drawing.Point(95, 335);
		this.checkFlag2.Name = "checkFlag2";
		this.checkFlag2.Size = new System.Drawing.Size(15, 14);
		this.checkFlag2.TabIndex = 5;
		this.toolTip.SetToolTip(this.checkFlag2, "Check to add logo to the flag");
		this.checkFlag2.UseVisualStyleBackColor = true;
		this.checkFlag1.AutoSize = true;
		this.checkFlag1.Checked = true;
		this.checkFlag1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkFlag1.Location = new System.Drawing.Point(29, 335);
		this.checkFlag1.Name = "checkFlag1";
		this.checkFlag1.Size = new System.Drawing.Size(15, 14);
		this.checkFlag1.TabIndex = 4;
		this.toolTip.SetToolTip(this.checkFlag1, "Check to add logo to the flag");
		this.checkFlag1.UseVisualStyleBackColor = true;
		this.labelFlag4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelFlag4.ImageIndex = 10;
		this.labelFlag4.ImageList = this.imageListFlags;
		this.labelFlag4.Location = new System.Drawing.Point(207, 358);
		this.labelFlag4.Name = "labelFlag4";
		this.labelFlag4.Size = new System.Drawing.Size(50, 30);
		this.labelFlag4.TabIndex = 3;
		this.toolTip.SetToolTip(this.labelFlag4, "Click to change flag style");
		this.labelFlag4.Click += new System.EventHandler(labelFlag1_Click);
		this.imageListFlags.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListFlags.ImageStream");
		this.imageListFlags.TransparentColor = System.Drawing.Color.Transparent;
		this.imageListFlags.Images.SetKeyName(0, "gf1.png");
		this.imageListFlags.Images.SetKeyName(1, "gf2.png");
		this.imageListFlags.Images.SetKeyName(2, "gf3.png");
		this.imageListFlags.Images.SetKeyName(3, "gf4.png");
		this.imageListFlags.Images.SetKeyName(4, "gf5.png");
		this.imageListFlags.Images.SetKeyName(5, "gf6.png");
		this.imageListFlags.Images.SetKeyName(6, "gf7.png");
		this.imageListFlags.Images.SetKeyName(7, "gf8.png");
		this.imageListFlags.Images.SetKeyName(8, "gf9.png");
		this.imageListFlags.Images.SetKeyName(9, "gf10.png");
		this.imageListFlags.Images.SetKeyName(10, "gf11.png");
		this.imageListFlags.Images.SetKeyName(11, "gf13.png");
		this.imageListFlags.Images.SetKeyName(12, "gf15.png");
		this.labelFlag3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelFlag3.ImageIndex = 2;
		this.labelFlag3.ImageList = this.imageListFlags;
		this.labelFlag3.Location = new System.Drawing.Point(142, 358);
		this.labelFlag3.Name = "labelFlag3";
		this.labelFlag3.Size = new System.Drawing.Size(50, 30);
		this.labelFlag3.TabIndex = 2;
		this.toolTip.SetToolTip(this.labelFlag3, "Click to change flag style");
		this.labelFlag3.Click += new System.EventHandler(labelFlag1_Click);
		this.labelFlag2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelFlag2.ImageIndex = 1;
		this.labelFlag2.ImageList = this.imageListFlags;
		this.labelFlag2.Location = new System.Drawing.Point(77, 358);
		this.labelFlag2.Name = "labelFlag2";
		this.labelFlag2.Size = new System.Drawing.Size(50, 30);
		this.labelFlag2.TabIndex = 1;
		this.toolTip.SetToolTip(this.labelFlag2, "Click to change flag style");
		this.labelFlag2.Click += new System.EventHandler(labelFlag1_Click);
		this.labelFlag1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelFlag1.ImageIndex = 0;
		this.labelFlag1.ImageList = this.imageListFlags;
		this.labelFlag1.Location = new System.Drawing.Point(12, 358);
		this.labelFlag1.Name = "labelFlag1";
		this.labelFlag1.Size = new System.Drawing.Size(50, 30);
		this.labelFlag1.TabIndex = 0;
		this.toolTip.SetToolTip(this.labelFlag1, "Click to change flag style");
		this.labelFlag1.Click += new System.EventHandler(labelFlag1_Click);
		this.viewer2DBanners.AutoTransparency = false;
		this.viewer2DBanners.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DBanners.ButtonStripVisible = false;
		this.viewer2DBanners.CurrentBitmap = null;
		this.viewer2DBanners.ExtendedFormat = false;
		this.viewer2DBanners.FullSizeButton = false;
		this.viewer2DBanners.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DBanners.ImageSize = new System.Drawing.Size(1024, 512);
		this.viewer2DBanners.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DBanners.Location = new System.Drawing.Point(8, 3);
		this.viewer2DBanners.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DBanners.Name = "viewer2DBanners";
		this.viewer2DBanners.RemoveButton = false;
		this.viewer2DBanners.ShowButton = false;
		this.viewer2DBanners.ShowButtonChecked = true;
		this.viewer2DBanners.Size = new System.Drawing.Size(512, 281);
		this.viewer2DBanners.TabIndex = 0;
		this.pageTeamrevMod.Controls.Add(this.flowLayoutPanel1);
		this.pageTeamrevMod.Location = new System.Drawing.Point(4, 22);
		this.pageTeamrevMod.Name = "pageTeamrevMod";
		this.pageTeamrevMod.Size = new System.Drawing.Size(1303, 781);
		this.pageTeamrevMod.TabIndex = 4;
		this.pageTeamrevMod.Text = "Rev. Mod. Extensions";
		this.pageTeamrevMod.UseVisualStyleBackColor = true;
		this.flowLayoutPanel1.Controls.Add(this.groupTeamAdboardsRevMod);
		this.flowLayoutPanel1.Controls.Add(this.groupTeamBallRevMod);
		this.flowLayoutPanel1.Controls.Add(this.groupTeamManager);
		this.flowLayoutPanel1.Controls.Add(this.groupTeamScarfRevMod);
		this.flowLayoutPanel1.Controls.Add(this.groupTeamGoalNetRevMod);
		this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
		this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		this.flowLayoutPanel1.Name = "flowLayoutPanel1";
		this.flowLayoutPanel1.Size = new System.Drawing.Size(1303, 781);
		this.flowLayoutPanel1.TabIndex = 0;
		this.groupTeamAdboardsRevMod.Controls.Add(this.viewer2DTeamAdboard);
		this.groupTeamAdboardsRevMod.Location = new System.Drawing.Point(3, 3);
		this.groupTeamAdboardsRevMod.Name = "groupTeamAdboardsRevMod";
		this.groupTeamAdboardsRevMod.Size = new System.Drawing.Size(270, 570);
		this.groupTeamAdboardsRevMod.TabIndex = 164;
		this.groupTeamAdboardsRevMod.TabStop = false;
		this.groupTeamAdboardsRevMod.Text = "Unique Adboards";
		this.viewer2DTeamAdboard.AutoTransparency = false;
		this.viewer2DTeamAdboard.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTeamAdboard.ButtonStripVisible = false;
		this.viewer2DTeamAdboard.CurrentBitmap = null;
		this.viewer2DTeamAdboard.ExtendedFormat = false;
		this.viewer2DTeamAdboard.FullSizeButton = false;
		this.viewer2DTeamAdboard.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DTeamAdboard.ImageSize = new System.Drawing.Size(512, 1024);
		this.viewer2DTeamAdboard.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTeamAdboard.Location = new System.Drawing.Point(5, 24);
		this.viewer2DTeamAdboard.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DTeamAdboard.Name = "viewer2DTeamAdboard";
		this.viewer2DTeamAdboard.RemoveButton = false;
		this.viewer2DTeamAdboard.ShowButton = false;
		this.viewer2DTeamAdboard.ShowButtonChecked = true;
		this.viewer2DTeamAdboard.Size = new System.Drawing.Size(256, 537);
		this.viewer2DTeamAdboard.TabIndex = 4;
		this.groupTeamBallRevMod.Controls.Add(this.toolTeamBall3D);
		this.groupTeamBallRevMod.Controls.Add(this.multiViewer2DTeamBallTextures);
		this.groupTeamBallRevMod.Location = new System.Drawing.Point(279, 3);
		this.groupTeamBallRevMod.Name = "groupTeamBallRevMod";
		this.groupTeamBallRevMod.Size = new System.Drawing.Size(529, 340);
		this.groupTeamBallRevMod.TabIndex = 166;
		this.groupTeamBallRevMod.TabStop = false;
		this.groupTeamBallRevMod.Text = "Unique Ball";
		this.toolTeamBall3D.AutoSize = false;
		this.toolTeamBall3D.Dock = System.Windows.Forms.DockStyle.None;
		this.toolTeamBall3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolTeamBall3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.buttonShow3DBall, this.toolStripSeparator1, this.buttonImport3DModelTeamBall, this.buttonExport3DModelTeamBall, this.toolStripSeparator2, this.buttonRemove3DModelTeamBall });
		this.toolTeamBall3D.Location = new System.Drawing.Point(265, 301);
		this.toolTeamBall3D.Name = "toolTeamBall3D";
		this.toolTeamBall3D.Size = new System.Drawing.Size(256, 25);
		this.toolTeamBall3D.TabIndex = 4;
		this.buttonShow3DBall.CheckOnClick = true;
		this.buttonShow3DBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow3DBall.Image = (System.Drawing.Image)resources.GetObject("buttonShow3DBall.Image");
		this.buttonShow3DBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow3DBall.Name = "buttonShow3DBall";
		this.buttonShow3DBall.Size = new System.Drawing.Size(23, 22);
		this.buttonShow3DBall.Text = "Show / Hide";
		this.buttonShow3DBall.Click += new System.EventHandler(buttonShow3DBall_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonImport3DModelTeamBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImport3DModelTeamBall.Image = (System.Drawing.Image)resources.GetObject("buttonImport3DModelTeamBall.Image");
		this.buttonImport3DModelTeamBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImport3DModelTeamBall.Name = "buttonImport3DModelTeamBall";
		this.buttonImport3DModelTeamBall.Size = new System.Drawing.Size(23, 22);
		this.buttonImport3DModelTeamBall.Text = "Import 3D Model";
		this.buttonImport3DModelTeamBall.Click += new System.EventHandler(buttonImport3DModelTeamBall_Click);
		this.buttonExport3DModelTeamBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport3DModelTeamBall.Image = (System.Drawing.Image)resources.GetObject("buttonExport3DModelTeamBall.Image");
		this.buttonExport3DModelTeamBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport3DModelTeamBall.Name = "buttonExport3DModelTeamBall";
		this.buttonExport3DModelTeamBall.Size = new System.Drawing.Size(23, 22);
		this.buttonExport3DModelTeamBall.Text = "Export 3D Model";
		this.buttonExport3DModelTeamBall.Click += new System.EventHandler(buttonExport3DModelTeamBall_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonRemove3DModelTeamBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove3DModelTeamBall.Image = (System.Drawing.Image)resources.GetObject("buttonRemove3DModelTeamBall.Image");
		this.buttonRemove3DModelTeamBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove3DModelTeamBall.Name = "buttonRemove3DModelTeamBall";
		this.buttonRemove3DModelTeamBall.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove3DModelTeamBall.Text = "Remove 3D Model";
		this.buttonRemove3DModelTeamBall.Click += new System.EventHandler(buttonRemove3DModelTeamBall_Click);
		this.multiViewer2DTeamBallTextures.AutoTransparency = false;
		this.multiViewer2DTeamBallTextures.Bitmaps = null;
		this.multiViewer2DTeamBallTextures.CheckBitmapSize = true;
		this.multiViewer2DTeamBallTextures.FixedSize = true;
		this.multiViewer2DTeamBallTextures.FullSizeButton = false;
		this.multiViewer2DTeamBallTextures.LabelText = "Texture";
		this.multiViewer2DTeamBallTextures.Location = new System.Drawing.Point(3, 24);
		this.multiViewer2DTeamBallTextures.Name = "multiViewer2DTeamBallTextures";
		this.multiViewer2DTeamBallTextures.ShowButton = false;
		this.multiViewer2DTeamBallTextures.ShowDeleteButton = false;
		this.multiViewer2DTeamBallTextures.Size = new System.Drawing.Size(256, 302);
		this.multiViewer2DTeamBallTextures.TabIndex = 1;
		this.groupTeamManager.Controls.Add(this.toolTeamManager3D);
		this.groupTeamManager.Controls.Add(this.viewer2DTeamManager);
		this.groupTeamManager.Location = new System.Drawing.Point(279, 349);
		this.groupTeamManager.Name = "groupTeamManager";
		this.groupTeamManager.Size = new System.Drawing.Size(529, 308);
		this.groupTeamManager.TabIndex = 167;
		this.groupTeamManager.TabStop = false;
		this.groupTeamManager.Text = "Unique Team Manager";
		this.toolTeamManager3D.AutoSize = false;
		this.toolTeamManager3D.Dock = System.Windows.Forms.DockStyle.None;
		this.toolTeamManager3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolTeamManager3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.buttonShow3DManager, this.toolStripSeparator3, this.buttonImportModel3DTeamManager, this.buttonExportModel3DTeamManager, this.toolStripSeparator4, this.buttonDeleteModel3DTeamManager });
		this.toolTeamManager3D.Location = new System.Drawing.Point(267, 275);
		this.toolTeamManager3D.Name = "toolTeamManager3D";
		this.toolTeamManager3D.Size = new System.Drawing.Size(256, 25);
		this.toolTeamManager3D.TabIndex = 6;
		this.buttonShow3DManager.CheckOnClick = true;
		this.buttonShow3DManager.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow3DManager.Image = (System.Drawing.Image)resources.GetObject("buttonShow3DManager.Image");
		this.buttonShow3DManager.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow3DManager.Name = "buttonShow3DManager";
		this.buttonShow3DManager.Size = new System.Drawing.Size(23, 22);
		this.buttonShow3DManager.Text = "Show / Hide";
		this.buttonShow3DManager.Click += new System.EventHandler(buttonShow3DManager_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
		this.buttonImportModel3DTeamManager.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportModel3DTeamManager.Image = (System.Drawing.Image)resources.GetObject("buttonImportModel3DTeamManager.Image");
		this.buttonImportModel3DTeamManager.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportModel3DTeamManager.Name = "buttonImportModel3DTeamManager";
		this.buttonImportModel3DTeamManager.Size = new System.Drawing.Size(23, 22);
		this.buttonImportModel3DTeamManager.Text = "Import 3D Model";
		this.buttonImportModel3DTeamManager.Click += new System.EventHandler(buttonImportModel3DTeamManager_Click);
		this.buttonExportModel3DTeamManager.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportModel3DTeamManager.Image = (System.Drawing.Image)resources.GetObject("buttonExportModel3DTeamManager.Image");
		this.buttonExportModel3DTeamManager.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportModel3DTeamManager.Name = "buttonExportModel3DTeamManager";
		this.buttonExportModel3DTeamManager.Size = new System.Drawing.Size(23, 22);
		this.buttonExportModel3DTeamManager.Text = "Export 3D Model";
		this.buttonExportModel3DTeamManager.Click += new System.EventHandler(buttonExportModel3DTeamManager_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.buttonDeleteModel3DTeamManager.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteModel3DTeamManager.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteModel3DTeamManager.Image");
		this.buttonDeleteModel3DTeamManager.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteModel3DTeamManager.Name = "buttonDeleteModel3DTeamManager";
		this.buttonDeleteModel3DTeamManager.Size = new System.Drawing.Size(23, 22);
		this.buttonDeleteModel3DTeamManager.Text = "Remove 3D Model";
		this.buttonDeleteModel3DTeamManager.Click += new System.EventHandler(buttonDeleteModel3DTeamManager_Click);
		this.viewer2DTeamManager.AutoTransparency = false;
		this.viewer2DTeamManager.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTeamManager.ButtonStripVisible = false;
		this.viewer2DTeamManager.CurrentBitmap = null;
		this.viewer2DTeamManager.ExtendedFormat = false;
		this.viewer2DTeamManager.FullSizeButton = false;
		this.viewer2DTeamManager.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DTeamManager.ImageSize = new System.Drawing.Size(1024, 1024);
		this.viewer2DTeamManager.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTeamManager.Location = new System.Drawing.Point(6, 19);
		this.viewer2DTeamManager.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DTeamManager.Name = "viewer2DTeamManager";
		this.viewer2DTeamManager.RemoveButton = false;
		this.viewer2DTeamManager.ShowButton = false;
		this.viewer2DTeamManager.ShowButtonChecked = true;
		this.viewer2DTeamManager.Size = new System.Drawing.Size(256, 281);
		this.viewer2DTeamManager.TabIndex = 4;
		this.viewer2DTeamManager.TabStop = false;
		this.groupTeamScarfRevMod.Controls.Add(this.multiViewer2DTeamScarf);
		this.groupTeamScarfRevMod.Location = new System.Drawing.Point(814, 3);
		this.groupTeamScarfRevMod.Name = "groupTeamScarfRevMod";
		this.groupTeamScarfRevMod.Size = new System.Drawing.Size(270, 128);
		this.groupTeamScarfRevMod.TabIndex = 165;
		this.groupTeamScarfRevMod.TabStop = false;
		this.groupTeamScarfRevMod.Text = "Unique Scarf";
		this.multiViewer2DTeamScarf.AutoTransparency = false;
		this.multiViewer2DTeamScarf.Bitmaps = null;
		this.multiViewer2DTeamScarf.CheckBitmapSize = true;
		this.multiViewer2DTeamScarf.FixedSize = true;
		this.multiViewer2DTeamScarf.FullSizeButton = false;
		this.multiViewer2DTeamScarf.LabelText = "Scarf n.";
		this.multiViewer2DTeamScarf.Location = new System.Drawing.Point(6, 24);
		this.multiViewer2DTeamScarf.Name = "multiViewer2DTeamScarf";
		this.multiViewer2DTeamScarf.ShowButton = false;
		this.multiViewer2DTeamScarf.ShowDeleteButton = false;
		this.multiViewer2DTeamScarf.Size = new System.Drawing.Size(256, 90);
		this.multiViewer2DTeamScarf.TabIndex = 155;
		this.groupTeamGoalNetRevMod.Controls.Add(this.viewer2DTeamNet);
		this.groupTeamGoalNetRevMod.Location = new System.Drawing.Point(814, 137);
		this.groupTeamGoalNetRevMod.Name = "groupTeamGoalNetRevMod";
		this.groupTeamGoalNetRevMod.Size = new System.Drawing.Size(270, 180);
		this.groupTeamGoalNetRevMod.TabIndex = 0;
		this.groupTeamGoalNetRevMod.TabStop = false;
		this.groupTeamGoalNetRevMod.Text = "Unique Goal Net";
		this.viewer2DTeamNet.AutoTransparency = true;
		this.viewer2DTeamNet.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTeamNet.ButtonStripVisible = false;
		this.viewer2DTeamNet.CurrentBitmap = null;
		this.viewer2DTeamNet.ExtendedFormat = false;
		this.viewer2DTeamNet.FullSizeButton = false;
		this.viewer2DTeamNet.ImageLayout = System.Windows.Forms.ImageLayout.Tile;
		this.viewer2DTeamNet.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DTeamNet.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTeamNet.Location = new System.Drawing.Point(67, 19);
		this.viewer2DTeamNet.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DTeamNet.Name = "viewer2DTeamNet";
		this.viewer2DTeamNet.RemoveButton = false;
		this.viewer2DTeamNet.ShowButton = false;
		this.viewer2DTeamNet.ShowButtonChecked = true;
		this.viewer2DTeamNet.Size = new System.Drawing.Size(128, 153);
		this.viewer2DTeamNet.TabIndex = 2;
		this.viewer2DTeamNet.TabStop = false;
		this.colorDialog.FullOpen = true;
		this.colorDialog.SolidColorOnly = true;
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
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = false;
		this.pickUpControl.CreateButtonEnabled = true;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = new string[5] { "All", "by League", "by Country", "No League", "Missed Kits" };
		this.pickUpControl.FilterEnabled = true;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = true;
		this.pickUpControl.Margin = new System.Windows.Forms.Padding(4);
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = true;
		this.pickUpControl.SearchEnabled = true;
		this.pickUpControl.Size = new System.Drawing.Size(1311, 25);
		this.pickUpControl.TabIndex = 4;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		this.formationListBindingSource.DataSource = typeof(FifaLibrary.FormationList);
		this.ballListBindingSource.DataSource = typeof(FifaLibrary.BallList);
		this.prevLeagueListBindingSource.DataSource = typeof(FifaLibrary.LeagueList);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1311, 832);
		base.Controls.Add(this.tableEditTeam);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "TeamForm";
		this.Text = "TeamForm";
		base.Load += new System.EventHandler(TeamForm_Load);
		this.tableEditTeam.ResumeLayout(false);
		this.pageTeamGeneric.ResumeLayout(false);
		this.flowPanelTeamGeneric.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBoxName.ResumeLayout(false);
		this.groupBoxName.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.teamBindingSource).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.stadiumListBindingSource).EndInit();
		this.groupManager.ResumeLayout(false);
		this.groupManager.PerformLayout();
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.leagueListBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamTerColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamPrimColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamSecColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTeamId).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBall).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBall).EndInit();
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericInitialBudget).EndInit();
		this.groupLastYear.ResumeLayout(false);
		this.groupLastYear.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericPositionLastYear).EndInit();
		this.groupLocation.ResumeLayout(false);
		this.groupLocation.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUtcOffset).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericLongitude).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericLatitude).EndInit();
		this.groupTeamTraits.ResumeLayout(false);
		this.groupTeamTraits.PerformLayout();
		this.groupBox7.ResumeLayout(false);
		this.groupBox7.PerformLayout();
		this.pageTeamRoster.ResumeLayout(false);
		this.groupBox6.ResumeLayout(false);
		this.groupBox6.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCcpassing).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericCccrossing).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericCcshooting).EndInit();
		this.groupBox5.ResumeLayout(false);
		this.groupBox5.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericBusbuildupspeed).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBuspassing).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
		this.groupBox4.ResumeLayout(false);
		this.groupBox4.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericDefmentality).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericDefaggression).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericDefteamwidth).EndInit();
		this.groupFormation.ResumeLayout(false);
		this.groupFormation.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.groupAvailablePlayers.ResumeLayout(false);
		this.panelAvailablePlayersTop.ResumeLayout(false);
		this.panelAvailablePlayersTop.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureAvailablePlayer).EndInit();
		this.groupTeamPlayers.ResumeLayout(false);
		this.panelTeamPlayersTop.ResumeLayout(false);
		this.panelTeamPlayersTop.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericRosterYear).EndInit();
		this.groupTeamPlayerTuning.ResumeLayout(false);
		this.pageTeamAdboard.ResumeLayout(false);
		this.pageTeamAdboard.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericAdboards).EndInit();
		this.pageTeamFlags.ResumeLayout(false);
		this.groupFlag.ResumeLayout(false);
		this.groupFlag.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFlagBlue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFlagRed).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFlagGreen).EndInit();
		this.pageTeamrevMod.ResumeLayout(false);
		this.flowLayoutPanel1.ResumeLayout(false);
		this.groupTeamAdboardsRevMod.ResumeLayout(false);
		this.groupTeamBallRevMod.ResumeLayout(false);
		this.toolTeamBall3D.ResumeLayout(false);
		this.toolTeamBall3D.PerformLayout();
		this.groupTeamManager.ResumeLayout(false);
		this.toolTeamManager3D.ResumeLayout(false);
		this.toolTeamManager3D.PerformLayout();
		this.groupTeamScarfRevMod.ResumeLayout(false);
		this.groupTeamGoalNetRevMod.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.formationListBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ballListBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.prevLeagueListBindingSource).EndInit();
		base.ResumeLayout(false);
	}
}
