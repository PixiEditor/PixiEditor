using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations
{
    internal interface IDrawOperation : IOperation
    {
        void DrawOnChunk(Chunk chunk, Vector2i chunkPos);
        HashSet<Vector2i> FindAffectedChunks(IReadOnlyChunkyImage image);
    }
}
