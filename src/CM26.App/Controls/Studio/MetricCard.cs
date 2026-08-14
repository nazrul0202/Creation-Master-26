using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Metric card: label + large value + optional delta, with an accent tint.
/// </summary>
public sealed class MetricCard : StudioCard
{
    private readonly Label _value;
    private readonly Label _label;
    private readonly Label _delta;

    public MetricCard()
    {
        Padding = new Padding(StudioSpacing.Medium);
        Height = 76;

        _value = new Label
        {
            AutoSize = true,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.MetricValue,
            Location = new Point(StudioSpacing.Medium, StudioSpacing.Medium),
        };

        _delta = new Label
        {
            AutoSize = true,
            ForeColor = StudioColors.MutedText,
            Font = StudioFonts.DataLabel,
            Location = new Point(StudioSpacing.Medium, StudioSpacing.Medium + 34),
            Visible = false,
        };

        _label = new Label
        {
            AutoSize = true,
            ForeColor = StudioColors.MutedText,
            Font = StudioFonts.MetricLabel,
            Location = new Point(StudioSpacing.Medium, StudioSpacing.Medium + 48),
        };

        Controls.Add(_value);
        Controls.Add(_delta);
        Controls.Add(_label);
    }

    public string ValueText
    {
        get => _value.Text;
        set => _value.Text = value;
    }

    public string LabelText
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public string? DeltaText
    {
        get => _delta.Visible ? _delta.Text : null;
        set
        {
            _delta.Text = value ?? string.Empty;
            _delta.Visible = !string.IsNullOrWhiteSpace(value);
        }
    }

    public Color ValueColor
    {
        get => _value.ForeColor;
        set => _value.ForeColor = value;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (ShowAccentLine) return;

        var g = e.Graphics;
        using var brush = new LinearGradientBrush(
            new Rectangle(0, 0, Width, Height),
            Color.FromArgb(18, AccentColor),
            Color.Transparent,
            LinearGradientMode.Horizontal);
        g.FillRectangle(brush, new Rectangle(StudioSpacing.Medium, Height - 3, Width - StudioSpacing.Medium * 2, 3));
    }
}
