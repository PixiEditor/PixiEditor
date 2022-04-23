namespace PixiEditor.ChangeableDocument.ChangeInfos;

public record class StructureMemberMask_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
}
