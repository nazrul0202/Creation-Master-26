using System.Windows;
using System.Windows.Controls;
using CM26.EngineBridge;

namespace CM26.Studio.Controls;

/// <summary>CM16-style named combo row: a label + drop-down whose options map to
/// the underlying numeric field value (e.g. Hair Color "Dark Brown" = 3). Used on
/// the players Face tab where CM16 shows named combos instead of raw text boxes.</summary>
public partial class NamedComboField : UserControl
{
    private int[] _values = Array.Empty<int>();
    private bool _loading;
    private int _initialIndex = -1;
    private string _fieldName = "";

    public NamedComboField()
    {
        InitializeComponent();
    }

    /// <summary>Called when the user picks a named option; returns the engine outcome.</summary>
    public Func<string, string, EditOutcome?>? Pending { get; set; }

    /// <summary>Fills the combo with the given options, selecting the entry whose
    /// value matches the current field value. Unmatched values leave nothing selected
    /// (they are preserved untouched until the user picks a named option).</summary>
    public void SetContent(string label, string fieldName, IReadOnlyList<string> names, IReadOnlyList<int> values,
        string? rawValue, bool isWritable, Func<string, string, EditOutcome?>? pending)
    {
        LabelText.Text = label;
        _fieldName = fieldName;
        Pending = pending;
        Combo.IsEnabled = isWritable;
        _loading = true;
        Combo.Items.Clear();
        for (int i = 0; i < names.Count; i++)
        {
            Combo.Items.Add(names[i]);
        }
        _values = values.ToArray();
        _initialIndex = -1;
        if (int.TryParse(rawValue, out var value))
        {
            _initialIndex = Array.IndexOf(_values, value);
        }
        Combo.SelectedIndex = _initialIndex;
        ModifiedMark.Visibility = Visibility.Collapsed;
        _loading = false;
    }

    private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || Combo.SelectedIndex < 0) return;
        ModifiedMark.Visibility = Combo.SelectedIndex == _initialIndex ? Visibility.Collapsed : Visibility.Visible;
        if (Pending == null) return;
        var outcome = Pending(_fieldName, _values[Combo.SelectedIndex].ToString());
        if (outcome is { Success: false })
        {
            MessageBox.Show(outcome.Message, "Edit rejected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
