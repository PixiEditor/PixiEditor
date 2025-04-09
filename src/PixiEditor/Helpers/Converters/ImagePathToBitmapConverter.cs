using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using PixiEditor.Helpers.Extensions;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace PixiEditor.Helpers.Converters;

internal class ImagePathToBitmapConverter : SingleInstanceConverter<ImagePathToBitmapConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path)
            return AvaloniaProperty.UnsetValue;

        try
        {
            return LoadImageFromRelativePath(path);
        }
        catch (FileNotFoundException)
        {
            return AvaloniaProperty.UnsetValue;
        }
    }

    public static IImage LoadImageFromRelativePath(string path)
    {
        Uri baseUri = new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}");
        Uri uri = new(baseUri, path);
        if (!AssetLoader.Exists(uri))
            throw new FileNotFoundException($"Could not find asset with path {path}");

        if (path.EndsWith(".svg"))
        {
            return new SvgImage() { Source = new SvgSource(baseUri) { Path = path } };
        }

        return new Bitmap(AssetLoader.Open(uri));
    }

    public static Bitmap LoadBitmapFromRelativePath(string path)
    {
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().FullName}{path}");
        if (!AssetLoader.Exists(uri))
            throw new FileNotFoundException($"Could not find asset with path {path}");

        return new Bitmap(AssetLoader.Open(uri));
    }

    public static Drawie.Backend.Core.Surfaces.Bitmap LoadDrawingApiBitmapFromRelativePath(string path)
    {
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().FullName}{path}");
        if (!AssetLoader.Exists(uri))
            throw new FileNotFoundException($"Could not find asset with path {path}");

        return BitmapExtensions.FromStream(AssetLoader.Open(uri));
    }

    public static Bitmap? TryLoadBitmapFromRelativePath(string path)
    {
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().FullName}{path}");
        return !AssetLoader.Exists(uri) ? null : new Bitmap(AssetLoader.Open(uri));
    }
}
