namespace ChangeableDocument.ChangeInfos
{
    public record class StructureMemberName_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }
}
