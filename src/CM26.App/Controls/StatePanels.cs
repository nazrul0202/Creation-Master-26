using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>Centered message panel used for empty / loading / error states.</summary>
public class StatePanel : BufferedPanel
{
    protected readonly Label _message;

    public StatePanel()
    {
        BackColor = Theme.Panel;
        _message = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Muted,
            Font = Theme.SectionTitle,
        };
        Controls.Add(_message);
    }

    public void Show(string message) { _message.Text = message; Visible = true; BringToFront(); }
    public new void Hide() { Visible = false; }
}

public sealed class EmptyStatePanel : StatePanel
{
    public EmptyStatePanel(string message = "Open a database to begin") { _message.Text = message; _message.ForeColor = Theme.Muted; }
}

public sealed class LoadingStatePanel : StatePanel
{
    public LoadingStatePanel() { _message.Text = "Loading…"; _message.ForeColor = Theme.Link; }
    public void SetMessage(string m) => _message.Text = m;
}

public sealed class ErrorStatePanel : StatePanel
{
    public ErrorStatePanel() { _message.ForeColor = Theme.Danger; }
    public void SetError(string m) => _message.Text = m;
}
