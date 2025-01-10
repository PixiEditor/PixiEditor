using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class DuplicateNode_Change : Change
{
    private Guid nodeGuid;
    
    private Guid createdNodeGuid;

    [GenerateMakeChangeAction]
    public DuplicateNode_Change(Guid nodeGuid, Guid newGuid)
    {
        this.nodeGuid = nodeGuid;
        createdNodeGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindNode(nodeGuid, out Node node) && node.GetNodeTypeUniqueName() != OutputNode.UniqueName;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        Node existingNode = target.FindNode(nodeGuid);
        Node clone = existingNode.Clone();
        clone.Id = createdNodeGuid;

        target.NodeGraph.AddNode(clone);

        ignoreInUndo = false;

        return CreateNode_ChangeInfo.CreateFromNode(clone);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNode(createdNodeGuid);
        target.NodeGraph.RemoveNode(node);
        
        node.Dispose();

        return new DeleteNode_ChangeInfo(node.Id);
    }
}
