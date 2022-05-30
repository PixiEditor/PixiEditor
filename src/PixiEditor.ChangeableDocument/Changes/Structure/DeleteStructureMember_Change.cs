using PixiEditor.ChangeableDocument.ChangeInfos.Structure;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class DeleteStructureMember_Change : Change
{
    private Guid memberGuid;
    private Guid parentGuid;
    private int originalIndex;
    private StructureMember? savedCopy;

    [GenerateMakeChangeAction]
    public DeleteStructureMember_Change(Guid memberGuid)
    {
        this.memberGuid = memberGuid;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document document)
    {
        var (member, parent) = document.FindChildAndParent(memberGuid);
        if (member is null || parent is null)
            return new Error();

        originalIndex = parent.Children.IndexOf(member);
        parentGuid = parent.GuidValue;
        savedCopy = member.Clone();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, out bool ignoreInUndo)
    {
        var (member, parent) = document.FindChildAndParentOrThrow(memberGuid);
        parent.Children = parent.Children.Remove(member);
        member.Dispose();
        ignoreInUndo = false;
        return new DeleteStructureMember_ChangeInfo() { GuidValue = memberGuid, ParentGuid = parentGuid };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document doc)
    {
        var parent = (Folder)doc.FindMemberOrThrow(parentGuid);

        parent.Children = parent.Children.Insert(originalIndex, savedCopy!.Clone());
        return new CreateStructureMember_ChangeInfo() { GuidValue = memberGuid };
    }

    public override void Dispose()
    {
        savedCopy?.Dispose();
    }
}
