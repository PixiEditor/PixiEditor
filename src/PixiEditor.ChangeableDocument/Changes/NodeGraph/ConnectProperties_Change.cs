using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using Drawie.Backend.Core.Shaders.Generation;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class ConnectProperties_Change : Change
{
    public Guid InputNodeId { get; }
    public Guid OutputNodeId { get; }
    public string InputProperty { get; }
    public string OutputProperty { get; }

    private PropertyConnection originalConnection;
    private List<PropertyConnection> originalConnectionsAtOutput;

    [GenerateMakeChangeAction]
    public ConnectProperties_Change(Guid inputNodeId, Guid outputNodeId, string inputProperty, string outputProperty)
    {
        InputNodeId = inputNodeId;
        OutputNodeId = outputNodeId;
        InputProperty = inputProperty;
        OutputProperty = outputProperty;
    }

    public override bool InitializeAndValidate(Document target)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        if (inputNode == null || outputNode == null)
        {
            return false;
        }

        InputProperty? inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty? outputProp = outputNode.GetOutputProperty(OutputProperty);

        if (inputNode == outputNode && outputProp == null)
        {
            var input = inputNode.GetInputProperty(OutputProperty);
            if (input is { Connection: not null })
            {
                outputProp = input.Connection as OutputProperty;
            }
        }

        if (inputProp == null || outputProp == null)
        {
            return false;
        }

        if (IsLoop(inputProp, outputProp))
        {
            return false;
        }

        bool canConnect = CheckTypeCompatibility(inputProp, outputProp);

        if (!canConnect)
        {
            return false;
        }

        originalConnection =
            new PropertyConnection(inputProp.Connection?.Node.Id, inputProp.Connection?.InternalPropertyName);
        originalConnectionsAtOutput = new List<PropertyConnection>();

        foreach (var connection in outputProp.Connections)
        {
            originalConnectionsAtOutput.Add(new PropertyConnection(connection.Node.Id,
                connection.InternalPropertyName));
        }

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        List<IChangeInfo> changes = new();

        if (inputNode == outputNode && outputProp == null)
        {
            var input = inputNode.GetInputProperty(OutputProperty);
            if (input is { Connection: not null })
            {
                outputProp = input.Connection as OutputProperty;

                if (outputProp != null)
                {
                    changes.Add(new ConnectProperty_ChangeInfo(null, inputNode.Id, null, OutputProperty));
                }

                outputProp.DisconnectFrom(input);
            }
        }

        outputProp.ConnectTo(inputProp);

        ignoreInUndo = false;

        changes.Add(new ConnectProperty_ChangeInfo(outputProp.Node.Id, InputNodeId, outputProp.InternalPropertyName,
            InputProperty));
        
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        List<IChangeInfo> changes = new();

        if (inputNode == outputNode && outputProp == null)
        {
            var input = inputNode.GetInputProperty(OutputProperty);

            if (inputProp.Connection != null)
            {
                inputProp.Connection.ConnectTo(input);
                changes.Add(new ConnectProperty_ChangeInfo(inputProp.Connection.Node.Id, input.Node.Id,
                    inputProp.Connection.InternalPropertyName, input.InternalPropertyName));
                
                outputProp = input.Connection as OutputProperty;
            }
        }

        outputProp.DisconnectFrom(inputProp);

        changes.Add(new ConnectProperty_ChangeInfo(null, InputNodeId, null, InputProperty));

        if (originalConnection.NodeId != null)
        {
            Node originalNode = target.FindNode(originalConnection.NodeId.Value);
            IOutputProperty? originalOutput = originalNode.GetOutputProperty(originalConnection.PropertyName);
            originalOutput.ConnectTo(inputProp);

            changes.Add(new ConnectProperty_ChangeInfo(originalOutput.Node.Id, inputProp.Node.Id,
                originalOutput.InternalPropertyName, InputProperty));
        }

        foreach (var connection in originalConnectionsAtOutput)
        {
            if (connection.NodeId.HasValue)
            {
                Node originalNode = target.FindNode(connection.NodeId.Value);
                IInputProperty? originalInput = originalNode.GetInputProperty(connection.PropertyName);
                outputProp.ConnectTo(originalInput);

                changes.Add(new ConnectProperty_ChangeInfo(outputProp.Node.Id, originalInput.Node.Id,
                    outputProp.InternalPropertyName, originalInput.InternalPropertyName));
            }
        }

        return changes;
    }

    private static bool IsLoop(InputProperty input, OutputProperty output)
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

    private static bool CheckTypeCompatibility(InputProperty input, OutputProperty output)
    {
        if (input.ValueType != output.ValueType)
        {
            if (IsCrossExpression(output.Value, input.ValueType))
            {
                return true;
            }

            object? outputValue = output.Value;

            if (IsExpressionToConstant(output, input, out var result))
            {
                outputValue = result;
            }

            if (IsConstantToExpression(input, outputValue, out result))
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

    private static bool IsConstantToExpression(InputProperty input, object objValue, out object result)
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

    private static bool IsExpressionToConstant(OutputProperty output, InputProperty input, out object o)
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
