using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using PixiEditor.ViewModels;
using System;
using System.Globalization;

namespace PixiEditor.Helpers.Converters
{
    public class LayerToFinalOpacityConverter
        : SingleInstanceMultiValueConverter<LayerToFinalOpacityConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is Layer layer && ViewModelMain.Current?.BitmapManager?.ActiveDocument != null)
            {
                return (double)LayerStructureUtils.GetFinalLayerOpacity(layer, ViewModelMain.Current.BitmapManager.ActiveDocument.LayerStructure);
            }

            return null;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}