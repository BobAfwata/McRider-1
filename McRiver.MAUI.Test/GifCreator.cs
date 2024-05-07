using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;

public class GifCreator
{
    public static void CreateGif(List<Bitmap> bitmaps, string outputPath, int delay = 500)
    {
        if (bitmaps == null || bitmaps.Count == 0)
            throw new ArgumentException("Bitmaps list is null or empty.");

        var encoder = GetEncoder(ImageFormat.Gif);
        var encoderParameters = new EncoderParameters(1)
        {
            Param = new[] { new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame) }
        };

        using (var gifEncoder = new FileStream(outputPath, FileMode.Create))
        {
            bitmaps[0].Save(gifEncoder, encoder, encoderParameters);

            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionTime);

            for (int i = 1; i < bitmaps.Count; i++)
            {
                var frameDelay = BitConverter.GetBytes(delay / 10);
                var delayProperty = new byte[4];

                frameDelay.CopyTo(delayProperty, 0);

                bitmaps[i].SetPropertyItem(CreatePropertyItem(PropertyTagFrameDelay, delayProperty));
                bitmaps[0].SaveAdd(bitmaps[i], encoderParameters);
            }

            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
            bitmaps[0].SaveAdd(encoderParameters);
        }
    }

    private static PropertyItem CreatePropertyItem(int id, byte[] value)
    {
        var propertyItem = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
        propertyItem.Id = id;
        propertyItem.Type = 4;
        propertyItem.Len = value.Length;
        propertyItem.Value = value;
        return propertyItem;
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        return Array.Find(ImageCodecInfo.GetImageDecoders(), codec => codec.FormatID == format.Guid);
    }

    private const int PropertyTagFrameDelay = 0x5100;
}
