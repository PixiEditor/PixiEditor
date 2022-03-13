using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations
{
    internal interface IChunkOperation : IOperation
    {
        void DrawOnChunk(Chunk chunk, Vector2i chunkPos);
        HashSet<Vector2i> FindAffectedChunks(IReadOnlyChunkyImage image);
    }
}
