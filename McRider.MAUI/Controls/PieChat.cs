using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace McRider.MAUI.Controls;

public class PieChart : GraphicsView, IDrawable
{
    private static ILogger _logger = App.ServiceProvider.GetService<ILogger<PieChart>>();

    public static readonly BindableProperty DataProperty = BindableProperty.Create(
        nameof(Data), typeof(ObservableCollection<PieChartData>), typeof(PieChart), null,
            propertyChanged: (a, b, c) => (a as PieChart)?.Invalidate());

    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(
        nameof(Thickness), typeof(int), typeof(PieChart), 10,
            propertyChanged: (a, b, c) => (a as PieChart)?.Invalidate());

    public static readonly BindableProperty StartAngleProperty = BindableProperty.Create(
        nameof(StartAngle), typeof(float), typeof(PieChart), 90F,
            propertyChanged: (a, b, c) => (a as PieChart)?.Invalidate());

    public PieChart()
    {
        Data = [];
        _ = Task.Run(() => Drawable = this);        
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
    }

    public new void Invalidate()
    {
        if (Data != null)
        {
            foreach (var item in Data)
            {
                item.Parent = item.Parent ?? this;
                item.BindingContext = item.BindingContext ?? this.BindingContext;
            }
        }

        Handler?.Invoke(nameof(IGraphicsView.Invalidate));
    }

    public ObservableCollection<PieChartData> Data
    {
        get => (ObservableCollection<PieChartData>)GetValue(DataProperty);
        set
        {
            SetValue(DataProperty, value);

            if (value != null)
                value.CollectionChanged += (s, e) => Invalidate();

            Invalidate();
        }
    }

    public int Thickness
    {
        get => (int)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public float StartAngle
    {
        get => (float)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public virtual int TotalThickness => Thickness;


    public virtual void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Data?.Any() != true)
            return;

        foreach (var item in Data)
        {
            item.Parent = item.Parent ?? this;
            item.BindingContext = item.BindingContext ?? this.BindingContext;
        }

        // Effective Thickness
        var effectiveThickness = TotalThickness;

        // Get the center point of the canvas
        var innerRadius = GetCalculatedInnerRadius(dirtyRect.Width, dirtyRect.Height, effectiveThickness);
        var origin = GetCalculatedOrigin(dirtyRect.Width, dirtyRect.Height, innerRadius, effectiveThickness);
        var center = GetCalculatedCenter(origin, innerRadius, effectiveThickness);

        // Width of the Gauge
        var effectiveSize = 2 * (innerRadius + effectiveThickness / 2);

        // 
        var totalValue = Data.Sum(item => item.Value ?? 0F);
        var startAngle = NormalizeAngle(0F + StartAngle);
        var count = Data.Count();

        if (Data.Count() == 1 || totalValue < 100)
            totalValue = Math.Max(100, totalValue);

        // 
        canvas.StrokeSize = Thickness;

        // Pie Chart Background
        if (BackgroundColor != null)
        {
            canvas.StrokeColor = BackgroundColor;
            canvas.DrawEllipse(origin.X, origin.Y, effectiveSize, effectiveSize);
        }

        // Draw each Pie section
        for (int i = 0; i < count; i++)
        {
            var dataItem = Data.ElementAt(i);

            // Calculate the sweep angle for the current slice
            var sweepAngle = 360F * (dataItem.Value ?? 0F) / totalValue;
            var endAngle = NormalizeAngle(startAngle - sweepAngle);

            if (dataItem.IsVisible && dataItem.Value > 0)
            {
                // Current slice color
                var color = dataItem.BackgroundColor ?? GetColor(i);

                canvas.StrokeColor = color;
                canvas.FillColor = GetLighterColor(color);

                if (startAngle == endAngle)
                    endAngle = NormalizeAngle(endAngle + 0.01F);

                // Draw the pie slice
                canvas.DrawArc(origin.X, origin.Y, effectiveSize, effectiveSize, startAngle, endAngle, true, false);
            }
            else
            {
                _logger.LogInformation($"Skipping Arc. Is index:{i}, startAngle:{startAngle}, endAngle:{endAngle}");
            }

            // Update startAngle
            startAngle = endAngle;
        }
    }

    protected PointF GetCalculatedCenter(PointF origin, float innerRadius, float? thickness = null)
    {
        var halfThickness = (thickness ?? TotalThickness) / 2;
        return new PointF(origin.X + innerRadius + halfThickness, origin.Y + innerRadius + halfThickness);
    }

    protected float GetCalculatedOutterRadius(float? width = null, float? height = null)
    {
        float defaultRadius = 50;

        width ??= (float)Width;
        height ??= (float)Height;

        return Math.Max(Math.Min(width.Value / 2, height.Value / 2), defaultRadius);
    }

    protected float GetCalculatedInnerRadius(float? width = null, float? height = null, float? thickness = null)
    {
        return (float)GetCalculatedOutterRadius(width, height) - (thickness ?? TotalThickness);
    }

    protected PointF GetCalculatedOrigin(float? width = null, float? height = null, float? innerRadius = null, float? thickness = null)
    {
        innerRadius ??= GetCalculatedInnerRadius();
        thickness ??= TotalThickness;

        width ??= (float)Width;
        height ??= (float)Height;

        var x = (width.Value - thickness.Value) / 2 - innerRadius.Value;
        var y = (height.Value - thickness.Value) / 2 - innerRadius.Value;

        if (width > height)
            return new(thickness.Value / 2, y);

        return new(x, thickness.Value / 2);
    }

    protected Color GetColor(int index)
    {
        // Provide a simple way to get colors.
        var colors = new[]
        {
            Colors.Blue, Colors.Orange, Colors.Green,
            Colors.Red, Colors.Purple, Colors.Yellow,
            Colors.Cyan, Colors.Magenta, Colors.Brown,
            Colors.Pink, Colors.Teal, Colors.Indigo,
            Colors.Lime, Colors.Silver, Colors.Gold
        };

        return colors[index % colors.Length];
    }

    protected static float NormalizeAngle(float angle)
    {
        // Normalize angle to the range [0, 360)
        return (angle % 360 + 360) % 360;
    }

    protected static Color GetLighterColor(Color color, float factor = 0.2f)
    {
        // Ensure the factor is within a valid range
        factor = Math.Clamp(factor, 0f, 1f);

        // Adjust RGB values to make the color lighter
        float red = color.Red + (1 - color.Red) * factor;
        float green = color.Green + (1 - color.Green) * factor;
        float blue = color.Blue + (1 - color.Blue) * factor;

        // Ensure the values are within the valid range [0, 1]
        red = Math.Clamp(red, 0f, 1f);
        green = Math.Clamp(green, 0f, 1f);
        blue = Math.Clamp(blue, 0f, 1f);

        return new Color(red, green, blue, color.Alpha);
    }
}

public class PieChartData : VisualElement
{
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(PieChartData), String.Empty,
        propertyChanged: (a, b, c) => ((a as PieChartData).Parent as PieChart)?.Invalidate());

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(float), typeof(PieChartData), null,
        propertyChanged: (a, b, c) => ((a as PieChartData).Parent as PieChart)?.Invalidate());

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public float? Value
    {
        get => (float?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}