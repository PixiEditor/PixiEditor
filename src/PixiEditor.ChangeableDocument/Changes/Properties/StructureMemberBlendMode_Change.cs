using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Properties;
internal class StructureMemberBlendMode_Change : Change
{
    private BlendMode originalBlendMode;
    private readonly BlendMode newBlendMode;
    private readonly Guid targetGuid;

    public StructureMemberBlendMode_Change(BlendMode newBlendMode, Guid targetGuid)
    {
        this.newBlendMode = newBlendMode;
        this.targetGuid = targetGuid;
    }

    public override void Initialize(Document target)
    {
        var member = target.FindMemberOrThrow(targetGuid);
        originalBlendMode = member.BlendMode;
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(targetGuid);
        member.BlendMode = newBlendMode;
        ignoreInUndo = false;
        return new StructureMemberBlendMode_ChangeInfo() { GuidValue = targetGuid };
    }

    public override IChangeInfo? Revert(Document target)
    {
        var member = target.FindMemberOrThrow(targetGuid);
        member.BlendMode = originalBlendMode;
        return new StructureMemberBlendMode_ChangeInfo() { GuidValue = targetGuid };
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is StructureMemberBlendMode_Change;
    }
}
