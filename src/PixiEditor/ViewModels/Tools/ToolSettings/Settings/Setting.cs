using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordRPC;
using PixiEditor.UI.Common.Localization;

#pragma warning disable SA1402 // File may only contain a single type, Justification: "Same class with generic value"

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal abstract class Setting<T, TControl> : Setting<T>
    where TControl : Control
{
    protected Setting(string name)
        : base(name)
    {
    }
}

internal abstract class Setting<T> : Setting
{
    protected Setting(string name)
        : base(name)
    {
    }

    public new event EventHandler<SettingValueChangedEventArgs<T>> ValueChanged;

    public new virtual T Value
    {
        get
        {
            if(base.Value != null && base.Value is not T value)
            {
                return default;
            }
            
            return (T)base.Value;
        }
        set
        {
            T oldValue = default;
            if (base.Value != null)
            {
                oldValue = Value;
            }

            base.Value = value;
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<T>(oldValue, Value));
            OnPropertyChanged(nameof(Value));
        }
    }

    public override Type GetSettingType() => typeof(T);
}

internal abstract class Setting : ObservableObject
{
    private object _value;
    private bool isExposed = true;
    
    protected bool overwrittenExposed;
    protected object overwrittenValue;

    protected bool hasOverwrittenValue;
    protected bool hasOverwrittenExposed;
    
    protected Setting(string name)
    {
        Name = name;
    }

    public event EventHandler<SettingValueChangedEventArgs<object>> ValueChanged;

    public object Value
    {
        get => hasOverwrittenValue ? overwrittenValue : _value;
        set
        {
            var old = _value;
            if (SetProperty(ref _value, value))
            {
                ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(old, value));
            }
        }
    }

    protected void InvokeValueChanged()
    {
        ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(_value, _value));
    }

    public bool IsExposed
    {
        get => hasOverwrittenExposed ? overwrittenExposed : isExposed;
        set => SetProperty(ref isExposed, value);
    }

    public string Name { get; }

    public LocalizedString Label { get; set; }

    public string Icon { get; set; }

    public string Tooltip { get; set; }

    public bool HasLabel => !string.IsNullOrEmpty(Label);
    public bool IsBuiltInLabelVisible => HasLabel && IsLabelVisible;
    public bool HasIcon => !string.IsNullOrEmpty(Icon);
    public bool AllowIconLabel { get; protected set; } = true;

    public bool IsLabelVisible { get; set; } = true;

    public object UserValue
    {
        get => _value;
        set => _value = value;
    }

    public abstract Type GetSettingType();
    
    public void SetOverwriteValue(object value)
    {
        overwrittenValue = value;
        hasOverwrittenValue = true;
        
        OnPropertyChanged(nameof(Value));
        ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(_value, value));
    }
    
    public void SetOverwriteExposed(bool value)
    {
        overwrittenExposed = value;
        hasOverwrittenExposed = true;
        
        OnPropertyChanged(nameof(IsExposed));
    }
    
    public void ResetOverwrite()
    {
        var old = overwrittenValue;
        bool hadOverwrittenValue = hasOverwrittenValue;
        overwrittenValue = null;
        overwrittenExposed = false;
        hasOverwrittenValue = false;
        hasOverwrittenExposed = false;
        
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(IsExposed));

        if (hadOverwrittenValue && old != _value)
        {
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(old, _value));
        }
    }
}
