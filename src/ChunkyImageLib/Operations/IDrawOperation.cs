using ChunkyImageLib.DataHolders;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal interface IDrawOperation : IOperation
{
    bool IgnoreEmptyChunks { get; }
    void DrawOnChunk(Chunk targetChunk, VecI chunkPos);
    AffectedArea FindAffectedArea(VecI imageSize);
}
