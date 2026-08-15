using System.Windows;
using System.Windows.Controls;
using CM26.EngineBridge;

namespace CM26.Studio.Controls;

/// <summary>CM16-style named check row: a labelled checkbox bound to a numeric
/// 0/1 field (e.g. High Quality Face / hashighqualityhead).</summary>
public partial class NamedCheckField : UserControl
{
    private bool _loading;
    private bool _initial;
    private string _fieldName = "";

    public NamedCheckField()
    {
        InitializeComponent();
    }

    /// <summary>Called when the checkbox changes; returns the engine outcome.</summary>
    public Func<string, string, EditOutcome?>? Pending { get; set; }

    public void SetContent(string label, string fieldName, string? rawValue, bool isWritable,
        Func<string, string, EditOutcome?>? pending)
    {
        LabelText.Text = label;
        _fieldName = fieldName;
        Pending = pending;
        Check.IsEnabled = isWritable;
        _loading = true;
        _initial = rawValue is "1" or "true" or "True";
        Check.IsChecked = _initial;
        _loading = false;
    }

    private void Check_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading || Pending == null) return;
        var outcome = Pending(_fieldName, Check.IsChecked == true ? "1" : "0");
        if (outcome is { Success: false })
        {
            MessageBox.Show(outcome.Message, "Edit rejected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
