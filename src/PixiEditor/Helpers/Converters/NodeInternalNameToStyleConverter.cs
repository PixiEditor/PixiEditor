using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class NodeInternalNameToStyleConverter : SingleInstanceConverter<NodeInternalNameToStyleConverter>
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return AvaloniaProperty.UnsetValue;
        
        string s = (string)value;
        s = s.Replace(".", string.Empty);
        
        if (Application.Current.Styles.TryGetResource($"{s}{parameter}", Application.Current.ActualThemeVariant, out var output))
        {
            return output;
        }

        return AvaloniaProperty.UnsetValue;
    }
}
