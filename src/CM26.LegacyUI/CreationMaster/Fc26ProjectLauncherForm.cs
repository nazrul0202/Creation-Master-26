using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CreationMaster;

internal sealed class Fc26ProjectLauncherForm : Form
{
    internal Fc26ProjectLauncherForm(
        Action openInstalled, Action openExtracted, Action openSession, Action saveSession)
    {
        Text = "FC26 Project Launcher";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(760, 520);
        MinimumSize = new Size(700, 500);
        Icon = Form.ActiveForm?.Icon;

        var header = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = Color.FromArgb(5, 38, 82), Padding = new Padding(18, 12, 12, 8) };
        header.Controls.Add(new Label { Text = "CM26  •  DIRECT FC26 PROJECT LAUNCHER", ForeColor = Color.White, Font = new Font(Font.FontFamily, 15, FontStyle.Bold), AutoSize = true, Location = new Point(18, 13) });
        header.Controls.Add(new Label { Text = "Open the real Frostbite source, then edit through the classic Creation Master interface.", ForeColor = Color.WhiteSmoke, AutoSize = true, Location = new Point(20, 48) });

        var grid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 150, Padding = new Padding(14), ColumnCount = 2, RowCount = 2 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var row = 0; row < 2; row++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        grid.Controls.Add(ActionButton("Open FC26 Game", "CAS / TOC / SB → database and assets", openInstalled), 0, 0);
        grid.Controls.Add(ActionButton("Open extracted database", "Database folder with XML descriptor", openExtracted), 1, 0);
        grid.Controls.Add(ActionButton("Open CM26 project/session", "Reopen a saved direct-edit source", openSession), 0, 1);
        grid.Controls.Add(ActionButton("Save CM26 project/session", "Store source path; data remains direct", saveSession), 1, 1);

        var source = new GroupBox { Text = "Current source", Dock = DockStyle.Top, Height = 76, Padding = new Padding(12) };
        source.Controls.Add(new Label { Dock = DockStyle.Fill, Text = Fc26SnapshotLoader.DescribeLoadedSource(), AutoEllipsis = true });

        var recent = new GroupBox { Text = "Recent projects", Dock = DockStyle.Fill, Padding = new Padding(10) };
        var recentList = new ListBox { Dock = DockStyle.Fill };
        recentList.Items.AddRange(Fc26ProjectSessionService.Recent().Cast<object>().ToArray());
        recentList.DoubleClick += (_, _) =>
        {
            if (recentList.SelectedItem is string path && File.Exists(path))
            {
                Tag = path; DialogResult = DialogResult.Retry; Close();
            }
        };
        recent.Controls.Add(recentList);

        Controls.Add(recent); Controls.Add(source); Controls.Add(grid); Controls.Add(header);
    }

    private Button ActionButton(string title, string detail, Action action)
    {
        var button = new Button { Dock = DockStyle.Fill, Margin = new Padding(6), TextAlign = ContentAlignment.MiddleLeft,
            Text = "  " + title + "\r\n     " + detail, Font = new Font(Font.FontFamily, 9, FontStyle.Bold) };
        button.Click += (_, _) => { Close(); action(); };
        return button;
    }
}
