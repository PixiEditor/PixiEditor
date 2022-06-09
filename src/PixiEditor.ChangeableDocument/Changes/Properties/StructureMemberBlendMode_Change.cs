using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Properties;
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

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(targetGuid);
        if (member is null || member.BlendMode == newBlendMode)
            return new Error();
        originalBlendMode = member.BlendMode;
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(targetGuid);
        member.BlendMode = newBlendMode;
        ignoreInUndo = false;
        return new StructureMemberBlendMode_ChangeInfo(targetGuid, newBlendMode);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(targetGuid);
        member.BlendMode = originalBlendMode;
        return new StructureMemberBlendMode_ChangeInfo(targetGuid, originalBlendMode);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is StructureMemberBlendMode_Change change && change.targetGuid == targetGuid;
    }
}
