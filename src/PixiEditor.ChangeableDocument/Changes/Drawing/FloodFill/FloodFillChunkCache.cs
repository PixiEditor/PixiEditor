using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;

internal class FloodFillChunkCache : IDisposable
{
    private Paint ReplacingPaint { get; } = new Paint() { BlendMode = BlendMode.Src };

    private readonly HashSet<Guid>? membersToRender;
    private readonly IReadOnlyDocument? document;
    private readonly IReadOnlyChunkyImage? image;
    private readonly int frame;

    private readonly Dictionary<VecI, OneOf<Chunk, EmptyChunk>> acquiredChunks = new();
    
    private ColorSpace processingColorSpace = ColorSpace.CreateSrgbLinear();

    public FloodFillChunkCache(IReadOnlyChunkyImage image)
    {
        this.image = image;
        this.processingColorSpace = image.ProcessingColorSpace;
    }

    public FloodFillChunkCache(HashSet<Guid> membersToRender, IReadOnlyDocument document, int frame)
    {
        this.membersToRender = membersToRender;
        this.document = document;
        this.frame = frame;
        processingColorSpace = document.ProcessingColorSpace;
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
        if (acquiredChunks.TryGetValue(pos, out var foundChunk))
            return foundChunk;

        // need to get the chunk by merging multiple members
        if (image is null)
        {
            if (document is null || membersToRender is null)
                throw new InvalidOperationException();
            Chunk chunk = Chunk.Create(processingColorSpace);
            chunk.Surface.DrawingSurface.Canvas.Save();
            
            VecI chunkPos = pos * ChunkyImage.FullChunkSize;
            
            chunk.Surface.DrawingSurface.Canvas.Translate(-chunkPos.X, -chunkPos.Y);
            
            document.Renderer.RenderLayers(chunk.Surface.DrawingSurface, membersToRender, frame, ChunkResolution.Full, chunk.Surface.Size);
            
            chunk.Surface.DrawingSurface.Canvas.Restore();
            
            acquiredChunks[pos] = chunk;
            return chunk;
        }

        // there is only a single image, just get the chunk from it
        if (!image.LatestOrCommittedChunkExists(pos))
            return new EmptyChunk();
        Chunk chunkOnImage = Chunk.Create(processingColorSpace, ChunkResolution.Full);

        if (!image.DrawMostUpToDateChunkOn(pos, ChunkResolution.Full, chunkOnImage.Surface.DrawingSurface, VecI.Zero, ReplacingPaint))
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
