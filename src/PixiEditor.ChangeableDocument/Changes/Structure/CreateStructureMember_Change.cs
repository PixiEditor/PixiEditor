using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class CreateStructureMember_Change : Change
{
    private Guid newMemberGuid;

    private Guid parentFolderGuid;
    private int parentFolderIndex;
    private StructureMemberType type;

    [GenerateMakeChangeAction]
    public CreateStructureMember_Change(Guid parentFolder, Guid newGuid, int parentFolderIndex,
        StructureMemberType type)
    {
        this.parentFolderGuid = parentFolder;
        this.parentFolderIndex = parentFolderIndex;
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
        AppendMember(backgroundInput, member);

        ignoreInUndo = false;
        return type switch
        {
            StructureMemberType.Layer => CreateLayer_ChangeInfo.FromLayer(parentFolderGuid, parentFolderIndex,
                (LayerNode)member),
            StructureMemberType.Folder => CreateFolder_ChangeInfo.FromFolder(parentFolderGuid, parentFolderIndex,
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
        
        childBackgroundConnection?.ConnectTo(backgroundInput.Background);

        return new DeleteStructureMember_ChangeInfo(newMemberGuid, parentFolderGuid);
    }

    private static void AppendMember(IBackgroundInput backgroundInput, StructureNode member)
    {
        IOutputProperty? previouslyConnected = null;
        if (backgroundInput.Background.Connection != null)
        {
            previouslyConnected = backgroundInput.Background.Connection;
        }

        member.Output.ConnectTo(backgroundInput.Background);

        if (previouslyConnected != null)
        {
            member.Background.Connection = previouslyConnected;
        }
    }
}
