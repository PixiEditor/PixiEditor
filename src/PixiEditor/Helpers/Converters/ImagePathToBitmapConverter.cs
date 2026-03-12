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
            return LoadImage(path) ?? AvaloniaProperty.UnsetValue;
        }
        catch (FileNotFoundException)
        {
            return AvaloniaProperty.UnsetValue;
        }
    }

    public static IImage? LoadImage(string path)
    {
        Uri baseUri = new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}");
        if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out _))
        {
            bool isAbsolute = Uri.TryCreate(path, UriKind.Absolute, out Uri? absUri);
            
            Uri assetsUri = new Uri(baseUri, path);
            
            if (AssetLoader.Exists(assetsUri))
            {
                if (path.EndsWith(".svg"))
                {
                    return new SvgImage() { Source = new SvgSource(baseUri) { Path = path } };
                }

                return new Bitmap(AssetLoader.Open(assetsUri));
            }

            if (isAbsolute && File.Exists(absUri!.LocalPath))
            {
                if (path.EndsWith(".svg"))
                {
                    return new SvgImage() { Source = new SvgSource((Uri?)null) { Path = absUri.LocalPath} };
                }

                return new Bitmap(File.OpenRead(absUri!.LocalPath));
            }

           
        }

        return null;
    }

    public static Bitmap LoadBitmapFromRelativePath(string path)
    {
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().FullName}{path}");
        if (!AssetLoader.Exists(uri))
            throw new FileNotFoundException($"Could not find asset with path {path}");

        return new Bitmap(AssetLoader.Open(uri));
    }

    public static Bitmap? TryLoadBitmapFromRelativePath(string path)
    {
        Uri uri = new($"avares://{Assembly.GetExecutingAssembly().FullName}{path}");
        return !AssetLoader.Exists(uri) ? null : new Bitmap(AssetLoader.Open(uri));
    }
}
