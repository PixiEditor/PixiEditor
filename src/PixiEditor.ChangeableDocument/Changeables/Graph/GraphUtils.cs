using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public static class GraphUtils
{
    public static Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode,
        Func<IInputProperty, bool>? branchFilter = null)
    {
        var finalQueue = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (finalQueue.Contains(node))
            {
                continue;
            }

            bool canAdd = true;

            foreach (var input in node.InputProperties)
            {
                if (input.Connection == null)
                {
                    continue;
                }

                if (finalQueue.Contains(input.Connection.Node))
                {
                    continue;
                }

                if (branchFilter != null && !branchFilter(input))
                {
                    continue;
                }

                canAdd = false;

                if (finalQueue.Contains(input.Connection.Node))
                {
                    finalQueue.Remove(input.Connection.Node);
                    finalQueue.Add(input.Connection.Node);
                }

                if (!queueNodes.Contains(input.Connection.Node))
                {
                    queueNodes.Enqueue(input.Connection.Node);
                }
            }

            if (canAdd)
            {
                finalQueue.Add(node);
            }
            else
            {
                queueNodes.Enqueue(node);
            }
        }

        return new Queue<IReadOnlyNode>(finalQueue);
    }

    public static int CalculateInputsHash(Node node)
    {
        HashCode hash = new();
        foreach (var input in node.InputProperties)
        {
            hash.Add(input.InternalPropertyName);
            hash.Add(input.ValueType);
        }

        return hash.ToHashCode();
    }

    public static int CalculateOutputsHash(Node node)
    {
        HashCode hash = new();
        foreach (var output in node.OutputProperties)
        {
            hash.Add(output.InternalPropertyName);
            hash.Add(output.ValueType);
        }

        return hash.ToHashCode();
    }

     public static bool IsLoop(IInputProperty input, OutputProperty output)
    {
        if (input.Node == output.Node)
        {
            return true;
        }

        if (input.Node.OutputProperties.Any(x => x.Connections.Any(y => y.Node == output.Node)))
        {
            return true;
        }

        bool isLoop = false;
        input.Node.TraverseForwards(x =>
        {
            if (x == output.Node)
            {
                isLoop = true;
                return false;
            }

            return true;
        });

        return isLoop;
    }

    public static object SetNonOverwrittenValue(InputProperty property, object? value)
    {
        if (property is IFuncInputProperty fieldInput)
        {
            fieldInput.SetFuncConstantValue(value);
        }
        else
        {
            if (value is int && property.ValueType.IsEnum)
            {
                value = Enum.ToObject(property.ValueType, value);
            }

            property.NonOverridenValue = value;
        }

        return value;
    }

    public static bool CheckTypeCompatibility(IInputProperty input, IOutputProperty output)
    {
        if (input.ValueType != output.ValueType)
        {
            if (IsCrossExpression(output.Value, input.ValueType))
            {
                return true;
            }

            object? outputValue = output.Value;

            if (IsExpressionToConstant(output, out var result))
            {
                outputValue = result;
            }

            if(outputValue == null && (output.ValueType == typeof(object) || IsExpressionType(output)))
            {
                return true;
            }

            if (IsConstantToExpression(input, out result))
            {
                return ConversionTable.TryConvert(result, output.ValueType, out _);
            }

            if (output.ValueType.IsAssignableTo(input.ValueType))
            {
                return true;
            }

            if (outputValue != null && ConversionTable.TryConvert(outputValue, input.ValueType, out _))
            {
                return true;
            }

            if (outputValue == null)
            {
                return ConversionTable.CanConvertType(input.ValueType, output.ValueType);
            }

            return false;
        }

        return true;
    }

    private static bool IsConstantToExpression(IInputProperty input, out object result)
    {
        if (input.Value is Delegate func && func.Method.ReturnType.IsAssignableTo(typeof(ShaderExpressionVariable)))
        {
            try
            {
                var actualArg = func.DynamicInvoke(FuncContext.NoContext);
                if (actualArg is ShaderExpressionVariable variable)
                {
                    result = variable.GetConstant();
                    return true;
                }
            }
            catch
            {
                result = null;
                return false;
            }
        }

        result = null;
        return false;
    }

    private static bool IsExpressionType(IOutputProperty output)
    {
        return output.ValueType.IsAssignableTo(typeof(Delegate));
    }

    private static bool IsExpressionToConstant(IOutputProperty output, out object o)
    {
        if (output.Value is Delegate func && func.Method.ReturnType.IsAssignableTo(typeof(ShaderExpressionVariable)))
        {
            try
            {
                o = func.DynamicInvoke(FuncContext.NoContext);
                if (o is ShaderExpressionVariable variable)
                {
                    o = variable.GetConstant();
                }

                return true;
            }
            catch
            {
                o = null;
                return false;
            }
        }

        o = null;
        return false;
    }

    private static bool IsCrossExpression(object first, Type secondType)
    {
        if (first is Delegate func && func.Method.ReturnType.IsAssignableTo(typeof(ShaderExpressionVariable)))
        {
            return secondType.IsAssignableTo(typeof(Delegate));
        }

        return false;
    }
}
