using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

internal class CreateStructureMemberMask_Change : Change
{
    private Guid targetMember;

    [GenerateMakeChangeAction]
    public CreateStructureMemberMask_Change(Guid memberGuid)
    {
        targetMember = memberGuid;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(targetMember);
        if (member is null || member.Mask is not null)
            return new Error();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(targetMember);
        if (member.Mask is not null)
            throw new InvalidOperationException("Cannot create a mask; the target member already has one");
        member.Mask = new ChunkyImage(target.Size);

        ignoreInUndo = false;
        return new StructureMemberMask_ChangeInfo() { GuidValue = targetMember };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(targetMember);
        if (member.Mask is null)
            throw new InvalidOperationException("Cannot delete the mask; the target member has no mask");
        member.Mask.Dispose();
        member.Mask = null;
        return new StructureMemberMask_ChangeInfo() { GuidValue = targetMember };
    }
}
