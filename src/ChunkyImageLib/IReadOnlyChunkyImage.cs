using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib;

public interface IReadOnlyChunkyImage
{
    bool DrawMostUpToDateChunkOn(Vector2i chunkPos, ChunkResolution resolution, SKSurface surface, Vector2i pos, SKPaint? paint = null);
    bool LatestOrCommittedChunkExists(Vector2i chunkPos);
    HashSet<Vector2i> FindAffectedChunks();
    HashSet<Vector2i> FindCommittedChunks();
    HashSet<Vector2i> FindAllChunks();
}
