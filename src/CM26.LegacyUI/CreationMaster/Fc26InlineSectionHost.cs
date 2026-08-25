using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>
/// Hosts the implemented FC26 workflows inside the original CM26 section forms.
/// The section remains the owner of the UI; no separate tool window is opened.
/// </summary>
internal static class Fc26InlineSectionHost
{
	internal sealed class Module
	{
		internal string Title { get; }
		internal string Description { get; }
		internal Func<Form> Create { get; }

		internal Module(string title, string description, Func<Form> create)
		{
			Title = title;
			Description = description;
			Create = create;
		}
	}

	internal static Module Tab(string title, string description, Func<Form> create)
		=> new Module(title, description, create);

	internal static void LoadForSmoke(TabPage page) => EnsureLoaded(page);

	internal static TabControl Integrate(Form section, string editorTitle, params Module[] modules)
	{
		if (section == null || modules == null || modules.Length == 0) return null;
		section.SuspendLayout();
		try
		{
			var tabs = section.Controls.OfType<TabControl>().FirstOrDefault();
			if (tabs == null)
				tabs = CreateInlineSidePanel(section, editorTitle);

			foreach (var module in modules)
				AddModuleTab(tabs, module);
			return tabs;
		}
		finally { section.ResumeLayout(false); }
	}

	private static TabControl CreateInlineSidePanel(Form section, string editorTitle)
	{
		var tabs = new TabControl
		{
			Name = "cm26InlineSectionTabs",
			Dock = DockStyle.Right,
			Width = Math.Min(900, Math.Max(620, section.ClientSize.Width * 2 / 3)),
			HotTrack = true,
			Tag = editorTitle
		};
		section.Controls.Add(tabs);
		tabs.BringToFront();
		return tabs;
	}

	private static void AddModuleTab(TabControl tabs, Module module)
	{
		var page = new TabPage(module.Title)
		{
			Name = "cm26Inline_" + new string(module.Title.Where(char.IsLetterOrDigit).ToArray()),
			BackColor = SystemColors.Control,
			Tag = module
		};
		page.Controls.Add(new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleCenter,
			ForeColor = Color.FromArgb(25, 75, 120),
			Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 10f, FontStyle.Bold),
			Text = module.Description + "\r\n\r\nSelect this tab to load the integrated CM26 editor."
		});
		tabs.TabPages.Add(page);
		tabs.Selected += (_, _) => EnsureLoaded(tabs.SelectedTab);
	}

	private static void EnsureLoaded(TabPage page)
	{
		if (!(page?.Tag is Module module)) return;
		page.SuspendLayout();
		try
		{
			var embedded = module.Create();
			embedded.TopLevel = false;
			embedded.FormBorderStyle = FormBorderStyle.None;
			embedded.MinimumSize = Size.Empty;
			embedded.MaximumSize = Size.Empty;
			embedded.Dock = DockStyle.Fill;
			embedded.StartPosition = FormStartPosition.Manual;
			page.Controls.Clear();
			page.Controls.Add(embedded);
			page.Tag = embedded;
			embedded.Show();
		}
		catch (Exception ex)
		{
			page.Controls.Clear();
			page.Controls.Add(new Label
			{
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				ForeColor = Color.DarkRed,
				Text = "This CM26 section could not be loaded.\r\n\r\n" + ex.Message
			});
		}
		finally { page.ResumeLayout(true); }
	}
}
