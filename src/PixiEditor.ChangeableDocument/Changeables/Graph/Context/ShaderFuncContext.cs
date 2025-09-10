using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

public class ShaderFuncContext : FuncContext
{
    public static ShaderFuncContext NoContext { get; } = new();
    /// <summary>
    ///     Original position of the pixel in the image. This is the input argument of the main function.
    /// </summary>
    public Float2 OriginalPosition { get; private set; }

    /// <summary>
    ///     Modified position of the pixel. This should be used to sample the texture, unless you want to sample the texture at the original position only.
    /// </summary>
    public Float2 SamplePosition { get; private set; }

    public VecI Size { get; private set; }
    public RenderContext RenderContext { get; set; }

    public ShaderBuilder Builder { get; set; }

    private Dictionary<IFuncInputProperty, ShaderExpressionVariable> _cachedValues = new();


    public ShaderFuncContext(RenderContext renderContext, ShaderBuilder builder)
    {
        RenderContext = renderContext;
        Builder = builder;
        HasContext = true;
        OriginalPosition = new Float2("coords"); // input argument 'half4 main(float2 coords)'
        SamplePosition = Builder.ConstructFloat2(OriginalPosition.X, OriginalPosition.Y);
    }

    public ShaderFuncContext()
    {

    }

     public Half4 SampleSurface(DrawingSurface surface, Expression pos, ColorSampleMode sampleMode, bool normalizedCoordinates)
    {
        SurfaceSampler texName = Builder.AddOrGetSurface(surface, sampleMode);
        return Builder.Sample(texName, pos, normalizedCoordinates);
    }

    public Float2 NewFloat2(Expression x, Expression y)
    {
        if (!HasContext && x is Float1 firstFloat && y is Float1 secondFloat)
        {
            Float2 constantFloat = new Float2("");
            constantFloat.ConstantValue = new VecD(firstFloat.ConstantValue, secondFloat.ConstantValue);
            return constantFloat;
        }

        return Builder.ConstructFloat2(x, y);
    }

    public Float1 NewFloat1(Expression result)
    {
        if (!HasContext && result is Float1 float1)
        {
            Float1 constantFloat = new Float1("");
            constantFloat.ConstantValue = float1.ConstantValue;
            return constantFloat;
        }

        return Builder.ConstructFloat1(result);
    }


    public Int2 NewInt2(Expression first, Expression second)
    {
        if (!HasContext && first is Int1 firstInt && second is Int1 secondInt)
        {
            Int2 constantInt = new Int2("");
            constantInt.ConstantValue = new VecI(firstInt.ConstantValue, secondInt.ConstantValue);
            return constantInt;
        }

        return Builder.ConstructInt2(first, second);
    }

    public Half4 NewHalf4(Expression r, Expression g, Expression b, Expression a)
    {
        if (!HasContext && r is Float1 firstFloat && g is Float1 secondFloat && b is Float1 thirdFloat &&
            a is Float1 fourthFloat)
        {
            Half4 constantHalf4 = new Half4("");
            byte rByte = firstFloat.AsConstantColorByte();
            byte gByte = secondFloat.AsConstantColorByte();
            byte bByte = thirdFloat.AsConstantColorByte();
            byte aByte = fourthFloat.AsConstantColorByte();
            constantHalf4.ConstantValue = new Color(rByte, gByte, bByte, aByte);
            return constantHalf4;
        }

        return Builder.ConstructHalf4(r, g, b, a);
    }

    public Half4 HsvaToRgba(Expression h, Expression s, Expression v, Expression a)
    {
        if (!HasContext && h is Float1 firstFloat && s is Float1 secondFloat && v is Float1 thirdFloat &&
            a is Float1 fourthFloat)
        {
            Half4 constantHalf4 = new Half4("");
            var hValue = firstFloat.ConstantValue * 360;
            var sValue = secondFloat.ConstantValue * 100;
            var vValue = thirdFloat.ConstantValue * 100;
            byte aByte = fourthFloat.AsConstantColorByte();
            constantHalf4.ConstantValue = Color.FromHsv((float)hValue, (float)sValue, (float)vValue, aByte);
            return constantHalf4;
        }

        return Builder.AssignNewHalf4(Builder.Functions.GetHsvToRgb(h, s, v, a));
    }

