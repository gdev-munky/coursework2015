using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing
{
    public class FastBitMap
    {
        public PixelFormat Format { get; private set; }
        public Bitmap Picture { get; private set; }
        public ColorPalette PicturePallete { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private BitmapData BitmapLock { get; set; }
        private ImageLockMode _lockMode = ImageLockMode.UserInputBuffer;
        private ImageLockMode LockMode {
            get { return _lockMode; }
            set
            {
                /*if (_lockMode != ImageLockMode.UserInputBuffer)
                    throw new Exception();*/
                if (BitmapLock != null)
                    Picture.UnlockBits(BitmapLock);
                _lockMode = value;
                if (value == ImageLockMode.UserInputBuffer)
                    return;
                BitmapLock = Picture.LockBits(new Rectangle(0, 0, Picture.Width, Picture.Height), value,
                Picture.PixelFormat);
            }}

        public FastBitMap(Bitmap bmp)
        {
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Indexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Canonical:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    break;
                default:
                    throw new NotSupportedException(bmp.PixelFormat + " is not supported");
            }
            Picture = bmp;
            Format = bmp.PixelFormat;
            PicturePallete = Picture.Palette;
            Width = Picture.Width;
            Height = Picture.Height;
        }

        public bool CanWrite
        {
            get { return LockMode == ImageLockMode.WriteOnly || LockMode == ImageLockMode.ReadWrite; }
            set
            {
                if (value)
                    LockMode = LockMode == ImageLockMode.ReadOnly 
                        ? ImageLockMode.ReadWrite 
                        : ImageLockMode.WriteOnly;
                else
                    LockMode = LockMode == ImageLockMode.ReadWrite
                        ? ImageLockMode.ReadOnly
                        : ImageLockMode.UserInputBuffer;
            }
        }
        public bool CanRead
        {
            get { return LockMode == ImageLockMode.ReadOnly || LockMode == ImageLockMode.ReadWrite; }
            set
            {
                if (value)
                    LockMode = LockMode == ImageLockMode.WriteOnly
                        ? ImageLockMode.ReadWrite
                        : ImageLockMode.ReadOnly;
                else
                    LockMode = LockMode == ImageLockMode.ReadWrite
                        ? ImageLockMode.WriteOnly
                        : ImageLockMode.UserInputBuffer;
            }
        }
        public bool CanReadAndWrite
        {
            get { return LockMode == ImageLockMode.ReadWrite; }
            set { LockMode = value ? ImageLockMode.ReadWrite : ImageLockMode.UserInputBuffer; }
        }

         ~FastBitMap()
        {
            if (CanWrite || CanRead)
                CanReadAndWrite = false;
        }

        public void WritePixel(int x, int y, Color c)
        {
            if (!CanWrite)
                throw new FieldAccessException("Cannot write, set CanWrite = true first");
            WriteColor(PixelAddress(x, y), c);
        }
        public Color ReadPixel(int x, int y)
        {
            if (!CanRead)
                throw new FieldAccessException("Cannot read, set CanRead = true first");
            return ReadColor(PixelAddress(x, y));
        }
        public IntPtr PixelAddress(int x, int y)
        {
            var len = Image.GetPixelFormatSize(Format)/8;
            return BitmapLock.Scan0 + y*BitmapLock.Stride + x*len;
        }
        public unsafe Color ReadColor(IntPtr pos)
        {
            var bytes = (byte*)pos.ToPointer();
            switch (Format)
            {
                case PixelFormat.Indexed:
                case PixelFormat.Format8bppIndexed:
                {
                    var idx = bytes[0];
                    var clr = PicturePallete.Entries[idx];
                    return clr;
                }

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Canonical:
                    {
                        var b = bytes[0];
                        var g = bytes[1];
                        var r = bytes[2];
                        var a = bytes[3];
                        return Color.FromArgb(a, r, g, b);
                    }

                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    {
                        var b = bytes[0];
                        var g = bytes[1];
                        var r = bytes[2];
                        return Color.FromArgb(255, r, g, b);
                    }
            }
            throw new Exception("Fuck : " + Picture.PixelFormat);
            throw new NotSupportedException();
        }
        public unsafe void WriteColor(IntPtr pos, Color clr)
        {
            var len = Image.GetPixelFormatSize(Picture.PixelFormat);
            var bytes = (byte*)pos.ToPointer();
            switch (Format)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Canonical:
                {
                    bytes[0] = clr.B;
                    bytes[1] = clr.G;
                    bytes[2] = clr.R;
                    bytes[3] = clr.A;
                    return;
                }
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                { 
                        // Premultiply alpha?
                        bytes[0] = clr.B;
                        bytes[1] = clr.G;
                        bytes[2] = clr.R;
                        return;
                }
            }
            throw new NotSupportedException();
        }
        public Color this[int x, int y]
        {
            get { return ReadPixel(x, y); }
            set { WritePixel(x, y, value); }
        }

    }
}
