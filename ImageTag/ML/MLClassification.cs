using ImageTag.Sql;
using ImageTag.Windows;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ImageTag.Train;

public static class MLClassification
{
    public static bool InitTrain(string group)
    {
        var tags = TagSql.GetTags(group);
        string dir = AutoTag.ML + "temp/";
        ReturnImage();
        Directory.CreateDirectory(dir);
        int a = 0;
        foreach (var item in tags)
        {
            var list = ImageSql.GetImageFromTag(item.group, item.uuid);
            if (list.Count < 10)
            {
                continue;
            }
            a++;
            string dir1 = dir + item.uuid + "/";
            Directory.CreateDirectory(dir1);
            foreach (var item1 in list)
            {
                string local = $"{ImageSql.Local}{item1.local}";
                int b = 0;
                while (b < 100)
                {
                    try
                    {
                        File.Move(local, dir1 + item1.local);
                        break;
                    }
                    catch
                    {
                        b++;
                        if (b > 100)
                        {
                            _ = new InfoWindow("机器学习错误", "文件复制失败");
                            return false;
                        }
                        Thread.Sleep(1000);
                    }
                }
            }
        }
        if (a < 2)
        {
            _ = new InfoWindow("错误", "样本数量不足，无法进行机器学习");
            return false;
        }
        return true;
    }

    public static void ReturnImage() 
    {
        string dir = AutoTag.ML + "temp/";

        DirectoryInfo info = new(dir);
        if (info.Exists)
        {
            foreach (var item in info.GetDirectories())
            {
                foreach (var item1 in item.GetFiles())
                {
                    File.Move(item1.FullName, ImageSql.Local + item1.Name);
                }
                item.Delete();
            }
            Directory.Delete(dir);
        }
    }

    public static void StartTrain(MLContext mlContext, string fullImagesetFolderPath, string outFile, CancellationToken ct, TrainWindow window)
    {
        // Specify MLContext Filter to only show feedback log/traces about ImageClassification
        // This is not needed for feedback output if using the explicit MetricsCallback parameter
        mlContext.Log += window.FilterMLContextLog;

        // 2. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
        IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: fullImagesetFolderPath, useFolderNameAsLabel: true);
        IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
        IDataView shuffledFullImageFilePathsDataset = mlContext.Data.ShuffleRows(fullImagesDataset);

        if (ct.IsCancellationRequested)
            return;

        // 3. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
        IDataView shuffledFullImagesDataset = mlContext.Transforms.Conversion.
                MapValueToKey(outputColumnName: "LabelAsKey", inputColumnName: "Label", keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
            .Append(mlContext.Transforms.LoadRawImageBytes(
                                            outputColumnName: "Image",
                                            imageFolder: fullImagesetFolderPath,
                                            inputColumnName: "ImagePath"))
            .Fit(shuffledFullImageFilePathsDataset)
            .Transform(shuffledFullImageFilePathsDataset);

        if (ct.IsCancellationRequested)
            return;

        // 4. Split the data 80:20 into train and test sets, train and evaluate.
        var trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
        IDataView trainDataView = trainTestData.TrainSet;
        IDataView testDataView = trainTestData.TestSet;

        if (ct.IsCancellationRequested)
            return;

        // 5. Define the model's training pipeline using DNN default values
        //
        var pipeline = mlContext.MulticlassClassification.Trainers
                .ImageClassification(featureColumnName: "Image",
                                     labelColumnName: "LabelAsKey",
                                     validationSet: testDataView)
            .Append(mlContext.Transforms.Conversion.MapKeyToValue(
                outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));

        if (ct.IsCancellationRequested)
            return;

        // 5.1 (OPTIONAL) Define the model's training pipeline by using explicit hyper-parameters
        //
        //var options = new ImageClassificationTrainer.Options()
        //{
        //    FeatureColumnName = "Image",
        //    LabelColumnName = "LabelAsKey",
        //    // Just by changing/selecting InceptionV3/MobilenetV2/ResnetV250  
        //    // you can try a different DNN architecture (TensorFlow pre-trained model). 
        //    Arch = ImageClassificationTrainer.Architecture.MobilenetV2,
        //    Epoch = 50,       //100
        //    BatchSize = 10,
        //    LearningRate = 0.01f,
        //    MetricsCallback = (metrics) => Console.WriteLine(metrics),
        //    ValidationSet = testDataView
        //};

        //var pipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(options)
        //        .Append(mlContext.Transforms.Conversion.MapKeyToValue(
        //            outputColumnName: "PredictedLabel",
        //            inputColumnName: "PredictedLabel"));

        // 6. Train/create the ML model
        window.WriteLine("*** Training the image classification model with DNN Transfer Learning on top of the selected pre-trained model/architecture ***");

        // Measuring training time
        var watch = Stopwatch.StartNew();

        //Train
        ITransformer trainedModel = pipeline.Fit(trainDataView);

        if (ct.IsCancellationRequested)
            return;

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        window.WriteLine($"Training with transfer learning took: {elapsedMs / 1000} seconds");

        // 7. Get the quality metrics (accuracy, etc.)

        window.WriteLine("Making predictions in bulk for evaluating model's quality...");

        // Measuring time
        watch = Stopwatch.StartNew();

        var predictionsDataView = trainedModel.Transform(testDataView);

        if (ct.IsCancellationRequested)
            return;

        var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName: "LabelAsKey", predictedLabelColumnName: "PredictedLabel");

