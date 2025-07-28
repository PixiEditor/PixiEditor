using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Helpers.Converters;

internal abstract class SingleInstanceMultiValueConverter<TThis> : MarkupConverter, IMultiValueConverter
    where TThis : SingleInstanceMultiValueConverter<TThis>
{
    private static SingleInstanceMultiValueConverter<TThis> instance;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (instance is null)
        {
            instance = this;
        }

        return instance;
    }

    public abstract object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture);
}
