using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FifaLibrary;

namespace FifaControls;

public class PickUpControl : UserControl
{
	public delegate IdObject PickUpCallback(object sender, object obj);

	private enum SearchMode
	{
		SearchExact,
		SearchStarting,
		SearchContaining
	}

	private IdArrayList m_ObjectList;

	private bool m_MainSelectionEnabled = true;

	private bool m_FilterEnabled;

	private bool m_YoungPlayersEnabled;

	private bool m_SearchEnabled = true;

	private bool m_CreateButtonEnabled = true;

	private bool m_RemoveButtonEnabled = true;

	private bool m_CloneButtonEnabled = true;

	private bool m_RefreshButtonEnabled = true;

	private bool m_WizardButtonEnabled;

	private string[] m_FilterByList;

	private IdArrayList[] m_FilterValues;

	private int[] m_FilterIndex;

	[Category("User")]
	[Description("Handle selected object change.")]
	public PickUpCallback SelectObject;

	public PickUpCallback CreateObject;

	public PickUpCallback DeleteObject;

	public PickUpCallback CloneObject;

	public PickUpCallback FilterChanged;

	public PickUpCallback WizardObject;

	public PickUpCallback RefreshObject;

	private bool m_SwitchSema;

	private int m_CurrentObject = -1;

	private int m_CurrentFilterBy = -1;

	private object m_FoundObject;

	private bool m_HasFound;

	private bool m_CaseSensitive;

	private int m_CurrentSearchIndex;

	private string m_Pattern;

	private SearchMode m_SearchMode = SearchMode.SearchContaining;

	private IContainer components;

	private ToolStrip toolStrip;

	private ToolStripSeparator separatorBegin;

	private ToolStripSeparator separatorSearch;

	private ToolStripSeparator separatorButtons;

	public ToolStripComboBox combo;

	public ToolStripButton buttonCaseSensitive;

	public ToolStripTextBox textSearch;

	public ToolStripButton buttonSearchContain;

	public ToolStripButton buttonSearchExactly;

	public ToolStripButton buttonSearchStart;

	public ToolStripButton buttonNew;

	public ToolStripButton buttonRemove;

	public ToolStripButton buttonClone;

	public ToolStripLabel labelFilter;

	public ToolStripComboBox comboFilterBy;

	public ToolStripComboBox comboFilterValue;

	public ToolStripSeparator separatorFilter;

	private ToolStripButton buttonWizard;

	private ToolStripButton buttonRefresh;

	[Category("User")]
	[Description("Array List to show")]
	public IdArrayList ObjectList
	{
		get
		{
			return m_ObjectList;
		}
		set
		{
			_ = combo.SelectedItem;
			m_ObjectList = value;
			if (m_ObjectList != null)
			{
				FilterObjects();
			}
			else
			{
				combo.Items.Clear();
			}
		}
	}

	[Category("User")]
	[Description("Enable the main selection combo box")]
	public bool MainSelectionEnabled
	{
		get
		{
			return m_MainSelectionEnabled;
		}
		set
		{
			m_MainSelectionEnabled = value;
			combo.Visible = value;
			separatorBegin.Visible = value;
		}
	}

	[Category("User")]
	[Description("Enable the filter tools")]
	public bool FilterEnabled
	{
		get
		{
			return m_FilterEnabled;
		}
		set
		{
			m_FilterEnabled = value;
			labelFilter.Visible = value;
			comboFilterBy.Visible = value;
			comboFilterValue.Visible = value;
			separatorFilter.Visible = value;
		}
	}

	[Category("User")]
	[Description("Enable the Young Players chack")]
	public bool YoungPlayersEnabled
	{
		get
		{
			return m_YoungPlayersEnabled;
		}
		set
		{
			m_YoungPlayersEnabled = value;
		}
	}

	[Category("User")]
	[Description("Enable the search tools")]
	public bool SearchEnabled
	{
		get
		{
			return m_SearchEnabled;
		}
		set
		{
			m_SearchEnabled = value;
			buttonSearchContain.Visible = value;
			buttonSearchExactly.Visible = value;
			buttonSearchStart.Visible = value;
			textSearch.Visible = value;
			buttonCaseSensitive.Visible = value;
			separatorSearch.Visible = value;
		}
	}

