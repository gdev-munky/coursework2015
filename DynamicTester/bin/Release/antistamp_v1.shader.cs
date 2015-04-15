
        private const float CHROMATICITY_MIN = 0.2f;
        private const float CHROMATICITY_MAX = 0.3f;
        private static readonly Color BackColor = Color.White;
        private static float sqrt2 = (float)System.Math.Sqrt(2);

        public static void main(int x, int y, ImageProcessingThreadContext context)
        {
            if (x == 0 || y == 0 || x == context.Original.Width - 1 || y == context.Original.Height - 1)
            {
                context.Result[x, y] = Color.FromArgb(0, Color.Black);
                return;
            }
            var sourceColor = context.Original[x, y];
            var chroma = sourceColor.CalculateChromaticity();
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
            }/*
            var cx = 0.0f; var cy = 0.0f;
            var bx = 0.0f; var by = 0.0f;
            for (var dx = -1; dx <= 1; ++dx)
                for (var dy = -1; dy <= 1; ++dy)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    var clr = context.Original[x + dx, y + dy];
                    var chr = clr.CalculateChromaticity();
                    var brs = clr.GetBrightness();
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
            if (System.Math.Abs(cx) > 0.9)
                k /= 5;
            if (System.Math.Abs(cy) > 0.9)
                k /= 5;
            if (System.Math.Abs(bx) > 0.9)
                k /= 5;
            if (System.Math.Abs(by) > 0.9)
                k /= 5;*/
            context.Result[x, y] = Color.FromArgb(255 - br, br, br, br).Lerp(Color.FromArgb(0, BackColor), k);
        }