using System.Globalization;
using Drawie.Numerics;

namespace PixiEditor.Helpers.Converters;

internal class ComputedValueToStringConverter : SingleInstanceConverter<ComputedValueToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return $"{d:0.##}";
        }

        if (value is float f)
        {
            return $"{f:0.##}";
        }

        if(value is VecD vecD)
        {
            return $"{vecD.X:0.##}, {vecD.Y:0.##}";
        }

        if (value is VecF vecI)
        {
            return $"{vecI.X:0.##}, {vecI.Y:0.##}";
        }

        if (value is VecI vec)
        {
            return $"{vec.X}, {vec.Y}";
        }

        return value?.ToString() ?? "null";
    }
}
