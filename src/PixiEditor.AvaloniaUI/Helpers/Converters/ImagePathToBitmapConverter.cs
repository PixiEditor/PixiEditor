using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class ImagePathToBitmapConverter : SingleInstanceConverter<ImagePathToBitmapConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            return new Bitmap(AssetLoader.Open(new Uri($"avares://{Assembly.GetExecutingAssembly().FullName}{path}")));
        }

        return null;
    }
}
