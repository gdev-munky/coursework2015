using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing.AntiStamp_v1
{
    public class AntiStampImageProcessor : TemplateImageProcessor
    {
        public AntiStampImageProcessor(string srcFileName, string targetFileName)
        {
            Tasks = new List<ImageProcessingTask>
            {
                new LoadImageFromFileTask(srcFileName),
                new CreateTargetForImageTask(srcFileName, "?target"),
                new LockImageTask(srcFileName, ImageLockMode.ReadOnly),
                new LockImageTask("?target", ImageLockMode.WriteOnly),
                new ProcessFullImageTask(srcFileName, PixelProcessor, "?target")
                {MaxFragmentWidth = 32, MaxFragmentHeight = 32, ThreadsToUse = 8},
                new LockImageTask(srcFileName, ImageLockMode.UserInputBuffer),
                new LockImageTask("?target", ImageLockMode.UserInputBuffer),
                new SaveImageToFileTask("?target", ImageFormat.Png, targetFileName)
            };
        }

        public readonly List<ImageProcessingTask> Tasks;
        protected override IEnumerable<ImageProcessingTask> GetTasks()
        {
            return Tasks;
        }

        private const float CHROMATICITY_MIN = 0.2f;
        private const float CHROMATICITY_MAX = 0.3f;
        private static readonly Color BackColor = Color.White;
        private static float sqrt2 = (float)System.Math.Sqrt(2);

        private static void PixelProcessor(int x, int y, ImageProcessingThreadContext context)
        {/*
            var sourceColor = context.Original[x, y];
            var chroma = sourceColor.CalculateChromaticity();
            const float treshold = 0.1f;
            var BackColor = Color.White;
            var targetColor = chroma < treshold
                ? sourceColor.Lerp(BackColor, (treshold - chroma) / (1 - treshold))
                : BackColor;
            context.Result[x, y] = targetColor;*/

            var sourceColor = context.Original[x, y];
            var chroma = sourceColor.CalculateChromaticity();
            if (x == 0 || y == 0 || x == context.Original.Width - 1 || y == context.Original.Height - 1)
            {
                context.Result[x, y] = Color.FromArgb(0, Color.Black);
                return;
            }
            var br = (byte)(sourceColor.GetBrightness() * 255);
            if (chroma <= CHROMATICITY_MIN)
            {
                context.Result[x, y] = Color.FromArgb(255 - br, br, br, br);
                return;
            }
            var k = 1.0f;
            if (chroma < CHROMATICITY_MAX)
            {
                k = (CHROMATICITY_MAX - chroma) / (CHROMATICITY_MAX - CHROMATICITY_MIN);
            }
            context.Result[x, y] = Color.FromArgb(255 - br, br, br, br).Lerp(Color.FromArgb(0, BackColor), k);
        }

    }
}
