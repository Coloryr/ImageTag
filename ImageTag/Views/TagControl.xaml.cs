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
    private Action<TagObj> OnClose;
    private TagObj Tag;
    public TagControl(TagObj tag, Action<TagObj> close, bool need)
    {
        InitializeComponent();

        OnClose = close;
        Tag = tag;
        Name.Content = tag.name;

        if (!need)
        {
            Close.Visibility = Visibility.Collapsed;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        OnClose(Tag);
    }
}
