using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Grouped vertical navigation sidebar with active-item highlight,
/// icon + title + optional shortcut, and keyboard accessibility.
/// </summary>
public sealed class StudioSidebar : Panel
{
    private readonly Panel _header;
    private readonly Label _title;
    private readonly FlowLayoutPanel _groups;
    private readonly Dictionary<string, StudioSidebarItem> _items = new(StringComparer.OrdinalIgnoreCase);
    private string? _activeKey;

    public event EventHandler<StudioSidebarItemClickedEventArgs>? ItemClicked;

    public StudioSidebar()
    {
        DoubleBuffered = true;
        BackColor = StudioColors.Surface;
        ForeColor = StudioColors.PrimaryText;
        Width = 248;
        Dock = DockStyle.Left;
        Padding = new Padding(0);

        _header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = StudioColors.Surface,
            Padding = new Padding(StudioSpacing.Medium, 0, StudioSpacing.Medium, 0),
        };

        _title = new Label
        {
            Text = "CM26  /  STUDIO",
            Dock = DockStyle.Fill,
            ForeColor = StudioColors.CyanAccent,
            Font = StudioFonts.Button,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        _header.Controls.Add(_title);

        _groups = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = StudioColors.Surface,
            Padding = new Padding(StudioSpacing.Small, StudioSpacing.Small, StudioSpacing.Small, StudioSpacing.Medium),
        };
        _groups.Resize += (_, _) => ResizeGroupChildren();

        Controls.Add(_groups);
        Controls.Add(_header);
    }

    public string HeaderText
    {
        get => _title.Text;
        set => _title.Text = value;
    }

    public void AddGroup(string label, IEnumerable<StudioSidebarItemModel> items)
    {
        if (!string.IsNullOrWhiteSpace(label))
        {
            var groupLabel = new Label
            {
                Text = label.ToUpperInvariant(),
                AutoSize = false,
                Height = 22,
                Width = Math.Max(80, _groups.ClientSize.Width - StudioSpacing.Medium),
                ForeColor = StudioColors.MutedText,
                Font = StudioFonts.Metadata,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(StudioSpacing.Tiny, StudioSpacing.Large, 0, StudioSpacing.Tiny),
            };
            _groups.Controls.Add(groupLabel);
        }

        foreach (var item in items)
        {
            var button = new StudioSidebarItem(item)
            {
                Width = Math.Max(80, _groups.ClientSize.Width - StudioSpacing.Small),
                Margin = new Padding(0, 1, 0, 1),
            };
            button.Click += (_, _) => SetActive(item.Key);
            _groups.Controls.Add(button);
            _items[item.Key] = button;
        }
    }

    public void SetActive(string key)
    {
        if (!_items.TryGetValue(key, out var target)) return;

        _activeKey = key;
        foreach (var item in _items.Values)
            item.IsActive = false;
        target.IsActive = true;
        ItemClicked?.Invoke(this, new StudioSidebarItemClickedEventArgs(key));
    }

    public bool TrySetActive(string key) => _items.ContainsKey(key);

    private void ResizeGroupChildren()
    {
        var width = Math.Max(80, _groups.ClientSize.Width - StudioSpacing.Small);
        foreach (Control c in _groups.Controls)
        {
            if (c is StudioSidebarItem item)
                item.Width = width;
            else if (c is Label label && label.Text != HeaderText)
                label.Width = width;
        }
    }
}

public sealed class StudioSidebarItemClickedEventArgs : EventArgs
{
    public string Key { get; }
    public StudioSidebarItemClickedEventArgs(string key) => Key = key;
}

public sealed class StudioSidebarItemModel
{
    public string Key { get; }
    public string Title { get; }
    public Image? Icon { get; }
    public string? Shortcut { get; }

    public StudioSidebarItemModel(string key, string title, Image? icon = null, string? shortcut = null)
    {
        Key = key;
        Title = title;
        Icon = icon;
        Shortcut = shortcut;
    }
}
