namespace ChangeableDocument.ChangeInfos
{
    public record LayerImageChunks_ChangeInfo : IChangeInfo
    {
        public Guid LayerGuid { get; init; }
        public HashSet<(int, int)>? Chunks { get; init; }
    }
}
