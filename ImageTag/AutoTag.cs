using ImageTag.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using ImageTag.Train;

namespace ImageTag;

internal static class AutoTag
{
    private static string? Dir;
    private static TagObj? Dir1;
    private static TagObj? Dir2;
    private static TagObj? Dir3;

    public static readonly string ML = App.Local + "ML/";

    public static MLContext mlContext { get; private set; }
    private static Dictionary<string, ITransformer> models = new();
    
    public static void Init() 
    {
        var group = TagSql.GetGroup("方向");
        if (group != null)
        {
            Dir = group.uuid;
            var list = TagSql.GetGroupTag(group.uuid);
            Dir1 = (from item in list where item.name == "横向" select item).FirstOrDefault();
            Dir2 = (from item in list where item.name == "纵向" select item).FirstOrDefault();
            Dir3 = (from item in list where item.name == "矩形" select item).FirstOrDefault();
        }

        Directory.CreateDirectory(ML);

        mlContext = new MLContext(seed: 1);
        // Load the model
        foreach (var item in TagSql.GetAllGroup())
        {
            string name = ML + item.uuid + ".zip";
            if (File.Exists(name))
            {
                var model = mlContext.Model.Load(name, out _);
                models.Add(item.uuid, model);
            }
        }
    }

    public static void LoadModel(string group) 
    {
        string name = ML + group + ".zip";
        if (File.Exists(name))
        {
            var model = mlContext.Model.Load(name, out _);
            models.Add(group, model);
        }
    }

    public static TagObj? PicDir(string file) 
    {
        if (Dir == null)
            return null;

        Image image = Image.FromFile(file);
        double num = image.Width / image.Height;
        image.Dispose();
        if (num > 1.1)
        {
            return Dir1;
        }
        else
        {
            if (num < 0.9)
            {
                return Dir2;
            }
            else
            {
                return Dir3;
            }
        }
    }

    public static TagObj? PicML(string file, string group = "") 
    {
        if (models.TryGetValue(group, out var model))
        {
            var res = MLClassification.TrySinglePrediction(file, mlContext, model);
        }

        return null;
    }

    public static List<TagObj> CheckTag(string file) 
    {
        var list = new List<TagObj>();
        var tag = PicDir(file);
        if (tag != null)
            list.Add(tag);

        tag = PicML(file);

        return list;
    }

   
}
