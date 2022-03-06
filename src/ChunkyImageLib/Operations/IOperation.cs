namespace ChunkyImageLib.Operations
{
    internal interface IOperation : IDisposable
    {
        void DrawOnChunk(Chunk chunk, int chunkX, int chunkY);
        HashSet<(int, int)> FindAffectedChunks();
    }
}
