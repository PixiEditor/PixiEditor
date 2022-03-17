using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.ChangeInfos
{
    public record class Selection_ChangeInfo : IChangeInfo
    {
        public HashSet<Vector2i>? Chunks { get; init; }
    }
}
