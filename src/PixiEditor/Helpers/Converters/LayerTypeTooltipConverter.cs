using System.Globalization;
using Avalonia;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Converters;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Converters;

internal class LayerTypeTooltipConverter : MarkupConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            IVectorLayerHandler => new LocalizedString("VECTOR_LAYER"),
            IRasterLayerHandler => new LocalizedString("RASTER_LAYER"),
            ILayerHandler => new LocalizedString("NESTED_DOCUMENT"),
            _ => AvaloniaProperty.UnsetValue
        };
    }
}
