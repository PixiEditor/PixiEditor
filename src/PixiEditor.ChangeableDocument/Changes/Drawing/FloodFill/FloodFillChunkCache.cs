using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;

internal class FloodFillChunkCache : IDisposable
{
    private SKPaint ReplacingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Src };

    private readonly HashSet<Guid>? membersToRender;
    private readonly IReadOnlyFolder? structureRoot;
    private readonly IReadOnlyChunkyImage? image;

    private readonly Dictionary<VecI, OneOf<Chunk, EmptyChunk>> acquiredChunks = new();

    public FloodFillChunkCache(IReadOnlyChunkyImage image)
    {
        this.image = image;
    }

    public FloodFillChunkCache(HashSet<Guid> membersToRender, IReadOnlyFolder structureRoot)
    {
        this.membersToRender = membersToRender;
        this.structureRoot = structureRoot;
    }

    public bool ChunkExistsInStorage(VecI pos)
    {
        if (image is not null)
            return acquiredChunks.ContainsKey(pos);
        return true;
    }

    public OneOf<Chunk, EmptyChunk> GetChunk(VecI pos)
    {
        // the chunk was already acquired before, return cached
        if (acquiredChunks.ContainsKey(pos))
            return acquiredChunks[pos];

        // need to get the chunk by merging multiple members
        if (image is null)
        {
            if (structureRoot is null || membersToRender is null)
                throw new InvalidOperationException();
            var chunk = ChunkRenderer.MergeChosenMembers(pos, ChunkResolution.Full, structureRoot, membersToRender);
            acquiredChunks[pos] = chunk;
            return chunk;
        }

        // there is only a single image, just get the chunk from it
        if (!image.LatestOrCommittedChunkExists(pos))
            return new EmptyChunk();
        Chunk chunkOnImage = Chunk.Create(ChunkResolution.Full);

        if (!image.DrawMostUpToDateChunkOn(pos, ChunkResolution.Full, chunkOnImage.Surface.SkiaSurface, VecI.Zero, ReplacingPaint))
        {
            chunkOnImage.Dispose();
            acquiredChunks[pos] = new EmptyChunk();
            return new EmptyChunk();
        }
        acquiredChunks[pos] = chunkOnImage;
        return chunkOnImage;
    }

    public void Dispose()
    {
        foreach (var chunk in acquiredChunks.Values.Where(static chunk => chunk.IsT0))
            chunk.AsT0.Dispose();
        ReplacingPaint.Dispose();
    }
}
