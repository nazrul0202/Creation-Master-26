using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FifaControls;
using FifaLibrary;

namespace CreationMaster;

public class KitForm : Form
{
	private PrivateFontCollection m_FontCollection;

	private Graphics m_FontGraphics;

	private SolidBrush m_FontBrush = new SolidBrush(Color.Black);

	private Kit m_CurrentKit;

	private bool m_IsLoaded;

	private NewKitCreator m_NewKitCreator = new NewKitCreator();

	private string m_FontnameCurrentFolder = FifaEnvironment.ExportFolder;

	private float[] m_CopyPosition = new float[32];

	private bool m_UpdatingLock;

	private bool m_PositionsLock;

	private readonly Dictionary<string, Bitmap[]> m_Fc26TextureCache = new Dictionary<string, Bitmap[]>(StringComparer.OrdinalIgnoreCase);

	private readonly Queue<string> m_Fc26TextureCacheOrder = new Queue<string>();

	private const int Fc26TextureCacheLimit = 16;

	private int m_Fc26TextureRequest;

	private Label m_Fc26Kit3dStatus;

	private static Color[] c_ColorPalette = new Color[20]
	{
		Color.Transparent,
		Color.White,
		Color.Black,
		Color.Blue,
		Color.Red,
		Color.Yellow,
		Color.Green,
		Color.Orange,
		Color.DarkViolet,
		Color.FromArgb(90, 60, 30),
		Color.Pink,
		Color.DarkRed,
		Color.LightSkyBlue,
		Color.DarkBlue,
		Color.Gray,
		Color.FromArgb(200, 200, 100),
		Color.FromArgb(160, 140, 85),
		Color.Gold,
		Color.OrangeRed,
		Color.ForestGreen
	};

	private Viewer3D viewer3DKit;

	private Viewer3D viewer3DMinikit;

	private IContainer components;

	public PickUpControl pickUpControl;

	private SplitContainer splitContainer1;

	private SplitContainer splitContainer3;

	private Viewer2D viewer2DMinikit;

	private SplitContainer splitContainer2;

	private SplitContainer splitContainer4;

	private SplitContainer splitContainer5;

	private MultiViewer2D multiViewer2DKit;

	private MultiViewer2D multiViewer2DJerseyNumbers;

	private MultiViewer2D multiViewer2DShortsNumbers;

	private GroupBox group3D;

	private ToolStrip toolNear3D;

	private ToolStripButton buttonShow3DModel;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonCamera;

	private FlowLayoutPanel flowPanel;

	private NumericUpDown numericCollar;

	private GroupBox groupCollar;

	private Label labelCollar;

	private BindingSource kitBindingSource;

	private CheckBox checkHasBackname;

	private CheckBox checkFrontNumber;

	private CheckBox checkShortsNumber;

	private Label labelNameFont;

	private NumericUpDown numericNameFont;

	private Label label1;

	private ComboBox comboNameLayout;

	private NumericUpDown numericShortsNumberFont;

	private NumericUpDown numericJerseyNumberFont;

	private PictureBox pictureNameColor;

	private ColorDialog colorDialog;

	private PictureBox pictureJerseyNumberColor;

	private PictureBox pictureShortsNumberColor;

	private PictureBox pictureTeamTerColor;

	private PictureBox pictureTeamPrimColor;

	private PictureBox pictureTeamSecColor;

	private GroupBox groupName;

	private CheckBox checkHasAdvertising;

	private NumericUpDown numericBottom;

	private NumericUpDown numericTop;

	private NumericUpDown numericLeft;

	private NumericUpDown numericRight;

	private ComboBox comboKitType;

	private ComboBox comboTeam;

	private BindingSource teamListBindingSource;

	private Label labelKitType;

	private Label labelTeam;

	private Label label2;

	private GroupBox groupPositions;

	private CheckBox checkLink;

	private Label label3;

	private ComboBox comboBox1;

	private ToolStrip toolStrip3D;

	private ToolStripButton buttonFrontNumber;

	private ToolStripButton buttonShortsBadge;

	private ToolStripButton buttonJerseyBadge;

	private ToolStripButton buttonShortsNumber;

	private ToolStripButton buttonBackName;

	private ToolStripButton buttonBackNumber;

	private ToolStripButton buttonNameCurvature;

	private ToolStripButton buttonRefresh3D;

	private ToolStripButton buttonShowNumbers3D;

	private FontDialog fontDialog;

	private ToolStrip toolStripNameFont;

	private ToolStripButton buttonPreviewNameFont;

	private ToolStripButton buttonImportNameFont;

	private ToolStripButton buttonExportNameFont;

	private ToolStripButton buttonDeleteNameFont;

	private Process processFontView;

	private ToolStripButton buttonCopyPositions;

	private ToolStripButton buttonPastePositions;

	private FontDialog fontDialog1;

	private CheckBox checkIsFitting;

	private ImageList imageListCollar;

	private Label labelCollarImage;

	private PictureBox pictureFont;

	private NumericUpDown numericTeamId;

	private Label labelTeamId;

	private Button buttonExportAllKits;

	private Button buttonMinikitPicture;

	public KitForm()
	{
		base.Visible = false;
		InitializeComponent();
		viewer3DKit = new Viewer3D();
		viewer3DKit.AmbientColor = Color.White;
		viewer3DKit.BackColor = Color.Gray;
		viewer3DKit.BorderStyle = BorderStyle.Fixed3D;
		viewer3DKit.Dock = DockStyle.Fill;
		viewer3DKit.LightDirectionX = 0f;
		viewer3DKit.LightDirectionY = 0f;
		viewer3DKit.LightDirectionZ = -1f;
		viewer3DKit.LightX = 100f;
		viewer3DKit.LightY = 10f;
		viewer3DKit.LightZ = 100f;
		viewer3DKit.Location = new Point(3, 16);
		viewer3DKit.Name = "viewer3DKit";
		viewer3DKit.RotationX = 0.1f;
		viewer3DKit.RotationY = 0f;
		viewer3DKit.RotationYCoeff = 0.01f;
		viewer3DKit.Size = new Size(427, 528);
		viewer3DKit.TabIndex = 1;
		viewer3DKit.ViewX = 0f;
		viewer3DKit.ViewY = 95f;
		viewer3DKit.ViewZ = 190f;
		viewer3DKit.ZbufferRenderState = null;
		group3D.Controls.Add(viewer3DKit);
		m_Fc26Kit3dStatus = new Label
		{
			AutoEllipsis = true,
			BackColor = Color.FromArgb(32, 32, 32),
			Dock = DockStyle.Bottom,
			ForeColor = Color.White,
			Height = 26,
			Name = "labelFc26Kit3dStatus",
			Padding = new Padding(6, 4, 6, 0),
			Text = "3D kit preview",
			Visible = false
		};
		group3D.Controls.Add(m_Fc26Kit3dStatus);
		m_Fc26Kit3dStatus.BringToFront();
		viewer3DMinikit = new Viewer3D();
		viewer3DMinikit.AmbientColor = Color.White;
		viewer3DMinikit.BackColor = Color.Gray;
		viewer3DMinikit.BorderStyle = BorderStyle.Fixed3D;
		viewer3DMinikit.LightDirectionX = 0f;
		viewer3DMinikit.LightDirectionY = 0f;
		viewer3DMinikit.LightDirectionZ = -1f;
		viewer3DMinikit.LightX = 100f;
		viewer3DMinikit.LightY = 10f;
		viewer3DMinikit.LightZ = 100f;
		viewer3DMinikit.Location = new Point(4, 19);
		viewer3DMinikit.Name = "viewer3DMinikit";
		viewer3DMinikit.RotationX = 0.1f;
		viewer3DMinikit.RotationY = 0f;
		viewer3DMinikit.RotationYCoeff = 0.01f;
		viewer3DMinikit.Size = new Size(258, 258);
		viewer3DMinikit.TabIndex = 156;
		viewer3DMinikit.ViewX = 0f;
		viewer3DMinikit.ViewY = 95f;
		viewer3DMinikit.ViewZ = 190f;
		viewer3DMinikit.ZbufferRenderState = null;
		groupCollar.Controls.Add(viewer3DMinikit);
		m_FontGraphics = pictureFont.CreateGraphics();
		m_FontGraphics.Clear(Color.White);
		pickUpControl.SelectObject = SelectKit;
		pickUpControl.CreateObject = CreateKit;
		pickUpControl.DeleteObject = DeleteKit;
		pickUpControl.CloneObject = CloneKit;
		pickUpControl.RefreshObject = RefreshKit;
		viewer2DMinikit.ImageImport = ImportImageMinikit;
		viewer2DMinikit.ImageDelete = DeleteMinikit;
		viewer2DMinikit.ButtonStripVisible = true;
		viewer2DMinikit.RemoveButton = true;
		viewer2DMinikit.FullSizeButton = true;
		multiViewer2DKit.Rx3ExportDelegate = ExportRx3Kit;
		multiViewer2DKit.Rx3ImportDelegate = ImportRx3Kit;
		multiViewer2DKit.Rx3SaveDelegate = SaveBitmapsKit;
		multiViewer2DKit.Rx3DeleteDelegate = DeleteRx3Kit;
		multiViewer2DKit.FullSizeButton = true;
		multiViewer2DJerseyNumbers.Rx3ExportDelegate = ExportRx3JerseyNumbers;
		multiViewer2DJerseyNumbers.Rx3ImportDelegate = ImportRx3JerseyNumbers;
		multiViewer2DJerseyNumbers.Rx3SaveDelegate = SaveBitmapsJerseyNumbers;
		multiViewer2DJerseyNumbers.Rx3DeleteDelegate = DeleteRx3JerseyNumbers;
		multiViewer2DJerseyNumbers.ShowDeleteButton = true;
		multiViewer2DShortsNumbers.Rx3ExportDelegate = ExportRx3ShortsNumbers;
		multiViewer2DShortsNumbers.Rx3ImportDelegate = ImportRx3ShortsNumbers;
		multiViewer2DShortsNumbers.Rx3SaveDelegate = SaveBitmapsShortsNumbers;
		multiViewer2DShortsNumbers.Rx3DeleteDelegate = DeleteRx3ShortsNumbers;
		multiViewer2DShortsNumbers.ShowDeleteButton = true;
		for (int i = 0; i < 32; i++)
		{
			m_CopyPosition[i] = 0f;
		}
		viewer3DMinikit.ViewX = 0f;
		viewer3DMinikit.ViewY = 124f;
		viewer3DMinikit.ViewZ = 120f;
	}

	private FontFamily LoadFontFamily(string fileName, out PrivateFontCollection _myFonts)
	{
		_myFonts = new PrivateFontCollection();
		FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		byte[] array = new byte[fileStream.Length];
		fileStream.Read(array, 0, array.Length);
		fileStream.Close();
		IntPtr memory = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
		_myFonts.AddMemoryFont(memory, array.Length);
		return _myFonts.Families[0];
	}

	public void Clean()
	{
		base.Visible = false;
	}

	public Kit RefreshKit(object sender, object obj)
	{
		Preset();
		ReloadKit(m_CurrentKit);
		return m_CurrentKit;
	}

	public void Preset()
	{
		Kit.Prepare3DModels();
		m_NewKitCreator.SetTeams(FifaEnvironment.Teams);
		m_NewKitCreator.KitList = FifaEnvironment.Kits;
		if (FifaEnvironment.Year == 26)
		{
			numericShortsNumberFont.Maximum = 9999;
			numericJerseyNumberFont.Maximum = 9999;
			numericNameFont.Maximum = 9999;
		}
		else
		{
			Table table = FifaEnvironment.FifaDb.Table[TI.teamkits];
			numericShortsNumberFont.Maximum = table.TableDescriptor.MaxValues[FI.teamkits_shortsnumberfonttype];
			numericJerseyNumberFont.Maximum = table.TableDescriptor.MaxValues[FI.teamkits_numberfonttype];
			numericNameFont.Maximum = table.TableDescriptor.MaxValues[FI.teamkits_jerseynamefonttype];
		}
		IdArrayList[] filterValues = new IdArrayList[5]
		{
			null,
			FifaEnvironment.Teams,
			FifaEnvironment.Leagues,
			FifaEnvironment.Countries,
			new DummyKitList()
		};
		pickUpControl.FilterValues = filterValues;
		teamListBindingSource.DataSource = FifaEnvironment.Teams;
		comboTeam.DataSource = teamListBindingSource;
		pickUpControl.ObjectList = FifaEnvironment.Kits;
		checkIsFitting.Visible = FifaEnvironment.Year > 14;
	}

	private Kit SelectKit(object sender, object obj)
	{
		Kit kit = (Kit)obj;
		LoadKit(kit);
		return kit;
	}

	private Kit CloneKit(object sender, object obj)
	{
		m_NewKitCreator.SetTeams(FifaEnvironment.Teams);
		m_NewKitCreator.SourceKit = m_CurrentKit;
		DialogResult dialogResult = m_NewKitCreator.ShowDialog();
		if (m_NewKitCreator.NewKit == null)
		{
			if (dialogResult == DialogResult.OK)
			{
				FifaEnvironment.UserMessages.ShowMessage(5060, m_NewKitCreator.NewId);
			}
			return null;
		}
		if (dialogResult == DialogResult.Cancel)
		{
			return null;
		}
		Kit kit = (Kit)obj;
		kit.CloneTextures(m_NewKitCreator.NewKit);
		if (kit.Positions != null)
		{
			for (int i = 0; i < kit.Positions.Length; i++)
			{
				m_NewKitCreator.NewKit.Positions[i] = kit.Positions[i];
			}
		}
		return m_NewKitCreator.NewKit;
	}

