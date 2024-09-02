using System;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Int2(string name) : ShaderExpressionVariable<VecI>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    public override string ConstantValueString => $"int2({ConstantValue.X}, {ConstantValue.Y})";

    public Int1 X
    {
        get => new Int1($"{VariableName}.x") { ConstantValue = ConstantValue.X, OverrideExpression = _overrideExpression };
    }

    public Int1 Y
    {
        get => new Int1($"{VariableName}.y") { ConstantValue = ConstantValue.Y, OverrideExpression = _overrideExpression };
    }

    public static implicit operator Int2(VecI value) => new Int2("") { ConstantValue = value };
    public static explicit operator VecI(Int2 value) => value.ConstantValue;
    
    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set
        {
            _overrideExpression = value;
            X.OverrideExpression = value;
            Y.OverrideExpression = value;
        }
    }

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => X,
            1 => Y,
            _ => throw new IndexOutOfRangeException()
        };
    }
}
