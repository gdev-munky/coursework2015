using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing.AntiStamp1
{
    public class AntiStampProcessor : ImageProcessor
    {
        public /*Float*/Color BackColor { get; set; }
        public /*Float*/Color ForeColor { get; set; }
        public float ChromaticityTreshold { get; set; }
        protected override void InitBlockFactory()
        {
            BlockFactory = new AntiStampBlockFactory();
        }

        public override Color ProcessPixel(Color sourcePixel, ImageProcessingBlock block, int px, int py,
            int tx, int ty)
        {
            var chroma = CalculateChromaticity(sourcePixel);
            /*
            var ic = (int) (chroma*255.0f);
            return Color.FromArgb(255, ic, ic, ic);*/
            return chroma < ChromaticityTreshold
                ? sourcePixel.Lerp(BackColor, (ChromaticityTreshold - chroma)/(1 - ChromaticityTreshold))
                : BackColor;
        }

        private static float CalculateChromaticity(/*Float*/Color fc)
        {
            var rg = Math.Abs(fc.R - fc.G);
            var gb = Math.Abs(fc.G - fc.B);
            var rb = Math.Abs(fc.R - fc.B);
            return Math.Max(rg, Math.Max(gb, rb))/255.0f;
        }

        public static Bitmap Process(Bitmap image, int xThreads = 4, int yThreads = 4,
            int maxThreads = 8, float chromaTreshold = 0.1f)
        {
            var result = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            var processor = new AntiStampProcessor
            {
                BackColor = /*new FloatColor*/(Color.White),
                ForeColor = /*new FloatColor*/(Color.Black),
                ChromaticityTreshold = chromaTreshold
            };
            processor.InitBlockFactory();
            processor.ProcessImage(image, result, xThreads, yThreads, maxThreads);
            Console.WriteLine("Processing took {0} ms", processor.TimeSpentOnProcessing);
            var resultFC = ((AntiStampBlockFactory) processor.BlockFactory).targetImage;
            resultFC.DumpToBitmap(result);
            return result;
        }
    }

    public static class Extensions
    {
        public static Color Lerp(this Color from, Color to, float k)
        {
            var bk = 1 - k;
            var r = from.R*bk + to.R*k;
            var g = from.G * bk + to.G * k;
            var b = from.B * bk + to.B * k;
            var a = from.A * bk + to.A * k;
            return Color.FromArgb((int) a, (int) r, (int) g, (int) b);
        }
    }
}
