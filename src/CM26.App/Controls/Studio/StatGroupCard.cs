using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Card showing a named group of attributes as label + rating-badge rows.
/// </summary>
public sealed class StatGroupCard : StudioCard
{
    private readonly Label _title;
    private readonly FlowLayoutPanel _rows;

    public StatGroupCard()
    {
        Padding = new Padding(StudioSpacing.Medium);
        Width = 260;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        _title = new Label
        {
            Text = "Group",
            Dock = DockStyle.Top,
            Height = 22,
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, StudioSpacing.Small),
        };

        _rows = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
        };

        Controls.Add(_rows);
        Controls.Add(_title);
    }

    public string GroupTitle
    {
        get => _title.Text;
        set => _title.Text = value;
    }

    public void AddStat(string label, int value)
    {
        var row = new Panel
        {
            Height = 22,
            Width = _rows.ClientSize.Width > 0 ? _rows.ClientSize.Width : 220,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, StudioSpacing.Tiny),
        };

        var name = new Label
        {
            Text = label,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.DataLabel,
            AutoSize = true,
            Location = new Point(0, 3),
            BackColor = Color.Transparent,
        };

        var badge = new RatingBadge
        {
            Rating = value,
            Location = new Point(row.Width - 36, 0),
        };

        row.Controls.Add(badge);
        row.Controls.Add(name);
        _rows.Controls.Add(row);
    }

    public void Clear() => _rows.Controls.Clear();
}
