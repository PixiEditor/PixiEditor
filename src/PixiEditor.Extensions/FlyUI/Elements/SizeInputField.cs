using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.UI.Common.Controls;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class SizeInputField : LayoutElement
{
    public event ElementEventHandler<NumberEventArgs> SizeChanged
    {
        add => AddEvent(nameof(SizeChanged), value);
        remove => RemoveEvent(nameof(SizeChanged), value);
    }

    public double Value { get; set; }
    public double Min { get; set; } = 1;
    public double Max { get; set; }
    public int Decimals { get; set; }
    public string Unit { get; set; }

    protected override Control CreateNativeControl()
    {
        SizeInput sizeInput = new SizeInput { MinSize = Min, MaxSize = Max, Decimals = Decimals, Unit = Unit };

        Binding binding = new Binding { Source = this, Path = nameof(Value), Mode = BindingMode.TwoWay, };

        sizeInput.Bind(SizeInput.SizeProperty, binding);

        sizeInput.PropertyChanged += (sender, args) =>
        {
            if (args.Property != SizeInput.SizeProperty) return;

            Value = sizeInput.Size;
            RaiseEvent(nameof(SizeChanged), new NumberEventArgs(Value));
        };

        return sizeInput;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        Value = (double)values[0];
        Min = (double)values[1];
        Max = (double)values[2];
        Decimals = (int)values[3];
        Unit = (string)values[4];
    }
}
