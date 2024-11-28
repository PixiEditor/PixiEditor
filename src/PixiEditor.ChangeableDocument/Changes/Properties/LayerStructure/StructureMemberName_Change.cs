using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;

internal class StructureMemberName_Change : Change
{
    private string? originalName;
    private string newName;
    private Guid targetMember;

    [GenerateMakeChangeAction]
    public StructureMemberName_Change(Guid memberGuid, string name)
    {
        this.targetMember = memberGuid;
        this.newName = name;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember(targetMember, out var member) || member.MemberName == newName)
            return false;
        originalName = member.MemberName;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.FindMemberOrThrow(targetMember).MemberName = newName;

        ignoreInUndo = false;
        return new StructureMemberName_ChangeInfo(targetMember, newName);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (originalName is null)
            throw new InvalidOperationException("No name to revert to");
        target.FindMemberOrThrow(targetMember).MemberName = originalName;
        return new StructureMemberName_ChangeInfo(targetMember, originalName);
    }

    public override bool IsMergeableWith(Change other)
    {
        if (other is not StructureMemberName_Change same)
            return false;
        return same.targetMember == targetMember;
    }
}
