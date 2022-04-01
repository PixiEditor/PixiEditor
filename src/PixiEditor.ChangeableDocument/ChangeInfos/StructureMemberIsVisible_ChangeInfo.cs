namespace PixiEditor.ChangeableDocument.ChangeInfos
{
    public record class StructureMemberIsVisible_ChangeInfo : IChangeInfo
    {
        public Guid GuidValue { get; init; }
    }
}
