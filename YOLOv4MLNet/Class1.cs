using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.ML;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ModelLibrary
{
    class ProcessedImages
    {
        ConcurrentBag<Bitmap> bag = new ConcurrentBag<Bitmap>();
        //ConcurrentBag<>
        int countOfImages;

        public ProcessedImages(int count)
        {
            countOfImages = count;
        }

        public string Add(Bitmap bitmap)
        {
            bag.Add(bitmap);
            //Console.Clear();
            //Detector.bufferBlock.SendAsync(((double)bag.Count / countOfImages * 100).ToString() + "%");
            //Console.WriteLine(((double)bag.Count / countOfImages * 100).ToString() + "%");
            return ((double)bag.Count / countOfImages * 100).ToString() + "%";
        }

        /*
        public string Percent()
        {
            return ((double)bag.Count / countOfImages * 100).ToString() + "%";
        }
        */
    }
}