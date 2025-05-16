using System.Numerics;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Extensions.CommonApi.FlyUI.Events;

public class NumberEventArgs : ElementEventArgs<NumberEventArgs>
{
    public double Value { get; }

    public NumberEventArgs(double value)
    {
        Value = value;
    }

    protected override void SerializeArgs(ByteWriter writer)
    {
        writer.WriteDouble(Value);
    }
}
