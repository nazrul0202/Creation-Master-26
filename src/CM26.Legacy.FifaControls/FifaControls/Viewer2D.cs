using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class Viewer2D : UserControl
{
	public enum SizeMultiplier
	{
		None,
		OneAndHalf,
		Double,
		Half,
		Kit,
		MiniFace,
		Auto256,
		Free
	}

	public delegate bool ImageImportHandler(object sender, Bitmap bitmap);

	public delegate bool ImageDeleteHandler(object sender);

	private Size m_ImageSize;

	private SizeMultiplier m_ImageSizeMultiplier;

	private bool m_AutoTransparency;

	private bool m_ExtendedFormat;

	private Bitmap m_Bitmap;

	private string m_InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

	public ImageImportHandler ImageImport;

	public ImageDeleteHandler ImageDelete;

	private static FullSizeViewer s_FullSizeViewer = new FullSizeViewer();

	private IContainer components;

	public PictureBox picture;

	private ToolStrip toolStrip;

	private ToolStripTextBox textSize;

	private OpenFileDialog openFileDialog;

	public ToolStripButton buttonImportImage;

	public ToolStripButton buttonExportImage;

	public ToolStripButton buttonRemove;

	public ToolStripButton buttonFullSize;

	private ToolStripButton buttonShow;

	private ContextMenuStrip contextMenuStrip;

	private ToolStripMenuItem importImageToolStripMenuItem;

	private ToolStripMenuItem exportImageToolStripMenuItem;

	private ToolStripMenuItem removeToolStripMenuItem;

	[Category("User")]
	[Description("Image Size.")]
	public Size ImageSize
	{
		get
		{
			return m_ImageSize;
		}
		set
		{
			m_ImageSize = value;
		}
	}

	[Category("User")]
	[Description("Image Alternative Size Multiplier.")]
	public SizeMultiplier ImageSizeMultiplier
	{
		get
		{
			return m_ImageSizeMultiplier;
		}
		set
		{
			m_ImageSizeMultiplier = value;
		}
	}

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
	[Description("Remove Button Visible.")]
	public bool RemoveButton
	{
		get
		{
			return buttonRemove.Visible;
		}
		set
		{
			buttonRemove.Visible = value;
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
	[Description("Show Button Checked.")]
	public bool ShowButtonChecked
	{
		get
		{
			return buttonShow.Checked;
		}
		set
		{
			buttonShow.Checked = value;
		}
	}

	[Category("User")]
	[Description("Button strip visible.")]
	public bool ButtonStripVisible
	{
		get
		{
			return toolStrip.Visible;
		}
		set
		{
			toolStrip.Visible = value;
		}
	}

	[Category("User")]
	[Description("Extended Format")]
	public bool ExtendedFormat
	{
		get
		{
			return m_ExtendedFormat;
		}
		set
		{
			m_ExtendedFormat = value;
		}
	}

	public Bitmap CurrentBitmap
	{
		get
		{
			return m_Bitmap;
		}
		set
		{
			m_Bitmap = value;
			if (buttonShow.Checked)
			{
				picture.BackgroundImage = value;
				EnableButtons();
			}
		}
	}

	[Category("User")]
	[Description("Image Layout.")]
	public ImageLayout ImageLayout
	{
		get
		{
			return picture.BackgroundImageLayout;
		}
		set
		{
			picture.BackgroundImageLayout = value;
		}
	}

	public string CurrentFolder
	{
		set
		{
			m_InitialDirectory = value;
		}
	}

	public Viewer2D()
	{
		InitializeComponent();
	}

	private void buttonImportImage_Click(object sender, EventArgs e)
	{
		ImportImage();
	}

	private void ImportImage()
	{
		Bitmap bitmap = BrowseAndCheckBitmap();
		if (bitmap != null)
		{
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				bitmap = GraphicUtil.Get32bitBitmap(bitmap);
			}
			if (ImageImport == null || ImageImport(this, bitmap))
			{
				CurrentBitmap = bitmap;
				Refresh();
			}
		}
	}

	private Bitmap BrowseAndCheckBitmap()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.CheckFileExists = true;
		openFileDialog.Multiselect = false;
		openFileDialog.InitialDirectory = m_InitialDirectory;
		openFileDialog.RestoreDirectory = true;
		if (m_ExtendedFormat)
		{
			openFileDialog.Filter = "Image Files (*.bmp;*.png;*.jpg)|*.bmp;*.png;*.jpg";
		}
		else
		{
			openFileDialog.Filter = "Image Files (*.bmp;*.png)|*.bmp;*.png";
		}
		openFileDialog.FilterIndex = 1;
		openFileDialog.Title = "Open Image File";
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			openFileDialog.Dispose();
			return null;
		}
		string fileName = openFileDialog.FileName;
		m_InitialDirectory = Path.GetDirectoryName(fileName);
		openFileDialog.Dispose();
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
		int num = m_ImageSize.Width;
		int num2 = m_ImageSize.Height;
		if (bitmap.Width != num || bitmap.Height != num2)
		{
			switch (m_ImageSizeMultiplier)
			{
			case SizeMultiplier.OneAndHalf:
				if (bitmap.Width == num + num / 2 && bitmap.Height == num2 + num2 / 2)
				{
					bitmap = GraphicUtil.ResizeBitmap(bitmap, num, num2, InterpolationMode.HighQualityBilinear);
				}
				break;
			case SizeMultiplier.Double:
				if (bitmap.Width == num * 2 && bitmap.Height == num2 * 2)
				{
					bitmap = GraphicUtil.ResizeBitmap(bitmap, num, num2, InterpolationMode.HighQualityBilinear);
				}
				break;
			case SizeMultiplier.Half:
				if (bitmap.Width == num / 2 && bitmap.Height == num2 / 2)
				{
					bitmap = GraphicUtil.ResizeBitmap(bitmap, num, num2, InterpolationMode.HighQualityBilinear);
				}
				break;
			case SizeMultiplier.Kit:
				if ((bitmap.Width == 512 && bitmap.Height == 512) || (bitmap.Width == 768 && bitmap.Height == 768) || (bitmap.Width == 1024 && bitmap.Height == 1024))
				{
					bitmap = GraphicUtil.ResizeBitmap(bitmap, num, num2, InterpolationMode.HighQualityBilinear);
				}
				break;
			case SizeMultiplier.MiniFace:
				if (bitmap.PixelFormat == PixelFormat.Format24bppRgb || bitmap.PixelFormat == PixelFormat.Format32bppArgb || bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
				{
					num = 128;
					num2 = 128;
					if (m_AutoTransparency)
					{
						bitmap = GraphicUtil.MakeAutoTransparent(bitmap);
					}
					bitmap = GraphicUtil.ResizeBitmap(bitmap, 128, 128, InterpolationMode.HighQualityBilinear);
					break;
				}
				return null;
			case SizeMultiplier.Auto256:
				if (num > num2)
				{
					num = 256;
					num2 = 256 * bitmap.Height / bitmap.Width;
				}
				else
				{
					num2 = 256;
					num = 256 * bitmap.Width / bitmap.Height;
				}
				bitmap = GraphicUtil.ResizeBitmap(bitmap, num, num2, InterpolationMode.HighQualityBilinear);
				if (num != 256 || num2 != 256)
				{
					bitmap = GraphicUtil.CanvasSizeBitmap(bitmap, 256, 256);
				}
				break;
			}
			if (m_ImageSizeMultiplier != SizeMultiplier.Free && (bitmap.Width != num || bitmap.Height != num2))
			{
				Cursor.Current = Cursors.Default;
				FifaEnvironment.UserMessages.ShowMessage(5015);
				return null;
			}
		}
		Cursor.Current = Cursors.Default;
		return bitmap;
	}

	private void buttonRemove_Click(object sender, EventArgs e)
	{
		RemoveImage();
	}

	private void RemoveImage()
	{
		picture.BackgroundImage = null;
		CurrentBitmap = null;
		if (ImageDelete != null)
		{
			ImageDelete(this);
		}
	}

	private void buttonExportImage_Click(object sender, EventArgs e)
	{
		ExportImage();
	}

	private void ExportImage()
	{
		AskAndSaveBitmap(m_Bitmap);
	}

	private bool AskAndSaveBitmap(Bitmap bitmap)
	{
		if (bitmap == null)
		{
			return false;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|png files (*.png)|*.png";
		saveFileDialog.InitialDirectory = m_InitialDirectory;
		saveFileDialog.FilterIndex = 2;
		saveFileDialog.Title = "Save picture as .bmp or .png";
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			saveFileDialog.Dispose();
			return false;
		}
		Cursor.Current = Cursors.WaitCursor;
		FindForm().Refresh();
		string extension = Path.GetExtension(saveFileDialog.FileName);
		m_InitialDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
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
				Cursor.Current = Cursors.Default;
				return false;
			}
			format = ImageFormat.Png;
		}
		_ = saveFileDialog.FileName;
		bitmap.Save(saveFileDialog.FileName, format);
		saveFileDialog.Dispose();
		Cursor.Current = Cursors.Default;
		return true;
	}

	public void DisposeBitmap()
	{
		if (picture.BackgroundImage != null)
		{
			picture.BackgroundImage.Dispose();
			picture.BackgroundImage = null;
		}
	}

	private void buttonFullSize_Click(object sender, EventArgs e)
	{
		if (m_Bitmap != null)
		{
			s_FullSizeViewer.SetImage(m_Bitmap);
			s_FullSizeViewer.ShowDialog();
		}
	}

	private void buttonShow_Click(object sender, EventArgs e)
	{
		EnableButtons();
	}

	private void EnableButtons()
	{
		bool flag = buttonShow.Checked && m_Bitmap != null;
		buttonExportImage.Enabled = flag;
		buttonFullSize.Enabled = flag;
		buttonRemove.Enabled = flag;
		if (!flag)
		{
			textSize.Text = string.Empty;
			picture.BackgroundImage = null;
		}
		else
		{
			textSize.Text = m_Bitmap.Width + " x " + m_Bitmap.Height;
			picture.BackgroundImage = m_Bitmap;
		}
		picture.Enabled = buttonShow.Checked;
		buttonImportImage.Enabled = true;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.Viewer2D));
		this.picture = new System.Windows.Forms.PictureBox();
		this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.importImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.buttonShow = new System.Windows.Forms.ToolStripButton();
		this.buttonImportImage = new System.Windows.Forms.ToolStripButton();
		this.buttonExportImage = new System.Windows.Forms.ToolStripButton();
		this.buttonFullSize = new System.Windows.Forms.ToolStripButton();
		this.buttonRemove = new System.Windows.Forms.ToolStripButton();
		this.textSize = new System.Windows.Forms.ToolStripTextBox();
		this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
		((System.ComponentModel.ISupportInitialize)this.picture).BeginInit();
		this.contextMenuStrip.SuspendLayout();
		this.toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.picture.BackColor = System.Drawing.SystemColors.Control;
		this.picture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.picture.ContextMenuStrip = this.contextMenuStrip;
		this.picture.Dock = System.Windows.Forms.DockStyle.Fill;
		this.picture.Location = new System.Drawing.Point(0, 0);
		this.picture.Name = "picture";
		this.picture.Size = new System.Drawing.Size(197, 187);
		this.picture.TabIndex = 0;
		this.picture.TabStop = false;
		this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.importImageToolStripMenuItem, this.exportImageToolStripMenuItem, this.removeToolStripMenuItem });
		this.contextMenuStrip.Name = "contextMenuStrip";
		this.contextMenuStrip.Size = new System.Drawing.Size(147, 70);
		this.importImageToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("importImageToolStripMenuItem.Image");
		this.importImageToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.importImageToolStripMenuItem.Name = "importImageToolStripMenuItem";
		this.importImageToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
		this.importImageToolStripMenuItem.Text = "Import Image";
		this.importImageToolStripMenuItem.Click += new System.EventHandler(buttonImportImage_Click);
		this.exportImageToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("exportImageToolStripMenuItem.Image");
		this.exportImageToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
		this.exportImageToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
		this.exportImageToolStripMenuItem.Text = "Export Image";
		this.exportImageToolStripMenuItem.Click += new System.EventHandler(buttonExportImage_Click);
		this.removeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("removeToolStripMenuItem.Image");
		this.removeToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
		this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
		this.removeToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
		this.removeToolStripMenuItem.Text = "Remove";
		this.removeToolStripMenuItem.Click += new System.EventHandler(buttonRemove_Click);
		this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.buttonShow, this.buttonImportImage, this.buttonExportImage, this.buttonFullSize, this.buttonRemove, this.textSize });
		this.toolStrip.Location = new System.Drawing.Point(0, 187);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.Size = new System.Drawing.Size(197, 25);
		this.toolStrip.TabIndex = 2;
		this.toolStrip.Text = "toolStrip1";
		this.buttonShow.Checked = true;
		this.buttonShow.CheckOnClick = true;
		this.buttonShow.CheckState = System.Windows.Forms.CheckState.Checked;
		this.buttonShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonShow.Image = (System.Drawing.Image)resources.GetObject("buttonShow.Image");
		this.buttonShow.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShow.Name = "buttonShow";
		this.buttonShow.Size = new System.Drawing.Size(23, 22);
		this.buttonShow.Text = "Show / Hide";
		this.buttonShow.Click += new System.EventHandler(buttonShow_Click);
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
		this.buttonFullSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonFullSize.Image = (System.Drawing.Image)resources.GetObject("buttonFullSize.Image");
		this.buttonFullSize.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonFullSize.Name = "buttonFullSize";
		this.buttonFullSize.Size = new System.Drawing.Size(23, 22);
		this.buttonFullSize.Text = "View Full Size";
		this.buttonFullSize.Visible = false;
		this.buttonFullSize.Click += new System.EventHandler(buttonFullSize_Click);
		this.buttonRemove.AutoSize = false;
		this.buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove.Image = (System.Drawing.Image)resources.GetObject("buttonRemove.Image");
		this.buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove.Name = "buttonRemove";
		this.buttonRemove.Size = new System.Drawing.Size(20, 22);
		this.buttonRemove.Text = "Remove";
		this.buttonRemove.Visible = false;
		this.buttonRemove.Click += new System.EventHandler(buttonRemove_Click);
		this.textSize.BackColor = System.Drawing.Color.White;
		this.textSize.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textSize.Name = "textSize";
		this.textSize.ReadOnly = true;
		this.textSize.Size = new System.Drawing.Size(65, 25);
		this.textSize.Text = "1024 x 1024";
		this.textSize.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Transparent;
		base.Controls.Add(this.picture);
		base.Controls.Add(this.toolStrip);
		base.Name = "Viewer2D";
		base.Size = new System.Drawing.Size(197, 212);
		((System.ComponentModel.ISupportInitialize)this.picture).EndInit();
		this.contextMenuStrip.ResumeLayout(false);
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
