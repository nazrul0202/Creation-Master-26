using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class AudioForm : Form
{
	private enum SearchMode
	{
		SearchExact,
		SearchStarting,
		SearchContaining,
		SearchEnding
	}

	private bool m_IsLoaded;

	public Player m_CurrentPlayer;

	private KeyValuePair<int, string> m_SelectedDictionaryName;

	private int m_CurrentDictionaryKey = 900000;

	private string m_CurrentDictionaryName;

	private bool m_HasToSave;

	private SearchMode m_SearchMode = SearchMode.SearchContaining;

	private int m_CurrentSearchIndexNameDirectory;

	private int m_CurrentSearchIndexExploreSounds;

	private int m_CurrentSearchIndexPatchSounds;

	private Audio m_ExploreAudio;

	private int[] m_ExplorePlayerIds;

	private int[] m_ExploreSurnamesIds;

	private Audio m_PatchAudio;

	private int[] m_PatchSurnamesIds;

	private string m_LastExportingFolder = FifaEnvironment.ExportFolder;

	private string m_LastImportingFolder = FifaEnvironment.ExportFolder;

	private IContainer components;

	public PickUpControl pickUpControl;

	private GroupBox groupAudio;

	private GroupBox groupNameDictionary;

	private ListView listViewNameDictionary;

	private ColumnHeader columnNameId;

	private ColumnHeader columnSurname;

	private NumericUpDown numericNameDictionary;

	private ToolStrip toolStripSearchnameDictionary;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripButton buttonFindNameExact;

	private ToolStripButton buttonFindNameStart;

	private ToolStripButton buttonFindNameAny;

	private ToolStrip toolStripNameDictionary;

	private ToolStripSeparator toolStripSeparator7;

	public ToolStripButton buttonAddName;

	public ToolStripButton buttonReplaceName;

	public ToolStripButton buttonRemoveName;

	private TextBox textKnownAs;

	private Label label13;

	private ToolStripTextBox textNameDictionary;

	private Button buttonSearchSurnameId;

	private ToolStripTextBox textSearchNameDictionary;

	private ToolStripLabel toolStripLabel1;

	private Label label1;

	private TextBox textPlayerId;

	private Button buttonSearchPlayerId;

	private ToolTip toolTip;

	private TextBox textSurnameSoundId;

	private Button buttonSetSound;

	private Button buttonDeleteSoundAssociation;

	private GroupBox groupPlayerInfo;

	private Label labelCommonName;

	private TextBox textCommonName;

	private Viewer2D viewer2DPhoto;

	private TextBox textSurname;

	private TextBox textFirstName;

	private Label labelFirstName;

	private Label labelSurame;

	private TextBox textAudioName;

	private Label label3;

	private Label label2;

	private GroupBox groupExploreAudio;

	private ListView listViewExploreSounds;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private ToolStrip toolStripExploreExistingSounds;

	private ToolStripButton buttonCloseSoundFile;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonSelectAllSounds;

	private ToolStripButton buttonDeselectAllSounds;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton buttonExportSps2;

	private ToolStripComboBox comboSelctSoundGroup;

	private ToolStripSplitButton buttonOpenSoundFile;

	private ToolStripMenuItem italianToolStripMenuItem;

	private ToolStripMenuItem itaBankToolStripMenuItem;

	private ToolStripMenuItem neutralToolStripMenuItem;

	private ToolStripMenuItem femaleToolStripMenuItem;

	private ToolStripMenuItem demoToolStripMenuItem;

	private ToolStripMenuItem demoNeutralToolStripMenuItem;

	private ToolStripMenuItem demoFemaleToolStripMenuItem;

	private ToolStripMenuItem spanishToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem1;

	private ToolStripMenuItem toolStripMenuItem2;

	private ToolStripMenuItem toolStripMenuItem3;

	private ToolStripMenuItem toolStripMenuItem4;

	private ToolStripMenuItem toolStripMenuItem5;

	private ToolStripMenuItem toolStripMenuItem6;

	private ToolStripMenuItem mexicoToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem7;

	private ToolStripMenuItem toolStripMenuItem8;

	private ToolStripMenuItem toolStripMenuItem9;

	private ToolStripMenuItem toolStripMenuItem10;

	private ToolStripMenuItem toolStripMenuItem11;

	private ToolStripMenuItem toolStripMenuItem12;

	private ToolStripMenuItem brazilToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem13;

	private ToolStripMenuItem toolStripMenuItem14;

	private ToolStripMenuItem toolStripMenuItem15;

	private ToolStripMenuItem toolStripMenuItem16;

	private ToolStripMenuItem toolStripMenuItem17;

	private ToolStripMenuItem toolStripMenuItem18;

	private ToolStripMenuItem deutchToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem19;

	private ToolStripMenuItem toolStripMenuItem20;

	private ToolStripMenuItem toolStripMenuItem21;

	private ToolStripMenuItem toolStripMenuItem22;

	private ToolStripMenuItem toolStripMenuItem23;

	private ToolStripMenuItem toolStripMenuItem24;

	private ToolStripMenuItem russianToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem25;

	private ToolStripMenuItem toolStripMenuItem26;

	private ToolStripMenuItem toolStripMenuItem27;

	private ToolStripMenuItem toolStripMenuItem28;

	private ToolStripMenuItem toolStripMenuItem29;

	private ToolStripMenuItem toolStripMenuItem30;

	private ToolStripMenuItem frenchToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem31;

	private ToolStripMenuItem toolStripMenuItem32;

	private ToolStripMenuItem toolStripMenuItem33;

	private ToolStripMenuItem toolStripMenuItem34;

	private ToolStripMenuItem toolStripMenuItem35;

	private ToolStripMenuItem toolStripMenuItem36;

	private ToolStripMenuItem englishToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem37;

	private ToolStripMenuItem toolStripMenuItem38;

	private ToolStripMenuItem toolStripMenuItem39;

	private ToolStripMenuItem toolStripMenuItem40;

	private ToolStripMenuItem toolStripMenuItem41;

	private ToolStripMenuItem toolStripMenuItem42;

	private ToolStripMenuItem toolStripMenuItem43;

	private ToolStripMenuItem toolStripMenuItem44;

	private ToolStripMenuItem toolStripMenuItem45;

	private ToolStripMenuItem toolStripMenuItem47;

	private ToolStripMenuItem toolStripMenuItem46;

	private ToolStripMenuItem toolStripMenuItem48;

	private FolderBrowserDialog folderBrowserExportSounds;

	private ToolStrip toolStripSearchSound;

	private ToolStripLabel toolStripLabel2;

	private ToolStripTextBox textSearchExplore;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripButton buttonSearchExploreSoundExact;

	private ToolStripButton buttonSearchExploreSoundStarting;

	private ToolStripButton buttonSearchExploreSoundContaining;

	private GroupBox groupPatchAudio;

	private ToolStrip toolStrip2;

	private ToolStripSplitButton buttonOpenPatchSound;

	private ToolStripMenuItem toolStripMenuItem49;

	private ToolStripMenuItem toolStripMenuItem50;

	private ToolStripMenuItem toolStripMenuItem56;

	private ToolStripMenuItem toolStripMenuItem57;

	private ToolStripMenuItem toolStripMenuItem58;

	private ToolStripMenuItem toolStripMenuItem59;

	private ToolStripMenuItem toolStripMenuItem60;

	private ToolStripMenuItem toolStripMenuItem61;

	private ToolStripMenuItem toolStripMenuItem62;

	private ToolStripMenuItem toolStripMenuItem63;

	private ToolStripMenuItem toolStripMenuItem64;

	private ToolStripMenuItem toolStripMenuItem65;

	private ToolStripMenuItem toolStripMenuItem66;

	private ToolStripMenuItem toolStripMenuItem67;

	private ToolStripMenuItem toolStripMenuItem68;

	private ToolStripMenuItem toolStripMenuItem69;

	private ToolStripMenuItem toolStripMenuItem70;

	private ToolStripMenuItem toolStripMenuItem71;

	private ToolStripMenuItem toolStripMenuItem72;

	private ToolStripMenuItem toolStripMenuItem73;

	private ToolStripMenuItem toolStripMenuItem74;

	private ToolStripMenuItem toolStripMenuItem75;

	private ToolStripMenuItem toolStripMenuItem76;

	private ToolStripMenuItem toolStripMenuItem77;

	private ToolStripMenuItem toolStripMenuItem78;

	private ToolStripMenuItem toolStripMenuItem79;

	private ToolStripMenuItem toolStripMenuItem80;

	private ToolStripMenuItem toolStripMenuItem81;

	private ToolStripMenuItem toolStripMenuItem82;

	private ToolStripMenuItem toolStripMenuItem83;

	private ToolStripMenuItem toolStripMenuItem84;

	private ToolStripMenuItem toolStripMenuItem85;

	private ToolStripMenuItem toolStripMenuItem86;

	private ToolStripMenuItem toolStripMenuItem87;

	private ToolStripMenuItem toolStripMenuItem88;

	private ToolStripMenuItem toolStripMenuItem89;

	private ToolStripMenuItem toolStripMenuItem90;

	private ToolStripMenuItem toolStripMenuItem91;

	private ToolStripMenuItem toolStripMenuItem92;

	private ToolStripMenuItem toolStripMenuItem93;

	private ToolStripMenuItem toolStripMenuItem94;

	private ToolStripMenuItem toolStripMenuItem95;

	private ToolStripMenuItem toolStripMenuItem96;

	private ToolStripMenuItem toolStripMenuItem97;

	private ToolStripMenuItem toolStripMenuItem98;

	private ToolStripMenuItem toolStripMenuItem99;

	private ToolStripMenuItem toolStripMenuItem100;

	private ToolStripMenuItem toolStripMenuItem101;

	private ToolStripMenuItem toolStripMenuItem102;

	private ToolStripMenuItem toolStripMenuItem103;

	private ToolStripMenuItem toolStripMenuItem104;

	private ToolStripMenuItem toolStripMenuItem105;

	private ToolStripMenuItem toolStripMenuItem106;

	private ToolStripMenuItem toolStripMenuItem107;

	private ToolStripMenuItem toolStripMenuItem108;

	private ToolStripMenuItem toolStripMenuItem109;

	private ToolStripMenuItem toolStripMenuItem110;

	private ToolStripButton buttonClosePatchedAudio;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStrip toolStrip1;

	private ToolStripLabel toolStripLabel3;

	private ToolStripTextBox textSearchPatch;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripButton buttonSearchPatchSoundExact;

	private ToolStripButton buttonSearchPatchSoundStarting;

	private ToolStripButton buttonSearchPatchSoundContaining;

	private ToolStripButton buttonSavePatchedAudio;

	private ToolStripButton buttonDeleteSps;

	private ListView listViewPatchSounds;

	private ColumnHeader columnHeader3;

	private ToolStripButton buttonImportSps;

	private OpenFileDialog openImportSound;

	private GroupBox groupSoundEditing;

	private ToolStrip toolStrip4;

	private ToolStripButton buttonOpenSound1;

	private ToolStripButton buttonOpenSound2;

	private ToolStripButton buttonSaveEditedSound;

	private Label labelSound2;

	private Label labelSound1;

	private SaveFileDialog saveFileDialog1;

	private ColumnHeader columnHeader4;

	private ListView listViewSound2;

	private ColumnHeader columnHeader9;

	private ListView listViewSound1;

	private ColumnHeader columnHeader6;

	private ToolStripButton buttonPlaySound;

	private ToolStripButton buttonExportWav;

	private ToolStripButton buttonPlaySound2;

	private ToolStripButton buttonExportWav2;

	private ToolStripButton buttonExportSps;

	private ToolStripButton buttonImportWav;

	private ToolStripSeparator toolStripSeparator8;

	private ToolStripSeparator toolStripSeparator9;

	private ToolStripMenuItem iTANeutralToolStripMenuItem;

	private ToolStripButton buttonSearchPatchSoundEnding;

	private ToolStripMenuItem arabicToolStripMenuItem;

	private ToolStripMenuItem aRAArasabankToolStripMenuItem;

	private ToolStripMenuItem aRANeutralToolStripMenuItem;

	private ToolStripMenuItem arabicToolStripMenuItem1;

	private ToolStripMenuItem menuAraNeutral;

	private ToolStripMenuItem aRADemoToolStripMenuItem;

	public AudioForm()
	{
		InitializeComponent();
		pickUpControl.SelectObject = SelectPlayerAudio;
		viewer2DPhoto.ButtonStripVisible = false;
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		IdArrayList[] filterValues = new IdArrayList[5]
		{
			null,
			FifaEnvironment.Teams,
			FifaEnvironment.Countries,
			FifaEnvironment.FreeAgents,
			null
		};
		pickUpControl.FilterValues = filterValues;
		pickUpControl.ObjectList = FifaEnvironment.Players;
	}

	private void AudioForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
		ShowNameDictionary();
	}

	public void LoadPlayerAudio(Player player)
	{
		if (!m_IsLoaded)
		{
			return;
		}
		m_CurrentPlayer = player;
		textKnownAs.Text = m_CurrentPlayer.audioname;
		textPlayerId.Text = m_CurrentPlayer.Id.ToString();
		string value = null;
		FifaEnvironment.NameDictionary.TryGetValue(m_CurrentPlayer.Id, out value);
		if (value != null)
		{
			textSurnameSoundId.Text = m_CurrentPlayer.Id.ToString();
		}
		else if (value == null)
		{
			FifaEnvironment.NameDictionary.TryGetValue(m_CurrentPlayer.commentaryid, out value);
			if (value != null)
			{
				textSurnameSoundId.Text = m_CurrentPlayer.commentaryid.ToString();
			}
			else
			{
				textSurnameSoundId.Text = "Not Assigned";
			}
		}
		textAudioName.Text = value;
		viewer2DPhoto.CurrentBitmap = m_CurrentPlayer.GetPhoto();
		textFirstName.Text = m_CurrentPlayer.firstname;
		textSurname.Text = m_CurrentPlayer.lastname;
		textCommonName.Text = m_CurrentPlayer.commonname;
		EnableDictionaryButtons();
	}

	public void ShowNameDictionary()
	{
		if (!m_IsLoaded)
		{
			return;
		}
		listViewNameDictionary.BeginUpdate();
		listViewNameDictionary.Items.Clear();
		foreach (KeyValuePair<int, string> item in FifaEnvironment.NameDictionary)
		{
			int key = item.Key;
			string value = item.Value;
			ListViewItem listViewItem = new ListViewItem(key.ToString().PadLeft(6));
			listViewItem.SubItems.Add(value);
			listViewItem.Tag = item;
			listViewNameDictionary.Items.Add(listViewItem);
		}
		listViewNameDictionary.EndUpdate();
	}

	private Player SelectPlayerAudio(object sender, object obj)
	{
		Player player = (Player)obj;
		LoadPlayerAudio(player);
		return player;
	}

	private void listViewNameDictionary_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		ListView obj = (ListView)sender;
		obj.ListViewItemSorter = new ListViewItemComparer(sortOrder: obj.Sorting = ((obj.Sorting != SortOrder.Ascending) ? SortOrder.Ascending : SortOrder.Descending), column: e.Column);
		KeepSelectedNameVisible();
	}

	private void listViewNameDictionary_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listViewNameDictionary.SelectedItems.Count > 0)
		{
			ShowSelectedNameDictionary();
			EnableDictionaryButtons();
		}
	}

	private void numericNameDictionary_ValueChanged(object sender, EventArgs e)
	{
		if (m_CurrentDictionaryKey != (int)numericNameDictionary.Value)
		{
			m_CurrentDictionaryKey = (int)numericNameDictionary.Value;
			if (!SelectNameDictionaryItem(m_CurrentDictionaryKey))
			{
				textNameDictionary.Text = "<Unknown>";
			}
			else
			{
				KeepSelectedNameVisible();
			}
			EnableDictionaryButtons();
		}
	}

	private void textNameDictionary_TextChanged(object sender, EventArgs e)
	{
		if (!(m_CurrentDictionaryName == textNameDictionary.Text))
		{
			m_CurrentDictionaryName = textNameDictionary.Text;
			EnableDictionaryButtons();
		}
	}

	private void EnableDictionaryButtons()
	{
		Button button = buttonSearchSurnameId;
		bool enabled = (buttonDeleteSoundAssociation.Enabled = m_CurrentPlayer.commentaryid > 900000);
		button.Enabled = enabled;
		if (m_CurrentDictionaryKey == 900000)
		{
			buttonReplaceName.Enabled = false;
			buttonRemoveName.Enabled = false;
			buttonAddName.Enabled = false;
			buttonSetSound.Enabled = false;
		}
		else if (FifaEnvironment.NameDictionary.ContainsKey(m_CurrentDictionaryKey))
		{
			buttonRemoveName.Enabled = true;
			buttonAddName.Enabled = false;
			if (FifaEnvironment.NameDictionary.TryGetValue(m_CurrentDictionaryKey, out var value))
			{
				if (value == m_CurrentDictionaryName)
				{
					buttonReplaceName.Enabled = false;
					buttonSetSound.Enabled = m_CurrentDictionaryKey > 900000;
				}
				else
				{
					buttonReplaceName.Enabled = true;
					buttonSetSound.Enabled = false;
				}
			}
		}
		else
		{
			buttonRemoveName.Enabled = false;
			buttonReplaceName.Enabled = false;
			buttonAddName.Enabled = true;
			buttonSetSound.Enabled = false;
		}
	}

	private bool SelectNameDictionaryItem(int commentaryid)
	{
		if (listViewNameDictionary.SelectedItems.Count > 0)
		{
			listViewNameDictionary.SelectedItems[0].Selected = false;
		}
		for (int i = 0; i < listViewNameDictionary.Items.Count; i++)
		{
			if (((KeyValuePair<int, string>)listViewNameDictionary.Items[i].Tag).Key == commentaryid)
			{
				listViewNameDictionary.Items[i].Selected = true;
				if (i > 8)
				{
					listViewNameDictionary.TopItem = listViewNameDictionary.Items[i - 8];
				}
				return true;
			}
		}
		return false;
	}

	private void KeepSelectedNameVisible()
	{
		if (listViewNameDictionary.SelectedItems.Count > 0)
		{
			int num = listViewNameDictionary.SelectedIndices[0];
			if (num > 8)
			{
				listViewNameDictionary.TopItem = listViewNameDictionary.Items[num - 8];
			}
		}
	}

	private void ShowSelectedNameDictionary()
	{
		if (listViewNameDictionary.SelectedItems.Count > 0)
		{
			m_SelectedDictionaryName = (KeyValuePair<int, string>)listViewNameDictionary.SelectedItems[0].Tag;
			m_CurrentDictionaryKey = m_SelectedDictionaryName.Key;
			m_CurrentDictionaryName = m_SelectedDictionaryName.Value;
			numericNameDictionary.Value = m_CurrentDictionaryKey;
			textNameDictionary.Text = m_CurrentDictionaryName;
		}
	}

	private void buttonFindNameExact_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchExact;
		SearchNameDirectory();
	}

	private void buttonFindNameStart_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchStarting;
		SearchNameDirectory();
	}

	private void buttonFindNameAny_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchContaining;
		SearchNameDirectory();
	}

	public bool SearchNameDirectory()
	{
		bool flag = false;
		if (textSearchNameDictionary.Text == null || textSearchNameDictionary.Text == string.Empty)
		{
			return false;
		}
		string text = textSearchNameDictionary.Text;
		text = text.ToLower();
		int currentSearchIndexNameDirectory = m_CurrentSearchIndexNameDirectory;
		currentSearchIndexNameDirectory++;
		if (currentSearchIndexNameDirectory >= listViewNameDictionary.Items.Count)
		{
			currentSearchIndexNameDirectory = 0;
		}
		while (true)
		{
			string text2 = null;
			KeyValuePair<int, string> keyValuePair = (KeyValuePair<int, string>)listViewNameDictionary.Items[currentSearchIndexNameDirectory].Tag;
			text2 = keyValuePair.Value;
			text2 = text2.ToLower();
			switch (m_SearchMode)
			{
			case SearchMode.SearchExact:
				flag = text2.ToString().Equals(text);
				break;
			case SearchMode.SearchStarting:
				flag = text2.ToString().StartsWith(text);
				break;
			case SearchMode.SearchContaining:
				flag = text2.Contains(text);
				break;
			}
			if (flag)
			{
				m_CurrentSearchIndexNameDirectory = currentSearchIndexNameDirectory;
				numericNameDictionary.Value = keyValuePair.Key;
				return true;
			}
			if (currentSearchIndexNameDirectory == m_CurrentSearchIndexNameDirectory)
			{
				break;
			}
			currentSearchIndexNameDirectory++;
			if (currentSearchIndexNameDirectory == listViewNameDictionary.Items.Count)
			{
				currentSearchIndexNameDirectory = 0;
			}
		}
		return false;
	}

	private void buttonAddName_Click(object sender, EventArgs e)
	{
		FifaEnvironment.NameDictionary.Add(m_CurrentDictionaryKey, m_CurrentDictionaryName);
		ListViewItem listViewItem = new ListViewItem(m_CurrentDictionaryKey.ToString().PadLeft(6));
		listViewItem.SubItems.Add(m_CurrentDictionaryName);
		KeyValuePair<int, string> keyValuePair = new KeyValuePair<int, string>(m_CurrentDictionaryKey, m_CurrentDictionaryName);
		listViewItem.Tag = keyValuePair;
		listViewNameDictionary.Items.Add(listViewItem);
		SelectNameDictionaryItem(m_CurrentDictionaryKey);
		EnableDictionaryButtons();
	}

	private void buttonRemoveName_Click(object sender, EventArgs e)
	{
		FifaEnvironment.NameDictionary.Remove(m_CurrentDictionaryKey);
		for (int i = 0; i < listViewNameDictionary.Items.Count; i++)
		{
			if (((KeyValuePair<int, string>)listViewNameDictionary.Items[i].Tag).Key == m_CurrentDictionaryKey)
			{
				listViewNameDictionary.Items.RemoveAt(i);
				EnableDictionaryButtons();
				break;
			}
		}
	}

	private void buttonReplaceName_Click(object sender, EventArgs e)
	{
		FifaEnvironment.NameDictionary.Remove(m_CurrentDictionaryKey);
		FifaEnvironment.NameDictionary.Add(m_CurrentDictionaryKey, m_CurrentDictionaryName);
		for (int i = 0; i < listViewNameDictionary.Items.Count; i++)
		{
			if (((KeyValuePair<int, string>)listViewNameDictionary.Items[i].Tag).Key == m_CurrentDictionaryKey)
			{
				listViewNameDictionary.Items[i].SubItems[1].Text = m_CurrentDictionaryName;
				EnableDictionaryButtons();
				break;
			}
		}
	}

	private void buttonSearchPlayerId_Click(object sender, EventArgs e)
	{
		numericNameDictionary.Value = m_CurrentPlayer.Id;
	}

	private void buttonSearchSurnameId_Click(object sender, EventArgs e)
	{
		if (m_CurrentPlayer.commentaryid != 900000)
		{
			numericNameDictionary.Value = m_CurrentPlayer.commentaryid;
		}
	}

	private void buttonDeleteSound_Click(object sender, EventArgs e)
	{
		PlayerName playerName = FifaEnvironment.PlayerNamesList.SearchName(m_CurrentPlayer.audioname);
		if (playerName != null)
		{
			playerName.CommentaryId = 900000;
			LoadPlayerAudio(m_CurrentPlayer);
		}
	}

	private void buttonSetSound_Click(object sender, EventArgs e)
	{
		PlayerName playerName = FifaEnvironment.PlayerNamesList.SearchName(m_CurrentPlayer.audioname);
		if (playerName != null)
		{
			playerName.CommentaryId = m_CurrentDictionaryKey;
			LoadPlayerAudio(m_CurrentPlayer);
		}
	}

	private void EnableExploreButtons()
	{
		bool flag = m_ExploreAudio != null;
		buttonOpenSoundFile.Enabled = !flag;
		buttonCloseSoundFile.Enabled = flag;
		buttonSelectAllSounds.Enabled = flag;
		buttonDeselectAllSounds.Enabled = flag;
		buttonExportSps2.Enabled = flag;
		comboSelctSoundGroup.Enabled = flag;
	}

	private bool LoadExploreAudio()
	{
		bool flag = m_ExploreAudio.OpenForReading();
		if (!flag)
		{
			return false;
		}
		ShowNamesOnExplorePanel();
		return flag;
	}

	private bool LoadPatchAudio()
	{
		bool flag = m_PatchAudio.OpenForEditing();
		if (!flag)
		{
			return false;
		}
		ShowGenericNamesOnPatchPanel();
		return flag;
	}

	private void ShowNamesOnExplorePanel()
	{
		if (comboSelctSoundGroup.SelectedIndex >= 0)
		{
			if (comboSelctSoundGroup.SelectedIndex == 0)
			{
				ShowSpecificNamesOnExplorePanel();
			}
			else
			{
				ShowGenericNamesOnExplorePanel();
			}
		}
	}

	private void ShowSpecificNamesOnExplorePanel()
	{
		if (!m_IsLoaded)
		{
			return;
		}
		listViewExploreSounds.Items.Clear();
		if (m_ExploreAudio.SbrFile.PlayerNamesGroup == null)
		{
			return;
		}
		NameSoundList nameSoundList = m_ExploreAudio.SbrFile.PlayerNamesGroup.NameSoundList;
		listViewExploreSounds.BeginUpdate();
		listViewExploreSounds.Items.Clear();
		foreach (NameSound item in nameSoundList)
		{
			ListViewItem listViewItem = new ListViewItem(item.Id.ToString());
			listViewItem.SubItems.Add(item.Text);
			listViewItem.Tag = item;
			listViewExploreSounds.Items.Add(listViewItem);
		}
		listViewExploreSounds.EndUpdate();
	}

	private void ShowGenericNamesOnExplorePanel()
	{
		if (!m_IsLoaded)
		{
			return;
		}
		listViewExploreSounds.Items.Clear();
		if (m_ExploreAudio.SbrFile.SimpleSurnamesGroup == null)
		{
			return;
		}
		NameSoundList nameSoundList = m_ExploreAudio.SbrFile.SimpleSurnamesGroup.NameSoundList;
		listViewExploreSounds.BeginUpdate();
		listViewExploreSounds.Items.Clear();
		foreach (NameSound item in nameSoundList)
		{
			ListViewItem listViewItem = new ListViewItem(item.Id.ToString());
			listViewItem.SubItems.Add(item.Text);
			listViewItem.Tag = item;
			listViewExploreSounds.Items.Add(listViewItem);
		}
		listViewExploreSounds.EndUpdate();
	}

	private void ShowGenericNamesOnPatchPanel()
	{
		if (!m_IsLoaded)
		{
			return;
		}
		listViewPatchSounds.Items.Clear();
		if (m_PatchAudio.SbrFile.SimpleSurnamesGroup == null)
		{
			return;
		}
		NameSoundList nameSoundList = m_PatchAudio.SbrFile.SimpleSurnamesGroup.NameSoundList;
		listViewPatchSounds.BeginUpdate();
		listViewPatchSounds.Items.Clear();
		foreach (NameSound item in nameSoundList)
		{
			ListViewItem listViewItem = new ListViewItem(item.Id.ToString());
			listViewItem.SubItems.Add(item.Text);
			listViewItem.Tag = item;
			listViewPatchSounds.Items.Add(listViewItem);
		}
		listViewPatchSounds.EndUpdate();
	}

	private void buttonCloseSoundFile_Click(object sender, EventArgs e)
	{
		m_ExploreAudio = null;
		listViewExploreSounds.Items.Clear();
		SetExploreButtonsState();
		groupExploreAudio.Text = "Exploring:";
	}

	private void comboSelctSoundGroup_SelectedIndexChanged(object sender, EventArgs e)
	{
		ShowNamesOnExplorePanel();
	}

	private void buttonSelectAllSounds_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewExploreSounds.Items)
		{
			item.Checked = true;
		}
	}

	private void buttonDeselectAllSounds_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewExploreSounds.Items)
		{
			item.Checked = false;
		}
	}

	private void buttonSearchSoundExact_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchExact;
		SearchExploreSoundByName();
	}

	private void buttonSearchSoundStarting_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchStarting;
		SearchExploreSoundByName();
	}

	private void buttonSearchSoundContaining_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchContaining;
		SearchExploreSoundByName();
	}

	public bool SearchExploreSoundByName()
	{
		bool flag = false;
		if (textSearchExplore.Text == null || textSearchExplore.Text == string.Empty)
		{
			return false;
		}
		string text = textSearchExplore.Text;
		text = text.ToLower();
		int currentSearchIndexExploreSounds = m_CurrentSearchIndexExploreSounds;
		currentSearchIndexExploreSounds++;
		if (currentSearchIndexExploreSounds >= listViewExploreSounds.Items.Count)
		{
			currentSearchIndexExploreSounds = 0;
			m_CurrentSearchIndexExploreSounds = 0;
		}
		while (true)
		{
			if (listViewExploreSounds.Items.Count == 0)
			{
				return false;
			}
			string text2 = null;
			text2 = listViewExploreSounds.Items[currentSearchIndexExploreSounds].SubItems[1].Text;
			text2 = text2.ToLower();
			switch (m_SearchMode)
			{
			case SearchMode.SearchExact:
				flag = text2.ToString().Equals(text);
				break;
			case SearchMode.SearchStarting:
				flag = text2.ToString().StartsWith(text);
				break;
			case SearchMode.SearchContaining:
				flag = text2.ToString().Contains(text);
				break;
			}
			if (flag)
			{
				m_CurrentSearchIndexExploreSounds = currentSearchIndexExploreSounds;
				SelectListViewItem(listViewExploreSounds, m_CurrentSearchIndexExploreSounds);
				return true;
			}
			if (currentSearchIndexExploreSounds == m_CurrentSearchIndexExploreSounds)
			{
				break;
			}
			currentSearchIndexExploreSounds++;
			if (currentSearchIndexExploreSounds == listViewExploreSounds.Items.Count)
			{
				currentSearchIndexExploreSounds = 0;
			}
		}
		return false;
	}

	public bool SearchPatchSoundByName()
	{
		bool flag = false;
		if (textSearchPatch.Text == null || textSearchPatch.Text == string.Empty)
		{
			return false;
		}
		string text = textSearchPatch.Text;
		text = text.ToLower();
		int currentSearchIndexPatchSounds = m_CurrentSearchIndexPatchSounds;
		currentSearchIndexPatchSounds++;
		if (currentSearchIndexPatchSounds >= listViewPatchSounds.Items.Count)
		{
			currentSearchIndexPatchSounds = 0;
			m_CurrentSearchIndexPatchSounds = 0;
		}
		while (true)
		{
			string text2 = null;
			text2 = listViewPatchSounds.Items[currentSearchIndexPatchSounds].SubItems[1].Text;
			text2 = text2.ToLower();
			switch (m_SearchMode)
			{
			case SearchMode.SearchExact:
				flag = text2.ToString().Equals(text);
				break;
			case SearchMode.SearchStarting:
				flag = text2.ToString().StartsWith(text);
				break;
			case SearchMode.SearchContaining:
				flag = text2.ToString().Contains(text);
				break;
			case SearchMode.SearchEnding:
				flag = text2.ToString().EndsWith(text);
				break;
			}
			if (flag)
			{
				m_CurrentSearchIndexPatchSounds = currentSearchIndexPatchSounds;
				SelectListViewItem(listViewPatchSounds, m_CurrentSearchIndexPatchSounds);
				return true;
			}
			if (currentSearchIndexPatchSounds == m_CurrentSearchIndexPatchSounds)
			{
				break;
			}
			currentSearchIndexPatchSounds++;
			if (currentSearchIndexPatchSounds == listViewPatchSounds.Items.Count)
			{
				currentSearchIndexPatchSounds = 0;
			}
		}
		return false;
	}

	private bool SelectListViewItem(ListView listView, int itemIndex)
	{
		if (listView.SelectedItems.Count > 0)
		{
			listView.SelectedItems[0].Selected = false;
		}
		listView.Items[itemIndex].Selected = true;
		if (itemIndex > 8)
		{
			listView.TopItem = listView.Items[itemIndex - 8];
		}
		else
		{
			listView.TopItem = listView.Items[0];
		}
		return true;
	}

	private void bankToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
		string text = toolStripMenuItem.Text.Substring(0, 3);
		string text2 = toolStripMenuItem.Text.Substring(4).ToLower();
		m_ExploreAudio = new Audio();
		switch (text)
		{
		case "ITA":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/ita_it/ita_it.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/ita_it/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/ita_it/" + text2 + ".sbs";
			break;
		case "SPA":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/spa_es/spa_es.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/spa_es/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/spa_es/" + text2 + ".sbs";
			break;
		case "BRA":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/por_br/por_br.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/por_br/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/por_br/" + text2 + ".sbs";
			break;
		case "MEX":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/spa_mx/spa_mx.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/spa_mx/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/spa_mx/" + text2 + ".sbs";
			break;
		case "FRA":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/fre_fr/fre_fr.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/fre_fr/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/fre_fr/" + text2 + ".sbs";
			break;
		case "GER":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/ger_de/ger_de.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/ger_de/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/ger_de/" + text2 + ".sbs";
			break;
		case "EN1":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/eng_us/eng_us.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/eng_us/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/eng_us/" + text2 + ".sbs";
			break;
		case "EN2":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/eng_us_2/eng_us_2.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/eng_us_2/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/eng_us_2/" + text2 + ".sbs";
			break;
		case "RUS":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/rus_ru/rus_ru.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/rus_ru/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/rus_ru/" + text2 + ".sbs";
			break;
		case "ARA":
			m_ExploreAudio.XmlFileName = "audiodata/speechdata/ara_sa/ara_sa.xml";
			m_ExploreAudio.SbrFileName = "audiodata/speechdata/ara_sa/" + text2 + ".sbr";
			m_ExploreAudio.SbsFileName = "audiodata/speechdata/ara_sa/" + text2 + ".sbs";
			break;
		default:
			m_ExploreAudio = null;
			return;
		}
		LoadExploreAudio();
		SetExploreButtonsState();
		groupExploreAudio.Text = "Exploring:" + toolStripMenuItem.ToString();
	}

	private void bankOpenForPatch(object sender, EventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
		string text = toolStripMenuItem.Text.Substring(0, 3);
		string text2 = toolStripMenuItem.Text.Substring(4).ToLower();
		m_PatchAudio = new Audio();
		switch (text)
		{
		case "ITA":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/ita_it/ita_it.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/ita_it/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/ita_it/" + text2 + ".sbs";
			break;
		case "SPA":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/spa_es/spa_es.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/spa_es/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/spa_es/" + text2 + ".sbs";
			break;
		case "BRA":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/por_br/por_br.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/por_br/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/por_br/" + text2 + ".sbs";
			break;
		case "MEX":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/spa_mx/spa_mx.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/spa_mx/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/spa_mx/" + text2 + ".sbs";
			break;
		case "FRA":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/fre_fr/fre_fr.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/fre_fr/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/fre_fr/" + text2 + ".sbs";
			break;
		case "GER":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/ger_de/ger_de.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/ger_de/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/ger_de/" + text2 + ".sbs";
			break;
		case "EN1":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/eng_us/eng_us.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/eng_us/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/eng_us/" + text2 + ".sbs";
			break;
		case "EN2":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/eng_us_2/eng_us_2.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/eng_us_2/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/eng_us_2/" + text2 + ".sbs";
			break;
		case "RUS":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/rus_ru/rus_ru.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/rus_ru/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/rus_ru/" + text2 + ".sbs";
			break;
		case "ARA":
			m_PatchAudio.XmlFileName = "audiodata/speechdata/ara_sa/ara_sa.xml";
			m_PatchAudio.SbrFileName = "audiodata/speechdata/ara_sa/" + text2 + ".sbr";
			m_PatchAudio.SbsFileName = "audiodata/speechdata/ara_sa/" + text2 + ".sbs";
			break;
		default:
			m_PatchAudio = null;
			return;
		}
		LoadPatchAudio();
		SetPatchButtonsState();
		groupPatchAudio.Text = "Editing:" + toolStripMenuItem.ToString();
	}

	private void buttonSavePatchedAudio_Click(object sender, EventArgs e)
	{
		if (m_PatchAudio != null)
		{
			m_PatchAudio.Save();
			m_HasToSave = false;
			SetPatchButtonsState();
		}
	}

	private void buttonClosePatchedAudio_Click(object sender, EventArgs e)
	{
		m_PatchAudio = null;
		listViewPatchSounds.Items.Clear();
		SetPatchButtonsState();
		groupPatchAudio.Text = "Editing:";
	}

	private void buttonImportSps_Click(object sender, EventArgs e)
	{
		openImportSound.CheckFileExists = true;
		openImportSound.Multiselect = true;
		openImportSound.InitialDirectory = m_LastImportingFolder;
		openImportSound.Filter = "Sound files (*.sps)|*.sps";
		openImportSound.FilterIndex = 1;
		openImportSound.Title = "Select one or more Sound files";
		if (openImportSound.ShowDialog() == DialogResult.OK)
		{
			string[] fileNames = openImportSound.FileNames;
			if (fileNames.Length != 0)
			{
				m_LastImportingFolder = Path.GetDirectoryName(fileNames[0]);
				ImportSounds(fileNames);
				m_HasToSave = true;
				ShowGenericNamesOnPatchPanel();
				SetPatchButtonsState();
			}
		}
	}

	private void buttonImportWav_Click(object sender, EventArgs e)
	{
		openImportSound.CheckFileExists = true;
		openImportSound.Multiselect = true;
		openImportSound.InitialDirectory = m_LastImportingFolder;
		openImportSound.Filter = "Sound files (*.wav;*.mp3)|*.wav;*.mp3";
		openImportSound.FilterIndex = 1;
		openImportSound.Title = "Select one or more Wave files";
		if (openImportSound.ShowDialog() == DialogResult.OK)
		{
			string[] fileNames = openImportSound.FileNames;
			if (fileNames.Length != 0)
			{
				m_LastImportingFolder = Path.GetDirectoryName(fileNames[0]);
				ImportSounds(fileNames);
				m_HasToSave = true;
				ShowGenericNamesOnPatchPanel();
				SetPatchButtonsState();
			}
		}
	}

	private void ImportSounds(string[] fileNames)
	{
		Cursor.Current = Cursors.WaitCursor;
		for (int i = 0; i < fileNames.Length; i++)
		{
			ImportSound(fileNames[i]);
		}
		ShowNameDictionary();
		Cursor.Current = Cursors.Default;
	}

	private void ImportSound(string fileName)
	{
		string text = Path.GetExtension(fileName).ToLower();
		if (!File.Exists(fileName))
		{
			return;
		}
		string path;
		switch (text)
		{
		default:
			return;
		case ".wav":
		case ".mp3":
			path = Path.ChangeExtension(fileName, "sps");
			if (!EncodeSps(fileName))
			{
				return;
			}
			break;
		case ".sps":
			path = fileName;
			break;
		}
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		binaryReader.BaseStream.Position = 0L;
		SpsSound spsSound = new SpsSound(binaryReader);
		fileStream.Close();
		binaryReader.Close();
		if (spsSound == null)
		{
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		int num;
		if (fileNameWithoutExtension.EndsWith("_0"))
		{
			fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 2);
			num = 0;
		}
		else
		{
			if (!fileNameWithoutExtension.EndsWith("_1"))
			{
				return;
			}
			fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 2);
			num = 1;
		}
		int num2 = FifaEnvironment.NameDictionary.TryGetKey(fileNameWithoutExtension);
		if (num2 < 0)
		{
			num2 = Audio.CommentaryDictionary.SearchName(fileNameWithoutExtension);
			if (FifaEnvironment.NameDictionary.ContainsKey(num2))
			{
				num2 = -1;
			}
			if (num2 < 0)
			{
				num2 = FifaEnvironment.NameDictionary.GetNewKey();
			}
			FifaEnvironment.NameDictionary.Add(num2, fileNameWithoutExtension);
		}
		NameSound nameSound = (NameSound)m_PatchAudio.SbrFile.SimpleSurnamesGroup.NameSoundList.SearchId(num2);
		if (nameSound == null)
		{
			nameSound = new NameSound(num2);
			nameSound.Text = fileNameWithoutExtension;
			m_PatchAudio.SbrFile.SimpleSurnamesGroup.NameSoundList.Add(nameSound);
			m_PatchAudio.SbrFile.SimpleSurnamesGroup.NameSoundList.SortId();
		}
		if (nameSound != null)
		{
			if (num == 0)
			{
				nameSound.HighSound = spsSound;
			}
			else
			{
				nameSound.LowSound = spsSound;
			}
		}
	}

	public void UseStandardId()
	{
		if (m_PatchAudio != null)
		{
			m_PatchAudio.UseStandardId();
		}
	}

	private void buttonDeleteSoundPatch_Click(object sender, EventArgs e)
	{
		if (listViewPatchSounds.SelectedItems.Count > 0)
		{
			NameSound nameSound = (NameSound)listViewPatchSounds.SelectedItems[0].Tag;
			listViewPatchSounds.Items.Remove(listViewPatchSounds.SelectedItems[0]);
			if (nameSound != null)
			{
				m_PatchAudio.SbrFile.SimpleSurnamesGroup.NameSoundList.Remove(nameSound);
				m_HasToSave = true;
				SetPatchButtonsState();
			}
		}
	}

	private void buttonOpenSound1_Click(object sender, EventArgs e)
	{
		openImportSound.CheckFileExists = true;
		openImportSound.Multiselect = false;
		openImportSound.InitialDirectory = m_LastImportingFolder;
		openImportSound.Filter = "Sound files (*.sps)|*.sps";
		openImportSound.FilterIndex = 1;
		openImportSound.Title = "Select one Sound file";
		if (openImportSound.ShowDialog() == DialogResult.OK)
		{
			string fileName = openImportSound.FileName;
			OpenSound(fileName, listViewSound1);
			labelSound1.Text = "Sound 1: " + Path.GetFileNameWithoutExtension(fileName);
			ShowEditingPanel();
		}
	}

	private void buttonOpenSound2_Click(object sender, EventArgs e)
	{
		openImportSound.CheckFileExists = true;
		openImportSound.Multiselect = false;
		openImportSound.InitialDirectory = m_LastImportingFolder;
		openImportSound.Filter = "Sound files (*.sps)|*.sps";
		openImportSound.FilterIndex = 1;
		openImportSound.Title = "Select one Sound file";
		if (openImportSound.ShowDialog() == DialogResult.OK)
		{
			string fileName = openImportSound.FileName;
			OpenSound(fileName, listViewSound2);
			labelSound2.Text = "Sound 2: " + Path.GetFileNameWithoutExtension(fileName);
			ShowEditingPanel();
		}
	}

	private void buttonSaveEditedSound_Click(object sender, EventArgs e)
	{
		saveFileDialog1.CheckFileExists = false;
		saveFileDialog1.InitialDirectory = m_LastImportingFolder;
		saveFileDialog1.RestoreDirectory = true;
		saveFileDialog1.Filter = "Sound Files (*.sps)|*.sps";
		saveFileDialog1.FilterIndex = 1;
		saveFileDialog1.Title = "Save Sound File";
		if (saveFileDialog1.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string fileName = saveFileDialog1.FileName;
		int num = 0;
		SpsSoundData[] array = new SpsSoundData[50];
		foreach (ListViewItem item in listViewSound1.Items)
		{
			if (item.Checked)
			{
				array[num] = (SpsSoundData)item.Tag;
				num++;
			}
		}
		foreach (ListViewItem item2 in listViewSound2.Items)
		{
			if (item2.Checked)
			{
				array[num] = (SpsSoundData)item2.Tag;
				num++;
			}
		}
		if (num != 0)
		{
			SpsSound spsSound = new SpsSound(num);
			for (int i = 0; i < num; i++)
			{
				spsSound.Segments[i] = array[i];
			}
			spsSound.AddHeaderAndTerminator();
			spsSound.ExportAsFile(fileName);
		}
	}

	private void ShowEditingPanel()
	{
		buttonSaveEditedSound.Enabled = listViewSound1.Items.Count != 0 || listViewSound2.Items.Count != 0;
	}

	private void OpenSound(string fileName, ListView listView)
	{
		SpsSound spsSound = new SpsSound();
		if (!spsSound.ImportFromFile(fileName))
		{
			return;
		}
		listView.BeginUpdate();
		listView.Items.Clear();
		listView.Tag = spsSound;
		int num = 0;
		foreach (SpsSoundData segment in spsSound.Segments)
		{
			if (segment != null)
			{
				num += segment.nSamples;
				ListViewItem listViewItem = new ListViewItem((num / 32).ToString());
				listViewItem.Tag = segment;
				listView.Items.Add(listViewItem);
			}
		}
		listView.EndUpdate();
	}

	private void buttonSearchPatchSoundExact_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchExact;
		SearchPatchSoundByName();
	}

	private void buttonSearchPatchSoundStarting_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchStarting;
		SearchPatchSoundByName();
	}

	private void buttonSearchPatchSoundContaining_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchContaining;
		SearchPatchSoundByName();
	}

	private void buttonPlaySound2_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		if (listViewExploreSounds.SelectedItems.Count > 0)
		{
			NameSound nameSound = (NameSound)listViewExploreSounds.SelectedItems[0].Tag;
			if (nameSound != null)
			{
				if (nameSound.HighSound != null)
				{
					string text = FifaEnvironment.ExportFolder + "\\" + nameSound.Text + "_0.sps";
					nameSound.HighSound.ExportAsFile(text);
					DecodeAndPlaySps(text);
				}
				if (nameSound.LowSound != null)
				{
					string text2 = FifaEnvironment.ExportFolder + "\\" + nameSound.Text + "_1.sps";
					nameSound.LowSound.ExportAsFile(text2);
					DecodeAndPlaySps(text2);
				}
			}
		}
		Cursor.Current = Cursors.Default;
	}

	private void ExportSelectedSounds(bool convertToWav)
	{
		if (m_LastExportingFolder == null)
		{
			m_LastExportingFolder = FifaEnvironment.ExportFolder;
		}
		folderBrowserExportSounds = new FolderBrowserDialog();
		folderBrowserExportSounds.SelectedPath = m_LastExportingFolder;
		folderBrowserExportSounds.Description = "Select the export folder";
		folderBrowserExportSounds.ShowNewFolderButton = true;
		if (folderBrowserExportSounds.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		m_LastExportingFolder = folderBrowserExportSounds.SelectedPath;
		Cursor.Current = Cursors.WaitCursor;
		foreach (ListViewItem item in listViewExploreSounds.Items)
		{
			if (item.Checked)
			{
				NameSound obj = (NameSound)item.Tag;
				string text = item.SubItems[1].Text;
				string text2 = m_LastExportingFolder + "\\" + text + "_1.sps";
				obj.ExportLowSound(text2);
				if (convertToWav)
				{
					DecodeSps(text2);
					File.Delete(text2);
				}
				text2 = m_LastExportingFolder + "\\" + text + "_0.sps";
				obj.ExportHighSound(text2);
				if (convertToWav)
				{
					DecodeSps(text2);
					File.Delete(text2);
				}
			}
		}
		Cursor.Current = Cursors.Default;
	}

	private void buttonExportSps2_Click(object sender, EventArgs e)
	{
		bool convertToWav = false;
		ExportSelectedSounds(convertToWav);
	}

	private void buttonExportWav2_Click(object sender, EventArgs e)
	{
		bool convertToWav = true;
		ExportSelectedSounds(convertToWav);
	}

	private void buttonPlaySound_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		if (listViewPatchSounds.SelectedItems.Count > 0)
		{
			NameSound nameSound = (NameSound)listViewPatchSounds.SelectedItems[0].Tag;
			if (nameSound != null)
			{
				if (nameSound.HighSound != null)
				{
					string text = FifaEnvironment.ExportFolder + "\\" + nameSound.Text + "_0.sps";
					nameSound.HighSound.ExportAsFile(text);
					DecodeAndPlaySps(text);
				}
				if (nameSound.LowSound != null)
				{
					string text2 = FifaEnvironment.ExportFolder + "\\" + nameSound.Text + "_1.sps";
					nameSound.LowSound.ExportAsFile(text2);
					DecodeAndPlaySps(text2);
				}
			}
		}
		Cursor.Current = Cursors.Default;
	}

	private bool EncodeSps(string inputFileName)
	{
		string path = Path.ChangeExtension(inputFileName, ".sps");
		if (File.Exists(inputFileName))
		{
			string arguments = "encode \"" + inputFileName + "\" -s";
			ProcessStartInfo processStartInfo = new ProcessStartInfo("EASounds.exe", arguments);
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			using Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
			process.WaitForExit();
		}
		return File.Exists(path);
	}

	private bool DecodeSps(string inputFileName)
	{
		string path = Path.ChangeExtension(inputFileName, ".wav");
		if (File.Exists(inputFileName))
		{
			string arguments = "decode \"" + inputFileName + "\" -w";
			ProcessStartInfo processStartInfo = new ProcessStartInfo("EASounds.exe", arguments);
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			using Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
			process.WaitForExit();
		}
		return File.Exists(path);
	}

	private void DecodeAndPlaySps(string inputFileName)
	{
		string soundLocation = Path.ChangeExtension(inputFileName, ".wav");
		if (DecodeSps(inputFileName))
		{
			new SoundPlayer(soundLocation).PlaySync();
		}
	}

	private void listViewExploreSounds_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetExploreButtonsState();
	}

	private void listViewPatchSounds_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetPatchButtonsState();
	}

	private void SetExploreButtonsState()
	{
		bool flag = m_ExploreAudio != null;
		bool num = listViewExploreSounds.Items.Count > 0;
		bool enabled = num && listViewExploreSounds.SelectedItems.Count > 0;
		bool enabled2 = num && listViewExploreSounds.CheckedItems.Count > 0;
		buttonPlaySound2.Enabled = enabled;
		buttonExportSps2.Enabled = enabled2;
		buttonExportWav2.Enabled = enabled2;
		buttonOpenSoundFile.Enabled = !flag;
		buttonCloseSoundFile.Enabled = flag;
		buttonSelectAllSounds.Enabled = flag;
		buttonDeselectAllSounds.Enabled = flag;
		comboSelctSoundGroup.Enabled = flag;
	}

	private void SetPatchButtonsState()
	{
		bool flag = m_PatchAudio != null;
		bool flag2 = listViewPatchSounds.SelectedItems.Count > 0;
		buttonOpenPatchSound.Enabled = !flag;
		buttonSavePatchedAudio.Enabled = flag && m_HasToSave;
		buttonClosePatchedAudio.Enabled = flag;
		buttonImportSps.Enabled = flag;
		buttonImportWav.Enabled = flag;
		buttonPlaySound.Enabled = flag && flag2;
		buttonDeleteSps.Enabled = flag && flag2;
		buttonExportSps.Enabled = flag && flag2;
		buttonExportWav.Enabled = flag && flag2;
	}

	private void buttonExportSps_Click(object sender, EventArgs e)
	{
		if (m_LastExportingFolder == null)
		{
			m_LastExportingFolder = FifaEnvironment.ExportFolder;
		}
		folderBrowserExportSounds = new FolderBrowserDialog();
		folderBrowserExportSounds.SelectedPath = m_LastExportingFolder;
		folderBrowserExportSounds.Description = "Select the export folder";
		folderBrowserExportSounds.ShowNewFolderButton = true;
		if (folderBrowserExportSounds.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		m_LastExportingFolder = folderBrowserExportSounds.SelectedPath;
		foreach (ListViewItem selectedItem in listViewPatchSounds.SelectedItems)
		{
			NameSound nameSound = (NameSound)selectedItem.Tag;
			string text = selectedItem.SubItems[1].Text;
			string fullFileName = m_LastExportingFolder + "\\" + text + "_1.sps";
			nameSound.ExportLowSound(fullFileName);
			fullFileName = m_LastExportingFolder + "\\" + text + "_0.sps";
			nameSound.ExportHighSound(fullFileName);
		}
	}

	private void buttonExportWav_Click(object sender, EventArgs e)
	{
		if (m_LastExportingFolder == null)
		{
			m_LastExportingFolder = FifaEnvironment.ExportFolder;
		}
		folderBrowserExportSounds = new FolderBrowserDialog();
		folderBrowserExportSounds.SelectedPath = m_LastExportingFolder;
		folderBrowserExportSounds.Description = "Select the export folder";
		folderBrowserExportSounds.ShowNewFolderButton = true;
		if (folderBrowserExportSounds.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		m_LastExportingFolder = folderBrowserExportSounds.SelectedPath;
		foreach (ListViewItem selectedItem in listViewPatchSounds.SelectedItems)
		{
			NameSound nameSound = (NameSound)selectedItem.Tag;
			string text = selectedItem.SubItems[1].Text;
			string text2 = m_LastExportingFolder + "\\" + text + "_1.sps";
			nameSound.ExportLowSound(text2);
			DecodeSps(text2);
			text2 = m_LastExportingFolder + "\\" + text + "_0.sps";
			nameSound.ExportHighSound(text2);
			DecodeSps(text2);
		}
	}

	private void buttonSearchPatchSoundEnding_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchEnding;
		SearchPatchSoundByName();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.AudioForm));
		this.groupAudio = new System.Windows.Forms.GroupBox();
		this.textAudioName = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.buttonDeleteSoundAssociation = new System.Windows.Forms.Button();
		this.buttonSetSound = new System.Windows.Forms.Button();
		this.textSurnameSoundId = new System.Windows.Forms.TextBox();
		this.buttonSearchPlayerId = new System.Windows.Forms.Button();
		this.textPlayerId = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.buttonSearchSurnameId = new System.Windows.Forms.Button();
		this.textKnownAs = new System.Windows.Forms.TextBox();
		this.label13 = new System.Windows.Forms.Label();
		this.groupNameDictionary = new System.Windows.Forms.GroupBox();
		this.numericNameDictionary = new System.Windows.Forms.NumericUpDown();
		this.listViewNameDictionary = new System.Windows.Forms.ListView();
		this.columnNameId = new System.Windows.Forms.ColumnHeader();
		this.columnSurname = new System.Windows.Forms.ColumnHeader();
		this.toolStripNameDictionary = new System.Windows.Forms.ToolStrip();
		this.textNameDictionary = new System.Windows.Forms.ToolStripTextBox();
		this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonAddName = new System.Windows.Forms.ToolStripButton();
		this.buttonReplaceName = new System.Windows.Forms.ToolStripButton();
		this.buttonRemoveName = new System.Windows.Forms.ToolStripButton();
		this.toolStripSearchnameDictionary = new System.Windows.Forms.ToolStrip();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.textSearchNameDictionary = new System.Windows.Forms.ToolStripTextBox();
		this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonFindNameExact = new System.Windows.Forms.ToolStripButton();
		this.buttonFindNameStart = new System.Windows.Forms.ToolStripButton();
		this.buttonFindNameAny = new System.Windows.Forms.ToolStripButton();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupPlayerInfo = new System.Windows.Forms.GroupBox();
		this.labelCommonName = new System.Windows.Forms.Label();
		this.textCommonName = new System.Windows.Forms.TextBox();
		this.textSurname = new System.Windows.Forms.TextBox();
		this.textFirstName = new System.Windows.Forms.TextBox();
		this.labelFirstName = new System.Windows.Forms.Label();
		this.labelSurame = new System.Windows.Forms.Label();
		this.groupExploreAudio = new System.Windows.Forms.GroupBox();
		this.listViewExploreSounds = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this.toolStripExploreExistingSounds = new System.Windows.Forms.ToolStrip();
		this.buttonOpenSoundFile = new System.Windows.Forms.ToolStripSplitButton();
		this.italianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.itaBankToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.neutralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.femaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.demoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.demoNeutralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.demoFemaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.spanishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
		this.mexicoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
		this.brazilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem16 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem17 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripMenuItem();
		this.deutchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem23 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem24 = new System.Windows.Forms.ToolStripMenuItem();
		this.russianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem25 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem26 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem27 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem28 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem29 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem30 = new System.Windows.Forms.ToolStripMenuItem();
		this.frenchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem31 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem32 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem33 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem34 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem35 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem36 = new System.Windows.Forms.ToolStripMenuItem();
		this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem38 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem43 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem44 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem45 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem47 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem46 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem37 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem39 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem40 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem48 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem41 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem42 = new System.Windows.Forms.ToolStripMenuItem();
		this.buttonCloseSoundFile = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonSelectAllSounds = new System.Windows.Forms.ToolStripButton();
		this.buttonDeselectAllSounds = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonPlaySound2 = new System.Windows.Forms.ToolStripButton();
		this.buttonExportSps2 = new System.Windows.Forms.ToolStripButton();
		this.buttonExportWav2 = new System.Windows.Forms.ToolStripButton();
		this.comboSelctSoundGroup = new System.Windows.Forms.ToolStripComboBox();
		this.toolStripSearchSound = new System.Windows.Forms.ToolStrip();
		this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
		this.textSearchExplore = new System.Windows.Forms.ToolStripTextBox();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonSearchExploreSoundExact = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchExploreSoundStarting = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchExploreSoundContaining = new System.Windows.Forms.ToolStripButton();
		this.folderBrowserExportSounds = new System.Windows.Forms.FolderBrowserDialog();
		this.groupPatchAudio = new System.Windows.Forms.GroupBox();
		this.listViewPatchSounds = new System.Windows.Forms.ListView();
		this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
		this.toolStrip2 = new System.Windows.Forms.ToolStrip();
		this.buttonOpenPatchSound = new System.Windows.Forms.ToolStripSplitButton();
		this.toolStripMenuItem49 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem50 = new System.Windows.Forms.ToolStripMenuItem();
		this.iTANeutralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem56 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem57 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem58 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem59 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem60 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem61 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem62 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem63 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem64 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem65 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem66 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem67 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem68 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem69 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem70 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem71 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem72 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem73 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem74 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem75 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem76 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem77 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem78 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem79 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem80 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem81 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem82 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem83 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem84 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem85 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem86 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem87 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem88 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem89 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem90 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem91 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem92 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem93 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem94 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem95 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem96 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem97 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem98 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem99 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem100 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem101 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem102 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem103 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem104 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem105 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem106 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem107 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem108 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem109 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem110 = new System.Windows.Forms.ToolStripMenuItem();
		this.buttonSavePatchedAudio = new System.Windows.Forms.ToolStripButton();
		this.buttonClosePatchedAudio = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonPlaySound = new System.Windows.Forms.ToolStripButton();
		this.buttonExportSps = new System.Windows.Forms.ToolStripButton();
		this.buttonExportWav = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImportSps = new System.Windows.Forms.ToolStripButton();
		this.buttonImportWav = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonDeleteSps = new System.Windows.Forms.ToolStripButton();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
		this.textSearchPatch = new System.Windows.Forms.ToolStripTextBox();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonSearchPatchSoundExact = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchPatchSoundStarting = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchPatchSoundContaining = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchPatchSoundEnding = new System.Windows.Forms.ToolStripButton();
		this.openImportSound = new System.Windows.Forms.OpenFileDialog();
		this.groupSoundEditing = new System.Windows.Forms.GroupBox();
		this.listViewSound2 = new System.Windows.Forms.ListView();
		this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
		this.labelSound2 = new System.Windows.Forms.Label();
		this.listViewSound1 = new System.Windows.Forms.ListView();
		this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
		this.labelSound1 = new System.Windows.Forms.Label();
		this.toolStrip4 = new System.Windows.Forms.ToolStrip();
		this.buttonOpenSound1 = new System.Windows.Forms.ToolStripButton();
		this.buttonOpenSound2 = new System.Windows.Forms.ToolStripButton();
		this.buttonSaveEditedSound = new System.Windows.Forms.ToolStripButton();
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		this.arabicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.arabicToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.menuAraNeutral = new System.Windows.Forms.ToolStripMenuItem();
		this.aRAArasabankToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.aRANeutralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.aRADemoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.viewer2DPhoto = new FifaControls.Viewer2D();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.groupAudio.SuspendLayout();
		this.groupNameDictionary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNameDictionary).BeginInit();
		this.toolStripNameDictionary.SuspendLayout();
		this.toolStripSearchnameDictionary.SuspendLayout();
		this.groupPlayerInfo.SuspendLayout();
		this.groupExploreAudio.SuspendLayout();
		this.toolStripExploreExistingSounds.SuspendLayout();
		this.toolStripSearchSound.SuspendLayout();
		this.groupPatchAudio.SuspendLayout();
		this.toolStrip2.SuspendLayout();
		this.toolStrip1.SuspendLayout();
		this.groupSoundEditing.SuspendLayout();
		this.toolStrip4.SuspendLayout();
		base.SuspendLayout();
		this.groupAudio.Controls.Add(this.textAudioName);
		this.groupAudio.Controls.Add(this.label3);
		this.groupAudio.Controls.Add(this.label2);
		this.groupAudio.Controls.Add(this.buttonDeleteSoundAssociation);
		this.groupAudio.Controls.Add(this.buttonSetSound);
		this.groupAudio.Controls.Add(this.textSurnameSoundId);
		this.groupAudio.Controls.Add(this.buttonSearchPlayerId);
		this.groupAudio.Controls.Add(this.textPlayerId);
		this.groupAudio.Controls.Add(this.label1);
		this.groupAudio.Controls.Add(this.buttonSearchSurnameId);
		this.groupAudio.Controls.Add(this.textKnownAs);
		this.groupAudio.Controls.Add(this.label13);
		this.groupAudio.Location = new System.Drawing.Point(9, 31);
		this.groupAudio.Name = "groupAudio";
		this.groupAudio.Size = new System.Drawing.Size(308, 134);
		this.groupAudio.TabIndex = 92;
		this.groupAudio.TabStop = false;
		this.groupAudio.Text = "Player Audio";
		this.textAudioName.BackColor = System.Drawing.Color.White;
		this.textAudioName.Location = new System.Drawing.Point(88, 104);
		this.textAudioName.Name = "textAudioName";
		this.textAudioName.ReadOnly = true;
		this.textAudioName.Size = new System.Drawing.Size(148, 20);
		this.textAudioName.TabIndex = 110;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(6, 107);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(65, 13);
		this.label3.TabIndex = 109;
		this.label3.Text = "Audio Name";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(6, 55);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(55, 13);
		this.label2.TabIndex = 108;
		this.label2.Text = "Known As";
		this.buttonDeleteSoundAssociation.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonDeleteSoundAssociation.BackgroundImage");
		this.buttonDeleteSoundAssociation.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonDeleteSoundAssociation.Location = new System.Drawing.Point(211, 76);
		this.buttonDeleteSoundAssociation.Name = "buttonDeleteSoundAssociation";
		this.buttonDeleteSoundAssociation.Size = new System.Drawing.Size(25, 23);
		this.buttonDeleteSoundAssociation.TabIndex = 107;
		this.toolTip.SetToolTip(this.buttonDeleteSoundAssociation, "Remove the generic audio associated to this name");
		this.buttonDeleteSoundAssociation.UseVisualStyleBackColor = true;
		this.buttonDeleteSoundAssociation.Click += new System.EventHandler(buttonDeleteSound_Click);
		this.buttonSetSound.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonSetSound.BackgroundImage");
		this.buttonSetSound.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonSetSound.Enabled = false;
		this.buttonSetSound.Location = new System.Drawing.Point(242, 69);
		this.buttonSetSound.Name = "buttonSetSound";
		this.buttonSetSound.Size = new System.Drawing.Size(50, 59);
		this.buttonSetSound.TabIndex = 106;
		this.buttonSetSound.UseVisualStyleBackColor = true;
		this.buttonSetSound.Click += new System.EventHandler(buttonSetSound_Click);
		this.textSurnameSoundId.BackColor = System.Drawing.Color.White;
		this.textSurnameSoundId.Location = new System.Drawing.Point(88, 78);
		this.textSurnameSoundId.Name = "textSurnameSoundId";
		this.textSurnameSoundId.ReadOnly = true;
		this.textSurnameSoundId.Size = new System.Drawing.Size(86, 20);
		this.textSurnameSoundId.TabIndex = 105;
		this.textSurnameSoundId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.buttonSearchPlayerId.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonSearchPlayerId.BackgroundImage");
		this.buttonSearchPlayerId.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonSearchPlayerId.Location = new System.Drawing.Point(180, 23);
		this.buttonSearchPlayerId.Name = "buttonSearchPlayerId";
		this.buttonSearchPlayerId.Size = new System.Drawing.Size(25, 23);
		this.buttonSearchPlayerId.TabIndex = 104;
		this.toolTip.SetToolTip(this.buttonSearchPlayerId, "Search specific audio for this player");
		this.buttonSearchPlayerId.UseVisualStyleBackColor = true;
		this.buttonSearchPlayerId.Click += new System.EventHandler(buttonSearchPlayerId_Click);
		this.textPlayerId.BackColor = System.Drawing.Color.White;
		this.textPlayerId.Location = new System.Drawing.Point(88, 25);
		this.textPlayerId.Name = "textPlayerId";
		this.textPlayerId.ReadOnly = true;
		this.textPlayerId.Size = new System.Drawing.Size(86, 20);
		this.textPlayerId.TabIndex = 103;
		this.textPlayerId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(6, 28);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(48, 13);
		this.label1.TabIndex = 102;
		this.label1.Text = "Player Id";
		this.buttonSearchSurnameId.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonSearchSurnameId.BackgroundImage");
		this.buttonSearchSurnameId.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonSearchSurnameId.Location = new System.Drawing.Point(180, 76);
		this.buttonSearchSurnameId.Name = "buttonSearchSurnameId";
		this.buttonSearchSurnameId.Size = new System.Drawing.Size(25, 23);
		this.buttonSearchSurnameId.TabIndex = 101;
		this.toolTip.SetToolTip(this.buttonSearchSurnameId, "Search generic audio for this name");
		this.buttonSearchSurnameId.UseVisualStyleBackColor = true;
		this.buttonSearchSurnameId.Click += new System.EventHandler(buttonSearchSurnameId_Click);
		this.textKnownAs.BackColor = System.Drawing.Color.White;
		this.textKnownAs.Location = new System.Drawing.Point(88, 52);
		this.textKnownAs.Name = "textKnownAs";
		this.textKnownAs.ReadOnly = true;
		this.textKnownAs.Size = new System.Drawing.Size(148, 20);
		this.textKnownAs.TabIndex = 2;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(6, 81);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(46, 13);
		this.label13.TabIndex = 1;
		this.label13.Text = "Audio Id";
		this.groupNameDictionary.Controls.Add(this.numericNameDictionary);
		this.groupNameDictionary.Controls.Add(this.listViewNameDictionary);
		this.groupNameDictionary.Controls.Add(this.toolStripNameDictionary);
		this.groupNameDictionary.Controls.Add(this.toolStripSearchnameDictionary);
		this.groupNameDictionary.Location = new System.Drawing.Point(9, 327);
		this.groupNameDictionary.Name = "groupNameDictionary";
		this.groupNameDictionary.Size = new System.Drawing.Size(308, 494);
		this.groupNameDictionary.TabIndex = 3;
		this.groupNameDictionary.TabStop = false;
		this.groupNameDictionary.Text = "Names Dictionary";
		this.numericNameDictionary.Location = new System.Drawing.Point(6, 44);
		this.numericNameDictionary.Maximum = new decimal(new int[4] { 1000000, 0, 0, 0 });
		this.numericNameDictionary.Name = "numericNameDictionary";
		this.numericNameDictionary.Size = new System.Drawing.Size(80, 20);
		this.numericNameDictionary.TabIndex = 126;
		this.numericNameDictionary.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNameDictionary.ThousandsSeparator = true;
		this.numericNameDictionary.Value = new decimal(new int[4] { 900000, 0, 0, 0 });
		this.numericNameDictionary.ValueChanged += new System.EventHandler(numericNameDictionary_ValueChanged);
		this.listViewNameDictionary.AllowDrop = true;
		this.listViewNameDictionary.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.columnNameId, this.columnSurname });
		this.listViewNameDictionary.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewNameDictionary.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewNameDictionary.FullRowSelect = true;
		this.listViewNameDictionary.GridLines = true;
		this.listViewNameDictionary.HideSelection = false;
		this.listViewNameDictionary.Location = new System.Drawing.Point(3, 66);
		this.listViewNameDictionary.MultiSelect = false;
		this.listViewNameDictionary.Name = "listViewNameDictionary";
		this.listViewNameDictionary.Size = new System.Drawing.Size(302, 425);
		this.listViewNameDictionary.TabIndex = 9;
		this.listViewNameDictionary.UseCompatibleStateImageBehavior = false;
		this.listViewNameDictionary.View = System.Windows.Forms.View.Details;
		this.listViewNameDictionary.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(listViewNameDictionary_ColumnClick);
		this.listViewNameDictionary.SelectedIndexChanged += new System.EventHandler(listViewNameDictionary_SelectedIndexChanged);
		this.columnNameId.Text = "N.";
		this.columnNameId.Width = 88;
		this.columnSurname.Text = "Name";
		this.columnSurname.Width = 154;
		this.toolStripNameDictionary.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStripNameDictionary.BackgroundImage");
		this.toolStripNameDictionary.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripNameDictionary.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.textNameDictionary, this.toolStripSeparator7, this.buttonAddName, this.buttonReplaceName, this.buttonRemoveName });
		this.toolStripNameDictionary.Location = new System.Drawing.Point(3, 41);
		this.toolStripNameDictionary.Name = "toolStripNameDictionary";
		this.toolStripNameDictionary.Size = new System.Drawing.Size(302, 25);
		this.toolStripNameDictionary.TabIndex = 127;
		this.toolStripNameDictionary.Text = "toolStrip1";
		this.textNameDictionary.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textNameDictionary.Margin = new System.Windows.Forms.Padding(90, 0, 1, 0);
		this.textNameDictionary.Name = "textNameDictionary";
		this.textNameDictionary.Size = new System.Drawing.Size(130, 25);
		this.textNameDictionary.TextChanged += new System.EventHandler(textNameDictionary_TextChanged);
		this.toolStripSeparator7.Name = "toolStripSeparator7";
		this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
		this.buttonAddName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonAddName.Enabled = false;
		this.buttonAddName.Image = (System.Drawing.Image)resources.GetObject("buttonAddName.Image");
		this.buttonAddName.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonAddName.Name = "buttonAddName";
		this.buttonAddName.Size = new System.Drawing.Size(23, 22);
		this.buttonAddName.Text = "Add";
		this.buttonAddName.ToolTipText = "Add to the Names Directory";
		this.buttonAddName.Click += new System.EventHandler(buttonAddName_Click);
		this.buttonReplaceName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonReplaceName.Enabled = false;
		this.buttonReplaceName.Image = (System.Drawing.Image)resources.GetObject("buttonReplaceName.Image");
		this.buttonReplaceName.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonReplaceName.Name = "buttonReplaceName";
		this.buttonReplaceName.Size = new System.Drawing.Size(23, 22);
		this.buttonReplaceName.Text = "Replace";
		this.buttonReplaceName.ToolTipText = "Replace in the Names Directory";
		this.buttonReplaceName.Click += new System.EventHandler(buttonReplaceName_Click);
		this.buttonRemoveName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemoveName.Enabled = false;
		this.buttonRemoveName.Image = (System.Drawing.Image)resources.GetObject("buttonRemoveName.Image");
		this.buttonRemoveName.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemoveName.Name = "buttonRemoveName";
		this.buttonRemoveName.Size = new System.Drawing.Size(23, 22);
		this.buttonRemoveName.Text = "Remove";
		this.buttonRemoveName.ToolTipText = "Remove from the Names Directory";
		this.buttonRemoveName.Click += new System.EventHandler(buttonRemoveName_Click);
		this.toolStripSearchnameDictionary.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStripSearchnameDictionary.BackgroundImage");
		this.toolStripSearchnameDictionary.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripSearchnameDictionary.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripLabel1, this.textSearchNameDictionary, this.toolStripSeparator6, this.buttonFindNameExact, this.buttonFindNameStart, this.buttonFindNameAny });
		this.toolStripSearchnameDictionary.Location = new System.Drawing.Point(3, 16);
		this.toolStripSearchnameDictionary.Name = "toolStripSearchnameDictionary";
		this.toolStripSearchnameDictionary.Size = new System.Drawing.Size(302, 25);
		this.toolStripSearchnameDictionary.TabIndex = 125;
		this.toolStripLabel1.AutoSize = false;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(90, 22);
		this.toolStripLabel1.Text = "Search";
		this.textSearchNameDictionary.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textSearchNameDictionary.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
		this.textSearchNameDictionary.Name = "textSearchNameDictionary";
		this.textSearchNameDictionary.Size = new System.Drawing.Size(130, 25);
		this.toolStripSeparator6.Name = "toolStripSeparator6";
		this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
		this.buttonFindNameExact.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonFindNameExact.Image = (System.Drawing.Image)resources.GetObject("buttonFindNameExact.Image");
		this.buttonFindNameExact.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonFindNameExact.Name = "buttonFindNameExact";
		this.buttonFindNameExact.Size = new System.Drawing.Size(23, 22);
		this.buttonFindNameExact.Text = "Search Exactly";
		this.buttonFindNameExact.Click += new System.EventHandler(buttonFindNameExact_Click);
		this.buttonFindNameStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonFindNameStart.Image = (System.Drawing.Image)resources.GetObject("buttonFindNameStart.Image");
		this.buttonFindNameStart.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonFindNameStart.Name = "buttonFindNameStart";
		this.buttonFindNameStart.Size = new System.Drawing.Size(23, 22);
		this.buttonFindNameStart.Text = "Search if starting with";
		this.buttonFindNameStart.Click += new System.EventHandler(buttonFindNameStart_Click);
		this.buttonFindNameAny.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonFindNameAny.Image = (System.Drawing.Image)resources.GetObject("buttonFindNameAny.Image");
		this.buttonFindNameAny.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonFindNameAny.Name = "buttonFindNameAny";
		this.buttonFindNameAny.Size = new System.Drawing.Size(23, 22);
		this.buttonFindNameAny.Text = "Search if containing";
		this.buttonFindNameAny.Click += new System.EventHandler(buttonFindNameAny_Click);
		this.groupPlayerInfo.Controls.Add(this.labelCommonName);
		this.groupPlayerInfo.Controls.Add(this.textCommonName);
		this.groupPlayerInfo.Controls.Add(this.viewer2DPhoto);
		this.groupPlayerInfo.Controls.Add(this.textSurname);
		this.groupPlayerInfo.Controls.Add(this.textFirstName);
		this.groupPlayerInfo.Controls.Add(this.labelFirstName);
		this.groupPlayerInfo.Controls.Add(this.labelSurame);
		this.groupPlayerInfo.Location = new System.Drawing.Point(9, 171);
		this.groupPlayerInfo.Name = "groupPlayerInfo";
		this.groupPlayerInfo.Size = new System.Drawing.Size(308, 150);
		this.groupPlayerInfo.TabIndex = 93;
		this.groupPlayerInfo.TabStop = false;
		this.labelCommonName.AutoSize = true;
		this.labelCommonName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCommonName.Location = new System.Drawing.Point(148, 99);
		this.labelCommonName.Name = "labelCommonName";
		this.labelCommonName.Size = new System.Drawing.Size(79, 13);
		this.labelCommonName.TabIndex = 168;
		this.labelCommonName.Text = "Common Name";
		this.labelCommonName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textCommonName.BackColor = System.Drawing.Color.White;
		this.textCommonName.Location = new System.Drawing.Point(147, 115);
		this.textCommonName.Name = "textCommonName";
		this.textCommonName.ReadOnly = true;
		this.textCommonName.Size = new System.Drawing.Size(131, 20);
		this.textCommonName.TabIndex = 166;
		this.textCommonName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textSurname.BackColor = System.Drawing.Color.White;
		this.textSurname.Location = new System.Drawing.Point(147, 76);
		this.textSurname.Name = "textSurname";
		this.textSurname.ReadOnly = true;
		this.textSurname.Size = new System.Drawing.Size(131, 20);
		this.textSurname.TabIndex = 163;
		this.textSurname.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textFirstName.BackColor = System.Drawing.Color.White;
		this.textFirstName.Location = new System.Drawing.Point(147, 37);
		this.textFirstName.Name = "textFirstName";
		this.textFirstName.ReadOnly = true;
		this.textFirstName.Size = new System.Drawing.Size(131, 20);
		this.textFirstName.TabIndex = 162;
		this.textFirstName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.labelFirstName.AutoSize = true;
		this.labelFirstName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelFirstName.Location = new System.Drawing.Point(148, 21);
		this.labelFirstName.Name = "labelFirstName";
		this.labelFirstName.Size = new System.Drawing.Size(57, 13);
		this.labelFirstName.TabIndex = 164;
		this.labelFirstName.Text = "First Name";
		this.labelFirstName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelSurame.AutoSize = true;
		this.labelSurame.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelSurame.Location = new System.Drawing.Point(148, 60);
		this.labelSurame.Name = "labelSurame";
		this.labelSurame.Size = new System.Drawing.Size(58, 13);
		this.labelSurame.TabIndex = 165;
		this.labelSurame.Text = "Last Name";
		this.labelSurame.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupExploreAudio.Controls.Add(this.listViewExploreSounds);
		this.groupExploreAudio.Controls.Add(this.toolStripExploreExistingSounds);
		this.groupExploreAudio.Controls.Add(this.toolStripSearchSound);
		this.groupExploreAudio.Location = new System.Drawing.Point(323, 31);
		this.groupExploreAudio.Name = "groupExploreAudio";
		this.groupExploreAudio.Size = new System.Drawing.Size(337, 790);
		this.groupExploreAudio.TabIndex = 94;
		this.groupExploreAudio.TabStop = false;
		this.groupExploreAudio.Text = "Exploring:";
		this.listViewExploreSounds.AllowColumnReorder = true;
		this.listViewExploreSounds.AllowDrop = true;
		this.listViewExploreSounds.CheckBoxes = true;
		this.listViewExploreSounds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.columnHeader1, this.columnHeader2 });
		this.listViewExploreSounds.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewExploreSounds.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewExploreSounds.FullRowSelect = true;
		this.listViewExploreSounds.GridLines = true;
		this.listViewExploreSounds.HideSelection = false;
		this.listViewExploreSounds.Location = new System.Drawing.Point(3, 66);
		this.listViewExploreSounds.MultiSelect = false;
		this.listViewExploreSounds.Name = "listViewExploreSounds";
		this.listViewExploreSounds.Size = new System.Drawing.Size(331, 721);
		this.listViewExploreSounds.TabIndex = 129;
		this.listViewExploreSounds.UseCompatibleStateImageBehavior = false;
		this.listViewExploreSounds.View = System.Windows.Forms.View.Details;
		this.listViewExploreSounds.SelectedIndexChanged += new System.EventHandler(listViewExploreSounds_SelectedIndexChanged);
		this.columnHeader1.Text = "N.";
		this.columnHeader1.Width = 66;
		this.columnHeader2.Text = "Name";
		this.columnHeader2.Width = 208;
		this.toolStripExploreExistingSounds.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStripExploreExistingSounds.BackgroundImage");
		this.toolStripExploreExistingSounds.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripExploreExistingSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.buttonOpenSoundFile, this.buttonCloseSoundFile, this.toolStripSeparator1, this.buttonSelectAllSounds, this.buttonDeselectAllSounds, this.toolStripSeparator2, this.buttonPlaySound2, this.buttonExportSps2, this.buttonExportWav2, this.comboSelctSoundGroup });
		this.toolStripExploreExistingSounds.Location = new System.Drawing.Point(3, 41);
		this.toolStripExploreExistingSounds.Name = "toolStripExploreExistingSounds";
		this.toolStripExploreExistingSounds.Size = new System.Drawing.Size(331, 25);
		this.toolStripExploreExistingSounds.TabIndex = 131;
		this.toolStripExploreExistingSounds.Text = "toolStrip1";
		this.buttonOpenSoundFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonOpenSoundFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.italianToolStripMenuItem, this.spanishToolStripMenuItem, this.mexicoToolStripMenuItem, this.brazilToolStripMenuItem, this.deutchToolStripMenuItem, this.russianToolStripMenuItem, this.frenchToolStripMenuItem, this.englishToolStripMenuItem, this.arabicToolStripMenuItem });
		this.buttonOpenSoundFile.Image = (System.Drawing.Image)resources.GetObject("buttonOpenSoundFile.Image");
		this.buttonOpenSoundFile.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonOpenSoundFile.Name = "buttonOpenSoundFile";
		this.buttonOpenSoundFile.Size = new System.Drawing.Size(32, 22);
		this.buttonOpenSoundFile.Text = "Open Sound File";
		this.italianToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.itaBankToolStripMenuItem, this.neutralToolStripMenuItem, this.femaleToolStripMenuItem, this.demoToolStripMenuItem, this.demoNeutralToolStripMenuItem, this.demoFemaleToolStripMenuItem });
		this.italianToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("italianToolStripMenuItem.Image");
		this.italianToolStripMenuItem.Name = "italianToolStripMenuItem";
		this.italianToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.italianToolStripMenuItem.Text = "Italian";
		this.itaBankToolStripMenuItem.Name = "itaBankToolStripMenuItem";
		this.itaBankToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
		this.itaBankToolStripMenuItem.Text = "ITA ita_it_bank";
		this.itaBankToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.neutralToolStripMenuItem.Name = "neutralToolStripMenuItem";
		this.neutralToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
		this.neutralToolStripMenuItem.Text = "ITA Neutral";
		this.neutralToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.femaleToolStripMenuItem.Name = "femaleToolStripMenuItem";
		this.femaleToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
		this.femaleToolStripMenuItem.Text = "ITA Female";
		this.femaleToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.demoToolStripMenuItem.Name = "demoToolStripMenuItem";
		this.demoToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
		this.demoToolStripMenuItem.Text = "ITA Demo";
		this.demoToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.demoNeutralToolStripMenuItem.Name = "demoNeutralToolStripMenuItem";
		this.demoNeutralToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
		this.demoNeutralToolStripMenuItem.Text = "ITA Demo_Neutral";
		this.demoNeutralToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.demoFemaleToolStripMenuItem.Name = "demoFemaleToolStripMenuItem";
		this.demoFemaleToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
		this.demoFemaleToolStripMenuItem.Text = "ITA Demo_Female";
		this.demoFemaleToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.spanishToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem1, this.toolStripMenuItem2, this.toolStripMenuItem3, this.toolStripMenuItem4, this.toolStripMenuItem5, this.toolStripMenuItem6 });
		this.spanishToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("spanishToolStripMenuItem.Image");
		this.spanishToolStripMenuItem.Name = "spanishToolStripMenuItem";
		this.spanishToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.spanishToolStripMenuItem.Text = "Spanish";
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem1.Text = "SPA spa_es_bank";
		this.toolStripMenuItem1.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem2.Name = "toolStripMenuItem2";
		this.toolStripMenuItem2.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem2.Text = "SPA Neutral";
		this.toolStripMenuItem2.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem3.Name = "toolStripMenuItem3";
		this.toolStripMenuItem3.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem3.Text = "SPA Female";
		this.toolStripMenuItem3.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem4.Name = "toolStripMenuItem4";
		this.toolStripMenuItem4.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem4.Text = "SPA Demo";
		this.toolStripMenuItem4.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem5.Name = "toolStripMenuItem5";
		this.toolStripMenuItem5.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem5.Text = "SPA Demo_Neutral";
		this.toolStripMenuItem5.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem6.Name = "toolStripMenuItem6";
		this.toolStripMenuItem6.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem6.Text = "SPA Demo_Female";
		this.toolStripMenuItem6.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.mexicoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem7, this.toolStripMenuItem8, this.toolStripMenuItem9, this.toolStripMenuItem10, this.toolStripMenuItem11, this.toolStripMenuItem12 });
		this.mexicoToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("mexicoToolStripMenuItem.Image");
		this.mexicoToolStripMenuItem.Name = "mexicoToolStripMenuItem";
		this.mexicoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.mexicoToolStripMenuItem.Text = "Mexico";
		this.toolStripMenuItem7.Name = "toolStripMenuItem7";
		this.toolStripMenuItem7.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem7.Text = "MEX spa_mx_bank";
		this.toolStripMenuItem7.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem8.Name = "toolStripMenuItem8";
		this.toolStripMenuItem8.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem8.Text = "MEX Neutral";
		this.toolStripMenuItem8.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem9.Name = "toolStripMenuItem9";
		this.toolStripMenuItem9.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem9.Text = "MEX Female";
		this.toolStripMenuItem9.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem10.Name = "toolStripMenuItem10";
		this.toolStripMenuItem10.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem10.Text = "MEX Demo";
		this.toolStripMenuItem10.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem11.Name = "toolStripMenuItem11";
		this.toolStripMenuItem11.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem11.Text = "MEX Demo_Neutral";
		this.toolStripMenuItem11.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem12.Name = "toolStripMenuItem12";
		this.toolStripMenuItem12.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem12.Text = "MEX Demo_Female";
		this.toolStripMenuItem12.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.brazilToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem13, this.toolStripMenuItem14, this.toolStripMenuItem15, this.toolStripMenuItem16, this.toolStripMenuItem17, this.toolStripMenuItem18 });
		this.brazilToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("brazilToolStripMenuItem.Image");
		this.brazilToolStripMenuItem.Name = "brazilToolStripMenuItem";
		this.brazilToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.brazilToolStripMenuItem.Text = "Portuguese";
		this.toolStripMenuItem13.Name = "toolStripMenuItem13";
		this.toolStripMenuItem13.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem13.Text = "BRA por_br_bank";
		this.toolStripMenuItem13.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem14.Name = "toolStripMenuItem14";
		this.toolStripMenuItem14.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem14.Text = "BRA Neutral";
		this.toolStripMenuItem14.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem15.Name = "toolStripMenuItem15";
		this.toolStripMenuItem15.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem15.Text = "BRA Female";
		this.toolStripMenuItem15.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem16.Name = "toolStripMenuItem16";
		this.toolStripMenuItem16.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem16.Text = "BRA Demo";
		this.toolStripMenuItem16.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem17.Name = "toolStripMenuItem17";
		this.toolStripMenuItem17.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem17.Text = "BRA Demo_Neutral";
		this.toolStripMenuItem17.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem18.Name = "toolStripMenuItem18";
		this.toolStripMenuItem18.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem18.Text = "BRA Demo_Female";
		this.toolStripMenuItem18.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.deutchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem19, this.toolStripMenuItem20, this.toolStripMenuItem21, this.toolStripMenuItem22, this.toolStripMenuItem23, this.toolStripMenuItem24 });
		this.deutchToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("deutchToolStripMenuItem.Image");
		this.deutchToolStripMenuItem.Name = "deutchToolStripMenuItem";
		this.deutchToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.deutchToolStripMenuItem.Text = "German";
		this.toolStripMenuItem19.Name = "toolStripMenuItem19";
		this.toolStripMenuItem19.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem19.Text = "GER ger_de_bank";
		this.toolStripMenuItem19.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem20.Name = "toolStripMenuItem20";
		this.toolStripMenuItem20.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem20.Text = "GER Neutral";
		this.toolStripMenuItem20.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem21.Name = "toolStripMenuItem21";
		this.toolStripMenuItem21.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem21.Text = "GER Female";
		this.toolStripMenuItem21.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem22.Name = "toolStripMenuItem22";
		this.toolStripMenuItem22.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem22.Text = "GER Demo";
		this.toolStripMenuItem22.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem23.Name = "toolStripMenuItem23";
		this.toolStripMenuItem23.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem23.Text = "GER Demo_Neutral";
		this.toolStripMenuItem23.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem24.Name = "toolStripMenuItem24";
		this.toolStripMenuItem24.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem24.Text = "GER Demo_Female";
		this.toolStripMenuItem24.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.russianToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem25, this.toolStripMenuItem26, this.toolStripMenuItem27, this.toolStripMenuItem28, this.toolStripMenuItem29, this.toolStripMenuItem30 });
		this.russianToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("russianToolStripMenuItem.Image");
		this.russianToolStripMenuItem.Name = "russianToolStripMenuItem";
		this.russianToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.russianToolStripMenuItem.Text = "Russian";
		this.toolStripMenuItem25.Name = "toolStripMenuItem25";
		this.toolStripMenuItem25.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem25.Text = "RUS rus_ru_bank";
		this.toolStripMenuItem25.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem26.Name = "toolStripMenuItem26";
		this.toolStripMenuItem26.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem26.Text = "RUS Neutral";
		this.toolStripMenuItem26.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem27.Name = "toolStripMenuItem27";
		this.toolStripMenuItem27.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem27.Text = "RUS Female";
		this.toolStripMenuItem27.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem28.Name = "toolStripMenuItem28";
		this.toolStripMenuItem28.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem28.Text = "RUS Demo";
		this.toolStripMenuItem28.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem29.Name = "toolStripMenuItem29";
		this.toolStripMenuItem29.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem29.Text = "RUS Demo_Neutral";
		this.toolStripMenuItem29.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem30.Name = "toolStripMenuItem30";
		this.toolStripMenuItem30.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem30.Text = "RUS Demo_Female";
		this.toolStripMenuItem30.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.frenchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem31, this.toolStripMenuItem32, this.toolStripMenuItem33, this.toolStripMenuItem34, this.toolStripMenuItem35, this.toolStripMenuItem36 });
		this.frenchToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("frenchToolStripMenuItem.Image");
		this.frenchToolStripMenuItem.Name = "frenchToolStripMenuItem";
		this.frenchToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.frenchToolStripMenuItem.Text = "French";
		this.toolStripMenuItem31.Name = "toolStripMenuItem31";
		this.toolStripMenuItem31.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem31.Text = "FRA fre_fr_bank";
		this.toolStripMenuItem31.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem32.Name = "toolStripMenuItem32";
		this.toolStripMenuItem32.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem32.Text = "FRA Neutral";
		this.toolStripMenuItem32.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem33.Name = "toolStripMenuItem33";
		this.toolStripMenuItem33.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem33.Text = "FRA Female";
		this.toolStripMenuItem33.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem34.Name = "toolStripMenuItem34";
		this.toolStripMenuItem34.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem34.Text = "FRA Demo";
		this.toolStripMenuItem34.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem35.Name = "toolStripMenuItem35";
		this.toolStripMenuItem35.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem35.Text = "FRA Demo_Neutral";
		this.toolStripMenuItem35.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem36.Name = "toolStripMenuItem36";
		this.toolStripMenuItem36.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem36.Text = "FRA Demo_Female";
		this.toolStripMenuItem36.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.englishToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[12]
		{
			this.toolStripMenuItem38, this.toolStripMenuItem43, this.toolStripMenuItem44, this.toolStripMenuItem45, this.toolStripMenuItem47, this.toolStripMenuItem46, this.toolStripMenuItem37, this.toolStripMenuItem39, this.toolStripMenuItem40, this.toolStripMenuItem48,
			this.toolStripMenuItem41, this.toolStripMenuItem42
		});
		this.englishToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("englishToolStripMenuItem.Image");
		this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
		this.englishToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.englishToolStripMenuItem.Text = "English";
		this.toolStripMenuItem38.Name = "toolStripMenuItem38";
		this.toolStripMenuItem38.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem38.Text = "EN1 eng_us_bank_1";
		this.toolStripMenuItem38.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem43.Name = "toolStripMenuItem43";
		this.toolStripMenuItem43.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem43.Text = "EN1 eng_us_bank_2";
		this.toolStripMenuItem43.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem44.Name = "toolStripMenuItem44";
		this.toolStripMenuItem44.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem44.Text = "EN1 eng_us_bank_3";
		this.toolStripMenuItem44.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem45.Name = "toolStripMenuItem45";
		this.toolStripMenuItem45.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem45.Text = "EN2 eng_us_2_bank_1";
		this.toolStripMenuItem45.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem47.Name = "toolStripMenuItem47";
		this.toolStripMenuItem47.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem47.Text = "EN2 eng_us_2_bank_2";
		this.toolStripMenuItem47.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem46.Name = "toolStripMenuItem46";
		this.toolStripMenuItem46.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem46.Text = "EN2 eng_us_2_bank_3";
		this.toolStripMenuItem46.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem37.Name = "toolStripMenuItem37";
		this.toolStripMenuItem37.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem37.Text = "EN1 Neutral";
		this.toolStripMenuItem37.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem39.Name = "toolStripMenuItem39";
		this.toolStripMenuItem39.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem39.Text = "EN1 Female";
		this.toolStripMenuItem39.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem40.Name = "toolStripMenuItem40";
		this.toolStripMenuItem40.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem40.Text = "EN1 Demo";
		this.toolStripMenuItem40.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem48.Name = "toolStripMenuItem48";
		this.toolStripMenuItem48.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem48.Text = "EN2 Demo";
		this.toolStripMenuItem48.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem41.Name = "toolStripMenuItem41";
		this.toolStripMenuItem41.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem41.Text = "EN1 Demo_Neutral";
		this.toolStripMenuItem41.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.toolStripMenuItem42.Name = "toolStripMenuItem42";
		this.toolStripMenuItem42.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem42.Text = "EN2 Demo_Female";
		this.toolStripMenuItem42.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.buttonCloseSoundFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCloseSoundFile.Enabled = false;
		this.buttonCloseSoundFile.Image = (System.Drawing.Image)resources.GetObject("buttonCloseSoundFile.Image");
		this.buttonCloseSoundFile.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCloseSoundFile.Name = "buttonCloseSoundFile";
		this.buttonCloseSoundFile.Size = new System.Drawing.Size(23, 22);
		this.buttonCloseSoundFile.Text = "Close Sound File";
		this.buttonCloseSoundFile.Click += new System.EventHandler(buttonCloseSoundFile_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonSelectAllSounds.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSelectAllSounds.Enabled = false;
		this.buttonSelectAllSounds.Image = (System.Drawing.Image)resources.GetObject("buttonSelectAllSounds.Image");
		this.buttonSelectAllSounds.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSelectAllSounds.Name = "buttonSelectAllSounds";
		this.buttonSelectAllSounds.Size = new System.Drawing.Size(23, 22);
		this.buttonSelectAllSounds.Text = "Select All Sounds";
		this.buttonSelectAllSounds.Click += new System.EventHandler(buttonSelectAllSounds_Click);
		this.buttonDeselectAllSounds.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeselectAllSounds.Enabled = false;
		this.buttonDeselectAllSounds.Image = (System.Drawing.Image)resources.GetObject("buttonDeselectAllSounds.Image");
		this.buttonDeselectAllSounds.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeselectAllSounds.Name = "buttonDeselectAllSounds";
		this.buttonDeselectAllSounds.Size = new System.Drawing.Size(23, 22);
		this.buttonDeselectAllSounds.Text = "Deselect All Sounds";
		this.buttonDeselectAllSounds.Click += new System.EventHandler(buttonDeselectAllSounds_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonPlaySound2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPlaySound2.Enabled = false;
		this.buttonPlaySound2.Image = (System.Drawing.Image)resources.GetObject("buttonPlaySound2.Image");
		this.buttonPlaySound2.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPlaySound2.Name = "buttonPlaySound2";
		this.buttonPlaySound2.Size = new System.Drawing.Size(23, 22);
		this.buttonPlaySound2.Text = "Play Sounds";
		this.buttonPlaySound2.Click += new System.EventHandler(buttonPlaySound2_Click);
		this.buttonExportSps2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportSps2.Enabled = false;
		this.buttonExportSps2.Image = (System.Drawing.Image)resources.GetObject("buttonExportSps2.Image");
		this.buttonExportSps2.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportSps2.Name = "buttonExportSps2";
		this.buttonExportSps2.Size = new System.Drawing.Size(23, 22);
		this.buttonExportSps2.Text = "Export Sounds (.sps format)";
		this.buttonExportSps2.Click += new System.EventHandler(buttonExportSps2_Click);
		this.buttonExportWav2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportWav2.Enabled = false;
		this.buttonExportWav2.Image = (System.Drawing.Image)resources.GetObject("buttonExportWav2.Image");
		this.buttonExportWav2.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportWav2.Name = "buttonExportWav2";
		this.buttonExportWav2.Size = new System.Drawing.Size(23, 22);
		this.buttonExportWav2.Text = "Export Sounds (.wav format)";
		this.buttonExportWav2.Click += new System.EventHandler(buttonExportWav2_Click);
		this.comboSelctSoundGroup.Enabled = false;
		this.comboSelctSoundGroup.Items.AddRange(new object[2] { "Specific Names", "Generic Names" });
		this.comboSelctSoundGroup.Name = "comboSelctSoundGroup";
		this.comboSelctSoundGroup.Size = new System.Drawing.Size(121, 25);
		this.comboSelctSoundGroup.Text = "Specific Names";
		this.comboSelctSoundGroup.SelectedIndexChanged += new System.EventHandler(comboSelctSoundGroup_SelectedIndexChanged);
		this.toolStripSearchSound.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStripSearchSound.BackgroundImage");
		this.toolStripSearchSound.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripSearchSound.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripLabel2, this.textSearchExplore, this.toolStripSeparator3, this.buttonSearchExploreSoundExact, this.buttonSearchExploreSoundStarting, this.buttonSearchExploreSoundContaining });
		this.toolStripSearchSound.Location = new System.Drawing.Point(3, 16);
		this.toolStripSearchSound.Name = "toolStripSearchSound";
		this.toolStripSearchSound.Size = new System.Drawing.Size(331, 25);
		this.toolStripSearchSound.TabIndex = 130;
		this.toolStripLabel2.AutoSize = false;
		this.toolStripLabel2.Name = "toolStripLabel2";
		this.toolStripLabel2.Size = new System.Drawing.Size(50, 22);
		this.toolStripLabel2.Text = "Search";
		this.textSearchExplore.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textSearchExplore.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
		this.textSearchExplore.Name = "textSearchExplore";
		this.textSearchExplore.Size = new System.Drawing.Size(130, 25);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
		this.buttonSearchExploreSoundExact.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchExploreSoundExact.Image = (System.Drawing.Image)resources.GetObject("buttonSearchExploreSoundExact.Image");
		this.buttonSearchExploreSoundExact.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchExploreSoundExact.Name = "buttonSearchExploreSoundExact";
		this.buttonSearchExploreSoundExact.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchExploreSoundExact.Text = "Search Exactly";
		this.buttonSearchExploreSoundExact.Click += new System.EventHandler(buttonSearchSoundExact_Click);
		this.buttonSearchExploreSoundStarting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchExploreSoundStarting.Image = (System.Drawing.Image)resources.GetObject("buttonSearchExploreSoundStarting.Image");
		this.buttonSearchExploreSoundStarting.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchExploreSoundStarting.Name = "buttonSearchExploreSoundStarting";
		this.buttonSearchExploreSoundStarting.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchExploreSoundStarting.Text = "Search if starting with";
		this.buttonSearchExploreSoundStarting.Click += new System.EventHandler(buttonSearchSoundStarting_Click);
		this.buttonSearchExploreSoundContaining.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchExploreSoundContaining.Image = (System.Drawing.Image)resources.GetObject("buttonSearchExploreSoundContaining.Image");
		this.buttonSearchExploreSoundContaining.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchExploreSoundContaining.Name = "buttonSearchExploreSoundContaining";
		this.buttonSearchExploreSoundContaining.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchExploreSoundContaining.Text = "Search if containing";
		this.buttonSearchExploreSoundContaining.Click += new System.EventHandler(buttonSearchSoundContaining_Click);
		this.groupPatchAudio.Controls.Add(this.listViewPatchSounds);
		this.groupPatchAudio.Controls.Add(this.toolStrip2);
		this.groupPatchAudio.Controls.Add(this.toolStrip1);
		this.groupPatchAudio.Location = new System.Drawing.Point(666, 31);
		this.groupPatchAudio.Name = "groupPatchAudio";
		this.groupPatchAudio.Size = new System.Drawing.Size(337, 787);
		this.groupPatchAudio.TabIndex = 95;
		this.groupPatchAudio.TabStop = false;
		this.groupPatchAudio.Text = "Patching:";
		this.listViewPatchSounds.AllowColumnReorder = true;
		this.listViewPatchSounds.AllowDrop = true;
		this.listViewPatchSounds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.columnHeader3, this.columnHeader4 });
		this.listViewPatchSounds.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewPatchSounds.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewPatchSounds.FullRowSelect = true;
		this.listViewPatchSounds.GridLines = true;
		this.listViewPatchSounds.HideSelection = false;
		this.listViewPatchSounds.Location = new System.Drawing.Point(3, 66);
		this.listViewPatchSounds.MultiSelect = false;
		this.listViewPatchSounds.Name = "listViewPatchSounds";
		this.listViewPatchSounds.Size = new System.Drawing.Size(331, 718);
		this.listViewPatchSounds.TabIndex = 133;
		this.listViewPatchSounds.UseCompatibleStateImageBehavior = false;
		this.listViewPatchSounds.View = System.Windows.Forms.View.Details;
		this.listViewPatchSounds.SelectedIndexChanged += new System.EventHandler(listViewPatchSounds_SelectedIndexChanged);
		this.columnHeader3.Text = "N.";
		this.columnHeader3.Width = 66;
		this.columnHeader4.Text = "Name";
		this.columnHeader4.Width = 183;
		this.toolStrip2.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStrip2.BackgroundImage");
		this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[12]
		{
			this.buttonOpenPatchSound, this.buttonSavePatchedAudio, this.buttonClosePatchedAudio, this.toolStripSeparator5, this.buttonPlaySound, this.buttonExportSps, this.buttonExportWav, this.toolStripSeparator8, this.buttonImportSps, this.buttonImportWav,
			this.toolStripSeparator9, this.buttonDeleteSps
		});
		this.toolStrip2.Location = new System.Drawing.Point(3, 41);
		this.toolStrip2.Name = "toolStrip2";
		this.toolStrip2.Size = new System.Drawing.Size(331, 25);
		this.toolStrip2.TabIndex = 132;
		this.toolStrip2.Text = "toolStrip1";
		this.buttonOpenPatchSound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonOpenPatchSound.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.toolStripMenuItem49, this.toolStripMenuItem56, this.toolStripMenuItem63, this.toolStripMenuItem70, this.toolStripMenuItem77, this.toolStripMenuItem84, this.toolStripMenuItem91, this.toolStripMenuItem98, this.arabicToolStripMenuItem1 });
		this.buttonOpenPatchSound.Image = (System.Drawing.Image)resources.GetObject("buttonOpenPatchSound.Image");
		this.buttonOpenPatchSound.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonOpenPatchSound.Name = "buttonOpenPatchSound";
		this.buttonOpenPatchSound.Size = new System.Drawing.Size(32, 22);
		this.buttonOpenPatchSound.Text = "Open Sound File";
		this.toolStripMenuItem49.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem50, this.iTANeutralToolStripMenuItem });
		this.toolStripMenuItem49.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem49.Image");
		this.toolStripMenuItem49.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem49.Name = "toolStripMenuItem49";
		this.toolStripMenuItem49.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem49.Text = "Italian";
		this.toolStripMenuItem50.Name = "toolStripMenuItem50";
		this.toolStripMenuItem50.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem50.Text = "ITA ita_it_bank";
		this.toolStripMenuItem50.Visible = false;
		this.toolStripMenuItem50.Click += new System.EventHandler(bankOpenForPatch);
		this.iTANeutralToolStripMenuItem.Name = "iTANeutralToolStripMenuItem";
		this.iTANeutralToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.iTANeutralToolStripMenuItem.Text = "ITA Neutral";
		this.iTANeutralToolStripMenuItem.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem56.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem57, this.toolStripMenuItem58, this.toolStripMenuItem59, this.toolStripMenuItem60, this.toolStripMenuItem61, this.toolStripMenuItem62 });
		this.toolStripMenuItem56.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem56.Image");
		this.toolStripMenuItem56.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem56.Name = "toolStripMenuItem56";
		this.toolStripMenuItem56.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem56.Text = "Spanish";
		this.toolStripMenuItem57.Name = "toolStripMenuItem57";
		this.toolStripMenuItem57.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem57.Text = "SPA spa_es_bank";
		this.toolStripMenuItem57.Visible = false;
		this.toolStripMenuItem57.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem58.Name = "toolStripMenuItem58";
		this.toolStripMenuItem58.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem58.Text = "SPA Neutral";
		this.toolStripMenuItem58.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem59.Name = "toolStripMenuItem59";
		this.toolStripMenuItem59.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem59.Text = "SPA Female";
		this.toolStripMenuItem59.Visible = false;
		this.toolStripMenuItem60.Name = "toolStripMenuItem60";
		this.toolStripMenuItem60.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem60.Text = "SPA Demo";
		this.toolStripMenuItem60.Visible = false;
		this.toolStripMenuItem61.Name = "toolStripMenuItem61";
		this.toolStripMenuItem61.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem61.Text = "SPA Demo_Neutral";
		this.toolStripMenuItem61.Visible = false;
		this.toolStripMenuItem62.Name = "toolStripMenuItem62";
		this.toolStripMenuItem62.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem62.Text = "SPA Demo_Female";
		this.toolStripMenuItem62.Visible = false;
		this.toolStripMenuItem63.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem64, this.toolStripMenuItem65, this.toolStripMenuItem66, this.toolStripMenuItem67, this.toolStripMenuItem68, this.toolStripMenuItem69 });
		this.toolStripMenuItem63.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem63.Image");
		this.toolStripMenuItem63.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem63.Name = "toolStripMenuItem63";
		this.toolStripMenuItem63.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem63.Text = "Mexico";
		this.toolStripMenuItem64.Name = "toolStripMenuItem64";
		this.toolStripMenuItem64.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem64.Text = "MEX spa_mx_bank";
		this.toolStripMenuItem64.Visible = false;
		this.toolStripMenuItem64.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem65.Name = "toolStripMenuItem65";
		this.toolStripMenuItem65.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem65.Text = "MEX Neutral";
		this.toolStripMenuItem65.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem66.Name = "toolStripMenuItem66";
		this.toolStripMenuItem66.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem66.Text = "MEX Female";
		this.toolStripMenuItem66.Visible = false;
		this.toolStripMenuItem67.Name = "toolStripMenuItem67";
		this.toolStripMenuItem67.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem67.Text = "MEX Demo";
		this.toolStripMenuItem67.Visible = false;
		this.toolStripMenuItem68.Name = "toolStripMenuItem68";
		this.toolStripMenuItem68.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem68.Text = "MEX Demo_Neutral";
		this.toolStripMenuItem68.Visible = false;
		this.toolStripMenuItem69.Name = "toolStripMenuItem69";
		this.toolStripMenuItem69.Size = new System.Drawing.Size(177, 22);
		this.toolStripMenuItem69.Text = "MEX Demo_Female";
		this.toolStripMenuItem69.Visible = false;
		this.toolStripMenuItem70.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem71, this.toolStripMenuItem72, this.toolStripMenuItem73, this.toolStripMenuItem74, this.toolStripMenuItem75, this.toolStripMenuItem76 });
		this.toolStripMenuItem70.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem70.Image");
		this.toolStripMenuItem70.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem70.Name = "toolStripMenuItem70";
		this.toolStripMenuItem70.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem70.Text = "Portuguese";
		this.toolStripMenuItem71.Name = "toolStripMenuItem71";
		this.toolStripMenuItem71.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem71.Text = "BRA por_br_bank";
		this.toolStripMenuItem71.Visible = false;
		this.toolStripMenuItem71.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem72.Name = "toolStripMenuItem72";
		this.toolStripMenuItem72.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem72.Text = "BRA Neutral";
		this.toolStripMenuItem72.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem73.Name = "toolStripMenuItem73";
		this.toolStripMenuItem73.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem73.Text = "BRA Female";
		this.toolStripMenuItem73.Visible = false;
		this.toolStripMenuItem74.Name = "toolStripMenuItem74";
		this.toolStripMenuItem74.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem74.Text = "BRA Demo";
		this.toolStripMenuItem74.Visible = false;
		this.toolStripMenuItem75.Name = "toolStripMenuItem75";
		this.toolStripMenuItem75.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem75.Text = "BRA Demo_Neutral";
		this.toolStripMenuItem75.Visible = false;
		this.toolStripMenuItem76.Name = "toolStripMenuItem76";
		this.toolStripMenuItem76.Size = new System.Drawing.Size(175, 22);
		this.toolStripMenuItem76.Text = "BRA Demo_Female";
		this.toolStripMenuItem76.Visible = false;
		this.toolStripMenuItem77.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem78, this.toolStripMenuItem79, this.toolStripMenuItem80, this.toolStripMenuItem81, this.toolStripMenuItem82, this.toolStripMenuItem83 });
		this.toolStripMenuItem77.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem77.Image");
		this.toolStripMenuItem77.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem77.Name = "toolStripMenuItem77";
		this.toolStripMenuItem77.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem77.Text = "German";
		this.toolStripMenuItem78.Name = "toolStripMenuItem78";
		this.toolStripMenuItem78.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem78.Text = "GER ger_de_bank";
		this.toolStripMenuItem78.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem79.Name = "toolStripMenuItem79";
		this.toolStripMenuItem79.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem79.Text = "GER Neutral";
		this.toolStripMenuItem79.Visible = false;
		this.toolStripMenuItem79.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem80.Name = "toolStripMenuItem80";
		this.toolStripMenuItem80.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem80.Text = "GER Female";
		this.toolStripMenuItem80.Visible = false;
		this.toolStripMenuItem81.Name = "toolStripMenuItem81";
		this.toolStripMenuItem81.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem81.Text = "GER Demo";
		this.toolStripMenuItem81.Visible = false;
		this.toolStripMenuItem82.Name = "toolStripMenuItem82";
		this.toolStripMenuItem82.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem82.Text = "GER Demo_Neutral";
		this.toolStripMenuItem82.Visible = false;
		this.toolStripMenuItem83.Name = "toolStripMenuItem83";
		this.toolStripMenuItem83.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem83.Text = "GER Demo_Female";
		this.toolStripMenuItem83.Visible = false;
		this.toolStripMenuItem84.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem85, this.toolStripMenuItem86, this.toolStripMenuItem87, this.toolStripMenuItem88, this.toolStripMenuItem89, this.toolStripMenuItem90 });
		this.toolStripMenuItem84.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem84.Image");
		this.toolStripMenuItem84.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem84.Name = "toolStripMenuItem84";
		this.toolStripMenuItem84.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem84.Text = "Russian";
		this.toolStripMenuItem85.Name = "toolStripMenuItem85";
		this.toolStripMenuItem85.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem85.Text = "RUS rus_ru_bank";
		this.toolStripMenuItem85.Visible = false;
		this.toolStripMenuItem85.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem86.Name = "toolStripMenuItem86";
		this.toolStripMenuItem86.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem86.Text = "RUS Neutral";
		this.toolStripMenuItem86.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem87.Name = "toolStripMenuItem87";
		this.toolStripMenuItem87.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem87.Text = "RUS Female";
		this.toolStripMenuItem87.Visible = false;
		this.toolStripMenuItem88.Name = "toolStripMenuItem88";
		this.toolStripMenuItem88.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem88.Text = "RUS Demo";
		this.toolStripMenuItem88.Visible = false;
		this.toolStripMenuItem89.Name = "toolStripMenuItem89";
		this.toolStripMenuItem89.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem89.Text = "RUS Demo_Neutral";
		this.toolStripMenuItem89.Visible = false;
		this.toolStripMenuItem90.Name = "toolStripMenuItem90";
		this.toolStripMenuItem90.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem90.Text = "RUS Demo_Female";
		this.toolStripMenuItem90.Visible = false;
		this.toolStripMenuItem91.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem92, this.toolStripMenuItem93, this.toolStripMenuItem94, this.toolStripMenuItem95, this.toolStripMenuItem96, this.toolStripMenuItem97 });
		this.toolStripMenuItem91.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem91.Image");
		this.toolStripMenuItem91.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem91.Name = "toolStripMenuItem91";
		this.toolStripMenuItem91.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem91.Text = "French";
		this.toolStripMenuItem92.Name = "toolStripMenuItem92";
		this.toolStripMenuItem92.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem92.Text = "FRA fre_fr_bank";
		this.toolStripMenuItem92.Visible = false;
		this.toolStripMenuItem92.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem93.Name = "toolStripMenuItem93";
		this.toolStripMenuItem93.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem93.Text = "FRA Neutral";
		this.toolStripMenuItem93.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem94.Name = "toolStripMenuItem94";
		this.toolStripMenuItem94.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem94.Text = "FRA Female";
		this.toolStripMenuItem94.Visible = false;
		this.toolStripMenuItem95.Name = "toolStripMenuItem95";
		this.toolStripMenuItem95.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem95.Text = "FRA Demo";
		this.toolStripMenuItem95.Visible = false;
		this.toolStripMenuItem96.Name = "toolStripMenuItem96";
		this.toolStripMenuItem96.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem96.Text = "FRA Demo_Neutral";
		this.toolStripMenuItem96.Visible = false;
		this.toolStripMenuItem97.Name = "toolStripMenuItem97";
		this.toolStripMenuItem97.Size = new System.Drawing.Size(174, 22);
		this.toolStripMenuItem97.Text = "FRA Demo_Female";
		this.toolStripMenuItem97.Visible = false;
		this.toolStripMenuItem98.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[12]
		{
			this.toolStripMenuItem99, this.toolStripMenuItem100, this.toolStripMenuItem101, this.toolStripMenuItem102, this.toolStripMenuItem103, this.toolStripMenuItem104, this.toolStripMenuItem105, this.toolStripMenuItem106, this.toolStripMenuItem107, this.toolStripMenuItem108,
			this.toolStripMenuItem109, this.toolStripMenuItem110
		});
		this.toolStripMenuItem98.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem98.Image");
		this.toolStripMenuItem98.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.toolStripMenuItem98.Name = "toolStripMenuItem98";
		this.toolStripMenuItem98.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem98.Text = "English";
		this.toolStripMenuItem99.Name = "toolStripMenuItem99";
		this.toolStripMenuItem99.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem99.Text = "EN1 eng_us_bank_1";
		this.toolStripMenuItem99.Visible = false;
		this.toolStripMenuItem99.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem100.Name = "toolStripMenuItem100";
		this.toolStripMenuItem100.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem100.Text = "EN1 eng_us_bank_2";
		this.toolStripMenuItem100.Visible = false;
		this.toolStripMenuItem100.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem101.Name = "toolStripMenuItem101";
		this.toolStripMenuItem101.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem101.Text = "EN1 eng_us_bank_3";
		this.toolStripMenuItem101.Visible = false;
		this.toolStripMenuItem101.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem102.Name = "toolStripMenuItem102";
		this.toolStripMenuItem102.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem102.Text = "EN2 eng_us_2_bank_1";
		this.toolStripMenuItem102.Visible = false;
		this.toolStripMenuItem102.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem103.Name = "toolStripMenuItem103";
		this.toolStripMenuItem103.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem103.Text = "EN2 eng_us_2_bank_2";
		this.toolStripMenuItem103.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem104.Name = "toolStripMenuItem104";
		this.toolStripMenuItem104.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem104.Text = "EN2 eng_us_2_bank_3";
		this.toolStripMenuItem104.Visible = false;
		this.toolStripMenuItem104.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem105.Name = "toolStripMenuItem105";
		this.toolStripMenuItem105.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem105.Text = "EN1 Neutral";
		this.toolStripMenuItem105.Click += new System.EventHandler(bankOpenForPatch);
		this.toolStripMenuItem106.Name = "toolStripMenuItem106";
		this.toolStripMenuItem106.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem106.Text = "EN1 Female";
		this.toolStripMenuItem106.Visible = false;
		this.toolStripMenuItem107.Name = "toolStripMenuItem107";
		this.toolStripMenuItem107.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem107.Text = "EN1 Demo";
		this.toolStripMenuItem107.Visible = false;
		this.toolStripMenuItem108.Name = "toolStripMenuItem108";
		this.toolStripMenuItem108.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem108.Text = "EN2 Demo";
		this.toolStripMenuItem108.Visible = false;
		this.toolStripMenuItem109.Name = "toolStripMenuItem109";
		this.toolStripMenuItem109.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem109.Text = "EN1 Demo_Neutral";
		this.toolStripMenuItem109.Visible = false;
		this.toolStripMenuItem110.Name = "toolStripMenuItem110";
		this.toolStripMenuItem110.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem110.Text = "EN2 Demo_Female";
		this.toolStripMenuItem110.Visible = false;
		this.buttonSavePatchedAudio.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSavePatchedAudio.Enabled = false;
		this.buttonSavePatchedAudio.Image = (System.Drawing.Image)resources.GetObject("buttonSavePatchedAudio.Image");
		this.buttonSavePatchedAudio.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSavePatchedAudio.Name = "buttonSavePatchedAudio";
		this.buttonSavePatchedAudio.Size = new System.Drawing.Size(23, 22);
		this.buttonSavePatchedAudio.Text = "Save Audio Files";
		this.buttonSavePatchedAudio.Click += new System.EventHandler(buttonSavePatchedAudio_Click);
		this.buttonClosePatchedAudio.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonClosePatchedAudio.Enabled = false;
		this.buttonClosePatchedAudio.Image = (System.Drawing.Image)resources.GetObject("buttonClosePatchedAudio.Image");
		this.buttonClosePatchedAudio.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonClosePatchedAudio.Name = "buttonClosePatchedAudio";
		this.buttonClosePatchedAudio.Size = new System.Drawing.Size(23, 22);
		this.buttonClosePatchedAudio.Text = "Close Audio File";
		this.buttonClosePatchedAudio.Click += new System.EventHandler(buttonClosePatchedAudio_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
		this.buttonPlaySound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPlaySound.Enabled = false;
		this.buttonPlaySound.Image = (System.Drawing.Image)resources.GetObject("buttonPlaySound.Image");
		this.buttonPlaySound.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPlaySound.Name = "buttonPlaySound";
		this.buttonPlaySound.Size = new System.Drawing.Size(23, 22);
		this.buttonPlaySound.Text = "Play Sounds";
		this.buttonPlaySound.Click += new System.EventHandler(buttonPlaySound_Click);
		this.buttonExportSps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportSps.Enabled = false;
		this.buttonExportSps.Image = (System.Drawing.Image)resources.GetObject("buttonExportSps.Image");
		this.buttonExportSps.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportSps.Name = "buttonExportSps";
		this.buttonExportSps.Size = new System.Drawing.Size(23, 22);
		this.buttonExportSps.Text = "Export Sounds (.sps format)";
		this.buttonExportSps.Click += new System.EventHandler(buttonExportSps_Click);
		this.buttonExportWav.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportWav.Enabled = false;
		this.buttonExportWav.Image = (System.Drawing.Image)resources.GetObject("buttonExportWav.Image");
		this.buttonExportWav.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportWav.Name = "buttonExportWav";
		this.buttonExportWav.Size = new System.Drawing.Size(23, 22);
		this.buttonExportWav.Text = "Export Sounds (.wav format)";
		this.buttonExportWav.Click += new System.EventHandler(buttonExportWav_Click);
		this.toolStripSeparator8.Name = "toolStripSeparator8";
		this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
		this.buttonImportSps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportSps.Enabled = false;
		this.buttonImportSps.Image = (System.Drawing.Image)resources.GetObject("buttonImportSps.Image");
		this.buttonImportSps.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportSps.Name = "buttonImportSps";
		this.buttonImportSps.Size = new System.Drawing.Size(23, 22);
		this.buttonImportSps.Text = "Import Sounds (.sps format)";
		this.buttonImportSps.Click += new System.EventHandler(buttonImportSps_Click);
		this.buttonImportWav.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportWav.Enabled = false;
		this.buttonImportWav.Image = (System.Drawing.Image)resources.GetObject("buttonImportWav.Image");
		this.buttonImportWav.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportWav.Name = "buttonImportWav";
		this.buttonImportWav.Size = new System.Drawing.Size(23, 22);
		this.buttonImportWav.Text = "Import Sounds (.wav format)";
		this.buttonImportWav.Click += new System.EventHandler(buttonImportWav_Click);
		this.toolStripSeparator9.Name = "toolStripSeparator9";
		this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
		this.buttonDeleteSps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteSps.Enabled = false;
		this.buttonDeleteSps.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteSps.Image");
		this.buttonDeleteSps.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteSps.Name = "buttonDeleteSps";
		this.buttonDeleteSps.Size = new System.Drawing.Size(23, 22);
		this.buttonDeleteSps.Text = "Delete Sounds";
		this.buttonDeleteSps.Click += new System.EventHandler(buttonDeleteSoundPatch_Click);
		this.toolStrip1.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStrip1.BackgroundImage");
		this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.toolStripLabel3, this.textSearchPatch, this.toolStripSeparator4, this.buttonSearchPatchSoundExact, this.buttonSearchPatchSoundStarting, this.buttonSearchPatchSoundContaining, this.buttonSearchPatchSoundEnding });
		this.toolStrip1.Location = new System.Drawing.Point(3, 16);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(331, 25);
		this.toolStrip1.TabIndex = 131;
		this.toolStripLabel3.AutoSize = false;
		this.toolStripLabel3.Name = "toolStripLabel3";
		this.toolStripLabel3.Size = new System.Drawing.Size(70, 22);
		this.toolStripLabel3.Text = "Search";
		this.textSearchPatch.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textSearchPatch.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
		this.textSearchPatch.Name = "textSearchPatch";
		this.textSearchPatch.Size = new System.Drawing.Size(130, 25);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.buttonSearchPatchSoundExact.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchPatchSoundExact.Image = (System.Drawing.Image)resources.GetObject("buttonSearchPatchSoundExact.Image");
		this.buttonSearchPatchSoundExact.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchPatchSoundExact.Name = "buttonSearchPatchSoundExact";
		this.buttonSearchPatchSoundExact.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchPatchSoundExact.Text = "Search Exactly";
		this.buttonSearchPatchSoundExact.Click += new System.EventHandler(buttonSearchPatchSoundExact_Click);
		this.buttonSearchPatchSoundStarting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchPatchSoundStarting.Image = (System.Drawing.Image)resources.GetObject("buttonSearchPatchSoundStarting.Image");
		this.buttonSearchPatchSoundStarting.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchPatchSoundStarting.Name = "buttonSearchPatchSoundStarting";
		this.buttonSearchPatchSoundStarting.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchPatchSoundStarting.Text = "Search if starting with";
		this.buttonSearchPatchSoundStarting.Click += new System.EventHandler(buttonSearchPatchSoundStarting_Click);
		this.buttonSearchPatchSoundContaining.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchPatchSoundContaining.Image = (System.Drawing.Image)resources.GetObject("buttonSearchPatchSoundContaining.Image");
		this.buttonSearchPatchSoundContaining.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchPatchSoundContaining.Name = "buttonSearchPatchSoundContaining";
		this.buttonSearchPatchSoundContaining.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchPatchSoundContaining.Text = "Search if containing";
		this.buttonSearchPatchSoundContaining.Click += new System.EventHandler(buttonSearchPatchSoundContaining_Click);
		this.buttonSearchPatchSoundEnding.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchPatchSoundEnding.Image = (System.Drawing.Image)resources.GetObject("buttonSearchPatchSoundEnding.Image");
		this.buttonSearchPatchSoundEnding.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchPatchSoundEnding.Name = "buttonSearchPatchSoundEnding";
		this.buttonSearchPatchSoundEnding.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchPatchSoundEnding.Text = "Search if ending";
		this.buttonSearchPatchSoundEnding.Click += new System.EventHandler(buttonSearchPatchSoundEnding_Click);
		this.groupSoundEditing.Controls.Add(this.listViewSound2);
		this.groupSoundEditing.Controls.Add(this.labelSound2);
		this.groupSoundEditing.Controls.Add(this.listViewSound1);
		this.groupSoundEditing.Controls.Add(this.labelSound1);
		this.groupSoundEditing.Controls.Add(this.toolStrip4);
		this.groupSoundEditing.Location = new System.Drawing.Point(1009, 34);
		this.groupSoundEditing.Name = "groupSoundEditing";
		this.groupSoundEditing.Size = new System.Drawing.Size(199, 787);
		this.groupSoundEditing.TabIndex = 96;
		this.groupSoundEditing.TabStop = false;
		this.groupSoundEditing.Text = "Sound Split and Merge";
		this.groupSoundEditing.Visible = false;
		this.listViewSound2.AllowDrop = true;
		this.listViewSound2.CheckBoxes = true;
		this.listViewSound2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.columnHeader9 });
		this.listViewSound2.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewSound2.Dock = System.Windows.Forms.DockStyle.Top;
		this.listViewSound2.FullRowSelect = true;
		this.listViewSound2.GridLines = true;
		this.listViewSound2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.listViewSound2.HideSelection = false;
		this.listViewSound2.Location = new System.Drawing.Point(3, 308);
		this.listViewSound2.MultiSelect = false;
		this.listViewSound2.Name = "listViewSound2";
		this.listViewSound2.Size = new System.Drawing.Size(193, 217);
		this.listViewSound2.TabIndex = 139;
		this.listViewSound2.UseCompatibleStateImageBehavior = false;
		this.listViewSound2.View = System.Windows.Forms.View.Details;
		this.columnHeader9.Text = "msec.";
		this.columnHeader9.Width = 169;
		this.labelSound2.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelSound2.Location = new System.Drawing.Point(3, 283);
		this.labelSound2.Name = "labelSound2";
		this.labelSound2.Size = new System.Drawing.Size(193, 25);
		this.labelSound2.TabIndex = 136;
		this.labelSound2.Text = "Sound 2: ";
		this.labelSound2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.listViewSound1.AllowColumnReorder = true;
		this.listViewSound1.AllowDrop = true;
		this.listViewSound1.CheckBoxes = true;
		this.listViewSound1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.columnHeader6 });
		this.listViewSound1.Cursor = System.Windows.Forms.Cursors.Hand;
		this.listViewSound1.Dock = System.Windows.Forms.DockStyle.Top;
		this.listViewSound1.FullRowSelect = true;
		this.listViewSound1.GridLines = true;
		this.listViewSound1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.listViewSound1.HideSelection = false;
		this.listViewSound1.Location = new System.Drawing.Point(3, 66);
		this.listViewSound1.MultiSelect = false;
		this.listViewSound1.Name = "listViewSound1";
		this.listViewSound1.Size = new System.Drawing.Size(193, 217);
		this.listViewSound1.TabIndex = 140;
		this.listViewSound1.UseCompatibleStateImageBehavior = false;
		this.listViewSound1.View = System.Windows.Forms.View.Details;
		this.columnHeader6.Text = "msec.";
		this.columnHeader6.Width = 157;
		this.labelSound1.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelSound1.Location = new System.Drawing.Point(3, 41);
		this.labelSound1.Name = "labelSound1";
		this.labelSound1.Size = new System.Drawing.Size(193, 25);
		this.labelSound1.TabIndex = 135;
		this.labelSound1.Text = "Sound 1: ";
		this.labelSound1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolStrip4.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStrip4.BackgroundImage");
		this.toolStrip4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.buttonOpenSound1, this.buttonOpenSound2, this.buttonSaveEditedSound });
		this.toolStrip4.Location = new System.Drawing.Point(3, 16);
		this.toolStrip4.Name = "toolStrip4";
		this.toolStrip4.Size = new System.Drawing.Size(193, 25);
		this.toolStrip4.TabIndex = 133;
		this.toolStrip4.Text = "toolStrip1";
		this.buttonOpenSound1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonOpenSound1.Image = (System.Drawing.Image)resources.GetObject("buttonOpenSound1.Image");
		this.buttonOpenSound1.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonOpenSound1.Name = "buttonOpenSound1";
		this.buttonOpenSound1.Size = new System.Drawing.Size(23, 22);
		this.buttonOpenSound1.Text = "Open Sound 1";
		this.buttonOpenSound1.Click += new System.EventHandler(buttonOpenSound1_Click);
		this.buttonOpenSound2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonOpenSound2.Image = (System.Drawing.Image)resources.GetObject("buttonOpenSound2.Image");
		this.buttonOpenSound2.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonOpenSound2.Name = "buttonOpenSound2";
		this.buttonOpenSound2.Size = new System.Drawing.Size(23, 22);
		this.buttonOpenSound2.Text = "Close Audio File";
		this.buttonOpenSound2.Click += new System.EventHandler(buttonOpenSound2_Click);
		this.buttonSaveEditedSound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSaveEditedSound.Enabled = false;
		this.buttonSaveEditedSound.Image = (System.Drawing.Image)resources.GetObject("buttonSaveEditedSound.Image");
		this.buttonSaveEditedSound.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSaveEditedSound.Name = "buttonSaveEditedSound";
		this.buttonSaveEditedSound.Size = new System.Drawing.Size(23, 22);
		this.buttonSaveEditedSound.Text = "Save Audio Files";
		this.buttonSaveEditedSound.Click += new System.EventHandler(buttonSaveEditedSound_Click);
		this.arabicToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.aRAArasabankToolStripMenuItem, this.aRANeutralToolStripMenuItem, this.aRADemoToolStripMenuItem });
		this.arabicToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("arabicToolStripMenuItem.Image");
		this.arabicToolStripMenuItem.Name = "arabicToolStripMenuItem";
		this.arabicToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.arabicToolStripMenuItem.Text = "Arabic";
		this.arabicToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.menuAraNeutral });
		this.arabicToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("arabicToolStripMenuItem1.Image");
		this.arabicToolStripMenuItem1.Name = "arabicToolStripMenuItem1";
		this.arabicToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
		this.arabicToolStripMenuItem1.Text = "Arabic";
		this.menuAraNeutral.Name = "menuAraNeutral";
		this.menuAraNeutral.Size = new System.Drawing.Size(180, 22);
		this.menuAraNeutral.Text = "ARA neutral";
		this.menuAraNeutral.Click += new System.EventHandler(bankOpenForPatch);
		this.aRAArasabankToolStripMenuItem.Name = "aRAArasabankToolStripMenuItem";
		this.aRAArasabankToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.aRAArasabankToolStripMenuItem.Text = "ARA ara_sa_bank";
		this.aRAArasabankToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.aRANeutralToolStripMenuItem.Name = "aRANeutralToolStripMenuItem";
		this.aRANeutralToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.aRANeutralToolStripMenuItem.Text = "ARA neutral";
		this.aRANeutralToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.aRADemoToolStripMenuItem.Name = "aRADemoToolStripMenuItem";
		this.aRADemoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.aRADemoToolStripMenuItem.Text = "ARA demo";
		this.aRADemoToolStripMenuItem.Click += new System.EventHandler(bankToolStripMenuItem_Click);
		this.viewer2DPhoto.AutoTransparency = false;
		this.viewer2DPhoto.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DPhoto.ButtonStripVisible = false;
		this.viewer2DPhoto.CurrentBitmap = null;
		this.viewer2DPhoto.ExtendedFormat = false;
		this.viewer2DPhoto.FullSizeButton = false;
		this.viewer2DPhoto.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DPhoto.ImageSize = new System.Drawing.Size(128, 128);
		this.viewer2DPhoto.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.MiniFace;
		this.viewer2DPhoto.Location = new System.Drawing.Point(6, 14);
		this.viewer2DPhoto.Name = "viewer2DPhoto";
		this.viewer2DPhoto.RemoveButton = false;
		this.viewer2DPhoto.ShowButton = false;
		this.viewer2DPhoto.ShowButtonChecked = true;
		this.viewer2DPhoto.Size = new System.Drawing.Size(128, 128);
		this.viewer2DPhoto.TabIndex = 167;
		this.viewer2DPhoto.TabStop = false;
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = false;
		this.pickUpControl.CreateButtonEnabled = false;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = new string[4] { "All", "by Team", "by Country", "Free Agents" };
		this.pickUpControl.FilterEnabled = true;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = true;
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = false;
		this.pickUpControl.SearchEnabled = true;
		this.pickUpControl.Size = new System.Drawing.Size(1310, 25);
		this.pickUpControl.TabIndex = 1;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1310, 824);
		base.Controls.Add(this.groupSoundEditing);
		base.Controls.Add(this.groupPatchAudio);
		base.Controls.Add(this.groupExploreAudio);
		base.Controls.Add(this.groupPlayerInfo);
		base.Controls.Add(this.groupNameDictionary);
		base.Controls.Add(this.groupAudio);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "AudioForm";
		this.Text = "aUDIOForm";
		base.Load += new System.EventHandler(AudioForm_Load);
		this.groupAudio.ResumeLayout(false);
		this.groupAudio.PerformLayout();
		this.groupNameDictionary.ResumeLayout(false);
		this.groupNameDictionary.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNameDictionary).EndInit();
		this.toolStripNameDictionary.ResumeLayout(false);
		this.toolStripNameDictionary.PerformLayout();
		this.toolStripSearchnameDictionary.ResumeLayout(false);
		this.toolStripSearchnameDictionary.PerformLayout();
		this.groupPlayerInfo.ResumeLayout(false);
		this.groupPlayerInfo.PerformLayout();
		this.groupExploreAudio.ResumeLayout(false);
		this.groupExploreAudio.PerformLayout();
		this.toolStripExploreExistingSounds.ResumeLayout(false);
		this.toolStripExploreExistingSounds.PerformLayout();
		this.toolStripSearchSound.ResumeLayout(false);
		this.toolStripSearchSound.PerformLayout();
		this.groupPatchAudio.ResumeLayout(false);
		this.groupPatchAudio.PerformLayout();
		this.toolStrip2.ResumeLayout(false);
		this.toolStrip2.PerformLayout();
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		this.groupSoundEditing.ResumeLayout(false);
		this.groupSoundEditing.PerformLayout();
		this.toolStrip4.ResumeLayout(false);
		this.toolStrip4.PerformLayout();
		base.ResumeLayout(false);
	}
}
