using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class DeleteNode_Change : Change
{
    public Guid NodeId { get; set; }

    private ConnectionsData originalConnections;

    private Node savedCopy;

    [GenerateMakeChangeAction]
    public DeleteNode_Change(Guid nodeId)
    {
        NodeId = nodeId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        Node node = target.FindNode<Node>(NodeId);

        if (node is null)
            return false;

        originalConnections = NodeOperations.CreateConnectionsData(node);

        savedCopy = node.Clone();

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        var node = target.FindNode<Node>(NodeId);

        List<IChangeInfo> changes = NodeOperations.DetachNode(target.NodeGraph, node);

        target.NodeGraph.RemoveNode(node);

        changes.Add(new DeleteNode_ChangeInfo(NodeId));

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document doc)
    {
        var copy = savedCopy!.Clone();
        copy.Id = NodeId;

        doc.NodeGraph.AddNode(copy);

        List<IChangeInfo> changes = new();

        IChangeInfo createChange = CreateNode_ChangeInfo.CreateFromNode(copy);

        changes.Add(createChange);

        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(originalConnections, copy, doc.NodeGraph));

        return changes;
    }

    public override void Dispose()
    {
        savedCopy?.Dispose();
    }
}
