using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations
{
    internal interface IOperation : IDisposable
    {
        void DrawOnChunk(Chunk chunk, Vector2i chunkPos);
        HashSet<Vector2i> FindAffectedChunks();
    }
}
