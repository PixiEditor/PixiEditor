using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using PixiEditor.ViewModels;

namespace PixiEditor.Helpers.Converters
{
    public class LayerToFinalOpacityConverter : MarkupExtension, IMultiValueConverter
    {
        private static LayerToFinalOpacityConverter converter;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is Layer layer && ViewModelMain.Current?.BitmapManager?.ActiveDocument != null)
            {
                return (double)LayerStructureUtils.GetFinalLayerOpacity(layer, ViewModelMain.Current.BitmapManager.ActiveDocument.LayerStructure);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(converter == null)
            {
                converter = new LayerToFinalOpacityConverter();
            }

            return converter;
        }
    }
}