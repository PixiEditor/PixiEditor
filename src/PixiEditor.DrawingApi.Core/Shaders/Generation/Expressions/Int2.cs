using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Int2(string name) : ShaderExpressionVariable<VecI>(name)
{
    public override string ConstantValueString => $"int2({ConstantValue.X}, {ConstantValue.Y})";

    public Int1 X
    {
        get => new Int1($"{VariableName}.x");
    }

    public Int1 Y
    {
        get => new Int1($"{VariableName}.y");
    }

    public static implicit operator Int2(VecI value) => new Int2("") { ConstantValue = value };
    public static explicit operator VecI(Int2 value) => value.ConstantValue;
}
