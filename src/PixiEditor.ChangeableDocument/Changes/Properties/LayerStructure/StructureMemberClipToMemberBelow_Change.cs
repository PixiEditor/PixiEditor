using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;
internal class StructureMemberClipToMemberBelow_Change : Change
{
    private bool originalValue;
    private readonly bool newValue;
    private readonly Guid memberGuid;

    [GenerateMakeChangeAction]
    public StructureMemberClipToMemberBelow_Change(bool enabled, Guid memberGuid)
    {
        this.newValue = enabled;
        this.memberGuid = memberGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember(memberGuid, out var member) || member.ClipToPreviousMember == newValue)
            return false;
        originalValue = member.ClipToPreviousMember;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        member.ClipToPreviousMember = newValue;
        ignoreInUndo = false;
        return new StructureMemberClipToMemberBelow_ChangeInfo(memberGuid, newValue);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        member.ClipToPreviousMember = originalValue;
        return new StructureMemberClipToMemberBelow_ChangeInfo(memberGuid, originalValue);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is StructureMemberClipToMemberBelow_Change change && change.memberGuid == memberGuid;
    }
}
