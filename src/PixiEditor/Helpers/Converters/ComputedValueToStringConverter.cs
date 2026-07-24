using System.Collections;
using System.Globalization;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Converters;

internal class ComputedValueToStringConverter : SingleInstanceConverter<ComputedValueToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string && value is IEnumerable arr && !arr.Cast<object>().Any())
        {
            return new LocalizedString("ARRAY_EMPTY", value.GetType().GetElementType()?.Name ?? string.Empty);
        }

        return Format(value) ?? string.Empty;
    }

    private static string? Format(object x)
    {
        x = Resolve(x);

        if (x == null)
        {
            return "null";
        }

        if (x is double d)
        {
            return $"{d:0.##}";
        }

        if (x is float f)
        {
            return $"{f:0.##}";
        }

        if (x is VecD vecD)
        {
            return $"{vecD.X:0.##}, {vecD.Y:0.##}";
        }

        if (x is VecF vecF)
        {
            return $"{vecF.X:0.##}, {vecF.Y:0.##}";
        }

        if (x is VecI vecI)
        {
            return $"{vecI.X}, {vecI.Y}";
        }

        if (x is string s)
        {
            return s;
        }

        if (x is IEnumerable arr)
        {
            var castedArr = arr.Cast<object>().ToArray();
            int count = castedArr.Length;
            if (count > 10)
            {
                return "[" + string.Join(", ", castedArr.Take(10).Select(Format)) + $"]... +{count - 10}";
            }

            return "[" + string.Join(", ", castedArr.Select(Format)) + "]";
        }

        if (x.ToString() == x.GetType().ToString())
        {
            return x.GetType().Name;
        }

        return x.ToString();
    }

    private static object Resolve(object value)
    {
        if (value is not Delegate func)
        {
            return value;
        }

        try
        {
            object? invoked = func.DynamicInvoke(FuncContext.NoContext);
            if (invoked is ShaderExpressionVariable expr)
            {
                invoked = expr.GetConstant();
            }

            return invoked ?? value;
        }
        catch
        {
            return value;
        }
    }
}
