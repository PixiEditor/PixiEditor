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
    public CreateStructureMember_Change(Guid parentFolder, Guid newGuid, int parentFolderIndex, StructureMemberType type)
    {
        this.parentFolderGuid = parentFolder;
        this.parentFolderIndex = parentFolderIndex;
        this.type = type;
        newMemberGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.HasMember(parentFolderGuid);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply, out bool ignoreInUndo)
    {
        var folder = document.FindMemberOrThrow<FolderNode>(parentFolderGuid);

        StructureNode member = type switch
        {
            // TODO: Add support for other types
            StructureMemberType.Layer => new ImageLayerNode(document.Size) { Id = newMemberGuid },
            StructureMemberType.Folder => new FolderNode() { Id = newMemberGuid },
            _ => throw new NotSupportedException(),
        };

        /*folder.Children = folder.Children.Insert(parentFolderIndex, member);

        ignoreInUndo = false;
        return type switch
        {
            StructureMemberType.Layer => CreateLayer_ChangeInfo.FromLayer(parentFolderGuid, parentFolderIndex, (Layer)member),
            StructureMemberType.Folder => CreateFolder_ChangeInfo.FromFolder(parentFolderGuid, parentFolderIndex, (Folder)member),
            _ => throw new NotSupportedException(),
        };*/
        
        ignoreInUndo = false;
        return new None();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document document)
    {
        FolderNode folder = document.FindMemberOrThrow<FolderNode>(parentFolderGuid);
        StructureNode child = document.FindMemberOrThrow(newMemberGuid);
        child.Dispose();
        //folder.Children = folder.Children.RemoveAt(folder.Children.FindIndex(member => member.GuidValue == newMemberGuid));

        return new DeleteStructureMember_ChangeInfo(newMemberGuid, parentFolderGuid);
    }
}
