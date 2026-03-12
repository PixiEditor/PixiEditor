using System.Text.Json;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Extensions.UI;
using PixiEditor.Helpers.Decorators;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class EnumSettingViewModel<TEnum> : Setting<TEnum>
    where TEnum : struct, Enum
{
    private EnumSettingPickerType pickerType = EnumSettingPickerType.ComboBox;
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

    public EnumSettingPickerType PickerType
    {
        get => pickerType;
        set
        {
            SetProperty(ref pickerType, value);
            OnPropertyChanged(nameof(PickerIsIconButtons));
        }
    }

    public bool PickerIsIconButtons => PickerType == EnumSettingPickerType.IconButtons;
    
    public TEnum[] EnumValues { get; } = Enum.GetValues<TEnum>();

    public ICommand ChangeValueCommand { get; }

    public EnumSettingViewModel(string name, string label)
        : base(name)
    {
        Label = label;
        ChangeValueCommand = new RelayCommand<TEnum>(val => Value = val);
    }

    public EnumSettingViewModel(string name, string label, TEnum defaultValue)
        : this(name, label)
    {
        Value = defaultValue;
    }
    
    private TEnum GetOverwrittenEnum()
    {
        var value = overwrittenValue;
        if (overwrittenValue is JsonElement jsonElement)
        {
            value = jsonElement.ValueKind switch
            {
                JsonValueKind.Number when jsonElement.TryGetInt32(out var intVal) => intVal,
                JsonValueKind.Number when jsonElement.TryGetSingle(out var floatVal) => floatVal,
                JsonValueKind.String => jsonElement.GetString(),
            };
        }

        int index;
        if (value is float finalFloatVal)
        {
            index = (int)finalFloatVal;
        }
        else if (value is int intVal)
        {
            index = intVal;
        }
        else if (value is string stringVal)
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

public enum EnumSettingPickerType
{
    ComboBox,
    IconButtons
}
