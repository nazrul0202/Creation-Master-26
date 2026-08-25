using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class ShoesForm : Form
{
	private Shoes m_CurrentShoes;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private string m_ShoesCurrentFolder = FifaEnvironment.ExportFolder;

	private IContainer components;

	public PickUpControl pickUpControl;

	private MultiViewer2D multiViewer2DShoesColor;

	private GroupBox group3D;

	private Viewer3D viewer3D;
	private CreationMaster.Controls.Mesh3DPreviewHost m_Fc26Preview;
	private int m_Fc26PreviewRequest;

	private ToolStrip tool3DModel;

	private ToolStripButton buttonShow3DModel;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonImport3DModel;

	private ToolStripButton buttonExport3DModel;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton buttonRemove3DModel;

	private Panel panel1;

	private Label label1;

	public NumericUpDown numericShoesColor;

	private TextBox textShoesName;

	private TextBox textShoesType;

	private Label labelId;

	private CheckBox checkIsAvailableInStore;

	private Panel panel2;

	private CheckBox checkShoesGender;

	private TextBox textShoesShopPackage;

	private Label label2;

	public ShoesForm()
	{
		base.Visible = false;
		InitializeComponent();
		CmStyleDetailsWindow.Attach(this, "Boot Details", DetailSection.Boot,
			() => m_CurrentShoes?.Id ?? -1);
		if (FifaEnvironment.Year == 26)
		{
			viewer3D.Visible = false;
			m_Fc26Preview = new CreationMaster.Controls.Mesh3DPreviewHost { Dock = DockStyle.Fill };
			group3D.Controls.Add(m_Fc26Preview);
			m_Fc26Preview.BringToFront();
		}
		pickUpControl.SelectObject = SelectShoes;
		pickUpControl.CreateObject = CreateShoes;
		pickUpControl.DeleteObject = DeleteShoes;
		pickUpControl.CloneObject = CloneShoes;
		pickUpControl.RefreshObject = RefreshShoes;
		pickUpControl.combo.Sorted = false;
		multiViewer2DShoesColor.Rx3ExportDelegate = ExportRx3ShoesColor;
		multiViewer2DShoesColor.Rx3ImportDelegate = ImportRx3ShoesColor;
		multiViewer2DShoesColor.Rx3SaveDelegate = SaveBitmapShoesColor;
		multiViewer2DShoesColor.Rx3DeleteDelegate = DeleteShoesColor;
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.Shoes;
		pickUpControl.ObjectList = FifaEnvironment.Shoes;
	}

	private Shoes SelectShoes(object sender, object obj)
	{
		Shoes shoes = (Shoes)obj;
		Refresh();
		LoadShoes(shoes);
		return shoes;
	}

	private Shoes CreateShoes(object sender, object obj)
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
		return (Shoes)m_NewIdCreator.NewObject;
	}

	private Shoes DeleteShoes(object sender, object obj)
	{
		Shoes shoes = (Shoes)obj;
		Shoes.DeleteShoes(shoes.Id, 0);
		FifaEnvironment.Shoes.RemoveId(shoes);
		return null;
	}

	private Shoes CloneShoes(object sender, object obj)
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
		Shoes srcIdObject = (Shoes)obj;
		return (Shoes)FifaEnvironment.Shoes.CloneId(srcIdObject, m_NewIdCreator.NewObject);
	}

	public Shoes RefreshShoes(object sender, object obj)
	{
		Preset();
		ReloadShoes(m_CurrentShoes);
		return m_CurrentShoes;
	}

	private bool SaveBitmapShoesColor(object sender, Bitmap[] bitmaps)
	{
		int shoesDesign = (int)numericShoesColor.Value;
		bool result = Shoes.SetShoesTextures(m_CurrentShoes.Id, shoesDesign, bitmaps);
		ReloadShoes(m_CurrentShoes);
		return result;
	}

	private bool ExportRx3ShoesColor(object sender, string exportDir)
	{
		int shoesColor = (int)numericShoesColor.Value;
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.Export(this, Shoes.ShoesTexturesFileName(m_CurrentShoes.Id, shoesColor), exportDir, "Boot texture container");
		return Shoes.ExportShoesTextures(m_CurrentShoes.Id, shoesColor, exportDir);
	}

	private bool ImportRx3ShoesColor(object sender, string rx3FileName)
	{
		int shoesColor = (int)numericShoesColor.Value;
		bool num = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Import(this, Shoes.ShoesTexturesFileName(m_CurrentShoes.Id, shoesColor), rx3FileName, "Boot texture container")
			: Shoes.ImportShoesTextures(m_CurrentShoes.Id, shoesColor, rx3FileName);
		if (num)
		{
			ReloadShoes(m_CurrentShoes);
		}
		return num;
	}

	private bool DeleteShoesColor(object sender)
	{
		int shoesColor = (int)numericShoesColor.Value;
		bool num = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Remove(this, Shoes.ShoesTexturesFileName(m_CurrentShoes.Id, shoesColor), "Boot texture container")
			: Shoes.DeleteShoesTextures(m_CurrentShoes.Id, shoesColor);
		if (num)
		{
			ReloadShoes(m_CurrentShoes);
		}
		return num;
	}

	private void LoadShoes(Shoes shoes)
	{
		if (m_IsLoaded && m_CurrentShoes != shoes)
		{
			m_CurrentShoes = shoes;
			Bitmap[] array = new Bitmap[3];
			int shoesDesign;
			if (m_CurrentShoes.Id == 0)
			{
				numericShoesColor.Enabled = true;
				shoesDesign = (int)numericShoesColor.Value;
			}
			else
			{
				numericShoesColor.Enabled = false;
				numericShoesColor.Value = 0m;
				shoesDesign = 0;
			}
			array = Shoes.GetShoesTextures(shoes.Id, shoesDesign);
			multiViewer2DShoesColor.Bitmaps = array;
			textShoesName.Text = m_CurrentShoes.Name;
			textShoesType.Text = m_CurrentShoes.Id.ToString();
			checkIsAvailableInStore.Checked = m_CurrentShoes.IsAvailableinStore;
			checkShoesGender.Checked = m_CurrentShoes.IsGender;
			if (m_CurrentShoes.powid == -1)
			{
				textShoesShopPackage.Text = "Not in the shop";
			}
			else
			{
				textShoesShopPackage.Text = "Shop Package n. " + m_CurrentShoes.powid;
			}
			Show3DShoes();
		}
	}

	private void ReloadShoes(Shoes shoes)
	{
		m_CurrentShoes = null;
		LoadShoes(shoes);
	}

	public async void Show3DShoes()
	{
		if (FifaEnvironment.Year == 26)
		{
			if (m_Fc26Preview == null || m_CurrentShoes == null) return;
			if (!buttonShow3DModel.Checked) { m_Fc26Preview.ClearModel(); return; }
			int request = ++m_Fc26PreviewRequest;
			m_Fc26Preview.ShowStatus("Loading boot preview...");
			try
			{
				var preview = await System.Threading.Tasks.Task.Run(() => Fc26HostBridge.ExportEquipmentPreview("boot", m_CurrentShoes.Id));
				if (request == m_Fc26PreviewRequest && !IsDisposed) m_Fc26Preview.LoadMesh(preview.MeshPath, preview.TexturePath);
			}
			catch (Exception ex) { if (request == m_Fc26PreviewRequest && !IsDisposed) m_Fc26Preview.ShowStatus(ex.Message); }
			return;
		}
		if (!buttonShow3DModel.Checked)
		{
			viewer3D.ShowEmpty();
			return;
		}
		int shoesDesign = (int)numericShoesColor.Value;
		Bitmap[] shoesTextures = Shoes.GetShoesTextures(m_CurrentShoes.Id, shoesDesign);
		if (shoesTextures == null)
		{
			viewer3D.ShowEmpty();
			return;
		}
		Bitmap bitmap = GraphicUtil.EmbossBitmap(shoesTextures[0], shoesTextures[1]);
		Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
		Rx3File shoesModel = Shoes.GetShoesModel(m_CurrentShoes.Id);
		if (bitmap == null || shoesModel == null)
		{
			viewer3D.Clean(1);
			return;
		}
		Model3D model3D = new Model3D(shoesModel.Rx3IndexArrays[0], shoesModel.Rx3VertexArrays[0], bitmap);
		viewer3D.Clean(2);
		viewer3D.SetMesh(0, model3D);
		viewer3D.Render();
	}

	private void ShoesForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void buttonExportNear3DModel_Click(object sender, EventArgs e)
	{
		string text = Shoes.ShoesModelFileName(m_CurrentShoes.Id);
		if (text != null)
		{
			if (FifaEnvironment.Year == 26)
				Fc26DirectAssetUi.ExportWithDialog(this, text, ref m_ShoesCurrentFolder, "Boot 3D model");
			else
				FifaEnvironment.AskAndExportFromZdata(text, ref m_ShoesCurrentFolder);
		}
	}

	private void buttonRemoveNear3DModel_Click(object sender, EventArgs e)
	{
		bool result = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Remove(this, Shoes.ShoesModelFileName(m_CurrentShoes.Id), "Boot 3D model")
			: Shoes.DeleteShoesModel(m_CurrentShoes.Id);
		if (result) ReloadShoes(m_CurrentShoes);
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		Show3DShoes();
	}

	private void numericShoesColor_ValueChanged(object sender, EventArgs e)
	{
		if (m_CurrentShoes.Id == 0)
		{
			ReloadShoes(m_CurrentShoes);
		}
	}

	private void buttonExport3DModel_Click(object sender, EventArgs e)
	{
		string text = Shoes.ShoesModelFileName(m_CurrentShoes.Id);
		if (text != null)
		{
			if (FifaEnvironment.Year == 26)
				Fc26DirectAssetUi.ExportWithDialog(this, text, ref m_ShoesCurrentFolder, "Boot 3D model");
			else
				FifaEnvironment.AskAndExportFromZdata(text, ref m_ShoesCurrentFolder);
		}
	}

	private void buttonImport3DModel_Click(object sender, EventArgs e)
	{
		string text = FifaEnvironment.BrowseAndCheckModel(ref m_ShoesCurrentFolder, "Open 3D Shoes Model file", "3D shoes model files (*.rx3)|shoe_*.rx3");
		if (text != null)
		{
			bool result = FifaEnvironment.Year == 26
				? Fc26DirectAssetUi.Import(this, Shoes.ShoesModelFileName(m_CurrentShoes.Id), text, "Boot 3D model")
				: Shoes.SetShoesModel(m_CurrentShoes.Id, text);
			if (result) ReloadShoes(m_CurrentShoes);
		}
	}

	private void buttonRemove3DModel_Click(object sender, EventArgs e)
	{
		bool result = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Remove(this, Shoes.ShoesModelFileName(m_CurrentShoes.Id), "Boot 3D model")
			: Shoes.DeleteShoesModel(m_CurrentShoes.Id);
		if (result) ReloadShoes(m_CurrentShoes);
	}

	private void textShoesName_TextChanged(object sender, EventArgs e)
	{
		if (textShoesName.Text != m_CurrentShoes.Name)
		{
			m_CurrentShoes.Name = textShoesName.Text;
		}
	}

	private void checkIsAvailableInStore_CheckedChanged(object sender, EventArgs e)
	{
		m_CurrentShoes.IsAvailableinStore = checkIsAvailableInStore.Checked;
	}

	private void checkShoesGender_CheckedChanged(object sender, EventArgs e)
	{
		m_CurrentShoes.IsGender = checkShoesGender.Checked;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.ShoesForm));
		this.group3D = new System.Windows.Forms.GroupBox();
		this.viewer3D = new FifaControls.Viewer3D();
		this.tool3DModel = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImport3DModel = new System.Windows.Forms.ToolStripButton();
		this.buttonExport3DModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonRemove3DModel = new System.Windows.Forms.ToolStripButton();
		this.panel1 = new System.Windows.Forms.Panel();
		this.checkShoesGender = new System.Windows.Forms.CheckBox();
		this.checkIsAvailableInStore = new System.Windows.Forms.CheckBox();
		this.textShoesType = new System.Windows.Forms.TextBox();
		this.labelId = new System.Windows.Forms.Label();
		this.textShoesName = new System.Windows.Forms.TextBox();
		this.numericShoesColor = new System.Windows.Forms.NumericUpDown();
		this.label1 = new System.Windows.Forms.Label();
		this.multiViewer2DShoesColor = new FifaControls.MultiViewer2D();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.panel2 = new System.Windows.Forms.Panel();
		this.label2 = new System.Windows.Forms.Label();
		this.textShoesShopPackage = new System.Windows.Forms.TextBox();
		this.group3D.SuspendLayout();
		this.tool3DModel.SuspendLayout();
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericShoesColor).BeginInit();
		this.panel2.SuspendLayout();
		base.SuspendLayout();
		this.group3D.Controls.Add(this.viewer3D);
		this.group3D.Controls.Add(this.tool3DModel);
		this.group3D.Dock = System.Windows.Forms.DockStyle.Fill;
		this.group3D.Location = new System.Drawing.Point(0, 0);
		this.group3D.Name = "group3D";
		this.group3D.Size = new System.Drawing.Size(516, 755);
		this.group3D.TabIndex = 2;
		this.group3D.TabStop = false;
		this.group3D.Text = "3D Model";
		this.viewer3D.AmbientColor = System.Drawing.Color.DimGray;
		this.viewer3D.BackColor = System.Drawing.Color.Gray;
		this.viewer3D.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.viewer3D.Dock = System.Windows.Forms.DockStyle.Fill;
		this.viewer3D.LightDirectionX = 0.5f;
		this.viewer3D.LightDirectionY = -0.25f;
		this.viewer3D.LightDirectionZ = -1f;
		this.viewer3D.LightX = -30f;
		this.viewer3D.LightY = 10f;
		this.viewer3D.LightZ = 50f;
		this.viewer3D.Location = new System.Drawing.Point(3, 16);
		this.viewer3D.Name = "viewer3D";
		this.viewer3D.RotationX = 0.43f;
		this.viewer3D.RotationY = 0.23f;
		this.viewer3D.RotationYCoeff = 0.01f;
		this.viewer3D.Size = new System.Drawing.Size(510, 711);
		this.viewer3D.TabIndex = 1;
		this.viewer3D.ViewX = 0f;
		this.viewer3D.ViewY = 0f;
		this.viewer3D.ViewZ = 64f;
		this.viewer3D.ZbufferRenderState = null;
		this.tool3DModel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.tool3DModel.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.tool3DModel.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.buttonShow3DModel, this.toolStripSeparator1, this.buttonImport3DModel, this.buttonExport3DModel, this.toolStripSeparator2, this.buttonRemove3DModel });
		this.tool3DModel.Location = new System.Drawing.Point(3, 727);
		this.tool3DModel.Name = "tool3DModel";
		this.tool3DModel.Size = new System.Drawing.Size(510, 25);
		this.tool3DModel.TabIndex = 2;
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
		this.buttonImport3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImport3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonImport3DModel.Image");
		this.buttonImport3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImport3DModel.Name = "buttonImport3DModel";
		this.buttonImport3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonImport3DModel.Text = "Import 3D Model";
		this.buttonImport3DModel.Click += new System.EventHandler(buttonImport3DModel_Click);
		this.buttonExport3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonExport3DModel.Image");
		this.buttonExport3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport3DModel.Name = "buttonExport3DModel";
		this.buttonExport3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonExport3DModel.Text = "Export 3D Model";
		this.buttonExport3DModel.Click += new System.EventHandler(buttonExport3DModel_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonRemove3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonRemove3DModel.Image");
		this.buttonRemove3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove3DModel.Name = "buttonRemove3DModel";
		this.buttonRemove3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove3DModel.Text = "Remove 3D Model";
		this.buttonRemove3DModel.Click += new System.EventHandler(buttonRemove3DModel_Click);
		this.panel1.Controls.Add(this.textShoesShopPackage);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.checkShoesGender);
		this.panel1.Controls.Add(this.checkIsAvailableInStore);
		this.panel1.Controls.Add(this.textShoesType);
		this.panel1.Controls.Add(this.labelId);
		this.panel1.Controls.Add(this.textShoesName);
		this.panel1.Controls.Add(this.numericShoesColor);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Controls.Add(this.multiViewer2DShoesColor);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
		this.panel1.Location = new System.Drawing.Point(0, 25);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(520, 755);
		this.panel1.TabIndex = 3;
		this.checkShoesGender.AutoSize = true;
		this.checkShoesGender.Location = new System.Drawing.Point(164, 644);
		this.checkShoesGender.Name = "checkShoesGender";
		this.checkShoesGender.Size = new System.Drawing.Size(111, 17);
		this.checkShoesGender.TabIndex = 66;
		this.checkShoesGender.Text = "Shoes for Woman";
		this.checkShoesGender.UseVisualStyleBackColor = true;
		this.checkShoesGender.CheckedChanged += new System.EventHandler(checkShoesGender_CheckedChanged);
		this.checkIsAvailableInStore.AutoSize = true;
		this.checkIsAvailableInStore.Location = new System.Drawing.Point(164, 621);
		this.checkIsAvailableInStore.Name = "checkIsAvailableInStore";
		this.checkIsAvailableInStore.Size = new System.Drawing.Size(80, 17);
		this.checkIsAvailableInStore.TabIndex = 0;
		this.checkIsAvailableInStore.Text = "Is Available";
		this.checkIsAvailableInStore.UseVisualStyleBackColor = true;
		this.checkIsAvailableInStore.CheckedChanged += new System.EventHandler(checkIsAvailableInStore_CheckedChanged);
		this.textShoesType.Enabled = false;
		this.textShoesType.Location = new System.Drawing.Point(76, 619);
		this.textShoesType.Name = "textShoesType";
		this.textShoesType.Size = new System.Drawing.Size(73, 20);
		this.textShoesType.TabIndex = 65;
		this.textShoesType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.labelId.AutoSize = true;
		this.labelId.Location = new System.Drawing.Point(10, 622);
		this.labelId.Name = "labelId";
		this.labelId.Size = new System.Drawing.Size(49, 13);
		this.labelId.TabIndex = 64;
		this.labelId.Text = "Shoes Id";
		this.labelId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textShoesName.Location = new System.Drawing.Point(76, 593);
		this.textShoesName.Name = "textShoesName";
		this.textShoesName.Size = new System.Drawing.Size(438, 20);
		this.textShoesName.TabIndex = 0;
		this.textShoesName.TextChanged += new System.EventHandler(textShoesName_TextChanged);
		this.numericShoesColor.Location = new System.Drawing.Point(51, 3);
		this.numericShoesColor.Maximum = new decimal(new int[4] { 3, 0, 0, 0 });
		this.numericShoesColor.Name = "numericShoesColor";
		this.numericShoesColor.Size = new System.Drawing.Size(76, 20);
		this.numericShoesColor.TabIndex = 63;
		this.numericShoesColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericShoesColor.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericShoesColor.ValueChanged += new System.EventHandler(numericShoesColor_ValueChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(4, 5);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(31, 13);
		this.label1.TabIndex = 3;
		this.label1.Text = "Color";
		this.multiViewer2DShoesColor.AutoTransparency = false;
		this.multiViewer2DShoesColor.Bitmaps = null;
		this.multiViewer2DShoesColor.CheckBitmapSize = true;
		this.multiViewer2DShoesColor.FixedSize = true;
		this.multiViewer2DShoesColor.FullSizeButton = false;
		this.multiViewer2DShoesColor.LabelText = "Texture";
		this.multiViewer2DShoesColor.Location = new System.Drawing.Point(3, 28);
		this.multiViewer2DShoesColor.Name = "multiViewer2DShoesColor";
		this.multiViewer2DShoesColor.ShowDeleteButton = true;
		this.multiViewer2DShoesColor.Size = new System.Drawing.Size(512, 559);
		this.multiViewer2DShoesColor.TabIndex = 1;
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
		this.pickUpControl.Size = new System.Drawing.Size(1036, 25);
		this.pickUpControl.TabIndex = 1;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		this.panel2.Controls.Add(this.group3D);
		this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel2.Location = new System.Drawing.Point(520, 25);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(516, 755);
		this.panel2.TabIndex = 4;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(10, 596);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(60, 13);
		this.label2.TabIndex = 67;
		this.label2.Text = "Description";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textShoesShopPackage.Location = new System.Drawing.Point(13, 644);
		this.textShoesShopPackage.Name = "textShoesShopPackage";
		this.textShoesShopPackage.ReadOnly = true;
		this.textShoesShopPackage.Size = new System.Drawing.Size(136, 20);
		this.textShoesShopPackage.TabIndex = 68;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1036, 780);
		base.Controls.Add(this.panel2);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "ShoesForm";
		this.Text = "ShoesForm";
		base.Load += new System.EventHandler(ShoesForm_Load);
		this.group3D.ResumeLayout(false);
		this.group3D.PerformLayout();
		this.tool3DModel.ResumeLayout(false);
		this.tool3DModel.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericShoesColor).EndInit();
		this.panel2.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
