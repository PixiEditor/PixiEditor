namespace PixiEditor.Helpers;

public interface ILocalizeEnumInfo
{
    public object GetEnumValue();
    
    public string LocalizationKey { get; }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class LocalizeEnumAttribute<T>(T value, string key) : Attribute, ILocalizeEnumInfo where T : Enum
{
    public T Value { get; } = value;

    object ILocalizeEnumInfo.GetEnumValue() => Value;
    
    public string LocalizationKey { get; } = key;
}
