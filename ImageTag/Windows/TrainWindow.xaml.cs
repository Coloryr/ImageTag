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
    public TrainWindow()
    {
        InitializeComponent();
    }

    public void Set(string group, Action<object> action)
    {
        Task.Run(() =>
        {
            var tk = tokenSource.Token;
            action(tk);
            if (tk.IsCancellationRequested)
                return;
            AutoTag.LoadModel(group);
        }, tokenSource.Token);
        ShowDialog();
    }

    public void WriteLine(string data) 
    {
        Log.AppendText(data + Environment.NewLine);
    }

    public void FilterMLContextLog(object? sender, LoggingEventArgs e)
    {
        Log.AppendText(e.Message + Environment.NewLine);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        tokenSource.Cancel();
    }
}
