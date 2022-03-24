namespace ChangeableDocument.ChangeInfos
{
    public record class CreateStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }
}
