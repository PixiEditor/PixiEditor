using System.Globalization;
using System.IO;
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

        try
        {
            return LoadBitmapFromRelativePath(path);
        }
        catch (FileNotFoundException)
        {
            return AvaloniaProperty.UnsetValue;
        }
    }

    public static Bitmap LoadBitmapFromRelativePath(string path)
    {
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().FullName}{path}");
        if (!AssetLoader.Exists(uri))
            throw new FileNotFoundException($"Could not find asset with path {path}");

        return new Bitmap(AssetLoader.Open(uri));
    }
}
