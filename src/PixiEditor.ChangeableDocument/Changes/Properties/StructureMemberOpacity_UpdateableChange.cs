using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

internal class StructureMemberOpacity_UpdateableChange : UpdateableChange
{
    private Guid memberGuid;

    private float originalOpacity;
    private float newOpacity;

    [GenerateUpdateableChangeActions]
    public StructureMemberOpacity_UpdateableChange(Guid memberGuid, float opacity)
    {
        this.memberGuid = memberGuid;
        newOpacity = opacity;
    }

    [UpdateChangeMethod]
    public void Update(float opacity)
    {
        newOpacity = opacity;
    }

    public override void Initialize(Document document)
    {
        var member = document.FindMemberOrThrow(memberGuid);
        originalOpacity = member.Opacity;
    }

    public override IChangeInfo? ApplyTemporarily(Document target) => Apply(target, out _);

    public override IChangeInfo? Apply(Document document, out bool ignoreInUndo)
    {
        if (originalOpacity == newOpacity)
        {
            ignoreInUndo = true;
            return null;
        }

        var member = document.FindMemberOrThrow(memberGuid);
        member.Opacity = newOpacity;

        ignoreInUndo = false;
        return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
    }

    public override IChangeInfo? Revert(Document document)
    {
        if (originalOpacity == newOpacity)
            return null;

        var member = document.FindMemberOrThrow(memberGuid);
        member.Opacity = originalOpacity;

        return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
    }

    public override bool IsMergeableWith(Change other)
    {
        if (other is not StructureMemberOpacity_UpdateableChange same)
            return false;
        return same.memberGuid == memberGuid;
    }
}
