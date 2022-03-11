using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib
{
    public interface IReadOnlyChunkyImage
    {
        Chunk? GetChunk(Vector2i pos);
        HashSet<Vector2i> FindAffectedChunks();
    }
}
