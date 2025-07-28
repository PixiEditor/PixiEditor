using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class DeleteStructureMember_Change : Change
{
    private Guid memberGuid;
    private int originalIndex;
    private ConnectionsData originalConnections;
    private StructureNode? savedCopy;

    private GroupKeyFrame savedKeyFrameGroup;

    [GenerateMakeChangeAction]
    public DeleteStructureMember_Change(Guid memberGuid)
    {
        this.memberGuid = memberGuid;
    }

    public override bool InitializeAndValidate(Document document)
    {
        var member = document.FindMember(memberGuid);
        if (member is null)
            return false;

        originalConnections = NodeOperations.CreateConnectionsData(member);

        savedCopy = (StructureNode)member.Clone(true);

        savedKeyFrameGroup = DeleteNode_Change.CloneGroupKeyFrame(document, memberGuid);

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply,
        out bool ignoreInUndo)
    {
        StructureNode node = document.FindMember(memberGuid);

        var bgConnection = node.Background.Connection;
        var outputConnections = node.Output.Connections.ToArray();

        document.NodeGraph.RemoveNode(node);

        List<IChangeInfo> changes = new();

        if (outputConnections != null && bgConnection != null)
        {
            foreach (var connection in outputConnections)
            {
                bgConnection.ConnectTo(connection);
                changes.Add(new ConnectProperty_ChangeInfo(bgConnection.Node.Id, connection.Node.Id,
                    bgConnection.InternalPropertyName, connection.InternalPropertyName));

                node.Output.DisconnectFrom(connection);
            }
        }

        if (savedKeyFrameGroup != null)
        {
            document.AnimationData.RemoveKeyFrame(savedKeyFrameGroup.Id);
            changes.Add(new DeleteKeyFrame_ChangeInfo(savedKeyFrameGroup.Id));
        }

        node.Dispose();
        ignoreInUndo = false;

        changes.Add(new DeleteStructureMember_ChangeInfo(memberGuid));
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document doc)
    {
        var copy = (StructureNode)savedCopy!.Clone(true);

        doc.NodeGraph.AddNode(copy);

        List<IChangeInfo> changes = new();

        IChangeInfo createChange = copy switch
        {
            LayerNode node => CreateLayer_ChangeInfo.FromLayer(node),
            FolderNode node => CreateFolder_ChangeInfo.FromFolder(node),
            _ => throw new NotSupportedException(),
        };

        changes.Add(createChange);

        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(originalConnections, copy, doc.NodeGraph));
        
        DeleteNode_Change.RevertKeyFrames(doc, savedKeyFrameGroup, changes);

        return changes;
    }

    public override void Dispose()
    {
        savedCopy?.Dispose();
    }
}
