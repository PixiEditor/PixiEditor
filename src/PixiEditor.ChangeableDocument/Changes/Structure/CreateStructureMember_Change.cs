using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class CreateStructureMember_Change : Change
{
    private Guid newMemberGuid;

    private Guid parentGuid;
    private Type structureMemberOfType;

    private ConnectionsData? connectionsData;
    private Dictionary<Guid, VecD> originalPositions;

    [GenerateMakeChangeAction]
    public CreateStructureMember_Change(Guid parent, Guid newGuid,
        Type ofType)
    {
        this.parentGuid = parent;
        this.structureMemberOfType = ofType;
        newMemberGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (structureMemberOfType == null || structureMemberOfType.IsAbstract || structureMemberOfType.IsInterface ||
            !structureMemberOfType.IsAssignableTo(typeof(StructureNode)))
            return false;

        if (!target.TryFindNode<Node>(parentGuid, out Node parent))
        {
            return false;
        }

        var painterInput = parent.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter)) as InputProperty<Painter>;

        if (painterInput == null)
        {
            FailedMessage = "GRAPH_STATE_UNABLE_TO_CREATE_MEMBER";
            return false;
        }

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply,
        out bool ignoreInUndo)
    {
        StructureNode member = (StructureNode)NodeOperations.CreateNode(structureMemberOfType, document);
        member.Id = newMemberGuid;

        document.TryFindNode<Node>(parentGuid, out var parentNode);

        List<IChangeInfo> changes = new() { CreateChangeInfo(member) };

        InputProperty<Painter> targetInput = parentNode.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter)) as InputProperty<Painter>;

        if (targetInput == null)
        {
            ignoreInUndo = true;
            return new None();
        }

        var previouslyConnected = targetInput?.Connection;

        if (member is FolderNode folder)
        {
            document.NodeGraph.AddNode(member);
            AppendFolder(targetInput, folder, changes);
        }
        else
        {
            document.NodeGraph.AddNode(member);
            var connectPropertyChangeInfo =
                NodeOperations.AppendMember(targetInput, member.Output, member.Background, member.Id);
            changes.AddRange(connectPropertyChangeInfo);
        }
        
        changes.AddRange(NodeOperations.AdjustPositionsAfterAppend(member, targetInput.Node, previouslyConnected?.Node as Node, out originalPositions));

        ignoreInUndo = false;

        return changes;
    }

    private IChangeInfo CreateChangeInfo(StructureNode member)
    {
        return member switch
        {
            LayerNode layer => CreateLayer_ChangeInfo.FromLayer(layer),
            FolderNode folderNode => CreateFolder_ChangeInfo.FromFolder(folderNode),
            _ => throw new NotSupportedException(),
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document document)
    {
        var container = document.FindNodeOrThrow<Node>(parentGuid);

        InputProperty<Painter> backgroundInput = container.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter)) as InputProperty<Painter>;

        StructureNode child = document.FindMemberOrThrow(newMemberGuid);
        var childBackgroundConnection = child.Background.Connection;
        child.Dispose();

        document.NodeGraph.RemoveNode(child);

        List<IChangeInfo> changes = new() { new DeleteStructureMember_ChangeInfo(newMemberGuid), };

        if (childBackgroundConnection != null)
        {
            childBackgroundConnection?.ConnectTo(backgroundInput);
            ConnectProperty_ChangeInfo change = new(childBackgroundConnection.Node.Id,
                backgroundInput.Node.Id, childBackgroundConnection.InternalPropertyName,
                backgroundInput.InternalPropertyName);
            changes.Add(change);
        }
        
        changes.AddRange(NodeOperations.RevertPositions(originalPositions, document));

        return changes;
    }

    private static void AppendFolder(InputProperty<Painter> backgroundInput, FolderNode folder,
        List<IChangeInfo> changes)
    {
        var appened = NodeOperations.AppendMember(backgroundInput, folder.Output, folder.Background, folder.Id);
        changes.AddRange(appened);
    }
}
