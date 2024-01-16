using System.Globalization;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;
internal class BlendModeToStringConverter : SingleInstanceConverter<BlendModeToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not BlendMode mode)
            return "<null>";
        return new LocalizedString(mode.LocalizedKeys()).Value;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
