using ImageTag.Sql;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
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
/// SelectImageControl.xaml 的交互逻辑
/// </summary>
public partial class SelectImageControl : UserControl
{
    private List<TagGroupObj> Groups;
    private readonly List<TagObj> ImageTags = new();
    private Dictionary<ImageObj, List<ImageTagObj>> Images;
    public SelectImageControl()
    {
        InitializeComponent();
    }

    private void MenuItem_Click1(object sender, RoutedEventArgs e)
    {
        var item = ImageList.SelectedItem as ImageObj;
        if (item == null)
            return;

        string local = $"{ImageSql.Local}{item.local}".Replace("\\", "/");
        Clipboard.SetText(local);
    }
    private void MenuItem_Click2(object sender, RoutedEventArgs e)
    {
        var item = ImageList.SelectedItem as ImageObj;
        if (item == null)
            return;

        string local = $"{ImageSql.Local}{item.local}".Replace("\\", "/");
        var file = new StringCollection
        {
            local
        };
        Clipboard.SetFileDropList(file);
    }
    private void MenuItem_Click3(object sender, RoutedEventArgs e)
    {
        var item = ImageList.SelectedItem as ImageObj;
        if (item == null)
            return;

        string local = $"{ImageSql.Local}{item.local}".Replace("\\", "/");
        BitmapImage bmp = new(new Uri(local));
        Clipboard.SetImage(bmp);
    }
    private void MenuItem_Click4(object sender, RoutedEventArgs e)
    {
        var item = ImageList.SelectedItem as ImageObj;
        if (item == null)
            return;

        string local = $"{ImageSql.Local}{item.local}".Replace("\\", "/");
        Process process = new();
        process.StartInfo.FileName = local;  
        process.StartInfo.Arguments = "rundl132.exe C://WINDOWS//system32//shimgvw.dll,ImageView_Fullscreen"; 
        process.StartInfo.UseShellExecute = true;  
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.Start(); 
    }
    private void MenuItem_Click5(object sender, RoutedEventArgs e)
    {
        var item = ImageList.SelectedItem as ImageObj;
        if (item == null)
            return;

        ImageSql.RemoveImageTag(item.uuid);
        Update();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ImageTags.Clear();
        SelectTags.Children.Clear();
        TagGroups.Children.Clear();
        Groups = TagSql.GetAllGroup();

        foreach (var item in Groups)
        {
            Expander expander = new()
            {
                IsExpanded = false,
                Header = item.name,
                Content = new TagsControl(item, AddTag, false),
                Margin = new(2, 2, 2, 0)
            };
            expander.SetValue(StyleProperty, Application.Current.Resources["ExpanderStyle"]);
            TagGroups.Children.Add(expander);
        }
    }

    private void AddTag(TagControl obj)
    {
        if (ImageTags.Contains(obj.TagObj))
            return;
        ImageTags.Add(obj.TagObj);
        SelectTags.Children.Add(new TagControl(obj.TagObj, true, RemoveTag));
        Update();
    }

    private void RemoveTag(TagControl obj)
    {
        if (!ImageTags.Contains(obj.TagObj))
            return;
        ImageTags.Remove(obj.TagObj);
        SelectTags.Children.Remove(obj);
        Update();
    }

    private void Update()
    {
        ImageShow.Source = null;
        ImageList.Items.Clear();
        NowTags.Children.Clear();

        if (!ImageTags.Any())
            return;

        Images = ImageSql.GetAllTags();
        var temp = new Dictionary<ImageObj, List<ImageTagObj>>(Images);
        foreach (var item in ImageTags)
        {
            temp = temp.Where(a =>
            {
                foreach (var item1 in a.Value)
                {
                    if (item1.tag_group == item.group && item1.tag_uuid == item.uuid)
                        return true;
                }
                return false;
            }).Select(a => a).ToDictionary(a => a.Key, a => a.Value);
        }

        foreach (var item in temp.Keys)
        {
            ImageList.Items.Add(item);
        }
    }

    private void ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = ImageList.SelectedItem as ImageObj;
        if (item == null)
            return;

        string local = $"{ImageSql.Local}{item.local}";
        ImageShow.Source = ImageTagUtils.GetBitmapImage(local);
        NowTags.Children.Clear();
        List<ImageTagObj> list = Images[item];
        foreach (var item1 in list)
        {
            var tag = TagSql.GetTag(item1.tag_group, item1.tag_uuid);
            if (tag == null)
                continue;
            NowTags.Children.Add(new TagControl(tag, false));
        }
    }

    
}
