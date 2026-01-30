using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class DisconnectProperty_Change : Change
{
    private Guid nodeGuid;
    private string property;
    
    private IOutputProperty? originalConnection;

    [GenerateMakeChangeAction]
    public DisconnectProperty_Change(Guid nodeGuid, string inputProperty)
    {
        this.nodeGuid = nodeGuid;
        this.property = inputProperty;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindNode<Node>(nodeGuid, out var node))
            return false;
        
        var inputProp = node.GetInputProperty(property) != null;
        if (!inputProp)
            return false;
        
        originalConnection = node.GetInputProperty(property).Connection;
        if (originalConnection is null)
            return false;
        
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var node = target.FindNodeOrThrow<Node>(nodeGuid);

        int inputsHash = GraphUtils.CalculateInputsHash(node);
        int outputsHash = GraphUtils.CalculateOutputsHash(node);

        var input = node.GetInputProperty(property);
        input.Connection.DisconnectFrom(input);

        ignoreInUndo = false;

        int newInputsHash = GraphUtils.CalculateInputsHash(node);
        int newOutputsHash = GraphUtils.CalculateOutputsHash(node);

        List<IChangeInfo> changes = new() { new ConnectProperty_ChangeInfo(null, node.Id, null, property) };

        if (inputsHash != newInputsHash)
            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(node));

        if (outputsHash != newOutputsHash)
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(node));

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNodeOrThrow<Node>(nodeGuid);
        var input = node.GetInputProperty(property);

        int inputsHash = GraphUtils.CalculateInputsHash(node);
        int outputsHash = GraphUtils.CalculateOutputsHash(node);
        
        var targetNode = target.FindNodeOrThrow<Node>(originalConnection!.Node.Id);
        var targetOutput = targetNode.GetOutputProperty(originalConnection.InternalPropertyName);
        
        targetOutput.ConnectTo(input);

        int newInputsHash = GraphUtils.CalculateInputsHash(node);
        int newOutputsHash = GraphUtils.CalculateOutputsHash(node);

        var propInfo = new ConnectProperty_ChangeInfo(targetNode.Id, node.Id, targetOutput.InternalPropertyName, property);

        List<IChangeInfo> changes = new() { propInfo };

        if (inputsHash != newInputsHash)
            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(node));

        if (outputsHash != newOutputsHash)
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(node));

        return changes;
    }
}
