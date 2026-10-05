using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PixiEditor.Helpers.Converters;

public class DisplayNameConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<ITextFormatter> FormatterProperty = AvaloniaProperty.Register<DisplayNameConverter, ITextFormatter>(
        nameof(Formatter));

    public ITextFormatter Formatter
    {
        get => GetValue(FormatterProperty);
        set => SetValue(FormatterProperty, value);
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(Formatter != null)
        {
            return Formatter.Format(value);
        }

        return value?.ToString() ?? "null";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public interface ITextFormatter
{
    string Format(object? value);
}

public class InlineTextFormatter<T> : ITextFormatter
{
    public Func<T?, string> FormatFunc { get; }
    public InlineTextFormatter(Func<T?, string> formatFunc)
    {
        FormatFunc = formatFunc;
    }

    public string Format(object? value)
    {
        if(value is T typedValue)
        {
            return FormatFunc(typedValue);
        }

        return value?.ToString() ?? "null";
    }
}
