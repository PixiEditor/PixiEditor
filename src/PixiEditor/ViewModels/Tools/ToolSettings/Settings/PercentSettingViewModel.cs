using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Views.Input;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class PercentSettingViewModel : Setting<float>
{
    private float min = 0;
    private float max = 1;
    
    public PercentSettingViewModel(
        string name,
        float initialValue,
        string label = "",
        float min = 0,
        float max = 1)
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
}
