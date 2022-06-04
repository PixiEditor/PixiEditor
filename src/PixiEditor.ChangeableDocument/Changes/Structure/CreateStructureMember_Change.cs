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

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.FindMember(parentFolderGuid) is null)
            return new Error();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, out bool ignoreInUndo)
    {
        var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);

        StructureMember member = type switch
        {
            StructureMemberType.Layer => new Layer(document.Size) { GuidValue = newMemberGuid },
            StructureMemberType.Folder => new Folder() { GuidValue = newMemberGuid },
            _ => throw new NotSupportedException(),
        };

        folder.Children = folder.Children.Insert(parentFolderIndex, member);

        ignoreInUndo = false;
        return type switch
        {
            StructureMemberType.Layer => CreateLayer_ChangeInfo.FromLayer(parentFolderGuid, parentFolderIndex, (Layer)member),
            StructureMemberType.Folder => CreateFolder_ChangeInfo.FromFolder(parentFolderGuid, parentFolderIndex, (Folder)member),
            _ => throw new NotSupportedException(),
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document document)
    {
        var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);
        var child = document.FindMemberOrThrow(newMemberGuid);
        child.Dispose();
        folder.Children = folder.Children.RemoveAt(folder.Children.FindIndex(child => child.GuidValue == newMemberGuid));

        return new DeleteStructureMember_ChangeInfo(newMemberGuid, parentFolderGuid);
    }
}
