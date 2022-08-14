using ImageTag.Sql;
using ImageTag.ML;
using ImageTag.Views;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Tensorflow.Keras.Engine;

namespace ImageTag.Windows;

/// <summary>
/// MLWindow.xaml 的交互逻辑
/// </summary>
public partial class MLWindow : Window
{
    public string File { get; set; }
    public TagGroupObj Group { get; set; }
    public Action<TagObj> Call { get; set; }
    public ITransformer Model { get; set; }
    private bool Going;
    public MLWindow()
    {
        InitializeComponent();
        Show();
    }

    public void Start() 
    {
        ResList.Items.Clear();
        GroupName.Content = $"标签组：{Group.name}，正在预测中";
        Task.Run(() =>
        {
            Going = true;
            var res = MLClassification.TrySinglePrediction(File, AutoTag.MlContext, Model);
            Going = false;
            Dispatcher.Invoke(() => Done(res));
        });
    }

    public record DataSet
    {
        public string TagName { get; set; }
        public float Score { get; set; }
        public TagObj Tag { get; set; }
        public string Score1 { get; set; }
    }

    private void Done((float[], string[]) res) 
    {
        GroupName.Content = $"标签组：{Group.name}，预测结束";

        int size = res.Item1.Length;
        float[] scores = res.Item1;
        string[] labels = res.Item2;

        for (int a = 0; a < size; a++)
        {
            var tag = TagSql.GetTag(Group.uuid, labels[a]);
            if (tag != null)
            {
                float score = scores[a];
                ResList.Items.Add(new DataSet()
                {
                    TagName = tag.name,
                    Tag = tag,
                    Score = score,
                    Score1 = $"{score * 100}%"
                });
            }
        }

        Score1.Visibility = Visibility.Collapsed;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (Going)
        {
            e.Cancel = true;
        }
        else
        {
            AutoTag.MLWindow.Remove(Group.uuid);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        AutoTag.MLWindow.Remove(Group.uuid);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Start();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as Button).Tag as TagObj;
        if (tag != null)
        {
            Call(tag);
        }
    }
}
