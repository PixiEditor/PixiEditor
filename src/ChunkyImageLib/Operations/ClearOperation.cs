using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations
{
    internal record class ClearOperation : IDrawOperation
    {
        public void DrawOnChunk(Chunk chunk, Vector2i chunkPos)
        {
            chunk.Surface.SkiaSurface.Canvas.Clear();
        }

        public HashSet<Vector2i> FindAffectedChunks(IReadOnlyChunkyImage image)
        {
            return image.FindAllChunks();
        }

        public void Dispose() { }
    }
}