	[Category("User")]
	[Description("Enable the create button")]
	public bool CreateButtonEnabled
	{
		get
		{
			return m_CreateButtonEnabled;
		}
		set
		{
			m_CreateButtonEnabled = value;
			buttonNew.Visible = value;
			separatorButtons.Visible = m_CreateButtonEnabled || m_RemoveButtonEnabled || m_CloneButtonEnabled;
		}
	}

	[Category("User")]
	[Description("Enable the create button")]
	public bool RemoveButtonEnabled
	{
		get
		{
			return m_RemoveButtonEnabled;
		}
		set
		{
			m_RemoveButtonEnabled = value;
			buttonRemove.Visible = value;
			separatorButtons.Visible = m_CreateButtonEnabled || m_RemoveButtonEnabled || m_CloneButtonEnabled;
		}
	}

	[Category("User")]
	[Description("Enable the create button")]
	public bool CloneButtonEnabled
	{
		get
		{
			return m_CloneButtonEnabled;
		}
		set
		{
			m_CloneButtonEnabled = value;
			buttonClone.Visible = value;
			separatorButtons.Visible = m_CreateButtonEnabled || m_RemoveButtonEnabled || m_CloneButtonEnabled;
		}
	}

	[Category("User")]
	[Description("Enable the refresh button")]
	public bool RefreshButtonEnabled
	{
		get
		{
			return m_RefreshButtonEnabled;
		}
		set
		{
			m_RefreshButtonEnabled = value;
			buttonRefresh.Visible = value;
		}
	}

	[Category("User")]
	[Description("Enable the wizard button")]
	public bool WizardButtonEnabled
	{
		get
		{
			return m_WizardButtonEnabled;
		}
		set
		{
			m_WizardButtonEnabled = value;
			buttonWizard.Visible = value;
			separatorButtons.Visible = m_CreateButtonEnabled || m_RemoveButtonEnabled || m_CloneButtonEnabled;
		}
	}

	[Category("User")]
	[Description("Filter by list")]
	public string[] FilterByList
	{
		get
		{
			return m_FilterByList;
		}
		set
		{
			m_FilterByList = value;
			comboFilterBy.Items.Clear();
			if (m_FilterByList != null)
			{
				ComboBox.ObjectCollection items = comboFilterBy.Items;
				object[] filterByList = m_FilterByList;
				items.AddRange(filterByList);
				comboFilterBy.SelectedIndex = 0;
				m_FilterIndex = new int[m_FilterByList.Length];
			}
		}
	}

	[Category("User")]
	[Description("Filter values")]
	public IdArrayList[] FilterValues
	{
		get
		{
			return m_FilterValues;
		}
		set
		{
			m_FilterValues = value;
		}
	}

	public object FoundObject => m_FoundObject;

	public bool HasFound => m_HasFound;

	public bool IsCaseSensitive => m_CaseSensitive;

	public int CurrentIndex
	{
		get
		{
			return m_CurrentSearchIndex;
		}
		set
		{
			m_CurrentSearchIndex = value;
		}
	}

	public PickUpControl()
	{
		InitializeComponent();
	}

	public void SwitchObject(IdObject idObject)
	{
		m_SwitchSema = true;
		int num = combo.Items.IndexOf(idObject);
		if (num >= 0)
		{
			combo.Items.RemoveAt(num);
			combo.Items.Insert(num, idObject);
			combo.SelectedItem = idObject;
		}
		m_SwitchSema = false;
	}

