using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class GlovesForm : Form
{
	private GkGloves m_CurrentGkGloves;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private Viewer3D viewer3DGloves;

	private IContainer components;

	public PickUpControl pickUpControl;

	private SplitContainer splitContainer1;

	private SplitContainer splitContainer2;

	private MultiViewer2D multiViewer2DGkGloves;

	private ToolStrip toolNear3D;

	private ToolStripButton buttonShow3DModel;

	private ToolStripSeparator toolStripSeparator1;

	public GlovesForm()
	{
		base.Visible = false;
		InitializeComponent();
		viewer3DGloves = new Viewer3D();
		viewer3DGloves.AmbientColor = Color.White;
		viewer3DGloves.BackColor = Color.Gray;
		viewer3DGloves.BorderStyle = BorderStyle.Fixed3D;
		viewer3DGloves.Dock = DockStyle.Fill;
		viewer3DGloves.LightDirectionX = 0f;
		viewer3DGloves.LightDirectionY = 0f;
		viewer3DGloves.LightDirectionZ = -1f;
		viewer3DGloves.LightX = 100f;
		viewer3DGloves.LightY = 10f;
		viewer3DGloves.LightZ = 100f;
		viewer3DGloves.Location = new Point(0, 0);
		viewer3DGloves.Name = "viewer3DGloves";
		viewer3DGloves.RotationX = 0.18f;
		viewer3DGloves.RotationY = 0.93f;
		viewer3DGloves.RotationYCoeff = 0.01f;
		viewer3DGloves.Size = new Size(645, 478);
		viewer3DGloves.TabIndex = 3;
		viewer3DGloves.ViewX = 12f;
		viewer3DGloves.ViewY = 110f;
		viewer3DGloves.ViewZ = 114.2f;
		viewer3DGloves.ZbufferRenderState = null;
		splitContainer2.Panel1.Controls.Add(viewer3DGloves);
		pickUpControl.SelectObject = SelectGkGloves;
		pickUpControl.CreateObject = CreateGkGloves;
		pickUpControl.DeleteObject = DeleteGkGloves;
		pickUpControl.CloneObject = CloneGkGloves;
		pickUpControl.RefreshObject = RefreshGkGloves;
		multiViewer2DGkGloves.Rx3ExportDelegate = ExportRx3GkGloves;
		multiViewer2DGkGloves.Rx3ImportDelegate = ImportRx3GkGloves;
		multiViewer2DGkGloves.Rx3SaveDelegate = SaveBitmapGkGloves;
		multiViewer2DGkGloves.Rx3DeleteDelegate = DeleteRx3GkGloves;
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.GkGloves;
		pickUpControl.ObjectList = FifaEnvironment.GkGloves;
	}

	private GkGloves SelectGkGloves(object sender, object obj)
	{
		GkGloves gkGloves = (GkGloves)obj;
		Refresh();
		LoadGkGloves(gkGloves);
		return gkGloves;
	}

	private GkGloves CreateGkGloves(object sender, object obj)
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
		return (GkGloves)m_NewIdCreator.NewObject;
	}

	private GkGloves DeleteGkGloves(object sender, object obj)
	{
		GkGloves gkGloves = (GkGloves)obj;
		GkGloves.DeleteGkGlovesTextures(gkGloves.Id);
		FifaEnvironment.GkGloves.RemoveId(gkGloves);
		return null;
	}

	private GkGloves CloneGkGloves(object sender, object obj)
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
		GkGloves srcIdObject = (GkGloves)obj;
		return (GkGloves)FifaEnvironment.GkGloves.CloneId(srcIdObject, m_NewIdCreator.NewObject);
	}

	public GkGloves RefreshGkGloves(object sender, object obj)
	{
		Preset();
		ReloadGkGloves(m_CurrentGkGloves);
		return m_CurrentGkGloves;
	}

	private void LoadGkGloves(GkGloves gkgloves)
	{
		if (m_IsLoaded && m_CurrentGkGloves != gkgloves)
		{
			m_CurrentGkGloves = gkgloves;
			Bitmap[] gkGlovesTextures = GkGloves.GetGkGlovesTextures(gkgloves.Id);
			multiViewer2DGkGloves.Bitmaps = gkGlovesTextures;
			Show3DGkGloves();
		}
	}

	private void ReloadGkGloves(GkGloves gkgloves)
	{
		m_CurrentGkGloves = null;
		LoadGkGloves(gkgloves);
	}

	public void Show3DGkGloves()
	{
		if (!buttonShow3DModel.Checked)
		{
			viewer3DGloves.ShowEmpty();
			return;
		}
		Bitmap[] gkGlovesTextures = GkGloves.GetGkGlovesTextures(m_CurrentGkGloves.Id);
		Bitmap bitmap = GraphicUtil.EmbossBitmap(gkGlovesTextures[0], gkGlovesTextures[1]);
		if (bitmap == null || GkGloves.GkGlovesModel == null)
		{
			viewer3DGloves.Clean(1);
			return;
		}
		GkGloves.GkGlovesModel.TextureBitmap = bitmap;
		viewer3DGloves.Clean(1);
		viewer3DGloves.SetMesh(0, GkGloves.GkGlovesModel);
		viewer3DGloves.Render();
	}

	private bool SaveBitmapGkGloves(object sender, Bitmap[] bitmaps)
	{
		bool result = GkGloves.SetGkGlovesTextures(m_CurrentGkGloves.Id, bitmaps);
		ReloadGkGloves(m_CurrentGkGloves);
		return result;
	}

	private bool ExportRx3GkGloves(object sender, string exportDir)
	{
		return GkGloves.ExportGkGlovesTextures(m_CurrentGkGloves.Id, exportDir);
	}

	private bool ImportRx3GkGloves(object sender, string rx3FileName)
	{
		bool num = GkGloves.SetGkGlovesTextures(m_CurrentGkGloves.Id, rx3FileName);
		if (num)
		{
			ReloadGkGloves(m_CurrentGkGloves);
		}
		return num;
	}

	private bool DeleteRx3GkGloves(object sender)
	{
		bool num = GkGloves.DeleteGkGlovesTextures(m_CurrentGkGloves.Id);
		if (num)
		{
			ReloadGkGloves(m_CurrentGkGloves);
		}
		return num;
	}

	private void GkGlovesForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		Show3DGkGloves();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.GlovesForm));
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.multiViewer2DGkGloves = new FifaControls.MultiViewer2D();
		this.splitContainer2 = new System.Windows.Forms.SplitContainer();
		this.toolNear3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.pickUpControl = new FifaControls.PickUpControl();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).BeginInit();
		this.splitContainer2.Panel1.SuspendLayout();
		this.splitContainer2.SuspendLayout();
		this.toolNear3D.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.multiViewer2DGkGloves);
		this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
		this.splitContainer1.Size = new System.Drawing.Size(1165, 773);
		this.splitContainer1.SplitterDistance = 516;
		this.splitContainer1.TabIndex = 1;
		this.multiViewer2DGkGloves.AutoTransparency = false;
		this.multiViewer2DGkGloves.Bitmaps = null;
		this.multiViewer2DGkGloves.CheckBitmapSize = true;
		this.multiViewer2DGkGloves.FixedSize = false;
		this.multiViewer2DGkGloves.FullSizeButton = false;
		this.multiViewer2DGkGloves.LabelText = "Image n.";
		this.multiViewer2DGkGloves.Location = new System.Drawing.Point(3, 6);
		this.multiViewer2DGkGloves.Name = "multiViewer2DGkGloves";
		this.multiViewer2DGkGloves.ShowDeleteButton = true;
		this.multiViewer2DGkGloves.Size = new System.Drawing.Size(512, 304);
		this.multiViewer2DGkGloves.TabIndex = 0;
		this.multiViewer2DGkGloves.Load += new System.EventHandler(GkGlovesForm_Load);
		this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer2.Location = new System.Drawing.Point(0, 0);
		this.splitContainer2.Name = "splitContainer2";
		this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer2.Panel1.Controls.Add(this.toolNear3D);
		this.splitContainer2.Size = new System.Drawing.Size(645, 773);
		this.splitContainer2.SplitterDistance = 503;
		this.splitContainer2.TabIndex = 0;
		this.toolNear3D.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolNear3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolNear3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.buttonShow3DModel, this.toolStripSeparator1 });
		this.toolNear3D.Location = new System.Drawing.Point(0, 478);
		this.toolNear3D.Name = "toolNear3D";
		this.toolNear3D.Size = new System.Drawing.Size(645, 25);
		this.toolNear3D.TabIndex = 4;
		this.buttonShow3DModel.CheckOnClick = true;
		this.buttonShow3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonShow3DModel.Image");
		this.buttonShow3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow3DModel.Name = "buttonShow3DModel";
		this.buttonShow3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonShow3DModel.Text = "Show / Hide";
		this.buttonShow3DModel.Click += new System.EventHandler(buttonShow3DModel_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = true;
		this.pickUpControl.CreateButtonEnabled = false;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = null;
		this.pickUpControl.FilterEnabled = false;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = true;
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = true;
		this.pickUpControl.SearchEnabled = false;
		this.pickUpControl.Size = new System.Drawing.Size(1165, 25);
		this.pickUpControl.TabIndex = 0;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1165, 798);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "GlovesForm";
		this.Text = "GlovesForm";
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.splitContainer2.Panel1.ResumeLayout(false);
		this.splitContainer2.Panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).EndInit();
		this.splitContainer2.ResumeLayout(false);
		this.toolNear3D.ResumeLayout(false);
		this.toolNear3D.PerformLayout();
		base.ResumeLayout(false);
	}
}
