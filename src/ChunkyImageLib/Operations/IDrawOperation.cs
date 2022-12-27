using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace ChunkyImageLib.Operations;

internal interface IDrawOperation : IOperation
{
    bool IgnoreEmptyChunks { get; }
    void DrawOnChunk(Chunk chunk, VecI chunkPos);
    HashSet<VecI> FindAffectedChunks(VecI imageSize);
}
