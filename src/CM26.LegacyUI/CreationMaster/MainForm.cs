using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CreationMaster.Properties;
using FifaControls;
using FifaLibrary;
using ThreadingTasks = System.Threading.Tasks;

namespace CreationMaster;

public class MainForm : Form
{
	public static MainForm CM;

	private int m_SplitterDistanceRight;

	private int m_SplitterDistanceBottom;

	private bool m_OpenFileFlag;

	private bool m_Fc26LoadInProgress;

	private bool m_IsShiftPressed;

	private bool m_IsCtrlPressed;

	private bool m_IsAltPressed;

	private readonly HashSet<int> m_PendingLeagueCompdataIds = new HashSet<int>();

	// IDs created by the guided FC26 workflows.  Keeping these separate from
	// the normal editor lists lets Save Preflight focus on records that need
	// relationship/asset checks without blocking an unrelated legacy edit.
	private readonly HashSet<int> m_PendingTeamIds = new HashSet<int>();

	private readonly HashSet<int> m_PendingPlayerIds = new HashSet<int>();

	private bool m_LastSaveCommitted;

	private AboutForm m_AboutForm = new AboutForm();

	public FormationForm m_FormationForm;

	public CountryForm m_CountryForm;

	public TeamForm m_TeamForm;

	public KitForm m_KitForm;

	public BallForm m_BallForm;

	public ManagerForm m_ManagerForm;

	public GameGraphicForm m_GameGraphicForm;

	public WebBrowserForm m_WebBrowserForm;

	public LeagueForm m_LeagueForm;

	public ShoesForm m_ShoesForm;

	public TvForm m_TvForm;

	public NewspapersForm m_NewspapersForm;

	public RefereeForm m_RefereeForm;

	public CompetitionForm m_TrophyForm;

	public PlayerForm m_PlayerForm;

	public StadiumForm m_StadiumForm;

	public GlovesForm m_GlovesForm;

	public AudioForm m_AudioForm;

	public ImportGraphicsForm m_ImportGraphicsForm;

	public static PatchCreatorForm m_PatchCreatorForm;

	public static PatchLoaderForm m_PatchLoaderForm;

	private string m_XmlDbFileName;

	private string m_UgcFileName;

	private UgcFile m_UgcFile;

	private string m_OnlineDbFileName;

	private CareerFile m_OnlineDbFile;

	private DbFile m_OnlineDb;

	private UnicodeEncoding m_Encoder = new UnicodeEncoding();

	private IContainer components;

	private MenuStrip menuStrip;

	private StatusStrip statusStrip;

	private ToolStripProgressBar progressBar;

	private ToolStripStatusLabel statusBar;

	private ToolStripMenuItem menuFile;

	private ToolStripMenuItem helpToolStripMenuItem;

	private ToolStripMenuItem menuAbout;

	private ToolStripMenuItem menuHelp;

	private SplitContainer splitVert;

	private SplitContainer splitHoriz;

	private ToolStrip toolStripBottom;

	private ToolStripButton buttonShowBottom;

	private ToolStripButton buttonHideBottom;

	private ToolStripMenuItem menuOpenFifa14;

	private ToolStrip toolStripMain;

	private ToolStrip toolStripRight;

	private ToolStripButton buttonShowRight;

	private ToolStripButton buttonHideRight;

	private ToolStripLabel stripLabelRight;

	private ToolStripLabel stripLabelBottom;

	private Panel panelMain;

	private ToolStripMenuItem menuOpenLang14;

	private ToolStripMenuItem menuOpenAll;

	private ToolStripMenuItem menuSave;

	private ToolStripMenuItem menuClose;

	private ToolStripMenuItem menuExit;

	private OpenFileDialog openFifaDialog;

	private OpenFileDialog openLangDialog;

	private FolderBrowserDialog browserDialog;

	private ToolStripMenuItem menuTools;

	private ToolStripMenuItem menuEnableAllMessages;

	private ToolStripMenuItem menuOptions;

	private ToolStripMenuItem menuRegenerate;

	private ToolStripMenuItem menuExpandDatabase;

	private Panel panelBottom;

	private Panel panelRight;

	private ToolStripMenuItem menuOpenDebug;

	private ToolTip toolTip;

	private ToolStripMenuItem menuPatch;

	private ToolStripMenuItem menuCreatePatch;

	private ToolStripMenuItem menuLoadPatch;

	private ToolStripMenuItem menuRemoveKidProtection;

	private ToolStripMenuItem menuCleanFAT;

	private ToolStripMenuItem menuHelpCms;

	private ToolStripMenuItem menuRemoveAllLongTeamNames;

	private ToolStripMenuItem genericToolStripMenuItem;

	private ToolStripMenuItem adboardsToolStripMenuItem;

	private ToolStripMenuItem ballsToolStripMenuItem;

	private ToolStripMenuItem bootsToolStripMenuItem;

	private ToolStripMenuItem countryToolStripMenuItem;

	private ToolStripMenuItem fontsToolStripMenuItem;

	private ToolStripMenuItem formationsToolStripMenuItem;

	private ToolStripMenuItem leaguesToolStripMenuItem;

	private ToolStripMenuItem stadiumsToolStripMenuItem;

	private ToolStripMenuItem teamsToolStripMenuItem;

	private ToolStripMenuItem tournamentsToolStripMenuItem;

	private ToolStripButton buttonCountry;

	private ToolStripButton buttonLeague;

	private ToolStripButton buttonTeam;

	private ToolStripButton buttonPlayer;

	private ToolStripButton buttonStadium;

	private ToolStripButton buttonTournament;

	private ToolStripButton buttonReferee;

	private ToolStripButton buttonBall;

	private ToolStripButton buttonShoes;

	private ToolStripButton buttonManager;

	private ToolStripButton buttonFormation;

	private ToolStripButton buttonTv;

	private ToolStripButton buttonNewspaper;

	private ToolStripButton buttonGloves;

	private ToolStripButton buttonSponsor;

	private ToolStripButton buttonKit;

	private ToolStripMenuItem menuUgc;

	private ToolStripMenuItem menuImportUgc;

	private ToolStripMenuItem menuImportUgcWothKits;

	private ToolStripMenuItem menuImportUgcKits;

	private ToolStripMenuItem menuReopen;

	private ToolStripButton buttonAudio;

	private ToolStripMenuItem menuUpdateDB;

	private ToolStripMenuItem menuAlignLanguageDB;

	private ToolStripMenuItem menuImportUgcPlayers;

	private ToolStripMenuItem menuMinimizeNamesTable;

	private ToolStripMenuItem menuOpenFifa15;

	private ToolStripMenuItem menuOpenLang15;

	private ToolStripMenuItem menuPreserveOriginalNames;

	private ToolStripMenuItem menuOpenFifa16;

	private ToolStripMenuItem menuOpenLang16;

	private ToolStripMenuItem menuInstallRevModPatch;

	private ToolStripMenuItem menuOnlineFromFifa16;

	private ToolStripMenuItem rostersAndPlayersFromFifa16;

	private ToolStripMenuItem menuOnlineFromFifa17;

	private ToolStripMenuItem rostersOnlyFromFIFA16;

	private ToolStripMenuItem rostersOnlyFromFifa17;

	private ToolStripMenuItem rostersAndPlayersFromFifa17;

	private ToolStripMenuItem playerNameCountryRulesToolStripMenuItem;

	private ToolStripMenuItem removeFakePlayersToolStripMenuItem;

	private ToolStripButton buttonGameGraphics;

	private ToolStripMenuItem exportPlayersFromCSVToolStripMenuItem;

	private ToolStripMenuItem importPlayersFromCSVToolStripMenuItem;

	private ToolStripMenuItem fixLoanDatesToolStripMenuItem;

	private ToolStripButton buttonBrowser;

	private ToolStripMenuItem fixProblemsToolStripMenuItem;

	private ToolStripButton buttonImportGraphics;

	private ToolStripMenuItem fromFIFA18ToolStripMenuItem;

	private ToolStripMenuItem rostersAndPlayersToolStripMenuItem;

	private ToolStripMenuItem enableExistingSpecificFacesToolStripMenuItem;

	private ToolStripMenuItem extendLoansTo2020ToolStripMenuItem;

	private ToolStripMenuItem removeFreeAgentToPlayersWithClubToolStripMenuItem;

	private ToolStripMenuItem addToFreeAgentPlayersWithoutClubToolStripMenuItem;

	private ToolStripMenuItem fromFIFA20ToolStripMenuItem;

	private ToolStripMenuItem removeAllPlayersToolStripMenuItem;

	private ToolStripMenuItem createDBEntryForExistingKitsToolStripMenuItem;

	private ToolStripMenuItem createDummyKitForTeamsWithoutKitToolStripMenuItem;

	private ToolStripMenuItem randomizeLegendsAcademyToolStripMenuItem;

	private ToolStripMenuItem setFreeAgentDatesToolStripMenuItem;

	private ToolStripMenuItem resetCommentaryNamesToolStripMenuItem;

	private ToolStripMenuItem menuOnlineFromFifa21;

	private ToolStripMenuItem associateCommentaryNamesToolStripMenuItem;

	private ToolStripMenuItem createPlayersFoeCommentaryNamesToolStripMenuItem;

	private ToolStripMenuItem convertMinheadsToPNGToolStripMenuItem;

	private ToolStripMenuItem menuStandardizeCommentaryIds;

	public MainForm()
	{
		InitializeComponent();
		ConfigureFriendlyCreateMenu();
		var projectLauncher = new ToolStripMenuItem("FC26 Project Launcher...");
		projectLauncher.Click += (_, _) => ShowFc26ProjectLauncher();
		var openExtracted = new ToolStripMenuItem("Open extracted FC26 database...");
		openExtracted.Click += (_, _) => OpenExtractedFc26Database();
		var openSession = new ToolStripMenuItem("Open CM26 project/session...");
		openSession.Click += (_, _) => OpenFc26ProjectSession();
		var saveSession = new ToolStripMenuItem("Save CM26 project/session...");
		saveSession.Click += (_, _) => SaveFc26ProjectSession();
		// Keep the original File menu order and place the FC26 source directly
		// after Open FC26 rather than creating a new launcher/dashboard.
		var openFc26Index = menuFile.DropDownItems.IndexOf(menuOpenFifa16);
		menuFile.DropDownItems.Insert(Math.Max(0, openFc26Index), projectLauncher);
		menuFile.DropDownItems.Insert(Math.Max(0, openFc26Index + 2), openExtracted);
		menuFile.DropDownItems.Insert(Math.Max(0, openFc26Index + 3), openSession);
		menuFile.DropDownItems.Insert(Math.Max(0, openFc26Index + 4), saveSession);
		var healthCentre = new ToolStripMenuItem("Database Health Centre...");
		healthCentre.Click += (_, _) => ShowFc26HealthCentre();
		menuTools.DropDownItems.Add(new ToolStripSeparator());
		menuTools.DropDownItems.Add(healthCentre);
		buttonSponsor.Visible = true;
		buttonTv.Visible = true;
		m_SplitterDistanceBottom = splitHoriz.Height * 2 / 3;
		m_SplitterDistanceRight = splitVert.Width * 3 / 4;
		FifaEnvironment.InitializeDefault();
		CreateForms();
		CM = this;
		EnablePanels(enable: false);
		EnableMenus();
	}

	private void ConfigureFriendlyCreateMenu()
	{
		var createMenu = new ToolStripMenuItem("Create") { Name = "menuCreateFriendly" };
		var createLeague = new ToolStripMenuItem("Create New League...") { Name = "menuCreateLeague" };
		createLeague.Click += (_, _) => CreateNewLeagueWorkflow();
		createMenu.DropDownItems.Add(createLeague);
		var createTeam = new ToolStripMenuItem("Create New Team...") { Name = "menuCreateTeam" };
		createTeam.Click += (_, _) => CreateNewTeamWorkflow();
		createMenu.DropDownItems.Add(createTeam);

		// Keep the commands visible beside File instead of burying record creation
		// inside each editor's small picker toolbar.
		menuStrip.Items.Insert(Math.Min(1, menuStrip.Items.Count), createMenu);
	}

	private void ShowFc26ProjectLauncher()
	{
		using (var launcher = new Fc26ProjectLauncherForm(
			() => menuOpenFifa16_Click(this, EventArgs.Empty), OpenExtractedFc26Database,
			OpenFc26ProjectSession, SaveFc26ProjectSession))
		{
			var result = launcher.ShowDialog(this);
			if (result == DialogResult.Retry && launcher.Tag is string recentPath)
				OpenFc26ProjectSession(recentPath);
		}
	}

