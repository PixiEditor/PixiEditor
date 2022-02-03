using PixiEditor.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Media;

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
            AssignFormat2Brush(Constants.NativeExtension, ColorBrush(226, 1, 45));
            AssignFormat2Brush(ImageFormat.Png, ColorBrush(56, 108, 254));
            AssignFormat2Brush(ImageFormat.Jpeg, ColorBrush(36, 179, 66));
            AssignFormat2Brush(ImageFormat.Bmp, ColorBrush(40, 170, 236));
            AssignFormat2Brush(ImageFormat.Gif, ColorBrush(180, 0, 255));
        }
        static void AssignFormat2Brush(ImageFormat format, SolidColorBrush brush)
        {
            SupportedFilesHelper.GetFormatExtensions(format).ForEach(i => AssignFormat2Brush(i, brush));
        }
        static void AssignFormat2Brush(string format, SolidColorBrush brush)
        {
            extensions2Brushes[format] = brush;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extension = (string)value;

            if (extensions2Brushes.ContainsKey(extension))
                return extensions2Brushes[extension];

            return UnknownBrush;
        }

        private static SolidColorBrush ColorBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
