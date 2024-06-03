using CommunityToolkit.Maui.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.Converters;

public class ThemeImageSourceConverter : BaseConverterOneWay<object, ImageSource, object>
{
    public static string? Theme { get; set; } = "schweppes";

    public override ImageSource DefaultConvertReturnValue { get; set; } = new UriImageSource
    {
        Uri = new Uri("https://via.placeholder.com/350x260"),
        CacheValidity = new TimeSpan(10, 0, 0, 0)
    };

    public override ImageSource ConvertFrom(object value, object parameter, CultureInfo? culture)
    {
        if (value is byte[] data)
            return ImageSource.FromStream(() => new MemoryStream(data));

        if (value is ImageSource source)
            return source;

        var url = value?.ToString() ?? "";

        var parts = url.Split("/", StringSplitOptions.RemoveEmptyEntries);

        // Check if the url is a theme url
        if (parts.Length >= 2 && parts[parts.Length - 2] == "theme")
        {
            parts[parts.Length - 2] = "Themes/" + Theme;
            url = string.Join("/", parts);
        }

        return url.ToImageSource(DefaultConvertReturnValue, parameter);
    }
}
