using System;
using System.Collections.Generic;

namespace ImageProcessing
{
    public class ImageProcessingContext
    {
        public ImageProcessingContext()
        {
            Images = new Dictionary<string, FastBitMap>();
            ThreadList = new List<ImageProcessingThread>(1024);
        }
        public Dictionary<string, FastBitMap> Images { get; private set; }
        public List<ImageProcessingThread> ThreadList { get; private set; }
        public bool TryGetImage(string name, out FastBitMap bmp)
        {
            bmp = Images[name];
            return (bmp != null);
        }
    }
}
