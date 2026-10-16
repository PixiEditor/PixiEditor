using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class EnumSettingViewModel<TEnum> : Setting<TEnum>
    where TEnum : struct, Enum
{
    private ListSettingPickerType pickerType = ListSettingPickerType.ComboBox;
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
                base.Value = Value; // Update the base Value to trigger any bindings or logic that depends on it.
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected value of the <see cref="ComboBox"/>.
    /// </summary>
    public override TEnum Value
    {
        get => hasOverwrittenValue ? GetOverwrittenEnum() : GetFilteredValues().ElementAtOrDefault(SelectedIndex);
        set
        {
            var filteredValues = GetFilteredValues();
            for (var i = 0; i < filteredValues.Length; i++)
            {
                if (filteredValues[i].Equals(value))
                {
                    SelectedIndex = i;
                    break;
                }
            }

            base.Value = value;
        }
    }

    private TEnum[] GetFilteredValues()
    {
        var values = Enum.GetValues<TEnum>();
        var filteredValues = Filter is null ? values : values.Where(Filter).ToArray();
        return filteredValues;
    }

    public ListSettingPickerType PickerType
    {
        get => pickerType;
        set
        {
            SetProperty(ref pickerType, value);
            OnPropertyChanged(nameof(PickerIsIconButtons));
        }
    }

    public bool PickerIsIconButtons => PickerType == ListSettingPickerType.IconButtons;
    
    public TEnum[] EnumValues => Filter is null ? allValues : allValues.Where(Filter).ToArray();

    public ICommand ChangeValueCommand { get; }
    public Func<TEnum, bool>? Filter { get; set; }

    private TEnum[] allValues = Enum.GetValues<TEnum>();

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

    public void RefreshEnumValues()
    {
        OnPropertyChanged(nameof(EnumValues));
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

        return GetFilteredValues()[index];
    }
}

public enum ListSettingPickerType
{
    ComboBox,
    IconButtons
}
