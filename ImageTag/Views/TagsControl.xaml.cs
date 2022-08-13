using Dapper;
using ImageTag.Sql;
using ImageTag.Train;
using ImageTag.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using static System.Net.Mime.MediaTypeNames;

namespace ImageTag.Views;

/// <summary>
/// TagsControl.xaml 的交互逻辑
/// </summary>
public partial class TagsControl : UserControl, IHighlight
{
    private TagGroupObj GroupObj;
    private List<TagObj> Tags;
    private Action<TagControl> AddTag;
    private readonly List<TagControl> Save = new();
    private bool IsEdit;
    public TagsControl(TagGroupObj group, Action<TagControl> call, bool edit)
    {
        InitializeComponent();

        GroupObj = group;
        Text.Content = group.name;
        AddTag = call;
        IsEdit = edit;

        if (edit == false) 
        {
            Text.Visibility = Visibility.Collapsed;
            EditS.Visibility = Visibility.Collapsed;
        }

        Refeash();
    }

    private void Refeash() 
    {
        Tags = TagSql.GetTags(GroupObj.uuid);
        List1.Children.Clear();
        Save.Clear();
        foreach (var item in Tags)
        {
            var tag = new TagControl(item, true, AddTag, IsEdit, "√", Edit);
            List1.Children.Add(tag);
            Save.Add(tag);
        }
    }

    public void Highlight(TagObj obj)
    {
        if (obj.group == GroupObj.uuid)
        {
            foreach (TagControl item1 in List1.Children)
            {
                item1.Opacity = 1.0d;
            }
        }

        var list = from item in Tags where item.bind_group == obj.@group && item.bind_uuid == obj.uuid select item.uuid;

        if (!list.Any())
        {
            return;
        }

        foreach (TagControl item in List1.Children)
        {
            Save.Add(item);
        }

        foreach (var item1 in Save)
        {
            if (list.Contains(item1.TagObj.uuid))
            {
                item1.Opacity = 1.0d;
            }
            else
            {
                item1.Opacity = 0.5d;
                List1.Children.Remove(item1);
                List1.Children.Add(item1);
            }
        }
    }

    public void ClearHighlight() 
    {
        foreach (TagControl item1 in List1.Children)
        {
            item1.Opacity = 1.0d;
        }
    }

    private void Edit(TagControl obj) 
    {
        var obj1 = new TagEditWindow(GroupObj, obj.TagObj).Set();
        TagSql.EditTag(obj1);
        Refeash();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var obj = new TagEditWindow(GroupObj).Set();
        if (obj == null || string.IsNullOrWhiteSpace(obj.name))
            return;

        if (TagSql.HaveTag(obj.group, obj.name))
        {
            new InfoWindow("新建标签失败", "已存在同名标签");
            return;
        }

        TagSql.AddTag(obj);
        Refeash();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        var obj = new TagRemoveWindow(GroupObj).Set();
        if (obj == null)
            return;

        TagSql.RemoveTag(obj.group, obj.uuid);
        Refeash();
    }

    private void Input_TextChanged(object sender, TextChangedEventArgs e)
    {
        string data = Input.Text;
        if (string.IsNullOrWhiteSpace(data))
        {
            List1.Children.Clear();
            foreach (var item in Save)
            {
                List1.Children.Add(item);
            }
            return;
        }

        var list = from item in Save where item.TagObj.name.Contains(data) select item;
        List1.Children.Clear();
        foreach (var item in list)
        {
            List1.Children.Add(item);
        }
    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
        new InfoWindow("提示", "标签内容不足10张图片的不会进行参与机器学习");

        if (!MLClassification.InitTrain(GroupObj.uuid))
        {
            new InfoWindow("错误", "样本数量不足，无法进行机器学习");
            return;
        }

        MLClassification.StartTrain(AutoTag.mlContext, GroupObj.uuid, AutoTag.ML + "temp/", AutoTag.ML + GroupObj.uuid + ".zip");
    }
}
