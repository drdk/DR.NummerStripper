using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
namespace DR.NummerStripper
{
    internal static class IconFactory
    {public static Icon MakeOne(char x, Brush color)
        {
            var canvas = new Bitmap(32,32, PixelFormat.Format32bppArgb);

            var rectf = new RectangleF(0, 0, 32, 32);
            var rectfShadow = new RectangleF(1, 1, 32, 32);
            var g = Graphics.FromImage(canvas);
            var font = new Font("Segoe", 20);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawString($"{x}", font, Brushes.DimGray, rectfShadow);
            g.DrawString($"{x}", font, color, rectf);
            g.Flush();

            var iconResult = Icon.FromHandle(canvas.GetHicon());
            return iconResult;
        }
    }
}
