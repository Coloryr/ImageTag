using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTag.Train;

public class InputData
{
    public InputData(List<string> name, string lable) 
    {
        ImageName = name;
        Label = lable;
    }

    public readonly List<string> ImageName;
    public readonly string Label;
}

public class ImageData
{
    public ImageData(string imagePath, string label)
    {
        ImagePath = imagePath;
        Label = label;
    }

    public readonly string ImagePath;
    public readonly string Label;
}

public class InMemoryImageData
{
    public InMemoryImageData(byte[] image)
    {
        ImageSource = image;
    }

    public readonly byte[] ImageSource;
    public readonly string Label;
}

public class ImagePrediction
{
    [ColumnName(@"Label")]
    public uint Label { get; set; }

    [ColumnName(@"ImageSource")]
    public byte[] ImageSource { get; set; }

    [ColumnName(@"PredictedLabel")]
    public string PredictedLabel { get; set; }

    [ColumnName(@"Score")]
    public float[] Score { get; set; }
}