	private Kit CreateKit(object sender, object obj)
	{
		m_NewKitCreator.SetTeams(FifaEnvironment.Teams);
		m_NewKitCreator.SourceKit = m_CurrentKit;
		DialogResult dialogResult = m_NewKitCreator.ShowDialog();
		if (m_NewKitCreator.NewKit == null)
		{
			if (dialogResult == DialogResult.OK)
			{
				FifaEnvironment.UserMessages.ShowMessage(5060, m_NewKitCreator.NewId);
			}
			return null;
		}
		return m_NewKitCreator.NewKit;
	}

	private Kit DeleteKit(object sender, object obj)
	{
		Kit kit = (Kit)obj;
		m_CurrentKit.Team?.m_KitList.Remove(kit);
		FifaEnvironment.Kits.DeleteKit(kit);
		m_CurrentKit = null;
		return null;
	}

	public void LoadKit(Kit kit)
	{
		if (m_IsLoaded && m_CurrentKit != kit)
		{
			m_UpdatingLock = true;
			m_CurrentKit = kit;
			kitBindingSource.DataSource = m_CurrentKit;
			if (FifaEnvironment.Year == 26)
			{
				multiViewer2DKit.Bitmaps = GetCurrentKitTextures();
				LoadFc26KitTextureAsync(m_CurrentKit);
			}
			else if (multiViewer2DKit.buttonShow.Checked)
			{
				multiViewer2DKit.Bitmaps = m_CurrentKit.GetKitTextures();
			}
			multiViewer2DJerseyNumbers.Bitmaps = NumberFont.GetNumberFont(m_CurrentKit.jerseyNumberFont, m_CurrentKit.jerseyNumberColor);
			multiViewer2DShortsNumbers.Bitmaps = NumberFont.GetNumberFont(m_CurrentKit.shortsNumberFont, m_CurrentKit.shortsNumberColor);
			viewer2DMinikit.CurrentBitmap = m_CurrentKit.GetMiniKit();
			pictureJerseyNumberColor.BackColor = SafePaletteColor(m_CurrentKit.jerseyNumberColor);
			pictureShortsNumberColor.BackColor = SafePaletteColor(m_CurrentKit.shortsNumberColor);
			labelCollarImage.ImageIndex = kit.jerseyCollar;
			LoadPositions();
			if (FifaEnvironment.Year != 26 || GetCurrentKitTextures() != null)
			{
				Show3DKit();
				Show3DMinikit();
			}
			ShowFont();
			m_UpdatingLock = false;
		}
	}

	private static Color SafePaletteColor(int index)
	{
		return index >= 0 && index < c_ColorPalette.Length ? c_ColorPalette[index] : Color.Gray;
	}

	public void AuditFc26RecordsForSmoke()
	{
		if (FifaEnvironment.Kits.Count == 0) return;
		var samples = new[] { 0, FifaEnvironment.Kits.Count / 2, FifaEnvironment.Kits.Count - 1 };
		foreach (var index in samples)
		{
			m_CurrentKit = null;
			LoadKit((Kit)FifaEnvironment.Kits[index]);
		}
	}

	private string Fc26KitTextureKey(Kit kit)
	{
		return kit.teamid + ":" + kit.kittype;
	}

	private Bitmap[] GetCurrentKitTextures()
	{
		if (m_CurrentKit == null) return null;
		if (FifaEnvironment.Year != 26) return m_CurrentKit.GetKitTextures();
		Bitmap[] textures;
		return m_Fc26TextureCache.TryGetValue(Fc26KitTextureKey(m_CurrentKit), out textures) ? textures : null;
	}

	private async void LoadFc26KitTextureAsync(Kit kit)
	{
		if (kit == null) return;
		int request = ++m_Fc26TextureRequest;
		string key = Fc26KitTextureKey(kit);
		viewer3DKit.ShowEmpty();
		viewer3DMinikit.ShowEmpty();
		SetFc26Kit3dStatus("Loading real FC26 kit texture...", Color.Gold);
		Bitmap[] cached;
		if (!m_Fc26TextureCache.TryGetValue(key, out cached))
		{
			string path;
			try
			{
				path = await System.Threading.Tasks.Task.Run(() => Fc26HostBridge.ExportKitTexture(kit.teamid, kit.kittype));
			}
			catch (Exception ex)
			{
				if (request == m_Fc26TextureRequest && m_CurrentKit == kit && !IsDisposed)
					SetFc26Kit3dStatus("FC26 kit export failed: " + ex.Message, Color.OrangeRed);
				return;
			}
			if (request != m_Fc26TextureRequest || m_CurrentKit != kit || IsDisposed) return;
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
			{
				SetFc26Kit3dStatus("No indexed FC26 texture is available for this kit.", Color.OrangeRed);
				return;
			}
			try
			{
				using (Image source = Image.FromFile(path))
				{
					Bitmap texture = new Bitmap(source);
					cached = new[] { texture, texture, texture, texture };
				}
				CacheFc26KitTextures(key, cached);
			}
			catch (Exception ex)
			{
				SetFc26Kit3dStatus("FC26 kit texture could not be decoded: " + ex.Message, Color.OrangeRed);
				return;
			}
		}
		if (request != m_Fc26TextureRequest || m_CurrentKit != kit || IsDisposed) return;
		if (multiViewer2DKit.buttonShow.Checked) multiViewer2DKit.Bitmaps = cached;
		SetFc26Kit3dStatus("Real FC26 texture loaded — team " + kit.teamid + ", kit " + kit.kittype + ".", Color.LightGreen);
		Show3DKit();
		Show3DMinikit();
	}

	private void SetFc26Kit3dStatus(string text, Color color)
	{
		if (m_Fc26Kit3dStatus == null) return;
		m_Fc26Kit3dStatus.Visible = FifaEnvironment.Year == 26;
		m_Fc26Kit3dStatus.ForeColor = color;
		m_Fc26Kit3dStatus.Text = text;
	}

	private void CacheFc26KitTextures(string key, Bitmap[] textures)
	{
		if (m_Fc26TextureCache.ContainsKey(key)) return;
		while (m_Fc26TextureCacheOrder.Count >= Fc26TextureCacheLimit)
		{
			string expiredKey = m_Fc26TextureCacheOrder.Dequeue();
			Bitmap[] expired;
			if (!m_Fc26TextureCache.TryGetValue(expiredKey, out expired)) continue;
			m_Fc26TextureCache.Remove(expiredKey);
			var disposed = new HashSet<Bitmap>();
			foreach (Bitmap bitmap in expired)
				if (bitmap != null && disposed.Add(bitmap)) bitmap.Dispose();
		}
		m_Fc26TextureCache[key] = textures;
		m_Fc26TextureCacheOrder.Enqueue(key);
	}

	public void LoadPositions()
	{
		m_PositionsLock = true;
		if (m_CurrentKit.Positions == null)
		{
			EnablePositions(enabled: false);
			return;
		}
		if (buttonBackName.Checked)
		{
			VerifyAndLoadPositions(12);
		}
		else if (buttonBackNumber.Checked)
		{
			VerifyAndLoadPositions(4);
		}
		else if (buttonNameCurvature.Checked)
		{
			numericLeft.Value = (decimal)m_CurrentKit.Positions[17];
			numericTop.Value = (decimal)m_CurrentKit.Positions[21];
			numericRight.Value = 0m;
			numericBottom.Value = 0m;
			EnablePositions(enabled: true);
		}
		else if (buttonFrontNumber.Checked)
		{
			VerifyAndLoadPositions(8);
		}
		else if (buttonJerseyBadge.Checked)
		{
			VerifyAndLoadPositions(0);
		}
		else if (buttonShortsBadge.Checked)
		{
			VerifyAndLoadPositions(24);
		}
		else if (buttonShortsNumber.Checked)
		{
			VerifyAndLoadPositions(28);
		}
		else
		{
			EnablePositions(enabled: false);
		}
		m_PositionsLock = false;
	}

	public void VerifyAndLoadPositions(int startingIndex)
	{
		if (m_CurrentKit.Positions[startingIndex] < 0f)
		{
			m_CurrentKit.Positions[startingIndex] = 0f;
		}
		if (m_CurrentKit.Positions[startingIndex] > 1f)
		{
			m_CurrentKit.Positions[startingIndex] = 1f;
		}
		if (m_CurrentKit.Positions[startingIndex + 1] < 0f)
		{
			m_CurrentKit.Positions[startingIndex + 1] = 0f;
		}
		if (m_CurrentKit.Positions[startingIndex + 1] > 1f)
		{
			m_CurrentKit.Positions[startingIndex + 1] = 1f;
		}
		if (m_CurrentKit.Positions[startingIndex + 2] < 0f)
		{
			m_CurrentKit.Positions[startingIndex + 2] = 0f;
		}
		if (m_CurrentKit.Positions[startingIndex + 2] > 1f)
		{
			m_CurrentKit.Positions[startingIndex + 2] = 1f;
		}
		if (m_CurrentKit.Positions[startingIndex + 3] < 0f)
		{
			m_CurrentKit.Positions[startingIndex + 3] = 0f;
		}
		if (m_CurrentKit.Positions[startingIndex + 3] > 1f)
		{
			m_CurrentKit.Positions[startingIndex + 3] = 1f;
		}
		numericLeft.Value = (decimal)m_CurrentKit.Positions[startingIndex];
		numericTop.Value = (decimal)m_CurrentKit.Positions[startingIndex + 1];
		numericRight.Value = (decimal)m_CurrentKit.Positions[startingIndex + 2];
		numericBottom.Value = (decimal)m_CurrentKit.Positions[startingIndex + 3];
		EnablePositions(enabled: true);
	}

	public void ChangePositions()
	{
		if (m_PositionsLock)
		{
			return;
		}
		if (!multiViewer2DKit.buttonSave.Enabled)
		{
			multiViewer2DKit.buttonSave.Enabled = true;
		}
		if (buttonBackName.Checked)
		{
			if (checkLink.Checked)
			{
				float num = (float)numericLeft.Value - m_CurrentKit.Positions[12];
				m_CurrentKit.Positions[12] += num;
				m_CurrentKit.Positions[14] += num;
				num = (float)numericTop.Value - m_CurrentKit.Positions[13];
				m_CurrentKit.Positions[13] += num;
				m_CurrentKit.Positions[15] += num;
			}
			else
			{
				m_CurrentKit.Positions[12] = (float)numericLeft.Value;
				m_CurrentKit.Positions[13] = (float)numericTop.Value;
				m_CurrentKit.Positions[14] = (float)numericRight.Value;
				m_CurrentKit.Positions[15] = (float)numericBottom.Value;
			}
			CheckPositions();
			LoadPositions();
		}
		else if (buttonBackNumber.Checked)
		{
			if (checkLink.Checked)
			{
				float num2 = (float)numericLeft.Value - m_CurrentKit.Positions[4];
				m_CurrentKit.Positions[4] += num2;
				m_CurrentKit.Positions[6] += num2;
				num2 = (float)numericTop.Value - m_CurrentKit.Positions[5];
				m_CurrentKit.Positions[5] += num2;
				m_CurrentKit.Positions[7] += num2;
			}
			else
			{
				m_CurrentKit.Positions[4] = (float)numericLeft.Value;
				m_CurrentKit.Positions[5] = (float)numericTop.Value;
				m_CurrentKit.Positions[6] = (float)numericRight.Value;
				m_CurrentKit.Positions[7] = (float)numericBottom.Value;
			}
			CheckPositions();
			LoadPositions();
		}
		else if (buttonNameCurvature.Checked)
		{
			m_CurrentKit.Positions[17] = (float)numericLeft.Value;
			m_CurrentKit.Positions[21] = (float)numericTop.Value;
			CheckPositions();
			LoadPositions();
		}
		else if (buttonFrontNumber.Checked)
		{
			if (checkLink.Checked)
			{
				float num3 = (float)numericLeft.Value - m_CurrentKit.Positions[8];
				m_CurrentKit.Positions[8] += num3;
				m_CurrentKit.Positions[10] += num3;
				num3 = (float)numericTop.Value - m_CurrentKit.Positions[9];
				m_CurrentKit.Positions[9] += num3;
				m_CurrentKit.Positions[11] += num3;
			}
			else
			{
				m_CurrentKit.Positions[8] = (float)numericLeft.Value;
				m_CurrentKit.Positions[9] = (float)numericTop.Value;
				m_CurrentKit.Positions[10] = (float)numericRight.Value;
				m_CurrentKit.Positions[11] = (float)numericBottom.Value;
			}
			CheckPositions();
			LoadPositions();
		}
		else if (buttonJerseyBadge.Checked)
		{
			if (checkLink.Checked)
			{
				float num4 = (float)numericLeft.Value - m_CurrentKit.Positions[0];
				m_CurrentKit.Positions[0] += num4;
				m_CurrentKit.Positions[2] += num4;
				num4 = (float)numericTop.Value - m_CurrentKit.Positions[1];
				m_CurrentKit.Positions[1] += num4;
				m_CurrentKit.Positions[3] += num4;
			}
			else
			{
				m_CurrentKit.Positions[0] = (float)numericLeft.Value;
				m_CurrentKit.Positions[1] = (float)numericTop.Value;
				m_CurrentKit.Positions[2] = (float)numericRight.Value;
				m_CurrentKit.Positions[3] = (float)numericBottom.Value;
			}
			CheckPositions();
			LoadPositions();
		}
		else if (buttonShortsBadge.Checked)
		{
			if (checkLink.Checked)
			{
				float num5 = (float)numericLeft.Value - m_CurrentKit.Positions[24];
				m_CurrentKit.Positions[24] += num5;
				m_CurrentKit.Positions[26] += num5;
				num5 = (float)numericTop.Value - m_CurrentKit.Positions[25];
				m_CurrentKit.Positions[25] += num5;
				m_CurrentKit.Positions[27] += num5;
			}
			else
			{
				m_CurrentKit.Positions[24] = (float)numericLeft.Value;
				m_CurrentKit.Positions[25] = (float)numericTop.Value;
				m_CurrentKit.Positions[26] = (float)numericRight.Value;
				m_CurrentKit.Positions[27] = (float)numericBottom.Value;
			}
			CheckPositions();
			LoadPositions();
		}
		else if (buttonShortsNumber.Checked)
		{
			if (checkLink.Checked)
			{
				float num6 = (float)numericLeft.Value - m_CurrentKit.Positions[28];
				m_CurrentKit.Positions[28] += num6;
				m_CurrentKit.Positions[30] += num6;
				num6 = (float)numericTop.Value - m_CurrentKit.Positions[29];
				m_CurrentKit.Positions[29] += num6;
				m_CurrentKit.Positions[31] += num6;
			}
			else
			{
				m_CurrentKit.Positions[28] = (float)numericLeft.Value;
				m_CurrentKit.Positions[29] = (float)numericTop.Value;
				m_CurrentKit.Positions[30] = (float)numericRight.Value;
				m_CurrentKit.Positions[31] = (float)numericBottom.Value;
			}
			CheckPositions();
			LoadPositions();
		}
	}

