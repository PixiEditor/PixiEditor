using System;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Float2(string name) : ShaderExpressionVariable<VecD>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    public override string ConstantValueString
    {
        get
        {
            string x = ConstantValue.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = ConstantValue.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"float2({x}, {y})";
        }
    }

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

    public Float1 X
    {
        get
        {
            return new Float1($"{VariableName}.x") { ConstantValue = ConstantValue.X, OverrideExpression = _overrideExpression };
        }
    }
    
    public Float1 Y
    {
        get
        {
            return new Float1($"{VariableName}.y") { ConstantValue = ConstantValue.Y, OverrideExpression = _overrideExpression };
        }
    }
    
    public static implicit operator Float2(VecD value) => new Float2("") { ConstantValue = value };
    public static explicit operator VecD(Float2 value) => value.ConstantValue;
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
