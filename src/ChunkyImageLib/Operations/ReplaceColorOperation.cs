using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;
internal class ReplaceColorOperation : IDrawOperation
{
    private readonly SKColor oldColor;
    private readonly SKColor newColor;

    private readonly ColorBounds oldColorBounds;
    private readonly ulong newColorBits;

    public bool IgnoreEmptyChunks => true;

    public ReplaceColorOperation(SKColor oldColor, SKColor newColor)
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

        using SKPixmap pixmap = chunk.Surface.SkiaSurface.PeekPixels();
        IntPtr pixels = pixmap.GetPixels();

        Half* endOffset = (Half*)(pixels + pixmap.BytesSize);
        for (Half* i = (Half*)pixels; i < endOffset; i += 4)
        {
            if (oldColorBounds.IsWithinBounds(i))
                *(ulong*)i = newColorBits;
        }
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        return OperationHelper.FindChunksTouchingRectangle(new RectI(VecI.Zero, imageSize), ChunkyImage.FullChunkSize);
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        return new ReplaceColorOperation(oldColor, newColor);
    }

    public void Dispose() { }
}
