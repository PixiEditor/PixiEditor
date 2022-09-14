using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;

namespace ChunkyImageLib;

public interface IReadOnlyChunkyImage
{
    bool DrawMostUpToDateChunkOn(VecI chunkPos, ChunkResolution resolution, DrawingSurface surface, VecI pos, Paint? paint = null);
    RectI? FindLatestBounds();
    Color GetCommittedPixel(VecI posOnImage);
    Color GetMostUpToDatePixel(VecI posOnImage);
    bool LatestOrCommittedChunkExists(VecI chunkPos);
    HashSet<VecI> FindAffectedChunks(int fromOperationIndex = 0);
    HashSet<VecI> FindCommittedChunks();
    HashSet<VecI> FindAllChunks();
}
