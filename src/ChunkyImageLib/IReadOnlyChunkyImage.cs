using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib;

public interface IReadOnlyChunkyImage
{
    bool DrawMostUpToDateChunkOn(VecI chunkPos, ChunkResolution resolution, Canvas surface, VecD pos, Paint? paint = null, SamplingOptions? sampling = null);
    bool DrawCachedMostUpToDateChunkOn(VecI chunkPos, ChunkResolution resolution, Canvas surface, VecD pos, Paint? paint = null, SamplingOptions? sampling = null);
    bool DrawCommittedChunkOn(VecI chunkPos, ChunkResolution resolution, Canvas surface, VecD pos, Paint? paint = null, SamplingOptions? sampling = null);
    RectI? FindChunkAlignedMostUpToDateBounds();
    RectI? FindChunkAlignedCommittedBounds();
    RectI? FindTightCommittedBounds(ChunkResolution precision = ChunkResolution.Full, bool fallbackToChunkAligned = false);
    Color GetCommittedPixel(VecI posOnImage);
    Color GetMostUpToDatePixel(VecI posOnImage);
    bool LatestOrCommittedChunkExists(VecI chunkPos);
    AffectedArea FindAffectedArea(int fromOperationIndex = 0);
    HashSet<VecI> FindCommittedChunks();
    HashSet<VecI> FindAllChunks();
    VecI CommittedSize { get; }
    VecI LatestSize { get; }
    public ColorSpace ProcessingColorSpace { get; }
}
