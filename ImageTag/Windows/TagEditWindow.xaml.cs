using ImageTag.Sql;
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
using System.Windows.Shapes;

namespace ImageTag.Windows;

/// <summary>
/// TagEditWindow.xaml 的交互逻辑
/// </summary>
public partial class TagEditWindow : Window
{
    private TagObj Obj;
    private bool Cancel;
    public TagEditWindow(TagGroupObj group)
    {
        InitializeComponent();

        Obj = new()
        {
            group = group.uuid
        };

        Group.Items.Add(group);
        Group.SelectedItem = group;
        Group.IsEnabled = false;

        var groups = TagSql.GetAllGroup();
        BindGroup.Items.Add(new TagGroupObj
        {
            uuid = "",
            name = ""
        });
        foreach (var item in groups)
        {
            if (item.uuid == group.uuid)
                continue;
            BindGroup.Items.Add(item);
        }
    }

    public TagEditWindow(TagGroupObj group, TagObj obj)
    {
        InitializeComponent();

        Obj = obj;
        TagName.Text = obj.name;
        UUID.Text = obj.uuid;
        Group.Items.Add(group);
        Group.SelectedItem = group;
        Group.IsEnabled = false;

        var groups = TagSql.GetAllGroup();
        BindGroup.Items.Add(new TagGroupObj
        {
            uuid = "",
            name = ""
        });
        foreach (var item in groups)
        {
            if (item.uuid == group.uuid)
                continue;
            BindGroup.Items.Add(item);
        }
        var group1 = from item in groups where item.uuid == obj.bind_group select item;
        if (group1.Any())
        {
            BindGroup.SelectedItem = group1.First();
            var tags = TagSql.GetTags(group.uuid);
            foreach (TagObj item1 in BindTag.Items)
            {
                if (item1.uuid == obj.bind_uuid)
                {
                    BindTag.SelectedItem = item1;
                    break;
                }
            }
        }
    }

    public TagObj? Set() 
    {
        ShowDialog();

        if (Cancel)
            return null;

        Obj.name = TagName.Text;
        Obj.group = (Group.SelectedItem as TagGroupObj)?.uuid ?? "";

        Obj.bind_group = (BindGroup.SelectedItem as TagGroupObj)?.uuid ?? "";
        Obj.bind_uuid = (BindTag.SelectedItem as TagObj)?.uuid ?? "";

        return Obj;
    }

    private void BindGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var group = BindGroup.SelectedItem as TagGroupObj;
        if (group == null)
            return;

        BindTag.SelectedItem = null;
        BindTag.Items.Clear();
        var tags = TagSql.GetTags(group.uuid);
        foreach (var item in tags)
        {
            BindTag.Items.Add(item);
        }
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
