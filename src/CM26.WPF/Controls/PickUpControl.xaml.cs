using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CM26.Application.Models;

namespace CM26.Studio.Controls;

/// <summary>
/// WPF port of the CM16 FifaControls.PickUpControl: a 25px toolstrip with a
/// main selection combo, refresh, case-sensitive search (exact / starting /
/// containing with wrap-around), create / remove / clone / wizard buttons and
/// an optional filter (by / value) pair. Wire the callbacks the same way CM16
/// forms do (e.g. pickUpControl.SelectObject = SelectFormation; ...).
/// </summary>
public partial class PickUpControl : UserControl
{
    private enum SearchMode
    {
        SearchExact,
        SearchStarting,
        SearchContaining,
    }

    private IReadOnlyList<RecordListItem> _objectList = Array.Empty<RecordListItem>();
    private int _currentObject = -1;
    private int _currentFilterBy = -1;
    private int _currentSearchIndex;
    private bool _caseSensitive;
    private string _pattern = string.Empty;
    private SearchMode _searchMode = SearchMode.SearchContaining;

    public PickUpControl()
    {
        InitializeComponent();
    }

    // ---------- CM16 PickUpCallback delegates ----------

    /// <summary>Raised when the user picks an item in the combo (CM16 SelectObject).</summary>
    public event Action<RecordListItem>? SelectObject;

    /// <summary>Called by the Create button; return the new item to add and select (CM16 CreateObject).</summary>
    public Func<RecordListItem>? CreateObject;

    /// <summary>Called by the Remove button; return null to delete the item, or a replacement (CM16 DeleteObject).</summary>
    public Func<RecordListItem, RecordListItem?>? DeleteObject;

    /// <summary>Called by the Clone button; return the clone to add and select (CM16 CloneObject).</summary>
    public Func<RecordListItem, RecordListItem?>? CloneObject;

    /// <summary>Raised by the Refresh button (CM16 RefreshObject).</summary>
    public event Action? RefreshObject;

    /// <summary>Raised when the filter combos change (CM16 FilterChanged).</summary>
    public event Action? FilterChanged;

    /// <summary>Raised by the Wizard button (CM16 WizardObject).</summary>
    public event Action? WizardObject;

    // ---------- CM16 flags ----------

