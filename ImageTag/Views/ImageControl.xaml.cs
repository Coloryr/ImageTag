using ImageTag.Sql;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
            ImageTags = new();
            Now = Need.First().Key;
            string local = $"{ImageSql.Local}{Now.local}";
            ImageShow.Source = ImageTagUtils.GetBitmapImage(local);
            var tag = AutoTag.PicDir(local);
            if (tag != null && !ImageTags.Contains(tag))
            {
                ImageTags.Add(tag);
                SelectTags.Children.Add(new TagControl(tag, true, RemoveTag));
            }
            foreach (var group in Groups)
            {
                AutoTag.PicML(local, group, AddTag);
            }
        }
        else
        {
            Now = null;
            ImageShow.Source = null;
            ImageTags = new();
        }
    }

    private void AddTag(TagObj obj)
    {
        if (Now != null)
        {
            if (ImageTags.Contains(obj))
                return;

            ImageTags.Add(obj);
            SelectTags.Children.Add(new TagControl(obj, true, RemoveTag));

            var tag = TagSql.GetTag(obj.bind_group, obj.bind_uuid);
            if (tag != null && !ImageTags.Contains(tag))
            {
                ImageTags.Add(tag);
                SelectTags.Children.Add(new TagControl(tag, true, RemoveTag));
            }
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

    private void PicLocal_Click(object sender, RoutedEventArgs e)
    {
        if (Now != null)
        {
            string local = $"{ImageSql.Local}{Now.local}".Replace("/", "\\");
            ProcessStartInfo psi = new("Explorer.exe")
            {
                Arguments = "/e,/select," + local
            };
            Process.Start(psi);
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (Now != null)
        {
            string local = $"{ImageSql.Local}{Now.local}".Replace("/", "\\");
            FileSystem.DeleteFile(System.IO.Path.GetFullPath(local), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            Need.Remove(Now);
            NextImage();
        }
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        if (Now != null)
        {
            Need.Remove(Now);
            NextImage();
        }
    }
}
