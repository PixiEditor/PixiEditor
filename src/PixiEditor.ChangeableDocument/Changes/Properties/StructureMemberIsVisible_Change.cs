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

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(targetMember);
        if (member is null || member.IsVisible == newIsVisible)
            return new Error();
        originalIsVisible = member.IsVisible;
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        // don't record layer/folder visibility changes - it's just more convenient this way
        ignoreInUndo = true;
        target.FindMemberOrThrow(targetMember).IsVisible = newIsVisible;
        return new StructureMemberIsVisible_ChangeInfo(targetMember, newIsVisible);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.FindMemberOrThrow(targetMember).IsVisible = originalIsVisible!.Value;
        return new StructureMemberIsVisible_ChangeInfo(targetMember, (bool)originalIsVisible);
    }
}
