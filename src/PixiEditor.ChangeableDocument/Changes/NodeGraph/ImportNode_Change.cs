using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class ImportNode_Change : Change
{
    private ICrossDocumentPipe<IReadOnlyNode> sourceDocumentPipe;
    private Guid duplicateGuid;
    private ConnectionsData connectionsData;
    private Node? cloned;

    [GenerateMakeChangeAction]
    public ImportNode_Change(ICrossDocumentPipe<IReadOnlyNode> pipe, Guid newGuid)
    {
        sourceDocumentPipe = pipe;
        duplicateGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (sourceDocumentPipe is not { CanOpen: true })
            return false;

        if (!sourceDocumentPipe.IsOpen)
        {
            sourceDocumentPipe.Open();
        }

        IReadOnlyNode? node = sourceDocumentPipe.TryAccessData();
        if (node == null || target.NodeGraph.OutputNode == null)
            return false;

        connectionsData = NodeOperations.CreateConnectionsData(target.NodeGraph.OutputNode);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;

        var node = cloned ?? sourceDocumentPipe.TryAccessData();
        if (node is not Node graphNode)
        {
            ignoreInUndo = true;
            return new None();
        }

        var clone = (Node)graphNode.Clone();
        clone.Id = duplicateGuid;
        cloned = clone;

        target.NodeGraph.AddNode(clone);

        return CreateNode_ChangeInfo.CreateFromNode(clone);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNode(duplicateGuid);

        target.NodeGraph.RemoveNode(node);
        node.Dispose();

        List<IChangeInfo> changes = new();

        changes.AddRange(NodeOperations.DetachNode(node));
        changes.Add(new DeleteNode_ChangeInfo(node.Id));

        if (connectionsData is not null)
        {
            Node originalNode = target.NodeGraph.OutputNode;
            changes.AddRange(
                NodeOperations.ConnectStructureNodeProperties(connectionsData, originalNode, target.NodeGraph));
        }

        return changes;
    }

    public override void Dispose()
    {
        base.Dispose();
        sourceDocumentPipe.Dispose();
        cloned?.Dispose();
    }
}
