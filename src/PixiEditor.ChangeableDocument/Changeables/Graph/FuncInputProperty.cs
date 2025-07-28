using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class FuncInputProperty<T> : InputProperty<Func<FuncContext, T>>, IFuncInputProperty
{
    private T? constantNonOverrideValue;
    private int lastConstantHashCode;

    internal FuncInputProperty(Node node, string internalName, string displayName, T defaultValue) : base(node,
        internalName, displayName, null)
    {
        constantNonOverrideValue = defaultValue;
        NonOverridenValue = _ => constantNonOverrideValue;
    }

    protected override void NonOverridenValueSet(Func<FuncContext, T> value)
    {
        constantNonOverrideValue = value(FuncContext.NoContext);
    }

    protected internal override object FuncFactory(object toReturn)
    {
        Func<FuncContext, T> func = _ =>
        {
            if (typeof(T).IsAssignableTo(typeof(ShaderExpressionVariable)))
            {
                var shaderExpressionVariable = (ShaderExpressionVariable)Activator.CreateInstance(typeof(T), "");
                shaderExpressionVariable.SetConstantValue(toReturn, ConversionTable.Convert);
                return (T)(object)shaderExpressionVariable;
            }

            return (T)toReturn;
        };
        return func;
    }

    protected override object FuncFactoryDelegate(Delegate delegateToCast)
    {
        Func<FuncContext, T> func = f =>
        {
            Type targetType = typeof(T);
            bool isShaderExpression = false;
            if (typeof(T).IsAssignableTo(typeof(ShaderExpressionVariable)))
            {
                targetType = targetType.BaseType.GenericTypeArguments[0];
                isShaderExpression = true;
            }

            var sourceObj = delegateToCast.DynamicInvoke(f);
            ConversionTable.TryConvert(sourceObj, targetType, out var result);
            if (isShaderExpression)
            {
                var toReturn = Activator.CreateInstance(typeof(T), "");
                if (result != null)
                {
                    ((ShaderExpressionVariable)toReturn).SetConstantValue(result, ConversionTable.Convert);
                }
                else if (sourceObj is Expression expression)
                {
                    ShaderExpressionVariable shaderExpressionVariable = (ShaderExpressionVariable)toReturn;
                    shaderExpressionVariable.OverrideExpression = Adjust(expression, toReturn, out var adjustNested);
                    if (adjustNested)
                    {
                        AdjustNested(((IMultiValueVariable)toReturn), expression);
                    }
                }

                return (T)toReturn;
            }

            return result == null ? default : (T)result;
        };
        return func;
    }

    private Expression Adjust(Expression expression, object toReturn, out bool adjustNestedVariables)
    {
        adjustNestedVariables = false;
        if (expression is IMultiValueVariable multiVal && toReturn is not IMultiValueVariable)
        {
            return multiVal.GetValueAt(0);
        }

        if (toReturn is IMultiValueVariable)
        {
            adjustNestedVariables = true;
            return expression;
        }

        return expression;
    }

    private void AdjustNested(IMultiValueVariable toReturn, Expression expression)
    {
        if (toReturn is not ShaderExpressionVariable shaderExpressionVariable)
        {
            return;
        }

        if (expression is not IMultiValueVariable multiVal)
        {
            int count = toReturn.GetValuesCount();
            for (int i = 0; i < count; i++)
            {
                toReturn.OverrideExpressionAt(i, expression);
            }
        }
        else
        {
            int sourceCount = multiVal.GetValuesCount();
            int targetCount = toReturn.GetValuesCount();

            int count = Math.Min(sourceCount, targetCount);
            for (int i = 0; i < count; i++)
            {
                toReturn.OverrideExpressionAt(i, multiVal.GetValueAt(i));
            }
        }
    }

    object? IFuncInputProperty.GetFuncConstantValue() => constantNonOverrideValue;

    void IFuncInputProperty.SetFuncConstantValue(object? value)
    {
        if (value is T)
        {
            constantNonOverrideValue = (T)value;
            return;
        }

        if (constantNonOverrideValue is ShaderExpressionVariable shaderExpressionVariable)
        {
            shaderExpressionVariable.SetConstantValue(value, ConversionTable.Convert);
            NonOverridenValue = _ => constantNonOverrideValue;
            return;
        }

        if (ConversionTable.TryConvert(value, typeof(T), out var result))
        {
            constantNonOverrideValue = (T)result;
            return;
        }

        constantNonOverrideValue = default;
    }

    internal override bool CacheChanged
    {
        get
        {
            if (constantNonOverrideValue == null)
            {
                return base.CacheChanged;
            }

            if (Connection == null && lastConnectionHash != -1)
            {
                return true;
            }

            if (Connection != null && lastConnectionHash != Connection.GetHashCode())
            {
                lastConnectionHash = Connection.GetHashCode();
                return true;
            }

            if (constantNonOverrideValue is ShaderExpressionVariable expressionVariable)
            {
                return expressionVariable.ConstantValueString.GetHashCode() != lastConstantHashCode;
            }

            return base.CacheChanged;
        }
    }

    internal override void UpdateCache()
    {
        base.UpdateCache();
        if (constantNonOverrideValue is ShaderExpressionVariable expressionVariable)
        {
            lastConstantHashCode = expressionVariable.ConstantValueString.GetHashCode();
        }
    }
}
