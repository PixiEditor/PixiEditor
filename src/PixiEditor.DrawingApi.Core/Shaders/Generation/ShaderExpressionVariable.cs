using System;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public abstract class ShaderExpressionVariable(string name) : Expression
{
    public string VariableName { get; set; } = name;
    public abstract string ConstantValueString { get; }

    public override string ToString()
    {
        return VariableName;
    }

    public override string ExpressionValue => VarOrConst();

    public abstract void SetConstantValue(object? value, Func<object, Type, object> convertFunc);
    
    public string VarOrConst()
    {
        return string.IsNullOrEmpty(VariableName) ? ConstantValueString : VariableName;
    }
}

public abstract class ShaderExpressionVariable<TConstant>(string name)
    : ShaderExpressionVariable(name)
{
    public TConstant? ConstantValue { get; set; }

    public override void SetConstantValue(object? value, Func<object, Type, object> convertFunc)
    {
        if (value is TConstant constantValue)
        {
            ConstantValue = constantValue;
        }
        else
        {
            try
            {
                constantValue = (TConstant)convertFunc(value, typeof(TConstant));
                ConstantValue = constantValue;
            }
            catch (InvalidCastException)
            {
                ConstantValue = default;
            }
        }
    }
}
