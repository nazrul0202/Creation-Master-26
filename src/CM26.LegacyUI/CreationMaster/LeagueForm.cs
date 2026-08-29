using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class LeagueForm : Form
{
	private League m_CurrentLeague;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private bool m_Locked;

	private int m_AssetLoadGeneration;

	private IContainer components;

	public PickUpControl pickUpControl;

	private FlowLayoutPanel flowPanel;

	private GroupBox groupBoxTeams;

	private ListView listViewPlayingTeams;

	private ToolStrip toolStripTeamAvailable;

	private ToolStripSeparator separatorBegin;

	public ToolStripComboBox comboTeamAvailable;

	private ToolStripSeparator separatorButtons;

	public ToolStripButton buttonAddTeam;

	public ToolStripButton buttonReplaceTeam;

	public ToolStripButton buttonRemoveTeam;

	private ToolStripSeparator separatorShowLogo;

	private ToolStripButton checkShowTeamLogo;

	private ToolStripButton buttonCreateTeamInLeague;
	private ToolStripButton buttonMakeLeagueInGameReady;

	private GroupBox groupBoxNames;

	private TextBox textLeagueFullName;

	private Label labelLeagueFullName;

	private TextBox textLeagueShortName;

	private Label labelLeagueShortName;

	private TextBox textDatabaseLeagueName;

	private Label labelDatabaseLeagueName;

	private GroupBox groupBox3;

	private Label labelLeagueId;

	private Button buttonGetId;

	private ComboBox comboLeagueCountry;

	private NumericUpDown numericLeagueId;

	private Label labelCountry;

	private Label labelLeagueLevel;

	private NumericUpDown numericLeagueLevel;

	private ImageList imageListTeamLogos;

	private BindingSource leagueBindingSource;

	private BindingSource countryListBindingSource;

	private Viewer2D viewer2DLeagueTinyLogo;

	private Button buttonreplicateLeagueTinyLogo;

	private Viewer2D viewer2DLeagueAnimLogo;

	private Viewer2D viewer2DLeagueSmallLogo;

	private Button buttonreplicateLeagueSmallLogo;

	private GroupBox groupSwitchLeagues;

	private Label labelThisLeague;

	private Button buttonSwitchLeagueIds;

	private ComboBox comboSwitchLeagues;

	private GroupBox groupLeaguePlayerTuning;

	private Button buttonLeaguePlayerMinus;

	private Button buttonLeaguePlayerPlus;

	private GroupBox groupBox1;

	private NumericUpDown numericBoardOutcome5;

	private Label label4;

	private NumericUpDown numericBoardOutcome4;

	private Label label5;

	private NumericUpDown numericBoardOutcome3;

	private Label label3;

	private NumericUpDown numericBoardOutcome2;

	private Label label2;

	private NumericUpDown numericBoardOutcome1;

	private Label label1;

	private Viewer2D viewer2DLeague512x128Logo;

	private Button buttonreplicateLeagueLogo512x128;

	private Button button1;

	private ComboBox comboLeaguePrestige;

	private Label label6;

	public LeagueForm()
	{
		base.Visible = false;
		InitializeComponent();
		CmStyleDetailsWindow.Attach(this, "League Details", DetailSection.League,
			() => m_CurrentLeague?.Id ?? -1);
		pickUpControl.SelectObject = SelectLeague;
		pickUpControl.CreateObject = CreateLeague;
		pickUpControl.DeleteObject = DeleteLeague;
		pickUpControl.RefreshObject = RefreshLeague;
		buttonCreateTeamInLeague = new ToolStripButton("Create Team Here")
		{
			Name = "buttonCreateTeamInLeague",
			DisplayStyle = ToolStripItemDisplayStyle.Text,
			ToolTipText = "Create a new team and link it directly to this league"
		};
		buttonCreateTeamInLeague.Click += buttonCreateTeamInLeague_Click;
		toolStripTeamAvailable.Items.Insert(Math.Min(3, toolStripTeamAvailable.Items.Count), buttonCreateTeamInLeague);
		buttonMakeLeagueInGameReady = new ToolStripButton("Make In-Game Ready")
		{
			Name = "buttonMakeLeagueInGameReady",
			DisplayStyle = ToolStripItemDisplayStyle.Text,
			ToolTipText = "Build Compdata, assign this league's teams, generate its calendar and stage it for Save"
		};
		buttonMakeLeagueInGameReady.Click += buttonMakeLeagueInGameReady_Click;
		toolStripTeamAvailable.Items.Insert(Math.Min(4, toolStripTeamAvailable.Items.Count), buttonMakeLeagueInGameReady);
		viewer2DLeagueTinyLogo.ImageImport = ImportImageLeagueTinyLogo;
		viewer2DLeagueTinyLogo.ImageDelete = DeleteLeagueTinyLogo;
		viewer2DLeagueTinyLogo.ButtonStripVisible = true;
		viewer2DLeagueTinyLogo.RemoveButton = true;
		viewer2DLeagueAnimLogo.ImageImport = ImportImageLeagueAnimLogo;
		viewer2DLeagueAnimLogo.ImageDelete = DeleteLeagueAnimLogo;
		viewer2DLeagueAnimLogo.ButtonStripVisible = true;
		viewer2DLeagueAnimLogo.RemoveButton = true;
		viewer2DLeague512x128Logo.ImageImport = ImportImageLeagueLogo512x128;
		viewer2DLeague512x128Logo.ImageDelete = DeleteLeagueLogo512x128;
		viewer2DLeague512x128Logo.ButtonStripVisible = true;
		viewer2DLeague512x128Logo.RemoveButton = true;
	}

	private void buttonCreateTeamInLeague_Click(object sender, EventArgs e)
	{
		if (m_CurrentLeague == null)
		{
			MessageBox.Show(this, "Select a league first.", "Create Team",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		MainForm.CM?.CreateTeamInLeague(m_CurrentLeague);
	}

	private void buttonMakeLeagueInGameReady_Click(object sender, EventArgs e)
	{
		if (m_CurrentLeague == null)
		{
			MessageBox.Show(this, "Select a league first.", "Make League In-Game Ready",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		MainForm.CM?.MakeLeagueInGameReady(m_CurrentLeague);
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.Leagues;
		Button button = buttonreplicateLeagueLogo512x128;
		bool visible = (viewer2DLeague512x128Logo.Visible = FifaEnvironment.Year > 14);
		button.Visible = visible;
		IdArrayList[] filterValues = new IdArrayList[2]
		{
			null,
			FifaEnvironment.Countries
		};
		pickUpControl.FilterValues = filterValues;
		// FC26 uses sparse league ids and the legacy list's MaxId metadata is not
		// guaranteed to describe every imported record.
		numericLeagueId.Maximum = Math.Max(200000, FifaEnvironment.Leagues.MaxId);
		RefreshComboBoxes();
		pickUpControl.ObjectList = FifaEnvironment.Leagues;
	}

	public void RefreshComboBoxes()
	{
		if (comboTeamAvailable.Items.Count != FifaEnvironment.Teams.Count)
		{
			comboTeamAvailable.Items.Clear();
			comboTeamAvailable.Items.AddRange(FifaEnvironment.Teams.ToArray());
		}
		if (comboLeagueCountry.Items.Count != FifaEnvironment.Countries.Count + 1)
		{
			comboLeagueCountry.Items.Clear();
			comboLeagueCountry.Items.Add("None");
			comboLeagueCountry.Items.AddRange(FifaEnvironment.Countries.ToArray());
		}
		if (comboSwitchLeagues.Items.Count != FifaEnvironment.Leagues.Count + 1)
		{
			comboSwitchLeagues.Items.Clear();
			comboSwitchLeagues.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		}
	}

	private League SelectLeague(object sender, object obj)
	{
		League league = (League)obj;
		Refresh();
		LoadLeague(league);
		return league;
	}

	private League CreateLeague(object sender, object obj)
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
		League league = (League)m_NewIdCreator.NewObject;
		if (m_NewIdCreator.NewName != null && league != null)
		{
			league.leaguename = m_NewIdCreator.NewName;
			league.ShortName = league.leaguename;
		}
		return league;
	}

	private League DeleteLeague(object sender, object obj)
	{
		League league = (League)obj;
		FifaEnvironment.Leagues.DeleteLeague(league);
		m_CurrentLeague = null;
		return null;
	}

	public League RefreshLeague(object sender, object obj)
	{
		Preset();
		ReloadLeague(m_CurrentLeague);
		return m_CurrentLeague;
	}

	private bool ImportImageLeagueLogo512x128(object sender, Bitmap bitmap)
	{
		m_CurrentLeague.SetLogo512x128Dark(bitmap);
		return m_CurrentLeague.SetLogo512x128(bitmap);
	}

	private bool DeleteLeagueLogo512x128(object sender)
	{
		m_CurrentLeague.DeleteLogo512x128Dark();
		return m_CurrentLeague.DeleteLogo512x128();
	}

	private bool ImportImageLeagueTinyLogo(object sender, Bitmap bitmap)
	{
		m_CurrentLeague.SetTinyLogoDark(bitmap);
		return m_CurrentLeague.SetTinyLogo(bitmap);
	}

	private bool DeleteLeagueTinyLogo(object sender)
	{
		m_CurrentLeague.DeleteTinyLogoDark();
		return m_CurrentLeague.DeleteTinyLogo();
	}

	private bool ImportImageLeagueAnimLogo(object sender, Bitmap bitmap)
	{
		m_CurrentLeague.SetAnimLogoDark(bitmap);
		return m_CurrentLeague.SetAnimLogo(bitmap);
	}

	private bool DeleteLeagueAnimLogo(object sender)
	{
		return m_CurrentLeague.DeleteAnimLogo();
	}

	private bool ImportImageLeagueSmallLogo(object sender, Bitmap bitmap)
	{
		m_CurrentLeague.SetSmallLogoDark(bitmap);
		return m_CurrentLeague.SetSmallLogo(bitmap);
	}

	private bool DeleteLeagueSmallLogo(object sender)
	{
		return m_CurrentLeague.DeleteSmallLogo();
	}

	public void ReloadLeague(League league)
	{
		m_CurrentLeague = null;
		LoadLeague(league);
	}

	public void LoadLeague(League league)
	{
		if (m_IsLoaded && m_CurrentLeague != league)
		{
			m_Locked = true;
			m_CurrentLeague = league;
			leagueBindingSource.DataSource = m_CurrentLeague;
			comboTeamAvailable.Text = "";
			SetNumericValue(numericLeagueId, m_CurrentLeague.Id);
			if (m_CurrentLeague.Country == null)
			{
				comboLeagueCountry.SelectedIndex = 0;
			}
			else
			{
				comboLeagueCountry.SelectedItem = m_CurrentLeague.Country;
			}
			// Draw the FC26 database content immediately. Frostbite asset lookup can
			// take several seconds on a cold cache and must not block league/section
			// navigation or leave the form looking empty.
			InitListViewPlayingTeams(league.PlayingTeams, false);
			viewer2DLeagueTinyLogo.CurrentBitmap = null;
			viewer2DLeagueAnimLogo.CurrentBitmap = null;
			viewer2DLeague512x128Logo.CurrentBitmap = null;
			labelThisLeague.Text = league.ShortName;
			buttonSwitchLeagueIds.Enabled = comboSwitchLeagues.SelectedItem != null;
			int prestige = (int)m_CurrentLeague.Prestige;
			comboLeaguePrestige.SelectedIndex = prestige >= 0 && prestige < comboLeaguePrestige.Items.Count ? prestige : -1;
			SetNumericValue(numericBoardOutcome1, m_CurrentLeague.boardoutcomes[0]);
			SetNumericValue(numericBoardOutcome2, m_CurrentLeague.boardoutcomes[1]);
			SetNumericValue(numericBoardOutcome3, m_CurrentLeague.boardoutcomes[2]);
			SetNumericValue(numericBoardOutcome4, m_CurrentLeague.boardoutcomes[3]);
			SetNumericValue(numericBoardOutcome5, m_CurrentLeague.boardoutcomes[4]);
			m_Locked = false;
			LoadLeagueAssetsAsync(league, ++m_AssetLoadGeneration);
		}
	}

	public void AuditFc26RecordsForSmoke()
	{
		if (FifaEnvironment.Leagues.Count == 0) return;
		var samples = new[] { 0, FifaEnvironment.Leagues.Count / 2, FifaEnvironment.Leagues.Count - 1 };
		foreach (var index in samples)
			ReloadLeague((League)FifaEnvironment.Leagues[index]);
	}

	private async void LoadLeagueAssetsAsync(League league, int generation)
	{
		try
		{
			await System.Threading.Tasks.Task.Run(() => PreloadLeagueAssets(league));
			if (IsDisposed || Disposing || m_CurrentLeague != league || generation != m_AssetLoadGeneration) return;
			RefreshTeamLogos();
			viewer2DLeagueTinyLogo.CurrentBitmap = league.GetTinyLogo();
			viewer2DLeagueAnimLogo.CurrentBitmap = league.GetAnimLogo();
			viewer2DLeague512x128Logo.CurrentBitmap = league.GetLogo512x128();
		}
		catch (Exception ex)
		{
			// Database editing remains usable when one optional Frostbite asset is
			// absent or malformed. The host bridge records detailed asset failures.
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}

	private void RefreshTeamLogos()
	{
		if (!checkShowTeamLogo.Checked) return;
		listViewPlayingTeams.BeginUpdate();
		try
		{
			imageListTeamLogos.Images.Clear();
			foreach (ListViewItem item in listViewPlayingTeams.Items)
			{
				if (item.Tag is not Team team) continue;
				Bitmap bitmap = team.GetCrest32();
				if (bitmap != null) imageListTeamLogos.Images.Add(team.ToString(), bitmap);
				item.ImageKey = team.ToString();
			}
		}
		finally
		{
			listViewPlayingTeams.EndUpdate();
		}
	}

	private static void PreloadLeagueAssets(League league)
	{
		if (league == null) return;
		var paths = new System.Collections.Generic.List<string>
		{
			league.TinyLogoDdsFileName(),
			league.AnimLogoDdsFileName(),
			league.Logo512x128DdsFileName()
		};
		for (int i = 0; i < league.PlayingTeams.Count; i++)
		{
			paths.Add(((Team)league.PlayingTeams[i]).Crest32DdsFileName());
		}
		Fc26HostBridge.PreloadAssets(paths);
	}

	private static void SetNumericValue(NumericUpDown control, int value)
	{
		control.Value = Math.Max(control.Minimum, Math.Min(control.Maximum, value));
	}

	private void InitListViewPlayingTeams(TeamList playingTeams, bool loadLogos = true)
	{
		listViewPlayingTeams.BeginUpdate();
		listViewPlayingTeams.Items.Clear();
		imageListTeamLogos.Images.Clear();
		for (int i = 0; i < playingTeams.Count; i++)
		{
			Team team = (Team)playingTeams[i];
			Bitmap bitmap = null;
			if (loadLogos && checkShowTeamLogo.Checked)
			{
				bitmap = team.GetCrest32();
			}
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

	private void textLeagueShortName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentLeague.ShortName = textLeagueShortName.Text;
			pickUpControl.SwitchObject(m_CurrentLeague);
		}
	}

	private void textLeagueFullName_TextChanged(object sender, EventArgs e)
	{
		if (!m_Locked)
		{
			m_CurrentLeague.LongName = textLeagueFullName.Text;
		}
	}

	private void numericLeagueId_ValueChanged(object sender, EventArgs e)
	{
		if (m_Locked || m_CurrentLeague == null)
		{
			return;
		}
		int num = (int)numericLeagueId.Value;
		if (num != m_CurrentLeague.Id)
		{
			if (FifaEnvironment.Leagues.SearchId(num) == null)
			{
				FifaEnvironment.Leagues.ChangeId(m_CurrentLeague, num);
				viewer2DLeagueTinyLogo.CurrentBitmap = m_CurrentLeague.GetTinyLogo();
				viewer2DLeagueAnimLogo.CurrentBitmap = m_CurrentLeague.GetAnimLogo();
				viewer2DLeague512x128Logo.CurrentBitmap = m_CurrentLeague.GetLogo512x128();
			}
			else
			{
				FifaEnvironment.UserMessages.ShowMessage(1015);
				numericLeagueId.Value = m_CurrentLeague.Id;
			}
		}
	}

	private void buttonGetId_Click(object sender, EventArgs e)
	{
		int newId = FifaEnvironment.Leagues.GetNewId();
		if (newId == -1)
		{
			FifaEnvironment.UserMessages.ShowMessage(5050);
		}
		else
		{
			numericLeagueId.Value = newId;
		}
	}

	private void checkShowTeamLogo_CheckedChanged(object sender, EventArgs e)
	{
		InitListViewPlayingTeams(m_CurrentLeague.PlayingTeams);
	}

	private int GetTeamIndex(Team team)
	{
		for (int i = 0; i < listViewPlayingTeams.Items.Count; i++)
		{
			if (listViewPlayingTeams.Items[i].Tag == team)
			{
				return i;
			}
		}
		return -1;
	}

	private bool AddTeam()
	{
		if (comboTeamAvailable.SelectedItem == null)
		{
			FifaEnvironment.UserMessages.ShowMessage(1000);
			return false;
		}
		Team team = (Team)comboTeamAvailable.SelectedItem;
		ListViewItem listViewItem = new ListViewItem(team.ToString(), team.ToString());
		listViewItem.Tag = team;
		if (GetTeamIndex(team) >= 0)
		{
			FifaEnvironment.UserMessages.ShowMessage(1001);
			return false;
		}
		if (checkShowTeamLogo.Checked)
		{
			Bitmap crest = team.GetCrest32();
			if (crest != null)
			{
				imageListTeamLogos.Images.Add(team.ToString(), crest);
			}
			if (crest != null)
			{
				imageListTeamLogos.Images.Add(team.ToString(), crest);
			}
		}
		listViewPlayingTeams.Items.Add(listViewItem);
		m_CurrentLeague.AddTeam(team);
		return true;
	}

	private void buttonAddTeam_Click(object sender, EventArgs e)
	{
		AddTeam();
	}

	private bool RemoveTeam()
	{
		if (listViewPlayingTeams.SelectedItems.Count <= 0)
		{
			FifaEnvironment.UserMessages.ShowMessage(1002);
			return false;
		}
		Team team = (Team)listViewPlayingTeams.SelectedItems[0].Tag;
		if (team == null)
		{
			return false;
		}
		int teamIndex = GetTeamIndex(team);
		if (teamIndex < 0)
		{
			return false;
		}
		listViewPlayingTeams.Items.RemoveAt(teamIndex);
		imageListTeamLogos.Images.RemoveByKey(team.ToString());
		m_CurrentLeague.RemoveTeam(team);
		return true;
	}

	private void buttonRemoveTeam_Click(object sender, EventArgs e)
	{
		RemoveTeam();
	}

	private void buttonReplaceTeam_Click(object sender, EventArgs e)
	{
		if (RemoveTeam())
		{
			AddTeam();
		}
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

	private void labelCountry_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentLeague.Country != null)
		{
			MainForm.CM.JumpTo(m_CurrentLeague.Country);
		}
	}

	private void LeagueForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void comboLeagueCountry_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboLeagueCountry.SelectedIndex >= 0)
		{
			if (comboLeagueCountry.SelectedIndex == 0)
			{
				m_CurrentLeague.Country = null;
			}
			else
			{
				m_CurrentLeague.Country = (Country)comboLeagueCountry.SelectedItem;
			}
		}
	}

	private void buttonreplicateLeagueTinyLogo_Click(object sender, EventArgs e)
	{
		Bitmap currentBitmap = viewer2DLeagueAnimLogo.CurrentBitmap;
		Bitmap bitmap = new Bitmap(256, 64, PixelFormat.Format32bppPArgb);
		Rectangle srcRect = new Rectangle(0, 0, 256, 256);
		Rectangle destRect = new Rectangle(145, 0, 64, 64);
		GraphicUtil.RemapRectangle(currentBitmap, srcRect, bitmap, destRect);
		m_CurrentLeague.SetTinyLogo(bitmap);
		m_CurrentLeague.SetTinyLogoDark(bitmap);
		viewer2DLeagueTinyLogo.CurrentBitmap = bitmap;
	}

	private void buttonreplicateLeagueLogo512x128_Click(object sender, EventArgs e)
	{
		Bitmap currentBitmap = viewer2DLeagueAnimLogo.CurrentBitmap;
		Bitmap bitmap = new Bitmap(512, 128, PixelFormat.Format32bppPArgb);
		Rectangle srcRect = new Rectangle(0, 0, 256, 256);
		Rectangle destRect = new Rectangle(192, 0, 128, 128);
		GraphicUtil.RemapRectangle(currentBitmap, srcRect, bitmap, destRect);
		destRect = new Rectangle(32, 0, 128, 128);
		GraphicUtil.RemapRectangle(currentBitmap, srcRect, bitmap, destRect);
		destRect = new Rectangle(352, 0, 128, 128);
		GraphicUtil.RemapRectangle(currentBitmap, srcRect, bitmap, destRect);
		m_CurrentLeague.SetLogo512x128(bitmap);
		m_CurrentLeague.SetLogo512x128Dark(bitmap);
		viewer2DLeague512x128Logo.CurrentBitmap = bitmap;
	}

	private void buttonreplicateLeagueSmallLogo_Click(object sender, EventArgs e)
	{
		Bitmap currentBitmap = viewer2DLeagueAnimLogo.CurrentBitmap;
		Bitmap bitmap = new Bitmap(256, 256, PixelFormat.Format32bppPArgb);
		Rectangle srcRect = new Rectangle(0, 0, 256, 256);
		Rectangle destRect = new Rectangle(25, 0, 150, 150);
		GraphicUtil.RemapRectangle(currentBitmap, srcRect, bitmap, destRect);
		m_CurrentLeague.SetSmallLogo(bitmap);
		m_CurrentLeague.SetSmallLogoDark(bitmap);
	}

	private void comboSwitchLeagues_SelectedIndexChanged(object sender, EventArgs e)
	{
		buttonSwitchLeagueIds.Enabled = comboSwitchLeagues.SelectedItem != null;
	}

	private void buttonSwitchLeagueIds_Click(object sender, EventArgs e)
	{
		League league = (League)comboSwitchLeagues.SelectedItem;
		if (league != null && league != m_CurrentLeague)
		{
			Bitmap animLogo = m_CurrentLeague.GetAnimLogo();
			Bitmap smallLogo = m_CurrentLeague.GetSmallLogo();
			Bitmap tinyLogo = m_CurrentLeague.GetTinyLogo();
			Bitmap animLogo2 = league.GetAnimLogo();
			Bitmap smallLogo2 = league.GetSmallLogo();
			Bitmap tinyLogo2 = league.GetTinyLogo();
			Trophy trophy = FifaEnvironment.CompetitionObjects.SearchTrophy(m_CurrentLeague.Id);
			Trophy trophy2 = FifaEnvironment.CompetitionObjects.SearchTrophy(league.Id);
			Bitmap trophy3 = null;
			Bitmap trophy4 = null;
			Bitmap[] textures = null;
			string model = null;
			if (trophy != null)
			{
				trophy3 = trophy.GetTrophy256();
				trophy4 = trophy.GetTrophy128();
				textures = trophy.GetTextures();
				model = trophy.ExportModelFile();
			}
			Bitmap trophy5 = null;
			Bitmap trophy6 = null;
			Bitmap[] textures2 = null;
			string model2 = null;
			if (trophy2 != null)
			{
				trophy5 = trophy2.GetTrophy256();
				trophy6 = trophy2.GetTrophy128();
				textures2 = trophy2.GetTextures();
				model2 = trophy2.ExportModelFile();
			}
			int id = m_CurrentLeague.Id;
			m_CurrentLeague.Id = league.Id;
			league.Id = id;
			m_CurrentLeague.SetAnimLogo(animLogo);
			m_CurrentLeague.SetAnimLogoDark(animLogo);
			m_CurrentLeague.SetSmallLogo(smallLogo);
			m_CurrentLeague.SetSmallLogoDark(smallLogo);
			m_CurrentLeague.SetTinyLogo(tinyLogo);
			m_CurrentLeague.SetTinyLogoDark(tinyLogo);
			league.SetAnimLogo(animLogo2);
			league.SetAnimLogoDark(animLogo2);
			league.SetSmallLogo(smallLogo2);
			league.SetSmallLogoDark(smallLogo2);
			league.SetTinyLogo(tinyLogo2);
			league.SetTinyLogoDark(tinyLogo2);
			if (trophy != null)
			{
				trophy.Settings.m_asset_id = league.Id;
				trophy.SetTrophy256(trophy3);
				trophy.SetTrophy128(trophy4);
				trophy.Settings.m_asset_id = m_CurrentLeague.Id;
				trophy.TypeString = "C" + m_CurrentLeague.Id;
				trophy.Description = FifaEnvironment.Language.GetTournamentConventionalString(m_CurrentLeague.Id, Language.ETournamentStringType.Abbr15);
				trophy.SetTextures(textures);
				trophy.SetModel(model);
			}
			if (trophy2 != null)
			{
				trophy2.Settings.m_asset_id = id;
				trophy2.SetTrophy256(trophy5);
				trophy2.SetTrophy128(trophy6);
				trophy2.Settings.m_asset_id = league.Id;
				trophy2.TypeString = "C" + league.Id;
				trophy2.Description = FifaEnvironment.Language.GetTournamentConventionalString(league.Id, Language.ETournamentStringType.Abbr15);
				trophy2.SetTextures(textures2);
				trophy2.SetModel(model2);
			}
			numericLeagueId.Value = m_CurrentLeague.Id;
			MainForm.CM.m_TrophyForm.ReloadCompetitions();
			Preset();
		}
	}

	private void buttonLeaguePlayerPlus_Click(object sender, EventArgs e)
	{
		foreach (Team playingTeam in m_CurrentLeague.PlayingTeams)
		{
			foreach (TeamPlayer item in playingTeam.Roster)
			{
				item.Player.ChangeSkills(1);
			}
		}
	}

	private void buttonLeaguePlayerMinus_Click(object sender, EventArgs e)
	{
		foreach (Team playingTeam in m_CurrentLeague.PlayingTeams)
		{
			foreach (TeamPlayer item in playingTeam.Roster)
			{
				item.Player.ChangeSkills(-1);
			}
		}
	}

	private void numericBoardOutcome1_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentLeague.boardoutcomes[0] = (int)numericBoardOutcome1.Value;
	}

	private void numericBoardOutcome2_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentLeague.boardoutcomes[1] = (int)numericBoardOutcome2.Value;
	}

	private void numericBoardOutcome3_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentLeague.boardoutcomes[2] = (int)numericBoardOutcome3.Value;
	}

	private void numericBoardOutcome4_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentLeague.boardoutcomes[3] = (int)numericBoardOutcome4.Value;
	}

	private void numericBoardOutcome5_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentLeague.boardoutcomes[4] = (int)numericBoardOutcome5.Value;
	}

	private void button1_Click(object sender, EventArgs e)
	{
		string text = FifaEnvironment.TempFolder + "\\2017";
		foreach (Kit kit in FifaEnvironment.Kits)
		{
			Team team = kit.Team;
			if (team != null && team.League == m_CurrentLeague)
			{
				int kittype = kit.kittype;
				Bitmap[] kitTextures = kit.GetKitTextures();
				string filename = text + "\\j_" + team.Id + "_" + kittype + ".png";
				kitTextures[1].Save(filename, ImageFormat.Png);
				filename = text + "\\s_" + team.Id + "_" + kittype + ".png";
				kitTextures[3].Save(filename, ImageFormat.Png);
				for (int i = 0; i < kitTextures.Length; i++)
				{
					kitTextures[i].Dispose();
				}
			}
		}
	}

	private void comboLeaguePrestige_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_Locked && comboLeagueCountry.SelectedIndex >= 0)
		{
			m_CurrentLeague.Prestige = (ELeaguePrestige)comboLeaguePrestige.SelectedIndex;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.LeagueForm));
		this.flowPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.groupBoxTeams = new System.Windows.Forms.GroupBox();
		this.listViewPlayingTeams = new System.Windows.Forms.ListView();
		this.imageListTeamLogos = new System.Windows.Forms.ImageList(this.components);
		this.toolStripTeamAvailable = new System.Windows.Forms.ToolStrip();
		this.separatorBegin = new System.Windows.Forms.ToolStripSeparator();
		this.comboTeamAvailable = new System.Windows.Forms.ToolStripComboBox();
		this.separatorButtons = new System.Windows.Forms.ToolStripSeparator();
		this.buttonAddTeam = new System.Windows.Forms.ToolStripButton();
		this.buttonReplaceTeam = new System.Windows.Forms.ToolStripButton();
		this.buttonRemoveTeam = new System.Windows.Forms.ToolStripButton();
		this.separatorShowLogo = new System.Windows.Forms.ToolStripSeparator();
		this.checkShowTeamLogo = new System.Windows.Forms.ToolStripButton();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.buttonreplicateLeagueLogo512x128 = new System.Windows.Forms.Button();
		this.viewer2DLeague512x128Logo = new FifaControls.Viewer2D();
		this.buttonreplicateLeagueSmallLogo = new System.Windows.Forms.Button();
		this.buttonreplicateLeagueTinyLogo = new System.Windows.Forms.Button();
		this.viewer2DLeagueTinyLogo = new FifaControls.Viewer2D();
		this.viewer2DLeagueSmallLogo = new FifaControls.Viewer2D();
		this.viewer2DLeagueAnimLogo = new FifaControls.Viewer2D();
		this.groupBoxNames = new System.Windows.Forms.GroupBox();
		this.comboLeaguePrestige = new System.Windows.Forms.ComboBox();
		this.label6 = new System.Windows.Forms.Label();
		this.textLeagueFullName = new System.Windows.Forms.TextBox();
		this.leagueBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.labelLeagueFullName = new System.Windows.Forms.Label();
		this.labelLeagueId = new System.Windows.Forms.Label();
		this.buttonGetId = new System.Windows.Forms.Button();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.numericBoardOutcome5 = new System.Windows.Forms.NumericUpDown();
		this.label4 = new System.Windows.Forms.Label();
		this.numericBoardOutcome4 = new System.Windows.Forms.NumericUpDown();
		this.label5 = new System.Windows.Forms.Label();
		this.numericBoardOutcome3 = new System.Windows.Forms.NumericUpDown();
		this.label3 = new System.Windows.Forms.Label();
		this.numericBoardOutcome2 = new System.Windows.Forms.NumericUpDown();
		this.label2 = new System.Windows.Forms.Label();
		this.numericBoardOutcome1 = new System.Windows.Forms.NumericUpDown();
		this.label1 = new System.Windows.Forms.Label();
		this.textLeagueShortName = new System.Windows.Forms.TextBox();
		this.labelLeagueShortName = new System.Windows.Forms.Label();
		this.textDatabaseLeagueName = new System.Windows.Forms.TextBox();
		this.comboLeagueCountry = new System.Windows.Forms.ComboBox();
		this.labelDatabaseLeagueName = new System.Windows.Forms.Label();
		this.numericLeagueId = new System.Windows.Forms.NumericUpDown();
		this.numericLeagueLevel = new System.Windows.Forms.NumericUpDown();
		this.labelCountry = new System.Windows.Forms.Label();
		this.labelLeagueLevel = new System.Windows.Forms.Label();
		this.groupLeaguePlayerTuning = new System.Windows.Forms.GroupBox();
		this.buttonLeaguePlayerMinus = new System.Windows.Forms.Button();
		this.buttonLeaguePlayerPlus = new System.Windows.Forms.Button();
		this.groupSwitchLeagues = new System.Windows.Forms.GroupBox();
		this.labelThisLeague = new System.Windows.Forms.Label();
		this.buttonSwitchLeagueIds = new System.Windows.Forms.Button();
		this.comboSwitchLeagues = new System.Windows.Forms.ComboBox();
		this.button1 = new System.Windows.Forms.Button();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.countryListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.flowPanel.SuspendLayout();
		this.groupBoxTeams.SuspendLayout();
		this.toolStripTeamAvailable.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBoxNames.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.leagueBindingSource).BeginInit();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericLeagueId).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericLeagueLevel).BeginInit();
		this.groupLeaguePlayerTuning.SuspendLayout();
		this.groupSwitchLeagues.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).BeginInit();
		base.SuspendLayout();
		this.flowPanel.AutoScroll = true;
		this.flowPanel.Controls.Add(this.groupBoxTeams);
		this.flowPanel.Controls.Add(this.groupBox3);
		this.flowPanel.Controls.Add(this.groupBoxNames);
		this.flowPanel.Controls.Add(this.groupLeaguePlayerTuning);
		this.flowPanel.Controls.Add(this.groupSwitchLeagues);
		this.flowPanel.Controls.Add(this.button1);
		this.flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowPanel.Location = new System.Drawing.Point(0, 25);
		this.flowPanel.Name = "flowPanel";
		this.flowPanel.Size = new System.Drawing.Size(1165, 755);
		this.flowPanel.TabIndex = 2;
		this.groupBoxTeams.Controls.Add(this.listViewPlayingTeams);
		this.groupBoxTeams.Controls.Add(this.toolStripTeamAvailable);
		this.groupBoxTeams.Location = new System.Drawing.Point(3, 3);
		this.groupBoxTeams.Name = "groupBoxTeams";
		this.groupBoxTeams.Size = new System.Drawing.Size(467, 454);
		this.groupBoxTeams.TabIndex = 0;
		this.groupBoxTeams.TabStop = false;
		this.groupBoxTeams.Text = "Teams";
		this.listViewPlayingTeams.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewPlayingTeams.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewPlayingTeams.FullRowSelect = true;
		this.listViewPlayingTeams.GridLines = true;
		this.listViewPlayingTeams.HideSelection = false;
		this.listViewPlayingTeams.LargeImageList = this.imageListTeamLogos;
		this.listViewPlayingTeams.Location = new System.Drawing.Point(3, 41);
		this.listViewPlayingTeams.MultiSelect = false;
		this.listViewPlayingTeams.Name = "listViewPlayingTeams";
		this.listViewPlayingTeams.Size = new System.Drawing.Size(461, 410);
		this.listViewPlayingTeams.TabIndex = 0;
		this.listViewPlayingTeams.UseCompatibleStateImageBehavior = false;
		this.listViewPlayingTeams.DoubleClick += new System.EventHandler(listViewPlayingTeams_DoubleClick);
		this.imageListTeamLogos.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
		this.imageListTeamLogos.ImageSize = new System.Drawing.Size(32, 32);
		this.imageListTeamLogos.TransparentColor = System.Drawing.Color.Transparent;
		this.toolStripTeamAvailable.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripTeamAvailable.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.separatorBegin, this.comboTeamAvailable, this.separatorButtons, this.buttonAddTeam, this.buttonReplaceTeam, this.buttonRemoveTeam, this.separatorShowLogo, this.checkShowTeamLogo });
		this.toolStripTeamAvailable.Location = new System.Drawing.Point(3, 16);
		this.toolStripTeamAvailable.Name = "toolStripTeamAvailable";
		this.toolStripTeamAvailable.Size = new System.Drawing.Size(461, 25);
		this.toolStripTeamAvailable.TabIndex = 124;
		this.separatorBegin.Name = "separatorBegin";
		this.separatorBegin.Size = new System.Drawing.Size(6, 25);
		this.comboTeamAvailable.DropDownHeight = 256;
		this.comboTeamAvailable.IntegralHeight = false;
		this.comboTeamAvailable.MaxDropDownItems = 16;
		this.comboTeamAvailable.Name = "comboTeamAvailable";
		this.comboTeamAvailable.Size = new System.Drawing.Size(150, 25);
		this.separatorButtons.Name = "separatorButtons";
		this.separatorButtons.Size = new System.Drawing.Size(6, 25);
		this.buttonAddTeam.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAddTeam.Image = (System.Drawing.Image)resources.GetObject("buttonAddTeam.Image");
		this.buttonAddTeam.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddTeam.Name = "buttonAddTeam";
		this.buttonAddTeam.Size = new System.Drawing.Size(23, 22);
		this.buttonAddTeam.Text = "Add";
		this.buttonAddTeam.Click += new System.EventHandler(buttonAddTeam_Click);
		this.buttonReplaceTeam.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonReplaceTeam.Image = (System.Drawing.Image)resources.GetObject("buttonReplaceTeam.Image");
		this.buttonReplaceTeam.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonReplaceTeam.Name = "buttonReplaceTeam";
		this.buttonReplaceTeam.Size = new System.Drawing.Size(23, 22);
		this.buttonReplaceTeam.Text = "Replace";
		this.buttonReplaceTeam.Click += new System.EventHandler(buttonReplaceTeam_Click);
		this.buttonRemoveTeam.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemoveTeam.Image = (System.Drawing.Image)resources.GetObject("buttonRemoveTeam.Image");
		this.buttonRemoveTeam.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemoveTeam.Name = "buttonRemoveTeam";
		this.buttonRemoveTeam.Size = new System.Drawing.Size(23, 22);
		this.buttonRemoveTeam.Text = "Remove";
		this.buttonRemoveTeam.Click += new System.EventHandler(buttonRemoveTeam_Click);
		this.separatorShowLogo.Name = "separatorShowLogo";
		this.separatorShowLogo.Size = new System.Drawing.Size(6, 25);
		this.checkShowTeamLogo.Checked = true;
		this.checkShowTeamLogo.CheckOnClick = true;
		this.checkShowTeamLogo.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkShowTeamLogo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.checkShowTeamLogo.Image = (System.Drawing.Image)resources.GetObject("checkShowTeamLogo.Image");
		this.checkShowTeamLogo.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.checkShowTeamLogo.Name = "checkShowTeamLogo";
		this.checkShowTeamLogo.Size = new System.Drawing.Size(102, 22);
		this.checkShowTeamLogo.Text = "Show Team Logo";
		this.checkShowTeamLogo.Click += new System.EventHandler(checkShowTeamLogo_CheckedChanged);
		this.groupBox3.Controls.Add(this.buttonreplicateLeagueLogo512x128);
		this.groupBox3.Controls.Add(this.viewer2DLeague512x128Logo);
		this.groupBox3.Controls.Add(this.buttonreplicateLeagueSmallLogo);
		this.groupBox3.Controls.Add(this.buttonreplicateLeagueTinyLogo);
		this.groupBox3.Controls.Add(this.viewer2DLeagueTinyLogo);
		this.groupBox3.Controls.Add(this.viewer2DLeagueSmallLogo);
		this.groupBox3.Controls.Add(this.viewer2DLeagueAnimLogo);
		this.groupBox3.Location = new System.Drawing.Point(476, 3);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(532, 454);
		this.groupBox3.TabIndex = 3;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Logos";
		this.buttonreplicateLeagueLogo512x128.Location = new System.Drawing.Point(138, 426);
		this.buttonreplicateLeagueLogo512x128.Name = "buttonreplicateLeagueLogo512x128";
		this.buttonreplicateLeagueLogo512x128.Size = new System.Drawing.Size(70, 23);
		this.buttonreplicateLeagueLogo512x128.TabIndex = 159;
		this.buttonreplicateLeagueLogo512x128.Text = "Replicate";
		this.buttonreplicateLeagueLogo512x128.UseVisualStyleBackColor = true;
		this.buttonreplicateLeagueLogo512x128.Click += new System.EventHandler(buttonreplicateLeagueLogo512x128_Click);
		this.viewer2DLeague512x128Logo.AutoTransparency = true;
		this.viewer2DLeague512x128Logo.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DLeague512x128Logo.ButtonStripVisible = true;
		this.viewer2DLeague512x128Logo.CurrentBitmap = null;
		this.viewer2DLeague512x128Logo.ExtendedFormat = false;
		this.viewer2DLeague512x128Logo.FullSizeButton = false;
		this.viewer2DLeague512x128Logo.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DLeague512x128Logo.ImageSize = new System.Drawing.Size(512, 128);
		this.viewer2DLeague512x128Logo.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DLeague512x128Logo.Location = new System.Drawing.Point(6, 297);
		this.viewer2DLeague512x128Logo.Name = "viewer2DLeague512x128Logo";
		this.viewer2DLeague512x128Logo.RemoveButton = true;
		this.viewer2DLeague512x128Logo.ShowButton = false;
		this.viewer2DLeague512x128Logo.ShowButtonChecked = true;
		this.viewer2DLeague512x128Logo.Size = new System.Drawing.Size(512, 153);
		this.viewer2DLeague512x128Logo.TabIndex = 158;
		this.buttonreplicateLeagueSmallLogo.Location = new System.Drawing.Point(399, 268);
		this.buttonreplicateLeagueSmallLogo.Name = "buttonreplicateLeagueSmallLogo";
		this.buttonreplicateLeagueSmallLogo.Size = new System.Drawing.Size(70, 23);
		this.buttonreplicateLeagueSmallLogo.TabIndex = 158;
		this.buttonreplicateLeagueSmallLogo.Text = "Replicate";
		this.buttonreplicateLeagueSmallLogo.UseVisualStyleBackColor = true;
		this.buttonreplicateLeagueSmallLogo.Visible = false;
		this.buttonreplicateLeagueSmallLogo.Click += new System.EventHandler(buttonreplicateLeagueSmallLogo_Click);
		this.buttonreplicateLeagueTinyLogo.Location = new System.Drawing.Point(399, 85);
		this.buttonreplicateLeagueTinyLogo.Name = "buttonreplicateLeagueTinyLogo";
		this.buttonreplicateLeagueTinyLogo.Size = new System.Drawing.Size(75, 23);
		this.buttonreplicateLeagueTinyLogo.TabIndex = 3;
		this.buttonreplicateLeagueTinyLogo.Text = "Replicate";
		this.buttonreplicateLeagueTinyLogo.UseVisualStyleBackColor = true;
		this.buttonreplicateLeagueTinyLogo.Click += new System.EventHandler(buttonreplicateLeagueTinyLogo_Click);
		this.viewer2DLeagueTinyLogo.AutoTransparency = true;
		this.viewer2DLeagueTinyLogo.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DLeagueTinyLogo.ButtonStripVisible = true;
		this.viewer2DLeagueTinyLogo.CurrentBitmap = null;
		this.viewer2DLeagueTinyLogo.ExtendedFormat = false;
		this.viewer2DLeagueTinyLogo.FullSizeButton = false;
		this.viewer2DLeagueTinyLogo.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DLeagueTinyLogo.ImageSize = new System.Drawing.Size(256, 64);
		this.viewer2DLeagueTinyLogo.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DLeagueTinyLogo.Location = new System.Drawing.Point(268, 19);
		this.viewer2DLeagueTinyLogo.Name = "viewer2DLeagueTinyLogo";
		this.viewer2DLeagueTinyLogo.RemoveButton = true;
		this.viewer2DLeagueTinyLogo.ShowButton = false;
		this.viewer2DLeagueTinyLogo.ShowButtonChecked = true;
		this.viewer2DLeagueTinyLogo.Size = new System.Drawing.Size(256, 89);
		this.viewer2DLeagueTinyLogo.TabIndex = 2;
		this.viewer2DLeagueSmallLogo.AutoTransparency = true;
		this.viewer2DLeagueSmallLogo.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DLeagueSmallLogo.ButtonStripVisible = true;
		this.viewer2DLeagueSmallLogo.CurrentBitmap = null;
		this.viewer2DLeagueSmallLogo.ExtendedFormat = false;
		this.viewer2DLeagueSmallLogo.FullSizeButton = false;
		this.viewer2DLeagueSmallLogo.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DLeagueSmallLogo.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DLeagueSmallLogo.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DLeagueSmallLogo.Location = new System.Drawing.Point(268, 114);
		this.viewer2DLeagueSmallLogo.Name = "viewer2DLeagueSmallLogo";
		this.viewer2DLeagueSmallLogo.RemoveButton = true;
		this.viewer2DLeagueSmallLogo.ShowButton = false;
		this.viewer2DLeagueSmallLogo.ShowButtonChecked = true;
		this.viewer2DLeagueSmallLogo.Size = new System.Drawing.Size(201, 177);
		this.viewer2DLeagueSmallLogo.TabIndex = 157;
		this.viewer2DLeagueSmallLogo.Visible = false;
		this.viewer2DLeagueAnimLogo.AutoTransparency = true;
		this.viewer2DLeagueAnimLogo.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DLeagueAnimLogo.ButtonStripVisible = true;
		this.viewer2DLeagueAnimLogo.CurrentBitmap = null;
		this.viewer2DLeagueAnimLogo.ExtendedFormat = false;
		this.viewer2DLeagueAnimLogo.FullSizeButton = false;
		this.viewer2DLeagueAnimLogo.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DLeagueAnimLogo.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DLeagueAnimLogo.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DLeagueAnimLogo.Location = new System.Drawing.Point(6, 19);
		this.viewer2DLeagueAnimLogo.Name = "viewer2DLeagueAnimLogo";
		this.viewer2DLeagueAnimLogo.RemoveButton = true;
		this.viewer2DLeagueAnimLogo.ShowButton = false;
		this.viewer2DLeagueAnimLogo.ShowButtonChecked = true;
		this.viewer2DLeagueAnimLogo.Size = new System.Drawing.Size(256, 281);
		this.viewer2DLeagueAnimLogo.TabIndex = 156;
		this.groupBoxNames.Controls.Add(this.comboLeaguePrestige);
		this.groupBoxNames.Controls.Add(this.label6);
		this.groupBoxNames.Controls.Add(this.textLeagueFullName);
		this.groupBoxNames.Controls.Add(this.labelLeagueFullName);
		this.groupBoxNames.Controls.Add(this.labelLeagueId);
		this.groupBoxNames.Controls.Add(this.buttonGetId);
		this.groupBoxNames.Controls.Add(this.groupBox1);
		this.groupBoxNames.Controls.Add(this.textLeagueShortName);
		this.groupBoxNames.Controls.Add(this.labelLeagueShortName);
		this.groupBoxNames.Controls.Add(this.textDatabaseLeagueName);
		this.groupBoxNames.Controls.Add(this.comboLeagueCountry);
		this.groupBoxNames.Controls.Add(this.labelDatabaseLeagueName);
		this.groupBoxNames.Controls.Add(this.numericLeagueId);
		this.groupBoxNames.Controls.Add(this.numericLeagueLevel);
		this.groupBoxNames.Controls.Add(this.labelCountry);
		this.groupBoxNames.Controls.Add(this.labelLeagueLevel);
		this.groupBoxNames.Location = new System.Drawing.Point(3, 463);
		this.groupBoxNames.Name = "groupBoxNames";
		this.groupBoxNames.Size = new System.Drawing.Size(531, 202);
		this.groupBoxNames.TabIndex = 1;
		this.groupBoxNames.TabStop = false;
		this.groupBoxNames.Text = "Names and Other Information";
		this.comboLeaguePrestige.FormattingEnabled = true;
		this.comboLeaguePrestige.Items.AddRange(new object[13]
		{
			"Top Prestige    \t(England Spain Germany Italy)", "Prestige Level  2 \t(France)", "Prestige Level  3\t(Argentina Brazil)", "Prestige Level  4\t(Russia Portugal Turkey)", "Prestige Level  5 \t(Holland)", "Prestige Level  6 \t(Mexico England2)", "Prestige Level  7 \t(Belgium Germany2 Colombia)", "Prestige Level  8 \t(USA Chile)", "Prestige Level  9 \t(Scotland Italy2 Spain2)", "Prestige Level 10 \t(Denmark Norway Switzerland France2)",
			"Prestige Level 11 \t(Poland Austria Korea)", "Prestige Level 12\t(Australia Sweden England3 Ireland)", "Undefined"
		});
		this.comboLeaguePrestige.Location = new System.Drawing.Point(90, 170);
		this.comboLeaguePrestige.Name = "comboLeaguePrestige";
		this.comboLeaguePrestige.Size = new System.Drawing.Size(426, 21);
		this.comboLeaguePrestige.TabIndex = 162;
		this.comboLeaguePrestige.SelectedIndexChanged += new System.EventHandler(comboLeaguePrestige_SelectedIndexChanged);
		this.label6.AutoSize = true;
		this.label6.BackColor = System.Drawing.SystemColors.Control;
		this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label6.Location = new System.Drawing.Point(9, 174);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(45, 13);
		this.label6.TabIndex = 161;
		this.label6.Text = "Prestige";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textLeagueFullName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.leagueBindingSource, "LongName", true));
		this.textLeagueFullName.Location = new System.Drawing.Point(91, 60);
		this.textLeagueFullName.Name = "textLeagueFullName";
		this.textLeagueFullName.Size = new System.Drawing.Size(192, 20);
		this.textLeagueFullName.TabIndex = 116;
		this.leagueBindingSource.DataSource = typeof(FifaLibrary.League);
		this.labelLeagueFullName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeagueFullName.Location = new System.Drawing.Point(6, 60);
		this.labelLeagueFullName.Name = "labelLeagueFullName";
		this.labelLeagueFullName.Size = new System.Drawing.Size(79, 20);
		this.labelLeagueFullName.TabIndex = 120;
		this.labelLeagueFullName.Text = "Long Name";
		this.labelLeagueFullName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelLeagueId.AutoSize = true;
		this.labelLeagueId.BackColor = System.Drawing.Color.Transparent;
		this.labelLeagueId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeagueId.Location = new System.Drawing.Point(6, 90);
		this.labelLeagueId.Name = "labelLeagueId";
		this.labelLeagueId.Size = new System.Drawing.Size(55, 13);
		this.labelLeagueId.TabIndex = 152;
		this.labelLeagueId.Text = "League Id";
		this.labelLeagueId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.buttonGetId.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonGetId.BackgroundImage");
		this.buttonGetId.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.buttonGetId.Location = new System.Drawing.Point(229, 90);
		this.buttonGetId.Name = "buttonGetId";
		this.buttonGetId.Size = new System.Drawing.Size(25, 23);
		this.buttonGetId.TabIndex = 153;
		this.buttonGetId.UseVisualStyleBackColor = true;
		this.buttonGetId.Click += new System.EventHandler(buttonGetId_Click);
		this.groupBox1.Controls.Add(this.numericBoardOutcome5);
		this.groupBox1.Controls.Add(this.label4);
		this.groupBox1.Controls.Add(this.numericBoardOutcome4);
		this.groupBox1.Controls.Add(this.label5);
		this.groupBox1.Controls.Add(this.numericBoardOutcome3);
		this.groupBox1.Controls.Add(this.label3);
		this.groupBox1.Controls.Add(this.numericBoardOutcome2);
		this.groupBox1.Controls.Add(this.label2);
		this.groupBox1.Controls.Add(this.numericBoardOutcome1);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Location = new System.Drawing.Point(292, 15);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(239, 148);
		this.groupBox1.TabIndex = 160;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Position necessary to achieve objectives";
		this.numericBoardOutcome5.Location = new System.Drawing.Point(164, 122);
		this.numericBoardOutcome5.Maximum = new decimal(new int[4] { 24, 0, 0, 0 });
		this.numericBoardOutcome5.Name = "numericBoardOutcome5";
		this.numericBoardOutcome5.Size = new System.Drawing.Size(60, 20);
		this.numericBoardOutcome5.TabIndex = 9;
		this.numericBoardOutcome5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBoardOutcome5.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBoardOutcome5.ValueChanged += new System.EventHandler(numericBoardOutcome5_ValueChanged);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(6, 124);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(154, 13);
		this.label4.TabIndex = 8;
		this.label4.Text = "Avoid Relegation or Low Class.";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericBoardOutcome4.Location = new System.Drawing.Point(164, 97);
		this.numericBoardOutcome4.Maximum = new decimal(new int[4] { 24, 0, 0, 0 });
		this.numericBoardOutcome4.Name = "numericBoardOutcome4";
		this.numericBoardOutcome4.Size = new System.Drawing.Size(60, 20);
		this.numericBoardOutcome4.TabIndex = 7;
		this.numericBoardOutcome4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBoardOutcome4.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBoardOutcome4.ValueChanged += new System.EventHandler(numericBoardOutcome4_ValueChanged);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(6, 99);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(108, 13);
		this.label5.TabIndex = 6;
		this.label5.Text = "Medium Classification";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericBoardOutcome3.Location = new System.Drawing.Point(164, 72);
		this.numericBoardOutcome3.Maximum = new decimal(new int[4] { 24, 0, 0, 0 });
		this.numericBoardOutcome3.Name = "numericBoardOutcome3";
		this.numericBoardOutcome3.Size = new System.Drawing.Size(60, 20);
		this.numericBoardOutcome3.TabIndex = 5;
		this.numericBoardOutcome3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBoardOutcome3.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBoardOutcome3.ValueChanged += new System.EventHandler(numericBoardOutcome3_ValueChanged);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(6, 74);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(148, 13);
		this.label3.TabIndex = 4;
		this.label3.Text = "Europa League or High Class.";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericBoardOutcome2.Location = new System.Drawing.Point(164, 47);
		this.numericBoardOutcome2.Maximum = new decimal(new int[4] { 24, 0, 0, 0 });
		this.numericBoardOutcome2.Name = "numericBoardOutcome2";
		this.numericBoardOutcome2.Size = new System.Drawing.Size(60, 20);
		this.numericBoardOutcome2.TabIndex = 3;
		this.numericBoardOutcome2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBoardOutcome2.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBoardOutcome2.ValueChanged += new System.EventHandler(numericBoardOutcome2_ValueChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(6, 49);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(145, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Champions League or Playoff";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericBoardOutcome1.Location = new System.Drawing.Point(164, 22);
		this.numericBoardOutcome1.Maximum = new decimal(new int[4] { 24, 0, 0, 0 });
		this.numericBoardOutcome1.Name = "numericBoardOutcome1";
		this.numericBoardOutcome1.Size = new System.Drawing.Size(60, 20);
		this.numericBoardOutcome1.TabIndex = 1;
		this.numericBoardOutcome1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBoardOutcome1.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericBoardOutcome1.ValueChanged += new System.EventHandler(numericBoardOutcome1_ValueChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(6, 24);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(119, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Win or Direct Promotion";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textLeagueShortName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.leagueBindingSource, "ShortName", true));
		this.textLeagueShortName.Location = new System.Drawing.Point(91, 37);
		this.textLeagueShortName.Name = "textLeagueShortName";
		this.textLeagueShortName.Size = new System.Drawing.Size(192, 20);
		this.textLeagueShortName.TabIndex = 1;
		this.textLeagueShortName.TextChanged += new System.EventHandler(textLeagueShortName_TextChanged);
		this.labelLeagueShortName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeagueShortName.Location = new System.Drawing.Point(6, 37);
		this.labelLeagueShortName.Name = "labelLeagueShortName";
		this.labelLeagueShortName.Size = new System.Drawing.Size(79, 20);
		this.labelLeagueShortName.TabIndex = 119;
		this.labelLeagueShortName.Text = "Name";
		this.labelLeagueShortName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textDatabaseLeagueName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.leagueBindingSource, "leaguename", true));
		this.textDatabaseLeagueName.Location = new System.Drawing.Point(91, 15);
		this.textDatabaseLeagueName.Name = "textDatabaseLeagueName";
		this.textDatabaseLeagueName.Size = new System.Drawing.Size(192, 20);
		this.textDatabaseLeagueName.TabIndex = 0;
		this.comboLeagueCountry.Location = new System.Drawing.Point(90, 142);
		this.comboLeagueCountry.Name = "comboLeagueCountry";
		this.comboLeagueCountry.Size = new System.Drawing.Size(193, 21);
		this.comboLeagueCountry.TabIndex = 3;
		this.comboLeagueCountry.SelectedIndexChanged += new System.EventHandler(comboLeagueCountry_SelectedIndexChanged);
		this.labelDatabaseLeagueName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDatabaseLeagueName.Location = new System.Drawing.Point(6, 15);
		this.labelDatabaseLeagueName.Name = "labelDatabaseLeagueName";
		this.labelDatabaseLeagueName.Size = new System.Drawing.Size(97, 20);
		this.labelDatabaseLeagueName.TabIndex = 54;
		this.labelDatabaseLeagueName.Text = "Database Name";
		this.labelDatabaseLeagueName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericLeagueId.Location = new System.Drawing.Point(91, 90);
		this.numericLeagueId.Maximum = new decimal(new int[4] { 200000, 0, 0, 0 });
		this.numericLeagueId.Name = "numericLeagueId";
		this.numericLeagueId.Size = new System.Drawing.Size(132, 20);
		this.numericLeagueId.TabIndex = 0;
		this.numericLeagueId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericLeagueId.ValueChanged += new System.EventHandler(numericLeagueId_ValueChanged);
		this.numericLeagueLevel.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.leagueBindingSource, "level", true));
		this.numericLeagueLevel.Location = new System.Drawing.Point(91, 116);
		this.numericLeagueLevel.Maximum = new decimal(new int[4] { 7, 0, 0, 0 });
		this.numericLeagueLevel.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericLeagueLevel.Name = "numericLeagueLevel";
		this.numericLeagueLevel.Size = new System.Drawing.Size(66, 20);
		this.numericLeagueLevel.TabIndex = 1;
		this.numericLeagueLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericLeagueLevel.ThousandsSeparator = true;
		this.numericLeagueLevel.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.labelCountry.AutoSize = true;
		this.labelCountry.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelCountry.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelCountry.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCountry.Location = new System.Drawing.Point(6, 145);
		this.labelCountry.Name = "labelCountry";
		this.labelCountry.Size = new System.Drawing.Size(43, 13);
		this.labelCountry.TabIndex = 123;
		this.labelCountry.Text = "Country";
		this.labelCountry.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCountry.DoubleClick += new System.EventHandler(labelCountry_DoubleClick);
		this.labelLeagueLevel.AutoSize = true;
		this.labelLeagueLevel.BackColor = System.Drawing.SystemColors.Control;
		this.labelLeagueLevel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelLeagueLevel.Location = new System.Drawing.Point(6, 118);
		this.labelLeagueLevel.Name = "labelLeagueLevel";
		this.labelLeagueLevel.Size = new System.Drawing.Size(33, 13);
		this.labelLeagueLevel.TabIndex = 108;
		this.labelLeagueLevel.Text = "Level";
		this.labelLeagueLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupLeaguePlayerTuning.Controls.Add(this.buttonLeaguePlayerMinus);
		this.groupLeaguePlayerTuning.Controls.Add(this.buttonLeaguePlayerPlus);
		this.groupLeaguePlayerTuning.Location = new System.Drawing.Point(540, 463);
		this.groupLeaguePlayerTuning.Name = "groupLeaguePlayerTuning";
		this.groupLeaguePlayerTuning.Size = new System.Drawing.Size(167, 139);
		this.groupLeaguePlayerTuning.TabIndex = 159;
		this.groupLeaguePlayerTuning.TabStop = false;
		this.groupLeaguePlayerTuning.Text = "Player Overall Tuning";
		this.buttonLeaguePlayerMinus.Cursor = System.Windows.Forms.Cursors.Hand;
		this.buttonLeaguePlayerMinus.Image = (System.Drawing.Image)resources.GetObject("buttonLeaguePlayerMinus.Image");
		this.buttonLeaguePlayerMinus.Location = new System.Drawing.Point(90, 43);
		this.buttonLeaguePlayerMinus.Name = "buttonLeaguePlayerMinus";
		this.buttonLeaguePlayerMinus.Size = new System.Drawing.Size(64, 64);
		this.buttonLeaguePlayerMinus.TabIndex = 1;
		this.buttonLeaguePlayerMinus.UseVisualStyleBackColor = false;
		this.buttonLeaguePlayerMinus.Click += new System.EventHandler(buttonLeaguePlayerMinus_Click);
		this.buttonLeaguePlayerPlus.Cursor = System.Windows.Forms.Cursors.Hand;
		this.buttonLeaguePlayerPlus.Image = (System.Drawing.Image)resources.GetObject("buttonLeaguePlayerPlus.Image");
		this.buttonLeaguePlayerPlus.Location = new System.Drawing.Point(11, 43);
		this.buttonLeaguePlayerPlus.Name = "buttonLeaguePlayerPlus";
		this.buttonLeaguePlayerPlus.Size = new System.Drawing.Size(64, 64);
		this.buttonLeaguePlayerPlus.TabIndex = 0;
		this.buttonLeaguePlayerPlus.UseVisualStyleBackColor = false;
		this.buttonLeaguePlayerPlus.Click += new System.EventHandler(buttonLeaguePlayerPlus_Click);
		this.groupSwitchLeagues.Controls.Add(this.labelThisLeague);
		this.groupSwitchLeagues.Controls.Add(this.buttonSwitchLeagueIds);
		this.groupSwitchLeagues.Controls.Add(this.comboSwitchLeagues);
		this.groupSwitchLeagues.Location = new System.Drawing.Point(713, 463);
		this.groupSwitchLeagues.Name = "groupSwitchLeagues";
		this.groupSwitchLeagues.Size = new System.Drawing.Size(237, 139);
		this.groupSwitchLeagues.TabIndex = 158;
		this.groupSwitchLeagues.TabStop = false;
		this.groupSwitchLeagues.Text = "Switch League Ids";
		this.groupSwitchLeagues.Visible = false;
		this.labelThisLeague.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelThisLeague.Enabled = false;
		this.labelThisLeague.Location = new System.Drawing.Point(24, 22);
		this.labelThisLeague.Name = "labelThisLeague";
		this.labelThisLeague.Size = new System.Drawing.Size(202, 21);
		this.labelThisLeague.TabIndex = 159;
		this.labelThisLeague.Text = "League name";
		this.labelThisLeague.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.buttonSwitchLeagueIds.Cursor = System.Windows.Forms.Cursors.Hand;
		this.buttonSwitchLeagueIds.Enabled = false;
		this.buttonSwitchLeagueIds.Image = (System.Drawing.Image)resources.GetObject("buttonSwitchLeagueIds.Image");
		this.buttonSwitchLeagueIds.Location = new System.Drawing.Point(87, 48);
		this.buttonSwitchLeagueIds.Name = "buttonSwitchLeagueIds";
		this.buttonSwitchLeagueIds.Size = new System.Drawing.Size(71, 54);
		this.buttonSwitchLeagueIds.TabIndex = 158;
		this.buttonSwitchLeagueIds.UseVisualStyleBackColor = true;
		this.buttonSwitchLeagueIds.Click += new System.EventHandler(buttonSwitchLeagueIds_Click);
		this.comboSwitchLeagues.FormattingEnabled = true;
		this.comboSwitchLeagues.Location = new System.Drawing.Point(24, 108);
		this.comboSwitchLeagues.Name = "comboSwitchLeagues";
		this.comboSwitchLeagues.Size = new System.Drawing.Size(202, 21);
		this.comboSwitchLeagues.TabIndex = 157;
		this.comboSwitchLeagues.SelectedIndexChanged += new System.EventHandler(comboSwitchLeagues_SelectedIndexChanged);
		this.button1.Location = new System.Drawing.Point(956, 463);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 160;
		this.button1.Text = "Export Kits";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Visible = false;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = false;
		this.pickUpControl.CreateButtonEnabled = true;
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
		this.pickUpControl.Size = new System.Drawing.Size(1165, 25);
		this.pickUpControl.TabIndex = 1;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		this.countryListBindingSource.DataSource = typeof(FifaLibrary.CountryList);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1165, 780);
		base.Controls.Add(this.flowPanel);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "LeagueForm";
		this.Text = "LeagueForm";
		base.Load += new System.EventHandler(LeagueForm_Load);
		this.flowPanel.ResumeLayout(false);
		this.groupBoxTeams.ResumeLayout(false);
		this.groupBoxTeams.PerformLayout();
		this.toolStripTeamAvailable.ResumeLayout(false);
		this.toolStripTeamAvailable.PerformLayout();
		this.groupBox3.ResumeLayout(false);
		this.groupBoxNames.ResumeLayout(false);
		this.groupBoxNames.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.leagueBindingSource).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBoardOutcome1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericLeagueId).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericLeagueLevel).EndInit();
		this.groupLeaguePlayerTuning.ResumeLayout(false);
		this.groupSwitchLeagues.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.countryListBindingSource).EndInit();
		base.ResumeLayout(false);
	}
}
