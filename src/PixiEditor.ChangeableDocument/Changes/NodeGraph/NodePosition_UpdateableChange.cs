using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class NodePosition_UpdateableChange : UpdateableChange
{
    public Guid NodeId { get; }
    public VecD NewPosition { get; private set; } 
    
    private VecD originalPosition;
    
    [GenerateUpdateableChangeActions]
    public NodePosition_UpdateableChange(Guid nodeId, VecD newPosition)
    {
        NodeId = nodeId;
        NewPosition = newPosition;
    }

    [UpdateChangeMethod]
    public void Update(VecD newPosition)
    {
        NewPosition = newPosition;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        var node = target.FindNode<Node>(NodeId);
        if (node == null)
        {
            return false;
        }

        originalPosition = node.Position;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var node = target.FindNode<Node>(NodeId);
        node.Position = NewPosition;
        return new NodePosition_ChangeInfo(NodeId, NewPosition);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        var node = target.FindNode<Node>(NodeId);
        node.Position = NewPosition;
        return new NodePosition_ChangeInfo(NodeId, NewPosition);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNode<Node>(NodeId);
        node.Position = originalPosition;
        return new NodePosition_ChangeInfo(NodeId, originalPosition);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is NodePosition_UpdateableChange change && change.NodeId == NodeId;
    }
}
