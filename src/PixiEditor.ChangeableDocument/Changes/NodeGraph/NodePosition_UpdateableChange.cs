using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class NodePosition_UpdateableChange : UpdateableChange
{
    public Guid[] NodeIds { get; }
    public VecD NewPosition { get; private set; }

    private Dictionary<Guid, VecD> originalPositions;
    
    private VecD startPosition;

    [GenerateUpdateableChangeActions]
    public NodePosition_UpdateableChange(IEnumerable<Guid> nodeIds, VecD newPosition)
    {
        NodeIds = nodeIds.ToArray();
        NewPosition = newPosition;
        startPosition = newPosition;
    }

    [UpdateChangeMethod]
    public void Update(VecD newPosition)
    {
        NewPosition = newPosition;
    }

    public override bool InitializeAndValidate(Document target)
    {
        originalPositions = new Dictionary<Guid, VecD>();
        foreach (var nodeId in NodeIds)
        {
            var node = target.FindNode<Node>(nodeId);
            if (node == null)
            {
                return false;
            }
            
            originalPositions.Add(nodeId, node.Position);
        }

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        List<IChangeInfo> changes = new();
        VecD delta = NewPosition - startPosition;
        foreach (var nodeId in NodeIds)
        {
            var node = target.FindNode<Node>(nodeId);
            node.Position = originalPositions[nodeId] + delta;
            changes.Add(new NodePosition_ChangeInfo(nodeId, node.Position));
        }
        
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;

        VecD delta = NewPosition - startPosition;
        bool setDirect = false;
        if (NewPosition == startPosition)
        {
            delta = NewPosition;
            setDirect = true;
        }
            
        List<IChangeInfo> changes = new();
        
        foreach (var nodeId in NodeIds)
        {
            var node = target.FindNode<Node>(nodeId);
            node.Position = setDirect ? delta : originalPositions[nodeId] + delta;
            changes.Add(new NodePosition_ChangeInfo(nodeId, node.Position));
        }
        
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new();
        foreach (var nodeId in NodeIds)
        {
            var node = target.FindNode<Node>(nodeId);
            node.Position = originalPositions[nodeId];
            changes.Add(new NodePosition_ChangeInfo(nodeId, node.Position));
        }
        
        return changes;
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is NodePosition_UpdateableChange change && change.NodeIds.SequenceEqual(NodeIds);
    }
}
