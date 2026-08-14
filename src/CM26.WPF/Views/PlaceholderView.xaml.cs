using System.Windows.Controls;

namespace CM26.Studio.Views;

public partial class PlaceholderView : UserControl
{
    public PlaceholderView(string key)
    {
        InitializeComponent();
        TitleText.Text = char.ToUpperInvariant(key[0]) + key[1..];
    }
}