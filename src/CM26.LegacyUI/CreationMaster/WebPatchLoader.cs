using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

public class WebPatchLoader : Form
{
	private DataTable m_WebData;

	private Bitmap m_Picture;

	private List<Bitmap> m_WebPictures;

	private int m_nTeams;

	private int m_nPlayers;

	private int m_nManagers;

	private PatchedObject m_CurrentPatchedObject;

	private IContainer components;

	private ToolStrip toolMain;

	private ToolStripButton buttonExitCreator;

	private ToolStripButton buttonSelectAllObjects;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripButton buttonDeselectAllObjects;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripButton buttonSelectNewObjects;

	private ListView listViewPatch;

	private ColumnHeader columnItem;

	private ColumnHeader columnType;

	private ColumnHeader columnImportId;

	private SplitContainer splitContainer1;

	private GroupBox groupReplaceSelection;

	private ComboBox comboReplacePlayer;

	private ComboBox comboReplaceTeam;

	private RadioButton radioReplaceItem;

	private RadioButton radioCreateItem;

	private Label labelCmsCreated;

	private Label labelCmsReplaced;

	private TextBox textCmsReplaced;

	private RadioButton radioCmsItem;

	private ToolStripButton buttonImportPatch;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripSeparator toolStripSeparator1;

	private GroupBox groupPatchOptions;

	private RadioButton radioMinifaceNever;

	private RadioButton radioMinifaceNotPresent;

	private RadioButton radioMinifaceAlways;

	private PictureBox pictureBox;

	private GroupBox groupMinifaceOptions;

	private ToolStripButton buttonSelectIfExisting;

	private CheckBox checkManagersPicture;

	private CheckBox checkPlayersPicture;

	private CheckBox checkTeamsPicture;

	public WebPatchLoader()
	{
		InitializeComponent();
	}

	public new bool Load(DataTable dataTable, List<Bitmap> pictures)
	{
		m_WebData = dataTable;
		m_WebPictures = pictures;
		pictureBox.BackgroundImage = ((pictures.Count > 0) ? pictures[0] : null);
		comboReplaceTeam.Items.Clear();
		comboReplaceTeam.Items.AddRange(FifaEnvironment.Teams.ToArray());
		comboReplaceTeam.Sorted = true;
		comboReplacePlayer.Items.Clear();
		comboReplacePlayer.Items.AddRange(FifaEnvironment.Players.ToArray());
		comboReplacePlayer.Sorted = true;
		listViewPatch.Items.Clear();
		foreach (DataRow row in m_WebData.Rows)
		{
			string[] array = new string[3]
			{
				(string)row["name"],
				(string)row["type"],
				(string)row["id"]
			};
			int id = Convert.ToInt32(array[2]);
			PatchedObject patchedObject = new PatchedObject(array[1], array[0], id);
			patchedObject.AssignReplacedObject();
			ListViewItem listViewItem = new ListViewItem(array);
			listViewItem.Tag = patchedObject;
			listViewItem.ForeColor = patchedObject.GetColor();
			listViewItem.Checked = true;
			listViewPatch.Items.Add(listViewItem);
			if (array[1] == "Team")
			{
				m_nTeams++;
			}
			else if (array[1] == "Player")
			{
				m_nPlayers++;
			}
			else if (array[1] == "Manager")
			{
				m_nManagers++;
			}
		}
		if (listViewPatch.Items[0].SubItems[1].Text == "Team")
		{
			groupMinifaceOptions.Visible = true;
		}
		else if (listViewPatch.Items[0].SubItems[1].Text == "Player")
		{
			groupMinifaceOptions.Visible = true;
		}
		else
		{
			groupMinifaceOptions.Visible = true;
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
			item2.SubItems[2] = new ListViewItem.ListViewSubItem(item2, patchedObject2.ImportId.ToString());
		}
		listViewPatch.Items[0].Selected = true;
		buttonImportPatch.Enabled = true;
		return true;
	}

	private void buttonExitCreator_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void RemoveNewObjectsNotImported()
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			((PatchedObject)item.Tag).RemoveNewObjectIfNotImported();
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

