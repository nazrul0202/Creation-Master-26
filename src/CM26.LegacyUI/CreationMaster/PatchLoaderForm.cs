using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;
using ICSharpCode.SharpZipLib.Zip;

namespace CreationMaster;

public class PatchLoaderForm : Form
{
	public string m_TempFolder;

	private DataSet m_FifaDataSet = new DataSet("FIFA16");

	private DataSet m_LangDataSet = new DataSet("LANG16");

	private Patch m_PatchDataSet = new Patch();

	private int m_PatchYear;

	private int m_PatchDatabaseVersion;

	private bool m_IsLastObjectCrossReferenced;

	private PatchedObject m_CurrentPatchedObject;

	private IContainer components;

	private MenuStrip mainMenu;

	private ToolStrip toolMain;

	private ToolStripButton buttonLoadPatch;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonImportPatch;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton buttonExitCreator;

	private ToolStripButton buttonSelectAllObjects;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripButton buttonDeselectAllObjects;

	private ToolStripButton stripButtonPreview;

	private ToolStripMenuItem patchToolStripMenuItem;

	private ToolStripMenuItem openToolStripMenuItem;

	private ToolStripMenuItem importToolStripMenuItem;

	private ToolStripMenuItem exitToolStripMenuItem;

	private Panel panelLeft;

	private GroupBox groupPatchOptions;

	private TabControl tabPatchOptions;

	private TabPage pagePlayerOptions;

	public CheckBox checkPlayerMiniface;

	public CheckBox checkPlayerHead;

	public CheckBox checkPlayerDatabase;

	private TabPage pageTeamOptions;

	public CheckBox checkTeamFlags;

	public CheckBox checkTeamBanner;

	public CheckBox checkTeamLogo;

	public CheckBox checkTeamDatabase;

	private TabPage pageLeagueOptions;

	public CheckBox checkLeagueLogo;

	public CheckBox checkLeagueDatabase;

	private TabPage pageCountryOptions;

	public CheckBox checkCountryDatabase;

	public CheckBox checkCountryFlag;

	private TabPage pageStadiumOptions;

	public CheckBox checkStadiumModel;

	public CheckBox checkStadiumPreview;

	public CheckBox checkStadiumDatabase;

	private TabPage pageKitOptions;

	public CheckBox checkKitDatabase;

	private TextBox textDescription;

	private Label labelDescription;

	private TextBox textPatchVersion;

	private Label labelPatchVersion;

	private TextBox textPatchName;

	private Label labelPatchName;

	private SplitContainer splitContainer1;

	private StatusStrip statusBar;

	private ToolStripStatusLabel statusLabel;

	private ListView listViewPatch;

	private ColumnHeader columnItem;

	private ColumnHeader columnType;

	private ColumnHeader columnPatchId;

	private ColumnHeader columnComment;

	private Panel panelRight;

	private TabControl tabPreview;

	private TabPage pageViewer2D;

	private Panel panelGraphicGroups;

	private GroupBox groupBall;

	private RadioButton radioBallPreview;

	private RadioButton radioBallTexture;

	private GroupBox groupCountry;

	private RadioButton radioCountryMainFlag;

	private RadioButton radioCountryMiniflag;

	private GroupBox groupPlayer;

	private RadioButton radioFaceTexture;

	private RadioButton radioMiniHead;

	private GroupBox groupLeague;

	private GroupBox groupTeam;

	private RadioButton radioTeamFlags;

	private RadioButton radioTeamBanners;

	private GroupBox groupStadium;

	private RadioButton radioStadiumGuiSunset;

	private RadioButton radioStadiumGuiOvercast;

	private RadioButton radioStadiumGuiClearDay;

	private RadioButton radioStadium3D;

	private RadioButton radioStadiumGuiNight;

	private GroupBox groupAdboards;

	private RadioButton radioAdboard1;

	private PictureBox pictureBox1;

	private GroupBox groupReplaceSelection;

	private ComboBox comboReplaceKit;

	private Label labelCmsCreated;

	private Label labelCmsReplaced;

	private TextBox textCmsReplaced;

	private ComboBox comboReplaceMowingPattern;

	private RadioButton radioCmsItem;

	private ComboBox comboReplaceGkGloves;

	private ComboBox comboReplaceNet;

	private ComboBox comboReplaceShoes;

	private ComboBox comboReplaceNamesFont;

	private ComboBox comboReplaceNumberFont;

	private ComboBox comboReplaceAdboard;

	private ComboBox comboReplaceBall;

	private ComboBox comboReplaceReferee;

	private ComboBox comboReplaceSponsor;

	private ComboBox comboReplaceFormation;

	private ComboBox comboReplaceTournament;

	private ComboBox comboReplaceStadium;

	private ComboBox comboReplaceCountry;

	private ComboBox comboReplaceLeague;

	private ComboBox comboReplacePlayer;

	private ComboBox comboReplaceTeam;

	private RadioButton radioReplaceItem;

	private RadioButton radioCreateItem;

	private Label labelDetails;

	private TabPage pageMultiViewer2D;

	public CheckBox checkMinikits;

	public CheckBox checkKits;

	private Viewer2D viewer2D;

	private MultiViewer2D multiViewer2D;

	private RadioButton radioEyesTexture;

	private RadioButton radioHairTextures;

	private RadioButton radioHairColorTexture;

	private GroupBox groupKit;

	private RadioButton radioKitKit;

	private RadioButton radioKitMinikit;

	private GroupBox groupShoes;

	private RadioButton radioShoesColor;

	private RadioButton radioStadiumPreview;

	private GroupBox groupTod;

	private ColumnHeader columnImportId;

	private ComboBox comboReplaceLicensedTournament;

	private RadioButton radioCountryCard;

	private RadioButton radioLeagueTinyLogo;

	private RadioButton radioLeagueSmallLogo;

	private RadioButton radioLeagueAnimLogo;

	private RadioButton radioTeamCrest16;

	private RadioButton radioTeamCrest32;

	private RadioButton radioTeamCrestLarge;

	private RadioButton radioLeagueLogo512x128;

	private RadioButton radioCountryFlag512x512;

	private RadioButton radioCountryMap;

	public CheckBox checkCountryMap;

	private RadioButton radioTeamCrest50;

	private GroupBox groupDualClub;

	public RadioButton radioPutInBothTeams;

	public RadioButton radioTransferToNewTeam;

	public RadioButton radioLeaveInExistingTeam;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripButton buttonSelectNewObjects;

	public int PatchYear => m_PatchYear;

	public bool IsLastObjectCrossReferenced => m_IsLastObjectCrossReferenced;

	public PatchLoaderForm()
	{
		InitializeComponent();
		m_FifaDataSet.Locale = CultureInfo.InvariantCulture;
		m_LangDataSet.Locale = CultureInfo.InvariantCulture;
		m_PatchDataSet.Locale = CultureInfo.InvariantCulture;
		InitPatchLoaderForm();
	}

	private void buttonLoadPatch_Click(object sender, EventArgs e)
	{
		OpenPatch();
	}

