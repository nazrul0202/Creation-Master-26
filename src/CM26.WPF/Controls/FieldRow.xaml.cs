using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CM26.Application.Models;
using CM26.EngineBridge;

namespace CM26.Studio.Controls;

/// <summary>One editable field row (label + value box + modified marker), CM16 style.</summary>
public partial class FieldRow : UserControl
{
    public FieldRow()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PendingProperty =
        DependencyProperty.Register(nameof(Pending), typeof(Func<string, string, EditOutcome?>),
            typeof(FieldRow), new PropertyMetadata(null));

    /// <summary>Called when the user commits a field edit; returns the engine outcome.</summary>
    public Func<string, string, EditOutcome?> Pending
    {
        get => (Func<string, string, EditOutcome?>)GetValue(PendingProperty);
        set => SetValue(PendingProperty, value);
    }

    private void FieldBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is not FieldValue field) return;
        var outcome = Pending?.Invoke(field.FieldName, field.Value);
        if (outcome is { Success: false })
        {
            MessageBox.Show(outcome.Message, "Edit rejected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}

public sealed class ModifiedBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? new SolidColorBrush(Color.FromRgb(0xFF, 0xF6, 0xD6)) : Brushes.White;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}