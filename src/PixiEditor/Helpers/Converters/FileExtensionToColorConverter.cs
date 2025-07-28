using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Media;
using PixiEditor.Models.Files;

namespace PixiEditor.Helpers.Converters;

internal class FileExtensionToColorConverter :
    SingleInstanceConverter<FileExtensionToColorConverter>
{
    private static readonly Dictionary<string, SolidColorBrush> extensionsToBrushes;
    public static readonly SolidColorBrush UnknownBrush = ColorBrush(100, 100, 100);

    static FileExtensionToColorConverter()
    {
        extensionsToBrushes = new Dictionary<string, SolidColorBrush>();

        foreach (var fileTypes in SupportedFilesHelper.FileTypes)
        {
            foreach (var ext in fileTypes.Extensions)
            {
                extensionsToBrushes[ext] = fileTypes.EditorColor;   
            }
        }
    }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        GetBrush((string)value);

    public static Brush GetBrush(string path)
    {
        return extensionsToBrushes.GetValueOrDefault(Path.GetExtension(path).ToLower(), UnknownBrush);
    }

    private static SolidColorBrush ColorBrush(byte r, byte g, byte b)
    {
        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }
}
