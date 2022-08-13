using ImageTag.Sql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// ImageControl.xaml 的交互逻辑
/// </summary>
public partial class ImageControl : UserControl
{
    private List<TagGroupObj> Groups;
    private Dictionary<ImageObj, List<ImageTagObj>> Need;
    private ImageObj? Now;
    private List<TagObj> ImageTags;
    public ImageControl()
    {
        InitializeComponent();
        Reload();
        NextImage();
    }

    public void Reload() 
    {
        TagGroups.Children.Clear();
        Groups = TagSql.GetAllGroup();

        foreach (var item in Groups)
        {
            var view = new TagsControl(item, AddTag, true);
            TagGroups.Children.Add(view);
        }

        var con = new TagGroupControl(Reload);
        TagGroups.Children.Add(con);
    }

    private void NextImage() 
    {
        if (Need == null)
        {
            Need = (from item in ImageSql.ImageTags
                    where item.Value.Count == 0
                    select item).ToDictionary(a => a.Key, a => a.Value);
        }
        else if (Need.Count == 0)
        {
            ImageSql.Refresh();
            Need = (from item in ImageSql.ImageTags
                    where item.Value.Count == 0
                    select item).ToDictionary(a => a.Key, a => a.Value);
        }
        Info.Content = $"还有{Need.Count}张图片需要分类";
        SelectTags.Children.Clear();
        if (Need.Count != 0)
        {
            Now = Need.First().Key;
            string local = $"{ImageSql.Local}{Now.local}";
            BitmapImage bmp = new(new Uri(local));
            ImageShow.Source = bmp;
            ImageTags = AutoTag.CheckTag(local);
            foreach (var item in ImageTags)
            {
                SelectTags.Children.Add(new TagControl(item, true, RemoveTag));
            }
        }
        else
        {
            Now = null;
            ImageShow.Source = null;
            ImageTags = new();
        }
    }

    private void AddTag(TagControl obj) 
    {
        if (Now != null)
        {
            if (ImageTags.Contains(obj.TagObj))
                return;

            ImageTags.Add(obj.TagObj);
            SelectTags.Children.Add(new TagControl(obj.TagObj, true, RemoveTag));
            foreach (IHighlight item in TagGroups.Children)
            {
                item.Highlight(obj.TagObj);
            }

            var tag = TagSql.GetTag(obj.TagObj.bind_group, obj.TagObj.bind_uuid);
            if (tag != null && !ImageTags.Contains(tag))
            {
                ImageTags.Add(tag);
                SelectTags.Children.Add(new TagControl(tag, true, RemoveTag));
            }
        }
    }

    private void RemoveTag(TagControl obj) 
    {
        if (Now != null)
        {
            if (!ImageTags.Contains(obj.TagObj))
                return;
            ImageTags.Remove(obj.TagObj);
            SelectTags.Children.Remove(obj);
            foreach (IHighlight item in TagGroups.Children)
            {
                item.ClearHighlight();
            }
        }
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (Now != null)
        {
            ImageSql.SetImageTag(Now, ImageTags);
            Need.Remove(Now);
        }
        NextImage();
    }

    private void TagGroupsScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        TagGroups.Height = TagGroupsScrollViewer.ViewportHeight;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TagGroups.Height = TagGroupsScrollViewer.ViewportHeight;
    }
}
