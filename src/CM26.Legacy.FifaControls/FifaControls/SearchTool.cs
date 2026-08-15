using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class SearchTool : UserControl
{
	private enum SearchMode
	{
		SearchExact,
		SearchStarting,
		SearchContaining
	}

	public delegate void SearchEventHandler(object sender, object obj);

	private ArrayList m_ObjectList;

	private object m_FoundObject;

	private bool m_HasFound;

	private bool m_CaseSensitive;

	private int m_CurrentIndex;

	private string m_Pattern;

	private SearchMode m_SearchMode = SearchMode.SearchContaining;

	[Category("Event")]
	[Description("Search done.")]
	public SearchEventHandler CallBack;

	private IContainer components;

	private ToolStrip toolStrip;

	public ToolStripTextBox textBox;

	public ToolStripButton buttonSearchExact;

	public ToolStripButton buttonSearchStart;

	public ToolStripButton buttonSearchContain;

	public ToolStripButton buttonCaseSensitive;

	[Category("User")]
	[Description("Width of the text box.")]
	public int TextWidth
	{
		get
		{
			return textBox.Width;
		}
		set
		{
			textBox.Size = new Size(value, textBox.Height);
		}
	}

	public ArrayList ObjectList
	{
		get
		{
			return m_ObjectList;
		}
		set
		{
			m_ObjectList = value;
		}
	}

	public object FoundObject => m_FoundObject;

	public bool HasFound => m_HasFound;

	public bool IsCaseSensitive => m_CaseSensitive;

	public int CurrentIndex
	{
		get
		{
			return m_CurrentIndex;
		}
		set
		{
			m_CurrentIndex = value;
		}
	}

	public SearchTool()
	{
		InitializeComponent();
	}

	private void textBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyValue == 13)
		{
			Search();
		}
	}

	private void buttonSearchExact_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchExact;
		Search();
	}

	private void buttonSearchStart_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchStarting;
		Search();
	}

	private void buttonSearchContain_Click(object sender, EventArgs e)
	{
		m_SearchMode = SearchMode.SearchContaining;
		Search();
	}

	public bool Search()
	{
		m_Pattern = textBox.Text;
		if (!IsCaseSensitive)
		{
			m_Pattern = m_Pattern.ToLower();
		}
		int currentIndex = m_CurrentIndex;
		currentIndex++;
		if (currentIndex == m_ObjectList.Count)
		{
			currentIndex = 0;
		}
		while (true)
		{
			string text = m_ObjectList[currentIndex].ToString();
			if (!IsCaseSensitive)
			{
				text = text.ToLower();
			}
			switch (m_SearchMode)
			{
			case SearchMode.SearchExact:
				m_HasFound = text.ToString().Equals(m_Pattern);
				break;
			case SearchMode.SearchStarting:
				m_HasFound = text.ToString().StartsWith(m_Pattern);
				break;
			case SearchMode.SearchContaining:
				m_HasFound = text.Contains(m_Pattern);
				break;
			}
			if (m_HasFound)
			{
				m_FoundObject = m_ObjectList[currentIndex];
				m_CurrentIndex = currentIndex;
				if (CallBack != null)
				{
					CallBack(this, m_FoundObject);
				}
				return true;
			}
			if (currentIndex == m_CurrentIndex)
			{
				break;
			}
			currentIndex++;
			if (currentIndex == m_ObjectList.Count)
			{
				currentIndex = 0;
			}
		}
		m_FoundObject = null;
		CallBack(this, null);
		return false;
	}

	private void buttonCaseSensitive_Click(object sender, EventArgs e)
	{
		m_CaseSensitive = buttonCaseSensitive.Checked;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.SearchTool));
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.textBox = new System.Windows.Forms.ToolStripTextBox();
		this.buttonCaseSensitive = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchContain = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchStart = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchExact = new System.Windows.Forms.ToolStripButton();
		this.toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.buttonCaseSensitive, this.textBox, this.buttonSearchExact, this.buttonSearchStart, this.buttonSearchContain });
		this.toolStrip.Location = new System.Drawing.Point(0, 0);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.Size = new System.Drawing.Size(345, 25);
		this.toolStrip.TabIndex = 0;
		this.toolStrip.Text = "toolStrip";
		this.textBox.Name = "textBox";
		this.textBox.Size = new System.Drawing.Size(200, 25);
		this.textBox.ToolTipText = "Type the string to search";
		this.textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(textBox_KeyDown);
		this.buttonCaseSensitive.CheckOnClick = true;
		this.buttonCaseSensitive.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCaseSensitive.Image = (System.Drawing.Image)resources.GetObject("buttonCaseSensitive.Image");
		this.buttonCaseSensitive.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCaseSensitive.Name = "buttonCaseSensitive";
		this.buttonCaseSensitive.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.buttonCaseSensitive.Size = new System.Drawing.Size(23, 22);
		this.buttonCaseSensitive.ToolTipText = "case sensitive search";
		this.buttonCaseSensitive.Click += new System.EventHandler(buttonCaseSensitive_Click);
		this.buttonSearchContain.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchContain.Image = (System.Drawing.Image)resources.GetObject("buttonSearchContain.Image");
		this.buttonSearchContain.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchContain.Name = "buttonSearchContain";
		this.buttonSearchContain.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchContain.Text = "Search if contains";
		this.buttonSearchContain.Click += new System.EventHandler(buttonSearchContain_Click);
		this.buttonSearchStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchStart.Image = (System.Drawing.Image)resources.GetObject("buttonSearchStart.Image");
		this.buttonSearchStart.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchStart.Name = "buttonSearchStart";
		this.buttonSearchStart.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchStart.Text = "Search if starts with";
		this.buttonSearchStart.Click += new System.EventHandler(buttonSearchStart_Click);
		this.buttonSearchExact.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchExact.Image = (System.Drawing.Image)resources.GetObject("buttonSearchExact.Image");
		this.buttonSearchExact.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchExact.Name = "buttonSearchExact";
		this.buttonSearchExact.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchExact.Text = "Search exactly";
		this.buttonSearchExact.Click += new System.EventHandler(buttonSearchExact_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		base.Controls.Add(this.toolStrip);
		base.Name = "SearchTool";
		base.Size = new System.Drawing.Size(345, 25);
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
