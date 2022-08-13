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
/// TagControl.xaml 的交互逻辑
/// </summary>
public partial class TagControl : UserControl
{
    private readonly Action<TagControl>? OnClose;
    private readonly Action<TagControl>? OnEdit;
    public TagObj TagObj { get; private set; }
    public TagControl(TagObj tag, bool need, Action<TagControl>? close = null, bool edit = false, string text="×", Action<TagControl>? onedit = null)
    {
        InitializeComponent();

        OnClose = close;
        TagObj = tag;
        TagName.Text = tag.name;
        Close.Content = text;
        OnEdit = onedit;

        if (!need)
        {
            Close.Visibility = Visibility.Collapsed;
        }
        if (!edit)
        {
            Edit.Visibility = Visibility.Collapsed;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        OnClose?.Invoke(this);
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        OnEdit?.Invoke(this);
    }
}
