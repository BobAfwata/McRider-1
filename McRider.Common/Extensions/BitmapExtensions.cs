using System.Drawing;

namespace McRider.Common.Extensions;

public static class BitmapExtensions
{
    public static Stream ToStream(this Bitmap bitmap)
    {
        // Create a new memory stream
        MemoryStream stream = new MemoryStream();

        // Save the bitmap to the stream
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

        // Reset the position of the stream to the beginning
        stream.Position = 0;

        // Return the stream
        return stream;
    }
}
