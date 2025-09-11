using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class GetComputedPropertyValue_Change : Change
{
    private readonly Guid nodeId;
    private readonly string propertyName;
    private readonly bool isInput;

    [GenerateMakeChangeAction]
    public GetComputedPropertyValue_Change(Guid nodeId, string propertyName, bool isInput)
    {
        this.nodeId = nodeId;
        this.propertyName = propertyName;
        this.isInput = isInput;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var foundNode = target.FindNode(nodeId);
        if (foundNode == null)
        {
            return false;
        }

        if (isInput)
        {
            return foundNode.InputProperties.Any(x => x.InternalPropertyName == propertyName);
        }

        return foundNode.OutputProperties.Any(x => x.InternalPropertyName == propertyName);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var node = target.FindNode(nodeId);
        ignoreInUndo = true;

        if (node == null)
        {
            return new None();
        }

        object value;
        if (isInput)
        {
            value = node.GetInputProperty(propertyName).Value;
        }
        else
        {
            value = node.GetOutputProperty(propertyName).Value;
        }
        if (value is Delegate del)
        {
            try
            {
                value = del.DynamicInvoke(ShaderFuncContext.NoContext);
            }
            catch (Exception e)
            {
                return new None();
            }
        }
        if(value is ShaderExpressionVariable variable)
        {
            value = variable.GetConstant();
        }

        return new ComputedPropertyValue_ChangeInfo(nodeId, propertyName, isInput, value);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return new None();
    }
}
