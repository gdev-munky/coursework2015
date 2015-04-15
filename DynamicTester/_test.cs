using System;
using System.Drawing;
using ImageProcessing;

namespace DynamicTester
{
    public class ShaderClass
    {
        private const float CHROMATICITY_MIN = 0.1f;
        private const float CHROMATICITY_MAX = 0.3f;
        private static readonly Color BackColor = Color.White;
        private static float sqrt2 = (float)System.Math.Sqrt(2);
        public static void main(int x, int y, ImageProcessingThreadContext context)
        {
            var sourceColor = context.Original[x, y];
            var chroma = sourceColor.CalculateChromaticity();
            if (x == 0 || y == 0 || x == context.Original.Width - 1 || y == context.Original.Height - 1)
            {
                context.Result[x, y] = Color.FromArgb(0, Color.Black);
                return;
            }
            if (chroma <= CHROMATICITY_MIN)
            {
                var br = (float)sourceColor.GetBrightness();
                var cbr = (byte) (br*255);
                context.Result[x, y] = Color.FromArgb(cbr, cbr, cbr);
                return;
            }
            var k = 1.0f;
            if (chroma < CHROMATICITY_MAX)
            {
                k = (CHROMATICITY_MAX - chroma) / (CHROMATICITY_MAX - CHROMATICITY_MIN);
            }
            var nb = new Color[3, 3];
            var nbc = new float[3, 3];
            var nbb = new float[3, 3];
            var cx = 0.0f; var cy = 0.0f;
            var bx = 0.0f; var by = 0.0f;
            for (var dx = -1; dx <= 1; ++dx)
                for (var dy = -1; dy <= 1; ++dy)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    var clr = nb[dx + 1, dy + 1] = context.Original[x + dx, y + dy];
                    var chr = nbc[dx + 1, dy + 1] = clr.CalculateChromaticity();
                    var brs = nbb[dx + 1, dy + 1] = clr.GetBrightness();
                    var len = (float)System.Math.Sqrt(dx * dx + dy * dy);
                    cx += dx * chr / len;
                    cy += dy * chr / len;
                    bx += dx * brs / len;
                    by += dy * brs / len;
                }
            cx /= 3;
            cy /= 3;
            bx /= 3;
            by /= 3;
            var t = ((1 - nbc[0, 1])*nbb[0, 1] + (1 - nbc[2, 1])*nbb[2, 1] + (1 - nbc[1, 0])*nbb[1, 0] +
                     (1 - nbc[1, 2])*nbb[1, 2]);
            var A = (byte) (t*(cx*bx + cy*by)*255);
            context.Result[x, y] = Color.FromArgb(A, A, A, A);
            //context.Result[x, y] = Color.FromArgb((byte)((cx + 1) / 2), (byte)((cy + 1) / 2), (byte)((bx + 1) / 2), (byte)((by + 1) / 2));
            //context.Result[x, y] = context.Original[x + (int)cx, y + (int)cy].Lerp(BackColor, k);
            /*
            cx = (256 + cx * 255 ) / 2;
            cy = (256 + cy * 255 ) / 2;
            context.Result[x, y] = Color.FromArgb(255, (byte)cx, (byte)cy, (int)(chroma * 255));*/
        }
    }
}