	public void CheckPositions()
	{
		for (int i = 0; i < 32; i++)
		{
			if ((double)m_CurrentKit.Positions[i] < 0.0)
			{
				m_CurrentKit.Positions[i] = 0f;
			}
			if ((double)m_CurrentKit.Positions[i] > 1.0)
			{
				m_CurrentKit.Positions[i] = 1f;
			}
		}
	}

	private void EnablePositions(bool enabled)
	{
		if (!enabled)
		{
			numericLeft.Enabled = enabled;
			numericTop.Enabled = enabled;
			numericRight.Enabled = enabled;
			numericBottom.Enabled = enabled;
		}
		else if (checkLink.Checked || buttonNameCurvature.Checked)
		{
			numericLeft.Enabled = enabled;
			numericTop.Enabled = enabled;
			numericRight.Enabled = !enabled;
			numericBottom.Enabled = !enabled;
		}
		else
		{
			numericLeft.Enabled = enabled;
			numericTop.Enabled = enabled;
			numericRight.Enabled = enabled;
			numericBottom.Enabled = enabled;
		}
	}

	public void ReloadKit(Kit kit)
	{
		m_CurrentKit = null;
		LoadKit(kit);
	}

	private void KitForm_Load(object sender, EventArgs e)
	{
		m_IsLoaded = true;
		Preset();
	}

	private bool ImportImageMinikit(object sender, Bitmap bitmap)
	{
		return m_CurrentKit.SetMiniKit(bitmap);
	}

	private bool DeleteMinikit(object sender)
	{
		return m_CurrentKit.DeleteMiniKit();
	}

	private bool SaveBitmapsKit(object sender, Bitmap[] bitmaps)
	{
		bool result = m_CurrentKit.SetKitTextures(bitmaps);
		ReloadKit(m_CurrentKit);
		return result;
	}

	private bool ExportRx3Kit(object sender, string exportDir)
	{
		return m_CurrentKit.ExportKitTextures(exportDir);
	}

