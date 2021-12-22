﻿using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;

namespace YOLOv4MLNet
{
    //https://towardsdatascience.com/yolo-v4-optimal-speed-accuracy-for-object-detection-79896ed47b50
    public class abc
    {
        
        // model is available here:
        // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4
        const string modelPath = @"D:\yolo\yolov4.onnx";

        // const string imageFolder = @"D:\yolo\YOLOv4MLNet\YOLOv4MLNet\Assets\Images";

        const string imageOutputFolder = @"D:\yolo\YOLOv4MLNet\YOLOv4MLNet\Assets\Output";
        public static BufferBlock<string> bB = new BufferBlock<string>();
        public static BufferBlock<(string, string)> rBB = new BufferBlock<(string, string)>();
        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };
        public static CancellationTokenSource cancelTokenSource;
        public static CancellationToken token;
        public  static async Task Main(string imageFolder)
        {
            Dictionary<string, int> hashmap = new Dictionary<string, int>();
            Directory.CreateDirectory(imageOutputFolder);
            MLContext mlContext = new MLContext();
            //Console.WriteLine(" path: ");
            //string imageFolder = Console.ReadLine();


            // model is available here:
            // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            
            // Create prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
            ConcurrentBag<YoloV4Result> dO = new ConcurrentBag<YoloV4Result>();
            string[] imageNames = Directory.GetFiles(imageFolder);
            ModelLibrary.image processedImages = new ModelLibrary.image(imageNames.Length);
          
            Dictionary<string, ConcurrentBag<string>> rO = new Dictionary<string, ConcurrentBag<string>>();//定义一个字典

            foreach (string name in classesNames)
            {
                rO.Add(name, new ConcurrentBag<string>());
            }
            object locke = new object();//定义一个锁
            // save model
            //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));
            var sw = new Stopwatch();
            sw.Start();
            var ab = new ActionBlock<string>(async image => {
                YoloV4Prediction predict;
                lock (locke)
                {
                    var bitmap = new Bitmap(Image.FromFile(Path.Combine(image)));
                    predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                }

                /* foreach (string imageName in new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg" })
                 {
                     using (var bitmap = new Bitmap(Image.FromFile(Path.Combine(imageFolder, imageName))))
                     {
                         // predict
                         var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                         var results = predict.GetResults(classesNames, 0.3f, 0.7f);

                         using (var g = Graphics.FromImage(bitmap))
                         {
                             foreach (var res in results)
                             {
                                 // draw predictions
                                 var x1 = res.BBox[0];
                                 var y1 = res.BBox[1];
                                 var x2 = res.BBox[2];
                                 var y2 = res.BBox[3];
                                 g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                                 using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                                 {
                                     g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                                 }

                                 g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                              new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
                             }
                             bitmap.Save(Path.Combine(imageOutputFolder, Path.ChangeExtension(imageName, "_processed" + Path.GetExtension(imageName))));
                         }
                     }
                 }*/




                var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                int i = 0;
                foreach (var res in results)
                {
                    rO[res.Label].Add(image);
                    var x1 = res.BBox[0];
                    var y1 = res.BBox[1];
                    var x2 = res.BBox[2];
                    var y2 = res.BBox[3];


                    Rectangle cropRect = new Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
                    Bitmap src = Image.FromFile(image) as Bitmap;
                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                         cropRect,
                                         GraphicsUnit.Pixel);
                    }
                    string imageOutputPath = imageOutputFolder + "/" + res.Label + x1.ToString()
                                           + y2.ToString() + i.ToString() + ".jpg";
                    target.Save(imageOutputPath);

                    
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = token
            });

            Parallel.For(0, imageNames.Length, i => ab.Post(imageNames[i]));
            ab.Complete();
            await ab.Completion;
            sw.Stop();

            foreach (KeyValuePair<string, int> vis in hashmap)
            {
                Console.WriteLine(vis.Key + ":" + vis.Value);
            }
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");
            
            /* await bufferBlock.SendAsync($"Total number of objects: {dO.Count}");
             await bufferBlock.SendAsync("end");*/
        }


        // sw.Stop();
    }
}

