using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class IndexOfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Layer layer && ViewModelMain.Current.BitmapManager.ActiveDocument != null)
            {
                int index = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.IndexOf(layer);
                return index;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
