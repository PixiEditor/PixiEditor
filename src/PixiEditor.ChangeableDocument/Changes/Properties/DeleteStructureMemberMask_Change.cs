using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

internal class DeleteStructureMemberMask_Change : Change
{
    private readonly Guid memberGuid;
    private ChunkyImage? storedMask;

    [GenerateMakeChangeAction]
    public DeleteStructureMemberMask_Change(Guid memberGuid)
    {
        this.memberGuid = memberGuid;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(memberGuid);
        if (member is null || member.Mask is null)
            return new Error();
        storedMask = member.Mask.CloneFromCommitted();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        if (member.Mask is null)
            throw new InvalidOperationException("Cannot delete the mask; Target member has no mask");
        member.Mask.Dispose();
        member.Mask = null;

        ignoreInUndo = false;
        return new StructureMemberMask_ChangeInfo(memberGuid, false);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        if (member.Mask is not null)
            throw new InvalidOperationException("Cannot revert mask deletion; The target member already has a mask");
        member.Mask = storedMask!.CloneFromCommitted();

        return new StructureMemberMask_ChangeInfo(memberGuid, true);
    }

    public override void Dispose()
    {
        storedMask?.Dispose();
    }
}