    public Half4 HslaToRgba(Expression h, Expression s, Expression l, Expression a)
    {
        if (!HasContext && h is Float1 firstFloat && s is Float1 secondFloat && l is Float1 thirdFloat &&
            a is Float1 fourthFloat)
        {
            Half4 constantHalf4 = new Half4("");
            var hValue = firstFloat.ConstantValue * 360;
            var sValue = secondFloat.ConstantValue * 100;
            var lValue = thirdFloat.ConstantValue * 100;
            byte aByte = fourthFloat.AsConstantColorByte();
            constantHalf4.ConstantValue = Color.FromHsl((float)hValue, (float)sValue, (float)lValue, aByte);
            return constantHalf4;
        }

        return Builder.AssignNewHalf4(Builder.Functions.GetHslToRgb(h, s, l, a));
    }

    public Half4 RgbaToHsva(Expression color)
    {
        if (!HasContext && color is Half4 constantColor)
        {
            var variable = new Half4(string.Empty);
            constantColor.ConstantValue.ToHsv(out float h, out float s, out float l);
            variable.ConstantValue = new Color((byte)(h * 255), (byte)(s * 255), (byte)(l * 255),
                constantColor.ConstantValue.A);

            return variable;
        }

        return Builder.AssignNewHalf4(Builder.Functions.GetRgbToHsv(color));
    }

    public Half4 RgbaToHsla(Expression color)
    {
        if (!HasContext && color is Half4 constantColor)
        {
            var variable = new Half4(string.Empty);
            constantColor.ConstantValue.ToHsl(out float h, out float s, out float l);
            variable.ConstantValue = new Color((byte)(h * 255), (byte)(s * 255), (byte)(l * 255),
                constantColor.ConstantValue.A);

            return variable;
        }

        return Builder.AssignNewHalf4(Builder.Functions.GetRgbToHsl(color));
    }

    public Half4 NewHalf4(Expression assignment)
    {
        if (!HasContext && assignment is Half4 half4)
        {
            Half4 constantHalf4 = new Half4("");
            constantHalf4.ConstantValue = half4.ConstantValue;
            return constantHalf4;
        }

        return Builder.AssignNewHalf4(assignment);
    }

    public Float1 GetValue(FuncInputProperty<Float1, ShaderFuncContext> getFrom)
    {
        if (HasContext)
        {
            if (getFrom.Connection == null || !IsFuncType(getFrom))
            {
                string uniformName = $"float_{Builder.GetUniqueNameNumber()}";
                Builder.AddUniform(uniformName, (float)getFrom.Value(this).ConstantValue);
                return new Float1(uniformName);
            }

            if (_cachedValues.TryGetValue(getFrom, out ShaderExpressionVariable cachedValue))
            {
                if (cachedValue is Float1 float1)
                {
                    return float1;
                }
            }
        }


        var val = getFrom.Value(this);
        _cachedValues[getFrom] = val;

        return val;
    }

    public Int1 GetValue(FuncInputProperty<Int1, ShaderFuncContext> getFrom)
    {
        if (HasContext)
        {
            if (getFrom.Connection == null || !IsFuncType(getFrom))
            {
                string uniformName = $"int_{Builder.GetUniqueNameNumber()}";
                Builder.AddUniform(uniformName, (int)getFrom.Value(this).ConstantValue);
                return new Int1(uniformName);
            }

            if (_cachedValues.TryGetValue(getFrom, out ShaderExpressionVariable cachedValue))
            {
                if (cachedValue is Int1 int1)
                {
                    return int1;
                }
            }
        }

        var val = getFrom.Value(this);
        _cachedValues[getFrom] = val;

        return val;
    }

    private static bool IsFuncType<T>(FuncInputProperty<T, ShaderFuncContext> getFrom)
    {
        return getFrom.Connection.ValueType.IsAssignableTo(typeof(Delegate));
    }

    public Half4 GetValue(FuncInputProperty<Half4, ShaderFuncContext> getFrom)
    {
        if (HasContext)
        {
            if (getFrom.Connection == null || !IsFuncType(getFrom))
            {
                Half4 color = getFrom.Value(this);
                color.VariableName = $"color_{Builder.GetUniqueNameNumber()}";
                Builder.AddUniform(color.VariableName, color.ConstantValue);
                return color;
            }

            if (_cachedValues.TryGetValue(getFrom, out ShaderExpressionVariable cachedValue))
            {
                if (cachedValue is Half4 half4)
                {
                    return half4;
                }
            }
        }

        var val = getFrom.Value(this);
        _cachedValues[getFrom] = val;

        return val;
    }

