using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;

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

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        ReplaceColor(oldColorBounds, newColorBits, chunk);
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

    HashSet<VecI> IDrawOperation.FindAffectedChunks(VecI imageSize)
    {
        return OperationHelper.FindChunksTouchingRectangle(new RectI(VecI.Zero, imageSize), ChunkyImage.FullChunkSize);
    }

    public void Dispose()
    {
    }
}
