using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;
internal class StructureMemberMaskIsVisible_Change : Change
{
    private readonly Guid memberGuid;
    private bool originalMaskIsVisible;
    private readonly bool newMaskIsVisible;

    [GenerateMakeChangeAction]
    public StructureMemberMaskIsVisible_Change(bool maskIsVisible, Guid memberGuid)
    {
        this.memberGuid = memberGuid;
        this.newMaskIsVisible = maskIsVisible;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember(memberGuid, out var member) || member.MaskIsVisible.Value == newMaskIsVisible)
            return false;
        
        originalMaskIsVisible = member.MaskIsVisible.Value;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        member.MaskIsVisible.Value = newMaskIsVisible;
        ignoreInUndo = false;
        return new StructureMemberMaskIsVisible_ChangeInfo(memberGuid, newMaskIsVisible);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        member.MaskIsVisible.Value = originalMaskIsVisible;
        return new StructureMemberMaskIsVisible_ChangeInfo(memberGuid, originalMaskIsVisible);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is StructureMemberMaskIsVisible_Change change && change.memberGuid == memberGuid;
    }
}
