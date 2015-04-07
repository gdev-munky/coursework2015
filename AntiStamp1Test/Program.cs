using System;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessing.AntiStamp1;

namespace AntiStamp1Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;

            int maxThreads = 16;
            int xThreads = 4;
            int yThreads = 8;
            float treshold = 0.1f;

            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                var ns = i + 1 < args.Length ? args[i + 1] : null;

                if (s == "-maxthreads")
                {
                    maxThreads = int.Parse(ns);
                }
                else if (s == "-xthreads")
                {
                    xThreads = int.Parse(ns);
                }
                else if (s == "-ythreads")
                {
                    yThreads = int.Parse(ns);
                }
                else if (s == "-treshold")
                {
                    treshold = float.Parse(ns);
                }

            }
            var fileName = args[0];
            Console.WriteLine("Running processor with {0}x{1} ({2} simultaneously) threads", xThreads, yThreads,
                maxThreads);
            Console.WriteLine("Loading bitmap ...");
            var bitmap = new Bitmap(fileName);
            Console.WriteLine("Processing ...");
            var time = DateTime.Now;
            var result = AntiStampProcessor.Process(bitmap, xThreads, yThreads, maxThreads, treshold);
            var ms = (DateTime.Now - time).TotalMilliseconds;
            Console.WriteLine("Processed in {0} ms, now saving...", ms);
            result.Save(fileName + ".processed.png", ImageFormat.Png);
            Console.WriteLine("Done! Press any key to exit");
            Console.ReadKey(true);
        }
    }
}
