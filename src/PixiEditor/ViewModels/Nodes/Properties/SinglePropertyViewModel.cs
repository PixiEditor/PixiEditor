using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class SinglePropertyViewModel : NodePropertyViewModel<float>
{
    private double min = double.MinValue;
    private double max = double.MaxValue;

    private NumberPickerMode numberPickerMode = NumberPickerMode.NumberInput;

    private SliderSettings sliderSettings = new SliderSettings();

    public NumberPickerMode NumberPickerMode
    {
        get => numberPickerMode;
        set => SetProperty(ref numberPickerMode, value);
    }

    public double DoubleValue
    {
        get => Value;
        set
        {
            if (updateBlocker)
                return;

            Value = (float)value;
        }
    }

    public double Min
    {
        get => min;
        set => SetProperty(ref min, value);
    }

    public double Max
    {
        get => max;
        set => SetProperty(ref max, value);
    }

    public SliderSettings SliderSettings
    {
        get => sliderSettings;
        set => SetProperty(ref sliderSettings, value);
    }

    private bool updateBlocker = false;

    public SinglePropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        updateBlocker = true;
        if (e.PropertyName == nameof(Value))
        {
            OnPropertyChanged(nameof(DoubleValue));
        }
        updateBlocker = false;
    }
}
