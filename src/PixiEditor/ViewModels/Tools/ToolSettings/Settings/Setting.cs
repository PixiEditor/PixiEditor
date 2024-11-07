using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Extensions.Common.Localization;

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
        get => (T)base.Value;
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
    
    private bool overwrittenExposed;
    private object overwrittenValue;

    private bool hasOverwrittenValue;
    private bool hasOverwrittenExposed;
    
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

    public bool IsExposed
    {
        get => hasOverwrittenExposed ? overwrittenExposed : isExposed;
        set => SetProperty(ref isExposed, value);
    }

    public string Name { get; }

    public LocalizedString Label { get; set; }

    public bool HasLabel => !string.IsNullOrEmpty(Label);

    public abstract Type GetSettingType();
    
    public void SetOverwriteValue(object value)
    {
        overwrittenValue = value;
        hasOverwrittenValue = true;
        
        OnPropertyChanged(nameof(Value));
    }
    
    public void SetOverwriteExposed(bool value)
    {
        overwrittenExposed = value;
        hasOverwrittenExposed = true;
        
        OnPropertyChanged(nameof(IsExposed));
    }
    
    public void ResetOverwrite()
    {
        overwrittenValue = null;
        overwrittenExposed = false;
        hasOverwrittenValue = false;
        hasOverwrittenExposed = false;
        
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(IsExposed));
    }
}
