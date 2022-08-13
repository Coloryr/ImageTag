using ImageTag.Train;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageTag.Windows;

/// <summary>
/// TrainWindow.xaml 的交互逻辑
/// </summary>
public partial class TrainWindow : Window
{
    private CancellationTokenSource tokenSource = new CancellationTokenSource();
    private TagGroupObj GroupObj;
    public TrainWindow(TagGroupObj group)
    {
        InitializeComponent();
        GroupObj = group;
        ShowDialog();
    }

    public void WriteLine(string data)
    {
        Dispatcher.Invoke(() =>
        {
            Log.AppendText(data + Environment.NewLine);
        });
    }

    public void FilterMLContextLog(object? sender, LoggingEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            Log.AppendText(e.Message + Environment.NewLine);
        });
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        tokenSource.Cancel();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Task.Run(() =>
        {
            WriteLine("正在初始化");
            if (!MLClassification.InitTrain(GroupObj.uuid))
            {
                return;
            }

            WriteLine("开始生成模型");
            var tk = tokenSource.Token;
            MLClassification.StartTrain(AutoTag.mlContext, AutoTag.ML + "temp/", AutoTag.ML + GroupObj.uuid + ".zip", tk, this);
            if (tk.IsCancellationRequested)
                return;
            AutoTag.LoadModel(GroupObj.uuid);

            WriteLine("清理缓存");
            MLClassification.ReturnImage();
            WriteLine("完成");
        }, tokenSource.Token);

    }
}
