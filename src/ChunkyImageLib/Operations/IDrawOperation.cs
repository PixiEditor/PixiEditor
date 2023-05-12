using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace ChunkyImageLib.Operations;

internal interface IDrawOperation : IOperation
{
    bool IgnoreEmptyChunks { get; }
    void DrawOnChunk(Chunk targetChunk, VecI chunkPos, ChunkyImage caller);
    AffectedArea FindAffectedArea(VecI imageSize);
}
