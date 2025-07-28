using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;
internal class StructureMemberBlendMode_Change : Change
{
    private BlendMode originalBlendMode;
    private readonly BlendMode newBlendMode;
    private readonly Guid targetGuid;

    [GenerateMakeChangeAction]
    public StructureMemberBlendMode_Change(BlendMode blendMode, Guid memberGuid)
    {
        this.newBlendMode = blendMode;
        this.targetGuid = memberGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember(targetGuid, out var member) || member.BlendMode.NonOverridenValue == newBlendMode)
            return false;
        originalBlendMode = member.BlendMode.NonOverridenValue;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(targetGuid);
        member.BlendMode.NonOverridenValue = newBlendMode;
        ignoreInUndo = false;
        return new StructureMemberBlendMode_ChangeInfo(targetGuid, newBlendMode);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(targetGuid);
        member.BlendMode.NonOverridenValue = originalBlendMode;
        return new StructureMemberBlendMode_ChangeInfo(targetGuid, originalBlendMode);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is StructureMemberBlendMode_Change change && change.targetGuid == targetGuid;
    }
}
