using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Empty state card with icon, title, description and optional action.
/// </summary>
public sealed class EmptyStateCard : StudioCard
{
    private readonly Label _icon;
    private readonly Label _title;
    private readonly Label _description;
    private readonly Button _action;

    public EmptyStateCard()
    {
        Padding = new Padding(StudioSpacing.XXLarge);

        _icon = new Label
        {
            AutoSize = true,
            Text = "🗂️",
            Font = new Font("Segoe UI Emoji", 32f, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = StudioColors.MutedText,
            Dock = DockStyle.Top,
            Height = 56,
            TextAlign = ContentAlignment.MiddleCenter,
        };

        _title = new Label
        {
            AutoSize = true,
            Text = "Nothing selected",
            Font = StudioFonts.CardTitle,
            ForeColor = StudioColors.PrimaryText,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, StudioSpacing.Medium, 0, StudioSpacing.Tiny),
        };

        _description = new Label
        {
            AutoSize = true,
            Text = "Select a record to view its details.",
            Font = StudioFonts.CardSubtitle,
            ForeColor = StudioColors.MutedText,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
        };

        _action = new Button
        {
            Text = "Open database",
            AutoSize = false,
            Height = 34,
            Width = 140,
            FlatStyle = FlatStyle.Flat,
            BackColor = StudioColors.CyanAccent,
            ForeColor = StudioColors.AppBackground,
            Font = StudioFonts.Button,
            Visible = false,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        _action.FlatAppearance.BorderSize = 0;
        _action.FlatAppearance.MouseOverBackColor = StudioColors.CyanAccentHover;

        var actionHost = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.Transparent,
            Padding = new Padding(0, StudioSpacing.Medium, 0, 0),
        };
        _action.Dock = DockStyle.None;
        _action.Location = new Point((actionHost.Width - _action.Width) / 2, 0);
        actionHost.Resize += (_, _) => _action.Location = new Point((actionHost.Width - _action.Width) / 2, 0);
        actionHost.Controls.Add(_action);

        Controls.Add(actionHost);
        Controls.Add(_description);
        Controls.Add(_title);
        Controls.Add(_icon);
    }

    public string IconText
    {
        get => _icon.Text;
        set => _icon.Text = value;
    }

    public string TitleText
    {
        get => _title.Text;
        set => _title.Text = value;
    }

    public string DescriptionText
    {
        get => _description.Text;
        set => _description.Text = value;
    }

    public string ActionText
    {
        get => _action.Text;
        set
        {
            _action.Text = value;
            _action.Visible = !string.IsNullOrWhiteSpace(value);
        }
    }

    public event EventHandler? ActionClicked
    {
        add => _action.Click += value;
        remove => _action.Click -= value;
    }
}
