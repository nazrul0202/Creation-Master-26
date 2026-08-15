using System.Windows.Controls;

namespace CM26.Studio.Views;

public partial class PlaceholderView : UserControl
{
    public PlaceholderView(string title)
    {
        InitializeComponent();
        TitleText.Text = title;
    }
}