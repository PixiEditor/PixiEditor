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
        if (!target.TryFindMember(targetMember, out var member) || member.IsVisible.Value == newIsVisible)
            return false;
        originalIsVisible = member.IsVisible.Value;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        // don't record layer/folder visibility changes - it's just more convenient this way
        ignoreInUndo = true;
        target.FindMemberOrThrow(targetMember).IsVisible.Value = newIsVisible;
        return new StructureMemberIsVisible_ChangeInfo(targetMember, newIsVisible);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.FindMemberOrThrow(targetMember).IsVisible.Value = originalIsVisible!.Value;
        return new StructureMemberIsVisible_ChangeInfo(targetMember, (bool)originalIsVisible);
    }
}
