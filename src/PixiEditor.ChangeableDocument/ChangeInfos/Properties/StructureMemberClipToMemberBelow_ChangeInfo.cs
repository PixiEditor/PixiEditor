namespace PixiEditor.ChangeableDocument.ChangeInfos.Properties;
public record class StructureMemberClipToMemberBelow_ChangeInfo : IChangeInfo
{
    public Guid MemberGuid { get; init; }
}
