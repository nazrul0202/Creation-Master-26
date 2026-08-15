using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class CmsBrowser : Form
{
	private int m_NewId = -1;

	public string m_NewName;

	private string m_CmsFileName;

	private CmsDataSet m_CmsDataSet = new CmsDataSet();

	private IContainer components;

	private Panel panelBottom;

	private Button button1;

	private Button buttonOK;

	private TreeView treeViewCms;

	public int NewId => m_NewId;

	public string NewName => m_NewName;

	public string CmsFileName
	{
		set
		{
			m_CmsFileName = value;
			if (!File.Exists(m_CmsFileName))
			{
				return;
			}
			m_CmsDataSet.Tables.Clear();
			m_CmsDataSet.Relations.Clear();
			m_CmsDataSet.ReadXml(m_CmsFileName);
			treeViewCms.Nodes.Clear();
			string text = string.Empty;
			TreeNode treeNode = null;
			foreach (DataRow row in m_CmsDataSet.Tables["CmsDataTable"].Rows)
			{
				if (row["Group"].ToString() != text)
				{
					text = row["Group"].ToString();
					treeNode = treeViewCms.Nodes.Add(text);
				}
				treeNode.Nodes.Add(row["Name"].ToString()).Tag = row["Id"];
			}
		}
	}

	public CmsBrowser()
	{
		InitializeComponent();
	}

	private void treeViewCms_AfterSelect(object sender, TreeViewEventArgs e)
	{
		TreeNode selectedNode = treeViewCms.SelectedNode;
		if (selectedNode == null)
		{
			m_NewId = -1;
			m_NewName = null;
		}
		else if (selectedNode.Level == 1)
		{
			m_NewId = Convert.ToInt32(treeViewCms.SelectedNode.Tag);
			m_NewName = selectedNode.Text;
		}
	}

	private void treeViewCms_DoubleClick(object sender, EventArgs e)
	{
		buttonOK.PerformClick();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.CmsBrowser));
		this.panelBottom = new System.Windows.Forms.Panel();
		this.button1 = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.treeViewCms = new System.Windows.Forms.TreeView();
		this.panelBottom.SuspendLayout();
		base.SuspendLayout();
		this.panelBottom.BackgroundImage = (System.Drawing.Image)resources.GetObject("panelBottom.BackgroundImage");
		this.panelBottom.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.panelBottom.Controls.Add(this.button1);
		this.panelBottom.Controls.Add(this.buttonOK);
		this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panelBottom.Location = new System.Drawing.Point(0, 337);
		this.panelBottom.Name = "panelBottom";
		this.panelBottom.Size = new System.Drawing.Size(285, 34);
		this.panelBottom.TabIndex = 6;
		this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button1.Location = new System.Drawing.Point(169, 8);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 6;
		this.button1.Text = "Cancel";
		this.button1.UseVisualStyleBackColor = true;
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOK.Location = new System.Drawing.Point(39, 8);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(75, 23);
		this.buttonOK.TabIndex = 4;
		this.buttonOK.Text = "OK";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.treeViewCms.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeViewCms.Location = new System.Drawing.Point(0, 0);
		this.treeViewCms.Name = "treeViewCms";
		this.treeViewCms.Size = new System.Drawing.Size(285, 337);
		this.treeViewCms.TabIndex = 7;
		this.treeViewCms.DoubleClick += new System.EventHandler(treeViewCms_DoubleClick);
		this.treeViewCms.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeViewCms_AfterSelect);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(285, 371);
		base.Controls.Add(this.treeViewCms);
		base.Controls.Add(this.panelBottom);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Name = "CmsBrowser";
		this.Text = "CmsBrowser";
		this.panelBottom.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
