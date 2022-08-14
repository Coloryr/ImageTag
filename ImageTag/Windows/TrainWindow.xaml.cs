using ImageTag.ML;
using Microsoft.ML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public int Epoch { get; set; } = 100;
    public int BatchSize { get; set; } = 20;
    public float LearningRate { get; set; } = 0.01f;
    private int Arch = 2;
    private bool Going;
    public TrainWindow(TagGroupObj group)
    {
        InitializeComponent();
        GroupObj = group;
        DataContext = this;
        ShowDialog();
    }

    public void WriteLine(string data)
    {
        Dispatcher.Invoke(() =>
        {
            Log.AppendText($"[{DateTime.Now}]{data}{Environment.NewLine}");
            Log.ScrollToEnd();
        });
    }

    public void FilterMLContextLog(object? sender, LoggingEventArgs e)
    {
        WriteLine(e.Message);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        tokenSource.Cancel();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        if (Going)
            return;

        Going = true;
        Select1.IsEnabled = Select2.IsEnabled = Select3.IsEnabled = Select4.IsEnabled = Select5.IsEnabled = false;
        TLearningRate.IsEnabled = TBatchSize.IsEnabled = TEpoch.IsEnabled = false;
        Cancel.IsEnabled = true;
        Start.IsEnabled = false;
        Task.Run(() =>
        {
            WriteLine("正在初始化");
            if (!MLClassification.InitTrain(GroupObj.uuid))
            {
                return;
            }

            WriteLine("开始生成模型");
            var tk = tokenSource.Token;
            MLClassification.StartTrain(AutoTag.MlContext, AutoTag.ML + "temp/",
                AutoTag.ML + GroupObj.uuid + ".zip", tk, this,
                Epoch, BatchSize, LearningRate, Arch);
            if (tk.IsCancellationRequested)
                return;
            AutoTag.LoadModel(GroupObj.uuid);

            WriteLine("清理缓存");
            MLClassification.ReturnImage();
            WriteLine("完成，请关闭窗口继续");
            Dispatcher.Invoke(() =>
            {
                Cancel.IsEnabled = false;
                Start.IsEnabled = true;
                Select1.IsEnabled = Select2.IsEnabled = Select3.IsEnabled = Select4.IsEnabled = Select5.IsEnabled = true;
            });
            Going = false;
        }, tokenSource.Token);
    }

    private void Select_Checked(object sender, RoutedEventArgs e)
    {
        var button = sender as RadioButton;
        if (button == null)
            return;

        switch (button.Name)
        {
            case "Select1":
                Arch = 0;
                break;
            case "Select2":
                Arch = 1;
                break;
            case "Select3":
                Arch = 2;
                break;
            case "Select4":
                Arch = 3;
                break;
            case "Select5":
                Arch = 4;
                break;
        }

        if (Arch == 0)
        {
            TLearningRate.IsEnabled = TBatchSize.IsEnabled = TEpoch.IsEnabled = false;
        }
        else
        {
            TLearningRate.IsEnabled = TBatchSize.IsEnabled = TEpoch.IsEnabled = true;
        }
    }
}
