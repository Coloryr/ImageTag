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
/// TagEditWindow.xaml 的交互逻辑
/// </summary>
public partial class TagEditWindow : Window
{
    private TagObj Obj;
    public TagEditWindow(TagGroupObj group)
    {
        InitializeComponent();

        Obj = new()
        {
            group = group.uuid
        };

        Group.Items.Add(group);
        Group.SelectedItem = group;
        Group.IsReadOnly = true;

        var groups = TagSql.GetAllGroup();
        BindGroup.Items.Add(new TagGroupObj
        {
            uuid = null,
            name = ""
        });
        foreach (var item in groups)
        {
            if (item.uuid == group.uuid)
                continue;
            BindGroup.Items.Add(item);
        }
    }

    public TagEditWindow(TagObj obj)
    {
        Obj = obj;
    }

    public TagObj Set() 
    {
        ShowDialog();
        Obj.name = Name.Text;
        Obj.group = (Group.SelectedItem as TagGroupObj)?.uuid;

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
}
