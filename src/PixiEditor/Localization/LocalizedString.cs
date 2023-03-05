namespace PixiEditor.Localization;

public struct LocalizedString
{
    public string Key { get; }
    public string Value { get; }

    public LocalizedString(string key)
    {
        Key = key;
        Value = ILocalizationProvider.Current.CurrentLanguage.Locale.ContainsKey(key) ? ILocalizationProvider.Current.CurrentLanguage.Locale[key] : key;
    }
    
    public static implicit operator LocalizedString(string key) => new(key);
    public static implicit operator string(LocalizedString localizedString) => localizedString.Value;
}