	private bool ImportRx3Kit(object sender, string rx3FileName)
	{
		bool num = m_CurrentKit.ImportKitTextures(rx3FileName);
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	private bool DeleteRx3Kit(object sender)
	{
		bool num = m_CurrentKit.DeleteKitTextures();
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	private bool SaveBitmapsJerseyNumbers(object sender, Bitmap[] bitmaps)
	{
		bool num = NumberFont.SetNumberFont(m_CurrentKit.jerseyNumberFont, m_CurrentKit.jerseyNumberColor, bitmaps);
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	private bool ExportRx3JerseyNumbers(object sender, string exportDir)
	{
		return NumberFont.Export(m_CurrentKit.jerseyNumberFont, m_CurrentKit.jerseyNumberColor, exportDir);
	}

	private bool ImportRx3JerseyNumbers(object sender, string rx3FileName)
	{
		bool num = NumberFont.Import(m_CurrentKit.jerseyNumberFont, m_CurrentKit.jerseyNumberColor, rx3FileName);
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	private bool DeleteRx3JerseyNumbers(object sender)
	{
		bool num = NumberFont.Delete(m_CurrentKit.jerseyNumberFont, m_CurrentKit.jerseyNumberColor);
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	private bool SaveBitmapsShortsNumbers(object sender, Bitmap[] bitmaps)
	{
		bool num = NumberFont.SetNumberFont(m_CurrentKit.shortsNumberFont, m_CurrentKit.shortsNumberColor, bitmaps);
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	private bool ExportRx3ShortsNumbers(object sender, string exportDir)
	{
		return NumberFont.Export(m_CurrentKit.shortsNumberFont, m_CurrentKit.shortsNumberColor, exportDir);
	}

	private bool ImportRx3ShortsNumbers(object sender, string rx3FileName)
	{
		bool num = NumberFont.Import(m_CurrentKit.shortsNumberFont, m_CurrentKit.shortsNumberColor, rx3FileName);
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	private bool DeleteRx3ShortsNumbers(object sender)
	{
		bool num = NumberFont.Delete(m_CurrentKit.shortsNumberFont, m_CurrentKit.shortsNumberColor);
		if (num)
		{
			ReloadKit(m_CurrentKit);
		}
		return num;
	}

	public void Show3DMinikit()
	{
		if (!buttonShow3DModel.Checked)
		{
			viewer3DMinikit.ShowEmpty();
			return;
		}
		Bitmap[] kitTextures = GetCurrentKitTextures();
		if (kitTextures == null || kitTextures.Length < 2 || kitTextures[0] == null || kitTextures[1] == null ||
			m_CurrentKit?.Positions == null || m_CurrentKit.Positions.Length < 4)
		{
			viewer3DMinikit.ShowEmpty();
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		Bitmap bitmap = null;
		if (kitTextures != null)
		{
			bitmap = kitTextures[1];
		}
		Rectangle destRectangle = new Rectangle((int)((float)bitmap.Width * m_CurrentKit.Positions[0]), (int)((float)bitmap.Height * m_CurrentKit.Positions[1]), (int)((float)bitmap.Width * (m_CurrentKit.Positions[2] - m_CurrentKit.Positions[0])), (int)((float)bitmap.Height * (m_CurrentKit.Positions[3] - m_CurrentKit.Positions[1])));
		if (destRectangle.Width > 0 && destRectangle.Height > 0)
		{
			bitmap = GraphicUtil.Overlap(bitmap, kitTextures[0], destRectangle);
		}
		viewer3DMinikit.Clean(3);
		bitmap = GraphicUtil.EmbossBitmap(bitmap, Kit.s_JerseyWrinkle);
		if (m_CurrentKit.jerseyCollar >= 0 && m_CurrentKit.jerseyCollar < Kit.s_JerseyModelMinikit.Length && Kit.s_JerseyModelMinikit[m_CurrentKit.jerseyCollar] != null)
		{
			Kit.s_JerseyModelMinikit[m_CurrentKit.jerseyCollar].TextureBitmap = bitmap;
			viewer3DMinikit.SetMesh(0, Kit.s_JerseyModelMinikit[m_CurrentKit.jerseyCollar]);
		}
		viewer3DMinikit.Render();
		Cursor.Current = Cursors.Default;
	}

	public void Show3DKit()
	{
		if (!buttonShow3DModel.Checked)
		{
			viewer3DKit.ShowEmpty();
			return;
		}
		Bitmap[] kitTextures = GetCurrentKitTextures();
		if (kitTextures == null || kitTextures.Length < 4 || kitTextures[0] == null || kitTextures[1] == null || kitTextures[3] == null ||
			m_CurrentKit?.Positions == null || m_CurrentKit.Positions.Length < 32)
		{
			viewer3DKit.ShowEmpty();
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		Bitmap bitmap = null;
		Bitmap bitmap2 = null;
		if (kitTextures != null)
		{
			bitmap = kitTextures[1];
			bitmap2 = kitTextures[3];
		}
		Rectangle destRectangle = new Rectangle((int)((float)bitmap2.Width * m_CurrentKit.Positions[24]), (int)((float)bitmap2.Height * m_CurrentKit.Positions[25]), (int)((float)bitmap2.Width * (m_CurrentKit.Positions[26] - m_CurrentKit.Positions[24])), (int)((float)bitmap2.Height * (m_CurrentKit.Positions[27] - m_CurrentKit.Positions[25])));
		if (destRectangle.Width > 0 && destRectangle.Height > 0)
		{
			bitmap2 = GraphicUtil.Overlap(bitmap2, kitTextures[0], destRectangle);
		}
		Rectangle destRectangle2 = new Rectangle((int)((float)bitmap.Width * m_CurrentKit.Positions[0]), (int)((float)bitmap.Height * m_CurrentKit.Positions[1]), (int)((float)bitmap.Width * (m_CurrentKit.Positions[2] - m_CurrentKit.Positions[0])), (int)((float)bitmap.Height * (m_CurrentKit.Positions[3] - m_CurrentKit.Positions[1])));
		if (destRectangle2.Width > 0 && destRectangle2.Height > 0)
		{
			bitmap = GraphicUtil.Overlap(bitmap, kitTextures[0], destRectangle2);
		}
		if (buttonShowNumbers3D.Checked && m_CurrentKit.jerseyBackName)
		{
			Bitmap bitmap3 = new Bitmap(FifaEnvironment.LaunchDir + "\\Templates\\PlayerName.png");
			if (bitmap3 != null)
			{
				bitmap3 = GraphicUtil.ColorizeWhite(bitmap3, pictureNameColor.BackColor);
				Rectangle destRectangle3 = new Rectangle((int)((float)bitmap.Width * m_CurrentKit.Positions[14]), (int)((float)bitmap.Height * m_CurrentKit.Positions[15]), (int)((float)bitmap.Width * (m_CurrentKit.Positions[12] - m_CurrentKit.Positions[14])), (int)((float)bitmap.Height * (m_CurrentKit.Positions[13] - m_CurrentKit.Positions[15])));
				if (destRectangle3.Width > 0 && destRectangle3.Height > 0)
				{
					bitmap = GraphicUtil.Overlap(bitmap, bitmap3, destRectangle3);
				}
			}
		}
		if (buttonShowNumbers3D.Checked)
		{
			Bitmap bitmap4 = null;
			if (multiViewer2DShortsNumbers.GetCurrentBitmap() != null && m_CurrentKit.shortsNumber)
			{
				int num = (int)((float)bitmap2.Width * m_CurrentKit.Positions[28]);
				int num2 = (int)((float)bitmap2.Height * m_CurrentKit.Positions[29]);
				int num3 = (int)((float)bitmap2.Width * (m_CurrentKit.Positions[30] - m_CurrentKit.Positions[28]));
				int num4 = (int)((float)bitmap2.Height * (m_CurrentKit.Positions[31] - m_CurrentKit.Positions[29]));
				bitmap4 = (Bitmap)multiViewer2DShortsNumbers.GetCurrentBitmap().Clone();
				destRectangle = new Rectangle(num, num2, num3 / 2, num4);
				if (bitmap4 != null && destRectangle.Width > 0 && destRectangle.Height > 0)
				{
					bitmap2 = GraphicUtil.Overlap(bitmap2, bitmap4, destRectangle);
				}
				destRectangle = new Rectangle(num + num3 / 2, num2, num3 / 2, num4);
				if (bitmap4 != null && destRectangle.Width > 0 && destRectangle.Height > 0)
				{
					bitmap2 = GraphicUtil.Overlap(bitmap2, bitmap4, destRectangle);
				}
			}
			if (multiViewer2DJerseyNumbers.GetCurrentBitmap() != null)
			{
				int num5 = (int)((float)bitmap.Width * m_CurrentKit.Positions[8]);
				int num6 = (int)((float)bitmap.Height * m_CurrentKit.Positions[9]);
				int num7 = (int)((float)bitmap.Width * (m_CurrentKit.Positions[10] - m_CurrentKit.Positions[8]));
				int num8 = (int)((float)bitmap.Height * (m_CurrentKit.Positions[11] - m_CurrentKit.Positions[9]));
				bitmap4 = (Bitmap)multiViewer2DJerseyNumbers.GetCurrentBitmap().Clone();
				if (m_CurrentKit.jerseyFrontNumber)
				{
					destRectangle2 = new Rectangle(num5, num6, num7 / 2, num8);
					if (bitmap4 != null && destRectangle2.Width > 0 && destRectangle2.Height > 0)
					{
						bitmap = GraphicUtil.Overlap(bitmap, bitmap4, destRectangle2);
					}
					destRectangle2 = new Rectangle(num5 + num7 / 2, num6, num7 / 2, num8);
					if (bitmap4 != null && destRectangle2.Width > 0 && destRectangle2.Height > 0)
					{
						bitmap = GraphicUtil.Overlap(bitmap, bitmap4, destRectangle2);
					}
				}
				num5 = (int)((float)bitmap.Width * m_CurrentKit.Positions[6]);
				num6 = (int)((float)bitmap.Height * m_CurrentKit.Positions[7]);
				num7 = (int)((float)bitmap.Width * (m_CurrentKit.Positions[4] - m_CurrentKit.Positions[6]));
				num8 = (int)((float)bitmap.Height * (m_CurrentKit.Positions[5] - m_CurrentKit.Positions[7]));
				bitmap4.RotateFlip(RotateFlipType.Rotate180FlipNone);
				destRectangle2 = new Rectangle(num5, num6, num7 / 2, num8);
				if (bitmap4 != null && destRectangle2.Width > 0 && destRectangle2.Height > 0)
				{
					bitmap = GraphicUtil.Overlap(bitmap, bitmap4, destRectangle2);
				}
				destRectangle2 = new Rectangle(num5 + num7 / 2, num6, num7 / 2, num8);
				if (bitmap4 != null && destRectangle2.Width > 0 && destRectangle2.Height > 0)
				{
					bitmap = GraphicUtil.Overlap(bitmap, bitmap4, destRectangle2);
				}
			}
		}
		viewer3DKit.Clean(3);
		bitmap = GraphicUtil.EmbossBitmap(bitmap, Kit.s_JerseyWrinkle);
		if (m_CurrentKit.jerseyCollar >= 0 && m_CurrentKit.jerseyCollar < Kit.s_JerseyModel3D.Length && Kit.s_JerseyModel3D[m_CurrentKit.jerseyCollar] != null)
		{
			Kit.s_JerseyModel3D[m_CurrentKit.jerseyCollar].TextureBitmap = bitmap;
			viewer3DKit.SetMesh(0, Kit.s_JerseyModel3D[m_CurrentKit.jerseyCollar]);
		}
		bitmap2 = GraphicUtil.EmbossBitmap(bitmap2, Kit.s_ShortsWrinkle);
		if (Kit.s_ShortsModel3D != null)
		{
			Kit.s_ShortsModel3D.TextureBitmap = bitmap2;
			viewer3DKit.SetMesh(1, Kit.s_ShortsModel3D);
		}
		if (Kit.s_SocksModel3D != null)
		{
			Kit.s_SocksModel3D.TextureBitmap = bitmap2;
			viewer3DKit.SetMesh(2, Kit.s_SocksModel3D);
		}
		viewer3DKit.Render();
		Cursor.Current = Cursors.Default;
	}

	private void buttonShow3DModel_Click(object sender, EventArgs e)
	{
		Show3DKit();
	}

	private void numericCollar_ValueChanged(object sender, EventArgs e)
	{
		m_CurrentKit.jerseyCollar = (int)numericCollar.Value;
		labelCollarImage.ImageIndex = m_CurrentKit.jerseyCollar;
	}

	private void pictureNameColor_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureNameColor.BackColor;
		colorDialog.ShowDialog();
		pictureNameColor.BackColor = colorDialog.Color;
		m_CurrentKit.JerseyNameColor = colorDialog.Color;
	}

	private void pictureJerseyNumberColor_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(c_ColorPalette, m_CurrentKit.jerseyNumberColor);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentKit.jerseyNumberColor = colorSelector.SelectedIndex;
			pictureJerseyNumberColor.BackColor = colorSelector.SelectedColor;
			if (!m_UpdatingLock)
			{
				m_UpdatingLock = true;
				multiViewer2DJerseyNumbers.Bitmaps = NumberFont.GetNumberFont(m_CurrentKit.jerseyNumberFont, m_CurrentKit.jerseyNumberColor);
				m_UpdatingLock = false;
			}
		}
		colorSelector.Dispose();
	}

	private void pictureShortsNumberColor_Click(object sender, EventArgs e)
	{
		ColorSelector colorSelector = new ColorSelector(c_ColorPalette, m_CurrentKit.shortsNumberColor);
		if (colorSelector.ShowDialog() == DialogResult.OK)
		{
			m_CurrentKit.shortsNumberColor = colorSelector.SelectedIndex;
			pictureShortsNumberColor.BackColor = colorSelector.SelectedColor;
			if (!m_UpdatingLock)
			{
				m_UpdatingLock = true;
				multiViewer2DShortsNumbers.Bitmaps = NumberFont.GetNumberFont(m_CurrentKit.shortsNumberFont, m_CurrentKit.shortsNumberColor);
				m_UpdatingLock = false;
			}
		}
		colorSelector.Dispose();
	}

	private void pictureTeamPrimColor_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureTeamPrimColor.BackColor;
		colorDialog.ShowDialog();
		pictureTeamPrimColor.BackColor = colorDialog.Color;
		m_CurrentKit.TeamColor1 = colorDialog.Color;
	}

	private void pictureTeamSecColor_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureTeamSecColor.BackColor;
		colorDialog.ShowDialog();
		pictureTeamSecColor.BackColor = colorDialog.Color;
		m_CurrentKit.TeamColor2 = colorDialog.Color;
	}

	private void pictureTeamTerColor_Click(object sender, EventArgs e)
	{
		colorDialog.Color = pictureTeamTerColor.BackColor;
		colorDialog.ShowDialog();
		pictureTeamTerColor.BackColor = colorDialog.Color;
		m_CurrentKit.TeamColor3 = colorDialog.Color;
	}

	private void numericJerseyNumberFont_ValueChanged(object sender, EventArgs e)
	{
		if (!m_UpdatingLock)
		{
			m_UpdatingLock = true;
			m_CurrentKit.jerseyNumberFont = (int)numericJerseyNumberFont.Value;
			multiViewer2DJerseyNumbers.Bitmaps = NumberFont.GetNumberFont(m_CurrentKit.jerseyNumberFont, m_CurrentKit.jerseyNumberColor);
			m_UpdatingLock = false;
		}
	}

	private void numericShortsNumberFont_ValueChanged(object sender, EventArgs e)
	{
		if (!m_UpdatingLock)
		{
			m_UpdatingLock = true;
			m_CurrentKit.shortsNumberFont = (int)numericShortsNumberFont.Value;
			multiViewer2DShortsNumbers.Bitmaps = NumberFont.GetNumberFont(m_CurrentKit.shortsNumberFont, m_CurrentKit.shortsNumberColor);
			m_UpdatingLock = false;
		}
	}

	private void buttonCamera_Click(object sender, EventArgs e)
	{
		Bitmap bitmap = viewer3DMinikit.Photo();
		Rectangle srcRect = new Rectangle(0, 0, 256, 256);
		Rectangle destRect = new Rectangle(0, 0, 256, 256);
		Bitmap srcBitmap = GraphicUtil.MakeAutoTransparent(bitmap);
		Bitmap bitmap2 = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
		GraphicUtil.RemapRectangle(srcBitmap, srcRect, bitmap2, destRect);
		m_CurrentKit.SetMiniKit(bitmap2);
		viewer2DMinikit.CurrentBitmap = bitmap2;
	}

	private void radioPosition_Click(object sender, EventArgs e)
	{
		if (((RadioButton)sender).Checked)
		{
			LoadPositions();
		}
	}

	private void numericPositions_ValueChanged(object sender, EventArgs e)
	{
		ChangePositions();
	}

	private void checkLink_CheckedChanged(object sender, EventArgs e)
	{
		LoadPositions();
	}

	private void buttonPositions_Click(object sender, EventArgs e)
	{
		ToolStripButton toolStripButton = (ToolStripButton)sender;
		if (toolStripButton.Checked)
		{
			if (buttonBackName.Checked && buttonBackName != toolStripButton)
			{
				buttonBackName.Checked = false;
			}
			if (buttonBackNumber.Checked && buttonBackNumber != toolStripButton)
			{
				buttonBackNumber.Checked = false;
			}
			if (buttonFrontNumber.Checked && buttonFrontNumber != toolStripButton)
			{
				buttonFrontNumber.Checked = false;
			}
			if (buttonJerseyBadge.Checked && buttonJerseyBadge != toolStripButton)
			{
				buttonJerseyBadge.Checked = false;
			}
			if (buttonNameCurvature.Checked && buttonNameCurvature != toolStripButton)
			{
				buttonNameCurvature.Checked = false;
			}
			if (buttonShortsBadge.Checked && buttonShortsBadge != toolStripButton)
			{
				buttonShortsBadge.Checked = false;
			}
			if (buttonShortsNumber.Checked && buttonShortsNumber != toolStripButton)
			{
				buttonShortsNumber.Checked = false;
			}
			LoadPositions();
		}
	}

	private void buttonSavePositions_Click(object sender, EventArgs e)
	{
	}

	private void buttonRefresh3D_Click(object sender, EventArgs e)
	{
		Show3DKit();
		Show3DMinikit();
	}

	private void buttonShowNumbers3D_Click(object sender, EventArgs e)
	{
		Show3DKit();
	}

	private void ShowFont()
	{
		int num = (int)numericNameFont.Value;
		string fileName = FifaEnvironment.ExportFolder + "\\" + NameFont.NameFontFileName(num);
		Font font = null;
		if (NameFont.Export(num, FifaEnvironment.ExportFolder))
		{
			FontFamily fontFamily = LoadFontFamily(fileName, out m_FontCollection);
			string text;
			if (fontFamily.IsStyleAvailable(FontStyle.Regular))
			{
				font = new Font(fontFamily, 15f, FontStyle.Regular);
			}
			else if (fontFamily.IsStyleAvailable(FontStyle.Bold))
			{
				font = new Font(fontFamily, 15f, FontStyle.Bold);
			}
			else if (fontFamily.IsStyleAvailable(FontStyle.Italic))
			{
				font = new Font(fontFamily, 15f, FontStyle.Italic);
			}
			else if (fontFamily.IsStyleAvailable(FontStyle.Strikeout))
			{
				font = new Font(fontFamily, 15f, FontStyle.Strikeout);
			}
			else if (fontFamily.IsStyleAvailable(FontStyle.Underline))
			{
				font = new Font(fontFamily, 15f, FontStyle.Underline);
			}
			else
			{
				text = "Font is present but cannot be shown.";
			}
			text = fontFamily.Name + "\r\n";
			text += "abcdefghijklmnopqrstuvwxyz\r\nABCDEFGHIJKLMNOPQRSTUVWXYZ";
			if (font != null)
			{
				m_FontGraphics.Clear(Color.White);
				m_FontGraphics.DrawString(text, font, m_FontBrush, 0f, 0f);
			}
			else
			{
				m_FontGraphics.Clear(Color.White);
			}
		}
		else
		{
			m_FontGraphics.Clear(Color.White);
		}
	}

	private void buttonPreviewNameFont_Click(object sender, EventArgs e)
	{
		int num = (int)numericNameFont.Value;
		string text = FifaEnvironment.ExportFolder + "\\" + NameFont.NameFontFileName(num);
		bool flag = true;
		if (!FifaUtil.IsFileLocked(text))
		{
			flag = NameFont.Export(num, FifaEnvironment.ExportFolder);
		}
		if (flag && text != null)
		{
			processFontView.StartInfo.WorkingDirectory = FifaEnvironment.LaunchDir;
			processFontView.StartInfo.FileName = "fontview";
			processFontView.StartInfo.CreateNoWindow = true;
			processFontView.StartInfo.UseShellExecute = false;
			processFontView.StartInfo.Arguments = text;
			processFontView.StartInfo.RedirectStandardOutput = false;
			processFontView.Start();
			processFontView.WaitForExit();
		}
	}

	private void buttonImportNameFont_Click(object sender, EventArgs e)
	{
		int style = (int)numericNameFont.Value;
		string text = FifaEnvironment.BrowseAndCheckTtf(ref m_FontnameCurrentFolder);
		if (text != null)
		{
			NameFont.Import(style, text);
			ShowFont();
		}
	}

	private void buttonExportNameFont_Click(object sender, EventArgs e)
	{
		NameFont.Export((int)numericNameFont.Value, FifaEnvironment.ExportFolder);
	}

	private void buttonDeleteNameFont_Click(object sender, EventArgs e)
	{
		NameFont.Delete((int)numericNameFont.Value);
		ShowFont();
	}

	private void checkFrontNumber_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_UpdatingLock)
		{
			m_CurrentKit.jerseyFrontNumber = checkFrontNumber.Checked;
		}
	}

	private void checkShortsNumber_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_UpdatingLock)
		{
			m_CurrentKit.shortsNumber = checkShortsNumber.Checked;
		}
	}

	private void checkHasBackname_CheckedChanged(object sender, EventArgs e)
	{
		if (!m_UpdatingLock)
		{
			m_CurrentKit.jerseyBackName = checkHasBackname.Checked;
		}
	}

	private void buttonCopyPositions_Click(object sender, EventArgs e)
	{
		for (int i = 0; i < 32; i++)
		{
			m_CopyPosition[i] = m_CurrentKit.Positions[i];
		}
	}

	private void buttonPastePositions_Click(object sender, EventArgs e)
	{
		for (int i = 0; i < 32; i++)
		{
			m_CurrentKit.Positions[i] = m_CopyPosition[i];
		}
		LoadPositions();
		if (!multiViewer2DKit.buttonSave.Enabled)
		{
			multiViewer2DKit.buttonSave.Enabled = true;
		}
	}

	private void labelTeam_DoubleClick(object sender, EventArgs e)
	{
		if (m_CurrentKit.Team != null)
		{
			MainForm.CM.JumpTo(m_CurrentKit.Team);
		}
	}

	private void buttonShowFont_Click(object sender, EventArgs e)
	{
		fontDialog.ShowDialog();
		_ = 1;
	}

