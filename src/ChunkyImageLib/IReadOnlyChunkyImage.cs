using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib;

public interface IReadOnlyChunkyImage
{
    bool DrawMostUpToDateChunkOn(VecI chunkPos, ChunkResolution resolution, SKSurface surface, VecI pos, SKPaint? paint = null);
    SKColor GetCommittedPixel(VecI posOnImage);
    SKColor GetMostUpToDatePixel(VecI posOnImage);
    bool LatestOrCommittedChunkExists(VecI chunkPos);
    HashSet<VecI> FindAffectedChunks(int fromOperationIndex = 0);
    HashSet<VecI> FindCommittedChunks();
    HashSet<VecI> FindAllChunks();
}
