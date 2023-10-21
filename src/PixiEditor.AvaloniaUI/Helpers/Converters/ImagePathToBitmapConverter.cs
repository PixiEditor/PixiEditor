using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class ImagePathToBitmapConverter : SingleInstanceConverter<ImagePathToBitmapConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path)
            return AvaloniaProperty.UnsetValue;
        
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().FullName}{path}");
        if (!AssetLoader.Exists(uri))
            return AvaloniaProperty.UnsetValue;
        
        return new Bitmap(AssetLoader.Open(uri));
    }
}
