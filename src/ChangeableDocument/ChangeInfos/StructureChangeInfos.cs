namespace ChangeableDocument.ChangeInfos
{
    public record CreateStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record DeleteStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record MoveStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }
}
