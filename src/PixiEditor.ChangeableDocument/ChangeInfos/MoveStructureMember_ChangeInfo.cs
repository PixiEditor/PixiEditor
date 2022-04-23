namespace PixiEditor.ChangeableDocument.ChangeInfos;

public record class MoveStructureMember_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
}
