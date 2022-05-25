using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;
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

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(memberGuid);
        if (member is null || member.ClipToMemberBelow == newValue)
            return new Error();
        originalValue = member.ClipToMemberBelow;
        return new Success();
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        if (originalValue == newValue)
        {
            ignoreInUndo = true;
            return null;
        }
        var member = target.FindMemberOrThrow(memberGuid);
        member.ClipToMemberBelow = newValue;
        ignoreInUndo = false;
        return new StructureMemberClipToMemberBelow_ChangeInfo() { GuidValue = memberGuid };
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (originalValue == newValue)
            return null;
        var member = target.FindMemberOrThrow(memberGuid);
        member.ClipToMemberBelow = originalValue;
        return new StructureMemberClipToMemberBelow_ChangeInfo() { GuidValue = memberGuid };
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is StructureMemberClipToMemberBelow_Change;
    }
}
