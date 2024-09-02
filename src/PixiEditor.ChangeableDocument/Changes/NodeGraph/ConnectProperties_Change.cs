using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.DrawingApi.Core.Shaders.Generation;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class ConnectProperties_Change : Change
{
    public Guid InputNodeId { get; }
    public Guid OutputNodeId { get; }
    public string InputProperty { get; }
    public string OutputProperty { get; }

    private IOutputProperty? originalConnection;

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

        if (inputProp == null || outputProp == null)
        {
            return false;
        }
        
        if(IsLoop(inputProp, outputProp))
        {
            return false;
        }

        bool canConnect = CheckTypeCompatibility(inputProp, outputProp);

        if (!canConnect)
        {
            return false;
        }

        originalConnection = inputProp.Connection;

        return true;
    }
    
    private bool IsLoop(InputProperty input, OutputProperty output)
    {
        if (input.Node == output.Node)
        {
            return true;
        }
        if(input.Node.OutputProperties.Any(x => x.Connections.Any(y => y.Node == output.Node)))
        {
            return true;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        outputProp.ConnectTo(inputProp);

        ignoreInUndo = false;

        return new ConnectProperty_ChangeInfo(outputNode.Id, inputNode.Id, OutputProperty, InputProperty);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        outputProp.DisconnectFrom(inputProp);

        ConnectProperty_ChangeInfo change = new(null, InputNodeId, null, InputProperty);

        inputProp.Connection = originalConnection;

        List<IChangeInfo> changes = new() { change, };

        if (originalConnection != null)
        {
            ConnectProperty_ChangeInfo oldConnection = new(originalConnection.Node.Id, InputNodeId,
                originalConnection.InternalPropertyName, InputProperty);
            changes.Add(oldConnection);
        }


        return changes;
    }

    private static bool CheckTypeCompatibility(InputProperty input, OutputProperty output)
    {
        if (input.ValueType != output.ValueType)
        {
            if (IsCrossExpression(output.Value, input.ValueType))
            {
                return true;
            }
            
            var outputValue = output.Value;
            
            if(IsExpressionToConstant(output, input, out var result))
            {
                outputValue = result;
            }
            
            if(IsConstantToExpression(input, outputValue, out result))
            {
                return ConversionTable.TryConvert(result, output.ValueType, out _);
            }

            if (outputValue == null)
            {
                return true;
            }
            
            if (outputValue.GetType().IsAssignableTo(input.ValueType))
            {
                return true;
            }

            if (ConversionTable.TryConvert(outputValue, input.ValueType, out _))
            {
                return true;
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
                if(actualArg is ShaderExpressionVariable variable)
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
                if(o is ShaderExpressionVariable variable)
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
