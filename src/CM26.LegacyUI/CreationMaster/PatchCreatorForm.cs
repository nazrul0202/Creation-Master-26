using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using FifaLibrary;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace CreationMaster;

public class PatchCreatorForm : Form
{
	public enum EPatchType
	{
		Country,
		League,
		Team,
		Player,
		Kit,
		Referee,
		Stadium,
		Formation,
		Ball,
		Adboard,
		NumberFont,
		NameFont,
		Shoes,
		GKGloves,
		Net,
		MowingPattern
	}

	private string m_TempFolder;

	private EPatchType m_PatchType;

	private DataSet m_FifaDataSet = new DataSet("FIFA14");

	private DataSet m_LangDataSet = new DataSet("LANG14");

	private int[] m_PlayerKeys;

	private int[] m_PlayerNameKeys;

	private int[] m_TeamKeys;

	private int[] m_KitKeys;

	private int[] m_LeagueKeys;

	private int[] m_CountryKeys;

	private int[] m_RefereeKeys;

	private int[] m_StadiumKeys;

	private int[] m_FormationKeys;

	private int[] m_LanguageKeys;

	private DataTable m_PlayersTable;

	private DataTable m_PlayernamesTable;

	private DataTable m_PreviousTeamTable;

	private DataTable m_DcPlayernamesTable;

	private DataTable m_PlayerLoansTable;

	private DataTable m_TeamsTable;

	private DataTable m_TeamkitsTable;

	private DataTable m_TeamplayerlinksTable;

	private DataTable m_LeaguesTable;

	private DataTable m_BoardOutcomesTable;

	private DataTable m_LeagueTeamLinksTable;

	private DataTable m_NationsTable;

	private DataTable m_AudionationsTable;

	private DataTable m_TeamStadiumLinksTable;

	private DataTable m_TeamFormationTeamStyleLinksTable;

	private DataTable m_RowTeamNationLinksTable;

	private DataTable m_TeamNationLinksTable;

	private DataTable m_RefereesTable;

	private DataTable m_StadiumsTable;

	private DataTable m_StadiumAssignmentsTable;

	private DataTable m_ManagerTable;

	private DataTable m_FormationsTable;

	private DataTable m_LanguageTable;

	private IContainer components;

	private SplitContainer splitContainer1;

	private Panel panelLeft;

	private TextBox textPatchVersion;

	private TextBox textPatchName;

	private TextBox textDescription;

	private Label label1;

	private Label labelPatchVersion;

	private Label labelPatchName;

	private ComboBox comboPatchType;

	private GroupBox groupPatchOptions;

	private TabControl tabPatchOptions;

	private TabPage pagePlayerOptions;

	private CheckBox checkPlayerShoes;

	private CheckBox checkPlayerMiniface;

	private CheckBox checkPlayerHead;

	private CheckBox checkPlayerDatabase;

	private TabPage pageTeamOptions;

	private CheckBox checkTeamAdboard;

	private CheckBox checkTeamBall;

	private CheckBox checkTeamLinkedPlayers;

	private CheckBox checkTeamKits;

	private CheckBox checkTeamFlags;

	private CheckBox checkTeamGuiBanner;

	private CheckBox checkTeamGuiLogo;

	private CheckBox checkTeamDatabase;

	private TabPage pageLeagueOptions;

	private CheckBox checkLeagueLinkedTournament;

	private CheckBox checkLeagueLinkedTeams;

	private CheckBox checkLeagueLogo;

	private CheckBox checkLeagueDatabase;

	private TabPage pageCountryOptions;

	private CheckBox checkCountryMiniFlag;

	private CheckBox checkCountryDatabase;

	private CheckBox checkCountryFlag;

	private TabPage pageRefereeOptions;

	private CheckBox checkRefereeDatabase;

	private TabPage pageStadiumOptions;

	private CheckBox checkStadiumMowingPattern;

	private CheckBox checkStadiumModel;

	private CheckBox checkStadiumPreview;

	private CheckBox checkStadiumDatabase;

	private CheckBox checkStadiumNet;

	private Label labelPatchType;

	private CheckBox checkCMSCompatible;

	private CheckBox checkTeamFormation;

	private CheckBox checkRefereeKits;

	private ListBox listSource;

	private ListView listViewDest;

	private ColumnHeader columnComment;

	private ColumnHeader columnType;

	private ColumnHeader columnId;

	private ColumnHeader columnItem;

	private ToolStrip toolAddRemove;

	private ToolStripButton buttonAddObject;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonRemoveObject;

	private TabPage pageGeneralOptions;

	private RadioButton radioIncludeOriginal;

	private RadioButton radioIncludePatched;

	private CheckBox checkCountryLeagues;

	private CheckBox checkContrynationalTeam;

	private CheckBox checkCountryTournaments;

	private TabPage pageKitOptions;

	private CheckBox checkKitNumbers;

	private CheckBox checkKitMinikits;

	private CheckBox checkKitDatabase;

	private CheckBox checkKitNameFonts;

	private CheckBox checkLeagueReferees;

	private CheckBox checkRefereeShoes;

	private CheckBox checkTeamStadium;

	private CheckBox checkPlayerGloves;

	private MenuStrip mainMenuStrip;

	private ToolStrip toolMain;

	private ToolStripMenuItem patchToolStripMenuItem;

	private ToolStripMenuItem newPatchToolStripMenuItem;

	private ToolStripMenuItem createPatchToolStripMenuItem;

	private ToolStripMenuItem openPatchToolStripMenuItem;

	private ToolStripMenuItem exitToolStripMenuItem;

	private ToolStripButton buttonNewPatch;

	private ToolStripButton buttonOpenPatch;

	private ToolStripButton buttonCreatePatch;

	private ToolStripButton buttonExit;

	private StatusStrip statusBar;

	private ToolStripStatusLabel statusLabel;

	private Patch m_PatchDataSet;

	private OpenFileDialog openFileDialog;

	private CheckBox checkKitTextures;

	private CheckBox checkCountryCardFlag;

	private CheckBox checkRefereeMiniFace;

	private CheckBox checkLeagueBall;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton buttonAddFile;

	private CheckBox checkCountryMap;

	private CheckBox checkCountryFlag512x512;

	public PatchCreatorForm()
	{
		InitializeComponent();
		m_TempFolder = FifaEnvironment.TempFolder + "\\Patch";
	}

