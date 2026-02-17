using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using OneOf;
using OneOf.Types;
using PixiEditor.Common;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

[assembly: InternalsVisibleTo("ChunkyImageLibTest")]

namespace ChunkyImageLib;

/// <summary>
/// This class is thread-safe only for reading! Only the functions from IReadOnlyChunkyImage can be called from any thread.
/// ChunkyImage can be in two general states: 
/// 1. a state with all chunks committed and no queued operations
///     - latestChunks and latestChunksData are empty
///     - queuedOperations are empty
///     - committedChunks[ChunkResolution.Full] contains the current versions of all stored chunks
///     - committedChunks[*any other resolution*] may contain the current low res versions of some of the chunks (or all of them, or none)
///     - LatestSize == CommittedSize == current image size (px)
/// 2. and a state with some queued operations
///     - queuedOperations contains all requested operations (drawing, raster clips, clear, etc.)
///     - committedChunks[ChunkResolution.Full] contains the last versions before any operations of all stored chunks
///     - committedChunks[*any other resolution*] may contain the last low res versions before any operations of some of the chunks (or all of them, or none)
///     - latestChunks stores chunks with some (or none, or all) queued operations applied
///     - latestChunksData stores the data for some or all of the latest chunks (not necessarily synced with latestChunks).
///         The data includes how many operations from the queue have already been applied to the chunk, as well as chunk deleted state (the clear operation deletes chunks)
///     - LatestSize contains the new size if any resize operations were requested, otherwise the committed size
/// You can check the current state via queuedOperations.Count == 0
/// 
/// Depending on the chosen blend mode the latest chunks contain different things:
///     - BlendMode.Src: default mode, the latest chunks are the same as committed ones but with some or all queued operations applied. 
///         This means that operations can work with the existing pixels.
///     - Any other blend mode: the latest chunks contain only the things drawn by the queued operations.
///         They need to be drawn over the committed chunks to obtain the final image. In this case, operations won't have access to the existing pixels. 
/// </summary>
public class ChunkyImage : IReadOnlyChunkyImage, IDisposable, ICloneable, ICacheable
{
    private struct LatestChunkData
    {
        public LatestChunkData()
        {
            QueueProgress = 0;
            IsDeleted = false;
        }

        public int QueueProgress { get; set; }
        public bool IsDeleted { get; set; }
    }

    private bool disposed = false;
    private readonly object lockObject = new();
    private int commitCounter = 0;

    private RectI cachedPreciseCommitedBounds = RectI.Empty;
    private RectI cachedPreciseLatestBounds = RectI.Empty;
    private int lastCommitedBoundsCacheHash = -1;
    private int lastLatestBoundsCacheHash = -1;

    public const int FullChunkSize = ChunkPool.FullChunkSize;
    private static Paint ClippingPaint { get; } = new Paint() { BlendMode = BlendMode.DstIn };
    private static Paint InverseClippingPaint { get; } = new Paint() { BlendMode = BlendMode.DstOut };
    private static Paint ReplacingPaint { get; } = new Paint() { BlendMode = BlendMode.Src };

    private static Paint SmoothReplacingPaint { get; } =
        new Paint() { BlendMode = BlendMode.Src, FilterQuality = FilterQuality.Medium };

    private static Paint AddingPaint { get; } = new Paint() { BlendMode = BlendMode.Plus };
    private readonly Paint blendModePaint = new Paint() { BlendMode = BlendMode.Src };

    public ColorSpace ProcessingColorSpace { get; set; }

    public int CommitCounter => commitCounter;

    public VecI CommittedSize { get; private set; }
    public VecI LatestSize { get; private set; }

    public int QueueLength
    {
        get
        {
            lock (lockObject)
                return queuedOperations.Count;
        }
    }

    private readonly List<(IOperation operation, AffectedArea affectedArea)> queuedOperations = new();
    private readonly List<ChunkyImage> activeClips = new();
    private BlendMode blendMode = BlendMode.Src;
    private bool lockTransparency = false;
    private VectorPath? clippingPath;
    private double? horizontalSymmetryAxis = null;
    private double? verticalSymmetryAxis = null;

    private int operationCounter = 0;

    private readonly Dictionary<ChunkResolution, Dictionary<VecI, Chunk>> committedChunks;
    private readonly Dictionary<ChunkResolution, Dictionary<VecI, Chunk>> latestChunks;
    private readonly Dictionary<ChunkResolution, Dictionary<VecI, LatestChunkData>> latestChunksData;

    public ChunkyImage(VecI size, ColorSpace colorSpace)
    {
        CommittedSize = size;
        LatestSize = size;
        committedChunks = CreateChunkResolutionDictionary<Chunk>();
        latestChunks = CreateChunkResolutionDictionary<Chunk>();
        latestChunksData = CreateChunkResolutionDictionary<LatestChunkData>();

        ProcessingColorSpace = colorSpace;
    }

    private static Dictionary<ChunkResolution, Dictionary<VecI, T>> CreateChunkResolutionDictionary<T>() =>
        new()
        {
            [ChunkResolution.Full] = new Dictionary<VecI, T>(),
            [ChunkResolution.Half] = new Dictionary<VecI, T>(),
            [ChunkResolution.Quarter] = new Dictionary<VecI, T>(),
            [ChunkResolution.Eighth] = new Dictionary<VecI, T>(),
        };

