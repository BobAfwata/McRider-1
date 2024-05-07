using System.Drawing;

namespace McRider.Common.Extensions;

public static class RectangleExtensions
{
    public static PointF MidLeft(this RectangleF rec)
    {
        float x = rec.Left;
        float y = rec.Top + rec.Height / 2f;
        return new PointF(x, y);
    }

    public static PointF MidRight(this RectangleF rec)
    {
        float x = rec.Right;
        float y = rec.Top + rec.Height / 2f;
        return new PointF(x, y);
    }
}
