using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class CreateStructureMember_Change : Change
{
    private Guid newMemberGuid;

    private Guid parentFolderGuid;
    private StructureMemberType type;

    [GenerateMakeChangeAction]
    public CreateStructureMember_Change(Guid parentFolder, Guid newGuid,
        StructureMemberType type)
    {
        this.parentFolderGuid = parentFolder;
        this.type = type;
        newMemberGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindNode<Node>(parentFolderGuid, out var targetNode) && targetNode is IBackgroundInput;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply,
        out bool ignoreInUndo)
    {
        StructureNode member = type switch
        {
            // TODO: Add support for other types
            StructureMemberType.Layer => new ImageLayerNode(document.Size) { Id = newMemberGuid },
            StructureMemberType.Folder => new FolderNode() { Id = newMemberGuid },
            _ => throw new NotSupportedException(),
        };

        document.TryFindNode<Node>(parentFolderGuid, out var parentNode);
        document.NodeGraph.AddNode(member);

        IBackgroundInput backgroundInput = (IBackgroundInput)parentNode;
        List<ConnectProperty_ChangeInfo> connectPropertyChangeInfo = AppendMember(backgroundInput, member);
        
        List<IChangeInfo> changes = new()
        {
            CreateChangeInfo(member),
        };
        
        changes.AddRange(connectPropertyChangeInfo);

        ignoreInUndo = false;
        
        return changes;
    }
    
    private IChangeInfo CreateChangeInfo(StructureNode member)
    {
        return type switch
        {
             StructureMemberType.Layer => CreateLayer_ChangeInfo.FromLayer(parentFolderGuid,
                (LayerNode)member),
            StructureMemberType.Folder => CreateFolder_ChangeInfo.FromFolder(parentFolderGuid,
                (FolderNode)member),
            _ => throw new NotSupportedException(),
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document document)
    {
        var container = document.FindNodeOrThrow<Node>(parentFolderGuid);
        if (container is not IBackgroundInput backgroundInput)
        {
            throw new InvalidOperationException("Parent folder is not a valid container.");
        }

        StructureNode child = document.FindMemberOrThrow(newMemberGuid);
        var childBackgroundConnection = child.Background.Connection;
        child.Dispose();

        document.NodeGraph.RemoveNode(child);

        List<IChangeInfo> changes = new()
        {
            new DeleteStructureMember_ChangeInfo(newMemberGuid, parentFolderGuid),
        };

        if (childBackgroundConnection != null)
        {
            childBackgroundConnection?.ConnectTo(backgroundInput.Background);
            ConnectProperty_ChangeInfo change = new(childBackgroundConnection.Node.Id, backgroundInput.Background.Node.Id, childBackgroundConnection.InternalPropertyName, backgroundInput.Background.InternalPropertyName);
            changes.Add(change);
        }

        return changes;
    }

    private static List<ConnectProperty_ChangeInfo> AppendMember(IBackgroundInput backgroundInput, StructureNode member)
    {
        List<ConnectProperty_ChangeInfo> changes = new();
        IOutputProperty? previouslyConnected = null;
        if (backgroundInput.Background.Connection != null)
        {
            previouslyConnected = backgroundInput.Background.Connection;
        }

        member.Output.ConnectTo(backgroundInput.Background);

        if (previouslyConnected != null)
        {
            member.Background.Connection = previouslyConnected;
            changes.Add(new ConnectProperty_ChangeInfo(previouslyConnected.Node.Id, member.Id, previouslyConnected.InternalPropertyName, member.Background.InternalPropertyName));
        }
        
        changes.Add(new ConnectProperty_ChangeInfo(member.Id, backgroundInput.Background.Node.Id, member.Output.InternalPropertyName, backgroundInput.Background.InternalPropertyName));
        
        return changes;
    }
}
