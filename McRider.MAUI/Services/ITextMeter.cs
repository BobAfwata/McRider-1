namespace McRider.MAUI.Services;

public interface ITextMeter
{
    System.Drawing.SizeF MeasureText(string text, float fontSize, string fontFamily);
}
