using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using FifaLibrary;
using HtmlAgilityPack;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace CreationMaster;

public class WebBrowserForm : Form
{
	private WebGrabber m_WebGrabber = new WebGrabber();

	private WebPatchLoader m_WebPatchLoader = new WebPatchLoader();

	private string m_CurrentHtmlString = string.Empty;

	private IContainer components;

	private ToolStrip toolStripWeb;

	private ToolStripButton buttonTm;

	private ToolStripButton buttonSw;

	private ToolStripButton buttonBack;

	private ToolStripButton buttonForward;

	private ToolStripButton buttonImportWeb;

	private ToolStripButton buttonReload;

	private WebView2 webView21;

	private ToolStripButton buttonSofifa;

	private ToolStripButton buttonSortitusi;

	private ToolStripLabel labelBasePlayerId;

	private ToolStripTextBox textBasePlayerId;

	private ToolStripLabel labelBaseTeamId;

	private ToolStripTextBox textBaseTeamId;

	public WebBrowserForm()
	{
		InitializeComponent();
		_ = FifaEnvironment.LaunchDir;
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		folderPath += "\\FM_Temp";
		Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(null, folderPath);
		task.Wait();
		CoreWebView2Environment result = task.Result;
		webView21.EnsureCoreWebView2Async(result);
	}

	private void buttonSofifa_Click(object sender, EventArgs e)
	{
		webView21.CoreWebView2.Settings.IsScriptEnabled = true;
		webView21.CoreWebView2.Navigate("https://sofifa.com/");
	}

	private void buttonTransfermrkt_Click(object sender, EventArgs e)
	{
		webView21.CoreWebView2.Navigate("https://www.transfermarkt.com/");
	}

	private void buttonSortitusi_Click(object sender, EventArgs e)
	{
		webView21.CoreWebView2.Navigate("https://sortitoutsi.net/search/database");
	}

