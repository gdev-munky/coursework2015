using System;
using System.Drawing;
using System.Threading;

namespace ImageProcessing
{
    public abstract class ImageProcessor
    {
        public double TimeSpentOnProcessing = 0;
        protected IImageProcessingBlockFactory BlockFactory = null;
        protected abstract void InitBlockFactory();

        public abstract /*Float*/Color ProcessPixel(/*Float*/Color sourcePixel, ImageProcessingBlock block, int px, int py, int tx, int ty);

        public virtual void ProcessBlock(ImageProcessingBlock block, int tx, int ty)
        {
            block.StartAccess();
            for (var y = block.Rectangle.Y; y < block.Rectangle.Bottom; ++y)
                for (var x = block.Rectangle.X; x < block.Rectangle.Right; ++x)
                {
                    block.SetPixel(
                        x, y,
                        ProcessPixel(
                            block.GetPixel(x, y),
                            block,
                            x, y,
                            tx, ty)
                        );
                }
            block.StopAccess();
            block.ResetEvent.Set();
        }

        protected virtual void ProcessBlockCallBack(object o)
        {
            var tple = (Tuple<ImageProcessingBlock, int, int>) o;
            ProcessBlock(tple.Item1, tple.Item2, tple.Item3);
        }

        /// <summary>
        /// Выполняет обработку изображения в заданное количество потоков
        /// </summary>
        /// <param name="sImage">Исходное изображение</param>
        /// <param name="tImage">Изображение - результат</param>
        /// <param name="xThreads">Количество потоков по оси X</param>
        /// <param name="yThreads">Количество потоков по оси Y</param>
        /// <param name="maxThreads">Максимальное колчество параллельно выполняющихся потоков</param>
        public virtual void ProcessImage(Bitmap sImage, Bitmap tImage, int xThreads = 4, int yThreads = 4,
            int maxThreads = 8)
        {
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);
            BlockFactory.SetSourceImage(sImage);
            BlockFactory.SetTargetImage(tImage);
            var blockMap = new ImageProcessingBlock[xThreads, yThreads];
            var resetEvents = new WaitHandle[xThreads*yThreads];

            var blockWidth = (int)Math.Ceiling((float) sImage.Width/(float) xThreads);
            var blockHeight = (int)Math.Ceiling((float)sImage.Height / (float)yThreads);

            var time = DateTime.Now;

            for (var y = 0; y < yThreads; ++y)
                for (var x = 0; x < xThreads; ++x)
                {
                    var bx = x * blockWidth;
                    var by = y * blockHeight;
                    var dx = bx + blockWidth - sImage.Width;
                    var dy = by + blockHeight - sImage.Height;
                    var bw = dx > 0 ? blockWidth - dx : blockWidth;
                    var bh = dy > 0 ? blockHeight - dy : blockHeight;

                    var block = blockMap[x, y] = BlockFactory.GetNewBlock(this);
                    block.Rectangle = new Rectangle(bx, by, bw, bh);
                    resetEvents[y * xThreads + x] = block.ResetEvent = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(ProcessBlockCallBack,
                        new Tuple<ImageProcessingBlock, int, int>(block, x, y));
                }
            WaitHandle.WaitAll(resetEvents);
            TimeSpentOnProcessing = (DateTime.Now - time).TotalMilliseconds;
        }
    }
}
