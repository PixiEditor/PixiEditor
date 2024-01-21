using System.ComponentModel;

namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

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

    public event SettingChangedHandler<T> ValueChanged; 
    
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
