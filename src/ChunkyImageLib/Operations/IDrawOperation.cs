using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations;

internal interface IDrawOperation : IOperation
{
    bool IgnoreEmptyChunks { get; }
    void DrawOnChunk(Chunk chunk, Vector2i chunkPos);
    HashSet<Vector2i> FindAffectedChunks();
    IDrawOperation AsMirrored(int? verAxisX, int? horAxisY);
}
