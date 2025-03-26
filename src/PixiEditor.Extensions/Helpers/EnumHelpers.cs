using System.ComponentModel;

namespace PixiEditor.Extensions.Helpers;

public static class EnumHelpers
{
    public static IEnumerable<T> GetFlags<T>(this T e)
        where T : Enum
    {
        return Enum.GetValues(e.GetType()).Cast<T>().Where(x => e.HasFlag(x));
    }

    public static string GetDescription<T>(this T enumValue)
        where T : struct, Enum
    {
        var description = enumValue.ToString();
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return description;
    }
    
    public static string GetDescription(object enumValue)
    {
        if (!enumValue.GetType().IsEnum)
        {
            throw new ArgumentException("enumValue must be a enum", nameof(enumValue));
        }
        
        var description = enumValue.ToString();
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs is { Length: > 0 })
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return description;
    }

    public static bool HasDescription(Enum enumValue)
    {
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            return attrs is { Length: > 0 };
        }

        return false;
    }
}
