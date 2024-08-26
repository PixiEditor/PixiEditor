using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace ChunkyImageLib.Operations;

internal interface IDrawOperation : IOperation
{
    bool IgnoreEmptyChunks { get; }
    void DrawOnChunk(Chunk targetChunk, VecI chunkPos);
    AffectedArea FindAffectedArea(VecI imageSize);
}