    public ChunkyImage(Surface image, ColorSpace colorSpace) : this(image.Size, colorSpace)
    {
        EnqueueDrawImage(VecI.Zero, image);
        CommitChanges();
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public RectI? FindChunkAlignedMostUpToDateBounds()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            RectI? rect = null;
            foreach (var pos in committedChunks[ChunkResolution.Full].Keys)
            {
                RectI chunkBounds = new RectI(pos * FullChunkSize, new VecI(FullChunkSize));
                rect ??= chunkBounds;
                rect = rect.Value.Union(chunkBounds);
            }

            foreach (var operation in queuedOperations)
            {
                foreach (var pos in operation.affectedArea.Chunks)
                {
                    RectI chunkBounds = new RectI(pos * FullChunkSize, new VecI(FullChunkSize));
                    rect ??= chunkBounds;
                    rect = rect.Value.Union(chunkBounds);
                }
            }

            return rect;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public RectI? FindChunkAlignedCommittedBounds()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            RectI? rect = null;
            foreach (var pos in committedChunks[ChunkResolution.Full].Keys)
            {
                RectI chunkBounds = new RectI(pos * FullChunkSize, new VecI(FullChunkSize));
                rect ??= chunkBounds;
                rect = rect.Value.Union(chunkBounds);
            }

            return rect;
        }
    }

    /// <summary>
    /// Finds the precise bounds in <paramref name="suggestedResolution"/>. If there are no chunks rendered for that resolution, full res chunks are used instead.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public RectI? FindTightCommittedBounds(ChunkResolution suggestedResolution = ChunkResolution.Full,
        bool fallbackToChunkAligned = false)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();

            if (lastCommitedBoundsCacheHash == GetCacheHash())
            {
                return cachedPreciseCommitedBounds;
            }

            var chunkSize = suggestedResolution.PixelSize();
            var multiplier = suggestedResolution.Multiplier();
            RectI scaledCommittedSize = (RectI)(new RectD(VecI.Zero, CommittedSize * multiplier)).RoundOutwards();

            RectI? preciseBounds = null;

            foreach (var (chunkPos, fullResChunk) in committedChunks[ChunkResolution.Full])
            {
                if (committedChunks[suggestedResolution].TryGetValue(chunkPos, out Chunk? requestedResChunk))
                {
                    RectI visibleArea = new RectI(chunkPos * chunkSize, new VecI(chunkSize))
                        .Intersect(scaledCommittedSize).Translate(-chunkPos * chunkSize);

                    RectI? chunkPreciseBounds = requestedResChunk.FindPreciseBounds(visibleArea);
                    if (chunkPreciseBounds is null)
                        continue;
                    RectI globalChunkBounds = chunkPreciseBounds.Value.Offset(chunkPos * chunkSize);

                    preciseBounds ??= globalChunkBounds;
                    preciseBounds = preciseBounds.Value.Union(globalChunkBounds);
                }
                else
                {
                    if (fallbackToChunkAligned)
                    {
                        return FindChunkAlignedCommittedBounds();
                    }

                    RectI visibleArea = new RectI(chunkPos * FullChunkSize, new VecI(FullChunkSize))
                        .Intersect(new RectI(VecI.Zero, CommittedSize)).Translate(-chunkPos * FullChunkSize);

                    RectI? chunkPreciseBounds = fullResChunk.FindPreciseBounds(visibleArea);
                    if (chunkPreciseBounds is null)
                        continue;
                    RectI globalChunkBounds = (RectI)chunkPreciseBounds.Value.Scale(multiplier)
                        .Offset(chunkPos * chunkSize).RoundOutwards();

                    preciseBounds ??= globalChunkBounds;
                    preciseBounds = preciseBounds.Value.Union(globalChunkBounds);
                }
            }

            preciseBounds = (RectI?)preciseBounds?.Scale(suggestedResolution.InvertedMultiplier()).RoundOutwards();
            preciseBounds = preciseBounds?.Intersect(new RectI(preciseBounds.Value.Pos, CommittedSize));

            cachedPreciseCommitedBounds = preciseBounds.GetValueOrDefault();
            lastCommitedBoundsCacheHash = GetCacheHash();

            return preciseBounds;
        }
    }

    public RectI? FindTightLatestBounds(ChunkResolution suggestedResolution = ChunkResolution.Full,
        bool fallbackToChunkAligned = false)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();

            if (queuedOperations.Count == 0)
            {
                return FindTightCommittedBounds(suggestedResolution, fallbackToChunkAligned);
            }

            if (lastLatestBoundsCacheHash == GetCacheHash())
            {
                return cachedPreciseLatestBounds;
            }

            var chunkSize = suggestedResolution.PixelSize();
            var multiplier = suggestedResolution.Multiplier();
            RectI scaledLatestSize = (RectI)(new RectD(VecI.Zero, LatestSize * multiplier)).RoundOutwards();

            RectI? preciseBounds = null;

            var possibleChunks = new HashSet<VecI>();
            foreach (var pos in committedChunks[ChunkResolution.Full].Keys)
                possibleChunks.Add(pos);

            foreach (var pos in latestChunks[ChunkResolution.Full].Keys)
                possibleChunks.Add(pos);

            foreach (var chunkPos in possibleChunks)
            {
                var committedChunk = GetCommittedChunk(chunkPos, suggestedResolution);
                var latestChunk = GetLatestChunk(chunkPos, suggestedResolution);

                Chunk? chunk;
                bool isTempChunk = false;

                if (latestChunk != null && committedChunk != null)
                {
                    // both exist, need to merge
                    var tempChunk = Chunk.Create(ProcessingColorSpace, suggestedResolution);
                    tempChunk.Surface.DrawingSurface.Canvas.DrawSurface(committedChunk.Surface.DrawingSurface, 0, 0,
                        ReplacingPaint);
                    blendModePaint.BlendMode = blendMode;
                    tempChunk.Surface.DrawingSurface.Canvas.DrawSurface(latestChunk.Surface.DrawingSurface, 0, 0,
                        blendModePaint);
                    if (lockTransparency)
                        OperationHelper.ClampAlpha(tempChunk.Surface,
                            committedChunk.Surface);
                    chunk = tempChunk;
                    isTempChunk = true;
                }
                else if (latestChunk != null)
                {
                    chunk = latestChunk;
                }
                else
                {
                    chunk = committedChunk;
                }


                if (chunk != null)
                {
                    RectI visibleArea = new RectI(chunkPos * chunkSize, new VecI(chunkSize))
                        .Intersect(scaledLatestSize).Translate(-chunkPos * chunkSize);

                    RectI? chunkPreciseBounds = chunk.FindPreciseBounds(visibleArea);
                    if (chunkPreciseBounds is null)
                        continue;
                    RectI globalChunkBounds = chunkPreciseBounds.Value.Offset(chunkPos * chunkSize);

                    preciseBounds ??= globalChunkBounds;
                    preciseBounds = preciseBounds.Value.Union(globalChunkBounds);

                    if (isTempChunk)
                    {
                        chunk.Dispose();
                    }
                }
                else
                {
                    if (fallbackToChunkAligned)
                    {
                        return FindChunkAlignedMostUpToDateBounds();
                    }

                    RectI visibleArea = new RectI(chunkPos * FullChunkSize, new VecI(FullChunkSize))
                        .Intersect(new RectI(VecI.Zero, LatestSize)).Translate(-chunkPos * FullChunkSize);

                    RectI? chunkPreciseBounds = chunk.FindPreciseBounds(visibleArea);
                    if (chunkPreciseBounds is null)
                        continue;
                    RectI globalChunkBounds = (RectI)chunkPreciseBounds.Value.Scale(multiplier)
                        .Offset(chunkPos * chunkSize).RoundOutwards();

                    preciseBounds ??= globalChunkBounds;
                    preciseBounds = preciseBounds.Value.Union(globalChunkBounds);
                }
            }

            preciseBounds = (RectI?)preciseBounds?.Scale(suggestedResolution.InvertedMultiplier()).RoundOutwards();
            preciseBounds = preciseBounds?.Intersect(new RectI(preciseBounds.Value.Pos, LatestSize));

            cachedPreciseLatestBounds = preciseBounds.GetValueOrDefault();
            lastLatestBoundsCacheHash = GetCacheHash();

            return preciseBounds;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public ChunkyImage CloneFromCommitted()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ChunkyImage output = new(LatestSize, ProcessingColorSpace);
            var chunks = FindCommittedChunks();
            foreach (var chunk in chunks)
            {
                var image = GetCommittedChunk(chunk, ChunkResolution.Full);
                if (image is null)
                    continue;
                output.EnqueueDrawTexture(chunk * FullChunkSize, image.Surface);
            }

            output.CommitChanges();
            return output;
        }
    }

    public ChunkyImage CloneFromLatest()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ChunkyImage output = new(LatestSize, ProcessingColorSpace);
            var chunks = FindAllChunks();
            foreach (var chunk in chunks)
            {
                var image = GetLatestChunk(chunk, ChunkResolution.Full);
                if (image is null)
                {
                    image = GetCommittedChunk(chunk, ChunkResolution.Full);
                    if (image is null)
                        continue;
                }

                output.EnqueueDrawTexture(chunk * FullChunkSize, image.Surface);
            }

            output.CommitChanges();
            return output;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public Color GetCommittedPixel(VecI posOnImage)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            var chunkPos = OperationHelper.GetChunkPos(posOnImage, FullChunkSize);
            var posInChunk = posOnImage - chunkPos * FullChunkSize;
            return MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) switch
            {
                null => Colors.Transparent,
                var chunk => chunk.Surface.GetSrgbPixel(posInChunk)
            };
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public Color GetCommittedPixelRaw(VecI posOnImage)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            var chunkPos = OperationHelper.GetChunkPos(posOnImage, FullChunkSize);
            var posInChunk = posOnImage - chunkPos * FullChunkSize;
            return MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) switch
            {
                null => Colors.Transparent,
                var chunk => chunk.Surface.GetRawPixel(posInChunk)
            };
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public Color GetMostUpToDatePixel(VecI posOnImage)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            var chunkPos = OperationHelper.GetChunkPos(posOnImage, FullChunkSize);
            var posInChunk = posOnImage - chunkPos * FullChunkSize;

            // nothing queued, return committed
            if (queuedOperations.Count == 0)
            {
                Chunk? committedChunk = MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full);
                return committedChunk switch
                {
                    null => Colors.Transparent,
                    _ => committedChunk.Surface.GetSrgbPixel(posInChunk)
                };
            }

            // something is queued, blend mode is Src so no merging needed
            if (blendMode == BlendMode.Src)
            {
                Chunk? latestChunk = GetLatestChunk(chunkPos, ChunkResolution.Full);
                return latestChunk switch
                {
                    null => Colors.Transparent,
                    _ => latestChunk.Surface.GetSrgbPixel(posInChunk)
                };
            }

            // something is queued, blend mode is not Src so we have to do merging
            {
                using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
                Chunk? committedChunk = MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full);
                Chunk? latestChunk = GetLatestChunk(chunkPos, ChunkResolution.Full);
                Color committedColor = committedChunk is null
                    ? Colors.Transparent
                    : committedChunk.Surface.GetSrgbPixel(posInChunk);
                Color latestColor = latestChunk is null
                    ? Colors.Transparent
                    : latestChunk.Surface.GetSrgbPixel(posInChunk);
                // using a whole chunk just to draw 1 pixel is kinda dumb,
                // but this should be faster than any approach that requires allocations
                using Chunk tempChunk = Chunk.Create(ProcessingColorSpace, ChunkResolution.Eighth);
                using Paint committedPaint = new Paint() { Color = committedColor, BlendMode = BlendMode.Src };
                using Paint latestPaint = new Paint() { Color = latestColor, BlendMode = this.blendMode };
                tempChunk.Surface.DrawingSurface.Canvas.DrawRect(new RectD(VecI.Zero, new VecD(1)), committedPaint);
                tempChunk.Surface.DrawingSurface.Canvas.DrawRect(new RectD(VecI.Zero, new VecI(1)), latestPaint);
                return tempChunk.Surface.GetSrgbPixel(VecI.Zero);
            }
        }
    }

    /// <returns>
    /// True if the chunk existed and was drawn, otherwise false
    /// </returns>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public bool DrawMostUpToDateChunkOn(VecI chunkPos, ChunkResolution resolution, Canvas surface, VecD pos,
        Paint? paint = null, SamplingOptions? samplingOptions = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            OneOf<None, EmptyChunk, Chunk> latestChunk;
            {
                var chunk = GetLatestChunk(chunkPos, resolution);
                if (latestChunksData[resolution].TryGetValue(chunkPos, out var chunkData) && chunkData.IsDeleted)
                {
                    latestChunk = new EmptyChunk();
                }
                else
                {
                    latestChunk = chunk is null ? new None() : chunk;
                }
            }

            var committedChunk = GetCommittedChunk(chunkPos, resolution);

            // draw committed directly
            if (latestChunk.IsT0 || latestChunk.IsT1 && committedChunk is not null && !BlendModeNeedsSource())
            {
                if (committedChunk is null)
                    return false;
                committedChunk.DrawChunkOn(surface, pos, paint, samplingOptions);
                return true;
            }

            // no need to combine with committed, draw directly
            if (blendMode == BlendMode.Src || committedChunk is null)
            {
                // Likely DstOut check is not exhaustive. Add more if needed
                if (latestChunk.IsT2 && blendMode != BlendMode.DstOut)
                {
                    latestChunk.AsT2.DrawChunkOn(surface, pos, paint, samplingOptions);
                    return true;
                }

                return false;
            }

            using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
            // combine with committed and then draw
            using var tempChunk = Chunk.Create(ProcessingColorSpace, resolution);
            tempChunk.Surface.DrawingSurface.Canvas.DrawSurface(committedChunk.Surface.DrawingSurface, 0, 0,
                ReplacingPaint);
            blendModePaint.BlendMode = blendMode;
            tempChunk.Surface.DrawingSurface.Canvas.DrawSurface(latestChunk.AsT2.Surface.DrawingSurface, 0, 0,
                blendModePaint);
            if (lockTransparency)
                OperationHelper.ClampAlpha(tempChunk.Surface, committedChunk.Surface);
            tempChunk.DrawChunkOn(surface, pos, paint, samplingOptions);

            return true;
        }
    }

    private bool BlendModeNeedsSource()
    {
        return blendMode is BlendMode.Src or BlendMode.DstIn or BlendMode.DstOut;
    }

    public bool DrawCachedMostUpToDateChunkOn(VecI chunkPos, ChunkResolution resolution, Canvas surface,
        VecD pos,
        Paint? paint = null, SamplingOptions? sampling = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            OneOf<None, EmptyChunk, Chunk> latestChunk;
            {
                var chunk = MaybeGetLatestChunk(chunkPos, resolution);
                if (latestChunksData[resolution].TryGetValue(chunkPos, out var chunkData) && chunkData.IsDeleted)
                {
                    latestChunk = new EmptyChunk();
                }
                else
                {
                    if (chunk is null && queuedOperations.Count > 0 && resolution != ChunkResolution.Full &&
                        MaybeGetLatestChunk(chunkPos, ChunkResolution.Full) != null)
                    {
                        chunk = GetLatestChunk(chunkPos, resolution);
                    }

                    latestChunk = chunk is null ? new None() : chunk;
                }
            }

            var committedChunk = GetCommittedChunk(chunkPos, resolution);

            // draw committed directly
            if (latestChunk.IsT0 || latestChunk.IsT1 && committedChunk is not null && !BlendModeNeedsSource())
            {
                if (committedChunk is null)
                    return false;
                committedChunk.DrawChunkOn(surface, pos, paint, sampling);
                return true;
            }

            // no need to combine with committed, draw directly
            if (blendMode == BlendMode.Src || committedChunk is null)
            {
                if (latestChunk.IsT2)
                {
                    var originalBlendMode = paint?.BlendMode ?? BlendMode.SrcOver;
                    if(paint != null && BlendModeNeedsSource())
                    {
                        paint.BlendMode = blendMode;
                    }

                    latestChunk.AsT2.DrawChunkOn(surface, pos, paint, sampling);
                    if(paint != null)
                    {
                        paint.BlendMode = originalBlendMode;
                    }
                    return true;
                }

                return false;
            }

            using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();

            // combine with committed and then draw
            using var tempChunk = Chunk.Create(ProcessingColorSpace, resolution);
            tempChunk.Surface.DrawingSurface.Canvas.DrawSurface(committedChunk.Surface.DrawingSurface, 0, 0,
                ReplacingPaint);
            blendModePaint.BlendMode = blendMode;
            tempChunk.Surface.DrawingSurface.Canvas.DrawSurface(latestChunk.AsT2.Surface.DrawingSurface, 0, 0,
                blendModePaint);
            if (lockTransparency)
                OperationHelper.ClampAlpha(tempChunk.Surface, committedChunk.Surface);
            tempChunk.DrawChunkOn(surface, pos, paint, sampling);

            return true;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public bool LatestOrCommittedChunkExists(VecI chunkPos)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            if (MaybeGetLatestChunk(chunkPos, ChunkResolution.Full) is not null ||
                MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) is not null)
                return true;
            foreach (var operation in queuedOperations)
            {
                if (operation.affectedArea.Chunks.Contains(chunkPos))
                    return true;
            }

            return false;
        }
    }

    public bool LatestOrCommittedChunkExists()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            var chunks = FindAllChunks();
            foreach (var chunk in chunks)
            {
                if (LatestOrCommittedChunkExists(chunk))
                    return true;
            }
        }

        return false;
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public bool DrawCommittedChunkOn(VecI chunkPos, ChunkResolution resolution, Canvas surface, VecD pos,
        Paint? paint = null, SamplingOptions? samplingOptions = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            var chunk = GetCommittedChunk(chunkPos, resolution);
            if (chunk is null)
                return false;
            chunk.DrawChunkOn(surface, pos, paint, samplingOptions);
            return true;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    internal bool CommittedChunkExists(VecI chunkPos)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            return MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) is not null;
        }
    }

    /// <summary>
    /// Returns the latest version of the chunk if it exists or should exist based on queued operation. The returned chunk is fully up to date.
    /// </summary>
    private Chunk? GetLatestChunk(VecI pos, ChunkResolution resolution)
    {
        if (queuedOperations.Count == 0)
            return null;

        MaybeCreateAndProcessQueueForChunk(pos, resolution);
        var maybeNewlyProcessedChunk = MaybeGetLatestChunk(pos, resolution);
        return maybeNewlyProcessedChunk;
    }

    /// <summary>
    /// Tries it's best to return a committed chunk, either if it exists or if it can be created from it's high res version. Returns null if it can't.
    private Chunk? GetCommittedChunk(VecI pos, ChunkResolution resolution)
    {
        var maybeSameRes = MaybeGetCommittedChunk(pos, resolution);
        if (maybeSameRes is not null)
            return maybeSameRes;

        var maybeFullRes = MaybeGetCommittedChunk(pos, ChunkResolution.Full);
        if (maybeFullRes is not null)
            return GetOrCreateCommittedChunk(pos, resolution);

        return null;
    }

    private Chunk? MaybeGetLatestChunk(VecI pos, ChunkResolution resolution)
        => latestChunks[resolution].GetValueOrDefault(pos);

    private Chunk? MaybeGetCommittedChunk(VecI pos, ChunkResolution resolution)
        => committedChunks[resolution].GetValueOrDefault(pos);

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void AddRasterClip(ChunkyImage clippingMask)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException(
                    "This function can only be executed when there are no queued operations");
            activeClips.Add(clippingMask);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void SetClippingPath(VectorPath clippingPath)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException(
                    "This function can only be executed when there are no queued operations");
            this.clippingPath = clippingPath;
        }
    }

    /// <summary>
    /// Porter duff compositing operators (apart from SrcOver) likely won't have the intended effect.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void SetBlendMode(BlendMode mode)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException(
                    "This function can only be executed when there are no queued operations");
            blendMode = mode;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void SetHorizontalAxisOfSymmetry(double position)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException(
                    "This function can only be executed when there are no queued operations");
            horizontalSymmetryAxis = position;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void SetVerticalAxisOfSymmetry(double position)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException(
                    "This function can only be executed when there are no queued operations");
            verticalSymmetryAxis = position;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnableLockTransparency()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            lockTransparency = true;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueReplaceColor(Color oldColor, Color newColor)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ReplaceColorOperation operation = new(oldColor, newColor);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawRectangle(ShapeData rect)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            RectangleOperation operation = new(rect);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawEllipse(RectD location, Paintable? strokeColor, Paintable? fillColor, float strokeWidth,
        double rotationRad = 0, bool antiAliased = false,
        Paint? paint = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            EllipseOperation operation = new(location, strokeColor, fillColor, strokeWidth, rotationRad, antiAliased,
                paint);
            EnqueueOperation(operation);
        }
    }

    /// <summary>
    /// Be careful about the copyImage argument. The default is true, and this is a thread safe version without any side effects. 
    /// It will however copy the surface right away which can be slow (in updateable changes especially). 
    /// If copyImage is set to false, the image won't be copied and instead a reference will be stored.
    /// Surface is NOT THREAD SAFE, so if you pass a Surface here with copyImage == false you must not do anything with that surface anywhere (not even read) until CommitChanges/CancelChanges is called.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawImage(Matrix3X3 transformMatrix, Surface image, SamplingOptions samplingOptions,
        Paint? paint = null, bool copyImage = true)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ImageOperation operation = new(transformMatrix, image, samplingOptions, paint, copyImage);
            EnqueueOperation(operation);
        }
    }

    /// <summary>
    /// Be careful about the copyImage argument, see other overload for details
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawImage(ShapeCorners corners, Surface image, Paint? paint = null, bool copyImage = true)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ImageOperation operation = new(corners, image, paint, copyImage);
            EnqueueOperation(operation);
        }
    }

    /// <summary>
    /// Be careful about the copyImage argument, see other overload for details
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawImage(VecI pos, Surface image, Paint? paint = null, bool copyImage = true)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ImageOperation operation = new(pos, image, paint, copyImage);
            EnqueueOperation(operation);
        }
    }

    /// <summary>
    /// Be careful about the copyImage argument. The default is true, and this is a thread safe version without any side effects. 
    /// It will however copy the surface right away which can be slow (in updateable changes especially). 
    /// If copyImage is set to false, the image won't be copied and instead a reference will be stored.
    /// Texture is NOT THREAD SAFE, so if you pass a Texture here with copyImage == false you must not do anything with that texture anywhere (not even read) until CommitChanges/CancelChanges is called.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawTexture(Matrix3X3 transformMatrix, Texture image, Paint? paint = null, bool copyImage = true)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            TextureOperation operation = new(transformMatrix, image, paint, copyImage);
            EnqueueOperation(operation);
        }
    }

    /// <summary>
    /// Be careful about the copyImage argument, see other overload for details
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawTexture(ShapeCorners corners, Texture image, Paint? paint = null, bool copyImage = true)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            TextureOperation operation = new(corners, image, paint, copyImage);
            EnqueueOperation(operation);
        }
    }

    /// <summary>
    /// Be careful about the copyImage argument, see other overload for details
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawTexture(VecI pos, Texture image, Paint? paint = null, bool copyImage = true)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            TextureOperation operation = new(pos, image, paint, copyImage);
            EnqueueOperation(operation);
        }
    }

    public void EnqueueApplyMask(ChunkyImage mask)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ApplyMaskOperation operation = new(mask);
            EnqueueOperation(operation);
        }
    }

    /// <param name="customBounds">Bounds used for affected chunks, will be computed from path in O(n) if null is passed</param>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawPath(VectorPath path, Color color, float strokeWidth, StrokeCap strokeCap,
        BlendMode blendMode, RectI? customBounds = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            PathOperation operation = new(path, color, strokeWidth, strokeCap, blendMode, customBounds);
            EnqueueOperation(operation);
        }
    }

    /// <param name="customBounds">Bounds used for affected chunks, will be computed from path in O(n) if null is passed</param>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawPath(VectorPath path, Paintable paintable, float strokeWidth, StrokeCap strokeCap,
        BlendMode blendMode, PaintStyle style, bool antiAliasing, RectI? customBounds = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            PathOperation operation = new(path, paintable, strokeWidth, strokeCap, blendMode, style, antiAliasing,
                customBounds);
            EnqueueOperation(operation);
        }
    }

    /// <param name="customBounds">Bounds used for affected chunks, will be computed from path in O(n) if null is passed</param>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawPath(VectorPath path, Paintable paintable, float strokeWidth, StrokeCap strokeCap,
        Blender blender, PaintStyle style, bool antiAliasing, RectI? customBounds = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            PathOperation operation = new(path, paintable, strokeWidth, strokeCap, blender, style, antiAliasing,
                customBounds);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawBresenhamLine(VecI from, VecI to, Paintable paintable, BlendMode blendMode)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            BresenhamLineOperation operation = new(from, to, paintable, blendMode);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawSkiaLine(VecD from, VecD to, StrokeCap strokeCap, float strokeWidth, Color color,
        BlendMode blendMode)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            DrawingSurfaceLineOperation operation = new(from, to, strokeCap, strokeWidth, color, blendMode);
            EnqueueOperation(operation);
        }
    }

    public void EnqueueDrawSkiaLine(VecD from, VecD to, Paint paint)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            DrawingSurfaceLineOperation operation = new(from, to, paint);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawPixels(IEnumerable<VecI> pixels, Color color, BlendMode blendMode)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            PixelsOperation operation = new(pixels, color, blendMode);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawPixel(VecI pos, Color color, BlendMode blendMode)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            PixelOperation operation = new(pos, color, blendMode);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueDrawCommitedChunkyImage(VecI pos, ChunkyImage image, bool flipHor = false, bool flipVer = false)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ChunkyImageOperation operation = new(image, pos, flipHor, flipVer, false);
            EnqueueOperation(operation);
        }
    }

    public void EnqueueDrawUpToDateChunkyImage(VecI pos, ChunkyImage image, bool flipHor = false, bool flipVer = false)
    {
        ThrowIfDisposed();
        ChunkyImageOperation operation = new(image, pos, flipHor, flipVer, true);
        EnqueueOperation(operation);
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueClearRegion(RectI region)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ClearRegionOperation operation = new(region);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueClearPath(VectorPath path, RectI? pathTightBounds = null)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ClearPathOperation operation = new(path, pathTightBounds);
            EnqueueOperation(operation);
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueClear()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ClearOperation operation = new();
            EnqueueOperation(operation, new(FindAllChunks()));
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void EnqueueResize(VecI newSize)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ResizeOperation operation = new(newSize);
            LatestSize = newSize;
            EnqueueOperation(operation, new(FindAllChunksOutsideBounds(newSize)));
        }
    }


    public void EnqueueDrawPaint(Paint paint)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            PaintOperation operation = new(paint);
            EnqueueOperation(operation);
        }
    }

    private void EnqueueOperation(IDrawOperation operation)
    {
        List<IDrawOperation> operations = new(4) { operation };

        if (operation is IMirroredDrawOperation mirroredOperation)
        {
            if (horizontalSymmetryAxis is not null && verticalSymmetryAxis is not null)
                operations.Add(mirroredOperation.AsMirrored(verticalSymmetryAxis, horizontalSymmetryAxis));
            if (horizontalSymmetryAxis is not null)
                operations.Add(mirroredOperation.AsMirrored(null, horizontalSymmetryAxis));
            if (verticalSymmetryAxis is not null)
                operations.Add(mirroredOperation.AsMirrored(verticalSymmetryAxis, null));
        }

        foreach (var op in operations)
        {
            var area = op.FindAffectedArea(LatestSize);
            area.Chunks.RemoveWhere(pos => IsOutsideBounds(pos, LatestSize));
            area.GlobalArea = area.GlobalArea?.Intersect(new RectI(VecI.Zero, LatestSize));
            if (operation.IgnoreEmptyChunks)
                area.Chunks.IntersectWith(FindAllChunks());
            EnqueueOperation(op, area);
            operationCounter++;
        }
    }

    private void EnqueueOperation(IOperation operation, AffectedArea area)
    {
        queuedOperations.Add((operation, area));
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void CancelChanges()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            //clear queued operations
            foreach (var operation in queuedOperations)
                operation.operation.Dispose();
            queuedOperations.Clear();

            //clear additional state
            activeClips.Clear();
            blendMode = BlendMode.Src;
            lockTransparency = false;
            horizontalSymmetryAxis = null;
            verticalSymmetryAxis = null;
            clippingPath = null;

            //clear latest chunks
            foreach (var chunksOfRes in latestChunks.Values)
            {
                foreach (var chunk in chunksOfRes.Values)
                {
                    chunk.Dispose();
                }
            }

            LatestSize = CommittedSize;
            foreach (var (res, chunks) in latestChunks)
            {
                chunks.Clear();
                latestChunksData[res].Clear();
            }
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public void CommitChanges()
    {
        lock (lockObject)
        {
            using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
            ThrowIfDisposed();
            var affectedArea = FindAffectedArea();

            foreach (var chunk in affectedArea.Chunks)
            {
                MaybeCreateAndProcessQueueForChunk(chunk, ChunkResolution.Full);
            }

            foreach (var (operation, _) in queuedOperations)
            {
                operation.Dispose();
            }

            CommitLatestChunks();
            CommittedSize = LatestSize;
            queuedOperations.Clear();
            activeClips.Clear();
            blendMode = BlendMode.Src;
            lockTransparency = false;
            horizontalSymmetryAxis = null;
            verticalSymmetryAxis = null;
            clippingPath = null;

            commitCounter++;
            if (commitCounter % 30 == 0)
                FindAndDeleteEmptyCommittedChunks();
        }
    }

    /// <summary>
    /// Does all necessary steps to convert latest chunks into committed ones. The latest chunk dictionary become empty after this function is called.
    /// </summary>
    private void CommitLatestChunks()
    {
        // move/draw fully processed latest chunks to/on committed
        foreach (var (resolution, chunks) in latestChunks)
        {
            foreach (var (pos, chunk) in chunks)
            {
                // get chunk if exists
                LatestChunkData data = latestChunksData[resolution][pos];
                if (data.QueueProgress != queuedOperations.Count)
                {
                    if (resolution == ChunkResolution.Full)
                    {
                        throw new InvalidOperationException(
                            "Trying to commit a full res chunk that wasn't fully processed");
                    }
                    else
                    {
                        chunk.Dispose();
                        continue;
                    }
                }

                // do a swap
                if (blendMode == BlendMode.Src)
                {
                    // delete committed version
                    if (committedChunks[resolution].ContainsKey(pos))
                    {
                        var oldChunk = committedChunks[resolution][pos];
                        committedChunks[resolution].Remove(pos);
                        oldChunk.Dispose();
                    }

                    // put the latest version in place of the committed one
                    if (!data.IsDeleted)
                        committedChunks[resolution].Add(pos, chunk);
                    else
                        chunk.Dispose();
                }
                // do blending
                else
                {
                    // nothing to blend, continue
                    if (data.IsDeleted)
                    {
                        chunk.Dispose();
                        continue;
                    }

                    // nothing to blend with, swap
                    var maybeCommitted = MaybeGetCommittedChunk(pos, resolution);
                    if (maybeCommitted is null)
                    {
                        if (!BlendModeNeedsSource())
                        {
                            committedChunks[resolution].Add(pos, chunk);
                        }

                        continue;
                    }

                    using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();

                    //blend
                    blendModePaint.BlendMode = blendMode;
                    if (lockTransparency)
                    {
                        using Chunk tempChunk = Chunk.Create(ProcessingColorSpace, resolution);
                        tempChunk.Surface.DrawingSurface.Canvas.DrawSurface(maybeCommitted.Surface.DrawingSurface, 0, 0,
                            ReplacingPaint);
                        maybeCommitted.Surface.DrawingSurface.Canvas.DrawSurface(chunk.Surface.DrawingSurface, 0, 0,
                            blendModePaint);
                        OperationHelper.ClampAlpha(maybeCommitted.Surface,
                            tempChunk.Surface);
                    }
                    else
                    {
                        maybeCommitted.Surface.DrawingSurface.Canvas.DrawSurface(chunk.Surface.DrawingSurface, 0, 0,
                            blendModePaint);
                    }

                    chunk.Dispose();
                }
            }
        }

        // delete committed low res chunks that weren't updated
        foreach (var pos in latestChunks[ChunkResolution.Full].Keys)
        {
            foreach (var resolution in latestChunks.Keys)
            {
                if (resolution == ChunkResolution.Full)
                    continue;
                if (!latestChunksData[resolution].TryGetValue(pos, out var halfChunk) ||
                    halfChunk.QueueProgress != queuedOperations.Count)
                {
                    if (committedChunks[resolution].TryGetValue(pos, out var committedLowResChunk))
                    {
                        committedChunks[resolution].Remove(pos);
                        committedLowResChunk.Dispose();
                    }
                }
            }
        }

        // clear latest chunks
        foreach (var (resolution, chunks) in latestChunks)
        {
            chunks.Clear();
            latestChunksData[resolution].Clear();
        }
    }

    /// <returns>
    ///     All chunks that have something in them, including latest (uncommitted) ones
    /// </returns>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public HashSet<VecI> FindAllChunks()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            var allChunks = committedChunks[ChunkResolution.Full].Select(chunk => chunk.Key).ToHashSet();
            foreach (var (_, affArea) in queuedOperations)
            {
                allChunks.UnionWith(affArea.Chunks);
            }

            return allChunks;
        }
    }

    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public HashSet<VecI> FindCommittedChunks()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            return committedChunks[ChunkResolution.Full].Select(chunk => chunk.Key).ToHashSet();
        }
    }

    public Dictionary<VecI, Surface> CloneAllCommitedNonEmptyChunks()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
            var dict = new Dictionary<VecI, Surface>();
            foreach (var (pos, chunk) in committedChunks[ChunkResolution.Full])
            {
                if (chunk.FindPreciseBounds().HasValue)
                {
                    var surf = new Surface(chunk.Surface.ImageInfo);
                    surf.DrawingSurface.Canvas.DrawSurface(chunk.Surface.DrawingSurface, 0, 0);
                    dict[pos] = surf;
                    surf.DrawingSurface.Flush();
                }
            }

            return dict;
        }
    }

    /// <returns>
    /// Chunks affected by operations that haven't been committed yet
    /// </returns>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public AffectedArea FindAffectedArea(int fromOperationIndex = 0)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            var chunks = new HashSet<VecI>();
            RectI? rect = null;

            for (int i = fromOperationIndex; i < queuedOperations.Count; i++)
            {
                var (_, area) = queuedOperations[i];
                chunks.UnionWith(area.Chunks);

                rect ??= area.GlobalArea;
                if (area.GlobalArea is not null && rect is not null)
                    rect = rect.Value.Union(area.GlobalArea.Value);
            }

            return new AffectedArea(chunks, rect);
        }
    }

    public void SetCommitedChunk(Chunk chunk, VecI pos, ChunkResolution resolution)
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            committedChunks[resolution][pos] = chunk;
        }
    }

    /// <summary>
    /// Applies all operations queued for a specific (latest) chunk. If the latest chunk doesn't exist yet, creates it. If none of the existing operations affect the chunk does nothing.
    /// </summary>
    private void MaybeCreateAndProcessQueueForChunk(VecI chunkPos, ChunkResolution resolution)
    {
        if (!latestChunksData[resolution].TryGetValue(chunkPos, out LatestChunkData chunkData))
            chunkData = new()
            {
                QueueProgress = 0, IsDeleted = !committedChunks[ChunkResolution.Full].ContainsKey(chunkPos)
            };
        if (chunkData.QueueProgress == queuedOperations.Count)
            return;

        Chunk? targetChunk = null;
        OneOf<FilledChunk, EmptyChunk, Chunk> combinedRasterClips = new FilledChunk();

        bool initialized = false;

        for (int i = 0; i < queuedOperations.Count; i++)
        {
            var (operation, affArea) = queuedOperations[i];
            if (!affArea.Chunks.Contains(chunkPos))
                continue;

            if (!initialized)
            {
                initialized = true;
                targetChunk = GetOrCreateLatestChunk(chunkPos, resolution);
                combinedRasterClips = CombineRasterClipsForChunk(chunkPos, resolution);
            }

            if (chunkData.QueueProgress <= i)
                chunkData.IsDeleted = ApplyOperationToChunk(operation, affArea, combinedRasterClips, targetChunk!,
                    chunkPos, resolution, chunkData);
        }

        if (initialized)
        {
            if (lockTransparency && !chunkData.IsDeleted &&
                MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) is not null)
            {
                var committed = GetCommittedChunk(chunkPos, resolution);
                OperationHelper.ClampAlpha(targetChunk!.Surface, committed!.Surface);
            }

            chunkData.QueueProgress = queuedOperations.Count;
            latestChunksData[resolution][chunkPos] = chunkData;
        }

        if (combinedRasterClips.TryPickT2(out Chunk value, out var _))
            value.Dispose();
    }

    private OneOf<FilledChunk, EmptyChunk, Chunk> CombineRasterClipsForChunk(VecI chunkPos, ChunkResolution resolution)
    {
        if (lockTransparency && MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) is null)
        {
            return new EmptyChunk();
        }

        if (activeClips.Count == 0)
        {
            return new FilledChunk();
        }

        var intersection = Chunk.Create(ProcessingColorSpace, resolution);
        intersection.Surface.DrawingSurface.Canvas.Clear(Colors.White);

        foreach (var mask in activeClips)
        {
            if (mask.CommittedChunkExists(chunkPos))
            {
                mask.DrawCommittedChunkOn(chunkPos, resolution, intersection.Surface.DrawingSurface.Canvas, VecI.Zero,
                    ClippingPaint);
            }
            else
            {
                intersection.Dispose();
                return new EmptyChunk();
            }
        }

        return intersection;
    }

    /// <returns>
    /// True if the chunk was fully cleared (and should be deleted).
    /// </returns>
    private bool ApplyOperationToChunk(
        IOperation operation,
        AffectedArea operationAffectedArea,
        OneOf<FilledChunk, EmptyChunk, Chunk> combinedRasterClips,
        Chunk targetChunk,
        VecI chunkPos,
        ChunkResolution resolution,
        LatestChunkData chunkData)
    {
        if (operation is ClearOperation)
            return true;

        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();

        if (operation is IDrawOperation chunkOperation)
        {
            if (combinedRasterClips.IsT1) // Nothing is visible
                return chunkData.IsDeleted;

            if (chunkData.IsDeleted)
                targetChunk.Surface.DrawingSurface.Canvas.Clear();

            // just regular drawing
            if (combinedRasterClips.IsT0) // Everything is visible as far as the raster clips are concerned
            {
                CallDrawWithClip(chunkOperation, operationAffectedArea.GlobalArea, targetChunk, resolution, chunkPos);
                return false;
            }

            // drawing with raster clipping
            var clip = combinedRasterClips.AsT2;

            using var tempChunk = Chunk.Create(ProcessingColorSpace, targetChunk.Resolution);
            targetChunk.DrawChunkOn(tempChunk.Surface.DrawingSurface.Canvas, VecI.Zero, ReplacingPaint);

            CallDrawWithClip(chunkOperation, operationAffectedArea.GlobalArea, tempChunk, resolution, chunkPos);

            clip.DrawChunkOn(tempChunk.Surface.DrawingSurface.Canvas, VecI.Zero, ClippingPaint);
            clip.DrawChunkOn(targetChunk.Surface.DrawingSurface.Canvas, VecI.Zero, InverseClippingPaint);

            tempChunk.DrawChunkOn(targetChunk.Surface.DrawingSurface.Canvas, VecI.Zero, AddingPaint);
            return false;
        }

        if (operation is ResizeOperation resizeOperation)
        {
            return IsOutsideBounds(chunkPos, resizeOperation.Size);
        }

        return chunkData.IsDeleted;
    }

    private void CallDrawWithClip(IDrawOperation operation, RectI? operationAffectedArea, Chunk targetChunk,
        ChunkResolution resolution, VecI chunkPos)
    {
        if (operationAffectedArea is null)
            return;

        bool needsSrgbChunk = !targetChunk.ColorSpace.IsSrgb && operation.NeedsDrawInSrgb;

        using var srgbChunk = needsSrgbChunk
            ? Chunk.Create(ColorSpace.CreateSrgb(), resolution)
            : null;

        srgbChunk?.Surface.DrawingSurface.Canvas.DrawSurface(targetChunk.Surface.DrawingSurface, 0, 0, ReplacingPaint);

        var finalTarget = needsSrgbChunk ? srgbChunk : targetChunk;

        int count = finalTarget.Surface.DrawingSurface.Canvas.Save();

        float scale = (float)resolution.Multiplier();
        if (clippingPath is not null && !clippingPath.IsDisposed && !clippingPath.IsEmpty)
        {
            using VectorPath transformedPath = new(clippingPath);
            VecD trans = -chunkPos * FullChunkSize * scale;

            transformedPath.Transform(Matrix3X3.CreateScaleTranslation(scale, scale, (float)trans.X, (float)trans.Y));
            finalTarget.Surface.DrawingSurface.Canvas.ClipPath(transformedPath);
        }

        VecD affectedAreaPos = operationAffectedArea.Value.TopLeft;
        VecD affectedAreaSize = operationAffectedArea.Value.Size;
        affectedAreaPos = (affectedAreaPos - chunkPos * FullChunkSize) * scale;
        affectedAreaSize = affectedAreaSize * scale;
        finalTarget.Surface.DrawingSurface.Canvas.ClipRect(new RectD(affectedAreaPos, affectedAreaSize));

        operation.DrawOnChunk(finalTarget, chunkPos);

        targetChunk.Surface.DrawingSurface.Canvas.RestoreToCount(count);

        if (needsSrgbChunk)
        {
            targetChunk.Surface.DrawingSurface.Canvas.DrawSurface(srgbChunk!.Surface.DrawingSurface, 0, 0,
                ReplacingPaint);
        }
    }

    /// <summary>
    /// Finds and deletes empty committed chunks. Returns true if all existing chunks were deleted.
    /// Note: this function modifies the internal state, it is not thread safe! Use it only in changes (same as all the other functions that change the image in some way).
    /// </summary>
    /// <exception cref="ObjectDisposedException">This image is disposed</exception>
    public bool CheckIfCommittedIsEmpty()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException(
                    "This function can only be used when there are no queued operations");
            FindAndDeleteEmptyCommittedChunks();
            return committedChunks[ChunkResolution.Full].Count == 0;
        }
    }

    private HashSet<VecI> FindAllChunksOutsideBounds(VecI size)
    {
        var chunks = FindAllChunks();
        chunks.RemoveWhere(pos => !IsOutsideBounds(pos, size));
        return chunks;
    }

    private static bool IsOutsideBounds(VecI chunkPos, VecI imageSize)
    {
        return chunkPos.X < 0 || chunkPos.Y < 0 || chunkPos.X * FullChunkSize >= imageSize.X ||
               chunkPos.Y * FullChunkSize >= imageSize.Y;
    }

    private void FindAndDeleteEmptyCommittedChunks()
    {
        if (queuedOperations.Count != 0)
            throw new InvalidOperationException("This method cannot be used while any operations are queued");
        HashSet<VecI> toRemove = new();
        foreach (var (pos, chunk) in committedChunks[ChunkResolution.Full])
        {
            if (chunk.Surface.IsFullyTransparent())
            {
                toRemove.Add(pos);
                chunk.Dispose();
            }
        }

        foreach (var pos in toRemove)
        {
            committedChunks[ChunkResolution.Full].Remove(pos);
            committedChunks[ChunkResolution.Half].Remove(pos);
            committedChunks[ChunkResolution.Quarter].Remove(pos);
            committedChunks[ChunkResolution.Eighth].Remove(pos);
        }
    }

    /// <summary>
    /// Gets existing committed chunk or creates a new one. Doesn't apply any operations to the chunk, returns it as it is.
    /// </summary>
    private Chunk GetOrCreateCommittedChunk(VecI chunkPos, ChunkResolution resolution)
    {
        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        // committed chunk of the same resolution exists
        Chunk? targetChunk = MaybeGetCommittedChunk(chunkPos, resolution);
        if (targetChunk is not null)
            return targetChunk;

        // for full res chunks: nothing exists, create brand new chunk
        if (resolution == ChunkResolution.Full)
        {
            var newChunk = Chunk.Create(ProcessingColorSpace, resolution);
            committedChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }

        // for low res chunks: full res version exists
        Chunk? existingFullResChunk = MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full);
        if (existingFullResChunk is not null)
        {
            var newChunk = Chunk.Create(ProcessingColorSpace, resolution);
            newChunk.Surface.DrawingSurface.Canvas.Save();
            newChunk.Surface.DrawingSurface.Canvas.Scale((float)resolution.Multiplier());

            using var snapshot = existingFullResChunk.Surface.DrawingSurface.Snapshot();
            newChunk.Surface.DrawingSurface.Canvas.DrawImage(snapshot, 0, 0, SamplingOptions.Bilinear,
                SmoothReplacingPaint);
            newChunk.Surface.DrawingSurface.Canvas.Restore();
            committedChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }

        // for low res chunks: full res version doesn't exist
        {
            GetOrCreateCommittedChunk(chunkPos, ChunkResolution.Full);
            var newChunk = Chunk.Create(ProcessingColorSpace, resolution);
            committedChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }
    }

    /// <summary>
    /// Gets existing latest chunk or creates a new one, based on a committed one if it exists. Doesn't do any operations to the chunk.
    /// </summary>
    private Chunk GetOrCreateLatestChunk(VecI chunkPos, ChunkResolution resolution)
    {
        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        // latest chunk exists
        Chunk? targetChunk = MaybeGetLatestChunk(chunkPos, resolution);
        if (targetChunk is not null)
            return targetChunk;

        // committed chunk of the same resolution exists
        var maybeCommittedAnyRes = MaybeGetCommittedChunk(chunkPos, resolution);
        if (maybeCommittedAnyRes is not null)
        {
            Chunk newChunk = Chunk.Create(ProcessingColorSpace, resolution);
            if (blendMode == BlendMode.Src)
                maybeCommittedAnyRes.Surface.CopyTo(newChunk.Surface);
            else
                newChunk.Surface.DrawingSurface.Canvas.Clear();
            latestChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }

        // committed chunk of full resolution exists
        var maybeCommittedFullRes = MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full);
        if (maybeCommittedFullRes is not null)
        {
            //create low res committed chunk
            var committedChunkLowRes = GetOrCreateCommittedChunk(chunkPos, resolution);
            //create latest based on it
            Chunk newChunk = Chunk.Create(ProcessingColorSpace, resolution);
            committedChunkLowRes.Surface.CopyTo(newChunk.Surface);
            latestChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }

        // no previous chunks exist
        var newLatestChunk = Chunk.Create(ProcessingColorSpace, resolution);
        newLatestChunk.Surface.DrawingSurface.Canvas.Clear();
        latestChunks[resolution][chunkPos] = newLatestChunk;
        return newLatestChunk;
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(ChunkyImage));
    }

    public void Dispose()
    {
        lock (lockObject)
        {
            if (disposed)
                return;
            CancelChanges();
            DisposeAll();
            blendModePaint.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    private void DisposeAll()
    {
        foreach (var chunks in committedChunks.Values)
        {
            foreach (var chunk in chunks.Values)
            {
                chunk.Dispose();
            }
        }

        foreach (var chunks in latestChunks.Values)
        {
            foreach (var chunk in chunks.Values)
            {
                chunk.Dispose();
            }
        }

        disposed = true;
    }

    public object Clone()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            ChunkyImage clone = CloneFromLatest();
            return clone;
        }
    }

    public int GetCacheHash()
    {
        HashCode hash = new HashCode();
        hash.Add(commitCounter);
        hash.Add(queuedOperations.Count);
        hash.Add(operationCounter);

        foreach (var queuedOperation in queuedOperations)
        {
            hash.Add(queuedOperation.affectedArea.GlobalArea?.GetHashCode() ?? 0);
            hash.Add(queuedOperation.operation.GetHashCode());
        }

        hash.Add(activeClips.Count);
        hash.Add((int)blendMode);
        hash.Add(lockTransparency);
        if (horizontalSymmetryAxis is not null)
            hash.Add((int)(horizontalSymmetryAxis * 100));
        if (verticalSymmetryAxis is not null)
            hash.Add((int)(verticalSymmetryAxis * 100));
        if (clippingPath is not null)
            hash.Add(1);
        return hash.ToHashCode();
    }
}
