using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class MultiViewer2D : UserControl
{
	public delegate bool Rx3SaveHandler(object sender, Bitmap[] bitmaps);

	public delegate bool Rx3ImportHandler(object sender, string rx3FileName);

	public delegate bool Rx3ExportHandler(object sender, string exportDir);

	public delegate bool Rx3DeleteHandler(object sender);

	public delegate bool BitmapUpdateHandler(object sender);

	private static FullSizeViewer s_FullSizeViewer = new FullSizeViewer();

	private bool m_AutoTransparency;

	private bool m_CheckBitmapSize;

	private bool m_FixedSize;

	public Rx3SaveHandler Rx3SaveDelegate;

	public Rx3ExportHandler Rx3ExportDelegate;

	public Rx3ImportHandler Rx3ImportDelegate;

	public Rx3DeleteHandler Rx3DeleteDelegate;

	public BitmapUpdateHandler BitmapUpdateDelegate;

	private int m_CurrentIndex;

	private Bitmap[] m_Bitmaps;

	private bool m_NeedToSave;

	private string m_InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

	private IContainer components;

	private Label label;

	private ToolStrip toolStrip;

	public ToolStripButton buttonImportImage;

	public ToolStripButton buttonExportImage;

	public ToolStripButton buttonImportRx3;

	public ToolStripButton buttonExportRx3;

	private ToolStripTextBox textSize;

	public PictureBox pictureBox;

	public ToolStripButton buttonSave;

	private FolderBrowserDialog folderBrowserDialog;

	private OpenFileDialog openFileDialogBmp;

	private OpenFileDialog openFileDialogRx3;

	private SaveFileDialog saveFileDialogBmp;

	private Label labelOf;

	private ToolStripButton buttonRemoveRx3;

	public ToolStripButton buttonFullSize;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator2;

	public ToolStripButton buttonShow;

	public NumericUpDown numeric;

	[Category("User")]
	[Description("Auto Transparency.")]
	public bool AutoTransparency
	{
		get
		{
			return m_AutoTransparency;
		}
		set
		{
			m_AutoTransparency = value;
		}
	}

	[Category("User")]
	[Description("Check for bitmap size.")]
	public bool CheckBitmapSize
	{
		get
		{
			return m_CheckBitmapSize;
		}
		set
		{
			m_CheckBitmapSize = value;
		}
	}

	[Category("User")]
	[Description("Label text.")]
	public string LabelText
	{
		get
		{
			return label.Text;
		}
		set
		{
			label.Text = value;
		}
	}

	[Category("User")]
	[Description("Show Delete Button.")]
	public bool ShowDeleteButton
	{
		get
		{
			return buttonRemoveRx3.Visible;
		}
		set
		{
			buttonRemoveRx3.Visible = value;
		}
	}

	[Category("User")]
	[Description("Full Size Button Visible.")]
	public bool FullSizeButton
	{
		get
		{
			return buttonFullSize.Visible;
		}
		set
		{
			buttonFullSize.Visible = value;
		}
	}

	[Category("User")]
	[Description("Show Button Visible.")]
	public bool ShowButton
	{
		get
		{
			return buttonShow.Visible;
		}
		set
		{
			buttonShow.Visible = value;
		}
	}

	[Category("User")]
	[Description("Fixed size.")]
	public bool FixedSize
	{
		get
		{
			return m_FixedSize;
		}
		set
		{
			m_FixedSize = value;
		}
	}

	[Category("User")]
	[Description("Bitmaps.")]
	public Bitmap[] Bitmaps
	{
		get
		{
			return m_Bitmaps;
		}
		set
		{
			m_Bitmaps = value;
			if (m_Bitmaps != null)
			{
				numeric.Maximum = m_Bitmaps.Length;
				labelOf.Text = "/" + m_Bitmaps.Length;
				if (m_CurrentIndex >= m_Bitmaps.Length)
				{
					m_CurrentIndex = 0;
				}
				else
				{
					numeric_ValueChanged(null, null);
				}
			}
			else
			{
				m_CurrentIndex = 0;
				numeric_ValueChanged(null, null);
			}
			m_NeedToSave = false;
		}
	}

	private string InitialDirectory
	{
		get
		{
			return m_InitialDirectory;
		}
		set
		{
			folderBrowserDialog.SelectedPath = value;
			openFileDialogBmp.InitialDirectory = value;
			openFileDialogRx3.InitialDirectory = value;
			saveFileDialogBmp.InitialDirectory = value;
			m_InitialDirectory = value;
		}
	}

	public void Redraw()
	{
		m_CurrentIndex = (int)numeric.Value - 1;
		if (m_Bitmaps != null && m_Bitmaps[m_CurrentIndex] != null && buttonShow.Checked)
		{
			pictureBox.BackgroundImage = m_Bitmaps[m_CurrentIndex];
			textSize.Text = m_Bitmaps[m_CurrentIndex].Width + " x " + m_Bitmaps[m_CurrentIndex].Height;
			AdjustImageLayout();
		}
		else
		{
			pictureBox.BackgroundImage = null;
		}
	}

	public Bitmap GetCurrentBitmap()
	{
		if (m_Bitmaps != null && m_CurrentIndex < m_Bitmaps.Length)
		{
			return m_Bitmaps[m_CurrentIndex];
		}
		return null;
	}

	public MultiViewer2D()
	{
		InitializeComponent();
	}

	private void numeric_ValueChanged(object sender, EventArgs e)
	{
		Redraw();
	}

	private void buttonImportImage_Click(object sender, EventArgs e)
	{
		ImportImage();
		if (BitmapUpdateDelegate != null)
		{
			BitmapUpdateDelegate(sender);
		}
	}

	private void ImportImage()
	{
		Bitmap bitmap = BrowseAndCheckBitmap();
		Refresh();
		if (bitmap != null)
		{
			pictureBox.BackgroundImage = bitmap;
			m_NeedToSave = true;
			buttonSave.Enabled = true;
			if (m_Bitmaps == null)
			{
				m_Bitmaps = new Bitmap[(int)numeric.Maximum];
			}
			m_Bitmaps[m_CurrentIndex] = bitmap;
		}
	}

	private Bitmap BrowseAndCheckBitmap()
	{
		openFileDialogBmp.CheckFileExists = true;
		openFileDialogBmp.Multiselect = false;
		openFileDialogBmp.InitialDirectory = m_InitialDirectory;
		openFileDialogBmp.RestoreDirectory = true;
		openFileDialogBmp.Filter = "Image Files (*.bmp;*.png)|*.bmp;*.png";
		openFileDialogBmp.FilterIndex = 1;
		openFileDialogBmp.Title = "Open Image File";
		if (openFileDialogBmp.ShowDialog() != DialogResult.OK)
		{
			return null;
		}
		string fileName = openFileDialogBmp.FileName;
		InitialDirectory = Path.GetDirectoryName(fileName);
		Bitmap bitmap;
		using (Bitmap original = new Bitmap(fileName))
		{
			bitmap = new Bitmap(original);
		}
		if (bitmap == null)
		{
			return null;
		}
		Cursor.Current = Cursors.WaitCursor;
		FindForm().Refresh();
		if (m_CheckBitmapSize && m_Bitmaps != null && m_Bitmaps[m_CurrentIndex] != null && m_CurrentIndex >= 0 && m_CurrentIndex < m_Bitmaps.Length)
		{
			int num = m_Bitmaps[m_CurrentIndex].Width;
			int num2 = m_Bitmaps[m_CurrentIndex].Height;
			if ((bitmap.Width != num || bitmap.Height != num2) && (bitmap.Width != num || bitmap.Height != num2))
			{
				if (num == 0 || num2 == 0 || m_FixedSize)
				{
					Cursor.Current = Cursors.Default;
					FifaEnvironment.UserMessages.ShowMessage(5015);
					return null;
				}
				bitmap = GraphicUtil.ResizeBitmap(bitmap, num, num2, InterpolationMode.HighQualityBilinear);
			}
		}
		if (m_AutoTransparency && Path.GetExtension(fileName).ToLower() == ".bmp")
		{
			Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
			Color pixel = bitmap.GetPixel(0, 0);
			Color color = Color.FromArgb(0, 0, 0, 0);
			for (int i = 0; i < bitmap.Width; i++)
			{
				for (int j = 0; j < bitmap.Height; j++)
				{
					Color pixel2 = bitmap.GetPixel(i, j);
					if (pixel2 == pixel)
					{
						bitmap2.SetPixel(i, j, color);
					}
					else
					{
						bitmap2.SetPixel(i, j, pixel2);
					}
				}
			}
			Cursor.Current = Cursors.Default;
			return bitmap2;
		}
		Cursor.Current = Cursors.Default;
		return bitmap;
	}

	private void buttonExportImage_Click(object sender, EventArgs e)
	{
		ExportImage();
	}

	private void ExportImage()
	{
		AskAndSaveBitmap((Bitmap)pictureBox.BackgroundImage);
	}

	private bool AskAndSaveBitmap(Bitmap bitmap)
	{
		if (bitmap == null)
		{
			return false;
		}
		saveFileDialogBmp.InitialDirectory = m_InitialDirectory;
		if (saveFileDialogBmp.ShowDialog() != DialogResult.OK)
		{
			return false;
		}
		string extension = Path.GetExtension(saveFileDialogBmp.FileName);
		InitialDirectory = Path.GetDirectoryName(saveFileDialogBmp.FileName);
		ImageFormat format;
		if (extension.ToLower() == ".bmp")
		{
			format = ImageFormat.Bmp;
			Color pixel = bitmap.GetPixel(0, 0);
			for (int num = bitmap.Width - 1; num >= 0; num--)
			{
				for (int num2 = bitmap.Height - 1; num2 >= 0; num2--)
				{
					if (bitmap.GetPixel(num, num2).A < 192)
					{
						bitmap.SetPixel(num, num2, pixel);
					}
				}
			}
		}
		else
		{
			if (!(extension.ToLower() == ".png"))
			{
				return false;
			}
			format = ImageFormat.Png;
		}
		bitmap.Save(saveFileDialogBmp.FileName, format);
		return true;
	}

	private void buttonImportFsh_Click(object sender, EventArgs e)
	{
		ImportRx3();
	}

	private void ImportRx3()
	{
		string text = BrowseRx3();
		if (text != null)
		{
			if (Rx3ImportDelegate != null)
			{
				Rx3ImportDelegate(this, text);
			}
			m_NeedToSave = false;
			buttonSave.Enabled = false;
		}
	}

	private string BrowseRx3()
	{
		openFileDialogRx3.InitialDirectory = m_InitialDirectory;
		if (openFileDialogRx3.ShowDialog() != DialogResult.OK)
		{
			return null;
		}
		string fileName = openFileDialogRx3.FileName;
		InitialDirectory = Path.GetDirectoryName(fileName);
		return fileName;
	}

	private string BrowseExportingFolder()
	{
		folderBrowserDialog.Description = "Select the export folder";
		folderBrowserDialog.ShowNewFolderButton = true;
		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			folderBrowserDialog.Dispose();
			return null;
		}
		return folderBrowserDialog.SelectedPath;
	}

	private void buttonExportRx3_Click(object sender, EventArgs e)
	{
		ExportRx3File();
	}

	private void ExportRx3File()
	{
		string text = BrowseExportingFolder();
		if (text != null && Rx3ExportDelegate != null)
		{
			Rx3ExportDelegate(this, text);
		}
	}

	private void buttonSave_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		if (Rx3SaveDelegate != null)
		{
			Rx3SaveDelegate(sender, m_Bitmaps);
		}
		m_NeedToSave = false;
		buttonSave.Enabled = false;
		Cursor.Current = Cursors.Default;
	}

	private void MultiViewer2D_Resize(object sender, EventArgs e)
	{
		AdjustImageLayout();
	}

	private void AdjustImageLayout()
	{
		int num = 128;
		int num2 = 128;
		if (m_Bitmaps != null && m_Bitmaps[m_CurrentIndex] != null)
		{
			num = m_Bitmaps[m_CurrentIndex].Width;
			num2 = m_Bitmaps[m_CurrentIndex].Height;
		}
		if (pictureBox.Width < num || pictureBox.Height < num2)
		{
			pictureBox.BackgroundImageLayout = ImageLayout.Zoom;
		}
		else
		{
			pictureBox.BackgroundImageLayout = ImageLayout.Center;
		}
	}

	private void buttonFullSize_Click(object sender, EventArgs e)
	{
		if (m_Bitmaps[m_CurrentIndex] != null)
		{
			s_FullSizeViewer.SetImage(m_Bitmaps[m_CurrentIndex]);
			s_FullSizeViewer.ShowDialog();
		}
	}

	private void buttonRemoveRx3_Click(object sender, EventArgs e)
	{
		m_Bitmaps = null;
		pictureBox.BackgroundImage = null;
		m_CurrentIndex = 0;
		numeric.Value = 1m;
		if (Rx3DeleteDelegate != null)
		{
			Rx3DeleteDelegate(this);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.MultiViewer2D));
		this.label = new System.Windows.Forms.Label();
		this.numeric = new System.Windows.Forms.NumericUpDown();
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.buttonShow = new System.Windows.Forms.ToolStripButton();
		this.buttonSave = new System.Windows.Forms.ToolStripButton();
		this.buttonImportImage = new System.Windows.Forms.ToolStripButton();
		this.buttonExportImage = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonImportRx3 = new System.Windows.Forms.ToolStripButton();
		this.buttonExportRx3 = new System.Windows.Forms.ToolStripButton();
		this.buttonRemoveRx3 = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonFullSize = new System.Windows.Forms.ToolStripButton();
		this.textSize = new System.Windows.Forms.ToolStripTextBox();
		this.pictureBox = new System.Windows.Forms.PictureBox();
		this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
		this.openFileDialogBmp = new System.Windows.Forms.OpenFileDialog();
		this.openFileDialogRx3 = new System.Windows.Forms.OpenFileDialog();
		this.saveFileDialogBmp = new System.Windows.Forms.SaveFileDialog();
		this.labelOf = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.numeric).BeginInit();
		this.toolStrip.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
		base.SuspendLayout();
		this.label.Dock = System.Windows.Forms.DockStyle.Top;
		this.label.Location = new System.Drawing.Point(0, 0);
		this.label.Name = "label";
		this.label.Size = new System.Drawing.Size(257, 20);
		this.label.TabIndex = 0;
		this.label.Text = "Image n.";
		this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.numeric.Location = new System.Drawing.Point(49, 0);
		this.numeric.Maximum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numeric.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numeric.Name = "numeric";
		this.numeric.Size = new System.Drawing.Size(54, 20);
		this.numeric.TabIndex = 1;
		this.numeric.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numeric.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numeric.ValueChanged += new System.EventHandler(numeric_ValueChanged);
		this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[11]
		{
			this.buttonShow, this.buttonSave, this.buttonImportImage, this.buttonExportImage, this.toolStripSeparator1, this.buttonImportRx3, this.buttonExportRx3, this.buttonRemoveRx3, this.toolStripSeparator2, this.buttonFullSize,
			this.textSize
		});
		this.toolStrip.Location = new System.Drawing.Point(0, 248);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.Size = new System.Drawing.Size(257, 25);
		this.toolStrip.TabIndex = 2;
		this.buttonShow.Checked = true;
		this.buttonShow.CheckOnClick = true;
		this.buttonShow.CheckState = System.Windows.Forms.CheckState.Checked;
		this.buttonShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow.Image = (System.Drawing.Image)resources.GetObject("buttonShow.Image");
		this.buttonShow.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow.Name = "buttonShow";
		this.buttonShow.Size = new System.Drawing.Size(23, 22);
		this.buttonShow.Text = "Show \\ Hide";
		this.buttonShow.Visible = false;
		this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSave.Enabled = false;
		this.buttonSave.Image = (System.Drawing.Image)resources.GetObject("buttonSave.Image");
		this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSave.Name = "buttonSave";
		this.buttonSave.Size = new System.Drawing.Size(23, 22);
		this.buttonSave.Text = "Save";
		this.buttonSave.Click += new System.EventHandler(buttonSave_Click);
		this.buttonImportImage.AutoSize = false;
		this.buttonImportImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportImage.Image = (System.Drawing.Image)resources.GetObject("buttonImportImage.Image");
		this.buttonImportImage.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportImage.Name = "buttonImportImage";
		this.buttonImportImage.Size = new System.Drawing.Size(20, 22);
		this.buttonImportImage.Text = "Import Image";
		this.buttonImportImage.Click += new System.EventHandler(buttonImportImage_Click);
		this.buttonExportImage.AutoSize = false;
		this.buttonExportImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportImage.Image = (System.Drawing.Image)resources.GetObject("buttonExportImage.Image");
		this.buttonExportImage.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportImage.Name = "buttonExportImage";
		this.buttonExportImage.Size = new System.Drawing.Size(20, 22);
		this.buttonExportImage.Text = "Export Image";
		this.buttonExportImage.Click += new System.EventHandler(buttonExportImage_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonImportRx3.AutoSize = false;
		this.buttonImportRx3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportRx3.Image = (System.Drawing.Image)resources.GetObject("buttonImportRx3.Image");
		this.buttonImportRx3.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportRx3.Name = "buttonImportRx3";
		this.buttonImportRx3.Size = new System.Drawing.Size(20, 22);
		this.buttonImportRx3.Text = "Import Rx3";
		this.buttonImportRx3.Click += new System.EventHandler(buttonImportFsh_Click);
		this.buttonExportRx3.AutoSize = false;
		this.buttonExportRx3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonExportRx3.Image = (System.Drawing.Image)resources.GetObject("buttonExportRx3.Image");
		this.buttonExportRx3.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportRx3.Name = "buttonExportRx3";
		this.buttonExportRx3.Size = new System.Drawing.Size(20, 22);
		this.buttonExportRx3.Text = "Export as Rx3";
		this.buttonExportRx3.Click += new System.EventHandler(buttonExportRx3_Click);
		this.buttonRemoveRx3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemoveRx3.Image = (System.Drawing.Image)resources.GetObject("buttonRemoveRx3.Image");
		this.buttonRemoveRx3.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemoveRx3.Name = "buttonRemoveRx3";
		this.buttonRemoveRx3.Size = new System.Drawing.Size(23, 22);
		this.buttonRemoveRx3.Text = "Remove";
		this.buttonRemoveRx3.Click += new System.EventHandler(buttonRemoveRx3_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.buttonFullSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonFullSize.Image = (System.Drawing.Image)resources.GetObject("buttonFullSize.Image");
		this.buttonFullSize.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonFullSize.Name = "buttonFullSize";
		this.buttonFullSize.Size = new System.Drawing.Size(23, 22);
		this.buttonFullSize.Text = "View Full Size";
		this.buttonFullSize.Visible = false;
		this.buttonFullSize.Click += new System.EventHandler(buttonFullSize_Click);
		this.textSize.BackColor = System.Drawing.Color.White;
		this.textSize.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textSize.Name = "textSize";
		this.textSize.ReadOnly = true;
		this.textSize.Size = new System.Drawing.Size(70, 25);
		this.textSize.Text = "1024 x 1024";
		this.textSize.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pictureBox.Location = new System.Drawing.Point(0, 20);
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.Size = new System.Drawing.Size(257, 228);
		this.pictureBox.TabIndex = 3;
		this.pictureBox.TabStop = false;
		this.openFileDialogBmp.Filter = "Image Files (*.bmp;*.png)|*.bmp;*.png";
		this.openFileDialogBmp.Title = "Open Image File";
		this.openFileDialogRx3.Filter = "rx3 files (*.rx3)|*.rx3";
		this.openFileDialogRx3.Title = "Open rx3 file";
		this.saveFileDialogBmp.Filter = "bmp files (*.bmp)|*.bmp|png files (*.png)|*.png";
		this.saveFileDialogBmp.FilterIndex = 2;
		this.saveFileDialogBmp.Title = "Save image as .bmp or .png";
		this.labelOf.AutoSize = true;
		this.labelOf.Location = new System.Drawing.Point(107, 4);
		this.labelOf.Name = "labelOf";
		this.labelOf.Size = new System.Drawing.Size(19, 13);
		this.labelOf.TabIndex = 4;
		this.labelOf.Text = "of ";
		base.Controls.Add(this.labelOf);
		base.Controls.Add(this.pictureBox);
		base.Controls.Add(this.toolStrip);
		base.Controls.Add(this.numeric);
		base.Controls.Add(this.label);
		base.Name = "MultiViewer2D";
		base.Size = new System.Drawing.Size(257, 273);
		base.Resize += new System.EventHandler(MultiViewer2D_Resize);
		((System.ComponentModel.ISupportInitialize)this.numeric).EndInit();
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
