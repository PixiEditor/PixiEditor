namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class DeleteStructureMember_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
}
