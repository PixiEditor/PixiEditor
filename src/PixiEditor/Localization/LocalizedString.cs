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
            #if DEBUG_LOCALIZATION
            Value = key;
            #else
            Value = GetValue(value);
            #endif
        }
    }
    public string Value { get; private set; }

    public object[] Parameters { get; set; }

    public LocalizedString(string key)
    {
        Key = key;
        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
    }

    public LocalizedString(string key, params object[] parameters)
    {
        Parameters = parameters;
        Key = key;
        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(Language lang)
    {
        Value = GetValue(Key);
    }

    public override string ToString()
    {
        return Value;
    }

    private string GetValue(string localizationKey)
    {
        if (string.IsNullOrEmpty(localizationKey))
        {
            return localizationKey;
        }
        
        ILocalizationProvider localizationProvider = ILocalizationProvider.Current;
        if (localizationProvider?.LocalizationData == null)
        {
            return localizationKey;
        }

        if (!localizationProvider.CurrentLanguage.Locale.ContainsKey(localizationKey))
        {
            Language defaultLanguage = localizationProvider.DefaultLanguage;

            if (localizationProvider.CurrentLanguage == defaultLanguage || !defaultLanguage.Locale.ContainsKey(localizationKey))
            {
                return localizationKey;
            }

            return ApplyParameters(defaultLanguage.Locale[localizationKey]);
        }


        return ApplyParameters(ILocalizationProvider.Current.CurrentLanguage.Locale[localizationKey]);
    }

    private string ApplyParameters(string value)
    {
        if (Parameters == null || Parameters.Length == 0)
        {
            return value;
        }

        try
        {
            return string.Format(value, Parameters);
        }
        catch (FormatException)
        {
            return value;
        }
    }

    public static implicit operator LocalizedString(string key) => new(key);
    public static implicit operator string(LocalizedString localizedString) => localizedString.Value;
}
