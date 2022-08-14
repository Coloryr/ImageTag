using System.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageTag.Windows;
using ImageTag.Sql;
using static System.Formats.Asn1.AsnWriter;

namespace ImageTag;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _ = new InfoWindow("导入图片", "导入图片会删除原有的图片");
        SelectFile();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        Tabs.SelectedIndex = 2;
    }

    public const string IMAGEFILTER = "图片文件(*.jpg;*.png)|*.jpg;*.png";

    public void SelectFile()
    {
        OpenFileDialog dialog = new()
        {
            Multiselect = true,
            Filter = IMAGEFILTER
        };
        bool error = false;
        if (dialog.ShowDialog() == true)
        {
            Tabs.IsEnabled = false;
            Tabs.Opacity = 0.2;
            Score1.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                foreach (var item in dialog.FileNames)
                {
                    try
                    {
                        FileInfo info = new(item);
                        File.Move(item, ImageSql.Local + info.Name);
                    }
                    catch
                    {
                        error = true;
                    }
                }
                ImageSql.Refresh();
                Dispatcher.Invoke(() =>
                {
                    Tabs.IsEnabled = true;
                    Tabs.Opacity = 1;
                    Score1.Visibility = Visibility.Collapsed;
                });
            });
        }
        if (error)
        {
            _ = new InfoWindow("导入图片错误", "有部分图片导入错误");
        }
    }
}
