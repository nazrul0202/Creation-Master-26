using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Hero card for a player: miniface, identity, OVR/GRO/POT and PAC/SHO/PAS/DRI/DEF/PHY metrics.
/// </summary>
public sealed class PlayerHeroCard : StudioCard
{
    private readonly PictureBox _miniface;
    private readonly Label _name;
    private readonly Label _teamNation;
    private readonly FlowLayoutPanel _positions;
    private readonly FlowLayoutPanel _topMetrics;
    private readonly FlowLayoutPanel _sixMetrics;

    public PlayerHeroCard()
    {
        Dock = DockStyle.Top;
        Height = 170;
        Padding = new Padding(StudioSpacing.Large);
        AccentColor = StudioColors.CyanAccent;

        _miniface = new PictureBox
        {
            Size = new Size(96, 96),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Location = new Point(StudioSpacing.Large, StudioSpacing.Large),
        };

        _name = new Label
        {
            Text = "Player Name",
            Location = new Point(120, StudioSpacing.Large),
            AutoSize = true,
            Font = StudioFonts.SectionTitle,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };

        _teamNation = new Label
        {
            Text = "Team · Nation",
            Location = new Point(120, 52),
            AutoSize = true,
            Font = StudioFonts.CardSubtitle,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        _positions = new FlowLayoutPanel
        {
            Location = new Point(120, 78),
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
        };

        _topMetrics = new FlowLayoutPanel
        {
            Location = new Point(Width - 280, StudioSpacing.Large),
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };

        _sixMetrics = new FlowLayoutPanel
        {
            Location = new Point(120, 112),
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
        };

        Controls.Add(_sixMetrics);
        Controls.Add(_topMetrics);
        Controls.Add(_positions);
        Controls.Add(_teamNation);
        Controls.Add(_name);
        Controls.Add(_miniface);

        Resize += (_, _) => Reposition();
    }

    public Image? Miniface
    {
        get => _miniface.Image;
        set => _miniface.Image = value;
    }

    public string PlayerName
    {
        get => _name.Text;
        set => _name.Text = value;
    }

    public string TeamNation
    {
        get => _teamNation.Text;
        set => _teamNation.Text = value;
    }

    public void SetPositions(IEnumerable<string> positions)
    {
        _positions.Controls.Clear();
        foreach (var p in positions)
            _positions.Controls.Add(new PositionChip { Position = p });
    }

    public void SetTopMetrics(IEnumerable<(string Label, int Value, Color Color)> metrics)
    {
        _topMetrics.Controls.Clear();
        foreach (var (label, value, color) in metrics)
        {
            _topMetrics.Controls.Add(new MetricCard
            {
                Width = 78,
                Height = 68,
                Margin = new Padding(0, 0, StudioSpacing.Small, 0),
                AccentColor = color,
                LabelText = label,
                ValueText = value.ToString(),
                ValueColor = color,
            });
        }
    }

    public void SetSixMetrics(IEnumerable<(string Label, int Value)> metrics)
    {
        _sixMetrics.Controls.Clear();
        foreach (var (label, value) in metrics)
        {
            _sixMetrics.Controls.Add(new MetricCard
            {
                Width = 92,
                Height = 58,
                Margin = new Padding(0, 0, StudioSpacing.Small, 0),
                AccentColor = StudioColors.CyanAccent,
                LabelText = label,
                ValueText = value.ToString(),
                ValueColor = StudioColors.PrimaryText,
            });
        }
    }

    private void Reposition()
    {
        _topMetrics.Location = new Point(Math.Max(320, Width - _topMetrics.Width - StudioSpacing.Large), StudioSpacing.Large);
    }
}
