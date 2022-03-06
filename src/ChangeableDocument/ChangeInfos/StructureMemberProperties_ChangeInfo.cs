namespace ChangeableDocument.ChangeInfos
{
    public record struct StructureMemberProperties_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
        public bool IsVisibleChanged { get; init; } = false;
        public bool NameChanged { get; init; } = false;
    }
}
