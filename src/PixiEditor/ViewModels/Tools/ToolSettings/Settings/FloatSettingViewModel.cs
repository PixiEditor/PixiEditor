using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Views.Input;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class FloatSettingViewModel : Setting<float>
{
    private float min = float.NegativeInfinity;
    private float max = float.PositiveInfinity;
    private int decimalPlaces = 2;
    
    public FloatSettingViewModel(
        string name,
        float initialValue,
        string label = "",
        float min = float.NegativeInfinity,
        float max = float.PositiveInfinity)
        : base(name)
    {
        Label = label;
        Value = initialValue;
        Min = min;
        Max = max;
    }

    public float Min
    {
        get => min;
        set
        {
            SetProperty(ref min, value);
        }
    }

    public float Max
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
}
