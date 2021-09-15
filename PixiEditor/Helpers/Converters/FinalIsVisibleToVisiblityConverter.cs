using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PixiEditor.Helpers.Converters
{
    public class FinalIsVisibleToVisiblityConverter : MarkupExtension, IMultiValueConverter
    {
        private static FinalIsVisibleToVisiblityConverter converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (converter == null)
            {
                converter = new FinalIsVisibleToVisiblityConverter();
            }

            return converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Layer layer)
            {
                if (ViewModelMain.Current?.BitmapManager?.ActiveDocument != null)
                {
                    return ViewModelMain.Current.BitmapManager.ActiveDocument.GetFinalLayerIsVisible(layer) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
