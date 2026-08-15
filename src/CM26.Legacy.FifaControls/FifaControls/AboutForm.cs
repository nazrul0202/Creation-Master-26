using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class AboutForm : Form
{
	private IContainer components;

	public Label labelProduct;

	public Label labelRelease;

	private Button buttonCountinue;

	public Label label2;

	public Label label1;

	private LinkLabel linkLabel1;

	private LinkLabel linkLabel2;

	private LinkLabel linkLabel3;

	public AboutForm()
	{
		InitializeComponent();
	}

	private void linkFifaMaster_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			VisitLink();
		}
		catch (Exception)
		{
		}
	}

	private void VisitLink()
	{
		Process.Start("http://www.fifa-master.com");
	}

	private void VisitLinkFifaInfinity()
	{
		Process.Start("https://sites.google.com/view/thefootballmaster/home-page");
	}

	private void VisitAbio()
	{
		Process.Start("http://www.abio.org");
	}

	private void VisitUnicef()
	{
		Process.Start("http://www.unicef.org");
	}

	private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			VisitAbio();
		}
		catch (Exception)
		{
		}
	}

	private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			VisitUnicef();
		}
		catch (Exception)
		{
		}
	}

	private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			VisitLinkFifaInfinity();
		}
		catch (Exception)
		{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.AboutForm));
		this.labelProduct = new System.Windows.Forms.Label();
		this.labelRelease = new System.Windows.Forms.Label();
		this.buttonCountinue = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.linkLabel1 = new System.Windows.Forms.LinkLabel();
		this.linkLabel2 = new System.Windows.Forms.LinkLabel();
		this.linkLabel3 = new System.Windows.Forms.LinkLabel();
		base.SuspendLayout();
		this.labelProduct.AutoSize = true;
		this.labelProduct.BackColor = System.Drawing.Color.Transparent;
		this.labelProduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelProduct.ForeColor = System.Drawing.Color.White;
		this.labelProduct.Location = new System.Drawing.Point(107, 114);
		this.labelProduct.Name = "labelProduct";
		this.labelProduct.Size = new System.Drawing.Size(122, 20);
		this.labelProduct.TabIndex = 1;
		this.labelProduct.Text = "Product Name";
		this.labelRelease.AutoSize = true;
		this.labelRelease.BackColor = System.Drawing.Color.Transparent;
		this.labelRelease.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelRelease.ForeColor = System.Drawing.Color.White;
		this.labelRelease.Location = new System.Drawing.Point(107, 143);
		this.labelRelease.Name = "labelRelease";
		this.labelRelease.Size = new System.Drawing.Size(75, 20);
		this.labelRelease.TabIndex = 2;
		this.labelRelease.Text = "Release";
		this.buttonCountinue.BackColor = System.Drawing.Color.FromArgb(20, 190, 140);
		this.buttonCountinue.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.buttonCountinue.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonCountinue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.buttonCountinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.buttonCountinue.ForeColor = System.Drawing.Color.White;
		this.buttonCountinue.Location = new System.Drawing.Point(16, 114);
		this.buttonCountinue.Name = "buttonCountinue";
		this.buttonCountinue.Size = new System.Drawing.Size(81, 49);
		this.buttonCountinue.TabIndex = 4;
		this.buttonCountinue.Text = "Go Back";
		this.buttonCountinue.UseVisualStyleBackColor = false;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.ForeColor = System.Drawing.Color.MediumSpringGreen;
		this.label2.Location = new System.Drawing.Point(13, 180);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(478, 84);
		this.label2.TabIndex = 6;
		this.label2.Text = resources.GetString("label2.Text");
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.MediumSpringGreen;
		this.label1.Location = new System.Drawing.Point(13, 310);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(475, 27);
		this.label1.TabIndex = 7;
		this.label1.Text = "Fifa Master is now available at the following link";
		this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.linkLabel1.AutoSize = true;
		this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
		this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.linkLabel1.LinkColor = System.Drawing.Color.Cyan;
		this.linkLabel1.Location = new System.Drawing.Point(87, 271);
		this.linkLabel1.Name = "linkLabel1";
		this.linkLabel1.Size = new System.Drawing.Size(48, 20);
		this.linkLabel1.TabIndex = 8;
		this.linkLabel1.TabStop = true;
		this.linkLabel1.Text = "ABIO";
		this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
		this.linkLabel2.AutoSize = true;
		this.linkLabel2.BackColor = System.Drawing.Color.Transparent;
		this.linkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.linkLabel2.LinkColor = System.Drawing.Color.Cyan;
		this.linkLabel2.Location = new System.Drawing.Point(327, 271);
		this.linkLabel2.Name = "linkLabel2";
		this.linkLabel2.Size = new System.Drawing.Size(69, 20);
		this.linkLabel2.TabIndex = 9;
		this.linkLabel2.TabStop = true;
		this.linkLabel2.Text = "UNICEF";
		this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel2_LinkClicked);
		this.linkLabel3.AutoSize = true;
		this.linkLabel3.BackColor = System.Drawing.Color.Transparent;
		this.linkLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.linkLabel3.LinkColor = System.Drawing.Color.Cyan;
		this.linkLabel3.Location = new System.Drawing.Point(118, 337);
		this.linkLabel3.Name = "linkLabel3";
		this.linkLabel3.Size = new System.Drawing.Size(262, 20);
		this.linkLabel3.TabIndex = 11;
		this.linkLabel3.TabStop = true;
		this.linkLabel3.Text = "Real Football Simulator Home Page";
		this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel3_LinkClicked);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.ClientSize = new System.Drawing.Size(502, 459);
		base.Controls.Add(this.linkLabel3);
		base.Controls.Add(this.linkLabel2);
		base.Controls.Add(this.linkLabel1);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.buttonCountinue);
		base.Controls.Add(this.labelRelease);
		base.Controls.Add(this.labelProduct);
		base.Controls.Add(this.label1);
		this.DoubleBuffered = true;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AboutForm";
		this.Text = " About Info";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
