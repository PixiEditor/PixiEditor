namespace ChangeableDocument.ChangeInfos
{
    public record Document_CreateStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record Document_DeleteStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record Document_MoveStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }

    public record Document_UpdateStructureMemberProperties_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
        public bool IsVisibleChanged { get; init; } = false;
        public bool NameChanged { get; init; } = false;
    }
}
