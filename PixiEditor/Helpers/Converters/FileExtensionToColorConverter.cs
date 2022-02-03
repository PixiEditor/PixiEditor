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
        private static readonly Dictionary<string, SolidColorBrush> extensions2Brushes;
        public static readonly SolidColorBrush UnknownBrush = ColorBrush(100, 100, 100);

        static FileExtensionToColorConverter()
        {
            extensions2Brushes = new Dictionary<string, SolidColorBrush>();
            AssignFormat2Brush(FileType.Unset, UnknownBrush);
            AssignFormat2Brush(FileType.Pixi, ColorBrush(226, 1, 45));
            AssignFormat2Brush(FileType.Png, ColorBrush(56, 108, 254));
            AssignFormat2Brush(FileType.Jpeg, ColorBrush(36, 179, 66));
            AssignFormat2Brush(FileType.Bmp, ColorBrush(40, 170, 236));
            AssignFormat2Brush(FileType.Gif, ColorBrush(180, 0, 255));
        }
        static void AssignFormat2Brush(FileType format, SolidColorBrush brush)
        {
            SupportedFilesHelper.GetFileTypeDialogData(format).Extensions.ForEach(i => extensions2Brushes[i] = brush);
        }
        
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extension = (string)value;

            return extensions2Brushes.ContainsKey(extension) ? extensions2Brushes[extension] : UnknownBrush;
        }

        private static SolidColorBrush ColorBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
