using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

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

    public override void Initialize(Document target)
    {
        var member = target.FindMemberOrThrow(targetMember);
        originalIsVisible = member.IsVisible;
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        // don't record layer/folder visibility changes - it's just more convenient this way
        ignoreInUndo = true;
        if (originalIsVisible == newIsVisible)
            return null;
        target.FindMemberOrThrow(targetMember).IsVisible = newIsVisible;

        return new StructureMemberIsVisible_ChangeInfo() { GuidValue = targetMember };
    }

    public override IChangeInfo? Revert(Document target)
    {
        target.FindMemberOrThrow(targetMember).IsVisible = originalIsVisible!.Value;
        return new StructureMemberIsVisible_ChangeInfo() { GuidValue = targetMember };
    }
}