	private void buttonImportTeamAs_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		string url = webView21.Source.ToString();
		HtmlWeb htmlWeb = new HtmlWeb();
		HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
		if (m_CurrentHtmlString.Contains("sofifa") || m_CurrentHtmlString.Contains("sortitoutsi"))
		{
			htmlDocument.LoadHtml(m_CurrentHtmlString);
		}
		else
		{
			htmlDocument = htmlWeb.Load(url);
		}
		if (m_WebGrabber.ExtractInfoFromWeb(htmlDocument))
		{
			m_WebPatchLoader.Load(m_WebGrabber.WebTable, m_WebGrabber.WebPictures);
			m_WebPatchLoader.ShowDialog();
		}
		Cursor.Current = Cursors.Default;
	}

	private void buttonBack_Click(object sender, EventArgs e)
	{
		webView21.GoBack();
	}

	private void buttonForward_Click(object sender, EventArgs e)
	{
		webView21.GoForward();
	}

	private void toolStripButton1_Click(object sender, EventArgs e)
	{
		webView21.Refresh();
	}

	private async void webView21_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
	{
		buttonBack.Enabled = webView21.CanGoBack;
		buttonForward.Enabled = webView21.CanGoForward;
		buttonReload.Enabled = buttonBack.Enabled || buttonForward.Enabled;
		m_CurrentHtmlString = await webView21.ExecuteScriptAsync("document.documentElement.outerHTML;");
		m_CurrentHtmlString = Regex.Unescape(m_CurrentHtmlString);
		m_CurrentHtmlString = m_CurrentHtmlString.Remove(0, 1);
		m_CurrentHtmlString = m_CurrentHtmlString.Remove(m_CurrentHtmlString.Length - 1, 1);
		buttonImportWeb.Enabled = m_WebGrabber.Sync(webView21.CoreWebView2.DocumentTitle, webView21.CoreWebView2.Source);
	}

	private void textBaseId_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	private void WebBrowserForm_Load(object sender, EventArgs e)
	{
		textBasePlayerId.Text = FifaEnvironment.Players.MinId.ToString();
		textBaseTeamId.Text = FifaEnvironment.Teams.MinId.ToString();
	}

	private void textBasePlayerId_TextChanged(object sender, EventArgs e)
	{
		try
		{
			int minId = Convert.ToInt32(textBasePlayerId.Text);
			FifaEnvironment.Players.MinId = minId;
		}
		catch
		{
		}
	}

	private void textBaseTeamId_TextChanged(object sender, EventArgs e)
	{
		try
		{
			int minId = Convert.ToInt32(textBaseTeamId.Text);
			FifaEnvironment.Teams.MinId = minId;
		}
		catch
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreationMaster.WebBrowserForm));
		this.toolStripWeb = new System.Windows.Forms.ToolStrip();
		this.buttonTm = new System.Windows.Forms.ToolStripButton();
		this.buttonSofifa = new System.Windows.Forms.ToolStripButton();
		this.buttonSortitusi = new System.Windows.Forms.ToolStripButton();
		this.buttonSw = new System.Windows.Forms.ToolStripButton();
		this.buttonBack = new System.Windows.Forms.ToolStripButton();
		this.buttonForward = new System.Windows.Forms.ToolStripButton();
		this.buttonReload = new System.Windows.Forms.ToolStripButton();
		this.buttonImportWeb = new System.Windows.Forms.ToolStripButton();
		this.labelBasePlayerId = new System.Windows.Forms.ToolStripLabel();
		this.textBasePlayerId = new System.Windows.Forms.ToolStripTextBox();
		this.labelBaseTeamId = new System.Windows.Forms.ToolStripLabel();
		this.textBaseTeamId = new System.Windows.Forms.ToolStripTextBox();
		this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
		this.toolStripWeb.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.webView21).BeginInit();
		base.SuspendLayout();
		this.toolStripWeb.Items.AddRange(new System.Windows.Forms.ToolStripItem[12]
		{
			this.buttonTm, this.buttonSofifa, this.buttonSortitusi, this.buttonSw, this.buttonBack, this.buttonForward, this.buttonReload, this.buttonImportWeb, this.labelBasePlayerId, this.textBasePlayerId,
			this.labelBaseTeamId, this.textBaseTeamId
		});
		this.toolStripWeb.Location = new System.Drawing.Point(0, 0);
		this.toolStripWeb.Name = "toolStripWeb";
		this.toolStripWeb.Size = new System.Drawing.Size(1010, 55);
		this.toolStripWeb.TabIndex = 1;
		this.toolStripWeb.Text = "toolStrip1";
		this.buttonTm.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonTm.Image = (System.Drawing.Image)resources.GetObject("buttonTm.Image");
		this.buttonTm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonTm.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonTm.Name = "buttonTm";
		this.buttonTm.Size = new System.Drawing.Size(52, 52);
		this.buttonTm.Text = "Connect to Tranfermrkt";
		this.buttonTm.Click += new System.EventHandler(buttonTransfermrkt_Click);
		this.buttonSofifa.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSofifa.Image = (System.Drawing.Image)resources.GetObject("buttonSofifa.Image");
		this.buttonSofifa.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonSofifa.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSofifa.Name = "buttonSofifa";
		this.buttonSofifa.Size = new System.Drawing.Size(52, 52);
		this.buttonSofifa.Text = "SOFIFA";
		this.buttonSofifa.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
		this.buttonSofifa.Click += new System.EventHandler(buttonSofifa_Click);
		this.buttonSortitusi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSortitusi.Image = (System.Drawing.Image)resources.GetObject("buttonSortitusi.Image");
		this.buttonSortitusi.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonSortitusi.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSortitusi.Name = "buttonSortitusi";
		this.buttonSortitusi.Size = new System.Drawing.Size(52, 52);
		this.buttonSortitusi.Text = "Sortitoutsi";
		this.buttonSortitusi.Click += new System.EventHandler(buttonSortitusi_Click);
		this.buttonSw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSw.Image = (System.Drawing.Image)resources.GetObject("buttonSw.Image");
		this.buttonSw.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonSw.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSw.Name = "buttonSw";
		this.buttonSw.Size = new System.Drawing.Size(52, 52);
		this.buttonSw.Text = "Connect to Soccerway";
		this.buttonSw.Visible = false;
		this.buttonBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonBack.Enabled = false;
		this.buttonBack.Image = (System.Drawing.Image)resources.GetObject("buttonBack.Image");
		this.buttonBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonBack.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonBack.Name = "buttonBack";
		this.buttonBack.Size = new System.Drawing.Size(52, 52);
		this.buttonBack.Text = "Navigate Back";
		this.buttonBack.Click += new System.EventHandler(buttonBack_Click);
		this.buttonForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonForward.Enabled = false;
		this.buttonForward.Image = (System.Drawing.Image)resources.GetObject("buttonForward.Image");
		this.buttonForward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonForward.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonForward.Name = "buttonForward";
		this.buttonForward.Size = new System.Drawing.Size(52, 52);
		this.buttonForward.Text = "Navigate Forward";
		this.buttonForward.Click += new System.EventHandler(buttonForward_Click);
		this.buttonReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonReload.Enabled = false;
		this.buttonReload.Image = (System.Drawing.Image)resources.GetObject("buttonReload.Image");
		this.buttonReload.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonReload.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonReload.Name = "buttonReload";
		this.buttonReload.Size = new System.Drawing.Size(52, 52);
		this.buttonReload.Text = "Reload";
		this.buttonReload.Click += new System.EventHandler(toolStripButton1_Click);
		this.buttonImportWeb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonImportWeb.Enabled = false;
		this.buttonImportWeb.Image = (System.Drawing.Image)resources.GetObject("buttonImportWeb.Image");
		this.buttonImportWeb.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
		this.buttonImportWeb.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonImportWeb.Name = "buttonImportWeb";
		this.buttonImportWeb.Size = new System.Drawing.Size(52, 52);
		this.buttonImportWeb.Text = "Import from Web";
		this.buttonImportWeb.Click += new System.EventHandler(buttonImportTeamAs_Click);
		this.labelBasePlayerId.Name = "labelBasePlayerId";
		this.labelBasePlayerId.Size = new System.Drawing.Size(82, 52);
		this.labelBasePlayerId.Text = "Base Player Id:";
		this.textBasePlayerId.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textBasePlayerId.Name = "textBasePlayerId";
		this.textBasePlayerId.Size = new System.Drawing.Size(100, 55);
		this.textBasePlayerId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBaseId_KeyPress);
		this.textBasePlayerId.TextChanged += new System.EventHandler(textBasePlayerId_TextChanged);
		this.labelBaseTeamId.Name = "labelBaseTeamId";
		this.labelBaseTeamId.Size = new System.Drawing.Size(79, 52);
		this.labelBaseTeamId.Text = "Base Team Id:";
		this.textBaseTeamId.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textBaseTeamId.Name = "textBaseTeamId";
		this.textBaseTeamId.Size = new System.Drawing.Size(100, 55);
		this.textBaseTeamId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBaseId_KeyPress);
		this.textBaseTeamId.TextChanged += new System.EventHandler(textBaseTeamId_TextChanged);
		this.webView21.AllowExternalDrop = true;
		this.webView21.CreationProperties = null;
		this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
		this.webView21.Dock = System.Windows.Forms.DockStyle.Fill;
		this.webView21.Location = new System.Drawing.Point(0, 55);
		this.webView21.Name = "webView21";
		this.webView21.Size = new System.Drawing.Size(1010, 688);
		this.webView21.TabIndex = 2;
		this.webView21.ZoomFactor = 1.0;
		this.webView21.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(webView21_NavigationCompleted);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1010, 743);
		base.Controls.Add(this.webView21);
		base.Controls.Add(this.toolStripWeb);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Margin = new System.Windows.Forms.Padding(2);
		base.Name = "WebBrowserForm";
		this.Text = "Web Browser";
		base.Load += new System.EventHandler(WebBrowserForm_Load);
		this.toolStripWeb.ResumeLayout(false);
		this.toolStripWeb.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.webView21).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
