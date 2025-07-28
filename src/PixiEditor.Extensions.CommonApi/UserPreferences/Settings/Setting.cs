using System.ComponentModel;
using System.Diagnostics;

namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public abstract class Setting<T> : INotifyPropertyChanged
{
    private readonly IPreferences preferences;
    private event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// The name of the preference
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The value of the preference
    /// </summary>
    public T? Value
    {
        get => GetValue(preferences, FallbackValue);
        set => SetValue(preferences, value);
    }

    /// <summary>
    /// The value used if the preference has not been set before
    /// </summary>
    public T? FallbackValue { get; }

    /// <summary>
    /// Called when the value of the preference has changed
    /// </summary>
    public event SettingChangedHandler<T> ValueChanged; 

    /// <param name="name">The name of the preference</param>
    /// <param name="fallbackValue">The value used if the preference has not been set before</param>
    protected Setting(string name, T? fallbackValue = default)
    {
        SettingHelper.ThrowIfEmptySettingName(name);
        
        Name = name;
        FallbackValue = fallbackValue;
        
        preferences = IPreferences.Current;
        preferences.AddCallback<T>(Name, SettingChangeCallback);
    }

    /// <summary>
    /// Gets the value of the preference or the <see cref="fallbackValue"/> if the preference has not been set before. Note: This will ignore the <see cref="FallbackValue"/> set in the setting constructor
    /// </summary>
    /// <param name="fallbackValue">The value used if the preference has not been set before</param>
    /// <returns>Either the value of the preference or <see cref="fallbackValue"/></returns>
    public T GetValueOrDefault(T fallbackValue) => GetValue(preferences, fallbackValue);

    /// <summary>
    /// Gets the value of the preference as <typeparamref name="T"/> instead of the type defined by the setting.
    /// </summary>
    /// <param name="fallbackValue">The value used if the preference has not been set before</param>
    /// <returns>Either the value of the preference as <typeparamref name="T"/> or <see cref="fallbackValue"/></returns>
    public TAny? As<TAny>(TAny? fallbackValue = default) => GetValue(preferences, fallbackValue);

    protected abstract TAny? GetValue<TAny>(IPreferences preferences, TAny fallbackValue);

    protected abstract void SetValue(IPreferences preferences, T? value);

    private void SettingChangeCallback(string name, T newValue)
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

// Generic types would create an instance for every type combination.
file static class PropertyChangedConstants
{
    public static readonly PropertyChangedEventArgs ValueChangedPropertyArgs = new("Value");
}
