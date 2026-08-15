using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FifaControls;

public class Previewer : UserControl
{
	private IContainer components;

	private PictureBox pictureBox;

	public Previewer()
	{
		InitializeComponent();
	}

	public void Show(Image bitmap, int x, int y, int srcWidth, int srcHeight)
	{
		if (bitmap == null)
		{
			pictureBox.BackgroundImage = null;
			return;
		}
		_ = pictureBox.Width;
		_ = pictureBox.Height;
		if (pictureBox.BackgroundImage != null)
		{
			pictureBox.BackgroundImage.Dispose();
		}
		pictureBox.BackgroundImage = new Bitmap(pictureBox.Width, pictureBox.Height, PixelFormat.Format32bppArgb);
		Graphics graphics = Graphics.FromImage(pictureBox.BackgroundImage);
		graphics.InterpolationMode = InterpolationMode.Bicubic;
		graphics.DrawImage((Bitmap)bitmap, new Rectangle(0, 0, pictureBox.Width, pictureBox.Height), new Rectangle(x, y, srcWidth, srcHeight), GraphicsUnit.Pixel);
		graphics.Dispose();
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
		this.pictureBox = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
		base.SuspendLayout();
		this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pictureBox.Location = new System.Drawing.Point(0, 0);
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.Size = new System.Drawing.Size(150, 150);
		this.pictureBox.TabIndex = 0;
		this.pictureBox.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.pictureBox);
		base.Name = "Previewer";
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		base.ResumeLayout(false);
	}
}
