using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

public static class GifCreator
{
    public static void CreateGif(List<Bitmap> bitmaps, string outputPath, int delay = 500)
    {
        using (var gifImage = new Image<Rgba32>(bitmaps[0].Width, bitmaps[0].Height))
        {
            //var gifMetadata = gifImage.Metadata.GetGifMetadata();
            //gifMetadata.RepeatCount = 0; // Infinite loop

            foreach (var bitmap in bitmaps)
            {
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Position = 0;

                    using (var frame = SixLabors.ImageSharp.Image.Load<Rgba32>(memoryStream))
                    {
                        var gifFrameMetadata = frame.Frames.RootFrame.Metadata.GetGifMetadata();
                        gifFrameMetadata.FrameDelay = delay / 10; // delay in 1/100th seconds
                        gifImage.Frames.AddFrame(frame.Frames.RootFrame);
                    }
                }
            }

            gifImage.Metadata.GetGifMetadata().RepeatCount = 0; // Infinite loop
            gifImage.Save(outputPath, new SixLabors.ImageSharp.Formats.Gif.GifEncoder());
        }
    }
}
