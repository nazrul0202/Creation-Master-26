using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class GameGraphicForm : Form
{
	public delegate bool BitmapUpdateHandler(object sender);

	public BitmapUpdateHandler BitmapUpdateDelegate;

	private static string[] s_FileNames = new string[43];

	private static string[] s_TemplateNames = new string[43];

	private static string[] s_IconNames = new string[217];

	private static Bitmap[] s_MenuBitmaps = new Bitmap[43];

	private IContainer components;

	private Viewer2D viewer2DMessi;

	private Viewer2D viewer2DFifa;

	private GroupBox groupMenu;

	private Button buttonSaveStartGraphics;

	private MultiViewer2D multiViewerMenuPictures;

	private MultiViewer2D multiViewerIcons;

	private GroupBox groupIcons;

	private NumericUpDown numericIcons;

	private Label labelTextIcons;

	private Viewer2D viewer2DIcons;

	private Button buttonReloadGraphics;

	public GameGraphicForm()
	{
		InitializeComponent();
		viewer2DMessi.ButtonStripVisible = true;
		viewer2DMessi.RemoveButton = false;
		viewer2DFifa.ButtonStripVisible = true;
		viewer2DFifa.RemoveButton = false;
		multiViewerMenuPictures.AutoTransparency = false;
		multiViewerMenuPictures.FullSizeButton = true;
		multiViewerMenuPictures.ShowButton = true;
		multiViewerMenuPictures.ShowDeleteButton = true;
		multiViewerMenuPictures.CheckBitmapSize = true;
		multiViewerMenuPictures.FixedSize = false;
		multiViewerMenuPictures.buttonSave.Visible = false;
		multiViewerMenuPictures.LabelText = "Menu";
		multiViewerMenuPictures.buttonImportRx3.Visible = false;
		multiViewerMenuPictures.buttonExportRx3.Visible = false;
		multiViewerMenuPictures.Rx3DeleteDelegate = DeleteBitmapMenu;
		multiViewerMenuPictures.BitmapUpdateDelegate = SaveBitmapMenu;
		multiViewerIcons.AutoTransparency = false;
		multiViewerIcons.FullSizeButton = true;
		multiViewerIcons.ShowButton = true;
		multiViewerIcons.ShowDeleteButton = true;
		multiViewerIcons.CheckBitmapSize = true;
		multiViewerIcons.FixedSize = false;
		multiViewerIcons.buttonSave.Visible = false;
		multiViewerIcons.LabelText = "Icons";
		multiViewerIcons.buttonImportRx3.Visible = false;
		multiViewerIcons.buttonExportRx3.Visible = false;
		viewer2DIcons.ImageImport = ImportIcon;
		viewer2DIcons.ImageDelete = DeleteIcon;
		viewer2DIcons.ButtonStripVisible = true;
		viewer2DIcons.RemoveButton = true;
		SetupFileNames();
	}

	public void Clean()
	{
		base.Visible = false;
	}

	private void GameGraphicForm_Load(object sender, EventArgs e)
	{
		LoadGameGraphics();
	}

	public static void SetupFileNames()
	{
		s_FileNames[0] = "data/ui/imgassets/tiles/mainhub/continuecareer.dds";
		s_FileNames[1] = "data/ui/imgassets/tiles/mainhub/continuecareerlarge.dds";
		s_FileNames[2] = "data/ui/imgassets/tiles/mainhub/coopseasons.dds";
		s_FileNames[3] = "data/ui/imgassets/tiles/mainhub/coopseasonslarge.dds";
		s_FileNames[4] = "data/ui/imgassets/tiles/mainhub/createwomentournament.dds";
		s_FileNames[5] = "data/ui/imgassets/tiles/mainhub/customtournament.dds";
		s_FileNames[6] = "data/ui/imgassets/tiles/mainhub/easfc_logo_bg_204.dds";
		s_FileNames[7] = "data/ui/imgassets/tiles/mainhub/easfc_logo_bg_418.dds";
		s_FileNames[8] = "data/ui/imgassets/tiles/mainhub/eatv_offline.dds";
		s_FileNames[9] = "data/ui/imgassets/tiles/mainhub/editplayers.dds";
		s_FileNames[10] = "data/ui/imgassets/tiles/mainhub/flt_disconnected.dds";
		s_FileNames[11] = "data/ui/imgassets/tiles/mainhub/kickoff.dds";
		s_FileNames[12] = "data/ui/imgassets/tiles/mainhub/kickofflarge0.dds";
		s_FileNames[13] = "data/ui/imgassets/tiles/mainhub/loadcareer.dds";
		s_FileNames[14] = "data/ui/imgassets/tiles/mainhub/loadtournamentlarge.dds";
		s_FileNames[15] = "data/ui/imgassets/tiles/mainhub/loadwomentournament.dds";
		s_FileNames[16] = "data/ui/imgassets/tiles/mainhub/loadwomentournamentlarge.dds";
		s_FileNames[17] = "data/ui/imgassets/tiles/mainhub/newcareer.dds";
		s_FileNames[18] = "data/ui/imgassets/tiles/mainhub/newtournament.dds";
		s_FileNames[19] = "data/ui/imgassets/tiles/mainhub/onlinefriendlieslarge.dds";
		s_FileNames[20] = "data/ui/imgassets/tiles/mainhub/practicearena.dds";
		s_FileNames[21] = "data/ui/imgassets/tiles/mainhub/proclubs.dds";
		s_FileNames[22] = "data/ui/imgassets/tiles/mainhub/proclubslarge.dds";
		s_FileNames[23] = "data/ui/imgassets/tiles/mainhub/seasons.dds";
		s_FileNames[24] = "data/ui/imgassets/tiles/mainhub/seasonslarge.dds";
		s_FileNames[25] = "data/ui/imgassets/tiles/mainhub/skillgames.dds";
		s_FileNames[26] = "data/ui/imgassets/tiles/mainhub/skgameslarge.dds";
		s_FileNames[27] = "data/ui/imgassets/tiles/careerhub/calendar_med.dds";
		s_FileNames[28] = "data/ui/imgassets/tiles/careerhub/contracts_medtall.dds";
		s_FileNames[29] = "data/ui/imgassets/tiles/careerhub/endofseason_med.dds";
		s_FileNames[30] = "data/ui/imgassets/tiles/careerhub/otherleagues_med.dds";
		s_FileNames[31] = "data/ui/imgassets/tiles/careerhub/playerstats_med.dds";
		s_FileNames[32] = "data/ui/imgassets/tiles/careerhub/searchplayers_medtall.dds";
		s_FileNames[33] = "data/ui/imgassets/tiles/careerhub/sellplayers_medtall.dds";
		s_FileNames[34] = "data/ui/imgassets/tiles/careerhub/teamstats_med.dds";
		s_FileNames[35] = "data/ui/imgassets/tiles/careerhub/teamstats_medtall.dds";
		s_FileNames[36] = "data/ui/imgassets/tiles/careerhub/topscorers_medtall.dds";
		s_FileNames[37] = "data/ui/imgassets/tiles/careerhub/trainerintro_lrg.dds";
		s_FileNames[38] = "data/ui/imgassets/tiles/careerhub/youthacademy_medtall.dds";
		s_FileNames[39] = "data/ui/imgassets/tiles/careerhub/youthscouts_medtall.dds";
		s_FileNames[40] = "data/ui/imgassets/tiles/careerhub/shortlist_medtall.dds";
		s_FileNames[41] = "data/ui/imgassets/tiles/careerhub/scoutinginstructions_medtall.dds";
		s_FileNames[42] = "data/ui/imgassets/tiles/careerhub/transfernegotiations_medtall.dds";
		s_TemplateNames[0] = "data/ui/imgassets/tiles/mainhub/menu_576x204.dds";
		s_TemplateNames[1] = "data/ui/imgassets/tiles/mainhub/menu_848x420.dds";
		s_TemplateNames[2] = "data/ui/imgassets/tiles/mainhub/menu_544x204.dds";
		s_TemplateNames[3] = "data/ui/imgassets/tiles/mainhub/menu_772x420.dds";
		s_TemplateNames[4] = "data/ui/imgassets/tiles/mainhub/menu_600x204.dds";
		s_TemplateNames[5] = "data/ui/imgassets/tiles/mainhub/menu_600x204.dds";
		s_TemplateNames[6] = "data/ui/imgassets/tiles/mainhub/menu_848x204.dds";
		s_TemplateNames[7] = "data/ui/imgassets/tiles/mainhub/menu_848x420.dds";
		s_TemplateNames[8] = "data/ui/imgassets/tiles/mainhub/menu_848x420.dds";
		s_TemplateNames[9] = "data/ui/imgassets/tiles/mainhub/menu_420x204.dds";
		s_TemplateNames[10] = "data/ui/imgassets/tiles/mainhub/menu_600x204.dds";
		s_TemplateNames[11] = "data/ui/imgassets/tiles/mainhub/menu_600x204.dds";
		s_TemplateNames[12] = "data/ui/imgassets/tiles/mainhub/menu_848x420.dds";
		s_TemplateNames[13] = "data/ui/imgassets/tiles/mainhub/menu_544x204.dds";
		s_TemplateNames[14] = "data/ui/imgassets/tiles/mainhub/menu_656x420.dds";
		s_TemplateNames[15] = "data/ui/imgassets/tiles/mainhub/menu_600x204.dds";
		s_TemplateNames[16] = "data/ui/imgassets/tiles/mainhub/menu_776x420.dds";
		s_TemplateNames[17] = "data/ui/imgassets/tiles/mainhub/menu_600x204.dds";
		s_TemplateNames[18] = "data/ui/imgassets/tiles/mainhub/menu_600x204.dds";
		s_TemplateNames[19] = "data/ui/imgassets/tiles/mainhub/menu_772x420.dds";
		s_TemplateNames[20] = "data/ui/imgassets/tiles/mainhub/menu_596x204.dds";
		s_TemplateNames[21] = "data/ui/imgassets/tiles/mainhub/menu_544x204.dds";
		s_TemplateNames[22] = "data/ui/imgassets/tiles/mainhub/menu_672x204.dds";
		s_TemplateNames[23] = "data/ui/imgassets/tiles/mainhub/menu_604x204.dds";
		s_TemplateNames[24] = "data/ui/imgassets/tiles/mainhub/menu_680x420.dds";
		s_TemplateNames[25] = "data/ui/imgassets/tiles/mainhub/menu_596x204.dds";
		s_TemplateNames[26] = "data/ui/imgassets/tiles/mainhub/menu_848x420.dds";
		s_TemplateNames[27] = "data/ui/imgassets/tiles/careerhub/menu_848x204.dds";
		s_TemplateNames[28] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[29] = "data/ui/imgassets/tiles/careerhub/menu_848x204.dds";
		s_TemplateNames[30] = "data/ui/imgassets/tiles/careerhub/menu_848x204.dds";
		s_TemplateNames[31] = "data/ui/imgassets/tiles/careerhub/menu_848x204.dds";
		s_TemplateNames[32] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[33] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[34] = "data/ui/imgassets/tiles/careerhub/menu_848x204.dds";
		s_TemplateNames[35] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[36] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[37] = "data/ui/imgassets/tiles/careerhub/menu_848x420.dds";
		s_TemplateNames[38] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[39] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[40] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[41] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_TemplateNames[42] = "data/ui/imgassets/tiles/careerhub/menu_420x420.dds";
		s_IconNames[0] = "data/ui/imgassets/tileicons/ti_accomplishments_small_active.dds";
		s_IconNames[1] = "data/ui/imgassets/tileicons/ti_accomplishments_small_nonactive.dds";
		s_IconNames[2] = "data/ui/imgassets/tileicons/ti_age_sml_active.dds";
		s_IconNames[3] = "data/ui/imgassets/tileicons/ti_age_sml_nonactive.dds";
		s_IconNames[4] = "data/ui/imgassets/tileicons/ti_basiccontrolspc_active.dds";
		s_IconNames[5] = "data/ui/imgassets/tileicons/ti_basiccontrolspc_nonactive.dds";
		s_IconNames[6] = "data/ui/imgassets/tileicons/ti_basiccontrolsps4_active.dds";
		s_IconNames[7] = "data/ui/imgassets/tileicons/ti_basiccontrolsps4_nonactive.dds";
		s_IconNames[8] = "data/ui/imgassets/tileicons/ti_basiccontrolsxbox_active.dds";
		s_IconNames[9] = "data/ui/imgassets/tileicons/ti_basiccontrolsxbox_nonactive.dds";
		s_IconNames[10] = "data/ui/imgassets/tileicons/ti_basiccontrols_square_active.dds";
		s_IconNames[11] = "data/ui/imgassets/tileicons/ti_basiccontrols_square_nonactive.dds";
		s_IconNames[12] = "data/ui/imgassets/tileicons/ti_browsejobs_small_active.dds";
		s_IconNames[13] = "data/ui/imgassets/tileicons/ti_browsejobs_small_nonactive.dds";
		s_IconNames[14] = "data/ui/imgassets/tileicons/ti_calendar_small_active.dds";
		s_IconNames[15] = "data/ui/imgassets/tileicons/ti_calendar_small_nonactive.dds";
		s_IconNames[16] = "data/ui/imgassets/tileicons/ti_catalogue_square_active.dds";
		s_IconNames[17] = "data/ui/imgassets/tileicons/ti_catalogue_square_nonactive.dds";
		s_IconNames[18] = "data/ui/imgassets/tileicons/ti_celebrations_square_active.dds";
		s_IconNames[19] = "data/ui/imgassets/tileicons/ti_celebrations_square_nonactive.dds";
		s_IconNames[20] = "data/ui/imgassets/tileicons/ti_combosettings_square_active.dds";
		s_IconNames[21] = "data/ui/imgassets/tileicons/ti_combosettings_square_nonactive.dds";
		s_IconNames[22] = "data/ui/imgassets/tileicons/ti_contract_sml_active.dds";
		s_IconNames[23] = "data/ui/imgassets/tileicons/ti_contract_sml_nonactive.dds";
		s_IconNames[24] = "data/ui/imgassets/tileicons/ti_controllersettingspc_active.dds";
		s_IconNames[25] = "data/ui/imgassets/tileicons/ti_controllersettingspc_nonactive.dds";
		s_IconNames[26] = "data/ui/imgassets/tileicons/ti_controllersettingsps4_active.dds";
		s_IconNames[27] = "data/ui/imgassets/tileicons/ti_controllersettingsps4_nonactive.dds";
		s_IconNames[28] = "data/ui/imgassets/tileicons/ti_controllersettingsxbox_active.dds";
		s_IconNames[29] = "data/ui/imgassets/tileicons/ti_controllersettingsxbox_nonactive.dds";
		s_IconNames[30] = "data/ui/imgassets/tileicons/ti_createplayer_square_active.dds";
		s_IconNames[31] = "data/ui/imgassets/tileicons/ti_createplayer_square_nonactive.dds";
		s_IconNames[32] = "data/ui/imgassets/tileicons/ti_credits_square_active.dds";
		s_IconNames[33] = "data/ui/imgassets/tileicons/ti_credits_square_nonactive.dds";
		s_IconNames[34] = "data/ui/imgassets/tileicons/ti_deleteplayer_square_active.dds";
		s_IconNames[35] = "data/ui/imgassets/tileicons/ti_deleteplayer_square_nonactive.dds";
		s_IconNames[36] = "data/ui/imgassets/tileicons/ti_delete_square_active.dds";
		s_IconNames[37] = "data/ui/imgassets/tileicons/ti_delete_square_nonactive.dds";
		s_IconNames[38] = "data/ui/imgassets/tileicons/ti_disconnected_half_active.dds";
		s_IconNames[39] = "data/ui/imgassets/tileicons/ti_disconnected_half_nonactive.dds";
		s_IconNames[40] = "data/ui/imgassets/tileicons/ti_downloadupdates_square_active.dds";
		s_IconNames[41] = "data/ui/imgassets/tileicons/ti_downloadupdates_square_nonactive.dds";
		s_IconNames[42] = "data/ui/imgassets/tileicons/ti_eaaccountsettings_half_active.dds";
		s_IconNames[43] = "data/ui/imgassets/tileicons/ti_eaaccountsettings_half_nonactive.dds";
		s_IconNames[44] = "data/ui/imgassets/tileicons/ti_eaaccountsettings_square_active.dds";
		s_IconNames[45] = "data/ui/imgassets/tileicons/ti_eaaccountsettings_square_nonactive.dds";
		s_IconNames[46] = "data/ui/imgassets/tileicons/ti_easfc_logo_small_active.dds";
		s_IconNames[47] = "data/ui/imgassets/tileicons/ti_easfc_logo_small_nonactive.dds";
		s_IconNames[48] = "data/ui/imgassets/tileicons/ti_easportstrax_square_active.dds";
		s_IconNames[49] = "data/ui/imgassets/tileicons/ti_easportstrax_square_nonactive.dds";
		s_IconNames[50] = "data/ui/imgassets/tileicons/ti_editplayereasfclock_small_active.dds";
		s_IconNames[51] = "data/ui/imgassets/tileicons/ti_editplayereasfclock_small_nonactive.dds";
		s_IconNames[52] = "data/ui/imgassets/tileicons/ti_editplayers_small_active.dds";
		s_IconNames[53] = "data/ui/imgassets/tileicons/ti_editplayers_small_nonactive.dds";
		s_IconNames[54] = "data/ui/imgassets/tileicons/ti_editplayer_med_active.dds";
		s_IconNames[55] = "data/ui/imgassets/tileicons/ti_editplayer_med_nonactive.dds";
		s_IconNames[56] = "data/ui/imgassets/tileicons/ti_editplayer_square_active.dds";
		s_IconNames[57] = "data/ui/imgassets/tileicons/ti_editplayer_square_nonactive.dds";
		s_IconNames[58] = "data/ui/imgassets/tileicons/ti_editteams_square_active.dds";
		s_IconNames[59] = "data/ui/imgassets/tileicons/ti_editteams_square_nonactive.dds";
		s_IconNames[60] = "data/ui/imgassets/tileicons/ti_endmatchasdraw_med_active.dds";
		s_IconNames[61] = "data/ui/imgassets/tileicons/ti_endmatchasdraw_med_nonactive.dds";
		s_IconNames[62] = "data/ui/imgassets/tileicons/ti_enduserlicenseagreement_square_active.dds";
		s_IconNames[63] = "data/ui/imgassets/tileicons/ti_enduserlicenseagreement_square_nonactive.dds";
		s_IconNames[64] = "data/ui/imgassets/tileicons/ti_finances_med_active.dds";
		s_IconNames[65] = "data/ui/imgassets/tileicons/ti_finances_med_nonactive.dds";
		s_IconNames[66] = "data/ui/imgassets/tileicons/ti_fixtures_medtall_active.dds";
		s_IconNames[67] = "data/ui/imgassets/tileicons/ti_fixtures_medtall_nonactive.dds";
		s_IconNames[68] = "data/ui/imgassets/tileicons/ti_fixtures_small_active.dds";
		s_IconNames[69] = "data/ui/imgassets/tileicons/ti_fixtures_small_nonactive.dds";
		s_IconNames[70] = "data/ui/imgassets/tileicons/ti_formationsettings_square_active.dds";
		s_IconNames[71] = "data/ui/imgassets/tileicons/ti_formationsettings_square_nonactive.dds";
		s_IconNames[72] = "data/ui/imgassets/tileicons/ti_friendliesplaymatch_sml_active.dds";
		s_IconNames[73] = "data/ui/imgassets/tileicons/ti_friendliesplaymatch_sml_nonactive.dds";
		s_IconNames[74] = "data/ui/imgassets/tileicons/ti_gameplayassistance_active.dds";
		s_IconNames[75] = "data/ui/imgassets/tileicons/ti_gameplayassistance_nonactive.dds";
		s_IconNames[76] = "data/ui/imgassets/tileicons/ti_gameplayassistance_square_active.dds";
		s_IconNames[77] = "data/ui/imgassets/tileicons/ti_gameplayassistance_square_nonactive.dds";
		s_IconNames[78] = "data/ui/imgassets/tileicons/ti_gamesettings_square_active.dds";
		s_IconNames[79] = "data/ui/imgassets/tileicons/ti_gamesettings_square_nonactive.dds";
		s_IconNames[80] = "data/ui/imgassets/tileicons/ti_globaltransfernetwork_med_active.dds";
		s_IconNames[81] = "data/ui/imgassets/tileicons/ti_globaltransfernetwork_med_nonactive.dds";
		s_IconNames[82] = "data/ui/imgassets/tileicons/ti_goalcelebrations_square_active.dds";
		s_IconNames[83] = "data/ui/imgassets/tileicons/ti_goalcelebrations_square_nonactive.dds";
		s_IconNames[84] = "data/ui/imgassets/tileicons/ti_help_small_active.dds";
		s_IconNames[85] = "data/ui/imgassets/tileicons/ti_help_small_nonactive.dds";
		s_IconNames[86] = "data/ui/imgassets/tileicons/ti_help_square_active.dds";
		s_IconNames[87] = "data/ui/imgassets/tileicons/ti_help_square_nonactive.dds";
		s_IconNames[88] = "data/ui/imgassets/tileicons/ti_injurylist_small_active.dds";
		s_IconNames[89] = "data/ui/imgassets/tileicons/ti_injurylist_small_nonactive.dds";
		s_IconNames[90] = "data/ui/imgassets/tileicons/ti_instantreplay_small_active.dds";
		s_IconNames[91] = "data/ui/imgassets/tileicons/ti_instantreplay_small_nonactive.dds";
		s_IconNames[92] = "data/ui/imgassets/tileicons/ti_kinectsettings_square_active.dds";
		s_IconNames[93] = "data/ui/imgassets/tileicons/ti_kinectsettings_square_nonactive.dds";
		s_IconNames[94] = "data/ui/imgassets/tileicons/ti_kitnumbers_small_active.dds";
		s_IconNames[95] = "data/ui/imgassets/tileicons/ti_kitnumbers_small_nonactive.dds";
		s_IconNames[96] = "data/ui/imgassets/tileicons/ti_leaguetable_medtall_active.dds";
		s_IconNames[97] = "data/ui/imgassets/tileicons/ti_leaguetable_medtall_nonactive.dds";
		s_IconNames[98] = "data/ui/imgassets/tileicons/ti_loadfifaprofile_square_active.dds";
		s_IconNames[99] = "data/ui/imgassets/tileicons/ti_loadfifaprofile_square_nonactive.dds";
		s_IconNames[100] = "data/ui/imgassets/tileicons/ti_loadsquads_square_active.dds";
		s_IconNames[101] = "data/ui/imgassets/tileicons/ti_loadsquads_square_nonactive.dds";
		s_IconNames[102] = "data/ui/imgassets/tileicons/ti_matchdetails_med_active.dds";
		s_IconNames[103] = "data/ui/imgassets/tileicons/ti_matchdetails_med_nonactive.dds";
		s_IconNames[104] = "data/ui/imgassets/tileicons/ti_matchhighlights_small_active.dds";
		s_IconNames[105] = "data/ui/imgassets/tileicons/ti_matchhighlights_small_nonactive.dds";
		s_IconNames[106] = "data/ui/imgassets/tileicons/ti_matchmakingsettings_longmedium_active.dds";
		s_IconNames[107] = "data/ui/imgassets/tileicons/ti_matchmakingsettings_longmedium_nonactive.dds";
		s_IconNames[108] = "data/ui/imgassets/tileicons/ti_mutualquit_small_active.dds";
		s_IconNames[109] = "data/ui/imgassets/tileicons/ti_mutualquit_small_nonactive.dds";
		s_IconNames[110] = "data/ui/imgassets/tileicons/ti_myactions_small_active.dds";
		s_IconNames[111] = "data/ui/imgassets/tileicons/ti_myactions_small_nonactive.dds";
		s_IconNames[112] = "data/ui/imgassets/tileicons/ti_mycareer_small_active.dds";
		s_IconNames[113] = "data/ui/imgassets/tileicons/ti_mycareer_small_nonactive.dds";
		s_IconNames[114] = "data/ui/imgassets/tileicons/ti_nationalsquads_small_active.dds";
		s_IconNames[115] = "data/ui/imgassets/tileicons/ti_nationalsquads_small_nonactive.dds";
		s_IconNames[116] = "data/ui/imgassets/tileicons/ti_nationalsquads_square_active.dds";
		s_IconNames[117] = "data/ui/imgassets/tileicons/ti_nationalsquads_square_nonactive.dds";
		s_IconNames[118] = "data/ui/imgassets/tileicons/ti_natlsquadselection_small_active.dds";
		s_IconNames[119] = "data/ui/imgassets/tileicons/ti_natlsquadselection_small_nonactive.dds";
		s_IconNames[120] = "data/ui/imgassets/tileicons/ti_natlteamjoboffers_small_active.dds";
		s_IconNames[121] = "data/ui/imgassets/tileicons/ti_natlteamjoboffers_small_nonactive.dds";
		s_IconNames[122] = "data/ui/imgassets/tileicons/ti_nextmatch_small_active.dds";
		s_IconNames[123] = "data/ui/imgassets/tileicons/ti_nextmatch_small_nonactive.dds";
		s_IconNames[124] = "data/ui/imgassets/tileicons/ti_objectives_small_active.dds";
		s_IconNames[125] = "data/ui/imgassets/tileicons/ti_objectives_small_nonactive.dds";
		s_IconNames[126] = "data/ui/imgassets/tileicons/ti_onlinesettings_square_active.dds";
		s_IconNames[127] = "data/ui/imgassets/tileicons/ti_onlinesettings_square_nonactive.dds";
		s_IconNames[128] = "data/ui/imgassets/tileicons/ti_penalties_med_active.dds";
		s_IconNames[129] = "data/ui/imgassets/tileicons/ti_penalties_med_nonactive.dds";
		s_IconNames[130] = "data/ui/imgassets/tileicons/ti_playerratings_med_active.dds";
		s_IconNames[131] = "data/ui/imgassets/tileicons/ti_playerratings_med_nonactive.dds";
		s_IconNames[132] = "data/ui/imgassets/tileicons/ti_privacysettings_longmedium_active.dds";
		s_IconNames[133] = "data/ui/imgassets/tileicons/ti_privacysettings_longmedium_nonactive.dds";
		s_IconNames[134] = "data/ui/imgassets/tileicons/ti_privacysettings_square_active.dds";
		s_IconNames[135] = "data/ui/imgassets/tileicons/ti_privacysettings_square_nonactive.dds";
		s_IconNames[136] = "data/ui/imgassets/tileicons/ti_profile_square_active.dds";
		s_IconNames[137] = "data/ui/imgassets/tileicons/ti_profile_square_nonactive.dds";
		s_IconNames[138] = "data/ui/imgassets/tileicons/ti_psvr_square_active.dds";
		s_IconNames[139] = "data/ui/imgassets/tileicons/ti_psvr_square_nonactive.dds";
		s_IconNames[140] = "data/ui/imgassets/tileicons/ti_quit_small_active.dds";
		s_IconNames[141] = "data/ui/imgassets/tileicons/ti_quit_small_nonactive.dds";
		s_IconNames[142] = "data/ui/imgassets/tileicons/ti_remoteplay_longmedium.dds";
		s_IconNames[143] = "data/ui/imgassets/tileicons/ti_remoteplay_small_active.dds";
		s_IconNames[144] = "data/ui/imgassets/tileicons/ti_remoteplay_small_nonactive.dds";
		s_IconNames[145] = "data/ui/imgassets/tileicons/ti_requestfunds_small_active.dds";
		s_IconNames[146] = "data/ui/imgassets/tileicons/ti_requestfunds_small_nonactive.dds";
		s_IconNames[147] = "data/ui/imgassets/tileicons/ti_requestsub_small_active.dds";
		s_IconNames[148] = "data/ui/imgassets/tileicons/ti_requestsub_small_nonactive.dds";
		s_IconNames[149] = "data/ui/imgassets/tileicons/ti_resetallsquads_square_active.dds";
		s_IconNames[150] = "data/ui/imgassets/tileicons/ti_resetallsquads_square_nonactive.dds";
		s_IconNames[151] = "data/ui/imgassets/tileicons/ti_resetofsquads_square_active.dds";
		s_IconNames[152] = "data/ui/imgassets/tileicons/ti_resetofsquads_square_nonactive.dds";
		s_IconNames[153] = "data/ui/imgassets/tileicons/ti_resignnatlteam_small_active.dds";
		s_IconNames[154] = "data/ui/imgassets/tileicons/ti_resignnatlteam_small_nonactive.dds";
		s_IconNames[155] = "data/ui/imgassets/tileicons/ti_restartlocked_small_active.dds";
		s_IconNames[156] = "data/ui/imgassets/tileicons/ti_restartlocked_small_nonactive.dds";
		s_IconNames[157] = "data/ui/imgassets/tileicons/ti_restartwithnewteams_small_active.dds";
		s_IconNames[158] = "data/ui/imgassets/tileicons/ti_restartwithnewteams_small_nonactive.dds";
		s_IconNames[159] = "data/ui/imgassets/tileicons/ti_restart_small_active.dds";
		s_IconNames[160] = "data/ui/imgassets/tileicons/ti_restart_small_nonactive.dds";
		s_IconNames[161] = "data/ui/imgassets/tileicons/ti_resumematch_small_active.dds";
		s_IconNames[162] = "data/ui/imgassets/tileicons/ti_resumematch_small_nonactive.dds";
		s_IconNames[163] = "data/ui/imgassets/tileicons/ti_savefifaprofile_square_active.dds";
		s_IconNames[164] = "data/ui/imgassets/tileicons/ti_savefifaprofile_square_nonactive.dds";
		s_IconNames[165] = "data/ui/imgassets/tileicons/ti_savesquads_square_active.dds";
		s_IconNames[166] = "data/ui/imgassets/tileicons/ti_savesquads_square_nonactive.dds";
		s_IconNames[167] = "data/ui/imgassets/tileicons/ti_searchclubs_med_active.dds";
		s_IconNames[168] = "data/ui/imgassets/tileicons/ti_searchclubs_med_nonactive.dds";
		s_IconNames[169] = "data/ui/imgassets/tileicons/ti_selectkeeperarena_square_active.dds";
		s_IconNames[170] = "data/ui/imgassets/tileicons/ti_selectkeeperarena_square_nonactive.dds";
		s_IconNames[171] = "data/ui/imgassets/tileicons/ti_selectplayerarena_square_active.dds";
		s_IconNames[172] = "data/ui/imgassets/tileicons/ti_selectplayerarena_square_nonactive.dds";
		s_IconNames[173] = "data/ui/imgassets/tileicons/ti_selectsidespc_active.dds";
		s_IconNames[174] = "data/ui/imgassets/tileicons/ti_selectsidespc_nonactive.dds";
		s_IconNames[175] = "data/ui/imgassets/tileicons/ti_selectsidesps4_active.dds";
		s_IconNames[176] = "data/ui/imgassets/tileicons/ti_selectsidesps4_nonactive.dds";
		s_IconNames[177] = "data/ui/imgassets/tileicons/ti_selectsidesxbox_active.dds";
		s_IconNames[178] = "data/ui/imgassets/tileicons/ti_selectsidesxbox_nonactive.dds";
		s_IconNames[179] = "data/ui/imgassets/tileicons/ti_settings_smallw_active.dds";
		s_IconNames[180] = "data/ui/imgassets/tileicons/ti_settings_smallw_nonactive.dds";
		s_IconNames[181] = "data/ui/imgassets/tileicons/ti_settings_small_active.dds";
		s_IconNames[182] = "data/ui/imgassets/tileicons/ti_settings_small_nonactive.dds";
		s_IconNames[183] = "data/ui/imgassets/tileicons/ti_simmatch_smallw_active.dds";
		s_IconNames[184] = "data/ui/imgassets/tileicons/ti_simmatch_smallw_nonactive.dds";
		s_IconNames[185] = "data/ui/imgassets/tileicons/ti_simmatch_small_active.dds";
		s_IconNames[186] = "data/ui/imgassets/tileicons/ti_simmatch_small_nonactive.dds";
		s_IconNames[187] = "data/ui/imgassets/tileicons/ti_simmatch_sml_active.dds";
		s_IconNames[188] = "data/ui/imgassets/tileicons/ti_simmatch_sml_nonactive.dds";
		s_IconNames[189] = "data/ui/imgassets/tileicons/ti_skillmoves_square_active.dds";
		s_IconNames[190] = "data/ui/imgassets/tileicons/ti_skillmoves_square_nonactive.dds";
		s_IconNames[191] = "data/ui/imgassets/tileicons/ti_squadranking_small_active.dds";
		s_IconNames[192] = "data/ui/imgassets/tileicons/ti_squadranking_small_nonactive.dds";
		s_IconNames[193] = "data/ui/imgassets/tileicons/ti_squadranks_small_active.dds";
		s_IconNames[194] = "data/ui/imgassets/tileicons/ti_squadranks_small_nonactive.dds";
		s_IconNames[195] = "data/ui/imgassets/tileicons/ti_squadreport_small_active.dds";
		s_IconNames[196] = "data/ui/imgassets/tileicons/ti_squadreport_small_nonactive.dds";
		s_IconNames[197] = "data/ui/imgassets/tileicons/ti_stadiums_square_active.dds";
		s_IconNames[198] = "data/ui/imgassets/tileicons/ti_stadiums_square_nonactive.dds";
		s_IconNames[199] = "data/ui/imgassets/tileicons/ti_teamsheets_smallw_active.dds";
		s_IconNames[200] = "data/ui/imgassets/tileicons/ti_teamsheets_smallw_nonactive.dds";
		s_IconNames[201] = "data/ui/imgassets/tileicons/ti_teamsheets_small_active.dds";
		s_IconNames[202] = "data/ui/imgassets/tileicons/ti_teamsheets_small_nonactive.dds";
		s_IconNames[203] = "data/ui/imgassets/tileicons/ti_teamsheets_square_active.dds";
		s_IconNames[204] = "data/ui/imgassets/tileicons/ti_teamsheets_square_nonactive.dds";
		s_IconNames[205] = "data/ui/imgassets/tileicons/ti_teamstats_small_active.dds";
		s_IconNames[206] = "data/ui/imgassets/tileicons/ti_teamstats_small_nonactive.dds";
		s_IconNames[207] = "data/ui/imgassets/tileicons/ti_topscorers_medtall_active.dds";
		s_IconNames[208] = "data/ui/imgassets/tileicons/ti_topscorers_medtall_nonactive.dds";
		s_IconNames[209] = "data/ui/imgassets/tileicons/ti_trainer_small_active.dds";
		s_IconNames[210] = "data/ui/imgassets/tileicons/ti_trainer_small_nonactive.dds";
		s_IconNames[211] = "data/ui/imgassets/tileicons/ti_videocalibration_square_active.dds";
		s_IconNames[212] = "data/ui/imgassets/tileicons/ti_videocalibration_square_nonactive.dds";
		s_IconNames[213] = "data/ui/imgassets/tileicons/ti_voicerecognitionhelp_square_active.dds";
		s_IconNames[214] = "data/ui/imgassets/tileicons/ti_voicerecognitionhelp_square_nonactive.dds";
		s_IconNames[215] = "data/ui/imgassets/tileicons/ti_xboxonehelp_small_active.dds";
		s_IconNames[216] = "data/ui/imgassets/tileicons/ti_xboxonehelp_small_nonactive.dds";
	}

	private void LoadGameGraphics()
	{
		Cursor.Current = Cursors.WaitCursor;
		viewer2DMessi.CurrentBitmap = FifaEnvironment.GetArtasset("data/ui/game/screens/bootflow/pressstart.big", "2");
		viewer2DFifa.CurrentBitmap = FifaEnvironment.GetArtasset("data/ui/game/screens/bootflow/pressstart.big", "5");
		for (int i = 0; i < s_FileNames.Length; i++)
		{
			s_MenuBitmaps[i] = FifaEnvironment.GetDdsArtasset(s_FileNames[i]);
		}
		multiViewerMenuPictures.Bitmaps = s_MenuBitmaps;
		LoadIcon(0);
		Cursor.Current = Cursors.Default;
	}

	private bool SaveBitmapMenu(object sender)
	{
		Bitmap bitmap = (Bitmap)multiViewerMenuPictures.pictureBox.BackgroundImage;
		int num = (int)multiViewerMenuPictures.numeric.Value - 1;
		return FifaEnvironment.SetDdsArtasset(s_TemplateNames[num], s_FileNames[num], bitmap);
	}

	private bool DeleteBitmapMenu(object sender)
	{
		int num = (int)multiViewerMenuPictures.numeric.Value - 1;
		return FifaEnvironment.DeleteFromZdata(s_FileNames[num]);
	}

	private void buttonSaveStartGraphics_Click(object sender, EventArgs e)
	{
		FifaEnvironment.SetArtasset("data/ui/game/screens/bootflow/pressstart#.big", new string[2] { "2", "5" }, "data/ui/game/screens/bootflow/pressstart.big", new Bitmap[2] { viewer2DMessi.CurrentBitmap, viewer2DFifa.CurrentBitmap });
	}

	private void numericUpDown1_ValueChanged(object sender, EventArgs e)
	{
		int ix = (int)numericIcons.Value;
		LoadIcon(ix);
	}

	private void LoadIcon(int ix)
	{
		viewer2DIcons.CurrentBitmap = FifaEnvironment.GetDdsArtasset(s_IconNames[ix]);
		labelTextIcons.Text = Path.GetFileName(s_IconNames[ix]);
	}

	private bool ImportIcon(object sender, Bitmap bitmap)
	{
		int num = (int)numericIcons.Value;
		Bitmap bitmap2 = GraphicUtil.CanvasSizeBitmap(bitmap, 260, 260);
		return FifaEnvironment.SetDdsArtasset("data/ui/imgassets/tileicons/ti_#.dds", s_IconNames[num], bitmap2);
	}

	private bool DeleteIcon(object sender)
	{
		int num = (int)numericIcons.Value;
		return FifaEnvironment.DeleteFromZdata(s_IconNames[num]);
	}

	private void buttonReloadGraphics_Click(object sender, EventArgs e)
	{
		LoadGameGraphics();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.GameGraphicForm));
		this.groupMenu = new System.Windows.Forms.GroupBox();
		this.buttonSaveStartGraphics = new System.Windows.Forms.Button();
		this.viewer2DMessi = new FifaControls.Viewer2D();
		this.viewer2DFifa = new FifaControls.Viewer2D();
		this.multiViewerMenuPictures = new FifaControls.MultiViewer2D();
		this.multiViewerIcons = new FifaControls.MultiViewer2D();
		this.groupIcons = new System.Windows.Forms.GroupBox();
		this.numericIcons = new System.Windows.Forms.NumericUpDown();
		this.labelTextIcons = new System.Windows.Forms.Label();
		this.viewer2DIcons = new FifaControls.Viewer2D();
		this.buttonReloadGraphics = new System.Windows.Forms.Button();
		this.groupMenu.SuspendLayout();
		this.groupIcons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericIcons).BeginInit();
		base.SuspendLayout();
		this.groupMenu.Controls.Add(this.buttonSaveStartGraphics);
		this.groupMenu.Controls.Add(this.viewer2DMessi);
		this.groupMenu.Controls.Add(this.viewer2DFifa);
		this.groupMenu.Location = new System.Drawing.Point(3, 3);
		this.groupMenu.Name = "groupMenu";
		this.groupMenu.Size = new System.Drawing.Size(534, 308);
		this.groupMenu.TabIndex = 2;
		this.groupMenu.TabStop = false;
		this.groupMenu.Text = "Start";
		this.buttonSaveStartGraphics.Image = (System.Drawing.Image)resources.GetObject("buttonSaveStartGraphics.Image");
		this.buttonSaveStartGraphics.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonSaveStartGraphics.Location = new System.Drawing.Point(352, 209);
		this.buttonSaveStartGraphics.Name = "buttonSaveStartGraphics";
		this.buttonSaveStartGraphics.Size = new System.Drawing.Size(75, 23);
		this.buttonSaveStartGraphics.TabIndex = 2;
		this.buttonSaveStartGraphics.Text = "Save";
		this.buttonSaveStartGraphics.UseVisualStyleBackColor = true;
		this.buttonSaveStartGraphics.Click += new System.EventHandler(buttonSaveStartGraphics_Click);
		this.viewer2DMessi.AutoTransparency = false;
		this.viewer2DMessi.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DMessi.ButtonStripVisible = true;
		this.viewer2DMessi.CurrentBitmap = null;
		this.viewer2DMessi.ExtendedFormat = false;
		this.viewer2DMessi.FullSizeButton = true;
		this.viewer2DMessi.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DMessi.ImageSize = new System.Drawing.Size(2048, 2048);
		this.viewer2DMessi.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DMessi.Location = new System.Drawing.Point(6, 19);
		this.viewer2DMessi.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DMessi.Name = "viewer2DMessi";
		this.viewer2DMessi.RemoveButton = false;
		this.viewer2DMessi.ShowButton = true;
		this.viewer2DMessi.ShowButtonChecked = true;
		this.viewer2DMessi.Size = new System.Drawing.Size(256, 281);
		this.viewer2DMessi.TabIndex = 0;
		this.viewer2DFifa.AutoTransparency = false;
		this.viewer2DFifa.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DFifa.ButtonStripVisible = true;
		this.viewer2DFifa.CurrentBitmap = null;
		this.viewer2DFifa.ExtendedFormat = false;
		this.viewer2DFifa.FullSizeButton = true;
		this.viewer2DFifa.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DFifa.ImageSize = new System.Drawing.Size(1024, 512);
		this.viewer2DFifa.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DFifa.Location = new System.Drawing.Point(268, 19);
		this.viewer2DFifa.Margin = new System.Windows.Forms.Padding(4);
		this.viewer2DFifa.Name = "viewer2DFifa";
		this.viewer2DFifa.RemoveButton = false;
		this.viewer2DFifa.ShowButton = true;
		this.viewer2DFifa.ShowButtonChecked = true;
		this.viewer2DFifa.Size = new System.Drawing.Size(256, 153);
		this.viewer2DFifa.TabIndex = 1;
		this.multiViewerMenuPictures.AutoTransparency = false;
		this.multiViewerMenuPictures.Bitmaps = null;
		this.multiViewerMenuPictures.CheckBitmapSize = false;
		this.multiViewerMenuPictures.FixedSize = false;
		this.multiViewerMenuPictures.FullSizeButton = false;
		this.multiViewerMenuPictures.LabelText = "Image n.";
		this.multiViewerMenuPictures.Location = new System.Drawing.Point(542, 10);
		this.multiViewerMenuPictures.Margin = new System.Windows.Forms.Padding(2);
		this.multiViewerMenuPictures.Name = "multiViewerMenuPictures";
		this.multiViewerMenuPictures.ShowButton = false;
		this.multiViewerMenuPictures.ShowDeleteButton = true;
		this.multiViewerMenuPictures.Size = new System.Drawing.Size(384, 457);
		this.multiViewerMenuPictures.TabIndex = 3;
		this.multiViewerIcons.AutoTransparency = false;
		this.multiViewerIcons.Bitmaps = null;
		this.multiViewerIcons.CheckBitmapSize = false;
		this.multiViewerIcons.FixedSize = false;
		this.multiViewerIcons.FullSizeButton = false;
		this.multiViewerIcons.LabelText = "Image n.";
		this.multiViewerIcons.Location = new System.Drawing.Point(950, 320);
		this.multiViewerIcons.Margin = new System.Windows.Forms.Padding(2);
		this.multiViewerIcons.Name = "multiViewerIcons";
		this.multiViewerIcons.ShowButton = false;
		this.multiViewerIcons.ShowDeleteButton = true;
		this.multiViewerIcons.Size = new System.Drawing.Size(280, 336);
		this.multiViewerIcons.TabIndex = 4;
		this.groupIcons.Controls.Add(this.buttonReloadGraphics);
		this.groupIcons.Controls.Add(this.numericIcons);
		this.groupIcons.Controls.Add(this.labelTextIcons);
		this.groupIcons.Controls.Add(this.viewer2DIcons);
		this.groupIcons.Location = new System.Drawing.Point(6, 318);
		this.groupIcons.Name = "groupIcons";
		this.groupIcons.Size = new System.Drawing.Size(534, 338);
		this.groupIcons.TabIndex = 9;
		this.groupIcons.TabStop = false;
		this.groupIcons.Text = "Icons";
		this.numericIcons.Location = new System.Drawing.Point(23, 17);
		this.numericIcons.Maximum = new decimal(new int[4] { 216, 0, 0, 0 });
		this.numericIcons.Name = "numericIcons";
		this.numericIcons.Size = new System.Drawing.Size(113, 20);
		this.numericIcons.TabIndex = 11;
		this.numericIcons.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericIcons.ValueChanged += new System.EventHandler(numericUpDown1_ValueChanged);
		this.labelTextIcons.AutoSize = true;
		this.labelTextIcons.Location = new System.Drawing.Point(154, 21);
		this.labelTextIcons.Name = "labelTextIcons";
		this.labelTextIcons.Size = new System.Drawing.Size(35, 13);
		this.labelTextIcons.TabIndex = 10;
		this.labelTextIcons.Text = "label2";
		this.viewer2DIcons.AutoTransparency = false;
		this.viewer2DIcons.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DIcons.ButtonStripVisible = true;
		this.viewer2DIcons.CurrentBitmap = null;
		this.viewer2DIcons.ExtendedFormat = false;
		this.viewer2DIcons.FullSizeButton = false;
		this.viewer2DIcons.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DIcons.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DIcons.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DIcons.Location = new System.Drawing.Point(9, 46);
		this.viewer2DIcons.Name = "viewer2DIcons";
		this.viewer2DIcons.RemoveButton = true;
		this.viewer2DIcons.ShowButton = true;
		this.viewer2DIcons.ShowButtonChecked = true;
		this.viewer2DIcons.Size = new System.Drawing.Size(256, 283);
		this.viewer2DIcons.TabIndex = 6;
		this.buttonReloadGraphics.Location = new System.Drawing.Point(289, 35);
		this.buttonReloadGraphics.Name = "buttonReloadGraphics";
		this.buttonReloadGraphics.Size = new System.Drawing.Size(75, 23);
		this.buttonReloadGraphics.TabIndex = 12;
		this.buttonReloadGraphics.Text = "Reload";
		this.buttonReloadGraphics.UseVisualStyleBackColor = true;
		this.buttonReloadGraphics.Click += new System.EventHandler(buttonReloadGraphics_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1024, 780);
		base.Controls.Add(this.groupIcons);
		base.Controls.Add(this.multiViewerIcons);
		base.Controls.Add(this.multiViewerMenuPictures);
		base.Controls.Add(this.groupMenu);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "GameGraphicForm";
		this.Text = "Form1";
		base.Load += new System.EventHandler(GameGraphicForm_Load);
		this.groupMenu.ResumeLayout(false);
		this.groupIcons.ResumeLayout(false);
		this.groupIcons.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericIcons).EndInit();
		base.ResumeLayout(false);
	}
}
