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
using System.Windows.Shapes;
using ImageTag.Sql;

namespace ImageTag.Windows;

/// <summary>
/// TagGroupRemoveWindow.xaml 的交互逻辑
/// </summary>
public partial class TagGroupRemoveWindow : Window
{
    private bool Cancel;
    public TagGroupRemoveWindow()
    {
        InitializeComponent();

        var groups = TagSql.GetAllGroup();
        foreach (var item in groups)
        {
            Group.Items.Add(item);
        }
    }

    public TagGroupObj? Set() 
    {
        ShowDialog();
        
        if (Cancel)
            return null;
        return Group.SelectedItem as TagGroupObj;
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
