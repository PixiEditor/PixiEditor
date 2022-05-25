using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

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

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(targetMember);
        if (member is null || member.Name == newName)
            return new Error();
        originalName = member.Name;
        return new Success();
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        target.FindMemberOrThrow(targetMember).Name = newName;

        ignoreInUndo = false;
        return new StructureMemberName_ChangeInfo() { GuidValue = targetMember };
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (originalName is null)
            throw new InvalidOperationException("No name to revert to");
        target.FindMemberOrThrow(targetMember).Name = originalName;
        return new StructureMemberName_ChangeInfo() { GuidValue = targetMember };
    }

    public override bool IsMergeableWith(Change other)
    {
        if (other is not StructureMemberName_Change same)
            return false;
        return same.targetMember == targetMember;
    }
}
