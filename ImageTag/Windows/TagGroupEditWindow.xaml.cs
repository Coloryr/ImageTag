using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageTag.Windows;

/// <summary>
/// TagGroupEditWindow.xaml 的交互逻辑
/// </summary>
public partial class TagGroupEditWindow : Window
{
    private bool Cancel;
    public TagGroupEditWindow()
    {
        InitializeComponent();
    }

    public string? Set() 
    {
        ShowDialog();

        if (Cancel)
            return null;
        return TagGroupName.Text;
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        Cancel = true;
        Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Cancel = false;
        Close();
    }
}
