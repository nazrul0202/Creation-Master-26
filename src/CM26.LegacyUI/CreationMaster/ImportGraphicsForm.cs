using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

public class ImportGraphicsForm : Form
{
	private string m_InputFolder;

	private string m_OutputFolder;

	private IContainer components;

	private Label label1;

	private Label label2;

	private TextBox inputText;

	private TextBox outputText;

	private Button buttonBrowseInputFolder;

	private Button buttonBrowseOutputFolder;

	private FolderBrowserDialog inputFolderBrowser;

	private FolderBrowserDialog outputFolderBrowser;

	private Button buttonImportMinifaces;

	private Label labelCounter;

	private Button buttonImportKits;

	public ImportGraphicsForm()
	{
		InitializeComponent();
	}

	public void Clean()
	{
		base.Visible = false;
	}

	private void buttonBrowseInputFolder_Click(object sender, EventArgs e)
	{
		if (m_InputFolder == null)
		{
			m_InputFolder = FifaEnvironment.TempFolder;
		}
		inputFolderBrowser.SelectedPath = m_InputFolder;
		inputFolderBrowser.Description = "Select the input folder";
		inputFolderBrowser.ShowNewFolderButton = true;
		if (inputFolderBrowser.ShowDialog() == DialogResult.OK)
		{
			m_InputFolder = inputFolderBrowser.SelectedPath;
			inputText.Text = m_InputFolder;
		}
	}

	private void buttonBrowseOutputFolder_Click(object sender, EventArgs e)
	{
		if (m_OutputFolder == null)
		{
			m_InputFolder = FifaEnvironment.TempFolder;
		}
		outputFolderBrowser.SelectedPath = m_OutputFolder;
		outputFolderBrowser.Description = "Select the output folder";
		outputFolderBrowser.ShowNewFolderButton = true;
		if (outputFolderBrowser.ShowDialog() == DialogResult.OK)
		{
			m_OutputFolder = outputFolderBrowser.SelectedPath;
			outputText.Text = m_OutputFolder;
		}
	}

	private void ConvertDdsToPng()
	{
		string[] files = Directory.GetFiles(m_InputFolder, "*.dds");
		foreach (string text in files)
		{
			Bitmap bitmap = new DdsFile(text).GetBitmap();
			string filename = m_OutputFolder + "\\" + Path.GetFileNameWithoutExtension(text) + ".png";
			bitmap.Save(filename, ImageFormat.Png);
			labelCounter.Text = Path.GetFileName(text);
			labelCounter.Refresh();
		}
	}

	private void buttonImportMinifaces_Click(object sender, EventArgs e)
	{
		ConvertDdsToPng();
	}

