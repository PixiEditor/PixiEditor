using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
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

    public override void Initialize(Document target)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        originalValue = member.ClipToMemberBelow;
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
