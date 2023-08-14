using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.AvaloniaUI.Views.Input;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Settings;

internal sealed class FloatSetting : Setting<float>
{
    public FloatSetting(
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

    public float Min { get; set; }

    public float Max { get; set; }

    private NumberInput GenerateNumberInput()
    {
        var numbrInput = new NumberInput
        {
            Width = 40,
            Height = 20,
            Min = Min,
            Max = Max
        };
        var binding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        };
        numbrInput.Bind(NumberInput.ValueProperty, binding);
        return numbrInput;
    }

    public override Control GenerateControl()
    {
        return GenerateNumberInput();
    }
}
