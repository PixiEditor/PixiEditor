namespace PixiEditor.Localization;

public struct LocalizedString
{
    private string key;

    public string Key
    {
        get => key;
        set
        {
            key = value;
            Value = GetValue(value);
        }
    }
    public string Value { get; private set; }

    public LocalizedString(string key)
    {
        Key = key;
    }

    public override string ToString()
    {
        return Value;
    }

    private static string GetValue(string key)
    {
        ILocalizationProvider localizationProvider = ILocalizationProvider.Current;
        if (localizationProvider?.LocalizationData == null)
        {
            return key;
        }

        if (!localizationProvider.CurrentLanguage.Locale.ContainsKey(key))
        {
            Language defaultLanguage = localizationProvider.DefaultLanguage;

            if (localizationProvider.CurrentLanguage == defaultLanguage || !defaultLanguage.Locale.ContainsKey(key))
            {
                return key;
            }

            return defaultLanguage.Locale[key];
        }


        return ILocalizationProvider.Current.CurrentLanguage.Locale[key];
    }

    public static implicit operator LocalizedString(string key) => new(key);
    public static implicit operator string(LocalizedString localizedString) => localizedString.Value;
}
