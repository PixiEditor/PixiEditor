using System.Windows.Controls;
using System.Windows.Data;
using PixiEditor.Views;

namespace PixiEditor.Models.Tools.ToolSettings.Settings;

public class FloatSetting : Setting<float>
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
        NumberInput numbrInput = new NumberInput
        {
            Width = 40,
            Height = 20,
            Min = Min,
            Max = Max
        };
        Binding binding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        };
        numbrInput.SetBinding(NumberInput.ValueProperty, binding);
        return numbrInput;
    }

    public override Control GenerateControl()
    {
        return GenerateNumberInput();
    }
}