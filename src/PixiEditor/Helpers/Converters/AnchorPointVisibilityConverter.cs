using System.Collections.Generic;
using System.Globalization;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Helpers.Converters;

internal class AnchorPointVisibilityConverter : SingleInstanceMultiValueConverter<AnchorPointVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        ResizeAnchor anchor = (ResizeAnchor)value;
        ResizeAnchor selectedAnchor = (ResizeAnchor)parameter;

        return anchor == selectedAnchor;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        ResizeAnchor selectedAnchor = (ResizeAnchor)values[0];
        ResizeAnchor anchor = (ResizeAnchor)values[1];

        return selectedAnchor == anchor;
    }
}
