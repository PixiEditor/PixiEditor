using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;
internal class StructureMemberMaskIsVisible_Change : Change
{
    private readonly Guid memberGuid;
    private bool originalMaskIsVisible;
    private readonly bool newMaskIsVisible;

    [GenerateMakeChangeAction]
    public StructureMemberMaskIsVisible_Change(bool maskIsVisible, Guid memberGuid)
    {
        this.memberGuid = memberGuid;
        this.newMaskIsVisible = maskIsVisible;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(memberGuid);
        if (member is null)
            return new Error();
        if (member.MaskIsVisible == newMaskIsVisible)
            return new Error();
        originalMaskIsVisible = member.MaskIsVisible;
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        member.MaskIsVisible = newMaskIsVisible;
        ignoreInUndo = false;
        return new StructureMemberMaskIsVisible_ChangeInfo(memberGuid, newMaskIsVisible);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        member.MaskIsVisible = originalMaskIsVisible;
        return new StructureMemberMaskIsVisible_ChangeInfo(memberGuid, originalMaskIsVisible);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is StructureMemberMaskIsVisible_Change change && change.memberGuid == memberGuid;
    }
}
