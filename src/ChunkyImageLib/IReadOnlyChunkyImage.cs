using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib
{
    public interface IReadOnlyChunkyImage
    {
        IReadOnlyChunk? GetLatestChunk(Vector2i pos);
        HashSet<Vector2i> FindAffectedChunks();
        HashSet<Vector2i> FindAllChunks();
    }
}