    public static readonly DependencyProperty MainSelectionEnabledProperty =
        DependencyProperty.Register(nameof(MainSelectionEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(true));

    public static readonly DependencyProperty FilterEnabledProperty =
        DependencyProperty.Register(nameof(FilterEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SearchEnabledProperty =
        DependencyProperty.Register(nameof(SearchEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CreateButtonEnabledProperty =
        DependencyProperty.Register(nameof(CreateButtonEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty RemoveButtonEnabledProperty =
        DependencyProperty.Register(nameof(RemoveButtonEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CloneButtonEnabledProperty =
        DependencyProperty.Register(nameof(CloneButtonEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty RefreshButtonEnabledProperty =
        DependencyProperty.Register(nameof(RefreshButtonEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty WizardButtonEnabledProperty =
        DependencyProperty.Register(nameof(WizardButtonEnabled), typeof(bool), typeof(PickUpControl),
            new PropertyMetadata(false));

    public bool MainSelectionEnabled { get => (bool)GetValue(MainSelectionEnabledProperty); set => SetValue(MainSelectionEnabledProperty, value); }
    public bool FilterEnabled { get => (bool)GetValue(FilterEnabledProperty); set => SetValue(FilterEnabledProperty, value); }
    public bool SearchEnabled { get => (bool)GetValue(SearchEnabledProperty); set => SetValue(SearchEnabledProperty, value); }
    public bool CreateButtonEnabled { get => (bool)GetValue(CreateButtonEnabledProperty); set => SetValue(CreateButtonEnabledProperty, value); }
    public bool RemoveButtonEnabled { get => (bool)GetValue(RemoveButtonEnabledProperty); set => SetValue(RemoveButtonEnabledProperty, value); }
    public bool CloneButtonEnabled { get => (bool)GetValue(CloneButtonEnabledProperty); set => SetValue(CloneButtonEnabledProperty, value); }
    public bool RefreshButtonEnabled { get => (bool)GetValue(RefreshButtonEnabledProperty); set => SetValue(RefreshButtonEnabledProperty, value); }
    public bool WizardButtonEnabled { get => (bool)GetValue(WizardButtonEnabledProperty); set => SetValue(WizardButtonEnabledProperty, value); }

    public bool IsCaseSensitive => _caseSensitive;

    public int CurrentIndex
    {
        get => _currentSearchIndex;
        set => _currentSearchIndex = value;
    }

    public object? SelectedItem => Combo.SelectedItem;

    public int SelectedIndex
    {
        get => Combo.SelectedIndex;
        set
        {
            if (value >= 0 && value < Combo.Items.Count) Combo.SelectedIndex = value;
        }
    }

    /// <summary>CM16 ObjectList: replaces the combo contents (filtered by the current filter).</summary>
    public IReadOnlyList<RecordListItem> ObjectList
    {
        get => _objectList;
        set
        {
            _objectList = value ?? Array.Empty<RecordListItem>();
            FilterObjects();
        }
    }

    /// <summary>CM16 FilterByList: names shown in the "Filter by" combo.</summary>
    public string[] FilterByList
    {
        set
        {
            FilterByCombo.Items.Clear();
            if (value != null)
            {
                foreach (var name in value) FilterByCombo.Items.Add(name);
                if (FilterByCombo.Items.Count > 0) FilterByCombo.SelectedIndex = 0;
            }
        }
    }

    /// <summary>Current filter value selection text (CM16 comboFilterValue).</summary>
    public string FilterValueText => FilterValueCombo.Text;

    /// <summary>Current "filter by" selection text (CM16 comboFilterBy).</summary>
    public string FilterByComboText => FilterByCombo.SelectedItem?.ToString() ?? string.Empty;

    private void FilterObjects()
    {
        Combo.Items.Clear();
        var list = _objectList.Where(x => MatchesFilter(x)).ToList();
        foreach (var item in list) Combo.Items.Add(item);
        if (Combo.Items.Count > 0) Combo.SelectedIndex = 0;
    }

    private bool MatchesFilter(RecordListItem item)
    {
        if (!FilterEnabled || _currentFilterBy < 0) return true;
        var filterBy = FilterByCombo.SelectedItem?.ToString() ?? string.Empty;
        if (filterBy.Length == 0 || FilterValueCombo.SelectedItem is not string value || value.Length == 0) return true;
        var text = filterBy switch
        {
            "Name" => item.Title,
            "Id" => item.Detail,
            _ => item.Title + " " + item.Detail + " " + item.Subtitle,
        };
        return text.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private void FilterBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _currentFilterBy = FilterByCombo.SelectedIndex;
        if (FilterByCombo.SelectedItem is not string by) return;
        FilterValueCombo.Items.Clear();
        var values = _objectList
            .Select(x => by switch
            {
                "Name" => x.Title,
                "Id" => x.Detail,
                _ => x.Subtitle,
            })
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .OrderBy(v => v)
            .ToList();
        foreach (var v in values) FilterValueCombo.Items.Add(v);
        FilterChanged?.Invoke();
    }

    private void FilterValue_SelectionChanged(object sender, SelectionChangedEventArgs e) => FilterChanged?.Invoke();

    private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Combo.SelectedItem is RecordListItem item)
        {
            _currentObject = Combo.SelectedIndex;
            SelectObject?.Invoke(item);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshObject?.Invoke();

    private void CaseSensitive_Click(object sender, RoutedEventArgs e)
        => _caseSensitive = CaseSensitiveButton.IsChecked == true;

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Search();
    }

    private void SearchExactly_Click(object sender, RoutedEventArgs e)
    {
        _searchMode = SearchMode.SearchExact;
        Search();
    }

    private void SearchStart_Click(object sender, RoutedEventArgs e)
    {
        _searchMode = SearchMode.SearchStarting;
        Search();
    }

    private void SearchContain_Click(object sender, RoutedEventArgs e)
    {
        _searchMode = SearchMode.SearchContaining;
        Search();
    }

    /// <summary>CM16 Search(): wrap-around search over the object list; selects the first match.</summary>
    public bool Search()
    {
        if (_objectList.Count == 0) return false;
        _pattern = SearchBox.Text;
        if (!_caseSensitive) _pattern = _pattern.ToLowerInvariant();
        if (_pattern.Length == 0) return false;

        var index = _currentSearchIndex + 1;
        if (index >= _objectList.Count) index = 0;
        var start = index;
        while (true)
        {
            var text = _objectList[index].Title;
            if (!_caseSensitive) text = text.ToLowerInvariant();
            var found = _searchMode switch
            {
                SearchMode.SearchExact => text.Equals(_pattern),
                SearchMode.SearchStarting => text.StartsWith(_pattern, StringComparison.Ordinal),
                _ => text.Contains(_pattern, StringComparison.Ordinal),
            };
            if (found)
            {
                _currentSearchIndex = index;
                Combo.SelectedIndex = index;
                return true;
            }
            index++;
            if (index >= _objectList.Count) index = 0;
            if (index == start) break;
        }
        return false;
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        var created = CreateObject?.Invoke();
        if (created != null)
        {
            ObjectList = _objectList.Append(created).ToArray();
            Combo.SelectedItem = created;
        }
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (Combo.SelectedItem is not RecordListItem item) return;
        var index = Combo.Items.IndexOf(item);
        var replacement = DeleteObject?.Invoke(item);
        if (replacement != null)
        {
            ObjectList = _objectList.Select(x => ReferenceEquals(x, item) ? replacement : x).ToArray();
            Combo.SelectedItem = replacement;
            return;
        }
        var list = _objectList.Where(x => !ReferenceEquals(x, item)).ToList();
        ObjectList = list;
        if (index < Combo.Items.Count) Combo.SelectedIndex = index;
        else if (Combo.Items.Count > 0) Combo.SelectedIndex = Combo.Items.Count - 1;
    }

    private void Clone_Click(object sender, RoutedEventArgs e)
    {
        if (Combo.SelectedItem is not RecordListItem item) return;
        var clone = CloneObject?.Invoke(item);
        if (clone != null)
        {
            ObjectList = _objectList.Append(clone).ToArray();
            Combo.SelectedItem = clone;
        }
    }

    private void Wizard_Click(object sender, RoutedEventArgs e) => WizardObject?.Invoke();
}
