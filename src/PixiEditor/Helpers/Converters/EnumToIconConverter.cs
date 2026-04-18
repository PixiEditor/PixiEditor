using System.Globalization;
using PixiEditor.Helpers.Decorators;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.Helpers.Converters;

internal class EnumToIconConverter : SingleInstanceConverter<EnumToIconConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        var enumType = value.GetType();
        var enumName = Enum.GetName(enumType, value);
        if (enumName == null)
            return null;

        var enumMember = enumType.GetMember(enumName).FirstOrDefault();
        if (enumMember == null)
            return null;

        var iconAttribute = enumMember.GetCustomAttributes(typeof(IconNameAttribute), false)
            .FirstOrDefault() as IconNameAttribute;
        if (iconAttribute == null)
            return null;

        var icon = PixiPerfectIconExtensions.TryGetByName(iconAttribute.IconName);
        return icon;
    }
}
