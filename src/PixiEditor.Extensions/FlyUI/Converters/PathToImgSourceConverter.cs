using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using PixiEditor.Extensions.IO;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class PathToImgSourceConverter : IValueConverter
{
    public IResourceStorage? ResourceStorage { get; set; }
    public PathToImgSourceConverter()
    {

    }

    public PathToImgSourceConverter(IResourceStorage resourceStorage)
    {
        ResourceStorage = resourceStorage;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            if (ResourceStorage != null)
            {
                if(ResourceStorage.Exists(path))
                {
                    bool isSvg = path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
                    if (isSvg)
                    {
                        return new SvgImage { Source = SvgSource.LoadFromStream(ResourceStorage.GetResourceStream(path)) };
                    }

                    return new Bitmap(ResourceStorage.GetResourceStream(path));
                }
            }
            else if (File.Exists(path))
            {
                bool isSvg = path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
                if (isSvg)
                {
                    return new SvgImage { Source = SvgSource.Load(path) };
                }

                return new Bitmap(path);
            }
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
