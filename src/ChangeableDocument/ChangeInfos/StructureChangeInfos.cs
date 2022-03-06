namespace ChangeableDocument.ChangeInfos
{
    public record struct CreateStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record struct DeleteStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record struct MoveStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }
}
