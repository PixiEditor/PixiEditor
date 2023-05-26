using System.ComponentModel;
using System.Windows.Markup;

namespace PixiEditor.Helpers;

public class EnumExtension : MarkupExtension
{
    private Type _enumType;

    public EnumExtension(Type enumType)
    {
        EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
    }

    private Type EnumType
    {
        get => _enumType;
        init
        {
            if (_enumType == value)
                return;

            var enumType = Nullable.GetUnderlyingType(value) ?? value;

            if (enumType.IsEnum == false)
                throw new ArgumentException("Type must be an Enum.");

            _enumType = value;
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider) // or IXamlServiceProvider for UWP and WinUI
    {
        return Enum.GetValues(EnumType);
    }
}
