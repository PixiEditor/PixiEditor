using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;

internal class DeleteStructureMemberMask_Change : Change
{
    private readonly Guid memberGuid;
    private ChunkyImage? storedMask;

    [GenerateMakeChangeAction]
    public DeleteStructureMemberMask_Change(Guid memberGuid)
    {
        this.memberGuid = memberGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember(memberGuid, out var member) || member.EmbeddedMask is null)
            return false;
        
        storedMask = member.EmbeddedMask.CloneFromCommitted();
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        if (member.EmbeddedMask is null)
            throw new InvalidOperationException("Cannot delete the mask; Target member has no mask");
        member.EmbeddedMask.Dispose();
        member.EmbeddedMask = null;

        ignoreInUndo = false;
        return new StructureMemberMask_ChangeInfo(memberGuid, false);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        if (member.EmbeddedMask is not null)
            throw new InvalidOperationException("Cannot revert mask deletion; The target member already has a mask");
        member.EmbeddedMask = storedMask!.CloneFromCommitted();

        return new StructureMemberMask_ChangeInfo(memberGuid, true);
    }

    public override void Dispose()
    {
        storedMask?.Dispose();
    }
}
