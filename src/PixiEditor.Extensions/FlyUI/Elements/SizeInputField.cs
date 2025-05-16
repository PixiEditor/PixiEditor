using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.UI.Common.Controls;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class SizeInputField : LayoutElement
{
    private double value;
    private double min = 1;
    private double max;
    private int decimals;
    private string unit;

    public event ElementEventHandler<NumberEventArgs> SizeChanged
    {
        add => AddEvent(nameof(SizeChanged), value);
        remove => RemoveEvent(nameof(SizeChanged), value);
    }

    public double Value
    {
        get => value;
        set => SetField(ref this.value, value, nameof(Value));
    }
    public double Min { get => min; set => SetField(ref min, value); }
    public double Max { get => max; set => SetField(ref max, value); }
    public int Decimals { get => decimals; set => SetField(ref decimals, value); }
    public string Unit { get => unit; set => SetField(ref unit, value); }

    private bool suppressSizeChanged;

    protected override Control CreateNativeControl()
    {
        SizeInput sizeInput = new SizeInput { MinSize = Min, MaxSize = Max, Decimals = Decimals, Unit = Unit };

        Binding binding = new Binding { Source = this, Path = nameof(Value), Mode = BindingMode.TwoWay, };

        sizeInput.Bind(SizeInput.SizeProperty, binding);

        sizeInput.PropertyChanged += (sender, args) =>
        {
            if (args.Property != SizeInput.SizeProperty || suppressSizeChanged) return;

            Value = sizeInput.Size;
            RaiseEvent(nameof(SizeChanged), new NumberEventArgs(Value));
        };

        return sizeInput;
    }

    protected override IEnumerable<object> GetControlProperties()
    {
        yield return Value;
        yield return Min;
        yield return Max;
        yield return Decimals;
        yield return Unit;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        suppressSizeChanged = true;
        Value = (double)values[0];
        Min = (double)values[1];
        Max = (double)values[2];
        Decimals = (int)values[3];
        Unit = (string)values[4];
        suppressSizeChanged = false;
    }
}
