using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Localization;

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
        
        string translated = Native.translate_key(localizationKey);
        if (translated == null)
        {
            return localizationKey;
        }

        return ApplyParameters(translated);
    }
    
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
}