    public Float2 GetValue(FuncInputProperty<Float2, ShaderFuncContext> getFrom)
    {
        if (HasContext)
        {
            if (getFrom.Connection == null || !IsFuncType(getFrom))
            {
                Float2 value = getFrom.Value(this);
                value.VariableName = $"float2_{Builder.GetUniqueNameNumber()}";
                Builder.AddUniform(value.VariableName, value.ConstantValue);
                return value;
            }

            if (_cachedValues.TryGetValue(getFrom, out ShaderExpressionVariable cachedValue))
            {
                if (cachedValue is Float2 float2)
                {
                    return float2;
                }
            }
        }

        var val = getFrom.Value(this);
        _cachedValues[getFrom] = val;

        return val;
    }

    public Int2 GetValue(FuncInputProperty<Int2, ShaderFuncContext>? getFrom)
    {
        if (HasContext)
        {
            if (getFrom?.Connection == null || !IsFuncType(getFrom))
            {
                Int2 value = getFrom.Value(this);
                value.VariableName = $"int2_{Builder.GetUniqueNameNumber()}";
                Builder.AddUniform(value.VariableName, value.ConstantValue);
                return value;
            }

            if (_cachedValues.TryGetValue(getFrom, out ShaderExpressionVariable cachedValue))
            {
                if (cachedValue is Int2 int2)
                {
                    return int2;
                }
            }
        }

        var val = getFrom.Value(this);
        _cachedValues[getFrom] = val;

        return val;
    }

    public Float3x3 GetValue(FuncInputProperty<Float3x3, ShaderFuncContext> getFrom)
    {
        if (HasContext)
        {
            if (getFrom.Connection == null || !IsFuncType(getFrom))
            {
                Float3x3 value = getFrom.Value(this);
                value.VariableName = $"float3x3_{Builder.GetUniqueNameNumber()}";
                Builder.AddUniform(value.VariableName, value.ConstantValue);
                return value;
            }

            if (_cachedValues.TryGetValue(getFrom, out ShaderExpressionVariable cachedValue))
            {
                if (cachedValue is Float3x3 float3x3)
                {
                    return float3x3;
                }
            }
        }

        var val = getFrom.Value(this);
        _cachedValues[getFrom] = val;

        return val;
    }

    public Float3x3 NewFloat3x3(Expression m00, Expression m01, Expression m02,
        Expression m10, Expression m11, Expression m12,
        Expression m20, Expression m21, Expression m22)
    {
        if (!HasContext && m00 is Float1 firstFloat && m01 is Float1 secondFloat && m02 is Float1 thirdFloat &&
            m10 is Float1 fourthFloat && m11 is Float1 fifthFloat && m12 is Float1 sixthFloat &&
            m20 is Float1 seventhFloat && m21 is Float1 eighthFloat && m22 is Float1 ninthFloat)
        {
            Float3x3 constantMatrix = new Float3x3("");
            constantMatrix.ConstantValue = new Matrix3X3(
                (float)firstFloat.ConstantValue, (float)secondFloat.ConstantValue, (float)thirdFloat.ConstantValue,
                (float)fourthFloat.ConstantValue, (float)fifthFloat.ConstantValue, (float)sixthFloat.ConstantValue,
                (float)seventhFloat.ConstantValue, (float)eighthFloat.ConstantValue, (float)ninthFloat.ConstantValue);
            return constantMatrix;
        }

        return Builder.ConstructFloat3x3(m00, m01, m02, m10, m11, m12, m20, m21, m22);
    }

    public Float3x3 NewFloat3x3(Expression matrixExpression)
    {
        if (!HasContext && matrixExpression is Float3x3 float3x3)
        {
            Float3x3 constantMatrix = new Float3x3("");
            constantMatrix.ConstantValue = float3x3.ConstantValue;
            return constantMatrix;
        }

        return Builder.AssignNewFloat3x3(matrixExpression);
    }
}
