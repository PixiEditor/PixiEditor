using System;
using System.Globalization;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    public class FileExtensionToColorConverter :
        SingleInstanceConverter<FileExtensionToColorConverter>
    {
        private static readonly SolidColorBrush PixiBrush = ColorBrush(226, 1, 45);

        private static readonly SolidColorBrush PngBrush = ColorBrush(56, 108, 254);

        private static readonly SolidColorBrush JpgBrush = ColorBrush(36, 179, 66);

        private static readonly SolidColorBrush UnknownBrush = ColorBrush(100, 100, 100);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extension = (string)value;

            if (extension == ".pixi")
            {
                return PixiBrush;
            }
            else if (extension == ".png")
            {
                return PngBrush;
            }
            else if (extension is ".jpg" or ".jpeg")
            {
                return JpgBrush;
            }

            return UnknownBrush;
        }

        private static SolidColorBrush ColorBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