	private void OpenPatch()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.CheckFileExists = true;
		openFileDialog.Title = "Open Creation Master Patch file";
		openFileDialog.Filter = "Creation Master Patch (*.cmp)|*.cmp";
		openFileDialog.FilterIndex = 1;
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			openFileDialog.Dispose();
			return;
		}
		string fileName = openFileDialog.FileName;
		openFileDialog.Dispose();
		if (File.Exists(fileName))
		{
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
			bool enabled = OpenCM12();
			buttonImportPatch.Enabled = enabled;
			importToolStripMenuItem.Enabled = enabled;
			Cursor.Current = Cursors.Default;
		}
	}

	private void openToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenPatch();
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

	private void ZipExtractAllFiles(ZipInputStream zip, string exportFolder)
	{
		ZipEntry nextEntry;
		while ((nextEntry = zip.GetNextEntry()) != null)
		{
			ZipExtractSingleFile(zip, nextEntry, exportFolder);
		}
	}

	private void RemoveAllNewObjects()
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			((PatchedObject)item.Tag).RemoveNewObject();
		}
	}

	private void RemoveAllUnusedObjects()
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			PatchedObject patchedObject = (PatchedObject)item.Tag;
			if (!item.Checked)
			{
				patchedObject.RemoveNewObject();
			}
			else
			{
				patchedObject.RemoveNewObjectIfUnused();
			}
		}
	}

	private bool OpenCM12()
	{
		statusLabel.Text = "Loading...";
		statusBar.Refresh();
		m_FifaDataSet.Locale = CultureInfo.InvariantCulture;
		m_LangDataSet.Locale = CultureInfo.InvariantCulture;
		m_PatchDataSet.Locale = CultureInfo.InvariantCulture;
		m_PatchDataSet.Tables.Clear();
		m_FifaDataSet.Tables.Clear();
		m_LangDataSet.Tables.Clear();
		m_PatchDataSet.ReadXml(m_TempFolder + "\\Patch.xml");
		m_FifaDataSet.ReadXml(m_TempFolder + "\\fifa.xml");
		m_LangDataSet.ReadXml(m_TempFolder + "\\lang.xml");
		if (m_FifaDataSet.DataSetName != "FIFA14" && m_FifaDataSet.DataSetName != "FIFA15" && m_FifaDataSet.DataSetName != "FIFA16")
		{
			FifaEnvironment.UserMessages.ShowMessage(1032);
			return false;
		}
		if (m_FifaDataSet.DataSetName == "FIFA14")
		{
			m_PatchYear = 14;
		}
		else if (m_FifaDataSet.DataSetName == "FIFA15")
		{
			m_PatchYear = 15;
		}
		else if (m_FifaDataSet.DataSetName == "FIFA16")
		{
			m_PatchYear = 16;
		}
		comboReplaceTeam.Items.Clear();
		comboReplaceTeam.Items.AddRange(FifaEnvironment.Teams.ToArray());
		comboReplaceTeam.Sorted = true;
		comboReplacePlayer.Items.Clear();
		comboReplacePlayer.Items.AddRange(FifaEnvironment.Players.ToArray());
		comboReplacePlayer.Sorted = true;
		comboReplaceLeague.Items.Clear();
		comboReplaceLeague.Items.AddRange(FifaEnvironment.Leagues.ToArray());
		comboReplaceLeague.Sorted = true;
		comboReplaceCountry.Items.Clear();
		comboReplaceCountry.Items.AddRange(FifaEnvironment.Countries.ToArray());
		comboReplaceCountry.Sorted = true;
		comboReplaceStadium.Items.Clear();
		comboReplaceStadium.Items.AddRange(FifaEnvironment.Stadiums.ToArray());
		comboReplaceStadium.Sorted = true;
		comboReplaceReferee.Items.Clear();
		comboReplaceReferee.Items.AddRange(FifaEnvironment.Referees.ToArray());
		comboReplaceReferee.Sorted = true;
		comboReplaceFormation.Items.Clear();
		comboReplaceFormation.Items.AddRange(FifaEnvironment.Formations.ToArray());
		comboReplaceBall.Items.Clear();
		comboReplaceBall.Items.AddRange(FifaEnvironment.Balls.ToArray());
		comboReplaceAdboard.Items.Clear();
		comboReplaceAdboard.Items.AddRange(FifaEnvironment.Adboards.ToArray());
		comboReplaceNumberFont.Items.Clear();
		comboReplaceNumberFont.Items.AddRange(FifaEnvironment.NumberFonts.ToArray());
		comboReplaceNamesFont.Items.Clear();
		comboReplaceNamesFont.Items.AddRange(FifaEnvironment.NameFonts.ToArray());
		comboReplaceShoes.Items.Clear();
		comboReplaceShoes.Items.AddRange(FifaEnvironment.Shoes.ToArray());
		comboReplaceNet.Items.Clear();
		comboReplaceNet.Items.AddRange(FifaEnvironment.Nets.ToArray());
		comboReplaceGkGloves.Items.Clear();
		comboReplaceGkGloves.Items.AddRange(FifaEnvironment.GkGloves.ToArray());
		comboReplaceMowingPattern.Items.Clear();
		comboReplaceMowingPattern.Items.AddRange(FifaEnvironment.MowingPatterns.ToArray());
		comboReplaceKit.Items.Clear();
		comboReplaceKit.Items.AddRange(FifaEnvironment.Kits.ToArray());
		labelDetails.Text = "Patch created for " + m_FifaDataSet.DataSetName;
		panelLeft.Enabled = true;
		panelRight.Enabled = true;
		textPatchName.Text = (string)m_PatchDataSet.Tables["PatchIdentity"].Rows[0].ItemArray[0];
		textPatchVersion.Text = (string)m_PatchDataSet.Tables["PatchIdentity"].Rows[0].ItemArray[1];
		textDescription.Text = (string)m_PatchDataSet.Tables["PatchIdentity"].Rows[0].ItemArray[2];
		string text = (string)m_PatchDataSet.Tables["PatchIdentity"].Rows[0].ItemArray[3];
		if (text != null && text != string.Empty)
		{
			m_PatchDatabaseVersion = Convert.ToInt32(text);
		}
		else
		{
			m_PatchDatabaseVersion = 0;
		}
		listViewPatch.Items.Clear();
		foreach (DataRow row in m_PatchDataSet.Tables["PatchElements"].Rows)
		{
			_ = row.ItemArray.Length;
			string[] array = new string[5];
			array[4] = (string)row.ItemArray[0];
			array[3] = string.Empty;
			array[1] = (string)row.ItemArray[1];
			array[2] = (string)row.ItemArray[2];
			array[0] = (string)row.ItemArray[3];
			int id = Convert.ToInt32(array[2]);
			PatchedObject patchedObject = new PatchedObject(array[1], array[0], id);
			patchedObject.AssignReplacedObject();
			ListViewItem listViewItem = new ListViewItem(array);
			listViewItem.Tag = patchedObject;
			listViewPatch.Items.Add(listViewItem);
		}
		foreach (ListViewItem item in listViewPatch.Items)
		{
			((PatchedObject)item.Tag).AssignNewCmsObject();
		}
		foreach (ListViewItem item2 in listViewPatch.Items)
		{
			PatchedObject patchedObject2 = (PatchedObject)item2.Tag;
			patchedObject2.AssignNewObject();
			item2.ForeColor = patchedObject2.GetColor();
			item2.Checked = item2.ForeColor == Color.Green;
			item2.SubItems[3] = new ListViewItem.ListViewSubItem(item2, patchedObject2.ImportId.ToString());
		}
		statusLabel.Text = "Ready";
		statusBar.Refresh();
		return true;
	}

	public bool IsItemChecked(string type, string name)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			PatchedObject patchedObject = (PatchedObject)item.Tag;
			if (patchedObject.GetObjectType() == type && patchedObject.Name == name)
			{
				return item.Checked;
			}
		}
		return false;
	}

	private void InitPatchLoaderForm()
	{
		m_TempFolder = FifaEnvironment.TempFolder + "\\Patch";
		listViewPatch.Items.Clear();
		buttonImportPatch.Enabled = false;
		panelGraphicGroups.Visible = false;
		tabPreview.Visible = false;
		stripButtonPreview.Checked = false;
		labelDetails.Text = string.Empty;
		textPatchName.Text = string.Empty;
		textPatchVersion.Text = string.Empty;
		textDescription.Text = string.Empty;
		viewer2D.CurrentBitmap = null;
		multiViewer2D.Bitmaps = null;
	}

	public int CrossReference(string type, int id)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			PatchedObject patchedObject = (PatchedObject)item.Tag;
			if (patchedObject.Id == id && patchedObject.GetObjectType() == type)
			{
				m_IsLastObjectCrossReferenced = true;
				return patchedObject.ImportId;
			}
		}
		m_IsLastObjectCrossReferenced = false;
		return id;
	}

	private void listViewPatch_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listViewPatch.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			m_CurrentPatchedObject = (PatchedObject)listViewItem.Tag;
			radioCreateItem.Checked = m_CurrentPatchedObject.IsUsedNewObject();
			radioReplaceItem.Checked = m_CurrentPatchedObject.IsUsedFittingObject();
			radioCmsItem.Checked = m_CurrentPatchedObject.IsUsedCmsObject();
			UpdateComboReplace(m_CurrentPatchedObject);
			UpdateTextCms(m_CurrentPatchedObject);
			SelectViewerRadio();
			Preview();
		}
	}

	private void UpdateTextCms(PatchedObject patchedObject)
	{
		if (patchedObject.IsUsedCmsObject())
		{
			textCmsReplaced.Text = patchedObject.CmsObject.ToString();
			textCmsReplaced.Visible = !patchedObject.IsCmsNew;
			labelCmsCreated.Visible = patchedObject.IsCmsNew;
			labelCmsCreated.Text = "Create with id = " + patchedObject.ImportId;
			labelCmsReplaced.Visible = !patchedObject.IsCmsNew;
		}
		else
		{
			textCmsReplaced.Visible = false;
			labelCmsCreated.Visible = false;
			labelCmsReplaced.Visible = false;
		}
	}

	private void UpdateComboReplace(PatchedObject patchedObject)
	{
		string type = patchedObject.Type;
		_ = patchedObject.Id;
		_ = patchedObject.ReplacedObject;
		if (comboReplacePlayer.Visible = type == "Player" && patchedObject.IsUsedFittingObject())
		{
			comboReplacePlayer.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceTeam.Visible = type == "Team" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceTeam.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceLeague.Visible = type == "League" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceLeague.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceCountry.Visible = type == "Country" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceCountry.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceStadium.Visible = type == "Stadium" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceStadium.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceReferee.Visible = type == "Referee" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceReferee.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceFormation.Visible = type == "Formation" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceFormation.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceSponsor.Visible = type == "Sponsor" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceSponsor.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceBall.Visible = type == "Ball" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceBall.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceAdboard.Visible = type == "Adboard" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceAdboard.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceNumberFont.Visible = type == "NumberFont" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceNumberFont.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceNamesFont.Visible = type == "NameFont" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceNamesFont.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceShoes.Visible = type == "Shoes" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceShoes.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceNet.Visible = type == "Net" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceNet.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceGkGloves.Visible = type == "GkGloves" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceGkGloves.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceMowingPattern.Visible = type == "MowingPattern" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceMowingPattern.SelectedItem = patchedObject.ReplacedObject;
		}
		if (comboReplaceKit.Visible = type == "Kit" && patchedObject.IsUsedFittingObject())
		{
			comboReplaceKit.SelectedItem = patchedObject.ReplacedObject;
		}
	}

	private void SelectViewerRadio()
	{
		string text = listViewPatch.SelectedItems[0].SubItems[1].Text;
		groupPlayer.Visible = text == "Player";
		groupTeam.Visible = text == "Team";
		groupLeague.Visible = text == "League";
		groupStadium.Visible = text == "Stadium";
		groupCountry.Visible = text == "Country";
		groupBall.Visible = text == "Ball";
		groupShoes.Visible = text == "Shoes";
		groupAdboards.Visible = text == "Adboard";
		groupKit.Visible = text == "Kit";
		if (stripButtonPreview.Checked)
		{
			Preview();
		}
	}

	private void radioViewer_CheckedChanged(object sender, EventArgs e)
	{
		if (((RadioButton)sender).Checked)
		{
			Preview();
		}
	}

	private void buttonSelectAll_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			item.Checked = true;
		}
	}

	private void buttonDeselectAll_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			item.Checked = false;
		}
	}

	private void radioCreateItem_CheckedChanged(object sender, EventArgs e)
	{
		if (radioCreateItem.Checked && listViewPatch.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			PatchedObject patchedObject = (PatchedObject)listViewItem.Tag;
			patchedObject.UseNewObject();
			if (!patchedObject.IsUsedNewObject())
			{
				radioCreateItem.Checked = false;
				radioCmsItem.Checked = false;
				radioReplaceItem.Checked = true;
			}
			listViewItem.ForeColor = patchedObject.GetColor();
			listViewItem.SubItems[3] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
			UpdateComboReplace(patchedObject);
			UpdateTextCms(patchedObject);
		}
	}

	private void radioReplaceItem_CheckedChanged(object sender, EventArgs e)
	{
		if (radioReplaceItem.Checked && listViewPatch.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			PatchedObject patchedObject = (PatchedObject)listViewItem.Tag;
			patchedObject.UseReplacedObject();
			listViewItem.ForeColor = patchedObject.GetColor();
			listViewItem.SubItems[3] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
			UpdateComboReplace(patchedObject);
			UpdateTextCms(patchedObject);
		}
	}

	private void radioUsePatchItem_CheckedChanged(object sender, EventArgs e)
	{
		if (radioCmsItem.Checked && listViewPatch.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			PatchedObject patchedObject = (PatchedObject)listViewItem.Tag;
			patchedObject.UsePatchId();
			listViewItem.ForeColor = patchedObject.GetColor();
			listViewItem.SubItems[3] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
			UpdateComboReplace(patchedObject);
			UpdateTextCms(patchedObject);
		}
	}

	private void stripButtonPreview_Click(object sender, EventArgs e)
	{
		if (!stripButtonPreview.Checked)
		{
			tabPreview.Visible = false;
			panelGraphicGroups.Visible = false;
		}
		else
		{
			tabPreview.Visible = true;
			panelGraphicGroups.Visible = true;
			Preview();
		}
	}

	private void Preview()
	{
		if (!stripButtonPreview.Checked)
		{
			tabPreview.Visible = false;
		}
		else
		{
			if (listViewPatch.SelectedItems.Count <= 0)
			{
				return;
			}
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			string text = listViewItem.SubItems[1].Text;
			int num = Convert.ToInt32(listViewItem.SubItems[2].Text);
			string text2 = null;
			string text3 = null;
			string text4 = null;
			switch (text)
			{
			case "Country":
				if (radioCountryMainFlag.Checked)
				{
					text2 = m_TempFolder + "\\" + Country.FlagBigFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowBigFile(text2);
				}
				else if (radioCountryMiniflag.Checked)
				{
					text2 = m_TempFolder + "\\" + Country.MiniFlagBigFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowBigFile(text2);
				}
				else if (radioCountryCard.Checked)
				{
					text2 = m_TempFolder + "\\" + Country.CardFlagBigFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowBigFile(text2);
				}
				else if (radioCountryFlag512x512.Checked)
				{
					text4 = m_TempFolder + "\\" + Country.Flag512DdsFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				else if (radioCountryCard.Checked)
				{
					text4 = m_TempFolder + "\\" + Country.ShapeFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				break;
			case "League":
				if (radioLeagueTinyLogo.Checked)
				{
					text4 = m_TempFolder + "\\" + League.TinyLogoDdsFileName(num, m_PatchYear);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				else if (radioLeagueAnimLogo.Checked)
				{
					text4 = m_TempFolder + "\\" + League.AnimLogoDdsFileName(num, m_PatchYear);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				else if (radioLeagueSmallLogo.Checked)
				{
					text4 = m_TempFolder + "\\" + League.SmallLogoDdsFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				else if (radioLeagueLogo512x128.Checked)
				{
					text4 = m_TempFolder + "\\" + League.Logo512x128DdsFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				break;
			case "Team":
				if (radioTeamBanners.Checked)
				{
					text3 = m_TempFolder + "\\" + Team.BannerFileName(num);
					ShowRx3File(text3);
				}
				else if (radioTeamFlags.Checked)
				{
					text3 = m_TempFolder + "\\" + Team.FlagFileName(num);
					ShowRx3File(text3);
				}
				else if (radioTeamCrestLarge.Checked)
				{
					text4 = m_TempFolder + "\\" + Team.CrestDdsFileName(num, m_PatchYear);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				else if (radioTeamCrest50.Checked)
				{
					text4 = m_TempFolder + "\\" + Team.Crest50DdsFileName(num, m_PatchYear);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				else if (radioTeamCrest32.Checked)
				{
					text4 = m_TempFolder + "\\" + Team.Crest32DdsFileName(num, m_PatchYear);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				else if (radioTeamCrest16.Checked)
				{
					text4 = m_TempFolder + "\\" + Team.Crest16DdsFileName(num, m_PatchYear);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				break;
			case "Player":
				if (radioEyesTexture.Checked)
				{
					text3 = m_TempFolder + "\\" + Player.SpecificEyesTextureFileName(num);
					ShowRx3File(text3);
				}
				else if (radioFaceTexture.Checked)
				{
					text3 = m_TempFolder + "\\" + Player.SpecificFaceTextureFileName(num);
					ShowRx3File(text3);
				}
				else if (radioHairTextures.Checked)
				{
					text3 = m_TempFolder + "\\" + Player.SpecificHairTexturesFileName(num);
					ShowRx3File(text3);
				}
				else if (radioMiniHead.Checked)
				{
					text4 = m_TempFolder + "\\" + Player.SpecificPhotoDdsFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				break;
			case "Kit":
			{
				int num3 = num / 10;
				int kittype = num - 10 * num3;
				if (radioKitKit.Checked)
				{
					text3 = m_TempFolder + "\\" + Kit.KitTextureFileName(num3, kittype, 0);
					ShowRx3File(text3);
				}
				else if (radioKitMinikit.Checked)
				{
					text4 = m_TempFolder + "\\" + Kit.MiniKitDdsFileName(num3, kittype, 0);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowDdsFile(text4);
				}
				break;
			}
			case "NumberFont":
			{
				int num4 = num / 20;
				int colorId = num - 20 * num4;
				text3 = m_TempFolder + "\\" + NumberFont.NumberFontFileName(num4, colorId);
				ShowRx3File(text3);
				break;
			}
			case "Net":
				text3 = m_TempFolder + "\\" + Net.NetFileName(num);
				ShowRx3File(text3);
				break;
			case "MowingPattern":
				text3 = m_TempFolder + "\\" + MowingPattern.MowingPatternFileName(num);
				ShowRx3File(text3);
				break;
			case "Adboard":
				text3 = m_TempFolder + "\\" + Adboard.AdboardFileName(num);
				ShowRx3File(text3);
				break;
			case "Shoes":
				text3 = m_TempFolder + "\\" + Shoes.ShoesTexturesFileName(num, 0);
				ShowRx3File(text3);
				break;
			case "Ball":
				if (radioBallTexture.Checked)
				{
					text3 = m_TempFolder + "\\" + Ball.BallTextureFileName(num);
					ShowRx3File(text3);
				}
				else if (radioBallPreview.Checked && FifaEnvironment.Year == 14)
				{
					text2 = m_TempFolder + "\\" + Ball.BallPictureBigFileName(num);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Center;
					ShowBigFile(text2);
				}
				break;
			case "Stadium":
			{
				int num2 = 0;
				if (radioStadiumGuiOvercast.Checked)
				{
					num2 = 0;
				}
				if (radioStadiumGuiClearDay.Checked)
				{
					num2 = 1;
				}
				if (radioStadiumGuiNight.Checked)
				{
					num2 = 3;
				}
				if (radioStadiumGuiSunset.Checked)
				{
					num2 = 4;
				}
				if (radioStadium3D.Checked)
				{
					if (num2 == 1 || num2 == 3)
					{
						text3 = m_TempFolder + "\\" + Stadium.TexturesFileName(num, num2);
						ShowRx3File(text3);
					}
				}
				else if (radioStadiumPreview.Checked)
				{
					text2 = m_TempFolder + "\\" + Stadium.PreviewBigFileName(num, num2);
					viewer2D.picture.BackgroundImageLayout = ImageLayout.Zoom;
					ShowBigFile(text2);
				}
				break;
			}
			case "GkGloves":
				text3 = m_TempFolder + "\\" + GkGloves.GkGlovesTextureFileName(num);
				ShowRx3File(text3);
				break;
			default:
				tabPreview.Visible = false;
				break;
			case "Sponsor":
				break;
			}
		}
	}

	private void ShowBigFile(string bigFileName)
	{
		tabPreview.SelectedIndex = 0;
		viewer2D.CurrentBitmap = FifaEnvironment.GetBitmapFromBigFile(bigFileName);
		tabPreview.Visible = viewer2D.CurrentBitmap != null;
	}

	private void ShowDdsFile(string ddsFileName)
	{
		tabPreview.SelectedIndex = 0;
		viewer2D.CurrentBitmap = FifaEnvironment.GetBitmapFromDdsFile(ddsFileName);
		tabPreview.Visible = viewer2D.CurrentBitmap != null;
	}

	private void ShowRx3File(string rx3FileName)
	{
		tabPreview.SelectedIndex = 1;
		multiViewer2D.Bitmaps = FifaEnvironment.GetBitmapsFromRx3File(rx3FileName);
		tabPreview.Visible = multiViewer2D.Bitmaps != null;
	}

	private void PatchLoaderForm_Load(object sender, EventArgs e)
	{
		InitPatchLoaderForm();
	}

	private void buttonImportPatch_Click(object sender, EventArgs e)
	{
		ImportPatch();
	}

	private void importToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ImportPatch();
	}

	private void ImportPatch()
	{
		RemoveAllUnusedObjects();
		PatchedObject.Initialize();
		PatchedObject.SetLanguageDataSet(m_LangDataSet);
		if (!PatchedObject.SetFifaDataSet(m_FifaDataSet))
		{
			return;
		}
		PatchedObject.s_PlayerCrossReferenceRequired = false;
		PatchedObject.s_TeamCrossReferenceRequired = false;
		PatchedObject.s_CountryCrossReferenceRequired = false;
		PatchedObject.s_ShoesCrossReferenceRequired = false;
		PatchedObject.s_AdboardCrossReferenceRequired = false;
		PatchedObject.s_BallCrossReferenceRequired = false;
		foreach (ListViewItem item in listViewPatch.Items)
		{
			PatchedObject patchedObject = (PatchedObject)item.Tag;
			if (item.Checked && patchedObject.ImportId != patchedObject.Id)
			{
				if (patchedObject.Type == "Player")
				{
					PatchedObject.s_PlayerCrossReferenceRequired = true;
				}
				else if (patchedObject.Type == "Team")
				{
					PatchedObject.s_TeamCrossReferenceRequired = true;
				}
				else if (patchedObject.Type == "Shoes")
				{
					PatchedObject.s_ShoesCrossReferenceRequired = true;
				}
				else if (patchedObject.Type == "Country")
				{
					PatchedObject.s_CountryCrossReferenceRequired = true;
				}
				else if (patchedObject.Type == "Ball")
				{
					PatchedObject.s_BallCrossReferenceRequired = true;
				}
				else if (patchedObject.Type == "Adboard")
				{
					PatchedObject.s_AdboardCrossReferenceRequired = true;
				}
			}
		}
		foreach (ListViewItem item2 in listViewPatch.Items)
		{
			PatchedObject patchedObject2 = (PatchedObject)item2.Tag;
			if (item2.Checked)
			{
				statusLabel.Text = "Importing " + patchedObject2.Name;
				statusBar.Refresh();
				patchedObject2.Import();
			}
		}
		if (m_FifaDataSet.DataSetName == "FIFA15" || m_FifaDataSet.DataSetName == "FIFA14")
		{
			foreach (ListViewItem item3 in listViewPatch.Items)
			{
				PatchedObject patchedObject3 = (PatchedObject)item3.Tag;
				if (item3.Checked && patchedObject3.Type == "Team")
				{
					Team team = null;
					switch (patchedObject3.UsedObject)
					{
					case PatchedObject.EUsedObject.UseCms:
						team = (Team)patchedObject3.CmsObject;
						break;
					case PatchedObject.EUsedObject.UseNew:
						team = (Team)patchedObject3.NewObject;
						break;
					case PatchedObject.EUsedObject.UseFitting:
						team = (Team)patchedObject3.ReplacedObject;
						break;
					}
					if (team != null && team.Formation != null && team.Formation.IsGeneric())
					{
						Formation formation = (Formation)FifaEnvironment.Formations.CloneId(team.Formation.Id);
						formation.Team = team;
						team.Formation = formation;
					}
				}
			}
			foreach (ListViewItem item4 in listViewPatch.Items)
			{
				PatchedObject patchedObject4 = (PatchedObject)item4.Tag;
				if (item4.Checked && patchedObject4.Type == "Formation")
				{
					Formation formation2 = null;
					switch (patchedObject4.UsedObject)
					{
					case PatchedObject.EUsedObject.UseCms:
						formation2 = (Formation)patchedObject4.CmsObject;
						break;
					case PatchedObject.EUsedObject.UseNew:
						formation2 = (Formation)patchedObject4.NewObject;
						break;
					}
					if (formation2 != null && formation2.IsGeneric())
					{
						FifaEnvironment.Formations.DeleteFormation(formation2);
					}
				}
			}
		}
		FifaEnvironment.UserMessages.ShowMessage(15005);
		Close();
	}

	private void buttonExitCreator_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void exitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		RemoveAllNewObjects();
		Close();
	}

	private void comboReplace_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listViewPatch.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			PatchedObject patchedObject = (PatchedObject)listViewItem.Tag;
			ComboBox comboBox = (ComboBox)sender;
			patchedObject.ReplacedObject = comboBox.SelectedItem;
			listViewItem.SubItems[3] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
		}
	}

	private void buttonSelectNewObjects_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			item.Checked = item.ForeColor == Color.Green;
		}
	}

	private void PatchLoaderForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		RemoveNewObjectsNotImported();
	}

	private void RemoveNewObjectsNotImported()
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			((PatchedObject)item.Tag).RemoveNewObjectIfNotImported();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.PatchLoaderForm));
		this.mainMenu = new System.Windows.Forms.MenuStrip();
		this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolMain = new System.Windows.Forms.ToolStrip();
		this.buttonLoadPatch = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImportPatch = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonExitCreator = new System.Windows.Forms.ToolStripButton();
		this.buttonSelectAllObjects = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonDeselectAllObjects = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonSelectNewObjects = new System.Windows.Forms.ToolStripButton();
		this.stripButtonPreview = new System.Windows.Forms.ToolStripButton();
		this.panelLeft = new System.Windows.Forms.Panel();
		this.textDescription = new System.Windows.Forms.TextBox();
		this.groupPatchOptions = new System.Windows.Forms.GroupBox();
		this.tabPatchOptions = new System.Windows.Forms.TabControl();
		this.pagePlayerOptions = new System.Windows.Forms.TabPage();
		this.groupDualClub = new System.Windows.Forms.GroupBox();
		this.radioPutInBothTeams = new System.Windows.Forms.RadioButton();
		this.radioTransferToNewTeam = new System.Windows.Forms.RadioButton();
		this.radioLeaveInExistingTeam = new System.Windows.Forms.RadioButton();
		this.checkPlayerMiniface = new System.Windows.Forms.CheckBox();
		this.checkPlayerHead = new System.Windows.Forms.CheckBox();
		this.checkPlayerDatabase = new System.Windows.Forms.CheckBox();
		this.pageTeamOptions = new System.Windows.Forms.TabPage();
		this.checkTeamFlags = new System.Windows.Forms.CheckBox();
		this.checkTeamBanner = new System.Windows.Forms.CheckBox();
		this.checkTeamLogo = new System.Windows.Forms.CheckBox();
		this.checkTeamDatabase = new System.Windows.Forms.CheckBox();
		this.pageLeagueOptions = new System.Windows.Forms.TabPage();
		this.checkLeagueLogo = new System.Windows.Forms.CheckBox();
		this.checkLeagueDatabase = new System.Windows.Forms.CheckBox();
		this.pageStadiumOptions = new System.Windows.Forms.TabPage();
		this.checkStadiumModel = new System.Windows.Forms.CheckBox();
		this.checkStadiumPreview = new System.Windows.Forms.CheckBox();
		this.checkStadiumDatabase = new System.Windows.Forms.CheckBox();
		this.pageKitOptions = new System.Windows.Forms.TabPage();
		this.checkMinikits = new System.Windows.Forms.CheckBox();
		this.checkKits = new System.Windows.Forms.CheckBox();
		this.checkKitDatabase = new System.Windows.Forms.CheckBox();
		this.pageCountryOptions = new System.Windows.Forms.TabPage();
		this.checkCountryMap = new System.Windows.Forms.CheckBox();
		this.checkCountryDatabase = new System.Windows.Forms.CheckBox();
		this.checkCountryFlag = new System.Windows.Forms.CheckBox();
		this.textPatchVersion = new System.Windows.Forms.TextBox();
		this.textPatchName = new System.Windows.Forms.TextBox();
		this.labelDescription = new System.Windows.Forms.Label();
		this.labelPatchVersion = new System.Windows.Forms.Label();
		this.labelPatchName = new System.Windows.Forms.Label();
		this.statusBar = new System.Windows.Forms.StatusStrip();
		this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.listViewPatch = new System.Windows.Forms.ListView();
		this.columnItem = new System.Windows.Forms.ColumnHeader();
		this.columnType = new System.Windows.Forms.ColumnHeader();
		this.columnPatchId = new System.Windows.Forms.ColumnHeader();
		this.columnImportId = new System.Windows.Forms.ColumnHeader();
		this.columnComment = new System.Windows.Forms.ColumnHeader();
		this.panelRight = new System.Windows.Forms.Panel();
		this.tabPreview = new System.Windows.Forms.TabControl();
		this.pageViewer2D = new System.Windows.Forms.TabPage();
		this.viewer2D = new FifaControls.Viewer2D();
		this.pageMultiViewer2D = new System.Windows.Forms.TabPage();
		this.multiViewer2D = new FifaControls.MultiViewer2D();
		this.panelGraphicGroups = new System.Windows.Forms.Panel();
		this.groupTeam = new System.Windows.Forms.GroupBox();
		this.radioTeamCrest50 = new System.Windows.Forms.RadioButton();
		this.radioTeamCrest16 = new System.Windows.Forms.RadioButton();
		this.radioTeamCrest32 = new System.Windows.Forms.RadioButton();
		this.radioTeamCrestLarge = new System.Windows.Forms.RadioButton();
		this.radioTeamFlags = new System.Windows.Forms.RadioButton();
		this.radioTeamBanners = new System.Windows.Forms.RadioButton();
		this.groupLeague = new System.Windows.Forms.GroupBox();
		this.radioLeagueLogo512x128 = new System.Windows.Forms.RadioButton();
		this.radioLeagueAnimLogo = new System.Windows.Forms.RadioButton();
		this.radioLeagueTinyLogo = new System.Windows.Forms.RadioButton();
		this.radioLeagueSmallLogo = new System.Windows.Forms.RadioButton();
		this.groupStadium = new System.Windows.Forms.GroupBox();
		this.radioStadiumPreview = new System.Windows.Forms.RadioButton();
		this.groupTod = new System.Windows.Forms.GroupBox();
		this.radioStadiumGuiNight = new System.Windows.Forms.RadioButton();
		this.radioStadiumGuiSunset = new System.Windows.Forms.RadioButton();
		this.radioStadiumGuiOvercast = new System.Windows.Forms.RadioButton();
		this.radioStadiumGuiClearDay = new System.Windows.Forms.RadioButton();
		this.radioStadium3D = new System.Windows.Forms.RadioButton();
		this.groupShoes = new System.Windows.Forms.GroupBox();
		this.radioShoesColor = new System.Windows.Forms.RadioButton();
		this.groupBall = new System.Windows.Forms.GroupBox();
		this.radioBallPreview = new System.Windows.Forms.RadioButton();
		this.radioBallTexture = new System.Windows.Forms.RadioButton();
		this.groupCountry = new System.Windows.Forms.GroupBox();
		this.radioCountryMap = new System.Windows.Forms.RadioButton();
		this.radioCountryFlag512x512 = new System.Windows.Forms.RadioButton();
		this.radioCountryCard = new System.Windows.Forms.RadioButton();
		this.radioCountryMainFlag = new System.Windows.Forms.RadioButton();
		this.radioCountryMiniflag = new System.Windows.Forms.RadioButton();
		this.groupAdboards = new System.Windows.Forms.GroupBox();
		this.radioAdboard1 = new System.Windows.Forms.RadioButton();
		this.groupKit = new System.Windows.Forms.GroupBox();
		this.radioKitKit = new System.Windows.Forms.RadioButton();
		this.radioKitMinikit = new System.Windows.Forms.RadioButton();
		this.groupPlayer = new System.Windows.Forms.GroupBox();
		this.radioHairTextures = new System.Windows.Forms.RadioButton();
		this.radioHairColorTexture = new System.Windows.Forms.RadioButton();
		this.radioEyesTexture = new System.Windows.Forms.RadioButton();
		this.radioFaceTexture = new System.Windows.Forms.RadioButton();
		this.radioMiniHead = new System.Windows.Forms.RadioButton();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.groupReplaceSelection = new System.Windows.Forms.GroupBox();
		this.comboReplaceLicensedTournament = new System.Windows.Forms.ComboBox();
		this.comboReplaceKit = new System.Windows.Forms.ComboBox();
		this.labelCmsCreated = new System.Windows.Forms.Label();
		this.labelCmsReplaced = new System.Windows.Forms.Label();
		this.textCmsReplaced = new System.Windows.Forms.TextBox();
		this.comboReplaceMowingPattern = new System.Windows.Forms.ComboBox();
		this.radioCmsItem = new System.Windows.Forms.RadioButton();
		this.comboReplaceGkGloves = new System.Windows.Forms.ComboBox();
		this.comboReplaceNet = new System.Windows.Forms.ComboBox();
		this.comboReplaceShoes = new System.Windows.Forms.ComboBox();
		this.comboReplaceNamesFont = new System.Windows.Forms.ComboBox();
		this.comboReplaceNumberFont = new System.Windows.Forms.ComboBox();
		this.comboReplaceAdboard = new System.Windows.Forms.ComboBox();
		this.comboReplaceBall = new System.Windows.Forms.ComboBox();
		this.comboReplaceReferee = new System.Windows.Forms.ComboBox();
		this.comboReplaceSponsor = new System.Windows.Forms.ComboBox();
		this.comboReplaceFormation = new System.Windows.Forms.ComboBox();
		this.comboReplaceTournament = new System.Windows.Forms.ComboBox();
		this.comboReplaceStadium = new System.Windows.Forms.ComboBox();
		this.comboReplaceCountry = new System.Windows.Forms.ComboBox();
		this.comboReplaceLeague = new System.Windows.Forms.ComboBox();
		this.comboReplacePlayer = new System.Windows.Forms.ComboBox();
		this.comboReplaceTeam = new System.Windows.Forms.ComboBox();
		this.radioReplaceItem = new System.Windows.Forms.RadioButton();
		this.radioCreateItem = new System.Windows.Forms.RadioButton();
		this.labelDetails = new System.Windows.Forms.Label();
		this.mainMenu.SuspendLayout();
		this.toolMain.SuspendLayout();
		this.panelLeft.SuspendLayout();
		this.groupPatchOptions.SuspendLayout();
		this.tabPatchOptions.SuspendLayout();
		this.pagePlayerOptions.SuspendLayout();
		this.groupDualClub.SuspendLayout();
		this.pageTeamOptions.SuspendLayout();
		this.pageLeagueOptions.SuspendLayout();
		this.pageStadiumOptions.SuspendLayout();
		this.pageKitOptions.SuspendLayout();
		this.pageCountryOptions.SuspendLayout();
		this.statusBar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		this.panelRight.SuspendLayout();
		this.tabPreview.SuspendLayout();
		this.pageViewer2D.SuspendLayout();
		this.pageMultiViewer2D.SuspendLayout();
		this.panelGraphicGroups.SuspendLayout();
		this.groupTeam.SuspendLayout();
		this.groupLeague.SuspendLayout();
		this.groupStadium.SuspendLayout();
		this.groupTod.SuspendLayout();
		this.groupShoes.SuspendLayout();
		this.groupBall.SuspendLayout();
		this.groupCountry.SuspendLayout();
		this.groupAdboards.SuspendLayout();
		this.groupKit.SuspendLayout();
		this.groupPlayer.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		this.groupReplaceSelection.SuspendLayout();
		base.SuspendLayout();
		this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.patchToolStripMenuItem });
		this.mainMenu.Location = new System.Drawing.Point(0, 0);
		this.mainMenu.Name = "mainMenu";
		this.mainMenu.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
		this.mainMenu.Size = new System.Drawing.Size(1543, 28);
		this.mainMenu.TabIndex = 0;
		this.mainMenu.Text = "menuStrip1";
		this.patchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.openToolStripMenuItem, this.importToolStripMenuItem, this.exitToolStripMenuItem });
		this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
		this.patchToolStripMenuItem.Size = new System.Drawing.Size(57, 24);
		this.patchToolStripMenuItem.Text = "Patch";
		this.openToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openToolStripMenuItem.Image");
		this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.openToolStripMenuItem.Name = "openToolStripMenuItem";
		this.openToolStripMenuItem.Size = new System.Drawing.Size(123, 24);
		this.openToolStripMenuItem.Text = "Open";
		this.openToolStripMenuItem.Click += new System.EventHandler(openToolStripMenuItem_Click);
		this.importToolStripMenuItem.Enabled = false;
		this.importToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("importToolStripMenuItem.Image");
		this.importToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.importToolStripMenuItem.Name = "importToolStripMenuItem";
		this.importToolStripMenuItem.Size = new System.Drawing.Size(123, 24);
		this.importToolStripMenuItem.Text = "Import";
		this.importToolStripMenuItem.Click += new System.EventHandler(importToolStripMenuItem_Click);
		this.exitToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("exitToolStripMenuItem.Image");
		this.exitToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
		this.exitToolStripMenuItem.Size = new System.Drawing.Size(123, 24);
		this.exitToolStripMenuItem.Text = "Exit";
		this.exitToolStripMenuItem.Click += new System.EventHandler(exitToolStripMenuItem_Click);
		this.toolMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[11]
		{
			this.buttonLoadPatch, this.toolStripSeparator1, this.buttonImportPatch, this.toolStripSeparator2, this.buttonExitCreator, this.buttonSelectAllObjects, this.toolStripSeparator3, this.buttonDeselectAllObjects, this.toolStripSeparator4, this.buttonSelectNewObjects,
			this.stripButtonPreview
		});
		this.toolMain.Location = new System.Drawing.Point(0, 28);
		this.toolMain.Name = "toolMain";
		this.toolMain.Size = new System.Drawing.Size(1543, 27);
		this.toolMain.TabIndex = 1;
		this.toolMain.Text = "toolStrip1";
		this.buttonLoadPatch.Image = (System.Drawing.Image)resources.GetObject("buttonLoadPatch.Image");
		this.buttonLoadPatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonLoadPatch.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
		this.buttonLoadPatch.Name = "buttonLoadPatch";
		this.buttonLoadPatch.Size = new System.Drawing.Size(73, 24);
		this.buttonLoadPatch.Text = "Open  ";
		this.buttonLoadPatch.Click += new System.EventHandler(buttonLoadPatch_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
		this.buttonImportPatch.Enabled = false;
		this.buttonImportPatch.Image = (System.Drawing.Image)resources.GetObject("buttonImportPatch.Image");
		this.buttonImportPatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportPatch.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
		this.buttonImportPatch.Name = "buttonImportPatch";
		this.buttonImportPatch.Size = new System.Drawing.Size(74, 24);
		this.buttonImportPatch.Text = "Import";
		this.buttonImportPatch.Click += new System.EventHandler(buttonImportPatch_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
		this.buttonExitCreator.Image = (System.Drawing.Image)resources.GetObject("buttonExitCreator.Image");
		this.buttonExitCreator.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExitCreator.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
		this.buttonExitCreator.Name = "buttonExitCreator";
		this.buttonExitCreator.Size = new System.Drawing.Size(53, 24);
		this.buttonExitCreator.Text = "Exit";
		this.buttonExitCreator.Click += new System.EventHandler(buttonExitCreator_Click);
		this.buttonSelectAllObjects.Image = (System.Drawing.Image)resources.GetObject("buttonSelectAllObjects.Image");
		this.buttonSelectAllObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSelectAllObjects.Margin = new System.Windows.Forms.Padding(220, 1, 0, 2);
		this.buttonSelectAllObjects.Name = "buttonSelectAllObjects";
		this.buttonSelectAllObjects.Size = new System.Drawing.Size(91, 24);
		this.buttonSelectAllObjects.Text = "Select All";
		this.buttonSelectAllObjects.Click += new System.EventHandler(buttonSelectAll_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
		this.buttonDeselectAllObjects.Image = (System.Drawing.Image)resources.GetObject("buttonDeselectAllObjects.Image");
		this.buttonDeselectAllObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeselectAllObjects.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
		this.buttonDeselectAllObjects.Name = "buttonDeselectAllObjects";
		this.buttonDeselectAllObjects.Size = new System.Drawing.Size(108, 24);
		this.buttonDeselectAllObjects.Text = "Deselect All";
		this.buttonDeselectAllObjects.Click += new System.EventHandler(buttonDeselectAll_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
		this.buttonSelectNewObjects.Image = (System.Drawing.Image)resources.GetObject("buttonSelectNewObjects.Image");
		this.buttonSelectNewObjects.ImageTransparentColor = System.Drawing.Color.Transparent;
		this.buttonSelectNewObjects.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
		this.buttonSelectNewObjects.Name = "buttonSelectNewObjects";
		this.buttonSelectNewObjects.Size = new System.Drawing.Size(113, 24);
		this.buttonSelectNewObjects.Text = "Select if new";
		this.buttonSelectNewObjects.Click += new System.EventHandler(buttonSelectNewObjects_Click);
		this.stripButtonPreview.CheckOnClick = true;
		this.stripButtonPreview.Image = (System.Drawing.Image)resources.GetObject("stripButtonPreview.Image");
		this.stripButtonPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.stripButtonPreview.Margin = new System.Windows.Forms.Padding(310, 1, 0, 2);
		this.stripButtonPreview.Name = "stripButtonPreview";
		this.stripButtonPreview.Size = new System.Drawing.Size(80, 24);
		this.stripButtonPreview.Text = "Preview";
		this.stripButtonPreview.Click += new System.EventHandler(stripButtonPreview_Click);
		this.panelLeft.AutoScroll = true;
		this.panelLeft.Controls.Add(this.textDescription);
		this.panelLeft.Controls.Add(this.groupPatchOptions);
		this.panelLeft.Controls.Add(this.textPatchVersion);
		this.panelLeft.Controls.Add(this.textPatchName);
		this.panelLeft.Controls.Add(this.labelDescription);
		this.panelLeft.Controls.Add(this.labelPatchVersion);
		this.panelLeft.Controls.Add(this.labelPatchName);
		this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
		this.panelLeft.Location = new System.Drawing.Point(0, 55);
		this.panelLeft.Margin = new System.Windows.Forms.Padding(4);
		this.panelLeft.Name = "panelLeft";
		this.panelLeft.Size = new System.Drawing.Size(400, 838);
		this.panelLeft.TabIndex = 2;
		this.textDescription.BackColor = System.Drawing.Color.White;
		this.textDescription.Enabled = false;
		this.textDescription.Location = new System.Drawing.Point(11, 96);
		this.textDescription.Margin = new System.Windows.Forms.Padding(4);
		this.textDescription.Multiline = true;
		this.textDescription.Name = "textDescription";
		this.textDescription.Size = new System.Drawing.Size(373, 245);
		this.textDescription.TabIndex = 33;
		this.groupPatchOptions.Controls.Add(this.tabPatchOptions);
		this.groupPatchOptions.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupPatchOptions.Location = new System.Drawing.Point(0, 347);
		this.groupPatchOptions.Margin = new System.Windows.Forms.Padding(4);
		this.groupPatchOptions.Name = "groupPatchOptions";
		this.groupPatchOptions.Padding = new System.Windows.Forms.Padding(4);
		this.groupPatchOptions.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.groupPatchOptions.Size = new System.Drawing.Size(400, 491);
		this.groupPatchOptions.TabIndex = 34;
		this.groupPatchOptions.TabStop = false;
		this.groupPatchOptions.Text = "Import Options";
		this.tabPatchOptions.Controls.Add(this.pagePlayerOptions);
		this.tabPatchOptions.Controls.Add(this.pageTeamOptions);
		this.tabPatchOptions.Controls.Add(this.pageLeagueOptions);
		this.tabPatchOptions.Controls.Add(this.pageStadiumOptions);
		this.tabPatchOptions.Controls.Add(this.pageKitOptions);
		this.tabPatchOptions.Controls.Add(this.pageCountryOptions);
		this.tabPatchOptions.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabPatchOptions.ItemSize = new System.Drawing.Size(80, 20);
		this.tabPatchOptions.Location = new System.Drawing.Point(4, 19);
		this.tabPatchOptions.Margin = new System.Windows.Forms.Padding(4);
		this.tabPatchOptions.Multiline = true;
		this.tabPatchOptions.Name = "tabPatchOptions";
		this.tabPatchOptions.SelectedIndex = 0;
		this.tabPatchOptions.Size = new System.Drawing.Size(392, 468);
		this.tabPatchOptions.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
		this.tabPatchOptions.TabIndex = 8;
		this.pagePlayerOptions.Controls.Add(this.groupDualClub);
		this.pagePlayerOptions.Controls.Add(this.checkPlayerMiniface);
		this.pagePlayerOptions.Controls.Add(this.checkPlayerHead);
		this.pagePlayerOptions.Controls.Add(this.checkPlayerDatabase);
		this.pagePlayerOptions.Location = new System.Drawing.Point(4, 24);
		this.pagePlayerOptions.Margin = new System.Windows.Forms.Padding(4);
		this.pagePlayerOptions.Name = "pagePlayerOptions";
		this.pagePlayerOptions.Padding = new System.Windows.Forms.Padding(4);
		this.pagePlayerOptions.Size = new System.Drawing.Size(384, 440);
		this.pagePlayerOptions.TabIndex = 0;
		this.pagePlayerOptions.Text = "Players";
		this.pagePlayerOptions.UseVisualStyleBackColor = true;
		this.groupDualClub.Controls.Add(this.radioPutInBothTeams);
		this.groupDualClub.Controls.Add(this.radioTransferToNewTeam);
		this.groupDualClub.Controls.Add(this.radioLeaveInExistingTeam);
		this.groupDualClub.Location = new System.Drawing.Point(27, 124);
		this.groupDualClub.Margin = new System.Windows.Forms.Padding(4);
		this.groupDualClub.Name = "groupDualClub";
		this.groupDualClub.Padding = new System.Windows.Forms.Padding(4);
		this.groupDualClub.Size = new System.Drawing.Size(319, 123);
		this.groupDualClub.TabIndex = 3;
		this.groupDualClub.TabStop = false;
		this.groupDualClub.Text = "Double Club Option";
		this.radioPutInBothTeams.AutoSize = true;
		this.radioPutInBothTeams.Checked = true;
		this.radioPutInBothTeams.Location = new System.Drawing.Point(11, 80);
		this.radioPutInBothTeams.Margin = new System.Windows.Forms.Padding(4);
		this.radioPutInBothTeams.Name = "radioPutInBothTeams";
		this.radioPutInBothTeams.Size = new System.Drawing.Size(139, 21);
		this.radioPutInBothTeams.TabIndex = 6;
		this.radioPutInBothTeams.TabStop = true;
		this.radioPutInBothTeams.Text = "Put in both teams";
		this.radioPutInBothTeams.UseVisualStyleBackColor = true;
		this.radioTransferToNewTeam.AutoSize = true;
		this.radioTransferToNewTeam.Location = new System.Drawing.Point(11, 52);
		this.radioTransferToNewTeam.Margin = new System.Windows.Forms.Padding(4);
		this.radioTransferToNewTeam.Name = "radioTransferToNewTeam";
		this.radioTransferToNewTeam.Size = new System.Drawing.Size(163, 21);
		this.radioTransferToNewTeam.TabIndex = 5;
		this.radioTransferToNewTeam.Text = "Transfer to new team";
		this.radioTransferToNewTeam.UseVisualStyleBackColor = true;
		this.radioLeaveInExistingTeam.AutoSize = true;
		this.radioLeaveInExistingTeam.Location = new System.Drawing.Point(11, 23);
		this.radioLeaveInExistingTeam.Margin = new System.Windows.Forms.Padding(4);
		this.radioLeaveInExistingTeam.Name = "radioLeaveInExistingTeam";
		this.radioLeaveInExistingTeam.Size = new System.Drawing.Size(167, 21);
		this.radioLeaveInExistingTeam.TabIndex = 4;
		this.radioLeaveInExistingTeam.Text = "Leave in current team";
		this.radioLeaveInExistingTeam.UseVisualStyleBackColor = true;
		this.checkPlayerMiniface.AutoSize = true;
		this.checkPlayerMiniface.Checked = true;
		this.checkPlayerMiniface.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerMiniface.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerMiniface.Location = new System.Drawing.Point(27, 81);
		this.checkPlayerMiniface.Margin = new System.Windows.Forms.Padding(4);
		this.checkPlayerMiniface.Name = "checkPlayerMiniface";
		this.checkPlayerMiniface.Size = new System.Drawing.Size(82, 21);
		this.checkPlayerMiniface.TabIndex = 2;
		this.checkPlayerMiniface.Text = "Miniface";
		this.checkPlayerMiniface.UseVisualStyleBackColor = true;
		this.checkPlayerHead.AutoSize = true;
		this.checkPlayerHead.Checked = true;
		this.checkPlayerHead.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerHead.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerHead.Location = new System.Drawing.Point(27, 53);
		this.checkPlayerHead.Margin = new System.Windows.Forms.Padding(4);
		this.checkPlayerHead.Name = "checkPlayerHead";
		this.checkPlayerHead.Size = new System.Drawing.Size(117, 21);
		this.checkPlayerHead.TabIndex = 1;
		this.checkPlayerHead.Text = "Specific Head";
		this.checkPlayerHead.UseVisualStyleBackColor = true;
		this.checkPlayerDatabase.AutoSize = true;
		this.checkPlayerDatabase.Checked = true;
		this.checkPlayerDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkPlayerDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkPlayerDatabase.Location = new System.Drawing.Point(27, 25);
		this.checkPlayerDatabase.Margin = new System.Windows.Forms.Padding(4);
		this.checkPlayerDatabase.Name = "checkPlayerDatabase";
		this.checkPlayerDatabase.Size = new System.Drawing.Size(161, 21);
		this.checkPlayerDatabase.TabIndex = 0;
		this.checkPlayerDatabase.Text = "Database player info";
		this.checkPlayerDatabase.UseVisualStyleBackColor = true;
		this.pageTeamOptions.Controls.Add(this.checkTeamFlags);
		this.pageTeamOptions.Controls.Add(this.checkTeamBanner);
		this.pageTeamOptions.Controls.Add(this.checkTeamLogo);
		this.pageTeamOptions.Controls.Add(this.checkTeamDatabase);
		this.pageTeamOptions.Location = new System.Drawing.Point(4, 24);
		this.pageTeamOptions.Margin = new System.Windows.Forms.Padding(4);
		this.pageTeamOptions.Name = "pageTeamOptions";
		this.pageTeamOptions.Padding = new System.Windows.Forms.Padding(4);
		this.pageTeamOptions.Size = new System.Drawing.Size(384, 440);
		this.pageTeamOptions.TabIndex = 1;
		this.pageTeamOptions.Text = "Teams";
		this.pageTeamOptions.UseVisualStyleBackColor = true;
		this.checkTeamFlags.AutoSize = true;
		this.checkTeamFlags.Checked = true;
		this.checkTeamFlags.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamFlags.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamFlags.Location = new System.Drawing.Point(27, 110);
		this.checkTeamFlags.Margin = new System.Windows.Forms.Padding(4);
		this.checkTeamFlags.Name = "checkTeamFlags";
		this.checkTeamFlags.Size = new System.Drawing.Size(64, 21);
		this.checkTeamFlags.TabIndex = 5;
		this.checkTeamFlags.Text = "Flags";
		this.checkTeamFlags.UseVisualStyleBackColor = true;
		this.checkTeamBanner.AutoSize = true;
		this.checkTeamBanner.Checked = true;
		this.checkTeamBanner.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamBanner.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamBanner.Location = new System.Drawing.Point(27, 81);
		this.checkTeamBanner.Margin = new System.Windows.Forms.Padding(4);
		this.checkTeamBanner.Name = "checkTeamBanner";
		this.checkTeamBanner.Size = new System.Drawing.Size(83, 21);
		this.checkTeamBanner.TabIndex = 3;
		this.checkTeamBanner.Text = "Banners";
		this.checkTeamBanner.UseVisualStyleBackColor = true;
		this.checkTeamLogo.AutoSize = true;
		this.checkTeamLogo.Checked = true;
		this.checkTeamLogo.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamLogo.Location = new System.Drawing.Point(27, 53);
		this.checkTeamLogo.Margin = new System.Windows.Forms.Padding(4);
		this.checkTeamLogo.Name = "checkTeamLogo";
		this.checkTeamLogo.Size = new System.Drawing.Size(62, 21);
		this.checkTeamLogo.TabIndex = 2;
		this.checkTeamLogo.Text = "Logo";
		this.checkTeamLogo.UseVisualStyleBackColor = true;
		this.checkTeamDatabase.AutoSize = true;
		this.checkTeamDatabase.Checked = true;
		this.checkTeamDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkTeamDatabase.Location = new System.Drawing.Point(27, 25);
		this.checkTeamDatabase.Margin = new System.Windows.Forms.Padding(4);
		this.checkTeamDatabase.Name = "checkTeamDatabase";
		this.checkTeamDatabase.Size = new System.Drawing.Size(153, 21);
		this.checkTeamDatabase.TabIndex = 1;
		this.checkTeamDatabase.Text = "Database team info";
		this.checkTeamDatabase.UseVisualStyleBackColor = true;
		this.pageLeagueOptions.Controls.Add(this.checkLeagueLogo);
		this.pageLeagueOptions.Controls.Add(this.checkLeagueDatabase);
		this.pageLeagueOptions.Location = new System.Drawing.Point(4, 24);
		this.pageLeagueOptions.Margin = new System.Windows.Forms.Padding(4);
		this.pageLeagueOptions.Name = "pageLeagueOptions";
		this.pageLeagueOptions.Size = new System.Drawing.Size(384, 440);
		this.pageLeagueOptions.TabIndex = 2;
		this.pageLeagueOptions.Text = "Leagues";
		this.pageLeagueOptions.UseVisualStyleBackColor = true;
		this.checkLeagueLogo.AutoSize = true;
		this.checkLeagueLogo.Checked = true;
		this.checkLeagueLogo.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkLeagueLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueLogo.Location = new System.Drawing.Point(27, 53);
		this.checkLeagueLogo.Margin = new System.Windows.Forms.Padding(4);
		this.checkLeagueLogo.Name = "checkLeagueLogo";
		this.checkLeagueLogo.Size = new System.Drawing.Size(62, 21);
		this.checkLeagueLogo.TabIndex = 10;
		this.checkLeagueLogo.Text = "Logo";
		this.checkLeagueLogo.UseVisualStyleBackColor = true;
		this.checkLeagueDatabase.AutoSize = true;
		this.checkLeagueDatabase.Checked = true;
		this.checkLeagueDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkLeagueDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkLeagueDatabase.Location = new System.Drawing.Point(27, 25);
		this.checkLeagueDatabase.Margin = new System.Windows.Forms.Padding(4);
		this.checkLeagueDatabase.Name = "checkLeagueDatabase";
		this.checkLeagueDatabase.Size = new System.Drawing.Size(165, 21);
		this.checkLeagueDatabase.TabIndex = 9;
		this.checkLeagueDatabase.Text = "Database league info";
		this.checkLeagueDatabase.UseVisualStyleBackColor = true;
		this.pageStadiumOptions.Controls.Add(this.checkStadiumModel);
		this.pageStadiumOptions.Controls.Add(this.checkStadiumPreview);
		this.pageStadiumOptions.Controls.Add(this.checkStadiumDatabase);
		this.pageStadiumOptions.Location = new System.Drawing.Point(4, 24);
		this.pageStadiumOptions.Margin = new System.Windows.Forms.Padding(4);
		this.pageStadiumOptions.Name = "pageStadiumOptions";
		this.pageStadiumOptions.Size = new System.Drawing.Size(384, 440);
		this.pageStadiumOptions.TabIndex = 6;
		this.pageStadiumOptions.Text = "Stadiums";
		this.pageStadiumOptions.ToolTipText = "Check the stadium elements that you want to import (if present)";
		this.pageStadiumOptions.UseVisualStyleBackColor = true;
		this.checkStadiumModel.AutoSize = true;
		this.checkStadiumModel.Checked = true;
		this.checkStadiumModel.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumModel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumModel.Location = new System.Drawing.Point(27, 53);
		this.checkStadiumModel.Margin = new System.Windows.Forms.Padding(4);
		this.checkStadiumModel.Name = "checkStadiumModel";
		this.checkStadiumModel.Size = new System.Drawing.Size(90, 21);
		this.checkStadiumModel.TabIndex = 19;
		this.checkStadiumModel.Text = "3D model";
		this.checkStadiumModel.UseVisualStyleBackColor = true;
		this.checkStadiumPreview.AutoSize = true;
		this.checkStadiumPreview.Checked = true;
		this.checkStadiumPreview.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumPreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumPreview.Location = new System.Drawing.Point(27, 81);
		this.checkStadiumPreview.Margin = new System.Windows.Forms.Padding(4);
		this.checkStadiumPreview.Name = "checkStadiumPreview";
		this.checkStadiumPreview.Size = new System.Drawing.Size(126, 21);
		this.checkStadiumPreview.TabIndex = 17;
		this.checkStadiumPreview.Text = "Preview picture";
		this.checkStadiumPreview.UseVisualStyleBackColor = true;
		this.checkStadiumDatabase.AutoSize = true;
		this.checkStadiumDatabase.Checked = true;
		this.checkStadiumDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkStadiumDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkStadiumDatabase.Location = new System.Drawing.Point(27, 25);
		this.checkStadiumDatabase.Margin = new System.Windows.Forms.Padding(4);
		this.checkStadiumDatabase.Name = "checkStadiumDatabase";
		this.checkStadiumDatabase.Size = new System.Drawing.Size(171, 21);
		this.checkStadiumDatabase.TabIndex = 16;
		this.checkStadiumDatabase.Text = "Database stadium info";
		this.checkStadiumDatabase.UseVisualStyleBackColor = true;
		this.pageKitOptions.Controls.Add(this.checkMinikits);
		this.pageKitOptions.Controls.Add(this.checkKits);
		this.pageKitOptions.Controls.Add(this.checkKitDatabase);
		this.pageKitOptions.Location = new System.Drawing.Point(4, 24);
		this.pageKitOptions.Margin = new System.Windows.Forms.Padding(4);
		this.pageKitOptions.Name = "pageKitOptions";
		this.pageKitOptions.Size = new System.Drawing.Size(384, 440);
		this.pageKitOptions.TabIndex = 5;
		this.pageKitOptions.Text = "Kits";
		this.pageKitOptions.UseVisualStyleBackColor = true;
		this.checkMinikits.AutoSize = true;
		this.checkMinikits.Checked = true;
		this.checkMinikits.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkMinikits.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkMinikits.Location = new System.Drawing.Point(27, 53);
		this.checkMinikits.Margin = new System.Windows.Forms.Padding(4);
		this.checkMinikits.Name = "checkMinikits";
		this.checkMinikits.Size = new System.Drawing.Size(76, 21);
		this.checkMinikits.TabIndex = 9;
		this.checkMinikits.Text = "Minikits";
		this.checkMinikits.UseVisualStyleBackColor = true;
		this.checkKits.AutoSize = true;
		this.checkKits.Checked = true;
		this.checkKits.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkKits.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKits.Location = new System.Drawing.Point(27, 25);
		this.checkKits.Margin = new System.Windows.Forms.Padding(4);
		this.checkKits.Name = "checkKits";
		this.checkKits.Size = new System.Drawing.Size(53, 21);
		this.checkKits.TabIndex = 8;
		this.checkKits.Text = "Kits";
		this.checkKits.UseVisualStyleBackColor = true;
		this.checkKitDatabase.AutoSize = true;
		this.checkKitDatabase.Checked = true;
		this.checkKitDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkKitDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkKitDatabase.Location = new System.Drawing.Point(205, 53);
		this.checkKitDatabase.Margin = new System.Windows.Forms.Padding(4);
		this.checkKitDatabase.Name = "checkKitDatabase";
		this.checkKitDatabase.Size = new System.Drawing.Size(136, 21);
		this.checkKitDatabase.TabIndex = 2;
		this.checkKitDatabase.Text = "Database kit info";
		this.checkKitDatabase.UseVisualStyleBackColor = true;
		this.checkKitDatabase.Visible = false;
		this.pageCountryOptions.Controls.Add(this.checkCountryMap);
		this.pageCountryOptions.Controls.Add(this.checkCountryDatabase);
		this.pageCountryOptions.Controls.Add(this.checkCountryFlag);
		this.pageCountryOptions.Location = new System.Drawing.Point(4, 24);
		this.pageCountryOptions.Margin = new System.Windows.Forms.Padding(4);
		this.pageCountryOptions.Name = "pageCountryOptions";
		this.pageCountryOptions.Size = new System.Drawing.Size(384, 440);
		this.pageCountryOptions.TabIndex = 3;
		this.pageCountryOptions.Text = "Countries";
		this.pageCountryOptions.UseVisualStyleBackColor = true;
		this.checkCountryMap.AutoSize = true;
		this.checkCountryMap.Checked = true;
		this.checkCountryMap.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkCountryMap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryMap.Location = new System.Drawing.Point(27, 81);
		this.checkCountryMap.Margin = new System.Windows.Forms.Padding(4);
		this.checkCountryMap.Name = "checkCountryMap";
		this.checkCountryMap.Size = new System.Drawing.Size(57, 21);
		this.checkCountryMap.TabIndex = 3;
		this.checkCountryMap.Text = "Map";
		this.checkCountryMap.UseVisualStyleBackColor = true;
		this.checkCountryDatabase.AutoSize = true;
		this.checkCountryDatabase.Checked = true;
		this.checkCountryDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkCountryDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryDatabase.Location = new System.Drawing.Point(27, 25);
		this.checkCountryDatabase.Margin = new System.Windows.Forms.Padding(4);
		this.checkCountryDatabase.Name = "checkCountryDatabase";
		this.checkCountryDatabase.Size = new System.Drawing.Size(169, 21);
		this.checkCountryDatabase.TabIndex = 1;
		this.checkCountryDatabase.Text = "Database country info";
		this.checkCountryDatabase.UseVisualStyleBackColor = true;
		this.checkCountryFlag.AutoSize = true;
		this.checkCountryFlag.Checked = true;
		this.checkCountryFlag.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkCountryFlag.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.checkCountryFlag.Location = new System.Drawing.Point(27, 53);
		this.checkCountryFlag.Margin = new System.Windows.Forms.Padding(4);
		this.checkCountryFlag.Name = "checkCountryFlag";
		this.checkCountryFlag.Size = new System.Drawing.Size(64, 21);
		this.checkCountryFlag.TabIndex = 0;
		this.checkCountryFlag.Text = "Flags";
		this.checkCountryFlag.UseVisualStyleBackColor = true;
		this.textPatchVersion.BackColor = System.Drawing.Color.White;
		this.textPatchVersion.Enabled = false;
		this.textPatchVersion.Location = new System.Drawing.Point(101, 41);
		this.textPatchVersion.Margin = new System.Windows.Forms.Padding(4);
		this.textPatchVersion.Name = "textPatchVersion";
		this.textPatchVersion.Size = new System.Drawing.Size(283, 22);
		this.textPatchVersion.TabIndex = 31;
		this.textPatchVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.textPatchName.BackColor = System.Drawing.Color.White;
		this.textPatchName.Enabled = false;
		this.textPatchName.Location = new System.Drawing.Point(103, 6);
		this.textPatchName.Margin = new System.Windows.Forms.Padding(4);
		this.textPatchName.Name = "textPatchName";
		this.textPatchName.Size = new System.Drawing.Size(283, 22);
		this.textPatchName.TabIndex = 29;
		this.textPatchName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.labelDescription.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDescription.Location = new System.Drawing.Point(0, 68);
		this.labelDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelDescription.Name = "labelDescription";
		this.labelDescription.Size = new System.Drawing.Size(400, 279);
		this.labelDescription.TabIndex = 32;
		this.labelDescription.Text = "Description";
		this.labelDescription.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.labelPatchVersion.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelPatchVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPatchVersion.Location = new System.Drawing.Point(0, 34);
		this.labelPatchVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelPatchVersion.Name = "labelPatchVersion";
		this.labelPatchVersion.Size = new System.Drawing.Size(400, 34);
		this.labelPatchVersion.TabIndex = 30;
		this.labelPatchVersion.Text = "Patch Version";
		this.labelPatchVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelPatchName.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelPatchName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelPatchName.Location = new System.Drawing.Point(0, 0);
		this.labelPatchName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelPatchName.Name = "labelPatchName";
		this.labelPatchName.Size = new System.Drawing.Size(400, 34);
		this.labelPatchName.TabIndex = 28;
		this.labelPatchName.Text = "Patch Name";
		this.labelPatchName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.statusLabel });
		this.statusBar.Location = new System.Drawing.Point(0, 893);
		this.statusBar.Name = "statusBar";
		this.statusBar.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
		this.statusBar.Size = new System.Drawing.Size(1543, 25);
		this.statusBar.TabIndex = 3;
		this.statusBar.Text = "statusStrip1";
		this.statusLabel.Name = "statusLabel";
		this.statusLabel.Size = new System.Drawing.Size(50, 20);
		this.statusLabel.Text = "Ready";
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(400, 55);
		this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.listViewPatch);
		this.splitContainer1.Panel2.Controls.Add(this.panelRight);
		this.splitContainer1.Size = new System.Drawing.Size(1143, 838);
		this.splitContainer1.SplitterDistance = 624;
		this.splitContainer1.SplitterWidth = 5;
		this.splitContainer1.TabIndex = 3;
		this.splitContainer1.TabStop = false;
		this.listViewPatch.AllowColumnReorder = true;
		this.listViewPatch.CheckBoxes = true;
		this.listViewPatch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[5] { this.columnItem, this.columnType, this.columnPatchId, this.columnImportId, this.columnComment });
		this.listViewPatch.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewPatch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.listViewPatch.FullRowSelect = true;
		this.listViewPatch.GridLines = true;
		this.listViewPatch.HideSelection = false;
		this.listViewPatch.Location = new System.Drawing.Point(0, 0);
		this.listViewPatch.Margin = new System.Windows.Forms.Padding(4);
		this.listViewPatch.Name = "listViewPatch";
		this.listViewPatch.Size = new System.Drawing.Size(624, 838);
		this.listViewPatch.TabIndex = 28;
		this.listViewPatch.UseCompatibleStateImageBehavior = false;
		this.listViewPatch.View = System.Windows.Forms.View.Details;
		this.listViewPatch.SelectedIndexChanged += new System.EventHandler(listViewPatch_SelectedIndexChanged);
		this.columnItem.Text = "Name";
		this.columnItem.Width = 136;
		this.columnType.Text = "Type";
		this.columnType.Width = 68;
		this.columnPatchId.Text = "Patch Id";
		this.columnPatchId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnPatchId.Width = 55;
		this.columnImportId.Text = "Import As";
		this.columnImportId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnComment.Text = "Comment";
		this.columnComment.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnComment.Width = 121;
		this.panelRight.AutoScroll = true;
		this.panelRight.Controls.Add(this.tabPreview);
		this.panelRight.Controls.Add(this.panelGraphicGroups);
		this.panelRight.Controls.Add(this.pictureBox1);
		this.panelRight.Controls.Add(this.groupReplaceSelection);
		this.panelRight.Controls.Add(this.labelDetails);
		this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelRight.Location = new System.Drawing.Point(0, 0);
		this.panelRight.Margin = new System.Windows.Forms.Padding(4);
		this.panelRight.Name = "panelRight";
		this.panelRight.Size = new System.Drawing.Size(514, 838);
		this.panelRight.TabIndex = 4;
		this.tabPreview.Controls.Add(this.pageViewer2D);
		this.tabPreview.Controls.Add(this.pageMultiViewer2D);
		this.tabPreview.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabPreview.Location = new System.Drawing.Point(0, 283);
		this.tabPreview.Margin = new System.Windows.Forms.Padding(4);
		this.tabPreview.Name = "tabPreview";
		this.tabPreview.SelectedIndex = 0;
		this.tabPreview.Size = new System.Drawing.Size(514, 555);
		this.tabPreview.TabIndex = 53;
		this.pageViewer2D.Controls.Add(this.viewer2D);
		this.pageViewer2D.Location = new System.Drawing.Point(4, 25);
		this.pageViewer2D.Margin = new System.Windows.Forms.Padding(4);
		this.pageViewer2D.Name = "pageViewer2D";
		this.pageViewer2D.Padding = new System.Windows.Forms.Padding(4);
		this.pageViewer2D.Size = new System.Drawing.Size(506, 526);
		this.pageViewer2D.TabIndex = 0;
		this.pageViewer2D.Text = "UI Art Assets";
		this.pageViewer2D.UseVisualStyleBackColor = true;
		this.viewer2D.AutoTransparency = false;
		this.viewer2D.BackColor = System.Drawing.Color.Transparent;
		this.viewer2D.ButtonStripVisible = false;
		this.viewer2D.CurrentBitmap = null;
		this.viewer2D.Dock = System.Windows.Forms.DockStyle.Fill;
		this.viewer2D.ExtendedFormat = false;
		this.viewer2D.FullSizeButton = false;
		this.viewer2D.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2D.ImageSize = new System.Drawing.Size(0, 0);
		this.viewer2D.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2D.Location = new System.Drawing.Point(4, 4);
		this.viewer2D.Margin = new System.Windows.Forms.Padding(5);
		this.viewer2D.Name = "viewer2D";
		this.viewer2D.RemoveButton = false;
		this.viewer2D.ShowButton = false;
		this.viewer2D.ShowButtonChecked = true;
		this.viewer2D.Size = new System.Drawing.Size(498, 518);
		this.viewer2D.TabIndex = 0;
		this.pageMultiViewer2D.Controls.Add(this.multiViewer2D);
		this.pageMultiViewer2D.Location = new System.Drawing.Point(4, 25);
		this.pageMultiViewer2D.Margin = new System.Windows.Forms.Padding(4);
		this.pageMultiViewer2D.Name = "pageMultiViewer2D";
		this.pageMultiViewer2D.Size = new System.Drawing.Size(506, 526);
		this.pageMultiViewer2D.TabIndex = 2;
		this.pageMultiViewer2D.Text = "Scene Assets";
		this.pageMultiViewer2D.UseVisualStyleBackColor = true;
		this.multiViewer2D.AutoTransparency = false;
		this.multiViewer2D.Bitmaps = null;
		this.multiViewer2D.CheckBitmapSize = false;
		this.multiViewer2D.Dock = System.Windows.Forms.DockStyle.Fill;
		this.multiViewer2D.FixedSize = false;
		this.multiViewer2D.FullSizeButton = false;
		this.multiViewer2D.LabelText = "Image n.";
		this.multiViewer2D.Location = new System.Drawing.Point(0, 0);
		this.multiViewer2D.Margin = new System.Windows.Forms.Padding(4);
		this.multiViewer2D.Name = "multiViewer2D";
		this.multiViewer2D.ShowButton = false;
		this.multiViewer2D.ShowDeleteButton = false;
		this.multiViewer2D.Size = new System.Drawing.Size(506, 526);
		this.multiViewer2D.TabIndex = 0;
		this.panelGraphicGroups.Controls.Add(this.groupTeam);
		this.panelGraphicGroups.Controls.Add(this.groupLeague);
		this.panelGraphicGroups.Controls.Add(this.groupStadium);
		this.panelGraphicGroups.Controls.Add(this.groupShoes);
		this.panelGraphicGroups.Controls.Add(this.groupBall);
		this.panelGraphicGroups.Controls.Add(this.groupCountry);
		this.panelGraphicGroups.Controls.Add(this.groupAdboards);
		this.panelGraphicGroups.Controls.Add(this.groupKit);
		this.panelGraphicGroups.Controls.Add(this.groupPlayer);
		this.panelGraphicGroups.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGraphicGroups.Location = new System.Drawing.Point(0, 155);
		this.panelGraphicGroups.Margin = new System.Windows.Forms.Padding(4);
		this.panelGraphicGroups.Name = "panelGraphicGroups";
		this.panelGraphicGroups.Size = new System.Drawing.Size(514, 128);
		this.panelGraphicGroups.TabIndex = 52;
		this.panelGraphicGroups.Visible = false;
		this.groupTeam.Controls.Add(this.radioTeamCrest50);
		this.groupTeam.Controls.Add(this.radioTeamCrest16);
		this.groupTeam.Controls.Add(this.radioTeamCrest32);
		this.groupTeam.Controls.Add(this.radioTeamCrestLarge);
		this.groupTeam.Controls.Add(this.radioTeamFlags);
		this.groupTeam.Controls.Add(this.radioTeamBanners);
		this.groupTeam.Location = new System.Drawing.Point(7, 6);
		this.groupTeam.Margin = new System.Windows.Forms.Padding(4);
		this.groupTeam.Name = "groupTeam";
		this.groupTeam.Padding = new System.Windows.Forms.Padding(4);
		this.groupTeam.Size = new System.Drawing.Size(320, 111);
		this.groupTeam.TabIndex = 41;
		this.groupTeam.TabStop = false;
		this.groupTeam.Text = "Team";
		this.groupTeam.Visible = false;
		this.radioTeamCrest50.AutoSize = true;
		this.radioTeamCrest50.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioTeamCrest50.Location = new System.Drawing.Point(164, 20);
		this.radioTeamCrest50.Margin = new System.Windows.Forms.Padding(4);
		this.radioTeamCrest50.Name = "radioTeamCrest50";
		this.radioTeamCrest50.Size = new System.Drawing.Size(112, 21);
		this.radioTeamCrest50.TabIndex = 14;
		this.radioTeamCrest50.Text = "Crest 50 x 50";
		this.radioTeamCrest50.UseVisualStyleBackColor = true;
		this.radioTeamCrest50.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioTeamCrest16.AutoSize = true;
		this.radioTeamCrest16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioTeamCrest16.Location = new System.Drawing.Point(164, 71);
		this.radioTeamCrest16.Margin = new System.Windows.Forms.Padding(4);
		this.radioTeamCrest16.Name = "radioTeamCrest16";
		this.radioTeamCrest16.Size = new System.Drawing.Size(112, 21);
		this.radioTeamCrest16.TabIndex = 13;
		this.radioTeamCrest16.Text = "Crest 16 x 16";
		this.radioTeamCrest16.UseVisualStyleBackColor = true;
		this.radioTeamCrest16.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioTeamCrest32.AutoSize = true;
		this.radioTeamCrest32.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioTeamCrest32.Location = new System.Drawing.Point(164, 46);
		this.radioTeamCrest32.Margin = new System.Windows.Forms.Padding(4);
		this.radioTeamCrest32.Name = "radioTeamCrest32";
		this.radioTeamCrest32.Size = new System.Drawing.Size(112, 21);
		this.radioTeamCrest32.TabIndex = 12;
		this.radioTeamCrest32.Text = "Crest 32 x 32";
		this.radioTeamCrest32.UseVisualStyleBackColor = true;
		this.radioTeamCrest32.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioTeamCrestLarge.AutoSize = true;
		this.radioTeamCrestLarge.Checked = true;
		this.radioTeamCrestLarge.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioTeamCrestLarge.Location = new System.Drawing.Point(8, 20);
		this.radioTeamCrestLarge.Margin = new System.Windows.Forms.Padding(4);
		this.radioTeamCrestLarge.Name = "radioTeamCrestLarge";
		this.radioTeamCrestLarge.Size = new System.Drawing.Size(62, 21);
		this.radioTeamCrestLarge.TabIndex = 11;
		this.radioTeamCrestLarge.TabStop = true;
		this.radioTeamCrestLarge.Text = "Crest";
		this.radioTeamCrestLarge.UseVisualStyleBackColor = true;
		this.radioTeamCrestLarge.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioTeamFlags.AutoSize = true;
		this.radioTeamFlags.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioTeamFlags.Location = new System.Drawing.Point(9, 73);
		this.radioTeamFlags.Margin = new System.Windows.Forms.Padding(4);
		this.radioTeamFlags.Name = "radioTeamFlags";
		this.radioTeamFlags.Size = new System.Drawing.Size(63, 21);
		this.radioTeamFlags.TabIndex = 10;
		this.radioTeamFlags.Text = "Flags";
		this.radioTeamFlags.UseVisualStyleBackColor = true;
		this.radioTeamFlags.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioTeamBanners.AutoSize = true;
		this.radioTeamBanners.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioTeamBanners.Location = new System.Drawing.Point(9, 46);
		this.radioTeamBanners.Margin = new System.Windows.Forms.Padding(4);
		this.radioTeamBanners.Name = "radioTeamBanners";
		this.radioTeamBanners.Size = new System.Drawing.Size(82, 21);
		this.radioTeamBanners.TabIndex = 9;
		this.radioTeamBanners.Text = "Banners";
		this.radioTeamBanners.UseVisualStyleBackColor = true;
		this.radioTeamBanners.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupLeague.Controls.Add(this.radioLeagueLogo512x128);
		this.groupLeague.Controls.Add(this.radioLeagueAnimLogo);
		this.groupLeague.Controls.Add(this.radioLeagueTinyLogo);
		this.groupLeague.Controls.Add(this.radioLeagueSmallLogo);
		this.groupLeague.Location = new System.Drawing.Point(7, 6);
		this.groupLeague.Margin = new System.Windows.Forms.Padding(4);
		this.groupLeague.Name = "groupLeague";
		this.groupLeague.Padding = new System.Windows.Forms.Padding(4);
		this.groupLeague.Size = new System.Drawing.Size(320, 111);
		this.groupLeague.TabIndex = 49;
		this.groupLeague.TabStop = false;
		this.groupLeague.Text = "League";
		this.groupLeague.Visible = false;
		this.radioLeagueLogo512x128.AutoSize = true;
		this.radioLeagueLogo512x128.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioLeagueLogo512x128.Location = new System.Drawing.Point(140, 23);
		this.radioLeagueLogo512x128.Margin = new System.Windows.Forms.Padding(4);
		this.radioLeagueLogo512x128.Name = "radioLeagueLogo512x128";
		this.radioLeagueLogo512x128.Size = new System.Drawing.Size(127, 21);
		this.radioLeagueLogo512x128.TabIndex = 4;
		this.radioLeagueLogo512x128.Text = "Logo 512 x 128";
		this.radioLeagueLogo512x128.UseVisualStyleBackColor = true;
		this.radioLeagueLogo512x128.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioLeagueAnimLogo.AutoSize = true;
		this.radioLeagueAnimLogo.Checked = true;
		this.radioLeagueAnimLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioLeagueAnimLogo.Location = new System.Drawing.Point(9, 23);
		this.radioLeagueAnimLogo.Margin = new System.Windows.Forms.Padding(4);
		this.radioLeagueAnimLogo.Name = "radioLeagueAnimLogo";
		this.radioLeagueAnimLogo.Size = new System.Drawing.Size(95, 21);
		this.radioLeagueAnimLogo.TabIndex = 3;
		this.radioLeagueAnimLogo.TabStop = true;
		this.radioLeagueAnimLogo.Text = "Main Logo";
		this.radioLeagueAnimLogo.UseVisualStyleBackColor = true;
		this.radioLeagueAnimLogo.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioLeagueTinyLogo.AutoSize = true;
		this.radioLeagueTinyLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioLeagueTinyLogo.Location = new System.Drawing.Point(8, 55);
		this.radioLeagueTinyLogo.Margin = new System.Windows.Forms.Padding(4);
		this.radioLeagueTinyLogo.Name = "radioLeagueTinyLogo";
		this.radioLeagueTinyLogo.Size = new System.Drawing.Size(92, 21);
		this.radioLeagueTinyLogo.TabIndex = 2;
		this.radioLeagueTinyLogo.Text = "Tiny Logo";
		this.radioLeagueTinyLogo.UseVisualStyleBackColor = true;
		this.radioLeagueTinyLogo.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioLeagueSmallLogo.AutoSize = true;
		this.radioLeagueSmallLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioLeagueSmallLogo.Location = new System.Drawing.Point(140, 54);
		this.radioLeagueSmallLogo.Margin = new System.Windows.Forms.Padding(4);
		this.radioLeagueSmallLogo.Name = "radioLeagueSmallLogo";
		this.radioLeagueSmallLogo.Size = new System.Drawing.Size(99, 21);
		this.radioLeagueSmallLogo.TabIndex = 1;
		this.radioLeagueSmallLogo.Text = "Small Logo";
		this.radioLeagueSmallLogo.UseVisualStyleBackColor = true;
		this.radioLeagueSmallLogo.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupStadium.Controls.Add(this.radioStadiumPreview);
		this.groupStadium.Controls.Add(this.groupTod);
		this.groupStadium.Controls.Add(this.radioStadium3D);
		this.groupStadium.Location = new System.Drawing.Point(7, 6);
		this.groupStadium.Margin = new System.Windows.Forms.Padding(4);
		this.groupStadium.Name = "groupStadium";
		this.groupStadium.Padding = new System.Windows.Forms.Padding(4);
		this.groupStadium.Size = new System.Drawing.Size(320, 111);
		this.groupStadium.TabIndex = 45;
		this.groupStadium.TabStop = false;
		this.groupStadium.Text = "Stadium";
		this.groupStadium.Visible = false;
		this.radioStadiumPreview.AutoSize = true;
		this.radioStadiumPreview.Checked = true;
		this.radioStadiumPreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioStadiumPreview.Location = new System.Drawing.Point(9, 20);
		this.radioStadiumPreview.Margin = new System.Windows.Forms.Padding(4);
		this.radioStadiumPreview.Name = "radioStadiumPreview";
		this.radioStadiumPreview.Size = new System.Drawing.Size(78, 21);
		this.radioStadiumPreview.TabIndex = 12;
		this.radioStadiumPreview.TabStop = true;
		this.radioStadiumPreview.Text = "Preview";
		this.radioStadiumPreview.UseVisualStyleBackColor = true;
		this.radioStadiumPreview.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupTod.Controls.Add(this.radioStadiumGuiNight);
		this.groupTod.Controls.Add(this.radioStadiumGuiSunset);
		this.groupTod.Controls.Add(this.radioStadiumGuiOvercast);
		this.groupTod.Controls.Add(this.radioStadiumGuiClearDay);
		this.groupTod.Location = new System.Drawing.Point(173, 9);
		this.groupTod.Margin = new System.Windows.Forms.Padding(4);
		this.groupTod.Name = "groupTod";
		this.groupTod.Padding = new System.Windows.Forms.Padding(4);
		this.groupTod.Size = new System.Drawing.Size(139, 102);
		this.groupTod.TabIndex = 11;
		this.groupTod.TabStop = false;
		this.groupTod.Text = "Time of Day";
		this.radioStadiumGuiNight.AutoSize = true;
		this.radioStadiumGuiNight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioStadiumGuiNight.Location = new System.Drawing.Point(9, 58);
		this.radioStadiumGuiNight.Margin = new System.Windows.Forms.Padding(4);
		this.radioStadiumGuiNight.Name = "radioStadiumGuiNight";
		this.radioStadiumGuiNight.Size = new System.Drawing.Size(62, 21);
		this.radioStadiumGuiNight.TabIndex = 10;
		this.radioStadiumGuiNight.TabStop = true;
		this.radioStadiumGuiNight.Text = "Night";
		this.radioStadiumGuiNight.UseVisualStyleBackColor = true;
		this.radioStadiumGuiNight.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioStadiumGuiSunset.AutoSize = true;
		this.radioStadiumGuiSunset.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioStadiumGuiSunset.Location = new System.Drawing.Point(9, 78);
		this.radioStadiumGuiSunset.Margin = new System.Windows.Forms.Padding(4);
		this.radioStadiumGuiSunset.Name = "radioStadiumGuiSunset";
		this.radioStadiumGuiSunset.Size = new System.Drawing.Size(73, 21);
		this.radioStadiumGuiSunset.TabIndex = 9;
		this.radioStadiumGuiSunset.TabStop = true;
		this.radioStadiumGuiSunset.Text = "Sunset";
		this.radioStadiumGuiSunset.UseVisualStyleBackColor = true;
		this.radioStadiumGuiSunset.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioStadiumGuiOvercast.AutoSize = true;
		this.radioStadiumGuiOvercast.Checked = true;
		this.radioStadiumGuiOvercast.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioStadiumGuiOvercast.Location = new System.Drawing.Point(9, 17);
		this.radioStadiumGuiOvercast.Margin = new System.Windows.Forms.Padding(4);
		this.radioStadiumGuiOvercast.Name = "radioStadiumGuiOvercast";
		this.radioStadiumGuiOvercast.Size = new System.Drawing.Size(115, 21);
		this.radioStadiumGuiOvercast.TabIndex = 6;
		this.radioStadiumGuiOvercast.TabStop = true;
		this.radioStadiumGuiOvercast.Text = "Overcast Day";
		this.radioStadiumGuiOvercast.UseVisualStyleBackColor = true;
		this.radioStadiumGuiOvercast.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioStadiumGuiClearDay.AutoSize = true;
		this.radioStadiumGuiClearDay.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioStadiumGuiClearDay.Location = new System.Drawing.Point(9, 37);
		this.radioStadiumGuiClearDay.Margin = new System.Windows.Forms.Padding(4);
		this.radioStadiumGuiClearDay.Name = "radioStadiumGuiClearDay";
		this.radioStadiumGuiClearDay.Size = new System.Drawing.Size(91, 21);
		this.radioStadiumGuiClearDay.TabIndex = 7;
		this.radioStadiumGuiClearDay.TabStop = true;
		this.radioStadiumGuiClearDay.Text = "Clear Day";
		this.radioStadiumGuiClearDay.UseVisualStyleBackColor = true;
		this.radioStadiumGuiClearDay.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioStadium3D.AutoSize = true;
		this.radioStadium3D.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioStadium3D.Location = new System.Drawing.Point(9, 46);
		this.radioStadium3D.Margin = new System.Windows.Forms.Padding(4);
		this.radioStadium3D.Name = "radioStadium3D";
		this.radioStadium3D.Size = new System.Drawing.Size(106, 21);
		this.radioStadium3D.TabIndex = 8;
		this.radioStadium3D.TabStop = true;
		this.radioStadium3D.Text = "3D Textures";
		this.radioStadium3D.UseVisualStyleBackColor = true;
		this.radioStadium3D.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupShoes.Controls.Add(this.radioShoesColor);
		this.groupShoes.Location = new System.Drawing.Point(7, 6);
		this.groupShoes.Margin = new System.Windows.Forms.Padding(4);
		this.groupShoes.Name = "groupShoes";
		this.groupShoes.Padding = new System.Windows.Forms.Padding(4);
		this.groupShoes.Size = new System.Drawing.Size(320, 111);
		this.groupShoes.TabIndex = 52;
		this.groupShoes.TabStop = false;
		this.groupShoes.Text = "Shoes";
		this.groupShoes.Visible = false;
		this.radioShoesColor.AutoSize = true;
		this.radioShoesColor.Checked = true;
		this.radioShoesColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioShoesColor.Location = new System.Drawing.Point(8, 23);
		this.radioShoesColor.Margin = new System.Windows.Forms.Padding(4);
		this.radioShoesColor.Name = "radioShoesColor";
		this.radioShoesColor.Size = new System.Drawing.Size(121, 21);
		this.radioShoesColor.TabIndex = 9;
		this.radioShoesColor.TabStop = true;
		this.radioShoesColor.Text = "Color Textures";
		this.radioShoesColor.UseVisualStyleBackColor = true;
		this.radioShoesColor.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupBall.Controls.Add(this.radioBallPreview);
		this.groupBall.Controls.Add(this.radioBallTexture);
		this.groupBall.Location = new System.Drawing.Point(7, 6);
		this.groupBall.Margin = new System.Windows.Forms.Padding(4);
		this.groupBall.Name = "groupBall";
		this.groupBall.Padding = new System.Windows.Forms.Padding(4);
		this.groupBall.Size = new System.Drawing.Size(320, 111);
		this.groupBall.TabIndex = 47;
		this.groupBall.TabStop = false;
		this.groupBall.Text = "Ball";
		this.groupBall.Visible = false;
		this.radioBallPreview.AutoSize = true;
		this.radioBallPreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioBallPreview.Location = new System.Drawing.Point(8, 52);
		this.radioBallPreview.Margin = new System.Windows.Forms.Padding(4);
		this.radioBallPreview.Name = "radioBallPreview";
		this.radioBallPreview.Size = new System.Drawing.Size(78, 21);
		this.radioBallPreview.TabIndex = 4;
		this.radioBallPreview.TabStop = true;
		this.radioBallPreview.Text = "Preview";
		this.radioBallPreview.UseVisualStyleBackColor = true;
		this.radioBallPreview.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioBallTexture.AutoSize = true;
		this.radioBallTexture.Checked = true;
		this.radioBallTexture.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioBallTexture.Location = new System.Drawing.Point(8, 23);
		this.radioBallTexture.Margin = new System.Windows.Forms.Padding(4);
		this.radioBallTexture.Name = "radioBallTexture";
		this.radioBallTexture.Size = new System.Drawing.Size(84, 21);
		this.radioBallTexture.TabIndex = 3;
		this.radioBallTexture.TabStop = true;
		this.radioBallTexture.Text = "Textures";
		this.radioBallTexture.UseVisualStyleBackColor = true;
		this.radioBallTexture.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupCountry.Controls.Add(this.radioCountryMap);
		this.groupCountry.Controls.Add(this.radioCountryFlag512x512);
		this.groupCountry.Controls.Add(this.radioCountryCard);
		this.groupCountry.Controls.Add(this.radioCountryMainFlag);
		this.groupCountry.Controls.Add(this.radioCountryMiniflag);
		this.groupCountry.Location = new System.Drawing.Point(7, 6);
		this.groupCountry.Margin = new System.Windows.Forms.Padding(4);
		this.groupCountry.Name = "groupCountry";
		this.groupCountry.Padding = new System.Windows.Forms.Padding(4);
		this.groupCountry.Size = new System.Drawing.Size(320, 111);
		this.groupCountry.TabIndex = 48;
		this.groupCountry.TabStop = false;
		this.groupCountry.Text = "Country";
		this.groupCountry.Visible = false;
		this.radioCountryMap.AutoSize = true;
		this.radioCountryMap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCountryMap.Location = new System.Drawing.Point(133, 58);
		this.radioCountryMap.Margin = new System.Windows.Forms.Padding(4);
		this.radioCountryMap.Name = "radioCountryMap";
		this.radioCountryMap.Size = new System.Drawing.Size(56, 21);
		this.radioCountryMap.TabIndex = 7;
		this.radioCountryMap.Text = "Map";
		this.radioCountryMap.UseVisualStyleBackColor = false;
		this.radioCountryMap.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioCountryFlag512x512.AutoSize = true;
		this.radioCountryFlag512x512.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCountryFlag512x512.Location = new System.Drawing.Point(132, 27);
		this.radioCountryFlag512x512.Margin = new System.Windows.Forms.Padding(4);
		this.radioCountryFlag512x512.Name = "radioCountryFlag512x512";
		this.radioCountryFlag512x512.Size = new System.Drawing.Size(122, 21);
		this.radioCountryFlag512x512.TabIndex = 6;
		this.radioCountryFlag512x512.Text = "Flag 512 x 512";
		this.radioCountryFlag512x512.UseVisualStyleBackColor = false;
		this.radioCountryFlag512x512.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioCountryCard.AutoSize = true;
		this.radioCountryCard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCountryCard.Location = new System.Drawing.Point(8, 84);
		this.radioCountryCard.Margin = new System.Windows.Forms.Padding(4);
		this.radioCountryCard.Name = "radioCountryCard";
		this.radioCountryCard.Size = new System.Drawing.Size(59, 21);
		this.radioCountryCard.TabIndex = 5;
		this.radioCountryCard.Text = "Card";
		this.radioCountryCard.UseVisualStyleBackColor = true;
		this.radioCountryCard.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioCountryMainFlag.AutoSize = true;
		this.radioCountryMainFlag.Checked = true;
		this.radioCountryMainFlag.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCountryMainFlag.Location = new System.Drawing.Point(8, 27);
		this.radioCountryMainFlag.Margin = new System.Windows.Forms.Padding(4);
		this.radioCountryMainFlag.Name = "radioCountryMainFlag";
		this.radioCountryMainFlag.Size = new System.Drawing.Size(90, 21);
		this.radioCountryMainFlag.TabIndex = 4;
		this.radioCountryMainFlag.TabStop = true;
		this.radioCountryMainFlag.Text = "Main Flag";
		this.radioCountryMainFlag.UseVisualStyleBackColor = false;
		this.radioCountryMainFlag.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioCountryMiniflag.AutoSize = true;
		this.radioCountryMiniflag.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCountryMiniflag.Location = new System.Drawing.Point(8, 55);
		this.radioCountryMiniflag.Margin = new System.Windows.Forms.Padding(4);
		this.radioCountryMiniflag.Name = "radioCountryMiniflag";
		this.radioCountryMiniflag.Size = new System.Drawing.Size(77, 21);
		this.radioCountryMiniflag.TabIndex = 2;
		this.radioCountryMiniflag.Text = "Miniflag";
		this.radioCountryMiniflag.UseVisualStyleBackColor = true;
		this.radioCountryMiniflag.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupAdboards.Controls.Add(this.radioAdboard1);
		this.groupAdboards.Location = new System.Drawing.Point(7, 6);
		this.groupAdboards.Margin = new System.Windows.Forms.Padding(4);
		this.groupAdboards.Name = "groupAdboards";
		this.groupAdboards.Padding = new System.Windows.Forms.Padding(4);
		this.groupAdboards.Size = new System.Drawing.Size(320, 111);
		this.groupAdboards.TabIndex = 46;
		this.groupAdboards.TabStop = false;
		this.groupAdboards.Text = "Adboards";
		this.groupAdboards.Visible = false;
		this.radioAdboard1.AutoSize = true;
		this.radioAdboard1.Checked = true;
		this.radioAdboard1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioAdboard1.Location = new System.Drawing.Point(8, 23);
		this.radioAdboard1.Margin = new System.Windows.Forms.Padding(4);
		this.radioAdboard1.Name = "radioAdboard1";
		this.radioAdboard1.Size = new System.Drawing.Size(77, 21);
		this.radioAdboard1.TabIndex = 9;
		this.radioAdboard1.TabStop = true;
		this.radioAdboard1.Text = "Texture";
		this.radioAdboard1.UseVisualStyleBackColor = true;
		this.radioAdboard1.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupKit.Controls.Add(this.radioKitKit);
		this.groupKit.Controls.Add(this.radioKitMinikit);
		this.groupKit.Location = new System.Drawing.Point(7, 6);
		this.groupKit.Margin = new System.Windows.Forms.Padding(4);
		this.groupKit.Name = "groupKit";
		this.groupKit.Padding = new System.Windows.Forms.Padding(4);
		this.groupKit.Size = new System.Drawing.Size(320, 111);
		this.groupKit.TabIndex = 51;
		this.groupKit.TabStop = false;
		this.groupKit.Text = "Kit";
		this.groupKit.Visible = false;
		this.radioKitKit.AutoSize = true;
		this.radioKitKit.Checked = true;
		this.radioKitKit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioKitKit.Location = new System.Drawing.Point(8, 27);
		this.radioKitKit.Margin = new System.Windows.Forms.Padding(4);
		this.radioKitKit.Name = "radioKitKit";
		this.radioKitKit.Size = new System.Drawing.Size(104, 21);
		this.radioKitKit.TabIndex = 4;
		this.radioKitKit.TabStop = true;
		this.radioKitKit.Text = "Kit Textures";
		this.radioKitKit.UseVisualStyleBackColor = false;
		this.radioKitKit.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioKitMinikit.AutoSize = true;
		this.radioKitMinikit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioKitMinikit.Location = new System.Drawing.Point(8, 55);
		this.radioKitMinikit.Margin = new System.Windows.Forms.Padding(4);
		this.radioKitMinikit.Name = "radioKitMinikit";
		this.radioKitMinikit.Size = new System.Drawing.Size(68, 21);
		this.radioKitMinikit.TabIndex = 2;
		this.radioKitMinikit.TabStop = true;
		this.radioKitMinikit.Text = "Minikit";
		this.radioKitMinikit.UseVisualStyleBackColor = true;
		this.radioKitMinikit.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.groupPlayer.Controls.Add(this.radioHairTextures);
		this.groupPlayer.Controls.Add(this.radioHairColorTexture);
		this.groupPlayer.Controls.Add(this.radioEyesTexture);
		this.groupPlayer.Controls.Add(this.radioFaceTexture);
		this.groupPlayer.Controls.Add(this.radioMiniHead);
		this.groupPlayer.Location = new System.Drawing.Point(7, 6);
		this.groupPlayer.Margin = new System.Windows.Forms.Padding(4);
		this.groupPlayer.Name = "groupPlayer";
		this.groupPlayer.Padding = new System.Windows.Forms.Padding(4);
		this.groupPlayer.Size = new System.Drawing.Size(320, 111);
		this.groupPlayer.TabIndex = 50;
		this.groupPlayer.TabStop = false;
		this.groupPlayer.Text = "Player";
		this.groupPlayer.Visible = false;
		this.radioHairTextures.AutoSize = true;
		this.radioHairTextures.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioHairTextures.Location = new System.Drawing.Point(155, 54);
		this.radioHairTextures.Margin = new System.Windows.Forms.Padding(4);
		this.radioHairTextures.Name = "radioHairTextures";
		this.radioHairTextures.Size = new System.Drawing.Size(107, 21);
		this.radioHairTextures.TabIndex = 52;
		this.radioHairTextures.TabStop = true;
		this.radioHairTextures.Text = "Hair Texture";
		this.radioHairTextures.UseVisualStyleBackColor = true;
		this.radioHairTextures.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioHairColorTexture.AutoSize = true;
		this.radioHairColorTexture.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioHairColorTexture.Location = new System.Drawing.Point(155, 30);
		this.radioHairColorTexture.Margin = new System.Windows.Forms.Padding(4);
		this.radioHairColorTexture.Name = "radioHairColorTexture";
		this.radioHairColorTexture.Size = new System.Drawing.Size(144, 21);
		this.radioHairColorTexture.TabIndex = 51;
		this.radioHairColorTexture.TabStop = true;
		this.radioHairColorTexture.Text = "Hair Color Texture";
		this.radioHairColorTexture.UseVisualStyleBackColor = true;
		this.radioHairColorTexture.Visible = false;
		this.radioHairColorTexture.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioEyesTexture.AutoSize = true;
		this.radioEyesTexture.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioEyesTexture.Location = new System.Drawing.Point(16, 76);
		this.radioEyesTexture.Margin = new System.Windows.Forms.Padding(4);
		this.radioEyesTexture.Name = "radioEyesTexture";
		this.radioEyesTexture.Size = new System.Drawing.Size(112, 21);
		this.radioEyesTexture.TabIndex = 2;
		this.radioEyesTexture.TabStop = true;
		this.radioEyesTexture.Text = "Eyes Texture";
		this.radioEyesTexture.UseVisualStyleBackColor = true;
		this.radioEyesTexture.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioFaceTexture.AutoSize = true;
		this.radioFaceTexture.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioFaceTexture.Location = new System.Drawing.Point(16, 52);
		this.radioFaceTexture.Margin = new System.Windows.Forms.Padding(4);
		this.radioFaceTexture.Name = "radioFaceTexture";
		this.radioFaceTexture.Size = new System.Drawing.Size(112, 21);
		this.radioFaceTexture.TabIndex = 1;
		this.radioFaceTexture.TabStop = true;
		this.radioFaceTexture.Text = "Face Texture";
		this.radioFaceTexture.UseVisualStyleBackColor = true;
		this.radioFaceTexture.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.radioMiniHead.AutoSize = true;
		this.radioMiniHead.Checked = true;
		this.radioMiniHead.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioMiniHead.Location = new System.Drawing.Point(16, 25);
		this.radioMiniHead.Margin = new System.Windows.Forms.Padding(4);
		this.radioMiniHead.Name = "radioMiniHead";
		this.radioMiniHead.Size = new System.Drawing.Size(92, 21);
		this.radioMiniHead.TabIndex = 0;
		this.radioMiniHead.TabStop = true;
		this.radioMiniHead.Text = "Mini Head";
		this.radioMiniHead.UseVisualStyleBackColor = true;
		this.radioMiniHead.CheckedChanged += new System.EventHandler(radioViewer_CheckedChanged);
		this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureBox1.Location = new System.Drawing.Point(-356, 242);
		this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(356, 290);
		this.pictureBox1.TabIndex = 38;
		this.pictureBox1.TabStop = false;
		this.pictureBox1.Visible = false;
		this.groupReplaceSelection.BackColor = System.Drawing.SystemColors.Control;
		this.groupReplaceSelection.Controls.Add(this.comboReplaceLicensedTournament);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceKit);
		this.groupReplaceSelection.Controls.Add(this.labelCmsCreated);
		this.groupReplaceSelection.Controls.Add(this.labelCmsReplaced);
		this.groupReplaceSelection.Controls.Add(this.textCmsReplaced);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceMowingPattern);
		this.groupReplaceSelection.Controls.Add(this.radioCmsItem);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceGkGloves);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceNet);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceShoes);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceNamesFont);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceNumberFont);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceAdboard);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceBall);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceReferee);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceSponsor);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceFormation);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceTournament);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceStadium);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceCountry);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceLeague);
		this.groupReplaceSelection.Controls.Add(this.comboReplacePlayer);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceTeam);
		this.groupReplaceSelection.Controls.Add(this.radioReplaceItem);
		this.groupReplaceSelection.Controls.Add(this.radioCreateItem);
		this.groupReplaceSelection.Dock = System.Windows.Forms.DockStyle.Top;
		this.groupReplaceSelection.Location = new System.Drawing.Point(0, 18);
		this.groupReplaceSelection.Margin = new System.Windows.Forms.Padding(4);
		this.groupReplaceSelection.Name = "groupReplaceSelection";
		this.groupReplaceSelection.Padding = new System.Windows.Forms.Padding(4);
		this.groupReplaceSelection.Size = new System.Drawing.Size(514, 137);
		this.groupReplaceSelection.TabIndex = 37;
		this.groupReplaceSelection.TabStop = false;
		this.groupReplaceSelection.Text = "Replace Selection";
		this.comboReplaceLicensedTournament.FormattingEnabled = true;
		this.comboReplaceLicensedTournament.Location = new System.Drawing.Point(109, 48);
		this.comboReplaceLicensedTournament.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceLicensedTournament.MaxDropDownItems = 20;
		this.comboReplaceLicensedTournament.Name = "comboReplaceLicensedTournament";
		this.comboReplaceLicensedTournament.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceLicensedTournament.TabIndex = 42;
		this.comboReplaceLicensedTournament.Visible = false;
		this.comboReplaceLicensedTournament.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceKit.FormattingEnabled = true;
		this.comboReplaceKit.Location = new System.Drawing.Point(109, 48);
		this.comboReplaceKit.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceKit.MaxDropDownItems = 20;
		this.comboReplaceKit.Name = "comboReplaceKit";
		this.comboReplaceKit.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceKit.TabIndex = 41;
		this.comboReplaceKit.Visible = false;
		this.comboReplaceKit.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.labelCmsCreated.AutoSize = true;
		this.labelCmsCreated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelCmsCreated.ForeColor = System.Drawing.Color.Green;
		this.labelCmsCreated.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCmsCreated.Location = new System.Drawing.Point(19, 107);
		this.labelCmsCreated.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelCmsCreated.Name = "labelCmsCreated";
		this.labelCmsCreated.Size = new System.Drawing.Size(56, 17);
		this.labelCmsCreated.TabIndex = 40;
		this.labelCmsCreated.Text = "Create";
		this.labelCmsCreated.Visible = false;
		this.labelCmsReplaced.AutoSize = true;
		this.labelCmsReplaced.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelCmsReplaced.ForeColor = System.Drawing.Color.Red;
		this.labelCmsReplaced.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCmsReplaced.Location = new System.Drawing.Point(19, 107);
		this.labelCmsReplaced.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelCmsReplaced.Name = "labelCmsReplaced";
		this.labelCmsReplaced.Size = new System.Drawing.Size(67, 17);
		this.labelCmsReplaced.TabIndex = 22;
		this.labelCmsReplaced.Text = "Replace";
		this.labelCmsReplaced.Visible = false;
		this.textCmsReplaced.BackColor = System.Drawing.Color.White;
		this.textCmsReplaced.Location = new System.Drawing.Point(108, 103);
		this.textCmsReplaced.Margin = new System.Windows.Forms.Padding(4);
		this.textCmsReplaced.Name = "textCmsReplaced";
		this.textCmsReplaced.ReadOnly = true;
		this.textCmsReplaced.Size = new System.Drawing.Size(236, 22);
		this.textCmsReplaced.TabIndex = 21;
		this.textCmsReplaced.Visible = false;
		this.comboReplaceMowingPattern.FormattingEnabled = true;
		this.comboReplaceMowingPattern.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceMowingPattern.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceMowingPattern.MaxDropDownItems = 20;
		this.comboReplaceMowingPattern.Name = "comboReplaceMowingPattern";
		this.comboReplaceMowingPattern.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceMowingPattern.TabIndex = 20;
		this.comboReplaceMowingPattern.Visible = false;
		this.comboReplaceMowingPattern.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.radioCmsItem.AutoSize = true;
		this.radioCmsItem.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCmsItem.Location = new System.Drawing.Point(13, 79);
		this.radioCmsItem.Margin = new System.Windows.Forms.Padding(4);
		this.radioCmsItem.Name = "radioCmsItem";
		this.radioCmsItem.Size = new System.Drawing.Size(109, 21);
		this.radioCmsItem.TabIndex = 19;
		this.radioCmsItem.TabStop = true;
		this.radioCmsItem.Text = "Use Patch Id";
		this.radioCmsItem.UseVisualStyleBackColor = true;
		this.radioCmsItem.CheckedChanged += new System.EventHandler(radioUsePatchItem_CheckedChanged);
		this.comboReplaceGkGloves.FormattingEnabled = true;
		this.comboReplaceGkGloves.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceGkGloves.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceGkGloves.MaxDropDownItems = 20;
		this.comboReplaceGkGloves.Name = "comboReplaceGkGloves";
		this.comboReplaceGkGloves.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceGkGloves.TabIndex = 18;
		this.comboReplaceGkGloves.Visible = false;
		this.comboReplaceGkGloves.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceNet.FormattingEnabled = true;
		this.comboReplaceNet.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceNet.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceNet.MaxDropDownItems = 20;
		this.comboReplaceNet.Name = "comboReplaceNet";
		this.comboReplaceNet.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceNet.TabIndex = 17;
		this.comboReplaceNet.Visible = false;
		this.comboReplaceNet.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceShoes.FormattingEnabled = true;
		this.comboReplaceShoes.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceShoes.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceShoes.MaxDropDownItems = 20;
		this.comboReplaceShoes.Name = "comboReplaceShoes";
		this.comboReplaceShoes.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceShoes.TabIndex = 16;
		this.comboReplaceShoes.Visible = false;
		this.comboReplaceShoes.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceNamesFont.FormattingEnabled = true;
		this.comboReplaceNamesFont.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceNamesFont.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceNamesFont.MaxDropDownItems = 20;
		this.comboReplaceNamesFont.Name = "comboReplaceNamesFont";
		this.comboReplaceNamesFont.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceNamesFont.TabIndex = 15;
		this.comboReplaceNamesFont.Visible = false;
		this.comboReplaceNamesFont.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceNumberFont.FormattingEnabled = true;
		this.comboReplaceNumberFont.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceNumberFont.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceNumberFont.MaxDropDownItems = 20;
		this.comboReplaceNumberFont.Name = "comboReplaceNumberFont";
		this.comboReplaceNumberFont.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceNumberFont.TabIndex = 14;
		this.comboReplaceNumberFont.Visible = false;
		this.comboReplaceNumberFont.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceAdboard.FormattingEnabled = true;
		this.comboReplaceAdboard.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceAdboard.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceAdboard.MaxDropDownItems = 20;
		this.comboReplaceAdboard.Name = "comboReplaceAdboard";
		this.comboReplaceAdboard.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceAdboard.TabIndex = 13;
		this.comboReplaceAdboard.Visible = false;
		this.comboReplaceAdboard.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceBall.FormattingEnabled = true;
		this.comboReplaceBall.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceBall.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceBall.MaxDropDownItems = 20;
		this.comboReplaceBall.Name = "comboReplaceBall";
		this.comboReplaceBall.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceBall.TabIndex = 12;
		this.comboReplaceBall.Visible = false;
		this.comboReplaceBall.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceReferee.FormattingEnabled = true;
		this.comboReplaceReferee.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceReferee.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceReferee.MaxDropDownItems = 20;
		this.comboReplaceReferee.Name = "comboReplaceReferee";
		this.comboReplaceReferee.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceReferee.TabIndex = 11;
		this.comboReplaceReferee.Visible = false;
		this.comboReplaceReferee.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceSponsor.FormattingEnabled = true;
		this.comboReplaceSponsor.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceSponsor.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceSponsor.MaxDropDownItems = 20;
		this.comboReplaceSponsor.Name = "comboReplaceSponsor";
		this.comboReplaceSponsor.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceSponsor.TabIndex = 10;
		this.comboReplaceSponsor.Visible = false;
		this.comboReplaceSponsor.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceFormation.FormattingEnabled = true;
		this.comboReplaceFormation.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceFormation.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceFormation.MaxDropDownItems = 20;
		this.comboReplaceFormation.Name = "comboReplaceFormation";
		this.comboReplaceFormation.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceFormation.TabIndex = 9;
		this.comboReplaceFormation.Visible = false;
		this.comboReplaceFormation.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceTournament.FormattingEnabled = true;
		this.comboReplaceTournament.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceTournament.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceTournament.MaxDropDownItems = 20;
		this.comboReplaceTournament.Name = "comboReplaceTournament";
		this.comboReplaceTournament.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceTournament.TabIndex = 8;
		this.comboReplaceTournament.Visible = false;
		this.comboReplaceTournament.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceStadium.FormattingEnabled = true;
		this.comboReplaceStadium.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceStadium.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceStadium.MaxDropDownItems = 20;
		this.comboReplaceStadium.Name = "comboReplaceStadium";
		this.comboReplaceStadium.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceStadium.TabIndex = 7;
		this.comboReplaceStadium.Visible = false;
		this.comboReplaceStadium.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceCountry.FormattingEnabled = true;
		this.comboReplaceCountry.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceCountry.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceCountry.MaxDropDownItems = 20;
		this.comboReplaceCountry.Name = "comboReplaceCountry";
		this.comboReplaceCountry.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceCountry.TabIndex = 6;
		this.comboReplaceCountry.Visible = false;
		this.comboReplaceCountry.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceLeague.FormattingEnabled = true;
		this.comboReplaceLeague.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceLeague.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceLeague.Name = "comboReplaceLeague";
		this.comboReplaceLeague.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceLeague.TabIndex = 5;
		this.comboReplaceLeague.Visible = false;
		this.comboReplaceLeague.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplacePlayer.FormattingEnabled = true;
		this.comboReplacePlayer.Location = new System.Drawing.Point(108, 48);
		this.comboReplacePlayer.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplacePlayer.MaxDropDownItems = 20;
		this.comboReplacePlayer.Name = "comboReplacePlayer";
		this.comboReplacePlayer.Size = new System.Drawing.Size(236, 24);
		this.comboReplacePlayer.TabIndex = 4;
		this.comboReplacePlayer.Visible = false;
		this.comboReplacePlayer.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceTeam.FormattingEnabled = true;
		this.comboReplaceTeam.Location = new System.Drawing.Point(108, 48);
		this.comboReplaceTeam.Margin = new System.Windows.Forms.Padding(4);
		this.comboReplaceTeam.MaxDropDownItems = 20;
		this.comboReplaceTeam.Name = "comboReplaceTeam";
		this.comboReplaceTeam.Size = new System.Drawing.Size(236, 24);
		this.comboReplaceTeam.TabIndex = 3;
		this.comboReplaceTeam.Visible = false;
		this.comboReplaceTeam.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.radioReplaceItem.AutoSize = true;
		this.radioReplaceItem.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioReplaceItem.Location = new System.Drawing.Point(13, 48);
		this.radioReplaceItem.Margin = new System.Windows.Forms.Padding(4);
		this.radioReplaceItem.Name = "radioReplaceItem";
		this.radioReplaceItem.Size = new System.Drawing.Size(81, 21);
		this.radioReplaceItem.TabIndex = 1;
		this.radioReplaceItem.TabStop = true;
		this.radioReplaceItem.Text = "Replace";
		this.radioReplaceItem.UseVisualStyleBackColor = true;
		this.radioReplaceItem.CheckedChanged += new System.EventHandler(radioReplaceItem_CheckedChanged);
		this.radioCreateItem.AutoSize = true;
		this.radioCreateItem.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCreateItem.Location = new System.Drawing.Point(13, 20);
		this.radioCreateItem.Margin = new System.Windows.Forms.Padding(4);
		this.radioCreateItem.Name = "radioCreateItem";
		this.radioCreateItem.Size = new System.Drawing.Size(71, 21);
		this.radioCreateItem.TabIndex = 0;
		this.radioCreateItem.TabStop = true;
		this.radioCreateItem.Text = "Create";
		this.radioCreateItem.UseVisualStyleBackColor = true;
		this.radioCreateItem.CheckedChanged += new System.EventHandler(radioCreateItem_CheckedChanged);
		this.labelDetails.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelDetails.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDetails.Location = new System.Drawing.Point(0, 0);
		this.labelDetails.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelDetails.Name = "labelDetails";
		this.labelDetails.Size = new System.Drawing.Size(514, 18);
		this.labelDetails.TabIndex = 41;
		this.labelDetails.Text = "Details";
		this.labelDetails.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1543, 918);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.panelLeft);
		base.Controls.Add(this.toolMain);
		base.Controls.Add(this.mainMenu);
		base.Controls.Add(this.statusBar);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MainMenuStrip = this.mainMenu;
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "PatchLoaderForm";
		this.Text = " CM-Patch Loader";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(PatchLoaderForm_FormClosing);
		base.Load += new System.EventHandler(PatchLoaderForm_Load);
		this.mainMenu.ResumeLayout(false);
		this.mainMenu.PerformLayout();
		this.toolMain.ResumeLayout(false);
		this.toolMain.PerformLayout();
		this.panelLeft.ResumeLayout(false);
		this.panelLeft.PerformLayout();
		this.groupPatchOptions.ResumeLayout(false);
		this.tabPatchOptions.ResumeLayout(false);
		this.pagePlayerOptions.ResumeLayout(false);
		this.pagePlayerOptions.PerformLayout();
		this.groupDualClub.ResumeLayout(false);
		this.groupDualClub.PerformLayout();
		this.pageTeamOptions.ResumeLayout(false);
		this.pageTeamOptions.PerformLayout();
		this.pageLeagueOptions.ResumeLayout(false);
		this.pageLeagueOptions.PerformLayout();
		this.pageStadiumOptions.ResumeLayout(false);
		this.pageStadiumOptions.PerformLayout();
		this.pageKitOptions.ResumeLayout(false);
		this.pageKitOptions.PerformLayout();
		this.pageCountryOptions.ResumeLayout(false);
		this.pageCountryOptions.PerformLayout();
		this.statusBar.ResumeLayout(false);
		this.statusBar.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.panelRight.ResumeLayout(false);
		this.tabPreview.ResumeLayout(false);
		this.pageViewer2D.ResumeLayout(false);
		this.pageMultiViewer2D.ResumeLayout(false);
		this.panelGraphicGroups.ResumeLayout(false);
		this.groupTeam.ResumeLayout(false);
		this.groupTeam.PerformLayout();
		this.groupLeague.ResumeLayout(false);
		this.groupLeague.PerformLayout();
		this.groupStadium.ResumeLayout(false);
		this.groupStadium.PerformLayout();
		this.groupTod.ResumeLayout(false);
		this.groupTod.PerformLayout();
		this.groupShoes.ResumeLayout(false);
		this.groupShoes.PerformLayout();
		this.groupBall.ResumeLayout(false);
		this.groupBall.PerformLayout();
		this.groupCountry.ResumeLayout(false);
		this.groupCountry.PerformLayout();
		this.groupAdboards.ResumeLayout(false);
		this.groupAdboards.PerformLayout();
		this.groupKit.ResumeLayout(false);
		this.groupKit.PerformLayout();
		this.groupPlayer.ResumeLayout(false);
		this.groupPlayer.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		this.groupReplaceSelection.ResumeLayout(false);
		this.groupReplaceSelection.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
