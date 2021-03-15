using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using PixiEditor.Models.Layers;

namespace PixiEditor.Helpers.Converters
{
    public class LayersCountToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ILayerContainer container)
            {
                bool moreThan = true;
                if (parameter is string)
                {
                    moreThan = false;
                }

                int layersCount = container.GetLayers().Count();

                return layersCount > 0 && (layersCount > 1 || (!moreThan && layersCount == 1)) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}