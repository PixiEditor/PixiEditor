namespace ChunkyImageLib.Operations
{
    internal interface IOperation
    {
        void DrawOnChunk(ImageData chunk, int chunkX, int chunkY);
        HashSet<(int, int)> FindAffectedChunks(int chunkSize);
    }
}
