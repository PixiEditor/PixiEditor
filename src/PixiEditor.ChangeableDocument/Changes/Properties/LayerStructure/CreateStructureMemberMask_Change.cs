using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;

internal class CreateStructureMemberMask_Change : Change
{
    private Guid targetMember;

    [GenerateMakeChangeAction]
    public CreateStructureMemberMask_Change(Guid memberGuid)
    {
        targetMember = memberGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindMember(targetMember, out var member) && member.EmbeddedMask is null;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(targetMember);
        if (member.EmbeddedMask is not null)
            throw new InvalidOperationException("Cannot create a mask; the target member already has one");
        member.EmbeddedMask = new ChunkyImage(target.Size, target.ProcessingColorSpace);

        ignoreInUndo = false;
        return new StructureMemberMask_ChangeInfo(targetMember, true);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(targetMember);
        if (member.EmbeddedMask is null)
            throw new InvalidOperationException("Cannot delete the mask; the target member has no mask");
        member.EmbeddedMask.Dispose();
        member.EmbeddedMask = null;
        return new StructureMemberMask_ChangeInfo(targetMember, false);
    }
}
