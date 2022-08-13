using System.Windows;

namespace ImageTag.Windows;

/// <summary>
/// InfoWindow.xaml 的交互逻辑
/// </summary>
public partial class InfoWindow : Window
{
    public InfoWindow(string title, string text = null)
    {
        InitializeComponent();
        Title = title;
        Text.Text = text;
        ShowDialog();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
