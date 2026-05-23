using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CommentZoneRect_UpdateableChange : InterruptableUpdateableChange
{
    public Guid NodeId { get; }
    public VecI NewSize { get; private set; }
    public VecI NewOffset { get; private set; }

    private VecI originalSize;
    private VecI originalOffset;

    [GenerateUpdateableChangeActions]
    public CommentZoneRect_UpdateableChange(Guid nodeId, VecI newSize, VecI newOffset)
    {
        NodeId = nodeId;
        NewSize = newSize;
        NewOffset = newOffset;
    }

    [UpdateChangeMethod]
    public void Update(VecI newSize, VecI newOffset)
    {
        NewSize = newSize;
        NewOffset = newOffset;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindNode<Node>(NodeId, out var node))
            return false;

        var sizeProp = node.GetInputProperty(CommentNode.SizePropertyName);
        var offsetProp = node.GetInputProperty(CommentNode.OffsetPropertyName);
        if (sizeProp == null || offsetProp == null)
            return false;

        originalSize = (VecI)sizeProp.NonOverridenValue;
        originalOffset = (VecI)offsetProp.NonOverridenValue;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return ApplyValues(target, NewSize, NewOffset);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = NewSize == originalSize && NewOffset == originalOffset;
        return ApplyValues(target, NewSize, NewOffset);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return ApplyValues(target, originalSize, originalOffset);
    }

    private List<IChangeInfo> ApplyValues(Document target, VecI size, VecI offset)
    {
        var node = target.NodeGraph.Nodes.First(x => x.Id == NodeId);
        var sizeProp = node.GetInputProperty(CommentNode.SizePropertyName);
        var offsetProp = node.GetInputProperty(CommentNode.OffsetPropertyName);

        target.NodeGraph.StartListenToPropertyChanges();

        object sizeValue = size;
        if (!sizeProp.Validator.Validate(sizeValue, out _))
            sizeValue = sizeProp.Validator.GetClosestValidValue(sizeValue);
        sizeValue = GraphUtils.SetNonOverwrittenValue(sizeProp, sizeValue);

        object offsetValue = offset;
        if (!offsetProp.Validator.Validate(offsetValue, out _))
            offsetValue = offsetProp.Validator.GetClosestValidValue(offsetValue);
        offsetValue = GraphUtils.SetNonOverwrittenValue(offsetProp, offsetValue);

        List<IChangeInfo> changes = new()
        {
            new PropertyValueUpdated_ChangeInfo(NodeId, CommentNode.SizePropertyName, sizeValue),
            new PropertyValueUpdated_ChangeInfo(NodeId, CommentNode.OffsetPropertyName, offsetValue),
        };

        List<Guid> nodesWithChangedIO = target.NodeGraph.StopListenToPropertyChanges();
        foreach (var nodeId in nodesWithChangedIO)
        {
            var changedNode = target.FindNode(nodeId);
            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(changedNode));
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(changedNode));
        }

        return changes;
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is CommentZoneRect_UpdateableChange change && change.NodeId == NodeId;
    }
}