	private void numericNameFont_ValueChanged(object sender, EventArgs e)
	{
		if (!m_UpdatingLock)
		{
			ShowFont();
		}
	}

	private void buttonExportAllKits_Click(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.SelectedPath = FifaEnvironment.ExportFolder;
		folderBrowserDialog.Description = "Select the export folder";
		folderBrowserDialog.ShowNewFolderButton = true;
		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			folderBrowserDialog.Dispose();
			return;
		}
		string selectedPath = folderBrowserDialog.SelectedPath;
		folderBrowserDialog.Dispose();
		int num = 1;
		for (int i = 0; i < pickUpControl.combo.Items.Count; i++)
		{
			Kit kit = (Kit)pickUpControl.combo.Items[i];
			if (kit.kittype != 0)
			{
				continue;
			}
			if (num > 0)
			{
				string text = kit.KitTextureFileName();
				text = FifaEnvironment.RootDir + "\\" + text;
				if (!File.Exists(text))
				{
					continue;
				}
				Bitmap[] kitTextures = kit.GetKitTextures();
				if (kitTextures != null)
				{
					string filename = selectedPath + "\\j_" + kit.teamid.ToString("0000") + "_" + kit.kittype + ".png";
					string filename2 = selectedPath + "\\s_" + kit.teamid.ToString("0000") + "_" + kit.kittype + ".png";
					kitTextures[1].Save(filename);
					kitTextures[3].Save(filename2);
					kit.DisposeKitTextures();
					for (int j = 0; j < kitTextures.Length; j++)
					{
						kitTextures[j].Dispose();
					}
				}
			}
			num++;
			if (num % 100 == 0)
			{
				GC.Collect();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			foreach (Bitmap[] textures in m_Fc26TextureCache.Values)
			{
				foreach (Bitmap texture in textures)
				{
					if (texture != null) texture.Dispose();
				}
			}
			m_Fc26TextureCache.Clear();
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.KitForm));
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.splitContainer3 = new System.Windows.Forms.SplitContainer();
		this.multiViewer2DKit = new FifaControls.MultiViewer2D();
		this.groupPositions = new System.Windows.Forms.GroupBox();
		this.toolStrip3D = new System.Windows.Forms.ToolStrip();
		this.buttonJerseyBadge = new System.Windows.Forms.ToolStripButton();
		this.buttonFrontNumber = new System.Windows.Forms.ToolStripButton();
		this.buttonBackName = new System.Windows.Forms.ToolStripButton();
		this.buttonNameCurvature = new System.Windows.Forms.ToolStripButton();
		this.buttonShortsNumber = new System.Windows.Forms.ToolStripButton();
		this.buttonShortsBadge = new System.Windows.Forms.ToolStripButton();
		this.buttonBackNumber = new System.Windows.Forms.ToolStripButton();
		this.buttonCopyPositions = new System.Windows.Forms.ToolStripButton();
		this.buttonPastePositions = new System.Windows.Forms.ToolStripButton();
		this.numericBottom = new System.Windows.Forms.NumericUpDown();
		this.numericTop = new System.Windows.Forms.NumericUpDown();
		this.numericRight = new System.Windows.Forms.NumericUpDown();
		this.numericLeft = new System.Windows.Forms.NumericUpDown();
		this.checkLink = new System.Windows.Forms.CheckBox();
		this.label2 = new System.Windows.Forms.Label();
		this.buttonExportAllKits = new System.Windows.Forms.Button();
		this.numericTeamId = new System.Windows.Forms.NumericUpDown();
		this.kitBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.labelTeamId = new System.Windows.Forms.Label();
		this.labelKitType = new System.Windows.Forms.Label();
		this.labelTeam = new System.Windows.Forms.Label();
		this.comboTeam = new System.Windows.Forms.ComboBox();
		this.teamListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.comboKitType = new System.Windows.Forms.ComboBox();
		this.splitContainer2 = new System.Windows.Forms.SplitContainer();
		this.splitContainer4 = new System.Windows.Forms.SplitContainer();
		this.group3D = new System.Windows.Forms.GroupBox();
		this.toolNear3D = new System.Windows.Forms.ToolStrip();
		this.buttonShow3DModel = new System.Windows.Forms.ToolStripButton();
		this.buttonRefresh3D = new System.Windows.Forms.ToolStripButton();
		this.buttonShowNumbers3D = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonCamera = new System.Windows.Forms.ToolStripButton();
		this.multiViewer2DShortsNumbers = new FifaControls.MultiViewer2D();
		this.pictureShortsNumberColor = new System.Windows.Forms.PictureBox();
		this.numericShortsNumberFont = new System.Windows.Forms.NumericUpDown();
		this.multiViewer2DJerseyNumbers = new FifaControls.MultiViewer2D();
		this.checkShortsNumber = new System.Windows.Forms.CheckBox();
		this.checkFrontNumber = new System.Windows.Forms.CheckBox();
		this.pictureJerseyNumberColor = new System.Windows.Forms.PictureBox();
		this.numericJerseyNumberFont = new System.Windows.Forms.NumericUpDown();
		this.splitContainer5 = new System.Windows.Forms.SplitContainer();
		this.viewer2DMinikit = new FifaControls.Viewer2D();
		this.checkIsFitting = new System.Windows.Forms.CheckBox();
		this.pictureTeamPrimColor = new System.Windows.Forms.PictureBox();
		this.checkHasAdvertising = new System.Windows.Forms.CheckBox();
		this.pictureTeamSecColor = new System.Windows.Forms.PictureBox();
		this.pictureTeamTerColor = new System.Windows.Forms.PictureBox();
		this.flowPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.groupCollar = new System.Windows.Forms.GroupBox();
		this.buttonMinikitPicture = new System.Windows.Forms.Button();
		this.labelCollarImage = new System.Windows.Forms.Label();
		this.imageListCollar = new System.Windows.Forms.ImageList(this.components);
		this.labelCollar = new System.Windows.Forms.Label();
		this.numericCollar = new System.Windows.Forms.NumericUpDown();
		this.groupName = new System.Windows.Forms.GroupBox();
		this.toolStripNameFont = new System.Windows.Forms.ToolStrip();
		this.buttonPreviewNameFont = new System.Windows.Forms.ToolStripButton();
		this.buttonImportNameFont = new System.Windows.Forms.ToolStripButton();
		this.buttonDeleteNameFont = new System.Windows.Forms.ToolStripButton();
		this.buttonExportNameFont = new System.Windows.Forms.ToolStripButton();
		this.label3 = new System.Windows.Forms.Label();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.checkHasBackname = new System.Windows.Forms.CheckBox();
		this.numericNameFont = new System.Windows.Forms.NumericUpDown();
		this.labelNameFont = new System.Windows.Forms.Label();
		this.pictureNameColor = new System.Windows.Forms.PictureBox();
		this.label1 = new System.Windows.Forms.Label();
		this.comboNameLayout = new System.Windows.Forms.ComboBox();
		this.pictureFont = new System.Windows.Forms.PictureBox();
		this.colorDialog = new System.Windows.Forms.ColorDialog();
		this.pickUpControl = new FifaControls.PickUpControl();
		this.fontDialog = new System.Windows.Forms.FontDialog();
		this.processFontView = new System.Diagnostics.Process();
		this.fontDialog1 = new System.Windows.Forms.FontDialog();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer3).BeginInit();
		this.splitContainer3.Panel1.SuspendLayout();
		this.splitContainer3.Panel2.SuspendLayout();
		this.splitContainer3.SuspendLayout();
		this.groupPositions.SuspendLayout();
		this.toolStrip3D.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericBottom).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTop).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericRight).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericLeft).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericTeamId).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.kitBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).BeginInit();
		this.splitContainer2.Panel1.SuspendLayout();
		this.splitContainer2.Panel2.SuspendLayout();
		this.splitContainer2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer4).BeginInit();
		this.splitContainer4.Panel1.SuspendLayout();
		this.splitContainer4.Panel2.SuspendLayout();
		this.splitContainer4.SuspendLayout();
		this.group3D.SuspendLayout();
		this.toolNear3D.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureShortsNumberColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericShortsNumberFont).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureJerseyNumberColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericJerseyNumberFont).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.splitContainer5).BeginInit();
		this.splitContainer5.Panel1.SuspendLayout();
		this.splitContainer5.Panel2.SuspendLayout();
		this.splitContainer5.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamPrimColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamSecColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamTerColor).BeginInit();
		this.flowPanel.SuspendLayout();
		this.groupCollar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCollar).BeginInit();
		this.groupName.SuspendLayout();
		this.toolStripNameFont.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNameFont).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureNameColor).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFont).BeginInit();
		base.SuspendLayout();
		this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
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
		this.splitContainer3.Panel1.Controls.Add(this.multiViewer2DKit);
		this.splitContainer3.Panel1.Controls.Add(this.groupPositions);
		this.splitContainer3.Panel2.Controls.Add(this.buttonExportAllKits);
		this.splitContainer3.Panel2.Controls.Add(this.numericTeamId);
		this.splitContainer3.Panel2.Controls.Add(this.labelTeamId);
		this.splitContainer3.Panel2.Controls.Add(this.labelKitType);
		this.splitContainer3.Panel2.Controls.Add(this.labelTeam);
		this.splitContainer3.Panel2.Controls.Add(this.comboTeam);
		this.splitContainer3.Panel2.Controls.Add(this.comboKitType);
		this.splitContainer3.Size = new System.Drawing.Size(516, 807);
		this.splitContainer3.SplitterDistance = 682;
		this.splitContainer3.TabIndex = 0;
		this.multiViewer2DKit.AutoTransparency = false;
		this.multiViewer2DKit.Bitmaps = null;
		this.multiViewer2DKit.CheckBitmapSize = true;
		this.multiViewer2DKit.Dock = System.Windows.Forms.DockStyle.Fill;
		this.multiViewer2DKit.FixedSize = false;
		this.multiViewer2DKit.FullSizeButton = true;
		this.multiViewer2DKit.LabelText = "Image n.";
		this.multiViewer2DKit.Location = new System.Drawing.Point(0, 0);
		this.multiViewer2DKit.Name = "multiViewer2DKit";
		this.multiViewer2DKit.ShowButton = true;
		this.multiViewer2DKit.ShowDeleteButton = false;
		this.multiViewer2DKit.Size = new System.Drawing.Size(512, 558);
		this.multiViewer2DKit.TabIndex = 0;
		this.groupPositions.Controls.Add(this.toolStrip3D);
		this.groupPositions.Controls.Add(this.numericBottom);
		this.groupPositions.Controls.Add(this.numericTop);
		this.groupPositions.Controls.Add(this.numericRight);
		this.groupPositions.Controls.Add(this.numericLeft);
		this.groupPositions.Controls.Add(this.checkLink);
		this.groupPositions.Controls.Add(this.label2);
		this.groupPositions.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.groupPositions.Location = new System.Drawing.Point(0, 558);
		this.groupPositions.Name = "groupPositions";
		this.groupPositions.Size = new System.Drawing.Size(512, 120);
		this.groupPositions.TabIndex = 3;
		this.groupPositions.TabStop = false;
		this.groupPositions.Text = "Positions";
		this.toolStrip3D.AutoSize = false;
		this.toolStrip3D.CanOverflow = false;
		this.toolStrip3D.Dock = System.Windows.Forms.DockStyle.Left;
		this.toolStrip3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.buttonJerseyBadge, this.buttonFrontNumber, this.buttonBackName, this.buttonNameCurvature, this.buttonShortsNumber, this.buttonShortsBadge, this.buttonBackNumber, this.buttonCopyPositions, this.buttonPastePositions });
		this.toolStrip3D.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.toolStrip3D.Location = new System.Drawing.Point(3, 16);
		this.toolStrip3D.Name = "toolStrip3D";
		this.toolStrip3D.Size = new System.Drawing.Size(201, 101);
		this.toolStrip3D.TabIndex = 190;
		this.toolStrip3D.Text = "toolStrip1";
		this.buttonJerseyBadge.AutoToolTip = false;
		this.buttonJerseyBadge.Checked = true;
		this.buttonJerseyBadge.CheckOnClick = true;
		this.buttonJerseyBadge.CheckState = System.Windows.Forms.CheckState.Checked;
		this.buttonJerseyBadge.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.buttonJerseyBadge.Image = (System.Drawing.Image)resources.GetObject("buttonJerseyBadge.Image");
		this.buttonJerseyBadge.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonJerseyBadge.Name = "buttonJerseyBadge";
		this.buttonJerseyBadge.Size = new System.Drawing.Size(90, 19);
		this.buttonJerseyBadge.Text = "  Jersey Badge  ";
		this.buttonJerseyBadge.Click += new System.EventHandler(buttonPositions_Click);
		this.buttonFrontNumber.AutoToolTip = false;
		this.buttonFrontNumber.CheckOnClick = true;
		this.buttonFrontNumber.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.buttonFrontNumber.Image = (System.Drawing.Image)resources.GetObject("buttonFrontNumber.Image");
		this.buttonFrontNumber.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonFrontNumber.Name = "buttonFrontNumber";
		this.buttonFrontNumber.Size = new System.Drawing.Size(92, 19);
		this.buttonFrontNumber.Text = " Front Number ";
		this.buttonFrontNumber.Click += new System.EventHandler(buttonPositions_Click);
		this.buttonBackName.AutoToolTip = false;
		this.buttonBackName.CheckOnClick = true;
		this.buttonBackName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.buttonBackName.Image = (System.Drawing.Image)resources.GetObject("buttonBackName.Image");
		this.buttonBackName.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonBackName.Name = "buttonBackName";
		this.buttonBackName.Size = new System.Drawing.Size(89, 19);
		this.buttonBackName.Text = "   Back Name   ";
		this.buttonBackName.Click += new System.EventHandler(buttonPositions_Click);
		this.buttonNameCurvature.AutoToolTip = false;
		this.buttonNameCurvature.CheckOnClick = true;
		this.buttonNameCurvature.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.buttonNameCurvature.Image = (System.Drawing.Image)resources.GetObject("buttonNameCurvature.Image");
		this.buttonNameCurvature.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonNameCurvature.Name = "buttonNameCurvature";
		this.buttonNameCurvature.Size = new System.Drawing.Size(98, 19);
		this.buttonNameCurvature.Text = "Name Curvature";
		this.buttonNameCurvature.Click += new System.EventHandler(buttonPositions_Click);
		this.buttonShortsNumber.AutoToolTip = false;
		this.buttonShortsNumber.CheckOnClick = true;
		this.buttonShortsNumber.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.buttonShortsNumber.Image = (System.Drawing.Image)resources.GetObject("buttonShortsNumber.Image");
		this.buttonShortsNumber.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShortsNumber.Name = "buttonShortsNumber";
		this.buttonShortsNumber.Size = new System.Drawing.Size(91, 19);
		this.buttonShortsNumber.Text = "Shorts Number";
		this.buttonShortsNumber.Click += new System.EventHandler(buttonPositions_Click);
		this.buttonShortsBadge.AutoToolTip = false;
		this.buttonShortsBadge.CheckOnClick = true;
		this.buttonShortsBadge.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.buttonShortsBadge.Image = (System.Drawing.Image)resources.GetObject("buttonShortsBadge.Image");
		this.buttonShortsBadge.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShortsBadge.Name = "buttonShortsBadge";
		this.buttonShortsBadge.Size = new System.Drawing.Size(86, 19);
		this.buttonShortsBadge.Text = " Shorts Badge ";
		this.buttonShortsBadge.Click += new System.EventHandler(buttonPositions_Click);
		this.buttonBackNumber.AutoToolTip = false;
		this.buttonBackNumber.CheckOnClick = true;
		this.buttonBackNumber.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.buttonBackNumber.Image = (System.Drawing.Image)resources.GetObject("buttonBackNumber.Image");
		this.buttonBackNumber.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonBackNumber.Name = "buttonBackNumber";
		this.buttonBackNumber.Size = new System.Drawing.Size(89, 19);
		this.buttonBackNumber.Text = " Back Number ";
		this.buttonBackNumber.Click += new System.EventHandler(buttonPositions_Click);
		this.buttonCopyPositions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCopyPositions.Image = (System.Drawing.Image)resources.GetObject("buttonCopyPositions.Image");
		this.buttonCopyPositions.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCopyPositions.Margin = new System.Windows.Forms.Padding(15, 1, 0, 2);
		this.buttonCopyPositions.Name = "buttonCopyPositions";
		this.buttonCopyPositions.Size = new System.Drawing.Size(23, 20);
		this.buttonCopyPositions.Text = "Copy All Positions";
		this.buttonCopyPositions.Click += new System.EventHandler(buttonCopyPositions_Click);
		this.buttonPastePositions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPastePositions.Image = (System.Drawing.Image)resources.GetObject("buttonPastePositions.Image");
		this.buttonPastePositions.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPastePositions.Name = "buttonPastePositions";
		this.buttonPastePositions.Size = new System.Drawing.Size(23, 20);
		this.buttonPastePositions.Text = "Paste All Positions";
		this.buttonPastePositions.Click += new System.EventHandler(buttonPastePositions_Click);
		this.numericBottom.DecimalPlaces = 3;
		this.numericBottom.Enabled = false;
		this.numericBottom.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.numericBottom.Location = new System.Drawing.Point(302, 74);
		this.numericBottom.Maximum = new decimal(new int[4] { 10, 0, 0, 65536 });
		this.numericBottom.Name = "numericBottom";
		this.numericBottom.Size = new System.Drawing.Size(64, 20);
		this.numericBottom.TabIndex = 178;
		this.numericBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericBottom.ValueChanged += new System.EventHandler(numericPositions_ValueChanged);
		this.numericTop.DecimalPlaces = 3;
		this.numericTop.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.numericTop.Location = new System.Drawing.Point(300, 17);
		this.numericTop.Maximum = new decimal(new int[4] { 10, 0, 0, 65536 });
		this.numericTop.Name = "numericTop";
		this.numericTop.Size = new System.Drawing.Size(64, 20);
		this.numericTop.TabIndex = 174;
		this.numericTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericTop.ValueChanged += new System.EventHandler(numericPositions_ValueChanged);
		this.numericRight.DecimalPlaces = 3;
		this.numericRight.Enabled = false;
		this.numericRight.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.numericRight.Location = new System.Drawing.Point(359, 44);
		this.numericRight.Maximum = new decimal(new int[4] { 10, 0, 0, 65536 });
		this.numericRight.Name = "numericRight";
		this.numericRight.Size = new System.Drawing.Size(64, 20);
		this.numericRight.TabIndex = 172;
		this.numericRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericRight.ValueChanged += new System.EventHandler(numericPositions_ValueChanged);
		this.numericLeft.DecimalPlaces = 3;
		this.numericLeft.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.numericLeft.Location = new System.Drawing.Point(237, 46);
		this.numericLeft.Maximum = new decimal(new int[4] { 10, 0, 0, 65536 });
		this.numericLeft.Name = "numericLeft";
		this.numericLeft.Size = new System.Drawing.Size(64, 20);
		this.numericLeft.TabIndex = 173;
		this.numericLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericLeft.ValueChanged += new System.EventHandler(numericPositions_ValueChanged);
		this.checkLink.AutoSize = true;
		this.checkLink.Checked = true;
		this.checkLink.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkLink.Location = new System.Drawing.Point(307, 47);
		this.checkLink.Name = "checkLink";
		this.checkLink.Size = new System.Drawing.Size(46, 17);
		this.checkLink.TabIndex = 189;
		this.checkLink.Text = "Link";
		this.checkLink.UseVisualStyleBackColor = true;
		this.checkLink.CheckedChanged += new System.EventHandler(checkLink_CheckedChanged);
		this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.label2.Location = new System.Drawing.Point(263, 27);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(129, 56);
		this.label2.TabIndex = 188;
		this.buttonExportAllKits.Location = new System.Drawing.Point(226, 35);
		this.buttonExportAllKits.Name = "buttonExportAllKits";
		this.buttonExportAllKits.Size = new System.Drawing.Size(89, 23);
		this.buttonExportAllKits.TabIndex = 12;
		this.buttonExportAllKits.Text = "Export all Kits";
		this.buttonExportAllKits.UseVisualStyleBackColor = true;
		this.buttonExportAllKits.Click += new System.EventHandler(buttonExportAllKits_Click);
		this.numericTeamId.BackColor = System.Drawing.SystemColors.Window;
		this.numericTeamId.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.kitBindingSource, "teamid", true));
		this.numericTeamId.Enabled = false;
		this.numericTeamId.Location = new System.Drawing.Point(106, 35);
		this.numericTeamId.Maximum = new decimal(new int[4] { 300000, 0, 0, 0 });
		this.numericTeamId.Name = "numericTeamId";
		this.numericTeamId.ReadOnly = true;
		this.numericTeamId.Size = new System.Drawing.Size(98, 20);
		this.numericTeamId.TabIndex = 11;
		this.numericTeamId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.kitBindingSource.DataSource = typeof(FifaLibrary.Kit);
		this.labelTeamId.AutoSize = true;
		this.labelTeamId.Location = new System.Drawing.Point(10, 42);
		this.labelTeamId.Name = "labelTeamId";
		this.labelTeamId.Size = new System.Drawing.Size(46, 13);
		this.labelTeamId.TabIndex = 4;
		this.labelTeamId.Text = "Team Id";
		this.labelKitType.AutoSize = true;
		this.labelKitType.Location = new System.Drawing.Point(227, 11);
		this.labelKitType.Name = "labelKitType";
		this.labelKitType.Size = new System.Drawing.Size(19, 13);
		this.labelKitType.TabIndex = 3;
		this.labelKitType.Text = "Kit";
		this.labelTeam.AutoSize = true;
		this.labelTeam.Cursor = System.Windows.Forms.Cursors.Hand;
		this.labelTeam.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTeam.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.labelTeam.Location = new System.Drawing.Point(10, 11);
		this.labelTeam.Name = "labelTeam";
		this.labelTeam.Size = new System.Drawing.Size(34, 13);
		this.labelTeam.TabIndex = 2;
		this.labelTeam.Text = "Team";
		this.labelTeam.DoubleClick += new System.EventHandler(labelTeam_DoubleClick);
		this.comboTeam.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.kitBindingSource, "Team", true));
		this.comboTeam.DataSource = this.teamListBindingSource;
		this.comboTeam.Enabled = false;
		this.comboTeam.FormattingEnabled = true;
		this.comboTeam.Location = new System.Drawing.Point(50, 8);
		this.comboTeam.Name = "comboTeam";
		this.comboTeam.Size = new System.Drawing.Size(154, 21);
		this.comboTeam.TabIndex = 0;
		this.teamListBindingSource.DataSource = typeof(FifaLibrary.TeamList);
		this.comboKitType.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.kitBindingSource, "kittype", true));
		this.comboKitType.Enabled = false;
		this.comboKitType.FormattingEnabled = true;
		this.comboKitType.Items.AddRange(new object[11]
		{
			"Home", "Away", "Goalkeeper", "3rd", "4th", "5th", "6th", "7th", "8th", "9th",
			"10th"
		});
		this.comboKitType.Location = new System.Drawing.Point(271, 8);
		this.comboKitType.Name = "comboKitType";
		this.comboKitType.Size = new System.Drawing.Size(114, 21);
		this.comboKitType.TabIndex = 1;
		this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer2.Location = new System.Drawing.Point(0, 0);
		this.splitContainer2.Name = "splitContainer2";
		this.splitContainer2.Panel1.Controls.Add(this.splitContainer4);
		this.splitContainer2.Panel2.Controls.Add(this.splitContainer5);
		this.splitContainer2.Size = new System.Drawing.Size(837, 807);
		this.splitContainer2.SplitterDistance = 437;
		this.splitContainer2.TabIndex = 0;
		this.splitContainer4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer4.Location = new System.Drawing.Point(0, 0);
		this.splitContainer4.Name = "splitContainer4";
		this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer4.Panel1.Controls.Add(this.group3D);
		this.splitContainer4.Panel2.AutoScroll = true;
		this.splitContainer4.Panel2.Controls.Add(this.multiViewer2DShortsNumbers);
		this.splitContainer4.Panel2.Controls.Add(this.pictureShortsNumberColor);
		this.splitContainer4.Panel2.Controls.Add(this.numericShortsNumberFont);
		this.splitContainer4.Panel2.Controls.Add(this.multiViewer2DJerseyNumbers);
		this.splitContainer4.Panel2.Controls.Add(this.checkShortsNumber);
		this.splitContainer4.Panel2.Controls.Add(this.checkFrontNumber);
		this.splitContainer4.Panel2.Controls.Add(this.pictureJerseyNumberColor);
		this.splitContainer4.Panel2.Controls.Add(this.numericJerseyNumberFont);
		this.splitContainer4.Size = new System.Drawing.Size(437, 807);
		this.splitContainer4.SplitterDistance = 576;
		this.splitContainer4.TabIndex = 0;
		this.group3D.Controls.Add(this.toolNear3D);
		this.group3D.Dock = System.Windows.Forms.DockStyle.Fill;
		this.group3D.Location = new System.Drawing.Point(0, 0);
		this.group3D.Name = "group3D";
		this.group3D.Size = new System.Drawing.Size(433, 572);
		this.group3D.TabIndex = 2;
		this.group3D.TabStop = false;
		this.group3D.Text = "3D Model";
		this.toolNear3D.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolNear3D.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolNear3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.buttonShow3DModel, this.buttonRefresh3D, this.buttonShowNumbers3D, this.toolStripSeparator1, this.buttonCamera });
		this.toolNear3D.Location = new System.Drawing.Point(3, 544);
		this.toolNear3D.Name = "toolNear3D";
		this.toolNear3D.Size = new System.Drawing.Size(427, 25);
		this.toolNear3D.TabIndex = 2;
		this.buttonShow3DModel.CheckOnClick = true;
		this.buttonShow3DModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow3DModel.Image = (System.Drawing.Image)resources.GetObject("buttonShow3DModel.Image");
		this.buttonShow3DModel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow3DModel.Name = "buttonShow3DModel";
		this.buttonShow3DModel.Size = new System.Drawing.Size(23, 22);
		this.buttonShow3DModel.Text = "Show / Hide";
		this.buttonShow3DModel.Click += new System.EventHandler(buttonShow3DModel_Click);
		this.buttonRefresh3D.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRefresh3D.Image = (System.Drawing.Image)resources.GetObject("buttonRefresh3D.Image");
		this.buttonRefresh3D.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRefresh3D.Name = "buttonRefresh3D";
		this.buttonRefresh3D.Size = new System.Drawing.Size(23, 22);
		this.buttonRefresh3D.Text = "Refresh 3D View";
		this.buttonRefresh3D.Click += new System.EventHandler(buttonRefresh3D_Click);
		this.buttonShowNumbers3D.CheckOnClick = true;
		this.buttonShowNumbers3D.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShowNumbers3D.Image = (System.Drawing.Image)resources.GetObject("buttonShowNumbers3D.Image");
		this.buttonShowNumbers3D.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShowNumbers3D.Name = "buttonShowNumbers3D";
		this.buttonShowNumbers3D.Size = new System.Drawing.Size(23, 22);
		this.buttonShowNumbers3D.Text = "Show Numbers";
		this.buttonShowNumbers3D.Click += new System.EventHandler(buttonShowNumbers3D_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCamera.Image = (System.Drawing.Image)resources.GetObject("buttonCamera.Image");
		this.buttonCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCamera.Name = "buttonCamera";
		this.buttonCamera.Size = new System.Drawing.Size(23, 22);
		this.buttonCamera.Text = "Take a picture for minikit";
		this.buttonCamera.Click += new System.EventHandler(buttonCamera_Click);
		this.multiViewer2DShortsNumbers.AutoTransparency = true;
		this.multiViewer2DShortsNumbers.Bitmaps = null;
		this.multiViewer2DShortsNumbers.CheckBitmapSize = true;
		this.multiViewer2DShortsNumbers.FixedSize = false;
		this.multiViewer2DShortsNumbers.FullSizeButton = false;
		this.multiViewer2DShortsNumbers.LabelText = "Shorts";
		this.multiViewer2DShortsNumbers.Location = new System.Drawing.Point(220, 27);
		this.multiViewer2DShortsNumbers.Name = "multiViewer2DShortsNumbers";
		this.multiViewer2DShortsNumbers.ShowButton = false;
		this.multiViewer2DShortsNumbers.ShowDeleteButton = false;
		this.multiViewer2DShortsNumbers.Size = new System.Drawing.Size(132, 178);
		this.multiViewer2DShortsNumbers.TabIndex = 1;
		this.pictureShortsNumberColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureShortsNumberColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureShortsNumberColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureShortsNumberColor.Location = new System.Drawing.Point(357, 108);
		this.pictureShortsNumberColor.Name = "pictureShortsNumberColor";
		this.pictureShortsNumberColor.Size = new System.Drawing.Size(24, 24);
		this.pictureShortsNumberColor.TabIndex = 147;
		this.pictureShortsNumberColor.TabStop = false;
		this.pictureShortsNumberColor.Click += new System.EventHandler(pictureShortsNumberColor_Click);
		this.numericShortsNumberFont.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.kitBindingSource, "shortsNumberFont", true));
		this.numericShortsNumberFont.Location = new System.Drawing.Point(357, 82);
		this.numericShortsNumberFont.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
		this.numericShortsNumberFont.Name = "numericShortsNumberFont";
		this.numericShortsNumberFont.Size = new System.Drawing.Size(55, 20);
		this.numericShortsNumberFont.TabIndex = 12;
		this.numericShortsNumberFont.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericShortsNumberFont.ValueChanged += new System.EventHandler(numericShortsNumberFont_ValueChanged);
		this.multiViewer2DJerseyNumbers.AutoTransparency = true;
		this.multiViewer2DJerseyNumbers.Bitmaps = null;
		this.multiViewer2DJerseyNumbers.CheckBitmapSize = true;
		this.multiViewer2DJerseyNumbers.FixedSize = false;
		this.multiViewer2DJerseyNumbers.FullSizeButton = false;
		this.multiViewer2DJerseyNumbers.LabelText = "Jersey";
		this.multiViewer2DJerseyNumbers.Location = new System.Drawing.Point(13, 27);
		this.multiViewer2DJerseyNumbers.Name = "multiViewer2DJerseyNumbers";
		this.multiViewer2DJerseyNumbers.ShowButton = false;
		this.multiViewer2DJerseyNumbers.ShowDeleteButton = false;
		this.multiViewer2DJerseyNumbers.Size = new System.Drawing.Size(132, 178);
		this.multiViewer2DJerseyNumbers.TabIndex = 0;
		this.checkShortsNumber.AutoSize = true;
		this.checkShortsNumber.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.kitBindingSource, "shortsNumber", true));
		this.checkShortsNumber.Location = new System.Drawing.Point(220, 10);
		this.checkShortsNumber.Name = "checkShortsNumber";
		this.checkShortsNumber.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.checkShortsNumber.Size = new System.Drawing.Size(117, 17);
		this.checkShortsNumber.TabIndex = 4;
		this.checkShortsNumber.Text = "Shorts Number       ";
		this.checkShortsNumber.UseVisualStyleBackColor = true;
		this.checkShortsNumber.CheckedChanged += new System.EventHandler(checkShortsNumber_CheckedChanged);
		this.checkFrontNumber.AutoSize = true;
		this.checkFrontNumber.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.kitBindingSource, "jerseyFrontNumber", true));
		this.checkFrontNumber.Location = new System.Drawing.Point(13, 10);
		this.checkFrontNumber.Name = "checkFrontNumber";
		this.checkFrontNumber.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.checkFrontNumber.Size = new System.Drawing.Size(105, 17);
		this.checkFrontNumber.TabIndex = 3;
		this.checkFrontNumber.Text = "Front Number     ";
		this.checkFrontNumber.UseVisualStyleBackColor = true;
		this.checkFrontNumber.CheckedChanged += new System.EventHandler(checkFrontNumber_CheckedChanged);
		this.pictureJerseyNumberColor.BackColor = System.Drawing.SystemColors.Control;
		this.pictureJerseyNumberColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureJerseyNumberColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureJerseyNumberColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureJerseyNumberColor.Location = new System.Drawing.Point(151, 108);
		this.pictureJerseyNumberColor.Name = "pictureJerseyNumberColor";
		this.pictureJerseyNumberColor.Size = new System.Drawing.Size(24, 24);
		this.pictureJerseyNumberColor.TabIndex = 146;
		this.pictureJerseyNumberColor.TabStop = false;
		this.pictureJerseyNumberColor.Click += new System.EventHandler(pictureJerseyNumberColor_Click);
		this.numericJerseyNumberFont.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.kitBindingSource, "jerseyNumberFont", true));
		this.numericJerseyNumberFont.Location = new System.Drawing.Point(151, 82);
		this.numericJerseyNumberFont.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
		this.numericJerseyNumberFont.Name = "numericJerseyNumberFont";
		this.numericJerseyNumberFont.Size = new System.Drawing.Size(55, 20);
		this.numericJerseyNumberFont.TabIndex = 10;
		this.numericJerseyNumberFont.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericJerseyNumberFont.ValueChanged += new System.EventHandler(numericJerseyNumberFont_ValueChanged);
		this.splitContainer5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer5.Location = new System.Drawing.Point(0, 0);
		this.splitContainer5.Name = "splitContainer5";
		this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer5.Panel1.Controls.Add(this.viewer2DMinikit);
		this.splitContainer5.Panel1.Controls.Add(this.checkIsFitting);
		this.splitContainer5.Panel1.Controls.Add(this.pictureTeamPrimColor);
		this.splitContainer5.Panel1.Controls.Add(this.checkHasAdvertising);
		this.splitContainer5.Panel1.Controls.Add(this.pictureTeamSecColor);
		this.splitContainer5.Panel1.Controls.Add(this.pictureTeamTerColor);
		this.splitContainer5.Panel2.Controls.Add(this.flowPanel);
		this.splitContainer5.Size = new System.Drawing.Size(396, 807);
		this.splitContainer5.SplitterDistance = 297;
		this.splitContainer5.TabIndex = 0;
		this.viewer2DMinikit.AutoTransparency = true;
		this.viewer2DMinikit.BackColor = System.Drawing.Color.Transparent;
		this.viewer2DMinikit.ButtonStripVisible = true;
		this.viewer2DMinikit.CurrentBitmap = null;
		this.viewer2DMinikit.ExtendedFormat = false;
		this.viewer2DMinikit.FullSizeButton = false;
		this.viewer2DMinikit.ImageLayout = System.Windows.Forms.ImageLayout.None;
		this.viewer2DMinikit.ImageSize = new System.Drawing.Size(256, 256);
		this.viewer2DMinikit.ImageSizeMultiplier = FifaControls.Viewer2D.SizeMultiplier.None;
		this.viewer2DMinikit.Location = new System.Drawing.Point(0, 0);
		this.viewer2DMinikit.Name = "viewer2DMinikit";
		this.viewer2DMinikit.RemoveButton = false;
		this.viewer2DMinikit.ShowButton = false;
		this.viewer2DMinikit.ShowButtonChecked = true;
		this.viewer2DMinikit.Size = new System.Drawing.Size(256, 281);
		this.viewer2DMinikit.TabIndex = 0;
		this.checkIsFitting.AutoSize = true;
		this.checkIsFitting.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.kitBindingSource, "jerseyfit", true));
		this.checkIsFitting.Location = new System.Drawing.Point(270, 168);
		this.checkIsFitting.Name = "checkIsFitting";
		this.checkIsFitting.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.checkIsFitting.Size = new System.Drawing.Size(65, 17);
		this.checkIsFitting.TabIndex = 152;
		this.checkIsFitting.Text = "Is Fitting";
		this.checkIsFitting.UseVisualStyleBackColor = true;
		this.pictureTeamPrimColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureTeamPrimColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureTeamPrimColor.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.kitBindingSource, "TeamColor1", true));
		this.pictureTeamPrimColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureTeamPrimColor.Location = new System.Drawing.Point(275, 115);
		this.pictureTeamPrimColor.Name = "pictureTeamPrimColor";
		this.pictureTeamPrimColor.Size = new System.Drawing.Size(24, 24);
		this.pictureTeamPrimColor.TabIndex = 148;
		this.pictureTeamPrimColor.TabStop = false;
		this.pictureTeamPrimColor.Click += new System.EventHandler(pictureTeamPrimColor_Click);
		this.checkHasAdvertising.AutoSize = true;
		this.checkHasAdvertising.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.kitBindingSource, "hasadvertisingkit", true));
		this.checkHasAdvertising.Location = new System.Drawing.Point(271, 145);
		this.checkHasAdvertising.Name = "checkHasAdvertising";
		this.checkHasAdvertising.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.checkHasAdvertising.Size = new System.Drawing.Size(118, 17);
		this.checkHasAdvertising.TabIndex = 151;
		this.checkHasAdvertising.Text = "Has Advertising      ";
		this.checkHasAdvertising.UseVisualStyleBackColor = true;
		this.pictureTeamSecColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureTeamSecColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureTeamSecColor.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.kitBindingSource, "TeamColor2", true));
		this.pictureTeamSecColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureTeamSecColor.Location = new System.Drawing.Point(312, 115);
		this.pictureTeamSecColor.Name = "pictureTeamSecColor";
		this.pictureTeamSecColor.Size = new System.Drawing.Size(24, 24);
		this.pictureTeamSecColor.TabIndex = 149;
		this.pictureTeamSecColor.TabStop = false;
		this.pictureTeamSecColor.Click += new System.EventHandler(pictureTeamSecColor_Click);
		this.pictureTeamTerColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureTeamTerColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureTeamTerColor.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.kitBindingSource, "TeamColor3", true));
		this.pictureTeamTerColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureTeamTerColor.Location = new System.Drawing.Point(349, 115);
		this.pictureTeamTerColor.Name = "pictureTeamTerColor";
		this.pictureTeamTerColor.Size = new System.Drawing.Size(24, 24);
		this.pictureTeamTerColor.TabIndex = 150;
		this.pictureTeamTerColor.TabStop = false;
		this.pictureTeamTerColor.Click += new System.EventHandler(pictureTeamTerColor_Click);
		this.flowPanel.AutoScroll = true;
		this.flowPanel.Controls.Add(this.groupCollar);
		this.flowPanel.Controls.Add(this.groupName);
		this.flowPanel.Controls.Add(this.pictureFont);
		this.flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowPanel.Location = new System.Drawing.Point(0, 0);
		this.flowPanel.Name = "flowPanel";
		this.flowPanel.Size = new System.Drawing.Size(392, 502);
		this.flowPanel.TabIndex = 0;
		this.groupCollar.Controls.Add(this.buttonMinikitPicture);
		this.groupCollar.Controls.Add(this.labelCollarImage);
		this.groupCollar.Controls.Add(this.labelCollar);
		this.groupCollar.Controls.Add(this.numericCollar);
		this.groupCollar.Location = new System.Drawing.Point(3, 3);
		this.groupCollar.Name = "groupCollar";
		this.groupCollar.Size = new System.Drawing.Size(379, 295);
		this.groupCollar.TabIndex = 1;
		this.groupCollar.TabStop = false;
		this.groupCollar.Text = "Jersey";
		this.buttonMinikitPicture.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonMinikitPicture.BackgroundImage");
		this.buttonMinikitPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonMinikitPicture.Location = new System.Drawing.Point(270, 240);
		this.buttonMinikitPicture.Name = "buttonMinikitPicture";
		this.buttonMinikitPicture.Size = new System.Drawing.Size(100, 24);
		this.buttonMinikitPicture.TabIndex = 155;
		this.buttonMinikitPicture.UseVisualStyleBackColor = true;
		this.labelCollarImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelCollarImage.ImageList = this.imageListCollar;
		this.labelCollarImage.Location = new System.Drawing.Point(267, 67);
		this.labelCollarImage.Name = "labelCollarImage";
		this.labelCollarImage.Size = new System.Drawing.Size(106, 120);
		this.labelCollarImage.TabIndex = 154;
		this.imageListCollar.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListCollar.ImageStream");
		this.imageListCollar.TransparentColor = System.Drawing.Color.Transparent;
		this.imageListCollar.Images.SetKeyName(0, "col_00.png");
		this.imageListCollar.Images.SetKeyName(1, "col_01.png");
		this.imageListCollar.Images.SetKeyName(2, "col_02.png");
		this.imageListCollar.Images.SetKeyName(3, "col_03.png");
		this.imageListCollar.Images.SetKeyName(4, "col_04.png");
		this.imageListCollar.Images.SetKeyName(5, "col_05.png");
		this.imageListCollar.Images.SetKeyName(6, "col_06.png");
		this.imageListCollar.Images.SetKeyName(7, "col_07.png");
		this.imageListCollar.Images.SetKeyName(8, "col_08.png");
		this.imageListCollar.Images.SetKeyName(9, "col_09.png");
		this.imageListCollar.Images.SetKeyName(10, "col_10.png");
		this.imageListCollar.Images.SetKeyName(11, "col_11.png");
		this.imageListCollar.Images.SetKeyName(12, "col_12.png");
		this.imageListCollar.Images.SetKeyName(13, "col_13.png");
		this.imageListCollar.Images.SetKeyName(14, "col_14.png");
		this.imageListCollar.Images.SetKeyName(15, "col_15.png");
		this.imageListCollar.Images.SetKeyName(16, "col_16.png");
		this.imageListCollar.Images.SetKeyName(17, "col_17.png");
		this.imageListCollar.Images.SetKeyName(18, "col_18.png");
		this.imageListCollar.Images.SetKeyName(19, "col_19.png");
		this.imageListCollar.Images.SetKeyName(20, "col_20.png");
		this.imageListCollar.Images.SetKeyName(21, "col_21.png");
		this.labelCollar.AutoSize = true;
		this.labelCollar.Location = new System.Drawing.Point(296, 28);
		this.labelCollar.Name = "labelCollar";
		this.labelCollar.Size = new System.Drawing.Size(33, 13);
		this.labelCollar.TabIndex = 1;
		this.labelCollar.Text = "Collar";
		this.numericCollar.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.kitBindingSource, "jerseyCollar", true));
		this.numericCollar.Location = new System.Drawing.Point(267, 44);
		this.numericCollar.Maximum = new decimal(new int[4] { 21, 0, 0, 0 });
		this.numericCollar.Name = "numericCollar";
		this.numericCollar.Size = new System.Drawing.Size(106, 20);
		this.numericCollar.TabIndex = 0;
		this.numericCollar.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericCollar.ValueChanged += new System.EventHandler(numericCollar_ValueChanged);
		this.groupName.Controls.Add(this.toolStripNameFont);
		this.groupName.Controls.Add(this.label3);
		this.groupName.Controls.Add(this.comboBox1);
		this.groupName.Controls.Add(this.checkHasBackname);
		this.groupName.Controls.Add(this.numericNameFont);
		this.groupName.Controls.Add(this.labelNameFont);
		this.groupName.Controls.Add(this.pictureNameColor);
		this.groupName.Controls.Add(this.label1);
		this.groupName.Controls.Add(this.comboNameLayout);
		this.groupName.Location = new System.Drawing.Point(3, 304);
		this.groupName.Name = "groupName";
		this.groupName.Size = new System.Drawing.Size(379, 99);
		this.groupName.TabIndex = 2;
		this.groupName.TabStop = false;
		this.groupName.Text = "Name";
		this.toolStripNameFont.AutoSize = false;
		this.toolStripNameFont.Dock = System.Windows.Forms.DockStyle.None;
		this.toolStripNameFont.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStripNameFont.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.buttonPreviewNameFont, this.buttonImportNameFont, this.buttonDeleteNameFont, this.buttonExportNameFont });
		this.toolStripNameFont.Location = new System.Drawing.Point(208, 42);
		this.toolStripNameFont.Name = "toolStripNameFont";
		this.toolStripNameFont.Size = new System.Drawing.Size(160, 25);
		this.toolStripNameFont.TabIndex = 148;
		this.buttonPreviewNameFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonPreviewNameFont.Image = (System.Drawing.Image)resources.GetObject("buttonPreviewNameFont.Image");
		this.buttonPreviewNameFont.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonPreviewNameFont.Name = "buttonPreviewNameFont";
		this.buttonPreviewNameFont.Size = new System.Drawing.Size(23, 22);
		this.buttonPreviewNameFont.Text = "Preview Font";
		this.buttonPreviewNameFont.Visible = false;
		this.buttonPreviewNameFont.Click += new System.EventHandler(buttonPreviewNameFont_Click);
		this.buttonImportNameFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportNameFont.Image = (System.Drawing.Image)resources.GetObject("buttonImportNameFont.Image");
		this.buttonImportNameFont.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportNameFont.Name = "buttonImportNameFont";
		this.buttonImportNameFont.Size = new System.Drawing.Size(23, 22);
		this.buttonImportNameFont.Text = "Import Font";
		this.buttonImportNameFont.Click += new System.EventHandler(buttonImportNameFont_Click);
		this.buttonDeleteNameFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonDeleteNameFont.Image = (System.Drawing.Image)resources.GetObject("buttonDeleteNameFont.Image");
		this.buttonDeleteNameFont.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonDeleteNameFont.Name = "buttonDeleteNameFont";
		this.buttonDeleteNameFont.Size = new System.Drawing.Size(23, 22);
		this.buttonDeleteNameFont.Text = "Remove Font";
		this.buttonDeleteNameFont.Click += new System.EventHandler(buttonDeleteNameFont_Click);
		this.buttonExportNameFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportNameFont.Image = (System.Drawing.Image)resources.GetObject("buttonExportNameFont.Image");
		this.buttonExportNameFont.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportNameFont.Name = "buttonExportNameFont";
		this.buttonExportNameFont.Size = new System.Drawing.Size(23, 22);
		this.buttonExportNameFont.Text = "Export";
		this.buttonExportNameFont.Visible = false;
		this.buttonExportNameFont.Click += new System.EventHandler(buttonExportNameFont_Click);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(6, 73);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(55, 13);
		this.label3.TabIndex = 146;
		this.label3.Text = "Font Case";
		this.comboBox1.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.kitBindingSource, "jerseyNameFontCase", true));
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Items.AddRange(new object[3] { "UPPER CASE", "lower case", "Mixed Case" });
		this.comboBox1.Location = new System.Drawing.Point(82, 68);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(108, 21);
		this.comboBox1.TabIndex = 147;
		this.checkHasBackname.AutoSize = true;
		this.checkHasBackname.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.kitBindingSource, "jerseyBackName", true));
		this.checkHasBackname.Location = new System.Drawing.Point(9, 19);
		this.checkHasBackname.Name = "checkHasBackname";
		this.checkHasBackname.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.checkHasBackname.Size = new System.Drawing.Size(112, 17);
		this.checkHasBackname.TabIndex = 2;
		this.checkHasBackname.Text = "Back Name          ";
		this.checkHasBackname.UseVisualStyleBackColor = true;
		this.checkHasBackname.CheckedChanged += new System.EventHandler(checkHasBackname_CheckedChanged);
		this.numericNameFont.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.kitBindingSource, "jerseyNameFont", true));
		this.numericNameFont.Location = new System.Drawing.Point(82, 42);
		this.numericNameFont.Maximum = new decimal(new int[4] { 21, 0, 0, 0 });
		this.numericNameFont.Name = "numericNameFont";
		this.numericNameFont.Size = new System.Drawing.Size(108, 20);
		this.numericNameFont.TabIndex = 5;
		this.numericNameFont.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericNameFont.ValueChanged += new System.EventHandler(numericNameFont_ValueChanged);
		this.labelNameFont.AutoSize = true;
		this.labelNameFont.Location = new System.Drawing.Point(6, 48);
		this.labelNameFont.Name = "labelNameFont";
		this.labelNameFont.Size = new System.Drawing.Size(55, 13);
		this.labelNameFont.TabIndex = 6;
		this.labelNameFont.Text = "Font Type";
		this.pictureNameColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureNameColor.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureNameColor.DataBindings.Add(new System.Windows.Forms.Binding("BackColor", this.kitBindingSource, "JerseyNameColor", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
		this.pictureNameColor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pictureNameColor.Location = new System.Drawing.Point(127, 12);
		this.pictureNameColor.Name = "pictureNameColor";
		this.pictureNameColor.Size = new System.Drawing.Size(24, 24);
		this.pictureNameColor.TabIndex = 145;
		this.pictureNameColor.TabStop = false;
		this.pictureNameColor.Click += new System.EventHandler(pictureNameColor_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(205, 73);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(39, 13);
		this.label1.TabIndex = 7;
		this.label1.Text = "Layout";
		this.comboNameLayout.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this.kitBindingSource, "jerseyNameLayout", true));
		this.comboNameLayout.FormattingEnabled = true;
		this.comboNameLayout.Items.AddRange(new object[2] { "Straight", "Curved" });
		this.comboNameLayout.Location = new System.Drawing.Point(262, 70);
		this.comboNameLayout.Name = "comboNameLayout";
		this.comboNameLayout.Size = new System.Drawing.Size(108, 21);
		this.comboNameLayout.TabIndex = 8;
		this.pictureFont.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureFont.Location = new System.Drawing.Point(3, 409);
		this.pictureFont.Name = "pictureFont";
		this.pictureFont.Size = new System.Drawing.Size(379, 84);
		this.pictureFont.TabIndex = 4;
		this.pictureFont.TabStop = false;
		this.pickUpControl.BackColor = System.Drawing.SystemColors.Control;
		this.pickUpControl.CloneButtonEnabled = true;
		this.pickUpControl.CreateButtonEnabled = true;
		this.pickUpControl.CurrentIndex = 0;
		this.pickUpControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.pickUpControl.FilterByList = new string[5] { "All", "by Team", "by League", "by Country", "Dummy Kits" };
		this.pickUpControl.FilterEnabled = true;
		this.pickUpControl.FilterValues = null;
		this.pickUpControl.Location = new System.Drawing.Point(0, 0);
		this.pickUpControl.MainSelectionEnabled = true;
		this.pickUpControl.Name = "pickUpControl";
		this.pickUpControl.ObjectList = null;
		this.pickUpControl.RefreshButtonEnabled = true;
		this.pickUpControl.RemoveButtonEnabled = true;
		this.pickUpControl.SearchEnabled = true;
		this.pickUpControl.Size = new System.Drawing.Size(1357, 25);
		this.pickUpControl.TabIndex = 1;
		this.pickUpControl.WizardButtonEnabled = false;
		this.pickUpControl.YoungPlayersEnabled = false;
		this.fontDialog.Color = System.Drawing.SystemColors.ControlText;
		this.processFontView.StartInfo.Domain = "";
		this.processFontView.StartInfo.LoadUserProfile = false;
		this.processFontView.StartInfo.Password = null;
		this.processFontView.StartInfo.StandardErrorEncoding = null;
		this.processFontView.StartInfo.StandardOutputEncoding = null;
		this.processFontView.StartInfo.UserName = "";
		this.processFontView.SynchronizingObject = this;
		this.fontDialog1.Color = System.Drawing.SystemColors.ControlText;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1357, 832);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.pickUpControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "KitForm";
		this.Text = "KitForm";
		base.Load += new System.EventHandler(KitForm_Load);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.splitContainer3.Panel1.ResumeLayout(false);
		this.splitContainer3.Panel2.ResumeLayout(false);
		this.splitContainer3.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer3).EndInit();
		this.splitContainer3.ResumeLayout(false);
		this.groupPositions.ResumeLayout(false);
		this.groupPositions.PerformLayout();
		this.toolStrip3D.ResumeLayout(false);
		this.toolStrip3D.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericBottom).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTop).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericRight).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericLeft).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericTeamId).EndInit();
		((System.ComponentModel.ISupportInitialize)this.kitBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.teamListBindingSource).EndInit();
		this.splitContainer2.Panel1.ResumeLayout(false);
		this.splitContainer2.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).EndInit();
		this.splitContainer2.ResumeLayout(false);
		this.splitContainer4.Panel1.ResumeLayout(false);
		this.splitContainer4.Panel2.ResumeLayout(false);
		this.splitContainer4.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer4).EndInit();
		this.splitContainer4.ResumeLayout(false);
		this.group3D.ResumeLayout(false);
		this.group3D.PerformLayout();
		this.toolNear3D.ResumeLayout(false);
		this.toolNear3D.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureShortsNumberColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericShortsNumberFont).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureJerseyNumberColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericJerseyNumberFont).EndInit();
		this.splitContainer5.Panel1.ResumeLayout(false);
		this.splitContainer5.Panel1.PerformLayout();
		this.splitContainer5.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer5).EndInit();
		this.splitContainer5.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.pictureTeamPrimColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamSecColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureTeamTerColor).EndInit();
		this.flowPanel.ResumeLayout(false);
		this.groupCollar.ResumeLayout(false);
		this.groupCollar.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericCollar).EndInit();
		this.groupName.ResumeLayout(false);
		this.groupName.PerformLayout();
		this.toolStripNameFont.ResumeLayout(false);
		this.toolStripNameFont.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericNameFont).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureNameColor).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureFont).EndInit();
		base.ResumeLayout(false);
	}
}
