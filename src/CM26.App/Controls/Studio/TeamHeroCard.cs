using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Hero card for a team: crest, name, league/nation, OVR and ATT/MID/DEF bars.
/// </summary>
public sealed class TeamHeroCard : StudioCard
{
    private readonly PictureBox _crest;
    private readonly Label _name;
    private readonly Label _leagueNation;
    private readonly Label _ovrValue;
    private readonly Label _ovrLabel;
    private readonly Panel _attBar;
    private readonly Panel _midBar;
    private readonly Panel _defBar;
    private readonly Label _attValue;
    private readonly Label _midValue;
    private readonly Label _defValue;
    private readonly Label _founded;
    private readonly Label _worth;
    private readonly Label _budgetLabel;
    private readonly NumericUpDown _budgetEditor;
    private bool _suppressBudgetEvent;

    public TeamHeroCard()
    {
        Dock = DockStyle.Top;
        Height = 150;
        Padding = new Padding(StudioSpacing.Large);
        AccentColor = StudioColors.CyanAccent;

        _crest = new PictureBox
        {
            Size = new Size(88, 88),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Location = new Point(StudioSpacing.Large, StudioSpacing.Large),
        };

        _name = new Label
        {
            Text = "Team Name",
            Location = new Point(112, StudioSpacing.Large),
            AutoSize = true,
            Font = StudioFonts.SectionTitle,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };

        _leagueNation = new Label
        {
            Text = "League · Nation",
            Location = new Point(112, 52),
            AutoSize = true,
            Font = StudioFonts.CardSubtitle,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        _ovrValue = new Label
        {
            Text = "—",
            AutoSize = true,
            Font = StudioFonts.MetricValue,
            ForeColor = StudioColors.Green,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };

        _ovrLabel = new Label
        {
            Text = "OVR",
            AutoSize = true,
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };

        _attBar = CreateBarTrack();
        _midBar = CreateBarTrack();
        _defBar = CreateBarTrack();

        _attValue = CreateBarValue();
        _midValue = CreateBarValue();
        _defValue = CreateBarValue();

        var barsTable = new TableLayoutPanel
        {
            Location = new Point(112, 82),
            Size = new Size(320, 52),
            ColumnCount = 3,
            RowCount = 3,
            BackColor = Color.Transparent,
        };
        barsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40f));
        barsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        barsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32f));
        for (var i = 0; i < 3; i++)
            barsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 17f));

        barsTable.Controls.Add(CreateBarLabel("ATT"), 0, 0);
        barsTable.Controls.Add(_attBar, 1, 0);
        barsTable.Controls.Add(_attValue, 2, 0);
        barsTable.Controls.Add(CreateBarLabel("MID"), 0, 1);
        barsTable.Controls.Add(_midBar, 1, 1);
        barsTable.Controls.Add(_midValue, 2, 1);
        barsTable.Controls.Add(CreateBarLabel("DEF"), 0, 2);
        barsTable.Controls.Add(_defBar, 1, 2);
        barsTable.Controls.Add(_defValue, 2, 2);

        _founded = new Label
        {
            Text = "Founded: —",
            Location = new Point(450, 82),
            AutoSize = true,
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        _worth = new Label
        {
            Text = "Worth: —",
            Location = new Point(450, 104),
            AutoSize = true,
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        _budgetLabel = new Label
        {
            Text = "Budget:",
            Location = new Point(450, 126),
            AutoSize = true,
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };

        _budgetEditor = new NumericUpDown
        {
            Location = new Point(500, 124),
            Size = new Size(120, 20),
            Font = StudioFonts.DataValue,
            ForeColor = StudioColors.PrimaryText,
            BackColor = StudioColors.InputBackground,
            BorderStyle = BorderStyle.FixedSingle,
            Minimum = 0,
            Maximum = 999999999,
            Increment = 1000000,
            ThousandsSeparator = true,
        };
        _budgetEditor.ValueChanged += (_, _) =>
        {
            if (!_suppressBudgetEvent) BudgetChanged?.Invoke(this, (long)_budgetEditor.Value);
        };

        Controls.Add(_budgetEditor);
        Controls.Add(_budgetLabel);
        Controls.Add(_worth);
        Controls.Add(_founded);
        Controls.Add(barsTable);
        Controls.Add(_ovrLabel);
        Controls.Add(_ovrValue);
        Controls.Add(_leagueNation);
        Controls.Add(_name);
        Controls.Add(_crest);

        Resize += (_, _) => Reposition();
        Reposition();
    }

    public Image? Crest
    {
        get => _crest.Image;
        set => _crest.Image = value;
    }

    public string TeamName
    {
        get => _name.Text;
        set => _name.Text = value;
    }

    public string LeagueNation
    {
        get => _leagueNation.Text;
        set => _leagueNation.Text = value;
    }

    public int Overall
    {
        get => int.TryParse(_ovrValue.Text, out var v) ? v : 0;
        set
        {
            _ovrValue.Text = value.ToString();
            _ovrValue.ForeColor = StudioColors.RatingColor(value);
        }
    }

    public int Attack
    {
        get => _attack;
        set { _attack = value; UpdateBar(_attBar, _attValue, value); }
    }

    public int Midfield
    {
        get => _midfield;
        set { _midfield = value; UpdateBar(_midBar, _midValue, value); }
    }

    public int Defence
    {
        get => _defence;
        set { _defence = value; UpdateBar(_defBar, _defValue, value); }
    }

    public string FoundedText
    {
        get => _founded.Text;
        set => _founded.Text = value;
    }

    public string WorthText
    {
        get => _worth.Text;
        set => _worth.Text = value;
    }

    public long TransferBudget
    {
        get => (long)_budgetEditor.Value;
        set
        {
            _suppressBudgetEvent = true;
            try { _budgetEditor.Value = Math.Clamp(value, 0, 999999999); }
            finally { _suppressBudgetEvent = false; }
        }
    }

    public string FinancialFieldLabel
    {
        get => _budgetLabel.Text.TrimEnd(':');
        set => _budgetLabel.Text = string.IsNullOrWhiteSpace(value) ? "Financial value:" : value.TrimEnd(':') + ":";
    }

    public bool FinancialEditorEnabled
    {
        get => _budgetEditor.Enabled;
        set => _budgetEditor.Enabled = value;
    }

    public event EventHandler<long>? BudgetChanged;

    private int _attack;
    private int _midfield;
    private int _defence;

    private static Panel CreateBarTrack()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.CardBorder,
            Margin = new Padding(0, 5, 0, 5),
        };
    }

    private static Label CreateBarValue()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = StudioFonts.DataValue,
            ForeColor = StudioColors.PrimaryText,
            BackColor = Color.Transparent,
        };
    }

    private static Label CreateBarLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = StudioFonts.DataLabel,
            ForeColor = StudioColors.MutedText,
            BackColor = Color.Transparent,
        };
    }

    private static void UpdateBar(Panel track, Label valueLabel, int value)
    {
        track.Controls.Clear();
        var fill = Math.Max(0, Math.Min(100, value));
        var fillPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = (int)(track.Width * fill / 100d),
            BackColor = StudioColors.RatingColor(value),
        };
        track.Controls.Add(fillPanel);
        valueLabel.Text = value.ToString();
        valueLabel.ForeColor = StudioColors.RatingColor(value);
        track.SizeChanged += (_, _) =>
        {
            if (track.Controls.Count > 0)
                track.Controls[0].Width = (int)(track.Width * fill / 100d);
        };
    }

    private void Reposition()
    {
        _ovrValue.Location = new Point(Width - 110, StudioSpacing.Large);
        _ovrLabel.Location = new Point(Width - 110, 56);
        _founded.Location = new Point(Math.Max(450, Width - 360), 82);
        _worth.Location = new Point(Math.Max(450, Width - 360), 104);
        _budgetLabel.Location = new Point(Math.Max(450, Width - 360), 126);
        _budgetEditor.Location = new Point(Math.Max(500, Width - 310), 124);
    }
}
