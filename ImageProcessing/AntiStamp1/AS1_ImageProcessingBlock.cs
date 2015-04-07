using System.Drawing;

namespace ImageProcessing.AntiStamp1
{
    public class AntiStampProcessingBlock : ImageProcessingBlock
    {
        public ColorImage srcImage, targetImage;

        public override void StartAccess() { }

        public override void StopAccess() { }

        public override /*Float*/Color GetPixel(int x, int y)
        {
            return srcImage[x, y];
        }

        public override void SetPixel(int x, int y, /*Float*/Color clr)
        {
            targetImage[x, y] = clr;
        }
    }

    public class AntiStampBlockFactory : IImageProcessingBlockFactory
    {
        public ColorImage srcImage, targetImage;
        public Bitmap srcBitmap, targetBitmap;
        public void SetSourceImage(Bitmap image)
        {
            srcBitmap = image;
            srcImage = new ColorImage(srcBitmap);
        }

        public void SetTargetImage(Bitmap image)
        {
            targetBitmap = image;
            targetImage = new ColorImage(targetBitmap.Width, targetBitmap.Height);
        }

        public ImageProcessingBlock GetNewBlock(ImageProcessor processor = null)
        {
            return new AntiStampProcessingBlock {srcImage = srcImage, targetImage = targetImage};
        }
    }
}
