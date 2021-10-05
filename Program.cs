using Microsoft.ML;
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

namespace YOLOv4MLNet
{
    //https://towardsdatascience.com/yolo-v4-optimal-speed-accuracy-for-object-detection-79896ed47b50
    class Program
    {
        private static object bufferLock = new object();
        // model is available here:
        // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4
        const string modelPath = @"C:\Users\zhang19990910\Desktop\net\YOLOv4MLNet\YOLOv4MLNet\yolov4.onnx";

        const string imageFolder = @"C:\Users\zhang19990910\Desktop\net\YOLOv4MLNet\YOLOv4MLNet\Assets\Images";

        const string imageOutputFolder = @"C:\Users\zhang19990910\Desktop\net\YOLOv4MLNet\YOLOv4MLNet\Assets\Output";
        private const string V = "j";
        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };


        static async Task Main()

        {
            Dictionary<string, int> hashmap = new Dictionary<string, int>();

            Directory.CreateDirectory(imageOutputFolder);
            MLContext mlContext = new MLContext();

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
            /* int i = 0;
             while (i < 10)
             {
                 Thread.Sleep(200);

                 Console.WriteLine($"{ i++ * 10}+%");


             }
             Console.WriteLine($"{ 99.99}+%");
             Console.WriteLine($"{ 100}+%");*/
            // save model
            //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));
            var sw = new Stopwatch();
            sw.Start();
            int j = 1, k = 1;

            await Task.Run(() => Parallel.ForEach(new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg", "xiazai.jpg" }, imageName =>//异步多线程
            {

                /*  foreach (string imageName in new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg", "xiazai.jpg" })
                  {*/
                using (var bitmap = new Bitmap(Image.FromFile(Path.Combine(imageFolder, imageName))))
                {
                    // predict
                    lock (bufferLock)
                    {
                        var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                        var results = predict.GetResults(classesNames, 0.3f, 0.7f);

                        using (var g = Graphics.FromImage(bitmap))
                        {

                            foreach (var res in results)
                            // await Task.Run(() => Parallel.ForEach(results, res =>
                            {
                                // draw predictions
                                var x1 = res.BBox[0];
                                var y1 = res.BBox[1];
                                var x2 = res.BBox[2];
                                var y2 = res.BBox[3];
                                //Console.WriteLine("total objects: ", results.Count);
                                lock (bufferLock)
                                {
                                    g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                                }

                                using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                                {
                                    lock (bufferLock)
                                    {
                                        g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                                    }
                                }

                                lock (bufferLock)
                                {

                                    g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                                new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));



                                }


                                // btnthread_click();


                                lock (bufferLock)
                                {

                                    //asyncResult = action.BeginInvoke(asyncCallback, "dwad");
                                }


                                Console.WriteLine(res.Label + " " + res.Confidence.ToString("0.00") + " id " + Task.CurrentId);
                                if (hashmap.ContainsKey(res.Label))
                                    hashmap[res.Label]++;
                                else
                                    hashmap.Add(res.Label, j);
                            };


                            bitmap.Save(Path.Combine(imageOutputFolder, Path.ChangeExtension(imageName, "_processed" + Path.GetExtension(imageName))));


                        }
                    }
                    DirectoryInfo info = new DirectoryInfo(imageFolder);
                    int count = info.GetFiles("*.jpg").Length;
                    if (k <= count)
                        Console.WriteLine("doing" + k * (100 / count) + "%");
                    k++;
                }


            }));

            sw.Stop();
            foreach (KeyValuePair<string, int> vis in hashmap)
            {
                Console.WriteLine(vis.Key + ":" + vis.Value);
            }
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");
        }
    }
}
