using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.Converters;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed partial class ListSettingViewModel<TValue> : Setting<TValue>
{
    private ListSettingPickerType pickerType = ListSettingPickerType.ComboBox;
    private int selectedIndex;

    public ListSettingViewModel(string name, string label, IEnumerable<TValue> values) : base(name)
    {
        Label = label;
        Values = new ObservableCollection<TValue>(values);
        ChangeValueCommand = new RelayCommand<TValue>(val => Value = val);
    }

    public ListSettingViewModel(string name, string label, IEnumerable<TValue> values, TValue defaultValue) : this(name, label, values)
    {
        Value = defaultValue;
    }

    /// <summary>
    /// Gets or sets the selected index of the <see cref="ComboBox"/>.
    /// </summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (SetProperty(ref selectedIndex, value))
            {
                base.Value = Value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected value.
    /// </summary>
    public override TValue Value
    {
        get => hasOverwrittenValue
            ? GetOverwrittenValue()
            : Values.ElementAtOrDefault(SelectedIndex);

        set
        {
            var filteredValues = Values;

            for (var i = 0; i < filteredValues.Count; i++)
            {
                if (EqualityComparer<TValue>.Default.Equals(filteredValues[i], value))
                {
                    SelectedIndex = i;
                    break;
                }
            }

            base.Value = value;
        }
    }

    public ListSettingPickerType PickerType
    {
        get => pickerType;
        set
        {
            if (SetProperty(ref pickerType, value))
                OnPropertyChanged(nameof(PickerIsIconButtons));
        }
    }

    public bool PickerIsIconButtons => PickerType == ListSettingPickerType.IconButtons;

    [ObservableProperty]
    public partial ObservableCollection<TValue> Values { get; set; }

    public ICommand ChangeValueCommand { get; }

    [ObservableProperty]
    public partial ITextFormatter? TextFormatter { get; set; }

    private TValue GetOverwrittenValue()
    {
        var value = overwrittenValue;

        if (value is JsonElement jsonElement)
            return jsonElement.Deserialize<TValue>()
                ?? throw new InvalidCastException(
                    $"Could not deserialize overwritten value to {typeof(TValue).Name}.");

        if (value is TValue typedValue)
            return typedValue;

        return (TValue)Convert.ChangeType(value, typeof(TValue));
    }
}
