using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class PathToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            if (File.Exists(path))
            {
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
