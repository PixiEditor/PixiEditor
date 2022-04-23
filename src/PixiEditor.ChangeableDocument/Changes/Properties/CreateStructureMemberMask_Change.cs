using ChunkyImageLib;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

internal class CreateStructureMemberMask_Change : Change
{
    private Guid targetMember;
    public CreateStructureMemberMask_Change(Guid memberGuid)
    {
        targetMember = memberGuid;
    }

    public override void Initialize(Document target) { }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(targetMember);
        if (member.Mask is not null)
            throw new InvalidOperationException("Cannot create a mask; the target member already has one");
        member.Mask = new ChunkyImage(target.Size);

        ignoreInUndo = false;
        return new StructureMemberMask_ChangeInfo() { GuidValue = targetMember };
    }

    public override IChangeInfo? Revert(Document target)
    {
        var member = target.FindMemberOrThrow(targetMember);
        if (member.Mask is null)
            throw new InvalidOperationException("Cannot delete the mask; the target member has no mask");
        member.Mask.Dispose();
        member.Mask = null;
        return new StructureMemberMask_ChangeInfo() { GuidValue = targetMember };
    }
}
