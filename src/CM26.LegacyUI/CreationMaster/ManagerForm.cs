using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class ManagerForm : Form
{
	private int m_CurrentSkin = 2;

	private int m_CurrentDress;

	private int m_CurrentColor;

	private int m_CurrentCoat;

	private int m_Body;

	private Viewer3D viewer3DManager;

	private IContainer components;

	private SplitContainer splitContainer1;

	private Viewer2D viewer2DManager;

	private Label labellManagerSkin;

	private NumericUpDown numericManagerColor;

	private GroupBox group3D;

	private ToolStrip toolNear3D;

	private ToolStripButton buttonShow3DModel;

	private Label label2;

	private Label label1;

	private CheckBox checkManagerCoat;

	private ComboBox comboManagerDress;

	private ComboBox comboManagerSkin;

	private ToolStripComboBox comboManagerBodyType;

	public ManagerForm()
	{
		InitializeComponent();
		viewer3DManager = new Viewer3D();
		viewer3DManager.AmbientColor = Color.Black;
		viewer3DManager.BackColor = Color.Gray;
		viewer3DManager.BorderStyle = BorderStyle.Fixed3D;
		viewer3DManager.Dock = DockStyle.Fill;
		viewer3DManager.LightDirectionX = 0.5f;
		viewer3DManager.LightDirectionY = -0.25f;
		viewer3DManager.LightDirectionZ = -1f;
		viewer3DManager.LightX = -30f;
		viewer3DManager.LightY = 180f;
		viewer3DManager.LightZ = 30f;
		viewer3DManager.Location = new Point(3, 16);
		viewer3DManager.Name = "viewer3DManager";
		viewer3DManager.RotationX = 0f;
		viewer3DManager.RotationY = 6.28f;
		viewer3DManager.RotationYCoeff = 0.01f;
		viewer3DManager.Size = new Size(475, 736);
		viewer3DManager.TabIndex = 1;
		viewer3DManager.ViewX = 0f;
		viewer3DManager.ViewY = 90f;
		viewer3DManager.ViewZ = 270f;
		viewer3DManager.ZbufferRenderState = null;
		group3D.Controls.Add(viewer3DManager);
		viewer2DManager.ImageImport = ImportImageManager;
		viewer2DManager.ImageDelete = DeleteManager;
		viewer2DManager.ButtonStripVisible = true;
		viewer2DManager.RemoveButton = true;
		if (comboManagerBodyType.SelectedIndex < 0)
		{
			comboManagerBodyType.SelectedIndex = 0;
		}
	}

	public void Clean()
	{
		base.Visible = false;
	}

	private void ShowManager()
	{
		viewer2DManager.CurrentBitmap = Manager.GetManagerTextures(m_CurrentDress, m_CurrentSkin, m_CurrentColor, m_CurrentCoat);
		Show3DManager();
	}

	private void LoadManager()
	{
		ShowManager();
	}

	private void numericManager3_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentColor = (int)numericManagerColor.Value;
		ShowManager();
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		Show3DManager();
	}

	public void Show3DManager()
	{
		if (!buttonShow3DModel.Checked)
		{
			viewer3DManager.ShowEmpty();
			return;
		}
		Bitmap currentBitmap = viewer2DManager.CurrentBitmap;
		if (currentBitmap == null)
		{
			viewer3DManager.ShowEmpty();
			return;
		}
		Rx3File managerModel = Manager.GetManagerModel(m_CurrentDress, m_Body, m_CurrentCoat);
		if (currentBitmap == null || managerModel == null)
		{
			viewer3DManager.Clean(1);
			return;
		}
		Rx3IndexArray.TriangleListType = Rx3IndexArray.ETriangleListType.InvertEven;
		Model3D model3D = new Model3D(managerModel.Rx3IndexArrays[0], managerModel.Rx3VertexArrays[0], currentBitmap);
		viewer3DManager.Clean(1);
		viewer3DManager.SetMesh(0, model3D);
		viewer3DManager.Render();
	}

	private void comboManagerSkin_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_CurrentSkin = comboManagerSkin.SelectedIndex + 1;
		ShowManager();
	}

	private void comboManagerDress_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_CurrentDress = comboManagerDress.SelectedIndex;
		ShowManager();
	}

	private void checkManagerCoat_CheckedChanged(object sender, EventArgs e)
	{
		m_CurrentCoat = (checkManagerCoat.Checked ? 1 : 0);
		ShowManager();
	}

	private bool ImportImageManager(object sender, Bitmap bitmap)
	{
		return Manager.SetManager(m_CurrentDress, m_CurrentSkin, m_CurrentColor, m_CurrentCoat, bitmap);
	}

	private bool DeleteManager(object sender)
	{
		return Manager.DeleteManagerTexture(m_CurrentDress, m_CurrentSkin, m_CurrentColor, m_CurrentCoat);
	}

	private void comboManagerBodyType_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboManagerBodyType.SelectedIndex >= 0)
		{
			m_Body = comboManagerBodyType.SelectedIndex;
			ShowManager();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.ManagerForm));
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.checkManagerCoat = new System.Windows.Forms.CheckBox();
		this.comboManagerDress = new System.Windows.Forms.ComboBox();
		this.comboManagerSkin = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.numericManagerColor = new System.Windows.Forms.NumericUpDown();
		this.labellManagerSkin = new System.Windows.Forms.Label();
		this.viewer2DManager = new FifaControls.Viewer2D();
		this.group3D = new System.Windows.Forms.GroupBox();
		this.toolNear3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.comboManagerBodyType = new System.Windows.Forms.ToolStripComboBox();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericManagerColor).BeginInit();
		this.group3D.SuspendLayout();
		this.toolNear3D.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 0);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.checkManagerCoat);
		this.splitContainer1.Panel1.Controls.Add(this.comboManagerDress);
		this.splitContainer1.Panel1.Controls.Add(this.comboManagerSkin);
		this.splitContainer1.Panel1.Controls.Add(this.label2);
		this.splitContainer1.Panel1.Controls.Add(this.label1);
		this.splitContainer1.Panel1.Controls.Add(this.numericManagerColor);
		this.splitContainer1.Panel1.Controls.Add(this.labellManagerSkin);
		this.splitContainer1.Panel1.Controls.Add(this.viewer2DManager);
		this.splitContainer1.Panel2.Controls.Add(this.group3D);
		this.splitContainer1.Size = new System.Drawing.Size(1024, 780);
		this.splitContainer1.SplitterDistance = 539;
		this.splitContainer1.TabIndex = 0;
		this.checkManagerCoat.Location = new System.Drawing.Point(9, 89);
		this.checkManagerCoat.Name = "checkManagerCoat";
		this.checkManagerCoat.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkManagerCoat.Size = new System.Drawing.Size(173, 24);
		this.checkManagerCoat.TabIndex = 87;
		this.checkManagerCoat.Text = "Winter Coat";
		this.checkManagerCoat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkManagerCoat.UseVisualStyleBackColor = true;
		this.checkManagerCoat.CheckedChanged += new System.EventHandler(checkManagerCoat_CheckedChanged);
		this.comboManagerDress.FormattingEnabled = true;
		this.comboManagerDress.Items.AddRange(new object[3] { "Jacket", "Shirt", "Sportswear" });
		this.comboManagerDress.Location = new System.Drawing.Point(121, 36);
		this.comboManagerDress.Name = "comboManagerDress";
		this.comboManagerDress.Size = new System.Drawing.Size(121, 21);
		this.comboManagerDress.TabIndex = 85;
		this.comboManagerDress.SelectedIndexChanged += new System.EventHandler(comboManagerDress_SelectedIndexChanged);
		this.comboManagerSkin.FormattingEnabled = true;
		this.comboManagerSkin.Items.AddRange(new object[10] { "1 = unused", "Pink", "3 = unused", "Llight Yellow", "Medium Yellow", "Dark Yellow", "7 = unused", "Light Brown", "Medium Brown", "Dark brown" });
		this.comboManagerSkin.Location = new System.Drawing.Point(121, 9);
		this.comboManagerSkin.Name = "comboManagerSkin";
		this.comboManagerSkin.Size = new System.Drawing.Size(121, 21);
		this.comboManagerSkin.TabIndex = 84;
		this.comboManagerSkin.SelectedIndexChanged += new System.EventHandler(comboManagerSkin_SelectedIndexChanged);
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.SystemColors.Control;
		this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label2.Location = new System.Drawing.Point(12, 67);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(61, 13);
		this.label2.TabIndex = 82;
		this.label2.Text = "Dress Color";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.SystemColors.Control;
		this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1.Location = new System.Drawing.Point(12, 39);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(61, 13);
		this.label1.TabIndex = 81;
		this.label1.Text = "Dress Type";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numericManagerColor.Location = new System.Drawing.Point(121, 63);
		this.numericManagerColor.Maximum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericManagerColor.Name = "numericManagerColor";
		this.numericManagerColor.Size = new System.Drawing.Size(121, 20);
		this.numericManagerColor.TabIndex = 79;
		this.numericManagerColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericManagerColor.ValueChanged += new System.EventHandler(numericManager3_ValueChanged);
		this.labellManagerSkin.AutoSize = true;
		this.labellManagerSkin.BackColor = System.Drawing.SystemColors.Control;
		this.labellManagerSkin.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labellManagerSkin.Location = new System.Drawing.Point(11, 12);
		this.labellManagerSkin.Name = "labellManagerSkin";
		this.labellManagerSkin.Size = new System.Drawing.Size(28, 13);
		this.labellManagerSkin.TabIndex = 76;
		this.labellManagerSkin.Text = "Skin";
		this.labellManagerSkin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.viewer2DManager.AutoTransparency = false;
		this.viewer2DManager.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DManager.ButtonStripVisible = true;
		this.viewer2DManager.CurrentBitmap = null;
		this.viewer2DManager.ExtendedFormat = false;
		this.viewer2DManager.FullSizeButton = true;
		this.viewer2DManager.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.viewer2DManager.ImageSize = new System.Drawing.Size(1024, 1024);
		this.viewer2DManager.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DManager.Location = new System.Drawing.Point(14, 127);
		this.viewer2DManager.Name = "viewer2DManager";
		this.viewer2DManager.RemoveButton = false;
		this.viewer2DManager.ShowButton = false;
		this.viewer2DManager.ShowButtonChecked = true;
		this.viewer2DManager.Size = new System.Drawing.Size(512, 537);
		this.viewer2DManager.TabIndex = 3;
		this.viewer2DManager.TabStop = false;
		this.group3D.Controls.Add(this.toolNear3D);
		this.group3D.Text = "3D Model";
		this.group3D.Dock = System.Windows.Forms.DockStyle.Fill;
		this.group3D.Location = new System.Drawing.Point(0, 0);
		this.group3D.Name = "group3D";
		this.group3D.Size = new System.Drawing.Size(481, 780);
		this.group3D.TabIndex = 2;
		this.group3D.TabStop = false;
		this.toolNear3D.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolNear3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolNear3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.buttonShow3DModel, this.comboManagerBodyType });
		this.toolNear3D.Location = new System.Drawing.Point(3, 752);
		this.toolNear3D.Name = "toolNear3D";
		this.toolNear3D.Size = new System.Drawing.Size(475, 25);
		this.toolNear3D.TabIndex = 2;
		this.buttonShow3DModel.CheckOnClick = true;
		this.buttonShow3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonShow3DModel.Image");
		this.buttonShow3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow3DModel.Name = "buttonShow3DModel";
		this.buttonShow3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonShow3DModel.Text = "Show / Hide";
		this.buttonShow3DModel.Click += new System.EventHandler(buttonShow3DModel_Click);
		this.comboManagerBodyType.Items.AddRange(new object[3] { "Average", "Lean", "Stocky" });
		this.comboManagerBodyType.Name = "comboManagerBodyType";
		this.comboManagerBodyType.Size = new System.Drawing.Size(121, 25);
		this.comboManagerBodyType.SelectedIndexChanged += new System.EventHandler(comboManagerBodyType_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1024, 780);
		base.Controls.Add(this.splitContainer1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "ManagerForm";
		this.Text = "ManagerForm";
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel1.PerformLayout();
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericManagerColor).EndInit();
		this.group3D.ResumeLayout(false);
		this.group3D.PerformLayout();
		this.toolNear3D.ResumeLayout(false);
		this.toolNear3D.PerformLayout();
		base.ResumeLayout(false);
	}
}
