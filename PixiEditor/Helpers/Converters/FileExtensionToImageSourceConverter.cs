using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class FileExtensionToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extension = (string)value;

            if (extension == ".pixi")
            {
                return Join("PixiFile.png", parameter);
            }
            else if (extension == ".png")
            {
                return Join("PngFile.png", parameter);
            }
            else if (extension == ".jpg" || extension == ".jpeg")
            {
                return Join("JpgFile.png", parameter);
            }

            return Join("UnknownFile.png", parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string Join(string path, object parameter)
        {
            return Path.Join((string)parameter, "Images", path);
        }
    }
}