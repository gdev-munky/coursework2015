using System;
using System.Drawing;

namespace ImageProcessing
{
    public static class ColorExtensions
    {
        public static Color Lerp(this Color s, Color t, float k)
        {
            var bk = (1 - k);
            var a = s.A * bk + t.A * k;
            var r = s.R * bk + t.R * k;
            var g = s.G * bk + t.G * k;
            var b = s.B * bk + t.B * k;
            return Color.FromArgb((int) a, (int) r, (int) g, (int) b);
        }

        public static float CalculateChromaticity(this Color fc)
        {
            var rg = Math.Abs(fc.R - fc.G);
            var gb = Math.Abs(fc.G - fc.B);
            var rb = Math.Abs(fc.R - fc.B);
            return Math.Max(rg, Math.Max(gb, rb)) / 255.0f;
        }
    }
}
