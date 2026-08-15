using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class NewIdCreator : Form
{
	private int m_NewId = -1;

	private IdObject m_NewObject;

	public string m_NewName;

	private string m_CmsFileName;

	private CmsDataSet m_CmsDataSet = new CmsDataSet();

	private IdArrayList m_IdArrayList;

	private IContainer components;

	private RadioButton radioAuto;

	private RadioButton radioSpacificId;

	private RadioButton radioCms;

	private TreeView treeViewCms;

	private Button buttonOK;

	private Button button1;

	private NumericUpDown numericSpecificId;

	private TextBox textCms;

	private Label labelMinMax;

	public int NewId => m_NewId;

	public IdObject NewObject => m_NewObject;

	public string NewName => m_NewName;

	public IdArrayList IdList
	{
		set
		{
			m_IdArrayList = value;
			if (m_IdArrayList != null)
			{
				numericSpecificId.Minimum = m_IdArrayList.MinId;
				numericSpecificId.Maximum = m_IdArrayList.MaxId;
				labelMinMax.Text = m_IdArrayList.MinId + " - " + m_IdArrayList.MaxId;
				if (numericSpecificId.Value < numericSpecificId.Minimum)
				{
					numericSpecificId.Value = numericSpecificId.Minimum;
				}
				else if (numericSpecificId.Value > numericSpecificId.Maximum)
				{
					numericSpecificId.Value = numericSpecificId.Maximum;
				}
			}
			else
			{
				numericSpecificId.Minimum = -1m;
				numericSpecificId.Maximum = -1m;
				numericSpecificId.Value = -1m;
			}
		}
	}

	public NewIdCreator()
	{
		InitializeComponent();
		numericSpecificId.Enabled = radioSpacificId.Checked;
	}

	private void radioSpacificId_CheckedChanged(object sender, EventArgs e)
	{
		numericSpecificId.Enabled = radioSpacificId.Checked;
		if (radioSpacificId.Checked)
		{
			m_NewId = (int)numericSpecificId.Value;
			m_NewName = null;
		}
	}

	private void radioAuto_CheckedChanged(object sender, EventArgs e)
	{
		if (radioAuto.Checked)
		{
			m_NewId = -1;
			m_NewName = null;
		}
	}

	private void numericSpecificId_ValueChanged(object sender, EventArgs e)
	{
		if (radioSpacificId.Checked)
		{
			m_NewId = (int)numericSpecificId.Value;
			m_NewName = null;
		}
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		if (m_IdArrayList == null)
		{
			m_NewObject = null;
		}
		else
		{
			m_NewObject = ((m_NewId < 0) ? m_IdArrayList.CreateNewId() : m_IdArrayList.CreateNewId(m_NewId));
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		m_NewObject = null;
	}

	private void radioCms_CheckedChanged(object sender, EventArgs e)
	{
		textCms.Enabled = radioCms.Checked;
		treeViewCms.Enabled = radioCms.Checked;
		if (radioCms.Checked)
		{
			treeViewCms_AfterSelect(null, null);
		}
	}

	private void treeViewCms_AfterSelect(object sender, TreeViewEventArgs e)
	{
		TreeNode selectedNode = treeViewCms.SelectedNode;
		if (selectedNode == null)
		{
			m_NewId = -1;
			textCms.Text = string.Empty;
			m_NewName = null;
		}
		else if (selectedNode.Level == 1)
		{
			m_NewId = Convert.ToInt32(treeViewCms.SelectedNode.Tag);
			textCms.Text = selectedNode.Text;
			m_NewName = textCms.Text;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.NewIdCreator));
		this.radioAuto = new System.Windows.Forms.RadioButton();
		this.radioSpacificId = new System.Windows.Forms.RadioButton();
		this.radioCms = new System.Windows.Forms.RadioButton();
		this.treeViewCms = new System.Windows.Forms.TreeView();
		this.buttonOK = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		this.numericSpecificId = new System.Windows.Forms.NumericUpDown();
		this.textCms = new System.Windows.Forms.TextBox();
		this.labelMinMax = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.numericSpecificId).BeginInit();
		base.SuspendLayout();
		this.radioAuto.AutoSize = true;
		this.radioAuto.BackColor = System.Drawing.Color.Transparent;
		this.radioAuto.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.radioAuto.Checked = true;
		this.radioAuto.Location = new System.Drawing.Point(0, 5);
		this.radioAuto.Name = "radioAuto";
		this.radioAuto.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
		this.radioAuto.Size = new System.Drawing.Size(82, 17);
		this.radioAuto.TabIndex = 0;
		this.radioAuto.TabStop = true;
		this.radioAuto.Text = "Automatic";
		this.radioAuto.UseVisualStyleBackColor = false;
		this.radioAuto.CheckedChanged += new System.EventHandler(radioAuto_CheckedChanged);
		this.radioSpacificId.AutoSize = true;
		this.radioSpacificId.BackColor = System.Drawing.Color.Transparent;
		this.radioSpacificId.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.radioSpacificId.Location = new System.Drawing.Point(0, 31);
		this.radioSpacificId.Name = "radioSpacificId";
		this.radioSpacificId.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
		this.radioSpacificId.Size = new System.Drawing.Size(85, 17);
		this.radioSpacificId.TabIndex = 1;
		this.radioSpacificId.Text = "Specific Id";
		this.radioSpacificId.UseVisualStyleBackColor = false;
		this.radioSpacificId.CheckedChanged += new System.EventHandler(radioSpacificId_CheckedChanged);
		this.radioCms.BackColor = System.Drawing.Color.Transparent;
		this.radioCms.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.radioCms.Enabled = false;
		this.radioCms.Location = new System.Drawing.Point(26, 122);
		this.radioCms.Name = "radioCms";
		this.radioCms.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
		this.radioCms.Size = new System.Drawing.Size(232, 24);
		this.radioCms.TabIndex = 2;
		this.radioCms.Text = "Browse CMS";
		this.radioCms.UseVisualStyleBackColor = false;
		this.radioCms.Visible = false;
		this.radioCms.CheckedChanged += new System.EventHandler(radioCms_CheckedChanged);
		this.treeViewCms.Location = new System.Drawing.Point(53, 170);
		this.treeViewCms.Name = "treeViewCms";
		this.treeViewCms.Size = new System.Drawing.Size(194, 49);
		this.treeViewCms.TabIndex = 3;
		this.treeViewCms.Visible = false;
		this.treeViewCms.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeViewCms_AfterSelect);
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOK.Location = new System.Drawing.Point(36, 66);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(75, 23);
		this.buttonOK.TabIndex = 4;
		this.buttonOK.Text = "OK";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button1.Location = new System.Drawing.Point(169, 66);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 6;
		this.button1.Text = "Cancel";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.numericSpecificId.Location = new System.Drawing.Point(102, 29);
		this.numericSpecificId.Name = "numericSpecificId";
		this.numericSpecificId.Size = new System.Drawing.Size(87, 20);
		this.numericSpecificId.TabIndex = 6;
		this.numericSpecificId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericSpecificId.ValueChanged += new System.EventHandler(numericSpecificId_ValueChanged);
		this.textCms.BackColor = System.Drawing.Color.White;
		this.textCms.Location = new System.Drawing.Point(116, 122);
		this.textCms.Name = "textCms";
		this.textCms.ReadOnly = true;
		this.textCms.Size = new System.Drawing.Size(97, 20);
		this.textCms.TabIndex = 7;
		this.textCms.Visible = false;
		this.labelMinMax.AutoSize = true;
		this.labelMinMax.BackColor = System.Drawing.Color.Transparent;
		this.labelMinMax.Location = new System.Drawing.Point(195, 33);
		this.labelMinMax.Name = "labelMinMax";
		this.labelMinMax.Size = new System.Drawing.Size(82, 13);
		this.labelMinMax.TabIndex = 8;
		this.labelMinMax.Text = "50000 - 300000";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.ClientSize = new System.Drawing.Size(293, 103);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.labelMinMax);
		base.Controls.Add(this.buttonOK);
		base.Controls.Add(this.textCms);
		base.Controls.Add(this.numericSpecificId);
		base.Controls.Add(this.treeViewCms);
		base.Controls.Add(this.radioCms);
		base.Controls.Add(this.radioSpacificId);
		base.Controls.Add(this.radioAuto);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Name = "NewIdCreator";
		this.Text = "New Id Selector";
		((System.ComponentModel.ISupportInitialize)this.numericSpecificId).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
