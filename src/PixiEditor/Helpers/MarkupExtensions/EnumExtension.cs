using Avalonia.Markup.Xaml;

namespace PixiEditor.Helpers.MarkupExtensions;

internal class EnumExtension : MarkupExtension
{
    private Type enumType;

    public EnumExtension(Type enumType)
    {
        EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
    }

    private Type EnumType
    {
        get => enumType;
        init
        {
            if (this.enumType == value)
                return;

            var type = Nullable.GetUnderlyingType(value) ?? value;

            if (type.IsEnum == false)
                throw new ArgumentException("Type must be an Enum.");

            this.enumType = value;
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Enum.GetValues(EnumType);
    }
}
