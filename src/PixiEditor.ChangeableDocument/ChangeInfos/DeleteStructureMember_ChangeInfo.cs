namespace PixiEditor.ChangeableDocument.ChangeInfos
{
    public record class DeleteStructureMember_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }
}
