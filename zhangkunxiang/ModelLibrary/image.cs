﻿using System;
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
    class image
    {
        ConcurrentBag<Bitmap> bag = new ConcurrentBag<Bitmap>();
        
        int countimage;

        public image(int count)
        {
            countimage = count;
        }

        public string Add(Bitmap bitmap)
        {
            bag.Add(bitmap);
            
            return ((double)bag.Count / countimage * 100).ToString() + "%";
        }

    
    }
}