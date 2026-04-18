namespace PixiEditor.Helpers;

public interface ILocalizedKeyInfo
{
    public string LocalizationKey { get; }
    public string Location { get; }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class LocalizedKeyAttribute(string key, string whereItIsUsed) : Attribute, ILocalizedKeyInfo
{
    public string LocalizationKey { get; } = key;
    public string Location { get; } = whereItIsUsed;
}
