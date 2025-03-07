using System.Reflection;
using PixiEditor.SVG.Attributes;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Helpers;

namespace PixiEditor.SVG.Units;

public struct SvgEnumUnit<T> : ISvgUnit where T : struct, Enum
{
    public T Value { get; set; }

    public SvgEnumUnit(T value)
    {
        Value = value;
    }

    public string ToXml()
    {
        FieldInfo field = Value.GetType().GetField(Value.ToString());
        SvgValueAttribute attribute = field.GetCustomAttribute<SvgValueAttribute>();
        
        if (attribute != null)
        {
            return attribute.Value;
        }
        
        return Value.ToString().ToKebabCase();
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        bool matched = TryMatchEnum(readerValue);
        if (!matched && Enum.TryParse(readerValue.FromKebabToTitleCase(), out T result))
        {
            Value = result;
        }
    }
    
    private bool TryMatchEnum(string value)
    {
        foreach (T enumValue in Enum.GetValues(typeof(T)))
        {
            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
            SvgValueAttribute attribute = field.GetCustomAttribute<SvgValueAttribute>();
            
            if (attribute != null && attribute.Value == value)
            {
                Value = enumValue;
                return true;
            }
        }
        
        return false;
    }
}
