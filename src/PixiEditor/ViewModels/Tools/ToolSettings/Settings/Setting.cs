using System.Diagnostics;
using System.Text.Json;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordRPC;
using PixiEditor.UI.Common.Localization;

#pragma warning disable SA1402 // File may only contain a single type, Justification: "Same class with generic value"

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

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
            var raw = base.Value;
            if (base.Value is JsonElement jsonElement)
            {
                try
                {
                    raw = jsonElement.Deserialize<T>();
                }
                catch
                {
                    Debug.WriteLine($"Failed to deserialize setting {Name} value from JSON.");
                    return default;
                }
            }

            var adjusted = AdjustValue(raw);
            if (adjusted != null && adjusted is not T)
            {
                return default;
            }

            return (T)adjusted;
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
    private string currentToolset = "";
    private Dictionary<string, object> toolsetValues = new Dictionary<string, object>();
    private Dictionary<string, bool> defaultValuesSet = new Dictionary<string, bool>();
    private bool isExposed = true;

    protected bool overwrittenExposed;
    protected object overwrittenValue;

    protected bool hasOverwrittenValue;
    protected bool hasOverwrittenExposed;

    private bool mergeChanges;

    protected Setting(string name)
    {
        Name = name;
    }

    public event EventHandler<SettingValueChangedEventArgs<object>> ValueChanged;

    public object Value
    {
        get => hasOverwrittenValue ? overwrittenValue : toolsetValues.GetValueOrDefault(currentToolset, null);
        set
        {
            var old = toolsetValues.GetValueOrDefault(currentToolset, null);

            if (value != null && old != null && value.GetType() != old.GetType())
            {
                try
                {
                    value = Convert.ChangeType(value, old.GetType());
                }
                catch
                {
                    // ignored
                }
            }

            if (old != value)
            {
                toolsetValues[currentToolset] = value;
                OnPropertyChanged(nameof(Value));
                ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(old, value));
            }
        }
    }

    protected void InvokeValueChanged()
    {
        object value = toolsetValues.GetValueOrDefault(currentToolset, null);
        ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(value, value));
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
        get => toolsetValues.GetValueOrDefault(currentToolset, null);
        set => toolsetValues[currentToolset] = value;
    }

    public bool MergeChanges
    {
        get => mergeChanges;
        set
        {
            if (SetProperty(ref mergeChanges, value) && !value)
            {
                MergeChangesEnded?.Invoke();
            }
        }
    }

    public event Action MergeChangesEnded;

    public abstract Type GetSettingType();

    public void SetOverwriteValue(object value)
    {
        var adjusted = AdjustValue(value);
        overwrittenValue = adjusted;
        hasOverwrittenValue = true;

        OnPropertyChanged(nameof(Value));
        ValueChanged?.Invoke(this,
            new SettingValueChangedEventArgs<object>(toolsetValues.GetValueOrDefault(currentToolset, null), adjusted));
    }

    public void SetCurrentToolset(string toolset)
    {
        var oldToolset = currentToolset;
        currentToolset = toolset;
        if (toolsetValues.Count <= 1)
        {
            if (toolsetValues.TryGetValue("", out object? value))
            {
                toolsetValues[toolset] = value;
                toolsetValues.Remove("");
            }
        }

        if (!toolsetValues.ContainsKey(currentToolset))
        {
            toolsetValues[currentToolset] = toolsetValues.FirstOrDefault().Value;
        }

        var oldValue = toolsetValues.GetValueOrDefault(oldToolset, null);

        OnPropertyChanged(nameof(Value));
        if (oldValue != Value)
        {
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(oldValue, Value));
        }
    }

    protected virtual object AdjustValue(object value)
    {
        return value;
    }

    public void SetOverwriteExposed(bool value)
    {
        overwrittenExposed = value;
        hasOverwrittenExposed = true;

        OnPropertyChanged(nameof(IsExposed));
    }

    public void SetDefaultValue(object defaultValue, string toolset)
    {
        if (!defaultValuesSet.GetValueOrDefault(toolset, false))
        {
            if (defaultValue != null && (defaultValue.GetType() != GetSettingType()))
            {
                try
                {
                    var adjusted = AdjustValue(defaultValue);

                    if (adjusted.GetType() != GetSettingType())
                    {
                        defaultValue = Convert.ChangeType(defaultValue, GetSettingType());
                    }
                }
                catch
                {
                    Debug.WriteLine($"Failed to convert default value of setting {Name} to type {GetSettingType()}");
                    return;
                }
            }
            toolsetValues[toolset] = defaultValue;
            defaultValuesSet[toolset] = true;
            OnPropertyChanged(nameof(Value));
        }
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

        object current = toolsetValues.GetValueOrDefault(currentToolset, null);
        if (hadOverwrittenValue && old != current)
        {
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs<object>(old, current));
        }
    }
}
