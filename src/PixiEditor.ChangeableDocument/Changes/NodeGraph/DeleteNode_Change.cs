using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class DeleteNode_Change : Change
{
    public Guid NodeId { get; set; }

    private ConnectionsData originalConnections;

    private Node savedCopy;

    private GroupKeyFrame? savedKeyFrameGroup;

    [GenerateMakeChangeAction]
    public DeleteNode_Change(Guid nodeId)
    {
        NodeId = nodeId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        Node node = target.FindNode<Node>(NodeId);

        if (node is null || target.NodeGraph.OutputNode == node)
            return false;

        originalConnections = NodeOperations.CreateConnectionsData(node);

        savedCopy = node.Clone();
        if (savedCopy is IPairNode pairNode)
        {
            pairNode.OtherNode = (node as IPairNode).OtherNode;
        }

        savedKeyFrameGroup = CloneGroupKeyFrame(target, NodeId);

        return true;
    }

    public static GroupKeyFrame? CloneGroupKeyFrame(Document target, Guid id)
    {
        GroupKeyFrame group = target.AnimationData.KeyFrames.FirstOrDefault(x => x.NodeId == id) as GroupKeyFrame;
        if (group is null)
            return null;
        return group.Clone() as GroupKeyFrame;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        var node = target.FindNode<Node>(NodeId);

        target.NodeGraph.StartListenToPropertyChanges();
        List<IChangeInfo> changes = NodeOperations.DetachNode(node);

        target.NodeGraph.RemoveNode(node);

        changes.Add(new DeleteNode_ChangeInfo(NodeId));

        if (savedKeyFrameGroup != null)
        {
            target.AnimationData.RemoveKeyFrame(savedKeyFrameGroup.Id);
            changes.Add(new DeleteKeyFrame_ChangeInfo(savedKeyFrameGroup.Id));
        }

        List<Guid> nodesWithChangedIO = target.NodeGraph.StopListenToPropertyChanges();

        foreach (var nodeId in nodesWithChangedIO)
        {
            Node n = target.FindNode(nodeId);
            if(n == node) continue;

            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(n));
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(n));
        }

        node.Dispose();

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document doc)
    {
        var copy = savedCopy!.Clone();
        copy.Id = NodeId;

        if (copy is IPairNode pairNode)
        {
            pairNode.OtherNode = (savedCopy as IPairNode).OtherNode;
        }

        doc.NodeGraph.AddNode(copy);

        List<IChangeInfo> changes = new();

        IChangeInfo createChange = CreateNode_ChangeInfo.CreateFromNode(copy);

        changes.Add(createChange);

        doc.NodeGraph.StartListenToPropertyChanges();

        changes.AddRange(NodeOperations.CreateUpdateInputs(copy));
        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(originalConnections, copy, doc.NodeGraph));
        changes.Add(new NodePosition_ChangeInfo(copy.Id, copy.Position));

        RevertKeyFrames(doc, savedKeyFrameGroup, changes);

        List<Guid> nodesWithChangedIO = doc.NodeGraph.StopListenToPropertyChanges();
        foreach (var nodeId in nodesWithChangedIO)
        {
            Node n = doc.FindNode(nodeId);
            if(n == copy) continue;

            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(n));
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(n));
        }

        return changes;
    }

    public static void RevertKeyFrames(Document doc, GroupKeyFrame savedKeyFrameGroup, List<IChangeInfo> changes)
    {
        if (savedKeyFrameGroup != null)
        {
            var cloned = savedKeyFrameGroup.Clone();
            doc.AnimationData.AddKeyFrame(cloned);
            foreach (var keyFrame in savedKeyFrameGroup.Children)
            {
                changes.Add(new CreateRasterKeyFrame_ChangeInfo(keyFrame.NodeId, keyFrame.StartFrame, keyFrame.Id,
                    false));
                changes.Add(new KeyFrameLength_ChangeInfo(keyFrame.Id, keyFrame.StartFrame, keyFrame.Duration));
            }

            changes.Add(new KeyFrameVisibility_ChangeInfo(savedKeyFrameGroup.Id, savedKeyFrameGroup.IsVisible));
        }
    }

    public override void Dispose()
    {
        savedCopy?.Dispose();
    }
}
