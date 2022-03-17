namespace ChangeableDocument.ChangeInfos
{
    public record class CreateStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record class DeleteStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record class MoveStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }
}
