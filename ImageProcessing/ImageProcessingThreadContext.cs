namespace ImageProcessing
{
    public class ImageProcessingThreadContext
    {
        public DProcessPixel Processor { get; set; }
        public FastBitMap Original { get; set; }
        public FastBitMap Result { get; set; }

        public int ThreadID { get; set; }
        public int ThreadCount { get; set; }
    }
}
