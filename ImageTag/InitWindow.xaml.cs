using ImageTag.Sql;
using ImageTag.ML;
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

namespace ImageTag;

/// <summary>
/// InitWindow.xaml 的交互逻辑
/// </summary>
public partial class InitWindow : Window
{
    public InitWindow()
    {
        InitializeComponent();

        Task.Run(() =>
        {
            MLClassification.ReturnImage();
            TagSql.Start();
            ImageSql.Start();
            AutoTag.Init();
            Dispatcher.Invoke(Done);
        });
    }

    private void Done() 
    {
        new MainWindow().Show();
        Close();
    }
}
