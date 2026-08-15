using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CreationMaster;

public class TvForm : Form
{
	private IContainer components;

	public TvForm()
	{
		InitializeComponent();
	}

	public void Clean()
	{
		base.Visible = false;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.TvForm));
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1165, 798);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "TvForm";
		this.Text = "TvForm";
		base.ResumeLayout(false);
	}
}
