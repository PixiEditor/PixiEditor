namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class CreateStructureMember_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
}
