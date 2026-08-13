using System.Drawing;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>Debounced search box with clear button and record-count label.</summary>
public sealed class SearchBar : UserControl
{
    private readonly TextBox _box;
    private readonly Button _clear;
    private readonly Label _count;
    private readonly System.Windows.Forms.Timer _debounce;
    private string _pending = string.Empty;

    public event EventHandler<string>? SearchChanged;

    public SearchBar()
    {
        Height = Theme.ControlHeight + 2;
        BackColor = Color.Transparent;

        _box = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Search…  (Ctrl+F)" };
        Theme.ApplyTextBox(_box);
        _clear = new Button { Text = "✕", Width = 30, Dock = DockStyle.Right, TabStop = false };
        Theme.ApplyButton(_clear);
        _count = new Label
        {
            Dock = DockStyle.Right,
            AutoSize = false,
            Width = 110,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Theme.Muted,
            Font = Theme.Muted9,
        };

        var pad = new BufferedPanel { Dock = DockStyle.Fill, Padding = new Padding(0) };
        pad.Controls.Add(_box);
        Controls.Add(pad);
        Controls.Add(_clear);
        Controls.Add(_count);

        _debounce = new System.Windows.Forms.Timer { Interval = 200 };
        _debounce.Tick += (_, _) =>
        {
            _debounce.Stop();
            SearchChanged?.Invoke(this, _pending);
        };
        _box.TextChanged += (_, _) =>
        {
            _pending = _box.Text.Trim();
            _debounce.Stop();
            _debounce.Start();
        };
        _clear.Click += (_, _) => { _box.Clear(); FocusBox(); };
    }

    public string Query => _box.Text.Trim();
    public void FocusBox() { _box.Focus(); _box.SelectAll(); }
    public void SetCount(int shown, int total) => _count.Text = total == shown ? $"{total:N0} records" : $"{shown:N0} / {total:N0}";
    public void ClearQuery() => _box.Clear();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _debounce?.Stop();
            _debounce?.Dispose();
        }
        base.Dispose(disposing);
    }
}
