using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib;

public interface IReadOnlyChunkyImage
{
    bool DrawMostUpToDateChunkOn(VecI chunkPos, ChunkResolution resolution, SKSurface surface, VecI pos, SKPaint? paint = null);
    bool LatestOrCommittedChunkExists(VecI chunkPos);
    HashSet<VecI> FindAffectedChunks();
    HashSet<VecI> FindCommittedChunks();
    HashSet<VecI> FindAllChunks();
}
