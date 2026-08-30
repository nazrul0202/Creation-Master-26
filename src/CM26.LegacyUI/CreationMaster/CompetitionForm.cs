using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace CreationMaster;

public class CompetitionForm : Form
{
	internal Fc26CompdataPanel Fc26Compdata { get; private set; }
	private bool m_IsLoaded;

	private CompobjList m_Competitions;

	private World m_CurrentWorld;

	private Confederation m_CurrentConfederation;

	private Nation m_CurrentNation;

	private Trophy m_CurrentTrophy;

	private Trophy m_ClipboardTrophy;

	private Stage m_CurrentStage;

	private Group m_CurrentGroup;

	private Compobj m_CurrentCompobj;

	private Schedule m_CurrentStageSchedule;

	private Schedule m_CurrentGroupSchedule;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private bool m_Locked;

	private bool m_LockTree;

	private Label[] m_QRLabels = new Label[64];

	private Label[] m_AdvanceLabels = new Label[78];

	private Label[] m_UpdateTableLabels = new Label[48];

	private int m_NUpdateTableLabels;

	private ComboBox[] m_SpecialTeamCombos = new ComboBox[4];

	private ComboBox[] m_StadiumCombos = new ComboBox[12];

	private QualifyRuleDialog m_QualifyRuleDialog = new QualifyRuleDialog();

	private AdvanceRuleDialog m_AdvanceRuleDialog = new AdvanceRuleDialog();

	private RankingRuleDialog m_RankingRuleDialog = new RankingRuleDialog();

	private NumericUpDown[] m_RainProb = new NumericUpDown[12];

	private NumericUpDown[] m_SnowProb = new NumericUpDown[12];

	private NumericUpDown[] m_OvercastProb = new NumericUpDown[12];

	private NumericUpDown[] m_ClearProb = new NumericUpDown[12];

	private NumericUpDown[] m_HazyProb = new NumericUpDown[12];

	private NumericUpDown[] m_CloudyProb = new NumericUpDown[12];

	private NumericUpDown[] m_FoggyProb = new NumericUpDown[12];

	private NumericUpDown[] m_ShowersProb = new NumericUpDown[12];

	private NumericUpDown[] m_FlurriesProb = new NumericUpDown[12];

	private ComboBox[] m_SunsetTime = new ComboBox[12];

	private ComboBox[] m_NightTime = new ComboBox[12];

	private Panel[] m_InitTeamPanel = new Panel[48];

	private ComboBox[] m_InitTeamCombo = new ComboBox[48];

	private Nation m_ClipboardNation;

	private Stage m_ClipboardStageForSchedule;

	private Group m_ClipboardGroupForSchedule;

	private Group m_ClipboardGroup;

	private string m_TrophyCurrentFolder = FifaEnvironment.ExportFolder;

	private string m_TempFolder;

	private string m_PatchFileName;

	private string[] m_PatchCompetitionFileNames;

	private StreamWriter[] m_PatchStreamWriters;

	private StreamReader[] m_PatchStreamReaders;

	private Viewer3D viewer3DTrophy;

	private Viewer3D viewer3DTournamentBall;

	private IContainer components;

	private TreeView treeWorld;

	private GroupBox groupConfederation;

	private Label labelConfStartMonth;

	private ComboBox comboConfederationStartingMonth;

	private GroupBox groupNation;

	private GroupBox groupTrophy;

	private GroupBox groupStage;

	private ComboBox comboNationStartMonth;

	private NumericUpDown numericNationYellowsStored;

	private ComboBox comboNationStandingsRules;

	private CheckBox checkNationStandingsRules;

	private ComboBox comboCountry;

	private Label labelDatabaseCountry;

	private ToolTip toolTip;

	private Label labelCompetitionType;

	private Label labelAssetId;

	private Label labelMatchImportance;

	private CheckBox checkTrophyStandingsRules;

	private CheckBox checkPromotionLeague;

	private CheckBox checkRelegationLeague;

	private GroupBox groupSchedule;

	private GroupBox groupPromotionRelegation;

	private CheckBox checkForceSchedule;

	private TextBox textTrophyLongName;

	private TextBox textTrophyShortName;

	private Label labeTrophylLongName;

	private Label labelTrophyShortName;

	private Button buttonGetId;

	private NumericUpDown numericAssetId;

	private ComboBox comboCompetitionType;

	private NumericUpDown numericImportance;

	private ComboBox comboTrophyStandingRules;

	private ComboBox comboRelegationLeague;

	private ComboBox comboPromotionLeague;

	private ComboBox comboSchedForce;

	private ToolStrip toolCompetitionTree;

	private SplitContainer splitContainer1;

	private Panel panelCompObj;

	private Label label1;

	private Label label3;

	private TextBox textLanguageKey;

	private Label label2;

	private TextBox textFourCharName;

	private NumericUpDown numericNTeams;

	private Label label4;

	private Panel panelQualificationRules;

	private Panel panelAdvancement;

	private TabControl tabCompetitions;

	private TabPage pageConfederation;

	private TabPage pageNation;

	private TabPage pageTrophy;

	private TabPage pageStage;

	private TabPage pageGroup;

	private TabPage pageWorld;

	private GroupBox groupGroup;

	private CheckBox checkScheduleConflicts;

	private GroupBox groupBenchPlayers;

	private RadioButton radioBench7Players;

	private RadioButton radioBench5Players;

	private Label label6;

	private Label label5;

	private ComboBox comboStageType;

	private Label label7;

	private GroupBox groupPlayStage;

	private ComboBox comboMatchSituation;

	private Label label8;

	private GroupBox groupSetupStage;

	private NumericUpDown numericPrizeMoney;

	private Label label9;

	private NumericUpDown numericMoneyDrop;

	private Label label10;

	private CheckBox checkRandomDraw;

	private CheckBox checkMaxteamsgroup;

	private CheckBox checkMaxteamsassoc;

	private CheckBox checkCalccompavgs;

	private CheckBox checkMatchReplay;

	private CheckBox checkClausuraSchedule;

	private NumericUpDown numericStartYear;

	private Label label13;

	private GroupBox groupBox2;

	private ComboBox comboSpecialTeam1;

	private ComboBox comboSpecialTeam4;

	private ComboBox comboSpecialTeam3;

	private ComboBox comboSpecialTeam2;

	private GroupBox groupStadiums;

	private ComboBox comboStadium12;

	private ComboBox comboStadium11;

	private ComboBox comboStadium10;

	private ComboBox comboStadium9;

	private ComboBox comboStadium8;

	private ComboBox comboStadium7;

	private ComboBox comboStadium6;

	private ComboBox comboStadium5;

	private ComboBox comboStadium4;

	private ComboBox comboStadium3;

	private ComboBox comboStadium2;

	private ComboBox comboStadium1;

	private NumericUpDown numericStageRef;

	private NumericUpDown numericStandingKeep;

	private CheckBox checkStandingKeep;

	private NumericUpDown numericKeepPointsPercentage;

	private CheckBox checkKeepPointsPercentage;

	private ComboBox comboSpecialKo2Rule;

	private CheckBox checkSpecialKo2Rule;

	private ComboBox comboSpecialKo1Rule;

	private CheckBox checkSpecialKo1Rule;

	private NumericUpDown numericRegularSeason;

	private NumericUpDown numericStandingsRank;

	private CheckBox checkStandingsRank;

	private GroupBox groupInfoColors;

	private CheckBox checkInfoColorAdvance;

	private CheckBox checkInfoColorPromotion;

	private CheckBox checkInfoColorPossiblePromotion;

	private CheckBox checkInfoColorRelegation;

	private CheckBox checkInfoColorPossibleRelegation;

	private CheckBox checkInfoColorEuropa;

	private CheckBox checkInfoColorChampions;

	private CheckBox checkInfoColorChamp;

	private Label label12;

	private Label label11;

	private NumericUpDown numericColorAdvanceMax;

	private NumericUpDown numericColorAdvanceMin;

	private NumericUpDown numericColorPromotionMax;

	private NumericUpDown numericColorPromotionMin;

	private NumericUpDown numericColorPossiblePromotionMax;

	private NumericUpDown numericColorPossiblePromotionMin;

	private NumericUpDown numericColorRelegationMax;

	private NumericUpDown numericColorRelegationMin;

	private NumericUpDown numericColorPossibleRelegationMax;

	private NumericUpDown numericColorPossibleRelegationMin;

	private NumericUpDown numericColorEuropaMax;

	private NumericUpDown numericColorEuropaMin;

	private NumericUpDown numericColorChampionsMax;

	private NumericUpDown numericColorChampionsMin;

	private GroupBox groupSlots;

	private NumericUpDown numericPossiblePromotionMax;

	private CheckBox checkInfoPossiblePromotion;

	private NumericUpDown numericPossiblePromotionMin;

	private NumericUpDown numericPromotionMax;

	private NumericUpDown numericPromotionMin;

	private NumericUpDown numericRelegationMax;

	private NumericUpDown numericRelegationMin;

	private NumericUpDown numericPossibleRelegationMax;

	private NumericUpDown numericPossibleRelegationMin;

	private Label label15;

	private Label label16;

	private CheckBox checkInfoPromotion;

	private CheckBox checkInfoRelegation;

	private CheckBox checkInfoPossibleRelegation;

	private CheckBox checkInfoChamp;

	private ComboBox comboLanguageKey;

	private GroupBox groupWeather;

	private Label label28;

	private Label label27;

	private Label label26;

	private Label label25;

	private Label label24;

	private Label label23;

	private Label label22;

	private Label label21;

	private Label label20;

	private Label label19;

	private Label label18;

	private Label label17;

	private ComboBox comboBox23;

	private ComboBox comboBox24;

	private NumericUpDown numericUpDown34;

	private NumericUpDown numericUpDown35;

	private NumericUpDown numericUpDown36;

	private ComboBox comboBox21;

	private ComboBox comboBox22;

	private NumericUpDown numericUpDown31;

	private NumericUpDown numericUpDown32;

	private NumericUpDown numericUpDown33;

	private ComboBox comboBox19;

	private ComboBox comboBox20;

	private NumericUpDown numericUpDown28;

	private NumericUpDown numericUpDown29;

	private NumericUpDown numericUpDown30;

	private ComboBox comboBox17;

	private ComboBox comboBox18;

	private NumericUpDown numericUpDown25;

	private NumericUpDown numericUpDown26;

	private NumericUpDown numericUpDown27;

	private ComboBox comboBox15;

	private ComboBox comboBox16;

	private NumericUpDown numericUpDown22;

	private NumericUpDown numericUpDown23;

	private NumericUpDown numericUpDown24;

	private ComboBox comboBox13;

	private ComboBox comboBox14;

	private NumericUpDown numericUpDown19;

	private NumericUpDown numericUpDown20;

	private NumericUpDown numericUpDown21;

	private ComboBox comboBox11;

	private ComboBox comboBox12;

	private NumericUpDown numericUpDown16;

	private NumericUpDown numericUpDown17;

	private NumericUpDown numericUpDown18;

	private ComboBox comboBox9;

	private ComboBox comboBox10;

	private NumericUpDown numericUpDown13;

	private NumericUpDown numericUpDown14;

	private NumericUpDown numericUpDown15;

	private ComboBox comboBox7;

	private ComboBox comboBox8;

	private NumericUpDown numericUpDown10;

	private NumericUpDown numericUpDown11;

	private NumericUpDown numericUpDown12;

	private ComboBox comboBox5;

	private ComboBox comboBox6;

	private NumericUpDown numericUpDown7;

	private NumericUpDown numericUpDown8;

	private NumericUpDown numericUpDown9;

	private ComboBox comboBox3;

	private ComboBox comboBox4;

	private NumericUpDown numericUpDown4;

	private NumericUpDown numericUpDown5;

	private NumericUpDown numericUpDown6;

	private ComboBox comboBox1;

	private ComboBox comboBox2;

	private NumericUpDown numericUpDown1;

	private NumericUpDown numericUpDown2;

	private NumericUpDown numericUpDown3;

	private ToolStrip toolWeather;

	private ToolStripButton buttonCopyWeather;

	private ToolStripButton buttonPasteWeather;

	private GroupBox groupStageSchedules;

	private TreeView treeStageSchedule;

	private ToolStrip toolStageSchedule;

	private Panel panelStageScheduleDetails;

	private Label label37;

	private Label label36;

	private Label label35;

	private Label label34;

	private ComboBox comboStageTime;

	private NumericUpDown numericStageMaxGames;

	private NumericUpDown numericStageMinGames;

	private DateTimePicker dateStagePicker;

	private GroupBox groupStageScheduleDetails;

	private GroupBox groupGroupScheduke;

	private TreeView treeGroupSchedule;

	private Panel panelGroupScheduleDetails;

	private GroupBox groupGroupScheduleDetails;

	private DateTimePicker dateGroupPicker;

	private Label label38;

	private NumericUpDown numericGroupMinGames;

	private Label label39;

	private NumericUpDown numericGroupMaxGames;

	private Label label40;

	private ComboBox comboGroupTime;

	private Label label41;

	private ToolStrip toolGroupSchedule;

	private NumericUpDown numericNumGames;

	private Label label14;

	private GroupBox groupPlayGroup;

	private ToolStripButton buttonAddTrophy;

	private ToolStripButton buttonDeleteTrophy;

	private ToolStripButton buttonAddStage;

	private ToolStripButton buttonDeleteStage;

	private ToolStripButton buttonAddGroup;

	private ToolStripButton buttonDeleteGroup;

	private ToolStripButton buttonAddNatiom;

	private ToolStripButton buttonDeleteNation;

	private ToolStripButton buttonPasteTrophy;

	private ToolStripButton buttonCopyTrophy;

	private GroupBox groupRules;

	private ToolStrip toolRules;

	private ToolStripButton buttonAddRule;

	private ToolStripButton buttonRemoveRule;

	private ToolStripButton buttonCopyGroupCalendar;

	private ToolStripButton buttonPasteGroupCalendar;

	private ToolStripButton buttonNewGroupLeg;

	private ToolStripButton buttonRemoveGroupLeg;

	private ToolStripButton buttonGroupAddTime;

	private ToolStripButton buttonGroupRemoveTime;

	private ToolStripButton buttonCopyStageCalendar;

	private ToolStripButton buttonPasteStageCalendar;

	private ToolStripButton buttonNeewStageLeg;

	private ToolStripButton buttonDeleteStageLeg;

	private ToolStripButton buttonStageAddTime;

	private ToolStripButton buttonStageRemoveTime;

	private ToolStripButton buttonCleanStageCalendar;

	private ToolStripButton buttonCleanGroupCalendar;

	private Viewer2D viewer2DTrophy256;

	private Label label66;

	private TextBox textUniqueId;

	private Label label67;

	private NumericUpDown numericBall;

	private PictureBox pictureBall;

	private GroupBox group3D;

	private ToolStrip toolNear3D;

	private ToolStripButton buttonShow3DModel;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonImport3DModel;

	private ToolStripButton buttonExport3DModel;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton buttonRemove3DModel;

	private TabControl tabTrophy;

	private TabPage tabPageTrophyStructure;

	private TabPage tabPageTrophyGraphics;

	private GroupBox groupGraphics;

	private MultiViewer2D multiViewer2DTextures;

	private Button buttonReplicateTrophy128;

	private Viewer2D viewer2DTrophy128;

	private GroupBox groupInternationalschedule;

	private NumericUpDown numericInternationalPeriodicity;

	private Label label69;

	private Label label68;

	private NumericUpDown numericInternationalFirstYear;

	private TabPage tabPageRankingTable;

	private GroupBox groupInitTeams;

	private Panel panelInitTeam24;

	private ComboBox comboInitTeam24;

	private Label label65;

	private Panel panelInitTeam23;

	private ComboBox comboInitTeam23;

	private Label label64;

	private Panel panelInitTeam22;

	private ComboBox comboInitTeam22;

	private Label label63;

	private Panel panelInitTeam21;

	private ComboBox comboInitTeam21;

	private Label label62;

	private Panel panelInitTeam20;

	private ComboBox comboInitTeam20;

	private Label label61;

	private Panel panelInitTeam19;

	private ComboBox comboInitTeam19;

	private Label label60;

	private Panel panelInitTeam18;

	private ComboBox comboInitTeam18;

	private Label label59;

	private Panel panelInitTeam17;

	private ComboBox comboInitTeam17;

	private Label label58;

	private Panel panelInitTeam16;

	private ComboBox comboInitTeam16;

	private Label label57;

	private Panel panelInitTeam15;

	private ComboBox comboInitTeam15;

	private Label label56;

	private Panel panelInitTeam14;

	private ComboBox comboInitTeam14;

	private Label label55;

	private Panel panelInitTeam13;

	private ComboBox comboInitTeam13;

	private Label label54;

	private Panel panelInitTeam12;

	private ComboBox comboInitTeam12;

	private Label label53;

	private Panel panelInitTeam11;

	private ComboBox comboInitTeam11;

	private Label label52;

	private Panel panelInitTeam10;

	private ComboBox comboInitTeam10;

	private Label label51;

	private Panel panelInitTeam9;

	private ComboBox comboInitTeam9;

	private Label label50;

	private Panel panelInitTeam8;

	private ComboBox comboInitTeam8;

	private Label label49;

	private Panel panelInitTeam7;

	private ComboBox comboInitTeam7;

	private Label label48;

	private Panel panelInitTeam6;

	private ComboBox comboInitTeam6;

	private Label label47;

	private Panel panelInitTeam5;

	private ComboBox comboInitTeam5;

	private Label label46;

	private Panel panelInitTeam4;

	private ComboBox comboInitTeam4;

	private Label label45;

	private Panel panelInitTeam3;

	private ComboBox comboInitTeam3;

	private Label label44;

	private Panel panelInitTeam2;

	private ComboBox comboInitTeam2;

	private Label label43;

	private Panel panelInitTeam1;

	private ComboBox comboInitTeam1;

	private Label label42;

	private Label labelUpdateTable24;

	private Label labelUpdateTable23;

	private Label labelUpdateTable22;

	private Label labelUpdateTable21;

	private Label labelUpdateTable20;

	private Label labelUpdateTable19;

	private Label labelUpdateTable18;

	private Label labelUpdateTable17;

	private Label labelUpdateTable16;

	private Label labelUpdateTable15;

	private Label labelUpdateTable14;

	private Label labelUpdateTable13;

	private Label labelUpdateTable12;

	private Label labelUpdateTable11;

	private Label labelUpdateTable10;

	private Label labelUpdateTable9;

	private Label labelUpdateTable8;

	private Label labelUpdateTable7;

	private Label labelUpdateTable6;

	private Label labelUpdateTable5;

	private Label labelUpdateTable4;

	private Label labelUpdateTable3;

	private Label labelUpdateTable2;

	private Label labelUpdateTable1;

	private Label label70;

	private NumericUpDown numericUpdateTableEntries;

	private CheckBox checkUpdateLeagueStats;

	private ComboBox comboLeagueStats;

	private CheckBox checkClearLeagueStats;

	private GroupBox groupLeaguetasks;

	private CheckBox checkUpdateLeagueTable;

	private ComboBox comboStageStandingRules;

	private CheckBox checkStageStandingsRules;

	private Label label71;

	private ComboBox comboTrophyStartMonth;

	private CheckBox checkRandomDrawEvent;

	private ToolStripComboBox comboTargetLeague;

	private TextBox textLanguageName;

	private ToolStripButton buttonStageSortLegs;

	private ToolStripButton buttongroupSortLegs;

	private CheckBox checkScheduleUseDates;

	private NumericUpDown numericKeepPointsStageRef;

	private Button buttonReplicateTropy;

	private Viewer2D viewer2DTrophy;

	private TabPage tabPageTrophyRevMod;

	private GroupBox groupTeamAdboardsRevMod;

	private Viewer2D viewer2DTournamentAdboard;

	private GroupBox groupTeamBallRevMod;

	private ToolStrip toolTeamBall3D;

	private ToolStripButton buttonShow3DBall;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripButton buttonImport3DModelTournamentBall;

	private ToolStripButton buttonExport3DModelTournamentBall;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripButton buttonRemove3DModelTournamentBall;

	private MultiViewer2D multiViewer2DTournamentBallTextures;

	private TabPage tabPageTrophyPitchGraphics;

	private Viewer2D viewer2DPitchDressing;

	private NumericUpDown numericUpDown97;

	private NumericUpDown numericUpDown98;

	private NumericUpDown numericUpDown99;

	private NumericUpDown numericUpDown100;

	private NumericUpDown numericUpDown101;

	private NumericUpDown numericUpDown102;

	private NumericUpDown numericUpDown103;

	private NumericUpDown numericUpDown104;

	private NumericUpDown numericUpDown105;

	private NumericUpDown numericUpDown106;

	private NumericUpDown numericUpDown107;

	private NumericUpDown numericUpDown108;

	private NumericUpDown numericUpDown85;

	private NumericUpDown numericUpDown86;

	private NumericUpDown numericUpDown87;

	private NumericUpDown numericUpDown88;

	private NumericUpDown numericUpDown89;

	private NumericUpDown numericUpDown90;

	private NumericUpDown numericUpDown91;

	private NumericUpDown numericUpDown92;

	private NumericUpDown numericUpDown93;

	private NumericUpDown numericUpDown94;

	private NumericUpDown numericUpDown95;

	private NumericUpDown numericUpDown96;

	private NumericUpDown numericUpDown73;

	private NumericUpDown numericUpDown74;

	private NumericUpDown numericUpDown75;

	private NumericUpDown numericUpDown76;

	private NumericUpDown numericUpDown77;

	private NumericUpDown numericUpDown78;

	private NumericUpDown numericUpDown79;

	private NumericUpDown numericUpDown80;

	private NumericUpDown numericUpDown81;

	private NumericUpDown numericUpDown82;

	private NumericUpDown numericUpDown83;

	private NumericUpDown numericUpDown84;

	private NumericUpDown numericUpDown61;

	private NumericUpDown numericUpDown62;

	private NumericUpDown numericUpDown63;

	private NumericUpDown numericUpDown64;

	private NumericUpDown numericUpDown65;

	private NumericUpDown numericUpDown66;

	private NumericUpDown numericUpDown67;

	private NumericUpDown numericUpDown68;

	private NumericUpDown numericUpDown69;

	private NumericUpDown numericUpDown70;

	private NumericUpDown numericUpDown71;

	private NumericUpDown numericUpDown72;

	private NumericUpDown numericUpDown49;

	private NumericUpDown numericUpDown50;

	private NumericUpDown numericUpDown51;

	private NumericUpDown numericUpDown52;

	private NumericUpDown numericUpDown53;

	private NumericUpDown numericUpDown54;

	private NumericUpDown numericUpDown55;

	private NumericUpDown numericUpDown56;

	private NumericUpDown numericUpDown57;

	private NumericUpDown numericUpDown58;

	private NumericUpDown numericUpDown59;

	private NumericUpDown numericUpDown60;

	private NumericUpDown numericUpDown37;

	private NumericUpDown numericUpDown38;

	private NumericUpDown numericUpDown39;

	private NumericUpDown numericUpDown40;

	private NumericUpDown numericUpDown41;

	private NumericUpDown numericUpDown42;

	private NumericUpDown numericUpDown43;

	private NumericUpDown numericUpDown44;

	private NumericUpDown numericUpDown45;

	private NumericUpDown numericUpDown46;

	private NumericUpDown numericUpDown47;

	private NumericUpDown numericUpDown48;

	private Label label80;

	private Label label76;

	private Label label77;

	private Label label78;

	private Label label79;

	private Label label74;

	private Label label75;

	private Label label73;

	private Label label72;

	private Label label30;

	private Label label29;

	private CheckBox checkCanUseFancards;

	private CheckBox checkLowCelebrationLevel;

	private ToolStripButton buttonCreatePatch;

	private ToolStripButton buttonLoadPatch;

	private Panel panelAllInitTeams;

	private Panel panelInitTeam25;

	private Label labelUpdateTable25;

	private ComboBox comboInitTeam25;

	private Label label32;

	private Panel panelInitTeam26;

	private Label labelUpdateTable26;

	private ComboBox comboInitTeam26;

	private Label label33;

	private Panel panelInitTeam27;

	private Label labelUpdateTable27;

	private ComboBox comboInitTeam27;

	private Label label127;

	private Panel panelInitTeam28;

	private Label labelUpdateTable28;

	private ComboBox comboInitTeam28;

	private Label label128;

	private Panel panelInitTeam29;

	private Label labelUpdateTable29;

	private ComboBox comboInitTeam29;

	private Label label129;

	private Panel panelInitTeam30;

	private Label labelUpdateTable30;

	private ComboBox comboInitTeam30;

	private Label label130;

	private Panel panelInitTeam31;

	private Label labelUpdateTable31;

	private ComboBox comboInitTeam31;

	private Label label131;

	private Panel panelInitTeam32;

	private Label labelUpdateTable32;

	private ComboBox comboInitTeam32;

	private Label label132;

	private Panel panelInitTeam33;

	private Label labelUpdateTable33;

	private ComboBox comboInitTeam33;

	private Label label133;

	private Panel panelInitTeam34;

	private Label labelUpdateTable34;

	private ComboBox comboInitTeam34;

	private Label label134;

	private Panel panelInitTeam35;

	private Label labelUpdateTable35;

	private ComboBox comboInitTeam35;

	private Label label135;

	private Panel panelInitTeam36;

	private Label labelUpdateTable36;

	private ComboBox comboInitTeam36;

	private Label label136;

	private Panel panelInitTeam37;

	private Label labelUpdateTable37;

	private ComboBox comboInitTeam37;

	private Label label137;

	private Panel panelInitTeam38;

	private Label labelUpdateTable38;

	private ComboBox comboInitTeam38;

	private Label label138;

	private Panel panelInitTeam39;

	private Label labelUpdateTable39;

	private ComboBox comboInitTeam39;

	private Label label139;

	private Panel panelInitTeam40;

	private Label labelUpdateTable40;

	private ComboBox comboInitTeam40;

	private Label label140;

	private Panel panelInitTeam41;

	private Label labelUpdateTable41;

	private ComboBox comboInitTeam41;

	private Label label141;

	private Panel panelInitTeam42;

	private Label labelUpdateTable42;

	private ComboBox comboInitTeam42;

	private Label label142;

	private Panel panelInitTeam43;

	private Label labelUpdateTable43;

	private ComboBox comboInitTeam43;

	private Label label143;

	private Panel panelInitTeam44;

	private Label labelUpdateTable44;

	private ComboBox comboInitTeam44;

	private Label label144;

	private Panel panelInitTeam45;

	private Label labelUpdateTable45;

	private ComboBox comboInitTeam45;

	private Label label145;

	private Panel panelInitTeam46;

	private Label labelUpdateTable46;

	private ComboBox comboInitTeam46;

	private Label label146;

	private Panel panelInitTeam47;

	private Label labelUpdateTable47;

	private ComboBox comboInitTeam47;

	private Label label147;

	private Panel panelInitTeam48;

	private Label labelUpdateTable48;

	private ComboBox comboInitTeam48;

	private Label label148;

	private TabPage tabPageWipe3D;

	private MultiViewer2D multiViewerWipe;

	private NumericUpDown numericAdvanceFrom;

	private CheckBox checkAdvanceFrom;

	public CompetitionForm()
	{
		base.Visible = false;
		InitializeComponent();
		// Creation Master 26 constructs its section forms before the FC26 database
		// is opened, so FC26 surfaces must be present from the initial shell build.
		// Their data actions remain guarded until a project is loaded.
		textUniqueId.Visible = false;
		label66.Visible = false;
		var compdataPage = new TabPage
		{
			Name = "pageFc26Compdata", Text = "Compdata", UseVisualStyleBackColor = true
		};
		Fc26Compdata = new Fc26CompdataPanel();
		compdataPage.Controls.Add(Fc26Compdata);
		tabCompetitions.TabPages.Add(compdataPage);
		CmStyleDetailsWindow.Attach(this, "Competition Details", DetailSection.Competition,
			() => m_CurrentCompobj?.Id ?? -1);
		viewer3DTrophy = new Viewer3D();
		viewer3DTrophy.AmbientColor = Color.Black;
		viewer3DTrophy.BackColor = Color.Gray;
		viewer3DTrophy.BorderStyle = BorderStyle.Fixed3D;
		viewer3DTrophy.Dock = DockStyle.Fill;
		viewer3DTrophy.LightDirectionX = 0.5f;
		viewer3DTrophy.LightDirectionY = -0.25f;
		viewer3DTrophy.LightDirectionZ = -1f;
		viewer3DTrophy.LightX = -30f;
		viewer3DTrophy.LightY = 10f;
		viewer3DTrophy.LightZ = 30f;
		viewer3DTrophy.Location = new Point(3, 16);
		viewer3DTrophy.Name = "viewer3DTrophy";
		viewer3DTrophy.RotationX = 0f;
		viewer3DTrophy.RotationY = 0f;
		viewer3DTrophy.RotationYCoeff = 0.01f;
		viewer3DTrophy.Size = new Size(439, 270);
		viewer3DTrophy.TabIndex = 1;
		viewer3DTrophy.ViewX = 0f;
		viewer3DTrophy.ViewY = 35f;
		viewer3DTrophy.ViewZ = 105f;
		viewer3DTrophy.ZbufferRenderState = null;
		group3D.Controls.Add(viewer3DTrophy);
		viewer3DTournamentBall = new Viewer3D();
		viewer3DTournamentBall.AmbientColor = Color.Black;
		viewer3DTournamentBall.BackColor = Color.Gray;
		viewer3DTournamentBall.BorderStyle = BorderStyle.Fixed3D;
		viewer3DTournamentBall.LightDirectionX = 0.5f;
		viewer3DTournamentBall.LightDirectionY = -0.25f;
		viewer3DTournamentBall.LightDirectionZ = -1f;
		viewer3DTournamentBall.LightX = -30f;
		viewer3DTournamentBall.LightY = 10f;
		viewer3DTournamentBall.LightZ = 30f;
		viewer3DTournamentBall.Location = new Point(259, 44);
		viewer3DTournamentBall.Name = "viewer3DTournamentBall";
		viewer3DTournamentBall.RotationX = 0f;
		viewer3DTournamentBall.RotationY = 0f;
		viewer3DTournamentBall.RotationYCoeff = 0.01f;
		viewer3DTournamentBall.Size = new Size(256, 256);
		viewer3DTournamentBall.TabIndex = 3;
		viewer3DTournamentBall.ViewX = 0f;
		viewer3DTournamentBall.ViewY = 0f;
		viewer3DTournamentBall.ViewZ = 30f;
		viewer3DTournamentBall.ZbufferRenderState = null;
		groupTeamBallRevMod.Controls.Add(viewer3DTournamentBall);
		for (int num = m_QRLabels.Length - 1; num >= 0; num--)
		{
			m_QRLabels[num] = new Label();
			m_QRLabels[num].Location = new Point(2, 58 + 20 * num);
			m_QRLabels[num].Name = "labelQR" + num;
			m_QRLabels[num].Text = "labelQR" + num;
			m_QRLabels[num].Size = new Size(480, 19);
			m_QRLabels[num].Dock = DockStyle.None;
			m_QRLabels[num].BorderStyle = BorderStyle.None;
			m_QRLabels[num].Cursor = Cursors.Hand;
			panelQualificationRules.Controls.Add(m_QRLabels[num]);
			m_QRLabels[num].Click += labelQR_Click;
		}
		for (int num2 = m_AdvanceLabels.Length - 1; num2 >= 0; num2--)
		{
			m_AdvanceLabels[num2] = new Label();
			m_AdvanceLabels[num2].Location = new Point(4, 28 + 20 * num2);
			m_AdvanceLabels[num2].Name = "labelAdvancemenet" + num2;
			m_AdvanceLabels[num2].Text = "label advancement " + num2;
			m_AdvanceLabels[num2].Size = new Size(120, 19);
			m_AdvanceLabels[num2].Dock = DockStyle.Top;
			m_AdvanceLabels[num2].Cursor = Cursors.Hand;
			panelAdvancement.Controls.Add(m_AdvanceLabels[num2]);
			m_AdvanceLabels[num2].Click += labelAdvance_Click;
		}
		m_UpdateTableLabels[0] = labelUpdateTable1;
		m_UpdateTableLabels[1] = labelUpdateTable2;
		m_UpdateTableLabels[2] = labelUpdateTable3;
		m_UpdateTableLabels[3] = labelUpdateTable4;
		m_UpdateTableLabels[4] = labelUpdateTable5;
		m_UpdateTableLabels[5] = labelUpdateTable6;
		m_UpdateTableLabels[6] = labelUpdateTable7;
		m_UpdateTableLabels[7] = labelUpdateTable8;
		m_UpdateTableLabels[8] = labelUpdateTable9;
		m_UpdateTableLabels[9] = labelUpdateTable10;
		m_UpdateTableLabels[10] = labelUpdateTable11;
		m_UpdateTableLabels[11] = labelUpdateTable12;
		m_UpdateTableLabels[12] = labelUpdateTable13;
		m_UpdateTableLabels[13] = labelUpdateTable14;
		m_UpdateTableLabels[14] = labelUpdateTable15;
		m_UpdateTableLabels[15] = labelUpdateTable16;
		m_UpdateTableLabels[16] = labelUpdateTable17;
		m_UpdateTableLabels[17] = labelUpdateTable18;
		m_UpdateTableLabels[18] = labelUpdateTable19;
		m_UpdateTableLabels[19] = labelUpdateTable20;
		m_UpdateTableLabels[20] = labelUpdateTable21;
		m_UpdateTableLabels[21] = labelUpdateTable22;
		m_UpdateTableLabels[22] = labelUpdateTable23;
		m_UpdateTableLabels[23] = labelUpdateTable24;
		m_UpdateTableLabels[24] = labelUpdateTable25;
		m_UpdateTableLabels[25] = labelUpdateTable26;
		m_UpdateTableLabels[26] = labelUpdateTable27;
		m_UpdateTableLabels[27] = labelUpdateTable28;
		m_UpdateTableLabels[28] = labelUpdateTable29;
		m_UpdateTableLabels[29] = labelUpdateTable30;
		m_UpdateTableLabels[30] = labelUpdateTable31;
		m_UpdateTableLabels[31] = labelUpdateTable32;
		m_UpdateTableLabels[32] = labelUpdateTable33;
		m_UpdateTableLabels[33] = labelUpdateTable34;
		m_UpdateTableLabels[34] = labelUpdateTable35;
		m_UpdateTableLabels[35] = labelUpdateTable36;
		m_UpdateTableLabels[36] = labelUpdateTable37;
		m_UpdateTableLabels[37] = labelUpdateTable38;
		m_UpdateTableLabels[38] = labelUpdateTable39;
		m_UpdateTableLabels[39] = labelUpdateTable40;
		m_UpdateTableLabels[40] = labelUpdateTable41;
		m_UpdateTableLabels[41] = labelUpdateTable42;
		m_UpdateTableLabels[42] = labelUpdateTable43;
		m_UpdateTableLabels[43] = labelUpdateTable44;
		m_UpdateTableLabels[44] = labelUpdateTable45;
		m_UpdateTableLabels[45] = labelUpdateTable46;
		m_UpdateTableLabels[46] = labelUpdateTable47;
		m_UpdateTableLabels[47] = labelUpdateTable48;
		m_SpecialTeamCombos[0] = comboSpecialTeam1;
		m_SpecialTeamCombos[1] = comboSpecialTeam2;
		m_SpecialTeamCombos[2] = comboSpecialTeam3;
		m_SpecialTeamCombos[3] = comboSpecialTeam4;
		m_StadiumCombos[0] = comboStadium1;
		m_StadiumCombos[1] = comboStadium2;
		m_StadiumCombos[2] = comboStadium3;
		m_StadiumCombos[3] = comboStadium4;
		m_StadiumCombos[4] = comboStadium5;
		m_StadiumCombos[5] = comboStadium6;
		m_StadiumCombos[6] = comboStadium7;
		m_StadiumCombos[7] = comboStadium8;
		m_StadiumCombos[8] = comboStadium9;
		m_StadiumCombos[9] = comboStadium10;
		m_StadiumCombos[10] = comboStadium11;
		m_StadiumCombos[11] = comboStadium12;
		m_OvercastProb[0] = numericUpDown1;
		m_SnowProb[0] = numericUpDown2;
		m_RainProb[0] = numericUpDown3;
		m_OvercastProb[1] = numericUpDown4;
		m_SnowProb[1] = numericUpDown5;
		m_RainProb[1] = numericUpDown6;
		m_OvercastProb[2] = numericUpDown7;
		m_SnowProb[2] = numericUpDown8;
		m_RainProb[2] = numericUpDown9;
		m_OvercastProb[3] = numericUpDown10;
		m_SnowProb[3] = numericUpDown11;
		m_RainProb[3] = numericUpDown12;
		m_OvercastProb[4] = numericUpDown13;
		m_SnowProb[4] = numericUpDown14;
		m_RainProb[4] = numericUpDown15;
		m_OvercastProb[5] = numericUpDown16;
		m_SnowProb[5] = numericUpDown17;
		m_RainProb[5] = numericUpDown18;
		m_OvercastProb[6] = numericUpDown19;
		m_SnowProb[6] = numericUpDown20;
		m_RainProb[6] = numericUpDown21;
		m_OvercastProb[7] = numericUpDown22;
		m_SnowProb[7] = numericUpDown23;
		m_RainProb[7] = numericUpDown24;
		m_OvercastProb[8] = numericUpDown25;
		m_SnowProb[8] = numericUpDown26;
		m_RainProb[8] = numericUpDown27;
		m_OvercastProb[9] = numericUpDown28;
		m_SnowProb[9] = numericUpDown29;
		m_RainProb[9] = numericUpDown30;
		m_OvercastProb[10] = numericUpDown31;
		m_SnowProb[10] = numericUpDown32;
		m_RainProb[10] = numericUpDown33;
		m_OvercastProb[11] = numericUpDown34;
		m_SnowProb[11] = numericUpDown35;
		m_RainProb[11] = numericUpDown36;
		m_ClearProb[11] = numericUpDown37;
		m_ClearProb[10] = numericUpDown38;
		m_ClearProb[9] = numericUpDown39;
		m_ClearProb[8] = numericUpDown40;
		m_ClearProb[7] = numericUpDown41;
		m_ClearProb[6] = numericUpDown42;
		m_ClearProb[5] = numericUpDown43;
		m_ClearProb[4] = numericUpDown44;
		m_ClearProb[3] = numericUpDown45;
		m_ClearProb[2] = numericUpDown46;
		m_ClearProb[1] = numericUpDown47;
		m_ClearProb[0] = numericUpDown48;
		m_HazyProb[11] = numericUpDown49;
		m_HazyProb[10] = numericUpDown50;
		m_HazyProb[9] = numericUpDown51;
		m_HazyProb[8] = numericUpDown52;
		m_HazyProb[7] = numericUpDown53;
		m_HazyProb[6] = numericUpDown54;
		m_HazyProb[5] = numericUpDown55;
		m_HazyProb[4] = numericUpDown56;
		m_HazyProb[3] = numericUpDown57;
		m_HazyProb[2] = numericUpDown58;
		m_HazyProb[1] = numericUpDown59;
		m_HazyProb[0] = numericUpDown60;
		m_CloudyProb[11] = numericUpDown61;
		m_CloudyProb[10] = numericUpDown62;
		m_CloudyProb[9] = numericUpDown63;
		m_CloudyProb[8] = numericUpDown64;
		m_CloudyProb[7] = numericUpDown65;
		m_CloudyProb[6] = numericUpDown66;
		m_CloudyProb[5] = numericUpDown67;
		m_CloudyProb[4] = numericUpDown68;
		m_CloudyProb[3] = numericUpDown69;
		m_CloudyProb[2] = numericUpDown70;
		m_CloudyProb[1] = numericUpDown71;
		m_CloudyProb[0] = numericUpDown72;
		m_FoggyProb[11] = numericUpDown73;
		m_FoggyProb[10] = numericUpDown74;
		m_FoggyProb[9] = numericUpDown75;
		m_FoggyProb[8] = numericUpDown76;
		m_FoggyProb[7] = numericUpDown77;
		m_FoggyProb[6] = numericUpDown78;
		m_FoggyProb[5] = numericUpDown79;
		m_FoggyProb[4] = numericUpDown80;
		m_FoggyProb[3] = numericUpDown81;
		m_FoggyProb[2] = numericUpDown82;
		m_FoggyProb[1] = numericUpDown83;
		m_FoggyProb[0] = numericUpDown84;
		m_ShowersProb[11] = numericUpDown85;
		m_ShowersProb[10] = numericUpDown86;
		m_ShowersProb[9] = numericUpDown87;
		m_ShowersProb[8] = numericUpDown88;
		m_ShowersProb[7] = numericUpDown89;
		m_ShowersProb[6] = numericUpDown90;
		m_ShowersProb[5] = numericUpDown91;
		m_ShowersProb[4] = numericUpDown92;
		m_ShowersProb[3] = numericUpDown93;
		m_ShowersProb[2] = numericUpDown94;
		m_ShowersProb[1] = numericUpDown95;
		m_ShowersProb[0] = numericUpDown96;
		m_FlurriesProb[11] = numericUpDown97;
		m_FlurriesProb[10] = numericUpDown98;
		m_FlurriesProb[9] = numericUpDown99;
		m_FlurriesProb[8] = numericUpDown100;
		m_FlurriesProb[7] = numericUpDown101;
		m_FlurriesProb[6] = numericUpDown102;
		m_FlurriesProb[5] = numericUpDown103;
		m_FlurriesProb[4] = numericUpDown104;
		m_FlurriesProb[3] = numericUpDown105;
		m_FlurriesProb[2] = numericUpDown106;
		m_FlurriesProb[1] = numericUpDown107;
		m_FlurriesProb[0] = numericUpDown108;
		for (int i = 0; i < 12; i++)
		{
			m_OvercastProb[i].ValueChanged += weatherProb_ValueChanged;
			m_SnowProb[i].ValueChanged += weatherProb_ValueChanged;
			m_RainProb[i].ValueChanged += weatherProb_ValueChanged;
			m_ClearProb[i].ValueChanged += weatherProb_ValueChanged;
			m_HazyProb[i].ValueChanged += weatherProb_ValueChanged;
			m_CloudyProb[i].ValueChanged += weatherProb_ValueChanged;
			m_FoggyProb[i].ValueChanged += weatherProb_ValueChanged;
			m_ShowersProb[i].ValueChanged += weatherProb_ValueChanged;
			m_FlurriesProb[i].ValueChanged += weatherProb_ValueChanged;
		}
		m_NightTime[0] = comboBox1;
		m_SunsetTime[0] = comboBox2;
		m_NightTime[1] = comboBox3;
		m_SunsetTime[1] = comboBox4;
		m_NightTime[2] = comboBox5;
		m_SunsetTime[2] = comboBox6;
		m_NightTime[3] = comboBox7;
		m_SunsetTime[3] = comboBox8;
		m_NightTime[4] = comboBox9;
		m_SunsetTime[4] = comboBox10;
		m_NightTime[5] = comboBox11;
		m_SunsetTime[5] = comboBox12;
		m_NightTime[6] = comboBox13;
		m_SunsetTime[6] = comboBox14;
		m_NightTime[7] = comboBox15;
		m_SunsetTime[7] = comboBox16;
		m_NightTime[8] = comboBox17;
		m_SunsetTime[8] = comboBox18;
		m_NightTime[9] = comboBox19;
		m_SunsetTime[9] = comboBox20;
		m_NightTime[10] = comboBox21;
		m_SunsetTime[10] = comboBox22;
		m_NightTime[11] = comboBox23;
		m_SunsetTime[11] = comboBox24;
		for (int j = 0; j < 12; j++)
		{
			m_NightTime[j].SelectedIndexChanged += dayTime_SelectedIndexChanged;
			m_SunsetTime[j].SelectedIndexChanged += dayTime_SelectedIndexChanged;
		}
		m_InitTeamPanel[0] = panelInitTeam1;
		m_InitTeamPanel[1] = panelInitTeam2;
		m_InitTeamPanel[2] = panelInitTeam3;
		m_InitTeamPanel[3] = panelInitTeam4;
		m_InitTeamPanel[4] = panelInitTeam5;
		m_InitTeamPanel[5] = panelInitTeam6;
		m_InitTeamPanel[6] = panelInitTeam7;
		m_InitTeamPanel[7] = panelInitTeam8;
		m_InitTeamPanel[8] = panelInitTeam9;
		m_InitTeamPanel[9] = panelInitTeam10;
		m_InitTeamPanel[10] = panelInitTeam11;
		m_InitTeamPanel[11] = panelInitTeam12;
		m_InitTeamPanel[12] = panelInitTeam13;
		m_InitTeamPanel[13] = panelInitTeam14;
		m_InitTeamPanel[14] = panelInitTeam15;
		m_InitTeamPanel[15] = panelInitTeam16;
		m_InitTeamPanel[16] = panelInitTeam17;
		m_InitTeamPanel[17] = panelInitTeam18;
		m_InitTeamPanel[18] = panelInitTeam19;
		m_InitTeamPanel[19] = panelInitTeam20;
		m_InitTeamPanel[20] = panelInitTeam21;
		m_InitTeamPanel[21] = panelInitTeam22;
		m_InitTeamPanel[22] = panelInitTeam23;
		m_InitTeamPanel[23] = panelInitTeam24;
		m_InitTeamPanel[24] = panelInitTeam25;
		m_InitTeamPanel[25] = panelInitTeam26;
		m_InitTeamPanel[26] = panelInitTeam27;
		m_InitTeamPanel[27] = panelInitTeam28;
		m_InitTeamPanel[28] = panelInitTeam29;
		m_InitTeamPanel[29] = panelInitTeam30;
		m_InitTeamPanel[30] = panelInitTeam31;
		m_InitTeamPanel[31] = panelInitTeam32;
		m_InitTeamPanel[32] = panelInitTeam33;
		m_InitTeamPanel[33] = panelInitTeam34;
		m_InitTeamPanel[34] = panelInitTeam35;
		m_InitTeamPanel[35] = panelInitTeam36;
		m_InitTeamPanel[36] = panelInitTeam37;
		m_InitTeamPanel[37] = panelInitTeam38;
		m_InitTeamPanel[38] = panelInitTeam39;
		m_InitTeamPanel[39] = panelInitTeam40;
		m_InitTeamPanel[40] = panelInitTeam41;
		m_InitTeamPanel[41] = panelInitTeam42;
		m_InitTeamPanel[42] = panelInitTeam43;
		m_InitTeamPanel[43] = panelInitTeam44;
		m_InitTeamPanel[44] = panelInitTeam45;
		m_InitTeamPanel[45] = panelInitTeam46;
		m_InitTeamPanel[46] = panelInitTeam47;
		m_InitTeamPanel[47] = panelInitTeam48;
		m_InitTeamCombo[0] = comboInitTeam1;
		m_InitTeamCombo[1] = comboInitTeam2;
		m_InitTeamCombo[2] = comboInitTeam3;
		m_InitTeamCombo[3] = comboInitTeam4;
		m_InitTeamCombo[4] = comboInitTeam5;
		m_InitTeamCombo[5] = comboInitTeam6;
		m_InitTeamCombo[6] = comboInitTeam7;
		m_InitTeamCombo[7] = comboInitTeam8;
		m_InitTeamCombo[8] = comboInitTeam9;
		m_InitTeamCombo[9] = comboInitTeam10;
		m_InitTeamCombo[10] = comboInitTeam11;
		m_InitTeamCombo[11] = comboInitTeam12;
		m_InitTeamCombo[12] = comboInitTeam13;
		m_InitTeamCombo[13] = comboInitTeam14;
		m_InitTeamCombo[14] = comboInitTeam15;
		m_InitTeamCombo[15] = comboInitTeam16;
		m_InitTeamCombo[16] = comboInitTeam17;
		m_InitTeamCombo[17] = comboInitTeam18;
		m_InitTeamCombo[18] = comboInitTeam19;
		m_InitTeamCombo[19] = comboInitTeam20;
		m_InitTeamCombo[20] = comboInitTeam21;
		m_InitTeamCombo[21] = comboInitTeam22;
		m_InitTeamCombo[22] = comboInitTeam23;
		m_InitTeamCombo[23] = comboInitTeam24;
		m_InitTeamCombo[24] = comboInitTeam25;
		m_InitTeamCombo[25] = comboInitTeam26;
		m_InitTeamCombo[26] = comboInitTeam27;
		m_InitTeamCombo[27] = comboInitTeam28;
		m_InitTeamCombo[28] = comboInitTeam29;
		m_InitTeamCombo[29] = comboInitTeam30;
		m_InitTeamCombo[30] = comboInitTeam31;
		m_InitTeamCombo[31] = comboInitTeam32;
		m_InitTeamCombo[32] = comboInitTeam33;
		m_InitTeamCombo[33] = comboInitTeam34;
		m_InitTeamCombo[34] = comboInitTeam35;
		m_InitTeamCombo[35] = comboInitTeam36;
		m_InitTeamCombo[36] = comboInitTeam37;
		m_InitTeamCombo[37] = comboInitTeam38;
		m_InitTeamCombo[38] = comboInitTeam39;
		m_InitTeamCombo[39] = comboInitTeam40;
		m_InitTeamCombo[40] = comboInitTeam41;
		m_InitTeamCombo[41] = comboInitTeam42;
		m_InitTeamCombo[42] = comboInitTeam43;
		m_InitTeamCombo[43] = comboInitTeam44;
		m_InitTeamCombo[44] = comboInitTeam45;
		m_InitTeamCombo[45] = comboInitTeam46;
		m_InitTeamCombo[46] = comboInitTeam47;
		m_InitTeamCombo[47] = comboInitTeam48;
		for (int k = 0; k < m_InitTeamCombo.Length; k++)
		{
			m_InitTeamCombo[k].SelectedIndexChanged += comboInitTeam_SelectedIndexChanged;
		}
		for (int l = 0; l < 48; l++)
		{
			m_InitTeamPanel[l].Visible = false;
		}
		viewer2DTrophy.ImageImport = ImportImageTrophy;
		viewer2DTrophy.ImageDelete = DeleteTrophy;
		viewer2DTrophy.ButtonStripVisible = true;
		viewer2DTrophy.RemoveButton = true;
		viewer2DTrophy256.ImageImport = ImportImageTrophy256;
		viewer2DTrophy256.ImageDelete = DeleteTrophy256;
		viewer2DTrophy256.ButtonStripVisible = true;
		viewer2DTrophy256.RemoveButton = true;
		viewer2DTrophy128.ImageImport = ImportImageTrophySmall;
		viewer2DTrophy128.ImageDelete = DeleteTrophySmall;
		viewer2DTrophy128.ButtonStripVisible = true;
		viewer2DTrophy128.RemoveButton = true;
		multiViewer2DTextures.Rx3ExportDelegate = ExportRx3TrophyTextures;
		multiViewer2DTextures.Rx3ImportDelegate = ImportRx3TrophyTextures;
		multiViewer2DTextures.Rx3SaveDelegate = SaveRx3TrophyTextures;
		multiViewerWipe.Rx3ExportDelegate = ExportRx3WipeTextures;
		multiViewerWipe.Rx3ImportDelegate = ImportRx3WipeTextures;
		multiViewerWipe.Rx3SaveDelegate = SaveRx3WipeTextures;
		viewer2DTournamentAdboard.ButtonStripVisible = true;
		viewer2DTournamentAdboard.FullSizeButton = true;
		viewer2DTournamentAdboard.ImageImport = ImportImageAdboard;
		viewer2DTournamentAdboard.ImageDelete = DeleteTrophyAdboard;
		viewer2DPitchDressing.ButtonStripVisible = true;
		viewer2DPitchDressing.FullSizeButton = true;
		viewer2DPitchDressing.RemoveButton = true;
		viewer2DPitchDressing.ImageImport = ImportImagePitchDressing;
		viewer2DPitchDressing.ImageDelete = DeletePitchDressing;
	}

	internal void MakeLeagueInGameReady(League league)
	{
		var page = tabCompetitions.TabPages.Cast<TabPage>()
			.FirstOrDefault(item => item.Name == "pageFc26Compdata");
		if (page != null) tabCompetitions.SelectedTab = page;
		Fc26Compdata.MakeLeagueInGameReady(league);
	}

	internal string StageLeagueForSave(League league) => Fc26Compdata.StageLeagueForSave(league);

	internal void SelectFc26Compdata()
	{
		var page = tabCompetitions.TabPages.Cast<TabPage>()
			.FirstOrDefault(item => item.Name == "pageFc26Compdata");
		if (page != null) tabCompetitions.SelectedTab = page;
	}

	public void Clean()
	{
		base.Visible = false;
	}

	private Trophy SelectTrophy(object sender, object obj)
	{
		Trophy trophy = (Trophy)obj;
		Refresh();
		LoadTrophy(trophy);
		return trophy;
	}

	private bool ImportImageTrophy(object sender, Bitmap bitmap)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.ImportImage(this, m_CurrentTrophy.TrophyDdsFileName(), bitmap,
				bitmap.Width, bitmap.Height, "Trophy menu image");
		return m_CurrentTrophy.SetTrophy(bitmap);
	}

	private bool DeleteTrophy(object sender)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.Remove(this, m_CurrentTrophy.TrophyDdsFileName(), "Trophy menu image");
		return m_CurrentTrophy.DeleteTrophy();
	}

	private bool ImportImageAdboard(object sender, Bitmap bitmap)
	{
		return m_CurrentTrophy.SetAdboard(bitmap);
	}

	private bool DeleteTrophyAdboard(object sender)
	{
		return m_CurrentTrophy.DeleteAdboard();
	}

	private bool ImportImagePitchDressing(object sender, Bitmap bitmap)
	{
		return m_CurrentTrophy.SetPitchDressing(bitmap);
	}

	private bool DeletePitchDressing(object sender)
	{
		return m_CurrentTrophy.DeletePitchDressing();
	}

	private bool ImportImageTrophy256(object sender, Bitmap bitmap)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.ImportImage(this, m_CurrentTrophy.TrophyDdsFileName256(), bitmap,
				bitmap.Width, bitmap.Height, "Trophy 256 image");
		return m_CurrentTrophy.SetTrophy256(bitmap);
	}

	private bool DeleteTrophy256(object sender)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.Remove(this, m_CurrentTrophy.TrophyDdsFileName256(), "Trophy 256 image");
		return m_CurrentTrophy.DeleteTrophy256();
	}

	private bool ImportImageTrophySmall(object sender, Bitmap bitmap)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.ImportImage(this, m_CurrentTrophy.TrophyDdsFileName128(), bitmap,
				bitmap.Width, bitmap.Height, "Trophy 128 image");
		return m_CurrentTrophy.SetTrophy128(bitmap);
	}

	private bool DeleteTrophySmall(object sender)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.Remove(this, m_CurrentTrophy.TrophyDdsFileName128(), "Trophy 128 image");
		return m_CurrentTrophy.DeleteTrophy128();
	}

	private bool ExportRx3TrophyTextures(object sender, string exportDir)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.Export(this, m_CurrentTrophy.TexturesFileName(), exportDir, "Trophy texture container");
		return FifaEnvironment.ExportFileFromZdata(m_CurrentTrophy.TexturesFileName(), exportDir);
	}

	private bool SaveRx3TrophyTextures(object sender, Bitmap[] bitmaps)
	{
		bool num = m_CurrentTrophy.SetTextures(bitmaps);
		if (num)
		{
			ReloadTrophy(m_CurrentTrophy);
		}
		return num;
	}

	private bool ImportRx3TrophyTextures(object sender, string rx3FileName)
	{
		bool num = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Import(this, m_CurrentTrophy.TexturesFileName(), rx3FileName, "Trophy texture container")
			: m_CurrentTrophy.SetTextures(rx3FileName);
		if (num)
		{
			ReloadTrophy(m_CurrentTrophy);
		}
		return num;
	}

	private bool ExportRx3WipeTextures(object sender, string exportDir)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.Export(this, m_CurrentTrophy.Wipe3DFileName(), exportDir, "Tournament wipe texture container");
		return FifaEnvironment.ExportFileFromZdata(m_CurrentTrophy.Wipe3DFileName(), exportDir);
	}

	private bool ImportRx3WipeTextures(object sender, string rx3FileName)
	{
		bool num = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Import(this, m_CurrentTrophy.Wipe3DFileName(), rx3FileName, "Tournament wipe texture container")
			: m_CurrentTrophy.SetWipe3DTextures(rx3FileName);
		if (num)
		{
			ReloadTrophy(m_CurrentTrophy);
		}
		return num;
	}

	private bool SaveRx3WipeTextures(object sender, Bitmap[] bitmaps)
	{
		bool num = m_CurrentTrophy.SetWipe3DTextures(bitmaps);
		if (num)
		{
			ReloadTrophy(m_CurrentTrophy);
		}
		return num;
	}

	private bool ImportImageTextures(object sender, Bitmap[] bitmaps)
	{
		if (m_CurrentTrophy == null)
		{
			return false;
		}
		bool result = Ball.SetRevModTrophyBallTextures(m_CurrentTrophy.Settings.m_asset_id, bitmaps);
		LoadRevModBall();
		return result;
	}

	private bool ExportFshTexture(object sender)
	{
		if (m_CurrentTrophy == null)
		{
			return false;
		}
		string logicalPath = Ball.RevModTrophyBallTextureFileName(m_CurrentTrophy.Settings.m_asset_id);
		return FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.ExportWithDialog(this, logicalPath, ref m_TrophyCurrentFolder, "Tournament ball texture")
			: FifaEnvironment.AskAndExportFromZdata(logicalPath, ref m_TrophyCurrentFolder);
	}

	private bool DeleteTexture(object sender)
	{
		if (m_CurrentTrophy == null)
		{
			return false;
		}
		string logicalPath = Ball.RevModTrophyBallTextureFileName(m_CurrentTrophy.Settings.m_asset_id);
		bool result = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Remove(this, logicalPath, "Tournament ball texture")
			: Ball.DeleteRevModTrophyBallTextures(m_CurrentTrophy.Settings.m_asset_id);
		LoadRevModBall();
		return result;
	}

	private void buttonImportRevModBall3DModel_Click(object sender, EventArgs e)
	{
		if (m_CurrentTrophy != null)
		{
			string text = FifaEnvironment.BrowseAndCheckModel(ref m_TrophyCurrentFolder, "Open 3D Ball Model file", "3D ball model files (*.rx3)|ball_*.rx3");
			if (text != null)
			{
				string logicalPath = Ball.RevModTrophyBallModelFileName(m_CurrentTrophy.Settings.m_asset_id);
				bool result = FifaEnvironment.Year == 26
					? Fc26DirectAssetUi.Import(this, logicalPath, text, "Tournament ball 3D model")
					: Ball.SetRevModTrophyBallModel(m_CurrentTrophy.Settings.m_asset_id, text);
				if (result) LoadRevModBall();
			}
		}
	}

	private void buttonExportRevModBall3DModel_Click(object sender, EventArgs e)
	{
		if (m_CurrentTrophy != null)
		{
			string text = Ball.RevModTrophyBallModelFileName(m_CurrentTrophy.Settings.m_asset_id);
			if (text != null)
			{
				if (FifaEnvironment.Year == 26)
					Fc26DirectAssetUi.ExportWithDialog(this, text, ref m_TrophyCurrentFolder, "Tournament ball 3D model");
				else
					FifaEnvironment.AskAndExportFromZdata(text, ref m_TrophyCurrentFolder);
			}
		}
	}

	private void buttonRemoveRevModBall3DModel_Click(object sender, EventArgs e)
	{
		if (m_CurrentTrophy != null)
		{
			string logicalPath = Ball.RevModTrophyBallModelFileName(m_CurrentTrophy.Settings.m_asset_id);
			bool result = FifaEnvironment.Year == 26
				? Fc26DirectAssetUi.Remove(this, logicalPath, "Tournament ball 3D model")
				: Ball.DeleteRevModTrophyBallModel(m_CurrentTrophy.Settings.m_asset_id);
			if (result) LoadRevModBall();
		}
	}

	private void buttonShowRevModBall3DModel_Click(object sender, EventArgs e)
	{
		Show3DRevModBall();
	}

	public void Show3DRevModBall()
	{
		if (!buttonShow3DBall.Checked)
		{
			viewer3DTournamentBall.ShowEmpty();
		}
		else if (m_CurrentTrophy != null)
		{
			Bitmap[] revModTrophyBallTextures = Ball.GetRevModTrophyBallTextures(m_CurrentTrophy.Settings.m_asset_id);
			Bitmap bitmap = null;
			if (revModTrophyBallTextures != null)
			{
				bitmap = GraphicUtil.EmbossBitmap(revModTrophyBallTextures[0], revModTrophyBallTextures[1]);
			}
			Rx3File revModTrophyBallModel = Ball.GetRevModTrophyBallModel(m_CurrentTrophy.Settings.m_asset_id);
			if (bitmap == null || revModTrophyBallModel == null)
			{
				viewer3DTournamentBall.Clean(1);
				viewer3DTournamentBall.Render();
				return;
			}
			Rx3IndexArray.TriangleListType = Rx3IndexArray.ETriangleListType.InvertEven;
			Model3D model3D = new Model3D(revModTrophyBallModel.Rx3IndexArrays[0], revModTrophyBallModel.Rx3VertexArrays[0], bitmap);
			viewer3DTournamentBall.Clean(1);
			viewer3DTournamentBall.SetMesh(0, model3D);
			viewer3DTournamentBall.Render();
		}
	}

	private void LoadRevModBall()
	{
		if (m_IsLoaded && m_CurrentTrophy != null)
		{
			multiViewer2DTournamentBallTextures.Bitmaps = Ball.GetRevModTrophyBallTextures(m_CurrentTrophy.Settings.m_asset_id);
			Show3DRevModBall();
		}
	}

	public void LoadCompetitions()
	{
		WorldStructureToPanel();
	}

	public void LoadTrophy(Trophy trophy)
	{
		if (m_IsLoaded && m_CurrentTrophy != trophy)
		{
			m_Locked = true;
			m_CurrentTrophy = trophy;
			m_Locked = false;
			TrophyToPanel();
		}
	}

	public void Preset()
	{
		if (FifaEnvironment.Year == 14)
		{
			viewer2DTrophy128.Visible = true;
			viewer2DTrophy.Visible = false;
			buttonReplicateTrophy128.Visible = true;
			buttonReplicateTropy.Visible = false;
		}
		else
		{
			viewer2DTrophy128.Visible = false;
			viewer2DTrophy.Visible = true;
			buttonReplicateTrophy128.Visible = false;
			buttonReplicateTropy.Visible = true;
		}
		m_NewIdCreator.IdList = FifaEnvironment.CompetitionObjects.Trophies;
		Schedule.s_BaseDate = new DateTime(2014, 12, 28, 0, 0, 0);
		if (comboCountry.Items.Count != FifaEnvironment.Countries.Count)
		{
			comboCountry.Items.Clear();
			comboCountry.Items.AddRange(FifaEnvironment.Countries.ToArray());
		}
		if (comboPromotionLeague.Items.Count != FifaEnvironment.Leagues.Count + 1)
		{
			comboPromotionLeague.Items.Clear();
			comboPromotionLeague.Items.Add("None");
			comboPromotionLeague.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		}
		if (comboRelegationLeague.Items.Count != FifaEnvironment.Leagues.Count + 1)
		{
			comboRelegationLeague.Items.Clear();
			comboRelegationLeague.Items.Add("None");
			comboRelegationLeague.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		}
		if (comboTargetLeague.Items.Count != FifaEnvironment.Leagues.Count + 1)
		{
			comboTargetLeague.Items.Clear();
			comboTargetLeague.Items.Add("None");
			comboTargetLeague.Items.AddRange(FifaEnvironment.Leagues.ToArray());
			comboTargetLeague.SelectedIndex = 0;
		}
		if (comboLeagueStats.Items.Count != FifaEnvironment.Leagues.Count + 1)
		{
			comboLeagueStats.Items.Clear();
			comboLeagueStats.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		}
		if (comboSpecialTeam1.Items.Count != FifaEnvironment.Teams.Count + 1)
		{
			for (int i = 0; i < 4; i++)
			{
				m_SpecialTeamCombos[i].Items.Clear();
				m_SpecialTeamCombos[i].Items.Add("<None>");
				m_SpecialTeamCombos[i].Items.AddRange(FifaEnvironment.Teams.ToArray());
			}
		}
		if (comboStadium1.Items.Count != FifaEnvironment.Stadiums.Count)
		{
			for (int j = 0; j < 12; j++)
			{
				m_StadiumCombos[j].Items.Clear();
				m_StadiumCombos[j].Items.Add("<Auto>");
				m_StadiumCombos[j].Items.AddRange(FifaEnvironment.Stadiums.ToArray());
			}
		}
		if (comboSchedForce.Items.Count != FifaEnvironment.CompetitionObjects.Trophies.Count)
		{
			comboSchedForce.Items.Clear();
			comboSchedForce.Items.AddRange(FifaEnvironment.CompetitionObjects.Trophies.ToArray());
		}
		if (comboLanguageKey.Items.Count != CompobjList.s_Descriptions.Count)
		{
			comboLanguageKey.Items.Clear();
			comboLanguageKey.Items.AddRange(CompobjList.s_Descriptions.ToArray());
		}
		if (m_InitTeamCombo[0].Items.Count != FifaEnvironment.Teams.Count + 1)
		{
			for (int k = 0; k < m_InitTeamCombo.Length; k++)
			{
				m_InitTeamCombo[k].Items.Clear();
				m_InitTeamCombo[k].Items.Add("<Unknown>");
				m_InitTeamCombo[k].Items.AddRange(FifaEnvironment.Teams.ToArray());
			}
		}
		m_Competitions = FifaEnvironment.CompetitionObjects;
		m_CurrentWorld = m_Competitions.World;
		numericBall.Maximum = FifaEnvironment.FifaDb != null
			? FifaEnvironment.FifaDb.Table[TI.teamballs].TableDescriptor.MaxValues[FI.teamballs_ballid]
			: Math.Max(200000, FifaEnvironment.Balls?.MaxId ?? 0);
	}

	private void CompetitionsForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
		LoadCompetitions();
	}

	public void ReloadCompetitions()
	{
		Preset();
		CompetitionToPanel();
	}

	public void ReloadTrophy(Trophy trophy)
	{
		m_CurrentTrophy = null;
		LoadTrophy(trophy);
	}

	public void WorldStructureToPanel()
	{
		treeWorld.Nodes.Clear();
		TreeNode treeNode = treeWorld.Nodes.Add(m_CurrentWorld.ToString());
		treeNode.Tag = m_CurrentWorld;
		treeNode.ForeColor = Color.Black;
		foreach (Trophy trophy4 in m_CurrentWorld.Trophies)
		{
			TreeNode treeNode2 = treeNode.Nodes.Add(trophy4.ToString());
			treeNode2.Tag = trophy4;
			treeNode2.ForeColor = Color.DarkGreen;
			foreach (Stage stage4 in trophy4.Stages)
			{
				TreeNode treeNode3 = treeNode2.Nodes.Add(stage4.ToString());
				treeNode3.Tag = stage4;
				treeNode3.ForeColor = Color.Magenta;
				foreach (Group group in stage4.Groups)
				{
					TreeNode treeNode4 = treeNode3.Nodes.Add(group.ToString());
					treeNode4.Tag = group;
					treeNode4.ForeColor = Color.Brown;
				}
			}
		}
		foreach (Confederation confederation in m_CurrentWorld.Confederations)
		{
			TreeNode treeNode5 = treeNode.Nodes.Add(confederation.ToString());
			treeNode5.Tag = confederation;
			treeNode5.ForeColor = Color.Red;
			foreach (Trophy trophy5 in confederation.Trophies)
			{
				TreeNode treeNode6 = treeNode5.Nodes.Add(trophy5.ToString());
				treeNode6.Tag = trophy5;
				treeNode6.ForeColor = Color.DarkGreen;
				foreach (Stage stage5 in trophy5.Stages)
				{
					TreeNode treeNode7 = treeNode6.Nodes.Add(stage5.ToString());
					treeNode7.Tag = stage5;
					treeNode7.ForeColor = Color.Magenta;
					foreach (Group group2 in stage5.Groups)
					{
						TreeNode treeNode8 = treeNode7.Nodes.Add(group2.ToString());
						treeNode8.Tag = group2;
						treeNode8.ForeColor = Color.Brown;
						foreach (Group group3 in group2.Groups)
						{
							TreeNode treeNode9 = treeNode8.Nodes.Add(group3.ToString());
							treeNode9.Tag = group3;
							treeNode9.ForeColor = Color.Brown;
						}
					}
				}
			}
			foreach (Nation nation in confederation.Nations)
			{
				TreeNode treeNode10 = treeNode5.Nodes.Add(nation.ToString());
				treeNode10.Tag = nation;
				treeNode10.ForeColor = Color.Blue;
				foreach (Trophy trophy6 in nation.Trophies)
				{
					TreeNode treeNode11 = treeNode10.Nodes.Add(trophy6.ToString());
					treeNode11.Tag = trophy6;
					treeNode11.ForeColor = Color.DarkGreen;
					foreach (Stage stage6 in trophy6.Stages)
					{
						TreeNode treeNode12 = treeNode11.Nodes.Add(stage6.ToString());
						treeNode12.Tag = stage6;
						treeNode12.ForeColor = Color.Magenta;
						foreach (Group group4 in stage6.Groups)
						{
							TreeNode treeNode13 = treeNode12.Nodes.Add(group4.ToString());
							treeNode13.Tag = group4;
							treeNode13.ForeColor = Color.Brown;
							foreach (Group group5 in group4.Groups)
							{
								TreeNode treeNode14 = treeNode13.Nodes.Add(group5.ToString());
								treeNode14.Tag = group5;
								treeNode14.ForeColor = Color.Brown;
							}
						}
					}
				}
			}
		}
		treeWorld.SelectedNode = treeWorld.Nodes[0];
	}

	public void WorldToPanel()
	{
		panelCompObj.Enabled = true;
		textUniqueId.Text = m_CurrentWorld.Id.ToString();
		textFourCharName.Text = m_CurrentWorld.TypeString;
		textLanguageKey.Text = m_CurrentWorld.Description;
		textLanguageName.Text = m_CurrentWorld.Description;
		textFourCharName.Enabled = false;
		textLanguageKey.Enabled = false;
		textLanguageName.Enabled = false;
		comboLanguageKey.Visible = false;
		numericStartYear.Value = m_CurrentWorld.Settings.m_schedule_year_start;
	}

	public void ConfederationToPanel()
	{
		if (m_CurrentConfederation == null)
		{
			panelCompObj.Enabled = false;
			groupConfederation.Visible = false;
			return;
		}
		m_Locked = true;
		groupConfederation.Visible = true;
		panelCompObj.Enabled = true;
		textUniqueId.Text = m_CurrentConfederation.Id.ToString();
		textFourCharName.Text = m_CurrentConfederation.TypeString;
		textLanguageKey.Text = m_CurrentConfederation.Description;
		textLanguageName.Text = string.Empty;
		textFourCharName.Enabled = false;
		textLanguageKey.Enabled = false;
		textLanguageName.Enabled = false;
		comboLanguageKey.Visible = false;
		groupConfederation.Text = "Confederation: " + m_CurrentConfederation.ToString();
		comboConfederationStartingMonth.Text = m_CurrentConfederation.Settings.GetProperty("schedule_seasonstartmonth", 0, out var _);
		m_Locked = false;
	}

	public void NationToPanel()
	{
		m_Locked = true;
		if (m_CurrentNation == null)
		{
			panelCompObj.Enabled = false;
			groupNation.Visible = false;
			return;
		}
		groupNation.Visible = true;
		panelCompObj.Enabled = true;
		textUniqueId.Text = m_CurrentNation.Id.ToString();
		textFourCharName.Text = m_CurrentNation.TypeString;
		textLanguageKey.Text = m_CurrentNation.Description;
		if (m_CurrentNation.Country != null)
		{
			textLanguageName.Text = m_CurrentNation.Country.LanguageName;
		}
		else
		{
			textLanguageName.Text = null;
		}
		textFourCharName.Enabled = true;
		textLanguageKey.Enabled = false;
		textLanguageName.Enabled = false;
		comboLanguageKey.Visible = false;
		groupNation.Text = "Nation: " + m_CurrentNation.ToString();
		comboCountry.SelectedItem = m_CurrentNation.Country;
		if (m_CurrentNation.Country == null)
		{
			comboCountry.Text = string.Empty;
		}
		comboNationStartMonth.Visible = true;
		comboNationStartMonth.Text = m_CurrentNation.Settings.GetProperty("schedule_seasonstartmonth", 0, out var isSpecific);
		numericNationYellowsStored.Visible = true;
		int num = Convert.ToInt32(m_CurrentNation.Settings.GetProperty("rule_numyellowstored", 0, out isSpecific));
		if (num < (int)numericNationYellowsStored.Minimum)
		{
			num = (int)numericNationYellowsStored.Minimum;
			m_CurrentNation.Settings.SetProperty("rule_numyellowstored", 0, num.ToString());
		}
		numericNationYellowsStored.Value = Convert.ToInt32(m_CurrentNation.Settings.GetProperty("rule_numyellowstored", 0, out isSpecific));
		isSpecific = m_CurrentNation.Settings.m_StandingsSort >= 0;
		comboNationStandingsRules.Visible = isSpecific;
		if (isSpecific)
		{
			comboNationStandingsRules.SelectedIndex = m_CurrentNation.Settings.m_StandingsSort;
		}
		checkNationStandingsRules.Checked = isSpecific;
		for (int i = 0; i < 12; i++)
		{
			m_ClearProb[i].Value = m_CurrentNation.ClearProb[i];
			m_HazyProb[i].Value = m_CurrentNation.HazyProb[i];
			m_CloudyProb[i].Value = m_CurrentNation.CloudyProb[i];
			m_OvercastProb[i].Value = m_CurrentNation.OvercastProb[i];
			m_FoggyProb[i].Value = m_CurrentNation.FoggyProb[i];
			m_RainProb[i].Value = m_CurrentNation.RainProb[i];
			m_ShowersProb[i].Value = m_CurrentNation.ShowersProb[i];
			m_FlurriesProb[i].Value = m_CurrentNation.FlurriesProb[i];
			m_SnowProb[i].Value = m_CurrentNation.SnowProb[i];
			switch (m_CurrentNation.SunsetTime[i])
			{
			default:
				m_SunsetTime[i].SelectedIndex = 0;
				break;
			case 1630:
				m_SunsetTime[i].SelectedIndex = 1;
				break;
			case 1700:
				m_SunsetTime[i].SelectedIndex = 2;
				break;
			case 1730:
				m_SunsetTime[i].SelectedIndex = 3;
				break;
			case 1800:
				m_SunsetTime[i].SelectedIndex = 4;
				break;
			case 1830:
				m_SunsetTime[i].SelectedIndex = 5;
				break;
			case 1900:
				m_SunsetTime[i].SelectedIndex = 6;
				break;
			case 1930:
				m_SunsetTime[i].SelectedIndex = 7;
				break;
			case 2000:
				m_SunsetTime[i].SelectedIndex = 8;
				break;
			case 2030:
				m_SunsetTime[i].SelectedIndex = 9;
				break;
			case 2100:
				m_SunsetTime[i].SelectedIndex = 10;
				break;
			}
			switch (m_CurrentNation.DarkTime[i])
			{
			case 1600:
				m_NightTime[i].SelectedIndex = 0;
				break;
			case 1630:
				m_NightTime[i].SelectedIndex = 1;
				break;
			case 1700:
				m_NightTime[i].SelectedIndex = 2;
				break;
			case 1730:
				m_NightTime[i].SelectedIndex = 3;
				break;
			case 1800:
				m_NightTime[i].SelectedIndex = 4;
				break;
			case 1830:
				m_NightTime[i].SelectedIndex = 5;
				break;
			case 1900:
				m_NightTime[i].SelectedIndex = 6;
				break;
			case 1930:
				m_NightTime[i].SelectedIndex = 7;
				break;
			case 2000:
				m_NightTime[i].SelectedIndex = 8;
				break;
			case 2030:
				m_NightTime[i].SelectedIndex = 9;
				break;
			case 2100:
				m_NightTime[i].SelectedIndex = 10;
				break;
			}
		}
		m_Locked = false;
	}

	public void StageToPanel()
	{
		if (m_CurrentStage == null)
		{
			panelCompObj.Enabled = false;
			groupStage.Visible = false;
			return;
		}
		m_Locked = true;
		groupStage.Visible = true;
		panelCompObj.Enabled = true;
		textUniqueId.Text = m_CurrentStage.Id.ToString();
		textFourCharName.Text = m_CurrentStage.TypeString;
		textLanguageKey.Text = m_CurrentStage.Description;
		textLanguageName.Text = m_CurrentStage.GetLanguageName();
		comboLanguageKey.SelectedItem = m_CurrentStage.Description;
		textFourCharName.Enabled = true;
		textLanguageKey.Enabled = true;
		textLanguageName.Enabled = true;
		comboLanguageKey.Visible = true;
		comboStageType.Text = m_CurrentStage.Settings.m_match_stagetype;
		bool flag = m_CurrentStage.Settings.m_StandingsSort >= 0;
		comboStageStandingRules.Visible = flag;
		if (flag)
		{
			comboStageStandingRules.SelectedIndex = m_CurrentStage.Settings.m_StandingsSort;
		}
		checkStageStandingsRules.Checked = flag;
		if (m_CurrentStage.Settings.Advance_standingskeep != -1)
		{
			numericStandingKeep.Value = m_CurrentStage.Settings.Advance_standingskeep;
		}
		checkStandingKeep.Checked = m_CurrentStage.Settings.Advance_standingskeep != -1;
		numericStandingKeep.Visible = checkStandingKeep.Checked;
		if (m_CurrentStage.Settings.Advance_standingsrank != -1)
		{
			numericStandingsRank.Value = m_CurrentStage.Settings.Advance_standingsrank;
		}
		checkStandingsRank.Checked = m_CurrentStage.Settings.Advance_standingsrank != -1;
		numericStandingsRank.Visible = checkStandingsRank.Checked;
		if (m_CurrentStage.Settings.m_match_stagetype != "SETUP")
		{
			groupSetupStage.Visible = false;
			groupPlayStage.Visible = true;
			comboMatchSituation.Text = m_CurrentStage.Settings.m_match_matchsituation;
			checkMatchReplay.Checked = m_CurrentStage.Settings.m_schedule_matchreplay != -1;
			numericPrizeMoney.Value = m_CurrentStage.Settings.m_info_prize_money;
			numericMoneyDrop.Value = m_CurrentStage.Settings.m_info_prize_money_drop;
			checkCanUseFancards.Checked = m_CurrentStage.Settings.m_match_canusefancards == "on";
			checkMaxteamsgroup.Checked = m_CurrentStage.Settings.m_advance_maxteamsgroup != -1;
			numericStageRef.Visible = checkMaxteamsgroup.Checked;
			numericStageRef.Value = m_CurrentStage.Settings.Advance_maxteamsstageref;
			checkMaxteamsassoc.Checked = m_CurrentStage.Settings.m_advance_maxteamsassoc != -1;
			checkClausuraSchedule.Checked = m_CurrentStage.Settings.m_schedule_reversed != -1;
			checkRandomDrawEvent.Checked = m_CurrentStage.Settings.m_advance_random_draw_event != -1;
			bool flag2 = m_CurrentStage.Settings.m_EndRuleKo1Leg != -1;
			comboSpecialKo1Rule.Visible = flag2;
			if (flag2)
			{
				comboSpecialKo1Rule.SelectedIndex = m_CurrentStage.Settings.m_EndRuleKo1Leg;
			}
			checkSpecialKo1Rule.Checked = flag2;
			flag2 = m_CurrentStage.Settings.m_EndRuleKo2Leg2 != -1;
			comboSpecialKo2Rule.Visible = flag2;
			if (flag2)
			{
				comboSpecialKo2Rule.SelectedIndex = m_CurrentStage.Settings.m_EndRuleKo2Leg2;
			}
			checkSpecialKo2Rule.Checked = flag2;
			numericRegularSeason.Visible = m_CurrentStage.Settings.m_EndRuleKo2Leg2 == 3;
			if (numericRegularSeason.Visible)
			{
				numericRegularSeason.Value = m_CurrentStage.Settings.Standings_checkrank;
			}
			for (int i = 0; i < 12; i++)
			{
				Stadium stadium = null;
				if (m_CurrentStage.Settings.m_match_stadium != null && m_CurrentStage.Settings.m_match_stadium[i] > 0)
				{
					stadium = (Stadium)FifaEnvironment.Stadiums.SearchId(m_CurrentStage.Settings.m_match_stadium[i]);
					if (stadium != null)
					{
						m_StadiumCombos[i].SelectedItem = stadium;
					}
				}
				if (stadium == null)
				{
					m_StadiumCombos[i].SelectedIndex = 0;
				}
			}
			treeStageSchedule.Nodes.Clear();
			groupStageScheduleDetails.Visible = false;
			buttonStageAddTime.Enabled = false;
			buttonStageRemoveTime.Enabled = false;
			buttonDeleteStageLeg.Enabled = false;
			for (int j = 1; j < 61; j++)
			{
				Schedule[] legSchedule = m_CurrentStage.GetLegSchedule(j);
				if (legSchedule == null)
				{
					break;
				}
				TreeNode treeNode = treeStageSchedule.Nodes.Add("Leg " + j);
				treeNode.ForeColor = Color.DarkGreen;
				for (int k = 0; k < legSchedule.Length; k++)
				{
					treeNode.Nodes.Add(legSchedule[k].Date.ToString("f")).Tag = legSchedule[k];
				}
			}
		}
		else
		{
			groupSetupStage.Visible = true;
			groupPlayStage.Visible = false;
			checkRandomDraw.Checked = m_CurrentStage.Settings.m_advance_randomdraw != -1;
			checkCalccompavgs.Checked = m_CurrentStage.Settings.m_advance_calccompavgs != -1;
			for (int l = 0; l < 4; l++)
			{
				Team team = null;
				if (m_CurrentStage.Settings.m_info_special_team_id[l] != 0)
				{
					team = (Team)FifaEnvironment.Teams.SearchId(m_CurrentStage.Settings.m_info_special_team_id[l]);
					if (team != null)
					{
						m_SpecialTeamCombos[l].SelectedItem = team;
					}
				}
				if (team == null)
				{
					m_SpecialTeamCombos[l].SelectedIndex = 0;
				}
			}
		}
		checkKeepPointsPercentage.Checked = m_CurrentStage.Settings.Advance_pointskeep != -1;
		numericKeepPointsPercentage.Visible = checkKeepPointsPercentage.Checked;
		numericKeepPointsStageRef.Visible = checkKeepPointsPercentage.Checked;
		if (m_CurrentStage.Settings.m_advance_pointskeeppercentage != -1)
		{
			numericKeepPointsPercentage.Value = m_CurrentStage.Settings.m_advance_pointskeeppercentage;
		}
		if (m_CurrentStage.Settings.Advance_pointskeep != -1)
		{
			numericKeepPointsStageRef.Value = m_CurrentStage.Settings.Advance_pointskeep;
		}
		groupLeaguetasks.Visible = m_CurrentStage.Settings.m_match_matchsituation == "LEAGUE";
		if (m_CurrentStage.Settings.m_match_matchsituation == "LEAGUE")
		{
			Task task = m_CurrentStage.SearchTask("start", "ClearLeagueStats", -1, -1, -1);
			League league = null;
			checkClearLeagueStats.Checked = task != null;
			if (task != null)
			{
				league = task.League;
			}
			task = m_CurrentStage.SearchTask("end", "UpdateLeagueStats", -1, -1, -1);
			checkUpdateLeagueStats.Checked = task != null;
			if (task != null)
			{
				league = task.League;
			}
			task = m_CurrentStage.SearchTask("end", "UpdateLeagueTable", -1, -1, -1);
			checkUpdateLeagueTable.Checked = task != null;
			if (task != null)
			{
				league = task.League;
			}
			if (league != null)
			{
				comboLeagueStats.SelectedItem = league;
			}
			comboLeagueStats.Visible = checkClearLeagueStats.Checked || checkUpdateLeagueStats.Checked || checkUpdateLeagueTable.Checked;
		}
		m_Locked = false;
	}

	public void GroupToPanel()
	{
		if (m_CurrentGroup == null)
		{
			panelCompObj.Enabled = false;
			groupGroup.Visible = false;
			return;
		}
		m_Locked = true;
		groupGroup.Visible = true;
		panelCompObj.Enabled = true;
		textUniqueId.Text = m_CurrentGroup.Id.ToString();
		textFourCharName.Text = m_CurrentGroup.TypeString;
		textLanguageKey.Text = m_CurrentGroup.Description;
		textLanguageName.Text = m_CurrentGroup.LanguageName;
		textFourCharName.Enabled = true;
		textLanguageKey.Enabled = true;
		textLanguageName.Enabled = true;
		comboLanguageKey.Visible = false;
		numericNTeams.Value = m_CurrentGroup.Ranks.Count - 1;
		Stage parentStage = m_CurrentGroup.ParentStage;
		_ = parentStage.Trophy;
		int i = 0;
		if (parentStage.TypeString == "S1")
		{
			panelQualificationRules.Visible = true;
			panelAdvancement.Visible = false;
			groupRules.Text = "Qualification Rules";
			for (int j = 0; j < m_CurrentGroup.m_NStartTasks; j++)
			{
				m_QRLabels[i].Text = m_CurrentGroup.m_StartTask[j].ToString();
				m_QRLabels[i].Tag = m_CurrentGroup.m_StartTask[j];
				m_QRLabels[i].Enabled = true;
				i++;
			}
			for (; i < m_QRLabels.Length; i++)
			{
				m_QRLabels[i].Enabled = false;
				m_QRLabels[i].Text = string.Empty;
			}
		}
		else
		{
			panelQualificationRules.Visible = false;
			panelAdvancement.Visible = true;
			groupRules.Text = "Advancement Rules";
			for (int k = 1; k < m_CurrentGroup.Ranks.Count; k++)
			{
				Rank rank = (Rank)m_CurrentGroup.Ranks[k];
				m_AdvanceLabels[k - 1].Text = rank.GetFromRankString();
				m_AdvanceLabels[k - 1].Visible = true;
				m_AdvanceLabels[k - 1].Tag = rank;
			}
			for (int l = m_CurrentGroup.Ranks.Count - 1; l < m_AdvanceLabels.Length; l++)
			{
				m_AdvanceLabels[l].Visible = false;
			}
		}
		if (parentStage.Settings.m_match_stagetype == "LEAGUE")
		{
			groupInfoColors.Visible = true;
			checkInfoColorChamp.Checked = m_CurrentGroup.Settings.m_info_color_slot_champ == 1;
			m_CurrentGroup.Settings.GetInfoColorSlotChampCup(out var min, out var max);
			if (min == -1 || max == -1)
			{
				checkInfoColorChampions.Checked = false;
				numericColorChampionsMin.Visible = false;
				numericColorChampionsMax.Visible = false;
			}
			else
			{
				checkInfoColorChampions.Checked = true;
				numericColorChampionsMin.Visible = true;
				numericColorChampionsMax.Visible = true;
				numericColorChampionsMin.Value = min;
				numericColorChampionsMax.Value = max;
			}
			m_CurrentGroup.Settings.GetInfoColorSlotEuroLeague(out min, out max);
			if (min == -1 || max == -1)
			{
				checkInfoColorEuropa.Checked = false;
				numericColorEuropaMin.Visible = false;
				numericColorEuropaMax.Visible = false;
			}
			else
			{
				checkInfoColorEuropa.Checked = true;
				numericColorEuropaMin.Visible = true;
				numericColorEuropaMax.Visible = true;
				numericColorEuropaMin.Value = min;
				numericColorEuropaMax.Value = max;
			}
			m_CurrentGroup.Settings.GetInfoColorSlotReleg(out min, out max);
			if (min == -1 || max == -1)
			{
				checkInfoColorRelegation.Checked = false;
				numericColorRelegationMin.Visible = false;
				numericColorRelegationMax.Visible = false;
			}
			else
			{
				checkInfoColorRelegation.Checked = true;
				numericColorRelegationMin.Visible = true;
				numericColorRelegationMax.Visible = true;
				numericColorRelegationMin.Value = min;
				numericColorRelegationMax.Value = max;
			}
			m_CurrentGroup.Settings.GetInfoColorSlotRelegPoss(out min, out max);
			if (min == -1 || max == -1)
			{
				checkInfoColorPossibleRelegation.Checked = false;
				numericColorPossibleRelegationMin.Visible = false;
				numericColorPossibleRelegationMax.Visible = false;
			}
			else
			{
				checkInfoColorPossibleRelegation.Checked = true;
				numericColorPossibleRelegationMin.Visible = true;
				numericColorPossibleRelegationMax.Visible = true;
				numericColorPossibleRelegationMin.Value = min;
				numericColorPossibleRelegationMax.Value = max;
			}
			m_CurrentGroup.Settings.GetInfoColorSlotPromo(out min, out max);
			if (min == -1 || max == -1)
			{
				checkInfoColorPromotion.Checked = false;
				numericColorPromotionMin.Visible = false;
				numericColorPromotionMax.Visible = false;
			}
			else
			{
				checkInfoColorPromotion.Checked = true;
				numericColorPromotionMin.Visible = true;
				numericColorPromotionMax.Visible = true;
				numericColorPromotionMin.Value = min;
				numericColorPromotionMax.Value = max;
			}
			m_CurrentGroup.Settings.GetInfoColorSlotPromoPoss(out min, out max);
			if (min == -1 || max == -1)
			{
				checkInfoColorPossiblePromotion.Checked = false;
				numericColorPossiblePromotionMin.Visible = false;
				numericColorPossiblePromotionMax.Visible = false;
			}
			else
			{
				checkInfoColorPossiblePromotion.Checked = true;
				numericColorPossiblePromotionMin.Visible = true;
				numericColorPossiblePromotionMax.Visible = true;
				numericColorPossiblePromotionMin.Value = min;
				numericColorPossiblePromotionMax.Value = max;
			}
			m_CurrentGroup.Settings.GetInfoColorSlotAdvGroup(out min, out max);
			if (min == -1 || max == -1)
			{
				checkInfoColorAdvance.Checked = false;
				numericColorAdvanceMin.Visible = false;
				numericColorAdvanceMax.Visible = false;
			}
			else
			{
				checkInfoColorAdvance.Checked = true;
				numericColorAdvanceMin.Visible = true;
				numericColorAdvanceMax.Visible = true;
				numericColorAdvanceMin.Value = min;
				numericColorAdvanceMax.Value = max;
			}
		}
		else
		{
			groupInfoColors.Visible = false;
		}
		if (parentStage.Settings.m_match_stagetype == "SETUP")
		{
			groupPlayGroup.Visible = false;
			groupSlots.Visible = false;
			groupGroupScheduke.Visible = false;
		}
		else
		{
			groupPlayGroup.Visible = true;
			groupGroupScheduke.Visible = true;
			if (m_CurrentGroup.Settings.m_num_games <= 0)
			{
				m_CurrentGroup.Settings.m_num_games = 1;
			}
			numericNumGames.Value = m_CurrentGroup.Settings.m_num_games;
			treeGroupSchedule.Nodes.Clear();
			groupGroupScheduleDetails.Visible = false;
			buttonGroupAddTime.Enabled = false;
			buttonGroupRemoveTime.Enabled = false;
			buttonRemoveGroupLeg.Enabled = false;
			for (int m = 1; m < 46; m++)
			{
				Schedule[] legSchedule = m_CurrentGroup.GetLegSchedule(m);
				if (legSchedule != null)
				{
					TreeNode treeNode = treeGroupSchedule.Nodes.Add("Leg " + m);
					treeNode.ForeColor = Color.DarkGreen;
					for (int n = 0; n < legSchedule.Length; n++)
					{
						treeNode.Nodes.Add(legSchedule[n].Date.ToString("f")).Tag = legSchedule[n];
					}
				}
			}
			groupSlots.Visible = true;
			checkInfoChamp.Checked = m_CurrentGroup.Settings.m_info_slot_champ == 1;
			m_CurrentGroup.Settings.GetInfoSlotReleg(out var min2, out var max2);
			if (min2 == -1 || max2 == -1)
			{
				checkInfoRelegation.Checked = false;
				numericRelegationMin.Visible = false;
				numericRelegationMax.Visible = false;
			}
			else
			{
				checkInfoRelegation.Checked = true;
				numericRelegationMin.Visible = true;
				numericRelegationMax.Visible = true;
				numericRelegationMin.Value = min2;
				numericRelegationMax.Value = max2;
			}
			m_CurrentGroup.Settings.GetInfoSlotRelegPoss(out min2, out max2);
			if (min2 == -1 || max2 == -1)
			{
				checkInfoPossibleRelegation.Checked = false;
				numericPossibleRelegationMin.Visible = false;
				numericPossibleRelegationMax.Visible = false;
			}
			else
			{
				checkInfoPossibleRelegation.Checked = true;
				numericPossibleRelegationMin.Visible = true;
				numericPossibleRelegationMax.Visible = true;
				numericPossibleRelegationMin.Value = min2;
				numericPossibleRelegationMax.Value = max2;
			}
			m_CurrentGroup.Settings.GetInfoSlotPromo(out min2, out max2);
			if (min2 == -1 || max2 == -1)
			{
				checkInfoPromotion.Checked = false;
				numericPromotionMin.Visible = false;
				numericPromotionMax.Visible = false;
			}
			else
			{
				checkInfoPromotion.Checked = true;
				numericPromotionMin.Visible = true;
				numericPromotionMax.Visible = true;
				numericPromotionMin.Value = min2;
				numericPromotionMax.Value = max2;
			}
			m_CurrentGroup.Settings.GetInfoSlotPromoPoss(out min2, out max2);
			if (min2 == -1 || max2 == -1)
			{
				checkInfoPossiblePromotion.Checked = false;
				numericPossiblePromotionMin.Visible = false;
				numericPossiblePromotionMax.Visible = false;
			}
			else
			{
				checkInfoPossiblePromotion.Checked = true;
				numericPossiblePromotionMin.Visible = true;
				numericPossiblePromotionMax.Visible = true;
				numericPossiblePromotionMin.Value = min2;
				numericPossiblePromotionMax.Value = max2;
			}
		}
		m_Locked = false;
	}

	public void TrophyStructureToPanel()
	{
		if (m_CurrentTrophy == null)
		{
			groupTrophy.Visible = false;
			return;
		}
		groupTrophy.Visible = true;
		groupTrophy.Text = m_CurrentTrophy.ShortName;
		textTrophyLongName.Text = m_CurrentTrophy.LongName;
		textTrophyShortName.Text = m_CurrentTrophy.ShortName;
		numericAssetId.Value = m_CurrentTrophy.Settings.m_asset_id;
		numericBall.Value = m_CurrentTrophy.ballid;
		if (m_CurrentTrophy.ballid >= 0)
		{
			pictureBall.BackgroundImage = Ball.GetBallPicture(m_CurrentTrophy.ballid);
		}
		else
		{
			pictureBall.BackgroundImage = null;
		}
		comboCompetitionType.SelectedItem = m_CurrentTrophy.Settings.m_comp_type;
		checkScheduleConflicts.Checked = m_CurrentTrophy.Settings.m_schedule_checkconflict == 1;
		bool flag = m_CurrentTrophy.Settings.TrophyForcecomp != null;
		comboSchedForce.Visible = flag;
		if (flag)
		{
			comboSchedForce.SelectedItem = m_CurrentTrophy.Settings.TrophyForcecomp;
		}
		checkForceSchedule.Checked = flag;
		checkScheduleUseDates.Checked = m_CurrentTrophy.Settings.m_schedule_use_dates_comp != -1;
		if (m_CurrentTrophy.Settings.m_match_matchimportance == -1)
		{
			m_CurrentTrophy.Settings.m_match_matchimportance = 25;
		}
		numericImportance.Value = m_CurrentTrophy.Settings.m_match_matchimportance;
		flag = m_CurrentTrophy.Settings.LeaguePromo != null;
		comboPromotionLeague.Visible = flag;
		if (flag)
		{
			comboPromotionLeague.SelectedItem = m_CurrentTrophy.Settings.LeaguePromo;
		}
		checkPromotionLeague.Checked = flag;
		flag = m_CurrentTrophy.Settings.LeagueReleg != null;
		comboRelegationLeague.Visible = flag;
		if (flag)
		{
			comboRelegationLeague.SelectedItem = m_CurrentTrophy.Settings.LeagueReleg;
		}
		checkRelegationLeague.Checked = flag;
		flag = m_CurrentTrophy.Settings.m_rule_numsubsbench != -1;
		radioBench5Players.Checked = flag;
		radioBench7Players.Checked = !flag;
		flag = m_CurrentTrophy.Settings.m_StandingsSort >= 0;
		comboTrophyStandingRules.Visible = flag;
		if (flag)
		{
			comboTrophyStandingRules.SelectedIndex = m_CurrentTrophy.Settings.m_StandingsSort;
		}
		checkTrophyStandingsRules.Checked = flag;
		if (m_CurrentTrophy.Settings.m_comp_type == "INTERCUP" || m_CurrentTrophy.Settings.m_comp_type == "INTERQUAL")
		{
			groupInternationalschedule.Visible = true;
			numericInternationalFirstYear.Value = m_CurrentTrophy.Settings.m_schedule_year_start;
			numericInternationalPeriodicity.Value = m_CurrentTrophy.Settings.m_schedule_year_offset;
			comboNationStartMonth.Visible = true;
			comboTrophyStartMonth.Text = m_CurrentTrophy.Settings.GetProperty("schedule_seasonstartmonth", 0, out flag);
		}
		else
		{
			groupInternationalschedule.Visible = false;
		}
		checkAdvanceFrom.Checked = m_CurrentTrophy.Settings.m_advance_teamcompdependency != -1;
		numericAdvanceFrom.Value = m_CurrentTrophy.Settings.m_advance_teamcompdependency;
		numericAdvanceFrom.Visible = checkAdvanceFrom.Checked;
	}

	public void TrophyGraphicsToPanel()
	{
		viewer2DTrophy256.CurrentBitmap = m_CurrentTrophy.GetTrophy256();
		if (FifaEnvironment.Year == 14)
		{
			viewer2DTrophy128.CurrentBitmap = m_CurrentTrophy.GetTrophy128();
		}
		else
		{
			viewer2DTrophy.CurrentBitmap = m_CurrentTrophy.GetTrophy();
		}
		multiViewer2DTextures.Bitmaps = m_CurrentTrophy.GetTextures();
		Show3DTrophy();
	}

	public void TrophyPitchDressToPanel()
	{
		if (m_CurrentTrophy == null)
		{
			viewer2DPitchDressing.CurrentBitmap = null;
		}
		else
		{
			viewer2DPitchDressing.CurrentBitmap = m_CurrentTrophy.GetPitchDressing();
		}
	}

	public void TrophyRevModToPanel()
	{
		if (m_CurrentTrophy == null)
		{
			viewer2DTournamentAdboard.CurrentBitmap = null;
			return;
		}
		viewer2DTournamentAdboard.CurrentBitmap = m_CurrentTrophy.GetAdboard();
		LoadRevModBall();
	}

	public void TrophyWipe3dToPanel()
	{
		if (m_CurrentTrophy == null)
		{
			multiViewerWipe.Bitmaps = null;
			return;
		}
		multiViewerWipe.Bitmaps = m_CurrentTrophy.GetWipe3DTextures();
		if (multiViewerWipe.Bitmaps == null)
		{
			multiViewerWipe.Enabled = false;
		}
		else
		{
			multiViewerWipe.Enabled = true;
		}
	}

	public void TrophyRankingToPanel()
	{
		Task task = null;
		m_NUpdateTableLabels = 0;
		for (int i = 0; i < 48; i++)
		{
			task = m_CurrentTrophy.SearchTask("end", "UpdateTable", -1, -1, i + 1);
			if (task == null)
			{
				break;
			}
			m_UpdateTableLabels[i].Text = task.ToString();
			m_UpdateTableLabels[i].Tag = task;
			m_NUpdateTableLabels++;
		}
		numericUpdateTableEntries.Value = m_NUpdateTableLabels;
		for (int j = 0; j < 48; j++)
		{
			m_InitTeamPanel[j].Visible = j < m_NUpdateTableLabels;
		}
		for (int k = 0; k < m_NUpdateTableLabels; k++)
		{
			InitTeam initTeam = m_CurrentTrophy.InitTeamArray[k];
			Team team = null;
			if (initTeam != null)
			{
				team = initTeam.Team;
			}
			if (team != null)
			{
				m_InitTeamCombo[k].SelectedItem = initTeam.Team;
			}
			else
			{
				m_InitTeamCombo[k].SelectedIndex = 0;
			}
		}
	}

	public void TrophyToPanel()
	{
		if (m_CurrentTrophy == null)
		{
			panelCompObj.Enabled = false;
			groupTrophy.Visible = false;
			groupGraphics.Visible = false;
			groupInitTeams.Visible = false;
			return;
		}
		m_Locked = true;
		panelCompObj.Enabled = true;
		groupTrophy.Visible = true;
		groupGraphics.Visible = true;
		groupInitTeams.Visible = true;
		textUniqueId.Text = m_CurrentTrophy.Id.ToString();
		textFourCharName.Text = m_CurrentTrophy.TypeString;
		textLanguageKey.Text = m_CurrentTrophy.Description;
		textLanguageName.Text = m_CurrentTrophy.ShortName;
		textFourCharName.Enabled = true;
		textLanguageKey.Enabled = false;
		textLanguageName.Enabled = false;
		comboLanguageKey.Visible = false;
		checkLowCelebrationLevel.Checked = m_CurrentTrophy.Settings.m_match_celebrationlevel == "LOW";
		if (tabTrophy.SelectedIndex == 0)
		{
			TrophyStructureToPanel();
		}
		else if (tabTrophy.SelectedIndex == 1)
		{
			TrophyRankingToPanel();
		}
		else if (tabTrophy.SelectedIndex == 2)
		{
			TrophyGraphicsToPanel();
		}
		else if (tabTrophy.SelectedIndex == 3)
		{
			TrophyPitchDressToPanel();
		}
		else if (tabTrophy.SelectedIndex == 4)
		{
			TrophyRevModToPanel();
		}
		else if (tabTrophy.SelectedIndex == 5)
		{
			TrophyWipe3dToPanel();
		}
		m_Locked = false;
	}

	public void Show3DTrophy()
	{
		if (!buttonShow3DModel.Checked)
		{
			viewer3DTrophy.ShowEmpty();
			return;
		}
		Bitmap[] textures = m_CurrentTrophy.GetTextures();
		Bitmap bitmap = null;
		if (textures != null)
		{
			bitmap = GraphicUtil.EmbossBitmap(textures[0], textures[1]);
		}
		Rx3File model = m_CurrentTrophy.GetModel();
		if (bitmap == null || model == null)
		{
			viewer3DTrophy.Clean(1);
			return;
		}
		Rx3IndexArray.TriangleListType = Rx3IndexArray.ETriangleListType.InvertEven;
		viewer3DTrophy.Clean(model.Rx3VertexArrays.Length);
		for (int i = 0; i < model.Rx3VertexArrays.Length; i++)
		{
			Model3D model3D = new Model3D(model.Rx3IndexArrays[i], model.Rx3VertexArrays[i], bitmap);
			viewer3DTrophy.SetMesh(i, model3D);
		}
		viewer3DTrophy.Render();
	}

	private void treeWorld_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (treeWorld.SelectedNode == null)
		{
			return;
		}
		m_CurrentCompobj = (Compobj)treeWorld.SelectedNode.Tag;
		if (!m_LockTree)
		{
			m_LockTree = true;
			if (m_CurrentCompobj.IsGroup())
			{
				m_CurrentGroup = (Group)treeWorld.SelectedNode.Tag;
				m_CurrentStage = m_CurrentGroup.ParentStage;
				m_CurrentTrophy = m_CurrentStage.Trophy;
				m_CurrentNation = m_CurrentTrophy.Nation;
				if (m_CurrentNation != null)
				{
					m_CurrentConfederation = m_CurrentNation.Confederation;
				}
				else
				{
					m_CurrentConfederation = m_CurrentTrophy.Confederation;
				}
				if (tabCompetitions.SelectedTab == pageGroup)
				{
					GroupToPanel();
				}
				tabCompetitions.SelectedTab = pageGroup;
				treeWorld.Select();
			}
			else if (m_CurrentCompobj.IsStage())
			{
				m_CurrentStage = (Stage)treeWorld.SelectedNode.Tag;
				m_CurrentGroup = null;
				m_CurrentTrophy = m_CurrentStage.Trophy;
				m_CurrentNation = m_CurrentTrophy.Nation;
				if (m_CurrentNation != null)
				{
					m_CurrentConfederation = m_CurrentNation.Confederation;
				}
				else
				{
					m_CurrentConfederation = m_CurrentTrophy.Confederation;
				}
				if (tabCompetitions.SelectedTab == pageStage)
				{
					StageToPanel();
				}
				tabCompetitions.SelectedTab = pageStage;
				treeWorld.Select();
			}
			else if (m_CurrentCompobj.IsTrophy())
			{
				m_CurrentTrophy = (Trophy)treeWorld.SelectedNode.Tag;
				m_CurrentStage = null;
				m_CurrentGroup = null;
				m_CurrentNation = m_CurrentTrophy.Nation;
				if (m_CurrentNation != null)
				{
					m_CurrentConfederation = m_CurrentNation.Confederation;
				}
				else
				{
					m_CurrentConfederation = m_CurrentTrophy.Confederation;
				}
				if (tabCompetitions.SelectedTab == pageTrophy)
				{
					TrophyToPanel();
				}
				tabCompetitions.SelectedTab = pageTrophy;
				treeWorld.Select();
			}
			else if (m_CurrentCompobj.IsNation())
			{
				m_CurrentNation = (Nation)treeWorld.SelectedNode.Tag;
				m_CurrentTrophy = null;
				m_CurrentStage = null;
				m_CurrentGroup = null;
				m_CurrentConfederation = m_CurrentNation.Confederation;
				if (tabCompetitions.SelectedTab == pageNation)
				{
					NationToPanel();
				}
				tabCompetitions.SelectedTab = pageNation;
				treeWorld.Select();
			}
			else if (m_CurrentCompobj.IsConfederation())
			{
				m_CurrentConfederation = (Confederation)treeWorld.SelectedNode.Tag;
				m_CurrentNation = null;
				m_CurrentTrophy = null;
				m_CurrentStage = null;
				m_CurrentGroup = null;
				if (tabCompetitions.SelectedTab == pageConfederation)
				{
					ConfederationToPanel();
				}
				tabCompetitions.SelectedTab = pageConfederation;
				treeWorld.Select();
			}
			else if (m_CurrentCompobj.IsWorld())
			{
				m_CurrentConfederation = null;
				m_CurrentNation = null;
				m_CurrentTrophy = null;
				m_CurrentStage = null;
				m_CurrentGroup = null;
				if (tabCompetitions.SelectedTab == pageWorld)
				{
					WorldToPanel();
				}
				tabCompetitions.SelectedTab = pageWorld;
				treeWorld.Select();
			}
			m_LockTree = false;
		}
		EnableToolWorld();
	}

	private void comboConfederationStartingMonth_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboConfederationStartingMonth.SelectedItem != null)
		{
			m_CurrentConfederation.Settings.m_schedule_seasonstartmonth = (string)comboConfederationStartingMonth.SelectedItem;
		}
	}

	private void comboNationStartMonth_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboNationStartMonth.SelectedItem != null)
		{
			m_CurrentNation.Settings.m_schedule_seasonstartmonth = (string)comboNationStartMonth.SelectedItem;
		}
	}

	private void numericYellowsStored_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentNation.Settings.m_rule_numyellowstored = (int)numericNationYellowsStored.Value;
		}
	}

	private void checkNationStandingsRules_CheckedChanged(object sender, EventArgs e)
	{
		comboNationStandingsRules.Visible = checkNationStandingsRules.Checked;
		if (checkNationStandingsRules.Checked)
		{
			m_CurrentNation.Settings.m_StandingsSort = comboNationStandingsRules.SelectedIndex;
		}
		else
		{
			m_CurrentNation.Settings.m_StandingsSort = -1;
		}
	}

	private void comboNationStandingsRules_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentNation.Settings.m_StandingsSort = comboNationStandingsRules.SelectedIndex;
		}
	}

	private void comboCountry_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboCountry.SelectedItem != null)
		{
			Country country = (Country)comboCountry.SelectedItem;
			if (m_CurrentNation.Country != country && country != null)
			{
				m_CurrentNation.Country = country;
				m_CurrentNation.Description = FifaEnvironment.Language.GetCountryConventionalString(country.Id, Language.ECountryStringType.Full);
				NationToPanel();
			}
		}
	}

	private void textTrophyLongName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentTrophy.LongName = textTrophyLongName.Text;
			treeWorld.SelectedNode.Text = m_CurrentTrophy.ToString();
		}
	}

	private void textTrophyShortName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentTrophy.ShortName = textTrophyShortName.Text;
			treeWorld.SelectedNode.Text = m_CurrentTrophy.ToString();
			textLanguageName.Text = m_CurrentTrophy.ShortName;
			m_CurrentTrophy.SetLanguageName(m_CurrentTrophy.ShortName);
		}
	}

	private void buttonGetId_Click(object sender, EventArgs e)
	{
		numericAssetId.Value = Trophy.AutoAsset();
	}

	private void numericAssetId_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		int num = (int)numericAssetId.Value;
		if (num == m_CurrentTrophy.Settings.m_asset_id)
		{
			return;
		}
		m_Locked = true;
		if (num != 993)
		{
			foreach (Compobj competitionObject in FifaEnvironment.CompetitionObjects)
			{
				if (competitionObject.IsTrophy() && competitionObject.Settings.m_asset_id == num)
				{
					FifaEnvironment.UserMessages.ShowMessage(1015);
					numericAssetId.Value = m_CurrentTrophy.Settings.m_asset_id;
					m_Locked = false;
					return;
				}
			}
		}
		m_CurrentTrophy.Settings.m_asset_id = num;
		if (num != 993)
		{
			m_CurrentTrophy.Description = FifaEnvironment.Language.GetTournamentConventionalString(num, Language.ETournamentStringType.Abbr15);
			textLanguageKey.Text = m_CurrentTrophy.Description;
			string text = "C" + m_CurrentTrophy.Settings.m_asset_id;
			textFourCharName.Text = text;
		}
		TrophyGraphicsToPanel();
		m_Locked = false;
	}

	private void comboCompetitionType_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && !(m_CurrentTrophy.Settings.m_comp_type == (string)comboCompetitionType.SelectedItem))
		{
			m_CurrentTrophy.Settings.m_comp_type = (string)comboCompetitionType.SelectedItem;
			TrophyToPanel();
		}
	}

	private void numericImportance_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentTrophy.Settings.m_match_matchimportance = (int)numericImportance.Value;
		}
	}

	private void comboPromotionLeague_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboPromotionLeague.SelectedIndex >= 0)
		{
			if (comboPromotionLeague.SelectedIndex == 0)
			{
				m_CurrentTrophy.Settings.LeaguePromo = null;
			}
			else
			{
				m_CurrentTrophy.Settings.LeaguePromo = (League)comboPromotionLeague.SelectedItem;
			}
		}
	}

	private void comboRelegationLeague_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboRelegationLeague.SelectedIndex >= 0)
		{
			if (comboRelegationLeague.SelectedIndex == 0)
			{
				m_CurrentTrophy.Settings.LeagueReleg = null;
			}
			else
			{
				m_CurrentTrophy.Settings.LeagueReleg = (League)comboRelegationLeague.SelectedItem;
			}
		}
	}

	private void checkForceSchedule_CheckedChanged(object sender, EventArgs e)
	{
		comboSchedForce.Visible = checkForceSchedule.Checked;
		if (checkForceSchedule.Checked)
		{
			if (comboSchedForce.SelectedItem == null)
			{
				comboSchedForce.SelectedItem = comboSchedForce.Items[0];
			}
			m_CurrentTrophy.Settings.TrophyForcecomp = (Trophy)comboSchedForce.SelectedItem;
		}
		else
		{
			m_CurrentTrophy.Settings.TrophyForcecomp = null;
		}
	}

	private void checkTrophyStandingsRules_CheckedChanged(object sender, EventArgs e)
	{
		comboTrophyStandingRules.Visible = checkTrophyStandingsRules.Checked;
		if (checkTrophyStandingsRules.Checked)
		{
			m_CurrentTrophy.Settings.m_StandingsSort = comboTrophyStandingRules.SelectedIndex;
		}
		else
		{
			m_CurrentTrophy.Settings.m_StandingsSort = -1;
		}
	}

	private void comboTrophyStandingRules_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboTrophyStandingRules.SelectedIndex >= 0)
		{
			m_CurrentTrophy.Settings.m_StandingsSort = comboTrophyStandingRules.SelectedIndex;
		}
	}

	private void comboSchedForce_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboSchedForce.SelectedItem != null)
		{
			m_CurrentTrophy.Settings.TrophyForcecomp = (Trophy)comboSchedForce.SelectedItem;
		}
	}

	private void checkPromotionLeague_CheckedChanged(object sender, EventArgs e)
	{
		comboPromotionLeague.Visible = checkPromotionLeague.Checked;
		if (checkPromotionLeague.Checked)
		{
			m_CurrentTrophy.Settings.LeaguePromo = (League)comboPromotionLeague.SelectedItem;
		}
		else
		{
			m_CurrentTrophy.Settings.LeaguePromo = null;
		}
	}

	private void checkRelegationLeague_CheckedChanged(object sender, EventArgs e)
	{
		comboRelegationLeague.Visible = checkRelegationLeague.Checked;
		if (checkRelegationLeague.Checked)
		{
			m_CurrentTrophy.Settings.LeagueReleg = (League)comboRelegationLeague.SelectedItem;
		}
		else
		{
			m_CurrentTrophy.Settings.LeagueReleg = null;
		}
	}

	private void checkScheduleConflicts_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentTrophy.Settings.m_schedule_checkconflict = (checkScheduleConflicts.Checked ? 1 : (-1));
		}
	}

	private void radioBench5Players_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked && radioBench5Players.Checked)
		{
			m_CurrentTrophy.Settings.m_rule_numsubsbench = 5;
		}
	}

	private void radioBench7Players_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked && radioBench7Players.Checked)
		{
			m_CurrentTrophy.Settings.m_rule_numsubsbench = -1;
		}
	}

	private void comboStageType_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboStageType.SelectedItem != null)
		{
			m_CurrentStage.Settings.m_match_stagetype = (string)comboStageType.SelectedItem;
			StageToPanel();
		}
	}

	private void comboMatchSituation_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboStageType.SelectedItem != null)
		{
			m_CurrentStage.Settings.m_match_matchsituation = (string)comboMatchSituation.SelectedItem;
			m_CurrentStage.Settings.m_schedule_matchreplay = ((m_CurrentStage.Settings.m_match_matchsituation == "REPLAY") ? 1 : (-1));
		}
	}

	private void numericPrizeMoney_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_info_prize_money = (int)numericPrizeMoney.Value;
		}
	}

	private void numericMoneyDrop_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_info_prize_money_drop = (int)numericMoneyDrop.Value;
		}
	}

	private void numericStartYear_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentWorld.Settings.m_schedule_year_start = (int)numericStartYear.Value;
		}
	}

	private void numericNumGames_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentStage.Settings.m_num_games = (int)numericNumGames.Value;
	}

	private void comboSpecialTeam1_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			SetSpecialTeam(0);
		}
	}

	private void comboSpecialTeam2_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			SetSpecialTeam(1);
		}
	}

	private void comboSpecialTeam3_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			SetSpecialTeam(2);
		}
	}

	private void comboSpecialTeam4_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			SetSpecialTeam(3);
		}
	}

	private void SetSpecialTeam(int index)
	{
		if (m_SpecialTeamCombos[index].SelectedIndex == 0)
		{
			m_CurrentStage.Settings.m_info_special_team_id[index] = -1;
			return;
		}
		Team team = (Team)m_SpecialTeamCombos[index].SelectedItem;
		m_CurrentStage.Settings.m_info_special_team_id[index] = team.Id;
	}

	private void SetMatchStadium(int index)
	{
		if (m_StadiumCombos[index].SelectedIndex == 0)
		{
			if (m_CurrentStage.Settings.m_match_stadium != null)
			{
				m_CurrentStage.Settings.m_match_stadium[index] = -1;
			}
			return;
		}
		Stadium stadium = (Stadium)m_StadiumCombos[index].SelectedItem;
		if (stadium != null && (m_CurrentStage.Settings.m_match_stadium == null || m_CurrentStage.Settings.m_match_stadium[index] != stadium.Id))
		{
			m_CurrentStage.Settings.SetProperty("match_stadium", index, stadium.Id.ToString());
		}
	}

	private void comboStadium1_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(0);
	}

	private void comboStadium2_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(1);
	}

	private void comboStadium3_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(2);
	}

	private void comboStadium4_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(3);
	}

	private void comboStadium5_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(4);
	}

	private void comboStadium6_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(5);
	}

	private void comboStadium7_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(6);
	}

	private void comboStadium8_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(7);
	}

	private void comboStadium9_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(8);
	}

	private void comboStadium10_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(9);
	}

	private void comboStadium11_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(10);
	}

	private void comboStadium12_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetMatchStadium(11);
	}

	private void checkMaxteamsgroup_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_advance_maxteamsgroup = (checkMaxteamsgroup.Checked ? 1 : (-1));
			numericStageRef.Visible = checkMaxteamsgroup.Checked;
			if (m_CurrentStage.Settings.m_advance_maxteamsgroup == -1)
			{
				m_CurrentStage.Settings.Advance_maxteamsstageref = -1;
			}
			else
			{
				numericStageRef.Value = m_CurrentStage.Settings.Advance_maxteamsstageref;
			}
		}
	}

	private void checkAdvanceFrom_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		numericAdvanceFrom.Visible = checkAdvanceFrom.Checked;
		if (checkAdvanceFrom.Checked != (m_CurrentTrophy.Settings.m_advance_teamcompdependency != -1))
		{
			if (!checkAdvanceFrom.Checked)
			{
				m_CurrentTrophy.Settings.m_advance_teamcompdependency = -1;
			}
			else
			{
				numericAdvanceFrom.Value = m_CurrentTrophy.Settings.m_advance_teamcompdependency;
			}
		}
	}

	private void numericAdvanceFrom_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentTrophy.Settings.m_advance_teamcompdependency != (int)numericAdvanceFrom.Value)
		{
			m_CurrentTrophy.Settings.m_advance_teamcompdependency = (int)numericAdvanceFrom.Value;
		}
	}

	private void checkStandingKeep_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.Advance_standingskeep = (checkStandingKeep.Checked ? ((int)numericStandingKeep.Value) : (-1));
			numericStandingKeep.Visible = checkStandingKeep.Checked;
			m_CurrentStage.Settings.Advance_standingskeep = (checkStandingKeep.Checked ? ((int)numericStandingKeep.Value) : (-1));
		}
	}

	private void checkKeepPointsPercentage_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		numericKeepPointsPercentage.Visible = checkKeepPointsPercentage.Checked;
		numericKeepPointsStageRef.Visible = checkKeepPointsPercentage.Checked;
		if (checkKeepPointsPercentage.Checked != (m_CurrentStage.Settings.Advance_pointskeep != -1))
		{
			if (!checkKeepPointsPercentage.Checked)
			{
				m_CurrentStage.Settings.Advance_pointskeep = -1;
				m_CurrentStage.Settings.m_advance_pointskeeppercentage = -1;
				return;
			}
			m_CurrentStage.Settings.Advance_pointskeep = m_CurrentStage.Id;
			m_CurrentStage.Settings.m_advance_pointskeeppercentage = 50;
			numericKeepPointsPercentage.Value = 50m;
			numericKeepPointsStageRef.Value = m_CurrentStage.Id;
		}
	}

	private void numericStandingKeep_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentStage.Settings.Advance_standingskeep != (int)numericStandingKeep.Value)
		{
			m_CurrentStage.Settings.Advance_standingskeep = (checkStandingKeep.Checked ? ((int)numericStandingKeep.Value) : (-1));
		}
	}

	private void numericKeepPointsPercentage_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_advance_pointskeeppercentage = (checkKeepPointsPercentage.Checked ? ((int)numericKeepPointsPercentage.Value) : (-1));
		}
	}

	private void checkSpecialKo1Rule_CheckedChanged(object sender, EventArgs e)
	{
		comboSpecialKo1Rule.Visible = checkSpecialKo1Rule.Checked;
		if (checkSpecialKo1Rule.Checked)
		{
			m_CurrentStage.Settings.m_EndRuleKo1Leg = comboSpecialKo1Rule.SelectedIndex;
		}
		else
		{
			m_CurrentStage.Settings.m_EndRuleKo1Leg = -1;
		}
	}

	private void checkSpecialKo2Rule_CheckedChanged(object sender, EventArgs e)
	{
		comboSpecialKo2Rule.Visible = checkSpecialKo2Rule.Checked;
		if (checkSpecialKo2Rule.Checked)
		{
			m_CurrentStage.Settings.m_EndRuleKo2Leg2 = comboSpecialKo2Rule.SelectedIndex;
		}
		else
		{
			m_CurrentStage.Settings.m_EndRuleKo2Leg2 = -1;
		}
	}

	private void comboSpecialKo1Rule_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboSpecialKo1Rule.SelectedIndex >= 0)
		{
			m_CurrentStage.Settings.m_EndRuleKo1Leg = comboSpecialKo1Rule.SelectedIndex;
		}
	}

	private void comboSpecialKo2Rule_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboSpecialKo2Rule.SelectedIndex >= 0)
		{
			m_CurrentStage.Settings.m_EndRuleKo2Leg2 = comboSpecialKo2Rule.SelectedIndex;
			numericRegularSeason.Visible = m_CurrentStage.Settings.m_EndRuleKo2Leg2 == 3;
		}
	}

	private void numericRegularSeason_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentStage.Settings.Standings_checkrank != (int)numericRegularSeason.Value)
		{
			m_CurrentStage.Settings.Standings_checkrank = (int)numericRegularSeason.Value;
		}
	}

	private void checkStandingsRank_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.Advance_standingsrank = (checkStandingsRank.Checked ? ((int)numericStandingsRank.Value) : (-1));
			numericStandingsRank.Visible = checkStandingsRank.Checked;
			m_CurrentStage.Settings.Advance_standingsrank = (checkStandingsRank.Checked ? ((int)numericStandingsRank.Value) : (-1));
		}
	}

	private void numericStandingsRank_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentStage.Settings.Advance_standingsrank != (int)numericStandingsRank.Value)
		{
			m_CurrentStage.Settings.Advance_standingsrank = (checkStandingsRank.Checked ? ((int)numericStandingsRank.Value) : (-1));
		}
	}

	private void checkInfoColorChamp_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentGroup.Settings.m_info_color_slot_champ = (checkInfoColorChamp.Checked ? 1 : (-1));
		}
	}

	private void numericColorChampionsMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorChampionsMin.Value;
			int max = (int)numericColorChampionsMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotChampCup(out var min, out var _);
			if (num != min && !m_CurrentGroup.Settings.SetInfoColorSlotChampCup(num, max))
			{
				numericColorChampionsMin.Value = min;
			}
		}
	}

	private void numericColorChampionsMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int min = (int)numericColorChampionsMin.Value;
			int num = (int)numericColorChampionsMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotChampCup(out var _, out var max);
			if (num != max && !m_CurrentGroup.Settings.SetInfoColorSlotChampCup(min, num))
			{
				numericColorChampionsMax.Value = max;
			}
		}
	}

	private void numericColorEuropaMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorEuropaMin.Value;
			int num2 = (int)numericColorEuropaMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotEuroLeague(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotEuroLeague(num, num2))
			{
				numericColorEuropaMin.Value = min;
			}
		}
	}

	private void numericColorEuropaMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorEuropaMin.Value;
			int num2 = (int)numericColorEuropaMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotEuroLeague(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotEuroLeague(num, num2))
			{
				numericColorEuropaMax.Value = max;
			}
		}
	}

	private void numericColorPossibleRelegationMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorPossibleRelegationMin.Value;
			int num2 = (int)numericColorPossibleRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotRelegPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotRelegPoss(num, num2))
			{
				numericColorPossibleRelegationMin.Value = min;
			}
		}
	}

	private void numericColorPossibleRelegationMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorPossibleRelegationMin.Value;
			int num2 = (int)numericColorPossibleRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotRelegPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotRelegPoss(num, num2))
			{
				numericColorPossibleRelegationMax.Value = max;
			}
		}
	}

	private void numericColorRelegationMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorRelegationMin.Value;
			int num2 = (int)numericColorRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotReleg(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotReleg(num, num2))
			{
				numericColorRelegationMin.Value = min;
			}
		}
	}

	private void numericColorRelegationMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorRelegationMin.Value;
			int num2 = (int)numericColorRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotReleg(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotReleg(num, num2))
			{
				numericColorRelegationMax.Value = max;
			}
		}
	}

	private void numericColorPromotionMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorPromotionMin.Value;
			int num2 = (int)numericColorPromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotPromo(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotPromo(num, num2))
			{
				numericColorPromotionMin.Value = min;
			}
		}
	}

	private void numericColorPromotionMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorPromotionMin.Value;
			int num2 = (int)numericColorPromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotPromo(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotPromo(num, num2))
			{
				numericColorPromotionMax.Value = max;
			}
		}
	}

	private void numericColorPossiblePromotionMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorPossiblePromotionMin.Value;
			int num2 = (int)numericColorPossiblePromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotPromoPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotPromoPoss(num, num2))
			{
				numericColorPossiblePromotionMin.Value = min;
			}
		}
	}

	private void numericColorPossiblePromotionMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorPossiblePromotionMin.Value;
			int num2 = (int)numericColorPossiblePromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotPromoPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotPromoPoss(num, num2))
			{
				numericColorPossiblePromotionMax.Value = max;
			}
		}
	}

	private void numericColorAdvanceMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorAdvanceMin.Value;
			int num2 = (int)numericColorAdvanceMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotAdvGroup(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotAdvGroup(num, num2))
			{
				numericColorAdvanceMin.Value = min;
			}
		}
	}

	private void numericColorAdvanceMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericColorAdvanceMin.Value;
			int num2 = (int)numericColorAdvanceMax.Value;
			m_CurrentGroup.Settings.GetInfoColorSlotAdvGroup(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoColorSlotAdvGroup(num, num2))
			{
				numericColorAdvanceMax.Value = max;
			}
		}
	}

	private void checkInfoColorChampions_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericColorChampionsMin;
		bool visible = (numericColorChampionsMax.Visible = checkInfoColorChampions.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoColorChampions.Checked)
		{
			m_CurrentGroup.Settings.GetInfoColorSlotChampCup(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericColorChampionsMax.Value = min;
			numericColorChampionsMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoColorSlotChampCup(min2, max2);
		}
	}

	private void checkInfoColorEuropa_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericColorEuropaMin;
		bool visible = (numericColorEuropaMax.Visible = checkInfoColorEuropa.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoColorEuropa.Checked)
		{
			m_CurrentGroup.Settings.GetInfoColorSlotEuroLeague(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericColorEuropaMin.Value = min;
			numericColorEuropaMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoColorSlotEuroLeague(min2, max2);
		}
	}

	private void checkInfoColorPossibleRelegation_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericColorPossibleRelegationMin;
		bool visible = (numericColorPossibleRelegationMax.Visible = checkInfoColorPossibleRelegation.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoColorPossibleRelegation.Checked)
		{
			m_CurrentGroup.Settings.GetInfoColorSlotRelegPoss(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericColorPossibleRelegationMin.Value = min;
			numericColorPossibleRelegationMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoColorSlotRelegPoss(min2, max2);
		}
	}

	private void checkInfoColorRelegation_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericColorRelegationMin;
		bool visible = (numericColorRelegationMax.Visible = checkInfoColorRelegation.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoColorRelegation.Checked)
		{
			m_CurrentGroup.Settings.GetInfoColorSlotReleg(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericColorRelegationMin.Value = min;
			numericColorRelegationMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoColorSlotReleg(min2, max2);
		}
	}

	private void checkInfoColorPromotion_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericColorPromotionMin;
		bool visible = (numericColorPromotionMax.Visible = checkInfoColorPromotion.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoColorPromotion.Checked)
		{
			m_CurrentGroup.Settings.GetInfoColorSlotPromo(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericColorPromotionMin.Value = min;
			numericColorPromotionMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoColorSlotPromo(min2, max2);
		}
	}

	private void checkInfoColorPossiblePromotion_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericColorPossiblePromotionMin;
		bool visible = (numericColorPossiblePromotionMax.Visible = checkInfoColorPossiblePromotion.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoColorPossiblePromotion.Checked)
		{
			m_CurrentGroup.Settings.GetInfoColorSlotPromoPoss(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericColorPossiblePromotionMin.Value = min;
			numericColorPossiblePromotionMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoColorSlotPromoPoss(min2, max2);
		}
	}

	private void checkInfoColorAdvance_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericColorAdvanceMin;
		bool visible = (numericColorAdvanceMax.Visible = checkInfoColorAdvance.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoColorAdvance.Checked)
		{
			m_CurrentGroup.Settings.GetInfoColorSlotAdvGroup(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericColorAdvanceMin.Value = min;
			numericColorAdvanceMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoColorSlotAdvGroup(min2, max2);
		}
	}

	private void labelQR_Click(object sender, EventArgs e)
	{
		Label label = (Label)sender;
		Task task = (Task)label.Tag;
		m_QualifyRuleDialog.QualifyRule = task;
		if (m_QualifyRuleDialog.ShowDialog() == DialogResult.OK)
		{
			label.Tag = task;
			label.Text = task.ToString();
		}
	}

	private void labelAdvance_Click(object sender, EventArgs e)
	{
		Rank rank = (Rank)((Label)sender).Tag;
		Rank rank2 = new Rank(rank.Group, rank.Id);
		rank2.MoveFrom = rank.MoveFrom;
		m_AdvanceRuleDialog.Rule = rank2;
		if (m_AdvanceRuleDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		if (rank2.MoveFrom != rank.MoveFrom)
		{
			if (rank.MoveFrom != null && rank.MoveFrom.Id != 0)
			{
				rank.MoveFrom.MoveTo = null;
			}
			if (rank2.MoveFrom.Id != 0)
			{
				if (rank2.MoveFrom.MoveTo == null)
				{
					rank2.MoveFrom.MoveTo = rank;
				}
				else
				{
					rank2.MoveFrom.MoveTo.MoveFrom = null;
					rank2.MoveFrom.MoveTo = rank;
				}
			}
			rank.MoveFrom = rank2.MoveFrom;
		}
		GroupToPanel();
	}

	private void textLanguageKey_TextChanged(object sender, EventArgs e)
	{
		if (m_CurrentCompobj != null)
		{
			if (m_CurrentCompobj.IsTrophy())
			{
				m_CurrentTrophy.Description = textLanguageKey.Text;
			}
			else if (m_CurrentCompobj.IsGroup())
			{
				m_CurrentGroup.Description = textLanguageKey.Text;
			}
		}
	}

	private void textFourCharName_TextChanged(object sender, EventArgs e)
	{
		if (m_CurrentCompobj == null || m_CurrentCompobj.TypeString == textFourCharName.Text)
		{
			return;
		}
		if (textFourCharName.Text.Length > 5)
		{
			textFourCharName.Text = textFourCharName.Text.Substring(0, 5);
			return;
		}
		m_CurrentCompobj.TypeString = textFourCharName.Text;
		if (m_CurrentCompobj.IsNation())
		{
			treeWorld.SelectedNode.Text = m_CurrentCompobj.TypeString;
		}
		if (m_CurrentCompobj.IsGroup())
		{
			treeWorld.SelectedNode.Text = m_CurrentCompobj.TypeString;
		}
	}

	private TreeNode SelectWorldTreeNode(Compobj compobj)
	{
		if (compobj == null)
		{
			treeWorld.SelectedNode = null;
			return null;
		}
		TreeNode treeNode = RecusiveSearchNode(treeWorld.TopNode, compobj);
		treeWorld.SelectedNode = treeNode;
		return treeNode;
	}

	private TreeNode RecusiveSearchNode(TreeNode node, Compobj compobj)
	{
		if ((Compobj)node.Tag == compobj)
		{
			return node;
		}
		foreach (TreeNode node2 in node.Nodes)
		{
			TreeNode treeNode = RecusiveSearchNode(node2, compobj);
			if (treeNode != null)
			{
				return treeNode;
			}
		}
		return null;
	}

	private void textLanguageName_TextChanged(object sender, EventArgs e)
	{
		if (m_CurrentCompobj == null)
		{
			return;
		}
		if (m_CurrentCompobj.IsTrophy())
		{
			m_CurrentTrophy.ShortName = textLanguageName.Text;
		}
		else if (m_CurrentCompobj.IsStage())
		{
			if (m_CurrentStage.GetLanguageName() != textLanguageName.Text)
			{
				m_CurrentStage.SetLanguageName(textLanguageName.Text);
			}
			string text = m_CurrentStage.ToString();
			if (treeWorld.SelectedNode.Text != text)
			{
				treeWorld.SelectedNode.Text = text;
			}
		}
		else if (m_CurrentCompobj.IsGroup())
		{
			m_CurrentGroup.LanguageName = textLanguageName.Text;
		}
	}

	private void comboLanguageKey_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLanguageKey.SelectedItem != null && comboLanguageKey.SelectedItem.ToString() != m_CurrentStage.Description)
		{
			m_CurrentStage.Description = (string)comboLanguageKey.SelectedItem;
			string text = FifaEnvironment.Language.GetString(m_CurrentStage.Description);
			textLanguageName.Text = text;
			treeWorld.SelectedNode.Text = m_CurrentStage.ToString();
		}
	}

	private void buttonCopyWeather_Click(object sender, EventArgs e)
	{
		m_ClipboardNation = m_CurrentNation;
		buttonPasteWeather.Enabled = true;
	}

	private void buttonPasteWeather_Click(object sender, EventArgs e)
	{
		if (m_ClipboardNation != null)
		{
			for (int i = 0; i < 12; i++)
			{
				m_CurrentNation.ClearProb[i] = m_ClipboardNation.ClearProb[i];
				m_CurrentNation.HazyProb[i] = m_ClipboardNation.HazyProb[i];
				m_CurrentNation.CloudyProb[i] = m_ClipboardNation.CloudyProb[i];
				m_CurrentNation.RainProb[i] = m_ClipboardNation.RainProb[i];
				m_CurrentNation.ShowersProb[i] = m_ClipboardNation.ShowersProb[i];
				m_CurrentNation.SnowProb[i] = m_ClipboardNation.SnowProb[i];
				m_CurrentNation.FlurriesProb[i] = m_ClipboardNation.FlurriesProb[i];
				m_CurrentNation.OvercastProb[i] = m_ClipboardNation.OvercastProb[i];
				m_CurrentNation.FoggyProb[i] = m_ClipboardNation.FoggyProb[i];
				m_CurrentNation.SunsetTime[i] = m_ClipboardNation.SunsetTime[i];
				m_CurrentNation.DarkTime[i] = m_ClipboardNation.DarkTime[i];
			}
			NationToPanel();
		}
	}

	private void weatherProb_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = (NumericUpDown)sender;
		for (int i = 0; i < 12; i++)
		{
			if (numericUpDown == m_HazyProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.HazyProb[i] = (int)m_HazyProb[i].Value;
				}
				else
				{
					m_HazyProb[i].Value = m_CurrentNation.HazyProb[i];
				}
				break;
			}
			if (numericUpDown == m_CloudyProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.CloudyProb[i] = (int)m_CloudyProb[i].Value;
				}
				else
				{
					m_CloudyProb[i].Value = m_CurrentNation.CloudyProb[i];
				}
				break;
			}
			if (numericUpDown == m_OvercastProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.OvercastProb[i] = (int)m_OvercastProb[i].Value;
				}
				else
				{
					m_OvercastProb[i].Value = m_CurrentNation.OvercastProb[i];
				}
				break;
			}
			if (numericUpDown == m_FoggyProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.FoggyProb[i] = (int)m_FoggyProb[i].Value;
				}
				else
				{
					m_FoggyProb[i].Value = m_CurrentNation.FoggyProb[i];
				}
				break;
			}
			if (numericUpDown == m_RainProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.RainProb[i] = (int)m_RainProb[i].Value;
				}
				else
				{
					m_RainProb[i].Value = m_CurrentNation.RainProb[i];
				}
				break;
			}
			if (numericUpDown == m_ShowersProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.ShowersProb[i] = (int)m_ShowersProb[i].Value;
				}
				else
				{
					m_ShowersProb[i].Value = m_CurrentNation.ShowersProb[i];
				}
				break;
			}
			if (numericUpDown == m_FlurriesProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.FlurriesProb[i] = (int)m_FlurriesProb[i].Value;
				}
				else
				{
					m_FlurriesProb[i].Value = m_CurrentNation.FlurriesProb[i];
				}
				break;
			}
			if (numericUpDown == m_SnowProb[i])
			{
				if (ComputeClearProb(i))
				{
					m_CurrentNation.SnowProb[i] = (int)m_SnowProb[i].Value;
				}
				else
				{
					m_SnowProb[i].Value = m_CurrentNation.SnowProb[i];
				}
				break;
			}
			if (numericUpDown == m_ClearProb[i])
			{
				m_CurrentNation.ClearProb[i] = (int)m_ClearProb[i].Value;
				break;
			}
		}
	}

	private bool ComputeClearProb(int month)
	{
		int num = (int)(m_HazyProb[month].Value + m_CloudyProb[month].Value + m_OvercastProb[month].Value + m_FoggyProb[month].Value + m_RainProb[month].Value + m_ShowersProb[month].Value + m_FlurriesProb[month].Value + m_SnowProb[month].Value);
		int num2 = 100 - num;
		if (num2 >= 0)
		{
			m_ClearProb[month].Value = num2;
			return true;
		}
		return false;
	}

	private void dayTime_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		for (int i = 0; i < 12; i++)
		{
			if (comboBox == m_SunsetTime[i])
			{
				switch (comboBox.SelectedIndex)
				{
				case 0:
					m_CurrentNation.SunsetTime[i] = 1600;
					break;
				case 1:
					m_CurrentNation.SunsetTime[i] = 1630;
					break;
				case 2:
					m_CurrentNation.SunsetTime[i] = 1700;
					break;
				case 3:
					m_CurrentNation.SunsetTime[i] = 1730;
					break;
				case 4:
					m_CurrentNation.SunsetTime[i] = 1800;
					break;
				case 5:
					m_CurrentNation.SunsetTime[i] = 1830;
					break;
				case 6:
					m_CurrentNation.SunsetTime[i] = 1900;
					break;
				case 7:
					m_CurrentNation.SunsetTime[i] = 1930;
					break;
				case 8:
					m_CurrentNation.SunsetTime[i] = 2000;
					break;
				case 9:
					m_CurrentNation.SunsetTime[i] = 2030;
					break;
				case 10:
					m_CurrentNation.SunsetTime[i] = 2100;
					break;
				}
			}
			else if (comboBox == m_NightTime[i])
			{
				switch (comboBox.SelectedIndex)
				{
				case 0:
					m_CurrentNation.DarkTime[i] = 1600;
					break;
				case 1:
					m_CurrentNation.DarkTime[i] = 1630;
					break;
				case 2:
					m_CurrentNation.DarkTime[i] = 1700;
					break;
				case 3:
					m_CurrentNation.DarkTime[i] = 1730;
					break;
				case 4:
					m_CurrentNation.DarkTime[i] = 1800;
					break;
				case 5:
					m_CurrentNation.DarkTime[i] = 1830;
					break;
				case 6:
					m_CurrentNation.DarkTime[i] = 1900;
					break;
				case 7:
					m_CurrentNation.DarkTime[i] = 1930;
					break;
				case 8:
					m_CurrentNation.DarkTime[i] = 2000;
					break;
				case 9:
					m_CurrentNation.DarkTime[i] = 2030;
					break;
				case 10:
					m_CurrentNation.DarkTime[i] = 2100;
					break;
				}
			}
		}
	}

	private void treeStageSchedule_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (treeStageSchedule.SelectedNode != null)
		{
			if (treeStageSchedule.SelectedNode.Tag == null)
			{
				groupStageScheduleDetails.Visible = false;
				buttonStageAddTime.Enabled = false;
				buttonStageRemoveTime.Enabled = false;
				buttonDeleteStageLeg.Enabled = false;
			}
			else
			{
				m_CurrentStageSchedule = (Schedule)treeStageSchedule.SelectedNode.Tag;
				buttonDeleteStageLeg.Enabled = true;
				StageScheduleToPanel();
			}
		}
	}

	private void StageScheduleToPanel()
	{
		groupStageScheduleDetails.Visible = true;
		buttonStageAddTime.Enabled = true;
		buttonStageRemoveTime.Enabled = true;
		dateStagePicker.Value = m_CurrentStageSchedule.Date;
		numericStageMinGames.Value = m_CurrentStageSchedule.MinGames;
		numericStageMaxGames.Value = m_CurrentStageSchedule.MaxGames;
		if (m_CurrentStageSchedule.TimeIndex < 0)
		{
			m_CurrentStageSchedule.TimeIndex = 1;
		}
		comboStageTime.SelectedIndex = m_CurrentStageSchedule.TimeIndex;
	}

	private void numericStageMinGames_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentStageSchedule.MinGames = (int)numericStageMinGames.Value;
	}

	private void numericStageMaxGames_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentStageSchedule.MaxGames = (int)numericStageMaxGames.Value;
	}

	private void comboStageTime_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboStageTime.SelectedIndex >= 0 && comboStageTime.SelectedIndex != m_CurrentStageSchedule.TimeIndex)
		{
			m_CurrentStageSchedule.TimeIndex = comboStageTime.SelectedIndex;
			treeStageSchedule.SelectedNode.Text = m_CurrentStageSchedule.Date.ToString("f");
		}
	}

	private void dateStagePicker_ValueChanged(object sender, EventArgs e)
	{
		if (!(dateStagePicker.Value == m_CurrentStageSchedule.Date))
		{
			m_CurrentStageSchedule.Date = dateStagePicker.Value;
			treeStageSchedule.SelectedNode.Text = m_CurrentStageSchedule.Date.ToString("f");
		}
	}

	private void GroupScheduleToPanel()
	{
		groupGroupScheduleDetails.Visible = true;
		buttonGroupAddTime.Enabled = true;
		buttonGroupRemoveTime.Enabled = true;
		dateGroupPicker.Value = m_CurrentGroupSchedule.Date;
		numericGroupMinGames.Value = m_CurrentGroupSchedule.MinGames;
		numericGroupMaxGames.Value = m_CurrentGroupSchedule.MaxGames;
		comboGroupTime.SelectedIndex = m_CurrentGroupSchedule.TimeIndex;
	}

	private void dateGroupPicker_ValueChanged(object sender, EventArgs e)
	{
		if (!(dateGroupPicker.Value == m_CurrentGroupSchedule.Date))
		{
			m_CurrentGroupSchedule.Date = dateGroupPicker.Value;
			treeGroupSchedule.SelectedNode.Text = m_CurrentGroupSchedule.Date.ToString("f");
		}
	}

	private void comboGroupTime_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboGroupTime.SelectedIndex >= 0 && comboGroupTime.SelectedIndex != m_CurrentGroupSchedule.TimeIndex)
		{
			m_CurrentGroupSchedule.TimeIndex = comboGroupTime.SelectedIndex;
			treeGroupSchedule.SelectedNode.Text = m_CurrentGroupSchedule.Date.ToString("f");
		}
	}

	private void numericGroupMinGames_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentGroupSchedule.MinGames = (int)numericGroupMinGames.Value;
	}

	private void numericGroupMaxGames_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentGroupSchedule.MaxGames = (int)numericGroupMaxGames.Value;
	}

	private void treeGroupSchedule_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (treeGroupSchedule.SelectedNode != null)
		{
			if (treeGroupSchedule.SelectedNode.Tag == null)
			{
				groupGroupScheduleDetails.Visible = false;
				buttonGroupAddTime.Enabled = false;
				buttonGroupRemoveTime.Enabled = false;
				buttonRemoveGroupLeg.Enabled = false;
			}
			else
			{
				m_CurrentGroupSchedule = (Schedule)treeGroupSchedule.SelectedNode.Tag;
				buttonRemoveGroupLeg.Enabled = true;
				GroupScheduleToPanel();
			}
		}
	}

	private void comboInitTeam_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		for (int i = 0; i < 48; i++)
		{
			if (comboBox != m_InitTeamCombo[i])
			{
				continue;
			}
			InitTeam initTeam = m_CurrentTrophy.InitTeamArray[i];
			if (initTeam == null)
			{
				initTeam = new InitTeam(i, -1);
				m_CurrentTrophy.InitTeamArray[i] = initTeam;
			}
			if (initTeam != null)
			{
				if (comboBox.SelectedIndex == 0)
				{
					initTeam.Team = null;
				}
				else
				{
					initTeam.Team = (Team)comboBox.SelectedItem;
				}
			}
		}
	}

	private void numericNTeams_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked || m_CurrentGroup == null || numericNTeams.Value == (decimal)(m_CurrentGroup.Ranks.Count - 1))
		{
			return;
		}
		if (numericNTeams.Value >= (decimal)m_CurrentGroup.Ranks.Count)
		{
			for (int i = m_CurrentGroup.Ranks.Count; (decimal)i <= numericNTeams.Value; i++)
			{
				m_CurrentGroup.AddRank();
			}
		}
		else
		{
			int num = m_CurrentGroup.Ranks.Count - 1;
			while ((decimal)num > numericNTeams.Value)
			{
				m_CurrentGroup.RemoveRank();
				num--;
			}
		}
		GroupToPanel();
	}

	private void numericNumGames_ValueChanged_1(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentGroup.Settings.m_num_games = (int)numericNumGames.Value;
		}
	}

	private void EnableToolWorld()
	{
		if (m_CurrentCompobj == null || m_CurrentCompobj.IsWorld())
		{
			buttonAddNatiom.Visible = false;
			buttonDeleteNation.Visible = false;
			buttonAddTrophy.Visible = true;
			buttonPasteTrophy.Visible = true;
			comboTargetLeague.Visible = true;
			buttonCopyTrophy.Visible = false;
			buttonDeleteTrophy.Visible = false;
			buttonAddStage.Visible = false;
			buttonDeleteStage.Visible = false;
			buttonAddGroup.Visible = false;
			buttonDeleteGroup.Visible = false;
			buttonLoadPatch.Visible = true;
			buttonCreatePatch.Visible = false;
		}
		else if (m_CurrentCompobj.IsConfederation())
		{
			buttonAddNatiom.Visible = true;
			buttonDeleteNation.Visible = false;
			buttonAddTrophy.Visible = true;
			buttonPasteTrophy.Visible = true;
			comboTargetLeague.Visible = true;
			buttonCopyTrophy.Visible = false;
			buttonDeleteTrophy.Visible = false;
			buttonAddStage.Visible = false;
			buttonDeleteStage.Visible = false;
			buttonAddGroup.Visible = false;
			buttonDeleteGroup.Visible = false;
			buttonLoadPatch.Visible = true;
			buttonCreatePatch.Visible = true;
		}
		else if (m_CurrentCompobj.IsNation())
		{
			buttonAddNatiom.Visible = false;
			buttonDeleteNation.Visible = true;
			buttonAddTrophy.Visible = true;
			buttonPasteTrophy.Visible = true;
			comboTargetLeague.Visible = true;
			buttonCopyTrophy.Visible = false;
			buttonDeleteTrophy.Visible = false;
			buttonAddStage.Visible = false;
			buttonDeleteStage.Visible = false;
			buttonAddGroup.Visible = false;
			buttonDeleteGroup.Visible = false;
			buttonLoadPatch.Visible = true;
			buttonCreatePatch.Visible = true;
		}
		else if (m_CurrentCompobj.IsTrophy())
		{
			buttonAddNatiom.Visible = false;
			buttonDeleteNation.Visible = false;
			buttonAddTrophy.Visible = false;
			buttonPasteTrophy.Visible = false;
			comboTargetLeague.Visible = false;
			buttonCopyTrophy.Visible = true;
			buttonDeleteTrophy.Visible = true;
			buttonAddStage.Visible = false;
			buttonDeleteStage.Visible = false;
			buttonAddGroup.Visible = false;
			buttonDeleteGroup.Visible = false;
			buttonLoadPatch.Visible = false;
			buttonCreatePatch.Visible = true;
		}
		else if (m_CurrentCompobj.IsStage())
		{
			buttonAddNatiom.Visible = false;
			buttonDeleteNation.Visible = false;
			buttonAddTrophy.Visible = false;
			buttonPasteTrophy.Visible = false;
			comboTargetLeague.Visible = false;
			buttonCopyTrophy.Visible = false;
			buttonDeleteTrophy.Visible = false;
			buttonAddStage.Visible = true;
			buttonDeleteStage.Visible = true;
			buttonAddGroup.Visible = false;
			buttonDeleteGroup.Visible = false;
			buttonLoadPatch.Visible = false;
			buttonCreatePatch.Visible = false;
		}
		else if (m_CurrentCompobj.IsGroup())
		{
			buttonAddNatiom.Visible = false;
			buttonDeleteNation.Visible = false;
			buttonAddTrophy.Visible = false;
			buttonPasteTrophy.Visible = false;
			comboTargetLeague.Visible = false;
			buttonCopyTrophy.Visible = false;
			buttonDeleteTrophy.Visible = false;
			buttonAddStage.Visible = false;
			buttonDeleteStage.Visible = false;
			buttonAddGroup.Visible = true;
			buttonDeleteGroup.Visible = true;
			buttonLoadPatch.Visible = false;
			buttonCreatePatch.Visible = false;
		}
	}

	private void labelDatabaseCountry_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentNation.Country != null)
		{
			MainForm.CM.JumpTo(m_CurrentNation.Country);
		}
	}

	private void buttonAddNatiom_Click(object sender, EventArgs e)
	{
		Nation nation = new Nation(FifaEnvironment.CompetitionObjects.GetNewId(), "COUN", "NationName_XXX", m_CurrentConfederation);
		m_CurrentConfederation.Nations.Add(nation);
		FifaEnvironment.CompetitionObjects.Add(nation);
		nation.Settings.m_schedule_seasonstartmonth = m_CurrentConfederation.Settings.m_schedule_seasonstartmonth;
		nation.Settings.m_rule_numyellowstored = 3;
		TreeNode treeNode = treeWorld.SelectedNode.Nodes.Add(nation.ToString());
		treeNode.Tag = nation;
		treeNode.ForeColor = Color.Blue;
		treeWorld.SelectedNode = treeNode;
		treeWorld.Refresh();
	}

	private void buttonDeleteNation_Click(object sender, EventArgs e)
	{
		m_CurrentConfederation = (Confederation)m_CurrentNation.ParentObj;
		foreach (Trophy trophy in m_CurrentNation.Trophies)
		{
			foreach (Stage stage in trophy.Stages)
			{
				foreach (Group group in stage.Groups)
				{
					m_Competitions.RemoveId(group);
				}
				m_Competitions.RemoveId(stage);
			}
			m_Competitions.RemoveId(trophy);
		}
		m_CurrentConfederation.Nations.RemoveId(m_CurrentNation);
		treeWorld.SelectedNode.Remove();
	}

	private void buttonAddTrophy_Click(object sender, EventArgs e)
	{
		int newId = FifaEnvironment.CompetitionObjects.GetNewId();
		int num = Trophy.AutoAsset();
		string tournamentConventionalString = FifaEnvironment.Language.GetTournamentConventionalString(num, Language.ETournamentStringType.Abbr15);
		string typeString = "C" + num;
		Trophy trophy = new Trophy(newId, typeString, tournamentConventionalString, m_CurrentCompobj);
		m_CurrentCompobj.Trophies.Add(trophy);
		m_Competitions.Add(trophy);
		trophy.Settings.m_asset_id = num;
		trophy.Settings.m_rule_numsubsbench = 5;
		trophy.Settings.m_match_matchimportance = 25;
		trophy.Settings.m_comp_type = "LEAGUE";
		if (trophy.InsertStage(0))
		{
			Stage stage = (Stage)trophy.Stages[0];
			stage.InsertGroup(0);
			Group obj = (Group)stage.Groups[0];
			TreeNode treeNode = treeWorld.SelectedNode.Nodes.Add(trophy.ToString());
			treeNode.Tag = trophy;
			treeNode.ForeColor = Color.DarkGreen;
			TreeNode treeNode2 = treeNode.Nodes.Add(stage.ToString());
			treeNode2.Tag = stage;
			treeNode2.ForeColor = Color.Magenta;
			TreeNode treeNode3 = treeNode2.Nodes.Add(obj.ToString());
			treeNode3.Tag = obj;
			treeNode3.ForeColor = Color.DarkRed;
			treeWorld.SelectedNode = treeNode;
			Preset();
			treeWorld.Refresh();
		}
	}

	private void buttonDeleteTrophy_Click(object sender, EventArgs e)
	{
		if (m_CurrentTrophy.ParentObj.IsConfederation())
		{
			m_CurrentConfederation = (Confederation)m_CurrentTrophy.ParentObj;
			m_CurrentConfederation.Trophies.RemoveId(m_CurrentTrophy);
		}
		else if (m_CurrentTrophy.ParentObj.IsNation())
		{
			m_CurrentNation = (Nation)m_CurrentTrophy.ParentObj;
			m_CurrentNation.Trophies.RemoveId(m_CurrentTrophy);
		}
		else if (m_CurrentTrophy.ParentObj.IsWorld())
		{
			m_CurrentWorld.Trophies.RemoveId(m_CurrentTrophy);
		}
		foreach (Stage stage in m_CurrentTrophy.Stages)
		{
			foreach (Group group in stage.Groups)
			{
				m_Competitions.RemoveId(group);
			}
			m_Competitions.RemoveId(stage);
		}
		m_Competitions.RemoveId(m_CurrentTrophy);
		treeWorld.SelectedNode.Remove();
		Preset();
	}

	private Stage CreateFirstStage(Trophy parentTrophy)
	{
		if (!parentTrophy.InsertStage(0))
		{
			return null;
		}
		return (Stage)parentTrophy.Stages[0];
	}

	private Group CreateFirstGroup(Stage parentStage)
	{
		Group obj = new Group(FifaEnvironment.CompetitionObjects.GetNewId(), "G1", "FCE_Setup_Group", parentStage);
		parentStage.Groups.Add(obj);
		FifaEnvironment.CompetitionObjects.Add(obj);
		obj.Settings.m_num_games = 1;
		return obj;
	}

	private void buttonAddStage_Click(object sender, EventArgs e)
	{
		int num = m_CurrentTrophy.Stages.IndexOf(m_CurrentStage);
		if (num >= 0)
		{
			num++;
			if (m_CurrentTrophy.InsertStage(num))
			{
				m_CurrentStage = (Stage)m_CurrentTrophy.Stages[num];
				m_Competitions.Add(m_CurrentStage);
				m_CurrentStage.InsertGroup(0);
				Group obj = (Group)m_CurrentStage.Groups[0];
				m_Competitions.Add(obj);
				TreeNode treeNode = treeWorld.SelectedNode.Parent.Nodes.Insert(num, m_CurrentStage.ToString());
				treeNode.ForeColor = Color.Magenta;
				treeNode.Tag = m_CurrentStage;
				TreeNode treeNode2 = treeNode.Nodes.Add(obj.ToString());
				treeNode2.Tag = obj;
				treeNode2.ForeColor = Color.DarkRed;
				treeWorld.SelectedNode = treeNode;
				Preset();
			}
		}
	}

	private void buttonDeleteStage_Click(object sender, EventArgs e)
	{
		foreach (Group group in m_CurrentStage.Groups)
		{
			for (int i = 1; i < group.Ranks.Count; i++)
			{
				Rank rank = (Rank)group.Ranks[i];
				if (rank.MoveFrom != null)
				{
					rank.MoveFrom.MoveTo = null;
				}
				if (rank.MoveTo != null)
				{
					rank.MoveTo.MoveFrom = null;
				}
			}
			m_Competitions.RemoveId(group);
		}
		m_Competitions.RemoveId(m_CurrentStage);
		m_CurrentTrophy.RemoveStage(m_CurrentStage);
		treeWorld.SelectedNode.Remove();
		Preset();
	}

	private void checkCalccompavgs_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_advance_calccompavgs = (checkCalccompavgs.Checked ? 1 : (-1));
		}
	}

	private void checkRandomDraw_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_advance_randomdraw = (checkRandomDraw.Checked ? 1 : (-1));
		}
	}

	private void buttonAddGroup_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroup == null)
		{
			return;
		}
		m_ClipboardGroup = m_CurrentGroup;
		int num = m_CurrentStage.Groups.IndexOf(m_CurrentGroup);
		if (num < 0)
		{
			return;
		}
		num++;
		if (!m_CurrentStage.InsertGroup(num))
		{
			return;
		}
		m_CurrentGroup = (Group)m_CurrentStage.Groups[num];
		m_Competitions.Add(m_CurrentGroup);
		for (int i = 1; i < m_ClipboardGroup.Ranks.Count; i++)
		{
			Rank rank = new Rank(m_CurrentGroup, i);
			Rank rank2 = (Rank)m_ClipboardGroup.Ranks[i];
			if (rank2.MoveFrom != null && rank2.MoveFrom.Id == 0)
			{
				rank.MoveFrom = rank2.MoveFrom;
			}
			m_CurrentGroup.Ranks.Add(rank);
		}
		m_CurrentGroup.Settings.m_num_games = m_ClipboardGroup.Settings.m_num_games;
		TreeNode treeNode = treeWorld.SelectedNode.Parent.Nodes.Insert(num, m_CurrentGroup.ToString());
		treeNode.ForeColor = Color.Brown;
		treeNode.Tag = m_CurrentGroup;
		treeWorld.SelectedNode = treeNode;
		foreach (TreeNode node in treeWorld.SelectedNode.Parent.Nodes)
		{
			node.Text = ((Group)node.Tag).ToString();
		}
		Preset();
	}

	private void buttonDeleteGroup_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroup == null)
		{
			return;
		}
		for (int i = 1; i < m_CurrentGroup.Ranks.Count; i++)
		{
			Rank rank = (Rank)m_CurrentGroup.Ranks[i];
			if (rank.MoveFrom != null)
			{
				rank.MoveFrom.MoveTo = null;
			}
			if (rank.MoveTo != null)
			{
				rank.MoveTo.MoveFrom = null;
			}
		}
		m_CurrentStage.RemoveGroup(m_CurrentGroup);
		m_Competitions.RemoveId(m_CurrentGroup);
		TreeNode treeNode = treeWorld.SelectedNode.Parent;
		treeWorld.SelectedNode.Remove();
		foreach (TreeNode node in treeNode.Nodes)
		{
			node.Text = ((Group)node.Tag).ToString();
		}
		Preset();
	}

	private void buttonCopyTrophy_Click(object sender, EventArgs e)
	{
		m_ClipboardTrophy = m_CurrentTrophy;
		buttonPasteTrophy.Enabled = m_ClipboardTrophy != null;
		comboTargetLeague.Enabled = buttonPasteTrophy.Enabled;
	}

	private void buttonPasteTrophy_Click(object sender, EventArgs e)
	{
		if (m_ClipboardTrophy == null)
		{
			return;
		}
		bool flag = false;
		if (m_ClipboardTrophy.Stages != null && ((Stage)m_ClipboardTrophy.Stages[0]).Settings.m_match_stagetype == "LEAGUE")
		{
			flag = true;
		}
		int newId = FifaEnvironment.CompetitionObjects.GetNewId();
		League league = null;
		if (comboTargetLeague.SelectedIndex > 0)
		{
			league = (League)comboTargetLeague.SelectedItem;
		}
		int num = ((!flag || league == null) ? Trophy.AutoAsset() : league.Id);
		string tournamentConventionalString = FifaEnvironment.Language.GetTournamentConventionalString(num, Language.ETournamentStringType.Abbr15);
		string typeString = "C" + num;
		Trophy trophy = new Trophy(newId, typeString, tournamentConventionalString, m_CurrentCompobj);
		m_CurrentCompobj.Trophies.Add(trophy);
		m_Competitions.Add(trophy);
		trophy.Settings.m_asset_id = num;
		trophy.Settings.m_rule_numsubsbench = m_ClipboardTrophy.Settings.m_rule_numsubsbench;
		trophy.Settings.m_match_matchimportance = m_ClipboardTrophy.Settings.m_match_matchimportance;
		trophy.Settings.m_comp_type = m_ClipboardTrophy.Settings.m_comp_type;
		trophy.Settings.m_StandingsSort = m_ClipboardTrophy.Settings.m_StandingsSort;
		trophy.Settings.m_schedule_checkconflict = m_ClipboardTrophy.Settings.m_schedule_checkconflict;
		trophy.Settings.TrophyCompdependency = m_ClipboardTrophy.Settings.TrophyCompdependency;
		trophy.Settings.TrophyForcecomp = m_ClipboardTrophy.Settings.TrophyForcecomp;
		trophy.Settings.LeaguePromo = m_ClipboardTrophy.Settings.LeaguePromo;
		trophy.Settings.LeagueReleg = m_ClipboardTrophy.Settings.LeagueReleg;
		TreeNode treeNode = treeWorld.SelectedNode.Nodes.Add(trophy.ToString());
		treeNode.Tag = trophy;
		treeNode.ForeColor = Color.DarkGreen;
		for (int i = 0; i < m_ClipboardTrophy.Stages.Count; i++)
		{
			trophy.AddStage();
			Stage stage = (Stage)trophy.Stages[i];
			Stage stage2 = (Stage)m_ClipboardTrophy.Stages[i];
			if (stage2.Schedules != null)
			{
				foreach (Schedule schedule5 in stage2.Schedules)
				{
					Schedule schedule2 = new Schedule(stage, schedule5.Day, schedule5.Leg, schedule5.MinGames, schedule5.MaxGames, schedule5.Time);
					stage.AddSchedule(schedule2);
				}
			}
			stage.Description = stage2.Description;
			stage.Settings.m_match_stagetype = stage2.Settings.m_match_stagetype;
			stage.Settings.m_match_matchsituation = stage2.Settings.m_match_matchsituation;
			stage.Settings.m_schedule_matchreplay = stage2.Settings.m_schedule_matchreplay;
			stage.Settings.m_info_prize_money = stage2.Settings.m_info_prize_money;
			stage.Settings.m_info_prize_money_drop = stage2.Settings.m_info_prize_money_drop;
			stage.Settings.m_advance_maxteamsassoc = stage2.Settings.m_advance_maxteamsassoc;
			stage.Settings.m_advance_maxteamsgroup = stage2.Settings.m_advance_maxteamsgroup;
			stage.Settings.m_schedule_reversed = stage2.Settings.m_schedule_reversed;
			stage.Settings.Advance_standingskeep = stage2.Settings.Advance_standingskeep;
			stage.Settings.Advance_pointskeep = stage2.Settings.Advance_pointskeep;
			stage.Settings.m_advance_pointskeeppercentage = stage2.Settings.m_advance_pointskeeppercentage;
			stage.Settings.Advance_standingsrank = stage2.Settings.Advance_standingsrank;
			stage.Settings.m_EndRuleKo1Leg = stage2.Settings.m_EndRuleKo1Leg;
			stage.Settings.m_EndRuleKo2Leg2 = stage2.Settings.m_EndRuleKo2Leg2;
			stage.Settings.Standings_checkrank = stage2.Settings.Standings_checkrank;
			stage.Settings.m_advance_randomdraw = stage2.Settings.m_advance_randomdraw;
			stage.Settings.m_advance_calccompavgs = stage2.Settings.m_advance_calccompavgs;
			stage2.CopyTasks(stage, league);
			TreeNode treeNode2 = treeNode.Nodes.Add(stage.ToString());
			treeNode2.Tag = stage;
			treeNode2.ForeColor = Color.Magenta;
			for (int j = 0; j < stage2.Groups.Count; j++)
			{
				stage.InsertGroup(j);
				Group obj = (Group)stage.Groups[j];
				Group obj2 = (Group)stage2.Groups[j];
				TreeNode treeNode3 = treeNode2.Nodes.Add(obj.ToString());
				treeNode3.Tag = obj;
				treeNode3.ForeColor = Color.DarkRed;
				if (obj2.Schedules != null)
				{
					foreach (Schedule schedule6 in obj2.Schedules)
					{
						Schedule schedule4 = new Schedule(obj, schedule6.Day, schedule6.Leg, schedule6.MinGames, schedule6.MaxGames, schedule6.Time);
						obj.AddSchedule(schedule4);
					}
				}
				obj.Description = obj2.Description;
				for (int k = 1; k < obj2.Ranks.Count; k++)
				{
					obj.AddRank();
					_ = (Rank)obj2.Ranks[k];
					_ = (Rank)obj.Ranks[k];
				}
				obj.Settings.m_num_games = obj2.Settings.m_num_games;
				obj.Settings.m_StandingsSort = obj2.Settings.m_StandingsSort;
				obj.Settings.m_info_color_slot_champ = obj2.Settings.m_info_color_slot_champ;
				obj.Settings.m_info_slot_champ = obj2.Settings.m_info_slot_champ;
				obj2.Settings.GetInfoColorSlotChampCup(out var min, out var max);
				obj.Settings.SetInfoColorSlotChampCup(min, max);
				obj2.Settings.GetInfoColorSlotEuroLeague(out min, out max);
				obj.Settings.SetInfoColorSlotEuroLeague(min, max);
				obj2.Settings.GetInfoColorSlotRelegPoss(out min, out max);
				obj.Settings.SetInfoColorSlotRelegPoss(min, max);
				obj2.Settings.GetInfoColorSlotReleg(out min, out max);
				obj.Settings.SetInfoColorSlotReleg(min, max);
				obj2.Settings.GetInfoColorSlotPromo(out min, out max);
				obj.Settings.SetInfoColorSlotPromo(min, max);
				obj2.Settings.GetInfoColorSlotPromoPoss(out min, out max);
				obj.Settings.SetInfoColorSlotPromoPoss(min, max);
				obj2.Settings.GetInfoColorSlotAdvGroup(out min, out max);
				obj.Settings.SetInfoColorSlotAdvGroup(min, max);
				obj2.Settings.GetInfoSlotRelegPoss(out min, out max);
				obj.Settings.SetInfoSlotRelegPoss(min, max);
				obj2.Settings.GetInfoSlotReleg(out min, out max);
				obj.Settings.SetInfoSlotReleg(min, max);
				obj2.Settings.GetInfoSlotPromo(out min, out max);
				obj.Settings.SetInfoSlotPromo(min, max);
				obj2.Settings.GetInfoSlotPromoPoss(out min, out max);
				obj.Settings.SetInfoSlotPromoPoss(min, max);
				obj2.CopyTasks(obj, league);
			}
		}
		m_ClipboardTrophy.CopyTasks(trophy, league);
		trophy.LinkCompetitions();
		for (int l = 0; l < m_ClipboardTrophy.Stages.Count; l++)
		{
			Stage stage3 = (Stage)trophy.Stages[l];
			Stage stage4 = (Stage)m_ClipboardTrophy.Stages[l];
			stage3.LinkCompetitions();
			for (int m = 0; m < stage4.Groups.Count; m++)
			{
				Group obj3 = (Group)stage3.Groups[m];
				Group obj4 = (Group)stage4.Groups[m];
				obj3.LinkCompetitions();
				for (int n = 1; n < obj4.Ranks.Count; n++)
				{
					Rank rank = (Rank)obj4.Ranks[n];
					Rank rank2 = (Rank)obj3.Ranks[n];
					if (rank.MoveFrom != null)
					{
						int num2 = rank.MoveFrom.Group.Id - rank.Group.Id;
						int id = rank2.Group.Id + num2;
						Compobj compobj = (Compobj)m_Competitions.SearchId(id);
						if (compobj.IsGroup())
						{
							Group obj5 = (Group)compobj;
							if (obj5 != null && obj5.Ranks.Count > rank.MoveFrom.Id)
							{
								rank2.MoveFrom = (Rank)obj5.Ranks[rank.MoveFrom.Id];
							}
						}
					}
					if (rank.MoveTo == null)
					{
						continue;
					}
					int num3 = rank.MoveTo.Group.Id - rank.Group.Id;
					int id2 = rank2.Group.Id + num3;
					Compobj compobj2 = (Compobj)m_Competitions.SearchId(id2);
					if (compobj2 != null && compobj2.IsGroup())
					{
						Group obj6 = (Group)compobj2;
						if (obj6 != null)
						{
							rank2.MoveTo = (Rank)obj6.Ranks[rank.MoveTo.Id];
						}
					}
				}
			}
		}
		treeWorld.SelectedNode = treeNode;
		Preset();
		treeWorld.Refresh();
	}

	private void buttonCopyStageCalendar_Click(object sender, EventArgs e)
	{
		if (m_CurrentStage.NSchedule != 0)
		{
			m_ClipboardStageForSchedule = m_CurrentStage;
			m_ClipboardGroupForSchedule = null;
			buttonPasteStageCalendar.Enabled = true;
			buttonPasteGroupCalendar.Enabled = true;
		}
	}

	private void buttonPasteStageCalendar_Click(object sender, EventArgs e)
	{
		if (m_ClipboardStageForSchedule != null && m_CurrentStage != m_ClipboardStageForSchedule)
		{
			m_CurrentStage.RemoveAllSchedules();
			foreach (Schedule schedule3 in m_ClipboardStageForSchedule.Schedules)
			{
				int day = schedule3.Day;
				int leg = schedule3.Leg;
				int minGames = schedule3.MinGames;
				int maxGames = schedule3.MaxGames;
				int time = schedule3.Time;
				Schedule schedule = new Schedule(m_CurrentStage, day, leg, minGames, maxGames, time);
				m_CurrentStage.AddSchedule(schedule);
			}
			StageToPanel();
		}
		else
		{
			if (m_ClipboardGroupForSchedule == null || m_ClipboardGroupForSchedule.NSchedule == 0)
			{
				return;
			}
			m_CurrentStage.RemoveAllSchedules();
			foreach (Schedule schedule4 in m_ClipboardGroupForSchedule.Schedules)
			{
				int day2 = schedule4.Day;
				int leg2 = schedule4.Leg;
				int minGames2 = schedule4.MinGames;
				int maxGames2 = schedule4.MaxGames;
				int time2 = schedule4.Time;
				Schedule schedule2 = new Schedule(m_CurrentStage, day2, leg2, minGames2, maxGames2, time2);
				m_CurrentStage.AddSchedule(schedule2);
			}
			StageToPanel();
		}
	}

	private void buttonNewStageLeg_Click(object sender, EventArgs e)
	{
		int dayDelay = 7;
		if (m_CurrentStageSchedule == null || m_CurrentStage.Schedules == null || m_CurrentStage.Schedules.Count == 0)
		{
			m_CurrentStageSchedule = m_CurrentStage.AppendLeg(dayDelay);
		}
		else
		{
			m_CurrentStageSchedule = m_CurrentStage.Schedules.DuplicatetLeg(m_CurrentStageSchedule.Leg, dayDelay);
		}
		StageToPanel();
	}

	private void buttonDeleteStageLeg_Click(object sender, EventArgs e)
	{
		if (m_CurrentStageSchedule != null)
		{
			m_CurrentStage.Schedules.RemoveLeg(m_CurrentStageSchedule.Leg);
			m_CurrentStageSchedule = null;
			StageToPanel();
		}
	}

	private void buttonStageAddTime_Click(object sender, EventArgs e)
	{
		if (m_CurrentStage != null && m_CurrentStageSchedule != null)
		{
			m_CurrentStage.CloneSchedule(m_CurrentStageSchedule, 100);
			StageToPanel();
		}
	}

	private void buttonStageRemoveTime_Click(object sender, EventArgs e)
	{
		if (m_CurrentStage != null && m_CurrentStageSchedule != null)
		{
			m_CurrentStage.DeleteSchedule(m_CurrentStageSchedule);
			m_CurrentStageSchedule = null;
			StageToPanel();
		}
	}

	private void buttonCopyGroupCalendar_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroup.NSchedule > 0)
		{
			m_ClipboardStageForSchedule = null;
			m_ClipboardGroupForSchedule = m_CurrentGroup;
			buttonPasteStageCalendar.Enabled = true;
			buttonPasteGroupCalendar.Enabled = true;
		}
	}

	private void buttonPasteGroupCalendar_Click(object sender, EventArgs e)
	{
		if (m_ClipboardStageForSchedule != null && m_ClipboardStageForSchedule.NSchedule != 0)
		{
			m_CurrentGroup.RemoveAllSchedules();
			foreach (Schedule schedule3 in m_ClipboardStageForSchedule.Schedules)
			{
				int day = schedule3.Day;
				int leg = schedule3.Leg;
				int minGames = schedule3.MinGames;
				int maxGames = schedule3.MaxGames;
				int time = schedule3.Time;
				Schedule schedule = new Schedule(m_CurrentGroup, day, leg, minGames, maxGames, time);
				m_CurrentGroup.AddSchedule(schedule);
			}
			GroupToPanel();
		}
		else
		{
			if (m_ClipboardGroupForSchedule == null || m_ClipboardGroupForSchedule == m_CurrentGroup)
			{
				return;
			}
			m_CurrentGroup.RemoveAllSchedules();
			foreach (Schedule schedule4 in m_ClipboardGroupForSchedule.Schedules)
			{
				int day2 = schedule4.Day;
				int leg2 = schedule4.Leg;
				int minGames2 = schedule4.MinGames;
				int maxGames2 = schedule4.MaxGames;
				int time2 = schedule4.Time;
				Schedule schedule2 = new Schedule(m_CurrentGroup, day2, leg2, minGames2, maxGames2, time2);
				m_CurrentGroup.AddSchedule(schedule2);
			}
			GroupToPanel();
		}
	}

	private void buttonGroupAddTime_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroup != null && m_CurrentGroupSchedule != null)
		{
			m_CurrentGroup.CloneSchedule(m_CurrentGroupSchedule, 100);
			GroupToPanel();
		}
	}

	private void buttonGroupRemoveTime_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroup != null && m_CurrentGroupSchedule != null)
		{
			m_CurrentGroup.DeleteSchedule(m_CurrentGroupSchedule);
			m_CurrentGroupSchedule = null;
			GroupToPanel();
		}
	}

	private void buttonAddRule_Click(object sender, EventArgs e)
	{
		Task task = new Task("start", "", m_CurrentGroup.Id, 0, 0, 0);
		task.Group = m_CurrentGroup;
		m_CurrentGroup.AddTask(task);
		GroupToPanel();
	}

	private void buttonRemoveRule_Click(object sender, EventArgs e)
	{
		m_CurrentGroup.RemoveLastTask("start");
		GroupToPanel();
	}

	private void checkInfoChamp_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentGroup.Settings.m_info_slot_champ = (checkInfoChamp.Checked ? 1 : (-1));
		}
	}

	private void checkInfoPossibleRelegation_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericPossibleRelegationMin;
		bool visible = (numericPossibleRelegationMax.Visible = checkInfoPossibleRelegation.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoPossibleRelegation.Checked)
		{
			m_CurrentGroup.Settings.GetInfoSlotRelegPoss(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericPossibleRelegationMin.Value = min;
			numericPossibleRelegationMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoSlotRelegPoss(min2, max2);
		}
	}

	private void checkInfoRelegation_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericRelegationMin;
		bool visible = (numericRelegationMax.Visible = checkInfoRelegation.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoRelegation.Checked)
		{
			m_CurrentGroup.Settings.GetInfoSlotReleg(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericRelegationMin.Value = min;
			numericRelegationMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoSlotReleg(min2, max2);
		}
	}

	private void checkInfoPromotion_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericPromotionMin;
		bool visible = (numericPromotionMax.Visible = checkInfoPromotion.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoPromotion.Checked)
		{
			m_CurrentGroup.Settings.GetInfoSlotPromo(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericPromotionMin.Value = min;
			numericPromotionMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoSlotPromo(min2, max2);
		}
	}

	private void checkInfoPossiblePromotion_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = numericPossiblePromotionMin;
		bool visible = (numericPossiblePromotionMax.Visible = checkInfoPossiblePromotion.Checked);
		numericUpDown.Visible = visible;
		if (checkInfoPossiblePromotion.Checked)
		{
			m_CurrentGroup.Settings.GetInfoSlotPromoPoss(out var min, out var max);
			if (min <= 0 || max <= 0)
			{
				min = (max = 1);
			}
			numericPossiblePromotionMin.Value = min;
			numericPossiblePromotionMax.Value = max;
		}
		else
		{
			int min2 = -1;
			int max2 = -1;
			m_CurrentGroup.Settings.SetInfoSlotPromoPoss(min2, max2);
		}
	}

	private void numericPossibleRelegationMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericPossibleRelegationMin.Value;
			int num2 = (int)numericPossibleRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotRelegPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotRelegPoss(num, num2))
			{
				numericPossibleRelegationMin.Value = min;
			}
		}
	}

	private void numericRelegationMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericRelegationMin.Value;
			int num2 = (int)numericRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotReleg(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotReleg(num, num2))
			{
				numericRelegationMin.Value = min;
			}
		}
	}

	private void numericPromotionMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericPromotionMin.Value;
			int num2 = (int)numericPromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotPromo(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotPromo(num, num2))
			{
				numericPromotionMin.Value = min;
			}
		}
	}

	private void numericPossiblePromotionMin_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericPossiblePromotionMin.Value;
			int num2 = (int)numericPossiblePromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotPromoPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotPromoPoss(num, num2))
			{
				numericPossiblePromotionMin.Value = min;
			}
		}
	}

	private void numericPossibleRelegationMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericPossibleRelegationMin.Value;
			int num2 = (int)numericPossibleRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotRelegPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotRelegPoss(num, num2))
			{
				numericPossibleRelegationMax.Value = max;
			}
		}
	}

	private void numericRelegationMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericRelegationMin.Value;
			int num2 = (int)numericRelegationMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotReleg(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotReleg(num, num2))
			{
				numericRelegationMax.Value = max;
			}
		}
	}

	private void numericPromotionMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericPromotionMin.Value;
			int num2 = (int)numericPromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotPromo(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotPromo(num, num2))
			{
				numericPromotionMax.Value = max;
			}
		}
	}

	private void numericPossiblePromotionMax_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			int num = (int)numericPossiblePromotionMin.Value;
			int num2 = (int)numericPossiblePromotionMax.Value;
			m_CurrentGroup.Settings.GetInfoSlotPromoPoss(out var min, out var max);
			if ((num != min || num2 != max) && !m_CurrentGroup.Settings.SetInfoSlotPromoPoss(num, num2))
			{
				numericPossiblePromotionMax.Value = max;
			}
		}
	}

	private void buttonCleanGroupCalendar_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroup != null)
		{
			m_CurrentGroup.RemoveAllSchedules();
			GroupToPanel();
		}
	}

	private void buttonCleanStageCalendar_Click(object sender, EventArgs e)
	{
		if (m_CurrentStage != null)
		{
			m_CurrentStage.RemoveAllSchedules();
			StageToPanel();
		}
	}

	private void buttonNewGroupLeg_Click(object sender, EventArgs e)
	{
		int dayDelay = 7;
		if (m_CurrentGroupSchedule == null || m_CurrentGroup.Schedules.Count == 0)
		{
			m_CurrentGroupSchedule = m_CurrentGroup.AppendLeg(dayDelay);
		}
		else
		{
			m_CurrentGroupSchedule = m_CurrentGroup.Schedules.DuplicatetLeg(m_CurrentGroupSchedule.Leg, dayDelay);
		}
		GroupToPanel();
	}

	private void buttonRemoveGroupLeg_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroupSchedule != null)
		{
			m_CurrentGroup.Schedules.RemoveLeg(m_CurrentGroupSchedule.Leg);
			m_CurrentGroupSchedule = null;
			GroupToPanel();
		}
	}

	private void checkMatchReplay_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_schedule_matchreplay = (checkMatchReplay.Checked ? 1 : (-1));
		}
	}

	private void checkMaxteamsassoc_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_advance_maxteamsassoc = (checkMaxteamsassoc.Checked ? 1 : (-1));
		}
	}

	private void numericStageRef_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentStage.Settings.Advance_maxteamsstageref != (int)numericStageRef.Value)
		{
			m_CurrentStage.Settings.Advance_maxteamsstageref = (int)numericStageRef.Value;
		}
	}

	private void checkClausuraSchedule_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_schedule_reversed = (checkClausuraSchedule.Checked ? 1 : (-1));
		}
	}

	private void probUpDown_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		NumericUpDown numericUpDown = (NumericUpDown)sender;
		string text = (string)numericUpDown.Tag;
		if (text != null)
		{
			int num = Convert.ToInt32(text.Substring(1));
			if (text.StartsWith("R"))
			{
				m_CurrentNation.RainProb[num] = (int)numericUpDown.Value;
			}
			else if (text.StartsWith("S"))
			{
				m_CurrentNation.SnowProb[num] = (int)numericUpDown.Value;
			}
			else if (text.StartsWith("O"))
			{
				m_CurrentNation.OvercastProb[num] = (int)numericUpDown.Value;
			}
		}
	}

	private void TimeComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		ComboBox comboBox = (ComboBox)sender;
		string text = (string)comboBox.Tag;
		if (text != null)
		{
			int num = Convert.ToInt32(text.Substring(1));
			if (text.StartsWith("U"))
			{
				m_CurrentNation.SunsetTime[num] = ConvertIndexToTime(comboBox.SelectedIndex);
			}
			else if (text.StartsWith("N"))
			{
				m_CurrentNation.DarkTime[num] = ConvertIndexToTime(comboBox.SelectedIndex);
			}
		}
	}

	private int ConvertIndexToTime(int index)
	{
		return index switch
		{
			0 => 1600, 
			1 => 1630, 
			2 => 1700, 
			3 => 1730, 
			4 => 1800, 
			5 => 1830, 
			6 => 1900, 
			7 => 1930, 
			8 => 2000, 
			9 => 2030, 
			10 => 2100, 
			_ => 0, 
		};
	}

	private void numericBall_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentTrophy != null)
		{
			m_CurrentTrophy.ballid = (int)numericBall.Value;
			int num = (int)numericBall.Value;
			if (num >= 0)
			{
				pictureBall.BackgroundImage = Ball.GetBallPicture(num);
			}
			else
			{
				pictureBall.BackgroundImage = null;
			}
		}
	}

	private void buttonReplicateTrophy128_Click(object sender, EventArgs e)
	{
		Bitmap currentBitmap = viewer2DTrophy256.CurrentBitmap;
		Rectangle srcRect = new Rectangle(0, 0, 256, 256);
		Bitmap bitmap = new Bitmap(128, 128, PixelFormat.Format32bppPArgb);
		Rectangle destRect = new Rectangle(0, 0, 128, 128);
		GraphicUtil.RemapRectangle(currentBitmap, srcRect, bitmap, destRect);
		m_CurrentTrophy.SetTrophy128(bitmap);
		viewer2DTrophy128.CurrentBitmap = bitmap;
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		Show3DTrophy();
	}

	private void buttonImport3DModel_Click(object sender, EventArgs e)
	{
		string text = FifaEnvironment.BrowseAndCheckModel(ref m_TrophyCurrentFolder, "Open 3D Trophy Model file", "3D trophy model files (*.rx3)|trophy_*.rx3");
		if (text != null)
		{
			bool result = FifaEnvironment.Year == 26
				? Fc26DirectAssetUi.Import(this, m_CurrentTrophy.ModelFileName(), text, "Trophy 3D model")
				: m_CurrentTrophy.SetModel(text);
			if (result) ReloadTrophy(m_CurrentTrophy);
		}
	}

	private void buttonExport3DModel_Click(object sender, EventArgs e)
	{
		string text = m_CurrentTrophy.ModelFileName();
		if (text != null)
		{
			if (FifaEnvironment.Year == 26)
				Fc26DirectAssetUi.ExportWithDialog(this, text, ref m_TrophyCurrentFolder, "Trophy 3D model");
			else
				FifaEnvironment.AskAndExportFromZdata(text, ref m_TrophyCurrentFolder);
		}
	}

	private void buttonRemove3DModel_Click(object sender, EventArgs e)
	{
		bool result = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Remove(this, m_CurrentTrophy.ModelFileName(), "Trophy 3D model")
			: m_CurrentTrophy.DeleteModel();
		if (result) ReloadTrophy(m_CurrentTrophy);
	}

	private void tabTrophy_SelectedIndexChanged(object sender, EventArgs e)
	{
		ReloadTrophy(m_CurrentTrophy);
	}

	private void labelUpdateTable_Click(object sender, EventArgs e)
	{
		Label label = (Label)sender;
		int num = -1;
		Task task = (Task)label.Tag;
		Task task2 = null;
		Rank rank = null;
		for (int i = 0; i < m_UpdateTableLabels.Length; i++)
		{
			if (label == m_UpdateTableLabels[i])
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		rank = ((task == null || task.Group == null || task.Group.Ranks == null || task.Parameter2 >= task.Group.Ranks.Count) ? new Rank((Group)((Stage)m_CurrentTrophy.Stages[0]).Groups[0], 1) : ((Rank)task.Group.Ranks[task.Parameter2]));
		m_RankingRuleDialog.Rank = rank;
		if (m_RankingRuleDialog.ShowDialog() == DialogResult.OK)
		{
			task2 = new Task("end", "UpdateTable", m_CurrentTrophy.Id, m_RankingRuleDialog.Rank.Group.Id, m_RankingRuleDialog.Rank.Id, num + 1);
			task2.LinkTrophy(m_CurrentTrophy);
			task2.Group = m_RankingRuleDialog.Rank.Group;
			label.Tag = task2;
			if (task == null)
			{
				m_CurrentTrophy.AddTask(task2);
			}
			else
			{
				m_CurrentTrophy.ReplaceTask(task, task2);
			}
		}
		TrophyRankingToPanel();
	}

	private void numericUpdateTableEntries_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked || numericUpdateTableEntries.Value == (decimal)m_NUpdateTableLabels)
		{
			return;
		}
		for (int i = 0; i < 48; i++)
		{
			m_InitTeamPanel[i].Visible = (decimal)i < numericUpdateTableEntries.Value;
		}
		int num = (int)numericUpdateTableEntries.Value;
		if (num < m_NUpdateTableLabels)
		{
			for (int j = num; j < m_NUpdateTableLabels; j++)
			{
				Task task = (Task)m_UpdateTableLabels[j].Tag;
				if (task != null)
				{
					m_CurrentTrophy.RemoveTask(task);
					m_UpdateTableLabels[j].Tag = null;
					m_UpdateTableLabels[j].Text = null;
				}
			}
		}
		else
		{
			for (int k = m_NUpdateTableLabels; k < num; k++)
			{
				m_UpdateTableLabels[k].Tag = null;
				m_UpdateTableLabels[k].Text = null;
			}
		}
		m_NUpdateTableLabels = num;
	}

	private void numericInternationalFirstYear_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentTrophy.Settings.m_schedule_year_start = (int)numericInternationalFirstYear.Value;
		}
	}

	private void numericInternationalPeriodicity_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentTrophy.Settings.m_schedule_year_offset = (int)numericInternationalPeriodicity.Value;
		}
	}

	private void checkClearLeagueStats_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		comboLeagueStats.Visible = checkClearLeagueStats.Checked || checkUpdateLeagueStats.Checked || checkUpdateLeagueTable.Checked;
		if (checkClearLeagueStats.Checked)
		{
			League league = (League)comboLeagueStats.SelectedItem;
			if (league != null)
			{
				Task task = new Task("start", "ClearLeagueStats", m_CurrentStage.Id, league.Id, 0, 0);
				task.LinkStage(m_CurrentStage);
				int num = m_CurrentStage.SearchTaskIndex("start", "ClearLeagueStats", -1, -1, -1);
				if (num >= 0)
				{
					m_CurrentStage.ReplaceTask(task, num);
				}
				else
				{
					m_CurrentStage.AddTask(task);
				}
			}
		}
		else
		{
			m_CurrentStage.RemoveTask("start", "ClearLeagueStats", -1, -1, -1);
		}
	}

	private void checkUpdateLeagueStats_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		comboLeagueStats.Visible = checkClearLeagueStats.Checked || checkUpdateLeagueStats.Checked || checkUpdateLeagueTable.Checked;
		if (checkUpdateLeagueStats.Checked)
		{
			League league = (League)comboLeagueStats.SelectedItem;
			if (league != null)
			{
				Task task = new Task("end", "UpdateLeagueStats", m_CurrentStage.Id, league.Id, 0, 0);
				task.LinkStage(m_CurrentStage);
				int num = m_CurrentStage.SearchTaskIndex("end", "UpdateLeagueStats", -1, -1, -1);
				if (num >= 0)
				{
					m_CurrentStage.ReplaceTask(task, num);
				}
				else
				{
					m_CurrentStage.AddTask(task);
				}
			}
		}
		else
		{
			m_CurrentStage.RemoveTask("end", "UpdateLeagueStats", -1, -1, -1);
		}
	}

	private void checkUpdateLeagueTable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		comboLeagueStats.Visible = checkClearLeagueStats.Checked || checkUpdateLeagueStats.Checked || checkUpdateLeagueTable.Checked;
		if (checkUpdateLeagueTable.Checked)
		{
			League league = (League)comboLeagueStats.SelectedItem;
			if (league != null)
			{
				Task task = new Task("end", "UpdateLeagueTable", m_CurrentStage.Id, league.Id, 0, 0);
				task.LinkStage(m_CurrentStage);
				int num = m_CurrentStage.SearchTaskIndex("end", "UpdateLeagueTable", -1, -1, -1);
				if (num >= 0)
				{
					m_CurrentStage.ReplaceTask(task, num);
				}
				else
				{
					m_CurrentStage.AddTask(task);
				}
			}
		}
		else
		{
			m_CurrentStage.RemoveTask("end", "UpdateLeagueTable", -1, -1, -1);
		}
	}

	private void comboLeagueStats_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_Locked)
		{
			return;
		}
		League league = (League)comboLeagueStats.SelectedItem;
		Task task = null;
		if (league == null)
		{
			return;
		}
		if (checkClearLeagueStats.Checked)
		{
			task = new Task("start", "ClearLeagueStats", m_CurrentStage.Id, league.Id, 0, 0);
			task.LinkStage(m_CurrentStage);
			int num = m_CurrentStage.SearchTaskIndex("start", "ClearLeagueStats", -1, -1, -1);
			if (num >= 0)
			{
				m_CurrentStage.ReplaceTask(task, num);
			}
			else
			{
				m_CurrentStage.AddTask(task);
			}
		}
		else
		{
			m_CurrentStage.RemoveTask("start", "ClearLeagueStats", -1, -1, -1);
		}
		if (checkUpdateLeagueStats.Checked)
		{
			task = new Task("end", "UpdateLeagueStats", m_CurrentStage.Id, league.Id, 0, 0);
			task.LinkStage(m_CurrentStage);
			int num2 = m_CurrentStage.SearchTaskIndex("end", "UpdateLeagueStats", -1, -1, -1);
			if (num2 >= 0)
			{
				m_CurrentStage.ReplaceTask(task, num2);
			}
			else
			{
				m_CurrentStage.AddTask(task);
			}
		}
		else
		{
			m_CurrentStage.RemoveTask("end", "UpdateLeagueStats", -1, -1, -1);
		}
		if (checkUpdateLeagueTable.Checked)
		{
			task = new Task("end", "UpdateLeagueTable", m_CurrentStage.Id, league.Id, 0, 0);
			task.LinkStage(m_CurrentStage);
			int num3 = m_CurrentStage.SearchTaskIndex("end", "UpdateLeagueTable", -1, -1, -1);
			if (num3 >= 0)
			{
				m_CurrentStage.ReplaceTask(task, num3);
			}
			else
			{
				m_CurrentStage.AddTask(task);
			}
		}
		else
		{
			m_CurrentStage.RemoveTask("end", "UpdateLeagueTable", -1, -1, -1);
		}
	}

	private void checkStageStandingsRules_CheckedChanged(object sender, EventArgs e)
	{
		comboStageStandingRules.Visible = checkStageStandingsRules.Checked;
		if (checkStageStandingsRules.Checked)
		{
			m_CurrentStage.Settings.m_StandingsSort = comboStageStandingRules.SelectedIndex;
		}
		else
		{
			m_CurrentStage.Settings.m_StandingsSort = -1;
		}
	}

	private void comboStageStandingRules_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboStageStandingRules.SelectedIndex >= 0)
		{
			m_CurrentStage.Settings.m_StandingsSort = comboStageStandingRules.SelectedIndex;
		}
	}

	private void comboTrophyStartMonth_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboTrophyStartMonth.SelectedItem != null)
		{
			m_CurrentTrophy.Settings.m_schedule_seasonstartmonth = (string)comboTrophyStartMonth.SelectedItem;
		}
	}

	private void checkRandomDrawEvent_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_advance_random_draw_event = (checkRandomDrawEvent.Checked ? 1 : (-1));
		}
	}

	private void tabCompetitions_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_LockTree)
		{
			m_LockTree = true;
			if (tabCompetitions.SelectedTab == pageWorld)
			{
				SelectWorldTreeNode(m_CurrentWorld);
			}
			else if (tabCompetitions.SelectedTab == pageConfederation)
			{
				SelectWorldTreeNode(m_CurrentConfederation);
			}
			else if (tabCompetitions.SelectedTab == pageNation)
			{
				SelectWorldTreeNode(m_CurrentNation);
			}
			else if (tabCompetitions.SelectedTab == pageTrophy)
			{
				SelectWorldTreeNode(m_CurrentTrophy);
			}
			else if (tabCompetitions.SelectedTab == pageStage)
			{
				SelectWorldTreeNode(m_CurrentStage);
			}
			else if (tabCompetitions.SelectedTab == pageGroup)
			{
				SelectWorldTreeNode(m_CurrentGroup);
			}
			m_LockTree = false;
		}
		CompetitionToPanel();
	}

	private void CompetitionToPanel()
	{
		if (tabCompetitions.SelectedTab == pageWorld)
		{
			WorldToPanel();
		}
		else if (tabCompetitions.SelectedTab == pageConfederation)
		{
			ConfederationToPanel();
		}
		else if (tabCompetitions.SelectedTab == pageNation)
		{
			NationToPanel();
		}
		else if (tabCompetitions.SelectedTab == pageTrophy)
		{
			TrophyToPanel();
		}
		else if (tabCompetitions.SelectedTab == pageStage)
		{
			StageToPanel();
		}
		else if (tabCompetitions.SelectedTab == pageGroup)
		{
			GroupToPanel();
		}
	}

	private void buttongroupSortLegs_Click(object sender, EventArgs e)
	{
		if (m_CurrentGroup != null && m_CurrentGroup.Schedules != null)
		{
			m_CurrentGroup.Schedules.RenumberLegs();
			GroupToPanel();
		}
	}

	private void buttonStageSortLegs_Click(object sender, EventArgs e)
	{
		if (m_CurrentStage != null && m_CurrentStage.Schedules != null)
		{
			m_CurrentStage.Schedules.RenumberLegs();
			StageToPanel();
		}
	}

	private void checkScheduleUseDates_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked && checkScheduleUseDates.Checked != (m_CurrentTrophy.Settings.m_schedule_use_dates_comp != -1))
		{
			m_CurrentTrophy.Settings.m_schedule_use_dates_comp = (checkScheduleUseDates.Checked ? FifaEnvironment.CompetitionObjects.GetInternationalFriendlyId() : (-1));
		}
	}

	private void numericKeepPointsStageRef_ValueChanged(object sender, EventArgs e)
	{
		if (!m_Locked && m_CurrentStage.Settings.Advance_pointskeep != (int)numericKeepPointsStageRef.Value)
		{
			m_CurrentStage.Settings.Advance_pointskeep = (int)numericKeepPointsStageRef.Value;
		}
	}

	private void buttonReplicateTropy_Click(object sender, EventArgs e)
	{
		Bitmap currentBitmap = viewer2DTrophy256.CurrentBitmap;
		Rectangle srcRect = new Rectangle(0, 0, 256, 256);
		Bitmap bitmap = new Bitmap(256, 256, PixelFormat.Format32bppPArgb);
		Rectangle destRect = new Rectangle(0, 0, 192, 192);
		GraphicUtil.RemapRectangle(currentBitmap, srcRect, bitmap, destRect);
		m_CurrentTrophy.SetTrophy(bitmap);
		viewer2DTrophy.CurrentBitmap = bitmap;
	}

	private void checkLowCelebrationLevel_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentTrophy.Settings.m_match_celebrationlevel = (checkLowCelebrationLevel.Checked ? "LOW" : null);
		}
	}

	private void checkCanUseFancards_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentStage.Settings.m_match_canusefancards = (checkCanUseFancards.Checked ? "on" : null);
		}
	}

	private void buttonCreatePatch_Click(object sender, EventArgs e)
	{
		DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(33);
		if (dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
		{
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "cmc files (*.cmc)|*.cmc";
		saveFileDialog.InitialDirectory = FifaEnvironment.TempFolder;
		saveFileDialog.FileName = m_CurrentCompobj.ToString();
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.Title = "Save Creation Master Competition-Patch";
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			saveFileDialog.Dispose();
			return;
		}
		m_PatchFileName = saveFileDialog.FileName;
		saveFileDialog.Dispose();
		m_TempFolder = FifaEnvironment.TempFolder + "\\Patch\\";
		if (Directory.Exists(m_TempFolder))
		{
			Directory.Delete(m_TempFolder, recursive: true);
			while (Directory.Exists(m_TempFolder))
			{
			}
		}
		Directory.CreateDirectory(m_TempFolder);
		m_PatchCompetitionFileNames = CompobjList.GetFileNames();
		m_PatchStreamWriters = new StreamWriter[m_PatchCompetitionFileNames.Length];
		for (int i = 0; i < m_PatchCompetitionFileNames.Length; i++)
		{
			m_PatchCompetitionFileNames[i] = m_TempFolder + Path.GetFileName(m_PatchCompetitionFileNames[i]);
			m_PatchStreamWriters[i] = new StreamWriter(m_PatchCompetitionFileNames[i]);
		}
		if (m_CurrentCompobj.IsNation())
		{
			CreateNationPatch();
		}
		else if (m_CurrentCompobj.IsTrophy())
		{
			CreateTrophyPatch();
		}
		else if (m_CurrentCompobj.IsConfederation())
		{
			CreateConfederationPatch();
		}
		for (int j = 0; j < m_PatchStreamWriters.Length; j++)
		{
			m_PatchStreamWriters[j].Close();
		}
		ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(m_PatchFileName));
		zipOutputStream.SetLevel(8);
		string[] files = Directory.GetFiles(m_TempFolder, "*.*", SearchOption.AllDirectories);
		if (files != null)
		{
			int length = m_TempFolder.Length;
			foreach (string obj in files)
			{
				string fileName = obj.Substring(length);
				FileStream fileStream = File.OpenRead(obj);
				AddStreamToZip(zipOutputStream, fileStream, fileName);
				fileStream.Close();
			}
			zipOutputStream.Finish();
			zipOutputStream.Close();
		}
	}

	private void buttonLoadPatch_Click(object sender, EventArgs e)
	{
		DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(33);
		if (dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
		{
			return;
		}
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.CheckFileExists = true;
		openFileDialog.Title = "Open Creation Master Competition-Patch file";
		openFileDialog.Filter = "Creation Master Competition-Patch (*.cmc)|*.cmc";
		openFileDialog.FilterIndex = 1;
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			openFileDialog.Dispose();
			return;
		}
		string fileName = openFileDialog.FileName;
		openFileDialog.Dispose();
		if (!File.Exists(fileName))
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		m_TempFolder = FifaEnvironment.TempFolder + "\\Patch\\";
		if (Directory.Exists(m_TempFolder))
		{
			Directory.Delete(m_TempFolder, recursive: true);
		}
		Directory.CreateDirectory(m_TempFolder);
		FileStream baseInputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		new ZipFile(fileName);
		ZipInputStream zipInputStream = new ZipInputStream(baseInputStream);
		ZipExtractAllFiles(zipInputStream, m_TempFolder);
		zipInputStream.Close();
		m_PatchCompetitionFileNames = CompobjList.GetFileNames();
		for (int i = 0; i < m_PatchCompetitionFileNames.Length; i++)
		{
			m_PatchCompetitionFileNames[i] = m_TempFolder + Path.GetFileName(m_PatchCompetitionFileNames[i]);
		}
		CompobjList compobjList = new CompobjList();
		if (compobjList.LoadFromCompobj(m_PatchCompetitionFileNames[0], m_CurrentCompobj))
		{
			compobjList.LoadFromSettings(m_PatchCompetitionFileNames[1]);
			compobjList.LoadFromStandings(m_PatchCompetitionFileNames[2]);
			compobjList.LoadFromAdvancement(m_PatchCompetitionFileNames[3]);
			compobjList.LoadFromSchedule(m_PatchCompetitionFileNames[4]);
			compobjList.LoadFromWeather(m_PatchCompetitionFileNames[5]);
			compobjList.LoadFromTasks(m_PatchCompetitionFileNames[6]);
			compobjList.LoadFromInitteams(m_PatchCompetitionFileNames[7]);
			compobjList.LoadFromInternationals(m_PatchCompetitionFileNames[9]);
			compobjList.Link();
			m_CurrentWorld.Renumber(0);
			for (int j = 0; j < compobjList.Count; j++)
			{
				FifaEnvironment.CompetitionObjects.Add(compobjList[j]);
			}
			WorldStructureToPanel();
		}
	}

	private void CreateConfederationPatch()
	{
		m_CurrentConfederation.SaveRecursivelyToCompobj(m_PatchStreamWriters[0]);
		m_CurrentConfederation.SaveRecursivelyToSettings(m_PatchStreamWriters[1]);
		m_CurrentConfederation.SaveRecursivelyToStandings(m_PatchStreamWriters[2]);
		m_CurrentConfederation.SaveRecursivelyToAdvancement(m_PatchStreamWriters[3]);
		m_CurrentConfederation.SaveRecursivelyToSchedule(m_PatchStreamWriters[4]);
		m_CurrentConfederation.SaveRecursivelyToWeather(m_PatchStreamWriters[5]);
		m_CurrentConfederation.SaveRecursivelyToTasks(m_PatchStreamWriters[6]);
		m_CurrentConfederation.SaveRecursivelyToInitteams(m_PatchStreamWriters[7]);
		m_CurrentConfederation.SaveRecursivelyToCompids(m_PatchStreamWriters[8]);
	}

	private void CreateNationPatch()
	{
		m_CurrentNation.SaveRecursivelyToCompobj(m_PatchStreamWriters[0]);
		m_CurrentNation.SaveRecursivelyToSettings(m_PatchStreamWriters[1]);
		m_CurrentNation.SaveRecursivelyToStandings(m_PatchStreamWriters[2]);
		m_CurrentNation.SaveRecursivelyToAdvancement(m_PatchStreamWriters[3]);
		m_CurrentNation.SaveRecursivelyToSchedule(m_PatchStreamWriters[4]);
		m_CurrentNation.SaveRecursivelyToWeather(m_PatchStreamWriters[5]);
		m_CurrentNation.SaveRecursivelyToTasks(m_PatchStreamWriters[6]);
		m_CurrentNation.SaveRecursivelyToInitteams(m_PatchStreamWriters[7]);
		m_CurrentNation.SaveRecursivelyToCompids(m_PatchStreamWriters[8]);
	}

	private void CreateTrophyPatch()
	{
		m_CurrentTrophy.SaveRecursivelyToCompobj(m_PatchStreamWriters[0]);
		m_CurrentTrophy.SaveRecursivelyToSettings(m_PatchStreamWriters[1]);
		m_CurrentTrophy.SaveRecursivelyToStandings(m_PatchStreamWriters[2]);
		m_CurrentTrophy.SaveRecursivelyToAdvancement(m_PatchStreamWriters[3]);
		m_CurrentTrophy.SaveRecursivelyToSchedule(m_PatchStreamWriters[4]);
		m_CurrentTrophy.SaveRecursivelyToWeather(m_PatchStreamWriters[5]);
		m_CurrentTrophy.SaveRecursivelyToTasks(m_PatchStreamWriters[6]);
		m_CurrentTrophy.SaveRecursivelyToInitteams(m_PatchStreamWriters[7]);
		m_CurrentTrophy.SaveRecursivelyToCompids(m_PatchStreamWriters[8]);
	}

	private void LoadNationPatch()
	{
	}

	private void LoadTrophyPatch()
	{
	}

	private bool AddStreamToZip(ZipOutputStream zip, Stream inputStream, string fileName)
	{
		if (inputStream == null)
		{
			return false;
		}
		Crc32 crc = new Crc32();
		byte[] array = new byte[inputStream.Length];
		inputStream.Read(array, 0, array.Length);
		ZipEntry zipEntry = new ZipEntry(fileName);
		zipEntry.DateTime = DateTime.Now;
		zipEntry.Size = inputStream.Length;
		crc.Reset();
		crc.Update(array);
		zipEntry.Crc = crc.Value;
		zip.PutNextEntry(zipEntry);
		zip.Write(array, 0, array.Length);
		return true;
	}

	private void ZipExtractAllFiles(ZipInputStream zip, string exportFolder)
	{
		ZipEntry nextEntry;
		while ((nextEntry = zip.GetNextEntry()) != null)
		{
			ZipExtractSingleFile(zip, nextEntry, exportFolder);
		}
	}

	private void ZipExtractSingleFile(ZipInputStream zip, ZipEntry zipEntry, string exportFolder)
	{
		string path = exportFolder + "\\" + Path.GetDirectoryName(zipEntry.Name);
		if (!(Path.GetFileName(zipEntry.Name) != string.Empty))
		{
			return;
		}
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		FileStream fileStream = File.Create(exportFolder + "\\" + zipEntry.Name);
		int num = 2048;
		byte[] array = new byte[2048];
		while (true)
		{
			num = zip.Read(array, 0, array.Length);
			if (num <= 0)
			{
				break;
			}
			fileStream.Write(array, 0, num);
		}
		fileStream.Close();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.CompetitionForm));
		this.treeWorld = new System.Windows.Forms.TreeView();
		this.groupConfederation = new System.Windows.Forms.GroupBox();
		this.comboConfederationStartingMonth = new System.Windows.Forms.ComboBox();
		this.labelConfStartMonth = new System.Windows.Forms.Label();
		this.groupNation = new System.Windows.Forms.GroupBox();
		this.groupWeather = new System.Windows.Forms.GroupBox();
		this.label30 = new System.Windows.Forms.Label();
		this.label29 = new System.Windows.Forms.Label();
		this.numericUpDown97 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown98 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown99 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown100 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown101 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown102 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown103 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown104 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown105 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown106 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown107 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown108 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown85 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown86 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown87 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown88 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown89 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown90 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown91 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown92 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown93 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown94 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown95 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown96 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown73 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown74 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown75 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown76 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown77 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown78 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown79 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown80 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown81 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown82 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown83 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown84 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown61 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown62 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown63 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown64 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown65 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown66 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown67 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown68 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown69 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown70 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown71 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown72 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown49 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown50 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown51 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown52 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown53 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown54 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown55 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown56 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown57 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown58 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown59 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown60 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown37 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown38 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown39 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown40 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown41 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown42 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown43 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown44 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown45 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown46 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown47 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown48 = new System.Windows.Forms.NumericUpDown();
		this.label80 = new System.Windows.Forms.Label();
		this.label76 = new System.Windows.Forms.Label();
		this.label77 = new System.Windows.Forms.Label();
		this.label78 = new System.Windows.Forms.Label();
		this.label79 = new System.Windows.Forms.Label();
		this.label74 = new System.Windows.Forms.Label();
		this.label75 = new System.Windows.Forms.Label();
		this.label73 = new System.Windows.Forms.Label();
		this.label72 = new System.Windows.Forms.Label();
		this.toolWeather = new System.Windows.Forms.ToolStrip();
		this.buttonCopyWeather = new System.Windows.Forms.ToolStripButton();
		this.buttonPasteWeather = new System.Windows.Forms.ToolStripButton();
		this.comboBox23 = new System.Windows.Forms.ComboBox();
		this.comboBox24 = new System.Windows.Forms.ComboBox();
		this.numericUpDown34 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown35 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown36 = new System.Windows.Forms.NumericUpDown();
		this.comboBox21 = new System.Windows.Forms.ComboBox();
		this.comboBox22 = new System.Windows.Forms.ComboBox();
		this.numericUpDown31 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown32 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown33 = new System.Windows.Forms.NumericUpDown();
		this.comboBox19 = new System.Windows.Forms.ComboBox();
		this.comboBox20 = new System.Windows.Forms.ComboBox();
		this.numericUpDown28 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown29 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown30 = new System.Windows.Forms.NumericUpDown();
		this.comboBox17 = new System.Windows.Forms.ComboBox();
		this.comboBox18 = new System.Windows.Forms.ComboBox();
		this.numericUpDown25 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown26 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown27 = new System.Windows.Forms.NumericUpDown();
		this.comboBox15 = new System.Windows.Forms.ComboBox();
		this.comboBox16 = new System.Windows.Forms.ComboBox();
		this.numericUpDown22 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown23 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown24 = new System.Windows.Forms.NumericUpDown();
		this.comboBox13 = new System.Windows.Forms.ComboBox();
		this.comboBox14 = new System.Windows.Forms.ComboBox();
		this.numericUpDown19 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown20 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown21 = new System.Windows.Forms.NumericUpDown();
		this.comboBox11 = new System.Windows.Forms.ComboBox();
		this.comboBox12 = new System.Windows.Forms.ComboBox();
		this.numericUpDown16 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown17 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown18 = new System.Windows.Forms.NumericUpDown();
		this.comboBox9 = new System.Windows.Forms.ComboBox();
		this.comboBox10 = new System.Windows.Forms.ComboBox();
		this.numericUpDown13 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown14 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown15 = new System.Windows.Forms.NumericUpDown();
		this.comboBox7 = new System.Windows.Forms.ComboBox();
		this.comboBox8 = new System.Windows.Forms.ComboBox();
		this.numericUpDown10 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown11 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown12 = new System.Windows.Forms.NumericUpDown();
		this.comboBox5 = new System.Windows.Forms.ComboBox();
		this.comboBox6 = new System.Windows.Forms.ComboBox();
		this.numericUpDown7 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown8 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown9 = new System.Windows.Forms.NumericUpDown();
		this.comboBox3 = new System.Windows.Forms.ComboBox();
		this.comboBox4 = new System.Windows.Forms.ComboBox();
		this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown6 = new System.Windows.Forms.NumericUpDown();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.comboBox2 = new System.Windows.Forms.ComboBox();
		this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
		this.label28 = new System.Windows.Forms.Label();
		this.label27 = new System.Windows.Forms.Label();
		this.label26 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.label24 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.label22 = new System.Windows.Forms.Label();
		this.label21 = new System.Windows.Forms.Label();
		this.label20 = new System.Windows.Forms.Label();
		this.label19 = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.labelDatabaseCountry = new System.Windows.Forms.Label();
		this.comboCountry = new System.Windows.Forms.ComboBox();
		this.comboNationStandingsRules = new System.Windows.Forms.ComboBox();
		this.checkNationStandingsRules = new System.Windows.Forms.CheckBox();
		this.numericNationYellowsStored = new System.Windows.Forms.NumericUpDown();
		this.comboNationStartMonth = new System.Windows.Forms.ComboBox();
		this.groupTrophy = new System.Windows.Forms.GroupBox();
		this.numericAdvanceFrom = new System.Windows.Forms.NumericUpDown();
		this.checkAdvanceFrom = new System.Windows.Forms.CheckBox();
		this.checkLowCelebrationLevel = new System.Windows.Forms.CheckBox();
		this.groupInternationalschedule = new System.Windows.Forms.GroupBox();
		this.label71 = new System.Windows.Forms.Label();
		this.comboTrophyStartMonth = new System.Windows.Forms.ComboBox();
		this.numericInternationalPeriodicity = new System.Windows.Forms.NumericUpDown();
		this.label69 = new System.Windows.Forms.Label();
		this.label68 = new System.Windows.Forms.Label();
		this.numericInternationalFirstYear = new System.Windows.Forms.NumericUpDown();
		this.label67 = new System.Windows.Forms.Label();
		this.numericBall = new System.Windows.Forms.NumericUpDown();
		this.pictureBall = new System.Windows.Forms.PictureBox();
		this.groupBenchPlayers = new System.Windows.Forms.GroupBox();
		this.radioBench7Players = new System.Windows.Forms.RadioButton();
		this.radioBench5Players = new System.Windows.Forms.RadioButton();
		this.comboTrophyStandingRules = new System.Windows.Forms.ComboBox();
		this.labelTrophyShortName = new System.Windows.Forms.Label();
		this.labelMatchImportance = new System.Windows.Forms.Label();
		this.labelCompetitionType = new System.Windows.Forms.Label();
		this.numericImportance = new System.Windows.Forms.NumericUpDown();
		this.labelAssetId = new System.Windows.Forms.Label();
		this.comboCompetitionType = new System.Windows.Forms.ComboBox();
		this.checkTrophyStandingsRules = new System.Windows.Forms.CheckBox();
		this.buttonGetId = new System.Windows.Forms.Button();
		this.groupPromotionRelegation = new System.Windows.Forms.GroupBox();
		this.comboRelegationLeague = new System.Windows.Forms.ComboBox();
		this.comboPromotionLeague = new System.Windows.Forms.ComboBox();
		this.checkPromotionLeague = new System.Windows.Forms.CheckBox();
		this.checkRelegationLeague = new System.Windows.Forms.CheckBox();
		this.numericAssetId = new System.Windows.Forms.NumericUpDown();
		this.groupSchedule = new System.Windows.Forms.GroupBox();
		this.checkScheduleUseDates = new System.Windows.Forms.CheckBox();
		this.checkScheduleConflicts = new System.Windows.Forms.CheckBox();
		this.comboSchedForce = new System.Windows.Forms.ComboBox();
		this.checkForceSchedule = new System.Windows.Forms.CheckBox();
		this.textTrophyLongName = new System.Windows.Forms.TextBox();
		this.labeTrophylLongName = new System.Windows.Forms.Label();
		this.textTrophyShortName = new System.Windows.Forms.TextBox();
		this.groupStage = new System.Windows.Forms.GroupBox();
		this.groupPlayStage = new System.Windows.Forms.GroupBox();
		this.checkCanUseFancards = new System.Windows.Forms.CheckBox();
		this.numericKeepPointsStageRef = new System.Windows.Forms.NumericUpDown();
		this.checkRandomDrawEvent = new System.Windows.Forms.CheckBox();
		this.groupLeaguetasks = new System.Windows.Forms.GroupBox();
		this.checkUpdateLeagueTable = new System.Windows.Forms.CheckBox();
		this.comboLeagueStats = new System.Windows.Forms.ComboBox();
		this.checkUpdateLeagueStats = new System.Windows.Forms.CheckBox();
		this.checkClearLeagueStats = new System.Windows.Forms.CheckBox();
		this.groupStageSchedules = new System.Windows.Forms.GroupBox();
		this.treeStageSchedule = new System.Windows.Forms.TreeView();
		this.panelStageScheduleDetails = new System.Windows.Forms.Panel();
		this.groupStageScheduleDetails = new System.Windows.Forms.GroupBox();
		this.dateStagePicker = new System.Windows.Forms.DateTimePicker();
		this.label37 = new System.Windows.Forms.Label();
		this.numericStageMinGames = new System.Windows.Forms.NumericUpDown();
		this.label36 = new System.Windows.Forms.Label();
		this.numericStageMaxGames = new System.Windows.Forms.NumericUpDown();
		this.label35 = new System.Windows.Forms.Label();
		this.comboStageTime = new System.Windows.Forms.ComboBox();
		this.label34 = new System.Windows.Forms.Label();
		this.toolStageSchedule = new System.Windows.Forms.ToolStrip();
		this.buttonCopyStageCalendar = new System.Windows.Forms.ToolStripButton();
		this.buttonPasteStageCalendar = new System.Windows.Forms.ToolStripButton();
		this.buttonCleanStageCalendar = new System.Windows.Forms.ToolStripButton();
		this.buttonNeewStageLeg = new System.Windows.Forms.ToolStripButton();
		this.buttonDeleteStageLeg = new System.Windows.Forms.ToolStripButton();
		this.buttonStageAddTime = new System.Windows.Forms.ToolStripButton();
		this.buttonStageRemoveTime = new System.Windows.Forms.ToolStripButton();
		this.buttonStageSortLegs = new System.Windows.Forms.ToolStripButton();
		this.numericRegularSeason = new System.Windows.Forms.NumericUpDown();
		this.comboSpecialKo2Rule = new System.Windows.Forms.ComboBox();
		this.checkSpecialKo2Rule = new System.Windows.Forms.CheckBox();
		this.comboSpecialKo1Rule = new System.Windows.Forms.ComboBox();
		this.checkSpecialKo1Rule = new System.Windows.Forms.CheckBox();
		this.numericKeepPointsPercentage = new System.Windows.Forms.NumericUpDown();
		this.checkKeepPointsPercentage = new System.Windows.Forms.CheckBox();
		this.numericStageRef = new System.Windows.Forms.NumericUpDown();
		this.checkClausuraSchedule = new System.Windows.Forms.CheckBox();
		this.groupStadiums = new System.Windows.Forms.GroupBox();
		this.comboStadium12 = new System.Windows.Forms.ComboBox();
		this.comboStadium11 = new System.Windows.Forms.ComboBox();
		this.comboStadium10 = new System.Windows.Forms.ComboBox();
		this.comboStadium9 = new System.Windows.Forms.ComboBox();
		this.comboStadium8 = new System.Windows.Forms.ComboBox();
		this.comboStadium7 = new System.Windows.Forms.ComboBox();
		this.comboStadium6 = new System.Windows.Forms.ComboBox();
		this.comboStadium5 = new System.Windows.Forms.ComboBox();
		this.comboStadium4 = new System.Windows.Forms.ComboBox();
		this.comboStadium3 = new System.Windows.Forms.ComboBox();
		this.comboStadium2 = new System.Windows.Forms.ComboBox();
		this.comboStadium1 = new System.Windows.Forms.ComboBox();
		this.checkMaxteamsgroup = new System.Windows.Forms.CheckBox();
		this.checkMatchReplay = new System.Windows.Forms.CheckBox();
		this.numericMoneyDrop = new System.Windows.Forms.NumericUpDown();
		this.checkMaxteamsassoc = new System.Windows.Forms.CheckBox();
		this.label10 = new System.Windows.Forms.Label();
		this.numericPrizeMoney = new System.Windows.Forms.NumericUpDown();
		this.label9 = new System.Windows.Forms.Label();
		this.comboMatchSituation = new System.Windows.Forms.ComboBox();
		this.label8 = new System.Windows.Forms.Label();
		this.groupSetupStage = new System.Windows.Forms.GroupBox();
		this.checkRandomDraw = new System.Windows.Forms.CheckBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.comboSpecialTeam4 = new System.Windows.Forms.ComboBox();
		this.comboSpecialTeam3 = new System.Windows.Forms.ComboBox();
		this.comboSpecialTeam2 = new System.Windows.Forms.ComboBox();
		this.comboSpecialTeam1 = new System.Windows.Forms.ComboBox();
		this.checkCalccompavgs = new System.Windows.Forms.CheckBox();
		this.comboStageStandingRules = new System.Windows.Forms.ComboBox();
		this.checkStageStandingsRules = new System.Windows.Forms.CheckBox();
		this.numericStandingsRank = new System.Windows.Forms.NumericUpDown();
		this.checkStandingsRank = new System.Windows.Forms.CheckBox();
		this.comboStageType = new System.Windows.Forms.ComboBox();
		this.label7 = new System.Windows.Forms.Label();
		this.numericStandingKeep = new System.Windows.Forms.NumericUpDown();
		this.checkStandingKeep = new System.Windows.Forms.CheckBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.toolCompetitionTree = new System.Windows.Forms.ToolStrip();
		this.buttonAddTrophy = new System.Windows.Forms.ToolStripButton();
		this.buttonDeleteTrophy = new System.Windows.Forms.ToolStripButton();
		this.buttonAddStage = new System.Windows.Forms.ToolStripButton();
		this.buttonDeleteStage = new System.Windows.Forms.ToolStripButton();
		this.buttonAddGroup = new System.Windows.Forms.ToolStripButton();
		this.buttonDeleteGroup = new System.Windows.Forms.ToolStripButton();
		this.buttonAddNatiom = new System.Windows.Forms.ToolStripButton();
		this.buttonDeleteNation = new System.Windows.Forms.ToolStripButton();
		this.buttonPasteTrophy = new System.Windows.Forms.ToolStripButton();
		this.buttonCopyTrophy = new System.Windows.Forms.ToolStripButton();
		this.comboTargetLeague = new System.Windows.Forms.ToolStripComboBox();
		this.buttonCreatePatch = new System.Windows.Forms.ToolStripButton();
		this.buttonLoadPatch = new System.Windows.Forms.ToolStripButton();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.tabCompetitions = new System.Windows.Forms.TabControl();
		this.pageWorld = new System.Windows.Forms.TabPage();
		this.numericStartYear = new System.Windows.Forms.NumericUpDown();
		this.label13 = new System.Windows.Forms.Label();
		this.pageConfederation = new System.Windows.Forms.TabPage();
		this.pageNation = new System.Windows.Forms.TabPage();
		this.pageTrophy = new System.Windows.Forms.TabPage();
		this.tabTrophy = new System.Windows.Forms.TabControl();
		this.tabPageTrophyStructure = new System.Windows.Forms.TabPage();
		this.tabPageRankingTable = new System.Windows.Forms.TabPage();
		this.groupInitTeams = new System.Windows.Forms.GroupBox();
		this.label70 = new System.Windows.Forms.Label();
		this.numericUpdateTableEntries = new System.Windows.Forms.NumericUpDown();
		this.panelAllInitTeams = new System.Windows.Forms.Panel();
		this.panelInitTeam1 = new System.Windows.Forms.Panel();
		this.labelUpdateTable1 = new System.Windows.Forms.Label();
		this.comboInitTeam1 = new System.Windows.Forms.ComboBox();
		this.label42 = new System.Windows.Forms.Label();
		this.panelInitTeam2 = new System.Windows.Forms.Panel();
		this.labelUpdateTable2 = new System.Windows.Forms.Label();
		this.comboInitTeam2 = new System.Windows.Forms.ComboBox();
		this.label43 = new System.Windows.Forms.Label();
		this.panelInitTeam24 = new System.Windows.Forms.Panel();
		this.labelUpdateTable24 = new System.Windows.Forms.Label();
		this.comboInitTeam24 = new System.Windows.Forms.ComboBox();
		this.label65 = new System.Windows.Forms.Label();
		this.panelInitTeam3 = new System.Windows.Forms.Panel();
		this.labelUpdateTable3 = new System.Windows.Forms.Label();
		this.comboInitTeam3 = new System.Windows.Forms.ComboBox();
		this.label44 = new System.Windows.Forms.Label();
		this.panelInitTeam23 = new System.Windows.Forms.Panel();
		this.labelUpdateTable23 = new System.Windows.Forms.Label();
		this.comboInitTeam23 = new System.Windows.Forms.ComboBox();
		this.label64 = new System.Windows.Forms.Label();
		this.panelInitTeam4 = new System.Windows.Forms.Panel();
		this.labelUpdateTable4 = new System.Windows.Forms.Label();
		this.comboInitTeam4 = new System.Windows.Forms.ComboBox();
		this.label45 = new System.Windows.Forms.Label();
		this.panelInitTeam22 = new System.Windows.Forms.Panel();
		this.labelUpdateTable22 = new System.Windows.Forms.Label();
		this.comboInitTeam22 = new System.Windows.Forms.ComboBox();
		this.label63 = new System.Windows.Forms.Label();
		this.panelInitTeam5 = new System.Windows.Forms.Panel();
		this.labelUpdateTable5 = new System.Windows.Forms.Label();
		this.comboInitTeam5 = new System.Windows.Forms.ComboBox();
		this.label46 = new System.Windows.Forms.Label();
		this.panelInitTeam21 = new System.Windows.Forms.Panel();
		this.labelUpdateTable21 = new System.Windows.Forms.Label();
		this.comboInitTeam21 = new System.Windows.Forms.ComboBox();
		this.label62 = new System.Windows.Forms.Label();
		this.panelInitTeam6 = new System.Windows.Forms.Panel();
		this.labelUpdateTable6 = new System.Windows.Forms.Label();
		this.comboInitTeam6 = new System.Windows.Forms.ComboBox();
		this.label47 = new System.Windows.Forms.Label();
		this.panelInitTeam20 = new System.Windows.Forms.Panel();
		this.labelUpdateTable20 = new System.Windows.Forms.Label();
		this.comboInitTeam20 = new System.Windows.Forms.ComboBox();
		this.label61 = new System.Windows.Forms.Label();
		this.panelInitTeam7 = new System.Windows.Forms.Panel();
		this.labelUpdateTable7 = new System.Windows.Forms.Label();
		this.comboInitTeam7 = new System.Windows.Forms.ComboBox();
		this.label48 = new System.Windows.Forms.Label();
		this.panelInitTeam19 = new System.Windows.Forms.Panel();
		this.labelUpdateTable19 = new System.Windows.Forms.Label();
		this.comboInitTeam19 = new System.Windows.Forms.ComboBox();
		this.label60 = new System.Windows.Forms.Label();
		this.panelInitTeam8 = new System.Windows.Forms.Panel();
		this.labelUpdateTable8 = new System.Windows.Forms.Label();
		this.comboInitTeam8 = new System.Windows.Forms.ComboBox();
		this.label49 = new System.Windows.Forms.Label();
		this.panelInitTeam18 = new System.Windows.Forms.Panel();
		this.labelUpdateTable18 = new System.Windows.Forms.Label();
		this.comboInitTeam18 = new System.Windows.Forms.ComboBox();
		this.label59 = new System.Windows.Forms.Label();
		this.panelInitTeam9 = new System.Windows.Forms.Panel();
		this.labelUpdateTable9 = new System.Windows.Forms.Label();
		this.comboInitTeam9 = new System.Windows.Forms.ComboBox();
		this.label50 = new System.Windows.Forms.Label();
		this.panelInitTeam17 = new System.Windows.Forms.Panel();
		this.labelUpdateTable17 = new System.Windows.Forms.Label();
		this.comboInitTeam17 = new System.Windows.Forms.ComboBox();
		this.label58 = new System.Windows.Forms.Label();
		this.panelInitTeam10 = new System.Windows.Forms.Panel();
		this.labelUpdateTable10 = new System.Windows.Forms.Label();
		this.comboInitTeam10 = new System.Windows.Forms.ComboBox();
		this.label51 = new System.Windows.Forms.Label();
		this.panelInitTeam16 = new System.Windows.Forms.Panel();
		this.labelUpdateTable16 = new System.Windows.Forms.Label();
		this.comboInitTeam16 = new System.Windows.Forms.ComboBox();
		this.label57 = new System.Windows.Forms.Label();
		this.panelInitTeam11 = new System.Windows.Forms.Panel();
		this.labelUpdateTable11 = new System.Windows.Forms.Label();
		this.comboInitTeam11 = new System.Windows.Forms.ComboBox();
		this.label52 = new System.Windows.Forms.Label();
		this.panelInitTeam15 = new System.Windows.Forms.Panel();
		this.labelUpdateTable15 = new System.Windows.Forms.Label();
		this.comboInitTeam15 = new System.Windows.Forms.ComboBox();
		this.label56 = new System.Windows.Forms.Label();
		this.panelInitTeam12 = new System.Windows.Forms.Panel();
		this.labelUpdateTable12 = new System.Windows.Forms.Label();
		this.comboInitTeam12 = new System.Windows.Forms.ComboBox();
		this.label53 = new System.Windows.Forms.Label();
		this.panelInitTeam14 = new System.Windows.Forms.Panel();
		this.labelUpdateTable14 = new System.Windows.Forms.Label();
		this.comboInitTeam14 = new System.Windows.Forms.ComboBox();
		this.label55 = new System.Windows.Forms.Label();
		this.panelInitTeam13 = new System.Windows.Forms.Panel();
		this.labelUpdateTable13 = new System.Windows.Forms.Label();
		this.comboInitTeam13 = new System.Windows.Forms.ComboBox();
		this.label54 = new System.Windows.Forms.Label();
		this.panelInitTeam25 = new System.Windows.Forms.Panel();
		this.labelUpdateTable25 = new System.Windows.Forms.Label();
		this.comboInitTeam25 = new System.Windows.Forms.ComboBox();
		this.label32 = new System.Windows.Forms.Label();
		this.panelInitTeam26 = new System.Windows.Forms.Panel();
		this.labelUpdateTable26 = new System.Windows.Forms.Label();
		this.comboInitTeam26 = new System.Windows.Forms.ComboBox();
		this.label33 = new System.Windows.Forms.Label();
		this.panelInitTeam27 = new System.Windows.Forms.Panel();
		this.labelUpdateTable27 = new System.Windows.Forms.Label();
		this.comboInitTeam27 = new System.Windows.Forms.ComboBox();
		this.label127 = new System.Windows.Forms.Label();
		this.panelInitTeam28 = new System.Windows.Forms.Panel();
		this.labelUpdateTable28 = new System.Windows.Forms.Label();
		this.comboInitTeam28 = new System.Windows.Forms.ComboBox();
		this.label128 = new System.Windows.Forms.Label();
		this.panelInitTeam29 = new System.Windows.Forms.Panel();
		this.labelUpdateTable29 = new System.Windows.Forms.Label();
		this.comboInitTeam29 = new System.Windows.Forms.ComboBox();
		this.label129 = new System.Windows.Forms.Label();
		this.panelInitTeam30 = new System.Windows.Forms.Panel();
		this.labelUpdateTable30 = new System.Windows.Forms.Label();
		this.comboInitTeam30 = new System.Windows.Forms.ComboBox();
		this.label130 = new System.Windows.Forms.Label();
		this.panelInitTeam31 = new System.Windows.Forms.Panel();
		this.labelUpdateTable31 = new System.Windows.Forms.Label();
		this.comboInitTeam31 = new System.Windows.Forms.ComboBox();
		this.label131 = new System.Windows.Forms.Label();
		this.panelInitTeam32 = new System.Windows.Forms.Panel();
		this.labelUpdateTable32 = new System.Windows.Forms.Label();
		this.comboInitTeam32 = new System.Windows.Forms.ComboBox();
		this.label132 = new System.Windows.Forms.Label();
		this.panelInitTeam33 = new System.Windows.Forms.Panel();
		this.labelUpdateTable33 = new System.Windows.Forms.Label();
		this.comboInitTeam33 = new System.Windows.Forms.ComboBox();
		this.label133 = new System.Windows.Forms.Label();
		this.panelInitTeam34 = new System.Windows.Forms.Panel();
		this.labelUpdateTable34 = new System.Windows.Forms.Label();
		this.comboInitTeam34 = new System.Windows.Forms.ComboBox();
		this.label134 = new System.Windows.Forms.Label();
		this.panelInitTeam35 = new System.Windows.Forms.Panel();
		this.labelUpdateTable35 = new System.Windows.Forms.Label();
		this.comboInitTeam35 = new System.Windows.Forms.ComboBox();
		this.label135 = new System.Windows.Forms.Label();
		this.panelInitTeam36 = new System.Windows.Forms.Panel();
		this.labelUpdateTable36 = new System.Windows.Forms.Label();
		this.comboInitTeam36 = new System.Windows.Forms.ComboBox();
		this.label136 = new System.Windows.Forms.Label();
		this.panelInitTeam37 = new System.Windows.Forms.Panel();
		this.labelUpdateTable37 = new System.Windows.Forms.Label();
		this.comboInitTeam37 = new System.Windows.Forms.ComboBox();
		this.label137 = new System.Windows.Forms.Label();
		this.panelInitTeam38 = new System.Windows.Forms.Panel();
		this.labelUpdateTable38 = new System.Windows.Forms.Label();
		this.comboInitTeam38 = new System.Windows.Forms.ComboBox();
		this.label138 = new System.Windows.Forms.Label();
		this.panelInitTeam39 = new System.Windows.Forms.Panel();
		this.labelUpdateTable39 = new System.Windows.Forms.Label();
		this.comboInitTeam39 = new System.Windows.Forms.ComboBox();
		this.label139 = new System.Windows.Forms.Label();
		this.panelInitTeam40 = new System.Windows.Forms.Panel();
		this.labelUpdateTable40 = new System.Windows.Forms.Label();
		this.comboInitTeam40 = new System.Windows.Forms.ComboBox();
		this.label140 = new System.Windows.Forms.Label();
		this.panelInitTeam41 = new System.Windows.Forms.Panel();
		this.labelUpdateTable41 = new System.Windows.Forms.Label();
		this.comboInitTeam41 = new System.Windows.Forms.ComboBox();
		this.label141 = new System.Windows.Forms.Label();
		this.panelInitTeam42 = new System.Windows.Forms.Panel();
		this.labelUpdateTable42 = new System.Windows.Forms.Label();
		this.comboInitTeam42 = new System.Windows.Forms.ComboBox();
		this.label142 = new System.Windows.Forms.Label();
		this.panelInitTeam43 = new System.Windows.Forms.Panel();
		this.labelUpdateTable43 = new System.Windows.Forms.Label();
		this.comboInitTeam43 = new System.Windows.Forms.ComboBox();
		this.label143 = new System.Windows.Forms.Label();
		this.panelInitTeam44 = new System.Windows.Forms.Panel();
		this.labelUpdateTable44 = new System.Windows.Forms.Label();
		this.comboInitTeam44 = new System.Windows.Forms.ComboBox();
		this.label144 = new System.Windows.Forms.Label();
		this.panelInitTeam45 = new System.Windows.Forms.Panel();
		this.labelUpdateTable45 = new System.Windows.Forms.Label();
		this.comboInitTeam45 = new System.Windows.Forms.ComboBox();
		this.label145 = new System.Windows.Forms.Label();
		this.panelInitTeam46 = new System.Windows.Forms.Panel();
		this.labelUpdateTable46 = new System.Windows.Forms.Label();
		this.comboInitTeam46 = new System.Windows.Forms.ComboBox();
		this.label146 = new System.Windows.Forms.Label();
		this.panelInitTeam47 = new System.Windows.Forms.Panel();
		this.labelUpdateTable47 = new System.Windows.Forms.Label();
		this.comboInitTeam47 = new System.Windows.Forms.ComboBox();
		this.label147 = new System.Windows.Forms.Label();
		this.panelInitTeam48 = new System.Windows.Forms.Panel();
		this.labelUpdateTable48 = new System.Windows.Forms.Label();
		this.comboInitTeam48 = new System.Windows.Forms.ComboBox();
		this.label148 = new System.Windows.Forms.Label();
		this.tabPageTrophyGraphics = new System.Windows.Forms.TabPage();
		this.groupGraphics = new System.Windows.Forms.GroupBox();
		this.buttonReplicateTropy = new System.Windows.Forms.Button();
		this.viewer2DTrophy = new FifaControls.Viewer2D();
		this.buttonReplicateTrophy128 = new System.Windows.Forms.Button();
		this.viewer2DTrophy128 = new FifaControls.Viewer2D();
		this.multiViewer2DTextures = new FifaControls.MultiViewer2D();
		this.group3D = new System.Windows.Forms.GroupBox();
		this.toolNear3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImport3DModel = new System.Windows.Forms.ToolStripButton();
		this.buttonExport3DModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonRemove3DModel = new System.Windows.Forms.ToolStripButton();
		this.viewer2DTrophy256 = new FifaControls.Viewer2D();
		this.tabPageTrophyPitchGraphics = new System.Windows.Forms.TabPage();
		this.viewer2DPitchDressing = new FifaControls.Viewer2D();
		this.tabPageTrophyRevMod = new System.Windows.Forms.TabPage();
		this.groupTeamBallRevMod = new System.Windows.Forms.GroupBox();
		this.toolTeamBall3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DBall = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImport3DModelTournamentBall = new System.Windows.Forms.ToolStripButton();
		this.buttonExport3DModelTournamentBall = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonRemove3DModelTournamentBall = new System.Windows.Forms.ToolStripButton();
		this.multiViewer2DTournamentBallTextures = new FifaControls.MultiViewer2D();
		this.groupTeamAdboardsRevMod = new System.Windows.Forms.GroupBox();
		this.viewer2DTournamentAdboard = new FifaControls.Viewer2D();
		this.tabPageWipe3D = new System.Windows.Forms.TabPage();
		this.multiViewerWipe = new FifaControls.MultiViewer2D();
		this.pageStage = new System.Windows.Forms.TabPage();
		this.pageGroup = new System.Windows.Forms.TabPage();
		this.groupGroup = new System.Windows.Forms.GroupBox();
		this.groupRules = new System.Windows.Forms.GroupBox();
		this.panelQualificationRules = new System.Windows.Forms.Panel();
		this.toolRules = new System.Windows.Forms.ToolStrip();
		this.buttonAddRule = new System.Windows.Forms.ToolStripButton();
		this.buttonRemoveRule = new System.Windows.Forms.ToolStripButton();
		this.panelAdvancement = new System.Windows.Forms.Panel();
		this.groupPlayGroup = new System.Windows.Forms.GroupBox();
		this.numericNumGames = new System.Windows.Forms.NumericUpDown();
		this.label14 = new System.Windows.Forms.Label();
		this.groupGroupScheduke = new System.Windows.Forms.GroupBox();
		this.treeGroupSchedule = new System.Windows.Forms.TreeView();
		this.panelGroupScheduleDetails = new System.Windows.Forms.Panel();
		this.groupGroupScheduleDetails = new System.Windows.Forms.GroupBox();
		this.dateGroupPicker = new System.Windows.Forms.DateTimePicker();
		this.label38 = new System.Windows.Forms.Label();
		this.numericGroupMinGames = new System.Windows.Forms.NumericUpDown();
		this.label39 = new System.Windows.Forms.Label();
		this.numericGroupMaxGames = new System.Windows.Forms.NumericUpDown();
		this.label40 = new System.Windows.Forms.Label();
		this.comboGroupTime = new System.Windows.Forms.ComboBox();
		this.label41 = new System.Windows.Forms.Label();
		this.toolGroupSchedule = new System.Windows.Forms.ToolStrip();
		this.buttonCopyGroupCalendar = new System.Windows.Forms.ToolStripButton();
		this.buttonPasteGroupCalendar = new System.Windows.Forms.ToolStripButton();
		this.buttonCleanGroupCalendar = new System.Windows.Forms.ToolStripButton();
		this.buttonNewGroupLeg = new System.Windows.Forms.ToolStripButton();
		this.buttonRemoveGroupLeg = new System.Windows.Forms.ToolStripButton();
		this.buttonGroupAddTime = new System.Windows.Forms.ToolStripButton();
		this.buttonGroupRemoveTime = new System.Windows.Forms.ToolStripButton();
		this.buttongroupSortLegs = new System.Windows.Forms.ToolStripButton();
		this.groupSlots = new System.Windows.Forms.GroupBox();
		this.numericPossiblePromotionMax = new System.Windows.Forms.NumericUpDown();
		this.checkInfoPossiblePromotion = new System.Windows.Forms.CheckBox();
		this.numericPossiblePromotionMin = new System.Windows.Forms.NumericUpDown();
		this.numericPromotionMax = new System.Windows.Forms.NumericUpDown();
		this.numericPromotionMin = new System.Windows.Forms.NumericUpDown();
		this.numericRelegationMax = new System.Windows.Forms.NumericUpDown();
		this.numericRelegationMin = new System.Windows.Forms.NumericUpDown();
		this.numericPossibleRelegationMax = new System.Windows.Forms.NumericUpDown();
		this.numericPossibleRelegationMin = new System.Windows.Forms.NumericUpDown();
		this.label15 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.checkInfoPromotion = new System.Windows.Forms.CheckBox();
		this.checkInfoRelegation = new System.Windows.Forms.CheckBox();
		this.checkInfoPossibleRelegation = new System.Windows.Forms.CheckBox();
		this.checkInfoChamp = new System.Windows.Forms.CheckBox();
		this.groupInfoColors = new System.Windows.Forms.GroupBox();
		this.numericColorPossiblePromotionMax = new System.Windows.Forms.NumericUpDown();
		this.checkInfoColorPossiblePromotion = new System.Windows.Forms.CheckBox();
		this.numericColorAdvanceMax = new System.Windows.Forms.NumericUpDown();
		this.numericColorPossiblePromotionMin = new System.Windows.Forms.NumericUpDown();
		this.numericColorAdvanceMin = new System.Windows.Forms.NumericUpDown();
		this.numericColorPromotionMax = new System.Windows.Forms.NumericUpDown();
		this.numericColorPromotionMin = new System.Windows.Forms.NumericUpDown();
		this.numericColorRelegationMax = new System.Windows.Forms.NumericUpDown();
		this.numericColorRelegationMin = new System.Windows.Forms.NumericUpDown();
		this.numericColorPossibleRelegationMax = new System.Windows.Forms.NumericUpDown();
		this.numericColorPossibleRelegationMin = new System.Windows.Forms.NumericUpDown();
		this.numericColorEuropaMax = new System.Windows.Forms.NumericUpDown();
		this.numericColorEuropaMin = new System.Windows.Forms.NumericUpDown();
		this.numericColorChampionsMax = new System.Windows.Forms.NumericUpDown();
		this.numericColorChampionsMin = new System.Windows.Forms.NumericUpDown();
		this.label12 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.checkInfoColorAdvance = new System.Windows.Forms.CheckBox();
		this.checkInfoColorPromotion = new System.Windows.Forms.CheckBox();
		this.checkInfoColorRelegation = new System.Windows.Forms.CheckBox();
		this.checkInfoColorPossibleRelegation = new System.Windows.Forms.CheckBox();
		this.checkInfoColorEuropa = new System.Windows.Forms.CheckBox();
		this.checkInfoColorChampions = new System.Windows.Forms.CheckBox();
		this.checkInfoColorChamp = new System.Windows.Forms.CheckBox();
		this.label4 = new System.Windows.Forms.Label();
		this.numericNTeams = new System.Windows.Forms.NumericUpDown();
		this.panelCompObj = new System.Windows.Forms.Panel();
		this.textLanguageName = new System.Windows.Forms.TextBox();
		this.label66 = new System.Windows.Forms.Label();
		this.textUniqueId = new System.Windows.Forms.TextBox();
		this.comboLanguageKey = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.textLanguageKey = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.textFourCharName = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.groupConfederation.SuspendLayout();
		this.groupNation.SuspendLayout();
		this.groupWeather.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown97).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown98).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown99).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown100).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown101).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown102).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown103).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown104).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown105).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown106).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown107).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown108).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown85).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown86).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown87).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown88).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown89).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown90).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown91).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown92).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown93).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown94).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown95).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown96).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown73).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown74).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown75).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown76).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown77).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown78).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown79).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown80).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown81).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown82).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown83).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown84).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown61).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown62).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown63).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown64).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown65).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown66).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown67).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown68).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown69).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown70).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown71).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown72).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown49).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown50).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown51).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown52).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown53).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown54).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown55).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown56).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown57).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown58).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown59).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown60).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown37).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown38).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown39).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown40).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown41).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown42).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown43).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown44).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown45).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown46).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown47).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown48).BeginInit();
		this.toolWeather.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown34).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown35).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown36).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown31).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown32).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown33).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown28).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown29).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown30).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown25).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown26).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown27).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown22).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown23).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown24).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown19).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown20).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown21).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown16).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown17).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown18).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown13).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown14).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown15).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown10).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown11).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown12).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown7).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown8).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown9).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericNationYellowsStored).BeginInit();
		this.groupTrophy.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericAdvanceFrom).BeginInit();
		this.groupInternationalschedule.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericInternationalPeriodicity).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericInternationalFirstYear).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBall).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBall).BeginInit();
		this.groupBenchPlayers.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericImportance).BeginInit();
		this.groupPromotionRelegation.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericAssetId).BeginInit();
		this.groupSchedule.SuspendLayout();
		this.groupStage.SuspendLayout();
		this.groupPlayStage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericKeepPointsStageRef).BeginInit();
		this.groupLeaguetasks.SuspendLayout();
		this.groupStageSchedules.SuspendLayout();
		this.panelStageScheduleDetails.SuspendLayout();
		this.groupStageScheduleDetails.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericStageMinGames).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericStageMaxGames).BeginInit();
		this.toolStageSchedule.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericRegularSeason).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericKeepPointsPercentage).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericStageRef).BeginInit();
		this.groupStadiums.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericMoneyDrop).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPrizeMoney).BeginInit();
		this.groupSetupStage.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericStandingsRank).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericStandingKeep).BeginInit();
		this.toolCompetitionTree.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		this.tabCompetitions.SuspendLayout();
		this.pageWorld.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericStartYear).BeginInit();
		this.pageConfederation.SuspendLayout();
		this.pageNation.SuspendLayout();
		this.pageTrophy.SuspendLayout();
		this.tabTrophy.SuspendLayout();
		this.tabPageTrophyStructure.SuspendLayout();
		this.tabPageRankingTable.SuspendLayout();
		this.groupInitTeams.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpdateTableEntries).BeginInit();
		this.panelAllInitTeams.SuspendLayout();
		this.panelInitTeam1.SuspendLayout();
		this.panelInitTeam2.SuspendLayout();
		this.panelInitTeam24.SuspendLayout();
		this.panelInitTeam3.SuspendLayout();
		this.panelInitTeam23.SuspendLayout();
		this.panelInitTeam4.SuspendLayout();
		this.panelInitTeam22.SuspendLayout();
		this.panelInitTeam5.SuspendLayout();
		this.panelInitTeam21.SuspendLayout();
		this.panelInitTeam6.SuspendLayout();
		this.panelInitTeam20.SuspendLayout();
		this.panelInitTeam7.SuspendLayout();
		this.panelInitTeam19.SuspendLayout();
		this.panelInitTeam8.SuspendLayout();
		this.panelInitTeam18.SuspendLayout();
		this.panelInitTeam9.SuspendLayout();
		this.panelInitTeam17.SuspendLayout();
		this.panelInitTeam10.SuspendLayout();
		this.panelInitTeam16.SuspendLayout();
		this.panelInitTeam11.SuspendLayout();
		this.panelInitTeam15.SuspendLayout();
		this.panelInitTeam12.SuspendLayout();
		this.panelInitTeam14.SuspendLayout();
		this.panelInitTeam13.SuspendLayout();
		this.panelInitTeam25.SuspendLayout();
		this.panelInitTeam26.SuspendLayout();
		this.panelInitTeam27.SuspendLayout();
		this.panelInitTeam28.SuspendLayout();
		this.panelInitTeam29.SuspendLayout();
		this.panelInitTeam30.SuspendLayout();
		this.panelInitTeam31.SuspendLayout();
		this.panelInitTeam32.SuspendLayout();
		this.panelInitTeam33.SuspendLayout();
		this.panelInitTeam34.SuspendLayout();
		this.panelInitTeam35.SuspendLayout();
		this.panelInitTeam36.SuspendLayout();
		this.panelInitTeam37.SuspendLayout();
		this.panelInitTeam38.SuspendLayout();
		this.panelInitTeam39.SuspendLayout();
		this.panelInitTeam40.SuspendLayout();
		this.panelInitTeam41.SuspendLayout();
		this.panelInitTeam42.SuspendLayout();
		this.panelInitTeam43.SuspendLayout();
		this.panelInitTeam44.SuspendLayout();
		this.panelInitTeam45.SuspendLayout();
		this.panelInitTeam46.SuspendLayout();
		this.panelInitTeam47.SuspendLayout();
		this.panelInitTeam48.SuspendLayout();
		this.tabPageTrophyGraphics.SuspendLayout();
		this.groupGraphics.SuspendLayout();
		this.group3D.SuspendLayout();
		this.toolNear3D.SuspendLayout();
		this.tabPageTrophyPitchGraphics.SuspendLayout();
		this.tabPageTrophyRevMod.SuspendLayout();
		this.groupTeamBallRevMod.SuspendLayout();
		this.toolTeamBall3D.SuspendLayout();
		this.groupTeamAdboardsRevMod.SuspendLayout();
		this.tabPageWipe3D.SuspendLayout();
		this.pageStage.SuspendLayout();
		this.pageGroup.SuspendLayout();
		this.groupGroup.SuspendLayout();
		this.groupRules.SuspendLayout();
		this.panelQualificationRules.SuspendLayout();
		this.toolRules.SuspendLayout();
		this.groupPlayGroup.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNumGames).BeginInit();
		this.groupGroupScheduke.SuspendLayout();
		this.panelGroupScheduleDetails.SuspendLayout();
		this.groupGroupScheduleDetails.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericGroupMinGames).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericGroupMaxGames).BeginInit();
		this.toolGroupSchedule.SuspendLayout();
		this.groupSlots.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericPossiblePromotionMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPossiblePromotionMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPromotionMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPromotionMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericRelegationMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericRelegationMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPossibleRelegationMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericPossibleRelegationMin).BeginInit();
		this.groupInfoColors.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossiblePromotionMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorAdvanceMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossiblePromotionMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorAdvanceMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPromotionMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPromotionMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorRelegationMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorRelegationMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossibleRelegationMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossibleRelegationMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorEuropaMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorEuropaMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorChampionsMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorChampionsMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericNTeams).BeginInit();
		this.panelCompObj.SuspendLayout();
		base.SuspendLayout();
		this.treeWorld.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeWorld.FullRowSelect = true;
		this.treeWorld.HideSelection = false;
		this.treeWorld.Location = new System.Drawing.Point(0, 52);
		this.treeWorld.Name = "treeWorld";
		this.treeWorld.Size = new System.Drawing.Size(332, 728);
		this.treeWorld.TabIndex = 6;
		this.treeWorld.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeWorld_AfterSelect);
		this.groupConfederation.Controls.Add(this.comboConfederationStartingMonth);
		this.groupConfederation.Controls.Add(this.labelConfStartMonth);
		this.groupConfederation.Location = new System.Drawing.Point(3, 3);
		this.groupConfederation.Name = "groupConfederation";
		this.groupConfederation.Size = new System.Drawing.Size(291, 71);
		this.groupConfederation.TabIndex = 7;
		this.groupConfederation.TabStop = false;
		this.groupConfederation.Text = "Confederation";
		this.groupConfederation.Visible = false;
		this.comboConfederationStartingMonth.FormattingEnabled = true;
		this.comboConfederationStartingMonth.Items.AddRange(new object[12]
		{
			"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT",
			"NOV", "DEC"
		});
		this.comboConfederationStartingMonth.Location = new System.Drawing.Point(162, 21);
		this.comboConfederationStartingMonth.Name = "comboConfederationStartingMonth";
		this.comboConfederationStartingMonth.Size = new System.Drawing.Size(90, 21);
		this.comboConfederationStartingMonth.TabIndex = 1;
		this.comboConfederationStartingMonth.SelectedIndexChanged += new System.EventHandler(comboConfederationStartingMonth_SelectedIndexChanged);
		this.labelConfStartMonth.AutoSize = true;
		this.labelConfStartMonth.Location = new System.Drawing.Point(6, 27);
		this.labelConfStartMonth.Name = "labelConfStartMonth";
		this.labelConfStartMonth.Size = new System.Drawing.Size(101, 13);
		this.labelConfStartMonth.TabIndex = 0;
		this.labelConfStartMonth.Text = "Season Start Month";
		this.groupNation.Controls.Add(this.groupWeather);
		this.groupNation.Controls.Add(this.label6);
		this.groupNation.Controls.Add(this.label5);
		this.groupNation.Controls.Add(this.labelDatabaseCountry);
		this.groupNation.Controls.Add(this.comboCountry);
		this.groupNation.Controls.Add(this.comboNationStandingsRules);
		this.groupNation.Controls.Add(this.checkNationStandingsRules);
		this.groupNation.Controls.Add(this.numericNationYellowsStored);
		this.groupNation.Controls.Add(this.comboNationStartMonth);
		this.groupNation.Location = new System.Drawing.Point(3, 3);
		this.groupNation.Name = "groupNation";
		this.groupNation.Size = new System.Drawing.Size(779, 650);
		this.groupNation.TabIndex = 8;
		this.groupNation.TabStop = false;
		this.groupNation.Text = "Nation";
		this.groupNation.Visible = false;
		this.groupWeather.Controls.Add(this.label30);
		this.groupWeather.Controls.Add(this.label29);
		this.groupWeather.Controls.Add(this.numericUpDown97);
		this.groupWeather.Controls.Add(this.numericUpDown98);
		this.groupWeather.Controls.Add(this.numericUpDown99);
		this.groupWeather.Controls.Add(this.numericUpDown100);
		this.groupWeather.Controls.Add(this.numericUpDown101);
		this.groupWeather.Controls.Add(this.numericUpDown102);
		this.groupWeather.Controls.Add(this.numericUpDown103);
		this.groupWeather.Controls.Add(this.numericUpDown104);
		this.groupWeather.Controls.Add(this.numericUpDown105);
		this.groupWeather.Controls.Add(this.numericUpDown106);
		this.groupWeather.Controls.Add(this.numericUpDown107);
		this.groupWeather.Controls.Add(this.numericUpDown108);
		this.groupWeather.Controls.Add(this.numericUpDown85);
		this.groupWeather.Controls.Add(this.numericUpDown86);
		this.groupWeather.Controls.Add(this.numericUpDown87);
		this.groupWeather.Controls.Add(this.numericUpDown88);
		this.groupWeather.Controls.Add(this.numericUpDown89);
		this.groupWeather.Controls.Add(this.numericUpDown90);
		this.groupWeather.Controls.Add(this.numericUpDown91);
		this.groupWeather.Controls.Add(this.numericUpDown92);
		this.groupWeather.Controls.Add(this.numericUpDown93);
		this.groupWeather.Controls.Add(this.numericUpDown94);
		this.groupWeather.Controls.Add(this.numericUpDown95);
		this.groupWeather.Controls.Add(this.numericUpDown96);
		this.groupWeather.Controls.Add(this.numericUpDown73);
		this.groupWeather.Controls.Add(this.numericUpDown74);
		this.groupWeather.Controls.Add(this.numericUpDown75);
		this.groupWeather.Controls.Add(this.numericUpDown76);
		this.groupWeather.Controls.Add(this.numericUpDown77);
		this.groupWeather.Controls.Add(this.numericUpDown78);
		this.groupWeather.Controls.Add(this.numericUpDown79);
		this.groupWeather.Controls.Add(this.numericUpDown80);
		this.groupWeather.Controls.Add(this.numericUpDown81);
		this.groupWeather.Controls.Add(this.numericUpDown82);
		this.groupWeather.Controls.Add(this.numericUpDown83);
		this.groupWeather.Controls.Add(this.numericUpDown84);
		this.groupWeather.Controls.Add(this.numericUpDown61);
		this.groupWeather.Controls.Add(this.numericUpDown62);
		this.groupWeather.Controls.Add(this.numericUpDown63);
		this.groupWeather.Controls.Add(this.numericUpDown64);
		this.groupWeather.Controls.Add(this.numericUpDown65);
		this.groupWeather.Controls.Add(this.numericUpDown66);
		this.groupWeather.Controls.Add(this.numericUpDown67);
		this.groupWeather.Controls.Add(this.numericUpDown68);
		this.groupWeather.Controls.Add(this.numericUpDown69);
		this.groupWeather.Controls.Add(this.numericUpDown70);
		this.groupWeather.Controls.Add(this.numericUpDown71);
		this.groupWeather.Controls.Add(this.numericUpDown72);
		this.groupWeather.Controls.Add(this.numericUpDown49);
		this.groupWeather.Controls.Add(this.numericUpDown50);
		this.groupWeather.Controls.Add(this.numericUpDown51);
		this.groupWeather.Controls.Add(this.numericUpDown52);
		this.groupWeather.Controls.Add(this.numericUpDown53);
		this.groupWeather.Controls.Add(this.numericUpDown54);
		this.groupWeather.Controls.Add(this.numericUpDown55);
		this.groupWeather.Controls.Add(this.numericUpDown56);
		this.groupWeather.Controls.Add(this.numericUpDown57);
		this.groupWeather.Controls.Add(this.numericUpDown58);
		this.groupWeather.Controls.Add(this.numericUpDown59);
		this.groupWeather.Controls.Add(this.numericUpDown60);
		this.groupWeather.Controls.Add(this.numericUpDown37);
		this.groupWeather.Controls.Add(this.numericUpDown38);
		this.groupWeather.Controls.Add(this.numericUpDown39);
		this.groupWeather.Controls.Add(this.numericUpDown40);
		this.groupWeather.Controls.Add(this.numericUpDown41);
		this.groupWeather.Controls.Add(this.numericUpDown42);
		this.groupWeather.Controls.Add(this.numericUpDown43);
		this.groupWeather.Controls.Add(this.numericUpDown44);
		this.groupWeather.Controls.Add(this.numericUpDown45);
		this.groupWeather.Controls.Add(this.numericUpDown46);
		this.groupWeather.Controls.Add(this.numericUpDown47);
		this.groupWeather.Controls.Add(this.numericUpDown48);
		this.groupWeather.Controls.Add(this.label80);
		this.groupWeather.Controls.Add(this.label76);
		this.groupWeather.Controls.Add(this.label77);
		this.groupWeather.Controls.Add(this.label78);
		this.groupWeather.Controls.Add(this.label79);
		this.groupWeather.Controls.Add(this.label74);
		this.groupWeather.Controls.Add(this.label75);
		this.groupWeather.Controls.Add(this.label73);
		this.groupWeather.Controls.Add(this.label72);
		this.groupWeather.Controls.Add(this.toolWeather);
		this.groupWeather.Controls.Add(this.comboBox23);
		this.groupWeather.Controls.Add(this.comboBox24);
		this.groupWeather.Controls.Add(this.numericUpDown34);
		this.groupWeather.Controls.Add(this.numericUpDown35);
		this.groupWeather.Controls.Add(this.numericUpDown36);
		this.groupWeather.Controls.Add(this.comboBox21);
		this.groupWeather.Controls.Add(this.comboBox22);
		this.groupWeather.Controls.Add(this.numericUpDown31);
		this.groupWeather.Controls.Add(this.numericUpDown32);
		this.groupWeather.Controls.Add(this.numericUpDown33);
		this.groupWeather.Controls.Add(this.comboBox19);
		this.groupWeather.Controls.Add(this.comboBox20);
		this.groupWeather.Controls.Add(this.numericUpDown28);
		this.groupWeather.Controls.Add(this.numericUpDown29);
		this.groupWeather.Controls.Add(this.numericUpDown30);
		this.groupWeather.Controls.Add(this.comboBox17);
		this.groupWeather.Controls.Add(this.comboBox18);
		this.groupWeather.Controls.Add(this.numericUpDown25);
		this.groupWeather.Controls.Add(this.numericUpDown26);
		this.groupWeather.Controls.Add(this.numericUpDown27);
		this.groupWeather.Controls.Add(this.comboBox15);
		this.groupWeather.Controls.Add(this.comboBox16);
		this.groupWeather.Controls.Add(this.numericUpDown22);
		this.groupWeather.Controls.Add(this.numericUpDown23);
		this.groupWeather.Controls.Add(this.numericUpDown24);
		this.groupWeather.Controls.Add(this.comboBox13);
		this.groupWeather.Controls.Add(this.comboBox14);
		this.groupWeather.Controls.Add(this.numericUpDown19);
		this.groupWeather.Controls.Add(this.numericUpDown20);
		this.groupWeather.Controls.Add(this.numericUpDown21);
		this.groupWeather.Controls.Add(this.comboBox11);
		this.groupWeather.Controls.Add(this.comboBox12);
		this.groupWeather.Controls.Add(this.numericUpDown16);
		this.groupWeather.Controls.Add(this.numericUpDown17);
		this.groupWeather.Controls.Add(this.numericUpDown18);
		this.groupWeather.Controls.Add(this.comboBox9);
		this.groupWeather.Controls.Add(this.comboBox10);
		this.groupWeather.Controls.Add(this.numericUpDown13);
		this.groupWeather.Controls.Add(this.numericUpDown14);
		this.groupWeather.Controls.Add(this.numericUpDown15);
		this.groupWeather.Controls.Add(this.comboBox7);
		this.groupWeather.Controls.Add(this.comboBox8);
		this.groupWeather.Controls.Add(this.numericUpDown10);
		this.groupWeather.Controls.Add(this.numericUpDown11);
		this.groupWeather.Controls.Add(this.numericUpDown12);
		this.groupWeather.Controls.Add(this.comboBox5);
		this.groupWeather.Controls.Add(this.comboBox6);
		this.groupWeather.Controls.Add(this.numericUpDown7);
		this.groupWeather.Controls.Add(this.numericUpDown8);
		this.groupWeather.Controls.Add(this.numericUpDown9);
		this.groupWeather.Controls.Add(this.comboBox3);
		this.groupWeather.Controls.Add(this.comboBox4);
		this.groupWeather.Controls.Add(this.numericUpDown4);
		this.groupWeather.Controls.Add(this.numericUpDown5);
		this.groupWeather.Controls.Add(this.numericUpDown6);
		this.groupWeather.Controls.Add(this.comboBox1);
		this.groupWeather.Controls.Add(this.comboBox2);
		this.groupWeather.Controls.Add(this.numericUpDown1);
		this.groupWeather.Controls.Add(this.numericUpDown2);
		this.groupWeather.Controls.Add(this.numericUpDown3);
		this.groupWeather.Controls.Add(this.label28);
		this.groupWeather.Controls.Add(this.label27);
		this.groupWeather.Controls.Add(this.label26);
		this.groupWeather.Controls.Add(this.label25);
		this.groupWeather.Controls.Add(this.label24);
		this.groupWeather.Controls.Add(this.label23);
		this.groupWeather.Controls.Add(this.label22);
		this.groupWeather.Controls.Add(this.label21);
		this.groupWeather.Controls.Add(this.label20);
		this.groupWeather.Controls.Add(this.label19);
		this.groupWeather.Controls.Add(this.label18);
		this.groupWeather.Controls.Add(this.label17);
		this.groupWeather.Location = new System.Drawing.Point(17, 146);
		this.groupWeather.Name = "groupWeather";
		this.groupWeather.Size = new System.Drawing.Size(756, 498);
		this.groupWeather.TabIndex = 12;
		this.groupWeather.TabStop = false;
		this.groupWeather.Text = "Weather";
		this.label30.Image = (System.Drawing.Image)resources.GetObject("label30.Image");
		this.label30.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label30.Location = new System.Drawing.Point(686, 72);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(64, 64);
		this.label30.TabIndex = 160;
		this.label30.Text = "Night";
		this.label30.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label29.Image = (System.Drawing.Image)resources.GetObject("label29.Image");
		this.label29.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label29.Location = new System.Drawing.Point(616, 72);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(64, 64);
		this.label29.TabIndex = 159;
		this.label29.Text = "Sunset";
		this.label29.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.numericUpDown97.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown97.Location = new System.Drawing.Point(487, 436);
		this.numericUpDown97.Name = "numericUpDown97";
		this.numericUpDown97.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown97.TabIndex = 158;
		this.numericUpDown97.Tag = "O11";
		this.numericUpDown97.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown97.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown98.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown98.Location = new System.Drawing.Point(487, 409);
		this.numericUpDown98.Name = "numericUpDown98";
		this.numericUpDown98.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown98.TabIndex = 157;
		this.numericUpDown98.Tag = "O10";
		this.numericUpDown98.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown98.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown99.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown99.Location = new System.Drawing.Point(487, 382);
		this.numericUpDown99.Name = "numericUpDown99";
		this.numericUpDown99.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown99.TabIndex = 156;
		this.numericUpDown99.Tag = "O9";
		this.numericUpDown99.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown99.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown100.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown100.Location = new System.Drawing.Point(487, 355);
		this.numericUpDown100.Name = "numericUpDown100";
		this.numericUpDown100.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown100.TabIndex = 155;
		this.numericUpDown100.Tag = "O8";
		this.numericUpDown100.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown100.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown101.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown101.Location = new System.Drawing.Point(487, 328);
		this.numericUpDown101.Name = "numericUpDown101";
		this.numericUpDown101.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown101.TabIndex = 154;
		this.numericUpDown101.Tag = "O7";
		this.numericUpDown101.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown101.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown102.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown102.Location = new System.Drawing.Point(487, 301);
		this.numericUpDown102.Name = "numericUpDown102";
		this.numericUpDown102.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown102.TabIndex = 153;
		this.numericUpDown102.Tag = "O6";
		this.numericUpDown102.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown102.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown103.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown103.Location = new System.Drawing.Point(487, 274);
		this.numericUpDown103.Name = "numericUpDown103";
		this.numericUpDown103.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown103.TabIndex = 152;
		this.numericUpDown103.Tag = "O5";
		this.numericUpDown103.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown103.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown104.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown104.Location = new System.Drawing.Point(487, 247);
		this.numericUpDown104.Name = "numericUpDown104";
		this.numericUpDown104.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown104.TabIndex = 151;
		this.numericUpDown104.Tag = "O4";
		this.numericUpDown104.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown104.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown105.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown105.Location = new System.Drawing.Point(487, 220);
		this.numericUpDown105.Name = "numericUpDown105";
		this.numericUpDown105.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown105.TabIndex = 150;
		this.numericUpDown105.Tag = "O3";
		this.numericUpDown105.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown105.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown106.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown106.Location = new System.Drawing.Point(487, 193);
		this.numericUpDown106.Name = "numericUpDown106";
		this.numericUpDown106.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown106.TabIndex = 149;
		this.numericUpDown106.Tag = "O2";
		this.numericUpDown106.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown106.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown107.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown107.Location = new System.Drawing.Point(487, 165);
		this.numericUpDown107.Name = "numericUpDown107";
		this.numericUpDown107.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown107.TabIndex = 148;
		this.numericUpDown107.Tag = "O1";
		this.numericUpDown107.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown107.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown108.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown108.Location = new System.Drawing.Point(487, 139);
		this.numericUpDown108.Name = "numericUpDown108";
		this.numericUpDown108.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown108.TabIndex = 147;
		this.numericUpDown108.Tag = "O0";
		this.numericUpDown108.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown108.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown85.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown85.Location = new System.Drawing.Point(423, 436);
		this.numericUpDown85.Name = "numericUpDown85";
		this.numericUpDown85.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown85.TabIndex = 146;
		this.numericUpDown85.Tag = "O11";
		this.numericUpDown85.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown85.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown86.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown86.Location = new System.Drawing.Point(423, 409);
		this.numericUpDown86.Name = "numericUpDown86";
		this.numericUpDown86.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown86.TabIndex = 145;
		this.numericUpDown86.Tag = "O10";
		this.numericUpDown86.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown86.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown87.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown87.Location = new System.Drawing.Point(423, 382);
		this.numericUpDown87.Name = "numericUpDown87";
		this.numericUpDown87.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown87.TabIndex = 144;
		this.numericUpDown87.Tag = "O9";
		this.numericUpDown87.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown87.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown88.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown88.Location = new System.Drawing.Point(423, 355);
		this.numericUpDown88.Name = "numericUpDown88";
		this.numericUpDown88.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown88.TabIndex = 143;
		this.numericUpDown88.Tag = "O8";
		this.numericUpDown88.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown88.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown89.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown89.Location = new System.Drawing.Point(423, 328);
		this.numericUpDown89.Name = "numericUpDown89";
		this.numericUpDown89.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown89.TabIndex = 142;
		this.numericUpDown89.Tag = "O7";
		this.numericUpDown89.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown89.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown90.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown90.Location = new System.Drawing.Point(423, 301);
		this.numericUpDown90.Name = "numericUpDown90";
		this.numericUpDown90.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown90.TabIndex = 141;
		this.numericUpDown90.Tag = "O6";
		this.numericUpDown90.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown90.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown91.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown91.Location = new System.Drawing.Point(423, 274);
		this.numericUpDown91.Name = "numericUpDown91";
		this.numericUpDown91.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown91.TabIndex = 140;
		this.numericUpDown91.Tag = "O5";
		this.numericUpDown91.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown91.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown92.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown92.Location = new System.Drawing.Point(423, 247);
		this.numericUpDown92.Name = "numericUpDown92";
		this.numericUpDown92.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown92.TabIndex = 139;
		this.numericUpDown92.Tag = "O4";
		this.numericUpDown92.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown92.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown93.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown93.Location = new System.Drawing.Point(423, 220);
		this.numericUpDown93.Name = "numericUpDown93";
		this.numericUpDown93.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown93.TabIndex = 138;
		this.numericUpDown93.Tag = "O3";
		this.numericUpDown93.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown93.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown94.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown94.Location = new System.Drawing.Point(423, 193);
		this.numericUpDown94.Name = "numericUpDown94";
		this.numericUpDown94.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown94.TabIndex = 137;
		this.numericUpDown94.Tag = "O2";
		this.numericUpDown94.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown94.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown95.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown95.Location = new System.Drawing.Point(423, 165);
		this.numericUpDown95.Name = "numericUpDown95";
		this.numericUpDown95.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown95.TabIndex = 136;
		this.numericUpDown95.Tag = "O1";
		this.numericUpDown95.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown95.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown96.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown96.Location = new System.Drawing.Point(423, 139);
		this.numericUpDown96.Name = "numericUpDown96";
		this.numericUpDown96.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown96.TabIndex = 135;
		this.numericUpDown96.Tag = "O0";
		this.numericUpDown96.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown96.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown73.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown73.Location = new System.Drawing.Point(298, 436);
		this.numericUpDown73.Name = "numericUpDown73";
		this.numericUpDown73.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown73.TabIndex = 134;
		this.numericUpDown73.Tag = "O11";
		this.numericUpDown73.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown73.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown74.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown74.Location = new System.Drawing.Point(298, 409);
		this.numericUpDown74.Name = "numericUpDown74";
		this.numericUpDown74.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown74.TabIndex = 133;
		this.numericUpDown74.Tag = "O10";
		this.numericUpDown74.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown74.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown75.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown75.Location = new System.Drawing.Point(298, 382);
		this.numericUpDown75.Name = "numericUpDown75";
		this.numericUpDown75.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown75.TabIndex = 132;
		this.numericUpDown75.Tag = "O9";
		this.numericUpDown75.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown75.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown76.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown76.Location = new System.Drawing.Point(298, 355);
		this.numericUpDown76.Name = "numericUpDown76";
		this.numericUpDown76.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown76.TabIndex = 131;
		this.numericUpDown76.Tag = "O8";
		this.numericUpDown76.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown76.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown77.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown77.Location = new System.Drawing.Point(298, 328);
		this.numericUpDown77.Name = "numericUpDown77";
		this.numericUpDown77.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown77.TabIndex = 130;
		this.numericUpDown77.Tag = "O7";
		this.numericUpDown77.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown77.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown78.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown78.Location = new System.Drawing.Point(298, 301);
		this.numericUpDown78.Name = "numericUpDown78";
		this.numericUpDown78.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown78.TabIndex = 129;
		this.numericUpDown78.Tag = "O6";
		this.numericUpDown78.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown78.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown79.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown79.Location = new System.Drawing.Point(298, 274);
		this.numericUpDown79.Name = "numericUpDown79";
		this.numericUpDown79.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown79.TabIndex = 128;
		this.numericUpDown79.Tag = "O5";
		this.numericUpDown79.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown79.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown80.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown80.Location = new System.Drawing.Point(298, 247);
		this.numericUpDown80.Name = "numericUpDown80";
		this.numericUpDown80.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown80.TabIndex = 127;
		this.numericUpDown80.Tag = "O4";
		this.numericUpDown80.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown80.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown81.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown81.Location = new System.Drawing.Point(298, 220);
		this.numericUpDown81.Name = "numericUpDown81";
		this.numericUpDown81.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown81.TabIndex = 126;
		this.numericUpDown81.Tag = "O3";
		this.numericUpDown81.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown81.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown82.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown82.Location = new System.Drawing.Point(298, 193);
		this.numericUpDown82.Name = "numericUpDown82";
		this.numericUpDown82.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown82.TabIndex = 125;
		this.numericUpDown82.Tag = "O2";
		this.numericUpDown82.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown82.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown83.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown83.Location = new System.Drawing.Point(298, 165);
		this.numericUpDown83.Name = "numericUpDown83";
		this.numericUpDown83.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown83.TabIndex = 124;
		this.numericUpDown83.Tag = "O1";
		this.numericUpDown83.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown83.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown84.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown84.Location = new System.Drawing.Point(298, 139);
		this.numericUpDown84.Name = "numericUpDown84";
		this.numericUpDown84.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown84.TabIndex = 123;
		this.numericUpDown84.Tag = "O0";
		this.numericUpDown84.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown84.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown61.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown61.Location = new System.Drawing.Point(171, 435);
		this.numericUpDown61.Name = "numericUpDown61";
		this.numericUpDown61.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown61.TabIndex = 122;
		this.numericUpDown61.Tag = "O11";
		this.numericUpDown61.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown61.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown62.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown62.Location = new System.Drawing.Point(171, 408);
		this.numericUpDown62.Name = "numericUpDown62";
		this.numericUpDown62.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown62.TabIndex = 121;
		this.numericUpDown62.Tag = "O10";
		this.numericUpDown62.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown62.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown63.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown63.Location = new System.Drawing.Point(171, 381);
		this.numericUpDown63.Name = "numericUpDown63";
		this.numericUpDown63.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown63.TabIndex = 120;
		this.numericUpDown63.Tag = "O9";
		this.numericUpDown63.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown63.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown64.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown64.Location = new System.Drawing.Point(171, 354);
		this.numericUpDown64.Name = "numericUpDown64";
		this.numericUpDown64.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown64.TabIndex = 119;
		this.numericUpDown64.Tag = "O8";
		this.numericUpDown64.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown64.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown65.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown65.Location = new System.Drawing.Point(171, 327);
		this.numericUpDown65.Name = "numericUpDown65";
		this.numericUpDown65.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown65.TabIndex = 118;
		this.numericUpDown65.Tag = "O7";
		this.numericUpDown65.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown65.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown66.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown66.Location = new System.Drawing.Point(171, 300);
		this.numericUpDown66.Name = "numericUpDown66";
		this.numericUpDown66.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown66.TabIndex = 117;
		this.numericUpDown66.Tag = "O6";
		this.numericUpDown66.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown66.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown67.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown67.Location = new System.Drawing.Point(171, 273);
		this.numericUpDown67.Name = "numericUpDown67";
		this.numericUpDown67.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown67.TabIndex = 116;
		this.numericUpDown67.Tag = "O5";
		this.numericUpDown67.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown67.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown68.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown68.Location = new System.Drawing.Point(171, 246);
		this.numericUpDown68.Name = "numericUpDown68";
		this.numericUpDown68.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown68.TabIndex = 115;
		this.numericUpDown68.Tag = "O4";
		this.numericUpDown68.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown68.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown69.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown69.Location = new System.Drawing.Point(171, 219);
		this.numericUpDown69.Name = "numericUpDown69";
		this.numericUpDown69.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown69.TabIndex = 114;
		this.numericUpDown69.Tag = "O3";
		this.numericUpDown69.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown69.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown70.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown70.Location = new System.Drawing.Point(171, 192);
		this.numericUpDown70.Name = "numericUpDown70";
		this.numericUpDown70.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown70.TabIndex = 113;
		this.numericUpDown70.Tag = "O2";
		this.numericUpDown70.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown70.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown71.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown71.Location = new System.Drawing.Point(171, 164);
		this.numericUpDown71.Name = "numericUpDown71";
		this.numericUpDown71.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown71.TabIndex = 112;
		this.numericUpDown71.Tag = "O1";
		this.numericUpDown71.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown71.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown72.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown72.Location = new System.Drawing.Point(171, 139);
		this.numericUpDown72.Name = "numericUpDown72";
		this.numericUpDown72.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown72.TabIndex = 111;
		this.numericUpDown72.Tag = "O0";
		this.numericUpDown72.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown72.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown49.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown49.Location = new System.Drawing.Point(108, 435);
		this.numericUpDown49.Name = "numericUpDown49";
		this.numericUpDown49.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown49.TabIndex = 110;
		this.numericUpDown49.Tag = "O11";
		this.numericUpDown49.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown49.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown50.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown50.Location = new System.Drawing.Point(108, 408);
		this.numericUpDown50.Name = "numericUpDown50";
		this.numericUpDown50.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown50.TabIndex = 109;
		this.numericUpDown50.Tag = "O10";
		this.numericUpDown50.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown50.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown51.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown51.Location = new System.Drawing.Point(108, 381);
		this.numericUpDown51.Name = "numericUpDown51";
		this.numericUpDown51.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown51.TabIndex = 108;
		this.numericUpDown51.Tag = "O9";
		this.numericUpDown51.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown51.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown52.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown52.Location = new System.Drawing.Point(108, 354);
		this.numericUpDown52.Name = "numericUpDown52";
		this.numericUpDown52.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown52.TabIndex = 107;
		this.numericUpDown52.Tag = "O8";
		this.numericUpDown52.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown52.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown53.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown53.Location = new System.Drawing.Point(108, 327);
		this.numericUpDown53.Name = "numericUpDown53";
		this.numericUpDown53.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown53.TabIndex = 106;
		this.numericUpDown53.Tag = "O7";
		this.numericUpDown53.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown53.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown54.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown54.Location = new System.Drawing.Point(108, 300);
		this.numericUpDown54.Name = "numericUpDown54";
		this.numericUpDown54.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown54.TabIndex = 105;
		this.numericUpDown54.Tag = "O6";
		this.numericUpDown54.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown54.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown55.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown55.Location = new System.Drawing.Point(108, 273);
		this.numericUpDown55.Name = "numericUpDown55";
		this.numericUpDown55.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown55.TabIndex = 104;
		this.numericUpDown55.Tag = "O5";
		this.numericUpDown55.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown55.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown56.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown56.Location = new System.Drawing.Point(108, 246);
		this.numericUpDown56.Name = "numericUpDown56";
		this.numericUpDown56.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown56.TabIndex = 103;
		this.numericUpDown56.Tag = "O4";
		this.numericUpDown56.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown56.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown57.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown57.Location = new System.Drawing.Point(108, 219);
		this.numericUpDown57.Name = "numericUpDown57";
		this.numericUpDown57.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown57.TabIndex = 102;
		this.numericUpDown57.Tag = "O3";
		this.numericUpDown57.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown57.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown58.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown58.Location = new System.Drawing.Point(108, 192);
		this.numericUpDown58.Name = "numericUpDown58";
		this.numericUpDown58.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown58.TabIndex = 101;
		this.numericUpDown58.Tag = "O2";
		this.numericUpDown58.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown58.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown59.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown59.Location = new System.Drawing.Point(108, 164);
		this.numericUpDown59.Name = "numericUpDown59";
		this.numericUpDown59.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown59.TabIndex = 100;
		this.numericUpDown59.Tag = "O1";
		this.numericUpDown59.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown59.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown60.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown60.Location = new System.Drawing.Point(108, 139);
		this.numericUpDown60.Name = "numericUpDown60";
		this.numericUpDown60.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown60.TabIndex = 99;
		this.numericUpDown60.Tag = "O0";
		this.numericUpDown60.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown60.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown37.Enabled = false;
		this.numericUpDown37.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown37.Location = new System.Drawing.Point(45, 435);
		this.numericUpDown37.Name = "numericUpDown37";
		this.numericUpDown37.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown37.TabIndex = 98;
		this.numericUpDown37.Tag = "O11";
		this.numericUpDown37.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown37.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown38.Enabled = false;
		this.numericUpDown38.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown38.Location = new System.Drawing.Point(45, 408);
		this.numericUpDown38.Name = "numericUpDown38";
		this.numericUpDown38.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown38.TabIndex = 97;
		this.numericUpDown38.Tag = "O10";
		this.numericUpDown38.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown38.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown39.Enabled = false;
		this.numericUpDown39.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown39.Location = new System.Drawing.Point(45, 381);
		this.numericUpDown39.Name = "numericUpDown39";
		this.numericUpDown39.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown39.TabIndex = 96;
		this.numericUpDown39.Tag = "O9";
		this.numericUpDown39.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown39.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown40.Enabled = false;
		this.numericUpDown40.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown40.Location = new System.Drawing.Point(45, 354);
		this.numericUpDown40.Name = "numericUpDown40";
		this.numericUpDown40.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown40.TabIndex = 95;
		this.numericUpDown40.Tag = "O8";
		this.numericUpDown40.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown40.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown41.Enabled = false;
		this.numericUpDown41.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown41.Location = new System.Drawing.Point(45, 327);
		this.numericUpDown41.Name = "numericUpDown41";
		this.numericUpDown41.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown41.TabIndex = 94;
		this.numericUpDown41.Tag = "O7";
		this.numericUpDown41.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown41.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown42.Enabled = false;
		this.numericUpDown42.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown42.Location = new System.Drawing.Point(45, 300);
		this.numericUpDown42.Name = "numericUpDown42";
		this.numericUpDown42.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown42.TabIndex = 93;
		this.numericUpDown42.Tag = "O6";
		this.numericUpDown42.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown42.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown43.Enabled = false;
		this.numericUpDown43.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown43.Location = new System.Drawing.Point(45, 273);
		this.numericUpDown43.Name = "numericUpDown43";
		this.numericUpDown43.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown43.TabIndex = 92;
		this.numericUpDown43.Tag = "O5";
		this.numericUpDown43.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown43.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown44.Enabled = false;
		this.numericUpDown44.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown44.Location = new System.Drawing.Point(45, 246);
		this.numericUpDown44.Name = "numericUpDown44";
		this.numericUpDown44.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown44.TabIndex = 91;
		this.numericUpDown44.Tag = "O4";
		this.numericUpDown44.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown44.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown45.Enabled = false;
		this.numericUpDown45.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown45.Location = new System.Drawing.Point(45, 219);
		this.numericUpDown45.Name = "numericUpDown45";
		this.numericUpDown45.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown45.TabIndex = 90;
		this.numericUpDown45.Tag = "O3";
		this.numericUpDown45.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown45.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown46.Enabled = false;
		this.numericUpDown46.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown46.Location = new System.Drawing.Point(45, 192);
		this.numericUpDown46.Name = "numericUpDown46";
		this.numericUpDown46.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown46.TabIndex = 89;
		this.numericUpDown46.Tag = "O2";
		this.numericUpDown46.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown46.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown47.Enabled = false;
		this.numericUpDown47.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown47.Location = new System.Drawing.Point(45, 164);
		this.numericUpDown47.Name = "numericUpDown47";
		this.numericUpDown47.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown47.TabIndex = 88;
		this.numericUpDown47.Tag = "O1";
		this.numericUpDown47.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown47.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown48.Enabled = false;
		this.numericUpDown48.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown48.Location = new System.Drawing.Point(45, 139);
		this.numericUpDown48.Name = "numericUpDown48";
		this.numericUpDown48.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown48.TabIndex = 87;
		this.numericUpDown48.Tag = "O0";
		this.numericUpDown48.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown48.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.label80.Image = (System.Drawing.Image)resources.GetObject("label80.Image");
		this.label80.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label80.Location = new System.Drawing.Point(549, 72);
		this.label80.Name = "label80";
		this.label80.Size = new System.Drawing.Size(64, 64);
		this.label80.TabIndex = 86;
		this.label80.Text = "Snow";
		this.label80.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label76.Image = (System.Drawing.Image)resources.GetObject("label76.Image");
		this.label76.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label76.Location = new System.Drawing.Point(485, 72);
		this.label76.Name = "label76";
		this.label76.Size = new System.Drawing.Size(64, 64);
		this.label76.TabIndex = 85;
		this.label76.Text = "Flurries";
		this.label76.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label77.Image = (System.Drawing.Image)resources.GetObject("label77.Image");
		this.label77.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label77.Location = new System.Drawing.Point(421, 72);
		this.label77.Name = "label77";
		this.label77.Size = new System.Drawing.Size(64, 64);
		this.label77.TabIndex = 84;
		this.label77.Text = "Showers";
		this.label77.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label78.Image = (System.Drawing.Image)resources.GetObject("label78.Image");
		this.label78.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label78.Location = new System.Drawing.Point(357, 72);
		this.label78.Name = "label78";
		this.label78.Size = new System.Drawing.Size(64, 64);
		this.label78.TabIndex = 83;
		this.label78.Text = "Rain";
		this.label78.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label79.Image = (System.Drawing.Image)resources.GetObject("label79.Image");
		this.label79.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label79.Location = new System.Drawing.Point(293, 72);
		this.label79.Name = "label79";
		this.label79.Size = new System.Drawing.Size(64, 64);
		this.label79.TabIndex = 82;
		this.label79.Text = "Foggy";
		this.label79.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label74.Image = (System.Drawing.Image)resources.GetObject("label74.Image");
		this.label74.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label74.Location = new System.Drawing.Point(229, 72);
		this.label74.Name = "label74";
		this.label74.Size = new System.Drawing.Size(64, 64);
		this.label74.TabIndex = 81;
		this.label74.Text = "Overcast";
		this.label74.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label75.Image = (System.Drawing.Image)resources.GetObject("label75.Image");
		this.label75.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label75.Location = new System.Drawing.Point(165, 72);
		this.label75.Name = "label75";
		this.label75.Size = new System.Drawing.Size(64, 64);
		this.label75.TabIndex = 80;
		this.label75.Text = "Cloudy";
		this.label75.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label73.Image = (System.Drawing.Image)resources.GetObject("label73.Image");
		this.label73.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label73.Location = new System.Drawing.Point(101, 72);
		this.label73.Name = "label73";
		this.label73.Size = new System.Drawing.Size(64, 64);
		this.label73.TabIndex = 79;
		this.label73.Text = "Hazy";
		this.label73.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.label72.Image = (System.Drawing.Image)resources.GetObject("label72.Image");
		this.label72.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label72.Location = new System.Drawing.Point(37, 72);
		this.label72.Name = "label72";
		this.label72.Size = new System.Drawing.Size(64, 64);
		this.label72.TabIndex = 78;
		this.label72.Text = "Clear";
		this.label72.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.toolWeather.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolWeather.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.buttonCopyWeather, this.buttonPasteWeather });
		this.toolWeather.Location = new System.Drawing.Point(3, 16);
		this.toolWeather.Name = "toolWeather";
		this.toolWeather.Size = new System.Drawing.Size(750, 55);
		this.toolWeather.TabIndex = 77;
		this.buttonCopyWeather.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCopyWeather.Image = (System.Drawing.Image)resources.GetObject("buttonCopyWeather.Image");
		this.buttonCopyWeather.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCopyWeather.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCopyWeather.Name = "buttonCopyWeather";
		this.buttonCopyWeather.Size = new System.Drawing.Size(52, 52);
		this.buttonCopyWeather.Text = "Copy Weather";
		this.buttonCopyWeather.Click += new System.EventHandler(buttonCopyWeather_Click);
		this.buttonPasteWeather.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPasteWeather.Enabled = false;
		this.buttonPasteWeather.Image = (System.Drawing.Image)resources.GetObject("buttonPasteWeather.Image");
		this.buttonPasteWeather.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonPasteWeather.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPasteWeather.Name = "buttonPasteWeather";
		this.buttonPasteWeather.Size = new System.Drawing.Size(52, 52);
		this.buttonPasteWeather.Text = "Paste Weather";
		this.buttonPasteWeather.Click += new System.EventHandler(buttonPasteWeather_Click);
		this.comboBox23.FormattingEnabled = true;
		this.comboBox23.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox23.Location = new System.Drawing.Point(684, 435);
		this.comboBox23.Name = "comboBox23";
		this.comboBox23.Size = new System.Drawing.Size(69, 21);
		this.comboBox23.TabIndex = 76;
		this.comboBox23.Tag = "N11";
		this.comboBox23.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox24.FormattingEnabled = true;
		this.comboBox24.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox24.Location = new System.Drawing.Point(611, 435);
		this.comboBox24.Name = "comboBox24";
		this.comboBox24.Size = new System.Drawing.Size(69, 21);
		this.comboBox24.TabIndex = 75;
		this.comboBox24.Tag = "U11";
		this.comboBox24.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown34.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown34.Location = new System.Drawing.Point(234, 435);
		this.numericUpDown34.Name = "numericUpDown34";
		this.numericUpDown34.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown34.TabIndex = 74;
		this.numericUpDown34.Tag = "O11";
		this.numericUpDown34.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown34.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown35.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown35.Location = new System.Drawing.Point(550, 436);
		this.numericUpDown35.Name = "numericUpDown35";
		this.numericUpDown35.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown35.TabIndex = 73;
		this.numericUpDown35.Tag = "S11";
		this.numericUpDown35.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown35.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown36.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown36.Location = new System.Drawing.Point(360, 436);
		this.numericUpDown36.Name = "numericUpDown36";
		this.numericUpDown36.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown36.TabIndex = 72;
		this.numericUpDown36.Tag = "R11";
		this.numericUpDown36.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown36.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox21.FormattingEnabled = true;
		this.comboBox21.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox21.Location = new System.Drawing.Point(684, 408);
		this.comboBox21.Name = "comboBox21";
		this.comboBox21.Size = new System.Drawing.Size(69, 21);
		this.comboBox21.TabIndex = 71;
		this.comboBox21.Tag = "N10";
		this.comboBox21.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox22.FormattingEnabled = true;
		this.comboBox22.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox22.Location = new System.Drawing.Point(611, 408);
		this.comboBox22.Name = "comboBox22";
		this.comboBox22.Size = new System.Drawing.Size(69, 21);
		this.comboBox22.TabIndex = 70;
		this.comboBox22.Tag = "U10";
		this.comboBox22.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown31.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown31.Location = new System.Drawing.Point(234, 408);
		this.numericUpDown31.Name = "numericUpDown31";
		this.numericUpDown31.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown31.TabIndex = 69;
		this.numericUpDown31.Tag = "O10";
		this.numericUpDown31.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown31.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown32.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown32.Location = new System.Drawing.Point(550, 409);
		this.numericUpDown32.Name = "numericUpDown32";
		this.numericUpDown32.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown32.TabIndex = 68;
		this.numericUpDown32.Tag = "S10";
		this.numericUpDown32.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown32.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown33.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown33.Location = new System.Drawing.Point(360, 409);
		this.numericUpDown33.Name = "numericUpDown33";
		this.numericUpDown33.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown33.TabIndex = 67;
		this.numericUpDown33.Tag = "R10";
		this.numericUpDown33.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown33.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox19.FormattingEnabled = true;
		this.comboBox19.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox19.Location = new System.Drawing.Point(684, 381);
		this.comboBox19.Name = "comboBox19";
		this.comboBox19.Size = new System.Drawing.Size(69, 21);
		this.comboBox19.TabIndex = 66;
		this.comboBox19.Tag = "N09";
		this.comboBox19.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox20.FormattingEnabled = true;
		this.comboBox20.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox20.Location = new System.Drawing.Point(611, 381);
		this.comboBox20.Name = "comboBox20";
		this.comboBox20.Size = new System.Drawing.Size(69, 21);
		this.comboBox20.TabIndex = 65;
		this.comboBox20.Tag = "U09";
		this.comboBox20.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown28.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown28.Location = new System.Drawing.Point(234, 381);
		this.numericUpDown28.Name = "numericUpDown28";
		this.numericUpDown28.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown28.TabIndex = 64;
		this.numericUpDown28.Tag = "O9";
		this.numericUpDown28.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown28.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown29.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown29.Location = new System.Drawing.Point(550, 382);
		this.numericUpDown29.Name = "numericUpDown29";
		this.numericUpDown29.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown29.TabIndex = 63;
		this.numericUpDown29.Tag = "S9";
		this.numericUpDown29.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown29.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown30.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown30.Location = new System.Drawing.Point(360, 382);
		this.numericUpDown30.Name = "numericUpDown30";
		this.numericUpDown30.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown30.TabIndex = 62;
		this.numericUpDown30.Tag = "R9";
		this.numericUpDown30.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown30.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox17.FormattingEnabled = true;
		this.comboBox17.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox17.Location = new System.Drawing.Point(684, 354);
		this.comboBox17.Name = "comboBox17";
		this.comboBox17.Size = new System.Drawing.Size(69, 21);
		this.comboBox17.TabIndex = 61;
		this.comboBox17.Tag = "N08";
		this.comboBox17.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox18.FormattingEnabled = true;
		this.comboBox18.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox18.Location = new System.Drawing.Point(611, 354);
		this.comboBox18.Name = "comboBox18";
		this.comboBox18.Size = new System.Drawing.Size(69, 21);
		this.comboBox18.TabIndex = 60;
		this.comboBox18.Tag = "U08";
		this.comboBox18.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown25.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown25.Location = new System.Drawing.Point(234, 354);
		this.numericUpDown25.Name = "numericUpDown25";
		this.numericUpDown25.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown25.TabIndex = 59;
		this.numericUpDown25.Tag = "O8";
		this.numericUpDown25.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown25.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown26.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown26.Location = new System.Drawing.Point(550, 355);
		this.numericUpDown26.Name = "numericUpDown26";
		this.numericUpDown26.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown26.TabIndex = 58;
		this.numericUpDown26.Tag = "S8";
		this.numericUpDown26.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown26.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown27.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown27.Location = new System.Drawing.Point(360, 355);
		this.numericUpDown27.Name = "numericUpDown27";
		this.numericUpDown27.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown27.TabIndex = 57;
		this.numericUpDown27.Tag = "R8";
		this.numericUpDown27.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown27.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox15.FormattingEnabled = true;
		this.comboBox15.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox15.Location = new System.Drawing.Point(684, 327);
		this.comboBox15.Name = "comboBox15";
		this.comboBox15.Size = new System.Drawing.Size(69, 21);
		this.comboBox15.TabIndex = 56;
		this.comboBox15.Tag = "N07";
		this.comboBox15.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox16.FormattingEnabled = true;
		this.comboBox16.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox16.Location = new System.Drawing.Point(611, 327);
		this.comboBox16.Name = "comboBox16";
		this.comboBox16.Size = new System.Drawing.Size(69, 21);
		this.comboBox16.TabIndex = 55;
		this.comboBox16.Tag = "U07";
		this.comboBox16.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown22.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown22.Location = new System.Drawing.Point(234, 327);
		this.numericUpDown22.Name = "numericUpDown22";
		this.numericUpDown22.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown22.TabIndex = 54;
		this.numericUpDown22.Tag = "O7";
		this.numericUpDown22.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown22.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown23.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown23.Location = new System.Drawing.Point(550, 328);
		this.numericUpDown23.Name = "numericUpDown23";
		this.numericUpDown23.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown23.TabIndex = 53;
		this.numericUpDown23.Tag = "S7";
		this.numericUpDown23.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown23.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown24.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown24.Location = new System.Drawing.Point(360, 328);
		this.numericUpDown24.Name = "numericUpDown24";
		this.numericUpDown24.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown24.TabIndex = 52;
		this.numericUpDown24.Tag = "R7";
		this.numericUpDown24.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown24.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox13.FormattingEnabled = true;
		this.comboBox13.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox13.Location = new System.Drawing.Point(684, 300);
		this.comboBox13.Name = "comboBox13";
		this.comboBox13.Size = new System.Drawing.Size(69, 21);
		this.comboBox13.TabIndex = 51;
		this.comboBox13.Tag = "N06";
		this.comboBox13.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox14.FormattingEnabled = true;
		this.comboBox14.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox14.Location = new System.Drawing.Point(611, 300);
		this.comboBox14.Name = "comboBox14";
		this.comboBox14.Size = new System.Drawing.Size(69, 21);
		this.comboBox14.TabIndex = 50;
		this.comboBox14.Tag = "U06";
		this.comboBox14.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown19.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown19.Location = new System.Drawing.Point(234, 300);
		this.numericUpDown19.Name = "numericUpDown19";
		this.numericUpDown19.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown19.TabIndex = 49;
		this.numericUpDown19.Tag = "O6";
		this.numericUpDown19.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown19.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown20.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown20.Location = new System.Drawing.Point(550, 301);
		this.numericUpDown20.Name = "numericUpDown20";
		this.numericUpDown20.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown20.TabIndex = 48;
		this.numericUpDown20.Tag = "S6";
		this.numericUpDown20.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown20.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown21.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown21.Location = new System.Drawing.Point(360, 301);
		this.numericUpDown21.Name = "numericUpDown21";
		this.numericUpDown21.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown21.TabIndex = 47;
		this.numericUpDown21.Tag = "R6";
		this.numericUpDown21.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown21.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox11.FormattingEnabled = true;
		this.comboBox11.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox11.Location = new System.Drawing.Point(684, 273);
		this.comboBox11.Name = "comboBox11";
		this.comboBox11.Size = new System.Drawing.Size(69, 21);
		this.comboBox11.TabIndex = 46;
		this.comboBox11.Tag = "N05";
		this.comboBox11.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox12.FormattingEnabled = true;
		this.comboBox12.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox12.Location = new System.Drawing.Point(611, 273);
		this.comboBox12.Name = "comboBox12";
		this.comboBox12.Size = new System.Drawing.Size(69, 21);
		this.comboBox12.TabIndex = 45;
		this.comboBox12.Tag = "U05";
		this.comboBox12.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown16.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown16.Location = new System.Drawing.Point(234, 273);
		this.numericUpDown16.Name = "numericUpDown16";
		this.numericUpDown16.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown16.TabIndex = 44;
		this.numericUpDown16.Tag = "O5";
		this.numericUpDown16.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown16.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown17.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown17.Location = new System.Drawing.Point(550, 274);
		this.numericUpDown17.Name = "numericUpDown17";
		this.numericUpDown17.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown17.TabIndex = 43;
		this.numericUpDown17.Tag = "S5";
		this.numericUpDown17.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown17.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown18.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown18.Location = new System.Drawing.Point(360, 274);
		this.numericUpDown18.Name = "numericUpDown18";
		this.numericUpDown18.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown18.TabIndex = 42;
		this.numericUpDown18.Tag = "R5";
		this.numericUpDown18.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown18.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox9.FormattingEnabled = true;
		this.comboBox9.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox9.Location = new System.Drawing.Point(684, 246);
		this.comboBox9.Name = "comboBox9";
		this.comboBox9.Size = new System.Drawing.Size(69, 21);
		this.comboBox9.TabIndex = 41;
		this.comboBox9.Tag = "N04";
		this.comboBox9.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox10.FormattingEnabled = true;
		this.comboBox10.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox10.Location = new System.Drawing.Point(611, 246);
		this.comboBox10.Name = "comboBox10";
		this.comboBox10.Size = new System.Drawing.Size(69, 21);
		this.comboBox10.TabIndex = 40;
		this.comboBox10.Tag = "U04";
		this.comboBox10.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown13.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown13.Location = new System.Drawing.Point(234, 246);
		this.numericUpDown13.Name = "numericUpDown13";
		this.numericUpDown13.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown13.TabIndex = 39;
		this.numericUpDown13.Tag = "O4";
		this.numericUpDown13.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown13.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown14.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown14.Location = new System.Drawing.Point(550, 247);
		this.numericUpDown14.Name = "numericUpDown14";
		this.numericUpDown14.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown14.TabIndex = 38;
		this.numericUpDown14.Tag = "S4";
		this.numericUpDown14.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown14.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown15.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown15.Location = new System.Drawing.Point(360, 247);
		this.numericUpDown15.Name = "numericUpDown15";
		this.numericUpDown15.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown15.TabIndex = 37;
		this.numericUpDown15.Tag = "R4";
		this.numericUpDown15.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown15.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox7.FormattingEnabled = true;
		this.comboBox7.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox7.Location = new System.Drawing.Point(685, 219);
		this.comboBox7.Name = "comboBox7";
		this.comboBox7.Size = new System.Drawing.Size(69, 21);
		this.comboBox7.TabIndex = 36;
		this.comboBox7.Tag = "N03";
		this.comboBox7.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox8.FormattingEnabled = true;
		this.comboBox8.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox8.Location = new System.Drawing.Point(612, 219);
		this.comboBox8.Name = "comboBox8";
		this.comboBox8.Size = new System.Drawing.Size(69, 21);
		this.comboBox8.TabIndex = 35;
		this.comboBox8.Tag = "U03";
		this.comboBox8.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown10.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown10.Location = new System.Drawing.Point(234, 219);
		this.numericUpDown10.Name = "numericUpDown10";
		this.numericUpDown10.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown10.TabIndex = 34;
		this.numericUpDown10.Tag = "O3";
		this.numericUpDown10.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown10.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown11.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown11.Location = new System.Drawing.Point(550, 220);
		this.numericUpDown11.Name = "numericUpDown11";
		this.numericUpDown11.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown11.TabIndex = 33;
		this.numericUpDown11.Tag = "S3";
		this.numericUpDown11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown11.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown12.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown12.Location = new System.Drawing.Point(360, 220);
		this.numericUpDown12.Name = "numericUpDown12";
		this.numericUpDown12.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown12.TabIndex = 32;
		this.numericUpDown12.Tag = "R3";
		this.numericUpDown12.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown12.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox5.FormattingEnabled = true;
		this.comboBox5.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox5.Location = new System.Drawing.Point(684, 192);
		this.comboBox5.Name = "comboBox5";
		this.comboBox5.Size = new System.Drawing.Size(69, 21);
		this.comboBox5.TabIndex = 31;
		this.comboBox5.Tag = "N02";
		this.comboBox5.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox6.FormattingEnabled = true;
		this.comboBox6.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox6.Location = new System.Drawing.Point(611, 192);
		this.comboBox6.Name = "comboBox6";
		this.comboBox6.Size = new System.Drawing.Size(69, 21);
		this.comboBox6.TabIndex = 30;
		this.comboBox6.Tag = "U02";
		this.comboBox6.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown7.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown7.Location = new System.Drawing.Point(234, 192);
		this.numericUpDown7.Name = "numericUpDown7";
		this.numericUpDown7.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown7.TabIndex = 29;
		this.numericUpDown7.Tag = "O2";
		this.numericUpDown7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown7.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown8.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown8.Location = new System.Drawing.Point(550, 193);
		this.numericUpDown8.Name = "numericUpDown8";
		this.numericUpDown8.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown8.TabIndex = 28;
		this.numericUpDown8.Tag = "S2";
		this.numericUpDown8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown8.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown9.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown9.Location = new System.Drawing.Point(360, 193);
		this.numericUpDown9.Name = "numericUpDown9";
		this.numericUpDown9.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown9.TabIndex = 27;
		this.numericUpDown9.Tag = "R2";
		this.numericUpDown9.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown9.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox3.FormattingEnabled = true;
		this.comboBox3.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox3.Location = new System.Drawing.Point(684, 164);
		this.comboBox3.Name = "comboBox3";
		this.comboBox3.Size = new System.Drawing.Size(69, 21);
		this.comboBox3.TabIndex = 26;
		this.comboBox3.Tag = "N01";
		this.comboBox3.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox4.FormattingEnabled = true;
		this.comboBox4.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox4.Location = new System.Drawing.Point(611, 164);
		this.comboBox4.Name = "comboBox4";
		this.comboBox4.Size = new System.Drawing.Size(69, 21);
		this.comboBox4.TabIndex = 25;
		this.comboBox4.Tag = "U01";
		this.comboBox4.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown4.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown4.Location = new System.Drawing.Point(234, 164);
		this.numericUpDown4.Name = "numericUpDown4";
		this.numericUpDown4.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown4.TabIndex = 24;
		this.numericUpDown4.Tag = "O1";
		this.numericUpDown4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown4.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown5.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown5.Location = new System.Drawing.Point(550, 165);
		this.numericUpDown5.Name = "numericUpDown5";
		this.numericUpDown5.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown5.TabIndex = 23;
		this.numericUpDown5.Tag = "S1";
		this.numericUpDown5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown5.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown6.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown6.Location = new System.Drawing.Point(360, 165);
		this.numericUpDown6.Name = "numericUpDown6";
		this.numericUpDown6.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown6.TabIndex = 22;
		this.numericUpDown6.Tag = "R1";
		this.numericUpDown6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown6.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox1.Location = new System.Drawing.Point(685, 138);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(69, 21);
		this.comboBox1.TabIndex = 21;
		this.comboBox1.Tag = "N00";
		this.comboBox1.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.comboBox2.FormattingEnabled = true;
		this.comboBox2.Items.AddRange(new object[11]
		{
			"16.00", "16.30", "17.00", "17.30", "18.00", "18.30", "19.00", "19.30", "20.00", "20.30",
			"21.00"
		});
		this.comboBox2.Location = new System.Drawing.Point(612, 138);
		this.comboBox2.Name = "comboBox2";
		this.comboBox2.Size = new System.Drawing.Size(69, 21);
		this.comboBox2.TabIndex = 20;
		this.comboBox2.Tag = "U00";
		this.comboBox2.SelectedIndexChanged += new System.EventHandler(TimeComboBox_SelectedIndexChanged);
		this.numericUpDown1.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown1.Location = new System.Drawing.Point(234, 139);
		this.numericUpDown1.Name = "numericUpDown1";
		this.numericUpDown1.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown1.TabIndex = 19;
		this.numericUpDown1.Tag = "O0";
		this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown1.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown2.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown2.Location = new System.Drawing.Point(550, 139);
		this.numericUpDown2.Name = "numericUpDown2";
		this.numericUpDown2.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown2.TabIndex = 18;
		this.numericUpDown2.Tag = "S0";
		this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown2.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.numericUpDown3.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDown3.Location = new System.Drawing.Point(360, 139);
		this.numericUpDown3.Name = "numericUpDown3";
		this.numericUpDown3.Size = new System.Drawing.Size(56, 20);
		this.numericUpDown3.TabIndex = 17;
		this.numericUpDown3.Tag = "R0";
		this.numericUpDown3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown3.ValueChanged += new System.EventHandler(probUpDown_ValueChanged);
		this.label28.AutoSize = true;
		this.label28.Location = new System.Drawing.Point(6, 438);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(29, 13);
		this.label28.TabIndex = 11;
		this.label28.Text = "DEC";
		this.label27.AutoSize = true;
		this.label27.Location = new System.Drawing.Point(7, 411);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(30, 13);
		this.label27.TabIndex = 10;
		this.label27.Text = "NOV";
		this.label26.AutoSize = true;
		this.label26.Location = new System.Drawing.Point(7, 384);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(29, 13);
		this.label26.TabIndex = 9;
		this.label26.Text = "OCT";
		this.label25.AutoSize = true;
		this.label25.Location = new System.Drawing.Point(7, 357);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(28, 13);
		this.label25.TabIndex = 8;
		this.label25.Text = "SEP";
		this.label24.AutoSize = true;
		this.label24.Location = new System.Drawing.Point(7, 330);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(30, 13);
		this.label24.TabIndex = 7;
		this.label24.Text = "AUG";
		this.label23.AutoSize = true;
		this.label23.Location = new System.Drawing.Point(7, 303);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(26, 13);
		this.label23.TabIndex = 6;
		this.label23.Text = "JUL";
		this.label22.AutoSize = true;
		this.label22.Location = new System.Drawing.Point(7, 276);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(28, 13);
		this.label22.TabIndex = 5;
		this.label22.Text = "JUN";
		this.label21.AutoSize = true;
		this.label21.Location = new System.Drawing.Point(7, 249);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(30, 13);
		this.label21.TabIndex = 4;
		this.label21.Text = "MAY";
		this.label20.AutoSize = true;
		this.label20.Location = new System.Drawing.Point(7, 222);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(29, 13);
		this.label20.TabIndex = 3;
		this.label20.Text = "APR";
		this.label19.AutoSize = true;
		this.label19.Location = new System.Drawing.Point(7, 195);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(31, 13);
		this.label19.TabIndex = 2;
		this.label19.Text = "MAR";
		this.label18.AutoSize = true;
		this.label18.Location = new System.Drawing.Point(7, 168);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(27, 13);
		this.label18.TabIndex = 1;
		this.label18.Text = "FEB";
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(7, 141);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(27, 13);
		this.label17.TabIndex = 0;
		this.label17.Text = "JAN";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(12, 83);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(77, 13);
		this.label6.TabIndex = 11;
		this.label6.Text = "Yellows Stored";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(12, 53);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(101, 13);
		this.label5.TabIndex = 10;
		this.label5.Text = "Season Start Month";
		this.labelDatabaseCountry.AutoSize = true;
		this.labelDatabaseCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelDatabaseCountry.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelDatabaseCountry.Location = new System.Drawing.Point(12, 25);
		this.labelDatabaseCountry.Name = "labelDatabaseCountry";
		this.labelDatabaseCountry.Size = new System.Drawing.Size(92, 13);
		this.labelDatabaseCountry.TabIndex = 9;
		this.labelDatabaseCountry.Text = "Database Country";
		this.labelDatabaseCountry.DoubleClick += new System.EventHandler(labelDatabaseCountry_DoubleClick);
		this.comboCountry.FormattingEnabled = true;
		this.comboCountry.Location = new System.Drawing.Point(153, 22);
		this.comboCountry.Name = "comboCountry";
		this.comboCountry.Size = new System.Drawing.Size(162, 21);
		this.comboCountry.TabIndex = 8;
		this.comboCountry.SelectedIndexChanged += new System.EventHandler(comboCountry_SelectedIndexChanged);
		this.comboNationStandingsRules.FormattingEnabled = true;
		this.comboNationStandingsRules.Items.AddRange(new object[6] { "Points, Goals, Wins", "Points. Wins, Goals", "Points, Head To Head, Goals", "Team Rating", "Previous Ranking", "Points, Goals, Head To Head" });
		this.comboNationStandingsRules.Location = new System.Drawing.Point(153, 105);
		this.comboNationStandingsRules.Name = "comboNationStandingsRules";
		this.comboNationStandingsRules.Size = new System.Drawing.Size(162, 21);
		this.comboNationStandingsRules.TabIndex = 6;
		this.comboNationStandingsRules.SelectedIndexChanged += new System.EventHandler(comboNationStandingsRules_SelectedIndexChanged);
		this.checkNationStandingsRules.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkNationStandingsRules.Location = new System.Drawing.Point(15, 105);
		this.checkNationStandingsRules.Name = "checkNationStandingsRules";
		this.checkNationStandingsRules.Size = new System.Drawing.Size(131, 23);
		this.checkNationStandingsRules.TabIndex = 5;
		this.checkNationStandingsRules.Text = "Special Standing Rules";
		this.toolTip.SetToolTip(this.checkNationStandingsRules, "Default value is: Points, Goals");
		this.checkNationStandingsRules.UseVisualStyleBackColor = true;
		this.checkNationStandingsRules.CheckedChanged += new System.EventHandler(checkNationStandingsRules_CheckedChanged);
		this.numericNationYellowsStored.Location = new System.Drawing.Point(153, 79);
		this.numericNationYellowsStored.Maximum = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericNationYellowsStored.Minimum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericNationYellowsStored.Name = "numericNationYellowsStored";
		this.numericNationYellowsStored.Size = new System.Drawing.Size(86, 20);
		this.numericNationYellowsStored.TabIndex = 4;
		this.numericNationYellowsStored.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNationYellowsStored.Value = new decimal(new int[4] { 2, 0, 0, 0 });
		this.numericNationYellowsStored.ValueChanged += new System.EventHandler(numericYellowsStored_ValueChanged);
		this.comboNationStartMonth.FormattingEnabled = true;
		this.comboNationStartMonth.Items.AddRange(new object[12]
		{
			"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT",
			"NOV", "DEC"
		});
		this.comboNationStartMonth.Location = new System.Drawing.Point(153, 50);
		this.comboNationStartMonth.Name = "comboNationStartMonth";
		this.comboNationStartMonth.Size = new System.Drawing.Size(90, 21);
		this.comboNationStartMonth.TabIndex = 2;
		this.comboNationStartMonth.SelectedIndexChanged += new System.EventHandler(comboNationStartMonth_SelectedIndexChanged);
		this.groupTrophy.Controls.Add(this.numericAdvanceFrom);
		this.groupTrophy.Controls.Add(this.checkAdvanceFrom);
		this.groupTrophy.Controls.Add(this.checkLowCelebrationLevel);
		this.groupTrophy.Controls.Add(this.groupInternationalschedule);
		this.groupTrophy.Controls.Add(this.label67);
		this.groupTrophy.Controls.Add(this.numericBall);
		this.groupTrophy.Controls.Add(this.pictureBall);
		this.groupTrophy.Controls.Add(this.groupBenchPlayers);
		this.groupTrophy.Controls.Add(this.comboTrophyStandingRules);
		this.groupTrophy.Controls.Add(this.labelTrophyShortName);
		this.groupTrophy.Controls.Add(this.labelMatchImportance);
		this.groupTrophy.Controls.Add(this.labelCompetitionType);
		this.groupTrophy.Controls.Add(this.numericImportance);
		this.groupTrophy.Controls.Add(this.labelAssetId);
		this.groupTrophy.Controls.Add(this.comboCompetitionType);
		this.groupTrophy.Controls.Add(this.checkTrophyStandingsRules);
		this.groupTrophy.Controls.Add(this.buttonGetId);
		this.groupTrophy.Controls.Add(this.groupPromotionRelegation);
		this.groupTrophy.Controls.Add(this.numericAssetId);
		this.groupTrophy.Controls.Add(this.groupSchedule);
		this.groupTrophy.Controls.Add(this.textTrophyLongName);
		this.groupTrophy.Controls.Add(this.labeTrophylLongName);
		this.groupTrophy.Controls.Add(this.textTrophyShortName);
		this.groupTrophy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupTrophy.Location = new System.Drawing.Point(3, 3);
		this.groupTrophy.Name = "groupTrophy";
		this.groupTrophy.Size = new System.Drawing.Size(532, 623);
		this.groupTrophy.TabIndex = 9;
		this.groupTrophy.TabStop = false;
		this.groupTrophy.Text = "Trophy";
		this.groupTrophy.Visible = false;
		this.numericAdvanceFrom.BackColor = System.Drawing.Color.Yellow;
		this.numericAdvanceFrom.Location = new System.Drawing.Point(162, 585);
		this.numericAdvanceFrom.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.numericAdvanceFrom.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericAdvanceFrom.Name = "numericAdvanceFrom";
		this.numericAdvanceFrom.Size = new System.Drawing.Size(83, 20);
		this.numericAdvanceFrom.TabIndex = 179;
		this.numericAdvanceFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.toolTip.SetToolTip(this.numericAdvanceFrom, "Set this value to the id of the competition from which teams are coming");
		this.numericAdvanceFrom.ValueChanged += new System.EventHandler(numericAdvanceFrom_ValueChanged);
		this.checkAdvanceFrom.AutoSize = true;
		this.checkAdvanceFrom.Location = new System.Drawing.Point(16, 587);
		this.checkAdvanceFrom.Name = "checkAdvanceFrom";
		this.checkAdvanceFrom.Size = new System.Drawing.Size(95, 17);
		this.checkAdvanceFrom.TabIndex = 178;
		this.checkAdvanceFrom.Text = "Advance From";
		this.checkAdvanceFrom.UseVisualStyleBackColor = true;
		this.checkAdvanceFrom.CheckedChanged += new System.EventHandler(checkAdvanceFrom_CheckedChanged);
		this.checkLowCelebrationLevel.AutoSize = true;
		this.checkLowCelebrationLevel.Location = new System.Drawing.Point(15, 162);
		this.checkLowCelebrationLevel.Name = "checkLowCelebrationLevel";
		this.checkLowCelebrationLevel.Size = new System.Drawing.Size(131, 17);
		this.checkLowCelebrationLevel.TabIndex = 177;
		this.checkLowCelebrationLevel.Text = "Low Celebration Level";
		this.toolTip.SetToolTip(this.checkLowCelebrationLevel, "Check if the celebration level for a victory will be low");
		this.checkLowCelebrationLevel.UseVisualStyleBackColor = true;
		this.checkLowCelebrationLevel.CheckedChanged += new System.EventHandler(checkLowCelebrationLevel_CheckedChanged);
		this.groupInternationalschedule.Controls.Add(this.label71);
		this.groupInternationalschedule.Controls.Add(this.comboTrophyStartMonth);
		this.groupInternationalschedule.Controls.Add(this.numericInternationalPeriodicity);
		this.groupInternationalschedule.Controls.Add(this.label69);
		this.groupInternationalschedule.Controls.Add(this.label68);
		this.groupInternationalschedule.Controls.Add(this.numericInternationalFirstYear);
		this.groupInternationalschedule.Location = new System.Drawing.Point(9, 487);
		this.groupInternationalschedule.Name = "groupInternationalschedule";
		this.groupInternationalschedule.Size = new System.Drawing.Size(347, 90);
		this.groupInternationalschedule.TabIndex = 167;
		this.groupInternationalschedule.TabStop = false;
		this.groupInternationalschedule.Text = "International Schedule";
		this.label71.AutoSize = true;
		this.label71.Location = new System.Drawing.Point(6, 66);
		this.label71.Name = "label71";
		this.label71.Size = new System.Drawing.Size(62, 13);
		this.label71.TabIndex = 163;
		this.label71.Text = "Start Month";
		this.comboTrophyStartMonth.FormattingEnabled = true;
		this.comboTrophyStartMonth.Items.AddRange(new object[12]
		{
			"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT",
			"NOV", "DEC"
		});
		this.comboTrophyStartMonth.Location = new System.Drawing.Point(79, 63);
		this.comboTrophyStartMonth.Name = "comboTrophyStartMonth";
		this.comboTrophyStartMonth.Size = new System.Drawing.Size(90, 21);
		this.comboTrophyStartMonth.TabIndex = 162;
		this.comboTrophyStartMonth.SelectedIndexChanged += new System.EventHandler(comboTrophyStartMonth_SelectedIndexChanged);
		this.numericInternationalPeriodicity.Location = new System.Drawing.Point(79, 41);
		this.numericInternationalPeriodicity.Maximum = new decimal(new int[4] { 4, 0, 0, 0 });
		this.numericInternationalPeriodicity.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericInternationalPeriodicity.Name = "numericInternationalPeriodicity";
		this.numericInternationalPeriodicity.Size = new System.Drawing.Size(90, 20);
		this.numericInternationalPeriodicity.TabIndex = 161;
		this.numericInternationalPeriodicity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericInternationalPeriodicity.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericInternationalPeriodicity.ValueChanged += new System.EventHandler(numericInternationalPeriodicity_ValueChanged);
		this.label69.AutoSize = true;
		this.label69.Location = new System.Drawing.Point(6, 43);
		this.label69.Name = "label69";
		this.label69.Size = new System.Drawing.Size(55, 13);
		this.label69.TabIndex = 160;
		this.label69.Text = "Periodicity";
		this.toolTip.SetToolTip(this.label69, "Set a number between 0 and 100");
		this.label68.AutoSize = true;
		this.label68.Location = new System.Drawing.Point(6, 20);
		this.label68.Name = "label68";
		this.label68.Size = new System.Drawing.Size(51, 13);
		this.label68.TabIndex = 158;
		this.label68.Text = "First Year";
		this.toolTip.SetToolTip(this.label68, "Set a number between 0 and 100");
		this.numericInternationalFirstYear.Location = new System.Drawing.Point(79, 18);
		this.numericInternationalFirstYear.Maximum = new decimal(new int[4] { 2100, 0, 0, 0 });
		this.numericInternationalFirstYear.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericInternationalFirstYear.Name = "numericInternationalFirstYear";
		this.numericInternationalFirstYear.Size = new System.Drawing.Size(90, 20);
		this.numericInternationalFirstYear.TabIndex = 159;
		this.numericInternationalFirstYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericInternationalFirstYear.Value = new decimal(new int[4] { 2023, 0, 0, 0 });
		this.numericInternationalFirstYear.ValueChanged += new System.EventHandler(numericInternationalFirstYear_ValueChanged);
		this.label67.Cursor = System.Windows.Forms.Cursors.Default;
		this.label67.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label67.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label67.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label67.Location = new System.Drawing.Point(367, 22);
		this.label67.Name = "label67";
		this.label67.Size = new System.Drawing.Size(51, 20);
		this.label67.TabIndex = 166;
		this.label67.Text = "Ball";
		this.label67.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericBall.Location = new System.Drawing.Point(424, 21);
		this.numericBall.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericBall.Name = "numericBall";
		this.numericBall.Size = new System.Drawing.Size(91, 20);
		this.numericBall.TabIndex = 165;
		this.numericBall.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBall.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBall.ValueChanged += new System.EventHandler(numericBall_ValueChanged);
		this.pictureBall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBall.Location = new System.Drawing.Point(363, 46);
		this.pictureBall.Name = "pictureBall";
		this.pictureBall.Size = new System.Drawing.Size(152, 110);
		this.pictureBall.TabIndex = 164;
		this.pictureBall.TabStop = false;
		this.groupBenchPlayers.Controls.Add(this.radioBench7Players);
		this.groupBenchPlayers.Controls.Add(this.radioBench5Players);
		this.groupBenchPlayers.Location = new System.Drawing.Point(7, 431);
		this.groupBenchPlayers.Name = "groupBenchPlayers";
		this.groupBenchPlayers.Size = new System.Drawing.Size(349, 50);
		this.groupBenchPlayers.TabIndex = 161;
		this.groupBenchPlayers.TabStop = false;
		this.groupBenchPlayers.Text = "Bench Players";
		this.radioBench7Players.AutoSize = true;
		this.radioBench7Players.Location = new System.Drawing.Point(110, 19);
		this.radioBench7Players.Name = "radioBench7Players";
		this.radioBench7Players.Size = new System.Drawing.Size(68, 17);
		this.radioBench7Players.TabIndex = 1;
		this.radioBench7Players.TabStop = true;
		this.radioBench7Players.Text = "7 Players";
		this.radioBench7Players.UseVisualStyleBackColor = true;
		this.radioBench7Players.CheckedChanged += new System.EventHandler(radioBench7Players_CheckedChanged);
		this.radioBench5Players.AutoSize = true;
		this.radioBench5Players.Location = new System.Drawing.Point(9, 19);
		this.radioBench5Players.Name = "radioBench5Players";
		this.radioBench5Players.Size = new System.Drawing.Size(68, 17);
		this.radioBench5Players.TabIndex = 0;
		this.radioBench5Players.TabStop = true;
		this.radioBench5Players.Text = "5 Players";
		this.radioBench5Players.UseVisualStyleBackColor = true;
		this.radioBench5Players.CheckedChanged += new System.EventHandler(radioBench5Players_CheckedChanged);
		this.comboTrophyStandingRules.FormattingEnabled = true;
		this.comboTrophyStandingRules.Items.AddRange(new object[6] { "Points, Goals, Wins", "Points. Wins, Goals", "Points, Head To Head, Goals", "Team Rating", "Previous Ranking", "Points, Goals, Head To Head" });
		this.comboTrophyStandingRules.Location = new System.Drawing.Point(162, 190);
		this.comboTrophyStandingRules.Name = "comboTrophyStandingRules";
		this.comboTrophyStandingRules.Size = new System.Drawing.Size(185, 21);
		this.comboTrophyStandingRules.TabIndex = 160;
		this.comboTrophyStandingRules.SelectedIndexChanged += new System.EventHandler(comboTrophyStandingRules_SelectedIndexChanged);
		this.labelTrophyShortName.AutoSize = true;
		this.labelTrophyShortName.Location = new System.Drawing.Point(15, 30);
		this.labelTrophyShortName.Name = "labelTrophyShortName";
		this.labelTrophyShortName.Size = new System.Drawing.Size(63, 13);
		this.labelTrophyShortName.TabIndex = 22;
		this.labelTrophyShortName.Text = "Short Name";
		this.labelMatchImportance.AutoSize = true;
		this.labelMatchImportance.Location = new System.Drawing.Point(15, 133);
		this.labelMatchImportance.Name = "labelMatchImportance";
		this.labelMatchImportance.Size = new System.Drawing.Size(93, 13);
		this.labelMatchImportance.TabIndex = 14;
		this.labelMatchImportance.Text = "Match Importance";
		this.toolTip.SetToolTip(this.labelMatchImportance, "Set a number between 0 and 100");
		this.labelCompetitionType.AutoSize = true;
		this.labelCompetitionType.Location = new System.Drawing.Point(16, 107);
		this.labelCompetitionType.Name = "labelCompetitionType";
		this.labelCompetitionType.Size = new System.Drawing.Size(89, 13);
		this.labelCompetitionType.TabIndex = 10;
		this.labelCompetitionType.Text = "Competition Type";
		this.numericImportance.Location = new System.Drawing.Point(164, 131);
		this.numericImportance.Name = "numericImportance";
		this.numericImportance.Size = new System.Drawing.Size(68, 20);
		this.numericImportance.TabIndex = 157;
		this.numericImportance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericImportance.ValueChanged += new System.EventHandler(numericImportance_ValueChanged);
		this.labelAssetId.AutoSize = true;
		this.labelAssetId.Location = new System.Drawing.Point(16, 85);
		this.labelAssetId.Name = "labelAssetId";
		this.labelAssetId.Size = new System.Drawing.Size(45, 13);
		this.labelAssetId.TabIndex = 12;
		this.labelAssetId.Text = "Asset Id";
		this.comboCompetitionType.FormattingEnabled = true;
		this.comboCompetitionType.Items.AddRange(new object[8] { "FRIENDLY", "LEAGUE", "PLAYOFF", "CUP", "SUPERCUP", "INTERCUP", "INTERQUAL", "INTERFRIENDLY" });
		this.comboCompetitionType.Location = new System.Drawing.Point(164, 104);
		this.comboCompetitionType.Name = "comboCompetitionType";
		this.comboCompetitionType.Size = new System.Drawing.Size(185, 21);
		this.comboCompetitionType.TabIndex = 156;
		this.comboCompetitionType.SelectedIndexChanged += new System.EventHandler(comboCompetitionType_SelectedIndexChanged);
		this.checkTrophyStandingsRules.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkTrophyStandingsRules.Location = new System.Drawing.Point(13, 190);
		this.checkTrophyStandingsRules.Name = "checkTrophyStandingsRules";
		this.checkTrophyStandingsRules.Size = new System.Drawing.Size(136, 23);
		this.checkTrophyStandingsRules.TabIndex = 15;
		this.checkTrophyStandingsRules.Text = "Special Standing Rules";
		this.toolTip.SetToolTip(this.checkTrophyStandingsRules, "By default use the value defined by the Nation");
		this.checkTrophyStandingsRules.UseVisualStyleBackColor = true;
		this.checkTrophyStandingsRules.CheckedChanged += new System.EventHandler(checkTrophyStandingsRules_CheckedChanged);
		this.buttonGetId.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonGetId.BackgroundImage");
		this.buttonGetId.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.buttonGetId.Location = new System.Drawing.Point(324, 76);
		this.buttonGetId.Name = "buttonGetId";
		this.buttonGetId.Size = new System.Drawing.Size(25, 23);
		this.buttonGetId.TabIndex = 155;
		this.buttonGetId.UseVisualStyleBackColor = true;
		this.buttonGetId.Click += new System.EventHandler(buttonGetId_Click);
		this.groupPromotionRelegation.Controls.Add(this.comboRelegationLeague);
		this.groupPromotionRelegation.Controls.Add(this.comboPromotionLeague);
		this.groupPromotionRelegation.Controls.Add(this.checkPromotionLeague);
		this.groupPromotionRelegation.Controls.Add(this.checkRelegationLeague);
		this.groupPromotionRelegation.Location = new System.Drawing.Point(7, 334);
		this.groupPromotionRelegation.Name = "groupPromotionRelegation";
		this.groupPromotionRelegation.Size = new System.Drawing.Size(349, 91);
		this.groupPromotionRelegation.TabIndex = 20;
		this.groupPromotionRelegation.TabStop = false;
		this.groupPromotionRelegation.Text = "Promotions and Relegations";
		this.comboRelegationLeague.FormattingEnabled = true;
		this.comboRelegationLeague.Location = new System.Drawing.Point(155, 51);
		this.comboRelegationLeague.Name = "comboRelegationLeague";
		this.comboRelegationLeague.Size = new System.Drawing.Size(185, 21);
		this.comboRelegationLeague.TabIndex = 19;
		this.comboRelegationLeague.SelectedIndexChanged += new System.EventHandler(comboRelegationLeague_SelectedIndexChanged);
		this.comboPromotionLeague.FormattingEnabled = true;
		this.comboPromotionLeague.Location = new System.Drawing.Point(155, 20);
		this.comboPromotionLeague.Name = "comboPromotionLeague";
		this.comboPromotionLeague.Size = new System.Drawing.Size(185, 21);
		this.comboPromotionLeague.TabIndex = 18;
		this.comboPromotionLeague.SelectedIndexChanged += new System.EventHandler(comboPromotionLeague_SelectedIndexChanged);
		this.checkPromotionLeague.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkPromotionLeague.Location = new System.Drawing.Point(6, 20);
		this.checkPromotionLeague.Name = "checkPromotionLeague";
		this.checkPromotionLeague.Size = new System.Drawing.Size(139, 23);
		this.checkPromotionLeague.TabIndex = 16;
		this.checkPromotionLeague.Text = "Promote To";
		this.checkPromotionLeague.UseVisualStyleBackColor = true;
		this.checkPromotionLeague.CheckedChanged += new System.EventHandler(checkPromotionLeague_CheckedChanged);
		this.checkRelegationLeague.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkRelegationLeague.Location = new System.Drawing.Point(6, 52);
		this.checkRelegationLeague.Name = "checkRelegationLeague";
		this.checkRelegationLeague.Size = new System.Drawing.Size(139, 23);
		this.checkRelegationLeague.TabIndex = 17;
		this.checkRelegationLeague.Text = "Relegate To";
		this.checkRelegationLeague.UseVisualStyleBackColor = true;
		this.checkRelegationLeague.CheckedChanged += new System.EventHandler(checkRelegationLeague_CheckedChanged);
		this.numericAssetId.Location = new System.Drawing.Point(164, 76);
		this.numericAssetId.Maximum = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericAssetId.Name = "numericAssetId";
		this.numericAssetId.Size = new System.Drawing.Size(146, 20);
		this.numericAssetId.TabIndex = 154;
		this.numericAssetId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericAssetId.ValueChanged += new System.EventHandler(numericAssetId_ValueChanged);
		this.groupSchedule.Controls.Add(this.checkScheduleUseDates);
		this.groupSchedule.Controls.Add(this.checkScheduleConflicts);
		this.groupSchedule.Controls.Add(this.comboSchedForce);
		this.groupSchedule.Controls.Add(this.checkForceSchedule);
		this.groupSchedule.Location = new System.Drawing.Point(7, 219);
		this.groupSchedule.Name = "groupSchedule";
		this.groupSchedule.Size = new System.Drawing.Size(349, 108);
		this.groupSchedule.TabIndex = 21;
		this.groupSchedule.TabStop = false;
		this.groupSchedule.Text = "Schedule";
		this.checkScheduleUseDates.AutoSize = true;
		this.checkScheduleUseDates.Location = new System.Drawing.Point(10, 80);
		this.checkScheduleUseDates.Name = "checkScheduleUseDates";
		this.checkScheduleUseDates.Size = new System.Drawing.Size(176, 17);
		this.checkScheduleUseDates.TabIndex = 162;
		this.checkScheduleUseDates.Text = "Use International Friendly Dates";
		this.toolTip.SetToolTip(this.checkScheduleUseDates, "Check this if you want to use the schedule dates of International friendlies");
		this.checkScheduleUseDates.UseVisualStyleBackColor = true;
		this.checkScheduleUseDates.CheckedChanged += new System.EventHandler(checkScheduleUseDates_CheckedChanged);
		this.checkScheduleConflicts.AutoSize = true;
		this.checkScheduleConflicts.Location = new System.Drawing.Point(9, 19);
		this.checkScheduleConflicts.Name = "checkScheduleConflicts";
		this.checkScheduleConflicts.Size = new System.Drawing.Size(148, 17);
		this.checkScheduleConflicts.TabIndex = 161;
		this.checkScheduleConflicts.Text = "Check Schedule Conflicts";
		this.toolTip.SetToolTip(this.checkScheduleConflicts, "Check this box for international competitions");
		this.checkScheduleConflicts.UseVisualStyleBackColor = true;
		this.checkScheduleConflicts.CheckedChanged += new System.EventHandler(checkScheduleConflicts_CheckedChanged);
		this.comboSchedForce.FormattingEnabled = true;
		this.comboSchedForce.Location = new System.Drawing.Point(153, 45);
		this.comboSchedForce.Name = "comboSchedForce";
		this.comboSchedForce.Size = new System.Drawing.Size(185, 21);
		this.comboSchedForce.TabIndex = 22;
		this.comboSchedForce.SelectedIndexChanged += new System.EventHandler(comboSchedForce_SelectedIndexChanged);
		this.checkForceSchedule.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkForceSchedule.Location = new System.Drawing.Point(4, 43);
		this.checkForceSchedule.Name = "checkForceSchedule";
		this.checkForceSchedule.Size = new System.Drawing.Size(136, 23);
		this.checkForceSchedule.TabIndex = 19;
		this.checkForceSchedule.Text = "Force Schedule of";
		this.toolTip.SetToolTip(this.checkForceSchedule, "Select a competition that must be scheduled after the completion of this trophy");
		this.checkForceSchedule.UseVisualStyleBackColor = true;
		this.checkForceSchedule.CheckedChanged += new System.EventHandler(checkForceSchedule_CheckedChanged);
		this.textTrophyLongName.Location = new System.Drawing.Point(164, 50);
		this.textTrophyLongName.Name = "textTrophyLongName";
		this.textTrophyLongName.Size = new System.Drawing.Size(185, 20);
		this.textTrophyLongName.TabIndex = 25;
		this.textTrophyLongName.TextChanged += new System.EventHandler(textTrophyLongName_TextChanged);
		this.labeTrophylLongName.AutoSize = true;
		this.labeTrophylLongName.Location = new System.Drawing.Point(16, 57);
		this.labeTrophylLongName.Name = "labeTrophylLongName";
		this.labeTrophylLongName.Size = new System.Drawing.Size(62, 13);
		this.labeTrophylLongName.TabIndex = 23;
		this.labeTrophylLongName.Text = "Long Name";
		this.textTrophyShortName.Location = new System.Drawing.Point(164, 23);
		this.textTrophyShortName.Name = "textTrophyShortName";
		this.textTrophyShortName.Size = new System.Drawing.Size(185, 20);
		this.textTrophyShortName.TabIndex = 24;
		this.textTrophyShortName.TextChanged += new System.EventHandler(textTrophyShortName_TextChanged);
		this.groupStage.Controls.Add(this.groupPlayStage);
		this.groupStage.Controls.Add(this.groupSetupStage);
		this.groupStage.Controls.Add(this.comboStageStandingRules);
		this.groupStage.Controls.Add(this.checkStageStandingsRules);
		this.groupStage.Controls.Add(this.numericStandingsRank);
		this.groupStage.Controls.Add(this.checkStandingsRank);
		this.groupStage.Controls.Add(this.comboStageType);
		this.groupStage.Controls.Add(this.label7);
		this.groupStage.Controls.Add(this.numericStandingKeep);
		this.groupStage.Controls.Add(this.checkStandingKeep);
		this.groupStage.Location = new System.Drawing.Point(0, 0);
		this.groupStage.Name = "groupStage";
		this.groupStage.Size = new System.Drawing.Size(790, 724);
		this.groupStage.TabIndex = 10;
		this.groupStage.TabStop = false;
		this.groupStage.Text = "Stage";
		this.groupStage.Visible = false;
		this.groupPlayStage.Controls.Add(this.checkCanUseFancards);
		this.groupPlayStage.Controls.Add(this.numericKeepPointsStageRef);
		this.groupPlayStage.Controls.Add(this.checkRandomDrawEvent);
		this.groupPlayStage.Controls.Add(this.groupLeaguetasks);
		this.groupPlayStage.Controls.Add(this.groupStageSchedules);
		this.groupPlayStage.Controls.Add(this.numericRegularSeason);
		this.groupPlayStage.Controls.Add(this.comboSpecialKo2Rule);
		this.groupPlayStage.Controls.Add(this.checkSpecialKo2Rule);
		this.groupPlayStage.Controls.Add(this.comboSpecialKo1Rule);
		this.groupPlayStage.Controls.Add(this.checkSpecialKo1Rule);
		this.groupPlayStage.Controls.Add(this.numericKeepPointsPercentage);
		this.groupPlayStage.Controls.Add(this.checkKeepPointsPercentage);
		this.groupPlayStage.Controls.Add(this.numericStageRef);
		this.groupPlayStage.Controls.Add(this.checkClausuraSchedule);
		this.groupPlayStage.Controls.Add(this.groupStadiums);
		this.groupPlayStage.Controls.Add(this.checkMaxteamsgroup);
		this.groupPlayStage.Controls.Add(this.checkMatchReplay);
		this.groupPlayStage.Controls.Add(this.numericMoneyDrop);
		this.groupPlayStage.Controls.Add(this.checkMaxteamsassoc);
		this.groupPlayStage.Controls.Add(this.label10);
		this.groupPlayStage.Controls.Add(this.numericPrizeMoney);
		this.groupPlayStage.Controls.Add(this.label9);
		this.groupPlayStage.Controls.Add(this.comboMatchSituation);
		this.groupPlayStage.Controls.Add(this.label8);
		this.groupPlayStage.Location = new System.Drawing.Point(8, 75);
		this.groupPlayStage.Name = "groupPlayStage";
		this.groupPlayStage.Size = new System.Drawing.Size(776, 643);
		this.groupPlayStage.TabIndex = 18;
		this.groupPlayStage.TabStop = false;
		this.groupPlayStage.Text = "Play Stage";
		this.checkCanUseFancards.AutoSize = true;
		this.checkCanUseFancards.Location = new System.Drawing.Point(9, 126);
		this.checkCanUseFancards.Name = "checkCanUseFancards";
		this.checkCanUseFancards.Size = new System.Drawing.Size(96, 17);
		this.checkCanUseFancards.TabIndex = 175;
		this.checkCanUseFancards.Text = "Use Fan Cards";
		this.toolTip.SetToolTip(this.checkCanUseFancards, "Check if the usage of fan cards is allowed");
		this.checkCanUseFancards.UseVisualStyleBackColor = true;
		this.checkCanUseFancards.CheckedChanged += new System.EventHandler(checkCanUseFancards_CheckedChanged);
		this.numericKeepPointsStageRef.BackColor = System.Drawing.Color.Yellow;
		this.numericKeepPointsStageRef.Location = new System.Drawing.Point(123, 335);
		this.numericKeepPointsStageRef.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.numericKeepPointsStageRef.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericKeepPointsStageRef.Name = "numericKeepPointsStageRef";
		this.numericKeepPointsStageRef.Size = new System.Drawing.Size(83, 20);
		this.numericKeepPointsStageRef.TabIndex = 174;
		this.numericKeepPointsStageRef.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.toolTip.SetToolTip(this.numericKeepPointsStageRef, "Set this value to the id of the stage from where to keep points");
		this.numericKeepPointsStageRef.ValueChanged += new System.EventHandler(numericKeepPointsStageRef_ValueChanged);
		this.checkRandomDrawEvent.AutoSize = true;
		this.checkRandomDrawEvent.Location = new System.Drawing.Point(6, 287);
		this.checkRandomDrawEvent.Name = "checkRandomDrawEvent";
		this.checkRandomDrawEvent.Size = new System.Drawing.Size(125, 17);
		this.checkRandomDrawEvent.TabIndex = 173;
		this.checkRandomDrawEvent.Text = "Random Draw Event";
		this.checkRandomDrawEvent.UseVisualStyleBackColor = true;
		this.checkRandomDrawEvent.CheckedChanged += new System.EventHandler(checkRandomDrawEvent_CheckedChanged);
		this.groupLeaguetasks.Controls.Add(this.checkUpdateLeagueTable);
		this.groupLeaguetasks.Controls.Add(this.comboLeagueStats);
		this.groupLeaguetasks.Controls.Add(this.checkUpdateLeagueStats);
		this.groupLeaguetasks.Controls.Add(this.checkClearLeagueStats);
		this.groupLeaguetasks.Location = new System.Drawing.Point(3, 431);
		this.groupLeaguetasks.Name = "groupLeaguetasks";
		this.groupLeaguetasks.Size = new System.Drawing.Size(358, 125);
		this.groupLeaguetasks.TabIndex = 172;
		this.groupLeaguetasks.TabStop = false;
		this.groupLeaguetasks.Text = "League Actions";
		this.checkUpdateLeagueTable.Location = new System.Drawing.Point(6, 96);
		this.checkUpdateLeagueTable.Name = "checkUpdateLeagueTable";
		this.checkUpdateLeagueTable.Size = new System.Drawing.Size(139, 23);
		this.checkUpdateLeagueTable.TabIndex = 172;
		this.checkUpdateLeagueTable.Text = "Update League Table";
		this.toolTip.SetToolTip(this.checkUpdateLeagueTable, "Check this only for normal league tournaments");
		this.checkUpdateLeagueTable.UseVisualStyleBackColor = true;
		this.checkUpdateLeagueTable.Visible = false;
		this.checkUpdateLeagueTable.CheckedChanged += new System.EventHandler(checkUpdateLeagueTable_CheckedChanged);
		this.comboLeagueStats.FormattingEnabled = true;
		this.comboLeagueStats.Location = new System.Drawing.Point(6, 19);
		this.comboLeagueStats.Name = "comboLeagueStats";
		this.comboLeagueStats.Size = new System.Drawing.Size(346, 21);
		this.comboLeagueStats.TabIndex = 170;
		this.comboLeagueStats.SelectedIndexChanged += new System.EventHandler(comboLeagueStats_SelectedIndexChanged);
		this.checkUpdateLeagueStats.Location = new System.Drawing.Point(6, 71);
		this.checkUpdateLeagueStats.Name = "checkUpdateLeagueStats";
		this.checkUpdateLeagueStats.Size = new System.Drawing.Size(139, 23);
		this.checkUpdateLeagueStats.TabIndex = 171;
		this.checkUpdateLeagueStats.Text = "Update League Stats";
		this.toolTip.SetToolTip(this.checkUpdateLeagueStats, "Check this for \"Aperture\" and \"Clausura\" tournaments.");
		this.checkUpdateLeagueStats.UseVisualStyleBackColor = true;
		this.checkUpdateLeagueStats.CheckedChanged += new System.EventHandler(checkUpdateLeagueStats_CheckedChanged);
		this.checkClearLeagueStats.Location = new System.Drawing.Point(6, 46);
		this.checkClearLeagueStats.Name = "checkClearLeagueStats";
		this.checkClearLeagueStats.Size = new System.Drawing.Size(139, 23);
		this.checkClearLeagueStats.TabIndex = 169;
		this.checkClearLeagueStats.Text = "Clear League Stats";
		this.toolTip.SetToolTip(this.checkClearLeagueStats, "Check this only for \"Apertura\" tournaments");
		this.checkClearLeagueStats.UseVisualStyleBackColor = true;
		this.checkClearLeagueStats.CheckedChanged += new System.EventHandler(checkClearLeagueStats_CheckedChanged);
		this.groupStageSchedules.Controls.Add(this.treeStageSchedule);
		this.groupStageSchedules.Controls.Add(this.panelStageScheduleDetails);
		this.groupStageSchedules.Controls.Add(this.toolStageSchedule);
		this.groupStageSchedules.Location = new System.Drawing.Point(466, 0);
		this.groupStageSchedules.Name = "groupStageSchedules";
		this.groupStageSchedules.Size = new System.Drawing.Size(305, 646);
		this.groupStageSchedules.TabIndex = 19;
		this.groupStageSchedules.TabStop = false;
		this.groupStageSchedules.Text = "Schedules";
		this.treeStageSchedule.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeStageSchedule.FullRowSelect = true;
		this.treeStageSchedule.HideSelection = false;
		this.treeStageSchedule.Location = new System.Drawing.Point(3, 220);
		this.treeStageSchedule.Name = "treeStageSchedule";
		this.treeStageSchedule.Size = new System.Drawing.Size(299, 423);
		this.treeStageSchedule.TabIndex = 7;
		this.treeStageSchedule.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeStageSchedule_AfterSelect);
		this.panelStageScheduleDetails.Controls.Add(this.groupStageScheduleDetails);
		this.panelStageScheduleDetails.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelStageScheduleDetails.Location = new System.Drawing.Point(3, 126);
		this.panelStageScheduleDetails.Name = "panelStageScheduleDetails";
		this.panelStageScheduleDetails.Size = new System.Drawing.Size(299, 94);
		this.panelStageScheduleDetails.TabIndex = 8;
		this.groupStageScheduleDetails.Controls.Add(this.dateStagePicker);
		this.groupStageScheduleDetails.Controls.Add(this.label37);
		this.groupStageScheduleDetails.Controls.Add(this.numericStageMinGames);
		this.groupStageScheduleDetails.Controls.Add(this.label36);
		this.groupStageScheduleDetails.Controls.Add(this.numericStageMaxGames);
		this.groupStageScheduleDetails.Controls.Add(this.label35);
		this.groupStageScheduleDetails.Controls.Add(this.comboStageTime);
		this.groupStageScheduleDetails.Controls.Add(this.label34);
		this.groupStageScheduleDetails.Location = new System.Drawing.Point(3, 0);
		this.groupStageScheduleDetails.Name = "groupStageScheduleDetails";
		this.groupStageScheduleDetails.Size = new System.Drawing.Size(264, 90);
		this.groupStageScheduleDetails.TabIndex = 25;
		this.groupStageScheduleDetails.TabStop = false;
		this.dateStagePicker.Location = new System.Drawing.Point(12, 13);
		this.dateStagePicker.Name = "dateStagePicker";
		this.dateStagePicker.Size = new System.Drawing.Size(241, 20);
		this.dateStagePicker.TabIndex = 17;
		this.dateStagePicker.ValueChanged += new System.EventHandler(dateStagePicker_ValueChanged);
		this.label37.AutoSize = true;
		this.label37.Location = new System.Drawing.Point(65, 70);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(26, 13);
		this.label37.TabIndex = 24;
		this.label37.Text = "min:";
		this.numericStageMinGames.Location = new System.Drawing.Point(95, 65);
		this.numericStageMinGames.Maximum = new decimal(new int[4] { 80, 0, 0, 0 });
		this.numericStageMinGames.Name = "numericStageMinGames";
		this.numericStageMinGames.Size = new System.Drawing.Size(60, 20);
		this.numericStageMinGames.TabIndex = 18;
		this.numericStageMinGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericStageMinGames.Value = new decimal(new int[4] { 12, 0, 0, 0 });
		this.numericStageMinGames.ValueChanged += new System.EventHandler(numericStageMinGames_ValueChanged);
		this.label36.AutoSize = true;
		this.label36.Location = new System.Drawing.Point(162, 70);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(29, 13);
		this.label36.TabIndex = 23;
		this.label36.Text = "max:";
		this.numericStageMaxGames.Location = new System.Drawing.Point(193, 65);
		this.numericStageMaxGames.Maximum = new decimal(new int[4] { 80, 0, 0, 0 });
		this.numericStageMaxGames.Name = "numericStageMaxGames";
		this.numericStageMaxGames.Size = new System.Drawing.Size(60, 20);
		this.numericStageMaxGames.TabIndex = 19;
		this.numericStageMaxGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericStageMaxGames.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.numericStageMaxGames.ValueChanged += new System.EventHandler(numericStageMaxGames_ValueChanged);
		this.label35.AutoSize = true;
		this.label35.Location = new System.Drawing.Point(16, 70);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(40, 13);
		this.label35.TabIndex = 22;
		this.label35.Text = "Games";
		this.comboStageTime.FormattingEnabled = true;
		this.comboStageTime.Items.AddRange(new object[47]
		{
			"12.00", "12.15", "12.30", "12.45", "13.00", "13.15", "13.30", "13.45", "14.00", "14.15",
			"14.30", "14.45", "15.00", "15.15", "15.30", "15.45", "16.00", "16.15", "16.30", "16.45",
			"17.00", "17.15", "17.30", "17.45", "18.00", "18.15", "18.30", "18.45", "19.00", "19.15",
			"19.30", "19.45", "20.00", "20.15", "20.30", "20.45", "21.00", "21.15", "21.30", "21.45",
			"22.00", "22.15", "22.30", "22.45", "23.00", "23.15", "23.30"
		});
		this.comboStageTime.Location = new System.Drawing.Point(60, 38);
		this.comboStageTime.Name = "comboStageTime";
		this.comboStageTime.Size = new System.Drawing.Size(121, 21);
		this.comboStageTime.TabIndex = 20;
		this.comboStageTime.SelectedIndexChanged += new System.EventHandler(comboStageTime_SelectedIndexChanged);
		this.label34.AutoSize = true;
		this.label34.Location = new System.Drawing.Point(16, 41);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(30, 13);
		this.label34.TabIndex = 21;
		this.label34.Text = "Time";
		this.toolStageSchedule.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.buttonCopyStageCalendar, this.buttonPasteStageCalendar, this.buttonCleanStageCalendar, this.buttonNeewStageLeg, this.buttonDeleteStageLeg, this.buttonStageAddTime, this.buttonStageRemoveTime, this.buttonStageSortLegs });
		this.toolStageSchedule.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.toolStageSchedule.Location = new System.Drawing.Point(3, 16);
		this.toolStageSchedule.Name = "toolStageSchedule";
		this.toolStageSchedule.Size = new System.Drawing.Size(299, 110);
		this.toolStageSchedule.TabIndex = 0;
		this.buttonCopyStageCalendar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCopyStageCalendar.Image = (System.Drawing.Image)resources.GetObject("buttonCopyStageCalendar.Image");
		this.buttonCopyStageCalendar.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCopyStageCalendar.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCopyStageCalendar.Name = "buttonCopyStageCalendar";
		this.buttonCopyStageCalendar.Size = new System.Drawing.Size(52, 52);
		this.buttonCopyStageCalendar.Text = "Copy Calendar";
		this.buttonCopyStageCalendar.Click += new System.EventHandler(buttonCopyStageCalendar_Click);
		this.buttonPasteStageCalendar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPasteStageCalendar.Enabled = false;
		this.buttonPasteStageCalendar.Image = (System.Drawing.Image)resources.GetObject("buttonPasteStageCalendar.Image");
		this.buttonPasteStageCalendar.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonPasteStageCalendar.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPasteStageCalendar.Name = "buttonPasteStageCalendar";
		this.buttonPasteStageCalendar.Size = new System.Drawing.Size(52, 52);
		this.buttonPasteStageCalendar.Text = "Paste Calendar";
		this.buttonPasteStageCalendar.Click += new System.EventHandler(buttonPasteStageCalendar_Click);
		this.buttonCleanStageCalendar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCleanStageCalendar.Image = (System.Drawing.Image)resources.GetObject("buttonCleanStageCalendar.Image");
		this.buttonCleanStageCalendar.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCleanStageCalendar.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCleanStageCalendar.Name = "buttonCleanStageCalendar";
		this.buttonCleanStageCalendar.Size = new System.Drawing.Size(52, 52);
		this.buttonCleanStageCalendar.Text = "Clean Calendar";
		this.buttonCleanStageCalendar.Click += new System.EventHandler(buttonCleanStageCalendar_Click);
		this.buttonNeewStageLeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonNeewStageLeg.Image = (System.Drawing.Image)resources.GetObject("buttonNeewStageLeg.Image");
		this.buttonNeewStageLeg.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonNeewStageLeg.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonNeewStageLeg.Name = "buttonNeewStageLeg";
		this.buttonNeewStageLeg.Size = new System.Drawing.Size(52, 52);
		this.buttonNeewStageLeg.Text = "New Leg";
		this.buttonNeewStageLeg.Click += new System.EventHandler(buttonNewStageLeg_Click);
		this.buttonDeleteStageLeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteStageLeg.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteStageLeg.Image");
		this.buttonDeleteStageLeg.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonDeleteStageLeg.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteStageLeg.Name = "buttonDeleteStageLeg";
		this.buttonDeleteStageLeg.Size = new System.Drawing.Size(52, 52);
		this.buttonDeleteStageLeg.Text = "Remove Leg";
		this.buttonDeleteStageLeg.Click += new System.EventHandler(buttonDeleteStageLeg_Click);
		this.buttonStageAddTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonStageAddTime.Image = (System.Drawing.Image)resources.GetObject("buttonStageAddTime.Image");
		this.buttonStageAddTime.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonStageAddTime.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonStageAddTime.Name = "buttonStageAddTime";
		this.buttonStageAddTime.Size = new System.Drawing.Size(52, 52);
		this.buttonStageAddTime.Text = "Add Time";
		this.buttonStageAddTime.Click += new System.EventHandler(buttonStageAddTime_Click);
		this.buttonStageRemoveTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonStageRemoveTime.Image = (System.Drawing.Image)resources.GetObject("buttonStageRemoveTime.Image");
		this.buttonStageRemoveTime.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonStageRemoveTime.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonStageRemoveTime.Name = "buttonStageRemoveTime";
		this.buttonStageRemoveTime.Size = new System.Drawing.Size(52, 52);
		this.buttonStageRemoveTime.Text = "Remove Time";
		this.buttonStageRemoveTime.Click += new System.EventHandler(buttonStageRemoveTime_Click);
		this.buttonStageSortLegs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonStageSortLegs.Image = (System.Drawing.Image)resources.GetObject("buttonStageSortLegs.Image");
		this.buttonStageSortLegs.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonStageSortLegs.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonStageSortLegs.Name = "buttonStageSortLegs";
		this.buttonStageSortLegs.Size = new System.Drawing.Size(52, 52);
		this.buttonStageSortLegs.Text = "Sort Legs By date";
		this.buttonStageSortLegs.Click += new System.EventHandler(buttonStageSortLegs_Click);
		this.numericRegularSeason.BackColor = System.Drawing.Color.Yellow;
		this.numericRegularSeason.Location = new System.Drawing.Point(367, 402);
		this.numericRegularSeason.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.numericRegularSeason.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericRegularSeason.Name = "numericRegularSeason";
		this.numericRegularSeason.Size = new System.Drawing.Size(83, 20);
		this.numericRegularSeason.TabIndex = 165;
		this.numericRegularSeason.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericRegularSeason.Visible = false;
		this.numericRegularSeason.ValueChanged += new System.EventHandler(numericRegularSeason_ValueChanged);
		this.comboSpecialKo2Rule.FormattingEnabled = true;
		this.comboSpecialKo2Rule.Items.AddRange(new object[4] { "Away Goal Rule, Extra Time, Penalties ", "Extra Time, Penalties (No Away Goal Rule)", "Penalties", "Regular Season Rank" });
		this.comboSpecialKo2Rule.Location = new System.Drawing.Point(152, 402);
		this.comboSpecialKo2Rule.Name = "comboSpecialKo2Rule";
		this.comboSpecialKo2Rule.Size = new System.Drawing.Size(209, 21);
		this.comboSpecialKo2Rule.TabIndex = 164;
		this.comboSpecialKo2Rule.SelectedIndexChanged += new System.EventHandler(comboSpecialKo2Rule_SelectedIndexChanged);
		this.checkSpecialKo2Rule.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkSpecialKo2Rule.Location = new System.Drawing.Point(3, 402);
		this.checkSpecialKo2Rule.Name = "checkSpecialKo2Rule";
		this.checkSpecialKo2Rule.Size = new System.Drawing.Size(136, 23);
		this.checkSpecialKo2Rule.TabIndex = 163;
		this.checkSpecialKo2Rule.Text = "Special Tie Rule 2 Legs";
		this.toolTip.SetToolTip(this.checkSpecialKo2Rule, "By default use the value defined by the Nation");
		this.checkSpecialKo2Rule.UseVisualStyleBackColor = true;
		this.checkSpecialKo2Rule.CheckedChanged += new System.EventHandler(checkSpecialKo2Rule_CheckedChanged);
		this.comboSpecialKo1Rule.FormattingEnabled = true;
		this.comboSpecialKo1Rule.Items.AddRange(new object[3] { "Extra Time, Penalties", "Penalties", "Replay" });
		this.comboSpecialKo1Rule.Location = new System.Drawing.Point(152, 373);
		this.comboSpecialKo1Rule.Name = "comboSpecialKo1Rule";
		this.comboSpecialKo1Rule.Size = new System.Drawing.Size(209, 21);
		this.comboSpecialKo1Rule.TabIndex = 162;
		this.comboSpecialKo1Rule.SelectedIndexChanged += new System.EventHandler(comboSpecialKo1Rule_SelectedIndexChanged);
		this.checkSpecialKo1Rule.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkSpecialKo1Rule.Location = new System.Drawing.Point(3, 373);
		this.checkSpecialKo1Rule.Name = "checkSpecialKo1Rule";
		this.checkSpecialKo1Rule.Size = new System.Drawing.Size(136, 23);
		this.checkSpecialKo1Rule.TabIndex = 161;
		this.checkSpecialKo1Rule.Text = "Special Tie Rule 1 Leg";
		this.toolTip.SetToolTip(this.checkSpecialKo1Rule, "By default use the value defined by the Nation");
		this.checkSpecialKo1Rule.UseVisualStyleBackColor = true;
		this.checkSpecialKo1Rule.CheckedChanged += new System.EventHandler(checkSpecialKo1Rule_CheckedChanged);
		this.numericKeepPointsPercentage.Increment = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericKeepPointsPercentage.Location = new System.Drawing.Point(123, 309);
		this.numericKeepPointsPercentage.Name = "numericKeepPointsPercentage";
		this.numericKeepPointsPercentage.Size = new System.Drawing.Size(83, 20);
		this.numericKeepPointsPercentage.TabIndex = 29;
		this.numericKeepPointsPercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericKeepPointsPercentage.ValueChanged += new System.EventHandler(numericKeepPointsPercentage_ValueChanged);
		this.checkKeepPointsPercentage.AutoSize = true;
		this.checkKeepPointsPercentage.Location = new System.Drawing.Point(6, 310);
		this.checkKeepPointsPercentage.Name = "checkKeepPointsPercentage";
		this.checkKeepPointsPercentage.Size = new System.Drawing.Size(94, 17);
		this.checkKeepPointsPercentage.TabIndex = 28;
		this.checkKeepPointsPercentage.Text = "Keep Points %";
		this.checkKeepPointsPercentage.UseVisualStyleBackColor = true;
		this.checkKeepPointsPercentage.CheckedChanged += new System.EventHandler(checkKeepPointsPercentage_CheckedChanged);
		this.numericStageRef.BackColor = System.Drawing.Color.Yellow;
		this.numericStageRef.Location = new System.Drawing.Point(123, 239);
		this.numericStageRef.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.numericStageRef.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericStageRef.Name = "numericStageRef";
		this.numericStageRef.Size = new System.Drawing.Size(83, 20);
		this.numericStageRef.TabIndex = 22;
		this.numericStageRef.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.toolTip.SetToolTip(this.numericStageRef, "Set this value to the id of the stage containing the groups");
		this.numericStageRef.ValueChanged += new System.EventHandler(numericStageRef_ValueChanged);
		this.checkClausuraSchedule.AutoSize = true;
		this.checkClausuraSchedule.Location = new System.Drawing.Point(6, 264);
		this.checkClausuraSchedule.Name = "checkClausuraSchedule";
		this.checkClausuraSchedule.Size = new System.Drawing.Size(115, 17);
		this.checkClausuraSchedule.TabIndex = 23;
		this.checkClausuraSchedule.Text = "Clausura Schedule";
		this.checkClausuraSchedule.UseVisualStyleBackColor = true;
		this.checkClausuraSchedule.CheckedChanged += new System.EventHandler(checkClausuraSchedule_CheckedChanged);
		this.groupStadiums.Controls.Add(this.comboStadium12);
		this.groupStadiums.Controls.Add(this.comboStadium11);
		this.groupStadiums.Controls.Add(this.comboStadium10);
		this.groupStadiums.Controls.Add(this.comboStadium9);
		this.groupStadiums.Controls.Add(this.comboStadium8);
		this.groupStadiums.Controls.Add(this.comboStadium7);
		this.groupStadiums.Controls.Add(this.comboStadium6);
		this.groupStadiums.Controls.Add(this.comboStadium5);
		this.groupStadiums.Controls.Add(this.comboStadium4);
		this.groupStadiums.Controls.Add(this.comboStadium3);
		this.groupStadiums.Controls.Add(this.comboStadium2);
		this.groupStadiums.Controls.Add(this.comboStadium1);
		this.groupStadiums.Location = new System.Drawing.Point(228, 16);
		this.groupStadiums.Name = "groupStadiums";
		this.groupStadiums.Size = new System.Drawing.Size(222, 347);
		this.groupStadiums.TabIndex = 20;
		this.groupStadiums.TabStop = false;
		this.groupStadiums.Text = "Stadiums";
		this.comboStadium12.FormattingEnabled = true;
		this.comboStadium12.Location = new System.Drawing.Point(17, 319);
		this.comboStadium12.Name = "comboStadium12";
		this.comboStadium12.Size = new System.Drawing.Size(200, 21);
		this.comboStadium12.TabIndex = 11;
		this.comboStadium12.SelectedIndexChanged += new System.EventHandler(comboStadium12_SelectedIndexChanged);
		this.comboStadium11.FormattingEnabled = true;
		this.comboStadium11.Location = new System.Drawing.Point(17, 292);
		this.comboStadium11.Name = "comboStadium11";
		this.comboStadium11.Size = new System.Drawing.Size(200, 21);
		this.comboStadium11.TabIndex = 10;
		this.comboStadium11.SelectedIndexChanged += new System.EventHandler(comboStadium11_SelectedIndexChanged);
		this.comboStadium10.FormattingEnabled = true;
		this.comboStadium10.Location = new System.Drawing.Point(16, 265);
		this.comboStadium10.Name = "comboStadium10";
		this.comboStadium10.Size = new System.Drawing.Size(200, 21);
		this.comboStadium10.TabIndex = 9;
		this.comboStadium10.SelectedIndexChanged += new System.EventHandler(comboStadium10_SelectedIndexChanged);
		this.comboStadium9.FormattingEnabled = true;
		this.comboStadium9.Location = new System.Drawing.Point(16, 238);
		this.comboStadium9.Name = "comboStadium9";
		this.comboStadium9.Size = new System.Drawing.Size(200, 21);
		this.comboStadium9.TabIndex = 8;
		this.comboStadium9.SelectedIndexChanged += new System.EventHandler(comboStadium9_SelectedIndexChanged);
		this.comboStadium8.FormattingEnabled = true;
		this.comboStadium8.Location = new System.Drawing.Point(16, 211);
		this.comboStadium8.Name = "comboStadium8";
		this.comboStadium8.Size = new System.Drawing.Size(200, 21);
		this.comboStadium8.TabIndex = 7;
		this.comboStadium8.SelectedIndexChanged += new System.EventHandler(comboStadium8_SelectedIndexChanged);
		this.comboStadium7.FormattingEnabled = true;
		this.comboStadium7.Location = new System.Drawing.Point(17, 184);
		this.comboStadium7.Name = "comboStadium7";
		this.comboStadium7.Size = new System.Drawing.Size(200, 21);
		this.comboStadium7.TabIndex = 6;
		this.comboStadium7.SelectedIndexChanged += new System.EventHandler(comboStadium7_SelectedIndexChanged);
		this.comboStadium6.FormattingEnabled = true;
		this.comboStadium6.Location = new System.Drawing.Point(17, 157);
		this.comboStadium6.Name = "comboStadium6";
		this.comboStadium6.Size = new System.Drawing.Size(200, 21);
		this.comboStadium6.TabIndex = 5;
		this.comboStadium6.SelectedIndexChanged += new System.EventHandler(comboStadium6_SelectedIndexChanged);
		this.comboStadium5.FormattingEnabled = true;
		this.comboStadium5.Location = new System.Drawing.Point(16, 130);
		this.comboStadium5.Name = "comboStadium5";
		this.comboStadium5.Size = new System.Drawing.Size(200, 21);
		this.comboStadium5.TabIndex = 4;
		this.comboStadium5.SelectedIndexChanged += new System.EventHandler(comboStadium5_SelectedIndexChanged);
		this.comboStadium4.FormattingEnabled = true;
		this.comboStadium4.Location = new System.Drawing.Point(17, 103);
		this.comboStadium4.Name = "comboStadium4";
		this.comboStadium4.Size = new System.Drawing.Size(200, 21);
		this.comboStadium4.TabIndex = 3;
		this.comboStadium4.SelectedIndexChanged += new System.EventHandler(comboStadium4_SelectedIndexChanged);
		this.comboStadium3.FormattingEnabled = true;
		this.comboStadium3.Location = new System.Drawing.Point(16, 76);
		this.comboStadium3.Name = "comboStadium3";
		this.comboStadium3.Size = new System.Drawing.Size(200, 21);
		this.comboStadium3.TabIndex = 2;
		this.comboStadium3.SelectedIndexChanged += new System.EventHandler(comboStadium3_SelectedIndexChanged);
		this.comboStadium2.FormattingEnabled = true;
		this.comboStadium2.Location = new System.Drawing.Point(16, 49);
		this.comboStadium2.Name = "comboStadium2";
		this.comboStadium2.Size = new System.Drawing.Size(200, 21);
		this.comboStadium2.TabIndex = 1;
		this.comboStadium2.SelectedIndexChanged += new System.EventHandler(comboStadium2_SelectedIndexChanged);
		this.comboStadium1.FormattingEnabled = true;
		this.comboStadium1.Location = new System.Drawing.Point(17, 22);
		this.comboStadium1.Name = "comboStadium1";
		this.comboStadium1.Size = new System.Drawing.Size(200, 21);
		this.comboStadium1.TabIndex = 0;
		this.comboStadium1.SelectedIndexChanged += new System.EventHandler(comboStadium1_SelectedIndexChanged);
		this.checkMaxteamsgroup.AutoSize = true;
		this.checkMaxteamsgroup.Location = new System.Drawing.Point(6, 240);
		this.checkMaxteamsgroup.Name = "checkMaxteamsgroup";
		this.checkMaxteamsgroup.Size = new System.Drawing.Size(111, 17);
		this.checkMaxteamsgroup.TabIndex = 2;
		this.checkMaxteamsgroup.Text = "Avoid same group";
		this.checkMaxteamsgroup.UseVisualStyleBackColor = true;
		this.checkMaxteamsgroup.CheckedChanged += new System.EventHandler(checkMaxteamsgroup_CheckedChanged);
		this.checkMatchReplay.AutoSize = true;
		this.checkMatchReplay.Location = new System.Drawing.Point(6, 194);
		this.checkMatchReplay.Name = "checkMatchReplay";
		this.checkMatchReplay.Size = new System.Drawing.Size(92, 17);
		this.checkMatchReplay.TabIndex = 22;
		this.checkMatchReplay.Text = "Match Replay";
		this.checkMatchReplay.UseVisualStyleBackColor = true;
		this.checkMatchReplay.CheckedChanged += new System.EventHandler(checkMatchReplay_CheckedChanged);
		this.numericMoneyDrop.Location = new System.Drawing.Point(135, 86);
		this.numericMoneyDrop.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericMoneyDrop.Name = "numericMoneyDrop";
		this.numericMoneyDrop.Size = new System.Drawing.Size(83, 20);
		this.numericMoneyDrop.TabIndex = 21;
		this.numericMoneyDrop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericMoneyDrop.ValueChanged += new System.EventHandler(numericMoneyDrop_ValueChanged);
		this.checkMaxteamsassoc.AutoSize = true;
		this.checkMaxteamsassoc.Location = new System.Drawing.Point(6, 217);
		this.checkMaxteamsassoc.Name = "checkMaxteamsassoc";
		this.checkMaxteamsassoc.Size = new System.Drawing.Size(122, 17);
		this.checkMaxteamsassoc.TabIndex = 1;
		this.checkMaxteamsassoc.Text = "Avoid Same Country";
		this.checkMaxteamsassoc.UseVisualStyleBackColor = true;
		this.checkMaxteamsassoc.CheckedChanged += new System.EventHandler(checkMaxteamsassoc_CheckedChanged);
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(6, 90);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(123, 13);
		this.label10.TabIndex = 20;
		this.label10.Text = "Money Drop Percentage";
		this.numericPrizeMoney.Increment = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.numericPrizeMoney.Location = new System.Drawing.Point(97, 60);
		this.numericPrizeMoney.Maximum = new decimal(new int[4] { 500000000, 0, 0, 0 });
		this.numericPrizeMoney.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericPrizeMoney.Name = "numericPrizeMoney";
		this.numericPrizeMoney.Size = new System.Drawing.Size(120, 20);
		this.numericPrizeMoney.TabIndex = 19;
		this.numericPrizeMoney.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPrizeMoney.ValueChanged += new System.EventHandler(numericPrizeMoney_ValueChanged);
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(6, 67);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(65, 13);
		this.label9.TabIndex = 18;
		this.label9.Text = "Prize Money";
		this.comboMatchSituation.FormattingEnabled = true;
		this.comboMatchSituation.Items.AddRange(new object[11]
		{
			"FRIENDLY", "LEAGUE", "QUALIFY", "GROUP", "ROUND16", "ROUNDX", "QUARTER", "SEMI", "FINAL", "THIRDPLACE",
			"REPLAY"
		});
		this.comboMatchSituation.Location = new System.Drawing.Point(97, 23);
		this.comboMatchSituation.Name = "comboMatchSituation";
		this.comboMatchSituation.Size = new System.Drawing.Size(121, 21);
		this.comboMatchSituation.TabIndex = 17;
		this.comboMatchSituation.SelectedIndexChanged += new System.EventHandler(comboMatchSituation_SelectedIndexChanged);
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(6, 26);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(81, 13);
		this.label8.TabIndex = 16;
		this.label8.Text = "Match Situation";
		this.groupSetupStage.Controls.Add(this.checkRandomDraw);
		this.groupSetupStage.Controls.Add(this.groupBox2);
		this.groupSetupStage.Controls.Add(this.checkCalccompavgs);
		this.groupSetupStage.Location = new System.Drawing.Point(6, 75);
		this.groupSetupStage.Name = "groupSetupStage";
		this.groupSetupStage.Size = new System.Drawing.Size(468, 157);
		this.groupSetupStage.TabIndex = 17;
		this.groupSetupStage.TabStop = false;
		this.groupSetupStage.Text = "Setup Stage";
		this.checkRandomDraw.AutoSize = true;
		this.checkRandomDraw.Location = new System.Drawing.Point(9, 19);
		this.checkRandomDraw.Name = "checkRandomDraw";
		this.checkRandomDraw.Size = new System.Drawing.Size(94, 17);
		this.checkRandomDraw.TabIndex = 20;
		this.checkRandomDraw.Text = "Random Draw";
		this.checkRandomDraw.UseVisualStyleBackColor = true;
		this.checkRandomDraw.CheckedChanged += new System.EventHandler(checkRandomDraw_CheckedChanged);
		this.groupBox2.Controls.Add(this.comboSpecialTeam4);
		this.groupBox2.Controls.Add(this.comboSpecialTeam3);
		this.groupBox2.Controls.Add(this.comboSpecialTeam2);
		this.groupBox2.Controls.Add(this.comboSpecialTeam1);
		this.groupBox2.Location = new System.Drawing.Point(180, 15);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(176, 134);
		this.groupBox2.TabIndex = 19;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Special Teams";
		this.comboSpecialTeam4.FormattingEnabled = true;
		this.comboSpecialTeam4.Location = new System.Drawing.Point(9, 100);
		this.comboSpecialTeam4.Name = "comboSpecialTeam4";
		this.comboSpecialTeam4.Size = new System.Drawing.Size(157, 21);
		this.comboSpecialTeam4.TabIndex = 29;
		this.comboSpecialTeam4.SelectedIndexChanged += new System.EventHandler(comboSpecialTeam4_SelectedIndexChanged);
		this.comboSpecialTeam3.FormattingEnabled = true;
		this.comboSpecialTeam3.Location = new System.Drawing.Point(9, 73);
		this.comboSpecialTeam3.Name = "comboSpecialTeam3";
		this.comboSpecialTeam3.Size = new System.Drawing.Size(157, 21);
		this.comboSpecialTeam3.TabIndex = 28;
		this.comboSpecialTeam3.SelectedIndexChanged += new System.EventHandler(comboSpecialTeam3_SelectedIndexChanged);
		this.comboSpecialTeam2.FormattingEnabled = true;
		this.comboSpecialTeam2.Location = new System.Drawing.Point(9, 46);
		this.comboSpecialTeam2.Name = "comboSpecialTeam2";
		this.comboSpecialTeam2.Size = new System.Drawing.Size(157, 21);
		this.comboSpecialTeam2.TabIndex = 27;
		this.comboSpecialTeam2.SelectedIndexChanged += new System.EventHandler(comboSpecialTeam2_SelectedIndexChanged);
		this.comboSpecialTeam1.FormattingEnabled = true;
		this.comboSpecialTeam1.Location = new System.Drawing.Point(9, 19);
		this.comboSpecialTeam1.Name = "comboSpecialTeam1";
		this.comboSpecialTeam1.Size = new System.Drawing.Size(157, 21);
		this.comboSpecialTeam1.TabIndex = 26;
		this.comboSpecialTeam1.SelectedIndexChanged += new System.EventHandler(comboSpecialTeam1_SelectedIndexChanged);
		this.checkCalccompavgs.AutoSize = true;
		this.checkCalccompavgs.Location = new System.Drawing.Point(9, 42);
		this.checkCalccompavgs.Name = "checkCalccompavgs";
		this.checkCalccompavgs.Size = new System.Drawing.Size(160, 17);
		this.checkCalccompavgs.TabIndex = 0;
		this.checkCalccompavgs.Text = "Sort by Competition Average";
		this.checkCalccompavgs.UseVisualStyleBackColor = true;
		this.checkCalccompavgs.CheckedChanged += new System.EventHandler(checkCalccompavgs_CheckedChanged);
		this.comboStageStandingRules.FormattingEnabled = true;
		this.comboStageStandingRules.Items.AddRange(new object[6] { "Points, Goals, Wins", "Points. Wins, Goals", "Points, Head To Head, Goals", "Team Rating", "Previous Ranking", "Points, Goals, Head To Head" });
		this.comboStageStandingRules.Location = new System.Drawing.Point(155, 46);
		this.comboStageStandingRules.Name = "comboStageStandingRules";
		this.comboStageStandingRules.Size = new System.Drawing.Size(185, 21);
		this.comboStageStandingRules.TabIndex = 162;
		this.comboStageStandingRules.SelectedIndexChanged += new System.EventHandler(comboStageStandingRules_SelectedIndexChanged);
		this.checkStageStandingsRules.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkStageStandingsRules.Location = new System.Drawing.Point(6, 46);
		this.checkStageStandingsRules.Name = "checkStageStandingsRules";
		this.checkStageStandingsRules.Size = new System.Drawing.Size(136, 23);
		this.checkStageStandingsRules.TabIndex = 161;
		this.checkStageStandingsRules.Text = "Special Standing Rules";
		this.toolTip.SetToolTip(this.checkStageStandingsRules, "By default use the value defined by the Nation");
		this.checkStageStandingsRules.UseVisualStyleBackColor = true;
		this.checkStageStandingsRules.CheckedChanged += new System.EventHandler(checkStageStandingsRules_CheckedChanged);
		this.numericStandingsRank.BackColor = System.Drawing.Color.Yellow;
		this.numericStandingsRank.Location = new System.Drawing.Point(480, 46);
		this.numericStandingsRank.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.numericStandingsRank.Name = "numericStandingsRank";
		this.numericStandingsRank.Size = new System.Drawing.Size(83, 20);
		this.numericStandingsRank.TabIndex = 167;
		this.numericStandingsRank.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericStandingsRank.ValueChanged += new System.EventHandler(numericStandingsRank_ValueChanged);
		this.checkStandingsRank.AutoSize = true;
		this.checkStandingsRank.Location = new System.Drawing.Point(363, 47);
		this.checkStandingsRank.Name = "checkStandingsRank";
		this.checkStandingsRank.Size = new System.Drawing.Size(102, 17);
		this.checkStandingsRank.TabIndex = 166;
		this.checkStandingsRank.Text = "Standings Rank";
		this.checkStandingsRank.UseVisualStyleBackColor = true;
		this.checkStandingsRank.CheckedChanged += new System.EventHandler(checkStandingsRank_CheckedChanged);
		this.comboStageType.FormattingEnabled = true;
		this.comboStageType.Items.AddRange(new object[5] { "SETUP", "FRIENDLY", "LEAGUE", "KO1LEG", "KO2LEGS" });
		this.comboStageType.Location = new System.Drawing.Point(106, 20);
		this.comboStageType.Name = "comboStageType";
		this.comboStageType.Size = new System.Drawing.Size(121, 21);
		this.comboStageType.TabIndex = 15;
		this.comboStageType.SelectedIndexChanged += new System.EventHandler(comboStageType_SelectedIndexChanged);
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(6, 26);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(62, 13);
		this.label7.TabIndex = 14;
		this.label7.Text = "Stage Type";
		this.numericStandingKeep.BackColor = System.Drawing.Color.Yellow;
		this.numericStandingKeep.Location = new System.Drawing.Point(480, 20);
		this.numericStandingKeep.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.numericStandingKeep.Name = "numericStandingKeep";
		this.numericStandingKeep.Size = new System.Drawing.Size(83, 20);
		this.numericStandingKeep.TabIndex = 27;
		this.numericStandingKeep.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericStandingKeep.ValueChanged += new System.EventHandler(numericStandingKeep_ValueChanged);
		this.checkStandingKeep.AutoSize = true;
		this.checkStandingKeep.Location = new System.Drawing.Point(363, 21);
		this.checkStandingKeep.Name = "checkStandingKeep";
		this.checkStandingKeep.Size = new System.Drawing.Size(99, 17);
		this.checkStandingKeep.TabIndex = 26;
		this.checkStandingKeep.Text = "Keep standings";
		this.checkStandingKeep.UseVisualStyleBackColor = true;
		this.checkStandingKeep.CheckedChanged += new System.EventHandler(checkStandingKeep_CheckedChanged);
		this.toolCompetitionTree.AutoSize = false;
		this.toolCompetitionTree.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolCompetitionTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[13]
		{
			this.buttonAddTrophy, this.buttonDeleteTrophy, this.buttonAddStage, this.buttonDeleteStage, this.buttonAddGroup, this.buttonDeleteGroup, this.buttonAddNatiom, this.buttonDeleteNation, this.buttonPasteTrophy, this.buttonCopyTrophy,
			this.comboTargetLeague, this.buttonCreatePatch, this.buttonLoadPatch
		});
		this.toolCompetitionTree.Location = new System.Drawing.Point(0, 0);
		this.toolCompetitionTree.Name = "toolCompetitionTree";
		this.toolCompetitionTree.Size = new System.Drawing.Size(332, 52);
		this.toolCompetitionTree.TabIndex = 14;
		this.toolCompetitionTree.Text = "stripToolWorld";
		this.buttonAddTrophy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAddTrophy.Image = (System.Drawing.Image)resources.GetObject("buttonAddTrophy.Image");
		this.buttonAddTrophy.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonAddTrophy.ImageTransparentColor = System.Drawing.Color.Transparent;
		this.buttonAddTrophy.Name = "buttonAddTrophy";
		this.buttonAddTrophy.Size = new System.Drawing.Size(52, 49);
		this.buttonAddTrophy.Text = "Add Trophy";
		this.buttonAddTrophy.Visible = false;
		this.buttonAddTrophy.Click += new System.EventHandler(buttonAddTrophy_Click);
		this.buttonDeleteTrophy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteTrophy.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteTrophy.Image");
		this.buttonDeleteTrophy.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonDeleteTrophy.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteTrophy.Name = "buttonDeleteTrophy";
		this.buttonDeleteTrophy.Size = new System.Drawing.Size(52, 49);
		this.buttonDeleteTrophy.Text = "Delete Trophy";
		this.buttonDeleteTrophy.Visible = false;
		this.buttonDeleteTrophy.Click += new System.EventHandler(buttonDeleteTrophy_Click);
		this.buttonAddStage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAddStage.Image = (System.Drawing.Image)resources.GetObject("buttonAddStage.Image");
		this.buttonAddStage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonAddStage.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddStage.Name = "buttonAddStage";
		this.buttonAddStage.Size = new System.Drawing.Size(52, 49);
		this.buttonAddStage.Text = "Add Stage";
		this.buttonAddStage.Visible = false;
		this.buttonAddStage.Click += new System.EventHandler(buttonAddStage_Click);
		this.buttonDeleteStage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteStage.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteStage.Image");
		this.buttonDeleteStage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonDeleteStage.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteStage.Name = "buttonDeleteStage";
		this.buttonDeleteStage.Size = new System.Drawing.Size(52, 49);
		this.buttonDeleteStage.Text = "Delete Stage";
		this.buttonDeleteStage.Visible = false;
		this.buttonDeleteStage.Click += new System.EventHandler(buttonDeleteStage_Click);
		this.buttonAddGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAddGroup.Image = (System.Drawing.Image)resources.GetObject("buttonAddGroup.Image");
		this.buttonAddGroup.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonAddGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddGroup.Name = "buttonAddGroup";
		this.buttonAddGroup.Size = new System.Drawing.Size(52, 49);
		this.buttonAddGroup.Text = "Add Group";
		this.buttonAddGroup.Visible = false;
		this.buttonAddGroup.Click += new System.EventHandler(buttonAddGroup_Click);
		this.buttonDeleteGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteGroup.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteGroup.Image");
		this.buttonDeleteGroup.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonDeleteGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteGroup.Name = "buttonDeleteGroup";
		this.buttonDeleteGroup.Size = new System.Drawing.Size(52, 49);
		this.buttonDeleteGroup.Text = "Delete Group";
		this.buttonDeleteGroup.Visible = false;
		this.buttonDeleteGroup.Click += new System.EventHandler(buttonDeleteGroup_Click);
		this.buttonAddNatiom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAddNatiom.Image = (System.Drawing.Image)resources.GetObject("buttonAddNatiom.Image");
		this.buttonAddNatiom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonAddNatiom.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddNatiom.Name = "buttonAddNatiom";
		this.buttonAddNatiom.Size = new System.Drawing.Size(52, 49);
		this.buttonAddNatiom.Text = "Add Nation";
		this.buttonAddNatiom.Visible = false;
		this.buttonAddNatiom.Click += new System.EventHandler(buttonAddNatiom_Click);
		this.buttonDeleteNation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteNation.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteNation.Image");
		this.buttonDeleteNation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonDeleteNation.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteNation.Name = "buttonDeleteNation";
		this.buttonDeleteNation.Size = new System.Drawing.Size(52, 49);
		this.buttonDeleteNation.Text = "Delete Nation";
		this.buttonDeleteNation.Visible = false;
		this.buttonDeleteNation.Click += new System.EventHandler(buttonDeleteNation_Click);
		this.buttonPasteTrophy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPasteTrophy.Enabled = false;
		this.buttonPasteTrophy.Image = (System.Drawing.Image)resources.GetObject("buttonPasteTrophy.Image");
		this.buttonPasteTrophy.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonPasteTrophy.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPasteTrophy.Name = "buttonPasteTrophy";
		this.buttonPasteTrophy.Size = new System.Drawing.Size(52, 49);
		this.buttonPasteTrophy.Text = "Paste Trophy";
		this.buttonPasteTrophy.Visible = false;
		this.buttonPasteTrophy.Click += new System.EventHandler(buttonPasteTrophy_Click);
		this.buttonCopyTrophy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCopyTrophy.Image = (System.Drawing.Image)resources.GetObject("buttonCopyTrophy.Image");
		this.buttonCopyTrophy.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCopyTrophy.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCopyTrophy.Name = "buttonCopyTrophy";
		this.buttonCopyTrophy.Size = new System.Drawing.Size(52, 49);
		this.buttonCopyTrophy.Text = "Copy Trophy";
		this.buttonCopyTrophy.Visible = false;
		this.buttonCopyTrophy.Click += new System.EventHandler(buttonCopyTrophy_Click);
		this.comboTargetLeague.Enabled = false;
		this.comboTargetLeague.Name = "comboTargetLeague";
		this.comboTargetLeague.Size = new System.Drawing.Size(125, 52);
		this.comboTargetLeague.ToolTipText = "Select Target League for \"Paste Trophy\"";
		this.buttonCreatePatch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCreatePatch.Image = (System.Drawing.Image)resources.GetObject("buttonCreatePatch.Image");
		this.buttonCreatePatch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCreatePatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCreatePatch.Name = "buttonCreatePatch";
		this.buttonCreatePatch.Size = new System.Drawing.Size(52, 49);
		this.buttonCreatePatch.Text = "Create Competition Patch";
		this.buttonCreatePatch.Visible = false;
		this.buttonCreatePatch.Click += new System.EventHandler(buttonCreatePatch_Click);
		this.buttonLoadPatch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonLoadPatch.Image = (System.Drawing.Image)resources.GetObject("buttonLoadPatch.Image");
		this.buttonLoadPatch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonLoadPatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonLoadPatch.Name = "buttonLoadPatch";
		this.buttonLoadPatch.Size = new System.Drawing.Size(52, 49);
		this.buttonLoadPatch.Text = "Load Competition Patch";
		this.buttonLoadPatch.ToolTipText = "Load Competition Patch";
		this.buttonLoadPatch.Visible = false;
		this.buttonLoadPatch.Click += new System.EventHandler(buttonLoadPatch_Click);
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 0);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.AutoScroll = true;
		this.splitContainer1.Panel1.Controls.Add(this.treeWorld);
		this.splitContainer1.Panel1.Controls.Add(this.toolCompetitionTree);
		this.splitContainer1.Panel2.AutoScroll = true;
		this.splitContainer1.Panel2.Controls.Add(this.tabCompetitions);
		this.splitContainer1.Panel2.Controls.Add(this.panelCompObj);
		this.splitContainer1.Size = new System.Drawing.Size(1087, 780);
		this.splitContainer1.SplitterDistance = 332;
		this.splitContainer1.TabIndex = 15;
		this.tabCompetitions.Controls.Add(this.pageWorld);
		this.tabCompetitions.Controls.Add(this.pageConfederation);
		this.tabCompetitions.Controls.Add(this.pageNation);
		this.tabCompetitions.Controls.Add(this.pageTrophy);
		this.tabCompetitions.Controls.Add(this.pageStage);
		this.tabCompetitions.Controls.Add(this.pageGroup);
		this.tabCompetitions.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabCompetitions.Location = new System.Drawing.Point(0, 30);
		this.tabCompetitions.Name = "tabCompetitions";
		this.tabCompetitions.SelectedIndex = 0;
		this.tabCompetitions.Size = new System.Drawing.Size(751, 750);
		this.tabCompetitions.TabIndex = 6;
		this.tabCompetitions.SelectedIndexChanged += new System.EventHandler(tabCompetitions_SelectedIndexChanged);
		this.pageWorld.Controls.Add(this.numericStartYear);
		this.pageWorld.Controls.Add(this.label13);
		this.pageWorld.Location = new System.Drawing.Point(4, 22);
		this.pageWorld.Name = "pageWorld";
		this.pageWorld.Padding = new System.Windows.Forms.Padding(3);
		this.pageWorld.Size = new System.Drawing.Size(743, 724);
		this.pageWorld.TabIndex = 5;
		this.pageWorld.Text = "FIFA";
		this.pageWorld.UseVisualStyleBackColor = true;
		this.numericStartYear.Location = new System.Drawing.Point(118, 30);
		this.numericStartYear.Maximum = new decimal(new int[4] { 9999, 0, 0, 0 });
		this.numericStartYear.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.numericStartYear.Name = "numericStartYear";
		this.numericStartYear.Size = new System.Drawing.Size(120, 20);
		this.numericStartYear.TabIndex = 21;
		this.numericStartYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericStartYear.Value = new decimal(new int[4] { 2010, 0, 0, 0 });
		this.numericStartYear.ValueChanged += new System.EventHandler(numericStartYear_ValueChanged);
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(27, 37);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(54, 13);
		this.label13.TabIndex = 20;
		this.label13.Text = "Year Start";
		this.pageConfederation.Controls.Add(this.groupConfederation);
		this.pageConfederation.Location = new System.Drawing.Point(4, 22);
		this.pageConfederation.Name = "pageConfederation";
		this.pageConfederation.Padding = new System.Windows.Forms.Padding(3);
		this.pageConfederation.Size = new System.Drawing.Size(743, 724);
		this.pageConfederation.TabIndex = 0;
		this.pageConfederation.Text = "Confederation";
		this.pageConfederation.UseVisualStyleBackColor = true;
		this.pageNation.AutoScroll = true;
		this.pageNation.Controls.Add(this.groupNation);
		this.pageNation.Location = new System.Drawing.Point(4, 22);
		this.pageNation.Name = "pageNation";
		this.pageNation.Padding = new System.Windows.Forms.Padding(3);
		this.pageNation.Size = new System.Drawing.Size(743, 724);
		this.pageNation.TabIndex = 1;
		this.pageNation.Text = "Nation";
		this.pageNation.UseVisualStyleBackColor = true;
		this.pageTrophy.AutoScroll = true;
		this.pageTrophy.Controls.Add(this.tabTrophy);
		this.pageTrophy.Location = new System.Drawing.Point(4, 22);
		this.pageTrophy.Name = "pageTrophy";
		this.pageTrophy.Size = new System.Drawing.Size(743, 724);
		this.pageTrophy.TabIndex = 2;
		this.pageTrophy.Text = "Trophy";
		this.pageTrophy.UseVisualStyleBackColor = true;
		this.tabTrophy.Controls.Add(this.tabPageTrophyStructure);
		this.tabTrophy.Controls.Add(this.tabPageRankingTable);
		this.tabTrophy.Controls.Add(this.tabPageTrophyGraphics);
		this.tabTrophy.Controls.Add(this.tabPageTrophyPitchGraphics);
		this.tabTrophy.Controls.Add(this.tabPageTrophyRevMod);
		this.tabTrophy.Controls.Add(this.tabPageWipe3D);
		this.tabTrophy.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabTrophy.Location = new System.Drawing.Point(0, 0);
		this.tabTrophy.Name = "tabTrophy";
		this.tabTrophy.SelectedIndex = 0;
		this.tabTrophy.Size = new System.Drawing.Size(743, 724);
		this.tabTrophy.TabIndex = 10;
		this.tabTrophy.SelectedIndexChanged += new System.EventHandler(tabTrophy_SelectedIndexChanged);
		this.tabPageTrophyStructure.Controls.Add(this.groupTrophy);
		this.tabPageTrophyStructure.Location = new System.Drawing.Point(4, 22);
		this.tabPageTrophyStructure.Name = "tabPageTrophyStructure";
		this.tabPageTrophyStructure.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageTrophyStructure.Size = new System.Drawing.Size(735, 698);
		this.tabPageTrophyStructure.TabIndex = 0;
		this.tabPageTrophyStructure.Text = "Structure";
		this.tabPageTrophyStructure.UseVisualStyleBackColor = true;
		this.tabPageRankingTable.Controls.Add(this.groupInitTeams);
		this.tabPageRankingTable.Location = new System.Drawing.Point(4, 22);
		this.tabPageRankingTable.Name = "tabPageRankingTable";
		this.tabPageRankingTable.Size = new System.Drawing.Size(735, 698);
		this.tabPageRankingTable.TabIndex = 2;
		this.tabPageRankingTable.Text = "Ranking Table";
		this.tabPageRankingTable.UseVisualStyleBackColor = true;
		this.groupInitTeams.Controls.Add(this.label70);
		this.groupInitTeams.Controls.Add(this.numericUpdateTableEntries);
		this.groupInitTeams.Controls.Add(this.panelAllInitTeams);
		this.groupInitTeams.Location = new System.Drawing.Point(10, 3);
		this.groupInitTeams.Name = "groupInitTeams";
		this.groupInitTeams.Size = new System.Drawing.Size(562, 695);
		this.groupInitTeams.TabIndex = 163;
		this.groupInitTeams.TabStop = false;
		this.groupInitTeams.Text = "Ranking";
		this.label70.AutoSize = true;
		this.label70.Location = new System.Drawing.Point(12, 18);
		this.label70.Name = "label70";
		this.label70.Size = new System.Drawing.Size(75, 13);
		this.label70.TabIndex = 24;
		this.label70.Text = "N. of Positions";
		this.numericUpdateTableEntries.Location = new System.Drawing.Point(108, 14);
		this.numericUpdateTableEntries.Maximum = new decimal(new int[4] { 48, 0, 0, 0 });
		this.numericUpdateTableEntries.Name = "numericUpdateTableEntries";
		this.numericUpdateTableEntries.Size = new System.Drawing.Size(74, 20);
		this.numericUpdateTableEntries.TabIndex = 25;
		this.numericUpdateTableEntries.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpdateTableEntries.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpdateTableEntries.ValueChanged += new System.EventHandler(numericUpdateTableEntries_ValueChanged);
		this.panelAllInitTeams.AutoScroll = true;
		this.panelAllInitTeams.BackColor = System.Drawing.Color.RosyBrown;
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam1);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam2);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam24);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam3);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam23);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam4);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam22);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam5);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam21);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam6);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam20);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam7);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam19);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam8);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam18);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam9);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam17);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam10);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam16);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam11);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam15);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam12);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam14);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam13);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam25);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam26);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam27);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam28);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam29);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam30);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam31);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam32);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam33);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam34);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam35);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam36);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam37);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam38);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam39);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam40);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam41);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam42);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam43);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam44);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam45);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam46);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam47);
		this.panelAllInitTeams.Controls.Add(this.panelInitTeam48);
		this.panelAllInitTeams.Location = new System.Drawing.Point(6, 37);
		this.panelAllInitTeams.Name = "panelAllInitTeams";
		this.panelAllInitTeams.Size = new System.Drawing.Size(550, 652);
		this.panelAllInitTeams.TabIndex = 26;
		this.panelInitTeam1.Controls.Add(this.labelUpdateTable1);
		this.panelInitTeam1.Controls.Add(this.comboInitTeam1);
		this.panelInitTeam1.Controls.Add(this.label42);
		this.panelInitTeam1.Location = new System.Drawing.Point(5, 3);
		this.panelInitTeam1.Name = "panelInitTeam1";
		this.panelInitTeam1.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam1.TabIndex = 0;
		this.labelUpdateTable1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable1.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable1.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable1.Name = "labelUpdateTable1";
		this.labelUpdateTable1.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable1.TabIndex = 2;
		this.labelUpdateTable1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable1.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam1.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam1.FormattingEnabled = true;
		this.comboInitTeam1.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam1.Name = "comboInitTeam1";
		this.comboInitTeam1.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam1.Sorted = true;
		this.comboInitTeam1.TabIndex = 1;
		this.label42.Dock = System.Windows.Forms.DockStyle.Left;
		this.label42.Location = new System.Drawing.Point(0, 0);
		this.label42.Name = "label42";
		this.label42.Size = new System.Drawing.Size(29, 25);
		this.label42.TabIndex = 0;
		this.label42.Text = " 1.";
		this.label42.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam2.Controls.Add(this.labelUpdateTable2);
		this.panelInitTeam2.Controls.Add(this.comboInitTeam2);
		this.panelInitTeam2.Controls.Add(this.label43);
		this.panelInitTeam2.Location = new System.Drawing.Point(5, 28);
		this.panelInitTeam2.Name = "panelInitTeam2";
		this.panelInitTeam2.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam2.TabIndex = 1;
		this.labelUpdateTable2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable2.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable2.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable2.Name = "labelUpdateTable2";
		this.labelUpdateTable2.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable2.TabIndex = 3;
		this.labelUpdateTable2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable2.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam2.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam2.FormattingEnabled = true;
		this.comboInitTeam2.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam2.Name = "comboInitTeam2";
		this.comboInitTeam2.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam2.Sorted = true;
		this.comboInitTeam2.TabIndex = 1;
		this.label43.Dock = System.Windows.Forms.DockStyle.Left;
		this.label43.Location = new System.Drawing.Point(0, 0);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(29, 25);
		this.label43.TabIndex = 0;
		this.label43.Text = " 2.";
		this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam24.Controls.Add(this.labelUpdateTable24);
		this.panelInitTeam24.Controls.Add(this.comboInitTeam24);
		this.panelInitTeam24.Controls.Add(this.label65);
		this.panelInitTeam24.Location = new System.Drawing.Point(6, 578);
		this.panelInitTeam24.Name = "panelInitTeam24";
		this.panelInitTeam24.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam24.TabIndex = 23;
		this.labelUpdateTable24.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable24.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable24.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable24.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable24.Name = "labelUpdateTable24";
		this.labelUpdateTable24.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable24.TabIndex = 4;
		this.labelUpdateTable24.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable24.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam24.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam24.FormattingEnabled = true;
		this.comboInitTeam24.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam24.Name = "comboInitTeam24";
		this.comboInitTeam24.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam24.Sorted = true;
		this.comboInitTeam24.TabIndex = 1;
		this.label65.Dock = System.Windows.Forms.DockStyle.Left;
		this.label65.Location = new System.Drawing.Point(0, 0);
		this.label65.Name = "label65";
		this.label65.Size = new System.Drawing.Size(28, 25);
		this.label65.TabIndex = 0;
		this.label65.Text = "24.";
		this.label65.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam3.Controls.Add(this.labelUpdateTable3);
		this.panelInitTeam3.Controls.Add(this.comboInitTeam3);
		this.panelInitTeam3.Controls.Add(this.label44);
		this.panelInitTeam3.Location = new System.Drawing.Point(5, 53);
		this.panelInitTeam3.Name = "panelInitTeam3";
		this.panelInitTeam3.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam3.TabIndex = 2;
		this.labelUpdateTable3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable3.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable3.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable3.Name = "labelUpdateTable3";
		this.labelUpdateTable3.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable3.TabIndex = 4;
		this.labelUpdateTable3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable3.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam3.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam3.FormattingEnabled = true;
		this.comboInitTeam3.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam3.Name = "comboInitTeam3";
		this.comboInitTeam3.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam3.Sorted = true;
		this.comboInitTeam3.TabIndex = 1;
		this.label44.Dock = System.Windows.Forms.DockStyle.Left;
		this.label44.Location = new System.Drawing.Point(0, 0);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(29, 25);
		this.label44.TabIndex = 0;
		this.label44.Text = " 3.";
		this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam23.Controls.Add(this.labelUpdateTable23);
		this.panelInitTeam23.Controls.Add(this.comboInitTeam23);
		this.panelInitTeam23.Controls.Add(this.label64);
		this.panelInitTeam23.Location = new System.Drawing.Point(6, 553);
		this.panelInitTeam23.Name = "panelInitTeam23";
		this.panelInitTeam23.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam23.TabIndex = 22;
		this.labelUpdateTable23.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable23.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable23.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable23.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable23.Name = "labelUpdateTable23";
		this.labelUpdateTable23.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable23.TabIndex = 4;
		this.labelUpdateTable23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable23.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam23.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam23.FormattingEnabled = true;
		this.comboInitTeam23.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam23.Name = "comboInitTeam23";
		this.comboInitTeam23.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam23.Sorted = true;
		this.comboInitTeam23.TabIndex = 1;
		this.label64.Dock = System.Windows.Forms.DockStyle.Left;
		this.label64.Location = new System.Drawing.Point(0, 0);
		this.label64.Name = "label64";
		this.label64.Size = new System.Drawing.Size(28, 25);
		this.label64.TabIndex = 0;
		this.label64.Text = "23.";
		this.label64.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam4.Controls.Add(this.labelUpdateTable4);
		this.panelInitTeam4.Controls.Add(this.comboInitTeam4);
		this.panelInitTeam4.Controls.Add(this.label45);
		this.panelInitTeam4.Location = new System.Drawing.Point(5, 78);
		this.panelInitTeam4.Name = "panelInitTeam4";
		this.panelInitTeam4.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam4.TabIndex = 3;
		this.labelUpdateTable4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable4.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable4.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable4.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable4.Name = "labelUpdateTable4";
		this.labelUpdateTable4.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable4.TabIndex = 4;
		this.labelUpdateTable4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable4.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam4.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam4.FormattingEnabled = true;
		this.comboInitTeam4.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam4.Name = "comboInitTeam4";
		this.comboInitTeam4.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam4.Sorted = true;
		this.comboInitTeam4.TabIndex = 1;
		this.label45.Dock = System.Windows.Forms.DockStyle.Left;
		this.label45.Location = new System.Drawing.Point(0, 0);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(29, 25);
		this.label45.TabIndex = 0;
		this.label45.Text = " 4.";
		this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam22.Controls.Add(this.labelUpdateTable22);
		this.panelInitTeam22.Controls.Add(this.comboInitTeam22);
		this.panelInitTeam22.Controls.Add(this.label63);
		this.panelInitTeam22.Location = new System.Drawing.Point(6, 528);
		this.panelInitTeam22.Name = "panelInitTeam22";
		this.panelInitTeam22.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam22.TabIndex = 21;
		this.labelUpdateTable22.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable22.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable22.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable22.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable22.Name = "labelUpdateTable22";
		this.labelUpdateTable22.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable22.TabIndex = 4;
		this.labelUpdateTable22.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable22.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam22.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam22.FormattingEnabled = true;
		this.comboInitTeam22.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam22.Name = "comboInitTeam22";
		this.comboInitTeam22.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam22.Sorted = true;
		this.comboInitTeam22.TabIndex = 1;
		this.label63.Dock = System.Windows.Forms.DockStyle.Left;
		this.label63.Location = new System.Drawing.Point(0, 0);
		this.label63.Name = "label63";
		this.label63.Size = new System.Drawing.Size(28, 25);
		this.label63.TabIndex = 0;
		this.label63.Text = "22.";
		this.label63.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam5.Controls.Add(this.labelUpdateTable5);
		this.panelInitTeam5.Controls.Add(this.comboInitTeam5);
		this.panelInitTeam5.Controls.Add(this.label46);
		this.panelInitTeam5.Location = new System.Drawing.Point(5, 103);
		this.panelInitTeam5.Name = "panelInitTeam5";
		this.panelInitTeam5.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam5.TabIndex = 4;
		this.labelUpdateTable5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable5.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable5.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable5.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable5.Name = "labelUpdateTable5";
		this.labelUpdateTable5.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable5.TabIndex = 4;
		this.labelUpdateTable5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable5.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam5.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam5.FormattingEnabled = true;
		this.comboInitTeam5.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam5.Name = "comboInitTeam5";
		this.comboInitTeam5.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam5.Sorted = true;
		this.comboInitTeam5.TabIndex = 1;
		this.label46.Dock = System.Windows.Forms.DockStyle.Left;
		this.label46.Location = new System.Drawing.Point(0, 0);
		this.label46.Name = "label46";
		this.label46.Size = new System.Drawing.Size(29, 25);
		this.label46.TabIndex = 0;
		this.label46.Text = " 5.";
		this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam21.Controls.Add(this.labelUpdateTable21);
		this.panelInitTeam21.Controls.Add(this.comboInitTeam21);
		this.panelInitTeam21.Controls.Add(this.label62);
		this.panelInitTeam21.Location = new System.Drawing.Point(6, 503);
		this.panelInitTeam21.Name = "panelInitTeam21";
		this.panelInitTeam21.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam21.TabIndex = 20;
		this.labelUpdateTable21.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable21.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable21.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable21.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable21.Name = "labelUpdateTable21";
		this.labelUpdateTable21.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable21.TabIndex = 4;
		this.labelUpdateTable21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable21.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam21.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam21.FormattingEnabled = true;
		this.comboInitTeam21.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam21.Name = "comboInitTeam21";
		this.comboInitTeam21.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam21.Sorted = true;
		this.comboInitTeam21.TabIndex = 1;
		this.label62.Dock = System.Windows.Forms.DockStyle.Left;
		this.label62.Location = new System.Drawing.Point(0, 0);
		this.label62.Name = "label62";
		this.label62.Size = new System.Drawing.Size(28, 25);
		this.label62.TabIndex = 0;
		this.label62.Text = "21.";
		this.label62.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam6.Controls.Add(this.labelUpdateTable6);
		this.panelInitTeam6.Controls.Add(this.comboInitTeam6);
		this.panelInitTeam6.Controls.Add(this.label47);
		this.panelInitTeam6.Location = new System.Drawing.Point(5, 128);
		this.panelInitTeam6.Name = "panelInitTeam6";
		this.panelInitTeam6.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam6.TabIndex = 5;
		this.labelUpdateTable6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable6.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable6.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable6.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable6.Name = "labelUpdateTable6";
		this.labelUpdateTable6.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable6.TabIndex = 4;
		this.labelUpdateTable6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable6.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam6.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam6.FormattingEnabled = true;
		this.comboInitTeam6.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam6.Name = "comboInitTeam6";
		this.comboInitTeam6.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam6.Sorted = true;
		this.comboInitTeam6.TabIndex = 1;
		this.label47.Dock = System.Windows.Forms.DockStyle.Left;
		this.label47.Location = new System.Drawing.Point(0, 0);
		this.label47.Name = "label47";
		this.label47.Size = new System.Drawing.Size(29, 25);
		this.label47.TabIndex = 0;
		this.label47.Text = " 6.";
		this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam20.Controls.Add(this.labelUpdateTable20);
		this.panelInitTeam20.Controls.Add(this.comboInitTeam20);
		this.panelInitTeam20.Controls.Add(this.label61);
		this.panelInitTeam20.Location = new System.Drawing.Point(6, 478);
		this.panelInitTeam20.Name = "panelInitTeam20";
		this.panelInitTeam20.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam20.TabIndex = 19;
		this.labelUpdateTable20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable20.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable20.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable20.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable20.Name = "labelUpdateTable20";
		this.labelUpdateTable20.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable20.TabIndex = 4;
		this.labelUpdateTable20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable20.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam20.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam20.FormattingEnabled = true;
		this.comboInitTeam20.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam20.Name = "comboInitTeam20";
		this.comboInitTeam20.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam20.Sorted = true;
		this.comboInitTeam20.TabIndex = 1;
		this.label61.Dock = System.Windows.Forms.DockStyle.Left;
		this.label61.Location = new System.Drawing.Point(0, 0);
		this.label61.Name = "label61";
		this.label61.Size = new System.Drawing.Size(28, 25);
		this.label61.TabIndex = 0;
		this.label61.Text = "20.";
		this.label61.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam7.Controls.Add(this.labelUpdateTable7);
		this.panelInitTeam7.Controls.Add(this.comboInitTeam7);
		this.panelInitTeam7.Controls.Add(this.label48);
		this.panelInitTeam7.Location = new System.Drawing.Point(5, 153);
		this.panelInitTeam7.Name = "panelInitTeam7";
		this.panelInitTeam7.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam7.TabIndex = 6;
		this.labelUpdateTable7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable7.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable7.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable7.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable7.Name = "labelUpdateTable7";
		this.labelUpdateTable7.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable7.TabIndex = 4;
		this.labelUpdateTable7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable7.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam7.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam7.FormattingEnabled = true;
		this.comboInitTeam7.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam7.Name = "comboInitTeam7";
		this.comboInitTeam7.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam7.Sorted = true;
		this.comboInitTeam7.TabIndex = 1;
		this.label48.Dock = System.Windows.Forms.DockStyle.Left;
		this.label48.Location = new System.Drawing.Point(0, 0);
		this.label48.Name = "label48";
		this.label48.Size = new System.Drawing.Size(29, 25);
		this.label48.TabIndex = 0;
		this.label48.Text = " 7.";
		this.label48.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam19.Controls.Add(this.labelUpdateTable19);
		this.panelInitTeam19.Controls.Add(this.comboInitTeam19);
		this.panelInitTeam19.Controls.Add(this.label60);
		this.panelInitTeam19.Location = new System.Drawing.Point(6, 453);
		this.panelInitTeam19.Name = "panelInitTeam19";
		this.panelInitTeam19.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam19.TabIndex = 18;
		this.labelUpdateTable19.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable19.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable19.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable19.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable19.Name = "labelUpdateTable19";
		this.labelUpdateTable19.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable19.TabIndex = 4;
		this.labelUpdateTable19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable19.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam19.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam19.FormattingEnabled = true;
		this.comboInitTeam19.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam19.Name = "comboInitTeam19";
		this.comboInitTeam19.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam19.Sorted = true;
		this.comboInitTeam19.TabIndex = 1;
		this.label60.Dock = System.Windows.Forms.DockStyle.Left;
		this.label60.Location = new System.Drawing.Point(0, 0);
		this.label60.Name = "label60";
		this.label60.Size = new System.Drawing.Size(28, 25);
		this.label60.TabIndex = 0;
		this.label60.Text = "19.";
		this.label60.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam8.Controls.Add(this.labelUpdateTable8);
		this.panelInitTeam8.Controls.Add(this.comboInitTeam8);
		this.panelInitTeam8.Controls.Add(this.label49);
		this.panelInitTeam8.Location = new System.Drawing.Point(5, 178);
		this.panelInitTeam8.Name = "panelInitTeam8";
		this.panelInitTeam8.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam8.TabIndex = 7;
		this.labelUpdateTable8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable8.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable8.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable8.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable8.Name = "labelUpdateTable8";
		this.labelUpdateTable8.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable8.TabIndex = 4;
		this.labelUpdateTable8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable8.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam8.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam8.FormattingEnabled = true;
		this.comboInitTeam8.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam8.Name = "comboInitTeam8";
		this.comboInitTeam8.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam8.Sorted = true;
		this.comboInitTeam8.TabIndex = 1;
		this.label49.Dock = System.Windows.Forms.DockStyle.Left;
		this.label49.Location = new System.Drawing.Point(0, 0);
		this.label49.Name = "label49";
		this.label49.Size = new System.Drawing.Size(29, 25);
		this.label49.TabIndex = 0;
		this.label49.Text = " 8.";
		this.label49.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam18.Controls.Add(this.labelUpdateTable18);
		this.panelInitTeam18.Controls.Add(this.comboInitTeam18);
		this.panelInitTeam18.Controls.Add(this.label59);
		this.panelInitTeam18.Location = new System.Drawing.Point(6, 428);
		this.panelInitTeam18.Name = "panelInitTeam18";
		this.panelInitTeam18.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam18.TabIndex = 17;
		this.labelUpdateTable18.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable18.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable18.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable18.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable18.Name = "labelUpdateTable18";
		this.labelUpdateTable18.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable18.TabIndex = 4;
		this.labelUpdateTable18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable18.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam18.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam18.FormattingEnabled = true;
		this.comboInitTeam18.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam18.Name = "comboInitTeam18";
		this.comboInitTeam18.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam18.Sorted = true;
		this.comboInitTeam18.TabIndex = 1;
		this.label59.Dock = System.Windows.Forms.DockStyle.Left;
		this.label59.Location = new System.Drawing.Point(0, 0);
		this.label59.Name = "label59";
		this.label59.Size = new System.Drawing.Size(28, 25);
		this.label59.TabIndex = 0;
		this.label59.Text = "18.";
		this.label59.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam9.Controls.Add(this.labelUpdateTable9);
		this.panelInitTeam9.Controls.Add(this.comboInitTeam9);
		this.panelInitTeam9.Controls.Add(this.label50);
		this.panelInitTeam9.Location = new System.Drawing.Point(5, 203);
		this.panelInitTeam9.Name = "panelInitTeam9";
		this.panelInitTeam9.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam9.TabIndex = 8;
		this.labelUpdateTable9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable9.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable9.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable9.Location = new System.Drawing.Point(29, 0);
		this.labelUpdateTable9.Name = "labelUpdateTable9";
		this.labelUpdateTable9.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable9.TabIndex = 4;
		this.labelUpdateTable9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable9.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam9.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam9.FormattingEnabled = true;
		this.comboInitTeam9.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam9.Name = "comboInitTeam9";
		this.comboInitTeam9.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam9.Sorted = true;
		this.comboInitTeam9.TabIndex = 1;
		this.label50.Dock = System.Windows.Forms.DockStyle.Left;
		this.label50.Location = new System.Drawing.Point(0, 0);
		this.label50.Name = "label50";
		this.label50.Size = new System.Drawing.Size(29, 25);
		this.label50.TabIndex = 0;
		this.label50.Text = " 9.";
		this.label50.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam17.Controls.Add(this.labelUpdateTable17);
		this.panelInitTeam17.Controls.Add(this.comboInitTeam17);
		this.panelInitTeam17.Controls.Add(this.label58);
		this.panelInitTeam17.Location = new System.Drawing.Point(6, 403);
		this.panelInitTeam17.Name = "panelInitTeam17";
		this.panelInitTeam17.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam17.TabIndex = 16;
		this.labelUpdateTable17.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable17.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable17.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable17.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable17.Name = "labelUpdateTable17";
		this.labelUpdateTable17.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable17.TabIndex = 4;
		this.labelUpdateTable17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable17.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam17.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam17.FormattingEnabled = true;
		this.comboInitTeam17.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam17.Name = "comboInitTeam17";
		this.comboInitTeam17.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam17.Sorted = true;
		this.comboInitTeam17.TabIndex = 1;
		this.label58.Dock = System.Windows.Forms.DockStyle.Left;
		this.label58.Location = new System.Drawing.Point(0, 0);
		this.label58.Name = "label58";
		this.label58.Size = new System.Drawing.Size(28, 25);
		this.label58.TabIndex = 0;
		this.label58.Text = "17.";
		this.label58.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam10.Controls.Add(this.labelUpdateTable10);
		this.panelInitTeam10.Controls.Add(this.comboInitTeam10);
		this.panelInitTeam10.Controls.Add(this.label51);
		this.panelInitTeam10.Location = new System.Drawing.Point(6, 228);
		this.panelInitTeam10.Name = "panelInitTeam10";
		this.panelInitTeam10.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam10.TabIndex = 9;
		this.labelUpdateTable10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable10.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable10.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable10.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable10.Name = "labelUpdateTable10";
		this.labelUpdateTable10.Size = new System.Drawing.Size(293, 25);
		this.labelUpdateTable10.TabIndex = 4;
		this.labelUpdateTable10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable10.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam10.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam10.FormattingEnabled = true;
		this.comboInitTeam10.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam10.Name = "comboInitTeam10";
		this.comboInitTeam10.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam10.Sorted = true;
		this.comboInitTeam10.TabIndex = 1;
		this.label51.Dock = System.Windows.Forms.DockStyle.Left;
		this.label51.Location = new System.Drawing.Point(0, 0);
		this.label51.Name = "label51";
		this.label51.Size = new System.Drawing.Size(28, 25);
		this.label51.TabIndex = 0;
		this.label51.Text = "10.";
		this.label51.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam16.Controls.Add(this.labelUpdateTable16);
		this.panelInitTeam16.Controls.Add(this.comboInitTeam16);
		this.panelInitTeam16.Controls.Add(this.label57);
		this.panelInitTeam16.Location = new System.Drawing.Point(6, 378);
		this.panelInitTeam16.Name = "panelInitTeam16";
		this.panelInitTeam16.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam16.TabIndex = 15;
		this.labelUpdateTable16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable16.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable16.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable16.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable16.Name = "labelUpdateTable16";
		this.labelUpdateTable16.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable16.TabIndex = 4;
		this.labelUpdateTable16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable16.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam16.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam16.FormattingEnabled = true;
		this.comboInitTeam16.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam16.Name = "comboInitTeam16";
		this.comboInitTeam16.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam16.Sorted = true;
		this.comboInitTeam16.TabIndex = 1;
		this.label57.Dock = System.Windows.Forms.DockStyle.Left;
		this.label57.Location = new System.Drawing.Point(0, 0);
		this.label57.Name = "label57";
		this.label57.Size = new System.Drawing.Size(28, 25);
		this.label57.TabIndex = 0;
		this.label57.Text = "16.";
		this.label57.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam11.Controls.Add(this.labelUpdateTable11);
		this.panelInitTeam11.Controls.Add(this.comboInitTeam11);
		this.panelInitTeam11.Controls.Add(this.label52);
		this.panelInitTeam11.Location = new System.Drawing.Point(6, 253);
		this.panelInitTeam11.Name = "panelInitTeam11";
		this.panelInitTeam11.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam11.TabIndex = 10;
		this.labelUpdateTable11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable11.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable11.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable11.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable11.Name = "labelUpdateTable11";
		this.labelUpdateTable11.Size = new System.Drawing.Size(293, 25);
		this.labelUpdateTable11.TabIndex = 4;
		this.labelUpdateTable11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable11.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam11.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam11.FormattingEnabled = true;
		this.comboInitTeam11.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam11.Name = "comboInitTeam11";
		this.comboInitTeam11.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam11.Sorted = true;
		this.comboInitTeam11.TabIndex = 1;
		this.label52.Dock = System.Windows.Forms.DockStyle.Left;
		this.label52.Location = new System.Drawing.Point(0, 0);
		this.label52.Name = "label52";
		this.label52.Size = new System.Drawing.Size(28, 25);
		this.label52.TabIndex = 0;
		this.label52.Text = "11.";
		this.label52.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam15.Controls.Add(this.labelUpdateTable15);
		this.panelInitTeam15.Controls.Add(this.comboInitTeam15);
		this.panelInitTeam15.Controls.Add(this.label56);
		this.panelInitTeam15.Location = new System.Drawing.Point(6, 353);
		this.panelInitTeam15.Name = "panelInitTeam15";
		this.panelInitTeam15.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam15.TabIndex = 14;
		this.labelUpdateTable15.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable15.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable15.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable15.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable15.Name = "labelUpdateTable15";
		this.labelUpdateTable15.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable15.TabIndex = 4;
		this.labelUpdateTable15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable15.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam15.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam15.FormattingEnabled = true;
		this.comboInitTeam15.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam15.Name = "comboInitTeam15";
		this.comboInitTeam15.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam15.Sorted = true;
		this.comboInitTeam15.TabIndex = 1;
		this.label56.Dock = System.Windows.Forms.DockStyle.Left;
		this.label56.Location = new System.Drawing.Point(0, 0);
		this.label56.Name = "label56";
		this.label56.Size = new System.Drawing.Size(28, 25);
		this.label56.TabIndex = 0;
		this.label56.Text = "15.";
		this.label56.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam12.Controls.Add(this.labelUpdateTable12);
		this.panelInitTeam12.Controls.Add(this.comboInitTeam12);
		this.panelInitTeam12.Controls.Add(this.label53);
		this.panelInitTeam12.Location = new System.Drawing.Point(6, 278);
		this.panelInitTeam12.Name = "panelInitTeam12";
		this.panelInitTeam12.Size = new System.Drawing.Size(501, 25);
		this.panelInitTeam12.TabIndex = 11;
		this.labelUpdateTable12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable12.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable12.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable12.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable12.Name = "labelUpdateTable12";
		this.labelUpdateTable12.Size = new System.Drawing.Size(293, 25);
		this.labelUpdateTable12.TabIndex = 4;
		this.labelUpdateTable12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable12.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam12.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam12.FormattingEnabled = true;
		this.comboInitTeam12.Location = new System.Drawing.Point(321, 0);
		this.comboInitTeam12.Name = "comboInitTeam12";
		this.comboInitTeam12.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam12.Sorted = true;
		this.comboInitTeam12.TabIndex = 1;
		this.label53.Dock = System.Windows.Forms.DockStyle.Left;
		this.label53.Location = new System.Drawing.Point(0, 0);
		this.label53.Name = "label53";
		this.label53.Size = new System.Drawing.Size(28, 25);
		this.label53.TabIndex = 0;
		this.label53.Text = "12.";
		this.label53.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam14.Controls.Add(this.labelUpdateTable14);
		this.panelInitTeam14.Controls.Add(this.comboInitTeam14);
		this.panelInitTeam14.Controls.Add(this.label55);
		this.panelInitTeam14.Location = new System.Drawing.Point(6, 328);
		this.panelInitTeam14.Name = "panelInitTeam14";
		this.panelInitTeam14.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam14.TabIndex = 13;
		this.labelUpdateTable14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable14.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable14.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable14.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable14.Name = "labelUpdateTable14";
		this.labelUpdateTable14.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable14.TabIndex = 4;
		this.labelUpdateTable14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable14.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam14.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam14.FormattingEnabled = true;
		this.comboInitTeam14.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam14.Name = "comboInitTeam14";
		this.comboInitTeam14.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam14.Sorted = true;
		this.comboInitTeam14.TabIndex = 1;
		this.label55.Dock = System.Windows.Forms.DockStyle.Left;
		this.label55.Location = new System.Drawing.Point(0, 0);
		this.label55.Name = "label55";
		this.label55.Size = new System.Drawing.Size(28, 25);
		this.label55.TabIndex = 0;
		this.label55.Text = "14.";
		this.label55.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam13.Controls.Add(this.labelUpdateTable13);
		this.panelInitTeam13.Controls.Add(this.comboInitTeam13);
		this.panelInitTeam13.Controls.Add(this.label54);
		this.panelInitTeam13.Location = new System.Drawing.Point(6, 303);
		this.panelInitTeam13.Name = "panelInitTeam13";
		this.panelInitTeam13.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam13.TabIndex = 12;
		this.labelUpdateTable13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable13.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable13.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable13.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable13.Name = "labelUpdateTable13";
		this.labelUpdateTable13.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable13.TabIndex = 4;
		this.labelUpdateTable13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable13.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam13.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam13.FormattingEnabled = true;
		this.comboInitTeam13.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam13.Name = "comboInitTeam13";
		this.comboInitTeam13.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam13.Sorted = true;
		this.comboInitTeam13.TabIndex = 1;
		this.label54.Dock = System.Windows.Forms.DockStyle.Left;
		this.label54.Location = new System.Drawing.Point(0, 0);
		this.label54.Name = "label54";
		this.label54.Size = new System.Drawing.Size(28, 25);
		this.label54.TabIndex = 0;
		this.label54.Text = "13.";
		this.label54.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam25.Controls.Add(this.labelUpdateTable25);
		this.panelInitTeam25.Controls.Add(this.comboInitTeam25);
		this.panelInitTeam25.Controls.Add(this.label32);
		this.panelInitTeam25.Location = new System.Drawing.Point(6, 603);
		this.panelInitTeam25.Name = "panelInitTeam25";
		this.panelInitTeam25.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam25.TabIndex = 24;
		this.labelUpdateTable25.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable25.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable25.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable25.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable25.Name = "labelUpdateTable25";
		this.labelUpdateTable25.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable25.TabIndex = 4;
		this.labelUpdateTable25.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable25.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam25.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam25.FormattingEnabled = true;
		this.comboInitTeam25.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam25.Name = "comboInitTeam25";
		this.comboInitTeam25.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam25.Sorted = true;
		this.comboInitTeam25.TabIndex = 1;
		this.label32.Dock = System.Windows.Forms.DockStyle.Left;
		this.label32.Location = new System.Drawing.Point(0, 0);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(28, 25);
		this.label32.TabIndex = 0;
		this.label32.Text = "25.";
		this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam26.Controls.Add(this.labelUpdateTable26);
		this.panelInitTeam26.Controls.Add(this.comboInitTeam26);
		this.panelInitTeam26.Controls.Add(this.label33);
		this.panelInitTeam26.Location = new System.Drawing.Point(6, 628);
		this.panelInitTeam26.Name = "panelInitTeam26";
		this.panelInitTeam26.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam26.TabIndex = 25;
		this.labelUpdateTable26.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable26.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable26.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable26.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable26.Name = "labelUpdateTable26";
		this.labelUpdateTable26.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable26.TabIndex = 4;
		this.labelUpdateTable26.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable26.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam26.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam26.FormattingEnabled = true;
		this.comboInitTeam26.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam26.Name = "comboInitTeam26";
		this.comboInitTeam26.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam26.Sorted = true;
		this.comboInitTeam26.TabIndex = 1;
		this.label33.Dock = System.Windows.Forms.DockStyle.Left;
		this.label33.Location = new System.Drawing.Point(0, 0);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(28, 25);
		this.label33.TabIndex = 0;
		this.label33.Text = "26.";
		this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam27.Controls.Add(this.labelUpdateTable27);
		this.panelInitTeam27.Controls.Add(this.comboInitTeam27);
		this.panelInitTeam27.Controls.Add(this.label127);
		this.panelInitTeam27.Location = new System.Drawing.Point(6, 653);
		this.panelInitTeam27.Name = "panelInitTeam27";
		this.panelInitTeam27.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam27.TabIndex = 27;
		this.labelUpdateTable27.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable27.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable27.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable27.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable27.Name = "labelUpdateTable27";
		this.labelUpdateTable27.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable27.TabIndex = 4;
		this.labelUpdateTable27.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable27.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam27.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam27.FormattingEnabled = true;
		this.comboInitTeam27.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam27.Name = "comboInitTeam27";
		this.comboInitTeam27.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam27.Sorted = true;
		this.comboInitTeam27.TabIndex = 1;
		this.label127.Dock = System.Windows.Forms.DockStyle.Left;
		this.label127.Location = new System.Drawing.Point(0, 0);
		this.label127.Name = "label127";
		this.label127.Size = new System.Drawing.Size(28, 25);
		this.label127.TabIndex = 0;
		this.label127.Text = "27.";
		this.label127.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam28.Controls.Add(this.labelUpdateTable28);
		this.panelInitTeam28.Controls.Add(this.comboInitTeam28);
		this.panelInitTeam28.Controls.Add(this.label128);
		this.panelInitTeam28.Location = new System.Drawing.Point(6, 678);
		this.panelInitTeam28.Name = "panelInitTeam28";
		this.panelInitTeam28.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam28.TabIndex = 28;
		this.labelUpdateTable28.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable28.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable28.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable28.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable28.Name = "labelUpdateTable28";
		this.labelUpdateTable28.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable28.TabIndex = 4;
		this.labelUpdateTable28.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable28.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam28.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam28.FormattingEnabled = true;
		this.comboInitTeam28.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam28.Name = "comboInitTeam28";
		this.comboInitTeam28.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam28.Sorted = true;
		this.comboInitTeam28.TabIndex = 1;
		this.label128.Dock = System.Windows.Forms.DockStyle.Left;
		this.label128.Location = new System.Drawing.Point(0, 0);
		this.label128.Name = "label128";
		this.label128.Size = new System.Drawing.Size(28, 25);
		this.label128.TabIndex = 0;
		this.label128.Text = "28.";
		this.label128.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam29.Controls.Add(this.labelUpdateTable29);
		this.panelInitTeam29.Controls.Add(this.comboInitTeam29);
		this.panelInitTeam29.Controls.Add(this.label129);
		this.panelInitTeam29.Location = new System.Drawing.Point(6, 703);
		this.panelInitTeam29.Name = "panelInitTeam29";
		this.panelInitTeam29.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam29.TabIndex = 29;
		this.labelUpdateTable29.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable29.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable29.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable29.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable29.Name = "labelUpdateTable29";
		this.labelUpdateTable29.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable29.TabIndex = 4;
		this.labelUpdateTable29.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable29.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam29.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam29.FormattingEnabled = true;
		this.comboInitTeam29.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam29.Name = "comboInitTeam29";
		this.comboInitTeam29.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam29.Sorted = true;
		this.comboInitTeam29.TabIndex = 1;
		this.label129.Dock = System.Windows.Forms.DockStyle.Left;
		this.label129.Location = new System.Drawing.Point(0, 0);
		this.label129.Name = "label129";
		this.label129.Size = new System.Drawing.Size(28, 25);
		this.label129.TabIndex = 0;
		this.label129.Text = "29.";
		this.label129.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam30.Controls.Add(this.labelUpdateTable30);
		this.panelInitTeam30.Controls.Add(this.comboInitTeam30);
		this.panelInitTeam30.Controls.Add(this.label130);
		this.panelInitTeam30.Location = new System.Drawing.Point(6, 728);
		this.panelInitTeam30.Name = "panelInitTeam30";
		this.panelInitTeam30.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam30.TabIndex = 30;
		this.labelUpdateTable30.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable30.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable30.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable30.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable30.Name = "labelUpdateTable30";
		this.labelUpdateTable30.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable30.TabIndex = 4;
		this.labelUpdateTable30.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable30.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam30.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam30.FormattingEnabled = true;
		this.comboInitTeam30.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam30.Name = "comboInitTeam30";
		this.comboInitTeam30.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam30.Sorted = true;
		this.comboInitTeam30.TabIndex = 1;
		this.label130.Dock = System.Windows.Forms.DockStyle.Left;
		this.label130.Location = new System.Drawing.Point(0, 0);
		this.label130.Name = "label130";
		this.label130.Size = new System.Drawing.Size(28, 25);
		this.label130.TabIndex = 0;
		this.label130.Text = "30.";
		this.label130.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam31.Controls.Add(this.labelUpdateTable31);
		this.panelInitTeam31.Controls.Add(this.comboInitTeam31);
		this.panelInitTeam31.Controls.Add(this.label131);
		this.panelInitTeam31.Location = new System.Drawing.Point(6, 753);
		this.panelInitTeam31.Name = "panelInitTeam31";
		this.panelInitTeam31.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam31.TabIndex = 31;
		this.labelUpdateTable31.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable31.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable31.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable31.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable31.Name = "labelUpdateTable31";
		this.labelUpdateTable31.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable31.TabIndex = 4;
		this.labelUpdateTable31.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable31.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam31.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam31.FormattingEnabled = true;
		this.comboInitTeam31.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam31.Name = "comboInitTeam31";
		this.comboInitTeam31.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam31.Sorted = true;
		this.comboInitTeam31.TabIndex = 1;
		this.label131.Dock = System.Windows.Forms.DockStyle.Left;
		this.label131.Location = new System.Drawing.Point(0, 0);
		this.label131.Name = "label131";
		this.label131.Size = new System.Drawing.Size(28, 25);
		this.label131.TabIndex = 0;
		this.label131.Text = "31.";
		this.label131.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam32.Controls.Add(this.labelUpdateTable32);
		this.panelInitTeam32.Controls.Add(this.comboInitTeam32);
		this.panelInitTeam32.Controls.Add(this.label132);
		this.panelInitTeam32.Location = new System.Drawing.Point(6, 778);
		this.panelInitTeam32.Name = "panelInitTeam32";
		this.panelInitTeam32.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam32.TabIndex = 32;
		this.labelUpdateTable32.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable32.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable32.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable32.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable32.Name = "labelUpdateTable32";
		this.labelUpdateTable32.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable32.TabIndex = 4;
		this.labelUpdateTable32.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable32.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam32.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam32.FormattingEnabled = true;
		this.comboInitTeam32.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam32.Name = "comboInitTeam32";
		this.comboInitTeam32.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam32.Sorted = true;
		this.comboInitTeam32.TabIndex = 1;
		this.label132.Dock = System.Windows.Forms.DockStyle.Left;
		this.label132.Location = new System.Drawing.Point(0, 0);
		this.label132.Name = "label132";
		this.label132.Size = new System.Drawing.Size(28, 25);
		this.label132.TabIndex = 0;
		this.label132.Text = "32.";
		this.label132.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam33.Controls.Add(this.labelUpdateTable33);
		this.panelInitTeam33.Controls.Add(this.comboInitTeam33);
		this.panelInitTeam33.Controls.Add(this.label133);
		this.panelInitTeam33.Location = new System.Drawing.Point(6, 803);
		this.panelInitTeam33.Name = "panelInitTeam33";
		this.panelInitTeam33.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam33.TabIndex = 33;
		this.labelUpdateTable33.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable33.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable33.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable33.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable33.Name = "labelUpdateTable33";
		this.labelUpdateTable33.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable33.TabIndex = 4;
		this.labelUpdateTable33.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable33.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam33.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam33.FormattingEnabled = true;
		this.comboInitTeam33.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam33.Name = "comboInitTeam33";
		this.comboInitTeam33.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam33.Sorted = true;
		this.comboInitTeam33.TabIndex = 1;
		this.label133.Dock = System.Windows.Forms.DockStyle.Left;
		this.label133.Location = new System.Drawing.Point(0, 0);
		this.label133.Name = "label133";
		this.label133.Size = new System.Drawing.Size(28, 25);
		this.label133.TabIndex = 0;
		this.label133.Text = "33.";
		this.label133.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam34.Controls.Add(this.labelUpdateTable34);
		this.panelInitTeam34.Controls.Add(this.comboInitTeam34);
		this.panelInitTeam34.Controls.Add(this.label134);
		this.panelInitTeam34.Location = new System.Drawing.Point(6, 828);
		this.panelInitTeam34.Name = "panelInitTeam34";
		this.panelInitTeam34.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam34.TabIndex = 34;
		this.labelUpdateTable34.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable34.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable34.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable34.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable34.Name = "labelUpdateTable34";
		this.labelUpdateTable34.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable34.TabIndex = 4;
		this.labelUpdateTable34.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable34.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam34.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam34.FormattingEnabled = true;
		this.comboInitTeam34.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam34.Name = "comboInitTeam34";
		this.comboInitTeam34.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam34.Sorted = true;
		this.comboInitTeam34.TabIndex = 1;
		this.label134.Dock = System.Windows.Forms.DockStyle.Left;
		this.label134.Location = new System.Drawing.Point(0, 0);
		this.label134.Name = "label134";
		this.label134.Size = new System.Drawing.Size(28, 25);
		this.label134.TabIndex = 0;
		this.label134.Text = "34.";
		this.label134.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam35.Controls.Add(this.labelUpdateTable35);
		this.panelInitTeam35.Controls.Add(this.comboInitTeam35);
		this.panelInitTeam35.Controls.Add(this.label135);
		this.panelInitTeam35.Location = new System.Drawing.Point(6, 853);
		this.panelInitTeam35.Name = "panelInitTeam35";
		this.panelInitTeam35.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam35.TabIndex = 35;
		this.labelUpdateTable35.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable35.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable35.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable35.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable35.Name = "labelUpdateTable35";
		this.labelUpdateTable35.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable35.TabIndex = 4;
		this.labelUpdateTable35.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable35.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam35.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam35.FormattingEnabled = true;
		this.comboInitTeam35.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam35.Name = "comboInitTeam35";
		this.comboInitTeam35.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam35.Sorted = true;
		this.comboInitTeam35.TabIndex = 1;
		this.label135.Dock = System.Windows.Forms.DockStyle.Left;
		this.label135.Location = new System.Drawing.Point(0, 0);
		this.label135.Name = "label135";
		this.label135.Size = new System.Drawing.Size(28, 25);
		this.label135.TabIndex = 0;
		this.label135.Text = "35.";
		this.label135.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam36.Controls.Add(this.labelUpdateTable36);
		this.panelInitTeam36.Controls.Add(this.comboInitTeam36);
		this.panelInitTeam36.Controls.Add(this.label136);
		this.panelInitTeam36.Location = new System.Drawing.Point(6, 878);
		this.panelInitTeam36.Name = "panelInitTeam36";
		this.panelInitTeam36.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam36.TabIndex = 36;
		this.labelUpdateTable36.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable36.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable36.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable36.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable36.Name = "labelUpdateTable36";
		this.labelUpdateTable36.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable36.TabIndex = 4;
		this.labelUpdateTable36.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable36.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam36.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam36.FormattingEnabled = true;
		this.comboInitTeam36.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam36.Name = "comboInitTeam36";
		this.comboInitTeam36.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam36.Sorted = true;
		this.comboInitTeam36.TabIndex = 1;
		this.label136.Dock = System.Windows.Forms.DockStyle.Left;
		this.label136.Location = new System.Drawing.Point(0, 0);
		this.label136.Name = "label136";
		this.label136.Size = new System.Drawing.Size(28, 25);
		this.label136.TabIndex = 0;
		this.label136.Text = "36.";
		this.label136.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam37.Controls.Add(this.labelUpdateTable37);
		this.panelInitTeam37.Controls.Add(this.comboInitTeam37);
		this.panelInitTeam37.Controls.Add(this.label137);
		this.panelInitTeam37.Location = new System.Drawing.Point(6, 903);
		this.panelInitTeam37.Name = "panelInitTeam37";
		this.panelInitTeam37.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam37.TabIndex = 37;
		this.labelUpdateTable37.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable37.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable37.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable37.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable37.Name = "labelUpdateTable37";
		this.labelUpdateTable37.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable37.TabIndex = 4;
		this.labelUpdateTable37.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable37.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam37.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam37.FormattingEnabled = true;
		this.comboInitTeam37.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam37.Name = "comboInitTeam37";
		this.comboInitTeam37.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam37.Sorted = true;
		this.comboInitTeam37.TabIndex = 1;
		this.label137.Dock = System.Windows.Forms.DockStyle.Left;
		this.label137.Location = new System.Drawing.Point(0, 0);
		this.label137.Name = "label137";
		this.label137.Size = new System.Drawing.Size(28, 25);
		this.label137.TabIndex = 0;
		this.label137.Text = "37.";
		this.label137.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam38.Controls.Add(this.labelUpdateTable38);
		this.panelInitTeam38.Controls.Add(this.comboInitTeam38);
		this.panelInitTeam38.Controls.Add(this.label138);
		this.panelInitTeam38.Location = new System.Drawing.Point(6, 928);
		this.panelInitTeam38.Name = "panelInitTeam38";
		this.panelInitTeam38.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam38.TabIndex = 38;
		this.labelUpdateTable38.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable38.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable38.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable38.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable38.Name = "labelUpdateTable38";
		this.labelUpdateTable38.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable38.TabIndex = 4;
		this.labelUpdateTable38.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable38.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam38.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam38.FormattingEnabled = true;
		this.comboInitTeam38.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam38.Name = "comboInitTeam38";
		this.comboInitTeam38.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam38.Sorted = true;
		this.comboInitTeam38.TabIndex = 1;
		this.label138.Dock = System.Windows.Forms.DockStyle.Left;
		this.label138.Location = new System.Drawing.Point(0, 0);
		this.label138.Name = "label138";
		this.label138.Size = new System.Drawing.Size(28, 25);
		this.label138.TabIndex = 0;
		this.label138.Text = "38.";
		this.label138.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam39.Controls.Add(this.labelUpdateTable39);
		this.panelInitTeam39.Controls.Add(this.comboInitTeam39);
		this.panelInitTeam39.Controls.Add(this.label139);
		this.panelInitTeam39.Location = new System.Drawing.Point(6, 953);
		this.panelInitTeam39.Name = "panelInitTeam39";
		this.panelInitTeam39.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam39.TabIndex = 39;
		this.labelUpdateTable39.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable39.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable39.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable39.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable39.Name = "labelUpdateTable39";
		this.labelUpdateTable39.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable39.TabIndex = 4;
		this.labelUpdateTable39.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable39.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam39.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam39.FormattingEnabled = true;
		this.comboInitTeam39.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam39.Name = "comboInitTeam39";
		this.comboInitTeam39.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam39.Sorted = true;
		this.comboInitTeam39.TabIndex = 1;
		this.label139.Dock = System.Windows.Forms.DockStyle.Left;
		this.label139.Location = new System.Drawing.Point(0, 0);
		this.label139.Name = "label139";
		this.label139.Size = new System.Drawing.Size(28, 25);
		this.label139.TabIndex = 0;
		this.label139.Text = "39.";
		this.label139.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam40.Controls.Add(this.labelUpdateTable40);
		this.panelInitTeam40.Controls.Add(this.comboInitTeam40);
		this.panelInitTeam40.Controls.Add(this.label140);
		this.panelInitTeam40.Location = new System.Drawing.Point(6, 978);
		this.panelInitTeam40.Name = "panelInitTeam40";
		this.panelInitTeam40.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam40.TabIndex = 40;
		this.labelUpdateTable40.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable40.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable40.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable40.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable40.Name = "labelUpdateTable40";
		this.labelUpdateTable40.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable40.TabIndex = 4;
		this.labelUpdateTable40.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable40.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam40.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam40.FormattingEnabled = true;
		this.comboInitTeam40.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam40.Name = "comboInitTeam40";
		this.comboInitTeam40.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam40.Sorted = true;
		this.comboInitTeam40.TabIndex = 1;
		this.label140.Dock = System.Windows.Forms.DockStyle.Left;
		this.label140.Location = new System.Drawing.Point(0, 0);
		this.label140.Name = "label140";
		this.label140.Size = new System.Drawing.Size(28, 25);
		this.label140.TabIndex = 0;
		this.label140.Text = "40.";
		this.label140.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam41.Controls.Add(this.labelUpdateTable41);
		this.panelInitTeam41.Controls.Add(this.comboInitTeam41);
		this.panelInitTeam41.Controls.Add(this.label141);
		this.panelInitTeam41.Location = new System.Drawing.Point(6, 1003);
		this.panelInitTeam41.Name = "panelInitTeam41";
		this.panelInitTeam41.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam41.TabIndex = 41;
		this.labelUpdateTable41.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable41.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable41.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable41.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable41.Name = "labelUpdateTable41";
		this.labelUpdateTable41.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable41.TabIndex = 4;
		this.labelUpdateTable41.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable41.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam41.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam41.FormattingEnabled = true;
		this.comboInitTeam41.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam41.Name = "comboInitTeam41";
		this.comboInitTeam41.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam41.Sorted = true;
		this.comboInitTeam41.TabIndex = 1;
		this.label141.Dock = System.Windows.Forms.DockStyle.Left;
		this.label141.Location = new System.Drawing.Point(0, 0);
		this.label141.Name = "label141";
		this.label141.Size = new System.Drawing.Size(28, 25);
		this.label141.TabIndex = 0;
		this.label141.Text = "41.";
		this.label141.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam42.Controls.Add(this.labelUpdateTable42);
		this.panelInitTeam42.Controls.Add(this.comboInitTeam42);
		this.panelInitTeam42.Controls.Add(this.label142);
		this.panelInitTeam42.Location = new System.Drawing.Point(6, 1028);
		this.panelInitTeam42.Name = "panelInitTeam42";
		this.panelInitTeam42.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam42.TabIndex = 42;
		this.labelUpdateTable42.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable42.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable42.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable42.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable42.Name = "labelUpdateTable42";
		this.labelUpdateTable42.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable42.TabIndex = 4;
		this.labelUpdateTable42.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable42.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam42.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam42.FormattingEnabled = true;
		this.comboInitTeam42.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam42.Name = "comboInitTeam42";
		this.comboInitTeam42.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam42.Sorted = true;
		this.comboInitTeam42.TabIndex = 1;
		this.label142.Dock = System.Windows.Forms.DockStyle.Left;
		this.label142.Location = new System.Drawing.Point(0, 0);
		this.label142.Name = "label142";
		this.label142.Size = new System.Drawing.Size(28, 25);
		this.label142.TabIndex = 0;
		this.label142.Text = "42.";
		this.label142.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam43.Controls.Add(this.labelUpdateTable43);
		this.panelInitTeam43.Controls.Add(this.comboInitTeam43);
		this.panelInitTeam43.Controls.Add(this.label143);
		this.panelInitTeam43.Location = new System.Drawing.Point(6, 1053);
		this.panelInitTeam43.Name = "panelInitTeam43";
		this.panelInitTeam43.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam43.TabIndex = 43;
		this.labelUpdateTable43.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable43.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable43.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable43.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable43.Name = "labelUpdateTable43";
		this.labelUpdateTable43.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable43.TabIndex = 4;
		this.labelUpdateTable43.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable43.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam43.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam43.FormattingEnabled = true;
		this.comboInitTeam43.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam43.Name = "comboInitTeam43";
		this.comboInitTeam43.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam43.Sorted = true;
		this.comboInitTeam43.TabIndex = 1;
		this.label143.Dock = System.Windows.Forms.DockStyle.Left;
		this.label143.Location = new System.Drawing.Point(0, 0);
		this.label143.Name = "label143";
		this.label143.Size = new System.Drawing.Size(28, 25);
		this.label143.TabIndex = 0;
		this.label143.Text = "43.";
		this.label143.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam44.Controls.Add(this.labelUpdateTable44);
		this.panelInitTeam44.Controls.Add(this.comboInitTeam44);
		this.panelInitTeam44.Controls.Add(this.label144);
		this.panelInitTeam44.Location = new System.Drawing.Point(6, 1078);
		this.panelInitTeam44.Name = "panelInitTeam44";
		this.panelInitTeam44.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam44.TabIndex = 44;
		this.labelUpdateTable44.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable44.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable44.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable44.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable44.Name = "labelUpdateTable44";
		this.labelUpdateTable44.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable44.TabIndex = 4;
		this.labelUpdateTable44.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable44.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam44.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam44.FormattingEnabled = true;
		this.comboInitTeam44.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam44.Name = "comboInitTeam44";
		this.comboInitTeam44.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam44.Sorted = true;
		this.comboInitTeam44.TabIndex = 1;
		this.label144.Dock = System.Windows.Forms.DockStyle.Left;
		this.label144.Location = new System.Drawing.Point(0, 0);
		this.label144.Name = "label144";
		this.label144.Size = new System.Drawing.Size(28, 25);
		this.label144.TabIndex = 0;
		this.label144.Text = "44.";
		this.label144.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam45.Controls.Add(this.labelUpdateTable45);
		this.panelInitTeam45.Controls.Add(this.comboInitTeam45);
		this.panelInitTeam45.Controls.Add(this.label145);
		this.panelInitTeam45.Location = new System.Drawing.Point(6, 1103);
		this.panelInitTeam45.Name = "panelInitTeam45";
		this.panelInitTeam45.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam45.TabIndex = 45;
		this.labelUpdateTable45.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable45.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable45.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable45.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable45.Name = "labelUpdateTable45";
		this.labelUpdateTable45.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable45.TabIndex = 4;
		this.labelUpdateTable45.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable45.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam45.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam45.FormattingEnabled = true;
		this.comboInitTeam45.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam45.Name = "comboInitTeam45";
		this.comboInitTeam45.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam45.Sorted = true;
		this.comboInitTeam45.TabIndex = 1;
		this.label145.Dock = System.Windows.Forms.DockStyle.Left;
		this.label145.Location = new System.Drawing.Point(0, 0);
		this.label145.Name = "label145";
		this.label145.Size = new System.Drawing.Size(28, 25);
		this.label145.TabIndex = 0;
		this.label145.Text = "45.";
		this.label145.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam46.Controls.Add(this.labelUpdateTable46);
		this.panelInitTeam46.Controls.Add(this.comboInitTeam46);
		this.panelInitTeam46.Controls.Add(this.label146);
		this.panelInitTeam46.Location = new System.Drawing.Point(6, 1128);
		this.panelInitTeam46.Name = "panelInitTeam46";
		this.panelInitTeam46.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam46.TabIndex = 46;
		this.labelUpdateTable46.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable46.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable46.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable46.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable46.Name = "labelUpdateTable46";
		this.labelUpdateTable46.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable46.TabIndex = 4;
		this.labelUpdateTable46.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable46.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam46.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam46.FormattingEnabled = true;
		this.comboInitTeam46.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam46.Name = "comboInitTeam46";
		this.comboInitTeam46.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam46.Sorted = true;
		this.comboInitTeam46.TabIndex = 1;
		this.label146.Dock = System.Windows.Forms.DockStyle.Left;
		this.label146.Location = new System.Drawing.Point(0, 0);
		this.label146.Name = "label146";
		this.label146.Size = new System.Drawing.Size(28, 25);
		this.label146.TabIndex = 0;
		this.label146.Text = "46.";
		this.label146.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam47.Controls.Add(this.labelUpdateTable47);
		this.panelInitTeam47.Controls.Add(this.comboInitTeam47);
		this.panelInitTeam47.Controls.Add(this.label147);
		this.panelInitTeam47.Location = new System.Drawing.Point(6, 1153);
		this.panelInitTeam47.Name = "panelInitTeam47";
		this.panelInitTeam47.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam47.TabIndex = 47;
		this.labelUpdateTable47.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable47.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable47.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable47.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable47.Name = "labelUpdateTable47";
		this.labelUpdateTable47.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable47.TabIndex = 4;
		this.labelUpdateTable47.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable47.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam47.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam47.FormattingEnabled = true;
		this.comboInitTeam47.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam47.Name = "comboInitTeam47";
		this.comboInitTeam47.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam47.Sorted = true;
		this.comboInitTeam47.TabIndex = 1;
		this.label147.Dock = System.Windows.Forms.DockStyle.Left;
		this.label147.Location = new System.Drawing.Point(0, 0);
		this.label147.Name = "label147";
		this.label147.Size = new System.Drawing.Size(28, 25);
		this.label147.TabIndex = 0;
		this.label147.Text = "47.";
		this.label147.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panelInitTeam48.Controls.Add(this.labelUpdateTable48);
		this.panelInitTeam48.Controls.Add(this.comboInitTeam48);
		this.panelInitTeam48.Controls.Add(this.label148);
		this.panelInitTeam48.Location = new System.Drawing.Point(6, 1178);
		this.panelInitTeam48.Name = "panelInitTeam48";
		this.panelInitTeam48.Size = new System.Drawing.Size(500, 25);
		this.panelInitTeam48.TabIndex = 48;
		this.labelUpdateTable48.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelUpdateTable48.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelUpdateTable48.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelUpdateTable48.Location = new System.Drawing.Point(28, 0);
		this.labelUpdateTable48.Name = "labelUpdateTable48";
		this.labelUpdateTable48.Size = new System.Drawing.Size(292, 25);
		this.labelUpdateTable48.TabIndex = 4;
		this.labelUpdateTable48.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateTable48.Click += new System.EventHandler(labelUpdateTable_Click);
		this.comboInitTeam48.Dock = System.Windows.Forms.DockStyle.Right;
		this.comboInitTeam48.FormattingEnabled = true;
		this.comboInitTeam48.Location = new System.Drawing.Point(320, 0);
		this.comboInitTeam48.Name = "comboInitTeam48";
		this.comboInitTeam48.Size = new System.Drawing.Size(180, 21);
		this.comboInitTeam48.Sorted = true;
		this.comboInitTeam48.TabIndex = 1;
		this.label148.Dock = System.Windows.Forms.DockStyle.Left;
		this.label148.Location = new System.Drawing.Point(0, 0);
		this.label148.Name = "label148";
		this.label148.Size = new System.Drawing.Size(28, 25);
		this.label148.TabIndex = 0;
		this.label148.Text = "48.";
		this.label148.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.tabPageTrophyGraphics.Controls.Add(this.groupGraphics);
		this.tabPageTrophyGraphics.Location = new System.Drawing.Point(4, 22);
		this.tabPageTrophyGraphics.Name = "tabPageTrophyGraphics";
		this.tabPageTrophyGraphics.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageTrophyGraphics.Size = new System.Drawing.Size(735, 698);
		this.tabPageTrophyGraphics.TabIndex = 1;
		this.tabPageTrophyGraphics.Text = "Trophy Graphics";
		this.tabPageTrophyGraphics.UseVisualStyleBackColor = true;
		this.groupGraphics.Controls.Add(this.buttonReplicateTropy);
		this.groupGraphics.Controls.Add(this.viewer2DTrophy);
		this.groupGraphics.Controls.Add(this.buttonReplicateTrophy128);
		this.groupGraphics.Controls.Add(this.viewer2DTrophy128);
		this.groupGraphics.Controls.Add(this.multiViewer2DTextures);
		this.groupGraphics.Controls.Add(this.group3D);
		this.groupGraphics.Controls.Add(this.viewer2DTrophy256);
		this.groupGraphics.Location = new System.Drawing.Point(3, 3);
		this.groupGraphics.Name = "groupGraphics";
		this.groupGraphics.Size = new System.Drawing.Size(721, 627);
		this.groupGraphics.TabIndex = 0;
		this.groupGraphics.TabStop = false;
		this.groupGraphics.Text = "Graphics";
		this.buttonReplicateTropy.Location = new System.Drawing.Point(448, 276);
		this.buttonReplicateTropy.Name = "buttonReplicateTropy";
		this.buttonReplicateTropy.Size = new System.Drawing.Size(75, 23);
		this.buttonReplicateTropy.TabIndex = 172;
		this.buttonReplicateTropy.Text = "Replicate";
		this.buttonReplicateTropy.UseVisualStyleBackColor = true;
		this.buttonReplicateTropy.Click += new System.EventHandler(buttonReplicateTropy_Click);
		this.viewer2DTrophy.AutoTransparency = true;
		this.viewer2DTrophy.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTrophy.ButtonStripVisible = false;
		this.viewer2DTrophy.CurrentBitmap = null;
		this.viewer2DTrophy.ExtendedFormat = false;
		this.viewer2DTrophy.FullSizeButton = false;
		this.viewer2DTrophy.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DTrophy.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DTrophy.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTrophy.Location = new System.Drawing.Point(271, 19);
		this.viewer2DTrophy.Name = "viewer2DTrophy";
		this.viewer2DTrophy.RemoveButton = false;
		this.viewer2DTrophy.ShowButton = false;
		this.viewer2DTrophy.ShowButtonChecked = true;
		this.viewer2DTrophy.Size = new System.Drawing.Size(256, 281);
		this.viewer2DTrophy.TabIndex = 171;
		this.buttonReplicateTrophy128.Location = new System.Drawing.Point(563, 179);
		this.buttonReplicateTrophy128.Name = "buttonReplicateTrophy128";
		this.buttonReplicateTrophy128.Size = new System.Drawing.Size(75, 23);
		this.buttonReplicateTrophy128.TabIndex = 170;
		this.buttonReplicateTrophy128.Text = "Replicate";
		this.buttonReplicateTrophy128.UseVisualStyleBackColor = true;
		this.buttonReplicateTrophy128.Click += new System.EventHandler(buttonReplicateTrophy128_Click);
		this.viewer2DTrophy128.AutoTransparency = true;
		this.viewer2DTrophy128.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTrophy128.ButtonStripVisible = false;
		this.viewer2DTrophy128.CurrentBitmap = null;
		this.viewer2DTrophy128.ExtendedFormat = false;
		this.viewer2DTrophy128.FullSizeButton = false;
		this.viewer2DTrophy128.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DTrophy128.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DTrophy128.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTrophy128.Location = new System.Drawing.Point(533, 20);
		this.viewer2DTrophy128.Name = "viewer2DTrophy128";
		this.viewer2DTrophy128.RemoveButton = false;
		this.viewer2DTrophy128.ShowButton = false;
		this.viewer2DTrophy128.ShowButtonChecked = true;
		this.viewer2DTrophy128.Size = new System.Drawing.Size(128, 153);
		this.viewer2DTrophy128.TabIndex = 169;
		this.multiViewer2DTextures.AutoTransparency = false;
		this.multiViewer2DTextures.Bitmaps = null;
		this.multiViewer2DTextures.CheckBitmapSize = true;
		this.multiViewer2DTextures.FixedSize = true;
		this.multiViewer2DTextures.FullSizeButton = false;
		this.multiViewer2DTextures.LabelText = "Texture";
		this.multiViewer2DTextures.Location = new System.Drawing.Point(6, 314);
		this.multiViewer2DTextures.Name = "multiViewer2DTextures";
		this.multiViewer2DTextures.ShowButton = false;
		this.multiViewer2DTextures.ShowDeleteButton = false;
		this.multiViewer2DTextures.Size = new System.Drawing.Size(256, 306);
		this.multiViewer2DTextures.TabIndex = 168;
		this.group3D.Controls.Add(this.toolNear3D);
		this.group3D.Location = new System.Drawing.Point(268, 306);
		this.group3D.Name = "group3D";
		this.group3D.Size = new System.Drawing.Size(445, 314);
		this.group3D.TabIndex = 167;
		this.group3D.TabStop = false;
		this.group3D.Text = "3D Model";
		this.toolNear3D.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolNear3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolNear3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.buttonShow3DModel, this.toolStripSeparator1, this.buttonImport3DModel, this.buttonExport3DModel, this.toolStripSeparator2, this.buttonRemove3DModel });
		this.toolNear3D.Location = new System.Drawing.Point(3, 286);
		this.toolNear3D.Name = "toolNear3D";
		this.toolNear3D.Size = new System.Drawing.Size(439, 25);
		this.toolNear3D.TabIndex = 2;
		this.buttonShow3DModel.CheckOnClick = true;
		this.buttonShow3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonShow3DModel.Image");
		this.buttonShow3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow3DModel.Name = "buttonShow3DModel";
		this.buttonShow3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonShow3DModel.Text = "Show / Hide";
		this.buttonShow3DModel.Click += new System.EventHandler(buttonShow3DModel_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonImport3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImport3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonImport3DModel.Image");
		this.buttonImport3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImport3DModel.Name = "buttonImport3DModel";
		this.buttonImport3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonImport3DModel.Text = "Import 3D Model";
		this.buttonImport3DModel.Click += new System.EventHandler(buttonImport3DModel_Click);
		this.buttonExport3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonExport3DModel.Image");
		this.buttonExport3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport3DModel.Name = "buttonExport3DModel";
		this.buttonExport3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonExport3DModel.Text = "Export 3D Model";
		this.buttonExport3DModel.Click += new System.EventHandler(buttonExport3DModel_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonRemove3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonRemove3DModel.Image");
		this.buttonRemove3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove3DModel.Name = "buttonRemove3DModel";
		this.buttonRemove3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove3DModel.Text = "Remove 3D Model";
		this.buttonRemove3DModel.Click += new System.EventHandler(buttonRemove3DModel_Click);
		this.viewer2DTrophy256.AutoTransparency = true;
		this.viewer2DTrophy256.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTrophy256.ButtonStripVisible = false;
		this.viewer2DTrophy256.CurrentBitmap = null;
		this.viewer2DTrophy256.ExtendedFormat = false;
		this.viewer2DTrophy256.FullSizeButton = false;
		this.viewer2DTrophy256.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DTrophy256.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DTrophy256.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTrophy256.Location = new System.Drawing.Point(6, 20);
		this.viewer2DTrophy256.Name = "viewer2DTrophy256";
		this.viewer2DTrophy256.RemoveButton = false;
		this.viewer2DTrophy256.ShowButton = false;
		this.viewer2DTrophy256.ShowButtonChecked = true;
		this.viewer2DTrophy256.Size = new System.Drawing.Size(256, 281);
		this.viewer2DTrophy256.TabIndex = 163;
		this.tabPageTrophyPitchGraphics.Controls.Add(this.viewer2DPitchDressing);
		this.tabPageTrophyPitchGraphics.Location = new System.Drawing.Point(4, 22);
		this.tabPageTrophyPitchGraphics.Name = "tabPageTrophyPitchGraphics";
		this.tabPageTrophyPitchGraphics.Size = new System.Drawing.Size(735, 698);
		this.tabPageTrophyPitchGraphics.TabIndex = 4;
		this.tabPageTrophyPitchGraphics.Text = "Pitch Graphics";
		this.tabPageTrophyPitchGraphics.UseVisualStyleBackColor = true;
		this.viewer2DPitchDressing.AutoTransparency = false;
		this.viewer2DPitchDressing.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPitchDressing.ButtonStripVisible = false;
		this.viewer2DPitchDressing.CurrentBitmap = null;
		this.viewer2DPitchDressing.ExtendedFormat = false;
		this.viewer2DPitchDressing.FullSizeButton = false;
		this.viewer2DPitchDressing.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DPitchDressing.ImageSize = new System.Drawing.Size(1024, 512);
		this.viewer2DPitchDressing.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DPitchDressing.Location = new System.Drawing.Point(8, 3);
		this.viewer2DPitchDressing.Name = "viewer2DPitchDressing";
		this.viewer2DPitchDressing.RemoveButton = false;
		this.viewer2DPitchDressing.ShowButton = false;
		this.viewer2DPitchDressing.ShowButtonChecked = true;
		this.viewer2DPitchDressing.Size = new System.Drawing.Size(512, 281);
		this.viewer2DPitchDressing.TabIndex = 0;
		this.tabPageTrophyRevMod.Controls.Add(this.groupTeamBallRevMod);
		this.tabPageTrophyRevMod.Controls.Add(this.groupTeamAdboardsRevMod);
		this.tabPageTrophyRevMod.Location = new System.Drawing.Point(4, 22);
		this.tabPageTrophyRevMod.Name = "tabPageTrophyRevMod";
		this.tabPageTrophyRevMod.Size = new System.Drawing.Size(735, 698);
		this.tabPageTrophyRevMod.TabIndex = 3;
		this.tabPageTrophyRevMod.Text = "Rev Mod Extensions";
		this.tabPageTrophyRevMod.UseVisualStyleBackColor = true;
		this.groupTeamBallRevMod.Controls.Add(this.toolTeamBall3D);
		this.groupTeamBallRevMod.Controls.Add(this.multiViewer2DTournamentBallTextures);
		this.groupTeamBallRevMod.Location = new System.Drawing.Point(264, 3);
		this.groupTeamBallRevMod.Name = "groupTeamBallRevMod";
		this.groupTeamBallRevMod.Size = new System.Drawing.Size(515, 340);
		this.groupTeamBallRevMod.TabIndex = 167;
		this.groupTeamBallRevMod.TabStop = false;
		this.groupTeamBallRevMod.Text = "Unique Ball";
		this.toolTeamBall3D.AutoSize = false;
		this.toolTeamBall3D.Dock = System.Windows.Forms.DockStyle.None;
		this.toolTeamBall3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolTeamBall3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.buttonShow3DBall, this.toolStripSeparator3, this.buttonImport3DModelTournamentBall, this.buttonExport3DModelTournamentBall, this.toolStripSeparator4, this.buttonRemove3DModelTournamentBall });
		this.toolTeamBall3D.Location = new System.Drawing.Point(259, 301);
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
		this.buttonShow3DBall.Click += new System.EventHandler(buttonShowRevModBall3DModel_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
		this.buttonImport3DModelTournamentBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImport3DModelTournamentBall.Image = (System.Drawing.Image)resources.GetObject("buttonImport3DModelTournamentBall.Image");
		this.buttonImport3DModelTournamentBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImport3DModelTournamentBall.Name = "buttonImport3DModelTournamentBall";
		this.buttonImport3DModelTournamentBall.Size = new System.Drawing.Size(23, 22);
		this.buttonImport3DModelTournamentBall.Text = "Import 3D Model";
		this.buttonImport3DModelTournamentBall.Click += new System.EventHandler(buttonImportRevModBall3DModel_Click);
		this.buttonExport3DModelTournamentBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport3DModelTournamentBall.Image = (System.Drawing.Image)resources.GetObject("buttonExport3DModelTournamentBall.Image");
		this.buttonExport3DModelTournamentBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport3DModelTournamentBall.Name = "buttonExport3DModelTournamentBall";
		this.buttonExport3DModelTournamentBall.Size = new System.Drawing.Size(23, 22);
		this.buttonExport3DModelTournamentBall.Text = "Export 3D Model";
		this.buttonExport3DModelTournamentBall.Click += new System.EventHandler(buttonExportRevModBall3DModel_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.buttonRemove3DModelTournamentBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove3DModelTournamentBall.Image = (System.Drawing.Image)resources.GetObject("buttonRemove3DModelTournamentBall.Image");
		this.buttonRemove3DModelTournamentBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove3DModelTournamentBall.Name = "buttonRemove3DModelTournamentBall";
		this.buttonRemove3DModelTournamentBall.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove3DModelTournamentBall.Text = "Remove 3D Model";
		this.buttonRemove3DModelTournamentBall.Click += new System.EventHandler(buttonRemoveRevModBall3DModel_Click);
		this.multiViewer2DTournamentBallTextures.AutoTransparency = false;
		this.multiViewer2DTournamentBallTextures.Bitmaps = null;
		this.multiViewer2DTournamentBallTextures.CheckBitmapSize = true;
		this.multiViewer2DTournamentBallTextures.FixedSize = true;
		this.multiViewer2DTournamentBallTextures.FullSizeButton = false;
		this.multiViewer2DTournamentBallTextures.LabelText = "Texture";
		this.multiViewer2DTournamentBallTextures.Location = new System.Drawing.Point(1, 24);
		this.multiViewer2DTournamentBallTextures.Name = "multiViewer2DTournamentBallTextures";
		this.multiViewer2DTournamentBallTextures.ShowButton = false;
		this.multiViewer2DTournamentBallTextures.ShowDeleteButton = false;
		this.multiViewer2DTournamentBallTextures.Size = new System.Drawing.Size(256, 302);
		this.multiViewer2DTournamentBallTextures.TabIndex = 1;
		this.groupTeamAdboardsRevMod.Controls.Add(this.viewer2DTournamentAdboard);
		this.groupTeamAdboardsRevMod.Location = new System.Drawing.Point(3, 3);
		this.groupTeamAdboardsRevMod.Name = "groupTeamAdboardsRevMod";
		this.groupTeamAdboardsRevMod.Size = new System.Drawing.Size(259, 570);
		this.groupTeamAdboardsRevMod.TabIndex = 165;
		this.groupTeamAdboardsRevMod.TabStop = false;
		this.groupTeamAdboardsRevMod.Text = "Unique Adboards";
		this.viewer2DTournamentAdboard.AutoTransparency = false;
		this.viewer2DTournamentAdboard.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTournamentAdboard.ButtonStripVisible = false;
		this.viewer2DTournamentAdboard.CurrentBitmap = null;
		this.viewer2DTournamentAdboard.ExtendedFormat = false;
		this.viewer2DTournamentAdboard.FullSizeButton = false;
		this.viewer2DTournamentAdboard.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DTournamentAdboard.ImageSize = new System.Drawing.Size(512, 1024);
		this.viewer2DTournamentAdboard.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTournamentAdboard.Location = new System.Drawing.Point(1, 19);
		this.viewer2DTournamentAdboard.Name = "viewer2DTournamentAdboard";
		this.viewer2DTournamentAdboard.RemoveButton = false;
		this.viewer2DTournamentAdboard.ShowButton = false;
		this.viewer2DTournamentAdboard.ShowButtonChecked = true;
		this.viewer2DTournamentAdboard.Size = new System.Drawing.Size(256, 537);
		this.viewer2DTournamentAdboard.TabIndex = 4;
		this.tabPageWipe3D.Controls.Add(this.multiViewerWipe);
		this.tabPageWipe3D.Location = new System.Drawing.Point(4, 22);
		this.tabPageWipe3D.Name = "tabPageWipe3D";
		this.tabPageWipe3D.Size = new System.Drawing.Size(735, 698);
		this.tabPageWipe3D.TabIndex = 5;
		this.tabPageWipe3D.Text = "Wipe 3D";
		this.tabPageWipe3D.UseVisualStyleBackColor = true;
		this.multiViewerWipe.AutoTransparency = false;
		this.multiViewerWipe.Bitmaps = null;
		this.multiViewerWipe.CheckBitmapSize = true;
		this.multiViewerWipe.FixedSize = true;
		this.multiViewerWipe.FullSizeButton = false;
		this.multiViewerWipe.LabelText = "Texture";
		this.multiViewerWipe.Location = new System.Drawing.Point(21, 15);
		this.multiViewerWipe.Name = "multiViewerWipe";
		this.multiViewerWipe.ShowButton = false;
		this.multiViewerWipe.ShowDeleteButton = false;
		this.multiViewerWipe.Size = new System.Drawing.Size(256, 306);
		this.multiViewerWipe.TabIndex = 169;
		this.pageStage.AutoScroll = true;
		this.pageStage.Controls.Add(this.groupStage);
		this.pageStage.Location = new System.Drawing.Point(4, 22);
		this.pageStage.Name = "pageStage";
		this.pageStage.Size = new System.Drawing.Size(743, 724);
		this.pageStage.TabIndex = 3;
		this.pageStage.Text = "Stage";
		this.pageStage.UseVisualStyleBackColor = true;
		this.pageGroup.AutoScroll = true;
		this.pageGroup.Controls.Add(this.groupGroup);
		this.pageGroup.Location = new System.Drawing.Point(4, 22);
		this.pageGroup.Name = "pageGroup";
		this.pageGroup.Size = new System.Drawing.Size(743, 724);
		this.pageGroup.TabIndex = 4;
		this.pageGroup.Text = "Group";
		this.pageGroup.UseVisualStyleBackColor = true;
		this.groupGroup.Controls.Add(this.groupRules);
		this.groupGroup.Controls.Add(this.groupPlayGroup);
		this.groupGroup.Controls.Add(this.groupGroupScheduke);
		this.groupGroup.Controls.Add(this.groupSlots);
		this.groupGroup.Controls.Add(this.groupInfoColors);
		this.groupGroup.Controls.Add(this.label4);
		this.groupGroup.Controls.Add(this.numericNTeams);
		this.groupGroup.Location = new System.Drawing.Point(0, 0);
		this.groupGroup.Name = "groupGroup";
		this.groupGroup.Size = new System.Drawing.Size(790, 724);
		this.groupGroup.TabIndex = 17;
		this.groupGroup.TabStop = false;
		this.groupGroup.Text = "Group";
		this.groupGroup.Visible = false;
		this.groupRules.Controls.Add(this.panelQualificationRules);
		this.groupRules.Controls.Add(this.panelAdvancement);
		this.groupRules.Location = new System.Drawing.Point(6, 47);
		this.groupRules.Name = "groupRules";
		this.groupRules.Size = new System.Drawing.Size(509, 472);
		this.groupRules.TabIndex = 39;
		this.groupRules.TabStop = false;
		this.groupRules.Text = "Rules";
		this.panelQualificationRules.AutoScroll = true;
		this.panelQualificationRules.Controls.Add(this.toolRules);
		this.panelQualificationRules.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelQualificationRules.Location = new System.Drawing.Point(3, 16);
		this.panelQualificationRules.Name = "panelQualificationRules";
		this.panelQualificationRules.Size = new System.Drawing.Size(503, 453);
		this.panelQualificationRules.TabIndex = 15;
		this.toolRules.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.buttonAddRule, this.buttonRemoveRule });
		this.toolRules.Location = new System.Drawing.Point(0, 0);
		this.toolRules.Name = "toolRules";
		this.toolRules.Size = new System.Drawing.Size(503, 55);
		this.toolRules.TabIndex = 17;
		this.buttonAddRule.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAddRule.Image = (System.Drawing.Image)resources.GetObject("buttonAddRule.Image");
		this.buttonAddRule.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonAddRule.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddRule.Name = "buttonAddRule";
		this.buttonAddRule.Size = new System.Drawing.Size(52, 52);
		this.buttonAddRule.Text = "Add Qualification Rule";
		this.buttonAddRule.Click += new System.EventHandler(buttonAddRule_Click);
		this.buttonRemoveRule.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemoveRule.Image = (System.Drawing.Image)resources.GetObject("buttonRemoveRule.Image");
		this.buttonRemoveRule.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonRemoveRule.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemoveRule.Name = "buttonRemoveRule";
		this.buttonRemoveRule.Size = new System.Drawing.Size(52, 52);
		this.buttonRemoveRule.Text = "Remove Qualification Rule";
		this.buttonRemoveRule.Click += new System.EventHandler(buttonRemoveRule_Click);
		this.panelAdvancement.AutoScroll = true;
		this.panelAdvancement.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelAdvancement.Location = new System.Drawing.Point(3, 16);
		this.panelAdvancement.Name = "panelAdvancement";
		this.panelAdvancement.Size = new System.Drawing.Size(503, 453);
		this.panelAdvancement.TabIndex = 16;
		this.groupPlayGroup.Controls.Add(this.numericNumGames);
		this.groupPlayGroup.Controls.Add(this.label14);
		this.groupPlayGroup.Location = new System.Drawing.Point(169, 11);
		this.groupPlayGroup.Name = "groupPlayGroup";
		this.groupPlayGroup.Size = new System.Drawing.Size(172, 34);
		this.groupPlayGroup.TabIndex = 37;
		this.groupPlayGroup.TabStop = false;
		this.numericNumGames.Location = new System.Drawing.Point(78, 10);
		this.numericNumGames.Maximum = new decimal(new int[4] { 4, 0, 0, 0 });
		this.numericNumGames.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericNumGames.Name = "numericNumGames";
		this.numericNumGames.Size = new System.Drawing.Size(83, 20);
		this.numericNumGames.TabIndex = 36;
		this.numericNumGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNumGames.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericNumGames.ValueChanged += new System.EventHandler(numericNumGames_ValueChanged_1);
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(6, 14);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(66, 13);
		this.label14.TabIndex = 35;
		this.label14.Text = "N. of Games";
		this.groupGroupScheduke.Controls.Add(this.treeGroupSchedule);
		this.groupGroupScheduke.Controls.Add(this.panelGroupScheduleDetails);
		this.groupGroupScheduke.Controls.Add(this.toolGroupSchedule);
		this.groupGroupScheduke.Location = new System.Drawing.Point(520, 11);
		this.groupGroupScheduke.Name = "groupGroupScheduke";
		this.groupGroupScheduke.Size = new System.Drawing.Size(267, 707);
		this.groupGroupScheduke.TabIndex = 34;
		this.groupGroupScheduke.TabStop = false;
		this.groupGroupScheduke.Text = "Schedules";
		this.treeGroupSchedule.FullRowSelect = true;
		this.treeGroupSchedule.HideSelection = false;
		this.treeGroupSchedule.Location = new System.Drawing.Point(3, 220);
		this.treeGroupSchedule.Name = "treeGroupSchedule";
		this.treeGroupSchedule.Size = new System.Drawing.Size(264, 487);
		this.treeGroupSchedule.TabIndex = 7;
		this.treeGroupSchedule.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeGroupSchedule_AfterSelect);
		this.panelGroupScheduleDetails.Controls.Add(this.groupGroupScheduleDetails);
		this.panelGroupScheduleDetails.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGroupScheduleDetails.Location = new System.Drawing.Point(3, 126);
		this.panelGroupScheduleDetails.Name = "panelGroupScheduleDetails";
		this.panelGroupScheduleDetails.Size = new System.Drawing.Size(261, 94);
		this.panelGroupScheduleDetails.TabIndex = 8;
		this.groupGroupScheduleDetails.Controls.Add(this.dateGroupPicker);
		this.groupGroupScheduleDetails.Controls.Add(this.label38);
		this.groupGroupScheduleDetails.Controls.Add(this.numericGroupMinGames);
		this.groupGroupScheduleDetails.Controls.Add(this.label39);
		this.groupGroupScheduleDetails.Controls.Add(this.numericGroupMaxGames);
		this.groupGroupScheduleDetails.Controls.Add(this.label40);
		this.groupGroupScheduleDetails.Controls.Add(this.comboGroupTime);
		this.groupGroupScheduleDetails.Controls.Add(this.label41);
		this.groupGroupScheduleDetails.Location = new System.Drawing.Point(3, -2);
		this.groupGroupScheduleDetails.Name = "groupGroupScheduleDetails";
		this.groupGroupScheduleDetails.Size = new System.Drawing.Size(261, 92);
		this.groupGroupScheduleDetails.TabIndex = 25;
		this.groupGroupScheduleDetails.TabStop = false;
		this.dateGroupPicker.Location = new System.Drawing.Point(12, 13);
		this.dateGroupPicker.Name = "dateGroupPicker";
		this.dateGroupPicker.Size = new System.Drawing.Size(241, 20);
		this.dateGroupPicker.TabIndex = 17;
		this.dateGroupPicker.ValueChanged += new System.EventHandler(dateGroupPicker_ValueChanged);
		this.label38.AutoSize = true;
		this.label38.Location = new System.Drawing.Point(65, 70);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(26, 13);
		this.label38.TabIndex = 24;
		this.label38.Text = "min:";
		this.numericGroupMinGames.Location = new System.Drawing.Point(95, 65);
		this.numericGroupMinGames.Maximum = new decimal(new int[4] { 80, 0, 0, 0 });
		this.numericGroupMinGames.Name = "numericGroupMinGames";
		this.numericGroupMinGames.Size = new System.Drawing.Size(60, 20);
		this.numericGroupMinGames.TabIndex = 18;
		this.numericGroupMinGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericGroupMinGames.Value = new decimal(new int[4] { 12, 0, 0, 0 });
		this.numericGroupMinGames.ValueChanged += new System.EventHandler(numericGroupMinGames_ValueChanged);
		this.label39.AutoSize = true;
		this.label39.Location = new System.Drawing.Point(162, 70);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(29, 13);
		this.label39.TabIndex = 23;
		this.label39.Text = "max:";
		this.numericGroupMaxGames.Location = new System.Drawing.Point(193, 65);
		this.numericGroupMaxGames.Maximum = new decimal(new int[4] { 80, 0, 0, 0 });
		this.numericGroupMaxGames.Name = "numericGroupMaxGames";
		this.numericGroupMaxGames.Size = new System.Drawing.Size(60, 20);
		this.numericGroupMaxGames.TabIndex = 19;
		this.numericGroupMaxGames.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericGroupMaxGames.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.numericGroupMaxGames.ValueChanged += new System.EventHandler(numericGroupMaxGames_ValueChanged);
		this.label40.AutoSize = true;
		this.label40.Location = new System.Drawing.Point(16, 70);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(40, 13);
		this.label40.TabIndex = 22;
		this.label40.Text = "Games";
		this.comboGroupTime.FormattingEnabled = true;
		this.comboGroupTime.Items.AddRange(new object[41]
		{
			"12.00", "12.15", "12.30", "12.45", "13.00", "13.15", "13.30", "13.45", "14.00", "14.15",
			"14.30", "14.45", "15.00", "15.15", "15.30", "15.45", "16.00", "16.15", "16.30", "16.45",
			"17.00", "17.15", "17.30", "17.45", "18.00", "18.15", "18.30", "18.45", "19.00", "19.15",
			"19.30", "19.45", "20.00", "20.15", "20.30", "20.45", "21.00", "21.15", "21.30", "21.45",
			"22.00"
		});
		this.comboGroupTime.Location = new System.Drawing.Point(60, 38);
		this.comboGroupTime.Name = "comboGroupTime";
		this.comboGroupTime.Size = new System.Drawing.Size(121, 21);
		this.comboGroupTime.TabIndex = 20;
		this.comboGroupTime.SelectedIndexChanged += new System.EventHandler(comboGroupTime_SelectedIndexChanged);
		this.label41.AutoSize = true;
		this.label41.Location = new System.Drawing.Point(16, 41);
		this.label41.Name = "label41";
		this.label41.Size = new System.Drawing.Size(30, 13);
		this.label41.TabIndex = 21;
		this.label41.Text = "Time";
		this.toolGroupSchedule.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.buttonCopyGroupCalendar, this.buttonPasteGroupCalendar, this.buttonCleanGroupCalendar, this.buttonNewGroupLeg, this.buttonRemoveGroupLeg, this.buttonGroupAddTime, this.buttonGroupRemoveTime, this.buttongroupSortLegs });
		this.toolGroupSchedule.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.toolGroupSchedule.Location = new System.Drawing.Point(3, 16);
		this.toolGroupSchedule.Name = "toolGroupSchedule";
		this.toolGroupSchedule.Size = new System.Drawing.Size(261, 110);
		this.toolGroupSchedule.TabIndex = 0;
		this.buttonCopyGroupCalendar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCopyGroupCalendar.Image = (System.Drawing.Image)resources.GetObject("buttonCopyGroupCalendar.Image");
		this.buttonCopyGroupCalendar.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCopyGroupCalendar.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCopyGroupCalendar.Name = "buttonCopyGroupCalendar";
		this.buttonCopyGroupCalendar.Size = new System.Drawing.Size(52, 52);
		this.buttonCopyGroupCalendar.Text = "Copy Calendar";
		this.buttonCopyGroupCalendar.Click += new System.EventHandler(buttonCopyGroupCalendar_Click);
		this.buttonPasteGroupCalendar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPasteGroupCalendar.Enabled = false;
		this.buttonPasteGroupCalendar.Image = (System.Drawing.Image)resources.GetObject("buttonPasteGroupCalendar.Image");
		this.buttonPasteGroupCalendar.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonPasteGroupCalendar.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPasteGroupCalendar.Name = "buttonPasteGroupCalendar";
		this.buttonPasteGroupCalendar.Size = new System.Drawing.Size(52, 52);
		this.buttonPasteGroupCalendar.Text = "Paste Calendar";
		this.buttonPasteGroupCalendar.Click += new System.EventHandler(buttonPasteGroupCalendar_Click);
		this.buttonCleanGroupCalendar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCleanGroupCalendar.Image = (System.Drawing.Image)resources.GetObject("buttonCleanGroupCalendar.Image");
		this.buttonCleanGroupCalendar.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCleanGroupCalendar.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCleanGroupCalendar.Name = "buttonCleanGroupCalendar";
		this.buttonCleanGroupCalendar.Size = new System.Drawing.Size(52, 52);
		this.buttonCleanGroupCalendar.Text = "Clean Calendar";
		this.buttonCleanGroupCalendar.Click += new System.EventHandler(buttonCleanGroupCalendar_Click);
		this.buttonNewGroupLeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonNewGroupLeg.Image = (System.Drawing.Image)resources.GetObject("buttonNewGroupLeg.Image");
		this.buttonNewGroupLeg.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonNewGroupLeg.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonNewGroupLeg.Name = "buttonNewGroupLeg";
		this.buttonNewGroupLeg.Size = new System.Drawing.Size(52, 52);
		this.buttonNewGroupLeg.Text = "New Leg";
		this.buttonNewGroupLeg.Click += new System.EventHandler(buttonNewGroupLeg_Click);
		this.buttonRemoveGroupLeg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemoveGroupLeg.Image = (System.Drawing.Image)resources.GetObject("buttonRemoveGroupLeg.Image");
		this.buttonRemoveGroupLeg.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonRemoveGroupLeg.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemoveGroupLeg.Name = "buttonRemoveGroupLeg";
		this.buttonRemoveGroupLeg.Size = new System.Drawing.Size(52, 52);
		this.buttonRemoveGroupLeg.Text = "Remove Leg";
		this.buttonRemoveGroupLeg.Click += new System.EventHandler(buttonRemoveGroupLeg_Click);
		this.buttonGroupAddTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonGroupAddTime.Image = (System.Drawing.Image)resources.GetObject("buttonGroupAddTime.Image");
		this.buttonGroupAddTime.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonGroupAddTime.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonGroupAddTime.Name = "buttonGroupAddTime";
		this.buttonGroupAddTime.Size = new System.Drawing.Size(52, 52);
		this.buttonGroupAddTime.Text = "Add Time";
		this.buttonGroupAddTime.Click += new System.EventHandler(buttonGroupAddTime_Click);
		this.buttonGroupRemoveTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonGroupRemoveTime.Image = (System.Drawing.Image)resources.GetObject("buttonGroupRemoveTime.Image");
		this.buttonGroupRemoveTime.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonGroupRemoveTime.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonGroupRemoveTime.Name = "buttonGroupRemoveTime";
		this.buttonGroupRemoveTime.Size = new System.Drawing.Size(52, 52);
		this.buttonGroupRemoveTime.Text = "Remove Time";
		this.buttonGroupRemoveTime.Click += new System.EventHandler(buttonGroupRemoveTime_Click);
		this.buttongroupSortLegs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttongroupSortLegs.Image = (System.Drawing.Image)resources.GetObject("buttongroupSortLegs.Image");
		this.buttongroupSortLegs.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttongroupSortLegs.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttongroupSortLegs.Name = "buttongroupSortLegs";
		this.buttongroupSortLegs.Size = new System.Drawing.Size(52, 52);
		this.buttongroupSortLegs.Text = "Sort Legs By date";
		this.buttongroupSortLegs.Click += new System.EventHandler(buttongroupSortLegs_Click);
		this.groupSlots.Controls.Add(this.numericPossiblePromotionMax);
		this.groupSlots.Controls.Add(this.checkInfoPossiblePromotion);
		this.groupSlots.Controls.Add(this.numericPossiblePromotionMin);
		this.groupSlots.Controls.Add(this.numericPromotionMax);
		this.groupSlots.Controls.Add(this.numericPromotionMin);
		this.groupSlots.Controls.Add(this.numericRelegationMax);
		this.groupSlots.Controls.Add(this.numericRelegationMin);
		this.groupSlots.Controls.Add(this.numericPossibleRelegationMax);
		this.groupSlots.Controls.Add(this.numericPossibleRelegationMin);
		this.groupSlots.Controls.Add(this.label15);
		this.groupSlots.Controls.Add(this.label16);
		this.groupSlots.Controls.Add(this.checkInfoPromotion);
		this.groupSlots.Controls.Add(this.checkInfoRelegation);
		this.groupSlots.Controls.Add(this.checkInfoPossibleRelegation);
		this.groupSlots.Controls.Add(this.checkInfoChamp);
		this.groupSlots.Location = new System.Drawing.Point(263, 525);
		this.groupSlots.Name = "groupSlots";
		this.groupSlots.Size = new System.Drawing.Size(252, 193);
		this.groupSlots.TabIndex = 33;
		this.groupSlots.TabStop = false;
		this.groupSlots.Text = "Slots";
		this.numericPossiblePromotionMax.Location = new System.Drawing.Point(185, 105);
		this.numericPossiblePromotionMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericPossiblePromotionMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossiblePromotionMax.Name = "numericPossiblePromotionMax";
		this.numericPossiblePromotionMax.Size = new System.Drawing.Size(61, 20);
		this.numericPossiblePromotionMax.TabIndex = 54;
		this.numericPossiblePromotionMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPossiblePromotionMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossiblePromotionMax.ValueChanged += new System.EventHandler(numericPossiblePromotionMax_ValueChanged);
		this.checkInfoPossiblePromotion.AutoSize = true;
		this.checkInfoPossiblePromotion.Location = new System.Drawing.Point(5, 106);
		this.checkInfoPossiblePromotion.Name = "checkInfoPossiblePromotion";
		this.checkInfoPossiblePromotion.Size = new System.Drawing.Size(115, 17);
		this.checkInfoPossiblePromotion.TabIndex = 45;
		this.checkInfoPossiblePromotion.Text = "Possible Promotion";
		this.checkInfoPossiblePromotion.UseVisualStyleBackColor = true;
		this.checkInfoPossiblePromotion.CheckedChanged += new System.EventHandler(checkInfoPossiblePromotion_CheckedChanged);
		this.numericPossiblePromotionMin.Location = new System.Drawing.Point(120, 105);
		this.numericPossiblePromotionMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericPossiblePromotionMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossiblePromotionMin.Name = "numericPossiblePromotionMin";
		this.numericPossiblePromotionMin.Size = new System.Drawing.Size(61, 20);
		this.numericPossiblePromotionMin.TabIndex = 53;
		this.numericPossiblePromotionMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPossiblePromotionMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossiblePromotionMin.ValueChanged += new System.EventHandler(numericPossiblePromotionMin_ValueChanged);
		this.numericPromotionMax.Location = new System.Drawing.Point(185, 83);
		this.numericPromotionMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericPromotionMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPromotionMax.Name = "numericPromotionMax";
		this.numericPromotionMax.Size = new System.Drawing.Size(61, 20);
		this.numericPromotionMax.TabIndex = 56;
		this.numericPromotionMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPromotionMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPromotionMax.ValueChanged += new System.EventHandler(numericPromotionMax_ValueChanged);
		this.numericPromotionMin.Location = new System.Drawing.Point(120, 83);
		this.numericPromotionMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericPromotionMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPromotionMin.Name = "numericPromotionMin";
		this.numericPromotionMin.Size = new System.Drawing.Size(61, 20);
		this.numericPromotionMin.TabIndex = 55;
		this.numericPromotionMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPromotionMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPromotionMin.ValueChanged += new System.EventHandler(numericPromotionMin_ValueChanged);
		this.numericRelegationMax.Location = new System.Drawing.Point(185, 61);
		this.numericRelegationMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericRelegationMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericRelegationMax.Name = "numericRelegationMax";
		this.numericRelegationMax.Size = new System.Drawing.Size(61, 20);
		this.numericRelegationMax.TabIndex = 52;
		this.numericRelegationMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericRelegationMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericRelegationMax.ValueChanged += new System.EventHandler(numericRelegationMax_ValueChanged);
		this.numericRelegationMin.Location = new System.Drawing.Point(120, 61);
		this.numericRelegationMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericRelegationMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericRelegationMin.Name = "numericRelegationMin";
		this.numericRelegationMin.Size = new System.Drawing.Size(61, 20);
		this.numericRelegationMin.TabIndex = 51;
		this.numericRelegationMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericRelegationMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericRelegationMin.ValueChanged += new System.EventHandler(numericRelegationMin_ValueChanged);
		this.numericPossibleRelegationMax.Location = new System.Drawing.Point(185, 39);
		this.numericPossibleRelegationMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericPossibleRelegationMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossibleRelegationMax.Name = "numericPossibleRelegationMax";
		this.numericPossibleRelegationMax.Size = new System.Drawing.Size(61, 20);
		this.numericPossibleRelegationMax.TabIndex = 50;
		this.numericPossibleRelegationMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPossibleRelegationMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossibleRelegationMax.ValueChanged += new System.EventHandler(numericPossibleRelegationMax_ValueChanged);
		this.numericPossibleRelegationMin.Location = new System.Drawing.Point(120, 39);
		this.numericPossibleRelegationMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericPossibleRelegationMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossibleRelegationMin.Name = "numericPossibleRelegationMin";
		this.numericPossibleRelegationMin.Size = new System.Drawing.Size(61, 20);
		this.numericPossibleRelegationMin.TabIndex = 49;
		this.numericPossibleRelegationMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericPossibleRelegationMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericPossibleRelegationMin.ValueChanged += new System.EventHandler(numericPossibleRelegationMin_ValueChanged);
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(196, 23);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(39, 13);
		this.label15.TabIndex = 48;
		this.label15.Text = "to pos.";
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(125, 23);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(50, 13);
		this.label16.TabIndex = 47;
		this.label16.Text = "from pos.";
		this.checkInfoPromotion.AutoSize = true;
		this.checkInfoPromotion.Location = new System.Drawing.Point(5, 84);
		this.checkInfoPromotion.Name = "checkInfoPromotion";
		this.checkInfoPromotion.Size = new System.Drawing.Size(73, 17);
		this.checkInfoPromotion.TabIndex = 46;
		this.checkInfoPromotion.Text = "Promotion";
		this.checkInfoPromotion.UseVisualStyleBackColor = true;
		this.checkInfoPromotion.CheckedChanged += new System.EventHandler(checkInfoPromotion_CheckedChanged);
		this.checkInfoRelegation.AutoSize = true;
		this.checkInfoRelegation.Location = new System.Drawing.Point(5, 62);
		this.checkInfoRelegation.Name = "checkInfoRelegation";
		this.checkInfoRelegation.Size = new System.Drawing.Size(77, 17);
		this.checkInfoRelegation.TabIndex = 44;
		this.checkInfoRelegation.Text = "Relegation";
		this.checkInfoRelegation.UseVisualStyleBackColor = true;
		this.checkInfoRelegation.CheckedChanged += new System.EventHandler(checkInfoRelegation_CheckedChanged);
		this.checkInfoPossibleRelegation.AutoSize = true;
		this.checkInfoPossibleRelegation.Location = new System.Drawing.Point(5, 40);
		this.checkInfoPossibleRelegation.Name = "checkInfoPossibleRelegation";
		this.checkInfoPossibleRelegation.Size = new System.Drawing.Size(119, 17);
		this.checkInfoPossibleRelegation.TabIndex = 43;
		this.checkInfoPossibleRelegation.Text = "Possible Relegation";
		this.checkInfoPossibleRelegation.UseVisualStyleBackColor = true;
		this.checkInfoPossibleRelegation.CheckedChanged += new System.EventHandler(checkInfoPossibleRelegation_CheckedChanged);
		this.checkInfoChamp.AutoSize = true;
		this.checkInfoChamp.Location = new System.Drawing.Point(5, 19);
		this.checkInfoChamp.Name = "checkInfoChamp";
		this.checkInfoChamp.Size = new System.Drawing.Size(60, 17);
		this.checkInfoChamp.TabIndex = 42;
		this.checkInfoChamp.Text = "Winner";
		this.checkInfoChamp.UseVisualStyleBackColor = true;
		this.checkInfoChamp.CheckedChanged += new System.EventHandler(checkInfoChamp_CheckedChanged);
		this.groupInfoColors.Controls.Add(this.numericColorPossiblePromotionMax);
		this.groupInfoColors.Controls.Add(this.checkInfoColorPossiblePromotion);
		this.groupInfoColors.Controls.Add(this.numericColorAdvanceMax);
		this.groupInfoColors.Controls.Add(this.numericColorPossiblePromotionMin);
		this.groupInfoColors.Controls.Add(this.numericColorAdvanceMin);
		this.groupInfoColors.Controls.Add(this.numericColorPromotionMax);
		this.groupInfoColors.Controls.Add(this.numericColorPromotionMin);
		this.groupInfoColors.Controls.Add(this.numericColorRelegationMax);
		this.groupInfoColors.Controls.Add(this.numericColorRelegationMin);
		this.groupInfoColors.Controls.Add(this.numericColorPossibleRelegationMax);
		this.groupInfoColors.Controls.Add(this.numericColorPossibleRelegationMin);
		this.groupInfoColors.Controls.Add(this.numericColorEuropaMax);
		this.groupInfoColors.Controls.Add(this.numericColorEuropaMin);
		this.groupInfoColors.Controls.Add(this.numericColorChampionsMax);
		this.groupInfoColors.Controls.Add(this.numericColorChampionsMin);
		this.groupInfoColors.Controls.Add(this.label12);
		this.groupInfoColors.Controls.Add(this.label11);
		this.groupInfoColors.Controls.Add(this.checkInfoColorAdvance);
		this.groupInfoColors.Controls.Add(this.checkInfoColorPromotion);
		this.groupInfoColors.Controls.Add(this.checkInfoColorRelegation);
		this.groupInfoColors.Controls.Add(this.checkInfoColorPossibleRelegation);
		this.groupInfoColors.Controls.Add(this.checkInfoColorEuropa);
		this.groupInfoColors.Controls.Add(this.checkInfoColorChampions);
		this.groupInfoColors.Controls.Add(this.checkInfoColorChamp);
		this.groupInfoColors.Location = new System.Drawing.Point(5, 525);
		this.groupInfoColors.Name = "groupInfoColors";
		this.groupInfoColors.Size = new System.Drawing.Size(254, 193);
		this.groupInfoColors.TabIndex = 32;
		this.groupInfoColors.TabStop = false;
		this.groupInfoColors.Text = "Colors";
		this.numericColorPossiblePromotionMax.Location = new System.Drawing.Point(188, 144);
		this.numericColorPossiblePromotionMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorPossiblePromotionMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossiblePromotionMax.Name = "numericColorPossiblePromotionMax";
		this.numericColorPossiblePromotionMax.Size = new System.Drawing.Size(61, 20);
		this.numericColorPossiblePromotionMax.TabIndex = 39;
		this.numericColorPossiblePromotionMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorPossiblePromotionMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossiblePromotionMax.ValueChanged += new System.EventHandler(numericColorPossiblePromotionMax_ValueChanged);
		this.checkInfoColorPossiblePromotion.AutoSize = true;
		this.checkInfoColorPossiblePromotion.Location = new System.Drawing.Point(6, 145);
		this.checkInfoColorPossiblePromotion.Name = "checkInfoColorPossiblePromotion";
		this.checkInfoColorPossiblePromotion.Size = new System.Drawing.Size(115, 17);
		this.checkInfoColorPossiblePromotion.TabIndex = 5;
		this.checkInfoColorPossiblePromotion.Text = "Possible Promotion";
		this.checkInfoColorPossiblePromotion.UseVisualStyleBackColor = true;
		this.checkInfoColorPossiblePromotion.CheckedChanged += new System.EventHandler(checkInfoColorPossiblePromotion_CheckedChanged);
		this.numericColorAdvanceMax.Location = new System.Drawing.Point(188, 166);
		this.numericColorAdvanceMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorAdvanceMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorAdvanceMax.Name = "numericColorAdvanceMax";
		this.numericColorAdvanceMax.Size = new System.Drawing.Size(61, 20);
		this.numericColorAdvanceMax.TabIndex = 43;
		this.numericColorAdvanceMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorAdvanceMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorAdvanceMax.ValueChanged += new System.EventHandler(numericColorAdvanceMax_ValueChanged);
		this.numericColorPossiblePromotionMin.Location = new System.Drawing.Point(122, 144);
		this.numericColorPossiblePromotionMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorPossiblePromotionMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossiblePromotionMin.Name = "numericColorPossiblePromotionMin";
		this.numericColorPossiblePromotionMin.Size = new System.Drawing.Size(61, 20);
		this.numericColorPossiblePromotionMin.TabIndex = 38;
		this.numericColorPossiblePromotionMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorPossiblePromotionMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossiblePromotionMin.ValueChanged += new System.EventHandler(numericColorPossiblePromotionMin_ValueChanged);
		this.numericColorAdvanceMin.Location = new System.Drawing.Point(122, 166);
		this.numericColorAdvanceMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorAdvanceMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorAdvanceMin.Name = "numericColorAdvanceMin";
		this.numericColorAdvanceMin.Size = new System.Drawing.Size(61, 20);
		this.numericColorAdvanceMin.TabIndex = 42;
		this.numericColorAdvanceMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorAdvanceMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorAdvanceMin.ValueChanged += new System.EventHandler(numericColorAdvanceMin_ValueChanged);
		this.numericColorPromotionMax.Location = new System.Drawing.Point(188, 122);
		this.numericColorPromotionMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorPromotionMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPromotionMax.Name = "numericColorPromotionMax";
		this.numericColorPromotionMax.Size = new System.Drawing.Size(61, 20);
		this.numericColorPromotionMax.TabIndex = 41;
		this.numericColorPromotionMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorPromotionMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPromotionMax.ValueChanged += new System.EventHandler(numericColorPromotionMax_ValueChanged);
		this.numericColorPromotionMin.Location = new System.Drawing.Point(122, 122);
		this.numericColorPromotionMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorPromotionMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPromotionMin.Name = "numericColorPromotionMin";
		this.numericColorPromotionMin.Size = new System.Drawing.Size(61, 20);
		this.numericColorPromotionMin.TabIndex = 40;
		this.numericColorPromotionMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorPromotionMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPromotionMin.ValueChanged += new System.EventHandler(numericColorPromotionMin_ValueChanged);
		this.numericColorRelegationMax.Location = new System.Drawing.Point(188, 100);
		this.numericColorRelegationMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorRelegationMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorRelegationMax.Name = "numericColorRelegationMax";
		this.numericColorRelegationMax.Size = new System.Drawing.Size(61, 20);
		this.numericColorRelegationMax.TabIndex = 37;
		this.numericColorRelegationMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorRelegationMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorRelegationMax.ValueChanged += new System.EventHandler(numericColorRelegationMax_ValueChanged);
		this.numericColorRelegationMin.Location = new System.Drawing.Point(122, 100);
		this.numericColorRelegationMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorRelegationMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorRelegationMin.Name = "numericColorRelegationMin";
		this.numericColorRelegationMin.Size = new System.Drawing.Size(61, 20);
		this.numericColorRelegationMin.TabIndex = 36;
		this.numericColorRelegationMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorRelegationMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorRelegationMin.ValueChanged += new System.EventHandler(numericColorRelegationMin_ValueChanged);
		this.numericColorPossibleRelegationMax.Location = new System.Drawing.Point(188, 78);
		this.numericColorPossibleRelegationMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorPossibleRelegationMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossibleRelegationMax.Name = "numericColorPossibleRelegationMax";
		this.numericColorPossibleRelegationMax.Size = new System.Drawing.Size(61, 20);
		this.numericColorPossibleRelegationMax.TabIndex = 35;
		this.numericColorPossibleRelegationMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorPossibleRelegationMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossibleRelegationMax.ValueChanged += new System.EventHandler(numericColorPossibleRelegationMax_ValueChanged);
		this.numericColorPossibleRelegationMin.Location = new System.Drawing.Point(122, 78);
		this.numericColorPossibleRelegationMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorPossibleRelegationMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossibleRelegationMin.Name = "numericColorPossibleRelegationMin";
		this.numericColorPossibleRelegationMin.Size = new System.Drawing.Size(61, 20);
		this.numericColorPossibleRelegationMin.TabIndex = 34;
		this.numericColorPossibleRelegationMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorPossibleRelegationMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorPossibleRelegationMin.ValueChanged += new System.EventHandler(numericColorPossibleRelegationMin_ValueChanged);
		this.numericColorEuropaMax.Location = new System.Drawing.Point(188, 56);
		this.numericColorEuropaMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorEuropaMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorEuropaMax.Name = "numericColorEuropaMax";
		this.numericColorEuropaMax.Size = new System.Drawing.Size(61, 20);
		this.numericColorEuropaMax.TabIndex = 33;
		this.numericColorEuropaMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorEuropaMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorEuropaMax.ValueChanged += new System.EventHandler(numericColorEuropaMax_ValueChanged);
		this.numericColorEuropaMin.Location = new System.Drawing.Point(122, 56);
		this.numericColorEuropaMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorEuropaMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorEuropaMin.Name = "numericColorEuropaMin";
		this.numericColorEuropaMin.Size = new System.Drawing.Size(61, 20);
		this.numericColorEuropaMin.TabIndex = 32;
		this.numericColorEuropaMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorEuropaMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorEuropaMin.ValueChanged += new System.EventHandler(numericColorEuropaMin_ValueChanged);
		this.numericColorChampionsMax.Location = new System.Drawing.Point(188, 34);
		this.numericColorChampionsMax.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorChampionsMax.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorChampionsMax.Name = "numericColorChampionsMax";
		this.numericColorChampionsMax.Size = new System.Drawing.Size(61, 20);
		this.numericColorChampionsMax.TabIndex = 31;
		this.numericColorChampionsMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorChampionsMax.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorChampionsMax.ValueChanged += new System.EventHandler(numericColorChampionsMax_ValueChanged);
		this.numericColorChampionsMin.Location = new System.Drawing.Point(122, 34);
		this.numericColorChampionsMin.Maximum = new decimal(new int[4] { 36, 0, 0, 0 });
		this.numericColorChampionsMin.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorChampionsMin.Name = "numericColorChampionsMin";
		this.numericColorChampionsMin.Size = new System.Drawing.Size(61, 20);
		this.numericColorChampionsMin.TabIndex = 30;
		this.numericColorChampionsMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericColorChampionsMin.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericColorChampionsMin.ValueChanged += new System.EventHandler(numericColorChampionsMin_ValueChanged);
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(199, 19);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(39, 13);
		this.label12.TabIndex = 9;
		this.label12.Text = "to pos.";
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(127, 19);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(50, 13);
		this.label11.TabIndex = 8;
		this.label11.Text = "from pos.";
		this.checkInfoColorAdvance.AutoSize = true;
		this.checkInfoColorAdvance.Location = new System.Drawing.Point(6, 167);
		this.checkInfoColorAdvance.Name = "checkInfoColorAdvance";
		this.checkInfoColorAdvance.Size = new System.Drawing.Size(69, 17);
		this.checkInfoColorAdvance.TabIndex = 7;
		this.checkInfoColorAdvance.Text = "Advance";
		this.checkInfoColorAdvance.UseVisualStyleBackColor = true;
		this.checkInfoColorAdvance.CheckedChanged += new System.EventHandler(checkInfoColorAdvance_CheckedChanged);
		this.checkInfoColorPromotion.AutoSize = true;
		this.checkInfoColorPromotion.Location = new System.Drawing.Point(6, 123);
		this.checkInfoColorPromotion.Name = "checkInfoColorPromotion";
		this.checkInfoColorPromotion.Size = new System.Drawing.Size(73, 17);
		this.checkInfoColorPromotion.TabIndex = 6;
		this.checkInfoColorPromotion.Text = "Promotion";
		this.checkInfoColorPromotion.UseVisualStyleBackColor = true;
		this.checkInfoColorPromotion.CheckedChanged += new System.EventHandler(checkInfoColorPromotion_CheckedChanged);
		this.checkInfoColorRelegation.AutoSize = true;
		this.checkInfoColorRelegation.Location = new System.Drawing.Point(6, 101);
		this.checkInfoColorRelegation.Name = "checkInfoColorRelegation";
		this.checkInfoColorRelegation.Size = new System.Drawing.Size(77, 17);
		this.checkInfoColorRelegation.TabIndex = 4;
		this.checkInfoColorRelegation.Text = "Relegation";
		this.checkInfoColorRelegation.UseVisualStyleBackColor = true;
		this.checkInfoColorRelegation.CheckedChanged += new System.EventHandler(checkInfoColorRelegation_CheckedChanged);
		this.checkInfoColorPossibleRelegation.AutoSize = true;
		this.checkInfoColorPossibleRelegation.Location = new System.Drawing.Point(6, 79);
		this.checkInfoColorPossibleRelegation.Name = "checkInfoColorPossibleRelegation";
		this.checkInfoColorPossibleRelegation.Size = new System.Drawing.Size(119, 17);
		this.checkInfoColorPossibleRelegation.TabIndex = 3;
		this.checkInfoColorPossibleRelegation.Text = "Possible Relegation";
		this.checkInfoColorPossibleRelegation.UseVisualStyleBackColor = true;
		this.checkInfoColorPossibleRelegation.CheckedChanged += new System.EventHandler(checkInfoColorPossibleRelegation_CheckedChanged);
		this.checkInfoColorEuropa.AutoSize = true;
		this.checkInfoColorEuropa.Location = new System.Drawing.Point(6, 57);
		this.checkInfoColorEuropa.Name = "checkInfoColorEuropa";
		this.checkInfoColorEuropa.Size = new System.Drawing.Size(99, 17);
		this.checkInfoColorEuropa.TabIndex = 2;
		this.checkInfoColorEuropa.Text = "Europa League";
		this.checkInfoColorEuropa.UseVisualStyleBackColor = true;
		this.checkInfoColorEuropa.CheckedChanged += new System.EventHandler(checkInfoColorEuropa_CheckedChanged);
		this.checkInfoColorChampions.AutoSize = true;
		this.checkInfoColorChampions.Location = new System.Drawing.Point(6, 35);
		this.checkInfoColorChampions.Name = "checkInfoColorChampions";
		this.checkInfoColorChampions.Size = new System.Drawing.Size(117, 17);
		this.checkInfoColorChampions.TabIndex = 1;
		this.checkInfoColorChampions.Text = "Champions League";
		this.checkInfoColorChampions.UseVisualStyleBackColor = true;
		this.checkInfoColorChampions.CheckedChanged += new System.EventHandler(checkInfoColorChampions_CheckedChanged);
		this.checkInfoColorChamp.AutoSize = true;
		this.checkInfoColorChamp.Location = new System.Drawing.Point(6, 15);
		this.checkInfoColorChamp.Name = "checkInfoColorChamp";
		this.checkInfoColorChamp.Size = new System.Drawing.Size(60, 17);
		this.checkInfoColorChamp.TabIndex = 0;
		this.checkInfoColorChamp.Text = "Winner";
		this.checkInfoColorChamp.UseVisualStyleBackColor = true;
		this.checkInfoColorChamp.CheckedChanged += new System.EventHandler(checkInfoColorChamp_CheckedChanged);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(6, 25);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(65, 13);
		this.label4.TabIndex = 13;
		this.label4.Text = "N. of Teams";
		this.numericNTeams.Location = new System.Drawing.Point(89, 21);
		this.numericNTeams.Maximum = new decimal(new int[4] { 128, 0, 0, 0 });
		this.numericNTeams.Name = "numericNTeams";
		this.numericNTeams.Size = new System.Drawing.Size(74, 20);
		this.numericNTeams.TabIndex = 14;
		this.numericNTeams.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNTeams.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericNTeams.ValueChanged += new System.EventHandler(numericNTeams_ValueChanged);
		this.panelCompObj.Controls.Add(this.textLanguageName);
		this.panelCompObj.Controls.Add(this.label66);
		this.panelCompObj.Controls.Add(this.textUniqueId);
		this.panelCompObj.Controls.Add(this.comboLanguageKey);
		this.panelCompObj.Controls.Add(this.label3);
		this.panelCompObj.Controls.Add(this.textLanguageKey);
		this.panelCompObj.Controls.Add(this.label2);
		this.panelCompObj.Controls.Add(this.textFourCharName);
		this.panelCompObj.Controls.Add(this.label1);
		this.panelCompObj.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelCompObj.Location = new System.Drawing.Point(0, 0);
		this.panelCompObj.Name = "panelCompObj";
		this.panelCompObj.Size = new System.Drawing.Size(751, 30);
		this.panelCompObj.TabIndex = 0;
		this.textLanguageName.Location = new System.Drawing.Point(626, 5);
		this.textLanguageName.Name = "textLanguageName";
		this.textLanguageName.Size = new System.Drawing.Size(168, 20);
		this.textLanguageName.TabIndex = 7;
		this.textLanguageName.TextChanged += new System.EventHandler(textLanguageName_TextChanged);
		this.label66.AutoSize = true;
		this.label66.Location = new System.Drawing.Point(7, 8);
		this.label66.Name = "label66";
		this.label66.Size = new System.Drawing.Size(19, 13);
		this.label66.TabIndex = 2;
		this.label66.Text = "Id.";
		this.textUniqueId.Enabled = false;
		this.textUniqueId.Location = new System.Drawing.Point(29, 4);
		this.textUniqueId.Name = "textUniqueId";
		this.textUniqueId.Size = new System.Drawing.Size(51, 20);
		this.textUniqueId.TabIndex = 3;
		this.comboLanguageKey.FormattingEnabled = true;
		this.comboLanguageKey.Location = new System.Drawing.Point(343, 4);
		this.comboLanguageKey.Name = "comboLanguageKey";
		this.comboLanguageKey.Size = new System.Drawing.Size(185, 21);
		this.comboLanguageKey.TabIndex = 6;
		this.comboLanguageKey.SelectedIndexChanged += new System.EventHandler(comboLanguageKey_SelectedIndexChanged);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(534, 7);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(86, 13);
		this.label3.TabIndex = 4;
		this.label3.Text = "Language Name";
		this.textLanguageKey.Location = new System.Drawing.Point(343, 4);
		this.textLanguageKey.Name = "textLanguageKey";
		this.textLanguageKey.Size = new System.Drawing.Size(185, 20);
		this.textLanguageKey.TabIndex = 3;
		this.textLanguageKey.TextChanged += new System.EventHandler(textLanguageKey_TextChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(261, 7);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(76, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Language Key";
		this.textFourCharName.Location = new System.Drawing.Point(173, 4);
		this.textFourCharName.Name = "textFourCharName";
		this.textFourCharName.Size = new System.Drawing.Size(72, 20);
		this.textFourCharName.TabIndex = 1;
		this.textFourCharName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.textFourCharName.TextChanged += new System.EventHandler(textFourCharName_TextChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(93, 7);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(74, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "4 Chars Name";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1087, 780);
		base.Controls.Add(this.splitContainer1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "CompetitionForm";
		this.Text = "TrophyForm";
		base.Load += new System.EventHandler(CompetitionsForm_Load);
		this.groupConfederation.ResumeLayout(false);
		this.groupConfederation.PerformLayout();
		this.groupNation.ResumeLayout(false);
		this.groupNation.PerformLayout();
		this.groupWeather.ResumeLayout(false);
		this.groupWeather.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown97).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown98).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown99).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown100).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown101).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown102).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown103).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown104).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown105).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown106).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown107).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown108).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown85).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown86).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown87).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown88).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown89).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown90).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown91).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown92).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown93).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown94).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown95).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown96).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown73).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown74).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown75).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown76).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown77).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown78).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown79).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown80).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown81).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown82).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown83).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown84).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown61).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown62).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown63).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown64).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown65).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown66).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown67).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown68).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown69).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown70).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown71).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown72).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown49).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown50).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown51).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown52).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown53).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown54).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown55).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown56).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown57).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown58).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown59).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown60).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown37).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown38).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown39).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown40).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown41).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown42).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown43).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown44).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown45).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown46).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown47).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown48).EndInit();
		this.toolWeather.ResumeLayout(false);
		this.toolWeather.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown34).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown35).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown36).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown31).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown32).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown33).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown28).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown29).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown30).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown25).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown26).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown27).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown22).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown23).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown24).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown19).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown20).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown21).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown16).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown17).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown18).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown13).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown14).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown15).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown10).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown11).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown12).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown7).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown8).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown9).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericNationYellowsStored).EndInit();
		this.groupTrophy.ResumeLayout(false);
		this.groupTrophy.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericAdvanceFrom).EndInit();
		this.groupInternationalschedule.ResumeLayout(false);
		this.groupInternationalschedule.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericInternationalPeriodicity).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericInternationalFirstYear).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBall).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBall).EndInit();
		this.groupBenchPlayers.ResumeLayout(false);
		this.groupBenchPlayers.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericImportance).EndInit();
		this.groupPromotionRelegation.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericAssetId).EndInit();
		this.groupSchedule.ResumeLayout(false);
		this.groupSchedule.PerformLayout();
		this.groupStage.ResumeLayout(false);
		this.groupStage.PerformLayout();
		this.groupPlayStage.ResumeLayout(false);
		this.groupPlayStage.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericKeepPointsStageRef).EndInit();
		this.groupLeaguetasks.ResumeLayout(false);
		this.groupStageSchedules.ResumeLayout(false);
		this.groupStageSchedules.PerformLayout();
		this.panelStageScheduleDetails.ResumeLayout(false);
		this.groupStageScheduleDetails.ResumeLayout(false);
		this.groupStageScheduleDetails.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericStageMinGames).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericStageMaxGames).EndInit();
		this.toolStageSchedule.ResumeLayout(false);
		this.toolStageSchedule.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericRegularSeason).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericKeepPointsPercentage).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericStageRef).EndInit();
		this.groupStadiums.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericMoneyDrop).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPrizeMoney).EndInit();
		this.groupSetupStage.ResumeLayout(false);
		this.groupSetupStage.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericStandingsRank).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericStandingKeep).EndInit();
		this.toolCompetitionTree.ResumeLayout(false);
		this.toolCompetitionTree.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.tabCompetitions.ResumeLayout(false);
		this.pageWorld.ResumeLayout(false);
		this.pageWorld.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericStartYear).EndInit();
		this.pageConfederation.ResumeLayout(false);
		this.pageNation.ResumeLayout(false);
		this.pageTrophy.ResumeLayout(false);
		this.tabTrophy.ResumeLayout(false);
		this.tabPageTrophyStructure.ResumeLayout(false);
		this.tabPageRankingTable.ResumeLayout(false);
		this.groupInitTeams.ResumeLayout(false);
		this.groupInitTeams.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpdateTableEntries).EndInit();
		this.panelAllInitTeams.ResumeLayout(false);
		this.panelInitTeam1.ResumeLayout(false);
		this.panelInitTeam2.ResumeLayout(false);
		this.panelInitTeam24.ResumeLayout(false);
		this.panelInitTeam3.ResumeLayout(false);
		this.panelInitTeam23.ResumeLayout(false);
		this.panelInitTeam4.ResumeLayout(false);
		this.panelInitTeam22.ResumeLayout(false);
		this.panelInitTeam5.ResumeLayout(false);
		this.panelInitTeam21.ResumeLayout(false);
		this.panelInitTeam6.ResumeLayout(false);
		this.panelInitTeam20.ResumeLayout(false);
		this.panelInitTeam7.ResumeLayout(false);
		this.panelInitTeam19.ResumeLayout(false);
		this.panelInitTeam8.ResumeLayout(false);
		this.panelInitTeam18.ResumeLayout(false);
		this.panelInitTeam9.ResumeLayout(false);
		this.panelInitTeam17.ResumeLayout(false);
		this.panelInitTeam10.ResumeLayout(false);
		this.panelInitTeam16.ResumeLayout(false);
		this.panelInitTeam11.ResumeLayout(false);
		this.panelInitTeam15.ResumeLayout(false);
		this.panelInitTeam12.ResumeLayout(false);
		this.panelInitTeam14.ResumeLayout(false);
		this.panelInitTeam13.ResumeLayout(false);
		this.panelInitTeam25.ResumeLayout(false);
		this.panelInitTeam26.ResumeLayout(false);
		this.panelInitTeam27.ResumeLayout(false);
		this.panelInitTeam28.ResumeLayout(false);
		this.panelInitTeam29.ResumeLayout(false);
		this.panelInitTeam30.ResumeLayout(false);
		this.panelInitTeam31.ResumeLayout(false);
		this.panelInitTeam32.ResumeLayout(false);
		this.panelInitTeam33.ResumeLayout(false);
		this.panelInitTeam34.ResumeLayout(false);
		this.panelInitTeam35.ResumeLayout(false);
		this.panelInitTeam36.ResumeLayout(false);
		this.panelInitTeam37.ResumeLayout(false);
		this.panelInitTeam38.ResumeLayout(false);
		this.panelInitTeam39.ResumeLayout(false);
		this.panelInitTeam40.ResumeLayout(false);
		this.panelInitTeam41.ResumeLayout(false);
		this.panelInitTeam42.ResumeLayout(false);
		this.panelInitTeam43.ResumeLayout(false);
		this.panelInitTeam44.ResumeLayout(false);
		this.panelInitTeam45.ResumeLayout(false);
		this.panelInitTeam46.ResumeLayout(false);
		this.panelInitTeam47.ResumeLayout(false);
		this.panelInitTeam48.ResumeLayout(false);
		this.tabPageTrophyGraphics.ResumeLayout(false);
		this.groupGraphics.ResumeLayout(false);
		this.group3D.ResumeLayout(false);
		this.group3D.PerformLayout();
		this.toolNear3D.ResumeLayout(false);
		this.toolNear3D.PerformLayout();
		this.tabPageTrophyPitchGraphics.ResumeLayout(false);
		this.tabPageTrophyRevMod.ResumeLayout(false);
		this.groupTeamBallRevMod.ResumeLayout(false);
		this.toolTeamBall3D.ResumeLayout(false);
		this.toolTeamBall3D.PerformLayout();
		this.groupTeamAdboardsRevMod.ResumeLayout(false);
		this.tabPageWipe3D.ResumeLayout(false);
		this.pageStage.ResumeLayout(false);
		this.pageGroup.ResumeLayout(false);
		this.groupGroup.ResumeLayout(false);
		this.groupGroup.PerformLayout();
		this.groupRules.ResumeLayout(false);
		this.panelQualificationRules.ResumeLayout(false);
		this.panelQualificationRules.PerformLayout();
		this.toolRules.ResumeLayout(false);
		this.toolRules.PerformLayout();
		this.groupPlayGroup.ResumeLayout(false);
		this.groupPlayGroup.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNumGames).EndInit();
		this.groupGroupScheduke.ResumeLayout(false);
		this.groupGroupScheduke.PerformLayout();
		this.panelGroupScheduleDetails.ResumeLayout(false);
		this.groupGroupScheduleDetails.ResumeLayout(false);
		this.groupGroupScheduleDetails.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericGroupMinGames).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericGroupMaxGames).EndInit();
		this.toolGroupSchedule.ResumeLayout(false);
		this.toolGroupSchedule.PerformLayout();
		this.groupSlots.ResumeLayout(false);
		this.groupSlots.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericPossiblePromotionMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPossiblePromotionMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPromotionMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPromotionMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericRelegationMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericRelegationMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPossibleRelegationMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericPossibleRelegationMin).EndInit();
		this.groupInfoColors.ResumeLayout(false);
		this.groupInfoColors.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossiblePromotionMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorAdvanceMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossiblePromotionMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorAdvanceMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPromotionMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPromotionMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorRelegationMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorRelegationMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossibleRelegationMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorPossibleRelegationMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorEuropaMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorEuropaMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorChampionsMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericColorChampionsMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericNTeams).EndInit();
		this.panelCompObj.ResumeLayout(false);
		this.panelCompObj.PerformLayout();
		base.ResumeLayout(false);
	}
}
