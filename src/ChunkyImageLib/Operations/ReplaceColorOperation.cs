using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Numerics;

namespace ChunkyImageLib.Operations;
internal class ReplaceColorOperation : IDrawOperation
{
    private readonly Color oldColor;
    private readonly Color newColor;

    private readonly ColorBounds oldColorBounds;
    private readonly ulong newColorBits;

    public bool IgnoreEmptyChunks => true;

    public ReplaceColorOperation(Color oldColor, Color newColor)
    {
        this.oldColor = oldColor;
        this.newColor = newColor;
        oldColorBounds = new ColorBounds(oldColor);
        newColorBits = newColor.ToULong();
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        ReplaceColor(oldColorBounds, newColorBits, targetChunk);
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
