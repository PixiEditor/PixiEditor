namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class MoveStructureMember_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
}
