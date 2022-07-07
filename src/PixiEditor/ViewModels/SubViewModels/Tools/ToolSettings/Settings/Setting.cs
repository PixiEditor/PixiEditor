using System.Windows.Controls;

#pragma warning disable SA1402 // File may only contain a single type, Justification: "Same class with generic value"

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

internal abstract class Setting<T, TControl> : Setting<T>
    where TControl : Control
{
    protected Setting(string name)
        : base(name)
    {
    }

    public new TControl SettingControl
    {
        get => (TControl)base.SettingControl;
        set => base.SettingControl = value;
    }
}

internal abstract class Setting<T> : Setting
{
    protected Setting(string name)
        : base(name)
    {
    }

    public event EventHandler<SettingValueChangedEventArgs<T>> ValueChanged;

    public new T Value
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
            RaisePropertyChanged(nameof(Value));
        }
    }
}

internal abstract class Setting : NotifyableObject
{
    protected Setting(string name)
    {
        Name = name;
    }

    public object Value { get; set; }

    public string Name { get; }

    public string Label { get; set; }

    public bool HasLabel => !string.IsNullOrEmpty(Label);

    public Control SettingControl { get; set; }

    public abstract Control GenerateControl();
}
