using System.Collections;
using System.Globalization;
using Drawie.Numerics;
using PixiEditor.UI.Common.Localization;

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

        if(value is IEnumerable arr)
        {
            var casted = arr.Cast<object>();
            var castedArr = casted as object[] ?? casted.ToArray();
            int count = castedArr.Count();
            if (count > 10)
            {
                return "[" + string.Join(", ", castedArr.Take(10).Select(Format)) + $"]... +{count - 10}";
            }

            if (count == 0)
            {
                return new LocalizedString("ARRAY_EMPTY", castedArr.GetType().GetElementType().Name);
            }

            return "[" + string.Join(", ", castedArr.Select(Format)) + "]";
        }

        return Format(value) ?? string.Empty;
    }

    private static string? Format(object x)
    {
        if (x == null)
        {
            return "null";
        }

        if (x.ToString() == x.GetType().ToString())
        {
            return x.GetType().Name;
        }

        return x.ToString();
    }
}
