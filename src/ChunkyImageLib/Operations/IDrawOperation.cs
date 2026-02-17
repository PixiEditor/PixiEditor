using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal interface IDrawOperation : IOperation
{
    bool IgnoreEmptyChunks { get; }
    bool NeedsDrawInSrgb { get; }
    void DrawOnChunk(Chunk targetChunk, VecI chunkPos);
    AffectedArea FindAffectedArea(VecI imageSize);
}
