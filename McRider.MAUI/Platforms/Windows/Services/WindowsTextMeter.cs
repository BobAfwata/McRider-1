using McRider.MAUI.Services;
using System.Drawing;

namespace McRider.MAUI.Platforms.Windows.Services
{
    public class WindowsTextMeter : ITextMeter
    {
        System.Drawing.SizeF ITextMeter.MeasureText(string text, float fontSize, string fontFamily)
        {
            using (var bitmap = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var font = new System.Drawing.Font(fontFamily, fontSize);
                var sizeF = graphics.MeasureString(text, font);
                return new System.Drawing.SizeF((int)sizeF.Width, (int)sizeF.Height);
            }
        }
    }
}
