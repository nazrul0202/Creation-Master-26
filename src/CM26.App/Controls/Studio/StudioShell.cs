using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Main dark workspace surface. Hosts the application chrome and a single
/// content panel that sections fill.
/// </summary>
public sealed class StudioShell : Panel
{
    private readonly Panel _content;

    public StudioShell()
    {
        DoubleBuffered = true;
        BackColor = StudioColors.AppBackground;
        ForeColor = StudioColors.PrimaryText;
        Padding = new Padding(0);
        Margin = Padding.Empty;

        _content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudioColors.AppBackground,
            ForeColor = StudioColors.PrimaryText,
            Padding = new Padding(StudioSpacing.Medium),
        };
        base.Controls.Add(_content);
    }

    /// <summary>The inner content panel. Sections and dashboards are added here.</summary>
    public Panel Content => _content;
}
