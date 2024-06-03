using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.Extensions;

public static class ImageSourceExtensions
{
    public static ImageSource ToImageSource(this string url, ImageSource defaultImageSource = null, object parameter = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            if (parameter is Binding { Source: View view } binding)
                parameter = view.BindingContext.GetValue(binding.Path);

            if (parameter is Binding _binding)
                parameter = _binding.Source.GetValue(_binding.Path);

            if (parameter is byte[] _data)
                return ImageSource.FromStream(() => new MemoryStream(_data));

            if (parameter is ImageSource _source)
                return _source;

            if (parameter is string str)
                url = str;
            else
                return defaultImageSource;
        }

        if (url.IsValidUrl())
            return new UriImageSource { Uri = new Uri(url), CacheValidity = new TimeSpan(10, 0, 0, 0) };

        if (url.IsBase64(out var stream))
            return ImageSource.FromStream(() => stream);

        if(File.Exists("./" + url))
            return ImageSource.FromFile(url);

        try
        {
            var assembly = Application.Current?.GetType().Assembly;

            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            var matches = assembly?.GetManifestResourceNames().Where(str => str.EndsWith("." + url.Replace("/", ".")));

            if (matches?.Count() > 1)
                throw new Exception($"File name '{url}' matches multiple resources!!");

            var resourcePath = matches.FirstOrDefault();

            if (!string.IsNullOrEmpty(resourcePath))
                return ImageSource.FromResource(resourcePath);

            return ImageSource.FromFile(url);
        }
        catch
        {
            //Ignore
        }

        return defaultImageSource;
    }
}
