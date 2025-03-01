using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class DoublePropertyViewModel : NodePropertyViewModel<double>
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
        set => Value = value;
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

    public DoublePropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(Value))
        {
            OnPropertyChanged(nameof(DoubleValue));
        }
    }
}

class SliderSettings : ObservableObject
{
    private bool isColorSlider;
    private IBrush backgroundBrush;
    private IBrush borderBrush;
    private Thickness borderThickness;
    public double thumbSize;
    public IBrush thumbBackground;

    public bool IsColorSlider
    {
        get => isColorSlider;
        set => SetProperty(ref isColorSlider, value);
    }

    public IBrush BackgroundBrush
    {
        get => backgroundBrush;
        set => SetProperty(ref backgroundBrush, value);
    }

    public SliderSettings()
    {

    }
}

public enum NumberPickerMode
{
    NumberInput,
    Slider,
}
