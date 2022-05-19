namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class MoveStructureMember_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
    public Guid ParentFromGuid { get; init; }
    public Guid ParentToGuid { get; init; }
}
