using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class ReplaceColorOperation : IDrawOperation
{
    private readonly Color oldColor;
    private readonly Color newColor;

    private readonly ColorBounds oldColorBounds;
    private readonly ulong newColorBits;

    public bool IgnoreEmptyChunks => true;
    public bool NeedsDrawInSrgb => false;

    public ReplaceColorOperation(Color oldColor, Color newColor)
    {
        this.oldColor = oldColor;
        this.newColor = newColor;
        oldColorBounds = new ColorBounds(oldColor);
        newColorBits = newColor.ToULong();
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        ulong targetColorBits = newColor.ToULong();
        ColorBounds colorBounds = new(oldColor);
        if (targetChunk.Surface.ImageInfo.ColorSpace is { IsSrgb: false })
        {
            var transform = ColorSpace.CreateSrgb().GetTransformFunction();
            targetColorBits = newColor.TransformColor(transform).ToULong();

            var transformOld = targetChunk.Surface.ImageInfo.ColorSpace.GetTransformFunction();
            colorBounds = new ColorBounds((Color)oldColor.TransformColor(transform));
        }

        ReplaceColor(colorBounds, targetColorBits, targetChunk);
    }

    private static unsafe void ReplaceColor(ColorBounds oldColorBounds, ulong newColorBits, Chunk chunk)
    {
        int maxThreads = Environment.ProcessorCount;
        VecI imageSize = chunk.PixelSize;
        int rowsPerThread = imageSize.Y / maxThreads;

        using Pixmap pixmap = chunk.Surface.DrawingSurface.PeekPixels();
        IntPtr pixels = pixmap.GetPixels();

        Half* endOffset = (Half*)(pixels + pixmap.BytesSize);
        for (Half* i = (Half*)pixels; i < endOffset; i += 4)
        {
            if (oldColorBounds.IsWithinBounds(i))
            {
                *(ulong*)i = newColorBits;
            }
        }
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        RectI rect = new(VecI.Zero, imageSize);
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(rect, ChunkyImage.FullChunkSize), rect);
    }

    public void Dispose()
    {
    }
}
