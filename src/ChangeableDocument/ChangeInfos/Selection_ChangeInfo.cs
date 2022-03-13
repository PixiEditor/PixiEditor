using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.ChangeInfos
{
    public record struct Selection_ChangeInfo : IChangeInfo
    {
        public HashSet<Vector2i>? Chunks { get; init; }
    }
}
