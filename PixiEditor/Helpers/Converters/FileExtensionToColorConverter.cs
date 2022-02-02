using PixiEditor.Models;
using System;
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

        private static readonly SolidColorBrush UnknownBrush = ColorBrush(100, 100, 100);

        static FileExtensionToColorConverter()
        {
            extensions2Brushes = new Dictionary<string, SolidColorBrush>();
            extensions2Brushes[Constants.NativeExtension] = ColorBrush(226, 1, 45);
            extensions2Brushes[SupportedFilesHelper.Format2Extension(ImageFormat.Png)] = ColorBrush(56, 108, 254);
            extensions2Brushes[SupportedFilesHelper.Format2Extension(ImageFormat.Jpeg)] = ColorBrush(36, 179, 66);
            extensions2Brushes[SupportedFilesHelper.Format2Extension(ImageFormat.Bmp)] = ColorBrush(40, 170, 236);
            extensions2Brushes[SupportedFilesHelper.Format2Extension(ImageFormat.Gif)] = ColorBrush(180, 0, 255);
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
