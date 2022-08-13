using ImageTag.Sql;
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

namespace ImageTag.Windows;

/// <summary>
/// TagRemoveWindow.xaml 的交互逻辑
/// </summary>
public partial class TagRemoveWindow : Window
{
    private bool Cancel = false;
    public TagRemoveWindow(TagGroupObj group)
    {
        InitializeComponent();

        var list = TagSql.GetTags(group.uuid);
        GroupName.Text = group.name;
        foreach (var item in list)
        {
            BindTag.Items.Add(item);
        }
    }

    public TagObj? Set() 
    {
        ShowDialog();

        if (Cancel)
        {
            return null;
        }

        return BindTag.SelectedItem as TagObj;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Cancel = false;
        Close();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        Cancel = true;
        Close();
    }
}
