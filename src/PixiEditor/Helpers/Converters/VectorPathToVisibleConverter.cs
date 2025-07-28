using System.Globalization;
using Avalonia.Data.Converters;
using Drawie.Backend.Core.Vector;

namespace PixiEditor.Helpers.Converters;

internal class VectorPathToVisibleConverter : SingleInstanceConverter<VectorPathToVisibleConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is VectorPath path)
        {
            return !path.IsEmpty;
        }

        return false;
    }
}