	private void buttonSelectAllObjects_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			item.Checked = true;
		}
	}

	private void buttonSelectNewObjects_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			item.Checked = item.ForeColor == Color.Green;
		}
	}

	private void buttonDeselectAllObjects_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			item.Checked = false;
		}
	}

	private void listViewPatch_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listViewPatch.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			int num = listViewPatch.SelectedIndices[0];
			if (num < m_WebPictures.Count)
			{
				pictureBox.BackgroundImage = m_WebPictures[num];
			}
			m_CurrentPatchedObject = (PatchedObject)listViewItem.Tag;
			radioCreateItem.Checked = m_CurrentPatchedObject.IsUsedNewObject();
			radioReplaceItem.Checked = m_CurrentPatchedObject.IsUsedFittingObject();
			radioCmsItem.Checked = m_CurrentPatchedObject.IsUsedCmsObject();
			UpdateComboReplace(m_CurrentPatchedObject);
			UpdateTextCms(m_CurrentPatchedObject);
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
			listViewItem.SubItems[2] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
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
			listViewItem.SubItems[2] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
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
			listViewItem.SubItems[2] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
			UpdateComboReplace(patchedObject);
			UpdateTextCms(patchedObject);
		}
	}

	private void comboReplace_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listViewPatch.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listViewPatch.SelectedItems[0];
			PatchedObject patchedObject = (PatchedObject)listViewItem.Tag;
			ComboBox comboBox = (ComboBox)sender;
			patchedObject.ReplacedObject = comboBox.SelectedItem;
			listViewItem.SubItems[2] = new ListViewItem.ListViewSubItem(listViewItem, patchedObject.ImportId.ToString());
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

	private void WebPatchLoader_FormClosing(object sender, FormClosingEventArgs e)
	{
		RemoveNewObjectsNotImported();
	}

	private void buttonImportPatch_Click(object sender, EventArgs e)
	{
		ImportWebPatch();
	}

	private void ImportWebPatch()
	{
		RemoveAllUnusedObjects();
		Team team = null;
		Player[] array = new Player[m_nPlayers];
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		foreach (ListViewItem item in listViewPatch.Items)
		{
			int index = item.Index;
			PatchedObject patchedObject = (PatchedObject)item.Tag;
			if (item.Checked)
			{
				if (patchedObject.Type == "Team")
				{
					if (team != null && num2 > 0)
					{
						num = num2;
						AdjustRoster(team, array, num);
					}
					team = null;
					num2 = 0;
					team = patchedObject.ImportWebTeam(m_WebData.Rows[index]);
					flag = true;
					num2 = 0;
					if ((radioMinifaceAlways.Checked || (radioMinifaceNotPresent.Checked && team.GetCrest() == null)) && checkTeamsPicture.Checked)
					{
						team.SetAllCrests(m_WebPictures[index]);
					}
					if (!patchedObject.IsObjectUsedNew())
					{
					}
				}
				else if (patchedObject.Type == "Player")
				{
					DataRow dataRow = m_WebData.Rows[index];
					Player player = null;
					if (dataRow["website"].ToString() == "transfermrkt" || dataRow["website"].ToString() == "sofifa")
					{
						player = (array[num2] = patchedObject.ImportWebPlayer(m_WebData.Rows[index], team));
					}
					else if (dataRow["website"].ToString() == "sortitoutsi")
					{
						player = patchedObject.GetPlayerToImport();
					}
					if (player != null)
					{
						bool flag3 = false;
						if (radioMinifaceAlways.Checked)
						{
							flag3 = checkPlayersPicture.Checked;
						}
						if (radioMinifaceNotPresent.Checked)
						{
							flag3 = player.GetPhoto() == null && checkPlayersPicture.Checked;
						}
						if (flag3 && index < m_WebPictures.Count)
						{
							Bitmap bitmap = m_WebPictures[index];
							if (bitmap != null)
							{
								int num3 = bitmap.Width * 128 / bitmap.Height;
								bitmap = GraphicUtil.ResizeBitmap(bitmap, num3, 128, InterpolationMode.HighQualityBicubic);
								bitmap = GraphicUtil.CanvasSizeBitmap(bitmap, 128, 128);
								player.SetPhoto(bitmap);
							}
						}
						num2++;
						flag2 = true;
						if (!patchedObject.IsObjectUsedNew())
						{
						}
					}
					else
					{
						listViewPatch.Items[index].Checked = false;
					}
				}
				else if (!(patchedObject.Type == "Manager"))
				{
				}
			}
			else if (patchedObject.Type == "Team")
			{
				if (team != null && num2 > 0)
				{
					num = num2;
					AdjustRoster(team, array, num);
				}
				team = null;
				num2 = 0;
			}
		}
		if (flag)
		{
			FifaEnvironment.Teams.SortId();
		}
		if (flag2)
		{
			FifaEnvironment.Players.SortId();
		}
		if (team != null && num2 > 0)
		{
			num = num2;
			AdjustRoster(team, array, num);
		}
		Close();
	}

	private void buttonSelectIfExisting_Click(object sender, EventArgs e)
	{
		foreach (ListViewItem item in listViewPatch.Items)
		{
			item.Checked = item.ForeColor == Color.Red;
		}
	}

	private void AdjustRoster(Team importingTeam, Player[] players, int nTotPlayer)
	{
		if (importingTeam != null)
		{
			for (int i = 0; i < importingTeam.Roster.Count; i++)
			{
				TeamPlayer teamPlayer = (TeamPlayer)importingTeam.Roster[i];
				int id = teamPlayer.Player.Id;
				bool flag = false;
				for (int j = 0; j < nTotPlayer; j++)
				{
					if (id == players[j].Id)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					teamPlayer.Player.NotPlayFor(importingTeam);
					importingTeam.Roster.Remove(teamPlayer);
					i--;
				}
			}
		}
		if (importingTeam == null)
		{
			return;
		}
		foreach (TeamPlayer item in importingTeam.Roster)
		{
			if (item.jerseynumber == 0)
			{
				item.jerseynumber = importingTeam.Roster.GetFreeNumber();
			}
		}
		importingTeam.AssignVacantRolesToSubstitute();
		importingTeam.AssignVacantSpecialPlayers();
		importingTeam.AssignBench();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.WebPatchLoader));
		this.toolMain = new System.Windows.Forms.ToolStrip();
		this.buttonImportPatch = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonExitCreator = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonSelectAllObjects = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonDeselectAllObjects = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonSelectNewObjects = new System.Windows.Forms.ToolStripButton();
		this.buttonSelectIfExisting = new System.Windows.Forms.ToolStripButton();
		this.listViewPatch = new System.Windows.Forms.ListView();
		this.columnItem = new System.Windows.Forms.ColumnHeader();
		this.columnType = new System.Windows.Forms.ColumnHeader();
		this.columnImportId = new System.Windows.Forms.ColumnHeader();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.pictureBox = new System.Windows.Forms.PictureBox();
		this.groupPatchOptions = new System.Windows.Forms.GroupBox();
		this.groupMinifaceOptions = new System.Windows.Forms.GroupBox();
		this.checkManagersPicture = new System.Windows.Forms.CheckBox();
		this.checkPlayersPicture = new System.Windows.Forms.CheckBox();
		this.checkTeamsPicture = new System.Windows.Forms.CheckBox();
		this.radioMinifaceAlways = new System.Windows.Forms.RadioButton();
		this.radioMinifaceNotPresent = new System.Windows.Forms.RadioButton();
		this.radioMinifaceNever = new System.Windows.Forms.RadioButton();
		this.groupReplaceSelection = new System.Windows.Forms.GroupBox();
		this.labelCmsCreated = new System.Windows.Forms.Label();
		this.labelCmsReplaced = new System.Windows.Forms.Label();
		this.textCmsReplaced = new System.Windows.Forms.TextBox();
		this.radioCmsItem = new System.Windows.Forms.RadioButton();
		this.comboReplacePlayer = new System.Windows.Forms.ComboBox();
		this.comboReplaceTeam = new System.Windows.Forms.ComboBox();
		this.radioReplaceItem = new System.Windows.Forms.RadioButton();
		this.radioCreateItem = new System.Windows.Forms.RadioButton();
		this.toolMain.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
		this.groupPatchOptions.SuspendLayout();
		this.groupMinifaceOptions.SuspendLayout();
		this.groupReplaceSelection.SuspendLayout();
		base.SuspendLayout();
		this.toolMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.buttonImportPatch, this.toolStripSeparator2, this.buttonExitCreator, this.toolStripSeparator3, this.buttonSelectAllObjects, this.toolStripSeparator4, this.buttonDeselectAllObjects, this.toolStripSeparator1, this.buttonSelectNewObjects, this.buttonSelectIfExisting });
		this.toolMain.Location = new System.Drawing.Point(0, 0);
		this.toolMain.Name = "toolMain";
		this.toolMain.Size = new System.Drawing.Size(643, 25);
		this.toolMain.TabIndex = 2;
		this.toolMain.Text = "toolStrip1";
		this.buttonImportPatch.Enabled = false;
		this.buttonImportPatch.Image = (System.Drawing.Image)resources.GetObject("buttonImportPatch.Image");
		this.buttonImportPatch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportPatch.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
		this.buttonImportPatch.Name = "buttonImportPatch";
		this.buttonImportPatch.Size = new System.Drawing.Size(63, 22);
		this.buttonImportPatch.Text = "Import";
		this.buttonImportPatch.Click += new System.EventHandler(buttonImportPatch_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonExitCreator.Image = (System.Drawing.Image)resources.GetObject("buttonExitCreator.Image");
		this.buttonExitCreator.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExitCreator.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
		this.buttonExitCreator.Name = "buttonExitCreator";
		this.buttonExitCreator.Size = new System.Drawing.Size(46, 22);
		this.buttonExitCreator.Text = "Exit";
		this.buttonExitCreator.Click += new System.EventHandler(buttonExitCreator_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
		this.buttonSelectAllObjects.Image = (System.Drawing.Image)resources.GetObject("buttonSelectAllObjects.Image");
		this.buttonSelectAllObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSelectAllObjects.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
		this.buttonSelectAllObjects.Name = "buttonSelectAllObjects";
		this.buttonSelectAllObjects.Size = new System.Drawing.Size(75, 22);
		this.buttonSelectAllObjects.Text = "Select All";
		this.buttonSelectAllObjects.Click += new System.EventHandler(buttonSelectAllObjects_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.buttonDeselectAllObjects.Image = (System.Drawing.Image)resources.GetObject("buttonDeselectAllObjects.Image");
		this.buttonDeselectAllObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeselectAllObjects.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
		this.buttonDeselectAllObjects.Name = "buttonDeselectAllObjects";
		this.buttonDeselectAllObjects.Size = new System.Drawing.Size(88, 22);
		this.buttonDeselectAllObjects.Text = "Deselect All";
		this.buttonDeselectAllObjects.Click += new System.EventHandler(buttonDeselectAllObjects_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonSelectNewObjects.ForeColor = System.Drawing.Color.DarkGreen;
		this.buttonSelectNewObjects.Image = (System.Drawing.Image)resources.GetObject("buttonSelectNewObjects.Image");
		this.buttonSelectNewObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSelectNewObjects.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
		this.buttonSelectNewObjects.Name = "buttonSelectNewObjects";
		this.buttonSelectNewObjects.Size = new System.Drawing.Size(93, 22);
		this.buttonSelectNewObjects.Text = "Select if new";
		this.buttonSelectNewObjects.Click += new System.EventHandler(buttonSelectNewObjects_Click);
		this.buttonSelectIfExisting.ForeColor = System.Drawing.Color.Red;
		this.buttonSelectIfExisting.Image = (System.Drawing.Image)resources.GetObject("buttonSelectIfExisting.Image");
		this.buttonSelectIfExisting.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSelectIfExisting.Name = "buttonSelectIfExisting";
		this.buttonSelectIfExisting.Size = new System.Drawing.Size(112, 22);
		this.buttonSelectIfExisting.Text = "Select if existing";
		this.buttonSelectIfExisting.Click += new System.EventHandler(buttonSelectIfExisting_Click);
		this.listViewPatch.AllowColumnReorder = true;
		this.listViewPatch.CheckBoxes = true;
		this.listViewPatch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.columnItem, this.columnType, this.columnImportId });
		this.listViewPatch.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewPatch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.listViewPatch.FullRowSelect = true;
		this.listViewPatch.GridLines = true;
		this.listViewPatch.HideSelection = false;
		this.listViewPatch.Location = new System.Drawing.Point(0, 0);
		this.listViewPatch.Name = "listViewPatch";
		this.listViewPatch.Size = new System.Drawing.Size(365, 686);
		this.listViewPatch.TabIndex = 29;
		this.listViewPatch.UseCompatibleStateImageBehavior = false;
		this.listViewPatch.View = System.Windows.Forms.View.Details;
		this.listViewPatch.SelectedIndexChanged += new System.EventHandler(listViewPatch_SelectedIndexChanged);
		this.columnItem.Text = "Name";
		this.columnItem.Width = 169;
		this.columnType.Text = "Type";
		this.columnType.Width = 68;
		this.columnImportId.Text = "Import As";
		this.columnImportId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnImportId.Width = 98;
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.listViewPatch);
		this.splitContainer1.Panel2.Controls.Add(this.pictureBox);
		this.splitContainer1.Panel2.Controls.Add(this.groupPatchOptions);
		this.splitContainer1.Panel2.Controls.Add(this.groupReplaceSelection);
		this.splitContainer1.Size = new System.Drawing.Size(643, 686);
		this.splitContainer1.SplitterDistance = 365;
		this.splitContainer1.TabIndex = 30;
		this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pictureBox.Location = new System.Drawing.Point(0, 290);
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.Size = new System.Drawing.Size(274, 396);
		this.pictureBox.TabIndex = 41;
		this.pictureBox.TabStop = false;
		this.groupPatchOptions.Controls.Add(this.groupMinifaceOptions);
		this.groupPatchOptions.Dock = System.Windows.Forms.DockStyle.Top;
		this.groupPatchOptions.Location = new System.Drawing.Point(0, 144);
		this.groupPatchOptions.Name = "groupPatchOptions";
		this.groupPatchOptions.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.groupPatchOptions.Size = new System.Drawing.Size(274, 146);
		this.groupPatchOptions.TabIndex = 40;
		this.groupPatchOptions.TabStop = false;
		this.groupPatchOptions.Text = "Import Options";
		this.groupMinifaceOptions.Controls.Add(this.checkManagersPicture);
		this.groupMinifaceOptions.Controls.Add(this.checkPlayersPicture);
		this.groupMinifaceOptions.Controls.Add(this.checkTeamsPicture);
		this.groupMinifaceOptions.Controls.Add(this.radioMinifaceAlways);
		this.groupMinifaceOptions.Controls.Add(this.radioMinifaceNotPresent);
		this.groupMinifaceOptions.Controls.Add(this.radioMinifaceNever);
		this.groupMinifaceOptions.Location = new System.Drawing.Point(10, 19);
		this.groupMinifaceOptions.Name = "groupMinifaceOptions";
		this.groupMinifaceOptions.Size = new System.Drawing.Size(229, 93);
		this.groupMinifaceOptions.TabIndex = 7;
		this.groupMinifaceOptions.TabStop = false;
		this.groupMinifaceOptions.Text = "Picture";
		this.checkManagersPicture.AutoSize = true;
		this.checkManagersPicture.Checked = true;
		this.checkManagersPicture.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkManagersPicture.Location = new System.Drawing.Point(149, 43);
		this.checkManagersPicture.Name = "checkManagersPicture";
		this.checkManagersPicture.Size = new System.Drawing.Size(73, 17);
		this.checkManagersPicture.TabIndex = 11;
		this.checkManagersPicture.Text = "Managers";
		this.checkManagersPicture.UseVisualStyleBackColor = true;
		this.checkPlayersPicture.AutoSize = true;
		this.checkPlayersPicture.Location = new System.Drawing.Point(149, 65);
		this.checkPlayersPicture.Name = "checkPlayersPicture";
		this.checkPlayersPicture.Size = new System.Drawing.Size(60, 17);
		this.checkPlayersPicture.TabIndex = 10;
		this.checkPlayersPicture.Text = "Players";
		this.checkPlayersPicture.UseVisualStyleBackColor = true;
		this.checkTeamsPicture.AutoSize = true;
		this.checkTeamsPicture.Checked = true;
		this.checkTeamsPicture.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkTeamsPicture.Location = new System.Drawing.Point(149, 19);
		this.checkTeamsPicture.Name = "checkTeamsPicture";
		this.checkTeamsPicture.Size = new System.Drawing.Size(58, 17);
		this.checkTeamsPicture.TabIndex = 9;
		this.checkTeamsPicture.Text = "Teams";
		this.checkTeamsPicture.UseVisualStyleBackColor = true;
		this.radioMinifaceAlways.AutoSize = true;
		this.radioMinifaceAlways.Location = new System.Drawing.Point(6, 19);
		this.radioMinifaceAlways.Name = "radioMinifaceAlways";
		this.radioMinifaceAlways.Size = new System.Drawing.Size(58, 17);
		this.radioMinifaceAlways.TabIndex = 3;
		this.radioMinifaceAlways.Text = "Always";
		this.radioMinifaceAlways.UseVisualStyleBackColor = true;
		this.radioMinifaceNotPresent.AutoSize = true;
		this.radioMinifaceNotPresent.Checked = true;
		this.radioMinifaceNotPresent.Location = new System.Drawing.Point(6, 42);
		this.radioMinifaceNotPresent.Name = "radioMinifaceNotPresent";
		this.radioMinifaceNotPresent.Size = new System.Drawing.Size(87, 17);
		this.radioMinifaceNotPresent.TabIndex = 4;
		this.radioMinifaceNotPresent.TabStop = true;
		this.radioMinifaceNotPresent.Text = "If not present";
		this.radioMinifaceNotPresent.UseVisualStyleBackColor = true;
		this.radioMinifaceNever.AutoSize = true;
		this.radioMinifaceNever.Location = new System.Drawing.Point(6, 65);
		this.radioMinifaceNever.Name = "radioMinifaceNever";
		this.radioMinifaceNever.Size = new System.Drawing.Size(54, 17);
		this.radioMinifaceNever.TabIndex = 5;
		this.radioMinifaceNever.Text = "Never";
		this.radioMinifaceNever.UseVisualStyleBackColor = true;
		this.groupReplaceSelection.BackColor = System.Drawing.SystemColors.Control;
		this.groupReplaceSelection.Controls.Add(this.labelCmsCreated);
		this.groupReplaceSelection.Controls.Add(this.labelCmsReplaced);
		this.groupReplaceSelection.Controls.Add(this.textCmsReplaced);
		this.groupReplaceSelection.Controls.Add(this.radioCmsItem);
		this.groupReplaceSelection.Controls.Add(this.comboReplacePlayer);
		this.groupReplaceSelection.Controls.Add(this.comboReplaceTeam);
		this.groupReplaceSelection.Controls.Add(this.radioReplaceItem);
		this.groupReplaceSelection.Controls.Add(this.radioCreateItem);
		this.groupReplaceSelection.Dock = System.Windows.Forms.DockStyle.Top;
		this.groupReplaceSelection.Location = new System.Drawing.Point(0, 0);
		this.groupReplaceSelection.Name = "groupReplaceSelection";
		this.groupReplaceSelection.Size = new System.Drawing.Size(274, 144);
		this.groupReplaceSelection.TabIndex = 38;
		this.groupReplaceSelection.TabStop = false;
		this.groupReplaceSelection.Text = "Replace Selection";
		this.labelCmsCreated.AutoSize = true;
		this.labelCmsCreated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelCmsCreated.ForeColor = System.Drawing.Color.Green;
		this.labelCmsCreated.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCmsCreated.Location = new System.Drawing.Point(14, 106);
		this.labelCmsCreated.Name = "labelCmsCreated";
		this.labelCmsCreated.Size = new System.Drawing.Size(44, 13);
		this.labelCmsCreated.TabIndex = 44;
		this.labelCmsCreated.Text = "Create";
		this.labelCmsCreated.Visible = false;
		this.labelCmsReplaced.AutoSize = true;
		this.labelCmsReplaced.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelCmsReplaced.ForeColor = System.Drawing.Color.Red;
		this.labelCmsReplaced.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCmsReplaced.Location = new System.Drawing.Point(14, 106);
		this.labelCmsReplaced.Name = "labelCmsReplaced";
		this.labelCmsReplaced.Size = new System.Drawing.Size(54, 13);
		this.labelCmsReplaced.TabIndex = 43;
		this.labelCmsReplaced.Text = "Replace";
		this.labelCmsReplaced.Visible = false;
		this.textCmsReplaced.BackColor = System.Drawing.Color.White;
		this.textCmsReplaced.Location = new System.Drawing.Point(81, 102);
		this.textCmsReplaced.Name = "textCmsReplaced";
		this.textCmsReplaced.ReadOnly = true;
		this.textCmsReplaced.Size = new System.Drawing.Size(178, 20);
		this.textCmsReplaced.TabIndex = 42;
		this.textCmsReplaced.Visible = false;
		this.radioCmsItem.AutoSize = true;
		this.radioCmsItem.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCmsItem.Location = new System.Drawing.Point(10, 83);
		this.radioCmsItem.Name = "radioCmsItem";
		this.radioCmsItem.Size = new System.Drawing.Size(87, 17);
		this.radioCmsItem.TabIndex = 41;
		this.radioCmsItem.TabStop = true;
		this.radioCmsItem.Text = "Use Patch Id";
		this.radioCmsItem.UseVisualStyleBackColor = true;
		this.radioCmsItem.CheckedChanged += new System.EventHandler(radioUsePatchItem_CheckedChanged);
		this.comboReplacePlayer.FormattingEnabled = true;
		this.comboReplacePlayer.Location = new System.Drawing.Point(81, 53);
		this.comboReplacePlayer.MaxDropDownItems = 20;
		this.comboReplacePlayer.Name = "comboReplacePlayer";
		this.comboReplacePlayer.Size = new System.Drawing.Size(178, 21);
		this.comboReplacePlayer.TabIndex = 4;
		this.comboReplacePlayer.Visible = false;
		this.comboReplacePlayer.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.comboReplaceTeam.FormattingEnabled = true;
		this.comboReplaceTeam.Location = new System.Drawing.Point(81, 53);
		this.comboReplaceTeam.MaxDropDownItems = 20;
		this.comboReplaceTeam.Name = "comboReplaceTeam";
		this.comboReplaceTeam.Size = new System.Drawing.Size(178, 21);
		this.comboReplaceTeam.TabIndex = 3;
		this.comboReplaceTeam.Visible = false;
		this.comboReplaceTeam.SelectedIndexChanged += new System.EventHandler(comboReplace_SelectedIndexChanged);
		this.radioReplaceItem.AutoSize = true;
		this.radioReplaceItem.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioReplaceItem.Location = new System.Drawing.Point(10, 53);
		this.radioReplaceItem.Name = "radioReplaceItem";
		this.radioReplaceItem.Size = new System.Drawing.Size(65, 17);
		this.radioReplaceItem.TabIndex = 1;
		this.radioReplaceItem.TabStop = true;
		this.radioReplaceItem.Text = "Replace";
		this.radioReplaceItem.UseVisualStyleBackColor = true;
		this.radioReplaceItem.CheckedChanged += new System.EventHandler(radioReplaceItem_CheckedChanged);
		this.radioCreateItem.AutoSize = true;
		this.radioCreateItem.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radioCreateItem.Location = new System.Drawing.Point(10, 23);
		this.radioCreateItem.Name = "radioCreateItem";
		this.radioCreateItem.Size = new System.Drawing.Size(56, 17);
		this.radioCreateItem.TabIndex = 0;
		this.radioCreateItem.TabStop = true;
		this.radioCreateItem.Text = "Create";
		this.radioCreateItem.UseVisualStyleBackColor = true;
		this.radioCreateItem.CheckedChanged += new System.EventHandler(radioCreateItem_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(643, 711);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.toolMain);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "WebPatchLoader";
		this.Text = "Web-Patch Loader";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(WebPatchLoader_FormClosing);
		this.toolMain.ResumeLayout(false);
		this.toolMain.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		this.groupPatchOptions.ResumeLayout(false);
		this.groupMinifaceOptions.ResumeLayout(false);
		this.groupMinifaceOptions.PerformLayout();
		this.groupReplaceSelection.ResumeLayout(false);
		this.groupReplaceSelection.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
