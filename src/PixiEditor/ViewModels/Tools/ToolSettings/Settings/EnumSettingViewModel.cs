using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Extensions.UI;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class EnumSettingViewModel<TEnum> : Setting<TEnum, ComboBox>
    where TEnum : struct, Enum
{
    private int selectedIndex;

    /// <summary>
    /// Gets or sets the selected Index of the <see cref="ComboBox"/>.
    /// </summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (SetProperty(ref selectedIndex, value))
            {
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected value of the <see cref="ComboBox"/>.
    /// </summary>
    public override TEnum Value
    {
        get => hasOverwrittenValue ? GetOverwrittenEnum() : Enum.GetValues<TEnum>()[SelectedIndex];
        set
        {
            var values = Enum.GetValues<TEnum>();

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].Equals(value))
                {
                    SelectedIndex = i;
                    break;
                }
            }

            base.Value = value;
        }
    }
    
    public TEnum[] EnumValues { get; } = Enum.GetValues<TEnum>();

    public EnumSettingViewModel(string name, string label)
        : base(name)
    {
        Label = label;
    }

    public EnumSettingViewModel(string name, string label, TEnum defaultValue)
        : this(name, label)
    {
        Value = defaultValue;
    }
    
    private TEnum GetOverwrittenEnum()
    {
        int index;
        if (overwrittenValue is float floatVal)
        {
            index = (int)floatVal;
        }
        else if (overwrittenValue is int intVal)
        {
            index = intVal;
        }
        else if (overwrittenValue is string stringVal)
        {
            return Enum.Parse<TEnum>(stringVal);
        }
        else
        {
            throw new InvalidCastException("Overwritten value is not a valid type.");
        }

        return Enum.GetValues<TEnum>()[index];
    }
}
