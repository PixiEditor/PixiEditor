using System.Runtime.CompilerServices;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using OneOf;
using OneOf.Types;
using SkiaSharp;

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
///     - LatestSize contains the new size if any resize operations were requested, otherwise the commited size
/// You can check the current state via queuedOperations.Count == 0
/// </summary>
public class ChunkyImage : IReadOnlyChunkyImage, IDisposable
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
    private object lockObject = new();
    private int commitCounter = 0;

    public static int ChunkSize => ChunkPool.FullChunkSize;
    private static SKPaint ClippingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.DstIn };
    private static SKPaint InverseClippingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.DstOut };
    private static SKPaint ReplacingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Src };
    private static SKPaint AddingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Plus };
    private SKPaint blendModePaint = new SKPaint() { BlendMode = SKBlendMode.Src };

    public Vector2i CommittedSize { get; private set; }
    public Vector2i LatestSize { get; private set; }

    private List<(IOperation operation, HashSet<Vector2i> affectedChunks)> queuedOperations = new();
    private List<ChunkyImage> activeClips = new();
    private SKBlendMode blendMode = SKBlendMode.Src;
    private bool lockTransparency = false;

    private Dictionary<ChunkResolution, Dictionary<Vector2i, Chunk>> committedChunks;
    private Dictionary<ChunkResolution, Dictionary<Vector2i, Chunk>> latestChunks;
    private Dictionary<ChunkResolution, Dictionary<Vector2i, LatestChunkData>> latestChunksData = new();

    public ChunkyImage(Vector2i size)
    {
        CommittedSize = size;
        LatestSize = size;
        committedChunks = new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new(),
        };
        latestChunks = new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new(),
        };
        latestChunksData = new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new(),
        };
    }

    public ChunkyImage CloneFromCommitted()
    {
        lock (lockObject)
        {
            ChunkyImage output = new(LatestSize);
            var chunks = FindCommittedChunks();
            foreach (var chunk in chunks)
            {
                var image = GetCommittedChunk(chunk, ChunkResolution.Full);
                if (image is null)
                    continue;
                output.EnqueueDrawImage(chunk * ChunkSize, image.Surface);
            }
            output.CommitChanges();
            return output;
        }
    }

    /// <returns>
    /// True if the chunk existed and was drawn, otherwise false
    /// </returns>
    public bool DrawMostUpToDateChunkOn(Vector2i chunkPos, ChunkResolution resolution, SKSurface surface, Vector2i pos, SKPaint? paint = null)
    {
        lock (lockObject)
        {
            var latestChunk = GetLatestChunk(chunkPos, resolution);
            var committedChunk = GetCommittedChunk(chunkPos, resolution);

            // latest chunk does not exist, draw committed if it exists
            if (latestChunk is null)
            {
                if (committedChunk is null)
                    return false;
                committedChunk.DrawOnSurface(surface, pos, paint);
                return true;
            }

            // latest chunk exists
            if (blendMode == SKBlendMode.Src || committedChunk is null)
            {
                // no need to combine with committed, draw directly
                latestChunk.DrawOnSurface(surface, pos, paint);
                return true;
            }

            // combine with committed and then draw
            using var tempChunk = Chunk.Create(resolution);
            tempChunk.Surface.SkiaSurface.Canvas.DrawSurface(committedChunk!.Surface.SkiaSurface, 0, 0, ReplacingPaint);
            blendModePaint.BlendMode = blendMode;
            tempChunk.Surface.SkiaSurface.Canvas.DrawSurface(latestChunk.Surface.SkiaSurface, 0, 0, blendModePaint);
            if (lockTransparency)
                ClampAlpha(tempChunk.Surface.SkiaSurface, committedChunk.Surface.SkiaSurface);
            tempChunk.DrawOnSurface(surface, pos, paint);

            return true;
        }
    }

    public bool LatestOrCommittedChunkExists(Vector2i chunkPos)
    {
        lock (lockObject)
        {
            return (
                MaybeGetLatestChunk(chunkPos, ChunkResolution.Full) ??
                MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full)
                ) is not null;
        }
    }

    internal bool DrawCommittedChunkOn(Vector2i chunkPos, ChunkResolution resolution, SKSurface surface, Vector2i pos, SKPaint? paint = null)
    {
        lock (lockObject)
        {
            var chunk = GetCommittedChunk(chunkPos, resolution);
            if (chunk is null)
                return false;
            chunk.DrawOnSurface(surface, pos, paint);
            return true;
        }
    }

    internal bool CommitedChunkExists(Vector2i chunkPos, ChunkResolution resolution)
    {
        lock (lockObject)
        {
            return GetCommittedChunk(chunkPos, resolution) is not null;
        }
    }

    /// <summary>
    /// Returns the latest version of the chunk if it exists or should exist based on queued operation. The returned chunk is fully up to date.
    /// </summary>
    private Chunk? GetLatestChunk(Vector2i pos, ChunkResolution resolution)
    {
        if (queuedOperations.Count == 0)
            return null;

        MaybeCreateAndProcessQueueForChunk(pos, resolution);
        var maybeNewlyProcessedChunk = MaybeGetLatestChunk(pos, resolution);
        return maybeNewlyProcessedChunk;
    }

    /// <summary>
    /// Tries it's best to return a committed chunk, either if it exists or if it can be created from it's high res version. Returns null if it can't.
    /// </summary>
    private Chunk? GetCommittedChunk(Vector2i pos, ChunkResolution resolution)
    {
        var maybeSameRes = MaybeGetCommittedChunk(pos, resolution);
        if (maybeSameRes is not null)
            return maybeSameRes;

        var maybeFullRes = MaybeGetCommittedChunk(pos, ChunkResolution.Full);
        if (maybeFullRes is not null)
            return GetOrCreateCommittedChunk(pos, resolution);

        return null;
    }

    private Chunk? MaybeGetLatestChunk(Vector2i pos, ChunkResolution resolution) => latestChunks[resolution].TryGetValue(pos, out Chunk? value) ? value : null;
    private Chunk? MaybeGetCommittedChunk(Vector2i pos, ChunkResolution resolution) => committedChunks[resolution].TryGetValue(pos, out Chunk? value) ? value : null;

    public void AddRasterClip(ChunkyImage clippingMask)
    {
        lock (lockObject)
        {
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException("This function can only be executed when there are no queued operations");
            activeClips.Add(clippingMask);
        }
    }

    /// <summary>
    /// Don't pass in porter duff compositing operators (apart from SrcOver) as they won't have the intended effect.
    /// </summary>
    public void SetBlendMode(SKBlendMode mode)
    {
        lock (lockObject)
        {
            if (queuedOperations.Count > 0)
                throw new InvalidOperationException("This function can only be executed when there are no queued operations");
            blendMode = mode;
        }
    }

    public void EnableLockTransparency()
    {
        lock (lockObject)
        {
            lockTransparency = true;
        }
    }

    public void EnqueueDrawRectangle(ShapeData rect)
    {
        lock (lockObject)
        {
            RectangleOperation operation = new(rect);
            EnqueueOperation(operation);
        }
    }

    public void EnqueueDrawImage(Vector2i pos, Surface image)
    {
        lock (lockObject)
        {
            ImageOperation operation = new(pos, image);
            EnqueueOperation(operation);
        }
    }

    public void EnqueueClearRegion(Vector2i pos, Vector2i size)
    {
        lock (lockObject)
        {
            ClearRegionOperation operation = new(pos, size);
            EnqueueOperation(operation);
        }
    }

    public void EnqueueClear()
    {
        lock (lockObject)
        {
            ClearOperation operation = new();
            EnqueueOperation(operation, FindAllChunks());
        }
    }

    public void EnqueueResize(Vector2i newSize)
    {
        lock (lockObject)
        {
            ResizeOperation operation = new(newSize);
            LatestSize = newSize;
            EnqueueOperation(operation, FindAllChunksOutsideBounds(newSize));
        }
    }

    private void EnqueueOperation(IDrawOperation operation)
    {
        var chunks = operation.FindAffectedChunks();
        chunks.RemoveWhere(pos => IsOutsideBounds(pos, LatestSize));
        if (operation.IgnoreEmptyChunks)
            chunks.IntersectWith(FindAllChunks());
        EnqueueOperation(operation, chunks);
    }
    private void EnqueueOperation(IOperation operation, HashSet<Vector2i> chunks)
    {
        queuedOperations.Add((operation, chunks));
    }

    public void CancelChanges()
    {
        lock (lockObject)
        {
            //clear queued operations
            foreach (var operation in queuedOperations)
                operation.Item1.Dispose();
            queuedOperations.Clear();

            //clear additional state
            activeClips.Clear();
            blendMode = SKBlendMode.Src;
            lockTransparency = false;

            //clear latest chunks
            foreach (var (_, chunksOfRes) in latestChunks)
            {
                foreach (var (_, chunk) in chunksOfRes)
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

    public void CommitChanges()
    {
        lock (lockObject)
        {
            var affectedChunks = FindAffectedChunks();
            foreach (var chunk in affectedChunks)
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
            blendMode = SKBlendMode.Src;
            lockTransparency = false;

            commitCounter++;
            if (commitCounter % 30 == 0)
                FindAndDeleteEmptyCommittedChunks();
        }
    }

    /// <summary>
    /// Does all necessery steps to convert latest chunks into committed ones. The latest chunk dictionary become empty after this function is called.
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
                        throw new InvalidOperationException("Trying to commit a full res chunk that wasn't fully processed");
                    }
                    else
                    {
                        chunk.Dispose();
                        continue;
                    }
                }

                // do a swap
                if (blendMode == SKBlendMode.Src)
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
                        committedChunks[resolution].Add(pos, chunk);
                        continue;
                    }

                    //blend
                    blendModePaint.BlendMode = blendMode;
                    if (lockTransparency)
                    {
                        using Chunk tempChunk = Chunk.Create(resolution);
                        tempChunk.Surface.SkiaSurface.Canvas.DrawSurface(maybeCommitted.Surface.SkiaSurface, 0, 0, ReplacingPaint);
                        maybeCommitted.Surface.SkiaSurface.Canvas.DrawSurface(chunk.Surface.SkiaSurface, 0, 0, blendModePaint);
                        ClampAlpha(maybeCommitted.Surface.SkiaSurface, tempChunk.Surface.SkiaSurface);
                    }
                    else
                    {
                        maybeCommitted.Surface.SkiaSurface.Canvas.DrawSurface(chunk.Surface.SkiaSurface, 0, 0, blendModePaint);
                    }
                    chunk.Dispose();
                }
            }
        }

        // delete committed low res chunks that weren't updated
        foreach (var (pos, chunk) in latestChunks[ChunkResolution.Full])
        {
            foreach (var (resolution, _) in latestChunks)
            {
                if (resolution == ChunkResolution.Full)
                    continue;
                if (!latestChunksData[resolution].TryGetValue(pos, out var halfChunk) || halfChunk.QueueProgress != queuedOperations.Count)
                {
                    if (committedChunks[resolution].TryGetValue(pos, out var commitedLowResChunk))
                    {
                        committedChunks[resolution].Remove(pos);
                        commitedLowResChunk.Dispose();
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
    /// All chunks that have something in them, including latest (uncommitted) ones
    /// </returns>
    public HashSet<Vector2i> FindAllChunks()
    {
        lock (lockObject)
        {
            var allChunks = committedChunks[ChunkResolution.Full].Select(chunk => chunk.Key).ToHashSet();
            foreach (var (_, opChunks) in queuedOperations)
            {
                allChunks.UnionWith(opChunks);
            }
            return allChunks;
        }
    }

    public HashSet<Vector2i> FindCommittedChunks()
    {
        lock (lockObject)
        {
            return committedChunks[ChunkResolution.Full].Select(chunk => chunk.Key).ToHashSet();
        }
    }

    /// <returns>
    /// Chunks affected by operations that haven't been committed yet
    /// </returns>
    public HashSet<Vector2i> FindAffectedChunks()
    {
        lock (lockObject)
        {
            var chunks = new HashSet<Vector2i>();
            foreach (var (_, opChunks) in queuedOperations)
            {
                chunks.UnionWith(opChunks);
            }
            return chunks;
        }
    }

    /// <summary>
    /// Applies all operations queued for a specific (latest) chunk. If the latest chunk doesn't exist yet, creates it. If none of the existing operations affect the chunk does nothing.
    /// </summary>
    private void MaybeCreateAndProcessQueueForChunk(Vector2i chunkPos, ChunkResolution resolution)
    {
        if (!latestChunksData[resolution].TryGetValue(chunkPos, out LatestChunkData chunkData))
            chunkData = new() { QueueProgress = 0, IsDeleted = !committedChunks[ChunkResolution.Full].ContainsKey(chunkPos) };
        if (chunkData.QueueProgress == queuedOperations.Count)
            return;

        Chunk? targetChunk = null;
        OneOf<All, None, Chunk> combinedClips = new All();

        bool initialized = false;

        for (int i = 0; i < queuedOperations.Count; i++)
        {
            var (operation, operChunks) = queuedOperations[i];
            if (!operChunks.Contains(chunkPos))
                continue;

            if (!initialized)
            {
                initialized = true;
                targetChunk = GetOrCreateLatestChunk(chunkPos, resolution);
                combinedClips = CombineClipsForChunk(chunkPos, resolution);
            }

            if (chunkData.QueueProgress <= i)
                chunkData.IsDeleted = ApplyOperationToChunk(operation, combinedClips, targetChunk!, chunkPos, resolution, chunkData);
        }

        if (initialized)
        {
            if (lockTransparency && !chunkData.IsDeleted && MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) is not null)
            {
                var committed = GetCommittedChunk(chunkPos, resolution);
                ClampAlpha(targetChunk!.Surface.SkiaSurface, committed!.Surface.SkiaSurface);
            }

            chunkData.QueueProgress = queuedOperations.Count;
            latestChunksData[resolution][chunkPos] = chunkData;
        }

        if (combinedClips.TryPickT2(out Chunk value, out var _))
            value.Dispose();
    }

    /// <summary>
    /// (All) -> All is visible, (None) -> None is visible, (Chunk) -> Combined clip
    /// </summary>
    private OneOf<All, None, Chunk> CombineClipsForChunk(Vector2i chunkPos, ChunkResolution resolution)
    {
        if (lockTransparency && MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) is null)
        {
            return new None();
        }
        if (activeClips.Count == 0)
        {
            return new All();
        }


        var intersection = Chunk.Create(resolution);
        intersection.Surface.SkiaSurface.Canvas.Clear(SKColors.White);

        foreach (var mask in activeClips)
        {
            // handle self-clipping as a special case to avoid deadlock
            if (!ReferenceEquals(this, mask))
            {
                if (mask.CommitedChunkExists(chunkPos, resolution))
                {
                    mask.DrawCommittedChunkOn(chunkPos, resolution, intersection.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                }
                else
                {
                    intersection.Dispose();
                    return new None();
                }
            }
            else
            {
                var maskChunk = GetCommittedChunk(chunkPos, resolution);
                if (maskChunk is null)
                {
                    intersection.Dispose();
                    return new None();
                }
                maskChunk.DrawOnSurface(intersection.Surface.SkiaSurface, new(0, 0), ClippingPaint);
            }
        }
        return intersection;
    }

    /// <summary>
    /// toModify[x,y].Alpha = Math.Min(toModify[x,y].Alpha, toGetAlphaFrom[x,y].Alpha)
    /// </summary>
    private unsafe void ClampAlpha(SKSurface toModify, SKSurface toGetAlphaFrom)
    {
        using (var map = toModify.PeekPixels())
        {
            using (var refMap = toGetAlphaFrom.PeekPixels())
            {
                long* pixels = (long*)map.GetPixels();
                long* refPixels = (long*)refMap.GetPixels();
                int size = map.Width * map.Height;
                if (map.Width != refMap.Width || map.Height != refMap.Height)
                    throw new ArgumentException("The surfaces must have the same size");

                for (int i = 0; i < size; i++)
                {
                    long* offset = pixels + i;
                    long* refOffset = refPixels + i;
                    Half* alpha = (Half*)offset + 3;
                    Half* refAlpha = (Half*)refOffset + 3;
                    if (*refAlpha < *alpha)
                    {
                        float a = (float)(*alpha);
                        float r = (float)(*((Half*)offset)) / a;
                        float g = (float)(*((Half*)offset + 1)) / a;
                        float b = (float)(*((Half*)offset + 2)) / a;
                        float newA = (float)(*refAlpha);
                        Half newR = (Half)(r * newA);
                        Half newG = (Half)(g * newA);
                        Half newB = (Half)(b * newA);
                        *offset = ((long)*(ushort*)(&newR)) | ((long)*(ushort*)(&newG)) << 16 | ((long)*(ushort*)(&newB)) << 32 | ((long)*(ushort*)(refAlpha)) << 48;
                    }
                }
            }
        }
    }

    /// <returns>
    /// True if the chunk was fully cleared (and should be deleted).
    /// </returns>
    private bool ApplyOperationToChunk(
        IOperation operation,
        OneOf<All, None, Chunk> combinedClips,
        Chunk targetChunk,
        Vector2i chunkPos,
        ChunkResolution resolution,
        LatestChunkData chunkData)
    {
        if (operation is ClearOperation)
            return true;

        if (operation is IDrawOperation chunkOperation)
        {
            if (combinedClips.IsT1) //None is visible
                return chunkData.IsDeleted;

            if (chunkData.IsDeleted)
                targetChunk.Surface.SkiaSurface.Canvas.Clear();

            // just regular drawing
            if (combinedClips.IsT0) //All is visible
            {
                chunkOperation.DrawOnChunk(targetChunk, chunkPos);
                return false;
            }

            // drawing with clipping
            var clip = combinedClips.AsT2;

            using var tempChunk = Chunk.Create(targetChunk.Resolution);
            targetChunk.DrawOnSurface(tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint);

            chunkOperation.DrawOnChunk(tempChunk, chunkPos);

            clip.DrawOnSurface(tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
            clip.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), InverseClippingPaint);

            tempChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), AddingPaint);
            return false;
        }

        if (operation is ResizeOperation resizeOperation)
        {
            return IsOutsideBounds(chunkPos, resizeOperation.Size);
        }
        return chunkData.IsDeleted;
    }

    /// <summary>
    /// Finds and deletes empty committed chunks. Returns true if all existing chunks were deleted.
    /// Note: this function modifies the internal state, it is not thread safe! Use it only in changes (same as all the other functions that change the image in some way).
    /// </summary>
    public bool CheckIfCommittedIsEmpty()
    {
        lock (lockObject)
        {
            FindAndDeleteEmptyCommittedChunks();
            return committedChunks[ChunkResolution.Full].Count == 0;
        }
    }

    private HashSet<Vector2i> FindAllChunksOutsideBounds(Vector2i size)
    {
        var chunks = FindAllChunks();
        chunks.RemoveWhere(pos => !IsOutsideBounds(pos, size));
        return chunks;
    }

    private static bool IsOutsideBounds(Vector2i chunkPos, Vector2i imageSize)
    {
        return chunkPos.X < 0 || chunkPos.Y < 0 || chunkPos.X * ChunkSize >= imageSize.X || chunkPos.Y * ChunkSize >= imageSize.Y;
    }

    private void FindAndDeleteEmptyCommittedChunks()
    {
        if (queuedOperations.Count != 0)
            throw new InvalidOperationException("This method cannot be used while any operations are queued");
        HashSet<Vector2i> toRemove = new();
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
    private Chunk GetOrCreateCommittedChunk(Vector2i chunkPos, ChunkResolution resolution)
    {
        // commited chunk of the same resolution exists
        Chunk? targetChunk = MaybeGetCommittedChunk(chunkPos, resolution);
        if (targetChunk is not null)
            return targetChunk;

        // for full res chunks: nothing exists, create brand new chunk
        if (resolution == ChunkResolution.Full)
        {
            var newChunk = Chunk.Create(resolution);
            committedChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }

        // for low res chunks: full res version exists
        Chunk? existingFullResChunk = MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full);
        if (resolution != ChunkResolution.Full && existingFullResChunk is not null)
        {
            var newChunk = Chunk.Create(resolution);
            newChunk.Surface.SkiaSurface.Canvas.Save();
            newChunk.Surface.SkiaSurface.Canvas.Scale((float)resolution.Multiplier());

            newChunk.Surface.SkiaSurface.Canvas.DrawSurface(existingFullResChunk!.Surface.SkiaSurface, 0, 0, ReplacingPaint);
            newChunk.Surface.SkiaSurface.Canvas.Restore();
            committedChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }

        // for low res chunks: full res version doesn't exist
        {
            GetOrCreateCommittedChunk(chunkPos, ChunkResolution.Full);
            var newChunk = Chunk.Create(resolution);
            committedChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }
    }

    /// <summary>
    /// Gets existing latest chunk or creates a new one, based on a committed one if it exists. Doesn't do any operations to the chunk.
    /// </summary>
    private Chunk GetOrCreateLatestChunk(Vector2i chunkPos, ChunkResolution resolution)
    {
        // latest chunk exists
        Chunk? targetChunk;
        targetChunk = MaybeGetLatestChunk(chunkPos, resolution);
        if (targetChunk is not null)
            return targetChunk;

        // committed chunk of the same resolution exists
        var maybeCommittedAnyRes = MaybeGetCommittedChunk(chunkPos, resolution);
        if (maybeCommittedAnyRes is not null)
        {
            Chunk newChunk = Chunk.Create(resolution);
            if (blendMode == SKBlendMode.Src)
                maybeCommittedAnyRes.Surface.CopyTo(newChunk.Surface);
            else
                newChunk.Surface.SkiaSurface.Canvas.Clear();
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
            Chunk newChunk = Chunk.Create(resolution);
            committedChunkLowRes.Surface.CopyTo(newChunk.Surface);
            latestChunks[resolution][chunkPos] = newChunk;
            return newChunk;
        }

        // no previous chunks exist
        var newLatestChunk = Chunk.Create(resolution);
        newLatestChunk.Surface.SkiaSurface.Canvas.Clear();
        latestChunks[resolution][chunkPos] = newLatestChunk;
        return newLatestChunk;
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
        foreach (var (_, chunks) in committedChunks)
        {
            foreach (var (_, chunk) in chunks)
            {
                chunk.Dispose();
            }
        }
        foreach (var (_, chunks) in latestChunks)
        {
            foreach (var (_, chunk) in chunks)
            {
                chunk.Dispose();
            }
        }
        disposed = true;
    }
    ~ChunkyImage()
    {
        DisposeAll();
    }
}