	private void combo_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_SwitchSema)
		{
			m_CurrentObject = combo.SelectedIndex;
			if (SelectObject != null && combo.SelectedItem != null)
			{
				Cursor.Current = Cursors.WaitCursor;
				SelectObject(sender, combo.SelectedItem);
				Cursor.Current = Cursors.Default;
			}
		}
	}

	private void buttonNew_Click(object sender, EventArgs e)
	{
		if (CreateObject != null)
		{
			IdObject idObject = CreateObject(sender, e);
			if (idObject != null)
			{
				combo.Items.Add(idObject);
				combo.SelectedItem = idObject;
			}
		}
	}

	private void buttonDelete_Click(object sender, EventArgs e)
	{
		IdObject idObject = (IdObject)combo.SelectedItem;
		IdObject idObject2 = null;
		int num = combo.Items.IndexOf(idObject);
		if (idObject == null)
		{
			return;
		}
		if (DeleteObject != null)
		{
			idObject2 = DeleteObject(sender, idObject);
		}
		if (idObject2 == null)
		{
			combo.Items.RemoveAt(num);
			if (num < combo.Items.Count)
			{
				combo.SelectedIndex = num;
			}
			else if (combo.Items.Count > 0)
			{
				combo.SelectedIndex = combo.Items.Count - 1;
			}
		}
	}

	private void buttonClone_Click(object sender, EventArgs e)
	{
		if (CloneObject == null)
		{
			return;
		}
		IdObject idObject = (IdObject)combo.SelectedItem;
		if (idObject != null)
		{
			IdObject idObject2 = CloneObject(sender, idObject);
			if (idObject2 != null)
			{
				combo.Items.Add(idObject2);
				combo.SelectedItem = idObject2;
			}
		}
	}

	private void buttonCaseSensitive_Click(object sender, EventArgs e)
	{
		m_CaseSensitive = buttonCaseSensitive.Checked;
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
		m_Pattern = textSearch.Text;
		if (!IsCaseSensitive)
		{
			m_Pattern = m_Pattern.ToLower();
		}
		int currentSearchIndex = m_CurrentSearchIndex;
		currentSearchIndex++;
		if (currentSearchIndex >= m_ObjectList.Count)
		{
			currentSearchIndex = 0;
			m_CurrentSearchIndex = 0;
		}
		while (true)
		{
			string text = m_ObjectList[currentSearchIndex].ToString();
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
				m_FoundObject = m_ObjectList[currentSearchIndex];
				m_CurrentSearchIndex = currentSearchIndex;
				combo.SelectedItem = m_FoundObject;
				return true;
			}
			if (currentSearchIndex == m_CurrentSearchIndex)
			{
				break;
			}
			currentSearchIndex++;
			if (currentSearchIndex == m_ObjectList.Count)
			{
				currentSearchIndex = 0;
			}
		}
		m_FoundObject = null;
		return false;
	}

	private void comboFilterBy_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboFilterBy.SelectedIndex < 0)
		{
			return;
		}
		if (comboFilterBy.SelectedIndex == 0)
		{
			comboFilterValue.Items.Clear();
			comboFilterValue.Enabled = false;
			combo.Items.Clear();
			if (FilterChanged != null)
			{
				FilterChanged(sender, null);
			}
			FilterObjects();
			m_CurrentFilterBy = 0;
			return;
		}
		int selectedIndex = comboFilterBy.SelectedIndex;
		comboFilterValue.Items.Clear();
		if (m_FilterValues[selectedIndex] != null)
		{
			comboFilterValue.Items.AddRange(m_FilterValues[selectedIndex].ToArray());
			comboFilterValue.Enabled = true;
			if (m_FilterIndex[selectedIndex] < comboFilterValue.Items.Count)
			{
				comboFilterValue.SelectedIndex = m_FilterIndex[selectedIndex];
			}
			else
			{
				m_FilterIndex[selectedIndex] = 0;
				comboFilterValue.SelectedIndex = m_FilterIndex[selectedIndex];
			}
		}
		if (FilterChanged != null)
		{
			FilterChanged(sender, comboFilterValue.SelectedItem);
		}
		m_CurrentFilterBy = comboFilterBy.SelectedIndex;
	}

	private void comboFilterValue_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = comboFilterBy.SelectedIndex;
		if (selectedIndex != m_CurrentFilterBy || (comboFilterValue.SelectedIndex >= 0 && comboFilterValue.SelectedIndex != m_FilterIndex[selectedIndex]))
		{
			m_FilterIndex[selectedIndex] = comboFilterValue.SelectedIndex;
			if (FilterChanged != null)
			{
				FilterChanged(sender, comboFilterValue.SelectedItem);
			}
			FilterObjects();
		}
	}

	private void FilterObjects()
	{
		if (m_ObjectList == null)
		{
			return;
		}
		object selectedItem = combo.SelectedItem;
		IdArrayList idArrayList = m_ObjectList.Filter((IdObject)comboFilterValue.SelectedItem);
		combo.BeginUpdate();
		combo.Items.Clear();
		combo.Items.AddRange(idArrayList.ToArray());
		combo.EndUpdate();
		if (selectedItem != null)
		{
			combo.SelectedItem = selectedItem;
		}
		if (combo.SelectedIndex < 0)
		{
			if (combo.Items.Count != 0)
			{
				combo.SelectedIndex = 0;
			}
			else
			{
				combo.Text = string.Empty;
			}
		}
	}

	private void buttonWizard_Click(object sender, EventArgs e)
	{
		if (WizardObject != null)
		{
			IdObject idObject = WizardObject(sender, e);
			if (idObject != null)
			{
				combo.Items.Add(idObject);
				combo.SelectedItem = idObject;
			}
		}
	}

	private void buttonRefresh_Click(object sender, EventArgs e)
	{
		if (RefreshObject != null)
		{
			RefreshObject(sender, e);
		}
	}

	private void buttonYoungPlayer_CheckedChanged(object sender, EventArgs e)
	{
		FilterObjects();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaControls.PickUpControl));
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.separatorBegin = new System.Windows.Forms.ToolStripSeparator();
		this.combo = new System.Windows.Forms.ToolStripComboBox();
		this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
		this.separatorSearch = new System.Windows.Forms.ToolStripSeparator();
		this.buttonCaseSensitive = new System.Windows.Forms.ToolStripButton();
		this.textSearch = new System.Windows.Forms.ToolStripTextBox();
		this.buttonSearchExactly = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchStart = new System.Windows.Forms.ToolStripButton();
		this.buttonSearchContain = new System.Windows.Forms.ToolStripButton();
		this.separatorButtons = new System.Windows.Forms.ToolStripSeparator();
		this.buttonNew = new System.Windows.Forms.ToolStripButton();
		this.buttonRemove = new System.Windows.Forms.ToolStripButton();
		this.buttonClone = new System.Windows.Forms.ToolStripButton();
		this.buttonWizard = new System.Windows.Forms.ToolStripButton();
		this.separatorFilter = new System.Windows.Forms.ToolStripSeparator();
		this.labelFilter = new System.Windows.Forms.ToolStripLabel();
		this.comboFilterBy = new System.Windows.Forms.ToolStripComboBox();
		this.comboFilterValue = new System.Windows.Forms.ToolStripComboBox();
		this.toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip.AllowItemReorder = true;
		this.toolStrip.BackColor = System.Drawing.SystemColors.Control;
		this.toolStrip.BackgroundImage = (System.Drawing.Image)resources.GetObject("toolStrip.BackgroundImage");
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[18]
		{
			this.separatorBegin, this.combo, this.buttonRefresh, this.separatorSearch, this.buttonCaseSensitive, this.textSearch, this.buttonSearchExactly, this.buttonSearchStart, this.buttonSearchContain, this.separatorButtons,
			this.buttonNew, this.buttonRemove, this.buttonClone, this.buttonWizard, this.separatorFilter, this.labelFilter, this.comboFilterBy, this.comboFilterValue
		});
		this.toolStrip.Location = new System.Drawing.Point(0, 0);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.Size = new System.Drawing.Size(1033, 25);
		this.toolStrip.TabIndex = 2;
		this.toolStrip.Text = "toolStrip";
		this.separatorBegin.Name = "separatorBegin";
		this.separatorBegin.Size = new System.Drawing.Size(6, 25);
		this.combo.DropDownHeight = 256;
		this.combo.IntegralHeight = false;
		this.combo.MaxDropDownItems = 16;
		this.combo.Name = "combo";
		this.combo.Size = new System.Drawing.Size(200, 25);
		this.combo.Sorted = true;
		this.combo.SelectedIndexChanged += new System.EventHandler(combo_SelectedIndexChanged);
		this.buttonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRefresh.Image = (System.Drawing.Image)resources.GetObject("buttonRefresh.Image");
		this.buttonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRefresh.Name = "buttonRefresh";
		this.buttonRefresh.Size = new System.Drawing.Size(23, 22);
		this.buttonRefresh.Text = "Refresh";
		this.buttonRefresh.Click += new System.EventHandler(buttonRefresh_Click);
		this.separatorSearch.Name = "separatorSearch";
		this.separatorSearch.Size = new System.Drawing.Size(6, 25);
		this.buttonCaseSensitive.CheckOnClick = true;
		this.buttonCaseSensitive.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonCaseSensitive.Image = (System.Drawing.Image)resources.GetObject("buttonCaseSensitive.Image");
		this.buttonCaseSensitive.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonCaseSensitive.Name = "buttonCaseSensitive";
		this.buttonCaseSensitive.Size = new System.Drawing.Size(23, 22);
		this.buttonCaseSensitive.Text = "Case sensitive";
		this.buttonCaseSensitive.Click += new System.EventHandler(buttonCaseSensitive_Click);
		this.textSearch.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.textSearch.Name = "textSearch";
		this.textSearch.Size = new System.Drawing.Size(150, 25);
		this.textSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(textBox_KeyDown);
		this.buttonSearchExactly.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchExactly.Image = (System.Drawing.Image)resources.GetObject("buttonSearchExactly.Image");
		this.buttonSearchExactly.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchExactly.Name = "buttonSearchExactly";
		this.buttonSearchExactly.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchExactly.Text = "Search Exactly";
		this.buttonSearchExactly.Click += new System.EventHandler(buttonSearchExact_Click);
		this.buttonSearchStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchStart.Image = (System.Drawing.Image)resources.GetObject("buttonSearchStart.Image");
		this.buttonSearchStart.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchStart.Name = "buttonSearchStart";
		this.buttonSearchStart.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchStart.Text = "Search if starts";
		this.buttonSearchStart.Click += new System.EventHandler(buttonSearchStart_Click);
		this.buttonSearchContain.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonSearchContain.Image = (System.Drawing.Image)resources.GetObject("buttonSearchContain.Image");
		this.buttonSearchContain.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSearchContain.Name = "buttonSearchContain";
		this.buttonSearchContain.Size = new System.Drawing.Size(23, 22);
		this.buttonSearchContain.Text = "Search if contains";
		this.buttonSearchContain.Click += new System.EventHandler(buttonSearchContain_Click);
		this.separatorButtons.Name = "separatorButtons";
		this.separatorButtons.Size = new System.Drawing.Size(6, 25);
		this.buttonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonNew.Image = (System.Drawing.Image)resources.GetObject("buttonNew.Image");
		this.buttonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonNew.Name = "buttonNew";
		this.buttonNew.Size = new System.Drawing.Size(23, 22);
		this.buttonNew.Text = "Create";
		this.buttonNew.Click += new System.EventHandler(buttonNew_Click);
		this.buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonRemove.Image = (System.Drawing.Image)resources.GetObject("buttonRemove.Image");
		this.buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRemove.Name = "buttonRemove";
		this.buttonRemove.Size = new System.Drawing.Size(23, 22);
		this.buttonRemove.Text = "Remove";
		this.buttonRemove.Click += new System.EventHandler(buttonDelete_Click);
		this.buttonClone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonClone.Image = (System.Drawing.Image)resources.GetObject("buttonClone.Image");
		this.buttonClone.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonClone.Name = "buttonClone";
		this.buttonClone.Size = new System.Drawing.Size(23, 22);
		this.buttonClone.Text = "Clone";
		this.buttonClone.Click += new System.EventHandler(buttonClone_Click);
		this.buttonWizard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buttonWizard.Image = (System.Drawing.Image)resources.GetObject("buttonWizard.Image");
		this.buttonWizard.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonWizard.Name = "buttonWizard";
		this.buttonWizard.Size = new System.Drawing.Size(23, 22);
		this.buttonWizard.Text = "Wizard";
		this.buttonWizard.Visible = false;
		this.buttonWizard.Click += new System.EventHandler(buttonWizard_Click);
		this.separatorFilter.Name = "separatorFilter";
		this.separatorFilter.Size = new System.Drawing.Size(6, 25);
		this.separatorFilter.Visible = false;
		this.labelFilter.Name = "labelFilter";
		this.labelFilter.Size = new System.Drawing.Size(33, 22);
		this.labelFilter.Text = "Filter";
		this.labelFilter.Visible = false;
		this.comboFilterBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboFilterBy.Name = "comboFilterBy";
		this.comboFilterBy.Size = new System.Drawing.Size(120, 25);
		this.comboFilterBy.Visible = false;
		this.comboFilterBy.SelectedIndexChanged += new System.EventHandler(comboFilterBy_SelectedIndexChanged);
		this.comboFilterValue.Name = "comboFilterValue";
		this.comboFilterValue.Size = new System.Drawing.Size(160, 25);
		this.comboFilterValue.Sorted = true;
		this.comboFilterValue.Visible = false;
		this.comboFilterValue.SelectedIndexChanged += new System.EventHandler(comboFilterValue_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Control;
		base.Controls.Add(this.toolStrip);
		base.Name = "PickUpControl";
		base.Size = new System.Drawing.Size(1033, 25);
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
