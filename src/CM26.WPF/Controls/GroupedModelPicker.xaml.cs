using System.Windows;
using System.Windows.Controls;
using CM26.EngineBridge;

namespace CM26.Studio.Controls;

/// <summary>
/// CM16's paired appearance controls: first choose a model family (for example
/// African or Afro), then a model id from that family. The family itself is UI
/// state; only the selected FC26 code is written to the database.
/// </summary>
public partial class GroupedModelPicker : UserControl
{
    private (string Name, int[] Values)[] _groups = [];
    private int _initialValue = int.MinValue;
    private string _fieldName = string.Empty;
    private bool _loading;

    public GroupedModelPicker() => InitializeComponent();

    public void SetContent(string label, string fieldName, IEnumerable<(string Name, int[] Values)> groups,
        string? rawValue, bool isWritable, Func<string, string, EditOutcome?>? pending)
    {
        LabelText.Text = label;
        _fieldName = fieldName;
        Pending = pending;
        _groups = groups.ToArray();
        _initialValue = int.TryParse(rawValue, out var value) ? value : int.MinValue;

        _loading = true;
        GroupCombo.IsEnabled = isWritable;
        ModelCombo.IsEnabled = isWritable;
        GroupCombo.ItemsSource = _groups.Select(group => group.Name).ToArray();
        var groupIndex = Array.FindIndex(_groups, group => Array.IndexOf(group.Values, _initialValue) >= 0);
        GroupCombo.SelectedIndex = groupIndex;
        if (groupIndex >= 0) PopulateModels(groupIndex, _initialValue);
        else ModelCombo.ItemsSource = null;
        ModifiedMark.Visibility = Visibility.Collapsed;
        _loading = false;
    }

    public Func<string, string, EditOutcome?>? Pending { get; set; }

    private void GroupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GroupCombo.SelectedIndex < 0) return;
        if (_loading)
        {
            PopulateModels(GroupCombo.SelectedIndex, _initialValue);
            return;
        }

        _loading = true;
        PopulateModels(GroupCombo.SelectedIndex, int.MinValue);
        _loading = false;
        if (ModelCombo.SelectedItem is int model) Stage(model);
    }

    private void ModelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loading && ModelCombo.SelectedItem is int model) Stage(model);
    }

    private void PopulateModels(int groupIndex, int preferredValue)
    {
        var values = _groups[groupIndex].Values;
        ModelCombo.ItemsSource = values;
        var modelIndex = Array.IndexOf(values, preferredValue);
        ModelCombo.SelectedIndex = modelIndex >= 0 ? modelIndex : (values.Length == 0 ? -1 : 0);
    }

    private void Stage(int value)
    {
        ModifiedMark.Visibility = value == _initialValue ? Visibility.Collapsed : Visibility.Visible;
        var outcome = Pending?.Invoke(_fieldName, value.ToString());
        if (outcome is { Success: false })
            MessageBox.Show(outcome.Message, "Edit rejected", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
