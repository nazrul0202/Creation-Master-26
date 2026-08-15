using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary;

public class UserOptions : Form
{
	private string m_XmlFileName;

	public bool m_AutoExportFolder = true;

	public string m_ExportFolder;

	public bool m_SaveDatabase = true;

	public bool m_SaveGui = true;

	public bool m_SaveZdata = true;

	public bool m_AutoZdata = true;

	public bool m_SpecificZdata;

	public bool m_SaveZdataInFolder = true;

	public int m_ZdataNumber;

	public bool m_SaveGuiInArchive;

	public bool m_SaveGuiInFolder = true;

	private IContainer components;

	private TextBox textExportFolder;

	private Button buttonBrowseExportFolder;

	private ToolTip toolTip;

	private CheckBox checkSaveDb;

	private CheckBox checkSaveZdata;

	private CheckBox checkSaveGui;

	private RadioButton radioAutoZdata;

	private RadioButton radioSpecificZdata;

	private NumericUpDown numericZdata;

	private GroupBox groupZdataSelection;

	private GroupBox groupAllowSaving;

	private GroupBox groupGuiSaveOptions;

	private RadioButton radioGuiSaveFolder;

	private RadioButton radioGuiSaveArchive;

	private GroupBox groupExportFolde;

	private Button buttonCancel;

	private Button buttonOK;

	private CheckBox checkAutoExportFolder;

	private Options optionsSet;

	private RadioButton radioZdataSaveFolder;

	public UserOptions()
	{
		InitializeComponent();
		string currentDirectory = Environment.CurrentDirectory;
		m_XmlFileName = currentDirectory + "\\Options.xml";
		if (File.Exists(m_XmlFileName))
		{
			optionsSet.ReadXml(m_XmlFileName);
			LoadOptions();
		}
	}

	public DialogResult ShowOptions()
	{
		LoadOptions();
		return ShowDialog();
	}

	private void LoadOptions()
	{
		for (int i = 0; i < optionsSet.DataTableOpt.Count; i++)
		{
			string option = optionsSet.DataTableOpt[i].Option;
			string value = optionsSet.DataTableOpt[i].Value;
			int num;
			try
			{
				num = Convert.ToInt32(value);
			}
			catch
			{
				num = 0;
			}
			bool flag = num != 0;
			_ = optionsSet.DataTableOpt[i].Default;
			switch (option)
			{
			case "ExportFolderAuto":
				checkAutoExportFolder.Checked = flag;
				m_AutoExportFolder = flag;
				textExportFolder.Enabled = !flag;
				buttonBrowseExportFolder.Enabled = !flag;
				break;
			case "ExportFolder":
				textExportFolder.Text = value;
				m_ExportFolder = value;
				break;
			case "DatabaseEditing":
				checkSaveDb.Checked = flag;
				m_SaveDatabase = flag;
				break;
			case "ZdataEditing":
				checkSaveZdata.Checked = flag;
				m_SaveZdata = flag;
				break;
			case "GuiEditing":
				checkSaveGui.Checked = flag;
				m_SaveGui = flag;
				break;
			case "AutoZdata":
				radioAutoZdata.Checked = flag;
				m_AutoZdata = flag;
				if (m_AutoZdata)
				{
					m_SpecificZdata = false;
					m_SaveZdataInFolder = false;
				}
				numericZdata.Enabled = m_SpecificZdata;
				break;
			case "SpecificZdata":
				radioSpecificZdata.Checked = flag;
				m_SpecificZdata = flag;
				if (m_SpecificZdata)
				{
					m_AutoZdata = false;
					m_SaveZdataInFolder = false;
				}
				numericZdata.Enabled = m_SpecificZdata;
				break;
			case "SaveZdataInFolder":
				radioZdataSaveFolder.Checked = flag;
				m_SaveZdataInFolder = flag;
				if (m_SaveZdataInFolder)
				{
					m_AutoZdata = false;
					m_SpecificZdata = false;
				}
				numericZdata.Enabled = m_SpecificZdata;
				break;
			case "ZdataNumber":
				numericZdata.Value = num;
				m_ZdataNumber = num;
				break;
			case "SaveGuiInArchive":
				radioGuiSaveArchive.Checked = flag;
				m_SaveGuiInArchive = flag;
				m_SaveGuiInFolder = !flag;
				break;
			case "SaveGuiInFolder":
				radioGuiSaveFolder.Checked = flag;
				m_SaveGuiInArchive = !flag;
				m_SaveGuiInFolder = flag;
				break;
			}
		}
	}

