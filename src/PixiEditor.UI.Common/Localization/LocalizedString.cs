namespace PixiEditor.UI.Common.Localization;

public struct LocalizedString
{
    public static LocalizationKeyShowMode? OverridenKeyFlowMode { get; set; } = null;
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
            Value = OverridenKeyFlowMode switch
            {
                LocalizationKeyShowMode.Key => Key,
                LocalizationKeyShowMode.ValueKey => $"{GetValue(value)} ({Key})",
                LocalizationKeyShowMode.LALALA => $"#~{GetLongString(GetValue(value).Count(x => x == ' ') + 1)}{Math.Abs(Key.GetHashCode()).ToString()[..2]}~#",
                _ => GetValue(value)
            };
            #endif
        }
    }
    public string Value { get; private set; }

    public object[]? Parameters { get; set; }

    public LocalizedString(string key)
    {
        Key = key;
    }

    public LocalizedString(string key, params object[]? parameters)
    {
        Parameters = parameters;
        Key = key;
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
        
        ILocalizationProvider? localizationProvider = ILocalizationProvider.Current;
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


        return ApplyParameters(ILocalizationProvider.Current!.CurrentLanguage.Locale[localizationKey]);
    }

    private string GetLongString(int length) => string.Join(' ', Enumerable.Repeat("LaLaLaLaLa", length));

    private string ApplyParameters(string value)
    {
        if (Parameters == null || Parameters.Length == 0)
        {
            return value;
        }

        try
        {
            var executedParameters = new object[Parameters.Length];
            for (var i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];
                object objToExecute = parameter;
                if (parameter is LocalizedString str)
                {
                    objToExecute = new LocalizedString(str.Key, str.Parameters).Value;
                }

                executedParameters[i] = objToExecute;
            }

            return string.Format(value, executedParameters);
        }
        catch (FormatException)
        {
            return value;
        }
    }

    public static implicit operator LocalizedString(string key) => new(key);
    public static implicit operator string(LocalizedString localizedString) => localizedString.Value;

    public static string FirstValidKey(string key, string fallbackKey)
    {
        LocalizedString localizedString = new(key);
        if (localizedString.Key == localizedString.Value)
        {
            return fallbackKey;
        }

        return key;
    }
}
