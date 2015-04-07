using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessing
{
    public abstract class ImageProcessingBlock
    {
        public Rectangle Rectangle { get; set; }
        public ManualResetEvent ResetEvent { get; set; }

        public abstract void StartAccess();
        public abstract void StopAccess();

        /// <summary>
        /// Считывает цвет пикселя в исходном изображении
        /// </summary>
        public abstract /*Float*/Color GetPixel(int x, int y);
        /// <summary>
        /// Записывает цвет пикселя в конечное изображение
        /// </summary>
        public abstract void SetPixel(int x, int y, /*Float*/Color clr);
    }

    public interface IImageProcessingBlockFactory
    {
        void SetSourceImage(Bitmap image);
        void SetTargetImage(Bitmap image);
        ImageProcessingBlock GetNewBlock(ImageProcessor processor = null);
    }
}