	private void buttonImportKits_Click(object sender, EventArgs e)
	{
		string[] files = Directory.GetFiles(m_InputFolder, "jersey*.png");
		new Rectangle(0, 0, 1024, 188);
		string[] array = files;
		foreach (string text in array)
		{
			string text2 = Path.GetFileName(text).Substring(6);
			string text3 = Path.GetDirectoryName(text) + "\\shorts" + text2;
			string text4 = Path.GetDirectoryName(text) + "\\socks" + text2;
			string text5 = Path.GetDirectoryName(text) + "\\crest" + text2;
			Bitmap bitmap = new Bitmap(text);
			Bitmap bitmap2 = null;
			Bitmap bitmap3 = null;
			Bitmap bitmap4 = null;
			if (File.Exists(text4))
			{
				bitmap2 = new Bitmap(text4);
			}
			if (File.Exists(text3))
			{
				bitmap3 = new Bitmap(text3);
			}
			if (File.Exists(text5))
			{
				bitmap4 = new Bitmap(text5);
			}
			if (bitmap.Width != 1024 && bitmap.Height != 1024)
			{
				bitmap = GraphicUtil.ResizeBitmap(bitmap, 1024, 1024, InterpolationMode.Bicubic);
			}
			if (bitmap2 != null && bitmap2.Width != 1024 && bitmap.Height != 188)
			{
				bitmap2 = GraphicUtil.ResizeBitmap(bitmap2, 1024, 188, InterpolationMode.Bicubic);
			}
			if (bitmap3 != null && bitmap2 != null)
			{
				bitmap3.SetResolution(96f, 96f);
				bitmap2.SetResolution(96f, 96f);
				GraphicUtil.DrawOver(bitmap3, bitmap2);
				bitmap2.Dispose();
			}
			string text6 = m_OutputFolder + "\\kit" + text2;
			text6 = text6.Replace("_color.png", ".rx3");
			text6 = m_OutputFolder + "\\" + Path.GetFileName(text);
			bitmap.Save(text6);
			bitmap.Dispose();
			if (bitmap3 != null)
			{
				text6 = m_OutputFolder + "\\" + Path.GetFileName(text3);
				bitmap3.Save(text6);
				bitmap3.Dispose();
			}
			if (bitmap4 != null)
			{
				text6 = m_OutputFolder + "\\" + Path.GetFileName(text5);
				bitmap4.Save(text6);
				bitmap4.Dispose();
			}
			labelCounter.Text = Path.GetFileName(text);
			labelCounter.Refresh();
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.inputText = new System.Windows.Forms.TextBox();
		this.outputText = new System.Windows.Forms.TextBox();
		this.buttonBrowseInputFolder = new System.Windows.Forms.Button();
		this.buttonBrowseOutputFolder = new System.Windows.Forms.Button();
		this.inputFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
		this.outputFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
		this.buttonImportMinifaces = new System.Windows.Forms.Button();
		this.labelCounter = new System.Windows.Forms.Label();
		this.buttonImportKits = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(26, 39);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(31, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Input";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(26, 76);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(39, 13);
		this.label2.TabIndex = 1;
		this.label2.Text = "Output";
		this.inputText.Location = new System.Drawing.Point(70, 37);
		this.inputText.Name = "inputText";
		this.inputText.ReadOnly = true;
		this.inputText.Size = new System.Drawing.Size(403, 20);
		this.inputText.TabIndex = 2;
		this.outputText.Location = new System.Drawing.Point(70, 69);
		this.outputText.Name = "outputText";
		this.outputText.ReadOnly = true;
		this.outputText.Size = new System.Drawing.Size(403, 20);
		this.outputText.TabIndex = 3;
		this.buttonBrowseInputFolder.Location = new System.Drawing.Point(489, 37);
		this.buttonBrowseInputFolder.Name = "buttonBrowseInputFolder";
		this.buttonBrowseInputFolder.Size = new System.Drawing.Size(75, 23);
		this.buttonBrowseInputFolder.TabIndex = 4;
		this.buttonBrowseInputFolder.Text = "Browse";
		this.buttonBrowseInputFolder.UseVisualStyleBackColor = true;
		this.buttonBrowseInputFolder.Click += new System.EventHandler(buttonBrowseInputFolder_Click);
		this.buttonBrowseOutputFolder.Location = new System.Drawing.Point(489, 67);
		this.buttonBrowseOutputFolder.Name = "buttonBrowseOutputFolder";
		this.buttonBrowseOutputFolder.Size = new System.Drawing.Size(75, 23);
		this.buttonBrowseOutputFolder.TabIndex = 5;
		this.buttonBrowseOutputFolder.Text = "Browse";
		this.buttonBrowseOutputFolder.UseVisualStyleBackColor = true;
		this.buttonBrowseOutputFolder.Click += new System.EventHandler(buttonBrowseOutputFolder_Click);
		this.buttonImportMinifaces.Location = new System.Drawing.Point(69, 110);
		this.buttonImportMinifaces.Name = "buttonImportMinifaces";
		this.buttonImportMinifaces.Size = new System.Drawing.Size(75, 57);
		this.buttonImportMinifaces.TabIndex = 6;
		this.buttonImportMinifaces.Text = "Import Minifaces";
		this.buttonImportMinifaces.UseVisualStyleBackColor = true;
		this.buttonImportMinifaces.Click += new System.EventHandler(buttonImportMinifaces_Click);
		this.labelCounter.AutoSize = true;
		this.labelCounter.Location = new System.Drawing.Point(119, 188);
		this.labelCounter.Name = "labelCounter";
		this.labelCounter.Size = new System.Drawing.Size(67, 13);
		this.labelCounter.TabIndex = 7;
		this.labelCounter.Text = "_ _ _ _ _ _ _";
		this.buttonImportKits.Location = new System.Drawing.Point(162, 110);
		this.buttonImportKits.Name = "buttonImportKits";
		this.buttonImportKits.Size = new System.Drawing.Size(75, 57);
		this.buttonImportKits.TabIndex = 8;
		this.buttonImportKits.Text = "Import Kits";
		this.buttonImportKits.UseVisualStyleBackColor = true;
		this.buttonImportKits.Click += new System.EventHandler(buttonImportKits_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1024, 780);
		base.Controls.Add(this.buttonImportKits);
		base.Controls.Add(this.labelCounter);
		base.Controls.Add(this.buttonImportMinifaces);
		base.Controls.Add(this.buttonBrowseOutputFolder);
		base.Controls.Add(this.buttonBrowseInputFolder);
		base.Controls.Add(this.outputText);
		base.Controls.Add(this.inputText);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "ImportGraphicsForm";
		this.Text = "ImportGraphicsForm";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
