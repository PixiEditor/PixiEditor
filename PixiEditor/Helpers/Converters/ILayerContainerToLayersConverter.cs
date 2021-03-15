using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using PixiEditor.Models.Layers;

namespace PixiEditor.Helpers.Converters
{
    public class ILayerContainerToLayersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Layer> layers = new ();
            if (value is IEnumerable<ILayerContainer> items)
            {
                foreach (var item in items)
                {
                    layers.AddRange(item.GetLayers());
                }

                return layers;
            }
            else if (value is Layer layer)
            {
                layers.Add(layer);
                return layers;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}