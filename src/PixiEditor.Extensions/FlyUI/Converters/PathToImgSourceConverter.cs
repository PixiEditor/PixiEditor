using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
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
            return GetImageFromPath(path, ResourceStorage);

        return null;
    }

    public static IImage? GetImageFromPath(string path, IResourceStorage storage)
    {
        if (storage != null)
        {
            if (storage.Exists(path))
            {
                bool isSvg = path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
                if (isSvg)
                {
                    var source = SvgSource.LoadFromStream(storage.GetResourceStream(path));
                    return new SvgImage() { Source = source };
                }

                return new Bitmap(storage.GetResourceStream(path));
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

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
