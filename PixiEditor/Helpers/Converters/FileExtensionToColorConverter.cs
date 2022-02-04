using PixiEditor.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Media;
using PixiEditor.Models.Enums;

namespace PixiEditor.Helpers.Converters
{
    public class FileExtensionToColorConverter :
        SingleInstanceConverter<FileExtensionToColorConverter>
    {
        private static readonly Dictionary<string, SolidColorBrush> extensionsToBrushes;
        public static readonly SolidColorBrush UnknownBrush = ColorBrush(100, 100, 100);

        static FileExtensionToColorConverter()
        {
            extensionsToBrushes = new Dictionary<string, SolidColorBrush>();
            AssignFormatToBrush(FileType.Unset, UnknownBrush);
            AssignFormatToBrush(FileType.Pixi, ColorBrush(226, 1, 45));
            AssignFormatToBrush(FileType.Png, ColorBrush(56, 108, 254));
            AssignFormatToBrush(FileType.Jpeg, ColorBrush(36, 179, 66));
            AssignFormatToBrush(FileType.Bmp, ColorBrush(255, 140, 0));
            AssignFormatToBrush(FileType.Gif, ColorBrush(180, 0, 255));
        }
        static void AssignFormatToBrush(FileType format, SolidColorBrush brush)
        {
            SupportedFilesHelper.GetFileTypeDialogData(format).Extensions.ForEach(i => extensionsToBrushes[i] = brush);
        }
        
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extension = (string)value;

            return extensionsToBrushes.ContainsKey(extension) ? extensionsToBrushes[extension] : UnknownBrush;
        }

        private static SolidColorBrush ColorBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
