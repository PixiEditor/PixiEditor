using System;
using System.Globalization;
using System.Windows.Data;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditorPrototype.Models;

namespace PixiEditorPrototype.Converters;
internal class BlendModeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not BlendMode mode)
            return "<null>";
        return mode.EnglishName();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
