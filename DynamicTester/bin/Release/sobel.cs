public static void main(int x, int y, ImageProcessingThreadContext context)
{
	if (x == 0 || y == 0 || x == context.Original.Width - 1 || y == context.Original.Height - 1)
	{
		context.Result[x, y] = Color.FromArgb(0, Color.Black);
		return;
	}
	var sourceColor = context.Original[x, y];
	var P1 = context.Original[x-1,y-1].GetBrightness();
	var P2 = context.Original[x  ,y-1].GetBrightness();
	var P3 = context.Original[x+1,y-1].GetBrightness();
	var P4 = context.Original[x+1,y  ].GetBrightness();
	var P5 = context.Original[x+1,y+1].GetBrightness();
	var P6 = context.Original[x  ,y-1].GetBrightness();
	var P7 = context.Original[x-1,y+1].GetBrightness();
	var P8 = context.Original[x-1,y  ].GetBrightness();
	var Gx = (P1 + 2*P2 + P3 - P7 - 2*P6 - P5+3)/6;
	var Gy = (P3 + 2*P4 + P5 - P1 - 2*P8 - P7+3)/6;
	var G = (Gx*Gx + Gy*Gy)/2;
	var bG = (byte)(G*G*255);
	//context.Result[x, y] = (G > 0.35) ? Color.Black : Color.White;
	context.Result[x, y] = Color.FromArgb(255 - bG, 255-bG, 255-bG, 255-bG).Lerp(sourceColor, G);
}