	private void buttonBrowseExportFolder_Click(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.Description = "Select the export folder";
		folderBrowserDialog.ShowNewFolderButton = true;
		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			folderBrowserDialog.Dispose();
			return;
		}
		textExportFolder.Text = folderBrowserDialog.SelectedPath;
		m_ExportFolder = textExportFolder.Text;
		folderBrowserDialog.Dispose();
	}

	private void checkEditDb_CheckedChanged(object sender, EventArgs e)
	{
		m_SaveDatabase = checkSaveDb.Checked;
	}

	private void textExportFolder_TextChanged(object sender, EventArgs e)
	{
		m_ExportFolder = textExportFolder.Text;
	}

	private void checkEditZdata_CheckedChanged(object sender, EventArgs e)
	{
		m_SaveZdata = checkSaveZdata.Checked;
	}

	private void checkEditGui_CheckedChanged(object sender, EventArgs e)
	{
		m_SaveGui = checkSaveGui.Checked;
	}

	private void radioAutoZdata_CheckedChanged(object sender, EventArgs e)
	{
		m_AutoZdata = radioAutoZdata.Checked;
	}

	private void radioSpecificZdata_CheckedChanged(object sender, EventArgs e)
	{
		m_SpecificZdata = radioSpecificZdata.Checked;
		numericZdata.Enabled = radioSpecificZdata.Checked;
	}

	private void numericZdata_ValueChanged(object sender, EventArgs e)
	{
		m_ZdataNumber = (int)numericZdata.Value;
	}

	private void radioGuiSaveArchive_CheckedChanged(object sender, EventArgs e)
	{
		m_SaveGuiInArchive = radioGuiSaveArchive.Checked;
	}

	private void radioGuiSaveFolder_CheckedChanged(object sender, EventArgs e)
	{
		m_SaveGuiInFolder = radioGuiSaveFolder.Checked;
	}

	private void SaveOptions()
	{
		for (int i = 0; i < optionsSet.DataTableOpt.Count; i++)
		{
			switch (optionsSet.DataTableOpt[i].Option)
			{
			case "ExportFolderAuto":
				optionsSet.DataTableOpt[i].Value = (m_AutoExportFolder ? "1" : "0");
				break;
			case "ExportFolder":
				optionsSet.DataTableOpt[i].Value = m_ExportFolder;
				break;
			case "DatabaseEditing":
				optionsSet.DataTableOpt[i].Value = (m_SaveDatabase ? "1" : "0");
				break;
			case "ZdataEditing":
				optionsSet.DataTableOpt[i].Value = (m_SaveZdata ? "1" : "0");
				break;
			case "GuiEditing":
				optionsSet.DataTableOpt[i].Value = (m_SaveGui ? "1" : "0");
				break;
			case "AutoZdata":
				optionsSet.DataTableOpt[i].Value = (m_AutoZdata ? "1" : "0");
				break;
			case "SpecificZdata":
				optionsSet.DataTableOpt[i].Value = (m_SpecificZdata ? "1" : "0");
				break;
			case "ZdataNumber":
				optionsSet.DataTableOpt[i].Value = m_ZdataNumber.ToString();
				break;
			case "SaveZdataInFolder":
				optionsSet.DataTableOpt[i].Value = (m_SaveZdataInFolder ? "1" : "0");
				break;
			case "SaveGuiInArchive":
				optionsSet.DataTableOpt[i].Value = (m_SaveGuiInArchive ? "1" : "0");
				break;
			case "SaveGuiInFolder":
				optionsSet.DataTableOpt[i].Value = (m_SaveGuiInFolder ? "1" : "0");
				break;
			}
		}
		optionsSet.WriteXml(m_XmlFileName);
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		SaveOptions();
	}

	private void checkAutoExportFolder_CheckedChanged(object sender, EventArgs e)
	{
		m_AutoExportFolder = checkAutoExportFolder.Checked;
		textExportFolder.Enabled = !checkAutoExportFolder.Checked;
		buttonBrowseExportFolder.Enabled = !checkAutoExportFolder.Checked;
	}

	private void UserOptions_FormClosing(object sender, FormClosingEventArgs e)
	{
		SaveOptions();
	}

	private void radioZdataSaveFolder_CheckedChanged(object sender, EventArgs e)
	{
		m_SaveZdataInFolder = radioZdataSaveFolder.Checked;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaLibrary.UserOptions));
		this.textExportFolder = new System.Windows.Forms.TextBox();
		this.buttonBrowseExportFolder = new System.Windows.Forms.Button();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.checkSaveDb = new System.Windows.Forms.CheckBox();
		this.checkSaveZdata = new System.Windows.Forms.CheckBox();
		this.checkSaveGui = new System.Windows.Forms.CheckBox();
		this.radioAutoZdata = new System.Windows.Forms.RadioButton();
		this.radioSpecificZdata = new System.Windows.Forms.RadioButton();
		this.numericZdata = new System.Windows.Forms.NumericUpDown();
		this.radioGuiSaveArchive = new System.Windows.Forms.RadioButton();
		this.radioGuiSaveFolder = new System.Windows.Forms.RadioButton();
		this.checkAutoExportFolder = new System.Windows.Forms.CheckBox();
		this.radioZdataSaveFolder = new System.Windows.Forms.RadioButton();
		this.groupZdataSelection = new System.Windows.Forms.GroupBox();
		this.groupAllowSaving = new System.Windows.Forms.GroupBox();
		this.groupGuiSaveOptions = new System.Windows.Forms.GroupBox();
		this.groupExportFolde = new System.Windows.Forms.GroupBox();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.optionsSet = new FifaLibrary.Options();
		((System.ComponentModel.ISupportInitialize)this.numericZdata).BeginInit();
		this.groupZdataSelection.SuspendLayout();
		this.groupAllowSaving.SuspendLayout();
		this.groupGuiSaveOptions.SuspendLayout();
		this.groupExportFolde.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.optionsSet).BeginInit();
		base.SuspendLayout();
		resources.ApplyResources(this.textExportFolder, "textExportFolder");
		this.textExportFolder.Name = "textExportFolder";
		this.textExportFolder.TextChanged += new System.EventHandler(textExportFolder_TextChanged);
		resources.ApplyResources(this.buttonBrowseExportFolder, "buttonBrowseExportFolder");
		this.buttonBrowseExportFolder.Name = "buttonBrowseExportFolder";
		this.buttonBrowseExportFolder.UseVisualStyleBackColor = true;
		this.buttonBrowseExportFolder.Click += new System.EventHandler(buttonBrowseExportFolder_Click);
		resources.ApplyResources(this.checkSaveDb, "checkSaveDb");
		this.checkSaveDb.Checked = true;
		this.checkSaveDb.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkSaveDb.Name = "checkSaveDb";
		this.toolTip.SetToolTip(this.checkSaveDb, resources.GetString("checkSaveDb.ToolTip"));
		this.checkSaveDb.UseVisualStyleBackColor = true;
		this.checkSaveDb.CheckedChanged += new System.EventHandler(checkEditDb_CheckedChanged);
		resources.ApplyResources(this.checkSaveZdata, "checkSaveZdata");
		this.checkSaveZdata.Checked = true;
		this.checkSaveZdata.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkSaveZdata.Name = "checkSaveZdata";
		this.toolTip.SetToolTip(this.checkSaveZdata, resources.GetString("checkSaveZdata.ToolTip"));
		this.checkSaveZdata.UseVisualStyleBackColor = true;
		this.checkSaveZdata.CheckedChanged += new System.EventHandler(checkEditZdata_CheckedChanged);
		resources.ApplyResources(this.checkSaveGui, "checkSaveGui");
		this.checkSaveGui.Checked = true;
		this.checkSaveGui.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkSaveGui.Name = "checkSaveGui";
		this.toolTip.SetToolTip(this.checkSaveGui, resources.GetString("checkSaveGui.ToolTip"));
		this.checkSaveGui.UseVisualStyleBackColor = true;
		this.checkSaveGui.CheckedChanged += new System.EventHandler(checkEditGui_CheckedChanged);
		resources.ApplyResources(this.radioAutoZdata, "radioAutoZdata");
		this.radioAutoZdata.Checked = true;
		this.radioAutoZdata.Name = "radioAutoZdata";
		this.radioAutoZdata.TabStop = true;
		this.toolTip.SetToolTip(this.radioAutoZdata, resources.GetString("radioAutoZdata.ToolTip"));
		this.radioAutoZdata.UseVisualStyleBackColor = true;
		this.radioAutoZdata.CheckedChanged += new System.EventHandler(radioAutoZdata_CheckedChanged);
		resources.ApplyResources(this.radioSpecificZdata, "radioSpecificZdata");
		this.radioSpecificZdata.Name = "radioSpecificZdata";
		this.radioSpecificZdata.TabStop = true;
		this.toolTip.SetToolTip(this.radioSpecificZdata, resources.GetString("radioSpecificZdata.ToolTip"));
		this.radioSpecificZdata.UseVisualStyleBackColor = true;
		this.radioSpecificZdata.CheckedChanged += new System.EventHandler(radioSpecificZdata_CheckedChanged);
		resources.ApplyResources(this.numericZdata, "numericZdata");
		this.numericZdata.Maximum = new decimal(new int[4] { 98, 0, 0, 0 });
		this.numericZdata.Minimum = new decimal(new int[4] { 40, 0, 0, 0 });
		this.numericZdata.Name = "numericZdata";
		this.toolTip.SetToolTip(this.numericZdata, resources.GetString("numericZdata.ToolTip"));
		this.numericZdata.Value = new decimal(new int[4] { 49, 0, 0, 0 });
		this.numericZdata.ValueChanged += new System.EventHandler(numericZdata_ValueChanged);
		resources.ApplyResources(this.radioGuiSaveArchive, "radioGuiSaveArchive");
		this.radioGuiSaveArchive.Checked = true;
		this.radioGuiSaveArchive.Name = "radioGuiSaveArchive";
		this.radioGuiSaveArchive.TabStop = true;
		this.toolTip.SetToolTip(this.radioGuiSaveArchive, resources.GetString("radioGuiSaveArchive.ToolTip"));
		this.radioGuiSaveArchive.UseVisualStyleBackColor = true;
		this.radioGuiSaveArchive.CheckedChanged += new System.EventHandler(radioGuiSaveArchive_CheckedChanged);
		resources.ApplyResources(this.radioGuiSaveFolder, "radioGuiSaveFolder");
		this.radioGuiSaveFolder.Name = "radioGuiSaveFolder";
		this.toolTip.SetToolTip(this.radioGuiSaveFolder, resources.GetString("radioGuiSaveFolder.ToolTip"));
		this.radioGuiSaveFolder.UseVisualStyleBackColor = true;
		this.radioGuiSaveFolder.CheckedChanged += new System.EventHandler(radioGuiSaveFolder_CheckedChanged);
		resources.ApplyResources(this.checkAutoExportFolder, "checkAutoExportFolder");
		this.checkAutoExportFolder.Checked = true;
		this.checkAutoExportFolder.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkAutoExportFolder.Name = "checkAutoExportFolder";
		this.toolTip.SetToolTip(this.checkAutoExportFolder, resources.GetString("checkAutoExportFolder.ToolTip"));
		this.checkAutoExportFolder.UseVisualStyleBackColor = true;
		this.checkAutoExportFolder.CheckedChanged += new System.EventHandler(checkAutoExportFolder_CheckedChanged);
		resources.ApplyResources(this.radioZdataSaveFolder, "radioZdataSaveFolder");
		this.radioZdataSaveFolder.Name = "radioZdataSaveFolder";
		this.radioZdataSaveFolder.TabStop = true;
		this.toolTip.SetToolTip(this.radioZdataSaveFolder, resources.GetString("radioZdataSaveFolder.ToolTip"));
		this.radioZdataSaveFolder.UseVisualStyleBackColor = true;
		this.radioZdataSaveFolder.CheckedChanged += new System.EventHandler(radioZdataSaveFolder_CheckedChanged);
		this.groupZdataSelection.BackColor = System.Drawing.Color.Transparent;
		this.groupZdataSelection.Controls.Add(this.radioZdataSaveFolder);
		this.groupZdataSelection.Controls.Add(this.numericZdata);
		this.groupZdataSelection.Controls.Add(this.radioAutoZdata);
		this.groupZdataSelection.Controls.Add(this.radioSpecificZdata);
		resources.ApplyResources(this.groupZdataSelection, "groupZdataSelection");
		this.groupZdataSelection.Name = "groupZdataSelection";
		this.groupZdataSelection.TabStop = false;
		this.groupAllowSaving.BackColor = System.Drawing.Color.Transparent;
		this.groupAllowSaving.Controls.Add(this.checkSaveDb);
		this.groupAllowSaving.Controls.Add(this.checkSaveZdata);
		this.groupAllowSaving.Controls.Add(this.checkSaveGui);
		resources.ApplyResources(this.groupAllowSaving, "groupAllowSaving");
		this.groupAllowSaving.Name = "groupAllowSaving";
		this.groupAllowSaving.TabStop = false;
		this.groupGuiSaveOptions.BackColor = System.Drawing.Color.Transparent;
		this.groupGuiSaveOptions.Controls.Add(this.radioGuiSaveFolder);
		this.groupGuiSaveOptions.Controls.Add(this.radioGuiSaveArchive);
		resources.ApplyResources(this.groupGuiSaveOptions, "groupGuiSaveOptions");
		this.groupGuiSaveOptions.Name = "groupGuiSaveOptions";
		this.groupGuiSaveOptions.TabStop = false;
		this.groupExportFolde.BackColor = System.Drawing.Color.Transparent;
		this.groupExportFolde.Controls.Add(this.checkAutoExportFolder);
		this.groupExportFolde.Controls.Add(this.textExportFolder);
		this.groupExportFolde.Controls.Add(this.buttonBrowseExportFolder);
		resources.ApplyResources(this.groupExportFolde, "groupExportFolde");
		this.groupExportFolde.Name = "groupExportFolde";
		this.groupExportFolde.TabStop = false;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		resources.ApplyResources(this.buttonCancel, "buttonCancel");
		this.buttonCancel.Name = "buttonCancel";
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		resources.ApplyResources(this.buttonOK, "buttonOK");
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		this.optionsSet.DataSetName = "Options";
		this.optionsSet.Locale = new System.Globalization.CultureInfo("");
		this.optionsSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOK);
		base.Controls.Add(this.groupExportFolde);
		base.Controls.Add(this.groupGuiSaveOptions);
		base.Controls.Add(this.groupAllowSaving);
		base.Controls.Add(this.groupZdataSelection);
		base.Name = "UserOptions";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(UserOptions_FormClosing);
		((System.ComponentModel.ISupportInitialize)this.numericZdata).EndInit();
		this.groupZdataSelection.ResumeLayout(false);
		this.groupZdataSelection.PerformLayout();
		this.groupAllowSaving.ResumeLayout(false);
		this.groupAllowSaving.PerformLayout();
		this.groupGuiSaveOptions.ResumeLayout(false);
		this.groupGuiSaveOptions.PerformLayout();
		this.groupExportFolde.ResumeLayout(false);
		this.groupExportFolde.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.optionsSet).EndInit();
		base.ResumeLayout(false);
	}
}
