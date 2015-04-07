using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing
{
    public class ColorImage
    {
        public ColorImage(Bitmap bmp)
        {
            Map = new Color[bmp.Width, bmp.Height];
            var rect = new Rectangle(0, 0, Width, Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            var pixelSize = Image.GetPixelFormatSize(bmpData.PixelFormat)/8;

            unsafe
            {
                var w = bmp.Width;
                var h = bmp.Height;
                var ptr = (byte*)bmpData.Scan0;
                var format = bmpData.PixelFormat;
                var palette = bmp.Palette;

                for (var y = 0; y < h; ++y)
                {
                    var dy = y*bmpData.Stride;
                    for (var x = 0; x < w; ++x)
                        Map[x, y] = ReadColor(palette, ptr, x * pixelSize + dy, format);
                }
            }
            bmp.UnlockBits(bmpData);
        }

        public ColorImage(int width, int height)
        {
            Map = new Color[width, height];
        }

        public int Width { get { return Map.GetLength(0); } }
        public int Height { get { return Map.GetLength(1); } }
        protected Color[,] Map { get; private set; }

        public Color this[int x, int y]
        {
            get { return Map[x, y]; }
            set { Map[x, y] = value; }
        }

        /// <summary>
        /// !!! Assuming BMP is 32bpp argb and has the same size
        /// </summary>
        /// <param name="bmp"></param>
        public void DumpToBitmap(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var ptrBase = bmpData.Scan0;
            unsafe
            {
                for (var y = 0; y < bmp.Height; y++)
                {
                    var ptr = ptrBase + (y * bmpData.Stride);
                    for (var x = 0; x < bmp.Width; x++)
                    {
                        var pos = ptr + (x << 2);
                        *(int*)pos = Map[x, y]/*.IntColor*/.ToArgb();
                    }
                }
            }
            bmp.UnlockBits(bmpData);
        }

        public static Color ReadColor(Bitmap bmp, byte[] bytes, int pos, PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Indexed:
                case PixelFormat.Format8bppIndexed:
                    return /*new FloatColor(*/bmp.Palette.Entries[bytes[pos]]/*)*/;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Canonical:
                {
                    var b = bytes[pos + 0]/* / 255f*/;
                    var g = bytes[pos + 1]/* / 255f*/;
                    var r = bytes[pos + 2]/* / 255f*/;
                    var a = bytes[pos + 3]/* / 255f*/;
                    return /*new FloatColor*/Color.FromArgb(a, r, g, b);
                }

                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    {/*
                        var b = bytes[pos + 0]/255f;
                        var g = bytes[pos + 1] / 255f;
                        var r = bytes[pos + 2] / 255f;
                        return new FloatColor(1, r, g, b);*/
                        var b = bytes[pos + 0]/* / 255f*/;
                        var g = bytes[pos + 1]/* / 255f*/;
                        var r = bytes[pos + 2]/* / 255f*/;
                        return /*new FloatColor*/Color.FromArgb(255, r, g, b);
                    }

                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Max:
                default:
                    throw new NotSupportedException();
            }
        }
        public unsafe static Color ReadColor(ColorPalette palette, byte* bytes, int pos, PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Indexed:
                case PixelFormat.Format8bppIndexed:
                    return palette.Entries[bytes[pos]];

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Canonical:
                    {
                        var b = bytes[pos];
                        var g = bytes[pos + 1];
                        var r = bytes[pos + 2];
                        var a = bytes[pos + 3];
                        return Color.FromArgb(a, r, g, b);
                    }

                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    {
                        var b = bytes[pos];
                        var g = bytes[pos + 1];
                        var r = bytes[pos + 2];
                        return Color.FromArgb(255, r, g, b);
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
