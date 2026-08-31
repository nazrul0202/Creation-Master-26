using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class BallForm : Form
{
	private Ball m_CurrentBall;

	private bool m_IsLoaded;

	private NewIdCreator m_NewIdCreator = new NewIdCreator();

	private string m_BallCurrentFolder = FifaEnvironment.ExportFolder;

	private Viewer3D viewer3DBall;
	private CreationMaster.Controls.Mesh3DPreviewHost m_Fc26Preview;
	private int m_Fc26PreviewRequest;

	private IContainer components;

	private GroupBox group3D;

	private ToolStrip toolNear3D;

	private ToolStripButton buttonImport3DModel;

	private ToolStripButton buttonExport3DModel;

	private ToolStripButton buttonRemove3DModel;

	public PickUpControl pickUpControl;

	private ToolStripButton buttonShow3DModel;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator2;

	private SplitContainer splitContainer1;

	private SplitContainer splitContainer2;

	private TextBox textBalllName;

	private Label labelBallName;

	private SplitContainer splitContainer3;

	private CheckBox checkIsGenericBall;

	private Viewer2D viewer2DBallPicture;

	private BindingSource ballBindingSource;

	private MultiViewer2D multiViewer2DTextures;

	private TextBox textBox1;

	private Label labelId;

	private ToolStripButton buttonCamera;

	private CheckBox checkBox1;

	public BallForm()
	{
		base.Visible = false;
		InitializeComponent();
		CmStyleDetailsWindow.Attach(this, "Ball Assignments", DetailSection.Ball,
			() => m_CurrentBall?.Id ?? -1);
		viewer3DBall = new Viewer3D();
		viewer3DBall.AmbientColor = Color.DimGray;
		viewer3DBall.BackColor = Color.Gray;
		viewer3DBall.BorderStyle = BorderStyle.Fixed3D;
		viewer3DBall.Dock = DockStyle.Fill;
		viewer3DBall.LightDirectionX = -0.5f;
		viewer3DBall.LightDirectionY = -0.5f;
		viewer3DBall.LightDirectionZ = -1f;
		viewer3DBall.LightX = 30f;
		viewer3DBall.LightY = 30f;
		viewer3DBall.LightZ = 30f;
		viewer3DBall.Location = new Point(3, 16);
		viewer3DBall.Name = "viewer3DBall";
		viewer3DBall.RotationX = 0f;
		viewer3DBall.RotationY = 0f;
		viewer3DBall.RotationYCoeff = 0.01f;
		viewer3DBall.Size = new Size(827, 514);
		viewer3DBall.TabIndex = 1;
		viewer3DBall.ViewX = 0f;
		viewer3DBall.ViewY = 0f;
		viewer3DBall.ViewZ = 30f;
		viewer3DBall.ZbufferRenderState = null;
		group3D.Controls.Add(viewer3DBall);
		if (FifaEnvironment.Year == 26)
		{
			viewer3DBall.Visible = false;
			m_Fc26Preview = new CreationMaster.Controls.Mesh3DPreviewHost { Dock = DockStyle.Fill };
			group3D.Controls.Add(m_Fc26Preview);
			m_Fc26Preview.BringToFront();
		}
		pickUpControl.SelectObject = SelectBall;
		pickUpControl.CreateObject = CreateBall;
		pickUpControl.DeleteObject = DeleteBall;
		pickUpControl.CloneObject = CloneBall;
		pickUpControl.RefreshObject = RefreshBall;
		multiViewer2DTextures.Rx3ExportDelegate = ExportRx3BallTextures;
		multiViewer2DTextures.Rx3ImportDelegate = ImportRx3BallTextures;
		multiViewer2DTextures.Rx3SaveDelegate = SaveRx3BallTextures;
		multiViewer2DTextures.Rx3DeleteDelegate = DeleteRx3BallTextures;
		viewer2DBallPicture.ImageImport = ImportImageBallPicture;
		viewer2DBallPicture.ImageDelete = DeleteImageBallPicture;
		viewer2DBallPicture.ButtonStripVisible = true;
		viewer2DBallPicture.RemoveButton = true;
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public void Preset()
	{
		m_NewIdCreator.IdList = FifaEnvironment.Balls;
		pickUpControl.ObjectList = FifaEnvironment.Balls;
	}

	private bool ImportImageTextures(object sender, Bitmap[] bitmaps)
	{
		bool result = m_CurrentBall.SetBallTextures(bitmaps);
		ReloadBall(m_CurrentBall);
		return result;
	}

	private bool ExportFshTexture(object sender)
	{
		return FifaEnvironment.AskAndExportFromZdata(m_CurrentBall.BallTextureFileName(), ref m_BallCurrentFolder);
	}

	private bool DeleteTexture(object sender)
	{
		bool result = m_CurrentBall.DeleteBallTextures();
		ReloadBall(m_CurrentBall);
		return result;
	}

	private void ReloadBall(Ball ball)
	{
		m_CurrentBall = null;
		LoadBall(ball);
	}

	private void LoadBall(Ball ball)
	{
		if (m_IsLoaded && m_CurrentBall != ball)
		{
			m_CurrentBall = ball;
			ballBindingSource.DataSource = m_CurrentBall;
			multiViewer2DTextures.Bitmaps = m_CurrentBall.GetBallTextures();
			viewer2DBallPicture.CurrentBitmap = m_CurrentBall.GetBallPicture();
			Show3DBall();
		}
	}

	public async void Show3DBall()
	{
		if (FifaEnvironment.Year == 26)
		{
			if (m_Fc26Preview == null || m_CurrentBall == null) return;
			if (!buttonShow3DModel.Checked) { m_Fc26Preview.ClearModel(); return; }
			int request = ++m_Fc26PreviewRequest;
			m_Fc26Preview.ShowStatus("Loading ball preview...");
			try
			{
				var preview = await System.Threading.Tasks.Task.Run(() => Fc26HostBridge.ExportEquipmentPreview("ball", m_CurrentBall.Id));
				if (request == m_Fc26PreviewRequest && !IsDisposed) m_Fc26Preview.LoadMesh(preview.MeshPath, preview.TexturePath);
			}
			catch (Exception) { if (request == m_Fc26PreviewRequest && !IsDisposed) m_Fc26Preview.ShowStatus("Preview unavailable; the editor remains usable."); }
			return;
		}
		if (!buttonShow3DModel.Checked)
		{
			viewer3DBall.ShowEmpty();
			return;
		}
		Bitmap[] ballTextures = m_CurrentBall.GetBallTextures();
		Bitmap bitmap = null;
		if (ballTextures != null)
		{
			bitmap = GraphicUtil.EmbossBitmap(ballTextures[0], ballTextures[1]);
		}
		Rx3File ballModel = m_CurrentBall.GetBallModel();
		if (bitmap == null || ballModel == null)
		{
			viewer3DBall.Clean(1);
			return;
		}
		Rx3IndexArray.TriangleListType = Rx3IndexArray.ETriangleListType.InvertEven;
		Model3D model3D = new Model3D(ballModel.Rx3IndexArrays[0], ballModel.Rx3VertexArrays[0], bitmap);
		viewer3DBall.Clean(1);
		viewer3DBall.SetMesh(0, model3D);
		viewer3DBall.Render();
	}

	private bool ImportImageBallPicture(object sender, Bitmap bitmap)
	{
		if (FifaEnvironment.Year == 26)
		{
			bool result = Fc26DirectAssetUi.ImportImage(this, m_CurrentBall.BallDdsFileName(), bitmap,
				bitmap.Width, bitmap.Height, "Ball menu image");
			if (result) ReloadBall(m_CurrentBall);
			return result;
		}
		return m_CurrentBall.SetBallPicture(bitmap);
	}

	private bool DeleteImageBallPicture(object sender)
	{
		if (FifaEnvironment.Year == 26)
		{
			bool result = Fc26DirectAssetUi.Remove(this, m_CurrentBall.BallDdsFileName(), "Ball menu image");
			if (result) ReloadBall(m_CurrentBall);
			return result;
		}
		return m_CurrentBall.DeleteBallPicture();
	}

	private bool ExportRx3BallTextures(object sender, string exportDir)
	{
		if (FifaEnvironment.Year == 26)
			return Fc26DirectAssetUi.Export(this, m_CurrentBall.BallTextureFileName(), exportDir, "Ball texture container");
		return FifaEnvironment.ExportFileFromZdata(m_CurrentBall.BallTextureFileName(), exportDir);
	}

	private bool SaveRx3BallTextures(object sender, Bitmap[] bitmaps)
	{
		bool num = m_CurrentBall.SetBallTextures(bitmaps);
		if (num)
		{
			ReloadBall(m_CurrentBall);
		}
		return num;
	}

	private bool ImportRx3BallTextures(object sender, string rx3FileName)
	{
		if (FifaEnvironment.Year == 26)
		{
			bool result = Fc26DirectAssetUi.Import(this, m_CurrentBall.BallTextureFileName(), rx3FileName, "Ball texture container");
			if (result) ReloadBall(m_CurrentBall);
			return result;
		}
		bool num = m_CurrentBall.SetBallTextures(rx3FileName);
		if (num)
		{
			ReloadBall(m_CurrentBall);
		}
		return num;
	}

	private bool DeleteRx3BallTextures(object sender)
	{
		if (FifaEnvironment.Year == 26)
		{
			bool result = Fc26DirectAssetUi.Remove(this, m_CurrentBall.BallTextureFileName(), "Ball texture container");
			if (result) ReloadBall(m_CurrentBall);
			return result;
		}
		bool num = m_CurrentBall.DeleteBallTextures();
		if (num)
		{
			ReloadBall(m_CurrentBall);
		}
		return num;
	}

	private Ball SelectBall(object sender, object obj)
	{
		Ball ball = (Ball)obj;
		Refresh();
		LoadBall(ball);
		return ball;
	}

	private Ball CreateBall(object sender, object obj)
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
		return (Ball)m_NewIdCreator.NewObject;
	}

	private Ball DeleteBall(object sender, object obj)
	{
		Ball ball = (Ball)obj;
		ball.DeleteBall();
		FifaEnvironment.Balls.RemoveId(ball);
		return null;
	}

	private Ball CloneBall(object sender, object obj)
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
		Ball srcIdObject = (Ball)obj;
		return (Ball)FifaEnvironment.Balls.CloneId(srcIdObject, m_NewIdCreator.NewObject);
	}

	public Ball RefreshBall(object sender, object obj)
	{
		Preset();
		ReloadBall(m_CurrentBall);
		return m_CurrentBall;
	}

	private void buttonImportNear3DModel_Click(object sender, EventArgs e)
	{
		string text = FifaEnvironment.BrowseAndCheckModel(ref m_BallCurrentFolder, "Open 3D Ball Model file", "3D ball model files (*.rx3)|ball_*.rx3");
		if (text != null)
		{
			bool result = FifaEnvironment.Year == 26
				? Fc26DirectAssetUi.Import(this, m_CurrentBall.BallModelFileName(), text, "Ball 3D model")
				: m_CurrentBall.SetBallModel(text);
			if (result) ReloadBall(m_CurrentBall);
		}
	}

	private void buttonExportNear3DModel_Click(object sender, EventArgs e)
	{
		string text = m_CurrentBall.BallModelFileName();
		if (text != null)
		{
			if (FifaEnvironment.Year == 26)
				Fc26DirectAssetUi.ExportWithDialog(this, text, ref m_BallCurrentFolder, "Ball 3D model");
			else
				FifaEnvironment.AskAndExportFromZdata(text, ref m_BallCurrentFolder);
		}
	}

	private void buttonRemoveNear3DModel_Click(object sender, EventArgs e)
	{
		bool result = FifaEnvironment.Year == 26
			? Fc26DirectAssetUi.Remove(this, m_CurrentBall.BallModelFileName(), "Ball 3D model")
			: m_CurrentBall.DeleteBallModel();
		if (result) ReloadBall(m_CurrentBall);
	}

	private void BallForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		Show3DBall();
	}

	private void textBalllName_TextChanged(object sender, EventArgs e)
	{
		m_CurrentBall.Name = textBalllName.Text;
		pickUpControl.SwitchObject(m_CurrentBall);
	}

	private void buttonCamera_Click(object sender, EventArgs e)
	{
		Bitmap bitmap = viewer3DBall.Photo();
		int num = bitmap.Height;
		int num2 = bitmap.Width;
		if (num2 >= num)
		{
			int num3 = num - num / 12;
			int num4 = num3;
			int num5 = (num2 - num4) / 2;
			int num6 = (num - num3) / 2;
			Rectangle srcRect = new Rectangle(num5, num6, num4, num3);
			Rectangle destRect = new Rectangle(73, 0, 177, 177);
			bitmap = GraphicUtil.MakeAutoTransparent(bitmap);
			Bitmap bitmap2 = new Bitmap(512, 256, PixelFormat.Format32bppArgb);
			GraphicUtil.RemapRectangle(bitmap, srcRect, bitmap2, destRect);
			Bitmap overBitmap = new Bitmap(FifaEnvironment.LaunchDir + "\\Templates\\BallShadow.png");
			GraphicUtil.DrawOver(bitmap2, overBitmap);
			m_CurrentBall.SetBallPicture(bitmap2);
			viewer2DBallPicture.CurrentBitmap = bitmap2;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.BallForm));
		this.group3D = new System.Windows.Forms.GroupBox();
		this.toolNear3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImport3DModel = new System.Windows.Forms.ToolStripButton();
		this.buttonExport3DModel = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonRemove3DModel = new System.Windows.Forms.ToolStripButton();
		this.buttonCamera = new System.Windows.Forms.ToolStripButton();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.splitContainer3 = new System.Windows.Forms.SplitContainer();
		this.multiViewer2DTextures = new FifaControls.MultiViewer2D();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		this.ballBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.labelId = new System.Windows.Forms.Label();
		this.checkIsGenericBall = new System.Windows.Forms.CheckBox();
		this.labelBallName = new System.Windows.Forms.Label();
		this.textBalllName = new System.Windows.Forms.TextBox();
		this.splitContainer2 = new System.Windows.Forms.SplitContainer();
		this.viewer2DBallPicture = new FifaControls.Viewer2D();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.group3D.SuspendLayout();
		this.toolNear3D.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer3).BeginInit();
		this.splitContainer3.Panel1.SuspendLayout();
		this.splitContainer3.Panel2.SuspendLayout();
		this.splitContainer3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ballBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).BeginInit();
		this.splitContainer2.Panel1.SuspendLayout();
		this.splitContainer2.Panel2.SuspendLayout();
		this.splitContainer2.SuspendLayout();
		base.SuspendLayout();
		this.group3D.Controls.Add(this.toolNear3D);
		this.group3D.Dock = System.Windows.Forms.DockStyle.Fill;
		this.group3D.Location = new System.Drawing.Point(0, 0);
		this.group3D.Name = "group3D";
		this.group3D.Size = new System.Drawing.Size(833, 558);
		this.group3D.TabIndex = 1;
		this.group3D.TabStop = false;
		this.group3D.Text = "3D Model";
		this.toolNear3D.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolNear3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolNear3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.buttonShow3DModel, this.toolStripSeparator1, this.buttonImport3DModel, this.buttonExport3DModel, this.toolStripSeparator2, this.buttonRemove3DModel, this.buttonCamera });
		this.toolNear3D.Location = new System.Drawing.Point(3, 530);
		this.toolNear3D.Name = "toolNear3D";
		this.toolNear3D.Size = new System.Drawing.Size(827, 25);
		this.toolNear3D.TabIndex = 2;
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
		this.buttonImport3DModel.Click += new System.EventHandler(buttonImportNear3DModel_Click);
		this.buttonExport3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExport3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonExport3DModel.Image");
		this.buttonExport3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExport3DModel.Name = "buttonExport3DModel";
		this.buttonExport3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonExport3DModel.Text = "Export 3D Model";
		this.buttonExport3DModel.Click += new System.EventHandler(buttonExportNear3DModel_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonRemove3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonRemove3DModel.Image");
		this.buttonRemove3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove3DModel.Name = "buttonRemove3DModel";
		this.buttonRemove3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove3DModel.Text = "Remove 3D Model";
		this.buttonRemove3DModel.Click += new System.EventHandler(buttonRemoveNear3DModel_Click);
		this.buttonCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCamera.Image = (System.Drawing.Image)resources.GetObject("buttonCamera.Image");
		this.buttonCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCamera.Name = "buttonCamera";
		this.buttonCamera.Size = new System.Drawing.Size(23, 22);
		this.buttonCamera.Text = "Picture";
		this.buttonCamera.Click += new System.EventHandler(buttonCamera_Click);
		this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
		this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.AutoScroll = true;
		this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
		this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
		this.splitContainer1.Size = new System.Drawing.Size(1357, 807);
		this.splitContainer1.SplitterDistance = 516;
		this.splitContainer1.TabIndex = 2;
		this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer3.Location = new System.Drawing.Point(0, 0);
		this.splitContainer3.Name = "splitContainer3";
		this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer3.Panel1.AutoScroll = true;
		this.splitContainer3.Panel1.Controls.Add(this.multiViewer2DTextures);
		this.splitContainer3.Panel2.AutoScroll = true;
		this.splitContainer3.Panel2.Controls.Add(this.checkBox1);
		this.splitContainer3.Panel2.Controls.Add(this.textBox1);
		this.splitContainer3.Panel2.Controls.Add(this.labelId);
		this.splitContainer3.Panel2.Controls.Add(this.checkIsGenericBall);
		this.splitContainer3.Panel2.Controls.Add(this.labelBallName);
		this.splitContainer3.Panel2.Controls.Add(this.textBalllName);
		this.splitContainer3.Size = new System.Drawing.Size(516, 807);
		this.splitContainer3.SplitterDistance = 562;
		this.splitContainer3.TabIndex = 1;
		this.multiViewer2DTextures.AutoTransparency = false;
		this.multiViewer2DTextures.Bitmaps = null;
		this.multiViewer2DTextures.CheckBitmapSize = true;
		this.multiViewer2DTextures.Dock = System.Windows.Forms.DockStyle.Fill;
		this.multiViewer2DTextures.FixedSize = true;
		this.multiViewer2DTextures.FullSizeButton = true;
		this.multiViewer2DTextures.LabelText = "Texture";
		this.multiViewer2DTextures.Location = new System.Drawing.Point(0, 0);
		this.multiViewer2DTextures.Name = "multiViewer2DTextures";
		this.multiViewer2DTextures.ShowDeleteButton = true;
		this.multiViewer2DTextures.Size = new System.Drawing.Size(512, 558);
		this.multiViewer2DTextures.TabIndex = 0;
		this.checkBox1.AutoSize = true;
		this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.ballBindingSource, "IsLicensed", true));
		this.checkBox1.Location = new System.Drawing.Point(9, 76);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBox1.Size = new System.Drawing.Size(129, 17);
		this.checkBox1.TabIndex = 5;
		this.checkBox1.Text = "Licensed                    ";
		this.checkBox1.UseVisualStyleBackColor = true;
		this.ballBindingSource.DataSource = typeof(FifaLibrary.Ball);
		this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ballBindingSource, "Id", true));
		this.textBox1.Enabled = false;
		this.textBox1.Location = new System.Drawing.Point(65, 3);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(73, 20);
		this.textBox1.TabIndex = 4;
		this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.labelId.AutoSize = true;
		this.labelId.Location = new System.Drawing.Point(10, 6);
		this.labelId.Name = "labelId";
		this.labelId.Size = new System.Drawing.Size(16, 13);
		this.labelId.TabIndex = 3;
		this.labelId.Text = "Id";
		this.labelId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.checkIsGenericBall.AutoSize = true;
		this.checkIsGenericBall.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.ballBindingSource, "IsAvailable", true));
		this.checkIsGenericBall.Location = new System.Drawing.Point(10, 53);
		this.checkIsGenericBall.Name = "checkIsGenericBall";
		this.checkIsGenericBall.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkIsGenericBall.Size = new System.Drawing.Size(128, 17);
		this.checkIsGenericBall.TabIndex = 2;
		this.checkIsGenericBall.Text = "Visible in Game Menu";
		this.checkIsGenericBall.UseVisualStyleBackColor = true;
		this.labelBallName.AutoSize = true;
		this.labelBallName.Location = new System.Drawing.Point(10, 30);
		this.labelBallName.Name = "labelBallName";
		this.labelBallName.Size = new System.Drawing.Size(35, 13);
		this.labelBallName.TabIndex = 0;
		this.labelBallName.Text = "Name";
		this.labelBallName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textBalllName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ballBindingSource, "Name", true));
		this.textBalllName.Location = new System.Drawing.Point(65, 27);
		this.textBalllName.Name = "textBalllName";
		this.textBalllName.Size = new System.Drawing.Size(312, 20);
		this.textBalllName.TabIndex = 1;
		this.textBalllName.TextChanged += new System.EventHandler(textBalllName_TextChanged);
		this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer2.Location = new System.Drawing.Point(0, 0);
		this.splitContainer2.Name = "splitContainer2";
		this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer2.Panel1.AutoScroll = true;
		this.splitContainer2.Panel1.Controls.Add(this.group3D);
		this.splitContainer2.Panel2.AutoScroll = true;
		this.splitContainer2.Panel2.Controls.Add(this.viewer2DBallPicture);
		this.splitContainer2.Size = new System.Drawing.Size(837, 807);
		this.splitContainer2.SplitterDistance = 562;
		this.splitContainer2.TabIndex = 0;
		this.viewer2DBallPicture.AutoTransparency = false;
		this.viewer2DBallPicture.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DBallPicture.ButtonStripVisible = true;
		this.viewer2DBallPicture.CurrentBitmap = null;
		this.viewer2DBallPicture.ExtendedFormat = false;
		this.viewer2DBallPicture.FullSizeButton = false;
		this.viewer2DBallPicture.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DBallPicture.ImageSize = new System.Drawing.Size(512, 256);
		this.viewer2DBallPicture.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DBallPicture.Location = new System.Drawing.Point(3, 3);
		this.viewer2DBallPicture.Name = "viewer2DBallPicture";
		this.viewer2DBallPicture.RemoveButton = false;
		this.viewer2DBallPicture.ShowButton = false;
		this.viewer2DBallPicture.ShowButtonChecked = true;
		this.viewer2DBallPicture.Size = new System.Drawing.Size(362, 207);
		this.viewer2DBallPicture.TabIndex = 3;
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
		this.pickUpControl.Size = new System.Drawing.Size(1357, 25);
		this.pickUpControl.TabIndex = 0;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1357, 832);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "BallForm";
		this.Text = "Ball";
		base.Load += new System.EventHandler(BallForm_Load);
		this.group3D.ResumeLayout(false);
		this.group3D.PerformLayout();
		this.toolNear3D.ResumeLayout(false);
		this.toolNear3D.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.splitContainer3.Panel1.ResumeLayout(false);
		this.splitContainer3.Panel2.ResumeLayout(false);
		this.splitContainer3.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer3).EndInit();
		this.splitContainer3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ballBindingSource).EndInit();
		this.splitContainer2.Panel1.ResumeLayout(false);
		this.splitContainer2.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).EndInit();
		this.splitContainer2.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
