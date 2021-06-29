using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;

namespace PixiEditor.Helpers.Converters
{
    public class IndexOfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Layer layer && ViewModelMain.Current.BitmapManager.ActiveDocument != null)
            {
                return ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.IndexOf(layer);
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}