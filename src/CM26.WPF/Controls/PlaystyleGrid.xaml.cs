using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CM26.EngineBridge;

namespace CM26.Studio.Controls;

/// <summary>
/// One playstyle flag: which field/bit owns it and the current on/off state.
/// </summary>
public sealed class PlaystyleFlag
{
    public required string Field { get; init; }
    public required int Bit { get; init; }
    public required string Name { get; init; }
    public required bool IsSet { get; init; }
    public required bool IsWritable { get; init; }
}

/// <summary>
/// CM16-style Traits checkbox grid: FC26 playstyles over trait1/trait2 (or the
/// icon "Virtual Pro" variant over icontrait1/icontrait2), 3 columns of checkboxes.
/// Bit 0-31 live in the first field, bit 32-33 in the second field.
/// </summary>
public partial class PlaystyleGrid : UserControl
{
    public PlaystyleGrid()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PlusVariantProperty =
        DependencyProperty.Register(nameof(PlusVariant), typeof(bool), typeof(PlaystyleGrid), new PropertyMetadata(false));
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(PlaystyleGrid), new PropertyMetadata(3, OnColumnsChanged));
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(System.Collections.IEnumerable), typeof(PlaystyleGrid),
            new PropertyMetadata(null, OnItemsChanged));
    public static readonly DependencyProperty ToggleProperty =
        DependencyProperty.Register(nameof(Toggle), typeof(Func<string, int, bool, EditOutcome?>),
            typeof(PlaystyleGrid), new PropertyMetadata(null));

    /// <summary>When true, labels get a "+" suffix (icon playstyles).</summary>
    public bool PlusVariant
    {
        get => (bool)GetValue(PlusVariantProperty);
        set => SetValue(PlusVariantProperty, value);
    }

    /// <summary>Number of checkbox columns (CM16 Traits uses 3; Virtual Pro uses 2).</summary>
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>The playstyle flags to render as checkboxes.</summary>
    public System.Collections.IEnumerable? Items
    {
        get => (System.Collections.IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Called when the user toggles a playstyle bit: (field, bit, set). The handler
    /// reads the current mask, sets/clears the bit and stages the new mask value.
    /// </summary>
    public Func<string, int, bool, EditOutcome?>? Toggle
    {
        get => (Func<string, int, bool, EditOutcome?>?)GetValue(ToggleProperty);
        set => SetValue(ToggleProperty, value);
    }

    /// <summary>True while rebuilding checkboxes (ignores toggle events).</summary>
    private bool _suppress;

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((PlaystyleGrid)d).Rebuild();

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((PlaystyleGrid)d).Grid.Columns = ((PlaystyleGrid)d).Columns;

    private void Rebuild()
    {
        _suppress = true;
        try
        {
            Grid.Children.Clear();
            if (Items == null) return;
            foreach (var item in Items.Cast<PlaystyleFlag>())
            {
                var box = new CheckBox
                {
                    Content = PlusVariant ? item.Name + "+" : item.Name,
                    IsChecked = item.IsSet,
                    IsEnabled = item.IsWritable,
                    Margin = new Thickness(2, 1, 2, 1),
                    Foreground = (Brush)FindResource("TextBrush"),
                    FontSize = 12.5,
                    Tag = item,
                };
                box.Click += Playstyle_Click;
                Grid.Children.Add(box);
            }
        }
        finally
        {
            _suppress = false;
        }
    }

    private void Playstyle_Click(object sender, RoutedEventArgs e)
    {
        if (_suppress || sender is not CheckBox box || box.Tag is not PlaystyleFlag flag) return;
        if (Toggle == null) return;
        var outcome = Toggle.Invoke(flag.Field, flag.Bit, box.IsChecked == true);
        if (outcome is { Success: false })
        {
            MessageBox.Show(outcome.Message, "Edit rejected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
