using System;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public abstract class ShaderExpressionVariable(string name)
{
    public string UniformName { get; set; } = name;
    public abstract string ConstantValueString { get; }

    public override string ToString()
    {
        return UniformName;
    }

    public abstract void SetConstantValue(object? value, Func<object, Type, object> convertFunc);
}

public abstract class ShaderExpressionVariable<TConstant>(string name, TConstant constant)
    : ShaderExpressionVariable(name)
{
    public TConstant? ConstantValue { get; set; } = constant;

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
