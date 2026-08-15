using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class NewspapersForm : Form
{
	private int m_CurrentNewspaperId;

	private int m_CurrentCmSponsorId;

	private IContainer components;

	private PickUpControl pickUpControl;

	private Viewer2D viewer2DNewspaper;

	private NumericUpDown numericNewspaper1;

	private Label labelNewpaper;

	private GroupBox groupNewspaper;

	private GroupBox groupCmSponsor;

	private Viewer2D viewer2DCmSponsor;

	private NumericUpDown numericCmSponsor;

	private Viewer2D viewer2DCmSponsorSmall;

	private GroupBox groupSpecificTeamNews;

	private ComboBox comboTeamAvailable;

	private Viewer2D viewer2DTeamNews;

	private NumericUpDown numericTeamNewsCounter;

	private ComboBox comboTeamNewsType;

	private ToolTip toolTip1;

	public NewspapersForm()
	{
		base.Visible = false;
		InitializeComponent();
		pickUpControl.RefreshObject = RefreshNewspapers;
		viewer2DNewspaper.ImageImport = ImportImageNewspapers;
		viewer2DNewspaper.ImageDelete = DeleteImageNewspapers;
		viewer2DNewspaper.ButtonStripVisible = true;
		viewer2DNewspaper.RemoveButton = true;
		viewer2DCmSponsor.ImageImport = ImportImageCmSponsor;
		viewer2DCmSponsor.ImageDelete = DeleteImageCmSponsor;
		viewer2DCmSponsor.ButtonStripVisible = true;
		viewer2DCmSponsor.RemoveButton = true;
		viewer2DCmSponsorSmall.ImageImport = ImportImageCmSponsorSmall;
		viewer2DCmSponsorSmall.ImageDelete = DeleteImageCmSponsorSmall;
		viewer2DCmSponsorSmall.ButtonStripVisible = true;
		viewer2DCmSponsorSmall.RemoveButton = true;
		viewer2DTeamNews.ImageImport = ImportImageTeamNews;
		viewer2DTeamNews.ImageDelete = DeleteImageTeamNews;
		viewer2DTeamNews.ButtonStripVisible = true;
		viewer2DTeamNews.RemoveButton = true;
		comboTeamNewsType.SelectedIndex = 0;
		Preset();
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		if (base.Visible)
		{
			if (FifaEnvironment.Year == 14)
			{
				viewer2DCmSponsor.ImageSize = new Size(256, 64);
				numericCmSponsor.Maximum = 19m;
				viewer2DCmSponsorSmall.Visible = true;
				viewer2DTeamNews.ImageSize = new Size(512, 512);
			}
			else
			{
				viewer2DCmSponsorSmall.Visible = false;
				viewer2DTeamNews.ImageSize = new Size(668, 580);
			}
			if (comboTeamAvailable.Items.Count != FifaEnvironment.Teams.Count)
			{
				comboTeamAvailable.Items.Clear();
				comboTeamAvailable.Items.AddRange(FifaEnvironment.Teams.ToArray());
			}
		}
	}

	public IdObject RefreshNewspapers(object sender, object obj)
	{
		Preset();
		LoadNews();
		return null;
	}

	private void LoadTeamNews()
	{
		Team team = (Team)comboTeamAvailable.SelectedItem;
		if (team == null)
		{
			viewer2DTeamNews.CurrentBitmap = null;
			return;
		}
		int id = team.Id;
		int selectedIndex = comboTeamNewsType.SelectedIndex;
		if (selectedIndex < 0)
		{
			viewer2DTeamNews.CurrentBitmap = null;
			return;
		}
		int order = (int)numericTeamNewsCounter.Value;
		viewer2DTeamNews.CurrentBitmap = TeamNews.GetTeamNews(id, selectedIndex, order);
	}

	private void LoadNews()
	{
		numericNewspaper1.Value = m_CurrentNewspaperId;
		viewer2DNewspaper.CurrentBitmap = Newspaper.GetNewspaper(m_CurrentNewspaperId);
		numericCmSponsor.Value = m_CurrentCmSponsorId;
		viewer2DCmSponsor.CurrentBitmap = CmSponsor.GetCmSponsor(m_CurrentCmSponsorId);
		if (FifaEnvironment.Year == 14)
		{
			viewer2DCmSponsorSmall.CurrentBitmap = CmSponsor.GetCmSponsorSmall(m_CurrentCmSponsorId);
		}
		LoadTeamNews();
	}

	private bool ImportImageNewspapers(object sender, Bitmap bitmap)
	{
		return Newspaper.SetNewspaper(m_CurrentNewspaperId, bitmap);
	}

	private bool DeleteImageNewspapers(object sender)
	{
		return FifaEnvironment.DeleteFromZdata(Newspaper.NewspaperBigFileName(m_CurrentNewspaperId));
	}

	private bool ImportImageCmSponsor(object sender, Bitmap bitmap)
	{
		return CmSponsor.SetCmSponsor(m_CurrentCmSponsorId, bitmap);
	}

	private bool DeleteImageCmSponsor(object sender)
	{
		bool num = CmSponsor.DeleteCmSponsor(m_CurrentCmSponsorId);
		if (num)
		{
			LoadNews();
		}
		return num;
	}

	private bool ImportImageTeamNews(object sender, Bitmap bitmap)
	{
		Team team = (Team)comboTeamAvailable.SelectedItem;
		if (team == null)
		{
			viewer2DTeamNews.CurrentBitmap = null;
			return false;
		}
		int id = team.Id;
		int selectedIndex = comboTeamNewsType.SelectedIndex;
		if (selectedIndex < 0)
		{
			viewer2DTeamNews.CurrentBitmap = null;
			return false;
		}
		int num = (int)numericTeamNewsCounter.Value;
		bool flag = TeamNews.SetTeamNews(id, selectedIndex, num, bitmap);
		if (flag)
		{
			switch (selectedIndex)
			{
			case 0:
				if (num > team.maxvariationsstd)
				{
					team.maxvariationsstd = num;
				}
				break;
			case 1:
				if (num > team.maxvariationspos)
				{
					team.maxvariationspos = num;
				}
				break;
			case 2:
				if (num > team.maxvariationsneg)
				{
					team.maxvariationsneg = num;
				}
				break;
			}
		}
		return flag;
	}

	private bool DeleteImageTeamNews(object sender)
	{
		Team team = (Team)comboTeamAvailable.SelectedItem;
		if (team == null)
		{
			viewer2DTeamNews.CurrentBitmap = null;
			return false;
		}
		int id = team.Id;
		int selectedIndex = comboTeamNewsType.SelectedIndex;
		if (selectedIndex < 0)
		{
			viewer2DTeamNews.CurrentBitmap = null;
			return false;
		}
		int num = (int)numericTeamNewsCounter.Value;
		bool flag = TeamNews.DeleteTeamNews(id, selectedIndex, num);
		if (flag)
		{
			switch (selectedIndex)
			{
			case 0:
				if (num == team.maxvariationsstd)
				{
					team.maxvariationsstd = num - 1;
				}
				break;
			case 1:
				if (num == team.maxvariationspos)
				{
					team.maxvariationspos = num - 1;
				}
				break;
			case 2:
				if (num == team.maxvariationsneg)
				{
					team.maxvariationsneg = num - 1;
				}
				break;
			}
		}
		if (flag)
		{
			LoadNews();
		}
		return flag;
	}

	private bool ImportImageCmSponsorSmall(object sender, Bitmap bitmap)
	{
		return CmSponsor.SetCmSponsorSmall(m_CurrentCmSponsorId, bitmap);
	}

	private bool DeleteImageCmSponsorSmall(object sender)
	{
		return FifaEnvironment.DeleteFromZdata(CmSponsor.CmSponsorSmallBigFileName(m_CurrentCmSponsorId));
	}

	private void NewspapersForm_Load(object sender, EventArgs e)
	{
		Preset();
		LoadNews();
	}

	private void numericNewspaper(object sender, EventArgs e)
	{
		m_CurrentNewspaperId = (int)numericNewspaper1.Value;
		viewer2DNewspaper.CurrentBitmap = Newspaper.GetNewspaper(m_CurrentNewspaperId);
	}

	private void numericCmSponsor_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentCmSponsorId = (int)numericCmSponsor.Value;
		viewer2DCmSponsor.CurrentBitmap = CmSponsor.GetCmSponsor(m_CurrentCmSponsorId);
		if (FifaEnvironment.Year == 14)
		{
			viewer2DCmSponsorSmall.CurrentBitmap = CmSponsor.GetCmSponsorSmall(m_CurrentCmSponsorId);
		}
	}

	private void comboTeamNewsType_SelectedIndexChanged(object sender, EventArgs e)
	{
		LoadTeamNews();
	}

	private void numericTeamNewsCounter_ValueChanged(object sender, EventArgs e)
	{
		LoadTeamNews();
	}

	private void comboTeamAvailable_SelectedIndexChanged(object sender, EventArgs e)
	{
		LoadTeamNews();
	}

	private void groupSpecificTeamNews_Paint(object sender, PaintEventArgs e)
	{
		if (comboTeamAvailable.Items.Count == 0)
		{
			Preset();
			LoadNews();
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
		this.numericNewspaper1 = new System.Windows.Forms.NumericUpDown();
		this.labelNewpaper = new System.Windows.Forms.Label();
		this.groupNewspaper = new System.Windows.Forms.GroupBox();
		this.viewer2DNewspaper = new FifaControls.Viewer2D();
		this.groupCmSponsor = new System.Windows.Forms.GroupBox();
		this.viewer2DCmSponsorSmall = new FifaControls.Viewer2D();
		this.numericCmSponsor = new System.Windows.Forms.NumericUpDown();
		this.viewer2DCmSponsor = new FifaControls.Viewer2D();
		this.groupSpecificTeamNews = new System.Windows.Forms.GroupBox();
		this.viewer2DTeamNews = new FifaControls.Viewer2D();
		this.numericTeamNewsCounter = new System.Windows.Forms.NumericUpDown();
		this.comboTeamNewsType = new System.Windows.Forms.ComboBox();
		this.comboTeamAvailable = new System.Windows.Forms.ComboBox();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		((System.ComponentModel.ISupportInitialize)this.numericNewspaper1).BeginInit();
		this.groupNewspaper.SuspendLayout();
		this.groupCmSponsor.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCmSponsor).BeginInit();
		this.groupSpecificTeamNews.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericTeamNewsCounter).BeginInit();
		base.SuspendLayout();
		this.numericNewspaper1.Location = new System.Drawing.Point(85, 176);
		this.numericNewspaper1.Maximum = new decimal(new int[4] { 14, 0, 0, 0 });
		this.numericNewspaper1.Name = "numericNewspaper1";
		this.numericNewspaper1.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.numericNewspaper1.Size = new System.Drawing.Size(66, 20);
		this.numericNewspaper1.TabIndex = 2;
		this.numericNewspaper1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNewspaper1.ValueChanged += new System.EventHandler(numericNewspaper);
		this.labelNewpaper.AutoSize = true;
		this.labelNewpaper.BackColor = System.Drawing.Color.Transparent;
		this.labelNewpaper.Location = new System.Drawing.Point(6, 178);
		this.labelNewpaper.Name = "labelNewpaper";
		this.labelNewpaper.Size = new System.Drawing.Size(73, 13);
		this.labelNewpaper.TabIndex = 3;
		this.labelNewpaper.Text = "Newspaper n.";
		this.groupNewspaper.Controls.Add(this.viewer2DNewspaper);
		this.groupNewspaper.Controls.Add(this.labelNewpaper);
		this.groupNewspaper.Controls.Add(this.numericNewspaper1);
		this.groupNewspaper.Location = new System.Drawing.Point(12, 31);
		this.groupNewspaper.Name = "groupNewspaper";
		this.groupNewspaper.Size = new System.Drawing.Size(524, 201);
		this.groupNewspaper.TabIndex = 4;
		this.groupNewspaper.TabStop = false;
		this.groupNewspaper.Text = "Newspapers";
		this.viewer2DNewspaper.AutoTransparency = false;
		this.viewer2DNewspaper.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DNewspaper.ButtonStripVisible = true;
		this.viewer2DNewspaper.CurrentBitmap = null;
		this.viewer2DNewspaper.ExtendedFormat = false;
		this.viewer2DNewspaper.FullSizeButton = false;
		this.viewer2DNewspaper.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DNewspaper.ImageSize = new System.Drawing.Size(1024, 128);
		this.viewer2DNewspaper.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DNewspaper.Location = new System.Drawing.Point(6, 19);
		this.viewer2DNewspaper.Name = "viewer2DNewspaper";
		this.viewer2DNewspaper.RemoveButton = false;
		this.viewer2DNewspaper.ShowButton = true;
		this.viewer2DNewspaper.ShowButtonChecked = true;
		this.viewer2DNewspaper.Size = new System.Drawing.Size(512, 153);
		this.viewer2DNewspaper.TabIndex = 1;
		this.toolTip1.SetToolTip(this.viewer2DNewspaper, "Import 1024 x 128 image");
		this.groupCmSponsor.Controls.Add(this.viewer2DCmSponsorSmall);
		this.groupCmSponsor.Controls.Add(this.numericCmSponsor);
		this.groupCmSponsor.Controls.Add(this.viewer2DCmSponsor);
		this.groupCmSponsor.Location = new System.Drawing.Point(12, 238);
		this.groupCmSponsor.Name = "groupCmSponsor";
		this.groupCmSponsor.Size = new System.Drawing.Size(524, 216);
		this.groupCmSponsor.TabIndex = 5;
		this.groupCmSponsor.TabStop = false;
		this.groupCmSponsor.Text = "News Sponsor";
		this.viewer2DCmSponsorSmall.AutoTransparency = false;
		this.viewer2DCmSponsorSmall.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DCmSponsorSmall.ButtonStripVisible = true;
		this.viewer2DCmSponsorSmall.CurrentBitmap = null;
		this.viewer2DCmSponsorSmall.ExtendedFormat = false;
		this.viewer2DCmSponsorSmall.FullSizeButton = false;
		this.viewer2DCmSponsorSmall.ImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.viewer2DCmSponsorSmall.ImageSize = new System.Drawing.Size(256, 32);
		this.viewer2DCmSponsorSmall.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DCmSponsorSmall.Location = new System.Drawing.Point(3, 138);
		this.viewer2DCmSponsorSmall.Name = "viewer2DCmSponsorSmall";
		this.viewer2DCmSponsorSmall.RemoveButton = false;
		this.viewer2DCmSponsorSmall.ShowButton = true;
		this.viewer2DCmSponsorSmall.ShowButtonChecked = true;
		this.viewer2DCmSponsorSmall.Size = new System.Drawing.Size(256, 64);
		this.viewer2DCmSponsorSmall.TabIndex = 4;
		this.toolTip1.SetToolTip(this.viewer2DCmSponsorSmall, "Import 256 x 32 image");
		this.viewer2DCmSponsorSmall.Visible = false;
		this.numericCmSponsor.Location = new System.Drawing.Point(3, 114);
		this.numericCmSponsor.Maximum = new decimal(new int[4] { 21, 0, 0, 0 });
		this.numericCmSponsor.Name = "numericCmSponsor";
		this.numericCmSponsor.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.numericCmSponsor.Size = new System.Drawing.Size(66, 20);
		this.numericCmSponsor.TabIndex = 3;
		this.numericCmSponsor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCmSponsor.ValueChanged += new System.EventHandler(numericCmSponsor_ValueChanged);
		this.viewer2DCmSponsor.AutoTransparency = false;
		this.viewer2DCmSponsor.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DCmSponsor.ButtonStripVisible = true;
		this.viewer2DCmSponsor.CurrentBitmap = null;
		this.viewer2DCmSponsor.ExtendedFormat = false;
		this.viewer2DCmSponsor.FullSizeButton = false;
		this.viewer2DCmSponsor.ImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.viewer2DCmSponsor.ImageSize = new System.Drawing.Size(512, 64);
		this.viewer2DCmSponsor.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DCmSponsor.Location = new System.Drawing.Point(6, 19);
		this.viewer2DCmSponsor.Name = "viewer2DCmSponsor";
		this.viewer2DCmSponsor.RemoveButton = true;
		this.viewer2DCmSponsor.ShowButton = true;
		this.viewer2DCmSponsor.ShowButtonChecked = true;
		this.viewer2DCmSponsor.Size = new System.Drawing.Size(512, 96);
		this.viewer2DCmSponsor.TabIndex = 2;
		this.toolTip1.SetToolTip(this.viewer2DCmSponsor, "Import 512 x 64 image");
		this.groupSpecificTeamNews.Controls.Add(this.viewer2DTeamNews);
		this.groupSpecificTeamNews.Controls.Add(this.numericTeamNewsCounter);
		this.groupSpecificTeamNews.Controls.Add(this.comboTeamNewsType);
		this.groupSpecificTeamNews.Controls.Add(this.comboTeamAvailable);
		this.groupSpecificTeamNews.Location = new System.Drawing.Point(536, 31);
		this.groupSpecificTeamNews.Name = "groupSpecificTeamNews";
		this.groupSpecificTeamNews.Size = new System.Drawing.Size(347, 423);
		this.groupSpecificTeamNews.TabIndex = 6;
		this.groupSpecificTeamNews.TabStop = false;
		this.groupSpecificTeamNews.Text = "Specific Team News";
		this.groupSpecificTeamNews.Paint += new System.Windows.Forms.PaintEventHandler(groupSpecificTeamNews_Paint);
		this.viewer2DTeamNews.AutoTransparency = false;
		this.viewer2DTeamNews.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DTeamNews.ButtonStripVisible = true;
		this.viewer2DTeamNews.CurrentBitmap = null;
		this.viewer2DTeamNews.ExtendedFormat = false;
		this.viewer2DTeamNews.FullSizeButton = true;
		this.viewer2DTeamNews.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DTeamNews.ImageSize = new System.Drawing.Size(668, 550);
		this.viewer2DTeamNews.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DTeamNews.Location = new System.Drawing.Point(6, 101);
		this.viewer2DTeamNews.Name = "viewer2DTeamNews";
		this.viewer2DTeamNews.RemoveButton = true;
		this.viewer2DTeamNews.ShowButton = true;
		this.viewer2DTeamNews.ShowButtonChecked = true;
		this.viewer2DTeamNews.Size = new System.Drawing.Size(334, 315);
		this.viewer2DTeamNews.TabIndex = 3;
		this.toolTip1.SetToolTip(this.viewer2DTeamNews, "Import 668 x 550 image");
		this.numericTeamNewsCounter.Location = new System.Drawing.Point(173, 57);
		this.numericTeamNewsCounter.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericTeamNewsCounter.Name = "numericTeamNewsCounter";
		this.numericTeamNewsCounter.Size = new System.Drawing.Size(71, 20);
		this.numericTeamNewsCounter.TabIndex = 2;
		this.numericTeamNewsCounter.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTeamNewsCounter.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericTeamNewsCounter.ValueChanged += new System.EventHandler(numericTeamNewsCounter_ValueChanged);
		this.comboTeamNewsType.FormattingEnabled = true;
		this.comboTeamNewsType.Items.AddRange(new object[3] { "Neutral", "Celebrating", "Disappointed" });
		this.comboTeamNewsType.Location = new System.Drawing.Point(6, 56);
		this.comboTeamNewsType.Name = "comboTeamNewsType";
		this.comboTeamNewsType.Size = new System.Drawing.Size(121, 21);
		this.comboTeamNewsType.TabIndex = 1;
		this.comboTeamNewsType.SelectedIndexChanged += new System.EventHandler(comboTeamNewsType_SelectedIndexChanged);
		this.comboTeamAvailable.FormattingEnabled = true;
		this.comboTeamAvailable.Location = new System.Drawing.Point(6, 19);
		this.comboTeamAvailable.Name = "comboTeamAvailable";
		this.comboTeamAvailable.Size = new System.Drawing.Size(238, 21);
		this.comboTeamAvailable.Sorted = true;
		this.comboTeamAvailable.TabIndex = 0;
		this.comboTeamAvailable.SelectedIndexChanged += new System.EventHandler(comboTeamAvailable_SelectedIndexChanged);
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = false;
		this.pickUpControl.CreateButtonEnabled = false;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = null;
		this.pickUpControl.FilterEnabled = false;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = false;
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = false;
		this.pickUpControl.SearchEnabled = false;
		this.pickUpControl.Size = new System.Drawing.Size(1165, 25);
		this.pickUpControl.TabIndex = 0;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1165, 798);
		base.Controls.Add(this.groupSpecificTeamNews);
		base.Controls.Add(this.groupCmSponsor);
		base.Controls.Add(this.groupNewspaper);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "NewspapersForm";
		this.Text = "NewspapersForm";
		((System.ComponentModel.ISupportInitialize)this.numericNewspaper1).EndInit();
		this.groupNewspaper.ResumeLayout(false);
		this.groupNewspaper.PerformLayout();
		this.groupCmSponsor.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericCmSponsor).EndInit();
		this.groupSpecificTeamNews.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericTeamNewsCounter).EndInit();
		base.ResumeLayout(false);
	}
}
