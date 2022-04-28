using System.Runtime.CompilerServices;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
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

    public Vector2i CommittedSize { get; private set; }
    public Vector2i LatestSize { get; private set; }

    private List<(IOperation operation, HashSet<Vector2i> affectedChunks)> queuedOperations = new();

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

    public ChunkyImage CloneFromLatest()
    {
        lock (lockObject)
        {
            ChunkyImage output = new(LatestSize);
            var chunks = FindAllChunks();
            foreach (var chunk in chunks)
            {
                var image = (Chunk?)GetLatestChunk(chunk, ChunkResolution.Full);
                if (image is not null)
                    output.DrawImage(chunk * ChunkSize, image.Surface);
            }
            output.CommitChanges();
            return output;
        }
    }

    public bool DrawLatestChunkOn(Vector2i chunkPos, ChunkResolution resolution, SKSurface surface, Vector2i pos, SKPaint? paint = null)
    {
        lock (lockObject)
        {
            var chunk = GetLatestChunk(chunkPos, resolution);
            if (chunk is null)
                return false;
            chunk.DrawOnSurface(surface, pos, paint);
            return true;
        }
    }

    public bool LatestChunkExists(Vector2i chunkPos, ChunkResolution resolution)
    {
        lock (lockObject)
        {
            return GetLatestChunk(chunkPos, resolution) is not null;
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
    /// Returns the latest version of the chunk, with uncommitted changes applied if they exist
    /// </summary>
    private Chunk? GetLatestChunk(Vector2i pos, ChunkResolution resolution)
    {
        //no queued operations
        if (queuedOperations.Count == 0)
        {
            var sameResChunk = MaybeGetCommittedChunk(pos, resolution);
            if (sameResChunk is not null)
                return sameResChunk;

            var fullResChunk = MaybeGetCommittedChunk(pos, ChunkResolution.Full);
            if (fullResChunk is not null)
                return GetOrCreateCommittedChunk(pos, resolution);

            return null;
        }

        // there are queued operations, target chunk is affected
        MaybeCreateAndProcessQueueForChunk(pos, resolution);
        var maybeNewlyProcessedChunk = MaybeGetLatestChunk(pos, resolution);
        if (maybeNewlyProcessedChunk is not null)
            return maybeNewlyProcessedChunk;

        // there are queued operations, target chunk is unaffected
        var maybeSameResCommitedChunk = MaybeGetCommittedChunk(pos, resolution);
        if (maybeSameResCommitedChunk is not null)
            return maybeSameResCommitedChunk;

        var maybeFullResCommitedChunk = MaybeGetCommittedChunk(pos, ChunkResolution.Full);
        if (maybeFullResCommitedChunk is not null)
            return GetOrCreateCommittedChunk(pos, resolution);

        return null;
    }

    /// <summary>
    /// Returns the committed version of the chunk ignoring any uncommitted changes
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

    public void DrawRectangle(ShapeData rect)
    {
        lock (lockObject)
        {
            RectangleOperation operation = new(rect);
            EnqueueOperation(operation);
        }
    }

    public void DrawImage(Vector2i pos, Surface image)
    {
        lock (lockObject)
        {
            ImageOperation operation = new(pos, image);
            EnqueueOperation(operation);
        }
    }

    public void ClearRegion(Vector2i pos, Vector2i size)
    {
        lock (lockObject)
        {
            ClearRegionOperation operation = new(pos, size);
            EnqueueOperation(operation);
        }
    }

    public void Clear()
    {
        lock (lockObject)
        {
            ClearOperation operation = new();
            EnqueueOperation(operation, FindAllChunks());
        }
    }

    public void ApplyRasterClip(ChunkyImage clippingMask)
    {
        lock (lockObject)
        {
            RasterClipOperation operation = new(clippingMask);
            EnqueueOperation(operation, new());
        }
    }

    public void Resize(Vector2i newSize)
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

            commitCounter++;
            if (commitCounter % 30 == 0)
                FindAndDeleteEmptyCommittedChunks();
        }
    }

    private void CommitLatestChunks()
    {
        // move fully processed latest chunks to committed
        foreach (var (resolution, chunks) in latestChunks)
        {
            foreach (var (pos, chunk) in chunks)
            {
                if (committedChunks[resolution].ContainsKey(pos))
                {
                    var oldChunk = committedChunks[resolution][pos];
                    committedChunks[resolution].Remove(pos);
                    oldChunk.Dispose();
                }

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

                if (!data.IsDeleted)
                    committedChunks[resolution].Add(pos, chunk);
                else
                    chunk.Dispose();
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

    /// <summary>
    /// Returns all chunks that have something in them, including latest (uncommitted) ones
    /// </summary>
    public HashSet<Vector2i> FindAllChunks()
    {
        lock (lockObject)
        {
            var allChunks = committedChunks[ChunkResolution.Full].Select(chunk => chunk.Key).ToHashSet();
            foreach (var (operation, opChunks) in queuedOperations)
            {
                allChunks.UnionWith(opChunks);
            }
            return allChunks;
        }
    }

    /// <summary>
    /// Returns chunks affected by operations that haven't been committed yet
    /// </summary>
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

    private void MaybeCreateAndProcessQueueForChunk(Vector2i chunkPos, ChunkResolution resolution)
    {
        if (!latestChunksData[resolution].TryGetValue(chunkPos, out LatestChunkData chunkData))
            chunkData = new() { QueueProgress = 0, IsDeleted = !committedChunks[ChunkResolution.Full].ContainsKey(chunkPos) };
        if (chunkData.QueueProgress == queuedOperations.Count)
            return;

        Chunk? targetChunk = null;
        List<ChunkyImage> activeClips = new();
        bool isFullyMaskedOut = false;
        bool somethingWasApplied = false;
        for (int i = 0; i < queuedOperations.Count; i++)
        {
            var (operation, operChunks) = queuedOperations[i];
            if (operation is RasterClipOperation clipOperation)
            {
                // handle self-clipping as a special case to avoid deadlock
                bool clippingChunkExists = ReferenceEquals(this, clipOperation.ClippingMask) ?
                    MaybeGetCommittedChunk(chunkPos, ChunkResolution.Full) != null :
                    clipOperation.ClippingMask.CommitedChunkExists(chunkPos, resolution);
                if (clippingChunkExists)
                    activeClips.Add(clipOperation.ClippingMask);
                else
                    isFullyMaskedOut = true;
            }

            if (!operChunks.Contains(chunkPos))
                continue;
            if (!somethingWasApplied)
            {
                somethingWasApplied = true;
                targetChunk = GetOrCreateLatestChunk(chunkPos, resolution);
            }

            if (chunkData.QueueProgress <= i)
                chunkData.IsDeleted = ApplyOperationToChunk(operation, activeClips, isFullyMaskedOut, targetChunk!, chunkPos, resolution, chunkData);
        }

        if (somethingWasApplied)
        {
            chunkData.QueueProgress = queuedOperations.Count;
            latestChunksData[resolution][chunkPos] = chunkData;
        }
    }

    /// <summary>
    /// Returns true if the chunk was fully cleared (and should be deleted)
    /// </summary>
    private bool ApplyOperationToChunk(
        IOperation operation,
        IReadOnlyList<ChunkyImage> activeClips,
        bool isFullyMaskedOut,
        Chunk targetChunk,
        Vector2i chunkPos,
        ChunkResolution resolution,
        LatestChunkData chunkData)
    {
        if (operation is ClearOperation)
            return true;

        if (operation is IDrawOperation chunkOperation)
        {
            if (isFullyMaskedOut)
                return chunkData.IsDeleted;

            if (chunkData.IsDeleted)
                targetChunk.Surface.SkiaSurface.Canvas.Clear();

            // just regular drawing
            if (activeClips.Count == 0)
            {
                chunkOperation.DrawOnChunk(targetChunk, chunkPos);
                return false;
            }

            // drawing with clipping
            using var tempChunk = Chunk.Create(targetChunk.Resolution);
            targetChunk.DrawOnSurface(tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint);
            chunkOperation.DrawOnChunk(tempChunk, chunkPos);

            if (activeClips.Count > 1)
            {
                using var intersection = IntersectMasks(chunkPos, activeClips, resolution);
                intersection.DrawOnSurface(tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                intersection.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), InverseClippingPaint);
            }
            else
            {
                if (!ReferenceEquals(this, activeClips[0]))
                {
                    activeClips[0].DrawCommittedChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                    activeClips[0].DrawCommittedChunkOn(chunkPos, resolution, targetChunk.Surface.SkiaSurface, new(0, 0), InverseClippingPaint);
                }
                else
                {
                    var maskChunk = GetCommittedChunk(chunkPos, resolution);
                    if (maskChunk is null)
                        return true; // this should never happen, there is a check in MaybeCreateAndProcessQueueForChunk
                    maskChunk.DrawOnSurface(tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                    maskChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), InverseClippingPaint);
                }
            }

            tempChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), AddingPaint);
            return false;
        }

        if (operation is ResizeOperation resizeOperation)
        {
            return IsOutsideBounds(chunkPos, resizeOperation.Size);
        }
        return chunkData.IsDeleted;
    }

    private Chunk IntersectMasks(Vector2i chunkPos, IReadOnlyList<ChunkyImage> activeClips, ChunkResolution resolution)
    {
        var maskIntersection = Chunk.Create(resolution);
        maskIntersection.Surface.SkiaSurface.Canvas.Clear(SKColors.White);

        foreach (var mask in activeClips)
        {
            // handle self-clipping as a special case to avoid deadlock
            if (!ReferenceEquals(this, mask))
            {
                mask.DrawCommittedChunkOn(chunkPos, resolution, maskIntersection.Surface.SkiaSurface, new(0, 0), ClippingPaint);
            }
            else
            {
                var maskChunk = GetCommittedChunk(chunkPos, resolution);
                if (maskChunk is null)
                {
                    maskIntersection.Surface.SkiaSurface.Canvas.Clear();
                    return maskIntersection;
                }
                maskChunk.DrawOnSurface(maskIntersection.Surface.SkiaSurface, new(0, 0), ClippingPaint);
            }
        }
        return maskIntersection;
    }

    /// <summary>
    /// Note: this function modifies the internal state, it is not thread safe! (same as all the other functions that change the image in some way)
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
            maybeCommittedAnyRes.Surface.CopyTo(newChunk.Surface);
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
