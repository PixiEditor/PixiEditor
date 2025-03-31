using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using PixiEditor.Views.Input;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class SizeSettingViewModel : Setting<double>
{
    private bool isEnabled = true;
    private double min;
    private double max;
    private int decimalPlaces;
    private string unit = "px";
    
    public SizeSettingViewModel(string name, string label = null, double defaultValue = 1, double min = 1, double max = double.PositiveInfinity,
        int decimalPlaces = 0, string unit = "px")
        : base(name)
    {
        Label = label;
        Value = defaultValue;

        this.min = min;
        this.max = max;
        this.decimalPlaces = decimalPlaces;
        this.unit = unit;
    }

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            SetProperty(ref isEnabled, value);
        }
    }

    public double Min
    {
        get => min;
        set
        {
            SetProperty(ref min, value);
        }
    }

    public double Max
    {
        get => max;
        set
        {
            SetProperty(ref max, value);
        }
    }

    public int DecimalPlaces
    {
        get => decimalPlaces;
        set
        {
            SetProperty(ref decimalPlaces, value);
        }
    }

    public string Unit
    {
        get => unit;
        set
        {
            SetProperty(ref unit, value);
        }
    }
}