	private void comboPatchType_SelectedIndexChanged(object sender, EventArgs e)
	{
		listSource.BeginUpdate();
		statusLabel.Text = "Loading ...";
		listSource.Items.Clear();
		EPatchType selectedIndex = (EPatchType)comboPatchType.SelectedIndex;
		listSource.Sorted = true;
		switch (selectedIndex)
		{
		case EPatchType.Player:
			listSource.Items.AddRange(FifaEnvironment.Players.ToArray());
			m_PatchType = EPatchType.Player;
			tabPatchOptions.SelectedTab = pagePlayerOptions;
			break;
		case EPatchType.Team:
			listSource.Items.AddRange(FifaEnvironment.Teams.ToArray());
			m_PatchType = EPatchType.Team;
			tabPatchOptions.SelectedTab = pageTeamOptions;
			break;
		case EPatchType.Kit:
			listSource.Items.AddRange(FifaEnvironment.Kits.ToArray());
			m_PatchType = EPatchType.Kit;
			tabPatchOptions.SelectedTab = pageKitOptions;
			break;
		case EPatchType.League:
			listSource.Items.AddRange(FifaEnvironment.Leagues.ToArray());
			m_PatchType = EPatchType.League;
			tabPatchOptions.SelectedTab = pageLeagueOptions;
			break;
		case EPatchType.Country:
			listSource.Items.AddRange(FifaEnvironment.Countries.ToArray());
			m_PatchType = EPatchType.Country;
			tabPatchOptions.SelectedTab = pageCountryOptions;
			break;
		case EPatchType.Referee:
			listSource.Items.AddRange(FifaEnvironment.Referees.ToArray());
			m_PatchType = EPatchType.Referee;
			tabPatchOptions.SelectedTab = pageRefereeOptions;
			break;
		case EPatchType.Stadium:
			listSource.Items.AddRange(FifaEnvironment.Stadiums.ToArray());
			m_PatchType = EPatchType.Stadium;
			tabPatchOptions.SelectedTab = pageStadiumOptions;
			break;
		case EPatchType.Formation:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.Formations.ToArray());
			m_PatchType = EPatchType.Formation;
			break;
		case EPatchType.Ball:
			listSource.Items.AddRange(FifaEnvironment.Balls.ToArray());
			m_PatchType = EPatchType.Ball;
			tabPatchOptions.SelectedTab = pageTeamOptions;
			break;
		case EPatchType.Adboard:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.Adboards.ToArray());
			m_PatchType = EPatchType.Adboard;
			tabPatchOptions.SelectedTab = pageTeamOptions;
			break;
		case EPatchType.NumberFont:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.NumberFonts.ToArray());
			m_PatchType = EPatchType.NumberFont;
			tabPatchOptions.SelectedTab = pageTeamOptions;
			break;
		case EPatchType.NameFont:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.NameFonts.ToArray());
			m_PatchType = EPatchType.NameFont;
			tabPatchOptions.SelectedTab = pageTeamOptions;
			break;
		case EPatchType.Shoes:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.Shoes.ToArray());
			m_PatchType = EPatchType.Shoes;
			tabPatchOptions.SelectedTab = pagePlayerOptions;
			break;
		case EPatchType.GKGloves:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.GkGloves.ToArray());
			m_PatchType = EPatchType.GKGloves;
			tabPatchOptions.SelectedTab = pagePlayerOptions;
			break;
		case EPatchType.Net:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.Nets.ToArray());
			m_PatchType = EPatchType.Net;
			tabPatchOptions.SelectedTab = pageStadiumOptions;
			break;
		case EPatchType.MowingPattern:
			listSource.Sorted = false;
			listSource.Items.AddRange(FifaEnvironment.MowingPatterns.ToArray());
			m_PatchType = EPatchType.MowingPattern;
			tabPatchOptions.SelectedTab = pageStadiumOptions;
			break;
		}
		listSource.EndUpdate();
		statusLabel.Text = "Ready";
	}

	private void buttonAdd_Click(object sender, EventArgs e)
	{
		if (listSource.SelectedItems.Count <= 0)
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		switch (m_PatchType)
		{
		case EPatchType.Player:
			foreach (Player selectedItem in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem);
			}
			break;
		case EPatchType.Team:
			foreach (Team selectedItem2 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem2);
			}
			break;
		case EPatchType.League:
			foreach (League selectedItem3 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem3);
			}
			break;
		case EPatchType.Country:
			foreach (Country selectedItem4 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem4);
			}
			break;
		case EPatchType.Stadium:
			foreach (Stadium selectedItem5 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem5);
			}
			break;
		case EPatchType.Referee:
			foreach (Referee selectedItem6 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem6);
			}
			break;
		case EPatchType.Formation:
			foreach (Formation selectedItem7 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem7);
			}
			break;
		case EPatchType.Ball:
			foreach (Ball selectedItem8 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem8);
			}
			break;
		case EPatchType.Adboard:
			foreach (Adboard selectedItem9 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem9);
			}
			break;
		case EPatchType.NumberFont:
			foreach (NumberFont selectedItem10 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem10);
			}
			break;
		case EPatchType.NameFont:
			foreach (NameFont selectedItem11 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem11);
			}
			break;
		case EPatchType.Net:
			foreach (Net selectedItem12 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem12);
			}
			break;
		case EPatchType.Shoes:
			foreach (Shoes selectedItem13 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem13);
			}
			break;
		case EPatchType.GKGloves:
			foreach (GkGloves selectedItem14 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem14);
			}
			break;
		case EPatchType.MowingPattern:
			foreach (MowingPattern selectedItem15 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem15);
			}
			break;
		case EPatchType.Kit:
			foreach (Kit selectedItem16 in listSource.SelectedItems)
			{
				AddToPatchList(selectedItem16);
			}
			break;
		}
		Cursor.Current = Cursors.Default;
		statusLabel.Text = "Ready";
		statusBar.Refresh();
	}

	private ListViewItem WriteToPatchList(IdObject obj)
	{
		return WriteToPatchList(obj, obj.Id, obj.ToString(), null);
	}

	private ListViewItem WriteToPatchList(IdObject obj, string name)
	{
		return WriteToPatchList(obj, obj.Id, name, null);
	}

	private ListViewItem WriteToPatchList(IdObject obj, int id, string name, string comment)
	{
		if (obj == null)
		{
			return null;
		}
		foreach (ListViewItem item in listViewDest.Items)
		{
			if (obj == item.Tag)
			{
				return null;
			}
		}
		string[] array = new string[4];
		array[1] = obj.GetType().Name;
		array[2] = id.ToString();
		array[3] = name;
		array[0] = comment;
		ListViewItem listViewItem2 = new ListViewItem(array);
		listViewItem2.Tag = obj;
		listViewDest.Items.Add(listViewItem2);
		statusLabel.Text = "Adding " + name;
		statusBar.Refresh();
		return listViewItem2;
	}

	private ListViewItem AddToPatchList(string desc)
	{
		if (desc == null)
		{
			return null;
		}
		foreach (ListViewItem item in listViewDest.Items)
		{
			if (desc == item.Tag.ToString())
			{
				return null;
			}
		}
		string[] array = new string[4];
		int num = desc.IndexOf(' ');
		array[1] = ((num > 0) ? desc.Substring(0, num) : desc);
		array[3] = desc;
		int startIndex = desc.LastIndexOf(' ');
		array[2] = desc.Substring(startIndex);
		array[0] = null;
		ListViewItem listViewItem2 = new ListViewItem(array);
		listViewItem2.Tag = desc;
		listViewDest.Items.Add(listViewItem2);
		return listViewItem2;
	}

	private void AddToPatchList(League league)
	{
		if (league == null)
		{
			return;
		}
		WriteToPatchList(league, league.leaguename);
		if (checkLeagueLinkedTeams.Checked)
		{
			foreach (Team playingTeam in league.PlayingTeams)
			{
				AddToPatchList(playingTeam);
			}
		}
		if (!checkLeagueReferees.Checked)
		{
			return;
		}
		foreach (Referee referee in FifaEnvironment.Referees)
		{
			if (referee.IsInLeague(league))
			{
				AddToPatchList(referee);
			}
		}
	}

	private void AddToPatchList(Team team)
	{
		if (team == null)
		{
			return;
		}
		WriteToPatchList(team, team.DatabaseName);
		if (checkTeamKits.Checked)
		{
			foreach (Kit kit in team.m_KitList)
			{
				AddToPatchList(kit);
			}
		}
		if (checkTeamFormation.Checked)
		{
			Formation formation = (Formation)FifaEnvironment.Formations.SearchId(team.formationid);
			AddToPatchList(formation);
		}
		if (checkTeamAdboard.Checked)
		{
			Adboard adboard = (Adboard)FifaEnvironment.Adboards.SearchId(team.adboardid);
			AddToPatchList(adboard);
		}
		if (checkTeamBall.Checked)
		{
			Ball ball = (Ball)FifaEnvironment.Balls.SearchId(team.balltype);
			AddToPatchList(ball);
		}
		if (checkTeamStadium.Checked)
		{
			Stadium stadium = (Stadium)FifaEnvironment.Stadiums.SearchId(team.Stadium);
			AddToPatchList(stadium);
		}
		if (!checkTeamLinkedPlayers.Checked)
		{
			return;
		}
		foreach (TeamPlayer item in team.Roster)
		{
			AddToPatchList(item.Player);
		}
	}

	private void AddToPatchList(Player player)
	{
		if (player != null)
		{
			WriteToPatchList(player);
			if (checkPlayerShoes.Checked)
			{
				Shoes shoes = (Shoes)FifaEnvironment.Shoes.SearchId(player.shoetypecode);
				AddToPatchList(shoes);
			}
			if (checkPlayerGloves.Checked)
			{
				GkGloves gloves = (GkGloves)FifaEnvironment.GkGloves.SearchId(player.gkglovetypecode);
				AddToPatchList(gloves);
			}
		}
	}

	private void AddToPatchList(Shoes shoes)
	{
		if (shoes != null && shoes.Id != 0 && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(Shoes.ShoesTexturesFileName(shoes.Id, 0))))
		{
			WriteToPatchList(shoes);
		}
	}

	private void AddToPatchList(Ball ball)
	{
		if (ball != null && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(ball.BallTextureFileName())))
		{
			WriteToPatchList(ball);
		}
	}

	private void AddToPatchList(Adboard adboard)
	{
		if (adboard != null && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(Adboard.AdboardFileName(adboard.Id))))
		{
			WriteToPatchList(adboard);
		}
	}

	private void AddToPatchList(Kit kit)
	{
		if (kit != null && kit.year == 0)
		{
			if (kit.Team != null)
			{
				WriteToPatchList(kit, kit.Team.Id * 10 + kit.kittype, kit.ToString(), null);
			}
			else
			{
				WriteToPatchList(kit, kit.teamid * 10 + kit.kittype, kit.ToString(), null);
			}
			if (checkKitNumbers.Checked)
			{
				NumberFont numberFont = (NumberFont)FifaEnvironment.NumberFonts.SearchId(kit.jerseyNumberFont * 20 + kit.jerseyNumberColor);
				AddToPatchList(numberFont);
				numberFont = (NumberFont)FifaEnvironment.NumberFonts.SearchId(kit.shortsNumberFont * 20 + kit.shortsNumberColor);
				AddToPatchList(numberFont);
			}
			if (checkKitNameFonts.Checked)
			{
				NameFont nameFont = (NameFont)FifaEnvironment.NameFonts.SearchId(kit.jerseyNameFont);
				AddToPatchList(nameFont);
			}
		}
	}

	private void AddToPatchList(Stadium stadium)
	{
		if (stadium != null)
		{
			WriteToPatchList(stadium, stadium.DatabaseString());
			if (checkStadiumNet.Checked)
			{
				Net net = (Net)FifaEnvironment.Nets.SearchId(stadium.NetColor);
				AddToPatchList(net);
			}
			if (checkStadiumMowingPattern.Checked)
			{
				int mowingPatternId = stadium.MowingPatternId;
				MowingPattern mowingPattern = (MowingPattern)FifaEnvironment.MowingPatterns.SearchId(mowingPatternId);
				AddToPatchList(mowingPattern);
			}
		}
	}

	private void AddToPatchList(Referee referee)
	{
		if (referee != null)
		{
			WriteToPatchList(referee);
			if (checkPlayerShoes.Checked)
			{
				Shoes shoes = (Shoes)FifaEnvironment.Shoes.SearchId(referee.shoetypecode);
				AddToPatchList(shoes);
			}
		}
	}

	private void AddToPatchList(Formation formation)
	{
		if (formation != null)
		{
			WriteToPatchList(formation);
		}
	}

	private void AddToPatchList(Country country)
	{
		if (country == null)
		{
			return;
		}
		WriteToPatchList(country, country.DatabaseName);
		if (checkCountryLeagues.Checked)
		{
			foreach (League league in FifaEnvironment.Leagues)
			{
				if (league.Country == country)
				{
					AddToPatchList(league);
				}
			}
		}
		if (checkContrynationalTeam.Checked && country.NationalTeam != null)
		{
			AddToPatchList(country.NationalTeam);
		}
	}

	private void AddToPatchList(NameFont nameFont)
	{
		if (nameFont != null && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(NameFont.NameFontFileName(nameFont.Id))))
		{
			WriteToPatchList(nameFont);
		}
	}

	private void AddToPatchList(NumberFont numberFont)
	{
		if (numberFont != null && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(numberFont.NumberFontFileName())))
		{
			WriteToPatchList(numberFont);
		}
	}

	private void AddToPatchList(Net net)
	{
		if (net != null && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(Net.NetFileName(net.Id))))
		{
			WriteToPatchList(net);
		}
	}

	private void AddToPatchList(MowingPattern mowingPattern)
	{
		if (mowingPattern != null && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(MowingPattern.MowingPatternFileName(mowingPattern.Id))))
		{
			WriteToPatchList(mowingPattern);
		}
	}

	private void AddToPatchList(GkGloves gloves)
	{
		if (gloves != null && (!radioIncludePatched.Checked || FifaEnvironment.IsPatched(GkGloves.GkGlovesTextureFileName(gloves.Id))))
		{
			WriteToPatchList(gloves);
		}
	}

	private void buttonRemoveObject_Click(object sender, EventArgs e)
	{
		int count = listViewDest.SelectedItems.Count;
		for (int i = 0; i < count; i++)
		{
			listViewDest.Items.Remove(listViewDest.SelectedItems[0]);
		}
	}

	private void buttonExit_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void buttonCreatePatch_Click(object sender, EventArgs e)
	{
		CreatePatch();
	}

	private void CreatePatch()
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "cmp files (*.cmp)|*.cmp";
		saveFileDialog.InitialDirectory = FifaEnvironment.TempFolder;
		saveFileDialog.FileName = textPatchName.Text;
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.Title = "Save Creation Master Patch";
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			saveFileDialog.Dispose();
			return;
		}
		string fileName = saveFileDialog.FileName;
		saveFileDialog.Dispose();
		Cursor.Current = Cursors.WaitCursor;
		Refresh();
		m_FifaDataSet.DataSetName = "FIFA16";
		m_LangDataSet.DataSetName = "LANG16";
		m_FifaDataSet.Tables.Clear();
		m_LangDataSet.Tables.Clear();
		m_PatchDataSet.Tables[0].Clear();
		DataRow dataRow = m_PatchDataSet.Tables[0].NewRow();
		dataRow[0] = textPatchName.Text;
		dataRow[1] = textPatchVersion.Text;
		dataRow[2] = textDescription.Text;
		dataRow[3] = "";
		m_PatchDataSet.Tables[0].Rows.Add(dataRow);
		m_PatchDataSet.Tables[1].Clear();
		foreach (ListViewItem item in listViewDest.Items)
		{
			DataRow dataRow2 = m_PatchDataSet.Tables[1].NewRow();
			dataRow2[0] = item.SubItems[0].Text;
			dataRow2[1] = item.SubItems[1].Text;
			dataRow2[2] = item.SubItems[2].Text;
			dataRow2[3] = item.SubItems[3].Text;
			m_PatchDataSet.Tables[1].Rows.Add(dataRow2);
		}
		CreateKeysArrays();
		m_LanguageTable = FifaEnvironment.LangDb.Table[TI.lang].ConvertToDataTable(m_LanguageKeys, "hashid");
		m_LangDataSet.Tables.Add(m_LanguageTable);
		if (m_PlayerKeys.Length != 0 && checkPlayerDatabase.Checked)
		{
			m_PlayersTable = FifaEnvironment.FifaDb.Table[TI.players].ConvertToDataTable(m_PlayerKeys, "playerid");
			m_FifaDataSet.Tables.Add(m_PlayersTable);
			m_PlayernamesTable = FifaEnvironment.FifaDb.Table[TI.playernames].ConvertToDataTable(m_PlayerNameKeys, "nameid");
			m_FifaDataSet.Tables.Add(m_PlayernamesTable);
			m_DcPlayernamesTable = FifaEnvironment.FifaDb.Table[TI.dcplayernames].ConvertToDataTable(m_PlayerNameKeys, "nameid");
			m_FifaDataSet.Tables.Add(m_DcPlayernamesTable);
			m_PlayerLoansTable = FifaEnvironment.FifaDb.Table[TI.playerloans].ConvertToDataTable(m_PlayerKeys, "playerid");
			m_FifaDataSet.Tables.Add(m_PlayerLoansTable);
			m_PreviousTeamTable = FifaEnvironment.FifaDb.Table[TI.previousteam].ConvertToDataTable(m_PlayerKeys, "playerid");
			m_FifaDataSet.Tables.Add(m_PreviousTeamTable);
		}
		if (m_TeamKeys.Length != 0 && checkTeamDatabase.Checked)
		{
			m_TeamsTable = FifaEnvironment.FifaDb.Table[TI.teams].ConvertToDataTable(m_TeamKeys, "teamid");
			m_FifaDataSet.Tables.Add(m_TeamsTable);
			m_RowTeamNationLinksTable = FifaEnvironment.FifaDb.Table[TI.rowteamnationlinks].ConvertToDataTable(m_TeamKeys, "teamid");
			m_FifaDataSet.Tables.Add(m_RowTeamNationLinksTable);
			if (TI.teamnationlinks >= 0)
			{
				m_TeamNationLinksTable = FifaEnvironment.FifaDb.Table[TI.teamnationlinks].ConvertToDataTable(m_TeamKeys, "teamid");
				m_FifaDataSet.Tables.Add(m_TeamNationLinksTable);
			}
			m_TeamplayerlinksTable = FifaEnvironment.FifaDb.Table[TI.teamplayerlinks].ConvertToDataTable(m_TeamKeys, "teamid");
			m_FifaDataSet.Tables.Add(m_TeamplayerlinksTable);
			m_TeamStadiumLinksTable = FifaEnvironment.FifaDb.Table[TI.teamstadiumlinks].ConvertToDataTable(m_TeamKeys, "teamid");
			m_FifaDataSet.Tables.Add(m_TeamStadiumLinksTable);
			m_TeamFormationTeamStyleLinksTable = FifaEnvironment.FifaDb.Table[TI.teamformationteamstylelinks].ConvertToDataTable(m_TeamKeys, "teamid");
			m_FifaDataSet.Tables.Add(m_TeamFormationTeamStyleLinksTable);
			m_StadiumAssignmentsTable = FifaEnvironment.FifaDb.Table[TI.stadiumassignments].ConvertToDataTable(m_TeamKeys, "teamid");
			m_FifaDataSet.Tables.Add(m_StadiumAssignmentsTable);
			m_ManagerTable = FifaEnvironment.FifaDb.Table[TI.manager].ConvertToDataTable(m_TeamKeys, "teamid");
			m_FifaDataSet.Tables.Add(m_ManagerTable);
		}
		if (checkKitDatabase.Checked)
		{
			m_TeamkitsTable = FifaEnvironment.FifaDb.Table[TI.teamkits].ConvertToDataTable(m_KitKeys, "teamkitid");
			m_FifaDataSet.Tables.Add(m_TeamkitsTable);
		}
		if (m_LeagueKeys.Length != 0 && checkLeagueDatabase.Checked)
		{
			m_LeaguesTable = FifaEnvironment.FifaDb.Table[TI.leagues].ConvertToDataTable(m_LeagueKeys, "leagueid");
			m_FifaDataSet.Tables.Add(m_LeaguesTable);
			m_BoardOutcomesTable = FifaEnvironment.FifaDb.Table[TI.career_boardoutcomes].ConvertToDataTable(m_LeagueKeys, "leagueid");
			m_FifaDataSet.Tables.Add(m_BoardOutcomesTable);
			if (checkLeagueLinkedTeams.Checked)
			{
				m_LeagueTeamLinksTable = FifaEnvironment.FifaDb.Table[TI.leagueteamlinks].ConvertToDataTable(m_LeagueKeys, "leagueid");
				m_FifaDataSet.Tables.Add(m_LeagueTeamLinksTable);
			}
		}
		if (m_CountryKeys.Length != 0 && checkCountryDatabase.Checked)
		{
			m_NationsTable = FifaEnvironment.FifaDb.Table[TI.nations].ConvertToDataTable(m_CountryKeys, "nationid");
			m_FifaDataSet.Tables.Add(m_NationsTable);
			m_AudionationsTable = FifaEnvironment.FifaDb.Table[TI.audionation].ConvertToDataTable(m_CountryKeys, "nationid");
			m_FifaDataSet.Tables.Add(m_AudionationsTable);
		}
		if (m_RefereeKeys.Length != 0 && checkRefereeDatabase.Checked)
		{
			m_RefereesTable = FifaEnvironment.FifaDb.Table[TI.referee].ConvertToDataTable(m_RefereeKeys, "refereeid");
			m_FifaDataSet.Tables.Add(m_RefereesTable);
		}
		if (m_StadiumKeys.Length != 0 && checkStadiumDatabase.Checked)
		{
			m_StadiumsTable = FifaEnvironment.FifaDb.Table[TI.stadiums].ConvertToDataTable(m_StadiumKeys, "stadiumid");
			m_FifaDataSet.Tables.Add(m_StadiumsTable);
		}
		if (m_FormationKeys.Length != 0)
		{
			m_FormationsTable = FifaEnvironment.FifaDb.Table[TI.formations].ConvertToDataTable(m_FormationKeys, "formationid");
			m_FifaDataSet.Tables.Add(m_FormationsTable);
		}
		if (Directory.Exists(m_TempFolder))
		{
			Directory.Delete(m_TempFolder, recursive: true);
		}
		Directory.CreateDirectory(m_TempFolder);
		statusLabel.Text = "Saving XML files...";
		statusBar.Refresh();
		m_PatchDataSet.WriteXml(m_TempFolder + "\\patch.xml");
		m_FifaDataSet.WriteXml(m_TempFolder + "\\fifa.xml");
		m_LangDataSet.WriteXml(m_TempFolder + "\\lang.xml");
		foreach (ListViewItem item2 in listViewDest.Items)
		{
			object tag = item2.Tag;
			string name = item2.Tag.GetType().Name;
			statusLabel.Text = "Saving " + item2.SubItems[3].Text;
			statusBar.Refresh();
			switch (name)
			{
			case "Player":
			{
				Player player = (Player)tag;
				if (checkPlayerHead.Checked && player.HasSpecificHeadModel)
				{
					CheckAndExport(player.SpecificFaceTextureFileName());
					CheckAndExport(player.SpecificHairTexturesFileName());
					CheckAndExport(player.SpecificHeadModelFileName());
					CheckAndExport(player.SpecificHairModelFileName());
					CheckAndExport(player.SpecificHairLodModelFileName());
					CheckAndExport(player.TattoTextureFileName());
				}
				if (checkPlayerMiniface.Checked)
				{
					CheckAndExport(player.SpecificPhotoDdsFileName());
				}
				continue;
			}
			case "Team":
			{
				Team team = (Team)tag;
				if (checkTeamGuiLogo.Checked)
				{
					CheckAndExport(team.CrestDdsFileName());
					CheckAndExport(team.Crest50DdsFileName());
					CheckAndExport(team.Crest32DdsFileName());
					CheckAndExport(team.Crest16DdsFileName());
				}
				if (checkTeamGuiBanner.Checked)
				{
					CheckAndExport(team.BannerFileName());
				}
				if (checkTeamFlags.Checked)
				{
					CheckAndExport(team.FlagFileName());
					CheckAndExport(team.ScarfFileName());
					CheckAndExport(team.RevModAdboardFileName());
					CheckAndExport(team.RevModBallModelFileName());
					CheckAndExport(team.RevModBallTextureFileName());
					CheckAndExport(team.RevModNetFileName());
					CheckAndExport(team.RevModManagerModelFileName());
					CheckAndExport(team.RevModManagerTextureFileName());
				}
				continue;
			}
			case "Kit":
			{
				Kit kit = (Kit)tag;
				if (checkKitTextures.Checked)
				{
					CheckAndExport(kit.KitTextureFileName());
				}
				if (checkKitMinikits.Checked)
				{
					CheckAndExport(kit.MiniKitDdsFileName());
				}
				continue;
			}
			case "League":
			{
				League league = (League)tag;
				if (checkLeagueLogo.Checked)
				{
					CheckAndExport(league.TinyLogoDdsFileName());
					CheckAndExport(league.SmallLogoDdsFileName());
					CheckAndExport(league.AnimLogoDdsFileName());
					CheckAndExport(league.Logo512x128DdsFileName());
				}
				continue;
			}
			case "Country":
			{
				Country country = (Country)tag;
				if (checkCountryFlag.Checked)
				{
					CheckAndExport(country.FlagBigFileName());
				}
				if (checkCountryMap.Checked)
				{
					CheckAndExport(country.ShapeFileName());
				}
				if (checkCountryFlag512x512.Checked)
				{
					CheckAndExport(country.Flag512DdsFileName());
				}
				if (checkCountryCardFlag.Checked)
				{
					CheckAndExport(country.CardFlagBigFileName());
				}
				if (checkCountryMiniFlag.Checked)
				{
					CheckAndExport(country.MiniFlagBigFileName());
				}
				continue;
			}
			case "Stadium":
			{
				Stadium stadium = (Stadium)tag;
				if (checkStadiumModel.Checked)
				{
					CheckAndExport(stadium.ModelFileName());
					CheckAndExport(stadium.RadiosityFileName());
					if (stadium.HasSunnyDay())
					{
						CheckAndExport(stadium.TexturesFileName(1));
						CheckAndExport(stadium.CrowdFileName(1));
					}
					if (stadium.HasNight())
					{
						CheckAndExport(stadium.TexturesFileName(3));
						CheckAndExport(stadium.CrowdFileName(3));
					}
				}
				if (checkStadiumPreview.Checked)
				{
					CheckAndExport(stadium.PreviewBigFileName(1));
					CheckAndExport(stadium.PreviewLargeBigFileName(1));
					CheckAndExport(stadium.PreviewBigFileName(3));
					CheckAndExport(stadium.PreviewLargeBigFileName(3));
				}
				continue;
			}
			case "Referee":
			{
				Referee referee = (Referee)tag;
				if (checkRefereeMiniFace.Checked && FifaEnvironment.Year == 14)
				{
					CheckAndExport(referee.PhotoBigFileName());
				}
				continue;
			}
			}
			name = item2.SubItems[1].Text;
			int num = Convert.ToInt32(item2.SubItems[2].Text);
			switch (name)
			{
			case "Ball":
				CheckAndExport(Ball.BallModelFileName(num));
				CheckAndExport(Ball.BallTextureFileName(num));
				if (FifaEnvironment.Year == 14)
				{
					CheckAndExport(Ball.BallPictureBigFileName(num));
				}
				else
				{
					CheckAndExport(Ball.BallDdsFileName(num));
				}
				break;
			case "Adboard":
				CheckAndExport(Adboard.AdboardFileName(num));
				break;
			case "Shoes":
				CheckAndExport(Shoes.ShoesTexturesFileName(num, 0));
				CheckAndExport(Shoes.ShoesModelFileName(num));
				break;
			case "Net":
				CheckAndExport(Net.NetFileName(num));
				break;
			case "MowingPattern":
				CheckAndExport(MowingPattern.MowingPatternFileName(num));
				break;
			case "GkGloves":
				CheckAndExport(GkGloves.GkGlovesTextureFileName(num));
				break;
			case "NumberFont":
			{
				int num2 = num / 20;
				int colorId = num - num2 * 20;
				CheckAndExport(NumberFont.NumberFontFileName(num2, colorId));
				break;
			}
			case "NameFont":
				CheckAndExport(NameFont.NameFontFileName(num));
				break;
			}
		}
		ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(fileName));
		zipOutputStream.SetLevel(8);
		string[] files = Directory.GetFiles(m_TempFolder, "*.*", SearchOption.AllDirectories);
		if (files != null)
		{
			int startIndex = m_TempFolder.Length + 1;
			for (int i = 0; i < files.Length; i++)
			{
				string obj = files[i];
				string fileName2 = obj.Substring(startIndex);
				FileStream fileStream = File.OpenRead(obj);
				AddStreamToZip(zipOutputStream, fileStream, fileName2);
				fileStream.Close();
				statusLabel.Text = "Zipping " + (files.Length - i);
				statusBar.Refresh();
			}
			zipOutputStream.Finish();
			zipOutputStream.Close();
			Cursor.Current = Cursors.Default;
			statusLabel.Text = "Ready";
			statusBar.Refresh();
		}
	}

	private void CheckAndExport(string fileName)
	{
		if (radioIncludeOriginal.Checked || FifaEnvironment.IsPatched(fileName))
		{
			FifaEnvironment.ExportFileFromZdata(fileName, m_TempFolder);
		}
	}

	private void CreateKeysArrays()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		foreach (ListViewItem item in listViewDest.Items)
		{
			_ = item.Tag;
			switch (item.Tag.GetType().Name)
			{
			case "Player":
				num4++;
				num5 += 4;
				break;
			case "Team":
				num3++;
				num10 += 5;
				break;
			case "Kit":
				num6++;
				break;
			case "League":
				num2++;
				num10 += 2;
				break;
			case "Country":
				num++;
				num10++;
				break;
			case "Stadium":
				num8++;
				num10++;
				break;
			case "Referee":
				num7++;
				break;
			case "Formation":
				num9++;
				break;
			case "Ball":
				num10++;
				break;
			}
		}
		m_PlayerKeys = new int[num4];
		m_PlayerNameKeys = new int[num5];
		m_TeamKeys = new int[num3];
		m_KitKeys = new int[num6];
		m_LeagueKeys = new int[num2];
		m_CountryKeys = new int[num];
		m_RefereeKeys = new int[num7];
		m_StadiumKeys = new int[num8];
		m_FormationKeys = new int[num9];
		m_LanguageKeys = new int[num10];
		num = 0;
		num2 = 0;
		num3 = 0;
		num4 = 0;
		num5 = 0;
		num6 = 0;
		num7 = 0;
		num8 = 0;
		num9 = 0;
		num10 = 0;
		foreach (ListViewItem item2 in listViewDest.Items)
		{
			object tag = item2.Tag;
			switch (item2.Tag.GetType().Name)
			{
			case "Player":
			{
				Player player = (Player)tag;
				m_PlayerKeys[num4++] = player.Id;
				m_PlayerNameKeys[num5++] = player.firstnameid;
				m_PlayerNameKeys[num5++] = player.lastnameid;
				m_PlayerNameKeys[num5++] = player.commonnameid;
				m_PlayerNameKeys[num5++] = player.playerjerseynameid;
				break;
			}
			case "Team":
			{
				Team team = (Team)tag;
				m_TeamKeys[num3++] = team.Id;
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetTeamHash(team.Id, Language.ETeamStringType.Full);
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetTeamHash(team.Id, Language.ETeamStringType.Abbr10);
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetTeamHash(team.Id, Language.ETeamStringType.Abbr15);
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetTeamHash(team.Id, Language.ETeamStringType.Abbr7);
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetTeamHash(team.Id, Language.ETeamStringType.Abbr3);
				break;
			}
			case "Kit":
			{
				Kit kit = (Kit)tag;
				m_KitKeys[num6++] = kit.Id;
				break;
			}
			case "League":
			{
				League league = (League)tag;
				m_LeagueKeys[num2++] = league.Id;
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetLeagueHash(league.Id, Language.ELeagueStringType.Abbr15);
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetLeagueHash(league.Id, Language.ELeagueStringType.Full);
				break;
			}
			case "Country":
			{
				Country country = (Country)tag;
				m_CountryKeys[num++] = country.Id;
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetCountryHash(country.Id, Language.ECountryStringType.Full);
				break;
			}
			case "Stadium":
			{
				Stadium stadium = (Stadium)tag;
				m_StadiumKeys[num8++] = stadium.Id;
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetStadiumHash(stadium.Id);
				break;
			}
			case "Referee":
			{
				Referee referee = (Referee)tag;
				m_RefereeKeys[num7++] = referee.Id;
				break;
			}
			case "Formation":
			{
				Formation formation = (Formation)tag;
				m_FormationKeys[num9++] = formation.Id;
				break;
			}
			case "Ball":
			{
				Ball ball = (Ball)tag;
				m_LanguageKeys[num10++] = (int)FifaEnvironment.Language.GetBallHash(ball.Id);
				break;
			}
			}
		}
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

	private void OpenPatch()
	{
		openFileDialog.CheckFileExists = true;
		openFileDialog.Title = "Open Creation Master Patch file";
		openFileDialog.Filter = "Creation Master Patch (*.cmp)|*.cmp";
		openFileDialog.FilterIndex = 1;
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string fileName = openFileDialog.FileName;
		if (!File.Exists(fileName))
		{
			return;
		}
		Refresh();
		Cursor.Current = Cursors.WaitCursor;
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
		m_PatchDataSet.Clear();
		m_PatchDataSet.ReadXml(m_TempFolder + "\\Patch.xml");
		DataRow dataRow = m_PatchDataSet.PatchIdentity.Rows[0];
		textDescription.Text = (string)dataRow["Description"];
		textPatchName.Text = (string)dataRow["Name"];
		textPatchVersion.Text = (string)dataRow["Version"];
		checkCMSCompatible.Checked = (string)dataRow["CMS"] == "CMS 14";
		listViewDest.Items.Clear();
		foreach (DataRow row in m_PatchDataSet.PatchElements.Rows)
		{
			string[] array = new string[4];
			array[1] = (string)row.ItemArray[1];
			array[2] = (string)row.ItemArray[2];
			array[3] = (string)row.ItemArray[3];
			array[0] = (string)row.ItemArray[0];
			switch (array[1])
			{
			case "Player":
			{
				Player player = (Player)FifaEnvironment.Players.SearchId(Convert.ToInt32(array[2]));
				if (player != null)
				{
					WriteToPatchList(player, player.Id, array[3], array[0]);
				}
				break;
			}
			case "Shoes":
			{
				Shoes shoes = (Shoes)FifaEnvironment.Shoes.SearchId(Convert.ToInt32(array[2]));
				if (shoes != null)
				{
					WriteToPatchList(shoes, shoes.Id, array[3], array[0]);
				}
				break;
			}
			case "Team":
			{
				Team team = (Team)FifaEnvironment.Teams.SearchId(Convert.ToInt32(array[2]));
				if (team != null)
				{
					WriteToPatchList(team, team.Id, array[3], array[0]);
				}
				break;
			}
			case "Kit":
			{
				Kit kit = (Kit)FifaEnvironment.Kits.SearchId(Convert.ToInt32(array[2]));
				if (kit != null)
				{
					WriteToPatchList(kit, kit.Id, array[3], array[0]);
				}
				break;
			}
			case "Formation":
			{
				Formation formation = (Formation)FifaEnvironment.Formations.SearchId(Convert.ToInt32(array[2]));
				if (formation != null)
				{
					WriteToPatchList(formation, formation.Id, array[3], array[0]);
				}
				break;
			}
			case "Ball":
			{
				Ball ball = (Ball)FifaEnvironment.Balls.SearchId(Convert.ToInt32(array[2]));
				if (ball != null)
				{
					WriteToPatchList(ball, ball.Id, array[3], array[0]);
				}
				break;
			}
			case "Adboard":
			{
				Adboard adboard = (Adboard)FifaEnvironment.Adboards.SearchId(Convert.ToInt32(array[2]));
				if (adboard != null)
				{
					WriteToPatchList(adboard, adboard.Id, array[3], array[0]);
				}
				break;
			}
			case "League":
			{
				League league = (League)FifaEnvironment.Leagues.SearchId(Convert.ToInt32(array[2]));
				if (league != null)
				{
					WriteToPatchList(league, league.Id, array[3], array[0]);
				}
				break;
			}
			case "Country":
			{
				Country country = (Country)FifaEnvironment.Countries.SearchId(Convert.ToInt32(array[2]));
				if (country != null)
				{
					WriteToPatchList(country, country.Id, array[3], array[0]);
				}
				break;
			}
			case "Referee":
			{
				Referee referee = (Referee)FifaEnvironment.Referees.SearchId(Convert.ToInt32(array[2]));
				if (referee != null)
				{
					WriteToPatchList(referee, referee.Id, array[3], array[0]);
				}
				break;
			}
			case "NameFont":
			{
				NameFont nameFont = (NameFont)FifaEnvironment.NameFonts.SearchId(Convert.ToInt32(array[2]));
				if (nameFont != null)
				{
					WriteToPatchList(nameFont, nameFont.Id, array[3], array[0]);
				}
				break;
			}
			case "NumberFont":
			{
				NumberFont numberFont = (NumberFont)FifaEnvironment.NumberFonts.SearchId(Convert.ToInt32(array[2]));
				if (numberFont != null)
				{
					WriteToPatchList(numberFont, numberFont.Id, array[3], array[0]);
				}
				break;
			}
			case "Net":
			{
				Net net = (Net)FifaEnvironment.Nets.SearchId(Convert.ToInt32(array[2]));
				if (net != null)
				{
					WriteToPatchList(net, net.Id, array[3], array[0]);
				}
				break;
			}
			case "MowingPattern":
			{
				MowingPattern mowingPattern = (MowingPattern)FifaEnvironment.MowingPatterns.SearchId(Convert.ToInt32(array[2]));
				if (mowingPattern != null)
				{
					WriteToPatchList(mowingPattern, mowingPattern.Id, array[3], array[0]);
				}
				break;
			}
			case "GkGloves":
			{
				GkGloves gkGloves = (GkGloves)FifaEnvironment.GkGloves.SearchId(Convert.ToInt32(array[2]));
				if (gkGloves != null)
				{
					WriteToPatchList(gkGloves, gkGloves.Id, array[3], array[0]);
				}
				break;
			}
			}
		}
		Cursor.Current = Cursors.Default;
	}

	private void buttonOpenPatch_Click(object sender, EventArgs e)
	{
		OpenPatch();
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

	private void buttonNewPatch_Click(object sender, EventArgs e)
	{
		InitPatchCreatorForm();
	}

	private void InitPatchCreatorForm()
	{
		listViewDest.Items.Clear();
		textDescription.Text = string.Empty;
		textPatchName.Text = string.Empty;
		textPatchVersion.Text = string.Empty;
		checkCMSCompatible.Checked = false;
	}

	private void exitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void newPatchToolStripMenuItem_Click(object sender, EventArgs e)
	{
		InitPatchCreatorForm();
	}

	private void createPatchToolStripMenuItem_Click(object sender, EventArgs e)
	{
		CreatePatch();
	}

	private void openPatchToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenPatch();
	}

	private void buttonAddFile_Click(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.PatchCreatorForm));
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.listSource = new System.Windows.Forms.ListBox();
		this.listViewDest = new System.Windows.Forms.ListView();
		this.columnComment = new System.Windows.Forms.ColumnHeader();
		this.columnType = new System.Windows.Forms.ColumnHeader();
		this.columnId = new System.Windows.Forms.ColumnHeader();
		this.columnItem = new System.Windows.Forms.ColumnHeader();
		this.toolAddRemove = new System.Windows.Forms.ToolStrip();
		this.buttonAddObject = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonRemoveObject = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonAddFile = new System.Windows.Forms.ToolStripButton();
		this.panelLeft = new System.Windows.Forms.Panel();
		this.statusBar = new System.Windows.Forms.StatusStrip();
		this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
		this.textDescription = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.checkCMSCompatible = new System.Windows.Forms.CheckBox();
		this.textPatchVersion = new System.Windows.Forms.TextBox();
		this.textPatchName = new System.Windows.Forms.TextBox();
		this.labelPatchVersion = new System.Windows.Forms.Label();
		this.labelPatchName = new System.Windows.Forms.Label();
		this.comboPatchType = new System.Windows.Forms.ComboBox();
		this.groupPatchOptions = new System.Windows.Forms.GroupBox();
		this.tabPatchOptions = new System.Windows.Forms.TabControl();
		this.pageGeneralOptions = new System.Windows.Forms.TabPage();
		this.radioIncludeOriginal = new System.Windows.Forms.RadioButton();
		this.radioIncludePatched = new System.Windows.Forms.RadioButton();
		this.pageCountryOptions = new System.Windows.Forms.TabPage();
		this.checkCountryMap = new System.Windows.Forms.CheckBox();
		this.checkCountryFlag512x512 = new System.Windows.Forms.CheckBox();
		this.checkCountryCardFlag = new System.Windows.Forms.CheckBox();
		this.checkCountryTournaments = new System.Windows.Forms.CheckBox();
		this.checkCountryLeagues = new System.Windows.Forms.CheckBox();
		this.checkContrynationalTeam = new System.Windows.Forms.CheckBox();
		this.checkCountryMiniFlag = new System.Windows.Forms.CheckBox();
		this.checkCountryDatabase = new System.Windows.Forms.CheckBox();
		this.checkCountryFlag = new System.Windows.Forms.CheckBox();
		this.pageLeagueOptions = new System.Windows.Forms.TabPage();
		this.checkLeagueBall = new System.Windows.Forms.CheckBox();
		this.checkLeagueReferees = new System.Windows.Forms.CheckBox();
		this.checkLeagueLinkedTournament = new System.Windows.Forms.CheckBox();
		this.checkLeagueLinkedTeams = new System.Windows.Forms.CheckBox();
		this.checkLeagueLogo = new System.Windows.Forms.CheckBox();
		this.checkLeagueDatabase = new System.Windows.Forms.CheckBox();
		this.pageTeamOptions = new System.Windows.Forms.TabPage();
		this.checkTeamStadium = new System.Windows.Forms.CheckBox();
		this.checkTeamFormation = new System.Windows.Forms.CheckBox();
		this.checkTeamAdboard = new System.Windows.Forms.CheckBox();
		this.checkTeamBall = new System.Windows.Forms.CheckBox();
		this.checkTeamLinkedPlayers = new System.Windows.Forms.CheckBox();
		this.checkTeamKits = new System.Windows.Forms.CheckBox();
		this.checkTeamFlags = new System.Windows.Forms.CheckBox();
		this.checkTeamGuiBanner = new System.Windows.Forms.CheckBox();
		this.checkTeamGuiLogo = new System.Windows.Forms.CheckBox();
		this.checkTeamDatabase = new System.Windows.Forms.CheckBox();
		this.pageKitOptions = new System.Windows.Forms.TabPage();
		this.checkKitTextures = new System.Windows.Forms.CheckBox();
		this.checkKitNameFonts = new System.Windows.Forms.CheckBox();
		this.checkKitDatabase = new System.Windows.Forms.CheckBox();
		this.checkKitNumbers = new System.Windows.Forms.CheckBox();
		this.checkKitMinikits = new System.Windows.Forms.CheckBox();
		this.pagePlayerOptions = new System.Windows.Forms.TabPage();
		this.checkPlayerGloves = new System.Windows.Forms.CheckBox();
		this.checkPlayerShoes = new System.Windows.Forms.CheckBox();
		this.checkPlayerMiniface = new System.Windows.Forms.CheckBox();
		this.checkPlayerHead = new System.Windows.Forms.CheckBox();
		this.checkPlayerDatabase = new System.Windows.Forms.CheckBox();
		this.pageRefereeOptions = new System.Windows.Forms.TabPage();
		this.checkRefereeMiniFace = new System.Windows.Forms.CheckBox();
		this.checkRefereeShoes = new System.Windows.Forms.CheckBox();
		this.checkRefereeKits = new System.Windows.Forms.CheckBox();
		this.checkRefereeDatabase = new System.Windows.Forms.CheckBox();
		this.pageStadiumOptions = new System.Windows.Forms.TabPage();
		this.checkStadiumMowingPattern = new System.Windows.Forms.CheckBox();
		this.checkStadiumModel = new System.Windows.Forms.CheckBox();
		this.checkStadiumPreview = new System.Windows.Forms.CheckBox();
		this.checkStadiumDatabase = new System.Windows.Forms.CheckBox();
		this.checkStadiumNet = new System.Windows.Forms.CheckBox();
		this.labelPatchType = new System.Windows.Forms.Label();
		this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
		this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.newPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.createPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.openPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolMain = new System.Windows.Forms.ToolStrip();
		this.buttonNewPatch = new System.Windows.Forms.ToolStripButton();
		this.buttonOpenPatch = new System.Windows.Forms.ToolStripButton();
		this.buttonCreatePatch = new System.Windows.Forms.ToolStripButton();
		this.buttonExit = new System.Windows.Forms.ToolStripButton();
		this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
		this.m_PatchDataSet = new CreationMaster.Patch();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		this.toolAddRemove.SuspendLayout();
		this.panelLeft.SuspendLayout();
		this.statusBar.SuspendLayout();
		this.groupPatchOptions.SuspendLayout();
		this.tabPatchOptions.SuspendLayout();
		this.pageGeneralOptions.SuspendLayout();
		this.pageCountryOptions.SuspendLayout();
		this.pageLeagueOptions.SuspendLayout();
		this.pageTeamOptions.SuspendLayout();
		this.pageKitOptions.SuspendLayout();
		this.pagePlayerOptions.SuspendLayout();
		this.pageRefereeOptions.SuspendLayout();
		this.pageStadiumOptions.SuspendLayout();
		this.mainMenuStrip.SuspendLayout();
		this.toolMain.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_PatchDataSet).BeginInit();
		base.SuspendLayout();
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(300, 49);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.listSource);
		this.splitContainer1.Panel2.Controls.Add(this.listViewDest);
		this.splitContainer1.Panel2.Controls.Add(this.toolAddRemove);
		this.splitContainer1.Size = new System.Drawing.Size(728, 697);
		this.splitContainer1.SplitterDistance = 262;
		this.splitContainer1.TabIndex = 0;
		this.listSource.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listSource.FormattingEnabled = true;
		this.listSource.Location = new System.Drawing.Point(0, 0);
		this.listSource.Name = "listSource";
		this.listSource.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this.listSource.Size = new System.Drawing.Size(262, 697);
		this.listSource.TabIndex = 27;
		this.listViewDest.AllowColumnReorder = true;
		this.listViewDest.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.columnComment, this.columnType, this.columnId, this.columnItem });
		this.listViewDest.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewDest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.listViewDest.FullRowSelect = true;
		this.listViewDest.GridLines = true;
		this.listViewDest.HideSelection = false;
		this.listViewDest.LabelEdit = true;
		this.listViewDest.Location = new System.Drawing.Point(0, 25);
		this.listViewDest.Name = "listViewDest";
		this.listViewDest.Size = new System.Drawing.Size(462, 672);
		this.listViewDest.TabIndex = 27;
		this.listViewDest.UseCompatibleStateImageBehavior = false;
		this.listViewDest.View = System.Windows.Forms.View.Details;
		this.columnComment.DisplayIndex = 3;
		this.columnComment.Text = "Comment";
		this.columnComment.Width = 147;
		this.columnType.DisplayIndex = 0;
		this.columnType.Text = "Type";
		this.columnType.Width = 72;
		this.columnId.DisplayIndex = 1;
		this.columnId.Text = "ID";
		this.columnId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnId.Width = 51;
		this.columnItem.DisplayIndex = 2;
		this.columnItem.Text = "Item";
		this.columnItem.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnItem.Width = 124;
		this.toolAddRemove.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolAddRemove.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.buttonAddObject, this.toolStripSeparator1, this.buttonRemoveObject, this.toolStripSeparator2, this.buttonAddFile });
		this.toolAddRemove.Location = new System.Drawing.Point(0, 0);
		this.toolAddRemove.Name = "toolAddRemove";
		this.toolAddRemove.Size = new System.Drawing.Size(462, 25);
		this.toolAddRemove.TabIndex = 7;
		this.buttonAddObject.Image = (System.Drawing.Image)resources.GetObject("buttonAddObject.Image");
		this.buttonAddObject.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddObject.Name = "buttonAddObject";
		this.buttonAddObject.Size = new System.Drawing.Size(49, 22);
		this.buttonAddObject.Text = "Add";
		this.buttonAddObject.Click += new System.EventHandler(buttonAdd_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonRemoveObject.Image = (System.Drawing.Image)resources.GetObject("buttonRemoveObject.Image");
		this.buttonRemoveObject.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemoveObject.Name = "buttonRemoveObject";
		this.buttonRemoveObject.Size = new System.Drawing.Size(70, 22);
		this.buttonRemoveObject.Text = "Remove";
		this.buttonRemoveObject.Click += new System.EventHandler(buttonRemoveObject_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonAddFile.Image = (System.Drawing.Image)resources.GetObject("buttonAddFile.Image");
		this.buttonAddFile.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddFile.Name = "buttonAddFile";
		this.buttonAddFile.Size = new System.Drawing.Size(70, 22);
		this.buttonAddFile.Text = "Add File";
		this.buttonAddFile.Visible = false;
		this.buttonAddFile.Click += new System.EventHandler(buttonAddFile_Click);
		this.panelLeft.AutoScroll = true;
		this.panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panelLeft.Controls.Add(this.statusBar);
		this.panelLeft.Controls.Add(this.textDescription);
		this.panelLeft.Controls.Add(this.label1);
		this.panelLeft.Controls.Add(this.checkCMSCompatible);
		this.panelLeft.Controls.Add(this.textPatchVersion);
		this.panelLeft.Controls.Add(this.textPatchName);
		this.panelLeft.Controls.Add(this.labelPatchVersion);
		this.panelLeft.Controls.Add(this.labelPatchName);
		this.panelLeft.Controls.Add(this.comboPatchType);
		this.panelLeft.Controls.Add(this.groupPatchOptions);
		this.panelLeft.Controls.Add(this.labelPatchType);
		this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
		this.panelLeft.Location = new System.Drawing.Point(0, 49);
		this.panelLeft.Name = "panelLeft";
		this.panelLeft.Size = new System.Drawing.Size(300, 697);
		this.panelLeft.TabIndex = 3;
		this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.statusLabel });
		this.statusBar.Location = new System.Drawing.Point(0, 671);
		this.statusBar.Name = "statusBar";
		this.statusBar.Size = new System.Drawing.Size(296, 22);
		this.statusBar.TabIndex = 29;
		this.statusLabel.Name = "statusLabel";
		this.statusLabel.Size = new System.Drawing.Size(39, 17);
		this.statusLabel.Text = "Status";
		this.textDescription.Dock = System.Windows.Forms.DockStyle.Top;
		this.textDescription.Location = new System.Drawing.Point(0, 411);
		this.textDescription.Multiline = true;
		this.textDescription.Name = "textDescription";
		this.textDescription.Size = new System.Drawing.Size(296, 185);
		this.textDescription.TabIndex = 15;
		this.label1.Dock = System.Windows.Forms.DockStyle.Top;
		this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1.Location = new System.Drawing.Point(0, 390);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(296, 21);
		this.label1.TabIndex = 14;
		this.label1.Text = "Description";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkCMSCompatible.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkCMSCompatible.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkCMSCompatible.Location = new System.Drawing.Point(0, 362);
		this.checkCMSCompatible.Name = "checkCMSCompatible";
		this.checkCMSCompatible.Size = new System.Drawing.Size(296, 28);
		this.checkCMSCompatible.TabIndex = 28;
		this.checkCMSCompatible.Text = "CMS 14 Compliant";
		this.checkCMSCompatible.UseVisualStyleBackColor = true;
		this.checkCMSCompatible.Visible = false;
		this.textPatchVersion.Location = new System.Drawing.Point(86, 338);
		this.textPatchVersion.Name = "textPatchVersion";
		this.textPatchVersion.Size = new System.Drawing.Size(203, 20);
		this.textPatchVersion.TabIndex = 13;
		this.textPatchVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.textPatchName.Location = new System.Drawing.Point(86, 310);
		this.textPatchName.Name = "textPatchName";
		this.textPatchName.Size = new System.Drawing.Size(203, 20);
		this.textPatchName.TabIndex = 11;
		this.textPatchName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.labelPatchVersion.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelPatchVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPatchVersion.Location = new System.Drawing.Point(0, 334);
		this.labelPatchVersion.Name = "labelPatchVersion";
		this.labelPatchVersion.Size = new System.Drawing.Size(296, 28);
		this.labelPatchVersion.TabIndex = 12;
		this.labelPatchVersion.Text = "Patch Version";
		this.labelPatchVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelPatchName.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelPatchName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPatchName.Location = new System.Drawing.Point(0, 306);
		this.labelPatchName.Name = "labelPatchName";
		this.labelPatchName.Size = new System.Drawing.Size(296, 28);
		this.labelPatchName.TabIndex = 10;
		this.labelPatchName.Text = "Patch Name";
		this.labelPatchName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.comboPatchType.FormattingEnabled = true;
		this.comboPatchType.Items.AddRange(new object[16]
		{
			"Countries", "Leagues", "Teams", "Players", "Kits", "Referees", "Stadiums", "Formations", "Balls", "Adboards",
			"Number Fonts", "Name Fonts", "Shoes", "GK Gloves", "Nets", "Mowing Patterns"
		});
		this.comboPatchType.Location = new System.Drawing.Point(7, 21);
		this.comboPatchType.Name = "comboPatchType";
		this.comboPatchType.Size = new System.Drawing.Size(282, 21);
		this.comboPatchType.TabIndex = 1;
		this.comboPatchType.SelectedIndexChanged += new System.EventHandler(comboPatchType_SelectedIndexChanged);
		this.groupPatchOptions.Controls.Add(this.tabPatchOptions);
		this.groupPatchOptions.Dock = System.Windows.Forms.DockStyle.Top;
		this.groupPatchOptions.Location = new System.Drawing.Point(0, 48);
		this.groupPatchOptions.Name = "groupPatchOptions";
		this.groupPatchOptions.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.groupPatchOptions.Size = new System.Drawing.Size(296, 258);
		this.groupPatchOptions.TabIndex = 9;
		this.groupPatchOptions.TabStop = false;
		this.groupPatchOptions.Text = "Patch Options";
		this.tabPatchOptions.Controls.Add(this.pageGeneralOptions);
		this.tabPatchOptions.Controls.Add(this.pageCountryOptions);
		this.tabPatchOptions.Controls.Add(this.pageLeagueOptions);
		this.tabPatchOptions.Controls.Add(this.pageTeamOptions);
		this.tabPatchOptions.Controls.Add(this.pageKitOptions);
		this.tabPatchOptions.Controls.Add(this.pagePlayerOptions);
		this.tabPatchOptions.Controls.Add(this.pageRefereeOptions);
		this.tabPatchOptions.Controls.Add(this.pageStadiumOptions);
		this.tabPatchOptions.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabPatchOptions.ItemSize = new System.Drawing.Size(80, 20);
		this.tabPatchOptions.Location = new System.Drawing.Point(3, 16);
		this.tabPatchOptions.Multiline = true;
		this.tabPatchOptions.Name = "tabPatchOptions";
		this.tabPatchOptions.SelectedIndex = 0;
		this.tabPatchOptions.Size = new System.Drawing.Size(290, 239);
		this.tabPatchOptions.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
		this.tabPatchOptions.TabIndex = 8;
		this.pageGeneralOptions.Controls.Add(this.radioIncludeOriginal);
		this.pageGeneralOptions.Controls.Add(this.radioIncludePatched);
		this.pageGeneralOptions.Location = new System.Drawing.Point(4, 44);
		this.pageGeneralOptions.Name = "pageGeneralOptions";
		this.pageGeneralOptions.Size = new System.Drawing.Size(282, 191);
		this.pageGeneralOptions.TabIndex = 8;
		this.pageGeneralOptions.Text = "General";
		this.pageGeneralOptions.UseVisualStyleBackColor = true;
		this.radioIncludeOriginal.AutoSize = true;
		this.radioIncludeOriginal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.radioIncludeOriginal.Location = new System.Drawing.Point(24, 43);
		this.radioIncludeOriginal.Name = "radioIncludeOriginal";
		this.radioIncludeOriginal.Size = new System.Drawing.Size(186, 17);
		this.radioIncludeOriginal.TabIndex = 1;
		this.radioIncludeOriginal.TabStop = true;
		this.radioIncludeOriginal.Text = "Include Patched and Original Files";
		this.radioIncludeOriginal.UseVisualStyleBackColor = true;
		this.radioIncludePatched.AutoSize = true;
		this.radioIncludePatched.Checked = true;
		this.radioIncludePatched.Location = new System.Drawing.Point(24, 20);
		this.radioIncludePatched.Name = "radioIncludePatched";
		this.radioIncludePatched.Size = new System.Drawing.Size(151, 17);
		this.radioIncludePatched.TabIndex = 0;
		this.radioIncludePatched.TabStop = true;
		this.radioIncludePatched.Text = "Include Patched Files Only";
		this.radioIncludePatched.UseVisualStyleBackColor = true;
		this.pageCountryOptions.Controls.Add(this.checkCountryMap);
		this.pageCountryOptions.Controls.Add(this.checkCountryFlag512x512);
		this.pageCountryOptions.Controls.Add(this.checkCountryCardFlag);
		this.pageCountryOptions.Controls.Add(this.checkCountryTournaments);
		this.pageCountryOptions.Controls.Add(this.checkCountryLeagues);
		this.pageCountryOptions.Controls.Add(this.checkContrynationalTeam);
		this.pageCountryOptions.Controls.Add(this.checkCountryMiniFlag);
		this.pageCountryOptions.Controls.Add(this.checkCountryDatabase);
		this.pageCountryOptions.Controls.Add(this.checkCountryFlag);
		this.pageCountryOptions.Location = new System.Drawing.Point(4, 44);
		this.pageCountryOptions.Name = "pageCountryOptions";
		this.pageCountryOptions.Size = new System.Drawing.Size(282, 191);
		this.pageCountryOptions.TabIndex = 3;
		this.pageCountryOptions.Text = "Countries";
		this.pageCountryOptions.UseVisualStyleBackColor = true;
		this.checkCountryMap.AutoSize = true;
		this.checkCountryMap.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkCountryMap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryMap.Location = new System.Drawing.Point(20, 66);
		this.checkCountryMap.Name = "checkCountryMap";
		this.checkCountryMap.Size = new System.Drawing.Size(47, 17);
		this.checkCountryMap.TabIndex = 9;
		this.checkCountryMap.Text = "Map";
		this.checkCountryMap.UseVisualStyleBackColor = true;
		this.checkCountryFlag512x512.AutoSize = true;
		this.checkCountryFlag512x512.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkCountryFlag512x512.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryFlag512x512.Location = new System.Drawing.Point(20, 135);
		this.checkCountryFlag512x512.Name = "checkCountryFlag512x512";
		this.checkCountryFlag512x512.Size = new System.Drawing.Size(93, 17);
		this.checkCountryFlag512x512.TabIndex = 8;
		this.checkCountryFlag512x512.Text = "512 x 512 flag";
		this.checkCountryFlag512x512.UseVisualStyleBackColor = true;
		this.checkCountryCardFlag.AutoSize = true;
		this.checkCountryCardFlag.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkCountryCardFlag.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryCardFlag.Location = new System.Drawing.Point(20, 89);
		this.checkCountryCardFlag.Name = "checkCountryCardFlag";
		this.checkCountryCardFlag.Size = new System.Drawing.Size(71, 17);
		this.checkCountryCardFlag.TabIndex = 7;
		this.checkCountryCardFlag.Text = "Card Flag";
		this.checkCountryCardFlag.UseVisualStyleBackColor = true;
		this.checkCountryTournaments.AutoSize = true;
		this.checkCountryTournaments.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryTournaments.Location = new System.Drawing.Point(150, 66);
		this.checkCountryTournaments.Name = "checkCountryTournaments";
		this.checkCountryTournaments.Size = new System.Drawing.Size(123, 17);
		this.checkCountryTournaments.TabIndex = 6;
		this.checkCountryTournaments.Text = "Linked Tournaments";
		this.checkCountryTournaments.UseVisualStyleBackColor = true;
		this.checkCountryTournaments.Visible = false;
		this.checkCountryLeagues.AutoSize = true;
		this.checkCountryLeagues.Checked = true;
		this.checkCountryLeagues.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkCountryLeagues.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryLeagues.Location = new System.Drawing.Point(150, 43);
		this.checkCountryLeagues.Name = "checkCountryLeagues";
		this.checkCountryLeagues.Size = new System.Drawing.Size(102, 17);
		this.checkCountryLeagues.TabIndex = 5;
		this.checkCountryLeagues.Text = "Linked Leagues";
		this.checkCountryLeagues.UseVisualStyleBackColor = true;
		this.checkContrynationalTeam.AutoSize = true;
		this.checkContrynationalTeam.Checked = true;
		this.checkContrynationalTeam.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkContrynationalTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkContrynationalTeam.Location = new System.Drawing.Point(150, 20);
		this.checkContrynationalTeam.Name = "checkContrynationalTeam";
		this.checkContrynationalTeam.Size = new System.Drawing.Size(130, 17);
		this.checkContrynationalTeam.TabIndex = 4;
		this.checkContrynationalTeam.Text = "Linked National Team";
		this.checkContrynationalTeam.UseVisualStyleBackColor = true;
		this.checkCountryMiniFlag.AutoSize = true;
		this.checkCountryMiniFlag.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkCountryMiniFlag.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryMiniFlag.Location = new System.Drawing.Point(20, 112);
		this.checkCountryMiniFlag.Name = "checkCountryMiniFlag";
		this.checkCountryMiniFlag.Size = new System.Drawing.Size(68, 17);
		this.checkCountryMiniFlag.TabIndex = 2;
		this.checkCountryMiniFlag.Text = "Mini Flag";
		this.checkCountryMiniFlag.UseVisualStyleBackColor = true;
		this.checkCountryDatabase.AutoSize = true;
		this.checkCountryDatabase.Checked = true;
		this.checkCountryDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkCountryDatabase.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkCountryDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryDatabase.Location = new System.Drawing.Point(20, 20);
		this.checkCountryDatabase.Name = "checkCountryDatabase";
		this.checkCountryDatabase.Size = new System.Drawing.Size(93, 17);
		this.checkCountryDatabase.TabIndex = 1;
		this.checkCountryDatabase.Text = "Database Info";
		this.checkCountryDatabase.UseVisualStyleBackColor = true;
		this.checkCountryFlag.AutoSize = true;
		this.checkCountryFlag.Checked = true;
		this.checkCountryFlag.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkCountryFlag.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkCountryFlag.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryFlag.Location = new System.Drawing.Point(20, 43);
		this.checkCountryFlag.Name = "checkCountryFlag";
		this.checkCountryFlag.Size = new System.Drawing.Size(46, 17);
		this.checkCountryFlag.TabIndex = 0;
		this.checkCountryFlag.Text = "Flag";
		this.checkCountryFlag.UseVisualStyleBackColor = true;
		this.pageLeagueOptions.Controls.Add(this.checkLeagueBall);
		this.pageLeagueOptions.Controls.Add(this.checkLeagueReferees);
		this.pageLeagueOptions.Controls.Add(this.checkLeagueLinkedTournament);
		this.pageLeagueOptions.Controls.Add(this.checkLeagueLinkedTeams);
		this.pageLeagueOptions.Controls.Add(this.checkLeagueLogo);
		this.pageLeagueOptions.Controls.Add(this.checkLeagueDatabase);
		this.pageLeagueOptions.Location = new System.Drawing.Point(4, 44);
		this.pageLeagueOptions.Name = "pageLeagueOptions";
		this.pageLeagueOptions.Size = new System.Drawing.Size(282, 191);
		this.pageLeagueOptions.TabIndex = 2;
		this.pageLeagueOptions.Text = "Leagues";
		this.pageLeagueOptions.UseVisualStyleBackColor = true;
		this.checkLeagueBall.AutoSize = true;
		this.checkLeagueBall.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueBall.Location = new System.Drawing.Point(150, 101);
		this.checkLeagueBall.Name = "checkLeagueBall";
		this.checkLeagueBall.Size = new System.Drawing.Size(78, 17);
		this.checkLeagueBall.TabIndex = 15;
		this.checkLeagueBall.Text = "Linked Ball";
		this.checkLeagueBall.UseVisualStyleBackColor = true;
		this.checkLeagueBall.Visible = false;
		this.checkLeagueReferees.AutoSize = true;
		this.checkLeagueReferees.Checked = true;
		this.checkLeagueReferees.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkLeagueReferees.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueReferees.Location = new System.Drawing.Point(150, 43);
		this.checkLeagueReferees.Name = "checkLeagueReferees";
		this.checkLeagueReferees.Size = new System.Drawing.Size(104, 17);
		this.checkLeagueReferees.TabIndex = 14;
		this.checkLeagueReferees.Text = "Linked Referees";
		this.checkLeagueReferees.UseVisualStyleBackColor = true;
		this.checkLeagueLinkedTournament.AutoSize = true;
		this.checkLeagueLinkedTournament.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueLinkedTournament.Location = new System.Drawing.Point(150, 124);
		this.checkLeagueLinkedTournament.Name = "checkLeagueLinkedTournament";
		this.checkLeagueLinkedTournament.Size = new System.Drawing.Size(118, 17);
		this.checkLeagueLinkedTournament.TabIndex = 13;
		this.checkLeagueLinkedTournament.Text = "Linked Tournament";
		this.checkLeagueLinkedTournament.UseVisualStyleBackColor = true;
		this.checkLeagueLinkedTournament.Visible = false;
		this.checkLeagueLinkedTeams.AutoSize = true;
		this.checkLeagueLinkedTeams.Checked = true;
		this.checkLeagueLinkedTeams.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkLeagueLinkedTeams.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueLinkedTeams.Location = new System.Drawing.Point(150, 20);
		this.checkLeagueLinkedTeams.Name = "checkLeagueLinkedTeams";
		this.checkLeagueLinkedTeams.Size = new System.Drawing.Size(93, 17);
		this.checkLeagueLinkedTeams.TabIndex = 12;
		this.checkLeagueLinkedTeams.Text = "Linked Teams";
		this.checkLeagueLinkedTeams.UseVisualStyleBackColor = true;
		this.checkLeagueLogo.AutoSize = true;
		this.checkLeagueLogo.Checked = true;
		this.checkLeagueLogo.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkLeagueLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueLogo.Location = new System.Drawing.Point(20, 43);
		this.checkLeagueLogo.Name = "checkLeagueLogo";
		this.checkLeagueLogo.Size = new System.Drawing.Size(55, 17);
		this.checkLeagueLogo.TabIndex = 10;
		this.checkLeagueLogo.Text = "Logos";
		this.checkLeagueLogo.UseVisualStyleBackColor = true;
		this.checkLeagueDatabase.AutoSize = true;
		this.checkLeagueDatabase.Checked = true;
		this.checkLeagueDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkLeagueDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueDatabase.Location = new System.Drawing.Point(20, 20);
		this.checkLeagueDatabase.Name = "checkLeagueDatabase";
		this.checkLeagueDatabase.Size = new System.Drawing.Size(93, 17);
		this.checkLeagueDatabase.TabIndex = 9;
		this.checkLeagueDatabase.Text = "Database Info";
		this.checkLeagueDatabase.UseVisualStyleBackColor = true;
		this.pageTeamOptions.Controls.Add(this.checkTeamStadium);
		this.pageTeamOptions.Controls.Add(this.checkTeamFormation);
		this.pageTeamOptions.Controls.Add(this.checkTeamAdboard);
		this.pageTeamOptions.Controls.Add(this.checkTeamBall);
		this.pageTeamOptions.Controls.Add(this.checkTeamLinkedPlayers);
		this.pageTeamOptions.Controls.Add(this.checkTeamKits);
		this.pageTeamOptions.Controls.Add(this.checkTeamFlags);
		this.pageTeamOptions.Controls.Add(this.checkTeamGuiBanner);
		this.pageTeamOptions.Controls.Add(this.checkTeamGuiLogo);
		this.pageTeamOptions.Controls.Add(this.checkTeamDatabase);
		this.pageTeamOptions.Location = new System.Drawing.Point(4, 44);
		this.pageTeamOptions.Name = "pageTeamOptions";
		this.pageTeamOptions.Padding = new System.Windows.Forms.Padding(3);
		this.pageTeamOptions.Size = new System.Drawing.Size(282, 191);
		this.pageTeamOptions.TabIndex = 1;
		this.pageTeamOptions.Text = "Teams";
		this.pageTeamOptions.UseVisualStyleBackColor = true;
		this.checkTeamStadium.AutoSize = true;
		this.checkTeamStadium.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamStadium.Location = new System.Drawing.Point(150, 135);
		this.checkTeamStadium.Name = "checkTeamStadium";
		this.checkTeamStadium.Size = new System.Drawing.Size(99, 17);
		this.checkTeamStadium.TabIndex = 13;
		this.checkTeamStadium.Text = "Linked Stadium";
		this.checkTeamStadium.UseVisualStyleBackColor = true;
		this.checkTeamFormation.AutoSize = true;
		this.checkTeamFormation.Checked = true;
		this.checkTeamFormation.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamFormation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamFormation.Location = new System.Drawing.Point(150, 66);
		this.checkTeamFormation.Name = "checkTeamFormation";
		this.checkTeamFormation.Size = new System.Drawing.Size(107, 17);
		this.checkTeamFormation.TabIndex = 12;
		this.checkTeamFormation.Text = "Linked Formation";
		this.checkTeamFormation.UseVisualStyleBackColor = true;
		this.checkTeamAdboard.AutoSize = true;
		this.checkTeamAdboard.Checked = true;
		this.checkTeamAdboard.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamAdboard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamAdboard.Location = new System.Drawing.Point(150, 89);
		this.checkTeamAdboard.Name = "checkTeamAdboard";
		this.checkTeamAdboard.Size = new System.Drawing.Size(106, 17);
		this.checkTeamAdboard.TabIndex = 11;
		this.checkTeamAdboard.Text = "Linked Adboards";
		this.checkTeamAdboard.UseVisualStyleBackColor = true;
		this.checkTeamBall.AutoSize = true;
		this.checkTeamBall.Checked = true;
		this.checkTeamBall.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamBall.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamBall.Location = new System.Drawing.Point(150, 112);
		this.checkTeamBall.Name = "checkTeamBall";
		this.checkTeamBall.Size = new System.Drawing.Size(78, 17);
		this.checkTeamBall.TabIndex = 10;
		this.checkTeamBall.Text = "Linked Ball";
		this.checkTeamBall.UseVisualStyleBackColor = true;
		this.checkTeamLinkedPlayers.AutoSize = true;
		this.checkTeamLinkedPlayers.Checked = true;
		this.checkTeamLinkedPlayers.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamLinkedPlayers.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamLinkedPlayers.Location = new System.Drawing.Point(150, 20);
		this.checkTeamLinkedPlayers.Name = "checkTeamLinkedPlayers";
		this.checkTeamLinkedPlayers.Size = new System.Drawing.Size(95, 17);
		this.checkTeamLinkedPlayers.TabIndex = 8;
		this.checkTeamLinkedPlayers.Text = "Linked Players";
		this.checkTeamLinkedPlayers.UseVisualStyleBackColor = true;
		this.checkTeamKits.AutoSize = true;
		this.checkTeamKits.Checked = true;
		this.checkTeamKits.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamKits.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamKits.Location = new System.Drawing.Point(150, 43);
		this.checkTeamKits.Name = "checkTeamKits";
		this.checkTeamKits.Size = new System.Drawing.Size(78, 17);
		this.checkTeamKits.TabIndex = 6;
		this.checkTeamKits.Text = "Linked Kits";
		this.checkTeamKits.UseVisualStyleBackColor = true;
		this.checkTeamFlags.AutoSize = true;
		this.checkTeamFlags.Checked = true;
		this.checkTeamFlags.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamFlags.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamFlags.Location = new System.Drawing.Point(20, 89);
		this.checkTeamFlags.Name = "checkTeamFlags";
		this.checkTeamFlags.Size = new System.Drawing.Size(51, 17);
		this.checkTeamFlags.TabIndex = 5;
		this.checkTeamFlags.Text = "Flags";
		this.checkTeamFlags.UseVisualStyleBackColor = true;
		this.checkTeamGuiBanner.AutoSize = true;
		this.checkTeamGuiBanner.Checked = true;
		this.checkTeamGuiBanner.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamGuiBanner.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamGuiBanner.Location = new System.Drawing.Point(20, 66);
		this.checkTeamGuiBanner.Name = "checkTeamGuiBanner";
		this.checkTeamGuiBanner.Size = new System.Drawing.Size(60, 17);
		this.checkTeamGuiBanner.TabIndex = 3;
		this.checkTeamGuiBanner.Text = "Banner";
		this.checkTeamGuiBanner.UseVisualStyleBackColor = true;
		this.checkTeamGuiLogo.AutoSize = true;
		this.checkTeamGuiLogo.Checked = true;
		this.checkTeamGuiLogo.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamGuiLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamGuiLogo.Location = new System.Drawing.Point(20, 43);
		this.checkTeamGuiLogo.Name = "checkTeamGuiLogo";
		this.checkTeamGuiLogo.Size = new System.Drawing.Size(55, 17);
		this.checkTeamGuiLogo.TabIndex = 2;
		this.checkTeamGuiLogo.Text = "Logos";
		this.checkTeamGuiLogo.UseVisualStyleBackColor = true;
		this.checkTeamDatabase.AutoSize = true;
		this.checkTeamDatabase.Checked = true;
		this.checkTeamDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamDatabase.Location = new System.Drawing.Point(20, 20);
		this.checkTeamDatabase.Name = "checkTeamDatabase";
		this.checkTeamDatabase.Size = new System.Drawing.Size(93, 17);
		this.checkTeamDatabase.TabIndex = 1;
		this.checkTeamDatabase.Text = "Database Info";
		this.checkTeamDatabase.UseVisualStyleBackColor = true;
		this.pageKitOptions.Controls.Add(this.checkKitTextures);
		this.pageKitOptions.Controls.Add(this.checkKitNameFonts);
		this.pageKitOptions.Controls.Add(this.checkKitDatabase);
		this.pageKitOptions.Controls.Add(this.checkKitNumbers);
		this.pageKitOptions.Controls.Add(this.checkKitMinikits);
		this.pageKitOptions.Location = new System.Drawing.Point(4, 44);
		this.pageKitOptions.Name = "pageKitOptions";
		this.pageKitOptions.Size = new System.Drawing.Size(282, 191);
		this.pageKitOptions.TabIndex = 9;
		this.pageKitOptions.Text = "Kits";
		this.pageKitOptions.UseVisualStyleBackColor = true;
		this.checkKitTextures.AutoSize = true;
		this.checkKitTextures.Checked = true;
		this.checkKitTextures.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkKitTextures.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKitTextures.Location = new System.Drawing.Point(20, 43);
		this.checkKitTextures.Name = "checkKitTextures";
		this.checkKitTextures.Size = new System.Drawing.Size(82, 17);
		this.checkKitTextures.TabIndex = 13;
		this.checkKitTextures.Text = "Kit Textures";
		this.checkKitTextures.UseVisualStyleBackColor = true;
		this.checkKitNameFonts.AutoSize = true;
		this.checkKitNameFonts.Checked = true;
		this.checkKitNameFonts.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkKitNameFonts.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKitNameFonts.Location = new System.Drawing.Point(150, 43);
		this.checkKitNameFonts.Name = "checkKitNameFonts";
		this.checkKitNameFonts.Size = new System.Drawing.Size(113, 17);
		this.checkKitNameFonts.TabIndex = 12;
		this.checkKitNameFonts.Text = "Linked Name Font";
		this.checkKitNameFonts.UseVisualStyleBackColor = true;
		this.checkKitDatabase.AutoSize = true;
		this.checkKitDatabase.Checked = true;
		this.checkKitDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkKitDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKitDatabase.Location = new System.Drawing.Point(20, 20);
		this.checkKitDatabase.Name = "checkKitDatabase";
		this.checkKitDatabase.Size = new System.Drawing.Size(93, 17);
		this.checkKitDatabase.TabIndex = 11;
		this.checkKitDatabase.Text = "Database Info";
		this.checkKitDatabase.UseVisualStyleBackColor = true;
		this.checkKitNumbers.AutoSize = true;
		this.checkKitNumbers.Checked = true;
		this.checkKitNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkKitNumbers.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKitNumbers.Location = new System.Drawing.Point(150, 20);
		this.checkKitNumbers.Name = "checkKitNumbers";
		this.checkKitNumbers.Size = new System.Drawing.Size(103, 17);
		this.checkKitNumbers.TabIndex = 10;
		this.checkKitNumbers.Text = "Linked Numbers";
		this.checkKitNumbers.UseVisualStyleBackColor = true;
		this.checkKitMinikits.AutoSize = true;
		this.checkKitMinikits.Checked = true;
		this.checkKitMinikits.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkKitMinikits.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKitMinikits.Location = new System.Drawing.Point(20, 66);
		this.checkKitMinikits.Name = "checkKitMinikits";
		this.checkKitMinikits.Size = new System.Drawing.Size(61, 17);
		this.checkKitMinikits.TabIndex = 8;
		this.checkKitMinikits.Text = "Minikits";
		this.checkKitMinikits.UseVisualStyleBackColor = true;
		this.pagePlayerOptions.Controls.Add(this.checkPlayerGloves);
		this.pagePlayerOptions.Controls.Add(this.checkPlayerShoes);
		this.pagePlayerOptions.Controls.Add(this.checkPlayerMiniface);
		this.pagePlayerOptions.Controls.Add(this.checkPlayerHead);
		this.pagePlayerOptions.Controls.Add(this.checkPlayerDatabase);
		this.pagePlayerOptions.Location = new System.Drawing.Point(4, 44);
		this.pagePlayerOptions.Name = "pagePlayerOptions";
		this.pagePlayerOptions.Padding = new System.Windows.Forms.Padding(3);
		this.pagePlayerOptions.Size = new System.Drawing.Size(282, 191);
		this.pagePlayerOptions.TabIndex = 0;
		this.pagePlayerOptions.Text = "Players";
		this.pagePlayerOptions.UseVisualStyleBackColor = true;
		this.checkPlayerGloves.AutoSize = true;
		this.checkPlayerGloves.Checked = true;
		this.checkPlayerGloves.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerGloves.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerGloves.Location = new System.Drawing.Point(150, 43);
		this.checkPlayerGloves.Name = "checkPlayerGloves";
		this.checkPlayerGloves.Size = new System.Drawing.Size(94, 17);
		this.checkPlayerGloves.TabIndex = 4;
		this.checkPlayerGloves.Text = "Linked Gloves";
		this.checkPlayerGloves.UseVisualStyleBackColor = true;
		this.checkPlayerShoes.AutoSize = true;
		this.checkPlayerShoes.Checked = true;
		this.checkPlayerShoes.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerShoes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerShoes.Location = new System.Drawing.Point(150, 20);
		this.checkPlayerShoes.Name = "checkPlayerShoes";
		this.checkPlayerShoes.Size = new System.Drawing.Size(91, 17);
		this.checkPlayerShoes.TabIndex = 3;
		this.checkPlayerShoes.Text = "Linked Shoes";
		this.checkPlayerShoes.UseVisualStyleBackColor = true;
		this.checkPlayerMiniface.AutoSize = true;
		this.checkPlayerMiniface.Checked = true;
		this.checkPlayerMiniface.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerMiniface.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerMiniface.Location = new System.Drawing.Point(20, 66);
		this.checkPlayerMiniface.Name = "checkPlayerMiniface";
		this.checkPlayerMiniface.Size = new System.Drawing.Size(72, 17);
		this.checkPlayerMiniface.TabIndex = 2;
		this.checkPlayerMiniface.Text = "Mini Face";
		this.checkPlayerMiniface.UseVisualStyleBackColor = true;
		this.checkPlayerHead.AutoSize = true;
		this.checkPlayerHead.Checked = true;
		this.checkPlayerHead.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerHead.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerHead.Location = new System.Drawing.Point(20, 43);
		this.checkPlayerHead.Name = "checkPlayerHead";
		this.checkPlayerHead.Size = new System.Drawing.Size(93, 17);
		this.checkPlayerHead.TabIndex = 1;
		this.checkPlayerHead.Text = "Specific Head";
		this.checkPlayerHead.UseVisualStyleBackColor = true;
		this.checkPlayerDatabase.AutoSize = true;
		this.checkPlayerDatabase.Checked = true;
		this.checkPlayerDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerDatabase.Location = new System.Drawing.Point(20, 20);
		this.checkPlayerDatabase.Name = "checkPlayerDatabase";
		this.checkPlayerDatabase.Size = new System.Drawing.Size(93, 17);
		this.checkPlayerDatabase.TabIndex = 0;
		this.checkPlayerDatabase.Text = "Database Info";
		this.checkPlayerDatabase.UseVisualStyleBackColor = true;
		this.pageRefereeOptions.Controls.Add(this.checkRefereeMiniFace);
		this.pageRefereeOptions.Controls.Add(this.checkRefereeShoes);
		this.pageRefereeOptions.Controls.Add(this.checkRefereeKits);
		this.pageRefereeOptions.Controls.Add(this.checkRefereeDatabase);
		this.pageRefereeOptions.Location = new System.Drawing.Point(4, 44);
		this.pageRefereeOptions.Name = "pageRefereeOptions";
		this.pageRefereeOptions.Size = new System.Drawing.Size(282, 191);
		this.pageRefereeOptions.TabIndex = 5;
		this.pageRefereeOptions.Text = "Referees";
		this.pageRefereeOptions.UseVisualStyleBackColor = true;
		this.checkRefereeMiniFace.AutoSize = true;
		this.checkRefereeMiniFace.Checked = true;
		this.checkRefereeMiniFace.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkRefereeMiniFace.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkRefereeMiniFace.Location = new System.Drawing.Point(20, 43);
		this.checkRefereeMiniFace.Name = "checkRefereeMiniFace";
		this.checkRefereeMiniFace.Size = new System.Drawing.Size(72, 17);
		this.checkRefereeMiniFace.TabIndex = 6;
		this.checkRefereeMiniFace.Text = "Mini Face";
		this.checkRefereeMiniFace.UseVisualStyleBackColor = true;
		this.checkRefereeShoes.AutoSize = true;
		this.checkRefereeShoes.Checked = true;
		this.checkRefereeShoes.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkRefereeShoes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkRefereeShoes.Location = new System.Drawing.Point(150, 20);
		this.checkRefereeShoes.Name = "checkRefereeShoes";
		this.checkRefereeShoes.Size = new System.Drawing.Size(91, 17);
		this.checkRefereeShoes.TabIndex = 5;
		this.checkRefereeShoes.Text = "Linked Shoes";
		this.checkRefereeShoes.UseVisualStyleBackColor = true;
		this.checkRefereeKits.AutoSize = true;
		this.checkRefereeKits.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkRefereeKits.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkRefereeKits.Location = new System.Drawing.Point(20, 73);
		this.checkRefereeKits.Name = "checkRefereeKits";
		this.checkRefereeKits.Size = new System.Drawing.Size(84, 17);
		this.checkRefereeKits.TabIndex = 4;
		this.checkRefereeKits.Text = "Referee Kits";
		this.checkRefereeKits.UseVisualStyleBackColor = true;
		this.checkRefereeKits.Visible = false;
		this.checkRefereeDatabase.AutoSize = true;
		this.checkRefereeDatabase.BackColor = System.Drawing.Color.Transparent;
		this.checkRefereeDatabase.Checked = true;
		this.checkRefereeDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkRefereeDatabase.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkRefereeDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkRefereeDatabase.Location = new System.Drawing.Point(20, 20);
		this.checkRefereeDatabase.Name = "checkRefereeDatabase";
		this.checkRefereeDatabase.Size = new System.Drawing.Size(93, 17);
		this.checkRefereeDatabase.TabIndex = 2;
		this.checkRefereeDatabase.Text = "Database Info";
		this.checkRefereeDatabase.UseVisualStyleBackColor = false;
		this.pageStadiumOptions.Controls.Add(this.checkStadiumMowingPattern);
		this.pageStadiumOptions.Controls.Add(this.checkStadiumModel);
		this.pageStadiumOptions.Controls.Add(this.checkStadiumPreview);
		this.pageStadiumOptions.Controls.Add(this.checkStadiumDatabase);
		this.pageStadiumOptions.Controls.Add(this.checkStadiumNet);
		this.pageStadiumOptions.Location = new System.Drawing.Point(4, 44);
		this.pageStadiumOptions.Name = "pageStadiumOptions";
		this.pageStadiumOptions.Size = new System.Drawing.Size(282, 191);
		this.pageStadiumOptions.TabIndex = 6;
		this.pageStadiumOptions.Text = "Stadiums";
		this.pageStadiumOptions.UseVisualStyleBackColor = true;
		this.checkStadiumMowingPattern.AutoSize = true;
		this.checkStadiumMowingPattern.Checked = true;
		this.checkStadiumMowingPattern.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumMowingPattern.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumMowingPattern.Location = new System.Drawing.Point(144, 43);
		this.checkStadiumMowingPattern.Name = "checkStadiumMowingPattern";
		this.checkStadiumMowingPattern.Size = new System.Drawing.Size(135, 17);
		this.checkStadiumMowingPattern.TabIndex = 17;
		this.checkStadiumMowingPattern.Text = "Linked Mowing Pattern";
		this.checkStadiumMowingPattern.UseVisualStyleBackColor = true;
		this.checkStadiumModel.AutoSize = true;
		this.checkStadiumModel.Checked = true;
		this.checkStadiumModel.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumModel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumModel.Location = new System.Drawing.Point(20, 43);
		this.checkStadiumModel.Name = "checkStadiumModel";
		this.checkStadiumModel.Size = new System.Drawing.Size(77, 17);
		this.checkStadiumModel.TabIndex = 15;
		this.checkStadiumModel.Text = "3D Models";
		this.checkStadiumModel.UseVisualStyleBackColor = true;
		this.checkStadiumPreview.AutoSize = true;
		this.checkStadiumPreview.Checked = true;
		this.checkStadiumPreview.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumPreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumPreview.Location = new System.Drawing.Point(20, 66);
		this.checkStadiumPreview.Name = "checkStadiumPreview";
		this.checkStadiumPreview.Size = new System.Drawing.Size(105, 17);
		this.checkStadiumPreview.TabIndex = 13;
		this.checkStadiumPreview.Text = "Preview Pictures";
		this.checkStadiumPreview.UseVisualStyleBackColor = true;
		this.checkStadiumDatabase.AutoSize = true;
		this.checkStadiumDatabase.Checked = true;
		this.checkStadiumDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumDatabase.Location = new System.Drawing.Point(20, 20);
		this.checkStadiumDatabase.Name = "checkStadiumDatabase";
		this.checkStadiumDatabase.Size = new System.Drawing.Size(93, 17);
		this.checkStadiumDatabase.TabIndex = 12;
		this.checkStadiumDatabase.Text = "Database Info";
		this.checkStadiumDatabase.UseVisualStyleBackColor = true;
		this.checkStadiumNet.AutoSize = true;
		this.checkStadiumNet.Checked = true;
		this.checkStadiumNet.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumNet.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumNet.Location = new System.Drawing.Point(144, 20);
		this.checkStadiumNet.Name = "checkStadiumNet";
		this.checkStadiumNet.Size = new System.Drawing.Size(78, 17);
		this.checkStadiumNet.TabIndex = 10;
		this.checkStadiumNet.Text = "Linked Net";
		this.checkStadiumNet.UseVisualStyleBackColor = true;
		this.labelPatchType.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelPatchType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPatchType.Location = new System.Drawing.Point(0, 0);
		this.labelPatchType.Name = "labelPatchType";
		this.labelPatchType.Size = new System.Drawing.Size(296, 48);
		this.labelPatchType.TabIndex = 6;
		this.labelPatchType.Text = "Objects Selection";
		this.labelPatchType.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.patchToolStripMenuItem });
		this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
		this.mainMenuStrip.Name = "mainMenuStrip";
		this.mainMenuStrip.Size = new System.Drawing.Size(1028, 24);
		this.mainMenuStrip.TabIndex = 29;
		this.mainMenuStrip.Text = "menuStrip1";
		this.patchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.newPatchToolStripMenuItem, this.createPatchToolStripMenuItem, this.openPatchToolStripMenuItem, this.exitToolStripMenuItem });
		this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
		this.patchToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
		this.patchToolStripMenuItem.Text = "Patch";
		this.newPatchToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("newPatchToolStripMenuItem.Image");
		this.newPatchToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.newPatchToolStripMenuItem.Name = "newPatchToolStripMenuItem";
		this.newPatchToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
		this.newPatchToolStripMenuItem.Text = "New Patch";
		this.newPatchToolStripMenuItem.Click += new System.EventHandler(newPatchToolStripMenuItem_Click);
		this.createPatchToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("createPatchToolStripMenuItem.Image");
		this.createPatchToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.createPatchToolStripMenuItem.Name = "createPatchToolStripMenuItem";
		this.createPatchToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
		this.createPatchToolStripMenuItem.Text = "Create Patch";
		this.createPatchToolStripMenuItem.Click += new System.EventHandler(createPatchToolStripMenuItem_Click);
		this.openPatchToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openPatchToolStripMenuItem.Image");
		this.openPatchToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.openPatchToolStripMenuItem.Name = "openPatchToolStripMenuItem";
		this.openPatchToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
		this.openPatchToolStripMenuItem.Text = "Open Patch";
		this.openPatchToolStripMenuItem.Click += new System.EventHandler(openPatchToolStripMenuItem_Click);
		this.exitToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("exitToolStripMenuItem.Image");
		this.exitToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
		this.exitToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
		this.exitToolStripMenuItem.Text = "Exit";
		this.exitToolStripMenuItem.Click += new System.EventHandler(exitToolStripMenuItem_Click);
		this.toolMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.buttonNewPatch, this.buttonOpenPatch, this.buttonCreatePatch, this.buttonExit });
		this.toolMain.Location = new System.Drawing.Point(0, 24);
		this.toolMain.Name = "toolMain";
		this.toolMain.Size = new System.Drawing.Size(1028, 25);
		this.toolMain.TabIndex = 30;
		this.toolMain.Text = "toolStrip2";
		this.buttonNewPatch.AutoSize = false;
		this.buttonNewPatch.Image = (System.Drawing.Image)resources.GetObject("buttonNewPatch.Image");
		this.buttonNewPatch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonNewPatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonNewPatch.Name = "buttonNewPatch";
		this.buttonNewPatch.Size = new System.Drawing.Size(90, 22);
		this.buttonNewPatch.Text = "New";
		this.buttonNewPatch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.buttonNewPatch.Click += new System.EventHandler(buttonNewPatch_Click);
		this.buttonOpenPatch.AutoSize = false;
		this.buttonOpenPatch.Image = (System.Drawing.Image)resources.GetObject("buttonOpenPatch.Image");
		this.buttonOpenPatch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonOpenPatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonOpenPatch.Name = "buttonOpenPatch";
		this.buttonOpenPatch.Size = new System.Drawing.Size(90, 22);
		this.buttonOpenPatch.Text = "Open";
		this.buttonOpenPatch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.buttonOpenPatch.Click += new System.EventHandler(buttonOpenPatch_Click);
		this.buttonCreatePatch.AutoSize = false;
		this.buttonCreatePatch.Image = (System.Drawing.Image)resources.GetObject("buttonCreatePatch.Image");
		this.buttonCreatePatch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonCreatePatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCreatePatch.Name = "buttonCreatePatch";
		this.buttonCreatePatch.Size = new System.Drawing.Size(90, 22);
		this.buttonCreatePatch.Text = "Create";
		this.buttonCreatePatch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.buttonCreatePatch.Click += new System.EventHandler(buttonCreatePatch_Click);
		this.buttonExit.AutoSize = false;
		this.buttonExit.Image = (System.Drawing.Image)resources.GetObject("buttonExit.Image");
		this.buttonExit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonExit.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExit.Name = "buttonExit";
		this.buttonExit.Size = new System.Drawing.Size(90, 22);
		this.buttonExit.Text = "Exit";
		this.buttonExit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.buttonExit.Click += new System.EventHandler(buttonExit_Click);
		this.openFileDialog.FileName = "openFileDialog";
		this.m_PatchDataSet.DataSetName = "Patch";
		this.m_PatchDataSet.Locale = new System.Globalization.CultureInfo("");
		this.m_PatchDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1028, 746);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.panelLeft);
		base.Controls.Add(this.toolMain);
		base.Controls.Add(this.mainMenuStrip);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MainMenuStrip = this.mainMenuStrip;
		base.Name = "PatchCreatorForm";
		this.Text = " CM-Patch Creator";
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		this.splitContainer1.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.toolAddRemove.ResumeLayout(false);
		this.toolAddRemove.PerformLayout();
		this.panelLeft.ResumeLayout(false);
		this.panelLeft.PerformLayout();
		this.statusBar.ResumeLayout(false);
		this.statusBar.PerformLayout();
		this.groupPatchOptions.ResumeLayout(false);
		this.tabPatchOptions.ResumeLayout(false);
		this.pageGeneralOptions.ResumeLayout(false);
		this.pageGeneralOptions.PerformLayout();
		this.pageCountryOptions.ResumeLayout(false);
		this.pageCountryOptions.PerformLayout();
		this.pageLeagueOptions.ResumeLayout(false);
		this.pageLeagueOptions.PerformLayout();
		this.pageTeamOptions.ResumeLayout(false);
		this.pageTeamOptions.PerformLayout();
		this.pageKitOptions.ResumeLayout(false);
		this.pageKitOptions.PerformLayout();
		this.pagePlayerOptions.ResumeLayout(false);
		this.pagePlayerOptions.PerformLayout();
		this.pageRefereeOptions.ResumeLayout(false);
		this.pageRefereeOptions.PerformLayout();
		this.pageStadiumOptions.ResumeLayout(false);
		this.pageStadiumOptions.PerformLayout();
		this.mainMenuStrip.ResumeLayout(false);
		this.mainMenuStrip.PerformLayout();
		this.toolMain.ResumeLayout(false);
		this.toolMain.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.m_PatchDataSet).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
