using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class SizeInputField : LayoutElement
{
    public event ElementEventHandler<NumberEventArgs> SizeChanged
    {
        add => AddEvent(nameof(SizeChanged), value);
        remove => RemoveEvent(nameof(SizeChanged), value);
    }
    public double Value { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public int Decimals { get; set; }
    public string Unit { get; set; }

    public SizeInputField(double value = 0, double min = 1, double max = double.MaxValue, int decimals = 0, string unit = "px", Cursor? cursor = null) : base(cursor)
    {
        Value = value;
        Min = min;
        Max = max;
        Decimals = decimals;
        Unit = unit;
        SizeChanged += e => Value = e.Value;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition field = new ControlDefinition(UniqueId, "SizeInputField");
        field.AddProperty(Value);
        field.AddProperty(Min);
        field.AddProperty(Max);
        field.AddProperty(Decimals);
        field.AddProperty(Unit);
        return field;
    }
}
