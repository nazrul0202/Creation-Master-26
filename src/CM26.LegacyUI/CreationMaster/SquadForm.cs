using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

public class SquadForm : Form
{
	public Rank m_Rank;

	private Trophy m_LastSelectedTrophy;

	private IContainer components;

	private Panel panelBottom;

	private Button button1;

	private Button buttonOK;

	private TreeView treeTurns;

	private ComboBox comboTrophy;

	public SquadForm(Trophy currentTrophy)
	{
		InitializeComponent();
		if (m_LastSelectedTrophy == null)
		{
			m_LastSelectedTrophy = currentTrophy;
		}
		Initialize();
	}

	public void Initialize()
	{
		comboTrophy.Items.Clear();
		foreach (Trophy competitionObject in FifaEnvironment.CompetitionObjects)
		{
			if (competitionObject.TypeNumber == 3)
			{
				comboTrophy.Items.Add(competitionObject);
			}
		}
		comboTrophy.SelectedItem = m_LastSelectedTrophy;
	}

	private void comboTrophy_SelectedIndexChanged(object sender, EventArgs e)
	{
		Trophy lastSelectedTrophy = (Trophy)comboTrophy.SelectedItem;
		m_LastSelectedTrophy = lastSelectedTrophy;
		treeTurns.Nodes.Clear();
		TreeNode treeNode = null;
		foreach (Stage stage in m_LastSelectedTrophy.Stages)
		{
			treeNode = treeTurns.Nodes.Add(stage.ToString());
			treeNode.Tag = stage;
			if (stage.Groups.Count > 1)
			{
				foreach (Group group in stage.Groups)
				{
					TreeNode treeNode2 = treeNode.Nodes.Add(group.ToString());
					treeNode2.Tag = group;
					foreach (Rank rank3 in group.Ranks)
					{
						treeNode2.Nodes.Add(rank3.ToString()).Tag = rank3;
					}
				}
				continue;
			}
			foreach (Rank rank4 in ((Group)stage.Groups[0]).Ranks)
			{
				treeNode.Nodes.Add(rank4.ToString()).Tag = rank4;
			}
		}
		treeNode?.Expand();
	}

	private void treeTurns_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (treeTurns.SelectedNode.Tag.GetType().FullName == "FifaLibrary.Squad")
		{
			buttonOK.Enabled = true;
			m_Rank = (Rank)treeTurns.SelectedNode.Tag;
		}
		else
		{
			buttonOK.Enabled = false;
		}
	}

	private void treeTurns_DoubleClick(object sender, EventArgs e)
	{
		if (treeTurns.SelectedNode.Tag.GetType().FullName == "FifaLibrary.Squad")
		{
			buttonOK.Enabled = true;
			m_Rank = (Rank)treeTurns.SelectedNode.Tag;
			buttonOK.PerformClick();
		}
		else
		{
			buttonOK.Enabled = false;
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
		this.panelBottom = new System.Windows.Forms.Panel();
		this.button1 = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.treeTurns = new System.Windows.Forms.TreeView();
		this.comboTrophy = new System.Windows.Forms.ComboBox();
		this.panelBottom.SuspendLayout();
		base.SuspendLayout();
		this.panelBottom.Controls.Add(this.button1);
		this.panelBottom.Controls.Add(this.buttonOK);
		this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panelBottom.Location = new System.Drawing.Point(0, 441);
		this.panelBottom.Name = "panelBottom";
		this.panelBottom.Size = new System.Drawing.Size(304, 42);
		this.panelBottom.TabIndex = 5;
		this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.button1.Location = new System.Drawing.Point(178, 9);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 1;
		this.button1.Text = "Cancel";
		this.button1.UseVisualStyleBackColor = true;
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOK.Enabled = false;
		this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.buttonOK.Location = new System.Drawing.Point(30, 9);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(75, 23);
		this.buttonOK.TabIndex = 0;
		this.buttonOK.Text = "OK";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.treeTurns.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeTurns.Location = new System.Drawing.Point(0, 21);
		this.treeTurns.Name = "treeTurns";
		this.treeTurns.Size = new System.Drawing.Size(304, 462);
		this.treeTurns.TabIndex = 4;
		this.treeTurns.DoubleClick += new System.EventHandler(treeTurns_DoubleClick);
		this.treeTurns.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeTurns_AfterSelect);
		this.comboTrophy.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboTrophy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboTrophy.FormattingEnabled = true;
		this.comboTrophy.Location = new System.Drawing.Point(0, 0);
		this.comboTrophy.MaxDropDownItems = 16;
		this.comboTrophy.Name = "comboTournament";
		this.comboTrophy.Size = new System.Drawing.Size(304, 21);
		this.comboTrophy.Sorted = true;
		this.comboTrophy.TabIndex = 3;
		this.comboTrophy.SelectedIndexChanged += new System.EventHandler(comboTrophy_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(304, 483);
		base.Controls.Add(this.panelBottom);
		base.Controls.Add(this.treeTurns);
		base.Controls.Add(this.comboTrophy);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Name = "SquadForm";
		this.Text = "Team Form";
		this.panelBottom.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