        if (ct.IsCancellationRequested)
            return;

        watch.Stop();
        var elapsed2Ms = watch.ElapsedMilliseconds;

        window.WriteLine($"Predicting and Evaluation took: {elapsed2Ms / 1000} seconds");

        // 8. Save the model to assets/outputs (You get ML.NET .zip model file and TensorFlow .pb model file)
        mlContext.Model.Save(trainedModel, trainDataView.Schema, outFile);

        if (ct.IsCancellationRequested)
            return;

        window.WriteLine($"Model saved to: {outFile}");
    }

    public static (float[], string[]) TrySinglePrediction(string file, MLContext mlContext, ITransformer trainedModel)
    {
        // Create prediction engine to try a single prediction (input = ImageData, output = ImagePrediction)
        var predictionEngine = mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(trainedModel);

        //Predict the first image in the folder
        var imagesToPredict = FileUtils.LoadInMemoryImagesFromFile(file);
        var prediction = predictionEngine.Predict(imagesToPredict);

        ////////
        VBuffer<ReadOnlyMemory<char>> keys = default;
        predictionEngine.OutputSchema[3].GetKeyValues(ref keys);
        var keysArray = keys.DenseValues().ToArray();
        string[] keys1 = new string[keysArray.Length];
        for (int a = 0; a < keysArray.Length; a++)
        {
            keys1[a] = new string(keysArray[a].ToArray());
        }
        ////////

        return (prediction.Score, keys1);
    }

    public static IEnumerable<ImageData> LoadImagesFromDirectory(
        string folder,
        bool useFolderNameAsLabel = true)
        => FileUtils.LoadImagesFromDirectory(folder, useFolderNameAsLabel)
            .Select(x => new ImageData(x.imagePath, x.label));
}

public class FileUtils
{
    public static IEnumerable<(string imagePath, string label)> LoadImagesFromDirectory(
        string folder,
        bool useFolderNameasLabel)
    {
        var imagesPath = Directory
            .GetFiles(folder, "*", searchOption: SearchOption.AllDirectories)
            .Where(x => Path.GetExtension(x) == ".jpg" || Path.GetExtension(x) == ".png");

        return useFolderNameasLabel
            ? imagesPath.Select(imagePath => (imagePath, Directory.GetParent(imagePath).Name))
            : imagesPath.Select(imagePath =>
            {
                var label = Path.GetFileName(imagePath);
                for (var index = 0; index < label.Length; index++)
                {
                    if (!char.IsLetter(label[index]))
                    {
                        label = label.Substring(0, index);
                        break;
                    }
                }
                return (imagePath, label);
            });
    }

    public static InMemoryImageData LoadInMemoryImagesFromFile(
        string file)
        => new (image: File.ReadAllBytes(file));
}