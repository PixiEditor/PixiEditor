using System.ComponentModel;
using System.Diagnostics;

namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public abstract class Setting<T> : INotifyPropertyChanged
{
    private readonly IPreferences preferences;
    private event PropertyChangedEventHandler PropertyChanged;

    public string Name { get; }

    public T? Value
    {
        get => GetValue(preferences, FallbackValue);
        set => SetValue(preferences, value);
    }

    public T? FallbackValue { get; }

    public event SettingChangedHandler<T> ValueChanged; 

    public Setting(string name, T? fallbackValue = default)
    {
        Name = name;
        FallbackValue = fallbackValue;
        
        preferences = IPreferences.Current;
        preferences.AddCallback<T>(Name, SettingChangeCallback);
    }

    public T GetValueOrDefault(T fallbackValue) => GetValue(preferences, fallbackValue);

    public T? As<T>(T? fallbackValue = default) => GetValue(preferences, fallbackValue);

    protected abstract TAny? GetValue<TAny>(IPreferences preferences, TAny fallbackValue);

    protected abstract void SetValue(IPreferences preferences, T? value);

    private void SettingChangeCallback(T newValue)
    {
        ValueChanged?.Invoke(this, newValue);
        PropertyChanged?.Invoke(this, PropertyChangedConstants.ValueChangedPropertyArgs);
    }

    private string GetDebuggerDisplay()
    {
        string value;

        try
        {
            value = Value.ToString();
        }
        catch (Exception e)
        {
            value = $"<{e.GetType()}: {e.Message}>";
        }

        if (typeof(T) == typeof(string))
        {
            value = $"""
                     "{value}"
                     """;
        }

        var type = typeof(T).ToString();
        
        string preferenceType = this switch
        {
            LocalSetting<T> => "local",
            SyncedSetting<T> => "synced",
            _ => "<undefined>"
        };

        return $"{preferenceType} {Name}: {type} = {value}";
    }
    
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
    }
}

// Generic types would create a instance for every type combination.
file static class PropertyChangedConstants
{
    public static readonly PropertyChangedEventArgs ValueChangedPropertyArgs = new("Value");
}
