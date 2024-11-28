using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;

internal class StructureMemberIsVisible_Change : Change
{
    private bool? originalIsVisible;
    private bool newIsVisible;
    private Guid targetMember;
    [GenerateMakeChangeAction]
    public StructureMemberIsVisible_Change(bool isVisible, Guid memberGuid)
    {
        this.targetMember = memberGuid;
        this.newIsVisible = isVisible;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember(targetMember, out var member) || member.IsVisible.NonOverridenValue == newIsVisible)
            return false;
        originalIsVisible = member.IsVisible.NonOverridenValue;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        // don't record layer/folder visibility changes - it's just more convenient this way
        ignoreInUndo = true;
        target.FindMemberOrThrow(targetMember).IsVisible.NonOverridenValue = newIsVisible;
        return new StructureMemberIsVisible_ChangeInfo(targetMember, newIsVisible);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.FindMemberOrThrow(targetMember).IsVisible.NonOverridenValue = originalIsVisible!.Value;
        return new StructureMemberIsVisible_ChangeInfo(targetMember, (bool)originalIsVisible);
    }
}