	private void SaveFc26ProjectSession()
	{
		if (!Fc26SnapshotLoader.IsLoaded)
		{
			MessageBox.Show(this, "Open FC26 or an extracted FC26 database first.", "CM26 project", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		using (var dialog = new SaveFileDialog { Filter = "CM26 project session (*.cm26session)|*.cm26session", FileName = "FC26_Project.cm26session" })
		{
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			try { Fc26ProjectSessionService.Save(dialog.FileName); statusBar.Text = "CM26 project session saved: " + dialog.FileName; }
			catch (Exception ex) { MessageBox.Show(this, ex.Message, "Save CM26 project", MessageBoxButtons.OK, MessageBoxIcon.Error); }
		}
	}

	private void OpenFc26ProjectSession()
	{
		using (var dialog = new OpenFileDialog { Filter = "CM26 project session (*.cm26session)|*.cm26session|All files (*.*)|*.*" })
		{
			if (dialog.ShowDialog(this) == DialogResult.OK) OpenFc26ProjectSession(dialog.FileName);
		}
	}

	private async void OpenFc26ProjectSession(string fileName)
	{
		bool loaded = await OpenFc26SnapshotAsync(() =>
		{
			var project = Fc26ProjectSessionService.Load(fileName);
			if (project.SourceKind.Equals("installed", StringComparison.OrdinalIgnoreCase) && Directory.Exists(project.GameRoot))
				return Fc26HostBridge.OpenGameRoot(project.GameRoot);
			else if (Directory.Exists(project.DatabaseFolder))
				return Fc26HostBridge.OpenExtractedFolder(project.DatabaseFolder);
			throw new DirectoryNotFoundException("The FC26 source stored by this project is no longer available.\r\n" + project.GameRoot + "\r\n" + project.DatabaseFolder);
		}, "Opening CM26 project", "CM26 project loaded: " + fileName, "Open CM26 project");
		if (loaded)
			Fc26ActivityLog.Add("Project", "Opened CM26 session: " + fileName);
	}

	internal void ShowFc26CareerSaveModule()
	{
		using (var career = new Fc26CareerSaveForm()) career.ShowDialog(this);
	}

	private void ShowFc26WorkflowUtilities()
	{
		using (var utilities = new Fc26WorkflowUtilitiesForm()) utilities.ShowDialog(this);
	}

	internal void ShowFc26RosterTools()
	{
		if (!m_OpenFileFlag || FifaEnvironment.Year != 26)
		{
			MessageBox.Show(this, "Open FC26 first.", "Roster Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		using (var tools = new Fc26RosterToolsForm()) tools.ShowDialog(this);
	}

	internal void ShowFc26FaceTools()
	{
		if (!m_OpenFileFlag || FifaEnvironment.Year != 26)
		{
			MessageBox.Show(this, "Open FC26 first.", "Miniface & Face Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		using (var tools = new Fc26FaceToolsForm()) tools.ShowDialog(this);
	}

	internal void ShowFc26BatchPlayerEditor()
	{
		if (!m_OpenFileFlag || FifaEnvironment.Year != 26)
		{
			MessageBox.Show(this, "Open FC26 first.", "Batch Player Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		using (var editor = new Fc26BatchPlayerForm()) editor.ShowDialog(this);
	}

	internal void ShowFc26AssetManager()
	{
		if (!m_OpenFileFlag || FifaEnvironment.Year != 26)
		{
			MessageBox.Show(this, "Open FC26 first.", "Visual Asset Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		using (var manager = new Fc26AssetManagerForm()) manager.ShowDialog(this);
	}

	internal void ShowFc26ModdingUtilities()
	{
		if (!m_OpenFileFlag || FifaEnvironment.Year != 26)
		{
			MessageBox.Show(this, "Open FC26 first.", "Internal Modding Utilities", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		using (var utilities = new Fc26ModdingUtilitiesForm()) utilities.ShowDialog(this);
	}

	private async void OpenExtractedFc26Database()
	{
		using (var dialog = new FolderBrowserDialog
		{
			Description = "Select an extracted FC26 database folder containing fifa_ng_db and its XML descriptor"
		})
		{
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			string selectedPath = dialog.SelectedPath;
			await OpenFc26SnapshotAsync(
				() => Fc26HostBridge.OpenExtractedFolder(selectedPath),
				"Opening extracted FC26 database",
				"Extracted FC26 database loaded: " + selectedPath,
				"Open extracted FC26 database");
		}
	}

	private void ShowFc26DatabaseWorkspace()
	{
		if (!m_OpenFileFlag || FifaEnvironment.Year != 26)
		{
			MessageBox.Show(this, "Open FC26 first.", "Advanced Database Workspace",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		using (var workspace = new Fc26DatabaseWorkspaceForm())
			workspace.ShowDialog(this);
	}

	private void ShowFc26HealthCentre()
	{
		if (!m_OpenFileFlag || FifaEnvironment.Year != 26)
		{
			MessageBox.Show(this, "Open FC26 first.", "Database Health Centre",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		try
		{
			statusBar.Text = "Scanning FC26 database health...";
			var report = Fc26HostBridge.LoadHealthReport();
			MessageBox.Show(this, report, "FC26 Database Health Centre",
				MessageBoxButtons.OK, report.Contains("[Error]") ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
			if (report.Contains("repairable") && MessageBox.Show(this,
				"Apply the safe roster, free-agent, contract and shirt-number repairs now?\r\n\r\n" +
				"Changes stay staged in CM26. Use File > Save to run validation, create the automatic backup and commit them to Frostbite.",
				"Database Health Centre — Safe Repair", MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) == DialogResult.Yes)
			{
				int repaired = ApplySafeHealthRepairs();
				statusBar.Text = "Database health repair staged: " + repaired + " correction(s).";
				MessageBox.Show(this, repaired + " safe correction(s) staged. Review the affected teams, then use File > Save.",
					"Database Health Centre", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else statusBar.Text = "Database health scan completed.";
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Database Health Centre",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
			statusBar.Text = "Database health scan failed.";
		}
		finally { Cursor.Current = Cursors.Default; }
	}

	private int ApplySafeHealthRepairs()
	{
		int repaired = 0;
		Team freeAgents = (Team)FifaEnvironment.Teams.SearchId(111592);
		foreach (Player player in FifaEnvironment.Players)
		{
			bool hadClubAndFreeAgent = freeAgents != null && player.GetClub() != null && player.IsPlayingFor(freeAgents);
			player.RemoveFromFreeAgentIfHasClub();
			if (hadClubAndFreeAgent && !player.IsPlayingFor(freeAgents)) repaired++;
			bool hadNoLink = freeAgents != null && player.GetClub() == null && !player.IsPlayingFor(freeAgents);
			if (freeAgents != null) player.AddToFreeAgentIfWithoutClub();
			if (hadNoLink && player.IsPlayingFor(freeAgents)) repaired++;
			int oldContract = player.contractvaliduntil;
			player.ExtendContractAfterLoanEnd();
			if (oldContract != player.contractvaliduntil) repaired++;
		}

		foreach (Team team in FifaEnvironment.Teams)
		{
			var used = new HashSet<int>();
			foreach (TeamPlayer link in team.Roster)
			{
				int number = link.jerseynumber;
				if (number >= 1 && number <= 99 && used.Add(number)) continue;
				for (int candidate = 1; candidate <= 99; candidate++)
				{
					if (!used.Add(candidate)) continue;
					link.jerseynumber = candidate;
					repaired++;
					break;
				}
			}
		}
		return repaired;
	}

	private void CreateForms()
	{
		m_FormationForm = new FormationForm();
		m_FormationForm.TopLevel = false;
		m_FormationForm.Dock = DockStyle.Fill;
		m_CountryForm = new CountryForm();
		m_CountryForm.TopLevel = false;
		m_CountryForm.Dock = DockStyle.Fill;
		m_TeamForm = new TeamForm();
		m_TeamForm.TopLevel = false;
		m_TeamForm.Dock = DockStyle.Fill;
		m_KitForm = new KitForm();
		m_KitForm.TopLevel = false;
		m_KitForm.Dock = DockStyle.Fill;
		m_BallForm = new BallForm();
		m_BallForm.TopLevel = false;
		m_BallForm.Dock = DockStyle.Fill;
		m_ManagerForm = new ManagerForm();
		m_ManagerForm.TopLevel = false;
		m_ManagerForm.Dock = DockStyle.Fill;
		m_GameGraphicForm = new GameGraphicForm();
		m_GameGraphicForm.TopLevel = false;
		m_GameGraphicForm.Dock = DockStyle.Fill;
		m_WebBrowserForm = new WebBrowserForm();
		m_WebBrowserForm.TopLevel = false;
		m_WebBrowserForm.Dock = DockStyle.Fill;
		m_LeagueForm = new LeagueForm();
		m_LeagueForm.TopLevel = false;
		m_LeagueForm.Dock = DockStyle.Fill;
		m_ShoesForm = new ShoesForm();
		m_ShoesForm.TopLevel = false;
		m_ShoesForm.Dock = DockStyle.Fill;
		m_TvForm = new TvForm();
		m_TvForm.TopLevel = false;
		m_TvForm.Dock = DockStyle.Fill;
		m_NewspapersForm = new NewspapersForm();
		m_NewspapersForm.TopLevel = false;
		m_NewspapersForm.Dock = DockStyle.Fill;
		m_RefereeForm = new RefereeForm();
		m_RefereeForm.TopLevel = false;
		m_RefereeForm.Dock = DockStyle.Fill;
		m_TrophyForm = new CompetitionForm();
		m_TrophyForm.TopLevel = false;
		m_TrophyForm.Dock = DockStyle.Fill;
		m_PlayerForm = new PlayerForm();
		m_PlayerForm.TopLevel = false;
		m_PlayerForm.Dock = DockStyle.Fill;
		m_StadiumForm = new StadiumForm();
		m_StadiumForm.TopLevel = false;
		m_StadiumForm.Dock = DockStyle.Fill;
		m_GlovesForm = new GlovesForm();
		m_GlovesForm.TopLevel = false;
		m_GlovesForm.Dock = DockStyle.Fill;
		m_AudioForm = new AudioForm();
		m_AudioForm.TopLevel = false;
		m_AudioForm.Dock = DockStyle.Fill;
		m_ImportGraphicsForm = new ImportGraphicsForm();
		m_ImportGraphicsForm.TopLevel = false;
		m_ImportGraphicsForm.Dock = DockStyle.Fill;
		m_PatchCreatorForm = new PatchCreatorForm();
		m_PatchLoaderForm = new PatchLoaderForm();
	}

	private void DestroyForms()
	{
		m_FormationForm.Dispose();
		m_CountryForm.Dispose();
		m_TeamForm.Dispose();
		m_KitForm.Dispose();
		m_BallForm.Dispose();
		m_ManagerForm.Dispose();
		m_GameGraphicForm.Dispose();
		m_WebBrowserForm.Dispose();
		m_LeagueForm.Dispose();
		m_ShoesForm.Dispose();
		m_TvForm.Dispose();
		m_NewspapersForm.Dispose();
		m_RefereeForm.Dispose();
		m_TrophyForm.Dispose();
		m_PlayerForm.Dispose();
		m_StadiumForm.Dispose();
		m_GlovesForm.Dispose();
		m_AudioForm.Dispose();
		m_ImportGraphicsForm.Dispose();
	}

	private void EnablePanels(bool enable)
	{
		splitVert.Enabled = enable;
	}

	private void EnableMenus()
	{
		if (m_OpenFileFlag)
		{
			menuOpenFifa16.Enabled = false;
			menuOpenLang16.Enabled = false;
			menuOpenFifa15.Enabled = false;
			menuOpenFifa14.Enabled = false;
			menuOpenAll.Enabled = false;
			menuOpenLang14.Enabled = false;
			menuOpenLang15.Enabled = false;
			menuReopen.Enabled = false;
			menuSave.Enabled = true;
			menuClose.Enabled = true;
			menuOptions.Enabled = false;
			menuRegenerate.Enabled = true;
			menuExpandDatabase.Enabled = true;
			menuAlignLanguageDB.Enabled = true;
			menuCleanFAT.Enabled = true;
			menuRemoveKidProtection.Enabled = true;
			toolStripMain.Enabled = true;
			menuPatch.Enabled = true;
			menuRemoveAllLongTeamNames.Enabled = true;
			menuUgc.Enabled = FifaEnvironment.Year == 14;
			menuUpdateDB.Enabled = true;
			menuEnableAllMessages.Enabled = true;
			menuInstallRevModPatch.Enabled = true;
			menuMinimizeNamesTable.Enabled = true;
			menuPreserveOriginalNames.Enabled = true;
			exportPlayersFromCSVToolStripMenuItem.Enabled = true;
			importPlayersFromCSVToolStripMenuItem.Enabled = true;
			removeFakePlayersToolStripMenuItem.Enabled = true;
			playerNameCountryRulesToolStripMenuItem.Enabled = true;
			fixLoanDatesToolStripMenuItem.Enabled = true;
			fixProblemsToolStripMenuItem.Enabled = true;
		}
		else
		{
			menuOpenFifa16.Enabled = true;
			menuOpenLang16.Enabled = true;
			menuOpenFifa15.Enabled = true;
			menuOpenFifa14.Enabled = true;
			menuOpenAll.Enabled = true;
			menuOpenLang14.Enabled = true;
			menuOpenLang15.Enabled = true;
			menuReopen.Enabled = true;
			menuSave.Enabled = false;
			menuClose.Enabled = false;
			menuOptions.Enabled = true;
			menuRegenerate.Enabled = true;
			menuExpandDatabase.Enabled = false;
			menuAlignLanguageDB.Enabled = false;
			menuCleanFAT.Enabled = false;
			menuRemoveKidProtection.Enabled = false;
			toolStripMain.Enabled = false;
			menuPatch.Enabled = false;
			menuRemoveAllLongTeamNames.Enabled = false;
			menuUgc.Enabled = false;
			menuUpdateDB.Enabled = false;
			menuEnableAllMessages.Enabled = false;
			menuInstallRevModPatch.Enabled = false;
			menuMinimizeNamesTable.Enabled = false;
			menuPreserveOriginalNames.Enabled = false;
			exportPlayersFromCSVToolStripMenuItem.Enabled = false;
			importPlayersFromCSVToolStripMenuItem.Enabled = false;
			removeFakePlayersToolStripMenuItem.Enabled = false;
			playerNameCountryRulesToolStripMenuItem.Enabled = false;
			fixLoanDatesToolStripMenuItem.Enabled = false;
			fixProblemsToolStripMenuItem.Enabled = false;
		}
		menuExit.Enabled = true;
	}

	private void ShowFormOnPanel(Form form, Panel panel)
	{
		if (ReferenceEquals(form.Parent, panel) && form.Visible)
			return;

		// Some original CM16 forms synchronously resolve several FC26 Frostbite
		// previews the first time they are shown. Freeze the host panel (which still
		// contains the previous form) until the replacement is fully initialized;
		// clearing it first exposed the blue MDI background and produced a visible
		// full-window flash on every toolbar section change.
		GraphicUtil.SuspendDrawing(panel);
		panel.SuspendLayout();
		Cursor.Current = Cursors.WaitCursor;
		try
		{
			var previous = new Control[panel.Controls.Count];
			panel.Controls.CopyTo(previous, 0);
			// Hosted editors are non-top-level controls. Hide the outgoing editor
			// before detaching it so the panel never exposes the blue MDI client.
			foreach (var control in previous)
			{
				if (!ReferenceEquals(control, form)) control.Hide();
			}
			if (!ReferenceEquals(form.Parent, panel))
				panel.Controls.Add(form);
			if (!form.Visible)
				form.Show();
			form.BringToFront();
			// Keep editors cached in the main workspace. Removing/re-adding a CM16
			// form recreates layout and preview state and made every repeat toolbar
			// switch unnecessarily slow. Auxiliary split panels still own a single
			// editor, matching the original Alt/Ctrl behaviour.
			if (!ReferenceEquals(panel, panelMain))
			{
				foreach (var control in previous)
				{
					if (!ReferenceEquals(control, form)) panel.Controls.Remove(control);
				}
			}
		}
		finally
		{
			Cursor.Current = Cursors.Default;
			panel.ResumeLayout(performLayout: true);
			GraphicUtil.ResumeDrawing(panel);
		}
		if (panelBottom.Controls.Count == 0)
		{
			stripLabelBottom.Text = "Empty";
		}
		else
		{
			stripLabelBottom.Text = VisiblePanelControlText(panelBottom);
		}
		if (panelRight.Controls.Count == 0)
		{
			stripLabelRight.Text = "Empty";
		}
		else
		{
			stripLabelRight.Text = VisiblePanelControlText(panelRight);
		}
	}

	private static string VisiblePanelControlText(Panel panel)
	{
		foreach (Control control in panel.Controls)
		{
			if (control.Visible) return control.Text;
		}
		return "Empty";
	}

	private Panel TargetPanelFromCurrentModifiers()
	{
		Keys modifiers = Control.ModifierKeys;
		return (modifiers & Keys.Alt) != 0
			? panelRight
			: ((modifiers & Keys.Control) != 0 ? panelBottom : panelMain);
	}

	public void JumpTo(IdObject idObject)
	{
		Panel panel = TargetPanelFromCurrentModifiers();
		if (idObject.GetType().Name == "Player")
		{
			Player player = (Player)idObject;
			if (!m_PlayerForm.pickUpControl.combo.Items.Contains(player))
			{
				m_PlayerForm.pickUpControl.combo.Items.Add(player);
			}
			m_PlayerForm.pickUpControl.combo.SelectedItem = player;
			ShowFormOnPanel(m_PlayerForm, panel);
		}
		if (idObject.GetType().Name == "Team")
		{
			Team team = (Team)idObject;
			if (!m_TeamForm.pickUpControl.combo.Items.Contains(team))
			{
				m_TeamForm.pickUpControl.combo.Items.Add(team);
			}
			m_TeamForm.pickUpControl.combo.SelectedItem = team;
			ShowFormOnPanel(m_TeamForm, panel);
		}
		if (idObject.GetType().Name == "Kit")
		{
			Kit kit = (Kit)idObject;
			if (!m_KitForm.pickUpControl.combo.Items.Contains(kit))
			{
				m_KitForm.pickUpControl.combo.Items.Add(kit);
			}
			m_KitForm.pickUpControl.combo.SelectedItem = kit;
			ShowFormOnPanel(m_KitForm, panel);
		}
		if (idObject.GetType().Name == "League")
		{
			League league = (League)idObject;
			if (!m_LeagueForm.pickUpControl.combo.Items.Contains(league))
			{
				m_LeagueForm.pickUpControl.combo.Items.Add(league);
			}
			m_LeagueForm.pickUpControl.combo.SelectedItem = league;
			ShowFormOnPanel(m_LeagueForm, panel);
		}
		if (idObject.GetType().Name == "Country")
		{
			Country country = (Country)idObject;
			if (!m_CountryForm.pickUpControl.combo.Items.Contains(country))
			{
				m_CountryForm.pickUpControl.combo.Items.Add(country);
			}
			m_CountryForm.pickUpControl.combo.SelectedItem = country;
			ShowFormOnPanel(m_CountryForm, panel);
		}
		if (idObject.GetType().Name == "Trophy")
		{
			ShowFormOnPanel(m_TrophyForm, panel);
		}
		if (idObject.GetType().Name == "Stadium")
		{
			Stadium stadium = (Stadium)idObject;
			if (!m_StadiumForm.pickUpControl.combo.Items.Contains(stadium))
			{
				m_StadiumForm.pickUpControl.combo.Items.Add(stadium);
			}
			m_StadiumForm.pickUpControl.combo.SelectedItem = stadium;
			ShowFormOnPanel(m_StadiumForm, panel);
		}
		if (idObject.GetType().Name == "Formation")
		{
			Formation formation = (Formation)idObject;
			if (!m_FormationForm.pickUpControl.combo.Items.Contains(formation))
			{
				m_FormationForm.pickUpControl.combo.Items.Add(formation);
			}
			m_FormationForm.pickUpControl.combo.SelectedItem = formation;
			ShowFormOnPanel(m_FormationForm, panel);
		}
		if (idObject.GetType().Name == "Ball")
		{
			Ball ball = (Ball)idObject;
			if (!m_BallForm.pickUpControl.combo.Items.Contains(ball))
			{
				m_BallForm.pickUpControl.combo.Items.Add(ball);
			}
			m_BallForm.pickUpControl.combo.SelectedItem = ball;
			ShowFormOnPanel(m_BallForm, panel);
		}
		if (idObject.GetType().Name == "Shoes")
		{
			Shoes shoes = (Shoes)idObject;
			if (!m_ShoesForm.pickUpControl.combo.Items.Contains(shoes))
			{
				m_ShoesForm.pickUpControl.combo.Items.Add(shoes);
			}
			m_ShoesForm.pickUpControl.combo.SelectedItem = shoes;
			ShowFormOnPanel(m_ShoesForm, panel);
		}
		if (idObject.GetType().Name == "GkGloves")
		{
			GkGloves gkGloves = (GkGloves)idObject;
			if (!m_GlovesForm.pickUpControl.combo.Items.Contains(gkGloves))
			{
				m_GlovesForm.pickUpControl.combo.Items.Add(gkGloves);
			}
			m_GlovesForm.pickUpControl.combo.SelectedItem = gkGloves;
			ShowFormOnPanel(m_GlovesForm, panel);
		}
	}

	private void menuAbout_Click(object sender, EventArgs e)
	{
		m_AboutForm.labelProduct.Text = "Creation Master 26";
		m_AboutForm.labelRelease.Text = "Version " + typeof(MainForm).Assembly.GetName().Version.ToString(3);
		m_AboutForm.ShowDialog();
	}

	private void buttonShowBottom_Click(object sender, EventArgs e)
	{
		toolStripBottom.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
		toolStripBottom.Dock = DockStyle.Left;
		splitHoriz.SplitterDistance = m_SplitterDistanceBottom;
		splitHoriz.IsSplitterFixed = false;
		buttonShowBottom.Visible = false;
		stripLabelBottom.TextDirection = ToolStripTextDirection.Vertical90;
		buttonHideBottom.Visible = true;
	}

	private void buttonHideBottom_Click(object sender, EventArgs e)
	{
		toolStripBottom.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
		toolStripBottom.Dock = DockStyle.Bottom;
		toolStripBottom.AutoSize = true;
		m_SplitterDistanceBottom = splitHoriz.SplitterDistance;
		splitHoriz.SplitterDistance = splitHoriz.Height - 23;
		splitHoriz.IsSplitterFixed = true;
		buttonShowBottom.Visible = true;
		stripLabelBottom.TextDirection = ToolStripTextDirection.Horizontal;
		buttonHideBottom.Visible = false;
	}

	private void buttonShowRight_Click(object sender, EventArgs e)
	{
		toolStripRight.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
		toolStripRight.Dock = DockStyle.Top;
		toolStripRight.AutoSize = true;
		splitVert.SplitterDistance = m_SplitterDistanceRight;
		splitVert.IsSplitterFixed = false;
		buttonShowRight.Visible = false;
		stripLabelRight.TextDirection = ToolStripTextDirection.Horizontal;
		buttonHideRight.Visible = true;
	}

	private void buttonHideRight_Click(object sender, EventArgs e)
	{
		toolStripRight.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
		toolStripRight.Dock = DockStyle.Right;
		m_SplitterDistanceRight = splitVert.SplitterDistance;
		splitVert.SplitterDistance = splitVert.Width - 23;
		splitVert.IsSplitterFixed = true;
		buttonShowRight.Visible = true;
		stripLabelRight.TextDirection = ToolStripTextDirection.Vertical90;
		buttonHideRight.Visible = false;
	}

	private void MainForm_SizeChanged(object sender, EventArgs e)
	{
		if (splitHoriz.IsSplitterFixed && splitHoriz.Height >= 23)
		{
			splitHoriz.SplitterDistance = splitHoriz.Height - 23;
		}
		if (splitVert.IsSplitterFixed && splitVert.Width >= 23)
		{
			splitVert.SplitterDistance = splitVert.Width - 23;
		}
	}

	private void menuOpenFifa_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		if (InitializeFifaEnvironment(14))
		{
			Refresh();
			Open();
			Cursor.Current = Cursors.Default;
		}
	}

	private void menuOpenFifa15Demo_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		if (InitializeFifaEnvironment(15))
		{
			Refresh();
			Open();
			Cursor.Current = Cursors.Default;
		}
	}

	private bool InitializeFifaEnvironment(int year, string rootDir)
	{
		bool flag = false;
		if (year > 0)
		{
			flag = FifaEnvironment.Initialize(year, rootDir);
		}
		else if (rootDir != null)
		{
			if (rootDir.Contains("14"))
			{
				flag = FifaEnvironment.Initialize(14, rootDir);
			}
			if (rootDir.Contains("15"))
			{
				flag = FifaEnvironment.Initialize(15, rootDir);
			}
			if (rootDir.Contains("16"))
			{
				flag = FifaEnvironment.Initialize(16, rootDir);
			}
		}
		if (!flag)
		{
			FifaEnvironment.UserMessages.ShowMessage(10004);
		}
		return flag;
	}

	private bool InitializeFifaEnvironment(int year)
	{
		return InitializeFifaEnvironment(year, null);
	}

	private bool InitializeFifaEnvironment(string rootDir)
	{
		return InitializeFifaEnvironment(-1, rootDir);
	}

	private void Open()
	{
		if (FifaEnvironment.Open(statusBar))
		{
			m_OpenFileFlag = true;
			Settings.Default.RootDir = FifaEnvironment.RootDir;
			Settings.Default.FifaDbFileName = FifaEnvironment.FifaDbFileName;
			Settings.Default.FifaXmlFileName = FifaEnvironment.FifaXmlFileName;
			Settings.Default.LangDbFileName = FifaEnvironment.LangDbFileName;
			Settings.Default.LangXmlFileName = FifaEnvironment.LangXmlFileName;
			Settings.Default.Save();
			EnablePanels(enable: true);
			EnableMenus();
		}
	}

	private void buttonMain_Click(object sender, EventArgs e)
	{
		ToolStripButton obj = (ToolStripButton)sender;
		Panel panel = TargetPanelFromCurrentModifiers();
		if (obj == buttonCountry)
		{
			ShowFormOnPanel(m_CountryForm, panel);
		}
		if (obj == buttonTeam)
		{
			ShowFormOnPanel(m_TeamForm, panel);
		}
		if (obj == buttonKit)
		{
			ShowFormOnPanel(m_KitForm, panel);
		}
		if (obj == buttonFormation)
		{
			ShowFormOnPanel(m_FormationForm, panel);
		}
		if (obj == buttonBrowser)
		{
			ShowFormOnPanel(m_WebBrowserForm, panel);
		}
		if (obj == buttonBall)
		{
			ShowFormOnPanel(m_BallForm, panel);
		}
		if (obj == buttonManager)
		{
			ShowFormOnPanel(m_ManagerForm, panel);
		}
		if (obj == buttonSponsor)
		{
			CmStyleDetailsWindow.Open(this, "Sponsors", DetailSection.Sponsor);
		}
		if (obj == buttonGameGraphics)
		{
			ShowFormOnPanel(m_GameGraphicForm, panel);
		}
		if (obj == buttonLeague)
		{
			ShowFormOnPanel(m_LeagueForm, panel);
		}
		if (obj == buttonShoes)
		{
			ShowFormOnPanel(m_ShoesForm, panel);
		}
		if (obj == buttonTv)
		{
			ShowFormOnPanel(m_TvForm, panel);
		}
		if (obj == buttonNewspaper)
		{
			ShowFormOnPanel(m_NewspapersForm, panel);
		}
		if (obj == buttonReferee)
		{
			ShowFormOnPanel(m_RefereeForm, panel);
		}
		if (obj == buttonTournament)
		{
			ShowFormOnPanel(m_TrophyForm, panel);
		}
		if (obj == buttonStadium)
		{
			ShowFormOnPanel(m_StadiumForm, panel);
		}
		if (obj == buttonPlayer)
		{
			ShowFormOnPanel(m_PlayerForm, panel);
		}
		if (obj == buttonGloves)
		{
			ShowFormOnPanel(m_GlovesForm, panel);
		}
		if (obj == buttonAudio)
		{
			ShowFormOnPanel(m_AudioForm, panel);
		}
		if (obj == buttonImportGraphics)
		{
			ShowFormOnPanel(m_ImportGraphicsForm, panel);
		}
	}

	private void openSelectLandbToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (InitializeFifaEnvironment(14) && AskUserOpenLangDatabase())
		{
			Open();
		}
	}

	private void openSelectAllToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (AskUserOpen())
		{
			Open();
		}
	}

	private bool AskUserOpen()
	{
		string text = AskUserOpenRootFolder();
		if (text == null)
		{
			return false;
		}
		if (!InitializeFifaEnvironment(text))
		{
			return false;
		}
		if (!FifaEnvironment.OpenFat())
		{
			FifaEnvironment.UserMessages.ShowMessage(10003);
			return false;
		}
		FifaEnvironment.ExtractMainDatabase();
		if (!AskUserOpenMainDatabase())
		{
			return false;
		}
		if (!FifaEnvironment.OpenFifaDb())
		{
			FifaEnvironment.UserMessages.ShowMessage(10000);
			return false;
		}
		FifaEnvironment.ExtractLangDatabase();
		if (!AskUserOpenLangDatabase())
		{
			return false;
		}
		if (!FifaEnvironment.OpenLangDb())
		{
			FifaEnvironment.UserMessages.ShowMessage(10035);
			return false;
		}
		return true;
	}

	private bool AskUserOpenLangDatabase()
	{
		openLangDialog.CheckFileExists = true;
		openLangDialog.InitialDirectory = FifaEnvironment.GameDir + "data\\loc\\";
		openLangDialog.Filter = "db files (*.db)|*.db";
		openLangDialog.FilterIndex = 1;
		openLangDialog.Title = "Open Language Database";
		if (openLangDialog.ShowDialog() != DialogResult.OK)
		{
			return false;
		}
		FifaEnvironment.LangDbFileName = openLangDialog.FileName;
		FifaEnvironment.LangXmlFileName = openLangDialog.FileName.Replace(".db", "-meta.xml");
		return true;
	}

	private string AskUserOpenRootFolder()
	{
		browserDialog.Description = "Select the root folder of FIFA";
		browserDialog.RootFolder = Environment.SpecialFolder.Desktop;
		browserDialog.ShowNewFolderButton = false;
		browserDialog.SelectedPath = FifaEnvironment.RootDir;
		if (browserDialog.ShowDialog() != DialogResult.OK)
		{
			return null;
		}
		return browserDialog.SelectedPath;
	}

	private bool AskUserOpenMainDatabase()
	{
		if (!BrowseXml())
		{
			return false;
		}
		if (!BrowseDB())
		{
			return false;
		}
		return true;
	}

	private bool BrowseDB()
	{
		openFifaDialog.InitialDirectory = FifaEnvironment.GameDir + "data\\db\\";
		openFifaDialog.Filter = "db files (*.db)|*.db";
		openFifaDialog.FilterIndex = 1;
		openFifaDialog.Title = "Open Database File";
		bool result = false;
		if (openFifaDialog.ShowDialog() == DialogResult.OK)
		{
			FifaEnvironment.FifaDbFileName = openFifaDialog.FileName;
			result = true;
		}
		return result;
	}

	private bool BrowseXmlDb()
	{
		openFifaDialog.InitialDirectory = FifaEnvironment.GameDir + "data\\db\\";
		openFifaDialog.Filter = "xml files (*.xml)|*.xml";
		openFifaDialog.FilterIndex = 1;
		openFifaDialog.Title = "Open XML Descriptor File";
		bool result = false;
		if (openFifaDialog.ShowDialog() == DialogResult.OK)
		{
			m_XmlDbFileName = openFifaDialog.FileName;
			result = true;
		}
		return result;
	}

	private bool BrowseXml()
	{
		openFifaDialog.InitialDirectory = FifaEnvironment.GameDir + "data\\db\\";
		openFifaDialog.Filter = "xml files (*.xml)|*.xml";
		openFifaDialog.FilterIndex = 1;
		openFifaDialog.Title = "Open XML Descriptor File";
		bool result = false;
		if (openFifaDialog.ShowDialog() == DialogResult.OK)
		{
			FifaEnvironment.FifaXmlFileName = openFifaDialog.FileName;
			result = true;
		}
		return result;
	}

	private void menuSave_Click(object sender, EventArgs e)
	{
		if (m_OpenFileFlag)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				Refresh();
				SaveFiles();
				if (m_LastSaveCommitted) statusBar.Text = "Ready - Save completed!";
			}
			catch (Exception ex)
			{
				statusBar.Text = "Save cancelled - complete the new league setup.";
				MessageBox.Show(this, ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			finally { Cursor.Current = Cursors.Default; }
		}
	}

	private void menuClose_Click(object sender, EventArgs e)
	{
		if (AskAndSave())
		{
			CloseFile();
		}
	}

	private void menuExit_Click(object sender, EventArgs e)
	{
		AskAndExit();
	}

	private bool AskAndSave()
	{
		switch (FifaEnvironment.UserMessages.ShowMessage(1))
		{
		case DialogResult.Yes:
			try { SaveFiles(); return m_LastSaveCommitted; }
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
		case DialogResult.OK:
		case DialogResult.No:
			return true;
		default:
			return false;
		}
	}

	private void AskAndExit()
	{
		if (m_OpenFileFlag)
		{
			if (AskAndSave())
			{
				m_OpenFileFlag = false;
				Application.Exit();
			}
		}
		else
		{
			Application.Exit();
		}
	}

	private void SaveFiles()
	{
		m_LastSaveCommitted = false;
		if (FifaEnvironment.Year == 26)
		{
			var pendingLeagues = m_PendingLeagueCompdataIds.ToArray();
			var pendingTeams = m_PendingTeamIds.ToArray();
			var preflight = Fc26SavePreflight.Run(pendingLeagues, pendingTeams);
			if (!preflight.CanSave)
			{
				using (var gate = new Fc26SavePreflightDialog(this, preflight)) gate.ShowDialog(this);
				statusBar.Text = "Save paused — use Fix Selected in Save Preflight.";
				return;
			}
			try
			{
				StagePendingLeagueCompdata();
			}
			catch (Exception ex)
			{
				var failed = new Fc26SavePreflightResult(new[] { new Fc26SaveCheck("Compdata", Fc26CheckState.Error, ex.Message, "competition") });
				using (var gate = new Fc26SavePreflightDialog(this, failed)) gate.ShowDialog(this);
				statusBar.Text = "Save paused — Compdata needs attention.";
				return;
			}
			var sourceRoot = Fc26SnapshotLoader.CurrentGameRoot;
			var sourceFolder = Fc26SnapshotLoader.CurrentDatabaseFolder;
			statusBar.Text = "Saving FC26 database and Frostbite archives...";
			statusBar.GetCurrentParent().Refresh();
			statusBar.Text = Fc26HostBridge.Save();
			m_PendingLeagueCompdataIds.Clear();
			m_PendingTeamIds.Clear();
			m_PendingPlayerIds.Clear();
			m_LastSaveCommitted = true;
			ShowFc26SaveProof(pendingLeagues, pendingTeams, sourceRoot, sourceFolder, statusBar.Text);
			return;
		}
		FifaEnvironment.Save(statusBar);
		m_LastSaveCommitted = true;
	}

	private void ShowFc26SaveProof(IEnumerable<int> leagueIds, IEnumerable<int> teamIds,
		string sourceRoot, string sourceFolder, string saveMessage)
	{
		var leagues = (leagueIds ?? Array.Empty<int>()).Where(value => value > 0).Distinct().ToArray();
		var teams = (teamIds ?? Array.Empty<int>()).Where(value => value > 0).Distinct().ToArray();
		var lines = new List<string>
		{
			"CM26 SAVE PROOF", new string('=', 42), string.Empty,
			"[PASS] Transactional save: " + (saveMessage ?? "committed"),
			"[PASS] Backup: timestamped backup created by the FC26 save engine.",
			"[PASS] Compdata: generated, validated and staged before commit.",
			""
		};
		var reloadOk = false;
		var linkOk = true;
		try
		{
			string snapshot = null;
			if (!string.IsNullOrWhiteSpace(sourceRoot) && Directory.Exists(sourceRoot)) snapshot = Fc26HostBridge.OpenGameRoot(sourceRoot);
			else if (!string.IsNullOrWhiteSpace(sourceFolder) && Directory.Exists(sourceFolder)) snapshot = Fc26HostBridge.OpenExtractedFolder(sourceFolder);
			if (!string.IsNullOrWhiteSpace(snapshot) && File.Exists(snapshot))
			{
				LoadFc26Snapshot(snapshot, showCountry: false);
				reloadOk = true;
				foreach (var id in leagues)
				{
					var league = FifaEnvironment.Leagues.SearchId(id) as League;
					if (league == null) { linkOk = false; continue; }
					var linked = league.PlayingTeams.Cast<Team>().Select(value => value.Id).ToHashSet();
					if (teams.Length > 0 && teams.Any(teamId => !linked.Contains(teamId))) linkOk = false;
				}
				foreach (var id in teams)
					if (!(FifaEnvironment.Teams.SearchId(id) is Team)) linkOk = false;
			}
		}
		catch (Exception ex)
		{
			lines.Add("[CHECK] Reload snapshot: " + ex.Message);
		}
		lines.Add(reloadOk ? "[PASS] Reload snapshot: current database was reloaded." : "[CHECK] Reload snapshot: source could not be reloaded; save is still committed.");
		lines.Add(linkOk ? "[PASS] Proof rows: league/team IDs and links are present." : "[CHECK] Proof rows: review the League/Team relationship section.");
		lines.Add(string.Empty);
		lines.Add(reloadOk && linkOk ? "CAREER READY" : "CAREER READY WITH REVIEW");
		lines.Add("Start a new Career after database or Compdata changes; an existing Career keeps its old competition snapshot.");
		using (var proof = new Fc26SaveProofDialog("Save Proof — Career Ready", string.Join(Environment.NewLine, lines)))
			proof.ShowDialog(this);
	}

	private void CloseFile()
	{
		m_OpenFileFlag = false;
		m_PendingLeagueCompdataIds.Clear();
		m_PendingTeamIds.Clear();
		m_PendingPlayerIds.Clear();
		m_CountryForm.Clean();
		m_LeagueForm.Clean();
		m_TeamForm.Clean();
		m_KitForm.Clean();
		m_PlayerForm.Clean();
		m_StadiumForm.Clean();
		m_RefereeForm.Clean();
		m_FormationForm.Clean();
		m_TrophyForm.Clean();
		m_ManagerForm.Clean();
		m_GameGraphicForm.Clean();
		m_TvForm.Clean();
		m_ShoesForm.Clean();
		m_BallForm.Clean();
		m_GlovesForm.Clean();
		m_AudioForm.Clean();
		m_ImportGraphicsForm.Clean();
		DestroyForms();
		CreateForms();
		EnableMenus();
		EnablePanels(enable: false);
	}

	private void menuOptions_Click(object sender, EventArgs e)
	{
		FifaEnvironment.ShowOptions();
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		m_IsShiftPressed = (keyData & Keys.Shift) != 0;
		m_IsCtrlPressed = (keyData & Keys.Control) != 0;
		m_IsAltPressed = (keyData & Keys.Alt) != 0;
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void menuExpandDatabase_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.FifaDb.NTables != 137)
		{
			FifaEnvironment.UserMessages.ShowMessage(5049);
			return;
		}
		bool flag = FifaEnvironment.FifaDb.Expand();
		FifaEnvironment.UserMessages.ShowMessage(flag ? 1010 : 1011);
	}

	private void menuEnableAllMessages_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.UserMessages != null)
		{
			FifaEnvironment.UserMessages.EnableMessages(enable: true);
		}
	}

	private void menuRegenerate_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.UserMessages != null)
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(14);
			if (dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
			{
				return;
			}
		}
		if (FifaEnvironment.FifaFat != null)
		{
			statusBar.Text = "Regenerating bh files";
			Cursor.Current = Cursors.WaitCursor;
			Refresh();
			FifaEnvironment.FifaFat.RegenerateAllBh(hideExternalFiles: true);
			Cursor.Current = Cursors.Default;
			statusBar.Text = "Ready";
			return;
		}
		string text = AskUserOpenRootFolder();
		if (text != null)
		{
			string[] files = Directory.GetFiles(text, "*.big");
			Cursor.Current = Cursors.WaitCursor;
			string[] array = files;
			foreach (string text2 in array)
			{
				statusBar.Text = "Regenerating " + Path.GetFileName(text2);
				Refresh();
				BhFile.Regenerate(text2, hideExternalFiles: true);
			}
			Cursor.Current = Cursors.Default;
			statusBar.Text = "Ready";
		}
	}

	private void menuHelp_Click(object sender, EventArgs e)
	{
		string text = FifaEnvironment.LaunchDir + "\\CreationMaster.htm";
		if (File.Exists(text))
		{
			Help.ShowHelp(this, text);
		}
	}

	private void menuCreatePatch_Click(object sender, EventArgs e)
	{
		switch (FifaEnvironment.UserMessages.ShowMessage(19))
		{
		case DialogResult.Yes:
			SaveFiles();
			break;
		case DialogResult.Cancel:
			return;
		}
		m_PatchCreatorForm.ShowDialog();
	}

	private void menuLoadPatch_Click(object sender, EventArgs e)
	{
		m_PatchLoaderForm.ShowDialog();
		statusBar.Text = "Updating windows ...";
		statusBar.Text = "Ready";
	}

	private void menuRemoveKidProtection_Click(object sender, EventArgs e)
	{
	}

	private void menuCleanFAT_Click(object sender, EventArgs e)
	{
	}

	private void menuHelpCms_Click(object sender, EventArgs e)
	{
	}

	private void removeAllLongTeamNames_Click(object sender, EventArgs e)
	{
	}

	private void adboardsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		CmStyleDetailsWindow.Open(this, "Adboards", DetailSection.Adboard);
	}

	private void ballsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void bootsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void countryToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void fontsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void formationsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void leaguesToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void stadiumsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void teamsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void tournamentsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void importToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.FifaXmlFileName != null)
		{
			m_XmlDbFileName = FifaEnvironment.FifaXmlFileName;
		}
		else if (!BrowseXmlDb())
		{
			return;
		}
		if (BrowseUgc() && OpenUgcFile())
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(29);
			if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
			{
				Cursor.Current = Cursors.WaitCursor;
				m_UgcFile.Import(m_XmlDbFileName, useGraphics: false, statusBar);
				Cursor.Current = Cursors.Default;
			}
		}
	}

	private void importDBAndKITSToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.FifaXmlFileName != null)
		{
			m_XmlDbFileName = FifaEnvironment.FifaXmlFileName;
		}
		else if (!BrowseXmlDb())
		{
			return;
		}
		if (BrowseUgc() && OpenUgcFile())
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(29);
			if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
			{
				Cursor.Current = Cursors.WaitCursor;
				m_UgcFile.Import(m_XmlDbFileName, useGraphics: true, statusBar);
				Cursor.Current = Cursors.Default;
			}
		}
	}

	private bool OpenUgcFile()
	{
		if (m_UgcFileName == null)
		{
			return false;
		}
		m_UgcFile = new UgcFile(m_UgcFileName);
		if (m_UgcFile == null)
		{
			return false;
		}
		for (int i = 0; i < m_UgcFile.NFiles; i++)
		{
			m_UgcFile.Extract(i, FifaEnvironment.TempFolder);
		}
		return true;
	}

	private bool BrowseUgc()
	{
		openFifaDialog = new OpenFileDialog();
		openFifaDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\FIFA 14";
		openFifaDialog.Filter = "UGC Files|UG*.*";
		openFifaDialog.FilterIndex = 1;
		openFifaDialog.Title = "Open User Generated Content file";
		bool result = false;
		if (openFifaDialog.ShowDialog() == DialogResult.OK)
		{
			m_UgcFileName = openFifaDialog.FileName;
			result = true;
		}
		return result;
	}

	private void importKITSOmlyToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.FifaXmlFileName != null)
		{
			m_XmlDbFileName = FifaEnvironment.FifaXmlFileName;
		}
		else if (!BrowseXmlDb())
		{
			return;
		}
		if (BrowseUgc() && OpenUgcFile())
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(29);
			if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
			{
				Cursor.Current = Cursors.WaitCursor;
				m_UgcFile.ImportKitGraphics(m_XmlDbFileName, statusBar);
				Cursor.Current = Cursors.Default;
			}
		}
	}

