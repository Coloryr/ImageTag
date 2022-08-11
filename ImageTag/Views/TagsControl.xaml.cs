using ImageTag.Sql;
using ImageTag.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
/// TagsControl.xaml 的交互逻辑
/// </summary>
public partial class TagsControl : UserControl
{
    private TagGroupObj GroupObj;
    private List<TagObj> Tags;
    public TagsControl(TagGroupObj group)
    {
        InitializeComponent();

        GroupObj = group;
        Text.Content = group.name;

        Task.Run(() =>
        {
            Tags = TagSql.GetTags(group.uuid);
            Dispatcher.Invoke(Refeash);
        });
    }

    private void Refeash() 
    {
        List1.Children.Clear();
        foreach (var item in Tags)
        {
            var tag = new TagControl(item, null, false);
            List1.Children.Add(tag);
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var name = new InputWindow("新建标签").Set();
        if (string.IsNullOrWhiteSpace(name))
            return;

        TagSql.AddTag();

    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {

    }
}
