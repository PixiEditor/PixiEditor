using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.ChangeInfos
{
    public record struct LayerImageChunks_ChangeInfo : IChangeInfo
    {
        public Guid LayerGuid { get; init; }
        public HashSet<Vector2i>? Chunks { get; init; }
    }
}
