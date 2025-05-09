using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;
using PixiEditor.Extensions.CommonApi.FlyUI;
using Cursor = PixiEditor.Extensions.CommonApi.FlyUI.Cursor;

namespace PixiEditor.Extensions.FlyUI.Converters;

internal class CursorToAvaloniaCursorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Cursor cursor)
        {
            return new Avalonia.Input.Cursor((StandardCursorType)(cursor.BuiltInCursor ?? BuiltInCursor.None));
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
