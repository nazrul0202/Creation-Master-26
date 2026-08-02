using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls;

internal static class CompdataCreationDialog
{
    public static bool TryShowLeague(IWin32Window owner, out CompdataLeagueBuildRequest request)
    {
        using var dialog = new Form { Text = "Build League / Cup Compdata", FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false, ShowInTaskbar = false,
            ClientSize = new Size(470, 244), Font = new Font("Segoe UI", 9F) };
        var name = Add(dialog, "Competition name", "New Competition", 16);
        var databaseId = Add(dialog, "Database Competition ID", "0", 50);
        var stages = Add(dialog, "Stages", "1", 84);
        var groups = Add(dialog, "Groups per stage", "1", 118);
        dialog.Controls.Add(new Label { Text = "Creates Compdata objects, standings and a schedule skeleton.\nCreate/link the database competition separately in the Competition editor.",
            Location = new Point(16, 150), Size = new Size(430, 42), ForeColor = SystemColors.GrayText });
        var create = new Button { Text = "Build", DialogResult = DialogResult.OK, Location = new Point(280, 204), Size = new Size(78, 28) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(366, 204), Size = new Size(78, 28) };
        dialog.Controls.Add(create); dialog.Controls.Add(cancel); dialog.AcceptButton = create; dialog.CancelButton = cancel;
        if (dialog.ShowDialog(owner) != DialogResult.OK || !int.TryParse(databaseId.Text, out var dbId) ||
            !int.TryParse(stages.Text, out var stageCount) || !int.TryParse(groups.Text, out var groupCount) || string.IsNullOrWhiteSpace(name.Text))
        { request = default!; return false; }
        request = new CompdataLeagueBuildRequest(name.Text.Trim(), dbId, stageCount, groupCount);
        return true;
    }

    public static bool TryShowAdvancement(IWin32Window owner, out (int Source, int SourceRank, int Destination, int DestinationRank) link)
    {
        using var dialog = new Form { Text = "Add Promotion / Relegation", FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false, ShowInTaskbar = false,
            ClientSize = new Size(430, 210), Font = new Font("Segoe UI", 9F) };
        var source = Add(dialog, "Source group ID", "", 16); var rank = Add(dialog, "Source rank", "0", 50);
        var destination = Add(dialog, "Destination group ID", "", 84); var destinationRank = Add(dialog, "Destination rank", "0", 118);
        var create = new Button { Text = "Add Link", DialogResult = DialogResult.OK, Location = new Point(240, 166), Size = new Size(84, 28) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(332, 166), Size = new Size(84, 28) };
        dialog.Controls.Add(create); dialog.Controls.Add(cancel); dialog.AcceptButton = create; dialog.CancelButton = cancel;
        if (dialog.ShowDialog(owner) != DialogResult.OK || !int.TryParse(source.Text, out var sourceId) ||
            !int.TryParse(rank.Text, out var sourceRank) || !int.TryParse(destination.Text, out var destinationId) ||
            !int.TryParse(destinationRank.Text, out var targetRank)) { link = default; return false; }
        link = (sourceId, sourceRank, destinationId, targetRank); return true;
    }

    private static TextBox Add(Form dialog, string label, string initial, int y)
    {
        dialog.Controls.Add(new Label { Text = label, Location = new Point(16, y + 4), Size = new Size(170, 22) });
        var box = new TextBox { Text = initial, Location = new Point(192, y), Size = new Size(222, 24) }; dialog.Controls.Add(box); return box;
    }
}
