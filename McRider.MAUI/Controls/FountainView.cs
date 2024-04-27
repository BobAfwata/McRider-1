using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System;

namespace McRider.MAUI.Controls;

public class FountainView : GraphicsView
{
    private const double Gravity = 9.8;
    private const double InitialVelocity = 20;
    private const double SplitHeight = 200; // Adjust as necessary
    private const double DropRadius = 5;

    public FountainView()
    {
        FountainDrawable fountainDrawable = new FountainDrawable();

        Drawable = fountainDrawable;
        Device.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        {
            fountainDrawable.ElaspedTime += 0.016;
            Invalidate();
            return true;
        });
    }

    class FountainDrawable : IDrawable
    {
        public double ElaspedTime { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Blue;

            double x = dirtyRect.Width / 2;
            double y = dirtyRect.Height;

            // Calculate the current height of the drop
            double height = y - (InitialVelocity * ElaspedTime - 0.5 * Gravity * ElaspedTime * ElaspedTime);

            // Draw the drop moving upwards
            if (height > y - SplitHeight)
            {
                canvas.FillCircle((float)x, (float)height, (float)DropRadius);
            }
            else
            {
                // Draw the drops splitting and falling
                double angle = Math.PI / 4; // 45 degrees to left and right
                double splitTime = ElaspedTime - Math.Sqrt(2 * SplitHeight / Gravity);
                double leftX = x - InitialVelocity * splitTime * Math.Cos(angle);
                double rightX = x + InitialVelocity * splitTime * Math.Cos(angle);
                double splitY = y - SplitHeight + InitialVelocity * splitTime * Math.Sin(angle) - 0.5 * Gravity * splitTime * splitTime;

                canvas.FillCircle((float)leftX, (float)splitY, (float)DropRadius);
                canvas.FillCircle((float)rightX, (float)splitY, (float)DropRadius);
            }
        }
    }
}
