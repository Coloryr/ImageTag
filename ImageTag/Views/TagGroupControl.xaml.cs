using ImageTag.Sql;
using ImageTag.Windows;
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

namespace ImageTag.Views;

/// <summary>
/// TagGroupControl.xaml 的交互逻辑
/// </summary>
public partial class TagGroupControl : UserControl
{
    private readonly Action Reload;
    public TagGroupControl(Action reload)
    {
        InitializeComponent();
        Reload = reload;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var name = new TagGroupEditWindow().Set();

        if (string.IsNullOrWhiteSpace(name))
            return;

        TagSql.AddGroup(name);

        Reload();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        var group = new TagGroupRemoveWindow().Set();

        if (group == null)
            return;

        TagSql.RemoveGroup(group.uuid);

        Reload();
    }
}
