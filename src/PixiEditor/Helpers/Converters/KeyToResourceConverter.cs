using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class KeyToResourceConverter : SingleInstanceConverter<KeyToResourceConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(Application.Current.Styles.TryGetResource(value, null, out object? icon))
        {
            return icon;
        }
        
        return null;
    }
}
