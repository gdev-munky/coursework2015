using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing
{
    public abstract class ImageProcessingTask
    {
        protected abstract bool Execute(ImageProcessingContext context);
        public bool Run(ImageProcessingContext context)
        {
            IsComplete = false;
            var timeStart = DateTime.UtcNow;
            var result = Execute(context);
            TimeSpentOnExecution = DateTime.UtcNow - timeStart;
            IsComplete = true;
            return result;
        }
        public string ExecutionMessage { get; protected set; }
        public bool IsComplete { get; private set; }
        public TimeSpan TimeSpentOnExecution { get; private set; }
    }

    public class LoadImageFromFileTask : ImageProcessingTask
    {
        public LoadImageFromFileTask(string fileName)
        {
            FileName = fileName;
        }
        public string FileName { get; private set; }
        public FastBitMap LoadedImage { get; set; }
        protected override bool Execute(ImageProcessingContext context)
        {
            try
            {
                var bitmap = new Bitmap(FileName);
                LoadedImage = new FastBitMap(bitmap);
                context.Images[FileName] = LoadedImage;
                return true;
            }
            catch (Exception ex)
            {
                ExecutionMessage = ex.Message;
#if DEBUG
                throw;
#else
                return false;
#endif
            }
        }

        public override string ToString()
        {
            return "TASK: LOAD_IMAGE_FROM_FILE " + FileName;
        }
    }

    public class LockImageTask : ImageProcessingTask
    {
        public LockImageTask(string imageName, ImageLockMode mode)
        {
            ImageFileName = imageName;
            LockMode = mode;
        }
        public string ImageFileName { get; private set; }
        public ImageLockMode LockMode { get; private set; }
        protected override bool Execute(ImageProcessingContext context)
        {
            FastBitMap bmp;
            if (!context.TryGetImage(ImageFileName, out bmp))
            {
                ExecutionMessage = string.Format("Image named '{0}' has not been found in current processing context",
                    ImageFileName);
                return false;
            }

            switch (LockMode)
            {
                case ImageLockMode.ReadOnly:
                    if (bmp.CanWrite)
                        bmp.CanWrite = false;
                    if (!bmp.CanRead)
                        bmp.CanRead = true;
                    break;
                case ImageLockMode.WriteOnly:
                    if (!bmp.CanWrite)
                        bmp.CanWrite = true;
                    if (bmp.CanRead)
                        bmp.CanRead = false;
                    break;
                case ImageLockMode.ReadWrite:
                    if (!bmp.CanReadAndWrite)
                        bmp.CanReadAndWrite = true;
                    break;
                case ImageLockMode.UserInputBuffer:
                    if (bmp.CanRead || bmp.CanWrite)
                        bmp.CanReadAndWrite = false;
                    break;
                default:
                    ExecutionMessage = string.Format("Passed LockMode value ({0}) is out of range", (int) LockMode);
                    return false;
            }
            return true;
        }
        public override string ToString()
        {
            return string.Format("TASK: LOCK_IMAGE {0} (with {1} mode)", ImageFileName, LockMode);
        }
    }

    public class ProcessFullImageTask : ImageProcessingTask
    {
        public ProcessFullImageTask(string imageNameSource, DProcessPixel processor, string imageNameTarget)
        {
            SourceImageFileName = imageNameSource;
            TargetImageFileName = imageNameTarget;
            PixelProcessor = processor;
            ThreadsToUse = 768;
            MaxFragmentWidth = 16;
            MaxFragmentHeight = 16;
        }

        public string SourceImageFileName { get; private set; }
        public string TargetImageFileName { get; private set; }
        public int ThreadsToUse { get; set; }
        public int MaxFragmentWidth { get; set; }
        public int MaxFragmentHeight { get; set; }
        public DProcessPixel PixelProcessor { get; private set; }


        protected override bool Execute(ImageProcessingContext context)
        {
            FastBitMap bmpSource, bmpTarget;
            if (!context.TryGetImage(SourceImageFileName, out bmpSource))
            {
                ExecutionMessage = string.Format("Image named '{0}' has not been found in current processing context",
                    SourceImageFileName);
                return false;
            }
            if (!context.TryGetImage(TargetImageFileName, out bmpTarget))
            {
                ExecutionMessage = string.Format("Image named '{0}' has not been found in current processing context",
                    TargetImageFileName);
                return false;
            }
            for (var i = 0; i < ThreadsToUse; ++i)
                context.ThreadList.Add(new ImageProcessingThread());
            
            var xBlocks = (int)Math.Ceiling((float)bmpSource.Width / MaxFragmentWidth);
            var yBlocks = (int)Math.Ceiling((float)bmpSource.Height / MaxFragmentHeight);
            for (var by = 0; by <= yBlocks; ++by)
                for (var bx = 0; bx <= xBlocks; ++bx)
                {
                    var x = bx*MaxFragmentWidth;
                    var y = by*MaxFragmentHeight;
                    var w = Math.Min(bmpSource.Width - x, MaxFragmentWidth);
                    var h = Math.Min(bmpSource.Height - y, MaxFragmentHeight);
                    context.ThreadList[(by*bmpSource.Width + bx)%ThreadsToUse].PushTask(new Rectangle(x, y, w, h));
                }
            for (var i = 0; i < ThreadsToUse; ++i)
            {
                context.ThreadList[i].Start(new ImageProcessingThreadContext
                {
                    Original = bmpSource,
                    Processor = PixelProcessor,
                    Result = bmpTarget,
                    ThreadCount = ThreadsToUse,
                    ThreadID = i
                });
            } 
            for (var i = 0; i < ThreadsToUse; ++i)
                context.ThreadList[i].Wait();
            return true;
        }
        public override string ToString()
        {
            return string.Format("TASK: PROCESS_IMAGE_FULL {0}->{1} (threads: {2}; maxFragment: {3}x{4})",
                SourceImageFileName, TargetImageFileName, 
                ThreadsToUse, MaxFragmentWidth, MaxFragmentHeight);
        }
    }

    public class CreateTargetForImageTask : ImageProcessingTask
    {
        public string SourceImageFileName { get; private set; }
        public string TargetImageName { get; private set; }

        public CreateTargetForImageTask(string srcname, string newname)
        {
            SourceImageFileName = srcname;
            TargetImageName = newname;
        }

        protected override bool Execute(ImageProcessingContext context)
        {
            FastBitMap bmpSource;
            if (!context.TryGetImage(SourceImageFileName, out bmpSource))
            {
                ExecutionMessage = string.Format("Image named '{0}' has not been found in current processing context",
                    SourceImageFileName);
                return false;
            }

            context.Images[TargetImageName] = new FastBitMap(new Bitmap(bmpSource.Width, bmpSource.Height, PixelFormat.Format32bppArgb));
            return true;
        }
        public override string ToString()
        {
            return string.Format("TASK: CREATE_TARGET {1} for {0}", SourceImageFileName, SourceImageFileName);
        }
    }

    public class SaveImageToFileTask : ImageProcessingTask
    {
        public SaveImageToFileTask(string imageName, ImageFormat imageFormat, string newFileName = null)
        {
            ImageName = imageName;
            FileName = string.IsNullOrWhiteSpace(newFileName) ? imageName : newFileName;
            Format = imageFormat;
        }

        public string ImageName { get; private set; }
        public string FileName { get; private set; }
        public ImageFormat Format { get; private set; }
        protected override bool Execute(ImageProcessingContext context)
        {
            FastBitMap bmpSource;
            if (!context.TryGetImage(ImageName, out bmpSource))
            {
                ExecutionMessage = string.Format("Image named '{0}' has not been found in current processing context",
                    ImageName);
                return false;
            }
            try
            {
                bmpSource.Picture.Save(FileName, Format);
            }
            catch (Exception ex)
            {
                ExecutionMessage = ex.Message;
#if DEBUG
                throw;
#else
                return false;
#endif
            }
            return true;
        }
        public override string ToString()
        {
            return string.Format("TASK: SAVE_TO_DISK {0} -> {1} (format {2})", ImageName, FileName, Format);
        }
    }
}