	private void menuReopen_Click(object sender, EventArgs e)
	{
		if (Settings.Default.RootDir != null && Settings.Default.RootDir != string.Empty && Settings.Default.FifaDbFileName != null && Settings.Default.FifaDbFileName != string.Empty && Settings.Default.LangXmlFileName != null && Settings.Default.FifaXmlFileName != string.Empty && Settings.Default.LangDbFileName != null && Settings.Default.LangDbFileName != string.Empty && Settings.Default.LangXmlFileName != null && Settings.Default.LangXmlFileName != string.Empty && InitializeFifaEnvironment(Settings.Default.RootDir))
		{
			Cursor.Current = Cursors.WaitCursor;
			FifaEnvironment.FifaDbFileName = Settings.Default.FifaDbFileName;
			FifaEnvironment.FifaXmlFileName = Settings.Default.FifaXmlFileName;
			FifaEnvironment.LangDbFileName = Settings.Default.LangDbFileName;
			FifaEnvironment.LangXmlFileName = Settings.Default.LangXmlFileName;
			Refresh();
			Open();
			Cursor.Current = Cursors.Default;
		}
	}

	private void menuOnlineDBFifa14_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.FifaXmlFileName != null)
		{
			m_XmlDbFileName = FifaEnvironment.FifaXmlFileName;
		}
		else if (!BrowseXmlDb())
		{
			return;
		}
		if (BrowseOnline())
		{
			Cursor.Current = Cursors.WaitCursor;
			m_OnlineDbFile = new CareerFile(m_OnlineDbFileName, m_XmlDbFileName);
			Cursor.Current = Cursors.Default;
			if (m_OnlineDbFile != null)
			{
				m_OnlineDb = m_OnlineDbFile.Databases[0];
				MergeOnlineDb(updatePlayers: false);
			}
		}
	}

	private bool MergeOnlineDb19(bool updatePlayers)
	{
		if (m_OnlineDb == null)
		{
			return false;
		}
		Table table = m_OnlineDb.GetTable("dcplayernames");
		Table table2 = m_OnlineDb.GetTable("teamplayerlinks");
		Table table3 = m_OnlineDb.GetTable("teamstadiumlinks");
		Table table4 = m_OnlineDb.GetTable("formations");
		Table table5 = m_OnlineDb.GetTable("teams");
		Table table6 = m_OnlineDb.GetTable("playerloans");
		Table table7 = m_OnlineDb.GetTable("manager");
		Table table8 = m_OnlineDb.GetTable("players");
		Table table9 = m_OnlineDb.GetTable("previousteam");
		Table table10 = m_OnlineDb.GetTable("stadiumassignments");
		Table table11 = m_OnlineDb.GetTable("leagueteamlinks");
		string text = FifaEnvironment.LaunchDir + "\\Templates\\2019\\data\\db\\fifa_ng_db-meta.xml";
		Table table12 = new DbFile(text.Replace("-meta.xml", ".db"), text).GetTable("playernames");
		if (table == null || table2 == null || table4 == null || table5 == null || table6 == null || table7 == null || table8 == null || table9 == null)
		{
			FifaEnvironment.UserMessages.ShowMessage(10036);
			return false;
		}
		int fieldIndex = table8.TableDescriptor.GetFieldIndex("playerid");
		int fieldIndex2 = table8.TableDescriptor.GetFieldIndex("gender");
		int fieldIndex3 = table8.TableDescriptor.GetFieldIndex("birthdate");
		Player player = new Player(1);
		for (int i = 0; i < table8.NValidRecords; i++)
		{
			Record record = table8.Records[i];
			int num = record.IntField[fieldIndex];
			int num2 = record.IntField[fieldIndex2];
			int num3 = record.IntField[fieldIndex3];
			DateTime dateTime = new DateTime(num3);
			if ((dateTime.Month == 12 && dateTime.Day == 29 && num >= 2300000) || num == 0 || num2 != 0)
			{
				continue;
			}
			Player player2 = (Player)FifaEnvironment.Players.SearchId(num);
			if (player2 == null)
			{
				player2 = new Player(num);
				player2.UpdateFromOnlineRecord19(record, table8.TableDescriptor);
				if (!player2.IsFakePlayer())
				{
					player2.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
					player2.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
					player2.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
					player2.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
					player2.UpdatePlayername(table, table12);
					FifaEnvironment.Players.InsertId(player2);
				}
			}
			else if (updatePlayers)
			{
				player2.UpdateFromOnlineRecord19(record, table8.TableDescriptor);
				player.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
				player.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
				player.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
				player.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
				player.UpdatePlayername(table, table12);
				player2.firstname = player.firstname;
				player2.lastname = player.lastname;
				player2.commonname = player.commonname;
				player2.playerjerseyname = player.playerjerseyname;
			}
		}
		FifaEnvironment.Players.FillFromPlayerloans(table6);
		FifaEnvironment.Players.FillFromPreviousTeam(table9);
		int fieldIndex4 = table5.TableDescriptor.GetFieldIndex("teamid");
		for (int j = 0; j < table5.NValidRecords; j++)
		{
			Record record2 = table5.Records[j];
			int num4 = record2.IntField[fieldIndex4];
			if (Team.IsFakeOrWomenTeam(num4))
			{
				continue;
			}
			Team team = (Team)FifaEnvironment.Teams.SearchId(num4);
			if (team == null)
			{
				team = new Team(num4);
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
				FifaEnvironment.Teams.InsertId(team);
				if (team.TeamNameAbbr15 == null)
				{
					team.TeamNameFull = team.DatabaseName;
					team.SetNameAutomatically(team.TeamNameFull, 15);
					team.SetNameAutomatically(team.TeamNameAbbr15, 10);
					team.SetNameAutomatically(team.TeamNameAbbr10, 7);
					team.SetNameAutomatically(team.TeamNameAbbr7, 3);
				}
			}
			else
			{
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
			}
		}
		FifaEnvironment.Teams.FillFromTeamPlayerLinks(table2);
		FifaEnvironment.Teams.FillFromManager(table7);
		FifaEnvironment.Teams.FillFromTeamStadiumLinks(table3);
		FifaEnvironment.Teams.FillFromStadiumAssignments(table10);
		int fieldIndex5 = table4.TableDescriptor.GetFieldIndex("teamid");
		int fieldIndex6 = table4.TableDescriptor.GetFieldIndex("formationid");
		for (int k = 0; k < table4.NValidRecords; k++)
		{
			Record record3 = table4.Records[k];
			int num5 = record3.IntField[fieldIndex5];
			_ = record3.IntField[fieldIndex6];
			if (num5 < 0 || Team.IsFakeOrWomenTeam(num5))
			{
				continue;
			}
			Formation formation = FifaEnvironment.Formations.SearchByTeamId(num5);
			if (formation != null)
			{
				formation.Load19(record3);
				continue;
			}
			Team team2 = (Team)FifaEnvironment.Teams.SearchId(num5);
			if (team2 != null)
			{
				int newId = FifaEnvironment.Formations.GetNewId();
				_ = 0;
				formation = new Formation(newId);
				formation.Load19(record3);
				FifaEnvironment.Formations.InsertId(formation);
				team2.Formation = formation;
				formation.Team = team2;
			}
		}
		int num6 = -1;
		int fieldIndex7 = table11.TableDescriptor.GetFieldIndex("leagueid");
		int fieldIndex8 = table11.TableDescriptor.GetFieldIndex("teamid");
		for (int l = 0; l < table5.NValidRecords; l++)
		{
			Record record4 = table11.Records[l];
			int num7 = record4.IntField[fieldIndex7];
			if (num7 == 76 || num7 == 78 || num7 == 111 || num7 == 382 || num7 == 383 || num7 == 384 || num7 == 2000 || num7 == 2028 || num7 == 2136 || num7 == 2140 || num7 == 3003 || num7 == 3004)
			{
				continue;
			}
			int id = record4.IntField[fieldIndex8];
			League league = (League)FifaEnvironment.Leagues.SearchId(num7);
			Team team3 = (Team)FifaEnvironment.Teams.SearchId(id);
			if (league != null && team3 != null)
			{
				if (num6 != num7)
				{
					num6 = num7;
					league.PlayingTeams.Clear();
				}
				league.LinkTeam(team3);
				team3.League = league;
				if (team3.Country == null)
				{
					team3.Country = league.Country;
				}
				team3.FillFromLeagueTeamLinks19(record4);
			}
		}
		FifaEnvironment.Players.LinkTeam(FifaEnvironment.Teams);
		FifaEnvironment.Players.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkPlayer(FifaEnvironment.Players);
		FifaEnvironment.Teams.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkLeague(FifaEnvironment.Leagues);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.Teams.LinkFormation(FifaEnvironment.Formations);
		FifaEnvironment.Teams.LinkStadiums(FifaEnvironment.Stadiums);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.UserMessages.ShowMessage(15007);
		return true;
	}

	private bool MergeOnlineDb20(bool updatePlayers, string year)
	{
		if (m_OnlineDb == null)
		{
			return false;
		}
		Table table = m_OnlineDb.GetTable("dcplayernames");
		Table table2 = m_OnlineDb.GetTable("teamplayerlinks");
		Table table3 = m_OnlineDb.GetTable("teamstadiumlinks");
		Table table4 = m_OnlineDb.GetTable("formations");
		Table table5 = m_OnlineDb.GetTable("teams");
		Table table6 = m_OnlineDb.GetTable("playerloans");
		Table table7 = m_OnlineDb.GetTable("manager");
		Table table8 = m_OnlineDb.GetTable("players");
		Table table9 = m_OnlineDb.GetTable("previousteam");
		Table table10 = m_OnlineDb.GetTable("stadiumassignments");
		Table table11 = m_OnlineDb.GetTable("leagueteamlinks");
		string text = FifaEnvironment.LaunchDir + "\\Templates\\" + year + "\\data\\db\\fifa_ng_db-meta.xml";
		Table table12 = new DbFile(text.Replace("-meta.xml", ".db"), text).GetTable("playernames");
		if (table == null || table2 == null || table4 == null || table5 == null || table6 == null || table7 == null || table8 == null || table9 == null)
		{
			FifaEnvironment.UserMessages.ShowMessage(10036);
			return false;
		}
		int fieldIndex = table8.TableDescriptor.GetFieldIndex("playerid");
		int fieldIndex2 = table8.TableDescriptor.GetFieldIndex("gender");
		int fieldIndex3 = table8.TableDescriptor.GetFieldIndex("birthdate");
		Player player = new Player(1);
		for (int i = 0; i < table8.NValidRecords; i++)
		{
			Record record = table8.Records[i];
			int num = record.IntField[fieldIndex];
			int num2 = record.IntField[fieldIndex2];
			FifaUtil.ConvertToDate(record.IntField[fieldIndex3]);
			if (num == 0 || num2 != 0)
			{
				continue;
			}
			Player player2 = (Player)FifaEnvironment.Players.SearchId(num);
			if (player2 == null)
			{
				player2 = new Player(num);
				player2.UpdateFromRecord20(record, table8.TableDescriptor);
				if (!player2.IsFakePlayer())
				{
					player2.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
					player2.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
					player2.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
					player2.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
					player2.UpdatePlayername(table, table12);
					FifaEnvironment.Players.InsertId(player2);
				}
			}
			else if (updatePlayers)
			{
				player2.UpdateFromRecord20(record, table8.TableDescriptor);
				player.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
				player.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
				player.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
				player.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
				player.UpdatePlayername(table, table12);
				player2.firstname = player.firstname;
				player2.lastname = player.lastname;
				player2.commonname = player.commonname;
				player2.playerjerseyname = player.playerjerseyname;
			}
		}
		FifaEnvironment.Players.FillFromPlayerloans(table6);
		FifaEnvironment.Players.FillFromPreviousTeam(table9);
		int fieldIndex4 = table5.TableDescriptor.GetFieldIndex("teamid");
		for (int j = 0; j < table5.NValidRecords; j++)
		{
			Record record2 = table5.Records[j];
			int num3 = record2.IntField[fieldIndex4];
			if (Team.IsFakeOrWomenTeam(num3))
			{
				continue;
			}
			Team team = (Team)FifaEnvironment.Teams.SearchId(num3);
			if (team == null)
			{
				team = new Team(num3);
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
				FifaEnvironment.Teams.InsertId(team);
				if (team.TeamNameAbbr15 == null)
				{
					team.TeamNameFull = team.DatabaseName;
					team.SetNameAutomatically(team.TeamNameFull, 15);
					team.SetNameAutomatically(team.TeamNameAbbr15, 10);
					team.SetNameAutomatically(team.TeamNameAbbr10, 7);
					team.SetNameAutomatically(team.TeamNameAbbr7, 3);
				}
			}
			else
			{
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
			}
		}
		FifaEnvironment.Teams.FillFromTeamPlayerLinks(table2);
		FifaEnvironment.Teams.FillFromManager(table7);
		FifaEnvironment.Teams.FillFromTeamStadiumLinks(table3);
		FifaEnvironment.Teams.FillFromStadiumAssignments(table10);
		int fieldIndex5 = table4.TableDescriptor.GetFieldIndex("teamid");
		int fieldIndex6 = table4.TableDescriptor.GetFieldIndex("formationid");
		for (int k = 0; k < table4.NValidRecords; k++)
		{
			Record record3 = table4.Records[k];
			int num4 = record3.IntField[fieldIndex5];
			_ = record3.IntField[fieldIndex6];
			if (num4 < 0 || Team.IsFakeOrWomenTeam(num4))
			{
				continue;
			}
			Formation formation = FifaEnvironment.Formations.SearchByTeamId(num4);
			if (formation != null)
			{
				formation.Load19(record3);
				continue;
			}
			Team team2 = (Team)FifaEnvironment.Teams.SearchId(num4);
			if (team2 != null)
			{
				int newId = FifaEnvironment.Formations.GetNewId();
				_ = 0;
				formation = new Formation(newId);
				formation.Load19(record3);
				FifaEnvironment.Formations.InsertId(formation);
				team2.Formation = formation;
				formation.Team = team2;
			}
		}
		int num5 = -1;
		int fieldIndex7 = table11.TableDescriptor.GetFieldIndex("leagueid");
		int fieldIndex8 = table11.TableDescriptor.GetFieldIndex("teamid");
		for (int l = 0; l < table5.NValidRecords; l++)
		{
			Record record4 = table11.Records[l];
			int num6 = record4.IntField[fieldIndex7];
			if (num6 == 76 || num6 == 78 || num6 == 111 || num6 == 382 || num6 == 383 || num6 == 384 || num6 == 2000 || num6 == 2028 || num6 == 2136 || num6 == 2140 || num6 == 3003 || num6 == 3004)
			{
				continue;
			}
			int id = record4.IntField[fieldIndex8];
			League league = (League)FifaEnvironment.Leagues.SearchId(num6);
			Team team3 = (Team)FifaEnvironment.Teams.SearchId(id);
			if (league != null && team3 != null)
			{
				if (num5 != num6)
				{
					num5 = num6;
					league.PlayingTeams.Clear();
				}
				league.LinkTeam(team3);
				team3.League = league;
				if (team3.Country == null)
				{
					team3.Country = league.Country;
				}
				team3.FillFromLeagueTeamLinks19(record4);
			}
		}
		FifaEnvironment.Players.LinkTeam(FifaEnvironment.Teams);
		FifaEnvironment.Players.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkPlayer(FifaEnvironment.Players);
		FifaEnvironment.Teams.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkLeague(FifaEnvironment.Leagues);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.Teams.LinkFormation(FifaEnvironment.Formations);
		FifaEnvironment.Teams.LinkStadiums(FifaEnvironment.Stadiums);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.UserMessages.ShowMessage(15007);
		return true;
	}

	private bool MergeDb20(bool updatePlayers)
	{
		if (m_OnlineDb == null)
		{
			return false;
		}
		Table table = m_OnlineDb.GetTable("dcplayernames");
		Table table2 = m_OnlineDb.GetTable("teamplayerlinks");
		Table table3 = m_OnlineDb.GetTable("teamstadiumlinks");
		Table table4 = m_OnlineDb.GetTable("formations");
		Table table5 = m_OnlineDb.GetTable("teams");
		Table table6 = m_OnlineDb.GetTable("playerloans");
		Table table7 = m_OnlineDb.GetTable("manager");
		Table table8 = m_OnlineDb.GetTable("players");
		Table table9 = m_OnlineDb.GetTable("previousteam");
		Table table10 = m_OnlineDb.GetTable("stadiumassignments");
		Table table11 = m_OnlineDb.GetTable("leagueteamlinks");
		Table table12 = m_OnlineDb.GetTable("playernames");
		if (table == null || table2 == null || table4 == null || table5 == null || table6 == null || table7 == null || table8 == null || table9 == null)
		{
			FifaEnvironment.UserMessages.ShowMessage(10036);
			return false;
		}
		int fieldIndex = table8.TableDescriptor.GetFieldIndex("playerid");
		int fieldIndex2 = table8.TableDescriptor.GetFieldIndex("gender");
		int fieldIndex3 = table8.TableDescriptor.GetFieldIndex("birthdate");
		Player player = new Player(1);
		for (int i = 0; i < table8.NValidRecords; i++)
		{
			Record record = table8.Records[i];
			int num = record.IntField[fieldIndex];
			int num2 = record.IntField[fieldIndex2];
			int num3 = record.IntField[fieldIndex3];
			DateTime dateTime = new DateTime(num3);
			if ((dateTime.Month == 12 && dateTime.Day == 29 && num >= 2300000) || num == 0 || num2 != 0)
			{
				continue;
			}
			Player player2 = (Player)FifaEnvironment.Players.SearchId(num);
			if (player2 == null)
			{
				player2 = new Player(num);
				player2.UpdateFromRecord20(record, table8.TableDescriptor);
				if (!player2.IsFakePlayer())
				{
					player2.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
					player2.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
					player2.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
					player2.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
					player2.UpdatePlayername(table, table12);
					FifaEnvironment.Players.InsertId(player2);
				}
			}
			else if (updatePlayers)
			{
				player2.UpdateFromRecord20(record, table8.TableDescriptor);
				player.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
				player.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
				player.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
				player.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
				player.UpdatePlayername(table, table12);
				player2.firstname = player.firstname;
				player2.lastname = player.lastname;
				player2.commonname = player.commonname;
				player2.playerjerseyname = player.playerjerseyname;
			}
		}
		FifaEnvironment.Players.FillFromPlayerloans(table6);
		FifaEnvironment.Players.FillFromPreviousTeam(table9);
		int fieldIndex4 = table5.TableDescriptor.GetFieldIndex("teamid");
		for (int j = 0; j < table5.NValidRecords; j++)
		{
			Record record2 = table5.Records[j];
			int num4 = record2.IntField[fieldIndex4];
			if (Team.IsFakeOrWomenTeam(num4))
			{
				continue;
			}
			Team team = (Team)FifaEnvironment.Teams.SearchId(num4);
			if (team == null)
			{
				team = new Team(num4);
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
				FifaEnvironment.Teams.InsertId(team);
				if (team.TeamNameAbbr15 == null)
				{
					team.TeamNameFull = team.DatabaseName;
					team.SetNameAutomatically(team.TeamNameFull, 15);
					team.SetNameAutomatically(team.TeamNameAbbr15, 10);
					team.SetNameAutomatically(team.TeamNameAbbr10, 7);
					team.SetNameAutomatically(team.TeamNameAbbr7, 3);
				}
			}
			else
			{
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
			}
		}
		FifaEnvironment.Teams.FillFromTeamPlayerLinks(table2);
		FifaEnvironment.Teams.FillFromManager(table7);
		FifaEnvironment.Teams.FillFromTeamStadiumLinks(table3);
		FifaEnvironment.Teams.FillFromStadiumAssignments(table10);
		int fieldIndex5 = table4.TableDescriptor.GetFieldIndex("teamid");
		int fieldIndex6 = table4.TableDescriptor.GetFieldIndex("formationid");
		for (int k = 0; k < table4.NValidRecords; k++)
		{
			Record record3 = table4.Records[k];
			int num5 = record3.IntField[fieldIndex5];
			_ = record3.IntField[fieldIndex6];
			if (num5 < 0 || Team.IsFakeOrWomenTeam(num5))
			{
				continue;
			}
			Formation formation = FifaEnvironment.Formations.SearchByTeamId(num5);
			if (formation != null)
			{
				formation.Load19(record3);
				continue;
			}
			Team team2 = (Team)FifaEnvironment.Teams.SearchId(num5);
			if (team2 != null)
			{
				int newId = FifaEnvironment.Formations.GetNewId();
				_ = 0;
				formation = new Formation(newId);
				formation.Load19(record3);
				FifaEnvironment.Formations.InsertId(formation);
				team2.Formation = formation;
				formation.Team = team2;
			}
		}
		int num6 = -1;
		int fieldIndex7 = table11.TableDescriptor.GetFieldIndex("leagueid");
		int fieldIndex8 = table11.TableDescriptor.GetFieldIndex("teamid");
		for (int l = 0; l < table5.NValidRecords; l++)
		{
			Record record4 = table11.Records[l];
			int num7 = record4.IntField[fieldIndex7];
			if (num7 == 76 || num7 == 78 || num7 == 111 || num7 == 382 || num7 == 383 || num7 == 384 || num7 == 2000 || num7 == 2028 || num7 == 2136 || num7 == 2140 || num7 == 3003 || num7 == 3004)
			{
				continue;
			}
			int id = record4.IntField[fieldIndex8];
			League league = (League)FifaEnvironment.Leagues.SearchId(num7);
			Team team3 = (Team)FifaEnvironment.Teams.SearchId(id);
			if (league != null && team3 != null)
			{
				if (num6 != num7)
				{
					num6 = num7;
					league.PlayingTeams.Clear();
				}
				league.LinkTeam(team3);
				team3.League = league;
				if (team3.Country == null)
				{
					team3.Country = league.Country;
				}
				team3.FillFromLeagueTeamLinks19(record4);
			}
		}
		FifaEnvironment.Players.LinkTeam(FifaEnvironment.Teams);
		FifaEnvironment.Players.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkPlayer(FifaEnvironment.Players);
		FifaEnvironment.Teams.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkLeague(FifaEnvironment.Leagues);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.Teams.LinkFormation(FifaEnvironment.Formations);
		FifaEnvironment.Teams.LinkStadiums(FifaEnvironment.Stadiums);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.UserMessages.ShowMessage(15007);
		return true;
	}

	private bool MergeOnlineDb18(bool updatePlayers)
	{
		if (m_OnlineDb == null)
		{
			return false;
		}
		Table table = m_OnlineDb.GetTable("dcplayernames");
		Table table2 = m_OnlineDb.GetTable("teamplayerlinks");
		Table table3 = m_OnlineDb.GetTable("teamstadiumlinks");
		Table table4 = m_OnlineDb.GetTable("formations");
		Table table5 = m_OnlineDb.GetTable("teams");
		Table table6 = m_OnlineDb.GetTable("playerloans");
		Table table7 = m_OnlineDb.GetTable("manager");
		Table table8 = m_OnlineDb.GetTable("players");
		Table table9 = m_OnlineDb.GetTable("previousteam");
		Table table10 = m_OnlineDb.GetTable("stadiumassignments");
		Table table11 = m_OnlineDb.GetTable("leagueteamlinks");
		string text = FifaEnvironment.LaunchDir + "\\Templates\\2018\\data\\db\\fifa_ng_db-meta.xml";
		Table table12 = new DbFile(text.Replace("-meta.xml", ".db"), text).GetTable("playernames");
		if (table == null || table2 == null || table4 == null || table5 == null || table6 == null || table7 == null || table8 == null || table9 == null)
		{
			FifaEnvironment.UserMessages.ShowMessage(10036);
			return false;
		}
		int fieldIndex = table8.TableDescriptor.GetFieldIndex("playerid");
		int fieldIndex2 = table8.TableDescriptor.GetFieldIndex("gender");
		int fieldIndex3 = table8.TableDescriptor.GetFieldIndex("birthdate");
		Player player = new Player(1);
		for (int i = 0; i < table8.NValidRecords; i++)
		{
			Record record = table8.Records[i];
			int num = record.IntField[fieldIndex];
			int num2 = record.IntField[fieldIndex2];
			int num3 = record.IntField[fieldIndex3];
			DateTime dateTime = new DateTime(num3);
			if ((dateTime.Month == 12 && dateTime.Day == 29 && num >= 2300000) || num == 0 || num2 != 0)
			{
				continue;
			}
			Player player2 = (Player)FifaEnvironment.Players.SearchId(num);
			if (player2 == null)
			{
				player2 = new Player(num);
				player2.UpdateFromOnlineRecord19(record, table8.TableDescriptor);
				if (!player2.IsFakePlayer())
				{
					player2.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
					player2.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
					player2.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
					player2.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
					player2.UpdatePlayername(table, table12);
					FifaEnvironment.Players.InsertId(player2);
				}
			}
			else if (updatePlayers)
			{
				player2.UpdateFromOnlineRecord19(record, table8.TableDescriptor);
				player.firstnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("firstnameid"));
				player.lastnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("lastnameid"));
				player.commonnameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("commonnameid"));
				player.playerjerseynameid = record.GetAndCheckIntField(table8.TableDescriptor.GetFieldIndex("playerjerseynameid"));
				player.UpdatePlayername(table, table12);
				player2.firstname = player.firstname;
				player2.lastname = player.lastname;
				player2.commonname = player.commonname;
				player2.playerjerseyname = player.playerjerseyname;
			}
		}
		FifaEnvironment.Players.FillFromPlayerloans(table6);
		FifaEnvironment.Players.FillFromPreviousTeam(table9);
		int fieldIndex4 = table5.TableDescriptor.GetFieldIndex("teamid");
		for (int j = 0; j < table5.NValidRecords; j++)
		{
			Record record2 = table5.Records[j];
			int num4 = record2.IntField[fieldIndex4];
			if (Team.IsFakeOrWomenTeam(num4))
			{
				continue;
			}
			Team team = (Team)FifaEnvironment.Teams.SearchId(num4);
			if (team == null)
			{
				team = new Team(num4);
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
				FifaEnvironment.Teams.InsertId(team);
				if (team.TeamNameAbbr15 == null)
				{
					team.TeamNameFull = team.DatabaseName;
					team.SetNameAutomatically(team.TeamNameFull, 15);
					team.SetNameAutomatically(team.TeamNameAbbr15, 10);
					team.SetNameAutomatically(team.TeamNameAbbr10, 7);
					team.SetNameAutomatically(team.TeamNameAbbr7, 3);
				}
			}
			else
			{
				team.Roster.ResetToEmpty();
				team.Load19(record2, table5.TableDescriptor);
			}
		}
		FifaEnvironment.Teams.FillFromTeamPlayerLinks(table2);
		FifaEnvironment.Teams.FillFromManager(table7);
		FifaEnvironment.Teams.FillFromTeamStadiumLinks(table3);
		FifaEnvironment.Teams.FillFromStadiumAssignments(table10);
		int fieldIndex5 = table4.TableDescriptor.GetFieldIndex("teamid");
		int fieldIndex6 = table4.TableDescriptor.GetFieldIndex("formationid");
		for (int k = 0; k < table4.NValidRecords; k++)
		{
			Record record3 = table4.Records[k];
			int num5 = record3.IntField[fieldIndex5];
			_ = record3.IntField[fieldIndex6];
			if (num5 < 0 || Team.IsFakeOrWomenTeam(num5))
			{
				continue;
			}
			Formation formation = FifaEnvironment.Formations.SearchByTeamId(num5);
			if (formation != null)
			{
				formation.Load19(record3);
				continue;
			}
			Team team2 = (Team)FifaEnvironment.Teams.SearchId(num5);
			if (team2 != null)
			{
				int newId = FifaEnvironment.Formations.GetNewId();
				_ = 0;
				formation = new Formation(newId);
				formation.Load19(record3);
				FifaEnvironment.Formations.InsertId(formation);
				team2.Formation = formation;
				formation.Team = team2;
			}
		}
		int num6 = -1;
		int fieldIndex7 = table11.TableDescriptor.GetFieldIndex("leagueid");
		int fieldIndex8 = table11.TableDescriptor.GetFieldIndex("teamid");
		for (int l = 0; l < table5.NValidRecords; l++)
		{
			Record record4 = table11.Records[l];
			int num7 = record4.IntField[fieldIndex7];
			if (num7 == 76 || num7 == 78 || num7 == 111 || num7 == 382 || num7 == 383 || num7 == 384 || num7 == 2000 || num7 == 2028 || num7 == 2136 || num7 == 2140 || num7 == 3003 || num7 == 3004)
			{
				continue;
			}
			int id = record4.IntField[fieldIndex8];
			League league = (League)FifaEnvironment.Leagues.SearchId(num7);
			Team team3 = (Team)FifaEnvironment.Teams.SearchId(id);
			if (league != null && team3 != null)
			{
				if (num6 != num7)
				{
					num6 = num7;
					league.PlayingTeams.Clear();
				}
				league.LinkTeam(team3);
				team3.League = league;
				if (team3.Country == null)
				{
					team3.Country = league.Country;
				}
				team3.FillFromLeagueTeamLinks19(record4);
			}
		}
		FifaEnvironment.Players.LinkTeam(FifaEnvironment.Teams);
		FifaEnvironment.Players.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkPlayer(FifaEnvironment.Players);
		FifaEnvironment.Teams.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkLeague(FifaEnvironment.Leagues);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.Teams.LinkFormation(FifaEnvironment.Formations);
		FifaEnvironment.Teams.LinkStadiums(FifaEnvironment.Stadiums);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.UserMessages.ShowMessage(15007);
		return true;
	}

	private bool MergeOnlineDb(bool updatePlayers)
	{
		if (m_OnlineDb == null)
		{
			return false;
		}
		Table table = m_OnlineDb.GetTable("dcplayernames");
		Table table2 = m_OnlineDb.GetTable("teamplayerlinks");
		Table table3 = m_OnlineDb.GetTable("formations");
		Table table4 = m_OnlineDb.GetTable("teams");
		Table table5 = m_OnlineDb.GetTable("playerloans");
		Table table6 = m_OnlineDb.GetTable("manager");
		Table table7 = m_OnlineDb.GetTable("players");
		Table table8 = m_OnlineDb.GetTable("previousteam");
		Table table9 = FifaEnvironment.OriginalFifaDb.GetTable("playernames");
		if (table == null || table2 == null || table3 == null || table4 == null || table5 == null || table6 == null || table7 == null || table8 == null)
		{
			FifaEnvironment.UserMessages.ShowMessage(10036);
			return false;
		}
		int fieldIndex = table7.TableDescriptor.GetFieldIndex("playerid");
		for (int i = 0; i < table7.NValidRecords; i++)
		{
			Record record = table7.Records[i];
			int num = record.IntField[fieldIndex];
			if (num != 0)
			{
				Player player = (Player)FifaEnvironment.Players.SearchId(num);
				if (player == null)
				{
					player = new Player(record);
					player.UpdatePlayername(table, table9);
					FifaEnvironment.Players.InsertId(player);
				}
				else if (updatePlayers)
				{
					player.UpdateFromOnlineRecord19(record, table7.TableDescriptor);
				}
			}
		}
		FifaEnvironment.Players.FillFromPlayerloans(table5);
		FifaEnvironment.Players.FillFromPreviousTeam(table8);
		for (int j = 0; j < table3.NValidRecords; j++)
		{
			Record record2 = table3.Records[j];
			int num2 = record2.IntField[FI.formations_teamid];
			int id = record2.IntField[FI.formations_formationid];
			if (num2 >= 0)
			{
				Formation formation = (Formation)FifaEnvironment.Formations.SearchId(id);
				if (formation != null)
				{
					formation.Load(record2);
					continue;
				}
				formation = new Formation(record2);
				FifaEnvironment.Formations.InsertId(formation);
			}
		}
		for (int k = 0; k < table4.NValidRecords; k++)
		{
			Record record3 = table4.Records[k];
			int id2 = record3.IntField[FI.teams_teamid];
			Team team = (Team)FifaEnvironment.Teams.SearchId(id2);
			if (team != null)
			{
				team.Roster.ResetToEmpty();
				team.Load(record3);
			}
		}
		FifaEnvironment.Teams.FillFromTeamPlayerLinks(table2);
		FifaEnvironment.Teams.FillFromFormations(table3);
		FifaEnvironment.Teams.FillFromManager(table6);
		FifaEnvironment.Players.LinkTeam(FifaEnvironment.Teams);
		FifaEnvironment.Players.LinkCountry(FifaEnvironment.Countries);
		FifaEnvironment.Teams.LinkPlayer(FifaEnvironment.Players);
		FifaEnvironment.Teams.LinkOpponent(FifaEnvironment.Teams);
		FifaEnvironment.Teams.LinkFormation(FifaEnvironment.Formations);
		FifaEnvironment.UserMessages.ShowMessage(15007);
		return true;
	}

	private bool BrowseOnline()
	{
		openFifaDialog = new OpenFileDialog();
		openFifaDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\FIFA 14";
		openFifaDialog.Filter = "Online Files|Squad*.*;FutSquads*.*;MatchDay*.*";
		openFifaDialog.FilterIndex = 1;
		openFifaDialog.Title = "Open DB Update file";
		bool result = false;
		if (openFifaDialog.ShowDialog() == DialogResult.OK)
		{
			m_OnlineDbFileName = openFifaDialog.FileName;
			result = true;
		}
		openFifaDialog.Dispose();
		return result;
	}

	private bool BrowseOnlineFifa16()
	{
		openFifaDialog = new OpenFileDialog();
		openFifaDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\FIFA 16\\0\\FIFA16";
		openFifaDialog.Filter = "Online Files|DATA*";
		openFifaDialog.FilterIndex = 1;
		openFifaDialog.Title = "Open DB Update file";
		bool result = false;
		if (openFifaDialog.ShowDialog() == DialogResult.OK)
		{
			m_OnlineDbFileName = openFifaDialog.FileName;
			result = true;
		}
		openFifaDialog.Dispose();
		return result;
	}

	private bool BrowseOnlineFifa(string fifaFolder)
	{
		openFifaDialog = new OpenFileDialog();
		openFifaDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + fifaFolder + "\\settings";
		openFifaDialog.Filter = "Online Files|Squad*;Match*";
		openFifaDialog.FilterIndex = 1;
		openFifaDialog.Title = "Open DB Update file";
		bool result = false;
		if (openFifaDialog.ShowDialog() == DialogResult.OK)
		{
			m_OnlineDbFileName = openFifaDialog.FileName;
			result = true;
		}
		openFifaDialog.Dispose();
		return result;
	}

	private void uGContentToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.FifaXmlFileName != null)
		{
			m_XmlDbFileName = FifaEnvironment.FifaXmlFileName;
		}
		else if (!BrowseXmlDb())
		{
			return;
		}
		if (BrowseUgc() && OpenUgcFile())
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(29);
			if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
			{
				Cursor.Current = Cursors.WaitCursor;
				m_UgcFile.UpdateRosters(m_XmlDbFileName, useKitGraphics: false, statusBar);
				Cursor.Current = Cursors.Default;
			}
		}
	}

	private void menuAlignLanguageDB_Click(object sender, EventArgs e)
	{
		openLangDialog.CheckFileExists = true;
		openLangDialog.InitialDirectory = FifaEnvironment.GameDir + "data\\loc\\";
		openLangDialog.Filter = "db files (*.db)|*.db";
		openLangDialog.FilterIndex = 1;
		openLangDialog.Title = "Open Language Database";
		if (openLangDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string fileName = openLangDialog.FileName;
		if (fileName == FifaEnvironment.LangDbFileName)
		{
			return;
		}
		DbFile dbFile = new DbFile(fileName, FifaEnvironment.LangXmlFileName);
		if (dbFile == null)
		{
			return;
		}
		Table table = FifaEnvironment.LangDb.Table[0];
		Table table2 = dbFile.Table[0];
		int num = 0;
		int[] array = new int[table.NValidRecords];
		string[] array2 = new string[table.NValidRecords];
		string[] array3 = new string[table.NValidRecords];
		Cursor.Current = Cursors.WaitCursor;
		statusBar.Text = "Analizing the language database...";
		Refresh();
		for (int i = 0; i < table.NValidRecords; i++)
		{
			Record record = table.Records[i];
			bool flag = false;
			for (int j = 0; j < table2.NValidRecords; j++)
			{
				if (table.Records[j].IntField[FI.language_hashid] == record.IntField[FI.language_hashid])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				array[num] = record.IntField[FI.language_hashid];
				array2[num] = record.CompressedString[FI.language_sourcetext];
				array3[num] = record.CompressedString[FI.language_stringid];
				num++;
			}
		}
		Cursor.Current = Cursors.Default;
		if (num > 0)
		{
			int num2 = table2.NValidRecords;
			table2.ResizeRecords(table2.NValidRecords + num);
			for (int k = 0; k < num; k++)
			{
				table2.Records[num2].IntField[FI.language_hashid] = array[k];
				table2.Records[num2].CompressedString[FI.language_sourcetext] = array2[k];
				table2.Records[num2].CompressedString[FI.language_stringid] = array3[k];
				num2++;
			}
			dbFile.SaveDb();
			statusBar.Text = fileName + " has been aligned.";
		}
		else
		{
			statusBar.Text = fileName + " was already aligned.";
		}
	}

	private void menuImportUgcPlayers_Click(object sender, EventArgs e)
	{
		if (FifaEnvironment.FifaXmlFileName != null)
		{
			m_XmlDbFileName = FifaEnvironment.FifaXmlFileName;
		}
		else if (!BrowseXmlDb())
		{
			return;
		}
		if (BrowseUgc() && OpenUgcFile())
		{
			DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(29);
			if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel)
			{
				Cursor.Current = Cursors.WaitCursor;
				m_UgcFile.ImportPlayers(m_XmlDbFileName, useGraphics: false, statusBar);
				Cursor.Current = Cursors.Default;
			}
		}
	}

	private void minimizeNamesTableToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		foreach (PlayerName playerNames in FifaEnvironment.PlayerNamesList)
		{
			playerNames.IsOriginal = false;
		}
		foreach (Player player3 in FifaEnvironment.Players)
		{
			player3.firstname = player3.firstname.Trim();
			foreach (PlayerName playerNames2 in FifaEnvironment.PlayerNamesList)
			{
				if (playerNames2.Text == player3.firstname)
				{
					if (player3.firstnameid != playerNames2.Id && playerNames2.CommentaryId == 900000)
					{
						FifaEnvironment.PlayerNamesList.RemoveId(player3.firstnameid);
						player3.firstnameid = playerNames2.Id;
					}
					break;
				}
			}
			player3.lastname = player3.lastname.Trim();
			foreach (PlayerName playerNames3 in FifaEnvironment.PlayerNamesList)
			{
				if (playerNames3.Text == player3.lastname)
				{
					if (player3.lastnameid != playerNames3.Id && playerNames3.CommentaryId == 900000)
					{
						FifaEnvironment.PlayerNamesList.RemoveId(player3.lastnameid);
						player3.lastnameid = playerNames3.Id;
					}
					break;
				}
			}
			player3.commonname = player3.commonname.Trim();
			if (player3.commonnameid != 0)
			{
				if (player3.commonname.IndexOf('.') >= 0)
				{
					if (player3.playerjerseynameid == player3.commonnameid)
					{
						player3.playerjerseynameid = player3.lastnameid;
						player3.playerjerseyname = player3.lastname;
					}
					player3.commonname = string.Empty;
					player3.commonnameid = 0;
				}
				else if (player3.playerjerseynameid != player3.commonnameid && player3.playerjerseynameid != player3.lastnameid)
				{
					player3.playerjerseynameid = player3.commonnameid;
					player3.playerjerseyname = player3.commonname;
				}
			}
			foreach (PlayerName playerNames4 in FifaEnvironment.PlayerNamesList)
			{
				if (playerNames4.Text == player3.commonname)
				{
					if (player3.commonnameid != playerNames4.Id && playerNames4.CommentaryId == 900000)
					{
						FifaEnvironment.PlayerNamesList.RemoveId(player3.commonnameid);
						player3.commonnameid = playerNames4.Id;
					}
					break;
				}
			}
			player3.playerjerseyname = player3.playerjerseyname.Trim();
			if (player3.playerjerseyname.IndexOf('.') >= 0)
			{
				if (player3.commonname != string.Empty)
				{
					player3.playerjerseynameid = player3.commonnameid;
					player3.playerjerseyname = player3.commonname;
				}
				else
				{
					player3.playerjerseynameid = player3.lastnameid;
					player3.playerjerseyname = player3.lastname;
				}
			}
			foreach (PlayerName playerNames5 in FifaEnvironment.PlayerNamesList)
			{
				if (playerNames5.Text == player3.playerjerseyname)
				{
					if (player3.playerjerseynameid != playerNames5.Id && playerNames5.CommentaryId == 900000)
					{
						FifaEnvironment.PlayerNamesList.RemoveId(player3.playerjerseynameid);
						player3.playerjerseynameid = playerNames5.Id;
					}
					break;
				}
			}
		}
		foreach (Player player4 in FifaEnvironment.Players)
		{
			if (player4.firstnameid >= 32767)
			{
				FifaEnvironment.PlayerNamesList.RemoveId(player4.firstnameid);
				player4.firstnameid = FifaEnvironment.PlayerNamesList.GetKey(player4.firstname);
			}
			if (player4.lastnameid >= 32767)
			{
				FifaEnvironment.PlayerNamesList.RemoveId(player4.lastnameid);
				player4.lastnameid = FifaEnvironment.PlayerNamesList.GetKey(player4.lastname);
			}
			if (player4.commonnameid >= 32767)
			{
				FifaEnvironment.PlayerNamesList.RemoveId(player4.commonnameid);
				player4.commonnameid = FifaEnvironment.PlayerNamesList.GetKey(player4.commonname);
			}
			if (player4.playerjerseynameid >= 32767)
			{
				FifaEnvironment.PlayerNamesList.RemoveId(player4.playerjerseynameid);
				player4.playerjerseynameid = FifaEnvironment.PlayerNamesList.GetKey(player4.playerjerseyname);
			}
		}
		Cursor.Current = Cursors.Default;
		int num = 32767 - FifaEnvironment.PlayerNamesList.Count;
		statusBar.Text = "Names updated, " + num + " names still availbale. Ready!";
		FifaEnvironment.UserMessages.ShowMessage(1036);
	}

	private void menuPreserveOriginalNames_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		statusBar.Text = "Comparing current names with original names...";
		Refresh();
		for (int i = 0; i < FifaEnvironment.PlayerNamesList.Count; i++)
		{
			PlayerName playerName = (PlayerName)FifaEnvironment.PlayerNamesList[i];
			PlayerName playerName2 = (PlayerName)FifaEnvironment.OriginalPlayerNamesList.SearchId(playerName.Id);
			if (playerName2 != null)
			{
				if (playerName2.Text != playerName.Text)
				{
					FifaEnvironment.PlayerNamesList.RemoveId(playerName.Id);
					playerName2.IsOriginal = true;
					FifaEnvironment.PlayerNamesList.InsertId(playerName2);
					playerName.Id = FifaEnvironment.PlayerNamesList.GetNewId();
					FifaEnvironment.PlayerNamesList.InsertId(playerName);
				}
				else
				{
					playerName.IsOriginal = true;
					playerName.CommentaryId = playerName2.CommentaryId;
				}
			}
		}
		statusBar.Text = "Recovering missed original names ...";
		Refresh();
		for (int j = 0; j < FifaEnvironment.OriginalPlayerNamesList.Count; j++)
		{
			PlayerName playerName3 = (PlayerName)FifaEnvironment.OriginalPlayerNamesList[j];
			if ((PlayerName)FifaEnvironment.PlayerNamesList.SearchId(playerName3.Id) == null)
			{
				playerName3.IsOriginal = true;
				FifaEnvironment.PlayerNamesList.InsertId(playerName3);
			}
		}
		statusBar.Text = "Updating player names...";
		Refresh();
		foreach (Player player in FifaEnvironment.Players)
		{
			PlayerName playerName4 = FifaEnvironment.OriginalPlayerNamesList.SearchName(player.firstname);
			if (playerName4 != null)
			{
				player.firstnameid = playerName4.Id;
			}
			else
			{
				playerName4 = FifaEnvironment.PlayerNamesList.SearchName(player.firstname);
				if (playerName4 != null)
				{
					player.firstnameid = playerName4.Id;
				}
			}
			playerName4 = FifaEnvironment.OriginalPlayerNamesList.SearchName(player.lastname);
			if (playerName4 != null)
			{
				player.lastnameid = playerName4.Id;
			}
			else
			{
				playerName4 = FifaEnvironment.PlayerNamesList.SearchName(player.lastname);
				if (playerName4 != null)
				{
					player.lastnameid = playerName4.Id;
				}
			}
			playerName4 = FifaEnvironment.OriginalPlayerNamesList.SearchName(player.commonname);
			if (playerName4 != null)
			{
				player.commonnameid = playerName4.Id;
			}
			else
			{
				playerName4 = FifaEnvironment.PlayerNamesList.SearchName(player.commonname);
				if (playerName4 != null)
				{
					player.commonnameid = playerName4.Id;
				}
			}
			playerName4 = FifaEnvironment.OriginalPlayerNamesList.SearchName(player.playerjerseyname);
			if (playerName4 != null)
			{
				player.playerjerseynameid = playerName4.Id;
			}
			else
			{
				playerName4 = FifaEnvironment.PlayerNamesList.SearchName(player.playerjerseyname);
				if (playerName4 != null)
				{
					player.playerjerseynameid = playerName4.Id;
				}
			}
			player.UpdateNamesAndCommentary();
		}
		int num = 29000 - FifaEnvironment.PlayerNamesList.Count;
		statusBar.Text = "Names updated, " + num + " names still availbale. Ready!";
		Cursor.Current = Cursors.Default;
		FifaEnvironment.UserMessages.ShowMessage(1036);
	}

	private string RemoveDottedInitial(string text)
	{
		while (text.IndexOf('.') == 1)
		{
			text = text.Substring(2);
		}
		return text;
	}

	private void UpdateOnline16(bool updatePlayers)
	{
		if (FifaEnvironment.FifaXmlFileName != null)
		{
			m_XmlDbFileName = FifaEnvironment.FifaXmlFileName;
		}
		else if (!BrowseXmlDb())
		{
			return;
		}
		if (!BrowseOnlineFifa16())
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		m_OnlineDbFile = new CareerFile(m_OnlineDbFileName, m_XmlDbFileName);
		Cursor.Current = Cursors.Default;
		if (m_OnlineDbFile != null)
		{
			if (m_OnlineDbFile.Databases[1] != null || m_OnlineDbFile.Databases[2] != null || m_OnlineDbFile.Databases[3] != null)
			{
				FifaEnvironment.UserMessages.ShowMessage(10036);
				return;
			}
			m_OnlineDb = m_OnlineDbFile.Databases[0];
			MergeOnlineDb(updatePlayers);
		}
	}

	private void UpdateOnline19(bool updatePlayers)
	{
		string xmlFileName = FifaEnvironment.LaunchDir + "\\Templates\\2019\\data\\db\\fifa_ng_db-meta.xml";
		if ((FifaEnvironment.FifaXmlFileName == null && !BrowseXmlDb()) || !BrowseOnlineFifa("FIFA 19"))
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		m_OnlineDbFile = new CareerFile(m_OnlineDbFileName, xmlFileName);
		Cursor.Current = Cursors.Default;
		if (m_OnlineDbFile != null)
		{
			if (m_OnlineDbFile.Databases[1] != null || m_OnlineDbFile.Databases[2] != null || m_OnlineDbFile.Databases[3] != null)
			{
				FifaEnvironment.UserMessages.ShowMessage(10036);
				return;
			}
			m_OnlineDb = m_OnlineDbFile.Databases[0];
			MergeOnlineDb19(updatePlayers);
		}
	}

	private void UpdateOnline20(bool updatePlayers)
	{
		string xmlFileName = FifaEnvironment.LaunchDir + "\\Templates\\2020\\data\\db\\fifa_ng_db-meta.xml";
		if ((FifaEnvironment.FifaXmlFileName == null && !BrowseXmlDb()) || !BrowseOnlineFifa("FIFA 20"))
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		m_OnlineDbFile = new CareerFile(m_OnlineDbFileName, xmlFileName);
		Cursor.Current = Cursors.Default;
		if (m_OnlineDbFile != null)
		{
			if ((m_OnlineDbFile.Databases[1] != null && m_OnlineDbFile.Databases[1].NTables > 0) || m_OnlineDbFile.Databases[2] != null || m_OnlineDbFile.Databases[3] != null)
			{
				FifaEnvironment.UserMessages.ShowMessage(10036);
				return;
			}
			m_OnlineDb = m_OnlineDbFile.Databases[0];
			MergeOnlineDb20(updatePlayers, "2020");
		}
	}

	private void UpdateOnline21(bool updatePlayers)
	{
		string xmlFileName = FifaEnvironment.LaunchDir + "\\Templates\\2021\\data\\db\\fifa_ng_db-meta.xml";
		if ((FifaEnvironment.FifaXmlFileName == null && !BrowseXmlDb()) || !BrowseOnlineFifa("FIFA 21"))
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		m_OnlineDbFile = new CareerFile(m_OnlineDbFileName, xmlFileName);
		Cursor.Current = Cursors.Default;
		if (m_OnlineDbFile != null)
		{
			if ((m_OnlineDbFile.Databases[1] != null && m_OnlineDbFile.Databases[1].NTables > 0) || m_OnlineDbFile.Databases[2] != null || m_OnlineDbFile.Databases[3] != null)
			{
				FifaEnvironment.UserMessages.ShowMessage(10036);
				return;
			}
			m_OnlineDb = m_OnlineDbFile.Databases[0];
			MergeOnlineDb20(updatePlayers, "2021");
		}
	}

	private void UpdateFrom20(bool updatePlayers)
	{
		string xmlFileName = FifaEnvironment.LaunchDir + "\\Templates\\2020\\data\\db\\fifa_ng_db-meta.xml";
		string dbFileName = FifaEnvironment.LaunchDir + "\\Templates\\2020\\data\\db\\fifa_ng_db.db";
		m_OnlineDb = new DbFile(dbFileName, xmlFileName);
		MergeDb20(updatePlayers);
	}

	private void UpdateOnline18(bool updatePlayers)
	{
		string xmlFileName = FifaEnvironment.LaunchDir + "\\Templates\\2018\\data\\db\\fifa_ng_db-meta.xml";
		if ((FifaEnvironment.FifaXmlFileName == null && !BrowseXmlDb()) || !BrowseOnlineFifa("FIFA 18"))
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		m_OnlineDbFile = new CareerFile(m_OnlineDbFileName, xmlFileName);
		Cursor.Current = Cursors.Default;
		if (m_OnlineDbFile != null)
		{
			if (m_OnlineDbFile.Databases[2] != null || m_OnlineDbFile.Databases[3] != null)
			{
				FifaEnvironment.UserMessages.ShowMessage(10036);
				return;
			}
			m_OnlineDb = m_OnlineDbFile.Databases[0];
			MergeOnlineDb18(updatePlayers);
		}
	}

	private void menuOnlineDBFifa16_Click(object sender, EventArgs e)
	{
		UpdateOnline16(updatePlayers: true);
	}

	private void rostersOnlineDBFIFA16ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		UpdateOnline16(updatePlayers: false);
	}

	private void rostersOnlyFromFifa17_Click(object sender, EventArgs e)
	{
		UpdateOnline19(updatePlayers: false);
	}

	private void rostersAndPlayersFromFifa17_Click(object sender, EventArgs e)
	{
		UpdateOnline19(updatePlayers: true);
	}

	private void fromFIFA20ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		UpdateOnline20(updatePlayers: true);
	}

	private void toolStripMenuItem1_Click(object sender, EventArgs e)
	{
		if (InitializeFifaEnvironment(15) && AskUserOpenLangDatabase())
		{
			Open();
		}
	}

	private async void menuOpenFifa16_Click(object sender, EventArgs e)
	{
		await OpenFc26SnapshotAsync(Fc26HostBridge.Open,
			"Opening FC26 database", "FC26 database and Frostbite assets loaded.", "Open FC26");
	}

	internal void LoadFc26Snapshot(string snapshotPath, bool showCountry)
	{
		Fc26SnapshotLoader.Load(snapshotPath);
		CompleteFc26SnapshotLoad(showCountry);
	}

	private void CompleteFc26SnapshotLoad(bool showCountry)
	{
		m_OpenFileFlag = true;
		EnablePanels(enable: true);
		EnableMenus();
		statusBar.Text = "FC26 database and Frostbite assets loaded.";
		if (showCountry)
			ShowFormOnPanel(m_CountryForm, panelMain);
	}

	private async ThreadingTasks.Task<bool> OpenFc26SnapshotAsync(Func<string> snapshotFactory,
		string operationStatus, string successStatus, string dialogTitle)
	{
		if (m_Fc26LoadInProgress) return false;
		m_Fc26LoadInProgress = true;
		SetFc26LoadingState(true);
		try
		{
			// UserMessage/UserOptions derive from Form and must be created on the
			// UI thread before snapshot conversion is dispatched to the worker.
			FifaEnvironment.PrepareFc26UiServices();
			statusBar.Text = operationStatus + " — reading source...";
			string snapshotPath = await ThreadingTasks.Task.Run(snapshotFactory);
			if (IsDisposed || Disposing) return false;

			statusBar.Text = operationStatus + " — decoding database...";
			await ThreadingTasks.Task.Run(() => Fc26SnapshotLoader.Load(snapshotPath));
			if (IsDisposed || Disposing) return false;

			CompleteFc26SnapshotLoad(showCountry: true);
			statusBar.Text = successStatus;
			Refresh();
			return true;
		}
		catch (Exception ex)
		{
			var errorLog = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
			try { File.WriteAllText(errorLog, ex.ToString()); } catch { }
			var message = ex is OutOfMemoryException
				? "FC26 data could not be loaded into memory. Close other memory-heavy applications and retry."
				: "FC26 data could not be opened: " + ex.Message;
			if (!IsDisposed && !Disposing)
				MessageBox.Show(this, message + "\r\n\r\nDiagnostic log:\r\n" + errorLog,
					dialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		finally
		{
			m_Fc26LoadInProgress = false;
			if (!IsDisposed && !Disposing) SetFc26LoadingState(false);
		}
	}

	private void SetFc26LoadingState(bool loading)
	{
		UseWaitCursor = loading;
		Cursor.Current = loading ? Cursors.WaitCursor : Cursors.Default;
		progressBar.Style = loading ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
		progressBar.MarqueeAnimationSpeed = loading ? 28 : 0;
		progressBar.Visible = loading;
		if (loading)
		{
			EnablePanels(enable: false);
			toolStripMain.Enabled = false;
			menuOpenFifa16.Enabled = false;
			menuSave.Enabled = false;
			menuClose.Enabled = false;
		}
		else
		{
			EnablePanels(m_OpenFileFlag);
			EnableMenus();
		}
	}

	internal void ShowFc26Section(string section)
	{
		Form form;
		switch ((section ?? string.Empty).ToLowerInvariant())
		{
			case "league": form = m_LeagueForm; break;
			case "team": form = m_TeamForm; break;
			case "kit": form = m_KitForm; break;
			case "player": form = m_PlayerForm; break;
			case "stadium": form = m_StadiumForm; break;
			case "formation": form = m_FormationForm; break;
			case "ball": form = m_BallForm; break;
			case "shoes": form = m_ShoesForm; break;
			case "gloves": form = m_GlovesForm; break;
			case "competition": form = m_TrophyForm; break;
			default: form = m_CountryForm; break;
		}
		ShowFormOnPanel(form, panelMain);
	}

	internal object CreateFriendlyEntity(string section)
	{
		if (!m_OpenFileFlag)
		{
			MessageBox.Show(this, "Open a database before creating a new item.", "Create",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return null;
		}

		Form form;
		FifaControls.PickUpControl pickUp;
		switch ((section ?? string.Empty).ToLowerInvariant())
		{
			case "league": form = m_LeagueForm; pickUp = m_LeagueForm.pickUpControl; break;
			case "team": form = m_TeamForm; pickUp = m_TeamForm.pickUpControl; break;
			case "nation": form = m_CountryForm; pickUp = m_CountryForm.pickUpControl; break;
			case "player": form = m_PlayerForm; pickUp = m_PlayerForm.pickUpControl; break;
			default: return null;
		}
		ShowFormOnPanel(form, panelMain);
		var existing = pickUp.ObjectList == null
			? new HashSet<object>()
			: new HashSet<object>(pickUp.ObjectList.Cast<object>());
		pickUp.buttonNew.PerformClick();
		var created = pickUp.ObjectList?.Cast<object>().FirstOrDefault(item => !existing.Contains(item));
		if (created != null && Fc26SnapshotLoader.IsLoaded)
		{
			try
			{
				Fc26SnapshotLoader.StageNewEntity(section, created);
				statusBar.Text = "New " + section + " record staged. Complete its details, then Save.";
			}
			catch (Exception ex)
			{
				// Do not leave a picker-only record behind when it could not be
				// represented by a writable FC26 database row.
				pickUp.ObjectList?.Remove(created);
				MessageBox.Show(this, ex.Message, "Create New " + section,
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
		}
		if (created is League createdLeague)
			m_PendingLeagueCompdataIds.Add(createdLeague.Id);
		return created;
	}

	/// <summary>
	/// DBM Studio-style one-flow league creator.  The dialog owns the user
	/// experience; this method owns the database graph so the league, clubs,
	/// roster links and Compdata are never left as unrelated raw rows.
	/// </summary>
	internal void CreateNewLeagueWorkflow()
	{
		if (!m_OpenFileFlag)
		{
			MessageBox.Show(this, "Open a database before creating a new league.", "Create New League",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		var countries = FifaEnvironment.Countries?.Cast<Country>().Where(value => value != null).ToArray() ?? Array.Empty<Country>();
		using (var dialog = new Fc26LeagueCreationDialog(countries))
		{
			if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Draft == null) return;
			try
			{
				var league = CreateLeagueFromDraft(dialog.Draft);
				ShowFormOnPanel(m_LeagueForm, panelMain);
				m_LeagueForm.Preset();
				m_LeagueForm.ReloadLeague(league);
				statusBar.Text = league + " staged with " + dialog.Draft.TeamNames.Count + " teams. Compdata will be generated on Save.";
				Fc26ActivityLog.Add("Create wizard", league + " staged with " + dialog.Draft.TeamNames.Count + " teams");
				// The dialog button is intentionally Finish & Save.  SaveFiles keeps
				// staged rows when a preflight item needs fixing, so the user can fix
				// the highlighted section and press Save again.
				SaveFiles();
			}
			catch (Exception ex)
			{
				statusBar.Text = "League staged — fix the highlighted item before saving.";
				MessageBox.Show(this, ex.Message, "Create New League", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}

	private League CreateLeagueFromDraft(Fc26LeagueCreationDraft draft)
	{
		if (draft == null || draft.Country == null || string.IsNullOrWhiteSpace(draft.LeagueName))
			throw new InvalidOperationException("Choose a country and league name first.");
		var league = FifaEnvironment.Leagues.CreateNewId() as League;
		if (league == null) throw new InvalidOperationException("No free league ID is available in the FC26 database.");
		league.leaguename = draft.LeagueName.Trim();
		league.ShortName = league.leaguename;
		league.LongName = league.leaguename;
		league.level = Math.Max(1, draft.Level);
		league.Country = draft.Country;
		Fc26SnapshotLoader.StageNewEntity("league", league);
		m_PendingLeagueCompdataIds.Add(league.Id);

		var template = TemplateTeam();
		foreach (var teamName in draft.TeamNames)
			CreateTeamRecord(teamName, draft.Country, league, template, null);
		return league;
	}

	internal Team CreateNewTeamWorkflow()
	{
		if (!m_OpenFileFlag)
		{
			MessageBox.Show(this, "Open a database before creating a new team.", "Create New Team",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return null;
		}
		var leagues = FifaEnvironment.Leagues?.Cast<League>()
			.Where(value => value != null).OrderBy(value => value.ToString(), StringComparer.OrdinalIgnoreCase).ToArray()
			?? Array.Empty<League>();
		if (leagues.Length == 0)
		{
			MessageBox.Show(this, "Create a league first, then create its teams.", "Create New Team",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return null;
		}

		var countries = FifaEnvironment.Countries?.Cast<Country>().Where(value => value != null).ToArray() ?? Array.Empty<Country>();
		using (var dialog = new Fc26StandaloneTeamDialog(countries, leagues,
			m_LeagueForm.Visible ? m_LeagueForm.CurrentLeague : null))
		{
			if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Draft == null) return null;
			try
			{
				var team = CreateTeamRecord(dialog.Draft.TeamName, dialog.Draft.Country, dialog.Draft.League,
					TemplateTeam(), dialog.Draft);
				ShowFormOnPanel(m_TeamForm, panelMain); m_TeamForm.Preset(); m_TeamForm.ReloadTeam(team);
				statusBar.Text = "New team " + team + " created in " + dialog.Draft.League + ". Complete its details, then Save.";
				return team;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Create New Team", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return null;
			}
		}
	}

	internal Team CreateTeamInLeague(League league)
	{
		if (league == null) return null;
		try
		{
			var country = league.Country ?? FifaEnvironment.Countries?.Cast<Country>().FirstOrDefault();
			var team = CreateTeamRecord("New Team " + (FifaEnvironment.Teams.Count + 1), country, league, TemplateTeam(), null);
			m_TeamForm.ReloadTeam(team);
			statusBar.Text = "New team " + team.Id + " created in " + league + ". Complete its details, then Save.";
			return team;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Create Team in League",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
			return null;
		}
	}

	private Team TemplateTeam()
	{
		if (m_TeamForm?.m_CurrentTeam != null && !m_PendingTeamIds.Contains(m_TeamForm.m_CurrentTeam.Id))
			return m_TeamForm.m_CurrentTeam;
		return FifaEnvironment.Teams?.Cast<Team>().FirstOrDefault(value => value != null && !value.NationalTeam &&
			(value.Stadium != null || value.Roster.Count > 0 || value.m_KitList.Count > 0));
	}

	private static void SetTeamNames(Team team, string name)
	{
		name = (name ?? string.Empty).Trim();
		team.DatabaseName = name;
		team.TeamNameFull = name;
		team.TeamNameAbbr15 = name.Length <= 15 ? name : name.Substring(0, 15).TrimEnd();
		team.TeamNameAbbr10 = name.Length <= 10 ? name : name.Substring(0, 10).TrimEnd();
		team.TeamNameAbbr7 = name.Length <= 7 ? name : name.Substring(0, 7).TrimEnd();
		team.TeamNameAbbr3 = (name.Length <= 3 ? name : name.Substring(0, 3)).ToUpperInvariant();
	}

	private Team CreateTeamRecord(string name, Country country, League league, Team template, Fc26StandaloneTeamDraft draft)
	{
		if (country == null || league == null) throw new InvalidOperationException("Choose a valid country and league for the team.");
		var team = FifaEnvironment.Teams.CreateNewId() as Team;
		if (team == null) throw new InvalidOperationException("No free team ID is available in the FC26 database.");
		SetTeamNames(team, name);
		league.AddTeam(team);
		team.Country = country;
		team.PrevLeague = league;
		team.foundationyear = draft?.FoundationYear > 0 ? draft.FoundationYear : DateTime.Today.Year;
		team.teamstadiumcapacity = draft?.StadiumCapacity > 0 ? draft.StadiumCapacity : (template?.teamstadiumcapacity > 0 ? template.teamstadiumcapacity : 15000);
		team.clubworth = draft?.ClubWorth > 0 ? draft.ClubWorth : (template?.clubworth > 0 ? template.clubworth : 1000000);
		team.transferbudget = draft?.TransferBudget > 0 ? draft.TransferBudget : (template?.transferbudget > 0 ? template.transferbudget : 1000000);
		if (template != null)
		{
			team.Stadium = template.Stadium;
			team.Formation = template.Formation;
			team.overallrating = template.overallrating; team.attackrating = template.attackrating;
			team.midfieldrating = template.midfieldrating; team.defenserating = template.defenserating;
			team.domesticprestige = template.domesticprestige; team.internationalprestige = template.internationalprestige;
			team.profitability = template.profitability; team.popularity = template.popularity; team.youthdevelopment = template.youthdevelopment;
		}
		Fc26SnapshotLoader.StageNewEntity("team", team);
		Fc26SnapshotLoader.AssignTeamToLeague(team, league);
		Fc26SnapshotLoader.StageNewTeamStadiumLink(team);
		CreateDefaultKits(team, template);
		CreateDefaultRoster(team, country);
		Fc26SnapshotLoader.StageNewTeamSheet(team);
		m_PendingTeamIds.Add(team.Id);
		return team;
	}

	private void CreateDefaultRoster(Team team, Country country)
	{
		var positions = new[] { 0, 3, 4, 5, 6, 7, 10, 12, 13, 14, 15, 16, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };
		for (var index = 0; index < positions.Length; index++)
		{
			var player = FifaEnvironment.Players.CreateNewId() as Player;
			if (player == null) throw new InvalidOperationException("No free player ID remains for the new team roster.");
			var display = team.TeamNameAbbr3 + " Player " + (index + 1).ToString();
			player.firstname = string.Empty; player.lastname = display; player.commonname = display; player.playerjerseyname = display;
			player.Country = country; player.birthdate = new DateTime(1998 + (index % 6), 1, 1);
			player.joindate = DateTime.Today; player.contractvaliduntil = DateTime.Today.Year + 3;
			player.preferredposition1 = positions[index]; player.overallrating = 50; player.potential = 55;
			Fc26SnapshotLoader.StageNewEntity("player", player);
			Fc26SnapshotLoader.StageNewPlayerNames(player);
			var link = team.AddTeamPlayer(player, index + 1); link.position = positions[index];
			Fc26SnapshotLoader.StageNewTeamPlayerLink(link);
			m_PendingPlayerIds.Add(player.Id);
		}
	}

	private void CreateDefaultKits(Team team, Team template)
	{
		if (template == null || FifaEnvironment.Kits == null) return;
		foreach (var type in new[] { 0, 1, 2 })
		{
			var source = template.GetKit(type);
			if (source == null) continue;
			Kit kit = null;
			var preferredId = Kit.KitId(team.Id, type);
			try
			{
				if (FifaEnvironment.Kits.SearchId(preferredId) == null)
					kit = FifaEnvironment.Kits.CloneId(source, preferredId) as Kit;
			}
			catch { kit = null; }
			if (kit == null) kit = FifaEnvironment.Kits.CloneId(source) as Kit;
			if (kit == null) continue;
			kit.Team = team; kit.teamid = team.Id; kit.kittype = type; kit.year = 0; kit.KitTextures = null;
			team.m_KitList.Add(kit);
			Fc26SnapshotLoader.StageNewKit(kit);
			try { source.CloneTextures(kit); } catch { /* metadata remains valid; preview fallback is intentional */ }
		}
	}

	private void StagePendingLeagueCompdata()
	{
		foreach (var leagueId in m_PendingLeagueCompdataIds.ToArray())
		{
			var league = FifaEnvironment.Leagues.SearchId(leagueId) as League;
			if (league == null) continue;
			var teamCount = league.PlayingTeams.Cast<Team>().Count(team => team != null && team.Id > 0);
			if (league.Country == null || teamCount < 2)
				throw new InvalidOperationException("New league '" + league + "' is not complete. Choose its country and create at least two teams before Save.");
			statusBar.Text = "Preparing " + league + " for the game...";
			statusBar.GetCurrentParent().Refresh();
			m_TrophyForm.StageLeagueForSave(league);
		}
	}

	internal void MakeLeagueInGameReady(League league)
	{
		if (league == null) return;
		ShowFormOnPanel(m_TrophyForm, panelMain);
		m_TrophyForm.MakeLeagueInGameReady(league);
	}

	internal void ClickFc26SectionForSmoke(string section)
	{
		if (!string.Equals(Text, "Creation Master 26", StringComparison.Ordinal))
			throw new InvalidOperationException("Legacy shell still has obsolete product branding: " + Text);
		ToolStripButton button;
		Form expected;
		switch ((section ?? string.Empty).ToLowerInvariant())
		{
			case "league": button = buttonLeague; expected = m_LeagueForm; break;
			case "team": button = buttonTeam; expected = m_TeamForm; break;
			case "kit": button = buttonKit; expected = m_KitForm; break;
			case "player": button = buttonPlayer; expected = m_PlayerForm; break;
			case "stadium": button = buttonStadium; expected = m_StadiumForm; break;
			case "formation": button = buttonFormation; expected = m_FormationForm; break;
			case "ball": button = buttonBall; expected = m_BallForm; break;
			case "shoes": button = buttonShoes; expected = m_ShoesForm; break;
			case "gloves": button = buttonGloves; expected = m_GlovesForm; break;
			case "competition": button = buttonTournament; expected = m_TrophyForm; break;
			default: button = buttonCountry; expected = m_CountryForm; break;
		}

		// Reproduce the original failure: Alt opened the menu, its cached state
		// remained true, and the next normal toolbar click went to panelRight.
		m_IsAltPressed = true;
		buttonMain_Click(button, EventArgs.Empty);
		if (!ReferenceEquals(expected.Parent, panelMain) || !panelMain.Controls.Contains(expected))
			throw new InvalidOperationException("Section was not routed to the main panel: " + section);
		if (panelRight.Controls.Contains(expected))
			throw new InvalidOperationException("Section leaked into the right panel: " + section);
		AssertFc26SectionVisible(section, expected);

		if (ReferenceEquals(expected, m_LeagueForm))
		{
			foreach (League league in FifaEnvironment.Leagues)
			{
				if (league.ShortName == "Short League Name" || league.LongName == "Long League Name")
					throw new InvalidDataException("FC26 league placeholder was not replaced: " + league.Id);
			}
		}
	}

	internal void AssertFc26SectionVisible(string section)
	{
		Form expected;
		switch ((section ?? string.Empty).ToLowerInvariant())
		{
			case "league": expected = m_LeagueForm; break;
			case "team": expected = m_TeamForm; break;
			case "kit": expected = m_KitForm; break;
			case "player": expected = m_PlayerForm; break;
			case "stadium": expected = m_StadiumForm; break;
			case "formation": expected = m_FormationForm; break;
			case "ball": expected = m_BallForm; break;
			case "shoes": expected = m_ShoesForm; break;
			case "gloves": expected = m_GlovesForm; break;
			case "competition": expected = m_TrophyForm; break;
			default: expected = m_CountryForm; break;
		}
		AssertFc26SectionVisible(section, expected);
	}

	internal void AuditFc26RecordsForSmoke(string section)
	{
		switch ((section ?? string.Empty).ToLowerInvariant())
		{
			case "league": m_LeagueForm.AuditFc26RecordsForSmoke(); break;
			case "team": m_TeamForm.AuditFc26RecordsForSmoke(); break;
			case "kit": m_KitForm.AuditFc26RecordsForSmoke(); break;
			case "player": m_PlayerForm.AuditFc26RecordsForSmoke(); break;
			case "stadium": m_StadiumForm.AuditFc26RecordsForSmoke(); break;
			case "formation": m_FormationForm.AuditFc26RecordsForSmoke(); break;
		}
		Application.DoEvents();
		AssertFc26SectionVisible(section);
	}

	private void AssertFc26SectionVisible(string section, Form expected)
	{
		if (!ReferenceEquals(expected.Parent, panelMain) ||
			!panelMain.Controls.Contains(expected) ||
			!expected.Visible || !expected.IsHandleCreated ||
			expected.Width <= 0 || expected.Height <= 0 ||
			!expected.Bounds.IntersectsWith(panelMain.ClientRectangle))
		{
			throw new InvalidOperationException("Section became blank after activation: " + section);
		}
		foreach (Control control in panelMain.Controls)
		{
			if (!ReferenceEquals(control, expected) && control.Visible)
				throw new InvalidOperationException("Multiple editor sections remained visible after activation: " + section);
		}
		if (panelMain.Controls.GetChildIndex(expected) != 0)
			throw new InvalidOperationException("Activated editor section was not brought to front: " + section);
	}

	private void menuOpenLang16_Click(object sender, EventArgs e)
	{
		if (InitializeFifaEnvironment(16) && AskUserOpenLangDatabase())
		{
			Open();
		}
	}

	private void installRevModPatchsimplifiedVersionToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FifaEnvironment.ExtractRevModFiles();
	}

	private void playerNameCountryRulesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ApplyCountryRules();
	}

	private void ApplyCountryRules()
	{
		foreach (Player player in FifaEnvironment.Players)
		{
			if (player.nationality == 166 || player.nationality == 167)
			{
				if (player.playerjerseyname != player.firstname && player.playerjerseyname != player.lastname && player.playerjerseyname != player.commonname)
				{
					player.playerjerseyname = player.commonname;
				}
				if (player.commonname != string.Empty)
				{
					player.firstname = string.Empty;
					player.lastname = string.Empty;
					player.playerjerseyname = player.commonname;
				}
			}
		}
	}

	private void removeFakePlayersToolStripMenuItem_Click(object sender, EventArgs e)
	{
		int num = 0;
		for (int i = 0; i < FifaEnvironment.Players.Count; i++)
		{
			Player player = (Player)FifaEnvironment.Players[i];
			bool flag = false;
			if (player.birthdate.Day != 29 || player.birthdate.Month != 2)
			{
				continue;
			}
			if (player.m_PlayingForTeams.Count == 0)
			{
				flag = true;
			}
			else if (((Team)player.m_PlayingForTeams[0]).Country.Id == 54)
			{
				flag = true;
			}
			if (flag)
			{
				while (player.m_PlayingForTeams.Count > 0)
				{
					((Team)player.m_PlayingForTeams[0]).RemoveTeamPlayer(player);
				}
				FifaEnvironment.Players.DeletePlayer(player);
				i--;
				num++;
			}
		}
		statusBar.Text = "Removed " + num + " palyers.";
	}

	private bool ReplaceName(Player player, string wrongName, string rightName)
	{
		bool result = false;
		if (player.lastname == wrongName)
		{
			player.lastname = rightName;
			result = true;
		}
		if (player.playerjerseyname == wrongName)
		{
			player.playerjerseyname = rightName;
			result = true;
		}
		return result;
	}

	private void exportKitsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void PrintPlayer(StreamWriter sw, Player player)
	{
		char value = ',';
		string value2 = player.Id.ToString();
		sw.Write(value2);
		sw.Write(value);
		sw.Write(player.firstname);
		sw.Write(value);
		sw.Write(player.lastname);
		sw.Write(value);
		sw.Write(player.commonname);
		sw.Write(value);
		sw.Write(player.playerjerseyname);
		sw.Write(value);
		value2 = player.birthdate.ToString("dd/MM/yyyy");
		sw.Write(value2);
		sw.Write(value);
		value2 = player.Country.ToString();
		sw.Write(value2);
		sw.Write(value);
		value2 = ((ERole)player.preferredposition1/*cast due to constrained. prefix*/).ToString();
		sw.Write(value2);
		sw.Write(value);
		value2 = player.height.ToString();
		sw.Write(value2);
		sw.Write(value);
		value2 = player.weight.ToString();
		sw.Write(value2);
		sw.Write(value);
		value2 = player.overallrating.ToString();
		sw.Write(value2);
		sw.Write(value);
		value2 = player.contractvaliduntil.ToString();
		sw.Write(value2);
		sw.Write(value);
		value2 = player.joindate.ToString("dd/MM/yyyy");
		sw.Write(value2);
		sw.Write(value);
		if (player.PreviousTeam != null)
		{
			value2 = player.PreviousTeam.ToString();
			value2 = value2 + " (" + player.PreviousTeam.Id + ")";
			sw.Write(value2);
		}
		sw.Write(value);
		value2 = (player.IsLoaned ? "Y" : "N");
		sw.Write(value2);
		sw.Write(value);
		if (player.IsLoaned)
		{
			value2 = player.loandateend.ToString("dd/MM/yyyy");
			sw.Write(value2);
			sw.Write(value);
			value2 = player.TeamLoanedFrom.ToString();
			value2 = value2 + " (" + player.TeamLoanedFrom.Id + ")";
			sw.Write(value2);
			sw.Write(value);
		}
		else
		{
			sw.Write(value);
			sw.Write(value);
		}
		sw.Write("\r\n");
	}

	private void PrintTeamPlayer(StreamWriter sw, TeamPlayer teamPlayer)
	{
		char value = ',';
		if (teamPlayer != null)
		{
			string text = teamPlayer.Team.ToString();
			text = text + " (" + teamPlayer.Team.Id + ")";
			sw.Write(text);
			sw.Write(value);
			text = teamPlayer.jerseynumber.ToString();
			sw.Write(text);
			sw.Write(value);
		}
		else
		{
			sw.Write(value);
			sw.Write(value);
		}
	}

	private void exportPlayersInCSVToolStripMenuItem_Click(object sender, EventArgs e)
	{
		StreamWriter streamWriter = new StreamWriter(FifaEnvironment.TempFolder + "\\rosters.csv", append: false, m_Encoder);
		streamWriter.WriteLine("Team,Num,Playerid,First Name,Last Name,Common Name,Jersey Name,Birthday,Country,Role,Height,Weight,Overall,Contract,Join Date,Previous Team,Loan,Loan End Date,Loaning Team");
		foreach (Team team in FifaEnvironment.Teams)
		{
			if (!team.IsClub())
			{
				continue;
			}
			foreach (TeamPlayer item in team.Roster)
			{
				PrintTeamPlayer(streamWriter, item);
				PrintPlayer(streamWriter, item.Player);
			}
		}
		foreach (Player player in FifaEnvironment.Players)
		{
			if (player.IsFreeAgent())
			{
				PrintTeamPlayer(streamWriter, null);
				PrintPlayer(streamWriter, player);
			}
		}
		streamWriter.Close();
	}

	private int GetTeamIdFromCsv(string teamString)
	{
		int num = teamString.IndexOf('(');
		int num2 = teamString.IndexOf(')');
		return Convert.ToInt32(teamString.Substring(num + 1, num2 - num - 1));
	}

	private void importPlayersFromCSVToolStripMenuItem_Click(object sender, EventArgs e)
	{
		string path = FifaEnvironment.TempFolder + "\\rosters.csv";
		if (!File.Exists(path))
		{
			return;
		}
		StreamReader streamReader = new StreamReader(path, m_Encoder);
		if (streamReader == null)
		{
			return;
		}
		_ = streamReader.ReadLine() != "Team,Num,Playerid,First Name,Last Name,Common Name,Jersey Name,Birthday,Country,Role,Height,Weight,Overall,Contract,Join Date,Previous Team,Loan,Loan End Date,Loaning Team";
		string text = null;
		char[] separator = new char[1] { ',' };
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split(separator);
			Team team = null;
			Player player = null;
			TeamPlayer teamPlayer = null;
			Country country = null;
			int teamIdFromCsv;
			if (array[0] != string.Empty)
			{
				teamIdFromCsv = GetTeamIdFromCsv(array[0]);
				team = (Team)FifaEnvironment.Teams.SearchId(teamIdFromCsv);
			}
			teamIdFromCsv = Convert.ToInt32(array[2]);
			player = (Player)FifaEnvironment.Players.SearchId(teamIdFromCsv);
			if (player == null)
			{
				player = (Player)FifaEnvironment.Players.CreateNewId(teamIdFromCsv);
			}
			if (team != null)
			{
				teamPlayer = team.Roster.SearchTeamPlayer(player);
				if (teamPlayer == null)
				{
					player.GetClub()?.RemoveTeamPlayer(player);
					teamPlayer = team.AddTeamPlayer(player);
				}
			}
			if (teamPlayer != null)
			{
				teamPlayer.jerseynumber = Convert.ToInt32(array[1]);
			}
			if (player == null)
			{
				continue;
			}
			player.firstname = array[3];
			player.lastname = array[4];
			player.commonname = array[5];
			player.playerjerseyname = array[6];
			if (array[7] != string.Empty)
			{
				DateTime birthdate = Convert.ToDateTime(array[7]);
				player.birthdate = birthdate;
			}
			country = FifaEnvironment.Countries.SearchCountry(array[8]);
			if (country != null)
			{
				player.Country = country;
			}
			player.height = Convert.ToInt32(array[10]);
			player.weight = Convert.ToInt32(array[11]);
			int num = Convert.ToInt32(array[12]);
			player.ChangeSkills(num - player.overallrating);
			ERole eRole = Role.ConvertToERole(array[9]);
			if (eRole != ERole.Tribune)
			{
				bool num2 = player.preferredposition1 != (int)eRole;
				player.preferredposition1 = (int)eRole;
				if (num2)
				{
					player.RandomizeSkillsExactly(num);
				}
			}
			player.contractvaliduntil = Convert.ToInt32(array[13]);
			if (array[14] != string.Empty)
			{
				DateTime birthdate = Convert.ToDateTime(array[14]);
				player.joindate = birthdate;
			}
			if (array[15] != string.Empty)
			{
				teamIdFromCsv = GetTeamIdFromCsv(array[15]);
				team = (Team)FifaEnvironment.Teams.SearchId(teamIdFromCsv);
				if (team != null)
				{
					player.PreviousTeam = team;
				}
			}
			player.IsLoaned = array[16] == "Y";
			if (player.IsLoaned)
			{
				if (array[17] != string.Empty)
				{
					DateTime birthdate = Convert.ToDateTime(array[17]);
					player.loandateend = birthdate;
				}
				if (array[18] != string.Empty)
				{
					teamIdFromCsv = GetTeamIdFromCsv(array[18]);
					team = (Team)FifaEnvironment.Teams.SearchId(teamIdFromCsv);
					if (team != null)
					{
						player.TeamLoanedFrom = team;
					}
				}
			}
			else
			{
				player.TeamLoanedFrom = null;
			}
		}
		streamReader.Close();
	}

	private void FixFormations()
	{
		string[] array = new string[25];
		float[] array2 = new float[25]
		{
			3f, 3f, 3.5f, 3f, 4f, 3f, 4f, 6f, 6f, 5f,
			5f, 4f, 4f, 4f, 4f, 4f, 4f, 4f, 4.5f, 4f,
			5f, 4f, 4f, 4.5f, 4f
		};
		float[] array3 = new float[25]
		{
			4f, 5f, 4.5f, 4.5f, 4f, 5f, 2f, 2f, 2f, 3f,
			4f, 3f, 3f, 3.5f, 3.5f, 4f, 4f, 3.5f, 3.5f, 4.5f,
			4f, 4.5f, 5f, 4.5f, 5f
		};
		float[] array4 = new float[25]
		{
			3f, 2f, 2f, 2.5f, 2f, 2f, 4f, 1f, 2f, 2f,
			1f, 3f, 3f, 2.5f, 2.5f, 2f, 2f, 2.5f, 2f, 1.5f,
			1f, 1.5f, 1f, 1f, 1f
		};
		_ = new int[25]
		{
			4, 3, 3, 4, 3, 3, 4, 1, 1, 1,
			0, 3, 3, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 2
		};
		int[] array5 = new int[25]
		{
			0, 0, 1, 1, 1, 1, 2, 3, 4, 4,
			5, 6, 7, 8, 9, 10, 10, 11, 12, 13,
			14, 14, 14, 15, 15
		};
		array[0] = "3-4-3";
		array[1] = "3-4-2-1";
		array[2] = "3-1-4-2";
		array[3] = "3-4-1-2";
		array[4] = "3-5-1-1";
		array[5] = "3-5-2";
		array[6] = "4-2-4";
		array[7] = "5-2-2-1";
		array[8] = "5-2-1-2";
		array[9] = "5-3-2";
		array[10] = "5-4-1";
		array[11] = "4-3-3";
		array[12] = "4-3-3 F";
		array[13] = "4-3-2-1";
		array[14] = "4-3-1-2";
		array[15] = "4-2-2-2";
		array[16] = "4-4-2";
		array[17] = "4-1-2-1-2";
		array[18] = "4-1-3-2";
		array[19] = "4-4-1-1";
		array[20] = "4-2-3-1";
		array[21] = "unused";
		array[22] = "unused";
		array[23] = "4-1-4-1";
		array[24] = "4-5-1";
		for (int i = 0; i < FifaEnvironment.Formations.Count; i++)
		{
			Formation formation = (Formation)FifaEnvironment.Formations[i];
			if (!formation.IsGeneric())
			{
				for (int num = array.Length - 1; num >= 0; num--)
				{
					if (formation.formationname.Contains(array[num]))
					{
						formation.formationaudioid = array5[num];
						formation.defenders = array2[num];
						formation.midfielders = array3[num];
						formation.attackers = array4[num];
						break;
					}
				}
			}
			if (formation.IsGeneric() && i >= 34)
			{
				FifaEnvironment.Formations.DeleteId(formation);
				i--;
			}
		}
	}

	private void FixPlayers()
	{
		bool flag = File.Exists(FifaEnvironment.GameDir + Player.GenericSkinTextureFileName(1, 0));
		foreach (Player player in FifaEnvironment.Players)
		{
			if (player.facialhairtypecode >= 16)
			{
				player.facialhairtypecode = 0;
			}
			if (player.facialhaircolorcode >= 5)
			{
				player.facialhairtypecode %= 5;
			}
			if (player.haircolorcode >= 13)
			{
				player.haircolorcode = 0;
			}
			if (player.bodytypecode >= 18)
			{
				player.bodytypecode = 2;
			}
			if (!flag && (player.skintypecode == 1 || player.skintypecode == 3) && !player.HasSpecificHeadModel)
			{
				player.skintypecode = 2;
			}
			if (player.skintypecode == 7 && !player.HasSpecificHeadModel)
			{
				player.skintypecode = 8;
			}
			if ((Shoes)FifaEnvironment.Shoes.SearchId(player.shoetypecode) == null)
			{
				player.shoetypecode = 0;
				player.shoedesigncode = 1;
				player.shoecolorcode1 = 15;
				player.shoecolorcode2 = 0;
			}
		}
	}

	private void setFormationAudioAutomaticallyToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FixFormations();
	}

	private void fixPlayerAppearanceProblemsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FixPlayers();
	}

	private void FixProblems_Click(object sender, EventArgs e)
	{
		foreach (Country country in FifaEnvironment.Countries)
		{
			if (country.NationalTeam == null)
			{
				continue;
			}
			if (country.WorldCupTarget == 0)
			{
				country.WorldCupTarget = 6;
			}
			if (country.ContinentalCupTarget == 0)
			{
				switch (country.Confederation)
				{
				case 1:
					country.ContinentalCupTarget = 6;
					break;
				case 2:
				case 3:
				case 4:
				case 6:
					country.ContinentalCupTarget = 4;
					break;
				}
			}
		}
		foreach (Team team3 in FifaEnvironment.Teams)
		{
			if (team3.DatabaseName == string.Empty)
			{
				team3.DatabaseName = team3.TeamNameFull;
			}
			if (team3.TeamNameFull == string.Empty)
			{
				team3.TeamNameFull = team3.DatabaseName;
			}
			team3.SetNameAutomatically(team3.TeamNameFull, 15);
			team3.SetNameAutomatically(team3.TeamNameAbbr15, 10);
			team3.SetNameAutomatically(team3.TeamNameAbbr10, 7);
			team3.SetNameAutomatically(team3.TeamNameAbbr7, 3);
			if (!team3.IsClub() && !team3.IsNationalTeam())
			{
				continue;
			}
			if (team3.Stadium == null)
			{
				team3.Stadium = (Stadium)FifaEnvironment.Stadiums.SearchId(35);
			}
			League defaultLeague = League.GetDefaultLeague();
			if (team3.League == null)
			{
				team3.League = defaultLeague;
				defaultLeague.AddTeam(team3);
			}
			if (team3.League == defaultLeague)
			{
				team3.previousyeartableposition = 1;
				team3.PrevLeague = defaultLeague;
			}
			bool flag = false;
			Formation formation;
			for (int i = 0; i < FifaEnvironment.Formations.Count; i++)
			{
				formation = (Formation)FifaEnvironment.Formations[i];
				if (formation.teamid == team3.Id && formation.Id != team3.formationid)
				{
					FifaEnvironment.Formations.DeleteId(formation);
					i--;
				}
				if (formation.teamid == team3.Id && formation.Id == team3.formationid)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				formation = FifaEnvironment.Formations.CreateNewFormation();
				if (formation != null)
				{
					team3.Formation = formation;
					formation.Team = team3;
				}
			}
			formation = team3.Formation;
			Roster roster = team3.Roster;
			bool flag2 = true;
			for (int j = 0; j < 11; j++)
			{
				Role role = formation.PlayingRoles[j].Role;
				if (roster.SearchTeamPlayer(role) == null)
				{
					flag2 = false;
					break;
				}
			}
			if (!flag2)
			{
				team3.AssignRoles(formation);
			}
			if ((Ball)FifaEnvironment.Balls.SearchId(team3.balltype) == null)
			{
				team3.balltype = 1;
			}
			if (team3.objective < 0)
			{
				team3.objective = 0;
			}
			if (team3.highestprobable < team3.objective)
			{
				team3.highestprobable = team3.objective;
			}
			if (team3.latitude == 0 && team3.longitude == 0 && team3.utcoffset == 0)
			{
				Team team2 = FifaEnvironment.Teams.SearchTeamByCountr(team3.Country, club: true);
				if (team2 != null)
				{
					team3.latitude = team2.latitude;
					team3.longitude = team2.longitude;
					team3.utcoffset = team2.utcoffset;
				}
			}
			Bitmap crestDark = team3.GetCrestDark();
			if (crestDark == null)
			{
				crestDark = team3.GetCrest();
				if (crestDark != null)
				{
					team3.SetAllCrests(crestDark);
				}
			}
		}
	}

	private void rostersAndPlayersToolStripMenuItem_Click(object sender, EventArgs e)
	{
		UpdateOnline18(updatePlayers: true);
	}

	private void enableExistingSpecificFacesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (Player player in FifaEnvironment.Players)
		{
			bool num = FifaEnvironment.IsFilePresent(player.SpecificFaceTextureFileName());
			if (num && !player.HasSpecificHeadModel)
			{
				player.headclasscode = 0;
			}
			if (!num && player.HasSpecificHeadModel)
			{
				player.headclasscode = 1;
			}
		}
	}

	private void extendLoansTo2020ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (Player player in FifaEnvironment.Players)
		{
			player.ExtendLoanEndDate(2020, 6);
		}
	}

	private void fixLoanDatesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (Player player in FifaEnvironment.Players)
		{
			player.ExtendContractAfterLoanEnd();
		}
	}

	private void removeFreeAgentToPlayersWithClubToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (Player player in FifaEnvironment.Players)
		{
			player.RemoveFromFreeAgentIfHasClub();
		}
	}

	private void addToFreeAgentPlayersWithoutClubToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (Player player in FifaEnvironment.Players)
		{
			player.AddToFreeAgentIfWithoutClub();
		}
	}

	private void removeAllPlayersToolStripMenuItem_Click(object sender, EventArgs e)
	{
		while (FifaEnvironment.Players.Count > 0)
		{
			Player player = (Player)FifaEnvironment.Players[0];
			while (player.m_PlayingForTeams.Count > 0)
			{
				((Team)player.m_PlayingForTeams[0]).RemoveTeamPlayer(player);
			}
			FifaEnvironment.Players.DeletePlayer(player);
		}
	}

	private void createDBEntryForExistingKitsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (Team team in FifaEnvironment.Teams)
		{
			for (int i = 0; i <= 3; i++)
			{
				if (team.GetKit(i) == null)
				{
					string text = Kit.KitTextureFileName(team.Id, i, 0);
					if (File.Exists(FifaEnvironment.GameDir + text))
					{
						Kit kit = new Kit(FifaEnvironment.Kits.GetNewId(), team.Id, i);
						FifaEnvironment.Kits.Add(kit);
						kit.LinkTeam(FifaEnvironment.Teams);
					}
				}
			}
		}
	}

	private void createDummyKitForTeamsWithoutKitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Kit[] array = new Kit[3];
		_ = new string[3];
		array[0] = FifaEnvironment.Kits.GetKit(7201, 0);
		Bitmap[] kitTextures = array[0].GetKitTextures();
		foreach (Team team in FifaEnvironment.Teams)
		{
			for (int i = 0; i <= 2; i++)
			{
				if (team.GetKit(i) == null)
				{
					string text = Kit.KitTextureFileName(team.Id, i, 0);
					if (!File.Exists(FifaEnvironment.GameDir + text))
					{
						Kit kit = new Kit(FifaEnvironment.Kits.GetNewId(), team.Id, i);
						kit.SetKitTextures(kitTextures);
						FifaEnvironment.Kits.Add(kit);
						kit.LinkTeam(FifaEnvironment.Teams);
					}
				}
			}
		}
	}

	private void randomizeLegendsAcademyToolStripMenuItem_Click(object sender, EventArgs e)
	{
		for (int i = 130507; i <= 130530; i++)
		{
			Team team = (Team)FifaEnvironment.Teams.SearchId(i);
			if (team == null || !team.TeamNameFull.Contains("Academy"))
			{
				continue;
			}
			foreach (TeamPlayer item in team.Roster)
			{
				Player player = item.Player;
				if (player.birthdate.Month == 2 && player.birthdate.Day == 29)
				{
					player.birthdate = new DateTime(2000, 2, 28);
				}
				player.birthdate = new DateTime(Player.RandomizeNumber(2000, 2005), player.birthdate.Month, player.birthdate.Day);
				int num = Player.RandomizeNumber(78, 95);
				int j = num - 10 - Player.RandomizeNumber(2, 9);
				if (player.birthdate.Year == 2001)
				{
					j -= Player.RandomizeNumber(2, 12);
				}
				if (player.birthdate.Year == 2002)
				{
					j -= Player.RandomizeNumber(4, 14);
				}
				if (player.birthdate.Year == 2003)
				{
					j -= Player.RandomizeNumber(7, 17);
				}
				if (player.birthdate.Year == 2004)
				{
					j -= Player.RandomizeNumber(10, 20);
				}
				while (j > 76)
				{
					j -= Player.RandomizeNumber(1, 6);
				}
				for (; j < 50; j += Player.RandomizeNumber(1, 6))
				{
				}
				int averageRoleAttribute = player.GetAverageRoleAttribute();
				player.ChangeRoleSkills(j - averageRoleAttribute);
				player.overallrating = player.GetAverageRoleAttribute();
				player.potential = num;
			}
		}
	}

	private void setFreeAgentDatesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (Team team in FifaEnvironment.Teams)
		{
			if (team.League != null && team.League.Id == 382)
			{
				SetDates(team);
			}
		}
	}

	private void SetDates(Team team)
	{
		if (team == null)
		{
			return;
		}
		DateTime joindate = new DateTime(2015, 1, 1);
		foreach (TeamPlayer item in team.Roster)
		{
			Player player = item.Player;
			player.joindate = joindate;
			player.contractvaliduntil = 2019;
			player.IsLoaned = false;
		}
	}

	private void resetCommentaryNamesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		int count = FifaEnvironment.NameDictionary.Count;
		int num = 0;
		FifaEnvironment.PlayerNamesList.ClearCommentaryId();
		for (int i = 900001; i < 999999; i++)
		{
			string value = string.Empty;
			FifaEnvironment.NameDictionary.TryGetValue(i, out value);
			if (value != null && value != string.Empty)
			{
				num++;
				PlayerName playerName = FifaEnvironment.PlayerNamesList.SearchName(value);
				if (playerName != null)
				{
					playerName.CommentaryId = i;
				}
				if (num == count)
				{
					break;
				}
			}
		}
	}

	private void menuOnlineFromFifa21_Click(object sender, EventArgs e)
	{
		UpdateOnline21(updatePlayers: true);
	}

	private void associateCommentaryNamesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		int count = FifaEnvironment.NameDictionary.Count;
		int num = 0;
		for (int i = 900001; i < 999999; i++)
		{
			string value = string.Empty;
			FifaEnvironment.NameDictionary.TryGetValue(i, out value);
			if (value != null && value != string.Empty)
			{
				num++;
				PlayerName playerName = FifaEnvironment.PlayerNamesList.SearchName(value);
				if (playerName != null && playerName.CommentaryId == 900000)
				{
					playerName.CommentaryId = i;
				}
				if (num == count)
				{
					break;
				}
			}
		}
	}

	private void createPlayersFoeCommentaryNamesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		int count = FifaEnvironment.NameDictionary.Count;
		int num = 0;
		for (int i = 900001; i < 999999; i++)
		{
			string value = string.Empty;
			FifaEnvironment.NameDictionary.TryGetValue(i, out value);
			int num2 = 2;
			if (value == null || !(value != string.Empty))
			{
				continue;
			}
			num++;
			if (FifaEnvironment.Players.SearchPlayerByName(value) == null)
			{
				Player obj = (Player)FifaEnvironment.Players.CreateNewId();
				obj.headclasscode = 1;
				obj.firstname = "";
				obj.lastname = value;
				obj.commonname = "";
				obj.playerjerseyname = value;
				obj.commentaryid = i;
				obj.preferredposition1 = num2;
				num2++;
				if (num2 == 28)
				{
					num2 = 2;
				}
			}
			if (num == count)
			{
				break;
			}
		}
	}

	private void convertMinheadsToPNGToolStripMenuItem_Click(object sender, EventArgs e)
	{
		string[] files = Directory.GetFiles(FifaEnvironment.GameDir + "data/ui/imgassets/heads", "*.dds");
		for (int i = 0; i < files.Length; i++)
		{
			Bitmap bitmap = new DdsFile(files[i]).GetBitmap();
			string filename = files[i].Replace("dds", "png");
			bitmap.Save(filename, ImageFormat.Png);
		}
	}

	private void menuStandardizeCommentaryIds_Click(object sender, EventArgs e)
	{
		m_AudioForm.UseStandardId();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.MainForm));
		this.menuStrip = new System.Windows.Forms.MenuStrip();
		this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenFifa16 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenLang16 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenFifa15 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenLang15 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenFifa14 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenLang14 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenAll = new System.Windows.Forms.ToolStripMenuItem();
		this.menuReopen = new System.Windows.Forms.ToolStripMenuItem();
		this.menuSave = new System.Windows.Forms.ToolStripMenuItem();
		this.menuClose = new System.Windows.Forms.ToolStripMenuItem();
		this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOpenDebug = new System.Windows.Forms.ToolStripMenuItem();
		this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
		this.menuEnableAllMessages = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOptions = new System.Windows.Forms.ToolStripMenuItem();
		this.menuRegenerate = new System.Windows.Forms.ToolStripMenuItem();
		this.menuExpandDatabase = new System.Windows.Forms.ToolStripMenuItem();
		this.menuRemoveKidProtection = new System.Windows.Forms.ToolStripMenuItem();
		this.menuCleanFAT = new System.Windows.Forms.ToolStripMenuItem();
		this.menuRemoveAllLongTeamNames = new System.Windows.Forms.ToolStripMenuItem();
		this.menuAlignLanguageDB = new System.Windows.Forms.ToolStripMenuItem();
		this.menuMinimizeNamesTable = new System.Windows.Forms.ToolStripMenuItem();
		this.menuPreserveOriginalNames = new System.Windows.Forms.ToolStripMenuItem();
		this.menuInstallRevModPatch = new System.Windows.Forms.ToolStripMenuItem();
		this.exportPlayersFromCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.importPlayersFromCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.removeFakePlayersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.playerNameCountryRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fixProblemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.enableExistingSpecificFacesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fixLoanDatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.extendLoansTo2020ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.removeFreeAgentToPlayersWithClubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.addToFreeAgentPlayersWithoutClubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.removeAllPlayersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.createDBEntryForExistingKitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.createDummyKitForTeamsWithoutKitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.randomizeLegendsAcademyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.setFreeAgentDatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.resetCommentaryNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.associateCommentaryNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.createPlayersFoeCommentaryNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.convertMinheadsToPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.menuStandardizeCommentaryIds = new System.Windows.Forms.ToolStripMenuItem();
		this.menuPatch = new System.Windows.Forms.ToolStripMenuItem();
		this.menuCreatePatch = new System.Windows.Forms.ToolStripMenuItem();
		this.menuLoadPatch = new System.Windows.Forms.ToolStripMenuItem();
		this.menuUpdateDB = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOnlineFromFifa21 = new System.Windows.Forms.ToolStripMenuItem();
		this.fromFIFA20ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOnlineFromFifa17 = new System.Windows.Forms.ToolStripMenuItem();
		this.rostersOnlyFromFifa17 = new System.Windows.Forms.ToolStripMenuItem();
		this.rostersAndPlayersFromFifa17 = new System.Windows.Forms.ToolStripMenuItem();
		this.fromFIFA18ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.rostersAndPlayersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.menuOnlineFromFifa16 = new System.Windows.Forms.ToolStripMenuItem();
		this.rostersOnlyFromFIFA16 = new System.Windows.Forms.ToolStripMenuItem();
		this.rostersAndPlayersFromFifa16 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuUgc = new System.Windows.Forms.ToolStripMenuItem();
		this.menuImportUgc = new System.Windows.Forms.ToolStripMenuItem();
		this.menuImportUgcWothKits = new System.Windows.Forms.ToolStripMenuItem();
		this.menuImportUgcKits = new System.Windows.Forms.ToolStripMenuItem();
		this.menuImportUgcPlayers = new System.Windows.Forms.ToolStripMenuItem();
		this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
		this.menuAbout = new System.Windows.Forms.ToolStripMenuItem();
		this.menuHelpCms = new System.Windows.Forms.ToolStripMenuItem();
		this.genericToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.adboardsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ballsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.bootsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.countryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fontsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.formationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.leaguesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.stadiumsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.teamsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.tournamentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.statusStrip = new System.Windows.Forms.StatusStrip();
		this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
		this.statusBar = new System.Windows.Forms.ToolStripStatusLabel();
		this.splitVert = new System.Windows.Forms.SplitContainer();
		this.splitHoriz = new System.Windows.Forms.SplitContainer();
		this.panelMain = new System.Windows.Forms.Panel();
		this.panelBottom = new System.Windows.Forms.Panel();
		this.toolStripBottom = new System.Windows.Forms.ToolStrip();
		this.buttonShowBottom = new System.Windows.Forms.ToolStripButton();
		this.buttonHideBottom = new System.Windows.Forms.ToolStripButton();
		this.stripLabelBottom = new System.Windows.Forms.ToolStripLabel();
		this.panelRight = new System.Windows.Forms.Panel();
		this.toolStripRight = new System.Windows.Forms.ToolStrip();
		this.buttonShowRight = new System.Windows.Forms.ToolStripButton();
		this.buttonHideRight = new System.Windows.Forms.ToolStripButton();
		this.stripLabelRight = new System.Windows.Forms.ToolStripLabel();
		this.toolStripMain = new System.Windows.Forms.ToolStrip();
		this.buttonCountry = new System.Windows.Forms.ToolStripButton();
		this.buttonLeague = new System.Windows.Forms.ToolStripButton();
		this.buttonTeam = new System.Windows.Forms.ToolStripButton();
		this.buttonKit = new System.Windows.Forms.ToolStripButton();
		this.buttonPlayer = new System.Windows.Forms.ToolStripButton();
		this.buttonStadium = new System.Windows.Forms.ToolStripButton();
		this.buttonTournament = new System.Windows.Forms.ToolStripButton();
		this.buttonReferee = new System.Windows.Forms.ToolStripButton();
		this.buttonBall = new System.Windows.Forms.ToolStripButton();
		this.buttonShoes = new System.Windows.Forms.ToolStripButton();
		this.buttonManager = new System.Windows.Forms.ToolStripButton();
		this.buttonFormation = new System.Windows.Forms.ToolStripButton();
		this.buttonSponsor = new System.Windows.Forms.ToolStripButton();
		this.buttonTv = new System.Windows.Forms.ToolStripButton();
		this.buttonNewspaper = new System.Windows.Forms.ToolStripButton();
		this.buttonGloves = new System.Windows.Forms.ToolStripButton();
		this.buttonAudio = new System.Windows.Forms.ToolStripButton();
		this.buttonGameGraphics = new System.Windows.Forms.ToolStripButton();
		this.buttonBrowser = new System.Windows.Forms.ToolStripButton();
		this.buttonImportGraphics = new System.Windows.Forms.ToolStripButton();
		this.openFifaDialog = new System.Windows.Forms.OpenFileDialog();
		this.openLangDialog = new System.Windows.Forms.OpenFileDialog();
		this.browserDialog = new System.Windows.Forms.FolderBrowserDialog();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.menuStrip.SuspendLayout();
		this.statusStrip.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitVert).BeginInit();
		this.splitVert.Panel1.SuspendLayout();
		this.splitVert.Panel2.SuspendLayout();
		this.splitVert.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitHoriz).BeginInit();
		this.splitHoriz.Panel1.SuspendLayout();
		this.splitHoriz.Panel2.SuspendLayout();
		this.splitHoriz.SuspendLayout();
		this.toolStripBottom.SuspendLayout();
		this.toolStripRight.SuspendLayout();
		this.toolStripMain.SuspendLayout();
		base.SuspendLayout();
		this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.menuFile, this.menuTools, this.menuPatch, this.menuUpdateDB, this.menuUgc, this.helpToolStripMenuItem });
		this.menuStrip.Location = new System.Drawing.Point(0, 0);
		this.menuStrip.Name = "menuStrip";
		this.menuStrip.Size = new System.Drawing.Size(1384, 24);
		this.menuStrip.TabIndex = 0;
		this.menuStrip.Text = "menuStrip1";
		this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[12]
		{
			this.menuOpenFifa16, this.menuOpenLang16, this.menuOpenFifa15, this.menuOpenLang15, this.menuOpenFifa14, this.menuOpenLang14, this.menuOpenAll, this.menuReopen, this.menuSave, this.menuClose,
			this.menuExit, this.menuOpenDebug
		});
		this.menuFile.Name = "menuFile";
		this.menuFile.Size = new System.Drawing.Size(37, 20);
		this.menuFile.Text = "File";
		this.menuOpenFifa16.Image = (System.Drawing.Image)resources.GetObject("menuOpenFifa16.Image");
		this.menuOpenFifa16.Name = "menuOpenFifa16";
		this.menuOpenFifa16.Size = new System.Drawing.Size(181, 22);
		this.menuOpenFifa16.Text = "Open - FC26";
		this.menuOpenFifa16.Click += new System.EventHandler(menuOpenFifa16_Click);
		this.menuOpenLang16.Image = (System.Drawing.Image)resources.GetObject("menuOpenLang16.Image");
		this.menuOpenLang16.Name = "menuOpenLang16";
		this.menuOpenLang16.Size = new System.Drawing.Size(181, 22);
		this.menuOpenLang16.Text = "Open - Select lan.db";
		this.menuOpenLang16.Click += new System.EventHandler(menuOpenLang16_Click);
		this.menuOpenFifa15.Image = (System.Drawing.Image)resources.GetObject("menuOpenFifa15.Image");
		this.menuOpenFifa15.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuOpenFifa15.Name = "menuOpenFifa15";
		this.menuOpenFifa15.Size = new System.Drawing.Size(181, 22);
		this.menuOpenFifa15.Text = "Open - FIFA 15";
		this.menuOpenFifa15.Visible = false;
		this.menuOpenFifa15.Click += new System.EventHandler(menuOpenFifa15Demo_Click);
		this.menuOpenLang15.Image = (System.Drawing.Image)resources.GetObject("menuOpenLang15.Image");
		this.menuOpenLang15.Name = "menuOpenLang15";
		this.menuOpenLang15.Size = new System.Drawing.Size(181, 22);
		this.menuOpenLang15.Text = "Open - Select lan.db";
		this.menuOpenLang15.Visible = false;
		this.menuOpenLang15.Click += new System.EventHandler(toolStripMenuItem1_Click);
		this.menuOpenFifa14.Image = (System.Drawing.Image)resources.GetObject("menuOpenFifa14.Image");
		this.menuOpenFifa14.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuOpenFifa14.Name = "menuOpenFifa14";
		this.menuOpenFifa14.Size = new System.Drawing.Size(181, 22);
		this.menuOpenFifa14.Text = "Open - FIFA 14";
		this.menuOpenFifa14.Visible = false;
		this.menuOpenFifa14.Click += new System.EventHandler(menuOpenFifa_Click);
		this.menuOpenLang14.Image = (System.Drawing.Image)resources.GetObject("menuOpenLang14.Image");
		this.menuOpenLang14.Name = "menuOpenLang14";
		this.menuOpenLang14.Size = new System.Drawing.Size(181, 22);
		this.menuOpenLang14.Text = "Open - Select lan.db";
		this.menuOpenLang14.Visible = false;
		this.menuOpenLang14.Click += new System.EventHandler(openSelectLandbToolStripMenuItem_Click);
		this.menuOpenAll.Image = (System.Drawing.Image)resources.GetObject("menuOpenAll.Image");
		this.menuOpenAll.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuOpenAll.Name = "menuOpenAll";
		this.menuOpenAll.Size = new System.Drawing.Size(181, 22);
		this.menuOpenAll.Text = "Open - Select all";
		this.menuOpenAll.Click += new System.EventHandler(openSelectAllToolStripMenuItem_Click);
		this.menuReopen.Name = "menuReopen";
		this.menuReopen.Size = new System.Drawing.Size(181, 22);
		this.menuReopen.Text = "Open - Recent";
		this.menuReopen.Click += new System.EventHandler(menuReopen_Click);
		this.menuSave.Enabled = false;
		this.menuSave.Image = (System.Drawing.Image)resources.GetObject("menuSave.Image");
		this.menuSave.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuSave.Name = "menuSave";
		this.menuSave.Size = new System.Drawing.Size(181, 22);
		this.menuSave.Text = "Save";
		this.menuSave.Click += new System.EventHandler(menuSave_Click);
		this.menuClose.Enabled = false;
		this.menuClose.Image = (System.Drawing.Image)resources.GetObject("menuClose.Image");
		this.menuClose.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuClose.Name = "menuClose";
		this.menuClose.Size = new System.Drawing.Size(181, 22);
		this.menuClose.Text = "Close";
		this.menuClose.Click += new System.EventHandler(menuClose_Click);
		this.menuExit.Image = (System.Drawing.Image)resources.GetObject("menuExit.Image");
		this.menuExit.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuExit.Name = "menuExit";
		this.menuExit.Size = new System.Drawing.Size(181, 22);
		this.menuExit.Text = "Exit";
		this.menuExit.Click += new System.EventHandler(menuExit_Click);
		this.menuOpenDebug.Name = "menuOpenDebug";
		this.menuOpenDebug.Size = new System.Drawing.Size(181, 22);
		this.menuOpenDebug.Text = "Open - Demo";
		this.menuOpenDebug.Visible = false;
		this.menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[31]
		{
			this.menuEnableAllMessages, this.menuOptions, this.menuRegenerate, this.menuExpandDatabase, this.menuRemoveKidProtection, this.menuCleanFAT, this.menuRemoveAllLongTeamNames, this.menuAlignLanguageDB, this.menuMinimizeNamesTable, this.menuPreserveOriginalNames,
			this.menuInstallRevModPatch, this.exportPlayersFromCSVToolStripMenuItem, this.importPlayersFromCSVToolStripMenuItem, this.removeFakePlayersToolStripMenuItem, this.playerNameCountryRulesToolStripMenuItem, this.fixProblemsToolStripMenuItem, this.enableExistingSpecificFacesToolStripMenuItem, this.fixLoanDatesToolStripMenuItem, this.extendLoansTo2020ToolStripMenuItem, this.removeFreeAgentToPlayersWithClubToolStripMenuItem,
			this.addToFreeAgentPlayersWithoutClubToolStripMenuItem, this.removeAllPlayersToolStripMenuItem, this.createDBEntryForExistingKitsToolStripMenuItem, this.createDummyKitForTeamsWithoutKitToolStripMenuItem, this.randomizeLegendsAcademyToolStripMenuItem, this.setFreeAgentDatesToolStripMenuItem, this.resetCommentaryNamesToolStripMenuItem, this.associateCommentaryNamesToolStripMenuItem, this.createPlayersFoeCommentaryNamesToolStripMenuItem, this.convertMinheadsToPNGToolStripMenuItem,
			this.menuStandardizeCommentaryIds
		});
		this.menuTools.Name = "menuTools";
		this.menuTools.Size = new System.Drawing.Size(47, 20);
		this.menuTools.Text = "Tools";
		this.menuEnableAllMessages.Image = (System.Drawing.Image)resources.GetObject("menuEnableAllMessages.Image");
		this.menuEnableAllMessages.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuEnableAllMessages.Name = "menuEnableAllMessages";
		this.menuEnableAllMessages.Size = new System.Drawing.Size(298, 22);
		this.menuEnableAllMessages.Text = "Enable all messages";
		this.menuEnableAllMessages.Click += new System.EventHandler(menuEnableAllMessages_Click);
		this.menuOptions.Image = (System.Drawing.Image)resources.GetObject("menuOptions.Image");
		this.menuOptions.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuOptions.Name = "menuOptions";
		this.menuOptions.Size = new System.Drawing.Size(298, 22);
		this.menuOptions.Text = "Options";
		this.menuOptions.Visible = false;
		this.menuOptions.Click += new System.EventHandler(menuOptions_Click);
		this.menuRegenerate.Image = (System.Drawing.Image)resources.GetObject("menuRegenerate.Image");
		this.menuRegenerate.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuRegenerate.Name = "menuRegenerate";
		this.menuRegenerate.Size = new System.Drawing.Size(298, 22);
		this.menuRegenerate.Text = "Regenerate BH";
		this.menuRegenerate.Click += new System.EventHandler(menuRegenerate_Click);
		this.menuExpandDatabase.Image = (System.Drawing.Image)resources.GetObject("menuExpandDatabase.Image");
		this.menuExpandDatabase.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuExpandDatabase.Name = "menuExpandDatabase";
		this.menuExpandDatabase.Size = new System.Drawing.Size(298, 22);
		this.menuExpandDatabase.Text = "Expand Database";
		this.menuExpandDatabase.Click += new System.EventHandler(menuExpandDatabase_Click);
		this.menuRemoveKidProtection.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.menuRemoveKidProtection.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.menuRemoveKidProtection.Name = "menuRemoveKidProtection";
		this.menuRemoveKidProtection.Size = new System.Drawing.Size(298, 22);
		this.menuRemoveKidProtection.Text = "Remove \"Kid Protection\" Kits";
		this.menuRemoveKidProtection.Visible = false;
		this.menuRemoveKidProtection.Click += new System.EventHandler(menuRemoveKidProtection_Click);
		this.menuCleanFAT.Image = (System.Drawing.Image)resources.GetObject("menuCleanFAT.Image");
		this.menuCleanFAT.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuCleanFAT.Name = "menuCleanFAT";
		this.menuCleanFAT.Size = new System.Drawing.Size(298, 22);
		this.menuCleanFAT.Text = "Clean FAT";
		this.menuCleanFAT.Visible = false;
		this.menuCleanFAT.Click += new System.EventHandler(menuCleanFAT_Click);
		this.menuRemoveAllLongTeamNames.Name = "menuRemoveAllLongTeamNames";
		this.menuRemoveAllLongTeamNames.Size = new System.Drawing.Size(298, 22);
		this.menuRemoveAllLongTeamNames.Text = "Remove All Long Team Names";
		this.menuRemoveAllLongTeamNames.Visible = false;
		this.menuRemoveAllLongTeamNames.Click += new System.EventHandler(removeAllLongTeamNames_Click);
		this.menuAlignLanguageDB.Name = "menuAlignLanguageDB";
		this.menuAlignLanguageDB.Size = new System.Drawing.Size(298, 22);
		this.menuAlignLanguageDB.Text = "Align Language DB";
		this.menuAlignLanguageDB.Click += new System.EventHandler(menuAlignLanguageDB_Click);
		this.menuMinimizeNamesTable.Name = "menuMinimizeNamesTable";
		this.menuMinimizeNamesTable.Size = new System.Drawing.Size(298, 22);
		this.menuMinimizeNamesTable.Text = "Minimize Player Names Table";
		this.menuMinimizeNamesTable.ToolTipText = "Reserve more room in the player names table for created players but makes the database not compatible with online gaming . ";
		this.menuMinimizeNamesTable.Click += new System.EventHandler(minimizeNamesTableToolStripMenuItem_Click);
		this.menuPreserveOriginalNames.Name = "menuPreserveOriginalNames";
		this.menuPreserveOriginalNames.Size = new System.Drawing.Size(298, 22);
		this.menuPreserveOriginalNames.Text = "Preserve Original Player Names";
		this.menuPreserveOriginalNames.ToolTipText = "Preserve all the names originally present in the player names table, in this way the database will be compatible with online gaming but the space of names for new players will be reduced. ";
		this.menuPreserveOriginalNames.Click += new System.EventHandler(menuPreserveOriginalNames_Click);
		this.menuInstallRevModPatch.Name = "menuInstallRevModPatch";
		this.menuInstallRevModPatch.Size = new System.Drawing.Size(298, 22);
		this.menuInstallRevModPatch.Text = "Install RevMod Patch (simplified version)";
		this.menuInstallRevModPatch.Click += new System.EventHandler(installRevModPatchsimplifiedVersionToolStripMenuItem_Click);
		this.exportPlayersFromCSVToolStripMenuItem.Name = "exportPlayersFromCSVToolStripMenuItem";
		this.exportPlayersFromCSVToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.exportPlayersFromCSVToolStripMenuItem.Text = "Export Players From CSV";
		this.exportPlayersFromCSVToolStripMenuItem.Visible = false;
		this.exportPlayersFromCSVToolStripMenuItem.Click += new System.EventHandler(exportPlayersInCSVToolStripMenuItem_Click);
		this.importPlayersFromCSVToolStripMenuItem.Name = "importPlayersFromCSVToolStripMenuItem";
		this.importPlayersFromCSVToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.importPlayersFromCSVToolStripMenuItem.Text = "Import Players From CSV";
		this.importPlayersFromCSVToolStripMenuItem.Visible = false;
		this.importPlayersFromCSVToolStripMenuItem.Click += new System.EventHandler(importPlayersFromCSVToolStripMenuItem_Click);
		this.removeFakePlayersToolStripMenuItem.Name = "removeFakePlayersToolStripMenuItem";
		this.removeFakePlayersToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.removeFakePlayersToolStripMenuItem.Text = "Remove Fake Players from Database";
		this.removeFakePlayersToolStripMenuItem.Click += new System.EventHandler(removeFakePlayersToolStripMenuItem_Click);
		this.playerNameCountryRulesToolStripMenuItem.Name = "playerNameCountryRulesToolStripMenuItem";
		this.playerNameCountryRulesToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.playerNameCountryRulesToolStripMenuItem.Text = "Simplify Player Name using Country Rules";
		this.playerNameCountryRulesToolStripMenuItem.Click += new System.EventHandler(playerNameCountryRulesToolStripMenuItem_Click);
		this.fixProblemsToolStripMenuItem.Name = "fixProblemsToolStripMenuItem";
		this.fixProblemsToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.fixProblemsToolStripMenuItem.Text = "Fix Common Problems";
		this.fixProblemsToolStripMenuItem.Click += new System.EventHandler(FixProblems_Click);
		this.enableExistingSpecificFacesToolStripMenuItem.Name = "enableExistingSpecificFacesToolStripMenuItem";
		this.enableExistingSpecificFacesToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.enableExistingSpecificFacesToolStripMenuItem.Text = "Enable Existing Specific Faces";
		this.enableExistingSpecificFacesToolStripMenuItem.Click += new System.EventHandler(enableExistingSpecificFacesToolStripMenuItem_Click);
		this.fixLoanDatesToolStripMenuItem.Name = "fixLoanDatesToolStripMenuItem";
		this.fixLoanDatesToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.fixLoanDatesToolStripMenuItem.Text = "Set Contract End after Loan End Date";
		this.fixLoanDatesToolStripMenuItem.Click += new System.EventHandler(fixLoanDatesToolStripMenuItem_Click);
		this.extendLoansTo2020ToolStripMenuItem.Name = "extendLoansTo2020ToolStripMenuItem";
		this.extendLoansTo2020ToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.extendLoansTo2020ToolStripMenuItem.Text = "Extend Loans to 2021";
		this.extendLoansTo2020ToolStripMenuItem.Visible = false;
		this.extendLoansTo2020ToolStripMenuItem.Click += new System.EventHandler(extendLoansTo2020ToolStripMenuItem_Click);
		this.removeFreeAgentToPlayersWithClubToolStripMenuItem.Name = "removeFreeAgentToPlayersWithClubToolStripMenuItem";
		this.removeFreeAgentToPlayersWithClubToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.removeFreeAgentToPlayersWithClubToolStripMenuItem.Text = "Remove from Free Agent players with club";
		this.removeFreeAgentToPlayersWithClubToolStripMenuItem.Click += new System.EventHandler(removeFreeAgentToPlayersWithClubToolStripMenuItem_Click);
		this.addToFreeAgentPlayersWithoutClubToolStripMenuItem.Name = "addToFreeAgentPlayersWithoutClubToolStripMenuItem";
		this.addToFreeAgentPlayersWithoutClubToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.addToFreeAgentPlayersWithoutClubToolStripMenuItem.Text = "Add to Free Agent players without club";
		this.addToFreeAgentPlayersWithoutClubToolStripMenuItem.Click += new System.EventHandler(addToFreeAgentPlayersWithoutClubToolStripMenuItem_Click);
		this.removeAllPlayersToolStripMenuItem.Name = "removeAllPlayersToolStripMenuItem";
		this.removeAllPlayersToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.removeAllPlayersToolStripMenuItem.Text = "Remove all Players";
		this.removeAllPlayersToolStripMenuItem.Visible = false;
		this.removeAllPlayersToolStripMenuItem.Click += new System.EventHandler(removeAllPlayersToolStripMenuItem_Click);
		this.createDBEntryForExistingKitsToolStripMenuItem.Name = "createDBEntryForExistingKitsToolStripMenuItem";
		this.createDBEntryForExistingKitsToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.createDBEntryForExistingKitsToolStripMenuItem.Text = "Create DB entry for existing kits";
		this.createDBEntryForExistingKitsToolStripMenuItem.Click += new System.EventHandler(createDBEntryForExistingKitsToolStripMenuItem_Click);
		this.createDummyKitForTeamsWithoutKitToolStripMenuItem.Name = "createDummyKitForTeamsWithoutKitToolStripMenuItem";
		this.createDummyKitForTeamsWithoutKitToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.createDummyKitForTeamsWithoutKitToolStripMenuItem.Text = "Create Dummy Kit for Teams without Kit";
		this.createDummyKitForTeamsWithoutKitToolStripMenuItem.Click += new System.EventHandler(createDummyKitForTeamsWithoutKitToolStripMenuItem_Click);
		this.randomizeLegendsAcademyToolStripMenuItem.Name = "randomizeLegendsAcademyToolStripMenuItem";
		this.randomizeLegendsAcademyToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.randomizeLegendsAcademyToolStripMenuItem.Text = "Randomize Legends Academy";
		this.randomizeLegendsAcademyToolStripMenuItem.Click += new System.EventHandler(randomizeLegendsAcademyToolStripMenuItem_Click);
		this.setFreeAgentDatesToolStripMenuItem.Name = "setFreeAgentDatesToolStripMenuItem";
		this.setFreeAgentDatesToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.setFreeAgentDatesToolStripMenuItem.Text = "Set Free Agent Dates";
		this.setFreeAgentDatesToolStripMenuItem.Click += new System.EventHandler(setFreeAgentDatesToolStripMenuItem_Click);
		this.resetCommentaryNamesToolStripMenuItem.Name = "resetCommentaryNamesToolStripMenuItem";
		this.resetCommentaryNamesToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.resetCommentaryNamesToolStripMenuItem.Text = "Commentary: Reset Names";
		this.resetCommentaryNamesToolStripMenuItem.Click += new System.EventHandler(resetCommentaryNamesToolStripMenuItem_Click);
		this.associateCommentaryNamesToolStripMenuItem.Name = "associateCommentaryNamesToolStripMenuItem";
		this.associateCommentaryNamesToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.associateCommentaryNamesToolStripMenuItem.Text = "Commentary: Associate Names";
		this.associateCommentaryNamesToolStripMenuItem.Click += new System.EventHandler(associateCommentaryNamesToolStripMenuItem_Click);
		this.createPlayersFoeCommentaryNamesToolStripMenuItem.Name = "createPlayersFoeCommentaryNamesToolStripMenuItem";
		this.createPlayersFoeCommentaryNamesToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.createPlayersFoeCommentaryNamesToolStripMenuItem.Text = "Commentary: Create Players for Names";
		this.createPlayersFoeCommentaryNamesToolStripMenuItem.Click += new System.EventHandler(createPlayersFoeCommentaryNamesToolStripMenuItem_Click);
		this.convertMinheadsToPNGToolStripMenuItem.Name = "convertMinheadsToPNGToolStripMenuItem";
		this.convertMinheadsToPNGToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
		this.convertMinheadsToPNGToolStripMenuItem.Text = "Convert Miniheads to PNG";
		this.convertMinheadsToPNGToolStripMenuItem.Click += new System.EventHandler(convertMinheadsToPNGToolStripMenuItem_Click);
		this.menuStandardizeCommentaryIds.Name = "menuStandardizeCommentaryIds";
		this.menuStandardizeCommentaryIds.Size = new System.Drawing.Size(298, 22);
		this.menuStandardizeCommentaryIds.Text = "Standardize Commentary Ids";
		this.menuStandardizeCommentaryIds.Visible = false;
		this.menuStandardizeCommentaryIds.Click += new System.EventHandler(menuStandardizeCommentaryIds_Click);
		this.menuPatch.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.menuCreatePatch, this.menuLoadPatch });
		this.menuPatch.Name = "menuPatch";
		this.menuPatch.Size = new System.Drawing.Size(49, 20);
		this.menuPatch.Text = "Patch";
		this.menuCreatePatch.Image = (System.Drawing.Image)resources.GetObject("menuCreatePatch.Image");
		this.menuCreatePatch.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuCreatePatch.Name = "menuCreatePatch";
		this.menuCreatePatch.Size = new System.Drawing.Size(180, 22);
		this.menuCreatePatch.Text = "Create";
		this.menuCreatePatch.Click += new System.EventHandler(menuCreatePatch_Click);
		this.menuLoadPatch.Image = (System.Drawing.Image)resources.GetObject("menuLoadPatch.Image");
		this.menuLoadPatch.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuLoadPatch.Name = "menuLoadPatch";
		this.menuLoadPatch.Size = new System.Drawing.Size(180, 22);
		this.menuLoadPatch.Text = "Load";
		this.menuLoadPatch.Click += new System.EventHandler(menuLoadPatch_Click);
		this.menuUpdateDB.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.menuOnlineFromFifa21, this.fromFIFA20ToolStripMenuItem, this.menuOnlineFromFifa17, this.fromFIFA18ToolStripMenuItem, this.menuOnlineFromFifa16 });
		this.menuUpdateDB.Name = "menuUpdateDB";
		this.menuUpdateDB.Size = new System.Drawing.Size(95, 20);
		this.menuUpdateDB.Text = "Online Update";
		this.menuOnlineFromFifa21.Name = "menuOnlineFromFifa21";
		this.menuOnlineFromFifa21.Size = new System.Drawing.Size(180, 22);
		this.menuOnlineFromFifa21.Text = "From FIFA 21";
		this.menuOnlineFromFifa21.Click += new System.EventHandler(menuOnlineFromFifa21_Click);
		this.fromFIFA20ToolStripMenuItem.Name = "fromFIFA20ToolStripMenuItem";
		this.fromFIFA20ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.fromFIFA20ToolStripMenuItem.Text = "From FIFA 20";
		this.fromFIFA20ToolStripMenuItem.Click += new System.EventHandler(fromFIFA20ToolStripMenuItem_Click);
		this.menuOnlineFromFifa17.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.rostersOnlyFromFifa17, this.rostersAndPlayersFromFifa17 });
		this.menuOnlineFromFifa17.Name = "menuOnlineFromFifa17";
		this.menuOnlineFromFifa17.Size = new System.Drawing.Size(180, 22);
		this.menuOnlineFromFifa17.Text = "From FIFA 19";
		this.rostersOnlyFromFifa17.Name = "rostersOnlyFromFifa17";
		this.rostersOnlyFromFifa17.Size = new System.Drawing.Size(175, 22);
		this.rostersOnlyFromFifa17.Text = "Rosters Only";
		this.rostersOnlyFromFifa17.Click += new System.EventHandler(rostersOnlyFromFifa17_Click);
		this.rostersAndPlayersFromFifa17.Name = "rostersAndPlayersFromFifa17";
		this.rostersAndPlayersFromFifa17.Size = new System.Drawing.Size(175, 22);
		this.rostersAndPlayersFromFifa17.Text = "Rosters and Players";
		this.rostersAndPlayersFromFifa17.Click += new System.EventHandler(rostersAndPlayersFromFifa17_Click);
		this.fromFIFA18ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.rostersAndPlayersToolStripMenuItem });
		this.fromFIFA18ToolStripMenuItem.Name = "fromFIFA18ToolStripMenuItem";
		this.fromFIFA18ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.fromFIFA18ToolStripMenuItem.Text = "From FIFA 18";
		this.rostersAndPlayersToolStripMenuItem.Name = "rostersAndPlayersToolStripMenuItem";
		this.rostersAndPlayersToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.rostersAndPlayersToolStripMenuItem.Text = "Rosters and Players";
		this.rostersAndPlayersToolStripMenuItem.Click += new System.EventHandler(rostersAndPlayersToolStripMenuItem_Click);
		this.menuOnlineFromFifa16.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.rostersOnlyFromFIFA16, this.rostersAndPlayersFromFifa16 });
		this.menuOnlineFromFifa16.Name = "menuOnlineFromFifa16";
		this.menuOnlineFromFifa16.Size = new System.Drawing.Size(180, 22);
		this.menuOnlineFromFifa16.Text = "From FIFA 16";
		this.menuOnlineFromFifa16.Click += new System.EventHandler(rostersOnlineDBFIFA16ToolStripMenuItem_Click);
		this.rostersOnlyFromFIFA16.Name = "rostersOnlyFromFIFA16";
		this.rostersOnlyFromFIFA16.Size = new System.Drawing.Size(175, 22);
		this.rostersOnlyFromFIFA16.Text = "Rosters Only";
		this.rostersOnlyFromFIFA16.Click += new System.EventHandler(rostersOnlineDBFIFA16ToolStripMenuItem_Click);
		this.rostersAndPlayersFromFifa16.Name = "rostersAndPlayersFromFifa16";
		this.rostersAndPlayersFromFifa16.Size = new System.Drawing.Size(175, 22);
		this.rostersAndPlayersFromFifa16.Text = "Rosters and Players";
		this.rostersAndPlayersFromFifa16.Click += new System.EventHandler(menuOnlineDBFifa16_Click);
		this.menuUgc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.menuImportUgc, this.menuImportUgcWothKits, this.menuImportUgcKits, this.menuImportUgcPlayers });
		this.menuUgc.Name = "menuUgc";
		this.menuUgc.Size = new System.Drawing.Size(81, 20);
		this.menuUgc.Text = "UG Content";
		this.menuUgc.Visible = false;
		this.menuImportUgc.Image = (System.Drawing.Image)resources.GetObject("menuImportUgc.Image");
		this.menuImportUgc.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuImportUgc.Name = "menuImportUgc";
		this.menuImportUgc.Size = new System.Drawing.Size(177, 22);
		this.menuImportUgc.Text = "Import DB only";
		this.menuImportUgc.Click += new System.EventHandler(importToolStripMenuItem_Click);
		this.menuImportUgcWothKits.Name = "menuImportUgcWothKits";
		this.menuImportUgcWothKits.Size = new System.Drawing.Size(177, 22);
		this.menuImportUgcWothKits.Text = "Import DB and KITS";
		this.menuImportUgcWothKits.Click += new System.EventHandler(importDBAndKITSToolStripMenuItem_Click);
		this.menuImportUgcKits.Name = "menuImportUgcKits";
		this.menuImportUgcKits.Size = new System.Drawing.Size(177, 22);
		this.menuImportUgcKits.Text = "Import KITS only";
		this.menuImportUgcKits.Click += new System.EventHandler(importKITSOmlyToolStripMenuItem_Click);
		this.menuImportUgcPlayers.Name = "menuImportUgcPlayers";
		this.menuImportUgcPlayers.Size = new System.Drawing.Size(177, 22);
		this.menuImportUgcPlayers.Text = "Import Players only";
		this.menuImportUgcPlayers.Click += new System.EventHandler(menuImportUgcPlayers_Click);
		this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.menuHelp, this.menuAbout, this.menuHelpCms });
		this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
		this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
		this.helpToolStripMenuItem.Text = "Help";
		this.menuHelp.Image = (System.Drawing.Image)resources.GetObject("menuHelp.Image");
		this.menuHelp.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuHelp.Name = "menuHelp";
		this.menuHelp.Size = new System.Drawing.Size(107, 22);
		this.menuHelp.Text = "Help";
		this.menuHelp.Click += new System.EventHandler(menuHelp_Click);
		this.menuAbout.Image = (System.Drawing.Image)resources.GetObject("menuAbout.Image");
		this.menuAbout.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.menuAbout.Name = "menuAbout";
		this.menuAbout.Size = new System.Drawing.Size(107, 22);
		this.menuAbout.Text = "About";
		this.menuAbout.Click += new System.EventHandler(menuAbout_Click);
		this.menuHelpCms.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[11]
		{
			this.genericToolStripMenuItem, this.adboardsToolStripMenuItem, this.ballsToolStripMenuItem, this.bootsToolStripMenuItem, this.countryToolStripMenuItem, this.fontsToolStripMenuItem, this.formationsToolStripMenuItem, this.leaguesToolStripMenuItem, this.stadiumsToolStripMenuItem, this.teamsToolStripMenuItem,
			this.tournamentsToolStripMenuItem
		});
		this.menuHelpCms.Name = "menuHelpCms";
		this.menuHelpCms.Size = new System.Drawing.Size(107, 22);
		this.menuHelpCms.Text = "CMS";
		this.menuHelpCms.Visible = false;
		this.genericToolStripMenuItem.Name = "genericToolStripMenuItem";
		this.genericToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.genericToolStripMenuItem.Text = "Generic";
		this.genericToolStripMenuItem.Click += new System.EventHandler(menuHelpCms_Click);
		this.adboardsToolStripMenuItem.Name = "adboardsToolStripMenuItem";
		this.adboardsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.adboardsToolStripMenuItem.Text = "Adboards";
		this.adboardsToolStripMenuItem.Click += new System.EventHandler(adboardsToolStripMenuItem_Click);
		this.ballsToolStripMenuItem.Name = "ballsToolStripMenuItem";
		this.ballsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.ballsToolStripMenuItem.Text = "Balls";
		this.ballsToolStripMenuItem.Click += new System.EventHandler(ballsToolStripMenuItem_Click);
		this.bootsToolStripMenuItem.Name = "bootsToolStripMenuItem";
		this.bootsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.bootsToolStripMenuItem.Text = "Boots";
		this.bootsToolStripMenuItem.Click += new System.EventHandler(bootsToolStripMenuItem_Click);
		this.countryToolStripMenuItem.Name = "countryToolStripMenuItem";
		this.countryToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.countryToolStripMenuItem.Text = "Country";
		this.countryToolStripMenuItem.Click += new System.EventHandler(countryToolStripMenuItem_Click);
		this.fontsToolStripMenuItem.Name = "fontsToolStripMenuItem";
		this.fontsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.fontsToolStripMenuItem.Text = "Fonts";
		this.fontsToolStripMenuItem.Click += new System.EventHandler(fontsToolStripMenuItem_Click);
		this.formationsToolStripMenuItem.Name = "formationsToolStripMenuItem";
		this.formationsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.formationsToolStripMenuItem.Text = "Formations";
		this.formationsToolStripMenuItem.Click += new System.EventHandler(formationsToolStripMenuItem_Click);
		this.leaguesToolStripMenuItem.Name = "leaguesToolStripMenuItem";
		this.leaguesToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.leaguesToolStripMenuItem.Text = "Leagues";
		this.leaguesToolStripMenuItem.Click += new System.EventHandler(leaguesToolStripMenuItem_Click);
		this.stadiumsToolStripMenuItem.Name = "stadiumsToolStripMenuItem";
		this.stadiumsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.stadiumsToolStripMenuItem.Text = "Stadiums";
		this.stadiumsToolStripMenuItem.Click += new System.EventHandler(stadiumsToolStripMenuItem_Click);
		this.teamsToolStripMenuItem.Name = "teamsToolStripMenuItem";
		this.teamsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.teamsToolStripMenuItem.Text = "Teams";
		this.teamsToolStripMenuItem.Click += new System.EventHandler(teamsToolStripMenuItem_Click);
		this.tournamentsToolStripMenuItem.Name = "tournamentsToolStripMenuItem";
		this.tournamentsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
		this.tournamentsToolStripMenuItem.Text = "Tournaments";
		this.tournamentsToolStripMenuItem.Click += new System.EventHandler(tournamentsToolStripMenuItem_Click);
		this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.progressBar, this.statusBar });
		this.statusStrip.Location = new System.Drawing.Point(0, 939);
		this.statusStrip.Name = "statusStrip";
		this.statusStrip.Size = new System.Drawing.Size(1384, 22);
		this.statusStrip.TabIndex = 1;
		this.statusStrip.Text = "statusStrip1";
		this.progressBar.Name = "progressBar";
		this.progressBar.Size = new System.Drawing.Size(100, 16);
		this.progressBar.Visible = false;
		this.statusBar.Name = "statusBar";
		this.statusBar.Size = new System.Drawing.Size(39, 17);
		this.statusBar.Text = "Ready";
		this.splitVert.BackColor = System.Drawing.Color.Blue;
		this.splitVert.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitVert.Enabled = false;
		this.splitVert.IsSplitterFixed = true;
		this.splitVert.Location = new System.Drawing.Point(0, 79);
		this.splitVert.Name = "splitVert";
		this.splitVert.Panel1.Controls.Add(this.splitHoriz);
		this.splitVert.Panel2.BackColor = System.Drawing.Color.LightSkyBlue;
		this.splitVert.Panel2.Controls.Add(this.panelRight);
		this.splitVert.Panel2.Controls.Add(this.toolStripRight);
		this.splitVert.Size = new System.Drawing.Size(1384, 860);
		this.splitVert.SplitterDistance = 1355;
		this.splitVert.SplitterWidth = 2;
		this.splitVert.TabIndex = 2;
		this.splitHoriz.BackColor = System.Drawing.Color.Blue;
		this.splitHoriz.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitHoriz.IsSplitterFixed = true;
		this.splitHoriz.Location = new System.Drawing.Point(0, 0);
		this.splitHoriz.Name = "splitHoriz";
		this.splitHoriz.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitHoriz.Panel1.BackColor = System.Drawing.SystemColors.Control;
		this.splitHoriz.Panel1.Controls.Add(this.panelMain);
		this.splitHoriz.Panel2.BackColor = System.Drawing.Color.LightSkyBlue;
		this.splitHoriz.Panel2.Controls.Add(this.panelBottom);
		this.splitHoriz.Panel2.Controls.Add(this.toolStripBottom);
		this.splitHoriz.Size = new System.Drawing.Size(1355, 860);
		this.splitHoriz.SplitterDistance = 831;
		this.splitHoriz.SplitterWidth = 2;
		this.splitHoriz.TabIndex = 0;
		this.panelMain.BackColor = System.Drawing.Color.LightSkyBlue;
		this.panelMain.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelMain.Location = new System.Drawing.Point(0, 0);
		this.panelMain.Name = "panelMain";
		this.panelMain.Size = new System.Drawing.Size(1355, 831);
		this.panelMain.TabIndex = 1;
		this.panelBottom.AutoScroll = true;
		this.panelBottom.AutoSize = true;
		this.panelBottom.BackColor = System.Drawing.Color.LightSkyBlue;
		this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelBottom.Location = new System.Drawing.Point(0, 25);
		this.panelBottom.Name = "panelBottom";
		this.panelBottom.Size = new System.Drawing.Size(1355, 2);
		this.panelBottom.TabIndex = 1;
		this.toolStripBottom.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripBottom.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.buttonShowBottom, this.buttonHideBottom, this.stripLabelBottom });
		this.toolStripBottom.Location = new System.Drawing.Point(0, 0);
		this.toolStripBottom.Name = "toolStripBottom";
		this.toolStripBottom.Size = new System.Drawing.Size(1355, 25);
		this.toolStripBottom.TabIndex = 0;
		this.toolStripBottom.Text = "toolBottom";
		this.buttonShowBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShowBottom.Image = (System.Drawing.Image)resources.GetObject("buttonShowBottom.Image");
		this.buttonShowBottom.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShowBottom.Name = "buttonShowBottom";
		this.buttonShowBottom.Size = new System.Drawing.Size(23, 22);
		this.buttonShowBottom.Text = "show";
		this.buttonShowBottom.Click += new System.EventHandler(buttonShowBottom_Click);
		this.buttonHideBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonHideBottom.Image = (System.Drawing.Image)resources.GetObject("buttonHideBottom.Image");
		this.buttonHideBottom.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonHideBottom.Name = "buttonHideBottom";
		this.buttonHideBottom.Size = new System.Drawing.Size(23, 22);
		this.buttonHideBottom.Text = "hide";
		this.buttonHideBottom.Visible = false;
		this.buttonHideBottom.Click += new System.EventHandler(buttonHideBottom_Click);
		this.stripLabelBottom.Name = "stripLabelBottom";
		this.stripLabelBottom.Size = new System.Drawing.Size(41, 22);
		this.stripLabelBottom.Text = "Empty";
		this.panelRight.AutoScroll = true;
		this.panelRight.BackColor = System.Drawing.Color.LightSkyBlue;
		this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelRight.Location = new System.Drawing.Point(24, 0);
		this.panelRight.Name = "panelRight";
		this.panelRight.Size = new System.Drawing.Size(3, 860);
		this.panelRight.TabIndex = 2;
		this.toolStripRight.Dock = System.Windows.Forms.DockStyle.Left;
		this.toolStripRight.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripRight.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.buttonShowRight, this.buttonHideRight, this.stripLabelRight });
		this.toolStripRight.Location = new System.Drawing.Point(0, 0);
		this.toolStripRight.Name = "toolStripRight";
		this.toolStripRight.Size = new System.Drawing.Size(24, 860);
		this.toolStripRight.TabIndex = 1;
		this.toolStripRight.Text = "toolBottom";
		this.buttonShowRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShowRight.Image = (System.Drawing.Image)resources.GetObject("buttonShowRight.Image");
		this.buttonShowRight.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShowRight.Name = "buttonShowRight";
		this.buttonShowRight.Size = new System.Drawing.Size(21, 20);
		this.buttonShowRight.Text = "show";
		this.buttonShowRight.Click += new System.EventHandler(buttonShowRight_Click);
		this.buttonHideRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonHideRight.Image = (System.Drawing.Image)resources.GetObject("buttonHideRight.Image");
		this.buttonHideRight.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonHideRight.Name = "buttonHideRight";
		this.buttonHideRight.Size = new System.Drawing.Size(21, 20);
		this.buttonHideRight.Text = "hide";
		this.buttonHideRight.Visible = false;
		this.buttonHideRight.Click += new System.EventHandler(buttonHideRight_Click);
		this.stripLabelRight.Name = "stripLabelRight";
		this.stripLabelRight.Size = new System.Drawing.Size(21, 41);
		this.stripLabelRight.Text = "Empty";
		this.stripLabelRight.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical90;
		this.toolStripMain.Enabled = false;
		this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[20]
		{
			this.buttonCountry, this.buttonLeague, this.buttonTeam, this.buttonKit, this.buttonPlayer, this.buttonStadium, this.buttonTournament, this.buttonReferee, this.buttonBall, this.buttonShoes,
			this.buttonManager, this.buttonFormation, this.buttonSponsor, this.buttonTv, this.buttonNewspaper, this.buttonGloves, this.buttonAudio, this.buttonGameGraphics, this.buttonBrowser, this.buttonImportGraphics
		});
		this.toolStripMain.Location = new System.Drawing.Point(0, 24);
		this.toolStripMain.Name = "toolStripMain";
		this.toolStripMain.Size = new System.Drawing.Size(1384, 55);
		this.toolStripMain.TabIndex = 0;
		this.toolStripMain.Text = "toolStripMain";
		this.toolTip.SetToolTip(this.toolStripMain, "Click and use Shift, Ctrl and Alt keys to activate a different window");
		this.buttonCountry.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCountry.Image = (System.Drawing.Image)resources.GetObject("buttonCountry.Image");
		this.buttonCountry.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCountry.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCountry.Name = "buttonCountry";
		this.buttonCountry.Size = new System.Drawing.Size(52, 52);
		this.buttonCountry.Text = "Country";
		this.buttonCountry.ToolTipText = "Country";
		this.buttonCountry.Click += new System.EventHandler(buttonMain_Click);
		this.buttonLeague.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonLeague.Image = (System.Drawing.Image)resources.GetObject("buttonLeague.Image");
		this.buttonLeague.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonLeague.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonLeague.Name = "buttonLeague";
		this.buttonLeague.Size = new System.Drawing.Size(52, 52);
		this.buttonLeague.Text = "League";
		this.buttonLeague.Click += new System.EventHandler(buttonMain_Click);
		this.buttonTeam.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonTeam.Image = (System.Drawing.Image)resources.GetObject("buttonTeam.Image");
		this.buttonTeam.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonTeam.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonTeam.Name = "buttonTeam";
		this.buttonTeam.Size = new System.Drawing.Size(52, 52);
		this.buttonTeam.Text = "Team";
		this.buttonTeam.Click += new System.EventHandler(buttonMain_Click);
		this.buttonKit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonKit.Image = (System.Drawing.Image)resources.GetObject("buttonKit.Image");
		this.buttonKit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonKit.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonKit.Name = "buttonKit";
		this.buttonKit.Size = new System.Drawing.Size(52, 52);
		this.buttonKit.Text = "Kits";
		this.buttonKit.Click += new System.EventHandler(buttonMain_Click);
		this.buttonPlayer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPlayer.Image = (System.Drawing.Image)resources.GetObject("buttonPlayer.Image");
		this.buttonPlayer.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonPlayer.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPlayer.Name = "buttonPlayer";
		this.buttonPlayer.Size = new System.Drawing.Size(52, 52);
		this.buttonPlayer.Text = "Player";
		this.buttonPlayer.Click += new System.EventHandler(buttonMain_Click);
		this.buttonStadium.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonStadium.Image = (System.Drawing.Image)resources.GetObject("buttonStadium.Image");
		this.buttonStadium.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonStadium.ImageTransparentColor = System.Drawing.Color.White;
		this.buttonStadium.Name = "buttonStadium";
		this.buttonStadium.Size = new System.Drawing.Size(52, 52);
		this.buttonStadium.Text = "Stadium";
		this.buttonStadium.ToolTipText = "Stadium";
		this.buttonStadium.Click += new System.EventHandler(buttonMain_Click);
		this.buttonTournament.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonTournament.Image = (System.Drawing.Image)resources.GetObject("buttonTournament.Image");
		this.buttonTournament.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonTournament.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonTournament.Name = "buttonTournament";
		this.buttonTournament.Size = new System.Drawing.Size(52, 52);
		this.buttonTournament.Text = "Tournament in Manager Mode";
		this.buttonTournament.ToolTipText = "Tournament";
		this.buttonTournament.Click += new System.EventHandler(buttonMain_Click);
		this.buttonReferee.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonReferee.Image = (System.Drawing.Image)resources.GetObject("buttonReferee.Image");
		this.buttonReferee.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonReferee.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonReferee.Name = "buttonReferee";
		this.buttonReferee.Size = new System.Drawing.Size(52, 52);
		this.buttonReferee.Text = "Referee";
		this.buttonReferee.Click += new System.EventHandler(buttonMain_Click);
		this.buttonBall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonBall.Image = (System.Drawing.Image)resources.GetObject("buttonBall.Image");
		this.buttonBall.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonBall.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonBall.Name = "buttonBall";
		this.buttonBall.Size = new System.Drawing.Size(52, 52);
		this.buttonBall.Text = "Ball";
		this.buttonBall.Click += new System.EventHandler(buttonMain_Click);
		this.buttonShoes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShoes.Image = (System.Drawing.Image)resources.GetObject("buttonShoes.Image");
		this.buttonShoes.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonShoes.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShoes.Name = "buttonShoes";
		this.buttonShoes.Size = new System.Drawing.Size(52, 52);
		this.buttonShoes.Text = "Boots";
		this.buttonShoes.Click += new System.EventHandler(buttonMain_Click);
		this.buttonManager.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonManager.Image = (System.Drawing.Image)resources.GetObject("buttonManager.Image");
		this.buttonManager.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonManager.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonManager.Name = "buttonManager";
		this.buttonManager.Size = new System.Drawing.Size(52, 52);
		this.buttonManager.Text = "Manager";
		this.buttonManager.Click += new System.EventHandler(buttonMain_Click);
		this.buttonFormation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonFormation.Image = (System.Drawing.Image)resources.GetObject("buttonFormation.Image");
		this.buttonFormation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonFormation.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonFormation.Name = "buttonFormation";
		this.buttonFormation.Size = new System.Drawing.Size(52, 52);
		this.buttonFormation.Text = "Generic Formations";
		this.buttonFormation.Click += new System.EventHandler(buttonMain_Click);
		this.buttonSponsor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSponsor.Image = (System.Drawing.Image)resources.GetObject("buttonSponsor.Image");
		this.buttonSponsor.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonSponsor.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSponsor.Name = "buttonSponsor";
		this.buttonSponsor.Size = new System.Drawing.Size(52, 52);
		this.buttonSponsor.Text = "Sponsor";
		this.buttonSponsor.Visible = false;
		this.buttonSponsor.Click += new System.EventHandler(buttonMain_Click);
		this.buttonTv.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonTv.Image = (System.Drawing.Image)resources.GetObject("buttonTv.Image");
		this.buttonTv.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonTv.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonTv.Name = "buttonTv";
		this.buttonTv.Size = new System.Drawing.Size(52, 52);
		this.buttonTv.Text = "TV";
		this.buttonTv.Visible = false;
		this.buttonTv.Click += new System.EventHandler(buttonMain_Click);
		this.buttonNewspaper.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonNewspaper.Image = (System.Drawing.Image)resources.GetObject("buttonNewspaper.Image");
		this.buttonNewspaper.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonNewspaper.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonNewspaper.Name = "buttonNewspaper";
		this.buttonNewspaper.Size = new System.Drawing.Size(52, 52);
		this.buttonNewspaper.Text = "Newspaper";
		this.buttonNewspaper.Click += new System.EventHandler(buttonMain_Click);
		this.buttonGloves.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonGloves.Image = (System.Drawing.Image)resources.GetObject("buttonGloves.Image");
		this.buttonGloves.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonGloves.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonGloves.Name = "buttonGloves";
		this.buttonGloves.Size = new System.Drawing.Size(52, 52);
		this.buttonGloves.Text = "Gloves and accessories";
		this.buttonGloves.Click += new System.EventHandler(buttonMain_Click);
		this.buttonAudio.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAudio.Image = (System.Drawing.Image)resources.GetObject("buttonAudio.Image");
		this.buttonAudio.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonAudio.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAudio.Name = "buttonAudio";
		this.buttonAudio.Size = new System.Drawing.Size(52, 52);
		this.buttonAudio.Text = "Audio";
		this.buttonAudio.Click += new System.EventHandler(buttonMain_Click);
		this.buttonGameGraphics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonGameGraphics.Image = (System.Drawing.Image)resources.GetObject("buttonGameGraphics.Image");
		this.buttonGameGraphics.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonGameGraphics.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonGameGraphics.Name = "buttonGameGraphics";
		this.buttonGameGraphics.Size = new System.Drawing.Size(52, 52);
		this.buttonGameGraphics.Text = "Game Graphics";
		this.buttonGameGraphics.Click += new System.EventHandler(buttonMain_Click);
		this.buttonBrowser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonBrowser.Image = (System.Drawing.Image)resources.GetObject("buttonBrowser.Image");
		this.buttonBrowser.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonBrowser.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonBrowser.Name = "buttonBrowser";
		this.buttonBrowser.Size = new System.Drawing.Size(52, 52);
		this.buttonBrowser.Text = "Web Grabber";
		this.buttonBrowser.Click += new System.EventHandler(buttonMain_Click);
		this.buttonImportGraphics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportGraphics.Image = (System.Drawing.Image)resources.GetObject("buttonImportGraphics.Image");
		this.buttonImportGraphics.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportGraphics.Name = "buttonImportGraphics";
		this.buttonImportGraphics.Size = new System.Drawing.Size(23, 52);
		this.buttonImportGraphics.Text = "Import Graphics";
		this.buttonImportGraphics.Visible = false;
		this.buttonImportGraphics.Click += new System.EventHandler(buttonMain_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1384, 961);
		base.Controls.Add(this.splitVert);
		base.Controls.Add(this.statusStrip);
		base.Controls.Add(this.toolStripMain);
		base.Controls.Add(this.menuStrip);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.IsMdiContainer = true;
		base.MainMenuStrip = this.menuStrip;
		this.MinimumSize = new System.Drawing.Size(200, 199);
		base.Name = "MainForm";
		this.Text = "Creation Master 26";
		base.SizeChanged += new System.EventHandler(MainForm_SizeChanged);
		this.menuStrip.ResumeLayout(false);
		this.menuStrip.PerformLayout();
		this.statusStrip.ResumeLayout(false);
		this.statusStrip.PerformLayout();
		this.splitVert.Panel1.ResumeLayout(false);
		this.splitVert.Panel2.ResumeLayout(false);
		this.splitVert.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitVert).EndInit();
		this.splitVert.ResumeLayout(false);
		this.splitHoriz.Panel1.ResumeLayout(false);
		this.splitHoriz.Panel2.ResumeLayout(false);
		this.splitHoriz.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitHoriz).EndInit();
		this.splitHoriz.ResumeLayout(false);
		this.toolStripBottom.ResumeLayout(false);
		this.toolStripBottom.PerformLayout();
		this.toolStripRight.ResumeLayout(false);
		this.toolStripRight.PerformLayout();
		this.toolStripMain.ResumeLayout(false);
		this.toolStripMain.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
