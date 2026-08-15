using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace FifaLibrary;

public class Localization
{
	private string m_LanguageFileName;

	private string m_LanguageDefaultFileName;

	private ResXResourceSet m_ResxSet;

	public ResXResourceSet ResxSet => m_ResxSet;

	public Localization()
	{
		string currentDirectory = Environment.CurrentDirectory;
		string twoLetterISOLanguageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
		m_LanguageFileName = currentDirectory + "\\Language." + twoLetterISOLanguageName + ".resx";
		m_LanguageDefaultFileName = currentDirectory + "\\Language.resx";
		if (File.Exists(m_LanguageFileName))
		{
			m_ResxSet = new ResXResourceSet(m_LanguageFileName);
		}
		else if (File.Exists(m_LanguageFileName))
		{
			m_ResxSet = new ResXResourceSet(m_LanguageDefaultFileName);
		}
		else
		{
			m_ResxSet = null;
		}
	}

	public string GetString(string s)
	{
		if (m_ResxSet == null)
		{
			return s;
		}
		string name = "_" + s;
		return m_ResxSet.GetString(name);
	}

	public void LocalizeControl(Control control)
	{
		string text = GetString(control.Text);
		if (text != null)
		{
			control.Text = text;
		}
		if (control.GetType().Name == "ListView")
		{
			foreach (ColumnHeader column in ((ListView)control).Columns)
			{
				column.Text = GetString(column.Text);
			}
		}
		foreach (Control control2 in control.Controls)
		{
			LocalizeControl(control2);
		}
	}

	public void LocalizeMenu(MenuStrip menu)
	{
		foreach (ToolStripMenuItem item in menu.Items)
		{
			LocalizeMenuItem(item);
		}
	}

	public void LocalizeMenuItem(ToolStripMenuItem menuItem)
	{
		string text = GetString(menuItem.Text);
		if (text != null)
		{
			menuItem.Text = text;
		}
		foreach (ToolStripMenuItem dropDownItem in menuItem.DropDownItems)
		{
			LocalizeMenuItem(dropDownItem);
		}
	}

	public void LocalizeToolStrip(ToolStrip toolStrip)
	{
		foreach (ToolStripItem item in toolStrip.Items)
		{
			LocalizeToolStripItem(item);
		}
	}

	public void LocalizeToolStripItem(ToolStripItem toolStripItem)
	{
		string text = GetString(toolStripItem.Text);
		if (text != null)
		{
			toolStripItem.Text = text;
		}
		text = GetString(toolStripItem.ToolTipText);
		if (text != null)
		{
			toolStripItem.ToolTipText = text;
		}
		try
		{
			foreach (ToolStripItem dropDownItem in ((ToolStripDropDownItem)toolStripItem).DropDownItems)
			{
				LocalizeToolStripItem(dropDownItem);
			}
		}
		catch
		{
		}
	}
}
